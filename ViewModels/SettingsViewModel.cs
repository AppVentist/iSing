using iSing.Services;
namespace iSing.ViewModels;

public sealed record BookItem(string Label, int Year, int Count)
{
    public string Caption => $"{Year} · {Count} {(Count == 1 ? "Lied" : "Lieder")}";
}
public sealed class SettingsViewModel : PageViewModel
{
    readonly ISettingsService settings;
    static readonly double[] sizes = [18, 22, 27];
    int fontSizeIndex, designIndex;
    bool booksExpanded;
    IReadOnlyList<BookItem> books = [];
    public IReadOnlyList<string> FontSizes { get; } = ["Normal", "Groß", "Sehr groß"];
    public IReadOnlyList<string> Designs { get; } = ["System", "Hell", "Dunkel"];
    public IReadOnlyList<BookItem> Books { get => books; private set { if (Set(ref books, value)) Notify(nameof(BooksSummary)); } }
    // Kopfzeile der aufklappbaren Liederbuch-Karte.
    public string BooksSummary
    {
        get
        {
            var songs = Books.Sum(b => b.Count);
            return $"{Books.Count} {(Books.Count == 1 ? "Liederbuch" : "Liederbücher")} · {songs} {(songs == 1 ? "Lied" : "Lieder")}";
        }
    }
    public bool IsBooksExpanded
    {
        get => booksExpanded;
        set { if (Set(ref booksExpanded, value)) Notify(nameof(BooksChevronRotation)); }
    }
    public double BooksChevronRotation => IsBooksExpanded ? -90 : 90;
    public RelayCommand ToggleBooksCommand { get; }
    public int FontSizeIndex
    {
        get => fontSizeIndex;
        set
        {
            if (value < 0 || value >= sizes.Length || value == fontSizeIndex) return;
            try { settings.FontSize = sizes[value]; Set(ref fontSizeIndex, value); }
            catch (Exception ex) { ShowError(ex); }
        }
    }
    public int DesignIndex
    {
        get => designIndex;
        set
        {
            if (value < 0 || value >= Designs.Count || value == designIndex) return;
            try { settings.Design = (AppDesign)value; Set(ref designIndex, value); }
            catch (Exception ex) { ShowError(ex); }
        }
    }
    public SettingsViewModel(ILibraryService library, ISettingsService settings) : base("Einstellungen", library)
    {
        this.settings = settings;
        ToggleBooksCommand = new(_ => IsBooksExpanded = !IsBooksExpanded);
    }
    protected override void Refresh()
    {
        Books = Library.Books.Select(b => new BookItem(b.Label, b.Year, b.Songs.Count)).ToList();
        fontSizeIndex = Math.Max(0, Array.IndexOf(sizes, settings.FontSize));
        designIndex = (int)settings.Design;
        Notify(nameof(FontSizeIndex));
        Notify(nameof(DesignIndex));
    }
    // VersionText = ApplicationDisplayVersion -> 1.0.0
    // BuildString = ApplicationVersion -> "1" > mit jedem Upload muss dieser Wert steigen
    public string VersionText => $"Version: {AppInfo.Current.VersionString} ({AppInfo.Current.BuildString})";
    public string CopyrightText => $"© {DateTime.Now.Year}";
}
