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
    info: "Trimmed faces, world-Z top/bottom faces, indexed face details, and centroid UV frame for Brep, BrepFace, Surface, and SubD values.",
    category: "Radyab",
    section: "Extraction")]
public sealed class ExtractSurfaces : ShapeComponent {
    private static readonly Port<int> Index = Port.Index(info: "Zero-based face selector; missing Index defaults to face 0 and supplied values clamp to [0, count-1].");
    private static readonly Seq<IPort> ControlPorts = Seq<IPort>(Index);
    private static readonly Seq<IOutput<Shape>> OutputSlots = Seq<IOutput<Shape>>(
        ShapeOutput(port: Port.List<Brep>(name: "All Surfaces", code: "AS", info: "Every face as a trimmed single-face Brep. Mesh input is intentionally rejected."), query: Query.Faces<object, Brep>(aspect: Faces.All)),
        ShapeOutput(port: Port.List<int>(name: "Surface Indices", code: "SI", info: "Source Brep face index aligned with every extracted surface."), query: Query.Faces<object, int>(aspect: Faces.All)),
        ShapeOutput(port: Port.List<Brep>(name: "Top Surface", code: "TS", info: "Trimmed face(s) with maximum world-Z centroid; ties within tolerance."), query: Query.Faces<object, Brep>(aspect: Faces.Top)),
        ShapeOutput(port: Port.List<Brep>(name: "Bottom Surface", code: "BS", info: "Trimmed face(s) with minimum world-Z centroid; ties within tolerance."), query: Query.Faces<object, Brep>(aspect: Faces.Bottom)),
        IndexedShapeOutput(port: Port.List<Brep>(name: "Indexed Surface", code: "IS", info: "Trimmed single-face Brep at Index input; missing Index defaults to 0, supplied values clamp to [0, count-1]. Empty when zero faces."), index: Index, build: static index => Query.Faces<object, Brep>(aspect: Faces.At(index: index))),
        IndexedShapeOutput(port: Port.List<Plane>(name: "UV Frame", code: "UV", info: "Native U/V frame at the indexed face centroid. X=surface U direction, Z=orientation-corrected normal, Y completes the basis."), index: Index, build: static index => Query.Faces<object, Plane>(aspect: Faces.At(index: index))),
        IndexedShapeOutput(port: Port.List<Point3d>(name: "Face Center", code: "FC", info: "Area centroid of the indexed trimmed face."), index: Index, build: static index => Query.Faces<object, Point3d>(aspect: Faces.At(index: index))),
        IndexedShapeOutput(port: Port.List<Vector3d>(name: "Face Normal", code: "FN", info: "Orientation-corrected indexed face normal at the face centroid."), index: Index, build: static index => Query.Faces<object, Vector3d>(aspect: Faces.At(index: index))),
        IndexedShapeOutput(port: Port.List<int>(name: "Face Index", code: "FI", info: "Source Brep face index selected by the clamped Index input."), index: Index, build: static index => Query.Faces<object, int>(aspect: Faces.At(index: index))),
        IndexedShapeOutput(port: Port.List<ComponentIndex>(param: Param.Generic, name: "Face Component", code: "CI", info: "Source Brep face component index selected by the clamped Index input."), index: Index, build: static index => Query.Faces<object, ComponentIndex>(aspect: Faces.At(index: index))),
        IndexedShapeOutput(port: Port.List<Interval>(name: "UV Domains", code: "UD", info: "Indexed face domains as two intervals: U first, then V."), index: Index, build: static index => Query.Faces<object, Interval>(aspect: Faces.At(index: index))));

    protected override Seq<IPort> Controls => ControlPorts;

    protected override Seq<IOutput<Shape>> Slots => OutputSlots;

    public ExtractSurfaces() : base(nomen: NomenOf<ExtractSurfaces>()) { }
    public ExtractSurfaces(IReader reader) : base(reader: reader) { }
}
