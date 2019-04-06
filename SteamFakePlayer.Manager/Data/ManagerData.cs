using System.Collections.Generic;
using ProtoBuf;

namespace SteamFakePlayer.Manager.Data
{
    [ProtoContract]
    public class ManagerData
    {
        [ProtoMember(3)]
        public string JoinerFile;

        [ProtoMember(1)]
        public string LastKey;

        [ProtoMember(2)]
        public List<ServerData> Servers = new List<ServerData>();

        [ProtoMember(4)]
        public List<BotAccountData> Bots = new List<BotAccountData>();

        [ProtoMember(5)]
        public BotOptionsData BotOptions = new BotOptionsData();
    }
}