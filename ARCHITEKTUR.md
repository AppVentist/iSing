# MVVM-Architektur

iSing trennt Models, ViewModels, Views und Services. Die Views sind XAML-Dateien; das Code-behind enthält nur Konstruktor und Lebenszyklus.

## Zuständigkeiten

| Ordner / Datei | Aufgabe |
| --- | --- |
| Models | SongBook, Song und Verse; Datenformat der JSON-Dateien, keine MAUI-Abhängigkeit |
| Services | Bibliothek, Suche, Favoriten-/Verlaufsspeicherung und Einstellungen; Zugriff über Interfaces |
| ViewModels | Anzeigezustand, Suchtext, Ergebnislisten, Auswahl, Lade-/Fehlerzustand und Commands |
| Views | XAML-Seiten mit Layout und Bindings; Code-behind setzt nur BindingContext und reicht OnAppearing weiter |
| Resources/Styles | Colors.xaml und Styles.xaml mit Farben, Schriftgraden, Kartenflächen und dem Seiten-ControlTemplate |
| Infrastructure | MAUI-Adapter für Preferences, Paketdateien, Design und Navigation; zentrale PageFactory |
| MauiProgram.cs | Dependency-Injection-Registrierung |
| Tests | Plattformunabhängige Prüfung der tatsächlichen Models, Services und ViewModels |

## Views

Jede Seite besteht aus `Seite.xaml` und `Seite.xaml.cs`:

| XAML | ViewModel |
| --- | --- |
| LibraryShell.xaml | – (Tabs; Seitenerzeugung über IPageFactory im Code-behind) |
| HomePage.xaml | HomeViewModel |
| SongsPage.xaml | SongsViewModel |
| CategoriesPage.xaml | CategoriesViewModel |
| BooksPage.xaml | BooksViewModel |
| SongPage.xaml | SongDetailViewModel |
| SettingsPage.xaml | SettingsViewModel |

- `PageViewBase` ist die gemeinsame Basisklasse aller Seiten und ruft in OnAppearing `InitializeAsync` des ViewModels auf. Sie ist das Wurzelelement der XAML-Dateien.
- Das Code-behind setzt `ViewModel` und `BindingContext` vor `InitializeComponent` und enthält sonst keine Logik.
- Lade- und Fehlerzustand stehen einmal zentral im ControlTemplate `PageTemplate` (Styles.xaml): ContentPresenter für den Seiteninhalt, Ladehinweis bei `IsBusy`, Fehlermeldung mit `InitializeCommand` bei `HasError`. Jede Seite bindet das über `Style="{StaticResource AppPage}"` ein.
- Farben und Schriftgrade stehen ausschließlich in Colors.xaml und Styles.xaml, nicht mehr in C#.
- Alle Seiten und Datentemplates setzen `x:DataType`, die Bindings werden also kompiliert.
- `LibraryShell.xaml` beschreibt Tabs und Farben; das Code-behind weist den vier ShellContent-Elementen die ContentTemplates der IPageFactory zu, weil die Lieder- und Favoritenseite mit unterschiedlichen `SongListOptions` erzeugt werden.
- `App.CreateWindow` holt die Shell aus dem Dienstanbieter, damit die Ressourcen aus App.xaml vor dem Laden der Shell-XAML bereitstehen.

## ViewModels

- HomeViewModel: Buch-/Liedanzahl und Navigation zu den Startseiten-Funktionen.
- SongsViewModel: gemeinsame Suche, Favoriten-/Verlaufs-/Kategoriefilter, Liederbuchfilter, Auswahl und Liednavigation.
- CategoriesViewModel: Kategorien über alle Bücher und Kategorieauswahl.
- BooksViewModel: Liederbuchübersicht mit Jahr und Liedzahl; öffnet die Liedliste mit vorausgewähltem Buch.
- SongDetailViewModel: Strophen, Schriftgröße, Buchlabel und Favoriten-Command.
- SettingsViewModel: Bücherübersicht und Zweiwege-Bindings für Schriftgröße und Design.
- PageViewModel: Initialisierung, Ladezustand, Fehlermeldung und erneuter Ladeversuch.

ObservableObject implementiert INotifyPropertyChanged. RelayCommand und AsyncCommand implementieren ICommand ohne zusätzliche Paketabhängigkeit. AsyncCommand verhindert parallele Ausführungen desselben Commands und führt Fehler in den bindbaren Fehlerzustand zurück.

## Datenfluss

1. Die View bindet den Suchtext bidirektional an SongsViewModel.Query.
2. Das ViewModel lässt die Daten vom ILibraryService filtern.
3. PropertyChanged aktualisiert die gebundene Liedliste und Trefferanzahl.
4. Die Auswahl löst OpenSongCommand aus. INavigationService öffnet die Liedansicht.
5. Favoritenänderungen werden über ILibraryService gespeichert; bei Rückkehr aktualisiert InitializeAsync die Liste.

## Liederbuchfilter

- `SongsViewModel.BookFilters` enthält „Alle Liederbücher“ und jedes Buch, das in der aktuellen Liste vorkommt. In den Favoriten erscheinen also nur Bücher mit Favoriten.
- `SelectedBookFilter` ist zweiwege gebunden und löst ein Refresh aus. Der Filter wirkt vor der Suche, Suchtext und Buchauswahl sind also kombinierbar.
- Die Filterliste wird nur neu aufgebaut, wenn sich die enthaltenen Bücher ändern. Eine Eingabe im Suchfeld verwirft die Auswahl deshalb nicht.
- `SongListOptions.BookId` ist nur die Vorauswahl. Aus der Liederbuchübersicht geöffnete Listen lassen sich also auf ein anderes Buch oder auf alle Bücher umschalten.
- `HasBookFilter` blendet die Filterzeile aus, solange nur ein Buch in der Liste vorkommt.
- In der View ist der Filter eine waagerechte CollectionView mit `SelectionMode="Single"`; ausgewählt und normal werden über den VisualStateManager unterschieden, ohne Konverter.

Navigation, Speicherung und Dateioperationen stehen nicht in den Views. ViewModels kennen weder Page/Shell noch MAUI Preferences. Die Tests können deshalb dieselben ViewModels ohne Emulator ausführen.

## Lebensdauer und Kompatibilität

Bibliothek, Einstellungen und Plattformadapter sind pro App gemeinsam. ViewModels werden pro Ansicht erstellt; dynamische Parameter wie Lied und Listenfilter werden von der PageFactory injiziert. Es gibt keinen globalen Library.Current-Zugriff.

Die fünf JSON-Dateien, Buch-IDs, bisherigen Preference-Schlüssel und die Migration von alten Favoriten/Verlaufseinträgen bleiben erhalten. Zum Bearbeiten der Liederbücher gilt weiterhin LIEDERBUECHER.md.

## Prüfen

~~~powershell
dotnet run --project Tests/iSing.Checks.csproj -p:UseSharedCompilation=false
~~~

Die Prüfungen umfassen Datenintegrität, buchübergreifende Suche und Identität, bestehende Favoriten, Binding-Benachrichtigungen, Commands, Navigation, Wiederanzeige, Einstellungen, Ladefehler, erneutes Laden sowie den Liederbuchfilter und die Liederbuchübersicht. Models, Services und ViewModels wurden bei der XAML-Umstellung nicht verändert, die Prüfungen gelten unverändert.

Die frühere Prüfung im Android-Emulator bezog sich auf die in C# aufgebauten Views (Screenshot ising-mvvm-preview.png). Nach der Umstellung auf XAML stehen Build und Gerätetest erneut aus: Darstellung, Lade-/Fehleransicht, Suche, Liedauswahl, Favoriten-Button, Schriftgröße und Dunkelmodus sind im Emulator nachzuprüfen.
