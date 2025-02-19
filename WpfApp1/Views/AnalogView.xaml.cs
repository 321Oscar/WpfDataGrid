using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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

namespace WpfApp1.Views
{
    /// <summary>
    /// AnalogView.xaml 的交互逻辑
    /// </summary>
    public partial class AnalogView : UserControl
    {
        Stopwatch watch;

        public AnalogView()
        {
            InitializeComponent();
            Console.WriteLine($"{DateTime.Now:HH:mm:ss fff} analog view");
            watch = new Stopwatch();
            watch.Restart();
            this.Loaded += AnalogView_Loaded;
        }
        ~AnalogView()
        {

        }
        private void AnalogView_Loaded(object sender, RoutedEventArgs e)
        {

            //watch.Stop();
            //Console.WriteLine($"{watch.ElapsedMilliseconds}");
            this.Loaded-= AnalogView_Loaded;
        }

        private void datagrid_Loaded(object sender, RoutedEventArgs e)
        {
            watch.Stop();
            Console.WriteLine($"{DateTime.Now:HH:mm:ss fff} datagrid loaded take {watch.ElapsedMilliseconds} ms");

            //this.datagrid.Loaded -= datagrid_Loaded;
        }
    }
}
