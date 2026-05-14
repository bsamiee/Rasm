namespace Radyab.Components;

[IoId("D702F9C9-A371-4038-A311-1C477F5497E9")]
[Nomen(
    name: "Extract Curves",
    info: "All, boundary, mid-domain U/V iso, and index-clamped curves from Rhino curve, surface, Brep, SubD, and mesh topology values.",
    category: Library.Name,
    section: Library.Extraction)]
public sealed class ExtractCurves : Component {
    private static readonly Port<Shape> Geometry = Port.Shape();
    private static readonly Port<int> Index = Port.Index(info: "Zero-based curve selector; missing Index defaults to curve 0 and supplied values clamp to [0, count-1].");
    private static readonly Port<Vector3d> Direction = Port.Direction(info: "Parallel projection or pull direction for silhouette and draft extraction. Missing Direction uses world Z.");
    private static readonly Port<Angle> DraftAngle = Port.Optional<Angle>(name: "Draft Angle", code: "A", info: "Draft angle for draft-curve extraction. Missing Draft Angle uses 0 radians.", fallback: Some(Angle.Zero));
    private static readonly IOutputGroup AllCurves = Output.Details<CurveProjection>(
        input: Geometry,
        aspect: static _ => shape => Rasm.Analysis.Query.CurveProjections(geometry: shape.Inner, aspect: Curves.All),
        emptyUnsupported: true,
        aspectLabel: nameof(Curves),
        slots: [
            Output.Plain<CurveProjection, Curve>(port: Port.Tree<Curve>(name: "All Curves", code: "AC", info: "All native curve pieces: structural curve segments, Brep/SubD edges, surface boundary iso curves, or mesh topology edge curves."), project: static value => value.Curve),
            Output.Plain<CurveProjection, ComponentIndex>(port: Port.Tree<ComponentIndex>(name: "Source", code: "SRC", info: "Component index aligned with All Curves."), project: static value => value.Source),
            Output.Plain<CurveProjection, CurveFeature>(port: Port.Tree<CurveFeature>(kind: PortKind.Enum(initial: CurveFeature.Input), name: "Feature", code: "F", info: "Feature label aligned with All Curves."), project: static value => value.Feature),
        ]);
    private static readonly IOutputGroup Segments = Output.Query(input: Geometry, port: Port.Tree<Curve>(name: "Segments", code: "SEG", info: "Structural segments from the curve data model, Brep/SubD edges, or mesh topology edge lines."), aspect: Curves.Segments);
    private static readonly IOutputGroup SubCurves = Output.Query(input: Geometry, port: Port.Tree<Curve>(name: "Sub Curves", code: "SCV", info: "Geometry-based curve subcurves, matching Rhino explode-style curve decomposition."), aspect: Curves.SubCurves);
    private static readonly IOutputGroup Boundary = Output.Query(input: Geometry, port: Port.Tree<Curve>(name: "Boundary Curves", code: "BC", info: "Boundary curves: naked Brep/mesh edges, surface/face boundary iso curves, or the input curve itself."), aspect: Curves.Boundary);
    private static readonly IOutputGroup NakedOuter = Output.Query(input: Geometry, port: Port.Tree<Curve>(name: "Naked Outer", code: "NO", info: "Outer naked Brep edges; mesh naked boundary polylines where mesh topology has no inner/outer split."), aspect: Curves.NakedOuter);
    private static readonly IOutputGroup NakedInner = Output.Query(input: Geometry, port: Port.Tree<Curve>(name: "Naked Inner", code: "NI", info: "Inner naked Brep loop edges."), aspect: Curves.NakedInner);
    private static readonly IOutputGroup Interior = Output.Query(input: Geometry, port: Port.Tree<Curve>(name: "Interior Edges", code: "IE", info: "Brep interior edges and mesh topology edges with exactly two connected faces."), aspect: Curves.Interior);
    private static readonly IOutputGroup NonManifold = Output.Query(input: Geometry, port: Port.Tree<Curve>(name: "Non-Manifold Edges", code: "NE", info: "Brep non-manifold edges and mesh topology edges with more than two connected faces."), aspect: Curves.NonManifold);
    private static readonly IOutputGroup OuterLoops = Output.Query(input: Geometry, port: Port.Tree<Curve>(name: "Outer Loops", code: "OL", info: "Brep outer loop curves."), aspect: Curves.OuterLoop);
    private static readonly IOutputGroup InnerLoops = Output.Query(input: Geometry, port: Port.Tree<Curve>(name: "Inner Loops", code: "IL", info: "Brep inner loop curves."), aspect: Curves.InnerLoop);
    private static readonly IOutputGroup IsoU = Output.Query(input: Geometry, port: Port.Tree<Curve>(name: "U Iso Curves", code: "U", info: "Trim-aware mid-domain U-direction iso curves for Brep faces and surface values."), aspect: Curves.Iso(direction: IsoStatus.X, normalized: 0.5));
    private static readonly IOutputGroup IsoV = Output.Query(input: Geometry, port: Port.Tree<Curve>(name: "V Iso Curves", code: "V", info: "Trim-aware mid-domain V-direction iso curves for Brep faces and surface values."), aspect: Curves.Iso(direction: IsoStatus.Y, normalized: 0.5));
    private static readonly IOutputGroup Silhouette = Output.Query<Curves, Curve>(input: Geometry, port: Port.Tree<Curve>(name: "Silhouette Curves", code: "SC", info: "Parallel-projection silhouette curves using Direction, defaulting to world Z."), aspect: runtime => Curves.Silhouette(direction: runtime.ReadOrInvalid(port: Direction, invalid: Vector3d.Unset).ToNullable()));
    private static readonly IOutputGroup Draft = Output.Query<Curves, Curve>(input: Geometry, port: Port.Tree<Curve>(name: "Draft Curves", code: "DC", info: "Draft transition curves using Direction as pull direction and Draft Angle, defaulting to 0 radians."), aspect: runtime => Curves.Draft(direction: runtime.ReadOrInvalid(port: Direction, invalid: Vector3d.Unset).ToNullable(), angle: runtime.ReadOrInvalid(port: DraftAngle, invalid: default).ToNullable()?.Radians));
    private static readonly IOutputGroup Indexed = Output.Details<CurveProjection>(
        input: Geometry,
        aspect: runtime => shape => Rasm.Analysis.Query.CurveProjections(geometry: shape.Inner, choose: count => Curves.At(index: runtime.Index(port: Index, limit: count).ToNullable())),
        emptyUnsupported: false,
        aspectLabel: nameof(Curves),
        slots: [
            Output.Plain<CurveProjection, Curve>(port: Port.Tree<Curve>(name: "Indexed Curve", code: "IC", info: "Curve at Index input using GH native index modifiers. Empty when zero curves."), project: static value => value.Curve),
        ]);
    public ExtractCurves() : base(self: typeof(ExtractCurves), spec: ComponentSpec.Of(
        inputs: Seq<IPort>(Geometry, Index, Direction, DraftAngle),
        outputs: Seq<IOutputGroup>(AllCurves, Segments, SubCurves, Boundary, NakedOuter, NakedInner, Interior, NonManifold, OuterLoops, InnerLoops, IsoU, IsoV, Silhouette, Draft, Indexed))) { }
}
