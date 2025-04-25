using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Threading.Tasks;
using ERad5TestGUI.Devices;
using ERad5TestGUI.Services;
using System.Threading;

namespace ERad5TestGUI.UDS
{
    public class UDSServerAbstract : ObservableObject, IUDSServer, IUDSEvent
    {
        private int progressInt;
        private string resultMsg;
        private UDSResponse result;
        private bool @checked = true;
        public const string END = "END";
        public const string SKIP = "Skip";
        public string Name { get; set; }
        public int Index { get; set; }
        /// <summary>
        /// 进度条权重，多个Server时使用
        /// </summary>
        public decimal ProgressWeights { get; set; }
        public uint FunctionID { get; set; }
        public uint PhyID_Req { get; set; }
        public uint PhyID_Res { get; set; }
        /// <summary>
        /// 是否是拓展帧
        /// </summary>
        public bool IDExtended { get; set; }
        public byte FillData { get; set; } = 0xAA;
        public int CanChannel { get; set; }
        public int NormalTimeout { get; set; }
        public int PendingTimeout { get; set; }
        public object AfterRunParameter { get; set; }
        /// <summary>
        /// 向上传递进度
        /// </summary>
        public IProgress<ServerResult> Progress { get; set; }
       
        public int ProgressInt
        {
            get => progressInt;
            set
            {
                if (!SetProperty(ref progressInt, value))
                    return;
                Progress?.Report(new ServerResult(Index, ProgressWeights) { Progress = ProgressInt, Message = $"{ResultMsg}"});
            }
        }
        public string ResultMsg { get => resultMsg; set { SetProperty(ref resultMsg, value); } }
        public UDSResponse Result
        {
            get => result; set { SetProperty(ref result, value); }
        }

        public bool Checked { get => @checked; set { SetProperty(ref @checked, value); } }

        public bool IsCanFD { get; set; }

        //public abstract event SendData Send;
        //public abstract event SendMultipData SendMultip;
        //public abstract event RegisterRecieve RegisterRecieveEvent;
        //public abstract event OutputLog DebugLog;
        public IDevice Device { get; }
        public ILogService Log { get; }

       

        public UDSServerAbstract(IDevice device, ILogService log)
        {
            Device = device;
            Log = log;
        }

        public virtual void ResetResult()
        {
            Result = UDSResponse.Init;
            ResultMsg = "";
            ProgressInt = 0;
        }

        public virtual bool Send(IFrame frame)
        {
            if (Device == null)
                return false;
            return Device.SendFD(frame);
        }

        public virtual bool SendMultip(System.Collections.Generic.IEnumerable<IFrame> frames)
        {
            if (Device == null)
                return false;
            return Device.SendFDMultip(frames);
        }

        public virtual Task<ServerResult> RunAsync(CancellationTokenSource cancelSource = null, object param = null)
        {
            throw new NotImplementedException();
        }

        public void ModifyProgressInt(int proInt, string caller)
        {
            //Console.WriteLine($"{caller} 进度更新；{proInt}");
            ProgressInt = proInt;
        }

        public virtual void ChangeChannel(int channel)
        {
            //throw new NotImplementedException();
            this.CanChannel = channel;
        }
    }
}