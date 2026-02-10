namespace CRM.V3.Shared.Services
{
    public interface IFormFactor
    {
        bool IsNativeApp { get; }
        public string GetFormFactor();
        public string GetPlatform();
    }
}
