namespace StatefulMenu.Core.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class InputFieldAttribute(string displayName) : Attribute
{
    public string DisplayName { get; } = displayName;
    public bool IsRequired { get; set; } = true;
    public string? Pattern { get; set; }
    public string? ErrorMessage { get; set; }
    public int Order { get; set; }
    public Type[] Validators { get; set; } = [];
    public Type[] Converters { get; set; } = [];
}