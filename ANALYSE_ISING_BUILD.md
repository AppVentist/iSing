# Analyse von iSing_build.zip

Stand: 15.09.2026. Gegenstand ist ausschließlich das gelieferte Archiv, nicht der bisherige Workspace-Stand. Die Quellen wurden in eine separate temporäre Kopie entpackt und nicht verändert.

## Gesamturteil

Die Architektur ist eine sinnvolle native .NET-MAUI/MVVM-Grundlage mit XAML-Views, Dependency Injection, plattformunabhängigen ViewModels und austauschbaren Services. Der aktuelle Archivstand hat jedoch zwei voneinander unabhängige Blocker: eine fehlende XAML-Ressource und einen vom eigenen Loader abgewiesenen Datenbestand.

## 1. P1 – Fehlende Ressource verhindert die Initialisierung der Oberfläche

[Resources/Styles/Styles.xaml:176](C:/Users/viktor.ott/AppData/Local/Temp/ising-review-864a7e5802244f6b9e58cc6592950a8c/iSing/Resources/Styles/Styles.xaml:176)

AppPage referenziert PageTemplate über StaticResource. Die vollständige Definition dieses ControlTemplate ist jedoch auskommentiert. Eine XML-Prüfung aller aktiven XAML-Ressourcen findet keinen Schlüssel PageTemplate.

App.xaml bindet dieses ResourceDictionary bereits während App.InitializeComponent ein. Damit ist beim Laden eine XAML-Ressourcenauflösung gegen einen fehlenden Schlüssel zu erwarten. Ein erfolgreicher Build würde diesen Fehler nicht als behoben nachweisen. Ein Gerätelauf wurde in dieser Analyse nicht durchgeführt.

Korrektur: ein gültiges PageTemplate wiederherstellen. Nur den Verweis zu entfernen wäre unvollständig: Dann fehlen weiterhin die an IsBusy, HasError, ErrorMessage und InitializeCommand gebundenen Lade-, Fehler- und Wiederholungsansichten.

## 2. P1 – Unvollständige Lieder blockieren die gesamte Bibliothek

[Resources/Raw/glauben_hoffen_singen_2015.json:195](C:/Users/viktor.ott/AppData/Local/Temp/ising-review-864a7e5802244f6b9e58cc6592950a8c/iSing/Resources/Raw/glauben_hoffen_singen_2015.json:195)
[Services/LibraryService.cs:62](C:/Users/viktor.ott/AppData/Local/Temp/ising-review-864a7e5802244f6b9e58cc6592950a8c/iSing/Services/LibraryService.cs:62)

Die Lieder 004 bis 008 haben leere Titel. LibraryService verlangt für jedes Lied Nummer und Titel und veröffentlicht die Bibliothek erst nach vollständiger Validierung aller Bücher.

Reproduziert mit dem unveränderten mitgelieferten Prüfprogramm:
~~~text
InvalidDataException:
glauben_hoffen_singen_2015.json:
Jedes Lied benötigt eine Nummer und einen Titel.
~~~

Auswirkung: Auch die gültigen Bücher werden nicht angezeigt. Der Prüfprozess beendet sich vor der ersten Assertion mit Exitcode 1.

Korrektur: Inhalte vervollständigen oder unvollständige Einträge vorläufig aus dem veröffentlichten Datenbestand nehmen. Soll es Entwürfe geben, benötigt das Format einen ausdrücklich vorgesehenen Entwurfszustand und eine entsprechende Ladeentscheidung.

## 3. P2 – Tests und Dokumentation passen nicht mehr zum Datenbestand

[Tests/Program.cs:22](C:/Users/viktor.ott/AppData/Local/Temp/ising-review-864a7e5802244f6b9e58cc6592950a8c/iSing/Tests/Program.cs:22)
[Tests/Program.cs:68](C:/Users/viktor.ott/AppData/Local/Temp/ising-review-864a7e5802244f6b9e58cc6592950a8c/iSing/Tests/Program.cs:68)

Aktueller JSON-Bestand, einschließlich unvollständiger Einträge:

| Buch | Einträge |
| --- | ---: |
| Wir loben Gott | 576 |
| glauben – hoffen – singen | 8 |
| Jugendlieder · Demo | 3 |
| Kinderlieder · Demo | 3 |
| Festtagslieder · Demo | 3 |
| Gesamt | 593 |

Tests erwarten noch 559 Einträge, 547 Lieder im Originalbuch und vier Dummies. Mehrere Fehlertests überschreiben liederbuch_demo_1.json, obwohl diese Datei nicht mehr im Manifest steht. Selbst nach Korrektur der leeren Titel würden daher weitere Prüfungen scheitern oder das falsche Szenario testen. README und Bearbeitungsanleitung beschreiben ebenfalls noch den alten Bestand.

Korrektur: feste Test-Fixtures von den bearbeitbaren App-Daten trennen. Die tatsächlichen App-Dateien zusätzlich auf Schema und eindeutige IDs prüfen; alte Dateinamen und Bestandsangaben aktualisieren.

## 4. P2 – iOS-Privacy-Manifest berücksichtigt Preferences nicht

[Platforms/iOS/Resources/PrivacyInfo.xcprivacy:39](C:/Users/viktor.ott/AppData/Local/Temp/ising-review-864a7e5802244f6b9e58cc6592950a8c/iSing/Platforms/iOS/Resources/PrivacyInfo.xcprivacy:39)

Die App verwendet MAUI Preferences für Favoriten, Verlauf und Einstellungen. Der Eintrag NSPrivacyAccessedAPICategoryUserDefaults mit CA92.1 ist im Manifest aber auskommentiert. Microsoft nennt diesen Eintrag für die Verwendung der Preferences-API ausdrücklich als erforderlich. Das ist vor einer iOS-Veröffentlichung zu korrigieren und im erzeugten App-Paket zu prüfen. [Microsoft-Dokumentation](https://devblogs.microsoft.com/dotnet/apple-privacy-manifest-support/)

## Architektur und Wartbarkeit

Positiv: kleine XAML-Code-behind-Dateien, sechs spezialisierte ViewModels, zentrale PageFactory, Navigation über ein Interface, Buch-ID plus Liednummer als Favoritenkennung, zentrale JSON-Validierung und getrennte Plattformadapter. Die neue Buchauswahl und der kombinierbare Buchfilter sind klar in den ViewModels umgesetzt.

Weitere Bereinigung: Das Archiv enthält ein rund 55 MB großes Diagnoseprotokoll, lokale .csproj.user-Einstellungen und nicht als Ziel aktivierte Windows-/MacCatalyst-Plattformordner. Diese Dateien sind keine aktuellen Startblocker, erschweren aber die Weitergabe und Orientierung. Die Paketkennung com.companyname.ising ist weiterhin ein Entwicklungsplatzhalter.

## Umfang der Verifikation

- ZIP-Inventar, Quellen, XAML-Ressourcen und JSON-Daten geprüft.
- Mitgelieferte Tests ausgeführt: Abbruch beim Datenladen, keine erfolgreich durchgelaufene Testsuite.
- Vollständiger Android-Debug-Build für android-x64 erfolgreich: 0 Fehler, 2 Warnungen vom Typ NU1900, Laufzeit 4:28 Minuten. Das ist ein Build-Nachweis, kein erfolgreicher App-Start. Gebaut wurde ausschließlich in der temporären Archivkopie.
- NuGet-Audit: NU1900; die Sicherheitsdaten von api.nuget.org waren auch beim erneuten Versuch nicht verfügbar. Keine Aussage zur Freiheit von Paket-Sicherheitslücken.
- Kein iOS-Build, kein Release-Build und keine Installation des Archivprojekts auf einem Gerät.
- Keine Korrekturen am Archiv oder bestehenden Projekt vorgenommen.

Empfohlene Reihenfolge: PageTemplate reparieren, Datenbestand bereinigen, Tests an feste Fixtures anpassen, anschließend App-Start und Nutzerabläufe erneut testen und iOS-Veröffentlichungsvoraussetzungen vervollständigen.
