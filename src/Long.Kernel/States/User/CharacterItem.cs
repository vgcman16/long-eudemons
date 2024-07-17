using Long.Database.Entities;
using Long.Kernel.Database;
using Long.Kernel.Managers;
using Long.Kernel.Modules.Systems.Booth;
using Long.Kernel.Modules.Systems.Trade;
using Long.Kernel.Network.Game.Packets;
using Long.Kernel.States.Items;
using Long.Shared.Helpers;
using Long.Shared.Managers;
using System.Drawing;
using static Long.Kernel.Network.Game.Packets.MsgAction;

namespace Long.Kernel.States.User
{
    public partial class Character
    {
        public UserPackage UserPackage { get; }

        public Package Packages { get; }

        public ItemBase GetEquipment(ItemBase.ItemPosition itemPosition) => UserPackage?.GetEquipment(itemPosition);

        public ItemBase Helmet => GetEquipment(ItemBase.ItemPosition.Helmet);

        public ItemBase Necklace => GetEquipment(ItemBase.ItemPosition.Necklace);

        public ItemBase Ring => GetEquipment(ItemBase.ItemPosition.Ring);

        public ItemBase Weapon => GetEquipment(ItemBase.ItemPosition.Weapon);

        public ItemBase Armor => GetEquipment(ItemBase.ItemPosition.Armor);

        public ItemBase Shoes => GetEquipment(ItemBase.ItemPosition.Shoes);

        public ItemBase Sprite => GetEquipment(ItemBase.ItemPosition.Sprite);

        #region Trade

        public ITrade Trade { get; set; }
        public IBooth Booth { get; set; }

        #endregion
    }
}
