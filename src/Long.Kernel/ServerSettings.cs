using Long.Database;
using Long.Network.Security;
using Long.Network.Sockets;
using Microsoft.Extensions.Configuration;

namespace Long.Kernel
{
    public sealed class ServerSettings
    {
        public ServerSettings()
        {
            new ConfigurationBuilder()
                .AddJsonFile("Config.Game.json")
                .AddEnvironmentVariables("Ai")
                .AddEnvironmentVariables("Game")
                .Build()
                .Bind(this);
        }

        public ServerSettings(params string[] args)
        {
            new ConfigurationBuilder()
                .AddJsonFile("Config.Game.json")
                .AddCommandLine(args)
                .AddEnvironmentVariables("Ai")
                .AddEnvironmentVariables("Game")
                .Build()
                .Bind(this);
        }

        public bool CooperatorMode { get; set; } = false;
        public AiServer Ai { get; set; }
        public GameServer Game { get; set; }
        public LoginClient Login { get; set; }
        public DatabaseConfiguration Database { get; set; }
        public string[] Modules { get; set; }

        public class AiServer
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string IPAddress { get; set; }
            public int Port { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public class GameServer
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string IPAddress { get; set; }
            public int Port { get; set; }
            public int MaxOnlinePlayers { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public DateTime? ReleaseDate { get; set; }
            public int Processors { get; set; } = 1;

            public ListenerSettings Listener { get; set; }
        }

        public class LoginClient
        {
            public string IPAddress { get; set; }
            public int Port { get; set; }
            public AesCipher.Settings Encryption { get; set; }
        }
    }
}
