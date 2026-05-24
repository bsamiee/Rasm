using Foundation.CSharp.Analyzers.Contracts;
using RhinoBrep = Rhino.Geometry.Brep;
using RhinoMesh = Rhino.Geometry.Mesh;

namespace Rasm.Vectors;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class ExtractionStatus {
    public static readonly ExtractionStatus Complete = new(key: 0);
    public static readonly ExtractionStatus Approximate = new(key: 1);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct ExtractionReceipt(
    ExtractionStatus Status,
    int Attempted,
    int Emitted,
    int Rejected,
    bool NativeRouted);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct GlyphPolicy(SampleKind Kind, PositiveMagnitude Scale);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct GridPolicy(SampleKind Kind);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct StreamBundlePolicy(
    SampleKind Kind,
    PositiveMagnitude InitialStep,
    FieldIntegrator Integrator,
    Termination Termination);

[Union]
public abstract partial record ContourPolicy {
    private ContourPolicy() { }
    public sealed record PlaneCase(Plane Section) : ContourPolicy;
    public sealed record AxisCase(Point3d Start, Point3d End, PositiveMagnitude Interval) : ContourPolicy;
    public sealed record SurfaceIsoCase(IsoStatus Status, double Parameter) : ContourPolicy;
    public sealed record MeshScalarCase(Arr<double> Values, Seq<double> Levels) : ContourPolicy;
    public static Fin<ContourPolicy> Plane(Plane section, Op? key = null) {
        Op op = key.OrDefault();
        return PlaneIsFinite(section: section)
            ? Fin.Succ<ContourPolicy>(new PlaneCase(Section: section))
            : Fin.Fail<ContourPolicy>(op.InvalidInput());
    }
    private static bool PlaneIsFinite(Plane section) =>
        PointIsFinite(point: section.Origin)
        && VectorIsFinite(vector: section.XAxis)
        && VectorIsFinite(vector: section.YAxis)
        && VectorIsFinite(vector: section.ZAxis)
        && section.XAxis.Length > RhinoMath.ZeroTolerance
        && section.YAxis.Length > RhinoMath.ZeroTolerance
        && section.ZAxis.Length > RhinoMath.ZeroTolerance;
    private static bool PointIsFinite(Point3d point) =>
        RhinoMath.IsValidDouble(x: point.X) && RhinoMath.IsValidDouble(x: point.Y) && RhinoMath.IsValidDouble(x: point.Z);
    private static bool VectorIsFinite(Vector3d vector) =>
        RhinoMath.IsValidDouble(x: vector.X) && RhinoMath.IsValidDouble(x: vector.Y) && RhinoMath.IsValidDouble(x: vector.Z);
    public static Fin<ContourPolicy> Axis(Point3d start, Point3d end, double interval, Op? key = null) {
        Op op = key.OrDefault();
        Vector3d vector = end - start;
        return from _ in guard(PointIsFinite(point: start) && PointIsFinite(point: end) && VectorIsFinite(vector: vector) && vector.Length > RhinoMath.ZeroTolerance, op.InvalidInput())
               from step in op.AcceptValidated<PositiveMagnitude>(candidate: interval)
               select (ContourPolicy)new AxisCase(Start: start, End: end, Interval: step);
    }
    public static Fin<ContourPolicy> SurfaceIso(IsoStatus status, double parameter, Op? key = null) {
        Op op = key.OrDefault();
        return status is IsoStatus.X or IsoStatus.Y or IsoStatus.North or IsoStatus.East or IsoStatus.South or IsoStatus.West
            && RhinoMath.IsValidDouble(x: parameter)
            ? Fin.Succ<ContourPolicy>(new SurfaceIsoCase(Status: status, Parameter: parameter))
            : Fin.Fail<ContourPolicy>(op.InvalidInput());
    }
    public static Fin<ContourPolicy> MeshScalar(Arr<double> values, Seq<double> levels, Op? key = null) {
        Op op = key.OrDefault();
        return values.Count > 0
            && !levels.IsEmpty
            && values.All(RhinoMath.IsValidDouble)
            && levels.ForAll(RhinoMath.IsValidDouble)
            ? Fin.Succ<ContourPolicy>(new MeshScalarCase(Values: values, Levels: levels))
            : Fin.Fail<ContourPolicy>(op.InvalidInput());
    }
}

[Union]
public abstract partial record ExtractionDomain {
    private ExtractionDomain() { }
    public sealed record SupportCase(SupportSpace Value) : ExtractionDomain;
    public sealed record MeshCase(MeshSpace Value) : ExtractionDomain;
    public sealed record CloudCase(VectorCloud Value) : ExtractionDomain;
    public static ExtractionDomain Support(SupportSpace value) => new SupportCase(Value: value);
    public static ExtractionDomain Mesh(MeshSpace value) => new MeshCase(Value: value);
    public static ExtractionDomain Cloud(VectorCloud value) => new CloudCase(Value: value);
    public static Fin<ExtractionDomain> Of(object? value, Context context, Op? key = null) {
        Op op = key.OrDefault();
        return Optional(value).ToFin(op.InvalidInput()).Bind(source => source switch {
            ExtractionDomain domain => Fin.Succ(domain),
            RhinoMesh mesh => MeshSpace.Of(native: mesh, context: context, key: op).Map(Mesh),
            VectorCloud cloud => Fin.Succ(Cloud(value: cloud)),
            PointCloud cloud => VectorCloud.Cluster(points: toSeq(cloud.GetPoints()), context: context, key: op).Map(Cloud),
            object candidate => SupportSpace.Of(value: candidate, key: op).Map(Support),
        });
    }
    internal Fin<CurveBatch> Contours(ContourPolicy policy, Context context, Op key) =>
        Switch(
            state: (Policy: policy, Context: context, Key: key),
            supportCase: static (state, domain) => domain.Value.Value switch {
                RhinoBrep brep => CurvesFromBrep(brep: brep, policy: state.Policy, key: state.Key),
                RhinoMesh mesh => CurvesFromMesh(mesh: mesh, policy: state.Policy, context: state.Context, key: state.Key),
                Surface surface => CurvesFromSurface(surface: surface, policy: state.Policy, key: state.Key),
                VectorCloud.ClusterCase cloud => CurvesFromCloud(cloud: cloud, policy: state.Policy, context: state.Context, key: state.Key),
                _ => Fin.Fail<CurveBatch>(error: state.Key.Unsupported(geometryType: domain.Value.SourceType, outputType: typeof(Seq<Curve>))),
            },
            meshCase: static (state, domain) => CurvesFromMesh(mesh: domain.Value.Native, policy: state.Policy, context: state.Context, key: state.Key),
            cloudCase: static (state, domain) => domain.Value is VectorCloud.ClusterCase cloud
                ? CurvesFromCloud(cloud: cloud, policy: state.Policy, context: state.Context, key: state.Key)
                : Fin.Fail<CurveBatch>(error: state.Key.Unsupported(geometryType: domain.Value.GetType(), outputType: typeof(Seq<Curve>))));
    private static Fin<CurveBatch> CurvesFromBrep(RhinoBrep brep, ContourPolicy policy, Op key) =>
        key.Catch(() => policy.Switch(
            state: (Brep: brep, Key: key),
            planeCase: static (state, p) => AcceptCurves(curves: RhinoBrep.CreateContourCurves(brepToContour: state.Brep, sectionPlane: p.Section), status: ExtractionStatus.Complete, nativeRouted: true, key: state.Key),
            axisCase: static (state, p) => AcceptCurves(curves: RhinoBrep.CreateContourCurves(brepToContour: state.Brep, contourStart: p.Start, contourEnd: p.End, interval: p.Interval.Value), status: ExtractionStatus.Complete, nativeRouted: true, key: state.Key),
            surfaceIsoCase: static (state, _) => Fin.Fail<CurveBatch>(error: state.Key.Unsupported(geometryType: typeof(RhinoBrep), outputType: typeof(ContourPolicy.SurfaceIsoCase))),
            meshScalarCase: static (state, _) => Fin.Fail<CurveBatch>(error: state.Key.Unsupported(geometryType: typeof(RhinoBrep), outputType: typeof(ContourPolicy.MeshScalarCase)))));
    private static Fin<CurveBatch> CurvesFromMesh(RhinoMesh mesh, ContourPolicy policy, Context context, Op key) =>
        key.Catch(() => policy.Switch(
            state: (Mesh: mesh, Context: context, Key: key),
            planeCase: static (state, p) => AcceptCurves(curves: RhinoMesh.CreateContourCurves(meshToContour: state.Mesh, sectionPlane: p.Section, tolerance: state.Context.Absolute.Value), status: ExtractionStatus.Complete, nativeRouted: true, key: state.Key),
            axisCase: static (state, p) => AcceptCurves(curves: RhinoMesh.CreateContourCurves(meshToContour: state.Mesh, contourStart: p.Start, contourEnd: p.End, interval: p.Interval.Value, tolerance: state.Context.Absolute.Value), status: ExtractionStatus.Complete, nativeRouted: true, key: state.Key),
            meshScalarCase: static (state, p) => MeshKernel.ScalarIsolines(mesh: state.Mesh, values: p.Values, levels: p.Levels, context: state.Context, key: state.Key)
                .Bind(result => AcceptCurves(curves: result.Curves, attempted: result.Attempted, status: ExtractionStatus.Complete, nativeRouted: false, key: state.Key)),
            surfaceIsoCase: static (state, _) => Fin.Fail<CurveBatch>(error: state.Key.Unsupported(geometryType: typeof(RhinoMesh), outputType: typeof(ContourPolicy.SurfaceIsoCase)))));
    private static Fin<CurveBatch> CurvesFromCloud(VectorCloud.ClusterCase cloud, ContourPolicy policy, Context context, Op key) =>
        key.Catch(() => policy.Switch(
            state: (Cloud: cloud, Context: context, Key: key),
            axisCase: static (state, p) => AcceptCurves(curves: state.Cloud.Indexed.CreateContourCurves(contourStart: p.Start, contourEnd: p.End, interval: p.Interval.Value, absoluteTolerance: state.Context.Absolute.Value), status: ExtractionStatus.Approximate, nativeRouted: true, key: state.Key),
            planeCase: static (state, p) => AcceptCurves(curves: state.Cloud.Indexed.CreateSectionCurve(plane: p.Section, absoluteTolerance: state.Context.Absolute.Value), status: ExtractionStatus.Approximate, nativeRouted: true, key: state.Key),
            surfaceIsoCase: static (state, _) => Fin.Fail<CurveBatch>(error: state.Key.Unsupported(geometryType: typeof(PointCloud), outputType: typeof(ContourPolicy.SurfaceIsoCase))),
            meshScalarCase: static (state, _) => Fin.Fail<CurveBatch>(error: state.Key.Unsupported(geometryType: typeof(PointCloud), outputType: typeof(ContourPolicy.MeshScalarCase)))));
    private static Fin<CurveBatch> CurvesFromSurface(Surface surface, ContourPolicy policy, Op key) =>
        key.Catch(() => policy.Switch(
            state: (Surface: surface, Key: key),
            surfaceIsoCase: static (state, p) => state.Surface is BrepFace face
                ? p.Status is IsoStatus.X or IsoStatus.Y
                    ? AcceptCurves(curves: face.TrimAwareIsoCurve(direction: p.Status is IsoStatus.X ? 1 : 0, constantParameter: p.Parameter), status: ExtractionStatus.Complete, nativeRouted: true, key: state.Key)
                    : Fin.Fail<CurveBatch>(state.Key.Unsupported(geometryType: typeof(BrepFace), outputType: typeof(ContourPolicy.SurfaceIsoCase)))
                : Optional(state.Surface.IsoCurve(iso: p.Status, t: p.Parameter))
                    .ToFin(state.Key.InvalidResult())
                    .Bind(curve => AcceptCurves(curves: [curve], status: ExtractionStatus.Complete, nativeRouted: true, key: state.Key)),
            planeCase: static (state, _) => Fin.Fail<CurveBatch>(error: state.Key.Unsupported(geometryType: typeof(Surface), outputType: typeof(ContourPolicy.PlaneCase))),
            axisCase: static (state, _) => Fin.Fail<CurveBatch>(error: state.Key.Unsupported(geometryType: typeof(Surface), outputType: typeof(ContourPolicy.AxisCase))),
            meshScalarCase: static (state, _) => Fin.Fail<CurveBatch>(error: state.Key.Unsupported(geometryType: typeof(Surface), outputType: typeof(ContourPolicy.MeshScalarCase)))));
    private static Fin<CurveBatch> AcceptCurves(Curve[] curves, ExtractionStatus status, bool nativeRouted, Op key) =>
        Optional(curves).ToFin(key.InvalidResult())
            .Bind(active => AcceptCurves(curves: toSeq(active), attempted: active.Length, status: status, nativeRouted: nativeRouted, key: key));
    private static Fin<CurveBatch> AcceptCurves(Seq<Curve> curves, int attempted, ExtractionStatus status, bool nativeRouted, Op key) {
        Seq<Curve> accepted = curves.Filter(static curve => curve is not null && curve.IsValid);
        int emitted = accepted.Count;
        int rejected = Math.Max(val1: 0, val2: attempted - emitted);
        return emitted + rejected == attempted
            ? Fin.Succ(new CurveBatch(Curves: accepted, Receipt: new ExtractionReceipt(Status: status, Attempted: attempted, Emitted: emitted, Rejected: rejected, NativeRouted: nativeRouted)))
            : Fin.Fail<CurveBatch>(key.InvalidResult());
    }
}

internal readonly record struct CurveBatch(Seq<Curve> Curves, ExtractionReceipt Receipt);

[Union]
public abstract partial record ExtractionProbe {
    private ExtractionProbe() { }
    public sealed record VectorCase(VectorField Source) : ExtractionProbe;
    public sealed record ScalarCase(ScalarField Source) : ExtractionProbe;
    public sealed record TensorCase(TensorField Source) : ExtractionProbe;
    public sealed record MeshScalarCase(ScalarField Source) : ExtractionProbe;
    public static ExtractionProbe Vector(VectorField source) => new VectorCase(Source: source);
    public static ExtractionProbe Scalar(ScalarField source) => new ScalarCase(Source: source);
    public static ExtractionProbe Tensor(TensorField source) => new TensorCase(Source: source);
    public static ExtractionProbe MeshScalar(ScalarField source) => new MeshScalarCase(Source: source);
    internal Fin<TOut> Project<TOut>(Point3d sample, Context context, Op key) =>
        Switch(
            state: (Sample: sample, Context: context, Key: key),
            vectorCase: static (state, probe) => probe.Source.Project<TOut>(sample: state.Sample, context: state.Context, key: state.Key),
            scalarCase: static (state, probe) => probe.Source.Project<TOut>(sample: state.Sample, context: state.Context, key: state.Key),
            tensorCase: static (state, probe) =>
                from tensor in probe.Source.SampleTensor(sample: state.Sample, context: state.Context, key: state.Key)
                from output in typeof(TOut) switch {
                    Type t when t == typeof(SymmetricMatrix) => Fin.Succ((TOut)(object)tensor),
                    Type t when t == typeof(Seq<(double Eigenvalue, Direction Eigenvector)>) =>
                        probe.Source.PrincipalDirections(sample: state.Sample, context: state.Context, key: state.Key).Map(static p => (TOut)(object)p),
                    _ => Fin.Fail<TOut>(error: state.Key.Unsupported(geometryType: typeof(TensorCase), outputType: typeof(TOut))),
                }
                select output,
            meshScalarCase: static (state, probe) => probe.Source.Project<TOut>(sample: state.Sample, context: state.Context, key: state.Key));
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
    public static Extraction Probe(ExtractionProbe source, Point3d sample) => new ProbeCase(Source: source, Sample: sample);
    public static Extraction Contour(ExtractionDomain domain, ContourPolicy policy) => new ContourCase(Domain: domain, Policy: policy);
    public static Extraction IsoSurface(ScalarField field, BoundingBox bounds, int resolution, int maxRootSteps) =>
        new IsoSurfaceCase(Field: field, Bounds: bounds, Resolution: resolution, MaxRootSteps: maxRootSteps);
    public static Extraction Glyph(VectorField field, ExtractionDomain domain, GlyphPolicy policy) =>
        new GlyphCase(Field: field, Domain: domain, Policy: policy);
    public static Extraction SampleGrid(ScalarField field, ExtractionDomain domain, GridPolicy policy) =>
        new SampleGridCase(Field: field, Domain: domain, Policy: policy);
    public static Extraction StreamBundle(VectorField field, ExtractionDomain domain, StreamBundlePolicy policy) =>
        new StreamBundleCase(Field: field, Domain: domain, Policy: policy);
    internal Fin<TOut> Project<TOut>(Context context, Op key) =>
        Switch(
            state: (Context: context, Key: key),
            probeCase: static (state, extraction) => extraction.Source.Project<TOut>(sample: extraction.Sample, context: state.Context, key: state.Key),
            contourCase: static (state, extraction) =>
                from batch in extraction.Domain.Contours(policy: extraction.Policy, context: state.Context, key: state.Key)
                from output in ProjectCurves<TOut>(batch: batch, key: state.Key)
                select output,
            isoSurfaceCase: static (state, extraction) =>
                from mesh in extraction.Field.IsoSurface(bounds: extraction.Bounds, resolution: extraction.Resolution, maxRootSteps: extraction.MaxRootSteps, context: state.Context, key: state.Key)
                from output in typeof(TOut) switch {
                    Type t when t == typeof(RhinoMesh) => state.Key.AcceptValue(value: mesh).Map(static value => (TOut)(object)value),
                    Type t when t == typeof(ExtractionReceipt) => Receipt<TOut>(status: ExtractionStatus.Complete, attempted: 1, emitted: 1, rejected: 0, nativeRouted: true, key: state.Key),
                    _ => Fin.Fail<TOut>(error: state.Key.Unsupported(geometryType: typeof(IsoSurfaceCase), outputType: typeof(TOut))),
                }
                select output,
            glyphCase: static (state, extraction) => ProjectGlyphs<TOut>(field: extraction.Field, domain: extraction.Domain, policy: extraction.Policy, context: state.Context, key: state.Key),
            sampleGridCase: static (state, extraction) => ProjectGrid<TOut>(field: extraction.Field, domain: extraction.Domain, policy: extraction.Policy, context: state.Context, key: state.Key),
            streamBundleCase: static (state, extraction) => ProjectBundle<TOut>(field: extraction.Field, domain: extraction.Domain, policy: extraction.Policy, context: state.Context, key: state.Key));
    private static Fin<TOut> ProjectCurves<TOut>(CurveBatch batch, Op key) =>
        typeof(TOut) switch {
            Type t when t == typeof(Seq<Curve>) => batch.Curves.TraverseM(curve => key.AcceptValue(value: curve)).As().Map(static value => (TOut)(object)value),
            Type t when t == typeof(ExtractionReceipt) => key.AcceptValue(value: batch.Receipt).Map(static value => (TOut)(object)value),
            _ => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(Extraction), outputType: typeof(TOut))),
        };
    private static Fin<TOut> ProjectGlyphs<TOut>(VectorField field, ExtractionDomain domain, GlyphPolicy policy, Context context, Op key) =>
        from samples in policy.Kind.Evaluate(domain: domain, context: context, key: key)
        from glyphs in samples.Points.TraverseM(point =>
                field.Project<VectorSpan>(sample: point, context: context, key: key)
                    .Map(span => new Line(span.Anchor, span.Anchor + (policy.Scale.Value * span.Value))))
            .As()
        from output in typeof(TOut) switch {
            Type t when t == typeof(Seq<Line>) => glyphs.TraverseM(line => key.AcceptValue(value: line)).As().Map(static value => (TOut)(object)value),
            Type t when t == typeof(ExtractionReceipt) => Receipt<TOut>(status: ExtractionStatus.Complete, attempted: samples.Receipt.Attempted, emitted: glyphs.Count, rejected: samples.Receipt.Attempted - glyphs.Count, nativeRouted: false, key: key),
            _ => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(GlyphCase), outputType: typeof(TOut))),
        }
        select output;
    private static Fin<TOut> ProjectGrid<TOut>(ScalarField field, ExtractionDomain domain, GridPolicy policy, Context context, Op key) =>
        from points in policy.Kind.Evaluate(domain: domain, context: context, key: key)
        from samples in points.Points.TraverseM(point =>
                field.SampleScalar(sample: point, context: context, key: key)
                    .Map(value => (Point: point, Value: value)))
            .As()
        from output in typeof(TOut) switch {
            Type t when t == typeof(Seq<(Point3d Point, double Value)>) => samples.ForAll(static sample => sample.Point.IsValid && RhinoMath.IsValidDouble(x: sample.Value))
                ? Fin.Succ((TOut)(object)samples)
                : Fin.Fail<TOut>(error: key.InvalidResult()),
            Type t when t == typeof(ExtractionReceipt) => Receipt<TOut>(status: ExtractionStatus.Complete, attempted: points.Receipt.Attempted, emitted: samples.Count, rejected: points.Receipt.Attempted - samples.Count, nativeRouted: false, key: key),
            _ => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(SampleGridCase), outputType: typeof(TOut))),
        }
        select output;
    private static Fin<TOut> ProjectBundle<TOut>(VectorField field, ExtractionDomain domain, StreamBundlePolicy policy, Context context, Op key) =>
        from seeds in policy.Kind.Evaluate(domain: domain, context: context, key: key)
        from traces in seeds.Points.TraverseM(seed =>
                FlowKernel.Trace<StreamlineTrace>(
                    source: field,
                    seed: seed,
                    initialStep: policy.InitialStep,
                    integrator: policy.Integrator,
                    termination: policy.Termination,
                    context: context,
                    key: key))
            .As()
        from output in typeof(TOut) switch {
            Type t when t == typeof(Seq<StreamlineTrace>) => traces.TraverseM(trace => FlowKernel.ProjectTrace<StreamlineTrace>(trace: trace, key: key)).As().Map(static value => (TOut)(object)value),
            Type t when t == typeof(Seq<Polyline>) => traces.TraverseM(trace => FlowKernel.ProjectTrace<Polyline>(trace: trace, key: key)).As().Map(static value => (TOut)(object)value),
            Type t when t == typeof(Seq<Curve>) => traces.TraverseM(trace => FlowKernel.ProjectTrace<Curve>(trace: trace, key: key)).As().Map(static value => (TOut)(object)value),
            Type t when t == typeof(ExtractionReceipt) => Receipt<TOut>(status: ExtractionStatus.Complete, attempted: seeds.Receipt.Attempted, emitted: traces.Count, rejected: seeds.Receipt.Attempted - traces.Count, nativeRouted: false, key: key),
            _ => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(StreamBundleCase), outputType: typeof(TOut))),
        }
        select output;
    private static Fin<TOut> Receipt<TOut>(ExtractionStatus status, int attempted, int emitted, int rejected, bool nativeRouted, Op key) =>
        attempted < 0 || emitted < 0 || rejected < 0 || emitted + rejected != attempted
            ? Fin.Fail<TOut>(error: key.InvalidResult())
            : Fin.Succ((TOut)(object)new ExtractionReceipt(
                Status: status,
                Attempted: attempted,
                Emitted: emitted,
                Rejected: rejected,
                NativeRouted: nativeRouted));
}
