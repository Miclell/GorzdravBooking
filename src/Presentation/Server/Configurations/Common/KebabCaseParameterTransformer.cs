namespace Server.Configurations.Common;

public partial class KebabCaseParameterTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value)
    {
        return value == null
            ? null 
            : MyRegex()
                .Replace(value.ToString() 
                         ?? string.Empty, "$1-$2")
                .ToLower();
    }

    [System.Text.RegularExpressions.GeneratedRegex("([a-z])([A-Z])")]
    private static partial System.Text.RegularExpressions.Regex MyRegex();
}