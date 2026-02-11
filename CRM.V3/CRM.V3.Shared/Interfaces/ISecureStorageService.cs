using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.V3.Shared.Interfaces
{
    public interface ISecureStorageService
    {
        Task SaveTokenAsync(string token);
        void RemoveTokenAsync();
        Task<string?> GetTokenAsync();
    }
}
