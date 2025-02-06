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


    public class MemoryViewModel : ViewModelBase
    {
        public MemoryViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService) : base(signalStore, deviceStore, logService)
        {
        }
    }

    public class NXPViewModel : ViewModelBase
    {
        public NXPViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService) : base(signalStore, deviceStore, logService)
        {
        }
    }

    public class ResolverViewModel : ViewModelBase
    {
        public ResolverViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService) : base(signalStore, deviceStore, logService)
        {
        }
    }

    public class SPIViewModel : ViewModelBase
    {
        public SPIViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService) : base(signalStore, deviceStore, logService)
        {
        }
    }

    public class LinViewModel : ViewModelBase
    {
        public LinViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService) : base(signalStore, deviceStore, logService)
        {
        }
    }

    public class NXPFlashViewModel : ViewModelBase
    {
        public NXPFlashViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService) : base(signalStore, deviceStore, logService)
        {
        }
    }
}
