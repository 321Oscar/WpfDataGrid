using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using ERad5TestGUI.Services;
using ERad5TestGUI.Stores;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace ERad5TestGUI.ViewModels
{
    public class LogViewModel : ViewModelBase
    {
        private bool _startLog;
        private int _interval = 500;
        private Thread _pushThread;
        private RelayCommand _startCommand;
        private RelayCommand _clearCommand;
        private RelayCommand _saveToCommand;
        private bool _analogSignalLogEnable = true;
        private bool _discreteInSignalLogEnable = true;
        private bool _discreteOutSignalLogEnable = true;
        private bool _pulseInSignalLogEnable = true;

        public LogViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService) : base(signalStore, deviceStore, logService)
        {
            SignalLogs = new ObservableCollection<string>();
        }
        public ICommand StartCommand { get => _startCommand ?? (_startCommand = new RelayCommand(StartLog)); }
        public ICommand ClearCommand { get => _clearCommand ?? (_clearCommand = new RelayCommand(Clear)); }
        public ICommand SaveToCommand { get => _saveToCommand ?? (_saveToCommand = new RelayCommand(ChangeLogFilePath)); }
        public int Interval { get => _interval; set => _interval = value; }
        public bool IsLogging { get => _startLog; set => SetProperty(ref _startLog, value); }

        public ObservableCollection<string> SignalLogs { get; private set; }
        public bool AnalogSignalLogEnable
        {
            get => _analogSignalLogEnable;
            set
            {
                if (SetProperty(ref _analogSignalLogEnable, value))
                {
                    SignalStore.RegisterLogMapping(typeof(Models.AnalogSignal), _analogSignalLogEnable);
                }
            }
        }

        public bool DiscreteInSignalLogEnable
        {
            get => _discreteInSignalLogEnable;
            set
            {
                if (SetProperty(ref _discreteInSignalLogEnable, value))
                {
                    SignalStore.RegisterLogMapping(typeof(Models.DiscreteInputSignal), _discreteInSignalLogEnable);
                }
            }
        }
        public bool DiscreteOutSignalLogEnable
        {
            get => _discreteOutSignalLogEnable;
            set
            {
                if (SetProperty(ref _discreteOutSignalLogEnable, value))
                {
                    SignalStore.RegisterLogMapping(typeof(Models.DiscreteOutputSignal), _discreteOutSignalLogEnable);
                }
            }
        }

        public bool PulseInSignalLogEnable
        {
            get => _pulseInSignalLogEnable;
            set
            {
                if (SetProperty(ref _pulseInSignalLogEnable, value))
                {
                    SignalStore.RegisterLogMapping(typeof(Models.PulseInSignal), _pulseInSignalLogEnable);
                }
            }
        }
        private void StartLog()
        {
            if (IsLogging && _pushThread != null)
            {
                IsLogging = false;
                DeviceStore.SignalLogEnable = false;
                _pushThread.Abort();
                while (!SignalStore.SignalValueLogQuere.IsEmpty)
                {
                    SignalStore.SignalValueLogQuere.TryDequeue(out _);
                }
            }
            else
            {
                DeviceStore.SignalLogEnable = true;
                IsLogging = true;
                _pushThread = new Thread(new ThreadStart(PushLogToView));
                _pushThread.IsBackground = true;
                _pushThread.Start();
            }
        }

        private void PushLogToView()
        {
            while (IsLogging)
            {
                var d = DateTime.Now;
                List<string> logs = new List<string>();
                while (d.AddMilliseconds(Interval) > DateTime.Now)
                {
                    if (SignalStore.SignalValueLogQuere.TryDequeue(out string log))
                    {
                        logs.Add(log);
                    }
                }
                Dispatch(() => 
                {
                    if (SignalLogs.Count + logs.Count > 10000)
                    {
                        for (int i = 0; i < logs.Count; i++)
                        {
                            SignalLogs.RemoveAt(0);
                            SignalLogs.Add(logs[i]);
                        }
                    }
                    else
                    {
                        SignalLogs.AddRange(logs);
                    }

                    if(_autoScrollEnabled && _listBoxScrollViewer != null)
                    {
                        _listBoxScrollViewer.ScrollToEnd();
                    }
                });
                if (SavingLogFile)
                    SaveToFile(logs);
            }
        }

        private void Clear()
        {
            SignalLogs.Clear();
        }
        private string _logFilePath = @"Log/SignalLog.txt";
        private bool _savingLogFile;

        public string LogFilePath { get => _logFilePath; set => _logFilePath = value; }
        public bool SavingLogFile { get => _savingLogFile; set => SetProperty(ref _savingLogFile ,value); }
        private void ChangeLogFilePath()
        {
            if (SavingLogFile)
            {
                SavingLogFile = false;
                ShowMsgInfoBox($"Log File Path:{LogFilePath}", "Save Signal Log");
            }
            else
            {
                CommonOpenFileDialog ofd = new CommonOpenFileDialog();
                ofd.DefaultFileName = "SignalValue" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                ofd.DefaultExtension = "txt";
                ofd.Filters.Add(new CommonFileDialogFilter("Log File", "*.txt"));
                if (ofd.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    LogFilePath = ofd.FileName;
                    SavingLogFile = true;
                    using (System.IO.File.Create(LogFilePath))
                    {

                    }
                }
            }

        }

        private void SaveToFile(IEnumerable<string> strings)
        {
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(this._logFilePath,true))
            {
                foreach (var str in strings)
                {
                    sw.WriteLine(str);
                }
            }
        }
        #region ListBox自动滚动
        private bool _autoScrollEnabled = true;
        private ScrollViewer _listBoxScrollViewer;

        public void AttachScrollViewer(ListBox listbox)
        {
            if (VisualTreeHelper.GetChildrenCount(listbox) > 0)
            {
                var border = (Border)VisualTreeHelper.GetChild(listbox, 0);
                _listBoxScrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                _listBoxScrollViewer.ScrollChanged -= OnScrollChanged;
                _listBoxScrollViewer.ScrollChanged += OnScrollChanged;
            }
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if(Math.Abs(e.VerticalChange - e.ExtentHeightChange) > 1e-6)
            {
                _autoScrollEnabled = false;
            }

            if(_listBoxScrollViewer.VerticalOffset >= _listBoxScrollViewer.ExtentHeight - _listBoxScrollViewer.ViewportHeight - 1)
            {
                _autoScrollEnabled = true;
            }
        }
        #endregion
    }
}
