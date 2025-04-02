using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using ERad5TestGUI.Models;
using ERad5TestGUI.Services;
using ERad5TestGUI.Stores;

namespace ERad5TestGUI.ViewModels
{
    public class SPIViewModel : SendFrameViewModelBase
    {
        private readonly ObservableCollection<SPISignalGroup> _spiSignals = new ObservableCollection<SPISignalGroup>();
        private RelayCommand _resetCommand;
        public SPIViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService) : base(signalStore, deviceStore, logService)
        {
            //Init();
        }

        public SPIViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService, ModalNavigationStore modalNavigationStore, IServiceProvider serviceProvider) 
            : base(signalStore, deviceStore, logService, modalNavigationStore, serviceProvider)
        {
            
        }

        public IEnumerable<SPISignalGroup> SPISignals { get => _spiSignals; }
        public DiscreteOutputSignal ResetSignal => SignalStore.GetSignals<DiscreteOutputSignal>(ViewName).FirstOrDefault();
        public ICommand ResetCommand => _resetCommand ?? (_resetCommand = new RelayCommand(Reset));
        public override void Init()
        {
            _spiSignals.AddRange(SignalStore.GetSignals<SPISignal>(ViewName)
                                .GroupBy(s => s.ChannelName)
                                .Select(g =>
                                {
                                    var group = new SPISignalGroup(g.Key);
                                    var signals = g.ToList();
                                    signals.Sort((x, y) =>
                                    {
                                        return x.Name.CompareTo(y.Name);
                                    });
                                    group.CurrentValue = signals.FirstOrDefault(x => x.Name.IndexOf("Cur") > -1);
                                    group.SelectValue = signals.FirstOrDefault(x => x.Name.IndexOf("Selec") > -1);
                                    return group;
                                })
                                .OrderBy(x => x.GroupName));
            //.ToList();
            SignalStore.GetSignals<SPISignal>(ViewName).Where(x => x.InOrOut).ToList().ForEach(s =>
            {
                s.PropertyChanged += SPIValueChanged;
            });
        }

        private void SPIValueChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SignalBase.OriginValue))
            {
                SendFD(SignalStore.BuildFrames(new SignalBase[] { sender as SPISignal }));
            }
        }

        public override void LocatorSignals()
        {
            base.LocatorSignals();
        }

        private void Reset()
        {
            ResetSignal.OriginValue = 1;
            SendFD(SignalStore.BuildFrames(new SignalBase[] { ResetSignal }));
            ResetSignal.OriginValue = 0;
        }
    }
}
