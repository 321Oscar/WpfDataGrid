﻿using System;
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
    public class VirtualDevice : IDevice
    {
        private readonly SignalStore _signalStore;
        private readonly LogService logService;
        private readonly Random random;
        private bool isOpen;
        private bool isStart;

        public VirtualDevice(SignalStore signalStore,Services.LogService logService)
        {
            Name = "Virtual Device";
            _signalStore = signalStore;
            this.logService = logService;
            random = new Random();
        }

        //private Thread _receiveThread;
        private Task _receiceTask;
        private CancellationTokenSource tokenSource;

        public event OnMsgReceived OnMsgReceived;

        public string Name { get; set; }
        public bool IsStart { get { return isOpen && isStart; } }
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
            if (IsStart)
                return;
            //_receiveThread = new Thread(new ThreadStart(() => Receive()));
            //_receiveThread.IsBackground = true;
            //_receiveThread.Start();
            tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            isStart = true;
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
        }

        private void RasieOnMsgReceived(IEnumerable<IFrame> frames)
        {
            OnMsgReceived?.Invoke(frames);
        }
        private void Receive()
        {
            tokenSource.Token.ThrowIfCancellationRequested();

            while (true && !tokenSource.Token.IsCancellationRequested)
            {
                CanFrame frame = new CanFrame()
                {
                    MessageID = 0x6f8,
                    Data = new byte[64]
                };
                for (int i = 0; i < 64; i++)
                {
                    frame.Data[i] = (byte)random.Next(0xff);
                }
                CanFrame frame101 = new CanFrame()
                {
                    MessageID = 0x101,
                    Data = new byte[64]
                };
                for (int i = 0; i < 64; i++)
                {
                    frame101.Data[i] = (byte)random.Next(0xff);
                }
                List<CanFrame> frames = new List<CanFrame>()
                {
                   frame,frame101
                };
                RasieOnMsgReceived(frames);
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
