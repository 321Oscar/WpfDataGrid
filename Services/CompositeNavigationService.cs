using System.Collections.Generic;

namespace WpfApp1.Services
{
    public class CompositeNavigationService : INavigationService
    {
        private readonly IEnumerable<INavigationService> _navigationServices;

        public CompositeNavigationService(params INavigationService[] navigationServices)
        {
            _navigationServices = navigationServices;
        }

        public string NavigationName { get; set; }

        public void Navigate()
        {
            foreach (INavigationService navigationService in _navigationServices)
            {
                navigationService.Navigate();
            }
        }
    }
}
