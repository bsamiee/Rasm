using Grasshopper2.Components;
using Grasshopper2.Types.Numeric;
using Grasshopper2.UI;
using GrasshopperIO;
using LanguageExt;
using Rasm.Analysis;
using Rasm.Domain;
using Rasm.Grasshopper;
using Rhino.Geometry;
using static LanguageExt.Prelude;
using Query = Rasm.Analysis.Query;

namespace Radyab.Components;

// --- [EXPORTS] -------------------------------------------------------------------------

[IoId("B88A1248-28B8-4F87-A178-6BCBD2458B33")]
[Nomen(
    name: "Extract Curves",
    info: "All, boundary, mid-domain U/V iso, and index-clamped curves from Rhino curve, surface, Brep, SubD, and mesh topology values.",
    category: "Radyab",
    section: "Extraction")]
public sealed class ExtractCurves : ShapeComponent {
    private static readonly Port<int> Index = Port.Index(info: "Zero-based curve selector; missing Index defaults to curve 0 and supplied values clamp to [0, count-1].");
    private static readonly Port<Vector3d> Direction = Port.Optional<Vector3d>(name: "Direction", code: "D", info: "Parallel projection or pull direction for silhouette and draft extraction. Missing Direction uses world Z.");
    private static readonly Port<Angle> DraftAngle = Port.Optional<Angle>(param: Param.Angle, name: "Draft Angle", code: "A", info: "Draft angle for draft-curve extraction. Missing Draft Angle uses 0 radians.");
    private static readonly Seq<IPort> ControlPorts = Seq<IPort>(Index, Direction, DraftAngle);
    private static readonly Seq<IOutput<Shape>> OutputSlots = Seq<IOutput<Shape>>(
        ShapeOutput(port: Port.List<Curve>(name: "All Curves", code: "AC", info: "All native curve pieces: structural curve segments, Brep/SubD edges, surface boundary iso curves, or mesh topology edge curves."), query: Query.Curves<object, Curve>(aspect: Curves.All)),
        ShapeOutput(port: Port.List<Curve>(name: "Segments", code: "S", info: "Structural segments from the curve data model, Brep/SubD edges, or mesh topology edge lines."), query: Query.Curves<object, Curve>(aspect: Curves.Segments)),
        ShapeOutput(port: Port.List<Curve>(name: "Sub Curves", code: "SCV", info: "Geometry-based curve subcurves, matching Rhino explode-style curve decomposition."), query: Query.Curves<object, Curve>(aspect: Curves.SubCurves)),
        ShapeOutput(port: Port.List<Curve>(name: "Boundary Curves", code: "BC", info: "Boundary curves: naked Brep/mesh edges, surface/face boundary iso curves, or the input curve itself."), query: Query.Curves<object, Curve>(aspect: Curves.Boundary)),
        ShapeOutput(port: Port.List<Curve>(name: "Naked Outer", code: "NO", info: "Outer naked Brep edges; mesh naked boundary polylines where mesh topology has no inner/outer split."), query: Query.Curves<object, Curve>(aspect: Curves.NakedOuter)),
        ShapeOutput(port: Port.List<Curve>(name: "Naked Inner", code: "NI", info: "Inner naked Brep loop edges."), query: Query.Curves<object, Curve>(aspect: Curves.NakedInner)),
        ShapeOutput(port: Port.List<Curve>(name: "Interior Edges", code: "IE", info: "Brep interior edges and mesh topology edges with exactly two connected faces."), query: Query.Curves<object, Curve>(aspect: Curves.Interior)),
        ShapeOutput(port: Port.List<Curve>(name: "Non-Manifold Edges", code: "NE", info: "Brep non-manifold edges and mesh topology edges with more than two connected faces."), query: Query.Curves<object, Curve>(aspect: Curves.NonManifold)),
        ShapeOutput(port: Port.List<Curve>(name: "Outer Loops", code: "OL", info: "Brep outer loop curves."), query: Query.Curves<object, Curve>(aspect: Curves.OuterLoop)),
        ShapeOutput(port: Port.List<Curve>(name: "Inner Loops", code: "IL", info: "Brep inner loop curves."), query: Query.Curves<object, Curve>(aspect: Curves.InnerLoop)),
        ShapeOutput(port: Port.List<Curve>(name: "U Iso Curves", code: "U", info: "Trim-aware mid-domain U-direction iso curves for Brep faces and surface values."), query: Query.Curves<object, Curve>(aspect: Curves.IsoU)),
        ShapeOutput(port: Port.List<Curve>(name: "V Iso Curves", code: "V", info: "Trim-aware mid-domain V-direction iso curves for Brep faces and surface values."), query: Query.Curves<object, Curve>(aspect: Curves.IsoV)),
        ControlledShapeOutput(port: Port.List<Curve>(name: "Silhouette Curves", code: "SC", info: "Parallel-projection silhouette curves using Direction, defaulting to world Z."), control: Direction, build: static direction => Query.Curves<object, Curve>(aspect: Curves.Silhouette(direction: direction.Map(static value => (Vector3d?)value).IfNone(static () => null)))),
        ControlledShapeOutput(port: Port.List<Curve>(name: "Draft Curves", code: "DC", info: "Draft transition curves using Direction as pull direction and Draft Angle, defaulting to 0 radians."), controlA: Direction, controlB: DraftAngle, build: static (direction, angle) => Query.Curves<object, Curve>(aspect: Curves.Draft(direction: direction.Map(static value => (Vector3d?)value).IfNone(static () => null), angle: angle.Map(static value => (double?)value.Radians).IfNone(static () => null)))),
        ShapeOutput(port: Port.List<ComponentIndex>(param: Param.Generic, name: "Curve Sources", code: "CS", info: "Component index aligned with All Curves."), query: Query.Curves<object, ComponentIndex>(aspect: Curves.All)),
        ShapeOutput(port: Port.List<CurveFeature>(param: Param.CurveFeature, name: "Curve Features", code: "CF", info: "Feature label aligned with All Curves."), query: Query.Curves<object, CurveFeature>(aspect: Curves.All)),
        IndexedShapeOutput(port: Port.List<Curve>(name: "Indexed Curve", code: "IC", info: "Curve at Index input; missing Index defaults to 0, supplied values clamp to [0, count-1]. Empty when zero curves."), index: Index, build: static index => Query.Curves<object, Curve>(aspect: Curves.At(index: index))));

    protected override Seq<IPort> Controls => ControlPorts;

    protected override Seq<IOutput<Shape>> Slots => OutputSlots;

    public ExtractCurves() : base(nomen: NomenOf<ExtractCurves>()) { }
    public ExtractCurves(IReader reader) : base(reader: reader) { }
}
