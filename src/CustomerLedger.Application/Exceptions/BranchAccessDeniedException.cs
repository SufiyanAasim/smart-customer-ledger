namespace CustomerLedger.Application.Exceptions;

/// <summary>
/// Thrown when a service detects that the current user's branch does not match the branch
/// that owns the requested record — e.g. a Staff user editing another branch's customer by
/// tampering with a URL id. Controllers translate this into a 403 result.
/// </summary>
public class BranchAccessDeniedException : Exception
{
    public BranchAccessDeniedException(string message) : base(message)
    {
    }
}
