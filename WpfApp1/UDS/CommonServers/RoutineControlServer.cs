using System.Collections.Generic;
using System.Linq;
using ERad5TestGUI.Devices;
using ERad5TestGUI.Services;

namespace ERad5TestGUI.UDS
{
    /// <summary>
    /// 0x31
    /// </summary>
    public class RoutineControlServer : UDSServerBase
    {
        public List<byte> PositiveCodes { get; protected set; }

        public override UDSServerCode CurrentUDSFunction { get; protected set; } = UDSServerCode.RountineControl;

        /// <summary>
        /// 0x31 服务
        /// </summary>
        /// <param name="slaver"></param>
        /// <param name="master"></param>
        public RoutineControlServer(uint slaver, uint master, IDevice device, ILogService logService) : base(slaver, master, device, logService)
        {
            ServerName = "RoutineControl";
            PositiveCodes = new List<byte>()
            {
                0x04,0xAA
            };
        }

        public void ModifyPositiveCode(byte newCode)
        {
            PositiveCodes = new List<byte>()
            {
                newCode
            };
        }

        public override ServerResult Run(object param = null)
        {
            return base.Run(param);
        }

        protected override void ParsePositiveSingleFrame(byte[] receivedata)
        {
            //if (receivedata[5] == 0x04)
            //{
            //    base.ParsePositiveSingleFrame(receivedata);
            //}
            //else if (receivedata[5] == 0xAA)
            //{
            //    base.ParsePositiveSingleFrame(receivedata);
            //}
            if (PositiveCodes.Contains(receivedata[5]))
            {
                base.ParsePositiveSingleFrame(receivedata);
            }
            else
            {
                Result = UDSResponse.Negative;
                ResultMsg = $"{CurrentStep} {string.Join(" ", receivedata.Select(x => x.ToString("X2")))}";
                if (ErrCode.ContainsKey(receivedata[5]))
                {
                    ResultMsg += $" {ErrCode[receivedata[5]]}";
                }
                CurrentStatue = ServerStatus.Done;
            }
        }

        public override byte[] BuildFrame()
        {
            return base.BuildFrame();
        }

        public override void ParseData(byte[] data)
        {
            //throw new NotImplementedException();
            base.ParseData(data);
        }
    }
}
