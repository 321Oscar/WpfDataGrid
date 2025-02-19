using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfApp1.Models;
using WpfApp1.Services;
using WpfApp1.Stores;

namespace WpfApp1.ViewModels
{
    public class SignalLocatorViewModel<TSignal> : ObservableRecipient, IDialogWindow
        where TSignal : class
    {
        private readonly INavigationService _navigationService;

        private RelayCommand addCommand;
        //private RelayCommand<IEnumerable<Signal>> addMultipCommand;
        private RelayCommand<object> addMultipCommand;

        private Func<Stores.Signal, TSignal> _createSignal;
        private Action<ObservableCollection<TSignal>, TSignal> _addSignal;
        private ObservableCollection<TSignal> _tmpsignals = new ObservableCollection<TSignal>();
        private Signal currentDbcSignal;
        private TSignal currentTSignal;
        private RelayCommand delCommand;
        private string title = "Locator Signal To View";

        /// <summary>
        /// Without Mvvm Dialog
        /// </summary>
        /// <param name="signals"></param>
        /// <param name="signalStore"></param>
        /// <param name="createSignal"></param>
        /// <param name="window"></param>
        /// <param name="addSignal"></param>
        public SignalLocatorViewModel(ObservableCollection<TSignal> signals, Stores.SignalStore signalStore,
            Func<Stores.Signal, TSignal> createSignal, Window window, Action<ObservableCollection<TSignal>, TSignal> addSignal = null)
        {
            Title = typeof(TSignal).Name;
            Window = window;
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
        /// <summary>
        /// MVVM Dialog
        /// </summary>
        /// <param name="navigationService"></param>
        /// <param name="signals"></param>
        /// <param name="signalStore"></param>
        /// <param name="createSignal"></param>
        /// <param name="addSignal"></param>
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
        public string Title { get => title; set => SetProperty(ref title , value); }
        public Stores.SignalStore SignalStore { get; }
        public ObservableCollection<TSignal> Signals { get; }
        public ObservableCollection<TSignal> TempSignals { get => _tmpsignals; }
        public Stores.Signal CurrentDbcSignal
        {
            get => currentDbcSignal;
            set
            {
                currentDbcSignal = value;
                addCommand?.NotifyCanExecuteChanged();
            }
        }
        public TSignal CurrentTSignal
        {
            get => currentTSignal;
            set
            {
                currentTSignal = value;
                delCommand.NotifyCanExecuteChanged();
            }
        }

        public ICommand CancelCommand { get; }
        public ICommand OkCommand { get; }
        public ICommand AddCommand { get => addCommand ?? (addCommand = new RelayCommand(Add, () => (_createSignal != null && CurrentDbcSignal != null))); }
        public ICommand AddMultipCommand { get => addMultipCommand ?? (addMultipCommand = new RelayCommand<object>(Add)); }
        public ICommand DelCommand { get => delCommand ?? (delCommand = new RelayCommand(Del, () => CurrentTSignal != null)); }

        public Window Window { get; }
        private void Cancel()
        {
            _navigationService?.Navigate();
            Window?.Close();
        }

        public void Ok()
        {
            UpdateList(TempSignals, Signals);
            //foreach (var item in TempSignals)
            //{
            //    if (!Signals.Contains(item))
            //        Signals.Add(item);
            //}
            //Signals.Remove()
            _navigationService?.Navigate();
            Window?.Close();
        }

        private void Add()
        {
            var signal = _createSignal(CurrentDbcSignal);
            if (_addSignal != null)
            {
                _addSignal(TempSignals, signal);
            }
            else
            {
                if (signal != null && !TempSignals.Contains(signal))
                    TempSignals.Add(signal);
            }
        }
         private void Add(Signal dbcsignal)
        {
            var signal = _createSignal(dbcsignal);
            if (_addSignal != null)
            {
                _addSignal(TempSignals, signal);
            }
            else
            {
                if (signal != null && !TempSignals.Contains(signal))
                    TempSignals.Add(signal);
            }
        }

        private void Add(IEnumerable<Signal> selectedSignals)
        {
            foreach (var signal in selectedSignals)
            {
                Add(signal);
            }
        }

        private void Add(object obj)
        {
            var signals = (obj as IList<object>).Cast<Signal>();
            Add(signals);
        }

        private void Del()
        {
            TempSignals.Remove(CurrentTSignal);
        }

        private void UpdateList(IList<TSignal> list1, IList<TSignal> list2)
        {
            // 添加 list1 中不在 list2 的项
            foreach (var item in list1)
            {
                if (!list2.Contains(item))
                {
                    list2.Add(item);
                }
            }

            // 删除 list2 中不在 list1 的项
            for (int i = list2.Count - 1; i >= 0; i--)
            {
                if (!list1.Contains(list2[i]))
                {
                    list2.RemoveAt(i);
                }
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
        public DiscreteInputSignalLocatorViewModel(ObservableCollection<DiscreteInputSignal> signals, SignalStore signalStore, Func<Signal, DiscreteInputSignal> createSignal
            , Window window)
            : base(signals, signalStore, createSignal, window)
        {

        }

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
        public DiscreteOutputSignalLocatorViewModel(ObservableCollection<DiscreteOutputSignal> signals, SignalStore signalStore, Func<Signal, DiscreteOutputSignal> createSignal
            , Window window)
            : base(signals, signalStore, createSignal, window)
        {

        }

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

    public class NXPSignalLocatorViewModel : SignalLocatorViewModel<NXPSignal>
    {
        public NXPSignalLocatorViewModel(ObservableCollection<NXPSignal> signals, 
            SignalStore signalStore, 
            Func<Signal, NXPSignal> createSignal, 
            Window window, 
            Action<ObservableCollection<NXPSignal>, NXPSignal> addSignal = null) 
            : base(signals, signalStore, createSignal, window, addSignal)
        {
        }
    }
    public class NXPInputSignalLocatorViewModel : SignalLocatorViewModel<NXPInputSignal>
    {
        public NXPInputSignalLocatorViewModel(ObservableCollection<NXPInputSignal> signals, 
            SignalStore signalStore, 
            Func<Signal, NXPInputSignal> createSignal, 
            Window window, 
            Action<ObservableCollection<NXPInputSignal>, NXPInputSignal> addSignal = null) 
            : base(signals, signalStore, createSignal, window, addSignal)
        {
        }
    }
}
