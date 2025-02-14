using System;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace WpfApp1.Models
{
    public class AnalogSignal : AverageSignalBase, ICalStandardDev, ITransform2
    {
        
        private string value2;
        private double standardDev;

        public AnalogSignal()
        {
            TmpValues = new LengthQueue<string>(1000);
            MaxThreshold = 5;
            MinThreshold = 0;
        }

        public string PinNumber { get; set; }
        public string ADChannel { get; set; }
        [XmlIgnore]
        public string Value2
        {
            get => value2;
            set
            {
                SetProperty(ref value2, value);
            }
        }
        public override double TransForm(double oldVal)
        {
            return oldVal * 5 / 4096; ;
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
                var realValue = TransForm(originValue);
                //MaxValue = Math.Max(MaxValue, realValue);
                //if (MinValue < 0)
                //    MinValue = realValue;
                //else
                //    MinValue = Math.Min(MinValue, realValue);
                ////cal value1
                //Value1 = TransForm(originValue).ToString(Format);
                //cal value2
                Value2 = TransForm2(realValue).ToString(Format);

                //OutLimits = realValue > MaxThreshold || realValue < MinThreshold;
            }
           
            TmpValues.Enqueue(Value1);
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
            if(Transform2Type == 0)
            {
                return oldVal * TransForm2Factor + TransForm2Offset;
            }
            else
            {
                //get table from tableName
            }

            return oldVal * 2;
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
        public void CalStandard()
        {
            double[] tmpArray = TmpValues.Select(x=>double.Parse(x)).ToArray();
            //TmpValues.CopyTo(tmpArray, 0);

            // 计算平均值
            double mean = tmpArray.Average();

            // 计算方差
            double variance = tmpArray.Select(val => Math.Pow(val - mean, 2)).Average();

            // 计算标准差
            StandardDev = Math.Sqrt(variance);
        }
    }

}
