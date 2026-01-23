using OrderManagement.Domain.Exception;
using System.Text.RegularExpressions;

namespace OrderManagement.Domain.ValueObjects;

public sealed class Phone
{
    public string Value { get;}

    private Phone(string value)
    {
        Validate(value);
        Value = value;
    }

    public static Phone Create(string value)
      => new Phone(value);

    private static void Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainValidationException("Phone is required.");

        if (!Regex.IsMatch(value, @"^\d{13}$"))
            throw new DomainValidationException("Phone format is invalid.");
    }

    public override string ToString()
        => Value;
}
