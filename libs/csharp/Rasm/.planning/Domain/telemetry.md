# [RASM_TELEMETRY]

`Rasm.Domain` owns the branch's one OTel-free signal fabric: the kernel sub-domain roster every span source and hook-point id derives from, the causal-edge vocabulary a bracket carries, the span band that conforms to the hook rail's own bracket floor, the per-operation cost evidence, the one emission entry that publishes a fact and meters it, and the contributor port a stratum hands a composing root. One home holds the fabric TYPE; a stratum above composes an INSTANCE against its own fact union, its own instrument roster, and its own objectives.

Every owner is instance-owned and composition-entered — evidence cell, meter, rail, and band arrive from the composing app, so two compositions never contend for one slot. Dependency split draws the boundary: this page carries `System.Diagnostics`, `System.Diagnostics.Metrics`, LanguageExt, and Thinktecture, while OTel-SDK wiring, exporters, sampling, resource identity, foreign-source rows, and the OTel baggage store stay at the app platform. Fire is synchronous from any stratum, and every owner this page composes downward — the hook capsule, the causal frame, the measurement plane, the reliability algebra — is declared on its own page and re-spelled here nowhere.

## [01]-[INDEX]

- [02]-[CAPSULE]: `KernelDomain`, `TraceCarrier`, `SpanEdge`, `SpanBand` — the sub-domain roster, the causal-edge vocabulary, and the span band that conforms to `IHookSpan`.
- [03]-[COST]: `Outcome`, `OpCost`, `CostMark` — the settled-verdict row and the per-operation billing capture, declared ahead of the tap that meters them.
- [04]-[TAP]: `FaultObservation`, `PointFacet`, `KernelPoint`, `SignalFact`, `SignalRail`, `KernelInstrument`, `TelemetrySink` — structured fault projection, hook roster, fact vocabulary, emission rail, instruments, and the ONE emission entry.
- [05]-[CONTRIBUTE]: `ArmKey`, `ArmRequest`, `InstrumentArm`, `ReceiptFan`, `ClassifiedValue`, `Sensitivity`, `TelemetryContributorPort` — the one projection dispatch and the one downward contribution fact.

## [02]-[CAPSULE]

- Owner: `KernelDomain` rows derive both `SourceName` and the prefix `KernelPoint` seats its ids on off one row key — span source and hook-point prefix are ONE derivation, never two spellings — and the scope projects through an `Items`-derived frozen index so a hot bracket pays a lookup rather than a re-parse; `TraceCarrier` is the one causal-edge owner, capturing the W3C pair where a producing span is live and projecting it back as the parent an ingress adopts or the link a batch fans in on; `SpanEdge` is the one bracket carriage binding span kind, that parent, and that edge set; `SpanBand` owns every admitted scope's `ActivitySource` and conforms to the hook rail's bracket floor.
- Cases: three span shapes off one carriage — a descendant bracket taking the carriage default, an ingress bracket adopting one inbound parent under a consumer or server kind, and a fan-in bracket carrying one link per upstream operation. Two rail shapes: a synchronous `Fin` arm brackets with `using`, an effectful `IO` arm brackets through `IO`, and both resolve the same admitted-scope table and the same carriage.
- Entry: `TraceCarrier.Of(Activity?)` captures an edge, `Parent` reconstructs it and owns the ONE parse on this fabric, `Link(facts)` projects a fan-in edge over that same parse; `SpanEdge.Under(carrier, kind)` and `SpanEdge.FanIn(links, kind)` fold either into the trailing carriage both `Traced` rail shapes take; `SpanBand.Of(version, plane, external)` mints the band and `Names` projects the scope names a tracer provider registers.
- Law: `KernelDomain` is a hand-kept MIRROR of the kernel's own sub-domain folder set — the roster and `ARCHITECTURE` `[01]` move as one edit, and a new sub-domain lands in both places or the span source it needs does not exist. `KernelDomain` states that mirror rather than deriving it, because a folder set is a repository fact no type can read.
- Law: `SpanBand` conforms to `IHookSpan` by taking the PLANE as an argument, so one band serves every roster plane a composition mounts and the rail hands the point's own `Plane` at the fire site; a hook rail therefore composes tracing through the bracket floor `Domain/hooks` declares and never through this type — the dependency points downward and the plane binds at band composition, because the floor's `Traced` carries a key and a body and no scope.
- Law: every capture funnels through `Op.Catch` on both rail shapes, so a throwing body inside a bracket parks as a typed refusal with its cancellation identity intact rather than escaping past the `using` that owns the span.
- Exemption: `Dispose` sweeps the frozen source set with a statement loop — the table is disposed once at composition teardown and every source is disposed even though the sweep carries no rail.
- Receipt: a failing rail lands `SetStatus(ActivityStatusCode.Error, message)` — the typed verdict, never a boolean error tag — and stamps the generated `FaultId` as the `rasm.fault.code`/`rasm.fault.case` pair a trace query groups on, behind `IsAllDataRequested` so an unsampled span pays the status alone; `HasListeners` gates every bracket, so an unlistened span costs one null test, and an expected `Fault` never fabricates an exception event to carry a tag.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`System.Collections.Frozen`, `System.Diagnostics`, `System.Threading`).
- Growth: a new sub-domain is one `KernelDomain` row, span source and point prefix deriving; a package trace plane is one `TraceScope` row admitted when the composition mints its band; a new bracket coordinate is one `SpanEdge` column every bracket already threads.
- Boundary: edge shape follows producer arity, and `SpanEdge` is where that choice lands — a batch relaying N durable rows descends from no single producer, so a parent edge to any one of them fabricates a causal chain the batch never had while the link set states exactly what caused it; a single-producer hop is the inverse, an ingress adopting one carrier through `Under` continuing the producing trace id where a link roots an orphan trace no query joins to its cause. Kind rides that same carriage because a remote-parented bracket declaring the internal default misreports the topology every backend derives its service graph from. Edges ride the START call because the sampler votes once at creation, and a producer whose span was unlistened carries the absent carrier.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Collections.Frozen;
using System.Diagnostics;
using System.Threading;
using Thinktecture;

namespace Rasm.Domain;

// --- [TYPES] --------------------------------------------------------------------------------
// `KernelDomain` MIRRORS the kernel's own sub-domain folder set: a tenth folder is a tenth row, and the two move as
// one edit because no type can read a repository layout.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class KernelDomain {
    public static readonly KernelDomain Domain = new("domain");
    public static readonly KernelDomain Numerics = new("numerics");
    public static readonly KernelDomain Spatial = new("spatial");
    public static readonly KernelDomain Parametric = new("parametric");
    public static readonly KernelDomain Meshing = new("meshing");
    public static readonly KernelDomain Processing = new("processing");
    public static readonly KernelDomain Solving = new("solving");
    public static readonly KernelDomain Drawing = new("drawing");
    public static readonly KernelDomain Analysis = new("analysis");
    public static readonly KernelDomain Interaction = new("interaction");

    // Items-derived index materializes on first read, so the measured-op bracket never re-admits its scope string.
    private static readonly Lazy<FrozenDictionary<KernelDomain, TraceScope>> Scopes = new(
        static () => Items.ToFrozenDictionary(static row => row, static row => TraceScope.Create(value: $"rasm.rasm.{row.Key}")),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public TraceScope Trace => Scopes.Value[this];
    // The point PREFIX: `KernelPoint` composes this with its facet key, so span source and hook-point id are one
    // derivation off one row key and no member here admits a caller-supplied point segment.
    public string SourceName => Trace.ToString();
}

// --- [MODELS] -------------------------------------------------------------------------------
public readonly record struct TraceCarrier(string? TraceParent, string? TraceState) {
    public static TraceCarrier Of(Activity? span) => new(span?.Id, span?.TraceStateString);

    // `isRemote` is TRUE by construction: a carrier reaches a consumer only across a process or a durable
    // boundary, so the context is foreign evidence and never an in-process parent whose recording flags a sampler
    // would inherit. A malformed pair projects None, so an ingress roots a fresh trace and a batch drops the one
    // edge it could not parse while keeping every edge it could.
    public Option<ActivityContext> Parent =>
        ActivityContext.TryParse(TraceParent, TraceState, isRemote: true, out ActivityContext context) ? Some(context) : None;

    // Tags materialize BEFORE the projection because a `ReadOnlySpan` cannot cross a lambda, and one parse serves
    // both members.
    public Option<ActivityLink> Link(params ReadOnlySpan<(string Slot, object? Value)> facts) {
        ActivityTagsCollection? tags = facts.IsEmpty
            ? null
            : new ActivityTagsCollection(InstrumentSet.Tags(TenantContext.Current, facts));
        return Parent.Map(context => new ActivityLink(context, tags));
    }
}

// `default` IS the in-process internal bracket — kind zero, absent parent, empty links — so a descendant call
// passes nothing and the runtime resolves `Activity.Current` for both the sampling vote and the parent edge.
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SpanEdge(ActivityKind Kind, Option<ActivityContext> Parent, Seq<ActivityLink> Links) {
    // `Consumer` heads the kind column because an internal kind on a remote-parented ingress misreports the
    // topology every backend derives its service graph from; a request-shaped ingress names `Server` here.
    public static SpanEdge Under(TraceCarrier carrier, ActivityKind kind = ActivityKind.Consumer) =>
        new(kind, carrier.Parent, Seq<ActivityLink>());

    public static SpanEdge FanIn(Seq<ActivityLink> links, ActivityKind kind = ActivityKind.Internal) =>
        new(kind, Option<ActivityContext>.None, links);

    // DEFAULT context resolves to `Activity.Current` at the runtime for both the sampling vote and the
    // parent edge, so a carriage-free bracket keeps byte-identical parenting.
    public ActivityContext Context => Parent.IfNone(default(ActivityContext));

    // Empty carriage passes null rather than an empty sequence, so a bracket with no edges pays no enumerator.
    public IEnumerable<ActivityLink>? Edges => Links.IsEmpty ? null : Links;
}

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed class SpanBand : IDisposable, IHookSpan {
    private readonly FrozenDictionary<TraceScope, ActivitySource> sources;

    private SpanBand(TraceScope plane, FrozenDictionary<TraceScope, ActivitySource> sources) =>
        (Plane, this.sources) = (plane, sources);

    public TraceScope Plane { get; }

    public static SpanBand Of(string version, TraceScope plane, params ReadOnlySpan<TraceScope> external) =>
        new(plane, toSeq(KernelDomain.Items).Map(static row => row.Trace)
            .Concat(toSeq(external.ToArray()))
            .Add(plane)
            .Distinct()
            .ToFrozenDictionary(static scope => scope, scope => new ActivitySource(scope.ToString(), version)));

    // Every admitted source name reaches a tracer provider through this projection. Freezing a scope here and
    // registering none at the provider holds a source no listener matches, so every bracket takes the null-span
    // arm and this band exports nothing while each call site still reads as traced.
    public Seq<string> Names => toSeq(sources.Values).Map(static source => source.Name).Strict();

    // `IHookSpan` conformance takes the PLANE as a parameter, so one band serves every roster plane a
    // composition mounts and the rail hands the point's own `Plane` rather than a second band per plane.
    public Fin<T> Traced<T>(TraceScope plane, Op key, Func<Fin<T>> body) => Traced(plane, key, _ => body());

    public Fin<T> Traced<T>(TraceScope scope, Op key, Func<Activity?, Fin<T>> body, SpanEdge edge = default) {
        if (!sources.TryGetValue(scope, out ActivitySource? source)) { return Fin.Fail<T>(Unadmitted(scope)); }
        if (!source.HasListeners()) { return key.Catch(() => body(null)); }
        using Activity? span = source.StartActivity(key.ToString(), edge.Kind, edge.Context, tags: null, links: edge.Edges);
        return key.Catch(() => body(span)).MapFail(error => Marked(span, error));
    }

    // `Bracket`'s three-arm failure arm receives the `Error` ALONE and never the acquired value, so the status
    // mark that must reach the span cannot ride it and stays inside `Use` behind `@catch`.
    public IO<T> Traced<T>(TraceScope scope, Op key, Func<Activity?, IO<T>> body, SpanEdge edge = default) =>
        !sources.TryGetValue(scope, out ActivitySource? source)
            ? IO.fail<T>(Unadmitted(scope))
            : !source.HasListeners()
            ? body(null)
            : IO.lift(() => source.StartActivity(key.ToString(), edge.Kind, edge.Context, tags: null, links: edge.Edges))
                .Bracket(
                    Use: span => (body(span) | @catch<IO, T>(static _ => true, error => IO.fail<T>(Marked(span, error)))).As(),
                    Fin: static span => IO.lift(() => ignore(span?.Dispose())));

    public void Dispose() {
        foreach (ActivitySource source in sources.Values) { source.Dispose(); }
    }

    private static Error Unadmitted(TraceScope scope) =>
        new KernelFault.InvalidValue(Label: scope.ToString(), Requirement: "a trace scope admitted at band composition");

    // The status carries the VERDICT and the tags carry the generated IDENTITY a trace query groups failures on
    // — the number a peer would also see beside the case token only this process holds. `IsAllDataRequested`
    // gates the tag pair, so an unsampled span pays the status alone, and an expected `Fault` records no
    // exception event because none was thrown to record. A foreign `Error` carries neither tag rather than a
    // fabricated code, exactly as the metric fold refuses a fabricated owner.
    private static Error Marked(Activity? span, Error error) {
        span?.SetStatus(ActivityStatusCode.Error, error.Message);
        if (span?.IsAllDataRequested is true && error is Fault fault) {
            ignore(span
                .SetTag(KernelInstrument.CodeSlot, fault.Identity.Code)
                .SetTag(KernelInstrument.CaseSlot, fault.Identity.Case));
        }

        return error;
    }
}
```

## [03]-[COST]

- Owner: `Outcome` is the settled-verdict row every emitted dimension reads; `CostMark` is the capture pair — a monotonic tick and the thread allocation counter, minted before the guarded work and folded by `Stop` into `OpCost`; `OpCost` is the uniform per-op evidence the app strata attribute to tenants.
- Entry: `Outcome.Of(bool)` is the ONE widening from a settled verdict to its dimension value, so the vocabulary lives at one owner and grows there.
- Law: one capture per operation runtime — the operation marks before its body fold, the admission gate sits inside the marked window so admission cost charges to the operation that demanded it, and BOTH exits charge: the success leg records `Outcome.Succeeded`, the fail leg `Outcome.Failed` and publishes the fault fact, so cost and failure evidence never diverge and the outcome dimension keeps the two populations separable on one series.
- Law: allocation delta is thread-local evidence, valid because the synchronous runtime runs the marked window on one thread; a thread-hopping lane keeps elapsed truth and reads the delta as an allocation floor, never a total.
- Receipt: `OpCost` registers `IValidityEvidence`, so the fact reaches the one acceptance oracle like every kernel receipt.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`System.Diagnostics`).
- Growth: a third settled verdict is one `Outcome` row and a widening the `Of` bridge no longer spans, which is the loud break that row is worth.
- Boundary: the fabric never wraps a second timer or a sampling profiler — profile capture is the app stratum's, this row the per-op scalar truth.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Diagnostics;
using Thinktecture;

namespace Rasm.Domain;

// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Outcome {
    public static readonly Outcome Succeeded = new("succeeded");
    public static readonly Outcome Failed = new("failed");

    public static Outcome Of(bool settled) => settled ? Succeeded : Failed;
}

// --- [MODELS] -------------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct OpCost(
    Op Key, KernelDomain Domain, TimeSpan Elapsed, long AllocatedBytes, int Items, Outcome Outcome)
    : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Nonnegative(value: Elapsed.TotalSeconds),
        AllocatedBytes >= 0L,
        ValidityClaim.CountAtLeast(count: Items, floor: 0));
}

// The S0 mark pair: `MonotonicTimeline` seats at S3 `Rasm.Parametric` and the floor reads nothing up-strata,
// so the op-cost capsule marks its own timestamp — the branch ruling names this seam from both ends.
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CostMark(long Timestamp, long Allocated) {
    public static CostMark Start() =>
        new(Timestamp: Stopwatch.GetTimestamp(), Allocated: GC.GetAllocatedBytesForCurrentThread());

    public OpCost Stop(Op key, KernelDomain domain, int items, Outcome outcome) =>
        new(Key: key, Domain: domain,
            Elapsed: Stopwatch.GetElapsedTime(startingTimestamp: Timestamp),
            AllocatedBytes: long.Max(0L, GC.GetAllocatedBytesForCurrentThread() - Allocated),
            Items: items, Outcome: outcome);
}
```

## [04]-[TAP]

- Owner: `FaultObservation` is the bounded structured projection of an `Error`, carrying the generated `FaultId` WHOLE so code and case token stay one member with one authority; `PointFacet` closes what a signal fact IS; `KernelPoint` is the kernel's own `IHookRoster` realization over the sub-domain and facet coordinates; `SignalFact`'s abstract `At` projects each case's own stored ROSTER ROW, so identity moves `with`-safe and no fact carries a spelling; `SignalRail` is the emission capsule over `Domain/hooks`'s one mechanism; `KernelInstrument` is the kernel's own instrument roster, each row CARRYING its declaration; `TelemetrySink` is the composition capsule an app stratum threads.
- Cases: `ReceiptCase`, `FaultCase`, and `CostCase`, one per `PointFacet` row; each factory resolves its seat through `KernelPoint.Of(domain, facet)` and none renders an id.
- Entry: `FaultObservation.Of(error)` projects the generated `FaultId` — transported code beside local case token — typed recovery, and bounded exact cause stamps without rendering `Message`; `KernelPoint.Of(domain, facet)` is the ONE seat mint, `SignalRail.Of` mounts the rail over the composition's evidence cell, `Publish` is the unary fire, and `TelemetrySink.Tap` is the ONE emission entry.
- Auto: the tap is TWO total projections and ONE fold. `Charged` answers the rows a case bills and their measurements, `Stamped` answers the dimensions it carries, and the gate, the tag mint, and the write fold run once for every case — the prior form spelled gate, mint, and write three times, so a fourth case meant a fourth body rather than two rows.
- Auto: `KernelInstrument` rows carry their own `InstrumentSpec` and `Rows` derives from `Items`, so the const-name roster and the hand-listed sequence that mirrored it are one declaration; construction proves the row's name against its key, because a mirror that cannot derive states its invariant at both owners and moves as one.
- Law: the roster is DERIVED, never listed — `KernelPoint.Items` materializes the whole sub-domain by facet product at type init, so a tenth sub-domain lands three seats with no edit here and a folder minting an inline `HookId.Create` literal does not compile (branch RULINGS `[02]`). The declare-or-resolve transition a runtime id space needed has no spelling left: a point outside the roster is unrepresentable rather than merely undeclared.
- Law: `SignalRail` governs EMISSION alone — it publishes the unary fire and the mechanism it holds, and never the guarded `Fire<T>` arity, so a gate guarding geometry, a lease, or a handle declares its own roster and its own closed fact union rather than borrowing the signal plane's (`Rasm` RULINGS `[02]`).
- Law: publication LEADS the write, and the instruments measure the PUBLISHED fact — the rail is a hook plane whose subscribers veto and transform, so a write ahead of it meters a fact a veto then withheld and meters the pre-admission shape of one a subscriber revised.
- Law: instrument refusals ACCUMULATE and settle the returned rail beside the already-published fact — every charged row is attempted, so a mount defect on one row never silences the two beside it and never silences the hook plane.
- Law: fault observation traverses aggregate MEMBERSHIP and causal `Inner` separately under one fixed ceiling; each retained cause carries the generated `FaultId` or the exact exception `Type` and `HResult`, and `Truncated` states when more evidence existed. Message text, category, owner, and a wire discriminant never enter the projection.
- Law: `KernelInstrument` owns the whole `rasm.fault.*` key family and each key states where it may be read — `OwnerSlot` and `PostureSlot` are the bounded pair the kernel counter mounts, `CodeSlot` an owner-specific opt-in metric dimension, `CaseSlot` a span tag and log field alone. A metric mounting the case token buys code-cardinality series for a spelling the code already keys, and a lowering copying it forks one identity into two a peer then joins on; an emitter prefixing its own estate segment onto a fault axis forks one estate-wide dimension into a per-package pair no board can group, which is the fork this one roster forecloses. The posture VALUE is `Domain/rails`'s `Retriability.Key`, so the key and the word it carries each have one owner.
- Exemption: the write fold is a statement seam because a `TagList` cannot cross a lambda; the listener gate precedes it, so a process with no exporter and no armed tally pays the key render, the boxed columns, and the tag fold on no operation.
- Receipt: fact payloads are evidence, never live resources — `ReceiptCase` carries the receipt value, `FaultCase` the already-lowered `Error` (both the substrate `Fault` union and the band-relative geometry faults arrive as `Error`, so one case serves both), and no case retains geometry, leases, or handles; both fault families land in ONE dimension-discriminated counter, never two.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`System.Collections.Frozen`, `System.Collections.Immutable`, `System.Diagnostics`, `System.Diagnostics.Metrics`, `System.Threading`).
- Growth: a new fact kind is one `PointFacet` row, one `SignalFact` case, and one arm in each of the two projections, both broken loudly by the generated `Switch` while the roster grows a seat per sub-domain unedited; a new kernel instrument is one `KernelInstrument` row and one entry in `Charged`.
- Boundary: `SignalFact` holds evidence over live resources, so a fact retains no geometry, lease, or handle and a subscriber reading one holds nothing the emitter must keep alive. `TelemetrySink` is composition-entered: an app stratum mints one per composition and threads it, and a kernel page never constructs, caches, or reaches an ambient sink. Quiet-path cost is structural — a subscriber-empty point folds an empty veto sequence and iterates an empty tap sequence, so a publish costs one keyed lookup and allocates nothing past its rail.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading;
using Rasm.Numerics;
using Thinktecture;

namespace Rasm.Domain;

// --- [TYPES] --------------------------------------------------------------------------------
// What a signal fact IS, closed at three because `SignalFact` closes at three: the roster and the union grow as
// ONE edit, and the facet key is the point id's trailing segment rather than a literal a factory spells.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PointFacet {
    public static readonly PointFacet Cost = new("cost");
    public static readonly PointFacet Fault = new("fault");
    public static readonly PointFacet Receipt = new("receipt");
}

// Each row CARRIES its declaration and `Rows` derives from `Items`, so the const-name roster and the hand-listed
// sequence that mirrored it are one declaration; the key and the row's name are one fact proved at construction.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class KernelInstrument {
    public const string OpSlot = "rasm.kernel.op";
    public const string DomainSlot = "rasm.kernel.domain";
    public const string OutcomeSlot = "rasm.kernel.outcome";
    // The allocating package is derived from the numeric identity's band.
    public const string OwnerSlot = "rasm.fault.owner";
    public const string PostureSlot = "rasm.fault.posture";
    // The `rasm.fault.*` key family has ONE owner and it is this roster — a metric dimension and a span tag read
    // the same const, so a second emitter cannot fork one stream's key from another's. Raw code is an OPT-IN
    // metric dimension for owner-specific bounded instruments; the kernel counter does not mount it.
    public const string CodeSlot = "rasm.fault.code";
    // The generated case token, at code cardinality: a span tag and a log field, never a metric dimension, so an
    // exporter's series budget never pays for it, and never a wire column — `FaultDetail` transports the number.
    public const string CaseSlot = "rasm.fault.case";

    public static readonly KernelInstrument Duration = new(
        "rasm.kernel.op.duration",
        InstrumentSpec.Create("rasm.kernel.op.duration", InstrumentKind.Distribution, MeasureForm.Real, "s",
            "Kernel operation wall time.", Seq(OpSlot, DomainSlot, OutcomeSlot), Some(Buckets.BenchSeconds), None, None));

    public static readonly KernelInstrument Allocated = new(
        "rasm.kernel.op.allocated",
        InstrumentSpec.Create("rasm.kernel.op.allocated", InstrumentKind.Distribution, MeasureForm.Whole, "By",
            "Kernel operation allocated bytes.", Seq(OpSlot, DomainSlot, OutcomeSlot), Some(Buckets.ByteSizes), None, None));

    // `Counted`, never `Items`: the generator's roster member is `Items` and a row of that name is CS0102.
    public static readonly KernelInstrument Counted = new(
        "rasm.kernel.op.items",
        InstrumentSpec.Create("rasm.kernel.op.items", InstrumentKind.Distribution, MeasureForm.Whole, "{item}",
            "Kernel operation item count.", Seq(OpSlot, DomainSlot, OutcomeSlot), Some(Buckets.GraphCounts), None, None));

    public static readonly KernelInstrument Receipts = new(
        "rasm.kernel.op.receipts",
        InstrumentSpec.Create("rasm.kernel.op.receipts", InstrumentKind.Count, MeasureForm.Whole, "{receipt}",
            "Kernel receipt stream by acceptance verdict.", Seq(OpSlot, OutcomeSlot), None, None, None));

    public static readonly KernelInstrument Faults = new(
        "rasm.kernel.fault.count",
        InstrumentSpec.Create("rasm.kernel.fault.count", InstrumentKind.Count, MeasureForm.Whole, "{fault}",
            "Kernel fault stream by owning package and recovery posture.", Seq(OwnerSlot, PostureSlot), None, None, None));

    public InstrumentSpec Row { get; }

    public static Seq<InstrumentSpec> Rows => toSeq(Items).Map(static row => row.Row).Strict();

    static partial void ValidateConstructorArguments(ref string key, ref InstrumentSpec row) {
        if (!string.Equals(key, row.Name, StringComparison.Ordinal)) {
            throw new ArgumentException($"<kernel-instrument:{key}>", nameof(row));
        }
    }
}

// --- [MODELS] -------------------------------------------------------------------------------
// Wire-neutral fault evidence: `Identity` exists only for a generated `Fault`; an exceptional error carries its
// exact runtime type and HResult instead. The cause walk is breadth-first and bounded in work as well as output,
// so an aggregate cannot turn a diagnostic projection into an unbounded payload.
public readonly record struct FaultCauseStamp(Option<FaultId> Identity, Option<Type> ExceptionType, Option<int> HResult) {
    public Option<int> Code => Identity.Map(static id => id.Code);
}

// The identity owner rides WHOLE rather than pre-split, so an in-process subscriber reads the case token without
// a second projection and a wire lowering reads `Code`. NAMED LOSS: the member TYPE no longer forecloses the
// crossing — an `Option<int>` column could carry no token at all — so the guard moves onto `FaultId`'s own
// `[JsonIgnore]` and declared equality, which a hand-written lowering still has to respect deliberately.
public sealed record FaultObservation(
    Option<FaultId> Identity, Retriability Recovery, Seq<FaultCauseStamp> Causes, bool Truncated) {
    public static readonly Dimension CauseCeiling = Dimension.Create(value: 8);

    // The transported half, DERIVED: every lowering reads this member and reaches no second stored column, so a
    // wire record cannot pick up the case token by being adjacent to the one it meant to copy.
    public Option<int> Code => Identity.Map(static id => id.Code);

    public static FaultObservation Of(Error error) {
        ArgumentNullException.ThrowIfNull(error);
        Queue<Error> pending = new();
        pending.Enqueue(error);
        Seq<FaultCauseStamp> causes = Seq<FaultCauseStamp>();
        int inspected = 0;
        while (inspected < CauseCeiling.Value && pending.TryDequeue(out Error? current)) {
            bool root = inspected++ == 0;
            Option<FaultId> identity = current is Fault fault ? Some(fault.Identity) : None;
            Option<Exception> exception = current is Fault ? None : current.Exception;
            if ((!root && identity.IsSome) || exception.IsSome) {
                causes = causes.Add(new FaultCauseStamp(
                    Identity: identity,
                    ExceptionType: exception.Map(static raised => raised.GetType()),
                    HResult: exception.Map(static raised => raised.HResult)));
            }
            if (current is ManyErrors many) {
                foreach (Error child in many.Errors) { pending.Enqueue(child); }
            }
            current.Inner.Iter(pending.Enqueue);
        }
        return new FaultObservation(
            Identity: error is Fault fault ? Some(fault.Identity) : None,
            Recovery: Redrive.Posture(error),
            Causes: causes,
            Truncated: pending.Count > 0);
    }
}

// The kernel's hook roster is DERIVED, not listed: every sub-domain fires every facet, so the product materializes
// once at type init and `Of` is the ONE mint — a coordinate stored beside a row forks it (`Rasm` RULINGS `[02]`).
// `HookId.Create` is the trusted-text entry here because both coordinates are closed rosters whose keys the two
// grammars already admit; no caller text reaches it.
public sealed record KernelPoint : IHookRoster<KernelPoint> {
    private KernelPoint(KernelDomain domain, PointFacet facet) =>
        (Domain, Facet, Id) = (domain, facet, HookId.Create(value: $"{domain.SourceName}.{facet.Key}"));

    public KernelDomain Domain { get; }
    public PointFacet Facet { get; }
    public HookId Id { get; }

    // Emission admits transform-or-withhold and shielded observation on EVERY point: `Publish` is the unary fire, so
    // a subscriber revises or refuses the fact the instruments then meter. Retention is a consuming folder's roster
    // decision — a standing kernel buffer over every operation's cost is a memory bound nothing declared.
    public CapabilitySet<HookModality> Modalities => Emission.Value;

    public Option<TraceScope> Plane => Some(Domain.Trace);

    public static IReadOnlyList<KernelPoint> Items => Roster.Value;

    public static KernelPoint Of(KernelDomain domain, PointFacet facet) => Index.Value[(domain, facet)];

    // Accessor-backed on all three: both coordinate rosters fill `Items` from their own static constructors, so an
    // eager field materializes the product of two EMPTY sequences and freezes a roster with no rows.
    private static readonly Lazy<CapabilitySet<HookModality>> Emission = new(
        static () => CapabilitySet<HookModality>.Of(HookModality.Veto, HookModality.Observe),
        LazyThreadSafetyMode.ExecutionAndPublication);

    private static readonly Lazy<ImmutableArray<KernelPoint>> Roster = new(
        static () => [.. from domain in KernelDomain.Items
                         from facet in PointFacet.Items
                         select new KernelPoint(domain: domain, facet: facet)],
        LazyThreadSafetyMode.ExecutionAndPublication);

    private static readonly Lazy<FrozenDictionary<(KernelDomain Domain, PointFacet Facet), KernelPoint>> Index = new(
        static () => Roster.Value.ToFrozenDictionary(static row => (row.Domain, row.Facet)),
        LazyThreadSafetyMode.ExecutionAndPublication);
}

[Union]
public abstract partial record SignalFact {
    private SignalFact() { }

    // `At` projects each case's stored ROSTER ROW — a second stored copy diverges under `with`, and a rendered id
    // would let a fact name a seat the roster never declared.
    public abstract KernelPoint At { get; }

    public sealed record ReceiptCase(KernelPoint Point, Op Key, IValidityEvidence Receipt) : SignalFact { public override KernelPoint At => Point; }
    public sealed record FaultCase(KernelPoint Point, Op Key, Error Fault) : SignalFact { public override KernelPoint At => Point; }
    public sealed record CostCase(KernelPoint Point, OpCost Cost) : SignalFact { public override KernelPoint At => Point; }

    public static SignalFact Receipt(KernelDomain domain, Op key, IValidityEvidence receipt) =>
        new ReceiptCase(Point: KernelPoint.Of(domain: domain, facet: PointFacet.Receipt), Key: key, Receipt: receipt);

    public static SignalFact Fault(KernelDomain domain, Op key, Error fault) =>
        new FaultCase(Point: KernelPoint.Of(domain: domain, facet: PointFacet.Fault), Key: key, Fault: fault);

    public static SignalFact Cost(OpCost cost) =>
        new CostCase(Point: KernelPoint.Of(domain: cost.Domain, facet: PointFacet.Cost), Cost: cost);
}

// --- [SERVICES] -----------------------------------------------------------------------------
// EMISSION alone over the branch's ONE hook mechanism. `Hooks` is PUBLIC because `Points`, `Drain`, `Replay`,
// `Release`, `Detach`, and `Faults` are the mechanism's own and a forwarding member per name would resolve one
// name in two hops; what this capsule adds is the narrowing — the unary `Publish` and no
// guarded `Fire<T>` arity — so the signal plane cannot become the gate a guarded seam borrows.
public sealed class SignalRail {
    private SignalRail(HookRail<KernelPoint, SignalFact, TelemetrySource> mounted) => Hooks = mounted;

    public HookRail<KernelPoint, SignalFact, TelemetrySource> Hooks { get; }

    public static Fin<SignalRail> Of(
        FaultCell faults,
        Op key,
        Seq<HookGate<KernelPoint, SignalFact, TelemetrySource>> gates = default,
        Seq<HookTap<KernelPoint, SignalFact, TelemetrySource>> taps = default,
        Option<IHookSpan> span = default) =>
        HookRail<KernelPoint, SignalFact, TelemetrySource>.Of(
                key: key, gates: gates, taps: taps, span: span, cell: Some(faults))
            .Map(static mounted => new SignalRail(mounted: mounted));

    // The fact CARRIES its seat, so publication resolves no name and admits no undeclared point — the roster is the
    // rail's type parameter, so a point outside it never reached a `SignalFact` in the first place.
    public Fin<SignalFact> Publish(SignalFact fact, Op key) => Hooks.Fire(at: fact.At, fact: fact, key: key);
}

// --- [COMPOSITION] --------------------------------------------------------------------------
public sealed class TelemetrySink {
    private readonly InstrumentSet set;

    private TelemetrySink(SignalRail rail, InstrumentSet mounted) => (Rail, set) = (rail, mounted);

    public SignalRail Rail { get; }

    // The evidence cell arrives from the composing app and reaches the rail whole, so the kernel plane, the tenancy
    // stamp, and the interaction shield park on ONE ring rather than three the composition never chose.
    public static Fin<TelemetrySink> Of(IMeterFactory factory, string version, FaultCell faults, Op key) =>
        from rail in SignalRail.Of(faults: faults, key: key)
        from mounted in InstrumentSet.Of(
            new LevelCells(),
            (TelemetryIdentity.Metered(factory, TelemetrySource.Kernel, version), KernelInstrument.Rows))
        select new TelemetrySink(rail: rail, mounted: mounted);

    public Fin<SignalFact> Tap(SignalFact fact, Op key) {
        Fin<SignalFact> published = Rail.Publish(fact: fact, key: key);
        Seq<(InstrumentSpec Row, double Value)> charged = Charged(fact: fact);
        if (published.IsFail || !set.Enabled(charged.Map(static row => row.Row))) { return published; }
        TagList tags = Stamped(fact: fact);
        Seq<Error> refusals = Seq<Error>();
        foreach ((InstrumentSpec row, double value) in charged) {
            refusals = set.Write(row: row, measurement: value, tags: in tags)
                .Match(Succ: _ => refusals, Fail: cause => refusals.Add(cause));
        }
        return refusals
            .Fold(Option<Error>.None, static (seat, cause) => Some(seat.Match(Some: first => first + cause, None: () => cause)))
            .Match(Some: Fin.Fail<SignalFact>, None: () => published);
    }

    private static Seq<(InstrumentSpec Row, double Value)> Charged(SignalFact fact) => fact.Switch(
        receiptCase: static _ => Seq((KernelInstrument.Receipts.Row, 1d)),
        faultCase: static _ => Seq((KernelInstrument.Faults.Row, 1d)),
        costCase: static row => Seq(
            (KernelInstrument.Duration.Row, row.Cost.Elapsed.TotalSeconds),
            (KernelInstrument.Allocated.Row, (double)row.Cost.AllocatedBytes),
            (KernelInstrument.Counted.Row, (double)row.Cost.Items)));

    private static TagList Stamped(SignalFact fact) => fact.Switch(
        state: TenantContext.Current,
        receiptCase: static (tenant, row) => InstrumentSet.Tags(tenant,
            (KernelInstrument.OpSlot, (object?)row.Key.ToString()),
            (KernelInstrument.OutcomeSlot, Outcome.Of(row.Receipt.IsValid).Key)),
        // Metrics stay bounded on locally derived owner and posture. Foreign errors receive neither a fabricated
        // owner nor a numeric project-fault identity; raw code remains an owner-specific opt-in dimension.
        faultCase: static (tenant, row) => InstrumentSet.Tags(tenant,
            (KernelInstrument.OwnerSlot, row.Fault.Owner.Map(static owner => owner.Key).Match<object?>(Some: static owner => owner, None: static () => null)),
            (KernelInstrument.PostureSlot, Redrive.Posture(row.Fault).Key)),
        costCase: static (tenant, row) => InstrumentSet.Tags(tenant,
            (KernelInstrument.OpSlot, (object?)row.Cost.Key.ToString()),
            (KernelInstrument.DomainSlot, row.Cost.Domain.Key),
            (KernelInstrument.OutcomeSlot, row.Cost.Outcome.Key)));

}
```

## [05]-[CONTRIBUTE]

- Owner: `IReceiptKind<TSelf>` is the receipt-kind roster floor a folder realizes (rows carrying `Key` and their own `Write`), so a kind→arm table is `toSeq(TKind.Items).Map(row => ReceiptFan.Arm(row.Key, row.Write))` and never a hand dictionary; `ArmKey` is the ONE projection discriminant closing both dispatch regimes; `ArmRequest` is the request a fan projects, carrying the wire envelope always and the typed fact where producer and consumer share a process; `InstrumentArm` is the one arm shape; `ReceiptFan` owns the one merged table; `ClassifiedValue` carries a sensitivity annotation as text; `TelemetryContributorPort` is the ONE downward contribution fact a stratum hands a composing root.
- Cases: two arm keys — a fact TYPE for the in-process dispatch, a kind NAME for the wire dispatch — one discriminant the request itself recovers.
- Entry: `Arm<TFact>(arm)` and `Arm(kind, arm)` register a typed and a wire row respectively, each erasing its cast ONCE at registration so the stored slot can never hold a delegate the dispatch miscasts; `Of` merges every contributed table; `Project<TFact>(kind, payload, fact)` and `Project(kind, payload)` are the ONE dispatch over one private request; NAMED LOSS: the fan's table is runtime-type keyed and the request slot is erased INSIDE the fan — the price of one table over every fact type a stratum contributes — and the public surface carries no `object`; `Roster` freezes the port's whole declaration by name and `Admit` proves its pack against that roster, so a mounting root folds every contributor before it mints a meter.
- Auto: the typed key resolves first and the wire key is its own fallthrough off the SAME request, so a partially typed table loses no projection, a kind mapped in both tables writes once, and one fact reaches one arm. In-process projection therefore costs one delegate call instead of a serialize-and-reparse round trip, while a cross-process subscriber holds only the envelope and projects it alone.
- Law: `Of` refuses a duplicate key on the RAIL and names every collided one, so a collision lands as a typed refusal rather than an untyped `ArgumentException` escaping a composition root.
- Law: `Roster` names WHICH declaration collided — a refusal that states only that some name repeats leaves a mounting root to diff two rosters by hand.
- Law: unmapped kinds stay receipt-only by declaration and succeed silently, while a MOUNTED arm's refusal is a defect that rides outward, so the two absences stay distinguishable at the subscribing seam.
- Law: the port names its scope with the branch's own `TelemetrySource` row rather than a bare string, so a contributor cannot fork the package census the fault-band registry and the causal frame already key on; the schema pin is read, never passed, so no contributor can break the one coordinate tracer, meter, and logger bump together on.
- Receipt: `InstrumentArm` returns the write rail, so a refusal survives the fan instead of dying at the delegate boundary.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`System.Text.Json`).
- Growth: a new projected kind is one arm-table row in the contributing folder; a contributor's whole board and reliability policy is one `Board` value on its own port, its whole span custody one `Planes` roster on that same port, and a newly annotated sensitivity one `ClassifiedValue` row on its `Classifications` column.
- Boundary: `Instruments` and `Published` split by WHO MOUNTS — the root binds handles for the first and a contributor owning its own meter lifetime declares the second, `Declared` is the union every naming gate, view predicate, and pack admission reads, and a row on neither roster exports a stream no gate can refuse. `Planes` carries the contributor's own `TraceScope` roster VERBATIM, because trace and meter scopes are distinct grammars neither derives from. `Classifications` carries sensitivity VALUES as `(taxonomy, value)` text, so no compliance type enters this assembly and a redaction root binding a redactor per rostered row has a set to PROVE its contributors against instead of a coincidence it discovers at egress.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Text.Json;
using Thinktecture;

namespace Rasm.Domain;

// --- [TYPES] --------------------------------------------------------------------------------
// ONE key over both dispatch regimes: a typed fact keys on its own runtime type and a wire envelope on its kind
// name, so one table answers both and no package seats a typed fan beside a string one.
[Union]
public abstract partial record ArmKey {
    private ArmKey() { }

    public sealed record Fact(Type Value) : ArmKey;
    public sealed record Kind(string Value) : ArmKey;
}

// --- [MODELS] -------------------------------------------------------------------------------
// Wire pairs are ALWAYS present; the typed fact is the fan's ONE erased slot, internal so only `Project<TFact>`
// seats it and only the fan's own registration cast reads it — an arm sees Kind and Payload alone.
public readonly record struct ArmRequest(string Kind, JsonElement Payload) {
    internal Option<object> Fact { get; init; }
}

public readonly record struct ClassifiedValue(string Taxonomy, string Value);

// This estate keeps ONE redaction taxonomy: both host boundaries declared byte-identical four-row classifications
// (`GhSensitivity`/`HostSensitivity`, both deleted), so the roster seats here and each boundary mints
// `new DataClassification(Sensitivity.Taxonomy, Row.Key)` into its own `[LogProperties]` attributes — no
// compliance type enters this assembly — and carries `Sensitivity.Values` on its contributor port.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Sensitivity {
    public const string Taxonomy = "DataClassification";
    public static readonly Sensitivity UserContent = new(key: "user-content");
    public static readonly Sensitivity HostPath = new(key: "host-path");
    public static readonly Sensitivity MachineIdentity = new(key: "host-identity");
    public static readonly Sensitivity AccountIdentity = new(key: "personal");
    public ClassifiedValue Value => new(Taxonomy: Taxonomy, Value: Key);
    // Declared roster a contributor port stamps wholesale.
    public static Seq<ClassifiedValue> Values => toSeq(Items).Map(static row => row.Value);
}

// --- [SERVICES] -----------------------------------------------------------------------------
// Six folders' kind→arm tables share this downward-contribution shape: a receipt KIND is a roster row carrying its
// own instrument write, so a folder declares rows and hands `ReceiptFan.Arm(row.Key, row.Write)` — never a
// string-keyed dispatch table beside the roster.
public interface IReceiptKind<TSelf> where TSelf : IReceiptKind<TSelf> {
    static abstract IReadOnlyList<TSelf> Items { get; }
    string Key { get; }
    Fin<Unit> Write(InstrumentSet set, JsonElement payload);
}

// `set` reaches its own cells, so a second cell parameter is the knob this shape deletes.
public delegate Fin<Unit> InstrumentArm(InstrumentSet set, ArmRequest request);

public sealed record ReceiptFan(InstrumentSet Set, HashMap<ArmKey, InstrumentArm> Arms) {
    public static Fin<ReceiptFan> Of(InstrumentSet set, params ReadOnlySpan<HashMap<ArmKey, InstrumentArm>> tables) {
        Seq<(ArmKey Key, InstrumentArm Arm)> rows = toSeq(tables.ToArray())
            .Bind(static table => toSeq(table).Map(static pair => (Key: pair.Key, Arm: pair.Value))).Strict();
        Seq<ArmKey> collided = rows.Collisions(static row => row.Key);
        return collided.IsEmpty
            ? Fin.Succ(new ReceiptFan(Set: set, Arms: rows.ToHashMap(static row => row.Key, static row => row.Arm)))
            : Fin.Fail<ReceiptFan>(new KernelFault.InvalidValue(
                Label: string.Join(", ", collided.Map(static key => key.ToString())),
                Requirement: "one arm per projection key across every contributed table"));
    }

    // Registration erases the cast ONCE, pinning the entry's key to its arm's own fact type, so the erased slot
    // can never hold a delegate the runtime-type dispatch would miscast.
    public static (ArmKey Key, InstrumentArm Arm) Arm<TFact>(Func<InstrumentSet, TFact, Fin<Unit>> arm)
        where TFact : notnull =>
        (new ArmKey.Fact(typeof(TFact)),
         (set, request) => request.Fact.Match(
             Some: fact => arm(set, (TFact)fact),
             None: () => Fin.Succ(unit)));

    public static (ArmKey Key, InstrumentArm Arm) Arm(string kind, Func<InstrumentSet, JsonElement, Fin<Unit>> arm) =>
        (new ArmKey.Kind(kind), (set, request) => arm(set, request.Payload));

    public Fin<Unit> Project<TFact>(string kind, JsonElement payload, TFact fact) where TFact : notnull =>
        Project(new ArmRequest(Kind: kind, Payload: payload) { Fact = Some<object>(fact) });
    public Fin<Unit> Project(string kind, JsonElement payload) =>
        Project(new ArmRequest(Kind: kind, Payload: payload));
    private Fin<Unit> Project(ArmRequest request) =>
        request.Fact.Bind(fact => Arms.Find(new ArmKey.Fact(fact.GetType())))
            .Match(
                Some: arm => arm(Set, request),
                None: () => Arms.Find(new ArmKey.Kind(request.Kind))
                    .Match(Some: arm => arm(Set, request), None: static () => Fin.Succ(unit)));
}

// --- [COMPOSITION] --------------------------------------------------------------------------
// Contribution is ONE downward fact: instrument rows, trace planes, sensitivity values, and the board pack over
// those same rows travel together, so a mounting root proves every contributor's descriptors inside the
// expression that binds the handles and never reaches a package-specific static field by name.
public sealed record TelemetryContributorPort(
    TelemetrySource Scope,
    string Version,
    Seq<InstrumentSpec> Instruments,
    Seq<InstrumentSpec> Published = default,
    Seq<TraceScope> Planes = default,
    Seq<ClassifiedValue> Classifications = default,
    Option<BoardPack> Board = default) {
    public static TelemetryContributorPort Kernel(string version) =>
        new(Scope: TelemetrySource.Kernel, Version: version, Instruments: KernelInstrument.Rows);

    public Seq<InstrumentSpec> Declared => Instruments + Published;

    // DECLARATION is the proof surface, never the mounted handle set: a contributor minting rows on a meter its
    // own load context owns takes no seat in any root's mount, so admitting its pack there proves nothing and
    // refuses everything. A name carried on BOTH columns refuses here as the second-handle defect it is.
    public Fin<HashMap<string, InstrumentSpec>> Roster =>
        Declared.Collisions(static row => row.Name) is { IsEmpty: false } collided
            ? Fin.Fail<HashMap<string, InstrumentSpec>>(new KernelFault.InvalidValue(
                Label: string.Join(", ", collided),
                Requirement: "one declaration per name across the mounted and published columns"))
            : Fin.Succ(Declared.ToHashMap(static row => row.Name, static row => row));

    // Traversal totalizes absence, so no arm exists: a packless port carries no descriptor to prove, one member
    // serves both shapes, and the port needs no argument because it already holds everything the proof reads.
    public Fin<Unit> Admit() =>
        Roster.Bind(roster => Board.TraverseM(pack => pack.Admit(roster)).As()).Map(static _ => unit);
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
