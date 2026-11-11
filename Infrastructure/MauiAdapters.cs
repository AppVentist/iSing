using iSing.Services;
namespace iSing.Infrastructure;

public sealed class MauiPreferencesStore : IPreferencesStore
{
    public string Get(string key, string fallback) => Preferences.Default.Get(key, fallback);
    public int Get(string key, int fallback) => Preferences.Default.Get(key, fallback);
    public double Get(string key, double fallback) => Preferences.Default.Get(key, fallback);
    public void Set(string key, string value) => Preferences.Default.Set(key, value);
    public void Set(string key, int value) => Preferences.Default.Set(key, value);
    public void Set(string key, double value) => Preferences.Default.Set(key, value);
}
public sealed class MauiAssetReader : IAssetReader
{
    public Task<Stream> OpenAsync(string name) => FileSystem.OpenAppPackageFileAsync(name);
}
public sealed class MauiThemeService : IThemeService
{
    public void Apply(AppDesign design)
    {
        if (Application.Current is { } app) app.UserAppTheme = (AppTheme)design;
    }
}
