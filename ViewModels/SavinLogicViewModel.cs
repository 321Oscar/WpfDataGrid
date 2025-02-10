using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using WpfApp1.Models;
using WpfApp1.Services;
using WpfApp1.Stores;

namespace WpfApp1.ViewModels
{
    public class SavinLogicViewModel : ViewModelBase
    {
        private List<ObservableObject> datas;

        public SavinLogicViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService) 
            : base(signalStore, deviceStore, logService)
        {
            ChangeSignalInputOutputCommand = new RelayCommand<object>(ChangeSignalInputOutput);
            datas = new List<ObservableObject>();
            Load();
            //load commands
            LoadCommands();
        }

        public IEnumerable<SavingLogicSignal> SavingLogicSignals => SignalStore.GetObservableCollection<SavingLogicSignal>();
        public IEnumerable<ObservableObject> SavingLogicGroups { get => datas; }
        public ICommand ChangeSignalInputOutputCommand { get; }
        private void Load()
        {
            var gdicSignals = SignalStore.GetSignals<SavingLogicSignal>();

            datas.AddRange( gdicSignals
            .GroupBy(s => s.GroupName)
            .Select(g =>
            {
                var classRoom = new SavingLogicInOutGroup(g.Key);
                //classRoom.InOutGroups = new List<>();
                var signals = g.ToList();
                signals.Sort((x, y) =>
                {
                    return x.Name.CompareTo(y.Name);
                });
                SavingLogicGroup savingLogicInputGroup = new SavingLogicGroup(g.Key + "Input");
                classRoom.InOutGroups.Add(savingLogicInputGroup);
                savingLogicInputGroup.Signals.AddRange(signals.Where(x => x.InputOrOutput == true));
                SavingLogicGroup savingLogicOutputGroup = new SavingLogicGroup(g.Key + "Output");
                classRoom.InOutGroups.Add(savingLogicOutputGroup);
                savingLogicOutputGroup.Signals.AddRange(signals.Where(x => x.InputOrOutput == false));
                return classRoom;
            })
            .OrderBy(x => x.SignalName)
            .ToList());
            //gDICStatusGroups.Sort
        }

        private void LoadCommands()
        {
            SavingLogicButtonSignalGroup g = new SavingLogicButtonSignalGroup("Pins");
            g.Signals.AddRange(SignalStore.GetSignals<SavingLogicButtonSignal>());
            datas.Add(g);
        }
        private void ChangeSignalInputOutput(object paramers)
        {
            if(paramers is SavingLogicButtonSignal inoutBtn)
            {
                inoutBtn.InputOrOut = !inoutBtn.InputOrOut;
            }
        }
    }
}
