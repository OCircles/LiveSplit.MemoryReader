using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace LiveSplit.UI.Components
{
    public class MemoryReader
    {

        const int PROCESS_WM_READ = 0x0010;

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        
        public int pointer;
        

        public byte[] ReadMemory(String proc)
        {

            if (Process.GetProcessesByName(proc).Length != 0)
            {

                Process gameProcess = Process.GetProcessesByName(proc)[0];

                var processHandle = OpenProcess(PROCESS_WM_READ, false, gameProcess.Id);

                int bytesRead = 0;
                byte[] buffer = new byte[64]; //'Hello World!' takes 12*2 bytes because of Unicode 

                ReadProcessMemory((int)processHandle, pointer, buffer, buffer.Length, ref bytesRead);
                


                return buffer;
                

            }
            else return null;
        }

        

    }
}