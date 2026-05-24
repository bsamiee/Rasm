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
public readonly record struct GlyphPolicy(Seq<Point3d> Samples, PositiveMagnitude Scale);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct GridPolicy(Seq<Point3d> Samples);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct StreamBundlePolicy(
    Seq<Point3d> Seeds,
    PositiveMagnitude InitialStep,
    FieldIntegrator Integrator,
    Termination Termination);

[Union]
public abstract partial record ContourPolicy {
    private ContourPolicy() { }
    public sealed record PlaneCase(Plane Section) : ContourPolicy;
    public sealed record AxisCase(Point3d Start, Point3d End, PositiveMagnitude Interval) : ContourPolicy;
    public sealed record IsoCase(int Direction, double Parameter) : ContourPolicy;
    public static ContourPolicy Plane(Plane section) => new PlaneCase(Section: section);
    public static Fin<ContourPolicy> Axis(Point3d start, Point3d end, double interval, Op? key = null) {
        Op op = key.OrDefault();
        Vector3d vector = end - start;
        return from _ in guard(start.IsValid && end.IsValid && vector.IsValid && !vector.IsTiny(), op.InvalidInput())
               from step in op.AcceptValidated<PositiveMagnitude>(candidate: interval)
               select (ContourPolicy)new AxisCase(Start: start, End: end, Interval: step);
    }
    public static Fin<ContourPolicy> Iso(int direction, double parameter, Op? key = null) {
        Op op = key.OrDefault();
        return direction is 0 or 1 && RhinoMath.IsValidDouble(x: parameter)
            ? Fin.Succ<ContourPolicy>(new IsoCase(Direction: direction, Parameter: parameter))
            : Fin.Fail<ContourPolicy>(op.InvalidInput());
    }
}

[Union]
public abstract partial record ExtractionDomain {
    private ExtractionDomain() { }
    public sealed record SupportCase(SupportSpace Value) : ExtractionDomain;
    public sealed record MeshCase(MeshSpace Value) : ExtractionDomain;
    public sealed record CloudCase(VectorCloud Value) : ExtractionDomain;
    public sealed record CurveCase(Curve Value) : ExtractionDomain;
    public sealed record SurfaceCase(Surface Value) : ExtractionDomain;
    public sealed record BrepCase(RhinoBrep Value) : ExtractionDomain;
    public static ExtractionDomain Support(SupportSpace value) => new SupportCase(Value: value);
    public static ExtractionDomain Mesh(MeshSpace value) => new MeshCase(Value: value);
    public static ExtractionDomain Cloud(VectorCloud value) => new CloudCase(Value: value);
    public static ExtractionDomain Curve(Curve value) => new CurveCase(Value: value);
    public static ExtractionDomain Surface(Surface value) => new SurfaceCase(Value: value);
    public static ExtractionDomain Brep(RhinoBrep value) => new BrepCase(Value: value);
    internal Fin<Seq<Curve>> Contours(ContourPolicy policy, Context context, Op key) =>
        Switch(
            state: (Policy: policy, Context: context, Key: key),
            supportCase: static (state, domain) => domain.Value.Value switch {
                RhinoBrep brep => CurvesFromBrep(brep: brep, policy: state.Policy, context: state.Context, key: state.Key),
                RhinoMesh mesh => CurvesFromMesh(mesh: mesh, policy: state.Policy, context: state.Context, key: state.Key),
                Surface surface => CurvesFromSurface(surface: surface, policy: state.Policy, key: state.Key),
                VectorCloud.ClusterCase cloud => CurvesFromCloud(cloud: cloud, policy: state.Policy, context: state.Context, key: state.Key),
                _ => Fin.Fail<Seq<Curve>>(error: state.Key.Unsupported(geometryType: domain.Value.SourceType, outputType: typeof(Seq<Curve>))),
            },
            meshCase: static (state, domain) => CurvesFromMesh(mesh: domain.Value.Native, policy: state.Policy, context: state.Context, key: state.Key),
            cloudCase: static (state, domain) => domain.Value is VectorCloud.ClusterCase cloud
                ? CurvesFromCloud(cloud: cloud, policy: state.Policy, context: state.Context, key: state.Key)
                : Fin.Fail<Seq<Curve>>(error: state.Key.Unsupported(geometryType: domain.Value.GetType(), outputType: typeof(Seq<Curve>))),
            curveCase: static (state, domain) => Fin.Fail<Seq<Curve>>(error: state.Key.Unsupported(geometryType: domain.Value.GetType(), outputType: typeof(Seq<Curve>))),
            surfaceCase: static (state, domain) => CurvesFromSurface(surface: domain.Value, policy: state.Policy, key: state.Key),
            brepCase: static (state, domain) => CurvesFromBrep(brep: domain.Value, policy: state.Policy, context: state.Context, key: state.Key));
    private static Fin<Seq<Curve>> CurvesFromBrep(RhinoBrep brep, ContourPolicy policy, Context context, Op key) =>
        key.Catch(() => policy.Switch(
            state: (Brep: brep, Context: context, Key: key),
            planeCase: static (state, p) => AcceptCurves(curves: RhinoBrep.CreateContourCurves(brepToContour: state.Brep, sectionPlane: p.Section), key: state.Key),
            axisCase: static (state, p) => AcceptCurves(curves: RhinoBrep.CreateContourCurves(brepToContour: state.Brep, contourStart: p.Start, contourEnd: p.End, interval: p.Interval.Value), key: state.Key),
            isoCase: static (state, _) => Fin.Fail<Seq<Curve>>(error: state.Key.Unsupported(geometryType: typeof(RhinoBrep), outputType: typeof(ContourPolicy.IsoCase)))));
    private static Fin<Seq<Curve>> CurvesFromMesh(RhinoMesh mesh, ContourPolicy policy, Context context, Op key) =>
        key.Catch(() => policy.Switch(
            state: (Mesh: mesh, Context: context, Key: key),
            planeCase: static (state, p) => AcceptCurves(curves: RhinoMesh.CreateContourCurves(meshToContour: state.Mesh, sectionPlane: p.Section, tolerance: state.Context.Absolute.Value), key: state.Key),
            axisCase: static (state, p) => AcceptCurves(curves: RhinoMesh.CreateContourCurves(meshToContour: state.Mesh, contourStart: p.Start, contourEnd: p.End, interval: p.Interval.Value, tolerance: state.Context.Absolute.Value), key: state.Key),
            isoCase: static (state, _) => Fin.Fail<Seq<Curve>>(error: state.Key.Unsupported(geometryType: typeof(RhinoMesh), outputType: typeof(ContourPolicy.IsoCase)))));
    private static Fin<Seq<Curve>> CurvesFromCloud(VectorCloud.ClusterCase cloud, ContourPolicy policy, Context context, Op key) =>
        key.Catch(() => policy.Switch(
            state: (Cloud: cloud, Context: context, Key: key),
            axisCase: static (state, p) => AcceptCurves(curves: state.Cloud.Indexed.CreateContourCurves(contourStart: p.Start, contourEnd: p.End, interval: p.Interval.Value, absoluteTolerance: state.Context.Absolute.Value), key: state.Key),
            planeCase: static (state, _) => Fin.Fail<Seq<Curve>>(error: state.Key.Unsupported(geometryType: typeof(PointCloud), outputType: typeof(ContourPolicy.PlaneCase))),
            isoCase: static (state, _) => Fin.Fail<Seq<Curve>>(error: state.Key.Unsupported(geometryType: typeof(PointCloud), outputType: typeof(ContourPolicy.IsoCase)))));
    private static Fin<Seq<Curve>> CurvesFromSurface(Surface surface, ContourPolicy policy, Op key) =>
        key.Catch(() => policy.Switch(
            state: (Surface: surface, Key: key),
            isoCase: static (state, p) => state.Surface is BrepFace face
                ? AcceptCurves(curves: face.TrimAwareIsoCurve(direction: p.Direction, constantParameter: p.Parameter), key: state.Key)
                : Optional(state.Surface.IsoCurve(direction: p.Direction, constantParameter: p.Parameter))
                    .ToFin(state.Key.InvalidResult())
                    .Map(static curve => Seq(curve)),
            planeCase: static (state, _) => Fin.Fail<Seq<Curve>>(error: state.Key.Unsupported(geometryType: typeof(Surface), outputType: typeof(ContourPolicy.PlaneCase))),
            axisCase: static (state, _) => Fin.Fail<Seq<Curve>>(error: state.Key.Unsupported(geometryType: typeof(Surface), outputType: typeof(ContourPolicy.AxisCase)))));
    private static Fin<Seq<Curve>> AcceptCurves(Curve[] curves, Op key) =>
        Optional(curves).ToFin(key.InvalidResult()).Map(static active => toSeq(active.Where(static curve => curve is not null && curve.IsValid)));
}

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
                    Type t when t == typeof(SymmetricMatrix) => state.Key.AcceptValue(value: tensor).Map(static v => (TOut)(object)v),
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
                from curves in extraction.Domain.Contours(policy: extraction.Policy, context: state.Context, key: state.Key)
                from output in ProjectCurves<TOut>(curves: curves, nativeRouted: true, key: state.Key)
                select output,
            isoSurfaceCase: static (state, extraction) =>
                from mesh in extraction.Field.IsoSurface(bounds: extraction.Bounds, resolution: extraction.Resolution, maxRootSteps: extraction.MaxRootSteps, context: state.Context, key: state.Key)
                from output in typeof(TOut) switch {
                    Type t when t == typeof(RhinoMesh) => state.Key.AcceptValue(value: mesh).Map(static value => (TOut)(object)value),
                    Type t when t == typeof(ExtractionReceipt) => Receipt<TOut>(attempted: 1, emitted: 1, rejected: 0, nativeRouted: true, key: state.Key),
                    _ => Fin.Fail<TOut>(error: state.Key.Unsupported(geometryType: typeof(IsoSurfaceCase), outputType: typeof(TOut))),
                }
                select output,
            glyphCase: static (state, extraction) => ProjectGlyphs<TOut>(field: extraction.Field, policy: extraction.Policy, context: state.Context, key: state.Key),
            sampleGridCase: static (state, extraction) => ProjectGrid<TOut>(field: extraction.Field, policy: extraction.Policy, context: state.Context, key: state.Key),
            streamBundleCase: static (state, extraction) => ProjectBundle<TOut>(field: extraction.Field, policy: extraction.Policy, context: state.Context, key: state.Key));
    private static Fin<TOut> ProjectCurves<TOut>(Seq<Curve> curves, bool nativeRouted, Op key) =>
        typeof(TOut) switch {
            Type t when t == typeof(Seq<Curve>) => curves.TraverseM(curve => key.AcceptValue(value: curve)).As().Map(static value => (TOut)(object)value),
            Type t when t == typeof(ExtractionReceipt) => Receipt<TOut>(attempted: curves.Count, emitted: curves.Count, rejected: 0, nativeRouted: nativeRouted, key: key),
            _ => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(Extraction), outputType: typeof(TOut))),
        };
    private static Fin<TOut> ProjectGlyphs<TOut>(VectorField field, GlyphPolicy policy, Context context, Op key) =>
        from glyphs in policy.Samples.TraverseM(point =>
                field.Project<VectorSpan>(sample: point, context: context, key: key)
                    .Map(span => new Line(span.Anchor, span.Anchor + (policy.Scale.Value * span.Value))))
            .As()
        from output in typeof(TOut) switch {
            Type t when t == typeof(Seq<Line>) => glyphs.TraverseM(line => key.AcceptValue(value: line)).As().Map(static value => (TOut)(object)value),
            Type t when t == typeof(ExtractionReceipt) => Receipt<TOut>(attempted: policy.Samples.Count, emitted: glyphs.Count, rejected: policy.Samples.Count - glyphs.Count, nativeRouted: false, key: key),
            _ => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(GlyphCase), outputType: typeof(TOut))),
        }
        select output;
    private static Fin<TOut> ProjectGrid<TOut>(ScalarField field, GridPolicy policy, Context context, Op key) =>
        from samples in policy.Samples.TraverseM(point =>
                field.SampleScalar(sample: point, context: context, key: key)
                    .Map(value => (Point: point, Value: value)))
            .As()
        from output in typeof(TOut) switch {
            Type t when t == typeof(Seq<(Point3d Point, double Value)>) => samples.ForAll(static sample => sample.Point.IsValid && RhinoMath.IsValidDouble(x: sample.Value))
                ? Fin.Succ((TOut)(object)samples)
                : Fin.Fail<TOut>(error: key.InvalidResult()),
            Type t when t == typeof(ExtractionReceipt) => Receipt<TOut>(attempted: policy.Samples.Count, emitted: samples.Count, rejected: policy.Samples.Count - samples.Count, nativeRouted: false, key: key),
            _ => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(SampleGridCase), outputType: typeof(TOut))),
        }
        select output;
    private static Fin<TOut> ProjectBundle<TOut>(VectorField field, StreamBundlePolicy policy, Context context, Op key) =>
        from traces in policy.Seeds.TraverseM(seed =>
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
            Type t when t == typeof(ExtractionReceipt) => Receipt<TOut>(attempted: policy.Seeds.Count, emitted: traces.Count, rejected: policy.Seeds.Count - traces.Count, nativeRouted: false, key: key),
            _ => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(StreamBundleCase), outputType: typeof(TOut))),
        }
        select output;
    private static Fin<TOut> Receipt<TOut>(int attempted, int emitted, int rejected, bool nativeRouted, Op key) =>
        attempted < 0 || emitted < 0 || rejected < 0 || emitted + rejected != attempted
            ? Fin.Fail<TOut>(error: key.InvalidResult())
            : Fin.Succ((TOut)(object)new ExtractionReceipt(
                Status: rejected == 0 ? ExtractionStatus.Complete : ExtractionStatus.Approximate,
                Attempted: attempted,
                Emitted: emitted,
                Rejected: rejected,
                NativeRouted: nativeRouted));
}
