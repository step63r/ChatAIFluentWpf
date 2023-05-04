using Azure.Identity;
using ChatAIFluentWpf.Models;
using ChatAIFluentWpf.Services;
using ChatAIFluentWpf.Services.Interfaces;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;

namespace ChatAIFluentWpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        // The.NET Generic Host provides dependency injection, configuration, logging, and other services.
        // https://docs.microsoft.com/dotnet/core/extensions/generic-host
        // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
        // https://docs.microsoft.com/dotnet/core/extensions/configuration
        // https://docs.microsoft.com/dotnet/core/extensions/logging
        private static readonly IHost _host = Host
            .CreateDefaultBuilder()
            .ConfigureAppConfiguration(c => { c.SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)); })
            .ConfigureServices((context, services) =>
            {
                // App Host
                services.AddHostedService<ApplicationHostService>();

                // Page resolver service
                services.AddSingleton<IPageService, PageService>();

                // Theme manipulation
                services.AddSingleton<IThemeService, ThemeService>();

                // TaskBar manipulation
                services.AddSingleton<ITaskBarService, TaskBarService>();

                // Service containing navigation, same as INavigationWindow... but without window
                services.AddSingleton<INavigationService, NavigationService>();

                // register VoiceVox service.
                services.AddSingleton<IVoiceVoxService, VoiceVoxService>();

                // register Azure clinets.
                services.AddAzureClients(clientBuilder =>
                {
                    // Add a KeyVault client.
                    clientBuilder.AddSecretClient(new Uri(ChatAIFluentWpf.Properties.Settings.Default.AzureKeyVaultUri));

                    // Use DefaultAzureCredential by default.
                    clientBuilder.UseCredential(new DefaultAzureCredential());
                });

                // Main window with navigation
                services.AddScoped<INavigationWindow, Views.Windows.MainWindow>();
                services.AddScoped<ViewModels.MainWindowViewModel>();

                // dependency injections
                services.AddScoped<IAudioService, AudioService>();

                // Views and ViewModels
                services.AddScoped<Views.Pages.ChatAIPage>();
                services.AddScoped<ViewModels.ChatAIViewModel>();
                services.AddScoped<Views.Pages.SettingsPage>();
                services.AddScoped<ViewModels.SettingsViewModel>();

                // Configuration
                services.Configure<AppConfig>(context.Configuration.GetSection(nameof(AppConfig)));
            })
            .ConfigureLogging(logBuilder =>
            {
                logBuilder.SetMinimumLevel(LogLevel.Information);
                logBuilder.AddNLog("NLog.config");
            }).Build();

        /// <summary>
        /// Gets registered service.
        /// </summary>
        /// <typeparam name="T">Type of the service to get.</typeparam>
        /// <returns>Instance of the service or <see langword="null"/>.</returns>
        public static T GetService<T>()
            where T : class
        {
            return _host.Services.GetService(typeof(T)) as T;
        }

        /// <summary>
        /// Occurs when the application is loading.
        /// </summary>
        private async void OnStartup(object sender, StartupEventArgs e)
        {
1            // configure Azure environment variables.
            Environment.SetEnvironmentVariable("AZURE_CLIENT_ID", ChatAIFluentWpf.Properties.Settings.Default.AzureClientID);
            Environment.SetEnvironmentVariable("AZURE_TENANT_ID", ChatAIFluentWpf.Properties.Settings.Default.AzureTenantID);
            Environment.SetEnvironmentVariable("AZURE_CLIENT_SECRET", ChatAIFluentWpf.Properties.Settings.Default.AzureClientSecret);
            await _host.StartAsync();
        }

        /// <summary>
        /// Occurs when the application is closing.
        /// </summary>
        private async void OnExit(object sender, ExitEventArgs e)
        {
            await _host.StopAsync();

            _host.Dispose();
        }

        /// <summary>
        /// Occurs when an exception is thrown by an application but not handled.
        /// </summary>
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // For more info see https://docs.microsoft.com/en-us/dotnet/api/system.windows.application.dispatcherunhandledexception?view=windowsdesktop-6.0
        }
    }
}