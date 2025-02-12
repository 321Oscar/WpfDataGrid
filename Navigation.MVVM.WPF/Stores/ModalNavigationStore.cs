using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Navigation.MVVM.WPF.Stores
{
    public class ModalNavigationStore
    {
        private ObservableObject currentViewModel;

        public bool IsOpen => CurrentViewModel != null;

        public event Action CurrentViewModelChanged;
        public ObservableObject CurrentViewModel
        {
            get => currentViewModel;
            set { currentViewModel = value; OnCurrentViewModelChanged(); }
        }

        private void OnCurrentViewModelChanged()
        {
            CurrentViewModelChanged?.Invoke();
        }

        public void Close()
        {
            CurrentViewModel = null;
        }
    }
}
