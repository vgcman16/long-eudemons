using Long.Game.Network.Login;
using Long.Kernel;
using Long.Kernel.Service;
using Long.Shared;
using Quartz;

namespace Long.Game.Threading
{
    [DisallowConcurrentExecution]
    public sealed class BasicThread : IJob
    {
        private static readonly ServerSettings serverSettings = new ServerSettings();
        private static DateTime startTime;

        private static long lastTick = Environment.TickCount64;
        private static TimeOut accountConnectTimeout = new TimeOut(3);

        public async Task Execute(IJobExecutionContext context)
        {
            Console.Title = $"{serverSettings.Game.Name} - {DateTime.Now:yyyy-MM-dd HH:mm:ss} - Start: {startTime:yyyy-MM-dd HH:mm:ss} - {Services.NetworkMonitor.UpdateStats((int)(Environment.TickCount64 - lastTick))}";

            if (LoginClientSocket.Instance == null && accountConnectTimeout.ToNextTime())
            {
                LoginClientSocket loginClientSocket = new LoginClientSocket();
                await loginClientSocket.ConnectToAsync(serverSettings.Login.IPAddress, serverSettings.Login.Port);
            }

            lastTick = Environment.TickCount64;
        }

        public static void SetStartTime()
        {
            if (startTime != default)
            {
                return;
            }
            startTime = DateTime.Now;
        }
    }
}
