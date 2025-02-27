using ERad5TestGUI.Devices;
using ERad5TestGUI.Services;
using System;
using System.Collections.Generic;

namespace ERad5TestGUI.UDS
{
    public interface IUDSEvent
    {
        IDevice Device { get; }
        ILogService Log { get; }

        ///// <summary>
        ///// 发送单帧事件
        ///// </summary>
        //event SendData Send;
        ///// <summary>
        ///// 发送多帧数据事件
        ///// </summary>
        //event SendMultipData SendMultip;

        ///// <summary>
        ///// 接收数据事件
        ///// </summary>
        //event RegisterRecieve RegisterRecieveEvent;
        //event OutputLog DebugLog;
    }

    public interface IMulti<T>: IMultiStatus
    {
        BinTmpFile BinTmpFile { get; set; }
        List<T> Servers { get; set; }
        void Add(T t, decimal weights = 0);
    }

    public interface IMultiStatus
    {
        MultiStatus MultiStatus { get; set; }
    }

    public enum MultiStatus
    {
        Init,
        Runing,
        Stop,
    }
}