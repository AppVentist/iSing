using iSing.Models;
using iSing.Services;
using iSing.ViewModels;
using iSing.Views;
using Microsoft.Extensions.DependencyInjection;
namespace iSing.Infrastructure;

// All platform-specific page creation is centralized here; viewmodels know only INavigationService.
public interface IPageFactory
{
    Page Home();
    Page Songs(SongListOptions options);
    Page Categories();
    Page Books();
    Page Song(Song song);
    Page Settings();
}
public sealed class MauiPageFactory(IServiceProvider services) : IPageFactory
{
    public Page Home() => new HomePage(services.GetRequiredService<HomeViewModel>());
    public Page Songs(SongListOptions options) => new SongsPage(ActivatorUtilities.CreateInstance<SongsViewModel>(services, options));
    public Page Categories() => new CategoriesPage(services.GetRequiredService<CategoriesViewModel>());
    public Page Books() => new BooksPage(services.GetRequiredService<BooksViewModel>());
    public Page Song(Song song) => new SongPage(ActivatorUtilities.CreateInstance<SongDetailViewModel>(services, song));
    public Page Settings() => new SettingsPage(services.GetRequiredService<SettingsViewModel>());
}
public sealed class MauiNavigationService(IPageFactory pages) : INavigationService
{
    public Task ShowSongsAsync(SongListOptions options) => Shell.Current.Navigation.PushAsync(pages.Songs(options));
    public Task ShowCategoriesAsync() => Shell.Current.Navigation.PushAsync(pages.Categories());
    public Task ShowBooksAsync() => Shell.Current.Navigation.PushAsync(pages.Books());
    public Task ShowSongAsync(Song song) => Shell.Current.Navigation.PushAsync(pages.Song(song));
}
