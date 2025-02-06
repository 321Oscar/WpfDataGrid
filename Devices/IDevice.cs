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
        void Start();
        void Stop();
        event OnMsgReceived OnMsgReceived;
        bool Send(IFrame frame);
        bool SendMultip(IEnumerable<IFrame> multiples);
    }

    public interface IFrame
    {
        uint MessageID { get; }
        byte[] Data { get; }
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
    }
}
