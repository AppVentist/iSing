using System.Text.Json.Serialization;
namespace iSing.Models;

public sealed class SongBook
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public int Year { get; set; }
    public bool IsDemo { get; set; }
    public List<SongCategory> Categories { get; set; } = [];
    public List<Song> Songs { get; set; } = [];
    [JsonIgnore] public string Label => IsDemo ? $"{Title} · Demo" : Title;
}
// Kategorien gelten je Liederbuch. Die Id ist nur innerhalb ihres Buchs eindeutig.
public sealed class SongCategory
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string IconKey { get; set; } = "";
    public int SortOrder { get; set; }
}
public sealed class Song
{
    public const string WithoutCategory = "Ohne Kategorie";
    public string Number { get; set; } = "";
    public string Title { get; set; } = "";
    public string CategoryId { get; set; } = "";
    public List<Verse> Verses { get; set; } = [];
    [JsonIgnore] public string BookId { get; internal set; } = "";
    [JsonIgnore] public string BookLabel { get; internal set; } = "";
    // Wird beim Laden aus den Kategorien des Buchs aufgelöst, nicht aus der Id abgeleitet.
    [JsonIgnore] public string CategoryName { get; internal set; } = WithoutCategory;
    [JsonIgnore] public string Key => MakeKey(BookId, Number);
    public static string MakeKey(string bookId, string number) => $"{Uri.EscapeDataString(bookId)}:{Uri.EscapeDataString(number)}";
    public string Caption => $"{Number}   ·   {CategoryName}";
}
public sealed class Verse
{
    public int? Number { get; set; }
    public bool IsRefrain { get; set; }
    public string Text { get; set; } = "";
}

public record DisplayVerse(string Label, string Text, bool IsRefrain);

// Refrain wird in der Anzeige immer nach dem letzten Vers angezeigt, egal an welcher Stelle er im Lied steht.
public static class SongFormatter
{
    public static List<DisplayVerse> BuildDisplay(IEnumerable<Verse> verses)
    {
        var refrain = verses.FirstOrDefault(v => v.IsRefrain);
        var strophen = verses.Where(v => !v.IsRefrain).OrderBy(v => v.Number);

        var result = new List<DisplayVerse>();
        foreach (var s in strophen)
        {
            result.Add(new DisplayVerse($"{s.Number}.", s.Text, false));
            if (refrain is not null)
                result.Add(new DisplayVerse("Refrain", refrain.Text, true));
        }
        return result;
    }
}
