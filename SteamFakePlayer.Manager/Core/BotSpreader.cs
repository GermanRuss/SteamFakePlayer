﻿using SteamFakePlayer.Manager.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamFakePlayer.Manager.Core
{
    internal class BotSpreader
    {
        private static BotSpreader _instance;
        public static BotSpreader Instance => _instance ?? (_instance = new BotSpreader());

        private BotPool _pool = new BotPool();

        public bool IsRunning { get; private set; }

        private List<ServerCore> _servers = new List<ServerCore>();

        private Timeout SpreaderTimer;

        public void Run()
        {
            if (IsRunning == false)
            {
                if (SpreaderTimer == null)
                {
                    SpreaderTimer = new Timeout(1000, SpreaderTick);
                    SpreaderTimer.AutoReset = true;
                }

                IsRunning = true;
            }
        }

        public void Stop()
        {
            if (IsRunning)
            {
                foreach (var bot in _pool.BotsTaked)
                {
                    new Timeout(bot.GetServer().RemovePlayer(bot), () => {
                        _pool.Push(bot);
                    });
                }

                _pool.BotsTaked.Clear();

                IsRunning = false;
            }
        }

        public void OnDataChanged()
        {
            if (_servers.Count != DataManager.Data.Servers.Count)
            {
                DataManager.Data.Servers.ForEach(serverData =>
                {
                    if (GetServerByData(serverData) == null)
                    {
                        AddServer(new ServerCore(serverData));
                    }
                });

                _servers.Where(server => DataManager.Data.Servers.Contains(server.Data) == false)
                    .ToList().ForEach(RemoveServer);
            }
            _pool.Setup();
        }

        private void SpreaderTick()
        {
            if (IsRunning == false)
            {
                return;
            }

            if (_servers.All(server=>server.IsFull))
            {
                return;
            }

            foreach (var server in _servers)
            {
                while (_pool.Get(out var bot))
                {
                    if (server.IsFull == false)
                    {
                        server.AddPlayer(bot);
                        break;
                    }
                }
            }
        }

        public void PushPlayer(BotPlayer player)
        {
            player.GetServer().RemovePlayer(player);
            _pool.Push(player);
        }

        public ServerCore GetServerByData(ServerData data)
        {
            return _servers.FirstOrDefault(server => server.Data == data);
        }

        public void AddServer(ServerCore server)
        {
            _servers.Add(server);
        }

        public void RemoveServer(ServerCore server)
        {
            server.Players.ToList().ForEach(player =>
            {
                new Timeout(server.RemovePlayer(player), () =>
                {
                    _pool.Push(player);
                });
            });
            _servers.Remove(server);
        }

        public void RemoveServerByData(ServerData serverData)
        {
            RemoveServer(GetServerByData(serverData));
        }
    }
}
