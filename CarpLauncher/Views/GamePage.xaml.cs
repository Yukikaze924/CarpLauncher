using CarpLauncher.ViewModels;
using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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
}
