using SpaceAge.PlayerAgent.Configuration;
using SpaceAge.PlayerAgent.Usage;

namespace SpaceAge.PlayerAgent.Inference;

public sealed class GuardedOllamaClient : IOllamaClient, IDisposable
{
    private static readonly SemaphoreSlim RemoteConcurrency = new(1, 1);

    private readonly OllamaClient _inner;
    private readonly PlayerAgentSettings _settings;
    private readonly RunPodBudgetSettings _budget;
    private readonly UsageLedger _ledger;
    private readonly UsageSessionManager? _usageManager;
    private readonly RunPodGuardrails _guardrails;
    private readonly UsageSessionScope? _usageScope;

    public GuardedOllamaClient(
        OllamaClient inner,
        PlayerAgentSettings settings,
        RunPodBudgetSettings budget,
        UsageLedger ledger,
        UsageSessionManager? usageManager,
        RunPodGuardrails guardrails,
        UsageSessionScope? usageScope)
    {
        _inner = inner;
        _settings = settings;
        _budget = budget;
        _ledger = ledger;
        _usageManager = usageManager;
        _guardrails = guardrails;
        _usageScope = usageScope;
    }

    public async Task SmokeTestAsync(CancellationToken cancellationToken = default)
    {
        var chat = await ChatAsync("Reply with exactly: ok", cancellationToken);
        if (string.IsNullOrWhiteSpace(chat))
        {
            throw new InvalidOperationException("Chat smoke test returned an empty response.");
        }

        var embedding = await EmbedAsync("MOVE order syntax", cancellationToken);
        if (embedding.Length == 0)
        {
            throw new InvalidOperationException("Embedding smoke test returned an empty response.");
        }
    }

    public Task<string> ChatAsync(string userPrompt, CancellationToken cancellationToken = default) =>
        ChatAsync(userPrompt, systemPrompt: null, cancellationToken);

    public async Task<string> ChatAsync(
        string userPrompt,
        string? systemPrompt,
        CancellationToken cancellationToken = default)
    {
        if (!_settings.IsRemoteHost)
        {
            _usageManager?.RecordChatCall();
            return await _inner.ChatAsync(userPrompt, systemPrompt, cancellationToken);
        }

        await RemoteConcurrency.WaitAsync(cancellationToken);
        try
        {
            AssertAllowed(isChatCall: true);
            _usageManager?.RecordChatCall();
            return await _inner.ChatAsync(userPrompt, systemPrompt, cancellationToken);
        }
        finally
        {
            RemoteConcurrency.Release();
        }
    }

    public async Task<float[]> EmbedAsync(string input, CancellationToken cancellationToken = default)
    {
        if (!_settings.IsRemoteHost)
        {
            _usageManager?.RecordEmbedCall();
            return await _inner.EmbedAsync(input, cancellationToken);
        }

        await RemoteConcurrency.WaitAsync(cancellationToken);
        try
        {
            AssertAllowed(isChatCall: false);
            _usageManager?.RecordEmbedCall();
            return await _inner.EmbedAsync(input, cancellationToken);
        }
        finally
        {
            RemoteConcurrency.Release();
        }
    }

    private void AssertAllowed(bool isChatCall)
    {
        var active = _usageManager?.GetActiveSession();
        if (active is null)
        {
            return;
        }

        var violation = _guardrails.AssertBeforeInferenceCall(_settings, _budget, _ledger, active, isChatCall);
        if (violation is null)
        {
            return;
        }

        _usageScope?.MarkStopReason(violation.StopReason);
        throw new RunPodGuardrailException(violation);
    }

    public void Dispose() => _inner.Dispose();
}

public sealed class RunPodGuardrailException : Exception
{
    public RunPodGuardrailException(GuardrailViolation violation)
        : base(RunPodGuardrails.FormatGuardrailFailure(violation))
    {
        Violation = violation;
    }

    public GuardrailViolation Violation { get; }
}
