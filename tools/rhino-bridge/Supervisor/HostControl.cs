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

// --- [MODELS] --------------------------------------------------------------------------

internal sealed record BundleInfo(string AppPath, string CFBundleName, string CFBundleExecutable, string CFBundleVersion) {
    internal const int RhinoLineMajor = 9;
    internal const string AppPathVariable = "RHINO_APP_PATH";
    internal const string ApplicationsDirectory = "/Applications";

    public string AutosaveMarker => $"Unsaved {CFBundleName} Document.3dm";
    public string CrashReportPattern => $"{CFBundleExecutable}-*.ips";
    private Version Numeric => Version.TryParse(CFBundleVersion, out Version? parsed) ? parsed : new Version(0, 0);

    internal static string UserLibraryDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library");

    public static Fin<BundleInfo> Discover(TimeSpan toolDeadline) {
        string? narrowed = Environment.GetEnvironmentVariable(AppPathVariable);
        bool pinned = narrowed is { Length: > 0 };
        Seq<string> candidates = pinned ? Seq(narrowed!) : Candidates();
        Seq<BundleInfo> admitted = candidates.Choose(path =>
            Read(path, toolDeadline).Filter(bundle => pinned || bundle.Numeric.Major >= RhinoLineMajor));
        return toSeq(admitted.OrderByDescending(bundle => bundle.Numeric)).Head.Case is BundleInfo newest
            ? newest
            : Error.New(string.Create(CultureInfo.InvariantCulture,
                $"no Rhino {RhinoLineMajor}+ bundle under {ApplicationsDirectory}; set {AppPathVariable} to one .app bundle"));
    }

    public Fin<Unit> Launch(TimeSpan toolDeadline) =>
        Exec.Run("/usr/bin/open", ["-a", AppPath, "--args", "-nosplash"], toolDeadline)
            .Bind(result => result.ExitCode == 0
                ? Fin.Succ(unit)
                : Error.New(string.Create(CultureInfo.InvariantCulture, $"open '{AppPath}' exited {result.ExitCode}: {result.StdErr.Trim()}")));

    private static Seq<string> Candidates() {
        try {
            return toSeq(Directory.GetDirectories(ApplicationsDirectory, "Rhino*.app"));
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
            return Seq<string>();
        }
    }

    private static Option<BundleInfo> Read(string appPath, TimeSpan toolDeadline) =>
        Exec.Run("/usr/bin/plutil", ["-convert", "json", "-o", "-", Path.Combine(appPath, "Contents", "Info.plist")], toolDeadline)
                is Fin<Exec.Result>.Succ(Exec.Result plist) && plist.ExitCode == 0
            ? Decode(appPath, plist.StdOut)
            : Option<BundleInfo>.None;

    private static Option<BundleInfo> Decode(string appPath, string json) {
        try {
            using JsonDocument plist = JsonDocument.Parse(json);
            return new BundleInfo(
                appPath,
                Member(plist.RootElement, "CFBundleName"),
                Member(plist.RootElement, "CFBundleExecutable"),
                Member(plist.RootElement, "CFBundleVersion"));
        } catch (JsonException) {
            return Option<BundleInfo>.None;
        }
    }

    private static string Member(JsonElement root, string name) =>
        root.TryGetProperty(name, out JsonElement member) ? member.GetString() ?? string.Empty : string.Empty;
}

internal sealed record LiveHost(int Pid, long StartedAtUnixMs, EndpointRecord.Live Endpoint, HostFingerprint Fingerprint) {
    public static Fin<LiveHost> Admit(EndpointRecord.Live endpoint, HostFingerprint fingerprint) {
        ArgumentNullException.ThrowIfNull(endpoint);
        return Posix.StartedAtUnixMs(endpoint.RhinoPid).Case is long started
            ? endpoint.IsLiveFor(endpoint.RhinoPid, started)
                ? new LiveHost(endpoint.RhinoPid, started, endpoint, fingerprint)
                : Error.New(string.Create(CultureInfo.InvariantCulture,
                    $"endpoint stale: pid {endpoint.RhinoPid} start-time drift (recorded {endpoint.RhinoStartedAtUnixMs}, observed {started}) — pid recycled"))
            : Error.New(string.Create(CultureInfo.InvariantCulture, $"endpoint stale: pid {endpoint.RhinoPid} is not alive"));
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

internal sealed class SupervisorConnection : IAsyncDisposable {
    private readonly NamedPipeClientStream stream;
    private readonly SystemTextJsonFormatter formatter;
    private readonly HeaderDelimitedMessageHandler handler;
    private readonly JsonRpc rpc;
    private readonly ConcurrentQueue<BridgeEvent> events = new();
    private bool disposed;

    private SupervisorConnection(NamedPipeClientStream stream, SystemTextJsonFormatter formatter, HeaderDelimitedMessageHandler handler, JsonRpc rpc) {
        this.stream = stream;
        this.formatter = formatter;
        this.handler = handler;
        this.rpc = rpc;
        rpc.AddLocalRpcTarget<IBridgeEvents>(new EventSink(events), options: null);
        Shell = rpc.Attach<IBridgeShell>();
        rpc.StartListening();
    }

    internal IBridgeShell Shell { get; }
    internal BridgeEvent[] Events => [.. events];

    internal static async Task<SupervisorConnection> ConnectAsync(string pipeName, TimeSpan timeout, CancellationToken ct) {
        NamedPipeClientStream pipe = new(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
        try {
            using CancellationTokenSource linked = CancellationTokenSource.CreateLinkedTokenSource(ct);
            linked.CancelAfter(timeout);
            await pipe.ConnectAsync(linked.Token).ConfigureAwait(false);
        } catch {
            await pipe.DisposeAsync().ConfigureAwait(false);
            throw;
        }
        SystemTextJsonFormatter wire = new() {
            JsonSerializerOptions = new JsonSerializerOptions(BridgeJsonContext.Default.Options) {
                TypeInfoResolver = JsonTypeInfoResolver.Combine(BridgeJsonContext.Default, new DefaultJsonTypeInfoResolver()),
            },
        };
        HeaderDelimitedMessageHandler messages = new(pipe, wire);
        return new SupervisorConnection(pipe, wire, messages, new BridgeRpc(messages));
    }

    public async ValueTask DisposeAsync() {
        if (disposed) {
            return;
        }
        disposed = true;
        rpc.Dispose();
        await handler.DisposeAsync().ConfigureAwait(false);
        formatter.Dispose();
        await stream.DisposeAsync().ConfigureAwait(false);
    }

    private sealed class EventSink(ConcurrentQueue<BridgeEvent> events) : IBridgeEvents {
        public Task PublishAsync(BridgeEvent evt) {
            events.Enqueue(evt);
            return Task.CompletedTask;
        }
    }

    private sealed class BridgeRpc(IJsonRpcMessageHandler messageHandler) : JsonRpc(messageHandler) {
        protected override Type? GetErrorDetailsDataType(JsonRpcError error) =>
            error.Error?.Code is { } code && (int)code == BridgeFault.RpcErrorCode ? typeof(BridgeFault) : base.GetErrorDetailsDataType(error);
    }
}

internal static partial class Posix {
    private const short EvFiltProc = -5;
    private const ushort EvAdd = 0x0001;
    private const ushort EvEnable = 0x0004;
    private const ushort EvOneShot = 0x0010;
    private const uint NoteExit = 0x80000000;
    private const int SigKill = 9;
    private const int Eperm = 1;
    private const int Eintr = 4;

    [StructLayout(LayoutKind.Sequential)]
    private struct KEvent {
        public nuint Ident;
        public short Filter;
        public ushort Flags;
        public uint FFlags;
        public nint Data;
        public nint Udata;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct TimeSpec {
        public nint Seconds;
        public nint Nanoseconds;
    }

    internal static bool Alive(int pid) => KillCall(pid, 0) == 0 || Marshal.GetLastPInvokeError() == Eperm;

    internal static bool Kill(int pid) => KillCall(pid, SigKill) == 0;

    internal static Option<long> StartedAtUnixMs(int pid) {
        try {
            using Process process = Process.GetProcessById(pid);
            return new DateTimeOffset(process.StartTime.ToUniversalTime()).ToUnixTimeMilliseconds();
        } catch (Exception error) when (error is ArgumentException or InvalidOperationException or Win32Exception) {
            return Option<long>.None;
        }
    }

    internal static int WatchExit(int pid) {
        int queue = KQueueCreate();
        if (queue < 0) {
            return -1;
        }
        KEvent change = new() { Ident = (nuint)pid, Filter = EvFiltProc, Flags = EvAdd | EvEnable | EvOneShot, FFlags = NoteExit };
        if (KEventRegister(queue, in change, 1, 0, 0, 0) == 0) {
            return queue;
        }
        _ = Close(queue);
        return -1;
    }

    internal static Option<bool> AwaitExit(int queue, TimeSpan window) {
        TimeSpec wake = new() { Seconds = (nint)(window.Ticks / TimeSpan.TicksPerSecond), Nanoseconds = (nint)(window.Ticks % TimeSpan.TicksPerSecond * 100) };
        int landed = KEventWait(queue, 0, 0, out KEvent observed, 1, in wake);
        return landed > 0 && (observed.FFlags & NoteExit) != 0 ? true
            : landed >= 0 || Marshal.GetLastPInvokeError() == Eintr ? false
            : Option<bool>.None;
    }

    [LibraryImport("libc", EntryPoint = "close", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static partial int Close(int fd);

    [LibraryImport("libc", EntryPoint = "kqueue", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static partial int KQueueCreate();

    [LibraryImport("libc", EntryPoint = "kevent", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static partial int KEventRegister(int kq, in KEvent changeList, int changes, nint eventList, int events, nint timeout);

    [LibraryImport("libc", EntryPoint = "kevent", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static partial int KEventWait(int kq, nint changeList, int changes, out KEvent eventOut, int events, in TimeSpec timeout);

    [LibraryImport("libc", EntryPoint = "kill", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static partial int KillCall(int pid, int signal);
}

internal static class Exec {
    internal readonly record struct Result(int ExitCode, string StdOut, string StdErr);

    internal static Fin<Result> Run(string file, string[] args, TimeSpan deadline) {
        try {
            using Process process = new();
            process.StartInfo.FileName = file;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            System.Array.ForEach(args, process.StartInfo.ArgumentList.Add);
            StringBuilder stdout = new();
            StringBuilder stderr = new();
            process.OutputDataReceived += (_, received) => Append(stdout, received.Data);
            process.ErrorDataReceived += (_, received) => Append(stderr, received.Data);
            _ = process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            if (!process.WaitForExit((int)deadline.TotalMilliseconds)) {
                process.Kill(entireProcessTree: true);
                return Error.New(string.Create(CultureInfo.InvariantCulture, $"{file} exceeded its {deadline.TotalMilliseconds:F0}ms deadline and was killed"));
            }
            process.WaitForExit();
            return new Result(process.ExitCode, stdout.ToString(), stderr.ToString());
        } catch (Exception error) when (error is Win32Exception or InvalidOperationException or IOException or PlatformNotSupportedException) {
            return Error.New($"{file} failed to start: {error.Message}");
        }
    }

    private static void Append(StringBuilder buffer, string? line) =>
        _ = line is null ? buffer : buffer.AppendLine(line);
}

internal static class Poll {
    internal static Fin<T> Until<T>(Func<Option<T>> probe, TimeSpan deadline, TimeSpan cadence, CancellationToken ct) {
        ArgumentNullException.ThrowIfNull(probe);
        bool unbounded = deadline == Timeout.InfiniteTimeSpan;
        long until = unbounded ? 0L : Environment.TickCount64 + (long)deadline.TotalMilliseconds;
        while ((unbounded || Environment.TickCount64 < until) && !ct.IsCancellationRequested) {
            if (probe().Case is T value) {
                return value;
            }
            Thread.Sleep(cadence);
        }
        return probe().Case is T settled
            ? settled
            : Error.New(ct.IsCancellationRequested
                ? "liveness poll cancelled"
                : string.Create(CultureInfo.InvariantCulture, $"liveness poll exceeded its {deadline.TotalMilliseconds:F0}ms deadline"));
    }
}

internal sealed class HostWatch : IDisposable {
    private readonly int queue;
    private readonly Thread watcher;
    private readonly CancellationTokenSource life = new();

    private HostWatch(int queue, int pid, Action exited, TimeSpan poll) {
        this.queue = queue;
        watcher = new Thread(() => Watch(pid, exited, poll)) { IsBackground = true, Name = $"host-watch-{Mode}" };
        watcher.Start();
    }

    internal string Mode => queue >= 0 ? "kqueue" : "poll";

    internal static HostWatch Attach(int pid, Action exited, TimeSpan poll) {
        ArgumentNullException.ThrowIfNull(exited);
        return new HostWatch(Posix.WatchExit(pid), pid, exited, poll);
    }

    public void Dispose() {
        life.Cancel();
        if (queue >= 0) {
            _ = Posix.Close(queue);
        }
        _ = watcher.Join(TimeSpan.FromSeconds(2));
        life.Dispose();
    }

    private void Watch(int pid, Action exited, TimeSpan poll) {
        bool gone = queue < 0
            ? Poll.Until(() => Posix.Alive(pid) ? Option<Unit>.None : Some(unit), Timeout.InfiniteTimeSpan, poll, life.Token).IsSucc
            : AwaitKernel(poll);
        if (gone) {
            exited();
        }
    }

    private bool AwaitKernel(TimeSpan poll) {
        while (!life.IsCancellationRequested) {
            switch (Posix.AwaitExit(queue, poll).Case) {
                case true:
                    return true;
                case false:
                    continue;
                default:
                    return false;
            }
        }
        return false;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

internal static class Lease {
    internal sealed record Token(int HolderPid, string Path);
    internal sealed record Claim(int HolderPid, long HolderStartedAtUnixMs, long AcquiredAtUnixMs);

    internal static string CanonicalPath => RasmHome.Resolve("rhino-bridge-rbx.lease");

    internal static Fin<Token> Acquire(string path, Guid sessionId, TimeProvider clock, Action<BridgeEvent> publish) {
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(publish);
        long now = clock.GetUtcNow().ToUnixTimeMilliseconds();
        Fin<Token> claimed = Write(path, now);
        return claimed.IsSucc
            ? claimed
            : Holder(path).Case is Claim held
                ? HolderAlive(held) ? Busy(held, now) : Reclaim(path, held, sessionId, now, publish)
                : Write(path, now);
    }

    internal static Unit Release(Token token) {
        ArgumentNullException.ThrowIfNull(token);
        try {
            if (Holder(token.Path).Case is Claim held && held.HolderPid == token.HolderPid) {
                File.Delete(token.Path);
            }
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
        }
        return unit;
    }

    private static bool HolderAlive(Claim held) =>
        Posix.Alive(held.HolderPid)
        && Posix.StartedAtUnixMs(held.HolderPid).Case is long started
        && Math.Abs(started - held.HolderStartedAtUnixMs) <= EndpointRecord.LivenessSkewMs;

    private static Fin<Token> Busy(Claim held, long now) {
        BridgeFault.BusyHeld fault = new(held.HolderPid, (now - held.AcquiredAtUnixMs) / 1_000.0);
        return Error.New(fault.Status.ExitCode, fault.Prescription);
    }

    private static Fin<Token> Write(string path, long now) {
        try {
            _ = Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
            using FileStream stream = new(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
            JsonSerializer.Serialize(stream, new Claim(
                Environment.ProcessId,
                Posix.StartedAtUnixMs(Environment.ProcessId).IfNone(0L),
                now), SupervisorJsonContext.Default.Claim);
            return new Token(Environment.ProcessId, path);
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
            return Error.New($"lease claim failed: {error.Message}");
        }
    }

    private static Option<Claim> Holder(string path) {
        try {
            return Optional(JsonSerializer.Deserialize(File.ReadAllText(path), SupervisorJsonContext.Default.Claim));
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException) {
            return Option<Claim>.None;
        }
    }

    private static Fin<Token> Reclaim(string path, Claim held, Guid sessionId, long now, Action<BridgeEvent> publish) {
        try {
            File.Delete(path);
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
            return Error.New($"stale lease delete failed: {error.Message}");
        }
        publish(BridgeEvent.Fact("lease.reclaimed",
            new JsonObject { ["holderPid"] = held.HolderPid, ["acquiredAtUnixMs"] = held.AcquiredAtUnixMs, ["path"] = path },
            new EventStamp(sessionId, 0, now, Scenario: null)));
        return Write(path, now);
    }
}

internal static class Shutdown {
    internal static Unit Drive(SupervisorRuntime runtime, string reason) {
        ArgumentNullException.ThrowIfNull(runtime);
        long now = runtime.Clock.GetUtcNow().ToUnixTimeMilliseconds();
        if ((runtime.LiveHostPid.Value | LivePid()).Case is int pid && pid > 0 && Posix.Alive(pid)) {
            _ = Posix.Kill(pid);
        }
        Endpoint.Poison(new EndpointRecord.Poisoned(0, now, string.Empty, $"supervisor shutdown: {reason}"));
        _ = runtime.Held.SwapMaybe(held => held.Map(token => { _ = Lease.Release(token); return Option<Lease.Token>.None; }));
        return unit;
    }

    private static Option<int> LivePid() =>
        Endpoint.Read() is Fin<EndpointRecord>.Succ(EndpointRecord.Live live) ? live.RhinoPid : Option<int>.None;
}

internal static class Endpoint {
    internal static Fin<EndpointRecord> Read() {
        string path = EndpointRecord.FilePath;
        try {
            return !File.Exists(path)
                ? Error.New($"endpoint absent at '{path}'")
                : Optional(JsonSerializer.Deserialize(File.ReadAllText(path), BridgeJsonContext.Default.EndpointRecord))
                    .ToFin(Error.New($"endpoint decoded to null: '{path}'"));
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException or NotSupportedException) {
            return Error.New($"endpoint read failed: {error.Message}");
        }
    }

    internal static Fin<LiveHost> ReadLive() =>
        Read().Bind(record => record.Switch(
            live: static live => LiveHost.Admit(live, new HostFingerprint(live.RhinoVersion, string.Empty, string.Empty, string.Empty)),
            poisoned: static Fin<LiveHost> (poisoned) => Error.New($"poisoned endpoint: {poisoned.Fault}")));

    internal static void Poison(EndpointRecord.Poisoned record) {
        try {
            if (File.Exists(EndpointRecord.FilePath)) {
                File.WriteAllText(EndpointRecord.FilePath, JsonSerializer.Serialize(record, BridgeJsonContext.Default.EndpointRecord));
            }
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
        }
    }
}

internal static class QuitJournal {
    internal sealed record Entry(int Pid, long StartedAtUnixMs, long RetiredAtUnixMs, string Rung, string PipeName);

    internal const int RetainedWindows = 256;

    internal static string CanonicalPath => RasmHome.Resolve("rhino-bridge-quits.jsonl");

    internal static Unit Append(string path, Entry entry) {
        try {
            _ = Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
            File.AppendAllText(path, JsonSerializer.Serialize(entry, SupervisorJsonContext.Default.Entry) + Environment.NewLine);
            Seq<Entry> entries = Read(path);
            if (entries.Count > RetainedWindows) {
                File.WriteAllLines(path, entries.Skip(entries.Count - RetainedWindows)
                    .Map(kept => JsonSerializer.Serialize(kept, SupervisorJsonContext.Default.Entry)));
            }
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
        }
        return unit;
    }

    internal static Seq<Entry> Read(string path) {
        try {
            return !File.Exists(path)
                ? Seq<Entry>()
                : toSeq(File.ReadLines(path)).Choose(Decode);
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
            return Seq<Entry>();
        }
    }

    private static Option<Entry> Decode(string line) {
        try {
            return Optional(JsonSerializer.Deserialize(line, SupervisorJsonContext.Default.Entry));
        } catch (JsonException) {
            return Option<Entry>.None;
        }
    }
}

internal static class QuitPrepare {
    private const int MaxAttempts = 2;

    internal static async Task RunAsync(Func<CancellationToken, Task<QuitScrub>> prepare, TimeSpan deadline, TimeProvider clock, Guid sessionId, Action<BridgeEvent> publish, CancellationToken root) {
        ArgumentNullException.ThrowIfNull(prepare);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(publish);
        Option<QuitScrub> clean = Option<QuitScrub>.None;
        string lastDetail = "scrub never settled before the bound";
        for (int attempt = 1; attempt <= MaxAttempts && clean.IsNone; attempt++) {
            (Option<QuitScrub> scrub, string detail) = await AttemptAsync(prepare, deadline, root).ConfigureAwait(false);
            lastDetail = detail;
            clean = scrub.Filter(static settled => settled.Scrubbed);
        }
        EventStamp stamp = new(sessionId, 0, clock.GetUtcNow().ToUnixTimeMilliseconds(), Scenario: null);
        publish(clean.Case is QuitScrub settled
            ? BridgeEvent.Fact("quit.prepared", JsonSerializer.SerializeToElement(settled, BridgeJsonContext.Default.QuitScrub), stamp)
            : BridgeEvent.Fact("quit.prepare.incomplete", lastDetail, stamp));
    }

    private static async Task<(Option<QuitScrub> Scrub, string Detail)> AttemptAsync(Func<CancellationToken, Task<QuitScrub>> prepare, TimeSpan deadline, CancellationToken root) {
        using CancellationTokenSource scope = CancellationTokenSource.CreateLinkedTokenSource(root);
        scope.CancelAfter(deadline);
        try {
            QuitScrub scrub = await prepare(scope.Token).ConfigureAwait(false);
            return (scrub, scrub.Scrubbed
                ? string.Empty
                : string.Create(CultureInfo.InvariantCulture, $"scrub left {scrub.ResidualDirty} doc(s) modified; dirtyPaths={string.Join(',', scrub.DirtyPaths)}"));
        } catch (OperationCanceledException) {
            return (Option<QuitScrub>.None, string.Create(CultureInfo.InvariantCulture, $"scrub exceeded its {deadline.TotalMilliseconds:F0}ms bound"));
        } catch (Exception error) when (error is RemoteRpcException or JsonException or IOException or ObjectDisposedException) {
            return (Option<QuitScrub>.None, $"{error.GetType().Name}: {error.Message}");
        }
    }
}

internal static class QuitLadder {
    internal static PhaseStatus Run(SupervisorRuntime runtime, LiveHost host, Guid sessionId, Action<BridgeEvent> publish) {
        ArgumentNullException.ThrowIfNull(runtime);
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(publish);
        return Seq(SessionPhase.QuitAe, SessionPhase.QuitForce, SessionPhase.QuitKill)
            .Fold(Option<PhaseStatus>.None, (closed, rung) => closed.IsSome ? closed : Rung(runtime, host, rung, sessionId, publish))
            .IfNone(PhaseStatus.Failed);
    }

    private static string Jxa(int pid, string verb) =>
        string.Create(CultureInfo.InvariantCulture,
            $"ObjC.import('AppKit'); var a = $.NSRunningApplication.runningApplicationWithProcessIdentifier({pid}); a ? a.{verb} : false;");

    private static Option<PhaseStatus> Rung(SupervisorRuntime runtime, LiveHost host, SessionPhase rung, Guid sessionId, Action<BridgeEvent> publish) {
        long startedMs = runtime.Clock.GetUtcNow().ToUnixTimeMilliseconds();
        if (rung == SessionPhase.QuitKill) {
            _ = Posix.Kill(host.Pid);
        } else {
            _ = Exec.Run("/usr/bin/osascript", ["-l", "JavaScript", "-e", Jxa(host.Pid, rung == SessionPhase.QuitAe ? "terminate" : "forceTerminate")], runtime.Policy.QuitRungDeadline);
        }
        bool closed = Poll.Until(() => Posix.Alive(host.Pid) ? Option<Unit>.None : Some(unit),
            runtime.Policy.QuitRungDeadline, runtime.Policy.WatchPoll, CancellationToken.None).IsSucc;
        long endedMs = runtime.Clock.GetUtcNow().ToUnixTimeMilliseconds();
        publish(new BridgeEvent.PhaseCase(rung, closed ? PhaseStatus.Ok : PhaseStatus.Failed, endedMs - startedMs, Fault: null) {
            Stamp = new EventStamp(sessionId, 0, endedMs, Scenario: null),
        });
        _ = QuitJournal.Append(runtime.JournalPath, new QuitJournal.Entry(host.Pid, host.StartedAtUnixMs, endedMs, rung.Key, host.Endpoint.PipeName));
        return closed ? PhaseStatus.Ok : Option<PhaseStatus>.None;
    }
}

internal static class Reconcile {
    internal static Seq<BridgeEvent> Sweep(SupervisorRuntime runtime, Guid sessionId) {
        ArgumentNullException.ThrowIfNull(runtime);
        Seq<QuitJournal.Entry> journal = QuitJournal.Read(runtime.JournalPath);
        long slack = (long)runtime.Policy.JournalSlack.TotalMilliseconds;
        long now = runtime.Clock.GetUtcNow().ToUnixTimeMilliseconds();
        return Markers(runtime).Map(BridgeEvent (path) => Classify(path, journal, slack, sessionId, now));
    }

    internal static Seq<BridgeEvent> ClearRecovery(SupervisorRuntime runtime, Guid sessionId) {
        ArgumentNullException.ThrowIfNull(runtime);
        long now = runtime.Clock.GetUtcNow().ToUnixTimeMilliseconds();
        Seq<string> blockers = Seq(Path.Combine(runtime.AutosaveDirectory, runtime.Bundle.AutosaveMarker + ".rhl")).Filter(File.Exists)
            + Reports(runtime.DiagnosticReportsDirectory, runtime.Bundle.CrashReportPattern);
        return blockers.Map(BridgeEvent (path) => BridgeEvent.Fact(
            TryDelete(path) ? "recovery.cleared" : "recovery.clear-failed",
            new JsonObject { ["path"] = path, ["reason"] = "pre-launch recovery-dialog blocker" },
            new EventStamp(sessionId, 0, now, Scenario: null)));
    }

    private static BridgeEvent.FactCase Classify(string path, Seq<QuitJournal.Entry> journal, long slack, Guid sessionId, long atUnixMs) {
        long observedMs = new DateTimeOffset(File.GetLastWriteTimeUtc(path)).ToUnixTimeMilliseconds();
        bool supervised = journal.Exists(entry => observedMs >= entry.StartedAtUnixMs && observedMs <= entry.RetiredAtUnixMs + slack);
        JsonObject payload = new() { ["path"] = path, ["observedAtUnixMs"] = observedMs };
        EventStamp stamp = new(sessionId, 0, atUnixMs, Scenario: null);
        return supervised
            ? BridgeEvent.Fact(TryDelete(path) ? "reconcile.cleared" : "reconcile.clear-failed", payload, stamp)
            : BridgeEvent.Fact("reconcile.skipped.foreign", payload, stamp);
    }

    private static Seq<string> Markers(SupervisorRuntime runtime) =>
        Seq(
            Path.Combine(runtime.AutosaveDirectory, runtime.Bundle.AutosaveMarker + ".rhl"),
            Path.Combine(runtime.AutosaveDirectory, runtime.Bundle.AutosaveMarker)).Filter(File.Exists)
        + Reports(runtime.DiagnosticReportsDirectory, runtime.Bundle.CrashReportPattern);

    internal static Seq<string> Reports(string directory, string pattern) {
        try {
            return Directory.Exists(directory) ? toSeq(Directory.GetFiles(directory, pattern)) : Seq<string>();
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
            return Seq<string>();
        }
    }

    private static bool TryDelete(string path) {
        try {
            File.Delete(path);
            return true;
        } catch (Exception error) when (error is IOException or UnauthorizedAccessException) {
            return false;
        }
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(Lease.Claim))]
[JsonSerializable(typeof(QuitJournal.Entry))]
internal sealed partial class SupervisorJsonContext : JsonSerializerContext;
