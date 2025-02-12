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
        private bool _isLoading;
        public SignalStore SignalStore { get; }
        public DeviceStore DeviceStore { get; }
        public LogService LogService { get; }

        public bool IsLoading
        {
            get => _isLoading;
            set 
            {
                Console.WriteLine($"{DateTime.Now:HH:mm:ss fff} Loading {value}");
                SetProperty(ref _isLoading, value);
            }
        }

        public ViewModelBase(SignalStore signalStore, DeviceStore deviceStore, LogService logService)
        {
            SignalStore = signalStore;
            DeviceStore = deviceStore;
            LogService = logService;

            //DeviceStore.BeforeCurrentDeviceChange += DeviceStore_CurrentDeviceChange;
            //DeviceStore.CurrentDeviceChanged += DeviceStore_CurrentDeviceChanged;
            
        }

        public virtual void Init()
        {
            
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            //if (DeviceStore.HasDevice)
            //    DeviceStore.CurrentDevice.OnMsgReceived += CurrentDevice_OnMsgReceived;
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();
            //if (DeviceStore.HasDevice)
            //{
            //    DeviceStore.CurrentDevice.OnMsgReceived -= CurrentDevice_OnMsgReceived;
            //}
        }

        [Obsolete]
        private void DeviceStore_CurrentDeviceChange(IDevice device)
        {
            if (device != null)
            {
                device.OnMsgReceived -= CurrentDevice_OnMsgReceived;
            }
        }
        [Obsolete]
        private void DeviceStore_CurrentDeviceChanged()
        {
            if (DeviceStore.HasDevice)
                DeviceStore.CurrentDevice.OnMsgReceived += CurrentDevice_OnMsgReceived;
        }
        /// <summary>
        /// 不在每个界面单独解析信号
        /// </summary>
        /// <param name="frames"></param>
        [Obsolete]
        protected virtual void CurrentDevice_OnMsgReceived(IEnumerable<IFrame> frames)
        {
           

        }
    }

    public class SendFrameViewModelBase : ViewModelBase
    {
        protected DBCSignalBuildHelper BuildFramesHelper;
        public SendFrameViewModelBase(SignalStore signalStore, DeviceStore deviceStore, LogService logService) 
            : base(signalStore, deviceStore, logService)
        {
            
        }

        public virtual void Send()
        {
            if (DeviceStore.HasDevice)
            {
                DeviceStore.CurrentDevice.SendMultip(BuildFramesHelper.BuildFrames());
            }
        }
    }
}
