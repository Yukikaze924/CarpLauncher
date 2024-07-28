using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using CarpLauncher.Contracts.Services;

namespace CarpLauncher.Helpers
{
    public class DialogHelper
    {
        public static async Task<ContentDialogResult> ShowRegularContentDialogAsync
        (string title, string content, string primaryBtnText = "Confirm", string closeBtnText = "Close")
        {
            ContentDialog dialog = new ContentDialog();

            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            dialog.XamlRoot = App.MainWindow.Content.XamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = title;
            dialog.PrimaryButtonText = primaryBtnText;
            dialog.CloseButtonText = closeBtnText;
            dialog.DefaultButton = ContentDialogButton.Primary;
            dialog.Content = new Controls.ContentDialog(content);
            dialog.RequestedTheme = App.GetService<IThemeSelectorService>().Theme;

            var result = await dialog.ShowAsync();

            return result;
        }

        public static ContentDialog GetIndeterminateProgressDialog(string title, string closeBtnText = "Close")
        {
            var dialog = new ContentDialog
            {
                XamlRoot = App.MainWindow.Content.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = title,
                IsPrimaryButtonEnabled = false,
                CloseButtonText = closeBtnText,
                Content = new Controls.ProgressDialog(),
                RequestedTheme = App.GetService<IThemeSelectorService>().Theme
            };

            return dialog;
        }

        public static ContentDialog GetProgressDialog(string title, string closeBtnText = "Close")
        {
            return new ContentDialog
            {
                XamlRoot = App.MainWindow.Content.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = title,
                IsPrimaryButtonEnabled = false,
                CloseButtonText = closeBtnText,
                Content = new Controls.ProgressDialog(false),
                RequestedTheme = App.GetService<IThemeSelectorService>().Theme
            }; ;
        }
    }
}
