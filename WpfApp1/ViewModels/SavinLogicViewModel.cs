using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using ERad5TestGUI.Models;
using ERad5TestGUI.Services;
using ERad5TestGUI.Stores;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ERad5TestGUI.Helpers;
using Microsoft.WindowsAPICodePack.Dialogs;
using NPOI.SS.UserModel;

namespace ERad5TestGUI.ViewModels
{
    public class SavinLogicViewModel : SendFrameViewModelBase
    {
        //private List<ObservableObject> _datas;
        private RelayCommand _updateDirCommand;
        private ObservableCollection<SignalGroupBase> _level1Group = new ObservableCollection<SignalGroupBase>();
        private ObservableCollection<SignalGroupBase> _level2Group = new ObservableCollection<SignalGroupBase>();
        private ObservableCollection<SignalGroupBase> _level3Group = new ObservableCollection<SignalGroupBase>();
        private ObservableCollection<SignalGroupBase> _level4Group = new ObservableCollection<SignalGroupBase>();
        private ObservableCollection<SafingLogicDirectionSelect> _derectionSignals = new ObservableCollection<SafingLogicDirectionSelect>();
        public SavinLogicViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService) : base(signalStore, deviceStore, logService)
        {
            _ViewName = "Safing_Logic";
        }
        public SavinLogicViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService, ModalNavigationStore modalNavigationStore, IServiceProvider serviceProvider)
            : base(signalStore, deviceStore, logService, modalNavigationStore, serviceProvider)
        {
            //ChangeSignalInputOutputCommand = new RelayCommand<object>(ChangeSignalInputOutput);
            //_datas = new List<ObservableObject>();
            //Load();
            //load commands
            //LoadCommands();
        }

        public IEnumerable<SignalGroupBase> Level1Signals { get => _level1Group; }
        public IEnumerable<SignalGroupBase> Level2Signals { get => _level2Group; }
        public IEnumerable<SignalGroupBase> Level3Signals { get => _level3Group; }
        public IEnumerable<SignalGroupBase> Level4Signals { get => _level4Group; }
        public IEnumerable<SafingLogicDirectionSelect> DerectionSignals { get => _derectionSignals; }

        //public IEnumerable<SavingLogicSignal> SavingLogicSignals => SignalStore.GetObservableCollection<SavingLogicSignal>();
        //public IEnumerable<ObservableObject> SavingLogicGroups { get => _datas; }
        public ICommand UpdateDirCommand { get => _updateDirCommand??(_updateDirCommand = new RelayCommand(UpdateDir)); }
        public DiscreteOutputSignal Safing_Logic_Pin_Dir_Update => SignalStore.GetSignalByName<DiscreteOutputSignal>("Safing_Logic_Pin_Dir_Update", inOrOut: true);
        public override void Init()
        {
            if (_ViewName == null)
                _ViewName = "Safing_Logic";
            base.Init();
            Load();
        }

        private void Load()
        {
            InitTestResultSignals();
            LoadLevels(_level1Group, new string[] { "LOW_OC_U_FLT" }, new string[] { "LOW_PHASE_UVW_OC_FLT" });
            LoadLevels(_level2Group, new string[] { "LOW_SAFESTATE1_NXP" }, new string[] { "LOW_OUT_EN" });

            var dirSignals = SignalStore.GetSignalsByName<SafingLogicDirectionSignal>("_Dir", addToStore: false);
            foreach (var item in dirSignals.Where(x => x.Name.IndexOf("Select") > -1))
            {
                item.InOrOut = true;
                var currentDirSignal = dirSignals.FirstOrDefault(x => x.Name == item.DisplayName);
                if (currentDirSignal != null)
                {
                    SafingLogicDirectionSelect select = new SafingLogicDirectionSelect
                    {
                        Select = item,
                        CurrentDirection = currentDirSignal
                    };

                    SignalStore.AddSignal(item);
                    SignalStore.AddSignal(currentDirSignal);

                    _derectionSignals.Insert(0, select);
                }
            }
        }

        private void LoadLevels(ObservableCollection<SignalGroupBase> levelGroup, string[] inputSignals, string[] outputSignals)
        {
            DiscreteInputSignalGroup disInputGroup = new DiscreteInputSignalGroup("Inputs");
            foreach (var input in inputSignals)
            {
                // disInputGroup.Signals.Add(GetSignalAndAddViewName<DiscreteInputSignal>("LOW_OC_U_FLT"));
                disInputGroup.Signals.Add(GetSignalAndAddViewName<DiscreteInputSignal>(input));
            }

            //s.ViewName += this.ViewName;
            //disInputGroup.Signals.AddRange(SignalStore.GetSignals<DiscreteInputSignal>(ViewName));
            levelGroup.Add(disInputGroup);

            DiscreteOutputSignalGroup diOutGroup = new DiscreteOutputSignalGroup("Outputs");
            foreach (var output in outputSignals)
            {
                //diOutGroup.Signals.Add(GetSignalAndAddViewName<DiscreteOutputSignal>("LOW_PHASE_UVW_OC_FLT"));
                diOutGroup.Signals.Add(GetSignalAndAddViewName<DiscreteOutputSignal>(output));
            }

            //diOutGroup.Signals.AddRange(SignalStore.GetSignals<DiscreteOutputSignal>(ViewName));
            levelGroup.Add(diOutGroup);
            //gDICStatusGroups.Sort
        }

        private TSignal GetSignalAndAddViewName<TSignal>(string name)
            where TSignal : SignalBase, new()
        {
            var s = SignalStore.GetSignalByName<TSignal>(name);
            if (s != null)
                s.ViewName += this.ViewName;
            return s;
        }

        private void UpdateDir()
        {
            Safing_Logic_Pin_Dir_Update.OriginValue = 1;
            Send(SignalStore.BuildFrames(new SignalBase[] { Safing_Logic_Pin_Dir_Update}));
            Safing_Logic_Pin_Dir_Update.OriginValue = 0;
        }

        private void LoadCommands()
        {
            SavingLogicButtonSignalGroup g = new SavingLogicButtonSignalGroup("Pins");
            g.Signals.AddRange(SignalStore.GetSignals<SavingLogicButtonSignal>());
            //_datas.Add(g);
        }
        private void ChangeSignalInputOutput(object paramers)
        {
            if (paramers is SavingLogicButtonSignal inoutBtn)
            {
                inoutBtn.InputOrOut = !inoutBtn.InputOrOut;
            }
        }



        #region Safing logic Test
        private readonly string SafingLogicTableFilePath = @"./Config/SafingLogicTestTable_v11_PV.xlsx";
        private DiscreteOutputSignal _testProgress; 
        private readonly List<SignalBase> _testResult = new List<SignalBase>();
        private bool _isTest;
        private bool _getResult;
        private RelayCommand _startTestCommand;
        private RelayCommand _stopTestCommand;
        public DiscreteOutputSignal TestStart { get => SignalStore.GetSignalByName<DiscreteOutputSignal>("Safing_Logic_Test_Start"); }
        public DiscreteOutputSignal TestStop { get => SignalStore.GetSignalByName<DiscreteOutputSignal>("Safing_Logic_Test_Stop"); }
        public DiscreteOutputSignal TestProgress { get => _testProgress; }
        public SignalBase ErrRowCount { get => SignalStore.GetSignalByName<SignalBase>("Safing_Logic_Test_Error_RowCnt"); }
        //public int TestProgress { get => _testProgress; set => SetProperty(ref _testProgress, value); }
        //private List<SignalBase> _testStatus = new List<SignalBase>();
        public bool IsTest 
        { 
            get => _isTest;
            set 
            {
                if (SetProperty(ref _isTest, value))
                {
                    if (!value)
                    {
                        ReadExcelTest();
                    }
                    _startTestCommand.NotifyCanExecuteChanged();
                    _stopTestCommand.NotifyCanExecuteChanged();
                }

            }
        }
        public ICommand StartTestCommand => _startTestCommand ?? (_startTestCommand = new RelayCommand(StartTest, () => !IsTest));
        public ICommand StopTestCommand => _stopTestCommand ?? (_stopTestCommand = new RelayCommand(StopTest, () => IsTest));
        public bool TestFinish
        {
            get => SignalStore.GetSignalByName<SignalBase>("Safing_Logic_Test_Status").OriginValue == 2;
        }

        public List<SafingLogicTestResult> _safingLogicTestResults = new List<SafingLogicTestResult>();
        private void InitTestResultSignals()
        {
            _testProgress = SignalStore.GetSignalByName<DiscreteOutputSignal>("Safing_Logic_Test_Current_Row");

            _testResult.Add(SignalStore.GetSignalByName<SignalBase>("Safing_Logic_Test_Frame_Header", addToStore: false));
            _testResult.Add(SignalStore.GetSignalByName<SignalBase>("Safing_Logic_Test_Frame_Index", addToStore: false));
            _testResult.Add(SignalStore.GetSignalByName<SignalBase>("Safing_Logic_Test_Frame_Row", addToStore: false));
            _testResult.Add(SignalStore.GetSignalByName<SignalBase>("Safing_Logic_Test_Frame_FORCE_LOWERS_ON", addToStore: false));
            _testResult.Add(SignalStore.GetSignalByName<SignalBase>("Safing_Logic_Test_Frame_FORCE_UPPERS_ON", addToStore: false));
            _testResult.Add(SignalStore.GetSignalByName<SignalBase>("Safing_Logic_Test_Frame_FSENB", addToStore: false));
            _testResult.Add(SignalStore.GetSignalByName<SignalBase>("Safing_Logic_Test_Frame_FSSTATE_BOT", addToStore: false));
            _testResult.Add(SignalStore.GetSignalByName<SignalBase>("Safing_Logic_Test_Frame_FSSTATE_TOP", addToStore: false)); 
            _testResult.Add(SignalStore.GetSignalByName<SignalBase>("Safing_Logic_Test_Frame_OUT_EN", addToStore: false));
            _testResult.Add(SignalStore.GetSignalByName<SignalBase>("Safing_Logic_Test_Frame_PHASE_UVW_OC_FLT", addToStore: false));
            _testResult.Add(SignalStore.GetSignalByName<SignalBase>("Safing_Logic_Test_Frame_PWM_BUFFER", addToStore: false));
        }

        private async void StartTest()
        {
            //1.Send Safing_Logic_Test_Start to 1
            ChangeSignal(TestStart);
            IsTest = true;
            _safingLogicTestResults.Clear();
            _getResult = true;
            DeviceStore.OnMsgReceived += DeviceStore_OnMsgReceived;
            do
            {
                await Task.Delay(1000);
            } while (_getResult && IsTest);

            //5.load test.xls and save SafingLogicTestResult

            //save
            //ReadExcelTest();

            IsTest = false;
        }

        private void StopTest()
        {
            ChangeSignal(TestStop);
            IsTest = false;
            DeviceStore.OnMsgReceived -= DeviceStore_OnMsgReceived;
        }

        private void ChangeSignal(SignalBase signal)
        {
            signal.OriginValue = 1;
            Send(SignalStore.BuildFrames(new SignalBase[] { signal }));
            signal.OriginValue = 0;
        }
        private void GenarateResult()
        {
            var res = new SafingLogicTestResult();
            res.Result.Add("Safing_Logic_Test_Frame_Header",0x10);
            res.Result.Add("Safing_Logic_Test_Frame_Index", 0x21);
            res.Result.Add("Safing_Logic_Test_Frame_Row", 0x10);
            res.Result.Add("Safing_Logic_Test_Frame_FORCE_UPPERS_ON", 0);
            res.Result.Add("Safing_Logic_Test_Frame_FORCE_LOWERS_ON", 1);
            res.Result.Add("Safing_Logic_Test_Frame_PWM_BUFFER", 1);
            res.Result.Add("Safing_Logic_Test_Frame_FSSTATE_BOT", 1);
            res.Result.Add("Safing_Logic_Test_Frame_FSSTATE_TOP", 0);
            res.Result.Add("Safing_Logic_Test_Frame_FSENB", 1);
            res.Result.Add("Safing_Logic_Test_Frame_OUT_EN", 0);
            res.Result.Add("Safing_Logic_Test_Frame_PHASE_UVW_OC_FLT", 0);
            _safingLogicTestResults.Add(res);
        }
        private void ReadExcelTest()
        {
            IWorkbook wb = NPOIHelper.GetWorkbookByExcel(SafingLogicTableFilePath);

            ISheet sheet = wb.GetSheetAt(0);
            var headerRow = sheet.GetRow(0);
            Dictionary<string, int> headerIndex = new Dictionary<string, int>();
            for (int i = 0; i < headerRow.LastCellNum; i++)
            {
                headerIndex.Add(headerRow.GetCell(i).StringCellValue, i);
            }
            //GenarateResult();
            foreach (var testResult in _safingLogicTestResults)
            {
                var rowIndex = testResult.RowIndex;
                var row = sheet.GetRow(rowIndex);
                foreach (var kv in headerIndex)
                {
                    if (testResult.TryGetValue(kv.Key, out int val))
                    {
                        ICell cell = row.GetCell(kv.Value);
                        if (cell.NumericCellValue != val)
                        {
                            cell.SetCellValue(val);
                            cell.CellStyle = NPOIHelper.GetCellStyle(wb);
                        }
                    }
                }
            }

            var saveFileDialog = new CommonSaveFileDialog
            {
                DefaultFileName = $"SafingLogic Test Result {DateTime.Now:yy-MM-dd-HH-mm}.xlsx"
            };
            saveFileDialog.Filters.Add(new CommonFileDialogFilter("excel(2007) file", "*.xlsx"));
            saveFileDialog.Filters.Add(new CommonFileDialogFilter("excel(2003) file", "*.xls"));
            if (saveFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                try
                {
                    NPOIHelper.WriteExcel(saveFileDialog.FileName, wb);
                    AdonisUI.Controls.MessageBox.Show($"Save Test File to {saveFileDialog.FileName}",
                        "SafingLogic Test Result Table Save",
                        icon:AdonisUI.Controls.MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    AdonisUI.Controls.MessageBox.Show($"Save Test File to Error:{ex.Message}",
                        "SafingLogic Test Result Table Save",
                        icon: AdonisUI.Controls.MessageBoxImage.Error);
                }
                
            }
        }
        private void DeviceStore_OnMsgReceived(IEnumerable<Devices.IFrame> can_msg)
        {
            var msgs611 = can_msg.Where(x => x.MessageID == 0x611);
            if (msgs611 != null)
            {
                if (TestFinish)
                {
                    foreach (var msg611 in msgs611)
                    {
                        SafingLogicTestResult res = new SafingLogicTestResult();
                        foreach (var item in SignalStore.ParseMsgYield(msg611, _testResult))
                        {
                            res.Result.Add(item.Name, (int)item.OriginValue);
                        }
                        if (res.RowIndex == 0)
                        {
                            _getResult = false;
                            DeviceStore.OnMsgReceived -= DeviceStore_OnMsgReceived;
                            break;
                        }
                        _safingLogicTestResults.Add(res);
                    }
                }
            }
        }

        #endregion
    }

    public class SafingLogicTestResult
    {
        public SafingLogicTestResult()
        {
            Result = new Dictionary<string, int>();
        }
        public int RowIndex
        {
            get
            {
                if (Result.TryGetValue("Safing_Logic_Test_Frame_Row", out int rowIndex))
                {
                    return rowIndex;
                }
                return -1;
            }
        }
        public Dictionary<string, int> Result { get; }

        public bool TryGetValue(string name, out int val)
        {
            name = name.Replace(" ", "").Replace("\n","_").Replace("/", "");
            name = $"Safing_Logic_Test_Frame_{name}";
            return Result.TryGetValue(name, out val);
        }
    }
}
