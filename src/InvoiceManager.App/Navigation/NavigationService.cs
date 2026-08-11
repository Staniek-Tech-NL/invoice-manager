namespace InvoiceManager.App.Navigation;

public sealed class NavigationService : INavigationService
{
    private readonly Dictionary<NavigationDestination, INavigationPage> _pages;

    public NavigationService(IEnumerable<INavigationPage> pages)
    {
        _pages = pages.ToDictionary(page => page.Destination);
        CurrentPage = GetPage(NavigationDestination.Dashboard);
    }

    public event EventHandler? CurrentPageChanged;

    public INavigationPage CurrentPage { get; private set; }

    public void NavigateTo(NavigationDestination destination)
    {
        var nextPage = GetPage(destination);

        if (ReferenceEquals(CurrentPage, nextPage))
        {
            return;
        }

        CurrentPage = nextPage;
        CurrentPageChanged?.Invoke(this, EventArgs.Empty);
    }

    private INavigationPage GetPage(NavigationDestination destination)
    {
        return _pages.TryGetValue(destination, out var page)
            ? page
            : throw new InvalidOperationException($"No page is registered for {destination}.");
    }
}
