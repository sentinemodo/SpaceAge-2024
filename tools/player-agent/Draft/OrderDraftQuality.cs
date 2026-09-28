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



        if (HasMoveRegionReachabilityViolations(orderText, reportText))

        {

            return false;

        }



        if (HasGrantTargetViolations(orderText, reportText))

        {

            return false;

        }



        if (HasWindGrantDrillViolations(orderText, reportText, personaPreference))

        {

            return false;

        }



        if (HasDrillUseResourceViolations(orderText, reportText))

        {

            return false;

        }



        if (HasCombinedDrillUseViolations(orderText))

        {

            return false;

        }



        if (HasEnergyStagingViolations(orderText, reportText))

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



        if (HasMilitaryPersonaViolations(orderText, personaPreference, reportText))

        {

            return false;

        }



        if (HasAbsentPlayerPersonaViolations(orderText, personaPreference, reportText))

        {

            return false;

        }



        if (HasEconomicPersonaViolations(orderText, personaPreference, reportText))

        {

            return false;

        }



        if (HasResearcherPersonaViolations(orderText, personaPreference, reportText))

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



        if (string.Equals(personaPreference, "absent-player", StringComparison.OrdinalIgnoreCase))

        {

            if (!hasCashProduce || !hasEnergyProduce)

            {

                return false;

            }

        }

        else if (string.Equals(personaPreference, "military", StringComparison.OrdinalIgnoreCase)

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



    public static bool HasMoveRegionReachabilityViolations(string orderText, string? reportText) =>

        DescribeMoveRegionReachabilityViolations(orderText, reportText).Count > 0;



    public static IReadOnlyList<string> DescribeMoveRegionReachabilityViolations(string orderText, string? reportText)

    {

        var violations = new List<string>();

        if (string.IsNullOrWhiteSpace(reportText))

        {

            return violations;

        }



        var grantRegion = ReportRegionGraph.ParseGrantRegionId(reportText);

        var adjacency = ReportRegionGraph.ParseGroundRegionAdjacency(reportText);

        if (grantRegion is null || adjacency.Count == 0)

        {

            return violations;

        }



        foreach (var block in ParseModuleStackBlocks(orderText))

        {

            var location = grantRegion;

            foreach (var line in block.Lines)

            {

                if (!IsMoveLine(line))

                {

                    continue;

                }



                if (OrbitHopRegex().IsMatch(line))

                {

                    continue;

                }



                var dest = ExtractGroundMoveRegionId(line);

                if (dest is null)

                {

                    continue;

                }



                if (location is null

                    || !adjacency.TryGetValue(location, out var exits)

                    || !exits.Contains(dest))

                {

                    violations.Add(

                        $"stack {block.StackId}: `move {dest}` is not reachable from {location ?? grantRegion} — pick a region listed under Exits in the grant report.");

                }



                location = dest;

            }

        }



        return violations;

    }



    public static bool HasGrantTargetViolations(string orderText, string? reportText = null) =>

        DescribeGrantTargetViolations(orderText, reportText).Count > 0;



    public static IReadOnlyList<string> DescribeGrantTargetViolations(string orderText, string? reportText = null)

    {

        var violations = new List<string>();

        foreach (Match match in GrantOrderLineRegex().Matches(orderText))

        {

            var target = match.Groups[1].Value.Trim();

            if (IsNumericStackId(target))

            {

                continue;

            }



            violations.Add(

                $"GRANT target `{target}` must be a numeric modulestack id from the report (not `factory`, `cargob`, or `newN`). GRANT may appear under `#faction` or `#modulestack`.");

        }



        return violations;

    }



    private static bool IsNumericStackId(string target) =>

        target.Length > 0 && target.All(char.IsDigit);



    private static string? ExtractGroundMoveRegionId(string line)

    {

        var trimmed = StripOrderPrefixes(line).TrimStart();

        if (!trimmed.StartsWith("move ", StringComparison.OrdinalIgnoreCase)

            && !trimmed.StartsWith("@move ", StringComparison.OrdinalIgnoreCase))

        {

            return null;

        }



        var match = GroundMoveRegionRegex().Match(trimmed);

        return match.Success ? match.Groups[1].Value.ToUpperInvariant() : null;

    }



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



    public static bool HasMilitaryPersonaViolations(string orderText, string? personaPreference, string? reportText = null)

    {

        if (!string.Equals(personaPreference, "military", StringComparison.OrdinalIgnoreCase))

        {

            return false;

        }



        return DescribeMilitaryPersonaViolations(orderText, reportText).Count > 0;

    }



    public static IReadOnlyList<string> DescribeMilitaryPersonaViolations(string orderText, string? reportText = null)

    {

        var violations = new List<string>();

        if (Regex.IsMatch(orderText, @"\bsell\s+\d+\s+food\b", RegexOptions.IgnoreCase))

        {

            violations.Add("military persona: do not sell food early — tanks consume 16 food per quarter; keep grant calories for the army.");

        }



        if (!MilitaryOffensivePlan(orderText))

        {

            return violations;

        }



        if (!Regex.IsMatch(orderText, @"grant\s+technology\s+armcbt\b", RegexOptions.IgnoreCase))

        {

            violations.Add(

                "military persona: `grant technology armcbt to <factry-id>` before `use armcbt` — pays for the tank tech copy on the factory.");

        }



        if (!Regex.IsMatch(orderText, @"grant\s+item\s+\d+\s+iron\b", RegexOptions.IgnoreCase))

        {

            violations.Add(

                "military persona: `grant item 50 iron to <cargob-id>` before factory `+get` / tank provisioning — bank-funded iron for grndtr and armcbt.");

        }



        if (!Regex.IsMatch(orderText, @"grant\s+item\s+\d+\s+oil\b", RegexOptions.IgnoreCase))

        {

            violations.Add(

                "military persona: `grant item 20 oil to <cargob-id>` — tanks and trucks burn oil; grant before `-get` oil on tank squads.");

        }



        if (!Regex.IsMatch(orderText, @"grant\s+item\s+\d+\s+titani\b", RegexOptions.IgnoreCase))

        {

            violations.Add(

                "military persona: `grant item 10 titani to <cargob-id>` before `use armcbt` / `+get` titani from cargob.");

        }



        var bankBalance = ReportRegionGraph.ParseBankBalance(reportText);

        if (bankBalance is >= 5000)

        {

            var terranPullFromHq = 0;

            foreach (Match match in TerranGetFromHqRegex().Matches(orderText))

            {

                if (int.TryParse(match.Groups[1].Value, out var qty))

                {

                    terranPullFromHq += qty;

                }

            }



            var hqTerranOnHand = 20;

            var grantedTerran = 0;

            foreach (Match match in GrantTerranRegex().Matches(orderText))

            {

                if (int.TryParse(match.Groups[1].Value, out var qty))

                {

                    grantedTerran += qty;

                }

            }



            if (terranPullFromHq > hqTerranOnHand + grantedTerran)

            {

                var need = terranPullFromHq - hqTerranOnHand;

                violations.Add(

                    $"military persona: tank squads pull {terranPullFromHq} terran from HQ but only ~{hqTerranOnHand} start on grant — `grant item {need} terran to <hq-id>` (bank ≥ 5000) before `-get … terran …` on `has 1 tanks` blocks.");

            }

        }



        return violations;

    }



    private static bool MilitaryOffensivePlan(string orderText) =>

        Regex.IsMatch(orderText, @"\buse\s+grndtr\b", RegexOptions.IgnoreCase)

        || Regex.IsMatch(orderText, @"\buse\s+armcbt\b", RegexOptions.IgnoreCase);



    public static bool HasDrillUseResourceViolations(string orderText, string? reportText) =>

        DescribeDrillUseResourceViolations(orderText, reportText).Count > 0;



    public static IReadOnlyList<string> DescribeDrillUseResourceViolations(string orderText, string? reportText)

    {

        var violations = new List<string>();

        if (string.IsNullOrWhiteSpace(reportText))

        {

            return violations;

        }



        var grantResources = ReportRegionGraph.ParseGrantRegionResourceIds(reportText);

        if (grantResources.Count == 0)

        {

            return violations;

        }



        foreach (var block in ParseModuleStackBlocks(orderText))

        {

            var blockText = string.Join('\n', block.Lines);

            if (!Regex.IsMatch(blockText, @"@use\s+(hcdril|iminng)\b", RegexOptions.IgnoreCase))

            {

                continue;

            }

            if (Regex.IsMatch(blockText, @"@use\s+hcdril\b", RegexOptions.IgnoreCase)

                && !grantResources.Contains("carbon")

                && !grantResources.Contains("oil"))

            {

                violations.Add(

                    $"stack {block.StackId}: `@use hcdril` needs carbon or oil in the grant region Resources line — this cell has neither; use `@use iminng` only when iron is listed (Anvil/wind grants).");

            }



            if (Regex.IsMatch(blockText, @"@use\s+iminng\b", RegexOptions.IgnoreCase)

                && !grantResources.Contains("iron"))

            {

                violations.Add(

                    $"stack {block.StackId}: `@use iminng` needs iron in the grant region Resources line — omit iminng or GRANT iron for factory staging only, not surface mining.");

            }

        }



        return violations;

    }



    public static bool HasCombinedDrillUseViolations(string orderText) =>

        DescribeCombinedDrillUseViolations(orderText).Count > 0;



    public static IReadOnlyList<string> DescribeCombinedDrillUseViolations(string orderText)

    {

        var violations = new List<string>();

        foreach (var block in ParseModuleStackBlocks(orderText))

        {

            var blockText = string.Join('\n', block.Lines);

            if (Regex.IsMatch(blockText, @"@use\s+hcdril\b", RegexOptions.IgnoreCase)

                && Regex.IsMatch(blockText, @"@use\s+iminng\b", RegexOptions.IgnoreCase))

            {

                violations.Add(

                    $"stack {block.StackId}: do not combine `@use hcdril` and `@use iminng` on one sdrill — the engine runs hydrocarbons first and iminng never executes; pick one tech matching grant region resources.");

            }

        }



        return violations;

    }



    public static bool HasEnergyStagingViolations(string orderText, string? reportText) =>

        DescribeEnergyStagingViolations(orderText, reportText).Count > 0;



    public static IReadOnlyList<string> DescribeEnergyStagingViolations(string orderText, string? reportText)

    {

        var violations = new List<string>();

        if (string.IsNullOrWhiteSpace(reportText))

        {

            return violations;

        }



        if (!Regex.IsMatch(orderText, @"\buse\s+mcored\b", RegexOptions.IgnoreCase)

            || !Regex.IsMatch(orderText, @"\bhas\s+1\s+cdrill\b", RegexOptions.IgnoreCase))

        {

            return violations;

        }



        var windGrant = ReportStackCatalog.GrantUsesWindPowerPlant(reportText);

        var wnplntId = ReportStackCatalog.FindStackIdByModuleType(reportText, "wnplnt");

        var cplantId = ReportStackCatalog.FindStackIdByModuleType(reportText, "cplant");



        if (windGrant && wnplntId is not null)

        {

            if (!Regex.IsMatch(orderText, $@"#\s*modulestack\s+{Regex.Escape(wnplntId)}\b", RegexOptions.IgnoreCase)

                || !Regex.IsMatch(orderText, @"@produce\s+energy", RegexOptions.IgnoreCase))

            {

                violations.Add(

                    $"energy: nested cdrill under `use mcored` needs `#modulestack {wnplntId}` with `@produce energy` before activation — grant tree is energy-starved.");

            }



            if (!Regex.IsMatch(orderText, @"\buse\s+wndtrb\b", RegexOptions.IgnoreCase))

            {

                violations.Add(

                    $"energy: after mcored/cdrill staging on Anvil, expand wind on `#modulestack {wnplntId}` — `use wndtrb as newN for {wnplntId}` (then `@produce energy`) so nested modules can activate.");

            }

        }

        else if (!windGrant && cplantId is not null

            && !Regex.IsMatch(orderText, $@"#\s*modulestack\s+{Regex.Escape(cplantId)}\b[\s\S]*?@produce\s+energy", RegexOptions.IgnoreCase))

        {

            violations.Add(

                $"energy: nested cdrill under `use mcored` needs `#modulestack {cplantId}` with `@produce energy` (and sdrill `@use hcdril` + cargob carbon) before `-get` terran on the cdrill nest.");

        }



        return violations;

    }



    public static bool HasWindGrantDrillViolations(string orderText, string? reportText, string? personaPreference) =>

        DescribeWindGrantDrillViolations(orderText, reportText, personaPreference).Count > 0;



    public static IReadOnlyList<string> DescribeWindGrantDrillViolations(

        string orderText,

        string? reportText,

        string? personaPreference)

    {

        var violations = new List<string>();

        if (!ReportStackCatalog.GrantUsesWindPowerPlant(reportText))

        {

            return violations;

        }



        if (Regex.IsMatch(orderText, @"@use\s+hcdril\b", RegexOptions.IgnoreCase))

        {

            violations.Add(

                "wind-grant (Anvil): do not `@use hcdril` — that mines carbon for coal plants. Use `@use iminng` on the sdrill for iron (and other metals); energy comes from `#modulestack <wnplnt-id>` `@produce energy`.");

        }



        if (string.Equals(personaPreference, "absent-player", StringComparison.OrdinalIgnoreCase))

        {

            return violations;

        }



        var sdrillId = ReportStackCatalog.FindStackIdByModuleType(reportText, "sdrill");

        var hasSdrillStackBlock = sdrillId is not null

            && Regex.IsMatch(orderText, $@"#\s*modulestack\s+{Regex.Escape(sdrillId)}\b", RegexOptions.IgnoreCase);



        if ((hasSdrillStackBlock || EconomicBootstrapPlan(orderText) || MilitaryOffensivePlan(orderText) || ResearchBootstrapPlan(orderText))

            && !Regex.IsMatch(orderText, @"@use\s+iminng\b", RegexOptions.IgnoreCase))

        {

            violations.Add(

                "wind-grant (Anvil): `#modulestack <sdrill-id>` needs `@use iminng` for iron/copper/silicium — not `@use hcdril` (carbon/coal path is for Arbor cplant grants).");

        }



        return violations;

    }



    public static bool HasAbsentPlayerPersonaViolations(string orderText, string? personaPreference, string? reportText = null)

    {

        if (!string.Equals(personaPreference, "absent-player", StringComparison.OrdinalIgnoreCase))

        {

            return false;

        }



        return DescribeAbsentPlayerPersonaViolations(orderText, reportText).Count > 0;

    }



    public static IReadOnlyList<string> DescribeAbsentPlayerPersonaViolations(string orderText, string? reportText = null)

    {

        var violations = new List<string>();

        var upper = orderText.ToUpperInvariant();

        var windGrant = ReportStackCatalog.GrantUsesWindPowerPlant(reportText);



        if (upper.Contains("@PRODUCE TERRAN", StringComparison.Ordinal)

            || upper.Contains("\nPRODUCE TERRAN", StringComparison.Ordinal))

        {

            violations.Add(

                "absent-player: use `@produce cash` on HQ for upkeep — do not `@produce terran` (it raises crew upkeep).");

        }



        if (Regex.IsMatch(orderText, @"\bsell\s+\d+\s+(food|carbon|iron|oil)\b", RegexOptions.IgnoreCase))

        {

            violations.Add("absent-player: do not sell grant food or resources — maintenance only.");

        }



        if (Regex.IsMatch(upper, @"\b@?USE\s+IMINNG\b", RegexOptions.None))

        {

            violations.Add(

                "absent-player: do not `@use iminng` on drills — iron mining is not needed for maintenance; keep `@use hcdril` on the sdrill for cplant fuel.");

        }



        if (!windGrant && !Regex.IsMatch(orderText, @"@use\s+hcdril\b", RegexOptions.IgnoreCase))

        {

            violations.Add(

                "absent-player: `#modulestack <sdrill-id>` needs `@use hcdril` (with cargob `@get all carbon` from sdrill) so cplant can `@produce energy`.");

        }



        if (!windGrant && !Regex.IsMatch(orderText, @"@get\s+all\s+carbon\s+from\s+\d+", RegexOptions.IgnoreCase))

        {

            violations.Add(

                "absent-player: cargob needs `@get all carbon from <sdrill-id>` so cplant can `@produce energy`.");

        }



        if (Regex.IsMatch(upper, @"\bUSE\s+(ARMCBT|GRNDTR|MCORED|TWNBLD|MSRVTM|FUSTOR|SSHULL)\b", RegexOptions.None))

        {

            violations.Add(

                "absent-player: no factory military/town/scout USE lines — upkeep and existing grant modules only.");

        }



        return violations;

    }



    public static bool HasEconomicPersonaViolations(string orderText, string? personaPreference, string? reportText = null)

    {

        if (!string.Equals(personaPreference, "economic", StringComparison.OrdinalIgnoreCase))

        {

            return false;

        }



        return DescribeEconomicPersonaViolations(orderText, reportText).Count > 0;

    }



    public static IReadOnlyList<string> DescribeEconomicPersonaViolations(string orderText, string? reportText = null)

    {

        var violations = new List<string>();

        if (!EconomicBootstrapPlan(orderText))

        {

            return violations;

        }



        if (!Regex.IsMatch(orderText, @"grant\s+technology\s+mcored\b", RegexOptions.IgnoreCase))

        {

            violations.Add(

                "economic persona: `grant technology mcored to <factry-stack-id>` before `use mcored as newN` — pays for the factory tech copy.");

        }



        if (!Regex.IsMatch(orderText, @"grant\s+technology\s+msrvtm\b", RegexOptions.IgnoreCase))

        {

            violations.Add(

                "economic persona: `grant technology msrvtm to <factry-stack-id>` before moblab/scout builds — stages the mobile-lab copy on the factory.");

        }



        if (!Regex.IsMatch(orderText, @"grant\s+item\s+\d+\s+iron\b", RegexOptions.IgnoreCase))

        {

            violations.Add(

                "economic persona: `grant item 50 iron to <sdrill-id>` (or cargob) before `@use iminng` / nested cdrill — bank-funded iron for the bootstrap plan.");

        }



        if (!Regex.IsMatch(orderText, @"grant\s+item\s+\d+\s+titani\b", RegexOptions.IgnoreCase))

        {

            violations.Add(

                "economic persona: `grant item 10 titani to <sdrill-id>` (or cargob) before `@use iminng` / `use mcored` — titani for drill and factory staging.");

        }



        if (Regex.IsMatch(orderText, @"grant\s+technology\s+msrvtm\b", RegexOptions.IgnoreCase)

            && !Regex.IsMatch(orderText, @"\buse\s+msrvtm\s+as\s+new\d+", RegexOptions.IgnoreCase))

        {

            violations.Add(

                "economic persona: after `grant technology msrvtm`, build a mobile survey lab — `use msrvtm as newN for <hq-id>` with `+get` iron/titani, then `move` the moblab toward the nearest deep metal pocket and copy `mcored` onto it.");

        }



        return violations;

    }



    private static bool EconomicBootstrapPlan(string orderText) =>

        Regex.IsMatch(orderText, @"\buse\s+mcored\b", RegexOptions.IgnoreCase)

        || Regex.IsMatch(orderText, @"@use\s+iminng\b", RegexOptions.IgnoreCase);



    public static bool HasResearcherPersonaViolations(string orderText, string? personaPreference, string? reportText = null)

    {

        if (!string.Equals(personaPreference, "researcher", StringComparison.OrdinalIgnoreCase))

        {

            return false;

        }



        return DescribeResearcherPersonaViolations(orderText, reportText).Count > 0;

    }



    public static IReadOnlyList<string> DescribeResearcherPersonaViolations(string orderText, string? reportText = null)

    {

        var violations = new List<string>();

        if (!ResearchBootstrapPlan(orderText))

        {

            return violations;

        }



        if (!Regex.IsMatch(orderText, @"grant\s+technology\s+msrvtm\b", RegexOptions.IgnoreCase))

        {

            violations.Add(

                "researcher persona: `grant technology msrvtm to <factry-id>` before `use msrvtm` — pays for the mobile-survey tech copy on the factory.");

        }



        if (!Regex.IsMatch(orderText, @"grant\s+item\s+\d+\s+iron\b", RegexOptions.IgnoreCase))

        {

            violations.Add(

                "researcher persona: `grant item 2 iron to <cargob-id>` before factory `+get` / `use msrvtm` — bank-funded iron for the moblab build.");

        }



        if (!Regex.IsMatch(orderText, @"grant\s+item\s+\d+\s+silici\b", RegexOptions.IgnoreCase))

        {

            violations.Add(

                "researcher persona: `grant item 2 silici to <cargob-id>` before `use msrvtm` / `+get` silici from cargob.");

        }



        if (!ReportStackCatalog.GrantUsesWindPowerPlant(reportText)

            && Regex.IsMatch(orderText, @"@use\s+iminng\b", RegexOptions.IgnoreCase))

        {

            violations.Add(

                "researcher persona (Arbor/coal grant): use `@use hcdril` on sdrill for carbon; defer `@use iminng` until after the survey column is staged.");

        }



        return violations;

    }



    private static bool ResearchBootstrapPlan(string orderText) =>

        Regex.IsMatch(orderText, @"\buse\s+msrvtm\b", RegexOptions.IgnoreCase);



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

            feedbackLines.AddRange(DescribeMoveRegionReachabilityViolations(orderText, reportText).Select(violation => "  - " + violation));

            feedbackLines.AddRange(DescribeGrantTargetViolations(orderText, reportText).Select(violation => "  - " + violation));

            feedbackLines.AddRange(DescribeUseTechPlacementViolations(orderText, reportText).Select(violation => "  - " + violation));

            feedbackLines.AddRange(DescribeInvalidItemTypeViolations(orderText).Select(violation => "  - " + violation));

            feedbackLines.AddRange(DescribeDoubleConditionViolations(orderText).Select(violation => "  - " + violation));

            feedbackLines.AddRange(DescribeDeferredNestGetViolations(orderText).Select(violation => "  - " + violation));

            feedbackLines.AddRange(DescribeTravelProvisioningViolations(orderText).Select(violation => "  - " + violation));

            feedbackLines.AddRange(DescribeFactoryPlusGetViolations(orderText, personaPreference).Select(violation => "  - " + violation));

            feedbackLines.AddRange(DescribeMobileAtViolations(orderText).Select(violation => "  - " + violation));

            feedbackLines.AddRange(DescribeSetHoldViolations(orderText).Select(violation => "  - " + violation));

            feedbackLines.AddRange(DescribeMilitaryPersonaViolations(orderText, reportText).Select(violation => "  - " + violation));

            feedbackLines.AddRange(DescribeAbsentPlayerPersonaViolations(orderText, reportText).Select(violation => "  - " + violation));

            feedbackLines.AddRange(DescribeWindGrantDrillViolations(orderText, reportText, personaPreference).Select(violation => "  - " + violation));

            feedbackLines.AddRange(DescribeDrillUseResourceViolations(orderText, reportText).Select(violation => "  - " + violation));

            feedbackLines.AddRange(DescribeCombinedDrillUseViolations(orderText).Select(violation => "  - " + violation));

            feedbackLines.AddRange(DescribeEnergyStagingViolations(orderText, reportText).Select(violation => "  - " + violation));

            feedbackLines.AddRange(DescribeEconomicPersonaViolations(orderText, reportText).Select(violation => "  - " + violation));

            feedbackLines.AddRange(DescribeResearcherPersonaViolations(orderText, reportText).Select(violation => "  - " + violation));

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

              - cargob: `grant item 2 iron`, `grant item 2 silici` to cargob-id, then `@get all food/carbon`, sell surplus food

              - factory: `grant technology msrvtm to <factry-id>`, then `use msrvtm as newNNN for <hq-id>` with `+get 2 iron` and `+get 2 silici` from cargob

              - economic loop: HQ `set hold 20 terran` + `@produce terran`; sdrill `@use hcdril` (not iminng); farms `@use farmng`; cplant `@produce energy`

              - #modulestack newNNN: `move` anomaly region-id; `+get` terran/oil/food; `@research` same region-id (no `@` on move/get)

              Use stack ids from the Orders template. Do not reply with only active/see lines. Include #end.

              """;

        }



        if (string.Equals(personaPreference, "military", StringComparison.OrdinalIgnoreCase))

        {

            return moveBlock + """

              Your previous draft was incomplete. Rewrite the full order file with at least:

              - HQ: `set hold 20 terran` and `@produce terran`; cargob: `grant item 50 iron`, `grant item 20 oil`, `grant item 10 titani` to cargob-id, then `@get all food/carbon` — **no sell food**

              - factory: `grant technology armcbt to <factry-id>`, then `use grndtr` as new1 with `+get`, two `use armcbt` as new2/new3 each with `+get` iron/titani

              - #modulestack new1: `move` Farm Belt, then `+get` terran/oil/food (no `@` on move/get)

              - #modulestack new2/new3: `has 1 tanks`, `-get` 16 terran / 8 oil / 32 food, `-move` Mid Vale, `tactic destroy` (no `@` on move/tactic/active). Include #end.

              """;

        }



        if (string.Equals(personaPreference, "absent-player", StringComparison.OrdinalIgnoreCase))

        {

            return moveBlock + """

              Your previous draft was incomplete. Rewrite the full order file with at least:

              - HQ: `set hold 20 terran` and `@produce cash` (NOT `@produce terran`)

              - Cargob: `@get all food from <farms-id>`, `@get all carbon from <sdrill-id>` — no sell lines

              - Sdrill: `@use hcdril` (not `@use iminng`); farms: `@use farmng`; cplant: `@produce energy`

              - No factory USE or military moves

              Use stack ids from the Orders template. Include #end.

              """;

        }



        if (string.Equals(personaPreference, "economic", StringComparison.OrdinalIgnoreCase))

        {

            return moveBlock + """

              Your previous draft was incomplete. Rewrite the full order file with at least:

              - `@produce energy` on cplant before scaling nested cdrill draw

              - `@produce energy` on cplant; HQ `set hold 20 terran` + `@produce terran`

              - sdrill: `grant item 50 iron` + `grant item 10 titani` to drill id, then `@use hcdril` + `@use iminng`

              - factory stack id: `grant technology mcored to <factry-id>`, `grant technology msrvtm to <factry-id>`, then `use mcored as newNNN for <hq-id>` with `+get` iron/titani from cargob

              - `#modulestack newNNN`: `has 1 cdrill`, `-get` 6 terran, `deactivate 1`; cargob `@get all` + sell food

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



    [GeneratedRegex(@"(?im)^\s*grant\s+(?:technology|item|skill)\s+.+?\s+to\s+(\S+)\s*$")]

    private static partial Regex GrantOrderLineRegex();



    [GeneratedRegex(@"\bmove\s+(R\d+)\b", RegexOptions.IgnoreCase)]

    private static partial Regex GroundMoveRegionRegex();



    [GeneratedRegex(@"\-get\s+(\d+)\s+terran\b", RegexOptions.IgnoreCase)]

    private static partial Regex TerranGetFromHqRegex();



    [GeneratedRegex(@"grant\s+item\s+(\d+)\s+terran\s+to\s+\d+", RegexOptions.IgnoreCase)]

    private static partial Regex GrantTerranRegex();

}


