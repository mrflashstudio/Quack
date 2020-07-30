using System;
using System.Text;

namespace Quaver.API.Memory.Objects
{
    public class QuaverGameplayScreen : QuaverObject
    {
        public override bool IsLoaded => base.IsLoaded && QuaverProcess.ReadInt32(BaseAddress + 0xE8) == 2;// && QuaverProcess.ReadBool(BaseAddress + 0x20C);

        public QuaverGameplayAudioTiming GameplayAudioTiming { get; private set; }

        public QuaverRuleset Ruleset { get; private set; }

        public string CurrentMapChecksum
        {
            get
            {
                UIntPtr md5Pointer = (UIntPtr)QuaverProcess.ReadUInt64(BaseAddress + 0x70);
                uint md5Length = QuaverProcess.ReadUInt32(md5Pointer + 0x8) * 2;

                return Encoding.Unicode.GetString(QuaverProcess.ReadMemory(md5Pointer + 0xC, md5Length));
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
