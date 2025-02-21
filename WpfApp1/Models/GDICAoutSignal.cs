using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using WpfApp1.Stores;

namespace WpfApp1.Models
{
    public class GDICAoutSignal : LimitsSignalBase, ICalStandardDev
    {
        private double standardDev = double.NaN;
        private string selection;

        public GDICAoutSignal()
        {
            TmpValues = new LengthQueue<string>(1000);
        }

        public GDICAoutSignal(Stores.Signal signal, string viewName) : base(signal, viewName)
        {
            Selection = Name.Split(DBCSignalNameSplit).Skip(2).Take(1).FirstOrDefault();
            CanChangeSelection = !(Name.IndexOf("Tsensea", System.StringComparison.OrdinalIgnoreCase) > -1);
        }
        [XmlIgnore]
        public LengthQueue<string> TmpValues { get; }
        [XmlIgnore]
        public double StandardDev
        {
            get
            {
                return standardDev;
            }

            set
            {
                SetProperty(ref standardDev, value);
            }
        }

        public string Selection { get => selection; set => SetProperty(ref selection, value); }

        public bool CanChangeSelection { get; set; }

        public string GDDevice { get => string.Join(SignalNameSplit, Name.Split(DBCSignalNameSplit).Take(2)); }

        public void CalStandard(int count)
        {
            StandardDev = CalStandardDev.Cal(count, TmpValues);
        }
    }

    public class GDICAoutTemperatureSignal : GDICAoutSignal
    {
        private GDICAoutSignal duty;
        private GDICAoutSignal freq;
        public GDICAoutTemperatureSignal(GDICAoutSignal duty, GDICAoutSignal freq)
        {
            Name = duty.GDDevice;

            Duty = duty;
            Freq = freq;
            Selection = duty.Selection;
            CanChangeSelection = duty.CanChangeSelection;
        }


        public GDICAoutSignal Duty 
        { 
            get => duty; 
            set 
            { 
                if(duty != null)
                    duty.PropertyChanged -= Duty_PropertyChanged;
                duty = value;
                if (duty != null)
                    duty.PropertyChanged += Duty_PropertyChanged;
            } 
        }

        public GDICAoutSignal Freq
        {
            get => freq; 
            set
            {
                if (freq != null)
                    freq.PropertyChanged -= Duty_PropertyChanged;
                freq = value;
                if (freq != null)
                    freq.PropertyChanged += Duty_PropertyChanged;
            }
        }

        public override void OnOriginValueChaned(double originValue, bool changed)
        {
            base.OnOriginValueChaned(originValue, changed);
        }

        private void Duty_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(OriginValue))
            {
                this.OriginValue = Duty.OriginValue + Freq.OriginValue;
            }
        }
    }

    public class GDICRegisterSignal : TransFormSignalBase, IGroupSignal
    {
        /// <summary>
        /// Top-U-Config
        /// </summary>
        public string GroupName { get => string.Join(SignalNameSplit, Name.Split(DBCSignalNameSplit).Take(3)); }
        public string DeviceName { get => string.Join(SignalNameSplit, Name.Split(DBCSignalNameSplit).Take(2)); }
        public string Address { get; set; }

        public GDICRegisterSignal()
        {

        }

        public GDICRegisterSignal(Signal signal,string viewName) : base(signal,viewName)
        {
            Address = signal.Comment.GetCommentByKey("Address");
        }
    }

    public class GDICRegisterGroup : IGroupSignal
    {
        public string Address { get => Data?.Address; }
        /// <summary>
        /// Config-1
        /// </summary>
        public string GroupName { get; }

        public GDICRegisterGroup(string groupName, GDICRegisterSignal data, GDICRegisterSignal cRC)
        {
            GroupName = groupName;
            Data = data;
            CRC = cRC;
        }

        public GDICRegisterSignal Data { get; }
        public GDICRegisterSignal CRC { get; }
    }

    public class GDICRegisterDeviceGroup : IGroupSignal
    {
        public GDICRegisterDeviceGroup(string groupName)
        {
            RegisterGroups = new List<GDICRegisterGroup>();
            GroupName = groupName;
        }
        /// <summary>
        /// TOP-U
        /// </summary>
        public string GroupName { get; }

        public List<GDICRegisterGroup> RegisterGroups { get; }
    }

    public class GDICADCSignal : CommunityToolkit.Mvvm.ComponentModel.ObservableObject
    {
        private double desat;
        private GDICRegisterSignal registerSignal;
        private double amuxin;
        private double vCC;
        private double vEE;
        private double powerTemp;
        private double dieTemp;

        public GDICRegisterSignal RegisterSignal
        {
            get => registerSignal;
            set
            {
                registerSignal = value;
                registerSignal.PropertyChanged += RegisterSignal_PropertyChanged;
            }
        }

        private void RegisterSignal_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(registerSignal.OriginValue))
            {
                ChangeValue(RegisterSignal.OriginValue);
            }
        }

        public GDICRegisterSignal WriteSignal { get; set; }
        private void ChangeValue(double originalValue)
        {
            switch (WriteSignal.OriginValue)
            {
                default:
                case 0:
                    Desat = originalValue;
                    break;
                case 1:
                    Amuxin = originalValue;
                    break; 
                case 2:
                    VCC = originalValue;
                    break; 
                case 3:
                    VEE = originalValue;
                    break; 
                case 4:
                    PowerTemp = originalValue;
                    break;
                case 5:
                    DieTemp = originalValue;
                    break;

            }
        }
        public double Desat
        {
            get => desat;
            set
            {
                SetProperty(ref desat, value);
            }
        }

        public double Amuxin { get => amuxin; set => SetProperty(ref amuxin , value); }
        public double VCC { get => vCC; set => SetProperty(ref vCC , value); }
        public double VEE { get => vEE; set => SetProperty(ref vEE , value); }
        public double PowerTemp { get => powerTemp; set => SetProperty(ref powerTemp , value); }
        public double DieTemp { get => dieTemp; set => SetProperty(ref dieTemp , value); }
    }
}
