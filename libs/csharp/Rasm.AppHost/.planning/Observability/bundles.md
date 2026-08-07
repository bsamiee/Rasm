# [APPHOST_SUPPORT_BUNDLES]

Support capture owns the runtime spine's bounded diagnostic evidence surface: one `SupportTrigger` union admits every cause, and one fold freezes the window, gathers ordered artifact rows, redacts before write, caps with receipts, and lands one zip. Capture law owns the trigger, artifact vocabulary, policy values, manifest, and receipt wire shapes. Each bundle is process-local evidence; HLC stamps correlate cross-process incidents.

## [01]-[INDEX]

- [02]-[TRIGGER_UNION]: Six capture causes as one sealed union with typed reason payloads.
- [03]-[CAPTURE_PIPELINE]: Window freeze, ordered fan-in, redaction before write, and caps.
- [04]-[MANIFEST_RECEIPT]: Zip assembly, wire manifest, receipt union, retention, and process law.
- [05]-[TS_PROJECTION]: Manifest and receipt wire shapes the TS dashboard ingests.

## [02]-[TRIGGER_UNION]

- Owner: `SupportTrigger` `[Union]` six capture-cause cases.
- Cases: `UserRequested`, `FaultTransition`, `HealthThreshold`, `WatchdogTimeout`, `ExternalCommand`, `Scheduled`.
- Auto: `FaultTransition` carries the wire-stable `FaultRecord` the `Runtime/lifecycle#FAULT_SPINE` `FaultRecord.From` flatten produces — the one fault-to-capture fact `FaultSpine.ArmTraps` emits for every `FaultSource` entry, the live unhandled/unobserved/signalled commits and the `ProbeMarkers` host-crash-marker boot probe alike, so a fault commit and its capture trigger are one fact rather than an untyped capture delegate beside a `PhaseTrigger.FaultCommitted` emission; the case holds `FaultRecord` (kind-discriminated, `Error`-free) so the trigger payload is the exact shape the bundle manifest serializes, never the live `Error`-bearing `FaultSource`; `WatchdogTimeout` fires on a missed heartbeat deadline and `Scheduled` fires from a `ScheduleEntry` row on the schedule port; `ExternalCommand` admits the `ControlService` capture-support verb for service modalities.
- Packages: Thinktecture.Runtime.Extensions, NodaTime, LanguageExt.Core
- Growth: one case lands a new capture cause and breaks the `Facts` dispatch at compile time; a new fault cause is one `FaultSource` case the `FaultRecord.From` flatten and the one `FaultTransition` payload both absorb, never a second trigger case per fault kind — zero new surface.
- Boundary: the private root constructor and deleted value conversion seal ingress; fault, health, and schedule causes carry their typed evidence whole, and rendering happens exactly once inside the total `Facts` dispatch; the `FaultTransition` payload is the wire-stable `FaultRecord` whose `kind` literals (`unhandled`/`unobserved-task`/`posix-signal`/`host-crash-marker`) the `Facts` rendering reads, so the durable-orchestration crash-recovery (`Runtime/orchestration#CRASH_RESUME`) and the bundle manifest read one kind-discriminated fault fact, and a flattened trigger that loses the `FaultRecord` kind fields is the deleted form.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SupportTrigger {
    private SupportTrigger() { }
    public sealed record UserRequested(CorrelationId Correlation, string Reason, Option<Duration> WindowOverride = default) : SupportTrigger;
    public sealed record FaultTransition(CorrelationId Correlation, FaultRecord Fault, Option<Duration> WindowOverride = default) : SupportTrigger;
    public sealed record HealthThreshold(CorrelationId Correlation, DegradationLevel Level, Option<Duration> WindowOverride = default) : SupportTrigger;
    public sealed record WatchdogTimeout(CorrelationId Correlation, ScheduleEntry Entry, Option<Duration> WindowOverride = default) : SupportTrigger;
    public sealed record ExternalCommand(CorrelationId Correlation, string Reason, Option<Duration> WindowOverride = default) : SupportTrigger;
    public sealed record Scheduled(CorrelationId Correlation, ScheduleEntry Entry, Option<Duration> WindowOverride = default) : SupportTrigger;
}

public static class SupportTriggerOps {
    extension(SupportTrigger trigger) {
        public (CorrelationId Correlation, string Kind, string Reason, Option<Duration> Override) Facts() => trigger.Switch(
            userRequested:   static u => (u.Correlation, "user-requested", u.Reason, u.WindowOverride),
            faultTransition: static f => (f.Correlation, "fault-transition", FaultReason(f.Fault), f.WindowOverride),
            healthThreshold: static h => (h.Correlation, "health-threshold", h.Level.ToString(), h.WindowOverride),
            watchdogTimeout: static w => (w.Correlation, "watchdog-timeout", w.Entry.ToString(), w.WindowOverride),
            externalCommand: static e => (e.Correlation, "external-command", e.Reason, e.WindowOverride),
            scheduled:       static s => (s.Correlation, "scheduled", s.Entry.ToString(), s.WindowOverride));

        // Fault reasons carry the FaultRecord kind literal with its wire-stable payload; the
        // manifest's flat reason string preserves the kind-discriminated evidence the [Union] pins.
        static string FaultReason(FaultRecord record) => record.Switch(
            unhandled:       static u => $"unhandled:{(u.Terminating ? "terminating" : "observed")}:{u.Evidence}",
            unobservedTask:  static t => $"unobserved-task:{t.Evidence}",
            signalled:       static s => $"posix-signal:{s.Signal}",
            hostCrashMarker: static h => $"host-crash-marker:{h.Path}");
    }
}
```

## [03]-[CAPTURE_PIPELINE]

- Owner: `SupportCapture` — the window-freeze, ordered fan-in, redact, and cap fold; `SupportArtifact` the contributor factory row; `SupportFault` `[Union]` fault family deriving its codes through `FaultBand.Support`; `DumpPolicy` the dump-completeness policy row carrying the `CensusCap`/`TriageRows`/`FrameCap` walk bounds; `DumpTriage` the ClrMD post-capture fold projecting the captured dump into bounded heap-sample, thread, and root rows; `SupportPolicy` and `SupportRuntime` the bound capture context.
- Entry: `Capture(SupportRuntime runtime, SupportTrigger trigger, ILatencyContext latency)` returns `IO<SupportReceipt>` — `IO` carries the freeze-fan-redact-cap-bundle effect, and the context is the capture phase's own ledger seat, minted through `LatencySpine.Open` at the composition root and marked at each phase boundary.
- Auto: `IncidentBuffers.Flush` replays both held scopes into the frozen window before contributor fan-in and counts the scopes it drained; the `DeadlineClass.SupportWindow` row bounds the capture run on the cancel spine.
- Receipt: per-artifact written bytes, truncated bytes, redaction counts, and the archive content key land as `SupportManifest.Entry` rows.
- Packages: Rasm (kernel `Dimension`; `ContentHash.Of` from `Rasm/Domain/identity#CONTENT_KEY`; `InstrumentTally`/`InstrumentReading`/`ReadingCell` from `Rasm/Domain/telemetry#INSTRUMENT_MECHANISM`), Microsoft.Diagnostics.NETCore.Client, Microsoft.Diagnostics.Runtime, Microsoft.Diagnostics.Tracing.TraceEvent, Microsoft.Extensions.Telemetry.Abstractions, Microsoft.Extensions.Compliance.Redaction, Microsoft.Extensions.Configuration, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: one `SupportArtifact` factory row lands a new contributor; a new dump completeness is one `DumpPolicy` value (`Snapshot` dumpless routine, `Triage` file routine, `WithHeap`/`Full` escalation-only), a new process-image source is one `DumpSource` row carrying its `DataTarget` factory and its `WritesFile` column, and a triage-depth retune is one `CensusCap`/`TriageRows`/`FrameCap` value; a new triage dimension is one `DumpTriage` row family; a new fault is one `SupportFault` case; zero new surface.
- Boundary: the `Active` cell is the coalesce gate — a trigger arriving mid-capture folds to `SupportReceipt.Coalesced` and never opens a second window; classification resolves redaction at row registration, so `Produce` returns only redacted bytes with their redaction count and no unredacted classified byte reaches assembly; every contributor row runs under its own recovery arm — a faulting `Produce` converts to a zero-byte `SupportFault.ContributorFaulted` manifest entry, so the bundle exports partial with the fault named on its row; every written row's manifest identity mints through the kernel `ContentHash.Of` over the CAPPED, already-redacted slice — the exact bytes its zip member carries — so the archive de-duplicates and verifies against the payload a reader extracts rather than one the redactor and the cap both moved, while a contributor fault, a cleanup refusal, and a bundle-cap drop each write no bytes and therefore carry no key at all, absence spelled by the empty `Option` because a digest over the empty span is a real key naming a payload the archive never held; `SupportArtifact.Cleanup` is the optional custody row, and `Assemble` brackets the whole contributor fan then folds every cleanup before bundle sealing, so cancellation, a skipped dependent row, or analysis failure cannot bypass staging cleanup and a cleanup refusal becomes a zero-byte `SupportFault.CleanupFaulted` manifest row; `ReleaseDump` owns every raw-dump delete, and eager release suppresses its refusal while the outer custody row reports that same refusal without replacing a capture or analysis fault; the `EffectiveConfig` row passes the `GetDebugView(Func<ConfigurationDebugViewContext, string>?)` per-value processor through the resolved `Redactor` so each provider value redacts at its origin from the `ConfigurationDebugViewContext.Value` through the one masking owner every redacted column shares, whose count rises on each entry the redactor CHANGED — a length-preserving HMAC or fixed-width fill masks in place, so a length comparison reports zero over a fully masked bundle — carrying no unredacted secret; the `ProcessDump` row composes `Microsoft.Diagnostics.NETCore.Client` — `DiagnosticsClient.WriteDumpAsync(DumpType, path, WriteDumpFlags, CancellationToken)` captures under the frozen window observing the `DeadlineClass.SupportWindow` token, so the declared bound binds through the largest artifact instead of expiring around a synchronous write no token reaches, with completeness as `DumpPolicy` row data; it materializes the manifest bytes and registers the raw path's cleanup independently of `DumpAnalysis`, and it is `None` on a `DumpSource.Snapshot` policy so the raw-dump artifact and its whole custody surface leave that path rather than being written and immediately deleted; `DumpAnalysis` retains a local `finally` as the eager release after ClrMD consumption where a file exists while the outer custody row remains the guaranteed release; `DumpAnalysis` folds through `Microsoft.Diagnostics.Runtime` — the policy's own `DumpSource.Open` resolves the image (`DataTarget.LoadDump(string filePath, DataTargetOptions? options = null)` for a captured file, `DataTarget.CreateSnapshotAndAttach(int processId, DataTargetOptions? options = null)` for the process fork a dumpless triage walks live), `DataTarget.ClrVersions[0].CreateRuntime()` materializes the `ClrRuntime`, `ClrHeap.EnumerateObjects` samples at most `CensusCap` objects before grouping by `ClrObject.Type?.Name` and summing shallow `ClrObject.Size`, `ClrRuntime.Threads` projects `OSThreadId`/`ManagedThreadId`/`GCMode`/`State` with `ClrThread.CurrentException?.Type?.Name` and the `EnumerateStackTrace(includeContext, maxFrames)`-bounded frame walk discriminated on `ClrStackFrameKind.ManagedMethod` versus the runtime `FrameName`, and `ClrHeap.EnumerateRoots` samples at most `CensusCap` roots before counting `ClrRoot.RootKind`; `CensusCap`, `TriageRows`, and `FrameCap` bound every enumeration and output family; the `EventTrace` row hands `EventPipeSession.EventStream` to `Microsoft.Diagnostics.Tracing.TraceEvent`'s `EventPipeEventSource(Stream).Process()` on a dedicated pump inside the `DeadlineClass.SupportWindow` bound, and the admitted `Dimension` supplies both `circularBufferMB` and the artifact estimate so runtime buffering and bundle accounting cannot drift; decode faults map to `SupportFault.DecodeFaulted` and land `SupportReceipt`-partial rather than aborting the bundle; the `.gcdump` heap graph has no reader in the admitted TraceEvent assembly, so the gcdump column binds the `dotnet-gcdump` tool boundary; `PerfMapLease` brackets perf-map emission around a profiled window — `EnablePerfMap(PerfMapType)` at open, `DisablePerfMap()` at disposal — so continuous-profiling and benchmark flame graphs resolve jitted native frames; the `SignalReadings` row is the capture's MEASUREMENT evidence and reads the kernel `InstrumentTally` alone — a bundle is pulled exactly when the exporter, collector, or store is what failed, so the read plane that answers it composes no exporter and no store, the tally's own lifetime and arming stay the composition root's (this row receives it, never opens it), and a tally refusal rides the standing contributor recovery arm as a named zero-byte entry rather than a second fault path.

```csharp signature
public sealed record SupportArtifact(
    string Name,
    DataClassification Classification,
    long EstimatedBytes,
    Func<Interval, IO<(ReadOnlyMemory<byte> Bytes, int Redactions)>> Produce,
    Option<Func<Fin<Unit>>> Cleanup = default) {
    public static SupportArtifact EffectiveConfig(IConfigurationRoot root, Redactor redactor) => new(
        Name: "effective-config",
        Classification: DataClassification.Operational,
        EstimatedBytes: 64 << 10,
        Produce: _ => IO.lift(() => {
            var redactions = 0;
            var view = root.GetDebugView(entry => Masked(redactor, entry.Value, ref redactions));
            return (new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(view)), redactions);
        }));

    // Backend-free measurement evidence: a bundle is pulled exactly when the exporter, collector, or store is
    // what failed, so a capture answering only stacks, heap, and config leaves the reader to guess what the
    // process was measuring. Kernel tallies own the read plane whole, and a refusal rides this row's own
    // recovery arm, so an unarmed tally lands a named zero-byte entry rather than an absent artifact.
    public static SupportArtifact SignalReadings(InstrumentTally tally, Redactor redactor) => new(
        Name: "signal-readings",
        Classification: DataClassification.HostIdentity,
        EstimatedBytes: 256 << 10,
        Produce: _ => IO.lift(tally.Read).Bind(readings => readings.Match(
            Succ: rows => IO.pure(Rendered(rows, redactor)),
            Fail: fault => IO.fail<(ReadOnlyMemory<byte> Bytes, int Redactions)>(
                (Error)new SupportFault.ContributorFaulted("signal-readings", fault.Message)))));

    // One line per measured series under its declaring row's name, kind, and unit. Tag VALUES carry the tenant
    // slug this row is classified for, so redaction runs over values alone and key spellings — already declared
    // as the row's own `Dimensions` — stay readable; rows the process never measured print `unmeasured` rather
    // than a zero, so a quiet producer and a dead one stay distinguishable.
    static (ReadOnlyMemory<byte> Bytes, int Redactions) Rendered(Seq<InstrumentReading> readings, Redactor redactor) {
        var redactions = 0;
        var sink = new StringBuilder();
        foreach (InstrumentReading reading in readings) {
            string head = $"{reading.Row.Name} {reading.Row.Kind.Key} {reading.Row.Unit}";
            if (reading.Cells.IsEmpty) { sink.AppendLine($"{head} unmeasured"); continue; }
            foreach (ReadingCell cell in reading.Cells) {
                sink.Append(head);
                foreach (KeyValuePair<string, object?> tag in cell.Tags) sink.Append(' ').Append(tag.Key).Append('=').Append(Masked(redactor, tag.Value, ref redactions));
                sink.AppendLine($" count={cell.Count} sum={cell.Sum:R} min={cell.Min:R} max={cell.Max:R} last={cell.Last:R}");
            }
        }
        return (new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(sink.ToString())), redactions);
    }

    // Lifecycle receipts inside the frozen window: the phase path a bundle is read for is exactly the one the
    // incident walked, so the window supplier — never a page-local receipt store — bounds the set and a capture
    // outside any transition writes an empty artifact rather than the whole process history.
    public static SupportArtifact PhaseReceipts(
        Func<Interval, Seq<PhaseReceipt>> window, JsonTypeInfo<ImmutableArray<PhaseReceipt>> contract) => new(
        Name: "phase-receipts",
        Classification: DataClassification.Operational,
        EstimatedBytes: 32 << 10,
        // The wire collection is the same immutable shape the manifest's own entries cross as, so one
        // serializer contract family covers the archive and no rail-carrier type reaches a source-generated row.
        Produce: frozen => IO.lift(() =>
            (new ReadOnlyMemory<byte>(JsonSerializer.SerializeToUtf8Bytes([.. window(frozen)], contract)), 0)));

    // The health fold's own coherent reading — snapshot and the level it produced in one value — so a bundle
    // never pairs a fresh level against a stale snapshot, which is the split the cell's single-swap commit
    // exists to foreclose and which a two-read artifact would reintroduce at the one place it is read as truth.
    // The member is named for the READING rather than the snapshot, so it never captures the `HealthSnapshot`
    // type name inside this declaring type.
    public static SupportArtifact HealthReading(DegradationCell cell, JsonTypeInfo<DegradationReading> contract) => new(
        Name: "health-reading",
        Classification: DataClassification.HostIdentity,
        EstimatedBytes: 16 << 10,
        Produce: _ => IO.lift(() =>
            (new ReadOnlyMemory<byte>(JsonSerializer.SerializeToUtf8Bytes(cell.Read(), contract)), 0)));

    // ONE masking owner for every redacted column, so the config view and the readings render cannot disagree on
    // what counts, and it routes through the branch's own `RedactedText` seam rather than the bare package call —
    // a second redaction spelling beside the declared egress read is exactly the split that seam owns. The count
    // rises on a CONTENT change: a length-preserving redactor — an HMAC token, a fixed-width fill — replaces the
    // value byte-for-byte, and a length comparison then reports zero redactions over a fully masked bundle.
    // Absent and empty values mask to the empty string, so no column carries a null.
    static string Masked(Redactor redactor, object? value, ref int redactions) {
        if (value?.ToString() is not { Length: > 0 } text) { return string.Empty; }
        (string masked, bool changed) = RedactedText.Changed(redactor, text);
        if (changed) { redactions++; }
        return masked;
    }

    // Dump admission composes the CANCELLABLE WriteDumpAsync under the frozen window with completeness as row
    // policy; the deadline the SupportWindow row declares now binds THROUGH the write rather than expiring
    // around it, because the synchronous WriteDump holds the capture thread for the whole 512 MiB serialization
    // with no token to observe — the largest artifact was the one place the declared bound did not apply.
    // A capture-tool fault is the typed registry-banded case, never a bare Error.New and never an orphan code
    // outside every band. NONE on a snapshot policy: no file is written, so no raw-dump row and no custody
    // cleanup exist on that path at all.
    public static Option<SupportArtifact> ProcessDump(DumpPolicy policy, string captureRoot) =>
        policy.Source.WritesFile
            ? Some(new SupportArtifact(
                Name: "process-dump",
                Classification: DataClassification.HostIdentity,
                EstimatedBytes: policy.EstimatedBytes,
                Produce: _ => IO.liftAsync(async envIO => {
                    var path = DumpPath(captureRoot);
                    try {
                        await new DiagnosticsClient(Environment.ProcessId)
                            .WriteDumpAsync(policy.Kind, path, policy.Flags, envIO.Token).ConfigureAwait(false);
                        return (new ReadOnlyMemory<byte>(await File.ReadAllBytesAsync(path, envIO.Token).ConfigureAwait(false)), 0);
                    } catch {
                        ignore(ReleaseDump(captureRoot));
                        throw;
                    }
                }).MapFail(static error => (Error)new SupportFault.DumpRejected(error.Message)),
                Cleanup: Some<Func<Fin<Unit>>>(() => ReleaseDump(captureRoot))))
            : None;

    // Dump triage folds the process image through ClrMD into bounded typed rows serialized as the bundle's
    // ANALYZED evidence — top heap types by shallow object bytes, per-thread managed stacks with in-flight
    // exceptions, root census — so first response reads diagnosis from the bundle alone. The image comes from
    // the policy's OWN DumpSource row: a Snapshot policy forks the process and walks it live, so the highest-
    // frequency capture causes (a WatchdogTimeout, a HealthThreshold breach) produce the same rows with no file
    // written, no artifact-cap pressure, and no custody row — the release runs only where a file exists.
    public static SupportArtifact DumpAnalysis(DumpPolicy policy, string captureRoot, JsonTypeInfo<DumpTriage> contract) => new(
        Name: "dump-triage",
        Classification: DataClassification.HostIdentity,
        EstimatedBytes: 256L << 10,
        Produce: _ => IO.lift(() => {
            var path = DumpPath(captureRoot);
            try {
                return (new ReadOnlyMemory<byte>(JsonSerializer.SerializeToUtf8Bytes(DumpTriage.Walk(path, policy), contract)), 0);
            } finally {
                if (policy.Source.WritesFile) { ignore(ReleaseDump(captureRoot)); }
            }
        }).MapFail(static error => (Error)new SupportFault.DumpRejected(error.Message)));

    static string DumpPath(string captureRoot) => Path.Join(captureRoot, $"dump-{Environment.ProcessId}.dmp");

    static Fin<Unit> ReleaseDump(string captureRoot) => Try.lift(() => {
        string path = DumpPath(captureRoot);
        if (File.Exists(path)) File.Delete(path);
        return unit;
    }).Run();

    // Event-stream capture decodes an EventPipe session through TraceEvent's EventPipeEventSource on a
    // dedicated pump; records clone before retention and a decode fault lands the bundle PARTIAL.
    public static SupportArtifact EventTrace(Seq<EventPipeProvider> providers, Duration window, Dimension circularBufferMiB) => new(
        Name: "event-trace",
        Classification: DataClassification.Operational,
        EstimatedBytes: (long)circularBufferMiB.Value << 20,
        Produce: _ => IO.lift(() => {
            using var session = new DiagnosticsClient(Environment.ProcessId).StartEventPipeSession(
                [.. providers], requestRundown: false, circularBufferMB: circularBufferMiB.Value);
            var sink = new StringBuilder();
            var source = new EventPipeEventSource(session.EventStream);
            source.Dynamic.All += evt => sink.AppendLine($"{evt.TimeStamp:O} {evt.ProviderName}/{evt.EventName}");
            ignore(Task.Delay(window.ToTimeSpan()).ContinueWith(_ => session.Stop()));
            source.Process();
            return (new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(sink.ToString())), 0);
        }).MapFail(static error => (Error)new SupportFault.DecodeFaulted(error.Message)));
}

// WHERE the triage walk gets its process image, carrying its own DataTarget factory: File opens a captured
// minidump read-only, Snapshot forks the live process and attaches to the copy — the only image source that
// needs no file on disk. WritesFile is what the capture pipeline reads to decide whether a raw-dump artifact
// and its custody row exist at all, so "cheap triage" is one row rather than a second pipeline beside the first.
[SmartEnum<string>]
public sealed partial class DumpSource {
    public static readonly DumpSource File = new("file", writesFile: true, static path => DataTarget.LoadDump(path));
    public static readonly DumpSource Snapshot = new("snapshot", writesFile: false, static _ => DataTarget.CreateSnapshotAndAttach(Environment.ProcessId));

    public bool WritesFile { get; }

    [UseDelegateFromConstructor]
    public partial DataTarget Open(string dumpPath);
}

// Dump completeness, image source, and walk breadth are policy data; every enumeration consumes a bound.
// Snapshot is the ROUTINE row: a watchdog timeout or a health-threshold breach fires it freely because it
// costs a process fork rather than a 512 MiB write, and File-sourced rows stay for the escalations where the
// raw dump IS the deliverable a support engineer opens elsewhere.
public sealed record DumpPolicy(DumpType Kind, WriteDumpFlags Flags, long EstimatedBytes, int CensusCap, int TriageRows, int FrameCap, DumpSource Source) {
    public static readonly DumpPolicy Snapshot = new(DumpType.Triage, WriteDumpFlags.None, 0L, CensusCap: 250_000, TriageRows: 32, FrameCap: 64, DumpSource.Snapshot);
    public static readonly DumpPolicy Routine = new(DumpType.Triage, WriteDumpFlags.None, 64L << 20, CensusCap: 250_000, TriageRows: 32, FrameCap: 64, DumpSource.File);
    public static readonly DumpPolicy Escalated = new(DumpType.WithHeap, WriteDumpFlags.None, 512L << 20, CensusCap: 2_000_000, TriageRows: 64, FrameCap: 128, DumpSource.File);
}

// ClrMD projects a bounded heap sample, thread census, and root sample into typed rows. Raw dump
// custody stays on its manifest row; this fold makes no retained-size or leak-causality claim.
public sealed record DumpTriage(
    ImmutableArray<DumpTriage.HeapRow> HeapSample,
    ImmutableArray<DumpTriage.ThreadRow> Threads,
    ImmutableArray<DumpTriage.RootRow> Roots) {
    public readonly record struct HeapRow(string Type, long Count, long ShallowBytes);
    public readonly record struct ThreadRow(uint OsId, int ManagedId, string GcMode, string State, ImmutableArray<string> Frames, Option<string> Exception = default);
    public readonly record struct RootRow(string Kind, long Count);

    public static DumpTriage Walk(string dumpPath, DumpPolicy policy) {
        using DataTarget target = policy.Source.Open(dumpPath);
        using ClrRuntime runtime = target.ClrVersions[0].CreateRuntime();
        return new(
            HeapSample: [.. runtime.Heap.EnumerateObjects()
                .Take(policy.CensusCap)
                .GroupBy(static row => row.Type?.Name ?? "<free>")
                .Select(static group => new HeapRow(group.Key, group.LongCount(), group.Sum(static row => (long)row.Size)))
                .OrderByDescending(static row => row.ShallowBytes)
                .Take(policy.TriageRows)],
            Threads: [.. runtime.Threads
                .Take(policy.TriageRows)
                .Select(thread => new ThreadRow(
                    thread.OSThreadId,
                    thread.ManagedThreadId,
                    thread.GCMode.ToString(),
                    thread.State.ToString(),
                    [.. thread.EnumerateStackTrace(includeContext: false, maxFrames: policy.FrameCap)
                        .Select(static frame => frame.Kind == ClrStackFrameKind.ManagedMethod ? frame.Method?.Name ?? "<method>" : frame.FrameName ?? "<runtime>")],
                    Optional(thread.CurrentException?.Type?.Name)))],
            Roots: [.. runtime.Heap.EnumerateRoots()
                .Take(policy.CensusCap)
                .CountBy(static root => root.RootKind.ToString())
                .Select(static pair => new RootRow(pair.Key, pair.Value))]);
    }
}

// Native-symbol lease: perf-map emission spans exactly the profiled window so sample and eBPF
// profilers resolve jitted frames; the kind row is caller policy from the verified PerfMapType
// cases (None, All, JitDump, PerfMap), disposal always disables.
public sealed record PerfMapLease(DiagnosticsClient Client) : IDisposable {
    public static PerfMapLease Open(PerfMapType kind) {
        var client = new DiagnosticsClient(Environment.ProcessId);
        client.EnablePerfMap(kind);
        return new(client);
    }

    public void Dispose() => Client.DisablePerfMap();
}

[Union]
public abstract partial record SupportFault : Expected, IValidationError<SupportFault> {
    private SupportFault(string detail, int code) : base(detail, code, None) { }
    public static SupportFault Create(string message) => new Text(message);
    public sealed record Text : SupportFault { public Text(string detail) : base(detail, FaultBand.Support.Code(0)) { } }
    public sealed record DumpRejected : SupportFault { public DumpRejected(string detail) : base(detail, FaultBand.Support.Code(1)) { } }
    public sealed record DecodeFaulted : SupportFault { public DecodeFaulted(string detail) : base(detail, FaultBand.Support.Code(2)) { } }
    public sealed record ContributorFaulted : SupportFault { public ContributorFaulted(string artifact, string detail) : base($"{artifact}: {detail}", FaultBand.Support.Code(3)) { } }
    public sealed record CleanupFaulted : SupportFault { public CleanupFaulted(string artifact, string detail) : base($"{artifact}: {detail}", FaultBand.Support.Code(4)) { } }
}

public sealed record SupportPolicy(
    Duration Lookback,
    Duration Settle,
    long ArtifactCapBytes,
    long BundleCapBytes,
    int MaxBundles,
    Duration MaxAge) {
    // ONE place the artifact cut falls. The manifest's byte count, the content key's preimage, and the zip
    // member are three reads of this projection, so a second `long.Min` spelling at any of them is how the
    // count, the key, and the archived bytes start describing three different slices of one payload.
    public ReadOnlyMemory<byte> Cap(ReadOnlyMemory<byte> payload) => payload[..(int)long.Min(payload.Length, ArtifactCapBytes)];
}

// `Buffer` is the two-scope hold owner rather than the raw process ring: the capture's own flush and the fault
// transition's flush are one replay plane, so a capture reaching past the record to `GlobalLogBuffer` would
// leave the operation ring held exactly where an incident is being assembled. `Signals` is the mounted set the
// flush writes its scope count through, `Phase` the checkpoint token `LatencySpine.Open` resolved once at
// composition so no capture resolves a name of its own, and `Phases` the lifecycle's own window read the
// phase-receipt artifact projects — a supplier column the composition fills, never a receipt store this page opens.
public sealed record SupportRuntime(
    SupportPolicy Policy,
    ConsumptionProfile Profile,
    string StorageRoot,
    ImmutableDictionary<string, string> Versions,
    ClockPolicy Clocks,
    IncidentBuffers Buffer,
    InstrumentSet Signals,
    CheckpointToken Phase,
    Func<Interval, Seq<PhaseReceipt>> Phases,
    JsonTypeInfo<SupportManifest> ManifestContract,
    Seq<SupportArtifact> Contributors,
    Atom<Option<CorrelationId>> Active);

public static class SupportCapture {
    // The latency context threads as a PARAMETER rather than a `SupportRuntime` column: the runtime record is
    // composition-lifetime while the ledger is one capture's, and a pooled context seated on a value that
    // outlives the operation returns to its pool with a later capture still marking through it.
    public static IO<SupportReceipt> Capture(SupportRuntime runtime, SupportTrigger trigger, ILatencyContext latency) =>
        from facts in IO.pure(trigger.Facts())
        from receipt in IO.lift(() => runtime.Active.Swap(gate => gate.IsNone ? Optional(facts.Correlation) : gate))
            .Bracket(
                Use: gate => gate is { IsSome: true, Case: CorrelationId owner } && owner == facts.Correlation
                    ? Assemble(runtime, facts, latency)
                    // A coalesced trigger marks NO checkpoint: the phase it would stamp belongs to the capture
                    // already running, and a second mark on one context is the re-entrant record the ledger
                    // refuses by contract.
                    : IO.pure<SupportReceipt>(new SupportReceipt.Coalesced(gate.IfNone(facts.Correlation), facts.Kind)),
                Fin: _ => IO.lift(() => ignore(runtime.Active.Swap(gate =>
                    gate is { IsSome: true, Case: CorrelationId owner } && owner == facts.Correlation
                        ? Option<CorrelationId>.None
                        : gate))))
        select receipt;

    static IO<SupportReceipt> Assemble(
        SupportRuntime runtime,
        (CorrelationId Correlation, string Kind, string Reason, Option<Duration> Override) facts,
        ILatencyContext latency) =>
        from at in IO.lift(() => runtime.Clocks.Now)
        from mark in IO.lift(runtime.Clocks.Mark)
        let opened = at - facts.Override.IfNone(runtime.Policy.Lookback)
        let closed = at + runtime.Policy.Settle
        let window = new Interval(opened, closed)
        // The capture boundary stamps once, AFTER the window freezes and before the fan opens, so the phase the
        // ledger reports is the contributor fan and not the clock read that preceded it.
        from _ in IO.lift(() => LatencySpine.Mark(latency, runtime.Phase))
        // The flush's own scope-count write rides its rail and never gates the capture: an incident bundle is
        // the evidence of the failure being assembled, so a refused counter is not a reason to withhold it.
        from held in IO.lift(() => runtime.Buffer.Flush(runtime.Signals))
        let cleanup = Atom(Seq<(SupportManifest.Entry Entry, ReadOnlyMemory<byte> Bytes)>())
        // Per-row recovery is the partial-receipt fold: a faulting contributor lands a zero-byte
        // ContributorFaulted entry and the bundle exports partial — one row never aborts the capture.
        from produced in IO.pure(unit).Bracket(
            Use: _ => runtime.Contributors
                .TraverseM(row => (row.Produce(window).Map(payload => Written(row, payload, runtime.Policy))
                    | @catch<IO, (SupportManifest.Entry Entry, ReadOnlyMemory<byte> Bytes)>(static _ => true,
                        error => IO.pure(Faulted(row.Name, row.Classification,
                            new SupportFault.ContributorFaulted(row.Name, error.Message))))).As())
                .As(),
            Fin: _ => IO.lift(() => ignore(cleanup.Swap(_ => runtime.Contributors.Fold(
                Seq<(SupportManifest.Entry Entry, ReadOnlyMemory<byte> Bytes)>(),
                (faults, row) => row.Cleanup.Match(
                    None: () => faults,
                    Some: release => Try.lift(release).Run().Bind(static outcome => outcome).Match(
                        Succ: _ => faults,
                        Fail: error => faults.Add(Faulted(
                            $"{row.Name}-cleanup", row.Classification,
                            new SupportFault.CleanupFaulted(row.Name, error.Message))))))))))
        let rows = Capped(produced + cleanup.Value, runtime.Policy)
        from receipt in SupportLedger.Bundle(runtime, SupportManifest.From(facts, opened, closed, rows, runtime), rows, mark)
        select receipt;

    // ONE refusal arm for both no-byte causes — a faulting contributor and a refused cleanup — so the row that
    // wrote nothing carries no content key in exactly one place. The name is a parameter rather than a second
    // near-identical mint, since the cleanup row's `-cleanup` slot is the only column the two causes disagree on.
    static (SupportManifest.Entry Entry, ReadOnlyMemory<byte> Bytes) Faulted(
        string name,
        DataClassification classification,
        SupportFault fault) =>
        (new SupportManifest.Entry(name, classification.ToString(), Bytes: 0L, TruncatedBytes: 0L, Redactions: 0,
             ContentKey: None, Fault: Some(fault.Message)),
         ReadOnlyMemory<byte>.Empty);

    // The archive identity mints over the FINAL slice: `Produce` already returned redacted bytes, and the
    // artifact cap decides how many of them the zip member carries, so a key over the pre-redaction or pre-cap
    // payload names bytes no reader can extract from the archive it is written into. `SupportPolicy.Cap` is the
    // one place the cut falls, so the counted length, the keyed preimage, and the written member cannot diverge.
    static (SupportManifest.Entry Entry, ReadOnlyMemory<byte> Bytes) Written(
        SupportArtifact row,
        (ReadOnlyMemory<byte> Bytes, int Redactions) payload,
        SupportPolicy policy) =>
        (new SupportManifest.Entry(
             Name: row.Name,
             Classification: row.Classification.ToString(),
             Bytes: policy.Cap(payload.Bytes).Length,
             TruncatedBytes: long.Max(payload.Bytes.Length - policy.ArtifactCapBytes, 0L),
             Redactions: payload.Redactions,
             ContentKey: Some(ContentHash.Of(policy.Cap(payload.Bytes).Span).ToString("x32"))),
         policy.Cap(payload.Bytes));

    static Seq<(SupportManifest.Entry Entry, ReadOnlyMemory<byte> Bytes)> Capped(
        Seq<(SupportManifest.Entry Entry, ReadOnlyMemory<byte> Bytes)> produced,
        SupportPolicy policy) =>
        produced.Fold(
            (Total: 0L, Rows: Seq<(SupportManifest.Entry Entry, ReadOnlyMemory<byte> Bytes)>()),
            // A row past the bundle budget is dropped WHOLE — its zip member carries zero bytes — so the
            // content key clears with the byte count. A key retained across the drop names a payload the
            // archive does not hold, which is the same forged identity a digest over the empty span would be.
            (acc, row) => acc.Total + row.Bytes.Length > policy.BundleCapBytes
                ? (acc.Total, acc.Rows.Add((row.Entry with { Bytes = 0L, TruncatedBytes = row.Entry.Bytes, ContentKey = None }, ReadOnlyMemory<byte>.Empty)))
                : (acc.Total + row.Bytes.Length, acc.Rows.Add(row)))
            .Rows;
}
```

Policy rows bind through the config rail: rows one through six freeze into the SupportPolicy record and the retention-sweep-cadence row binds the Sweep ScheduleEntry registration; every capture and retention literal traces to this table:

| [INDEX] | [POLICY]                |    [VALUE] | [RELOAD_CLASS] |
| :-----: | :---------------------- | ---------: | :------------- |
|  [01]   | window-lookback         | 10 minutes | transition     |
|  [02]   | window-settle           | 30 seconds | transition     |
|  [03]   | artifact-cap            |     16 MiB | transition     |
|  [04]   | bundle-cap              |    128 MiB | transition     |
|  [05]   | retention-max-bundles   |         16 | transition     |
|  [06]   | retention-max-age       |    30 days | transition     |
|  [07]   | retention-sweep-cadence |   `@daily` | transition     |

Every row names its `SupportArtifact` factory, so the table and the fence carry one roster and a row without a factory is a name the bundle never writes. The gcdump heap graph rides the `dotnet-gcdump` tool boundary, since the admitted TraceEvent assembly ships no reader for it. The held log records are deliberately absent: `IncidentBuffers.Flush` replays them into the LIVE pipeline inside the frozen window rather than into the zip, so they land wherever this profile's log pipeline delivers and a second copy in the archive would be the same records under two retention policies. Sibling packages add rows through ordered contributor descriptors.

| [INDEX] | [ARTIFACT]       | [FACTORY]         | [PRODUCER]                                                            |
| :-----: | :--------------- | :---------------- | :-------------------------------------------------------------------- |
|  [01]   | effective-config | `EffectiveConfig` | redacted configuration debug view                                     |
|  [02]   | signal-readings  | `SignalReadings`  | kernel `InstrumentTally` rows, backend-free                           |
|  [03]   | phase-receipts   | `PhaseReceipts`   | lifecycle receipts the runtime's window supplier returns              |
|  [04]   | health-reading   | `HealthReading`   | the degradation cell's own coherent reading                           |
|  [05]   | process-dump     | `ProcessDump`     | `DiagnosticsClient.WriteDumpAsync` under a file-sourced `DumpPolicy`  |
|  [06]   | dump-triage      | `DumpAnalysis`    | ClrMD `DumpTriage.Walk` rows over the policy's own `DumpSource` image |
|  [07]   | event-trace      | `EventTrace`      | EventPipe session decoded through TraceEvent `EventPipeEventSource`   |

## [04]-[MANIFEST_RECEIPT]

- Owner: `SupportManifest` the wire-neutral manifest; `SupportReceipt` `[Union]` the wire receipt family; `SupportCaptureWire` the flattened export projection the TS census decodes; `SupportLedger` the zip-assembly and retention surface.
- Cases: `Exported`, `Coalesced`, `Evicted`.
- Entry: `Sweep(SupportRuntime runtime)` returns `IO<SupportReceipt>` — `IO` carries the retention-eviction effect.
- Auto: `Sweep` registers as one `ScheduleEntry` row carrying the retention-sweep-cadence value; eviction emits `SupportReceipt.Evicted` into the receipt rail with bundle and byte counts.
- Receipt: `SupportReceipt` is the wire receipt family; the kind discriminator is pinned by `JsonPolymorphic` metadata on the union root.
- Packages: Thinktecture.Runtime.Extensions, NodaTime, BCL inbox
- Growth: one policy value retunes caps or retention; one case extends `SupportReceipt` and breaks every consumer arm at compile time — zero new surface.
- Boundary: `Bundle` and `Evict` are the named `System.IO` boundary capsules and carry statement bodies; a bundle captures exactly one process's evidence — cross-process incidents correlate by HLC stamp at the evidence layer, and contributor requests never cross the UDS hop; an exported bundle crosses the receipt rail under `InstrumentFan.SupportKind` and its manifest entries are what the redaction counter partitions on, so the archive's own per-entry mask count is the branch's ONE measured redaction population and no second tally forms beside it; each entry's `ContentKey` is the archive identity every consumer keys on — a bundle de-duplicates against a store, a re-upload proves untampered, and an extracted member verifies against the manifest without re-reading the zip, so archive-shape proof pins bundle identity off this column rather than a re-hash of the file; the AppUi `Rasm.AppUi/Document/export#EXPORT_DESTINATIONS` `BundleMember.ContentKey` is the PRE-redaction identity of the contributed payload and the two keys agree exactly where nothing was masked or truncated, so an inequality names redaction or a cap rather than corruption and neither key substitutes for the other; the source-generated `ManifestContract` carries `Entry` whole, so the column rides the one manifest serializer and reaches `SupportCaptureWire.Entries` verbatim with no second wire row; `SupportCaptureWire` is the flattened export projection the corpus registers at `tests/contracts/MANIFEST.md` `[02.21]-[APPHOST_WIRE]` — the family the TS census decodes carries the bundle FACTS a dashboard reads (trigger, correlation, window, path, byte totals, entry roster) and never the receipt union, because a coalesced or evicted receipt names no bundle and a decoder branching on a kind discriminant to find three quarters of its fields absent is the shape a flat projection deletes.

```csharp signature
public sealed record SupportManifest(
    string Trigger,
    string Reason,
    CorrelationId Correlation,
    Instant WindowStart,
    Instant WindowEnd,
    ConsumptionProfile Profile,
    ImmutableArray<Entry> Entries,
    ImmutableDictionary<string, string> PackageVersions) {
    // `ContentKey` is the archive's identity column — the seed-zero kernel digest over the bytes THIS entry's
    // zip member carries, rendered as the branch's 32-digit lowercase hex because a `UInt128` exceeds the
    // exact-integer range every wire consumer decodes on. The `= default` is the DECODE half of the suite's
    // omission posture and nothing else: `Runtime/ports#WIRE_LAW` drops an absent `Option` member on write, so
    // a parameter with no default would read as wire-required and fail every faulted entry. Every construction
    // still answers the slot explicitly — a row that wrote nothing answers empty rather than falling through.
    public sealed record Entry(string Name, string Classification, long Bytes, long TruncatedBytes, int Redactions,
        Option<string> ContentKey = default, Option<string> Fault = default);

    public int Redactions => Entries.Sum(static entry => entry.Redactions);

    public static SupportManifest From(
        (CorrelationId Correlation, string Kind, string Reason, Option<Duration> Override) facts,
        Instant opened,
        Instant closed,
        Seq<(Entry Entry, ReadOnlyMemory<byte> Bytes)> rows,
        SupportRuntime runtime) =>
        new(
            Trigger: facts.Kind,
            Reason: facts.Reason,
            Correlation: facts.Correlation,
            WindowStart: opened,
            WindowEnd: closed,
            Profile: runtime.Profile,
            Entries: [.. rows.Map(static row => row.Entry)],
            PackageVersions: runtime.Versions);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(SupportReceipt.Exported), "exported")]
[JsonDerivedType(typeof(SupportReceipt.Coalesced), "coalesced")]
[JsonDerivedType(typeof(SupportReceipt.Evicted), "evicted")]
public abstract partial record SupportReceipt {
    private SupportReceipt() { }
    public sealed record Exported(SupportManifest Manifest, string BundlePath, long TotalBytes, Duration Elapsed) : SupportReceipt;
    public sealed record Coalesced(CorrelationId Active, string FoldedKind) : SupportReceipt;
    public sealed record Evicted(int Bundles, long Bytes, Instant At) : SupportReceipt;
}

// The export projection the TS census lands: one FLAT record over the exported case alone, because a bundle
// fact only exists where a bundle was written and a decoder reading a kind discriminant to discover that its
// path, byte total, and entries are all absent has learned nothing the absence of the row would not have told
// it. Entry rows carry through verbatim — the manifest's per-artifact evidence is the whole reason a dashboard
// reads this family rather than the zip.
public readonly record struct SupportCaptureWire(
    string Trigger,
    string Reason,
    CorrelationId Correlation,
    Instant WindowStart,
    Instant WindowEnd,
    string BundlePath,
    long TotalBytes,
    Duration Elapsed,
    int Redactions,
    ImmutableArray<SupportManifest.Entry> Entries) {
    public static SupportCaptureWire Of(SupportReceipt.Exported exported) =>
        new(exported.Manifest.Trigger, exported.Manifest.Reason, exported.Manifest.Correlation,
            exported.Manifest.WindowStart, exported.Manifest.WindowEnd, exported.BundlePath,
            exported.TotalBytes, exported.Elapsed, exported.Manifest.Redactions, exported.Manifest.Entries);
}

public static class SupportLedger {
    public static IO<SupportReceipt> Sweep(SupportRuntime runtime) =>
        IO.lift(() => runtime.Clocks.Now).Bind(at => Evict(runtime, at));

    internal static IO<SupportReceipt> Bundle(
        SupportRuntime runtime,
        SupportManifest manifest,
        Seq<(SupportManifest.Entry Entry, ReadOnlyMemory<byte> Bytes)> rows,
        long mark) =>
        IO.lift(() => {
            string path = Path.Join(runtime.StorageRoot, $"{manifest.Correlation}.zip");
            using (FileStream sink = File.Create(path))
            using (ZipArchive zip = new(sink, ZipArchiveMode.Create)) {
                using (Stream head = zip.CreateEntry("manifest.json").Open()) {
                    JsonSerializer.Serialize(head, manifest, runtime.ManifestContract);
                }
                foreach ((SupportManifest.Entry entry, ReadOnlyMemory<byte> bytes) in rows) {
                    using Stream body = zip.CreateEntry(entry.Name).Open();
                    body.Write(bytes.Span);
                }
            }
            return (SupportReceipt)new SupportReceipt.Exported(manifest, path, new FileInfo(path).Length, runtime.Clocks.Elapsed(mark));
        });

    static IO<SupportReceipt> Evict(SupportRuntime runtime, Instant at) =>
        IO.lift(() => {
            DateTime cutoff = (at - runtime.Policy.MaxAge).ToDateTimeOffset().UtcDateTime;
            FileInfo[] bundles = [.. new DirectoryInfo(runtime.StorageRoot).EnumerateFiles("*.zip").OrderByDescending(static file => file.CreationTimeUtc)];
            (int Bundles, long Bytes) swept = (0, 0L);
            foreach ((int rank, FileInfo file) in bundles.Index()) {
                if (rank >= runtime.Policy.MaxBundles || file.CreationTimeUtc < cutoff) {
                    swept = (swept.Bundles + 1, swept.Bytes + file.Length);
                    file.Delete();
                }
            }
            return (SupportReceipt)new SupportReceipt.Evicted(swept.Bundles, swept.Bytes, at);
        });
}
```

## [05]-[TS_PROJECTION]

- Owner: `SupportManifest`, `SupportReceipt`, and the `SupportCaptureWire` export projection
- Packages: NodaTime.Serialization.SystemTextJson, Thinktecture.Runtime.Extensions.Json, BCL inbox
- Growth: one field row per additive manifest extension, which the flat export projection inherits with no second edit; the TS dashboard tolerates additive fields — zero new surface.
- Boundary: instants and durations serialize as ISO-8601 text through the NodaTime converters; correlation and profile serialize as their keys through the generated Thinktecture converters; the entry content key serializes as its 32-digit lowercase hex projection, which is where the kernel's `UInt128` identity currency crosses into a wire encoding — a numeric slot loses the low bits at every decoder this family reaches — and its `Option` slot OMITS on absence through the suite's `Runtime/ports#WIRE_LAW` contract modifier, so an entry that wrote no bytes carries no key property at all rather than a null one; record property names ride the camelCase wire policy and the receipt kind discriminator is the JsonPolymorphic metadata property.

```typescript signature
type SupportTriggerKind =
  | "user-requested"
  | "fault-transition"
  | "health-threshold"
  | "watchdog-timeout"
  | "external-command"
  | "scheduled";

interface SupportManifestEntry {
  readonly name: string;
  readonly classification: string;
  readonly bytes: number;
  readonly truncatedBytes: number;
  readonly redactions: number;
  // 32 lowercase hex digits, never a number: a UInt128 exceeds this runtime's exact-integer range, so the key
  // crosses as text and compares as text. Omitted exactly where the entry wrote no bytes — a faulted
  // contributor, a refused cleanup, a bundle-cap drop — so presence and a written payload stay one fact here.
  readonly contentKey?: string;
  // Optional-omitted, never null-written: the suite's `OmitAbsent` contract modifier drops an absent `Option`
  // member at the producer, so the decode side reads presence and a null-typed slot spells a byte no mint emits.
  readonly fault?: string;
}

interface SupportManifest {
  readonly trigger: SupportTriggerKind;
  readonly reason: string;
  readonly correlation: string;
  readonly windowStart: string;
  readonly windowEnd: string;
  readonly profile: string;
  readonly entries: readonly SupportManifestEntry[];
  readonly packageVersions: Readonly<Record<string, string>>;
  readonly redactions: number;
}

type SupportReceipt =
  | { readonly kind: "exported"; readonly manifest: SupportManifest; readonly bundlePath: string; readonly totalBytes: number; readonly elapsed: string }
  | { readonly kind: "coalesced"; readonly active: string; readonly foldedKind: SupportTriggerKind }
  | { readonly kind: "evicted"; readonly bundles: number; readonly bytes: number; readonly at: string };

interface SupportCaptureWire {
  readonly trigger: SupportTriggerKind;
  readonly reason: string;
  readonly correlation: string;
  readonly windowStart: string;
  readonly windowEnd: string;
  readonly bundlePath: string;
  readonly totalBytes: number;
  readonly elapsed: string;
  readonly redactions: number;
  readonly entries: readonly SupportManifestEntry[];
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
