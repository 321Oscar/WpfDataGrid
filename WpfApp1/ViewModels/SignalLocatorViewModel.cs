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
using ERad5TestGUI.Models;
using ERad5TestGUI.Services;
using ERad5TestGUI.Stores;

namespace ERad5TestGUI.ViewModels
{
    public class SignalLocatorViewModel<TSignal> : ObservableRecipient, IDialogWindow
        where TSignal : class
    {
        private readonly INavigationService _navigationService;

        private RelayCommand _addCommand;
        //private RelayCommand<IEnumerable<Signal>> addMultipCommand;
        private RelayCommand<object> _addMultipCommand;

        private Func<Stores.Signal, TSignal> _createSignal;
        private Action<ObservableCollection<TSignal>, TSignal> _addSignal;
        private ObservableCollection<TSignal> _tmpsignals = new ObservableCollection<TSignal>();
        private ObservableCollection<Signal> _filterSignals = new ObservableCollection<Signal>();
        private Signal _currentDbcSignal;
        private TSignal _currentTSignal;
        private RelayCommand _delCommand;
        private string _title = "Locator Signal To View";
        private string _filterStr;
        protected string _viewName;
        private bool filterView = true;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signals"></param>
        /// <param name="signalStore"></param>
        /// <param name="createSignal"></param>
        /// <param name="addSignal"></param>
        private SignalLocatorViewModel(string viewName, ObservableCollection<TSignal> signals,
                                       Stores.SignalStore signalStore,
                                       Func<Stores.Signal, TSignal> createSignal,
                                       Action<ObservableCollection<TSignal>, TSignal> addSignal = null)
        {
            _createSignal = createSignal;
            _addSignal = addSignal;
            _viewName = viewName;

            SignalStore = signalStore;
            if (signals != null)
            {
                Signals = signals;
                _tmpsignals.AddRange(Signals);
            }

            //var dbcSignals = SignalStore.DBCSignals.Where(x => x.Page == null || x.Page.IndexOf(viewName) > -1);
            //dbcSignals.OrderBy(x => x.Page);
            //DbcSignals = dbcSignals;
            LoadDBCSignals();

            CancelCommand = new RelayCommand(Cancel);
            OkCommand = new RelayCommand(Ok);
        }
        /// <summary>
        /// Without Mvvm Dialog
        /// </summary>
        /// <param name="signals"></param>
        /// <param name="signalStore"></param>
        /// <param name="createSignal"></param>
        /// <param name="window"></param>
        /// <param name="addSignal"></param>
        public SignalLocatorViewModel(string viewName, 
            ObservableCollection<TSignal> signals, 
            Stores.SignalStore signalStore,
            Func<Stores.Signal, TSignal> createSignal, 
            Window window,
            Action<ObservableCollection<TSignal>, TSignal> addSignal = null) 
            : this(viewName, signals, signalStore, createSignal, addSignal)
        {
            Title = typeof(TSignal).Name;
            Window = window;

        }
        /// <summary>
        /// MVVM Dialog
        /// </summary>
        /// <param name="navigationService"></param>
        /// <param name="signals"></param>
        /// <param name="signalStore"></param>
        /// <param name="createSignal"></param>
        /// <param name="addSignal"></param>
        public SignalLocatorViewModel(string viewName, INavigationService navigationService, ObservableCollection<TSignal> signals, Stores.SignalStore signalStore,
            Func<Stores.Signal, TSignal> createSignal, Action<ObservableCollection<TSignal>, TSignal> addSignal = null) :
             this(viewName, signals, signalStore, createSignal, addSignal)

        {
            this._navigationService = navigationService;
        }
        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public Stores.SignalStore SignalStore { get; }
        public IEnumerable<Signal> DbcSignals { get => _filterSignals; }
        public ObservableCollection<TSignal> Signals { get; }
        public ObservableCollection<TSignal> TempSignals { get => _tmpsignals; }
        public Stores.Signal CurrentDbcSignal
        {
            get => _currentDbcSignal;
            set
            {
                _currentDbcSignal = value;
                _addCommand?.NotifyCanExecuteChanged();
            }
        }
        public TSignal CurrentTSignal
        {
            get => _currentTSignal;
            set
            {
                _currentTSignal = value;
                _delCommand.NotifyCanExecuteChanged();
            }
        }

        public ICommand CancelCommand { get; }
        public ICommand OkCommand { get; }
        public ICommand AddCommand { get => _addCommand ?? (_addCommand = new RelayCommand(Add, () => (_createSignal != null && CurrentDbcSignal != null))); }
        public ICommand AddMultipCommand { get => _addMultipCommand ?? (_addMultipCommand = new RelayCommand<object>(Add)); }
        public ICommand DelCommand { get => _delCommand ?? (_delCommand = new RelayCommand(Del, () => CurrentTSignal != null)); }

        public Window Window { get; }
        public bool FilterView 
        { 
            get => filterView;
            set
            {
                if (SetProperty(ref filterView, value))
                {
                    LoadDBCSignals();
                }
            }
        }
        public string FilterStr
        {
            get => _filterStr;
            set
            {
                if (SetProperty(ref _filterStr, value))
                {
                    LoadDBCSignals();
                }
            }
        }
        private void LoadDBCSignals()
        {
            _filterSignals.Clear();
            var dbcSignals = SignalStore.DBCSignals;
            if (FilterView && !string.IsNullOrEmpty(_viewName))
                dbcSignals = dbcSignals.Where(x => x.Page == null || x.Page.IndexOf(_viewName) > -1);
            if (!string.IsNullOrEmpty(FilterStr))
            {
                dbcSignals = dbcSignals.Where(x => x.SignalName.IndexOf(FilterStr, StringComparison.OrdinalIgnoreCase) > -1);
            }
            _filterSignals.AddRange(dbcSignals);
        }

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
            if (list2 == null) return;

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
        public AnalogSignalLocatorViewModel(string viewName, ObservableCollection<AnalogSignal> signals, SignalStore signalStore, Func<Signal, AnalogSignal> createSignal
           , Window window)
           : base(viewName, signals, signalStore, createSignal, window)
        {

        }
        public AnalogSignalLocatorViewModel(string viewName, INavigationService navigationService, 
            ObservableCollection<AnalogSignal> signals, 
            SignalStore signalStore, 
            Func<Signal, AnalogSignal> createSignal) 
            : base(viewName, navigationService, signals, signalStore, createSignal)
        {
        }
    }
    public class ResolverSignalLocatorViewModel : SignalLocatorViewModel<ResolverSignal>
    {
        public ResolverSignalLocatorViewModel(string viewName, ObservableCollection<ResolverSignal> signals, SignalStore signalStore, Func<Signal, ResolverSignal> createSignal
           , Window window)
           : base(viewName, signals, signalStore, createSignal, window)
        {

        }
        public ResolverSignalLocatorViewModel(string viewName, INavigationService navigationService, 
            ObservableCollection<ResolverSignal> signals, 
            SignalStore signalStore, 
            Func<Signal, ResolverSignal> createSignal) 
            : base(viewName, navigationService, signals, signalStore, createSignal)
        {
        }
    }

    public class DiscreteInputSignalLocatorViewModel : SignalLocatorViewModel<DiscreteInputSignal>
    {
        public DiscreteInputSignalLocatorViewModel(string viewName, ObservableCollection<DiscreteInputSignal> signals, SignalStore signalStore, Func<Signal, DiscreteInputSignal> createSignal
            , Window window)
            : base(viewName, signals, signalStore, createSignal, window)
        {

        }

        public DiscreteInputSignalLocatorViewModel(string viewName, INavigationService navigationService, ObservableCollection<DiscreteInputSignal> signals, SignalStore signalStore, Func<Signal, DiscreteInputSignal> createSignal) : base(viewName, navigationService, signals, signalStore, createSignal)
        {
        }
    }

    public class DiscreteOutputSignalLocatorViewModel : SignalLocatorViewModel<DiscreteOutputSignal>
    {
        public DiscreteOutputSignalLocatorViewModel(string viewName, ObservableCollection<DiscreteOutputSignal> signals, SignalStore signalStore, Func<Signal, DiscreteOutputSignal> createSignal, Window window) : base(viewName, signals, signalStore, createSignal, window)
        {

        }

        public DiscreteOutputSignalLocatorViewModel(string viewName, INavigationService navigationService, ObservableCollection<DiscreteOutputSignal> signals, SignalStore signalStore, Func<Signal, DiscreteOutputSignal> createSignal) : base(viewName, navigationService, signals, signalStore, createSignal)
        {
        }
    }
    public class PulseInSignalLocatorViewModel : SignalLocatorViewModel<PulseInSignalGroup>
    {
        public PulseInSignalLocatorViewModel(string viewName, ObservableCollection<PulseInSignalGroup> signals, SignalStore signalStore, Func<Signal, PulseInSignalGroup> createSignal, Window window, Action<ObservableCollection<PulseInSignalGroup>, PulseInSignalGroup> addSignal = null) : base(viewName, signals, signalStore, createSignal, window, addSignal)
        {
        }
    }
    public class PulseOutSignalLocatorViewModel : SignalLocatorViewModel<PulseOutGroupSignalGroup>
    {
        public PulseOutSignalLocatorViewModel(string viewName, ObservableCollection<PulseOutGroupSignalGroup> signals, SignalStore signalStore, Func<Signal, PulseOutGroupSignalGroup> createSignal, Window window) : base(viewName, signals, signalStore, createSignal, window)
        {

        }
        public PulseOutSignalLocatorViewModel(string viewName, INavigationService navigationService, 
            ObservableCollection<PulseOutGroupSignalGroup> signals, 
            SignalStore signalStore, 
            Func<Signal, PulseOutGroupSignalGroup> createSignal,
            Action<ObservableCollection<PulseOutGroupSignalGroup>, PulseOutGroupSignalGroup> addSignal) 
            : base(viewName, navigationService, signals, signalStore, createSignal, addSignal)
        {
        }
    }

    public class NXPSignalLocatorViewModel : SignalLocatorViewModel<NXPSignal>
    {
        public NXPSignalLocatorViewModel(string viewName, ObservableCollection<NXPSignal> signals, SignalStore signalStore, Func<Signal, NXPSignal> createSignal, Window window, Action<ObservableCollection<NXPSignal>, NXPSignal> addSignal = null) : base(viewName, signals, signalStore, createSignal, window, addSignal)
        {
        }
    }
    public class NXPInputSignalLocatorViewModel : SignalLocatorViewModel<NXPInputSignal>
    {
        public NXPInputSignalLocatorViewModel(string viewName, ObservableCollection<NXPInputSignal> signals, SignalStore signalStore, Func<Signal, NXPInputSignal> createSignal, Window window, Action<ObservableCollection<NXPInputSignal>, NXPInputSignal> addSignal = null) : base(viewName, signals, signalStore, createSignal, window, addSignal)
        {
        }
    }

    public class GDICStatusDataSignalLocatorViewModel : SignalLocatorViewModel<GDICStatusDataSignal>
    {
        public GDICStatusDataSignalLocatorViewModel(string viewName, 
            ObservableCollection<GDICStatusDataSignal> signals, 
            SignalStore signalStore, 
            Func<Signal, GDICStatusDataSignal> createSignal, 
            Window window, 
            Action<ObservableCollection<GDICStatusDataSignal>, GDICStatusDataSignal> addSignal = null) 
            : base(viewName, signals, signalStore, createSignal, window, addSignal)
        {
        }
    }
    public class GDICRegisterSignalLocatorViewModel : SignalLocatorViewModel<GDICRegisterSignal>
    {
        public GDICRegisterSignalLocatorViewModel(string viewName, 
            ObservableCollection<GDICRegisterSignal> signals, 
            SignalStore signalStore, 
            Func<Signal, GDICRegisterSignal> createSignal, 
            Window window, 
            Action<ObservableCollection<GDICRegisterSignal>, GDICRegisterSignal> addSignal = null) 
            : base(viewName, signals, signalStore, createSignal, window, addSignal)
        {
        }
    }
}
