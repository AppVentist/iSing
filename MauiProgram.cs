using iSing.Infrastructure;
using iSing.Services;
using iSing.ViewModels;
using iSing.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
namespace iSing;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>().ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            fonts.AddFont("BunnyCute.otf", "BunnyCute");
        });
        builder.Services.AddSingleton<IPreferencesStore, MauiPreferencesStore>();
        builder.Services.AddSingleton<IAssetReader, MauiAssetReader>();
        builder.Services.AddSingleton<ILibraryService, LibraryService>();
        builder.Services.AddSingleton<IThemeService, MauiThemeService>();
        builder.Services.AddSingleton<ISettingsService, SettingsService>();
        builder.Services.AddSingleton<IPageFactory, MauiPageFactory>();
        builder.Services.AddSingleton<INavigationService, MauiNavigationService>();
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<CategoriesViewModel>();
        builder.Services.AddTransient<BooksViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddSingleton<LibraryShell>();
#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}
