using iSing.ViewModels;
namespace iSing.Views;

public partial class SettingsPage : PageViewBase
{
    public SettingsViewModel ViewModel { get; }
    public SettingsPage(SettingsViewModel viewModel)
    {
        ViewModel = viewModel;
        BindingContext = viewModel;
        InitializeComponent();
    }
}
