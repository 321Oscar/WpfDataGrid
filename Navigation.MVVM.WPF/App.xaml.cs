using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Microsoft.Extensions.Logging;
using Navigation.MVVM.WPF.Services;
using Navigation.MVVM.WPF.Stores;
using Navigation.MVVM.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Navigation.MVVM.WPF
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        
        private readonly IHost _host;
        public App()
        {
            //Ioc.Default.ConfigureServices();
            _host = Host.CreateDefaultBuilder()
                .UseSerilog((host, loggerConfiguration) =>
                {
                    loggerConfiguration.WriteTo.File("test.txt", rollingInterval: RollingInterval.Day)
                        .WriteTo.Debug()
                        .MinimumLevel.Error()
                        .MinimumLevel.Override("LoggingDemo.Commands", Serilog.Events.LogEventLevel.Debug);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<AccountStore>();
                    services.AddSingleton<NavigationStore>();
                    services.AddSingleton<ModalNavigationStore>();
                    services.AddSingleton<INavigationService>(s => CreateHomeNavigationService(s));
                    services.AddSingleton<CloseModalNavigationService>(s => new CloseModalNavigationService(s.GetRequiredService<ModalNavigationStore>()));
                    services.AddTransient<HomeViewModel>(s => new HomeViewModel(CreateLoginNavigationService(s)));
                    services.AddTransient<AccountViewModel>(s => new AccountViewModel(
                        s.GetRequiredService<AccountStore>(),
                        CreateHomeNavigationService(s)));
                    services.AddTransient<LoginViewModel>(CreateLoginViewModel);
                    services.AddSingleton<NavigationBarViewModel>(CreateNavigationBarViewModel);
                    services.AddSingleton<MainViewModel>();
                    services.AddSingleton<MainWindow>(s => new MainWindow()
                    {
                        DataContext = s.GetRequiredService<MainViewModel>()
                    });
                })
                .Build();
            //IServiceCollection services = new ServiceCollection();

            //services.AddSingleton<AccountStore>();
            //services.AddSingleton<NavigationStore>();
            //services.AddSingleton<ModalNavigationStore>();

            //services.AddSingleton<INavigationService>(s => CreateHomeNavigationService(s));
            //services.AddSingleton<CloseModalNavigationService>(s => new CloseModalNavigationService(s.GetRequiredService<ModalNavigationStore>()));

            //services.AddTransient<HomeViewModel>(s => new HomeViewModel(CreateLoginNavigationService(s)));
            //services.AddTransient<AccountViewModel>(s => new AccountViewModel(
            //    s.GetRequiredService<AccountStore>(),
            //    CreateHomeNavigationService(s)));
            //services.AddTransient<LoginViewModel>(CreateLoginViewModel);
            //services.AddSingleton<NavigationBarViewModel>(CreateNavigationBarViewModel);
            //services.AddSingleton<MainViewModel>();

            //services.AddSingleton<MainWindow>(s => new MainWindow()
            //{
            //    DataContext = s.GetRequiredService<MainViewModel>()
            //});

            //_serviceProvider = services.BuildServiceProvider();
        }
        private IServiceProvider _serviceProvider => _host.Services;

        protected override void OnStartup(StartupEventArgs e)
        {
            //base.OnStartup(e);
            _host.Start();

            var homeNavigationService = CreateHomeNavigationService(_serviceProvider);
            homeNavigationService.Navigate();


            MainWindow mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await _host.StopAsync();
            _host.Dispose();

            base.OnExit(e);
        }

        private LoginViewModel CreateLoginViewModel(IServiceProvider provider)
        {
            CompositeNavigationService navigationService = new CompositeNavigationService(
               provider.GetRequiredService<CloseModalNavigationService>(),
               CreateAccountNavigationService(provider));

            return new LoginViewModel(provider.GetRequiredService<AccountStore>(), navigationService);
        }

        private INavigationService CreateHomeNavigationService(IServiceProvider serviceProvider)
        {
            return new LayoutNavigationService<HomeViewModel>(
                serviceProvider.GetRequiredService<NavigationStore>(), 
                    () => serviceProvider.GetRequiredService<HomeViewModel>(),
                    () => serviceProvider.GetRequiredService<NavigationBarViewModel>());
        }

        private INavigationService CreateLoginNavigationService(IServiceProvider serviceProvider)
        {
           return new ModalNavigationService<LoginViewModel>(
               serviceProvider.GetRequiredService<ModalNavigationStore>(),
               () => serviceProvider.GetRequiredService<LoginViewModel>());
        }

        private INavigationService CreateAccountNavigationService(IServiceProvider serviceProvider)
        {
            return new LayoutNavigationService<AccountViewModel>(
                serviceProvider.GetRequiredService<NavigationStore>(),
                    () => serviceProvider.GetRequiredService<AccountViewModel>(),
                    () => serviceProvider.GetRequiredService<NavigationBarViewModel>());
        }

        private NavigationBarViewModel CreateNavigationBarViewModel(IServiceProvider serviceProvider)
        {
            return new NavigationBarViewModel(
                CreateHomeNavigationService(serviceProvider),
                CreateAccountNavigationService(serviceProvider),
                CreateLoginNavigationService(serviceProvider),
                serviceProvider.GetRequiredService<AccountStore>()
                );
        }
    }
}
