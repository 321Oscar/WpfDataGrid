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
        public DeviceStore(SignalStore signalStore, Services.LogService logService)
        {
            _signalStore = signalStore;
            this.logService = logService;
            devices = new ObservableCollection<IDevice>();

            LoadVirtualDevice();
            LoadVectorDevices();

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

        public IEnumerable<TDevice> GetDevices<TDevice>() where TDevice : IDevice
        {
            return devices.OfType<TDevice>();
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

        private void OnCurrentDeviceChanged()
        {
            if (HasDevice)
                logService.Debug($"Change Device: {CurrentDevice.Name}");
            CurrentDeviceChanged?.Invoke();
        }
        private void OnCurrentDeviceChange(IDevice device)
        {
            BeforeCurrentDeviceChange?.Invoke(device);
        }

        public event Action CurrentDeviceChanged;
        public event Action<IDevice> BeforeCurrentDeviceChange;
    }
}
