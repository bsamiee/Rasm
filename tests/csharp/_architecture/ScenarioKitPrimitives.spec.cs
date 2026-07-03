using System.Text.Json;
using Rasm.Bridge.Contract;
using Rasm.ScenarioKit;
using Rasm.TestKit;

namespace Rasm.Architecture.Tests;

// --- [MODELS] --------------------------------------------------------------------------------

internal static class EvidenceGens {
    public static readonly Gen<string> Label = Gen.Char[start: 'a', finish: 'z'].Array[1, 16].Select(selector: static chars => new string(value: chars));
    public static readonly Gen<(string Label, int Payload)> Projection = Label.Select(Gen.Int[start: -1_000_000, finish: 1_000_000], static (string label, int payload) => (label, payload));
    public static readonly Gen<(string Key, int Payload)[]> Stream = Projection.Array[0, 24];

    // Null Doc proves evidence verbs stay doc-blind; any dereference fails the law.
    public static ScenarioContext Context(List<(string Key, object? Value)> log) =>
        new(doc: null!, sink: (key, value) => log.Add(item: (key, value)));
}

// --- [OPERATIONS] ----------------------------------------------------------------------------

public sealed class RequireLaws {
    [Fact]
    public void TrueObservationRidesTheSuccessRail() =>
        Spec.ForAll(gen: EvidenceGens.Label, property: static label => {
            List<(string Key, object? Value)> log = [];
            ScenarioContext ctx = EvidenceGens.Context(log: log);
            Spec.Succ(result: ctx.Require(label: label, observed: true));
            (string key, object? value) = Assert.Single(collection: log);
            Assert.Equal(expected: label, actual: key);
            Assert.Equal(expected: true, actual: value);
        });

    [Fact]
    public void FalseObservationFailsNamingTheLabel() =>
        Spec.ForAll(gen: EvidenceGens.Label, property: static label => {
            List<(string Key, object? Value)> log = [];
            ScenarioContext ctx = EvidenceGens.Context(log: log);
            Spec.Fail(result: ctx.Require(label: label, observed: false), then: error =>
                Assert.Contains(expectedSubstring: $"'{label}'", actualString: error.Message, comparisonType: StringComparison.Ordinal));
            (string key, object? value) = Assert.Single(collection: log);
            Assert.Equal(expected: label, actual: key);
            Assert.Equal(expected: false, actual: value);
        });
}

public sealed class ExpectLaws {
    [Fact]
    public void SuccessProjectionReturnsVerbatimAndFactsTheValue() =>
        Spec.ForAll(gen: EvidenceGens.Projection, property: static input => {
            List<(string Key, object? Value)> log = [];
            ScenarioContext ctx = EvidenceGens.Context(log: log);
            Spec.Succ(result: ctx.Expect(label: input.Label, projection: Fin.Succ(value: input.Payload)), then: value =>
                Assert.Equal(expected: input.Payload, actual: value));
            (string key, object? facted) = Assert.Single(collection: log);
            Assert.Equal(expected: input.Label, actual: key);
            Assert.Equal(expected: input.Payload, actual: facted);
        });

    [Fact]
    public void FailureProjectionPreservesTheRailAndFactsTheFailure() =>
        Spec.ForAll(gen: EvidenceGens.Projection, property: static input => {
            List<(string Key, object? Value)> log = [];
            ScenarioContext ctx = EvidenceGens.Context(log: log);
            string message = string.Create(System.Globalization.CultureInfo.InvariantCulture, $"broken-{input.Payload}");
            Spec.Fail(result: ctx.Expect(label: input.Label, projection: Fin.Fail<int>(error: Error.New(message: message))), then: error =>
                Assert.Equal(expected: message, actual: error.Message));
            (string key, object? facted) = Assert.Single(collection: log);
            Assert.Equal(expected: input.Label, actual: key);
            Assert.Equal(expected: $"FAIL: {message}", actual: facted);
        });
}

public sealed class FactLaws {
    [Fact]
    public void StreamForwardsInOrderAndCountsConserve() =>
        Spec.ForAll(gen: EvidenceGens.Stream, property: static stream => {
            List<(string Key, object? Value)> log = [];
            ScenarioContext ctx = EvidenceGens.Context(log: log);
            foreach ((string key, int payload) in stream) {
                ctx.Fact(key: key, value: payload);
            }
            Assert.Equal(expected: stream.Length, actual: ctx.FactCount);
            Assert.Equal(expected: stream.Select(selector: static row => (row.Key, (object?)row.Payload)), actual: log);
        });
}

public sealed class FactsEmptyTriggerLaws {
    // The runner's facts.empty trigger is exactly FactCount == 0 after entrypoint return.
    [Fact]
    public void FreshContextFiresTheTrigger() {
        List<(string Key, object? Value)> log = [];
        ScenarioContext ctx = EvidenceGens.Context(log: log);
        Assert.Equal(expected: 0, actual: ctx.FactCount);
        Assert.Empty(collection: log);
    }

    [Fact]
    public void EveryEvidenceVerbSuppressesTheTrigger() {
        // Failed evidence verbs still suppress facts.empty; failure plus empty evidence is contradictory.
        (string Verb, Action<ScenarioContext> Call)[] verbs = [
            (Verb: "fact", Call: static ctx => ctx.Fact(key: "k", value: 1)),
            (Verb: "require.pass", Call: static ctx => _ = ctx.Require(label: "k", observed: true)),
            (Verb: "require.fail", Call: static ctx => _ = ctx.Require(label: "k", observed: false)),
            (Verb: "expect.succ", Call: static ctx => _ = ctx.Expect(label: "k", projection: Fin.Succ(value: 9))),
            (Verb: "expect.fail", Call: static ctx => _ = ctx.Expect(label: "k", projection: Fin.Fail<int>(error: Error.New(message: "boom")))),
        ];
        Assert.All(collection: verbs, action: static verb => {
            List<(string Key, object? Value)> log = [];
            ScenarioContext ctx = EvidenceGens.Context(log: log);
            verb.Call(ctx);
            Spec.Holds(condition: ctx.FactCount > 0, label: $"{verb.Verb} must count as evidence");
            Assert.Equal(expected: ctx.FactCount, actual: log.Count);
        });
    }
}

public sealed class ReferenceEmissionLaws {
    // Author-mode candidates are minted from exactly this wire payload: the supervisor fold reads
    // {name, actual, tolerance} off the reference.-prefixed fact and decides admission itself from
    // evidence mode and corpus state — an SDK-asserted admission field would be a standing lie.
    [Fact]
    public void CertifyEmitsTheCandidateActualUnderTheReferenceGrammar() =>
        Spec.ForAll(gen: EvidenceGens.Projection, property: static input => {
            List<(string Key, object? Value)> log = [];
            ScenarioContext ctx = EvidenceGens.Context(log: log);
            ReferenceTolerance tolerance = new(Mode: "absolute", Absolute: 1.0e-9, Relative: 0.0);
            Spec.Succ(result: ctx.Certify(key: new EvidenceName(Key: input.Label), actual: input.Payload, tolerance: tolerance));
            (string key, object? value) = Assert.Single(collection: log);
            Assert.Equal(expected: $"reference.{input.Label}", actual: key);
            Assert.Equal(expected: EvidenceRole.Reference, actual: EvidenceRole.OfFactKey(key: key));
            Assert.Equal(expected: input.Label, actual: EvidenceRole.Reference.FactArgument(key: key));
            JsonElement payload = JsonSerializer.SerializeToElement(value: value);
            Assert.Equal(expected: input.Label, actual: payload.GetProperty("name").GetString());
            Assert.Equal(expected: input.Payload, actual: payload.GetProperty("actual").GetInt32());
            Assert.Equal(expected: "absolute", actual: payload.GetProperty("tolerance").GetProperty("Mode").GetString());
            Assert.False(condition: payload.TryGetProperty("admission", out _), "admission is supervisor-decided; the SDK never asserts it");
        });

    [Fact]
    public void ReferenceEmissionCountsRideTheEvidenceStream() {
        List<(string Key, object? Value)> log = [];
        ScenarioContext ctx = EvidenceGens.Context(log: log);
        _ = ctx.Certify(key: new EvidenceName(Key: "axis"), actual: 42.0, tolerance: default);
        _ = ctx.Certify(key: new EvidenceName(Key: "span"), actual: JsonSerializer.SerializeToElement(value: 2.5), tolerance: default);
        Assert.Equal(expected: 2, actual: ctx.ReferenceCount);
        Assert.Equal(expected: 2, actual: ctx.FactCount);
        Assert.Equal(expected: ["reference.axis", "reference.span"], actual: log.Select(selector: static row => row.Key));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t ")]
    public void BlankReferenceKeyIsAnInputGuardAndCountsNothing(string? key) {
        ScenarioContext ctx = EvidenceGens.Context(log: []);
        _ = Assert.ThrowsAny<ArgumentException>(testCode: () => ctx.Certify(key: new EvidenceName(Key: key!), actual: 1, tolerance: default));
        Assert.Equal(expected: 0, actual: ctx.FactCount);
        Assert.Equal(expected: 0, actual: ctx.ReferenceCount);
    }
}

public sealed class ScopeAndCaptureLaws {
    [Fact]
    public void EmptyRegistryDrainsZeroAndRealizesNoView() {
        ScenarioContext ctx = EvidenceGens.Context(log: []);
        Assert.Null(ctx.RealizedView);
        Assert.Equal(expected: 0, actual: ctx.DrainScopes());
    }

    [Fact]
    public void SnapshotOutsideARunDegradesTyped() =>
        Spec.Fail(result: Capture.Snapshot(label: "outside"), then: static error =>
            Assert.Contains(expectedSubstring: "no capture surface bound", actualString: error.Message, comparisonType: StringComparison.Ordinal));

    [Fact]
    public void BoundHookReceivesTheLabelVerbatim() {
        try {
            Capture.Hook = static label => Fin.Succ(value: new CaptureReceipt(Path: $"png:{label}", Width: 1, Height: 2, OnFailure: false));
            Spec.ForAll(gen: EvidenceGens.Label, property: static label =>
                Spec.Succ(result: Capture.Snapshot(label: label), then: receipt =>
                    Assert.Equal(expected: $"png:{label}", actual: receipt.Path)));
        } finally {
            Capture.Hook = null;
        }
    }
}

public sealed class FactKeyGrammarLaws {
    // Rendered fact keys are frozen wire law: Supervisor session folds classify by these exact
    // strings and prefixes, so any drift in a row is a bridge protocol break, never a refactor.
    [Fact]
    public void EveryRowRendersTheExactWireString() {
        Assert.Equal(expected: 13, actual: FactKey.Items.Count);
        static bool Renders(FactKey row, string argument, string wire) =>
            string.Equals(a: row.Render(argument: argument), b: wire, comparisonType: StringComparison.Ordinal);
        Spec.Matrix(
            ("reference", () => Renders(FactKey.Reference, "k", "reference.k"), true),
            ("manifest.object", () => Renders(FactKey.ObjectManifest, "k", "manifest.object.k"), true),
            ("manifest.geometry", () => Renders(FactKey.GeometryManifest, "k", "manifest.geometry.k"), true),
            ("manifest.viewport", () => Renders(FactKey.ViewportManifest, "k", "manifest.viewport.k"), true),
            ("manifest.gh2", () => Renders(FactKey.Gh2Manifest, "k", "manifest.gh2.k"), true),
            ("artifact", () => Renders(FactKey.Artifact, "role", "artifact.role"), true),
            ("case.start", () => Renders(FactKey.CaseStart, "name", "case.name.start"), true),
            ("case.status", () => Renders(FactKey.CaseStatus, "name", "case.name.status"), true),
            ("scratch.path", () => Renders(FactKey.ScratchPath, "ignored", "scratch.path"), true),
            ("stamp", () => Renders(FactKey.Stamp, "ignored", "stamp"), true),
            ("document.before", () => Renders(FactKey.DocumentBefore, string.Empty, "document.before.objects"), true),
            ("document.opened", () => Renders(FactKey.DocumentOpened, string.Empty, "document.opened.objects"), true),
            ("document.after", () => Renders(FactKey.DocumentAfter, string.Empty, "document.after.objects"), true));
    }

    // The evidence verbs must route through the grammar: a context-emitted key equals the row render.
    [Fact]
    public void EvidenceVerbsRenderThroughTheGrammar() {
        List<(string Key, object? Value)> log = [];
        ScenarioContext ctx = EvidenceGens.Context(log: log);
        _ = ctx.Case(name: "probe", action: static () => Fin.Succ(value: unit));
        Assert.Equal(expected: ["case.probe.start", "case.probe.status"], actual: log.Select(selector: static row => row.Key));
    }
}

// --- [ERRORS] --------------------------------------------------------------------------------

public sealed class GuardLaws {
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t ")]
    public void BlankFactKeyIsAnInputGuard(string? key) {
        ScenarioContext ctx = EvidenceGens.Context(log: []);
        _ = Assert.ThrowsAny<ArgumentException>(testCode: () => ctx.Fact(key: key!, value: 1));
        Assert.Equal(expected: 0, actual: ctx.FactCount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void BlankSnapshotLabelIsAnInputGuard(string? label) =>
        _ = Assert.ThrowsAny<ArgumentException>(testCode: () => Capture.Snapshot(label: label!));

    [Fact]
    public void NullProjectionIsAnInputGuard() {
        ScenarioContext ctx = EvidenceGens.Context(log: []);
        _ = Assert.Throws<ArgumentNullException>(testCode: () => ctx.Expect<int>(label: "k", projection: null!));
    }

    [Fact]
    public void NullValueIsLegalEvidence() {
        List<(string Key, object? Value)> log = [];
        ScenarioContext ctx = EvidenceGens.Context(log: log);
        ctx.Fact(key: "absent", value: null);
        (string key, object? value) = Assert.Single(collection: log);
        Assert.Equal(expected: "absent", actual: key);
        Assert.Null(value);
        Assert.Equal(expected: 1, actual: ctx.FactCount);
    }
}
