using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ERad5TestGUI.Commands;
using ERad5TestGUI.Services;
using ERad5TestGUI.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ERad5TestGUI.ViewModels
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

        public IEnumerable<NavigationBarCommand> Commands => _commands;

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
