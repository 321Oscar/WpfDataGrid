using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WpfApp1.Devices;
using WpfApp1.Models;
using WpfApp1.Services;
using WpfApp1.Stores;

namespace WpfApp1.ViewModels
{
    public class AnalogViewModel : ViewModelBase
    {
        private RelayCommand _updateSignalThresholdCommand;
        private RelayCommand _resetSignalThresholdCommand;
        private RelayCommand _calStandardDevCommand;
        private AnalogSignal currentAnalogSignal;
        private double maxThreshold;
        private double minThreshold;
        private int standardCount;
        private RelayCommand _searchSignalByNameCommand;
        private string searchSignalName;
        private readonly ObservableCollection<AnalogSignal> _signals = new ObservableCollection<AnalogSignal>();
        /// <summary>
        /// MVVM Mode with DI
        /// </summary>
        /// <param name="signalStore"></param>
        /// <param name="deviceStore"></param>
        /// <param name="logService"></param>
        /// <param name="modalNavigationStore"></param>
        /// <param name="serviceProvider"></param>
        public AnalogViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService,
            ModalNavigationStore modalNavigationStore, IServiceProvider serviceProvider)
            : base(signalStore, deviceStore, logService, modalNavigationStore, serviceProvider)
        {

        }
        /// <summary>
        /// WinForm Mode,No DependencyInjection
        /// </summary>
        /// <param name="signalStore"></param>
        /// <param name="deviceStore"></param>
        /// <param name="logService"></param>
        public AnalogViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService) : base(signalStore, deviceStore, logService)
        {

        }

        ~AnalogViewModel()
        {

        }

        public override void LocatorSignals()
        {
            if (ModalNavigationStore != null)
            {
                var modalNavigationService = new ModalNavigationService<AnalogSignalLocatorViewModel>(this.ModalNavigationStore, CreateSignalLocatorViewModel);
                modalNavigationService.Navigate();
            }
            else
            {
                Views.DialogView dialogView = new Views.DialogView(CreateSignalLocatorViewModel());
                dialogView.ShowDialog();
            }
        }

        public override void Init()
        {
            foreach (var signal in SignalStore.GetSignals<AnalogSignal>(nameof(AnalogViewModel)))
            {
                _signals.Add(signal);
            }
        }

        public override void Dispose()
        {
            SignalStore.SaveViewSignalLocator(nameof(AnalogViewModel), _signals);
        }
        public string SearchSignalName
        { 
            get => searchSignalName; 
            set 
            {
                if (SetProperty(ref searchSignalName, value))
                {
                    updateSearchedSignals = true;
                }
                _searchSignalByNameCommand.NotifyCanExecuteChanged();
            }
        }
        public AnalogSignal CurrentAnalogSignal
        {
            get => currentAnalogSignal;
            set
            {
                if (SetProperty(ref currentAnalogSignal, value))
                {
                    MaxThreshold = currentAnalogSignal.MaxThreshold;
                    MinThreshold = currentAnalogSignal.MinThreshold;
                }
            }
        }

        public bool UpdateAll { get; set; }

        public double MaxThreshold { get => maxThreshold; set => SetProperty(ref maxThreshold, value); }

        public double MinThreshold { get => minThreshold; set => SetProperty(ref minThreshold, value); }

        public IEnumerable<AnalogSignal> AnalogSignals
        {
            get
            {
                return _signals;
            }
        }

        public ICommand UpdateSignalThresholdCommand { get => _updateSignalThresholdCommand ?? (_updateSignalThresholdCommand = new RelayCommand(UpdateSignalThreshold)); }
        public ICommand ResetSignalThresholdCommand { get => _resetSignalThresholdCommand ?? (_resetSignalThresholdCommand = new RelayCommand(ResetSignalThreshold)); }
        public ICommand LoadSignalThresholdCommand { get; }
        public ICommand SaveSignalThresholdCommand { get; }
        public ICommand CalculateSignalStdDevCommand { get => _calStandardDevCommand ?? (_calStandardDevCommand = new RelayCommand(CalStandardDev, () => StandardCount > 0)); }
        public ICommand SearchSignalByNameCommand { get => _searchSignalByNameCommand ?? (_searchSignalByNameCommand = new RelayCommand(SearchSignalByName, () => !string.IsNullOrEmpty(SearchSignalName))); }
        private AnalogSignal[] searchedSignals;
        private bool updateSearchedSignals = true;
        private int searchSignalIndexIn = -1;
        private void SearchSignalByName()
        {
            if (updateSearchedSignals)
            {
                searchedSignals = AnalogSignals.Where(x => x.Name.IndexOf(SearchSignalName, StringComparison.OrdinalIgnoreCase) > -1).ToArray();
                updateSearchedSignals = false;
                searchSignalIndexIn = -1;
            }
            
            if (searchedSignals != null && searchedSignals.Length > 0)
            {
                if (searchSignalIndexIn < 0)
                {
                    searchSignalIndexIn++;
                    CurrentAnalogSignal = searchedSignals[searchSignalIndexIn];
                }
                else
                {
                    searchSignalIndexIn += 1;
                    if (searchSignalIndexIn == searchedSignals.Length)
                        searchSignalIndexIn = 0;
                    CurrentAnalogSignal = searchedSignals[searchSignalIndexIn];
                }
            }
            else
            {
                AdonisUI.Controls.MessageBox.Show($"Cannot find signalname Contains [{SearchSignalName}] !"
                    , "Info", AdonisUI.Controls.MessageBoxButton.OKCancel, AdonisUI.Controls.MessageBoxImage.Information);
            }
        }

        private void ResetSignalThreshold()
        {
            UpdateSignalThreshold(5, 0);
        }

        private void UpdateSignalThreshold()
        {
            UpdateSignalThreshold(MaxThreshold, MinThreshold);
        }
        private void UpdateSignalThreshold(double max, double min)
        {
            if (UpdateAll)
            {
                foreach (var signal in AnalogSignals)
                {
                    if (signal is AnalogSignal analogSignal)
                    {
                        analogSignal.MinThreshold = min;
                        analogSignal.MaxThreshold = max;
                    }
                }
            }
            else
            {
                if (CurrentAnalogSignal != null)
                {
                    CurrentAnalogSignal.MinThreshold = min;
                    CurrentAnalogSignal.MaxThreshold = max;
                }
                else
                {
                    //do nothing
                }
            }

        }
        public int StandardCount
        {
            get => standardCount;
            set
            {
                if (value > 1000)
                    value = 1000;
                if (SetProperty(ref standardCount, value))
                {
                    _calStandardDevCommand.NotifyCanExecuteChanged();
                }
            }
        }
        private void CalStandardDev()
        {
            //throw new NotImplementedException();
            foreach (var signal in AnalogSignals)
            {
                signal.CalStandard(StandardCount);
            }
        }

        private AnalogSignalLocatorViewModel CreateSignalLocatorViewModel()
        {
            return new AnalogSignalLocatorViewModel(new CloseModalNavigationService(ModalNavigationStore), _signals, SignalStore, CreateAnalogByDBCSignal);
        }

        private AnalogSignal CreateAnalogByDBCSignal(Signal signal)
        {
            var existSignal = SignalStore.Signals.FirstOrDefault(x => x.Name == signal.SignalName && x.MessageID == signal.MessageID);
            if (existSignal != null && existSignal is AnalogSignal analog)
                return analog;

            AnalogSignal analogSignal = new AnalogSignal(signal, "Analog");
            SignalStore.AddSignal(analogSignal);
            return analogSignal;
        }
    }
}
