using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace WpfApp1.Models
{
    public class SignalBase : ObservableObject, IDBCSignal
    {
        private double originValue;

        public string Name { get; set; }

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

        /// <summary>
        /// 传进一个新的原始值引发
        /// </summary>
        /// <param name="originValue">originValue</param>
        /// <param name="changed"></param>
        public virtual void OnOriginValueChaned(double originValue, bool changed)
        {
            //RealValue = TransForm(originValue).ToString();
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class TransFormSignalBase : SignalBase
    {
        private string value1;
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
       
        //private SolidColorBrush valueColor;

        //public System.Windows.Media.SolidColorBrush ValueColor { get => valueColor; set => SetProperty(ref valueColor, value); }
        public double MaxValue { get => maxValue; set => SetProperty(ref maxValue, value); }
        public double MinValue { get => minValue; set => SetProperty(ref minValue, value); }
        public double MaxThreshold { get => maxThreshold; set => SetProperty(ref maxThreshold, value); }
        public double MinThreshold { get => minThreshold; set => SetProperty(ref minThreshold, value); }
        public bool OutLimits { get => outLimits; set => SetProperty(ref outLimits, value); }

        public override void OnOriginValueChaned(double originValue, bool changed)
        {
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

                OutLimits = realValue > MaxThreshold || realValue < MinThreshold;
            }
        }
    }

    public class AverageSignalBase: LimitsSignalBase, IAverage
    {
        private int valueCount;
        private double totalValue;

        private double average;

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



    public class SignalGroupBase: ObservableObject
    {
        public string SignalName { get; set; }

        public SignalGroupBase(string signalName)
        {
            SignalName = signalName;
        }
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
    }

    public interface ICalStandardDev
    {
        LengthQueue<string> TmpValues { get; }

        double StandardDev { get; set; }

        void CalStandard();
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
            if(base.Count == length)
                base.Dequeue();
            base.Enqueue(item);
        }
    }

}
