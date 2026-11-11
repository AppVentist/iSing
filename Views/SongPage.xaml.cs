using iSing.ViewModels;
namespace iSing.Views;

public partial class SongPage : PageViewBase
{
    public SongDetailViewModel ViewModel { get; }
    public SongPage(SongDetailViewModel viewModel)
    {
        ViewModel = viewModel;
        BindingContext = viewModel;
        InitializeComponent();
    }
}
