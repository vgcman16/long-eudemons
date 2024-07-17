using Long.Kernel.Modules.Systems.Syndicate;
using Long.Kernel.States.Items;
using Long.Kernel.States.User;

namespace Long.Kernel.Modules.Systems.Totem
{
    public interface ITotem
    {
        int SharedBattlePower { get; }
        DateTime? LastOpenTotem { get; set; }

        bool HasTotemHeadgear { get; set; }
        bool HasTotemNecklace { get; set; }
        bool HasTotemRing { get; set; }
        bool HasTotemWeapon { get; set; }
        bool HasTotemArmor { get; set; }
        bool HasTotemBoots { get; set; }
        bool HasTotemFan { get; set; }
        bool HasTotemTower { get; set; }
        int Level { get; }

        Task CreateAsync();
        Task InitializeAsync();

        Task<bool> OpenTotemPoleAsync(TotemPoleType type);
        Task<bool> InscribeItemAsync(Character user, ItemBase item);
        Task<bool> UnsubscribeItemAsync(uint idItem, uint idUser, bool synchro = true, bool isSystem = false);
        Task UnsubscribeAllAsync(uint idUser, bool isSystem = false);
        Task RefreshMemberArsenalDonationAsync(ISyndicateMember member);
        Task<bool> EnhanceTotemPoleAsync(TotemPoleType type, byte power);
        Task SendTotemPolesAsync(Character user);
        Task SendTotemsAsync(Character user, TotemPoleType type, int index);
        int UnlockTotemPolePrice();

        int TotemLimit(int level, int metempsychosis)
        {
            if (metempsychosis == 0)
            {
                if (level < 100)
                {
                    return 7;
                }

                return 14;
            }

            if (metempsychosis == 1)
            {
                return 21;
            }

            return 30;
        }

        static TotemPoleType GetTotemPoleType(uint type)
        {
            if (ItemBase.IsHelmet(type))
            {
                return TotemPoleType.Headgear;
            }

            if (ItemBase.IsArmor(type) && type / 1000 != 137)
            {
                return TotemPoleType.Armor;
            }

            if (ItemBase.IsWeapon(type))
            {
                return TotemPoleType.Weapon;
            }

            if (ItemBase.IsRing(type))
            {
                return TotemPoleType.Ring;
            }

            if (ItemBase.IsShoes(type))
            {
                return TotemPoleType.Boots;
            }

            if (ItemBase.IsNeck(type))
            {
                return TotemPoleType.Necklace;
            }

            return TotemPoleType.None;
        }

        [Flags]
        public enum TotemPoleFlag
        {
            Headgear = 0x1,
            Armor = 0x2,
            Weapon = 0x4,
            Ring = 0x8,
            Boots = 0x10,
            Necklace = 0x20,
            Fan = 0x40,
            Tower = 0x80,
            None = 0x100
        }

        public enum TotemPoleType
        {
            Headgear,
            Armor,
            Weapon,
            Ring,
            Boots,
            Necklace,
            Fan,
            Tower,
            None
        }

    }
}
