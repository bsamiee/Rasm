namespace Rasm.Rhino.Commands;

// --- [MODELS] ---------------------------------------------------------------------------
public readonly record struct PickLocation(Option<double> CurveParameter, Option<Point2d> SurfaceParameter) {
    internal static Option<PickLocation> Of(ObjRef reference, SelectionMethod selectionMethod) {
        ArgumentNullException.ThrowIfNull(argument: reference);
        Option<double> curve = selectionMethod switch {
            SelectionMethod.MousePick => reference.CurveParameter(parameter: out double curveParameter) switch {
                Curve when RhinoMath.IsValidDouble(x: curveParameter) => Some(curveParameter),
                _ => Option<double>.None,
            },
            _ => Option<double>.None,
        };
        Option<Point2d> surface = selectionMethod switch {
            SelectionMethod.MousePick => reference.SurfaceParameter(u: out double surfaceU, v: out double surfaceV) switch {
                Surface => new Point2d(x: surfaceU, y: surfaceV),
                _ => Point2d.Unset,
            } switch {
                Point2d point when point.IsValid => Some(point),
                _ => Option<Point2d>.None,
            },
            _ => Option<Point2d>.None,
        };
        return (curve.IsSome || surface.IsSome) switch {
            true => Some(new PickLocation(CurveParameter: curve, SurfaceParameter: surface)),
            false => Option<PickLocation>.None,
        };
    }

    internal static Option<PickLocation> Of(GetPoint getter) {
        ArgumentNullException.ThrowIfNull(argument: getter);
        Option<double> curve = getter.PointOnCurve(t: out double curveParameter) switch {
            Curve when RhinoMath.IsValidDouble(x: curveParameter) => Some(curveParameter),
            _ => Option<double>.None,
        };
        Option<Point2d> surface = getter.PointOnBrep(u: out double brepU, v: out double brepV) switch {
            BrepFace => new Point2d(x: brepU, y: brepV),
            _ => getter.PointOnSurface(u: out double surfaceU, v: out double surfaceV) switch {
                Surface => new Point2d(x: surfaceU, y: surfaceV),
                _ => Point2d.Unset,
            },
        } switch {
            Point2d point when point.IsValid => Some(point),
            _ => Option<Point2d>.None,
        };
        return (curve.IsSome || surface.IsSome) switch {
            true => Some(new PickLocation(CurveParameter: curve, SurfaceParameter: surface)),
            false => Option<PickLocation>.None,
        };
    }
}

public sealed record CommandSelection {
    private CommandSelection(RhinoDoc document, Seq<Reference> items) {
        ArgumentNullException.ThrowIfNull(argument: document);
        Document = document;
        Items = items;
    }

    public RhinoDoc Document { get; }
    public Seq<Reference> Items { get; }
    public Seq<Guid> ObjectIds => Items.Map(static item => item.ObjectId);

    internal static CommandSelection From(RhinoDoc document, Seq<ObjRef> references, Seq<(Guid ObjectId, ComponentIndex ComponentIndex)> preselected) {
        ArgumentNullException.ThrowIfNull(argument: document);
        ObjRef[] source = [.. references];
        Reference[] snapshots = [.. toSeq(source).Map(reference => Reference.Of(
            reference: reference,
            preselected: preselected.Exists(item => item.ObjectId == reference.ObjectId && item.ComponentIndex == reference.GeometryComponentIndex)))];
        _ = toSeq(source).Iter(static reference => reference.Dispose());
        return new(document: document, items: toSeq(snapshots));
    }

    internal Fin<int> SelectInto(RhinoDoc document, bool selected, Op op) {
        ArgumentNullException.ThrowIfNull(argument: document);
        ObjRef[] references = [.. Items.Map(reference => reference.ObjRef(document: document))];
        Fin<int> result = document.Objects.Select(references, selected) switch {
            int count and >= 0 => Fin.Succ(value: count),
            _ => Fin.Fail<int>(error: op.InvalidResult()),
        };
        _ = toSeq(references).Iter(static reference => reference.Dispose());
        return result;
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
        Option<PickLocation> Location) {
        internal static Reference Of(ObjRef reference, bool preselected) {
            ArgumentNullException.ThrowIfNull(argument: reference);
            SelectionMethod selectionMethod = reference.SelectionMethod();
            Point3d selectionPoint = reference.SelectionPoint();
            RhinoView? selectionView = reference.SelectionView();
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
                Location: PickLocation.Of(reference: reference, selectionMethod: selectionMethod));
        }

        internal ObjRef ObjRef(RhinoDoc document) {
            ArgumentNullException.ThrowIfNull(argument: document);
            RhinoObject? found = document.Objects.Find(runtimeSerialNumber: RuntimeSerialNumber);
            return (found, ComponentIndex.IsSet) switch {
                (RhinoObject { IsDeleted: false } native, true) => new ObjRef(doc: document, id: native.Id, ci: ComponentIndex),
                (RhinoObject { IsDeleted: false } native, false) => new ObjRef(rhinoObject: native),
                (_, true) => new ObjRef(doc: document, id: ObjectId, ci: ComponentIndex),
                _ => new ObjRef(doc: document, id: ObjectId),
            };
        }
    }
}
