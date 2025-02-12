using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
    public partial class AccountViewModel : ObservableRecipient
    {
        private readonly AccountStore _accountStore;
        
        public string Username=>_accountStore.CurrentAccount?.Name;
        public string Email => _accountStore.CurrentAccount?.Email;

        public AccountViewModel(AccountStore accountStore, INavigationService homeNavigationService)
        {
            this._accountStore = accountStore;
            _accountStore.CurrentAccountChanged += OnCurrentAccountChanged;
            NavigationToHomeCommand = new NaviagtionCommand<HomeViewModel>(homeNavigationService);
           
            //_navigationStore = navigationStore;
        }

        ~AccountViewModel()
        {

        }

        protected override void OnActivated()
        {
            base.OnActivated();
        }

        protected override void OnDeactivated()
        {
            _accountStore.CurrentAccountChanged -= OnCurrentAccountChanged;
            base.OnDeactivated();
        }

        private void OnCurrentAccountChanged()
        {
            OnPropertyChanged(nameof(Username));
            OnPropertyChanged(nameof(Email));
        }

        public ICommand NavigationToHomeCommand { get; }

    }
}
