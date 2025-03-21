﻿using System;
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

namespace ERad5TestGUI.Views
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

        private void Expander_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
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

        private async void groupTag_Checked(object sender, RoutedEventArgs e)
        {
            if (!(this.FindResource("DataGridData") is CollectionViewSource collection) ||
                !(this.DataContext is ViewModels.AnalogViewModel vm))
                return;

            await Task.Run(async () =>
             {
                 vm.IsLoading = true;
                 vm.Dispatch(() =>
                 {
                     if (groupTag.IsChecked == true)
                     {
                         collection.GroupDescriptions.Clear();
                         collection.GroupDescriptions.Add(new PropertyGroupDescription("GroupName"));
                     }
                     else
                     {
                         collection.GroupDescriptions.Clear();
                     }
                 });

                 await Task.Delay(1000);
             }).ContinueWith(x =>
             {
                 vm.IsLoading = false;
             });


        }

        private void tog_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
