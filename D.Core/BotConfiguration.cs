using Microsoft.Extensions.Configuration;

namespace D.Core
{
    public enum QuitMethod
    {
        None,
        Both,
        Socket,
        Menu
    }

    public class BotConfiguration
    {
        public BotConfiguration(IConfiguration config)
        {
            QuitOnHealthPercentage = (double) config.GetValue("QuitOnHealthPercentage", 35) / 100;
            QuitMethod = config.GetValue("QuitMethod", QuitMethod.Both);
            QuitOnMajorHit = config.GetValue("QuitOnMajorHit", true);
        }

        public double QuitOnHealthPercentage { get; set; }
        public QuitMethod QuitMethod { get; set; }
        public bool QuitOnMajorHit { get; set; }

        public bool QuitWithSocket => QuitMethod is QuitMethod.Both or QuitMethod.Socket;

        public bool QuitWithMenu => QuitMethod is QuitMethod.Both or QuitMethod.Menu;
    }
}