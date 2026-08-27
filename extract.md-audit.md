# 1. Fold field probes into the extraction request

## From

`libs/dotnet/Rasm/.planning/Processing/extract.md:371`

```csharp
[Union]
public abstract partial record ExtractionProbe {
    public sealed record VectorCase(VectorField Source) : ExtractionProbe;
    public sealed record ScalarCase(ScalarField Source) : ExtractionProbe;
    public sealed record TensorCase(TensorField Source) : ExtractionProbe;
    private ExtractionProbe() { }
    public static ExtractionProbe Vector(VectorField source) => new VectorCase(Source: source);
    public static ExtractionProbe Scalar(ScalarField source) => new ScalarCase(Source: source);
    public static ExtractionProbe Tensor(TensorField source) => new TensorCase(Source: source);
    internal Fin<ExtractionProbe> Admit(Op key) => Switch(
        state: key,
        vectorCase: static (op, probe) => op.Need(probe.Source).Map(_ => (ExtractionProbe)probe),
        scalarCase: static (op, probe) => op.Need(probe.Source).Map(_ => (ExtractionProbe)probe),
        tensorCase: static (op, probe) => op.Need(probe.Source).Map(_ => (ExtractionProbe)probe));
```

`libs/dotnet/Rasm/.planning/Processing/extract.md:448`

```csharp
public abstract partial record Extraction {
    public sealed record ProbeCase(ExtractionProbe Source, Point3d Sample) : Extraction;
```

`libs/dotnet/Rasm/.planning/Processing/extract.md:454`

```csharp
public static Fin<Extraction> Probe(ExtractionProbe source, Point3d sample, Op? key = null) {
    Op op = key.OrDefault();
    return from validSource in Optional(source).ToFin(op.InvalidInput()).Bind(active => active.Admit(key: op))
           from validSample in op.AcceptValue(value: sample)
           select (Extraction)new ProbeCase(Source: validSource, Sample: validSample);
}
```

## To

```csharp
// ExtractionProbe DELETED
```

```csharp
public abstract partial record Extraction {
    public sealed record VectorProbeCase(VectorField Source, Point3d Sample) : Extraction;
    public sealed record ScalarProbeCase(ScalarField Source, Point3d Sample) : Extraction;
    public sealed record TensorProbeCase(TensorField Source, Point3d Sample) : Extraction;
```

```csharp
public static Fin<Extraction> Probe(VectorField source, Point3d sample, Op? key = null) =>
    Probe(source, sample, static (field, point) => (Extraction)new VectorProbeCase(field, point), key);
public static Fin<Extraction> Probe(ScalarField source, Point3d sample, Op? key = null) =>
    Probe(source, sample, static (field, point) => (Extraction)new ScalarProbeCase(field, point), key);
public static Fin<Extraction> Probe(TensorField source, Point3d sample, Op? key = null) =>
    Probe(source, sample, static (field, point) => (Extraction)new TensorProbeCase(field, point), key);

private static Fin<Extraction> Probe<TField>(TField source, Point3d sample, Func<TField, Point3d, Extraction> create, Op? key) where TField : class {
    Op op = key.OrDefault();
    return from field in op.Need(source)
           from point in op.AcceptValue(sample)
           select create(field, point);
}
```

## Why

`ExtractionProbe` is a wrapper union consumed only by `Extraction.ProbeCase`; it duplicates the request discriminant, three factories, admission, and exhaustive dispatch before the same projection rows run.

## Change

Delete `ExtractionProbe`; put its three field cases directly on `Extraction`; move the existing projection rows into the three generated `Extraction.Switch` arms. Move `SpanAt` into `Extraction.Project` as a local function, and replace the glyph arm's temporary probe with direct `SampleVector(...).Bind(VectorSpan.Of(...))` composition. Keep one private generic admission body shared by the three public overloads.

## Delta

Code-fence LOC: `-12`. Declared types: `-2` net. Module-level methods: `-3` net. Net module-level declared symbols: `-5`.

# 2. Fold sampled modes into the extraction request

## From

`libs/dotnet/Rasm/.planning/Processing/extract.md:417`

```csharp
[Union]
public abstract partial record SampledExtraction {
    public sealed record GlyphCase(VectorField Field, PositiveMagnitude Scale) : SampledExtraction;
    public sealed record GridCase(ScalarField Field) : SampledExtraction;
    public sealed record StreamBundleCase(VectorField Field, PositiveMagnitude InitialStep, RungeKuttaIntegrator Integrator, Termination Termination) : SampledExtraction;
    public sealed record DrapeCase(Vector3d Direction) : SampledExtraction;
    private SampledExtraction() { }
```

`libs/dotnet/Rasm/.planning/Processing/extract.md:452`

```csharp
public sealed record SampledCase(SampledExtraction Mode, ExtractionDomain Domain, SampleKind Seeds) : Extraction;
```

`libs/dotnet/Rasm/.planning/Processing/extract.md:474`

```csharp
public static Fin<Extraction> Sampled(SampledExtraction mode, ExtractionDomain domain, SampleKind seeds, Op? key = null) {
    Op op = key.OrDefault();
    return from validMode in Admit.NotNull(value: mode, key: op)
           from validDomain in Optional(domain).ToFin(op.InvalidInput()).Bind(active => active.Admit(key: op))
           from validSeeds in SampleKind.Admit(value: seeds, key: op)
           select (Extraction)new SampledCase(Mode: validMode, Domain: validDomain, Seeds: validSeeds);
}
```

`libs/dotnet/Rasm/.planning/Processing/extract.md:509`

```csharp
sampledCase: static (state, extraction) => extraction.Mode.Switch(
    state: (Domain: extraction.Domain, Seeds: extraction.Seeds, Context: state.Context, Key: state.Key),
```

## To

```csharp
// SampledExtraction DELETED
// Extraction.SampledCase DELETED
```

```csharp
public sealed record GlyphCase(VectorField Field, PositiveMagnitude Scale, ExtractionDomain Domain, SampleKind Seeds) : Extraction;
public sealed record GridCase(ScalarField Field, ExtractionDomain Domain, SampleKind Seeds) : Extraction;
public sealed record StreamBundleCase(VectorField Field, PositiveMagnitude InitialStep, RungeKuttaIntegrator Integrator, Termination Termination, ExtractionDomain Domain, SampleKind Seeds) : Extraction;
public sealed record DrapeCase(Vector3d Direction, ExtractionDomain Domain, SampleKind Seeds) : Extraction;
```

```csharp
glyphCase: static (state, extraction) => ProjectSamples<TOut, Line>(
    seeds: extraction.Seeds, domain: extraction.Domain, context: state.Context, key: state.Key,
```

## Why

`SampledExtraction` is a second request union nested inside the only request union that consumes it. It adds a root type, intermediate case, forwarding factory, and second generated switch without independent capability.

## Change

Delete `SampledExtraction` and `Extraction.SampledCase`; add the four sampled cases directly to `Extraction`; move the four existing mode factories to `Extraction`, admitting payload, domain, and seeds once there; move the existing projection arms into the outer exhaustive switch. Preserve `ProjectSamples` as the shared three-mode spine.

## Delta

Code-fence LOC: `-9`. Declared types: `-2` net. Module-level methods: `-1`. Net module-level declared symbols: `-3`.

# 3. Stop rebuilding admitted extraction domains

## From

`libs/dotnet/Rasm/.planning/Processing/extract.md:138`

```csharp
public static Fin<ExtractionDomain> Cloud(VectorCloud value, Op? key = null) {
    Op op = key.OrDefault();
    return op.Need(value)
        .Bind(cloud => cloud.Admit(key: op))
        .Map(static valid => (ExtractionDomain)new CloudCase(value: valid));
}
```

`libs/dotnet/Rasm/.planning/Processing/extract.md:149`

```csharp
ExtractionDomain domain => domain.Admit(key: op),
```

`libs/dotnet/Rasm/.planning/Processing/extract.md:157`

```csharp
internal Fin<ExtractionDomain> Admit(Op key) => Switch(
    state: key,
    supportCase: static (op, domain) => Support(value: domain.Value, key: op),
    meshCase: static (op, domain) => Mesh(value: domain.Value, key: op),
    cloudCase: static (op, domain) => Cloud(value: domain.Value, key: op),
    latticeCase: static (op, domain) => Lattice(value: domain.Value, key: op));
```

`libs/dotnet/Rasm/.planning/Processing/extract.md:462`

```csharp
from validDomain in Optional(domain).ToFin(op.InvalidInput()).Bind(active => active.Admit(key: op))
```

## To

```csharp
public static Fin<ExtractionDomain> Cloud(VectorCloud value, Op? key = null) =>
    Admit.NotNull(value, key.OrDefault()).Map(static cloud => (ExtractionDomain)new CloudCase(cloud));
```

```csharp
ExtractionDomain domain => Fin.Succ(domain),
```

```csharp
// ExtractionDomain.Admit DELETED
```

```csharp
from validDomain in Admit.NotNull(value: domain, key: op)
```

## Why

Every `ExtractionDomain` case is minted by its owner factory. Re-dispatching an existing domain repeats native, lattice, and support gates; the cloud arm also rebuilds its point-cloud index and ownership state. Admission once means an existing wrapper is null-gated, not reconstructed.

## Change

Delete `ExtractionDomain.Admit`; let `Of` return an existing domain unchanged; null-gate domains accepted by contour and sampled factories. Change `Cloud` to null-gate an already-admitted `VectorCloud`. Preserve raw-object admission in `Of` and the default-value/native-handle gates in `Support`, `Mesh`, and `Lattice`.

## Delta

Code-fence LOC: `-8`. Module-level methods: `-1`. Repeated admission dispatches: `-4`. Net module-level declared symbols: `-1`.

## Ripples

`libs/dotnet/Rasm/.planning/Processing/intent.md:226`: replace `Admit.NotNull(...).Bind(active => active.Admit(...))` with `Admit.NotNull(...)`.

# 4. Replace route and tally indirection with direct evidence

## From

`libs/dotnet/Rasm/.planning/Processing/extract.md:53`

```csharp
[SmartEnum<int>]
public sealed partial class ExtractionRoute {
    public static readonly ExtractionRoute Native = new(key: 0, failures: static (attempted, emitted) => Some(attempted - emitted));
    public static readonly ExtractionRoute Local = new(key: 1, failures: static (_, _) => Option<int>.None);

    [UseDelegateFromConstructor]
    public partial Option<int> Failures(int attempted, int emitted);
}
```

`libs/dotnet/Rasm/.planning/Processing/extract.md:81`

```csharp
public Option<Tolerance> Band => Switch(
    fromContextCase: static c => Some(c.Value),
    rhinoDefaultCase: static _ => Option<Tolerance>.None,
    notApplicableCase: static _ => Option<Tolerance>.None);
public Option<double> Value => Switch(
    fromContextCase: static c => Some(c.Value.Value),
    rhinoDefaultCase: static r => r.Witnessed,
    notApplicableCase: static _ => Option<double>.None);
```

`libs/dotnet/Rasm/.planning/Processing/extract.md:248`

```csharp
return ExtractionTally.Of(
        route: route, attempted: attempted, emitted: accepted.Count, tolerance: tolerance, key: key,
        scalarIsoline: scalarIsoline.Map(static result => result.Census),
        itemFailures: route.Failures(attempted: attempted, emitted: accepted.Count))
    .Map(tally => new CurveBatch(Curves: accepted, ScalarIsoline: scalarIsoline, Tally: tally));
```

`libs/dotnet/Rasm/.planning/Processing/extract.md:615`

```csharp
public readonly record struct ExtractionTally(
    ExtractionRoute Route, int Attempted, int Emitted, ExtractionTolerance Tolerance,
    Option<IsoSurfaceRun> IsoSurface = default, Option<IsolineCensus> ScalarIsoline = default,
    Option<SampleTally> Sample = default, Option<int> ItemFailures = default) : IValidityEvidence {
    public int Rejected => Attempted - Emitted;
    public bool Complete => Emitted == Attempted;
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(Attempted, 0), ValidityClaim.CountAtLeast(Emitted, 0),
        Emitted <= Attempted,
        ItemFailures.Map(static count => count >= 0).IfNone(noneValue: true),
        IsoSurface.Map(static child => child.IsValid).IfNone(noneValue: true),
        ScalarIsoline.Map(static child => child.IsValid).IfNone(noneValue: true),
        Sample.Map(static child => child.IsValid).IfNone(noneValue: true));
    internal static Fin<ExtractionTally> Of(ExtractionRoute route, int attempted, int emitted, ExtractionTolerance tolerance, Op key, Option<IsoSurfaceRun> isoSurface = default, Option<IsolineCensus> scalarIsoline = default, Option<SampleTally> sample = default, Option<int> itemFailures = default) =>
        attempted < 0 || emitted < 0 || emitted > attempted
            ? Fin.Fail<ExtractionTally>(error: key.InvalidResult())
            : Fin.Succ(new ExtractionTally(Route: route, Attempted: attempted, Emitted: emitted, Tolerance: tolerance, IsoSurface: isoSurface, ScalarIsoline: scalarIsoline, Sample: sample, ItemFailures: itemFailures));
}
```

## To

```csharp
// ExtractionRoute DELETED
```

```csharp
// ExtractionTolerance.Band DELETED
// ExtractionTolerance.Value DELETED
```

```csharp
return Fin.Succ(new CurveBatch(
    accepted, scalarIsoline,
    new ExtractionTally(Native: native, Attempted: attempted, Emitted: accepted.Count, Tolerance: tolerance,
        ScalarIsoline: scalarIsoline.Map(static result => result.Census))));
```

```csharp
public readonly record struct ExtractionTally(
    bool Native, int Attempted, int Emitted, ExtractionTolerance Tolerance,
    Option<IsoSurfaceRun> IsoSurface = default, Option<IsolineCensus> ScalarIsoline = default,
    Option<SampleTally> Sample = default) : IValidityEvidence {
    public int Rejected => Attempted - Emitted;
    public bool Complete => Emitted == Attempted;
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(Attempted, 0), ValidityClaim.CountAtLeast(Emitted, 0), Emitted <= Attempted,
        IsoSurface.Map(static child => child.IsValid).IfNone(true),
        ScalarIsoline.Map(static child => child.IsValid).IfNone(true),
        Sample.Map(static child => child.IsValid).IfNone(true));

    // ExtractionTally.Of DELETED
}
```

## Why

Route provenance is a payloadless two-state fact, so a keyed smart enum plus delegate is more surface than the `Native` predicate it represents. `ItemFailures` duplicates `Rejected`; the drape arm alone diverges because it counts projected points instead of covered source items. `ExtractionTally.Of` validates counts already derived from owned collection cardinalities while the public constructor remains unrestricted. `ExtractionTolerance.Band` and `.Value` are unused forwarding projections over generated dispatch.

## Change

Replace `ExtractionRoute` with `bool Native` on `ExtractionTally` and `AcceptCurves`; pass `true` only for Rhino-host routes. Delete `ItemFailures`; set drape `Emitted` to its distinct covered-source count. Delete `ExtractionTally.Of` and construct tallies directly at its four derived-count sites, using `let` where the query no longer binds a fallible step. Preserve typed child evidence and `ExtractionTolerance`; project that union through generated `Switch` where needed.

## Delta

Code-fence LOC: `-28`. Module-level types: `-1`. Declared members: `-7`. Generated keyed lookup/value surface: removed. Net module-level declared symbols: `-8`.

# 5. Replace private point and segment wrappers with named tuples

## From

`libs/dotnet/Rasm/.planning/Processing/extract.md:47`

```csharp
using SegmentKeySet = System.Collections.Generic.HashSet<(ScalarIsolinePointKey A, ScalarIsolinePointKey B)>;
```

`libs/dotnet/Rasm/.planning/Processing/extract.md:571`

```csharp
[StructLayout(LayoutKind.Auto)]
internal readonly record struct ScalarIsolinePointKey(long X, long Y, long Z) {
    internal int Compare(ScalarIsolinePointKey other) => (X, Y, Z).CompareTo((other.X, other.Y, other.Z));
}
```

`libs/dotnet/Rasm/.planning/Processing/extract.md:576`

```csharp
[StructLayout(LayoutKind.Auto)]
internal readonly record struct ScalarIsolineSegment(Point3d A, Point3d B);
```

## To

```csharp
// SegmentKeySet DELETED
```

```csharp
// ScalarIsolinePointKey DELETED
// ScalarIsolineSegment DELETED
```

```csharp
private static (long X, long Y, long Z) KeyOf(Point3d point, Tolerance weld) {
```

## Why

Both records are private kernel plumbing with tuple semantics. Neither enforces admission, owns behavior, crosses a boundary, nor has a consumer outside this file; the point key's only method forwards to tuple comparison. `CurveBatch` remains because its named three-field return shape is reused across every contour adapter and reduces signature duplication.

## Change

Use named `(long X, long Y, long Z)` tuples for weld keys and named `(Point3d A, Point3d B)` tuples for segments. Update the private lists, arrays, dictionary, and set in place; compare keys with tuple `CompareTo`; declare the dedup set locally.

## Delta

Code-fence LOC: `-7`. Module-level types: `-2`. Type aliases: `-1`. Declared methods: `-1`. Net module-level declared symbols: `-3`.

# 6. Localize the scalar-isoline kernel and delete endpoint dispatch

## From

`libs/dotnet/Rasm/.planning/Processing/extract.md:62`

```csharp
[SmartEnum<int>]
public sealed partial class ChainEnd {
    public static readonly ChainEnd Head = new(key: 0, anchor: static points => points[index: 0], slot: static _ => 0);
    public static readonly ChainEnd Tail = new(key: 1, anchor: static points => points[^1], slot: static points => points.Count);

    [UseDelegateFromConstructor] public partial Point3d Anchor(List<Point3d> points);
    [UseDelegateFromConstructor] public partial int Slot(List<Point3d> points);
}
```

`libs/dotnet/Rasm/.planning/Processing/extract.md:277`

```csharp
private static Writer<IsolineCensus, Unit> AddFaceIsolines(Mesh mesh, MeshFace face, Arr<double> values, Seq<double> levels, Tolerance band, Tolerance weld, List<ScalarIsolineSegment> segments) {
```

`libs/dotnet/Rasm/.planning/Processing/extract.md:309`

```csharp
private static Writer<IsolineCensus, Seq<ScalarIsolineSegment>> DeduplicateSegments(List<ScalarIsolineSegment> segments, Tolerance weld) {
```

`libs/dotnet/Rasm/.planning/Processing/extract.md:322`

```csharp
private static Writer<IsolineCensus, Seq<Curve>> StitchSegments(Seq<ScalarIsolineSegment> segments, Tolerance weld) {
```

`libs/dotnet/Rasm/.planning/Processing/extract.md:342`

```csharp
ledger = toSeq(ChainEnd.Items).Fold(ledger, (held, end) => held.Combine(Extend(points: points, end: end, all: all, used: used, incident: incident, weld: weld)));
```

`libs/dotnet/Rasm/.planning/Processing/extract.md:349`

```csharp
private static IsolineCensus Extend(List<Point3d> points, ChainEnd end, ScalarIsolineSegment[] all, bool[] used, Dictionary<ScalarIsolinePointKey, List<int>> incident, Tolerance weld) {
```

`libs/dotnet/Rasm/.planning/Processing/extract.md:362`

```csharp
private static ScalarIsolinePointKey KeyOf(Point3d point, Tolerance weld) {
```

## To

```csharp
// ChainEnd DELETED
```

```csharp
Writer<IsolineCensus, Unit> AddFaceIsolines(Mesh mesh, MeshFace face, Arr<double> values, Seq<double> levels, Tolerance band, Tolerance weld, List<(Point3d A, Point3d B)> segments) {
```

```csharp
Writer<IsolineCensus, Seq<(Point3d A, Point3d B)>> DeduplicateSegments(List<(Point3d A, Point3d B)> segments, Tolerance weld) {
```

```csharp
ledger = ledger.Combine(Extend(prepend: true)).Combine(Extend(prepend: false));

IsolineCensus Extend(bool prepend) {
    IsolineCensus tally = IsolineCensus.Empty;
    while (true) {
        (long X, long Y, long Z) at = KeyOf(prepend ? points[0] : points[^1], weld);
        if (!incident.TryGetValue(at, out List<int>? candidates)) return tally;
        Seq<int> open = toSeq(candidates).Filter(candidate => !used[candidate]);
        if (open.Count > 1) return tally.Combine(IsolineCensus.BranchStop);
        if (open.Case is not int index) return tally;
        (Point3d A, Point3d B) segment = all[index];
        points.Insert(prepend ? 0 : points.Count, KeyOf(segment.A, weld).Equals(at) ? segment.B : segment.A);
        used[index] = true;
    }
}
```

```csharp
(long X, long Y, long Z) KeyOf(Point3d point, Tolerance weld) {
```

## Why

The face, dedup, stitch, extension, and key functions form one private statement kernel with one caller. Keeping five class-level methods exposes operation-local machinery as module structure. `ChainEnd` is a generated two-item vocabulary used once to choose index zero versus the current count.

## Change

Move `AddFaceIsolines`, `DeduplicateSegments`, `StitchSegments`, and `KeyOf` into `ScalarIsolinesDetailed` as local functions. Move `Extend` inside `StitchSegments`; call it once for prepend and once for append. Delete `ChainEnd`. Keep `Writer<IsolineCensus, ...>` because it lawfully accumulates non-failing evidence beside values.

## Delta

Code-fence LOC: `-6`. Module-level types: `-1`. Module-level methods: `-5`. Other declared members: `-4`. Net module-level declared symbols: `-10`.

# 7. Inline one-line census constructors

## From

`libs/dotnet/Rasm/.planning/Processing/extract.md:593`

```csharp
internal static IsolineCensus Raw(int count) => Empty with { RawSegments = count };
internal static IsolineCensus Plateau => Empty with { PlateauRejected = 1 };
internal static IsolineCensus VertexTouch => Empty with { VertexTouchRejected = 1 };
internal static IsolineCensus BranchStop => Empty with { BranchStops = 1 };
internal static IsolineCensus Branched(int branchNodes, int maxIncident) =>
    Empty with { BranchNodes = branchNodes, MaxIncidentSegments = maxIncident };
internal static IsolineCensus Stitched(int attempted, int emitted) =>
    Empty with { StitchedCandidates = attempted, EmittedCurves = emitted };
```

## To

```csharp
// IsolineCensus.Raw DELETED
// IsolineCensus.Plateau DELETED
// IsolineCensus.VertexTouch DELETED
// IsolineCensus.BranchStop DELETED
// IsolineCensus.Branched DELETED
// IsolineCensus.Stitched DELETED
```

## Why

These members only rename `Empty with { ... }`; five have one call site and `Raw` has two. None encodes a shared multi-field rule. They add owner surface without admission, calculation, or reuse value.

## Change

Replace every call with the corresponding `IsolineCensus.Empty with { ... }` expression at the evidence-emission site. Preserve `Empty`, `Combine`, and `Monoid<IsolineCensus>` for `Writer`.

## Delta

Code-fence LOC: `-8`. Module-level methods/properties: `-6`. Types: unchanged. Net module-level declared symbols: `-6`.

# 8. Inline support-only contour adapters and iso projection

## From

`libs/dotnet/Rasm/.planning/Processing/extract.md:166`

```csharp
supportCase: static (state, domain) => domain.Value.Value switch {
    Brep brep => CurvesFromBrep(brep: brep, policy: state.Policy, key: state.Key),
    Mesh mesh => MeshSpace.Of(native: mesh, context: state.Context, key: state.Key)
        .Bind(space => CurvesFromMesh(space: space, policy: state.Policy, key: state.Key)),
    Surface surface => CurvesFromSurface(surface: surface, policy: state.Policy, key: state.Key),
```

`libs/dotnet/Rasm/.planning/Processing/extract.md:180`

```csharp
private static Fin<CurveBatch> CurvesFromBrep(Brep brep, ContourPolicy policy, Op key) =>
    key.Catch(() => policy.Switch(
        state: (Brep: brep, Key: key),
        planeCase: static (state, p) => AcceptNative(curves: Brep.CreateContourCurves(brepToContour: state.Brep, sectionPlane: p.Section), tolerance: ExtractionTolerance.RhinoDefault, key: state.Key),
        axisCase: static (state, p) => AcceptNative(curves: Brep.CreateContourCurves(brepToContour: state.Brep, contourStart: p.Start, contourEnd: p.End, interval: p.Interval.Value), tolerance: ExtractionTolerance.RhinoDefault, key: state.Key),
        surfaceIsoCase: static (state, _) => Fin.Fail<CurveBatch>(error: state.Key.Unsupported(inputType: typeof(Brep), outputType: typeof(ContourPolicy.SurfaceIsoCase))),
        meshScalarCase: static (state, _) => Fin.Fail<CurveBatch>(error: state.Key.Unsupported(inputType: typeof(Brep), outputType: typeof(ContourPolicy.MeshScalarCase)))));
```

`libs/dotnet/Rasm/.planning/Processing/extract.md:200`

```csharp
private static Fin<CurveBatch> CurvesFromSurface(Surface surface, ContourPolicy policy, Op key) =>
    key.Catch(() => policy.Switch(
        state: (Surface: surface, Key: key),
        surfaceIsoCase: static (state, p) =>
            from frame in IsoFrame(status: p.Status, parameter: p.Parameter, domain: state.Surface.Domain, key: state.Key)
            from curves in state.Surface is BrepFace face
                ? Optional(face.TrimAwareIsoCurve(direction: frame.Direction, constantParameter: frame.Parameter)).ToFin(state.Key.InvalidResult())
                : Optional(state.Surface.IsoCurve(direction: frame.Direction, constantParameter: frame.Parameter)).ToFin(state.Key.InvalidResult()).Map(curve => (Curve[])[curve])
            from batch in AcceptNative(curves: curves, tolerance: ExtractionTolerance.NotApplicable, key: state.Key)
            select batch,
        planeCase: static (state, _) => Fin.Fail<CurveBatch>(error: state.Key.Unsupported(inputType: typeof(Surface), outputType: typeof(ContourPolicy.PlaneCase))),
        axisCase: static (state, _) => Fin.Fail<CurveBatch>(error: state.Key.Unsupported(inputType: typeof(Surface), outputType: typeof(ContourPolicy.AxisCase))),
        meshScalarCase: static (state, _) => Fin.Fail<CurveBatch>(error: state.Key.Unsupported(inputType: typeof(Surface), outputType: typeof(ContourPolicy.MeshScalarCase)))));
```

`libs/dotnet/Rasm/.planning/Processing/extract.md:213`

```csharp
private static Fin<(int Direction, double Parameter)> IsoFrame(IsoStatus status, double parameter, Func<int, Interval> domain, Op key) =>
    status switch {
        IsoStatus.X => key.Finite(parameter).Map(_ => (Direction: 1, Parameter: parameter)),
        IsoStatus.Y => key.Finite(parameter).Map(_ => (Direction: 0, Parameter: parameter)),
        IsoStatus.West => Fin.Succ((Direction: 1, Parameter: domain(0).T0)),
        IsoStatus.East => Fin.Succ((Direction: 1, Parameter: domain(0).T1)),
        IsoStatus.South => Fin.Succ((Direction: 0, Parameter: domain(1).T0)),
        IsoStatus.North => Fin.Succ((Direction: 0, Parameter: domain(1).T1)),
        _ => Fin.Fail<(int Direction, double Parameter)>(key.Unsupported(inputType: typeof(Surface), outputType: typeof(ContourPolicy.SurfaceIsoCase))),
    };
```

## To

```csharp
Brep brep => state.Key.Catch(() => state.Policy.Switch(
    state: (Brep: brep, Key: state.Key),
    planeCase: static (s, p) => AcceptNative(Brep.CreateContourCurves(s.Brep, p.Section), ExtractionTolerance.RhinoDefault, s.Key),
    axisCase: static (s, p) => AcceptNative(Brep.CreateContourCurves(s.Brep, p.Start, p.End, p.Interval.Value), ExtractionTolerance.RhinoDefault, s.Key),
    surfaceIsoCase: static (s, _) => Fin.Fail<CurveBatch>(s.Key.Unsupported(typeof(Brep), typeof(ContourPolicy.SurfaceIsoCase))),
    meshScalarCase: static (s, _) => Fin.Fail<CurveBatch>(s.Key.Unsupported(typeof(Brep), typeof(ContourPolicy.MeshScalarCase))))),
```

```csharp
Surface surface => state.Key.Catch(() => state.Policy.Switch(
    state: (Surface: surface, Key: state.Key),
    surfaceIsoCase: static (s, p) =>
        from frame in (p.Status switch {
            IsoStatus.X => Fin.Succ((Direction: 1, Parameter: p.Parameter)),
            IsoStatus.Y => Fin.Succ((Direction: 0, Parameter: p.Parameter)),
            IsoStatus.West => Fin.Succ((Direction: 1, Parameter: s.Surface.Domain(0).T0)),
            IsoStatus.East => Fin.Succ((Direction: 1, Parameter: s.Surface.Domain(0).T1)),
            IsoStatus.South => Fin.Succ((Direction: 0, Parameter: s.Surface.Domain(1).T0)),
            IsoStatus.North => Fin.Succ((Direction: 0, Parameter: s.Surface.Domain(1).T1)),
            _ => Fin.Fail<(int Direction, double Parameter)>(s.Key.InvalidInput()),
        })
        from curves in s.Surface is BrepFace face
            ? Optional(face.TrimAwareIsoCurve(frame.Direction, frame.Parameter)).ToFin(s.Key.InvalidResult())
            : Optional(s.Surface.IsoCurve(frame.Direction, frame.Parameter)).ToFin(s.Key.InvalidResult()).Map(curve => (Curve[])[curve])
        from batch in AcceptNative(curves, ExtractionTolerance.NotApplicable, s.Key)
        select batch,
    planeCase: static (s, _) => Fin.Fail<CurveBatch>(s.Key.Unsupported(typeof(Surface), typeof(ContourPolicy.PlaneCase))),
    axisCase: static (s, _) => Fin.Fail<CurveBatch>(s.Key.Unsupported(typeof(Surface), typeof(ContourPolicy.AxisCase))),
    meshScalarCase: static (s, _) => Fin.Fail<CurveBatch>(s.Key.Unsupported(typeof(Surface), typeof(ContourPolicy.MeshScalarCase))))),
```

```csharp
// ExtractionDomain.CurvesFromBrep DELETED
// ExtractionDomain.CurvesFromSurface DELETED
// ExtractionDomain.IsoFrame DELETED
```

## Why

The Brep and Surface adapters each have one dispatch site and no independent caller-facing meaning. `IsoFrame` is another one-call projection nested behind the Surface adapter; its `X`/`Y` finiteness checks repeat `ContourPolicy.Admit`.

## Change

Move both host adapters into their `SupportCase` pattern arms and keep every Rhino call inside `Op.Catch`. Inline exhaustive `IsoStatus` projection into the surface-iso arm; remove duplicate finite gates while retaining the foreign-enum failure. Keep the two-call mesh and cloud adapters as shared methods.

## Delta

Code-fence LOC: `-5`. Module-level methods: `-3`. Repeated validation calls: `-2`. Types: unchanged. Net module-level declared symbols: `-3`.

# 9. Inline lattice contour dispatch and remove repeated scalar admission

## From

`libs/dotnet/Rasm/.planning/Processing/extract.md:178`

```csharp
latticeCase: static (state, domain) => CurvesFromLattice(grid: domain.Value, policy: state.Policy, context: state.Context, key: state.Key));
```

`libs/dotnet/Rasm/.planning/Processing/extract.md:223`

```csharp
private static Fin<CurveBatch> CurvesFromLattice(CellLattice grid, ContourPolicy policy, Context context, Op key) =>
    key.Catch(() => policy.Switch(
        state: (Grid: grid, Context: context, Key: key),
        meshScalarCase: static (state, p) => LatticeIsolines(grid: state.Grid, values: p.Values, levels: p.Levels, context: state.Context, key: state.Key),
        planeCase: static (state, _) => Fin.Fail<CurveBatch>(error: state.Key.Unsupported(inputType: typeof(CellLattice), outputType: typeof(ContourPolicy.PlaneCase))),
        axisCase: static (state, _) => Fin.Fail<CurveBatch>(error: state.Key.Unsupported(inputType: typeof(CellLattice), outputType: typeof(ContourPolicy.AxisCase))),
        surfaceIsoCase: static (state, _) => Fin.Fail<CurveBatch>(error: state.Key.Unsupported(inputType: typeof(CellLattice), outputType: typeof(ContourPolicy.SurfaceIsoCase)))));
private static Fin<CurveBatch> LatticeIsolines(CellLattice grid, Arr<double> values, Seq<double> levels, Context context, Op key) =>
    from field in ScalarField.Lattice(grid: grid, values: values, key: key)
    from results in levels.TraverseM(level => IsoContour.Detailed(field: field, grid: grid, isoValue: level, context: context, key: key)).As()
    from batch in AcceptCurves(
        curves: results.Bind(static result => result.Loops.Map(static chain => (Curve)chain.Points.ToPolylineCurve())),
        attempted: results.Sum(static result => result.Loops.Count),
        route: ExtractionRoute.Local, tolerance: ExtractionTolerance.FromContext(context.Absolute), key: key)
    select batch;
```

`libs/dotnet/Rasm/.planning/Processing/extract.md:257`

```csharp
private static Fin<ScalarIsolineResult> ScalarIsolinesDetailed(Mesh mesh, Arr<double> values, Seq<double> levels, Context context, Op key) {
    if (values.Count != mesh.Vertices.Count || values.Exists(static value => !double.IsFinite(value)) || levels.IsEmpty || levels.Exists(static value => !double.IsFinite(value)))
        return Fin.Fail<ScalarIsolineResult>(key.InvalidInput());
```

## To

```csharp
latticeCase: static (state, domain) => state.Key.Catch(() => state.Policy.Switch(
    state: (Grid: domain.Value, Context: state.Context, Key: state.Key),
    meshScalarCase: static (s, p) =>
        from field in ScalarField.Lattice(s.Grid, p.Values, key: s.Key)
        from results in p.Levels.TraverseM(level => IsoContour.Detailed(field, s.Grid, level, s.Context, s.Key)).As()
        from batch in AcceptCurves(
            results.Bind(static result => result.Loops.Map(static chain => (Curve)chain.Points.ToPolylineCurve())),
            results.Sum(static result => result.Loops.Count), native: false,
            ExtractionTolerance.FromContext(s.Context.Absolute), s.Key)
        select batch,
    planeCase: static (s, _) => Fin.Fail<CurveBatch>(s.Key.Unsupported(typeof(CellLattice), typeof(ContourPolicy.PlaneCase))),
    axisCase: static (s, _) => Fin.Fail<CurveBatch>(s.Key.Unsupported(typeof(CellLattice), typeof(ContourPolicy.AxisCase))),
    surfaceIsoCase: static (s, _) => Fin.Fail<CurveBatch>(s.Key.Unsupported(typeof(CellLattice), typeof(ContourPolicy.SurfaceIsoCase))))));

// ExtractionDomain.CurvesFromLattice DELETED
// ExtractionDomain.LatticeIsolines DELETED
```

```csharp
private static Fin<ScalarIsolineResult> ScalarIsolinesDetailed(Mesh mesh, Arr<double> values, Seq<double> levels, Context context, Op key) {
    if (values.Count != mesh.Vertices.Count) return Fin.Fail<ScalarIsolineResult>(key.InvalidInput());
```

## Why

Both lattice methods have one call site and only stage `ScalarField.Lattice`, `IsoContour.Detailed`, and curve acceptance. `ContourPolicy.MeshScalar.Admit` already proves non-empty finite values and levels, so the mesh kernel repeats three predicates before its one mesh-specific vertex-count invariant.

## Change

Move the policy switch and lattice pipeline into the generated `ExtractionDomain.LatticeCase` arm; delete both forwarding methods. Retain `TraverseM` because level evaluation is dependent fail-fast `Fin` sequencing. Retain only the mesh vertex-count invariant in `ScalarIsolinesDetailed`.

## Delta

Code-fence LOC: `-6`. Module-level methods: `-2`. Repeated validation predicates: `-3`. Types: unchanged. Net module-level declared symbols: `-2`.
