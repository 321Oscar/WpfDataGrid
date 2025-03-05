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
        private string log;
       
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
        public MainViewModel(IServiceProvider serviceProvider, NavigationStore navigationStore, ModalNavigationStore modalNavigationStore,
            DeviceStore deviceStore, LogService logService):this(deviceStore, logService)
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

        public MainViewModel(DeviceStore deviceStore, LogService logService)
        {
            this._logService = logService;
            this._logService.LogAdded += LogService_LogAdded;
            this._deviceStore = deviceStore;

            this._deviceStore.CurrentDeviceChanged += OnCurrentDeviceChanged;
            this._deviceStore.FrameCountChanged += OnFrameCountChanged;

            //OpenCommand = new RelayCommand(Open, () => HasDevice);
            //CloseCommand = new RelayCommand(Close, () => HasDevice);
            //StartCommand = new RelayCommand(Start, () => HasDevice);
            StopCommand = new RelayCommand(Stop, () => HasDevice);
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

        private void Stop() 
        {
            if (CurrentDevice.Started)
            {
                CurrentDevice?.Stop();
                _logService.Debug($"{CurrentDevice.Name} Stop Receive");
            }
            else
            {
                CurrentDevice.Start();
            }
            OnPropertyChanged(nameof(Started));
        }

        protected virtual void DeivceConfig()
        {
            ModalNavigationService<DeviceViewModel> modalNavigationService = new ModalNavigationService<DeviceViewModel>(this._modalNavigationStore,
                _serviceProvider.GetRequiredService<DeviceViewModel>);
            modalNavigationService.Navigate();
        }
    }

    public partial class MainViewModel
    {
        /// <summary>
        /// <see cref="Models.NXPInputSignal"/> 转换无需 * 5 /4096
        /// </summary>
        public string Version { get; } = "0.0.0.1";
    }
}
