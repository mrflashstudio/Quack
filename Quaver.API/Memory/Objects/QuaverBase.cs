using System;

namespace Quaver.API.Memory.Objects
{
    public class QuaverBase : QuaverObject
    {
        public QuaverGameplayScreen GameplayScreen { get; private set; }

        public QuaverBase(UIntPtr pointerToBaseAddress) : base(pointerToBaseAddress)
        {
            Children = new QuaverObject[]
            {
                GameplayScreen = new QuaverGameplayScreen
                {
                    Offset = 0x128
                }
            };
        }
    }
}
