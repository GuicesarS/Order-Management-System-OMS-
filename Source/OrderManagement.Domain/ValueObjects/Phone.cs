using OrderManagement.Domain.Exception;
using System.Text.RegularExpressions;

namespace OrderManagement.Domain.ValueObjects;

public partial class Phone
{
    public string Value { get; private set; } = string.Empty;
    
    protected Phone() { } // EF Core
    
    private Phone(string value)
    {
        Validate(value);
        Value = value;
    }

    public static Phone Create(string value)
      => new Phone(value);

    [GeneratedRegex(@"^\d{13}$")]
    private static partial Regex ThirteenDigitsRegex();

    private static void Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainValidationException("Phone is required.");

        if (!ThirteenDigitsRegex().IsMatch(value))
            throw new DomainValidationException("Phone format is invalid.");
    }

    public override string ToString() => Value;

}
