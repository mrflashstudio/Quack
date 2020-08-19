using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Threading;

namespace Quack.Updater
{
    public class Updater
    {
        public string CurrentVersion
        {
            get
            {
                string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                return version.Remove(version.LastIndexOf(".0"));
            }
        }

        public Updater()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            cleanup();
        }

        public void CheckForUpdates()
        {
            Console.Clear();
            Console.WriteLine("Checking for updates...");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@"https://api.github.com/repos/mrflashstudio/Quack/releases/latest");
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.UserAgent = "Quack";

            string responseString = string.Empty;

            using (var response = (HttpWebResponse)request.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
                responseString = reader.ReadToEnd();

            var release = JsonConvert.DeserializeObject<GithubRelease>(responseString);
            if (release.tag_name != CurrentVersion)
                downloadUpdate(release);
            else
            {
                Console.WriteLine($"You are using the latest version!");
                Thread.Sleep(3500);
                Console.Clear();
            }
        }

        private void downloadUpdate(GithubRelease release)
        {
            Console.WriteLine("Downloading latest release...");

            Directory.CreateDirectory("_temp");
            using (var client = new WebClient())
                client.DownloadFile(release.assets[0].browser_download_url, $@"_temp\{release.assets[0].name}");

            ZipFile.ExtractToDirectory($@"_temp\{release.assets[0].name}", @"_temp");
            File.Delete($@"_temp\{release.assets[0].name}");

            string executableFilename = Path.GetFileName(Environment.GetCommandLineArgs()[0]);
            File.Move(@"_temp\Quack.exe", $@"_temp\{executableFilename}");
            File.Move(@"_temp\Quack.runtimeconfig.json", $@"_temp\{Path.GetFileNameWithoutExtension(executableFilename)}.runtimeconfig.json");

            var mainDirectoryInfo = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            foreach (var file in mainDirectoryInfo.GetFiles())
                if (file.Extension != ".ini")
                    file.MoveTo($"{file.Name}.old");

            var tempDirectoryInfo = new DirectoryInfo(@"_temp");
            foreach (var file in tempDirectoryInfo.GetFiles())
                file.MoveTo(file.Name);

            tempDirectoryInfo.Delete(true);

            Console.WriteLine("Restarting in 3 seconds...");

            Thread.Sleep(3000);
            Process.Start(Environment.GetCommandLineArgs()[0]);
            Environment.Exit(0);
        }

        private void cleanup()
        {
            var mainDirectoryInfo = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            foreach (var file in mainDirectoryInfo.GetFiles())
                if (file.Extension == ".old")
                    file.Delete();

            if (Directory.Exists(@"_temp"))
                Directory.Delete(@"_temp", true);
        }
    }
}
