# [RASM_VECTORS_EXTRACT]

The extraction/projection rail — ONE `ExtractionDomain` `[Union]` (`Support`/`Mesh`/`Cloud`) with the polymorphic `Of(object?)` ingress that admits raw `Mesh`/`VectorCloud`/`PointCloud`/`Brep`/`Surface` into a typed sampling domain, ONE `ContourPolicy` `[Union]` (`Plane`/`Axis`/`SurfaceIso`/`MeshScalar`) that sections every domain native-first (`Brep.CreateContourCurves`, `Mesh.CreateContourCurves`, `Surface.IsoCurve(direction, constantParameter)`, `BrepFace.TrimAwareIsoCurve`, `PointCloud.CreateContourCurves`/`CreateSectionCurve`) and falls to the owned marching-triangles scalar-isoline kernel ONLY where RhinoCommon has no surface (per-vertex scalar contouring), and ONE public `Extraction` `[Union]` (`Probe`/`Contour`/`IsoSurface`/`Sampled`) that is the extraction request vocabulary the `intent.md` rail carries as a single case. The `SurfaceIso` band resolves through ONE `IsoFrame` status resolver — `IsoStatus.X`/`Y` carry the caller parameter, the four edge statuses read the opposite-direction domain end — serving both backends (`TrimAwareIsoCurve` when the face carries trims, the raw `IsoCurve` otherwise); a second status ladder is the deleted form. The former `Glyph`/`Grid`/`StreamBundle` policy triple is dead: sampled extraction is ONE `SampledExtraction` `[Union]` whose cases (`Glyph(VectorField, PositiveMagnitude Scale)` / `Grid(ScalarField)` / `StreamBundle(VectorField, PositiveMagnitude InitialStep, FieldIntegrator, Termination)`) share one `SampleKind` seed generator and one `ProjectSamples` spine — a new sampled mode is one case plus one item arm, never a third near-identical policy struct.

Every `.Project<TOut>` on this page routes through `Numerics/atoms.md`'s `AtomProjection.Rows` typed-row dispatch — the `typeof(TOut)` reflection branching that this file's ancestor carried is the named kill: the receipt rides as the IMPLICIT self row (`Rows<TSelf, TOut>(self, key, rows…)` yields self when `TOut == TSelf`), a row is `ProjectionRow.Of<T>(producer)` lazily produced, and only the matched row runs. Receipt metadata is folded: `ExtractionRoute` (`Native`/`Local`) is the ONE retained vocabulary; completion status DERIVES from the counts (`Complete => Emitted == Attempted`, never stored beside them); tolerance provenance rides the tolerance value as the ONE `ExtractionTolerance` `[Union]` (`FromContext(double)` / `RhinoDefault` optionally witnessing the native evaluator's fixed internal via `RhinoFixed(double)` / `NotApplicable`) — the former three-enum spread (`ExtractionStatus`, `ToleranceSource`) is dead. The three parallel per-mode failure slots collapse to ONE `Option<int> ItemFailures` (the mode is already on the request). This page composes `sample.md` (`SampleKind` seed evaluation), `flow.md` (`FlowKernel.Trace` stream bundles), `Spatial/fields.md` (`ScalarField`/`VectorField`/`TensorField` sampling + the exposed `SampleSdfDetailed`/`SampleDetailed` tagged rails), `Processing/geodesics.md` (`GeodesicKernel.TangentLogMapAt` for the mesh-bound log-map probe), and `Meshing/reconstruct.md` (`IsoSurface.Detailed` marching-cubes extraction) — the ONE sampling/extraction rail invariant. `Op` stays the explicit value key; every receipt's `IsValid` is one `Domain/rails.md` `ValidityClaim.All` fold.

## [01]-[INDEX]

- [02]-[SECTIONING]: `ExtractionDomain` ingress + admission; `ContourPolicy` union; the native-first contour adapters per geometry with the ONE `IsoFrame` status resolver; the marching-triangles scalar-isoline kernel (plateau rejection, quantized welding, dedup, incidence stitch with branch stops) and its `ScalarIsolineReceipt`.
- [03]-[PROJECTION_RAIL]: `ExtractionProbe` field point-probe rows; the public `Extraction` request union + factories; the `SampledExtraction` mode family and the one `ProjectSamples` spine; `ExtractionTolerance`; the folded `ExtractionReceipt`.

## [02]-[SECTIONING]

- Owner: `ExtractionDomain` `[Union]` — `SupportCase(SupportSpace)` / `MeshCase(MeshSpace)` / `CloudCase(VectorCloud)`; `Of(object? value, Context, Op?)` is the polymorphic ingress discriminating on the RUNTIME shape (an already-typed domain re-admits; a raw `Mesh` lifts through `MeshSpace.Of`; a `VectorCloud` re-validates through its own `Admit`; a raw `PointCloud` clusters through `VectorCloud.Cluster`; any other geometry falls to `SupportSpace.Of`) — one arity owns every ingress modality. `ContourPolicy` `[Union]` — `PlaneCase(Plane)` / `AxisCase(Point3d Start, Point3d End, PositiveMagnitude Interval)` / `SurfaceIsoCase(IsoStatus, double Parameter)` / `MeshScalarCase(Arr<double> Values, Seq<double> Levels)` — each factory admits through the `Domain/validation.md` vocabulary (finite endpoints, non-degenerate span, iso-status/parameter coherence, finite scalar/level sets).
- Entry: `domain.Contours(ContourPolicy, Context, Op)` → `Fin<CurveBatch>` — a total `Switch` routing by domain then by policy: Brep plane/axis contours, Mesh plane/axis contours at `Context.Absolute`, Surface/BrepFace iso curves through the ONE `IsoFrame` resolver (trim-aware on faces, raw `IsoCurve` on surfaces), Cloud plane sections and axis contours, and Mesh scalar isolines through the local kernel; every unsupported domain-policy pairing is a typed `Unsupported` fault naming both sides, never a silent empty.
- Auto: the scalar-isoline kernel triangulates a duplicate (quads converted once), then per face per level — plateau faces reject (all three scalars within the scale-derived epsilon `EpsilonPolicy.SqrtEpsilon · max(1, |level|, max|scalar|)`); an edge lying ON the level emits the edge segment; a strict sign change interpolates the crossing; the per-face crossing set welds through the quantized `ScalarIsolinePointKey` (round-to-even at `1/max(tolerance, √ε)` pitch) and emits a segment only on exactly two distinct points. Dedup canonicalizes each segment's key pair; stitching builds the point-key incidence map and extends each seed polyline at both ends, stopping at branch nodes (>1 unused incident segment — recorded, never guessed through).
- Receipt: `ScalarIsolineReceipt` — finite levels, raw/deduped segment counts, degenerate/plateau rejections, stitched candidates, branch stops/nodes, max incidence, emitted curves — the full kernel evidence on the `ValidityClaim.All` count fold; `CurveBatch` carries the accepted curves, the optional isoline result, and the composed `ExtractionReceipt`.
- Boundary: the isoline kernel is the named statement-kernel boundary adapter — RhinoCommon owns no per-vertex scalar contour, so the PL kernel follows Rhino's triangulated topology and returns stitched candidates only; its accumulation ledger is kernel-local state inside the exemption. Native adapters wrap every RhinoCommon call in `Op.Catch` so a host throw converts at the boundary; curves filter through `IsValid` before acceptance and the rejected count lands on the receipt.

## [03]-[PROJECTION_RAIL]

- Owner: `ExtractionProbe` `[Union]` — `Vector(VectorField)` / `Scalar(ScalarField)` / `Tensor(TensorField)` — the field point-probe; `Extraction` `[Union]` (PUBLIC — the extraction request vocabulary `intent.md` wraps as ONE case) — `ProbeCase(ExtractionProbe, Point3d)` / `ContourCase(ExtractionDomain, ContourPolicy)` / `IsoSurfaceCase(ScalarField, BoundingBox, Dimension Resolution, Dimension MaxRootSteps)` / `SampledCase(SampledExtraction Mode, ExtractionDomain Domain, SampleKind Seeds)`; `SampledExtraction` `[Union]` — `GlyphCase(VectorField, PositiveMagnitude Scale)` / `GridCase(ScalarField)` / `StreamBundleCase(VectorField, PositiveMagnitude InitialStep, FieldIntegrator, Termination)` — ONE sampled-mode family over one shared seed generator; `ExtractionTolerance` `[Union]` — `FromContext(double)` / `RhinoDefault(Option<double> Witnessed)` with the `RhinoFixed` witness factory / `NotApplicable` — provenance and value as one carrier.
- Entry: `Extraction.Probe/Contour/IsoSurface/Sampled(...)` factories admit once (probe source and sample gated — a null field union never reaches `Project` — domain re-admitted, policy validated through the mode's own `Admit`, iso bounds gated finite and non-degenerate); `extraction.Project<TOut>(Context, Op)` is the one egress — probe rows project field samples (`Vector3d`, magnitude `double`, and the `VectorSpan`/`Direction`/`Line` span family; the mesh-bound `TangentLogMapCase` routes to `Processing/geodesics.md`'s `GeodesicKernel.TangentLogMapAt` for `TangentLogMapResult`/`TangentLogMapReceipt`; scalar rows ride `Spatial/fields.md`'s exposed `SampleSdfDetailed` → `SdfSample` and `SampleDetailed` → `FieldSample` tagged rails; tensor rows project `SymmetricMatrix` or principal eigenpairs); contour rows project `Seq<Curve>`, `ExtractionReceipt`, `ScalarIsolineResult`/`ScalarIsolineReceipt`; iso-surface rows project `Mesh`/`IsoSurfaceResult`/`ExtractionReceipt` off `Meshing/reconstruct.md`'s `IsoSurface.Detailed` (the `IsoSurfaceReceipt` is the implicit self row); sampled rows project `Seq<Line>` glyphs, `Seq<(Point3d, double)>` grids, and `Seq<StreamlineTrace>`/`Seq<Polyline>`/`Seq<Curve>` bundles through `flow.md`'s `ProjectTrace`.
- Auto: `ProjectSamples` is the ONE sampled spine — evaluate `Seeds` through `sample.md` (`SampleKind.Evaluate` over the domain), fold each seed through the mode's item arm (glyph = probe the vector field and scale a `Line` along the span; grid = sample the scalar; bundle = `FlowKernel.Trace`), mint the `ExtractionReceipt` (seed receipt + `ItemFailures` count), and project through ONE `Rows` call per mode with the receipt as the implicit self row — item rows are gated on zero rejections, so a partial sampled extraction is a typed fault, never a truncated success.
- Receipt: `ExtractionReceipt` — `Route` (`Native`/`Local`), `Attempted`/`Emitted` with DERIVED `Rejected` and `Complete`, the `ExtractionTolerance` carrier, `ParallelCallback`, the optional child receipts (`IsoSurfaceReceipt`/`ScalarIsolineReceipt`/`SampleReceipt`), and ONE `Option<int> ItemFailures` — the three former per-mode failure slots and the stored status/tolerance-source enums are dead; `IsValid` is one `ValidityClaim.All` fold over the count rows and nested evidence.
- Packages: `Rasm`/Spatial (`ScalarField`/`VectorField`/`TensorField` sampling, `SampleSdfDetailed`/`SampleDetailed`, `SupportSpace`, `VectorCloud`), `Rasm`/Meshing (`MeshSpace`, `IsoSurface.Detailed` + `IsoSurfacePolicy`/`IsoSurfaceResult`/`IsoSurfaceReceipt`), `Rasm`/Processing (`SampleKind`/`SampleReceipt`, `FlowKernel`/`Termination`/`StreamlineTrace`, `GeodesicKernel.TangentLogMapAt`), `Rasm`/Numerics (`FieldIntegrator`, `AtomProjection`/`ProjectionRow`, `EpsilonPolicy`, `Dimension`/`PositiveMagnitude`, `VectorSpan`/`Direction`), `Rasm`/Domain (`Op`/`Context`/`Admit`/`ValidityClaim`), LanguageExt.Core, Thinktecture.Runtime.Extensions, RhinoCommon (contour/iso/section natives, `IsoStatus`, `CollectionsMarshal` welding).
- Growth: a new section policy is one `ContourPolicy` case + one adapter arm per admitting domain; a new sampled mode is one `SampledExtraction` case + one item arm on the spine; a new probe output is one `ProjectionRow`; a new ingress shape is one `Of` arm — the request union, the mode family, and the row set absorb all growth.
- Boundary: native-first is law — the local PL kernel exists ONLY for the scalar-contour hole in RhinoCommon and never shadows a native route; `Extraction` construction is the page's factories (sealed union, private root constructor) so no half-admitted request exists; sampled projection composes the `sample.md`/`flow.md`/`fields.md` owners and re-implements none of them — a domain-local sampler, tracer, or field evaluator beside the owning pages is the deleted parallel-rail form; `typeof(TOut)` comparison anywhere on this page is the named dead pattern, `AtomProjection.Rows` is the only output dispatch. The log-map pair is the probe's ONLY mesh-band special case — its result has no value-only form; the retired probe's second `typeof`-keyed special case (Hodge decomposition evidence) is ABSORBED, never carried: a Hodge field probes its sampled component vector here, and the `HodgeDecompositionReceipt` rides `fields.md`'s tagged vector rail (the declared `SampleDetailed` vector-sibling arm, whose nested-evidence slot is exactly this receipt) — a probe row that re-derived the 1-form beside `fields.md`'s edge-integration would be the parallel-rail form this rail deletes.

```csharp contract
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Foundation.CSharp.Analyzers.Contracts;
using LanguageExt;
using Rasm.Domain;
using Rasm.Meshing;
using Rasm.Numerics;
using Rasm.Spatial;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;
// CS0104 guard: LanguageExt.HashSet collides with the BCL name under the dual usings.
using SegmentKeySet = System.Collections.Generic.HashSet<(ScalarIsolinePointKey A, ScalarIsolinePointKey B)>;
using Dimension = Rasm.Numerics.Dimension;

namespace Rasm.Processing;

// --- [TYPES] ----------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class ExtractionRoute {
    public static readonly ExtractionRoute Native = new(key: 0);
    public static readonly ExtractionRoute Local = new(key: 1);
}

// Tolerance provenance and value as ONE carrier; the stored ToleranceSource/Option<double> pair is dead.
// RhinoDefault optionally WITNESSES the native evaluator's fixed internal value — recorded, never chosen.
[Union]
public abstract partial record ExtractionTolerance {
    public sealed record FromContextCase(double Value) : ExtractionTolerance;
    public sealed record RhinoDefaultCase(Option<double> Witnessed) : ExtractionTolerance;
    public sealed record NotApplicableCase : ExtractionTolerance;
    private ExtractionTolerance() { }
    public static ExtractionTolerance FromContext(double value) => new FromContextCase(Value: value);
    public static ExtractionTolerance RhinoFixed(double witnessed) => new RhinoDefaultCase(Witnessed: Some(witnessed));
    public static readonly ExtractionTolerance RhinoDefault = new RhinoDefaultCase(Witnessed: Option<double>.None);
    public static readonly ExtractionTolerance NotApplicable = new NotApplicableCase();
    public Option<double> Value => this switch {
        FromContextCase c => Some(c.Value),
        RhinoDefaultCase r => r.Witnessed,
        _ => Option<double>.None,
    };
}

[Union]
public abstract partial record ContourPolicy {
    public sealed record PlaneCase(Plane Section) : ContourPolicy;
    public sealed record AxisCase(Point3d Start, Point3d End, PositiveMagnitude Interval) : ContourPolicy;
    public sealed record SurfaceIsoCase(IsoStatus Status, double Parameter) : ContourPolicy;
    public sealed record MeshScalarCase(Arr<double> Values, Seq<double> Levels) : ContourPolicy;
    private ContourPolicy() { }
    public static Fin<ContourPolicy> Plane(Plane section, Op? key = null) =>
        new PlaneCase(Section: section).Admit(key: key.OrDefault());
    public static Fin<ContourPolicy> Axis(Point3d start, Point3d end, double interval, Op? key = null) {
        Op op = key.OrDefault();
        return op.AcceptValidated<PositiveMagnitude>(candidate: interval)
            .Bind(step => new AxisCase(Start: start, End: end, Interval: step).Admit(key: op));
    }
    public static Fin<ContourPolicy> SurfaceIso(IsoStatus status, double parameter, Op? key = null) =>
        new SurfaceIsoCase(Status: status, Parameter: parameter).Admit(key: key.OrDefault());
    public static Fin<ContourPolicy> MeshScalar(Arr<double> values, Seq<double> levels, Op? key = null) =>
        new MeshScalarCase(Values: values, Levels: levels).Admit(key: key.OrDefault());
    // Qualified Rasm.Domain.Admit: the Admit member name shadows the vocabulary class inside this type.
    internal Fin<ContourPolicy> Admit(Op key) => Switch(
        state: key,
        planeCase: static (op, policy) => Rasm.Domain.Admit.Plane(basis: policy.Section, key: op).Map(_ => (ContourPolicy)policy),
        axisCase: static (op, policy) =>
            from start in Rasm.Domain.Admit.Finite(point: policy.Start, key: op)
            from end in Rasm.Domain.Admit.Finite(point: policy.End, key: op)
            from span in guard((policy.End - policy.Start).Length > 0.0, op.InvalidInput())
            select (ContourPolicy)policy,
        surfaceIsoCase: static (op, policy) => (policy.Status, policy.Parameter) switch {
            (IsoStatus.X or IsoStatus.Y, double parameter) when double.IsFinite(parameter) => Fin.Succ<ContourPolicy>(policy),
            (IsoStatus.North or IsoStatus.East or IsoStatus.South or IsoStatus.West, _) => Fin.Succ<ContourPolicy>(policy),
            _ => Fin.Fail<ContourPolicy>(op.InvalidInput()),
        },
        meshScalarCase: static (op, policy) =>
            from scalars in Rasm.Domain.Admit.FiniteScalars(values: toSeq(policy.Values.AsIterable()), allowEmpty: false, key: op)
            from levels in Rasm.Domain.Admit.FiniteScalars(values: policy.Levels, allowEmpty: false, key: op)
            select (ContourPolicy)policy);
}

[Union]
public abstract partial record ExtractionDomain {
    public sealed record SupportCase : ExtractionDomain { internal SupportCase(SupportSpace value) => Value = value; public SupportSpace Value { get; } }
    public sealed record MeshCase : ExtractionDomain { internal MeshCase(MeshSpace value) => Value = value; public MeshSpace Value { get; } }
    public sealed record CloudCase : ExtractionDomain { internal CloudCase(VectorCloud value) => Value = value; public VectorCloud Value { get; } }
    private ExtractionDomain() { }
    public static Fin<ExtractionDomain> Support(SupportSpace value, Op? key = null) =>
        Optional(value).ToFin(key.OrDefault().InvalidInput()).Map(valid => (ExtractionDomain)new SupportCase(value: valid));
    // Op.Need is the null gate here: the Admit member name shadows the Rasm.Domain.Admit class inside this type.
    public static Fin<ExtractionDomain> Mesh(MeshSpace value, Op? key = null) =>
        key.OrDefault().Need(value.Native).Map(_ => (ExtractionDomain)new MeshCase(value: value));
    public static Fin<ExtractionDomain> Cloud(VectorCloud value, Op? key = null) {
        Op op = key.OrDefault();
        return op.Need(value)
            .Bind(cloud => cloud.Admit(key: op))
            .Map(static valid => (ExtractionDomain)new CloudCase(value: valid));
    }
    // The one polymorphic ingress: runtime shape discriminates, every arm admits through its owner.
    public static Fin<ExtractionDomain> Of(object? value, Context context, Op? key = null) {
        Op op = key.OrDefault();
        return Optional(value).ToFin(op.InvalidInput()).Bind(source => source switch {
            ExtractionDomain domain => domain.Admit(key: op),
            Mesh mesh => MeshSpace.Of(native: mesh, context: context, key: op).Bind(space => Mesh(value: space, key: op)),
            VectorCloud cloud => Cloud(value: cloud, key: op),
            PointCloud cloud => VectorCloud.Cluster(points: toSeq(cloud.GetPoints()), context: context, key: op).Bind(active => Cloud(value: active, key: op)),
            object candidate => SupportSpace.Of(value: candidate, key: op).Bind(space => Support(value: space, key: op)),
        });
    }
    internal Fin<ExtractionDomain> Admit(Op key) => Switch(
        state: key,
        supportCase: static (op, domain) => Support(value: domain.Value, key: op),
        meshCase: static (op, domain) => Mesh(value: domain.Value, key: op),
        cloudCase: static (op, domain) => Cloud(value: domain.Value, key: op));

    internal Fin<CurveBatch> Contours(ContourPolicy policy, Context context, Op key) => Switch(
        state: (Policy: policy, Context: context, Key: key),
        supportCase: static (state, domain) => domain.Value.Value switch {
            Brep brep => CurvesFromBrep(brep: brep, policy: state.Policy, key: state.Key),
            Mesh mesh => CurvesFromMesh(mesh: mesh, policy: state.Policy, context: state.Context, key: state.Key),
            Surface surface => CurvesFromSurface(surface: surface, policy: state.Policy, key: state.Key),
            VectorCloud.ClusterCase cloud => CurvesFromCloud(cloud: cloud, policy: state.Policy, context: state.Context, key: state.Key),
            _ => Fin.Fail<CurveBatch>(error: state.Key.Unsupported(geometryType: domain.Value.SourceType, outputType: typeof(Seq<Curve>))),
        },
        meshCase: static (state, domain) => CurvesFromMesh(mesh: domain.Value.Native, policy: state.Policy, context: state.Context, key: state.Key),
        cloudCase: static (state, domain) => domain.Value is VectorCloud.ClusterCase cloud
            ? CurvesFromCloud(cloud: cloud, policy: state.Policy, context: state.Context, key: state.Key)
            : Fin.Fail<CurveBatch>(error: state.Key.Unsupported(geometryType: domain.Value.GetType(), outputType: typeof(Seq<Curve>))));

    private static Fin<CurveBatch> CurvesFromBrep(Brep brep, ContourPolicy policy, Op key) =>
        key.Catch(() => policy.Switch(
            state: (Brep: brep, Key: key),
            planeCase: static (state, p) => AcceptCurves(curves: Brep.CreateContourCurves(brepToContour: state.Brep, sectionPlane: p.Section), nativeRouted: true, tolerance: ExtractionTolerance.RhinoDefault, key: state.Key),
            axisCase: static (state, p) => AcceptCurves(curves: Brep.CreateContourCurves(brepToContour: state.Brep, contourStart: p.Start, contourEnd: p.End, interval: p.Interval.Value), nativeRouted: true, tolerance: ExtractionTolerance.RhinoDefault, key: state.Key),
            surfaceIsoCase: static (state, _) => Fin.Fail<CurveBatch>(error: state.Key.Unsupported(geometryType: typeof(Brep), outputType: typeof(ContourPolicy.SurfaceIsoCase))),
            meshScalarCase: static (state, _) => Fin.Fail<CurveBatch>(error: state.Key.Unsupported(geometryType: typeof(Brep), outputType: typeof(ContourPolicy.MeshScalarCase)))));
    private static Fin<CurveBatch> CurvesFromMesh(Mesh mesh, ContourPolicy policy, Context context, Op key) =>
        key.Catch(() => policy.Switch(
            state: (Mesh: mesh, Context: context, Key: key),
            planeCase: static (state, p) => AcceptCurves(curves: Rhino.Geometry.Mesh.CreateContourCurves(meshToContour: state.Mesh, sectionPlane: p.Section, tolerance: state.Context.Absolute.Value), nativeRouted: true, tolerance: ExtractionTolerance.FromContext(state.Context.Absolute.Value), key: state.Key),
            axisCase: static (state, p) => AcceptCurves(curves: Rhino.Geometry.Mesh.CreateContourCurves(meshToContour: state.Mesh, contourStart: p.Start, contourEnd: p.End, interval: p.Interval.Value, tolerance: state.Context.Absolute.Value), nativeRouted: true, tolerance: ExtractionTolerance.FromContext(state.Context.Absolute.Value), key: state.Key),
            meshScalarCase: static (state, p) => ScalarIsolinesDetailed(mesh: state.Mesh, values: p.Values, levels: p.Levels, context: state.Context, key: state.Key)
                .Bind(result => AcceptCurves(curves: result.Curves, attempted: result.Receipt.StitchedCandidates, nativeRouted: false, tolerance: ExtractionTolerance.FromContext(state.Context.Absolute.Value), scalarIsoline: Some(result), key: state.Key)),
            surfaceIsoCase: static (state, _) => Fin.Fail<CurveBatch>(error: state.Key.Unsupported(geometryType: typeof(Mesh), outputType: typeof(ContourPolicy.SurfaceIsoCase)))));
    private static Fin<CurveBatch> CurvesFromSurface(Surface surface, ContourPolicy policy, Op key) =>
        key.Catch(() => policy.Switch(
            state: (Surface: surface, Key: key),
            surfaceIsoCase: static (state, p) =>
                from frame in IsoFrame(status: p.Status, parameter: p.Parameter, domain: state.Surface.Domain, key: state.Key)
                from curves in state.Surface is BrepFace face
                    ? Optional(face.TrimAwareIsoCurve(direction: frame.Direction, constantParameter: frame.Parameter)).ToFin(state.Key.InvalidResult())
                    : Optional(state.Surface.IsoCurve(direction: frame.Direction, constantParameter: frame.Parameter)).ToFin(state.Key.InvalidResult()).Map(curve => (Curve[])[curve])
                from batch in AcceptCurves(curves: curves, nativeRouted: true, tolerance: ExtractionTolerance.NotApplicable, key: state.Key)
                select batch,
            planeCase: static (state, _) => Fin.Fail<CurveBatch>(error: state.Key.Unsupported(geometryType: typeof(Surface), outputType: typeof(ContourPolicy.PlaneCase))),
            axisCase: static (state, _) => Fin.Fail<CurveBatch>(error: state.Key.Unsupported(geometryType: typeof(Surface), outputType: typeof(ContourPolicy.AxisCase))),
            meshScalarCase: static (state, _) => Fin.Fail<CurveBatch>(error: state.Key.Unsupported(geometryType: typeof(Surface), outputType: typeof(ContourPolicy.MeshScalarCase)))));
    // THE one IsoStatus resolver: X/Y carry the caller parameter; edge statuses read the opposite-direction
    // domain end. direction = which way the RESULTING curve runs (RhinoCommon IsoCurve law).
    private static Fin<(int Direction, double Parameter)> IsoFrame(IsoStatus status, double parameter, Func<int, Interval> domain, Op key) =>
        status switch {
            IsoStatus.X => key.Finite(parameter).Map(_ => (Direction: 1, Parameter: parameter)),
            IsoStatus.Y => key.Finite(parameter).Map(_ => (Direction: 0, Parameter: parameter)),
            IsoStatus.West => Fin.Succ((Direction: 1, Parameter: domain(0).T0)),
            IsoStatus.East => Fin.Succ((Direction: 1, Parameter: domain(0).T1)),
            IsoStatus.South => Fin.Succ((Direction: 0, Parameter: domain(1).T0)),
            IsoStatus.North => Fin.Succ((Direction: 0, Parameter: domain(1).T1)),
            _ => Fin.Fail<(int Direction, double Parameter)>(key.Unsupported(geometryType: typeof(Surface), outputType: typeof(ContourPolicy.SurfaceIsoCase))),
        };
    private static Fin<CurveBatch> CurvesFromCloud(VectorCloud.ClusterCase cloud, ContourPolicy policy, Context context, Op key) =>
        key.Catch(() => policy.Switch(
            state: (Cloud: cloud, Context: context, Key: key),
            axisCase: static (state, p) => AcceptCurves(curves: state.Cloud.Indexed.CreateContourCurves(contourStart: p.Start, contourEnd: p.End, interval: p.Interval.Value, absoluteTolerance: state.Context.Absolute.Value), nativeRouted: true, tolerance: ExtractionTolerance.FromContext(state.Context.Absolute.Value), key: state.Key),
            planeCase: static (state, p) => AcceptCurves(curves: state.Cloud.Indexed.CreateSectionCurve(plane: p.Section, absoluteTolerance: state.Context.Absolute.Value), nativeRouted: true, tolerance: ExtractionTolerance.FromContext(state.Context.Absolute.Value), key: state.Key),
            surfaceIsoCase: static (state, _) => Fin.Fail<CurveBatch>(error: state.Key.Unsupported(geometryType: typeof(PointCloud), outputType: typeof(ContourPolicy.SurfaceIsoCase))),
            meshScalarCase: static (state, _) => Fin.Fail<CurveBatch>(error: state.Key.Unsupported(geometryType: typeof(PointCloud), outputType: typeof(ContourPolicy.MeshScalarCase)))));
    private static Fin<CurveBatch> AcceptCurves(Curve[] curves, bool nativeRouted, ExtractionTolerance tolerance, Op key) =>
        Optional(curves).ToFin(key.InvalidResult())
            .Bind(active => AcceptCurves(curves: toSeq(active), attempted: active.Length, nativeRouted: nativeRouted, tolerance: tolerance, key: key));
    private static Fin<CurveBatch> AcceptCurves(Seq<Curve> curves, int attempted, bool nativeRouted, ExtractionTolerance tolerance, Op key, Option<ScalarIsolineResult> scalarIsoline = default) {
        Seq<Curve> accepted = curves.Filter(static curve => curve is not null && curve.IsValid);
        return ExtractionReceipt.Of(
                route: nativeRouted ? ExtractionRoute.Native : ExtractionRoute.Local, attempted: attempted, emitted: accepted.Count,
                tolerance: tolerance, parallelCallback: false, key: key,
                scalarIsoline: scalarIsoline.Map(static result => result.Receipt),
                itemFailures: nativeRouted ? Some(attempted - accepted.Count) : Option<int>.None)
            .Map(receipt => new CurveBatch(Curves: accepted, ScalarIsoline: scalarIsoline, Receipt: receipt));
    }

    // BOUNDARY ADAPTER — RhinoCommon owns no per-vertex scalar contour; this PL kernel follows the
    // triangulated topology and returns stitched candidates only. Named statement-kernel exemption.
    private static Fin<ScalarIsolineResult> ScalarIsolinesDetailed(Mesh mesh, Arr<double> values, Seq<double> levels, Context context, Op key) {
        if (values.Count != mesh.Vertices.Count || values.Exists(static value => !double.IsFinite(value)) || levels.IsEmpty || levels.Exists(static value => !double.IsFinite(value)))
            return Fin.Fail<ScalarIsolineResult>(key.InvalidInput());
        using Mesh triangulated = mesh.DuplicateMesh();
        if (triangulated.Faces.QuadCount > 0 && !triangulated.Faces.ConvertQuadsToTriangles()) return Fin.Fail<ScalarIsolineResult>(key.InvalidResult());
        if (triangulated.Vertices.Count != values.Count) return Fin.Fail<ScalarIsolineResult>(key.InvalidResult());
        List<ScalarIsolineSegment> segments = [];
        IsolineLedger ledger = default;
        for (int f = 0; f < triangulated.Faces.Count; f++) {
            MeshFace face = triangulated.Faces[index: f];
            if (face.IsTriangle) ledger = AddFaceIsolines(mesh: triangulated, face: face, values: values, levels: levels, tolerance: context.Absolute.Value, segments: segments, ledger: ledger);
        }
        (Seq<ScalarIsolineSegment> deduped, IsolineLedger afterDedup) = DeduplicateSegments(segments: segments, tolerance: context.Absolute.Value, ledger: ledger);
        (Seq<Curve> curves, IsolineLedger final) = StitchSegments(segments: deduped, tolerance: context.Absolute.Value, ledger: afterDedup);
        return Fin.Succ(new ScalarIsolineResult(Curves: curves, Receipt: new ScalarIsolineReceipt(
            NativeRouted: false, FiniteLevels: levels.Count, RawSegments: final.RawSegments, DedupedSegments: final.DedupedSegments,
            DegenerateRejected: final.DegenerateRejected, PlateauRejected: final.PlateauRejected, StitchedCandidates: final.StitchedCandidates,
            BranchStops: final.BranchStops, BranchNodes: final.BranchNodes, MaxIncidentSegments: final.MaxIncidentSegments, EmittedCurves: final.EmittedCurves)));
    }
    private static IsolineLedger AddFaceIsolines(Mesh mesh, MeshFace face, Arr<double> values, Seq<double> levels, double tolerance, List<ScalarIsolineSegment> segments, IsolineLedger ledger) {
        Point3d[] points = [mesh.Vertices[index: face.A], mesh.Vertices[index: face.B], mesh.Vertices[index: face.C]];
        double[] scalars = [values[index: face.A], values[index: face.B], values[index: face.C]];
        (int A, int B)[] edges = [(0, 1), (1, 2), (2, 0)];
        foreach (double level in levels.AsIterable()) {
            double epsilon = EpsilonPolicy.SqrtEpsilon * Math.Max(1.0, Math.Max(Math.Abs(value: level), scalars.Max(static value => Math.Abs(value: value))));
            if (scalars.All(value => Math.Abs(value: value - level) <= epsilon)) { ledger = ledger with { PlateauRejected = ledger.PlateauRejected + 1 }; continue; }
            ((int A, int B) Edge, double ADelta, double BDelta)[] cuts = System.Array.ConvertAll(array: edges, converter: edge => (edge, scalars[edge.A] - level, scalars[edge.B] - level));
            ScalarIsolineSegment[] edgeSegments = System.Array.ConvertAll(
                array: System.Array.FindAll(array: cuts, match: cut => Math.Abs(value: cut.ADelta) <= epsilon && Math.Abs(value: cut.BDelta) <= epsilon),
                converter: cut => new ScalarIsolineSegment(A: points[cut.Edge.A], B: points[cut.Edge.B]));
            segments.AddRange(collection: edgeSegments);
            ledger = ledger with { RawSegments = ledger.RawSegments + edgeSegments.Length };
            Point3d[] unique = [.. cuts.SelectMany(cut =>
                    (Math.Abs(value: cut.ADelta) <= epsilon, Math.Abs(value: cut.BDelta) <= epsilon, cut.ADelta * cut.BDelta < 0.0) switch {
                        (true, true, _) => (Point3d[])[],
                        (true, false, _) => [points[cut.Edge.A]],
                        (false, true, _) => [points[cut.Edge.B]],
                        (false, false, true) => [points[cut.Edge.A] + (-cut.ADelta / (cut.BDelta - cut.ADelta) * (points[cut.Edge.B] - points[cut.Edge.A]))],
                        _ => [],
                    })
                .Where(predicate: static point => point.IsValid)
                .DistinctBy(keySelector: point => KeyOf(point: point, tolerance: tolerance))];
            if (unique.Length == 2) { segments.Add(item: new ScalarIsolineSegment(A: unique[0], B: unique[1])); ledger = ledger with { RawSegments = ledger.RawSegments + 1 }; }
        }
        return ledger;
    }
    private static (Seq<ScalarIsolineSegment> Unique, IsolineLedger Ledger) DeduplicateSegments(List<ScalarIsolineSegment> segments, double tolerance, IsolineLedger ledger) {
        SegmentKeySet seen = [];
        List<ScalarIsolineSegment> unique = [];
        foreach (ScalarIsolineSegment segment in segments) {
            ScalarIsolinePointKey a = KeyOf(point: segment.A, tolerance: tolerance);
            ScalarIsolinePointKey b = KeyOf(point: segment.B, tolerance: tolerance);
            if (a.Equals(b)) { ledger = ledger with { DegenerateRejected = ledger.DegenerateRejected + 1 }; continue; }
            (ScalarIsolinePointKey A, ScalarIsolinePointKey B) edge = a.Compare(other: b) <= 0 ? (a, b) : (b, a);
            if (seen.Add(item: edge)) unique.Add(item: segment);
        }
        return (toSeq(unique), ledger with { DedupedSegments = unique.Count });
    }
    private static (Seq<Curve> Curves, IsolineLedger Ledger) StitchSegments(Seq<ScalarIsolineSegment> segments, double tolerance, IsolineLedger ledger) {
        ScalarIsolineSegment[] all = [.. segments.AsIterable()];
        bool[] used = new bool[all.Length];
        Dictionary<ScalarIsolinePointKey, List<int>> incident = [];
        for (int i = 0; i < all.Length; i++) {
            ref List<int>? a = ref CollectionsMarshal.GetValueRefOrAddDefault(dictionary: incident, key: KeyOf(point: all[i].A, tolerance: tolerance), exists: out _);
            ref List<int>? b = ref CollectionsMarshal.GetValueRefOrAddDefault(dictionary: incident, key: KeyOf(point: all[i].B, tolerance: tolerance), exists: out _);
            (a ??= []).Add(item: i);
            (b ??= []).Add(item: i);
        }
        (int maxIncident, int branchNodes) = incident.Values.Select(static edges => edges.Count).Aggregate(
            seed: (Max: ledger.MaxIncidentSegments, Branches: ledger.BranchNodes),
            func: static (state, count) => (Math.Max(val1: state.Max, val2: count), state.Branches + (count > 2 ? 1 : 0)));
        ledger = ledger with { MaxIncidentSegments = maxIncident, BranchNodes = branchNodes };
        List<Curve> curves = [];
        int attempted = 0;
        for (int i = 0; i < all.Length; i++) {
            if (used[i]) continue;
            List<Point3d> points = [all[i].A, all[i].B];
            used[i] = true;
            ledger = Extend(points: points, atEnd: true, all: all, used: used, incident: incident, tolerance: tolerance, ledger: ledger);
            ledger = Extend(points: points, atEnd: false, all: all, used: used, incident: incident, tolerance: tolerance, ledger: ledger);
            Polyline polyline = [.. points];
            attempted++;
            if (polyline.IsValid && polyline.Count >= 2) curves.Add(item: polyline.ToPolylineCurve());
        }
        return (toSeq(curves), ledger with { StitchedCandidates = attempted, EmittedCurves = curves.Count });
    }
    private static IsolineLedger Extend(List<Point3d> points, bool atEnd, ScalarIsolineSegment[] all, bool[] used, Dictionary<ScalarIsolinePointKey, List<int>> incident, double tolerance, IsolineLedger ledger) {
        bool moved = true;
        while (moved) {
            moved = false;
            Point3d anchor = atEnd ? points[^1] : points[index: 0];
            ScalarIsolinePointKey key = KeyOf(point: anchor, tolerance: tolerance);
            if (!incident.TryGetValue(key: key, value: out List<int>? candidates)) continue;
            foreach (int index in candidates) {
                if (used[index]) continue;
                ScalarIsolineSegment segment = all[index];
                Point3d next = KeyOf(point: segment.A, tolerance: tolerance).Equals(key) ? segment.B : segment.A;
                int available = candidates.Count(candidate => !used[candidate]);
                ledger = available > 1 ? ledger with { BranchStops = ledger.BranchStops + 1 } : ledger;
                if (available == 1) {
                    points.Insert(index: atEnd ? points.Count : 0, item: next);
                    used[index] = true;
                    moved = true;
                }
                break;
            }
        }
        return ledger;
    }
    private static ScalarIsolinePointKey KeyOf(Point3d point, double tolerance) {
        double scale = 1.0 / Math.Max(val1: tolerance, val2: EpsilonPolicy.SqrtEpsilon);
        return new ScalarIsolinePointKey(
            X: (long)Math.Round(point.X * scale, MidpointRounding.ToEven),
            Y: (long)Math.Round(point.Y * scale, MidpointRounding.ToEven),
            Z: (long)Math.Round(point.Z * scale, MidpointRounding.ToEven));
    }
}

[Union]
public abstract partial record ExtractionProbe {
    public sealed record VectorCase(VectorField Source) : ExtractionProbe;
    public sealed record ScalarCase(ScalarField Source) : ExtractionProbe;
    public sealed record TensorCase(TensorField Source) : ExtractionProbe;
    private ExtractionProbe() { }
    public static ExtractionProbe Vector(VectorField source) => new VectorCase(Source: source);
    public static ExtractionProbe Scalar(ScalarField source) => new ScalarCase(Source: source);
    public static ExtractionProbe Tensor(TensorField source) => new TensorCase(Source: source);
    // The bare constructors stay cheap; the request boundary (Extraction.Probe) runs this source gate,
    // so a null field union never reaches Project. Op.Need: the member name shadows the Admit class.
    internal Fin<ExtractionProbe> Admit(Op key) => Switch(
        state: key,
        vectorCase: static (op, probe) => op.Need(probe.Source).Map(_ => (ExtractionProbe)probe),
        scalarCase: static (op, probe) => op.Need(probe.Source).Map(_ => (ExtractionProbe)probe),
        tensorCase: static (op, probe) => op.Need(probe.Source).Map(_ => (ExtractionProbe)probe));
    // Typed rows kill the typeof(TOut) branching; only the matched row's producer runs. The mesh-bound
    // log-map rows route to geodesics.md's kernel; the Hodge evidence rows route to dec.md's memoized
    // HodgeSolutionOf seat; scalar rows ride fields.md's tagged rails.
    internal Fin<TOut> Project<TOut>(Point3d sample, Context context, Op key) => Switch(
        state: (Sample: sample, Context: context, Key: key),
        vectorCase: static (state, probe) => AtomProjection.Rows<ExtractionProbe.VectorCase, TOut>(self: probe, key: state.Key, owner: typeof(VectorCase),
            ProjectionRow.Of<TangentLogMapResult>(() => probe.Source is VectorField.TangentLogMapCase log
                ? GeodesicKernel.TangentLogMapAt(space: log.Space, source: log.Source, sample: state.Sample, time: log.Time.Value, algorithm: log.Algorithm, trace: log.Trace, windows: log.Windows, key: state.Key)
                : Fin.Fail<TangentLogMapResult>(state.Key.Unsupported(geometryType: probe.Source.GetType(), outputType: typeof(TangentLogMapResult)))),
            ProjectionRow.Of<TangentLogMapReceipt>(() => probe.Source is VectorField.TangentLogMapCase log
                ? GeodesicKernel.TangentLogMapAt(space: log.Space, source: log.Source, sample: state.Sample, time: log.Time.Value, algorithm: log.Algorithm, trace: log.Trace, windows: log.Windows, key: state.Key).Map(static result => result.Receipt)
                : Fin.Fail<TangentLogMapReceipt>(state.Key.Unsupported(geometryType: probe.Source.GetType(), outputType: typeof(TangentLogMapReceipt)))),
            ProjectionRow.Of<HodgeDecompositionReceipt>(() => probe.Source is VectorField.HodgeCase hodge
                ? DecAssembly.HodgeSolutionOf(source: hodge.Source, space: hodge.Space, context: state.Context, key: state.Key).Map(static solved => solved.Receipt)
                : Fin.Fail<HodgeDecompositionReceipt>(state.Key.Unsupported(geometryType: probe.Source.GetType(), outputType: typeof(HodgeDecompositionReceipt)))),
            ProjectionRow.Of<HarmonicOneFormReceipt>(() => probe.Source is VectorField.HodgeCase hodge
                ? DecAssembly.HodgeSolutionOf(source: hodge.Source, space: hodge.Space, context: state.Context, key: state.Key).Bind(solved => solved.Receipt.Harmonic.ToFin(state.Key.InvalidResult()))
                : Fin.Fail<HarmonicOneFormReceipt>(state.Key.Unsupported(geometryType: probe.Source.GetType(), outputType: typeof(HarmonicOneFormReceipt)))),
            ProjectionRow.Of<Vector3d>(() => probe.Source.SampleVector(sample: state.Sample, context: state.Context, key: state.Key)),
            ProjectionRow.Of<double>(() => probe.Source.SampleVector(sample: state.Sample, context: state.Context, key: state.Key).Map(static vector => vector.Length)),
            ProjectionRow.Of<VectorSpan>(() => SpanAt(probe: probe, state: state)),
            ProjectionRow.Of<Direction>(() => SpanAt(probe: probe, state: state).Bind(span => span.Project<Direction>(key: state.Key))),
            ProjectionRow.Of<Line>(() => SpanAt(probe: probe, state: state).Bind(span => span.Project<Line>(key: state.Key)))),
        scalarCase: static (state, probe) => AtomProjection.Rows<ExtractionProbe.ScalarCase, TOut>(self: probe, key: state.Key, owner: typeof(ScalarCase),
            ProjectionRow.Of<SdfSample>(() => probe.Source.SampleSdfDetailed(sample: state.Sample, context: state.Context, key: state.Key)),
            ProjectionRow.Of<FieldSample>(() => probe.Source.SampleDetailed(sample: state.Sample, context: state.Context, key: state.Key)),
            ProjectionRow.Of<double>(() => probe.Source.SampleScalar(sample: state.Sample, context: state.Context, key: state.Key))),
        tensorCase: static (state, probe) => AtomProjection.Rows<ExtractionProbe.TensorCase, TOut>(self: probe, key: state.Key, owner: typeof(TensorCase),
            ProjectionRow.Of<SymmetricMatrix>(() => probe.Source.SampleTensor(sample: state.Sample, context: state.Context, key: state.Key)),
            ProjectionRow.Of<Seq<(double Eigenvalue, Direction Eigenvector)>>(() => probe.Source.PrincipalDirections(sample: state.Sample, context: state.Context, key: state.Key))));
    private static Fin<VectorSpan> SpanAt(VectorCase probe, (Point3d Sample, Context Context, Op Key) state) =>
        probe.Source.SampleVector(sample: state.Sample, context: state.Context, key: state.Key)
            .Bind(vector => VectorSpan.Of(anchor: state.Sample, vector: vector, context: state.Context, key: state.Key));
}

// ONE sampled-mode family over one shared seed generator; the Glyph/Grid/StreamBundle policy triple is dead.
[Union]
public abstract partial record SampledExtraction {
    public sealed record GlyphCase(VectorField Field, PositiveMagnitude Scale) : SampledExtraction;
    public sealed record GridCase(ScalarField Field) : SampledExtraction;
    public sealed record StreamBundleCase(VectorField Field, PositiveMagnitude InitialStep, FieldIntegrator Integrator, Termination Termination) : SampledExtraction;
    private SampledExtraction() { }
    public static Fin<SampledExtraction> Glyph(VectorField field, double scale, Op? key = null) {
        Op op = key.OrDefault();
        return from source in Admit.NotNull(value: field, key: op)
               from magnitude in op.AcceptValidated<PositiveMagnitude>(candidate: scale)
               select (SampledExtraction)new GlyphCase(Field: source, Scale: magnitude);
    }
    public static Fin<SampledExtraction> Grid(ScalarField field, Op? key = null) =>
        Admit.NotNull(value: field, key: key.OrDefault()).Map(static source => (SampledExtraction)new GridCase(Field: source));
    public static Fin<SampledExtraction> StreamBundle(VectorField field, double initialStep, Termination termination, FieldIntegrator? integrator = null, Op? key = null) {
        Op op = key.OrDefault();
        return from source in Admit.NotNull(value: field, key: op)
               from step in op.AcceptValidated<PositiveMagnitude>(candidate: initialStep)
               from stop in Termination.Admit(value: termination, key: op)
               from active in FieldIntegrator.AdmitOrFixed(value: integrator, key: op)
               select (SampledExtraction)new StreamBundleCase(Field: source, InitialStep: step, Integrator: active, Termination: stop);
    }
}

// The public extraction request vocabulary; intent.md carries it as ONE case.
[Union]
public abstract partial record Extraction {
    public sealed record ProbeCase(ExtractionProbe Source, Point3d Sample) : Extraction;
    public sealed record ContourCase(ExtractionDomain Domain, ContourPolicy Policy) : Extraction;
    public sealed record IsoSurfaceCase(ScalarField Field, BoundingBox Bounds, Dimension Resolution, Dimension MaxRootSteps) : Extraction;
    public sealed record SampledCase(SampledExtraction Mode, ExtractionDomain Domain, SampleKind Seeds) : Extraction;
    private Extraction() { }
    public static Fin<Extraction> Probe(ExtractionProbe source, Point3d sample, Op? key = null) {
        Op op = key.OrDefault();
        return from validSource in Optional(source).ToFin(op.InvalidInput()).Bind(active => active.Admit(key: op))
               from validSample in op.AcceptValue(value: sample)
               select (Extraction)new ProbeCase(Source: validSource, Sample: validSample);
    }
    public static Fin<Extraction> Contour(ExtractionDomain domain, ContourPolicy policy, Op? key = null) {
        Op op = key.OrDefault();
        return from validDomain in Optional(domain).ToFin(op.InvalidInput()).Bind(active => active.Admit(key: op))
               from validPolicy in Optional(policy).ToFin(op.InvalidInput()).Bind(active => active.Admit(key: op))
               select (Extraction)new ContourCase(Domain: validDomain, Policy: validPolicy);
    }
    public static Fin<Extraction> IsoSurface(ScalarField field, BoundingBox bounds, int resolution, int maxRootSteps, Op? key = null) {
        Op op = key.OrDefault();
        return from validField in Optional(field).ToFin(op.InvalidInput())
               from _ in guard(bounds.IsValid && bounds.Diagonal.Length > 0.0, op.InvalidInput())
               from cells in op.AcceptValidated<Dimension>(candidate: resolution)
               from steps in op.AcceptValidated<Dimension>(candidate: maxRootSteps)
               select (Extraction)new IsoSurfaceCase(Field: validField, Bounds: bounds, Resolution: cells, MaxRootSteps: steps);
    }
    public static Fin<Extraction> Sampled(SampledExtraction mode, ExtractionDomain domain, SampleKind seeds, Op? key = null) {
        Op op = key.OrDefault();
        return from validMode in Admit.NotNull(value: mode, key: op)
               from validDomain in Optional(domain).ToFin(op.InvalidInput()).Bind(active => active.Admit(key: op))
               from validSeeds in SampleKind.Admit(value: seeds, key: op)
               select (Extraction)new SampledCase(Mode: validMode, Domain: validDomain, Seeds: validSeeds);
    }

    internal Fin<TOut> Project<TOut>(Context context, Op key) => Switch(
        state: (Context: context, Key: key),
        probeCase: static (state, extraction) => extraction.Source.Project<TOut>(sample: extraction.Sample, context: state.Context, key: state.Key),
        contourCase: static (state, extraction) =>
            from batch in extraction.Domain.Contours(policy: extraction.Policy, context: state.Context, key: state.Key)
            from output in AtomProjection.Rows<ExtractionReceipt, TOut>(self: batch.Receipt, key: state.Key, owner: typeof(ContourCase),
                ProjectionRow.Of<Seq<Curve>>(() => Fin.Succ(batch.Curves)),
                ProjectionRow.Of<ScalarIsolineResult>(() => batch.ScalarIsoline.ToFin(Fail: state.Key.Unsupported(geometryType: typeof(ContourPolicy), outputType: typeof(ScalarIsolineResult)))),
                ProjectionRow.Of<ScalarIsolineReceipt>(() => batch.ScalarIsoline.Map(static result => result.Receipt).ToFin(Fail: state.Key.Unsupported(geometryType: typeof(ContourPolicy), outputType: typeof(ScalarIsolineReceipt)))))
            select output,
        // Marching cubes is reconstruct.md's IsoSurface owner; the case's root-step budget rides its policy row.
        isoSurfaceCase: static (state, extraction) =>
            from result in IsoSurface.Detailed(field: extraction.Field, bounds: extraction.Bounds, resolution: extraction.Resolution.Value,
                policy: IsoSurfacePolicy.Default with { MaxRootSteps = extraction.MaxRootSteps }, context: state.Context, key: state.Key)
            from output in AtomProjection.Rows<IsoSurfaceReceipt, TOut>(self: result.Receipt, key: state.Key, owner: typeof(IsoSurfaceCase),
                ProjectionRow.Of<Mesh>(() => result.Receipt.Valid ? Fin.Succ(result.Mesh) : Fin.Fail<Mesh>(state.Key.InvalidResult())),
                ProjectionRow.Of<IsoSurfaceResult>(() => result.Receipt.Valid ? Fin.Succ(result) : Fin.Fail<IsoSurfaceResult>(state.Key.InvalidResult())),
                ProjectionRow.Of<ExtractionReceipt>(() => ExtractionReceipt.Of(
                    route: result.Receipt.NativeRouted ? ExtractionRoute.Native : ExtractionRoute.Local,
                    attempted: 1, emitted: result.Receipt.Valid ? 1 : 0,
                    tolerance: result.Receipt.FixedTolerance.Map(ExtractionTolerance.RhinoFixed).IfNone(ExtractionTolerance.RhinoDefault),
                    parallelCallback: result.Receipt.ParallelCallback, key: state.Key,
                    isoSurface: Some(result.Receipt), itemFailures: result.Receipt.Valid ? Option<int>.None : Some(1))))
            select output,
        sampledCase: static (state, extraction) => extraction.Mode.Switch(
            state: (Domain: extraction.Domain, Seeds: extraction.Seeds, Context: state.Context, Key: state.Key),
            glyphCase: static (s, mode) => ProjectSamples<TOut, Line>(
                seeds: s.Seeds, domain: s.Domain, context: s.Context, key: s.Key,
                sample: (point, model, op) => ExtractionProbe.Vector(source: mode.Field).Project<VectorSpan>(sample: point, context: model, key: op)
                    .Map(span => new Line(span.Anchor, span.Anchor + (mode.Scale.Value * span.Value))),
                project: static (glyphs, rejected, receipt, op) => AtomProjection.Rows<ExtractionReceipt, TOut>(self: receipt, key: op, owner: typeof(SampledExtraction.GlyphCase),
                    ProjectionRow.Of<Seq<Line>>(() => rejected == 0 ? Fin.Succ(glyphs) : Fin.Fail<Seq<Line>>(op.InvalidResult())))),
            gridCase: static (s, mode) => ProjectSamples<TOut, (Point3d Point, double Value)>(
                seeds: s.Seeds, domain: s.Domain, context: s.Context, key: s.Key,
                sample: (point, model, op) => mode.Field.SampleScalar(sample: point, context: model, key: op).Map(value => (Point: point, Value: value)),
                project: static (samples, rejected, receipt, op) => AtomProjection.Rows<ExtractionReceipt, TOut>(self: receipt, key: op, owner: typeof(SampledExtraction.GridCase),
                    ProjectionRow.Of<Seq<(Point3d Point, double Value)>>(() => rejected == 0 && samples.ForAll(static sample => sample.Point.IsValid && double.IsFinite(sample.Value))
                        ? Fin.Succ(samples)
                        : Fin.Fail<Seq<(Point3d Point, double Value)>>(op.InvalidResult())))),
            streamBundleCase: static (s, mode) => ProjectSamples<TOut, StreamlineTrace>(
                seeds: s.Seeds, domain: s.Domain, context: s.Context, key: s.Key,
                sample: (seed, model, op) => FlowKernel.Trace<StreamlineTrace>(source: mode.Field, seed: seed, initialStep: mode.InitialStep, integrator: mode.Integrator, termination: mode.Termination, context: model, key: op),
                project: static (traces, rejected, receipt, op) => AtomProjection.Rows<ExtractionReceipt, TOut>(self: receipt, key: op, owner: typeof(SampledExtraction.StreamBundleCase),
                    ProjectionRow.Of<Seq<StreamlineTrace>>(() => rejected == 0 ? traces.TraverseM(trace => FlowKernel.ProjectTrace<StreamlineTrace>(trace: trace, key: op)).As() : Fin.Fail<Seq<StreamlineTrace>>(op.InvalidResult())),
                    ProjectionRow.Of<Seq<Polyline>>(() => rejected == 0 ? traces.TraverseM(trace => FlowKernel.ProjectTrace<Polyline>(trace: trace, key: op)).As() : Fin.Fail<Seq<Polyline>>(op.InvalidResult())),
                    ProjectionRow.Of<Seq<Curve>>(() => rejected == 0 ? traces.TraverseM(trace => FlowKernel.ProjectTrace<Curve>(trace: trace, key: op)).As() : Fin.Fail<Seq<Curve>>(op.InvalidResult()))))));

    // The ONE sampled spine: seed via sample.md, fold the mode's item arm, mint the receipt, project through
    // one Rows call with the receipt as the implicit self row — items gate on zero rejections.
    private static Fin<TOut> ProjectSamples<TOut, TItem>(SampleKind seeds, ExtractionDomain domain, Context context, Op key, Func<Point3d, Context, Op, Fin<TItem>> sample, Func<Seq<TItem>, int, ExtractionReceipt, Op, Fin<TOut>> project) =>
        from samples in seeds.Evaluate(domain: domain, context: context, key: key)
        let sampled = samples.Points.Fold(
            initialState: (Items: (Seq<TItem>)[], Rejected: 0),
            f: (state, point) => sample(point, context, key).Match(
                Succ: item => (state.Items.Add(item), state.Rejected),
                Fail: _ => (state.Items, state.Rejected + 1)))
        from receipt in ExtractionReceipt.Of(
            route: ExtractionRoute.Local, attempted: samples.Points.Count, emitted: sampled.Items.Count,
            tolerance: ExtractionTolerance.NotApplicable, parallelCallback: false, key: key,
            sample: Some(samples.Receipt), itemFailures: Some(sampled.Rejected))
        from output in project(sampled.Items, sampled.Rejected, receipt, key)
        select output;
}

// --- [MODELS] ---------------------------------------------------------------------------------
internal readonly record struct CurveBatch(Seq<Curve> Curves, Option<ScalarIsolineResult> ScalarIsoline, ExtractionReceipt Receipt);

[StructLayout(LayoutKind.Auto)]
internal readonly record struct ScalarIsolinePointKey(long X, long Y, long Z) {
    internal int Compare(ScalarIsolinePointKey other) => (X, Y, Z).CompareTo((other.X, other.Y, other.Z));
}

[StructLayout(LayoutKind.Auto)]
internal readonly record struct ScalarIsolineSegment(Point3d A, Point3d B);

// Immutable kernel ledger folded through the isoline passes; the former mutable stats class is dead.
[StructLayout(LayoutKind.Auto)]
internal readonly record struct IsolineLedger(int RawSegments, int DedupedSegments, int DegenerateRejected, int PlateauRejected, int StitchedCandidates, int BranchStops, int BranchNodes, int MaxIncidentSegments, int EmittedCurves);

// IsValid = ONE ValidityClaim.All fold (Domain/rails.md) over the count rows.
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct ScalarIsolineReceipt(bool NativeRouted, int FiniteLevels, int RawSegments, int DedupedSegments, int DegenerateRejected, int PlateauRejected, int StitchedCandidates, int BranchStops, int BranchNodes, int MaxIncidentSegments, int EmittedCurves) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(FiniteLevels, 1), ValidityClaim.CountAtLeast(RawSegments, 0),
        ValidityClaim.CountAtLeast(DedupedSegments, 0), ValidityClaim.CountAtLeast(DegenerateRejected, 0),
        ValidityClaim.CountAtLeast(PlateauRejected, 0), ValidityClaim.CountAtLeast(StitchedCandidates, 0),
        ValidityClaim.CountAtLeast(BranchStops, 0), ValidityClaim.CountAtLeast(BranchNodes, 0),
        ValidityClaim.CountAtLeast(MaxIncidentSegments, 0), ValidityClaim.CountAtLeast(EmittedCurves, 0),
        ValidityClaim.Of(DedupedSegments <= RawSegments), ValidityClaim.Of(EmittedCurves <= StitchedCandidates));
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct ScalarIsolineResult(Seq<Curve> Curves, ScalarIsolineReceipt Receipt);

// Route retained, completion DERIVED from the counts, tolerance provenance on the value, ONE failure slot.
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct ExtractionReceipt(
    ExtractionRoute Route, int Attempted, int Emitted, ExtractionTolerance Tolerance, bool ParallelCallback,
    Option<IsoSurfaceReceipt> IsoSurface = default, Option<ScalarIsolineReceipt> ScalarIsoline = default,
    Option<SampleReceipt> Sample = default, Option<int> ItemFailures = default) : IValidityEvidence {
    public int Rejected => Attempted - Emitted;
    public bool Complete => Emitted == Attempted;
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(Attempted, 0), ValidityClaim.CountAtLeast(Emitted, 0),
        ValidityClaim.Of(Emitted <= Attempted),
        ValidityClaim.Of(ItemFailures.Map(static count => count >= 0).IfNone(noneValue: true)),
        ValidityClaim.Of(IsoSurface.Map(static receipt => receipt.IsValid).IfNone(noneValue: true)),
        ValidityClaim.Of(ScalarIsoline.Map(static receipt => receipt.IsValid).IfNone(noneValue: true)),
        ValidityClaim.Of(Sample.Map(static receipt => receipt.IsValid).IfNone(noneValue: true)));
    internal static Fin<ExtractionReceipt> Of(ExtractionRoute route, int attempted, int emitted, ExtractionTolerance tolerance, bool parallelCallback, Op key, Option<IsoSurfaceReceipt> isoSurface = default, Option<ScalarIsolineReceipt> scalarIsoline = default, Option<SampleReceipt> sample = default, Option<int> itemFailures = default) =>
        attempted < 0 || emitted < 0 || emitted > attempted
            ? Fin.Fail<ExtractionReceipt>(error: key.InvalidResult())
            : Fin.Succ(new ExtractionReceipt(Route: route, Attempted: attempted, Emitted: emitted, Tolerance: tolerance, ParallelCallback: parallelCallback, IsoSurface: isoSurface, ScalarIsoline: scalarIsoline, Sample: sample, ItemFailures: itemFailures));
}
```
