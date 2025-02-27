using CommunityToolkit.Mvvm.ComponentModel;
using ERad5TestGUI.Stores;
using ERad5TestGUI.ViewModels;

namespace ERad5TestGUI.Services
{
    public class NavigationService<TViewModel> : INavigationService where TViewModel : ObservableRecipient
    {
        private readonly INavigationStore _navigationStore;
        private readonly CreateViewModel<TViewModel> _createViewModel;

        public NavigationService(INavigationStore navigationStore, CreateViewModel<TViewModel> createViewModel)
        {
            _navigationStore = navigationStore;
            _createViewModel = createViewModel;
        }

        public string NavigationName { get; set; }

        public void Navigate()
        {
            _navigationStore.CurrentViewModel = _createViewModel();
        }
    }

    public class ModalNavigationService<TViewModel> : INavigationService
       where TViewModel : ObservableRecipient
    {
        private readonly ModalNavigationStore _navigationStore;
        private readonly CreateViewModel<TViewModel> _createViewModel;

        public ModalNavigationService(ModalNavigationStore navigationStore, CreateViewModel<TViewModel> createViewModel)
        {
            _navigationStore = navigationStore;
            _createViewModel = createViewModel;
        }
        public string NavigationName { get; set; }
        public void Navigate()
        {
            _navigationStore.CurrentViewModel = _createViewModel();
        }
    }

    public class ModalParameterNavigationService<TParameter, TViewModel>
        where TViewModel : ObservableRecipient
    {
        private readonly ModalNavigationStore _navigationStore;
        private readonly CreateViewModel<TParameter, TViewModel> _createViewModel;

        public ModalParameterNavigationService(ModalNavigationStore navigationStore, CreateViewModel<TParameter, TViewModel> createViewModel)
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
