using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Rasm.Bridge.Contract;
using StreamJsonRpc;
using StreamJsonRpc.Protocol;

namespace Rasm.Bridge.Supervisor;

// --- [MODELS] -----------------------------------------------------------------------------

// Ownership: host-bundle identity. Discovery and marker names derive from bundle metadata, while
// RHINO_WIP_APP_PATH only narrows candidates and launch suppresses MCP autostart.
internal sealed record BundleInfo(string AppPath, string CFBundleName, string CFBundleExecutable,
                                string CFBundleVersion) {
    public string AutosaveMarker => $"Unsaved {CFBundleName} Document.3dm";
    public string CrashReportPattern => $"{CFBundleExecutable}-*.ips";

    private Version Numeric => Version.TryParse(input: CFBundleVersion, result: out Version? parsed) ? parsed : new Version(major: 0, minor: 0);

    public static Fin<BundleInfo> Discover(TimeSpan toolDeadline) {
        string? narrowed = Environment.GetEnvironmentVariable(variable: "RHINO_WIP_APP_PATH");
        Seq<string> candidates = narrowed is { Length: > 0 } ? Seq(value: narrowed) : Candidates();
        Seq<BundleInfo> admitted = candidates.Choose(selector: path => Read(appPath: path, toolDeadline: toolDeadline));
        return toSeq(value: admitted.OrderByDescending(keySelector: static bundle => bundle.Numeric)).Head.Case is BundleInfo newest
            ? Fin.Succ(value: newest)
            : Fin.Fail<BundleInfo>(error: Error.New(message: "no Rhino*.app bundle discovered under /Applications; set RHINO_WIP_APP_PATH to narrow"));
    }

    public Fin<Unit> Launch(TimeSpan toolDeadline) =>
        Exec.Run(file: "open", args: ["-a", AppPath, "--env", "RHINO_MCP_AUTOSTART_PORT=0", "--args", "-nosplash"], deadline: toolDeadline)
            .Bind(f: result => result.ExitCode == 0
                ? Fin.Succ(value: unit)
                : Fin.Fail<Unit>(error: Error.New(message: string.Create(provider: CultureInfo.InvariantCulture, $"open '{AppPath}' exited {result.ExitCode}: {result.StdErr.Trim()}"))));

    private static Seq<string> Candidates() {
        // BOUNDARY ADAPTER: bundle enumeration absence yields an empty set.
        try {
            return toSeq(value: Directory.GetDirectories(path: "/Applications", searchPattern: "Rhino*.app"));
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
            return Seq<string>();
        }
    }

    private static Option<BundleInfo> Read(string appPath, TimeSpan toolDeadline) =>
        Exec.Run(file: "plutil", args: ["-convert", "json", "-o", "-", Path.Combine(path1: appPath, path2: "Contents", path3: "Info.plist")], deadline: toolDeadline)
                is Fin<ExecResult>.Succ(ExecResult plist) && plist.ExitCode == 0
            ? Decode(appPath: appPath, json: plist.StdOut)
            : Option<BundleInfo>.None;

    private static Option<BundleInfo> Decode(string appPath, string json) {
        // BOUNDARY ADAPTER: malformed Info.plist data disqualifies the candidate.
        try {
            using JsonDocument plist = JsonDocument.Parse(json: json);
            return Some(value: new BundleInfo(
                AppPath: appPath,
                CFBundleName: Member(root: plist.RootElement, name: "CFBundleName"),
                CFBundleExecutable: Member(root: plist.RootElement, name: "CFBundleExecutable"),
                CFBundleVersion: Member(root: plist.RootElement, name: "CFBundleVersion")));
        } catch (JsonException) {
            return Option<BundleInfo>.None;
        }
    }

    private static string Member(JsonElement root, string name) =>
        root.TryGetProperty(propertyName: name, value: out JsonElement member) ? member.GetString() ?? string.Empty : string.Empty;
}

// Ownership: live supervised host identity. Admit distinguishes dead pids from start-time drift
// and leaves endpoint files as evidence.
internal sealed record LiveHost(int Pid, long StartedAtUnixMs, EndpointRecord Endpoint,
                              HostFingerprint Fingerprint) {
    public static Fin<LiveHost> Admit(EndpointRecord endpoint, HostFingerprint fingerprint) {
        ArgumentNullException.ThrowIfNull(argument: endpoint);
        return Posix.StartedAtUnixMs(pid: endpoint.RhinoPid).Case is long started
            ? endpoint.IsLiveFor(pid: endpoint.RhinoPid, startedAtUnixMs: started)
                ? Fin.Succ(value: new LiveHost(Pid: endpoint.RhinoPid, StartedAtUnixMs: started, Endpoint: endpoint, Fingerprint: fingerprint))
                : Fin.Fail<LiveHost>(error: Error.New(message: string.Create(provider: CultureInfo.InvariantCulture,
                    $"endpoint stale: pid {endpoint.RhinoPid} start-time drift (recorded {endpoint.RhinoStartedAtUnixMs}, observed {started}) — pid recycled")))
            : Fin.Fail<LiveHost>(error: Error.New(message: string.Create(provider: CultureInfo.InvariantCulture,
                $"endpoint stale: pid {endpoint.RhinoPid} is not alive")));
    }
}

// Ownership: workstation pipe admission and JSON-RPC shell binding.
internal sealed class SupervisorConnection : IAsyncDisposable {
    private const string SupervisorVersion = "supervisor";

    private readonly NamedPipeClientStream stream;
    private readonly SystemTextJsonFormatter formatter;
    private readonly HeaderDelimitedMessageHandler handler;
    private readonly JsonRpc rpc;
    private readonly IBridgeShell shell;
    private readonly ConcurrentQueue<BridgeEvent> events;
    private bool disposed;

    private SupervisorConnection(NamedPipeClientStream stream, SystemTextJsonFormatter formatter, HeaderDelimitedMessageHandler handler, JsonRpc rpc, IBridgeShell shell, ConcurrentQueue<BridgeEvent> events) {
        this.stream = stream;
        this.formatter = formatter;
        this.handler = handler;
        this.rpc = rpc;
        this.shell = shell;
        this.events = events;
    }

    internal BridgeEvent[] Events => [.. events];

    internal static async Task<SupervisorConnection> ConnectAsync(string pipeName, TimeSpan timeout, CancellationToken ct) {
        NamedPipeClientStream admittedStream = new(
            serverName: ".", pipeName: pipeName, direction: PipeDirection.InOut,
            options: PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
        SystemTextJsonFormatter? admittedFormatter = null;
        HeaderDelimitedMessageHandler? admittedHandler = null;
        JsonRpc? admittedRpc = null;
        bool transferred = false;
        try {
            using CancellationTokenSource linked = CancellationTokenSource.CreateLinkedTokenSource(ct);
            linked.CancelAfter(delay: timeout);
            await admittedStream.ConnectAsync(cancellationToken: linked.Token).ConfigureAwait(false);
            admittedFormatter = new SystemTextJsonFormatter {
                JsonSerializerOptions = new JsonSerializerOptions(BridgeJsonContext.Default.Options) {
                    TypeInfoResolver = JsonTypeInfoResolver.Combine(BridgeJsonContext.Default, new DefaultJsonTypeInfoResolver()),
                },
            };
            admittedHandler = new HeaderDelimitedMessageHandler(duplexStream: admittedStream, formatter: admittedFormatter);
            admittedRpc = new BridgeRpc(messageHandler: admittedHandler);
            ConcurrentQueue<BridgeEvent> admittedEvents = new();
            admittedRpc.AddLocalRpcTarget<IBridgeEvents>(target: new EventSink(events: admittedEvents), options: null);
            IBridgeShell admittedShell = admittedRpc.Attach<IBridgeShell>();
            admittedRpc.StartListening();
            SupervisorConnection connection = new(
                stream: admittedStream,
                formatter: admittedFormatter,
                handler: admittedHandler,
                rpc: admittedRpc,
                shell: admittedShell,
                events: admittedEvents);
            transferred = true;
            return connection;
        } finally {
            if (!transferred) {
                admittedRpc?.Dispose();
                if (admittedHandler is not null) {
                    await admittedHandler.DisposeAsync().ConfigureAwait(false);
                }
                admittedFormatter?.Dispose();
                await admittedStream.DisposeAsync().ConfigureAwait(false);
            }
        }
    }

    internal Task<Handshake> HelloAsync(CancellationToken ct) =>
        shell.HelloAsync(supervisor: new Handshake(
            ContractVersion: Handshake.CurrentVersion,
            SenderVersion: SupervisorVersion,
            Capabilities: [new CapabilityEntry(
                Key: "client.pid", Outcome: PhaseStatus.Ok,
                Receipt: Environment.ProcessId.ToString(provider: CultureInfo.InvariantCulture))],
            Fingerprint: null,
            Endpoint: null), ct: ct);

    internal Task<CargoReceipt> LoadAsync(CargoManifest manifest, CancellationToken ct) =>
        shell.LoadCargoAsync(manifest: manifest, ct: ct);

    internal Task<ScenarioReceipt[]> RunAsync(ScenarioSelection selection, CancellationToken ct) =>
        shell.RunAsync(selection: selection, ct: ct);

    internal Task<UnloadReceipt> UnloadAsync(CancellationToken ct) =>
        shell.UnloadCargoAsync(ct: ct);

    internal Task PrepareQuitAsync(CancellationToken ct) =>
        shell.PrepareQuitAsync(ct: ct);

    public async ValueTask DisposeAsync() {
        if (disposed)
            return;
        disposed = true;
        rpc.Dispose();
        await handler.DisposeAsync().ConfigureAwait(false);
        formatter.Dispose();
        await stream.DisposeAsync().ConfigureAwait(false);
    }

    private sealed class EventSink(ConcurrentQueue<BridgeEvent> events) : IBridgeEvents {
        public Task PublishAsync(BridgeEvent evt) {
            events.Enqueue(item: evt);
            return Task.CompletedTask;
        }
    }

    private sealed class BridgeRpc(IJsonRpcMessageHandler messageHandler) : JsonRpc(messageHandler: messageHandler) {
        protected override Type? GetErrorDetailsDataType(JsonRpcError error) =>
            error.Error?.Code is { } code && (int)code == -32050 ? typeof(BridgeFault) : base.GetErrorDetailsDataType(error: error);
    }
}

// Ownership: lease payloads; holder pid plus start time gives leases the same staleness test as endpoints.
internal sealed record LeaseToken(int HolderPid, long AcquiredAtUnixMs, string Path);
internal sealed record LeaseClaim(int HolderPid, long HolderStartedAtUnixMs, long AcquiredAtUnixMs);

// Ownership: retired supervised host windows for reconcile; foreign markers are reported, not deleted.
internal sealed record QuitJournalEntry(int Pid, long StartedAtUnixMs, long RetiredAtUnixMs, string Rung, string PipeName);

internal readonly record struct ExecResult(int ExitCode, string StdOut, string StdErr);

// --- [SERVICES] ---------------------------------------------------------------------------

// Ownership: libc process boundary: kqueue/kevent NOTE_EXIT, kill(2), pid liveness, and start time.
// SIGTERM is banned because Rhino turns it into crash markers reconcile would then need to clear.
internal static partial class Posix {
    internal const short EvFiltProc = -5;
    internal const ushort EvAdd = 0x0001;
    internal const ushort EvEnable = 0x0004;
    internal const ushort EvOneShot = 0x0010;
    internal const uint NoteExit = 0x80000000;
    private const int SigKill = 9;
    private const int Eperm = 1;

    [StructLayout(LayoutKind.Sequential)]
    internal struct KEvent {
        public nuint Ident;
        public short Filter;
        public ushort Flags;
        public uint FFlags;
        public nint Data;
        public nint Udata;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct TimeSpec {
        public nint Seconds;
        public nint Nanoseconds;
    }

    internal static bool Alive(int pid) => KillCall(pid: pid, signal: 0) == 0 || Marshal.GetLastPInvokeError() == Eperm;

    internal static bool Kill(int pid) => KillCall(pid: pid, signal: SigKill) == 0;

    internal static Option<long> StartedAtUnixMs(int pid) {
        // BOUNDARY ADAPTER: dead or inaccessible pids project to None.
        try {
            using Process process = Process.GetProcessById(processId: pid);
            return Some(value: new DateTimeOffset(dateTime: process.StartTime.ToUniversalTime()).ToUnixTimeMilliseconds());
        } catch (Exception error) when (error is ArgumentException or InvalidOperationException or Win32Exception) {
            return Option<long>.None;
        }
    }

    [LibraryImport("libc", EntryPoint = "close", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static partial int Close(int fd);

    [LibraryImport("libc", EntryPoint = "kqueue", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static partial int KQueueCreate();

    [LibraryImport("libc", EntryPoint = "kevent", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static partial int KEventRegister(int kq, in KEvent changeList, int changes, nint eventList, int events, nint timeout);

    [LibraryImport("libc", EntryPoint = "kevent", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static partial int KEventWait(int kq, nint changeList, int changes, out KEvent eventOut, int events, in TimeSpec timeout);

    [LibraryImport("libc", EntryPoint = "kill", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static partial int KillCall(int pid, int signal);
}

// Ownership: subprocess boundary for workstation effects. Deadline overruns kill the child tree,
// and event-driven output capture prevents pipe-buffer deadlock.
internal static class Exec {
    internal static Fin<ExecResult> Run(string file, string[] args, TimeSpan deadline) {
        // BOUNDARY ADAPTER: process spawn and IO failures become one typed Fin failure.
        try {
            using Process process = new();
            process.StartInfo.FileName = file;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            System.Array.ForEach(array: args, action: process.StartInfo.ArgumentList.Add);
            StringBuilder stdout = new();
            StringBuilder stderr = new();
            process.OutputDataReceived += (_, received) => Append(buffer: stdout, line: received.Data);
            process.ErrorDataReceived += (_, received) => Append(buffer: stderr, line: received.Data);
            _ = process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            if (!process.WaitForExit(milliseconds: (int)deadline.TotalMilliseconds)) {
                process.Kill(entireProcessTree: true);
                return Fin.Fail<ExecResult>(error: Error.New(message: string.Create(provider: CultureInfo.InvariantCulture,
                    $"{file} exceeded its {deadline.TotalMilliseconds:F0}ms deadline and was killed")));
            }
            process.WaitForExit();
            return Fin.Succ(value: new ExecResult(ExitCode: process.ExitCode, StdOut: stdout.ToString(), StdErr: stderr.ToString()));
        } catch (Exception error) when (error is Win32Exception or InvalidOperationException or IOException or PlatformNotSupportedException) {
            return Fin.Fail<ExecResult>(error: Error.New(message: $"{file} failed to start: {error.Message}"));
        }
    }

    private static void Append(StringBuilder buffer, string? line) =>
        _ = line is null ? buffer : buffer.AppendLine(value: line);
}

// Ownership: bounded liveness polling for endpoint appearance, quit-rung exit, and degraded host
// watch. The deadline selects bounded session waits versus unbounded background watches.
internal static class Poll {
    internal static Fin<T> Until<T>(Func<Option<T>> probe, TimeSpan deadline, TimeSpan cadence, CancellationToken ct) {
        ArgumentNullException.ThrowIfNull(argument: probe);
        bool unbounded = deadline == Timeout.InfiniteTimeSpan;
        long until = unbounded ? 0L : Environment.TickCount64 + (long)deadline.TotalMilliseconds;
        while ((unbounded || Environment.TickCount64 < until) && !ct.IsCancellationRequested) {
            if (probe() is { IsSome: true, Case: T value })
                return Fin.Succ(value: value);
            Thread.Sleep(timeout: cadence);
        }
        return probe() is { IsSome: true, Case: T settled }
            ? Fin.Succ(value: settled)
            : Fin.Fail<T>(error: Error.New(message: ct.IsCancellationRequested
                ? "liveness poll cancelled"
                : string.Create(provider: CultureInfo.InvariantCulture, $"liveness poll exceeded its {deadline.TotalMilliseconds:F0}ms deadline")));
    }
}

// Ownership: host-exit detection through kqueue NOTE_EXIT with PID-poll fallback. Mode reports the
// attached lane so degraded watch behavior remains observable.
internal sealed class HostWatch : IDisposable {
    private readonly int kq;
    private readonly Thread watcher;
    private readonly CancellationTokenSource life = new();

    private HostWatch(int kq, string mode, int pid, Action<SessionSignal> raise, TimeSpan poll, TimeProvider clock) {
        this.kq = kq;
        Mode = mode;
        watcher = new Thread(start: () => Watch(pid: pid, raise: raise, poll: poll, clock: clock)) { IsBackground = true, Name = $"host-watch-{mode}" };
        watcher.Start();
    }

    internal string Mode { get; }

    // Kernel refusal degrades to the poll lane instead of failing attachment.
    internal static HostWatch Attach(int pid, Action<SessionSignal> raise, TimeSpan poll, TimeProvider clock) {
        ArgumentNullException.ThrowIfNull(argument: raise);
        ArgumentNullException.ThrowIfNull(argument: clock);
        (int queue, string mode) = Kernel(pid: pid);
        return new HostWatch(kq: queue, mode: mode, pid: pid, raise: raise, poll: poll, clock: clock);
    }

    private static (int Queue, string Mode) Kernel(int pid) {
        int queue = Posix.KQueueCreate();
        if (queue < 0)
            return (-1, "poll");
        Posix.KEvent change = new() {
            Ident = (nuint)pid, Filter = Posix.EvFiltProc, Flags = Posix.EvAdd | Posix.EvEnable | Posix.EvOneShot,
            FFlags = Posix.NoteExit, Data = 0, Udata = 0,
        };
        if (Posix.KEventRegister(kq: queue, changeList: in change, changes: 1, eventList: 0, events: 0, timeout: 0) == 0)
            return (queue, "kqueue");
        // Registration refusal degrades to PID polling, including already-dead pid races.
        _ = Posix.Close(fd: queue);
        return (-1, "poll");
    }

    public void Dispose() {
        life.Cancel();
        if (kq >= 0)
            _ = Posix.Close(fd: kq);
        _ = watcher.Join(timeout: TimeSpan.FromSeconds(value: 2));
        life.Dispose();
    }

    // Blocking kqueue wait wakes at WatchPoll cadence only so Dispose can stop the watcher.
    private void Watch(int pid, Action<SessionSignal> raise, TimeSpan poll, TimeProvider clock) {
        if (kq < 0) {
            if (Poll.Until(probe: () => Posix.Alive(pid: pid) ? Option<Unit>.None : Some(value: unit),
                           deadline: Timeout.InfiniteTimeSpan, cadence: poll, ct: life.Token).IsSucc)
                raise(new SessionSignal.HostExited(Pid: pid, AtUnixMs: clock.GetUtcNow().ToUnixTimeMilliseconds()));
            return;
        }
        Posix.TimeSpec wake = new() {
            Seconds = (nint)(poll.Ticks / TimeSpan.TicksPerSecond),
            Nanoseconds = (nint)(poll.Ticks % TimeSpan.TicksPerSecond * 100),
        };
        while (!life.IsCancellationRequested) {
            int landed = Posix.KEventWait(kq: kq, changeList: 0, changes: 0, eventOut: out Posix.KEvent observed, events: 1, timeout: in wake);
            if (landed > 0 && (observed.FFlags & Posix.NoteExit) != 0) {
                raise(new SessionSignal.HostExited(Pid: pid, AtUnixMs: clock.GetUtcNow().ToUnixTimeMilliseconds()));
                return;
            }
            if (landed < 0 && Marshal.GetLastPInvokeError() != 4 /* EINTR */)
                return;
        }
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------

// Ownership: singleton lease gate. O_EXCL claims serialize sessions; stale holders are reclaimed
// with evidence, and live holders fail as BusyHeld.
internal static class Lease {
    internal static string CanonicalPath =>
        Path.Combine(path1: Environment.GetFolderPath(folder: Environment.SpecialFolder.UserProfile), path2: ".rasm", path3: "rhino-bridge-rbx.lease");

    internal static Fin<LeaseToken> Acquire(string path, Guid sessionId, TimeProvider clock, Action<BridgeEvent> publish) {
        ArgumentNullException.ThrowIfNull(argument: clock);
        ArgumentNullException.ThrowIfNull(argument: publish);
        long now = clock.GetUtcNow().ToUnixTimeMilliseconds();
        Fin<LeaseToken> claimed = Claim(path: path, now: now);
        return claimed is Fin<LeaseToken>.Succ
            ? claimed
            : Holder(path: path).Case is LeaseClaim held
                ? Posix.StartedAtUnixMs(pid: held.HolderPid).Case is long started && Math.Abs(value: started - held.HolderStartedAtUnixMs) <= 1_000
                    ? Busy(held: held, now: now)
                    : Reclaim(path: path, held: held, sessionId: sessionId, now: now, publish: publish)
                : Claim(path: path, now: now);
    }

    internal static Unit Release(LeaseToken token) {
        ArgumentNullException.ThrowIfNull(argument: token);
        // BOUNDARY ADAPTER: release removes only this process's claim.
        try {
            if (Holder(path: token.Path).Case is LeaseClaim held && held.HolderPid == token.HolderPid)
                File.Delete(path: token.Path);
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
        }
        return unit;
    }

    private static Fin<LeaseToken> Busy(LeaseClaim held, long now) {
        BridgeFault.BusyHeld fault = new(HolderPid: held.HolderPid, AgeSeconds: (now - held.AcquiredAtUnixMs) / 1_000.0);
        return Fin.Fail<LeaseToken>(error: Error.New(code: fault.Status.ExitCode, message: fault.Prescription));
    }

    private static Fin<LeaseToken> Claim(string path, long now) {
        // BOUNDARY ADAPTER: FileMode.CreateNew is the O_EXCL claim.
        try {
            _ = Directory.CreateDirectory(path: Path.GetDirectoryName(path: path) ?? ".");
            using FileStream stream = new(path: path, mode: FileMode.CreateNew, access: FileAccess.Write, share: FileShare.Read);
            JsonSerializer.Serialize(utf8Json: stream, value: new LeaseClaim(
                HolderPid: Environment.ProcessId,
                HolderStartedAtUnixMs: Posix.StartedAtUnixMs(pid: Environment.ProcessId).IfNone(0L),
                AcquiredAtUnixMs: now), jsonTypeInfo: SupervisorJsonContext.Default.LeaseClaim);
            return Fin.Succ(value: new LeaseToken(HolderPid: Environment.ProcessId, AcquiredAtUnixMs: now, Path: path));
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
            return Fin.Fail<LeaseToken>(error: Error.New(message: $"lease claim failed: {error.Message}"));
        }
    }

    private static Option<LeaseClaim> Holder(string path) {
        // BOUNDARY ADAPTER: unreadable or corrupt claims read as absent.
        try {
            return JsonSerializer.Deserialize(json: File.ReadAllText(path: path), jsonTypeInfo: SupervisorJsonContext.Default.LeaseClaim) is { } held
                ? Some(value: held)
                : Option<LeaseClaim>.None;
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException) {
            return Option<LeaseClaim>.None;
        }
    }

    private static Fin<LeaseToken> Reclaim(string path, LeaseClaim held, Guid sessionId, long now, Action<BridgeEvent> publish) {
        // BOUNDARY ADAPTER: if a holder revives during reclaim, the retry reports busy.
        try {
            File.Delete(path: path);
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
            return Fin.Fail<LeaseToken>(error: Error.New(message: $"stale lease delete failed: {error.Message}"));
        }
        publish(HostEvents.Fact(key: "lease.reclaimed", sessionId: sessionId, atUnixMs: now,
            payload: new JsonObject { ["holderPid"] = held.HolderPid, ["acquiredAtUnixMs"] = held.AcquiredAtUnixMs, ["path"] = path }));
        return Claim(path: path, now: now);
    }
}

// Ownership: workstation-side fact materialization; terminal fold assigns final sequence ordering.
internal static class HostEvents {
    internal static BridgeEvent.FactCase Fact(string key, Guid sessionId, long atUnixMs, JsonObject payload) {
        ArgumentNullException.ThrowIfNull(argument: payload);
        using JsonDocument value = JsonDocument.Parse(json: payload.ToJsonString());
        return new BridgeEvent.FactCase(Key: key, Value: value.RootElement.Clone()) {
            Stamp = new EventStamp(SessionId: sessionId, Sequence: 0, AtUnixMs: atUnixMs, Scenario: null),
        };
    }
}

// Ownership: retired-instance journal. QuitLadder writes confirmed windows; Reconcile reads them
// to scope marker cleanup, and corrupt lines decode to no window.
internal static class QuitJournal {
    internal static string CanonicalPath =>
        Path.Combine(path1: Environment.GetFolderPath(folder: Environment.SpecialFolder.UserProfile), path2: ".rasm", path3: "rhino-bridge-quits.jsonl");

    internal static Unit Append(string path, QuitJournalEntry entry) {
        // BOUNDARY ADAPTER: journal write failures cannot fail a completed quit.
        try {
            _ = Directory.CreateDirectory(path: Path.GetDirectoryName(path: path) ?? ".");
            File.AppendAllText(path: path, contents: JsonSerializer.Serialize(value: entry, jsonTypeInfo: SupervisorJsonContext.Default.QuitJournalEntry) + Environment.NewLine);
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
        }
        return unit;
    }

    internal static Seq<QuitJournalEntry> Read(string path) {
        // BOUNDARY ADAPTER: absent journal means no supervised windows.
        try {
            return !File.Exists(path: path)
                ? Seq<QuitJournalEntry>()
                : toSeq(value: File.ReadLines(path: path)).Choose(selector: static line => Decode(line: line));
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
            return Seq<QuitJournalEntry>();
        }
    }

    private static Option<QuitJournalEntry> Decode(string line) {
        try {
            return JsonSerializer.Deserialize(json: line, jsonTypeInfo: SupervisorJsonContext.Default.QuitJournalEntry) is { } entry
                ? Some(value: entry)
                : Option<QuitJournalEntry>.None;
        } catch (JsonException) {
            return Option<QuitJournalEntry>.None;
        }
    }
}

// Ownership: quit ladder. Rungs are AE terminate, Cocoa forceTerminate, then kill(2) SIGKILL;
// SIGTERM is banned. Each rung emits a PhaseCase, and confirmed closes journal reconcile windows.
internal static class QuitLadder {
    internal static Eff<SupervisorRuntime, PhaseStatus> Run(LiveHost host, Guid sessionId, Action<BridgeEvent> publish) {
        ArgumentNullException.ThrowIfNull(argument: host);
        ArgumentNullException.ThrowIfNull(argument: publish);
        return Eff<SupervisorRuntime, PhaseStatus>.Lift(f: runtime => Fin.Succ(value: Descend(runtime: runtime, host: host, sessionId: sessionId, publish: publish)));
    }

    private static PhaseStatus Descend(SupervisorRuntime runtime, LiveHost host, Guid sessionId, Action<BridgeEvent> publish) =>
        Seq(SessionPhase.QuitAe, SessionPhase.QuitForce, SessionPhase.QuitKill)
            .Fold(initialState: Option<PhaseStatus>.None, f: (closed, rung) => closed.IsSome
                ? closed
                : Rung(runtime: runtime, host: host, rung: rung, sessionId: sessionId, publish: publish))
            .IfNone(PhaseStatus.Failed);

    private static string Jxa(int pid, string verb) =>
        string.Create(provider: CultureInfo.InvariantCulture,
            $"ObjC.import('AppKit'); var a = $.NSRunningApplication.runningApplicationWithProcessIdentifier({pid}); a ? a.{verb} : false;");

    private static Option<PhaseStatus> Rung(SupervisorRuntime runtime, LiveHost host, SessionPhase rung, Guid sessionId, Action<BridgeEvent> publish) {
        long startedMs = runtime.Clock.GetUtcNow().ToUnixTimeMilliseconds();
        _ = rung == SessionPhase.QuitKill
            ? Fin.Succ(value: Posix.Kill(pid: host.Pid))
            : Exec.Run(file: "osascript",
                args: ["-l", "JavaScript", "-e", Jxa(pid: host.Pid, verb: rung == SessionPhase.QuitAe ? "terminate" : "forceTerminate")],
                deadline: runtime.Policy.QuitRungDeadline).Map(f: static result => result.ExitCode == 0);
        bool closed = Poll.Until(probe: () => Posix.Alive(pid: host.Pid) ? Option<Unit>.None : Some(value: unit),
            deadline: runtime.Policy.QuitRungDeadline, cadence: runtime.Policy.WatchPoll, ct: CancellationToken.None).IsSucc;
        long endedMs = runtime.Clock.GetUtcNow().ToUnixTimeMilliseconds();
        publish(new BridgeEvent.PhaseCase(Phase: rung, Status: closed ? PhaseStatus.Ok : PhaseStatus.Failed, DurationMs: endedMs - startedMs, Fault: null) {
            Stamp = new EventStamp(SessionId: sessionId, Sequence: 0, AtUnixMs: endedMs, Scenario: null),
        });
        if (!closed)
            return Option<PhaseStatus>.None;
        _ = QuitJournal.Append(path: runtime.JournalPath, entry: new QuitJournalEntry(
            Pid: host.Pid, StartedAtUnixMs: host.StartedAtUnixMs, RetiredAtUnixMs: endedMs, Rung: rung.Key, PipeName: host.Endpoint.PipeName));
        return Some(value: PhaseStatus.Ok);
    }
}

// Ownership: pre-launch crash-marker reconcile. Only markers inside supervised journal windows are
// cleared; foreign recovery state is reported and preserved.
internal static class Reconcile {
    internal static Eff<SupervisorRuntime, Seq<BridgeEvent>> Run(BundleInfo bundle, Guid sessionId) {
        ArgumentNullException.ThrowIfNull(argument: bundle);
        return Eff<SupervisorRuntime, Seq<BridgeEvent>>.Lift(f: runtime => Fin.Succ(value: Sweep(runtime: runtime, bundle: bundle, sessionId: sessionId)));
    }

    private static Seq<BridgeEvent> Sweep(SupervisorRuntime runtime, BundleInfo bundle, Guid sessionId) {
        Seq<QuitJournalEntry> journal = QuitJournal.Read(path: runtime.JournalPath);
        long slack = (long)runtime.Policy.JournalSlack.TotalMilliseconds;
        long now = runtime.Clock.GetUtcNow().ToUnixTimeMilliseconds();
        return Markers(bundle: bundle).Map(f: BridgeEvent (path) => Classify(path: path, journal: journal, slack: slack, sessionId: sessionId, atUnixMs: now));
    }

    private static BridgeEvent.FactCase Classify(string path, Seq<QuitJournalEntry> journal, long slack, Guid sessionId, long atUnixMs) {
        long observedMs = new DateTimeOffset(dateTime: File.GetLastWriteTimeUtc(path: path)).ToUnixTimeMilliseconds();
        bool supervised = journal.Exists(f: entry => observedMs >= entry.StartedAtUnixMs && observedMs <= entry.RetiredAtUnixMs + slack);
        JsonObject payload = new() { ["path"] = path, ["observedAtUnixMs"] = observedMs };
        return supervised
            ? HostEvents.Fact(key: TryDelete(path: path) ? "reconcile.cleared" : "reconcile.clear-failed", sessionId: sessionId, atUnixMs: atUnixMs, payload: payload)
            : HostEvents.Fact(key: "reconcile.skipped.foreign", sessionId: sessionId, atUnixMs: atUnixMs, payload: payload);
    }

    private static Seq<string> Markers(BundleInfo bundle) {
        string library = Path.Combine(path1: Environment.GetFolderPath(folder: Environment.SpecialFolder.UserProfile), path2: "Library");
        string autosave = Path.Combine(path1: library, path2: "Autosave Information");
        Seq<string> documents = Seq(
            Path.Combine(path1: autosave, path2: bundle.AutosaveMarker + ".rhl"),
            Path.Combine(path1: autosave, path2: bundle.AutosaveMarker)).Filter(f: File.Exists);
        return documents + Reports(directory: Path.Combine(path1: library, path2: "Logs", path3: "DiagnosticReports"), pattern: bundle.CrashReportPattern);
    }

    private static Seq<string> Reports(string directory, string pattern) {
        // BOUNDARY ADAPTER: absence or permission denial yields an empty set.
        try {
            return Directory.Exists(path: directory) ? toSeq(value: Directory.GetFiles(path: directory, searchPattern: pattern)) : Seq<string>();
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
            return Seq<string>();
        }
    }

    private static bool TryDelete(string path) {
        // BOUNDARY ADAPTER: marker deletion is best-effort and facts carry disposition.
        try {
            File.Delete(path: path);
            return true;
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException or DirectoryNotFoundException) {
            return false;
        }
    }
}

// --- [COMPOSITION] ------------------------------------------------------------------------

// Ownership: supervisor-private file codec; BridgeJsonContext owns wire shapes.
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(LeaseClaim))]
[JsonSerializable(typeof(QuitJournalEntry))]
[JsonSerializable(typeof(ClosureManifest))]
internal sealed partial class SupervisorJsonContext : JsonSerializerContext;
