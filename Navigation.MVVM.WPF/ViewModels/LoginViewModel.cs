using CommunityToolkit.Mvvm.ComponentModel;
using Navigation.MVVM.WPF.Commands;
using Navigation.MVVM.WPF.Models;
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
    public class LoginViewModel : ObservableRecipient
    {
        private string username;
        private string email;

        public string Username
        {
            get => username;
            set => SetProperty(ref username, value);
        }

        public string Email
        {
            get => email;
            set => SetProperty(ref email, value);
        }

        public ICommand LoginCommand { get; }

        public LoginViewModel(AccountStore accountStore, INavigationService accountNavigationService)
        {
            LoginCommand = new LoginCommand(this, accountStore,accountNavigationService);
        }

    }
}
