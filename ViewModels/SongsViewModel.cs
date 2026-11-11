using iSing.Models;
using iSing.Services;
namespace iSing.ViewModels;

public sealed record BookFilterItem(string? Id, string Name);
public sealed class SongsViewModel : PageViewModel
{
    static readonly BookFilterItem allBooks = new(null, "Alle Liederbücher");
    readonly SongListOptions options;
    string query = "";
    bool filterReady;
    IReadOnlyList<Song> songs = [];
    IReadOnlyList<BookFilterItem> bookFilters = [allBooks];
    BookFilterItem selectedBookFilter = allBooks;
    Song? selectedSong;
    public string Query { get => query; set { if (Set(ref query, value ?? "")) Refresh(); } }
    public IReadOnlyList<Song> Songs { get => songs; private set => Set(ref songs, value); }
    public Song? SelectedSong { get => selectedSong; set => Set(ref selectedSong, value); }
    // Der Filter enthält "Alle Liederbücher" und jedes Buch, das in dieser Liste überhaupt vorkommt.
    public IReadOnlyList<BookFilterItem> BookFilters { get => bookFilters; private set => Set(ref bookFilters, value); }
    public bool HasBookFilter => BookFilters.Count > 2;
    public BookFilterItem? SelectedBookFilter
    {
        get => selectedBookFilter;
        set { if (Set(ref selectedBookFilter, value ?? allBooks)) Refresh(); }
    }
    public string ResultSummary
    {
        get
        {
            var books = Songs.Select(s => s.BookId).Distinct().Count();
            return $"{Songs.Count} {(Songs.Count == 1 ? "Lied" : "Lieder")} · {books} {(books == 1 ? "Liederbuch" : "Liederbücher")}";
        }
    }
    public string EmptyMessage => selectedBookFilter.Id != null
        ? "Keine passenden Lieder in diesem Liederbuch."
        : options.Kind switch
        {
            SongListKind.Favorites => "Noch keine Favoriten erfasst.\nSpeichere Lieder über das Herz auf der Liedseite.",
            SongListKind.Recent => "Hier erscheinen deine zuletzt geöffneten Lieder.",
            _ => "Keine Lieder gefunden."
        };
    public AsyncCommand OpenSongCommand { get; }
    // Das X zum Entfernen erscheint nur in "Letzte Lieder".
    public bool CanRemoveRecent => options.Kind == SongListKind.Recent;
    public RelayCommand RemoveRecentCommand { get; }
    public SongsViewModel(ILibraryService library, INavigationService navigation, SongListOptions options) : base(options.Title, library)
    {
        this.options = options;
        OpenSongCommand = new(async parameter =>
        {
            var song = parameter as Song;
            SelectedSong = null;
            if (song == null) return;
            await navigation.ShowSongAsync(song);
            Library.Visit(song);
        }, ShowError);
        RemoveRecentCommand = new(parameter =>
        {
            if (parameter is not Song song || !CanRemoveRecent) return;
            Library.RemoveRecent(song);
            Refresh();
        });
    }
    protected override void Refresh()
    {
        if (!Library.IsLoaded) return;
        IEnumerable<Song> source = Library.Songs;
        if (options.Kind == SongListKind.Favorites) source = source.Where(s => Library.Favorites.Contains(s.Key));
        if (options.Kind == SongListKind.Recent)
        {
            var order = Library.Recent.Select((key, index) => (key, index)).ToDictionary(x => x.key, x => x.index);
            source = source.Where(s => order.ContainsKey(s.Key)).OrderBy(s => order[s.Key]);
        }
        // Eine Kategorie gehört immer zu genau einem Buch; ohne die BookId würden gleichnamige Ids anderer Bücher mitkommen.
        if (options.Kind == SongListKind.Category)
            source = source.Where(s => s.CategoryId == options.CategoryId && (options.BookId == null || s.BookId == options.BookId));
        var scope = source as IReadOnlyList<Song> ?? source.ToList();
        UpdateBookFilters(scope);
        IEnumerable<Song> filtered = scope;
        if (selectedBookFilter.Id is { } bookId) filtered = filtered.Where(s => s.BookId == bookId);
        Songs = Library.Search(filtered, Query).ToList();
        Notify(nameof(ResultSummary));
        Notify(nameof(EmptyMessage));
    }
    // Die Filterliste wird nur neu aufgebaut, wenn sich die enthaltenen Bücher ändern.
    // Sonst würde jede Eingabe im Suchfeld die Auswahl des Nutzers verwerfen.
    void UpdateBookFilters(IReadOnlyList<Song> scope)
    {
        var ids = scope.Select(s => s.BookId).ToHashSet();
        var items = new List<BookFilterItem> { allBooks };
        items.AddRange(Library.Books.Where(book => ids.Contains(book.Id)).Select(book => new BookFilterItem(book.Id, book.Title)));
        if (BookFilters.Select(item => item.Id).SequenceEqual(items.Select(item => item.Id))) return;
        BookFilters = items;
        var wanted = filterReady ? selectedBookFilter.Id : options.BookId;
        filterReady = true;
        selectedBookFilter = items.FirstOrDefault(item => item.Id == wanted) ?? allBooks;
        Notify(nameof(SelectedBookFilter));
        Notify(nameof(HasBookFilter));
    }
}
