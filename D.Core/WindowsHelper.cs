using System;
using System.Runtime.InteropServices;

namespace D.Core
{
    public static class WindowsHelper
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SetForegroundWindow(IntPtr hWnd);

        //This is a replacement for Cursor.Position in WinForms
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;

        //This simulates a left mouse click
        public static void LeftMouseClick(int xpos, int ypos)
        {
            SetCursorPos(xpos, ypos);
            mouse_event(MOUSEEVENTF_LEFTDOWN, xpos, ypos, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, xpos, ypos, 0, 0);
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);

        const UInt32 WM_KEYDOWN = 0x0100;
        const int VK_ESCAPE = 0x1B;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);

        public static void SendEscapeKey(IntPtr hWnd) => PostMessage(hWnd, WM_KEYDOWN, VK_ESCAPE, 0);

        public static Rect GetWindowRect(IntPtr hWnd)
        {
            var windowRec = new Rect();
            GetWindowRect(hWnd, ref windowRec);
            return windowRec;
        }

        public enum AllocationProtect : uint
        {
            PAGE_EXECUTE = 0x00000010,
            PAGE_EXECUTE_READ = 0x00000020,
            PAGE_EXECUTE_READWRITE = 0x00000040,
            PAGE_EXECUTE_WRITECOPY = 0x00000080,
            PAGE_NOACCESS = 0x00000001,
            PAGE_READONLY = 0x00000002,
            PAGE_READWRITE = 0x00000004,
            PAGE_WRITECOPY = 0x00000008,
            PAGE_GUARD = 0x00000100,
            PAGE_NOCACHE = 0x00000200,
            PAGE_WRITECOMBINE = 0x00000400
        }

        //  https://docs.microsoft.com/en-au/windows/win32/memory/memory-protection-constants
        [Flags]
        public enum MemoryInformationProtection
        {
            PAGE_EXECUTE = 0x10,
            PAGE_EXECUTE_READ = 0x20,
            PAGE_EXECUTE_READWRITE = 0x40,
            PAGE_EXECUTE_WRITECOPY = 0x80,
            PAGE_NOACCESS = 0x01,
            PAGE_READONLY = 0x02,
            PAGE_READWRITE = 0x04,
            PAGE_WRITECOPY = 0x08,
            PAGE_TARGETS_INVALID = 0x40000000,
            PAGE_TARGETS_NO_UPDATE = 0x40000000,

            PAGE_GUARD = 0x100,
            PAGE_NOCACHE = 0x200,
            PAGE_WRITECOMBINE = 0x400
        }

        //  https://docs.microsoft.com/en-us/windows/win32/api/winnt/ns-winnt-memory_basic_information
        public enum MemoryInformationState
        {
            MEM_COMMIT = 0x1000,
            MEM_FREE = 0x10000,
            MEM_RESERVE = 0x2000
        }

        //  https://docs.microsoft.com/en-us/windows/win32/api/winnt/ns-winnt-memory_basic_information
        public enum MemoryInformationType
        {
            MEM_IMAGE = 0x1000000,
            MEM_MAPPED = 0x40000,
            MEM_PRIVATE = 0x20000
        }

        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VirtualMemoryOperation = 0x00000008,
            VirtualMemoryRead = 0x00000010,
            VirtualMemoryWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x000000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }

        public static T Read<T>(IntPtr processHandle, IntPtr address) where T : struct
        {
            return Read<T>(processHandle, address, 1)[0];
        }

        // copied from https://github.com/misterokaygo/MapAssist/
        public static T[] Read<T>(IntPtr processHandle, IntPtr address, int count) where T : struct
        {
            var size = Marshal.SizeOf<T>();
            var buffer = new byte[size * count];

            ReadProcessMemory(processHandle, address, buffer, buffer.Length, out var bytesRead);

            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                var result = new T[count];
                for (var i = 0; i < count; i++)
                    result[i] = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject() + i * size, typeof(T));

                return result;
            }
            finally
            {
                handle.Free();
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(
            uint processAccess,
            bool bInheritHandle,
            int processId
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out] byte[] lpBuffer,
            int dwSize,
            out IntPtr lpNumberOfBytesRead);

        // This helper static method is required because the 32-bit version of user32.dll does not contain this API
        // (on any versions of Windows), so linking the method will fail at run-time. The bridge dispatches the request
        // to the correct function (GetWindowLong in 32-bit mode and GetWindowLongPtr in 64-bit mode)

        // If that doesn't work, the following signature can be used alternatively.
        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        // This static method is required because Win32 does not support
        // GetWindowLongPtr directly
        public static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 8)
                return GetWindowLongPtr64(hWnd, nIndex);
            return GetWindowLongPtr32(hWnd, nIndex);
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy,
            uint uFlags);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress,
            out MEMORY_BASIC_INFORMATION64 lpBuffer, uint dwLength);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory2(IntPtr hProcess, ulong lpBaseAddress, byte[] lpBuffer,
            UIntPtr nSize, ref UIntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hHandle);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetProcessDPIAware();

        [DllImport("user32.dll")]
        public static extern IntPtr GetClientRect(IntPtr hWnd, ref Rect rect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int Left { get; set; }
            public int Top { get; set; }
            public int Right { get; set; }
            public int Bottom { get; set; }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Point
        {
            public int x;

            public int y;
        }

        //  http://www.pinvoke.net/default.aspx/kernel32.virtualqueryex
        //  https://docs.microsoft.com/en-us/windows/win32/api/winnt/ns-winnt-memory_basic_information
        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORY_BASIC_INFORMATION64
        {
            public ulong BaseAddress;
            public ulong AllocationBase;
            public int AllocationProtect;
            public int __alignment1;
            public ulong RegionSize;
            public int State;
            public int Protect;
            public int Type;
            public int __alignment2;
        }
    }
}