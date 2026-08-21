# [APPHOST_SUPPORT_BUNDLES]

Support capture owns the runtime spine's bounded diagnostic evidence surface: one `SupportTrigger` union admits every cause, and one fold freezes the window, gathers ordered artifact rows, redacts before write, caps with receipts, and lands one zip. Capture law owns the trigger vocabulary, the artifact roster and its contributed ports, dump custody, policy values, the manifest, and the receipt wire shapes. Each bundle is process-local evidence; HLC stamps correlate cross-process incidents.

Settled composition: `SupportContributorPort` arrives from Runtime/ports#PORT_SURFACE; `ContentHash.Of`/`.Hex`/`.Admit`, `Dimension`, `Masked`, `Cell`, `Transition`, `MonotonicTimeline`, `Op`, and `InstrumentTally`/`InstrumentReading`/`ReadingCell` arrive from the kernel; `FaultCell` and `IsolatedFault` from Rasm/Domain/hooks#HOOK_RAIL; `RedactedText`/`DataClassification` from Observability/telemetry#REDACTION_TAXONOMY; `LatencySpine`/`LatencyCheckpoint` from Observability/telemetry#LATENCY_PLANE; `AppHostMeasure`/`InstrumentSet` from Observability/instruments; `ClockPolicy`/`ScheduleEntry`/`DeadlineClass`/`DeadlineReceipt` from Runtime/time; `WatchdogEnrollment` from Runtime/profiles#BOOT_SURFACE; `AdversarialProbe.Drift`/`ChaosDrift`/`ChaosObservation` from Runtime/determinism; `PerfMapLease` from Observability/benchmarks#CAPTURE_SEAM, which is where the profiled-window symbol lease declares and where all three of its consumers live.

## [01]-[INDEX]

- [02]-[TRIGGER_UNION]: Six capture causes as one wire vocabulary over four payload cases with the universal columns on the root.
- [03]-[ARTIFACT_ROSTER]: Contributor factory rows, the contributed-port seam, the one masking ledger, and the event-trace pump.
- [04]-[DUMP_CUSTODY]: Image source with its custody column, completeness policy, and the ClrMD triage fold.
- [05]-[CAPTURE_PIPELINE]: Window freeze, coalesce gate, ordered fan-in, cleanup fold, and the bundle cap.
- [06]-[MANIFEST_RECEIPT]: Zip assembly, the one wire mapper, receipt union, retention sweep, and process law.
- [07]-[TS_PROJECTION]: Manifest and receipt wire shapes the TS dashboard ingests.

## [02]-[TRIGGER_UNION]

- Owner: `SupportTriggerKind` `[SmartEnum<string>]` the six capture-cause WIRE keys, each carrying the dump completeness its cause deserves; `SupportTrigger` `[Union]` the four payload cases over one root carrying the correlation and window-override columns every cause shares; `TriggerFacts` the named projection every downstream fold reads.
- Cases: `Requested` covers the two operator causes — a local request and an admitted `ControlService` verb — discriminated by its `Origin` key; `FaultTransition` carries the wire-stable `FaultRecord`; `HealthThreshold` carries the crossed `DegradationLevel`; `Timed` covers the two schedule-port causes — a missed heartbeat deadline and a registered cadence row — discriminated by the same `Origin` column and carrying the firing entry beside its measured `DeadlineReceipt`.
- Entry: `trigger.Facts()` returns `TriggerFacts` — correlation, kind, rendered reason, and window override in one named value the capture fold, the manifest mapper, and the coalesce receipt all read.
- Law: the kind vocabulary is WIRE and outranks the case split. All six keys survive the pair collapse as `SupportTriggerKind` rows because the TS `SupportTriggerKind` union and `tests/contracts/MANIFEST.md` `[02.21]` both decode them, so two cases sharing a payload shape share a case and name their origin as a column rather than losing a key the peer parses (E-A34).
- Auto: `FaultTransition` carries the wire-stable `FaultRecord` the `Runtime/lifecycle#FAULT_SPINE` `FaultRecordMap.From` flatten produces — the one fault-to-capture fact `FaultSpine.ArmTraps` emits for every `FaultSource` entry, the live unhandled/unobserved/signalled commits and the `ProbeMarkers` host-crash-marker boot probe alike, so a fault commit and its capture trigger are one fact rather than an untyped capture delegate beside a `PhaseTrigger.FaultCommitted` emission; `Facts` carries that record as `Some` into both `SupportManifest` and `SupportCaptureWire` while `Reason` remains the short operator label, so exact cause stamps never disappear into the label; `Timed` fires from `Runtime/time#SCHEDULE_PORT` `Heartbeat` on a missed deadline and from a `ScheduleEntry` row on the schedule port, rendering its reason off the entry's wire-stable `Key` and the receipt's own lane and outcome keys rather than a synthesized record text.
- Packages: Thinktecture.Runtime.Extensions, NodaTime, LanguageExt.Core
- Growth: one capture cause is one `SupportTriggerKind` row and, where its payload differs from every standing case, one `SupportTrigger` case whose `Facts` arm the generated `Switch` demands; a new fault cause is one `FaultSource` case the `FaultRecordMap.From` flatten and the one `FaultTransition` payload both absorb, never a second trigger case per fault kind; a completeness retune is one `Dump` column value; zero new surface.
- Boundary: the abstract root and its deleted value conversion seal ingress — every case reaches the root's own constructor and no caller mints a trigger from a key; fault, health, and schedule causes carry their typed evidence whole, and the short reason renders exactly once inside the total `Facts` dispatch; the `FaultTransition` payload is the wire-stable `FaultRecord` whose `kind` literals (`unhandled`/`unobserved-task`/`posix-signal`/`host-crash-marker`/`marker-drifted`) ride the manifest beside that label, so durable-orchestration crash recovery (`Runtime/orchestration#CRASH_RESUME`) and the bundle read one kind-discriminated fact, and a trigger that keeps only rendered reason text is the deleted form; the live `ScheduleEntry` with its `Func<IO<Unit>> Work` closure stays process-local — `Facts` renders its `Key` beside the `DeadlineReceipt`'s lane and outcome keys, which together are the wire-stable contract `Runtime/time#SCHEDULE_PORT` declares, so no closure and no rendered duration ride into a serialized manifest and the reason stays one smart-enum alphabet; dump completeness for a watchdog miss is the ENROLLMENT's answer and not this roster's, because a manager that stopped receiving keep-alives is the one cause a routine image cannot explain and `Runtime/profiles#BOOT_SURFACE` already mints the `WatchdogEnrollment` carrying it.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
// Six WIRE keys the TS union and the corpus manifest both decode, each carrying the dump completeness its cause
// deserves — so the two exact-shape case pairs collapse without losing a key a peer parses.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SupportTriggerKind {
    public static readonly SupportTriggerKind UserRequested = new("user-requested", Some(DumpPolicy.Routine));
    public static readonly SupportTriggerKind FaultTransition = new("fault-transition", Some(DumpPolicy.Routine));
    public static readonly SupportTriggerKind HealthThreshold = new("health-threshold", Some(DumpPolicy.Snapshot));
    // Watchdog completeness is the ENROLLMENT's — `ProfileBoot.Enrolled` already mints the row carrying it, so an
    // absent value here keeps the manager's own answer the only one.
    public static readonly SupportTriggerKind WatchdogTimeout = new("watchdog-timeout", Option<DumpPolicy>.None);
    public static readonly SupportTriggerKind ExternalCommand = new("external-command", Some(DumpPolicy.Routine));
    public static readonly SupportTriggerKind Scheduled = new("scheduled", Some(DumpPolicy.Snapshot));

    public Option<DumpPolicy> Dump { get; }
}

// --- [MODELS] -------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SupportTrigger {
    private protected SupportTrigger(CorrelationId correlation, Option<Duration> windowOverride) =>
        (Correlation, WindowOverride) = (correlation, windowOverride);

    public CorrelationId Correlation { get; }
    public Option<Duration> WindowOverride { get; }

    public sealed record Requested(CorrelationId Correlation, SupportTriggerKind Origin, string Reason, Option<Duration> WindowOverride = default)
        : SupportTrigger(Correlation, WindowOverride);
    public sealed record FaultTransition(CorrelationId Correlation, FaultRecord Fault, Option<Duration> WindowOverride = default)
        : SupportTrigger(Correlation, WindowOverride);
    public sealed record HealthThreshold(CorrelationId Correlation, DegradationLevel Level, Option<Duration> WindowOverride = default)
        : SupportTrigger(Correlation, WindowOverride);
    // The schedule causes carry BOTH halves of the `Runtime/time#SCHEDULE_PORT` contract: the entry, whose
    // wire-stable `Key` names the row that fired, and the measured `DeadlineReceipt`, whose lane and outcome
    // name the crossing. The receipt is the half that makes a watchdog bundle diagnostic — a key alone reports
    // that some occurrence was late and nothing about which ceiling it blew or whether it escalated.
    public sealed record Timed(CorrelationId Correlation, SupportTriggerKind Origin, ScheduleEntry Entry, DeadlineReceipt Deadline, Option<Duration> WindowOverride = default)
        : SupportTrigger(Correlation, WindowOverride);
}

public readonly record struct TriggerFacts(
    CorrelationId Correlation,
    SupportTriggerKind Kind,
    string Reason,
    Option<Duration> Override,
    Option<FaultRecord> Fault = default);

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class SupportTriggerOps {
    extension(SupportTrigger trigger) {
        public TriggerFacts Facts() => trigger.Switch(
            requested:       static row => new TriggerFacts(row.Correlation, row.Origin, row.Reason, row.WindowOverride),
            faultTransition: static row => new TriggerFacts(
                                 row.Correlation, SupportTriggerKind.FaultTransition,
                                 FaultReason(row.Fault), row.WindowOverride, Some(row.Fault)),
            healthThreshold: static row => new TriggerFacts(row.Correlation, SupportTriggerKind.HealthThreshold, row.Level.Key, row.WindowOverride),
            // The entry's `Key` and the receipt's LANE and OUTCOME cross — three smart-enum keys, no formatted
            // duration, so the reason stays wire-stable text under one alphabet. The live row's work closure
            // and the receipt's raw span stay process-local: the schedule port keeps the first, and a rendered
            // elapsed would put a temporal grammar in a manifest field that owns none.
            timed:           static row => new TriggerFacts(
                                 row.Correlation, row.Origin,
                                 $"{row.Entry.Key}:{row.Deadline.Class.Key}:{row.Deadline.Outcome.Key}",
                                 row.WindowOverride));

        static string FaultReason(FaultRecord record) => record.Switch(
            unhandled:       static row => $"unhandled:{row.Termination.Key}:{Evidence(row.Evidence)}",
            unobservedTask:  static row => $"unobserved-task:{Evidence(row.Evidence)}",
            signalled:       static row => $"posix-signal:{row.Signal}",
            hostCrashMarker: static row => $"host-crash-marker:{row.Path}",
            markerDrifted:   static row => $"marker-drifted:{row.Path}:{Evidence(row.Cause)}");

        static string Evidence(FaultObservationWire evidence) {
            string identity = evidence.Code.Match(Some: static code => $"{code}:", None: static () => string.Empty);
            return evidence.Recovery.Switch(
                terminal: _ => $"{identity}terminal",
                transient: _ => $"{identity}transient",
                throttled: row => $"{identity}retry-after-{row.RetryAfter.BclCompatibleTicks}");
        }
    }
}
```

## [03]-[ARTIFACT_ROSTER]

- Owner: `SupportArtifact` the contributor factory row; `CaptureWindow` the per-capture context every producer reads; `ArtifactPayload` the produced bytes beside their masking tally; `MaskTally` the redaction monoid and `MaskLedger` the ONE masking owner every redacted column writes through; `TraceSession` the event-trace pump's own acquisition; the contributed-row carrier is `Runtime/ports#PORT_SURFACE` `SupportContributorPort`, which this owner folds and never re-declares.
- Cases: seven standing rows — effective config, signal readings, phase receipts, health reading, hook faults, determinism drift, the event trace — beside the two dump rows `[04]` mints; each is a factory over composition-supplied ports and none opens a capture window of its own.
- Entry: each `SupportArtifact.<Row>(…)` returns the factory row; `Runtime/ports#PORT_SURFACE` `SupportContributorPort` carries a package's rows inward; `SupportArtifact.Folded(own, contributed)` is the ONE roster projection the runtime binds; `MaskLedger.Mask(object?)` returns the masked text and records its verdict, `MaskLedger.Tally` reads the accumulated count.
- Law: redaction verdicts are kernel `Masked` cases and the tally is their MONOID fold. Three `ref int` counters threaded through every producer are the deleted form — they let two producers disagree on what counts and made a length-preserving redactor report zero over a fully masked bundle — so one ledger owns the verdict, one tally owns the count, and a producer that writes a byte cannot skip either.
- Auto: `IncidentBuffers.Flush` replays both held scopes into the frozen window before contributor fan-in and counts the scopes it drained; the `DeadlineClass.SupportWindow` row bounds the capture run on the cancel spine; the hook rail's parked subscriber faults drain here rather than through a second retention plane, because `FaultCell` is the bounded custody the rail already keeps and a capture is exactly when a late panel gets read; the determinism campaign's placement fold runs as a contributor, so a pulled bundle carries the drift evidence naming any strategy that swallowed an injection.
- Receipt: per-artifact written bytes, truncated bytes, redaction counts, and the archive content key land as `SupportManifest.Entry` rows.
- Packages: Rasm (kernel `Dimension`, `Masked`, `InstrumentTally`/`InstrumentReading`/`ReadingCell`, `FaultCell`), Microsoft.Diagnostics.NETCore.Client, Microsoft.Diagnostics.Tracing.TraceEvent, Microsoft.Extensions.Compliance.Redaction, Microsoft.Extensions.Configuration, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: one `SupportArtifact` factory row lands a new contributor and one inward `SupportContributorPort` lands a whole package's set, which the one `Folded` projection already reads; zero new surface.
- Boundary: classification resolves redaction at row registration, so `Produce` returns only redacted bytes with their tally and no unredacted classified byte reaches assembly; every contributor row runs under its own recovery arm — a faulting `Produce` converts to a zero-byte `SupportFault.ContributorFaulted` manifest entry, so the bundle exports partial with the fault named on its row; `SupportArtifact.Cleanup` is the optional custody row the `[05]` fan folds before bundle sealing; the `EffectiveConfig` row passes the `GetDebugView(Func<ConfigurationDebugViewContext, string>?)` per-value processor through the ledger so each provider value redacts at its origin from the `ConfigurationDebugViewContext.Value`, carrying no unredacted secret — the framework drives that callback itself, which is why the ledger's accumulation seat is one cell inside the owner rather than a `Writer` bind no callback can reach; the `EventTrace` row hands `EventPipeSession.EventStream` to `Microsoft.Diagnostics.Tracing.TraceEvent`'s `EventPipeEventSource(Stream).Process()` on a FORKED pump whose window is the fork's own timeout, so the decode blocks on nothing the capture cannot cancel and a detached `Task.Delay` continuation whose faults nothing observes and whose stop races the dispose is the deleted form, with the admitted `Dimension` supplying both `circularBufferMB` and the artifact estimate so runtime buffering and bundle accounting cannot drift; decode faults map to `SupportFault.DecodeFaulted` and land `SupportReceipt`-partial rather than aborting the bundle; the `.gcdump` heap graph has no reader in the admitted TraceEvent assembly, so the gcdump column binds the `dotnet-gcdump` tool boundary; native-symbol leasing is `Observability/benchmarks#CAPTURE_SEAM` `PerfMapLease`, whose three consumers all live on that page — this owner opens no lease and a declaration here is a symbol seam beside a fan that never brackets a profiled window; the `SignalReadings` row is the capture's MEASUREMENT evidence and reads the kernel `InstrumentTally` alone — a bundle is pulled exactly when the exporter, collector, or store is what failed, so the read plane that answers it composes no exporter and no store, the tally's own lifetime and arming stay the composition root's, and a tally refusal rides the standing contributor recovery arm as a named zero-byte entry rather than a second fault path; contributed ports carry rows and never reach the freeze, redact, or cap law, so a benchmark session lends its event-trace row without opening a second capture window.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------
// Producers read ONE per-capture context, so a dump row reads the escalated policy a watchdog miss deserves
// without a second roster minted per cause.
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

// --- [SERVICES] -----------------------------------------------------------------------------
// Masking has ONE owner, so the config view and the readings render cannot disagree on what counts; the verdict
// is the kernel `Masked` case authored where the transform ran, never a length compare.
public sealed class MaskLedger(Redactor redactor) {
    readonly Atom<MaskTally> tally = Atom(MaskTally.Empty);

    public MaskTally Tally => tally.Value;

    // Absent and empty values mask to the empty string, so no column carries a null and none counts.
    public string Mask(object? value) =>
        value?.ToString() is not { Length: > 0 } text
            ? string.Empty
            : Recorded(RedactedText.Mask(redactor, text));

    string Recorded(Masked verdict) =>
        (ignore(tally.Swap(held => held.Combine(MaskTally.Of(verdict)))), verdict.Value).Item2;
}

// --- [MODELS] -------------------------------------------------------------------------------
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

    // Tag VALUES carry the tenant slug this row is classified for, so redaction runs over values alone; rows the
    // process never measured print `unmeasured`, so a quiet producer and a dead one stay distinguishable.
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

    // Window SUPPLIERS bound the set — never a page-local receipt store — so a capture outside any transition
    // writes an empty artifact rather than the whole process history.
    public static SupportArtifact PhaseReceipts(
        Func<Interval, Seq<PhaseReceipt>> window, JsonTypeInfo<ImmutableArray<PhaseReceipt>> contract) => new(
        Name: "phase-receipts",
        Classification: DataClassification.Operational,
        EstimatedBytes: 32 << 10,
        // Wire collections keep the manifest's own immutable shape, so one contract family covers the archive.
        Produce: capture => IO.lift(() => Serialized([.. window(capture.Frozen)], contract)));

    // One coherent reading, so a bundle never pairs a fresh level against a stale snapshot; the member is named for
    // its READING, so it never captures the `HealthSnapshot` type name inside this declaring type.
    public static SupportArtifact HealthReading(DegradationCell cell, JsonTypeInfo<DegradationReading> contract) => new(
        Name: "health-reading",
        Classification: DataClassification.HostIdentity,
        EstimatedBytes: 16 << 10,
        Produce: _ => IO.lift(() => Serialized(cell.Read(), contract)));

    // Shed and lost counters ride beside the parked rows, so a tap storm reads as a number instead of an absence.
    public static SupportArtifact HookFaults(FaultCell cell, JsonTypeInfo<HookFaultView> contract) => new(
        Name: "hook-faults",
        Classification: DataClassification.Operational,
        EstimatedBytes: 32 << 10,
        Produce: _ => IO.lift(() => Serialized(
            new HookFaultView(
                [.. cell.Parked.Map(static row => new HookFaultRow(
                    row.Point.Value, AppHostFaultMap.Wire(row.Cause), row.At))],
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

    // Decoding blocks until the session stops, so the fork's own cancellation ends it and the bracket stops the
    // session on every exit — a detached delay continuation raced the dispose and observed no faults.
    public static SupportArtifact EventTrace(Seq<EventPipeProvider> providers, Duration window, Dimension circularBufferMiB) => new(
        Name: "event-trace",
        Classification: DataClassification.Operational,
        EstimatedBytes: (long)circularBufferMiB.Value << 20,
        Produce: _ => IO.lift(() => TraceSession.Open(providers, circularBufferMiB)).Bracket(
            Use: opened => Pumped(opened, window),
            Catch: static error => IO.fail<ArtifactPayload>((Error)new SupportFault.DecodeFaulted(error.Message, error)),
            Fin: static opened => IO.lift(() => (fun(opened.Session.Stop)(), unit).Item2)));

    // Cancellation is the only expected exit, so every other error stays on the rail for the bracket's `Catch` arm.
    static IO<ArtifactPayload> Pumped(TraceSession opened, Duration window) =>
        from pump in IO.lift(() => (fun(opened.Source.Process)(), unit).Item2).Fork(window.ToTimeSpan())
        from _ in pump.Await | @catch(static error => error.Is(Errors.Cancelled), static _ => IO.pure(unit))
        select ArtifactPayload.Of(opened.Sink.ToString(), MaskTally.Empty);

    // Contributed rows fold beside this root's own, so the capture reads ONE roster.
    public static Seq<SupportArtifact> Folded(Seq<SupportArtifact> own, params ReadOnlySpan<SupportContributorPort> contributed) =>
        Iterable<SupportContributorPort>.FromSpan(contributed).ToSeq().Fold(own, static (rows, port) => rows + port.Rows);
}

[Equatable]
public sealed partial record HookFaultView(
    [property: OrderedEquality] ImmutableArray<HookFaultRow> Parked, long Shed, long Lost);

public readonly record struct HookFaultRow(string Point, FaultObservationWire Cause, DateTimeOffset At);

// One acquisition value, so the bracket releases session and source together and no arm holds half of it.
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

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
// Custody is the ONE answer both the raw-dump row's existence and its cleanup arm read: a captured minidump owes
// a release and a forked snapshot owes none.
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

// --- [MODELS] -------------------------------------------------------------------------------
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

    // CLR-free images are a real refusal: the bare index turned one into an exception the recovery arm reported as a
    // decode fault, which reads as a corrupt trace rather than a native-only image.
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

    // Existence-guarded, so an eager release and the custody row can both run.
    public static Fin<Unit> Release(string captureRoot) => Op.Of().Catch(() => {
        string path = Path(captureRoot);
        if (System.IO.File.Exists(path)) { System.IO.File.Delete(path); }
        return Fin.Succ(unit);
    });
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class DumpArtifacts {
    extension(SupportArtifact) {
        // Failure releases EAGERLY while custody stays the guaranteed one: the success path must leave the file for
        // `DumpAnalysis`, which is the later reader and owns the terminal release.
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
                // Custody releases on EVERY exit, so cancellation and analysis failure leave the disk as they found it.
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
- Entry: `SupportCapture.Capture(SupportRuntime runtime, SupportTrigger trigger)` returns `IO<SupportReceipt>` — `IO` carries the freeze-fan-redact-cap-bundle effect and the fold opens, marks, and releases its OWN latency ledger.
- Law: the capture fold IS the operation, so it opens its own ledger where the drain conductor and the outbound hop RECEIVE one. That is the discriminant: a fold running inside a caller's operation takes the context that caller already opened, while a fold triggered by a fault commit, a watchdog miss, or a control verb has no ambient operation to take one from — which is why the ledger is a per-call factory column rather than a `SupportRuntime` field a pooled context outlives.
- Law: every `Cleanup` delegate runs EXACTLY once. Folding runs OUTSIDE the compare-and-swap and the settled roster commits through `Cell.Commit`, where a fold inside the CAS body re-ran every release on each contended retry (E-A35).
- Auto: the coalesce gate seats through `Cell.Seat`, so the winner reads `Committed` and every arriving trigger reads `Ceded` with the live correlation on the transition rather than probing the cell a second time; `IncidentBuffers.Flush` replays both held scopes into the frozen window before contributor fan-in and its own scope-count write rides its rail without gating the capture, because an incident bundle is the evidence of the failure being assembled; the artifact cut falls in ONE place, so the manifest's byte count, the content key's preimage, and the zip member are three reads of one projection.
- Receipt: `SupportReceipt` — exported, coalesced, or evicted; the exported case carries the manifest, the bundle path, its total bytes, and the monotonic elapsed span.
- Packages: Rasm (kernel `ContentHash`, `Cell`, `Transition`, `MonotonicTimeline`, `Op`), Microsoft.Extensions.Telemetry.Abstractions, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: one policy value retunes caps or retention; one fault is one `SupportFault` case; one entry state is one `EntryState` case every consumer's `Switch` breaks on; zero new surface.
- Boundary: the `Active` cell is the coalesce gate — a trigger arriving mid-capture folds to `SupportReceipt.Coalesced` and never opens a second window, and only the capture that SEATED the gate clears it; classification resolves redaction at row registration, so no unredacted classified byte reaches assembly; every contributor row runs under its own recovery arm — a faulting `Produce` converts to a zero-byte `SupportFault.ContributorFaulted` entry so the bundle exports partial with the fault named on its row — and a cleanup refusal becomes a zero-byte `SupportFault.CleanupFaulted` row through the SAME refusal mint, since the `-cleanup` slot is the only column the two causes disagree on; `Assemble` brackets the whole contributor fan then folds every cleanup before bundle sealing, so cancellation, a skipped dependent row, or analysis failure cannot bypass staging cleanup; every written row's manifest identity mints through the kernel `ContentHash.Of` over the CAPPED, already-redacted slice — the exact bytes its zip member carries — so the archive de-duplicates and verifies against the payload a reader extracts rather than one the redactor and the cap both moved; a contributor fault, a cleanup refusal, and a bundle-cap drop each write no bytes and therefore carry no key at all, and `EntryState` makes that unspellable rather than disciplined: the empty case has no key column, so a zero-byte row carrying a digest over the empty span — a real key naming a payload the archive never held — has no construction path; elapsed rides the kernel `MonotonicTimeline`, so a wall-clock step during a long capture cannot report a negative or inflated span on the receipt every dashboard reads; the `DeadlineClass.SupportWindow` row bounds the capture run on the cancel spine.

```csharp signature
// --- [ERRORS] ---------------------------------------------------------------------------
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

// --- [MODELS] -------------------------------------------------------------------------------
// Rows either wrote bytes and own their key or wrote none and own none — the `with`-chain that cleared a byte
// count while a key rode along made a forged identity spellable; two cases make it unrepresentable.
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
    Option<FaultObservationWire> Fault, ReadOnlyMemory<byte> Bytes);

public sealed record SupportPolicy(
    Duration Lookback,
    Duration Settle,
    long ArtifactCapBytes,
    long BundleCapBytes,
    int MaxBundles,
    Duration MaxAge) {
    // Cutting happens ONCE: manifest count, keyed preimage, and zip member are three reads of one projection, so a
    // second `long.Min` spelling is how they start describing three different slices.
    public ReadOnlyMemory<byte> Cap(ReadOnlyMemory<byte> payload) =>
        payload[..(int)long.Min(payload.Length, ArtifactCapBytes)];
}

// `Latency` is the per-capture ledger FACTORY the composition binds off `LatencySpine.Open`, and `Watchdog` is
// its enrollment `Runtime/profiles#BOOT_SURFACE` minted — read for completeness, never re-decided here.
public sealed record SupportRuntime(
    SupportPolicy Policy,
    ConsumptionProfile Profile,
    string StorageRoot,
    ImmutableDictionary<string, string> Versions,
    ClockPolicy Clocks,
    IncidentBuffers Buffer,
    InstrumentSet Signals,
    Func<(ILatencyContext Context, CheckpointToken Phase)> Latency,
    Func<Interval, Seq<PhaseReceipt>> Phases,
    Option<WatchdogEnrollment> Watchdog,
    JsonTypeInfo<SupportManifest> ManifestContract,
    Seq<SupportArtifact> Contributors,
    Atom<Option<CorrelationId>> Active);

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class SupportCapture {
    static readonly Op CaptureWork = Op.Of(nameof(Capture));

    public static IO<SupportReceipt> Capture(SupportRuntime runtime, SupportTrigger trigger) =>
        trigger.Facts() switch {
            var facts => IO.lift(() => Cell.Seat(runtime.Active, () => facts.Correlation)).Bracket(
                Use: seat => seat is Transition<Option<CorrelationId>>.Committed
                    ? Ledgered(runtime, facts)
                    // Coalesced triggers open NO ledger and mark NO checkpoint, and the live correlation rides the transition the
                    // seat already answered rather than a second read of the cell.
                    : IO.pure<SupportReceipt>(new SupportReceipt.Coalesced(
                        seat.Current.IfNone(facts.Correlation), facts.Kind)),
                Catch: static error => IO.fail<SupportReceipt>(error),
                Fin: seat => IO.lift(() => Cleared(runtime.Active, seat))),
        };

    static IO<SupportReceipt> Ledgered(SupportRuntime runtime, TriggerFacts facts) =>
        IO.lift(runtime.Latency).Bracket(
            Use: opened => Assemble(runtime, facts, opened.Context, opened.Phase),
            Fin: static opened => IO.lift(() => (fun(opened.Context.Dispose)(), unit).Item2));

    // Only the capture that SEATED the gate clears it, so a coalesced release cannot open a live window.
    static Unit Cleared(Atom<Option<CorrelationId>> gate, Transition<Option<CorrelationId>> seat) =>
        seat is Transition<Option<CorrelationId>>.Committed ? ignore(Cell.Take(gate)) : unit;

    static IO<SupportReceipt> Assemble(
        SupportRuntime runtime, TriggerFacts facts, ILatencyContext latency, CheckpointToken phase) =>
        from at in IO.lift(() => runtime.Clocks.Now)
        from opened in Stamped(runtime)
        let window = new CaptureWindow(
            new Interval(at - facts.Override.IfNone(runtime.Policy.Lookback), at + runtime.Policy.Settle),
            facts,
            Completeness(runtime, facts.Kind))
        // Stamping runs AFTER the freeze and before the fan, so the phase reported is the fan and not the clock read.
        from _marked in IO.lift(() => LatencySpine.Mark(latency, phase))
        // Flush counters ride their own rail: a refused counter is no reason to withhold an incident bundle.
        from _held in IO.lift(() => runtime.Buffer.Flush(runtime.Signals))
        let cleanup = Atom(Seq<ArtifactRow>())
        from produced in IO.pure(unit).Bracket(
            Use: _ => runtime.Contributors.TraverseM(row => Produced(row, window, runtime.Policy)).As(),
            Fin: _ => IO.lift(() => Released(runtime.Contributors, cleanup)))
        let rows = Capped(produced + cleanup.Value, runtime.Policy)
        from receipt in SupportLedger.Bundle(runtime, BundleMap.Manifest(facts, window, rows, runtime), rows, opened)
        select receipt;

    // Watchdog completeness READS the enrollment row rather than re-deciding one; every other cause names its own
    // on the trigger roster and an unenrolled host falls to the dumpless routine.
    static DumpPolicy Completeness(SupportRuntime runtime, SupportTriggerKind kind) =>
        (kind.Dump | runtime.Watchdog.Map(static held => held.Policy)).IfNone(DumpPolicy.Snapshot);

    static IO<MonotonicStamp> Stamped(SupportRuntime runtime) =>
        IO.lift(() => runtime.Clocks.Line.Capture(CaptureWork))
            .Bind(static captured => captured.Match(Succ: IO.pure, Fail: IO.fail<MonotonicStamp>));

    // Per-row recovery is the partial-receipt fold — one row never aborts the capture.
    static IO<ArtifactRow> Produced(SupportArtifact row, CaptureWindow window, SupportPolicy policy) =>
        (row.Produce(window).Map(payload => Written(row, payload, policy))
            | @catch<IO, ArtifactRow>(static _ => true, error => IO.pure(Refused(
                row.Name, row.Classification, new SupportFault.ContributorFaulted(row.Name, error.Message, error))))).As();

    // Cleanup runs EXACTLY once: the fold runs outside the swap and the commit publishes the settled roster, where
    // a fold inside the CAS body re-ran every release on each contended retry.
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

    // ONE refusal arm for both no-byte causes, so the row that wrote nothing carries no key in exactly one place;
    // its `-cleanup` slot is the only column the two disagree on.
    static ArtifactRow Refused(string name, DataClassification classification, SupportFault fault) =>
        new(name, classification, new EntryState.Empty(0L), Redactions: 0,
            Fault: Some(AppHostFaultMap.Wire(fault)), Bytes: ReadOnlyMemory<byte>.Empty);

    // Identity mints over the FINAL slice, cut ONCE: a key over the pre-redaction or pre-cap payload names bytes no
    // reader can extract from the archive it is written into.
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

    // Rows past the budget drop WHOLE, so the key clears with the byte count by CASE rather than a `with`-chain.
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
|  [03]   | phase-receipts    | `PhaseReceipts`   | lifecycle receipts the runtime's window supplier returns             |
|  [04]   | health-reading    | `HealthReading`   | the degradation cell's own coherent reading                          |
|  [05]   | hook-faults       | `HookFaults`      | the rail's `FaultCell` parked rows beside its shed and lost counters |
|  [06]   | determinism-drift | `DriftProbe`      | `AdversarialProbe.Drift` over the recorded chaos chain               |
|  [07]   | process-dump      | `ProcessDump`     | `DiagnosticsClient.WriteDumpAsync` where the source owes custody     |
|  [08]   | dump-triage       | `DumpAnalysis`    | ClrMD `DumpTriage.Walk` rows over the capture's own `DumpSource`     |
|  [09]   | event-trace       | `EventTrace`      | EventPipe session decoded through TraceEvent `EventPipeEventSource`  |

## [06]-[MANIFEST_RECEIPT]

- Owner: `SupportManifest` the wire-neutral manifest; `SupportReceipt` `[Union]` the wire receipt family; `SupportCaptureWire` the flattened export projection the TS census decodes; `BundleMap` the ONE `[Mapper]` over both wire seams; `SupportLedger` the zip-assembly and retention surface.
- Cases: `Exported`, `Coalesced`, `Evicted`.
- Entry: `BundleMap.Manifest(TriggerFacts, CaptureWindow, Seq<ArtifactRow>, SupportRuntime)` projects the manifest and `BundleMap.Export(SupportReceipt.Exported)` the flattened wire face; `SupportLedger.Bundle(...)` returns `IO<SupportReceipt>` writing the zip; `SupportLedger.Sweep(SupportRuntime runtime)` returns `IO<SupportReceipt>` — `IO` carries the retention-eviction effect.
- Auto: `Sweep` registers as one `ScheduleEntry` row carrying the retention-sweep-cadence value at `Runtime/modules#MODULE_LEDGER`; eviction emits `SupportReceipt.Evicted` into the receipt rail with bundle and byte counts; every exported bundle writes the ledger, so the archive on disk and the receipt on the rail are one motion.
- Receipt: `SupportReceipt` is the wire receipt family; the kind discriminator is pinned by `JsonPolymorphic` metadata on the union root.
- Packages: Riok.Mapperly, Thinktecture.Runtime.Extensions, Generator.Equals, NodaTime, LanguageExt.Core, BCL inbox
- Growth: one field row per additive manifest extension, which the mapper's own member matching demands at COMPILE time; one case extends `SupportReceipt` and breaks every consumer arm; zero new surface.
- Boundary: `Bundle` and `Evict` are the named `System.IO` boundary capsules; a bundle captures exactly one process's evidence — cross-process incidents correlate by HLC stamp at the evidence layer, and contributor requests never cross the UDS hop; an exported bundle crosses the receipt rail under `ReceiptKind.Support` and its manifest entries are what the redaction counter partitions on, so the archive's own per-entry mask count is the branch's ONE measured redaction population and no second tally forms beside it; each entry's `ContentKey` is the archive identity every consumer keys on — a bundle de-duplicates against a store, a re-upload proves untampered, and an extracted member verifies against the manifest without re-reading the zip — and a READING consumer re-admits that text through the kernel's own `ContentHash.Admit`, which refuses uppercase, so one key has one text in both directions and no local hex parse forks the round trip; the AppUi `Rasm.AppUi/Document/export#EXPORT_DESTINATIONS` `BundleMember.ContentKey` is the PRE-redaction identity of the contributed payload and the two keys agree exactly where nothing was masked or truncated, so an inequality names redaction or a cap rather than corruption and neither key substitutes for the other; the source-generated `ManifestContract` carries `Entry` whole, so the column rides the one manifest serializer and reaches `SupportCaptureWire.Entries` verbatim with no second wire row; both wire projections ride ONE `[Mapper]` under `RequiredMappingStrategy.Both`, so a manifest column added without its wire member breaks the mapper at compile time where two hand transcriptions of eight and ten fields drifted silently; `SupportCaptureWire` is the flattened export projection the corpus registers at `tests/contracts/MANIFEST.md` `[02.21]-[APPHOST_WIRE]` — the family the TS census decodes carries the bundle FACTS a dashboard reads and never the receipt union, because a coalesced or evicted receipt names no bundle and a decoder branching on a kind discriminant to find three quarters of its fields absent is the shape a flat projection deletes; retention folds rather than accumulating a mutable tuple, so the swept count and its bytes are the fold's own state and a file's size reads before its delete rather than after.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------
[Equatable]
public sealed partial record SupportManifest(
    string Trigger,
    string Reason,
    CorrelationId Correlation,
    Instant WindowStart,
    Instant WindowEnd,
    ConsumptionProfile Profile,
    [property: OrderedEquality] ImmutableArray<SupportManifest.Entry> Entries,
    [property: UnorderedEquality] ImmutableDictionary<string, string> PackageVersions,
    Option<FaultRecord> Fault = default) {
    // `ContentKey` is the seed-zero kernel digest over the bytes THIS entry's zip member carries, rendered as 32
    // lowercase hex because a `UInt128` exceeds the exact-integer range every wire consumer decodes on. The
    // `= default` is the DECODE half of the omission posture: a parameter with no default reads as wire-required.
    public sealed record Entry(string Name, string Classification, long Bytes, long TruncatedBytes, int Redactions,
        Option<string> ContentKey = default, Option<FaultObservationWire> Fault = default) {
        // Extracting readers re-admit through the kernel's own peer, which refuses uppercase — one key, one text.
        public Fin<Option<UInt128>> Verified(Op key) => ContentKey.Traverse(hex => ContentHash.Admit(hex, key)).As();
    }

    public int Redactions => Entries.Sum(static entry => entry.Redactions);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(SupportReceipt.Exported), "exported")]
[JsonDerivedType(typeof(SupportReceipt.Coalesced), "coalesced")]
[JsonDerivedType(typeof(SupportReceipt.Evicted), "evicted")]
public abstract partial record SupportReceipt {
    private SupportReceipt() { }
    public sealed record Exported(SupportManifest Manifest, string BundlePath, long TotalBytes, Duration Elapsed) : SupportReceipt;
    public sealed record Coalesced(CorrelationId Active, SupportTriggerKind FoldedKind) : SupportReceipt;
    public sealed record Evicted(int Bundles, long Bytes, Instant At) : SupportReceipt;
}

[Equatable]
public readonly partial record struct SupportCaptureWire(
    string Trigger,
    string Reason,
    CorrelationId Correlation,
    Instant WindowStart,
    Instant WindowEnd,
    string BundlePath,
    long TotalBytes,
    Duration Elapsed,
    int Redactions,
    [property: OrderedEquality] ImmutableArray<SupportManifest.Entry> Entries,
    Option<FaultRecord> Fault = default);

// --- [BOUNDARIES] ---------------------------------------------------------------------------
// `RequiredMappingStrategy.Both` makes target completeness a COMPILE proof, so a manifest column added without
// its wire member breaks here (RMG012/RMG013) where two hand transcriptions drifted silently.
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Both,
        EnabledConversions = MappingConversionType.All & ~MappingConversionType.ExplicitCast)]
internal static partial class BundleMap {
    public static SupportManifest Manifest(
        TriggerFacts facts, CaptureWindow window, Seq<ArtifactRow> rows, SupportRuntime runtime) =>
        new(
            Trigger: facts.Kind.Key,
            Reason: facts.Reason,
            Correlation: facts.Correlation,
            WindowStart: window.Frozen.Start,
            WindowEnd: window.Frozen.End,
            Profile: runtime.Profile,
            Entries: [.. rows.Map(Entry)],
            PackageVersions: runtime.Versions,
            Fault: facts.Fault);

    // State projections flatten onto the wire columns, so the discriminant lives on the interior carrier alone.
    [MapProperty(nameof(ArtifactRow.State) + "." + nameof(EntryState.Bytes), nameof(SupportManifest.Entry.Bytes))]
    [MapProperty(nameof(ArtifactRow.State) + "." + nameof(EntryState.TruncatedBytes), nameof(SupportManifest.Entry.TruncatedBytes))]
    [MapProperty(nameof(ArtifactRow.State) + "." + nameof(EntryState.ContentKey), nameof(SupportManifest.Entry.ContentKey))]
    [MapProperty(nameof(ArtifactRow.Classification) + "." + nameof(DataClassification.Key), nameof(SupportManifest.Entry.Classification))]
    private static partial SupportManifest.Entry Entry(ArtifactRow row);

    [MapNestedProperties(nameof(SupportReceipt.Exported.Manifest))]
    public static partial SupportCaptureWire Export(SupportReceipt.Exported exported);
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class SupportLedger {
    static readonly Op BundleWork = Op.Of(nameof(Bundle));

    public static IO<SupportReceipt> Sweep(SupportRuntime runtime) =>
        IO.lift(() => runtime.Clocks.Now).Map(at => Swept(runtime, at));

    internal static IO<SupportReceipt> Bundle(
        SupportRuntime runtime, SupportManifest manifest, Seq<ArtifactRow> rows, MonotonicStamp opened) =>
        from path in IO.lift(() => Written(runtime, manifest, rows))
        from elapsed in Spanned(runtime.Clocks, opened)
        select (SupportReceipt)new SupportReceipt.Exported(
            manifest, path, new FileInfo(path).Length, Duration.FromTimeSpan(elapsed));

    // Elapsed rides the kernel timeline, so a clock step during a long capture cannot report a negative span.
    static IO<TimeSpan> Spanned(ClockPolicy clocks, MonotonicStamp opened) =>
        IO.lift(() => clocks.Line.Capture(BundleWork)
                .Bind(closed => clocks.Line.Elapsed(opened, closed, BundleWork)))
            .Bind(static measured => measured.Match(Succ: IO.pure, Fail: IO.fail<TimeSpan>));

    // Named `System.IO` capsule: the archive's stream nesting is this page's statement region.
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

    // Rank past the ceiling and age past the cutoff are ONE predicate, so the swept count is the fold's own state.
    static SupportReceipt Swept(SupportRuntime runtime, Instant at) =>
        toSeq(new DirectoryInfo(runtime.StorageRoot).EnumerateFiles("*.zip")
                .OrderByDescending(static file => file.CreationTimeUtc))
            .Map(static (file, rank) => (File: file, Rank: rank))
            .Filter(row => row.Rank >= runtime.Policy.MaxBundles
                || row.File.CreationTimeUtc < (at - runtime.Policy.MaxAge).ToDateTimeOffset().UtcDateTime)
            .Fold((Bundles: 0, Bytes: 0L), static (swept, row) =>
                (swept.Bundles + 1, swept.Bytes + Released(row.File)))
        switch {
            var swept => new SupportReceipt.Evicted(swept.Bundles, swept.Bytes, at),
        };

    // Size reads BEFORE the delete — `FileInfo.Length` throws once the entry is gone.
    static long Released(FileInfo file) {
        long size = file.Length;
        file.Delete();
        return size;
    }
}
```

## [07]-[TS_PROJECTION]

- Owner: `SupportManifest`, `SupportReceipt`, and the `SupportCaptureWire` export projection
- Packages: NodaTime.Serialization.SystemTextJson, Thinktecture.Runtime.Extensions.Json, BCL inbox
- Growth: one field row per additive manifest extension, which the flat export projection inherits through the one mapper; the TS dashboard tolerates additive fields — zero new surface.
- Boundary: instants and durations serialize as ISO-8601 text through the NodaTime converters; correlation, profile, and the trigger kind serialize as their keys through the generated Thinktecture converters, so the six kind keys cross as the same text the roster declares; the entry content key serializes as its 32-digit lowercase hex projection, which is where the kernel's `UInt128` identity currency crosses into a wire encoding — a numeric slot loses the low bits at every decoder this family reaches — and its `Option` slot OMITS on absence through the suite's `Runtime/ports#WIRE_LAW` contract modifier, so an entry that wrote no bytes carries no key property at all rather than a null one; record property names ride the camelCase wire policy and the receipt kind discriminator is the `JsonPolymorphic` metadata property.

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
  readonly fault?: FaultObservationWire;
}

type FaultRecord =
  | { readonly kind: "unhandled"; readonly evidence: FaultObservationWire; readonly termination: "terminating" | "observed" }
  | { readonly kind: "unobserved-task"; readonly evidence: FaultObservationWire }
  | { readonly kind: "posix-signal"; readonly signal: string }
  | { readonly kind: "host-crash-marker"; readonly path: string; readonly marker?: BootMarker }
  | { readonly kind: "marker-drifted"; readonly path: string; readonly cause: FaultObservationWire };

interface BootMarker {
  readonly pid: number;
  readonly phase: string;
  readonly appVersion: string;
  readonly startedAt: string;
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
  readonly fault?: FaultRecord;
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
  readonly fault?: FaultRecord;
}
```

## [08]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
