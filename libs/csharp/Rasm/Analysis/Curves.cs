using Rasm.Vectors;

namespace Rasm.Analysis;

// --- [TYPES] ------------------------------------------------------------------------------
[SkipUnionOps]
[Union]
internal abstract partial record EdgeDescriptor {
    private EdgeDescriptor() { }
    public sealed record OfBrep(EdgeAdjacency Valence, Seq<BrepLoopType> Loops) : EdgeDescriptor;
    public sealed record OfMesh(int ConnectedFaces) : EdgeDescriptor;
    public sealed record OfLoop(BrepLoopType LoopType) : EdgeDescriptor;
}

public enum CurveFeature {
    Input = 0,
    Segment = 1,
    Edge = 2,
    Boundary = 3,
    NakedOuter = 4,
    NakedInner = 5,
    Interior = 6,
    NonManifold = 7,
    OuterLoop = 8,
    InnerLoop = 9,
    Iso = 10,
    Silhouette = 11,
    SubCurve = 12,
    Draft = 13,
}

[SkipUnionOps]
[Union]
public partial record Curves {
    public sealed record EdgesCase(Option<CurveFeature> Kind) : Curves;
    public sealed record SegmentsCase(bool Smooth) : Curves;
    public sealed record IsoCase(IsoStatus Direction, double Normalized) : Curves;
    public sealed record SilhouetteCase(Vector3d? Direction, Option<double> DraftAngle) : Curves;
    public sealed record AtCase(int? Value) : Curves;
    public sealed record FormCase(int? Index = null) : Curves;
    internal static readonly Op Key = Op.Of(name: nameof(Curves));
    public static Curves All => new EdgesCase(Kind: Option<CurveFeature>.None);
    public static Curves Boundary => new EdgesCase(Kind: Some(CurveFeature.Boundary));
    public static Curves NakedOuter => new EdgesCase(Kind: Some(CurveFeature.NakedOuter));
    public static Curves NakedInner => new EdgesCase(Kind: Some(CurveFeature.NakedInner));
    public static Curves Interior => new EdgesCase(Kind: Some(CurveFeature.Interior));
    public static Curves NonManifold => new EdgesCase(Kind: Some(CurveFeature.NonManifold));
    public static Curves OuterLoop => new EdgesCase(Kind: Some(CurveFeature.OuterLoop));
    public static Curves InnerLoop => new EdgesCase(Kind: Some(CurveFeature.InnerLoop));
    public static Curves Segments(bool smooth = false) => new SegmentsCase(Smooth: smooth);
    public static Curves Iso(IsoStatus direction, double normalized = 0.5) => new IsoCase(Direction: direction, Normalized: normalized);
    public static Curves Silhouette(Vector3d? direction = null) => new SilhouetteCase(Direction: direction, DraftAngle: Option<double>.None);
    public static Curves Draft(Vector3d? direction = null, double? angle = null) => new SilhouetteCase(Direction: direction, DraftAngle: Some(Optional(angle).IfNone(0.0)));
    public static Curves At(int? index = null) => new AtCase(Value: index);
    public static Curves Form(int? index = null) => new FormCase(Index: index);
    internal Operation<TGeometry, TOut> Operation<TGeometry, TOut>() where TGeometry : notnull =>
        CanProject(type: typeof(TGeometry)) switch {
            false => Key.Unsupported<TGeometry, TOut>(),
            true => typeof(TOut) switch {
                Type t when t == typeof(Curve) => Analyze.CurveProject<TGeometry, TOut, Curve>(key: Key, aspect: this, project: static (p, _, _, _) => Fin.Succ(p.As<Curve>())),
                Type t when t == typeof(TopologyProjection) => Analyze.CurveProject<TGeometry, TOut, TopologyProjection>(key: Key, aspect: this, project: static (p, _, _, _) => Fin.Succ(Some(p))),
                Type t when t == typeof(CurveFeature) => Analyze.CurveProject<TGeometry, TOut, CurveFeature>(key: Key, aspect: this, project: static (_, feature, _, _) => Fin.Succ(Some(feature))),
                Type t when t == typeof(ComponentIndex) => Analyze.CurveProject<TGeometry, TOut, ComponentIndex>(key: Key, aspect: this, project: static (p, _, _, _) => Fin.Succ(Some(p.Source))),
                Type t when t == typeof(CurveForm) && this is FormCase => Analyze.CurveProject<TGeometry, TOut, CurveForm>(key: Key, aspect: this, project: static (p, _, context, op) => Analyze.ClassifyCurveForm(projection: p, context: context, op: op)),
                _ => Key.Unsupported<TGeometry, TOut>(),
            },
        };
    internal bool CanProject(Type type) =>
        type == typeof(object)
        || type == typeof(GeometryBase)
        || Kind.Of(type: type).Map(kind => CanProject(topology: kind.Topology, type: type)).IfNone(noneValue: false);
    private bool CanProject(Topology topology, Type type) => Switch(
        state: (Topology: topology, Type: type),
        edgesCase: static (state, e) => e.Kind.Case switch {
            null => GeometryKernel.CanCurveForm(type: state.Type) || GeometryKernel.CanCoerce(source: state.Type, target: typeof(Brep)) || (state.Topology == Topology.Mesh && typeof(Mesh).IsAssignableFrom(state.Type)) || (state.Topology == Topology.SubD && typeof(SubD).IsAssignableFrom(state.Type)),
            CurveFeature.Boundary => GeometryKernel.CanCurveForm(type: state.Type) || GeometryKernel.CanCoerce(source: state.Type, target: typeof(Brep)) || (state.Topology == Topology.Mesh && typeof(Mesh).IsAssignableFrom(state.Type)),
            CurveFeature.NakedOuter or CurveFeature.NakedInner or CurveFeature.OuterLoop or CurveFeature.InnerLoop => state.Topology == Topology.Brep && typeof(Brep).IsAssignableFrom(state.Type),
            CurveFeature.Interior or CurveFeature.NonManifold => (state.Topology == Topology.Brep && typeof(Brep).IsAssignableFrom(state.Type)) || (state.Topology == Topology.Mesh && typeof(Mesh).IsAssignableFrom(state.Type)),
            _ => false,
        },
        atCase: static (state, _) =>
            GeometryKernel.CanCurveForm(type: state.Type) || GeometryKernel.CanSurfaceForm(type: state.Type) || (state.Topology == Topology.Brep && typeof(Brep).IsAssignableFrom(state.Type)) || (state.Topology == Topology.Mesh && typeof(Mesh).IsAssignableFrom(state.Type)) || (state.Topology == Topology.SubD && typeof(SubD).IsAssignableFrom(state.Type)),
        segmentsCase: static (state, _) => GeometryKernel.CanCurveForm(type: state.Type) || (state.Topology == Topology.SubD && typeof(SubD).IsAssignableFrom(state.Type)),
        isoCase: static (state, _) => (state.Topology == Topology.Brep && typeof(Brep).IsAssignableFrom(state.Type)) || GeometryKernel.CanSurfaceForm(type: state.Type),
        silhouetteCase: static (state, _) =>
            GeometryKernel.CanSurfaceForm(type: state.Type) || typeof(Extrusion).IsAssignableFrom(state.Type) || (state.Topology == Topology.Brep && typeof(Brep).IsAssignableFrom(state.Type)) || (state.Topology == Topology.Mesh && typeof(Mesh).IsAssignableFrom(state.Type)) || (state.Topology == Topology.SubD && typeof(SubD).IsAssignableFrom(state.Type)) || (state.Topology == Topology.Extrusion && typeof(Extrusion).IsAssignableFrom(state.Type)),
        formCase: static (state, _) =>
            GeometryKernel.CanCurveForm(type: state.Type) || (state.Topology == Topology.Brep && typeof(Brep).IsAssignableFrom(state.Type)) || (state.Topology == Topology.Mesh && typeof(Mesh).IsAssignableFrom(state.Type)) || (state.Topology == Topology.SubD && typeof(SubD).IsAssignableFrom(state.Type)));
    internal Fin<Seq<TopologyProjection>> Select(Seq<TopologyProjection> curves) =>
        (this, curves.Count) switch {
            (_, 0) => Fin.Succ(Seq<TopologyProjection>()),
            (AtCase { Value: int index }, int count) when index < 0 || index >= count => Fin.Fail<Seq<TopologyProjection>>(Key.InvalidInput()),
            (FormCase { Index: int index }, int count) when index < 0 || index >= count => Fin.Fail<Seq<TopologyProjection>>(Key.InvalidInput()),
            (AtCase { Value: int index }, _) => Fin.Succ(Seq(curves[index])),
            (FormCase { Index: int index }, _) => Fin.Succ(Seq(curves[index])),
            (AtCase, _) => Fin.Succ(Seq(curves[0])),
            _ => Fin.Succ(curves),
        };
    internal CurveFeature Feature(Topology topology) => Switch(
        state: topology,
        edgesCase: static (t, e) => e.Kind.IfNone(EdgeFeatureFor(topology: t)),
        segmentsCase: static (_, s) => s.Smooth ? CurveFeature.SubCurve : CurveFeature.Segment,
        isoCase: static (_, _) => CurveFeature.Iso,
        silhouetteCase: static (_, s) => s.DraftAngle.IsSome switch { true => CurveFeature.Draft, false => CurveFeature.Silhouette },
        atCase: static (t, _) => EdgeFeatureFor(topology: t),
        formCase: static (t, _) => EdgeFeatureFor(topology: t));
    internal bool Matches(EdgeDescriptor descriptor) =>
        (this, descriptor) switch {
            (EdgesCase { Kind.IsNone: true } or AtCase or FormCase, EdgeDescriptor.OfBrep or EdgeDescriptor.OfMesh) => true,
            (EdgesCase { Kind.Case: CurveFeature.Boundary }, EdgeDescriptor.OfBrep { Valence: EdgeAdjacency.Naked }) => true,
            (EdgesCase { Kind.Case: CurveFeature.Boundary }, EdgeDescriptor.OfMesh { ConnectedFaces: 1 }) => true,
            (EdgesCase { Kind.Case: CurveFeature.NakedOuter }, EdgeDescriptor.OfBrep { Valence: EdgeAdjacency.Naked, Loops: Seq<BrepLoopType> loops }) => loops.Exists(static loop => loop == BrepLoopType.Outer),
            (EdgesCase { Kind.Case: CurveFeature.NakedInner }, EdgeDescriptor.OfBrep { Valence: EdgeAdjacency.Naked, Loops: Seq<BrepLoopType> loops }) => loops.Exists(static loop => loop == BrepLoopType.Inner),
            (EdgesCase { Kind.Case: CurveFeature.Interior }, EdgeDescriptor.OfBrep { Valence: EdgeAdjacency.Interior }) => true,
            (EdgesCase { Kind.Case: CurveFeature.Interior }, EdgeDescriptor.OfMesh { ConnectedFaces: 2 }) => true,
            (EdgesCase { Kind.Case: CurveFeature.NonManifold }, EdgeDescriptor.OfBrep { Valence: EdgeAdjacency.NonManifold }) => true,
            (EdgesCase { Kind.Case: CurveFeature.NonManifold }, EdgeDescriptor.OfMesh { ConnectedFaces: > 2 }) => true,
            (EdgesCase { Kind.Case: CurveFeature.OuterLoop }, EdgeDescriptor.OfLoop { LoopType: BrepLoopType.Outer }) => true,
            (EdgesCase { Kind.Case: CurveFeature.InnerLoop }, EdgeDescriptor.OfLoop { LoopType: BrepLoopType.Inner }) => true,
            _ => false,
        };
    private static CurveFeature EdgeFeatureFor(Topology topology) =>
        topology == Topology.Curve ? CurveFeature.Input : topology == Topology.Surface ? CurveFeature.Boundary : CurveFeature.Edge;
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Analyze {
    internal static Operation<Surface, Curve> Iso(IsoStatus iso, double normalized = 0.5) {
        Op key = Op.Of();
        return Operation<Surface, Curve>.Build(
            key: key, requirement: Requirement.SurfaceEvaluation, state: (Key: key, Iso: iso, Normalized: normalized),
            evaluator: static (state, geometry) =>
                from curves in IsoSeq(surface: geometry, iso: state.Iso, normalized: state.Normalized, op: state.Key).ToEff()
                from result in state.Key.Accept(values: curves).ToEff()
                select result);
    }
    internal static Operation<TGeometry, TOut> Segments<TGeometry, TOut>() where TGeometry : notnull =>
        Curves.Segments().Operation<TGeometry, TOut>();
    internal static Operation<TGeometry, TOut> CurveProject<TGeometry, TOut, TValue>(Op key, Curves aspect, Func<TopologyProjection, CurveFeature, Context, Op, Fin<Option<TValue>>> project) where TGeometry : notnull =>
        Operation<TGeometry, TValue>.Build(
            key: key, state: (Key: key, Aspect: aspect, Project: project), requiresContext: true,
            evaluator: static (state, geometry) =>
                from runtime in Env.EnvAsks
                from kind in geometry.KindOf(context: runtime.Context).ToEff()
                let feature = state.Aspect.Feature(topology: kind.Topology)
                from curves in CurveProjections(geometry: geometry, aspect: state.Aspect, context: runtime.Context, op: state.Key, cancel: runtime.Cancellation).ToEff()
                from chosen in state.Aspect.Select(curves: curves).ToEff()
                from result in TopologyProjection.Project(all: curves, chosen: chosen, project: values => values.TraverseM(projection => state.Project(arg1: projection, arg2: feature, arg3: runtime.Context, arg4: state.Key)).As().Bind(projected => state.Key.Accept(values: projected.Choose(static value => value)))).ToEff()
                select result).As<TGeometry, TOut>(key: key);
    internal static Fin<Seq<TopologyProjection>> CurveProjections<TGeometry>(TGeometry geometry, Curves aspect, Context context, Op op, CancellationToken cancel) where TGeometry : notnull =>
        Optional(geometry).ToFin(op.InvalidInput()).Bind(g => (g, aspect) switch {
            (Curve or Line or Polyline or Circle or Arc or Ellipse, Curves.EdgesCase { Kind.Case: null or CurveFeature.Boundary } or Curves.AtCase or Curves.SegmentsCase or Curves.FormCase) => CurveInput(source: g, aspect: aspect, op: op),
            (Brep brep, Curves.EdgesCase { Kind.Case: null or CurveFeature.Boundary or CurveFeature.NakedOuter or CurveFeature.NakedInner or CurveFeature.Interior or CurveFeature.NonManifold } or Curves.AtCase or Curves.FormCase) =>
                SelectTopologyFeatures(source: brep.Edges, selector: aspect,
                    describe: static edge => new EdgeDescriptor.OfBrep(Valence: edge.Valence, Loops: toSeq(edge.TrimIndices()).Choose(t => Optional(edge.Brep.Trims[t].Loop).Map(static loop => loop.LoopType))),
                    project: edge => Optional(edge.DuplicateCurve()).Map(c => TopologyProjection.Of(c, new ComponentIndex(ComponentIndexType.BrepEdge, edge.EdgeIndex)))),
            (Brep brep, Curves.EdgesCase { Kind.Case: CurveFeature.OuterLoop or CurveFeature.InnerLoop }) =>
                SelectTopologyFeatures(source: brep.Loops, selector: aspect,
                    describe: static loop => new EdgeDescriptor.OfLoop(LoopType: loop.LoopType),
                    project: loop => Optional(loop.To3dCurve()).Map(c => TopologyProjection.Of(c, new ComponentIndex(ComponentIndexType.BrepLoop, loop.LoopIndex)))),
            (Brep brep, Curves.IsoCase iso) => toSeq(brep.Faces).TraverseM(f => IsoSeq(surface: f, iso: iso.Direction, normalized: iso.Normalized, op: op).Map(s => s.Map(c => TopologyProjection.Of(c, new ComponentIndex(ComponentIndexType.BrepFace, f.FaceIndex))))).As().Map(static nested => nested.Bind(static seq => seq)),
            (BrepFace face, Curves.EdgesCase { Kind.Case: null or CurveFeature.Boundary } or Curves.AtCase or Curves.FormCase) =>
                FaceBoundaryEdgesOf(face: face, selector: aspect),
            (Mesh mesh, Curves.EdgesCase { Kind.Case: null or CurveFeature.Boundary or CurveFeature.Interior or CurveFeature.NonManifold } or Curves.AtCase or Curves.FormCase) =>
                SelectTopologyFeatures(source: Enumerable.Range(start: 0, count: mesh.TopologyEdges.Count), selector: aspect,
                    describe: i => new EdgeDescriptor.OfMesh(ConnectedFaces: mesh.TopologyEdges.GetConnectedFaces(topologyEdgeIndex: i).Length),
                    project: i => Some(TopologyProjection.Of(curve: mesh.TopologyEdges.EdgeLine(topologyEdgeIndex: i).ToNurbsCurve(), source: new ComponentIndex(ComponentIndexType.MeshTopologyEdge, i)))),
            (Surface surface, Curves.IsoCase iso) => IsoSeq(surface: surface, iso: iso.Direction, normalized: iso.Normalized, op: op).Map(seq => seq.Map(c => TopologyProjection.Of(c, new ComponentIndex(ComponentIndexType.NoType, 0)))),
            (object surfaceLike, Curves.IsoCase iso) when GeometryKernel.CanSurfaceForm(type: surfaceLike.GetType()) =>
                GeometryKernel.SurfaceForm(source: surfaceLike, op: op).Bind(lease => lease.Use(surface => IsoSeq(surface: surface, iso: iso.Direction, normalized: iso.Normalized, op: op).Map(seq => seq.Map(c => TopologyProjection.Of(c, new ComponentIndex(ComponentIndexType.NoType, 0)))))),
            (object brepLike, Curves.EdgesCase { Kind.Case: null or CurveFeature.Boundary } or Curves.AtCase or Curves.FormCase) when GeometryKernel.CanCoerce(source: brepLike.GetType(), target: typeof(Brep)) =>
                GeometryKernel.BrepForm(source: brepLike, op: op).Bind(lease => lease.Use(brep => SelectTopologyFeatures(source: brep.Edges, selector: aspect,
                    describe: static edge => new EdgeDescriptor.OfBrep(Valence: edge.Valence, Loops: toSeq(edge.TrimIndices()).Choose(t => Optional(edge.Brep.Trims[t].Loop).Map(static loop => loop.LoopType))),
                    project: edge => Optional(edge.DuplicateCurve()).Map(c => TopologyProjection.Of(c, new ComponentIndex(ComponentIndexType.BrepEdge, edge.EdgeIndex)))))),
            (SubD subd, Curves.EdgesCase { Kind.Case: null } or Curves.AtCase or Curves.SegmentsCase or Curves.FormCase) => SubDEdges(subd: subd),
            (GeometryBase native, Curves.SilhouetteCase silhouette) => SilhouettesOf(geometry: native, silhouette: silhouette, context: context, op: op, cancel: cancel),
            _ => Fin.Fail<Seq<TopologyProjection>>(op.Unsupported(g.GetType(), typeof(Curve))),
        });
    internal static Fin<Seq<Curve>> IsoSeq(Surface surface, IsoStatus iso, double normalized, Op op) => (iso, normalized is >= 0.0 and <= 1.0) switch {
        (IsoStatus.West, _) when surface is BrepFace face => Fin.Succ(toSeq(face.TrimAwareIsoCurve(1, face.Domain(0).T0))),
        (IsoStatus.East, _) when surface is BrepFace face => Fin.Succ(toSeq(face.TrimAwareIsoCurve(1, face.Domain(0).T1))),
        (IsoStatus.South, _) when surface is BrepFace face => Fin.Succ(toSeq(face.TrimAwareIsoCurve(0, face.Domain(1).T0))),
        (IsoStatus.North, _) when surface is BrepFace face => Fin.Succ(toSeq(face.TrimAwareIsoCurve(0, face.Domain(1).T1))),
        (IsoStatus.West or IsoStatus.South or IsoStatus.East or IsoStatus.North, _) => Optional(surface.IsoCurve(iso)).ToFin(op.InvalidResult()).Map(static c => Seq(c)),
        (IsoStatus.X or IsoStatus.Y, true) when surface.Domain(iso == IsoStatus.X ? 0 : 1) is { IsValid: true } d =>
            surface is BrepFace face ? Fin.Succ(toSeq(face.TrimAwareIsoCurve(iso == IsoStatus.X ? 1 : 0, d.ParameterAt(normalized))))
                : Optional(surface.IsoCurve(iso, d.ParameterAt(normalized))).ToFin(op.InvalidResult()).Map(static c => Seq(c)),
        _ => Fin.Fail<Seq<Curve>>(op.InvalidInput()),
    };
    internal static Fin<Option<CurveForm>> ClassifyCurveForm(TopologyProjection projection, Context context, Op op) =>
        projection.As<Curve>()
            .ToFin(op.InvalidResult())
            .Bind(curve => GeometryKernel.CurveFormOf(curve: curve, context: context, op: op).Map(static form => Some(form)));
    private static Fin<Seq<TopologyProjection>> CurveInput(object source, Curves aspect, Op op) =>
        GeometryKernel.CurveForm(source: source, op: op).Bind(lease => lease.Use(native => aspect switch {
            Curves.EdgesCase { Kind.Case: null or CurveFeature.Boundary } when native.TryGetPolyline(polyline: out Polyline polyline) && polyline.SegmentCount > 0 =>
                Fin.Succ(toSeq(polyline.GetSegments().Select((segment, i) => TopologyProjection.Of(new LineCurve(segment), new ComponentIndex(ComponentIndexType.PolycurveSegment, i))))),
            Curves.SegmentsCase segments => Optional(segments.Smooth ? native.GetSubCurves() : native.DuplicateSegments()) switch {
                Option<Curve[]> opt when opt.Case is Curve[] arr && arr.Length > 0 => Fin.Succ(toSeq(arr.Select((cc, i) => TopologyProjection.Of(cc, new ComponentIndex(ComponentIndexType.PolycurveSegment, i))))),
                _ => Optional(native.DuplicateCurve()).ToFin(op.InvalidResult()).Map(d => Seq(TopologyProjection.Of(d, new ComponentIndex(ComponentIndexType.PolycurveSegment, 0)))),
            },
            _ => Optional(native.DuplicateCurve()).ToFin(op.InvalidResult()).Map(d => Seq(TopologyProjection.Of(d, new ComponentIndex(ComponentIndexType.NoType, 0)))),
        }));
    private static Fin<Seq<TopologyProjection>> SelectTopologyFeatures<TPrimitive>(IEnumerable<TPrimitive> source, Curves selector, Func<TPrimitive, EdgeDescriptor> describe, Func<TPrimitive, Option<TopologyProjection>> project) =>
        Fin.Succ(toSeq(source).Choose(item => selector.Matches(descriptor: describe(arg: item)) ? project(arg: item) : Option<TopologyProjection>.None));
    private static Fin<Seq<TopologyProjection>> FaceBoundaryEdgesOf(BrepFace face, Curves selector) =>
        Fin.Succ(toSeq(face.Loops).Bind(loop => toSeq(loop.Trims).Choose(trim => (selector, trim.Edge) switch {
            (Curves.EdgesCase { Kind.Case: null or CurveFeature.Boundary } or Curves.AtCase or Curves.FormCase, BrepEdge edge) =>
                Optional(edge.DuplicateCurve()).Map(curve => TopologyProjection.Of(curve: curve, source: new ComponentIndex(ComponentIndexType.BrepEdge, edge.EdgeIndex))),
            _ => Option<TopologyProjection>.None,
        })));
    private static Fin<Seq<TopologyProjection>> SubDEdges(SubD subd) {
        _ = subd.UpdateSurfaceMeshCache(lazyUpdate: true);
        return Fin.Succ(toSeq(subd.DuplicateEdgeCurves().Select((c, i) => TopologyProjection.Of(c, new ComponentIndex(type: ComponentIndexType.SubdEdge, index: i)))));
    }
    private static Fin<Seq<TopologyProjection>> SilhouettesOf(GeometryBase geometry, Curves.SilhouetteCase silhouette, Context context, Op op, CancellationToken cancel) =>
        cancel.IsCancellationRequested
            ? Fin.Fail<Seq<TopologyProjection>>(new Fault.Cancelled())
            : VectorIntent.Direction(value: Optional(silhouette.Direction).IfNone(Vector3d.ZAxis)).Project<Vector3d>(context: context, key: op)
                .Bind(direction =>
                    (geometry switch {
                        Brep or BrepFace or Mesh or Extrusion => Fin.Succ((Geometry: geometry, Owned: Option<GeometryBase>.None)),
                        Surface surface => Optional(surface.ToBrep()).ToFin(op.InvalidResult()).Map(static b => (Geometry: (GeometryBase)b, Owned: Some((GeometryBase)b))),
                        SubD subd => Optional(subd.ToBrep(SubDToBrepOptions.Default)).ToFin(op.InvalidResult()).Map(static b => (Geometry: (GeometryBase)b, Owned: Some((GeometryBase)b))),
                        _ => Fin.Fail<(GeometryBase Geometry, Option<GeometryBase> Owned)>(op.Unsupported(geometry.GetType(), typeof(Curve))),
                    }).Bind(shape => {
                        Fin<Seq<TopologyProjection>> result = Optional(silhouette.DraftAngle.Case switch {
                            double a => Silhouette.ComputeDraftCurve(shape.Geometry, a, direction, context.Absolute.Value, context.Angle.Value, cancel),
                            _ => Silhouette.Compute(shape.Geometry, SilhouetteType.Projecting | SilhouetteType.TangentProjects | SilhouetteType.Tangent | SilhouetteType.Crease | SilhouetteType.Boundary, direction, context.Absolute.Value, context.Angle.Value, [], cancel),
                        }).ToFin(cancel.IsCancellationRequested ? new Fault.Cancelled() : op.InvalidResult())
                            .Map(arr => toSeq(arr).Map(sil => TopologyProjection.Of(sil.Curve, sil.GeometryComponentIndex)));
                        _ = shape.Owned.Iter(static geom => geom.Dispose());
                        return result;
                    }));
}
