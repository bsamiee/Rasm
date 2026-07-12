# [APPHOST_SUPPORT_BUNDLES]

Support capture is the runtime spine's bounded diagnostic evidence surface: one `SupportTrigger` union admits every capture cause, one capture fold freezes the evidence window, fans contributor artifact rows in declared order, redacts by classification before any byte is written, caps with truncation receipts, and lands one zip whose wire-neutral manifest and export receipt the dashboard ingests unchanged. The page owns the trigger axis, the artifact-row vocabulary, the capture and retention policy values, and the manifest and receipt wire shapes. A bundle is process-local evidence; cross-process incidents correlate by HLC stamp at the evidence layer.

## [01]-[INDEX]

- [01]-[TRIGGER_UNION]: Six capture causes as one sealed union with typed reason payloads.
- [02]-[CAPTURE_PIPELINE]: Window freeze, ordered fan-in, redaction before write, and caps.
- [03]-[MANIFEST_RECEIPT]: Zip assembly, wire manifest, receipt union, retention, and process law.
- [04]-[TS_PROJECTION]: Manifest and receipt wire shapes the TS dashboard ingests.

## [02]-[TRIGGER_UNION]

- Owner: `SupportTrigger` `[Union]` six capture-cause cases.
- Cases: `UserRequested`, `FaultTransition`, `HealthThreshold`, `WatchdogTimeout`, `ExternalCommand`, `Scheduled`.
- Auto: `FaultTransition` carries the wire-stable `FaultRecord` the `Runtime/lifecycle#FAULT_SPINE` `FaultRecord.From` flatten produces — the one fault-to-capture fact `FaultSpine.ArmTraps` emits for every `FaultSource` entry, the live unhandled/unobserved/signalled commits and the `ProbeMarkers` host-crash-marker boot probe alike, so a fault commit and its capture trigger are one fact rather than an untyped capture delegate beside a `PhaseTrigger.FaultCommitted` emission; the case holds `FaultRecord` (kind-discriminated, `Error`-free) so the trigger payload is the exact shape the bundle manifest serializes, never the live `Error`-bearing `FaultSource`; `WatchdogTimeout` fires on a missed heartbeat deadline and `Scheduled` fires from a `ScheduleEntry` row on the schedule port; `ExternalCommand` admits the `ControlService` capture-support verb for service modalities.
- Packages: Thinktecture.Runtime.Extensions, NodaTime, LanguageExt.Core
- Growth: one case lands a new capture cause and breaks the `Facts` dispatch at compile time; a new fault cause is one `FaultSource` case the `FaultRecord.From` flatten and the one `FaultTransition` payload both absorb, never a second trigger case per fault kind — zero new surface.
- Boundary: the private root constructor plus deleted value conversion seal ingress; fault, health, and schedule causes carry their typed evidence whole, and rendering happens exactly once inside the total `Facts` dispatch; the `FaultTransition` payload is the wire-stable `FaultRecord` whose `kind` literals (`unhandled`/`unobserved-task`/`posix-signal`/`host-crash-marker`) the `Facts` rendering reads, so the durable-orchestration crash-recovery (`Runtime/orchestration#CRASH_RESUME`) and the bundle manifest read one kind-discriminated fault fact, and a flattened trigger that loses the `FaultRecord` kind fields is the deleted form.

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

        // The fault reason carries the FaultRecord kind literal plus its wire-stable payload, so the
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

- Owner: `SupportCapture` — the window-freeze, ordered fan-in, redact, and cap fold; `SupportArtifact` the contributor factory row; `SupportFault` `[Union]` fault family deriving its codes through `FaultBand.Support`; `DumpPolicy` the dump-completeness policy row; `SupportPolicy` and `SupportRuntime` the bound capture context.
- Entry: `Capture(SupportRuntime runtime, SupportTrigger trigger)` returns `IO<SupportReceipt>` — `IO` carries the freeze-fan-redact-cap-bundle effect.
- Auto: `GlobalLogBuffer.Flush` replays the fault buffer into the frozen window before contributor fan-in; the `DeadlineClass.SupportWindow` row bounds the capture run on the cancel spine.
- Receipt: per-artifact written bytes, truncated bytes, and redaction counts land as `SupportManifest.Entry` rows.
- Packages: Microsoft.Diagnostics.NETCore.Client, Microsoft.Diagnostics.Tracing.TraceEvent, Microsoft.Extensions.Telemetry.Abstractions, Microsoft.Extensions.Compliance.Redaction, Microsoft.Extensions.Configuration, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: one `SupportArtifact` factory row lands a new contributor; a new dump completeness is one `DumpPolicy` value (`Triage` routine, `WithHeap`/`Full` escalation-only); a new fault is one `SupportFault` case; zero new surface.
- Boundary: the `Active` cell is the coalesce gate — a trigger arriving mid-capture folds to `SupportReceipt.Coalesced` and never opens a second window; classification resolves redaction at row registration, so `Produce` returns only redacted bytes with their redaction count and no unredacted classified byte reaches assembly; the `EffectiveConfig` row passes the `GetDebugView(Func<ConfigurationDebugViewContext, string>?)` per-value processor through the resolved `Redactor` so each provider value redacts at its origin from the `ConfigurationDebugViewContext.Value` and the redaction count rises per masked entry, carrying no unredacted secret; the `ProcessDump` row composes `Microsoft.Diagnostics.NETCore.Client` — `DiagnosticsClient.WriteDump(DumpType, path, WriteDumpFlags)` captures under the frozen window with completeness as `DumpPolicy` row data, `ServerNotAvailableException`/IO faults mapping to the typed `SupportFault.DumpRejected` — and the `EventTrace` row hands `EventPipeSession.EventStream` to `Microsoft.Diagnostics.Tracing.TraceEvent`'s `EventPipeEventSource(Stream).Process()` on a dedicated pump inside the `DeadlineClass.SupportWindow` bound, decode faults mapping to `SupportFault.DecodeFaulted` and landing `SupportReceipt`-partial rather than aborting the bundle; the `.gcdump` heap graph has NO reader in the admitted TraceEvent assembly, so the gcdump column binds the `dotnet-gcdump` TOOL boundary — deleting the column is capability deletion, the forbidden form.

```csharp signature
public sealed record SupportArtifact(
    string Name,
    DataClassification Classification,
    long EstimatedBytes,
    Func<Interval, IO<(ReadOnlyMemory<byte> Bytes, int Redactions)>> Produce) {
    public static SupportArtifact EffectiveConfig(IConfigurationRoot root, Redactor redactor) => new(
        Name: "effective-config",
        Classification: DataClassification.Operational,
        EstimatedBytes: 64 << 10,
        Produce: _ => IO.lift(() => {
            var redactions = 0;
            string Mask(ConfigurationDebugViewContext entry) {
                if (entry.Value is not { Length: > 0 } value) return entry.Value ?? string.Empty;
                var masked = redactor.Redact(value);
                if (masked.Length != value.Length) redactions++;
                return masked;
            }
            var view = root.GetDebugView(Mask);
            return (new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(view)), redactions);
        }));

    // The [DUMP_ADMISSION] fill: DiagnosticsClient.WriteDump captures under the frozen window with
    // completeness as row policy; a capture-tool fault is the typed registry-banded case, never a
    // bare Error.New and never an orphan code outside every band.
    public static SupportArtifact ProcessDump(DumpPolicy policy, string captureRoot) => new(
        Name: "process-dump",
        Classification: DataClassification.HostIdentity,
        EstimatedBytes: policy.EstimatedBytes,
        Produce: _ => IO.lift(() => {
            var path = Path.Join(captureRoot, $"dump-{Environment.ProcessId}.dmp");
            new DiagnosticsClient(Environment.ProcessId).WriteDump(policy.Kind, path, policy.Flags);
            return (new ReadOnlyMemory<byte>(File.ReadAllBytes(path)), 0);
        }).MapFail(static error => (Error)new SupportFault.DumpRejected(error.Message)));

    // The event-STREAM leg: an EventPipe session decodes through TraceEvent's EventPipeEventSource on a
    // dedicated pump; records clone before retention and a decode fault lands the bundle PARTIAL.
    public static SupportArtifact EventTrace(Seq<EventPipeProvider> providers, Duration window) => new(
        Name: "event-trace",
        Classification: DataClassification.Operational,
        EstimatedBytes: 32L << 20,
        Produce: _ => IO.lift(() => {
            using var session = new DiagnosticsClient(Environment.ProcessId).StartEventPipeSession([.. providers], requestRundown: false);
            var sink = new StringBuilder();
            var source = new EventPipeEventSource(session.EventStream);
            source.Dynamic.All += evt => sink.AppendLine($"{evt.TimeStamp:O} {evt.ProviderName}/{evt.EventName}");
            ignore(Task.Delay(window.ToTimeSpan()).ContinueWith(_ => session.Stop()));
            source.Process();
            return (new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(sink.ToString())), 0);
        }).MapFail(static error => (Error)new SupportFault.DecodeFaulted(error.Message)));
}

// Dump completeness is policy DATA: Triage is the routine row, WithHeap/Full escalation-only.
public sealed record DumpPolicy(DumpType Kind, WriteDumpFlags Flags, long EstimatedBytes) {
    public static readonly DumpPolicy Routine = new(DumpType.Triage, WriteDumpFlags.None, 64L << 20);
    public static readonly DumpPolicy Escalated = new(DumpType.WithHeap, WriteDumpFlags.None, 512L << 20);
}

[Union]
public abstract partial record SupportFault : Expected, IValidationError<SupportFault> {
    private SupportFault(string detail, int code) : base(detail, code, None) { }
    public static SupportFault Create(string message) => new Text(message);
    public sealed record Text : SupportFault { public Text(string detail) : base(detail, FaultBand.Support.Code(0)) { } }
    public sealed record DumpRejected : SupportFault { public DumpRejected(string detail) : base(detail, FaultBand.Support.Code(1)) { } }
    public sealed record DecodeFaulted : SupportFault { public DecodeFaulted(string detail) : base(detail, FaultBand.Support.Code(2)) { } }
    public sealed record ContributorFaulted : SupportFault { public ContributorFaulted(string artifact, string detail) : base($"{artifact}: {detail}", FaultBand.Support.Code(3)) { } }
}

public sealed record SupportPolicy(
    Duration Lookback,
    Duration Settle,
    long ArtifactCapBytes,
    long BundleCapBytes,
    int MaxBundles,
    Duration MaxAge);

public sealed record SupportRuntime(
    SupportPolicy Policy,
    HostProfile Profile,
    string StorageRoot,
    ImmutableDictionary<string, string> Versions,
    ClockPolicy Clocks,
    GlobalLogBuffer Buffer,
    JsonTypeInfo<SupportManifest> ManifestContract,
    Seq<SupportArtifact> Contributors,
    Atom<Option<CorrelationId>> Active);

public static class SupportCapture {
    public static IO<SupportReceipt> Capture(SupportRuntime runtime, SupportTrigger trigger) =>
        from facts in IO.pure(trigger.Facts())
        from receipt in IO.lift(() => runtime.Active.Swap(gate => gate.IsNone ? Optional(facts.Correlation) : gate))
            .Bracket(
                Use: gate => gate is { IsSome: true, Case: CorrelationId owner } && owner == facts.Correlation
                    ? Assemble(runtime, facts)
                    : IO.pure<SupportReceipt>(new SupportReceipt.Coalesced(gate.IfNone(facts.Correlation), facts.Kind)),
                Fin: _ => IO.lift(() => ignore(runtime.Active.Swap(gate =>
                    gate is { IsSome: true, Case: CorrelationId owner } && owner == facts.Correlation
                        ? Option<CorrelationId>.None
                        : gate))))
        select receipt;

    static IO<SupportReceipt> Assemble(
        SupportRuntime runtime,
        (CorrelationId Correlation, string Kind, string Reason, Option<Duration> Override) facts) =>
        from at in IO.lift(() => runtime.Clocks.Now)
        from mark in IO.lift(runtime.Clocks.Mark)
        let opened = at - facts.Override.IfNone(runtime.Policy.Lookback)
        let closed = at + runtime.Policy.Settle
        let window = new Interval(opened, closed)
        from _ in IO.lift(fun(runtime.Buffer.Flush))
        from produced in runtime.Contributors
            .TraverseM(row => row.Produce(window).Map(payload => Written(row, payload, runtime.Policy)))
            .As()
        let rows = Capped(produced, runtime.Policy)
        from receipt in SupportLedger.Bundle(runtime, SupportManifest.From(facts, opened, closed, rows, runtime), rows, mark)
        select receipt;

    static (SupportManifest.Entry Entry, ReadOnlyMemory<byte> Bytes) Written(
        SupportArtifact row,
        (ReadOnlyMemory<byte> Bytes, int Redactions) payload,
        SupportPolicy policy) =>
        (new SupportManifest.Entry(
             Name: row.Name,
             Classification: row.Classification.ToString(),
             Bytes: long.Min(payload.Bytes.Length, policy.ArtifactCapBytes),
             TruncatedBytes: long.Max(payload.Bytes.Length - policy.ArtifactCapBytes, 0L),
             Redactions: payload.Redactions),
         payload.Bytes[..(int)long.Min(payload.Bytes.Length, policy.ArtifactCapBytes)]);

    static Seq<(SupportManifest.Entry Entry, ReadOnlyMemory<byte> Bytes)> Capped(
        Seq<(SupportManifest.Entry Entry, ReadOnlyMemory<byte> Bytes)> produced,
        SupportPolicy policy) =>
        produced.Fold(
            (Total: 0L, Rows: Seq<(SupportManifest.Entry Entry, ReadOnlyMemory<byte> Bytes)>()),
            (acc, row) => acc.Total + row.Bytes.Length > policy.BundleCapBytes
                ? (acc.Total, acc.Rows.Add((row.Entry with { Bytes = 0L, TruncatedBytes = row.Entry.Bytes }, ReadOnlyMemory<byte>.Empty)))
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

Canonical AppHost artifact rows are current; `process-dump` is the designed capture row. Sibling packages add rows through ordered contributor descriptors.

| [INDEX] | [ARTIFACT]       | [PRODUCER]                                                                                     |
| :-----: | :--------------- | :--------------------------------------------------------------------------------------------- |
|  [01]   | effective-config | redacted configuration debug view                                                              |
|  [02]   | buffered-logs    | profile log pipeline fault buffer                                                              |
|  [03]   | phase-receipts   | lifecycle receipts in the frozen window                                                        |
|  [04]   | health-snapshot  | latest health fold                                                                             |
|  [05]   | process-dump     | `DiagnosticsClient.WriteDump` under `DumpPolicy`; gcdump via the `dotnet-gcdump` tool boundary |
|  [06]   | event-trace      | EventPipe session decoded through TraceEvent `EventPipeEventSource`                            |

## [04]-[MANIFEST_RECEIPT]

- Owner: `SupportManifest` the wire-neutral manifest; `SupportReceipt` `[Union]` the wire receipt family; `SupportLedger` the zip-assembly and retention surface.
- Cases: `Exported`, `Coalesced`, `Evicted`.
- Entry: `Sweep(SupportRuntime runtime)` returns `IO<SupportReceipt>` — `IO` carries the retention-eviction effect.
- Auto: `Sweep` registers as one `ScheduleEntry` row carrying the retention-sweep-cadence value; eviction emits `SupportReceipt.Evicted` into the receipt rail with bundle and byte counts.
- Receipt: `SupportReceipt` is the wire receipt family; the kind discriminator is pinned by `JsonPolymorphic` metadata on the union root.
- Packages: Thinktecture.Runtime.Extensions, NodaTime, BCL inbox
- Growth: one policy value retunes caps or retention; one case extends `SupportReceipt` and breaks every consumer arm at compile time — zero new surface.
- Boundary: `Bundle` and `Evict` are the named `System.IO` boundary capsules and carry statement bodies; a bundle captures exactly one process's evidence — cross-process incidents correlate by HLC stamp at the evidence layer, and contributor requests never cross the UDS hop.

```csharp signature
public sealed record SupportManifest(
    string Trigger,
    string Reason,
    CorrelationId Correlation,
    Instant WindowStart,
    Instant WindowEnd,
    HostProfile Profile,
    ImmutableArray<Entry> Entries,
    ImmutableDictionary<string, string> PackageVersions) {
    public sealed record Entry(string Name, string Classification, long Bytes, long TruncatedBytes, int Redactions);

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

- Owner: `SupportManifest` and `SupportReceipt` wire shapes
- Packages: NodaTime.Serialization.SystemTextJson, Thinktecture.Runtime.Extensions.Json, BCL inbox
- Growth: one field row per additive manifest extension; the TS dashboard tolerates additive fields — zero new surface.
- Boundary: instants and durations serialize as ISO-8601 text through the NodaTime converters; correlation and profile serialize as their keys through the generated Thinktecture converters; record property names ride the camelCase wire policy and the receipt kind discriminator is the JsonPolymorphic metadata property.

```ts contract
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
```

## [06]-[RESEARCH]

- [DUMP_ADMISSION]: RESOLVED — the process-dump row composes `Microsoft.Diagnostics.NETCore.Client` (`DiagnosticsClient(int)`, `WriteDump(DumpType, string, WriteDumpFlags)`, `StartEventPipeSession`, `EventPipeSession.EventStream`) and the event-trace row decodes through `Microsoft.Diagnostics.Tracing.TraceEvent` (`EventPipeEventSource(Stream)`, `TraceEventDispatcher.Process()`), both inside the capture fan's caps/redaction/truncation law; the `.gcdump` heap-graph read has no owner in the admitted 3.2.4 assembly (`DotNetHeapDumpGraphReader`/`GCHeapDump`/`MemoryGraph` absent — the recorded REJECT in `api-traceevent.md`), so the gcdump column binds the `dotnet-gcdump` tool boundary.
