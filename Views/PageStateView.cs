using System.Windows.Input;
namespace iSing.Views;

// Zeigt Lade-, Fehler- und Inhaltszustand einer Seite an.
// Bewusst ein ContentView und kein ControlTemplate: die Sichtbarkeit wird hier im Code
// gesetzt statt über Bindungen im Template. Damit können sich die drei Zustände nicht
// überlagern, und die Bindungssemantik innerhalb von ControlTemplates entfällt.
[ContentProperty(nameof(PageContent))]
public sealed class PageStateView : ContentView
{
    public static readonly BindableProperty PageContentProperty = BindableProperty.Create(
        nameof(PageContent), typeof(View), typeof(PageStateView), propertyChanged: OnPageContentChanged);
    public static readonly BindableProperty IsBusyProperty = BindableProperty.Create(
        nameof(IsBusy), typeof(bool), typeof(PageStateView), false, propertyChanged: OnStateChanged);
    public static readonly BindableProperty ErrorMessageProperty = BindableProperty.Create(
        nameof(ErrorMessage), typeof(string), typeof(PageStateView), "", propertyChanged: OnStateChanged);
    public static readonly BindableProperty IsContentVisibleProperty = BindableProperty.Create(
        nameof(IsContentVisible), typeof(bool), typeof(PageStateView), false, propertyChanged: OnStateChanged);
    public static readonly BindableProperty RetryCommandProperty = BindableProperty.Create(
        nameof(RetryCommand), typeof(ICommand), typeof(PageStateView), propertyChanged: OnRetryCommandChanged);

    // Der eigentliche Seiteninhalt. Dank ContentProperty landet das XAML-Kind der Seite hier.
    public View? PageContent { get => (View?)GetValue(PageContentProperty); set => SetValue(PageContentProperty, value); }
    public bool IsBusy { get => (bool)GetValue(IsBusyProperty); set => SetValue(IsBusyProperty, value); }
    public string ErrorMessage { get => (string)GetValue(ErrorMessageProperty) ?? ""; set => SetValue(ErrorMessageProperty, value); }
    public bool IsContentVisible { get => (bool)GetValue(IsContentVisibleProperty); set => SetValue(IsContentVisibleProperty, value); }
    public ICommand? RetryCommand { get => (ICommand?)GetValue(RetryCommandProperty); set => SetValue(RetryCommandProperty, value); }

    readonly ContentView host = new();
    readonly VerticalStackLayout busyLayer;
    readonly VerticalStackLayout errorLayer;
    readonly ActivityIndicator indicator;
    readonly Label errorDetail;
    readonly Button retryButton;

    public PageStateView()
    {
        indicator = new ActivityIndicator { HorizontalOptions = LayoutOptions.Start };
        var busyTitle = new Label { Text = "Lieder werden geladen …" };
        busyTitle.SetDynamicResource(StyleProperty, "HeadlineLabel");
        busyLayer = new VerticalStackLayout { Padding = 20, Spacing = 14, IsVisible = false };
        busyLayer.Add(busyTitle);
        busyLayer.Add(indicator);

        var errorTitle = new Label { Text = "Die Ansicht konnte nicht geladen werden." };
        errorTitle.SetDynamicResource(StyleProperty, "ErrorHeadlineLabel");
        errorDetail = new Label();
        errorDetail.SetDynamicResource(StyleProperty, "SmallLabel");
        retryButton = new Button { Text = "Erneut versuchen", HorizontalOptions = LayoutOptions.Start };
        errorLayer = new VerticalStackLayout { Padding = 20, Spacing = 14, IsVisible = false };
        errorLayer.Add(errorTitle);
        errorLayer.Add(errorDetail);
        errorLayer.Add(retryButton);

        var root = new Grid();
        root.Add(host);
        root.Add(busyLayer);
        root.Add(errorLayer);
        Content = root;
        UpdateState();
    }

    static void OnPageContentChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((PageStateView)bindable).host.Content = newValue as View;
    static void OnStateChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((PageStateView)bindable).UpdateState();
    static void OnRetryCommandChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((PageStateView)bindable).retryButton.Command = newValue as ICommand;

    // Feste Rangfolge Fehler > Laden > Inhalt, damit nie zwei Ebenen gleichzeitig sichtbar sind.
    void UpdateState()
    {
        var hasError = !string.IsNullOrEmpty(ErrorMessage);
        var isBusy = !hasError && IsBusy;
        errorLayer.IsVisible = hasError;
        errorDetail.Text = ErrorMessage;
        busyLayer.IsVisible = isBusy;
        indicator.IsRunning = isBusy;
        host.IsVisible = !hasError && !isBusy && IsContentVisible;
    }
}
