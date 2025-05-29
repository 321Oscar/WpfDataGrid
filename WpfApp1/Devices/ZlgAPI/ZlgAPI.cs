using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ERad5TestGUI.Devices.ZlgAPI
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ZCAN
    {
        public uint acc_code;
        public uint acc_mask;
        public uint reserved;
        public byte filter;
        public byte timing0;
        public byte timing1;
        public byte mode;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct CANFD
    {
        public uint acc_code;
        public uint acc_mask;
        public uint abit_timing;
        public uint dbit_timing;
        public uint brp;
        public byte filter;
        public byte mode;
        public UInt16 pad;
        public uint reserved;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct can_frame
    {
        public uint can_id;  /* 32 bit MAKE_CAN_ID + EFF/RTR/ERR flags */
        public byte can_dlc; /* frame payload length in byte (0 .. CAN_MAX_DLEN) */
        public byte __pad;   /* padding */
        public byte __res0;  /* reserved / padding */
        public byte __res1;  /* reserved / padding */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] data/* __attribute__((aligned(8)))*/;

        public override string ToString()
        {
            string log = $"{this.can_id:X3} ";//(int) + "\n\r";
            for (int j = 0; j < this.can_dlc; j++)
            {
                log += this.data[j].ToString("X2") + " ";
            }
            return log;
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct canfd_frame
    {
        public uint can_id;  /* 32 bit MAKE_CAN_ID + EFF/RTR/ERR flags */
        public byte len;     /* frame payload length in byte */
        public byte flags;   /* additional flags for CAN FD,i.e error code */
        public byte __res0;  /* reserved / padding */
        public byte __res1;  /* reserved / padding */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public byte[] data/* __attribute__((aligned(8)))*/;
    };

    [StructLayout(LayoutKind.Explicit)]
    public struct ZCAN_CHANNEL_INIT_CONFIG
    {
        [FieldOffset(0)]
        public uint can_type; //type:TYPE_CAN TYPE_CANFD

        [FieldOffset(4)]
        public ZCAN can;

        [FieldOffset(4)]
        public CANFD canfd;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct ZCAN_Transmit_Data
    {
        public can_frame frame;
        public uint transmit_type;

        public override string ToString()
        {
            return $"Tx {frame}";
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct ZCAN_Receive_Data
    {
        public can_frame frame;
        public UInt64 timestamp;//us

        public override string ToString()
        {
            return $"{timestamp} Rx {frame}";
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct ZCAN_TransmitFD_Data
    {
        public canfd_frame frame;
        public uint transmit_type;
    };

    public struct DeviceInfo
    {
        public uint device_type;  //设备类型
        public uint channel_count;//设备的通道个数
        public DeviceInfo(uint type, uint count)
        {
            device_type = type;
            channel_count = count;
        }
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct ZCAN_ReceiveFD_Data
    {
        public canfd_frame frame;
        public UInt64 timestamp;//us
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct ZCAN_CHANNEL_ERROR_INFO
    {
        public uint error_code;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] passive_ErrData;
        public byte arLost_ErrData;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct ZCLOUD_DEVINFO
    {
        public int devIndex;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public char[] type;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public char[] id;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public char[] owner;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public char[] model;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public char[] fwVer;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public char[] hwVer;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public char[] serial;
        public int status;             // 0:online, 1:offline
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] bCanUploads;   // each channel enable can upload
        public byte bGpsUpload;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct ZCLOUD_USER_DATA
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public char[] username;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public char[] mobile;

        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        // public char[] email;
        // public IntPtr pDevGroups;
        // public uint devGroupSize

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public char[] dllVer;

        public uint devCnt;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
        public ZCLOUD_DEVINFO[] devices;

    };
    public enum ZlgDeviceType
    {
        USBCANFD200U = 41,
        USBCANFD100U = 42,
    }

    public class Define
    {
        public const int TYPE_CAN = 0;
        public const int TYPE_CANFD = 1;

        /// <summary>
        /// CANFD 加速
        /// </summary>
        public const int CANFD_BRS = 0x01;

        public const int ZCAN_USBCAN1 = 3;
        public const int ZCAN_USBCAN2 = 4;
        public const int ZCAN_CANETUDP = 12;
        public const int ZCAN_CANETTCP = 17;
        public const int ZCAN_USBCAN_E_U = 20;
        public const int ZCAN_USBCAN_2E_U = 21;
        public const int ZCAN_CANWIFI_TCP = 25;
        public const int ZCAN_PCIECANFD_100U = 38;
        public const int ZCAN_PCIECANFD_200U = 39;
        public const int ZCAN_PCIECANFD_400U = 61;
        public const int ZCAN_PCIECANFD_200U_EX = 62;
        public const int ZCAN_USBCANFD_200U = 41;
        public const int ZCAN_USBCANFD_100U = 42;
        public const int ZCAN_USBCANFD_MINI = 43;
        public const int ZCAN_CLOUD = 46;
        public const int ZCAN_CANFDNET_200U_TCP = 48;
        public const int ZCAN_CANFDNET_200U_UDP = 49;
        public const int ZCAN_CANFDNET_400U_TCP = 52;
        public const int ZCAN_CANFDNET_400U_UDP = 53;
        public const int ZCAN_CANFDNET_800U_TCP = 57;
        public const int ZCAN_CANFDNET_800U_UDP = 58;
        public const int ZCAN_USBCANFD_400U = 76;

        public const int STATUS_ERR = 0;
        public const int STATUS_OK = 1;



        public static bool CanFDNetDevice(uint type)
        {
            bool canfdnetDevice = type == Define.ZCAN_CANFDNET_400U_TCP || type == Define.ZCAN_CANFDNET_400U_UDP ||
                type == Define.ZCAN_CANFDNET_200U_TCP || type == Define.ZCAN_CANFDNET_200U_UDP || type == Define.ZCAN_CANFDNET_800U_TCP ||
                type == Define.ZCAN_CANFDNET_800U_UDP;
            return canfdnetDevice;
        }
        public static bool TcpDevice(uint type)
        {
            return type == Define.ZCAN_CANETTCP || type == Define.ZCAN_CANWIFI_TCP || type == Define.ZCAN_CANFDNET_400U_TCP || type == Define.ZCAN_CANFDNET_200U_TCP || type == Define.ZCAN_CANFDNET_800U_TCP;
        }
        public static bool NetDevice(uint type)
        {
            return type == Define.ZCAN_CANETTCP || type == Define.ZCAN_CANETUDP || type == Define.ZCAN_CANWIFI_TCP ||
                type == Define.ZCAN_CANFDNET_400U_TCP || type == Define.ZCAN_CANFDNET_400U_UDP ||
                type == Define.ZCAN_CANFDNET_200U_TCP || type == Define.ZCAN_CANFDNET_200U_UDP || type == Define.ZCAN_CANFDNET_800U_TCP ||
                type == Define.ZCAN_CANFDNET_800U_UDP;
        }
        public static bool USBCANFD(uint type)
        {
            bool usbCanfd = type == Define.ZCAN_USBCANFD_100U ||
               type == Define.ZCAN_USBCANFD_200U ||
               type == Define.ZCAN_USBCANFD_MINI;
            return usbCanfd;
        }

        public static bool PCIECANFD(uint type)
        {
            return type == Define.ZCAN_PCIECANFD_100U ||
                type == Define.ZCAN_PCIECANFD_200U ||
                type == Define.ZCAN_PCIECANFD_400U ||
                type == Define.ZCAN_PCIECANFD_200U_EX;
        }

        public static bool CanfdDevice(uint type)
        {
            return USBCANFD(type) || PCIECANFD(type);
        }

        public static uint MakeCanId(uint id, int eff, int rtr, int err)//1:extend frame 0:standard frame
        {
            uint ueff = (uint)(!!(Convert.ToBoolean(eff)) ? 1 : 0);
            uint urtr = (uint)(!!(Convert.ToBoolean(rtr)) ? 1 : 0);
            uint uerr = (uint)(!!(Convert.ToBoolean(err)) ? 1 : 0);
            return id | ueff << 31 | urtr << 30 | uerr << 29;
        }

        public static bool SetCANFDStandard(uint channelID, IntPtr deviceHandle, string canfd_standard)
        {
            string path = channelID + "/canfd_standard";
            string value = canfd_standard.ToString();
            return 1 == Method.ZCAN_SetValue(deviceHandle, path, Encoding.ASCII.GetBytes(value));
        }

        internal static bool SetFdBaudrate(uint channelID, IntPtr deviceHandle, string abit_Baud, string dbit_Baud)
        {
            string path = channelID + "/canfd_abit_baud_rate";
            string value = abit_Baud.ToString();
            //if (1 != property_.SetValue(path, Encoding.ASCII.GetBytes(value)))
            if (1 != Method.ZCAN_SetValue(deviceHandle, path, Encoding.ASCII.GetBytes(value)))
            {
                return false;
            }
            path = channelID + "/canfd_dbit_baud_rate";
            value = dbit_Baud.ToString();
            //if (1 != property_.SetValue(path, Encoding.ASCII.GetBytes(value)))
            if (1 != Method.ZCAN_SetValue(deviceHandle, path, Encoding.ASCII.GetBytes(value)))
            {
                return false;
            }
            return true;
        }

        internal static bool SetResistanceEnable(IntPtr deviceHandle, uint channelIndex, string resistanceEnable)
        {
            string path = channelIndex + "/initenal_resistance";
            string value = resistanceEnable;
            return 1 == Method.ZCAN_SetValue(deviceHandle, path, Encoding.ASCII.GetBytes(value));
        }

        internal static bool SetFilters(IntPtr deviceHandle, uint channelIndex, List<ZlgFilterConfig> zlgFilterConfigs)
        {
            string path = channelIndex + "/filter_clear";//清除滤波
            string value = "0";

            if (0 == Method.ZCAN_SetValue(deviceHandle, path, Encoding.ASCII.GetBytes(value)))
            {
                return false;
            }
            foreach (var config in zlgFilterConfigs)
            {
                path = channelIndex + "/filter_mode";
                value = config.FilterMode.ToString();

                if (value == "2")
                {
                    return true;
                }

                if (0 == Method.ZCAN_SetValue(deviceHandle, path, Encoding.ASCII.GetBytes(value)))
                {
                    return false;
                }

                path = channelIndex + "/filter_start";
                value = config.StartID;

                if (0 == Method.ZCAN_SetValue(deviceHandle, path, Encoding.ASCII.GetBytes(value)))
                {
                    return false;
                }

                path = channelIndex + "/filter_end";
                value = config.EndID;

                if (0 == Method.ZCAN_SetValue(deviceHandle, path, Encoding.ASCII.GetBytes(value)))
                {
                    return false;
                }
            }
            path = channelIndex + "/filter_ack";//滤波生效
            value = "0";

            if (0 == Method.ZCAN_SetValue(deviceHandle, path, Encoding.ASCII.GetBytes(value)))
            {
                return false;
            }

            //如果要设置多条滤波，在清除滤波和滤波生效之间设置多条滤波即可
            return true;
        }

        internal static IEnumerable<uint> GetCanChannels(uint deviceTypeIndex)
        {
            switch (deviceTypeIndex)
            {
                case 42: return new uint[] { 0 };
                default:
                case 41:
                    return new List<uint> { 0, 1 };
            }
        }
    }
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int SetValueFunc(string path, byte[] value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr GetValueFunc(string path);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr GetPropertysFunc(string path, string value);

    public struct IProperty
    {
        public SetValueFunc SetValue;
        public GetValueFunc GetValue;
        public GetPropertysFunc GetPropertys;
    };

    public class Method
    {
        [DllImport("zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr ZCAN_OpenDevice(uint device_type, uint device_index, uint reserved);

        [DllImport("zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern uint ZCAN_CloseDevice(IntPtr device_handle);

        [DllImport("zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
        // pInitConfig -> ZCAN_CHANNEL_INIT_CONFIG
        public static extern IntPtr ZCAN_InitCAN(IntPtr device_handle, uint can_index, IntPtr pInitConfig);

        [DllImport("zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern uint ZCAN_SetValue(IntPtr device_handle, string path, byte[] value);

        [DllImport("zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern uint ZCAN_SetValue(IntPtr device_handle, string path, IntPtr value);

        [DllImport("zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern uint ZCAN_StartCAN(IntPtr channel_handle);

        [DllImport("zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern uint ZCAN_ResetCAN(IntPtr channel_handle);
        [DllImport("zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern uint ZCAN_ClearBuffer(IntPtr channel_handle);

        [DllImport("zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
        // pTransmit -> ZCAN_Transmit_Data
        public static extern uint ZCAN_Transmit(IntPtr channel_handle, IntPtr pTransmit, uint len);

        [DllImport("zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
        // pTransmit -> ZCAN_TransmitFD_Data
        public static extern uint ZCAN_TransmitFD(IntPtr channel_handle, IntPtr pTransmit, uint len);

        [DllImport("zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern uint ZCAN_GetReceiveNum(IntPtr channel_handle, byte type);

        [DllImport("zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern uint ZCAN_Receive(IntPtr channel_handle, IntPtr data, uint len, int wait_time = -1);

        [DllImport("zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern uint ZCAN_ReceiveFD(IntPtr channel_handle, IntPtr data, uint len, int wait_time = -1);

        [DllImport("zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
        // pErrInfo -> ZCAN_CHANNEL_ERROR_INFO
        public static extern uint ZCAN_ReadChannelErrInfo(IntPtr channel_handle, IntPtr pErrInfo);

        [DllImport("zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr GetIProperty(IntPtr device_handle);

        [DllImport("zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool ZCLOUD_IsConnected();

        [DllImport("zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern void ZCLOUD_SetServerInfo(string httpAddr, ushort httpPort,
            string mqttAddr, ushort mqttPort);

        [DllImport("zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern uint ZCLOUD_ConnectServer(string username, string password);

        [DllImport("zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern uint ZCLOUD_DisconnectServer();

        [DllImport("zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr ZCLOUD_GetUserData();
    }
}
