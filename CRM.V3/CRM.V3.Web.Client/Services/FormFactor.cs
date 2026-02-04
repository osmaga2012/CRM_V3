

using CRM.V3.Shared.Services;

namespace CRM.V3.Web.Client.Services
{
    public class FormFactor : IFormFactor
    {
        public bool IsNativeApp => false;

        public string GetFormFactor()
        {
            return "WebAssembly";
        }

        public string GetPlatform()
        {
            return Environment.OSVersion.ToString();
        }
    }
}
