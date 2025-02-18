﻿using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using WpfApp1.Models;
using WpfApp1.Services;
using WpfApp1.Stores;

namespace WpfApp1.ViewModels
{
    public class PulseInViewModel : ViewModelBase
    {
        public const string VIEWNAME = "Pulse_IN"; 

        public PulseInViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService, ModalNavigationStore modalNavigationStore, IServiceProvider serviceProvider) : base(signalStore, deviceStore, logService, modalNavigationStore, serviceProvider)
        {
            GetGroups();
        }

        private IEnumerable<PulseInSignalGroup> groups;

        public IEnumerable<PulseInSignalGroup> Groups => groups;
        private void GetGroups()
        {
            //var gdicSignals = SignalStore.GetSignals<PulseInSignal>(nameof(PulseInViewModel));

            groups = SignalStore.GetSignals<PulseInSignal>(VIEWNAME)
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
                                .OrderBy(x => x.GroupName)
                                .ToList();
        }
    }
}