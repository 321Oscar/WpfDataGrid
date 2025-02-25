using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using vxlapi_NET;
using WpfApp1.Services;

namespace WpfApp1.Devices
{
    public class VectorCan : CommunityToolkit.Mvvm.ComponentModel.ObservableObject, IDevice
    {
        // -----------------------------------------------------------------------------------------------
        // DLL Import for RX events
        // -----------------------------------------------------------------------------------------------
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int WaitForSingleObject(int handle, int timeOut);

        private readonly LogService logService;

        // -----------------------------------------------------------------------------------------------
        private readonly VectorCanService _vectorCanService;



        // -----------------------------------------------------------------------------------------------
        // Global variables
        // -----------------------------------------------------------------------------------------------
        // Driver access through XLDriver (wrapper)
        public XLDriver VectorDriver { get => _vectorCanService.VectorDriver; }
        //private String appName = "xlCANdemoNET";

        /// <summary>
        /// // Driver configuration
        /// </summary>
        //private XLClass.xl_driver_config driverConfig = new XLClass.xl_driver_config();

        /// <summary>
        /// Variables required by XLDriver
        /// </summary>
        //private XLDefine.XL_HardwareType hwType = XLDefine.XL_HardwareType.XL_HWTYPE_NONE;
        //private uint hwIndex = 0;
        //private uint hwChannel = 0;
        private int portHandle = -1;
        private int eventHandle = -1;
        private UInt64 accessMask = 0;
        private UInt64 permissionMask = 0;
        private UInt64 txMask = 0;
        private UInt64 rxMask = 0;
        private int txCi = 0;
        private int rxCi = 0;

        /// <summary>
        /// Global CAN FD ISO (default) / no ISO mode flag
        /// </summary>
        private uint canFdModeNoIso = 0;

        // RX thread
        private Thread rxThread;
        private bool blockRxThread = false;
        private DeviceRecieveFrameStatus recieveStatus;

        // -----------------------------------------------------------------------------------------------

        public VectorCan(LogService logService, VectorCanService vectorCanService, XLClass.xl_channel_config cfg, string name)
        {
            this.logService = logService;
            _vectorCanService = vectorCanService;
            ChannelCfg = cfg;
            Name = name;
        }
        public bool Opened { get; private set; }
        public bool Started { get; private set; }

        public XLClass.xl_channel_config ChannelCfg { get; }
        public event OnMsgReceived OnMsgReceived;
        public XLDefine.XL_Status OpenDriver()
        {
            return VectorDriver.XL_OpenDriver();
        }

        public void OpenPort()
        {
            //var channelCfg = Channels[channelIndex];
            //List<Func<(bool, string)>> funcs = new List<Func<(bool, string)>>()
            //{
            //    SetAppConfig,
            //    GetAppChannelAndCheck,
            //    OpenPortFunc,
            //    SetCANFDconfigFunc,
            //    SetNotificationFunc,
            //    ActivateChannelFunc,
            //};
            ////默认值
            //bool x = true;
            //bool resTotal = funcs.Aggregate(x, (b, s) =>
            //{
            //    if (!b)//第一个为x的值
            //        return b;
            //    var res = s.Invoke();
            //    if (!res.Item1)
            //        Console.WriteLine(res.Item2);

            //    return res.Item1;
            //});
            //Opened = resTotal;
            //return;
            var status = VectorDriver.XL_SetApplConfig(appName: VectorCanService.AppName,
                                     appChannel: 0,
                                     hwType: ChannelCfg.hwType,
                                     hwIndex: ChannelCfg.hwIndex,
                                     hwChannel: ChannelCfg.hwChannel,
                                     XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN);

            if (status == XLDefine.XL_Status.XL_SUCCESS && _vectorCanService.GetAppChannelAndTestIsOk(0, ref txMask, ref txCi,
                ChannelCfg.hwType, ChannelCfg.hwIndex, ChannelCfg.hwChannel, canFdModeNoIso, VectorCanService.AppName))
            {
                accessMask = txMask | rxMask;
                permissionMask = accessMask;

                status = VectorDriver.XL_OpenPort(ref portHandle, VectorCanService.AppName, accessMask, ref permissionMask, 16000, XLDefine.XL_InterfaceVersion.XL_INTERFACE_VERSION_V4, XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN);

                // --------------------
                // Set CAN FD config
                // --------------------
                XLClass.XLcanFdConf canFdConf = new XLClass.XLcanFdConf();

                // arbitration bitrate
                //canFdConf.arbitrationBitRate    = 1000000;
                canFdConf.arbitrationBitRate = 500000;
                canFdConf.tseg1Abr = 7;
                canFdConf.tseg2Abr = 2;
                canFdConf.sjwAbr = 2;

                // data bitrate
                //canFdConf.dataBitRate           = canFdConf.arbitrationBitRate * 2;
                canFdConf.dataBitRate = canFdConf.arbitrationBitRate * 4;
                canFdConf.tseg1Dbr = 7;
                canFdConf.tseg2Dbr = 2;
                canFdConf.sjwDbr = 2;

                if (canFdModeNoIso > 0)
                {
                    canFdConf.options = (byte)XLDefine.XL_CANFD_ConfigOptions.XL_CANFD_CONFOPT_NO_ISO;
                }
                else
                {
                    canFdConf.options = 0;
                }

                status = VectorDriver.XL_CanFdSetConfiguration(portHandle, accessMask, canFdConf);

                // Get RX event handle
                status = VectorDriver.XL_SetNotification(portHandle, ref eventHandle, 1);
                // Activate channel - with reset clock
                status = VectorDriver.XL_ActivateChannel(portHandle, accessMask, XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN, XLDefine.XL_AC_Flags.XL_ACTIVATE_RESET_CLOCK);

                Opened = true;
            }
            else
            {
                Opened = false;
            }
        }

        #region Func<(bool, string)>
        (bool, string) SetAppConfig()
        {
            var status = VectorDriver.XL_SetApplConfig(appName: VectorCanService.AppName,
                                     appChannel: 0,
                                     hwType: ChannelCfg.hwType,
                                     hwIndex: ChannelCfg.hwIndex,
                                     hwChannel: ChannelCfg.hwChannel,
                                     XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN);

            bool suc = status == XLDefine.XL_Status.XL_SUCCESS;

            return (suc, suc ? "" : $"Set App Config Fail {status}");
        }

        (bool, string) GetAppChannelAndCheck()
        {
            var suc = _vectorCanService.GetAppChannelAndTestIsOk(0, ref txMask, ref txCi,
                ChannelCfg.hwType, ChannelCfg.hwIndex, ChannelCfg.hwChannel, canFdModeNoIso, VectorCanService.AppName);

            //bool suc = status == XLDefine.XL_Status.XL_SUCCESS;

            return (suc, suc ? "" : $"AppChannelNotOK");
        }
        (bool, string) OpenPortFunc()
        {
            var status = VectorDriver.XL_OpenPort(ref portHandle, VectorCanService.AppName, accessMask, ref permissionMask, 16000, XLDefine.XL_InterfaceVersion.XL_INTERFACE_VERSION_V4, XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN);
            bool suc = status == XLDefine.XL_Status.XL_SUCCESS;

            return (suc, suc ? "" : $"Open Port Fail {status}");
        }

        (bool, string) SetCANFDconfigFunc()
        {
            XLClass.XLcanFdConf canFdConf = new XLClass.XLcanFdConf();

            // arbitration bitrate
            //canFdConf.arbitrationBitRate    = 1000000;
            canFdConf.arbitrationBitRate = 500000;
            canFdConf.tseg1Abr = 7;
            canFdConf.tseg2Abr = 2;
            canFdConf.sjwAbr = 2;

            // data bitrate
            //canFdConf.dataBitRate           = canFdConf.arbitrationBitRate * 2;
            canFdConf.dataBitRate = canFdConf.arbitrationBitRate * 4;
            canFdConf.tseg1Dbr = 7;
            canFdConf.tseg2Dbr = 2;
            canFdConf.sjwDbr = 2;

            if (canFdModeNoIso > 0)
            {
                canFdConf.options = (byte)XLDefine.XL_CANFD_ConfigOptions.XL_CANFD_CONFOPT_NO_ISO;
            }
            else
            {
                canFdConf.options = 0;
            }

            var status = VectorDriver.XL_CanFdSetConfiguration(portHandle, accessMask, canFdConf);

            bool suc = status == XLDefine.XL_Status.XL_SUCCESS;

            return (suc, suc ? "" : $"Set CANFD config Fail {status}");
        }

        (bool, string) SetNotificationFunc()
        {
            var status = VectorDriver.XL_SetNotification(portHandle, ref eventHandle, 1);

            bool suc = status == XLDefine.XL_Status.XL_SUCCESS;

            return (suc, suc ? "" : $"Set Notification Fail {status}");
        }
        (bool, string) ActivateChannelFunc()
        {
            var status = VectorDriver.XL_ActivateChannel(portHandle, accessMask, XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN, XLDefine.XL_AC_Flags.XL_ACTIVATE_RESET_CLOCK);

            bool suc = status == XLDefine.XL_Status.XL_SUCCESS;

            return (suc, suc ? "" : $"Activate Channel Fail {status}");
        }
        #endregion

        public void ClosePort()
        {
            VectorDriver.XL_DeactivateChannel(portHandle, accessMask);
            VectorDriver.XL_ClosePort(portHandle);
            Opened = false;
        }

        public void CANFDTransmitTest()
        {
            XLDefine.XL_Status txStatus;

            // Create an event collection with 2 messages (events)
            XLClass.xl_canfd_event_collection xlEventCollection = new XLClass.xl_canfd_event_collection(2);

            // event 1
            xlEventCollection.xlCANFDEvent[0].tag = XLDefine.XL_CANFD_TX_EventTags.XL_CAN_EV_TAG_TX_MSG;
            xlEventCollection.xlCANFDEvent[0].tagData.canId = 0x100;
            xlEventCollection.xlCANFDEvent[0].tagData.dlc = XLDefine.XL_CANFD_DLC.DLC_CAN_CANFD_8_BYTES;
            xlEventCollection.xlCANFDEvent[0].tagData.msgFlags = XLDefine.XL_CANFD_TX_MessageFlags.XL_CAN_TXMSG_FLAG_BRS | XLDefine.XL_CANFD_TX_MessageFlags.XL_CAN_TXMSG_FLAG_EDL;
            xlEventCollection.xlCANFDEvent[0].tagData.data[0] = 1;
            xlEventCollection.xlCANFDEvent[0].tagData.data[1] = 1;
            xlEventCollection.xlCANFDEvent[0].tagData.data[2] = 2;
            xlEventCollection.xlCANFDEvent[0].tagData.data[3] = 2;
            xlEventCollection.xlCANFDEvent[0].tagData.data[4] = 3;
            xlEventCollection.xlCANFDEvent[0].tagData.data[5] = 3;
            xlEventCollection.xlCANFDEvent[0].tagData.data[6] = 4;
            xlEventCollection.xlCANFDEvent[0].tagData.data[7] = 4;


            // event 2
            xlEventCollection.xlCANFDEvent[1].tag = XLDefine.XL_CANFD_TX_EventTags.XL_CAN_EV_TAG_TX_MSG;
            xlEventCollection.xlCANFDEvent[1].tagData.canId = 0x200;
            xlEventCollection.xlCANFDEvent[1].tagData.dlc = XLDefine.XL_CANFD_DLC.DLC_CANFD_64_BYTES;
            xlEventCollection.xlCANFDEvent[1].tagData.msgFlags = XLDefine.XL_CANFD_TX_MessageFlags.XL_CAN_TXMSG_FLAG_BRS | XLDefine.XL_CANFD_TX_MessageFlags.XL_CAN_TXMSG_FLAG_EDL;
            xlEventCollection.xlCANFDEvent[1].tagData.data[0] = 7;
            xlEventCollection.xlCANFDEvent[1].tagData.data[1] = 7;
            xlEventCollection.xlCANFDEvent[1].tagData.data[2] = 8;
            xlEventCollection.xlCANFDEvent[1].tagData.data[3] = 8;
            xlEventCollection.xlCANFDEvent[1].tagData.data[4] = 9;
            xlEventCollection.xlCANFDEvent[1].tagData.data[5] = 9;
            xlEventCollection.xlCANFDEvent[1].tagData.data[6] = 3;
            xlEventCollection.xlCANFDEvent[1].tagData.data[7] = 2;
            xlEventCollection.xlCANFDEvent[1].tagData.data[8] = 1;
            xlEventCollection.xlCANFDEvent[1].tagData.data[63] = 9;


            // Transmit events
            uint messageCounterSent = 0;
            txStatus = VectorDriver.XL_CanTransmitEx(portHandle, txMask, ref messageCounterSent, xlEventCollection);
            Console.WriteLine("Transmit Message ({0})     : " + txStatus, messageCounterSent);

        }

        public bool Transmit(uint msgID, byte[] data, int dlc, ref string fail)
        {
            //CANFDTransmitTest();

            // Create an event collection with 2 messages (events)
            XLClass.xl_canfd_event_collection xlEventCollection = new XLClass.xl_canfd_event_collection(1);

            // event 1
            xlEventCollection.xlCANFDEvent[0].tag = XLDefine.XL_CANFD_TX_EventTags.XL_CAN_EV_TAG_TX_MSG;
            xlEventCollection.xlCANFDEvent[0].tagData.canId = msgID;
            switch (dlc)
            {
                default:
                case 8:
                    xlEventCollection.xlCANFDEvent[0].tagData.dlc = XLDefine.XL_CANFD_DLC.DLC_CAN_CANFD_8_BYTES;
                    break;
                case 15:
                    xlEventCollection.xlCANFDEvent[0].tagData.dlc = XLDefine.XL_CANFD_DLC.DLC_CANFD_64_BYTES;
                    break;
            }

            xlEventCollection.xlCANFDEvent[0].tagData.msgFlags = XLDefine.XL_CANFD_TX_MessageFlags.XL_CAN_TXMSG_FLAG_BRS
                | XLDefine.XL_CANFD_TX_MessageFlags.XL_CAN_TXMSG_FLAG_EDL;
            for (int i = 0; i < data.Length; i++)
            {
                xlEventCollection.xlCANFDEvent[0].tagData.data[i] = data[i];
            }

            // Transmit events
            uint messageCounterSent = 0;
            var txStatus = VectorDriver.XL_CanTransmitEx(portHandle, txMask, ref messageCounterSent, xlEventCollection);
            fail = txStatus.ToString();
            return messageCounterSent == 1;
        }

        public bool Transmit(IEnumerable<IFrame> frames, ref string status)
        {
            uint count = (uint)frames.Count();
            XLClass.xl_canfd_event_collection xlEventCollection = new XLClass.xl_canfd_event_collection(count);
            for (int i = 0; i < count; i++)
            {
                IFrame frame = frames.Skip(i).Take(1).FirstOrDefault();
                xlEventCollection.xlCANFDEvent[i].tag = XLDefine.XL_CANFD_TX_EventTags.XL_CAN_EV_TAG_TX_MSG;
                xlEventCollection.xlCANFDEvent[i].tagData.canId = frame.MessageID;
                switch (frame.DLC)
                {
                    default:
                    case 8:
                        xlEventCollection.xlCANFDEvent[0].tagData.dlc = XLDefine.XL_CANFD_DLC.DLC_CAN_CANFD_8_BYTES;
                        break;
                    case 15:
                        xlEventCollection.xlCANFDEvent[0].tagData.dlc = XLDefine.XL_CANFD_DLC.DLC_CANFD_64_BYTES;
                        break;
                }

                xlEventCollection.xlCANFDEvent[0].tagData.msgFlags = XLDefine.XL_CANFD_TX_MessageFlags.XL_CAN_TXMSG_FLAG_BRS
                    | XLDefine.XL_CANFD_TX_MessageFlags.XL_CAN_TXMSG_FLAG_EDL;
                for (int j = 0; j < frame.Data.Length; j++)
                {
                    xlEventCollection.xlCANFDEvent[0].tagData.data[j] = frame.Data[j];
                }
            }
            uint messageCounterSent = 0;
            var txStatus = VectorDriver.XL_CanTransmitEx(portHandle, txMask, ref messageCounterSent, xlEventCollection);
            status = txStatus.ToString();
            return messageCounterSent == count;
        }

        // -----------------------------------------------------------------------------------------------
        /// <summary>
        /// EVENT THREAD (RX)
        /// 
        /// RX thread waits for Vector interface events and displays filtered CAN messages.
        /// </summary>
        // ----------------------------------------------------------------------------------------------- 
        public DeviceRecieveFrameStatus RecieveStatus { get => recieveStatus; private set => SetProperty(ref recieveStatus , value); }
        private void RXThread()
        {
            // Create new object containing received data 
            XLClass.XLcanRxEvent receivedEvent = new XLClass.XLcanRxEvent();

            // Result of XL Driver function calls
            XLDefine.XL_Status xlStatus = XLDefine.XL_Status.XL_SUCCESS;

            // Result values of WaitForSingleObject 
            XLDefine.WaitResults waitResult = new XLDefine.WaitResults();

            int receiveFlag = 0;
            // Note: this thread will be destroyed by MAIN
            while (true)
            {
                // Wait for hardware events
                //logService.Debug($"{Name} Wait for hardware events {receiveFlag++}");
                waitResult = (XLDefine.WaitResults)WaitForSingleObject(eventHandle, 1000);

                //int count = 0;
                // If event occurred...
                if (waitResult != XLDefine.WaitResults.WAIT_TIMEOUT)
                {
                    // ...init xlStatus first
                    xlStatus = XLDefine.XL_Status.XL_SUCCESS;
                    //logService.Debug($"{Name} receive success {receiveFlag}");
                    // afterwards: while hw queue is not empty...
                    while (xlStatus != XLDefine.XL_Status.XL_ERR_QUEUE_IS_EMPTY)
                    {
                        // ...block RX thread to generate RX-Queue overflows
                        while (blockRxThread) Thread.Sleep(1000);
                        //logService.Debug($"{Name} receive not Empty {receiveFlag}");
                        // ...receive data from hardware.
                        xlStatus = VectorDriver.XL_CanReceive(portHandle, ref receivedEvent);

                        //  If receiving succeed....
                        if (xlStatus == XLDefine.XL_Status.XL_SUCCESS)
                        {
                            if (receivedEvent.tagData.canRxOkMsg.canId != 0)
                            {
                                List<CanFrame> frames = new List<CanFrame>();
                                CanFrame frame = new CanFrame(
                                    receivedEvent.tagData.canRxOkMsg.canId,
                                    receivedEvent.tagData.canRxOkMsg.data,
                                    dlc: (int)receivedEvent.tagData.canRxOkMsg.dlc);
                                frames.Add(frame);
                                //count++;
                                OnMsgReceived?.Invoke(frames);
                                //logService.Debug($"{Name} add Frame {receiveFlag}");
                            }
                            //if (count == 10)
                            //{
                            //    OnMsgReceived?.Invoke(frames.Take(frames.Count));
                            //    frames.Clear();
                            //    count = 0;
                            //    logService.Debug($"{Name} Invoke Frame {receiveFlag}");
                            //}
                            //Console.WriteLine(CANDemo.XL_CanGetEventString(receivedEvent));
                        }
                    }
                    RecieveStatus = DeviceRecieveFrameStatus.Connected;
                }
                else
                {
                    RecieveStatus = DeviceRecieveFrameStatus.NoFramesFor1Second;
                }
                // No event occurred
            }
        }
        // -----------------------------------------------------------------------------------------------

        public string Name { get; set; }
        public void Open()
        {
            try
            {
                OpenPort();
            }
            catch (Exception ex)
            {
                logService.Error($"Vector Can[{Name}] Open Fail", ex);
                //throw;
            }

        }

        public void Close()
        {
            if (rxThread != null && rxThread.ThreadState == ThreadState.Running)
                rxThread.Abort();
            RecieveStatus = DeviceRecieveFrameStatus.NotStart;
            ClosePort();
        }

        public void Start()
        {
            // Run Rx thread
            //Console.WriteLine("Start Rx thread...");
            rxThread = new Thread(new ThreadStart(RXThread));
            rxThread.Start();
            Started = true;
            RecieveStatus = DeviceRecieveFrameStatus.Connected;
        }

        public void Stop()
        {
            if (rxThread != null && rxThread.ThreadState == ThreadState.Running)
                rxThread.Abort();
            //ClosePort();
            Started = false;
        }

        public bool Send(IFrame frame)
        {
            string fail = "";
            if (!Transmit(frame.MessageID, frame.Data, frame.DLC, ref fail))
            {
                logService.Debug($"Send Fail:{fail}");
                return false;
            }
            return true;
        }

        public bool SendMultip(IEnumerable<IFrame> multiples)
        {
            string fail = "";
            if (!Transmit(multiples, ref fail))
            {
                logService.Debug($"Send Fail:{fail}");
                return false;
            }
            return true;
        }
    }

    public class VectorCanService
    {
        /// <summary>
        /// // Driver configuration
        /// </summary>
        private XLClass.xl_driver_config driverConfig = new XLClass.xl_driver_config();
        private List<IDevice> vectorChannels;

        public const string AppName = "WPF_ERad5";
        private readonly LogService logService;

        //public string AppName { get => appName; }

        public VectorCanService(LogService logService)
        {
            VectorDriver = new XLDriver();
            VectorDriver.XL_OpenDriver();
            this.logService = logService;
            //GetDriverConfig();
        }

        public XLDriver VectorDriver { get; }

        //public List<XLClass.xl_channel_config> VectorChannels 
        //{
        //    get 
        //    {
        //        if (vectorChannels == null)
        //            GetDriverConfig();
        //        return vectorChannels; 
        //    } 
        //    private set => vectorChannels = value; 
        //}
        public XLClass.xl_driver_config DriverConfig => driverConfig;
       
        public List<IDevice> VectorChannels
        {
            get
            {
                if (vectorChannels == null)
                    GetDriverConfig();
                return vectorChannels;
            }
            private set => vectorChannels = value;
        }
        public void GetDriverConfig()
        {
            var status = VectorDriver.XL_GetDriverConfig(ref driverConfig);
            if (status == XLDefine.XL_Status.XL_SUCCESS)
            {
                VectorChannels = new List<IDevice>();
                for (int i = 0; i < driverConfig.channelCount; i++)
                {
                    //Console.WriteLine("\n                   [{0}] " + driverConfig.channel[i].name, i);
                    //Console.WriteLine("                    - Channel Mask    : " + driverConfig.channel[i].channelMask);
                    //Console.WriteLine("                    - Transceiver Name: " + driverConfig.channel[i].transceiverName);
                    //Console.WriteLine("                    - Serial Number   : " + driverConfig.channel[i].serialNumber);
                    //cbbChannels.Items.Add($"[{i}] {driverConfig.channel[i].name}");
                    VectorChannels.Add(new VectorCan(logService, this, driverConfig.channel[i], driverConfig.channel[i].name));
                }
            }
            else
            {
                throw new Exception("Get Vector Driver Config Fail!!");
            }

        }

        public bool GetAppChannelAndTestIsOk(uint appChIdx, ref UInt64 chMask, ref int chIdx,
             XLDefine.XL_HardwareType hwType, uint hwIndex, uint hwChannel, uint canFdModeNoIso, string appName = "eRad5WPF")
        {
            XLDefine.XL_Status status = VectorDriver.XL_GetApplConfig(appName, appChIdx, ref hwType, ref hwIndex, ref hwChannel, XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN);
            if (status != XLDefine.XL_Status.XL_SUCCESS)
            {
                throw new Exception("XL_GetApplConfig      : " + status);
                //PrintFunctionError();
            }

            chMask = VectorDriver.XL_GetChannelMask(hwType, (int)hwIndex, (int)hwChannel);
            chIdx = VectorDriver.XL_GetChannelIndex(hwType, (int)hwIndex, (int)hwChannel);
            if (chIdx < 0 || chIdx >= driverConfig.channelCount)
            {
                // the (hwType, hwIndex, hwChannel) triplet stored in the application configuration does not refer to any available channel.
                return false;
            }

            if ((driverConfig.channel[chIdx].channelBusCapabilities & XLDefine.XL_BusCapabilities.XL_BUS_ACTIVE_CAP_CAN) == 0)
            {
                // CAN is not available on this channel
                return false;
            }

            if (canFdModeNoIso > 0)
            {
                if ((driverConfig.channel[chIdx].channelCapabilities & XLDefine.XL_ChannelCapabilities.XL_CHANNEL_FLAG_CANFD_BOSCH_SUPPORT) == 0)
                {
                    throw new Exception(String.Format("{0} ({1}) does not support CAN FD NO-ISO", driverConfig.channel[chIdx].name.TrimEnd(' ', '\0'),
                        driverConfig.channel[chIdx].transceiverName.TrimEnd(' ', '\0')));
                    // return false;
                }
            }
            else
            {
                if ((driverConfig.channel[chIdx].channelCapabilities & XLDefine.XL_ChannelCapabilities.XL_CHANNEL_FLAG_CANFD_ISO_SUPPORT) == 0)
                {
                    throw new Exception(String.Format("{0} ({1}) does not support CAN FD ISO", driverConfig.channel[chIdx].name.TrimEnd(' ', '\0'),
                        driverConfig.channel[chIdx].transceiverName.TrimEnd(' ', '\0')));
                    //return false;
                }
            }

            return true;
        }
    }

    public delegate void OnMsgReceived(IEnumerable<IFrame> can_msg);
}
