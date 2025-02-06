using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.Stores;

namespace WpfApp1.ViewModels
{
    public class PulseOutViewModel : ObservableRecipient
    {
        private readonly SignalStore _signalStore;

        public PulseOutViewModel(SignalStore signalStore)
        {
            _signalStore = signalStore;
        }
    }
}
