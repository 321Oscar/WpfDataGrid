using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WpfApp1.Devices;
using WpfApp1.Services;

namespace WpfApp1.UDS
{
    public abstract class MultiBase<T> : UDSServerAbstract, IMulti<T> where T : UDSServerAbstract
    {
        private decimal percentStep;
        private int currentStep;
        private MultiStatus multiStatus;

        //private int canChannel;
        public List<T> Servers { get; set; }
        public BinTmpFile BinTmpFile { get; set; }
        public MultiStatus MultiStatus { get => multiStatus; set => SetProperty(ref multiStatus, value); }

        public int ServerCount
        {
            get
            {
                if (Servers == null)
                    return 0;
                else
                {
                    if (Servers[0] is MultipServer)
                    {
                        int count = 0;
                        foreach (var item in Servers)
                        {
                            count += (item as MultipServer).ServerCount;
                        }
                        return count;
                    }
                    else
                    {
                        return Servers.Count;
                    }
                }

            }
        }

        public MultiBase(int normalTimeout, int pendingTimeout, IDevice device, ILogService logService)
            :base(device,logService)
        {
            NormalTimeout = normalTimeout;
            PendingTimeout = pendingTimeout;
        }

        //public override event SendData Send;
        //public override event SendMultipData SendMultip;
        //public override event RegisterRecieve RegisterRecieveEvent;
        //public override event OutputLog DebugLog;

        //public new int CanChannel 
        //{ 
        //    get => canChannel;
        //    set 
        //    {
        //        canChannel = value;
        //        if(Servers != null)
        //        {
        //            foreach (var serv in Servers)
        //            {
        //                serv.CanChannel = canChannel;
        //            }
        //        }
        //    }
        //}

        public abstract void LoadServers();
        public override async Task<ServerResult> RunAsync(object param = null)
        {
            //return Task.Run(async () =>
            //{
            ServerResult res = null;
            MultiStatus = MultiStatus.Runing;
            //ResultMsg = $"{Name} {GlobalVar.START} {(BinTmpFile != null ? BinTmpFile.FilePath:"")}.";
            //ModifyProgressInt(1, $"{Name} {GlobalVar.START}");
            for (int i = 0; i < Servers.Count; i++)
            {
                //ModifyProgressInt((int)((double)(i) / Servers.Count * 100), $"{Name},{Servers[i].Name} {GlobalVar.START}");
                currentStep = (int)(i * 1.00 / Servers.Count * 100);
                if (!Servers[i].Checked)
                {
                    ResultMsg = $"{Name}-{(i + 1)} {SKIP}.";
                    ModifyProgressInt((int)((double)(i + 1) / Servers.Count * 100), $"{Name},{Servers[i].Name} {SKIP}");
                    continue;
                }
                object previousParam;
                if (i == 0)
                {
                    previousParam = param;
                }
                else
                {
                    previousParam = Servers[i - 1].AfterRunParameter;
                }
                res = await Servers[i].RunAsync(previousParam);
                Result = res.UDSResponse;
                if (res.UDSResponse != UDSResponse.Positive)
                {
                    if (res.UDSResponse == UDSResponse.Pass)
                        continue;
                    break;
                }
                //ResultMsg = $"{Name}-{(i + 1)} {GlobalVar.END}.";
            }
            MultiStatus = MultiStatus.Stop;
            return res;
            //});

        }
        public override void ResetResult()
        {
            base.ResetResult();
            currentStep = -1;
            if (Servers != null && Servers.Count > 0)
            {
                foreach (var server in Servers)
                {
                    server.ResetResult();
                }
            }
        }

        public override void ChangeChannel(int channel)
        {
            base.ChangeChannel(channel);
            if (Servers != null)
            {
                foreach (var serv in Servers)
                {
                    serv.ChangeChannel(channel);
                }
            }
        }

        void CheckValid()
        {

        }

        public void Add(T t, decimal weights = 1)
        {
            //t.FunctionID = FunctionID;
            //t.PhyID_Req = PhyID_Req;
            //t.PhyID_Res = PhyID_Res;
            t.NormalTimeout = NormalTimeout;
            t.PendingTimeout = PendingTimeout;
            //t.Send += Send;
            //t.SendMultip += SendMultip;
            //t.RegisterRecieveEvent += RegisterRecieveEvent;
            //t.DebugLog += DebugLog;
            t.CanChannel = CanChannel;
            t.Progress = new Progress<ServerResult>(value => ReportProgress(value));
            t.Index = Servers == null ? 0 : Servers.Count;
            t.IsCanFD = IsCanFD;
            t.IDExtended = IDExtended;
            t.FillData = FillData;
            //cal weights;

            t.ProgressWeights = weights;
            Servers.Add(t);
            percentStep = 1.0m / Servers.Count;

            if(this is WpfApp1.Interfaces.ISeedNKey mk && t is WpfApp1.Interfaces.ISeedNKey tk)
            {
                tk.SeedKeyPath = mk.SeedKeyPath;
            }
        }
        /// <summary>
        /// 计算该权重占所有服务的占比
        /// </summary>
        /// <param name="weights"></param>
        /// <returns></returns>
        public decimal GetWeights(decimal weights)
        {
            if (this.Servers == null || Servers.Count == 0) return 1;

            decimal totalWeights = 0;

            foreach (var item in Servers)
            {
                totalWeights += item.ProgressWeights;
            }

            return weights / totalWeights;
        }
        /// <summary>
        /// 计算完成i个服务的进度
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public decimal GetDoneProgress(int index)
        {
            if (this.Servers == null || Servers.Count == 0) return 1;

            decimal totalWeights = 0;
            decimal doneWeights = 0;
            for (int i = 0; i < Servers.Count; i++)
            {
                totalWeights += Servers[i].ProgressWeights;
                if (index > i)
                    doneWeights += Servers[i].ProgressWeights;
            }

            return (doneWeights / totalWeights);
        }

        public void ClearServer()
        {
            if (Servers != null && Servers.Count > 0)
            {
                Servers.Clear();
            }
        }

        private void ReportProgress(ServerResult value)
        {
            if (value.Progress < 0)
            {
                ModifyProgressInt(-1, $"{Name} Wait");
            }
            else
            {
                if (currentStep < 0)
                {
                    ModifyProgressInt(0, $"{Name} {value.Index} Reset.");
                    return;
                }

                int p;
                decimal stepWeights = (value.ProgressWeights == 0) ? percentStep : GetWeights(value.ProgressWeights);
                if (value.Index + 1 == Servers.Count && value.Progress == 100)
                {
                    p = 100;
                }
                else
                {
                    //p = (int)(value.Index * 1.00 / Servers.Count * 100) + (int)(value.Progress * stepWeights);
                    decimal doneP = GetDoneProgress(value.Index);
                    p = (int)(doneP * 100) + (int)(value.Progress * stepWeights);
                }
                string caller = $"{Name} Running,{value.Message},{value.Index}/{Servers.Count}+ {value.Progress}*{stepWeights:f2}";

                ModifyProgressInt(p, caller);

                Progress?.Report(new ServerResult(Index, ProgressWeights) { Progress = p, Message = value.Message, UDSResponse = value.UDSResponse });
            }
        }
    }

    public class MultiServers<T> : MultiBase<T> where T : UDSServerAbstract
    {
        public MultiServers(int normalTimeout, int pendingTimeout, IDevice device, ILogService logService) 
            : base(normalTimeout, pendingTimeout, device, logService)
        {
        }

        public override void LoadServers()
        {
            //throw new NotImplementedException();
        }
    }
}