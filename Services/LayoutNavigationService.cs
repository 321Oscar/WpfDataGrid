using CommunityToolkit.Mvvm.ComponentModel;
using WpfApp1.Stores;
using WpfApp1.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Services
{
    public class LayoutNavigationService<TViewModel> : INavigationService
        where TViewModel : ObservableRecipient
    {
        private readonly NavigationStore _navigationStore;
        private readonly Func<TViewModel> _createViewModel;
        private readonly Func<NavigationBarViewModel> _createNavigationBarViewModel;

        public LayoutNavigationService(NavigationStore navigationStore, Func<TViewModel> createViewModel, Func<NavigationBarViewModel> createNavigationBarViewModel, 
            string navigationName)
        {
            _navigationStore = navigationStore;
            _createViewModel = createViewModel;
            _createNavigationBarViewModel = createNavigationBarViewModel;
            NavigationName = navigationName;
        }
        public string NavigationName { get; set; }
        public void Navigate()
        {
            _navigationStore.CurrentViewModel = new LayoutViewModel(_createNavigationBarViewModel(), _createViewModel());
        }
    }
}
