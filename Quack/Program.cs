using Quack.Configuration;
using Quack.Core;
using Quaver.API;
using Quaver.API.Enums;
using Quaver.API.Replays;
using SimpleDependencyInjection;
using System;
using System.Threading;
using System.Windows.Forms;

namespace Quack
{
    class Program
    {
        private static Updater.Updater updater;

        private static QuaverManager quaverManager;
        private static ConfigManager configManager;

        private static Bot bot;

        private static BotMode currentBotMode = BotMode.None;

        private static Replay replay;

        private static bool canLoad
        {
            get
            {
                if (quaverManager.QuaverBase.GameplayScreen.IsLoaded)
                    return currentBotMode == BotMode.AutoplayReplay || replay.MapMd5 == quaverManager.QuaverBase.GameplayScreen.CurrentMapChecksum;

                return false;
            }
        }

        [STAThread]
        static void Main(string[] args)
        {
            Console.Title = "Quack";

            updater = new Updater.Updater();

            try
            {
                updater.CheckForUpdates();
            }
            catch (Exception ex)
            {
                Console.Clear();
                Console.WriteLine("Checking for updates failed!");
                Console.WriteLine("Please report this on GitHub!");
                Console.WriteLine($"\nERROR: {ex}");
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey(true);
            }

            quaverManager = new QuaverManager();
            if (!quaverManager.Initialize())
            {
                Console.Clear();
                Console.WriteLine("Quack failed to initialize!");
                Console.WriteLine("Please report this on GitHub!");
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
                Environment.Exit(0);
            }

            configManager = new ConfigManager();

            DependencyContainer.Cache(quaverManager);
            DependencyContainer.Cache(configManager);

            bot = new Bot();

            drawMainMenu();
        }

        private static void drawMainMenu()
        {
            Console.Clear();
            drawDuck();

            if (currentBotMode == BotMode.None)
                drawModeSelection();
            else if (canLoad)
            {
                configManager.RefreshConfig();
                quaverManager.ConfigManager.RefreshConfig();

                var map = quaverManager.QuaverBase.GameplayScreen.CurrentMap;

                var mods = quaverManager.QuaverBase.GameplayScreen.Ruleset.ScoreProcessor.CurrentMods;
                bool flipInputs = currentBotMode == BotMode.UserReplay && (Mods)Math.Abs(replay.Mods - mods) == Mods.Mirror;

                if (currentBotMode == BotMode.AutoplayReplay)
                    replay = Replay.GenerateAutoplayReplay(map);

                Console.WriteLine($"\n\n~ Playing{(currentBotMode == BotMode.UserReplay ? $" {replay.PlayerName}'s replay on" : string.Empty)} {map.Artist} - {map.Title} [{map.DifficultyName}] by {map.Creator}");

                bot.Start(replay, flipInputs);
            }
            else
            {
                Console.WriteLine("\n\n~ Waiting for player...");
                Console.WriteLine("\n~ Press ESC to change mode.");

                while (!canLoad && !Console.KeyAvailable)
                    Thread.Sleep(150);

                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                    currentBotMode = BotMode.None;
            }

            drawMainMenu();
        }

        private static void drawModeSelection()
        {
            Console.Clear();
            drawDuck();

            Console.WriteLine("\n\n~ Select bot mode:");
            Console.WriteLine("\n~ 1. Autoplay");
            Console.WriteLine("~ 2. Replay");

            switch (Console.ReadKey(true).Key)
            {
                case ConsoleKey.D1:
                    currentBotMode = BotMode.AutoplayReplay;
                    break;
                case ConsoleKey.D2:
                    using (var openFileDialog = new OpenFileDialog())
                    {
                        openFileDialog.Filter = "Quaver replay file (*.qr)|*.qr";
                        openFileDialog.RestoreDirectory = true;

                        if (openFileDialog.ShowDialog() == DialogResult.OK)
                            replay = Replay.Parse(openFileDialog.FileName);
                        else
                            goto default;
                    }
                    currentBotMode = BotMode.UserReplay;
                    break;
                default:
                    drawModeSelection();
                    break;
            }

            drawMainMenu();
        }

        private static void drawDuck()
        {
            Console.WriteLine();
            Console.WriteLine("                    ██████      ");
            Console.WriteLine("                  ██      ██    ");
            Console.WriteLine("                ██          ██  ");
            Console.WriteLine($"                ██      ██  ██      Quack v{updater.CurrentVersion} by mrflashstudio");
            Console.WriteLine("                ██        ░░░░██  ~");
            Console.WriteLine("                  ██      ████      The best and only *yet* Quaver multihack");
            Console.WriteLine("    ██              ██  ██      ");
            Console.WriteLine("  ██  ██        ████    ██      ");
            Console.WriteLine("  ██    ████████          ██    ");
            Console.WriteLine("  ██                        ██  ");
            Console.WriteLine("    ██                      ██  ");
            Console.WriteLine("    ██    ██      ████      ██  ");
            Console.WriteLine("      ██    ████████      ██    ");
            Console.WriteLine("      ██                  ██    ");
            Console.WriteLine("        ████          ████      ");
            Console.WriteLine("            ██████████          ");
        }
    }

    public enum BotMode
    {
        None,
        AutoplayReplay,
        UserReplay
    }
}
