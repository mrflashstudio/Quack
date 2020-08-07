using Quaver.API.Configuration;
using Quaver.API.Maps;
using Quaver.API.Memory;
using Quaver.API.Memory.Objects;
using Quaver.API.Memory.Processes;
using SimpleDependencyInjection;
using System;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace Quaver.API
{
    public class QuaverManager
    {
        public QuaverProcess QuaverProcess { get; private set; }

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

            var quaverProcess = Process.GetProcessesByName("Quaver").FirstOrDefault();
            if (quaverProcess == null)
            {
                Console.WriteLine("\nWaiting for Quaver...");

                while (quaverProcess == null)
                {
                    quaverProcess = Process.GetProcessesByName("Quaver").FirstOrDefault();
                    Thread.Sleep(1500);
                }
            }

            QuaverProcess = new QuaverProcess(quaverProcess);
            QuaverProcess.Process.EnableRaisingEvents = true;
            QuaverProcess.Process.Exited += (o, e) => Environment.Exit(1);
            DependencyContainer.Cache(QuaverProcess);

            QuaverDirectory = Path.GetDirectoryName(QuaverProcess.Process.MainModule.FileName);

            try
            {
                if (!QuaverProcess.FindPattern(Signatures.QuaverBase.Pattern, out UIntPtr quaverBasePointer))
                    return false;

                QuaverBase = new QuaverBase(quaverBasePointer + Signatures.QuaverBase.Offset);
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