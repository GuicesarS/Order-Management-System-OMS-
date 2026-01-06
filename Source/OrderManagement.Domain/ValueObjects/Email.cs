using System.Net.Mail;

namespace OrderManagement.Domain.ValueObjects;

public sealed class Email
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.");
        // add DomainException later

        try
        {
            var mailAddress = new MailAddress(email);

            return new Email(mailAddress.Address.ToLowerInvariant());
        }
        catch (FormatException)
        {
            throw new ArgumentException("Email format is invalid.");
            // add DomainException later
        }
    }

    public override string ToString() => Value;
}