using NUnit.Framework;
using SpaceAge.PlayerAgent.Draft;

namespace SpaceAge.PlayerAgent.Tests;

[TestFixture]
public class VerbInferenceTests
{
    [Test]
    public void InferBoostVerbs_ResearcherPersona_PrefersOperationalVerbs()
    {
        const string persona = "## Preference: researcher\nBuild moblib first; defer CT0007 town CONTRACT until later.";

        var verbs = VerbInference.InferBoostVerbs(persona);

        Assert.That(verbs, Does.Contain("USE"));
        Assert.That(verbs, Does.Contain("MOVE"));
        Assert.That(verbs, Does.Contain("RESEARCH"));
        Assert.That(verbs, Does.Not.Contain("CONTRACT"));
    }

    [Test]
    public void InferBoostVerbs_ContractorPersona_IncludesTransfer()
    {
        const string persona = "## Preference: contractor\nFile UN CONTRACT / give-module jobs first.";

        var verbs = VerbInference.InferBoostVerbs(persona);

        Assert.That(verbs, Does.Contain("TRANSFER"));
        Assert.That(verbs, Does.Contain("USE"));
    }

    [Test]
    public void DetectPersonaPreference_ReadsPreferenceHeading()
    {
        const string persona = "## Preference: researcher\nHome: Arbor";

        Assert.That(VerbInference.DetectPersonaPreference(persona), Is.EqualTo("researcher"));
    }
}
