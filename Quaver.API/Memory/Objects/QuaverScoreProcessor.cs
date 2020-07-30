using Quaver.API.Enums;

namespace Quaver.API.Memory.Objects
{
    public class QuaverScoreProcessor : QuaverObject
    {
        public Mods CurrentMods => (Mods)QuaverProcess.ReadInt64(BaseAddress + 0x40);
    }
}
