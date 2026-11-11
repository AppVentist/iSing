using iSing.ViewModels;
namespace iSing.Views;

public partial class HomePage : PageViewBase
{
    public HomeViewModel ViewModel { get; }
    public HomePage(HomeViewModel viewModel)
    {
        ViewModel = viewModel;
        BindingContext = viewModel;
        InitializeComponent();
    }
}
