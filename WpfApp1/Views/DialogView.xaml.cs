﻿using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// DialogView.xaml 的交互逻辑
    /// </summary>
    public partial class DialogView : Window, INotifyPropertyChanged
    {
        private ObservableRecipient dialogViewModel;

        public DialogView()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public DialogView(CommunityToolkit.Mvvm.ComponentModel.ObservableRecipient dialogViewModel) : this()
        {
            DialogViewModel = dialogViewModel;
        }

        public CommunityToolkit.Mvvm.ComponentModel.ObservableRecipient DialogViewModel 
        { 
            get => dialogViewModel;
            set 
            { 
                dialogViewModel = value;
                RasiePropertyChanged(nameof(DialogViewModel));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RasiePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
