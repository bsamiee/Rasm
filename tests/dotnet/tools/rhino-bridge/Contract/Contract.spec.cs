using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Nerdbank.Streams;
using PolyType;
using Rasm.TestKit;
using StreamJsonRpc;
using StreamJsonRpc.Protocol;
using StreamJsonRpc.Reflection;

namespace Rasm.Bridge.Contract.Tests;

// --- [MODELS] --------------------------------------------------------------------------

internal static class WireGens {
    public static readonly EventStamp Stamp = new(Guid.Parse("6a8e6c1e-9f5a-4d2c-8b8e-2f1a3c4d5e6f"), Sequence: 17, AtUnixMs: 1_765_432_100_123, Scenario: "blocks.baseline");
    public static readonly HostFingerprint Host = new("9.0.26237.15344", "9.0.26237.15344", "2.0.0", "10.0.2");
    public static readonly EndpointRecord.Live Endpoint = new("rbx-test", RhinoPid: 4242, RhinoStartedAtUnixMs: 1_765_432_000_000, RhinoVersion: "9.0.26237");
    public static readonly ArtifactRef Artifact = new("captures/blocks.baseline/BASELINE.png", EvidenceRole.Capture, "image/png", Bytes: 4096, "ab".PadLeft(64, '0'), "blocks.baseline", OnFailure: false);
    public static readonly Gen<long> ClockSkew = Gen.Long[-5_000L, 5_000L];
    public static readonly Gen<PhaseStatus> Status = Gen.OneOfConst([.. PhaseStatus.Items]);

    public static T Roundtrip<T>(T value, JsonTypeInfo<T> contract) =>
        JsonSerializer.Deserialize(JsonSerializer.Serialize(value, contract), contract)!;

    public static void ByteIdentical<T>(T value, JsonTypeInfo<T> contract) {
        byte[] raw = JsonSerializer.SerializeToUtf8Bytes(value, contract);
        T decoded = JsonSerializer.Deserialize(raw, contract)!;
        byte[] again = JsonSerializer.SerializeToUtf8Bytes(decoded, contract);
        Spec.Holds(raw.AsSpan().SequenceEqual(again), $"wire bytes drift across a roundtrip for {typeof(T).Name}");
    }
}

internal sealed class ShellStub : IBridgeShell {
    public Task<HostFingerprint> HelloAsync(int supervisorPid, CancellationToken ct) =>
        Task.FromResult(WireGens.Host with { RuntimeVersion = supervisorPid.ToString(System.Globalization.CultureInfo.InvariantCulture) });
    public Task<LoadedCargo> LoadCargoAsync(CargoManifest manifest, CancellationToken ct) =>
        Task.FromResult(new LoadedCargo(manifest.ContentHash, LoadMs: 412.3, [], []));
    public Task<ScenarioOutcome[]> RunAsync(ScenarioSelection selection, CancellationToken ct) =>
        Task.FromResult<ScenarioOutcome[]>([new ScenarioOutcome(
            selection.Switch(
                allCase: static _ => "all",
                themesCase: static t => $"themes:{string.Join(',', t.Themes)}",
                namesCase: static n => $"names:{string.Join(',', n.Names)}"),
            PhaseStatus.Ok, DurationMs: 1.0, Fault: null)]);
    public Task<UnloadOutcome> UnloadCargoAsync(CancellationToken ct) =>
        Task.FromResult(new UnloadOutcome(ReleaseRequested: true, ElapsedMs: 2.5));
    public Task<QuitScrub> PrepareQuitAsync(CancellationToken ct) =>
        Task.FromResult(new QuitScrub(Documents: 1, MarkedClean: 1, ResidualDirty: 0, new Gh2Scrub.Scrubbed(Documents: 2, ModifiedBefore: 1, ModifiedAfter: 0), []));
}

[JsonRpcContract]
[GenerateShape(IncludeMethods = MethodShapeFlags.PublicInstance)]
internal partial interface IFutureShell {
    public Task<long> FutureProbeAsync(CancellationToken ct);
}

internal static class RpcPair {
    public static async Task<T> WithClientAsync<T>(IBridgeShell target, Func<JsonRpc, Task<T>> law) {
        (Stream clientStream, Stream serverStream) = FullDuplexStream.CreatePair();
        using SystemTextJsonFormatter serverFormatter = Formatter();
        using HeaderDelimitedMessageHandler serverHandler = new(serverStream, serverFormatter);
        using JsonRpc server = new(serverHandler);
        server.AddLocalRpcTarget(target);
        server.StartListening();
        using SystemTextJsonFormatter clientFormatter = Formatter();
        using HeaderDelimitedMessageHandler clientHandler = new(clientStream, clientFormatter);
        using JsonRpc client = new(clientHandler);
        client.StartListening();
        return await law(client).ConfigureAwait(false);
    }

    private static SystemTextJsonFormatter Formatter() {
        SystemTextJsonFormatter formatter = new();
        formatter.JsonSerializerOptions.TypeInfoResolverChain.Insert(0, BridgeJsonContext.Default);
        return formatter;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class RpcProxyLaws {
    [Fact]
    public async Task ProxyIsSourceGeneratedNotDynamicAsync() =>
        _ = await RpcPair.WithClientAsync(new ShellStub(), static client => {
            IBridgeShell proxy = client.Attach<IBridgeShell>();
            Assert.False(proxy.GetType().Assembly.IsDynamic);
            _ = Assert.IsType<ProxyBase>(proxy, exactMatch: false);
            Assert.Same(typeof(IBridgeShell).Assembly, proxy.GetType().Assembly);
            return Task.FromResult(true);
        }).ConfigureAwait(true);

    [Fact]
    public async Task VerbSurfaceRoundTripsAsync() =>
        _ = await RpcPair.WithClientAsync(new ShellStub(), static async client => {
            IBridgeShell proxy = client.Attach<IBridgeShell>();
            CancellationToken ct = TestContext.Current.CancellationToken;
            HostFingerprint reply = await proxy.HelloAsync(777, ct).ConfigureAwait(true);
            Assert.Equal("777", reply.RuntimeVersion);
            Assert.Equal(WireGens.Host.BundleVersion, reply.BundleVersion);
            LoadedCargo cargo = await proxy.LoadCargoAsync(new CargoManifest(WireGens.Stamp.SessionId, "report", "xx64:abc", "stage"), ct).ConfigureAwait(false);
            Assert.Equal("xx64:abc", cargo.ContentHash);
            ScenarioOutcome[] outcomes = await proxy.RunAsync(new ScenarioSelection.ThemesCase(["blocks", "vectors"]), ct).ConfigureAwait(false);
            Assert.Equal("themes:blocks,vectors", outcomes[0].Scenario);
            Assert.True((await proxy.UnloadCargoAsync(ct).ConfigureAwait(false)).ReleaseRequested);
            QuitScrub quit = await proxy.PrepareQuitAsync(ct).ConfigureAwait(false);
            Assert.True(quit.Scrubbed);
            _ = Assert.IsType<Gh2Scrub.Scrubbed>(quit.Gh2);
            return true;
        }).ConfigureAwait(true);

    [Fact]
    public async Task MissingMethodSurfacesAsMethodNotFoundAsync() =>
        _ = await RpcPair.WithClientAsync(new ShellStub(), static async client => {
            IFutureShell future = client.Attach<IFutureShell>();
            RemoteMethodNotFoundException missing = await Assert.ThrowsAsync<RemoteMethodNotFoundException>(async () =>
                _ = await future.FutureProbeAsync(TestContext.Current.CancellationToken).ConfigureAwait(false)).ConfigureAwait(false);
            Assert.Equal(JsonRpcErrorCode.MethodNotFound, missing.ErrorCode);
            return true;
        }).ConfigureAwait(true);
}

public sealed class UnionWireLaws {
    public static TheoryData<BridgeEvent> Events => [
        new BridgeEvent.FactCase("cargo.swapMs", JsonSerializer.SerializeToElement(412.3, BridgeJsonContext.Default.Double)) { Stamp = WireGens.Stamp },
        new BridgeEvent.CaptureCase(WireGens.Artifact, Width: 1280, Height: 720, "BASELINE", "Perspective") { Stamp = WireGens.Stamp },
        new BridgeEvent.PhaseCase(SessionPhase.Execute, PhaseStatus.Failed, DurationMs: 30_000.0, new BridgeFault.ExecuteDeadline("gh.canvas", ElapsedMs: 30_000.0)) { Stamp = WireGens.Stamp },
    ];

    public static TheoryData<BridgeFault> Faults => [
        new BridgeFault.LaunchFailed("bundle missing"),
        new BridgeFault.ConnectFailed("no pipe", ElapsedMs: 90_000.0),
        new BridgeFault.BusyHeld(HolderPid: 777, AgeSeconds: 12.0),
        new BridgeFault.HostDrift("RhinoDoc.Create", WireGens.Host),
        new BridgeFault.CargoRecycleRequired("xx64:active", "xx64:requested"),
        new BridgeFault.RhinoCrash(new CrashFact("/tmp/r.ips", "main", "SIGABRT", "RhMacSignalHandler"), "blocks.baseline"),
        new BridgeFault.ExecuteDeadline("gh.canvas", ElapsedMs: 30_000.0),
        new BridgeFault.CapabilityAbsent("gh2.render", "editor unavailable"),
    ];

    public static TheoryData<EndpointRecord> Endpoints => [
        WireGens.Endpoint,
        new EndpointRecord.Poisoned(RhinoPid: 4242, RhinoStartedAtUnixMs: 1L, "9.0", "shell assembly absent"),
    ];

    public static TheoryData<Gh2Scrub> Scrubs => [
        new Gh2Scrub.NotLoaded(),
        new Gh2Scrub.Scrubbed(Documents: 3, ModifiedBefore: 2, ModifiedAfter: 0),
        new Gh2Scrub.Failed("TypeLoadException"),
    ];

    [Theory]
    [MemberData(nameof(Events), DisableDiscoveryEnumeration = true)]
    public void EventCasesRoundTrip(BridgeEvent evt) {
        ArgumentNullException.ThrowIfNull(evt);
        BridgeEvent back = WireGens.Roundtrip(evt, BridgeJsonContext.Default.BridgeEvent);
        Assert.Equal(evt.Stamp, back.Stamp);
        Assert.Same(evt.GetType(), back.GetType());
        _ = evt switch {
            BridgeEvent.FactCase fact => AssertFact(fact, (BridgeEvent.FactCase)back),
            _ => AssertEqual(evt, back),
        };
        WireGens.ByteIdentical(evt, BridgeJsonContext.Default.BridgeEvent);
    }

    [Theory]
    [MemberData(nameof(Faults), DisableDiscoveryEnumeration = true)]
    public void FaultCasesRoundTrip(BridgeFault fault) {
        Assert.Equal(fault, WireGens.Roundtrip(fault, BridgeJsonContext.Default.BridgeFault));
        WireGens.ByteIdentical(fault, BridgeJsonContext.Default.BridgeFault);
    }

    [Theory]
    [MemberData(nameof(Endpoints), DisableDiscoveryEnumeration = true)]
    public void EndpointCasesRoundTrip(EndpointRecord endpoint) {
        Assert.Equal(endpoint, WireGens.Roundtrip(endpoint, BridgeJsonContext.Default.EndpointRecord));
        WireGens.ByteIdentical(endpoint, BridgeJsonContext.Default.EndpointRecord);
    }

    [Theory]
    [MemberData(nameof(Scrubs), DisableDiscoveryEnumeration = true)]
    public void Gh2ScrubCasesRoundTrip(Gh2Scrub scrub) {
        QuitScrub quit = new(Documents: 1, MarkedClean: 0, ResidualDirty: 0, scrub, ["/tmp/a.3dm"]);
        Assert.Equal(quit, WireGens.Roundtrip(quit, BridgeJsonContext.Default.QuitScrub) with { DirtyPaths = quit.DirtyPaths });
        Assert.Equal(scrub, WireGens.Roundtrip(quit, BridgeJsonContext.Default.QuitScrub).Gh2);
    }

    [Fact]
    public void StubPoisonDocumentDecodesAsThePoisonedCase() {
        EndpointRecord.Poisoned poisoned = Assert.IsType<EndpointRecord.Poisoned>(JsonSerializer.Deserialize(
            """{"$type":"poisoned","rhinoPid":9,"rhinoStartedAtUnixMs":5,"rhinoVersion":"9.0","fault":"shell assembly absent"}""",
            BridgeJsonContext.Default.EndpointRecord));
        Assert.Equal("shell assembly absent", poisoned.Fault);
        Assert.Equal(9, poisoned.RhinoPid);
    }

    [Fact]
    public void DiscriminatorLeadsTheDocument() {
        string json = JsonSerializer.Serialize(Events.First().Data, BridgeJsonContext.Default.BridgeEvent);
        Assert.StartsWith("{\"$type\":\"fact\"", json, StringComparison.Ordinal);
        Assert.Contains("\"sessionId\":\"6a8e6c1e-9f5a-4d2c-8b8e-2f1a3c4d5e6f\"", json, StringComparison.Ordinal);
        Assert.StartsWith("{\"$type\":\"live\"", JsonSerializer.Serialize(WireGens.Endpoint, BridgeJsonContext.Default.EndpointRecord), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("chaos-case")]
    [InlineData("host-exception")]
    [InlineData("evidence")]
    public void UnknownEventDiscriminatorsFailLoud(string discriminator) =>
        _ = Assert.ThrowsAny<JsonException>(() =>
            JsonSerializer.Deserialize(
                $$$"""{"$type":"{{{discriminator}}}","stamp":{"sessionId":"6a8e6c1e-9f5a-4d2c-8b8e-2f1a3c4d5e6f","sequence":1,"atUnixMs":1,"scenario":null}}""",
                BridgeJsonContext.Default.BridgeEvent));

    [Theory]
    [InlineData("future-fault")]
    [InlineData("nuget-lock-drift")]
    [InlineData("ui-wedged")]
    public void UnknownFaultDiscriminatorsFailLoud(string discriminator) =>
        _ = Assert.ThrowsAny<JsonException>(() =>
            JsonSerializer.Deserialize($$"""{"$type":"{{discriminator}}","detail":"x"}""", BridgeJsonContext.Default.BridgeFault));

    [Theory]
    [MemberData(nameof(Faults), DisableDiscoveryEnumeration = true)]
    public void StatusProjectionMatchesTaxonomy(BridgeFault fault) {
        ArgumentNullException.ThrowIfNull(fault);
        Assert.Same(
            fault switch {
                BridgeFault.BusyHeld => PhaseStatus.Busy,
                BridgeFault.ExecuteDeadline => PhaseStatus.Timeout,
                BridgeFault.CapabilityAbsent => PhaseStatus.Unsupported,
                _ => PhaseStatus.Failed,
            },
            fault.Status);
        Assert.False(string.IsNullOrWhiteSpace(fault.Prescription));
    }

    [Fact]
    public void StampedRewritesOnlyTheStamp() {
        EventStamp later = WireGens.Stamp with { Sequence = 99 };
        Assert.All(Events.Select(static row => row.Data), evt => {
            BridgeEvent stamped = evt.Stamped(later);
            Assert.Equal(later, stamped.Stamp);
            Assert.Equal(evt, stamped.Stamped(WireGens.Stamp));
        });
    }

    private static bool AssertFact(BridgeEvent.FactCase expected, BridgeEvent.FactCase actual) {
        Assert.Equal(expected.Key, actual.Key);
        Assert.Equal(expected.Value.GetRawText(), actual.Value.GetRawText());
        return true;
    }

    private static bool AssertEqual(BridgeEvent expected, BridgeEvent actual) {
        Assert.Equal(expected, actual);
        return true;
    }
}

public sealed class ConverterCompositionLaws {
    [Fact]
    public void EvidenceNamesCanonicalizeAndRejectBlankInput() {
        Assert.Null(EvidenceName.Validate("  cleanup.rhino.objects.added  ", provider: null, out EvidenceName? name));
        Assert.Equal("cleanup.rhino.objects.added", name!.Key);
        Assert.NotNull(EvidenceName.Validate("  ", provider: null, out _));
    }

    [Fact]
    public void EvidenceNamesRoundTripAsValidatedKeyStrings() {
        EvidenceName expected = EvidenceName.Create("cleanup.gh2.documents.closed");
        string json = JsonSerializer.Serialize(expected, BridgeJsonContext.Default.EvidenceName);
        Assert.Equal("\"cleanup.gh2.documents.closed\"", json);
        Assert.Equal(expected, JsonSerializer.Deserialize(json, BridgeJsonContext.Default.EvidenceName));
    }

    [Fact]
    public void SmartEnumKeysRoundTripToSingletons() {
        BridgeEvent.PhaseCase back = (BridgeEvent.PhaseCase)WireGens.Roundtrip(
            new BridgeEvent.PhaseCase(SessionPhase.QuitAe, PhaseStatus.Unsupported, DurationMs: 0.5, Fault: null) { Stamp = WireGens.Stamp },
            BridgeJsonContext.Default.BridgeEvent);
        Assert.Same(SessionPhase.QuitAe, back.Phase);
        Assert.Same(PhaseStatus.Unsupported, back.Status);
        Assert.Contains("\"phase\":\"quit.ae\"", JsonSerializer.Serialize(back, BridgeJsonContext.Default.BridgeEvent), StringComparison.Ordinal);
    }


    [Fact]
    public void LivenessWindowIsOneSecondOfStartSkew() =>
        Spec.ForAll(WireGens.ClockSkew, static skew =>
            Assert.Equal(
                Math.Abs(skew) <= EndpointRecord.LivenessSkewMs,
                WireGens.Endpoint.IsLiveFor(WireGens.Endpoint.RhinoPid, WireGens.Endpoint.RhinoStartedAtUnixMs + skew)));

    [Fact]
    public void LivenessRejectsForeignPid() =>
        Assert.False(WireGens.Endpoint.IsLiveFor(WireGens.Endpoint.RhinoPid + 1, WireGens.Endpoint.RhinoStartedAtUnixMs));
}

public sealed class WireVocabularyLaws {
    [Fact]
    public void ReportLayoutRoutesUnderTheReportDirectory() {
        Assert.Equal(Path.Combine("r", "bridge-certificate.json"), ReportLayout.Certificate("r"));
        Assert.Equal(Path.Combine("r", "events", "s.jsonl"), ReportLayout.Spool("r", "s"));
        Assert.Equal(Path.Combine("r", "scratch", "s"), ReportLayout.Scratch("r", "s"));
    }

    [Theory]
    [InlineData("case.volume.status", "assertion")]
    [InlineData("manifest.object.blocks", "object-manifest")]
    [InlineData("manifest.geometry.blocks", "geometry-manifest")]
    [InlineData("manifest.viewport.blocks", "viewport-manifest")]
    [InlineData("manifest.gh2.blocks", "gh2-canvas-manifest")]
    [InlineData("artifact.capture", "artifact")]
    [InlineData("cargo.loaded", "fact")]
    [InlineData("manifest.objects", "fact")]
    [InlineData("capture.frame", "fact")]
    public void FactKeysClassifyThroughTheTypedVocabulary(string key, string role) =>
        Assert.Same(EvidenceRole.Get(role), EvidenceRole.OfFactKey(key));

    [Fact]
    public void ManifestLanesAreExactlyTheManifestPrefixedRoles() =>
        Assert.Equal(
            [.. EvidenceRole.Items.Where(static role => role.FactPrefix.StartsWith("manifest.", StringComparison.Ordinal))],
            EvidenceRole.Items.Where(static role => role.IsManifest));

    [Fact]
    public void EmptyFactPrefixOwnsNoKey() =>
        Assert.All(EvidenceRole.Items.Where(static role => role.FactPrefix.Length == 0), static role => Assert.False(role.OwnsFactKey("anything")));
}

public sealed class SelectionFilterLaws {
    private static readonly ScenarioEntry[] Corpus = [
        new("blocks", "blocks.Baseline", [], BudgetMs: 0),
        new("blocks", "blocks.Insert", [], BudgetMs: 0),
        new("vectors", "vectors.CoreCross", [], BudgetMs: 0),
    ];

    [Fact]
    public void AllSelectsTheWholeCorpus() =>
        Assert.Equal(Corpus, new ScenarioSelection.AllCase().Filter(Corpus));

    [Fact]
    public void ThemesMatchExactAndGlob() {
        Assert.Equal(2, new ScenarioSelection.ThemesCase(["blocks"]).Filter(Corpus).Length);
        Assert.Equal(3, new ScenarioSelection.ThemesCase(["b*", "vec?ors"]).Filter(Corpus).Length);
    }

    [Fact]
    public void NamesMatchExactGlobAndBareMethod() {
        Assert.Equal(["blocks.Baseline"], Names(["blocks.Baseline"]));
        Assert.Equal(["blocks.Baseline", "blocks.Insert"], Names(["blocks.*"]));
        Assert.Equal(["vectors.CoreCross"], Names(["CoreCross"]));
        Assert.Equal(["blocks.Baseline"], Names(["Base*"]));
    }

    [Fact]
    public void MatchingIsCaseSensitiveAndZeroMatchIsEmpty() {
        Assert.Empty(Names(["BLOCKS.*"]));
        Assert.Empty(new ScenarioSelection.ThemesCase(["chaos"]).Filter(Corpus));
        Assert.Empty(Names(["blocks.Base?"]));
    }

    [Fact]
    public void SelectionDiscriminatorsAreFrozen() {
        Assert.StartsWith("{\"$type\":\"all\"", JsonSerializer.Serialize(new ScenarioSelection.AllCase(), BridgeJsonContext.Default.ScenarioSelection), StringComparison.Ordinal);
        Assert.StartsWith("{\"$type\":\"themes\"", JsonSerializer.Serialize(new ScenarioSelection.ThemesCase(["a"]), BridgeJsonContext.Default.ScenarioSelection), StringComparison.Ordinal);
        Assert.StartsWith("{\"$type\":\"names\"", JsonSerializer.Serialize(new ScenarioSelection.NamesCase(["a"]), BridgeJsonContext.Default.ScenarioSelection), StringComparison.Ordinal);
    }

    private static string[] Names(string[] patterns) =>
        [.. new ScenarioSelection.NamesCase(patterns).Filter(Corpus).Select(static entry => entry.Name)];
}

public sealed class EnvelopeWireLaws {
    [Fact]
    public void EnvelopeFieldNamesAreFrozen() {
        SessionEnvelope envelope = new(
            "r", "verify", new StatusBreakdown(PhaseStatus.Ok, PhaseStatus.Ok, PhaseStatus.Ok), DurationMs: 1.0, "d",
            WireGens.Host, [], [new ScenarioOutcome("s", PhaseStatus.Ok, DurationMs: 1.0, Fault: null)], [],
            [], [WireGens.Artifact], new EvidenceCounts(1, 0, 0, 1, 1), new SpoolSummary(1, 1, 1),
            FirstFailure: "", FaultPhase: null, Fault: null);
        using JsonDocument document = JsonDocument.Parse(JsonSerializer.Serialize(envelope, BridgeJsonContext.Default.SessionEnvelope));
        string[] fields = [.. document.RootElement.EnumerateObject().Select(static property => property.Name)];
        Assert.Equal(
            ["runId", "verb", "status", "durationMs", "reportDir", "host", "capabilities", "scenarios", "phases",
                "evidence", "artifacts", "counts", "spool", "firstFailure", "faultPhase", "fault", "exitCode"],
            fields);
        Assert.Equal(["scenario", "session", "overall"], document.RootElement.GetProperty("status").EnumerateObject().Select(static property => property.Name), StringComparer.Ordinal);
        Assert.Equal(["scenario", "status", "durationMs", "fault"], document.RootElement.GetProperty("scenarios")[0].EnumerateObject().Select(static property => property.Name), StringComparer.Ordinal);
        Assert.Equal(0, envelope.ExitCode);
    }

    [Fact]
    public void SpoolDivergenceIsDurableExceedingRelayed() {
        Assert.True(new SpoolSummary(DurableEvents: 4, RelayedEvents: 2, LastSequence: 4).Diverged);
        Assert.False(new SpoolSummary(DurableEvents: 2, RelayedEvents: 2, LastSequence: 2).Diverged);
        Assert.False(new SpoolSummary(DurableEvents: 1, RelayedEvents: 2, LastSequence: 2).Diverged);
    }
}

public sealed class PhaseStatusAlgebraLaws {
    [Fact]
    public void RankIsDeclarationOrderAndExitCodesAreThePolicy() {
        Assert.Equal(Enumerable.Range(0, PhaseStatus.Items.Count), PhaseStatus.Items.Select(static status => status.Rank));
        Assert.Equal(
            [("ok", 0), ("skipped", 0), ("degraded", 2), ("unsupported", 3), ("failed", 1), ("timeout", 5), ("busy", 5)],
            PhaseStatus.Items.Select(static status => (status.Key, status.ExitCode)));
    }

    [Fact]
    public void WorstNeverPicksTheIndecisiveOperand() {
        Assert.Same(PhaseStatus.Ok, PhaseStatus.Ok.Worst(PhaseStatus.Skipped));
        Assert.Same(PhaseStatus.Skipped, PhaseStatus.Skipped.Worst(PhaseStatus.Ok));
        Assert.Same(PhaseStatus.Timeout, PhaseStatus.Failed.Worst(PhaseStatus.Timeout));
        Assert.Same(PhaseStatus.Busy, PhaseStatus.Busy.Worst(PhaseStatus.Timeout));
        Assert.All(PhaseStatus.Items, static status => Assert.Same(status, status.Worst(PhaseStatus.Skipped)));
    }

    [Fact]
    public void WorstIsAnAssociativeIdempotentFoldOverDecisiveStatuses() =>
        Spec.ForAll(WireGens.Status.Select(WireGens.Status, WireGens.Status, static (a, b, c) => (a, b, c)), static triple => {
            Assert.Same(triple.a.Worst(triple.b).Worst(triple.c), triple.a.Worst(triple.b.Worst(triple.c)));
            Assert.Same(triple.a, triple.a.Worst(triple.a));
            Assert.True(triple.a.Worst(triple.b).Rank >= triple.a.Rank);
        });

    [Fact]
    public void OnlySkippedIsIndecisive() =>
        Assert.Equal([PhaseStatus.Skipped], PhaseStatus.Items.Where(static status => !status.IsDecisive));
}
