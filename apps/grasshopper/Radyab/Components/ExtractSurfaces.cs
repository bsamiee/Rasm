namespace Radyab.Components;

[IoId("7EBA9774-99EA-4A47-95EA-8CAEA6F09323")]
[Nomen(
    name: "Extract Surfaces",
    info: "Trimmed faces, direction-ranked top and bottom faces, plus indexed face frame (centroid, normal, UV plane) for Brep, BrepFace, and Surface values.",
    category: Library.Name,
    section: Library.Extraction)]
public sealed class ExtractSurfaces : Component<ExtractSurfaces> {
    private static readonly Port<Shape> Geometry = Port.Shape();
    private static readonly Port<int> Index = Port.Index(info: "Zero-based face selector for the Frame output; missing Index defaults to face 0 and GH index modifiers apply per source face count.");
    private static readonly Port<Vector3d> Direction = Port.Direction(info: "Ranking direction for Top and Bottom surfaces; missing Direction uses world Z.");
    private static readonly OutputGroup Faces = Output.Details<Rasm.Analysis.Faces, TopologyProjection>(
        input: Geometry,
        aspect: Rasm.Analysis.Faces.All,
        emptyUnsupported: true,
        slots: [
            Output.Choose<TopologyProjection, Brep>(port: Port.Tree<Brep>(name: "Faces", code: "F", info: "Every face as a trimmed single-face Brep."), project: static value => value.As<Brep>()),
            Output.Plain<TopologyProjection, int>(port: Port.Tree<int>(name: "Face Index", code: "FX", info: "Source Brep face index aligned with each face."), project: static value => value.FaceIndex),
        ]);
    private static readonly OutputGroup Top = Output.Single<Rasm.Analysis.Faces, Brep>(
        input: Geometry, port: Port.Tree<Brep>(name: "Top", code: "T", info: "Trimmed face(s) with maximum centroid projection along Direction; ties resolve within tolerance."),
        aspect: runtime => runtime.Read(port: Direction).Map(axis => Rasm.Analysis.Faces.Top(axis: axis.ToNullable())));
    private static readonly OutputGroup Bottom = Output.Single<Rasm.Analysis.Faces, Brep>(
        input: Geometry, port: Port.Tree<Brep>(name: "Bottom", code: "B", info: "Trimmed face(s) with minimum centroid projection along Direction; ties resolve within tolerance."),
        aspect: runtime => runtime.Read(port: Direction).Map(axis => Rasm.Analysis.Faces.Bottom(axis: axis.ToNullable())));
    private static readonly OutputGroup Frame = Output.Details<Rasm.Analysis.Faces, TopologyProjection>(
        input: Geometry,
        aspect: runtime => Fin.Succ(Rasm.Analysis.Faces.At(index: runtime.Index(port: Index, limit: int.MaxValue).ToNullable())),
        emptyUnsupported: false,
        slots: [
            Output.Choose<TopologyProjection, Brep>(port: Port.Tree<Brep>(name: "Face", code: "FA", info: "Trimmed single-face Brep at Index; missing Index defaults to 0 and GH index modifiers apply per source face count."), project: static value => value.As<Brep>()),
            Output.One<TopologyProjection, Plane>(port: Port.Tree<Plane>(name: "UV Frame", code: "UV", info: "Native U/V frame at the indexed face centroid: X = surface U direction, Z = orientation-corrected normal, Y completes the basis."), project: static (face, context) => face.FrameAtCentroid(context: context)),
            Output.One<TopologyProjection, Point3d>(port: Port.Tree<Point3d>(name: "Centroid", code: "FC", info: "Area centroid of the indexed trimmed face."), project: static (face, context) => face.Centroid(context: context)),
            Output.One<TopologyProjection, Vector3d>(port: Port.Tree<Vector3d>(name: "Normal", code: "FN", info: "Orientation-corrected indexed face normal at the face centroid."), project: static (face, context) => face.FrameAtCentroid(context: context).Map(static frame => frame.ZAxis)),
        ]);
    public ExtractSurfaces() : base(spec: ComponentSpec.Of(
        inputs: Seq<Port>(Geometry, Index, Direction),
        outputs: Seq<OutputGroup>(Faces, Top, Bottom, Frame))) { }
}
