using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ERad5TestGUI.Devices;
using ERad5TestGUI.Models;

namespace ERad5TestGUI.Stores
{
    public class DBCSignalBuildHelper
    {
        ///// <summary>
        ///// CAN 数据，有8列（1个byte 8个bit）
        ///// </summary>
        //const int BitColumn = 8;
        ///// <summary>
        ///// Can 2.0 有8行 bits
        ///// </summary>
        //const int BitRows_CanV2 = 8;
        ///// <summary>
        ///// Can FD 有64行 bits
        ///// </summary>
        //const int BitRows_CanFD = 64;
        ///// <summary>
        ///// 一个CAN ID 有多少个byte数据
        ///// </summary>
        //private readonly int bitRows = 8;
        private readonly IEnumerable<SignalBase> signals;
        public List<CANMessage> Messages { get; }
        public DBCSignalBuildHelper(IEnumerable<SignalBase> signals, List<Message> messages)
        {
            Messages = new List<CANMessage>();
            foreach (var msg in messages)
            {
                CANMessage canMessage = new CANMessage(msg.MessageID, msg.messageSize, msg.isExternID ? 1 : 0);
                Messages.Add(canMessage);
            }
            this.signals = signals;
        }

        public IEnumerable<IFrame> BuildFrames()
        {
            List<IFrame> cAN_Msg_BytesList = new List<IFrame>();

            var ids = signals.Select(x =>x.MessageID).Distinct();

            foreach (var signal in signals)
            {
               ModifyBytesRef(signal);
            }

            foreach (var messageid in ids)
            {
                // var bitsdata =
                var message = Messages.FirstOrDefault(x => x.MessageID == messageid);
                if (message != null)
                {
                    byte[] resData = message.Data;//8 帧数据
                    cAN_Msg_BytesList.Add(new CanFrame(messageid, resData));
                }
            }

            return cAN_Msg_BytesList;
        }

        private void ModifyBytesRef(SignalBase signal)
        {
            var message = Messages.FirstOrDefault(x => x.MessageID == signal.MessageID);
            if (message != null)
            {
                byte[] initdata = message.Data;

                if (signal.StartBit / 8 > initdata.Length)
                    throw new ArgumentOutOfRangeException("信号Startbit错误，检查是否CANFD！");

                ReConstructByteArray(signal, ref initdata);
                message.Data = initdata;
            }
        }

        public void ReConstructByteArray(SignalBase signal, ref byte[] msg)
        {
            int startBit = signal.StartBit;

            int startIndex = startBit / 8;
            int startOffset = startBit % 8;

            int rawvalue = (int)(((decimal)(signal.OriginValue) - (decimal)signal.Offset) / (decimal)signal.Factor);

            if (signal.ByteOrder == 0)//moto
            {
                for (int i = 0; i < signal.Length; i++)
                {
                    int byteIndex = startIndex + (7 - startOffset + i) / 8;
                    int bitIndex = (GetMotorolaBitIndex(startOffset - i) % 8);
                    int clearmask = ~(1 << bitIndex);
                    msg[byteIndex] = (byte)(msg[byteIndex] & clearmask);
                    int bitvaule = (rawvalue >> (signal.Length - i - 1)) & 1;//(msg[byteIndex] >> bitIndex) & 1;
                    msg[byteIndex] = (byte)(msg[byteIndex] | (bitvaule << bitIndex));
                }
            }
            else
            {
                for (int i = 0; i < signal.Length; i++)
                {
                    int byteIndex = startIndex + (startOffset + i) / 8;
                    int bitIndex = (startOffset + i) % 8;
                    int clearmask = ~(1 << bitIndex);
                    msg[byteIndex] = (byte)(msg[byteIndex] & clearmask);
                    if ((rawvalue & (1 << i)) != 0)
                    {
                        msg[byteIndex] |= (byte)(1 << bitIndex);
                    }
                }
            }
        }

        private int GetMotorolaBitIndex(int index)
        {
            while (index < 0)
            {
                index += 8;
            }
            return index;
        }

    
    }

    public class CANMessage
    {
        public CANMessage(uint messageID, uint messageSize, int frameType)
        {
            MessageID = messageID;
            MessageSize = messageSize;

            Data = new byte[messageSize];
            FrameType = frameType;
        }

        public uint MessageID { get; }
        public uint MessageSize { get; }
        public int FrameType { get; }
        public byte[] Data { get; set; }
    }

    public class DbcFile
    {
        public List<string> Nodes = new List<string>();
        public List<Message> Messages = new List<Message>();
        public List<Comment> Comments = new List<Comment>();

        public List<ValEnum> ValEnums { get; internal set; } = new List<ValEnum>();

        public string Path { get; }

        public DbcFile(string path)
        {
            Path = path;
            StrToDbeFile();
        }

        private int StrToDbeFile()
        {
            string fileBuffer;
            using (FileStream fs = new FileStream(Path, FileMode.Open, FileAccess.Read))
            using (StreamReader sr = new StreamReader(fs, System.Text.Encoding.Default))
            {
                fileBuffer = sr.ReadToEnd();
            }

            int err = 0;
            string[] bufferAry = null;

            // dbcFile = new DbcFile();
            if (fileBuffer == null)
            {
                if (fileBuffer == "")
                {
                    throw new Exception("Dbc文件为空");
                }
            }
            /**
             BU_: ABS ACU BCM BMS CCU DCDC EPS GW ICU MCU MMI PTC TPMS VCU IVI OBC
             BO_ 1707 RMCU_0x6AB: 8 MCU
                SG_ DBC_MCU_idc : 48|16@1+ (0.1,0) [0|6553.5] "" Vector__XXX
                SG_ DBC_MCU_F16_resver1 : 32|16@1+ (0.1,-1000) [-1000|5553.5] "" Vector__XXX
                SG_ DBC_MCU_Tcalc : 16|16@1+ (0.1,-1000) [-1000|5553.5] "" Vector__XXX
                SG_ DBC_MCU_Pinv : 0|16@1+ (0.1,-1000) [-1000|5553.5] "" Vector__XXX
            **/
            bufferAry = fileBuffer.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (bufferAry.Length < 3)
            {
                throw new Exception("Dbc文件格式有误");
            }

            int lineNum = bufferAry.Length;
            bool isMessageValid = false;
            int i = 0;
            try
            {
                for (i = 0; i < lineNum; i++)
                {
                    string[] lineAry = bufferAry[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (lineAry.Length < 1)
                    {
                        continue;
                    }
                    //每行的开头
                    switch (lineAry[0])
                    {
                        case "VAL_":
                            if (lineAry.Length < 5)
                                break;
                            ValEnum valEnum = new ValEnum();
                            valEnum.messageID = Convert.ToUInt32(lineAry[1]);
                            valEnum.signalName = lineAry[2];

                            for (int j = 3; j + 1 < lineAry.Length; j += 2)
                            {
                                if (int.TryParse(lineAry[j], out int val) && !valEnum.values.ContainsKey(val))
                                    //int val = Convert.ToInt32();
                                    valEnum.values.Add(val, lineAry[j + 1].Replace("\"", ""));
                            }

                            ValEnums.Add(valEnum);
                            break;
                        case "CM_":
                            Comment cmt = new Comment();
                            //lineAry = bufferAry[i].Split(new char[] { ' ', '"', '\\' }, StringSplitOptions.RemoveEmptyEntries);
                            //if (lineAry.Length == 6)
                            //{
                            //    cmt.messageID = lineAry[2];
                            //    cmt.signalName = lineAry[3];
                            //    cmt.comment = lineAry[4];
                            //}
                            //else
                            //{
                            //    cmt.comment = bufferAry[i];
                            //}
                            string pattern = @"(?<=\s|^)([^""\s]+)(?=\s|$)|""([^""]+)""";
                            MatchCollection matches = Regex.Matches(bufferAry[i], pattern);
                            List<string> tmp = new List<string>();
                            // 输出结果
                            foreach (Match match in matches)
                            {
                                // 使用 Group 1 或 Group 2 来获取匹配的内容
                                string result = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
                                tmp.Add(result);
                            }
                            if (tmp.Count == 5)
                            {
                                cmt.MessageID = tmp[2];
                                cmt.signalName = tmp[3];
                                cmt.CommentValue = tmp[4];
                            }
                            else
                            {
                                cmt.CommentValue = bufferAry[i];
                            }
                            //get messge 
                            var msg = Messages.FirstOrDefault(x => x.MessageID == cmt.MsgIDUint);
                            if (Messages.FirstOrDefault(x => x.MessageID == cmt.MsgIDUint) != null)
                            {
                                var signal = msg.signals.FirstOrDefault(x => x.SignalName == cmt.signalName);
                                if (signal != null)
                                {
                                    signal.Comment = cmt;
                                }
                            }
                            Comments.Add(cmt);
                            break;
                        case "BU_":
                            for (int j = 1; j < lineAry.Length; j++)
                            {
                                Nodes.Add(lineAry[j]);
                            }
                            break;
                        case "BO_":

                            Message message = new Message();
                            uint id = Convert.ToUInt32(lineAry[1]);
                            //跳过默认信息
                            if (id == 0xC0000000)
                            {
                                isMessageValid = false;
                                break;
                            }
                            else
                            {
                                isMessageValid = true;
                            }
                            //最高位为1的为扩展帧
                            if ((id & 0x80000000) != 0)
                            {
                                id &= 0x7Fffffff;
                                message.isExternID = true;
                            }
                            else
                            {
                                message.isExternID = false;
                            }
                            message.MessageID = id;
                            message.messageName = lineAry[2].Substring(0, lineAry[2].Length - 1);
                            message.messageSize = Convert.ToUInt32(lineAry[3]);
                            message.transmitter = lineAry[4];

                            Messages.Add(message);
                            break;
                        case "SG_":
                            if (isMessageValid)
                            {
                                uint byteOffset = 0;
                                Signal signal = new Signal();
                                signal.SignalName = lineAry[1];

                                if (lineAry[2] == ":")
                                {
                                    signal.multiplexerIndicator = -2;
                                    byteOffset = 0;
                                }
                                else
                                {
                                    byteOffset = 1;
                                    if (lineAry[2][0] == 'M')
                                    {
                                        signal.multiplexerIndicator = -1;
                                    }
                                    else if (lineAry[2][0] == 'm')
                                    {
                                        signal.multiplexerIndicator = Convert.ToInt32(lineAry[2].Substring(1, lineAry[2].Length - 1));
                                    }
                                    else
                                    {
                                        throw new Exception("Dbc信号格式错误");
                                    }
                                }

                                string[] sp = lineAry[3 + byteOffset].Split(new char[] { '|', '@' }, StringSplitOptions.RemoveEmptyEntries);

                                signal.startBit = Convert.ToUInt32(sp[0]);
                                signal.signalSize = Convert.ToUInt32(sp[1]);

                                if (sp[2][0] == '0')
                                {
                                    signal.byteOrder = 0;
                                }
                                else if (sp[2][0] == '1')
                                {
                                    signal.byteOrder = 1;
                                }

                                if (sp[2][1] == '+')
                                {
                                    signal.valueType = 0;
                                }
                                else if (sp[2][1] == '-')
                                {
                                    signal.valueType = 1;
                                }

                                string[] sp1 = lineAry[4 + byteOffset].Split(new char[] { '(', ',', ')' }, StringSplitOptions.RemoveEmptyEntries);
                                signal.factor = Convert.ToDouble(sp1[0]);
                                signal.offset = Convert.ToDouble(sp1[1]);

                                sp1 = lineAry[5 + byteOffset].Split(new char[] { '[', '|', ']' }, StringSplitOptions.RemoveEmptyEntries);
                                signal.minimum = Convert.ToDouble(sp1[0]);
                                signal.maximum = Convert.ToDouble(sp1[1]);

                                signal.Unit = lineAry[6 + byteOffset];

                                signal.receivers = lineAry[7 + byteOffset].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                                Messages[Messages.Count - 1].signals.Add(signal);
                            }
                            break;
                        case "BA_":
                            if (lineAry.Length < 3)
                                break;
                            if (lineAry[1].Contains("CycleTime") && lineAry[2] == "BO_")
                            {
                                if (uint.TryParse(lineAry[3], out uint messageID))
                                {
                                    Messages.Find(x => x.CheckID(messageID)).cycleTime = int.Parse(lineAry[4].Replace(';', ' '));
                                }
                            }
                            break;
                    }
                }
            }
            catch (Exception er)
            {
                throw new Exception($"dbc解析错误:Line {i} {bufferAry[i]}", er);
            }
            return err;
        }
    }

    public class Signal
    {
        public string MessageName
        {
            get => messageName;
            set
            {
                messageName = value;
                if (messageName.IndexOf("TX") > -1)
                {
                    InOrOut = true;
                }
            }
        }
        public uint MessageID { get; set; }

        public string SignalName { get; set; }
        /// <summary>
        /// -2:普通信号；-1：复用选择信号；0-N：复用信号
        /// </summary>
        public int multiplexerIndicator = -2;
        /// <summary>
        /// 起始位
        /// </summary>
        public uint startBit = 0;
        /// <summary>
        /// 数据长度
        /// </summary>
        public uint signalSize = 0;
        /// <summary>
        /// 0:Motorola;1:Intel
        /// </summary>
        public uint byteOrder = 0;
        /// <summary>
        /// 0:unsigned;1:signed
        /// </summary>
        public uint valueType = 0;
        public double factor = 0;
        /// <summary>
        /// 偏移量
        /// </summary>
        public double offset = 0;

        public double minimum = 0;
        public double maximum = 0;
        /// <summary>
        /// 单位
        /// </summary>
        public string Unit
        {
            get => unit;
            set => unit = value.Replace("\"", ""); //remove "
        }

        public string[] receivers;
        private string unit;
        private string messageName;
        private Comment comment;

        public override string ToString()
        {
            return $"{MessageName} 0x{MessageID:X} {SignalName}";
        }

        public Comment Comment 
        { 
            get => comment;
            set 
            {
                comment = value; 
                //get pageName
                if(comment.KeyValues.TryGetValue("Page", out string pageName))
                {
                    Page = pageName;
                }
            }
        }
        /// <summary>
        /// true:TX_Msg(In)
        /// </summary>
        public bool InOrOut { get; set; }
        public string Page { get; set; }
    }

    public class ValEnum
    {
        public uint messageID;
        public string signalName;
        public Dictionary<int, string> values = new Dictionary<int, string>();
    }

    public class Message
    {
        public uint MessageID { get; set; } = 0;
        /// <summary>
        /// 是否是扩展帧
        /// </summary>
        public bool isExternID = false;
        public string messageName = "";
        public uint messageSize = 0;
        public string transmitter = "";
        /// <summary>
        /// 周期 ，默认为10ms
        /// </summary>
        public int cycleTime = 10;
        public List<Signal> signals = new List<Signal>();

        public bool CheckID(uint msgid)
        {
            if (isExternID && (msgid != MessageID))
            {
                msgid &= 0x7Fffffff;
            }
            return msgid == MessageID;
        }

        public override string ToString()
        {
            return $"0x{MessageID:X}";
        }
    }

    public class Comment
    {
        public string signalName;
        public string MessageID 
        { 
            get => messageID;
            set
            {
                messageID = value;
                MsgIDUint = uint.Parse(messageID);
            }
        }
        private string commentValue;
        private Dictionary<string, string> _keyValues = new Dictionary<string, string>();
        private string messageID;
        public uint MsgIDUint { get; private set; }
        public string CommentValue
        {
            get => commentValue;
            set
            {
                commentValue = value;
                string[] properties = commentValue.Split(" ");
                if (properties.Length > 0)
                {
                    foreach (var property in properties)
                    {
                        string[] nameValueArray = property.Split(":");
                        if (nameValueArray.Length == 2)
                        {
                            if (!_keyValues.ContainsKey(nameValueArray[0]))
                                _keyValues.Add(nameValueArray[0], nameValueArray[1]);
                        }
                    }
                }
            }
        }

        public Dictionary<string, string> KeyValues { get => _keyValues; }

        public string GetCommentByKey(string key)
        {
            if (_keyValues.TryGetValue(key, out string value)) return value;
            return "";
        }

        public double GetCommenDoubleByKey(string key, double defaultVal)
        {
            if (!_keyValues.TryGetValue(key, out string value) ||
                !double.TryParse(value, out double valueDouble))
                return defaultVal;

            return valueDouble;
        }

        public override string ToString()
        {
            return $"{MessageID} {signalName} {CommentValue}";
        }

    }

    public static class ExtendMethod
    {
        public static string[] Split(this string value, string splitValue, StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries)
        {
            return value.Split(new string[] { splitValue }, options);
        }


        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                collection.Add(item);
            }
        }

    }
}
