# `Domain/telemetry.md` Surgical Refinement Audit

Target: `libs/dotnet/Rasm/.planning/Domain/telemetry.md`

Apply the moves in order. Counts are authored, nonblank C# fence lines; generated Thinktecture members are not counted as fenced LOC. The ordered result removes 37 fenced LOC, one module-level type, and eleven declared type members (nine public, two private). It also removes two public parameters without adding a type, helper, enum, or alternate result rail.

## 1. Let each `KernelDomain` row own its `TraceScope`

The smart-enum row already owns the stable domain key. Carry its derived trace scope as a plain generated column instead of maintaining a second lazy dictionary indexed by the same rows.

**Location:** `libs/dotnet/Rasm/.planning/Domain/telemetry.md:33,54-59,388-389`, anchors `using System.Threading;`, `Scopes`, `Trace`, `SourceName`, and `private KernelPoint`

**From:**

```csharp
using System.Threading;

private static readonly Lazy<FrozenDictionary<KernelDomain, TraceScope>> Scopes = new(
    static () => Items.ToFrozenDictionary(static row => row, static row => TraceScope.Create(value: $"rasm.rasm.{row.Key}")),
    LazyThreadSafetyMode.ExecutionAndPublication);
public TraceScope Trace => Scopes.Value[this];
public string SourceName => Trace.ToString();

private KernelPoint(KernelDomain domain, PointFacet facet) =>
    (Domain, Facet, Id) = (domain, facet, HookId.Create(value: $"{domain.SourceName}.{facet.Key}"));
```

**To:**

```csharp
private KernelDomain(string key) : this(key, TraceScope.Create(value: $"rasm.rasm.{key}")) { }

public TraceScope Trace { get; }

private KernelPoint(KernelDomain domain, PointFacet facet) =>
    (Domain, Facet, Id) = (domain, facet, HookId.Create(value: $"{domain.Trace.ToValue()}.{facet.Key}"));
```

**Effect:** fenced LOC `8 -> 4` (`-4`); declared members net `-1` (`Scopes` and `SourceName` removed, one deriving constructor added); public members `-1`; one `Lazy<FrozenDictionary<KernelDomain, TraceScope>>`, one frozen dictionary, and every lookup through it are removed.

**Proof:** checked-in `libs/dotnet/.api/api-thinktecture-runtime-extensions.md` explicitly admits a hand-declared chaining constructor beside the generated smart-enum constructor and generates plain-column properties plus `ToValue()`. Each fixed row already enters through the one-argument constructor, so the new constructor derives exactly the former dictionary value from the same key and delegates it to the generated `(key, trace)` constructor. The corpus has no `Scopes` consumer and exactly one `SourceName` consumer, in `KernelPoint`; projecting the admitted `TraceScope` key there removes the forwarding string without changing the hook id. This is a direct row capability, not a cache requiring delayed generated-`Items` access.

**Same-file ripple:** at line 16, replace the claim that a row derives both `SourceName` and the hook prefix with the exact statement that `Trace` owns the source and its generated key supplies the point prefix. Remove `System.Threading` from the `[02]-[CAPSULE]` package list; move 2 removes it from `[04]-[TAP]`.

## 2. Remove redundant `Lazy<T>` machinery from `KernelPoint`

The immutable modality set has no generated-roster dependency, while the two roster-dependent caches preserve the BCL default lazy mode without spelling it.

**Location:** `libs/dotnet/Rasm/.planning/Domain/telemetry.md:289,395,403-415`, anchors `using System.Threading;`, `Modalities`, `Emission`, `Roster`, and `Index`

**From:**

```csharp
using System.Threading;

public CapabilitySet<HookModality> Modalities => Emission.Value;

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
```

**To:**

```csharp
public CapabilitySet<HookModality> Modalities => Emission;

private static readonly CapabilitySet<HookModality> Emission =
    CapabilitySet<HookModality>.Of(HookModality.Veto, HookModality.Observe);

private static readonly Lazy<ImmutableArray<KernelPoint>> Roster = new(
    static () => [.. from domain in KernelDomain.Items
                     from facet in PointFacet.Items
                     select new KernelPoint(domain: domain, facet: facet)]);

private static readonly Lazy<FrozenDictionary<(KernelDomain Domain, PointFacet Facet), KernelPoint>> Index = new(
    static () => Roster.Value.ToFrozenDictionary(static row => (row.Domain, row.Facet)));
```

**Effect:** fenced LOC `14 -> 10` (`-4`); declared symbols `0`; one unnecessary `Lazy<CapabilitySet<HookModality>>` allocation removed; behavior `0`.

**Proof:** the BCL `Lazy<T>(Func<T>)` constructor already selects `LazyThreadSafetyMode.ExecutionAndPublication`, so the two generated-roster-dependent caches preserve the same deferred, cached behavior without the explicit mode. `Emission` reads named `HookModality.Veto` and `Observe` fields directly, not the generated `Items` roster; touching those fields completes `HookModality` type initialization before `CapabilitySet.Of` runs. The capability set is immutable and safe to construct once at `KernelPoint` type initialization, so a second lazy owner adds neither ordering nor observable deferred work.

**Same-file ripple:** remove `System.Threading` from the `[04]-[TAP]` package list.

## 3. Make `SpanBand` an all-plane source table and close its deferred-capture gap

These two edits leave the owner smaller while making both deferred body factories obey the page's existing `Op.Catch` boundary.

### 3a. Remove the privileged plane

**Location:** `libs/dotnet/Rasm/.planning/Domain/telemetry.md:158-168`, anchors `private SpanBand` and `public static SpanBand Of`

**From:**

```csharp
private SpanBand(TraceScope plane, FrozenDictionary<TraceScope, ActivitySource> sources) =>
    (Plane, this.sources) = (plane, sources);

public TraceScope Plane { get; }

public static SpanBand Of(string version, TraceScope plane, params ReadOnlySpan<TraceScope> external) =>
    new(plane, toSeq(KernelDomain.Items).Map(static row => row.Trace)
        .Concat(toSeq(external.ToArray()))
        .Add(plane)
        .Distinct()
        .ToFrozenDictionary(static scope => scope, scope => new ActivitySource(scope.ToString(), version)));
```

**To:**

```csharp
private SpanBand(FrozenDictionary<TraceScope, ActivitySource> sources) => this.sources = sources;

public static SpanBand Of(string version, params ReadOnlySpan<TraceScope> planes) =>
    new(toSeq(KernelDomain.Items).Map(static row => row.Trace)
        .Concat(Iterable<TraceScope>.FromSpan(planes).ToSeq())
        .Distinct()
        .ToFrozenDictionary(static scope => scope, scope => new ActivitySource(scope.ToString(), version)));
```

### 3b. Defer both `IO` body factories through `Op.Catch`

**Location:** `libs/dotnet/Rasm/.planning/Domain/telemetry.md:181-189`, anchors `public IO<T> Traced<T>` and `Use: span =>`

**From:**

```csharp
: !source.HasListeners()
? body(null)
: IO.lift(() => source.StartActivity(key.ToString(), edge.Kind, edge.Context, tags: null, links: edge.Edges))
    .Bracket(
        Use: span => (body(span) | @catch<IO, T>(static _ => true, error => IO.fail<T>(Marked(span, error)))).As(),
```

**To:**

```csharp
: !source.HasListeners()
? IO.lift(() => key.Catch(() => Fin.Succ(body(null)))).Bind(static effect => effect)
: IO.lift(() => source.StartActivity(key.ToString(), edge.Kind, edge.Context, tags: null, links: edge.Edges))
    .Bracket(
        Use: span => (IO.lift(() => key.Catch(() => Fin.Succ(body(span)))).Bind(static effect => effect) | @catch<IO, T>(static _ => true, error => IO.fail<T>(Marked(span, error)))).As(),
```

**Effect:** fenced LOC `14 -> 11` across 3a-3b (`-3`); declared/public members `-1` (`SpanBand.Plane`); public factory parameters `3 -> 2`; private constructor parameters `2 -> 1`; deferred body-factory exceptions now use the same operation-key lowering as the synchronous overload.

**Proof:** `Plane` has no code-fence consumer anywhere in the planning corpus. The dictionary already is the complete admitted-source authority, and both `Traced` overloads take the actual `TraceScope` at the fire site. The checked-in `libs/dotnet/.api/api-diagnostics-activity.md` catalogue describes `SpanBand.Of(version, scopes)` and source lookup by the supplied scope. The live composing call at `libs/dotnet/Rasm.AppHost/.planning/Observability/telemetry.md:553` already has the denser shape `SpanBand.Of(resolved.ServiceVersion, [.. contributors.Bind(static port => port.Planes)])`; this change makes the owner match its consumer instead of preserving a stale singled-out plane. Checked-in `libs/dotnet/.api/api-languageext.md` names `Iterable<T>.FromSpan(ReadOnlySpan<T>)` as the one span-to-carrier lift, so the replacement also removes the hand `ToArray` hop while preserving the required frame copy. For the deferred overload, the same catalogue exposes both `IO.lift(Func<Fin<A>>)` and `IO<A>.Bind`; lifting `Fin<IO<T>>` then binding identity defers the factory, lowers a synchronous throw through `key.Catch`, and leaves the returned `IO<T>` on the existing runtime `@catch`/`Marked` rail. The current `body(null)` call occurs while constructing the effect and can throw before any `IO` exists.

**Same-file ripple:** at line 18, spell the entry `SpanBand.Of(version, planes)`; at line 20, retain the law that the plane binds per `Traced` fire and delete the contradictory statement that it binds at band composition.

## 4. Derive each `KernelInstrument` key from its carried declaration

Apply the constructor collapse first, then route each row through it. Every subchange stays below ten fenced lines.

### 4a. Replace the runtime mirror check with a deriving constructor

**Location:** `libs/dotnet/Rasm/.planning/Domain/telemetry.md:336-344`, anchors `public InstrumentSpec Row` and `ValidateConstructorArguments`

**From:**

```csharp
public InstrumentSpec Row { get; }

public static Seq<InstrumentSpec> Rows => toSeq(Items).Map(static row => row.Row).Strict();

static partial void ValidateConstructorArguments(ref string key, ref InstrumentSpec row) {
    if (!string.Equals(key, row.Name, StringComparison.Ordinal)) {
        throw new ArgumentException($"<kernel-instrument:{key}>", nameof(row));
    }
}
```

**To:**

```csharp
private KernelInstrument(InstrumentSpec row) : this(row.Name, row) { }

public InstrumentSpec Row { get; }

public static Seq<InstrumentSpec> Rows => toSeq(Items).Map(static row => row.Row).Strict();
```

### 4b. Duration row

**Location:** `libs/dotnet/Rasm/.planning/Domain/telemetry.md:316-319`, anchor `KernelInstrument Duration`

**From:**

```csharp
public static readonly KernelInstrument Duration = new(
    "rasm.kernel.op.duration",
    InstrumentSpec.Create("rasm.kernel.op.duration", InstrumentKind.Distribution, MeasureForm.Real, "s",
        "Kernel operation wall time.", Seq(OpSlot, DomainSlot, OutcomeSlot), Some(Buckets.BenchSeconds), None, None));
```

**To:**

```csharp
public static readonly KernelInstrument Duration = new(InstrumentSpec.Create(
    "rasm.kernel.op.duration", InstrumentKind.Distribution, MeasureForm.Real, "s",
    "Kernel operation wall time.", Seq(OpSlot, DomainSlot, OutcomeSlot), Some(Buckets.BenchSeconds), None, None));
```

### 4c. Allocation row

**Location:** `libs/dotnet/Rasm/.planning/Domain/telemetry.md:321-324`, anchor `KernelInstrument Allocated`

**From:**

```csharp
public static readonly KernelInstrument Allocated = new(
    "rasm.kernel.op.allocated",
    InstrumentSpec.Create("rasm.kernel.op.allocated", InstrumentKind.Distribution, MeasureForm.Whole, "By",
        "Kernel operation allocated bytes.", Seq(OpSlot, DomainSlot, OutcomeSlot), Some(Buckets.ByteSizes), None, None));
```

**To:**

```csharp
public static readonly KernelInstrument Allocated = new(InstrumentSpec.Create(
    "rasm.kernel.op.allocated", InstrumentKind.Distribution, MeasureForm.Whole, "By",
    "Kernel operation allocated bytes.", Seq(OpSlot, DomainSlot, OutcomeSlot), Some(Buckets.ByteSizes), None, None));
```

### 4d. Item-count row

**Location:** `libs/dotnet/Rasm/.planning/Domain/telemetry.md:326-329`, anchor `KernelInstrument Counted`

**From:**

```csharp
public static readonly KernelInstrument Counted = new(
    "rasm.kernel.op.items",
    InstrumentSpec.Create("rasm.kernel.op.items", InstrumentKind.Distribution, MeasureForm.Whole, "{item}",
        "Kernel operation item count.", Seq(OpSlot, DomainSlot, OutcomeSlot), Some(Buckets.GraphCounts), None, None));
```

**To:**

```csharp
public static readonly KernelInstrument Counted = new(InstrumentSpec.Create(
    "rasm.kernel.op.items", InstrumentKind.Distribution, MeasureForm.Whole, "{item}",
    "Kernel operation item count.", Seq(OpSlot, DomainSlot, OutcomeSlot), Some(Buckets.GraphCounts), None, None));
```

### 4e. Fault-count row

**Location:** `libs/dotnet/Rasm/.planning/Domain/telemetry.md:331-334`, anchor `KernelInstrument Faults`

**From:**

```csharp
public static readonly KernelInstrument Faults = new(
    "rasm.kernel.fault.count",
    InstrumentSpec.Create("rasm.kernel.fault.count", InstrumentKind.Count, MeasureForm.Whole, "{fault}",
        "Kernel fault stream by owning package and recovery posture.", Seq(OwnerSlot, PostureSlot), None, None, None));
```

**To:**

```csharp
public static readonly KernelInstrument Faults = new(InstrumentSpec.Create(
    "rasm.kernel.fault.count", InstrumentKind.Count, MeasureForm.Whole, "{fault}",
    "Kernel fault stream by owning package and recovery posture.", Seq(OwnerSlot, PostureSlot), None, None, None));
```

**Effect:** fenced LOC `23 -> 15` (`-8`); declared symbols net `0` (one private forwarding constructor replaces one private validation hook); repeated instrument-name literals `8 -> 4`; runtime mirror checks `4 -> 0`.

**Proof:** `InstrumentSpec.Name` is already the admitted declaration identity used by `InstrumentSet`, contributor collision checks, and board admission. Checked-in `libs/dotnet/.api/api-thinktecture-runtime-extensions.md` explicitly admits a hand-declared chaining constructor beside the generated smart-enum constructor; for a keyed smart enum with one plain instance member, that generated constructor is `(string key, InstrumentSpec row)`. Deriving `key` from `row.Name` makes mismatch unrepresentable and deletes the validation hook rather than merely moving its comparison. All consumers continue to read the generated smart-enum key and `Row` unchanged.

**Same-file ripple:** at line 270, replace the runtime-validation claim with the stronger statement that the private constructor derives the roster key from the carried declaration name.

## 5. Replace the two-case payloadless `Outcome` family with the Boolean it wraps

This collapse removes an entire generated owner whose only authored behavior widens a `bool` back into two names.

### 5a. Delete the type and carry the settled bit on `OpCost`

**Location:** `libs/dotnet/Rasm/.planning/Domain/telemetry.md:225,229-243`, anchors `using Thinktecture;`, `public sealed partial class Outcome`, and `public readonly record struct OpCost`

**From:**

```csharp
using Thinktecture;

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Outcome {
    public static readonly Outcome Succeeded = new("succeeded");
    public static readonly Outcome Failed = new("failed");

    public static Outcome Of(bool settled) => settled ? Succeeded : Failed;
}

public readonly record struct OpCost(
    Op Key, KernelDomain Domain, TimeSpan Elapsed, long AllocatedBytes, int Items, Outcome Outcome)
```

**To:**

```csharp
public readonly record struct OpCost(
    Op Key, KernelDomain Domain, TimeSpan Elapsed, long AllocatedBytes, int Items, bool Succeeded)
```

### 5b. Thread the Boolean through `CostMark.Stop`

**Location:** `libs/dotnet/Rasm/.planning/Domain/telemetry.md:256-260`, anchor `public OpCost Stop`

**From:**

```csharp
public OpCost Stop(Op key, KernelDomain domain, int items, Outcome outcome) =>
    new(Key: key, Domain: domain,
        Elapsed: Stopwatch.GetElapsedTime(startingTimestamp: Timestamp),
        AllocatedBytes: long.Max(0L, GC.GetAllocatedBytesForCurrentThread() - Allocated),
        Items: items, Outcome: outcome);
```

**To:**

```csharp
public OpCost Stop(Op key, KernelDomain domain, int items, bool succeeded) =>
    new(Key: key, Domain: domain,
        Elapsed: Stopwatch.GetElapsedTime(startingTimestamp: Timestamp),
        AllocatedBytes: long.Max(0L, GC.GetAllocatedBytesForCurrentThread() - Allocated),
        Items: items, Succeeded: succeeded);
```

### 5c. Emit the Boolean dimension directly

**Location:** `libs/dotnet/Rasm/.planning/Domain/telemetry.md:495-498`, anchor `costCase: static (tenant, row)`

**From:**

```csharp
costCase: static (tenant, row) => InstrumentSet.Tags(tenant,
    (KernelInstrument.OpSlot, (object?)row.Cost.Key.ToString()),
    (KernelInstrument.DomainSlot, row.Cost.Domain.Key),
    (KernelInstrument.OutcomeSlot, row.Cost.Outcome.Key)));
```

**To:**

```csharp
costCase: static (tenant, row) => InstrumentSet.Tags(tenant,
    (KernelInstrument.OpSlot, (object?)row.Cost.Key.ToString()),
    (KernelInstrument.DomainSlot, row.Cost.Domain.Key),
    (KernelInstrument.OutcomeSlot, row.Cost.Succeeded)));
```

**Effect:** fenced LOC `20 -> 11` (`-9`); module-level types `-1` (`Outcome`); declared public members `-3` (`Succeeded`, `Failed`, `Of`); generated `Outcome` surface disappears; the cost record retains the same number of columns.

**Proof:** `Outcome` is exactly a two-case closed family with no payload or behavior beyond `Outcome.Of(bool)`. `CLAUDE.md` explicitly rejects that shape in favor of a `bool` column. The only metric consumer reads its key, and no filter, lookup, or third state exists in the corpus. Emitting the Boolean preserves the success/failure partition while making a third verdict an intentional future data-shape change instead of speculative surface.

**Required landing ripple:** at `libs/dotnet/Rasm/.planning/Analysis/query.md:509-510`, replace `outcome: exit.IsSucc ? Outcome.Succeeded : Outcome.Failed` with `succeeded: exit.IsSucc`.

**Same-file ripple:** remove `Outcome` from the index and `[03]-[COST]` owner/entry/growth claims; describe `OpCost.Succeeded` as the settled verdict; remove Thinktecture from the COST package list.

## 6. Compact `FaultObservation` without weakening its evidence boundary

This is one bounded-walk cleanup with three ordered, small edits.

### 6a. Remove the unused cause-code forwarding member

**Location:** `libs/dotnet/Rasm/.planning/Domain/telemetry.md:348-350`, anchor `public readonly record struct FaultCauseStamp`

**From:**

```csharp
public readonly record struct FaultCauseStamp(Option<FaultId> Identity, Option<Type> ExceptionType, Option<int> HResult) {
    public Option<int> Code => Identity.Map(static id => id.Code);
}
```

**To:**

```csharp
public readonly record struct FaultCauseStamp(Option<FaultId> Identity, Option<Type> ExceptionType, Option<int> HResult);
```

### 6b. Keep the fixed ceiling local to its only reader

**Location:** `libs/dotnet/Rasm/.planning/Domain/telemetry.md:290,354,358-364`, anchors `using Rasm.Numerics;`, `CauseCeiling`, and `public static FaultObservation Of`

**From:**

```csharp
using Rasm.Numerics;

public static readonly Dimension CauseCeiling = Dimension.Create(value: 8);

public static FaultObservation Of(Error error) {
    ArgumentNullException.ThrowIfNull(error);
    Queue<Error> pending = new();
    pending.Enqueue(error);
    Seq<FaultCauseStamp> causes = Seq<FaultCauseStamp>();
    int inspected = 0;
    while (inspected < CauseCeiling.Value && pending.TryDequeue(out Error? current)) {
```

**To:**

```csharp
public static FaultObservation Of(Error error) {
    ArgumentNullException.ThrowIfNull(error);
    const int causeCeiling = 8;
    Queue<Error> pending = new();
    pending.Enqueue(error);
    Seq<FaultCauseStamp> causes = Seq<FaultCauseStamp>();
    int inspected = 0;
    while (inspected < causeCeiling && pending.TryDequeue(out Error? current)) {
```

### 6c. Use the existing `Seq.Iter` side-effect traversal for aggregate children

**Location:** `libs/dotnet/Rasm/.planning/Domain/telemetry.md:374-376`, anchor `if (current is ManyErrors many)`

**From:**

```csharp
if (current is ManyErrors many) {
    foreach (Error child in many.Errors) { pending.Enqueue(child); }
}
```

**To:**

```csharp
if (current is ManyErrors many) { many.Errors.Iter(pending.Enqueue); }
```

**Effect:** fenced LOC `15 -> 10` (`-5`); declared/public members `-2` (`FaultCauseStamp.Code`, `FaultObservation.CauseCeiling`); module dependencies `-1` (`Rasm.Numerics`).

**Proof:** the only external cause-stamp consumer, `libs/dotnet/Rasm.AppHost/.planning/Runtime/ports.md:378-380`, reads `Identity`, `ExceptionType`, and `HResult` directly; no code fence reads `FaultCauseStamp.Code`. `CauseCeiling` likewise has one read, inside `Of`, and does not represent a tunable or cross-module unit, so wrapping `8` in public `Dimension` surface widens a local algorithm bound. The existing `Queue<T>` plus `Enqueue` seed remains: replacing it with `new([error])` would manufacture a one-element collection merely to save one authored line. Checked-in `libs/dotnet/.api/api-languageext.md` exposes `Seq.Iter` for the aggregate's effect-only child sweep. The traversal order, eight-node bound, identity/exception retention, and `Truncated` test remain unchanged.

**Same-file ripple:** none beyond deleting the unused import; the prose already calls the ceiling fixed and does not promise either removed member.

## 7. Make `SignalFact` satisfy its hook contract while removing the duplicate operation key

**Location:** `libs/dotnet/Rasm/.planning/Domain/telemetry.md:418-431`, anchors `[Union]`, `public sealed record FaultCase`, and `public static SignalFact Fault`

**From:**

```csharp
public abstract partial record SignalFact {
    private SignalFact() { }

    public abstract KernelPoint At { get; }

    public sealed record FaultCase(KernelPoint Point, Op Key, Error Fault) : SignalFact { public override KernelPoint At => Point; }
    public sealed record CostCase(KernelPoint Point, OpCost Cost) : SignalFact { public override KernelPoint At => Point; }

    public static SignalFact Fault(KernelDomain domain, Op key, Error fault) =>
        new FaultCase(Point: KernelPoint.Of(domain: domain, facet: PointFacet.Fault), Key: key, Fault: fault);

    public static SignalFact Cost(OpCost cost) =>
        new CostCase(Point: KernelPoint.Of(domain: cost.Domain, facet: PointFacet.Cost), Cost: cost);
}
```

**To:**

```csharp
public abstract partial record SignalFact : IHookFact<KernelPoint> {
    private SignalFact() { }

    public abstract KernelPoint At { get; }
    public bool Seats(KernelPoint at) => At == at;

    public sealed record FaultCase(KernelPoint Point, Error Fault) : SignalFact { public override KernelPoint At => Point; }
    public sealed record CostCase(KernelPoint Point, OpCost Cost) : SignalFact { public override KernelPoint At => Point; }

    public static SignalFact Fault(KernelDomain domain, Error fault) => new FaultCase(KernelPoint.Of(domain, PointFacet.Fault), fault);
    public static SignalFact Cost(OpCost cost) => new CostCase(KernelPoint.Of(cost.Domain, PointFacet.Cost), cost);
}
```

**Effect:** fenced LOC `10 -> 9` (`-1`); declared/public members net `0` (`FaultCase.Key` removed, required `Seats` implemented); public factory parameters `-1`; `SignalFact` becomes admissible to the already-declared `HookSet<KernelPoint, SignalFact, TelemetrySource>` constraint.

**Proof:** `FaultCase.Key` has no read in the planning corpus. The only producer passes the same `key` separately to `TelemetrySink.Tap(fact, key)`, where `SignalHooks.Publish` and the hook bus consume it; copying it into the fault payload creates two authorities for the emission operation. `CostCase` legitimately retains its key because `OpCost.Key` is metered as a dimension, while fault metering reads owner and recovery only. More importantly, `libs/dotnet/Rasm/.planning/Domain/hooks.md:188-190,294-297` requires `TFact : IHookFact<TPoint>` and defines that contract as `bool Seats(TPoint at)`. The same page's union law derives seating from the fact's authoritative `At`; `At == at` is therefore the missing implementation, not a new identity rail.

**Required landing ripple:** at `libs/dotnet/Rasm/.planning/Analysis/query.md:513`, replace `SignalFact.Fault(domain: KernelDomain.Analysis, key: key, fault: error)` with `SignalFact.Fault(domain: KernelDomain.Analysis, fault: error)`.

**Same-file ripple:** none; the case prose promises the lowered `Error`, not an operation-key copy.

## 8. Meter the published fact and absorb both one-use projection helpers into `Tap`

This is one semantic repair and surface collapse. Apply the three subchanges in order; each touches at most ten lines of the fence.

### 8a. Bind publication before deriving measurements

**Location:** `libs/dotnet/Rasm/.planning/Domain/telemetry.md:468-472,480-481`, anchor `public Fin<SignalFact> Tap`

**From:**

```csharp
public Fin<SignalFact> Tap(SignalFact fact, Op key) {
    Fin<SignalFact> published = Hooks.Publish(fact: fact, key: key);
    Seq<(InstrumentSpec Row, double Value)> charged = Charged(fact: fact);
    if (published.IsFail || !set.Enabled(charged.Map(static row => row.Row))) { return published; }
    TagList tags = Stamped(fact: fact);
```

```csharp
        .Match(Some: Fin.Fail<SignalFact>, None: () => published);
}
```

**To:**

```csharp
public Fin<SignalFact> Tap(SignalFact fact, Op key) =>
    Signals.Publish(fact: fact, key: key).Bind(published => {
        Seq<(InstrumentSpec Row, double Value)> charged = Charged(fact: published);
        if (!set.Enabled(charged.Map(static row => row.Row))) { return Fin.Succ(published); }
        TagList tags = Stamped(fact: published);
```

```csharp
            .Match(Some: Fin.Fail<SignalFact>, None: () => Fin.Succ(published));
    });
```

### 8b. Inline `Charged` at its only call

**Location:** `libs/dotnet/Rasm/.planning/Domain/telemetry.md:470,483-488`, anchors `charged = Charged` and `private static ... Charged`

**From:**

```csharp
Seq<(InstrumentSpec Row, double Value)> charged = Charged(fact: published);

private static Seq<(InstrumentSpec Row, double Value)> Charged(SignalFact fact) => fact.Switch(
    faultCase: static _ => Seq((KernelInstrument.Faults.Row, 1d)),
    costCase: static row => Seq(
        (KernelInstrument.Duration.Row, row.Cost.Elapsed.TotalSeconds),
        (KernelInstrument.Allocated.Row, (double)row.Cost.AllocatedBytes),
        (KernelInstrument.Counted.Row, (double)row.Cost.Items)));
```

**To:**

```csharp
Seq<(InstrumentSpec Row, double Value)> charged = published.Switch(
    faultCase: static _ => Seq((KernelInstrument.Faults.Row, 1d)),
    costCase: static row => Seq(
        (KernelInstrument.Duration.Row, row.Cost.Elapsed.TotalSeconds),
        (KernelInstrument.Allocated.Row, (double)row.Cost.AllocatedBytes),
        (KernelInstrument.Counted.Row, (double)row.Cost.Items)));
```

### 8c. Inline `Stamped` at its only call

**Location:** `libs/dotnet/Rasm/.planning/Domain/telemetry.md:472,490-498`, anchors `TagList tags = Stamped` and `private static TagList Stamped`

**From:**

```csharp
TagList tags = Stamped(fact: published);

private static TagList Stamped(SignalFact fact) => fact.Switch(
    state: TenantContext.Current,
    faultCase: static (tenant, row) => InstrumentSet.Tags(tenant,
        (KernelInstrument.OwnerSlot, row.Fault.Owner.Map(static owner => owner.Key).Match<object?>(Some: static owner => owner, None: static () => null)),
        (KernelInstrument.PostureSlot, Redrive.Posture(row.Fault).Key)),
    costCase: static (tenant, row) => InstrumentSet.Tags(tenant,
        (KernelInstrument.OpSlot, (object?)row.Cost.Key.ToString()),
        (KernelInstrument.DomainSlot, row.Cost.Domain.Key),
        (KernelInstrument.OutcomeSlot, row.Cost.Succeeded)));
```

**To:**

```csharp
TagList tags = published.Switch(
    state: TenantContext.Current,
    faultCase: static (tenant, row) => InstrumentSet.Tags(tenant,
        (KernelInstrument.OwnerSlot, row.Fault.Owner.Map(static owner => owner.Key).Match<object?>(Some: static owner => owner, None: static () => null)),
        (KernelInstrument.PostureSlot, Redrive.Posture(row.Fault).Key)),
    costCase: static (tenant, row) => InstrumentSet.Tags(tenant,
        (KernelInstrument.OpSlot, (object?)row.Cost.Key.ToString()),
        (KernelInstrument.DomainSlot, row.Cost.Domain.Key),
        (KernelInstrument.OutcomeSlot, row.Cost.Succeeded)));
```

**Effect:** fenced LOC `24 -> 22` (`-2`); declared private members `-2` (`Charged`, `Stamped`); publication and measurement remain on one `Fin<SignalFact>` rail; the call resolves through the sink's actual `Signals` owner rather than an undeclared `Hooks` member.

**Proof:** `TelemetrySink` exposes the mounted signal capsule as `Signals`; no `Hooks` member exists on the sink, so the current unqualified call has no owner. The current code also derives both `charged` and `tags` from the original `fact`; that contradicts the page's line-273 law and meters a pre-transformation fact after a subscriber returns a revised one. `Fin.Bind` runs the meter body only for a successful publication and gives that body the admitted `published` value. Each generated `Switch` is used once, immediately beside the write it governs, so the private helpers are single-call extractions rather than reusable owners. Keep the existing refusal fold: LanguageExt converts `Error.Many` input into a `ManyErrors` carrier, so replacing the fold would change the singleton case from the original error to an aggregate wrapper even though it saves no LOC.

**Same-file ripple:** at line 269, replace the named-helper description with “two inline generated `Switch` projections inside `Tap`”; at line 280, say a new instrument adds one arm/row to the inline charged projection. Keep lines 273-274: the resulting code finally implements both laws.

## 9. Remove two one-use contributor projections

This final collapse keeps each value at its actual consumer and removes two public alternative paths.

### 9a. Project sensitivity rows directly

**Location:** `libs/dotnet/Rasm/.planning/Domain/telemetry.md:531-532`, anchors `public ClassifiedValue Value` and `public static Seq<ClassifiedValue> Values`

**From:**

```csharp
public ClassifiedValue Value => new(Taxonomy: Taxonomy, Value: Key);
public static Seq<ClassifiedValue> Values => toSeq(Items).Map(static row => row.Value);
```

**To:**

```csharp
public static Seq<ClassifiedValue> Values => toSeq(Items).Map(static row => new ClassifiedValue(Taxonomy, row.Key));
```

### 9b. Build the declaration roster inside its only consumer

**Location:** `libs/dotnet/Rasm/.planning/Domain/telemetry.md:549-557`, anchors `public Fin<HashMap<string, InstrumentSpec>> Roster` and `public Fin<Unit> Admit`

**From:**

```csharp
public Fin<HashMap<string, InstrumentSpec>> Roster =>
    Declared.Collisions(static row => row.Name) is { IsEmpty: false } collided
        ? Fin.Fail<HashMap<string, InstrumentSpec>>(new KernelFault.InvalidValue(
            Label: string.Join(", ", collided),
            Requirement: "one declaration per name across the mounted and published columns"))
        : Fin.Succ(Declared.ToHashMap(static row => row.Name, static row => row));

public Fin<Unit> Admit() =>
    Roster.Bind(roster => Board.TraverseM(pack => pack.Admit(roster)).As()).Map(static _ => unit);
```

**To:**

```csharp
public Fin<Unit> Admit() =>
    (Declared.Collisions(static row => row.Name) is { IsEmpty: false } collided
        ? Fin.Fail<HashMap<string, InstrumentSpec>>(new KernelFault.InvalidValue(
            Label: string.Join(", ", collided),
            Requirement: "one declaration per name across the mounted and published columns"))
        : Fin.Succ(Declared.ToHashMap(static row => row.Name, static row => row)))
    .Bind(roster => Board.TraverseM(pack => pack.Admit(roster)).As())
    .Map(static _ => unit);
```

**Effect:** fenced LOC `10 -> 9` (`-1`); declared/public members `-2` (`Sensitivity.Value`, `TelemetryContributorPort.Roster`).

**Proof:** `Sensitivity.Value` is read only by `Sensitivity.Values`, so the instance property is an extra public hop around the generated Thinktecture `Key`. `TelemetryContributorPort.Roster` is read only by `Admit`; no caller needs the intermediate `Fin<HashMap<...>>`, while `Admit` is already the declared composition gate. Inlining preserves the named collision refusal and the exact roster passed to `BoardPack.Admit` without creating a local helper or duplicating the expression.

**Same-file ripple:** at line 506, remove `Roster` as an entry and state that `Admit` builds the local declaration map and admits the optional board pack; at line 508, name `Admit` as the operation that reports collided declaration names.

## Deliberate non-moves

- Do not delete or inline `SignalHooks`. `Rasm/RULINGS.md` settles it as the emission-only capsule, and its exposed `Hooks` member is used to park telemetry refusals through the shared `FaultCell`. Removing it would cross a settled ownership boundary rather than refine local logic.
- Do not delete `TelemetryContributorPort.Kernel` solely because no current code fence calls it. The C# deep-surface law says zero consumers does not lower the API bar, and this factory binds the kernel scope and its complete generated instrument roster as one contributor declaration; deleting it would remove an intended composition capability, not collapse a duplicate implementation.
- Do not collapse `PointFacet` to a Boolean or attempt to derive it from the `SignalFact` case. Unlike `Outcome`, it is the stable `KernelDomain × PointFacet` roster coordinate used by `HookId`. Checked-in Thinktecture API evidence says regular-union metadata supplies no stable case name or ordinal, so deleting the facet owner would require a new manual case-to-token projection or a Boolean identity knob; neither proves a smaller whole-module shape.
- Do not collapse `TraceBaggage` to its value string or entries alone. `Domain/frame.md`, `Domain/event.md`, and `Persistence/egress.md` consume `Value`, while `Rasm.AppUi/.planning/Diagnostics/telemetry.md` consumes `Entries`; the record is the single admitted parse result that keeps both representations coherent.
- Do not replace `TraceFields` with a dictionary or inline its callbacks. `DistributedContextPropagator` requires carrier getter/setter delegates; the named three-field carrier admits only the W3C fields and avoids an arbitrary header surface. A dictionary would add allocation and broader state without reducing the callback symbols.
- Do not remove or reshape `SpanEdge`. The diagnostics substrate and `Rasm/RULINGS.md` settle one carriage for kind, parent, and links; its projections feed the BCL `ActivitySource.StartActivity` shape directly.
- Do not turn `FaultCauseStamp` into a union. Identity and exception evidence can coexist in the bounded walk, and a case family would add nested types and dispatch without reducing any consumer logic.
