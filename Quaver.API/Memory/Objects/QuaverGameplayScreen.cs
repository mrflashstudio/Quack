using Quaver.API.Enums.Maps;
using Quaver.API.Maps;
using System;
using System.Collections.Generic;
using System.Text;

namespace Quaver.API.Memory.Objects
{
    public class QuaverGameplayScreen : QuaverObject
    {
        public override bool IsLoaded => base.IsLoaded && QuaverProcess.ReadInt32(BaseAddress + 0xE8) == 2;// && QuaverProcess.ReadBool(BaseAddress + 0x20C);

        public QuaverGameplayAudioTiming GameplayAudioTiming { get; private set; }

        public QuaverRuleset Ruleset { get; private set; }

        public Qua CurrentMap
        {
            get
            {
                var qua = new Qua();

                var mapsetPointer = (UIntPtr)QuaverProcess.ReadUInt64(BaseAddress + 0x58);

                //metadata
                qua.Mode = (GameMode)QuaverProcess.ReadInt32(mapsetPointer + 0x9c);
                qua.Title = QuaverProcess.ReadString(mapsetPointer + 0x20, true);
                qua.Artist = QuaverProcess.ReadString(mapsetPointer + 0x28, true);
                qua.Creator = QuaverProcess.ReadString(mapsetPointer + 0x40, true);
                qua.DifficultyName = QuaverProcess.ReadString(mapsetPointer + 0x48, true);
                qua.Checksum = QuaverProcess.ReadString(BaseAddress + 0x70, true);

                //hitobjects
                var hitObjectsList = (UIntPtr)QuaverProcess.ReadUInt64(mapsetPointer + 0x88);
                var hitObjectsElements = (UIntPtr)QuaverProcess.ReadUInt64(hitObjectsList + 0x8);
                int count = QuaverProcess.ReadInt32(hitObjectsElements + 0x8);
                for (int i = 0; i < count; i++)
                {
                    var currentElement = (UIntPtr)QuaverProcess.ReadUInt64(hitObjectsElements + 0x10 + 0x8 * i);

                    int lane = QuaverProcess.ReadInt32(currentElement + 0x14);
                    int startTime = QuaverProcess.ReadInt32(currentElement + 0x10);
                    int endTime = QuaverProcess.ReadInt32(currentElement + 0x18);

                    qua.HitObjects.Add(new HitObject
                    {
                        Lane = lane,
                        StartTime = startTime,
                        EndTime = endTime
                    });
                }

                return qua;
            }
        }

        public QuaverGameplayScreen()
        {
            Children = new QuaverObject[]
            {
                GameplayAudioTiming = new QuaverGameplayAudioTiming
                {
                    Offset = 0x48
                },
                Ruleset = new QuaverRuleset
                {
                    Offset = 0x50
                }
            };
        }
    }
}
