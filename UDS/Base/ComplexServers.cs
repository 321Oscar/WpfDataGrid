using System;
using WpfApp1.Devices;
using WpfApp1.Services;

namespace WpfApp1.UDS
{
    /// <summary>
    ///  MultiBase(MultipServer)
    /// </summary>
    public class ComplexServers : MultiBase<MultipServer>, WpfApp1.Interfaces.ISeedNKey
    {
        /// <summary>
        ///  MultiBase(MultipServer)
        /// </summary>
        /// <param name="normalTimeout"></param>
        /// <param name="pendingTimeout"></param>
        public ComplexServers(int normalTimeout, int pendingTimeout, IDevice device, ILogService logService) 
            : base(normalTimeout, pendingTimeout, device, logService)
        {
        }

        public string SeedKeyPath { get; set; }

        public override void LoadServers()
        {
            throw new NotImplementedException();
        }
    }

}
