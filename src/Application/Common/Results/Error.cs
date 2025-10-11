using Application.Common.Enums;

namespace Application.Common.Results;

public record Error(string Code, string Description, ErrorType Type)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);
    public static readonly Error NullValue = new(
        "General.Null", 
        "Null value was provided", 
        ErrorType.Failure);

    public Dictionary<string, object>? Metadata { get; init; }

    public static Error Validation(string code, string description) => 
        new(code, description, ErrorType.Validation);

    public static Error NotFound(string code, string description) => 
        new(code, description, ErrorType.NotFound);

    public static Error Conflict(string code, string description) => 
        new(code, description, ErrorType.Conflict);
    
    public static Error Failure(string code, string description) =>
        new(code, description, ErrorType.Failure);

    public static Error Unauthorized(string code, string description) => 
        new(code, description, ErrorType.Unauthorized);

    public static Error Forbidden(string code, string description) => 
        new(code, description, ErrorType.Forbidden);

    public Error WithMetadata(string key, object value)
    {
        var metadata = Metadata ?? new Dictionary<string, object>();
        metadata[key] = value;
        
        return this with { Metadata = metadata };
    }

    public override string ToString() => $"{Code}: {Description}";
}