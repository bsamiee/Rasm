namespace Rasm.Rhino;

// --- [MODELS] ---------------------------------------------------------------------------
public sealed record CommandSelection {
    private CommandSelection(RhinoDoc document, Seq<Reference> items) {
        ArgumentNullException.ThrowIfNull(argument: document);
        Document = document;
        Items = items;
    }

    public RhinoDoc Document { get; }
    public Seq<Reference> Items { get; }
    public Seq<Guid> ObjectIds => Items.Map(static item => item.ObjectId);

    internal static CommandSelection From(RhinoDoc document, Seq<ObjRef> references, Seq<Guid> preselected) {
        ArgumentNullException.ThrowIfNull(argument: document);
        ObjRef[] source = [.. references];
        Reference[] snapshots = [.. toSeq(source).Map(reference => Reference.Of(
            reference: reference,
            preselected: preselected.Exists(id => id == reference.ObjectId)))];
        _ = toSeq(source).Iter(static reference => reference.Dispose());
        return new(document: document, items: toSeq(snapshots));
    }

    public readonly record struct Reference(
        Guid ObjectId,
        uint RuntimeSerialNumber,
        ComponentIndex ComponentIndex,
        bool Preselected,
        SelectionMethod SelectionMethod,
        Option<Point3d> SelectionPoint,
        Option<uint> SelectionViewRuntimeSerialNumber,
        Option<Guid> SelectionViewportId,
        uint SelectionViewDetailSerialNumber,
        Option<double> CurveParameter,
        Option<Point2d> SurfaceParameter,
        Option<Point2d> BrepParameter) {
        internal static Reference Of(ObjRef reference, bool preselected) {
            ArgumentNullException.ThrowIfNull(argument: reference);
            SelectionMethod selectionMethod = reference.SelectionMethod();
            Point3d selectionPoint = reference.SelectionPoint();
            RhinoView? selectionView = reference.SelectionView();
            double curveParameter = 0.0;
            double surfaceU = 0.0;
            double surfaceV = 0.0;
            Curve? curve = selectionMethod switch {
                SelectionMethod.MousePick => reference.CurveParameter(parameter: out curveParameter),
                _ => null,
            };
            Surface? surface = selectionMethod switch {
                SelectionMethod.MousePick => reference.SurfaceParameter(u: out surfaceU, v: out surfaceV),
                _ => null,
            };
            Point2d surfaceParameter = new(x: surfaceU, y: surfaceV);
            return new(
                ObjectId: reference.ObjectId,
                RuntimeSerialNumber: reference.RuntimeSerialNumber,
                ComponentIndex: reference.GeometryComponentIndex,
                Preselected: preselected,
                SelectionMethod: selectionMethod,
                SelectionPoint: selectionPoint switch {
                    Point3d point when point.IsValid => Some(point),
                    _ => Option<Point3d>.None,
                },
                SelectionViewRuntimeSerialNumber: Optional(selectionView).Map(static view => view.RuntimeSerialNumber),
                SelectionViewportId: Optional(selectionView).Map(static view => view.ActiveViewportID),
                SelectionViewDetailSerialNumber: reference.SelectionViewDetailSerialNumber(),
                CurveParameter: curve switch {
                    Curve when RhinoMath.IsValidDouble(x: curveParameter) => Some(curveParameter),
                    _ => Option<double>.None,
                },
                SurfaceParameter: surface switch {
                    Surface when surfaceParameter.IsValid => Some(surfaceParameter),
                    _ => Option<Point2d>.None,
                },
                BrepParameter: surface switch {
                    BrepFace when surfaceParameter.IsValid => Some(surfaceParameter),
                    _ => Option<Point2d>.None,
                });
        }

        public Fin<TResult> UseObjRef<TResult>(RhinoDoc document, Func<ObjRef, Fin<TResult>> project) {
            ArgumentNullException.ThrowIfNull(argument: document);
            ArgumentNullException.ThrowIfNull(argument: project);
            RhinoObject? found = document.Objects.Find(runtimeSerialNumber: RuntimeSerialNumber);
            using ObjRef reference = (found, ComponentIndex.IsSet) switch {
                (RhinoObject { IsDeleted: false } native, true) => new ObjRef(doc: document, id: native.Id, ci: ComponentIndex),
                (RhinoObject { IsDeleted: false } native, false) => new ObjRef(rhinoObject: native),
                (_, true) => new ObjRef(doc: document, id: ObjectId, ci: ComponentIndex),
                _ => new ObjRef(doc: document, id: ObjectId),
            };
            return project(arg: reference);
        }
    }
}
