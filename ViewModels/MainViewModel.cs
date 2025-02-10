using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
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
    public delegate TViewModel CreateViewModel<TViewModel>() where TViewModel : ObservableObject;

    public delegate TViewModel CreateViewModel<TParameter, TViewModel>(TParameter parameter) where TViewModel : ObservableObject;

    public class MainViewModel : ObservableObject
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly NavigationStore _navigationStore;
        private readonly ModalNavigationStore _modalNavigationStore;
        private readonly DeviceStore _deviceStore;
        private readonly LogService _logService;
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

        public string Log { get => log; set => SetProperty(ref log , value); }
        public int FramesCount { get => _deviceStore.FramesCount; }
        public MainViewModel(IServiceProvider serviceProvider, NavigationStore navigationStore, ModalNavigationStore modalNavigationStore,
            DeviceStore deviceStore, LogService logService)
        {
            this._logService = logService;
            this._logService.LogAdded += LogService_LogAdded;
            this._serviceProvider = serviceProvider;
            _navigationStore = navigationStore;
            _modalNavigationStore = modalNavigationStore;
            this._deviceStore = deviceStore;

            _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;
            _modalNavigationStore.CurrentViewModelChanged += OnCurrentModalViewModelChanged;
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
        
        private void Open()
        {
            CurrentDevice?.Open();
            _logService.Debug($"{CurrentDevice.Name} Open");
        }

        private void Close()
        {
            CurrentDevice?.Close();
            _logService.Debug($"{CurrentDevice.Name} Close");
        }

        private void Start() {
            CurrentDevice?.Start();
            _logService.Debug($"{CurrentDevice.Name} Start Receive");
        }
        private void Stop() {
            CurrentDevice?.Stop();
            _logService.Debug($"{CurrentDevice.Name} Stop Receive");
        }

        private void DeivceConfig()
        {
            ModalNavigationService<DeviceViewModel> modalNavigationService = new ModalNavigationService<DeviceViewModel>(this._modalNavigationStore,
                _serviceProvider.GetRequiredService<DeviceViewModel>);
            modalNavigationService.Navigate();
        }
    }
}
