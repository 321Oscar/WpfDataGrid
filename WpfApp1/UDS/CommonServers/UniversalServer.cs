using System;
using System.Linq;
using System.Threading.Tasks;
using ERad5TestGUI.Devices;
using ERad5TestGUI.Services;

namespace ERad5TestGUI.UDS
{
    /// <summary>
    /// 通用服务：发送 单帧+serverid+ subfuncid。仅接收一帧，判断正负响应
    /// </summary>
    public class UniversalServer : UDSServerBase
    {
        public UniversalServer(uint slaver, uint master, IDevice device, ILogService logService) : base(slaver, master, device, logService)
        {
            //ServerName = "Common Server";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="slaver"></param>
        /// <param name="master"></param>
        /// <param name="serverName"></param>
        /// <param name="udsFunc">发送命令：【10:03,01】</param>
        public UniversalServer(uint slaver, uint master, string udsFunc, IDevice device, ILogService logService) : this(slaver, master, device, logService)
        {
            string[] val = udsFunc.Split(':');
            CurrentUDSFunction = (UDSServerCode)Convert.ToByte(val[0], 16);
            ServerName = $"{CurrentUDSFunction}";
            var subfuncs = val[1].Split(',').Select(x => Convert.ToByte(x, 16)).ToArray();
            SubFuncCode = subfuncs[0];
            if (subfuncs.Length > 1)
            {
                SendDatas = subfuncs.Skip(1).ToArray();
            }
        }

        public UniversalServer(uint slaver, uint master,string serverName, UDSServerCode udsFunc, IDevice device, ILogService logService) : this(slaver, master, device, logService)
        {
            ServerName = $"{serverName}";
            CurrentUDSFunction = udsFunc;
        }

        public override UDSServerCode CurrentUDSFunction  { get; protected set; }


        public override void ParseData(byte[] data)
        {
            //throw new NotImplementedException();
            base.ParseData(data);
        }
    }

    public sealed class AwaitServer : UDSServerBase
    {
        public int DelayTime { get; set; }
        public AwaitServer(uint slaver, uint master, IDevice device, ILogService logService) 
            : base(slaver, master, device, logService)
        {
            ServerName = "Delay";
        }

        public override UDSServerCode CurrentUDSFunction { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }

        public override Task<ServerResult> RunAsync(System.Threading.CancellationTokenSource cancelSource = null, object param = null)
        {
            ServerResult = new ServerResult(Index, ProgressWeights);
            return Task.Run(async () =>
            {
                await Task.Delay(DelayTime);
                Result = UDSResponse.Positive;
                ProgressInt = 100;
                return ServerResult;
            }, cancelSource.Token);
        }
    }
}
