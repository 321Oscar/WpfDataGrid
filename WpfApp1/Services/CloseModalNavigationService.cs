using WpfApp1.Stores;

namespace WpfApp1.Services
{
    public class CloseModalNavigationService : INavigationService
    {
        private readonly ModalNavigationStore _navigationStore;

        public CloseModalNavigationService(ModalNavigationStore navigationStore)
        {
            _navigationStore = navigationStore;
        }
        public string NavigationName { get; set; }
        public void Navigate()
        {
            _navigationStore.Close();
        }
    }
}
