using System;
using System.Threading.Tasks;

namespace ERad5TestGUI.UDS
{
    /// <summary>
    /// UDS 服务中通用的属性&事件
    /// </summary>
    public interface IUDSServer
    {
        /// <summary>
        /// 功能寻址：发送id
        /// </summary>
        uint FunctionID { get; set; }
        /// <summary>
        /// 物理寻址：发送id
        /// </summary>
        uint PhyID_Req { get; set; }
        /// <summary>
        /// 物理寻址：接收id
        /// </summary>
        uint PhyID_Res { get; set; }
        /// <summary>
        /// 收发的can通道
        /// </summary>
        int CanChannel { get; set; }
        void ChangeChannel(int channel);
        /// <summary>
        /// 正常数据帧接收超时时间
        /// </summary>
        int NormalTimeout { get; set; }
        /// <summary>
        /// Pending超时时间
        /// </summary>
        int PendingTimeout { get; set; }
        /// <summary>
        /// 进度报告
        /// </summary>
        IProgress<ServerResult> Progress { get; set; }
        int ProgressInt { get; set; }
        string ResultMsg { get; set; }
        UDSResponse Result { get; set; }
        bool Checked { get; set; }
        string Name { get; set; }

        void ResetResult();
        Task<ServerResult> RunAsync(object param = null);

        object AfterRunParameter { get; set; }
    }
}