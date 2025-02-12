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
    public class NavigationBarViewModel : ObservableRecipient
    {

        private readonly AccountStore _accountStore;
        public ICommand NavigationHomeCommand { get; }
        public ICommand NavigationAccountCommand { get; }
        public ICommand NavigationLoginCommand { get; }
        public ICommand LogoutCommand { get; }

        public bool IsLoggedIn => _accountStore.IsLoggedIn;

        public NavigationBarViewModel(
            INavigationService navigationHomeService,
            INavigationService navigationAccountService,
            INavigationService navigationLoginService,
            AccountStore accountStore)
        {
            _accountStore = accountStore;

            NavigationHomeCommand = new NaviagtionCommand<HomeViewModel>(navigationHomeService);
            NavigationAccountCommand = new NaviagtionCommand<AccountViewModel>(navigationAccountService);
            NavigationLoginCommand = new NaviagtionCommand<LoginViewModel>(navigationLoginService);

            LogoutCommand = new RelayCommand(() => _accountStore.Logout()) ;

            _accountStore.CurrentAccountChanged += OnCurrentAccountChanged;
        }
        ~NavigationBarViewModel() 
        { 
        
        }

        protected override void OnDeactivated()
        {
            _accountStore.CurrentAccountChanged -= OnCurrentAccountChanged;
            base.OnDeactivated();
        }

        private void OnCurrentAccountChanged()
        {
            OnPropertyChanged(nameof(IsLoggedIn));
        }
    }
}
