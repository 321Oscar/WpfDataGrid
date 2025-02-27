using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ERad5TestGUI.UDS.SRecord
{
    public class SrecData : IComparable<SrecData>,INotifyPropertyChanged
	{
		/// <summary>
		/// 字符串数据中，[Record Type] 所占的长度
		/// </summary>
		private const int STR_LENGTH_TYPE = 2;
		/// <summary>
		/// 字符串数据中，[CRC] 所占的长度
		/// </summary>
		private const int STR_LENGTH_CRC = 2;
		private const int UINT_LENGTH_CRC = 1;
		/// <summary>
		/// 字符串数据中，[数据长度]所占的长度
		/// </summary>
		private const int STR_LENGTH_TOTALLENGTH = 2;

		#region Property & Field

		private string recordType;

        /// <summary>
        /// RecordType string 
        /// <para>S3 15 011003A0 03FE79FDF1880100031F03FE7A50F189 DE</para>
        /// <para>RecordType = S3</para>
        /// </summary>
        public string RecordType { get => recordType; private set { recordType = value; RefreshCRC(); OnPropertyChanged(); } }

		private int _dataLength;

		/// <summary>
		/// 数据长度 int 
		/// <para>S3 15 011003A0 03FE79FDF1880100031F03FE7A50F189 DE</para>
		/// <para>DataLength = 0x15 = 21</para>
		/// </summary>
		public int DataLength
		{
			get => _dataLength;
			set
			{
				_dataLength = value;
				RefreshCRC();
			}
		}

		/// <summary>
		/// 数据长度 string 16进制
		/// <para>S3 15 011003A0 03FE79FDF1880100031F03FE7A50F189 DE</para>
		/// <para>DataLengthStr = 15</para>
		/// </summary>
		public string DataLengthStr
		{
			get => _dataLength.HexToString();
            set
            {
                _dataLength = Convert.ToInt32(value, fromBase: 16); OnPropertyChanged();
			}
        }
        /// <summary>
        /// 原先Addr为int类型，数据有误，会转成负数;添加string类型的addr 用于int与uint之间转换
        /// </summary>
        public string AddrStr
        {
            get => addrStr;
            set { addrStr = value; OnPropertyChanged(); }
        }
        private string addrStr;
		/// <summary>
		/// Addr int 
		/// <para>S3 15 011003A0 03FE79FDF1880100031F03FE7A50F189 DE</para>
		/// <para>Addr = 011003A0</para>
		/// </summary>
		public int Addr { get => int.Parse(AddrStr, NumberStyles.HexNumber); set { AddrStr = value.HexToString(AddrLength); RefreshCRC(); } }

        /// <summary>
        /// Addr Uint
        /// <para>S3 15 011003A0 03FE79FDF1880100031F03FE7A50F189 DE</para>
        /// <para>AddrUint = 011003A0</para>
        /// </summary>
        public uint AddrUint 
		{ 
			get => uint.Parse(AddrStr, NumberStyles.HexNumber); 
			set {
                AddrStr = value.HexToString(AddrLength);
                RefreshCRC();
            } 
		}
		private byte[] data;

        /// <summary>
        /// Data string 
        /// <para>S3 15 011003A0 03FE79FDF1880100031F03FE7A50F189 DE</para>
        /// <para>Data = 03FE79FDF1880100031F03FE7A50F189</para>
        /// </summary>
        public string Data
        {
            get
            {
				if (!string.IsNullOrEmpty(_dataStr))
					return _dataStr;
				var builder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    builder.Append(data[i].HexToString());

                }
                return builder.ToString();
            }
            set 
			{
				if (value.Length % 2 > 0)
                {
					_dataStr = value.ToString();
					return;
				}
				_dataStr = string.Empty;
				data = value.StringDataToBytes(2); 
				this.DataLengthStr = (data.Length + this.AddrLength /2 + 1).ToString("X");
				RefreshCRC();
				OnPropertyChanged();
			}
        }
		private string _dataStr = string.Empty;

		public byte[] DataBytes { get => data; set => data = value; }

        /// <summary>
        /// CRC String 
        /// <para>S3 15 011003A0 03FE79FDF1880100031F03FE7A50F189 DE</para>
        /// <para>CRC = DE</para>
        /// </summary>
        public string CRC { get=> crc; private set { crc = value; OnPropertyChanged(); }  }
		private string crc;
		/// <summary>
		/// 地址长度 int ,用来截取字符串/转换字符串
		/// <para>S3 15 011003A0 03FE79FDF1880100031F03FE7A50F189 DE</para>
		/// <para>addrLength = 8</para>
		/// </summary>
		public int AddrLength { get => GetStrAddrLengthByType(this.RecordType, out _); }

		/// <summary>
		/// 是否全是FF
		/// </summary>
		public bool ISFF
		{
			get
			{
				return ToByte().All(x => x == 0xff);
			}
		}

		/// <summary>
		/// 是否全是00
		/// </summary>
		public bool IS00
		{
			get
			{
				return ToByte().All(x => x == 0);
			}
		}
		

		/// <summary>
		/// 数据长度，不包括地址，校验
		/// <para>S3 15 011003A0 03FE79FDF1880100031F03FE7A50F189 DE</para>
		/// <para>DataLength_OnlyData = 16</para>
		/// </summary>
		public int DataLength_OnlyData => Data.Length / 2;

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

		#endregion Property & Field
		public SrecData() { }

		/// <summary>
		/// 传入信息，自动计算CRC
		/// </summary>
		/// <param name="recordType"></param>
		/// <param name="datalength"></param>
		/// <param name="addr"></param>
		/// <param name="data"></param>
		/// <param name="addrlength"></param>
		public SrecData(string recordType, int datalength, int addr, string data, int addrlength)
		{
			RecordType = recordType;
			DataLength = datalength;
			Addr = addr;
			//AddrLength = addrlength;
			Data = data;
			RefreshCRC();
			//CRC = SrecHelper.CalCRC(data: this.ToByte(),
			//startPosition: (uint)this.Addr,
			//        recordType: recordType,
			//        lineLength: Convert.ToByte(DataLength)).ToString("X").PadLeft(2, '0').ToUpper();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="recordType"></param>
		/// <param name="datalength">数据长度，不包括CRC和地址</param>
		/// <param name="addr"></param>
		/// <param name="data"></param>
		/// <param name="eightOrsixteen"></param>
		public SrecData(string recordType, int datalength, uint addr, byte[] data,int eightOrsixteen = 1 )
        {
            int addrStrLength = GetStrAddrLengthByAddr(addr, out recordType);
            RecordType = recordType;
            DataLength = (addrStrLength / 2) + datalength + UINT_LENGTH_CRC;
            AddrUint = addr;
            DataBytes = data;
            RefreshCRC();
        }

		/// <summary>
		/// string 转成 SrecData(S3 15 011003A0 03FE79FDF1880100031F03FE7A50F189 DE)
		/// </summary>
		/// <param name="data"></param>
		/// <param name="addrLength">地址长度</param>
		public SrecData(string data)
		{
            try
            {
                //S3 15 011003A0 03FE79FDF1880100031F03FE7A50F189 DE
                this.RecordType = data.Substring(0, STR_LENGTH_TYPE);
                //this.AddrLength = GetTypeAddrLength(RecordType, out _);
                this.DataLength = Convert.ToInt32(data.Substring(STR_LENGTH_TYPE, STR_LENGTH_TOTALLENGTH), 16);
                this.Addr = int.Parse(data.Substring((STR_LENGTH_TOTALLENGTH + STR_LENGTH_TYPE), this.AddrLength), System.Globalization.NumberStyles.HexNumber);
                //从地址后开始取，长度为总长度-crc长度("DE",固定2)-地址长度("011003A0")-类型长度("S3",固定2)-数据长度值的长度("15",固定2)
                //长度为数据长度*2（字符串长度）- 地址长度 - CRC长度
                this.Data = data.Substring((STR_LENGTH_TOTALLENGTH + STR_LENGTH_TYPE) + this.AddrLength, DataLength * 2 - this.AddrLength - STR_LENGTH_CRC);
                //判断是否有CRC
                if (data.Length > STR_LENGTH_TYPE + STR_LENGTH_TOTALLENGTH + DataLength * 2 - STR_LENGTH_CRC)
                {
                    string fileCRC = data.Substring(data.Length - STR_LENGTH_CRC);
                    if (!fileCRC.Equals(this.CRC))
                        Console.WriteLine("CRC Error");
                }
            }
            catch (ArgumentOutOfRangeException)
            {
				throw new ArgumentOutOfRangeException($"[{data}]解析错误，长度不对");
            }
			
        }
		/// <summary>
		/// SREC 当行数据
		/// </summary>
		/// <param name="data"></param>
		/// <param name="hasCrc">若没有CRC 则重新计算</param>
		public SrecData(string data, bool hasCrc):this(data)
		{
            if (!hasCrc)
            {
                this.Data = data.Substring(STR_LENGTH_TOTALLENGTH + STR_LENGTH_TYPE + this.AddrLength, data.Length - STR_LENGTH_TYPE - this.AddrLength - STR_LENGTH_TOTALLENGTH);
                //this.RefreshCRC();
            }
        }

		/// <summary>
		/// 类型之间转换
		/// </summary>
		/// <param name="data">数据：S3 15 011003A0 03FE79FDF1880100031F03FE7A50F189 DE</param>
		/// <param name="sourceAddrLength">原地址长度</param>
		/// <param name="targeAddrLen">目标地址长度</param>
		/// <param name="targeType">目标类型</param>
		/// <param name="addrOffset">是否需要改变地址偏移量</param>
		public SrecData(string data, byte sourceAddrLength, byte targeAddrLen, string targeType, int? addrOffset):this(data)
		{
            this.ChangeType(targeType);
            if (addrOffset.HasValue)
                this.Addr += addrOffset.Value;
            //this.AddrLength = targeAddrLen * 2;//转成app的长度
            //this.RecordType = targeType;//转成app的类型
            //this.DataLengthStr = (int.Parse(data.Substring(2, 2), NumberStyles.HexNumber) + (targeAddrLen - sourceAddrLength)).ToString("X").PadLeft(2, '0');
            //if (addrOffset == null)
            //	this.Addr = Convert.ToInt32(data.Substring(4, sourceAddrLength * 2), 16);
            //else
            //{
            //	this.Addr = Convert.ToInt32(data.Substring(4, sourceAddrLength * 2), 16) + addrOffset.Value;
            //	//this.Addr = addr.Value;
            //}

            //this.Data = data.Substring(4 + sourceAddrLength * 2, data.Length - 2 - 4 - sourceAddrLength * 2);
            //RefreshCRC();
			//this.CRC = SrecHelper.CalCRC(this.ToByte(), (uint)this.Addr, targeType, Convert.ToByte(this.DataLengthStr, 16)).ToString("X").PadLeft(2, '0').ToUpper();//.Substring(newline.Length - 2);
		}

		#region Pubilc

		/// <summary>
		/// 根据长度填充一行数据；有缺陷，若和下一行是连续的，填充之后会导致错误
		/// </summary>
		/// <param name="length"></param>
		/// <param name="fillData"></param>
		[Obsolete("有缺陷，若和下一行是连续的，填充之后会导致错误")]
		public void Fill(int length, uint fillData)
		{
			int add = length - this.DataLength_OnlyData;
			if (add > 0)
			{
				_dataLength += add;

				for (int i = 0; i < add; i++)
				{
					this.Data += fillData.HexToString();
				}
			}
			RefreshCRC();
		}

		/// <summary>
		/// 替换数据. 不能比原数据长
		/// </summary>
		/// <param name="data">替换的数据</param>
		/// <param name="startidx">替换的起始位</param>
		public void Replace(byte[] data, int startidx)
		{
			if (data.Length > this.DataLength_OnlyData)
				return;
			var oldData = Data.StringDataToBytes(2);
			Console.WriteLine($"Replace Old: {this}");
			for (int i = 0; i < data.Length; i++)
			{
				oldData[startidx + i] = data[i];
			}
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < oldData.Length; i++)
			{
				builder.Append(oldData[i].HexToString());

			}
			this.Data = builder.ToString();
			Console.WriteLine($"New:{this}");
		}

		/// <summary>
		/// 修改类型，主要是 recordType，addrlength,CRC
		/// </summary>
		/// <param name="type"></param>
		public bool ChangeType(string type)
		{
			int newaddrlength = GetStrAddrLengthByType(type, out uint max);
			if (this.Addr > max)
			{
				return false;
			}
			//更新数据长度
			DataLength += (newaddrlength - AddrLength) / 2;
			//this.AddrLength = newaddrlength;
			//更新类型
			this.RecordType = type;
			//修改地址的长度
			AddrStr = AddrUint.HexToString(AddrLength);
			return true;
		}

		public void RefreshCRC()
		{
			if (string.IsNullOrEmpty(AddrStr))
				return;
			if (DataBytes == null || DataBytes.Length == 0)
				return;

			List<byte> crcdata = new List<byte>();
			crcdata.Add((byte)DataLength);

			byte[] addr = AddrStr.StringDataToBytes(2);
			crcdata.AddRange(addr);

			crcdata.AddRange(DataBytes);

			var sum = crcdata.Sum(x => x);
			var CRCbyte = (byte)(~sum);

			CRC = CRCbyte.HexToString();
			//CRC = SrecHelper.CalCRC(this.ToByte(), (uint)this.Addr, RecordType, Convert.ToByte(this.DataLengthStr, 16)).ToString("X").PadLeft(2, '0').ToUpper();
		}

		public byte[] ToByte()
		{
			return DataBytes;
		}
		#endregion Pubilc

		#region Private
		

		public static int GetStrAddrLengthByType(string RecordType, out uint max)
		{
			if (RecordType == "S1")
			{
				max = 0xFFFF;
				return 4;
			}

			else if (RecordType == "S2")
			{
				max = 0xFFFFFF;
				return 6;
			}

			else if (RecordType == "S3")
			{
				max = 0xFFFFFFFF;
				return 8;
			}
			else
			{
				max = 0xFFFF;
				return 4;
			}

		}

        public static int GetStrAddrLengthByAddr(uint addr, out string type)
        {
            if (addr < 0xffff)
            {
                type = "S1";
                return 4;
            }
            else if (addr < 0xffffff)
            {
                type = "S2";
                return 6;
            }
            else
            {
                type = "S3";
                return 8;
            }
        }


        #endregion Private

        public int CompareTo(SrecData other)
		{
			return AddrUint.CompareTo(other.AddrUint);
		}

		public override string ToString()
		{
			return RecordType + DataLengthStr + AddrStr + Data + CRC;
		}
	}

	public static class HexToStringClass
    {
		public static string HexToString(this uint hex, int length = 2)
		{
			return hex.ToString($"X{length}");
		}

		public static string HexToString(this int hex, int length = 2)
		{
			return hex.ToString($"X{length}");
		}

		public static string HexToString(this byte hex)
		{
			return hex.ToString("X2");
		}

		/// <summary>
		/// 将字符串转为byte数组
		/// </summary>
		/// <param name="str">字符串</param>
		/// <param name="count">字符串分割长度</param>
		/// <returns></returns>
		public static byte[] StringDataToBytes(this string str, int count)
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
