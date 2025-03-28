using CommunityToolkit.Mvvm.Input;
using ERad5TestGUI.Services;
using ERad5TestGUI.Stores;
using ERad5TestGUI.UDS;
using ERad5TestGUI.UDS.SRecord;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ERad5TestGUI.ViewModels
{
    public class MemoryViewModel : ViewModelBase
    {
        private readonly ObservableCollection<IUDSServer> _servers = new ObservableCollection<IUDSServer>();
        private readonly ObservableCollection<UDS.SRecord.SrecDataOnly> _s19Records = new ObservableCollection<UDS.SRecord.SrecDataOnly>();
        private AsyncRelayCommand _loadSrecFileCommand;
        private AsyncRelayCommand _readMemoryCommand;
        private AsyncRelayCommand _writeMemoryCommand;
        private UDSServerAbstract _runningServer;
        private int erad5MemoryValue;

        public MemoryViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService) : base(signalStore, deviceStore, logService)
        {
            S19Records = new ReadOnlyObservableCollection<UDS.SRecord.SrecDataOnly>(_s19Records);
        }
        public MemoryViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService, ModalNavigationStore modalNavigationStore, IServiceProvider serviceProvider)
            : base(signalStore, deviceStore, logService, modalNavigationStore, serviceProvider)
        {
        }

        public ICommand LoadSrecFileCommand => _loadSrecFileCommand ?? (_loadSrecFileCommand = new AsyncRelayCommand(LoadSrecFile));
        public ICommand ReadCommand => _readMemoryCommand ?? (_readMemoryCommand = new AsyncRelayCommand(ReadMemory));
        public ICommand WriteCommand => _writeMemoryCommand ?? (_writeMemoryCommand = new AsyncRelayCommand(WriteMemory));
        public ReadOnlyObservableCollection<UDS.SRecord.SrecDataOnly> S19Records { get; set; }

        public UDS.UDSConfig _udsConfig => SignalStore.UDSConfig;
        public UpgradeID CurrentUpgradeType => _udsConfig.UpGradeIDs[0];
        public bool UdsRunning { get; set; }
        public UDSServerAbstract RunningServer { get => _runningServer; set => SetProperty(ref _runningServer, value); }
        public int Erad5MemoryAddr { get; set; }
        public int Erad5MemoryValue { get => erad5MemoryValue; set => SetProperty(ref erad5MemoryValue, value); }
        public int Erad5MemoryWriteAddr { get; set; }
        public int Erad5MemoryWriteValue { get; set; }
        public SrecFileOnlyData SrecFileOnlyData { get; set; }
        public ObservableCollection<IUDSServer> Servers { get => _servers; }
        private Task LoadSrecFile()
        {
            var ofd = new CommonOpenFileDialog();
            ofd.Filters.Add(new CommonFileDialogFilter("srec file", "*.srec;*.s19;*.s28;*.s37"));
            ofd.Filters.Add(new CommonFileDialogFilter("all", "*.*"));

            if (ofd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                return Task.Run(() =>
                {
                    //S19RecordFile s19File = new S19RecordFile();
                    //S19RecordFile.ParseS19File(ofd.FileName, S19Records);
                    IsLoading = true;
                    UDS.SRecord.SrecFileOnlyData f = new UDS.SRecord.SrecFileOnlyData(ofd.FileName);
                    Dispatch(() =>
                    {
                        _s19Records.Clear();
                        foreach (var sData in f.Content)
                        {
                            _s19Records.Add(sData);
                        }
                    });
                    return;
                }).ContinueWith((x) =>
                {
                    IsLoading = false;
                });
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        private async Task<int> ReadMemory()
        {
            var readMemory = new Erad5ReadMemoryServer(_udsConfig.NormalTimeout, _udsConfig.PendingTimeout, this.DeviceStore.CurrentDevice, LogService)
            {
                FunctionID = CurrentUpgradeType.ReqFunID,
                PhyID_Req = CurrentUpgradeType.ReqPhyID,
                PhyID_Res = CurrentUpgradeType.ResPhyID
            };
            AddEventToServer(readMemory);

            readMemory.BinTmpFile = new BinTmpFile(new BinDataSegment(Erad5MemoryAddr, dataLength: 0x04, null));
            readMemory.LoadServers();

            AddServer(readMemory);
            UdsRunning = true;
            var res = await readMemory.RunAsync();
            var suc = res.UDSResponse == UDSResponse.Positive;
            if (suc)
            {
                var data = readMemory.UploadData.FirstOrDefault().Value;
                if (data.Count > 4)
                {
                    if (SrecFileOnlyData == null) SrecFileOnlyData = new SrecFileOnlyData();

                    foreach (var addrAndData in readMemory.UploadData)
                    {
                        SrecFileOnlyData.AddData(addrAndData.Key, addrAndData.Value.ToArray());
                    }
                }
                else
                {
                    data.Reverse();
                    try
                    {
                        Erad5MemoryValue = BitConverter.ToInt32(data.ToArray(), 0);
                    }
                    catch (Exception ex)
                    {
                        LogService.Error($"Read {Erad5MemoryAddr:X}:{string.Join("", readMemory.UploadData.FirstOrDefault().Value.Select(x => x.ToString("X2")))}", ex);
                    }
                }
            }
            else
            {
                LogService.Info($"read 0x{Erad5MemoryAddr:X} Memory fail");
            }
            UdsRunning = false;
            return 1;
        }

        private async Task<int> WriteMemory()
        {
            var writeMemory = new Erad5WriteMemoryServer(_udsConfig.NormalTimeout, _udsConfig.PendingTimeout, this.DeviceStore.CurrentDevice, LogService)
            {
                FunctionID = CurrentUpgradeType.ReqFunID,
                PhyID_Req = CurrentUpgradeType.ReqPhyID,
                PhyID_Res = CurrentUpgradeType.ResPhyID
            };
            AddEventToServer(writeMemory);
            var data = BitConverter.GetBytes(Erad5MemoryWriteValue).Reverse().ToArray();

            //data = new byte[4];

            writeMemory.BinTmpFile = new BinTmpFile(new BinDataSegment(Erad5MemoryWriteAddr, dataLength: data.Length, data));
            writeMemory.LoadServers();

            AddServer(writeMemory);

            var res = await writeMemory.RunAsync();

            return 1;
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
            //server.IDExtended = CurrentUpgradeType.IDExtended;
            //server.FillData = 0xaa;
            //if (server is ERad5TestGUI.Interfaces.ISeedNKey ss)
            //{
            //    ss.SeedKeyPath = _udsConfig.SeedNKeyPath;
            //}
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

    public class NXPFlashViewModel : ViewModelBase
    {
        public NXPFlashViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService, ModalNavigationStore modalNavigationStore, IServiceProvider serviceProvider) : base(signalStore, deviceStore, logService, modalNavigationStore, serviceProvider)
        {
        }
    }
}
