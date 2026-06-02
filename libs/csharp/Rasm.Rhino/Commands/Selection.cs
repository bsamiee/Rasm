namespace Rasm.Rhino.Commands;

// --- [TYPES] ------------------------------------------------------------------------------
[Union]
public abstract partial record PickHit {
    private PickHit() { }
    public sealed record Curve(global::Rhino.Geometry.Curve Value, double T) : PickHit;
    public sealed record Surface(global::Rhino.Geometry.Surface Value, double U, double V) : PickHit;
    public sealed record Brep(BrepFace Value, double U, double V) : PickHit;

    public Point3d Location =>
        Switch(
            curve: static value => value.Value.PointAt(t: value.T),
            surface: static value => value.Value.PointAt(u: value.U, v: value.V),
            brep: static value => value.Value.PointAt(u: value.U, v: value.V));

    internal static Option<PickHit> Of(Option<ObjRef> reference) =>
        reference.Bind(active => {
            double t = double.NaN, u = double.NaN, v = double.NaN;
            bool picked = active.SelectionMethod() == SelectionMethod.MousePick;
            global::Rhino.Geometry.Curve? curve = picked ? active.CurveParameter(parameter: out t) : null;
            global::Rhino.Geometry.Surface? surface = picked ? active.SurfaceParameter(u: out u, v: out v) : null;
            BrepFace? face = picked ? active.Face() : null;
            return (curve, surface, face) switch {
                (global::Rhino.Geometry.Curve c, _, _) when RhinoMath.IsValidDouble(x: t) => Some<PickHit>(new Curve(Value: c, T: t)),
                (_, _, BrepFace f) when RhinoMath.IsValidDouble(x: u) && RhinoMath.IsValidDouble(x: v) => Some<PickHit>(new Brep(Value: f, U: u, V: v)),
                (_, global::Rhino.Geometry.Surface s, _) when RhinoMath.IsValidDouble(x: u) && RhinoMath.IsValidDouble(x: v) => Some<PickHit>(new Surface(Value: s, U: u, V: v)),
                _ => Option<PickHit>.None,
            };
        });
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record CommandPickPolicy(
    Option<RhinoView> View = default,
    Option<Line> PickLine = default,
    PickStyle PickStyle = PickStyle.PointPick,
    PickMode PickMode = PickMode.Shaded,
    bool PickGroups = false,
    bool SubObjects = false,
    Option<Transform> Transform = default,
    bool UpdateClippingPlanes = true) {
    internal Fin<T> Use<T>(RhinoDoc document, Func<PickContext, Fin<T>> use) =>
        from valid in Optional(use).ToFin(Fail: Op.Of(name: nameof(CommandPickPolicy)).InvalidInput())
        from targetView in View.Case switch {
            RhinoView value => Fin.Succ(value: Some(value)),
            _ when !UpdateClippingPlanes => Fin.Succ(value: Option<RhinoView>.None),
            _ => Optional(document).Bind(static doc => Optional(doc.Views.ActiveView)).ToFin(Fail: Op.Of(name: nameof(CommandPickPolicy)).InvalidInput()).Map(Some),
        }
        from result in UI.RhinoUi.Protect(valid: () => {
            using PickContext context = new();
            _ = targetView.Iter(active => context.View = active);
            _ = PickLine.Iter(line => context.PickLine = line);
            context.PickStyle = PickStyle;
            context.PickMode = PickMode;
            context.PickGroupsEnabled = PickGroups;
            context.SubObjectSelectionEnabled = SubObjects;
            _ = Transform.Iter(active => context.SetPickTransform(active));
            return valid(arg: context);
        })
        select result;
}

public readonly record struct ReferenceHit(Option<double> CurveParameter, Option<Point2d> SurfaceParameter, Option<PickHit> Geometry) {
    internal static Option<ReferenceHit> Of(ObjRef reference, SelectionMethod selectionMethod) {
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
        return Of(curve: curve, surface: surface, geometry: PickHit.Of(reference: Optional(reference)));
    }

    private static Option<ReferenceHit> Of(Option<double> curve, Option<Point2d> surface, Option<PickHit> geometry) =>
        (curve.IsSome || surface.IsSome || geometry.IsSome) switch {
            true => Some(new ReferenceHit(CurveParameter: curve, SurfaceParameter: surface, Geometry: geometry)),
            false => Option<ReferenceHit>.None,
        };
}

// --- [SERVICES] ---------------------------------------------------------------------------
public sealed partial record CommandSelection {
    private CommandSelection(RhinoDoc document, Seq<Reference> items) {
        ArgumentNullException.ThrowIfNull(argument: document);
        Document = document;
        Items = items;
    }

    public RhinoDoc Document { get; }
    public Seq<Reference> Items { get; }
    public Seq<Guid> ObjectIds => Items.Map(static item => item.ObjectId).Distinct();
    public Seq<Guid> MutationObjectIds => Items.Map(static item => item.MutationObjectId).Distinct();
    internal Seq<Reference> ObjectTargets =>
        toSeq(Items.DistinctBy(static item => (item.ObjectId, item.RuntimeSerialNumber)));

    public Fin<Seq<T>> Project<T>(Func<Reference, Fin<T>> project) => Optional(project).ToFin(Fail: Op.Of(name: nameof(Project)).InvalidInput()).Bind(valid => Items.TraverseM(reference => valid(arg: reference)).As());

    public Fin<Seq<T>> Project<T>(CommandObjectSelection policy, Func<Reference, Fin<T>> project) =>
        from trimmed in Trim(policy: policy)
        from values in trimmed.Project(project: project)
        select values;

    public Fin<Seq<TGeometry>> Geometry<TGeometry>() where TGeometry : GeometryBase =>
        Project(project: reference => reference.Geometry<TGeometry>(document: Document));

    public Fin<Reference> Single() =>
        Items.Count switch {
            1 => Fin.Succ(value: Items[0]),
            _ => Fin.Fail<Reference>(error: Op.Of(name: nameof(Single)).InvalidInput()),
        };

    public Fin<CommandSelection> Trim(CommandObjectSelection policy) =>
        from trimmed in Trimmed(policy: policy)
        from selection in trimmed.Require(policy: policy)
        select selection;

    public Fin<TrimResult> Trimmed(CommandObjectSelection policy) =>
        from active in Optional(policy).ToFin(Fail: Op.Of(name: nameof(Trimmed)).InvalidInput())
        let values = toSeq(Items
            .Filter(item => active.SubObjects || !item.IsSubObject)
            .Filter(item => item.Preselected || active.AlreadySelected || !item.Selected)
            .Filter(item => active.References || !item.IsReference)
            .Filter(item => active.Locked || !item.IsLocked)
            .Filter(item => !active.IgnoreGrips || !item.IsGrip)
            .DistinctBy(static item => (item.ObjectId, item.RuntimeSerialNumber, item.ComponentIndex.ComponentIndexType, item.ComponentIndex.Index)))
        select new TrimResult(Selection: new CommandSelection(document: Document, items: values), Accepted: values.Count, Rejected: Math.Max(0, Items.Count - values.Count));

    public readonly record struct TrimResult(CommandSelection Selection, int Accepted, int Rejected) {
        public Fin<CommandSelection> Require(CommandObjectSelection policy) {
            int accepted = Accepted;
            CommandSelection selection = Selection;
            return
            from active in Optional(policy).ToFin(Fail: Op.Of(name: nameof(TrimResult)).InvalidInput())
            from _ in guard(accepted >= active.Minimum && (active.Maximum < 0 || accepted <= active.Maximum), Op.Of(name: nameof(TrimResult)).InvalidInput())
            select selection;
        }

        public Fin<CommandSelection> RequireAny() {
            int accepted = Accepted;
            CommandSelection selection = Selection;
            return accepted switch {
                > 0 => Fin.Succ(value: selection),
                _ => Fin.Fail<CommandSelection>(error: Op.Of(name: nameof(TrimResult)).InvalidResult()),
            };
        }
    }

    public static Fin<CommandSelection> Pick(RhinoDoc document, CommandPickPolicy policy) =>
        new SelectionSource.PickPolicy(policy).Read(document: document);

    public static Fin<CommandSelection> Pick(RhinoDoc document, PickContext context) =>
        new SelectionSource.Context(Value: context, UpdateClippingPlanes: true).Read(document: document);

    internal static Fin<CommandSelection> FromGetter(RhinoDoc document, GetObject getter, GetResult raw) =>
        new SelectionSource.Getter(Source: getter, Raw: raw).Read(document: document);

    [Union(SwitchMapStateParameterName = "document")]
    private abstract partial record SelectionSource {
        private SelectionSource() { }
        public sealed record PickPolicy(CommandPickPolicy Policy) : SelectionSource;
        public sealed record Context(PickContext Value, bool UpdateClippingPlanes) : SelectionSource;
        public sealed record Getter(GetObject Source, GetResult Raw) : SelectionSource;

        internal Fin<CommandSelection> Read(RhinoDoc document) =>
            Switch(
                document,
                pickPolicy: static (doc, source) =>
                    from valid in Optional(source.Policy).ToFin(Fail: Op.Of(name: nameof(Pick)).InvalidInput())
                    from selection in valid.Use(document: doc, use: context => new Context(Value: context, UpdateClippingPlanes: valid.UpdateClippingPlanes).Read(document: doc))
                    select selection,
                context: static (doc, source) => PickWith(document: doc, context: source.Value, updateClippingPlanes: source.UpdateClippingPlanes),
                getter: static (doc, source) =>
                    source.Raw is GetResult.Object && source.Source.Objects() is ObjRef[] references
                    ? Fin.Succ(value: From(
                        document: doc,
                        references: toSeq(references),
                        preselected: source.Source.ObjectsWerePreselected
                            ? toSeq(references)
                                .Filter(static reference => Optional(reference.Object()).Map(static item => item.IsSelected(checkSubObjects: true) > 0).IfNone(noneValue: false))
                                .Map(static reference => (reference.ObjectId, reference.GeometryComponentIndex))
                            : Seq<(Guid ObjectId, ComponentIndex ComponentIndex)>()))
                    : Fin.Fail<CommandSelection>(error: Op.Of(name: nameof(FromGetter)).InvalidResult()));

        private static Fin<CommandSelection> PickWith(RhinoDoc document, PickContext context, bool updateClippingPlanes) =>
        from validDocument in Optional(document).ToFin(Fail: Op.Of(name: nameof(Pick)).InvalidInput())
        from validContext in Optional(context).ToFin(Fail: Op.Of(name: nameof(Pick)).InvalidInput())
        from _view in updateClippingPlanes && validContext.View is null
            ? Optional(validDocument.Views.ActiveView)
                .ToFin(Fail: Op.Of(name: nameof(Pick)).MissingContext())
                .Map(view => { validContext.View = view; return unit; })
            : Fin.Succ(value: unit)
        from selection in UI.RhinoUi.Protect(valid: () => {
            _ = Op.SideWhen(updateClippingPlanes, validContext.UpdateClippingPlanes);
            return Optional(validDocument.Objects.PickObjects(pickContext: validContext)).ToFin(Fail: Op.Of(name: nameof(Pick)).InvalidResult()).Bind(references => references switch { { Length: > 0 } values => Fin.Succ(value: From(document: validDocument, references: toSeq(values), preselected: Seq<(Guid ObjectId, ComponentIndex ComponentIndex)>())), _ => Fin.Fail<CommandSelection>(error: Op.Of(name: nameof(Pick)).InvalidResult()) });
        })
        select selection;
    }

    internal static CommandSelection From(RhinoDoc document, Seq<ObjRef> references, Seq<(Guid ObjectId, ComponentIndex ComponentIndex)> preselected) {
        ArgumentNullException.ThrowIfNull(argument: document);
        ObjRef[] source = [.. references];
        try {
            Reference[] snapshots = [.. toSeq(source).Map(reference => Reference.Of(
                document: document,
                reference: reference,
                preselected: preselected.Exists(item => item.ObjectId == reference.ObjectId && item.ComponentIndex == reference.GeometryComponentIndex)))];
            return new(document: document, items: toSeq(snapshots));
        } finally {
            _ = toSeq(source).Iter(static reference => reference.Dispose());
        }
    }

    internal static Fin<CommandSelection> FromObjects(RhinoDoc document, IEnumerable<Guid> objectIds, CommandObjectSelection policy) =>
        from validDocument in Optional(document).ToFin(Fail: Op.Of(name: nameof(FromObjects)).InvalidInput())
        from active in Optional(policy).ToFin(Fail: Op.Of(name: nameof(FromObjects)).InvalidInput())
        from target in DocumentTarget.Objects(objectIds: objectIds)
        from ids in target.Ids(document: validDocument, op: Op.Of(name: nameof(FromObjects)))
        from references in ids.TraverseM(id => Optional(validDocument.Objects.FindId(id)).ToFin(Fail: Op.Of(name: nameof(FromObjects)).InvalidResult()).Map(native => new ObjRef(rhinoObject: native))).As()
        from selection in Fin.Succ(value: From(document: validDocument, references: references, preselected: Seq<(Guid ObjectId, ComponentIndex ComponentIndex)>()))
        from trimmed in selection.Trim(policy: active)
        select trimmed;

    internal Fin<int> SelectInto(RhinoDoc document, bool selected, DocumentSelectionPolicy policy, Op op) {
        CommandSelection self = this;
        return op.Catch(() => self.SelectNative(document: document, selected: selected, policy: policy, op: op));
    }

    private Fin<int> SelectNative(RhinoDoc document, bool selected, DocumentSelectionPolicy policy, Op op) {
        ArgumentNullException.ThrowIfNull(argument: document);
        Seq<ObjRef> references = Seq<ObjRef>();
        try {
            references = Items.Map(reference => reference.ObjRef(document: document));
            return references.TraverseM(reference => policy.Select((highlight, persistent, ignoreGrips, ignoreLayerLocking, ignoreLayerVisibility) => document.Objects.Select(reference, selected, highlight, persistent, ignoreGrips, ignoreLayerLocking, ignoreLayerVisibility)) switch {
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
        bool Selected,
        bool Locked,
        bool Hidden,
        bool ReferenceObject,
        ObjectType ObjectType,
        SelectionMethod SelectionMethod,
        Option<(Guid OwnerId, Guid GripId, int Index, bool Selected)> Grip,
        Option<Point3d> SelectionPoint,
        Option<uint> SelectionViewRuntimeSerialNumber,
        Option<Guid> SelectionViewportId,
        uint SelectionViewDetailSerialNumber,
        Option<ReferenceHit> Hit) {
        public bool IsSubObject => ComponentIndex.IsSet;
        public bool IsGrip => Grip.IsSome;
        public bool IsLocked => Locked;
        public bool IsReference => ReferenceObject;
        public Guid MutationObjectId => GripOwnerId.IfNone(ObjectId);
        public Option<Guid> GripOwnerId => Grip.Map(static value => value.OwnerId);
        public Option<Guid> GripObjectId => Grip.Map(static value => value.GripId);
        public Option<int> GripIndex => Grip.Map(static value => value.Index);
        public Option<bool> GripSelected => Grip.Map(static value => value.Selected);

        internal static Reference Of(RhinoDoc document, ObjRef reference, bool preselected) {
            ArgumentNullException.ThrowIfNull(argument: document);
            ArgumentNullException.ThrowIfNull(argument: reference);
            SelectionMethod selectionMethod = reference.SelectionMethod();
            Point3d selectionPoint = reference.SelectionPoint();
            RhinoView? selectionView = reference.SelectionView();
            RhinoObject? native = reference.Object();
            uint detailSerial = reference.SelectionViewDetailSerialNumber();
            return new(
                ObjectId: reference.ObjectId,
                DocumentRuntimeSerialNumber: document.RuntimeSerialNumber,
                RuntimeSerialNumber: reference.RuntimeSerialNumber,
                ComponentIndex: reference.GeometryComponentIndex,
                Preselected: preselected,
                Selected: Optional(native).Map(static value => value.IsSelected(checkSubObjects: true) > 0).IfNone(noneValue: false),
                Locked: Optional(native).Map(static value => value.IsLocked).IfNone(noneValue: false),
                Hidden: Optional(native).Map(static value => value.IsHidden).IfNone(noneValue: false),
                ReferenceObject: Optional(native).Map(static value => value.IsReference).IfNone(noneValue: false),
                ObjectType: Optional(native).Map(static value => value.ObjectType).IfNone(ObjectType.None),
                SelectionMethod: selectionMethod,
                Grip: Optional(native as GripObject).Map(static grip => (grip.OwnerId, grip.Id, grip.Index, grip.IsSelected(checkSubObjects: true) > 0)),
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
                Hit: ReferenceHit.Of(reference: reference, selectionMethod: selectionMethod));
        }

        internal static Option<Reference> Of(GetPoint getter) =>
            Optional(getter).Bind(valid => {
                using ObjRef? reference = valid.PointOnObject();
                return Of(reference: Optional(reference));
            });

        internal static Option<Reference> Of(Option<ObjRef> reference) =>
            reference.Bind(active =>
                Optional(active.Document).Map(document => Of(document: document, reference: active, preselected: false)));

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
        internal static Fin<T> Extract<T>(ObjRef reference, Op op) where T : class =>
            NativeOf<T>(reference: reference).ToFin(Fail: op.InvalidResult(detail: $"{typeof(T).Name} not extractable"));

        public Fin<TGeometry> Geometry<TGeometry>(RhinoDoc document) where TGeometry : GeometryBase {
            Op op = Op.Of(name: nameof(Geometry));
            return Use(document: document, op: op, use: reference =>
                Extract<TGeometry>(reference: reference, op: op)
                    .Bind(geometry => Optional(geometry.Duplicate() as TGeometry).ToFin(Fail: op.InvalidResult())));
        }

        public Fin<T> Project<TPart, T>(RhinoDoc document, Func<TPart, Fin<T>> use) where TPart : class {
            Reference snapshot = this;
            Op op = Op.Of(name: nameof(Project));
            return Optional(use).ToFin(Fail: op.InvalidInput())
                .Bind(valid => snapshot.Use(document: document, op: op, use: reference => Extract<TPart>(reference: reference, op: op).Bind(valid)));
        }

        private static Option<TPart> NativeOf<TPart>(ObjRef reference) where TPart : class =>
            Optional(reference).Bind(valid => typeof(TPart) switch {
                Type v when v == typeof(Brep) => Cast<TPart>(valid.Brep()),
                Type v when v == typeof(BrepFace) => Cast<TPart>(valid.Face()),
                Type v when v == typeof(BrepEdge) => Cast<TPart>(valid.Edge()),
                Type v when v == typeof(BrepTrim) => Cast<TPart>(valid.Trim()),
                Type v when v == typeof(SubD) => Cast<TPart>(valid.SubD()),
                Type v when v == typeof(SubDFace) => Cast<TPart>(valid.SubDFace()),
                Type v when v == typeof(SubDEdge) => Cast<TPart>(valid.SubDEdge()),
                Type v when v == typeof(SubDVertex) => Cast<TPart>(valid.SubDVertex()),
                Type v when v == typeof(ClippingPlaneSurface) => Cast<TPart>(valid.ClippingPlaneSurface()),
                Type v when v == typeof(Curve) => Cast<TPart>(valid.Curve()),
                Type v when v == typeof(Surface) => Cast<TPart>(valid.Surface()),
                Type v when v == typeof(Mesh) => Cast<TPart>(valid.Mesh()),
                Type v when v == typeof(Point) => Cast<TPart>(valid.Point()),
                Type v when v == typeof(PointCloud) => Cast<TPart>(valid.PointCloud()),
                Type v when v == typeof(TextDot) => Cast<TPart>(valid.TextDot()),
                Type v when v == typeof(TextEntity) => Cast<TPart>(valid.TextEntity()),
                Type v when v == typeof(Light) => Cast<TPart>(valid.Light()),
                Type v when v == typeof(Hatch) => Cast<TPart>(valid.Hatch()),
                Type v when typeof(RhinoObject).IsAssignableFrom(c: v) => Cast<TPart>(valid.InstanceDefinitionPart()) | Cast<TPart>(valid.Object()),
                Type v when typeof(GeometryBase).IsAssignableFrom(c: v) => Cast<TPart>(valid.Geometry()),
                _ => Option<TPart>.None,
            });

        private static Option<TPart> Cast<TPart>(object? value) where TPart : class =>
            value is TPart typed ? Some(typed) : Option<TPart>.None;
    }
}
