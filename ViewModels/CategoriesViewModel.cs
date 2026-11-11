using iSing.Services;
namespace iSing.ViewModels;

// Eine Kategorie gehört immer zu einem Liederbuch. BookId und Id zusammen identifizieren sie eindeutig.
public sealed record CategoryItem(string BookId, string Id, string Name, int Count, int SortOrder)
{
    public string Caption => $"{Name}     ·     {Count} {(Count == 1 ? "Lied" : "Lieder")}   ›";
}
public sealed record CategoryGroup(string BookId, string BookLabel, IReadOnlyList<CategoryItem> Categories);

// Eintrag der Liederbuch-Leiste. Group == null steht für "Alle Liederbücher".
// (Eigener Name, weil BookFilterItem bereits vom SongsViewModel belegt ist.)
public sealed class CategoryBookFilter(string label, CategoryGroup? group)
{
    public string Label { get; } = label;
    public CategoryGroup? Group { get; } = group;
}

public sealed class CategoriesViewModel : PageViewModel
{
    List<CategoryGroup> allGroups = [];
    IReadOnlyList<CategoryGroup> groups = [];
    IReadOnlyList<CategoryBookFilter> bookFilters = [];
    CategoryBookFilter? selectedBookFilter;

    // Sichtbare Gruppen: alle oder nur das gewählte Liederbuch.
    public IReadOnlyList<CategoryGroup> Groups { get => groups; private set { if (Set(ref groups, value)) Notify(nameof(HasNoCategories)); } }
    public bool HasNoCategories => Groups.Count == 0;

    public IReadOnlyList<CategoryBookFilter> BookFilters { get => bookFilters; private set => Set(ref bookFilters, value); }
    // Die Leiste erscheint erst ab zwei Liederbüchern mit Kategorien.
    public bool ShowBookFilter => allGroups.Count > 1;
    public CategoryBookFilter? SelectedBookFilter
    {
        get => selectedBookFilter;
        set { if (Set(ref selectedBookFilter, value ?? BookFilters.FirstOrDefault())) ApplyBookFilter(); }
    }

    public AsyncCommand OpenCategoryCommand { get; }
    public CategoriesViewModel(ILibraryService library, INavigationService navigation) : base("Nach Kategorien", library)
    {
        OpenCategoryCommand = new(parameter => parameter is CategoryItem category
            ? navigation.ShowSongsAsync(new(category.Name, SongListKind.Category, category.Id, category.BookId))
            : Task.CompletedTask, ShowError);
    }

    protected override void Refresh()
    {
        SetGroups(Library.Books.Select(book =>
        {
            var counts = book.Songs.GroupBy(s => s.CategoryId).ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);
            var items = book.Categories
                .Where(category => counts.ContainsKey(category.Id))
                .Select(category => new CategoryItem(book.Id, category.Id, category.Name, counts[category.Id], category.SortOrder))
                .OrderBy(item => item.SortOrder).ThenBy(item => item.Name, StringComparer.CurrentCulture)
                .ToList();
            return new CategoryGroup(book.Id, book.Label, items);
        }).Where(group => group.Categories.Count > 0));
    }

    void SetGroups(IEnumerable<CategoryGroup> loaded)
    {
        var previousBookId = selectedBookFilter?.Group?.BookId;
        allGroups = loaded.ToList();

        var filters = new List<CategoryBookFilter> { new("Alle Liederbücher", null) };
        filters.AddRange(allGroups.Select(group => new CategoryBookFilter(group.BookLabel, group)));
        BookFilters = filters;

        // Auswahl nach einem Reload beibehalten, sonst "Alle Liederbücher".
        selectedBookFilter = filters.FirstOrDefault(f => f.Group?.BookId == previousBookId) ?? filters[0];
        Notify(nameof(SelectedBookFilter));
        Notify(nameof(ShowBookFilter));
        ApplyBookFilter();
    }

    void ApplyBookFilter() =>
        Groups = selectedBookFilter?.Group is { } group ? [group] : allGroups.ToList();
}
