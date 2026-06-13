using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO.Pipes;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using System.Threading.Channels;
using Rasm.Bridge.Contract;
using Rhino;
using Rhino.PlugIns;
using Rhino.Runtime;
using StreamJsonRpc;

namespace Rasm.Bridge.Shell;

// --- [SERVICES] -----------------------------------------------------------------------------

// Ownership: composition root + RPC target seam. Pipe server lifecycle, StreamJsonRpc attach,
// endpoint file write/heal, host event subscriptions (symmetric detachers), readiness facts.
// Method bodies are one-hop boundary adapters — admit Contract payload, dispatch to
// CargoGate/IdlePump, project the Contract outcome; no rail types in the shell ALC.
//
// Busy admission (one generation token, no semaphore queueing): Hello/Ping answer normally while a
// session is live; LoadCargo/Run/Unload/PrepareQuit from a second connection fail fast with
// BridgeFault.BusyHeld carried as LocalRpcException.ErrorData (serialized as BridgeFault so the
// `$type` discriminator survives; the supervisor folds on the data, not the code). The per-call
// connection identity the admission needs is what forces the nested per-connection target.
//
// Endpoint poisoning: a failed Start writes the endpoint file with a `fault` member naming the
// load exception instead of deleting it — doctor discriminates poisoned-start in one read; the
// record is evidence and is never deleted by the shell.
public sealed class ShellHost : IDisposable {
    private const string EndpointFileName = "rhino-bridge-rbx.json";
    private const int FaultErrorCode = -32050;
    private const int PipeInstances = 4;

    private readonly Lock sync = new();
    private readonly ConcurrentQueue<string> defaultResolves = new();
    private readonly Channel<BridgeEvent> outbox = Channel.CreateUnbounded<BridgeEvent>(options: new UnboundedChannelOptions { SingleReader = true });
    private readonly CancellationTokenSource lifetime = new();
    private readonly Func<AssemblyLoadContext, AssemblyName, Assembly?> resolvingTap;
    private readonly HostUtils.ExceptionReportDelegate reportTap;
    private readonly EventHandler closingTap;
    private readonly EndpointRecord endpoint;
    private readonly HostFingerprint fingerprint;
    private readonly IdlePump pump;
    private readonly CargoGate gate;
    private readonly Task[] acceptLoops;
    private readonly Task forwarder;
    private CargoManifest? activeManifest;
    private Connection? owner;
    private long ownedAtUnixMs;
    private long sequence;
    private bool disposed;

    private ShellHost(int rhinoPid) {
        endpoint = EndpointRecord.Create(
            pipeName: string.Create(provider: CultureInfo.InvariantCulture, $"{EndpointRecord.PipePrefix}{rhinoPid}-{Guid.NewGuid().ToString(format: "N")[..8]}"),
            rhinoPid: rhinoPid,
            rhinoStartedAtUnixMs: HostStartedAtUnixMs(),
            contractVersion: Handshake.CurrentVersion,
            shellVersion: ShellVersion,
            rhinoVersion: RhinoApp.Version.ToString());
        fingerprint = new HostFingerprint(
            BundleVersion: RhinoApp.Version.ToString(),
            RhinoCommonVersion: typeof(RhinoApp).Assembly.GetName().Version?.ToString() ?? string.Empty,
            Grasshopper2Version: LoadedAssemblyVersion(simpleName: "Grasshopper2"),
            RuntimeVersion: Environment.Version.ToString());
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
        // BOUNDARY ADAPTER — pipe creation, endpoint IO, and tap subscription can all throw during
        // host idle; the failure becomes one poisoned endpoint record, never a host fault.
        _ = deployDir;
        try {
            return new ShellHost(rhinoPid: rhinoPid);
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
            // The endpoint file stays in place: staleness is discriminated by pid/start-time
            // liveness, and a surviving record is doctor evidence, never garbage.
        }
    }

    // --- [CONNECTION_TARGET] ------------------------------------------------------------------

    // Ownership: the per-connection RPC target — one instance per accepted pipe, carrying the
    // connection identity the busy admission discriminates on. Pure forwarding, no state beyond
    // the hello-declared client pid.
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

        public Task PrepareQuitAsync(CancellationToken ct) =>
            host.PrepareQuitAsync(connection: this, ct: ct);
    }

    // --- [VERBS] --------------------------------------------------------------------------------

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
            return gate.Swap(manifest: manifest, publish: Publish);
        }, ct: ct).ConfigureAwait(continueOnCapturedContext: false);
    }

    private async Task<ScenarioReceipt[]> RunAsync(Connection connection, ScenarioSelection selection, CancellationToken ct) {
        ArgumentNullException.ThrowIfNull(argument: selection);
        Admit(connection: connection);
        IBridgeCargo cargo = gate.Current ?? throw new LocalRpcException(message: "no cargo loaded: LoadCargoAsync precedes RunAsync") { ErrorCode = FaultErrorCode };
        ScenarioEntry[] discovered = await pump.OnUiThreadAsync(job: cargo.Discover, ct: ct).ConfigureAwait(continueOnCapturedContext: false);
        ScenarioEntry[] selected = Select(entries: discovered, selection: selection);
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

    private async Task PrepareQuitAsync(Connection connection, CancellationToken ct) {
        Admit(connection: connection);
        (bool hadModified, string gh2) = await pump.OnUiThreadAsync(job: static () => {
            RhinoDoc[] open = RhinoDoc.OpenDocuments();
            bool modified = open.Any(predicate: static doc => doc.Modified);
            Array.ForEach(array: open, action: static doc => doc.Modified = false);
            return (modified, CleanGrasshopper2());
        }, ct: ct).ConfigureAwait(continueOnCapturedContext: false);
        Publish(evt: Fact(key: "quit.prepared", value: $"{(hadModified ? "rhino-docs-marked-clean" : "rhino-docs-already-clean")}; gh2={gh2}"));
    }

    // --- [ADMISSION] ----------------------------------------------------------------------------

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

    // --- [EVIDENCE] -----------------------------------------------------------------------------

    // Single in-host writer: the shell stamps SessionId + Sequence for EVERY in-host event at
    // publish (cargo publishes through this delegate), preserving the author's Scenario slot.
    private void Publish(BridgeEvent evt) {
        EventStamp stamp = new(
            SessionId: activeManifest?.SessionId ?? Guid.Empty,
            Sequence: Interlocked.Increment(location: ref sequence),
            AtUnixMs: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Scenario: evt.Stamp.Scenario);
        _ = outbox.Writer.TryWrite(item: evt.Switch(
            state: stamp,
            factCase: static (s, f) => (BridgeEvent)(f with { Stamp = s }),
            captureCase: static (s, c) => c with { Stamp = s },
            phaseCase: static (s, p) => p with { Stamp = s },
            progressCase: static (s, p) => p with { Stamp = s },
            hostExceptionCase: static (s, h) => h with { Stamp = s }));
    }

    private async Task ForwardLoopAsync(CancellationToken token) {
        // BOUNDARY ADAPTER — one ordered forwarder; a dead peer drops events instead of faulting
        // the publisher (the spool, not the wire, is the crash-durable evidence rail).
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
        // Proof 13 evidence: every name that falls through to default-ALC Resolving during a live
        // session — the deploy-set trim decision reads this row from hello.
        defaultResolves.Enqueue(item: assemblyName.Name ?? "unknown");
        return null;
    }

    private string DefaultResolveReceipt() {
        string[] names = [.. defaultResolves.Distinct(comparer: StringComparer.Ordinal).Order(comparer: StringComparer.Ordinal)];
        return string.Create(provider: CultureInfo.InvariantCulture, $"{names.Length}:{string.Join(separator: ',', value: names)}");
    }

    private static CapabilityEntry McpListener() =>
        new(Key: "mcp.listener", Outcome: PhaseStatus.Unsupported,
            Receipt: Environment.GetEnvironmentVariable(variable: "RHINO_MCP_AUTOSTART_PORT") == "0"
                ? "autostart suppressed by RHINO_MCP_AUTOSTART_PORT=0; bridge did not start a listener"
                : "bridge did not start a listener");

    private static CapabilityEntry McpPlatform() {
        string[] loaded = [.. AppDomain.CurrentDomain.GetAssemblies()
            .Select(selector: static assembly => assembly.GetName())
            .Where(predicate: static name => (name.Name ?? string.Empty).Contains(value: "mcp", comparisonType: StringComparison.OrdinalIgnoreCase))
            .Select(selector: static name => $"{name.Name}:{name.Version}")
            .Order(comparer: StringComparer.Ordinal)];
        return loaded.Length == 0
            ? new CapabilityEntry(Key: "mcp.platform.version", Outcome: PhaseStatus.Unsupported, Receipt: "no MCP platform assembly loaded")
            : new CapabilityEntry(Key: "mcp.platform.version", Outcome: PhaseStatus.Ok, Receipt: string.Join(separator: ',', value: loaded));
    }

    private void PreloadHostPlugins(Guid[] plugins) {
        // Proof 10 evidence: the absent/repackaged-plugin failure mode (return-false vs throw) is
        // recorded per GUID; both outcomes map to the same capability-absent reading downstream.
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
            Publish(evt: Fact(key: $"hostplugin.{id:D}", value: outcome));
        }
    }

    private static BridgeEvent.FactCase Fact(string key, string value) =>
        new(Key: key, Value: JsonSerializer.SerializeToElement(value: value, jsonTypeInfo: BridgeJsonContext.Default.String)) { Stamp = default };

    private static ScenarioEntry[] Select(ScenarioEntry[] entries, ScenarioSelection selection) =>
        selection.Switch(
            state: entries,
            allCase: static (all, _) => all,
            themesCase: static (all, themes) => [.. all.Where(entry => themes.Themes.Contains(value: entry.Theme, comparer: StringComparer.Ordinal))],
            namesCase: static (all, names) => [.. all.Where(entry => names.Names.Contains(value: entry.Name, comparer: StringComparer.Ordinal))]);

    private static string CleanGrasshopper2() {
        try {
            Type? editorType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(selector: static assembly => SafeTypes(assembly: assembly))
                .FirstOrDefault(predicate: static type => string.Equals(a: type.FullName, b: "Grasshopper2.UI.Editor", comparisonType: StringComparison.Ordinal));
            object? editor = editorType?.GetProperty(name: "Instance", bindingAttr: BindingFlags.Public | BindingFlags.Static)?.GetValue(obj: null);
            object? bag = editorType?.GetProperty(name: "Documents", bindingAttr: BindingFlags.Public | BindingFlags.Instance)?.GetValue(obj: editor);
            object? unsaved = bag?.GetType().GetProperty(name: "Unsaved", bindingAttr: BindingFlags.Public | BindingFlags.Instance)?.GetValue(obj: bag);
            object[] documents = [.. Enumerate(value: unsaved)];
            int unmodified = 0;
            int closed = 0;
            foreach (object document in documents) {
                unmodified += Invoke(document, methodName: "Unmodify") ? 1 : 0;
                closed += PopOrClose(bag: bag, document: document) ? 1 : 0;
            }
            return editorType is null ? "not-loaded" : $"unsaved={documents.Length};unmodified={unmodified};closed={closed}";
        } catch (Exception error) when (error is not OutOfMemoryException and not StackOverflowException and not AccessViolationException) {
            return $"cleanup-failed:{error.GetType().Name}:{error.Message}";
        }
    }

    private static IEnumerable<Type> SafeTypes(Assembly assembly) {
        try {
            return assembly.GetTypes();
        } catch (ReflectionTypeLoadException partial) {
            return partial.Types.Where(predicate: static type => type is not null)!;
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

    private static bool PopOrClose(object? bag, object document) {
        MethodInfo? pop = bag?.GetType().GetMethods(bindingAttr: BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(predicate: method => method.Name == "Pop"
                && method.GetParameters() is { Length: 2 } parameters
                && parameters[0].ParameterType.IsInstanceOfType(o: document)
                && parameters[1].ParameterType == typeof(bool));
        if (pop is not null) {
            _ = pop.Invoke(obj: bag, parameters: [document, true]);
            return true;
        }
        return Invoke(target: document, methodName: "Close");
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

    // --- [TRANSPORT] ----------------------------------------------------------------------------

    private async Task AcceptLoopAsync(CancellationToken token) {
        // BOUNDARY ADAPTER — NamedPipeServerStream IO surface; cancellation and transient IO must
        // not kill the loop.
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
        // The formatter options DERIVE from the Contract context options (camelCase naming +
        // unmapped-member skip travel WITH the options: metadata-mode resolution applies the
        // runtime options' naming policy, not the baked context names). The default-resolver tail
        // is mandatory: a context-only chain drops the implicit reflection fallback, making
        // protocol-owned CommonErrorData on any error response undecodable — the peer then fatals
        // the connection on the first faulted call.
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

    // --- [ENDPOINT] -----------------------------------------------------------------------------

    private static string EndpointDirectory =>
        Path.Combine(path1: Environment.GetFolderPath(folder: Environment.SpecialFolder.UserProfile), path2: ".rasm");

    private static string EndpointPath => Path.Combine(path1: EndpointDirectory, path2: EndpointFileName);

    private void EnsureEndpoint() {
        // BOUNDARY ADAPTER — the endpoint file heals on every hello: a missing, foreign, or
        // poisoned record is rewritten from the live truth.
        try {
            EndpointRecord? onDisk = File.Exists(path: EndpointPath)
                ? JsonSerializer.Deserialize(json: File.ReadAllText(path: EndpointPath), jsonTypeInfo: BridgeJsonContext.Default.EndpointRecord)
                : null;
            if (onDisk is null || onDisk != endpoint) {
                WriteEndpoint(record: endpoint);
            }
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException) {
            WriteEndpoint(record: endpoint);
        }
    }

    private static void WriteEndpoint(EndpointRecord record) {
        _ = Directory.CreateDirectory(path: EndpointDirectory);
        using FileStream stream = new(path: EndpointPath, mode: FileMode.Create, access: FileAccess.Write, share: FileShare.Read);
        JsonSerializer.Serialize(utf8Json: stream, value: record, jsonTypeInfo: BridgeJsonContext.Default.EndpointRecord);
    }

    private static void WritePoisoned(int rhinoPid, string fault) {
        // BOUNDARY ADAPTER — the poisoned record cannot be an EndpointRecord (no live pipe to
        // admit), so it is written field-for-field with the extra `fault` member; last-resort
        // console line only when the evidence write itself fails.
        try {
            _ = Directory.CreateDirectory(path: EndpointDirectory);
            using FileStream stream = new(path: EndpointPath, mode: FileMode.Create, access: FileAccess.Write, share: FileShare.Read);
            using Utf8JsonWriter writer = new(utf8Json: stream);
            writer.WriteStartObject();
            writer.WriteString(propertyName: "pipeName", value: string.Empty);
            writer.WriteNumber(propertyName: "rhinoPid", value: rhinoPid);
            writer.WriteNumber(propertyName: "rhinoStartedAtUnixMs", value: HostStartedAtUnixMs());
            writer.WriteNumber(propertyName: "contractVersion", value: Handshake.CurrentVersion);
            writer.WriteString(propertyName: "shellVersion", value: ShellVersion);
            writer.WriteString(propertyName: "rhinoVersion", value: RhinoApp.Version.ToString());
            writer.WriteString(propertyName: "fault", value: fault);
            writer.WriteEndObject();
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
