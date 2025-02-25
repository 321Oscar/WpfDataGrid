using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using WpfApp1.Devices;
using WpfApp1.Models;
using WpfApp1.Services;
using WpfApp1.Stores;

namespace WpfApp1.ViewModels
{
    public class ViewModelBase : ObservableRecipient,IDisposable
    {
        private bool _isLoading;
        private RelayCommand _locatorSignalsCommand;
        public ModalNavigationStore ModalNavigationStore { get; }
        public  IServiceProvider ServiceProvider { get; }
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
        public ICommand LocatorSignalsCommand { get => _locatorSignalsCommand ?? (_locatorSignalsCommand = new RelayCommand(LocatorSignals)); }
        /// <summary>
        /// MVVM Mode with DI
        /// </summary>
        /// <param name="signalStore"></param>
        /// <param name="deviceStore"></param>
        /// <param name="logService"></param>
        /// <param name="modalNavigationStore"></param>
        /// <param name="serviceProvider"></param>
        public ViewModelBase(SignalStore signalStore, DeviceStore deviceStore, LogService logService, ModalNavigationStore modalNavigationStore, IServiceProvider serviceProvider)
            : this(signalStore, deviceStore, logService)
        {
            ModalNavigationStore = modalNavigationStore;
            ServiceProvider = serviceProvider;

            //DeviceStore.BeforeCurrentDeviceChange += DeviceStore_CurrentDeviceChange;
            //DeviceStore.CurrentDeviceChanged += DeviceStore_CurrentDeviceChanged;

        }
        /// <summary>
        /// No DI
        /// </summary>
        /// <param name="signalStore"></param>
        /// <param name="deviceStore"></param>
        /// <param name="logService"></param>
        public ViewModelBase(SignalStore signalStore, DeviceStore deviceStore, LogService logService)
        {
            SignalStore = signalStore;
            DeviceStore = deviceStore;
            LogService = logService;
        }
        /// <summary>
        /// In No DI, must call it 
        /// </summary>
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
            this.Dispose();
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

        public virtual void Dispose()
        {
            //throw new NotImplementedException();
        }
        /// <summary>
        /// ModalNavigationService SignalLocatorViewModel
        /// </summary>
        public virtual void LocatorSignals()
        {
            //ModalNavigationService<AnalogSignalLocatorViewModel> modalNavigationService =
            //     new ModalNavigationService<AnalogSignalLocatorViewModel>(
            //         this.ModalNavigationStore,
            //         () => new AnalogSignalLocatorViewModel(new CloseModalNavigationService(ModalNavigationStore), _signals, SignalStore,
            //         (signal) =>
            //         {
                 
            //         }));
            //modalNavigationService.Navigate();
        }

        public void Dispatch(Action action)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, action);
        }
    }

    public class SendFrameViewModelBase : ViewModelBase
    {
        [Obsolete("SignalStore.BuildFrames()")]
        protected DBCSignalBuildHelper BuildFramesHelper;
        public SendFrameViewModelBase(SignalStore signalStore, DeviceStore deviceStore, LogService logService,
            ModalNavigationStore modalNavigationStore, IServiceProvider serviceProvider) 
            : base(signalStore, deviceStore, logService, modalNavigationStore, serviceProvider)
        {
            
        }

        public SendFrameViewModelBase(SignalStore signalStore, DeviceStore deviceStore, LogService logService) : base(signalStore, deviceStore, logService) { }

        public virtual void Send(IEnumerable<IFrame> frames)
        {
            if (DeviceStore.HasDevice)
            {
                DeviceStore.CurrentDevice.SendMultip(frames);
            }
            else
            {
                LogService.Log("No CAN Device Connected!");
            }
        }

        public virtual void Send()
        {
            //if (DeviceStore.HasDevice)
            //{
            //    DeviceStore.CurrentDevice.SendMultip(SignalStore.BuildFrames());
            //}
        }
    }
}
