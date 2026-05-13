namespace Radyab.Components;

[IoId("B88A1248-28B8-4F87-A178-6BCBD2458B33")]
[Nomen(
    name: "Extract Curves",
    info: "All, boundary, mid-domain U/V iso, and index-clamped curves from Rhino curve, surface, Brep, SubD, and mesh topology values.",
    category: Library.Name,
    section: Library.Extraction)]
public sealed class ExtractCurves : Component {
    public ExtractCurves() : base(self: typeof(ExtractCurves)) { }
    [Input] private readonly Port<Shape> Geometry = Port.Shape();
    [Input] private readonly Port<int> Index = Port.Index(info: "Zero-based curve selector; missing Index defaults to curve 0 and supplied values clamp to [0, count-1].");
    [Input] private readonly Port<Vector3d> Direction = Port.Direction(info: "Parallel projection or pull direction for silhouette and draft extraction. Missing Direction uses world Z.");
    [Input] private readonly Port<Angle> DraftAngle = Port.Optional<Angle>(name: "Draft Angle", code: "A", info: "Draft angle for draft-curve extraction. Missing Draft Angle uses 0 radians.");
    [Output]
    private IOutputGroup AllCurves => Output.Details<CurveProjection>(
        input: Geometry,
        aspect: static _ => shape => Rasm.Analysis.Query.CurveProjections(geometry: shape.Inner, aspect: Curves.All),
        emptyUnsupported: true,
        aspectLabel: nameof(Curves),
        slots: [
            Output.Plain<CurveProjection, Curve>(port: Port.List<Curve>(name: "All Curves", code: "AC", info: "All native curve pieces: structural curve segments, Brep/SubD edges, surface boundary iso curves, or mesh topology edge curves."), project: static value => value.Curve),
            Output.Plain<CurveProjection, ComponentIndex>(port: Port.List<ComponentIndex>(name: "Source", code: "S", info: "Component index aligned with All Curves."), project: static value => value.Source),
            Output.Plain<CurveProjection, CurveFeature>(port: Port.List<CurveFeature>(kind: PortKind.Enum(initial: CurveFeature.Input), name: "Feature", code: "F", info: "Feature label aligned with All Curves."), project: static value => value.Feature),
        ]);
    [Output] private IOutputGroup Segments => Output.Query(input: Geometry, port: Port.List<Curve>(name: "Segments", code: "S", info: "Structural segments from the curve data model, Brep/SubD edges, or mesh topology edge lines."), aspect: Curves.Segments);
    [Output] private IOutputGroup SubCurves => Output.Query(input: Geometry, port: Port.List<Curve>(name: "Sub Curves", code: "SCV", info: "Geometry-based curve subcurves, matching Rhino explode-style curve decomposition."), aspect: Curves.SubCurves);
    [Output] private IOutputGroup Boundary => Output.Query(input: Geometry, port: Port.List<Curve>(name: "Boundary Curves", code: "BC", info: "Boundary curves: naked Brep/mesh edges, surface/face boundary iso curves, or the input curve itself."), aspect: Curves.Boundary);
    [Output] private IOutputGroup NakedOuter => Output.Query(input: Geometry, port: Port.List<Curve>(name: "Naked Outer", code: "NO", info: "Outer naked Brep edges; mesh naked boundary polylines where mesh topology has no inner/outer split."), aspect: Curves.NakedOuter);
    [Output] private IOutputGroup NakedInner => Output.Query(input: Geometry, port: Port.List<Curve>(name: "Naked Inner", code: "NI", info: "Inner naked Brep loop edges."), aspect: Curves.NakedInner);
    [Output] private IOutputGroup Interior => Output.Query(input: Geometry, port: Port.List<Curve>(name: "Interior Edges", code: "IE", info: "Brep interior edges and mesh topology edges with exactly two connected faces."), aspect: Curves.Interior);
    [Output] private IOutputGroup NonManifold => Output.Query(input: Geometry, port: Port.List<Curve>(name: "Non-Manifold Edges", code: "NE", info: "Brep non-manifold edges and mesh topology edges with more than two connected faces."), aspect: Curves.NonManifold);
    [Output] private IOutputGroup OuterLoops => Output.Query(input: Geometry, port: Port.List<Curve>(name: "Outer Loops", code: "OL", info: "Brep outer loop curves."), aspect: Curves.OuterLoop);
    [Output] private IOutputGroup InnerLoops => Output.Query(input: Geometry, port: Port.List<Curve>(name: "Inner Loops", code: "IL", info: "Brep inner loop curves."), aspect: Curves.InnerLoop);
    [Output] private IOutputGroup IsoU => Output.Query(input: Geometry, port: Port.List<Curve>(name: "U Iso Curves", code: "U", info: "Trim-aware mid-domain U-direction iso curves for Brep faces and surface values."), aspect: Curves.Iso(direction: IsoStatus.X, normalized: 0.5));
    [Output] private IOutputGroup IsoV => Output.Query(input: Geometry, port: Port.List<Curve>(name: "V Iso Curves", code: "V", info: "Trim-aware mid-domain V-direction iso curves for Brep faces and surface values."), aspect: Curves.Iso(direction: IsoStatus.Y, normalized: 0.5));
    [Output] private IOutputGroup Silhouette => Output.Query<Curves, Curve>(input: Geometry, port: Port.List<Curve>(name: "Silhouette Curves", code: "SC", info: "Parallel-projection silhouette curves using Direction, defaulting to world Z."), aspect: runtime => Curves.Silhouette(direction: runtime.Read(port: Direction).ToNullable()));
    [Output] private IOutputGroup Draft => Output.Query<Curves, Curve>(input: Geometry, port: Port.List<Curve>(name: "Draft Curves", code: "DC", info: "Draft transition curves using Direction as pull direction and Draft Angle, defaulting to 0 radians."), aspect: runtime => Curves.Draft(direction: runtime.Read(port: Direction).ToNullable(), angle: runtime.Read(port: DraftAngle).ToNullable()?.Radians));
    [Output] private IOutputGroup Indexed => Output.Query<Curves, Curve>(input: Geometry, port: Port.List<Curve>(name: "Indexed Curve", code: "IC", info: "Curve at Index input; missing Index defaults to 0, supplied values clamp to [0, count-1]. Empty when zero curves."), aspect: runtime => Curves.At(index: runtime.Read(port: Index).ToNullable()));
}
