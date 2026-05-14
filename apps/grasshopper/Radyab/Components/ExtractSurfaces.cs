namespace Radyab.Components;

[IoId("7EBA9774-99EA-4A47-95EA-8CAEA6F09323")]
[Nomen(
    name: "Extract Surfaces",
    info: "Trimmed faces, direction-ranked top/bottom faces, indexed face details, and centroid UV frame for Brep, BrepFace, Surface, and SubD values.",
    category: Library.Name,
    section: Library.Extraction)]
public sealed class ExtractSurfaces : Component {
    private static readonly Port<Shape> Geometry = Port.Shape();
    private static readonly Port<int> Index = Port.Index(info: "Zero-based face selector; missing Index defaults to face 0 and supplied values clamp to [0, count-1].");
    private static readonly Port<Vector3d> Direction = Port.Direction(info: "Ranking direction for Top/Bottom surfaces; missing Direction uses world Z.");
    private static readonly IOutputGroup AllSurfaces = Output.Details<FaceProjection>(
        input: Geometry,
        aspect: static _ => shape => Rasm.Analysis.Query.FaceProjections(geometry: shape.Inner, selector: Faces.All),
        emptyUnsupported: true,
        aspectLabel: nameof(Faces),
        slots: [
            Output.Plain<FaceProjection, Brep>(port: Port.Tree<Brep>(name: "All Surfaces", code: "AS", info: "Every face as a trimmed single-face Brep. Mesh input is intentionally rejected."), project: static value => value.Brep),
            Output.Plain<FaceProjection, int>(port: Port.Tree<int>(name: "All Index", code: "AIX", info: "Source Brep face index aligned with every extracted surface."), project: static value => value.FaceIndex),
        ]);
    private static readonly IOutputGroup TopSurface = Output.Query(input: Geometry, port: Port.Tree<Brep>(name: "Top Surface", code: "TS", info: "Trimmed face(s) with maximum centroid projection along Direction; ties within tolerance."), aspect: runtime => Rasm.Analysis.Query.Faces<object, Brep>(aspect: Faces.Top(axis: runtime.ReadOrInvalid(port: Direction, invalid: Vector3d.Unset).ToNullable())));
    private static readonly IOutputGroup BottomSurface = Output.Query(input: Geometry, port: Port.Tree<Brep>(name: "Bottom Surface", code: "BS", info: "Trimmed face(s) with minimum centroid projection along Direction; ties within tolerance."), aspect: runtime => Rasm.Analysis.Query.Faces<object, Brep>(aspect: Faces.Bottom(axis: runtime.ReadOrInvalid(port: Direction, invalid: Vector3d.Unset).ToNullable())));
    private static readonly IOutputGroup IndexedFace = Output.Details<FaceProjection>(
        input: Geometry,
        aspect: runtime => shape => Rasm.Analysis.Query.FaceProjections(geometry: shape.Inner, choose: count => Faces.At(index: runtime.Index(port: Index, limit: count).ToNullable())),
        emptyUnsupported: false,
        aspectLabel: nameof(Faces),
        slots: [
            Output.Plain<FaceProjection, Brep>(port: Port.Tree<Brep>(name: "Indexed Surface", code: "IS", info: "Trimmed single-face Brep at Index input; missing Index defaults to 0, supplied values clamp to [0, count-1]. Empty when zero faces."), project: static value => value.Brep),
            Output.One<FaceProjection, Plane>(port: Port.Tree<Plane>(name: "UV Frame", code: "UV", info: "Native U/V frame at the indexed face centroid. X=surface U direction, Z=orientation-corrected normal, Y completes the basis."), project: static (face, context) => Rasm.Analysis.Query.FrameAtCentroid(face: face, runtime: context)),
            Output.One<FaceProjection, Point3d>(port: Port.Tree<Point3d>(name: "Face Center", code: "FC", info: "Area centroid of the indexed trimmed face."), project: static (face, context) => Rasm.Analysis.Query.FaceCentroid(face: face, runtime: context)),
            Output.One<FaceProjection, Vector3d>(port: Port.Tree<Vector3d>(name: "Face Normal", code: "FN", info: "Orientation-corrected indexed face normal at the face centroid."), project: static (face, context) => Rasm.Analysis.Query.FrameAtCentroid(face: face, runtime: context).Map(static frame => frame.ZAxis)),
            Output.Plain<FaceProjection, int>(port: Port.Tree<int>(name: "Indexed Index", code: "IIX", info: "Source Brep face index selected by the GH native Index input."), project: static value => value.FaceIndex),
            Output.Plain<FaceProjection, ComponentIndex>(port: Port.Tree<ComponentIndex>(name: "Source", code: "S", info: "Source Brep face component index selected by the GH native Index input."), project: static value => value.Source),
            Output.Many<FaceProjection, Interval>(port: Port.Tree<Interval>(name: "UV Domains", code: "UD", info: "Indexed face domains as two intervals: U first, then V."), project: static face => Rasm.Analysis.Query.FaceDomains(face: face)),
        ]);
    public ExtractSurfaces() : base(self: typeof(ExtractSurfaces), spec: ComponentSpec.Of(
        inputs: Seq<IPort>(Geometry, Index, Direction),
        outputs: Seq<IOutputGroup>(AllSurfaces, TopSurface, BottomSurface, IndexedFace))) { }
}
