using System;
using ERad5TestGUI.Devices;
using ERad5TestGUI.Services;

namespace ERad5TestGUI.UDS
{
    /// <summary>
    ///  MultiBase(MultipServer)
    /// </summary>
    public class ComplexServers : MultiBase<MultipServer>, ERad5TestGUI.Interfaces.ISeedNKey
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
