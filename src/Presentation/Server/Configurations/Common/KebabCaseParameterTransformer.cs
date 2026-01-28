using System.Text.RegularExpressions;

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

    [GeneratedRegex("([a-z])([A-Z])")]
    private static partial Regex MyRegex();
}