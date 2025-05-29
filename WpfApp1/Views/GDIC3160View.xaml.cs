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
    /// GDIC3160View.xaml 的交互逻辑
    /// </summary>
    public partial class GDIC3160View : UserControl
    {
        public GDIC3160View()
        {
            InitializeComponent();
        }

        private void s1_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (sender == sv1)
            {
                sv2.ScrollToVerticalOffset(e.VerticalOffset);
                sv2.ScrollToHorizontalOffset(e.HorizontalOffset);
            }
            else
            {
                sv1.ScrollToVerticalOffset(e.VerticalOffset);
                sv1.ScrollToHorizontalOffset(e.HorizontalOffset);
            }
        }

        private void SplitButton_Click(object sender, RoutedEventArgs e)
        {
            AdonisUI.Controls.SplitButton btn = sender as AdonisUI.Controls.SplitButton;

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
                if(this.DataContext is Interfaces.IClearData clearData)
                {
                    clearData.ClearData();
                }

                //// 如果点击在 PART_Content 内部，则执行 Button 的 Command
                ////expander.
                //object[] pas = new object[2];

                ////ClearDataCommand:SignalStore,ViewName
                //var vm = this.DataContext as ViewModels.NXPViewModel;
                ////var signalStore = ().SignalStore;

                //pas[0] = vm.SignalStore;
                //pas[1] = vm.ViewName;

                //btn.Command.Execute(pas);
            }

        }
    }
}
