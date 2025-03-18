using ERad5TestGUI.Stores;
using System;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace ERad5TestGUI.Models
{
    public class AnalogSignal : LimitsSignalBase, ICalStandardDev, ITransform2, IGroupSignal
    {
        private string value2 = "NaN";
        private double standardDev = double.NaN;

        public AnalogSignal()
        {
            TmpValues = new LengthQueue<string>(1000);
           
        }

        public AnalogSignal(Stores.Signal signal, string viewName) : base(signal, viewName)
        {
            TmpValues = new LengthQueue<string>(1000);
            //PinNumber = signal.Comment.GetCommentByKey("Pin_Number");
            //ADChannel = signal.Comment.GetCommentByKey("A/D_Channel");
            //Transform2Type = (int)signal.Comment.GetCommenDoubleByKey("Conversion_mode", 0);
            //if (Transform2Type == 0)
            //{
            //    TransForm2Factor = signal.Comment.GetCommenDoubleByKey("Factor", 1);
            //    TransForm2Offset = signal.Comment.GetCommenDoubleByKey("Offset", 0);
            //}
            //else
            //{
            //    TableName = signal.Comment.GetCommentByKey("Table");
            //}
            //GroupName = signal.Comment.GetCommentByKey("GroupName");
        }
        public string GroupName { get; set; }
        public string PinNumber { get; set; }
        public string ADChannel { get; set; }
        [XmlIgnore]
        public string Value2
        {
            get => NeedTransform ? value2 : "/";
            set
            {
                SetProperty(ref value2, value);
            }
        }
     
        /// <summary>
        /// 0:系数；1：查表
        /// </summary>
        public int Transform2Type { get; set; }

        /// <summary>
        /// 根据每个信号的系数/表
        /// </summary>
        /// <param name="oldVal"></param>
        /// <returns></returns>
        public double TransForm2(double oldVal)
        {
            if (Transform2Type == 0)
            {
                return oldVal * TransForm2Factor + TransForm2Offset;
            }
            else
            {
                //get table from tableName
                oldVal = TransForm(oldVal);
                return Stores.ValueTable.ConvertByTable(TableName, oldVal);
            }

            //return oldVal * 2;
        }

        public string TableName { get; set; }
        public double TransForm2Factor { get; set; }
        public double TransForm2Offset { get; set; }

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
        /// <summary>
        /// cal standard old values
        /// </summary>
        [XmlIgnore]
        public LengthQueue<string> TmpValues { get; }

        public override void UpdateFormDBC(Signal signal)
        {
            base.UpdateFormDBC(signal);
            PinNumber = signal.Comment.GetCommentByKey("Pin_Number");
            ADChannel = signal.Comment.GetCommentByKey("A/D_Channel");
            Transform2Type = (int)signal.Comment.GetCommenDoubleByKey("Conversion_mode", 0);
            if (Transform2Type == 0)
            {
                TransForm2Factor = signal.Comment.GetCommenDoubleByKey("Factor", 1);
                TransForm2Offset = signal.Comment.GetCommenDoubleByKey("Offset", 0);
            }
            else
            {
                TableName = signal.Comment.GetCommentByKey("Table");
            }
            GroupName = signal.Comment.GetCommentByKey("GroupName");
        }

        public override double TransForm(double oldVal)
        {
            return oldVal * 5 / 4096;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="originValue">new Value</param>
        /// <param name="changed">is different</param>
        public override void OnOriginValueChaned(double originValue, bool changed)
        {
            base.OnOriginValueChaned(originValue, changed);
            if (changed)
            {
                //var realValue = TransForm(originValue);
                Value2 = TransForm2(originValue).ToString(Format);
            }
            if (NeedTransform)
                TmpValues.Enqueue(Value1);
        }

        public override string GetValue()
        {
            return $"{Name} {Value2},Max:{MaxValue},Min:{MinValue},{Value1}";
        }

        public void CalStandard(int count)
        {
            if (NeedTransform)
                StandardDev = CalStandardDev.Cal(count, TmpValues);
        }
    }

    public class AnalogSignalGroup : SignalGroup<AnalogSignal>
    {
        public AnalogSignalGroup(string groupName) : base(groupName)
        {
        }
    }

    public class ResolverSignal : AverageSignalBase
    {
        public ResolverSignal()
        {

        }

        public ResolverSignal(Stores.Signal signal, string viewName) : base(signal, viewName)
        {

        }

        public override void OnOriginValueChaned(double originValue, bool changed)
        {
            base.OnOriginValueChaned(originValue, changed);
        }
    }

   

    public class CalStandardDev
    {
        public static double Cal(int count, LengthQueue<string> TmpValues)
        {
            double[] tmpArray = TmpValues.Select(x => double.Parse(x)).ToArray();
            //TmpValues.CopyTo(tmpArray, 0);
            if (tmpArray == null || tmpArray.Length == 0)
            {
                return double.NaN;
            }
            tmpArray = GetLastNElements(tmpArray, count);

            // 计算平均值
            double mean = tmpArray.Average();

            // 计算方差
            double variance = tmpArray.Select(val => Math.Pow(val - mean, 2)).Average();

            // 计算标准差
            return Math.Sqrt(variance);
        }

        public static double[] GetLastNElements(double[] array, int N)
        {
            if (array.Length <= N)
            {
                return array; // 如果数组长度小于或等于N，直接返回整个数组
            }
            else
            {
                double[] result = new double[N];
                System.Array.Copy(array, array.Length - N, result, 0, N); // 复制后N个元素
                return result;
            }
        }
    }
}
