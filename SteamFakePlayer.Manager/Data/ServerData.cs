using System.Collections.Generic;
using ProtoBuf;

namespace SteamFakePlayer.Manager.Data
{
    [ProtoContract]
    public class ServerData
    {
        [ProtoMember(5)]
        public BotOptionsData BotOptions = new BotOptionsData();

        [ProtoMember(4)]
        public List<BotAccountData> Bots = new List<BotAccountData>();

        [ProtoMember(3)]
        public string DisplayName;

        [ProtoMember(1)]
        public string IP;

        [ProtoMember(2)]
        public int Port;
    }
}