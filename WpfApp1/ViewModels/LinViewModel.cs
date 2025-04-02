using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using ERad5TestGUI.Models;
using ERad5TestGUI.Services;
using ERad5TestGUI.Stores;

namespace ERad5TestGUI.ViewModels
{
    public class LinViewModel : SendFrameViewModelBase
    {
        private RelayCommand _updateCommand;

        public LinViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService) : base(signalStore, deviceStore, logService)
        {
            //deviceStore.OnMsgReceived += DeviceStore_OnMsgReceived;
        }

        public LinViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService, ModalNavigationStore modalNavigationStore, IServiceProvider serviceProvider) 
            : base(signalStore, deviceStore, logService, modalNavigationStore, serviceProvider)
        {
        }

        public LinConfigSignal SendID { get => SignalStore.GetSignals<LinConfigSignal>().FirstOrDefault(x => x.Name == "LIN_Send_ID"); }
        public LinConfigSignal SendLength { get => SignalStore.GetSignals<LinConfigSignal>().FirstOrDefault(x => x.Name == "LIN_Send_Length"); }
        public LinConfigSignal SendChecksumType { get => SignalStore.GetSignals<LinConfigSignal>().FirstOrDefault(x => x.Name == "LIN_Send_Checksum_Type"); } 
        public LinConfigSignal ReceiveID { get => SignalStore.GetSignals<LinConfigSignal>().FirstOrDefault(x => x.Name == "LIN_Receive_ID"); }
        public LinConfigSignal ReceiveLength { get => SignalStore.GetSignals<LinConfigSignal>().FirstOrDefault(x => x.Name == "LIN_Receive_Length"); }
        public LinConfigSignal ReceiveChecksumType { get=> SignalStore.GetSignals<LinConfigSignal>().FirstOrDefault(x => x.Name == "LIN_Receive_Checksum_Type");}

        public Models.LinData SendLinData { get; private set; }
        public Models.LinData ReceiveLinData { get; private set; }
        public ICommand UpdateCommand { get => _updateCommand ?? (_updateCommand = new RelayCommand(Update)); }

        public override void Init()
        {
            var linConfigDataSignals = SignalStore.GetSignals<LinConfigSignal>(ViewName).Where(x => x.Name.IndexOf("Data") > -1);
            SendLinData = new LinData();
            ReceiveLinData = new LinData();
            for (int i = 0; i < SendLinData.Data.Length; i++)
            {
                SendLinData.Data[i] = linConfigDataSignals.FirstOrDefault(x => x.InOrOut && x.Name.IndexOf($"Data{i}") > -1);
                SendLinData.Data[i].OriginValue = 0;
                ReceiveLinData.Data[i] = linConfigDataSignals.FirstOrDefault(x => !x.InOrOut && x.Name.IndexOf($"Data{i}") > -1);
                ReceiveLinData.Data[i].OriginValue = 0;
            }
        }

        private void Update()
        {
            //Send();
            SendFD(SignalStore.BuildFrames(SignalStore.GetSignals<LinConfigSignal>().Where(x => x.InOrOut)));
        }


        [Obsolete]
        private void DeviceStore_OnMsgReceived(System.Collections.Generic.IEnumerable<Devices.IFrame> can_msg)
        {
            if (ReceiveID == null)
                return;
            foreach (var item in can_msg)
            {
                if (item.MessageID == ReceiveID.OriginValue)
                {
                    //receive lin data

                    //_receiveLinData.Add();
                }
            }
        }
    }
}
