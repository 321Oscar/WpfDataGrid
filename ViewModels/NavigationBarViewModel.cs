using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WpfApp1.Commands;
using WpfApp1.Services;
using WpfApp1.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WpfApp1.ViewModels
{
    public class NavigationBarViewModel : ObservableRecipient
    {
        private System.Collections.ObjectModel.ObservableCollection<NavigationBarCommand> _commands;

        public NavigationBarViewModel(params INavigationService[] navigationServices)
        {
            _commands = new System.Collections.ObjectModel.ObservableCollection<NavigationBarCommand>();
            for (int i = 0; i < navigationServices.Length; i++)
            {
                NavigationBarCommand cmd = new NavigationBarCommand(navigationServices[i]);
                _commands.Add(cmd);
            }
        }
        ~NavigationBarViewModel() 
        { 
        
        }

        protected override void OnDeactivated()
        {
            //_accountStore.CurrentAccountChanged -= OnCurrentAccountChanged;
            base.OnDeactivated();
        }

        private void OnCurrentAccountChanged()
        {
            //OnPropertyChanged(nameof(IsLoggedIn));
        }
    }

 
}
