using iSing.ViewModels;
namespace iSing.Views;

public partial class CategoriesPage : PageViewBase
{
    public CategoriesViewModel ViewModel { get; }
    public CategoriesPage(CategoriesViewModel viewModel)
    {
        ViewModel = viewModel;
        BindingContext = viewModel;
        InitializeComponent();
    }
}
