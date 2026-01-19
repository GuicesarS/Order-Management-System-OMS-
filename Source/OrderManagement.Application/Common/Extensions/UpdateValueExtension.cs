namespace OrderManagement.Application.Common.Extensions;

public static class UpdateValueExtension
{
    public static string? GetValueForUpdate(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Equals("string", StringComparison.OrdinalIgnoreCase))
            return null;

        return value;
    }
}
