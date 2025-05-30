﻿using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace ERad5TestGUI.Models
{
    public class PulseInSignalGroup : SignalGroupBase
    {
        //public string SignalName { get; set; }

        public PulseInSignalGroup(string signalName) : base(signalName)
        {
            //SignalName = signalName;
        }

        public PulseInSignal Signal_DC { get; set; }

        public PulseInSignal Signal_Freq { get; set; }
    }
    /// <summary>
    /// has List of PulseInSignalGroup
    /// </summary>
    public class PulseInGroupGroup : SignalGroupBase, Interfaces.IClearData
    {
        public System.Collections.Generic.List<PulseInSignalGroup> Groups { get; }
        public PulseInGroupGroup(string groupName) : base(groupName)
        {
            Groups = new System.Collections.Generic.List<PulseInSignalGroup>();
        }

        public void ClearData()
        {
            foreach (var g in Groups)
            {
                g.Signal_Freq.Clear();
                g.Signal_DC.Clear();
            }
        }
    }

    public class PulseOutGroupList : SignalGroupBase
    {
        public PulseOutGroupList(string groupName) : base(groupName)
        {
            Groups = new List<PulseOutGroupSignalGroup>();
            SingleSignals = new List<SignalView>();
        }
        /// <summary>
        /// duty and cycle Group
        /// </summary>
        public System.Collections.Generic.List<PulseOutGroupSignalGroup> Groups { get; }
        public System.Collections.Generic.List<SignalView> SingleSignals { get; }

        //public PulseOutSingleSignal TimeFrame { get; set; }
        //public PulseOutGroupSignal Update { get; set; }
        public RelayCommand UpdateCommand { get; set; }
        public bool HasCommand { get => UpdateCommand != null; }

        public void AddSignalViews(params SignalView[] signalViews)
        {
            SingleSignals.AddRange(signalViews);
        }
    }
    public abstract class SignalView
    {
        public string Title { get; set; }
        public SignalBase Signal { get; set; }
    }
    /// <summary>
    /// TextBox
    /// </summary>
    public class TextBoxSignalView : SignalView
    {

    }

    public class CommandSignalView : SignalView
    {
        public CommandSignalView(RelayCommand command)
        {
            ChangeSignalCommand = command;
        }

        public RelayCommand ChangeSignalCommand { get; }
    }

    public class PulseInSignal : LimitsSignalBase, IGroupSignal
    {
        public string GroupName { get; set; }
        public PulseInSignal()
        {

        }

        public PulseInSignal(string groupName)
        {
            GroupName = groupName;
        }

        public PulseInSignal(Stores.Signal signal, string viewName, string groupName) : base(signal, viewName)
        {
            GroupName = groupName;
        }
    }

    public class PulseOutSingleSignal : TransFormSignalBase, ISyncValue
    {
        private double tempValue;
        //public new bool InOrOut { get; }
        public PulseOutSingleSignal()
        {
            InOrOut = true;
        }

        public PulseOutSingleSignal(Stores.Signal signal, string viewName) : base(signal, viewName)
        {
            InOrOut = true;
        }

        [XmlIgnore]
        public double? TempValue { get => tempValue; set => SetProperty(ref tempValue, value.Value); }
        [XmlIgnore]
        public bool Sync { get; set; } = true;

        protected override void OnOriginValueChaned(double originValue, bool changed)
        {
            if (changed)
                TempValue = TransForm(originValue);
        }

        public void UpdateRealValue()
        {
            OriginValue = tempValue;
        }
    }
    ///// <summary>
    ///// include PulseOutSingleSignal
    ///// </summary>
    //public class PulseOutSingleSignalGroup : SignalGroupBase
    //{
    //    public List<PulseOutSingleSignal> Signals { get; }
    //    public PulseOutSingleSignalGroup(string groupName) : base(groupName)
    //    {
    //        Signals = new List<PulseOutSingleSignal>();
    //    }
    //}

    /// <summary>
    /// Has GroupName PulseOutSingleSignal
    /// <para><see cref="SignalBase.Name"/> Only Frequency Or DutyCycle</para>
    /// <para><see cref="GroupName"/> is the SignalName Split with "_"</para>
    /// </summary>
    public class PulseOutGroupSignal : PulseOutSingleSignal, IGroupSignal
    {
        public string GroupName { get; set; }
       
        public PulseOutGroupSignal():base()
        {
            //InOrOut = true;
        }
        public PulseOutGroupSignal(Stores.Signal signal, string viewName, string groupName) : base(signal, viewName)
        {
            GroupName = groupName;
        }

    }
    /// <summary>
    /// included Frequency and Duty Cycle two signals Group
    /// </summary>
    public class PulseOutGroupSignalGroup : SignalGroupBase
    {
        private PulseOutGroupSignal freq;
        private PulseOutGroupSignal dutyCycle;
        private bool currentViewEnable = true;

        public PulseOutGroupSignalGroup(string groupName) : base(groupName)
        {
        }
        public bool CurrentViewEnable { get => currentViewEnable; set => SetProperty(ref currentViewEnable , value); }
        public PulseOutGroupSignal Freq { get => freq; set => SetProperty(ref freq, value); }
        public PulseOutGroupSignal DutyCycle { get => dutyCycle; set => SetProperty(ref dutyCycle, value); }
        public void UpdateViewEnable(string showViewName)
        {
            CurrentViewEnable = freq.Views.FirstOrDefault(x => x.ViewName == showViewName) != null && 
                freq.Views.FirstOrDefault(x => x.ViewName == showViewName).IsEnabled;
            if (!CurrentViewEnable)
            {
                Note = $"Please Control this in [{freq.Views.FirstOrDefault(x => x.IsEnabled).ViewName}] page";
            }
        }

        public string Note { get; set; }
    }
}
