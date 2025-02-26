using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using WpfApp1.Stores;

namespace WpfApp1.Models
{
    [XmlInclude(typeof(AnalogSignal))]
    [XmlInclude(typeof(DiscreteInputSignal))]
    [XmlInclude(typeof(DiscreteOutputSignal))]
    [XmlInclude(typeof(PulseInSignal))]
    [XmlInclude(typeof(PulseOutSingleSignal))]
    [XmlInclude(typeof(PulseOutGroupSignal))]
    [XmlInclude(typeof(NXPSignal))]
    [XmlInclude(typeof(NXPInputSignal))]
    [XmlInclude(typeof(GDICStatusDataSignal))]
    [XmlInclude(typeof(GDICAoutSignal))]
    [XmlInclude(typeof(GDICRegisterSignal))]
    public class SignalBase : ObservableObject, IDBCSignal
    {
        public const string ViewNameSplit = ";";
        /// <summary>
        /// 显示信号的分割线
        /// </summary>
        public const string SignalNameSplit = "-";
        /// <summary>
        /// DBC文件中的信号分割线
        /// </summary>
        public const string DBCSignalNameSplit = "_";

        private double originValue;


        public SignalBase()
        {

        }

        public SignalBase(Stores.Signal signal, string viewName)
        {
            Name = signal.SignalName;
            StartBit = (int)signal.startBit;
            Factor = signal.factor;
            Offset = signal.offset;
            ByteOrder = (int)signal.byteOrder;
            Length = (int)signal.signalSize;
            MessageID = signal.MessageID;
            ViewName += viewName;
            Unit = signal.Unit;
        }

        //private string name;
        public string Name { get; set; }

        public string DisplayName { get => RelaceSignalName(Name); }
        public string ViewName
        {
            get => string.Join(ViewNameSplit, ViewNames.Distinct()) + ViewNameSplit; 
            set
            {
                string[] names = value.Split(ViewNameSplit);

                foreach (var name in names)
                {
                    if (ViewNames.Contains(name))
                        continue;
                    ViewNames.Add(name);
                }
            }
        }
        [XmlIgnore]
        public List<string> ViewNames { get; set; } = new List<string>();
        [XmlIgnore]
        public double OriginValue 
        {
            get => originValue;
            set
            {
                var equal = SetProperty(ref originValue, value);
                OnOriginValueChaned(value, equal);
            }
        }

        public uint MessageID { get; set; }
        public int StartBit { get; set; }

        public int Length { get; set; }

        public int ByteOrder { get; set; }

        public uint ValueType { get; set; }

        public double Factor { get; set; }

        public double Offset { get; set; }
        public string Format { get; set; } = "f2";
        public string Unit { get; set; }
        /// <summary>
        /// In : false
        ///<para>Out: True</para>
        /// </summary>
        public bool InOrOut { get;set; }
        /// <summary>
        /// 传进一个新的原始值引发
        /// </summary>
        /// <param name="originValue">originValue</param>
        /// <param name="changed"></param>
        public virtual void OnOriginValueChaned(double originValue, bool changed)
        {
            //RealValue = TransForm(originValue).ToString();
        }

        /// <summary>
        /// start with "VI_" Replace ""
        /// <para>start with "Low_" Replace "/"</para>
        /// </summary>
        /// <param name="signalName"></param>
        /// <returns></returns>
        public virtual string RelaceSignalName(string signalName)
        {
            if (signalName.StartsWith("VI_"))
            {
                return signalName.Replace("VI_", "");
            }
            if (signalName.StartsWith("LOW_"))
            {
                return signalName.Replace("LOW_", "/");
            }
            return signalName;
        }
        public static string ReplaceViewModel(string viewName)
        {
            return viewName.Replace("ViewModel", "");
        }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            if(obj is SignalBase otherSignal)
            {
                return this.MessageID == otherSignal.MessageID && this.Name == otherSignal.Name;
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return MessageID.GetHashCode() + Name.GetHashCode();
        }
    }

    public class TransFormSignalBase : SignalBase
    {
        private string value1 = "NaN";

        public TransFormSignalBase()
        {
                
        }

        public TransFormSignalBase(Stores.Signal signal, string viewName) : base(signal, viewName)
        {

        }

        [XmlIgnore]
        public string Value1
        {
            get => value1;
            set
            {
                SetProperty(ref value1, value);
            }
        }
        public virtual double TransForm(double oldVal)
        {
            return oldVal;
        }

        public override void OnOriginValueChaned(double originValue, bool changed)
        {
            if (changed)
            {
                //var realValue = TransForm(originValue);
                //MaxValue = Math.Max(MaxValue, realValue);
                //if (MinValue < 0)
                //    MinValue = realValue;
                //else
                //    MinValue = Math.Min(MinValue, realValue);
                //cal value1
                Value1 = TransForm(originValue).ToString(Format);

                //OutLimits = realValue > MaxThreshold || realValue < MinThreshold;
            }
        }
    }

    /// <summary>
    /// max,min value and OutLimits
    /// </summary>
    public class LimitsSignalBase : TransFormSignalBase, ILimits
    {
        //private string value1;
        private double maxValue;
        private double minValue = -1;
        private double maxThreshold;
        private double minThreshold;
        private bool outLimits = true;

        public LimitsSignalBase()
        {
            MaxThreshold = 5;
            MinThreshold = 0;
        }

        public LimitsSignalBase(Stores.Signal signal, string viewName) : base(signal, viewName)
        {
            MaxThreshold = 5;
            MinThreshold = 0;
        }

        //private SolidColorBrush valueColor;

        //public System.Windows.Media.SolidColorBrush ValueColor { get => valueColor; set => SetProperty(ref valueColor, value); }
        [XmlIgnore]
        public double MaxValue { get => maxValue; set => SetProperty(ref maxValue, value); }
        [XmlIgnore]
        public double MinValue { get => minValue; set => SetProperty(ref minValue, value); }
        public double MaxThreshold 
        { 
            get => maxThreshold;
            set 
            {
                if (SetProperty(ref maxThreshold, value))
                {
                    ChangeOutLimits();
                }
            }
        }
        public double MinThreshold
        {
            get => minThreshold;
            set
            {
                if (SetProperty(ref minThreshold, value))
                    ChangeOutLimits();
            }
        }
        [XmlIgnore]
        public bool OutLimits { get => outLimits; set => SetProperty(ref outLimits, value); }

        public override void OnOriginValueChaned(double originValue, bool changed)
        {
            base.OnOriginValueChaned(originValue, changed);
            if (changed)
            {
                var realValue = TransForm(originValue);
                MaxValue = Math.Max(MaxValue, realValue);
                if (MinValue < 0)
                    MinValue = realValue;
                else
                    MinValue = Math.Min(MinValue, realValue);
                //cal value1
                //Value1 = TransForm(originValue).ToString(Format);
                ChangeOutLimits();
                //OutLimits = realValue > MaxThreshold || realValue < MinThreshold;
            }
        }

        private void ChangeOutLimits()
        {
            if (double.TryParse(this.Value1, out double realVal))
                OutLimits = realVal > MaxThreshold || realVal < MinThreshold;
        }
    }

    public class AverageSignalBase: LimitsSignalBase, IAverage
    {
        private int valueCount;
        private double totalValue;

        private double average;

        public AverageSignalBase()
        {

        }

        public AverageSignalBase(Stores.Signal signal, string viewName) : base(signal, viewName)
        {

        }

        [XmlIgnore]
        public double Average { get => average; set => SetProperty(ref average, value); }

        public override void OnOriginValueChaned(double originValue, bool changed)
        {
            valueCount++;
            base.OnOriginValueChaned(originValue, changed);
            var realValue = TransForm(originValue);
            totalValue += realValue;
            Average = totalValue / valueCount;
        }

    }

    internal interface IAverage
    {
        double Average { get; set; }
    }

    internal interface ILimits
    {
        double MaxValue { get; set; }
        double MinValue { get; set; }
        double MaxThreshold { get; set; }
        bool OutLimits { get; }
        //SolidColorBrush ValueColor { get; set; }
    }



    public class SignalGroupBase : ObservableObject
    {
        public string GroupName { get; set; }

        public SignalGroupBase(string groupName)
        {
            GroupName = groupName;
        }
    }

    public class SignalGroup<TSignal> : SignalGroupBase
        where TSignal : SignalBase, new()
    {
        public List<TSignal> Signals { get; }

        public SignalGroup(string groupName) : base(groupName)
        {
            Signals = new List<TSignal>();
        }
    }

    public interface IGroupSignal
    {
        string GroupName { get; }
    }
    
    public interface IDBCSignal
    {
        uint MessageID { get; }

        /// <summary>
        /// 起始位
        /// </summary>
        int StartBit { get; }
        /// <summary>
        /// 数据长度
        /// </summary>
        int Length { get; }
        /// <summary>
        /// 0:Motorola;1:Intel
        /// </summary>
        int ByteOrder { get; }
        /// <summary>
        /// 0:unsigned;1:signed
        /// </summary>
        uint ValueType { get; }
        double Factor { get; }
        /// <summary>
        /// 偏移量
        /// </summary>
        double Offset { get; }
    }

    /// <summary>
    /// 转换
    /// </summary>
    public interface ITransform2
    {
        int Transform2Type { get; }

        double TransForm2(double oldVal);

        string TableName { get; }

        double TransForm2Factor { get; }
        double TransForm2Offset { get; }
        string Value2 { get; set; }
    }

    public interface ICalStandardDev
    {
        LengthQueue<string> TmpValues { get; }

        double StandardDev { get; set; }

        void CalStandard(int count);
    }

    public class LengthQueue<T> : System.Collections.Generic.Queue<T>
    {
        private readonly int length;

        public LengthQueue(int length) : base(length)
        {
            this.length = length;
        }

        public new void Enqueue(T item)
        {
            if (base.Count == length)
                base.Dequeue();
            base.Enqueue(item);
        }
    }

    public interface IDbcSignalConvert<TOutSignal>
        where TOutSignal : class
    {
        TOutSignal Convert();
    }

    [Serializable]
    public class ViewSignalsInfo
    {
        //[XmlAttribute]
        public string ViewName { get; set; }
        public List<SignalBase> Signals { get; set; } = new List<SignalBase>();

        public override string ToString()
        {
            return ViewName;
        }
    }
    [Serializable]
    public class ViewsSignals
    {
        public List<ViewSignalsInfo> ViewSignalsInfos { get; set; } = new List<ViewSignalsInfo>();

        public ViewSignalsInfo GetViewSignalInfo(string viewName)
        {
            viewName = SignalBase.ReplaceViewModel(viewName);

            ViewSignalsInfo viewSignalsInfo = ViewSignalsInfos.FirstOrDefault(x => x.ViewName == viewName);
            if (viewSignalsInfo == null)
            {
                viewSignalsInfo = new ViewSignalsInfo()
                {
                    ViewName = viewName,
                };
                ViewSignalsInfos.Add(viewSignalsInfo);
            }

            return viewSignalsInfo;
        }
    }
    [Serializable]
    public class SignalCollection
    {
        public List<SignalBase> Signals { get; set; } = new List<SignalBase>();
    }

    public class SignalValueTables
    {
        public List<SignalValueTable> Tables { get; set; }
    }

    [Serializable]
    public class SignalValueTable
    {
        [XmlAttribute]
        public string TableName { get; set; }

        public List<SignalValueTableRow> Rows { get; set; }

        public double GetTForV(double v)
        {
            var exactMatch = Rows.FirstOrDefault(r => r.ValueV == v);
            if (exactMatch != null)
            {
                return exactMatch.ValueT; // 如果找到完全匹配的V，返回对应的T
            }

            // 找到最近的两个Row
            // 使用二分查找找到插入位置
            int index = Rows.BinarySearch(new SignalValueTableRow(v), new RowComparer());

            // 找到最近的两个Row
            SignalValueTableRow below = null, above = null;
            foreach (var item in Rows.OrderBy(x => x.ValueV))
            {
                if (item.ValueV > v)
                {
                    above = item;
                    if (Rows.IndexOf(item) + 1 < Rows.Count)
                        below = Rows[Rows.IndexOf(item) + 1];
                    break;
                }
            }

            // 如果未找到足够的数据点
            if (below == null || above == null)
            {
                return Rows.FirstOrDefault().ValueT;
            }

            // 计算斜率
            double slope = (above.ValueT - below.ValueT) / (above.ValueV - below.ValueV);
            // 根据斜率和下方点计算出新的T
            double interpolatedT = below.ValueT + slope * (v - below.ValueV);

            return interpolatedT;
        }
    }

    public class RowComparer : IComparer<SignalValueTableRow>
    {
        public int Compare(SignalValueTableRow x, SignalValueTableRow y)
        {
            return x.ValueV.CompareTo(y.ValueV);
        }
    }

    public class SignalValueTableRow
    {
        private double recent;
        private string formula;
        public SignalValueTableRow()
        {

        }
        public SignalValueTableRow(double valueT, string formula, double recent)
        {
            ValueT = valueT;
            Formula = formula;
            Recent = recent;
        }

        public SignalValueTableRow(double val)
        {
            ValueV = val;
        }

        [XmlAttribute("T")]
        public double ValueT { get; set; }
        [XmlAttribute]
        public string Formula 
        {
            get => formula;
            set 
            {
                formula = value;
                if (Recent != 0)
                {
                    ValueV = FormulaCalculator.CalculateV(formula, Recent);
                }
            }
        }
        [XmlAttribute]
        public double Recent
        {
            get => recent;
            set
            {
                recent = value;
                if (!string.IsNullOrEmpty(formula))
                {
                    ValueV = FormulaCalculator.CalculateV(Formula, recent);
                }
            }
        }
        [XmlIgnore]
        public double ValueV { get; private set; }

        public override string ToString()
        {
            return $"T:{ValueT} ,{ValueV}";
        }
    }
    /*
    要根据字符串提取和计算公式 V = 5 * Rt / (Rt + 19.6k)，
    我们可以使用 C# 的 DataTable.Compute 方法，这个方法可以执行简单的数学表达式。
    如果需要对字符串进行解析和计算，可以利用正则表达式和自定义解析逻辑来处理更复杂的情况。
    下面是一个示例，实现了如何解析公式字符串并计算 V 的值：
    示例代码创建一个 Row 类（如之前所定义）实现一个方法来解析公式并计算 V
    */

    public class FormulaCalculator
    {
        public static double CalculateV(string formula, double Rt)
        {
            // 替换公式中的变量
            string expression = formula.Replace("Rt", Rt.ToString());

            // 处理可能存在的单位，例如 "19.6k" 转为数值
            expression = ProcessUnits(expression);

            // 使用 DataTable.Compute 进行计算
            var result = new System.Data.DataTable().Compute(expression, null);
            return Convert.ToDouble(result);
        }

        private static string ProcessUnits(string expression)
        {
            // 将 "k" 替换为 * 1000
            return System.Text.RegularExpressions.Regex.Replace(expression, @"(\d+\.?\d*)k", match =>
            {
                return (double.Parse(match.Groups[1].Value) * 1000).ToString();
            });
        }
    }

}
