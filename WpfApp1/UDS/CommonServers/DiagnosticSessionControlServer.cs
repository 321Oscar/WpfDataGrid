using WpfApp1.Devices;
using WpfApp1.Services;

namespace WpfApp1.UDS
{
    /// <summary>
    /// 诊断服务：0x10
    /// </summary>
    public class DiagnosticSessionControlServer : UDSServerBase
    {
        /// <summary>
        /// 0x10
        /// </summary>
        /// <param name="slaver"></param>
        /// <param name="master"></param>
        public DiagnosticSessionControlServer(uint slaver, uint master, IDevice device, ILogService logService) 
            : base(slaver, master, device, logService)
        {
            this.ServerName = "DiagnosticSession";
        }

        public override UDSServerCode CurrentUDSFunction { get; protected set; } = UDSServerCode.DiagnosticSessionControl;

        public override byte[] BuildFrame()
        {
            return base.BuildFrame();
        }

        public override ServerResult Run(object param = null)
        {
           // CurrentTimeout = timeout;
            return base.Run(param);
        }

        public override void ParseData(byte[] data)
        {
            base.ParseData(data);
        }
    }
}
