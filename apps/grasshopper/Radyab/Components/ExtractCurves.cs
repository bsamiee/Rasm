using Grasshopper2.Components;
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
public sealed class ExtractCurves : Component<Shape> {
    private static readonly Port<Shape> Geometry = Port.Required<Shape>(
        param: Param.Generic,
        name: "Geometry",
        code: "G",
        info: "Curve, primitive curve, Brep, BrepFace, Surface, SubD, or Mesh to extract as Rhino curves.");
    private static readonly Port<int> Index = Port.Index(info: "Zero-based curve selector; missing Index defaults to curve 0 and supplied values clamp to [0, count-1].");

    protected override Seq<IPort> Inputs { get; } = Seq<IPort>(Geometry, Index);

    protected override Seq<IOutput<Shape>> Slots { get; } = Seq<IOutput<Shape>>(
        Output.Of<Shape, object, Curve>(port: Port.List<Curve>(param: Param.Curve, name: "All Curves", code: "AC", info: "All native curve pieces: subcurves, Brep/SubD edges, surface boundary iso curves, or mesh topology edge curves."), select: static shape => shape.Inner, query: Query.Curves<object, Curve>(aspect: Curves.All)),
        Output.Of<Shape, object, Curve>(port: Port.List<Curve>(param: Param.Curve, name: "Boundary Curves", code: "BC", info: "Boundary curves: naked Brep/mesh edges, surface/face boundary iso curves, or the input curve itself."), select: static shape => shape.Inner, query: Query.Curves<object, Curve>(aspect: Curves.Boundary)),
        Output.Of<Shape, object, Curve>(port: Port.List<Curve>(param: Param.Curve, name: "U Iso Curves", code: "U", info: "Trim-aware mid-domain U-direction iso curves for Brep faces and surface values."), select: static shape => shape.Inner, query: Query.Curves<object, Curve>(aspect: Curves.IsoU)),
        Output.Of<Shape, object, Curve>(port: Port.List<Curve>(param: Param.Curve, name: "V Iso Curves", code: "V", info: "Trim-aware mid-domain V-direction iso curves for Brep faces and surface values."), select: static shape => shape.Inner, query: Query.Curves<object, Curve>(aspect: Curves.IsoV)),
        Output.Indexed<Shape, object, Curve>(port: Port.List<Curve>(param: Param.Curve, name: "Indexed Curve", code: "IC", info: "Curve at Index input; missing Index defaults to 0, supplied values clamp to [0, count-1]. Empty when zero curves."), select: static shape => shape.Inner, build: static hint => Query.Curves<object, Curve>(aspect: Curves.At(index: hint.Match(Some: static v => (int?)v, None: static () => (int?)null)))));

    public ExtractCurves() : base(nomen: NomenOf<ExtractCurves>()) { }
    public ExtractCurves(IReader reader) : base(reader: reader) { }

    protected override Fin<Shape> Read(IDataAccess access) =>
        access.ReadShape(slot: 0, port: Geometry);
    protected override Option<int> Hint(IDataAccess access) =>
        access.Index(slot: 1);
}
