using System.Text.Json;
using iSing.Models;
namespace iSing.Services;

public sealed class LibraryService : ILibraryService
{
    readonly IPreferencesStore preferences;
    readonly IAssetReader assets;
    readonly HashSet<string> favorites;
    readonly List<string> recent;
    public LibraryService(IPreferencesStore preferences, IAssetReader assets)
    {
        this.preferences = preferences;
        this.assets = assets;
        favorites = ReadState("favorites.v2", "favorites").ToHashSet();
        recent = ReadState("recent.v2", "recent").Distinct().Take(30).ToList();
    }
    readonly SemaphoreSlim gate = new(1, 1);
    public IReadOnlyList<SongBook> Books { get; private set; } = [];
    public IReadOnlyList<Song> Songs { get; private set; } = [];
    public bool IsLoaded { get; private set; }
    public IReadOnlySet<string> Favorites => favorites;
    public IReadOnlyList<string> Recent => recent;
    List<string> ReadState(string key, string legacyKey)
    {
        try
        {
            var saved = preferences.Get(key, "");
            if (saved.Length > 0) return JsonSerializer.Deserialize<List<string>>(saved) ?? [];
            var legacy = JsonSerializer.Deserialize<List<string>>(preferences.Get(legacyKey, "[]")) ?? [];
            return legacy.Select(number => Song.MakeKey("wir_loben_gott_1998", number)).ToList();
        }
        catch (JsonException) { return []; }
    }
    public async Task LoadAsync()
    {
        await gate.WaitAsync();
        try
        {
            if (IsLoaded) return;
            using var manifest = await assets.OpenAsync("songbooks.json");
            var files = await JsonSerializer.DeserializeAsync<List<string>>(manifest)
                ?? throw new InvalidDataException("Die Liederbuchliste ist leer.");
            if (files.Count == 0) throw new InvalidDataException("Die Liederbuchliste ist leer.");
            var books = new List<SongBook>();
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var file in files)
            {
                try
                {
                    using var stream = await assets.OpenAsync(file);
                    var book = await JsonSerializer.DeserializeAsync<SongBook>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                        ?? throw new InvalidDataException("Das Liederbuch ist leer.");
                    if (string.IsNullOrWhiteSpace(book.Id) || string.IsNullOrWhiteSpace(book.Title))
                        throw new InvalidDataException("Buch-ID und Titel müssen angegeben sein.");
                    if (!ids.Add(book.Id)) throw new InvalidDataException($"Die Buch-ID '{book.Id}' ist mehrfach vergeben.");
                    if (book.Songs == null) throw new InvalidDataException("Die Liederliste darf nicht null sein.");
                    if (book.Categories == null) throw new InvalidDataException("Die Kategorienliste darf nicht null sein.");
                    // Kategorie-Ids sind nur innerhalb ihres Buchs eindeutig, deshalb je Buch ein eigenes Verzeichnis.
                    var categories = new Dictionary<string, SongCategory>(StringComparer.Ordinal);
                    foreach (var category in book.Categories)
                    {
                        if (category == null || string.IsNullOrWhiteSpace(category.Id) || string.IsNullOrWhiteSpace(category.Name))
                            throw new InvalidDataException("Jede Kategorie benötigt eine Id und einen Namen.");
                        if (!categories.TryAdd(category.Id, category))
                            throw new InvalidDataException($"Die Kategorie-Id '{category.Id}' ist innerhalb des Buchs doppelt.");
                    }
                    var numbers = new HashSet<string>(StringComparer.Ordinal);
                    foreach (var song in book.Songs)
                    {
                        if (song == null || string.IsNullOrWhiteSpace(song.Number) || string.IsNullOrWhiteSpace(song.Title))
                            throw new InvalidDataException("Jedes Lied benötigt eine Nummer und einen Titel.");
                        if (!numbers.Add(song.Number)) throw new InvalidDataException($"Liednummer '{song.Number}' ist innerhalb des Buchs doppelt.");
                        if (song.Verses == null || song.Verses.Any(v => v == null || v.Text == null))
                            throw new InvalidDataException($"Ungültige Strophen bei Lied '{song.Number}'.");
                        song.CategoryId ??= "";
                        song.BookId = book.Id;
                        song.BookLabel = book.Label;
                        // Ein unbekannter Verweis ist kein harter Fehler: er würde sonst das ganze Buch unbrauchbar machen.
                        song.CategoryName = categories.TryGetValue(song.CategoryId, out var match) ? match.Name : Song.WithoutCategory;
                    }
                    books.Add(book);
                }
                catch (Exception ex) when (ex is JsonException or InvalidDataException or IOException)
                {
                    throw new InvalidDataException($"{file}: {ex.Message}", ex);
                }
            }
            Books = books;
            Songs = books.SelectMany(book => book.Songs).ToList();
            IsLoaded = true;
        }
        finally { gate.Release(); }
    }
    public void Toggle(Song song)
    {
        if (!favorites.Remove(song.Key)) favorites.Add(song.Key);
        preferences.Set("favorites.v2", JsonSerializer.Serialize(Favorites));
    }
    public void Visit(Song song)
    {
        recent.Remove(song.Key);
        recent.Insert(0, song.Key);
        if (recent.Count > 30) recent.RemoveRange(30, recent.Count - 30);
        preferences.Set("recent.v2", JsonSerializer.Serialize(Recent));
    }
    public void RemoveRecent(Song song)
    {
        if (!recent.Remove(song.Key)) return;
        preferences.Set("recent.v2", JsonSerializer.Serialize(Recent));
    }
    public IEnumerable<Song> Search(IEnumerable<Song> songs, string? query)
    {
        var terms = (query ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return songs.Where(s => terms.All(t =>
            s.Number.Contains(t, StringComparison.OrdinalIgnoreCase) ||
            (int.TryParse(t, out var n) && int.TryParse(s.Number, out var sn) && n == sn) ||
            s.Title.Contains(t, StringComparison.OrdinalIgnoreCase)));
    }
}

