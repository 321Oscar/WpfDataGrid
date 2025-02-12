using System.Collections.Generic;
using WpfApp1.Devices;
using WpfApp1.Services;

namespace WpfApp1.UDS
{
    /// <summary>
    /// 复合服务：一次执行多个服务，通过Add 增加，按照添加顺序执行
    /// </summary>
    public class MultipServer : MultiBase<UDSServerBase>, WpfApp1.Interfaces.ISeedNKey
    {
        public MultipServer(int normalTimeout, int pendingTimeout , IDevice device, ILogService logService) 
            : base(normalTimeout, pendingTimeout, device, logService)
        {
            Servers = new List<UDSServerBase>();
        }
        public string SeedKeyPath { get; set; }
        /// <summary>
        /// 初始化服务
        /// </summary>
        public override void LoadServers() { }
        
    }
}
