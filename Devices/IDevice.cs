using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Devices
{
    public interface IDevice
    {
        string Name { get; }
        void Open();
        void Close();
        void Start();
        void Stop();
        event OnMsgReceived OnMsgReceived;
        bool Send(IFrame frame);
        bool SendMultip(IEnumerable<IFrame> multiples);
    }

    public enum DeviceHardWareType
    {
        None,
        Virtual,
        Vector
    }

    public interface IFrame
    {
        uint MessageID { get; }
        byte[] Data { get; }
        int DLC { get; }
    }

    public class CanFrame : IFrame
    {
        public CanFrame()
        {

        }
        public CanFrame(uint messageID, byte[] data)
        {
            MessageID = messageID;
            Data = data;
        }

        public uint MessageID { get; set; }

        public byte[] Data { get; set; }

        public int DLC { get => GetDLCDataLength(Data.Length); }

        public static int GetDLCDataLength(int dataLength)
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
    }
}
