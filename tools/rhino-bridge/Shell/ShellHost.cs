using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO.Pipes;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Channels;
using Rasm.Bridge.Contract;
using Rhino;
using Rhino.PlugIns;
using Rhino.Runtime;
using StreamJsonRpc;

namespace Rasm.Bridge.Shell;

// --- [TYPES] --------------------------------------------------------------------------------

// Ownership: closed GH2 quit-scrub outcome. The reflective Document.AllDocuments + Unmodify path
// projects to one of these cases so its failure surfaces as a typed fact instead of a sentinel string.
[Union]
internal abstract partial record Gh2ScrubOutcome {
    private Gh2ScrubOutcome() { }
    public sealed record NotLoaded : Gh2ScrubOutcome;
    public sealed record Scrubbed(int Documents, int Unmodified) : Gh2ScrubOutcome;
    public sealed record Failed(string Detail) : Gh2ScrubOutcome;

    public string Summary => Switch(
        notLoaded: static _ => "not-loaded",
        scrubbed: static s => string.Create(provider: CultureInfo.InvariantCulture, $"documents={s.Documents};unmodified={s.Unmodified}"),
        failed: static f => $"reflective-scrub-failed:{f.Detail}");
}

// --- [SERVICES] -----------------------------------------------------------------------------

// Ownership: shell composition root and RPC target seam. It owns pipe lifecycle, endpoint
// write/heal, host event taps, busy admission, and Contract payload projection through CargoGate
// and IdlePump. Failed starts poison the endpoint record instead of deleting evidence.
public sealed class ShellHost : IDisposable {
    private const int FaultErrorCode = -32050;
    private const int PipeInstances = 4;
    // McNeel Rhino-MCP-Platform PlugIn GUID ([assembly: Guid] in its AssemblyInfo); the community fork uses a distinct GUID.
    private const string McneelPlugInGuid = "2668d7ed-f507-4a68-8295-8172147a0e39";

    private readonly Lock sync = new();
    private readonly ConcurrentQueue<string> defaultResolves = new();
    private readonly Channel<BridgeEvent> outbox = Channel.CreateUnbounded<BridgeEvent>(options: new UnboundedChannelOptions { SingleReader = true });
    private readonly CancellationTokenSource lifetime = new();
    private readonly Func<AssemblyLoadContext, AssemblyName, Assembly?> resolvingTap;
    private readonly HostUtils.ExceptionReportDelegate reportTap;
    private readonly EventHandler closingTap;
    private readonly EndpointRecord endpoint;
    private readonly HostFingerprint fingerprint;
    private readonly CapabilityEntry shellContent;
    private readonly IdlePump pump;
    private readonly CargoGate gate;
    private readonly Task[] acceptLoops;
    private readonly Task forwarder;
    private CargoManifest? activeManifest;
    private Connection? owner;
    private long ownedAtUnixMs;
    private long sequence;
    private bool disposed;

    private ShellHost(string deployDir, int rhinoPid) {
        endpoint = EndpointRecord.Create(
            pipeName: string.Create(provider: CultureInfo.InvariantCulture, $"{EndpointRecord.PipePrefix}{rhinoPid}-{Guid.NewGuid().ToString(format: "N")[..8]}"),
            rhinoPid: rhinoPid,
            rhinoStartedAtUnixMs: HostStartedAtUnixMs(),
            contractVersion: Handshake.CurrentVersion,
            shellVersion: ShellVersion,
            rhinoVersion: RhinoApp.Version.ToString(),
            fault: string.Empty);
        fingerprint = RunningFingerprint();
        shellContent = ShellContent(deployDir: deployDir);
        resolvingTap = (_, name) => RecordDefaultResolve(assemblyName: name);
        reportTap = (source, error) => Publish(evt: new BridgeEvent.HostExceptionCase(Report: $"{source}: {error.GetType().Name}: {error.Message}") { Stamp = default });
        closingTap = (_, _) => Publish(evt: new BridgeEvent.PhaseCase(Phase: SessionPhase.QuitAe, Status: PhaseStatus.Ok, DurationMs: 0.0, Fault: null) { Stamp = default });
        pump = new IdlePump();
        gate = new CargoGate();
        AssemblyLoadContext.Default.Resolving += resolvingTap;
        HostUtils.OnExceptionReport += reportTap;
        RhinoApp.Closing += closingTap;
        WriteEndpoint(record: endpoint);
        forwarder = ForwardLoopAsync(token: lifetime.Token);
        acceptLoops = [.. Enumerable.Range(start: 0, count: PipeInstances).Select(_ => AcceptLoopAsync(token: lifetime.Token))];
    }

    internal bool IsRunning =>
        !lifetime.IsCancellationRequested && !forwarder.IsCompleted && acceptLoops.Any(predicate: static loop => !loop.IsCompleted);

    private static string ShellVersion =>
        typeof(ShellHost).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? typeof(ShellHost).Assembly.GetName().Version?.ToString()
        ?? "unknown";

    public static ShellHost? Start(string deployDir, int rhinoPid) {
        // BOUNDARY ADAPTER: startup failures become poisoned endpoint records, never host faults.
        try {
            return new ShellHost(deployDir: deployDir, rhinoPid: rhinoPid);
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException or InvalidOperationException
            or ArgumentException or NotSupportedException or System.ComponentModel.DataAnnotations.ValidationException) {
            WritePoisoned(rhinoPid: rhinoPid, fault: error.GetBaseException().Message);
            return null;
        }
    }

    public void Dispose() {
        bool alreadyDisposed;
        lock (sync) {
            alreadyDisposed = disposed;
            disposed = true;
        }
        if (!alreadyDisposed) {
            RhinoApp.Closing -= closingTap;
            HostUtils.OnExceptionReport -= reportTap;
            AssemblyLoadContext.Default.Resolving -= resolvingTap;
            lifetime.Cancel();
            _ = outbox.Writer.TryComplete();
            pump.Dispose();
            gate.Dispose();
            lifetime.Dispose();
            // Endpoint records stay as staleness evidence after shell disposal.
        }
    }

    // --- [CONNECTION_TARGET]

    // Ownership: per-connection RPC target carrying the busy-admission identity.
    private sealed class Connection(ShellHost host, JsonRpc rpc, IBridgeEvents events) : IBridgeShell {
        internal IBridgeEvents Events { get; } = events;
        internal int ClientPid { get; set; }
        internal bool IsAlive => !rpc.Completion.IsCompleted;

        public Task<Handshake> HelloAsync(Handshake supervisor, CancellationToken ct) =>
            Task.FromResult(result: host.Hello(connection: this, supervisor: supervisor));

        public Task<CargoReceipt> LoadCargoAsync(CargoManifest manifest, CancellationToken ct) =>
            host.LoadCargoAsync(connection: this, manifest: manifest, ct: ct);

        public Task<ScenarioReceipt[]> RunAsync(ScenarioSelection selection, CancellationToken ct) =>
            host.RunAsync(connection: this, selection: selection, ct: ct);

        public Task<UnloadReceipt> UnloadCargoAsync(CancellationToken ct) =>
            host.UnloadCargoAsync(connection: this, ct: ct);

        public Task<long> PingAsync(CancellationToken ct) =>
            Task.FromResult(result: Environment.TickCount64);

        public Task<QuitPrepareReceipt> PrepareQuitAsync(CancellationToken ct) =>
            host.PrepareQuitAsync(connection: this, ct: ct);
    }

    // --- [VERBS]

    private Handshake Hello(Connection connection, Handshake supervisor) {
        ArgumentNullException.ThrowIfNull(argument: supervisor);
        connection.ClientPid = ClientPidOf(capabilities: supervisor.Capabilities);
        EnsureEndpoint();
        return new Handshake(
            ContractVersion: Handshake.CurrentVersion,
            SenderVersion: ShellVersion,
            Capabilities: [
                new CapabilityEntry(Key: "rpc.streamjsonrpc", Outcome: PhaseStatus.Ok, Receipt: RpcAssemblyVersion),
                new CapabilityEntry(Key: "alc.default.resolving", Outcome: PhaseStatus.Ok, Receipt: DefaultResolveReceipt()),
                shellContent,
                McpListener(),
                McpPlatform(),
            ],
            Fingerprint: fingerprint,
            Endpoint: endpoint);
    }

    private async Task<CargoReceipt> LoadCargoAsync(Connection connection, CargoManifest manifest, CancellationToken ct) {
        ArgumentNullException.ThrowIfNull(argument: manifest);
        Admit(connection: connection);
        activeManifest = manifest;
        return await pump.OnUiThreadAsync(job: () => {
            PreloadHostPlugins(plugins: manifest.HostPlugins);
            // The post-preload fingerprint is the drift baseline cargo attributes HostDrift against.
            CargoReceipt receipt = gate.Swap(manifest: manifest, running: RunningFingerprint(), publish: Publish);
            Publish(evt: BridgeEvent.Fact(key: "scenario.discovered.count", value: receipt.Scenarios.Length.ToString(provider: CultureInfo.InvariantCulture)));
            Publish(evt: BridgeEvent.Fact(key: "scenario.discovered.names", value: string.Join(separator: ',', values: receipt.Scenarios.Select(selector: static scenario => scenario.Name))));
            return receipt;
        }, ct: ct).ConfigureAwait(continueOnCapturedContext: false);
    }

    private async Task<ScenarioReceipt[]> RunAsync(Connection connection, ScenarioSelection selection, CancellationToken ct) {
        ArgumentNullException.ThrowIfNull(argument: selection);
        Admit(connection: connection);
        IBridgeCargo cargo = gate.Current ?? throw new LocalRpcException(message: "no cargo loaded: LoadCargoAsync precedes RunAsync") { ErrorCode = FaultErrorCode };
        ScenarioEntry[] discovered = await pump.OnUiThreadAsync(job: cargo.Discover, ct: ct).ConfigureAwait(continueOnCapturedContext: false);
        ScenarioEntry[] selected = selection.Filter(entries: discovered);
        Publish(evt: BridgeEvent.Fact(key: "scenario.selected.count", value: selected.Length.ToString(provider: CultureInfo.InvariantCulture)));
        Publish(evt: BridgeEvent.Fact(key: "scenario.selected.names", value: string.Join(separator: ',', values: selected.Select(selector: static scenario => scenario.Name))));
        if (selected.Length == 0) {
            BridgeFault fault = new BridgeFault.CapabilityAbsent(
                Capability: "scenario.selection",
                ProbeReceipt: $"selection matched zero scenarios; discovered={string.Join(separator: ',', values: discovered.Select(selector: static scenario => scenario.Name))}");
            throw new LocalRpcException(message: fault.Prescription) {
                ErrorCode = FaultErrorCode,
                ErrorData = JsonSerializer.SerializeToElement(value: fault, jsonTypeInfo: BridgeJsonContext.Default.BridgeFault),
            };
        }
        ScenarioReceipt[] receipts = new ScenarioReceipt[selected.Length];
        for (int index = 0; index < selected.Length; index++) {
            ScenarioEntry entry = selected[index];
            receipts[index] = await pump.OnUiThreadAsync(job: () => cargo.Run(scenario: entry, publish: Publish), ct: ct).ConfigureAwait(continueOnCapturedContext: false);
        }
        return receipts;
    }

    private async Task<UnloadReceipt> UnloadCargoAsync(Connection connection, CancellationToken ct) {
        Admit(connection: connection);
        UnloadReceipt receipt = await pump.OnUiThreadAsync(job: gate.Unload, ct: ct).ConfigureAwait(continueOnCapturedContext: false);
        ReleaseOwner(connection: connection);
        return receipt;
    }

    private async Task<QuitPrepareReceipt> PrepareQuitAsync(Connection connection, CancellationToken ct) {
        Admit(connection: connection);
        // UI-thread scrub, then re-read: ResidualDirty == 0 is the AE-rung precondition that keeps
        // `terminate` off the AppKit save sheet; the GH2 outcome stays a typed union, never a sentinel.
        (QuitPrepareReceipt receipt, Gh2ScrubOutcome gh2) = await pump.OnUiThreadAsync(job: static () => {
            RhinoDoc[] open = RhinoDoc.OpenDocuments();
            int markedClean = open.Count(predicate: static doc => doc.Modified);
            Array.ForEach(array: open, action: static doc => doc.Modified = false);
            Gh2ScrubOutcome scrub = CleanGrasshopper2();
            RhinoDoc[] residual = RhinoDoc.OpenDocuments();
            int residualDirty = residual.Count(predicate: static doc => doc.Modified);
            string[] savedPaths = [.. residual.Where(predicate: static doc => doc.Modified && doc.Path is { Length: > 0 }).Select(selector: static doc => doc.Path)];
            return (new QuitPrepareReceipt(Documents: open.Length, MarkedClean: markedClean, ResidualDirty: residualDirty, Gh2: scrub.Summary, SavedPaths: savedPaths), scrub);
        }, ct: ct).ConfigureAwait(continueOnCapturedContext: false);
        Publish(evt: BridgeEvent.Fact(
            key: "quit.prepared",
            value: string.Create(provider: CultureInfo.InvariantCulture,
                $"{(receipt.Scrubbed ? "rhino-docs-marked-clean" : "rhino-docs-residual-dirty")}; documents={receipt.Documents};markedClean={receipt.MarkedClean};residualDirty={receipt.ResidualDirty}; gh2={receipt.Gh2}")));
        if (gh2 is Gh2ScrubOutcome.Failed) {
            Publish(evt: BridgeEvent.Fact(key: "quit.scrub.gh2-reflective-failed", value: receipt.Gh2));
        }
        return receipt;
    }

    // --- [ADMISSION]

    private void Admit(Connection connection) {
        lock (sync) {
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (owner is { } held && !ReferenceEquals(objA: held, objB: connection) && held.IsAlive) {
                BridgeFault fault = new BridgeFault.BusyHeld(HolderPid: held.ClientPid, AgeSeconds: (now - ownedAtUnixMs) / 1000.0);
                throw new LocalRpcException(message: fault.Prescription) {
                    ErrorCode = FaultErrorCode,
                    ErrorData = JsonSerializer.SerializeToElement(value: fault, jsonTypeInfo: BridgeJsonContext.Default.BridgeFault),
                };
            }
            if (!ReferenceEquals(objA: owner, objB: connection)) {
                owner = connection;
                ownedAtUnixMs = now;
            }
        }
    }

    private void ReleaseOwner(Connection connection) {
        lock (sync) {
            if (ReferenceEquals(objA: owner, objB: connection)) {
                owner = null;
            }
        }
    }

    // --- [EVIDENCE]

    // Single in-host writer: shell stamps every event while preserving the author's scenario slot.
    private void Publish(BridgeEvent evt) {
        EventStamp stamp = new(
            SessionId: activeManifest?.SessionId ?? Guid.Empty,
            Sequence: Interlocked.Increment(location: ref sequence),
            AtUnixMs: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Scenario: evt.Stamp.Scenario);
        BridgeEvent stamped = evt switch {
            BridgeEvent.FactCase row => row with { Stamp = stamp },
            BridgeEvent.CaptureCase row => row with { Stamp = stamp },
            BridgeEvent.PhaseCase row => row with { Stamp = stamp },
            BridgeEvent.ProgressCase row => row with { Stamp = stamp },
            BridgeEvent.HostExceptionCase row => row with { Stamp = stamp },
            _ => evt,
        };
        _ = outbox.Writer.TryWrite(item: stamped);
    }

    private async Task ForwardLoopAsync(CancellationToken token) {
        // BOUNDARY ADAPTER: dead peers drop forwarded events; spool remains the durable rail.
        try {
            await foreach (BridgeEvent evt in outbox.Reader.ReadAllAsync(cancellationToken: token).ConfigureAwait(continueOnCapturedContext: false)) {
                if (CurrentOwner() is { Events: { } events }) {
                    try {
                        await events.PublishAsync(evt: evt).ConfigureAwait(continueOnCapturedContext: false);
                    } catch (Exception error) when (error is IOException or ObjectDisposedException or ConnectionLostException or RemoteInvocationException) {
                        Debug.WriteLine(message: $"event forward dropped: {error.Message}");
                    }
                }
            }
        } catch (OperationCanceledException) when (token.IsCancellationRequested) {
        }
    }

    private Connection? CurrentOwner() {
        lock (sync) {
            return owner;
        }
    }

    private Assembly? RecordDefaultResolve(AssemblyName assemblyName) {
        // Default-ALC fallthrough names are reported through hello for deploy-set trimming.
        defaultResolves.Enqueue(item: assemblyName.Name ?? "unknown");
        return null;
    }

    private string DefaultResolveReceipt() {
        string[] names = [.. defaultResolves.Distinct(comparer: StringComparer.Ordinal).Order(comparer: StringComparer.Ordinal)];
        return string.Create(provider: CultureInfo.InvariantCulture, $"{names.Length}:{string.Join(separator: ',', value: names)}");
    }

    private static CapabilityEntry McpListener() =>
        // Autostart is never driven by the bridge; the gate distinguishes deliberate suppression
        // from an unavailable gate so the absent listener is never silently identical evidence.
        Environment.GetEnvironmentVariable(variable: "RHINO_MCP_AUTOSTART_PORT") switch {
            "0" => new CapabilityEntry(Key: "mcp.listener", Outcome: PhaseStatus.Unsupported,
                Receipt: "autostart suppressed by RHINO_MCP_AUTOSTART_PORT=0; bridge did not start a listener"),
            { Length: > 0 } port => new CapabilityEntry(Key: "mcp.listener", Outcome: PhaseStatus.Unsupported,
                Receipt: $"listener autostart gate unavailable; bridge owns no autostart for RHINO_MCP_AUTOSTART_PORT={port}"),
            _ => new CapabilityEntry(Key: "mcp.listener", Outcome: PhaseStatus.Unsupported,
                Receipt: "listener autostart gate unavailable; bridge did not start a listener"),
        };

    private static CapabilityEntry McpPlatform() {
        string[] loaded = [.. AppDomain.CurrentDomain.GetAssemblies()
            .Where(predicate: IsMcneelRhinoMcp)
            .Select(selector: static assembly => assembly.GetName())
            .Select(selector: static name => $"{name.Name}:{name.Version}")
            .Order(comparer: StringComparer.Ordinal)];
        return loaded.Length == 0
            ? new CapabilityEntry(Key: "mcp.platform.version", Outcome: PhaseStatus.Unsupported, Receipt: "McNeel Rhino-MCP-Platform not loaded")
            : new CapabilityEntry(Key: "mcp.platform.version", Outcome: PhaseStatus.Ok, Receipt: string.Join(separator: ',', value: loaded));
    }

    private static bool IsMcneelRhinoMcp(Assembly assembly) =>
        // McNeel's official Rhino-MCP-Platform ships assembly RhinoMcpPlatform / namespace RhMcp and
        // pins its PlugIn GUID via [assembly: Guid]; the community fork (assembly rhinomcp) carries
        // neither, so the rename-stable GUID is the primary signal with the identity pair as fallback.
        string.Equals(a: assembly.GetCustomAttribute<GuidAttribute>()?.Value, b: McneelPlugInGuid, comparisonType: StringComparison.OrdinalIgnoreCase)
        || (string.Equals(a: assembly.GetName().Name, b: "RhinoMcpPlatform", comparisonType: StringComparison.Ordinal)
            && HostReflection.LoadableTypes(assembly: assembly).Any(predicate: static type => IsRhMcpNamespace(ns: type.Namespace)));

    private static bool IsRhMcpNamespace(string? ns) =>
        ns is "RhMcp" || (ns?.StartsWith(value: "RhMcp.", comparisonType: StringComparison.Ordinal) ?? false);

    private static CapabilityEntry ShellContent(string deployDir) {
        string path = Path.Combine(path1: deployDir, path2: "Rasm.Bridge.Shell.dll");
        try {
            return File.Exists(path: path)
                ? new CapabilityEntry(Key: Handshake.ShellContentCapability, Outcome: PhaseStatus.Ok,
                    Receipt: Convert.ToHexStringLower(inArray: SHA256.HashData(source: File.ReadAllBytes(path: path))))
                : new CapabilityEntry(Key: Handshake.ShellContentCapability, Outcome: PhaseStatus.Failed, Receipt: $"missing:{path}");
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException) {
            return new CapabilityEntry(Key: Handshake.ShellContentCapability, Outcome: PhaseStatus.Failed, Receipt: $"{error.GetType().Name}: {error.Message}");
        }
    }

    private void PreloadHostPlugins(Guid[] plugins) {
        // Host plugin preload records false returns and throws under the same capability reading.
        foreach (Guid id in plugins) {
            string outcome;
            try {
                outcome = PlugIn.GetPlugInInfo(pluginId: id)?.IsLoaded == true
                    ? "already-loaded"
                    : PlugIn.LoadPlugIn(pluginId: id, loadQuietly: true, forceLoad: false)
                        ? PlugIn.GetPlugInInfo(pluginId: id)?.IsLoaded == true ? "loaded" : "load-true-not-loaded"
                        : "load-returned-false";
            } catch (Exception error) when (error is not OutOfMemoryException and not StackOverflowException and not AccessViolationException) {
                outcome = $"threw {error.GetType().Name}: {error.Message}";
            }
            Publish(evt: BridgeEvent.Fact(key: $"hostplugin.{id:D}", value: outcome));
        }
    }

    private static HostFingerprint RunningFingerprint() => new(
        BundleVersion: RhinoApp.Version.ToString(),
        RhinoCommonVersion: typeof(RhinoApp).Assembly.GetName().Version?.ToString() ?? string.Empty,
        Grasshopper2Version: LoadedAssemblyVersion(simpleName: "Grasshopper2"),
        RuntimeVersion: Environment.Version.ToString());

    private static Gh2ScrubOutcome CleanGrasshopper2() {
        // BOUNDARY ADAPTER: the reflective GH2 Document.AllDocuments + Unmodify scrub projects loader
        // drift and reflection faults onto the typed Failed case, never a swallow folded into success.
        try {
            Type? documentType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(selector: static assembly => HostReflection.LoadableTypes(assembly: assembly))
                .FirstOrDefault(predicate: static type => string.Equals(a: type.FullName, b: "Grasshopper2.Doc.Document", comparisonType: StringComparison.Ordinal));
            if (documentType is null) {
                return new Gh2ScrubOutcome.NotLoaded();
            }
            object? all = documentType.GetProperty(name: "AllDocuments", bindingAttr: BindingFlags.Public | BindingFlags.Static)?.GetValue(obj: null);
            object[] documents = [.. Enumerate(value: all)];
            int unmodified = 0;
            foreach (object document in documents) {
                unmodified += Invoke(document, methodName: "Unmodify") ? 1 : 0;
            }
            return new Gh2ScrubOutcome.Scrubbed(Documents: documents.Length, Unmodified: unmodified);
        } catch (Exception error) when (error is not OutOfMemoryException and not StackOverflowException and not AccessViolationException) {
            return new Gh2ScrubOutcome.Failed(Detail: $"{error.GetType().Name}: {error.Message}");
        }
    }

    private static IEnumerable<object> Enumerate(object? value) {
        if (value is System.Collections.IEnumerable items) {
            foreach (object? item in items) {
                if (item is not null) {
                    yield return item;
                }
            }
        }
    }

    private static bool Invoke(object target, string methodName) {
        MethodInfo? method = target.GetType().GetMethod(
            name: methodName, bindingAttr: BindingFlags.Public | BindingFlags.Instance,
            binder: null, types: Type.EmptyTypes, modifiers: null);
        if (method is null) {
            return false;
        }
        _ = method.Invoke(obj: target, parameters: null);
        return true;
    }

    private static string RpcAssemblyVersion =>
        typeof(JsonRpc).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? typeof(JsonRpc).Assembly.GetName().Version?.ToString()
        ?? "unknown";

    private static int ClientPidOf(CapabilityEntry[] capabilities) =>
        capabilities.FirstOrDefault(predicate: static entry => string.Equals(a: entry.Key, b: "client.pid", comparisonType: StringComparison.Ordinal))
            is { Receipt: { Length: > 0 } receipt } && int.TryParse(s: receipt, provider: CultureInfo.InvariantCulture, result: out int pid) ? pid : 0;

    // --- [TRANSPORT]

    private async Task AcceptLoopAsync(CancellationToken token) {
        // BOUNDARY ADAPTER: pipe cancellation and transient IO do not kill the accept loop.
        while (!token.IsCancellationRequested) {
            try {
                NamedPipeServerStream pipe = new(
                    pipeName: endpoint.PipeName, direction: PipeDirection.InOut, maxNumberOfServerInstances: PipeInstances,
                    transmissionMode: PipeTransmissionMode.Byte, options: PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
                await using (pipe.ConfigureAwait(continueOnCapturedContext: false)) {
                    await pipe.WaitForConnectionAsync(cancellationToken: token).ConfigureAwait(continueOnCapturedContext: false);
                    await ServeAsync(pipe: pipe, token: token).ConfigureAwait(continueOnCapturedContext: false);
                }
            } catch (OperationCanceledException) when (token.IsCancellationRequested) {
            } catch (Exception error) when (!token.IsCancellationRequested && error is IOException or InvalidOperationException
                or UnauthorizedAccessException or ObjectDisposedException or ConnectionLostException) {
                Debug.WriteLine(message: $"accept loop recovered: {error.Message}");
                await Task.Delay(millisecondsDelay: 100, cancellationToken: CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);
            }
        }
    }

    private async Task ServeAsync(NamedPipeServerStream pipe, CancellationToken token) {
        using SystemTextJsonFormatter formatter = new();
        // Formatter options derive from Contract; the default resolver tail preserves protocol
        // error payloads that the generated context does not own.
        formatter.JsonSerializerOptions = new JsonSerializerOptions(BridgeJsonContext.Default.Options) {
            TypeInfoResolver = System.Text.Json.Serialization.Metadata.JsonTypeInfoResolver.Combine(
                BridgeJsonContext.Default, new System.Text.Json.Serialization.Metadata.DefaultJsonTypeInfoResolver()),
        };
        using HeaderDelimitedMessageHandler handler = new(duplexStream: pipe, formatter: formatter);
        using JsonRpc rpc = new(messageHandler: handler);
        Connection connection = new(host: this, rpc: rpc, events: rpc.Attach<IBridgeEvents>());
        rpc.AddLocalRpcTarget<IBridgeShell>(target: connection, options: null);
        rpc.StartListening();
        try {
            await rpc.Completion.WaitAsync(cancellationToken: token).ConfigureAwait(continueOnCapturedContext: false);
        } catch (Exception error) when (error is ConnectionLostException or IOException or ObjectDisposedException or OperationCanceledException) {
            Debug.WriteLine(message: $"connection closed: {error.Message}");
        } finally {
            ReleaseOwner(connection: connection);
        }
    }

    // --- [ENDPOINT]

    private void EnsureEndpoint() {
        // BOUNDARY ADAPTER: hello heals missing, foreign, or poisoned endpoint records.
        try {
            EndpointRecord? onDisk = File.Exists(path: EndpointRecord.EndpointPath)
                ? JsonSerializer.Deserialize(json: File.ReadAllText(path: EndpointRecord.EndpointPath), jsonTypeInfo: BridgeJsonContext.Default.EndpointRecord)
                : null;
            if (onDisk is null || onDisk != endpoint) {
                WriteEndpoint(record: endpoint);
            }
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException) {
            WriteEndpoint(record: endpoint);
        }
    }

    private static void WriteEndpoint(EndpointRecord record) {
        _ = Directory.CreateDirectory(path: EndpointRecord.EndpointDirectory);
        using FileStream stream = new(path: EndpointRecord.EndpointPath, mode: FileMode.Create, access: FileAccess.Write, share: FileShare.Read);
        JsonSerializer.Serialize(utf8Json: stream, value: record, jsonTypeInfo: BridgeJsonContext.Default.EndpointRecord);
    }

    private static void WritePoisoned(int rhinoPid, string fault) {
        // BOUNDARY ADAPTER: poisoned endpoint records use the live codec with an empty pipe.
        try {
            WriteEndpoint(record: EndpointRecord.Create(
                pipeName: string.Empty,
                rhinoPid: rhinoPid,
                rhinoStartedAtUnixMs: HostStartedAtUnixMs(),
                contractVersion: Handshake.CurrentVersion,
                shellVersion: ShellVersion,
                rhinoVersion: RhinoApp.Version.ToString(),
                fault: fault));
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
            RhinoApp.WriteLine(message: $"[rasm-bridge] poisoned endpoint write failed: {error.Message}; fault was: {fault}");
        }
    }

    private static long HostStartedAtUnixMs() {
        using Process host = Process.GetCurrentProcess();
        return new DateTimeOffset(dateTime: host.StartTime.ToUniversalTime()).ToUnixTimeMilliseconds();
    }

    private static string LoadedAssemblyVersion(string simpleName) =>
        AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(predicate: assembly => string.Equals(a: assembly.GetName().Name, b: simpleName, comparisonType: StringComparison.Ordinal))
            ?.GetName().Version?.ToString() ?? string.Empty;
}
