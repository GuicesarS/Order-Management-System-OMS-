
using System;
namespace OrderManagement.Domain.Exception;

public class DomainValidationException : System.Exception
{
    public DomainValidationException(string? message) : base(message) { }
}
