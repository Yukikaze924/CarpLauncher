﻿using CarpLauncher.Activation;
using CarpLauncher.Contracts.Services;
using CarpLauncher.Core.Contracts.Services;
using CarpLauncher.Core.Services;
using CarpLauncher.Models;
using CarpLauncher.Services;
using CarpLauncher.ViewModels;
using CarpLauncher.Views;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using System.Net;

namespace CarpLauncher;

// To learn more about WinUI 3, see https://docs.microsoft.com/windows/apps/winui/winui3/.
public partial class App : Application
{
    // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    public IHost Host
    {
        get;
    }

    public static T GetService<T>()
        where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    public static WindowEx MainWindow { get; } = new MainWindow();

    public static UIElement? AppTitlebar
    {
        get; set;
    }

    public App()
    {
        InitializeComponent();

        Host = Microsoft.Extensions.Hosting.Host.
        CreateDefaultBuilder().
        UseContentRoot(AppContext.BaseDirectory).
        ConfigureServices((context, services) =>
        {
            // Default Activation Handler
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Other Activation Handlers

            // Services
            services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
            services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
            services.AddTransient<INavigationViewService, NavigationViewService>();

            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();

            // Core Services
            services.AddSingleton<IFileService, FileService>();

            // Views and ViewModels
            services.AddSingleton<GameViewModel>();
            services.AddSingleton<GamePage>();
            //
            services.AddSingleton<SettingsViewModel>();
            services.AddTransient<SettingsPage>();
            //
            services.AddSingleton<HomeViewModel>();
            services.AddTransient<HomePage>();
            //
            services.AddTransient<ShellPage>();
            services.AddTransient<ShellViewModel>();

            // Configuration
            services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
        }).
        Build();

        UnhandledException += App_UnhandledException;
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // TODO: Log and handle exceptions as appropriate.
        // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        await GetService<IActivationService>().ActivateAsync(args);

        ServicePointManager.DefaultConnectionLimit = 512;

        await InitServicesAsync();
        InitFileSystemWatcher();
        //GetService<GameViewModel>().Profiles = Core.GameHelper.GetAllGames([]);
    }

    public DispatcherQueue DispatcherQueue { get; } = DispatcherQueue.GetForCurrentThread();

    private async Task InitServicesAsync()
    {
        var _localSettingsService = GetService<ILocalSettingsService>();

        var rootPath
            = await _localSettingsService.ReadSettingAsync<string>("GameRootPath")
            ?? $@"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\.minecraft";
        await _localSettingsService.SaveSettingAsync("GameRootPath", rootPath);

        // 潜在导致debugger问题
        var isVersionIsolate = await _localSettingsService.ReadSettingAsync<bool>("IsVersionIsolate");
        if (!isVersionIsolate)
        {
            await _localSettingsService.SaveSettingAsync("IsVersionIsolate", true);
        }

        var backgroundImageUrl = await _localSettingsService.ReadSettingAsync<string>("BackgroundImageUrl");
        if (string.IsNullOrEmpty(backgroundImageUrl))
        {
            await _localSettingsService.SaveSettingAsync("BackgroundImageUrl", "/");
        }

        await Core.Core.InitLaunchCore(await _localSettingsService.ReadSettingAsync<string>("GameRootPath") ?? ".minecraft");
    }

    public static FileSystemWatcher watcher = new();

    private void InitFileSystemWatcher()
    {
        watcher.Path = Core.Core.GetGameCore().RootPath + "\\versions";
        watcher.IncludeSubdirectories = true;
        watcher.NotifyFilter = NotifyFilters.DirectoryName;
        watcher.Deleted += (sender, args) =>
        {
            var dispatcherQueue = (App.Current as App).DispatcherQueue;
            dispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    GetService<GameViewModel>().FetchProfiles();
                }
                catch { return; }
            });
        };
        watcher.Created += (sender, args) =>
        {
            var dispatcherQueue = (App.Current as App).DispatcherQueue;
            dispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    GetService<GameViewModel>().FetchProfiles();
                }
                catch { return; }
            });
        };
        watcher.EnableRaisingEvents = true;
    }

    public static void TaskInvoker(Action task)
    {
        watcher.EnableRaisingEvents = false;

        if (!watcher.EnableRaisingEvents)
        {
            task.Invoke();
        }

        watcher.EnableRaisingEvents = true;
    }
}
