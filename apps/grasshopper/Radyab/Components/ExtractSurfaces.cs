namespace Radyab.Components;

[IoId("F51A09A8-A5A5-467A-ADBA-C950511A0020")]
[Nomen(
    name: "Extract Surfaces",
    info: "Trimmed faces, direction-ranked top/bottom faces, indexed face details, and centroid UV frame for Brep, BrepFace, Surface, and SubD values.",
    category: "Radyab",
    section: "Extraction")]
public sealed class ExtractSurfaces : Component<ExtractSurfaces> {
    [Input] private readonly Port<Shape> Geometry = Port.Shape();
    [Input] private readonly Port<int> Index = Port.Index(info: "Zero-based face selector; missing Index defaults to face 0 and supplied values clamp to [0, count-1].");
    [Input] private readonly Port<Vector3d> Direction = Port.Direction(info: "Ranking direction for Top/Bottom surfaces; missing Direction uses world Z.");
    [Output]
    private IOutputGroup AllSurfaces => Output.Details<FaceProjection>(
        input: Geometry,
        source: static (_, _) => shape => Rasm.Analysis.Query.FaceProjections(geometry: shape.Inner, selector: Faces.All),
        slots: [
            Output.Plain<FaceProjection, Brep>(port: Port.List<Brep>(name: "All Surfaces", code: "AS", info: "Every face as a trimmed single-face Brep. Mesh input is intentionally rejected."), project: static value => value.Brep),
            Output.Plain<FaceProjection, int>(port: Port.List<int>(name: "Index", code: "IX", info: "Source Brep face index aligned with every extracted surface."), project: static value => value.FaceIndex),
        ]);
    [Output] private IOutputGroup TopSurface => Output.Query(input: Geometry, port: Port.List<Brep>(name: "Top Surface", code: "TS", info: "Trimmed face(s) with maximum centroid projection along Direction; ties within tolerance."), operation: (access, runtime) => Rasm.Analysis.Query.Faces<object, Brep>(aspect: Faces.Top(axis: runtime.Value(access: access, port: Direction).IfNone(Vector3d.ZAxis))));
    [Output] private IOutputGroup BottomSurface => Output.Query(input: Geometry, port: Port.List<Brep>(name: "Bottom Surface", code: "BS", info: "Trimmed face(s) with minimum centroid projection along Direction; ties within tolerance."), operation: (access, runtime) => Rasm.Analysis.Query.Faces<object, Brep>(aspect: Faces.Bottom(axis: runtime.Value(access: access, port: Direction).IfNone(Vector3d.ZAxis))));
    [Output]
    private IOutputGroup IndexedFace => Output.Details<FaceProjection>(
        input: Geometry,
        source: (access, runtime) => shape => Rasm.Analysis.Query.FaceProjections(geometry: shape.Inner, selector: Faces.At(index: runtime.NullableIndex(access: access, port: Index))),
        slots: [
            Output.Plain<FaceProjection, Brep>(port: Port.List<Brep>(name: "Indexed Surface", code: "IS", info: "Trimmed single-face Brep at Index input; missing Index defaults to 0, supplied values clamp to [0, count-1]. Empty when zero faces."), project: static value => value.Brep),
            Output.One<FaceProjection, Plane>(port: Port.List<Plane>(name: "UV Frame", code: "UV", info: "Native U/V frame at the indexed face centroid. X=surface U direction, Z=orientation-corrected normal, Y completes the basis."), project: static (face, context) => Rasm.Analysis.Query.FrameAtCentroid(face: face, runtime: context)),
            Output.One<FaceProjection, Point3d>(port: Port.List<Point3d>(name: "Face Center", code: "FC", info: "Area centroid of the indexed trimmed face."), project: static (face, context) => Rasm.Analysis.Query.FaceCentroid(face: face, runtime: context)),
            Output.One<FaceProjection, Vector3d>(port: Port.List<Vector3d>(name: "Face Normal", code: "FN", info: "Orientation-corrected indexed face normal at the face centroid."), project: static (face, context) => Rasm.Analysis.Query.FrameAtCentroid(face: face, runtime: context).Map(static frame => frame.ZAxis)),
            Output.Plain<FaceProjection, int>(port: Port.List<int>(name: "Index", code: "IX", info: "Source Brep face index selected by the clamped Index input."), project: static value => value.FaceIndex),
            Output.Plain<FaceProjection, ComponentIndex>(port: Port.List<ComponentIndex>(name: "Source", code: "S", info: "Source Brep face component index selected by the clamped Index input."), project: static value => value.Source),
            Output.Many<FaceProjection, Interval>(port: Port.List<Interval>(name: "UV Domains", code: "UD", info: "Indexed face domains as two intervals: U first, then V."), project: static face => Rasm.Analysis.Query.FaceDomains(face: face)),
        ]);
}
