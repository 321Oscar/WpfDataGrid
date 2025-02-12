using CommunityToolkit.Mvvm.ComponentModel;
using Navigation.MVVM.WPF.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Navigation.MVVM.WPF.Services
{
    public class NavigationService<TViewModel> : INavigationService where TViewModel : ObservableRecipient
    {
        private readonly NavigationStore _navigationStore;
        private Func<TViewModel> _createViewModel;

        public NavigationService(NavigationStore navigationStore, Func<TViewModel> createViewModel)
        {
            _navigationStore = navigationStore;
            _createViewModel = createViewModel;
        }

        public void Navigate()
        {
            _navigationStore.CurrentViewModel = _createViewModel();
        }
    }

    public class ModalNavigationService<TViewModel> :INavigationService
        where TViewModel: ObservableRecipient
    {
        private readonly ModalNavigationStore _navigationStore;
        private readonly Func<TViewModel> _createViewModel;

        public ModalNavigationService(ModalNavigationStore navigationStore, Func<TViewModel> createViewModel)
        {
            _navigationStore = navigationStore;
            _createViewModel = createViewModel;
        }

        public void Navigate()
        {
            _navigationStore.CurrentViewModel = _createViewModel();
        }
    }

    public class CloseModalNavigationService : INavigationService
    {
        private readonly ModalNavigationStore _navigationStore;
        public CloseModalNavigationService(ModalNavigationStore navigationStore)
        {
            _navigationStore = navigationStore;
        }
        public void Navigate()
        {
            _navigationStore.Close();
        }
    }

    public class ParamterNavigationService<TParamter, TViewModel>
        where TViewModel : ObservableRecipient
    {
        private readonly NavigationStore _navigationStore;
        private readonly Func<TParamter,TViewModel> _createViewModel;

        public ParamterNavigationService(NavigationStore navigationStore, Func<TParamter, TViewModel> createViewModel)
        {
            _navigationStore = navigationStore;
            _createViewModel = createViewModel;
        }

        public void Navigate(TParamter paramter)
        {
            _navigationStore.CurrentViewModel = _createViewModel(paramter);
        }
    }
}
