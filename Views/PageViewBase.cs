using iSing.ViewModels;
namespace iSing.Views;

// Basis aller XAML-Seiten: reicht nur das Lebenszyklus-Ereignis weiter.
// Laden, Fehlerbehandlung und Zustand liegen im ViewModel, Layout und Zustandsanzeige in XAML.
public abstract class PageViewBase : ContentPage
{
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is PageViewModel viewModel) await viewModel.InitializeAsync();
    }
}
