namespace Rasm.Analysis;

// --- [TYPES] ------------------------------------------------------------------------------
public enum NakedKind { Outer, Inner }

[Union]
public partial record EdgeSelector {
    public sealed record AllOf : EdgeSelector; public sealed record Boundary : EdgeSelector; public sealed record NakedFiltered(NakedKind Kind) : EdgeSelector;
    public sealed record Interior : EdgeSelector; public sealed record NonManifold : EdgeSelector; public sealed record LoopFiltered(BrepLoopType Type) : EdgeSelector;
}

[Union]
public partial record SilhouetteQuery {
    public sealed record Projecting(Vector3d? Direction) : SilhouetteQuery; public sealed record Draft(Vector3d? Direction, Option<double> Angle) : SilhouetteQuery;
}

[Union]
internal abstract partial record EdgeDescriptor {
    private EdgeDescriptor() { }
    public sealed record OfBrep(EdgeAdjacency Valence, Seq<BrepLoopType> Loops) : EdgeDescriptor;
    public sealed record OfMesh(int ConnectedFaces) : EdgeDescriptor;
    public sealed record OfLoop(BrepLoopType LoopType) : EdgeDescriptor;
}

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public partial record Curves : IAspect {
    public sealed record EdgesCase(EdgeSelector Selector) : Curves; public sealed record SegmentsCase(bool Smooth) : Curves; public sealed record IsoCase(IsoStatus Direction, double Normalized) : Curves; public sealed record SilhouetteCase(SilhouetteQuery Query) : Curves; public sealed record AtCase(int? Value) : Curves;
    public static Curves All => new EdgesCase(Selector: new EdgeSelector.AllOf()); public static Curves Boundary => new EdgesCase(Selector: new EdgeSelector.Boundary());
    public static Curves NakedOuter => new EdgesCase(Selector: new EdgeSelector.NakedFiltered(Kind: NakedKind.Outer)); public static Curves NakedInner => new EdgesCase(Selector: new EdgeSelector.NakedFiltered(Kind: NakedKind.Inner));
    public static Curves Interior => new EdgesCase(Selector: new EdgeSelector.Interior()); public static Curves NonManifold => new EdgesCase(Selector: new EdgeSelector.NonManifold());
    public static Curves OuterLoop => new EdgesCase(Selector: new EdgeSelector.LoopFiltered(Type: BrepLoopType.Outer)); public static Curves InnerLoop => new EdgesCase(Selector: new EdgeSelector.LoopFiltered(Type: BrepLoopType.Inner));
    public static Curves Segments => new SegmentsCase(Smooth: false); public static Curves SubCurves => new SegmentsCase(Smooth: true);
    public static Curves Iso(IsoStatus direction, double normalized = 0.5) => new IsoCase(Direction: direction, Normalized: normalized);
    public static Curves Silhouette(Vector3d? direction = null) => new SilhouetteCase(Query: new SilhouetteQuery.Projecting(Direction: direction));
    public static Curves Draft(Vector3d? direction = null, double? angle = null) => new SilhouetteCase(Query: new SilhouetteQuery.Draft(Direction: direction, Angle: Optional(angle)));
    public static Curves At(int? index = null) => new AtCase(Value: index);
    internal static readonly Op Key = Op.Of(name: nameof(Curves));
    public Operation<TGeometry, TOut> Operation<TGeometry, TOut>() where TGeometry : notnull =>
        CanProject(type: typeof(TGeometry)) switch {
            false => Key.Unsupported<TGeometry, TOut>(),
            true => typeof(TOut) switch {
                Type t when t == typeof(Curve) => Analyze.CurveProject<TGeometry, TOut, Curve>(key: Key, aspect: this, project: static p => p.As<Curve>()),
                Type t when t == typeof(TopologyProjection) => Analyze.CurveProject<TGeometry, TOut, TopologyProjection>(key: Key, aspect: this, project: static p => Some(p)),
                Type t when t == typeof(CurveFeature) => Analyze.CurveProject<TGeometry, TOut, CurveFeature>(key: Key, aspect: this, project: static p => Some(p.Feature)),
                Type t when t == typeof(ComponentIndex) => Analyze.CurveProject<TGeometry, TOut, ComponentIndex>(key: Key, aspect: this, project: static p => Some(p.Source)),
                _ => Key.Unsupported<TGeometry, TOut>(),
            },
        };
    internal Fin<Seq<TopologyProjection>> Select(Seq<TopologyProjection> curves) =>
        (this, curves.Count) switch {
            (_, 0) => Fin.Succ(Seq<TopologyProjection>()),
            (AtCase { Value: int index }, int count) when index < 0 || index >= count => Fin.Fail<Seq<TopologyProjection>>(Key.InvalidInput()),
            (AtCase { Value: int index }, _) => Fin.Succ(Seq(curves[index])),
            (AtCase, _) => Fin.Succ(Seq(curves[0])),
            _ => Fin.Succ(curves),
        };
    internal CurveFeature Feature(Topology topology) => Switch(
        state: topology,
        edgesCase: static (t, e) => e.Selector switch {
            EdgeSelector.Boundary => CurveFeature.Boundary,
            EdgeSelector.NakedFiltered { Kind: NakedKind.Outer } => CurveFeature.NakedOuter,
            EdgeSelector.NakedFiltered { Kind: NakedKind.Inner } => CurveFeature.NakedInner,
            EdgeSelector.Interior => CurveFeature.Interior,
            EdgeSelector.NonManifold => CurveFeature.NonManifold,
            EdgeSelector.LoopFiltered { Type: BrepLoopType.Outer } => CurveFeature.OuterLoop,
            EdgeSelector.LoopFiltered { Type: BrepLoopType.Inner } => CurveFeature.InnerLoop,
            _ => EdgeFeatureFor(topology: t),
        },
        segmentsCase: static (_, s) => s.Smooth ? CurveFeature.SubCurve : CurveFeature.Segment,
        isoCase: static (_, _) => CurveFeature.Iso,
        silhouetteCase: static (_, s) => s.Query is SilhouetteQuery.Draft ? CurveFeature.Draft : CurveFeature.Silhouette,
        atCase: static (t, _) => EdgeFeatureFor(topology: t));
    internal bool Matches(EdgeDescriptor descriptor) =>
        (this, descriptor) switch {
            (EdgesCase { Selector: EdgeSelector.AllOf } or AtCase, EdgeDescriptor.OfBrep or EdgeDescriptor.OfMesh) => true,
            (EdgesCase { Selector: EdgeSelector.Boundary }, EdgeDescriptor.OfBrep { Valence: EdgeAdjacency.Naked }) => true,
            (EdgesCase { Selector: EdgeSelector.Boundary }, EdgeDescriptor.OfMesh { ConnectedFaces: 1 }) => true,
            (EdgesCase { Selector: EdgeSelector.NakedFiltered { Kind: NakedKind.Outer } }, EdgeDescriptor.OfBrep { Valence: EdgeAdjacency.Naked, Loops: var loops }) => loops.Exists(static loop => loop == BrepLoopType.Outer),
            (EdgesCase { Selector: EdgeSelector.NakedFiltered { Kind: NakedKind.Inner } }, EdgeDescriptor.OfBrep { Valence: EdgeAdjacency.Naked, Loops: var loops }) => loops.Exists(static loop => loop == BrepLoopType.Inner),
            (EdgesCase { Selector: EdgeSelector.Interior }, EdgeDescriptor.OfBrep { Valence: EdgeAdjacency.Interior }) => true,
            (EdgesCase { Selector: EdgeSelector.Interior }, EdgeDescriptor.OfMesh { ConnectedFaces: 2 }) => true,
            (EdgesCase { Selector: EdgeSelector.NonManifold }, EdgeDescriptor.OfBrep { Valence: EdgeAdjacency.NonManifold }) => true,
            (EdgesCase { Selector: EdgeSelector.NonManifold }, EdgeDescriptor.OfMesh { ConnectedFaces: > 2 }) => true,
            (EdgesCase { Selector: EdgeSelector.LoopFiltered { Type: var lt } }, EdgeDescriptor.OfLoop { LoopType: var dt }) => lt == dt,
            _ => false,
        };
    internal bool CanProject(Type type) =>
        type == typeof(object)
        || type == typeof(GeometryBase)
        || KindLookup.Resolve(type).Map(kind => CanProject(topology: kind.Topology, type: type)).IfNone(false);
    private bool CanProject(Topology topology, Type type) => Switch(
        state: (Topology: topology, Type: type),
        edgesCase: static (state, e) => e.Selector switch {
            EdgeSelector.AllOf => state.Topology switch { Topology.Curve => IsCurveType(state.Type), Topology.Surface => IsSurfaceType(state.Type), Topology.Brep => typeof(Brep).IsAssignableFrom(state.Type), Topology.Mesh => typeof(Mesh).IsAssignableFrom(state.Type), Topology.SubD => typeof(SubD).IsAssignableFrom(state.Type), _ => false },
            EdgeSelector.Boundary => state.Topology switch { Topology.Curve => IsCurveType(state.Type), Topology.Surface => IsSurfaceType(state.Type) || typeof(Extrusion).IsAssignableFrom(state.Type), Topology.Brep => typeof(Brep).IsAssignableFrom(state.Type), Topology.Mesh => typeof(Mesh).IsAssignableFrom(state.Type), Topology.Extrusion => typeof(Extrusion).IsAssignableFrom(state.Type), _ => false },
            EdgeSelector.NakedFiltered or EdgeSelector.LoopFiltered => state.Topology == Topology.Brep && typeof(Brep).IsAssignableFrom(state.Type),
            EdgeSelector.Interior or EdgeSelector.NonManifold => (state.Topology == Topology.Brep && typeof(Brep).IsAssignableFrom(state.Type)) || (state.Topology == Topology.Mesh && typeof(Mesh).IsAssignableFrom(state.Type)),
            _ => false,
        },
        atCase: static (state, _) => state.Topology switch { Topology.Curve => IsCurveType(state.Type), Topology.Surface => IsSurfaceType(state.Type), Topology.Brep => typeof(Brep).IsAssignableFrom(state.Type), Topology.Mesh => typeof(Mesh).IsAssignableFrom(state.Type), Topology.SubD => typeof(SubD).IsAssignableFrom(state.Type), _ => false },
        segmentsCase: static (state, _) => (state.Topology == Topology.Curve && IsCurveType(state.Type)) || (state.Topology == Topology.SubD && typeof(SubD).IsAssignableFrom(state.Type)),
        isoCase: static (state, _) => (state.Topology == Topology.Brep && typeof(Brep).IsAssignableFrom(state.Type)) || (state.Topology == Topology.Surface && IsSurfaceType(state.Type)),
        silhouetteCase: static (state, _) => state.Topology switch { Topology.Surface => IsSurfaceType(state.Type) || typeof(Extrusion).IsAssignableFrom(state.Type), Topology.Brep => typeof(Brep).IsAssignableFrom(state.Type), Topology.Mesh => typeof(Mesh).IsAssignableFrom(state.Type), Topology.SubD => typeof(SubD).IsAssignableFrom(state.Type), Topology.Extrusion => typeof(Extrusion).IsAssignableFrom(state.Type), _ => false });
    private static CurveFeature EdgeFeatureFor(Topology topology) => topology switch { Topology.Curve => CurveFeature.Input, Topology.Surface => CurveFeature.Boundary, _ => CurveFeature.Edge };
    private static bool IsCurveType(Type type) => KindLookup.Resolve(type).Map(static kind => kind.Topology == Topology.Curve).IfNone(false);
    private static bool IsSurfaceType(Type type) => KindLookup.Resolve(type).Map(static kind => kind.Topology == Topology.Surface).IfNone(false);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Analyze {
    public static Operation<TGeometry, TOut> Curves<TGeometry, TOut>(Curves aspect) where TGeometry : notnull => Aspect<Curves, TGeometry, TOut>(aspect: aspect);
    public static Operation<TGeometry, TOut> Segments<TGeometry, TOut>() where TGeometry : notnull =>
        Curves<TGeometry, TOut>(aspect: Analysis.Curves.Segments);
    public static Operation<Surface, Curve> Iso(IsoStatus iso, double normalized = 0.5) {
        Op key = Op.Of();
        return Operation<Surface, Curve>.Build(
            key: key, requirement: Requirement.SurfaceEvaluation, state: (Key: key, Iso: iso, Normalized: normalized), requiresContext: true,
            evaluator: static (state, geometry) =>
                from context in Env.Asks
                from curves in IsoSeq(surface: geometry, iso: state.Iso, normalized: state.Normalized, op: state.Key).ToEff()
                from result in state.Key.Accept(values: curves).ToEff()
                select result);
    }
    internal static Operation<TGeometry, TOut> CurveProject<TGeometry, TOut, TValue>(Op key, Curves aspect, Func<TopologyProjection, Option<TValue>> project) where TGeometry : notnull =>
        Cast<TGeometry, TOut>(key: key, operation: Operation<TGeometry, TValue>.Build(
            key: key, state: (Key: key, Aspect: aspect, Project: project), requiresContext: true,
            evaluator: static (state, geometry) =>
                from runtime in Env.EnvAsks
                from kind in ((object)geometry).KindOf(context: runtime.Context).ToEff()
                let feature = state.Aspect.Feature(topology: kind.Topology)
                from curves in CurveProjections(geometry: geometry, aspect: state.Aspect, feature: feature, context: runtime.Context, op: state.Key, cancel: runtime.Cancellation).ToEff()
                from chosen in state.Aspect.Select(curves: curves).ToEff()
                from result in TopologyProjection.Project(all: curves, chosen: chosen, project: values => state.Key.Accept(values: values.Choose(state.Project))).ToEff()
                select result));
    internal static Fin<Seq<TopologyProjection>> CurveProjections<TGeometry>(TGeometry geometry, Curves aspect, CurveFeature feature, Context context, Op op, CancellationToken cancel) where TGeometry : notnull =>
        Optional(geometry).ToFin(op.InvalidInput()).Bind(g => (g, aspect) switch {
            (Curve or Line or Polyline or Circle or Arc or Ellipse, Analysis.Curves.EdgesCase { Selector: EdgeSelector.AllOf or EdgeSelector.Boundary } or Analysis.Curves.AtCase or Analysis.Curves.SegmentsCase) => CurveInput(source: g, aspect: aspect, feature: feature, op: op),
            (Brep brep, Analysis.Curves.EdgesCase { Selector: EdgeSelector.AllOf or EdgeSelector.Boundary or EdgeSelector.NakedFiltered or EdgeSelector.Interior or EdgeSelector.NonManifold } or Analysis.Curves.AtCase) =>
                SelectTopologyFeatures(source: brep.Edges, selector: aspect,
                    describe: static edge => new EdgeDescriptor.OfBrep(Valence: edge.Valence, Loops: toSeq(edge.TrimIndices()).Choose(t => Optional(edge.Brep.Trims[t].Loop).Map(static loop => loop.LoopType))),
                    project: edge => Optional(edge.DuplicateCurve()).Map(c => TopologyProjection.Of(c, feature, new ComponentIndex(ComponentIndexType.BrepEdge, edge.EdgeIndex)))),
            (Brep brep, Analysis.Curves.EdgesCase { Selector: EdgeSelector.LoopFiltered }) =>
                SelectTopologyFeatures(source: brep.Loops, selector: aspect,
                    describe: static loop => new EdgeDescriptor.OfLoop(LoopType: loop.LoopType),
                    project: loop => Optional(loop.To3dCurve()).Map(c => TopologyProjection.Of(c, feature, new ComponentIndex(ComponentIndexType.BrepLoop, loop.LoopIndex)))),
            (Brep brep, Analysis.Curves.IsoCase iso) => toSeq(brep.Faces).TraverseM(f => IsoSeq(surface: f, iso: iso.Direction, normalized: iso.Normalized, op: op).Map(s => s.Map(c => TopologyProjection.Of(c, feature, new ComponentIndex(ComponentIndexType.BrepFace, f.FaceIndex))))).As().Map(static nested => nested.Bind(static seq => seq)),
            (BrepFace face, Analysis.Curves.EdgesCase { Selector: EdgeSelector.AllOf or EdgeSelector.Boundary } or Analysis.Curves.AtCase) => Optional(face.DuplicateFace(duplicateMeshes: false)).ToFin(op.InvalidResult()).Bind(fb => new Lease<Brep>.Owned(Value: fb).Use(owned => NakedBrepEdgesOf(brep: owned, feature: feature, source: new ComponentIndex(ComponentIndexType.BrepFace, face.FaceIndex)))),
            (Mesh mesh, Analysis.Curves.EdgesCase { Selector: EdgeSelector.AllOf or EdgeSelector.Boundary or EdgeSelector.Interior or EdgeSelector.NonManifold } or Analysis.Curves.AtCase) =>
                SelectTopologyFeatures(source: Enumerable.Range(start: 0, count: mesh.TopologyEdges.Count), selector: aspect,
                    describe: i => new EdgeDescriptor.OfMesh(ConnectedFaces: mesh.TopologyEdges.GetConnectedFaces(topologyEdgeIndex: i).Length),
                    project: i => Some(TopologyProjection.Of(curve: mesh.TopologyEdges.EdgeLine(topologyEdgeIndex: i).ToNurbsCurve(), feature: feature, source: new ComponentIndex(ComponentIndexType.MeshTopologyEdge, i)))),
            (Surface surface, Analysis.Curves.IsoCase iso) => IsoSeq(surface: surface, iso: iso.Direction, normalized: iso.Normalized, op: op).Map(seq => seq.Map(c => TopologyProjection.Of(c, feature, new ComponentIndex(ComponentIndexType.NoType, 0)))),
            (GeometryBase { HasBrepForm: true } native, Analysis.Curves.EdgesCase { Selector: EdgeSelector.AllOf or EdgeSelector.Boundary } or Analysis.Curves.AtCase) when feature == CurveFeature.Boundary => GeometryKernel.BrepForm(source: native, op: op).Bind(lease => lease.Use(brep => NakedBrepEdgesOf(brep: brep, feature: feature, source: new ComponentIndex(ComponentIndexType.NoType, 0)))),
            (SubD subd, Analysis.Curves.EdgesCase { Selector: EdgeSelector.AllOf } or Analysis.Curves.AtCase or Analysis.Curves.SegmentsCase) => SubDEdges(subd: subd, feature: feature),
            (GeometryBase native, Analysis.Curves.SilhouetteCase) => SilhouettesOf(geometry: native, aspect: aspect, feature: feature, context: context, op: op, cancel: cancel),
            _ => Fin.Fail<Seq<TopologyProjection>>(op.Unsupported(g.GetType(), typeof(Curve))),
        });
    internal static Fin<Seq<Curve>> IsoSeq(Surface surface, IsoStatus iso, double normalized, Op op) => (iso, normalized is >= 0.0 and <= 1.0) switch {
        (IsoStatus.West or IsoStatus.South or IsoStatus.East or IsoStatus.North, _) => Optional(surface.IsoCurve(iso)).ToFin(op.InvalidResult()).Map(static c => Seq(c)),
        (IsoStatus.X or IsoStatus.Y, true) when surface.Domain(iso == IsoStatus.X ? 0 : 1) is { IsValid: true } d =>
            surface is BrepFace face ? Fin.Succ(toSeq(face.TrimAwareIsoCurve(iso == IsoStatus.X ? 0 : 1, d.ParameterAt(normalized))))
                : Optional(surface.IsoCurve(iso, d.ParameterAt(normalized))).ToFin(op.InvalidResult()).Map(static c => Seq(c)),
        _ => Fin.Fail<Seq<Curve>>(op.InvalidInput()),
    };
    private static Fin<Seq<TopologyProjection>> CurveInput(object source, Curves aspect, CurveFeature feature, Op op) =>
        GeometryKernel.CurveForm(source: source, op: op).Bind(lease => lease.Use(native => aspect switch {
            Analysis.Curves.SegmentsCase segments => Optional(segments.Smooth ? native.GetSubCurves() : native.DuplicateSegments()) switch {
                Option<Curve[]> opt when opt.Case is Curve[] arr && arr.Length > 0 => Fin.Succ(toSeq(arr.Select((cc, i) => TopologyProjection.Of(cc, feature, new ComponentIndex(ComponentIndexType.PolycurveSegment, i))))),
                _ => Optional(native.DuplicateCurve()).ToFin(op.InvalidResult()).Map(d => Seq(TopologyProjection.Of(d, feature, new ComponentIndex(ComponentIndexType.PolycurveSegment, 0)))),
            },
            _ => Optional(native.DuplicateCurve()).ToFin(op.InvalidResult()).Map(d => Seq(TopologyProjection.Of(d, feature, new ComponentIndex(ComponentIndexType.NoType, 0)))),
        }));
    private static Fin<Seq<TopologyProjection>> SelectTopologyFeatures<TPrimitive>(IEnumerable<TPrimitive> source, Curves selector, Func<TPrimitive, EdgeDescriptor> describe, Func<TPrimitive, Option<TopologyProjection>> project) =>
        Fin.Succ(toSeq(source).Choose(item => selector.Matches(descriptor: describe(arg: item)) ? project(arg: item) : Option<TopologyProjection>.None));
    private static Fin<Seq<TopologyProjection>> NakedBrepEdgesOf(Brep brep, CurveFeature feature, ComponentIndex source) =>
        Fin.Succ(toSeq(brep.Edges).Choose(edge => edge.Valence == EdgeAdjacency.Naked
            ? Optional(edge.DuplicateCurve()).Map(curve => TopologyProjection.Of(curve: curve, feature: feature, source: source.ComponentIndexType == ComponentIndexType.NoType ? new ComponentIndex(ComponentIndexType.BrepEdge, edge.EdgeIndex) : source))
            : Option<TopologyProjection>.None));
    private static Fin<Seq<TopologyProjection>> SubDEdges(SubD subd, CurveFeature feature) {
        _ = subd.UpdateSurfaceMeshCache(true);
        return Fin.Succ(toSeq(subd.DuplicateEdgeCurves().Select((c, i) => TopologyProjection.Of(c, feature, new ComponentIndex(ComponentIndexType.SubdEdge, i)))));
    }
    private static Fin<Seq<TopologyProjection>> SilhouettesOf(GeometryBase geometry, Curves aspect, CurveFeature feature, Context context, Op op, CancellationToken cancel) =>
        cancel.IsCancellationRequested
            ? Fin.Fail<Seq<TopologyProjection>>(new Fault.Cancelled())
            : (aspect switch {
                Curves.SilhouetteCase { Query: SilhouetteQuery.Projecting projecting } => Optional(projecting.Direction),
                Curves.SilhouetteCase { Query: SilhouetteQuery.Draft draft } => Optional(draft.Direction),
                _ => Option<Vector3d>.None,
            }).IfNone(static () => Vector3d.ZAxis) switch {
                Vector3d dir when dir.IsValid && !dir.IsTiny() =>
                    (geometry switch {
                        Brep or BrepFace or Mesh or Extrusion => Fin.Succ((Geometry: geometry, Owned: Option<GeometryBase>.None)),
                        Surface surface => Optional(surface.ToBrep()).ToFin(op.InvalidResult()).Map(static b => (Geometry: (GeometryBase)b, Owned: Some((GeometryBase)b))),
                        SubD subd => Optional(subd.ToBrep(SubDToBrepOptions.Default)).ToFin(op.InvalidResult()).Map(static b => (Geometry: (GeometryBase)b, Owned: Some((GeometryBase)b))),
                        _ => Fin.Fail<(GeometryBase Geometry, Option<GeometryBase> Owned)>(op.Unsupported(geometry.GetType(), typeof(Curve))),
                    }).Bind(shape => {
                        Fin<Seq<TopologyProjection>> result = Optional((aspect switch {
                            Curves.SilhouetteCase { Query: SilhouetteQuery.Draft draft } => Some(draft.Angle.IfNone(0.0)),
                            _ => Option<double>.None,
                        }).Case switch {
                            double angle => Silhouette.ComputeDraftCurve(shape.Geometry, angle, dir, context.Absolute.Value, context.Angle.Value, cancel),
                            _ => Silhouette.Compute(shape.Geometry, SilhouetteType.Projecting | SilhouetteType.TangentProjects | SilhouetteType.Tangent | SilhouetteType.Crease | SilhouetteType.Boundary, dir, context.Absolute.Value, context.Angle.Value, [], cancel),
                        }).ToFin(cancel.IsCancellationRequested ? (Error)new Fault.Cancelled() : op.InvalidResult())
                            .Map(arr => toSeq(arr).Map(sil => TopologyProjection.Of(sil.Curve, feature, sil.GeometryComponentIndex)));
                        _ = shape.Owned.Iter(static geom => geom.Dispose());
                        return result;
                    }),
                _ => Fin.Fail<Seq<TopologyProjection>>(op.Unsupported(geometry.GetType(), typeof(Curve))),
            };
}
