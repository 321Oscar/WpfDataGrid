using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Navigation.MVVM.WPF.Commands;
using Navigation.MVVM.WPF.Services;
using Navigation.MVVM.WPF.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Navigation.MVVM.WPF.ViewModels
{
    public class HomeViewModel : ObservableRecipient
    {

        public HomeViewModel(INavigationService loginNavigationService)
        {
            NavigationCommand = new NaviagtionCommand<LoginViewModel>(loginNavigationService);
            
        }

        public ICommand NavigationCommand { get; }

    }
}
