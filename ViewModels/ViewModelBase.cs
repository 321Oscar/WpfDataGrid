using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.Devices;
using WpfApp1.Services;
using WpfApp1.Stores;

namespace WpfApp1.ViewModels
{
    public class ViewModelBase : ObservableRecipient
    {
        public SignalStore SignalStore { get; }
        public DeviceStore DeviceStore { get; }
        public LogService LogService { get; }

        public ViewModelBase(SignalStore signalStore, DeviceStore deviceStore, LogService logService)
        {
            SignalStore = signalStore;
            DeviceStore = deviceStore;
            LogService = logService;

            DeviceStore.BeforeCurrentDeviceChange += DeviceStore_CurrentDeviceChange;
            DeviceStore.CurrentDeviceChanged += DeviceStore_CurrentDeviceChanged;
            
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            if (DeviceStore.HasDevice)
                DeviceStore.CurrentDevice.OnMsgReceived += CurrentDevice_OnMsgReceived;
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();
            if (DeviceStore.HasDevice)
            {
                DeviceStore.CurrentDevice.OnMsgReceived -= CurrentDevice_OnMsgReceived;
            }
        }


        private void DeviceStore_CurrentDeviceChange(IDevice device)
        {
            if (device != null)
            {
                device.OnMsgReceived -= CurrentDevice_OnMsgReceived;
            }
        }

        private void DeviceStore_CurrentDeviceChanged()
        {
            DeviceStore.CurrentDevice.OnMsgReceived += CurrentDevice_OnMsgReceived;
        }

        protected virtual void CurrentDevice_OnMsgReceived(IEnumerable<IFrame> frames)
        {
           

        }
    }
}
