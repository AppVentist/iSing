using iSing.Models;
using iSing.Services;
namespace iSing.ViewModels;

public sealed record VerseItem(string Heading, string Text, double FontSize);
public sealed class SongDetailViewModel : PageViewModel
{
    Song song;
    List<Song> bookSongs = [];
    int index = -1;
    readonly ISettingsService settings;
    IReadOnlyList<VerseItem> verses = [];
    public string SongTitle => song.Title;
    public string BookLabel => song.BookLabel;
    public string Category => song.CategoryName;
    public bool IsFavorite => Library.Favorites.Contains(song.Key);
    public string FavoriteGlyph => IsFavorite ? "♥" : "♡";
    public bool CanGoPrevious => index > 0;
    public bool CanGoNext => index >= 0 && index < bookSongs.Count - 1;
    public IReadOnlyList<VerseItem> Verses { get => verses; private set => Set(ref verses, value); }
    public bool HasNoVerses => Verses.Count == 0;
    public RelayCommand ToggleFavoriteCommand { get; }
    public RelayCommand PreviousCommand { get; }
    public RelayCommand NextCommand { get; }
    public SongDetailViewModel(ILibraryService library, ISettingsService settings, Song song) : base($"Lied {song.Number}", library)
    {
        this.song = song;
        this.settings = settings;
        ToggleFavoriteCommand = new(_ =>
        {
            try { Library.Toggle(this.song); Notify(nameof(IsFavorite)); Notify(nameof(FavoriteGlyph)); }
            catch (Exception ex) { ShowError(ex); }
        });
        PreviousCommand = new(_ => GoTo(index - 1));
        NextCommand = new(_ => GoTo(index + 1));
    }

    public class VerseItem
    {
        public VerseItem(string heading, string text, double fontSize, string? refrainText = null)
        {
            Heading = heading;
            Text = text;
            FontSize = fontSize;
            RefrainText = refrainText;
        }

        public string Heading { get; }
        public string Text { get; }
        public double FontSize { get; }
        public string? RefrainText { get; set; }
        public bool HasRefrain => !string.IsNullOrEmpty(RefrainText);
    }
    // Blättert innerhalb des gleichen Liederbuchs zum Nachbarlied.
    void GoTo(int newIndex)
    {
        if (newIndex < 0 || newIndex >= bookSongs.Count) return;
        song = bookSongs[newIndex];
        Library.Visit(song);
        Title = $"Lied {song.Number}";
        Refresh();
    }
    protected override void Refresh()
    {
        bookSongs = Library.Songs.Where(s => s.BookId == song.BookId).ToList();
        index = bookSongs.IndexOf(song);
        var items = new List<VerseItem>();
        Verse? refrain = null;

        foreach (var v in song.Verses)
        {
            if (v.IsRefrain)
            {
                refrain = v;
                if (items.Count == 0)
                    items.Add(new VerseItem("Refrain", v.Text, settings.FontSize)); // Refrain steht am Anfang
                else
                    items[^1].RefrainText = v.Text; // an die vorherige Strophe anhängen
                continue;
            }

            items.Add(new VerseItem(
                v.Number.HasValue ? $"{v.Number}. Strophe" : "Strophe",
                v.Text, settings.FontSize, refrain?.Text));
        }

        Verses = items;
        Notify(nameof(HasNoVerses));
        Notify(nameof(SongTitle));
        Notify(nameof(BookLabel));
        Notify(nameof(Category));
        Notify(nameof(IsFavorite));
        Notify(nameof(FavoriteGlyph));
        Notify(nameof(CanGoPrevious));
        Notify(nameof(CanGoNext));
    }
}
