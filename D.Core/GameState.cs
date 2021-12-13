using System;
using System.Text.Json.Serialization;
using D.Models;

namespace D.Core
{
        public record  GameState
        {
            public string PlayerName  { get; set; }
            public Area CurrentArea { get; set; }
            public bool IsInTown { get; set; }
            
            public bool IsGameMenuOpen { get; set; }
            
            public int? PreviousHealth { get; set; }

            public int CurrentHealth { get; set; }

            public int MaxHealth { get; set; }

            [JsonIgnore]
            public IntPtr WindowHandle { get; set; }
            

            [JsonIgnore]
            public InteropTcpHelper.MIB_TCPROW_OWNER_PID? GameSocket { get; set; }

            public bool IsInGame() => CurrentArea != Area.None;

            public bool IsDead() => IsInGame() && CurrentHealth <= 0;

        }
}
