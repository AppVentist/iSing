using iSing.Infrastructure;
using iSing.Services;
namespace iSing.Views;

// Aufbau und Farben stehen in LibraryShell.xaml.
// Nur die Seitenerzeugung bleibt in C#, damit die Seiten weiterhin aus der PageFactory kommen.
public partial class LibraryShell : Shell
{
    public LibraryShell(IPageFactory pages)
    {
        InitializeComponent();
        HomeContent.ContentTemplate = new DataTemplate(() => pages.Home());
        SongsContent.ContentTemplate = new DataTemplate(() => pages.Songs(new("Alle Lieder")));
        FavoritesContent.ContentTemplate = new DataTemplate(() => pages.Songs(new("Favoriten", SongListKind.Favorites)));
        SettingsContent.ContentTemplate = new DataTemplate(() => pages.Settings());
    }
}
