namespace Core.Models;

public class ApiResponse<T>
{
    public T? Result { get; set; }
    public bool Success { get; set; }
    public int ErrorCode { get; set; }
    public string? Message { get; set; }
    public string? StackTrace { get; set; }
}