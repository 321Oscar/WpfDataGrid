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
        private IDialogService _dialogService;
        private RelayCommand _updateDirCommand;
        private RelayCommand _DialogCommand;
        private ObservableCollection<SignalGroupBase> _level1Group = new ObservableCollection<SignalGroupBase>();
        private ObservableCollection<SignalGroupBase> _level2Group = new ObservableCollection<SignalGroupBase>();
        private ObservableCollection<SignalGroupBase> _level3Group = new ObservableCollection<SignalGroupBase>();
        private ObservableCollection<SignalGroupBase> _level4Group = new ObservableCollection<SignalGroupBase>();
        private SignalGroup<SignalBase> _inputSignals = new SignalGroup<SignalBase>("Inputs");
        private SignalGroup<SignalBase> _outputSignals = new SignalGroup<SignalBase>("Outputs");
        private ObservableCollection<SafingLogicDirectionSelect> _derectionSignals = new ObservableCollection<SafingLogicDirectionSelect>();
        public SafingLogicViewModel(SignalStore signalStore, DeviceStore deviceStore, LogService logService, IDialogService dialog)
            : base(signalStore, deviceStore, logService)
        {
            _dialogService = dialog;
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
        public SignalGroupBase InputSignals { get => _inputSignals; }
        public SignalGroupBase OutputSignals { get => _outputSignals; }
        public IEnumerable<SafingLogicDirectionSelect> DerectionSignals { get => _derectionSignals; }

        //public IEnumerable<SavingLogicSignal> SavingLogicSignals => SignalStore.GetObservableCollection<SavingLogicSignal>();
        //public IEnumerable<ObservableObject> SavingLogicGroups { get => _datas; }
        public ICommand UpdateDirCommand { get => _updateDirCommand ?? (_updateDirCommand = new RelayCommand(UpdateDir)); }
        public ICommand DialogCommand { get => _DialogCommand ?? (_DialogCommand = new RelayCommand(Dialog)); }
        public DiscreteOutputSignal Safing_Logic_Pin_Dir_Update => SignalStore.GetSignalByName<DiscreteOutputSignal>("Safing_Logic_Pin_Dir_Update", inOrOut: true);
        public override void Init()
        {
            if (_ViewName == null)
                _ViewName = "Safing_Logic";
            base.Init();
            Load();
            LoadInOutSignals();
        }

        private void Dialog()
        {
            _dialogService.ShowDialog("SafingLogicResultTableView",
                (x) =>
                {
                    LogService.Debug(x);
                });
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
                    { ("E_STOP_MAIN_MICRO_OUTPUT", "LOW_E_STOP_MAIN_MICRO")  , true  },
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
                new Dictionary<(string, string), bool>
                {
                    { ("LOW_OUT_EN"               , "")                      ,false },
                    { ("PWM_EN_CB"                , "")                      ,false },
                    { ("LOW_HVOV_PHOC_LATCH_CLR_O", "")                      ,false },
                });
            LoadLevels(_level2Group,
                new Dictionary<(string, string), bool>
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
                new Dictionary<(string, string), bool>
                {
                    { ("FORCE_LOWERS_ON_FB", ""), false } ,
                    { ("FORCE_UPPERS_ON_FB", ""), false } ,
                    { ("LOW_PWM_BUFFER_FB" , ""), false }
                });
            LoadLevels(_level3Group,
                new Dictionary<(string, string), bool>
                {
                    { ("FORCE_LOWERS_ON_FB", ""), false } ,
                    { ("FORCE_UPPERS_ON_FB", ""), false } ,
                    { ("LOW_PWM_BUFFER_FB" , ""), false } ,
                },
                new Dictionary<(string, string), bool>
                {
                    { ("LOW_FSENB_FB"  , ""), false } ,
                    { ("FSSTATE_BOT_FB", ""), false } ,
                    { ("FSSTATE_TOP_FB", ""), false }
                });
            LoadLevels(_level4Group,
                new Dictionary<(string, string), bool>
                {
                    { ("OC_U_FLT_OUTPUT"          , "LOW_OC_U_FLT"), true },
                    { ("OC_V_FLT_OUTPUT"          , "LOW_OC_V_FLT"), true },
                    { ("OC_W_FLT_OUTPUT"          , "LOW_OC_W_FLT"), true },
                    { ("LOW_HVOV_PHOC_LATCH_CLR"  , "")            , true },
                    { ("LOW_HVOV_PHOC_LATCH_CLR_O", "")            , false},

                },
                new Dictionary<(string, string), bool>
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
                    if (dirDisOutSignal != null)
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

        private void LoadInOutSignals()
        {
            _inputSignals.Signals.Add(SignalStore.GetSignalByName<DiscreteInputSignal>("LOW_SAFESTATE1_NXP"));
            _inputSignals.Signals.Add(SignalStore.GetSignalByName<DiscreteInputSignal>("LOW_SAFESTATE2_NXP"));
            _inputSignals.Signals.Add(SignalStore.GetSignalByName<DiscreteOutputSignal>("SPD_HW_3PS_I_O"));
            _inputSignals.Signals.Add(SignalStore.GetSignalByName<DiscreteOutputSignal>("LOW_3PS_MAIN_MICRO"));
            _inputSignals.Signals.Add(SignalStore.GetSignalByName<DiscreteOutputSignal>("LOW_MTR_SPEED_STAT"));
            _inputSignals.Signals.Add(SignalStore.GetSignalByName<DiscreteOutputSignal>("PWM_EN"));
            _inputSignals.Signals.Add(SignalStore.GetSignalByName<DiscreteOutputSignal>("E_STOP_MAIN_MICRO_OUTPUT"));
            _inputSignals.Signals.Add(SignalStore.GetSignalByName<DiscreteOutputSignal>("HVDC_OV_FLT_FB_OUTPUT"));
            _inputSignals.Signals.Add(SignalStore.GetSignalByName<DiscreteOutputSignal>("DSAT_TOP_FLT_FB_OUTPUT"));
            _inputSignals.Signals.Add(SignalStore.GetSignalByName<DiscreteOutputSignal>("DSAT_BOT_FLT_FB_OUTPUT"));
            _inputSignals.Signals.Add(SignalStore.GetSignalByName<DiscreteOutputSignal>("UVLO_TOP_FLT_FB_OUTPUT"));
            _inputSignals.Signals.Add(SignalStore.GetSignalByName<DiscreteOutputSignal>("UVLO_BOT_FLT_FB_OUTPUT"));
            _inputSignals.Signals.Add(SignalStore.GetSignalByName<DiscreteOutputSignal>("OC_U_FLT_OUTPUT"));
            _inputSignals.Signals.Add(SignalStore.GetSignalByName<DiscreteOutputSignal>("OC_V_FLT_OUTPUT"));
            _inputSignals.Signals.Add(SignalStore.GetSignalByName<DiscreteOutputSignal>("OC_W_FLT_OUTPUT"));

            _outputSignals.Signals.Add(SignalStore.GetSignalByName<DiscreteInputSignal>("FORCE_UPPERS_ON_FB"));
            _outputSignals.Signals.Add(SignalStore.GetSignalByName<DiscreteInputSignal>("FORCE_LOWERS_ON_FB"));
            _outputSignals.Signals.Add(SignalStore.GetSignalByName<DiscreteInputSignal>("LOW_PWM_BUFFER_FB"));
            _outputSignals.Signals.Add(SignalStore.GetSignalByName<DiscreteInputSignal>("FSSTATE_BOT_FB"));
            _outputSignals.Signals.Add(SignalStore.GetSignalByName<DiscreteInputSignal>("FSSTATE_TOP_FB"));
            _outputSignals.Signals.Add(SignalStore.GetSignalByName<DiscreteInputSignal>("LOW_FSENB_FB"));
            _outputSignals.Signals.Add(SignalStore.GetSignalByName<DiscreteInputSignal>("LOW_OUT_EN"));
            _outputSignals.Signals.Add(SignalStore.GetSignalByName<DiscreteOutputSignal>("PHASE_UVW_OC_FLT_OUTPUT"));
        }

        private void LoadLevels(ObservableCollection<SignalGroupBase> levelGroup, Dictionary<(string, string), bool> inputSignals, Dictionary<(string, string), bool> outputSignals)
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
            var s = SignalStore.GetSignalByName<DiscreteOutputSignal>(outSignalName, inOrOut: true);

            s.PropertyChanged += Signal_PropertyChanged;

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
                else if (!string.IsNullOrEmpty(stateName))
                {
                    var stateExsit = SignalStore.GetSignalByName<DiscreteInputSignal>(stateName, inOrOut: false);
                    if (SignalStore.GetSignalByName<DiscreteInputSignal>(stateName, inOrOut: false) != null)
                    {
                        s.State = stateExsit;
                    }
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
        public void AddSignalOriginalValueChanged()
        {
            foreach (var item in _inputSignals.Signals)
            {
                //while (item.PropertyChanged != null)
                    item.PropertyChanged -= Signal_PropertyChanged;//避免两次
                item.PropertyChanged += Signal_PropertyChanged;
                
                   // item.PropertyChanged -= Signal_PropertyChanged;//避免两次
            }
            foreach (var item in _outputSignals.Signals)
            {
                item.PropertyChanged -= Signal_PropertyChanged;//避免两次
                item.PropertyChanged += Signal_PropertyChanged;
            }
        }
        public void RemoveSignalOriginalValueChanged()
        {
            foreach (var item in _inputSignals.Signals)
            {
                item.PropertyChanged -= Signal_PropertyChanged;//避免两次
            }
            foreach (var item in _outputSignals.Signals)
            {
                item.PropertyChanged -= Signal_PropertyChanged;//避免两次
            }
        }
        private void Signal_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is DiscreteOutputSignal outputSignal && !IsTest)
            {
                if (e.PropertyName == nameof(DiscreteOutputSignal.Pin_High))
                {
                    outputSignal.UpdateRealValue();
                    return;
                }
                else if (e.PropertyName == nameof(SignalBase.OriginValue))
                {
                    //var syncTemp = outputSignal.Sync;
                    if (double.IsNaN(outputSignal.OriginValue))
                        return;

                    if (outputSignal.OriginValue == outputSignal.State.OriginValue)
                    {
                        return;
                    }
                    SendSignal(sender as SignalBase);
                }

            }

        }

        private void SendSignal(SignalBase signal)
        {
            SendFD(SignalStore.BuildFrames(new SignalBase[] { signal }));
        }

        public override void Send()
        {
            SendFD(SignalStore.BuildFrames(SignalStore.GetSignals<DiscreteOutputSignal>(ViewName)));
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
            SendFD(SignalStore.BuildFrames(new SignalBase[] { Safing_Logic_Pin_Dir_Update }));
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
        private SignalBase _testProgress;
        private DiscreteOutputSignal _testStart;
        private DiscreteOutputSignal _testStop;
        private SignalBase _testFinish;
        private SignalBase _testErrInfo;
        private readonly List<SignalBase> _testResult = new List<SignalBase>();
        private readonly ObservableCollection<SafingLogicTestTableRow> _excelRows = new ObservableCollection<SafingLogicTestTableRow>();
        private bool _isTest;
        private bool _getResult;
        private AsyncRelayCommand _selectExcelCommand;
        private RelayCommand _exportExcelCommand;
        private AsyncRelayCommand _startTestCommand;
        private RelayCommand _stopTestCommand;
        public DiscreteOutputSignal TestStart { get => _testStart; }
        public DiscreteOutputSignal TestStop { get => _testStop; }
        public SignalBase TestProgress { get => _testProgress; }
        public ObservableCollection<SafingLogicTestTableRow> TableRows { get => _excelRows; }

        public string TestExcelFile
        {
            get 
            { 
                if (string.IsNullOrEmpty(_testExcelFile))
                    return SafingLogicTableFilePath; 
                return _testExcelFile; 
            }
            set => _testExcelFile = value;
        }

        public string TestProgressPercent
        {
            get
            {
                if (_testProgress.OriginValue > 199)
                    return "100%/100%";
                return (_testProgress.OriginValue / 199 * 100).ToString("F2") + "%/100%";
            }
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
                        ShowTestResult();
                    }
                    _startTestCommand.NotifyCanExecuteChanged();
                    _stopTestCommand.NotifyCanExecuteChanged();
                    _exportExcelCommand.NotifyCanExecuteChanged();
                }

            }
        }

        private bool _isCancel;
        public ICommand StartTestCommand => _startTestCommand ?? (_startTestCommand = new AsyncRelayCommand(StartTest, () => !IsTest));
        public ICommand StopTestCommand => _stopTestCommand ?? (_stopTestCommand = new RelayCommand(StopTest, () => IsTest));
        public ICommand SelectExcelCommand => _selectExcelCommand ?? (_selectExcelCommand = new AsyncRelayCommand(SelectExcel));
        public ICommand ExportExcelCommand => _exportExcelCommand ?? (_exportExcelCommand = new RelayCommand(SaveExcelTest, () => TableRows.All(x => x.Result != null)));

        public bool TestFinish
        {
            get => _testFinish.OriginValue == 2;
        }

        public List<SafingLogicTestResult> _safingLogicTestResults = new List<SafingLogicTestResult>();
        private string _testExcelFile;

        public bool TestNoFail => _testErrInfo.OriginValue == 0;
        public bool PreconditionFail => (((int)_testErrInfo.OriginValue) & (1 << 0)) != 0;
        public bool NXPFail => (((int)_testErrInfo.OriginValue) & (1 << 1)) != 0;
        public bool SBCFail => (((int)_testErrInfo.OriginValue) & (1 << 2)) != 0;
        public bool TestRowFail => (((int)_testErrInfo.OriginValue) & (1 << 3)) != 0;
        public override void Dispose()
        {
            if (_testProgress != null)
                _testProgress.PropertyChanged -= TestProgress_PropertyChanged;
            if (_testErrInfo != null)
                _testErrInfo.PropertyChanged -= TestErrInfo_PropertyChanged;
            base.Dispose();
        }

        private void InitTestResultSignals()
        {
            _testProgress = SignalStore.GetSignalByName<SignalBase>("Safing_Logic_Test_Current_Row");
            _testProgress.PropertyChanged += TestProgress_PropertyChanged;
            _testErrInfo = SignalStore.GetSignalByName<SignalBase>("Safing_Logic_Test_Error_info");
            _testErrInfo.PropertyChanged += TestErrInfo_PropertyChanged; ;

            _testStart = SignalStore.GetSignalByName<DiscreteOutputSignal>("Safing_Logic_Test_Start", inOrOut: true);
            _testStop = SignalStore.GetSignalByName<DiscreteOutputSignal>("Safing_Logic_Test_Stop", inOrOut: true);

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

            //LoadExcel(TestExcelFile);
        }

        private void TestErrInfo_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SignalBase.OriginValue))
            {
                //OnPropertyChanged(nameof(TestProgressPercent));
                OnPropertyChanged(nameof(TestNoFail));
                OnPropertyChanged(nameof(PreconditionFail));
                OnPropertyChanged(nameof(NXPFail));
                OnPropertyChanged(nameof(SBCFail));
                OnPropertyChanged(nameof(TestRowFail));
            }
        }

        private void TestProgress_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SignalBase.OriginValue))
            {
               
                OnPropertyChanged(nameof(TestProgressPercent));
            }
        }

        private async Task StartTest()
        {
            if (!DeviceStore.HasDevice)
            {
                AdonisUI.Controls.MessageBox.Show($"No Device Selected!",
                      "SafingLogic Test",
                      icon: AdonisUI.Controls.MessageBoxImage.Information);
                return; 
            }
            _isCancel = false;
            //1.Send Safing_Logic_Test_Start to 1
            await Task.Run(() => Dispatch(() => TableRows.ToList().ForEach(x => x.Clear())));
            
            ChangeSignal(TestStart);
            IsTest = true;
            _testFinish.OriginValue = -1;
            _safingLogicTestResults.Clear();
            _getResult = true;
            if (DeviceStore.CurrentDevice is Devices.VirtualDevice)
            {
                await Task.Delay(1000);
                _getResult = false;
                GenarateResult();
            }
            else
            {
                Log("Start Test");
                DeviceStore.OnMsgReceived += DeviceStore_OnMsgReceived;
                do
                {
                    await Task.Delay(1000);
                } while (_getResult && IsTest);
            }

            IsTest = false;
            Log("Test Done");
        }

        private void StopTest()
        {
            ChangeSignal(TestStop);
            _isCancel = true;
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
            res.Result.Add("Safing_Logic_Test_Frame_Header", 0x10);
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

        private Task SelectExcel()
        {
            //throw new NotImplementedException();
            //CommonOpenFileDialog ofd = new CommonOpenFileDialog();
            //ofd.Filters.Add(new CommonFileDialogFilter("excel File", "*.xls;*.xlsx"));
            //if (ofd.ShowDialog() == CommonFileDialogResult.Ok)
            //{
               return LoadExcel(TestExcelFile);
            //}

            //return Task.CompletedTask;
        }

        private Task LoadExcel(string fileName)
        {
            TableRows.Clear();

            //read excel and binding to Rows
            IWorkbook wb = NPOIHelper.GetWorkbookByExcel(fileName);

            ISheet sheet = wb.GetSheetAt(0);

            int rowCount = sheet.LastRowNum;
            //第一行为列名
            var headerRow = sheet.GetRow(0);
            Dictionary<string, int> headerIndex = new Dictionary<string, int>();
            for (int i = 0; i < headerRow.LastCellNum; i++)
            {
                headerIndex.Add(headerRow.GetCell(i).StringCellValue.Replace(" ", "").Replace("\n", "_").Replace("/", ""), i);
            }

            var properties = typeof(SafingLogicTestTableRow).GetProperties();
            IsLoading = true;
            var rows = new List<SafingLogicTestTableRow>();
            return Task.Run(() =>
            {
                 for (int i = 1; i < rowCount; i++)
                 {
                     var excelRow = sheet.GetRow(i);
                     if (excelRow.Cells.Count < 17)
                         continue;

                     if (excelRow.Cells.FirstOrDefault().CellType == CellType.Blank)
                         continue;

                     SafingLogicTestTableRow row = new SafingLogicTestTableRow()
                     {
                         RowIndex = i
                     };

                     foreach (var prop in properties)
                     {
                         // 检查当前列名是否与实体属性匹配
                         var columnName = headerIndex.Keys.FirstOrDefault(x => x.IndexOf(prop.Name, StringComparison.OrdinalIgnoreCase) > -1);

                         if (!string.IsNullOrEmpty(columnName) && headerIndex.TryGetValue(columnName, out int idx))
                         {
                             var cell = excelRow.GetCell(idx);
                             //cell.CellType
                             var cellValue = excelRow.GetCell(idx).NumericCellValue;
                             // if (!string.IsNullOrEmpty(cellValue))
                             {
                                 // 使用反射赋值
                                 if (prop.PropertyType == typeof(SafingLoficTestResult))
                                 {
                                     SafingLoficTestResult res = new SafingLoficTestResult()
                                     {
                                         TargetValue = (int)cellValue
                                     };
                                     prop.SetValue(row, res);
                                 }
                                 else
                                 {
                                     prop.SetValue(row, Convert.ChangeType(cellValue, prop.PropertyType));
                                 }
                             }
                         }
                     }
                     rows.Add(row);

                 }
            }).ContinueWith((x) =>
            {
                Dispatch(() => TableRows.AddRange(rows));
                //foreach (var item in rows)
                //{
                //    Dispatch(() => TableRows.Add(item));
                //}
                IsLoading = false;
            });
        }

        private void ShowTestResult()
        {
            if (_isCancel)
            {
                AdonisUI.Controls.MessageBox.Show(text: $"SafingLogic Test  Cancel.",
                        caption: "SafingLogic Test",
                        icon: AdonisUI.Controls.MessageBoxImage.Information);
                return;
            }

            if (_safingLogicTestResults.Count == 0)
            {
                AdonisUI.Controls.MessageBox.Show($"SafingLogic Test Success",
                        "SafingLogic Test",
                        icon: AdonisUI.Controls.MessageBoxImage.Information);

                TableRows.ToList().ForEach(x => x.UpdateResultPass());
            }
            else
            {
                foreach (var item in TableRows)
                {
                    var errorRow = _safingLogicTestResults.FirstOrDefault(x => x.RowIndex == item.RowIndex);
                    if(errorRow != null)
                    {
                        item.Result = false;
                        errorRow.TryGetValue(nameof(SafingLogicTestTableRow.FORCE_UPPERS_ON), out int rV);
                        errorRow.TryGetValue(nameof(SafingLogicTestTableRow.FORCE_LOWERS_ON), out int rV2);
                        errorRow.TryGetValue(nameof(SafingLogicTestTableRow.PWM_BUFFER), out int rV3);
                        errorRow.TryGetValue(nameof(SafingLogicTestTableRow.FSSTATE_BOT), out int rV4);
                        errorRow.TryGetValue(nameof(SafingLogicTestTableRow.FSSTATE_TOP), out int rV5);
                        errorRow.TryGetValue(nameof(SafingLogicTestTableRow.FSENB), out int rV6);
                        errorRow.TryGetValue(nameof(SafingLogicTestTableRow.OUT_EN), out int rV7);
                        errorRow.TryGetValue(nameof(SafingLogicTestTableRow.PHASE_UVW_OC_FLT), out int rV8);
                        item.FORCE_UPPERS_ON.RealValue = rV;
                        item.FORCE_LOWERS_ON.RealValue = rV2;
                        item.PWM_BUFFER.RealValue = rV3;
                        item.FSSTATE_BOT.RealValue = rV4;
                        item.FSSTATE_TOP.RealValue = rV5;
                        item.FSENB.RealValue = rV6;
                        item.OUT_EN.RealValue = rV7;
                        item.PHASE_UVW_OC_FLT.RealValue = rV8;
                    }
                    else
                    {
                        item.UpdateResultPass();
                    }
                }

                // if (AdonisUI.Controls.MessageBox.Show($"SafingLogic Test Failure, Do you Need to Export the Result Excel? The result excel can only be exported this time.",
                if (AdonisUI.Controls.MessageBox.Show($"SafingLogic Test Failure. ErrorRow Count:{_safingLogicTestResults.Count}",
                        "SafingLogic Test",
                        buttons: AdonisUI.Controls.MessageBoxButton.OK,
                        icon: AdonisUI.Controls.MessageBoxImage.Information) != AdonisUI.Controls.MessageBoxResult.Yes)
                {
                    return;
                }
            }
        }

        private void SaveExcelTest()
        {
            IWorkbook wb = NPOIHelper.GetWorkbookByExcel(TestExcelFile);

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
                        icon: AdonisUI.Controls.MessageBoxImage.Information);
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
                //Log("Receive 0x611 Msg.");
                if (!TestFinish)
                {
                    //Log("Parse finish status.");
                    SignalStore.ParseBytes(msgs611.FirstOrDefault().Data, _testFinish);
                    Log($"Parse finish status.{_testFinish.OriginValue} [{string.Join(" ", msgs611.FirstOrDefault().Data.Select(x => x.ToString("X2")))}]");
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
                        Log($"Error Row:{res.RowIndex}  [{string.Join(" ", msg611.Data.Select(x => x.ToString("X2")))}]");
                        if (res.RowIndex == 0)
                        {
                            //start receive error row info
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
                //Log("UnReceive 0x611 Msg.");
            }
        }

        #endregion
    }


}
