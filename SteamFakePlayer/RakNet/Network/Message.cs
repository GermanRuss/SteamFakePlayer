using Network;

namespace RakNet.Network
{
    public class Message
    {
        public enum Type : byte
        {
            Welcome = 1,
            Auth = 2,
            Approved = 3,
            Ready = 4,
            Entities = 5,
            EntityDestroy = 6,
            GroupChange = 7,
            GroupDestroy = 8,
            RPCMessage = 9,
            EntityPosition = 10,
            ConsoleMessage = 11,
            ConsoleCommand = 12,
            Effect = 13,
            DisconnectReason = 14,
            Tick = 15,
            Message = 16,
            RequestUserInformation = 17,
            GiveUserInformation = 18,
            GroupEnter = 19,
            GroupLeave = 20,
            VoiceData = 21,
            EAC = 22,
            Last = 22
        }

        public Connection connection;

        public NetworkPeer peer;
        public Type type;

        public Read read => peer.read;

        public Write write => peer.write;

        public virtual void Clear()
        {
            connection = null;
            peer = null;
            type = 0;
        }
    }
}