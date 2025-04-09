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
using ERad5TestGUI.Devices;
using ERad5TestGUI.Models;
using ERad5TestGUI.Services;
using ERad5TestGUI.Stores;
using Microsoft.WindowsAPICodePack.Dialogs;
using ERad5TestGUI.Helpers;
using System.Windows.Data;
using System.ComponentModel;

namespace ERad5TestGUI.ViewModels
{
    public class AnalogViewModel : ViewModelBase, Interfaces.IClearData
    {
        private RelayCommand _updateSignalThresholdCommand;
        private RelayCommand _resetSignalThresholdCommand;
        private RelayCommand _saveSignalThresholdCommand;
        private RelayCommand _loadSignalThresholdCommand;
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
                //Log("123");
                Views.DialogView dialogView = new Views.DialogView();
                var locatorViewModel = CreateSignalLocatorViewModel(dialogView);
                dialogView.DialogViewModel = locatorViewModel;
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
                if (SetProperty(ref currentAnalogSignal, value) && currentAnalogSignal != null)
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

                //var viewSource = CollectionViewSource.GetDefaultView(_signals);
                //viewSource.GroupDescriptions.Add(new PropertyGroupDescription("GroupName"));
                //return viewSource;
            }
        }

        public ICommand UpdateSignalThresholdCommand { get => _updateSignalThresholdCommand ?? (_updateSignalThresholdCommand = new RelayCommand(UpdateSignalThreshold)); }
        public ICommand ResetSignalThresholdCommand { get => _resetSignalThresholdCommand ?? (_resetSignalThresholdCommand = new RelayCommand(ResetSignalThreshold)); }
        public ICommand LoadSignalThresholdCommand { get => _loadSignalThresholdCommand ?? (_loadSignalThresholdCommand = new RelayCommand(LoadLimit)); }
        public ICommand SaveSignalThresholdCommand { get => _saveSignalThresholdCommand ?? (_saveSignalThresholdCommand = new RelayCommand(SaveLimits)); }
        public ICommand CalculateSignalStdDevCommand { get => _calStandardDevCommand ?? (_calStandardDevCommand = new RelayCommand(CalStandardDev, () => StandardCount > 0)); }
        public ICommand SearchSignalByNameCommand { get => _searchSignalByNameCommand ?? (_searchSignalByNameCommand = new RelayCommand(SearchSignalByName, () => !string.IsNullOrEmpty(SearchSignalName))); }
        private AnalogSignal[] searchedSignals;
        private bool updateSearchedSignals = true;
        private int searchSignalIndexIn = -1;

        public void ClearData()
        {
            foreach (var signal in AnalogSignals)
            {
                //signal.cle
                signal.Clear();
            }
        }

        private void SearchSignalByName()
        {
            if (updateSearchedSignals)
            {
                searchedSignals = _signals.Where(x => x.Name.IndexOf(SearchSignalName, StringComparison.OrdinalIgnoreCase) > -1).ToArray();
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
                    , "Info", AdonisUI.Controls.MessageBoxButton.OK, AdonisUI.Controls.MessageBoxImage.Information);
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

#if DEBUG
                    var ss = SignalStore.Signals.FirstOrDefault(x => x.Name == CurrentAnalogSignal.Name);
                    var xx = SignalStore.SignalLocation.Signals.FirstOrDefault(x => x.Name == CurrentAnalogSignal.Name);
                    LogService.Debug($"{(ss as AnalogSignal).MaxThreshold};{(xx as AnalogSignal).MaxThreshold}");
#endif
                }
                else
                {
                    //do nothing
                }
            }

        }

        private void LoadLimit()
        {
            var ofd = new CommonOpenFileDialog();
            ofd.DefaultDirectory = @".\Config\";
            ofd.Filters.Add(new CommonFileDialogFilter("Limit xml file", "*.xml"));
            if(ofd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                try
                {
                    var limits = XmlHelper.DeserializeFromXml<List<LimitInfo>>(ofd.FileName);
                    foreach (var signal in _signals)
                    {
                        var limitSignal = limits.FirstOrDefault(x => x.Name == signal.Name);
                        if(limitSignal != null)
                        {
                            signal.MaxThreshold = limitSignal.Max;
                            signal.MinThreshold = limitSignal.Min;
                        }
                    }
                    LogService.Log($"Load Limit File:{ofd.FileName}");
                }
                catch (Exception ex)
                {
                    AdonisUI.Controls.MessageBox.Show($"Load Limit File Fail:{ex.Message}"
                    , "Error", AdonisUI.Controls.MessageBoxButton.OK, AdonisUI.Controls.MessageBoxImage.Error);
                }
            }
        }

        private void SaveLimits()
        {
            var sfd = new CommonSaveFileDialog();
            sfd.Filters.Add(new CommonFileDialogFilter("Limit xml file", "*.xml"));
            sfd.DefaultFileName = "Limits.xml";
            if (sfd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                List<LimitInfo> limitInfos = new List<LimitInfo>();
                foreach (var signal in _signals)
                {
                    LimitInfo i = new LimitInfo()
                    {
                        Name = signal.Name,
                        Max = signal.MaxThreshold,
                        Min = signal.MinThreshold
                    };
                    limitInfos.Add(i);
                }

                try
                {
                    XmlHelper.SerializeToXml(limitInfos, sfd.FileName);
                    LogService.Log($"Save Limit File to:{sfd.FileName}");
                }
                catch (Exception ex)
                {
                    AdonisUI.Controls.MessageBox.Show($"Save Limit File Fail:{ex.Message}"
                                        , "Error", AdonisUI.Controls.MessageBoxButton.OK, AdonisUI.Controls.MessageBoxImage.Error); ;
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
            if (DeviceStore.CurrentDevice.Started)
            {
                AdonisUI.Controls.MessageBox.Show($"calculate Standrad Must without receive Data!",
                      "Calculate Stad",
                      icon: AdonisUI.Controls.MessageBoxImage.Information);
                return;
            }

            //throw new NotImplementedException();
            foreach (var signal in _signals)
            {
                signal.CalStandard(StandardCount);
            }
        }

        private AnalogSignalLocatorViewModel CreateSignalLocatorViewModel(System.Windows.Window window)
            => new AnalogSignalLocatorViewModel(ViewName,_signals,
                                                SignalStore,
                                                CreateAnalogByDBCSignal, 
                                                window);

        private AnalogSignalLocatorViewModel CreateSignalLocatorViewModel()
        {
            return new AnalogSignalLocatorViewModel(ViewName, new CloseModalNavigationService(ModalNavigationStore), _signals, SignalStore, CreateAnalogByDBCSignal);
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
