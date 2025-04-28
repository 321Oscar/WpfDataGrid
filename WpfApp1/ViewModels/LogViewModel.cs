using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using ERad5TestGUI.Services;
using ERad5TestGUI.Stores;

namespace ERad5TestGUI.ViewModels
{
    public class LogViewModel : ViewModelBase
    {
        private bool _startLog;
        private int _interval = 50;
        private Thread _pushThread;
        private RelayCommand _startCommand;
        private RelayCommand _clearCommand;
        private bool _analogSignalLogEnable = true;

        public LogViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService) : base(signalStore, deviceStore, logService)
        {
            SignalLogs = new ObservableCollection<string>();
        }
        public ICommand StartCommand { get => _startCommand ?? (_startCommand = new RelayCommand(StartLog)); }
        public ICommand ClearCommand { get => _clearCommand ?? (_clearCommand = new RelayCommand(Clear)); }
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
                _pushThread.Start();
            }
        }

        private void PushLogToView()
        {
            while (_startLog)
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
                Dispatch(() => SignalLogs.AddRange(logs));
            }
        }

        private void Clear()
        {
            SignalLogs.Clear();
        }
    }
}
