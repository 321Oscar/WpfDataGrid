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

        public DeviceViewModel(DeviceStore deviceStore, LogService logService)
        {
            this.deviceStore = deviceStore;
            this.logService = logService;

            SaveConfigCommand = new RelayCommand(SaveConfig, () => CurrentDevice != null);
            CancelCommand = new RelayCommand(Cancel);

            if (deviceStore.HasDevice)
            {
                HardWareType = (deviceStore.CurrentDevice is VirtualDevice) ? DeviceHardWareType.Virtual : DeviceHardWareType.Vector;
                CurrentDevice = deviceStore.CurrentDevice;
            }
        }
        /// <summary>
        /// MVVM Dialog
        /// </summary>
        /// <param name="deviceStore"></param>
        /// <param name="logService"></param>
        /// <param name="navigationService"></param>
        public DeviceViewModel(DeviceStore deviceStore, LogService logService, INavigationService navigationService)
            : this(deviceStore, logService)
        {
            this.navigationService = navigationService;
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
        protected virtual void SaveConfig()
        {
            deviceStore.CurrentDevice = currentDevice;
            deviceStore.CurrentDevice.Open();
            deviceStore.CurrentDevice.Start();
            //open and start receive
            
            navigationService?.Navigate();
        }

        public ICommand CancelCommand { get; }
        protected virtual void Cancel()
        {
            navigationService?.Navigate();
        }
    }

    public class DeviceWithWindowViewModel : DeviceViewModel, IDialogWindow
    {
        public System.Windows.Window Window { get; }

        public DeviceWithWindowViewModel(DeviceStore deviceStore, LogService logService, System.Windows.Window window)
            : base(deviceStore, logService)
        {
            Window = window;
        }
        public void Ok()
        {
            Window.DialogResult = true;
            Window.Close();
        }

        //public void Cancel()
        //{
           
        //}
        protected override void SaveConfig()
        {
            base.SaveConfig();
            Ok();
        }

        protected override void Cancel()
        {
            base.Cancel();
            Window.DialogResult = false;
            Window.Close();
        }
    }

    public interface IDialogWindow
    {
        System.Windows.Window Window { get; }
        void Ok();
        //void Cancel();
    }
}
