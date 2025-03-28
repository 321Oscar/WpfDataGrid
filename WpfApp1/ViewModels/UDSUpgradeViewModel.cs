using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ERad5TestGUI.Helpers;
using ERad5TestGUI.Services;
using ERad5TestGUI.Stores;
using ERad5TestGUI.UDS;

namespace ERad5TestGUI.ViewModels
{
    public class UDSUpgradeViewModel : ViewModelBase
    {
        private const string UDSConfigPath = @".\Config\UDSConfig.xml";
        private ObservableCollection<IUDSServer> servers;
        private ICommand selectFileCommand;
        private ICommand readDIDCommand;
        private bool _udsRunning;
        private DIDInfo currentDID;
        private UpgradeID currentUpgradeType;
        private UDSConfig _udsConfig;
        private string didData;
        private UDSServerAbstract runningServer;
        public const int FAILFLAG = 0;
        public const int SUCFLAG = 1;
        public UDSUpgradeViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService) 
            : base(signalStore, deviceStore, logService)
        {
            servers = new ObservableCollection<IUDSServer>();
            _udsConfig = XmlHelper.DeserializeFromXml<UDSConfig>(UDSConfigPath);
            DIDInfos = _udsConfig.DIDInfos;
            UpgradeTypeSources = _udsConfig.UpGradeIDs;
            CurrentUpgradeType = _udsConfig.UpGradeIDs[0];

            DeviceStore.CurrentDeviceChanged += OnCurrentDeviceChanged;
        }

      

        public IEnumerable<DIDInfo> DIDInfos { get; set; }
        public string DIDData
        {
            get => didData;
            set => SetProperty(ref didData, value);
        }
        public IEnumerable<UpgradeID> UpgradeTypeSources
        {
            get;
            private set;
        }
        public ObservableCollection<IUDSServer> Servers { get => servers; }
        public bool UdsRunning
        {
            get => _udsRunning;
            set 
            {
                if (SetProperty(ref _udsRunning, value))
                {
                    RaiseCommandCanExecute();
                }
            }
        }
        public ICommand SelectFileCommand 
        { 
            get 
            { 
                if (selectFileCommand == null) 
                    selectFileCommand = new RelayCommand(SelectFile); 
                return selectFileCommand; 
            } 
        }
        public ICommand ReadDIDCommand 
        {
            get
            {
                if (readDIDCommand == null)
                    readDIDCommand = new AsyncRelayCommand(ReadDID, () => DeviceStore.HasDevice && !UdsRunning, AsyncRelayCommandOptions.None);
                return readDIDCommand;
            }
        }
        public DIDInfo CurrentDID
        {
            get => currentDID;
            set => SetProperty(ref currentDID, value);
        }

        public UpgradeID CurrentUpgradeType
        {
            get => currentUpgradeType;
            set => SetProperty(ref currentUpgradeType, value);
        }

        /// <summary>
        /// 正在执行的服务
        /// </summary>
        public UDSServerAbstract RunningServer { get => runningServer; set => SetProperty(ref runningServer, value); }

        private void OnCurrentDeviceChanged()
        {
            RaiseCommandCanExecute();
        }

        private void RaiseCommandCanExecute()
        {
            (ReadDIDCommand as IRelayCommand).NotifyCanExecuteChanged();
        }

        private void SelectFile()
        {

        }

        private async Task<int> ReadDID()
        {
            if (CurrentDID == null)
            {
                //Logger("未知DID");
                return FAILFLAG;
            }

            DIDData = null;

            MultipServer ms = new MultipServer(_udsConfig.NormalTimeout, _udsConfig.PendingTimeout,DeviceStore.CurrentDevice,LogService) 
            { Name = $"Read DID {CurrentDID}" };
            AddEventToServer(ms);

            ReadDataByIdServer readDID = new ReadDataByIdServer(CurrentUpgradeType.ResPhyID, CurrentUpgradeType.ReqPhyID, DeviceStore.CurrentDevice, LogService)
            {
                DIDInfo = CurrentDID,
            };
            ms.Add(readDID);/**/

            //ClearServers();
            AddServer(ms);

            DisEnableCtrl(true);

            var res = await ms.RunAsync();
            if (res.UDSResponse == UDSResponse.Positive)
            {
                DIDData = readDID.DIDInfo.Data;
                //SnackbarMessage?.Enqueue(DIDData);
            }
            else
            {
                //SnackbarMessage?.Enqueue("Read Fail");
            }
            //
            // ConsoleLog($"{res.UDSResponse},{res.Message}");

            DisEnableCtrl(false);

            return ((res != null) && (res.UDSResponse == UDSResponse.Positive)) ? SUCFLAG : FAILFLAG;
        }

        private void DisEnableCtrl(bool enable)
        {
            UdsRunning = enable;
        }

        private void AddServer(IUDSServer server)
        {
            this.Servers.Add(server);
            if (server is UDSServerAbstract binding)
                RunningServer = binding;
        }
        private void AddEventToServer(UDSServerAbstract server)
        {
            var progress = new Progress<ServerResult>(value => ReportProgress(value));
            server.Progress = progress;
            //server.Send += CAN_Send;
            //server.SendMultip += CANSendMultip;
            //server.RegisterRecieveEvent += RegisterRecieveEvent;
            //server.DebugLog += Server_DebugLog; ;
            //server.CanChannel = CanChannel;
            //server.IsCanFD = SendCanFD;
            server.IDExtended = CurrentUpgradeType.IDExtended;
            server.FillData = 0xaa;
            if (server is ERad5TestGUI.Interfaces.ISeedNKey ss)
            {
                ss.SeedKeyPath = _udsConfig.SeedNKeyPath;
            }
        }

        private void ReportProgress(ServerResult value)
        {
            //LPLogLevel l = LPLogLevel.INFO;
            switch (value.UDSResponse)
            {
                default:
                case UDSResponse.Init:
                case UDSResponse.Positive:
                case UDSResponse.FlowControl:
                case UDSResponse.Pass:
                    break;

                case UDSResponse.Unknow:
                case UDSResponse.SendFail:
                case UDSResponse.ParseError:
                case UDSResponse.Timeout:
                case UDSResponse.Negative:
                    //l = LPLogLevel.ERROR;
                    break;
            }
            //Logger(value.Message, l);
        }
    }
}
