﻿using System;
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

namespace ERad5TestGUI.Dialogs
{
    /// <summary>
    /// ChangeLimitDialog.xaml 的交互逻辑
    /// </summary>
    public partial class ChangeLimitDialog : UserControl
    {
        public ChangeLimitDialog()
        {
            InitializeComponent();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (this.Parent is Window window)
            {
                window.DialogResult = true;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (this.Parent is Window window)
            {
                window.DialogResult = false;
            }
        }
    }
}
