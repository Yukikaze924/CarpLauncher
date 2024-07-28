using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CarpLauncher.Controls
{
    public sealed partial class ProgressDialog : Page
    {
        public ProgressDialog(bool isIndeterminate = true)
        {
            this.InitializeComponent();

            ProgressBar.IsIndeterminate = isIndeterminate;

            ProgressInvoker += (sender, progress) =>
            {
                ProgressBar.Value = progress;
            };
        }

        private event EventHandler<double>? ProgressInvoker;

        public void ReportProgress(double dParam_0)
        {
            ProgressInvoker?.Invoke(null, dParam_0);
        }

        public void IsProgressIndeterminate(bool bParam_0) => ProgressBar.IsIndeterminate = bParam_0;
    }
}
