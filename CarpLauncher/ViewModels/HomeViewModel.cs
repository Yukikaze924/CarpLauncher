using CarpLauncher.Contracts.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CarpLauncher.ViewModels;

public partial class HomeViewModel : ObservableRecipient
{
    private readonly ILocalSettingsService _localSettingsService;

    [ObservableProperty]
    private string _backgroundImageUrl;

    public HomeViewModel(ILocalSettingsService localSettingsService)
    {
        _localSettingsService = localSettingsService;

        Task.Run(InitializeAsync);
    }

    public async Task InitializeAsync()
    {
        //var localSettingsService = App.GetService<ILocalSettingsService>();

        BackgroundImageUrl = await _localSettingsService.ReadSettingAsync<string>("BackgroundImageUrl");
    }
}
