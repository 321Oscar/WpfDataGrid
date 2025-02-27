using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;

namespace ERad5TestGUI.UDS
{
    [Serializable]
    public class DIDInfo
    {
        public DIDInfo() { }
        public DIDInfo(string name, ushort DID, int length, DIDType type, string data = "", decimal facor = 0)
        {
            this.Name = name;
            this.DID = DID;
            this.DIDType = type;
            this.Length = length;
            Factor = facor;
            Data = data;
        }
        public string Name { get; set; }
        [XmlIgnore]
        public string Data 
        {
            get;
            set ; 
        }

        public ushort DID { get; set; }

        //[JsonConverter(typeof(StringEnumConverter))]
        public DIDType DIDType { get; set; } = DIDType.ASCII;

        /// <summary>
        /// 转换系数，仅当<see cref="DIDType"/> = <see cref="DIDType.HEX"/>时有效；
        /// Factor = 0时，数据按照Hex显示；
        /// 默认为0
        /// </summary>
        public decimal Factor { get; set; } = 0;

        public decimal Offset { get; set; } = 0;

        public int Length
        {
            get;
            set;
        }


        public List<SubDIDInfo> SubDIDs { get; set; }

        public override string ToString()
        {
            return $"0x{DID:X4} {Name}";
        }

        public void Byte2String(byte[] data)
        {
            switch (DIDType)
            {
                case DIDType.HEX:
                    if (Factor != 0)
                    {
                        Data = "";//清空之前的数据
                        if (SubDIDs != null && SubDIDs.Count > 0)
                        {
                            foreach (var item in SubDIDs)
                            {
                                string zerobyte = "";
                                for (int j = 0; j < item.Length; j++)
                                {
                                    zerobyte += data[item.StartIndx + j].ToString("x2");
                                }
                                uint zeroInt = uint.Parse(zerobyte, NumberStyles.HexNumber);
                                Data += $"{zeroInt * item.Factor}#";
                            }
                            Data = Data.Remove(Data.Length - 1, 1);
                        }
                        else
                        {
                            string[] zerobyte = new string[] { data[0].ToString("x2"), data[1].ToString("x2") };
                            uint zeroInt = uint.Parse(zerobyte[0] + zerobyte[1], NumberStyles.HexNumber);
                            Data = (zeroInt * Factor + Offset).ToString();
                        }
                    }
                    else
                        Data = BitConverter.ToString(data.Take(Length).ToArray()).Replace("-", String.Empty);
                    break;
                case DIDType.ASCII:
                    Data = System.Text.Encoding.ASCII.GetString(data.Take(Length).ToArray()); ;
                    //Console.WriteLine(d);
                    break;
                case DIDType.BCD:
                    Data = BitConverter.ToString(data.Take(Length).ToArray()).Replace("-", String.Empty);
                    break;
                default:
                    break;
            }

            //Data = Data.Replace('\0', '*');
        }

        public byte[] DataByte()
        {
            if (string.IsNullOrEmpty(Data))
                return new byte[Length];
            Data = Data.Replace('*', '\0');
            if (DIDType == DIDType.HEX)
            {
                if (this.Factor != 0)
                {
                    bool valid = decimal.TryParse(Data, out decimal zeroValue);
                    if (valid)
                    {
                        uint hollValueMax = (uint)(zeroValue / Factor);
                        byte[] bData = BitConverter.GetBytes(hollValueMax);
                        Array.Reverse(bData);
                        int skip = 4 - Length;
                        skip = skip < 0 ? 0 : skip;
                        return bData.Skip(skip).ToArray();
                    }
                    else
                    {
                        throw new Exception("数据格式错误");
                    }
                }
                return StringDataToBytes(this.Data);
            }
            else if (DIDType == DIDType.ASCII)
            {
                byte[] data = new byte[Length];
                for (int i = 0; i < Math.Min(Data.Length, Length); i++)
                {
                    data[i] = (byte)Data[i];
                }
                return data;
            }
            else
            {
                return StringDataToBytes(this.Data); ;
            }
        }

        private byte[] StringDataToBytes(string str, int count = 2)
        {
            List<byte> list = new List<byte>();
            int num = (int)Math.Ceiling((double)str.Length / (double)count);
            for (int i = 0; i < num; i++)
            {
                int num2 = count * i;
                if (str.Length <= num2)
                {
                    break;
                }
                if (str.Length < num2 + count)
                {
                    list.Add(byte.Parse(str.Substring(num2), NumberStyles.HexNumber));
                }
                else
                {
                    list.Add(byte.Parse(str.Substring(num2, count), NumberStyles.HexNumber));
                }
            }
            return list.ToArray();
        }
    }
}
