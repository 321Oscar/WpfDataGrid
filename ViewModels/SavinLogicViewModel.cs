using System.Collections.Generic;
using WpfApp1.Models;
using WpfApp1.Services;
using WpfApp1.Stores;

namespace WpfApp1.ViewModels
{
    public class SavinLogicViewModel : ViewModelBase
    {
        public SavinLogicViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService) 
            : base(signalStore, deviceStore, logService)
        {
        }

        public IEnumerable<SavingLogicSignal> SavingLogicSignals => SignalStore.GetObservableCollection<SavingLogicSignal>();
    }
}
