using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.Services;
using WpfApp1.Stores;

namespace WpfApp1.ViewModels
{
    public class GDICViewModel : ViewModelBase
    {
        //private readonly SignalStore _signalStore;

        public GDICViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService)
            : base(signalStore, deviceStore, logService)
        {
            //_signalStore = signalStore;
        }
    }

    public class GDICStatusSignal : Models.SignalBase
    {

    }
}
