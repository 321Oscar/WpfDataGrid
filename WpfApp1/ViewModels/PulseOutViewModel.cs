﻿using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using WpfApp1.Models;
using WpfApp1.Services;
using WpfApp1.Stores;

namespace WpfApp1.ViewModels
{
    public class PulseOutViewModel : SendFrameViewModelBase
    {
        public const string VIEWNAME = "Pulse_OUT";

        private ObservableCollection<PulseGroupSignalOutGroup> groups = new ObservableCollection<PulseGroupSignalOutGroup>();
        private RelayCommand updateCommand;

        public PulseOutViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService, ModalNavigationStore modalNavigationStore, IServiceProvider serviceProvider) 
            : base(signalStore, deviceStore, logService, modalNavigationStore, serviceProvider)
        {
           
        }
      
        public ICommand UpdateCommand { get => updateCommand ?? (updateCommand = new RelayCommand(Update)); }
        public IEnumerable<PulseGroupSignalOutGroup> Groups => groups;
        public IEnumerable<PulseOutSingleSignal> PulseOutSignals => SignalStore.GetSignals<PulseOutSingleSignal>();

        public override void Init()
        {
            GetGroups(SignalStore.SignalLocatorInfo.GetViewSignalInfo(VIEWNAME).Signals.OfType<PulseOutGroupSignal>());
            //{
            //    _signals.Add(signal);
            //}
            //BuildFramesHelper = new DBCSignalBuildHelper(PulseOutSignals, SignalStore.DbcFile.Messages);
        }

        public override void LocatorSignals()
        {
            ModalNavigationService<PulseOutSignalLocatorViewModel> modalNavigationService =
               new ModalNavigationService<PulseOutSignalLocatorViewModel>(
                   this.ModalNavigationStore,
                   () => new PulseOutSignalLocatorViewModel(new CloseModalNavigationService(ModalNavigationStore), 
                    groups, 
                    SignalStore,
                    (signal) =>
                    {
                        if (signal.SignalName.IndexOf("_Duty") > -1 || signal.SignalName.IndexOf("_Freq") > -1)
                        {
                            string[] groupName = signal.SignalName.Split(new string[] { "_Duty", "_Freq" }, StringSplitOptions.RemoveEmptyEntries);
                            if (groupName.Length == 1)
                            {
                                var group = new PulseGroupSignalOutGroup(groupName[0]);
                                var existSignal = SignalStore.Signals.FirstOrDefault(x => x.Name == signal.SignalName && x.MessageID == signal.MessageID);
                                PulseOutGroupSignal pulseInSignal;
                                if (existSignal == null || !(existSignal is PulseOutGroupSignal analog))
                                {
                                    pulseInSignal = new PulseOutGroupSignal(groupName[0])
                                    {
                                        Name = signal.SignalName,
                                        StartBit = (int)signal.startBit,
                                        Factor = signal.factor,
                                        Offset = signal.offset,
                                        ByteOrder = (int)signal.byteOrder,
                                        Length = (int)signal.signalSize,
                                        MessageID = signal.MessageID,
                                        ViewName = VIEWNAME
                                    };
                                    SignalStore.AddSignal(pulseInSignal);
                                }
                                else
                                {
                                    pulseInSignal = existSignal as PulseOutGroupSignal;
                                }
                                if (signal.SignalName.IndexOf("_Duty") > -1)
                                {
                                    group.DutyCycle = pulseInSignal;
                                }
                                else
                                {
                                    group.Freq = pulseInSignal;
                                }
                                return group;
                            }
                        }
                        return null;
                    },
                    (groups, group) =>
                    {
                        var existGroup = groups.FirstOrDefault(x => x.GroupName == group.GroupName);
                        if (existGroup != null)
                        {
                            if (group.Freq != null)
                                existGroup.Freq = group.Freq;
                            if (group.DutyCycle != null)
                                existGroup.DutyCycle = group.DutyCycle;
                        }
                        else
                        {
                            groups.Add(group);
                        }
                    }));
            modalNavigationService.Navigate();
        }

        public override void Dispose()
        {
            //SignalStore.SaveViewSignalLocator(VIEWNAME, PulseOutSignals);
            SignalStore.SaveViewSignalLocator(VIEWNAME, SignalStore.GetSignals<PulseOutGroupSignal>(), clear:false);
        }

        private void GetGroups(IEnumerable<PulseOutGroupSignal> pulseOutGroupsignals)
        {
            //var gdicSignals = SignalStore.GetSignals<PulseOutGroupSignal>();

             pulseOutGroupsignals
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
            .OrderBy(x=>x.GroupName)
            .ToList()
            .ForEach(x =>
            {
                groups.Add(x);
            });
            //gDICStatusGroups.Sort
        }

        private void Update()
        {
            foreach (var group in Groups)
            {
                group.Freq.UpdateRealValue();
                group.DutyCycle.UpdateRealValue();
            }

            Send();
        }

        public override void Send()
        {
            //base.Send();
            if (DeviceStore.HasDevice)
                DeviceStore.CurrentDevice.SendMultip(SignalStore.BuildFrames(SignalStore.GetSignals<PulseOutSingleSignal>(VIEWNAME)));
        }
    }
}
