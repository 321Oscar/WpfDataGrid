using Navigation.MVVM.WPF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Navigation.MVVM.WPF.Stores
{
    public class AccountStore
    {
        private Account _currentAccount;
        public Account CurrentAccount
        {
            get => _currentAccount;
            set
            {
                _currentAccount = value;
                CurrentAccountChanged?.Invoke();
            }
        }
        public event Action CurrentAccountChanged;
        public bool IsLoggedIn => CurrentAccount != null;

        public void Logout()
        {
            CurrentAccount = null;
        }
    }
}
