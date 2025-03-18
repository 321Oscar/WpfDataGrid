using ERad5TestGUI.Models;
using ERad5TestGUI.Services;
using ERad5TestGUI.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERad5TestGUI.ViewModels
{
    public class ELockerViewModel : SendFrameViewModelBase
    {
        private readonly List<SignalGroupBase> _groups = new List<SignalGroupBase>();

        public ELockerViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService) : base(signalStore, deviceStore, logService)
        {
            _ViewName = "E-Locker";
        }

        public ELockerViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService, ModalNavigationStore modalNavigationStore, IServiceProvider serviceProvider) : base(signalStore, deviceStore, logService, modalNavigationStore, serviceProvider)
        {
        }


        public IEnumerable<SignalGroupBase> Groups { get => _groups; }

        public override void Init()
        {
            if(_ViewName == null)
                _ViewName = "E-Locker";
            base.Init();
            LoadSignals();
        }

        private void LoadSignals()
        {
            AnalogSignalGroup anGroup = new AnalogSignalGroup("Analog");
            anGroup.Signals.AddRange(SignalStore.GetSignals<AnalogSignal>(ViewName));
            _groups.Add(anGroup);

            DiscreteInputSignalGroup disInputGroup = new DiscreteInputSignalGroup("Discrete Inputs");
            disInputGroup.Signals.AddRange(SignalStore.GetSignals<DiscreteInputSignal>(ViewName));
            _groups.Add(disInputGroup);

            DiscreteOutputSignalGroup diOutGroup = new DiscreteOutputSignalGroup("Discrete Outputs");
            diOutGroup.Signals.AddRange(SignalStore.GetSignals<DiscreteOutputSignal>(ViewName));
            _groups.Add(diOutGroup);

            PulseInGroupGroup pulseInGroupGroup = new PulseInGroupGroup("Pulse In");
            var pulseInSignals = SignalStore.GetSignals<PulseInSignal>(ViewName);
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

            _groups.Add(pulseInGroupGroup);

            PulseOutGroupGroup pulseOutGroup = new PulseOutGroupGroup("Pulse Out");

            SignalStore.GetSignals<PulseOutGroupSignal>(ViewName)
                       .GroupBy(s => s.GroupName)
                       .Select(g =>
                       {
                           if (!string.IsNullOrEmpty(g.Key))
                           {
                               var group = new PulseGroupSignalOutGroup(g.Key);
                               var signals = g.ToList();
                               signals.Sort((x, y) =>
                               {
                                   return x.Name.CompareTo(y.Name);
                               });
                               group.DutyCycle = signals[0];
                               group.Freq = signals[1];
                               return group;
                           }
                           return null;
                       })
                       .OrderBy(x => x.GroupName)
                       .ToList()
                       .ForEach(x => pulseOutGroup.Groups.Add(x));

            _groups.Add(pulseOutGroup);

            GDICStatuGroupGroup sentSignals = new GDICStatuGroupGroup("SENT");
            var sentDatas = SignalStore.GetSignals<GDICStatusDataSignal>(ViewName);
            var xx = sentDatas
                                .GroupBy(s => s.GroupName)
                                .Select(g =>
                                {
                                    int length = 6;
                                    var classRoom = new GDICStatusGroup(g.Key, length);
                                    var signals = g.OrderByDescending(x => x.StartBit).ToList();
                                    for (int i = signals.Count - 1; i > -1; i--)
                                    {
                                        int idenx = i + (length - signals.Count);

                                        classRoom.GDICStatusSignals[idenx] = signals[i];
                                    }

                                    return classRoom;
                                });
            sentSignals.Groups.AddRange(xx);
            _groups.Add(sentSignals);

            //GDICRegistersGroup registers = new GDICRegistersGroup("DRV8705");
            //var resDatas = SignalStore.GetSignals<GDICRegisterSignal>(ViewName);
            //registers.Signals.AddRange(resDatas);
            //_groups.Add(registers);
        }
    }
}
