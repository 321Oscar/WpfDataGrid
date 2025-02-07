using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.Models;
using WpfApp1.Services;
using WpfApp1.Stores;

namespace WpfApp1.ViewModels
{
    public class GDICViewModel : ViewModelBase
    {
        private readonly SignalStore _signalStore;

        public GDICViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService)
            : base(signalStore, deviceStore, logService)
        {
            _signalStore = signalStore;
            GetGDICStatusGroups();
        }

        //--Status-------------------------------------------------------------
        private IEnumerable<GDICStatusGroup> gDICStatusGroups;
        private GDICStatusGroup currentGDICStatusGroup;

        public IEnumerable<GDICStatusGroup> GDICStatusGroups => gDICStatusGroups;
        private void GetGDICStatusGroups()
        {
            var gdicSignals = _signalStore.GetSignals<GDICStatusSignal>();

            gDICStatusGroups = gdicSignals
            .GroupBy(s => s.GroupName)
            .Select(g =>
            {
                var classRoom = new GDICStatusGroup(g.Key);
                var signals = g.ToList();
                signals.Sort((x, y) =>
                {
                    return x.Name.CompareTo(y.Name);
                });
                classRoom.GDICStatusSignals.AddRange(signals);
                return classRoom;
            })
            .OrderBy(x=>x.GroupName)
            .ToList();
            //gDICStatusGroups.Sort
        }
        public GDICStatusGroup CurrentGDICStatusGroup 
        { 
            get => currentGDICStatusGroup; 
            set => SetProperty(ref currentGDICStatusGroup , value); 
        }
        //---------------------------------------------------------------
    }
}
