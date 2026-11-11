using iSing.Models;
namespace iSing.Services;

public interface IPreferencesStore
{
    string Get(string key, string fallback);
    int Get(string key, int fallback);
    double Get(string key, double fallback);
    void Set(string key, string value);
    void Set(string key, int value);
    void Set(string key, double value);
}
public interface IAssetReader { Task<Stream> OpenAsync(string name); }
public interface ILibraryService
{
    IReadOnlyList<SongBook> Books { get; }
    IReadOnlyList<Song> Songs { get; }
    IReadOnlySet<string> Favorites { get; }
    IReadOnlyList<string> Recent { get; }
    bool IsLoaded { get; }
    Task LoadAsync();
    void Toggle(Song song);
    void Visit(Song song);
    void RemoveRecent(Song song);
    IEnumerable<Song> Search(IEnumerable<Song> songs, string? query);
}
public enum SongListKind { All, Favorites, Recent, Category }
// BookId ist die Vorauswahl des Liederbuchfilters, kein fester Filter: die Liste bleibt auf andere Bücher umschaltbar.
// Ausnahme SongListKind.Category: dort ist BookId fest, weil eine Kategorie-Id nur innerhalb ihres Buchs gilt.
public sealed record SongListOptions(string Title, SongListKind Kind = SongListKind.All, string? CategoryId = null, string? BookId = null);
public interface INavigationService
{
    Task ShowSongsAsync(SongListOptions options);
    Task ShowCategoriesAsync();
    Task ShowBooksAsync();
    Task ShowSongAsync(Song song);
}
public enum AppDesign { System, Light, Dark }
public interface IThemeService { void Apply(AppDesign design); }
public interface ISettingsService
{
    double FontSize { get; set; }
    AppDesign Design { get; set; }
}
