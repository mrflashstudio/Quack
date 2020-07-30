using Quaver.API.Memory.Processes.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using static Quaver.API.Memory.Processes.MemoryBasicInformation;

namespace Quaver.API.Memory.Processes
{
    public class QuaverProcess
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, [Out] byte[] lpBuffer, uint dwSize, out UIntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out UIntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", EntryPoint = "VirtualQueryEx")]
        public static extern int VirtualQueryEx32(IntPtr hProcess, UIntPtr lpAddress, out MEMORY_BASIC_INFORMATION_32 lpBuffer, uint dwLength);

        [DllImport("kernel32.dll", EntryPoint = "VirtualQueryEx")]
        public static extern int VirtualQueryEx64(IntPtr hProcess, UIntPtr lpAddress, out MEMORY_BASIC_INFORMATION_64 lpBuffer, uint dwLength);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern bool IsWow64Process(IntPtr processHandle, out bool wow64Process);

        public Process Process { get; private set; }

        public QuaverProcess(Process process)
        {
            Process = process;

            foreach (ProcessModule module in Process.Modules)
            {
                if (module.ModuleName == "Quaver.Shared.dll")
                {
                    quaverModule = module;
                    break;
                }
            }
        }

        private ProcessModule quaverModule;
        private List<MemoryRegion> cachedMemoryRegions;

        public bool Is64BitProcess
        {
            get
            {
                if (IsWow64Process(Process.Handle, out bool result))
                    return result;

                return false;
            }

        }

        public bool FindPattern(string pattern, out UIntPtr result)
        {
            var parsedPattern = Pattern.Parse(pattern);

            //UIntPtr quaverModuleBaseAddress = quaverModule != null ? (UIntPtr)quaverModule.BaseAddress.ToInt64() : UIntPtr.Zero;

            var regions = EnumerateMemoryRegions();
            foreach (var region in regions)
            {
                //if ((ulong)region.BaseAddress < (ulong)quaverModuleBaseAddress)
                //    continue;

                byte[] buffer = ReadMemory(region.BaseAddress, region.RegionSize.ToUInt32());
                if (findMatch(parsedPattern, buffer, out UIntPtr match))
                {
                    result = (UIntPtr)(region.BaseAddress.ToUInt64() + match.ToUInt64());
                    return true;
                }
            }

            result = UIntPtr.Zero;
            return false;
        }

        public List<MemoryRegion> EnumerateMemoryRegions()
        {
            var regions = new List<MemoryRegion>();
            ulong address = 0;
            ulong maxAddress = ulong.MaxValue;

            do
            {
                MemoryRegion region;
                if (Is64BitProcess)
                {
                    VirtualQueryEx64(Process.Handle, (UIntPtr)address, out MEMORY_BASIC_INFORMATION_64 basicInformation64, (uint)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION_64)));
                    region = new MemoryRegion(basicInformation64);
                }
                else
                {
                    VirtualQueryEx32(Process.Handle, (UIntPtr)address, out MEMORY_BASIC_INFORMATION_32 basicInformation32, (uint)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION_32)));
                    region = new MemoryRegion(basicInformation32);
                }

                if (region.State != MemoryState.MemFree && !region.Protect.HasFlag(MemoryProtect.PageGuard))
                    regions.Add(region);

                if (address == (ulong)region.BaseAddress + (ulong)region.RegionSize)
                    break;

                address = (ulong)region.BaseAddress + (ulong)region.RegionSize;
            }
            while (address <= maxAddress);

            return regions;
        }

        public byte[] ReadMemory(UIntPtr address, uint size)
        {
            byte[] result = new byte[size];
            ReadProcessMemory(Process.Handle, address, result, size, out UIntPtr bytesRead);
            return result;
        }

        public UIntPtr ReadMemory(UIntPtr address, byte[] buffer, uint size)
        {
            UIntPtr bytesRead;
            ReadProcessMemory(Process.Handle, address, buffer, size, out bytesRead);
            return bytesRead;
        }

        public void WriteMemory(UIntPtr address, byte[] data, uint length)
        {
            WriteProcessMemory(Process.Handle, address, data, length, out UIntPtr bytesWritten);
        }

        public int ReadInt32(UIntPtr address) => BitConverter.ToInt32(ReadMemory(address, sizeof(int)), 0);

        public uint ReadUInt32(UIntPtr address) => BitConverter.ToUInt32(ReadMemory(address, sizeof(uint)), 0);

        public long ReadInt64(UIntPtr address) => BitConverter.ToInt64(ReadMemory(address, sizeof(long)), 0);

        public ulong ReadUInt64(UIntPtr address) => BitConverter.ToUInt64(ReadMemory(address, sizeof(ulong)), 0);

        public float ReadFloat(UIntPtr address) => BitConverter.ToSingle(ReadMemory(address, sizeof(float)), 0);

        public double ReadDouble(UIntPtr address) => BitConverter.ToDouble(ReadMemory(address, sizeof(double)), 0);

        public bool ReadBool(UIntPtr address) => BitConverter.ToBoolean(ReadMemory(address, sizeof(bool)), 0);

        public string ReadString(UIntPtr address, bool multiply = false, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            UIntPtr stringAddress = (UIntPtr)ReadUInt32(address);
            int length = ReadInt32(stringAddress + 0x4) * (multiply ? 2 : 1);

            return encoding.GetString(ReadMemory(stringAddress + 0x8, (uint)length)).Replace("\0", string.Empty);
        }

        private unsafe bool findMatch(Pattern pattern, byte[] buffer, out UIntPtr result)
        {
            result = UIntPtr.Zero;

            int patternLength = pattern.Bytes.Length;
            int bufferLength = buffer.Length;

            fixed (byte* bufferPtr = buffer)
            {
                fixed (bool* maskPtr = pattern.Mask)
                {
                    fixed (byte* patternPtr = pattern.Bytes)
                    {
                        for (int i = 0; i + patternLength <= bufferLength; i++)
                        {
                            for (int j = 0; j < patternLength; j++)
                            {
                                if (!maskPtr[j] || patternPtr[j] == bufferPtr[i + j])
                                    continue;

                                goto loopEnd;
                            }

                            result = (UIntPtr)i;
                            return true;

                        loopEnd:;
                        }
                    }
                }
            }

            return false;
        }
    }
}
