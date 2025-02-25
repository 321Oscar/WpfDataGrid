using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using WpfApp1.Services;
using WpfApp1.Stores;

namespace WpfApp1.Devices
{
    public class VirtualDevice :CommunityToolkit.Mvvm.ComponentModel.ObservableObject, IDevice
    {
        private readonly SignalStore _signalStore;
        private readonly LogService logService;
        private readonly Random random;
        private bool isOpen;
        private bool isStart;

        public VirtualDevice(SignalStore signalStore, Services.LogService logService)
        {
            Name = "Virtual Device";
            _signalStore = signalStore;
            GenerateFrames();
            this.logService = logService;
            random = new Random();
        }

        //private Thread _receiveThread;
        private Task _receiceTask;
        private CancellationTokenSource tokenSource;
        private DeviceRecieveFrameStatus recieveStatus;

        public event OnMsgReceived OnMsgReceived;

        public string Name { get; set; }
        public bool Started { get { return isOpen && isStart; } }
        public bool Opened { get => isOpen; }
        public void Open()
        {
            isOpen = true;
        }

        public void Close()
        {
            isOpen = false;
        }

        public void Start()
        {
            if (Started)
                return;
            //_receiveThread = new Thread(new ThreadStart(() => Receive()));
            //_receiveThread.IsBackground = true;
            //_receiveThread.Start();
            tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            isStart = true;
            RecieveStatus = DeviceRecieveFrameStatus.Connected;
            _receiceTask = Task.Factory.StartNew(Receive, token);
        }

        public void Stop()
        {
            //_receiveThread.Abort();
            isStart = false;
            if (tokenSource != null)
            {
                tokenSource.Cancel();
                //tokenSource.Dispose();
            }
            RecieveStatus = DeviceRecieveFrameStatus.NotStart;
        }

        private void RasieOnMsgReceived(IEnumerable<IFrame> frames)
        {
            OnMsgReceived?.Invoke(frames);
        }
        public DeviceRecieveFrameStatus RecieveStatus { get => recieveStatus; private set => SetProperty(ref recieveStatus, value); }

        private void Receive()
        {
            tokenSource.Token.ThrowIfCancellationRequested();

            while (true && !tokenSource.Token.IsCancellationRequested)
            {
                
                RasieOnMsgReceived(GenerateFrameData());
                //foreach (var signal in _signalStore.ParseMsgsYield(frames, _signalStore.Signals))
                //{
                //    if (signal != null)
                //        logService.Info($"{signal.Name}:{signal.RealValue}");
                //}

                //foreach (var item in _signalStore.Signals)
                //{

                //    if (item is Models.DiscreteSignal)
                //    {
                //        item.RealValue = item.RealValue == 0 ? 1 : 0;
                //    }
                //    else
                //    {
                //        item.RealValue = random.NextDouble() * 100;
                //    }
                //}

                Thread.Sleep(100);
            }
        }
        //private List<IFrame> virtualFrames;
        private List<IFrame> GenerateFrames()
        {
            List<IFrame> virtualFrames = new List<IFrame>();

            var inSignals = _signalStore.GetSignals<Models.SignalBase>()
                                     .Where(x => !x.InOrOut).ToList();

            var msgIDs = _signalStore.GetSignals<Models.SignalBase>()
                                     .Where(x=> !x.InOrOut)
                                     .Select(x => x.MessageID)
                                     .Distinct();
            foreach (var msgID in msgIDs)
            {
                virtualFrames.Add(new CanFrame(msgID, new byte[64]));
            }

            return virtualFrames;
        }
        private List<IFrame> GenerateFrameData()
        {
            var frames = GenerateFrames();
            foreach (var frame in frames)
            {
                GenerateFrameData(frame);
            }

            return frames;
        }

        private void GenerateFrameData(IFrame frame)
        {
            for (int i = 0; i < 64; i++)
            {
                frame.Data[i] = (byte)random.Next(0xff);
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public bool Send(IFrame frame)
        {
            return true;
        }

        public bool SendMultip(IEnumerable<IFrame> multiples)
        {
            foreach (var frame in multiples)
            {
                logService.Debug($"{frame.MessageID:X} : {string.Join(" ", frame.Data.Select(x => x.ToString("X2")))}");
            }
            return true;
        }
    }
}
