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
using ERad5TestGUI.Devices;
using ERad5TestGUI.Models;
using ERad5TestGUI.Services;
using ERad5TestGUI.Stores;

namespace ERad5TestGUI.ViewModels
{
    public class ViewModelBase : ObservableRecipient,IDisposable
    {
        private bool _isLoading;
        private RelayCommand _locatorSignalsCommand;
        private readonly log4net.ILog logger;
        /// <summary>
        /// 特定的ViewName/PageName
        /// </summary>
        protected string _ViewName;

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

#if DEBUG
        public bool DebugMode { get => true; }
#else
        public bool DebugMode { get => false; }
#endif
        public string ViewName { get => _ViewName ?? SignalBase.ReplaceViewModel(this.GetType().Name); }
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
            logger = LogService.GetLogger(this.GetType().Name);
            Init();
        }
        /// <summary>
        /// In No DI, must call it 
        /// </summary>
        public virtual void Init()
        {
            
        }

        public void Log(string msg)
        {
            logger.Info(msg);
            LogService.Log(msg);
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
                device.OnIFramesReceived -= CurrentDevice_OnMsgReceived;
            }
        }
        [Obsolete]
        private void DeviceStore_CurrentDeviceChanged()
        {
            if (DeviceStore.HasDevice)
                DeviceStore.CurrentDevice.OnIFramesReceived += CurrentDevice_OnMsgReceived;
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

        protected void ShowMsgInfoBox(string text, string caption)
        {
            MessageBox.Show(text, caption, MessageBoxButton.OK, MessageBoxImage.Information);
            //AdonisUI.Controls.MessageBox.Show(text, caption: caption,
            //            icon: AdonisUI.Controls.MessageBoxImage.Information);
        }

        protected void ShowMsgErrorBox(string text, string caption)
        {
            MessageBox.Show(text, caption, MessageBoxButton.OK, MessageBoxImage.Error);
            //AdonisUI.Controls.MessageBox.Show(text, caption: caption,
            //            icon: AdonisUI.Controls.MessageBoxImage.Error);
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

        public virtual void SendFD(IEnumerable<IFrame> frames)
        {
            if (DeviceStore.HasDevice)
            {
                DeviceStore.CurrentDevice.SendFDMultip(frames);
            }
            else
            {
                LogService.Log("No CAN Device Connected!");
            }
        }

        protected void SendFDNoExp(IEnumerable<IFrame> frames)
        {
            if (DeviceStore.HasDevice)
            {
                foreach (var frame in frames)
                {
                    DeviceStore.CurrentDevice.Send(frame);
                }
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
