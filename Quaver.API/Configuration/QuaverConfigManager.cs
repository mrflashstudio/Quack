using Quaver.API.Configuration.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using WindowsInput.Native;

namespace Quaver.API.Configuration
{
    public class QuaverConfigManager
    {
        private string configPath;

        private string[] rawConfig;

        public string SongsDirectory { get; private set; }

        public Dictionary<QuaverKey, VirtualKeyCode> KeyBindings = new Dictionary<QuaverKey, VirtualKeyCode>();

        public QuaverConfigManager(string configPath)
        {
            this.configPath = configPath;

            RefreshConfig();
        }

        public void RefreshConfig()
        {
            rawConfig = File.ReadAllLines(configPath);

            SongsDirectory = findLine("SongDirectory").Split(new char[] { '=' }, 2)[1].Trim();

            //keybinds
            QuaverKey[] quaverKeys = (QuaverKey[])Enum.GetValues(typeof(QuaverKey));

            foreach (var key in quaverKeys)
            {
                string line = findLine(key.ToString());

                string rawKey = line.Split(new char[] { '=' }, 2)[1].Trim();

                KeyBindings[key] = (VirtualKeyCode)(int)(XNAKeys)Enum.Parse(typeof(XNAKeys), rawKey);
            }
        }

        public VirtualKeyCode GetBindedKey(QuaverKey quaverKey) => KeyBindings[quaverKey];

        private string findLine(string configKey)
        {
            string line = Array.Find(rawConfig, l => l.StartsWith(configKey));
            if (line == default)
                throw new Exception($"Configuration key [{configKey}] was not found in the config file!");

            return line;
        }
    }
}
