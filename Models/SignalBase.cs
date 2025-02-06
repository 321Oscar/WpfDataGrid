using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Windows.Media;

namespace WpfApp1.Models
{
    public class SignalBase : ObservableObject, IDBCSignal
    {
        private double realValue;
        private double maxValue;
        private double minValue = -1;
        private double maxThreshold;
        private double minThreshold;
        private double average;
        private SolidColorBrush valueColor;
        private double originValue;

        public string Name { get; set; }

        public double OriginValue 
        {
            get => originValue;
            set
            {
                SetProperty(ref originValue, value);
                OnOriginValueChaned(value);
            }
        }

        /// <summary>
        /// it will Change the MaxValue/MinValue and change the ValueColor
        /// </summary>
        public double RealValue
        {
            get => realValue;
            set
            {
                //double tmp = TransForm(value);
                if (SetProperty(ref realValue, value))
                {
                    OnRealValueChanged();
                }
            }
        }
        public System.Windows.Media.SolidColorBrush ValueColor { get => valueColor; set => SetProperty(ref valueColor, value); }
        public double MaxValue { get => maxValue; set => SetProperty(ref maxValue, value); }
        public double MinValue { get => minValue; set => SetProperty(ref minValue, value); }
        public double MaxThreshold { get => maxThreshold; set => SetProperty(ref maxThreshold, value); }
        public double MinThreshold { get => minThreshold; set => SetProperty(ref minThreshold, value); }
        public double Average { get => average; set => SetProperty(ref average, value); }
        public bool OutLimits => RealValue > MaxThreshold || RealValue < MinThreshold;
        public uint MessageID { get; set; }
        public int StartBit { get; set; }

        public int Length { get; set; }

        public int ByteOrder { get; set; }

        public uint ValueType { get; set; }

        public double Factor { get; set; }

        public double Offset { get; set; }
        /// <summary>
        /// 固定系数转换
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public virtual double TransForm(double val)
        {
            return val;
        }
        /// <summary>
        /// 实际值改变
        /// </summary>
        public virtual void OnRealValueChanged()
        {
            //RealValue = TransForm(originValue);
            MaxValue = Math.Max(maxValue, RealValue);
            if (MinValue < 0)
                MinValue = RealValue;
            else
                Math.Min(MinValue, RealValue);
            OnPropertyChanged(nameof(OutLimits));
        }
        /// <summary>
        /// 传进一个新的原始值引发
        /// </summary>
        /// <param name="value">originValue</param>
        public virtual void OnOriginValueChaned(double originValue)
        {
            RealValue = TransForm(originValue);
        }
    }

    public class SavingLogicSignal : SignalBase
    {
        public string Group { get; set; }
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

        public override void OnRealValueChanged()
        {
            //base.OnRealValueChanged();
            OnPropertyChanged(nameof(PinHigh));
            OnPropertyChanged(nameof(PinStatus));
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
        LengthQueue<double> TmpValues { get; }

        double StandardDev { get; set; }

        void CalStandard();
    }

    public class LengthQueue<T> : System.Collections.Generic.Queue<T>
    {
        int length = 1;

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
