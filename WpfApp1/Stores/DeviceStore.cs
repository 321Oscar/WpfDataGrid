using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ERad5TestGUI.Devices;
using ERad5TestGUI.Services;

namespace ERad5TestGUI.Stores
{
    public class DeviceStore
    {
        private readonly SignalStore _signalStore;
        private readonly LogService logService;
        private VectorCanService vectorCanService;
        private IDevice currentDevice;
        private ObservableCollection<IDevice> devices;
        private int framesCount;
        private readonly log4net.ILog _logger;

        public event Action CurrentDeviceChanged;
        public event Action FrameCountChanged;
        public event Action<IDevice> BeforeCurrentDeviceChange;

        public DeviceStore(SignalStore signalStore, Services.LogService logService)
        {
            _signalStore = signalStore;
            this.logService = logService;
            _logger = this.logService.GetLogger();
            devices = new ObservableCollection<IDevice>();

            LoadVirtualDevice();
            LoadVectorDevices();

        }

        public IEnumerable<IDevice> Devices => devices;

        public IDevice CurrentDevice
        {
            get { return currentDevice; }
            set
            {
                OnCurrentDeviceChange(currentDevice);
                currentDevice = value;
                OnCurrentDeviceChanged();
            }
        }

        public bool HasDevice { get => CurrentDevice != null; }
        public int FramesCount
        {
            get => framesCount;
            set
            {
                framesCount = value;
                OnFrameCountChanged();
            }
        }

        public IEnumerable<TDevice> GetDevices<TDevice>() where TDevice : IDevice
        {
            return devices.OfType<TDevice>();
        }

        private void LoadVectorDevices()
        {
            //throw new NotImplementedException();
            vectorCanService = new VectorCanService(logService);
            foreach (var device in vectorCanService.VectorChannels)
            {
                devices.Add(device);
            }
        }

        private void LoadVirtualDevice()
        {
            devices.Add(new VirtualDevice(_signalStore, logService)
            {
                Name = "Virtual Device",
                //Description = "This is a virtual device"
            });
        }

       
        private void OnCurrentDeviceChanged()
        {
            if (HasDevice)
            {
                logService.Debug($"Change Device: {CurrentDevice.Name}");
                CurrentDevice.OnIFramesReceived += CurrentDevice_OnIFramesReceived;
                CurrentDevice.OnMsgReceived += CurrentDevice_OnMsgReceived;
            }

            CurrentDeviceChanged?.Invoke();
        }

        private void CurrentDevice_OnMsgReceived(uint id, byte[] data, int dlc)
        {
            //Task.Run(() =>
            //{
            //    FramesCount++;

            var canFrames = new CanFrame[] { new CanFrame(id, data, dlc: dlc) };

            //    foreach (var item in _signalStore.ParseMsgsYield(canFrames))
            //    {
            //        if (item != null)
            //        {
            //            logService.Log($"{item.GetValue()}", item.GetType());
            //        }
            //    }
            //    _signalStore.MessagesStates.ForEach(x => x.UpdateReceiveTime(canFrames.Select(msg => msg.MessageID)));

            //    OnMsgReceived?.Invoke(canFrames);
            //});
            //throw new NotImplementedException();
            OnMsgReceived?.Invoke(canFrames);
        }
        private bool _signalLog = false;
        public bool SignalLogEnable { get => _signalLog; set => _signalLog = value; }
        private void CurrentDevice_OnIFramesReceived(IEnumerable<IFrame> can_msgs)
        {
            FramesCount += can_msgs.Count();
            foreach (var item in _signalStore.ParseMsgsYield(can_msgs))
            {
                if (item != null)
                {
                    //不记录 最快
                    if (_signalLog)
                    {
                        Task.Run(() => _signalStore.LogSignal($"{item.GetValue()}", item.GetType()));
                    }
                    //_signalStore.LogSignal($"{item.GetValue()}", item.GetType()); //最慢，判断信号类型

                    //logService.Log($"{item.GetValue()}", item.GetType());
                }
            }
            _signalStore.MessagesStates.ForEach(x => x.UpdateReceiveTime(can_msgs.Select(msg => msg.MessageID)));
            
        }

        public event OnIFrameReceived OnMsgReceived;

        private void OnCurrentDeviceChange(IDevice device)
        {
            if (device != null)
            {
                device.OnIFramesReceived -= CurrentDevice_OnIFramesReceived;
                device.OnMsgReceived -= CurrentDevice_OnMsgReceived;
                device.Close();
            }
            BeforeCurrentDeviceChange?.Invoke(device);
        }

        private void OnFrameCountChanged()
        {
            FrameCountChanged?.Invoke();
        }

      
    }
}
