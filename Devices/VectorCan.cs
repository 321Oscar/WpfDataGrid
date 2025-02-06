using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using vxlapi_NET;

namespace WpfApp1.Devices
{
    public class VectorCan
    {
        // -----------------------------------------------------------------------------------------------
        // DLL Import for RX events
        // -----------------------------------------------------------------------------------------------
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int WaitForSingleObject(int handle, int timeOut);
        // -----------------------------------------------------------------------------------------------

        // -----------------------------------------------------------------------------------------------
        // Global variables
        // -----------------------------------------------------------------------------------------------
        // Driver access through XLDriver (wrapper)
        private XLDriver CANDemo = new XLDriver();
        private String appName = "xlCANdemoNET";

        /// <summary>
        /// // Driver configuration
        /// </summary>
        private XLClass.xl_driver_config driverConfig = new XLClass.xl_driver_config();

        /// <summary>
        /// Variables required by XLDriver
        /// </summary>
        private XLDefine.XL_HardwareType hwType = XLDefine.XL_HardwareType.XL_HWTYPE_NONE;
        private uint hwIndex = 0;
        private uint hwChannel = 0;
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
        // -----------------------------------------------------------------------------------------------
        public event OnMsgReceived OnMsgReceived;
        public XLDefine.XL_Status OpenDriver()
        {
            return CANDemo.XL_OpenDriver();
        }
        public List<XLClass.xl_channel_config> Channels { get; private set; }

        public VectorCan()
        {
            GetDriverConfig();
        }

        public void GetDriverConfig()
        {
            var status = CANDemo.XL_GetDriverConfig(ref driverConfig);
            if (status == XLDefine.XL_Status.XL_SUCCESS)
            {
                Channels = new List<XLClass.xl_channel_config>();
                for (int i = 0; i < driverConfig.channelCount; i++)
                {
                    //Console.WriteLine("\n                   [{0}] " + driverConfig.channel[i].name, i);
                    //Console.WriteLine("                    - Channel Mask    : " + driverConfig.channel[i].channelMask);
                    //Console.WriteLine("                    - Transceiver Name: " + driverConfig.channel[i].transceiverName);
                    //Console.WriteLine("                    - Serial Number   : " + driverConfig.channel[i].serialNumber);
                    //cbbChannels.Items.Add($"[{i}] {driverConfig.channel[i].name}");
                    Channels.Add(driverConfig.channel[i]);
                }
            }
            else
            {
                throw new Exception("Get Vector Driver Config Fail!!");
            }

        }

        public void OpenPort(int channelIndex)
        {
            var channelCfg = Channels[channelIndex];

            var status = CANDemo.XL_SetApplConfig(appName: appName,
                                     appChannel: 0,
                                     hwType: channelCfg.hwType,
                                     hwIndex: channelCfg.hwIndex,
                                     hwChannel: channelCfg.hwChannel,
                                     XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN);

            if (status == XLDefine.XL_Status.XL_SUCCESS && GetAppChannelAndTestIsOk(0, ref txMask, ref txCi))
            {
                accessMask = txMask | rxMask;
                permissionMask = accessMask;

                status = CANDemo.XL_OpenPort(ref portHandle, appName, accessMask, ref permissionMask, 16000, XLDefine.XL_InterfaceVersion.XL_INTERFACE_VERSION_V4, XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN);

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

                status = CANDemo.XL_CanFdSetConfiguration(portHandle, accessMask, canFdConf);

                // Get RX event handle
                status = CANDemo.XL_SetNotification(portHandle, ref eventHandle, 1);
                // Activate channel - with reset clock
                status = CANDemo.XL_ActivateChannel(portHandle, accessMask, XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN, XLDefine.XL_AC_Flags.XL_ACTIVATE_RESET_CLOCK);
                // Run Rx thread
                //Console.WriteLine("Start Rx thread...");
                rxThread = new Thread(new ThreadStart(RXThread));
                rxThread.Start();

            }
        }

        public void ClosePort()
        {
            CANDemo.XL_ClosePort(portHandle);
            CANDemo.XL_DeactivateChannel(portHandle, accessMask);
        }

        public void CloseDriver()
        {
            CANDemo.XL_CloseDriver();
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
            txStatus = CANDemo.XL_CanTransmitEx(portHandle, txMask, ref messageCounterSent, xlEventCollection);
            Console.WriteLine("Transmit Message ({0})     : " + txStatus, messageCounterSent);

        }

        public uint Transmit(uint msgID, byte[] data, int dlc)
        {
            // Create an event collection with 2 messages (events)
            XLClass.xl_canfd_event_collection xlEventCollection = new XLClass.xl_canfd_event_collection(2);

            // event 1
            xlEventCollection.xlCANFDEvent[0].tag = XLDefine.XL_CANFD_TX_EventTags.XL_CAN_EV_TAG_TX_MSG;
            xlEventCollection.xlCANFDEvent[0].tagData.canId = msgID;
            switch (dlc)
            {
                default:
                case 8:
                    xlEventCollection.xlCANFDEvent[0].tagData.dlc = XLDefine.XL_CANFD_DLC.DLC_CAN_CANFD_8_BYTES;
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
            var txStatus = CANDemo.XL_CanTransmitEx(portHandle, txMask, ref messageCounterSent, xlEventCollection);
            return messageCounterSent;
        }

        // -----------------------------------------------------------------------------------------------
        /// <summary>
        /// Retrieve the application channel assignment and test if this channel can be opened
        /// </summary>
        // -----------------------------------------------------------------------------------------------
        private bool GetAppChannelAndTestIsOk(uint appChIdx, ref UInt64 chMask, ref int chIdx)
        {
            XLDefine.XL_Status status = CANDemo.XL_GetApplConfig(appName, appChIdx, ref hwType, ref hwIndex, ref hwChannel, XLDefine.XL_BusTypes.XL_BUS_TYPE_CAN);
            if (status != XLDefine.XL_Status.XL_SUCCESS)
            {
                throw new Exception("XL_GetApplConfig      : " + status);
                //PrintFunctionError();
            }

            chMask = CANDemo.XL_GetChannelMask(hwType, (int)hwIndex, (int)hwChannel);
            chIdx = CANDemo.XL_GetChannelIndex(hwType, (int)hwIndex, (int)hwChannel);
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

        // -----------------------------------------------------------------------------------------------
        /// <summary>
        /// EVENT THREAD (RX)
        /// 
        /// RX thread waits for Vector interface events and displays filtered CAN messages.
        /// </summary>
        // ----------------------------------------------------------------------------------------------- 
        public void RXThread()
        {
            // Create new object containing received data 
            XLClass.XLcanRxEvent receivedEvent = new XLClass.XLcanRxEvent();

            // Result of XL Driver function calls
            XLDefine.XL_Status xlStatus = XLDefine.XL_Status.XL_SUCCESS;

            // Result values of WaitForSingleObject 
            XLDefine.WaitResults waitResult = new XLDefine.WaitResults();


            // Note: this thread will be destroyed by MAIN
            while (true)
            {
                // Wait for hardware events
                waitResult = (XLDefine.WaitResults)WaitForSingleObject(eventHandle, 1000);
                List<CanFrame> frames = new List<CanFrame>();
                int count = 0;
                // If event occurred...
                if (waitResult != XLDefine.WaitResults.WAIT_TIMEOUT)
                {
                    // ...init xlStatus first
                    xlStatus = XLDefine.XL_Status.XL_SUCCESS;

                    // afterwards: while hw queue is not empty...
                    while (xlStatus != XLDefine.XL_Status.XL_ERR_QUEUE_IS_EMPTY)
                    {
                        // ...block RX thread to generate RX-Queue overflows
                        while (blockRxThread) Thread.Sleep(1000);

                        // ...receive data from hardware.
                        xlStatus = CANDemo.XL_CanReceive(portHandle, ref receivedEvent);

                        //  If receiving succeed....
                        if (xlStatus == XLDefine.XL_Status.XL_SUCCESS)
                        {
                            if (receivedEvent.tagData.canRxOkMsg.canId != 0)
                            {
                                CanFrame frame = new CanFrame(receivedEvent.tagData.canRxOkMsg.canId, receivedEvent.tagData.canRxOkMsg.data);
                                frames.Add(frame);
                                count++;
                            }
                            if(count == 10)
                            {
                                OnMsgReceived?.Invoke(frames.Take(frames.Count));
                                frames.Clear();
                                count = 0;
                            }
                            //Console.WriteLine(CANDemo.XL_CanGetEventString(receivedEvent));
                        }
                    }
                }
                // No event occurred
            }
        }
        // -----------------------------------------------------------------------------------------------

    }

    public delegate void OnMsgReceived(IEnumerable<IFrame> can_msg);
}
