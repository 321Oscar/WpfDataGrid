using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Serialization;

namespace WpfApp1.UDS
{
    [Serializable]
    public class SnapDIDInfo
    {
        public SnapDIDInfo() { }

        public string Name { get; set; }
        /// <summary>
        /// byte 长度
        /// </summary>
        public int Length {get;set;}

        public List<SnapSubDIDInfo> SubDIDInfos { get;set;}
        [XmlIgnore]
        public Action<byte[], SnapSubDIDInfo> Parse = null;
        public void ParseByte(byte[] b)
        {
            foreach (var signal in SubDIDInfos)
            {
                if(Parse != null)
                {
                    Parse(b,signal);
                }
                else
                {
                    int len_rem1 = (int)(8 - (signal.StartBit % 8));
                    int byte_start = (int)(signal.StartBit / 8);
                    int len_rem2 = (int)((signal.StartBit + signal.Length) % 8);
                    int byte_end = (int)((signal.StartBit + signal.Length) / 8);
                    byte_end = byte_end == 0 ? byte_end : byte_end - 1;
                    long tmp = 0;
                    if ((byte_start + 1) <= byte_end)
                    {
                        List<string> bytestr = new List<string>();
                        for (int i = byte_start; i <= byte_end; i++)
                        {
                            bytestr.Add(b[i].ToString("x2"));
                        }
                        string zerb = string.Join("", bytestr.ToArray());
                        tmp = uint.Parse(zerb, NumberStyles.HexNumber);
                    }
                    else
                    {
                        tmp = (b[byte_start] % (int)Math.Pow(2, len_rem2)) >> (8 - len_rem1);
                    }

                    decimal tmp_value = (tmp * signal.Factor) + signal.Offset;

                    signal.StrVal = tmp_value.ToString();
                }
                
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
