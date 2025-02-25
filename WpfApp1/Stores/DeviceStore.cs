using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WpfApp1.Devices;
using WpfApp1.Services;

namespace WpfApp1.Stores
{
    public class DeviceStore
    {
        private readonly SignalStore _signalStore;
        private readonly LogService logService;
        private VectorCanService vectorCanService;
        private IDevice currentDevice;
        private ObservableCollection<IDevice> devices;
        private int framesCount;

        public event Action CurrentDeviceChanged;
        public event Action FrameCountChanged;
        public event Action<IDevice> BeforeCurrentDeviceChange;

        public DeviceStore(SignalStore signalStore, Services.LogService logService)
        {
            _signalStore = signalStore;
            this.logService = logService;
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
                CurrentDevice.OnMsgReceived += CurrentDevice_OnMsgReceived;
            }

            CurrentDeviceChanged?.Invoke();
        }

        private void CurrentDevice_OnMsgReceived(IEnumerable<IFrame> can_msg)
        {
            FramesCount += can_msg.Count();
            foreach (var item in _signalStore.ParseMsgsYield(can_msg))
            {
                if (item != null)
                {
                    logService.Log($"{item}", item.GetType());
                }

            }
        }

        private void OnCurrentDeviceChange(IDevice device)
        {
            if (device != null)
            {
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
