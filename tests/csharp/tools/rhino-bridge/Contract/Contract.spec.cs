using System.Text.Json;
using Nerdbank.Streams;
using Rasm.TestKit;
using StreamJsonRpc;
using StreamJsonRpc.Protocol;
using StreamJsonRpc.Reflection;

namespace Rasm.Bridge.Contract.Tests;

// --- [MODELS] --------------------------------------------------------------------------

internal static class WireGens {
    public static readonly EventStamp Stamp = new(SessionId: Guid.Parse(input: "6a8e6c1e-9f5a-4d2c-8b8e-2f1a3c4d5e6f"), Sequence: 17, AtUnixMs: 1_765_432_100_123, Scenario: "blocks.baseline");
    public static readonly HostFingerprint Host = new(BundleVersion: "9.0.26153.12416", RhinoCommonVersion: "9.0.26153.12416", Grasshopper2Version: "2.0.0", RuntimeVersion: "10.0.2");
    public static readonly EndpointRecord Endpoint = EndpointRecord.Create(pipeName: "rbx-test", rhinoPid: 4242, rhinoStartedAtUnixMs: 1_765_432_000_000, contractVersion: 1, shellVersion: "1.0.0", rhinoVersion: "9.0.26153", fault: "");
    public static readonly Handshake Shell = new(
        ContractVersion: 1, SenderVersion: "1.0.0",
        Capabilities: [new CapabilityEntry(Key: "rpc.streamjsonrpc", Outcome: PhaseStatus.Ok, Receipt: "2.25.25")],
        Fingerprint: Host, Endpoint: Endpoint);
    public static readonly Gen<string> PipeSuffix = Gen.Char[start: 'a', finish: 'z'].Array[0, 60].Select(selector: static chars => new string(value: chars));
    public static readonly Gen<long> ClockSkew = Gen.Long[start: -5_000L, finish: 5_000L];

    public static BridgeEvent Roundtrip(BridgeEvent evt) =>
        JsonSerializer.Deserialize(json: JsonSerializer.Serialize(value: evt, jsonTypeInfo: BridgeJsonContext.Default.BridgeEvent), jsonTypeInfo: BridgeJsonContext.Default.BridgeEvent)!;
    public static BridgeFault RoundtripFault(BridgeFault fault) =>
        JsonSerializer.Deserialize(json: JsonSerializer.Serialize(value: fault, jsonTypeInfo: BridgeJsonContext.Default.BridgeFault), jsonTypeInfo: BridgeJsonContext.Default.BridgeFault)!;
}

// Protocol stub projects selection cases into receipts so union traversal stays observable.
internal sealed class ShellStub : IBridgeShell {
    public Task<Handshake> HelloAsync(Handshake supervisor, CancellationToken ct) =>
        Task.FromResult(result: supervisor with { SenderVersion = "shell", Fingerprint = WireGens.Host, Endpoint = WireGens.Endpoint });
    public Task<CargoReceipt> LoadCargoAsync(CargoManifest manifest, CancellationToken ct) =>
        Task.FromResult(result: new CargoReceipt(ContentHash: manifest.ContentHash, SwapMs: 412.3, Scenarios: [], Capabilities: []));
    public Task<ScenarioReceipt[]> RunAsync(ScenarioSelection selection, CancellationToken ct) =>
        Task.FromResult<ScenarioReceipt[]>(result: [new ScenarioReceipt(
            Scenario: selection.Switch(
                allCase: static _ => "all",
                themesCase: static t => $"themes:{string.Join(separator: ',', value: t.Themes)}",
                namesCase: static n => $"names:{string.Join(separator: ',', value: n.Names)}"),
            Status: PhaseStatus.Ok, DurationMs: 1.0, Fault: null)]);
    public Task<UnloadReceipt> UnloadCargoAsync(CancellationToken ct) =>
        Task.FromResult(result: new UnloadReceipt(Confirmed: true, DebuggerAttached: false, GcRetries: 0, ElapsedMs: 2.5));
    public Task<long> PingAsync(CancellationToken ct) => Task.FromResult(result: 42L);
    public Task<QuitPrepareReceipt> PrepareQuitAsync(CancellationToken ct) =>
        Task.FromResult(result: new QuitPrepareReceipt(Documents: 0, MarkedClean: 0, ResidualDirty: 0, Gh2: "documents=0;unmodified=0", SavedPaths: []));
}

// Future contract pins JSON-RPC method-not-found behavior instead of fallback.
[JsonRpcContract]
internal partial interface IFutureShell {
    public Task<long> FutureProbeAsync(CancellationToken ct);
}

internal static class RpcPair {
    public static async Task<T> WithClientAsync<T>(IBridgeShell target, Func<JsonRpc, Task<T>> law) {
        ArgumentNullException.ThrowIfNull(argument: law);
        (Stream clientStream, Stream serverStream) = FullDuplexStream.CreatePair();
        using SystemTextJsonFormatter serverFormatter = Formatter();
        using HeaderDelimitedMessageHandler serverHandler = new(duplexStream: serverStream, formatter: serverFormatter);
        using JsonRpc server = new(serverHandler);
        server.AddLocalRpcTarget(target: target);
        server.StartListening();
        using SystemTextJsonFormatter clientFormatter = Formatter();
        using HeaderDelimitedMessageHandler clientHandler = new(duplexStream: clientStream, formatter: clientFormatter);
        using JsonRpc client = new(clientHandler);
        client.StartListening();
        return await law(arg: client).ConfigureAwait(continueOnCapturedContext: false);
    }

    private static SystemTextJsonFormatter Formatter() {
        SystemTextJsonFormatter formatter = new();
        formatter.JsonSerializerOptions.TypeInfoResolverChain.Insert(index: 0, item: BridgeJsonContext.Default);
        return formatter;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class RpcProxyLaws {
    [Fact]
    public async Task ProxyIsSourceGeneratedNotDynamicAsync() =>
        _ = await RpcPair.WithClientAsync(target: new ShellStub(), law: static client => {
            IBridgeShell proxy = client.Attach<IBridgeShell>();
            Assert.False(condition: proxy.GetType().Assembly.IsDynamic);
            _ = Assert.IsAssignableFrom<ProxyBase>(@object: proxy);
            Assert.Same(expected: typeof(IBridgeShell).Assembly, actual: proxy.GetType().Assembly);
            return Task.FromResult(result: true);
        }).ConfigureAwait(continueOnCapturedContext: true);

    [Fact]
    public async Task VerbSurfaceRoundTripsAsync() =>
        _ = await RpcPair.WithClientAsync(target: new ShellStub(), law: static async client => {
            IBridgeShell proxy = client.Attach<IBridgeShell>();
            CancellationToken ct = TestContext.Current.CancellationToken;
            Handshake reply = await proxy.HelloAsync(supervisor: new Handshake(ContractVersion: 1, SenderVersion: "supervisor", Capabilities: [], Fingerprint: null, Endpoint: null), ct: ct).ConfigureAwait(continueOnCapturedContext: false);
            Assert.Equal(expected: "shell", actual: reply.SenderVersion);
            Assert.Equal(expected: WireGens.Endpoint, actual: reply.Endpoint);
            Assert.Equal(expected: WireGens.Host, actual: reply.Fingerprint);
            CargoReceipt cargo = await proxy.LoadCargoAsync(manifest: new CargoManifest(SessionId: WireGens.Stamp.SessionId, ReportDir: "/tmp/rbx", ContentHash: "xx64:abc", StagePath: "/tmp/stage", HostPlugins: [Guid.Parse(input: "b45a29b1-4343-4035-989e-044e8580d9cf")], BuiltAgainst: WireGens.Host, ScenarioAssemblies: ["Rasm.Rhino.Tests.dll"]), ct: ct).ConfigureAwait(continueOnCapturedContext: false);
            Assert.Equal(expected: "xx64:abc", actual: cargo.ContentHash);
            ScenarioReceipt[] receipts = await proxy.RunAsync(selection: new ScenarioSelection.ThemesCase(Themes: ["blocks", "vectors"]), ct: ct).ConfigureAwait(continueOnCapturedContext: false);
            Assert.Equal(expected: "themes:blocks,vectors", actual: receipts[0].Scenario);
            UnloadReceipt unload = await proxy.UnloadCargoAsync(ct: ct).ConfigureAwait(continueOnCapturedContext: false);
            Assert.True(condition: unload.Confirmed);
            Assert.Equal(expected: 42L, actual: await proxy.PingAsync(ct: ct).ConfigureAwait(continueOnCapturedContext: false));
            QuitPrepareReceipt quit = await proxy.PrepareQuitAsync(ct: ct).ConfigureAwait(continueOnCapturedContext: false);
            Assert.Equal(expected: 0, actual: quit.Documents);
            Assert.Equal(expected: 0, actual: quit.MarkedClean);
            Assert.Equal(expected: 0, actual: quit.ResidualDirty);
            Assert.Empty(collection: quit.SavedPaths);
            return true;
        }).ConfigureAwait(continueOnCapturedContext: true);
}

public sealed class UnionWireLaws {
    public static TheoryData<BridgeEvent> Events => [
        new BridgeEvent.FactCase(Key: "cargo.swapMs", Value: JsonSerializer.SerializeToElement(value: 412.3, jsonTypeInfo: BridgeJsonContext.Default.Double)) { Stamp = WireGens.Stamp },
        new BridgeEvent.CaptureCase(Path: ".artifacts/rhino/verify/gh.canvas.png", Width: 1280, Height: 720, OnFailure: true) { Stamp = WireGens.Stamp },
        new BridgeEvent.PhaseCase(Phase: SessionPhase.Execute, Status: PhaseStatus.Failed, DurationMs: 30_000.0, Fault: new BridgeFault.ExecuteDeadline(Scenario: "gh.canvas", ElapsedMs: 30_000.0)) { Stamp = WireGens.Stamp },
        new BridgeEvent.ProgressCase(Done: 3, Total: 21) { Stamp = WireGens.Stamp },
        new BridgeEvent.HostExceptionCase(Report: "System.InvalidOperationException at Rhino.Render") { Stamp = WireGens.Stamp },
    ];

    public static TheoryData<BridgeFault> Faults => [
        new BridgeFault.LaunchFailed(Detail: "bundle missing"),
        new BridgeFault.ConnectFailed(Detail: "no pipe", ElapsedMs: 90_000.0),
        new BridgeFault.BusyHeld(HolderPid: 777, AgeSeconds: 12.0),
        new BridgeFault.ShellSkew(ShellContract: 1, SupervisorContract: 2),
        new BridgeFault.HostDrift(MissingMember: "RhinoDoc.Create", BuiltAgainst: WireGens.Host, Running: WireGens.Host),
        new BridgeFault.CargoUnloadLeak(GcdumpPath: "/tmp/rbx/leak.gcdump"),
        new BridgeFault.RhinoCrash(Crash: new CrashFact(IpsPath: "/tmp/r.ips", CrashThread: "main", ExceptionType: "SIGABRT", Detail: "RhMacSignalHandler"), Scenario: "blocks.baseline"),
        new BridgeFault.DialogSuspected(SilentForMs: 5_000.0),
        new BridgeFault.UiWedged(SilentForMs: 8_000.0, Scenario: "gh.canvas"),
        new BridgeFault.ExecuteDeadline(Scenario: "gh.canvas", ElapsedMs: 30_000.0),
        new BridgeFault.NugetLockDrift(Detail: "NU1004 Rasm.Bridge.Contract"),
        new BridgeFault.CapabilityAbsent(Capability: "gh2.dataflow", ProbeReceipt: "headless solve unsupported"),
        new BridgeFault.RedeployIncomplete(FailingCheck: "mcp.listener"),
    ];

    [Theory]
    [MemberData(memberName: nameof(Events))]
    public void EventCasesRoundTrip(BridgeEvent evt) {
        ArgumentNullException.ThrowIfNull(argument: evt);
        BridgeEvent back = WireGens.Roundtrip(evt: evt);
        Assert.Equal(expected: evt.Stamp, actual: back.Stamp);
        Assert.Same(expected: evt.GetType(), actual: back.GetType());
        _ = evt switch {
            BridgeEvent.FactCase fact => AssertFact(expected: fact, actual: (BridgeEvent.FactCase)back),
            _ => AssertEqual(expected: evt, actual: back),
        };
    }

    [Theory]
    [MemberData(memberName: nameof(Faults))]
    public void FaultCasesRoundTrip(BridgeFault fault) {
        ArgumentNullException.ThrowIfNull(argument: fault);
        Assert.Equal(expected: fault, actual: WireGens.RoundtripFault(fault: fault));
    }

    [Fact]
    public void DiscriminatorLeadsTheDocument() {
        string json = JsonSerializer.Serialize(value: new BridgeEvent.ProgressCase(Done: 1, Total: 2) { Stamp = WireGens.Stamp }, jsonTypeInfo: BridgeJsonContext.Default.BridgeEvent);
        Assert.StartsWith(expectedStartString: "{\"$type\":\"progress\"", actualString: json, comparisonType: StringComparison.Ordinal);
        Assert.Contains(expectedSubstring: "\"sessionId\":\"6a8e6c1e-9f5a-4d2c-8b8e-2f1a3c4d5e6f\"", actualString: json, comparisonType: StringComparison.Ordinal);
        Assert.Contains(expectedSubstring: "\"scenario\":\"blocks.baseline\"", actualString: json, comparisonType: StringComparison.Ordinal);
    }

    [Fact]
    public void UnknownEventTypeFailsLoud() =>
        _ = Assert.ThrowsAny<JsonException>(testCode: static () =>
            JsonSerializer.Deserialize(json: """{"$type":"chaos-case","stamp":{"sessionId":"6a8e6c1e-9f5a-4d2c-8b8e-2f1a3c4d5e6f","sequence":1,"atUnixMs":1,"scenario":null}}""", jsonTypeInfo: BridgeJsonContext.Default.BridgeEvent));

    [Theory]
    [InlineData("evidence")]
    [InlineData("artifact")]
    [InlineData("certificate")]
    public void RetiredEventDiscriminatorsStayRetired(string discriminator) =>
        // The never-constructed union cases were purged; their discriminators must not silently decode.
        _ = Assert.ThrowsAny<JsonException>(testCode: () =>
            JsonSerializer.Deserialize(
                json: """{"$type":"RETIRED","stamp":{"sessionId":"6a8e6c1e-9f5a-4d2c-8b8e-2f1a3c4d5e6f","sequence":1,"atUnixMs":1,"scenario":null}}"""
                    .Replace(oldValue: "RETIRED", newValue: discriminator, comparisonType: StringComparison.Ordinal),
                jsonTypeInfo: BridgeJsonContext.Default.BridgeEvent));

    [Fact]
    public void UnknownFaultTypeFailsLoud() =>
        _ = Assert.ThrowsAny<JsonException>(testCode: static () =>
            JsonSerializer.Deserialize(json: """{"$type":"future-fault","detail":"x"}""", jsonTypeInfo: BridgeJsonContext.Default.BridgeFault));

    [Theory]
    [MemberData(memberName: nameof(Faults))]
    public void StatusProjectionMatchesTaxonomy(BridgeFault fault) {
        ArgumentNullException.ThrowIfNull(argument: fault);
        Assert.Same(
            expected: fault switch {
                BridgeFault.BusyHeld => PhaseStatus.Busy,
                BridgeFault.ExecuteDeadline or BridgeFault.UiWedged => PhaseStatus.Timeout,
                BridgeFault.CapabilityAbsent => PhaseStatus.Unsupported,
                _ => PhaseStatus.Failed,
            },
            actual: fault.Status);
    }

    [Theory]
    [MemberData(memberName: nameof(Faults))]
    public void PrescriptionIsAlwaysActionable(BridgeFault fault) {
        ArgumentNullException.ThrowIfNull(argument: fault);
        Assert.False(condition: string.IsNullOrWhiteSpace(value: fault.Prescription));
    }

    private static bool AssertFact(BridgeEvent.FactCase expected, BridgeEvent.FactCase actual) {
        Assert.Equal(expected: expected.Key, actual: actual.Key);
        Assert.Equal(expected: expected.Value.GetRawText(), actual: actual.Value.GetRawText());
        return true;
    }

    private static bool AssertEqual(BridgeEvent expected, BridgeEvent actual) {
        Assert.Equal(expected: expected, actual: actual);
        return true;
    }
}

public sealed class ConverterCompositionLaws {
    [Fact]
    public void SmartEnumsSerializeAsKeyStrings() {
        string json = JsonSerializer.Serialize(value: new BridgeEvent.PhaseCase(Phase: SessionPhase.Execute, Status: PhaseStatus.Failed, DurationMs: 1.0, Fault: null) { Stamp = WireGens.Stamp }, jsonTypeInfo: BridgeJsonContext.Default.BridgeEvent);
        Assert.Contains(expectedSubstring: "\"phase\":\"execute\"", actualString: json, comparisonType: StringComparison.Ordinal);
        Assert.Contains(expectedSubstring: "\"status\":\"failed\"", actualString: json, comparisonType: StringComparison.Ordinal);
    }

    [Fact]
    public void SmartEnumKeysRoundTripToSingletons() {
        BridgeEvent.PhaseCase back = (BridgeEvent.PhaseCase)WireGens.Roundtrip(evt: new BridgeEvent.PhaseCase(Phase: SessionPhase.QuitAe, Status: PhaseStatus.Unsupported, DurationMs: 0.5, Fault: null) { Stamp = WireGens.Stamp });
        Assert.Same(expected: SessionPhase.QuitAe, actual: back.Phase);
        Assert.Same(expected: PhaseStatus.Unsupported, actual: back.Status);
    }

    [Fact]
    public void HandshakeWithEndpointRoundTrips() {
        Handshake back = JsonSerializer.Deserialize(json: JsonSerializer.Serialize(value: WireGens.Shell, jsonTypeInfo: BridgeJsonContext.Default.Handshake), jsonTypeInfo: BridgeJsonContext.Default.Handshake)!;
        Assert.Equal(expected: WireGens.Shell.ContractVersion, actual: back.ContractVersion);
        Assert.Equal(expected: WireGens.Shell.Capabilities, actual: back.Capabilities);
        Assert.Equal(expected: WireGens.Shell.Fingerprint, actual: back.Fingerprint);
        Assert.Equal(expected: WireGens.Endpoint, actual: back.Endpoint);
    }

    [Fact]
    public void EndpointAdmissionTakesPipePrefixOrEmptyPoison() {
        Assert.Equal(expected: "rbx-", actual: EndpointRecord.PipePrefix);
        Spec.ForAll(gen: WireGens.PipeSuffix, property: static suffix =>
            Assert.Null(@object: EndpointRecord.Validate(pipeName: $"rbx-{suffix}", rhinoPid: 1, rhinoStartedAtUnixMs: 1L, contractVersion: 1, shellVersion: "s", rhinoVersion: "r", fault: "", obj: out _)));
        // Empty pipe is the poison-record shape admitted for typed startup-failure evidence.
        Assert.Null(@object: EndpointRecord.Validate(pipeName: "", rhinoPid: 1, rhinoStartedAtUnixMs: 1L, contractVersion: 1, shellVersion: "s", rhinoVersion: "r", fault: "shell load failed", obj: out _));
        Assert.All(collection: (string[])["rb-old-prefix", "RBX-upper", $"rbx-{new string(c: 'x', count: 61)}"], action: static name =>
            Assert.NotNull(@object: EndpointRecord.Validate(pipeName: name, rhinoPid: 1, rhinoStartedAtUnixMs: 1L, contractVersion: 1, shellVersion: "s", rhinoVersion: "r", fault: "", obj: out _)));
    }

    [Fact]
    public void LivenessWindowIsOneSecondOfStartSkew() =>
        Spec.ForAll(gen: WireGens.ClockSkew, property: static skew =>
            Assert.Equal(
                expected: Math.Abs(value: skew) <= 1_000L,
                actual: WireGens.Endpoint.IsLiveFor(pid: WireGens.Endpoint.RhinoPid, startedAtUnixMs: WireGens.Endpoint.RhinoStartedAtUnixMs + skew)));

    [Fact]
    public void LivenessRejectsForeignPid() =>
        Assert.False(condition: WireGens.Endpoint.IsLiveFor(pid: WireGens.Endpoint.RhinoPid + 1, startedAtUnixMs: WireGens.Endpoint.RhinoStartedAtUnixMs));
}

public sealed class ToleranceLaws {
    [Fact]
    public void UnknownHandshakeFieldsAreSkipped() {
        Handshake back = JsonSerializer.Deserialize(json: """{"contractVersion":7,"senderVersion":"future","capabilities":[],"fingerprint":null,"endpoint":null,"futureField":{"nested":[1,2,3]}}""", jsonTypeInfo: BridgeJsonContext.Default.Handshake)!;
        Assert.Equal(expected: 7, actual: back.ContractVersion);
        Assert.Equal(expected: "future", actual: back.SenderVersion);
    }

    [Fact]
    public void UnknownEndpointFieldsAreSkipped() {
        Handshake back = JsonSerializer.Deserialize(json: """{"contractVersion":1,"senderVersion":"s","capabilities":[],"fingerprint":null,"endpoint":{"pipeName":"rbx-test","rhinoPid":4242,"rhinoStartedAtUnixMs":1765432000000,"contractVersion":1,"shellVersion":"1.0.0","rhinoVersion":"9.0.26153","futureField":true}}""", jsonTypeInfo: BridgeJsonContext.Default.Handshake)!;
        Assert.Equal(expected: WireGens.Endpoint, actual: back.Endpoint);
    }

    [Fact]
    public async Task MissingMethodSurfacesAsMethodNotFoundAsync() =>
        _ = await RpcPair.WithClientAsync(target: new ShellStub(), law: static async client => {
            IFutureShell future = client.Attach<IFutureShell>();
            RemoteMethodNotFoundException missing = await Assert.ThrowsAsync<RemoteMethodNotFoundException>(testCode: async () =>
                _ = await future.FutureProbeAsync(ct: TestContext.Current.CancellationToken).ConfigureAwait(continueOnCapturedContext: false)).ConfigureAwait(continueOnCapturedContext: false);
            Assert.Equal(expected: JsonRpcErrorCode.MethodNotFound, actual: missing.ErrorCode);
            return true;
        }).ConfigureAwait(continueOnCapturedContext: true);
}

// The report-dir layout and fact-key vocabularies are frozen wire law the assay workstation decodes.
public sealed class WireVocabularyLaws {
    [Fact]
    public void ReportLayoutStringsAreFrozen() {
        Assert.Equal(expected: "bridge-certificate.json", actual: ReportLayout.CertificateFile);
        Assert.Equal(expected: "captures", actual: ReportLayout.CapturesDirectory);
        Assert.Equal(expected: "events", actual: ReportLayout.EventsDirectory);
        Assert.Equal(expected: "gh2", actual: ReportLayout.Gh2Directory);
        Assert.Equal(expected: "manifests", actual: ReportLayout.ManifestsDirectory);
        Assert.Equal(expected: "references", actual: ReportLayout.ReferencesDirectory);
        Assert.Equal(expected: "scratch", actual: ReportLayout.ScratchDirectory);
        Assert.Equal(expected: Path.Combine("r", "bridge-certificate.json"), actual: ReportLayout.Certificate(reportDir: "r"));
        Assert.Equal(expected: Path.Combine("r", "events", "s.jsonl"), actual: ReportLayout.Spool(reportDir: "r", scenario: "s"));
    }

    [Fact]
    public void EvidenceModeKeysAreTheFrozenTokens() {
        Assert.Equal(expected: ["verify", "author"], actual: EvidenceMode.Items.Select(selector: static mode => mode.Key));
        Assert.Same(expected: EvidenceMode.Author, actual: EvidenceMode.Get(key: "author"));
        Assert.False(condition: EvidenceMode.TryGet(key: "chaos", item: out _));
    }

    [Fact]
    public void ReferenceAdmissionKeysStayAdditive() =>
        Assert.Equal(
            expected: ["reviewed", "candidate", "unpromoted", "missing", "mismatch", "matched"],
            actual: ReferenceAdmission.Items.Select(selector: static admission => admission.Key));

    [Theory]
    [InlineData("case.volume.status", "assertion")]
    [InlineData("case.volume.start", "assertion")]
    [InlineData("reference.volume", "reference")]
    [InlineData("manifest.object.blocks", "object-manifest")]
    [InlineData("manifest.geometry.blocks", "geometry-manifest")]
    [InlineData("manifest.viewport.blocks", "viewport-manifest")]
    [InlineData("manifest.gh2.blocks", "gh2-canvas-manifest")]
    [InlineData("artifact.capture", "artifact")]
    [InlineData("cargo.swapped", "fact")]
    [InlineData("manifest.objects", "fact")]
    [InlineData("reference", "fact")]
    [InlineData("capture.frame", "fact")]
    public void FactKeysClassifyThroughTheTypedVocabulary(string key, string role) =>
        Assert.Same(expected: EvidenceRole.Get(key: role), actual: EvidenceRole.OfFactKey(key: key));

    [Fact]
    public void FactArgumentStripsExactlyTheOwnedPrefix() {
        Assert.Equal(expected: "volume", actual: EvidenceRole.Reference.FactArgument(key: "reference.volume"));
        Assert.Equal(expected: "unowned", actual: EvidenceRole.Reference.FactArgument(key: "unowned"));
        Assert.False(condition: EvidenceRole.Fact.OwnsFactKey(key: "anything"));
    }
}

// Selection matching is one Contract-owned semantics: ordinal exact, fnmatch-style wildcards, and
// bare-method-name hits; zero-match returns empty so the shell raises its typed fault.
public sealed class SelectionFilterLaws {
    private static readonly ScenarioEntry[] Corpus = [
        new ScenarioEntry(Theme: "blocks", Name: "blocks.Baseline", Requires: [], BudgetMs: 0),
        new ScenarioEntry(Theme: "blocks", Name: "blocks.Insert", Requires: [], BudgetMs: 0),
        new ScenarioEntry(Theme: "vectors", Name: "vectors.CoreRail", Requires: [], BudgetMs: 0),
    ];

    [Fact]
    public void AllSelectsTheWholeCorpus() =>
        Assert.Equal(expected: Corpus, actual: new ScenarioSelection.AllCase().Filter(entries: Corpus));

    [Fact]
    public void ThemesMatchExactAndGlob() {
        Assert.Equal(expected: 2, actual: new ScenarioSelection.ThemesCase(Themes: ["blocks"]).Filter(entries: Corpus).Length);
        Assert.Equal(expected: 3, actual: new ScenarioSelection.ThemesCase(Themes: ["b*", "vec?ors"]).Filter(entries: Corpus).Length);
    }

    [Fact]
    public void NamesMatchExactGlobAndBareMethod() {
        Assert.Equal(expected: ["blocks.Baseline"], actual: new ScenarioSelection.NamesCase(Names: ["blocks.Baseline"]).Filter(entries: Corpus).Select(selector: static entry => entry.Name));
        Assert.Equal(expected: ["blocks.Baseline", "blocks.Insert"], actual: new ScenarioSelection.NamesCase(Names: ["blocks.*"]).Filter(entries: Corpus).Select(selector: static entry => entry.Name));
        Assert.Equal(expected: ["vectors.CoreRail"], actual: new ScenarioSelection.NamesCase(Names: ["CoreRail"]).Filter(entries: Corpus).Select(selector: static entry => entry.Name));
        Assert.Equal(expected: ["blocks.Baseline"], actual: new ScenarioSelection.NamesCase(Names: ["Base*"]).Filter(entries: Corpus).Select(selector: static entry => entry.Name));
    }

    [Fact]
    public void MatchingIsCaseSensitiveAndZeroMatchIsEmpty() {
        Assert.Empty(collection: new ScenarioSelection.NamesCase(Names: ["BLOCKS.*"]).Filter(entries: Corpus));
        Assert.Empty(collection: new ScenarioSelection.ThemesCase(Themes: ["chaos"]).Filter(entries: Corpus));
        Assert.Empty(collection: new ScenarioSelection.NamesCase(Names: ["blocks.Base?"]).Filter(entries: Corpus));
    }
}

// The exact camelCase field names the assay workstation decoder binds; removal or rename is a
// schema_version event, never a silent drift.
public sealed class EnvelopeWireLaws {
    [Fact]
    public void EnvelopeFieldNamesAreFrozen() {
        SessionEnvelope envelope = new(
            RunId: "r", Verb: "verify", Status: PhaseStatus.Ok, DurationMs: 1.0, ReportDir: "d",
            Host: WireGens.Host, Capabilities: [], Scenarios: [new ScenarioReceipt(Scenario: "s", Status: PhaseStatus.Ok, DurationMs: 1.0, Fault: null)],
            Evidence: [], FirstFailure: "", FirstFaultPhase: null, Fault: null);
        using JsonDocument document = JsonDocument.Parse(json: JsonSerializer.Serialize(value: envelope, jsonTypeInfo: BridgeJsonContext.Default.SessionEnvelope));
        string[] fields = [.. document.RootElement.EnumerateObject().Select(selector: static property => property.Name)];
        Assert.All(
            collection: (string[])[
                "runId", "verb", "status", "scenarioStatus", "sessionStatus", "durationMs", "reportDir",
                "host", "capabilities", "scenarios", "evidence", "firstFailure", "firstScenarioFailure",
                "firstSessionFault", "firstFaultPhase", "fault", "phaseReceipts", "certificatePath",
                "artifactRefs", "evidenceCounts", "scenarioCounts", "spool"],
            action: field => Assert.Contains(expected: field, collection: fields));
        string[] scenarioFields = [.. document.RootElement.GetProperty(propertyName: "scenarios")[0].EnumerateObject().Select(selector: static property => property.Name)];
        Assert.All(
            collection: (string[])["scenario", "status", "scenarioStatus", "durationMs", "fault", "referenceResults", "firstScenarioFailure"],
            action: field => Assert.Contains(expected: field, collection: scenarioFields));
    }

    [Fact]
    public void SelectionDiscriminatorsAreFrozen() {
        Assert.StartsWith(expectedStartString: "{\"$type\":\"all\"", actualString: JsonSerializer.Serialize(value: new ScenarioSelection.AllCase(), jsonTypeInfo: BridgeJsonContext.Default.ScenarioSelection), comparisonType: StringComparison.Ordinal);
        Assert.StartsWith(expectedStartString: "{\"$type\":\"themes\"", actualString: JsonSerializer.Serialize(value: new ScenarioSelection.ThemesCase(Themes: ["a"]), jsonTypeInfo: BridgeJsonContext.Default.ScenarioSelection), comparisonType: StringComparison.Ordinal);
        Assert.StartsWith(expectedStartString: "{\"$type\":\"names\"", actualString: JsonSerializer.Serialize(value: new ScenarioSelection.NamesCase(Names: ["a"]), jsonTypeInfo: BridgeJsonContext.Default.ScenarioSelection), comparisonType: StringComparison.Ordinal);
    }
}

public sealed class PhaseStatusAlgebraLaws {
    [Fact]
    public void RanksFormTheAmendedTotalOrder() =>
        Assert.Equal(
            expected: [("ok", 1, 0), ("skipped", 1, 0), ("degraded", 2, 2), ("unsupported", 3, 3), ("failed", 4, 1), ("timeout", 5, 5), ("busy", 6, 5)],
            actual: PhaseStatus.Items.Select(selector: static status => (status.Key, status.Rank, status.ExitCode)));

    [Fact]
    public void WorstKeepsTheAccumulatorOnRankTies() {
        Assert.Same(expected: PhaseStatus.Ok, actual: PhaseStatus.Ok.Worst(other: PhaseStatus.Skipped));
        Assert.Same(expected: PhaseStatus.Skipped, actual: PhaseStatus.Skipped.Worst(other: PhaseStatus.Ok));
        Assert.Same(expected: PhaseStatus.Timeout, actual: PhaseStatus.Failed.Worst(other: PhaseStatus.Timeout));
        Assert.Same(expected: PhaseStatus.Busy, actual: PhaseStatus.Busy.Worst(other: PhaseStatus.Timeout));
    }

    [Fact]
    public void WorstIsAnAssociativeIdempotentFold() =>
        Assert.All(
            collection: PhaseStatus.Items.SelectMany(collectionSelector: static _ => PhaseStatus.Items, resultSelector: static (a, b) => (a, b)).SelectMany(collectionSelector: static _ => PhaseStatus.Items, resultSelector: static (ab, c) => (ab.a, ab.b, c)),
            action: static triple => {
                Assert.Same(expected: triple.a.Worst(other: triple.b).Worst(other: triple.c), actual: triple.a.Worst(other: triple.b.Worst(other: triple.c)));
                Assert.Same(expected: triple.a, actual: triple.a.Worst(other: triple.a));
            });

    [Fact]
    public void OnlySkippedIsIndecisive() =>
        Assert.Equal(
            expected: [true, false, true, true, true, true, true],
            actual: PhaseStatus.Items.Select(selector: static status => status.IsDecisive));
}

