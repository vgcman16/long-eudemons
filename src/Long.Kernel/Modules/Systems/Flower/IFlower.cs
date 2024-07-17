using Long.Database.Entities;
using Long.Kernel.Modules.Interfaces;

namespace Long.Kernel.Modules.Systems.Flower
{
    public interface IFlower : IInitializeSystem
    {
        public const uint
            RedRose = 30000001,
            WhiteRose = 30000002;

        uint RedRoses { get; set; }
        uint WhiteRoses { get; set; }
        uint LastFlowerDate { get; set; }

        uint Charm { get; set; }
        uint FairyType { get; set; }
        bool IsSendGiftEnable();

        DbFlower FlowerToday { get; set; }

        Task SaveAsync();
    }
}
