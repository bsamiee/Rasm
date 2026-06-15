# [APPHOST_SUPPORT_BUNDLES]

Support capture is the runtime spine's bounded diagnostic evidence surface: one SupportTrigger union admits every capture cause, one capture fold freezes the evidence window, fans contributor artifact rows in declared order, redacts by classification before any byte is written, caps with truncation receipts, and lands one zip whose wire-neutral manifest and export receipt the dashboard ingests unchanged. The page owns the trigger axis, the artifact-row vocabulary, the capture and retention policy values, and the manifest and receipt wire shapes. A bundle is process-local evidence; cross-process incidents correlate by HLC stamp at the evidence layer.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]        | [OWNS]                                                             |
| :-----: | :--------------- | :----------------------------------------------------------------- |
|   [1]   | TRIGGER_UNION    | Six capture causes, one sealed union, typed reason payloads        |
|   [2]   | CAPTURE_PIPELINE | Window freeze, ordered fan-in, redaction before write, caps        |
|   [3]   | MANIFEST_RECEIPT | Zip assembly, wire manifest, receipt union, retention, process law |
|   [4]   | TS_PROJECTION    | Manifest and receipt wire shapes the TS dashboard ingests          |

## [2]-[TRIGGER_UNION]

- Owner: `SupportTrigger` [Union]
- Cases: UserRequested · FaultTransition · HealthThreshold · WatchdogTimeout · ExternalCommand · Scheduled
- Auto: FaultTransition auto-arms on every FaultSource entry including the host-crash-marker boot probe; WatchdogTimeout fires on a missed heartbeat deadline and Scheduled fires from a ScheduleEntry row on the schedule port; ExternalCommand admits the ControlService capture-support verb for service modalities.
- Packages: Thinktecture.Runtime.Extensions, NodaTime, LanguageExt.Core
- Growth: one case lands a new capture cause and breaks the Facts dispatch at compile time — zero new surface.
- Boundary: the private root constructor plus deleted value conversion seal ingress; fault, health, and schedule causes carry their typed evidence whole, and rendering happens exactly once inside the total Facts dispatch.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SupportTrigger {
    private SupportTrigger() { }
    public sealed record UserRequested(CorrelationId Correlation, string Reason, Option<Duration> WindowOverride = default) : SupportTrigger;
    public sealed record FaultTransition(CorrelationId Correlation, FaultSource Fault, Option<Duration> WindowOverride = default) : SupportTrigger;
    public sealed record HealthThreshold(CorrelationId Correlation, DegradationLevel Level, Option<Duration> WindowOverride = default) : SupportTrigger;
    public sealed record WatchdogTimeout(CorrelationId Correlation, ScheduleEntry Entry, Option<Duration> WindowOverride = default) : SupportTrigger;
    public sealed record ExternalCommand(CorrelationId Correlation, string Reason, Option<Duration> WindowOverride = default) : SupportTrigger;
    public sealed record Scheduled(CorrelationId Correlation, ScheduleEntry Entry, Option<Duration> WindowOverride = default) : SupportTrigger;
}

public static class SupportTriggerOps {
    extension(SupportTrigger trigger) {
        public (CorrelationId Correlation, string Kind, string Reason, Option<Duration> Override) Facts() => trigger.Switch(
            userRequested:   static u => (u.Correlation, "user-requested", u.Reason, u.WindowOverride),
            faultTransition: static f => (f.Correlation, "fault-transition", f.Fault.ToString(), f.WindowOverride),
            healthThreshold: static h => (h.Correlation, "health-threshold", h.Level.ToString(), h.WindowOverride),
            watchdogTimeout: static w => (w.Correlation, "watchdog-timeout", w.Entry.ToString(), w.WindowOverride),
            externalCommand: static e => (e.Correlation, "external-command", e.Reason, e.WindowOverride),
            scheduled:       static s => (s.Correlation, "scheduled", s.Entry.ToString(), s.WindowOverride));
    }
}
```

## [3]-[CAPTURE_PIPELINE]

- Owner: `SupportCapture`
- Entry: `public static IO<SupportReceipt> Capture(SupportRuntime runtime, SupportTrigger trigger)` — IO rail
- Auto: the GlobalLogBuffer Flush replays the fault buffer into the frozen window before contributor fan-in; the support-window deadline class bounds the capture run on the cancel spine.
- Receipt: per-artifact written bytes, truncated bytes, and redaction counts land as manifest entries.
- Packages: Microsoft.Extensions.Telemetry.Abstractions, Microsoft.Extensions.Compliance.Redaction, Microsoft.Extensions.Configuration, LanguageExt.Core, NodaTime
- Growth: one artifact row lands a new contributor — the process-dump row will land this way once the diagnostics-tool gate clears; zero new surface.
- Boundary: the Active cell is the coalesce gate — a trigger arriving mid-capture folds to SupportReceipt.Coalesced and never opens a second window; classification resolves redaction at row registration, so Produce returns only redacted bytes with their redaction count and no unredacted classified byte reaches assembly.

```csharp signature
public sealed record SupportArtifact(
    string Name,
    DataClassification Classification,
    long EstimatedBytes,
    Func<Interval, IO<(ReadOnlyMemory<byte> Bytes, int Redactions)>> Produce);

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
|   [1]   | window-lookback         | 10 minutes | transition     |
|   [2]   | window-settle           | 30 seconds | transition     |
|   [3]   | artifact-cap            |     16 MiB | transition     |
|   [4]   | bundle-cap              |    128 MiB | transition     |
|   [5]   | retention-max-bundles   |         16 | transition     |
|   [6]   | retention-max-age       |    30 days | transition     |
|   [7]   | retention-sweep-cadence |   `@daily` | transition     |

Canonical AppHost artifact rows are current; `process-dump` is the designed capture row. Sibling packages add rows through ordered contributor descriptors.

| [INDEX] | [ARTIFACT]       | [PRODUCER]                              |
| :-----: | :--------------- | :-------------------------------------- |
|   [1]   | effective-config | redacted configuration debug view       |
|   [2]   | buffered-logs    | profile log pipeline fault buffer       |
|   [3]   | phase-receipts   | lifecycle receipts in the frozen window |
|   [4]   | health-snapshot  | latest health fold                      |
|   [5]   | process-dump     | dump and gcdump capture                 |

## [4]-[MANIFEST_RECEIPT]

- Owner: `SupportManifest` · `SupportReceipt` [Union] · `SupportLedger`
- Cases: Exported · Coalesced · Evicted
- Entry: `public static IO<SupportReceipt> Sweep(SupportRuntime runtime)` — IO rail
- Auto: Sweep registers as one ScheduleEntry row carrying the retention-sweep-cadence value; eviction emits SupportReceipt.Evicted into the receipt rail with bundle and byte counts.
- Receipt: SupportReceipt is the wire receipt family; the kind discriminator is pinned by JsonPolymorphic metadata on the union root.
- Packages: Thinktecture.Runtime.Extensions, NodaTime, BCL inbox
- Growth: one policy value retunes caps or retention; one case extends SupportReceipt and breaks every consumer arm at compile time — zero new surface.
- Boundary: Bundle and Evict are the named System.IO boundary capsules and carry statement bodies; a bundle captures exactly one process's evidence — cross-process incidents correlate by HLC stamp at the evidence layer, and contributor requests never cross the UDS hop.

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

## [5]-[TS_PROJECTION]

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

## [6]-[RESEARCH]

- [DUMP_ADMISSION]: dump and gcdump capture-tool admission for the process-dump row.
