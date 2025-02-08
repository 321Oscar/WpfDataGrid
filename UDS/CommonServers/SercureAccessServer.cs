using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WpfApp1.Devices;
using WpfApp1.Helpers;
using WpfApp1.Interfaces;
using WpfApp1.Services;

namespace WpfApp1.UDS
{
    public class SercureAccessServer : UDSServerBase, ISeedNKey
    {
        /// <summary>
        /// seed key 子功能码
        /// </summary>
        private readonly Dictionary<byte, byte> seedKeyCodes;
        protected byte[] _seed;

        public virtual byte[] BuildSeedNKey(uint securityLevel)
        {
            if (!string.IsNullOrEmpty(SeedKeyPath))
            {
                UDSHelper.GetKeyBySeedInvoke(SeedKeyPath, securityLevel, _seed, out byte[] key);
                return key;
            }

            return UDSHelper.GetKeyBySeed27(_seed, securityLevel);
        }

        public string SeedKeyPath { get; set; }

        public SercureAccessServer(uint slaver, uint master, IDevice device, ILogService logService) : base(slaver, master, device, logService)
        {
            this.ServerName = "Security";
            //init seed key
            seedKeyCodes = new Dictionary<byte, byte>();
            for (int i = 1; i < 0x43; i += 2)
            {
                seedKeyCodes.Add((byte)i, (byte)(i + 1));
            }

            ModifyErrCode(0x22, "Condition not correct");
            ModifyErrCode(0x24, "request sequence error");
            ModifyErrCode(0x35, "Key Error");
            ModifyErrCode(0x36, "try out Count");
            ModifyErrCode(0x37, "required time delay not expired");
            //ModifyErrCode(0x35, "Key Error");
        }

        //public byte SubFuc { get; set; }

        public override async Task<ServerResult> RunAsync(object param = null)
        {
            await Task.Delay(600);

            return await base.RunAsync(param);
        }

        /// <summary>
        /// 当前UDS 服务
        /// </summary>
        public override UDSServerCode CurrentUDSFunction { get; protected set; } = UDSServerCode.SecurityAccess;

        protected override void ParsePositiveSingleFrame(byte[] receivedata)
        {
            if (seedKeyCodes.ContainsKey(receivedata[2]))//request seed
            {
                ParseData(receivedata);
                

                //重置subfuc
                SubFuncCode = receivedata[2];
                SendDatas = null;
                //return true;
            }
            else if (seedKeyCodes.ContainsValue(receivedata[2]))//key
            {
                Result = UDSResponse.Positive;
                ResultMsg = $"{CurrentStep} {receivedata[3]:X2}";

                CurrentStatue = ServerStatus.Done;
                //return true;
            }
            else
            {
                throw new UDSException($"UnKnow SubFuc {receivedata[2]:X}");
            }
        }

        public override void ParseData(byte[] data)
        {
            if (seedKeyCodes.ContainsKey(SubFuncCode.Value))
            {
                _seed = data.Skip(3).Take(4).ToArray();
                //send the key
                var key = BuildSeedNKey(data[2]);
                SubFuncCode = seedKeyCodes[data[2]];
                SendDatas = key;
                var sendbytes = BuildFrame();
                previousDatas = sendbytes;
                SendFrame = new CanFrame(PhyID_Req, sendbytes, fillData: FillData, extendedFrame: IDExtended);
                CurrentStatue = ServerStatus.WaitReceive;
            }
            else
            {
                base.ParseData(data);
            }
                
        }
    }
}
