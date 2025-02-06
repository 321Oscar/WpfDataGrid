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
    public delegate TViewModel CreateViewModel<TViewModel>() where TViewModel : ObservableObject;

    public delegate TViewModel CreateViewModel<TParameter, TViewModel>(TParameter parameter) where TViewModel : ObservableObject;

    public class MainViewModel : ObservableObject
    {
        private readonly NavigationStore _navigationStore;
        private readonly ModalNavigationStore _modalNavigationStore;
        private readonly DeviceStore deviceStore;
        private readonly LogService logService;
        private string log;

        public ObservableObject CurrentViewModel => _navigationStore.CurrentViewModel;
        public ObservableObject CurrentModalViewModel => _modalNavigationStore.CurrentViewModel;
        public bool IsOpen => _modalNavigationStore.IsOpen;
        public IEnumerable<IDevice> Devices => deviceStore.Devices;
        public IDevice CurrentDevice
        {
            get
            {
                return deviceStore.CurrentDevice;
            }
            set
            {
                deviceStore.CurrentDevice = value;
                logService.Debug($"Change Device: {deviceStore.CurrentDevice}");
            }
        }

        public bool HasDevice => deviceStore.HasDevice;

        public string Log { get => log; set => SetProperty(ref log , value); }

        public MainViewModel(NavigationStore navigationStore, ModalNavigationStore modalNavigationStore,
            DeviceStore deviceStore, LogService logService)
        {
            this.logService = logService;
            this.logService.LogAdded += LogService_LogAdded;
            _navigationStore = navigationStore;
            _modalNavigationStore = modalNavigationStore;
            this.deviceStore = deviceStore;
            _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;
            _modalNavigationStore.CurrentViewModelChanged += OnCurrentModalViewModelChanged;
            this.deviceStore.CurrentDeviceChanged += OnCurrentDeviceChanged;
            StartCommand = new RelayCommand(() =>
            {
                //start receive msg from Device 
                CurrentDevice?.Start();
                logService.Debug($"Start");
            });
            StopCommand = new RelayCommand(() =>
            {
                //start receive msg from Device 
                CurrentDevice?.Stop();
                logService.Debug($"Stop");
            });
        }

        private void LogService_LogAdded(string obj)
        {
            Log = obj;
        }

        private void OnCurrentDeviceChanged()
        {
            OnPropertyChanged(nameof(HasDevice));
        }

        private void OnCurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentViewModel));
        }

        private void OnCurrentModalViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentModalViewModel));
            OnPropertyChanged(nameof(IsOpen));
        }

        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }
    }
}
