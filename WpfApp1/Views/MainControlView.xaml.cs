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
    /// MainControlView.xaml 的交互逻辑
    /// </summary>
    public partial class MainControlView : UserControl
    {
        public MainControlView()
        {
            InitializeComponent();

            this.DataContextChanged += MainControlView_DataContextChanged;
        }

        private void MainControlView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext is ViewModels.MainViewModel viewModel)
            {
                viewModel.LogService.UiLogAppender.LogControl = this.logListBox;
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            
        }
    }
}
