namespace Quaver.API.Memory
{
    public static class Signatures
    {
        public static readonly Signature QuaverBase = new Signature
        {
            Pattern = "00 00 00 01 41 ?? ?? ?? 7F 00 00",
            Offset = 0x9B
        };
    }
}
