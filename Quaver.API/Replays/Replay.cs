using Quaver.API.Enums.Maps;
using Quaver.API.Maps;
using Quaver.API.Replays.SevenZip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Quaver.API.Replays
{
    public class Replay
    {
        public GameMode Mode { get; set; }

        public List<ReplayFrame> Frames { get; set; }

        public string ReplayVersion { get; set; }

        public string PlayerName { get; set; }

        public string MapMd5 { get; set; }

        public int RandomizeModifierSeed { get; set; } = -1;

        public void Flip() //todo: implement
        {
            throw new NotImplementedException();
        }

        public static Replay Parse(string filePath)
        {
            var parsedReplay = new Replay();

            using (var fs = new FileStream(filePath, FileMode.Open))
            using (var br = new BinaryReader(fs))
            {
                parsedReplay.ReplayVersion = br.ReadString();
                parsedReplay.MapMd5 = br.ReadString();
                br.ReadString();
                parsedReplay.PlayerName = br.ReadString();
                br.ReadString();
                br.ReadInt64();

                parsedReplay.Mode = (GameMode)br.ReadInt32();

                //mods
                if (parsedReplay.ReplayVersion == "0.0.1" || parsedReplay.ReplayVersion == "None")
                    br.ReadInt32();
                else
                    br.ReadInt64();

                br.ReadBytes(40);

                if (parsedReplay.ReplayVersion != "None")
                {
                    var replayVersion = new Version(parsedReplay.ReplayVersion);

                    if (replayVersion >= new Version("0.0.1"))
                        parsedReplay.RandomizeModifierSeed = br.ReadInt32();
                }

                parsedReplay.Frames = new List<ReplayFrame>();

                var frames = new List<string>();

                frames = Encoding.ASCII.GetString(LZMAHelper.Decompress(br.BaseStream).ToArray()).Split(',').ToList();

                foreach (var frame in frames)
                {
                    try
                    {
                        var frameSplit = frame.Split('|');

                        parsedReplay.Frames.Add(new ReplayFrame(int.Parse(frameSplit[0]), (ReplayKeyPressState)Enum.Parse(typeof(ReplayKeyPressState), frameSplit[1])));
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            return parsedReplay;
        }

        public static Replay GenerateAutoplayReplay(Qua map)
        {
            var replay = new Replay();

            replay.PlayerName = "Autoplay";
            replay.Mode = map.Mode;
            replay.Frames = new List<ReplayFrame>();

            var nonCombined = new List<ReplayAutoplayFrame>();

            foreach (var hitObject in map.HitObjects)
            {
                nonCombined.Add(new ReplayAutoplayFrame(hitObject, ReplayAutoplayFrameType.Press, hitObject.StartTime, KeyLaneToPressState(hitObject.Lane)));

                if (hitObject.IsLongNote)
                    nonCombined.Add(new ReplayAutoplayFrame(hitObject, ReplayAutoplayFrameType.Release, hitObject.EndTime - 1, KeyLaneToPressState(hitObject.Lane)));
                else
                    nonCombined.Add(new ReplayAutoplayFrame(hitObject, ReplayAutoplayFrameType.Release, hitObject.StartTime + 30, KeyLaneToPressState(hitObject.Lane)));
            }

            nonCombined = nonCombined.OrderBy(x => x.Time).ToList();

            ReplayKeyPressState state = 0;

            replay.Frames.Add(new ReplayFrame(-10000, 0));

            var startTimeGroup = nonCombined.GroupBy(x => x.Time).ToDictionary(x => x.Key, x => x.ToList());

            foreach (var item in startTimeGroup)
            {
                foreach (var frame in item.Value)
                {
                    switch (frame.Type)
                    {
                        case ReplayAutoplayFrameType.Press:
                            state |= KeyLaneToPressState(frame.HitObject.Lane);
                            break;
                        case ReplayAutoplayFrameType.Release:
                            state -= KeyLaneToPressState(frame.HitObject.Lane);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                replay.Frames.Add(new ReplayFrame(item.Key, state));
            }

            return replay;
        }

        public static ReplayKeyPressState KeyLaneToPressState(int lane) => (ReplayKeyPressState)Enum.Parse(typeof(ReplayKeyPressState), $"K{lane}");

        public static List<int> KeyPressStateToLanes(ReplayKeyPressState keys)
        {
            var lanes = new List<int>();

            if (keys.HasFlag(ReplayKeyPressState.K1))
                lanes.Add(0);
            if (keys.HasFlag(ReplayKeyPressState.K2))
                lanes.Add(1);
            if (keys.HasFlag(ReplayKeyPressState.K3))
                lanes.Add(2);
            if (keys.HasFlag(ReplayKeyPressState.K4))
                lanes.Add(3);
            if (keys.HasFlag(ReplayKeyPressState.K5))
                lanes.Add(4);
            if (keys.HasFlag(ReplayKeyPressState.K6))
                lanes.Add(5);
            if (keys.HasFlag(ReplayKeyPressState.K7))
                lanes.Add(6);
            if (keys.HasFlag(ReplayKeyPressState.K8))
                lanes.Add(7);
            if (keys.HasFlag(ReplayKeyPressState.K9))
                lanes.Add(8);

            return lanes;
        }
    }
}
