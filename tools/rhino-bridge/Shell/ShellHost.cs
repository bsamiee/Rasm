using System.Diagnostics;
using System.Globalization;
using System.IO.Pipes;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Channels;
using Rasm.Bridge.Contract;
using Rhino;
using Rhino.PlugIns;
using Rhino.Runtime;
using StreamJsonRpc;
using GhDocument = Grasshopper2.Doc.Document;

namespace Rasm.Bridge.Shell;

// --- [SERVICES] ------------------------------------------------------------------------

public sealed class ShellHost : IDisposable {
    private const int PipeInstances = 4;
    private static readonly Guid Grasshopper2PluginId = Guid.Parse("8307876d-a461-4daa-bb77-eb3715925513");

    private readonly Lock sync = new();
    private readonly Channel<BridgeEvent> outbox = Channel.CreateUnbounded<BridgeEvent>(new UnboundedChannelOptions { SingleReader = true });
    private readonly CancellationTokenSource lifetime = new();
    private readonly HostUtils.ExceptionReportDelegate reportTap;
    private readonly EndpointRecord.Live endpoint;
    private readonly IdlePump pump = new();
    private readonly CargoGate gate = new();
    private CargoManifest? activeManifest;
    private Connection? owner;
    private long ownedAtUnixMs;
    private long sequence;
    private bool disposed;

    private ShellHost(int rhinoPid) {
        endpoint = new EndpointRecord.Live(
            PipeName: string.Create(CultureInfo.InvariantCulture, $"{EndpointRecord.PipePrefix}{rhinoPid}-{Guid.NewGuid().ToString("N")[..8]}"),
            RhinoPid: rhinoPid,
            RhinoStartedAtUnixMs: HostStartedAtUnixMs(),
            RhinoVersion: RhinoApp.Version.ToString());
        reportTap = (source, error) => Publish(BridgeEvent.Fact("host.exception", $"{source}: {error.GetType().Name}: {error.Message}"));
        HostUtils.OnExceptionReport += reportTap;
        WriteEndpoint(endpoint);
        _ = ForwardLoopAsync(lifetime.Token);
        _ = Enumerable.Range(0, PipeInstances).Select(_ => AcceptLoopAsync(lifetime.Token)).ToArray();
    }

    public static ShellHost? Start(int rhinoPid) {
        try {
            return new ShellHost(rhinoPid);
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException or InvalidOperationException or ArgumentException or NotSupportedException) {
            WritePoisoned(rhinoPid, error.GetBaseException().Message);
            return null;
        }
    }

    public void Dispose() {
        lock (sync) {
            if (disposed) {
                return;
            }
            disposed = true;
        }
        HostUtils.OnExceptionReport -= reportTap;
        lifetime.Cancel();
        _ = outbox.Writer.TryComplete();
        pump.Dispose();
        gate.Dispose();
        lifetime.Dispose();
    }

    // --- [CONNECTION_TARGET]

    private sealed class Connection(ShellHost host, JsonRpc rpc, IBridgeEvents events) : IBridgeShell {
        internal IBridgeEvents Events { get; } = events;
        internal int ClientPid { get; set; }
        internal bool IsAlive => !rpc.Completion.IsCompleted;

        public Task<HostFingerprint> HelloAsync(int supervisorPid, CancellationToken ct) =>
            Task.FromResult(host.Hello(this, supervisorPid));

        public Task<LoadedCargo> LoadCargoAsync(CargoManifest manifest, CancellationToken ct) =>
            host.LoadCargoAsync(this, manifest, ct);

        public Task<ScenarioOutcome[]> RunAsync(ScenarioSelection selection, CancellationToken ct) =>
            host.RunAsync(this, selection, ct);

        public Task<UnloadOutcome> UnloadCargoAsync(CancellationToken ct) =>
            host.UnloadCargoAsync(this, ct);

        public Task<QuitScrub> PrepareQuitAsync(CancellationToken ct) =>
            host.PrepareQuitAsync(this, ct);
    }

    // --- [VERBS]

    private HostFingerprint Hello(Connection connection, int supervisorPid) {
        connection.ClientPid = supervisorPid;
        EnsureEndpoint();
        return RunningFingerprint();
    }

    private async Task<LoadedCargo> LoadCargoAsync(Connection connection, CargoManifest manifest, CancellationToken ct) {
        ArgumentNullException.ThrowIfNull(manifest);
        Admit(connection);
        activeManifest = manifest;
        LoadedCargo loaded = await pump.OnUiThreadAsync(() => {
            PreloadGrasshopper2();
            return gate.Load(manifest, RunningFingerprint(), Publish);
        }, ct).ConfigureAwait(false);
        Publish(BridgeEvent.Fact("scenario.discovered", string.Join(',', loaded.Scenarios.Select(static scenario => scenario.Name))));
        return loaded;
    }

    private async Task<ScenarioOutcome[]> RunAsync(Connection connection, ScenarioSelection selection, CancellationToken ct) {
        ArgumentNullException.ThrowIfNull(selection);
        Admit(connection);
        IBridgeCargo cargo = gate.Current ?? throw CargoGate.Refuse(new BridgeFault.CapabilityAbsent("cargo", "no cargo loaded: LoadCargoAsync precedes RunAsync"));
        ScenarioEntry[] discovered = await pump.OnUiThreadAsync(cargo.Discover, ct).ConfigureAwait(false);
        ScenarioEntry[] selected = selection.Filter(discovered);
        Publish(BridgeEvent.Fact("scenario.selected", string.Join(',', selected.Select(static scenario => scenario.Name))));
        if (selected.Length == 0) {
            throw CargoGate.Refuse(new BridgeFault.CapabilityAbsent(
                "scenario.selection",
                $"selection matched zero scenarios; discovered={string.Join(',', discovered.Select(static scenario => scenario.Name))}"));
        }
        ScenarioOutcome[] outcomes = new ScenarioOutcome[selected.Length];
        for (int index = 0; index < selected.Length; index++) {
            ScenarioEntry entry = selected[index];
            outcomes[index] = await pump.OnUiThreadAsync(() => cargo.Run(entry, Publish), ct).ConfigureAwait(false);
        }
        return outcomes;
    }

    private async Task<UnloadOutcome> UnloadCargoAsync(Connection connection, CancellationToken ct) {
        Admit(connection);
        UnloadOutcome unloaded = await pump.OnUiThreadAsync(gate.Unload, ct).ConfigureAwait(false);
        ReleaseOwner(connection);
        return unloaded;
    }

    private async Task<QuitScrub> PrepareQuitAsync(Connection connection, CancellationToken ct) {
        Admit(connection);
        QuitScrub quit = await pump.OnUiThreadAsync(static () => {
            RhinoDoc[] open = RhinoDoc.OpenDocuments();
            int markedClean = open.Count(static doc => doc.Modified);
            Array.ForEach(open, static doc => doc.Modified = false);
            Gh2Scrub gh2 = Grasshopper2Loaded ? ScrubGrasshopper2() : new Gh2Scrub.NotLoaded();
            RhinoDoc[] dirty = [.. RhinoDoc.OpenDocuments().Where(static doc => doc.Modified)];
            return new QuitScrub(open.Length, markedClean, dirty.Length, gh2,
                [.. dirty.Select(static doc => doc.Path).Where(static path => path is { Length: > 0 })]);
        }, ct).ConfigureAwait(false);
        Publish(BridgeEvent.Fact("quit.prepared", JsonSerializer.SerializeToElement(quit, BridgeJsonContext.Default.QuitScrub)));
        return quit;
    }

    // --- [ADMISSION]

    private void Admit(Connection connection) {
        lock (sync) {
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (owner is { } held && !ReferenceEquals(held, connection) && held.IsAlive) {
                throw CargoGate.Refuse(new BridgeFault.BusyHeld(held.ClientPid, (now - ownedAtUnixMs) / 1000.0));
            }
            if (!ReferenceEquals(owner, connection)) {
                owner = connection;
                ownedAtUnixMs = now;
            }
        }
    }

    private void ReleaseOwner(Connection connection) {
        lock (sync) {
            if (ReferenceEquals(owner, connection)) {
                owner = null;
            }
        }
    }

    // --- [EVIDENCE]

    private void Publish(BridgeEvent evt) =>
        _ = outbox.Writer.TryWrite(evt.Stamped(new EventStamp(
            activeManifest?.SessionId ?? Guid.Empty,
            Interlocked.Increment(ref sequence),
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            evt.Stamp.Scenario)));

    private async Task ForwardLoopAsync(CancellationToken token) {
        try {
            await foreach (BridgeEvent evt in outbox.Reader.ReadAllAsync(token).ConfigureAwait(false)) {
                if (CurrentOwner() is { Events: { } events }) {
                    try {
                        await events.PublishAsync(evt).ConfigureAwait(false);
                    } catch (Exception error) when (error is IOException or ObjectDisposedException or ConnectionLostException or RemoteInvocationException) {
                        Debug.WriteLine($"event forward dropped: {error.Message}");
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

    // --- [HOST]

    private static bool Grasshopper2Loaded => PlugIn.GetPlugInInfo(Grasshopper2PluginId)?.IsLoaded == true;

    private void PreloadGrasshopper2() {
        string outcome;
        try {
            outcome = Grasshopper2Loaded ? "already-loaded"
                : PlugIn.LoadPlugIn(Grasshopper2PluginId, loadQuietly: true, forceLoad: false) && Grasshopper2Loaded ? "loaded"
                : "load-refused";
        } catch (Exception error) when (error is not OutOfMemoryException and not StackOverflowException and not AccessViolationException) {
            outcome = $"threw {error.GetType().Name}: {error.Message}";
        }
        Publish(BridgeEvent.Fact("hostplugin.grasshopper2", outcome));
    }

    private static HostFingerprint RunningFingerprint() => new(
        RhinoApp.Version.ToString(),
        typeof(RhinoApp).Assembly.GetName().Version?.ToString() ?? string.Empty,
        Grasshopper2Loaded ? Grasshopper2Version() : string.Empty,
        Environment.Version.ToString());

    private static string Grasshopper2Version() => typeof(GhDocument).Assembly.GetName().Version?.ToString() ?? string.Empty;

    private static Gh2Scrub ScrubGrasshopper2() {
        try {
            GhDocument[] documents = [.. GhDocument.AllDocuments];
            int modifiedBefore = documents.Count(static document => document.Modified);
            foreach (GhDocument document in documents.Where(static document => document.Modified)) {
                document.Unmodify();
            }
            return new Gh2Scrub.Scrubbed(documents.Length, modifiedBefore, documents.Count(static document => document.Modified));
        } catch (Exception error) when (error is not OutOfMemoryException and not StackOverflowException and not AccessViolationException) {
            return new Gh2Scrub.Failed($"{error.GetType().Name}: {error.Message}");
        }
    }

    private static long HostStartedAtUnixMs() {
        using Process host = Process.GetCurrentProcess();
        return new DateTimeOffset(host.StartTime.ToUniversalTime()).ToUnixTimeMilliseconds();
    }

    // --- [TRANSPORT]

    private async Task AcceptLoopAsync(CancellationToken token) {
        while (!token.IsCancellationRequested) {
            try {
                NamedPipeServerStream pipe = new(
                    endpoint.PipeName, PipeDirection.InOut, PipeInstances,
                    PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
                await using (pipe.ConfigureAwait(false)) {
                    await pipe.WaitForConnectionAsync(token).ConfigureAwait(false);
                    await ServeAsync(pipe, token).ConfigureAwait(false);
                }
            } catch (OperationCanceledException) when (token.IsCancellationRequested) {
            } catch (Exception error) when (!token.IsCancellationRequested && error is IOException or InvalidOperationException
                or UnauthorizedAccessException or ObjectDisposedException or ConnectionLostException) {
                Debug.WriteLine($"accept loop recovered: {error.Message}");
                await Task.Delay(100, CancellationToken.None).ConfigureAwait(false);
            }
        }
    }

    private async Task ServeAsync(NamedPipeServerStream pipe, CancellationToken token) {
        using SystemTextJsonFormatter formatter = new();
        formatter.JsonSerializerOptions = new JsonSerializerOptions(BridgeJsonContext.Default.Options) {
            TypeInfoResolver = JsonTypeInfoResolver.Combine(BridgeJsonContext.Default, new DefaultJsonTypeInfoResolver()),
        };
        using HeaderDelimitedMessageHandler handler = new(pipe, formatter);
        using JsonRpc rpc = new(handler);
        Connection connection = new(this, rpc, rpc.Attach<IBridgeEvents>());
        rpc.AddLocalRpcTarget<IBridgeShell>(connection, options: null);
        rpc.StartListening();
        try {
            await rpc.Completion.WaitAsync(token).ConfigureAwait(false);
        } catch (Exception error) when (error is ConnectionLostException or IOException or ObjectDisposedException or OperationCanceledException) {
            Debug.WriteLine($"connection closed: {error.Message}");
        } finally {
            ReleaseOwner(connection);
        }
    }

    // --- [ENDPOINT]

    private void EnsureEndpoint() {
        try {
            EndpointRecord? onDisk = File.Exists(EndpointRecord.FilePath)
                ? JsonSerializer.Deserialize(File.ReadAllText(EndpointRecord.FilePath), BridgeJsonContext.Default.EndpointRecord)
                : null;
            if (onDisk != endpoint) {
                WriteEndpoint(endpoint);
            }
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException) {
            WriteEndpoint(endpoint);
        }
    }

    private static void WriteEndpoint(EndpointRecord record) {
        _ = Directory.CreateDirectory(RasmHome.Directory);
        using FileStream stream = new(EndpointRecord.FilePath, FileMode.Create, FileAccess.Write, FileShare.Read);
        JsonSerializer.Serialize(stream, record, BridgeJsonContext.Default.EndpointRecord);
    }

    private static void WritePoisoned(int rhinoPid, string fault) {
        try {
            WriteEndpoint(new EndpointRecord.Poisoned(rhinoPid, HostStartedAtUnixMs(), RhinoApp.Version.ToString(), fault));
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
            RhinoApp.WriteLine($"[rasm-bridge] poisoned endpoint write failed: {error.Message}; fault was: {fault}");
        }
    }
}
