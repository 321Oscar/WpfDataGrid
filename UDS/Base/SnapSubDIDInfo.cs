using System;
using System.Xml.Serialization;

namespace WpfApp1.UDS
{
    [Serializable]
    public class SnapSubDIDInfo
    {
        public string Name { get; set; }
        /// <summary>
        /// bit
        /// </summary>
        public int StartBit { get; set; }
        /// <summary>
        /// bit 长度
        /// </summary>
        public int Length { get; set; }
        public decimal Factor { get; set; }
        public decimal Offset { get; set; }
        [XmlIgnore]
        public string StrVal { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
