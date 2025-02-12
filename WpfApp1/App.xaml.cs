using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using WpfApp1.Services;
using WpfApp1.Stores;
using WpfApp1.ViewModels;

namespace WpfApp1
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Services = ConfigureServices();
        }

        /// <summary>
        /// Gets the current <see cref="App"/> instance in use
        /// </summary>
        public new static App Current => (App)Application.Current;

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
        /// </summary>
        public IServiceProvider Services { get; }

        /// <summary>
        /// Configures the services for the application.
        /// </summary>
        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            services.AddSingleton<WpfApp1.Services.LogService>();
            services.AddSingleton<Stores.NavigationStore>();
            services.AddSingleton<Stores.ModalNavigationStore>();
            services.AddSingleton<Stores.SignalStore>();
            services.AddSingleton<Stores.DeviceStore>();

            //services.AddSingleton<INavigationService>(s => CreateAnalogService(s));

            services.AddTransient<ViewModels.DeviceViewModel>(s => CreateDeviceViewModel(s));
            //services.AddTransient<ViewModels.DeviceViewModel>();
            services.AddTransient<ViewModels.AnalogViewModel>();
            services.AddTransient<ViewModels.DiscreteViewModel>();
            services.AddTransient<ViewModels.PulseInViewModel>();
            services.AddTransient<ViewModels.GDICViewModel>();
            services.AddTransient<ViewModels.PulseOutViewModel>();
            services.AddTransient<ViewModels.ResolverViewModel>();
            services.AddTransient<ViewModels.NXPViewModel>();
            services.AddTransient<ViewModels.MemoryViewModel>();
            services.AddTransient<ViewModels.SavinLogicViewModel>();
            services.AddTransient<ViewModels.PPAWLViewModel>();
            services.AddTransient<ViewModels.SPIViewModel>();
            services.AddTransient<ViewModels.LinViewModel>();
            services.AddTransient<ViewModels.NXPFlashViewModel>();
            services.AddTransient<ViewModels.LogViewModel>();
            services.AddSingleton<ViewModels.UDSUpgradeViewModel>();

            services.AddSingleton<ViewModels.MainViewModel>();

            services.AddSingleton<MainWindow>(s => new WpfApp1.MainWindow()
            {
                DataContext = s.GetRequiredService<ViewModels.MainViewModel>()
            });

            return services.BuildServiceProvider();
        }

       
        protected override void OnStartup(StartupEventArgs e)
        {
            var analogNavigationService = CreateService<AnalogViewModel>(Services,"Analog");
            analogNavigationService.Navigate();

            MainWindow mainWindow = Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
            //base.OnStartup(e);
        }

        private DeviceViewModel CreateDeviceViewModel(IServiceProvider serviceProvider)
        {
            return new DeviceViewModel(serviceProvider.GetRequiredService<DeviceStore>(),
                serviceProvider.GetRequiredService<LogService>(),
                new CloseModalNavigationService(serviceProvider.GetRequiredService<ModalNavigationStore>()));
        }

        private INavigationService CreateAnalogService(IServiceProvider serviceProvider)
        {
            return new LayoutNavigationService<ViewModels.AnalogViewModel>(
                serviceProvider.GetRequiredService<Stores.NavigationStore>(),
                () => serviceProvider.GetRequiredService<ViewModels.AnalogViewModel>(),
                () => CreateNavigationBarViewModel(serviceProvider),
                "Analog");
        }

        private INavigationService CreateService<TViewModel>(IServiceProvider serviceProvider, string name)
            where TViewModel : CommunityToolkit.Mvvm.ComponentModel.ObservableRecipient
        {
            return new LayoutNavigationService<TViewModel>(
              serviceProvider.GetRequiredService<Stores.NavigationStore>(),
              () => serviceProvider.GetRequiredService<TViewModel>(),
              () => CreateNavigationBarViewModel(serviceProvider),
              name);
        }

        private NavigationBarViewModel CreateNavigationBarViewModel(IServiceProvider serviceProvider)
        {
            return new NavigationBarViewModel(
                CreateService<AnalogViewModel>(serviceProvider, "Analog"),
                CreateService<ViewModels.DiscreteViewModel>(serviceProvider,"Discrete"),
                CreateService<ViewModels.PulseInViewModel>(serviceProvider, "PulseIn"),
                CreateService<ViewModels.GDICViewModel>(serviceProvider, "GDIC"),
                CreateService<ViewModels.PulseOutViewModel>(serviceProvider, "PulseOut"),
                CreateService<ViewModels.NXPViewModel>(serviceProvider, "NXP"),
                CreateService<ViewModels.ResolverViewModel>(serviceProvider, "Resolver"),
                CreateService<ViewModels.MemoryViewModel>(serviceProvider, "Memory"),
                CreateService<ViewModels.SavinLogicViewModel>(serviceProvider, "SavingLogic"),
                CreateService<ViewModels.PPAWLViewModel>(serviceProvider, "PPAWL"),
                CreateService<ViewModels.SPIViewModel>(serviceProvider, "SPI"),
                CreateService<ViewModels.UDSUpgradeViewModel>(serviceProvider, "UDSUpgrade"),
                CreateService<ViewModels.LogViewModel>(serviceProvider, "Log")
                );
        }

        
    }
}
