using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ERad5TestGUI.Devices;
using ERad5TestGUI.Services;
using ERad5TestGUI.Stores;
using System.Collections.ObjectModel;

namespace ERad5TestGUI.ViewModels
{
    public class DeviceViewModel : ObservableRecipient
    {
        private readonly Stores.DeviceStore deviceStore;
        private readonly LogService logService;
        private readonly INavigationService navigationService;
        private IDevice currentDevice;
        private DeviceHardWareType hardWareType;
        

        public DeviceViewModel(DeviceStore deviceStore, LogService logService)
        {
            this.deviceStore = deviceStore;
            this.logService = logService;

            SaveConfigCommand = new RelayCommand(SaveConfig, () => CurrentDevice != null);
            CancelCommand = new RelayCommand(Cancel);

            LoadZlgSource();

            if (deviceStore.HasDevice)
            {
                HardWareType = GetHardWareType(deviceStore.CurrentDevice);
                CurrentDevice = deviceStore.CurrentDevice;
            }
        }
        /// <summary>
        /// MVVM Dialog
        /// </summary>
        /// <param name="deviceStore"></param>
        /// <param name="logService"></param>
        /// <param name="navigationService"></param>
        public DeviceViewModel(DeviceStore deviceStore, LogService logService, INavigationService navigationService)
            : this(deviceStore, logService)
        {
            this.navigationService = navigationService;
        }

        public Devices.DeviceHardWareType HardWareType 
        { 
            get => hardWareType;
            set 
            { 
                hardWareType = value; 
                //CurrentDevice = null; 
            }
        }
        public IEnumerable<IDevice> VectorDevices => deviceStore.GetDevices<VectorCan>();
        public IEnumerable<IDevice> VirtualDevices => deviceStore.GetDevices<VirtualDevice>();
        public IEnumerable<IDevice> ZlgDevices => deviceStore.ZlgDeviceService.Devices;
        public IDevice CurrentDevice
        {
            get
            {
                return currentDevice;
                //return deviceStore.CurrentDevice;
            }
            set
            {
                SetProperty(ref currentDevice, value);
                (SaveConfigCommand as IRelayCommand).NotifyCanExecuteChanged();
                if (currentDevice is ZlgDeviceCanChannel zlgChannel)
                {
                    ZlgDeviceIndex = zlgChannel.DeviceIndex;
                    ZlgDeviceType = (Devices.ZlgAPI.ZlgDeviceType)zlgChannel.DeviceType;
                    ZlgCanChannel = (int)zlgChannel.ChannelIndex;
                }
            }
        }

        public ICommand SaveConfigCommand { get; }
        protected virtual void SaveConfig()
        {
            deviceStore.CurrentDevice = currentDevice;
            deviceStore.CurrentDevice.Open();
            //deviceStore.CurrentDevice.Start();
            //open and start receive
            
            navigationService?.Navigate();
        }

        public ICommand CancelCommand { get; }
        protected virtual void Cancel()
        {
            navigationService?.Navigate();
        }

        private DeviceHardWareType GetHardWareType(IDevice device)
        {
            if (device is VirtualDevice)
                return DeviceHardWareType.Virtual;
            else if (device is ZlgDeviceCanChannel)
                return DeviceHardWareType.Zlg;
            return DeviceHardWareType.Vector;
        }

        #region Zlg CAN
        private uint _zlgDeviceIndex;
        private uint _zlgDeviceTypeIndex;
        private Devices.ZlgAPI.ZlgDeviceType _zlgDeviceType;
        private int _zlgDeviceChannelIndex;
        private RelayCommand _addZlgCommand;
        private RelayCommand _delZlgCommand;
        public ICommand AddZlgChannelCommand { get => _addZlgCommand ?? (_addZlgCommand = new RelayCommand(AddZlgChannel)); }
        public ICommand DelZlgChannelCommand { get => _delZlgCommand ?? (_delZlgCommand = new RelayCommand(DelZlgChannel)); }
        //private uint _zlgDeviceTypeIndex;
        public IEnumerable<KeyValuePair<uint, string>> ZLGDeviceTypes
        {
            get;
            private set;
        }
        public ObservableCollection<uint> CanChannelSource { get; set; }
        /// <summary>
        /// 选择的设备索引
        /// </summary>
        public uint ZlgDeviceIndex
        {
            get => _zlgDeviceIndex;
            set => SetProperty(ref _zlgDeviceIndex, value);
        }

        /// <summary>
        /// 选择的设备类型
        /// </summary>
        //public uint ZlgDeviceTypeIndex
        //{
        //    get => _zlgDeviceTypeIndex;
        //    set
        //    {
        //        if (SetProperty(ref _zlgDeviceTypeIndex, value))
        //        {
        //            CanChannelSource.Clear();
        //            foreach (var channel in Devices.ZlgAPI.Define.GetCanChannels(_zlgDeviceTypeIndex))
        //            {
        //                CanChannelSource.Add(channel);
        //            }
        //            ZlgDeviceIndex = 0;
        //            ZlgCanChannel = 0;
        //            //IsTCPDevice = CANFactory.TcpDevice((DeviceType)deviceTypeIndex);
        //        }
        //    }
        //}

        public Devices.ZlgAPI.ZlgDeviceType ZlgDeviceType
        {
            get => _zlgDeviceType;
            set
            {
                if(SetProperty(ref _zlgDeviceType, value))
                {
                    CanChannelSource.Clear();
                    foreach (var channel in Devices.ZlgAPI.Define.GetCanChannels((uint)_zlgDeviceType))
                    {
                        CanChannelSource.Add(channel);
                    }
                    ZlgDeviceIndex = 0;
                    ZlgCanChannel = 0;
                }
            }
        }

        public int ZlgCanChannel
        {
            get => _zlgDeviceChannelIndex;
            set => SetProperty(ref _zlgDeviceChannelIndex, value);
        }

        private void LoadZlgSource()
        {
            CanChannelSource = new ObservableCollection<uint>();
            var source = new Dictionary<uint, string>();
            foreach (var item in Enum.GetValues(typeof(Devices.ZlgAPI.ZlgDeviceType)))
            {
                source.Add((uint)(Devices.ZlgAPI.ZlgDeviceType)Enum.Parse(typeof(Devices.ZlgAPI.ZlgDeviceType), item.ToString()), item.ToString());
            }
            ZLGDeviceTypes = source;
        }

        private void AddZlgChannel()
        {
            if (ZlgDeviceType != 0)
                deviceStore.ZlgDeviceService.CreateZlgChannel((uint)ZlgDeviceType, ZlgDeviceIndex, (uint)ZlgCanChannel);
            //deviceStore.AddDevice()
        }

        private void DelZlgChannel()
        {
            if (CurrentDevice is ZlgDeviceCanChannel zlgChannel)
            {
                deviceStore.ZlgDeviceService.CloseChannel(zlgChannel.DeviceType, zlgChannel.DeviceIndex, zlgChannel.ChannelIndex);
            }
        }

        #endregion
    }

    public class DeviceWithWindowViewModel : DeviceViewModel, IDialogWindow
    {
        public System.Windows.Window Window { get; }

        public DeviceWithWindowViewModel(DeviceStore deviceStore, LogService logService, System.Windows.Window window)
            : base(deviceStore, logService)
        {
            Window = window;
        }
        public void Ok()
        {
            Window.DialogResult = true;
            Window.Close();
        }

        //public void Cancel()
        //{
           
        //}
        protected override void SaveConfig()
        {
            base.SaveConfig();
            Ok();
        }

        protected override void Cancel()
        {
            base.Cancel();
            Window.DialogResult = false;
            Window.Close();
        }
    }

    public interface IDialogWindow
    {
        System.Windows.Window Window { get; }
        void Ok();
        //void Cancel();
    }
}
