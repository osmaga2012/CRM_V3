using System;
using CRM.Dtos;

namespace CRM.V3.Shared.Services
{
    public class CofradiaState
    {
        private CofradiasDto? _cofradia;

        public CofradiasDto? Cofradia => _cofradia;

        public event Action? OnChange;

        public void SetCofradia(CofradiasDto? cofradia)
        {
            _cofradia = cofradia;
            NotifyStateChanged();
        }

        public void Clear()
        {
            _cofradia = null;
            NotifyStateChanged();
        }

        public void NotifyStateChanged() => OnChange?.Invoke();
    }
}
