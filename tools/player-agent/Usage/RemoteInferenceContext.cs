using SpaceAge.PlayerAgent.Configuration;
using SpaceAge.PlayerAgent.Inference;

namespace SpaceAge.PlayerAgent.Usage;

public sealed class RemoteInferenceContext : IAsyncDisposable
{
    private readonly UsageSessionScope? _usageScope;

    private RemoteInferenceContext(
        IOllamaClient client,
        UsageSessionScope? usageScope,
        IDisposable? disposableClient)
    {
        Client = client;
        _usageScope = usageScope;
        DisposableClient = disposableClient;
    }

    public IOllamaClient Client { get; }

    private IDisposable? DisposableClient { get; }

    public static RemoteInferenceContext Create(
        PlayerAgentSettings settings,
        bool assumeYes,
        IUserPrompt prompt,
        string command,
        string? runId,
        IReadOnlyList<int>? factionIds,
        bool dryRun,
        HttpClient? httpClient = null)
    {
        var ledger = new UsageLedger(settings.IndexDirectory);
        ledger.EnsureLayout();
        var manager = new UsageSessionManager(ledger);
        var guardrails = new RunPodGuardrails();
        var budget = RunPodBudgetSettings.LoadFromEnvironment(settings.IsRemoteHost);

        UsageSessionScope? usageScope = UsageSessionScope.BeginRemote(
            settings,
            ledger,
            manager,
            guardrails,
            budget,
            assumeYes,
            prompt,
            command,
            runId,
            factionIds,
            dryRun);

        if (usageScope is not null)
        {
            var client = usageScope.CreateClient(settings, budget, ledger, guardrails, httpClient);
            return new RemoteInferenceContext(client, usageScope, client);
        }

        var plain = new OllamaClient(settings, httpClient);
        return new RemoteInferenceContext(plain, usageScope: null, plain);
    }

    public UsageSessionScope? UsageScope => _usageScope;

    public UsageSessionRecord? CompletedRecord => _usageScope?.CompletedRecord;

    public ValueTask DisposeAsync()
    {
        DisposableClient?.Dispose();
        return _usageScope?.DisposeAsync() ?? ValueTask.CompletedTask;
    }
}
