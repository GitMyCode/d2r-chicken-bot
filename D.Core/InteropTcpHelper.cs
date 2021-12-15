using System;
using System.Runtime.InteropServices;

namespace D.Core {
    public static class InteropTcpHelper {
        public static int DeleteSocket(MIB_TCPROW_OWNER_PID socket) {
            socket.state = (int)InteropTcpHelper.State.Delete_TCB;
            var ptr = GetPtrToNewObject(socket);
            // https://github.com/magisterquis/EDRSniper#error-317
            // 317 The function is unable to set the TCP entry since the application is running non-elevated.
            return InteropTcpHelper.SetTcpEntry(ptr);
        }

        private static IntPtr GetPtrToNewObject(object? obj) {
            if (obj == null) return IntPtr.Zero;
            IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(obj));
            Marshal.StructureToPtr(obj, ptr, false);
            return ptr;
        }

        [DllImport("iphlpapi.dll")]
        //private static extern int SetTcpEntry(MIB_TCPROW tcprow); 
        public static extern int SetTcpEntry(IntPtr pTcprow);

        public enum TCP_TABLE_CLASS {
            TCP_TABLE_BASIC_LISTENER,
            TCP_TABLE_BASIC_CONNECTIONS,
            TCP_TABLE_BASIC_ALL,
            TCP_TABLE_OWNER_PID_LISTENER,
            TCP_TABLE_OWNER_PID_CONNECTIONS,
            TCP_TABLE_OWNER_PID_ALL,
            TCP_TABLE_OWNER_MODULE_LISTENER,
            TCP_TABLE_OWNER_MODULE_CONNECTIONS,
            TCP_TABLE_OWNER_MODULE_ALL
        }

        [DllImport("iphlpapi.dll", SetLastError = true)]
        private static extern uint GetExtendedTcpTable(IntPtr pTcpTable,
            ref int dwOutBufLen,
            bool sort,
            int ipVersion,
            TCP_TABLE_CLASS tblClass,
            int reserved);

        public static MIB_TCPROW_OWNER_PID[] GetAllTcpConnections() {
            var AF_INET = 2; // IP_v4
            var buffSize = 0;

            // how much memory do we need?
            var ret = GetExtendedTcpTable(IntPtr.Zero, ref buffSize, true, AF_INET,
                TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL, 0);
            if (ret != 0 && ret != 122) // 122 insufficient buffer size
                throw new Exception("bad ret on check " + ret);
            var buffTable = Marshal.AllocHGlobal(buffSize);

            try {
                ret = GetExtendedTcpTable(buffTable,
                    ref buffSize,
                    true,
                    AF_INET,
                    TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL,
                    0);
                if (ret != 0)
                    throw new Exception("bad ret " + ret);

                // get the number of entries in the table
                var tab = (MIB_TCPTABLE_OWNER_PID)Marshal.PtrToStructure(buffTable, typeof(MIB_TCPTABLE_OWNER_PID));
                var rowPtr = (IntPtr)((long)buffTable + Marshal.SizeOf(tab.dwNumEntries));
                MIB_TCPROW_OWNER_PID[] tTable = new MIB_TCPROW_OWNER_PID[tab.dwNumEntries];

                for (var i = 0; i < tab.dwNumEntries; i++) {
                    var tcpRow = (MIB_TCPROW_OWNER_PID)Marshal
                        .PtrToStructure(rowPtr, typeof(MIB_TCPROW_OWNER_PID));
                    tTable[i] = tcpRow;
                    // next entry
                    rowPtr = (IntPtr)((long)rowPtr + Marshal.SizeOf(tcpRow));
                }

                return tTable;
            } finally {
                // Free the Memory
                Marshal.FreeHGlobal(buffTable);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_TCPROW_OWNER_PID {
            public uint state;
            public uint localAddr;
            public byte localPort1;
            public byte localPort2;
            public byte localPort3;
            public byte localPort4;
            public uint remoteAddr;
            public byte remotePort1;
            public byte remotePort2;
            public byte remotePort3;
            public byte remotePort4;
            public int owningPid;

            public ushort LocalPort {
                get {
                    return BitConverter.ToUInt16(
                        new byte[2] { localPort2, localPort1 }, 0);
                }
            }

            public ushort RemotePort {
                get {
                    return BitConverter.ToUInt16(
                        new byte[2] { remotePort2, remotePort1 }, 0);
                }
            }
        }

        public enum State {
            /// <summary> All </summary> 
            All = 0,

            /// <summary> Closed </summary> 
            Closed = 1,

            /// <summary> Listen </summary> 
            Listen = 2,

            /// <summary> Syn_Sent </summary> 
            Syn_Sent = 3,

            /// <summary> Syn_Rcvd </summary> 
            Syn_Rcvd = 4,

            /// <summary> Established </summary> 
            Established = 5,

            /// <summary> Fin_Wait1 </summary> 
            Fin_Wait1 = 6,

            /// <summary> Fin_Wait2 </summary> 
            Fin_Wait2 = 7,

            /// <summary> Close_Wait </summary> 
            Close_Wait = 8,

            /// <summary> Closing </summary> 
            Closing = 9,

            /// <summary> Last_Ack </summary> 
            Last_Ack = 10,

            /// <summary> Time_Wait </summary> 
            Time_Wait = 11,

            /// <summary> Delete_TCB </summary> 
            Delete_TCB = 12
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_TCPTABLE_OWNER_PID {
            public uint dwNumEntries;
            private readonly MIB_TCPROW_OWNER_PID table;
        }
    }
}