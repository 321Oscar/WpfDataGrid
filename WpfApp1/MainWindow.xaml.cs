using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using vxlapi_NET;
using Microsoft.Win32.SafeHandles;
using AdonisUI.Controls;
using AdonisUI;

namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : AdonisWindow
    {
        public bool IsDark
        {
            get => (bool)GetValue(IsDarkProperty);
            set => SetValue(IsDarkProperty, value);
        }

        public static readonly DependencyProperty IsDarkProperty = DependencyProperty.Register("IsDark", typeof(bool), typeof(MainWindow), new PropertyMetadata(false, OnIsDarkChanged));

        private static void OnIsDarkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MainWindow)d).ChangeTheme((bool)e.OldValue);
        }

        public List<Models.AnalogSignal> AnalogSignals { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            //this.DataContext = this;
            //LoadVectorDevices();
            //LoadAnalog();
            // 1.列表中的信号值更新，一个信号名能够对应多个值
            // 2.列表修改checkbox，并且能够决定下发报文的时机
            // 3.计算信号的平均值并实时更新：要记录存储的数据
        }
        ~MainWindow()
        {

        }

        private void ChangeTheme(bool oldValue)
        {
            ResourceLocator.SetColorScheme(Application.Current.Resources, oldValue ? ResourceLocator.LightColorScheme : ResourceLocator.DarkColorScheme);
        }

        void LoadAnalog()
        {
            AnalogSignals = new List<Models.AnalogSignal>()
            {
                new Models.AnalogSignal()
                {
                    Name="AnalogSignal1",
                    OriginValue = 10
                }
            };
            //analogView.DataContext = AnalogSignals;
        }

        void LoadVectorDevices()
        {
            //Device = new Devices.VectorCan();
            //cbbChannels.Items.Clear();
            //foreach (var channelCfg in Device.Channels)
            //{
                //cbbChannels.Items.Add(channelCfg.name);
            //}
        }

        private void Button_OpenPort_Click(object sender, RoutedEventArgs e)
        {
            //if (Device != null && cbbChannels.SelectedIndex > -1)
            //{
            //    Device.OpenPort(cbbChannels.SelectedIndex);
            //    Device.OnMsgReceived += Device_OnMsgReceived;
            //    WriteLine($"Opened");
            //}
        }
        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            //Device.OnMsgReceived -= Device_OnMsgReceived;
            //Device.ClosePort();
        }

        private void Device_OnMsgReceived(IEnumerable<Devices.IFrame> frames)
        {
            foreach (var item in frames)
            {
                WriteLine($"{item.MessageID:X} {string.Join(" ", item.Data.Select(x => x.ToString("X2")))}");
            }
            
        }

        private void Button_Send_Click(object sender, RoutedEventArgs e)
        {
            //if (Device != null)
            //{
                //Device.CANFDTransmitTest();
            //}
        }


        public void WriteLine(string line)
        {
            //if(this.)
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Run runMessage = new Run(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - " + line);
                Paragraph paragraph = new Paragraph(runMessage);
                //TxMsg.Document.Blocks.Add(paragraph);
                //TxMsg.ScrollToEnd();

                //TxMsg.Document.
                //TxMsg.AppendText(line);
                //TxMsg.AppendText("\n\r");
            }));

        }

        private void ContentControl_Loaded(object sender, RoutedEventArgs e)
        {
            Console.WriteLine($"{DateTime.Now:HH:mm:ss fff} ViewModel Loaded");
        }
    }
}
