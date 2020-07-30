namespace Quaver.API.Memory.Objects
{
    public class QuaverGameplayAudioTiming : QuaverObject
    {
        public double Time => QuaverProcess.ReadDouble(BaseAddress + 0x10);
    }
}
