using iSing.Services;
using iSing.Views;
using Microsoft.Extensions.DependencyInjection;
namespace iSing;
public partial class App : Application
{
    readonly IServiceProvider services;
    readonly ISettingsService settings;
    readonly IThemeService theme;
    public App(IServiceProvider services, ISettingsService settings, IThemeService theme)
    {
        InitializeComponent();
        this.services = services;
        this.settings = settings;
        this.theme = theme;
    }
    // Die Shell wird erst hier aufgelöst. Ihre XAML greift auf die Ressourcen aus App.xaml zu,
    // die erst nach InitializeComponent zur Verfügung stehen.
    protected override Window CreateWindow(IActivationState? activationState)
    {
        theme.Apply(settings.Design);
        return new Window(services.GetRequiredService<LibraryShell>());
    }
}
