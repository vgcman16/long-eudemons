using Long.Kernel.States.Items;
using Long.Network.Packets;

namespace Long.Kernel.Network.Game.Packets
{
    public sealed class MsgItemInfo : MsgBase<GameClientBase>
    {
        public MsgItemInfo() 
        { 
        }

        public MsgItemInfo(ItemBase item, ItemMode mode = ItemMode.Default)
        {
            if (mode == ItemMode.View)
            {
                Identity = item.PlayerIdentity;
            }
            else
            {
                Identity = item.Identity;
            }

            if (item.IsEudemon() || item.IsEudemonEgg())
            {
                Itemtype = item.Type;
                Mode = mode;
                Position = (byte)item.Position;
                Amount = (ushort)item.EudemonLife;
                AmountLimit = (ushort)item.EudemonMaxLife;
                Data = item.Data;
                LockType = item.Locked;
                ActivationTime = (uint)item.AvailableTime;
                GiftFlag = item.GiftFlag;
                EarthAttr = item.EarthAttr;
                FireAttr = item.FireAttr;
                WaterAttr = item.WaterAttr;
                AirAttr = item.AirAttr;
                SpecialAttr = item.SpecialAttr;
                Name = item.Name;
            }
            else
            {
                Itemtype = item.Type;
                Mode = mode;
                Position = (byte)item.Position;
                Ident = (byte)ItemBase.ITEM_STATUS_NOT_IDENT;
                if (!item.IsNeedIdent())
                {
                    Ident = item.Ident;
                    Amount = item.Amount;
                    AmountLimit = item.AmountLimit;
                    SocketOne = (byte)item.SocketOne;
                    SocketTwo = (byte)item.SocketTwo;
                    Magic1 = (byte)item.Magic1;
                    Magic2 = item.Magic2;
                    Magic3 = item.Plus;
                    Data = item.Data;
                    LockType = item.Locked;
                    WarGhostExp = item.WarGhostExp;
                    GiftFlag = item.GiftFlag;
                    ActivationTime = (uint)item.AvailableTime;
                    EarthAttr = item.EarthAttr;
                    FireAttr = item.FireAttr;
                    WaterAttr = item.WaterAttr;
                    AirAttr = item.AirAttr;
                    SpecialAttr = item.SpecialAttr;
                    Name = item.ForgenName;
                }
                else
                {
                    Itemtype = ItemBase.HideTypeUnident(item.Type);
                }
            }
        }

        public uint Itemtype { get; set; }
        public uint Identity { get; set; }
        public ushort Amount { get; set; }
        public ushort AmountLimit { get; set; }
        public ItemMode Mode { get; set; }
        public byte Ident { get; set; }
        public byte Position { get; set; }
        public byte SocketOne { get; set; }
        public byte SocketTwo { get; set; }
        public bool IsLocked { get; set; }
        public byte Magic1 { get; set; }
        public byte Magic2 { get; set; }
        public byte Magic3 { get; set; }
        public uint Data { get; set; }
        public uint LockType { get; set; }
        public uint WarGhostExp { get; set; }
        public uint GemAtkType { get; set; }
        public uint ActivationTime { get; set; }
        public byte EarthAttr { get; set; }
        public byte FireAttr { get; set; }
        public byte WaterAttr { get; set; }
        public byte AirAttr { get; set; }
        public byte SpecialAttr { get; set; }
        public ushort GiftFlag { get; set; }
        public string Name { get; set; }

        /// <summary>
        ///     Encodes the packet structure defined by this message class into a byte packet
        ///     that can be sent to the client. Invoked automatically by the client's send
        ///     method. Encodes using byte ordering rules interoperable with the game client.
        /// </summary>
        /// <returns>Returns a byte packet of the encoded packet.</returns>
        public override byte[] Encode()
        {
            using var writer = new PacketWriter();
            writer.Write((ushort)PacketType.MsgItemInfo);
            writer.Write(0);                // 4
            writer.Write(Identity);         // 8
            writer.Write(Itemtype);         // 12
            writer.Write(Amount);           // 16
            writer.Write(AmountLimit);      // 18
            writer.Write((byte)Mode);       // 20
            writer.Write(Ident);            // 21
            writer.Write(Position);         // 22
            writer.Write(SocketOne);        // 23
            writer.Write(SocketTwo);        // 24
            writer.Write(Magic1);           // 25
            writer.Write(Magic2);           // 26 
            writer.Write(Magic3);           // 27
            writer.Write(Data);             // 28
            writer.Write(LockType);         // 32
            writer.Write(WarGhostExp);      // 36
            writer.Write(GemAtkType);       // 40
            writer.Write(ActivationTime);   // 44
            writer.Write(EarthAttr);        // 48
            writer.Write(WaterAttr);        // 49
            writer.Write(FireAttr);         // 50
            writer.Write(AirAttr);          // 51
            writer.Write(SpecialAttr);      // 52
            writer.Write(GiftFlag);         // 54
            writer.Write(new List<string>() // 55
            {
                Name
            });

            return writer.ToArray();
        }

        public enum ItemMode : ushort
        {
            Default = 1,
            Trade = 2,
            Update = 3,
            View = 4,
            Active = 5,
            AddItemReturned = 8,
            Inbox = 11,
            Auction = 5, // this is according to the old source
            DiplsayInfo = 12,
        }
    }
}
