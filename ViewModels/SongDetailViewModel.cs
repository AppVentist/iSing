using iSing.Models;
using iSing.Services;
namespace iSing.ViewModels;

public sealed record VerseItem(string Heading, string Text, double FontSize);
public sealed class SongDetailViewModel : PageViewModel
{
    readonly Song song;
    readonly ISettingsService settings;
    IReadOnlyList<VerseItem> verses = [];
    public string SongTitle => song.Title;
    public string BookLabel => song.BookLabel;
    public string Category => song.CategoryName;
    public bool IsFavorite => Library.Favorites.Contains(song.Key);
    public string FavoriteText => IsFavorite ? "♥  Aus Favoriten entfernen" : "♡  Als Favorit speichern";
    public IReadOnlyList<VerseItem> Verses { get => verses; private set => Set(ref verses, value); }
    public bool HasNoVerses => Verses.Count == 0;
    public RelayCommand ToggleFavoriteCommand { get; }
    public SongDetailViewModel(ILibraryService library, ISettingsService settings, Song song) : base($"Lied {song.Number}", library)
    {
        this.song = song;
        this.settings = settings;
        ToggleFavoriteCommand = new(_ =>
        {
            try { Library.Toggle(song); Notify(nameof(IsFavorite)); Notify(nameof(FavoriteText)); }
            catch (Exception ex) { ShowError(ex); }
        });
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
    protected override void Refresh()
    {
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
        Notify(nameof(IsFavorite));
        Notify(nameof(FavoriteText));
    }
}
