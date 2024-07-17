using Long.Kernel.Modules.Interfaces;

namespace Long.Kernel.Modules.Systems.Nobility
{
    public interface INobility : IInitializeSystem
    {
        NobilityRank Rank { get; }
        int Position { get; }
        ulong Donation { get; set; }
        string Name { get; }
        Task BroadcastAsync();
    }
}
