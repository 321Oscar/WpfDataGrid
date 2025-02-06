using System;
using System.ComponentModel;
using System.Linq;

namespace WpfApp1.Models
{
    public class AnalogSignal : SignalBase, ICalStandardDev, ITransform2
    {
        private double value1;
        private double standardDev;

        public AnalogSignal()
        {
            TmpValues = new LengthQueue<double>(100);
            MaxThreshold = 5;
            MinThreshold = 0;
        }

        public string PinNumber { get; set; }
        public string ADChannel { get; set; }

        public double Value1
        {
            get => value1;
            set
            {
                SetProperty(ref value1, value); 
            }
        }

        public override void OnRealValueChanged()
        {
            base.OnRealValueChanged();
            Value1 = TransForm2(RealValue);
        }

        public override void OnOriginValueChaned(double originValue)
        {
            base.OnOriginValueChaned(originValue);
            TmpValues.Enqueue(RealValue);
        }

        /// <summary>
        /// 0:系数；1：查表
        /// </summary>
        public int Transform2Type { get; set; }

        public override double TransForm(double oldVal)
        {
            return oldVal * 5 / 4096;
        }
        /// <summary>
        /// 根据每个信号的系数/表
        /// </summary>
        /// <param name="oldVal"></param>
        /// <returns></returns>
        public double TransForm2(double oldVal)
        {
            return oldVal * 2;
        }

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
        public LengthQueue<double> TmpValues { get; }
        public void CalStandard()
        {
            double[] tmpArray = new double[TmpValues.Count];
            TmpValues.CopyTo(tmpArray, 0);

            // 计算平均值
            double mean = tmpArray.Average();

            // 计算方差
            double variance = tmpArray.Select(val => Math.Pow(val - mean, 2)).Average();

            // 计算标准差
            StandardDev = Math.Sqrt(variance);
        }
    }

}
