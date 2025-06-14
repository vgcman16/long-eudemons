using Long.Game.Network.Game;
using Long.Game.Threading;
using Long.Kernel;
using Long.Kernel.Managers;
using Long.Kernel.Processors;
using Long.Kernel.Threads;
using Long.Shared.Threads;
using Long.World;
using Serilog;

namespace Long.Game
{
    public class Kernel
    {
        private static readonly ILogger logger = Log.ForContext<Kernel>();

        private static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private static SchedulerFactory schedulerFactory { get; set; }
        private static GameServerSocket gameServerSocket { get; set; }

        private static UserThread userThread { get; set; }
        private static RoleThread roleThread { get; set; }

        public static async Task<bool> InitializeAsync(ServerSettings serverSettings)
        {
            try
            {
                WorldProcessor.Create(cancellationTokenSource.Token);

                await MapDataManager.LoadDataAsync();
                await MapManager.InitializeAsync();
                await ItemManager.InitializeAsync();
                await BattleSystemManager.InitializeAsync();
                await MagicManager.InitializeAsync();
                await ExperienceManager.InitializeAsync();
                await RoleManager.InitializeAsync();
                await ScriptManager.InitializeAsync();
                await NpcManager.InitializeAsync();

                ModuleManager.Initialize();
                await ModuleManager.OnServerInitializeModulesAsync();

                gameServerSocket = new GameServerSocket(serverSettings);
                _ = gameServerSocket.StartAsync(serverSettings.Game.Port, serverSettings.Game.IPAddress);

                BasicThread.SetStartTime();

                userThread = new UserThread();
                await userThread.StartAsync();

                roleThread = new RoleThread();
                await roleThread.StartAsync();

                schedulerFactory = new SchedulerFactory();
                await schedulerFactory.StartAsync();
                await schedulerFactory.ScheduleAsync<BasicThread>("* * * * * ?");
                await schedulerFactory.ScheduleAsync<EventThread>("* * * * * ?");
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error on server initialization! {0}", ex.Message);
                return false;
            }
        }

        public static async Task CloseAsync()
        {
            await cancellationTokenSource.CancelAsync();
            for (int i = 5; i >= 0; i--)
            {
                logger.Information("Closing in {0} seconds...", i);
                await Task.Delay(1000);
            }
        }
    }
}
