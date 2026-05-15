namespace Rasm.Analysis;

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public partial record Curves : IAspect {
    public sealed record AllCase : Curves; public sealed record SegmentsCase : Curves; public sealed record BoundaryCase : Curves; public sealed record NakedOuterCase : Curves; public sealed record NakedInnerCase : Curves; public sealed record InteriorCase : Curves; public sealed record NonManifoldCase : Curves; public sealed record OuterLoopCase : Curves; public sealed record InnerLoopCase : Curves; public sealed record IsoCase(IsoStatus Direction, double Normalized) : Curves; public sealed record SubCurvesCase : Curves; public sealed record SilhouetteCase(Vector3d? Direction) : Curves; public sealed record DraftCase(Vector3d? Direction, double? Angle) : Curves; public sealed record AtCase(int? Value) : Curves;
    [StructLayout(LayoutKind.Auto)] internal readonly record struct Selector(CurveFeature Feature, Option<Vector3d> Direction = default, Option<double> Angle = default, Option<double> Normalized = default, Option<IsoStatus> Iso = default);
    public static Curves All => new AllCase(); public static Curves Segments => new SegmentsCase(); public static Curves Boundary => new BoundaryCase(); public static Curves NakedOuter => new NakedOuterCase();
    public static Curves NakedInner => new NakedInnerCase(); public static Curves Interior => new InteriorCase(); public static Curves NonManifold => new NonManifoldCase(); public static Curves OuterLoop => new OuterLoopCase();
    public static Curves InnerLoop => new InnerLoopCase(); public static Curves SubCurves => new SubCurvesCase();
    public static Curves Iso(IsoStatus direction, double normalized = 0.5) => new IsoCase(Direction: direction, Normalized: normalized);
    public static Curves Silhouette(Vector3d? direction = null) => new SilhouetteCase(Direction: direction);
    public static Curves Draft(Vector3d? direction = null, double? angle = null) => new DraftCase(Direction: direction, Angle: angle);
    public static Curves At(int? index = null) => new AtCase(Value: index);
    internal static readonly Op Key = Op.Of(name: nameof(Curves));
    public global::Rasm.Analysis.Operation<TGeometry, TOut> Operation<TGeometry, TOut>() where TGeometry : notnull =>
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
    internal Selector ToSelector(Topology topology) => Switch(
        state: topology,
        allCase: static (topology, _) => new Selector(Feature: topology switch {
            Topology.Curve => CurveFeature.Input,
            Topology.Surface => CurveFeature.Boundary,
            _ => CurveFeature.Edge,
        }),
        segmentsCase: static (_, _) => new Selector(Feature: CurveFeature.Segment),
        boundaryCase: static (_, _) => new Selector(Feature: CurveFeature.Boundary),
        nakedOuterCase: static (_, _) => new Selector(Feature: CurveFeature.NakedOuter),
        nakedInnerCase: static (_, _) => new Selector(Feature: CurveFeature.NakedInner),
        interiorCase: static (_, _) => new Selector(Feature: CurveFeature.Interior),
        nonManifoldCase: static (_, _) => new Selector(Feature: CurveFeature.NonManifold),
        outerLoopCase: static (_, _) => new Selector(Feature: CurveFeature.OuterLoop),
        innerLoopCase: static (_, _) => new Selector(Feature: CurveFeature.InnerLoop),
        isoCase: static (_, iso) => new Selector(Feature: CurveFeature.Iso, Normalized: Some(iso.Normalized), Iso: Some(iso.Direction)),
        subCurvesCase: static (_, _) => new Selector(Feature: CurveFeature.SubCurve),
        silhouetteCase: static (_, silhouette) => new Selector(Feature: CurveFeature.Silhouette, Direction: Optional(silhouette.Direction)),
        draftCase: static (_, draft) => new Selector(Feature: CurveFeature.Draft, Direction: Optional(draft.Direction), Angle: Optional(draft.Angle)),
        atCase: static (topology, at) => new Selector(Feature: topology switch {
            Topology.Curve => CurveFeature.Input,
            Topology.Surface => CurveFeature.Boundary,
            _ => CurveFeature.Edge,
        }));
    internal bool CanProject(Type type) =>
        GeometryKernel.CanProjectCurves(type: type, feature: KindLookup.Resolve(type).Map(kind => ToSelector(topology: kind.Topology).Feature));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Analyze {
    public static global::Rasm.Analysis.Operation<TGeometry, TOut> Curves<TGeometry, TOut>(Curves aspect) where TGeometry : notnull => Aspect<Curves, TGeometry, TOut>(aspect: aspect);
    public static global::Rasm.Analysis.Operation<TGeometry, TOut> Segments<TGeometry, TOut>() where TGeometry : notnull =>
        Analyze.Curves<TGeometry, TOut>(aspect: Rasm.Analysis.Curves.Segments);
    public static global::Rasm.Analysis.Operation<Surface, Curve> Iso(IsoStatus iso, double normalized = 0.5) {
        Op key = Op.Of();
        return global::Rasm.Analysis.Operation<Surface, Curve>.Build(
            key: key, requirement: Requirement.SurfaceEvaluation, state: (Key: key, Iso: iso, Normalized: normalized), requiresContext: true,
            evaluator: static (state, geometry) =>
                from context in Env.Asks
                from curves in IsoSeq(surface: geometry, iso: state.Iso, normalized: state.Normalized, op: state.Key).ToEff()
                from result in state.Key.Accept(values: curves).ToEff()
                select result);
    }
    internal static global::Rasm.Analysis.Operation<TGeometry, TOut> CurveProject<TGeometry, TOut, TValue>(Op key, Curves aspect, Func<TopologyProjection, Option<TValue>> project) where TGeometry : notnull =>
        Cast<TGeometry, TOut>(key: key, operation: global::Rasm.Analysis.Operation<TGeometry, TValue>.Build(
            key: key, state: (Key: key, Aspect: aspect, Project: project), requiresContext: true,
            evaluator: static (state, geometry) =>
                from runtime in Env.EnvAsks
                from kind in ((object)geometry).Kind(context: runtime.Context).ToEff()
                let selector = state.Aspect.ToSelector(topology: kind.Topology)
                from curves in CurveProjections(geometry: geometry, selector: selector, context: runtime.Context, op: state.Key, cancel: runtime.Cancellation).ToEff()
                from chosen in state.Aspect.Select(curves: curves).ToEff()
                from result in TopologyProjection.Project(all: curves, chosen: chosen, project: values => state.Key.Accept(values: values.Choose(state.Project))).ToEff()
                select result));
    internal static Fin<Seq<TopologyProjection>> CurveProjections<TGeometry>(TGeometry geometry, Curves.Selector selector, Context context, Op op, CancellationToken cancel) where TGeometry : notnull =>
        Optional(geometry).ToFin(op.InvalidInput()).Bind(g => (g, selector.Feature) switch {
            (Curve or Line or Polyline or Circle or Arc or Ellipse, CurveFeature feature) when feature.IsCurveLike => CurveInput(source: g, selector: selector, op: op),
            (Brep brep, CurveFeature feature) when feature.IsBrepEdge => BrepEdges(brep: brep, selector: selector),
            (Brep brep, CurveFeature feature) when feature.IsLoop => BrepLoops(brep: brep, selector: selector),
            (Brep brep, CurveFeature feature) when feature.IsIso => toSeq(brep.Faces).TraverseM(f => IsoSeq(surface: f, iso: selector.Iso.IfNone(static () => IsoStatus.X), normalized: selector.Normalized.IfNone(static () => 0.5), op: op).Map(s => s.Map(c => TopologyProjection.FromCurve(c, selector.Feature, new ComponentIndex(ComponentIndexType.BrepFace, f.FaceIndex))))).As().Map(static nested => nested.Bind(static seq => seq)),
            (BrepFace face, CurveFeature feature) when feature.IsBrepFormBoundary => Optional(face.DuplicateFace(duplicateMeshes: false)).ToFin(op.InvalidResult()).Bind(fb => new Lease<Brep>.Owned(Value: fb).Use(owned => NakedBrepEdges(brep: owned, feature: feature, source: new ComponentIndex(ComponentIndexType.BrepFace, face.FaceIndex), op: op))),
            (Mesh mesh, CurveFeature feature) when feature.IsMeshEdge => MeshEdges(mesh: mesh, selector: selector),
            (Surface surface, CurveFeature feature) when feature.IsIso => IsoSeq(surface: surface, iso: selector.Iso.IfNone(static () => IsoStatus.X), normalized: selector.Normalized.IfNone(static () => 0.5), op: op).Map(seq => seq.Map(c => TopologyProjection.FromCurve(c, selector.Feature, new ComponentIndex(ComponentIndexType.NoType, 0)))),
            (GeometryBase { HasBrepForm: true } native, CurveFeature feature) when feature.IsBrepFormBoundary => GeometryKernel.BrepForm(source: native, op: op).Bind(lease => lease.Use(brep => NakedBrepEdges(brep: brep, feature: feature, source: new ComponentIndex(ComponentIndexType.NoType, 0), op: op))),
            (SubD subd, CurveFeature feature) when feature.IsSubDTopology => SubDEdges(subd: subd, selector: selector),
            (GeometryBase native, CurveFeature feature) when feature.IsSilhouette => Silhouettes(geometry: native, selector: selector, context: context, op: op, cancel: cancel),
            _ => Fin.Fail<Seq<TopologyProjection>>(op.Unsupported(g.GetType(), typeof(Curve))),
        });
    internal static Fin<Seq<Curve>> IsoSeq(Surface surface, IsoStatus iso, double normalized, Op op) => (iso, normalized is >= 0.0 and <= 1.0) switch {
        (IsoStatus.West or IsoStatus.South or IsoStatus.East or IsoStatus.North, _) => Optional(surface.IsoCurve(iso)).ToFin(op.InvalidResult()).Map(static c => Seq(c)),
        (IsoStatus.X or IsoStatus.Y, true) when surface.Domain(iso == IsoStatus.X ? 0 : 1) is { IsValid: true } d =>
            surface is BrepFace face ? Fin.Succ(toSeq(face.TrimAwareIsoCurve(iso == IsoStatus.X ? 0 : 1, d.ParameterAt(normalized))))
                : Optional(surface.IsoCurve(iso, d.ParameterAt(normalized))).ToFin(op.InvalidResult()).Map(static c => Seq(c)),
        _ => Fin.Fail<Seq<Curve>>(op.InvalidInput()),
    };
    private static Fin<Seq<TopologyProjection>> CurveInput(object source, Curves.Selector selector, Op op) =>
        source switch {
            Curve curve => CurveInputNative(native: curve, selector: selector, op: op),
            _ => GeometryKernel.CurveForm(source: source, op: op).Bind(lease => lease.Use(native => CurveInputNative(native: native, selector: selector, op: op))),
        };
    private static Fin<Seq<TopologyProjection>> CurveInputNative(Curve native, Curves.Selector selector, Op op) =>
        selector.Feature.IsSegmentLike
            ? Optional(selector.Feature.IsSubCurve ? native.GetSubCurves() : native.DuplicateSegments()) switch {
                Option<Curve[]> opt when opt.Case is Curve[] arr && arr.Length > 0 => Fin.Succ(toSeq(arr.Select((cc, i) => TopologyProjection.FromCurve(cc, selector.Feature, ComponentIndexType.PolycurveSegment, i)))),
                _ => Optional(native.DuplicateCurve()).ToFin(op.InvalidResult()).Map(d => Seq(TopologyProjection.FromCurve(d, selector.Feature, ComponentIndexType.PolycurveSegment, 0))),
            }
            : Optional(native.DuplicateCurve()).ToFin(op.InvalidResult()).Map(d => Seq(TopologyProjection.FromCurve(d, selector.Feature, ComponentIndexType.NoType, 0)));
    private static Fin<Seq<TopologyProjection>> BrepEdges(Brep brep, Curves.Selector selector) =>
        Fin.Succ(toSeq(brep.Edges).Where(e => selector.Feature.MatchesBrepEdge(valence: e.Valence, loops: toSeq(e.TrimIndices()).Choose(t => Optional(e.Brep.Trims[t].Loop).Map(static loop => loop.LoopType)))).Bind(e => Optional(e.DuplicateCurve()).Map(c => TopologyProjection.FromCurve(c, selector.Feature, ComponentIndexType.BrepEdge, e.EdgeIndex)).ToSeq()));
    private static Fin<Seq<TopologyProjection>> MeshEdges(Mesh mesh, Curves.Selector selector) =>
        Fin.Succ(toSeq(Enumerable.Range(0, mesh.TopologyEdges.Count)).Where(i => selector.Feature.MatchesMeshEdge(connectedFaces: mesh.TopologyEdges.GetConnectedFaces(i).Length)).Map(i => TopologyProjection.FromCurve(mesh.TopologyEdges.EdgeLine(i).ToNurbsCurve(), selector.Feature, ComponentIndexType.MeshTopologyEdge, i)));
    private static Fin<Seq<TopologyProjection>> BrepLoops(Brep brep, Curves.Selector selector) =>
        Fin.Succ(toSeq(brep.Loops).Where(l => selector.Feature.MatchesBrepLoop(loop: l.LoopType)).Bind(l => Optional(l.To3dCurve()).Map(c => TopologyProjection.FromCurve(c, selector.Feature, ComponentIndexType.BrepLoop, l.LoopIndex)).ToSeq()));
    private static Fin<Seq<TopologyProjection>> NakedBrepEdges(Brep brep, CurveFeature feature, ComponentIndex source, Op op) =>
        Optional(brep.DuplicateNakedEdgeCurves(nakedOuter: true, nakedInner: true))
            .ToFin(op.InvalidResult())
            .Map(curves => toSeq(curves.Select((curve, index) => TopologyProjection.FromCurve(curve: curve, feature: feature, source: source.ComponentIndexType == ComponentIndexType.NoType ? new ComponentIndex(ComponentIndexType.BrepEdge, index) : source)).ToArray()));
    private static Fin<Seq<TopologyProjection>> SubDEdges(SubD subd, Curves.Selector selector) {
        _ = subd.UpdateSurfaceMeshCache(true);
        return Fin.Succ(toSeq(subd.DuplicateEdgeCurves().Select((c, i) => TopologyProjection.FromCurve(c, selector.Feature, ComponentIndexType.SubdEdge, i))));
    }
    private static Fin<Seq<TopologyProjection>> Silhouettes(GeometryBase geometry, Curves.Selector selector, Context context, Op op, CancellationToken cancel) =>
        cancel.IsCancellationRequested
            ? Fin.Fail<Seq<TopologyProjection>>(new Fault.Cancelled())
            : selector.Direction.IfNone(static () => Vector3d.ZAxis) switch {
                Vector3d dir when dir.IsValid && !dir.IsTiny() =>
                    (geometry switch {
                        Brep or BrepFace or Mesh or Extrusion => Fin.Succ((Geometry: geometry, Owned: Option<GeometryBase>.None)),
                        Surface surface => Optional(surface.ToBrep()).ToFin(op.InvalidResult()).Map(static b => (Geometry: (GeometryBase)b, Owned: Some((GeometryBase)b))),
                        SubD subd => Optional(subd.ToBrep(SubDToBrepOptions.Default)).ToFin(op.InvalidResult()).Map(static b => (Geometry: (GeometryBase)b, Owned: Some((GeometryBase)b))),
                        _ => Fin.Fail<(GeometryBase Geometry, Option<GeometryBase> Owned)>(op.Unsupported(geometry.GetType(), typeof(Curve))),
                    }).Bind(shape => {
                        Fin<Seq<TopologyProjection>> result = Optional((selector.Feature.IsDraft ? Some(selector.Angle.IfNone(static () => 0.0)) : None).Case switch {
                            double angle => Silhouette.ComputeDraftCurve(shape.Geometry, angle, dir, context.Absolute.Value, context.Angle.Value, cancel),
                            _ => Silhouette.Compute(shape.Geometry, SilhouetteType.Projecting | SilhouetteType.TangentProjects | SilhouetteType.Tangent | SilhouetteType.Crease | SilhouetteType.Boundary, dir, context.Absolute.Value, context.Angle.Value, [], cancel),
                        }).ToFin(cancel.IsCancellationRequested ? (Error)new Fault.Cancelled() : op.InvalidResult())
                            .Map(arr => toSeq(arr).Map(sil => TopologyProjection.FromCurve(sil.Curve, selector.Feature, sil.GeometryComponentIndex)));
                        _ = shape.Owned.Iter(static geom => geom.Dispose());
                        return result;
                    }),
                _ => Fin.Fail<Seq<TopologyProjection>>(op.Unsupported(geometry.GetType(), typeof(Curve))),
            };
}
