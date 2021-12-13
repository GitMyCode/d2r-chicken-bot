using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using analytics_lib;
using D.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Yaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace D.WpfApp
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private static readonly string machineId = MachineStamp.CreateMachineId();
        
        private readonly TaskScheduler _uiScheduler;
        private readonly AnalyticService m_AnalyticService;

        private readonly IServiceProvider m_ServiceProvider;
        private Thread botThread;
        private CancellationTokenSource cancellationTokenSource;
        private readonly ILogger logger;
        private IDisposable sessionSpan;

        public MainWindow()
        {
            _uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            InitializeComponent();
            var v = MachineStamp.Version;

            version.Text = $"V{v}";
        
            var builder = new ConfigurationBuilder()
#if !DEBUG
 .SetBasePath(System.IO.Path.GetDirectoryName(Environment.ProcessPath))
#endif
                .AddYamlFile("config.yml", false, false);
            var conf = builder.Build();

            var services = new ServiceCollection()
                .AddSingleton<IConfiguration>(conf)
                .AddSingleton<BotConfiguration>()
                .AddAnalytics(
                    (x, p) =>
                        x.WithSourceName("diablo get out bot")
                            .WithCommonHeader(new Dictionary<string, string>
                            {
                                {"version", v},
                                {"machineId", machineId}
                            }))
                .AddLogging(x =>
                {
                    x.ClearProviders();
                    x.AddSerilog(new LoggerConfiguration()
                        .MinimumLevel.Information()
                        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
                        .Enrich.FromLogContext()
                        // "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
                        .WriteTo.File("bot.log", rollOnFileSizeLimit: true,
                            outputTemplate:
                            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}][{SourceContext}] {Message}{NewLine}{Exception}")
                        .CreateLogger());
                    x.AddFilter("Microsoft", LogLevel.Warning);
                });

            m_ServiceProvider = services.BuildServiceProvider();
            logger = m_ServiceProvider.GetService<ILoggerFactory>().CreateLogger<MainWindow>();
            m_AnalyticService = m_ServiceProvider.InitAnalytics();

            logger.LogInformation("Initialize, machineId: {@machineId}", machineId);

            if (!ApplicationHelper.ReserveMutex()) return;
            logger.LogWarning("App already launched. Exiting!");
            Close();
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            sessionSpan = GetOutBotAnalytic.StartSession();
            GetOutBotAnalytic.SendStart();
            StartBot();
        }

        private void WindowClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            sessionSpan?.Dispose();
            m_AnalyticService.ForceFlush().GetAwaiter().GetResult();
            ApplicationHelper.FreeMutex();
            e.Cancel = false;
        }

        public void StartBot()
        {
            cancellationTokenSource = new CancellationTokenSource();

            botThread = new Thread(() => Run(cancellationTokenSource.Token));
            botThread.Start();
        }

        public async Task Run(CancellationToken token = default)
        {
            await EnsureWindowVisible();
            var uiWriter = new StateUiWriter(statusBox, additionalData, _uiScheduler);
            try
            {
                var bot = new GetOutBot(uiWriter, m_ServiceProvider.GetService<BotConfiguration>(),
                    new MemoryReader(m_ServiceProvider.GetService<ILoggerFactory>().CreateLogger<MemoryReader>()),
                    m_ServiceProvider.GetService<ILoggerFactory>().CreateLogger<GetOutBot>(),
                    cancellationTokenSource.Token);
                await bot.Run();
            }
            catch (ThreadInterruptedException)
            {
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Bot stopped");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                logger.LogError("Error {@exception}", exception);
                uiWriter.WriteState("Error:");
                uiWriter.WriteAdditionalData(exception.Message);
                GetOutBotAnalytic.SendError(exception.Message);
                throw;
            }
        }

        private async Task EnsureWindowVisible()
        {
            await Task.Factory.StartNew(async () =>
            {
                await Task.Delay(8000);
                WindowState = WindowState.Normal;
                Activate();
            }, CancellationToken.None, TaskCreationOptions.None, _uiScheduler);
        }
    }
}