using SimpleIniConfig;
using System.IO;

namespace Quack.Configuration
{
    public class ConfigManager
    {
        private Config config;

        public int AudioOffset
        {
            get => config.GetValue("AudioOffset", 0);
            set => config.SetValue("AudioOffset", value);
        }

        public void RefreshConfig() => config = new Config();

        public ConfigManager()
        {
            if (!File.Exists(@"config.ini")) //todo: haaaaaack
                File.AppendAllText(@"config.ini", "AudioOffset = 0");

            RefreshConfig();
        }
    }
}
