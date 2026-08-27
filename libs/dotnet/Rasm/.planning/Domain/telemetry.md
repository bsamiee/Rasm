# [RASM_TELEMETRY]

`Rasm.Domain` owns the branch's one OTel-free signal fabric: the kernel sub-domain roster every span source and hook-point id derives from, the causal-edge vocabulary a bracket carries, the span band that conforms to the hook bus's own bracket floor, the per-operation cost evidence, the one emission entry that publishes a fact and meters it, and the contributor port a stratum hands a composing root. One home holds the fabric TYPE; a stratum above composes an INSTANCE against its own fact union, its own instrument roster, and its own objectives.

Every owner is instance-owned and composition-entered — evidence cell, meter, bus, and band arrive from the composing app, so two compositions never contend for one slot. Dependency split draws the boundary: this page carries `System.Diagnostics`, `System.Diagnostics.Metrics`, LanguageExt, and Thinktecture, while OTel-SDK wiring, exporters, sampling, resource identity, foreign-source rows, and the OTel baggage store stay at the app platform. Fire is synchronous from any stratum, and every owner this page composes downward — the hook capsule, the causal frame, the measurement plane, the reliability algebra — is declared on its own page and re-spelled here nowhere.

## [01]-[INDEX]

- [02]-[CAPSULE]: `KernelDomain`, `TraceCarrier`, `SpanEdge`, `SpanBand` — the sub-domain roster, the causal-edge vocabulary, and the span band that conforms to `IHookSpan`.
- [03]-[COST]: `Cost`, `CostMark` — the per-operation billing capture and its settled verdict, declared ahead of the tap that meters them.
- [04]-[TAP]: `FaultObservation`, `PointFacet`, `KernelPoint`, `SignalFact`, `SignalHooks`, `KernelInstrument`, `TelemetrySink` — structured fault projection, hook roster, fact vocabulary, emission bus, instruments, and the ONE emission entry.
- [05]-[CONTRIBUTE]: `ClassifiedValue`, `Sensitivity`, `TelemetryContributorPort` — the sensitivity vocabulary and the one downward contribution fact a stratum hands a composing root.

## [02]-[CAPSULE]

- Owner: `KernelDomain` rows admit `Trace` at construction — `Trace` owns the span source and its generated key supplies the prefix `KernelPoint` seats its ids on, so span source and hook-point prefix are ONE derivation, never two spellings; `TraceBaggage` is the admitted W3C baggage value and `TraceCarrier` is the one causal-edge owner, capturing the W3C `traceparent`/`tracestate`/`baggage` triplet where a producing span is live and projecting its parsed parent back as an ingress parent or batch link; `SpanEdge` is the one bracket carriage binding span kind, that parent, and that edge set; `SpanBand` owns every admitted scope's `ActivitySource` and conforms to the hook bus's bracket floor.
- Cases: three span shapes off one carriage — a descendant bracket taking the carriage default, an ingress bracket adopting one inbound parent under a consumer or server kind, and a fan-in bracket carrying one link per upstream operation. Two result shapes: a synchronous `Fin` arm brackets with `using`, an effectful `IO` arm brackets through `IO`, and both resolve the same admitted-scope table and the same carriage.
- Entry: `TraceCarrier.Of(Activity?)` delegates capture of all three W3C fields to the in-box propagator, `Admit(traceParent, traceState, baggage)` delegates foreign-field parsing to that same codec, `Parent` reconstructs the admitted context, and `Link(facts)` projects a fan-in edge over that parse; `SpanEdge.Under(carrier, kind)` and `SpanEdge.FanIn(links, kind)` fold either into the trailing carriage both `Traced` result shapes take; `SpanBand.Of(version, planes)` mints the band and `Names` projects the scope names a tracer provider registers.
- Law: `KernelDomain` is a hand-kept MIRROR of the kernel's own sub-domain folder set — the roster and `ARCHITECTURE` `[01]` move as one edit, and a new sub-domain lands in both places or the span source it needs does not exist. `KernelDomain` states that mirror rather than deriving it, because a folder set is a repository fact no type can read.
- Law: `SpanBand` conforms to `IHookSpan` by taking the PLANE as an argument, so one band serves every roster plane a composition mounts and the bus hands the point's own `Plane` at the fire site; a hook bus therefore composes tracing through the bracket floor `Domain/hooks` declares and never through this type — the dependency points downward and the plane binds per `Traced` fire, because the floor's `Traced` carries a key and a body and no scope.
- Law: every capture funnels through `Op.Catch` on both result shapes, so a throwing body inside a bracket parks as a typed refusal with its cancellation identity intact rather than escaping past the `using` that owns the span.
- Law: `DistributedContextPropagator.CreateW3CPropagator()` is the ONE grammar and budget owner for the carrier. `TraceBaggage` is constructed only from its extracted members, the carrier exposes no arbitrary header dictionary, and event producers project its admitted wire value rather than re-parsing or re-formatting baggage locally.
- Exemption: `Dispose` sweeps the frozen source set with a statement loop — the table is disposed once at composition teardown and every source is disposed even though the sweep carries no result.
- Law: a failing result lands `SetStatus(ActivityStatusCode.Error, message)` — the typed verdict, never a boolean error tag — and stamps the generated `FaultId` as the `rasm.fault.code`/`rasm.fault.case` pair a trace query groups on, behind `IsAllDataRequested` so an unsampled span pays the status alone; `HasListeners` gates every bracket, so an unlistened span costs one null test, and an expected `Fault` never fabricates an exception event to carry a tag.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`System.Collections.Frozen`, `System.Diagnostics` including `DistributedContextPropagator`).
- Growth: a new sub-domain is one `KernelDomain` row, span source and point prefix deriving; a package trace plane is one `TraceScope` row admitted when the composition mints its band; a new bracket coordinate is one `SpanEdge` column every bracket already threads.
- Boundary: edge shape follows producer arity, and `SpanEdge` is where that choice lands — a batch relaying N durable rows descends from no single producer, so a parent edge to any one of them fabricates a causal chain the batch never had while the link set states exactly what caused it; a single-producer hop is the inverse, an ingress adopting one carrier through `Under` continuing the producing trace id where a link roots an orphan trace no query joins to its cause. Kind rides that same carriage because a remote-parented bracket declaring the internal default misreports the topology every backend derives from the kind column. Edges ride the START call because the sampler votes once at creation, and a producer whose span was unlistened carries the absent carrier; absence never fabricates trace context or baggage.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Diagnostics;
using Thinktecture;

namespace Rasm.Domain;

// --- [TYPES] ---------------------------------------------------------------------------
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

    private KernelDomain(string key) : this(TraceScope.Create(value: $"rasm.rasm.{key}")) { }

    public TraceScope Trace { get; }
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record TraceBaggage {
    internal TraceBaggage(string value, Seq<KeyValuePair<string, string?>> entries) =>
        (Value, Entries) = (value, entries);

    public string Value { get; }
    public Seq<KeyValuePair<string, string?>> Entries { get; }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct TraceCarrier {
    static readonly DistributedContextPropagator W3C = DistributedContextPropagator.CreateW3CPropagator();

    TraceCarrier(string? traceParent, string? traceState, Option<TraceBaggage> baggage) =>
        (TraceParent, TraceState, Baggage) = (traceParent, traceState, baggage);

    public string? TraceParent { get; }
    public string? TraceState { get; }
    public Option<TraceBaggage> Baggage { get; }

    public static TraceCarrier Of(Activity? span) {
        TraceFields fields = new();
        W3C.Inject(span, fields, TraceFields.Set);
        return Admit(fields.TraceParent, fields.TraceState, fields.Baggage);
    }

    public static TraceCarrier Admit(string? traceParent, string? traceState, string? baggage) {
        TraceFields fields = new(traceParent, traceState, baggage);
        W3C.ExtractTraceIdAndState(fields, TraceFields.Get, out string? admittedParent, out string? admittedState);
        admittedState = admittedParent is null ? null : admittedState;
        IEnumerable<KeyValuePair<string, string?>> parsed = W3C.ExtractBaggage(fields, TraceFields.Get) ?? [];
        Seq<KeyValuePair<string, string?>> entries = toSeq(parsed);
        Option<TraceBaggage> admittedBaggage = !string.IsNullOrWhiteSpace(fields.Baggage) && !entries.IsEmpty
            ? Some(new TraceBaggage(fields.Baggage, entries))
            : None;
        return new TraceCarrier(admittedParent, admittedState, admittedBaggage);
    }

    public Option<ActivityContext> Parent =>
        ActivityContext.TryParse(TraceParent, TraceState, isRemote: true, out ActivityContext context) ? Some(context) : None;

    public Option<ActivityLink> Link(params ReadOnlySpan<(string Slot, object? Value)> facts) {
        ActivityTagsCollection? tags = facts.IsEmpty
            ? null
            : new ActivityTagsCollection(InstrumentSet.Tags(TenantContext.Current, facts));
        return Parent.Map(context => new ActivityLink(context, tags));
    }
}

file sealed class TraceFields(string? traceParent = null, string? traceState = null, string? baggage = null) {
    public string? TraceParent { get; private set; } = traceParent;
    public string? TraceState { get; private set; } = traceState;
    public string? Baggage { get; private set; } = baggage;

    public static void Set(object? carrier, string field, string value) {
        TraceFields fields = (TraceFields)carrier!;
        switch (field) {
            case "traceparent": fields.TraceParent = value; break;
            case "tracestate": fields.TraceState = value; break;
            case "baggage": fields.Baggage = value; break;
        }
    }

    public static void Get(
        object? carrier,
        string field,
        out string? value,
        out IEnumerable<string>? values) {
        TraceFields fields = (TraceFields)carrier!;
        value = field switch {
            "traceparent" => fields.TraceParent,
            "tracestate" => fields.TraceState,
            "baggage" => fields.Baggage,
            _ => null,
        };
        values = null;
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct SpanEdge(ActivityKind Kind, Option<ActivityContext> Parent, Seq<ActivityLink> Links) {
    public static SpanEdge Under(TraceCarrier carrier, ActivityKind kind = ActivityKind.Consumer) =>
        new(kind, carrier.Parent, Seq<ActivityLink>());

    public static SpanEdge FanIn(Seq<ActivityLink> links, ActivityKind kind = ActivityKind.Internal) =>
        new(kind, Option<ActivityContext>.None, links);

    public ActivityContext Context => Parent.IfNone(default(ActivityContext));

    public IEnumerable<ActivityLink>? Edges => Links.IsEmpty ? null : Links;
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class SpanBand : IDisposable, IHookSpan {
    private readonly FrozenDictionary<TraceScope, ActivitySource> sources;

    private SpanBand(FrozenDictionary<TraceScope, ActivitySource> sources) => this.sources = sources;

    public static SpanBand Of(string version, params ReadOnlySpan<TraceScope> planes) =>
        new(toSeq(KernelDomain.Items).Map(static row => row.Trace)
            .Concat(Iterable<TraceScope>.FromSpan(planes).ToSeq())
            .Distinct()
            .ToFrozenDictionary(static scope => scope, scope => new ActivitySource(scope.ToString(), version)));

    public Seq<string> Names => toSeq(sources.Values).Map(static source => source.Name).Strict();

    public Fin<T> Traced<T>(TraceScope plane, Func<Fin<T>> body) => Traced(plane, _ => body());

    public Fin<T> Traced<T>(TraceScope scope, Func<Activity?, Fin<T>> body, SpanEdge edge = default) {
        if (!sources.TryGetValue(scope, out ActivitySource? source)) { return Fin.Fail<T>(Unadmitted(scope)); }
        if (!source.HasListeners()) { return Try.lift(() => body(null)).Run().Bind(static inner => inner); }
        using Activity? span = source.StartActivity(key.ToString(), edge.Kind, edge.Context, tags: null, links: edge.Edges);
        return Try.lift(() => body(span)).Run().Bind(static inner => inner).MapFail(error => Marked(span, error));
    }

    public IO<T> Traced<T>(TraceScope scope, Func<Activity?, IO<T>> body, SpanEdge edge = default) =>
        !sources.TryGetValue(scope, out ActivitySource? source)
            ? IO.fail<T>(Unadmitted(scope))
            : !source.HasListeners()
            ? IO.lift(() => Try.lift(() => Fin.Succ(body(null))).Run().Bind(static inner => inner)).Bind(static effect => effect)
            : IO.lift(() => source.StartActivity(key.ToString(), edge.Kind, edge.Context, tags: null, links: edge.Edges))
                .Bracket(
                    Use: span => (IO.lift(() => Try.lift(() => Fin.Succ(body(span))).Run().Bind(static inner => inner)).Bind(static effect => effect) | @catch<IO, T>(static _ => true, error => IO.fail<T>(Marked(span, error)))).As(),
                    Fin: static span => IO.lift(() => ignore(span?.Dispose())));

    public void Dispose() {
        foreach (ActivitySource source in sources.Values) { source.Dispose(); }
    }

    private static Error Unadmitted(TraceScope scope) =>
        new KernelFault.InvalidValue(Label: scope.ToString(), Requirement: "a trace scope admitted at band composition");

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

- Owner: `CostMark` is the capture pair — a monotonic tick and the thread allocation counter, minted before the guarded work and folded by `Stop` into `Cost`; `Cost` is the uniform per-op evidence the app strata attribute to tenants, and its `Succeeded` column is the settled verdict every emitted outcome dimension reads.
- Entry: `CostMark.Start()` mints the capture and `Stop` folds it into `Cost`, carrying the settled verdict as the `Succeeded` column.
- Law: one capture per operation runtime — the operation marks before its body fold, the admission gate sits inside the marked window so admission cost charges to the operation that demanded it, and BOTH exits charge: the success leg records `Succeeded: true`, the fail leg `Succeeded: false` and publishes the fault fact, so cost and failure evidence never diverge and the outcome dimension keeps the two populations separable on one series.
- Law: allocation delta is thread-local evidence, valid because the synchronous runtime runs the marked window on one thread; a thread-hopping lane keeps elapsed truth and reads the delta as an allocation floor, never a total.
- Law: `Cost` registers `IValidityEvidence`, so the fact reaches the one acceptance oracle like every kernel result.
- Packages: LanguageExt.Core, BCL inbox (`System.Diagnostics`).
- Growth: a third settled verdict is a deliberate data-shape change replacing the `Succeeded` column whole, never a case bolted beside a Boolean it mirrors.
- Boundary: the fabric never wraps a second timer or a sampling profiler — profile capture is the app stratum's, this row the per-op scalar truth.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Diagnostics;

namespace Rasm.Domain;

// --- [MODELS] --------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct Cost(KernelDomain Domain, TimeSpan Elapsed, long AllocatedBytes, int Items, bool Succeeded)
    : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Nonnegative(value: Elapsed.TotalSeconds),
        AllocatedBytes >= 0L,
        ValidityClaim.CountAtLeast(count: Items, floor: 0));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct CostMark(long Timestamp, long Allocated) {
    public static CostMark Start() =>
        new(Timestamp: Stopwatch.GetTimestamp(), Allocated: GC.GetAllocatedBytesForCurrentThread());

    public Cost Stop(KernelDomain domain, int items, bool succeeded) =>
        new(Domain: domain,
            Elapsed: Stopwatch.GetElapsedTime(startingTimestamp: Timestamp),
            AllocatedBytes: long.Max(0L, GC.GetAllocatedBytesForCurrentThread() - Allocated),
            Items: items, Succeeded: succeeded);
}
```

## [04]-[TAP]

- Owner: `FaultObservation` is the bounded structured projection of an `Error`, carrying the generated `FaultId` WHOLE so code and case token stay one member with one authority; `PointFacet` closes what a signal fact IS; `KernelPoint` is the kernel's own `IHookRoster` realization over the sub-domain and facet coordinates; `SignalFact`'s abstract `At` projects each case's own stored ROSTER ROW, so identity moves `with`-safe and no fact carries a spelling; `SignalHooks` is the emission capsule over `Domain/hooks`'s one mechanism; `KernelInstrument` is the kernel's own instrument roster, each row CARRYING its declaration; `TelemetrySink` is the composition capsule an app stratum threads.
- Cases: `FaultCase` and `CostCase`, one per `PointFacet` row; each factory resolves its seat through `KernelPoint.Of(domain, facet)` and none renders an id. A produced result is no hook fact — it returns in the result, and its validity is the `IValidityEvidence` fold its caller reads.
- Entry: `FaultObservation.Of(error)` projects the generated `FaultId` — transported code beside local case token — typed recovery, and bounded exact cause stamps without rendering `Message`; `KernelPoint.Of(domain, facet)` is the ONE seat mint, `SignalHooks.Of` mounts the bus over the composition's evidence cell, `Publish` is the unary fire, and `TelemetrySink.Tap` is the ONE emission entry.
- Auto: the tap is TWO inline generated `Switch` projections inside `Tap` and ONE fold — one answers the rows a case bills and their measurements, the other the dimensions it carries — and the gate, the tag mint, and the write fold run once for every case; the prior form spelled gate, mint, and write three times, so a fourth case meant a fourth body rather than two arms.
- Auto: `KernelInstrument` rows carry their own `InstrumentSpec` and `Rows` derives from `Items`, so the const-name roster and the hand-listed sequence that mirrored it are one declaration; the private constructor derives the roster key from the carried declaration name, so a key-name mismatch is unrepresentable rather than checked.
- Law: the roster is DERIVED, never listed — `KernelPoint.Items` materializes the whole sub-domain by facet product at type init, so a tenth sub-domain lands three seats with no edit here and a folder minting an inline `HookId.Create` literal does not compile (branch RULINGS `[02]`). The declare-or-resolve transition a runtime id space needed has no spelling left: a point outside the roster is unrepresentable rather than merely undeclared.
- Law: `SignalHooks` governs EMISSION alone — it publishes the unary fire and the mechanism it holds, and never the guarded `Fire<T>` arity, so a gate guarding geometry, a lease, or a handle declares its own roster and its own closed fact union rather than borrowing the signal plane's (`Rasm` RULINGS `[02]`).
- Law: publication LEADS the write, and the instruments measure the PUBLISHED fact — the bus is a hook plane whose subscribers veto and transform, so a write ahead of it meters a fact a veto then withheld and meters the pre-admission shape of one a subscriber revised.
- Law: instrument refusals ACCUMULATE and settle the returned result beside the already-published fact — every charged row is attempted, so a mount defect on one row never silences the two beside it and never silences the hook plane.
- Law: fault observation traverses aggregate MEMBERSHIP and causal `Inner` separately under one fixed ceiling; each retained cause carries the generated `FaultId` or the exact exception `Type` and `HResult`, and `Truncated` states when more evidence existed. Message text, category, owner, and a wire discriminant never enter the projection.
- Law: `KernelInstrument` owns the whole `rasm.fault.*` key family and each key states where it may be read — `OwnerSlot` and `PostureSlot` are the bounded pair the kernel counter mounts, `CodeSlot` an owner-specific opt-in metric dimension, `CaseSlot` a span tag and log field alone. A metric mounting the case token buys code-cardinality series for a spelling the code already keys, and a lowering copying it forks one identity into two a peer then joins on; an emitter prefixing its own project segment onto a fault axis forks one solution-wide dimension into a per-package pair no board can group, which is the fork this one roster forecloses. The posture VALUE is `Domain/results`'s `Retriability.Key`, so the key and the word it carries each have one owner.
- Exemption: the write fold is a statement form because a `TagList` cannot cross a lambda; the listener gate precedes it, so a process with no exporter and no armed tally pays the key render, the boxed columns, and the tag fold on no operation.
- Law: fact payloads are evidence, never live resources — `FaultCase` carries the already-lowered `Error` (both the substrate `Fault` union and the band-relative geometry faults arrive as `Error`, so one case serves both) and `CostCase` the settled `Cost`; no case retains geometry, leases, or handles, and both fault families land in ONE dimension-discriminated counter, never two.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`System.Collections.Frozen`, `System.Collections.Immutable`, `System.Diagnostics`, `System.Diagnostics.Metrics`).
- Growth: a new fact kind is one `PointFacet` row, one `SignalFact` case, and one arm in each of the two projections, both broken loudly by the generated `Switch` while the roster grows a seat per sub-domain unedited; a new kernel instrument is one `KernelInstrument` row and one row in the inline charged projection.
- Boundary: `SignalFact` holds evidence over live resources, so a fact retains no geometry, lease, or handle and a subscriber reading one holds nothing the emitter must keep alive. `TelemetrySink` is composition-entered: an app stratum mints one per composition and threads it, and a kernel page never constructs, caches, or reaches an ambient sink. Quiet-path cost is structural — a subscriber-empty point folds an empty veto sequence and iterates an empty tap sequence, so a publish costs one keyed lookup and allocates nothing past its result.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Thinktecture;

namespace Rasm.Domain;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PointFacet {
    public static readonly PointFacet Cost = new("cost");
    public static readonly PointFacet Fault = new("fault");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class KernelInstrument {
    public const string OpSlot = "rasm.kernel.op";
    public const string DomainSlot = "rasm.kernel.domain";
    public const string OutcomeSlot = "rasm.kernel.outcome";
    public const string OwnerSlot = "rasm.fault.owner";
    public const string PostureSlot = "rasm.fault.posture";
    public const string CodeSlot = "rasm.fault.code";
    public const string CaseSlot = "rasm.fault.case";

    public static readonly KernelInstrument Duration = new(InstrumentSpec.Create(
        "rasm.kernel.op.duration", InstrumentKind.Distribution, MeasureForm.Real, "s",
        "Kernel operation wall time.", Seq(OpSlot, DomainSlot, OutcomeSlot), Some(Buckets.BenchSeconds), None, None));

    public static readonly KernelInstrument Allocated = new(InstrumentSpec.Create(
        "rasm.kernel.op.allocated", InstrumentKind.Distribution, MeasureForm.Whole, "By",
        "Kernel operation allocated bytes.", Seq(OpSlot, DomainSlot, OutcomeSlot), Some(Buckets.ByteSizes), None, None));

    public static readonly KernelInstrument Counted = new(InstrumentSpec.Create(
        "rasm.kernel.op.items", InstrumentKind.Distribution, MeasureForm.Whole, "{item}",
        "Kernel operation item count.", Seq(OpSlot, DomainSlot, OutcomeSlot), Some(Buckets.GraphCounts), None, None));

    public static readonly KernelInstrument Faults = new(InstrumentSpec.Create(
        "rasm.kernel.fault.count", InstrumentKind.Count, MeasureForm.Whole, "{fault}",
        "Kernel fault stream by owning package and recovery posture.", Seq(OwnerSlot, PostureSlot), None, None, None));

    private KernelInstrument(InstrumentSpec row) : this(row.Name, row) { }

    public InstrumentSpec Row { get; }

    public static Seq<InstrumentSpec> Rows => toSeq(Items).Map(static row => row.Row).Strict();
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct FaultCauseStamp(Option<FaultId> Identity, Option<Type> ExceptionType, Option<int> HResult);

public sealed record FaultObservation(
    Option<FaultId> Identity, Retriability Recovery, Seq<FaultCauseStamp> Causes, bool Truncated) {
    public Option<int> Code => Identity.Map(static id => id.Code);

    public static FaultObservation Of(Error error) {
        ArgumentNullException.ThrowIfNull(error);
        const int causeCeiling = 8;
        Queue<Error> pending = new();
        pending.Enqueue(error);
        Seq<FaultCauseStamp> causes = Seq<FaultCauseStamp>();
        int inspected = 0;
        while (inspected < causeCeiling && pending.TryDequeue(out Error? current)) {
            bool root = inspected++ == 0;
            Option<FaultId> identity = current is Fault fault ? Some(fault.Identity) : None;
            Option<Exception> exception = current is Fault ? None : current.Exception;
            if ((!root && identity.IsSome) || exception.IsSome) {
                causes = causes.Add(new FaultCauseStamp(
                    Identity: identity,
                    ExceptionType: exception.Map(static raised => raised.GetType()),
                    HResult: exception.Map(static raised => raised.HResult)));
            }
            if (current is ManyErrors many) { many.Errors.Iter(pending.Enqueue); }
            current.Inner.Iter(pending.Enqueue);
        }
        return new FaultObservation(
            Identity: error is Fault fault ? Some(fault.Identity) : None,
            Recovery: Redrive.Posture(error),
            Causes: causes,
            Truncated: pending.Count > 0);
    }
}

public sealed record KernelPoint : IHookRoster<KernelPoint> {
    private KernelPoint(KernelDomain domain, PointFacet facet) =>
        (Domain, Facet, Id) = (domain, facet, HookId.Create(value: $"{domain.Trace.ToValue()}.{facet.Key}"));

    public KernelDomain Domain { get; }
    public PointFacet Facet { get; }
    public HookId Id { get; }

    public CapabilitySet<HookModality> Modalities => Emission;

    public Option<TraceScope> Plane => Some(Domain.Trace);

    public static IReadOnlyList<KernelPoint> Items => Roster.Value;

    public static KernelPoint Of(KernelDomain domain, PointFacet facet) => Index.Value[(domain, facet)];

    private static readonly CapabilitySet<HookModality> Emission =
        CapabilitySet<HookModality>.Of(HookModality.Veto, HookModality.Observe);

    private static readonly Lazy<ImmutableArray<KernelPoint>> Roster = new(
        static () => [.. from domain in KernelDomain.Items
                         from facet in PointFacet.Items
                         select new KernelPoint(domain: domain, facet: facet)]);

    private static readonly Lazy<FrozenDictionary<(KernelDomain Domain, PointFacet Facet), KernelPoint>> Index = new(
        static () => Roster.Value.ToFrozenDictionary(static row => (row.Domain, row.Facet)));
}

[Union]
public abstract partial record SignalFact : IHookFact<KernelPoint> {
    private SignalFact() { }

    public abstract KernelPoint At { get; }
    public bool Seats(KernelPoint at) => At == at;

    public sealed record FaultCase(KernelPoint Point, Error Fault) : SignalFact { public override KernelPoint At => Point; }
    public sealed record CostCase(KernelPoint Point, Cost Cost) : SignalFact { public override KernelPoint At => Point; }

    public static SignalFact Fault(KernelDomain domain, Error fault) => new FaultCase(KernelPoint.Of(domain, PointFacet.Fault), fault);
    public static SignalFact Cost(Cost cost) => new CostCase(KernelPoint.Of(cost.Domain, PointFacet.Cost), cost);
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class SignalHooks {
    private SignalHooks(HookSet<KernelPoint, SignalFact, TelemetrySource> mounted) => Hooks = mounted;

    public HookSet<KernelPoint, SignalFact, TelemetrySource> Hooks { get; }

    public static Fin<SignalHooks> Of(
        FaultCell faults,
        Seq<HookGate<KernelPoint, SignalFact, TelemetrySource>> gates = default,
        Seq<HookTap<KernelPoint, SignalFact, TelemetrySource>> taps = default,
        Option<IHookSpan> span = default) =>
        HookSet<KernelPoint, SignalFact, TelemetrySource>.Of(gates: gates, taps: taps, span: span, cell: Some(faults))
            .Map(static mounted => new SignalHooks(mounted: mounted));

    public Fin<SignalFact> Publish(SignalFact fact) => Hooks.Fire(at: fact.At, fact: fact);
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public sealed class TelemetrySink {
    private readonly InstrumentSet set;

    private TelemetrySink(SignalHooks hooks, InstrumentSet mounted) => (Signals, set) = (hooks, mounted);

    public SignalHooks Signals { get; }

    public static Fin<TelemetrySink> Of(IMeterFactory factory, string version, FaultCell faults) =>
        from hooks in SignalHooks.Of(faults: faults)
        from mounted in InstrumentSet.Of(
            new LevelCells(),
            (TelemetryIdentity.Metered(factory, TelemetrySource.Kernel, version), KernelInstrument.Rows))
        select new TelemetrySink(hooks: hooks, mounted: mounted);

    public Fin<SignalFact> Tap(SignalFact fact) =>
        Signals.Publish(fact: fact).Bind(published => {
            Seq<(InstrumentSpec Row, double Value)> charged = published.Switch(
                faultCase: static _ => Seq((KernelInstrument.Faults.Row, 1d)),
                costCase: static row => Seq(
                    (KernelInstrument.Duration.Row, row.Cost.Elapsed.TotalSeconds),
                    (KernelInstrument.Allocated.Row, (double)row.Cost.AllocatedBytes),
                    (KernelInstrument.Counted.Row, (double)row.Cost.Items)));
            if (!set.Enabled(charged.Map(static row => row.Row))) { return Fin.Succ(published); }
            TagList tags = published.Switch(
                state: TenantContext.Current,
                faultCase: static (tenant, row) => InstrumentSet.Tags(tenant,
                    (KernelInstrument.OwnerSlot, row.Fault.Owner.Map(static owner => owner.Key).Match<object?>(Some: static owner => owner, None: static () => null)),
                    (KernelInstrument.PostureSlot, Redrive.Posture(row.Fault).Key)),
                costCase: static (tenant, row) => InstrumentSet.Tags(tenant,
                    (KernelInstrument.OpSlot, (object?)row.Cost.Key.ToString()),
                    (KernelInstrument.DomainSlot, row.Cost.Domain.Key),
                    (KernelInstrument.OutcomeSlot, row.Cost.Succeeded)));
            Seq<Error> refusals = Seq<Error>();
            foreach ((InstrumentSpec row, double value) in charged) {
                refusals = set.Write(row: row, measurement: value, tags: in tags)
                    .Match(Succ: _ => refusals, Fail: cause => refusals.Add(cause));
            }
            return refusals
                .Fold(Option<Error>.None, static (seat, cause) => Some(seat.Match(Some: first => first + cause, None: () => cause)))
                .Match(Some: Fin.Fail<SignalFact>, None: () => Fin.Succ(published));
        });
}
```

## [05]-[CONTRIBUTE]

- Owner: `ClassifiedValue` carries a sensitivity annotation as text; `Sensitivity` is the kernel's own sensitivity roster; `TelemetryContributorPort` is the ONE downward contribution fact a stratum hands a composing root — its instrument roster, published handles, trace planes, classifications, and board.
- Entry: `Kernel(version)` mints the kernel's own port; `Admit` builds the port's whole declaration map by name and admits the optional board pack against it, so a mounting root folds every contributor before it mints a meter.
- Law: a producer measures at its own site — `InstrumentSet.Write`/`Level`/`Bind` against the rows this port declared, `SpanBand.Traced` for the bracket, `RasmEventEnvelope.Publish` for a durable fact — so no projection table, kind roster, or fan stands between a result and the instrument that observes it.
- Law: `Admit` names WHICH declaration collided — a refusal that states only that some name repeats leaves a mounting root to diff two rosters by hand.
- Law: the port names its scope with the branch's own `TelemetrySource` row rather than a bare string, so a contributor cannot fork the package census the fault-band registry and the causal frame already key on; the schema pin is read, never passed, so no contributor can break the one coordinate tracer, meter, and logger bump together on.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a contributor's whole board and reliability policy is one `Board` value on its own port, its whole span custody one `Planes` roster on that same port, and a newly annotated sensitivity one `ClassifiedValue` row on its `Classifications` column.
- Boundary: `Instruments` and `Published` split by WHO MOUNTS — the root binds handles for the first and a contributor owning its own meter lifetime declares the second, `Declared` is the union every naming gate, view predicate, and pack admission reads, and a row on neither roster exports a stream no gate can refuse. `Planes` carries the contributor's own `TraceScope` roster VERBATIM, because trace and meter scopes are distinct grammars neither derives from. `Classifications` carries sensitivity VALUES as `(taxonomy, value)` text, so no compliance type enters this assembly and a redaction root binding a redactor per rostered row has a set to PROVE its contributors against instead of a coincidence it discovers at egress.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Thinktecture;

namespace Rasm.Domain;

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct ClassifiedValue(string Taxonomy, string Value);

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Sensitivity {
    public const string Taxonomy = "DataClassification";
    public static readonly Sensitivity UserContent = new(key: "user-content");
    public static readonly Sensitivity HostPath = new(key: "host-path");
    public static readonly Sensitivity MachineIdentity = new(key: "host-identity");
    public static readonly Sensitivity AccountIdentity = new(key: "personal");
    public static Seq<ClassifiedValue> Values => toSeq(Items).Map(static row => new ClassifiedValue(Taxonomy, row.Key));
}

// --- [COMPOSITION] ---------------------------------------------------------------------
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

    public Fin<Unit> Admit() =>
        (Declared.Collisions(static row => row.Name) is { IsEmpty: false } collided
            ? Fin.Fail<HashMap<string, InstrumentSpec>>(new KernelFault.InvalidValue(
                Label: string.Join(", ", collided),
                Requirement: "one declaration per name across the mounted and published columns"))
            : Fin.Succ(Declared.ToHashMap(static row => row.Name, static row => row)))
        .Bind(roster => Board.TraverseM(pack => pack.Admit(roster)).As())
        .Map(static _ => unit);
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
