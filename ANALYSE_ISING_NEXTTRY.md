# Analyse von iSing_nexttry.zip

Stand: 15. September 2026. Geprüft wurde ausschließlich der neue Archivstand, isoliert entpackt unter `C:\Users\viktor.ott\AppData\Local\Temp\ising-nexttry-review-ce50607dc7c94a54be686f83fb8851a1\iSing`. ZIP und Anwendungsquellcode wurden nicht verändert. Enthaltene Dokumentation wurde als Projektinhalt behandelt; frühere Testaussagen wurden nicht als aktuelle Prüfergebnisse übernommen.

## Ergebnis

Das Projekt ist nachvollziehbar nach MVVM aufgebaut. Die beiden zuvor gefundenen Ladeblocker wurden korrigiert. Der aktuelle Datenbestand lässt sich vollständig laden. Noch offen sind eine unvollständige Kategorienansicht, eine nicht zum Datenbestand passende Testsuite und die iOS-Privacy-Deklaration für Preferences.

## Befunde nach Priorität

### 1. P1: 167 Lieder fehlen in der Kategorienübersicht

`Services/LibraryService.cs:80` löst Kategorienamen nur über die Kategorie-Definitionen des jeweiligen Buchs auf. Unbekannte IDs werden zu „Ohne Kategorie“. `ViewModels/CategoriesViewModel.cs:27–32` erzeugt anschließend ausschließlich Einträge aus diesen Definitionen. Für unbekannte IDs wird auch keine Auffangkategorie angezeigt.

Auswirkung mit den tatsächlich ausgelieferten JSON-Dateien:

| Buch | Lieder | Ohne auflösbare Kategorie |
| --- | ---: | ---: |
| glauben – hoffen – singen | 8 | 0 |
| Wir loben Gott | 576 | 158 |
| Demo 2 | 3 | 3 |
| Demo 3 | 3 | 3 |
| Demo 4 | 3 | 3 |
| Gesamt | 593 | 167 |

Im Originalbuch fehlen Definitionen für `advent` (2 Lieder), `anbetung` (74), `lobpreis` (35), `vertrauen` (19) und `auftrag-und-zeugniss` (28). Für die letzte ID existiert stattdessen `auftrag-und-zeugnis`. Die drei Demo-Dateien enthalten keine Kategorie-Definitionen.

Ein separat ausgeführter Test mit den unveränderten Models, Services und ViewModels bestätigt: **593 geladene Lieder, 167 mit „Ohne Kategorie“, nur zwei Buchgruppen und 426 Lieder in der Kategorienübersicht.** In der allgemeinen Suche bleiben sämtliche Lieder verfügbar.

Die früher gewünschte Darstellung tatsächlich zugewiesener Kategorien wird damit nicht vollständig erfüllt. Abhilfe: fehlende Definitionen ergänzen, den Schreibfehler korrigieren und/oder unbekannte Zuordnungen sichtbar auffangen. Eine fachliche Neuzuordnung von Liedern sollte vorher mit dem Nutzer abgestimmt werden.

### 2. P2: Die Testsuite bricht vor den eigentlichen Funktionsprüfungen ab

`dotnet run --project Tests/iSing.Checks.csproj -p:UseSharedCompilation=false` wurde ausgeführt. Ergebnis: Exitcode 1, Abbruch bei `Tests/Program.cs:22`, der ersten Assertion. Sie erwartet 559 statt tatsächlich 593 Lieder. Weitere Erwartungen beruhen ebenfalls auf dem alten Bestand: 547 Original-Lieder, vier Demo-Bücher und vier gemeinsame Kategorien.

Zusätzlich überschreiben die Fehlerfalltests in `Tests/Program.cs:68`, `:70` und `:169` weiterhin `liederbuch_demo_1.json`. Diese Datei wird vom aktuellen Manifest nicht geladen. Eine Korrektur nur der Liedanzahl reicht daher nicht aus. Die hinzugefügten Kategorieprüfungen werden wegen des frühen Abbruchs gar nicht erreicht; außerdem akzeptiert der Test auf einen nichtleeren Kategorienamen auch den Fallback „Ohne Kategorie“.

Abhilfe: Logiktests auf kontrollierte Testdaten umstellen und die ausgelieferten JSON-Dateien separat auf Konsistenz prüfen. Bei den Datenprüfungen insbesondere unbekannte Kategorieverweise erfassen.

### 3. P2: iOS-Privacy-Deklaration für Preferences bleibt auskommentiert

`Infrastructure/MauiAdapters.cs:6–11` verwendet `Preferences.Default` für Favoriten, Verlauf und Einstellungen. In `Platforms/iOS/Resources/PrivacyInfo.xcprivacy:39–48` befindet sich der Eintrag für `NSPrivacyAccessedAPICategoryUserDefaults` mit Grund `CA92.1` weiterhin in einem XML-Kommentar.

Die Deklaration muss für die eigene Nutzung von Preferences im iOS-Manifest enthalten sein. Das ist ein offener Punkt für die App-Store-Einreichung; ein Android-Build überprüft ihn nicht. Quelle: [Microsoft Learn – Apple privacy manifest](https://learn.microsoft.com/en-us/dotnet/maui/ios/privacy-manifest?view=net-maui-10.0).

## Gegenüber dem vorherigen ZIP behoben

- `AppPage` referenziert keine fehlende `PageTemplate` mehr. Stattdessen verwenden alle sechs Inhaltsseiten `PageStateView` für Inhalt, Ladezustand, Fehler und Wiederholen. Bei der statischen Prüfung wurden keine fehlenden `StaticResource`-Schlüssel gefunden. Die Darstellung auf einem Gerät wurde nicht geprüft.
- Die zuvor leeren Titel der Lieder 004–008 in `glauben_hoffen_singen_2015.json` sind ergänzt. Der Loader lädt jetzt alle fünf Bücher erfolgreich.

## Architektur und Funktionsumfang

- Models enthalten Bücher, Lieder, Strophen und buchbezogene Kategorie-Definitionen.
- Services kapseln Laden, Suche, Favoriten, Verlauf und Einstellungen hinter Interfaces.
- ViewModels veröffentlichen bindbare Zustände und Commands. Sie lassen sich ohne MAUI kompilieren und ausführen.
- Die Plattformadapter kapseln Preferences, Paketdateien, Theme und Navigation. Dependency Injection und PageFactory verbinden die Schichten.
- `PageStateView` enthält ausschließlich Darstellungslogik; das widerspricht MVVM nicht.
- Buchübergreifende Suche, sichtbare Buchlabels und buchbezogene Filter sind implementiert. Liedkennungen kombinieren Buch-ID und Liednummer, sodass gleiche Nummern verschiedener Bücher getrennt bleiben.
- Ein Media-Player ist entsprechend der Vorgabe nicht umgesetzt.

Die JSON-Dateien sind eingebettete App-Ressourcen: Inhaltliche Änderungen oder zusätzliche registrierte Bücher werden durch Neubauen und Installieren ausgeliefert. Ein dynamischer Import zur Laufzeit ist nicht vorhanden.

## Tatsächlich ausgeführte Prüfungen

- Originale Konsolen-Testsuite: fehlgeschlagen bei der ersten Assertion, siehe Befund 2.
- Separater Prüfcode außerhalb des Projekts mit unveränderten Models/Services/ViewModels: Laden von 5 Büchern und 593 Liedern; sichtbarer Listenstatus ohne Fehler; Suche nach `002` liefert Treffer aus allen 5 Büchern; alle 5 Buchfilter auf korrekte Anzahl und Buchzugehörigkeit geprüft; Favoriten und Verlauf exemplarisch geprüft. Alle diese Prüfungen erfolgreich.
- Kategorienabdeckung mit demselben Prüfcode: 426 von 593 Liedern, siehe Befund 1.
- XAML-Ressourcen: keine fehlenden statischen Ressourcenschlüssel gefunden; alle sechs Inhaltsseiten verwenden `PageStateView`.
- Android-Debug-Build für `net10.0-android` / `android-x64`: erfolgreich, signierte Debug-APK erzeugt, 0 Fehler und 2 NU1900-Warnungen. NuGet konnte Sicherheitsrisikodaten von `api.nuget.org` nicht abrufen; die Paket-Sicherheitsprüfung ist damit nicht bestätigt. Befehl: `dotnet build iSing.csproj -f net10.0-android -r android-x64 -p:TargetFrameworks=net10.0-android -p:UseSharedCompilation=false -m:1 --nologo`.
- Kein Emulator-/Gerätetest, kein iOS-Build und kein Release-Build durchgeführt. Aussagen zur tatsächlichen Darstellung und iOS-Lauffähigkeit sind deshalb nicht abschließend.

## Weitere Abweichungen

Der Bestand umfasst zwei reguläre Bücher und drei Dummies, nicht mehr ein reguläres Buch und vier Dummies. `README.md` und `LIEDERBUECHER.md` beschreiben weiterhin den früheren Bestand. Die Anleitung sollte außerdem die jetzt verwendeten buchbezogenen Kategorie-Definitionen erklären, damit durch reine JSON-Anpassungen keine unsichtbaren Kategorien entstehen.
