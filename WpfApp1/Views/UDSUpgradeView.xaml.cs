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
    /// UDSUpgradeView.xaml 的交互逻辑
    /// </summary>
    public partial class UDSUpgradeView : UserControl
    {
        public UDSUpgradeView()
        {
            InitializeComponent();
        }

        private void ListView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;

                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                var parent = ((Control)sender).Parent as UIElement;
                while (!(parent is ScrollViewer))
                {
                    if (parent is Control)
                    {
                        if (((Control)parent).Parent != null)
                        {
                            parent = ((Control)parent).Parent as UIElement;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else if (parent is Grid)
                    {
                        parent = ((Grid)parent).Parent as UIElement;
                    }
                    else
                    {
                        break;
                    }
                }
                parent.RaiseEvent(eventArg);
            }
        }
    }
}
