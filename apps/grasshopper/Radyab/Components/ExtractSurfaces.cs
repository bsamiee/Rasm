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

[IoId("F51A09A8-A5A5-467A-ADBA-C950511A0020")]
[Nomen(
    name: "Extract Surfaces",
    info: "All faces, top/bottom by world-Z centroid (ties returned), an index-clamped face, and the UV frame on the indexed face. Accepts Brep, BrepFace, Surface, SubD; rejects Mesh.",
    category: "Radyab",
    section: "Extraction")]
public sealed class ExtractSurfaces : Component<Shape> {
    private static readonly Port<Shape> Geometry = Port.Required<Shape>(
        param: Param.Generic,
        name: "Geometry",
        code: "G",
        info: "Brep, BrepFace, Surface, or SubD to extract as surface faces; Mesh is intentionally rejected.");
    private static readonly Port<int> Index = Port.Index(info: "Zero-based face selector; missing Index defaults to face 0 and supplied values clamp to [0, count-1].");

    protected override Seq<IPort> Inputs { get; } = Seq<IPort>(Geometry, Index);

    protected override Seq<IOutput<Shape>> Slots { get; } = Seq<IOutput<Shape>>(
        Output.Of<Shape, object, Brep>(port: Port.List<Brep>(param: Param.Brep, name: "All Surfaces", code: "AS", info: "Every face as single-face Brep; preserves trims; SubD via SubDToBrepOptions.Default; Mesh is not a GH2 surface-broker surface."), select: static shape => shape.Inner, query: Query.Faces<object, Brep>(aspect: Faces.All)),
        Output.Of<Shape, object, Brep>(port: Port.List<Brep>(param: Param.Brep, name: "Top Surface", code: "TS", info: "Face(s) with maximum world-Z centroid; ties within tolerance."), select: static shape => shape.Inner, query: Query.Faces<object, Brep>(aspect: Faces.Top)),
        Output.Of<Shape, object, Brep>(port: Port.List<Brep>(param: Param.Brep, name: "Bottom Surface", code: "BS", info: "Face(s) with minimum world-Z centroid; ties within tolerance."), select: static shape => shape.Inner, query: Query.Faces<object, Brep>(aspect: Faces.Bottom)),
        Output.Indexed<Shape, object, Brep>(port: Port.List<Brep>(param: Param.Brep, name: "Indexed Surface", code: "IS", info: "Face at Index input; missing Index defaults to 0, supplied values clamp to [0, count-1]. Empty when zero faces."), select: static shape => shape.Inner, build: static hint => Query.Faces<object, Brep>(aspect: Faces.At(index: hint.Match(Some: static v => (int?)v, None: static () => (int?)null)))),
        Output.Indexed<Shape, object, Plane>(port: Port.List<Plane>(param: Param.Plane, name: "UV Frame", code: "UV", info: "Native face frame at indexed face centroid; missing Index defaults to 0. X=surface U direction, Z=outward, Y completes the basis."), select: static shape => shape.Inner, build: static hint => Query.Faces<object, Plane>(aspect: Faces.At(index: hint.Match(Some: static v => (int?)v, None: static () => (int?)null)))));

    public ExtractSurfaces() : base(nomen: NomenOf<ExtractSurfaces>()) { }
    public ExtractSurfaces(IReader reader) : base(reader: reader) { }

    protected override Fin<Shape> Read(IDataAccess access) =>
        access.ReadShape(slot: 0, port: Geometry);
    protected override Option<int> Hint(IDataAccess access) =>
        access.Index(slot: 1);
}
