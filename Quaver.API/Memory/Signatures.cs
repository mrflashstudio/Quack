namespace Quaver.API.Memory
{
    public static class Signatures
    {
        public static readonly Signature QuaverBase = new Signature
        {
            Pattern = "48 89 45 F8 48 89 45 F0 48 89 4D 10 48 BA ?? ?? ?? ?? ?? ?? ?? ?? 48 83 3A 00",
            Offset = 0xe
        };
    }
}
