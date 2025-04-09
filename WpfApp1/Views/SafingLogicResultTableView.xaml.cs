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
    /// SafingLogicResultTableView.xaml 的交互逻辑
    /// </summary>
    public partial class SafingLogicResultTableView : UserControl
    {
        public SafingLogicResultTableView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.Parent is Window window)
            {
                window.DialogResult = true;
            }
        }
    }
}
