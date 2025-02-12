using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Navigation.MVVM.WPF.ViewModels
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
