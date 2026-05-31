using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Vectors;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>] public sealed partial class ExtractionStatus { public static readonly ExtractionStatus Complete = new(key: 0), Approximate = new(key: 1); }
[SmartEnum<int>] public sealed partial class ExtractionRoute { public static readonly ExtractionRoute Native = new(key: 0), Local = new(key: 1); }
[SmartEnum<int>] public sealed partial class ToleranceSource { public static readonly ToleranceSource Context = new(key: 0), RhinoDefault = new(key: 1), NotApplicable = new(key: 2); }

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct ExtractionReceipt(ExtractionStatus Status, int Attempted, int Emitted, int Rejected, bool NativeRouted, ToleranceSource ToleranceSource, Option<double> Tolerance, bool ParallelCallback, Option<IsoSurfaceReceipt> IsoSurface = default, Option<ScalarIsolineReceipt> ScalarIsoline = default, Option<SampleReceipt> Sample = default) {
    public ExtractionRoute Route => NativeRouted ? ExtractionRoute.Native : ExtractionRoute.Local;
    internal static Fin<ExtractionReceipt> Of(ExtractionStatus status, int attempted, int emitted, bool nativeRouted, ToleranceSource toleranceSource, Option<double> tolerance, bool parallelCallback, Op key, Option<IsoSurfaceReceipt> isoSurface = default, Option<ScalarIsolineReceipt> scalarIsoline = default, Option<SampleReceipt> sample = default) =>
        attempted < 0 || emitted < 0 || emitted > attempted
            ? Fin.Fail<ExtractionReceipt>(error: key.InvalidResult())
            : Fin.Succ(new ExtractionReceipt(Status: status, Attempted: attempted, Emitted: emitted, Rejected: attempted - emitted, NativeRouted: nativeRouted, ToleranceSource: toleranceSource, Tolerance: tolerance, ParallelCallback: parallelCallback, IsoSurface: isoSurface, ScalarIsoline: scalarIsoline, Sample: sample));
}

[SmartEnum<int>] public sealed partial class ScalarIsolineRoute { public static readonly ScalarIsolineRoute LocalPiecewiseLinearMesh = new(key: 0, nativeRouted: false); public bool NativeRouted { get; } }

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct ScalarIsolineReceipt(ScalarIsolineRoute Route, int FiniteLevels, int RawSegments, int DedupedSegments, int DegenerateRejected, int PlateauRejected, int StitchedCandidates, int BranchStops, int BranchNodes, int MaxIncidentSegments, int EmittedCurves);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct ScalarIsolineResult(Seq<Curve> Curves, ScalarIsolineReceipt Receipt);

[StructLayout(LayoutKind.Auto)] internal readonly record struct ScalarIsolineSegment(Point3d A, Point3d B);

[StructLayout(LayoutKind.Auto)] internal readonly record struct ScalarIsolinePointKey(long X, long Y, long Z) { internal int Compare(ScalarIsolinePointKey other) => (X, Y, Z).CompareTo((other.X, other.Y, other.Z)); }
internal sealed class ScalarIsolineStats { internal int RawSegments, DedupedSegments, DegenerateRejected, PlateauRejected, StitchedCandidates, BranchStops, BranchNodes, MaxIncidentSegments, EmittedCurves; }

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct GlyphPolicy {
    private GlyphPolicy(SampleKind kind, PositiveMagnitude scale) { Kind = kind; Scale = scale; }
    public SampleKind Kind { get; }
    public PositiveMagnitude Scale { get; }
    public static Fin<GlyphPolicy> Of(SampleKind kind, double scale, Op? key = null) =>
        key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: scale)
            .Bind(validScale => Of(kind: kind, scale: validScale, key: key));
    public static Fin<GlyphPolicy> Of(SampleKind kind, PositiveMagnitude scale, Op? key = null) =>
        from validKind in SampleKind.Admit(value: kind, key: key.OrDefault())
        from _ in guard(scale.Value > RhinoMath.ZeroTolerance, key.OrDefault().InvalidInput())
        select new GlyphPolicy(kind: validKind, scale: scale);
}
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct GridPolicy {
    private GridPolicy(SampleKind kind) => Kind = kind;
    public SampleKind Kind { get; }
    public static Fin<GridPolicy> Of(SampleKind kind, Op? key = null) =>
        SampleKind.Admit(value: kind, key: key.OrDefault()).Map(static validKind => new GridPolicy(kind: validKind));
}
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct StreamBundlePolicy {
    private StreamBundlePolicy(SampleKind kind, PositiveMagnitude initialStep, FieldIntegrator integrator, Termination termination) { Kind = kind; InitialStep = initialStep; Integrator = integrator; Termination = termination; }
    public SampleKind Kind { get; }
    public PositiveMagnitude InitialStep { get; }
    public FieldIntegrator Integrator { get; }
    public Termination Termination { get; }
    public static Fin<StreamBundlePolicy> Of(SampleKind kind, double initialStep, Termination termination, FieldIntegrator? integrator = null, Op? key = null) =>
        key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: initialStep)
            .Bind(step => FieldIntegrator.AdmitOrFixed(value: integrator, key: key.OrDefault())
                .Bind(active => Of(kind: kind, initialStep: step, termination: termination, integrator: active, key: key)));
    public static Fin<StreamBundlePolicy> Of(SampleKind kind, PositiveMagnitude initialStep, Termination termination, FieldIntegrator integrator, Op? key = null) {
        Op op = key.OrDefault();
        return from validKind in SampleKind.Admit(value: kind, key: op)
               from validIntegrator in FieldIntegrator.Admit(value: integrator, key: op)
               from validTermination in Optional(termination).ToFin(op.InvalidInput())
               from _ in guard(initialStep.Value > RhinoMath.ZeroTolerance, op.InvalidInput())
               select new StreamBundlePolicy(kind: validKind, initialStep: initialStep, integrator: validIntegrator, termination: validTermination);
    }
}

[Union]
public abstract partial record ContourPolicy {
    private ContourPolicy() { }
    public sealed record PlaneCase(Plane Section) : ContourPolicy;
    public sealed record AxisCase(Point3d Start, Point3d End, PositiveMagnitude Interval) : ContourPolicy;
    public sealed record SurfaceIsoCase(IsoStatus Status, double Parameter) : ContourPolicy;
    public sealed record MeshScalarCase(Arr<double> Values, Seq<double> Levels) : ContourPolicy;
    public static Fin<ContourPolicy> Plane(Plane section, Op? key = null) =>
        FieldNabla.Plane(basis: section, key: key.OrDefault()).Map(active => (ContourPolicy)new PlaneCase(Section: active));
    public static Fin<ContourPolicy> Axis(Point3d start, Point3d end, double interval, Op? key = null) {
        Op op = key.OrDefault();
        Vector3d vector = end - start;
        return from _ in guard(FieldNabla.Finite(point: start) && FieldNabla.Finite(point: end) && FieldNabla.Finite(vector: vector) && vector.Length > RhinoMath.ZeroTolerance, op.InvalidInput())
               from step in op.AcceptValidated<PositiveMagnitude>(candidate: interval)
               select (ContourPolicy)new AxisCase(Start: start, End: end, Interval: step);
    }
    public static Fin<ContourPolicy> SurfaceIso(IsoStatus status, double parameter, Op? key = null) =>
        status switch {
            IsoStatus.X or IsoStatus.Y when RhinoMath.IsValidDouble(x: parameter) => Fin.Succ<ContourPolicy>(new SurfaceIsoCase(Status: status, Parameter: parameter)),
            IsoStatus.North or IsoStatus.East or IsoStatus.South or IsoStatus.West => Fin.Succ<ContourPolicy>(new SurfaceIsoCase(Status: status, Parameter: parameter)),
            _ => Fin.Fail<ContourPolicy>(key.OrDefault().InvalidInput()),
        };
    public static Fin<ContourPolicy> MeshScalar(Arr<double> values, Seq<double> levels, Op? key = null) =>
        values.Count > 0
            && !levels.IsEmpty
            && values.All(RhinoMath.IsValidDouble)
            && levels.ForAll(RhinoMath.IsValidDouble)
            ? Fin.Succ<ContourPolicy>(new MeshScalarCase(Values: values, Levels: levels))
            : Fin.Fail<ContourPolicy>(key.OrDefault().InvalidInput());
    internal Fin<ContourPolicy> Admit(Op key) =>
        Switch(
            state: key,
            planeCase: static (op, policy) => Plane(section: policy.Section, key: op),
            axisCase: static (op, policy) => Axis(start: policy.Start, end: policy.End, interval: policy.Interval.Value, key: op),
            surfaceIsoCase: static (op, policy) => SurfaceIso(status: policy.Status, parameter: policy.Parameter, key: op),
            meshScalarCase: static (op, policy) => MeshScalar(values: policy.Values, levels: policy.Levels, key: op));
}

[Union]
public abstract partial record ExtractionDomain {
    private ExtractionDomain() { }
    public sealed record SupportCase : ExtractionDomain { internal SupportCase(SupportSpace value) => Value = value; public SupportSpace Value { get; } }
    public sealed record MeshCase : ExtractionDomain { internal MeshCase(MeshSpace value) => Value = value; public MeshSpace Value { get; } }
    public sealed record CloudCase : ExtractionDomain { internal CloudCase(VectorCloud value) => Value = value; public VectorCloud Value { get; } }
    public static Fin<ExtractionDomain> Support(SupportSpace value, Op? key = null) =>
        Optional(value).ToFin(key.OrDefault().InvalidInput()).Map(valid => (ExtractionDomain)new SupportCase(value: valid));
    public static Fin<ExtractionDomain> Mesh(MeshSpace value, Op? key = null) =>
        Optional(value.Native).ToFin(key.OrDefault().InvalidInput()).Map(_ => (ExtractionDomain)new MeshCase(value: value));
    public static Fin<ExtractionDomain> Cloud(VectorCloud value, Op? key = null) =>
        VectorCloud.Admit(value: value, key: key.OrDefault()).Map(static valid => (ExtractionDomain)new CloudCase(value: valid));
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
    internal Fin<ExtractionDomain> Admit(Op key) =>
        Switch(
            state: key,
            supportCase: static (op, domain) => Support(value: domain.Value, key: op),
            meshCase: static (op, domain) => Mesh(value: domain.Value, key: op),
            cloudCase: static (op, domain) => Cloud(value: domain.Value, key: op));
    internal Fin<CurveBatch> Contours(ContourPolicy policy, Context context, Op key) =>
        Switch(
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
            planeCase: static (state, p) => AcceptCurves(curves: Brep.CreateContourCurves(brepToContour: state.Brep, sectionPlane: p.Section), status: ExtractionStatus.Complete, nativeRouted: true, toleranceSource: ToleranceSource.RhinoDefault, key: state.Key),
            axisCase: static (state, p) => AcceptCurves(curves: Brep.CreateContourCurves(brepToContour: state.Brep, contourStart: p.Start, contourEnd: p.End, interval: p.Interval.Value), status: ExtractionStatus.Complete, nativeRouted: true, toleranceSource: ToleranceSource.RhinoDefault, key: state.Key),
            surfaceIsoCase: static (state, _) => Fin.Fail<CurveBatch>(error: state.Key.Unsupported(geometryType: typeof(Brep), outputType: typeof(ContourPolicy.SurfaceIsoCase))),
            meshScalarCase: static (state, _) => Fin.Fail<CurveBatch>(error: state.Key.Unsupported(geometryType: typeof(Brep), outputType: typeof(ContourPolicy.MeshScalarCase)))));
    private static Fin<CurveBatch> CurvesFromMesh(Mesh mesh, ContourPolicy policy, Context context, Op key) =>
        key.Catch(() => policy.Switch(
            state: (Mesh: mesh, Context: context, Key: key),
            planeCase: static (state, p) => AcceptCurves(curves: Rhino.Geometry.Mesh.CreateContourCurves(meshToContour: state.Mesh, sectionPlane: p.Section, tolerance: state.Context.Absolute.Value), status: ExtractionStatus.Complete, nativeRouted: true, tolerance: Some(state.Context.Absolute.Value), key: state.Key),
            axisCase: static (state, p) => AcceptCurves(curves: Rhino.Geometry.Mesh.CreateContourCurves(meshToContour: state.Mesh, contourStart: p.Start, contourEnd: p.End, interval: p.Interval.Value, tolerance: state.Context.Absolute.Value), status: ExtractionStatus.Complete, nativeRouted: true, tolerance: Some(state.Context.Absolute.Value), key: state.Key),
            meshScalarCase: static (state, p) => ScalarIsolinesDetailed(mesh: state.Mesh, values: p.Values, levels: p.Levels, context: state.Context, key: state.Key)
                .Bind(result => AcceptCurves(curves: result.Curves, attempted: result.Receipt.StitchedCandidates, status: ExtractionStatus.Complete, nativeRouted: false, tolerance: Some(state.Context.Absolute.Value), scalarIsoline: Some(result), key: state.Key)),
            surfaceIsoCase: static (state, _) => Fin.Fail<CurveBatch>(error: state.Key.Unsupported(geometryType: typeof(Mesh), outputType: typeof(ContourPolicy.SurfaceIsoCase)))));
    // BOUNDARY ADAPTER — scalar contouring is absent from RhinoCommon; this local PL kernel
    // follows Rhino's triangulated mesh topology and returns stitched curve candidates only.
    private static Fin<ScalarIsolineResult> ScalarIsolinesDetailed(Mesh mesh, Arr<double> values, Seq<double> levels, Context context, Op key) {
        if (values.Count != mesh.Vertices.Count || values.Exists(static value => !RhinoMath.IsValidDouble(x: value)) || levels.IsEmpty || levels.Exists(static value => !RhinoMath.IsValidDouble(x: value)))
            return Fin.Fail<ScalarIsolineResult>(key.InvalidInput());
        using Mesh triangulated = mesh.DuplicateMesh();
        if (triangulated.Faces.QuadCount > 0 && !triangulated.Faces.ConvertQuadsToTriangles()) return Fin.Fail<ScalarIsolineResult>(key.InvalidResult());
        if (triangulated.Vertices.Count != values.Count) return Fin.Fail<ScalarIsolineResult>(key.InvalidResult());
        List<ScalarIsolineSegment> segments = [];
        ScalarIsolineStats stats = new();
        for (int f = 0; f < triangulated.Faces.Count; f++) {
            MeshFace face = triangulated.Faces[index: f];
            if (face.IsTriangle) AddFaceIsolines(mesh: triangulated, face: face, values: values, levels: levels, tolerance: context.Absolute.Value, segments: segments, stats: stats);
        }
        Seq<ScalarIsolineSegment> deduped = DeduplicateSegments(segments: segments, tolerance: context.Absolute.Value, stats: stats);
        Seq<Curve> curves = StitchSegments(segments: deduped, tolerance: context.Absolute.Value, stats: stats);
        return key.AcceptValue(value: new ScalarIsolineResult(Curves: curves, Receipt: new ScalarIsolineReceipt(Route: ScalarIsolineRoute.LocalPiecewiseLinearMesh, FiniteLevels: levels.Count, RawSegments: stats.RawSegments, DedupedSegments: stats.DedupedSegments, DegenerateRejected: stats.DegenerateRejected, PlateauRejected: stats.PlateauRejected, StitchedCandidates: stats.StitchedCandidates, BranchStops: stats.BranchStops, BranchNodes: stats.BranchNodes, MaxIncidentSegments: stats.MaxIncidentSegments, EmittedCurves: stats.EmittedCurves)));
    }
    private static void AddFaceIsolines(Mesh mesh, MeshFace face, Arr<double> values, Seq<double> levels, double tolerance, List<ScalarIsolineSegment> segments, ScalarIsolineStats stats) {
        Point3d[] points = [mesh.Vertices[index: face.A], mesh.Vertices[index: face.B], mesh.Vertices[index: face.C]];
        double[] scalars = [values[index: face.A], values[index: face.B], values[index: face.C]];
        (int A, int B)[] edges = [(0, 1), (1, 2), (2, 0)];
        foreach (double level in levels.AsIterable()) {
            double epsilon = RhinoMath.SqrtEpsilon * Math.Max(val1: 1.0, val2: Math.Max(val1: Math.Abs(value: level), val2: scalars.Max(static value => Math.Abs(value: value))));
            if (scalars.All(value => Math.Abs(value: value - level) <= epsilon)) { stats.PlateauRejected++; continue; }
            ((int A, int B) Edge, double ADelta, double BDelta)[] cuts = System.Array.ConvertAll(array: edges, converter: edge => (edge, scalars[edge.A] - level, scalars[edge.B] - level));
            ScalarIsolineSegment[] edgeSegments = System.Array.ConvertAll(
                array: System.Array.FindAll(array: cuts, match: cut => Math.Abs(value: cut.ADelta) <= epsilon && Math.Abs(value: cut.BDelta) <= epsilon),
                converter: cut => new ScalarIsolineSegment(A: points[cut.Edge.A], B: points[cut.Edge.B]));
            segments.AddRange(collection: edgeSegments);
            stats.RawSegments += edgeSegments.Length;
            Point3d[] unique = [.. Enumerable.SelectMany<((int A, int B) Edge, double ADelta, double BDelta), Point3d>(cuts, cut =>
                    (Math.Abs(value: cut.ADelta) <= epsilon, Math.Abs(value: cut.BDelta) <= epsilon, cut.ADelta * cut.BDelta < 0.0) switch {
                        (true, true, _) => [],
                        (true, false, _) => [points[cut.Edge.A]],
                        (false, true, _) => [points[cut.Edge.B]],
                        (false, false, true) => [points[cut.Edge.A] + (-cut.ADelta / (cut.BDelta - cut.ADelta) * (points[cut.Edge.B] - points[cut.Edge.A]))],
                        _ => [],
                    })
                .Where(predicate: static point => point.IsValid)
                .DistinctBy(keySelector: point => KeyOf(point: point, tolerance: tolerance))];
            if (unique.Length == 2) { segments.Add(item: new ScalarIsolineSegment(A: unique[0], B: unique[1])); stats.RawSegments++; }
        }
    }
    private static Seq<ScalarIsolineSegment> DeduplicateSegments(List<ScalarIsolineSegment> segments, double tolerance, ScalarIsolineStats stats) {
        System.Collections.Generic.HashSet<(ScalarIsolinePointKey A, ScalarIsolinePointKey B)> seen = [];
        List<ScalarIsolineSegment> unique = [];
        foreach (ScalarIsolineSegment segment in segments) {
            ScalarIsolinePointKey a = KeyOf(point: segment.A, tolerance: tolerance);
            ScalarIsolinePointKey b = KeyOf(point: segment.B, tolerance: tolerance);
            if (a.Equals(b)) { stats.DegenerateRejected++; continue; }
            (ScalarIsolinePointKey A, ScalarIsolinePointKey B) edge = a.Compare(other: b) <= 0 ? (a, b) : (b, a);
            if (seen.Add(item: edge)) unique.Add(item: segment);
        }
        stats.DedupedSegments = unique.Count;
        return toSeq(unique);
    }
    private static Seq<Curve> StitchSegments(Seq<ScalarIsolineSegment> segments, double tolerance, ScalarIsolineStats stats) {
        ScalarIsolineSegment[] all = [.. segments.AsIterable()];
        bool[] used = new bool[all.Length];
        Dictionary<ScalarIsolinePointKey, List<int>> incident = [];
        for (int i = 0; i < all.Length; i++) {
            ref List<int>? a = ref CollectionsMarshal.GetValueRefOrAddDefault(dictionary: incident, key: KeyOf(point: all[i].A, tolerance: tolerance), exists: out _);
            ref List<int>? b = ref CollectionsMarshal.GetValueRefOrAddDefault(dictionary: incident, key: KeyOf(point: all[i].B, tolerance: tolerance), exists: out _);
            (a ??= []).Add(item: i);
            (b ??= []).Add(item: i);
        }
        (int maxIncidentSegments, int branchNodes) = incident.Values
            .Select(static edges => edges.Count)
            .Aggregate(
                seed: (Max: stats.MaxIncidentSegments, Branches: stats.BranchNodes),
                func: static (state, count) => (Max: Math.Max(val1: state.Max, val2: count), Branches: state.Branches + (count > 2 ? 1 : 0)));
        stats.MaxIncidentSegments = maxIncidentSegments;
        stats.BranchNodes = branchNodes;
        List<Curve> curves = [];
        int attempted = 0;
        for (int i = 0; i < all.Length; i++) {
            if (used[i]) continue;
            List<Point3d> points = [all[i].A, all[i].B];
            used[i] = true;
            Extend(points: points, atEnd: true, all: all, used: used, incident: incident, tolerance: tolerance, stats: stats);
            Extend(points: points, atEnd: false, all: all, used: used, incident: incident, tolerance: tolerance, stats: stats);
            Polyline polyline = [.. points];
            attempted++;
            if (polyline.IsValid && polyline.Count >= 2) curves.Add(item: polyline.ToPolylineCurve());
        }
        stats.StitchedCandidates = attempted;
        stats.EmittedCurves = curves.Count;
        return toSeq(curves);
    }
    private static void Extend(List<Point3d> points, bool atEnd, ScalarIsolineSegment[] all, bool[] used, Dictionary<ScalarIsolinePointKey, List<int>> incident, double tolerance, ScalarIsolineStats stats) {
        bool moved = true;
        while (moved) {
            moved = false;
            Point3d anchor = atEnd ? points[^1] : points[index: 0];
            ScalarIsolinePointKey key = KeyOf(point: anchor, tolerance: tolerance);
            if (!incident.TryGetValue(key: key, value: out List<int>? candidates)) continue;
            foreach (int index in candidates) {
                if (used[index]) continue;
                ScalarIsolineSegment segment = all[index];
                ScalarIsolinePointKey a = KeyOf(point: segment.A, tolerance: tolerance);
                Point3d next = a.Equals(key) ? segment.B : segment.A;
                int available = candidates.Count(candidate => !used[candidate]);
                stats.BranchStops += available > 1 ? 1 : 0;
                if (available == 1) {
                    points.Insert(index: atEnd ? points.Count : 0, item: next);
                    used[index] = true;
                    moved = true;
                }
                break;
            }
        }
    }
    private static ScalarIsolinePointKey KeyOf(Point3d point, double tolerance) {
        double scale = 1.0 / Math.Max(val1: tolerance, val2: RhinoMath.SqrtEpsilon);
        return new ScalarIsolinePointKey(
            X: (long)Math.Round(point.X * scale, MidpointRounding.ToEven),
            Y: (long)Math.Round(point.Y * scale, MidpointRounding.ToEven),
            Z: (long)Math.Round(point.Z * scale, MidpointRounding.ToEven));
    }
    private static Fin<CurveBatch> CurvesFromCloud(VectorCloud.ClusterCase cloud, ContourPolicy policy, Context context, Op key) =>
        key.Catch(() => policy.Switch(
            state: (Cloud: cloud, Context: context, Key: key),
            axisCase: static (state, p) => AcceptCurves(curves: state.Cloud.Indexed.CreateContourCurves(contourStart: p.Start, contourEnd: p.End, interval: p.Interval.Value, absoluteTolerance: state.Context.Absolute.Value), status: ExtractionStatus.Approximate, nativeRouted: true, tolerance: Some(state.Context.Absolute.Value), key: state.Key),
            planeCase: static (state, p) => AcceptCurves(curves: state.Cloud.Indexed.CreateSectionCurve(plane: p.Section, absoluteTolerance: state.Context.Absolute.Value), status: ExtractionStatus.Approximate, nativeRouted: true, tolerance: Some(state.Context.Absolute.Value), key: state.Key),
            surfaceIsoCase: static (state, _) => Fin.Fail<CurveBatch>(error: state.Key.Unsupported(geometryType: typeof(PointCloud), outputType: typeof(ContourPolicy.SurfaceIsoCase))),
            meshScalarCase: static (state, _) => Fin.Fail<CurveBatch>(error: state.Key.Unsupported(geometryType: typeof(PointCloud), outputType: typeof(ContourPolicy.MeshScalarCase)))));
    private static Fin<CurveBatch> CurvesFromSurface(Surface surface, ContourPolicy policy, Op key) =>
        key.Catch(() => policy.Switch(
            state: (Surface: surface, Key: key),
            surfaceIsoCase: static (state, p) => state.Surface is BrepFace face
                ? BrepFaceIsoCurve(face: face, status: p.Status, parameter: p.Parameter, key: state.Key)
                    .Bind(curve => AcceptCurves(curves: curve, status: ExtractionStatus.Complete, nativeRouted: true, key: state.Key))
                : Optional(p.Status is IsoStatus.North or IsoStatus.East or IsoStatus.South or IsoStatus.West
                        ? state.Surface.IsoCurve(iso: p.Status)
                        : state.Surface.IsoCurve(iso: p.Status, t: p.Parameter))
                    .ToFin(state.Key.InvalidResult())
                    .Bind(curve => AcceptCurves(curves: [curve], status: ExtractionStatus.Complete, nativeRouted: true, key: state.Key)),
            planeCase: static (state, _) => Fin.Fail<CurveBatch>(error: state.Key.Unsupported(geometryType: typeof(Surface), outputType: typeof(ContourPolicy.PlaneCase))),
            axisCase: static (state, _) => Fin.Fail<CurveBatch>(error: state.Key.Unsupported(geometryType: typeof(Surface), outputType: typeof(ContourPolicy.AxisCase))),
            meshScalarCase: static (state, _) => Fin.Fail<CurveBatch>(error: state.Key.Unsupported(geometryType: typeof(Surface), outputType: typeof(ContourPolicy.MeshScalarCase)))));
    private static Fin<Curve[]> BrepFaceIsoCurve(BrepFace face, IsoStatus status, double parameter, Op key) =>
        status switch {
            IsoStatus.X => AcceptTrimAwareIso(face: face, direction: 1, parameter: parameter, key: key),
            IsoStatus.Y => AcceptTrimAwareIso(face: face, direction: 0, parameter: parameter, key: key),
            IsoStatus.West => AcceptTrimAwareIso(face: face, direction: 1, parameter: face.Domain(direction: 0).T0, key: key),
            IsoStatus.East => AcceptTrimAwareIso(face: face, direction: 1, parameter: face.Domain(direction: 0).T1, key: key),
            IsoStatus.South => AcceptTrimAwareIso(face: face, direction: 0, parameter: face.Domain(direction: 1).T0, key: key),
            IsoStatus.North => AcceptTrimAwareIso(face: face, direction: 0, parameter: face.Domain(direction: 1).T1, key: key),
            _ => Fin.Fail<Curve[]>(key.Unsupported(geometryType: typeof(BrepFace), outputType: typeof(ContourPolicy.SurfaceIsoCase))),
        };
    private static Fin<Curve[]> AcceptTrimAwareIso(BrepFace face, int direction, double parameter, Op key) =>
        RhinoMath.IsValidDouble(x: parameter)
            ? Optional(face.TrimAwareIsoCurve(direction: direction, constantParameter: parameter)).ToFin(key.InvalidResult())
            : Fin.Fail<Curve[]>(key.InvalidInput());
    private static Fin<CurveBatch> AcceptCurves(Curve[] curves, ExtractionStatus status, bool nativeRouted, Op key, Option<double> tolerance = default, ToleranceSource? toleranceSource = null) =>
        Optional(curves).ToFin(key.InvalidResult())
            .Bind(active => AcceptCurves(curves: toSeq(active), attempted: active.Length, status: status, nativeRouted: nativeRouted, key: key, tolerance: tolerance, toleranceSource: toleranceSource));
    private static Fin<CurveBatch> AcceptCurves(Seq<Curve> curves, int attempted, ExtractionStatus status, bool nativeRouted, Op key, Option<double> tolerance = default, bool parallelCallback = false, ToleranceSource? toleranceSource = null, Option<ScalarIsolineResult> scalarIsoline = default) {
        Seq<Curve> accepted = curves.Filter(static curve => curve is not null && curve.IsValid);
        int emitted = accepted.Count;
        return ExtractionReceipt.Of(status: status, attempted: attempted, emitted: emitted, nativeRouted: nativeRouted, toleranceSource: toleranceSource ?? (tolerance.IsSome ? ToleranceSource.Context : ToleranceSource.NotApplicable), tolerance: tolerance, parallelCallback: parallelCallback, key: key, scalarIsoline: scalarIsoline.Map(static result => result.Receipt))
            .Map(receipt => new CurveBatch(Curves: accepted, ScalarIsoline: scalarIsoline, Receipt: receipt));
    }
}

internal readonly record struct CurveBatch(Seq<Curve> Curves, Option<ScalarIsolineResult> ScalarIsoline, ExtractionReceipt Receipt);

[Union]
public abstract partial record ExtractionProbe {
    private ExtractionProbe() { }
    public sealed record VectorCase(VectorField Source) : ExtractionProbe; public sealed record ScalarCase(ScalarField Source) : ExtractionProbe; public sealed record TensorCase(TensorField Source) : ExtractionProbe;
    public static ExtractionProbe Vector(VectorField source) => new VectorCase(Source: source); public static ExtractionProbe Scalar(ScalarField source) => new ScalarCase(Source: source); public static ExtractionProbe Tensor(TensorField source) => new TensorCase(Source: source);
    internal Fin<TOut> Project<TOut>(Point3d sample, Context context, Op key) =>
        Switch(
            state: (Sample: sample, Context: context, Key: key),
            vectorCase: static (state, probe) =>
                from vector in probe.Source.SampleVector(sample: state.Sample, context: state.Context, key: state.Key)
                from output in typeof(TOut) switch {
                    Type t when t == typeof(Vector3d) => Extraction.Accept<TOut, Vector3d>(value: vector, key: state.Key),
                    Type t when t == typeof(double) => Extraction.Accept<TOut, double>(value: vector.Length, key: state.Key),
                    _ => VectorSpan.Of(anchor: state.Sample, vector: vector, context: state.Context, key: state.Key).Bind(span => span.Project<TOut>(key: state.Key)),
                }
                select output,
            scalarCase: static (state, probe) =>
                from value in probe.Source.SampleScalar(sample: state.Sample, context: state.Context, key: state.Key)
                from output in typeof(TOut) == typeof(double)
                    ? Extraction.Accept<TOut, double>(value: value, key: state.Key)
                    : Fin.Fail<TOut>(error: state.Key.Unsupported(geometryType: typeof(ScalarCase), outputType: typeof(TOut)))
                select output,
            tensorCase: static (state, probe) =>
                from tensor in probe.Source.SampleTensor(sample: state.Sample, context: state.Context, key: state.Key)
                from output in typeof(TOut) switch {
                    Type t when t == typeof(SymmetricMatrix) => Fin.Succ((TOut)(object)tensor),
                    Type t when t == typeof(Seq<(double Eigenvalue, Direction Eigenvector)>) =>
                        probe.Source.PrincipalDirections(sample: state.Sample, context: state.Context, key: state.Key).Map(static p => (TOut)(object)p),
                    _ => Fin.Fail<TOut>(error: state.Key.Unsupported(geometryType: typeof(TensorCase), outputType: typeof(TOut))),
                }
                select output);
}

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
internal abstract partial record Extraction {
    private Extraction() { }
    public sealed record ProbeCase(ExtractionProbe Source, Point3d Sample) : Extraction;
    public sealed record ContourCase(ExtractionDomain Domain, ContourPolicy Policy) : Extraction;
    public sealed record IsoSurfaceCase(ScalarField Field, BoundingBox Bounds, int Resolution, int MaxRootSteps) : Extraction;
    public sealed record GlyphCase(VectorField Field, ExtractionDomain Domain, GlyphPolicy Policy) : Extraction;
    public sealed record SampleGridCase(ScalarField Field, ExtractionDomain Domain, GridPolicy Policy) : Extraction;
    public sealed record StreamBundleCase(VectorField Field, ExtractionDomain Domain, StreamBundlePolicy Policy) : Extraction;
    public static Fin<Extraction> Probe(ExtractionProbe source, Point3d sample, Op? key = null) =>
        from validSource in Optional(source).ToFin(key.OrDefault().InvalidInput())
        from validSample in key.OrDefault().AcceptValue(value: sample)
        select (Extraction)new ProbeCase(Source: validSource, Sample: validSample);
    public static Fin<Extraction> Contour(ExtractionDomain domain, ContourPolicy policy, Op? key = null) {
        Op op = key.OrDefault();
        return from validDomain in Optional(domain).ToFin(op.InvalidInput()).Bind(active => active.Admit(key: op))
               from validPolicy in Optional(policy).ToFin(op.InvalidInput()).Bind(active => active.Admit(key: op))
               select (Extraction)new ContourCase(Domain: validDomain, Policy: validPolicy);
    }
    public static Fin<Extraction> IsoSurface(ScalarField field, BoundingBox bounds, int resolution, int maxRootSteps, Op? key = null) {
        Op op = key.OrDefault();
        return from validField in Optional(field).ToFin(op.InvalidInput())
               from _ in FieldNabla.IsoSurfaceInput(bounds: bounds, resolution: resolution, maxRootSteps: maxRootSteps, key: op)
               select (Extraction)new IsoSurfaceCase(Field: validField, Bounds: bounds, Resolution: resolution, MaxRootSteps: maxRootSteps);
    }
    public static Fin<Extraction> Glyph(VectorField field, ExtractionDomain domain, GlyphPolicy policy, Op? key = null) =>
        Sampled(field: field, domain: domain, policy: policy, key: key.OrDefault(),
            admit: static (candidate, active) => GlyphPolicy.Of(kind: candidate.Kind, scale: candidate.Scale, key: active),
            create: static (source, target, active) => new GlyphCase(Field: source, Domain: target, Policy: active));
    public static Fin<Extraction> SampleGrid(ScalarField field, ExtractionDomain domain, GridPolicy policy, Op? key = null) =>
        Sampled(field: field, domain: domain, policy: policy, key: key.OrDefault(),
            admit: static (candidate, active) => GridPolicy.Of(kind: candidate.Kind, key: active),
            create: static (source, target, active) => new SampleGridCase(Field: source, Domain: target, Policy: active));
    public static Fin<Extraction> StreamBundle(VectorField field, ExtractionDomain domain, StreamBundlePolicy policy, Op? key = null) =>
        Sampled(field: field, domain: domain, policy: policy, key: key.OrDefault(),
            admit: static (candidate, active) => StreamBundlePolicy.Of(kind: candidate.Kind, initialStep: candidate.InitialStep, termination: candidate.Termination, integrator: candidate.Integrator, key: active),
            create: static (source, target, active) => new StreamBundleCase(Field: source, Domain: target, Policy: active));
    private static Fin<Extraction> Sampled<TField, TPolicy>(TField field, ExtractionDomain domain, TPolicy policy, Op key, Func<TPolicy, Op, Fin<TPolicy>> admit, Func<TField, ExtractionDomain, TPolicy, Extraction> create) where TField : class =>
        from validField in Optional(field).ToFin(key.InvalidInput())
        from validDomain in Optional(domain).ToFin(key.InvalidInput()).Bind(active => active.Admit(key: key))
        from validPolicy in admit(policy, key)
        select create(validField, validDomain, validPolicy);
    internal Fin<TOut> Project<TOut>(Context context, Op key) =>
        Switch(
            state: (Context: context, Key: key),
            probeCase: static (state, extraction) => extraction.Source.Project<TOut>(sample: extraction.Sample, context: state.Context, key: state.Key),
            contourCase: static (state, extraction) =>
                from batch in extraction.Domain.Contours(policy: extraction.Policy, context: state.Context, key: state.Key)
                from output in ProjectCurves<TOut>(batch: batch, key: state.Key)
                select output,
            isoSurfaceCase: static (state, extraction) =>
                from result in extraction.Field.IsoSurfaceAttemptDetailed(bounds: extraction.Bounds, resolution: extraction.Resolution, maxRootSteps: extraction.MaxRootSteps, context: state.Context, key: state.Key)
                from output in typeof(TOut) switch {
                    Type t when t.Equals(typeof(Mesh)) => result.Receipt.Valid ? Accept<TOut, Mesh>(value: result.Mesh, key: state.Key) : Fin.Fail<TOut>(state.Key.InvalidResult()),
                    Type t when t == typeof(IsoSurfaceReceipt) => Fin.Succ((TOut)(object)result.Receipt),
                    Type t when t == typeof(IsoSurfaceResult) => Fin.Succ((TOut)(object)result),
                    Type t when t == typeof(ExtractionReceipt) => ExtractionReceipt.Of(status: result.Receipt.Valid ? ExtractionStatus.Complete : ExtractionStatus.Approximate, attempted: 1, emitted: result.Receipt.Valid ? 1 : 0, nativeRouted: result.Receipt.NativeRouted, toleranceSource: result.Receipt.FixedTolerance.IsSome ? ToleranceSource.RhinoDefault : ToleranceSource.NotApplicable, tolerance: result.Receipt.FixedTolerance, parallelCallback: result.Receipt.ParallelCallback, key: state.Key, isoSurface: Some(result.Receipt)).Map(static receipt => (TOut)(object)receipt),
                    _ => Fin.Fail<TOut>(error: state.Key.Unsupported(geometryType: typeof(IsoSurfaceCase), outputType: typeof(TOut))),
                }
                select output,
            glyphCase: static (state, extraction) => ProjectSamples<TOut, Line, (VectorField, PositiveMagnitude)>(
                kind: extraction.Policy.Kind, domain: extraction.Domain, context: state.Context, key: state.Key, state: (extraction.Field, extraction.Policy.Scale),
                sample: static (sampleState, point, model, op) => ExtractionProbe.Vector(source: sampleState.Item1).Project<VectorSpan>(sample: point, context: model, key: op).Map(span => new Line(span.Anchor, span.Anchor + (sampleState.Item2.Value * span.Value))),
                project: static (glyphs, op) => typeof(TOut) == typeof(Seq<Line>) ? op.Accept(values: glyphs.AsIterable()).Map(static value => (TOut)(object)value) : Fin.Fail<TOut>(error: op.Unsupported(geometryType: typeof(GlyphCase), outputType: typeof(TOut)))),
            sampleGridCase: static (state, extraction) => ProjectSamples<TOut, (Point3d, double), ScalarField>(
                kind: extraction.Policy.Kind, domain: extraction.Domain, context: state.Context, key: state.Key, state: extraction.Field,
                sample: static (source, point, model, op) => source.SampleScalar(sample: point, context: model, key: op).Map(value => (Point: point, Value: value)),
                project: static (samples, op) => typeof(TOut) == typeof(Seq<(Point3d Point, double Value)>)
                    ? samples.ForAll(static sample => sample.Item1.IsValid && RhinoMath.IsValidDouble(x: sample.Item2)) ? Fin.Succ((TOut)(object)samples) : Fin.Fail<TOut>(error: op.InvalidResult())
                    : Fin.Fail<TOut>(error: op.Unsupported(geometryType: typeof(SampleGridCase), outputType: typeof(TOut)))),
            streamBundleCase: static (state, extraction) => ProjectSamples<TOut, StreamlineTrace, (VectorField, StreamBundlePolicy)>(
                kind: extraction.Policy.Kind, domain: extraction.Domain, context: state.Context, key: state.Key, state: (extraction.Field, extraction.Policy),
                sample: static (sampleState, seed, model, op) => FlowKernel.Trace<StreamlineTrace>(source: sampleState.Item1, seed: seed, initialStep: sampleState.Item2.InitialStep, integrator: sampleState.Item2.Integrator, termination: sampleState.Item2.Termination, context: model, key: op),
                project: static (traces, op) => typeof(TOut) switch {
                    Type t when t == typeof(Seq<StreamlineTrace>) => traces.TraverseM(trace => FlowKernel.ProjectTrace<StreamlineTrace>(trace: trace, key: op)).As().Map(static value => (TOut)(object)value),
                    Type t when t == typeof(Seq<Polyline>) => traces.TraverseM(trace => FlowKernel.ProjectTrace<Polyline>(trace: trace, key: op)).As().Map(static value => (TOut)(object)value),
                    Type t when t == typeof(Seq<Curve>) => traces.TraverseM(trace => FlowKernel.ProjectTrace<Curve>(trace: trace, key: op)).As().Map(static value => (TOut)(object)value),
                    _ => Fin.Fail<TOut>(error: op.Unsupported(geometryType: typeof(StreamBundleCase), outputType: typeof(TOut))),
                }));
    private static Fin<TOut> ProjectCurves<TOut>(CurveBatch batch, Op key) =>
        typeof(TOut) switch {
            Type t when t == typeof(Seq<Curve>) => key.Accept(values: batch.Curves.AsIterable()).Map(static value => (TOut)(object)value),
            Type t when t == typeof(ExtractionReceipt) => Accept<TOut, ExtractionReceipt>(value: batch.Receipt, key: key),
            Type t when t == typeof(ScalarIsolineResult) => batch.ScalarIsoline.ToFin(Fail: key.Unsupported(geometryType: typeof(ContourPolicy), outputType: typeof(ScalarIsolineResult))).Map(static value => (TOut)(object)value),
            Type t when t == typeof(ScalarIsolineReceipt) => batch.ScalarIsoline.ToFin(Fail: key.Unsupported(geometryType: typeof(ContourPolicy), outputType: typeof(ScalarIsolineReceipt))).Map(static value => (TOut)(object)value.Receipt),
            _ => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(Extraction), outputType: typeof(TOut))),
        };
    private static Fin<TOut> ProjectSamples<TOut, TItem, TState>(SampleKind kind, ExtractionDomain domain, Context context, Op key, TState state, Func<TState, Point3d, Context, Op, Fin<TItem>> sample, Func<Seq<TItem>, Op, Fin<TOut>> project) =>
        from samples in kind.Evaluate(domain: domain, context: context, key: key)
        from items in samples.Points.TraverseM(point => sample(state, point, context, key)).As()
        from output in typeof(TOut) == typeof(ExtractionReceipt)
            ? Receipt<TOut>(attempted: samples.Receipt.Attempted, emitted: items.Count, nativeRouted: false, toleranceSource: ToleranceSource.NotApplicable, tolerance: Option<double>.None, parallelCallback: false, key: key, sample: Some(samples.Receipt))
            : project(items, key)
        select output;
    internal static Fin<TOut> Accept<TOut, TValue>(TValue value, Op key) =>
        key.AcceptValue(value: value).Map(static accepted => (TOut)(object)accepted!);
    private static Fin<TOut> Receipt<TOut>(int attempted, int emitted, bool nativeRouted, ToleranceSource toleranceSource, Option<double> tolerance, bool parallelCallback, Op key, Option<SampleReceipt> sample = default) =>
        ExtractionReceipt.Of(status: ExtractionStatus.Complete, attempted: attempted, emitted: emitted, nativeRouted: nativeRouted, toleranceSource: toleranceSource, tolerance: tolerance, parallelCallback: parallelCallback, key: key, sample: sample)
            .Map(static receipt => (TOut)(object)receipt);
}
