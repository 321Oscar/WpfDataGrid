using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERad5TestGUI.Services;
using ERad5TestGUI.Stores;

namespace ERad5TestGUI.ViewModels
{
    /// <summary>
    /// No Mvvm
    /// </summary>
    public class Main2ViewModel : MainViewModel
    {
        public Main2ViewModel(DeviceStore deviceStore, LogService logService, SignalStore signalStore) : base(deviceStore, logService, signalStore)
        {
            InitViewModel();
        }

        public AnalogViewModel AnalogViewModel { get; private set; }
        public DiscreteViewModel DiscreteViewModel { get; private set; }
        public PulseInViewModel PulseInViewModel { get; private set; }
        public PulseOutViewModel PulseOutViewModel { get; private set; }
        public NXPViewModel NXPViewModel { get; private set; }
        public GDICViewModel GDICViewModel { get; private set; }
        public MemoryViewModel MemoryViewModel { get; private set; }
        public ResolverViewModel ResolverViewModel { get; private set; }
        public LinViewModel LinViewModel { get; private set; }
        public DisConnectViewModel DisConnectViewModel { get; private set; }
        public SPIViewModel SPIViewModel { get; private set; }
        public PPAWLViewModel PPAWLViewModel { get; private set; }
        public ELockerViewModel ELockerViewModel { get; private set; }
        public SafingLogicViewModel SafingLogicViewModel { get; private set; }

        protected override void DeivceConfig()
        {
            //base.DeivceConfig();
            Views.DialogView dialogView = new Views.DialogView()
            {
                Width = 300,
                Height = 240
            };
            DeviceWithWindowViewModel deviceViewModel = new DeviceWithWindowViewModel(_deviceStore, _logService, dialogView);
            dialogView.DialogViewModel = deviceViewModel;
            dialogView.ShowDialog();
        }
        private IDialogService _dialogService;
        private void InitViewModel()
        {
            _dialogService = new DialogService();

            AnalogViewModel = new AnalogViewModel(SignalStore, _deviceStore, _logService);
            //AnalogViewModel.Init();
            DiscreteViewModel = new DiscreteViewModel(SignalStore, _deviceStore, _logService);
           // DiscreteViewModel.Init();
            PulseInViewModel = new PulseInViewModel(SignalStore, _deviceStore, _logService);
            //PulseInViewModel.Init();
            PulseOutViewModel = new PulseOutViewModel(SignalStore, _deviceStore, _logService);
            //PulseOutViewModel.Init();
            NXPViewModel = new NXPViewModel(SignalStore, _deviceStore, _logService);
           // NXPViewModel.Init();
            GDICViewModel = new GDICViewModel(SignalStore, _deviceStore, _logService);
           // GDICViewModel.Init();
            ResolverViewModel = new ResolverViewModel(SignalStore, _deviceStore, _logService);
           // ResolverViewModel.Init();
            LinViewModel = new LinViewModel(SignalStore, _deviceStore, _logService);
            //LinViewModel.Init();
            DisConnectViewModel = new DisConnectViewModel(SignalStore, _deviceStore, _logService);
            SPIViewModel = new SPIViewModel(SignalStore, _deviceStore, _logService);
            PPAWLViewModel = new PPAWLViewModel(SignalStore, _deviceStore, _logService);
            ELockerViewModel = new ELockerViewModel(SignalStore, _deviceStore, _logService);
            SafingLogicViewModel = new SafingLogicViewModel(SignalStore, _deviceStore, _logService, _dialogService);
            //DisConnectViewModel.Init(); 
            MemoryViewModel = new MemoryViewModel(SignalStore, _deviceStore, _logService);
           // MemoryViewModel.Init();
        }

        public void Dispose()
        {
            this.SignalStore.Dispose();
        }
    }
}
