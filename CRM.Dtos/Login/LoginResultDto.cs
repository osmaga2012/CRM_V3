namespace CRM.Dtos.Login;

public class LoginResultDto
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public string? access_token { get; set; }
}
