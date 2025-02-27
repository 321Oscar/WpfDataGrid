using CommunityToolkit.Mvvm.Input;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Navigation;

namespace ERad5TestGUI.Services
{
    public interface INavigationService
    {
        string NavigationName { get; }
        void Navigate();
    }

    public class NavigationBarCommand
    {
        public NavigationBarCommand(INavigationService navigationService)
        {
            CommandName = navigationService.NavigationName;
            Command = new RelayCommand(()=> navigationService.Navigate());
        }

        public string CommandName { get; }

        public ICommand Command { get; }
    }
}
