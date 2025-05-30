﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERad5TestGUI.Devices
{
    public interface IDevice
    {
        string Name { get; }

        bool Opened { get; }

        bool Started { get; }
        void Open();
        void Close();
        void Start();
        void Stop();
        event OnIFrameReceived OnIFramesReceived;
        event OnMsgReceived OnMsgReceived;
        DeviceRecieveFrameStatus RecieveStatus { get; }
        bool SendFD(IFrame frame);
        bool Send(IFrame frame);
        bool SendFDMultip(IEnumerable<IFrame> multiples);
    }

    public delegate bool SendData();
    public delegate bool SendMultipData();
    public delegate void RegisterRecieve();

    public enum DeviceRecieveFrameStatus
    {
        NotStart,
        Connected,
        DisConnect
    }

    public enum DeviceHardWareType
    {
        None,
        Virtual,
        Vector,
        Zlg
    }
    /// <summary>
    /// can 2.0 or can fd
    /// </summary>
    public enum FrameFlags
    {
        CAN,
        CANFD,
        CANFDSpeed,
    }

    public interface IFrame
    {
        uint MessageID { get; }
        byte[] Data { get; }
        int DLC { get; }
        FrameFlags FrameFlags { get; }
    }

    public class CanFrame : IFrame
    {
        private bool extendedFrame;
        private bool isCanFD;
        private int dlc;
        private byte fillData;

        public CanFrame()
        {

        }
        public CanFrame(uint messageID, byte[] data, FrameFlags frameFlags = FrameFlags.CANFDSpeed)
        {
            MessageID = messageID;
            Data = data;
            FrameFlags = frameFlags;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageID"></param>
        /// <param name="data"></param>
        /// <param name="extendedFrame"></param>
        /// <param name="isCanFD"></param>
        /// <param name="dlc">1-15</param>
        /// <param name="fillData"></param>
        public CanFrame(uint messageID, byte[] data, bool extendedFrame = false, bool isCanFD = false, int dlc = 8, byte fillData= 0x00, FrameFlags frameFlags = FrameFlags.CANFDSpeed) 
            : this(messageID, data, frameFlags)
        {
            this.extendedFrame = extendedFrame;
            this.isCanFD = isCanFD;
            this.dlc = dlc;
            this.fillData = fillData;
            int length = Math.Min(GetLengthByDlc(dlc), data.Length);
            //length = Math.Min(length , 8);

            Data = Enumerable.Repeat(fillData, length).ToArray();

            for (int i = 0; i < length; i++)
            {
                Data[i] = data[i];
            }
            FrameFlags = frameFlags;
        }

        public uint MessageID { get; set; }

        public byte[] Data { get; set; }

        public int DLC { get => GetDLCByDataLength(Data.Length); }

        public FrameFlags FrameFlags { get; }

        public static int GetDLCByDataLength(int dataLength)
        {
            if (dataLength <= 8)
            {
                return dataLength;
            }
            else if (dataLength <= 12)
            {
                return 9;
            }
            else if (dataLength <= 16)
            {
                return 10;
            }
            else if (dataLength <= 20)
            {
                return 11;
            }
            else if (dataLength <= 24)
            {
                return 12;
            }
            else if (dataLength <= 32)
            {
                return 13;
            }
            else if (dataLength <= 48)
            {
                return 14;
            }
            else if (dataLength <= 64)
            {
                return 15;
            }

            return 8;
        }
        public static int GetLengthByDlc(int dlc)
        {
            if (dlc <= 8)
                return dlc;

            switch (dlc)
            {
                case 9:
                    return 12;
                case 10:
                    return 16;
                case 11:
                    return 20;
                case 12:
                    return 24;
                case 13:
                    return 32; 
                case 14:
                    return 48; 
                case 15:
                    return 64;
                default:
                    return 64;
            }
        }
        public override string ToString()
        {
            return $"{MessageID:X} {string.Join(" ",Data.Select(x=>x.ToString("X2")))}";
        }
    }
}
