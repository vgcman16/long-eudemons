using Long.Network.Sockets;
using ProtoBuf;

namespace Long.Network.Packets.Login
{
    public abstract class MsgLoginRealmAuthEx<TActor>
        : MsgProtoBufBase<TActor, MsgLoginRealmAuthEx<TActor>.RealmAuthDataEx> where TActor : TcpServerActor
    {
        public MsgLoginRealmAuthEx()
            : base(PacketType.MsgLoginRealmAuthEx)
        {
            serializeWithHeaders = true;
        }

        [ProtoContract]
        public struct RealmAuthDataEx
        {
            [ProtoMember(1)]
            public int Response { get; set; }
            public readonly ResponseCode Code => (ResponseCode)Response;
        }

        public enum ResponseCode
        {
            Success,
            InvalidPassword,
            InvalidRealm,
            InvalidAddress,
            DebugOnlyRealm,
            AlreadyConnected,
            AlreadyConnected2
        }
    }
}