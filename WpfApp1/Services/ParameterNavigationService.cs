using CommunityToolkit.Mvvm.ComponentModel;
using ERad5TestGUI.Stores;
using ERad5TestGUI.ViewModels;

namespace ERad5TestGUI.Services
{
    public class ParameterNavigationService<TParameter, TViewModel>
        where TViewModel : ViewModels.ViewModelBase
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
