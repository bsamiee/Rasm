namespace Rasm.Analysis;

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public partial record Curves : IAspect {
    public sealed record AllCase : Curves; public sealed record SegmentsCase : Curves; public sealed record BoundaryCase : Curves; public sealed record NakedOuterCase : Curves; public sealed record NakedInnerCase : Curves; public sealed record InteriorCase : Curves; public sealed record NonManifoldCase : Curves; public sealed record OuterLoopCase : Curves; public sealed record InnerLoopCase : Curves; public sealed record IsoCase(IsoStatus Direction, double Normalized) : Curves; public sealed record SubCurvesCase : Curves; public sealed record SilhouetteCase(Vector3d? Direction) : Curves; public sealed record DraftCase(Vector3d? Direction, double? Angle) : Curves; public sealed record AtCase(int? Value) : Curves;
    public static Curves All => new AllCase(); public static Curves Segments => new SegmentsCase(); public static Curves Boundary => new BoundaryCase(); public static Curves NakedOuter => new NakedOuterCase();
    public static Curves NakedInner => new NakedInnerCase(); public static Curves Interior => new InteriorCase(); public static Curves NonManifold => new NonManifoldCase(); public static Curves OuterLoop => new OuterLoopCase();
    public static Curves InnerLoop => new InnerLoopCase(); public static Curves SubCurves => new SubCurvesCase();
    public static Curves Iso(IsoStatus direction, double normalized = 0.5) => new IsoCase(Direction: direction, Normalized: normalized);
    public static Curves Silhouette(Vector3d? direction = null) => new SilhouetteCase(Direction: direction);
    public static Curves Draft(Vector3d? direction = null, double? angle = null) => new DraftCase(Direction: direction, Angle: angle);
    public static Curves At(int? index = null) => new AtCase(Value: index);
    internal static readonly Op Key = Op.Of(name: nameof(Curves));
    public Query<TGeometry, TOut> ToQuery<TGeometry, TOut>() where TGeometry : notnull =>
        Dispatch.Supports(CapTag.Curves, typeof(TGeometry)) switch {
            false => Key.Unsupported<TGeometry, TOut>(),
            true => typeof(TOut) switch {
                Type t when t == typeof(Curve) => Analyze.CurveProject<TGeometry, TOut, Curve>(key: Key, aspect: this, project: static p => p.As<Curve>()),
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
    internal CurveSelector ToSelector(Topology topology) => this switch {
        AllCase => new(Feature: topology switch {
            Topology.Curve => CurveFeature.Input,
            Topology.Surface => CurveFeature.Boundary,
            _ => CurveFeature.Edge,
        }),
        SegmentsCase => new(Feature: CurveFeature.Segment),
        BoundaryCase => new(Feature: CurveFeature.Boundary),
        NakedOuterCase => new(Feature: CurveFeature.NakedOuter),
        NakedInnerCase => new(Feature: CurveFeature.NakedInner),
        InteriorCase => new(Feature: CurveFeature.Interior),
        NonManifoldCase => new(Feature: CurveFeature.NonManifold),
        OuterLoopCase => new(Feature: CurveFeature.OuterLoop),
        InnerLoopCase => new(Feature: CurveFeature.InnerLoop),
        SubCurvesCase => new(Feature: CurveFeature.SubCurve),
        IsoCase iso => new(Feature: CurveFeature.Iso, Normalized: Some(iso.Normalized), Iso: Some(iso.Direction)),
        SilhouetteCase s => new(Feature: CurveFeature.Silhouette, Direction: Optional(s.Direction)),
        DraftCase d => new(Feature: CurveFeature.Draft, Direction: Optional(d.Direction), Angle: Optional(d.Angle)),
        AtCase at => new(Feature: topology switch {
            Topology.Curve => CurveFeature.Input,
            Topology.Surface => CurveFeature.Boundary,
            _ => CurveFeature.Edge,
        }, Index: Optional(at.Value)),
        _ => new(Feature: CurveFeature.Input),
    };
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Analyze {
    public static Query<TGeometry, TOut> Curves<TGeometry, TOut>(Curves aspect) where TGeometry : notnull => Aspect<Curves, TGeometry, TOut>(aspect: aspect);
    public static Query<TGeometry, TOut> Segments<TGeometry, TOut>() where TGeometry : notnull =>
        Analyze.Curves<TGeometry, TOut>(aspect: Rasm.Analysis.Curves.Segments);
    public static Query<Surface, Curve> Iso(IsoStatus iso, double normalized = 0.5) {
        Op key = Op.Of();
        return Query<Surface, Curve>.Build(
            key: key, requirement: Requirement.SurfaceEvaluation, state: (Key: key, Iso: iso, Normalized: normalized), requiresContext: true,
            evaluator: static (state, geometry) =>
                from context in Env.Asks
                from curves in Dispatch.Resolve<Seq<Curve>, (IsoStatus, double, Op)>(CapTag.IsoCurves, geometry, (state.Iso, state.Normalized, state.Key), state.Key).ToEff()
                from result in Many(key: state.Key, values: curves).ToEff()
                select result);
    }
    internal static Query<TGeometry, TOut> CurveProject<TGeometry, TOut, TValue>(Op key, Curves aspect, Func<TopologyProjection, Option<TValue>> project) where TGeometry : notnull =>
        Cast<TGeometry, TOut>(key: key, query: Query<TGeometry, TValue>.Build(
            key: key, state: (Key: key, Aspect: aspect, Project: project), requiresContext: true,
            evaluator: static (state, geometry) =>
                from runtime in Env.EnvAsks
                from kind in ((object)geometry).Kind(context: runtime.Context).ToEff()
                let selector = state.Aspect.ToSelector(topology: kind.Topology)
                from curves in Dispatch.Resolve<Seq<TopologyProjection>, (CurveSelector, Context, Op, CancellationToken)>(CapTag.Curves, geometry, (selector, runtime.Context, state.Key, runtime.Cancellation), state.Key, variant: selector.Feature).ToEff()
                from chosen in state.Aspect.Select(curves: curves).ToEff()
                from result in ProjectOwned(all: curves, chosen: chosen, ownership: typeof(TValue) == typeof(Curve) ? ProjectionOwnership.Transfer : ProjectionOwnership.Dispose, project: values => Many(key: state.Key, values: values.Choose(state.Project))).ToEff()
                select result));
    public static Eff<Env, Seq<TopologyProjection>> TopologyProjections(object geometry, Curves aspect) =>
        from runtime in Env.EnvAsks
        from kind in geometry.Kind(context: runtime.Context).ToEff()
        let selector = aspect.ToSelector(topology: kind.Topology)
        from curves in Dispatch.Resolve<Seq<TopologyProjection>, (CurveSelector, Context, Op, CancellationToken)>(CapTag.Curves, geometry, (selector, runtime.Context, Rasm.Analysis.Curves.Key, runtime.Cancellation), Rasm.Analysis.Curves.Key, variant: selector.Feature).ToEff()
        from chosen in aspect.Select(curves: curves).ToEff()
        from result in ProjectOwned(all: curves, chosen: chosen, ownership: ProjectionOwnership.Transfer, project: static values => Fin.Succ(values)).ToEff()
        select result;
    public static Eff<Env, Seq<TopologyProjection>> TopologyProjections(object geometry, Func<int, Curves> choose) =>
        from runtime in Env.EnvAsks
        from kind in geometry.Kind(context: runtime.Context).ToEff()
        let allSelector = Rasm.Analysis.Curves.All.ToSelector(topology: kind.Topology)
        from curves in Dispatch.Resolve<Seq<TopologyProjection>, (CurveSelector, Context, Op, CancellationToken)>(CapTag.Curves, geometry, (allSelector, runtime.Context, Rasm.Analysis.Curves.Key, runtime.Cancellation), Rasm.Analysis.Curves.Key, variant: allSelector.Feature).ToEff()
        from aspect in Fin.Succ(choose(arg: curves.Count)).ToEff()
        from chosen in aspect.Select(curves: curves).ToEff()
        from result in ProjectOwned(all: curves, chosen: chosen, ownership: ProjectionOwnership.Transfer, project: static values => Fin.Succ(values)).ToEff()
        select result;
}
