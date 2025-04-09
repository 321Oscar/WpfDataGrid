using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ERad5TestGUI.Devices;
using ERad5TestGUI.Services;
using ERad5TestGUI.Stores;
using ERad5TestGUI.Models;

namespace ERad5TestGUI.ViewModels
{
    public delegate TViewModel CreateViewModel<TViewModel>() where TViewModel : ObservableObject;

    public delegate TViewModel CreateViewModel<TParameter, TViewModel>(TParameter parameter) where TViewModel : ObservableObject;

    public partial class MainViewModel : ObservableObject
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly NavigationStore _navigationStore;
        private readonly ModalNavigationStore _modalNavigationStore;
        protected readonly DeviceStore _deviceStore;
        protected readonly LogService _logService;
        private readonly SignalStore _signalStore;
        private string log;
        private RelayCommand _disableINHCANCommand;
        private RelayCommand<uint> _sendCANFDWakeUpCommand;
       
        public ObservableObject CurrentViewModel => _navigationStore.CurrentViewModel;
        public ObservableObject CurrentModalViewModel => _modalNavigationStore.CurrentViewModel;
        public bool IsOpen => _modalNavigationStore.IsOpen;
        public IEnumerable<IDevice> Devices => _deviceStore.Devices;
        public IDevice CurrentDevice
        {
            get
            {
                return _deviceStore.CurrentDevice;
            }
            set
            {
                _deviceStore.CurrentDevice = value;
                _logService.Debug($"Change Device: {_deviceStore.CurrentDevice.Name}");
            }
        }

        public bool HasDevice => _deviceStore.HasDevice;
        public bool Started => HasDevice && _deviceStore.CurrentDevice.Started;
        public string Log { get => log; set => SetProperty(ref log , value); }
        public int FramesCount { get => _deviceStore.FramesCount; }
        public SignalStore SignalStore { get => _signalStore; }
        public DiscreteOutputSignal FD5_INH_DISABLE => SignalStore.GetSignals<DiscreteOutputSignal>().FirstOrDefault(x => x.Name == "FD5_INH_DISABLE");
        public DiscreteOutputSignal FD16_INH_DISABLE => SignalStore.GetSignals<DiscreteOutputSignal>().FirstOrDefault(x => x.Name == "FD16_INH_DISABLE");
        public DiscreteOutputSignal CAN_INH_Update => SignalStore.GetSignalByName<DiscreteOutputSignal>("CAN_INH_Update", inOrOut: true);
        public MainViewModel(IServiceProvider serviceProvider, NavigationStore navigationStore, ModalNavigationStore modalNavigationStore,
            DeviceStore deviceStore, LogService logService, SignalStore signalStore) 
            : this(deviceStore, logService, signalStore)
        {
           
            this._serviceProvider = serviceProvider;
            _navigationStore = navigationStore;
            _modalNavigationStore = modalNavigationStore;
            
            if (_navigationStore != null)
            {
                _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;
                _modalNavigationStore.CurrentViewModelChanged += OnCurrentModalViewModelChanged;
            }
        }

        public MainViewModel(DeviceStore deviceStore, LogService logService, SignalStore signalStore)
        {
            this._logService = logService;
            this._logService.LogAdded += LogService_LogAdded;
            this._deviceStore = deviceStore;

            this._deviceStore.CurrentDeviceChanged += OnCurrentDeviceChanged;
            this._deviceStore.FrameCountChanged += OnFrameCountChanged;
            _signalStore = signalStore;
            _udsVm = new UDSUpgradeViewModel(signalStore, deviceStore, logService);
            //OpenCommand = new RelayCommand(Open, () => HasDevice);
            //CloseCommand = new RelayCommand(Close, () => HasDevice);
            //StartCommand = new RelayCommand(Start, () => HasDevice);
            StopCommand = new AsyncRelayCommand(Stop, () => HasDevice);
            DeivceConfigCommand = new RelayCommand(DeivceConfig);
        }

        private void OnFrameCountChanged()
        {
            OnPropertyChanged(nameof(FramesCount));
        }

        private void LogService_LogAdded(string obj)
        {
            Log = obj;
        }

        private void OnCurrentDeviceChanged()
        {
            OnPropertyChanged(nameof(HasDevice));
            OnPropertyChanged(nameof(Started));
            OnPropertyChanged(nameof(CurrentDevice));
            //(OpenCommand as IRelayCommand).NotifyCanExecuteChanged();
            //(CloseCommand as IRelayCommand).NotifyCanExecuteChanged();
            //(StartCommand as IRelayCommand).NotifyCanExecuteChanged();
            (StopCommand as IRelayCommand).NotifyCanExecuteChanged();
            _disableINHCANCommand.NotifyCanExecuteChanged();
            _sendCANFDWakeUpCommand.NotifyCanExecuteChanged();
        }

        private void OnCurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentViewModel));
            Console.WriteLine($"{DateTime.Now:HH:mm:ss fff} view changed");
        }

        private void OnCurrentModalViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentModalViewModel));
            OnPropertyChanged(nameof(IsOpen));
        }

        public ICommand OpenCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand DeivceConfigCommand { get; }
        public ICommand DisableINHCANCommand { get => _disableINHCANCommand ?? (_disableINHCANCommand = new RelayCommand(DisableINHCAN, () => HasDevice)); }
        public ICommand SendWakeUpCommand { get => _sendCANFDWakeUpCommand ?? (_sendCANFDWakeUpCommand = new RelayCommand<uint>(SendWakeUp, (x) => HasDevice)); }

        private async Task Stop() 
        {
            if (CurrentDevice.Started)
            {
                CurrentDevice?.Stop();
                _logService.Debug($"{CurrentDevice.Name} Stop Receive");
            }
            else
            {
                CurrentDevice.Start();
                if (CurrentDevice is VectorCan)
                {
                    HardwareID = await ReadDID(DIDF193);
                    EMSWVersion = await ReadDID(DIDF130);
                }
                else
                {
                    HardwareID = "x.x.x.x";
                    EMSWVersion = "x.x.x.x";
                }
            }
            OnPropertyChanged(nameof(Started));
        }

        protected virtual void DeivceConfig()
        {
            ModalNavigationService<DeviceViewModel> modalNavigationService = new ModalNavigationService<DeviceViewModel>(this._modalNavigationStore,
                _serviceProvider.GetRequiredService<DeviceViewModel>);
            modalNavigationService.Navigate();
        }

        private void DisableINHCAN()
        {
            //FD5_INH_DISABLE.UpdateRealValue();
            //FD16_INH_DISABLE.UpdateRealValue();

            if (this.HasDevice)
            {
                CAN_INH_Update.OriginValue = 1;
                CurrentDevice.SendFDMultip(SignalStore.BuildFrames(new DiscreteOutputSignal[] {
                    CAN_INH_Update,
                    FD16_INH_DISABLE,
                    FD5_INH_DISABLE
                }));
                CAN_INH_Update.OriginValue = 0;
            }
            else
            {
                Log = "No Device Connected!";
            }
        }

        private void SendWakeUp(uint msgID)
        {
            _deviceStore.CurrentDevice.Send(new CanFrame(msgID, new byte[8], FrameFlags.CAN));
        }

        #region DID
        private UDSUpgradeViewModel _udsVm;
        private UDS.DIDInfo DIDF193 = new UDS.DIDInfo("hardware version", 0xF193, 10, UDS.DIDType.ASCII);
        private UDS.DIDInfo DIDF130 = new UDS.DIDInfo("software version", 0xF130, 10, UDS.DIDType.ASCII);
        private string _hardVersion;
        private string _softVersion;
        public string HardwareID { get => _hardVersion; set=>SetProperty(ref _hardVersion,value); }
        public string EMSWVersion { get => _softVersion; set=>SetProperty(ref _softVersion, value); }
        private async Task<string> ReadDID(UDS.DIDInfo did)
        {
            _udsVm.CurrentDID = did;

            var flags = await _udsVm.ReadDID();

            if(flags == 1)
            {
                return _udsVm.DIDData;
            }
            return "";
        }
        #endregion
    }

    public partial class MainViewModel
    {
        /// 0.0.1.0
        /// 重绘SafingLogic
        /// 优化Analog计算标准差
        /// 修改UDS诊断ID
        /// 0.0.0.9
        /// 1.PPAWL 中 Duty Freq下发失败
        /// 2.PulseOut中 Freq设置后会自行变化
        /// 3.SafingLogic增加输入输出可变信号
        /// 4.增加读取DIDF193 & F130
        /// 0.0.0.8
        /// 关闭软件时，取消MsgStatus后台线程
        /// Disable Can 需绑定信号
        /// safinglogic测试进度改为百分制；
        /// dbc文件增加解析信号长名称
        /// 0.0.0.7：
        /// 1.GDIC_Aout 右侧删除Temperature
        /// 2.DisConnect 增加后台循环发送can2.0报文
        /// 3.safinglogic测试结果通过弹窗显示，测试失败则弹出是否保存excel,选择保存后再导出excel
        /// 4.修改Frame结构，修复发送can2.0报文失败

        /// <summary>
        /// Soft Version
        /// </summary>
        /// <remarks>
        /// <para>V0.0.0.6 safinglogic Test 
        /// </para>
        /// <para>V0.0.0.5 1.完善PPAWL，E-Locker界面
        ///2.修改GDIC ADC信号显示
        ///3.增加SafingLogic，Memory界面
        ///4.各个界面增加清空数据按钮</para>
        /// <para>V0.0.0.4 1.新增PPAWL，E-Locker界面
        ///2.修改NXP,PulseIn,Analog,Discrete等界面中显示的信号
        ///3.增加Enabel DevCan，以及DevCAN,CANFD实时状态</para>
        /// <para>V0.0.0.3 增加SPIView <see cref="Models.SPISignal"/>，DisConnect,Resolver界面
        /// </para>
        /// <para>V0.0.0.2 : <see cref="Models.AnalogSignal"/> 第二次转换无需 * 5 /4096;
        /// 增加NXP界面信号；
        /// 增加Disable CAN INH功能；
        /// 增加Discrete界面中的控制;
        /// 发送CAN报文，根据ID发送该ID下的所有信号</para>
        /// <para>V0.0.0.1 : <see cref="Models.NXPInputSignal"/> 转换无需 * 5 /4096；增加LIN 界面</para>
        /// </remarks>
        public string Version { get; } = "0.0.1.0";
    }
}
