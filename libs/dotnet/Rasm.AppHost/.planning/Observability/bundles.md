# [APPHOST_SUPPORT_BUNDLES]

Support capture owns the runtime spine's bounded diagnostic evidence surface: one `SupportTrigger` union admits every cause, and one fold freezes the window, gathers ordered artifact rows, redacts before write, caps, and lands one zip. Capture law owns the trigger vocabulary, artifact roster and contributed ports, dump custody, policy values, archive manifest, and host-local outcomes. Each bundle is process-local evidence; HLC stamps correlate cross-process incidents.

Settled composition: `SupportContributorPort` arrives from Runtime/ports#PORT_SURFACE; `ContentHash.Of`/`.Hex`, `Dimension`, `Masked`, `Cell`, `Transition`, `MonotonicTimeline`, `Op`, and `InstrumentTally`/`InstrumentReading`/`ReadingCell` arrive from the kernel; `FaultCell` and `IsolatedFault` from Rasm/Domain/hooks#HOOK_RAIL; `RedactedText`/`DataClassification` from Observability/telemetry#REDACTION_TAXONOMY; `LatencySpine`/`LatencyCheckpoint` from Observability/telemetry#LATENCY_PLANE; `AppHostMeasure`/`InstrumentSet` from Observability/instruments; `ClockPolicy`/`ScheduleEntry`/`DeadlineClass`/`DeadlineOutcome` from Runtime/time and `GaugedSpan<DeadlineClass>` from the kernel; `PhaseCommit` from Runtime/lifecycle#PHASE_FAMILY; `AppHostMeasure.RedactionTags` and `InstrumentSet` from Observability/instruments; `WatchdogEnrollment` from Runtime/profiles#BOOT_SURFACE; `AdversarialProbe.Drift`/`ChaosDrift`/`ChaosObservation` from Runtime/determinism; `PerfMapLease` from Observability/benchmarks#CAPTURE_SEAM, which is where the profiled-window symbol lease declares and where all three of its consumers live.

## [01]-[INDEX]

- [02]-[TRIGGER_UNION]: Six capture causes as one native vocabulary over four payload cases with the universal columns on the root.
- [03]-[ARTIFACT_ROSTER]: Contributor factory rows, the contributed-port seam, the one masking ledger, and the event-trace pump.
- [04]-[DUMP_CUSTODY]: Image source with its custody column, completeness policy, and the ClrMD triage fold.
- [05]-[CAPTURE_PIPELINE]: Window freeze, coalesce gate, ordered fan-in, cleanup fold, and the bundle cap.
- [06]-[BUNDLE_SEAL]: Zip assembly, the settled outcome family, retention sweep, and process law.

## [02]-[TRIGGER_UNION]

- Owner: `SupportTriggerKind` `[SmartEnum<string>]` carries the six cause keys and dump completeness; `SupportTrigger` `[Union]` has four interior payload cases over the universal correlation and window columns; `TriggerFacts` is the named projection every downstream fold reads.
- Cases: `Requested` covers local and admitted-control requests through its `Origin`; `FaultTransition` carries the runtime's native `FaultSource`; `HealthThreshold` carries the crossed `DegradationLevel`; `Timed` covers heartbeat and scheduled causes through the same `Origin` column.
- Entry: `trigger.Facts()` returns `TriggerFacts` — correlation, kind, rendered reason, and window override in one named value the capture fold, the manifest mapper, and the coalesce outcome all read.
- Law: the kind vocabulary outranks the case split. Two causes sharing a payload shape share one case and name their origin as a column, while all six keys survive as `SupportTriggerKind` rows because lifecycle, scheduling, coalescing, and the archive manifest read those distinctions.
- Auto: `FaultTransition` carries the same native fault fact from `Runtime/lifecycle#FAULT_SPINE` for live faults and boot-marker probes alike, so capture and phase transition derive from one value; `Facts` preserves that value on the manifest while `Reason` remains the short operator label; `Timed` renders the schedule key beside the gauged lane and its `DeadlineOutcome` and never serializes the live work closure.
- Packages: Rasm.Contracts, Thinktecture.Runtime.Extensions, NodaTime, LanguageExt.Core
- Growth: one capture cause is one `SupportTriggerKind` row and, where its payload differs from every standing case, one `SupportTrigger` case whose `Facts` arm the generated `Switch` demands; a new fault cause extends the one `FaultSource` union the `FaultTransition` payload carries; a completeness retune is one `Dump` column value; zero new surface.
- Boundary: the abstract root seals ingress; fault, health, and schedule causes carry typed evidence whole, while the short reason renders once inside total `Facts` dispatch. Durable crash recovery and support capture read the same native fault fact, never a peer protobuf or local mirror. The live `ScheduleEntry.Work` closure stays process-local; only its key and the deadline lane/outcome enter the reason. Watchdog dump completeness remains enrollment-owned.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SupportTriggerKind {
    public static readonly SupportTriggerKind UserRequested = new(
        "user-requested", Some(DumpPolicy.Routine));
    public static readonly SupportTriggerKind FaultTransition = new(
        "fault-transition", Some(DumpPolicy.Routine));
    public static readonly SupportTriggerKind HealthThreshold = new(
        "health-threshold", Some(DumpPolicy.Snapshot));
    public static readonly SupportTriggerKind WatchdogTimeout = new(
        "watchdog-timeout", Option<DumpPolicy>.None);
    public static readonly SupportTriggerKind ExternalCommand = new(
        "external-command", Some(DumpPolicy.Routine));
    public static readonly SupportTriggerKind Scheduled = new(
        "scheduled", Some(DumpPolicy.Snapshot));

    public Option<DumpPolicy> Dump { get; }
}

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SupportTrigger {
    private protected SupportTrigger(CorrelationId correlation, Option<Duration> windowOverride) =>
        (Correlation, WindowOverride) = (correlation, windowOverride);

    public CorrelationId Correlation { get; }
    public Option<Duration> WindowOverride { get; }

    public sealed record Requested(CorrelationId Correlation, SupportTriggerKind Origin, string Reason, Option<Duration> WindowOverride = default)
        : SupportTrigger(Correlation, WindowOverride);
    public sealed record FaultTransition(
        CorrelationId Correlation,
        FaultSource Fault,
        Option<Duration> WindowOverride = default)
        : SupportTrigger(Correlation, WindowOverride);
    public sealed record HealthThreshold(CorrelationId Correlation, DegradationLevel Level, Option<Duration> WindowOverride = default)
        : SupportTrigger(Correlation, WindowOverride);
    public sealed record Timed(CorrelationId Correlation, SupportTriggerKind Origin, ScheduleEntry Entry, GaugedSpan<DeadlineClass> Span, Option<Duration> WindowOverride = default)
        : SupportTrigger(Correlation, WindowOverride);
}

public readonly record struct TriggerFacts(
    CorrelationId Correlation,
    SupportTriggerKind Kind,
    string Reason,
    Option<Duration> Override,
    Option<FaultSource> Fault = default);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SupportTriggerOps {
    extension(SupportTrigger trigger) {
        public TriggerFacts Facts() => trigger.Switch(
            requested:       static row => new TriggerFacts(row.Correlation, row.Origin, row.Reason, row.WindowOverride),
            faultTransition: static row => new TriggerFacts(
                                 row.Correlation, SupportTriggerKind.FaultTransition,
                                 $"fault:{row.Fault.Kind}", row.WindowOverride, Some(row.Fault)),
            healthThreshold: static row => new TriggerFacts(row.Correlation, SupportTriggerKind.HealthThreshold, row.Level.Key, row.WindowOverride),
            timed:           static row => new TriggerFacts(
                                 row.Correlation, row.Origin,
                                 $"{row.Entry.Key}:{row.Span.Lane.Key}:{DeadlineOutcome.Of(row.Span).Key}",
                                 row.WindowOverride));
    }
}
```

## [03]-[ARTIFACT_ROSTER]

- Owner: `SupportArtifact` the contributor factory row; `CaptureWindow` the per-capture context every producer reads; `ArtifactPayload` the produced bytes beside their masking tally; `MaskTally` the redaction monoid and `MaskLedger` the ONE masking owner every redacted column writes through; `TraceSession` the event-trace pump's own acquisition; the contributed-row carrier is `Runtime/ports#PORT_SURFACE` `SupportContributorPort`, which this owner folds and never re-declares.
- Cases: seven standing rows — effective config, signal readings, phase commits, health reading, hook faults, determinism drift, the event trace — beside the two dump rows `[04]` mints; each is a factory over composition-supplied ports and none opens a capture window of its own.
- Entry: each `SupportArtifact.<Row>(…)` returns the factory row; `Runtime/ports#PORT_SURFACE` `SupportContributorPort` carries a package's rows inward; `SupportArtifact.Folded(own, contributed)` is the ONE roster projection the runtime binds; `MaskLedger.Mask(object?)` returns the masked text and records its verdict, `MaskLedger.Tally` reads the accumulated count.
- Law: redaction verdicts are kernel `Masked` cases and the tally is their MONOID fold. Three `ref int` counters threaded through every producer are the deleted form — they let two producers disagree on what counts and made a length-preserving redactor report zero over a fully masked bundle — so one ledger owns the verdict, one tally owns the count, and a producer that writes a byte cannot skip either.
- Auto: `IncidentBuffers.Flush` replays both held scopes into the frozen window before contributor fan-in and counts the scopes it drained; the `DeadlineClass.SupportWindow` row bounds the capture run on the cancel spine; the hook rail's parked subscriber faults drain here rather than through a second retention plane, because `FaultCell` is the bounded custody the rail already keeps and a capture is exactly when a late panel gets read; the determinism campaign's placement fold runs as a contributor, so a pulled bundle carries the drift evidence naming any strategy that swallowed an injection.
- Output: per-artifact written bytes, truncated bytes, redaction counts, and each written member's content key land as `SupportManifest.Entry` rows.
- Packages: Rasm (kernel `Dimension`, `Masked`, `InstrumentTally`/`InstrumentReading`/`ReadingCell`, `FaultCell`), Microsoft.Diagnostics.NETCore.Client, Microsoft.Diagnostics.Tracing.TraceEvent, Microsoft.Extensions.Compliance.Redaction, Microsoft.Extensions.Configuration, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: one `SupportArtifact` factory row lands a new contributor and one inward `SupportContributorPort` lands a whole package's set, which the one `Folded` projection already reads; zero new surface.
- Boundary: classification resolves redaction at row registration, so `Produce` returns only redacted bytes with their tally and no unredacted classified byte reaches assembly; every contributor row runs under its own recovery arm — a faulting `Produce` converts to a zero-byte `SupportFault.ContributorFaulted` manifest entry, so the bundle exports partial with the fault named on its row; `SupportArtifact.Cleanup` is the optional custody row the `[05]` fan folds before bundle sealing; the `EffectiveConfig` row passes the `GetDebugView(Func<ConfigurationDebugViewContext, string>?)` per-value processor through the ledger so each provider value redacts at its origin from the `ConfigurationDebugViewContext.Value`, carrying no unredacted secret — the framework drives that callback itself, which is why the ledger's accumulation seat is one cell inside the owner rather than a `Writer` bind no callback can reach; the `EventTrace` row hands `EventPipeSession.EventStream` to `Microsoft.Diagnostics.Tracing.TraceEvent`'s `EventPipeEventSource(Stream).Process()` on a FORKED pump whose window is the fork's own timeout, so the decode blocks on nothing the capture cannot cancel and a detached `Task.Delay` continuation whose faults nothing observes and whose stop races the dispose is the deleted form, with the admitted `Dimension` supplying both `circularBufferMB` and the artifact estimate so runtime buffering and bundle accounting cannot drift; decode faults map to `SupportFault.DecodeFaulted` and land the bundle partial rather than aborting it; the `.gcdump` heap graph has no reader in the admitted TraceEvent assembly, so the gcdump column binds the `dotnet-gcdump` tool boundary; native-symbol leasing is `Observability/benchmarks#CAPTURE_SEAM` `PerfMapLease`, whose three consumers all live on that page — this owner opens no lease and a declaration here is a symbol seam beside a fan that never brackets a profiled window; the `SignalReadings` row is the capture's MEASUREMENT evidence and reads the kernel `InstrumentTally` alone — a bundle is pulled exactly when the exporter, collector, or store is what failed, so the read plane that answers it composes no exporter and no store, the tally's own lifetime and arming stay the composition root's, and a tally refusal rides the standing contributor recovery arm as a named zero-byte entry rather than a second fault path; contributed ports carry rows and never reach the freeze, redact, or cap law, so a benchmark session lends its event-trace row without opening a second capture window.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public sealed record CaptureWindow(Interval Frozen, TriggerFacts Facts, DumpPolicy Dump);

public readonly record struct ArtifactPayload(ReadOnlyMemory<byte> Bytes, MaskTally Tally) {
    public static readonly ArtifactPayload Empty = new(ReadOnlyMemory<byte>.Empty, MaskTally.Empty);

    public static ArtifactPayload Of(string text, MaskTally tally) =>
        new(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(text)), tally);
}

public readonly record struct MaskTally(int Masked) : Monoid<MaskTally> {
    public static MaskTally Empty => new(0);
    public static MaskTally Of(Masked verdict) => new(verdict is Masked.Redacted ? 1 : 0);
    public MaskTally Combine(MaskTally rhs) => new(Masked + rhs.Masked);
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class MaskLedger(Redactor redactor) {
    readonly Atom<MaskTally> tally = Atom(MaskTally.Empty);

    public MaskTally Tally => tally.Value;

    public string Mask(object? value) =>
        value?.ToString() is not { Length: > 0 } text
            ? string.Empty
            : Recorded(RedactedText.Mask(redactor, text));

    string Recorded(Masked verdict) =>
        (ignore(tally.Swap(held => held.Combine(MaskTally.Of(verdict)))), verdict.Value).Item2;
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record SupportArtifact(
    string Name,
    DataClassification Classification,
    long EstimatedBytes,
    Func<CaptureWindow, IO<ArtifactPayload>> Produce,
    Option<Func<Fin<Unit>>> Cleanup = default) {
    public static SupportArtifact EffectiveConfig(IConfigurationRoot root, Redactor redactor) => new(
        Name: "effective-config",
        Classification: DataClassification.Operational,
        EstimatedBytes: 64 << 10,
        Produce: _ => IO.lift(() => Viewed(root, new MaskLedger(redactor))));

    static ArtifactPayload Viewed(IConfigurationRoot root, MaskLedger ledger) =>
        ArtifactPayload.Of(root.GetDebugView(entry => ledger.Mask(entry.Value)), ledger.Tally);

    public static SupportArtifact SignalReadings(InstrumentTally tally, Redactor redactor) => new(
        Name: "signal-readings",
        Classification: DataClassification.HostIdentity,
        EstimatedBytes: 256 << 10,
        Produce: _ => IO.lift(tally.Read).Bind(readings => readings.Match(
            Succ: rows => IO.pure(Rendered(rows, new MaskLedger(redactor))),
            Fail: fault => IO.fail<ArtifactPayload>(
                (Error)new SupportFault.ContributorFaulted("signal-readings", fault.Message, fault)))));

    static ArtifactPayload Rendered(Seq<InstrumentReading> readings, MaskLedger ledger) =>
        ArtifactPayload.Of(
            string.Join(Environment.NewLine, readings.Bind(reading => Lines(reading, ledger))),
            ledger.Tally);

    static Seq<string> Lines(InstrumentReading reading, MaskLedger ledger) =>
        $"{reading.Row.Name} {reading.Row.Kind.Key} {reading.Row.Unit}" switch {
            var head => reading.Cells.IsEmpty
                ? Seq($"{head} unmeasured")
                : reading.Cells.Map(cell => string.Create(CultureInfo.InvariantCulture,
                    $"{head}{Tagged(cell, ledger)} count={cell.Count} sum={cell.Sum:R} min={cell.Min:R} max={cell.Max:R} last={cell.Last:R}")),
        };

    static string Tagged(ReadingCell cell, MaskLedger ledger) =>
        string.Concat(toSeq(cell.Tags).Map(tag => $" {tag.Key}={ledger.Mask(tag.Value)}"));

    public static SupportArtifact PhaseCommits(
        Func<Interval, Seq<PhaseCommit>> window, JsonTypeInfo<ImmutableArray<PhaseCommit>> contract) => new(
        Name: "phase-commits",
        Classification: DataClassification.Operational,
        EstimatedBytes: 32 << 10,
        Produce: capture => IO.lift(() => Serialized([.. window(capture.Frozen)], contract)));

    public static SupportArtifact HealthReading(DegradationCell cell, JsonTypeInfo<DegradationReading> contract) => new(
        Name: "health-reading",
        Classification: DataClassification.HostIdentity,
        EstimatedBytes: 16 << 10,
        Produce: _ => IO.lift(() => Serialized(cell.Read(), contract)));

    public static SupportArtifact HookFaults(FaultCell cell, JsonTypeInfo<HookFaultView> contract) => new(
        Name: "hook-faults",
        Classification: DataClassification.Operational,
        EstimatedBytes: 32 << 10,
        Produce: _ => IO.lift(() => Serialized(
            new HookFaultView(
                [.. cell.Parked.Map(static row => new HookFaultRow(
                    row.Point.Value, FaultWire.Observe(row.Cause), row.At))],
                cell.Shed, cell.Lost),
            contract)));

    public static SupportArtifact DriftProbe(
        Func<Seq<LogEntry>> chain, Func<ChaosObservation, long> observed,
        JsonTypeInfo<ImmutableArray<ChaosDrift>> contract) => new(
        Name: "determinism-drift",
        Classification: DataClassification.Operational,
        EstimatedBytes: 16 << 10,
        Produce: _ => IO.lift(() => Serialized([.. AdversarialProbe.Drift(chain(), observed)], contract)));

    static ArtifactPayload Serialized<TPayload>(TPayload payload, JsonTypeInfo<TPayload> contract) =>
        new(new ReadOnlyMemory<byte>(JsonSerializer.SerializeToUtf8Bytes(payload, contract)), MaskTally.Empty);

    public static SupportArtifact EventTrace(Seq<EventPipeProvider> providers, Duration window, Dimension circularBufferMiB) => new(
        Name: "event-trace",
        Classification: DataClassification.Operational,
        EstimatedBytes: (long)circularBufferMiB.Value << 20,
        Produce: _ => IO.lift(() => TraceSession.Open(providers, circularBufferMiB)).Bracket(
            Use: opened => Pumped(opened, window),
            Catch: static error => IO.fail<ArtifactPayload>((Error)new SupportFault.DecodeFaulted(error.Message, error)),
            Fin: static opened => IO.lift(() => (fun(opened.Session.Stop)(), unit).Item2)));

    static IO<ArtifactPayload> Pumped(TraceSession opened, Duration window) =>
        from pump in IO.lift(() => (fun(opened.Source.Process)(), unit).Item2).Fork(window.ToTimeSpan())
        from _ in pump.Await | @catch(static error => error.Is(Errors.Cancelled), static _ => IO.pure(unit))
        select ArtifactPayload.Of(opened.Sink.ToString(), MaskTally.Empty);

    public static Seq<SupportArtifact> Folded(Seq<SupportArtifact> own, params ReadOnlySpan<SupportContributorPort> contributed) =>
        Iterable<SupportContributorPort>.FromSpan(contributed).ToSeq().Fold(own, static (rows, port) => rows + port.Rows);
}

[Equatable]
public sealed partial record HookFaultView(
    [property: OrderedEquality] ImmutableArray<HookFaultRow> Parked, long Shed, long Lost);

public readonly record struct HookFaultRow(
    string Point,
    Rasm.Contracts.Fault.FaultObservation Cause,
    DateTimeOffset At);

public sealed record TraceSession(EventPipeSession Session, EventPipeEventSource Source, StringBuilder Sink) : IDisposable {
    public static TraceSession Open(Seq<EventPipeProvider> providers, Dimension circularBufferMiB) {
        var session = new DiagnosticsClient(Environment.ProcessId).StartEventPipeSession(
            [.. providers], requestRundown: false, circularBufferMB: circularBufferMiB.Value);
        var sink = new StringBuilder();
        var source = new EventPipeEventSource(session.EventStream);
        source.Dynamic.All += evt => ignore(sink.AppendLine($"{evt.TimeStamp:O} {evt.ProviderName}/{evt.EventName}"));
        return new(session, source, sink);
    }

    public void Dispose() {
        Source.Dispose();
        Session.Dispose();
    }
}
```

## [04]-[DUMP_CUSTODY]

- Owner: `DumpSource` `[SmartEnum<string>]` the process-image source carrying its `DataTarget` factory and its CUSTODY column; `DumpPolicy` the completeness and walk-bound row; `DumpTriage` the ClrMD post-capture fold projecting the captured image into bounded heap-sample, thread, and root rows; the two dump `SupportArtifact` rows that bracket both.
- Cases: two image sources — a captured minidump file and a forked live snapshot; three completeness rows — a dumpless snapshot routine, a file triage routine, and the escalation the watchdog enrollment names.
- Entry: `DumpSource.Open(string dumpPath)` resolves the image; `DumpSource.Custody(string captureRoot)` answers the release this source owes, absent where it writes no file; `SupportArtifact.ProcessDump(DumpPolicy, string captureRoot)` returns the raw-image row where custody exists; `SupportArtifact.DumpAnalysis(string captureRoot, JsonTypeInfo<DumpTriage>)` returns the triage row; `DumpTriage.Walk(string dumpPath, DumpPolicy policy)` returns `Fin<DumpTriage>`.
- Law: custody is the SOURCE's own column and not a boolean a reader re-interprets. One answer decides both whether a raw-dump row exists at all and what releases it, so a forked image writes nothing, owes nothing, and mints no artifact whose whole custody surface a capture then writes and immediately deletes.
- Auto: `Snapshot` is the ROUTINE completeness — a watchdog timeout or a health-threshold breach fires it freely because it costs a process fork rather than a 512 MiB write — and `Escalated` rides the `WatchdogEnrollment` alone; `CensusCap`, `TriageRows`, and `FrameCap` bound every enumeration and output family; the triage fold makes no retained-size or leak-causality claim.
- Packages: Rasm (kernel `ContentHash`), Microsoft.Diagnostics.NETCore.Client, Microsoft.Diagnostics.Runtime, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new process-image source is one `DumpSource` row carrying its `DataTarget` factory and its custody answer; a new completeness is one `DumpPolicy` value; a triage-depth retune is one `CensusCap`/`TriageRows`/`FrameCap` value; a new triage dimension is one `DumpTriage` row family; zero new surface.
- Boundary: the `ProcessDump` row composes `Microsoft.Diagnostics.NETCore.Client` — `DiagnosticsClient.WriteDumpAsync(DumpType, path, WriteDumpFlags, CancellationToken)` captures under the frozen window observing the `DeadlineClass.SupportWindow` token, so the declared bound binds THROUGH the largest artifact instead of expiring around a synchronous write no token reaches, with completeness as `DumpPolicy` row data; a capture-tool fault is the typed registry-banded case, never a bare `Error.New` and never an orphan code outside every band; the raw row materializes the manifest bytes and registers the image path's cleanup independently of `DumpAnalysis`, and its failure arm releases EAGERLY while the custody row remains the guaranteed release — the two are independent because `DumpAnalysis` still has to read the file the success path leaves; `DumpAnalysis` folds through `Microsoft.Diagnostics.Runtime` — the policy's own `DumpSource.Open` resolves the image (`DataTarget.LoadDump(string filePath, DataTargetOptions? options = null)` for a captured file, `DataTarget.CreateSnapshotAndAttach(int processId, DataTargetOptions? options = null)` for the process fork a dumpless triage walks live), the FIRST `ClrVersions` entry admits through the rail rather than an index — a process image carrying no CLR is a real refusal a bare `[0]` turns into an exception the recovery arm reports as a decode fault — `ClrHeap.EnumerateObjects` samples at most `CensusCap` objects before grouping by `ClrObject.Type?.Name` and summing shallow `ClrObject.Size`, `ClrRuntime.Threads` projects `OSThreadId`/`ManagedThreadId`/`GCMode`/`State` with `ClrThread.CurrentException?.Type?.Name` and the `EnumerateStackTrace(includeContext, maxFrames)`-bounded frame walk discriminated on `ClrStackFrameKind.ManagedMethod` versus the runtime `FrameName`, and `ClrHeap.EnumerateRoots` samples at most `CensusCap` roots before counting `ClrRoot.RootKind`; the triage row's own bracket releases the image on every exit, so cancellation, a skipped dependent row, and an analysis failure cannot leave a partial dump on disk.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DumpSource {
    public static readonly DumpSource File = new("file",
        open: static path => DataTarget.LoadDump(path),
        custody: static root => Some<Func<Fin<Unit>>>(() => DumpTriage.Release(root)));
    public static readonly DumpSource Snapshot = new("snapshot",
        open: static _ => DataTarget.CreateSnapshotAndAttach(Environment.ProcessId),
        custody: static _ => Option<Func<Fin<Unit>>>.None);

    [UseDelegateFromConstructor]
    public partial DataTarget Open(string dumpPath);

    [UseDelegateFromConstructor]
    public partial Option<Func<Fin<Unit>>> Custody(string captureRoot);
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record DumpPolicy(DumpType Kind, WriteDumpFlags Flags, long EstimatedBytes, int CensusCap, int TriageRows, int FrameCap, DumpSource Source) {
    public static readonly DumpPolicy Snapshot = new(DumpType.Triage, WriteDumpFlags.None, 0L, CensusCap: 250_000, TriageRows: 32, FrameCap: 64, DumpSource.Snapshot);
    public static readonly DumpPolicy Routine = new(DumpType.Triage, WriteDumpFlags.None, 64L << 20, CensusCap: 250_000, TriageRows: 32, FrameCap: 64, DumpSource.File);
    public static readonly DumpPolicy Escalated = new(DumpType.WithHeap, WriteDumpFlags.None, 512L << 20, CensusCap: 2_000_000, TriageRows: 64, FrameCap: 128, DumpSource.File);
}

[Equatable]
public sealed partial record DumpTriage(
    [property: OrderedEquality] ImmutableArray<DumpTriage.HeapRow> HeapSample,
    [property: OrderedEquality] ImmutableArray<DumpTriage.ThreadRow> Threads,
    [property: OrderedEquality] ImmutableArray<DumpTriage.RootRow> Roots) {
    public readonly record struct HeapRow(string Type, long Count, long ShallowBytes);

    [Equatable]
    public sealed partial record ThreadRow(uint OsId, int ManagedId, string GcMode, string State,
        [property: OrderedEquality] ImmutableArray<string> Frames, Option<string> Exception = default);

    public readonly record struct RootRow(string Kind, long Count);

    public static Fin<DumpTriage> Walk(string dumpPath, DumpPolicy policy) {
        using DataTarget target = policy.Source.Open(dumpPath);
        return Runtime(target).Map(runtime => Walked(runtime, policy));
    }

    static Fin<ClrRuntime> Runtime(DataTarget target) =>
        toSeq(target.ClrVersions).Head
            .Map(static version => version.CreateRuntime())
            .ToFin(new KernelFault.InvalidResult(Op.Of(), Some("<no-clr-runtime-in-process-image>")));

    static DumpTriage Walked(ClrRuntime runtime, DumpPolicy policy) {
        using (runtime) {
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

    public static string Path(string captureRoot) =>
        System.IO.Path.Join(captureRoot, $"dump-{Environment.ProcessId}.dmp");

    public static Fin<Unit> Release(string captureRoot) => Op.Of().Catch(() => {
        string path = Path(captureRoot);
        if (System.IO.File.Exists(path)) { System.IO.File.Delete(path); }
        return Fin.Succ(unit);
    });
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class DumpArtifacts {
    extension(SupportArtifact) {
        public static Option<SupportArtifact> ProcessDump(DumpPolicy policy, string captureRoot) =>
            policy.Source.Custody(captureRoot).Map(release => new SupportArtifact(
                Name: "process-dump",
                Classification: DataClassification.HostIdentity,
                EstimatedBytes: policy.EstimatedBytes,
                Produce: capture => IO.lift(() => DumpTriage.Path(captureRoot)).Bracket(
                    Use: path => Captured(capture.Dump, path),
                    Catch: error => IO.lift(() => ignore(release()))
                        .Bind(_ => IO.fail<ArtifactPayload>((Error)new SupportFault.DumpRejected(error.Message, error))),
                    Fin: static _ => IO.pure(unit)),
                Cleanup: Some(release)));

        public static SupportArtifact DumpAnalysis(string captureRoot, JsonTypeInfo<DumpTriage> contract) => new(
            Name: "dump-triage",
            Classification: DataClassification.HostIdentity,
            EstimatedBytes: 256L << 10,
            Produce: capture => IO.lift(() => capture.Dump).Bracket(
                Use: policy => Walked(policy, captureRoot, contract),
                Catch: static error => IO.fail<ArtifactPayload>((Error)new SupportFault.DumpRejected(error.Message, error)),
                Fin: policy => IO.lift(() => policy.Source.Custody(captureRoot)
                    .Match(Some: static release => ignore(release()), None: static () => unit))));
    }

    static IO<ArtifactPayload> Captured(DumpPolicy policy, string path) =>
        IO.liftAsync(async envIO => await Op.Of(nameof(Captured)).Catch(async token => {
            await new DiagnosticsClient(Environment.ProcessId)
                .WriteDumpAsync(policy.Kind, path, policy.Flags, token).ConfigureAwait(false);
            return Fin.Succ(new ArtifactPayload(
                new ReadOnlyMemory<byte>(await File.ReadAllBytesAsync(path, token).ConfigureAwait(false)),
                MaskTally.Empty));
        }, envIO.Token)).Bind(static captured => captured.Match(
            Succ: IO.pure,
            Fail: IO.fail<ArtifactPayload>));

    static IO<ArtifactPayload> Walked(DumpPolicy policy, string captureRoot, JsonTypeInfo<DumpTriage> contract) =>
        IO.lift(() => DumpTriage.Walk(DumpTriage.Path(captureRoot), policy)).Bind(static walked => walked.Match(
            Succ: rows => IO.pure(rows),
            Fail: IO.fail<DumpTriage>))
            .Map(rows => new ArtifactPayload(
                new ReadOnlyMemory<byte>(JsonSerializer.SerializeToUtf8Bytes(rows, contract)), MaskTally.Empty));
}
```

## [05]-[CAPTURE_PIPELINE]

- Owner: `SupportFault` `[Union]` the fault family riding the kernel `[FaultCase]`/`Fault` floor (`[FaultCase]` realizes the registry over `FaultBand.Support`; `Code` derive SEALED); `SupportPolicy` the config-bound value row; `SupportRuntime` the bound capture context; `EntryState` `[Union]` the written-or-empty manifest state; `ArtifactRow` the interior row a producer yields; `SupportCapture` the window-freeze, coalesce, ordered fan-in, cleanup, and cap fold.
- Entry: `SupportCapture.Capture(SupportRuntime runtime, SupportTrigger trigger)` returns `IO<SupportOutcome>` — `IO` carries the freeze-fan-redact-cap-bundle effect and the fold opens, marks, and releases its OWN latency ledger.
- Law: the capture fold IS the operation, so it opens its own ledger where the drain conductor and the outbound hop RECEIVE one. That is the discriminant: a fold running inside a caller's operation takes the context that caller already opened, while a fold triggered by a fault commit, a watchdog miss, or a control verb has no ambient operation to take one from — which is why the ledger is a per-call factory column rather than a `SupportRuntime` field a pooled context outlives.
- Law: every `Cleanup` delegate runs EXACTLY once. Folding runs OUTSIDE the compare-and-swap and the settled roster commits through `Cell.Commit`, where a fold inside the CAS body re-ran every release on each contended retry (E-A35).
- Auto: the coalesce gate seats through `Cell.Seat`, so the winner reads `Committed` and every arriving trigger reads `Ceded` with the live correlation on the transition rather than probing the cell a second time; `IncidentBuffers.Flush` replays both held scopes into the frozen window before contributor fan-in and its own scope-count write rides its rail without gating the capture, because an incident bundle is the evidence of the failure being assembled; the artifact cut falls in ONE place, so the manifest's byte count, the content key's preimage, and the zip member are three reads of one projection.
- Output: `SupportOutcome` — exported, coalesced, or evicted; the exported case carries the manifest, the bundle path, its total bytes, and the monotonic elapsed span, and `Assemble` writes one `AppHostMeasure.RedactionTags` point per masked entry off that manifest through the runtime's own `Signals`.
- Packages: Rasm (kernel `ContentHash`, `Cell`, `Transition`, `MonotonicTimeline`, `Op`), Microsoft.Extensions.Telemetry.Abstractions, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: one policy value retunes caps or retention; one fault is one `SupportFault` case; one entry state is one `EntryState` case every consumer's `Switch` breaks on; zero new surface.
- Boundary: the `Active` cell is the coalesce gate — a trigger arriving mid-capture folds to host-local `SupportOutcome.Coalesced` and never opens a second window, and only the capture that seated the gate clears it; classification resolves redaction at row registration, so no unredacted classified byte reaches assembly; every contributor and cleanup runs under one recovery arm and faults become zero-byte entries; `Assemble` brackets fan and cleanup before sealing; every written row's content key covers the capped, already-redacted bytes in its zip member; empty rows cannot carry keys by `EntryState` construction; elapsed rides `MonotonicTimeline`; the exported case is the value `Capture` returns and the phase bracket `Lifecycle.Captured` resumes on, and it crosses no seam beyond the caller.

```csharp
// --- [ERRORS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SupportFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Support;
    private SupportFault(string detail) => Detail = detail;
    public string Detail { get; }
    public sealed override string Message => Detail;

    [FaultCase(0)]
    public sealed partial record DumpRejected : SupportFault, ICausedFault {
        public DumpRejected(string detail, Error cause) : base(detail) => Cause = cause;
        public Error Cause { get; }
    }
    [FaultCase(1)]
    public sealed partial record DecodeFaulted : SupportFault, ICausedFault {
        public DecodeFaulted(string detail, Error cause) : base(detail) => Cause = cause;
        public Error Cause { get; }
    }
    [FaultCase(2)]
    public sealed partial record ContributorFaulted : SupportFault, ICausedFault {
        public ContributorFaulted(string artifact, string detail, Error cause) : base($"{artifact}: {detail}") => Cause = cause;
        public Error Cause { get; }
    }
    [FaultCase(3)]
    public sealed partial record CleanupFaulted : SupportFault, ICausedFault {
        public CleanupFaulted(string artifact, string detail, Error cause) : base($"{artifact}: {detail}") => Cause = cause;
        public Error Cause { get; }
    }
}

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record EntryState {
    private EntryState() { }
    public sealed record Written(long Bytes, long TruncatedBytes, string ContentKey) : EntryState;
    public sealed record Empty(long TruncatedBytes) : EntryState;

    public long Bytes => Switch(written: static row => row.Bytes, empty: static _ => 0L);
    public long TruncatedBytes => Switch(written: static row => row.TruncatedBytes, empty: static row => row.TruncatedBytes);
    public Option<string> ContentKey => Switch(
        written: static row => Some(row.ContentKey), empty: static _ => Option<string>.None);
}

public sealed record ArtifactRow(
    string Name, DataClassification Classification, EntryState State, int Redactions,
    Option<Rasm.Contracts.Fault.FaultObservation> Fault, ReadOnlyMemory<byte> Bytes);

public sealed record SupportPolicy(
    Duration Lookback,
    Duration Settle,
    long ArtifactCapBytes,
    long BundleCapBytes,
    int MaxBundles,
    Duration MaxAge) {
    public ReadOnlyMemory<byte> Cap(ReadOnlyMemory<byte> payload) =>
        payload[..(int)long.Min(payload.Length, ArtifactCapBytes)];
}

public sealed record SupportRuntime(
    SupportPolicy Policy,
    ConsumptionProfile Profile,
    string StorageRoot,
    ImmutableDictionary<string, string> Versions,
    ClockPolicy Clocks,
    IncidentBuffers Buffer,
    InstrumentSet Signals,
    Func<(ILatencyContext Context, CheckpointToken Phase)> Latency,
    Func<Interval, Seq<PhaseCommit>> Phases,
    Option<WatchdogEnrollment> Watchdog,
    JsonTypeInfo<SupportManifest> ManifestContract,
    Seq<SupportArtifact> Contributors,
    Atom<Option<CorrelationId>> Active);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SupportCapture {
    static readonly Op CaptureWork = Op.Of(nameof(Capture));

    public static IO<SupportOutcome> Capture(SupportRuntime runtime, SupportTrigger trigger) =>
        trigger.Facts() switch {
            var facts => IO.lift(() => Cell.Seat(runtime.Active, () => facts.Correlation)).Bracket(
                Use: seat => seat is Transition<Option<CorrelationId>>.Committed
                    ? Ledgered(runtime, facts)
                    : IO.pure<SupportOutcome>(new SupportOutcome.Coalesced(
                        seat.Current.IfNone(facts.Correlation), facts.Kind)),
                Catch: static error => IO.fail<SupportOutcome>(error),
                Fin: seat => IO.lift(() => Cleared(runtime.Active, seat))),
        };

    static IO<SupportOutcome> Ledgered(SupportRuntime runtime, TriggerFacts facts) =>
        IO.lift(runtime.Latency).Bracket(
            Use: opened => Assemble(runtime, facts, opened.Context, opened.Phase),
            Fin: static opened => IO.lift(() => (fun(opened.Context.Dispose)(), unit).Item2));

    static Unit Cleared(Atom<Option<CorrelationId>> gate, Transition<Option<CorrelationId>> seat) =>
        seat is Transition<Option<CorrelationId>>.Committed ? ignore(Cell.Take(gate)) : unit;

    static IO<SupportOutcome> Assemble(
        SupportRuntime runtime, TriggerFacts facts, ILatencyContext latency, CheckpointToken phase) =>
        from at in IO.lift(() => runtime.Clocks.Now)
        from opened in Stamped(runtime)
        let window = new CaptureWindow(
            new Interval(at - facts.Override.IfNone(runtime.Policy.Lookback), at + runtime.Policy.Settle),
            facts,
            Completeness(runtime, facts.Kind))
        from _marked in IO.lift(() => LatencySpine.Mark(latency, phase))
        from _held in IO.lift(() => runtime.Buffer.Flush(runtime.Signals))
        let cleanup = Atom(Seq<ArtifactRow>())
        from produced in IO.pure(unit).Bracket(
            Use: _ => runtime.Contributors.TraverseM(row => Produced(row, window, runtime.Policy)).As(),
            Fin: _ => IO.lift(() => Released(runtime.Contributors, cleanup)))
        let rows = Capped(produced + cleanup.Value, runtime.Policy)
        from exported in SupportLedger.Bundle(runtime, BundleMap.Manifest(facts, window, rows, runtime), rows, opened)
        from _masked in IO.lift(() => toSeq(exported.Manifest.Entries)
            .Filter(static entry => entry.Redactions > 0)
            .TraverseM(entry => runtime.Signals.Write(AppHostMeasure.RedactionTags.Row, entry.Redactions,
                InstrumentSet.Tags((AppHostSlot.Class, entry.Classification.Key)))).As())
            .Bind(static written => written.Match(Succ: static _ => IO.pure(unit), Fail: IO.fail<Unit>))
        select (SupportOutcome)exported;

    static DumpPolicy Completeness(SupportRuntime runtime, SupportTriggerKind kind) =>
        (kind.Dump | runtime.Watchdog.Map(static held => held.Policy)).IfNone(DumpPolicy.Snapshot);

    static IO<MonotonicStamp> Stamped(SupportRuntime runtime) =>
        IO.lift(() => runtime.Clocks.Line.Capture(CaptureWork))
            .Bind(static captured => captured.Match(Succ: IO.pure, Fail: IO.fail<MonotonicStamp>));

    static IO<ArtifactRow> Produced(SupportArtifact row, CaptureWindow window, SupportPolicy policy) =>
        (row.Produce(window).Map(payload => Written(row, payload, policy))
            | @catch<IO, ArtifactRow>(static _ => true, error => IO.pure(Refused(
                row.Name, row.Classification, new SupportFault.ContributorFaulted(row.Name, error.Message, error))))).As();

    static Unit Released(Seq<SupportArtifact> contributors, Atom<Seq<ArtifactRow>> cell) =>
        contributors.Fold(Seq<ArtifactRow>(), Refusal) switch {
            var folded => ignore(Cell.Commit(cell, _ => folded, Cell.SwapBudget)),
        };

    static Seq<ArtifactRow> Refusal(Seq<ArtifactRow> faults, SupportArtifact row) =>
        row.Cleanup.Match(
            None: () => faults,
            Some: release => Op.Of().Catch(() => Fin.Succ(release())).Bind(static outcome => outcome).Match(
                Succ: _ => faults,
                Fail: error => faults.Add(Refused(
                    $"{row.Name}-cleanup", row.Classification,
                    new SupportFault.CleanupFaulted(row.Name, error.Message, error)))));

    static ArtifactRow Refused(string name, DataClassification classification, SupportFault fault) =>
        new(name, classification, new EntryState.Empty(0L), Redactions: 0,
            Fault: Some(FaultWire.Observe(fault)), Bytes: ReadOnlyMemory<byte>.Empty);

    static ArtifactRow Written(SupportArtifact row, ArtifactPayload payload, SupportPolicy policy) =>
        policy.Cap(payload.Bytes) switch {
            var kept => new ArtifactRow(
                row.Name, row.Classification,
                new EntryState.Written(
                    kept.Length,
                    long.Max(payload.Bytes.Length - kept.Length, 0L),
                    ContentHash.Hex(ContentHash.Of(kept.Span))),
                payload.Tally.Masked, Fault: None, Bytes: kept),
        };

    static Seq<ArtifactRow> Capped(Seq<ArtifactRow> produced, SupportPolicy policy) =>
        produced.Fold(
            (Total: 0L, Rows: Seq<ArtifactRow>()),
            (acc, row) => acc.Total + row.State.Bytes > policy.BundleCapBytes
                ? (acc.Total, acc.Rows.Add(row with {
                    State = new EntryState.Empty(row.State.Bytes),
                    Bytes = ReadOnlyMemory<byte>.Empty,
                }))
                : (acc.Total + row.State.Bytes, acc.Rows.Add(row)))
            .Rows;
}
```

Policy rows bind through the config rail: rows one through six freeze into the `SupportPolicy` record and the retention-sweep-cadence row binds the `Sweep` `ScheduleEntry` registration `Runtime/modules#MODULE_LEDGER` seats. Every capture and retention literal traces to this table:

| [INDEX] | [POLICY]                |    [VALUE] | [RELOAD_CLASS] |
| :-----: | :---------------------- | ---------: | :------------- |
|  [01]   | window-lookback         | 10 minutes | transition     |
|  [02]   | window-settle           | 30 seconds | transition     |
|  [03]   | artifact-cap            |     16 MiB | transition     |
|  [04]   | bundle-cap              |    128 MiB | transition     |
|  [05]   | retention-max-bundles   |         16 | transition     |
|  [06]   | retention-max-age       |    30 days | transition     |
|  [07]   | retention-sweep-cadence |   `@daily` | transition     |

Every row names its `SupportArtifact` factory, so table and fence carry one roster and a row without a factory is a name the bundle never writes. Gcdump heap graphs ride the `dotnet-gcdump` tool boundary, since the admitted TraceEvent assembly ships no reader for it. Held log records are deliberately absent: `IncidentBuffers.Flush` replays them into the LIVE pipeline inside the frozen window rather than into the zip, so they land wherever this profile's log pipeline delivers and a second copy in the archive is the same records under two retention policies. Sibling packages add rows through `SupportContributorPort`.

| [INDEX] | [ARTIFACT]        | [FACTORY]         | [PRODUCER]                                                           |
| :-----: | :---------------- | :---------------- | :------------------------------------------------------------------- |
|  [01]   | effective-config  | `EffectiveConfig` | redacted configuration debug view                                    |
|  [02]   | signal-readings   | `SignalReadings`  | kernel `InstrumentTally` rows, backend-free                          |
|  [03]   | phase-commits     | `PhaseCommits`    | lifecycle commits the runtime's window supplier returns              |
|  [04]   | health-reading    | `HealthReading`   | the degradation cell's own coherent reading                          |
|  [05]   | hook-faults       | `HookFaults`      | the rail's `FaultCell` parked rows beside its shed and lost counters |
|  [06]   | determinism-drift | `DriftProbe`      | `AdversarialProbe.Drift` over the recorded chaos chain               |
|  [07]   | process-dump      | `ProcessDump`     | `DiagnosticsClient.WriteDumpAsync` where the source owes custody     |
|  [08]   | dump-triage       | `DumpAnalysis`    | ClrMD `DumpTriage.Walk` rows over the capture's own `DumpSource`     |
|  [09]   | event-trace       | `EventTrace`      | EventPipe session decoded through TraceEvent `EventPipeEventSource`  |

## [06]-[BUNDLE_SEAL]

- Owner: `SupportManifest` is the archive manifest; `SupportOutcome` `[Union]` is the native outcome family; `BundleMap` projects produced rows into the manifest; `SupportLedger` owns zip assembly and retention.
- Cases: `Exported`, `Coalesced`, `Evicted`.
- Entry: `BundleMap.Manifest(TriggerFacts, CaptureWindow, Seq<ArtifactRow>, SupportRuntime)` projects the archive manifest; `SupportLedger.Bundle(...)` writes the zip and `Sweep` owns retention eviction.
- Auto: `Sweep` registers as one retention schedule row; every case stays host-local — `Exported` is what `Capture` returns, `Coalesced` what a mid-capture trigger folds to, `Evicted` what `Sweep` returns.
- Packages: Rasm.Contracts, Thinktecture.Runtime.Extensions, Generator.Equals, NodaTime, LanguageExt.Core, BCL inbox
- Growth: an archive column extends `SupportManifest`; one outcome extends `SupportOutcome` and breaks every consumer arm; zero mirror surface.
- Boundary: `Bundle` and `Evict` are the named `System.IO` capsules. Each entry's archive `ContentKey` stays lowercase kernel text over the final written bytes. The `Exported` case carries the archive path for local custody and crosses no peer boundary. Native `FaultSource` facts and generated `FaultObservation` members serialize through the one `SuiteContracts.Host` graph, whose protobuf converter gives embedded fault messages canonical ProtoJSON without reflecting over them. Retention folds count and bytes in one state and reads file size before deletion.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
[Equatable]
public sealed partial record SupportManifest(
    SupportTriggerKind Trigger,
    string Reason,
    CorrelationId Correlation,
    Instant WindowStart,
    Instant WindowEnd,
    ConsumptionProfile Profile,
    [property: OrderedEquality] ImmutableArray<SupportManifest.Entry> Entries,
    [property: UnorderedEquality] ImmutableDictionary<string, string> PackageVersions,
    Option<FaultSource> Fault = default) {
    public sealed record Entry(
        string Name,
        DataClassification Classification,
        long Bytes,
        long TruncatedBytes,
        int Redactions,
        Option<string> ContentKey = default,
        Option<Rasm.Contracts.Fault.FaultObservation> Fault = default);

    public int Redactions => Entries.Sum(static entry => entry.Redactions);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SupportOutcome {
    private SupportOutcome() { }
    public sealed record Exported(SupportManifest Manifest, string BundlePath, long TotalBytes, Duration Elapsed) : SupportOutcome;
    public sealed record Coalesced(CorrelationId Active, SupportTriggerKind FoldedKind) : SupportOutcome;
    public sealed record Evicted(int Bundles, long Bytes, Instant At) : SupportOutcome;
}

// --- [BOUNDARIES] ----------------------------------------------------------------------
internal static class BundleMap {
    public static SupportManifest Manifest(
        TriggerFacts facts, CaptureWindow window, Seq<ArtifactRow> rows, SupportRuntime runtime) =>
        new(
            Trigger: facts.Kind,
            Reason: facts.Reason,
            Correlation: facts.Correlation,
            WindowStart: window.Frozen.Start,
            WindowEnd: window.Frozen.End,
            Profile: runtime.Profile,
            Entries: [.. rows.Map(Entry)],
            PackageVersions: runtime.Versions,
            Fault: facts.Fault);

    static SupportManifest.Entry Entry(ArtifactRow row) => new(
        row.Name,
        row.Classification,
        row.State.Bytes,
        row.State.TruncatedBytes,
        row.Redactions,
        row.State.ContentKey,
        row.Fault);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SupportLedger {
    static readonly Op BundleWork = Op.Of(nameof(Bundle));

    public static IO<SupportOutcome> Sweep(SupportRuntime runtime) =>
        IO.lift(() => runtime.Clocks.Now).Map(at => Swept(runtime, at));

    internal static IO<SupportOutcome.Exported> Bundle(
        SupportRuntime runtime, SupportManifest manifest, Seq<ArtifactRow> rows, MonotonicStamp opened) =>
        from path in IO.lift(() => Written(runtime, manifest, rows))
        from elapsed in Spanned(runtime.Clocks, opened)
        select new SupportOutcome.Exported(
            manifest, path, new FileInfo(path).Length, Duration.FromTimeSpan(elapsed));

    static IO<TimeSpan> Spanned(ClockPolicy clocks, MonotonicStamp opened) =>
        IO.lift(() => clocks.Line.Capture(BundleWork)
                .Bind(closed => clocks.Line.Elapsed(opened, closed, BundleWork)))
            .Bind(static measured => measured.Match(Succ: IO.pure, Fail: IO.fail<TimeSpan>));

    static string Written(SupportRuntime runtime, SupportManifest manifest, Seq<ArtifactRow> rows) {
        string path = Path.Join(runtime.StorageRoot, $"{manifest.Correlation}.zip");
        using (FileStream sink = File.Create(path))
        using (ZipArchive zip = new(sink, ZipArchiveMode.Create)) {
            using (Stream head = zip.CreateEntry("manifest.json").Open()) {
                JsonSerializer.Serialize(head, manifest, runtime.ManifestContract);
            }
            foreach (ArtifactRow row in rows) {
                using Stream body = zip.CreateEntry(row.Name).Open();
                body.Write(row.Bytes.Span);
            }
        }
        return path;
    }

    static SupportOutcome Swept(SupportRuntime runtime, Instant at) =>
        toSeq(new DirectoryInfo(runtime.StorageRoot).EnumerateFiles("*.zip")
                .OrderByDescending(static file => file.CreationTimeUtc))
            .Map(static (file, rank) => (File: file, Rank: rank))
            .Filter(row => row.Rank >= runtime.Policy.MaxBundles
                || row.File.CreationTimeUtc < (at - runtime.Policy.MaxAge).ToDateTimeOffset().UtcDateTime)
            .Fold((Bundles: 0, Bytes: 0L), static (swept, row) =>
                (swept.Bundles + 1, swept.Bytes + Released(row.File)))
        switch {
            var swept => new SupportOutcome.Evicted(swept.Bundles, swept.Bytes, at),
        };

    static long Released(FileInfo file) {
        long size = file.Length;
        file.Delete();
        return size;
    }
}
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
