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
            AdonisUI.Controls.SplitButton btn = sender as AdonisUI.Controls.SplitButton;

           
            //double x = mousePosition.X;
            //double y = mousePosition.Y;

            //btn.PART_MenuExpander
            var expander = btn.Template.FindName("PART_MenuExpander", btn) as Button;
            var mousePosition = Mouse.GetPosition(expander);

            //不在expander中则执行Command
            if (mousePosition.X >= 0 && mousePosition.X <= expander.ActualWidth &&
                  mousePosition.Y >= 0 && mousePosition.Y <= expander.ActualHeight)
            {
                return;
            }
            else
            {
                // 如果点击在 PART_Content 内部，则执行 Button 的 Command
                //expander.
                object[] pas = new object[2];

                //ClearDataCommand:SignalStore,ViewName
                var vm = this.DataContext as ViewModels.NXPViewModel;
                //var signalStore = ().SignalStore;

                pas[0] = vm.SignalStore;
                pas[1] = vm.ViewName;

                btn.Command.Execute(pas);
            }

            
        }
    }
}
