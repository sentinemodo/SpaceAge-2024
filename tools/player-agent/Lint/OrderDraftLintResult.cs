namespace SpaceAge.PlayerAgent.Lint;

public sealed class OrderDraftLintResult
{
    public OrderDraftLintResult(bool isValid, IReadOnlyList<string> errors)
    {
        IsValid = isValid;
        Errors = errors;
    }

    public bool IsValid { get; }
    public IReadOnlyList<string> Errors { get; }
}
