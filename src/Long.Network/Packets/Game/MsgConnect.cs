using Org.BouncyCastle.Utilities.Encoders;

namespace Long.Network.Packets.Game
{
    /// <remarks>Packet Type 1052</remarks>
    /// <summary>
    ///     Message containing a connection request to the game server. Contains the player's
    ///     access token from the Account server, and the patch and language versions of the
    ///     game client.
    /// </summary>
    public abstract class MsgConnect<T> : MsgBase<T>
    {
        // Packet Properties
        public uint AccountId { get; set; }
        public uint Data { get; set; }
        public string Message { get; set; }

        /// <summary>
        ///     Decodes a byte packet into the packet structure defined by this message class.
        ///     Should be invoked to structure data from the client for processing. Decoding
        ///     follows TQ Digital's byte ordering rules for an all-binary protocol.
        /// </summary>
        /// <param name="bytes">Bytes from the packet processor or client socket</param>
        public override void Decode(byte[] bytes)
        {
            using var reader = new PacketReader(bytes);
            Length = reader.ReadUInt16();
            Type = (PacketType)reader.ReadUInt16();
            AccountId = reader.ReadUInt32(); // 4
            Data = reader.ReadUInt32();//8
            reader.BaseStream.Seek(21, SeekOrigin.Begin);
            Message = reader.ReadString(16);
        }
    }
}