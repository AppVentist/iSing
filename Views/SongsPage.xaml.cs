using iSing.ViewModels;
namespace iSing.Views;

public partial class SongsPage : PageViewBase
{
    public SongsViewModel ViewModel { get; }
    public SongsPage(SongsViewModel viewModel)
    {
        ViewModel = viewModel;
        BindingContext = viewModel;
        InitializeComponent();
    }
}
