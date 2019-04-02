﻿using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using SteamFakePlayer.Manager.Data;

namespace SteamFakePlayer.Manager.Core
{
    internal enum ConnectionState
    {
        ConnectingToSteam,
        LaunchingRust,
        Joining,
        Playing,
        Disconnected
    }

    internal delegate void StateChanged(ConnectionState state);

    internal abstract class PlayerJoiner
    {
        private readonly BotAccountData _account;
        private readonly ServerData _server;
        private Process _joinerProcess;

        private bool _running;

        private ConnectionState state = ConnectionState.Disconnected;

        private Timeout _connectingTask, _disconnectingTask;

        public PlayerJoiner(BotAccountData account, ServerData server)
        {
            _account = account;
            _server = server;
        }

        internal ConnectionState State
        {
            get => state;
            set
            {
                if (state != value)
                {
                    state = value;
                    OnStateChanged(state);
                }
            }
        }

        public BotAccountData Account => _account;

        internal event StateChanged StateChanged;

        protected virtual void OnStateChanged(ConnectionState state)
        {
            if (StateChanged != null)
            {
                StateChanged(state);
            }
        }        

        private void RunJoiner()
        {
            ProcessStartInfo processInfo;

            var args = GenerateJoinerArguments();

            processInfo = new ProcessStartInfo(DataManager.Data.JoinerFile, args)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                StandardErrorEncoding = Encoding.UTF8,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                WorkingDirectory = Path.GetDirectoryName(DataManager.Data.JoinerFile)
            };

            _joinerProcess = Process.Start(processInfo);
            _joinerProcess.OutputDataReceived += Joiner_OutputDataReceived;
            _joinerProcess.ErrorDataReceived += Joiner_ErrorDataReceived;
            _joinerProcess.BeginOutputReadLine();
            _joinerProcess.BeginErrorReadLine();
            _joinerProcess.WaitForExit();

            OnDisconnectedFromServer("Quiting...");


            _running = false;
        }

        private void Joiner_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }

        internal void ConnectWithSettingsDelay()
        {
            var delay = 1000 * Rand.Int32(_server.BotOptions.EnterMin, _server.BotOptions.EnterMax);
            ConnectWithDelay(delay);
        }

        internal void ConnectWithDelay(int delay)
        {
            ReplaceTask(ref _connectingTask, new Timeout(delay, Connect));
        }

        private void Connect()
        {
            if (_running)
            {
                new Timeout(100, Connect);
                return;
            }

            _running = true;
            Task.Run(() => RunJoiner());
        }

        internal void DisconnectWithSettingsDelay()
        {
            var delay = 1000 * Rand.Int32(_server.BotOptions.ExitMin, _server.BotOptions.ExitMax);
            DisconnectWithDelay(delay);
        }

        internal void DisconnectWithDelay(int delay)
        {
            ReplaceTask(ref _disconnectingTask, new Timeout(delay, DisconnectWithBlocking));
        }

        private void DisconnectWithBlocking()
        {
            if (State != ConnectionState.Disconnected)
            {
                KillJoiner();
            }
        }

        private void KillJoiner()
        {
            _joinerProcess.Kill();
        }

        private void ReplaceTask(ref Timeout currentTimeout, Timeout newTimeout)
        {
            if (currentTimeout != null)
            {
                currentTimeout.Stop();
                currentTimeout.Dispose();
                currentTimeout = null;
            }

            currentTimeout = newTimeout;
        }        

        protected virtual void OnConnectedToServer(string servername)
        {
            State = ConnectionState.Playing;
            Console.WriteLine($"'{_account.Username}' connected to '{_server.DisplayName}'");
        }

        protected virtual void OnDisconnectedFromServer(string reason)
        {
            State = ConnectionState.Disconnected;
            Console.WriteLine($"'{_account.Username}' disconnected from '{_server.DisplayName}' reason: {reason}");
        }

        private void Joiner_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
            {
                return;
            }

            if (e.Data.Contains("Подключились к: "))
            {
                var servername = e.Data.Split(new[] {"Подключились к: "}, StringSplitOptions.RemoveEmptyEntries)[1];
                OnConnectedToServer(servername);
            }
            else if (e.Data.Contains("Соеденение с игровым сервером разорвано: Connection Attempt Failed"))
            {
                OnDisconnectedFromServer("Connection Attempt Failed");
            }
            else if (e.Data.Contains("От игрового сервера получена причина дисконнекта: Server Restarting"))
            {
                OnDisconnectedFromServer("Server Restarting");
            }

            Console.WriteLine($"[{Account.Username}] {e.Data}");
        }
        }

        protected virtual string GenerateJoinerArguments()
        {
            return $"\"{_account.Username}\" " +
                   $"\"{_account.Password}\" " +
                   $"\"{_server.IP}\" " +
                   $"\"{_server.Port}\" \" \" " +
                   $"\"-pid\" \"{Process.GetCurrentProcess().Id}\" "
#if !DEBUG
                   + $"\"-hide\" "
#endif
                ;
        }
    }
}