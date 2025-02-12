using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Navigation.MVVM.WPF.Stores
{
    public class NavigationStore
    {
        private ObservableRecipient currentViewModel;

        public event Action CurrentViewModelChanged;
        public ObservableRecipient CurrentViewModel 
        { 
            get => currentViewModel;
            set 
            {
                if (currentViewModel != null)
                    currentViewModel.IsActive = false;
                currentViewModel = value;
                if (currentViewModel != null)
                    currentViewModel.IsActive = true;
                OnCurrentViewModelChanged(); 
            }
        }

        private void OnCurrentViewModelChanged()
        {
            CurrentViewModelChanged?.Invoke();
        }
    }
}
