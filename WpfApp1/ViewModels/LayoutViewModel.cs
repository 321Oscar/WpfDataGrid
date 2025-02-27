using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERad5TestGUI.ViewModels
{
    public class LayoutViewModel : ObservableRecipient
    {
        public LayoutViewModel(NavigationBarViewModel navigationBarViewModel, ObservableRecipient contentViewModel)
        {
            NavigationBarViewModel = navigationBarViewModel;
            ContentViewModel = contentViewModel;
        }

        public NavigationBarViewModel NavigationBarViewModel { get; }
        public ObservableRecipient ContentViewModel { get; }

        protected override void OnActivated()
        {
            NavigationBarViewModel.IsActive = true;

            if (ContentViewModel is ViewModelBase viewModelBase)
            {
                if (!viewModelBase.IsLoading)
                {
                    Task.Run(() =>
                    {
                        viewModelBase.IsLoading = true;
                        viewModelBase.Init();
                    }).ContinueWith((task) => 
                    viewModelBase.IsLoading = false
                    );
                }
            }

            ContentViewModel.IsActive = true;

            base.OnActivated();
        }


        protected override void OnDeactivated()
        {
            NavigationBarViewModel.IsActive = false;
            ContentViewModel.IsActive = false;

            base.OnDeactivated();
        }
    }
}
