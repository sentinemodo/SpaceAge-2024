namespace SpaceAge.PlayerAgent.Configuration;

public static class RunPodGuard
{
    public static void EnsureRemoteAllowed(PlayerAgentSettings settings)
    {
        if (!settings.IsRemoteHost)
        {
            return;
        }

        if (!settings.AllowRunPod)
        {
            throw new InvalidOperationException(
                "Remote Ollama host detected (RunPod or non-local URL). "
                + "Pass --allow-runpod or set PLAYER_AGENT_ALLOW_RUNPOD=1.");
        }

        Console.Error.WriteLine(
            "Warning: report text may leave this machine on a remote Ollama host. "
            + "Strip faction passwords from prompts; set PLAYER_AGENT_BUDGET_USD before remote work; "
            + "usage is tracked under .data/usage/; stop the pod when idle.");
    }
}
