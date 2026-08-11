using System.Net.Mail;

namespace InvoiceManager.Domain.Common;

internal static class TextRules
{
    public static string Required(string? value, string parameterName, int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A value is required.", parameterName);
        }

        var normalized = value.Trim();
        EnsureMaximumLength(normalized, parameterName, maximumLength);
        return normalized;
    }

    public static string? Optional(string? value, string parameterName, int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        EnsureMaximumLength(normalized, parameterName, maximumLength);
        return normalized;
    }

    public static string? Email(string? value, string parameterName)
    {
        var normalized = Optional(value, parameterName, 254);

        if (normalized is not null && !MailAddress.TryCreate(normalized, out _))
        {
            throw new ArgumentException("Enter a valid email address.", parameterName);
        }

        return normalized;
    }

    private static void EnsureMaximumLength(string value, string parameterName, int maximumLength)
    {
        if (value.Length > maximumLength)
        {
            throw new ArgumentException($"The value cannot exceed {maximumLength} characters.", parameterName);
        }
    }
}
