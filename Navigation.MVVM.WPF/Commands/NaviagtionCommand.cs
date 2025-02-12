using CommunityToolkit.Mvvm.ComponentModel;
using Navigation.MVVM.WPF.Models;
using Navigation.MVVM.WPF.Services;
using Navigation.MVVM.WPF.Stores;
using Navigation.MVVM.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Navigation.MVVM.WPF.Commands
{
    public class NaviagtionCommand<TViewModel> : CommandBase
        where TViewModel : ObservableRecipient
    {
        private readonly INavigationService _navigationService;

        public NaviagtionCommand(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        public override void Execute(object parameter)
        {
            _navigationService.Navigate();
        }
    }

    public class LoginCommand : CommandBase
    {
        private readonly LoginViewModel _loginViewModel;
        private readonly AccountStore _accountStore;
        private readonly INavigationService _navigationService;

        public LoginCommand(LoginViewModel loginViewModel,AccountStore accountStore, INavigationService navigationService)
        {
            _loginViewModel = loginViewModel;
            _navigationService = navigationService;
            _accountStore = accountStore;
        }

        public override void Execute(object parameter)
        {
            //MessageBox.Show($"Login {_loginViewModel.Name}");
            Account account = new Account()
            {
                Name = _loginViewModel.Username,
                Email = _loginViewModel.Username + "@test.com"
            };
            _accountStore.CurrentAccount = account;

            _navigationService.Navigate();
        }
    }
}
