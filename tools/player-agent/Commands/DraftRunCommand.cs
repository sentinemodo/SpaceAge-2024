using System.CommandLine;
using SpaceAge.PlayerAgent.Configuration;
using SpaceAge.PlayerAgent.Draft;
using SpaceAge.PlayerAgent.Inference;
using SpaceAge.PlayerAgent.Paths;
using SpaceAge.PlayerAgent.Rag;
using SpaceAge.PlayerAgent.Usage;

namespace SpaceAge.PlayerAgent.Commands;

internal static class DraftRunCommand
{
    public static Command Create()
    {
        var command = new Command(
            "draft-run",
            "Queue UTF-8 order drafts for AI seats 2–11 against one Ollama host (Phase 6).");
        command.AddOption(CommandHelpers.ModeOption);
        command.AddOption(CommandHelpers.RunOption);
        command.AddOption(CommandHelpers.AllowRunPodOption);
        command.AddOption(CommandHelpers.YesOption);
        command.AddOption(CommandHelpers.DryRunOption);
        command.AddOption(CommandHelpers.TopOption);
        command.AddOption(CommandHelpers.FromFactionOption);
        command.AddOption(CommandHelpers.ToFactionOption);
        command.AddOption(CommandHelpers.TurnOption);
        command.AddOption(CommandHelpers.IterationOption);
        command.AddOption(CommandHelpers.SkipIsolationAuditOption);
        command.AddOption(CommandHelpers.NoRecordAuditOption);

        command.SetHandler(async (context) =>
        {
            var mode = CommandHelpers.ParseRequiredMode(context);
            var settings = CommandHelpers.LoadSettings(context);
            var dryRun = context.ParseResult.GetValueForOption(CommandHelpers.DryRunOption);
            var assumeYes = context.ParseResult.GetValueForOption(CommandHelpers.YesOption);
            var runId = context.ParseResult.GetValueForOption(CommandHelpers.RunOption)
                ?? throw new InvalidOperationException("--run is required for draft-run.");
            var fromFaction = context.ParseResult.GetValueForOption(CommandHelpers.FromFactionOption) ?? 2;
            var toFaction = context.ParseResult.GetValueForOption(CommandHelpers.ToFactionOption) ?? 11;
            var turnOverride = context.ParseResult.GetValueForOption(CommandHelpers.TurnOption);
            var iterationOverride = context.ParseResult.GetValueForOption(CommandHelpers.IterationOption);
            var topK = context.ParseResult.GetValueForOption(CommandHelpers.TopOption) ?? 6;
            var skipAudit = context.ParseResult.GetValueForOption(CommandHelpers.SkipIsolationAuditOption);
            var recordAudit = !context.ParseResult.GetValueForOption(CommandHelpers.NoRecordAuditOption);

            if (fromFaction is < 2 or > 11 || toFaction is < 2 or > 11 || fromFaction > toFaction)
            {
                throw new InvalidOperationException("--from-faction and --to-faction must be between 2 and 11 with from <= to.");
            }

            var repoRoot = RepoPaths.FindRepositoryRoot();
            RepoPaths.EnsureIndexLayout(settings.IndexDirectory);

            Console.WriteLine($"Run id:           {runId}");
            Console.WriteLine($"Factions:         {fromFaction}..{toFaction}");
            Console.WriteLine($"Draft mode:       {(dryRun ? "dry-run (prompt pack only)" : "full draft")}");
            Console.WriteLine($"Remote host:      {settings.IsRemoteHost}");

            if (!skipAudit)
            {
                var auditResult = IsolationAuditor.AuditRun(
                    settings.IndexDirectory,
                    runId,
                    mode,
                    repoRoot,
                    fromFaction,
                    toFaction);
                IsolationAuditor.PrintViolations(auditResult);

                if (recordAudit)
                {
                    var note = IsolationAuditor.BuildAuditNote(
                        runId,
                        mode,
                        fromFaction,
                        toFaction,
                        DateTimeOffset.UtcNow.ToString("O"),
                        auditResult);
                    var auditPath = Path.Combine(repoRoot, "play", "runs", runId, "gm", "isolation-audit.md");
                    IsolationAuditor.AppendRunAuditNote(auditPath, note);
                    Console.WriteLine($"Recorded audit: {auditPath}");
                }

                if (!auditResult.IsClean)
                {
                    throw new InvalidOperationException(
                        $"draft-run aborted: isolation audit found {auditResult.Violations.Count} violation(s).");
                }
            }

            var failures = new List<(int FactionId, string Error)>();
            var successes = new List<int>();
            var factionRange = Enumerable.Range(fromFaction, toFaction - fromFaction + 1).ToList();
            var inference = RemoteInferenceContext.Create(
                settings,
                assumeYes,
                new ConsoleUserPrompt(),
                command: "draft-run",
                runId,
                factionRange,
                dryRun);
            var service = new OrderDraftService(inference.Client, settings);
            var cancellationToken = context.GetCancellationToken();

            try
            {
                for (var factionId = fromFaction; factionId <= toFaction; factionId++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var factionDir = RepoPaths.FactionFolder(repoRoot, runId, factionId);
                    Console.WriteLine();
                    Console.WriteLine($"=== Faction {factionId} ===");
                    Console.WriteLine($"Folder:           {factionDir}");

                    if (!Directory.Exists(factionDir))
                    {
                        failures.Add((factionId, $"Faction folder not found: {factionDir}"));
                        Console.WriteLine("Skipped:          missing faction folder.");
                        continue;
                    }

                    try
                    {
                        var reportPath = FactionCorpusPaths.ReportPaths(factionDir).LastOrDefault();
                        if (reportPath is null)
                        {
                            throw new InvalidOperationException("No report.*.txt found in faction folder.");
                        }

                        var draftPath = RepoPaths.ResolveDraftOutput(
                            repoRoot,
                            outputPath: null,
                            runId,
                            factionId,
                            turnOverride,
                            iterationOverride,
                            reportPath);
                        var storyPath = FactionCorpusPaths.StoryPath(factionDir);

                        Console.WriteLine($"Draft output:     {draftPath}");
                        Console.WriteLine($"Report:           {reportPath}");
                        Console.WriteLine($"Story:            {storyPath ?? "(none)"}");

                        var request = new OrderDraftRequest
                        {
                            Mode = mode,
                            FactionId = factionId,
                            OutputPath = draftPath,
                            RunId = runId,
                            FactionDir = factionDir,
                            ReportPath = reportPath,
                            StoryPath = storyPath,
                            DryRun = dryRun,
                            TopK = topK,
                        };

                        var result = await service.DraftAsync(request, cancellationToken);
                        Console.WriteLine($"Retrieved chunks: {result.RetrievedChunks.Count}");
                        if (dryRun)
                        {
                            Console.WriteLine("Dry run prompt pack ready (no chat call).");
                        }
                        else
                        {
                            Console.WriteLine($"Wrote draft:      {result.OutputPath}");
                            Console.WriteLine($"Lint:             {(result.LintResult?.IsValid == true ? "pass" : "fail")}");
                        }

                        successes.Add(factionId);
                    }
                    catch (RunPodGuardrailException ex)
                    {
                        failures.Add((factionId, ex.Message));
                        Console.WriteLine($"Failed:           {ex.Message}");
                        throw;
                    }
                    catch (Exception ex)
                    {
                        failures.Add((factionId, ex.Message));
                        Console.WriteLine($"Failed:           {ex.Message}");
                    }
                }

                Console.WriteLine();
                Console.WriteLine($"draft-run summary: {successes.Count} succeeded, {failures.Count} failed.");
                if (failures.Count > 0)
                {
                    inference.UsageScope?.MarkError();
                    throw new InvalidOperationException(
                        $"draft-run completed with {failures.Count} failure(s): "
                        + string.Join("; ", failures.Select(f => $"faction {f.FactionId}: {f.Error}")));
                }

                if (!dryRun)
                {
                    Console.WriteLine("Reminder: UTF-8 drafts; play/turn.ps1 converts to Windows-1251 for Game.exe.");
                }

                Console.WriteLine("draft-run complete.");
            }
            catch (OperationCanceledException)
            {
                inference.UsageScope?.MarkCancelled();
                throw;
            }
            catch (RunPodGuardrailException)
            {
                throw;
            }
            catch
            {
                inference.UsageScope?.MarkError();
                throw;
            }
            finally
            {
                await inference.DisposeAsync();
                if (inference.CompletedRecord is { } record && !dryRun && settings.IsRemoteHost)
                {
                    LlmUsageNoteWriter.AppendRunNote(repoRoot, runId, record);
                    Console.WriteLine($"Recorded usage:   play/runs/{runId}/gm/llm-usage.md");
                }
            }
        });

        return command;
    }
}
