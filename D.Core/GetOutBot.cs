using System;
using System.Threading;
using System.Threading.Tasks;
using D.Models;
using Microsoft.Extensions.Logging;
using Stateless;

namespace D.Core {
    public class GetOutBot {
        public enum State {
            LookingForPlayer,
            WatchingYou,
            InTown,
            GettingOut,
            IsDead
        }

        public enum Trigger {
            Watch,
            Quited,
            SearchPlayer,
            GetOut,
            IsSafeInTown,
            Death
        }

        private readonly CancellationTokenSource m_BotTaskSrc = new();

        private readonly BotConfiguration m_Config;

        private readonly ILogger m_Logger;

        private readonly MemoryReader m_MemoryReader;

        public readonly StateMachine<State, Trigger> m_stateMachine;
        private readonly IStateUiWriter m_StateUiWriter;

        private readonly CancellationToken m_token;
        private StateMachine<State, Trigger>.TriggerWithParameters<GameState> _setDeathTrigger;

        private StateMachine<State, Trigger>.TriggerWithParameters<GameState> _setGetOutTrigger;

        public GetOutBot(IStateUiWriter stateUiWriter, BotConfiguration config, MemoryReader memoryReader,
            ILogger<GetOutBot> mLogger, CancellationToken mToken = default) {
            m_Config = config;
            m_Logger = mLogger;
            m_MemoryReader = memoryReader;
            m_StateUiWriter = stateUiWriter;
            m_stateMachine = new StateMachine<State, Trigger>(State.LookingForPlayer);
            m_token = mToken;
            m_token = m_BotTaskSrc.Token;

            ConfigureStateMachine();
        }

        public void ConfigureStateMachine() {
            m_StateUiWriter.WriteState($"{State.LookingForPlayer}");
            _setGetOutTrigger = m_stateMachine.SetTriggerParameters<GameState>(Trigger.GetOut);
            _setDeathTrigger = m_stateMachine.SetTriggerParameters<GameState>(Trigger.Death);
            m_stateMachine.OnTransitioned(transition => {
                // if change in state
                if (transition.Source != transition.Destination) {
                    m_StateUiWriter.WriteState($"{transition.Destination}");
                    m_Logger.LogInformation($"State transition: {transition.Destination}");
                    if (transition.Source == State.LookingForPlayer)
                        m_StateUiWriter.WriteAdditionalData("");
                    else if (transition.Destination == State.LookingForPlayer && transition.Source != State.GettingOut)
                        m_StateUiWriter.WriteAdditionalData("");
                }
            });

            m_stateMachine.Configure(State.LookingForPlayer)
                .PermitReentry(Trigger.SearchPlayer)
                .OnEntryFromAsync(Trigger.SearchPlayer, async () => await Task.Delay(1000))
                .Permit(Trigger.Death, State.IsDead)
                .Permit(Trigger.IsSafeInTown, State.InTown)
                .Permit(Trigger.GetOut, State.GettingOut)
                .Permit(Trigger.Watch, State.WatchingYou)
                .OnEntryAsync(async () => await OnWatching());

            m_stateMachine.Configure(State.WatchingYou)
                .PermitReentry(Trigger.Watch)
                .Permit(Trigger.Death, State.IsDead)
                .Permit(Trigger.IsSafeInTown, State.InTown)
                .OnEntryFromAsync(Trigger.IsSafeInTown, async () => await Task.Delay(500))
                .Permit(Trigger.GetOut, State.GettingOut)
                .Permit(Trigger.SearchPlayer, State.LookingForPlayer)
                .OnEntryAsync(async () => await OnWatching());

            m_stateMachine.Configure(State.InTown)
                .SubstateOf(State.WatchingYou)
                .PermitReentry(Trigger.IsSafeInTown)
                .OnEntryFromAsync(Trigger.IsSafeInTown, async () => await Task.Delay(500))
                .Permit(Trigger.Watch, State.WatchingYou);

            m_stateMachine.Configure(State.GettingOut)
                .Permit(Trigger.Death, State.IsDead)
                .Permit(Trigger.Watch, State.WatchingYou)
                .Permit(Trigger.IsSafeInTown, State.InTown)
                .OnEntryFromAsync(_setGetOutTrigger, async state => await OnGetOut(state))
                .Permit(Trigger.Quited, State.LookingForPlayer);

            m_stateMachine.Configure(State.IsDead)
                .OnEntryFromAsync(_setDeathTrigger, async state => await OnDeath(state))
                .Permit(Trigger.Watch, State.WatchingYou);

            // just ignore trigger in bad state
            m_stateMachine.OnUnhandledTrigger((s, t) => { m_Logger.LogWarning($"Invalid transition: {s} {t}"); });
        }

        public async Task Run() {
            m_Logger.LogInformation("run bot");
            await Task.Run(async () => {
                do {
                    await OnWatching();
                    await Task.Delay(10);
                } while (true);
            });
        }

        public async Task OnWatching() {
            try {
                if (m_token.IsCancellationRequested)
                    return;

                var state = GetState();

                if (state == null) {
                    await m_stateMachine.FireAsync(Trigger.SearchPlayer);
                    return;
                }

                m_StateUiWriter.WriteAdditionalData($"guarding: {state.PlayerName}");
                if (state.IsInTown && state.CurrentArea != Area.None) {
                    await m_stateMachine.FireAsync(Trigger.IsSafeInTown);
                    return;
                }

                if (state.IsDead()) {
                    await m_stateMachine.FireAsync(_setDeathTrigger, state);
                    return;
                }

                if (HasToGetOut(state)) {
                    await m_stateMachine.FireAsync(_setGetOutTrigger, state);
                    return;
                }

                await Task.Delay(1);
                await m_stateMachine.FireAsync(Trigger.Watch);
            } catch (Exception e) {
                GetOutBotAnalytic.SendError(e.Message);
                m_Logger.LogError("Exception in OnWatching {@e}", e);
                throw;
            }
        }

        private async Task OnGetOut(GameState receivedState) {
            try {
                m_StateUiWriter.WriteAdditionalData("Try to get out!");
                var currentHealth = receivedState.CurrentHealth;

                var successfulOut = await TryToGetOut(receivedState);
                var state = GetState();
                if (!successfulOut && state != null && state.IsDead()) {
                    await m_stateMachine.FireAsync(_setDeathTrigger, state);
                } else if (!successfulOut) {
                    m_Logger.LogCritical("Couldn't get out!");
                    m_Logger.LogWarning("Couldn't get out :(  {@state}", state);
                    m_StateUiWriter.WriteAdditionalData("Couldn't get out :(");
                    GetOutBotAnalytic.SendInfo("couldn't get out", receivedState);
                    await Task.Delay(2000);
                    await m_stateMachine.FireAsync(Trigger.Watch);
                } else // Success!
                  {
                    m_Logger.LogInformation("Got out! With {currentHealth} hp", currentHealth);
                    m_StateUiWriter.WriteAdditionalData($"Got out! With {currentHealth} hp");
                    GetOutBotAnalytic.SendGotOutEvent(receivedState);
                    await Task.Delay(6000);
                    await m_stateMachine.FireAsync(Trigger.Quited);
                }
            } catch (Exception e) {
                GetOutBotAnalytic.SendError(e.Message);
                m_Logger.LogError("Exception in OnGetOut {@e}", e);
                throw;
            }
        }

        private async Task OnDeath(GameState state) {
            try {
                m_Logger.LogWarning("Death  {@state}", state);
                m_StateUiWriter.WriteAdditionalData("woups..");
                GetOutBotAnalytic.SendDeathEvent(state);
                // stay in that state until not death (so in menu or other char)
                SpinWait.SpinUntil(() => {
                    Thread.Sleep(500);
                    return !GetState()?.IsDead() ?? true;
                }, -1);
                m_StateUiWriter.Clear();
                await m_stateMachine.FireAsync(Trigger.Watch);
            } catch (Exception e) {
                GetOutBotAnalytic.SendError(e.Message);
                m_Logger.LogError("exception in OnDeath {@e}", e);
                throw;
            }
        }

        private bool HasToGetOut(GameState? state) {
            if (state == null || state.IsDead() || state.IsInTown)
                return false;

            if (m_Config.QuitOnMajorHit && state.PreviousHealth.HasValue) {
                var previousHealth = state.PreviousHealth.Value;
                var previousHit = previousHealth - state.CurrentHealth;
                var wasMajorHit = previousHit >= state.CurrentHealth;
                if (wasMajorHit) {
                    m_Logger.LogInformation(
                        "previous hit was {previousHit} and life remaining {currentHealth} so get out", previousHit,
                        state.CurrentHealth);
                    return true;
                }
            }

            return (double)state.CurrentHealth / state.MaxHealth <= m_Config.QuitOnHealthPercentage;
        }

        private GameState? GetState() {
            var state = m_MemoryReader.GetState();
            return state;
        }

        private async Task<bool> TryToGetOut(GameState? state) {
            var tokenSrc = new CancellationTokenSource();
            var token = tokenSrc.Token;
            m_Logger.LogInformation("Quit method: {QuitMethod}", m_Config.QuitMethod);

            var outFromSocketTsk = m_Config.QuitWithSocket
                ? Task.Run(() => TryToGetOutSocket(state))
                : Task.FromResult(false);
            var outFromMenuTsk = m_Config.QuitWithMenu
                ? Task.Run(() => TryToGetOutWithGameMenu(state, token))
                : Task.FromResult(false);

            Task.WaitAny(outFromSocketTsk, outFromMenuTsk);
            // cancel only if successfully killed socket
            if (outFromSocketTsk.IsCompleted && outFromSocketTsk.Result)
                tokenSrc.Cancel();

            if (outFromMenuTsk.Result)
                m_Logger.LogInformation("out from menu");
            if (outFromSocketTsk.Result)
                m_Logger.LogInformation("out from socket");

            return outFromSocketTsk.Result || outFromMenuTsk.Result;
        }

        private async Task<bool> TryToGetOutWithGameMenu(GameState receivedState, CancellationToken token) {
            var state = receivedState;
            m_Logger.LogInformation("start get out from menu task");
            try {
                // try some time
                var delayInMilliseconds = 10;
                var tryCountMax = 2500 / delayInMilliseconds;
                var tryCount = 0;
                var hasToGetOut = true;
                do {
                    // we at least try 4 time before cancelling. since maybe the socket kill wasn't the right one lol
                    if (token.IsCancellationRequested && tryCount > 4) {
                        m_Logger.LogInformation("get out from menu was cancelled");
                        return false;
                    }

                    WindowsHelper.SetForegroundWindow(state.WindowHandle);
                    if (!state.IsGameMenuOpen) WindowsHelper.SendEscapeKey(state.WindowHandle);

                    var windowRect = WindowsHelper.GetWindowRect(state.WindowHandle);

                    var width = windowRect.Right - windowRect.Left;
                    var height = windowRect.Bottom - windowRect.Top;
                    WindowsHelper.LeftMouseClick(windowRect.Left + width / 2, windowRect.Top + height / 20 * 9);
                    m_Logger.LogInformation($"Tried to get out from menu. tryCount: {tryCount}");
                    await Task.Delay(10);
                    tryCount++;
                    state = GetState();
                    hasToGetOut = HasToGetOut(state);
                } while (hasToGetOut && tryCount <= tryCountMax);

                return !hasToGetOut;
            } catch (TaskCanceledException) {
                m_Logger.LogInformation("get out from menu was cancelled");
                return false;
            }
        }

        private async Task<bool> TryToGetOutSocket(GameState? state) {
            m_Logger.LogInformation("start get out from socket task");
            if (state?.GameSocket == null)
                return false;

            var result = InteropTcpHelper.DeleteSocket(state.GameSocket.Value);
            if (result != 0) {
                GetOutBotAnalytic.SendError($"couldn't kill socket {result}");
                m_Logger.LogError("couldn't kill socket {@result}", result);
                return false;
            }

            m_Logger.LogInformation("successful kill socket");
            return true;
        }
    }
}