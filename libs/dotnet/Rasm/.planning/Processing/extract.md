# [RASM_VECTORS_EXTRACT]

`ExtractionDomain` owns the extraction/projection API: one polymorphic `Of` ingress admits raw Rhino geometry or an admitted `CellLattice` into a typed sampling domain, `ContourPolicy` sections every domain through the owner its shape names — RhinoCommon's contour and iso adapters for the multi-plane and surface-iso routes, the `Meshing/intersect` `IntersectOp.PlaneMesh` crossing table for a single mesh section, the `reconstruct.md` marching-squares owner for lattice scalar levels — and typed projection rows fold every request shape to any output type. One local marching-triangles kernel serves the per-vertex scalar contouring no owner carries.

Output dispatch rides `Numerics/atoms.md`'s `ResultProjection.Rows`; evidence validity folds through `Domain/results.md`'s `ValidityClaim` under the `Op` value key. Sampling owners compose unchanged — `sample.md` evaluates seeds, `flow.md` traces stream bundles, `Spatial/fields.md` samples the scalar, vector, and tensor fields through its tagged rows, `Processing/geodesics.md` resolves the mesh-bound log-map probe, and `Meshing/reconstruct.md` extracts the marching-cubes iso-surface — this API re-implements none of them.

## [01]-[INDEX]

- [02]-[SECTIONING]: domain ingress, admission, and owner-routed contour/iso sectioning with the local scalar-isoline kernel.
- [03]-[PROJECTION_ROW]: `Extraction` request union and its typed `Project<TOut>` egress over probe, iso-surface, and sampled modes.

## [02]-[SECTIONING]

- Owner: `ExtractionDomain` `[Union]`, whose polymorphic `Of` ingress discriminates on runtime shape and admits each arm through its own owner; `ContourPolicy` `[Union]`, whose factories admit every section policy through the `Domain/validation.md` vocabulary.
- Entry: `domain.Contours(policy, …)` is a total `Switch` routing domain then policy to its owner, and every unsupported domain-policy pairing is a typed `Unsupported` fault naming both sides, never a silent empty.
- Law: a mesh-plane section has ONE owner — `Meshing/intersect`'s `IntersectOp.PlaneMesh`, whose exact straddle signs and oriented `Chain` loops this page emits as polyline curves. Reaching RhinoCommon's single-plane contour beside it spells two answers to one question. Interval sweep (`AxisCase`) and surface iso stay native: RhinoCommon owns a multi-plane sweep and a trim-aware iso curve that no kernel owner carries, and `ExtractionTally.Native` records which side answered.
- Auto: the local scalar-isoline kernel triangulates, extracts per-face level crossings under the `Fraction` lane's scale-relative band, welds them through a point key quantized on the `Weld` lane, dedups, and stitches seed polylines end-to-end, recording every branch node rather than guessing through it. Its census is the `IsolineCensus` MONOID the kernels `tell` — one `Writer` run yields curves and census together, so no kernel returns-and-reassigns a census through its own signature.
- Output: `IsolineCensus` carries the full kernel evidence — segment, rejection (plateau, degenerate, and vertex-touch), stitch, and branch counts — folded to one validity claim; `CurveBatch` bundles the accepted curves with the composed `ExtractionTally`. Routing is `ExtractionTally.Native`'s business alone, so no child carries a second routing column.
- Boundary: native adapters wrap every RhinoCommon call in `Op.Catch` so a host throw converts at the boundary. Scalar-isoline extraction is the named statement-kernel exemption: no owner carries a per-vertex scalar contour, so the kernel follows Rhino's triangulated topology and returns stitched candidates only.
- Exemption: the stitch frontier stays a BCL `bool[]`/`Dictionary` walk and REFUSES QuikGraph by name — `ConnectedComponents` answers the component set where the whole product here is the polyline's vertex ORDER, and `EulerianTrailAlgorithm` demands a traversal this kernel deliberately refuses, stopping at every branch node and tallying it as `BranchStops` rather than choosing an arm. Every table dies inside the one fold that fills it.

## [03]-[PROJECTION_ROW]

- Owner: `Extraction` `[Union]` is the public request vocabulary — three probe cases as the field point-probes, one contour case, one iso-surface case, and four sampled cases over one shared seed generator; `ExtractionTolerance` `[Union]` carries provenance and value as one.
- Entry: the request factories admit once — probe source and sample gated, an admitted domain null-gated and never rebuilt, policy validated through its own `Admit`, sampled payload, domain, and seeds gated at the case factory, iso bounds gated finite and non-degenerate; `extraction.Project<TOut>(…)` is the one egress, each request kind resolving its output through typed projection rows.
- Law: every union on this page is one owner, so its factories carry the `Op? key = null` + `OrDefault()` spelling as a type-wide contract rather than a per-member attribute.
- Auto: `ProjectSamples` is the one sampled spine — evaluate the seeds, fold each through the mode's item arm, mint the tally, and project through one `Rows` call; item rows gate on zero rejections, so a partial sampled extraction is a typed fault, never a truncated success.
- Output: `ExtractionTally` carries the native-route flag, attempted and emitted counts with derived rejected and completion, the tolerance carrier, and the optional child evidence, all folded to one validity claim.
- Growth: a new section policy is one `ContourPolicy` case and one adapter arm per admitting domain, a new sampled mode one `Extraction` case and one spine arm, a new probe output one `ProjectionRow`, a new ingress shape one `Of` arm.
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
using Dimension = Rasm.Numerics.Dimension;

namespace Rasm.Processing;

// --- [TYPES] ---------------------------------------------------------------------------
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
}

[Union]
public abstract partial record ContourPolicy {
    public sealed record PlaneCase(Plane Section) : ContourPolicy;
    public sealed record AxisCase(Point3d Start, Point3d End, PositiveMagnitude Interval) : ContourPolicy;
    public sealed record SurfaceIsoCase(IsoStatus Status, double Parameter) : ContourPolicy;
    public sealed record MeshScalarCase(Arr<double> Values, Seq<double> Levels) : ContourPolicy;
    private ContourPolicy() { }
    public static Fin<ContourPolicy> Plane(Plane section) =>
        new PlaneCase(Section: section).Admit();
    public static Fin<ContourPolicy> Axis(Point3d start, Point3d end, double interval) {
        return FactoryBridge.Accept<PositiveMagnitude>(candidate: interval)
            .Bind(step => new AxisCase(Start: start, End: end, Interval: step).Admit());
    }
    public static Fin<ContourPolicy> SurfaceIso(IsoStatus status, double parameter) =>
        new SurfaceIsoCase(Status: status, Parameter: parameter).Admit();
    public static Fin<ContourPolicy> MeshScalar(Arr<double> values, Seq<double> levels) =>
        new MeshScalarCase(Values: values, Levels: levels).Admit();
    internal Fin<ContourPolicy> Admit() => Switch(
        state: key,
        planeCase: static (policy) => Rasm.Domain.Admit.Plane(basis: policy.Section).Map(_ => (ContourPolicy)policy),
        axisCase: static (policy) =>
            from _ in Rasm.Domain.Admit.AllFinite(policy.Start, policy.End)
            from __ in guard((policy.End - policy.Start).Length > 0.0, new KernelFault.InvalidInput())
            select (ContourPolicy)policy,
        surfaceIsoCase: static (policy) => (policy.Status, policy.Parameter) switch {
            (IsoStatus.X or IsoStatus.Y, double parameter) when double.IsFinite(parameter) => Fin.Succ<ContourPolicy>(policy),
            (IsoStatus.North or IsoStatus.East or IsoStatus.South or IsoStatus.West, _) => Fin.Succ<ContourPolicy>(policy),
            _ => Fin.Fail<ContourPolicy>(new KernelFault.InvalidInput()),
        },
        meshScalarCase: static (policy) =>
            from scalars in Rasm.Domain.Admit.All(toSeq(policy.Values.AsIterable()), static value => ValidityClaim.Finite(value: value), floor: 1)
            from levels in Rasm.Domain.Admit.All(policy.Levels, static value => ValidityClaim.Finite(value: value), floor: 1)
            select (ContourPolicy)policy);
}

[Union]
public abstract partial record ExtractionDomain {
    public sealed record SupportCase : ExtractionDomain { internal SupportCase(SupportSpace value) => Value = value; public SupportSpace Value { get; } }
    public sealed record MeshCase : ExtractionDomain { internal MeshCase(MeshSpace value) => Value = value; public MeshSpace Value { get; } }
    public sealed record CloudCase : ExtractionDomain { internal CloudCase(VectorCloud value) => Value = value; public VectorCloud Value { get; } }
    public sealed record LatticeCase : ExtractionDomain { internal LatticeCase(CellLattice value) => Value = value; public CellLattice Value { get; } }
    private ExtractionDomain() { }
    public static Fin<ExtractionDomain> Support(SupportSpace value) =>
        Optional(value).ToFin(key.OrDefault().InvalidInput()).Map(valid => (ExtractionDomain)new SupportCase(value: valid));
    public static Fin<ExtractionDomain> Mesh(MeshSpace value) =>
        key.OrDefault().Need(value.Native).Map(_ => (ExtractionDomain)new MeshCase(value: value));
    public static Fin<ExtractionDomain> Cloud(VectorCloud value) =>
        Admit.NotNull(value).Map(static cloud => (ExtractionDomain)new CloudCase(cloud));
    public static Fin<ExtractionDomain> Lattice(CellLattice value) =>
        key.OrDefault().AcceptValue(value: (ExtractionDomain)new LatticeCase(value: value));
    public static Fin<ExtractionDomain> Of(object? value, Context context) {
        return Optional(value).ToFin(new KernelFault.InvalidInput()).Bind(source => source switch {
            ExtractionDomain domain => Fin.Succ(domain),
            Mesh mesh => MeshSpace.Of(native: mesh, context: context).Bind(space => Mesh(value: space)),
            VectorCloud cloud => Cloud(value: cloud),
            PointCloud cloud => VectorCloud.Cluster(points: toSeq(cloud.GetPoints()), context: context).Bind(active => Cloud(value: active)),
            CellLattice lattice => Lattice(value: lattice),
            object candidate => SupportSpace.Of(value: candidate).Bind(space => Support(value: space)),
        });
    }
    internal Fin<CurveBatch> Contours(ContourPolicy policy, Context context) => Switch(
        state: (Policy: policy, Context: context),
        supportCase: static (state, domain) => domain.Value.Value switch {
            Brep brep => Try.lift(() => state.Policy.Switch(
                state: brep,
                planeCase: static (s, p) => AcceptNative(Brep.CreateContourCurves(s, p.Section), ExtractionTolerance.RhinoDefault, s.Key),
                axisCase: static (s, p) => AcceptNative(Brep.CreateContourCurves(s, p.Start, p.End, p.Interval.Value), ExtractionTolerance.RhinoDefault, s.Key),
                surfaceIsoCase: static (s, _) => Fin.Fail<CurveBatch>(new KernelFault.Unsupported(typeof(Brep), typeof(ContourPolicy.SurfaceIsoCase))),
                meshScalarCase: static (s, _) => Fin.Fail<CurveBatch>(new KernelFault.Unsupported(typeof(Brep), typeof(ContourPolicy.MeshScalarCase))))).Run().Bind(static inner => inner),
            Mesh mesh => MeshSpace.Of(native: mesh, context: state.Context)
                .Bind(space => CurvesFromMesh(space: space, policy: state.Policy)),
            Surface surface => Try.lift(() => state.Policy.Switch(
                state: surface,
                surfaceIsoCase: static (s, p) =>
                    from frame in (p.Status switch {
                        IsoStatus.X => Fin.Succ((Direction: 1, Parameter: p.Parameter)),
                        IsoStatus.Y => Fin.Succ((Direction: 0, Parameter: p.Parameter)),
                        IsoStatus.West => Fin.Succ((Direction: 1, Parameter: s.Domain(0).T0)),
                        IsoStatus.East => Fin.Succ((Direction: 1, Parameter: s.Domain(0).T1)),
                        IsoStatus.South => Fin.Succ((Direction: 0, Parameter: s.Domain(1).T0)),
                        IsoStatus.North => Fin.Succ((Direction: 0, Parameter: s.Domain(1).T1)),
                        _ => Fin.Fail<(int Direction, double Parameter)>(new KernelFault.InvalidInput()),
                    })
                    from curves in s is BrepFace face
                        ? Optional(face.TrimAwareIsoCurve(frame.Direction, frame.Parameter)).ToFin(new KernelFault.InvalidResult())
                        : Optional(s.IsoCurve(frame.Direction, frame.Parameter)).ToFin(new KernelFault.InvalidResult()).Map(curve => (Curve[])[curve])
                    from batch in AcceptNative(curves, ExtractionTolerance.NotApplicable, s.Key)
                    select batch,
                planeCase: static (s, _) => Fin.Fail<CurveBatch>(new KernelFault.Unsupported(typeof(Surface), typeof(ContourPolicy.PlaneCase))),
                axisCase: static (s, _) => Fin.Fail<CurveBatch>(new KernelFault.Unsupported(typeof(Surface), typeof(ContourPolicy.AxisCase))),
                meshScalarCase: static (s, _) => Fin.Fail<CurveBatch>(new KernelFault.Unsupported(typeof(Surface), typeof(ContourPolicy.MeshScalarCase))))).Run().Bind(static inner => inner),
            VectorCloud.ClusterCase cloud => CurvesFromCloud(cloud: cloud, policy: state.Policy, context: state.Context),
            _ => Fin.Fail<CurveBatch>(error: new KernelFault.Unsupported(InputType: domain.Value.SourceType, OutputType: typeof(Seq<Curve>))),
        },
        meshCase: static (state, domain) => CurvesFromMesh(space: domain.Value, policy: state.Policy),
        cloudCase: static (state, domain) => domain.Value is VectorCloud.ClusterCase cloud
            ? CurvesFromCloud(cloud: cloud, policy: state.Policy, context: state.Context)
            : Fin.Fail<CurveBatch>(error: new KernelFault.Unsupported(InputType: domain.Value.GetType(), OutputType: typeof(Seq<Curve>))),
        latticeCase: static (state, domain) => Try.lift(() => state.Policy.Switch(
            state: (Grid: domain.Value, Context: state.Context, Key: state.Key),
            meshScalarCase: static (s, p) =>
                from field in ScalarField.Lattice(s.Grid, p.Values)
                from results in p.Levels.TraverseM(level => IsoContour.Detailed(field, s.Grid, level, s.Context, s.Key)).As()
                from batch in AcceptCurves(
                    results.Bind(static result => result.Loops.Map(static chain => (Curve)chain.Points.ToPolylineCurve())),
                    results.Sum(static result => result.Loops.Count), native: false,
                    ExtractionTolerance.FromContext(s.Context.Absolute))
                select batch,
            planeCase: static (s, _) => Fin.Fail<CurveBatch>(new KernelFault.Unsupported(typeof(CellLattice), typeof(ContourPolicy.PlaneCase))),
            axisCase: static (s, _) => Fin.Fail<CurveBatch>(new KernelFault.Unsupported(typeof(CellLattice), typeof(ContourPolicy.AxisCase))),
            surfaceIsoCase: static (s, _) => Fin.Fail<CurveBatch>(new KernelFault.Unsupported(typeof(CellLattice), typeof(ContourPolicy.SurfaceIsoCase))))).Run().Bind(static inner => inner));

    private static Fin<CurveBatch> CurvesFromMesh(MeshSpace space, ContourPolicy policy) =>
        Try.lift(() => policy.Switch(
            state: space,
            planeCase: static (state, p) =>
                Intersection.Apply(new IntersectOp.PlaneMesh(Cut: p.Section, Mesh: state, Policy: IntersectPolicy.Canonical), state.Key)
                    .Bind(result => result is IntersectResult.Chains chains
                        ? AcceptCurves(curves: chains.Walked.Map(static chain => (Curve)chain.Points.ToPolylineCurve()), attempted: chains.Walked.Count,
                            native: false, tolerance: ExtractionTolerance.FromContext(state.Tolerance.Absolute))
                        : Fin.Fail<CurveBatch>(new KernelFault.InvalidResult())),
            axisCase: static (state, p) => AcceptNative(curves: Rhino.Geometry.Mesh.CreateContourCurves(meshToContour: state.Native, contourStart: p.Start, contourEnd: p.End, interval: p.Interval.Value, tolerance: state.Tolerance.Absolute.Value), tolerance: ExtractionTolerance.FromContext(state.Tolerance.Absolute)),
            meshScalarCase: static (state, p) => ScalarIsolinesDetailed(mesh: state.Native, values: p.Values, levels: p.Levels, context: state.Tolerance)
                .Bind(result => AcceptCurves(curves: result.Curves, attempted: result.Census.StitchedCandidates, native: false, tolerance: ExtractionTolerance.FromContext(state.Tolerance.For(ToleranceLane.Weld)), scalarIsoline: Some(result))),
            surfaceIsoCase: static (state, _) => Fin.Fail<CurveBatch>(error: new KernelFault.Unsupported(InputType: typeof(Mesh), OutputType: typeof(ContourPolicy.SurfaceIsoCase))))).Run().Bind(static inner => inner);
    private static Fin<CurveBatch> CurvesFromCloud(VectorCloud.ClusterCase cloud, ContourPolicy policy, Context context) =>
        Try.lift(() => policy.Switch(
            state: (Cloud: cloud, Context: context),
            axisCase: static (state, p) => state.Cloud.UseIndex(project: pc => AcceptNative(curves: pc.CreateContourCurves(contourStart: p.Start, contourEnd: p.End, interval: p.Interval.Value, absoluteTolerance: state.Context.Absolute.Value), tolerance: ExtractionTolerance.FromContext(state.Context.Absolute))),
            planeCase: static (state, p) => state.Cloud.UseIndex(project: pc => AcceptNative(curves: pc.CreateSectionCurve(plane: p.Section, absoluteTolerance: state.Context.Absolute.Value), tolerance: ExtractionTolerance.FromContext(state.Context.Absolute))),
            surfaceIsoCase: static (state, _) => Fin.Fail<CurveBatch>(error: new KernelFault.Unsupported(InputType: typeof(PointCloud), OutputType: typeof(ContourPolicy.SurfaceIsoCase))),
            meshScalarCase: static (state, _) => Fin.Fail<CurveBatch>(error: new KernelFault.Unsupported(InputType: typeof(PointCloud), OutputType: typeof(ContourPolicy.MeshScalarCase))))).Run().Bind(static inner => inner);
    private static Fin<CurveBatch> AcceptNative(Curve[] curves, ExtractionTolerance tolerance) =>
        Optional(curves).ToFin(new KernelFault.InvalidResult())
            .Bind(active => AcceptCurves(curves: toSeq(active), attempted: active.Length, native: true, tolerance: tolerance));
    private static Fin<CurveBatch> AcceptCurves(Seq<Curve> curves, int attempted, bool native, ExtractionTolerance tolerance, Option<ScalarIsolineResult> scalarIsoline = default) {
        Seq<Curve> accepted = curves.Filter(static curve => curve is not null && curve.IsValid);
        return Fin.Succ(new CurveBatch(
            accepted, scalarIsoline,
            new ExtractionTally(Native: native, Attempted: attempted, Emitted: accepted.Count, Tolerance: tolerance,
                ScalarIsoline: scalarIsoline.Map(static result => result.Census))));
    }

    private static Fin<ScalarIsolineResult> ScalarIsolinesDetailed(Mesh mesh, Arr<double> values, Seq<double> levels, Context context) {
        if (values.Count != mesh.Vertices.Count) return Fin.Fail<ScalarIsolineResult>(new KernelFault.InvalidInput());
        using Mesh triangulated = mesh.DuplicateMesh();
        if (triangulated.Faces.QuadCount > 0 && !triangulated.Faces.ConvertQuadsToTriangles()) return Fin.Fail<ScalarIsolineResult>(new KernelFault.InvalidResult());
        if (triangulated.Vertices.Count != values.Count) return Fin.Fail<ScalarIsolineResult>(new KernelFault.InvalidResult());
        Tolerance band = context.For(ToleranceLane.Fraction);
        Tolerance weld = context.For(ToleranceLane.Weld);
        List<(Point3d A, Point3d B)> segments = [];
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

        static Writer<IsolineCensus, Unit> AddFaceIsolines(Mesh mesh, MeshFace face, Arr<double> values, Seq<double> levels, Tolerance band, Tolerance weld, List<(Point3d A, Point3d B)> segments) {
            Point3d[] points = [mesh.Vertices[index: face.A], mesh.Vertices[index: face.B], mesh.Vertices[index: face.C]];
            double[] scalars = [values[index: face.A], values[index: face.B], values[index: face.C]];
            (int A, int B)[] edges = [(0, 1), (1, 2), (2, 0)];
            return levels.Fold(Writer.pure<IsolineCensus, Unit>(unit), (acc, level) => acc.Bind(_ => {
                double epsilon = band.Value * Math.Max(1.0, Math.Max(Math.Abs(value: level), scalars.Max(static value => Math.Abs(value: value))));
                if (scalars.All(value => Math.Abs(value: value - level) <= epsilon)) { return Writer.tell(IsolineCensus.Empty with { PlateauRejected = 1 }); }
                ((int A, int B) Edge, double ADelta, double BDelta)[] cuts = System.Array.ConvertAll(array: edges, converter: edge => (edge, scalars[edge.A] - level, scalars[edge.B] - level));
                (Point3d A, Point3d B)[] edgeSegments = System.Array.ConvertAll(
                    array: System.Array.FindAll(array: cuts, match: cut => Math.Abs(value: cut.ADelta) <= epsilon && Math.Abs(value: cut.BDelta) <= epsilon),
                    converter: cut => (A: points[cut.Edge.A], B: points[cut.Edge.B]));
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
                    ? Writer.tell((IsolineCensus.Empty with { RawSegments = edgeSegments.Length + 1 }).Combine(Emit(segments, unique)))
                    : Writer.tell(IsolineCensus.Empty with { RawSegments = edgeSegments.Length, VertexTouchRejected = 1 });
            }));

            static IsolineCensus Emit(List<(Point3d A, Point3d B)> sink, Point3d[] unique) {
                sink.Add((A: unique[0], B: unique[1]));
                return IsolineCensus.Empty;
            }
        }
        static Writer<IsolineCensus, Seq<(Point3d A, Point3d B)>> DeduplicateSegments(List<(Point3d A, Point3d B)> segments, Tolerance weld) {
            HashSet<((long X, long Y, long Z) A, (long X, long Y, long Z) B)> seen = [];
            List<(Point3d A, Point3d B)> unique = [];
            int degenerate = 0;
            foreach ((Point3d A, Point3d B) segment in segments) {
                (long X, long Y, long Z) a = KeyOf(point: segment.A, weld: weld);
                (long X, long Y, long Z) b = KeyOf(point: segment.B, weld: weld);
                if (a.Equals(b)) { degenerate++; continue; }
                ((long X, long Y, long Z) A, (long X, long Y, long Z) B) edge = a.CompareTo(b) <= 0 ? (a, b) : (b, a);
                if (seen.Add(edge)) unique.Add(segment);
            }
            return Writer.write(toSeq(unique), IsolineCensus.Empty with { DegenerateRejected = degenerate, DedupedSegments = unique.Count });
        }
        static Writer<IsolineCensus, Seq<Curve>> StitchSegments(Seq<(Point3d A, Point3d B)> segments, Tolerance weld) {
            (Point3d A, Point3d B)[] all = [.. segments.AsIterable()];
            bool[] used = new bool[all.Length];
            Dictionary<(long X, long Y, long Z), List<int>> incident = [];
            for (int i = 0; i < all.Length; i++) {
                ref List<int>? a = ref CollectionsMarshal.GetValueRefOrAddDefault(dictionary: incident, key: KeyOf(point: all[i].A, weld: weld), exists: out _);
                ref List<int>? b = ref CollectionsMarshal.GetValueRefOrAddDefault(dictionary: incident, key: KeyOf(point: all[i].B, weld: weld), exists: out _);
                (a ??= []).Add(item: i);
                (b ??= []).Add(item: i);
            }
            (int maxIncident, int branchNodes) = incident.Values.Select(static edges => edges.Count).Aggregate(
                seed: (Max: 0, Branches: 0),
                func: static (state, count) => (Math.Max(val1: state.Max, val2: count), state.Branches + (count > 2 ? 1 : 0)));
            IsolineCensus ledger = IsolineCensus.Empty with { BranchNodes = branchNodes, MaxIncidentSegments = maxIncident };
            List<Curve> curves = [];
            int attempted = 0;
            for (int i = 0; i < all.Length; i++) {
                if (used[i]) continue;
                List<Point3d> points = [all[i].A, all[i].B];
                used[i] = true;
                ledger = ledger.Combine(Extend(prepend: true)).Combine(Extend(prepend: false));
                Polyline polyline = [.. points];
                attempted++;
                if (polyline.IsValid && polyline.Count >= 2) curves.Add(item: polyline.ToPolylineCurve());

                IsolineCensus Extend(bool prepend) {
                    IsolineCensus tally = IsolineCensus.Empty;
                    while (true) {
                        (long X, long Y, long Z) at = KeyOf(prepend ? points[0] : points[^1], weld);
                        if (!incident.TryGetValue(at, out List<int>? candidates)) return tally;
                        Seq<int> open = toSeq(candidates).Filter(candidate => !used[candidate]);
                        if (open.Count > 1) return tally.Combine(IsolineCensus.Empty with { BranchStops = 1 });
                        if (open.Case is not int index) return tally;
                        (Point3d A, Point3d B) segment = all[index];
                        points.Insert(prepend ? 0 : points.Count, KeyOf(segment.A, weld).Equals(at) ? segment.B : segment.A);
                        used[index] = true;
                    }
                }
            }
            return Writer.write(toSeq(curves), ledger.Combine(IsolineCensus.Empty with { StitchedCandidates = attempted, EmittedCurves = curves.Count }));
        }
        static (long X, long Y, long Z) KeyOf(Point3d point, Tolerance weld) {
            double scale = 1.0 / Math.Max(val1: weld.Value, val2: EpsilonPolicy.SqrtEpsilon);
            return (
                X: (long)Math.Round(point.X * scale, MidpointRounding.ToEven),
                Y: (long)Math.Round(point.Y * scale, MidpointRounding.ToEven),
                Z: (long)Math.Round(point.Z * scale, MidpointRounding.ToEven));
        }
    }
}

[Union]
public abstract partial record Extraction {
    public sealed record VectorProbeCase(VectorField Source, Point3d Sample) : Extraction;
    public sealed record ScalarProbeCase(ScalarField Source, Point3d Sample) : Extraction;
    public sealed record TensorProbeCase(TensorField Source, Point3d Sample) : Extraction;
    public sealed record ContourCase(ExtractionDomain Domain, ContourPolicy Policy) : Extraction;
    public sealed record IsoSurfaceCase(ScalarField Field, BoundingBox Bounds, Dimension Resolution, Dimension MaxRootSteps) : Extraction;
    public sealed record GlyphCase(VectorField Field, PositiveMagnitude Scale, ExtractionDomain Domain, SampleKind Seeds) : Extraction;
    public sealed record GridCase(ScalarField Field, ExtractionDomain Domain, SampleKind Seeds) : Extraction;
    public sealed record StreamBundleCase(VectorField Field, PositiveMagnitude InitialStep, RungeKuttaIntegrator Integrator, Termination Termination, ExtractionDomain Domain, SampleKind Seeds) : Extraction;
    public sealed record DrapeCase(Vector3d Direction, ExtractionDomain Domain, SampleKind Seeds) : Extraction;
    private Extraction() { }
    public static Fin<Extraction> Probe(VectorField source, Point3d sample) =>
        Probe(source, sample, static (field, point) => (Extraction)new VectorProbeCase(field, point));
    public static Fin<Extraction> Probe(ScalarField source, Point3d sample) =>
        Probe(source, sample, static (field, point) => (Extraction)new ScalarProbeCase(field, point));
    public static Fin<Extraction> Probe(TensorField source, Point3d sample) =>
        Probe(source, sample, static (field, point) => (Extraction)new TensorProbeCase(field, point));
    private static Fin<Extraction> Probe<TField>(TField source, Point3d sample, Func<TField, Point3d, Extraction> create) where TField : class {
        return from field in Admit.Need(source)
               from point in Acceptance.Value(sample)
               select create(field, point);
    }
    public static Fin<Extraction> Contour(ExtractionDomain domain, ContourPolicy policy) {
        return from validDomain in Admit.NotNull(value: domain)
               from validPolicy in Optional(policy).ToFin(new KernelFault.InvalidInput()).Bind(active => active.Admit())
               select (Extraction)new ContourCase(Domain: validDomain, Policy: validPolicy);
    }
    public static Fin<Extraction> IsoSurface(ScalarField field, BoundingBox bounds, int resolution, int maxRootSteps) {
        return from validField in Optional(field).ToFin(new KernelFault.InvalidInput())
               from _ in guard(bounds.IsValid && bounds.Diagonal.Length > 0.0, new KernelFault.InvalidInput())
               from cells in FactoryBridge.Accept<Dimension>(candidate: resolution)
               from steps in FactoryBridge.Accept<Dimension>(candidate: maxRootSteps)
               select (Extraction)new IsoSurfaceCase(Field: validField, Bounds: bounds, Resolution: cells, MaxRootSteps: steps);
    }
    public static Fin<Extraction> Glyph(VectorField field, double scale, ExtractionDomain domain, SampleKind seeds) {
        return from source in Admit.NotNull(value: field)
               from magnitude in FactoryBridge.Accept<PositiveMagnitude>(candidate: scale)
               from validDomain in Admit.NotNull(value: domain)
               from validSeeds in SampleKind.Admit(value: seeds)
               select (Extraction)new GlyphCase(Field: source, Scale: magnitude, Domain: validDomain, Seeds: validSeeds);
    }
    public static Fin<Extraction> Grid(ScalarField field, ExtractionDomain domain, SampleKind seeds) {
        return from source in Admit.NotNull(value: field)
               from validDomain in Admit.NotNull(value: domain)
               from validSeeds in SampleKind.Admit(value: seeds)
               select (Extraction)new GridCase(Field: source, Domain: validDomain, Seeds: validSeeds);
    }
    public static Fin<Extraction> Drape(Vector3d direction, ExtractionDomain domain, SampleKind seeds) {
        return from _ in guard(ValidityClaim.Finite(direction) && direction.Length > 0.0, new KernelFault.InvalidInput()).ToFin()
               from validDomain in Admit.NotNull(value: domain)
               from validSeeds in SampleKind.Admit(value: seeds)
               select (Extraction)new DrapeCase(Direction: direction, Domain: validDomain, Seeds: validSeeds);
    }
    public static Fin<Extraction> StreamBundle(VectorField field, double initialStep, Termination termination, ExtractionDomain domain, SampleKind seeds, Option<RungeKuttaIntegrator> integrator = default) {
        return from source in Admit.NotNull(value: field)
               from step in FactoryBridge.Accept<PositiveMagnitude>(candidate: initialStep)
               from stop in Termination.Admit(value: termination)
               from active in RungeKuttaIntegrator.AdmitOrFixed(value: integrator)
               from validDomain in Admit.NotNull(value: domain)
               from validSeeds in SampleKind.Admit(value: seeds)
               select (Extraction)new StreamBundleCase(Field: source, InitialStep: step, Integrator: active, Termination: stop, Domain: validDomain, Seeds: validSeeds);
    }

    internal Fin<TOut> Project<TOut>(Context context) {
        return Switch(
        state: context,
        vectorProbeCase: static (state, extraction) => ResultProjection.Rows<VectorProbeCase, TOut>(self: extraction, owner: typeof(VectorProbeCase),
            ProjectionRow.Of<LogMapResult>(() => extraction.Source is VectorField.TangentLogMapCase log
                ? GeodesicKernel.LogMapAt(space: log.Space, source: log.Source, sample: extraction.Sample, time: log.Time.Value, algorithm: log.Algorithm, trace: log.Trace, windows: log.Windows)
                : Fin.Fail<LogMapResult>(new KernelFault.Unsupported(InputType: extraction.Source.GetType(), OutputType: typeof(LogMapResult)))),
            ProjectionRow.Of<LogMapTrace>(() => extraction.Source is VectorField.TangentLogMapCase log
                ? GeodesicKernel.LogMapAt(space: log.Space, source: log.Source, sample: extraction.Sample, time: log.Time.Value, algorithm: log.Algorithm, trace: log.Trace, windows: log.Windows).Map(static result => result.Trace)
                : Fin.Fail<LogMapTrace>(new KernelFault.Unsupported(InputType: extraction.Source.GetType(), OutputType: typeof(LogMapTrace)))),
            ProjectionRow.Of<HodgeWitness>(() => extraction.Source is VectorField.HodgeCase hodge
                ? DecAssembly.HodgeSolutionOf(source: hodge.Source, space: hodge.Space, context: state).Map(static solved => solved.Witness)
                : Fin.Fail<HodgeWitness>(new KernelFault.Unsupported(InputType: extraction.Source.GetType(), OutputType: typeof(HodgeWitness)))),
            ProjectionRow.Of<HarmonicCensus>(() => extraction.Source is VectorField.HodgeCase hodge
                ? DecAssembly.HodgeSolutionOf(source: hodge.Source, space: hodge.Space, context: state).Bind(solved => solved.Witness.Harmonic.ToFin(new KernelFault.InvalidResult()))
                : Fin.Fail<HarmonicCensus>(new KernelFault.Unsupported(InputType: extraction.Source.GetType(), OutputType: typeof(HarmonicCensus)))),
            ProjectionRow.Of<Vector3d>(() => extraction.Source.SampleVector(sample: extraction.Sample, context: state)),
            ProjectionRow.Of<double>(() => extraction.Source.SampleVector(sample: extraction.Sample, context: state).Map(static vector => vector.Length)),
            ProjectionRow.Of<VectorSpan>(() => SpanAt(extraction.Source, extraction.Sample, state, state.Key)),
            ProjectionRow.Of<Direction>(() => SpanAt(extraction.Source, extraction.Sample, state, state.Key).Bind(span => span.Project<Direction>())),
            ProjectionRow.Of<Line>(() => SpanAt(extraction.Source, extraction.Sample, state, state.Key).Bind(span => span.Project<Line>()))),
        scalarProbeCase: static (state, extraction) => ResultProjection.Rows<ScalarProbeCase, TOut>(self: extraction, owner: typeof(ScalarProbeCase),
            ProjectionRow.Of<SdfSample>(() => extraction.Source.SampleSdfDetailed(sample: extraction.Sample, context: state)),
            ProjectionRow.Of<FieldSample>(() => extraction.Source.SampleDetailed(sample: extraction.Sample, context: state)),
            ProjectionRow.Of<double>(() => extraction.Source.SampleScalar(sample: extraction.Sample, context: state))),
        tensorProbeCase: static (state, extraction) => ResultProjection.Rows<TensorProbeCase, TOut>(self: extraction, owner: typeof(TensorProbeCase),
            ProjectionRow.Of<SymmetricMatrix>(() => extraction.Source.SampleTensor(sample: extraction.Sample, context: state)),
            ProjectionRow.Of<Seq<(double Eigenvalue, Direction Eigenvector)>>(() => extraction.Source.PrincipalDirections(sample: extraction.Sample, context: state))),
        contourCase: static (state, extraction) =>
            from batch in extraction.Domain.Contours(policy: extraction.Policy, context: state)
            from output in ResultProjection.Rows<ExtractionTally, TOut>(self: batch.Tally, owner: typeof(ContourCase),
                ProjectionRow.Of<Seq<Curve>>(() => Fin.Succ(batch.Curves)),
                ProjectionRow.Of<ScalarIsolineResult>(() => batch.ScalarIsoline.ToFin(Fail: new KernelFault.Unsupported(InputType: typeof(ContourPolicy), OutputType: typeof(ScalarIsolineResult)))),
                ProjectionRow.Of<IsolineCensus>(() => batch.ScalarIsoline.Map(static result => result.Census).ToFin(Fail: new KernelFault.Unsupported(InputType: typeof(ContourPolicy), OutputType: typeof(IsolineCensus)))))
            select output,
        isoSurfaceCase: static (state, extraction) =>
            from cell in FactoryBridge.Accept<PositiveMagnitude>(candidate: extraction.Bounds.Diagonal.MaximumCoordinate / extraction.Resolution.Value)
            from grid in CellLattice.Of(bounds: extraction.Bounds, cell: cell,
                ceiling: (long)extraction.Resolution.Value * extraction.Resolution.Value * extraction.Resolution.Value)
            from run in IsoSurface.Detailed(field: extraction.Field, grid: grid,
                policy: IsoSurfacePolicy.Default with { MaxRootSteps = extraction.MaxRootSteps }, context: state)
            from output in ResultProjection.Rows<IsoSurfaceRun, TOut>(self: run, owner: typeof(IsoSurfaceCase),
                ProjectionRow.Of<Mesh>(() => run.Space.ToFin(new KernelFault.InvalidResult()).Map(static space => space.DuplicateNative())),
                ProjectionRow.Of<MeshSpace>(() => run.Space.ToFin(new KernelFault.InvalidResult())),
                ProjectionRow.Of<ExtractionTally>(() => Fin.Succ(new ExtractionTally(
                    Native: true, Attempted: 1, Emitted: run.Space.IsSome ? 1 : 0,
                    Tolerance: ExtractionTolerance.RhinoFixed(run.FixedTolerance), IsoSurface: Some(run)))))
            select output,
        glyphCase: static (state, extraction) => ProjectSamples<TOut, Line>(
            seeds: extraction.Seeds, domain: extraction.Domain, context: state,
            sample: (point, model, op) => extraction.Field.SampleVector(sample: point, context: model)
                .Bind(vector => VectorSpan.Of(anchor: point, vector: vector, context: model))
                .Map(span => new Line(span.Anchor, span.Anchor + (extraction.Scale.Value * span.Value))),
            project: static (glyphs, rejected, tally, op) => ResultProjection.Rows<ExtractionTally, TOut>(self: tally, owner: typeof(GlyphCase),
                Gated<Seq<Line>>(rejected, () => Fin.Succ(glyphs)))),
        gridCase: static (state, extraction) => ProjectSamples<TOut, (Point3d Point, double Value)>(
            seeds: extraction.Seeds, domain: extraction.Domain, context: state,
            sample: (point, model, op) => extraction.Field.SampleScalar(sample: point, context: model).Map(value => (Point: point, Value: value)),
            project: static (samples, rejected, tally, op) => ResultProjection.Rows<ExtractionTally, TOut>(self: tally, owner: typeof(GridCase),
                Gated<Seq<(Point3d Point, double Value)>>(rejected, () => samples.ForAll(static sample => ValidityClaim.Finite(sample.Point) && ValidityClaim.Finite(value: sample.Value))
                    ? Fin.Succ(samples)
                    : Fin.Fail<Seq<(Point3d Point, double Value)>>(new KernelFault.InvalidResult())))),
        streamBundleCase: static (state, extraction) => ProjectSamples<TOut, StreamlineTrace>(
            seeds: extraction.Seeds, domain: extraction.Domain, context: state,
            sample: (seed, model, op) => FlowKernel.Trace<StreamlineTrace>(source: extraction.Field, seed: seed, initialStep: extraction.InitialStep, integrator: extraction.Integrator, termination: extraction.Termination, context: model),
            project: static (traces, rejected, tally, op) => ResultProjection.Rows<ExtractionTally, TOut>(self: tally, owner: typeof(StreamBundleCase),
                Streamed<StreamlineTrace>(rejected, traces),
                Streamed<Polyline>(rejected, traces),
                Streamed<Curve>(rejected, traces))),
        drapeCase: static (state, extraction) => extraction.Domain is ExtractionDomain.MeshCase meshDomain
            ? from samples in extraction.Seeds.Evaluate(domain: extraction.Domain, context: state)
              from direction in Direction.Of(value: extraction.Direction, context: state)
              from hits in Try.lift(() => {
                  Point3d[] projected = Rhino.Geometry.Intersect.Intersection.ProjectPointsToMeshesEx(
                      meshes: [meshDomain.Value.Native], points: samples.Points, direction: direction.Value,
                      tolerance: state.Absolute.Value, indices: out int[] indices);
                  return Acceptance.Value(value: (Projected: projected, Indices: indices));
              }).Run().Bind(static inner => inner)
              let covered = toSeq(hits.Indices).Distinct().Count
              let tally = new ExtractionTally(
                  Native: true, Attempted: samples.Points.Count, Emitted: covered,
                  Tolerance: ExtractionTolerance.FromContext(state.Absolute), Sample: Some(samples.Tally))
              from output in ResultProjection.Rows<ExtractionTally, TOut>(self: tally, owner: typeof(DrapeCase),
                  ProjectionRow.Of<Seq<Point3d>>(() => Fin.Succ(toSeq(hits.Projected))),
                  ProjectionRow.Of<Seq<(int Source, Point3d Point)>>(() => Fin.Succ(toSeq(hits.Indices.Zip(hits.Projected, static (source, point) => (Source: source, Point: point))))))
              select output
            : Fin.Fail<TOut>(new KernelFault.Unsupported(InputType: extraction.Domain.GetType(), OutputType: typeof(DrapeCase))));

        static Fin<VectorSpan> SpanAt(VectorField source, Point3d sample, Context context) =>
            source.SampleVector(sample: sample, context: context)
                .Bind(vector => VectorSpan.Of(anchor: sample, vector: vector, context: context));
    }

    private static ProjectionRow Gated<T>(int rejected, Func<Fin<T>> body) =>
        ProjectionRow.Of<T>(() => rejected == 0 ? body() : Fin.Fail<T>(new KernelFault.InvalidResult()));

    private static ProjectionRow Streamed<TShape>(int rejected, Seq<StreamlineTrace> traces) =>
        Gated<Seq<TShape>>(rejected, () => traces.TraverseM(trace => FlowKernel.ProjectTrace<TShape>(trace: trace, key: op)).As());

    private static Fin<TOut> ProjectSamples<TOut, TItem>(SampleKind seeds, ExtractionDomain domain, Context context, Func<Point3d, Context, Fin<TItem>> sample, Func<Seq<TItem>, int, ExtractionTally, Fin<TOut>> project) =>
        from samples in seeds.Evaluate(domain: domain, context: context)
        let split = samples.Points.Map(point => sample(point, context)).Partition()
        let tally = new ExtractionTally(
            Native: false, Attempted: samples.Points.Count, Emitted: split.Succs.Count,
            Tolerance: ExtractionTolerance.NotApplicable, Sample: Some(samples.Tally))
        from output in project(split.Succs, split.Fails.Count, tally)
        select output;
}

// --- [MODELS] --------------------------------------------------------------------------
internal readonly record struct CurveBatch(Seq<Curve> Curves, Option<ScalarIsolineResult> ScalarIsoline, ExtractionTally Tally);

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
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
