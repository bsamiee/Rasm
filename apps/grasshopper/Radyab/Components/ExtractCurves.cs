namespace Radyab.Components;

[IoId("D702F9C9-A371-4038-A311-1C477F5497E9")]
[Nomen(
    name: "Extract Curves",
    info: "Polycurve segments, direction-aware iso curves, parallel silhouette and draft curves, plus classified curve topology from Rhino curve, surface, Brep, SubD, and mesh values.",
    category: Library.Name,
    section: Library.Extraction)]
public sealed class ExtractCurves : Component {
    private static readonly Port<Shape> Geometry = Port.Shape();
    private static readonly Port<Vector3d> Direction = Port.Direction(info: "Parallel projection or pull direction for silhouette, draft, and iso direction; missing Direction uses world Z (iso U).");
    private static readonly Port<Angle> Angle = Port.Angle(info: "Draft angle for draft-curve extraction; missing Angle defaults to 0 radians.");
    private static readonly OutputGroup Segments = Output.Single(input: Geometry, port: Port.Tree<Curve>(name: "Segments", code: "S", info: "Structural segments from the curve data model, Brep/SubD edges, or mesh topology edge lines."), aspect: Curves.Segments);
    private static readonly OutputGroup IsoCurves = Output.Single<Curves, Curve>(
        input: Geometry,
        port: Port.Tree<Curve>(name: "Iso Curves", code: "I", info: "Trim-aware mid-domain iso curves for Brep faces and surfaces; |Direction.Y| > |Direction.X| picks V-iso, otherwise U-iso."),
        aspect: runtime => runtime.Read(port: Direction).Map(direction => Curves.Iso(
            direction: direction.Map(static v => Math.Abs(v.Y) > Math.Abs(v.X) ? IsoStatus.Y : IsoStatus.X).IfNone(IsoStatus.X),
            normalized: 0.5)));
    private static readonly OutputGroup Silhouette = Output.Single<Curves, Curve>(
        input: Geometry,
        port: Port.Tree<Curve>(name: "Silhouette", code: "SL", info: "Parallel-projection silhouette curves using Direction; missing Direction uses world Z."),
        aspect: runtime => runtime.Read(port: Direction).Map(direction => Curves.Silhouette(direction: direction.ToNullable())));
    private static readonly OutputGroup Draft = Output.Single<Curves, Curve>(
        input: Geometry,
        port: Port.Tree<Curve>(name: "Draft", code: "DR", info: "Draft transition curves using Direction as pull direction and Angle; missing inputs use world Z and 0 radians."),
        aspect: runtime => (runtime.Read(port: Direction), runtime.Read(port: Angle))
            .Apply(static (direction, angle) => Curves.Draft(direction: direction.ToNullable(), angle: angle.ToNullable()?.Radians))
            .As());
    private static readonly OutputGroup Topology = Output.Details<TopologyProjection>(
        input: Geometry,
        aspect: static _ => Fin.Succ<Func<Shape, Eff<Env, Seq<TopologyProjection>>>>(shape => Rasm.Analysis.Analyze.TopologyProjections(geometry: shape.Inner, aspect: Curves.All)),
        emptyUnsupported: true,
        aspectLabel: nameof(Curves),
        slots: [
            Output.Choose<TopologyProjection, Curve>(port: Port.Tree<Curve>(name: "Curves", code: "C", info: "Every curve piece: structural segments, Brep/SubD edges, surface boundary iso curves, mesh topology edge curves, and loop curves."), project: static value => value.As<Curve>()),
            Output.Plain<TopologyProjection, ComponentIndex>(port: Port.Tree<ComponentIndex>(name: "Source", code: "X", info: "Source component index aligned with each curve."), project: static value => value.Source),
            Output.Plain<TopologyProjection, CurveFeature>(port: Port.Tree<CurveFeature>(kind: PortKind.Enum(initial: CurveFeature.Input), name: "Feature", code: "F", info: "Feature classification aligned with each curve (boundary, naked, interior, loop, segment, iso, silhouette, draft)."), project: static value => value.Feature),
        ]);
    public ExtractCurves() : base(self: typeof(ExtractCurves), spec: ComponentSpec.Of(
        inputs: Seq<IPort>(Geometry, Direction, Angle),
        outputs: Seq<OutputGroup>(Segments, IsoCurves, Silhouette, Draft, Topology))) { }
}
