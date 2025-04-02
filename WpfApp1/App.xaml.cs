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
using ERad5TestGUI.Services;
using ERad5TestGUI.Stores;
using ERad5TestGUI.ViewModels;

namespace ERad5TestGUI
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            //Services = ConfigureServices();
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
            services.AddSingleton<ERad5TestGUI.Services.LogService>();
            services.AddSingleton<Stores.NavigationStore>();
            services.AddSingleton<Stores.ModalNavigationStore>();
            services.AddSingleton<Stores.SignalStore>();
            services.AddSingleton<Stores.DeviceStore>();

            //services.AddSingleton<INavigationService>(s => CreateAnalogService(s));

            services.AddTransient<ViewModels.DeviceViewModel>(s => CreateDeviceViewModel(s));
            //services.AddTransient<ViewModels.DeviceViewModel>();
            services.AddTransient<ViewModels.AnalogViewModel>();
            //services.AddTransient<ViewModels.SignalLocatorViewModel>();
            services.AddTransient<ViewModels.DiscreteViewModel>();
            services.AddTransient<ViewModels.PulseInViewModel>();
            services.AddTransient<ViewModels.GDICViewModel>();
            services.AddTransient<ViewModels.PulseOutViewModel>();
            services.AddTransient<ViewModels.ResolverViewModel>();
            services.AddTransient<ViewModels.NXPViewModel>();
            services.AddTransient<ViewModels.MemoryViewModel>();
            services.AddTransient<ViewModels.SafingLogicViewModel>();
            services.AddTransient<ViewModels.PPAWLViewModel>();
            services.AddTransient<ViewModels.SPIViewModel>();
            services.AddTransient<ViewModels.LinViewModel>();
            services.AddTransient<ViewModels.NXPFlashViewModel>();
            services.AddTransient<ViewModels.LogViewModel>();
            services.AddSingleton<ViewModels.UDSUpgradeViewModel>();

            services.AddSingleton<ViewModels.MainViewModel>();

            services.AddSingleton<MainWindow>(s => new ERad5TestGUI.MainWindow()
            {
                DataContext = s.GetRequiredService<ViewModels.MainViewModel>()
            });

            return services.BuildServiceProvider();
        }

       
        protected override void OnStartup(StartupEventArgs e)
        {
            RegisterEvents();
            //var analogNavigationService = CreateService<AnalogViewModel>(Services,"Analog");
            //analogNavigationService.Navigate();

            //MainWindow mainWindow = Services.GetRequiredService<MainWindow>();
            //mainWindow.Show();
            //base.OnStartup(e);

            Views.MainView mainView = new Views.MainView();
            mainView.Show();
        }

        #region Exception

        /// <summary>
        /// 注册事件
        /// </summary>
        private void RegisterEvents()
        {
            //Task线程内未捕获异常处理事件
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            //UI线程未捕获异常处理事件（UI主线程）
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;

            //非UI线程未捕获异常处理事件(例如自己创建的一个子线程)
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            try
            {
                var exception = e.Exception as Exception;
                if (exception != null)
                {
                    HandleException(exception);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            finally
            {
                e.SetObserved();
            }
        }

        //非UI线程未捕获异常处理事件(例如自己创建的一个子线程)      
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                var exception = e.ExceptionObject as Exception;
                if (exception != null)
                {
                    HandleException(exception);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            finally
            {
                //ignore
            }
        }

        //UI线程未捕获异常处理事件（UI主线程）
        private static void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                HandleException(e.Exception);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            finally
            {
                //处理完后，我们需要将Handler=true表示已此异常已处理过
                e.Handled = true;
            }
        }
        private static void HandleException(Exception e)
        {
            MessageBox.Show("程序异常：" + e.Source + "\r\n@@" + Environment.NewLine + e.StackTrace + "\r\n##" + Environment.NewLine + e.Message);

            //记录日志
            //Utils.LogWrite(e);

        }

        #endregion

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
                CreateService<ViewModels.SafingLogicViewModel>(serviceProvider, "SavingLogic"),
                CreateService<ViewModels.PPAWLViewModel>(serviceProvider, "PPAWL"),
                CreateService<ViewModels.SPIViewModel>(serviceProvider, "SPI"),
                CreateService<ViewModels.UDSUpgradeViewModel>(serviceProvider, "UDSUpgrade"),
                CreateService<ViewModels.LogViewModel>(serviceProvider, "Log")
                );
        }

    }
}
