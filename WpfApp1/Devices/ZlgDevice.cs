using ERad5TestGUI.Devices.ZlgAPI;
using ERad5TestGUI.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ERad5TestGUI.Devices
{
    public class ZlgDeviceService
    {
        private readonly Services.LogService _logService;
        public ZlgDeviceService(Services.LogService logService)
        {
            Devices = new ObservableCollection<IDevice>();
            ZlgDevices = new ObservableCollection<ZlgDevice>();
            _logService = logService;
        }
        public ObservableCollection<IDevice> Devices { get; }
        public ObservableCollection<ZlgDevice> ZlgDevices { get; }
        public void CreateZlgChannel(uint zlgDeviceType, uint zlgDeviceIndex, uint zlgDeviceChannelIdx)
        {
            IDevice zlgDevice = Devices.FirstOrDefault(x =>
            {
                var zlg = x as ZlgDeviceCanChannel;
                return zlg.ChannelIndex == zlgDeviceChannelIdx && zlg.DeviceIndex == zlgDeviceIndex && zlg.DeviceType == zlgDeviceType;
            });
            if (zlgDevice != null)
                return;
            IDevice d;
            switch (zlgDeviceType)
            {
                default:
                    throw new NotSupportedException();
                case 41:
                    d = new ZlgUSBCANFDChannel(this, zlgDeviceType, zlgDeviceIndex, zlgDeviceChannelIdx, _logService);
                    break;
            }
            Devices.Add(d);
        }
        public bool OpenDevice(uint zlgDeviceType, uint zlgDeviceIndex)
        {
            ZlgDevice zlgDevice = ZlgDevices.FirstOrDefault(x => x.DeviceType == zlgDeviceType && x.DeviceIndex == zlgDeviceIndex);
            if (zlgDevice == null)
            {
                zlgDevice = new ZlgDevice(zlgDeviceType, zlgDeviceIndex);
                if (zlgDevice.Open())
                {
                    ZlgDevices.Add(zlgDevice);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return zlgDevice.Open();
            }

            return true;
        }
        public bool CloseDevice(uint zlgDeviceType, uint zlgDeviceIndex)
        {
            ZlgDevice zlgDevice = ZlgDevices.FirstOrDefault(x => x.DeviceType == zlgDeviceType && x.DeviceIndex == zlgDeviceIndex);
            if (zlgDevice == null)
            {
                return true;
            }
            else
            {
                return zlgDevice.Close();
            }
        }

        public void CloseChannel(uint zlgDeviceType, uint zlgDeviceIndex, uint zlgDeviceChannelIdx)
        {
            IDevice zlgChannel = Devices.FirstOrDefault(x =>
            {
                var zlg = x as ZlgDeviceCanChannel;
                return zlg.ChannelIndex == zlgDeviceChannelIdx && zlg.DeviceIndex == zlgDeviceIndex && zlg.DeviceType == zlgDeviceType;
            });
            if (zlgChannel == null)
                return;
            zlgChannel.Close();
            Devices.Remove(zlgChannel);
            if (!Devices.Any(x =>
                {
                    var zlg = x as ZlgDeviceCanChannel;
                    return zlg.DeviceIndex == zlgDeviceIndex && zlg.DeviceType == zlgDeviceType;
                }))
            {
                CloseDevice(zlgDeviceType, zlgDeviceIndex);
            }
        }

        internal IntPtr GetHandle(uint zlgDeviceType, uint zlgDeviceIndex)
        {
            ZlgDevice zlgDevice = ZlgDevices.FirstOrDefault(x => x.DeviceType == zlgDeviceType && x.DeviceIndex == zlgDeviceIndex);
            if (zlgDevice != null)
            {
                return zlgDevice.DeviceHandle;
            }
            return IntPtr.Zero;
        }
    }

    public class ZlgDevice
    {
        public ZlgDevice(uint deviceType, uint deviceIndex)
        {
            DeviceType = deviceType;
            DeviceIndex = deviceIndex;
        }
        public string Name { get; set; }
        public uint DeviceType { get; }
        public uint DeviceIndex { get; }
        public IntPtr DeviceHandle { get; private set; }
        public bool Opened { get; private set; }
        public bool Open()
        {
            if (!Opened)
            {
                DeviceHandle = ZlgAPI.Method.ZCAN_OpenDevice(DeviceType, DeviceIndex, 0);
                if (ZlgAPI.Define.STATUS_ERR == (int)DeviceHandle)
                {
                    //AddErr();
                    //LogInvoke("[zlgcan]打开设备失败,请检查设备类型和设备索引号是否正确");
                    return false;
                }
                Opened = true;
            }
            return Opened;
        }

        public bool Close()
        {
            Opened = false;
            ZlgAPI.Method.ZCAN_CloseDevice(DeviceHandle);
            return true;
        }
    }

    public class ZlgDeviceCanChannel : CommunityToolkit.Mvvm.ComponentModel.ObservableObject, IDevice
    {
        private readonly ZlgDeviceService _zlgDeviceService;
        private readonly Services.LogService _logService;
        /// <summary>
        /// 接收数据线程
        /// </summary>
        private Thread recv_thread;
        public ZlgDeviceCanChannel(ZlgDeviceService zlgDeviceService, uint deviceType, uint deviceIndex, uint channelIndex, Services.LogService logService)
        {
            _zlgDeviceService = zlgDeviceService;
            DeviceType = deviceType;
            DeviceIndex = deviceIndex;
            ChannelIndex = channelIndex;
            _logService = logService;
        }

        public string Name { get => $"{(ZlgAPI.ZlgDeviceType)DeviceType} Index {DeviceIndex} Channel {ChannelIndex}"; }
        public uint DeviceType { get; }
        public uint DeviceIndex { get; }
        public uint ChannelIndex { get; }
        public bool Opened { get; private set; }
        public bool Started 
        { 
            get => started;
            private set 
            { 
                started = value;
                if (started)
                    RecieveStatus = DeviceRecieveFrameStatus.Connected;
                else
                    RecieveStatus = DeviceRecieveFrameStatus.NotStart;
            }
        }
        public IntPtr ChannelHandle { get; protected set; }
        public IntPtr DeviceHandle { get; private set; }
        public DeviceRecieveFrameStatus RecieveStatus { get => _recieveStatus; private set => SetProperty(ref _recieveStatus, value); }

        public IZlgChannelConfig ZlgChannelConfig { get; set; }

        public event OnIFrameReceived OnIFramesReceived;
        public event OnMsgReceived OnMsgReceived;
        protected void Logger(string msg, [System.Runtime.CompilerServices.CallerMemberName] string caller = "")
        {
            _logService.Debug($"{Name} {caller} {msg}");
        }
        public void Close()
        {
            Started = false;
            Opened = false;
            Method.ZCAN_ResetCAN(ChannelHandle);
            //_zlgDeviceService.CloseDevice(DeviceType, DeviceIndex);
        }

        public void Open()
        {
            if (_zlgDeviceService.OpenDevice(DeviceType, DeviceIndex))
            {
                DeviceHandle = _zlgDeviceService.GetHandle(DeviceType, DeviceIndex);
                Opened = true;
            }
        }

        public bool Send(IFrame frame)
        {
            ZCAN_Transmit_Data can_data = new ZCAN_Transmit_Data();
            can_data.frame.can_id = frame.MessageID;
            can_data.frame.data = new byte[8];
            for (int i = 0; i < frame.Data.Length; i++)
            {
                can_data.frame.data[i] = frame.Data[i];
            }

            can_data.frame.can_dlc = (byte)frame.DLC;
            can_data.transmit_type = 2;

            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(can_data));
            Marshal.StructureToPtr(can_data, ptr, true);
            uint result = Method.ZCAN_Transmit(ChannelHandle, ptr, 1);
            Marshal.FreeHGlobal(ptr);

            return result == 1;
        }

        public bool SendFD(IFrame frame)
        {
            ZCAN_TransmitFD_Data canfd_data = new ZCAN_TransmitFD_Data();
            canfd_data.frame.can_id = frame.MessageID;
            canfd_data.frame.data = new byte[64];
            for (int i = 0; i < frame.Data.Length; i++)
            {
                canfd_data.frame.data[i] = frame.Data[i];
            }
            canfd_data.frame.len = (byte)frame.Data.Length;
            canfd_data.transmit_type = 2;
            canfd_data.frame.flags = (byte)(frame.FrameFlags == FrameFlags.CANFDSpeed ? Define.CANFD_BRS : 0);
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(canfd_data));
            Marshal.StructureToPtr(canfd_data, ptr, true);
            uint result = Method.ZCAN_TransmitFD(ChannelHandle, ptr, 1);
            Marshal.FreeHGlobal(ptr);

            return result == 1;
        }

        public bool SendFDMultip(IEnumerable<IFrame> frames)
        {
            var zcanFrames = new List<ZCAN_TransmitFD_Data>();
            foreach (var frame in frames)
            {
                //frame.ModifyTimeStamp(StartTime);
                //OnMsgSendReceive(frame);
                ZCAN_TransmitFD_Data canfd_data = new ZCAN_TransmitFD_Data();
                canfd_data.frame.can_id = frame.MessageID;
                canfd_data.frame.data = frame.Data;
                canfd_data.frame.len = (byte)frame.Data.Length;
                canfd_data.transmit_type = 2;
                canfd_data.frame.flags = (byte)(frame.FrameFlags == FrameFlags.CANFDSpeed ? Define.CANFD_BRS : 0);
                zcanFrames.Add(canfd_data);
            }
            var datas = zcanFrames.ToArray();
            int structsize = Marshal.SizeOf(typeof(ZCAN_TransmitFD_Data));
            IntPtr structInptr = Marshal.AllocHGlobal(structsize * datas.Length);

            for (int i = 0; i < datas.Length; i++)
            {
                Marshal.StructureToPtr(datas[i], IntPtr.Add(structInptr, i * structsize), false);
            }

            uint result = Method.ZCAN_TransmitFD(ChannelHandle, structInptr, (uint)zcanFrames.Count);

            Marshal.FreeHGlobal(structInptr);

            return result == (uint)frames.Count();
        }

        public void Start()
        {
            if (!InitChannel())
                return;

            if (Method.ZCAN_StartCAN(ChannelHandle) != Define.STATUS_OK)
            {
                return;
            }

            Started = true;
            recv_thread = new Thread(ReceiveCanMsgLoop)
            {
                //recv_thread.Name = "Thread" + startCount.ToString();
                //Priority = ThreadPriority.Highest,
                IsBackground = true
            };
            recv_thread.Start();
        }

        protected virtual bool InitChannel()
        {
            return true;
        }

        public void Stop()
        {
            Started = false;
            RecieveStatus = DeviceRecieveFrameStatus.NotStart;
            if (recv_thread != null)
            {
                recv_thread.Join();
            }
        }

        private readonly object locker = new object();
        private bool lockTaken = false;
        private DeviceRecieveFrameStatus _recieveStatus;
        private bool started;

        public bool GetMonitor()
        {
            bool x = Monitor.TryEnter(locker, 1000);
            lockTaken = true;
            return x;
        }

        public void ExitMonitor()
        {
            if (lockTaken)
            {
                Monitor.Exit(locker);
                lockTaken = false;
            }
        }
        private void ReceiveCanMsgLoop()
        {
            while (Started)
            {
                try
                {
                    if (GetMonitor())
                        RasieOnMsgReceived(Receive());
                    Thread.Sleep(10);
                }
                catch (Exception)
                {
                }
                finally { ExitMonitor(); }
            }
        }

        private IEnumerable<IFrame> Receive()
        {
            List<IFrame> can_mail = new List<IFrame>();
            uint len;

            len = Method.ZCAN_GetReceiveNum(ChannelHandle, Define.TYPE_CAN);
            if (len > 0 && len < 0xffff)
            {
                int size = Marshal.SizeOf(typeof(ZCAN_Receive_Data));
                IntPtr ptr = Marshal.AllocHGlobal((int)len * size);
                len = Method.ZCAN_Receive(ChannelHandle, ptr, len, 50);
                //Span
                for (int i = 0; i < len; ++i)
                {
                    ZCAN_Receive_Data receive_Data = (ZCAN_Receive_Data)Marshal.PtrToStructure((IntPtr)((Int64)ptr + i * size), typeof(ZCAN_Receive_Data));
                    CanFrame frame = new CanFrame(
                           receive_Data.frame.can_id,
                           receive_Data.frame.data,
                           dlc: receive_Data.frame.can_dlc);
                    //CANReceiveFrame frame = new CANReceiveFrame(receive_Data.frame.can_id, receive_Data.frame.data, receive_Data.frame.can_dlc, receive_Data.timestamp) { CanChannel = (int)ChannelId };
                    can_mail.Add(frame);
                }
                Marshal.FreeHGlobal(ptr);
            }

            len = Method.ZCAN_GetReceiveNum(ChannelHandle, Define.TYPE_CANFD);
            if (len > 0 && len < 0xffff)
            {
                int size = Marshal.SizeOf(typeof(ZCAN_ReceiveFD_Data));
                IntPtr ptr = Marshal.AllocHGlobal((int)len * size);
                try
                {
                    len = Method.ZCAN_ReceiveFD(ChannelHandle, ptr, len, 50);
                    for (int i = 0; i < len; ++i)
                    {
                        ZCAN_ReceiveFD_Data receive_Data = (ZCAN_ReceiveFD_Data)Marshal.PtrToStructure(
                            (IntPtr)((Int64)ptr + i * size), typeof(ZCAN_ReceiveFD_Data));
                        CanFrame frame = new CanFrame(
                            receive_Data.frame.can_id,
                            receive_Data.frame.data,
                            isCanFD: true,
                            dlc: CanFrame.GetDLCByDataLength(receive_Data.frame.len));

                        can_mail.Add(frame);
                    }
                    //OnRecvFDDataEvent(canfd_data, len);
                }
                catch (Exception)
                {
                }

                Marshal.FreeHGlobal(ptr);
            }

            return can_mail.ToArray();
        }

        private void RasieOnMsgReceived(IEnumerable<IFrame> frames)
        {
            if (frames == null || frames.Count() == 0)
                RecieveStatus = DeviceRecieveFrameStatus.DisConnect;
            else
            {
                RecieveStatus = DeviceRecieveFrameStatus.Connected;
            }
            OnIFramesReceived?.Invoke(frames);
        }

        public override string ToString()
        {
            return Name;
        }
    }
    /// <summary>
    /// USBCANFD-100U、USBCANFD-200U、USBCANFD-MINI
    /// </summary>
    public class ZlgUSBCANFDChannel : ZlgDeviceCanChannel
    {
        /// <summary>
        /// USBCANFD-100U、USBCANFD-200U、USBCANFD-MINI
        /// </summary>
        /// <param name="zlgDeviceService"></param>
        /// <param name="deviceType">USBCANFD-100U、USBCANFD-200U、USBCANFD-MINI</param>
        /// <param name="deviceIndex"></param>
        /// <param name="channelIndex"></param>
        public ZlgUSBCANFDChannel(ZlgDeviceService zlgDeviceService, uint deviceType, uint deviceIndex, uint channelIndex, Services.LogService logService)
            : base(zlgDeviceService, deviceType, deviceIndex, channelIndex, logService)
        {
            ZlgChannelConfig = new ZlgUSBCANFDChannelConfig()
            {
                Abit_Baud = "500000",
                Dbit_Baud = "2000000",
                ResistanceEnable = "1",
            };
        }
        
        protected override bool InitChannel()
        {
            if (ZlgChannelConfig is ZlgUSBCANFDChannelConfig fdConfig)
            {
                if (!Define.SetCANFDStandard(this.ChannelIndex, DeviceHandle, fdConfig.CANFDStandard))
                {
                    Logger(ResourceHelper.GetXamlStringByKey("InitZlgCANFDStandardError"));
                    return false;
                }

                if (DeviceType == Define.ZCAN_USBCANFD_200U)
                {
                    string path = "0/set_device_recv_merge";
                    string value = "0";
                    Method.ZCAN_SetValue(DeviceHandle, path, Encoding.ASCII.GetBytes(value));
                }

                if (!Define.SetFdBaudrate(ChannelIndex, DeviceHandle, fdConfig.Abit_Baud, fdConfig.Dbit_Baud))
                {
                    Logger(ResourceHelper.GetXamlStringByKey("InitZlgCANBaudError"));
                    return false;
                }

                ZCAN_CHANNEL_INIT_CONFIG config_ = new ZCAN_CHANNEL_INIT_CONFIG();
                config_.canfd.mode = 0;
                config_.can_type = Define.TYPE_CANFD;

                IntPtr pConfig = Marshal.AllocHGlobal(Marshal.SizeOf(config_));
                Marshal.StructureToPtr(config_, pConfig, true);

                ChannelHandle = Method.ZCAN_InitCAN(DeviceHandle, ChannelIndex, pConfig);

                Marshal.FreeHGlobal(pConfig);

                if (0 == (int)ChannelHandle)
                {
                    //Logger("初始化CAN失败");
                    Logger(ResourceHelper.GetXamlStringByKey("InitZlgCANError"));
                    return false;
                }

                if (!Define.SetResistanceEnable(DeviceHandle, ChannelIndex, fdConfig.ResistanceEnable))
                {
                    //Logger("使能终端电阻失败");
                    Logger(ResourceHelper.GetXamlStringByKey("InitZlgUSBCANFDResistanceEnableError"));
                    return false;
                }

                //设置滤波
                if (fdConfig.ZlgFilterConfigs != null && !Define.SetFilters(DeviceHandle, ChannelIndex, fdConfig.ZlgFilterConfigs))
                {
                    //Logger("设置滤波失败");
                    Logger(ResourceHelper.GetXamlStringByKey("InitZlgUSBCANFilterError"));
                    return false;
                }

                return true;
            }

            return base.InitChannel();
        }
    }

    public interface IZlgChannelConfig
    {
        string Abit_Baud { get; set; }
        /// <summary>
        /// 0 - CAN;1 - CANFD 
        /// </summary>
        uint CanType { get; set; }
        /// <summary>
        /// 0 - 正常模式，1 - 只听模式
        /// </summary>
        uint Mode { get; set; }
    }

    public class ZlgFilterConfig
    {
        public string FilterMode { get; set; }
        public string StartID { get; set; }
        public string EndID { get; set; }
    }

    public class ZlgUSBCANFDChannelConfig : IZlgChannelConfig
    {
        public string Dbit_Baud { get; set; }
        /// <summary>
        /// 0-不使能;1-使能
        /// </summary>
        public string ResistanceEnable { get; set; }
        /// <summary>
        /// 0-CANFD ISO; 1-CANFD Non-ISO
        /// </summary>
        public string CANFDStandard { get; set; } = "0";
        public string Abit_Baud { get; set; }
        public uint CanType { get; set; } = 1;
        public uint Mode { get; set; }
        public List<ZlgFilterConfig> ZlgFilterConfigs { get; set; }
    }
}
