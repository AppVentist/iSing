# iSing

Native .NET MAUI Liederbuch-App für Android und iOS, nach dem bereitgestellten Mockup.

## Umfang

- Gemeinsame Suche in allen fünf Liederbüchern: 547 Original-Lieder plus zwölf ausdrücklich gekennzeichnete Demo-Lieder.
- Suche nach Liednummer, Titel, Buchlabel und Strophentext; mehrere Suchwörter werden kombiniert.
- Buchlabels in Suchergebnissen, Favoriten, Verlauf und Liedansicht.
- Liederbuchfilter über der Liedliste: Auswahl eines Buchs oder aller Bücher, kombinierbar mit Suche, Favoriten, Verlauf und Kategorien.
- Liederbuchübersicht über die Startseite: ein Buch öffnet die Liedliste mit vorausgewähltem Filter.
- Kategorien aus den tatsächlichen Liedzuordnungen aller Bücher.
- Favoriten und Verlauf verwenden Buch-ID plus Liednummer. Bestehende Einträge der ersten Version werden dem ursprünglichen Buch zugeordnet und bleiben erhalten.
- Drei Schriftgrößen, System-/Hell-/Dunkelmodus; vollständig offline, ohne Media-Player.

## Liederbücher bearbeiten

Die vier Dummies sind bereits registriert und werden automatisch gemeinsam dargestellt. Nur JSON-Inhalte ändern und die App anschließend neu bauen/installieren. Es ist keine C#-Änderung nötig.

| Datei unter Resources/Raw | Aktueller Titel |
| --- | --- |
| liederbuch_demo_1.json | Gemeindelieder |
| liederbuch_demo_2.json | Jugendlieder |
| liederbuch_demo_3.json | Kinderlieder |
| liederbuch_demo_4.json | Festtagslieder |

Details und Datenformat: [LIEDERBUECHER.md](LIEDERBUECHER.md).

## Architektur

MVVM mit getrennten Models, Views, ViewModels und Services sowie Dependency Injection. Die Views sind XAML-Dateien mit kompilierten Bindings (x:DataType); das Code-behind setzt nur den BindingContext. Die ViewModels sind ohne MAUI testbar. Details: [ARCHITEKTUR.md](ARCHITEKTUR.md).

## Entwickeln

Voraussetzungen: .NET 10, MAUI-Workloads, Android SDK/JDK; für iOS zusätzlich ein Mac mit passendem Xcode und Apple-Signierung.

~~~powershell
dotnet build iSing.csproj -f net10.0-android -p:TargetFrameworks=net10.0-android
dotnet build iSing.csproj -f net10.0-ios -p:TargetFrameworks=net10.0-ios
dotnet run --project Tests/iSing.Checks.csproj -p:UseSharedCompilation=false
~~~

Für das x64-Emulator-Testpaket wird eine separate Kennung verwendet. Damit wird eine vorhandene iSing-App nicht ersetzt. Dieses Paket ist nicht für übliche ARM64-Smartphones bestimmt.

~~~powershell
dotnet build iSing.csproj -f net10.0-android -r android-x64 -p:TargetFrameworks=net10.0-android -p:ApplicationId=dev.local.ising.mockup -p:UseSharedCompilation=false -m:1
~~~

## Prüfung

Die automatisierten Prüfungen decken unter anderem gemeinsame Suche, Liederbuchfilter, überlappende Liednummern, Übernahme alter Favoriten, getrennte Favoriten und Verlauf sowie ungültige und korrigierte JSON-Dateien ab. Die Prüfungen verwenden In-Memory-Adapter und ersetzen keine Gerätetests. iOS wurde auf diesem Windows-Rechner nicht gebaut oder ausgeführt.

Android-x64-Build nach der MVVM-Umstellung: erfolgreich, 0 Warnungen und 0 Fehler. Im Emulator wurden 559 Lieder aus fünf Büchern, die Suche nach „Gemeinsam“, Liedauswahl, Favoriten-Command, Schriftgrößen-Auswahl und Dunkelmodus geprüft. Screenshot: ising-mvvm-preview.png.

Diese Prüfung stammt aus der Version mit in C# aufgebauten Views. Die Umstellung der Views auf XAML ist noch nicht gebaut und nicht auf einem Gerät geprüft.

## Vor Veröffentlichung klären

Die Paketkennung com.companyname.ising ist ein Entwicklungsplatzhalter. Endgültige Paketkennung und Signierung sind mit dem Auftraggeber festzulegen. Bergmotiv und Buch-/Noten-Icon sind vorläufige eigene Vektorgrafiken.

Technische Referenz: https://learn.microsoft.com/dotnet/maui/whats-new/dotnet-10?view=net-maui-10.0

