using Quaver.API.Enums.Maps;
using System;
using System.Collections.Generic;
using System.IO;

namespace Quaver.API.Maps
{
    public class Qua
    {
        public GameMode Mode { get; set; }

        public string Title { get; set; }

        public string Artist { get; set; }

        public string Creator { get; set; }

        public string DifficultyName { get; set; }

        public List<HitObject> HitObjects { get; private set; } = new List<HitObject>();

        public static Qua Parse(string filePath)
        {
            var parsedMap = new Qua();

            string[] lines = File.ReadAllLines(filePath);

            bool isHitobjects = false;
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                string[] lineSplit = line.Split(new char[] { ':' }, 2);
                string variable = lineSplit[0].TrimStart(new char[] { '-' }).Trim();
                string value = lineSplit[1].Trim();

                if (variable == "HitObjects")
                    isHitobjects = true;

                switch (variable)
                {
                    case "Mode":
                        parsedMap.Mode = (GameMode)Enum.Parse(typeof(GameMode), value);
                        break;
                    case "Title":
                        parsedMap.Title = value;
                        break;
                    case "Artist":
                        parsedMap.Artist = value;
                        break;
                    case "Creator":
                        parsedMap.Creator = value;
                        break;
                    case "DifficultyName":
                        parsedMap.DifficultyName = value;
                        break;
                    case "StartTime" when isHitobjects:
                        var ho = new HitObject();
                        ho.StartTime = int.Parse(value);
                        ho.Lane = int.Parse(lines[++i].Split(new char[] { ':' }, 2)[1].Trim());
                        if (lines[i + 1].Contains("EndTime"))
                            ho.EndTime = int.Parse(lines[++i].Split(new char[] { ':' }, 2)[1].Trim());

                        parsedMap.HitObjects.Add(ho);
                        break;
                }
            }

            return parsedMap;
        }
    }
}
