namespace CustomerLedger.Application.Exceptions;

/// <summary>
/// Thrown when an operation violates a documented business rule (e.g. duplicate branch
/// code, negative credit limit). Controllers catch this and surface ex.Message as a
/// user-safe validation error — never a raw exception or stack trace.
/// </summary>
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message)
    {
    }
}
