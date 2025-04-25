using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERad5TestGUI.Devices
{
    public class ZlgDevice
    {

    }

    public class ZlgDeviceCan : IDevice
    {
        public string Name => throw new NotImplementedException();

        public bool Opened => throw new NotImplementedException();

        public bool Started => throw new NotImplementedException();

        public DeviceRecieveFrameStatus RecieveStatus => throw new NotImplementedException();

        public event OnIFrameReceived OnIFramesReceived;
        public event OnMsgReceived OnMsgReceived;

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void Open()
        {
            throw new NotImplementedException();
        }

        public bool Send(IFrame frame)
        {
            throw new NotImplementedException();
        }

        public bool SendFD(IFrame frame)
        {
            throw new NotImplementedException();
        }

        public bool SendFDMultip(IEnumerable<IFrame> multiples)
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
