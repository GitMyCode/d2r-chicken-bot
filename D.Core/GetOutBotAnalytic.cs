using System;
using System.Collections.Generic;
using analytics_lib;
using System.Text.Json;

namespace D.Core {
    public static class EventTypes {
        public static string Session = "session";

        public static string Error = "error";

        public static string Info = "info";

        public static string Death = "death";

        public static string GotOut = "gotOut";

        public static string Start = "start";
    }

    public static class GetOutBotAnalytic {

        public static IDisposable StartSession() => new GetOutAnalytic();

        public static void SendError(string message) => new BotErrorEvent(message).Send();

        public static void SendInfo(string message, object data = null) => new InfoEvent(message, data).Send();
        public static void SendDeathEvent(object data) => new DeathEvent(data).Send();

        public static void SendStart() => new StartEvent().Send();

        public static void SendGotOutEvent(GameState data) => new GotOutEvent(data).Send();
    }

    public class BotSpanBase : AnalyticSpanBase {
        static readonly JsonSerializerOptions k_SerializeOptions = new() {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
        };

        public BotSpanBase(string name) : base(name) {
            AddData("ts", DateTime.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssK"));
            AddData("eventType", name);
        }

        public void SetMessage(string message) => AddData("message", message);

        public void SetAdditionalData(IDictionary<string, object> data) => AddData("data", JsonSerializer.Serialize(data, k_SerializeOptions));
    }

    public class BotEventBase : AnalyticEventBase {
        static readonly JsonSerializerOptions k_SerializeOptions = new() {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
        };

        public BotEventBase(string name) : base(name) {
            AddData("ts", DateTime.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssK"));
            AddData("eventType", name);
        }

        public void SetMessage(string message) => AddData("message", message);

        public void SetAdditionalData(object data) => AddData("data", JsonSerializer.Serialize(data, k_SerializeOptions));
    }

    public class StartEvent : BotEventBase {
        public StartEvent() : base(EventTypes.Start) {
        }
    }

    public class InfoEvent : BotEventBase {
        public InfoEvent(string message, object data) : base(EventTypes.Info) {
            SetMessage(message);
            if (data != null)
                SetAdditionalData(data);
        }
    }


    public class GetOutAnalytic : BotSpanBase {
        public GetOutAnalytic() : base(EventTypes.Session) {
        }
    }

    public class BotErrorEvent : BotEventBase {
        public BotErrorEvent(string message) : base(EventTypes.Error) {
            SetMessage(message);
        }
    }

    public class DeathEvent : BotEventBase {
        public DeathEvent(object data) : base(EventTypes.Death) {
            SetAdditionalData(data);
        }
    }

    public class GotOutEvent : BotEventBase {
        public GotOutEvent(GameState state) : base(EventTypes.GotOut) {
            SetAdditionalData(new Dictionary<string, object> {
                {"state", state },
                {"percentage", ((double)state.CurrentHealth/state.MaxHealth).ToString("0.##") }
            });
        }
    }
}
