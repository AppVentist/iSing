using iSing.ViewModels;
namespace iSing.Views;

public partial class BooksPage : PageViewBase
{
    public BooksViewModel ViewModel { get; }
    public BooksPage(BooksViewModel viewModel)
    {
        ViewModel = viewModel;
        BindingContext = viewModel;
        InitializeComponent();
    }
}
