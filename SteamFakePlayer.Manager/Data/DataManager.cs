using System.IO;
using System.Threading;
using ProtoBuf;

namespace SteamFakePlayer.Manager.Data
{
    public static class DataManager
    {
        private const string _file = "data.bin";
        private static ManagerData _data;
        private static bool _needSave;

        public static ManagerData Data
        {
            get
            {
                if (_data != null)
                {
                    return _data;
                }

                if (File.Exists(_file) == false)
                {
                    return _data = new ManagerData();
                }

                try
                {
                    using (var stream = File.OpenRead(_file))
                    {
                        return _data = Serializer.Deserialize<ManagerData>(stream);
                    }
                }
                catch (ProtoException)
                {
                    return _data = new ManagerData();
                }
            }
        }

        public static void Save()
        {
            _needSave = true;
        }

        private static void SaveInternal()
        {
            if (Data != null)
            {
                using (var stream = new FileStream(_file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
                {
                    Serializer.Serialize(stream, Data);
                }
            }
        }

        public static void RunSaver()
        {
            var saveThread = new Thread(o =>
            {
                for (;;)
                {
                    Thread.Sleep(100);
                    if (_needSave)
                    {
                        _needSave = false;
                        SaveInternal();
                    }
                }
            });

            saveThread.Name = "DataManager.SaveWorker";
            saveThread.IsBackground = true;
            saveThread.Start();
        }
    }
}