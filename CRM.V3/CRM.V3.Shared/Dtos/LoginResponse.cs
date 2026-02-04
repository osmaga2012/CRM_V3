namespace CRM.V3.Shared.Dtos
{
    public class LoginResponse
    {
        public string access_token { get; set; } = string.Empty;
        public string token_type { get; set; }
        public long expires_in { get; set; }
        public Usuario2Dto user { get; set; }
    }
}
