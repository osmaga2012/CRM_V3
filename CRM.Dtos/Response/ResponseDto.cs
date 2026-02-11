namespace CRM.Dtos.Response;

public class ResponseDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public object? Data { get; set; }
}
