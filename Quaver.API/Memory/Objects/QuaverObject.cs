using Quaver.API.Memory.Processes;
using SimpleDependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Quaver.API.Memory.Objects
{
    public class QuaverObject
    {
        protected QuaverProcess QuaverProcess;

        public bool SingleComponentLoaded => Parent?.SingleComponentLoaded ?? true && BaseAddress != UIntPtr.Zero;

        public virtual bool IsLoaded => SingleComponentLoaded && Children.All(child => child.IsLoaded);

        private UIntPtr? pointerToBaseAddress;
        public virtual UIntPtr BaseAddress
        {
            get
            {
                if (pointerToBaseAddress.HasValue)
                    return (UIntPtr)QuaverProcess.ReadUInt64(pointerToBaseAddress.Value);

                if (Parent.SingleComponentLoaded)
                    return (UIntPtr)QuaverProcess.ReadUInt64(Parent.BaseAddress + Offset);

                return UIntPtr.Zero;
            }
        }

        public int Offset;

        public QuaverObject Parent { get; set; } = null;

        private List<QuaverObject> children = new List<QuaverObject>();
        public QuaverObject[] Children
        {
            get => children.ToArray();
            set
            {
                children = value.ToList();

                foreach (var child in children)
                    child.Parent = this;
            }
        }

        public QuaverObject(UIntPtr? pointerToBaseAddress = null)
        {
            this.pointerToBaseAddress = pointerToBaseAddress;
            QuaverProcess = DependencyContainer.Get<QuaverProcess>();
        }

        public void Add(QuaverObject osuObject)
        {
            osuObject.Parent = this;
            children.Add(osuObject);
        }

        public void Clear() => children.Clear();
    }
}
