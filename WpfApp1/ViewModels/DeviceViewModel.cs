using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WpfApp1.Devices;
using WpfApp1.Services;
using WpfApp1.Stores;

namespace WpfApp1.ViewModels
{
    public class DeviceViewModel : ObservableRecipient
    {
        private readonly Stores.DeviceStore deviceStore;
        private readonly LogService logService;
        private readonly INavigationService navigationService;
        private IDevice currentDevice;
        private DeviceHardWareType hardWareType;

        public DeviceViewModel(DeviceStore deviceStore, LogService logService, INavigationService navigationService)
        {
            this.deviceStore = deviceStore;
            this.logService = logService;
            this.navigationService = navigationService;

            SaveConfigCommand = new RelayCommand(SaveConfig, () => CurrentDevice != null);
            CancelCommand = new RelayCommand(Cancel);

            if (deviceStore.HasDevice)
            {
                HardWareType = (deviceStore.CurrentDevice is VirtualDevice) ? DeviceHardWareType.Virtual : DeviceHardWareType.Vector;
                CurrentDevice = deviceStore.CurrentDevice;
            }
        }
        public Devices.DeviceHardWareType HardWareType 
        { 
            get => hardWareType;
            set 
            { 
                hardWareType = value; 
                //CurrentDevice = null; 
            }
        }
        public IEnumerable<IDevice> VectorDevices => deviceStore.GetDevices<VectorCan>();
        public IEnumerable<IDevice> VirtualDevices => deviceStore.GetDevices<VirtualDevice>();
        public IDevice CurrentDevice
        {
            get
            {
                return currentDevice;
                //return deviceStore.CurrentDevice;
            }
            set
            {
                SetProperty(ref currentDevice, value);
                (SaveConfigCommand as IRelayCommand).NotifyCanExecuteChanged();
            }
        }

        public ICommand SaveConfigCommand { get; }
        private void SaveConfig()
        {
            deviceStore.CurrentDevice = currentDevice;
            deviceStore.CurrentDevice.Open();
            deviceStore.CurrentDevice.Start();
            //open and start receive

            navigationService.Navigate();
        }

        public ICommand CancelCommand { get; }
        private void Cancel()
        {
            navigationService.Navigate();
        }
    }
}
