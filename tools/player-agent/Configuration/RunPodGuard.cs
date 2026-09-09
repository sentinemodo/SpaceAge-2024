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
                + "Pass --allow-runpod or set PLAYER_AGENT_ALLOW_RUNPOD=1. "
                + "Full usage ledger and budget guardrails land in Phases 7–8; "
                + "Phase 1B adds thin start/stop warnings.");
        }

        Console.Error.WriteLine(
            "Warning: report text may leave this machine on a remote Ollama host. "
            + "Strip faction passwords from prompts; stop the pod when idle.");
    }
}
