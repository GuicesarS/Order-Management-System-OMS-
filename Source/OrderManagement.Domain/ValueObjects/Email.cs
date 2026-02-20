using OrderManagement.Domain.Exception;
using System.Net.Mail;

namespace OrderManagement.Domain.ValueObjects;

public partial class Email
{
    public string Value { get; private set; } = string.Empty;
    
    protected Email() { } // EF Core
    
    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainValidationException("Email is required.");

        try
        {
            var mailAddress = new MailAddress(email);

            return new Email(mailAddress.Address.ToLowerInvariant());
        }
        catch (FormatException)
        {
            throw new DomainValidationException("Email format is invalid.");
        }
    }

    public override string ToString() => Value;
}