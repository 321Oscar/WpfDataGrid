using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.Services;
using WpfApp1.Stores;

namespace WpfApp1.ViewModels
{
    /// <summary>
    /// No Mvvm
    /// </summary>
    public class Main2ViewModel : MainViewModel
    {
        private readonly SignalStore signalStore;

        public Main2ViewModel(DeviceStore deviceStore, LogService logService, SignalStore signalStore) : base(deviceStore, logService)
        {
            this.signalStore = signalStore;

            InitViewModel();
        }

        public AnalogViewModel AnalogViewModel { get; private set; }
        public DiscreteViewModel DiscreteViewModel { get; private set; }
        public PulseInViewModel PulseInViewModel { get; private set; }
        public PulseOutViewModel PulseOutViewModel { get; private set; }
        public NXPViewModel NXPViewModel { get; private set; }
        public GDICViewModel GDICViewModel { get; private set; }

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

        private void InitViewModel()
        {
            AnalogViewModel = new AnalogViewModel(signalStore, _deviceStore, _logService);
            AnalogViewModel.Init();
            DiscreteViewModel = new DiscreteViewModel(signalStore, _deviceStore, _logService);
            DiscreteViewModel.Init();
            PulseInViewModel = new PulseInViewModel(signalStore, _deviceStore, _logService);
            PulseInViewModel.Init();
            PulseOutViewModel = new PulseOutViewModel(signalStore, _deviceStore, _logService);
            PulseOutViewModel.Init();
            NXPViewModel = new NXPViewModel(signalStore, _deviceStore, _logService);
            NXPViewModel.Init();
            GDICViewModel = new GDICViewModel(signalStore, _deviceStore, _logService);
            GDICViewModel.Init();
        }
    }
}
