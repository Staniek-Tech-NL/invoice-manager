namespace InvoiceManager.App.Navigation;

public interface IActivatableNavigationPage : INavigationPage
{
    Task ActivateAsync(CancellationToken cancellationToken = default);
}
