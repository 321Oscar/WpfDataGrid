using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERad5TestGUI.Converters;

namespace ERad5TestGUI.UDS.SRecord
{
    public class SrecFile
    {
        public string RecordType
        {
            get
            {
                if (srecDatas.Count == 0)
                    return type;
                return srecDatas[0].RecordType;
            }
        }

        readonly string type;
        /// <summary>
        /// 不包含头尾的数据
        /// </summary>
        public List<SrecData> Content { get => srecDatas; }

        readonly List<SrecData> srecDatas;
        Dictionary<uint, List<byte>> addrData;
        /// <summary>
        /// 按照地址存储的连续数据，按照地址大小排序
        /// </summary>
        public Dictionary<uint, List<byte>> AddrData
        {
            get
            {
                List<SrecData> tempData = new List<SrecData>();
                srecDatas.ForEach(x => tempData.Add(x));
                tempData.Sort();
                addrData = new Dictionary<uint, List<byte>>();

                uint lastAddr = 0;
                for (int i = 0; i < srecDatas.Count; i++)
                {
                    if (i == 0)
                    {
                        var bytes = new List<byte>();
                        bytes.AddRange(tempData[i].ToByte());
                        addrData.Add(tempData[i].AddrUint, bytes);
                        lastAddr = tempData[i].AddrUint;
                    }
                    else
                    {
                        //连续的数据
                        if ((tempData[i].Addr - tempData[i - 1].Addr) * this.EightOrsixteen == tempData[i - 1].DataLength_OnlyData)
                        {
                            addrData[lastAddr].AddRange(srecDatas[i].ToByte());
                        }
                        else
                        {
                            var bytes = new List<byte>();
                            bytes.AddRange(tempData[i].ToByte());
                            addrData.Add(tempData[i].AddrUint, bytes);
                            lastAddr = tempData[i].AddrUint;
                        }
                    }
                }

                return addrData;

            }
        }

        public Dictionary<uint, List<byte>> AddrDataUnsort
        {
            get
            {
                List<SrecData> tempData = new List<SrecData>();
                srecDatas.ForEach(x => tempData.Add(x));
                //tempData.Sort();
                addrData = new Dictionary<uint, List<byte>>();

                uint lastAddr = 0;
                for (int i = 0; i < srecDatas.Count; i++)
                {
                    if (i == 0)
                    {
                        var bytes = new List<byte>();
                        bytes.AddRange(tempData[i].ToByte());
                        addrData.Add(tempData[i].AddrUint, bytes);
                        lastAddr = tempData[i].AddrUint;
                    }
                    else
                    {
                        //连续的数据
                        if ((tempData[i].Addr - tempData[i - 1].Addr) * this.EightOrsixteen == tempData[i - 1].DataLength_OnlyData)
                        {
                            addrData[lastAddr].AddRange(srecDatas[i].ToByte());
                        }
                        else
                        {
                            var bytes = new List<byte>();
                            bytes.AddRange(tempData[i].ToByte());
                            addrData.Add(tempData[i].AddrUint, bytes);
                            lastAddr = tempData[i].AddrUint;
                        }
                    }
                }

                return addrData;

            }
        }

        string srecHeader;
        string srecEnd;

        byte lineLength;
        private int eightOrsixteen = 1;
        private string fileName = "NewFile";
        readonly byte addrLength = 0;
        /// <summary>
        /// 计算数据长度的长度
        /// </summary>
        public int EightOrsixteen
        {
            get => eightOrsixteen;
            private set
            {
                eightOrsixteen = value;
                Console.WriteLine($"Open/Create File:[{FileName}], FileType(8/16):[{eightOrsixteen * 8}]");
            }
        }
        public byte AddrLength
        {
            get
            {
                if (srecDatas.Count == 0)
                    return addrLength;
                SrecHelper.GetSType(RecordType, out byte length, out _, out _);
                return length;
            }
        }
        /// <summary>
        /// 纯数据长度，不包括地址，CRC
        /// </summary>
        public int Datalength
        {
            get
            {
                return lineLength;
            }
            private set
            {
                lineLength = (byte)value;
            }
        }

        public uint StartPosition
        {
            get
            {
                if (srecDatas.Count == 0)
                    return 0;
                else
                {
                    srecDatas.Sort();
                    return srecDatas[0].AddrUint;
                }
            }
        }

        public string SrecHeader { get => srecHeader; set => srecHeader = value; }
        public string SrecEnd { get => srecEnd; set => srecEnd = value; }
        public string FileName { get => fileName; private set { fileName = value; } }   
        /// <summary>
        /// 新建一个空的SREC文件，输入类型和单行数据长度
        /// </summary>
        /// <param name="type"></param>
        /// <param name="datalength"></param>
        public SrecFile(string type, byte datalength, int eightOrsixteen = 1)
        {
            this.EightOrsixteen = eightOrsixteen;
            this.type = type;
            SrecHelper.GetSType(type, out addrLength, out _, out _);
            lineLength = datalength;
            srecDatas = new List<SrecData>();
        }
        /// <summary>
        /// 从srec文件中读取数据
        /// </summary>
        /// <param name="filePath">文件路径</param>
        public SrecFile(string filePath, int eightOrsixteen = 1)
        {
            FileName = filePath;
            this.EightOrsixteen = eightOrsixteen;
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (StreamReader sw = new StreamReader(fs, Encoding.Default))
            {
                srecDatas = new List<SrecData>();
                string newline;
                while ((newline = sw.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(newline))
                        continue;
                    if (newline.Substring(0, 2) == "S0")
                    {
                        SrecHeader = newline;
                    }
                    else if (!SrecHelper.cmdEndStrs.Contains(newline.Substring(0, 2)))// != cmdEndStr)
                    {
                        //if (addrLength == 0)
                        //{
                        //	type = newline.Substring(0, 2);

                        //	SrecHelper.GetSType(newline.Substring(0, 2), out addrLength, out lineLength, out _);
                        //}
                        SrecData srecData = new SrecData(newline);
                        if (srecData.DataLength_OnlyData > Datalength)
                        {
                            Datalength = (byte)srecData.DataLength_OnlyData;
                        }
                        srecDatas.Add(srecData);
                    }
                    else
                    {
                        SrecEnd = newline;
                    }
                }
            }
        }

        public void Add(string data)
        {
            SrecData srecdata = new SrecData(data);
            this.Add(srecdata);
        }
        /// <summary>
        /// 插入数据
        /// </summary>
        /// <param name="srecLine"></param>
        /// <param name="index"></param>
        public void Insert(SrecData srecLine, int index)
        {
            srecDatas.Insert(index, srecLine);
        }
        /// <summary>
        /// 插入多条数据
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="index"></param>
        public void Insert(List<SrecData> lines, int index)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                srecDatas.Insert(index + i, lines[i]);
            }
        }

        /// <summary>
        /// 添加单个数据，若地址存在数据， 替换相应长度数据 //且数据全为00或全为FF
        /// </summary>
        /// <param name="srecData"></param>
        public void Add(SrecData srecData, int index = 0)
        {
            //var exist = srecDatas.Find(x => x.Addr == srecData.Addr);
            if (srecDatas.Count == 0)
            {
                srecDatas.Add(srecData);
            }
            int idx = srecDatas.FindIndex(x => x.Addr == srecData.Addr);
            if (idx >= 0)
            {
                srecDatas[idx].Replace(srecData.ToByte(), index);
                //if (srecDatas[idx].IS00 || srecDatas[idx].ISFF)
                //{
                //	srecDatas[idx].Replace(srecData.ToByte(), index);
                //	Console.WriteLine($"Update Info:{srecDatas[idx]}");
                //}
            }
            else
            {
                srecDatas.Add(srecData);
            }


            //srecDatas.Sort();
        }
        /// <summary>
        /// 添加数据，不去重
        /// </summary>
        /// <param name="content"></param>
        public void AddRange(List<SrecData> content)
        {
            //确保 加入的数据 类型一致，S1 or S2 or S3
            if (content[0].RecordType != this.RecordType)
            {
                foreach (var srecData in content)
                {
                    srecData.ChangeType(this.RecordType);
                }
            }

            srecDatas.AddRange(content);
            srecDatas.Sort();
            //一个个加太慢
            //content.AsParallel().ForAll((x) => { Add(x); });

            //foreach (var item in content)
            //{
            //    Add(item);
            //}
        }

        public void Add(byte[] data, uint startPosition, string 调用方 = "", int index = 0)
        {
            //if (startPosition >= this.StartPosition)
            //{
            //    //判断该地址是否是数据行的开头
            //    var offset = (int)(startPosition - this.StartPosition) % this.Datalength;

            //    //在其中，找到包含该地址的数据行，覆盖数据
            //    var closePostion = startPosition - offset;
            //    int position_idx = Content.FindIndex(x => x.AddrUint == closePostion);

            //    if (position_idx < 0)//没有该地址的原数据
            //    {
            //        foreach (SrecData srecdata in BytesToSrecData(data, Datalength, startPosition))
            //        {
            //            this.Add(srecdata);
            //            Console.WriteLine($"Add {调用方} Config Info:{srecdata}");
            //        }
            //    }
            //    else
            //    {
            //        //first data
            //        Content[position_idx].Replace(data.Take(Datalength - offset).ToArray(), (int)offset);
            //        int forCount = (data.Length + offset) / Datalength;
            //        //intermediate data
            //        if (forCount > 1)
            //        {
            //            for (int i = 1; i < forCount; i++)
            //            {
            //                Content[position_idx + i].Replace(data.Skip(Datalength - offset + Datalength * (i - 1)).Take(Datalength).ToArray(), 0);
            //            }
            //        }
            //        //last one data
            //        if (forCount > 0)
            //            Content[position_idx + forCount].Replace(data.Skip(Datalength - offset + Datalength * (forCount - 1)).ToArray(), 0);
            //    }
            //}
            //else
            //{
#if DEBUG
            int cutCount = 0x20 * 5000;

            List<byte[]> cutDatas = new List<byte[]>();
            int remainLength = data.Length;
            int takedCount = 0;
            if (data.Length > cutCount)
            {
                List<Task> tasks = new List<Task>();
                do
                {
                    List<byte> cutData = new List<byte>();
                    int takeCount = Math.Min(cutCount, remainLength);
                    cutData.AddRange(data.Skip(takedCount).Take(takeCount));
                    uint startpositiontemp = (uint)(startPosition + takedCount);
                    tasks.Add(Task.Run(new Action(() =>
                    {
                        foreach (SrecData srecdata in BytesToSrecDataYeild(cutData.ToArray(), Datalength, startpositiontemp))
                        {
                            if (srecdata != null)
                                this.Add(srecdata, index);
                        }
                    })));
                    remainLength -= takeCount;
                    takedCount += takeCount;
                } while (remainLength > 0);

                System.Threading.Tasks.Task.WaitAll(tasks.ToArray());
                srecDatas.Sort();
            }
            else
#endif
            {
                foreach (SrecData srecdata in BytesToSrecDataYeild(data, Datalength, startPosition))
                {
                    if (srecdata != null)
                        this.Add(srecdata, index);
                    //Console.WriteLine($"Add {调用方} Config Info:{srecdata}");
                }
            }


            //}

        }

        /// <summary>
        /// 数据类型转换
        /// </summary>
        /// <param name="type"></param>
        public void ChangeType(string type)
        {
            srecDatas.Sort();
            SrecData.GetStrAddrLengthByType(type, out uint Max);

            if (srecDatas[srecDatas.Count - 1].AddrUint > Max)
            {
                Console.WriteLine($"类型转换失败：地址太大{srecDatas[srecDatas.Count - 1].AddrUint}");
                return;
            }

            foreach (var data in srecDatas)
            {
                if (!data.ChangeType(type))
                {
                    Console.WriteLine("转换失败");
                }
            }
        }
        /// <summary>
        /// 修改地址
        /// </summary>
        /// <param name="startAddress">目标地址</param>
        /// <param name="newAddress">修改后的地址</param>
        public void ChangeAddr(int startAddress, int newAddress)
        {
            var data = srecDatas.Where(x => x.Addr >= startAddress);
            int interval = 0;
            if (startAddress == 0)
            {
                interval = newAddress - srecDatas[0].Addr;
            }
            else
            {
                interval = newAddress - startAddress;
            }

            foreach (var item in data)
            {
                item.Addr += interval;
            }
        }

        public void ChangeAddr(uint startAddress, uint newAddress)
        {
            var data = srecDatas.Where(x => x.AddrUint >= startAddress);
            uint interval = 0;
            if (startAddress == 0)
            {
                interval = (newAddress - srecDatas[0].AddrUint);
            }
            else
            {
                interval = (newAddress - startAddress);
            }

            foreach (var item in data)
            {
                item.AddrUint += interval;
            }
        }

        /// <summary>
        /// 改变数据长度，并填充数据
        /// </summary>
        /// <param name="datalength"></param>
        /// <param name="fillData">是否需要填充</param>
        /// <exception cref="Exception">填充地址错误</exception>
        public void ChangeDataLength(int datalength, bool needFill = true, byte fillData = 0xff)
        {
            List<SrecData> allNewData = new List<SrecData>();

            if (needFill)
            {
                List<byte> data = new List<byte>();
                uint lastAddr = 0;
                foreach (var addrAndData in this.AddrData)
                {
                    if (lastAddr == 0)
                    {
                        data.AddRange(addrAndData.Value);
                        lastAddr = addrAndData.Key + (uint)(addrAndData.Value.Count / EightOrsixteen);
                    }
                    else
                    {
                        //计算出的上一条数据的末尾地址比当前的大，说明该数据应该包含在里面，而非单独另起一行
                        //if (lastAddr > addrAndData.Key)
                        //	throw new Exception("地址错误");
                        //计算上一条数据的末尾地址与当前数据的起始地址之间的差额，若为0则不需要填充数据
                        if (lastAddr > addrAndData.Key)
                        {
                            throw new Exception($"填充地址错误，上一行[{lastAddr:X}]地址比下一行[{addrAndData.Key:X}]地址大");
                        }
                        uint interval = addrAndData.Key - lastAddr;
                        for (int i = 0; i < interval * EightOrsixteen; i++)
                        {
                            data.Add(fillData);
                        }
                        //Console.WriteLine($"add {interval * eightOrsixteen} 0xff");
                        data.AddRange(addrAndData.Value);
                        lastAddr = addrAndData.Key + (uint)(addrAndData.Value.Count / EightOrsixteen);

                    }
                }

                allNewData = BytesToSrecData(data.ToArray(), datalength, this.srecDatas[0].AddrUint);

            }
            else
            {
                foreach (var item in this.AddrData)
                {
                    var newAddrData = BytesToSrecData(item.Value.ToArray(), datalength, item.Key);
                    allNewData.AddRange(newAddrData);
                }
            }
            this.Datalength = datalength;
            this.srecDatas.Clear();
            this.srecDatas.AddRange(allNewData);
        }
        /// <summary>
        /// 获取CRC，起始地址，数据总长度，并根据大小端 改变byte顺序
        /// </summary>
        /// <param name="startAddr">ref </param>
        /// <param name="expectedvalue">ref</param>
        /// <param name="byteCount">ref</param>
        /// <param name="byteorder">大小端;<para>Modify at 2024/06/19:改为0，大小端不在这里体现</para></param>
        /// <param name="fillData">计算CRC时，空白地址中填充的数据默认为0xFF</param>
        public void GetInfo(ref uint startAddr, ref uint expectedvalue, ref uint byteCount, int byteorder = 0, byte fillData = 0xff)
        {
            uint crc32 = GetCRC(out int length, fillData);
            if (byteorder == 1)
            {
                //startAddr = (uint)IPAddress.NetworkToHostOrder(srecDatas[0].Addr);
                //expectedvalue = (uint)IPAddress.NetworkToHostOrder(crc32);
                //byteCount = (uint)IPAddress.NetworkToHostOrder((int)length);	
                if (EightOrsixteen == 1)
                {
                    startAddr = StartPosition.Convert(new UInt32Reverse());
                    expectedvalue = crc32.Convert(new UInt32Reverse());
                    byteCount = ((uint)length).Convert(new UInt32Reverse());
                    Console.WriteLine($"StartAddr:初始值：{srecDatas[0].Addr:X}，转换后：{startAddr:X}");
                    Console.WriteLine($"Len:初始值：{length:X}，转换后：{byteCount:X}");
                    Console.WriteLine($"crc:初始值：{crc32:X}，转换后：{expectedvalue:X}");
                }
                if (EightOrsixteen == 2)
                {
                    startAddr = StartPosition.Convert(new UInt3216Reverse());
                    expectedvalue = crc32.Convert(new UInt3216Reverse());
                    byteCount = ((uint)length).Convert(new UInt3216Reverse());
                    Console.WriteLine($"StartAddr:初始值：{srecDatas[0].Addr:X}，按照16位转换后：{startAddr:X}");
                    Console.WriteLine($"Len:初始值：{length:X}，按照16位转换后：{byteCount:X}");
                    Console.WriteLine($"crc:初始值：{crc32:X}，按照16位转换后：{expectedvalue:X}");
                }
            }
            else
            {
                startAddr = StartPosition;
                expectedvalue = crc32;
                byteCount = (uint)(length);
                Console.WriteLine($"StartAddr:{startAddr:X}");
                Console.WriteLine($"Len：{byteCount:X}");
                Console.WriteLine($"crc：{expectedvalue:X}");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="startAddr"></param>
        /// <param name="expectedvalue"></param>
        /// <param name="byteCount"></param>
        /// <param name="byteorder">大小端;<para>Modify at 2024/06/19:改为 0 ，大小端不在这里体现</para></param>
        /// <param name="fillData"></param>
        public void GetCmacInfo(ref uint startAddr, ref byte[] expectedvalue, ref uint byteCount, int byteorder = 0, byte fillData = 0xff)
        {
            byte[] cmac = GetCmac(out int length, fillData);
            if (byteorder == 1)
            {
                if (EightOrsixteen == 1)
                {
                    startAddr = StartPosition.Convert(new UInt32Reverse());
                    expectedvalue = cmac;// cmac.Convert(new UInt32Reverse());
                    byteCount = ((uint)length).Convert(new UInt32Reverse());
                    Console.WriteLine($"StartAddr:初始值：{srecDatas[0].Addr:X}，转换后：{startAddr:X}");
                    Console.WriteLine($"Len:初始值：{length:X}，转换后：{byteCount:X}");
                    Console.WriteLine($"cmac：{string.Join("", cmac)}");
                }
                if (EightOrsixteen == 2)
                {
                    startAddr = StartPosition.Convert(new UInt3216Reverse());
                    expectedvalue = cmac;
                    byteCount = ((uint)length).Convert(new UInt3216Reverse());
                    Console.WriteLine($"StartAddr:初始值：{srecDatas[0].Addr:X}，按照16位转换后：{startAddr:X}");
                    Console.WriteLine($"Len:初始值：{length:X}，按照16位转换后：{byteCount:X}");
                    Console.WriteLine($"cmacc:初始值：{string.Join("", cmac)}");
                }
            }
            else
            {
                startAddr = StartPosition;
                //Array.Reverse(cmac);
                expectedvalue = cmac;
                byteCount = (uint)(length);
                Console.WriteLine($"StartAddr:{startAddr:X}");
                Console.WriteLine($"Len：{byteCount:X}");
                Console.WriteLine($"cmac:转换后：{string.Join("", cmac)}");
            }
        }

        /// <summary>
        /// 填充最后一行
        /// </summary>
        /// <param name="fillData"></param>
        public void FillLastRecord(uint fillData = 0xff)
        {
            srecDatas.Sort();
            srecDatas[srecDatas.Count - 1].Fill(Datalength, fillData);
        }

        /// <summary>
        /// 输出文件
        /// </summary>
        /// <param name="outpuFile"></param>
        public void Output(string outpuFile, uint fillData = 0xff, bool addFill = false, bool isSort = true)
        {
            if (string.IsNullOrEmpty(outpuFile))
                outpuFile = $"{DateTime.Now:MMdd-HHmmss}.srec";
            if (outpuFile.IndexOf(".srec") < 0 || 
                outpuFile.IndexOf(".s19") < 0)
                outpuFile += ".srec";
            if (isSort)
                srecDatas.Sort();

            if (addFill)
            {
                FillLastRecord(fillData);
            }

            using (StreamWriter file = new StreamWriter(outpuFile, false))
            {
                file.WriteLine(SrecHeader);
                foreach (var item in srecDatas)
                {
                    file.WriteLine(item.ToString());
                }
                file.WriteLine(SrecEnd);
            }
            Console.WriteLine($"Output File Path:[{outpuFile}].");
        }

        public string Show()
        {
            StringBuilder allData = new StringBuilder();
            foreach (var item in srecDatas)
            {
                allData.AppendLine(item.ToString());
            }

            return allData.ToString();

            //double input = 1023.1024;
            //input.Convert(new DoubleToIntSetp()).Convert(new IntToStringStep());
        }

        /// <summary>
        /// 生成SRecord数据
        /// </summary>
        /// <param name="data"></param>
        /// <param name="datalength">数据长度，不包括CRC和地址</param>
        /// <param name="startPosition">起始地址</param>
        /// <returns></returns>
        private List<SrecData> BytesToSrecData(byte[] data, int datalength, uint startPosition)
        {
            var newData = new List<SrecData>();
            if (data.Length < datalength)
            {
                SrecData srecData = new SrecData(RecordType, data.Length, startPosition, data, EightOrsixteen);
                newData.Add(srecData);
            }
            else
            {
                int count = data.Length / datalength + ((data.Length % datalength > 0) ? 1 : 0);
                int 余数 = data.Length % datalength == 0 ? datalength : data.Length % datalength;
                uint position = startPosition;
                for (int i = 0; i < count; i++)
                {
                    int length = datalength;

                    if (i + 1 == count)//最后一个
                    {
                        length = 余数;
                    }
                    byte[] lineData = new byte[length];
                    //string dataStr = type + (length + addrLength + 1).HexToString() + position.HexToString(addrLength * 2);
                    for (int j = 0; j < length; j++)
                    {
                        lineData[j] = data[j + i * datalength];
                    }
                    //SrecData srecData = new SrecData(dataStr, false);
                    SrecData srecData = new SrecData(RecordType, length, position, lineData, EightOrsixteen);
                    position += (uint)(length / EightOrsixteen);
                    newData.Add(srecData);
                }
            }

            return newData;
        }

        private IEnumerable<SrecData> BytesToSrecDataYeild(byte[] data, int datalength, uint startPosition)
        {
            IEnumerable<SrecData> newData = new List<SrecData>();
            if (data.Length < datalength)
            {
                SrecData srecData = new SrecData(RecordType, data.Length, startPosition, data, EightOrsixteen);
                yield return (srecData);
            }
            else
            {
                int count = data.Length / datalength + ((data.Length % datalength > 0) ? 1 : 0);
                int 余数 = data.Length % datalength == 0 ? datalength : data.Length % datalength;
                uint position = startPosition;
                for (int i = 0; i < count; i++)
                {
                    int length = datalength;

                    if (i + 1 == count)//最后一个
                    {
                        length = 余数;
                    }
                    byte[] lineData = new byte[length];
                    //string dataStr = type + (length + addrLength + 1).HexToString() + position.HexToString(addrLength * 2);
                    for (int j = 0; j < length; j++)
                    {
                        lineData[j] = data[j + i * datalength];
                    }
                    //SrecData srecData = new SrecData(dataStr, false);
                    SrecData srecData = new SrecData(RecordType, length, position, lineData, EightOrsixteen);
                    position += (uint)(length / EightOrsixteen);
                    yield return srecData;
                }
            }

            yield return null;
        }

        /// <summary>
        /// 根据地址填充数据
        /// </summary>
        /// <param name="srecDatas">原数据</param>
        /// <param name="datalength">数据长度（从地址到CRC的数据长度），一行需要填充多少数据</param>
        /// <param name="fillData">填充的数据</param>
        /// <param name="fillLastLine">是否填充最后一行</param>
        /// <returns></returns>
        private byte[] FillData(List<SrecData> srecDatas, int datalength, byte fillData = 0xFF, bool fillLastLine = true)
        {
            List<byte> srecDataBytes = new List<byte>();

            srecDatas.Sort();
            //fill FF
            try
            {
                int bytecount = 0;
                for (int i = 0; i < srecDatas.Count; i++)
                {
                    bytecount += srecDatas[i].DataLength_OnlyData;
                    srecDataBytes.AddRange(srecDatas[i].ToByte());
                    if (i + 1 == srecDatas.Count)
                    {
                        if (fillLastLine)//补齐最后一行
                            if (srecDatas[i].DataLength_OnlyData < datalength)
                            {
                                int count = srecDataBytes.Count % datalength;
                                for (int j = 0; j < (datalength - count); j++)
                                {
                                    srecDataBytes.Add(fillData);
                                }
                            }
                        break;
                    }
                    int intervael = srecDatas[i + 1].Addr - srecDatas[i].Addr - srecDatas[i].DataLength_OnlyData / EightOrsixteen;
                    if (intervael > 0)
                    {
                        for (int j = 0; j < intervael * EightOrsixteen; j++)
                        {
                            srecDataBytes.Add(fillData);
                        }
                        //Console.WriteLine($"add {intervael * eightOrsixteen} 0xff");
                    }
                    //Console.WriteLine($"Current byte Count:{srecDataBytes.Count}(0x{srecDataBytes.Count:X})");
                }

                int 余数 = srecDataBytes.Count % 16;

                if (余数 > 0)
                {
                    //Console.WriteLine($"totalCount：{bytecount}。Fill count:{srecDataBytes.Count}.16位对齐，add {16 - 余数} 0xff"); 
                    for (int i = 0; i < 16 - 余数; i++)
                    {
                        srecDataBytes.Add(fillData);
                    }
                }


            }
            catch (OutOfMemoryException ex)
            {
                Console.WriteLine($"地址间隔太大，无法填充数据:{ex.Message}");
                //throw;
            }

            return srecDataBytes.ToArray();
        }
        /// <summary>
        /// 16位数据转8位数据，修改每行的地址间隔
        /// </summary>
        /// <param name="targetType"></param>
        /// <exception cref="Exception"></exception>
        public void Change(int targetType)
        {
            var oldaddrData = this.AddrData;
            //if (oldaddrData.Keys.Count != 1)
            //	throw new Exception("无法转换，数据不是连续的");
            this.EightOrsixteen = targetType;
            List<SrecData> allNewData = new List<SrecData>();
            foreach (var item in oldaddrData)
            {
                var newAddrData = BytesToSrecData(item.Value.ToArray(), Datalength, item.Key);
                allNewData.AddRange(newAddrData);
            }
            //this.Datalength = datalength;
            this.srecDatas.Clear();
            this.srecDatas.AddRange(allNewData);
        }

        /// <summary>
        /// 整个文件的CRC 校验值
        /// </summary>
        /// <param name="length">文件长度</param>
        /// <param name="fillData">空白地址填充的数据</param>
        /// <returns></returns>
        public uint GetCRC(out int length, byte fillData = 0xff)
        {
            var bytes = FillData(srecDatas, Datalength, fillData, false);

            length = bytes.Length;

            uint crc32 = CRC32Cls.GetCRC32(bytes.ToArray());

            return crc32;
        }

        readonly byte[] AESKey = {
            0x2b, 0x7e, 0x15, 0x16, 0x28, 0xae, 0xd2, 0xa6,
            0xab, 0xf7, 0x15, 0x88, 0x09, 0xcf, 0x4f, 0x3c };

        public byte[] GetCmac(out int length, byte[] key, byte fillData = 0xff)
        {
            var bytes = FillData(srecDatas, Datalength, fillData, false);
            length = bytes.Length;
            return AES128Cmac.AES_CMAC(key, bytes, bytes.Length);
        }
        public byte[] GetCmac(out int length, byte fillData = 0xff)
        {
            var bytes = FillData(srecDatas, Datalength, fillData, false);
            length = bytes.Length;
            return AES128Cmac.AES_CMAC(AESKey, bytes, bytes.Length);
        }

        public void Srec2Hex()
        {
            List<HexLine> hexlines = new List<HexLine>();
            foreach (var srecLine in Content)
            {
                HexLine hexline = new HexLine(srecLine);
                hexlines.Add(hexline);

            }
        }

    }

    public class SrecFileOnlyData
    {
        private readonly ObservableCollection<SrecDataOnly> _content = new ObservableCollection<SrecDataOnly>();
        private SrecFile _srecFile;
        public ObservableCollection<SrecDataOnly> Content { get => _content; }

        public SrecFile SrecFile { get => _srecFile; set => _srecFile = value; }

        public SrecFileOnlyData()
        {

        }

        public SrecFileOnlyData(string filePath)
        {
            ParseWithPath(filePath);
        }
        public int TakeLength = 4;
        private void ParseWithPath(string filePath)
        {
            _srecFile = new SrecFile(filePath);

            foreach (var item in _srecFile.AddrData)
            {
                var startPostion = item.Key;
                int takeLength = TakeLength;

                int dataOnlyCount = item.Value.Count / takeLength;
                int remainder = item.Value.Count % takeLength;
                dataOnlyCount = remainder == 0 ? dataOnlyCount : dataOnlyCount + 1;

                for (int i = 0; i < dataOnlyCount; i++)
                {
                    if ((i + 1) * takeLength > item.Value.Count)
                    {
                        if (remainder != 0)
                        {
                            takeLength = remainder;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    SrecDataOnly dataonly = new SrecDataOnly
                    {
                        Address = (uint)(startPostion + i * takeLength),
                        Data = new List<byte>(new byte[takeLength])
                    };
                    for (int j = 0; j < takeLength; j++)
                    {
                        dataonly.Data[j] = item.Value[i * takeLength + j];
                    }
                    _content.Add(dataonly);

                }
            }
        }

        public void AddData(uint address, byte[] data)
        {
            int count = data.Length / TakeLength;
            for (int i = 0; i < count; i++)
            {
                SrecDataOnly srecdata = null;
                if (Content != null)
                {
                    //find close Content by address
                    srecdata = Content.FirstOrDefault(x => x.Address == address + (uint)(i * TakeLength));
                }
                if (srecdata == null)
                {
                    srecdata = new SrecDataOnly();
                    Content.Add(srecdata);
                }

                srecdata.Address = address + (uint)(i * TakeLength);
                srecdata.UpdateData(data.Skip(i * TakeLength).Take(TakeLength).ToArray(), 0);
            }
        }

        public void InsertData(byte[] byteArray, uint startAddr = 0x00)
        {
            for (int i = 0; i < byteArray.Length; i += 4)
            {
                if (i + 3 < byteArray.Length) // 确保不会越界
                {
                    uint value = byteArray.ToUInt32Endian(i, true);
                    //string address = $"0x{i:X}";
                    _content.Add(new SrecDataOnly 
                    {
                        Address = (uint)i + startAddr,
                        DataStr = $"0x{value:X8}",
                        Data = new List<byte>()
                        {
                            byteArray[i],
                            byteArray[i + 1],
                            byteArray[i + 2],
                            byteArray[i + 3],
                        },
                    });
                }
            }
        }
    }

     public static class BitConverterExtensions
    {
        // 扩展方法，支持根据指定的大小端转换为UInt32
        public static uint ToUInt32Endian(this byte[] bytes, int startIndex, bool isBigEndian = false)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            if (bytes.Length < startIndex + 4)
                throw new ArgumentException("Byte array is too small to convert to UInt32.", nameof(bytes));

            byte[] byteArray = new byte[4];

            // 将字节复制到新的数组中
            Array.Copy(bytes, startIndex, byteArray, 0, 4);

            // 如果是大端，反转字节数组
            if (isBigEndian)
            {
                Array.Reverse(byteArray);
            }

            return BitConverter.ToUInt32(byteArray, 0);
        }
    }

    public class SrecDataOnly :CommunityToolkit.Mvvm.ComponentModel.ObservableObject
    {
        private uint address;
        private string _value;

        public uint Address { get => address; set => SetProperty(ref address, value); }

        public List<byte> Data { get; set; }

        public string DataStr
        {
            get
            {
                if (!string.IsNullOrEmpty(_value))
                    return _value;

                if (Data == null || Data.Count == 0)
                    return "";
                return string.Join("", Data.Select(d => d.ToString("X2")));
            }
            set => _value = value;
        }

        public void UpdateData(byte data, int index)
        {
            if (Data == null) Data = new List<byte>();
            if (Data.Count <= index)
                Data.Add(data);
            else
                Data[index] = data;
            OnPropertyChanged(nameof(DataStr));
        }

        public void UpdateData(byte[] data, int index)
        {
            for (int i = 0; i < data.Length; i++)
            {
                UpdateData(data[i], index + i);
            }
        }
    }
}
