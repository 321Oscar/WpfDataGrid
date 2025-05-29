using CommunityToolkit.Mvvm.Input;
using ERad5TestGUI.Services;
using ERad5TestGUI.Stores;
using ERad5TestGUI.UDS;
using ERad5TestGUI.UDS.SRecord;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ERad5TestGUI.ViewModels
{
    public class MemoryViewModel : ViewModelBase
    {
        private readonly int MaxAddr = 0x200000;
        private readonly ObservableCollection<IUDSServer> _servers = new ObservableCollection<IUDSServer>();
        private readonly ObservableCollection<UDS.SRecord.SrecDataOnly> _s19Records = new ObservableCollection<UDS.SRecord.SrecDataOnly>();
        private AsyncRelayCommand _loadSrecFileCommand;
        private AsyncRelayCommand _readMemoryCommand;
        private AsyncRelayCommand _readAllMemoryCommand;
        private AsyncRelayCommand _writeMemoryCommand;
        private AsyncRelayCommand _writeAllMemoryCommand;
        private AsyncRelayCommand _saveAsSrecCommand;
        private RelayCommand _cancelUdsServersCommand;
        private RelayCommand _clearUdsServersCommand;
        private UDSServerAbstract _runningServer;
        private int erad5MemoryValue;
        private bool _udsRunning;
        private int erad5MemoryWriteAddr;
        private int erad5MemoryAddr;
        private SrecFileOnlyData srecFileOnlyData;

        public MemoryViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService) : base(signalStore, deviceStore, logService)
        {
            S19Records = new ReadOnlyObservableCollection<UDS.SRecord.SrecDataOnly>(_s19Records);
        }
        public MemoryViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService, ModalNavigationStore modalNavigationStore, IServiceProvider serviceProvider)
            : base(signalStore, deviceStore, logService, modalNavigationStore, serviceProvider)
        {
        }

        public ICommand LoadSrecFileCommand => _loadSrecFileCommand ?? (_loadSrecFileCommand = new AsyncRelayCommand(LoadSrecFile, () => !UdsRunning));
        public ICommand ReadCommand => _readMemoryCommand ?? (_readMemoryCommand = new AsyncRelayCommand(ReadMemory, () => !UdsRunning));
        public ICommand ReadAllCommand => _readAllMemoryCommand ?? (_readAllMemoryCommand = new AsyncRelayCommand(ReadMemoryAll, () => !UdsRunning));
        public ICommand WriteCommand => _writeMemoryCommand ?? (_writeMemoryCommand = new AsyncRelayCommand(WriteMemory, () => !UdsRunning));
        public ICommand WriteAllCommand => _writeAllMemoryCommand ?? (_writeAllMemoryCommand = new AsyncRelayCommand(WriteMemoryAll, () => !UdsRunning));
        public ICommand SaveAsSrecCommand => _saveAsSrecCommand ?? (_saveAsSrecCommand = new AsyncRelayCommand(SaveAsSrecAsync, () => !UdsRunning));
        public ICommand CancelUdsServersCommand => _cancelUdsServersCommand ?? (_cancelUdsServersCommand = new RelayCommand(CancelUds, () => UdsRunning));
        public ICommand ClearUdsServersCommand => _clearUdsServersCommand ?? (_clearUdsServersCommand = new RelayCommand(ClearUDSServers, () => !UdsRunning));
        public ReadOnlyObservableCollection<UDS.SRecord.SrecDataOnly> S19Records { get; set; }

        public UDS.UDSConfig _udsConfig => SignalStore.UDSConfig;
        public UpgradeID CurrentUpgradeType => _udsConfig.UpGradeIDs[0];
        public bool UdsRunning 
        { 
            get => _udsRunning;
            set 
            { 
                SetProperty(ref _udsRunning, value);
                _loadSrecFileCommand.NotifyCanExecuteChanged();
                _readMemoryCommand.NotifyCanExecuteChanged();
                _writeMemoryCommand.NotifyCanExecuteChanged();
                _readAllMemoryCommand.NotifyCanExecuteChanged();
                _writeAllMemoryCommand.NotifyCanExecuteChanged();
                _saveAsSrecCommand.NotifyCanExecuteChanged();
                _cancelUdsServersCommand.NotifyCanExecuteChanged();
                _clearUdsServersCommand.NotifyCanExecuteChanged();
            }
        }
        public UDSServerAbstract RunningServer { get => _runningServer; set => SetProperty(ref _runningServer, value); }
        public int Erad5MemoryAddr
        {
            get => erad5MemoryAddr;
            set
            {
                if (value > MaxAddr - 4)
                {
                    erad5MemoryAddr = MaxAddr - 4;
                }
                else
                {
                    erad5MemoryAddr = value;
                }
            }
        }
        public int Erad5MemoryValue { get => erad5MemoryValue; set => SetProperty(ref erad5MemoryValue, value); }
        public int Erad5MemoryWriteAddr
        {
            get => erad5MemoryWriteAddr;
            set
            {
                if (value > MaxAddr - 4)
                {
                    erad5MemoryWriteAddr = MaxAddr - 4;
                }
                else
                {
                    erad5MemoryWriteAddr = value;
                }
            }
        }
        public int Erad5MemoryWriteValue { get; set; }
        public SrecFileOnlyData SrecFileOnlyData { get => srecFileOnlyData; set => SetProperty(ref srecFileOnlyData, value); }
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
                    SrecFileOnlyData = new UDS.SRecord.SrecFileOnlyData(ofd.FileName);
                    //Dispatch(() =>
                    //{
                    //    if(SrecFileOnlyData == null)
                    //    _s19Records.Clear();
                    //    foreach (var sData in f.Content)
                    //    {
                    //        _s19Records.Add(sData);
                    //    }
                    //});
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
            cancelSource = new CancellationTokenSource();

            var res = await ReadMemoryByLengthAsync(Erad5MemoryAddr, 0x04);
            //var suc = res.UDSResponse == UDSResponse.Positive;
            if (res != null)
            {
                //var data = readMemory.UploadData.FirstOrDefault().Value;
                var data = res.Reverse();
                try
                {
                    Erad5MemoryValue = BitConverter.ToInt32(data.ToArray(), 0);
                    ShowMsgInfoBox($"Success : {Erad5MemoryValue:X8}", caption: "Read Memory");
                }
                catch (Exception ex)
                {
                    ShowMsgErrorBox($"Read {Erad5MemoryAddr:X} Data : {string.Join("", res.Select(x => x.ToString("X2")))} Error : {ex.Message}",
                        caption: "Read Memory");
                }
            }
            else
            {
                ShowMsgErrorBox($"Read 0x{Erad5MemoryAddr:X} Memory Fail",
                    caption: "Read Memory");
            }

            return 1;
        }

        private async Task<int> ReadMemoryAll()
        {
            int totalLength = 1024 * 1024 * 2;
            int startAddr = 0x00;
            if (SrecFileOnlyData == null) SrecFileOnlyData = new SrecFileOnlyData();
            SrecFileOnlyData.Content.Clear();
#if DEBUG
            byte[] byteArray = new byte[totalLength];
            // 填充数组（这里可以根据需要填充数据）
            new Random().NextBytes(byteArray);
            //await Task.Run(() =>
            //{
                Dispatch(() =>
                {
                    SrecFileOnlyData.InsertData(byteArray, (uint)startAddr);
                });
            //});
            return 1;
#endif
            int step = 1024 * 100;
            int count = totalLength / step;
            
            int retry = 0;
            bool fail = false;
            cancelSource = new CancellationTokenSource();
            for (int i = 0; i < count; i++)
            {
                do
                {
                    if (cancelSource != null && cancelSource.IsCancellationRequested)
                        break;

                    var data = await ReadMemoryByLengthAsync(startAddr, step);

                    if (data != null)
                    {
                        fail = false;
                        retry = 0;
                        await Task.Run(() =>
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                SrecFileOnlyData.InsertData(data.ToArray(), (uint)startAddr);
                            });
                        });
                        break;
                    }
                    else
                    {
                        fail = true;
                        retry++;
                    }
                } while (retry < 3 && fail);

                if (retry == 3 || fail)
                {
                    ShowMsgErrorBox($"Read 0x{startAddr:X} Memory Fail / Cancel", caption: "Read Memory");
                    return -1;
                }

                startAddr += step;
            }

            ShowMsgInfoBox("Read All Memory(0x00~0x200000) Success!", "Read Memory");

            return 1;
        }

        private async Task<int> WriteMemory()
        {
            var data = BitConverter.GetBytes(Erad5MemoryWriteValue).Reverse().ToArray();
            cancelSource = new CancellationTokenSource();
            var res = await WriteMemoryByLengthAsync(new Dictionary<int, byte[]>() { { Erad5MemoryWriteAddr, data } }); ;

            return res;
        }

        private async Task<int> WriteMemoryAll()
        {
            cancelSource = new CancellationTokenSource();
            if (SrecFileOnlyData == null || SrecFileOnlyData.SrecFile == null)
            {
                if (AdonisUI.Controls.MessageBox.Show("No S19 File Loaded, it will write random data. Click OK to Write.",
                    caption:"Write Memory",
                    icon: AdonisUI.Controls.MessageBoxImage.Information,
                    buttons: AdonisUI.Controls.MessageBoxButton.OKCancel) == AdonisUI.Controls.MessageBoxResult.OK)
                {
                    byte[] byteArray = new byte[2 * 1024 * 1024];
                    // 填充数组（这里可以根据需要填充数据）
                    new Random().NextBytes(byteArray);

                    return await WriteMemoryByLengthAsync(new Dictionary<int, byte[]>() { { 0x00, byteArray } });
                }
                else
                {
                    return 1;
                }
            }
            //return -1;
            return await WriteMemoryByLengthAsync(this.SrecFileOnlyData.SrecFile.AddrData);
        }

        private Task<int> WriteMemoryByLengthAsync(Dictionary<uint, List<byte>> addrAndDatas)
        {
            Dictionary<int, byte[]> addrAndDatasArray = new Dictionary<int, byte[]>();

            foreach (var item in addrAndDatas)
            {
                addrAndDatasArray.Add((int)item.Key, item.Value.ToArray());
            }
            return WriteMemoryByLengthAsync(addrAndDatasArray);
        }
        private CancellationTokenSource cancelSource;
        private async Task<IEnumerable<byte>> ReadMemoryByLengthAsync(int startAddr, int length)
        {
            
            var readMemory = new Erad5ReadMemoryServer(_udsConfig.NormalTimeout, _udsConfig.PendingTimeout, this.DeviceStore.CurrentDevice, LogService,
                $"Read Memory : 0x{startAddr:X},length:0x{length:X}")
            {
                FunctionID = CurrentUpgradeType.ReqFunID,
                PhyID_Req = CurrentUpgradeType.ReqPhyID,
                PhyID_Res = CurrentUpgradeType.ResPhyID
            };
            AddEventToServer(readMemory);

            readMemory.BinTmpFile = new BinTmpFile(new BinDataSegment(startAddr, length, null));
            readMemory.LoadServers();

            AddServer(readMemory);
            UdsRunning = true;
            ServerResult res = null;
            res = await readMemory.RunAsync(cancelSource);
            var suc = false;
            if (res != null)
                suc = res.UDSResponse == UDSResponse.Positive;
            UdsRunning = false;
            if (suc)
            {
                return readMemory.UploadData.FirstOrDefault().Value;
            }

            return null;
        }

        private async Task SaveAsSrecAsync()
        {
            if (SrecFileOnlyData == null ||( SrecFileOnlyData.SrecFile == null && SrecFileOnlyData.Content.Count == 0))
            {
                return;
            }

            CommonSaveFileDialog sfd = new CommonSaveFileDialog();
            sfd.Filters.Add(new CommonFileDialogFilter("srec file", "*.srec;*.s19;*.s28;*.s37"));
            //sfd.DefaultDirectory 
            if (sfd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string saveFilePath = sfd.FileName;
                string errMsg = string.Empty;
                bool res = await Task.Run(() =>
                {
                    try
                    {
                        if (SrecFileOnlyData.SrecFile == null)
                        {
                            SrecFileOnlyData.SrecFile = new SrecFile(type: "S3", datalength: 0x20);
                        }
                        List<byte> allData = new List<byte>();
                        foreach (var item in SrecFileOnlyData.Content)
                        {
                            //SrecFileOnlyData.Content
                            allData.AddRange(item.Data);
                        }
                        ByteArrayToSrecFile(allData.ToArray(), saveFilePath);
                        //SrecFileOnlyData.SrecFile.Add(allData.ToArray(), startPosition: 0x00);
                        //SrecFileOnlyData.SrecFile.Output(saveFilePath);
                    }
                    catch (Exception ex)
                    {
                        errMsg = ex.Message;
                        return false;
                    }

                    return true;
                });
                if (res)
                {
                    ShowMsgInfoBox($"Save Srec File to [{saveFilePath}] Success.", caption: "Save Srec File");
                }
                else
                {
                    ShowMsgErrorBox($"Save Srec File Fail: {errMsg}", caption: "Save Srec File");
                }
            }

            return;
        }

        public static void ByteArrayToSrecFile(byte[] data, string filePath)
        {
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(filePath))
            {
                int addressSize = 4; // S3记录使用32位地址
                int bytesPerLine = 0x20 + 4 + 2; // 每行最多包含的字节数，可以根据需要调整
                int maxDataBytes = bytesPerLine - (addressSize + 2); // 地址和校验和占用的空间

                for (int i = 0; i < data.Length; i += maxDataBytes)
                {
                    int length = Math.Min(maxDataBytes, data.Length - i);
                    byte[] lineData = new byte[length];
                    Array.Copy(data, i, lineData, 0, length);

                    uint address = (uint)(i); // 假设地址从0开始
                    string srecLine = CreateS3Record(lineData, address, addressSize);
                    writer.WriteLine(srecLine);
                }

                // 写入结束记录 S7
                writer.WriteLine("S70500000000FA");
            }
        }

        private static string CreateS3Record(byte[] data, uint address,int addressSize)
        {
            int byteCount = data.Length + addressSize + 1; // 数据长度+地址长度+校验和长度
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("S3"); // S3记录标识
            sb.Append(byteCount.ToString("X2")); // 字节计数
            sb.Append(address.ToString("X8")); // 地址

            int checksum = byteCount;
            foreach (byte b in BitConverter.GetBytes(address))
            {
                checksum += b;
                //sb.Append(b.ToString("X2"));
            }

            foreach (byte b in data)
            {
                checksum += b;
                sb.Append(b.ToString("X2"));
            }

            checksum = (~checksum); // 取反加一得到补码
            sb.Append(((byte)checksum).ToString("X2")); // 校验和

            return sb.ToString();
        }

        private void CancelUds()
        {
            if (cancelSource != null)
            {
                cancelSource.Cancel();
                //cancelSource = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="addrAndDatas"></param>
        /// <returns>1 & -1</returns>
        private async Task<int> WriteMemoryByLengthAsync(Dictionary<int, byte[]> addrAndDatas)
        {
            var writeMemory = new Erad5WriteMemoryServer(_udsConfig.NormalTimeout, _udsConfig.PendingTimeout, this.DeviceStore.CurrentDevice, LogService)
            {
                FunctionID = CurrentUpgradeType.ReqFunID,
                PhyID_Req = CurrentUpgradeType.ReqPhyID,
                PhyID_Res = CurrentUpgradeType.ResPhyID
            };
            AddEventToServer(writeMemory);

            List<BinDataSegment> segements = new List<BinDataSegment>();
            foreach (var addrAndData in addrAndDatas)
            {
                segements.Add(new BinDataSegment(addrAndData.Key, addrAndData.Value.Length, addrAndData.Value));
            }
            writeMemory.BinTmpFile = new BinTmpFile(segements.ToArray());
            writeMemory.LoadServers();

            AddServer(writeMemory);
            UdsRunning = true;
            var res = await writeMemory.RunAsync(cancelSource);
            UdsRunning = false;
            var suc = res.UDSResponse == UDSResponse.Positive;
            if (suc)
            {
                ShowMsgInfoBox("Write Memory Success", "Write Memory");
                return 1;
            }
            ShowMsgErrorBox("Write Memory Fail", "Write Memory");
            return -1;
        }

        private void ClearUDSServers()
        {
            Servers.Clear();
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
            server.IsCanFD = true;
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
