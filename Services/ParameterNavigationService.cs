using CommunityToolkit.Mvvm.ComponentModel;
using WpfApp1.Stores;
using WpfApp1.ViewModels;

namespace WpfApp1.Services
{
    public class ParameterNavigationService<TParameter, TViewModel>
        where TViewModel : ObservableRecipient
    {
        private readonly NavigationStore _navigationStore;
        private readonly CreateViewModel<TParameter, TViewModel> _createViewModel;

        public ParameterNavigationService(NavigationStore navigationStore, CreateViewModel<TParameter, TViewModel> createViewModel)
        {
            _navigationStore = navigationStore;
            _createViewModel = createViewModel;
        }

        public void Navigate(TParameter parameter)
        {
            _navigationStore.CurrentViewModel = _createViewModel(parameter);
        }
    }
}
