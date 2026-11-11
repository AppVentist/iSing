using iSing.Services;
namespace iSing.ViewModels;

public sealed class HomeViewModel : PageViewModel
{
    public string BookSummary => $"{Library.Books.Count} Liederbücher · iSing";
    public string SongSummary => $"{Library.Songs.Count} Lieder aus allen Büchern";
    public AsyncCommand ShowSongsCommand { get; }
    public AsyncCommand ShowBooksCommand { get; }
    public AsyncCommand ShowCategoriesCommand { get; }
    public AsyncCommand ShowFavoritesCommand { get; }
    public AsyncCommand ShowRecentCommand { get; }
    public HomeViewModel(ILibraryService library, INavigationService navigation) : base("Meine Liederbücher", library)
    {
        ShowSongsCommand = new(_ => navigation.ShowSongsAsync(new("Alle Lieder")), ShowError);
        ShowBooksCommand = new(_ => navigation.ShowBooksAsync(), ShowError);
        ShowCategoriesCommand = new(_ => navigation.ShowCategoriesAsync(), ShowError);
        ShowFavoritesCommand = new(_ => navigation.ShowSongsAsync(new("Favoriten", SongListKind.Favorites)), ShowError);
        ShowRecentCommand = new(_ => navigation.ShowSongsAsync(new("Letzte Lieder", SongListKind.Recent)), ShowError);
    }
    protected override void Refresh() { Notify(nameof(BookSummary)); Notify(nameof(SongSummary)); }
}
