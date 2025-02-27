using CommunityToolkit.Mvvm.ComponentModel;
using ERad5TestGUI.Stores;
using ERad5TestGUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERad5TestGUI.Services
{
    public class LayoutNavigationService<TViewModel> : INavigationService
        where TViewModel : ObservableRecipient
    {
        private readonly NavigationStore _navigationStore;
        private readonly Func<TViewModel> _createViewModel;
        private readonly Func<NavigationBarViewModel> _createNavigationBarViewModel;
        private System.Diagnostics.Stopwatch stopwatch;
        public LayoutNavigationService(NavigationStore navigationStore, Func<TViewModel> createViewModel, Func<NavigationBarViewModel> createNavigationBarViewModel, 
            string navigationName)
        {
            NavigationName = navigationName;
            Console.WriteLine($"{DateTime.Now:HH:mm:ss,fff} ctor navigation {NavigationName}");
            stopwatch = new System.Diagnostics.Stopwatch();
            //stopwatch.Restart();
            _navigationStore = navigationStore;
            _createViewModel = createViewModel;
            _createNavigationBarViewModel = createNavigationBarViewModel;
        }
        ~LayoutNavigationService()
        {
            Console.WriteLine($"{DateTime.Now:HH:mm:ss,fff} dispose navigation {NavigationName}");
        }
        public string NavigationName { get; set; }
        public void Navigate()
        {
            //stopwatch.Stop();
            //string time1 = stopwatch.ElapsedMilliseconds.ToString();
            stopwatch.Restart();
            _navigationStore.CurrentViewModel = new LayoutViewModel(_createNavigationBarViewModel(), _createViewModel());
            stopwatch.Stop();
            string time2 = stopwatch.ElapsedMilliseconds.ToString();

            Console.WriteLine($"{DateTime.Now:HH:mm:ss,fff} navigation {NavigationName}: {time2} ms");
        }
    }
}
