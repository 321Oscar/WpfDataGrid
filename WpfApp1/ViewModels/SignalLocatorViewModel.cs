using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WpfApp1.Models;
using WpfApp1.Services;
using WpfApp1.Stores;

namespace WpfApp1.ViewModels
{
    public class SignalLocatorViewModel<TSignal> : ObservableRecipient
        where TSignal : class
    {
        private readonly INavigationService _navigationService;

        private RelayCommand addCommand;

        private Func<Stores.Signal, TSignal> _createSignal;
        private Action<ObservableCollection<TSignal>,TSignal> _addSignal;
        private ObservableCollection<TSignal> _tmpsignals = new ObservableCollection<TSignal>();
        private Signal currentDbcSignal;

        public SignalLocatorViewModel(INavigationService navigationService, ObservableCollection<TSignal> signals, Stores.SignalStore signalStore,
            Func<Stores.Signal, TSignal> createSignal, Action<ObservableCollection<TSignal>, TSignal> addSignal = null)
        {
            this._navigationService = navigationService;
            _createSignal = createSignal;
            _addSignal = addSignal;
            Signals = signals;
            SignalStore = signalStore;
            foreach (var item in Signals)
            {
                _tmpsignals.Add(item);
            }
            CancelCommand = new RelayCommand(Cancel);
            OkCommand = new RelayCommand(Ok);
            
        }
        public string Title { get; set; } = "Locator Signal To View";
        public Stores.SignalStore SignalStore { get; }
        public ObservableCollection<TSignal> Signals { get; }
        public ObservableCollection<TSignal> TempSignals { get => _tmpsignals; }
        public Stores.Signal CurrentDbcSignal 
        { 
            get => currentDbcSignal;
            set 
            {
                currentDbcSignal = value; 
                addCommand.NotifyCanExecuteChanged(); 
            }
        }
        public ICommand CancelCommand { get; }
        public ICommand OkCommand { get; }
        public ICommand AddCommand { get => addCommand ?? (addCommand = new RelayCommand(Add, () => (_createSignal != null && CurrentDbcSignal != null))); }
        private void Cancel()
        {
            _navigationService.Navigate();
        }

        private void Ok()
        {
            foreach (var item in TempSignals)
            {
                if (!Signals.Contains(item))
                    Signals.Add(item);
            }
            _navigationService.Navigate();
        }

        private void Add()
        {
            var signal = _createSignal(CurrentDbcSignal);
            if(_addSignal != null)
            {
                _addSignal(TempSignals,signal);
            }
            else
            {
                if (signal != null && !TempSignals.Contains(signal))
                    TempSignals.Add(signal);
            }
        }
    }
    /// <summary>
    /// 为了使用DateTemplate，class 不能有泛型
    /// </summary>
    public class AnalogSignalLocatorViewModel : SignalLocatorViewModel<AnalogSignal>
    {
        public AnalogSignalLocatorViewModel(INavigationService navigationService, 
            ObservableCollection<AnalogSignal> signals, 
            SignalStore signalStore, 
            Func<Signal, AnalogSignal> createSignal) 
            : base(navigationService, signals, signalStore, createSignal)
        {
        }
    }

    public class DiscreteInputSignalLocatorViewModel : SignalLocatorViewModel<DiscreteInputSignal>
    {
        public DiscreteInputSignalLocatorViewModel(
            INavigationService navigationService, 
            ObservableCollection<DiscreteInputSignal> signals, 
            SignalStore signalStore, 
            Func<Signal, DiscreteInputSignal> createSignal) 
            : base(navigationService, signals, signalStore, createSignal)
        {
        }
    }

    public class DiscreteOutputSignalLocatorViewModel : SignalLocatorViewModel<DiscreteOutputSignal>
    {
        public DiscreteOutputSignalLocatorViewModel(
            INavigationService navigationService,
            ObservableCollection<DiscreteOutputSignal> signals,
            SignalStore signalStore,
            Func<Signal, DiscreteOutputSignal> createSignal)
            : base(navigationService, signals, signalStore, createSignal)
        {
        }
    }

    public class PulseOutSignalLocatorViewModel : SignalLocatorViewModel<PulseGroupSignalOutGroup>
    {
        public PulseOutSignalLocatorViewModel(INavigationService navigationService, 
            ObservableCollection<PulseGroupSignalOutGroup> signals, 
            SignalStore signalStore, 
            Func<Signal, PulseGroupSignalOutGroup> createSignal,
            Action<ObservableCollection<PulseGroupSignalOutGroup>, PulseGroupSignalOutGroup> addSignal) 
            : base(navigationService, signals, signalStore, createSignal, addSignal)
        {
        }
    }
}
