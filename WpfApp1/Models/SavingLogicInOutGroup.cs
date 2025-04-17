using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ERad5TestGUI.Models
{
    public class SavingLogicInOutGroup : SignalGroupBase
    {
        public SavingLogicInOutGroup(string signalName) 
            : base(signalName)
        {
            InOutGroups = new List<SavingLogicGroup>();
        }

        public List<SavingLogicGroup> InOutGroups { get; }
    }


    public class SavingLogicGroup : SignalGroupBase
    {
        public SavingLogicGroup(string signalName)
            : base(signalName)
        {
            Signals = new List<SavingLogicSignal>();
        }

        public List<SavingLogicSignal> Signals { get; }
    }

    public class SavingLogicSignal : SignalBase
    {
        public string GroupName { get; set; }
        /// <summary>
        /// true:Input
        /// </summary>
        public bool InputOrOutput { get; set; }

        public bool PinHigh
        {
            get
            {
                return OriginValue == 1;
            }
            set
            {
                OriginValue = value ? 1 : 0;
            }
        }

        public string PinStatus
        {
            get
            {
                return PinHigh ? "High" : "Low";
            }
        }

        protected override void OnOriginValueChaned(double originValue, bool equal)
        {
            base.OnOriginValueChaned(originValue, equal);
            if (equal)
            {
                //base.OnRealValueChanged();
                OnPropertyChanged(nameof(PinHigh));
                OnPropertyChanged(nameof(PinStatus));
            }

        }
    }

    public class SavingLogicButtonSignalGroup : SignalGroupBase
    {
        public SavingLogicButtonSignalGroup(string signalName) : base(signalName)
        {
            Signals = new List<SavingLogicButtonSignal>();
        }

        public List<SavingLogicButtonSignal> Signals { get; }
    }

    public class SavingLogicButtonSignal : TransFormSignalBase
    {
        //private string direction;
        private bool inputOrOut;

        public string Direction { get => OriginValue == 1 ? "Input" : "Output"; }
        
        public bool InputOrOut 
        {
            get => inputOrOut;
            set
            {
                if (SetProperty(ref inputOrOut, value))
                {
                    OriginValue = inputOrOut ? 1 : 0;
                }
            }
        }

        protected override void OnOriginValueChaned(double originValue, bool changed)
        {
            base.OnOriginValueChaned(originValue, changed);
            if (changed)
            {
                //InputOrOut = originValue == 1;
                OnPropertyChanged(nameof(Direction));
            }
        }
    }

    public class SafingLogicDirectionSelect
    {
        private SafingLogicDirectionSignal _currentDirection;

        public SafingLogicDirectionSignal CurrentDirection 
        { 
            get => _currentDirection;
            set
            {
                if (_currentDirection != null)
                    _currentDirection.PropertyChanged -= CurrentDirection_PropertyChanged;
                _currentDirection = value;
                if (_currentDirection != null)
                    _currentDirection.PropertyChanged += CurrentDirection_PropertyChanged;
            }
        }

        private void CurrentDirection_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SignalBase.OriginValue))
            {
                var cur = sender as SafingLogicDirectionSignal;
                if (Select != null)
                {
                    Select.OriginValue = cur.OriginValue;
                }

                if(DirEnableSignal != null)
                {
                    DirEnableSignal.IsOutput = cur.OriginValue == 1;
                }
            }
        }
        public DiscreteOutputSignal DirEnableSignal { get; set; }
        public SafingLogicDirectionSignal Select { get; set; }
    }

    public class SafingLogicDirectionSignal : TransFormSignalBase
    {
        //private bool _direction;
        [XmlIgnore]
        public bool Direction { get => OriginValue == 1; set => OriginValue = value ? 1 : 0; }
        //public string Direction { get; set; }
        private const string Header = "Safing_Logic_";
        private const string End = "_Dir";

        protected override void OnOriginValueChaned(double originValue, bool changed)
        {
            base.OnOriginValueChaned(originValue, changed);
            OnPropertyChanged(nameof(Direction));
        }


        public override string RelaceSignalName(string signalName)
        {
            string baseName =  base.RelaceSignalName(signalName);

            int idxStart = baseName.IndexOf(Header);
            if (idxStart == -1)
                idxStart = 0;
            else
                idxStart += Header.Length;
            int idxEnd = baseName.IndexOf(End) + End.Length;
            int nameLength = idxEnd - idxStart;
            baseName = baseName.Substring(idxStart, nameLength);

            return baseName;
        }
    }

    /**
     * 信号Direction：
     * 1.显示信号当前Dir
     * 2.改变信号Dir，并且修改对应Pin信号的显示
     * 
     * Pin 信号值
     * 1.分为Input/OutPut
     * Input 显示 Pin Low/High
     * Output 发送？
     */

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
            name = name.Replace(" ", "").Replace("\n", "_").Replace("/", "");
            name = $"Safing_Logic_Test_Frame_{name}";
            return Result.TryGetValue(name, out val);
        }
    }
    public class SafingLogicTestTableRow : ObservableObject
    {
        private int _safeState1;
        private int _safeState2;
        private bool? _result;

        public int RowIndex { get; set; }

        public int SAFESTATE1 { get => _safeState1; set => SetProperty(ref _safeState1, value); }

        public int SAFESTATE2
        {
            get { return _safeState2; }
            set { SetProperty(ref _safeState2, value); }
        }

        private int spdHW2psout;

        public int SPD_HW_3PS_OUT
        {
            get { return spdHW2psout; }
            set { spdHW2psout = value; }
        }
        public int PS_MAIN_MICRO { get; set; }
        public int MTR_SPEED_STAT { get; set; }
        public int PWM_EN { get; set; }
        public int ESTOP { get; set; }
        public int HVDC_OV_FLT { get; set; }
        public int DSAT_TOP_FLT { get; set; }
        public int DSAT_BOT_FLT { get; set; }
        public int UVLO_TOP_FLT { get; set; }
        public int UVLO_BOT_FLT { get; set; }
        public int OC_U_FLT { get; set; }
        public int OC_V_FLT { get; set; }
        public int OC_W_FLT { get; set; }

        public IEnumerable<int> Inputs => new List<int>()
        {
            SAFESTATE1,
            SAFESTATE2,
            SPD_HW_3PS_OUT,
            PS_MAIN_MICRO,
            MTR_SPEED_STAT,
            PWM_EN,
            ESTOP,
            HVDC_OV_FLT,
            DSAT_TOP_FLT,
            DSAT_BOT_FLT,
            UVLO_TOP_FLT,
            UVLO_BOT_FLT,
            OC_U_FLT,
            OC_V_FLT,
            OC_W_FLT,
        };
            

        /***Need To Compare****/
        private SafingLoficTestResult forceUppersOn;
        private SafingLoficTestResult forcLowersOn;
        private SafingLoficTestResult pwmBuffer;
        private SafingLoficTestResult fsstateTop;
        private SafingLoficTestResult fsstateBot;
        private SafingLoficTestResult fsenb;
        private SafingLoficTestResult outEn;
        private SafingLoficTestResult phaseUvw_OC_FLT;
        public SafingLoficTestResult FORCE_UPPERS_ON { get => forceUppersOn; set => SetProperty(ref forceUppersOn,value); }
        public SafingLoficTestResult FORCE_LOWERS_ON { get => forcLowersOn; set => SetProperty(ref forcLowersOn, value); }
        public SafingLoficTestResult PWM_BUFFER { get => pwmBuffer; set => SetProperty(ref pwmBuffer, value); }
        public SafingLoficTestResult FSSTATE_BOT { get => fsstateTop; set => SetProperty(ref fsstateTop, value); }
        public SafingLoficTestResult FSSTATE_TOP { get => fsstateBot; set => SetProperty(ref fsstateBot, value); }
        public SafingLoficTestResult FSENB { get => fsenb; set => SetProperty(ref fsenb, value); }
        public SafingLoficTestResult OUT_EN { get => outEn; set => SetProperty(ref outEn, value); }
        public SafingLoficTestResult PHASE_UVW_OC_FLT { get => phaseUvw_OC_FLT; set => SetProperty(ref phaseUvw_OC_FLT, value); }

        public bool? Result { get => _result; set => SetProperty(ref _result, value); }

        public IEnumerable<object> Outputs => new List<object>()
        {
            FORCE_UPPERS_ON,
            FORCE_LOWERS_ON,
            PWM_BUFFER,
            FSSTATE_BOT,
            FSSTATE_TOP,
            OUT_EN,
            PHASE_UVW_OC_FLT,
            Result,
        };

        public void UpdateResultPass()
        {
            Result = true;
            FORCE_UPPERS_ON.RealValue  = FORCE_UPPERS_ON.TargetValue;
            FORCE_LOWERS_ON.RealValue  = FORCE_LOWERS_ON.TargetValue;
            PWM_BUFFER.RealValue       = PWM_BUFFER.TargetValue;
            FSSTATE_BOT.RealValue      = FSSTATE_BOT.TargetValue;
            FSSTATE_TOP.RealValue      = FSSTATE_TOP.TargetValue;
            FSENB.RealValue            = FSENB.TargetValue;
            OUT_EN.RealValue           = OUT_EN.TargetValue;
            PHASE_UVW_OC_FLT.RealValue = PHASE_UVW_OC_FLT.TargetValue;
        }
        public void Clear()
        {
            Result                     = null;
            FORCE_UPPERS_ON.RealValue  = null;
            FORCE_LOWERS_ON.RealValue  = null;
            PWM_BUFFER.RealValue       = null;
            FSSTATE_BOT.RealValue      = null;
            FSSTATE_TOP.RealValue      = null;
            FSENB.RealValue            = null;
            OUT_EN.RealValue           = null;
            PHASE_UVW_OC_FLT.RealValue = null;
        }

    }

    public class SafingLoficTestResult : ObservableObject
    {
        private int? realValue;

        public string Name { get; set; }

        public int TargetValue { get; set; }

        public int? RealValue
        {
            get => realValue;
            set
            {
                if (SetProperty(ref realValue, value))
                {
                    OnPropertyChanged(nameof(Pass));
                }
            }
        }

        public bool? Pass
        {
            get
            {
                if (!RealValue.HasValue)
                    return null;
                return RealValue.Value == TargetValue;
            }
            set
            {

            }
        }

        public override string ToString()
        {
            if (RealValue.HasValue)
                return realValue.Value.ToString();
            else return TargetValue.ToString();

            //return base.ToString();
        }
    }
}
