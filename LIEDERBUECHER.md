# Liederbücher in iSing pflegen

## Die vier vorhandenen Dummies

In Resources/Raw liegen liederbuch_demo_1.json bis liederbuch_demo_4.json. Jede Datei enthält drei frei bearbeitbare Beispiel-Lieder und ist bereits in songbooks.json eingetragen.

1. Buchtitel, Jahr und Lieder in der jeweiligen Datei anpassen.
2. isDemo auf false setzen, sobald echte Inhalte eingetragen sind. Dadurch entfällt der Zusatz „Demo“ im Buchlabel.
3. Als UTF-8 speichern, die App neu bauen und installieren. JSON-Dateien sind Teil des App-Pakets; eine laufende installierte App übernimmt Änderungen im Projektordner nicht unmittelbar.

Alle Bücher werden gemeinsam durchsucht. Die Herkunft steht als Buchlabel an jedem Lied. In „Mehr“ werden alle geladenen Bücher mit Jahr und Liedanzahl angezeigt.

## Format

~~~json
{
  "id": "liederbuch_demo_1",
  "title": "Mein Liederbuch",
  "year": 2026,
  "isDemo": false,
  "songs": [
    {
      "number": "001",
      "title": "Mein erstes Lied",
      "categoryId": "lobpreis",
      "verses": [
        {
          "number": 1,
          "isRefrain": false,
          "text": "Erste Zeile
Zweite Zeile"
        },
        {
          "number": null,
          "isRefrain": true,
          "text": "Hier steht der Refrain."
        }
      ]
    }
  ]
}
~~~

- id: eindeutige, dauerhaft stabile Buch-ID. Spätere Änderungen lösen die Zuordnung bestehender Favoriten und Verlaufseinträge. Die Demo-IDs können auch nach dem Ersetzen der Inhalte beibehalten werden.
- title: angezeigter Buchname und Grundlage des Labels. Kann ohne Verlust von Favoriten geändert werden.
- year: Erscheinungsjahr, angezeigt in der Bücherübersicht.
- isDemo: optional, standardmäßig false. true ergänzt „Demo“ im Label.
- songs: beliebig viele Lieder; eine leere Liste ist erlaubt.
- number: nicht leere Zeichenfolge, beispielsweise "001" oder "12a". Muss innerhalb des Buchs eindeutig sein. Gleiche Nummern in verschiedenen Büchern sind erlaubt.
- categoryId: thematische Zuordnung. Identische Kategorie-IDs werden über alle Bücher zusammengefasst. Neue Kategorien erscheinen automatisch. Eine separate categories-Liste ist nicht erforderlich und wird nicht ausgewertet.
- verses: Strophen in Anzeigereihenfolge. isRefrain kennzeichnet einen Refrain; dessen Nummer darf null sein. Zeilenumbrüche als \n im JSON schreiben (im Beispiel oben als JSON-Escape dargestellt).

Weitere Metadaten der Originaldatei sind erlaubt und werden ignoriert. Audio wird nicht ausgewertet.

## Später weitere Bücher hinzufügen

Eine weitere JSON-Datei in Resources/Raw ablegen und ihren Dateinamen als zusätzlichen Eintrag in Resources/Raw/songbooks.json aufnehmen. Eine neue, eindeutige Buch-ID vergeben. App anschließend neu bauen.

## Prüfung und Fehler

~~~powershell
dotnet run --project Tests/iSing.Checks.csproj -p:UseSharedCompilation=false
~~~

Die Prüfungen erwarten den ausgelieferten Demo-Datenbestand (fünf Bücher, zwölf Demo-Lieder). Nach inhaltlichen Änderungen müssen diese Beispieldaten-Erwartungen gegebenenfalls angepasst werden.

Ungültiges JSON, doppelte Buch-IDs oder doppelte Liednummern innerhalb eines Buchs führen zu einer Fehlermeldung mit Dateinamen. Es wird keine unvollständige Bibliothek angezeigt. Datei korrigieren und neu bauen/installieren. Die ursprüngliche Datei wir_loben_gott_1998.json wurde unverändert übernommen.
