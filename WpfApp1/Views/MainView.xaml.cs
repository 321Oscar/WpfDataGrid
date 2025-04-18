﻿using ERad5TestGUI.Models;
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
using System.Windows.Shapes;

namespace ERad5TestGUI.Views
{
    /// <summary>
    /// MainView.xaml 的交互逻辑
    /// </summary>
    public partial class MainView : Window
    {
        private Services.LogService logService;
        private Stores.DeviceStore deviceStore;
        private Stores.SignalStore signalStore;

        public MainView()
        {
            InitializeComponent();
            Init();
            this.DataContext = new ViewModels.Main2ViewModel(deviceStore, logService, signalStore);
        }

        private void Init()
        {
            logService = new Services.LogService();
            signalStore = new Stores.SignalStore(logService);
            deviceStore = new Stores.DeviceStore(signalStore, logService);
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if(e.Parameter is SignalBase signal)
            {
                Clipboard.SetText($"{signal}");
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            var vm = this.DataContext as ViewModels.Main2ViewModel;
            vm.Dispose();

            base.OnClosed(e);
        }
    }
}
