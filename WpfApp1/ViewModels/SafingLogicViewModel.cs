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
    public class SafingLogicViewModel : SendFrameViewModelBase
    {
        //private List<ObservableObject> _datas;
        private RelayCommand _updateDirCommand;
        private ObservableCollection<SignalGroupBase> _level1Group = new ObservableCollection<SignalGroupBase>();
        private ObservableCollection<SignalGroupBase> _level2Group = new ObservableCollection<SignalGroupBase>();
        private ObservableCollection<SignalGroupBase> _level3Group = new ObservableCollection<SignalGroupBase>();
        private ObservableCollection<SignalGroupBase> _level4Group = new ObservableCollection<SignalGroupBase>();
        private ObservableCollection<SafingLogicDirectionSelect> _derectionSignals = new ObservableCollection<SafingLogicDirectionSelect>();
        public SafingLogicViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService) : base(signalStore, deviceStore, logService)
        {
            _ViewName = "Safing_Logic";
        }
        public SafingLogicViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService, ModalNavigationStore modalNavigationStore, IServiceProvider serviceProvider)
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
            LoadLevels(_level1Group,
                new Dictionary<(string, string), bool>
                {
                    /***/
                    { ("LOW_SAFESTATE1_NXP"      , "")                       , false },
                    { ("LOW_SAFESTATE2_NXP"      , "")                       , false },
                    { ("E_STOP_MAIN_MICRO_OUTPUT", "LOW_E_STOP_MAIN_MICRO")  , false },
                    { ("PWM_EN"                  , "")                       , true  },
                    { ("LOW_F_RESET_NXP"         , "")                       , true  },
                    { ("LOW_HVOV_PHOC_LATCH_CLR" , "")                       , true  },
                    { ("SPD_HW_3PS_I_O"          , "")                       , true  },
                    { ("LOW_MTR_SPEED_STAT"      , "")                       , true  },
                    { ("PHASE_UVW_OC_FLT_OUTPUT" , "LOW_PHASE_UVW_OC_FLT_FB"), true  },
                    { ("HVDC_OV_FLT_FB_OUTPUT"   , "LOW_HVDC_OV_FLT_FB")     , true  },
                    { ("UVLO_TOP_FLT_FB_OUTPUT"  , "LOW_UVLO_TOP_FLT_FB")    , true  },
                    { ("UVLO_BOT_FLT_FB_OUTPUT"  , "LOW_UVLO_BOT_FLT_FB")    , true  },
                    { ("DSAT_TOP_FLT_FB_OUTPUT"  , "LOW_DSAT_TOP_FLT_FB")    , true  },
                    { ("DSAT_BOT_FLT_FB_OUTPUT"  , "LOW_DSAT_BOT_FLT_FB")    , true  },
                }, 
                new Dictionary<(string,string), bool>
                {
                    { ("LOW_OUT_EN"               , "")                      ,false },
                    { ("PWM_EN_CB"                , "")                      ,false },
                    { ("LOW_HVOV_PHOC_LATCH_CLR_O", "")                      ,false },
                });
            LoadLevels(_level2Group,
                new Dictionary<(string,string), bool> 
                { 
                    { ("LOW_3PS_MAIN_MICRO"      , "")                       , true },
                    { ("SPD_HW_3PS_I_O"          , "")                       , true },
                    { ("HVDC_OV_FLT_FB_OUTPUT"   , "LOW_HVDC_OV_FLT_FB")     , true },
                    { ("UVLO_TOP_FLT_FB_OUTPUT"  , "LOW_UVLO_TOP_FLT_FB")    , true },
                    { ("UVLO_BOT_FLT_FB_OUTPUT"  , "LOW_UVLO_BOT_FLT_FB")    , true },
                    { ("DSAT_TOP_FLT_FB_OUTPUT"  , "LOW_DSAT_TOP_FLT_FB")    , true },
                    { ("DSAT_BOT_FLT_FB_OUTPUT"  , "LOW_DSAT_BOT_FLT_FB")    , true },
                    { ("LOW_SAFESTATE1_NXP"      , "")                       , false },
                    { ("LOW_SAFESTATE2_NXP"      , "")                       , false },
                    { ("E_STOP_MAIN_MICRO_OUTPUT", "LOW_E_STOP_MAIN_MICRO")  , false },
                }, 
                new Dictionary<(string,string), bool> 
                {
                    { ("FORCE_LOWERS_ON_FB", ""), false } ,
                    { ("FORCE_UPPERS_ON_FB", ""), false } ,
                    { ("LOW_PWM_BUFFER_FB" , ""), false }
                }); 
            LoadLevels(_level3Group,
                new Dictionary<(string,string), bool> 
                {
                    { ("FORCE_LOWERS_ON_FB", ""), false } ,
                    { ("FORCE_UPPERS_ON_FB", ""), false } ,
                    { ("LOW_PWM_BUFFER_FB" , ""), false } ,
                }, 
                new Dictionary<(string,string), bool> 
                { 
                    { ("LOW_FSENB_FB"  , ""), false } ,
                    { ("FSSTATE_BOT_FB", ""), false } ,
                    { ("FSSTATE_TOP_FB", ""), false } 
                }); 
            LoadLevels(_level4Group,
                new Dictionary<(string,string), bool> 
                { 
                    { ("OC_U_FLT_OUTPUT"          , "LOW_OC_U_FLT"), true },
                    { ("OC_V_FLT_OUTPUT"          , "LOW_OC_V_FLT"), true },
                    { ("OC_W_FLT_OUTPUT"          , "LOW_OC_W_FLT"), true },
                    { ("LOW_HVOV_PHOC_LATCH_CLR"  , "")            , true },
                    { ("LOW_HVOV_PHOC_LATCH_CLR_O", "")            , false},

                }, 
                new Dictionary<(string,string), bool> 
                { 
                    { ("PHASE_UVW_OC_FLT_OUTPUT" ,"LOW_PHASE_UVW_OC_FLT_FB"),true } 
                });

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
                    var dirDisOutSignal = SignalStore.GetSignalByName<DiscreteOutputSignal>(currentDirSignal.Name.Replace("Dir", "OUTPUT"));
                    if(dirDisOutSignal != null)
                    {
                        dirDisOutSignal.IsOutput = false;
                        select.DirEnableSignal = dirDisOutSignal;
                    }
                    SignalStore.AddSignal(item);
                    SignalStore.AddSignal(currentDirSignal);

                    _derectionSignals.Insert(0, select);
                }
            }
        }

        private void LoadLevels(ObservableCollection<SignalGroupBase> levelGroup, Dictionary<(string,string), bool> inputSignals, Dictionary<(string, string), bool> outputSignals)
        {
            var disInputGroup = new SignalGroup<SignalBase>("Inputs");
            foreach (var input in inputSignals)
            {
                if (input.Value)//out
                    disInputGroup.Signals.Add(GetOutSignal(input.Key.Item1, input.Key.Item2));
                else
                    disInputGroup.Signals.Add(GetSignalAndAddViewName<DiscreteInputSignal>(input.Key.Item1));
            }

            levelGroup.Add(disInputGroup);

            var diOutGroup = new SignalGroup<SignalBase>("Outputs");
            foreach (var output in outputSignals)
            {
                if (output.Value)
            
                    diOutGroup.Signals.Add(GetOutSignal(output.Key.Item1, output.Key.Item2));
                else
                    diOutGroup.Signals.Add(GetSignalAndAddViewName<DiscreteInputSignal>(output.Key.Item1));
            }

            levelGroup.Add(diOutGroup);
        }

        private void SetOutSignalDir()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outSignalName"></param>
        /// <param name="stateName"></param>
        /// <returns></returns>
        private DiscreteOutputSignal GetOutSignal(string outSignalName, string stateName)
        {
            var s = SignalStore.GetSignalByName<DiscreteOutputSignal>(outSignalName);

            if (s != null)
            {
                if (s.State == null)
                {
                    if (!string.IsNullOrEmpty(stateName))
                        //create or find state
                        s.State = GetSignalAndAddViewName<DiscreteInputSignal>(stateName);
                    else
                        s.SetStateSignal(this.SignalStore);
                }
            }
            else
            {
                s = new DiscreteOutputSignal()
                {
                    Name = outSignalName
                };
            }

            return s;
        }

        private TSignal GetSignalAndAddViewName<TSignal>(string name)
            where TSignal : SignalBase, new()
        {
            var s = SignalStore.GetSignalByName<TSignal>(name);
            if (s != null)
            {
                s.ViewName += this.ViewName;
            }
            else
            {
                s = new TSignal
                {
                    Name = name
                };
                //return ;
            }
            return s;
        }

        private void UpdateDir()
        {
            Safing_Logic_Pin_Dir_Update.OriginValue = 1;
            SendFD(SignalStore.BuildFrames(new SignalBase[] { Safing_Logic_Pin_Dir_Update}));
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
        private DiscreteOutputSignal _testStart; 
        private DiscreteOutputSignal _testStop; 
        private SignalBase _testFinish; 
        private readonly List<SignalBase> _testResult = new List<SignalBase>();
        private bool _isTest;
        private bool _getResult;
        private RelayCommand _startTestCommand;
        private RelayCommand _stopTestCommand;
        public DiscreteOutputSignal TestStart { get => _testStart; }
        public DiscreteOutputSignal TestStop { get => _testStop; }
        public DiscreteOutputSignal TestProgress { get => _testProgress; }

        public string TestProgressPercent 
        { get 
            {
                if (_testProgress.OriginValue > 199)
                    return "100%/100%";
                return (_testProgress.OriginValue / 199 * 100).ToString("F2") + "%/100%"; } 
        }

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
            get => _testFinish.OriginValue == 2;
        }

        public List<SafingLogicTestResult> _safingLogicTestResults = new List<SafingLogicTestResult>();
        private void InitTestResultSignals()
        {
            _testProgress = SignalStore.GetSignalByName<DiscreteOutputSignal>("Safing_Logic_Test_Current_Row");
            _testProgress.PropertyChanged += TestProgress_PropertyChanged;


            _testStart = SignalStore.GetSignalByName<DiscreteOutputSignal>("Safing_Logic_Test_Start");
            _testStop = SignalStore.GetSignalByName<DiscreteOutputSignal>("Safing_Logic_Test_Stop");

            _testFinish = SignalStore.GetSignalByName<SignalBase>("Safing_Logic_Test_Status", addToStore: false);

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

        private void TestProgress_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SignalBase.OriginValue))
            {
                OnPropertyChanged(nameof(TestProgressPercent));
            }
        }

        private async void StartTest()
        {
            //1.Send Safing_Logic_Test_Start to 1
            ChangeSignal(TestStart);
            IsTest = true;
            _testFinish.OriginValue = -1;
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
            SendFD(SignalStore.BuildFrames(new SignalBase[] { signal }));
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
            if (_safingLogicTestResults.Count == 0)
            {
                AdonisUI.Controls.MessageBox.Show($"SafingLogic Test Success",
                        "SafingLogic Test",
                        icon: AdonisUI.Controls.MessageBoxImage.Information);
                return;
            }

            if (AdonisUI.Controls.MessageBox.Show($"SafingLogic Test Failure, Do you Need to Export the Result Excel? The result excel can only be exported this time.",
                        "Save SafingLogic Test",
                        buttons: AdonisUI.Controls.MessageBoxButton.YesNo,
                        icon: AdonisUI.Controls.MessageBoxImage.Information) != AdonisUI.Controls.MessageBoxResult.Yes)
            {
                return;
            }

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
            if (msgs611 != null && msgs611.FirstOrDefault() != null)
            {
                Log("Receive 0x611 Msg.");
                if (!TestFinish)
                {
                    Log("Parse finish status.");
                    SignalStore.ParseBytes(msgs611.FirstOrDefault().Data, _testFinish);
                    Log($"Parse finish status.{_testFinish.OriginValue} [{string.Join(" ", msgs611.FirstOrDefault().Data.Select(x=>x.ToString("X2")))}]");
                }
                else
                {
                    foreach (var msg611 in msgs611)
                    {
                        SafingLogicTestResult res = new SafingLogicTestResult();
                        foreach (var item in SignalStore.ParseMsgYield(msg611, _testResult))
                        {
                            res.Result.Add(item.Name, (int)item.OriginValue);
                        }
                        Log($"Error Row:{res.RowIndex} [ [{string.Join(" ", msg611.Data.Select(x => x.ToString("X2")))}]]");
                        if (res.RowIndex == 0)
                        {
                            _getResult = false;
                            DeviceStore.OnMsgReceived -= DeviceStore_OnMsgReceived;
                            break;
                        }
                        _safingLogicTestResults.Add(res);
                    }
                }
                //else
                //{
                //    Log("Safing_Logic_Test_Status not finish");
                //}
            }
            else
            {
                Log("UnReceive 0x611 Msg.");
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
