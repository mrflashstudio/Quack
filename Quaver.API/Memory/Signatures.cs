namespace Quaver.API.Memory
{
    public static class Signatures
    {
        public static readonly Signature QuaverBase = new Signature
        {
            Pattern = "A8 62 ?? ?? FE 7F 00 00 ?? ?? ?? ?? ?? 00 00 00 00 00 00 00 00 00 00 00 1B",
            Offset = 0x20
            //Pattern = "01 41 ?? ?? FD 7F",
            //Offset = 0x28
        };
    }
}
