using CarpLauncher.Contracts.Services;
using CarpLauncher.Core;
using CarpLauncher.Helpers;
using CarpLauncher.ViewModels;
using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using ProjBobcat.Class.Model;
using ProjBobcat.Class.Model.LauncherProfile;

namespace CarpLauncher.Views;

public sealed partial class GamePage : Page
{
    public GameViewModel ViewModel
    {
        get;
    }

    public GamePage()
    {
        ViewModel = App.GetService<GameViewModel>();
        InitializeComponent();
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MinecraftPanel is null || ForgePanel is null || FabricPanel is null) { return; }

        // Hide all panels
        GamePanel.Visibility = Visibility.Collapsed;
        MinecraftPanel.Visibility = Visibility.Collapsed;
        ForgePanel.Visibility = Visibility.Collapsed;
        FabricPanel.Visibility = Visibility.Collapsed;

        // Show the selected panel
        switch (((Segmented)sender).SelectedIndex)
        {
            case 0:
                GamePanel.Visibility = Visibility.Visible;
                break;
            case 1:
                MinecraftPanel.Visibility = Visibility.Visible;
                break;
            case 2:
                ForgePanel.Visibility = Visibility.Visible;
                break;
            case 3:
                FabricPanel.Visibility = Visibility.Visible;
                break;
        }
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        Segmented.SelectedIndex = 1;
    }


    private void SettingsCard_Click(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is SettingsCard card && card.DataContext is VersionInfo version)
        {
            var name = version.Name;
            App.GetService<HomeViewModel>().CurrentSelectedVersion = name;
            App.GetService<INavigationService>().NavigateTo(typeof(HomeViewModel).FullName!);
        }
    }

    private void Context_Click(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is AppBarButton button)
        {
            var version = (VersionInfo)currentItem!.DataContext;
            var label = button.Label;
            switch (label)
            {
                case "Choose":
                    {
                        App.GetService<HomeViewModel>().CurrentSelectedVersion = version.Name;
                        App.GetService<INavigationService>().NavigateTo(typeof(HomeViewModel).FullName!);
                        break;
                    }

                case "Edit":
                    {

                        break;
                    }

                case "Delete":
                    try
                    {
                        App.TaskInvoker(() =>
                        {
                            Directory.Delete($@"{Core.Core.GetGameCore().RootPath}\versions\{version.Name}", true);
                            GameHelper.RemoveGameProfile(version.Name);
                        });
                        App.GetService<GameViewModel>().FetchProfiles();
                    }
                    catch
                    {
                        _ = DialogHelper.ShowRegularContentDialogAsync("Error", "Unable to delete this profile!");
                        return;
                    }
                    break;
            }

        }
    }

    private Control? currentItem;

    private void SettingsCard_ContextRequested(UIElement sender, Microsoft.UI.Xaml.Input.ContextRequestedEventArgs args)
    {
        if (sender is SettingsCard item)
        {
            ShowMenu(item, true);
            currentItem = item;
        }
    }
    private void ShowMenu(Control item, bool isTransient)
    {
        FlyoutShowOptions myOption = new();
        myOption.ShowMode = isTransient ? FlyoutShowMode.Transient : FlyoutShowMode.Standard;
        CommandBarFlyout1.ShowAt(item, myOption);
    }
}
