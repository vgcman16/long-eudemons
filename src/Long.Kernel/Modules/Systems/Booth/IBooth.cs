using Long.Kernel.Network.Game.Packets;
using Long.Kernel.States.Items;
using Long.Kernel.States.Npcs;
using Long.Kernel.States.User;

namespace Long.Kernel.Modules.Systems.Booth
{
    public interface IBooth
    {
        uint Identity { get; }
        uint Index { get; }
        ushort Type { get; }
        string SellerName { get; }
        string HawkMessage { get; set; }
        DynamicNpc OwnerNpc { get; }

        Task<bool> InitializeAsync();
        Task QueryItemsAsync(Character requester, bool advertise = false);
        bool AddItem(ItemBase item, uint value, MsgItem.Moneytype type);
        Task<bool> SellBoothItemAsync(uint idItem, Character target);
        bool HasItem(uint idItem);
        bool RemoveItem(uint idItem);
        bool ValidateItem(uint id);
        Task EnterMapAsync();
        Task LeaveMapAsync();
        Task SendSpawnToAsync(Character player);

        Task<bool> CheckBoothRentalAsync();

        int RemainingAdvertiseTime { get; }
        void SetAdvertise(int hours);
        bool CheckAdvertiseTimeOut();
    }
}
