using iSing.Services;
namespace iSing.ViewModels;

public abstract class PageViewModel : ObservableObject
{
    protected ILibraryService Library { get; }
    public string Title { get; }
    bool busy, ready;
    string error = "";
    public bool IsBusy { get => busy; private set { if (Set(ref busy, value)) Notify(nameof(IsContentVisible)); } }
    public bool IsContentVisible => ready && !IsBusy && !HasError;
    public string ErrorMessage
    {
        get => error;
        private set
        {
            if (!Set(ref error, value)) return;
            Notify(nameof(HasError));
            Notify(nameof(IsContentVisible));
        }
    }
    public bool HasError => ErrorMessage.Length > 0;
    public AsyncCommand InitializeCommand { get; }
    protected PageViewModel(string title, ILibraryService library)
    {
        Title = title;
        Library = library;
        InitializeCommand = new AsyncCommand(async _ =>
        {
            IsBusy = true;
            ErrorMessage = "";
            try
            {
                await Library.LoadAsync();
                Refresh();
                ready = true;
            }
            finally { IsBusy = false; }
        }, ShowError);
    }
    public Task InitializeAsync() => InitializeCommand.ExecuteAsync();
    protected void ShowError(Exception ex) => ErrorMessage = ex.Message;
    protected abstract void Refresh();
}
