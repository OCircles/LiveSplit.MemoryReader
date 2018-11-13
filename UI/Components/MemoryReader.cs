using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace LiveSplit.UI.Components
{
    public class MemoryReader
    {

        // Making old module read multi level pointers properly was too much of a hassle so I just lifted https://www.unknowncheats.me/forum/programming-for-beginners/174396-reading-multilevel-pointer-using.html

        private const int PROCESS_WM_READ = 0x0010;

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern IntPtr ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In, Out] byte[] buffer, UInt32 size, out IntPtr lpNumberOfBytesRead);


        public IntPtr[] pointers;


        public byte[] ReadMemory(string pname, IntPtr[] offsets, bool debug = false, string module = null)
        {
            byte[] mem = null; ;
            IntPtr tmpptr = IntPtr.Zero;
            IntPtr ptr = IntPtr.Zero;
            IntPtr ptr2 = IntPtr.Zero;

            var h = Process.GetProcessesByName(pname);
            
            if (h.Length != 0)
            {
                Process handle = Process.GetProcessesByName(pname)[0];
                
                IntPtr Base = getBase(handle);
                Console.WriteLine("Original base: " + Base);
                if (module != null)
                {
                    Base = getBase(handle, module);
                    Console.WriteLine("Module base: " + Base);
                    Console.WriteLine("");

                }

                for (int i = 0; i <= offsets.Length - 1; i++)
                {
                    if (i == 0)
                    {
                        if (debug)
                            Console.Write(Base + "[Base] + " + offsets[i] + "[OFFSET 0]");
                        ptr = IntPtr.Add(Base, (int)offsets[i]);
                        tmpptr = (IntPtr)ReadInt64(ptr, 8, handle.Handle);
                        if (debug)
                            Console.WriteLine(" is " + tmpptr);
                    }
                    else
                    {
                        if (debug)
                            Console.Write(tmpptr + " + " + offsets[i] + "[OFFSET " + i + "]");
                        ptr2 = IntPtr.Add(tmpptr, (int)offsets[i]);
                        tmpptr = (IntPtr)ReadInt64(ptr2, 8, handle.Handle);
                        if (debug)
                            Console.WriteLine(" is " + tmpptr);
                    }
                    if (offsets.Length == 1) ptr2 = ptr;
                    if (i == offsets.Length - 1) mem = ReadBytes(handle.Handle, ptr2, 64);
                }
            }
            return mem;
        }



        public static IntPtr[] ConstructPointer(string str)
        {
            IntPtr[] ptr = null;

            if (str.Contains(","))
            {
                String formatted = MemReaderUtil.TrimAllWithInplaceCharArray(str);
                String[] add = formatted.Split(',');
                ptr = new IntPtr[add.Length];
                for (int i = 0; i < add.Length; i++)
                {
                    if (IsHex(add[i])) ptr[i] = (IntPtr)Convert.ToInt32(add[i], 16);
                    else break;
                }
            }
            else
            {
                if (IsHex(str)) ptr = new IntPtr[] { (IntPtr)Convert.ToInt32(str, 16) };
            }

            return ptr;

        }

        private static bool IsHex(string str)
        {
            if (str.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase) || str.StartsWith("&H", StringComparison.CurrentCultureIgnoreCase))
            {
                str = str.Substring(2);
            }

            return uint.TryParse(str, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.CurrentCulture, out uint n);
        }

        private static Int64 ReadInt64(IntPtr Address, uint length = 4, IntPtr? Handle = null)
        {
            return BitConverter.ToInt32(ReadBytes((IntPtr)Handle, Address, length), 0);
        }

        static IntPtr getBase(Process handle, string module = null)
        {
            ProcessModuleCollection modules = handle.Modules;
            if (module != null)
            {
                for (int i = 0; i <= modules.Count - 1; i++)
                {
                    if (modules[i].ModuleName == module)
                    {
                        return (IntPtr)modules[i].BaseAddress;
                    }
                }
                Console.WriteLine("Module Not Found");

            }
            else
            {
                return (IntPtr)handle.MainModule.BaseAddress;
            }
            return (IntPtr)0;
        }

        public static byte[] ReadBytes(IntPtr Handle, IntPtr Address, uint BytesToRead)
        {
            IntPtr ptrBytesRead;
            byte[] buffer = new byte[BytesToRead];
            ReadProcessMemory(Handle, Address, buffer, BytesToRead, out ptrBytesRead);
            return buffer;
        }

    }
}