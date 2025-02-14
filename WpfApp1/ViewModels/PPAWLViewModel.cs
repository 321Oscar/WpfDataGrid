using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using WpfApp1.Models;
using WpfApp1.Services;
using WpfApp1.Stores;

namespace WpfApp1.ViewModels
{
    public class PPAWLViewModel : ViewModelBase
    {
        private readonly List<SignalGroupBase> groups = new List<SignalGroupBase>();

        public PPAWLViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService, ModalNavigationStore modalNavigationStore, IServiceProvider serviceProvider) : base(signalStore, deviceStore, logService, modalNavigationStore, serviceProvider)
        {
            LoadOtherSignals();
        }

        public IEnumerable<AnalogSignal> AnalogSignals
        {
            get
            {
                //System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                //stopwatch.Restart();
                var signals = SignalStore.GetSignals<AnalogSignal>(nameof(PPAWLViewModel));
                //stopwatch.Stop();
                //Console.WriteLine($"{DateTime.Now:HH:mm:ss fff} analog get data take {stopwatch.ElapsedMilliseconds} ms");
                return signals;
            }
        }

        public IEnumerable<SignalGroupBase> Groups { get => groups; }

        private void LoadOtherSignals()
        {
            DiscreteInputSignalGroup disInputGroup = new DiscreteInputSignalGroup("Discrete Inputs");
            disInputGroup.Signals.AddRange(SignalStore.GetSignals<DiscreteInputSignal>(nameof(PPAWLViewModel)));
            groups.Add(disInputGroup);

            DiscreteOutputSignalGroup diOutGroup = new DiscreteOutputSignalGroup("Discrete Outputs");
            diOutGroup.Signals.AddRange(SignalStore.GetSignals<DiscreteOutputSignal>(nameof(PPAWLViewModel)));
            groups.Add(diOutGroup);

            PulseInGroupGroup pulseInGroupGroup = new PulseInGroupGroup("Pulse In");
            var pulseInSignals = SignalStore.GetSignals<PulseInSignal>(nameof(PPAWLViewModel));
            pulseInGroupGroup.Groups.AddRange(pulseInSignals
                                              .GroupBy(s => s.GroupName)
                                              .Select(g =>
                                              {
                                                  var classRoom = new PulseInSignalGroup(g.Key);
                                                  var signals = g.ToList();
                                                  signals.Sort((x, y) =>
                                                  {
                                                      return x.Name.CompareTo(y.Name);
                                                  });
                                                  classRoom.Signal_DC = signals[0];
                                                  classRoom.Signal_Freq = signals[1];
                                                  return classRoom;
                                              })
                                              .OrderBy(x => x.GroupName));

            groups.Add(pulseInGroupGroup);
        }
    }
}