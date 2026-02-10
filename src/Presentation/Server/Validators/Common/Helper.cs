namespace Server.Validators.Common;

public static class Helper
{
    public static bool ContainDigit(string value)
    {
        return value?.Any(char.IsDigit) == true;
    }

    public static bool ContainLetter(string value)
    {
        return value?.Any(char.IsLetter) == true;
    }
}