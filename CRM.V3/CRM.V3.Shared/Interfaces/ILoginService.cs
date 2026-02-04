using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.V3.Shared.Interfaces
{
    public interface ILoginService
    {
        event Action OnLoginSuccess;
    }
}
