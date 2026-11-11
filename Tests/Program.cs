using System.Text;
using System.Text.Json;
using iSing.Models;
using iSing.Services;
using iSing.ViewModels;

var preferences = new MemoryPreferences();
var assets = new TestAssets();
var checks = 0;
void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
    checks++;
    Console.WriteLine("PASS: " + message);
}
preferences.Set("favorites", "[\"002\"]");
preferences.Set("recent", "[\"002\",\"004\"]");
var library = new LibraryService(preferences, assets);
await Task.WhenAll(library.LoadAsync(), library.LoadAsync());
var songs = library.Songs;
var original = library.Books.Single(b => b.Id == "wir_loben_gott_1998");
Check(library.Books.Count == 5 && songs.Count == 559, "5 Bücher und 559 Lieder geladen, auch bei parallelem Laden");
Check(original.Songs.Count == 547, "Originalbuch weiterhin mit 547 Liedern");
Check(library.Books.Count(b => b.IsDemo && b.Songs.Count == 3) == 4, "4 Dummies mit je 3 Liedern");
Check(songs.Select(s => s.Key).Distinct().Count() == songs.Count, "Buchübergreifend eindeutige Liedkennungen");
Check(songs.All(s => s.Verses.Count > 0 && !string.IsNullOrWhiteSpace(s.BookLabel)), "Alle Lieder mit Strophen und Buchlabel");
Check(songs.Select(s => s.CategoryId).Distinct().Count() == 4, "Kategorien aller Bücher zusammengeführt");
Check(library.Search(songs, "2").Any(s => s.BookId == original.Id && s.Number == "002"), "Liednummer ohne führende Nullen");
Check(library.Search(songs, "Lobe mächtigen").Any(s => s.BookId == original.Id && s.Number == "002"), "Mehrwortsuche mit Umlauten");
Check(!library.Search(songs, "Adelers Fittichen").Any(), "Strophentext wird nicht mehr durchsucht");
Check(!library.Search(songs, "xyzxyzxyz").Any(), "Keine Treffer für unbekannten Suchtext");
Check(library.Search(songs, "002").Select(s => s.BookId).Distinct().Count() == 5, "Gleiche Nummer in allen fünf Büchern auffindbar");
Check(library.Search(songs, "Gemeinsam singen").Where(s => s.BookId != original.Id).Select(s => s.BookId).Distinct().Count() == 4, "Gemeinsame Suche findet alle vier Dummies");
Check(!library.Search(songs, "Kinderlieder").Any(s => !s.Title.Contains("Kinderlieder", StringComparison.OrdinalIgnoreCase)), "Buchtitel wird nicht mehr durchsucht");
var song = original.Songs.First(s => s.Number == "002");
var demo = library.Books.First(b => b.IsDemo).Songs.Single(s => s.Number == "002");
Check(library.Favorites.SetEquals([song.Key]) && library.Recent[0] == song.Key, "Alte Favoriten und Verlauf auf ursprüngliches Buch migriert");
library.Toggle(demo);
Check(library.Favorites.Contains(song.Key) && library.Favorites.Contains(demo.Key), "Gleiche Liednummer separat favorisiert");
library.Toggle(song);
Check(!library.Favorites.Contains(song.Key) && library.Favorites.Contains(demo.Key), "Entfernen eines Favoriten lässt anderes Buch unverändert");
Check(new LibraryService(preferences, assets).Favorites.SetEquals([demo.Key]), "Neue Favoriten gespeichert, entfernter Alt-Favorit wird nicht erneut migriert");
library.Visit(song);
library.Visit(demo);
Check(library.Recent[0] == demo.Key && library.Recent[1] == song.Key, "Verlauf unterscheidet gleiche Nummern aus verschiedenen Büchern");
foreach (var item in songs.Take(35)) library.Visit(item);
library.Visit(song);
Check(library.Recent.Count == 30 && library.Recent[0] == song.Key && library.Recent.Distinct().Count() == 30, "Verlauf begrenzt, eindeutig und korrekt sortiert");

async Task Reject(string filename, string content, string expected)
{
    assets.Overrides[filename] = content;
    var candidate = new LibraryService(preferences, assets);
    try
    {
        await candidate.LoadAsync();
        throw new Exception("Ungültige Daten wurden akzeptiert: " + expected);
    }
    catch (InvalidDataException ex)
    {
        Check(ex.Message.Contains(expected) && !candidate.IsLoaded && candidate.Songs.Count == 0, expected + ": verständlicher Fehler ohne teilweise geladene Daten");
    }
    finally { assets.Overrides.Clear(); }
    await candidate.LoadAsync();
    Check(candidate.Songs.Count == 559, "Erneutes Laden nach Korrektur funktioniert");
}
await Reject("songbooks.json", "[\"wir_loben_gott_1998.json\",\"wir_loben_gott_1998.json\"]", "mehrfach");
await Reject("liederbuch_demo_1.json", "{broken", "liederbuch_demo_1.json");
var duplicate = new SongBook { Id = "test", Title = "Test", Songs = [new Song { Number = "001", Title = "A" }, new Song { Number = "001", Title = "B" }] };
await Reject("liederbuch_demo_1.json", JsonSerializer.Serialize(duplicate), "doppelt");



var navigation = new TestNavigation();
var theme = new TestTheme();
var settings = new SettingsService(preferences, theme);
var all = new SongsViewModel(library, navigation, new("Alle Lieder"));
var changed = new List<string?>();
all.PropertyChanged += (_, e) => changed.Add(e.PropertyName);
await all.InitializeAsync();
Check(all.IsContentVisible && all.Songs.Count == 559 && !all.IsBusy, "Listen-ViewModel lädt und veröffentlicht sichtbaren Zustand");
all.Query = "Gemeinsam";
Check(all.Songs.Count == 4 && all.ResultSummary == "4 Lieder · 4 Liederbücher", "Query-Binding aktualisiert gemeinsame Suchergebnisse");
Check(changed.Contains(nameof(SongsViewModel.Songs)) && changed.Contains(nameof(SongsViewModel.ResultSummary)), "ViewModel meldet Änderungen für UI-Bindings");
all.Query = "xyzxyzxyz";
Check(all.Songs.Count == 0 && all.ResultSummary.StartsWith("0 Lieder"), "Leere Suche ist im ViewModel abgebildet");
all.Query = "Gemeinsam";
all.SelectedSong = demo;
await all.OpenSongCommand.ExecuteAsync(demo);
Check(navigation.LastSong == demo && all.SelectedSong == null && library.Recent[0] == demo.Key, "Command navigiert, setzt Auswahl zurück und pflegt Verlauf");
await all.InitializeAsync();
Check(all.Query == "Gemeinsam" && all.Songs.Count == 4, "Suchtext bleibt nach Rückkehr zur Liste erhalten");

var favoriteList = new SongsViewModel(library, navigation, new("Favoriten", SongListKind.Favorites));
await favoriteList.InitializeAsync();
Check(favoriteList.Songs.Count == 1 && favoriteList.Songs[0].Key == demo.Key, "Favoriten-ViewModel filtert nach Buch und Nummer");
library.Toggle(demo);
await favoriteList.InitializeAsync();
Check(favoriteList.Songs.Count == 0, "Favoritenliste aktualisiert sich beim Wiederanzeigen");
var detail = new SongDetailViewModel(library, settings, demo);
await detail.InitializeAsync();
var favoriteChanged = false;
detail.PropertyChanged += (_, e) => favoriteChanged |= e.PropertyName == nameof(SongDetailViewModel.FavoriteText);
detail.ToggleFavoriteCommand.Execute(null);
Check(detail.IsFavorite && favoriteChanged && detail.FavoriteText.Contains("entfernen"), "Detail-Command aktualisiert Favoritenzustand und Button-Binding");
Check(!library.Favorites.Contains(song.Key), "Detail-Command lässt gleich nummeriertes Lied eines anderen Buchs unverändert");
library.Visit(song);
library.Visit(demo);
var recentList = new SongsViewModel(library, navigation, new("Letzte Lieder", SongListKind.Recent));
await recentList.InitializeAsync();
Check(recentList.Songs[0].Key == demo.Key && recentList.Songs[1].Key == song.Key, "Verlauf-ViewModel zeigt neueste Lieder zuerst");
Check(recentList.CanRemoveRecent && !favoriteList.CanRemoveRecent, "X zum Entfernen nur in \"Letzte Lieder\"");
recentList.RemoveRecentCommand.Execute(demo);
Check(recentList.Songs[0].Key == song.Key && !library.Recent.Contains(demo.Key), "X entfernt Lied aus \"Letzte Lieder\" und aktualisiert die Liste");
Check(!new LibraryService(preferences, assets).Recent.Contains(demo.Key), "Entfernen aus Verlauf wird gespeichert");
library.Visit(demo);

var categories = new CategoriesViewModel(library, navigation);
await categories.InitializeAsync();
Check(categories.Groups.All(g => library.Books.Any(b => b.Id == g.BookId)), "Kategorien-ViewModel gruppiert je Liederbuch");
Check(categories.Groups.SelectMany(g => g.Categories).All(c => c.Count > 0), "Nur Kategorien mit Liedern werden angezeigt");
Check(categories.Groups.SelectMany(g => g.Categories).Select(c => (c.BookId, c.Id)).Distinct().Count()
      == categories.Groups.Sum(g => g.Categories.Count), "Buch und Kategorie-Id sind zusammen eindeutig");
Check(categories.BookFilters.Count == categories.Groups.Count + 1 && categories.BookFilters[0].Group == null
      && categories.SelectedBookFilter == categories.BookFilters[0], "Kategorie-Filter startet mit \"Alle Liederbücher\"");
var allCategoryGroups = categories.Groups.Count;
if (categories.BookFilters.Count > 1)
{
    var bookFilter = categories.BookFilters[1];
    categories.SelectedBookFilter = bookFilter;
    Check(categories.Groups.Count == 1 && categories.Groups[0].BookId == bookFilter.Group!.BookId, "Kategorie-Filter zeigt nur das gewählte Liederbuch");
    await categories.InitializeAsync();
    Check(categories.SelectedBookFilter?.Group?.BookId == bookFilter.Group.BookId && categories.Groups.Count == 1,
        "Kategorie-Filter behält Auswahl nach Reload");
    categories.SelectedBookFilter = categories.BookFilters[0];
    Check(categories.Groups.Count == allCategoryGroups, "\"Alle Liederbücher\" zeigt wieder alle Kategorien");
}
var firstCategory = categories.Groups[0].Categories[0];
await categories.OpenCategoryCommand.ExecuteAsync(firstCategory);
Check(navigation.LastOptions?.CategoryId == firstCategory.Id && navigation.LastOptions?.BookId == firstCategory.BookId,
    "Kategorie-Command übergibt Kategorie und Liederbuch an Navigation");
var categoryList = new SongsViewModel(library, navigation, navigation.LastOptions!);
await categoryList.InitializeAsync();
Check(categoryList.Songs.Count == firstCategory.Count
      && categoryList.Songs.All(s => s.CategoryId == firstCategory.Id && s.BookId == firstCategory.BookId),
    "Kategorieansicht filtert auf Kategorie und Buch");
Check(library.Songs.All(s => s.CategoryName.Length > 0), "Jedes Lied hat einen aufgelösten Kategorienamen");

var booksVm = new BooksViewModel(library, navigation);
await booksVm.InitializeAsync();
Check(booksVm.Books.Count == 5 && booksVm.Books.Select(b => b.Id).Distinct().Count() == 5, "Liederbuch-ViewModel listet alle Bücher");
var chosenBook = booksVm.Books[1];
await booksVm.OpenBookCommand.ExecuteAsync(chosenBook);
Check(navigation.LastOptions?.BookId == chosenBook.Id && booksVm.SelectedBook == null, "Buch-Command übergibt die Buch-ID und setzt die Auswahl zurück");
var bookList = new SongsViewModel(library, navigation, navigation.LastOptions!);
await bookList.InitializeAsync();
Check(bookList.Songs.Count > 0 && bookList.Songs.All(s => s.BookId == chosenBook.Id), "Liedliste startet mit vorausgewähltem Liederbuchfilter");
Check(bookList.BookFilters.Count == 6 && bookList.HasBookFilter && bookList.SelectedBookFilter?.Id == chosenBook.Id, "Filter enthält alle Bücher und zeigt die Vorauswahl");
bookList.SelectedBookFilter = bookList.BookFilters[0];
Check(bookList.Songs.Count == 559 && bookList.SelectedBookFilter?.Id == null, "Auswahl von „Alle Liederbücher“ hebt den Filter auf");
var demoBookId = library.Books.First(b => b.IsDemo).Id;
bookList.SelectedBookFilter = bookList.BookFilters.First(f => f.Id == demoBookId);
bookList.Query = "Gemeinsam";
Check(bookList.Songs.Count == 1 && bookList.Songs[0].BookId == demoBookId, "Suche und Liederbuchfilter wirken gemeinsam");
Check(bookList.BookFilters.Count == 6 && bookList.SelectedBookFilter?.Id == demoBookId, "Suchtext verwirft weder Filterliste noch Auswahl");
var favoriteFilter = new SongsViewModel(library, navigation, new("Favoriten", SongListKind.Favorites));
await favoriteFilter.InitializeAsync();
Check(favoriteFilter.BookFilters.Count == 2 && !favoriteFilter.HasBookFilter, "Filter bietet nur Bücher an, die in der Liste vorkommen");

preferences.Set("fontSize", 22.0);
preferences.Set("theme", 1);
var settingsVm = new SettingsViewModel(library, settings);
await settingsVm.InitializeAsync();
Check(settingsVm.FontSizeIndex == 1 && settingsVm.DesignIndex == 1 && settingsVm.Books.Count == 5, "Einstellungen-ViewModel übernimmt bestehende Geräteeinstellungen");
settingsVm.FontSizeIndex = 2;
settingsVm.DesignIndex = 2;
await detail.InitializeAsync();
Check(detail.Verses.All(v => v.FontSize == 27) && preferences.Get("fontSize", 0.0) == 27, "Geänderte Schriftgröße wird gespeichert und beim Wiederanzeigen übernommen");
Check(theme.LastDesign == AppDesign.Dark && preferences.Get("theme", 0) == 2, "Design-Binding speichert und aktiviert Design über Service");
settingsVm.FontSizeIndex = -1;
settingsVm.DesignIndex = -1;
Check(settingsVm.FontSizeIndex == 2 && settingsVm.DesignIndex == 2, "Temporär leere Picker-Auswahl überschreibt keine Einstellungen");
var refrain = library.Books.First(b => b.IsDemo).Songs[0];
var refrainVm = new SongDetailViewModel(library, settings, refrain);
await refrainVm.InitializeAsync();
Check(refrainVm.Verses[1].Heading == "Refrain", "Refrain ohne Nummer bleibt in der Liedansicht korrekt");

assets.Overrides["liederbuch_demo_1.json"] = "{broken";
var failing = new SongsViewModel(new LibraryService(preferences, assets), navigation, new("Alle Lieder"));
await failing.InitializeAsync();
Check(failing.HasError && !failing.IsBusy && !failing.IsContentVisible, "Ladefehler wird als bindbarer Zustand angezeigt");
assets.Overrides.Clear();
await failing.InitializeCommand.ExecuteAsync();
Check(!failing.HasError && failing.IsContentVisible && failing.Songs.Count == 559, "Retry-Command erholt sich nach Datenkorrektur");
var blockedNavigation = new TestNavigation { Pending = new TaskCompletionSource() };
var home = new HomeViewModel(library, blockedNavigation);
await home.InitializeAsync();
var firstNavigation = home.ShowSongsCommand.ExecuteAsync();
await home.ShowSongsCommand.ExecuteAsync();
Check(blockedNavigation.CallCount == 1 && !home.ShowSongsCommand.CanExecute(null), "Mehrfachklick startet keine doppelte Navigation");
blockedNavigation.Pending.SetResult();
await firstNavigation;
Check(home.ShowSongsCommand.CanExecute(null), "Navigation-Command wird nach Abschluss wieder verfügbar");
navigation.FailNext = true;
await all.OpenSongCommand.ExecuteAsync(song);
Check(all.HasError && all.OpenSongCommand.CanExecute(null), "Navigationsfehler wird abgefangen und Command freigegeben");
Check(!typeof(SongsViewModel).Assembly.GetReferencedAssemblies().Any(a => a.Name!.StartsWith("Microsoft.Maui")), "Models, Services und ViewModels sind ohne MAUI testbar");
Console.WriteLine($"{checks} Prüfungen erfolgreich.");

public sealed class MemoryPreferences : IPreferencesStore
{
    readonly Dictionary<string, object> data = [];
    public string Get(string key, string fallback) => data.TryGetValue(key, out var value) ? (string)value : fallback;
    public int Get(string key, int fallback) => data.TryGetValue(key, out var value) ? (int)value : fallback;
    public double Get(string key, double fallback) => data.TryGetValue(key, out var value) ? (double)value : fallback;
    public void Set(string key, string value) => data[key] = value;
    public void Set(string key, int value) => data[key] = value;
    public void Set(string key, double value) => data[key] = value;
}
public sealed class TestAssets : IAssetReader
{
    public Dictionary<string, string> Overrides { get; } = [];
    public Task<Stream> OpenAsync(string name) =>
        Task.FromResult<Stream>(Overrides.TryGetValue(name, out var text)
            ? new MemoryStream(Encoding.UTF8.GetBytes(text))
            : File.OpenRead(Path.Combine(AppContext.BaseDirectory, name)));
}
public sealed class TestTheme : IThemeService
{
    public AppDesign LastDesign { get; private set; }
    public void Apply(AppDesign design) => LastDesign = design;
}
public sealed class TestNavigation : INavigationService
{
    public Song? LastSong { get; private set; }
    public SongListOptions? LastOptions { get; private set; }
    public int CallCount { get; private set; }
    public TaskCompletionSource? Pending { get; set; }
    public bool FailNext { get; set; }
    Task Complete()
    {
        CallCount++;
        if (FailNext) { FailNext = false; throw new InvalidOperationException("Navigation fehlgeschlagen"); }
        return Pending?.Task ?? Task.CompletedTask;
    }
    public Task ShowSongsAsync(SongListOptions options) { LastOptions = options; return Complete(); }
    public Task ShowCategoriesAsync() => Complete();
    public Task ShowBooksAsync() => Complete();
    public Task ShowSongAsync(Song song) { LastSong = song; return Complete(); }
}
