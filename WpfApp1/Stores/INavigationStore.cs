﻿using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERad5TestGUI.Stores
{
    public interface INavigationStore
    {
        ObservableRecipient CurrentViewModel { set; }
    }

    public class NavigationStore : INavigationStore
    {
        private ObservableRecipient _currentViewModel;
        public ObservableRecipient CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                if (_currentViewModel != null)
                    _currentViewModel.IsActive = false;
                _currentViewModel = value;
                if (_currentViewModel != null)
                {
                    //if (value != null && !value.IsLoading)
                    //{
                    //    Task.Run(() =>
                    //    {
                    //        value.IsLoading = true;
                    //        value.Init();
                    //    }).ContinueWith((task) => value.IsLoading = false);
                    //}
                    _currentViewModel.IsActive = true;
                    //SetProperty(ref _selectedPage, value);
                }
                    
                OnCurrentViewModelChanged();
            }
        }

        public event Action CurrentViewModelChanged;

        private void OnCurrentViewModelChanged()
        {
            CurrentViewModelChanged?.Invoke();
        }
    }

    public class ModalNavigationStore : INavigationStore
    {
        private ObservableRecipient _currentViewModel;
        public ObservableRecipient CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                //_currentViewModel?.Dispose();
                _currentViewModel = value;
                OnCurrentViewModelChanged();
            }
        }

        public bool IsOpen => CurrentViewModel != null;

        public event Action CurrentViewModelChanged;

        public void Close()
        {
            CurrentViewModel = null;
        }

        private void OnCurrentViewModelChanged()
        {
            CurrentViewModelChanged?.Invoke();
        }
    }
}
