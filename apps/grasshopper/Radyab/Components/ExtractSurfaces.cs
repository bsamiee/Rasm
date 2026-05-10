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
public sealed class ExtractSurfaces : ShapeComponent<ExtractSurfaces.Spec> {
    public sealed class Spec : IComponentSpec<Shape, ShapeState> {
        private static readonly Port<int> Index = Port.Index(info: "Zero-based face selector; missing Index defaults to face 0 and supplied values clamp to [0, count-1].");
        private static Seq<IPort> Controls { get; } = Seq<IPort>(Index);

        public static Seq<IPort> Inputs =>
            ShapeFoundation.Inputs(controls: Controls);
        public static Seq<IOutputGroup<ShapeState>> Outputs { get; } = Seq<IOutputGroup<ShapeState>>(
            ShapeFoundation.FaceDetails(
                selector: Rasm.Analysis.Faces.All,
                breps: Port.List<Brep>(name: "All Surfaces", code: "AS", info: "Every face as a trimmed single-face Brep. Mesh input is intentionally rejected."),
                indices: Port.List<int>(name: "Surface Indices", code: "SI", info: "Source Brep face index aligned with every extracted surface.")),
            ShapeFoundation.Query(port: Port.List<Brep>(name: "Top Surface", code: "TS", info: "Trimmed face(s) with maximum world-Z centroid; ties within tolerance."), query: static () => Query.Faces<object, Brep>(aspect: Rasm.Analysis.Faces.Top)),
            ShapeFoundation.Query(port: Port.List<Brep>(name: "Bottom Surface", code: "BS", info: "Trimmed face(s) with minimum world-Z centroid; ties within tolerance."), query: static () => Query.Faces<object, Brep>(aspect: Rasm.Analysis.Faces.Bottom)),
            ShapeFoundation.IndexedFaceDetails(
                index: Index,
                brep: Port.List<Brep>(name: "Indexed Surface", code: "IS", info: "Trimmed single-face Brep at Index input; missing Index defaults to 0, supplied values clamp to [0, count-1]. Empty when zero faces."),
                frame: Port.List<Plane>(name: "UV Frame", code: "UV", info: "Native U/V frame at the indexed face centroid. X=surface U direction, Z=orientation-corrected normal, Y completes the basis."),
                center: Port.List<Point3d>(name: "Face Center", code: "FC", info: "Area centroid of the indexed trimmed face."),
                normal: Port.List<Vector3d>(name: "Face Normal", code: "FN", info: "Orientation-corrected indexed face normal at the face centroid."),
                face: Port.List<int>(name: "Face Index", code: "FI", info: "Source Brep face index selected by the clamped Index input."),
                component: Port.List<ComponentIndex>(param: Param.Generic, name: "Face Component", code: "CI", info: "Source Brep face component index selected by the clamped Index input."),
                domains: Port.List<Interval>(name: "UV Domains", code: "UD", info: "Indexed face domains as two intervals: U first, then V.")));

        public static Fin<Shape> Read(IDataAccess access) =>
            ShapeFoundation.Read(access: access);
        public static Fin<ShapeState> Prepare(IDataAccess access, Analyze.Scope scope, Hints hints, Shape input) =>
            ShapeFoundation.Prepare(access: access, scope: scope, hints: hints, input: input);

    }

    public ExtractSurfaces() : base(nomen: ComponentNomen.Of<ExtractSurfaces>()) { }
    public ExtractSurfaces(IReader reader) : base(reader: reader) { }
}
