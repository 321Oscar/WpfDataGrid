using System;
using System.Collections.Generic;
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

namespace ERad5TestGUI.Views
{
    /// <summary>
    /// NXPView.xaml 的交互逻辑
    /// </summary>
    public partial class NXPView : UserControl
    {
        public NXPView()
        {
            InitializeComponent();
        }

        private void SplitButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            object[] pas = new object[2];

            //ClearDataCommand:SignalStore,ViewName
            var vm = this.DataContext as ViewModels.NXPViewModel;
            //var signalStore = ().SignalStore;

            pas[0] =  vm.SignalStore;
            pas[1] =  vm.ViewName;

            btn.Command.Execute(pas);
        }
    }
}
