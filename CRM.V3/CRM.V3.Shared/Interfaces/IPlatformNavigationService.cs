namespace CRM.V3.Shared.Interfaces
{
    public interface IPlatformNavigationService
    {
        void NavigateToNativePage(string route);
        Task NavigateToAsync(string route);
    }
}
