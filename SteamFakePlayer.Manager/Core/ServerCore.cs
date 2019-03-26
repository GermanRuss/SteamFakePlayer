﻿using System;
using System.Collections.Generic;
using SteamFakePlayer.Manager.Data;

namespace SteamFakePlayer.Manager.Core
{
    public class ServerStats
    {
        public int ActiveBotsCount;
    }

    public delegate void ServerStatsChanged(ServerStats stats);

    public class ServerCore
    {
        private readonly List<BotPlayer> _players = new List<BotPlayer>();
        private readonly ServerData _serverData;
        private readonly ServerStats _serverStats;

        public ServerCore(ServerData serverData)
        {
            _serverData = serverData;
            _serverStats = new ServerStats();

            SetupPlayers();
        }

        public event ServerStatsChanged StatsChanged;

        public void SetupPlayers()
        {
            foreach (var account in _serverData.Bots)
            {
                var bot = new BotPlayer(account, _serverData);
                bot.StateChanged += OnBot_StateChanged;
                _players.Add(bot);
            }
        }

        private void OnBot_StateChanged(ConnectionState state)
        {
            if (state == ConnectionState.Disconnected)
            {
                _serverStats.ActiveBotsCount--;
                StatsChanged?.Invoke(_serverStats);
            }
            else if (state == ConnectionState.Playing)
            {
                _serverStats.ActiveBotsCount++;
                StatsChanged?.Invoke(_serverStats);
            }
        }

        internal void ForEach(Action<BotPlayer> action) => _players.ForEach(action);

        internal void ConnectBots()
        {
            ForEach(bot => { bot.Join(); });
        }

        internal void DisconnectBots()
        {
            ForEach(bot => { bot.Disconnect(); });
        }
    }
}