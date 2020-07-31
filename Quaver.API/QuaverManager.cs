using Quaver.API.Configuration;
using Quaver.API.Maps;
using Quaver.API.Memory;
using Quaver.API.Memory.Objects;
using Quaver.API.Memory.Processes;
using SimpleDependencyInjection;
using System;
using System.Data.SQLite;
using System.IO;

namespace Quaver.API
{
    public class QuaverManager
    {
        public QuaverProcess Process { get; private set; }

        public QuaverBase QuaverBase { get; private set; }

        public QuaverConfigManager ConfigManager { get; private set; }

        public Qua CurrentMap
        {
            get
            {
                if (!QuaverBase.GameplayScreen.IsLoaded)
                    return null;

                using (var connection = new SQLiteConnection($@"Data Source={QuaverDirectory}\quaver.db;Version=3;"))
                {
                    connection.Open();

                    string checksum = QuaverBase.GameplayScreen.CurrentMapChecksum;

                    string commandText = $"SELECT * FROM Map WHERE md5checksum = '{checksum}'";

                    using (SQLiteCommand sqliteCommand = new SQLiteCommand(commandText, connection))
                    {
                        using (SQLiteDataReader reader = sqliteCommand.ExecuteReader())
                        {
                            reader.Read();
                            string directory = reader.GetString(2);
                            string path = reader.GetString(3);

                            return Qua.Parse($@"{ConfigManager.SongsDirectory}\{directory}\{path}");
                        }
                    }
                }
            }
        }

        public string QuaverDirectory { get; private set; }

        public bool Initialize()
        {
            Console.WriteLine("Initializing...");

            var processes = System.Diagnostics.Process.GetProcessesByName("Quaver");
            if (processes.Length == 0)
                Environment.Exit(0);

            Process = new QuaverProcess(processes[0]);
            Process.Process.EnableRaisingEvents = true;
            Process.Process.Exited += (o, e) => Environment.Exit(1);
            DependencyContainer.Cache(Process);

            QuaverDirectory = Path.GetDirectoryName(Process.Process.MainModule.FileName);

            try
            {
                if (!Process.FindPattern(Signatures.QuaverBase.Pattern, out UIntPtr quaverBasePointer))
                    return false;

                QuaverBase = new QuaverBase((UIntPtr)Process.ReadUInt64(quaverBasePointer + Signatures.QuaverBase.Offset));
                ConfigManager = new QuaverConfigManager($@"{QuaverDirectory}\quaver.cfg");
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}