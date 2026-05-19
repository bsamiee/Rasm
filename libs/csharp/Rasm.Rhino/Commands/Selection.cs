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
        return Of(curve: curve, surface: surface);
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
        return Of(curve: curve, surface: surface);
    }

    private static Option<PickLocation> Of(Option<double> curve, Option<Point2d> surface) =>
        (curve.IsSome || surface.IsSome) switch {
            true => Some(new PickLocation(CurveParameter: curve, SurfaceParameter: surface)),
            false => Option<PickLocation>.None,
        };
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
    internal Seq<Reference> TransformTargets {
        get {
            System.Collections.Generic.HashSet<(Guid, uint, ComponentIndex)> seen = [];
            return Items.Filter(item => seen.Add(item: (item.ObjectId, item.RuntimeSerialNumber, item.ComponentIndex)));
        }
    }

    public Fin<Seq<T>> Project<T>(Func<Reference, Fin<T>> project) => Optional(project).ToFin(Fail: Op.Of(name: nameof(Project)).InvalidInput()).Bind(valid => Items.TraverseM(reference => valid(arg: reference)).As());

    public Fin<Seq<TGeometry>> Geometry<TGeometry>() where TGeometry : GeometryBase =>
        Project(project: reference => reference.Geometry<TGeometry>(document: Document));

    public Fin<Reference> Single() =>
        Items.Count switch {
            1 => Fin.Succ(value: Items[0]),
            _ => Fin.Fail<Reference>(error: Op.Of(name: nameof(Single)).InvalidInput()),
        };

    public static Fin<CommandSelection> Pick(RhinoDoc document, global::Rhino.Input.Custom.PickContext context) =>
        from validDocument in Optional(document).ToFin(Fail: Op.Of(name: nameof(Pick)).InvalidInput()) from validContext in Optional(context).ToFin(Fail: Op.Of(name: nameof(Pick)).InvalidInput())
        from selection in Rasm.Rhino.UI.RhinoUi.Protect(valid: () => Optional(validDocument.Objects.PickObjects(pickContext: validContext)).ToFin(Fail: Op.Of(name: nameof(Pick)).InvalidResult()).Bind(references => references switch { { Length: > 0 } values => Fin.Succ(value: From(document: validDocument, references: toSeq(values), preselected: Seq<(Guid ObjectId, ComponentIndex ComponentIndex)>())), _ => Fin.Fail<CommandSelection>(error: Op.Of(name: nameof(Pick)).InvalidResult()) })) select selection;

    internal static CommandSelection From(RhinoDoc document, Seq<ObjRef> references, Seq<(Guid ObjectId, ComponentIndex ComponentIndex)> preselected) {
        ArgumentNullException.ThrowIfNull(argument: document);
        ObjRef[] source = [.. references];
        // BOUNDARY ADAPTER — ObjRef is native disposable state; snapshots must release it even when Rhino accessors throw.
        try {
            Reference[] snapshots = [.. toSeq(source).Map(reference => Reference.Of(
                reference: reference,
                preselected: preselected.Exists(item => item.ObjectId == reference.ObjectId && item.ComponentIndex == reference.GeometryComponentIndex)))];
            return new(document: document, items: toSeq(snapshots));
        } finally {
            _ = toSeq(source).Iter(static reference => reference.Dispose());
        }
    }

    internal Fin<int> SelectInto(RhinoDoc document, bool selected, DocumentSelectionPolicy policy, Op op) =>
        Try.lift<Fin<int>>(f: () => SelectNative(document: document, selected: selected, policy: policy, op: op))
            .Run()
            .MapFail(_ => op.InvalidResult())
            .Bind(static value => value);

    private Fin<int> SelectNative(RhinoDoc document, bool selected, DocumentSelectionPolicy policy, Op op) {
        ArgumentNullException.ThrowIfNull(argument: document);
        Seq<ObjRef> references = Seq<ObjRef>();
        // BOUNDARY ADAPTER — reconstructed ObjRefs are native disposable state around the selection call.
        try {
            references = Items.Map(reference => reference.ObjRef(document: document));
            return references.TraverseM(reference => document.Objects.Select(reference, selected, policy.Highlight, policy.Persistent, policy.IgnoreGrips, policy.IgnoreLayerLocking, policy.IgnoreLayerVisibility) switch {
                true => Fin.Succ(value: 1),
                false => Fin.Fail<int>(error: op.InvalidResult()),
            }).As().Map(static values => values.Fold(initialState: 0, f: static (state, value) => state + value));
        } finally {
            _ = references.Iter(static reference => reference.Dispose());
        }
    }

    public readonly record struct Reference(
        Guid ObjectId,
        uint DocumentRuntimeSerialNumber,
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
            uint detailSerial = reference.SelectionViewDetailSerialNumber();
            return new(
                ObjectId: reference.ObjectId,
                DocumentRuntimeSerialNumber: Optional(reference.Document).Map(static document => document.RuntimeSerialNumber).IfNone(0u),
                RuntimeSerialNumber: reference.RuntimeSerialNumber,
                ComponentIndex: reference.GeometryComponentIndex,
                Preselected: preselected,
                SelectionMethod: selectionMethod,
                SelectionPoint: selectionPoint switch {
                    Point3d point when point.IsValid => Some(point),
                    _ => Option<Point3d>.None,
                },
                SelectionViewRuntimeSerialNumber: Optional(selectionView).Map(static view => view.RuntimeSerialNumber),
                SelectionViewportId: detailSerial switch {
                    > 0 => Optional(RhinoObject.FromRuntimeSerialNumber(detailSerial)).Bind(static native => native is DetailViewObject detail ? Some(detail.Viewport.Id) : Option<Guid>.None) | Optional(selectionView).Map(static view => view.ActiveViewportID),
                    _ => Optional(selectionView).Map(static view => view.ActiveViewportID),
                },
                SelectionViewDetailSerialNumber: detailSerial,
                Location: PickLocation.Of(reference: reference, selectionMethod: selectionMethod));
        }

        internal static Option<Reference> Of(GetPoint getter) =>
            Optional(getter.PointOnObject())
                .Map(reference => {
                    using ObjRef owned = reference;
                    Reference snapshot = Of(reference: owned, preselected: false);
                    return snapshot with {
                        Location = PickLocation.Of(getter: getter).Case switch {
                            PickLocation location => Some(location),
                            _ => snapshot.Location,
                        },
                    };
                });

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

        internal Fin<T> Use<T>(RhinoDoc document, Op op, Func<ObjRef, Fin<T>> use) {
            Reference snapshot = this;
            return Optional(document)
                .ToFin(Fail: op.InvalidInput())
                .Bind(valid => snapshot.DocumentRuntimeSerialNumber switch {
                    0 => Fin.Succ(value: valid),
                    uint serial when serial == valid.RuntimeSerialNumber => Fin.Succ(value: valid),
                    _ => Fin.Fail<RhinoDoc>(error: op.InvalidInput()),
                })
                .Bind(valid => {
                    using ObjRef reference = snapshot.ObjRef(document: valid);
                    return use(arg: reference);
                });
        }

        public Fin<T> Use<T>(RhinoDoc document, Func<ObjRef, Fin<T>> use) =>
            Use(document: document, op: Op.Of(name: nameof(Use)), use: use);

        public Fin<RhinoView> View() =>
            SelectionViewRuntimeSerialNumber
                .Bind(static serial => Optional(RhinoView.FromRuntimeSerialNumber(serialNumber: serial)))
                .ToFin(Fail: Op.Of(name: nameof(View)).MissingContext());

        public Fin<TGeometry> Geometry<TGeometry>(RhinoDoc document) where TGeometry : GeometryBase =>
            Use(document: document, op: Op.Of(name: nameof(Geometry)), use: reference =>
                Optional(reference.Geometry())
                    .ToFin(Fail: Op.Of(name: nameof(Geometry)).InvalidResult())
                    .Bind(static geometry => geometry switch {
                        TGeometry typed => Fin.Succ(value: typed),
                        _ => Fin.Fail<TGeometry>(error: Op.Of(name: nameof(Geometry)).InvalidResult()),
                    }));
    }
}
