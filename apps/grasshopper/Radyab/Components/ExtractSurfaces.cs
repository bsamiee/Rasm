using Analysis;
using Core.Domain;
using Grasshopper;
using Grasshopper2.UI;
using GrasshopperIO;
using LanguageExt;
using Rhino.Geometry;
using static LanguageExt.Prelude;
using Query = Analysis.Query;

namespace Radyab.Components;

// --- [EXPORTS] ----------------------------------------------------------------------------------

[IoId("F51A09A8-A5A5-467A-ADBA-C950511A0020")]
[Nomen(
    name: "Extract Surfaces",
    info: "All faces, top/bottom by world-Z centroid (ties returned), an index-clamped face, and the UV frame on the indexed face. Accepts Brep, BrepFace, Surface, SubD; rejects Mesh.",
    category: "Radyab",
    section: "Extraction")]
public sealed class ExtractSurfaces : Component<Shape> {
    protected override Seq<IPort> Auxiliaries { get; } = Seq<IPort>(Port.Index());

    protected override Seq<IOutput<Shape>> Slots { get; } = Seq<IOutput<Shape>>(
        Output.Of<Shape, Brep>(name: "All Surfaces", code: "AS", info: "Every face as single-face Brep; preserves trims; SubD via SubDToBrepOptions.Default.", query: Query.Faces<object, Brep>(aspect: Faces.All)),
        Output.Of<Shape, Brep>(name: "Top Surface", code: "TS", info: "Face(s) with maximum world-Z centroid; ties within tolerance.", query: Query.Faces<object, Brep>(aspect: Faces.Top)),
        Output.Of<Shape, Brep>(name: "Bottom Surface", code: "BS", info: "Face(s) with minimum world-Z centroid; ties within tolerance.", query: Query.Faces<object, Brep>(aspect: Faces.Bottom)),
        Output.Indexed<Shape, Brep>(name: "Indexed Surface", code: "IS", info: "Face at Index input; clamped to [0, count-1]. Empty when zero faces.", build: static (Option<int> hint) => Query.Faces<object, Brep>(aspect: Faces.At(index: hint.Match(Some: static (int v) => (int?)v, None: static () => (int?)null)))),
        Output.Indexed<Shape, Plane>(name: "UV Frame", code: "UV", info: "Orthonormal frame at indexed face centroid; X=du, Z=outward, Y completes RH basis.", build: static (Option<int> hint) => Query.Faces<object, Plane>(aspect: Faces.At(index: hint.Match(Some: static (int v) => (int?)v, None: static () => (int?)null)))));

    public ExtractSurfaces() : base(nomen: NomenOf<ExtractSurfaces>()) { }
    public ExtractSurfaces(IReader reader) : base(reader: reader) { }
}
