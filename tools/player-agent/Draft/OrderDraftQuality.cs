using System.Text.RegularExpressions;

using SpaceAge.PlayerAgent.Lint;



namespace SpaceAge.PlayerAgent.Draft;



public static partial class OrderDraftQuality

{

    private static readonly HashSet<string> PersonAllowedVerbs = new(StringComparer.OrdinalIgnoreCase)

    {

        "ACTIVE", "ALIAS", "HAS", "NAME", "SEE", "TRAIN",

    };



    private static readonly HashSet<string> OilFuelUseTechs = new(StringComparer.OrdinalIgnoreCase)

    {

        "GRNDTR", "ARMCBT", "MSRVTM", "TRUCKS", "TANKS", "SHUTTL", "ALNDRN",

    };



    private static readonly Dictionary<string, string> UseTechRequiredModule = new(StringComparer.OrdinalIgnoreCase)

    {

        ["farmng"] = "farms",

        ["hcdril"] = "sdrill",

    };



    private static readonly Dictionary<string, string> WrongItemTypeNames = new(StringComparer.OrdinalIgnoreCase)

    {

        ["titanium"] = "titani",

        ["silicium"] = "silici",

        ["uranium"] = "uran (not in Helios turn-1 catalog — omit unless report lists it)",

    };



    private static readonly HashSet<string> DeferredNestUseTechs = new(StringComparer.OrdinalIgnoreCase)

    {

        "ARMCBT",

        "MCORED",

    };



    private static readonly HashSet<string> FactoryStageItems = new(StringComparer.OrdinalIgnoreCase)

    {

        "iron", "titani", "silici", "copper", "uraniu",

    };



    public static bool IsUsable(string orderText, string? personaPreference = null, string? reportText = null)

    {

        if (string.IsNullOrWhiteSpace(orderText))

        {

            return false;

        }



        if (ContainsReportTemplateBoilerplate(orderText) || HasPersonSubjectViolations(orderText))

        {

            return false;

        }



        if (HasMoveReadinessViolations(orderText))

        {

            return false;

        }



        if (HasUseTechPlacementViolations(orderText, reportText))

        {

            return false;

        }



        if (HasInvalidItemTypeViolations(orderText))

        {

            return false;

        }



        if (HasDoubleConditionViolations(orderText))

        {

            return false;

        }



        if (HasDeferredNestGetViolations(orderText))

        {

            return false;

        }



        if (HasTravelProvisioningViolations(orderText))

        {

            return false;

        }



        if (HasFactoryPlusGetViolations(orderText, personaPreference))

        {

            return false;

        }



        if (HasMobileAtViolations(orderText, personaPreference))

        {

            return false;

        }



        if (HasSetHoldViolations(orderText))

        {

            return false;

        }



        if (HasMilitaryPersonaViolations(orderText, personaPreference))

        {

            return false;

        }



        var lines = orderText.Replace("\r\n", "\n").Split('\n');

        var moduleStacks = 0;

        var actionLines = 0;

        var hasImmediateVerb = false;

        var hasLeftoverVerb = false;



        foreach (var rawLine in lines)

        {

            var line = rawLine.Trim();

            if (line.Length == 0 || line.StartsWith(';'))

            {

                continue;

            }



            if (line.StartsWith("#modulestack", StringComparison.OrdinalIgnoreCase))

            {

                moduleStacks++;

                continue;

            }



            if (line.StartsWith('#'))

            {

                continue;

            }



            actionLines++;

            if (line.StartsWith("@", StringComparison.Ordinal))

            {

                hasLeftoverVerb = true;

            }

            else if (!line.StartsWith("active", StringComparison.OrdinalIgnoreCase)

                && !line.StartsWith("see", StringComparison.OrdinalIgnoreCase))

            {

                hasImmediateVerb = true;

            }

        }



        if (moduleStacks < 2 || actionLines < 6)

        {

            return false;

        }



        if (!hasImmediateVerb || !hasLeftoverVerb)

        {

            return false;

        }



        var upperAll = orderText.ToUpperInvariant();

        var hasTerranProduce = upperAll.Contains("@PRODUCE TERRAN", StringComparison.Ordinal)

            || upperAll.Contains("\nPRODUCE TERRAN", StringComparison.Ordinal);

        var hasCashProduce = upperAll.Contains("@PRODUCE CASH", StringComparison.Ordinal)

            || upperAll.Contains("\nPRODUCE CASH", StringComparison.Ordinal);

        var hasEnergyProduce = upperAll.Contains("@PRODUCE ENERGY", StringComparison.Ordinal);



        if (string.Equals(personaPreference, "military", StringComparison.OrdinalIgnoreCase)

            || string.Equals(personaPreference, "economic", StringComparison.OrdinalIgnoreCase))

        {

            if (!hasTerranProduce && !hasEnergyProduce)

            {

                return false;

            }

        }

        else if (!hasTerranProduce && !hasCashProduce && !hasEnergyProduce)

        {

            return false;

        }



        if (upperAll.Contains("#PERSON", StringComparison.Ordinal))

        {

            return false;

        }



        if (string.Equals(personaPreference, "researcher", StringComparison.OrdinalIgnoreCase))

        {

            var upper = orderText.ToUpperInvariant();

            if (!upper.Contains("USE ", StringComparison.Ordinal) && !upper.Contains("\nUSE ", StringComparison.Ordinal))

            {

                return false;

            }



            if (!upper.Contains("@RESEARCH", StringComparison.Ordinal))

            {

                return false;

            }



            if (!Regex.IsMatch(upper, @"\bMOVE\s+R", RegexOptions.None))

            {

                return false;

            }

        }



        if (string.Equals(personaPreference, "military", StringComparison.OrdinalIgnoreCase))

        {

            var upper = orderText.ToUpperInvariant();

            if (!upper.Contains("GRNDTR", StringComparison.Ordinal))

            {

                return false;

            }



            var armcbtUses = Regex.Matches(upper, @"\bUSE\s+ARMCBT\b", RegexOptions.None).Count;

            if (armcbtUses < 2)

            {

                return false;

            }



            if (!upper.Contains("DECLARE FACTION", StringComparison.Ordinal))

            {

                return false;

            }



            if (!Regex.IsMatch(upper, @"(@MOVE|\bMOVE\s+R|\-MOVE\s+R)", RegexOptions.None))

            {

                return false;

            }

        }



        return true;

    }



    public static bool HasMoveReadinessViolations(string orderText) =>

        DescribeMoveReadinessViolations(orderText).Count > 0;



    public static IReadOnlyList<string> DescribeMoveReadinessViolations(string orderText)

    {

        var violations = new List<string>();

        var useTechByAlias = ParseUseAsNewMap(orderText);

        var hasShipHullUse = orderText.Contains("use fustor", StringComparison.OrdinalIgnoreCase)

            || orderText.Contains("use sshull", StringComparison.OrdinalIgnoreCase)

            || orderText.Contains("use corvet", StringComparison.OrdinalIgnoreCase)

            || orderText.Contains("use frigat", StringComparison.OrdinalIgnoreCase);



        foreach (var block in ParseModuleStackBlocks(orderText))

        {

            if (!block.Lines.Any(IsMoveLine))

            {

                continue;

            }



            useTechByAlias.TryGetValue(block.StackId, out var stackTech);

            var isGrndtrPlusGetScout = stackTech is not null

                && stackTech.Equals("grndtr", StringComparison.OrdinalIgnoreCase)

                && block.Lines.Any(line => line.TrimStart().StartsWith("+get ", StringComparison.OrdinalIgnoreCase));



            if (isGrndtrPlusGetScout)

            {

                continue;

            }



            var beforeMove = TakeLinesBeforeFirstMove(block.Lines);

            var beforeMoveText = string.Join('\n', beforeMove);

            var blockText = string.Join('\n', block.Lines);

            var isHasGatedTank = block.Lines.Any(line =>

                StripOrderPrefixes(line).TrimStart().StartsWith("has ", StringComparison.OrdinalIgnoreCase));



            if (!isHasGatedTank && !ContainsGetItem(beforeMoveText, "terran") && !ContainsGetItem(blockText, "terran"))

            {

                violations.Add(

                    $"stack {block.StackId}: move requires crew — add `+get` or `-get … terran …` (disabled stacks cannot move).");

            }



            var needsOil = block.StackId.StartsWith("new", StringComparison.OrdinalIgnoreCase)

                && stackTech is not null

                && OilFuelUseTechs.Contains(stackTech.ToUpperInvariant());



            if (needsOil && !ContainsGetItem(beforeMoveText, "oil") && !ContainsGetItem(blockText, "oil"))

            {

                violations.Add(

                    $"stack {block.StackId}: move requires fuel — add `+get` or `-get … oil …` (trucks/tanks/moblab burn oil).");

            }



            foreach (var moveLine in block.Lines.Where(IsMoveLine))

            {

                if (OrbitHopRegex().IsMatch(moveLine) && !ContainsGetItem(beforeMoveText, "h2o2"))

                {

                    violations.Add(

                        $"stack {block.StackId}: surface↔orbit @move needs `@get … h2o2 …` before @move (launch surcharge).");

                }

            }

        }



        if (orderText.Contains("@jump", StringComparison.OrdinalIgnoreCase))

        {

            if (!orderText.Contains("spctrl", StringComparison.OrdinalIgnoreCase))

            {

                violations.Add("ship @jump requires a command bridge — `use spctrl as new… for <hull-id>` before @jump.");

            }



            if (!hasShipHullUse)

            {

                violations.Add("ship @jump requires a ship hull stack (e.g. `use fustor` / `use sshull`) with nested spctrl.");

            }

        }



        if (hasShipHullUse

            && orderText.Contains("@move", StringComparison.OrdinalIgnoreCase)

            && !orderText.Contains("spctrl", StringComparison.OrdinalIgnoreCase))

        {

            violations.Add(

                "ship hull @move requires a nested command bridge — `use spctrl as new… for <hull-id>` before @move (hulls cannot move without spctrl).");

        }



        foreach (var block in ParseModuleStackBlocks(orderText))

        {

            if (!block.Lines.Any(line => line.TrimStart().StartsWith("@repair", StringComparison.OrdinalIgnoreCase)))

            {

                continue;

            }



            if (!block.Lines.Any(IsMoveLine))

            {

                continue;

            }



            var moveIndex = block.Lines.ToList().FindIndex(IsMoveLine);

            var repairIndex = block.Lines.ToList().FindIndex(line =>

                line.TrimStart().StartsWith("@repair", StringComparison.OrdinalIgnoreCase));

            if (repairIndex > moveIndex)

            {

                violations.Add(

                    $"stack {block.StackId}: @repair must come before @move when fixing damage-disabled units.");

            }

        }



        return violations;

    }



    public static bool HasUseTechPlacementViolations(string orderText, string? reportText = null) =>

        DescribeUseTechPlacementViolations(orderText, reportText).Count > 0;



    public static IReadOnlyList<string> DescribeUseTechPlacementViolations(string orderText, string? reportText)

    {

        var violations = new List<string>();

        var stackTypes = ReportStackCatalog.ParseModuleTypes(reportText);

        if (stackTypes.Count == 0)

        {

            return violations;

        }



        foreach (var block in ParseModuleStackBlocks(orderText))

        {

            if (block.StackId.StartsWith("new", StringComparison.OrdinalIgnoreCase))

            {

                continue;

            }



            if (!stackTypes.TryGetValue(block.StackId, out var moduleType))

            {

                continue;

            }



            foreach (var line in block.Lines)

            {

                var tech = ExtractUseTech(line);

                if (tech is null || !UseTechRequiredModule.TryGetValue(tech, out var requiredModule))

                {

                    continue;

                }



                if (!string.Equals(moduleType, requiredModule, StringComparison.OrdinalIgnoreCase))

                {

                    violations.Add(

                        $"stack {block.StackId} ({moduleType}): @use {tech.ToLowerInvariant()} belongs on {requiredModule} stacks only — use the farms/sdrill id from the Orders template, not factory or other modules.");

                }

            }

        }



        return violations;

    }



    public static bool HasDeferredNestGetViolations(string orderText) =>

        DescribeDeferredNestGetViolations(orderText).Count > 0;



    public static bool HasDoubleConditionViolations(string orderText) =>

        DescribeDoubleConditionViolations(orderText).Count > 0;



    public static IReadOnlyList<string> DescribeDoubleConditionViolations(string orderText)

    {

        var violations = new List<string>();

        foreach (var rawLine in orderText.Replace("\r\n", "\n").Split('\n'))

        {

            var trimmed = rawLine.Trim();

            if (trimmed.StartsWith("-+", StringComparison.Ordinal))

            {

                violations.Add(

                    $"double condition `-+` on `{trimmed}` — use a single `-` child under `has`/`active`, not `-+`.");

            }

        }



        return violations;

    }



    public static IReadOnlyList<string> DescribeDeferredNestGetViolations(string orderText)

    {

        var violations = new List<string>();

        var useTechByAlias = ParseUseAsNewMap(orderText);

        var nestedForHq = ParseUseAsNewForParent(orderText);



        foreach (var block in ParseModuleStackBlocks(orderText))

        {

            if (!block.StackId.StartsWith("new", StringComparison.OrdinalIgnoreCase))

            {

                continue;

            }



            if (!useTechByAlias.TryGetValue(block.StackId, out var tech)

                || !DeferredNestUseTechs.Contains(tech.ToUpperInvariant())

                || !nestedForHq.Contains(block.StackId))

            {

                continue;

            }



            var hasGate = block.Lines.Any(line => IsHasModuleGateLine(line, tech));

            if (!hasGate)

            {

                violations.Add(

                    $"stack {block.StackId} ({tech} nests under HQ after multi-week build): add `has 1 <module>` (e.g. `has 1 tanks` for armcbt) before one-time `-get` crew/fuel lines — `has` waits for production; `active` checks full provisioning.");

            }



            foreach (var line in block.Lines)

            {

                if (!LineMentionsCrewOrFuelItem(line) || !IsGetLine(line))

                {

                    continue;

                }



                if (IsDoubleConditionGetLine(line))

                {

                    violations.Add(

                        $"stack {block.StackId}: `-+get`/`-+@get` is a double condition — use `-get` as a single child under `has`.");

                    continue;

                }



                if (IsOneTimeChildGetLine(line))

                {

                    continue;

                }



                violations.Add(

                    $"stack {block.StackId}: use one-time `-get …` under `has` for crew/fuel (not bare `get`, `@get`, or `-+@get`).");

            }



            if (hasGate)

            {

                if (block.Lines.Any(line =>

                        StripOrderPrefixes(line).TrimStart().StartsWith("@move", StringComparison.OrdinalIgnoreCase)))

                {

                    violations.Add(

                        $"stack {block.StackId}: after `has`, use `-move` (not `@move`) so travel waits until the module exists and is provisioned.");

                }



                useTechByAlias.TryGetValue(block.StackId, out var moveTech);

                if (block.Lines.Any(line =>

                    {

                        var trimmed = line.TrimStart();

                        return trimmed.StartsWith("move ", StringComparison.OrdinalIgnoreCase)

                            && !trimmed.StartsWith("-move", StringComparison.OrdinalIgnoreCase);

                    })

                    && (moveTech == null || moveTech.Equals("armcbt", StringComparison.OrdinalIgnoreCase)))

                {

                    violations.Add(

                        $"stack {block.StackId}: after `has`, use `-move <region>` (not bare `move`) to avoid disabled-move warnings before the module is ready.");

                }



                useTechByAlias.TryGetValue(block.StackId, out var blockTech);

                var wantsMove = block.Lines.Any(IsMoveLine)

                    || (blockTech != null && blockTech.Equals("armcbt", StringComparison.OrdinalIgnoreCase));

                if (wantsMove

                    && !block.Lines.Any(line => line.TrimStart().StartsWith("-move ", StringComparison.OrdinalIgnoreCase)))

                {

                    violations.Add(

                        $"stack {block.StackId}: after `has`, add `-move <region>` as a child order once `-get` provisioning is listed.");

                }



                if (block.Lines.Any(line =>

                        line.TrimStart().StartsWith("@active", StringComparison.OrdinalIgnoreCase)))

                {

                    violations.Add(

                        $"stack {block.StackId}: `@active` is unnecessary when `has` gates production — use `has 1 tanks` then `-get` provisioning.");

                }

            }

        }



        return violations;

    }



    public static bool HasFactoryPlusGetViolations(string orderText, string? personaPreference = null) =>

        DescribeFactoryPlusGetViolations(orderText, personaPreference).Count > 0;



    private static readonly HashSet<string> PlusGetAfterUseTechs = new(StringComparer.OrdinalIgnoreCase)

    {

        "GRNDTR", "ARMCBT", "MCORED",

    };



    public static IReadOnlyList<string> DescribeFactoryPlusGetViolations(string orderText, string? personaPreference = null)

    {

        var violations = new List<string>();

        if (!string.Equals(personaPreference, "military", StringComparison.OrdinalIgnoreCase)

            && !string.Equals(personaPreference, "economic", StringComparison.OrdinalIgnoreCase))

        {

            return violations;

        }



        foreach (var block in ParseModuleStackBlocks(orderText))

        {

            if (!block.Lines.Any(line => ExtractUseTech(line) is not null))

            {

                continue;

            }



            for (var i = 0; i < block.Lines.Count; i++)

            {

                var tech = ExtractUseTech(block.Lines[i]);

                if (tech is null || !PlusGetAfterUseTechs.Contains(tech.ToUpperInvariant()))

                {

                    continue;

                }



                var tail = block.Lines.Skip(i + 1).TakeWhile(line => ExtractUseTech(line) is null).ToList();

                if (!tail.Any(line => line.TrimStart().StartsWith("+get ", StringComparison.OrdinalIgnoreCase)))

                {

                    violations.Add(

                        $"stack {block.StackId}: after `{block.Lines[i].Trim()}`, add `+get` iron/titani lines — `+get` waits until cargob has stock (do not use bare `get` or `@get` on factory staging).");

                }

            }

        }



        return violations;

    }



    public static bool HasMobileAtViolations(string orderText, string? personaPreference) =>

        DescribeMobileAtViolations(orderText).Count > 0;



    public static bool HasSetHoldViolations(string orderText) =>

        DescribeSetHoldViolations(orderText).Count > 0;



    public static IReadOnlyList<string> DescribeSetHoldViolations(string orderText)

    {

        var violations = new List<string>();

        foreach (var block in ParseModuleStackBlocks(orderText))

        {

            if (!block.Lines.Any(line =>

                    line.TrimStart().StartsWith("@produce terran", StringComparison.OrdinalIgnoreCase)))

            {

                continue;

            }



            if (!block.Lines.Any(line =>

                    Regex.IsMatch(line.Trim(), @"^set\s+hold\s+20\s+terran\b", RegexOptions.IgnoreCase)))

            {

                violations.Add(

                    $"stack {block.StackId}: HQ with `@produce terran` needs `set hold 20 terran` to reserve crew for continuous operation.");

            }

        }



        return violations;

    }



    public static IReadOnlyList<string> DescribeMobileAtViolations(string orderText)

    {

        var violations = new List<string>();

        var useTechByAlias = ParseUseAsNewMap(orderText);



        foreach (var block in ParseModuleStackBlocks(orderText))

        {

            if (!block.StackId.StartsWith("new", StringComparison.OrdinalIgnoreCase))

            {

                continue;

            }



            foreach (var line in block.Lines)

            {

                var trimmed = line.TrimStart();

                if (trimmed.StartsWith("@move", StringComparison.OrdinalIgnoreCase)

                    || trimmed.StartsWith("@tactic", StringComparison.OrdinalIgnoreCase)

                    || trimmed.StartsWith("@active", StringComparison.OrdinalIgnoreCase)

                    || trimmed.StartsWith("@get", StringComparison.OrdinalIgnoreCase))

                {

                    violations.Add(

                        $"stack {block.StackId}: on mobile stacks use definite `get` / `move` / `tactic` (no `@`); `@research` is continuous like `@produce`.");

                }

            }



            if (useTechByAlias.TryGetValue(block.StackId, out var tech)

                && tech.Equals("grndtr", StringComparison.OrdinalIgnoreCase))

            {

                var hasMoveBeforePlusGet = block.Lines.Any(line =>

                        StripOrderPrefixes(line).TrimStart().StartsWith("move ", StringComparison.OrdinalIgnoreCase))

                    && block.Lines.Any(line => line.TrimStart().StartsWith("+get ", StringComparison.OrdinalIgnoreCase));

                if (!hasMoveBeforePlusGet)

                {

                    violations.Add(

                        $"stack {block.StackId}: scout truck pattern is `move <region>` then `+get` terran/oil/food (execute move, then pull when stock lands).");

                }

            }

        }



        return violations;

    }



    public static bool HasMilitaryPersonaViolations(string orderText, string? personaPreference)

    {

        if (!string.Equals(personaPreference, "military", StringComparison.OrdinalIgnoreCase))

        {

            return false;

        }



        return DescribeMilitaryPersonaViolations(orderText).Count > 0;

    }



    public static IReadOnlyList<string> DescribeMilitaryPersonaViolations(string orderText)

    {

        var violations = new List<string>();

        if (Regex.IsMatch(orderText, @"\bsell\s+\d+\s+food\b", RegexOptions.IgnoreCase))

        {

            violations.Add("military persona: do not sell food early — tanks consume 16 food per quarter; keep grant calories for the army.");

        }



        return violations;

    }



    public static bool HasTravelProvisioningViolations(string orderText) =>

        DescribeTravelProvisioningViolations(orderText).Count > 0;



    public static IReadOnlyList<string> DescribeTravelProvisioningViolations(string orderText)

    {

        var violations = new List<string>();

        var useTechByAlias = ParseUseAsNewMap(orderText);



        foreach (var block in ParseModuleStackBlocks(orderText))

        {

            if (!block.Lines.Any(IsMoveLine))

            {

                continue;

            }



            var beforeMove = TakeLinesBeforeFirstMove(block.Lines);

            var beforeMoveText = string.Join('\n', beforeMove);



            var blockText = string.Join('\n', block.Lines);

            if (useTechByAlias.TryGetValue(block.StackId, out var tech)

                && tech.Equals("armcbt", StringComparison.OrdinalIgnoreCase))

            {

                if (!Regex.IsMatch(blockText, @"-get\s+32\s+food\b", RegexOptions.IgnoreCase))

                {

                    violations.Add(

                        $"stack {block.StackId}: tank column needs `-get 32 food` (16 food per quarter × 2 quarters upkeep).");

                }



                if (!Regex.IsMatch(blockText, @"-get\s+8\s+oil\b", RegexOptions.IgnoreCase))

                {

                    violations.Add(

                        $"stack {block.StackId}: tank column needs `-get 8 oil` (4 oil per quarter × 2 quarters fuel).");

                }

            }



            foreach (var moveLine in block.Lines.Where(IsMoveLine))

            {

                if (OrbitHopRegex().IsMatch(moveLine)

                    && !ContainsGetItem(beforeMoveText, "terair")

                    && !ContainsGetItem(beforeMoveText, "h2o2"))

                {

                    violations.Add(

                        $"stack {block.StackId}: space/orbit travel needs `-get` terair (and h2o2 for surface↔orbit hops) before move.");

                }

            }

        }



        return violations;

    }



    public static bool HasFactoryMaterialStagingViolations(string orderText) =>

        DescribeFactoryMaterialStagingViolations(orderText).Count > 0;



    public static IReadOnlyList<string> DescribeFactoryMaterialStagingViolations(string orderText) =>

        Array.Empty<string>();



    public static bool HasInvalidItemTypeViolations(string orderText) =>

        DescribeInvalidItemTypeViolations(orderText).Count > 0;



    public static IReadOnlyList<string> DescribeInvalidItemTypeViolations(string orderText)

    {

        var violations = new List<string>();

        foreach (var rawLine in orderText.Replace("\r\n", "\n").Split('\n'))

        {

            var line = rawLine.Trim();

            if (line.Length == 0 || line.StartsWith(';') || line.StartsWith('#'))

            {

                continue;

            }



            if (!line.StartsWith("@get", StringComparison.OrdinalIgnoreCase)

                && !line.StartsWith("get ", StringComparison.OrdinalIgnoreCase))

            {

                continue;

            }



            foreach (var pair in WrongItemTypeNames)

            {

                if (line.Contains(pair.Key, StringComparison.OrdinalIgnoreCase))

                {

                    violations.Add(

                        $"invalid item name '{pair.Key}' in `{line}` — use catalog id `{pair.Value}`.");

                }

            }

        }



        return violations;

    }



    public static string BuildRetryInstruction(string? personaPreference, string? orderText = null, string? reportText = null)

    {

        var feedbackLines = new List<string>();

        if (orderText is not null)

        {

            feedbackLines.AddRange(DescribeMoveReadinessViolations(orderText).Select(violation => "  - " + violation));

            feedbackLines.AddRange(DescribeUseTechPlacementViolations(orderText, reportText).Select(violation => "  - " + violation));

            feedbackLines.AddRange(DescribeInvalidItemTypeViolations(orderText).Select(violation => "  - " + violation));

            feedbackLines.AddRange(DescribeDoubleConditionViolations(orderText).Select(violation => "  - " + violation));

            feedbackLines.AddRange(DescribeDeferredNestGetViolations(orderText).Select(violation => "  - " + violation));

            feedbackLines.AddRange(DescribeTravelProvisioningViolations(orderText).Select(violation => "  - " + violation));

            feedbackLines.AddRange(DescribeFactoryPlusGetViolations(orderText, personaPreference).Select(violation => "  - " + violation));

            feedbackLines.AddRange(DescribeMobileAtViolations(orderText).Select(violation => "  - " + violation));

            feedbackLines.AddRange(DescribeSetHoldViolations(orderText).Select(violation => "  - " + violation));

            feedbackLines.AddRange(DescribeMilitaryPersonaViolations(orderText).Select(violation => "  - " + violation));

        }



        var moveBlock = feedbackLines.Count > 0

            ? """

              Quality gate failures (fix before resubmitting):

              """ + string.Join(Environment.NewLine, feedbackLines) + Environment.NewLine

            : string.Empty;



        if (string.Equals(personaPreference, "researcher", StringComparison.OrdinalIgnoreCase))

        {

            return moveBlock + """

              Your previous draft was incomplete. Rewrite the full order file with at least:

              - #modulestack <factory-id> first: get iron and silici from cargob, use msrvtm as newNNN

              - economic loop: @produce cash, @use farmng, @use hcdril, @produce energy, sell food

              - HQ: `set hold 20 terran` beside `@produce terran`

              - #modulestack newNNN: get terran and oil; move <anomaly-region-id>; @research <same-region-id> (move is definite; @research is continuous)

              Disabled stacks cannot move — stage crew and fuel on the new stack before move.

              Use stack ids from the Orders template. Do not reply with only active/see lines. Include #end.

              """;

        }



        if (string.Equals(personaPreference, "military", StringComparison.OrdinalIgnoreCase))

        {

            return moveBlock + """

              Your previous draft was incomplete. Rewrite the full order file with at least:

              - HQ: `set hold 20 terran` and `@produce terran`; @get all food/carbon on cargob — **no sell food**

              - factory: `use grndtr` as new1 with `+get`, then two `use armcbt` as new2/new3 each with `+get` iron/titani

              - #modulestack new1: `move` Farm Belt, then `+get` terran/oil/food (no `@` on move/get)

              - #modulestack new2/new3: `has 1 tanks`, `-get` 16 terran / 8 oil / 32 food, `-move` Mid Vale, `tactic destroy` (no `@` on move/tactic/active). Include #end.

              """;

        }



        if (string.Equals(personaPreference, "economic", StringComparison.OrdinalIgnoreCase))

        {

            return moveBlock + """

              Your previous draft was incomplete. Rewrite the full order file with at least:

              - `@produce energy` on cplant before scaling nested cdrill draw

              - economic loop: @produce terran, @use hcdril + @use iminng, @use farmng, sell food

              - factory: one-time `get iron+titani` (no @), then `use mcored as newNNN for <hq-id>`

              - #modulestack newNNN: `has 1 <drill module>` then `-get` terran from HQ (one-time nest crew — NOT @get or `-+@get`), then deactivate 1

              Use stack ids from the Orders template. Include #end.

              """;

        }



        return moveBlock + """

            Your previous draft was incomplete. Rewrite the full order file using stack ids from the Orders template:

            - Do NOT paste template comment lines (; + …, ; items: …). Do NOT include #person CEO blocks unless TRAIN/ACTIVE/SEE.

            - @produce terran under #modulestack <hq-id> only (corphq), not under #person or cargob.

            - Use `@` only for continuous pulls (e.g. `@get all food from <farm-id>`). One-time travel/crew staging uses bare `get`/`-get`/`move`/`-move` without `@`.

            - Before any move: stage terran crew and oil fuel on that stack — disabled units cannot move.

            - factory USE lines, economic @produce/@use loop, and contract TRANSFER if the open charter applies this quarter.

            Use newN aliases for USE receivers. Do not reply with only active/see lines. Include #end.

            """;

    }



    public static bool ContainsReportTemplateBoilerplate(string orderText)

    {

        foreach (var rawLine in orderText.Replace("\r\n", "\n").Split('\n'))

        {

            var trimmed = rawLine.TrimStart();

            if (!trimmed.StartsWith(';'))

            {

                continue;

            }



            if (trimmed.StartsWith("; +", StringComparison.Ordinal)

                || trimmed.Contains('[', StringComparison.Ordinal)

                || trimmed.StartsWith("; items:", StringComparison.OrdinalIgnoreCase)

                || trimmed.StartsWith("; technologies:", StringComparison.OrdinalIgnoreCase))

            {

                return true;

            }

        }



        return false;

    }



    public static bool HasPersonSubjectViolations(string orderText)

    {

        var subjectIsPerson = false;

        foreach (var rawLine in orderText.Replace("\r\n", "\n").Split('\n'))

        {

            var line = rawLine.Trim();

            if (line.Length == 0 || line.StartsWith(';'))

            {

                continue;

            }



            if (line.StartsWith("#modulestack", StringComparison.OrdinalIgnoreCase))

            {

                subjectIsPerson = false;

                continue;

            }



            if (line.StartsWith("#person", StringComparison.OrdinalIgnoreCase))

            {

                subjectIsPerson = true;

                continue;

            }



            if (line.StartsWith('#'))

            {

                continue;

            }



            if (!subjectIsPerson)

            {

                continue;

            }



            var verb = OrderDraftLinter.ExtractVerb(line);

            if (verb.Length == 0 || PersonAllowedVerbs.Contains(verb))

            {

                continue;

            }



            return true;

        }



        return false;

    }



    private static Dictionary<string, string> ParseUseAsNewMap(string orderText)

    {

        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (Match match in UseAsNewRegex().Matches(orderText))

        {

            map[match.Groups[2].Value] = match.Groups[1].Value;

        }



        return map;

    }



    private static IEnumerable<ModuleStackBlock> ParseModuleStackBlocks(string orderText)

    {

        string? currentStack = null;

        var lines = new List<string>();



        foreach (var rawLine in orderText.Replace("\r\n", "\n").Split('\n'))

        {

            var line = rawLine.Trim();

            if (line.StartsWith("#modulestack", StringComparison.OrdinalIgnoreCase))

            {

                if (currentStack is not null)

                {

                    yield return new ModuleStackBlock(currentStack, lines);

                }



                currentStack = line["#modulestack".Length..].Trim();

                lines = new List<string>();

                continue;

            }



            if (line.StartsWith('#'))

            {

                if (currentStack is not null)

                {

                    yield return new ModuleStackBlock(currentStack, lines);

                    currentStack = null;

                    lines = new List<string>();

                }



                continue;

            }



            if (currentStack is not null && line.Length > 0 && !line.StartsWith(';'))

            {

                lines.Add(line);

            }

        }



        if (currentStack is not null)

        {

            yield return new ModuleStackBlock(currentStack, lines);

        }

    }



    private static List<string> TakeLinesBeforeFirstMove(IReadOnlyList<string> lines)

    {

        var before = new List<string>();

        foreach (var line in lines)

        {

            if (IsMoveLine(line))

            {

                break;

            }



            before.Add(line);

        }



        return before;

    }



    private static bool IsMoveLine(string line)

    {

        var trimmed = StripOrderPrefixes(line).TrimStart();

        return trimmed.StartsWith("@move", StringComparison.OrdinalIgnoreCase)

            || trimmed.StartsWith("move ", StringComparison.OrdinalIgnoreCase);

    }



    private static bool ContainsGetItem(string blockText, string itemId)

    {

        foreach (var line in blockText.Split('\n'))

        {

            var trimmed = line.TrimStart();

            if (!IsGetLine(line)

                && !trimmed.StartsWith("+get ", StringComparison.OrdinalIgnoreCase)

                && !trimmed.StartsWith("-get ", StringComparison.OrdinalIgnoreCase))

            {

                continue;

            }



            if (line.Contains(itemId, StringComparison.OrdinalIgnoreCase))

            {

                return true;

            }

        }



        return false;

    }



    private static bool IsGetLine(string line)

    {

        var trimmed = StripOrderPrefixes(line).TrimStart();

        return trimmed.StartsWith("@get", StringComparison.OrdinalIgnoreCase)

            || trimmed.StartsWith("get ", StringComparison.OrdinalIgnoreCase);

    }



    private static bool IsBareGetLine(string line)

    {

        var trimmed = line.TrimStart();

        if (IsDoubleConditionGetLine(line))

        {

            return false;

        }



        if (trimmed.StartsWith("+@get", StringComparison.OrdinalIgnoreCase)

            || trimmed.StartsWith("-@get", StringComparison.OrdinalIgnoreCase))

        {

            return false;

        }



        return trimmed.StartsWith("@get", StringComparison.OrdinalIgnoreCase)

            || (trimmed.StartsWith("get ", StringComparison.OrdinalIgnoreCase) && !trimmed.StartsWith("-get", StringComparison.OrdinalIgnoreCase));

    }



    private static bool IsDoubleConditionGetLine(string line)

    {

        var trimmed = line.TrimStart();

        return trimmed.StartsWith("-+@get", StringComparison.OrdinalIgnoreCase)

            || trimmed.StartsWith("-+get ", StringComparison.OrdinalIgnoreCase);

    }



    private static bool IsOneTimeChildGetLine(string line)

    {

        return line.TrimStart().StartsWith("-get ", StringComparison.OrdinalIgnoreCase);

    }



    private static bool IsHasModuleGateLine(string line, string tech)

    {

        var trimmed = StripOrderPrefixes(line).TrimStart();

        if (!trimmed.StartsWith("has ", StringComparison.OrdinalIgnoreCase))

        {

            return false;

        }



        if (tech.Equals("armcbt", StringComparison.OrdinalIgnoreCase))

        {

            return trimmed.Contains("tanks", StringComparison.OrdinalIgnoreCase);

        }



        return trimmed.Contains("drill", StringComparison.OrdinalIgnoreCase)

            || trimmed.Contains("mcored", StringComparison.OrdinalIgnoreCase);

    }



    private static bool IsBareFactoryMaterialGet(string line)

    {

        var trimmed = line.TrimStart();

        if (!trimmed.StartsWith("get ", StringComparison.OrdinalIgnoreCase))

        {

            return false;

        }



        if (trimmed.StartsWith('-') || trimmed.StartsWith('+'))

        {

            return false;

        }



        return FactoryStageItems.Any(item => trimmed.Contains(item, StringComparison.OrdinalIgnoreCase));

    }



    private static bool LineMentionsCrewOrFuelItem(string line) =>

        line.Contains("terran", StringComparison.OrdinalIgnoreCase)

        || line.Contains("oil", StringComparison.OrdinalIgnoreCase);



    private static bool IsActiveGateLine(string line, string stackId)

    {

        var trimmed = StripOrderPrefixes(line).TrimStart();

        if (!trimmed.StartsWith("@active", StringComparison.OrdinalIgnoreCase)

            && !trimmed.StartsWith("active ", StringComparison.OrdinalIgnoreCase))

        {

            return false;

        }



        return trimmed.Contains(stackId, StringComparison.OrdinalIgnoreCase);

    }



    private static string StripOrderPrefixes(string line)

    {

        var trimmed = line.TrimStart();

        while (trimmed.StartsWith('+') || trimmed.StartsWith('-'))

        {

            trimmed = trimmed[1..].TrimStart();

        }



        return trimmed;

    }



    private static HashSet<string> ParseUseAsNewForParent(string orderText)

    {

        var nested = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (Match match in UseAsNewForParentRegex().Matches(orderText))

        {

            nested.Add(match.Groups[2].Value);

        }



        return nested;

    }



    private static string? ExtractUseTech(string line)

    {

        var trimmed = line.Trim();

        if (trimmed.StartsWith("@use ", StringComparison.OrdinalIgnoreCase))

        {

            trimmed = trimmed[1..];

        }



        if (!trimmed.StartsWith("use ", StringComparison.OrdinalIgnoreCase))

        {

            return null;

        }



        var rest = trimmed["use ".Length..].TrimStart();

        var token = rest.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

        return string.IsNullOrWhiteSpace(token) ? null : token.Trim();

    }



    private sealed record ModuleStackBlock(string StackId, IReadOnlyList<string> Lines);



    [GeneratedRegex(@"\buse\s+(\w+)\s+as\s+(new\d+)", RegexOptions.IgnoreCase)]

    private static partial Regex UseAsNewRegex();



    [GeneratedRegex(@"\buse\s+(\w+)\s+as\s+(new\d+)\s+for\s+\d+", RegexOptions.IgnoreCase)]

    private static partial Regex UseAsNewForParentRegex();



    [GeneratedRegex(@"\b(O\d+|P\d+|M\d+)\b", RegexOptions.IgnoreCase)]

    private static partial Regex OrbitHopRegex();

}


