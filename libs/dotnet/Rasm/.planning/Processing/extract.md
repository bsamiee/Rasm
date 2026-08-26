# [RASM_VECTORS_EXTRACT]

`ExtractionDomain` owns the extraction/projection API: one polymorphic `Of` ingress admits raw Rhino geometry or an admitted `CellLattice` into a typed sampling domain, `ContourPolicy` sections every domain through the owner its shape names — RhinoCommon's contour and iso adapters for the multi-plane and surface-iso routes, the `Meshing/intersect` `IntersectOp.PlaneMesh` crossing table for a single mesh section, the `reconstruct.md` marching-squares owner for lattice scalar levels — and typed projection rows fold every request shape to any output type. One local marching-triangles kernel serves the per-vertex scalar contouring no owner carries.

Output dispatch rides `Numerics/atoms.md`'s `AtomProjection.Rows`; evidence validity folds through `Domain/results.md`'s `ValidityClaim` under the `Op` value key. Sampling owners compose unchanged — `sample.md` evaluates seeds, `flow.md` traces stream bundles, `Spatial/fields.md` samples the scalar, vector, and tensor fields through its tagged rows, `Processing/geodesics.md` resolves the mesh-bound log-map probe, and `Meshing/reconstruct.md` extracts the marching-cubes iso-surface — this API re-implements none of them.

## [01]-[INDEX]

- [02]-[SECTIONING]: domain ingress, admission, and owner-routed contour/iso sectioning with the local scalar-isoline kernel.
- [03]-[PROJECTION_ROW]: `Extraction` request union and its typed `Project<TOut>` egress over probe, iso-surface, and sampled modes.

## [02]-[SECTIONING]

- Owner: `ExtractionDomain` `[Union]`, whose polymorphic `Of` ingress discriminates on runtime shape and admits each arm through its own owner; `ContourPolicy` `[Union]`, whose factories admit every section policy through the `Domain/validation.md` vocabulary.
- Entry: `domain.Contours(policy, …)` is a total `Switch` routing domain then policy to its owner, and every unsupported domain-policy pairing is a typed `Unsupported` fault naming both sides, never a silent empty.
- Law: a mesh-plane section has ONE owner — `Meshing/intersect`'s `IntersectOp.PlaneMesh`, whose exact straddle signs and oriented `Chain` loops this page emits as polyline curves. Reaching RhinoCommon's single-plane contour beside it spells two answers to one question. Interval sweep (`AxisCase`) and surface iso stay native: RhinoCommon owns a multi-plane sweep and a trim-aware iso curve that no kernel owner carries, and `ExtractionRoute` records which side answered.
- Auto: the local scalar-isoline kernel triangulates, extracts per-face level crossings under the `Fraction` lane's scale-relative band, welds them through a point key quantized on the `Weld` lane, dedups, and stitches seed polylines end-to-end, recording every branch node rather than guessing through it. Its census is the `IsolineCensus` MONOID the kernels `tell` — one `Writer` run yields curves and census together, so no kernel returns-and-reassigns a census through its own signature.
- Output: `IsolineCensus` carries the full kernel evidence — segment, rejection (plateau, degenerate, and vertex-touch), stitch, and branch counts — folded to one validity claim; `CurveBatch` bundles the accepted curves with the composed `ExtractionTally`. Routing is `ExtractionTally.Route`'s business alone, so no child carries a second routing column.
- Boundary: native adapters wrap every RhinoCommon call in `Op.Catch` so a host throw converts at the boundary. Scalar-isoline extraction is the named statement-kernel exemption: no owner carries a per-vertex scalar contour, so the kernel follows Rhino's triangulated topology and returns stitched candidates only.
- Exemption: the stitch frontier stays a BCL `bool[]`/`Dictionary` walk and REFUSES QuikGraph by name — `ConnectedComponents` answers the component set where the whole product here is the polyline's vertex ORDER, and `EulerianTrailAlgorithm` demands a traversal this kernel deliberately refuses, stopping at every branch node and tallying it as `BranchStops` rather than choosing an arm. Every table dies inside the one fold that fills it.

## [03]-[PROJECTION_ROW]

- Owner: `ExtractionProbe` `[Union]` is the field point-probe; `Extraction` `[Union]` is the public request vocabulary `intent.md` wraps as one case; `SampledExtraction` `[Union]` is the one sampled-mode family over one shared seed generator; `ExtractionTolerance` `[Union]` carries provenance and value as one.
- Entry: the request factories admit once — probe source and sample gated, domain re-admitted, policy and mode validated through their own `Admit`, iso bounds gated finite and non-degenerate; `extraction.Project<TOut>(…)` is the one egress, each request kind resolving its output through typed projection rows.
- Law: every union on this page is one owner, so its factories carry the `Op? key = null` + `OrDefault()` spelling as a type-wide contract rather than a per-member attribute.
- Auto: `ProjectSamples` is the one sampled spine — evaluate the seeds, fold each through the mode's item arm, mint the tally, and project through one `Rows` call; item rows gate on zero rejections, so a partial sampled extraction is a typed fault, never a truncated success.
- Output: `ExtractionTally` carries the extraction route, attempted and emitted counts with derived rejected and completion, the tolerance carrier, one `ItemFailures` slot, and the optional child evidence, all folded to one validity claim.
- Growth: a new section policy is one `ContourPolicy` case and one adapter arm per admitting domain, a new sampled mode one `SampledExtraction` case and one spine arm, a new probe output one `ProjectionRow`, a new ingress shape one `Of` arm.
- Boundary: owner-first is law — the local kernel never shadows a route another owner carries, and the sampled projection composes the `sample.md`, `flow.md`, and `fields.md` owners rather than re-implementing any. Log-map is the probe's only mesh-band special case; a Hodge probe reads its sampled component vector here while the `HodgeWitness` rides `fields.md`'s tagged vector row.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using LanguageExt;
using LanguageExt.Traits;
using Rasm.Domain;
using Rasm.Meshing;
using Rasm.Numerics;
using Rasm.Spatial;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;
using SegmentKeySet = System.Collections.Generic.HashSet<(ScalarIsolinePointKey A, ScalarIsolinePointKey B)>;
using Dimension = Rasm.Numerics.Dimension;

namespace Rasm.Processing;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class ExtractionRoute {
    public static readonly ExtractionRoute Native = new(key: 0, failures: static (attempted, emitted) => Some(attempted - emitted));
    public static readonly ExtractionRoute Local = new(key: 1, failures: static (_, _) => Option<int>.None);

    [UseDelegateFromConstructor]
    public partial Option<int> Failures(int attempted, int emitted);
}

[SmartEnum<int>]
public sealed partial class ChainEnd {
    public static readonly ChainEnd Head = new(key: 0, anchor: static points => points[index: 0], slot: static _ => 0);
    public static readonly ChainEnd Tail = new(key: 1, anchor: static points => points[^1], slot: static points => points.Count);

    [UseDelegateFromConstructor] public partial Point3d Anchor(List<Point3d> points);
    [UseDelegateFromConstructor] public partial int Slot(List<Point3d> points);
}

[Union]
public abstract partial record ExtractionTolerance {
    public sealed record FromContextCase(Tolerance Value) : ExtractionTolerance;
    public sealed record RhinoDefaultCase(Option<double> Witnessed) : ExtractionTolerance;
    public sealed record NotApplicableCase : ExtractionTolerance;
    private ExtractionTolerance() { }
    public static ExtractionTolerance FromContext(Tolerance value) => new FromContextCase(Value: value);
    public static ExtractionTolerance RhinoFixed(double witnessed) => new RhinoDefaultCase(Witnessed: Some(witnessed));
    public static readonly ExtractionTolerance RhinoDefault = new RhinoDefaultCase(Witnessed: Option<double>.None);
    public static readonly ExtractionTolerance NotApplicable = new NotApplicableCase();
    public Option<Tolerance> Band => Switch(
        fromContextCase: static c => Some(c.Value),
        rhinoDefaultCase: static _ => Option<Tolerance>.None,
        notApplicableCase: static _ => Option<Tolerance>.None);
    public Option<double> Value => Switch(
        fromContextCase: static c => Some(c.Value.Value),
        rhinoDefaultCase: static r => r.Witnessed,
        notApplicableCase: static _ => Option<double>.None);
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
    internal Fin<ContourPolicy> Admit(Op key) => Switch(
        state: key,
        planeCase: static (op, policy) => Rasm.Domain.Admit.Plane(basis: policy.Section, key: op).Map(_ => (ContourPolicy)policy),
        axisCase: static (op, policy) =>
            from _ in Rasm.Domain.Admit.AllFinite(op, policy.Start, policy.End)
            from __ in guard((policy.End - policy.Start).Length > 0.0, op.InvalidInput())
            select (ContourPolicy)policy,
        surfaceIsoCase: static (op, policy) => (policy.Status, policy.Parameter) switch {
            (IsoStatus.X or IsoStatus.Y, double parameter) when double.IsFinite(parameter) => Fin.Succ<ContourPolicy>(policy),
            (IsoStatus.North or IsoStatus.East or IsoStatus.South or IsoStatus.West, _) => Fin.Succ<ContourPolicy>(policy),
            _ => Fin.Fail<ContourPolicy>(op.InvalidInput()),
        },
        meshScalarCase: static (op, policy) =>
            from scalars in Rasm.Domain.Admit.All(toSeq(policy.Values.AsIterable()), static value => ValidityClaim.Finite(value: value), floor: 1, key: op)
            from levels in Rasm.Domain.Admit.All(policy.Levels, static value => ValidityClaim.Finite(value: value), floor: 1, key: op)
            select (ContourPolicy)policy);
}

[Union]
public abstract partial record ExtractionDomain {
    public sealed record SupportCase : ExtractionDomain { internal SupportCase(SupportSpace value) => Value = value; public SupportSpace Value { get; } }
    public sealed record MeshCase : ExtractionDomain { internal MeshCase(MeshSpace value) => Value = value; public MeshSpace Value { get; } }
    public sealed record CloudCase : ExtractionDomain { internal CloudCase(VectorCloud value) => Value = value; public VectorCloud Value { get; } }
    public sealed record LatticeCase : ExtractionDomain { internal LatticeCase(CellLattice value) => Value = value; public CellLattice Value { get; } }
    private ExtractionDomain() { }
    public static Fin<ExtractionDomain> Support(SupportSpace value, Op? key = null) =>
        Optional(value).ToFin(key.OrDefault().InvalidInput()).Map(valid => (ExtractionDomain)new SupportCase(value: valid));
    public static Fin<ExtractionDomain> Mesh(MeshSpace value, Op? key = null) =>
        key.OrDefault().Need(value.Native).Map(_ => (ExtractionDomain)new MeshCase(value: value));
    public static Fin<ExtractionDomain> Cloud(VectorCloud value, Op? key = null) {
        Op op = key.OrDefault();
        return op.Need(value)
            .Bind(cloud => cloud.Admit(key: op))
            .Map(static valid => (ExtractionDomain)new CloudCase(value: valid));
    }
    public static Fin<ExtractionDomain> Lattice(CellLattice value, Op? key = null) =>
        key.OrDefault().AcceptValue(value: (ExtractionDomain)new LatticeCase(value: value));
    public static Fin<ExtractionDomain> Of(object? value, Context context, Op? key = null) {
        Op op = key.OrDefault();
        return Optional(value).ToFin(op.InvalidInput()).Bind(source => source switch {
            ExtractionDomain domain => domain.Admit(key: op),
            Mesh mesh => MeshSpace.Of(native: mesh, context: context, key: op).Bind(space => Mesh(value: space, key: op)),
            VectorCloud cloud => Cloud(value: cloud, key: op),
            PointCloud cloud => VectorCloud.Cluster(points: toSeq(cloud.GetPoints()), context: context, key: op).Bind(active => Cloud(value: active, key: op)),
            CellLattice lattice => Lattice(value: lattice, key: op),
            object candidate => SupportSpace.Of(value: candidate, key: op).Bind(space => Support(value: space, key: op)),
        });
    }
    internal Fin<ExtractionDomain> Admit(Op key) => Switch(
        state: key,
        supportCase: static (op, domain) => Support(value: domain.Value, key: op),
        meshCase: static (op, domain) => Mesh(value: domain.Value, key: op),
        cloudCase: static (op, domain) => Cloud(value: domain.Value, key: op),
        latticeCase: static (op, domain) => Lattice(value: domain.Value, key: op));

    internal Fin<CurveBatch> Contours(ContourPolicy policy, Context context, Op key) => Switch(
        state: (Policy: policy, Context: context, Key: key),
        supportCase: static (state, domain) => domain.Value.Value switch {
            Brep brep => CurvesFromBrep(brep: brep, policy: state.Policy, key: state.Key),
            Mesh mesh => MeshSpace.Of(native: mesh, context: state.Context, key: state.Key)
                .Bind(space => CurvesFromMesh(space: space, policy: state.Policy, key: state.Key)),
            Surface surface => CurvesFromSurface(surface: surface, policy: state.Policy, key: state.Key),
            VectorCloud.ClusterCase cloud => CurvesFromCloud(cloud: cloud, policy: state.Policy, context: state.Context, key: state.Key),
            _ => Fin.Fail<CurveBatch>(error: state.Key.Unsupported(inputType: domain.Value.SourceType, outputType: typeof(Seq<Curve>))),
        },
        meshCase: static (state, domain) => CurvesFromMesh(space: domain.Value, policy: state.Policy, key: state.Key),
        cloudCase: static (state, domain) => domain.Value is VectorCloud.ClusterCase cloud
            ? CurvesFromCloud(cloud: cloud, policy: state.Policy, context: state.Context, key: state.Key)
            : Fin.Fail<CurveBatch>(error: state.Key.Unsupported(inputType: domain.Value.GetType(), outputType: typeof(Seq<Curve>))),
        latticeCase: static (state, domain) => CurvesFromLattice(grid: domain.Value, policy: state.Policy, context: state.Context, key: state.Key));

    private static Fin<CurveBatch> CurvesFromBrep(Brep brep, ContourPolicy policy, Op key) =>
        key.Catch(() => policy.Switch(
            state: (Brep: brep, Key: key),
            planeCase: static (state, p) => AcceptNative(curves: Brep.CreateContourCurves(brepToContour: state.Brep, sectionPlane: p.Section), tolerance: ExtractionTolerance.RhinoDefault, key: state.Key),
            axisCase: static (state, p) => AcceptNative(curves: Brep.CreateContourCurves(brepToContour: state.Brep, contourStart: p.Start, contourEnd: p.End, interval: p.Interval.Value), tolerance: ExtractionTolerance.RhinoDefault, key: state.Key),
            surfaceIsoCase: static (state, _) => Fin.Fail<CurveBatch>(error: state.Key.Unsupported(inputType: typeof(Brep), outputType: typeof(ContourPolicy.SurfaceIsoCase))),
            meshScalarCase: static (state, _) => Fin.Fail<CurveBatch>(error: state.Key.Unsupported(inputType: typeof(Brep), outputType: typeof(ContourPolicy.MeshScalarCase)))));
    private static Fin<CurveBatch> CurvesFromMesh(MeshSpace space, ContourPolicy policy, Op key) =>
        key.Catch(() => policy.Switch(
            state: (Space: space, Key: key),
            planeCase: static (state, p) =>
                Intersection.Apply(new IntersectOp.PlaneMesh(Cut: p.Section, Mesh: state.Space, Policy: IntersectPolicy.Canonical), state.Key)
                    .Bind(result => result is IntersectResult.Chains chains
                        ? AcceptCurves(curves: chains.Walked.Map(static chain => (Curve)chain.Points.ToPolylineCurve()), attempted: chains.Walked.Count,
                            route: ExtractionRoute.Local, tolerance: ExtractionTolerance.FromContext(state.Space.Tolerance.Absolute), key: state.Key)
                        : Fin.Fail<CurveBatch>(state.Key.InvalidResult())),
            axisCase: static (state, p) => AcceptNative(curves: Rhino.Geometry.Mesh.CreateContourCurves(meshToContour: state.Space.Native, contourStart: p.Start, contourEnd: p.End, interval: p.Interval.Value, tolerance: state.Space.Tolerance.Absolute.Value), tolerance: ExtractionTolerance.FromContext(state.Space.Tolerance.Absolute), key: state.Key),
            meshScalarCase: static (state, p) => ScalarIsolinesDetailed(mesh: state.Space.Native, values: p.Values, levels: p.Levels, context: state.Space.Tolerance, key: state.Key)
                .Bind(result => AcceptCurves(curves: result.Curves, attempted: result.Census.StitchedCandidates, route: ExtractionRoute.Local, tolerance: ExtractionTolerance.FromContext(state.Space.Tolerance.For(ToleranceLane.Weld)), scalarIsoline: Some(result), key: state.Key)),
            surfaceIsoCase: static (state, _) => Fin.Fail<CurveBatch>(error: state.Key.Unsupported(inputType: typeof(Mesh), outputType: typeof(ContourPolicy.SurfaceIsoCase)))));
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
    private static Fin<CurveBatch> CurvesFromLattice(CellLattice grid, ContourPolicy policy, Context context, Op key) =>
        key.Catch(() => policy.Switch(
            state: (Grid: grid, Context: context, Key: key),
            meshScalarCase: static (state, p) => LatticeIsolines(grid: state.Grid, values: p.Values, levels: p.Levels, context: state.Context, key: state.Key),
            planeCase: static (state, _) => Fin.Fail<CurveBatch>(error: state.Key.Unsupported(inputType: typeof(CellLattice), outputType: typeof(ContourPolicy.PlaneCase))),
            axisCase: static (state, _) => Fin.Fail<CurveBatch>(error: state.Key.Unsupported(inputType: typeof(CellLattice), outputType: typeof(ContourPolicy.AxisCase))),
            surfaceIsoCase: static (state, _) => Fin.Fail<CurveBatch>(error: state.Key.Unsupported(inputType: typeof(CellLattice), outputType: typeof(ContourPolicy.SurfaceIsoCase)))));
    private static Fin<CurveBatch> LatticeIsolines(CellLattice grid, Arr<double> values, Seq<double> levels, Context context, Op key) =>
        from field in ScalarField.Lattice(grid: grid, values: values, key: key)
        from results in levels.TraverseM(level => IsoContour.Detailed(field: field, grid: grid, policy: new IsoContourPolicy(IsoValue: level), context: context, key: key)).As()
        from batch in AcceptCurves(
            curves: results.Bind(static result => result.Loops.Map(static chain => (Curve)chain.Points.ToPolylineCurve())),
            attempted: results.Sum(static result => result.Loops.Count),
            route: ExtractionRoute.Local, tolerance: ExtractionTolerance.FromContext(context.Absolute), key: key)
        select batch;
    private static Fin<CurveBatch> CurvesFromCloud(VectorCloud.ClusterCase cloud, ContourPolicy policy, Context context, Op key) =>
        key.Catch(() => policy.Switch(
            state: (Cloud: cloud, Context: context, Key: key),
            axisCase: static (state, p) => state.Cloud.UseIndex(key: state.Key, project: pc => AcceptNative(curves: pc.CreateContourCurves(contourStart: p.Start, contourEnd: p.End, interval: p.Interval.Value, absoluteTolerance: state.Context.Absolute.Value), tolerance: ExtractionTolerance.FromContext(state.Context.Absolute), key: state.Key)),
            planeCase: static (state, p) => state.Cloud.UseIndex(key: state.Key, project: pc => AcceptNative(curves: pc.CreateSectionCurve(plane: p.Section, absoluteTolerance: state.Context.Absolute.Value), tolerance: ExtractionTolerance.FromContext(state.Context.Absolute), key: state.Key)),
            surfaceIsoCase: static (state, _) => Fin.Fail<CurveBatch>(error: state.Key.Unsupported(inputType: typeof(PointCloud), outputType: typeof(ContourPolicy.SurfaceIsoCase))),
            meshScalarCase: static (state, _) => Fin.Fail<CurveBatch>(error: state.Key.Unsupported(inputType: typeof(PointCloud), outputType: typeof(ContourPolicy.MeshScalarCase)))));
    private static Fin<CurveBatch> AcceptNative(Curve[] curves, ExtractionTolerance tolerance, Op key) =>
        Optional(curves).ToFin(key.InvalidResult())
            .Bind(active => AcceptCurves(curves: toSeq(active), attempted: active.Length, route: ExtractionRoute.Native, tolerance: tolerance, key: key));
    private static Fin<CurveBatch> AcceptCurves(Seq<Curve> curves, int attempted, ExtractionRoute route, ExtractionTolerance tolerance, Op key, Option<ScalarIsolineResult> scalarIsoline = default) {
        Seq<Curve> accepted = curves.Filter(static curve => curve is not null && curve.IsValid);
        return ExtractionTally.Of(
                route: route, attempted: attempted, emitted: accepted.Count, tolerance: tolerance, key: key,
                scalarIsoline: scalarIsoline.Map(static result => result.Census),
                itemFailures: route.Failures(attempted: attempted, emitted: accepted.Count))
            .Map(tally => new CurveBatch(Curves: accepted, ScalarIsoline: scalarIsoline, Tally: tally));
    }

    private static Fin<ScalarIsolineResult> ScalarIsolinesDetailed(Mesh mesh, Arr<double> values, Seq<double> levels, Context context, Op key) {
        if (values.Count != mesh.Vertices.Count || values.Exists(static value => !double.IsFinite(value)) || levels.IsEmpty || levels.Exists(static value => !double.IsFinite(value)))
            return Fin.Fail<ScalarIsolineResult>(key.InvalidInput());
        using Mesh triangulated = mesh.DuplicateMesh();
        if (triangulated.Faces.QuadCount > 0 && !triangulated.Faces.ConvertQuadsToTriangles()) return Fin.Fail<ScalarIsolineResult>(key.InvalidResult());
        if (triangulated.Vertices.Count != values.Count) return Fin.Fail<ScalarIsolineResult>(key.InvalidResult());
        Tolerance band = context.For(ToleranceLane.Fraction);
        Tolerance weld = context.For(ToleranceLane.Weld);
        List<ScalarIsolineSegment> segments = [];
        Writer<IsolineCensus, Seq<Curve>> run =
            from _ in toSeq(Enumerable.Range(start: 0, count: triangulated.Faces.Count))
                .Filter(f => triangulated.Faces[index: f].IsTriangle)
                .Fold(Writer.pure<IsolineCensus, Unit>(unit),
                    (acc, f) => acc.Bind(__ => AddFaceIsolines(mesh: triangulated, face: triangulated.Faces[index: f], values: values, levels: levels, band: band, weld: weld, segments: segments)))
            from deduped in DeduplicateSegments(segments: segments, weld: weld)
            from curves in StitchSegments(segments: deduped, weld: weld)
            select curves;
        (Seq<Curve> emitted, IsolineCensus census) = run.Run();
        return Fin.Succ(new ScalarIsolineResult(Curves: emitted, Census: census with { FiniteLevels = levels.Count }));
    }
    private static Writer<IsolineCensus, Unit> AddFaceIsolines(Mesh mesh, MeshFace face, Arr<double> values, Seq<double> levels, Tolerance band, Tolerance weld, List<ScalarIsolineSegment> segments) {
        Point3d[] points = [mesh.Vertices[index: face.A], mesh.Vertices[index: face.B], mesh.Vertices[index: face.C]];
        double[] scalars = [values[index: face.A], values[index: face.B], values[index: face.C]];
        (int A, int B)[] edges = [(0, 1), (1, 2), (2, 0)];
        return levels.Fold(Writer.pure<IsolineCensus, Unit>(unit), (acc, level) => acc.Bind(_ => {
            double epsilon = band.Value * Math.Max(1.0, Math.Max(Math.Abs(value: level), scalars.Max(static value => Math.Abs(value: value))));
            if (scalars.All(value => Math.Abs(value: value - level) <= epsilon)) { return Writer.tell(IsolineCensus.Plateau); }
            ((int A, int B) Edge, double ADelta, double BDelta)[] cuts = System.Array.ConvertAll(array: edges, converter: edge => (edge, scalars[edge.A] - level, scalars[edge.B] - level));
            ScalarIsolineSegment[] edgeSegments = System.Array.ConvertAll(
                array: System.Array.FindAll(array: cuts, match: cut => Math.Abs(value: cut.ADelta) <= epsilon && Math.Abs(value: cut.BDelta) <= epsilon),
                converter: cut => new ScalarIsolineSegment(A: points[cut.Edge.A], B: points[cut.Edge.B]));
            segments.AddRange(collection: edgeSegments);
            Point3d[] unique = [.. cuts.SelectMany(cut =>
                    (Math.Abs(value: cut.ADelta) <= epsilon, Math.Abs(value: cut.BDelta) <= epsilon, Sign.Of(cut.ADelta) != Sign.Of(cut.BDelta)) switch {
                        (true, true, _) => (Point3d[])[],
                        (true, false, _) => [points[cut.Edge.A]],
                        (false, true, _) => [points[cut.Edge.B]],
                        (false, false, true) => [points[cut.Edge.A] + ((-cut.ADelta / (cut.BDelta - cut.ADelta)) * (points[cut.Edge.B] - points[cut.Edge.A]))],
                        _ => [],
                    })
                .Where(predicate: static point => point.IsValid)
                .DistinctBy(keySelector: point => KeyOf(point: point, weld: weld))];
            return unique.Length == 2
                ? Writer.tell(IsolineCensus.Raw(edgeSegments.Length + 1).Combine(Emit(segments, unique)))
                : Writer.tell(IsolineCensus.Raw(edgeSegments.Length).Combine(IsolineCensus.VertexTouch));
        }));

        static IsolineCensus Emit(List<ScalarIsolineSegment> sink, Point3d[] unique) {
            sink.Add(item: new ScalarIsolineSegment(A: unique[0], B: unique[1]));
            return IsolineCensus.Empty;
        }
    }
    private static Writer<IsolineCensus, Seq<ScalarIsolineSegment>> DeduplicateSegments(List<ScalarIsolineSegment> segments, Tolerance weld) {
        SegmentKeySet seen = [];
        List<ScalarIsolineSegment> unique = [];
        int degenerate = 0;
        foreach (ScalarIsolineSegment segment in segments) {
            ScalarIsolinePointKey a = KeyOf(point: segment.A, weld: weld);
            ScalarIsolinePointKey b = KeyOf(point: segment.B, weld: weld);
            if (a.Equals(b)) { degenerate++; continue; }
            (ScalarIsolinePointKey A, ScalarIsolinePointKey B) edge = a.Compare(other: b) <= 0 ? (a, b) : (b, a);
            if (seen.Add(item: edge)) unique.Add(item: segment);
        }
        return Writer.write(toSeq(unique), IsolineCensus.Empty with { DegenerateRejected = degenerate, DedupedSegments = unique.Count });
    }
    private static Writer<IsolineCensus, Seq<Curve>> StitchSegments(Seq<ScalarIsolineSegment> segments, Tolerance weld) {
        ScalarIsolineSegment[] all = [.. segments.AsIterable()];
        bool[] used = new bool[all.Length];
        Dictionary<ScalarIsolinePointKey, List<int>> incident = [];
        for (int i = 0; i < all.Length; i++) {
            ref List<int>? a = ref CollectionsMarshal.GetValueRefOrAddDefault(dictionary: incident, key: KeyOf(point: all[i].A, weld: weld), exists: out _);
            ref List<int>? b = ref CollectionsMarshal.GetValueRefOrAddDefault(dictionary: incident, key: KeyOf(point: all[i].B, weld: weld), exists: out _);
            (a ??= []).Add(item: i);
            (b ??= []).Add(item: i);
        }
        (int maxIncident, int branchNodes) = incident.Values.Select(static edges => edges.Count).Aggregate(
            seed: (Max: 0, Branches: 0),
            func: static (state, count) => (Math.Max(val1: state.Max, val2: count), state.Branches + (count > 2 ? 1 : 0)));
        IsolineCensus ledger = IsolineCensus.Branched(branchNodes: branchNodes, maxIncident: maxIncident);
        List<Curve> curves = [];
        int attempted = 0;
        for (int i = 0; i < all.Length; i++) {
            if (used[i]) continue;
            List<Point3d> points = [all[i].A, all[i].B];
            used[i] = true;
            ledger = toSeq(ChainEnd.Items).Fold(ledger, (held, end) => held.Combine(Extend(points: points, end: end, all: all, used: used, incident: incident, weld: weld)));
            Polyline polyline = [.. points];
            attempted++;
            if (polyline.IsValid && polyline.Count >= 2) curves.Add(item: polyline.ToPolylineCurve());
        }
        return Writer.write(toSeq(curves), ledger.Combine(IsolineCensus.Stitched(attempted: attempted, emitted: curves.Count)));
    }
    private static IsolineCensus Extend(List<Point3d> points, ChainEnd end, ScalarIsolineSegment[] all, bool[] used, Dictionary<ScalarIsolinePointKey, List<int>> incident, Tolerance weld) {
        IsolineCensus ledger = IsolineCensus.Empty;
        while (true) {
            ScalarIsolinePointKey at = KeyOf(point: end.Anchor(points), weld: weld);
            if (!incident.TryGetValue(key: at, value: out List<int>? candidates)) { return ledger; }
            Seq<int> open = toSeq(candidates).Filter(candidate => !used[candidate]);
            if (open.Count > 1) { return ledger.Combine(IsolineCensus.BranchStop); }
            if (open.Case is not int index) { return ledger; }
            ScalarIsolineSegment segment = all[index];
            points.Insert(index: end.Slot(points), item: KeyOf(point: segment.A, weld: weld).Equals(at) ? segment.B : segment.A);
            used[index] = true;
        }
    }
    private static ScalarIsolinePointKey KeyOf(Point3d point, Tolerance weld) {
        double scale = 1.0 / Math.Max(val1: weld.Value, val2: EpsilonPolicy.SqrtEpsilon);
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
    internal Fin<ExtractionProbe> Admit(Op key) => Switch(
        state: key,
        vectorCase: static (op, probe) => op.Need(probe.Source).Map(_ => (ExtractionProbe)probe),
        scalarCase: static (op, probe) => op.Need(probe.Source).Map(_ => (ExtractionProbe)probe),
        tensorCase: static (op, probe) => op.Need(probe.Source).Map(_ => (ExtractionProbe)probe));
    internal Fin<TOut> Project<TOut>(Point3d sample, Context context, Op key) => Switch(
        state: (Sample: sample, Context: context, Key: key),
        vectorCase: static (state, probe) => AtomProjection.Rows<ExtractionProbe.VectorCase, TOut>(self: probe, key: state.Key, owner: typeof(VectorCase),
            ProjectionRow.Of<TangentLogMapResult>(() => probe.Source is VectorField.TangentLogMapCase log
                ? GeodesicKernel.TangentLogMapAt(space: log.Space, source: log.Source, sample: state.Sample, time: log.Time.Value, algorithm: log.Algorithm, trace: log.Trace, windows: log.Windows, key: state.Key)
                : Fin.Fail<TangentLogMapResult>(state.Key.Unsupported(inputType: probe.Source.GetType(), outputType: typeof(TangentLogMapResult)))),
            ProjectionRow.Of<LogMapTrace>(() => probe.Source is VectorField.TangentLogMapCase log
                ? GeodesicKernel.TangentLogMapAt(space: log.Space, source: log.Source, sample: state.Sample, time: log.Time.Value, algorithm: log.Algorithm, trace: log.Trace, windows: log.Windows, key: state.Key).Map(static result => result.Trace)
                : Fin.Fail<LogMapTrace>(state.Key.Unsupported(inputType: probe.Source.GetType(), outputType: typeof(LogMapTrace)))),
            ProjectionRow.Of<HodgeWitness>(() => probe.Source is VectorField.HodgeCase hodge
                ? DecAssembly.HodgeSolutionOf(source: hodge.Source, space: hodge.Space, context: state.Context, key: state.Key).Map(static solved => solved.Witness)
                : Fin.Fail<HodgeWitness>(state.Key.Unsupported(inputType: probe.Source.GetType(), outputType: typeof(HodgeWitness)))),
            ProjectionRow.Of<HarmonicCensus>(() => probe.Source is VectorField.HodgeCase hodge
                ? DecAssembly.HodgeSolutionOf(source: hodge.Source, space: hodge.Space, context: state.Context, key: state.Key).Bind(solved => solved.Witness.Harmonic.ToFin(state.Key.InvalidResult()))
                : Fin.Fail<HarmonicCensus>(state.Key.Unsupported(inputType: probe.Source.GetType(), outputType: typeof(HarmonicCensus)))),
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

[Union]
public abstract partial record SampledExtraction {
    public sealed record GlyphCase(VectorField Field, PositiveMagnitude Scale) : SampledExtraction;
    public sealed record GridCase(ScalarField Field) : SampledExtraction;
    public sealed record StreamBundleCase(VectorField Field, PositiveMagnitude InitialStep, FieldIntegrator Integrator, Termination Termination) : SampledExtraction;
    public sealed record DrapeCase(Vector3d Direction) : SampledExtraction;
    private SampledExtraction() { }
    public static Fin<SampledExtraction> Glyph(VectorField field, double scale, Op? key = null) {
        Op op = key.OrDefault();
        return from source in Admit.NotNull(value: field, key: op)
               from magnitude in op.AcceptValidated<PositiveMagnitude>(candidate: scale)
               select (SampledExtraction)new GlyphCase(Field: source, Scale: magnitude);
    }
    public static Fin<SampledExtraction> Grid(ScalarField field, Op? key = null) =>
        Admit.NotNull(value: field, key: key.OrDefault()).Map(static source => (SampledExtraction)new GridCase(Field: source));
    public static Fin<SampledExtraction> Drape(Vector3d direction, Op? key = null) {
        Op op = key.OrDefault();
        return guard(ValidityClaim.Finite(direction) && direction.Length > 0.0, op.InvalidInput()).ToFin()
            .Map(_ => (SampledExtraction)new DrapeCase(Direction: direction));
    }
    public static Fin<SampledExtraction> StreamBundle(VectorField field, double initialStep, Termination termination, Option<FieldIntegrator> integrator = default, Op? key = null) {
        Op op = key.OrDefault();
        return from source in Admit.NotNull(value: field, key: op)
               from step in op.AcceptValidated<PositiveMagnitude>(candidate: initialStep)
               from stop in Termination.Admit(value: termination, key: op)
               from active in FieldIntegrator.AdmitOrFixed(value: integrator, key: op)
               select (SampledExtraction)new StreamBundleCase(Field: source, InitialStep: step, Integrator: active, Termination: stop);
    }
}

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
            from output in AtomProjection.Rows<ExtractionTally, TOut>(self: batch.Tally, key: state.Key, owner: typeof(ContourCase),
                ProjectionRow.Of<Seq<Curve>>(() => Fin.Succ(batch.Curves)),
                ProjectionRow.Of<ScalarIsolineResult>(() => batch.ScalarIsoline.ToFin(Fail: state.Key.Unsupported(inputType: typeof(ContourPolicy), outputType: typeof(ScalarIsolineResult)))),
                ProjectionRow.Of<IsolineCensus>(() => batch.ScalarIsoline.Map(static result => result.Census).ToFin(Fail: state.Key.Unsupported(inputType: typeof(ContourPolicy), outputType: typeof(IsolineCensus)))))
            select output,
        isoSurfaceCase: static (state, extraction) =>
            from result in IsoSurface.Detailed(field: extraction.Field, bounds: extraction.Bounds, resolution: extraction.Resolution.Value,
                policy: IsoSurfacePolicy.Default with { MaxRootSteps = extraction.MaxRootSteps }, context: state.Context, key: state.Key)
            from output in AtomProjection.Rows<IsoSurfaceRun, TOut>(self: result.Run, key: state.Key, owner: typeof(IsoSurfaceCase),
                ProjectionRow.Of<Mesh>(() => result.Run.Valid ? Fin.Succ(result.Mesh) : Fin.Fail<Mesh>(state.Key.InvalidResult())),
                ProjectionRow.Of<IsoSurfaceResult>(() => result.Run.Valid ? Fin.Succ(result) : Fin.Fail<IsoSurfaceResult>(state.Key.InvalidResult())),
                ProjectionRow.Of<ExtractionTally>(() => ExtractionTally.Of(
                    route: result.Run.NativeRouted ? ExtractionRoute.Native : ExtractionRoute.Local,
                    attempted: 1, emitted: result.Run.Valid ? 1 : 0,
                    tolerance: result.Run.FixedTolerance.Map(ExtractionTolerance.RhinoFixed).IfNone(ExtractionTolerance.RhinoDefault),
                    key: state.Key,
                    isoSurface: Some(result.Run), itemFailures: result.Run.Valid ? Option<int>.None : Some(1))))
            select output,
        sampledCase: static (state, extraction) => extraction.Mode.Switch(
            state: (Domain: extraction.Domain, Seeds: extraction.Seeds, Context: state.Context, Key: state.Key),
            glyphCase: static (s, mode) => ProjectSamples<TOut, Line>(
                seeds: s.Seeds, domain: s.Domain, context: s.Context, key: s.Key,
                sample: (point, model, op) => ExtractionProbe.Vector(source: mode.Field).Project<VectorSpan>(sample: point, context: model, key: op)
                    .Map(span => new Line(span.Anchor, span.Anchor + (mode.Scale.Value * span.Value))),
                project: static (glyphs, rejected, tally, op) => AtomProjection.Rows<ExtractionTally, TOut>(self: tally, key: op, owner: typeof(SampledExtraction.GlyphCase),
                    Gated<Seq<Line>>(rejected, op, () => Fin.Succ(glyphs)))),
            gridCase: static (s, mode) => ProjectSamples<TOut, (Point3d Point, double Value)>(
                seeds: s.Seeds, domain: s.Domain, context: s.Context, key: s.Key,
                sample: (point, model, op) => mode.Field.SampleScalar(sample: point, context: model, key: op).Map(value => (Point: point, Value: value)),
                project: static (samples, rejected, tally, op) => AtomProjection.Rows<ExtractionTally, TOut>(self: tally, key: op, owner: typeof(SampledExtraction.GridCase),
                    Gated<Seq<(Point3d Point, double Value)>>(rejected, op, () => samples.ForAll(static sample => ValidityClaim.Finite(sample.Point) && ValidityClaim.Finite(value: sample.Value))
                        ? Fin.Succ(samples)
                        : Fin.Fail<Seq<(Point3d Point, double Value)>>(op.InvalidResult())))),
            streamBundleCase: static (s, mode) => ProjectSamples<TOut, StreamlineTrace>(
                seeds: s.Seeds, domain: s.Domain, context: s.Context, key: s.Key,
                sample: (seed, model, op) => FlowKernel.Trace<StreamlineTrace>(source: mode.Field, seed: seed, initialStep: mode.InitialStep, integrator: mode.Integrator, termination: mode.Termination, context: model, key: op),
                project: static (traces, rejected, tally, op) => AtomProjection.Rows<ExtractionTally, TOut>(self: tally, key: op, owner: typeof(SampledExtraction.StreamBundleCase),
                    Streamed<StreamlineTrace>(rejected, op, traces),
                    Streamed<Polyline>(rejected, op, traces),
                    Streamed<Curve>(rejected, op, traces))),
            drapeCase: static (s, mode) => s.Domain is ExtractionDomain.MeshCase meshDomain
                ? from samples in s.Seeds.Evaluate(domain: s.Domain, context: s.Context, key: s.Key)
                  from direction in Direction.Of(value: mode.Direction, context: s.Context, key: s.Key)
                  from hits in s.Key.Catch(() => {
                      Point3d[] projected = Rhino.Geometry.Intersect.Intersection.ProjectPointsToMeshesEx(
                          meshes: [meshDomain.Value.Native], points: samples.Points, direction: direction.Value,
                          tolerance: s.Context.Absolute.Value, indices: out int[] indices);
                      return s.Key.AcceptValue(value: (Projected: projected, Indices: indices));
                  })
                  let covered = toSeq(hits.Indices).Distinct().Count
                  from tally in ExtractionTally.Of(
                      route: ExtractionRoute.Native, attempted: samples.Points.Count, emitted: hits.Projected.Length,
                      tolerance: ExtractionTolerance.FromContext(s.Context.Absolute), key: s.Key,
                      sample: Some(samples.Tally), itemFailures: Some(samples.Points.Count - covered))
                  from output in AtomProjection.Rows<ExtractionTally, TOut>(self: tally, key: s.Key, owner: typeof(SampledExtraction.DrapeCase),
                      ProjectionRow.Of<Seq<Point3d>>(() => Fin.Succ(toSeq(hits.Projected))),
                      ProjectionRow.Of<Seq<(int Source, Point3d Point)>>(() => Fin.Succ(toSeq(hits.Indices.Zip(hits.Projected, static (source, point) => (Source: source, Point: point))))))
                  select output
                : Fin.Fail<TOut>(s.Key.Unsupported(inputType: s.Domain.GetType(), outputType: typeof(SampledExtraction.DrapeCase)))));

    private static ProjectionRow Gated<T>(int rejected, Op op, Func<Fin<T>> body) =>
        ProjectionRow.Of<T>(() => rejected == 0 ? body() : Fin.Fail<T>(op.InvalidResult()));

    private static ProjectionRow Streamed<TShape>(int rejected, Op op, Seq<StreamlineTrace> traces) =>
        Gated<Seq<TShape>>(rejected, op, () => traces.TraverseM(trace => FlowKernel.ProjectTrace<TShape>(trace: trace, key: op)).As());

    private static Fin<TOut> ProjectSamples<TOut, TItem>(SampleKind seeds, ExtractionDomain domain, Context context, Op key, Func<Point3d, Context, Op, Fin<TItem>> sample, Func<Seq<TItem>, int, ExtractionTally, Op, Fin<TOut>> project) =>
        from samples in seeds.Evaluate(domain: domain, context: context, key: key)
        let split = samples.Points.Map(point => sample(point, context, key)).Partition()
        from tally in ExtractionTally.Of(
            route: ExtractionRoute.Local, attempted: samples.Points.Count, emitted: split.Succs.Count,
            tolerance: ExtractionTolerance.NotApplicable, key: key,
            sample: Some(samples.Tally), itemFailures: Some(split.Fails.Count))
        from output in project(split.Succs, split.Fails.Count, tally, key)
        select output;
}

// --- [MODELS] --------------------------------------------------------------------------
internal readonly record struct CurveBatch(Seq<Curve> Curves, Option<ScalarIsolineResult> ScalarIsoline, ExtractionTally Tally);

[StructLayout(LayoutKind.Auto)]
internal readonly record struct ScalarIsolinePointKey(long X, long Y, long Z) {
    internal int Compare(ScalarIsolinePointKey other) => (X, Y, Z).CompareTo((other.X, other.Y, other.Z));
}

[StructLayout(LayoutKind.Auto)]
internal readonly record struct ScalarIsolineSegment(Point3d A, Point3d B);

[StructLayout(LayoutKind.Auto)]
public readonly record struct IsolineCensus(
    int FiniteLevels, int RawSegments, int DedupedSegments, int DegenerateRejected, int PlateauRejected, int VertexTouchRejected,
    int StitchedCandidates, int BranchStops, int BranchNodes, int MaxIncidentSegments, int EmittedCurves)
    : IValidityEvidence, Monoid<IsolineCensus> {
    public static IsolineCensus Empty => default;

    public IsolineCensus Combine(IsolineCensus rhs) =>
        new(FiniteLevels + rhs.FiniteLevels, RawSegments + rhs.RawSegments, DedupedSegments + rhs.DedupedSegments,
            DegenerateRejected + rhs.DegenerateRejected, PlateauRejected + rhs.PlateauRejected,
            VertexTouchRejected + rhs.VertexTouchRejected, StitchedCandidates + rhs.StitchedCandidates,
            BranchStops + rhs.BranchStops, BranchNodes + rhs.BranchNodes,
            Math.Max(val1: MaxIncidentSegments, val2: rhs.MaxIncidentSegments), EmittedCurves + rhs.EmittedCurves);

    internal static IsolineCensus Raw(int count) => Empty with { RawSegments = count };
    internal static IsolineCensus Plateau => Empty with { PlateauRejected = 1 };
    internal static IsolineCensus VertexTouch => Empty with { VertexTouchRejected = 1 };
    internal static IsolineCensus BranchStop => Empty with { BranchStops = 1 };
    internal static IsolineCensus Branched(int branchNodes, int maxIncident) =>
        Empty with { BranchNodes = branchNodes, MaxIncidentSegments = maxIncident };
    internal static IsolineCensus Stitched(int attempted, int emitted) =>
        Empty with { StitchedCandidates = attempted, EmittedCurves = emitted };

    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(FiniteLevels, 1), ValidityClaim.CountAtLeast(RawSegments, 0),
        ValidityClaim.CountAtLeast(DedupedSegments, 0), ValidityClaim.CountAtLeast(DegenerateRejected, 0),
        ValidityClaim.CountAtLeast(PlateauRejected, 0), ValidityClaim.CountAtLeast(VertexTouchRejected, 0),
        ValidityClaim.CountAtLeast(StitchedCandidates, 0),
        ValidityClaim.CountAtLeast(BranchStops, 0), ValidityClaim.CountAtLeast(BranchNodes, 0),
        ValidityClaim.CountAtLeast(MaxIncidentSegments, 0), ValidityClaim.CountAtLeast(EmittedCurves, 0),
        DedupedSegments <= RawSegments, EmittedCurves <= StitchedCandidates);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct ScalarIsolineResult(Seq<Curve> Curves, IsolineCensus Census);

[StructLayout(LayoutKind.Auto)]
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

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
