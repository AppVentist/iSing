using iSing.Services;
namespace iSing.ViewModels;

public sealed record BookListItem(string Id, string Label, string Caption);
public sealed class BooksViewModel : PageViewModel
{
    IReadOnlyList<BookListItem> books = [];
    BookListItem? selectedBook;
    public IReadOnlyList<BookListItem> Books { get => books; private set => Set(ref books, value); }
    public BookListItem? SelectedBook { get => selectedBook; set => Set(ref selectedBook, value); }
    public AsyncCommand OpenBookCommand { get; }
    public BooksViewModel(ILibraryService library, INavigationService navigation) : base("Liederbücher", library)
    {
        OpenBookCommand = new(async parameter =>
        {
            var book = parameter as BookListItem;
            SelectedBook = null;
            if (book == null) return;
            await navigation.ShowSongsAsync(new(book.Label, BookId: book.Id));
        }, ShowError);
    }
    protected override void Refresh() => Books = Library.Books
        .Select(book => new BookListItem(book.Id, book.Label,
            $"{book.Year} · {book.Songs.Count} {(book.Songs.Count == 1 ? "Lied" : "Lieder")}"))
        .ToList();
}
