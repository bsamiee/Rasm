namespace Rasm.Rhino.Commands;

public readonly record struct DocumentSelectionPolicy(bool Highlight, bool IgnoreGrips, bool Persistent, bool IgnoreLayerLocking, bool IgnoreLayerVisibility) {
    public static DocumentSelectionPolicy Default { get; } = new(Highlight: true, IgnoreGrips: true, Persistent: true, IgnoreLayerLocking: false, IgnoreLayerVisibility: false);
}

file abstract record DocumentGeometry {
    private DocumentGeometry() { }
    internal abstract Fin<T> Use<T>(Op op, Func<GeometryBase, Fin<T>> use);
    internal static Fin<DocumentGeometry> Of(object source) =>
        Optional(source).ToFin(Fail: Op.Of(name: nameof(DocumentGeometry)).InvalidInput()).Bind(static value => value switch {
            GeometryBase geometry => Fin.Succ<DocumentGeometry>(value: new Borrowed(geometry)),
            Point3d point when point.IsValid => Fin.Succ<DocumentGeometry>(value: new Owned(new Point(location: point))),
            Line line when line.IsValid => Fin.Succ<DocumentGeometry>(value: new Owned(new LineCurve(line: line))),
            Circle circle when circle.IsValid => Fin.Succ<DocumentGeometry>(value: new Owned(new ArcCurve(circle: circle))),
            Arc arc when arc.IsValid => Fin.Succ<DocumentGeometry>(value: new Owned(new ArcCurve(arc: arc))),
            Polyline polyline when polyline.IsValid => Fin.Succ<DocumentGeometry>(value: new Owned(new PolylineCurve(polyline))),
            _ => Fin.Fail<DocumentGeometry>(error: Op.Of(name: nameof(DocumentGeometry)).InvalidInput()),
        });
    private sealed record Borrowed(GeometryBase Geometry) : DocumentGeometry {
        internal override Fin<T> Use<T>(Op op, Func<GeometryBase, Fin<T>> use) => from geometry in Optional(Geometry).ToFin(Fail: op.InvalidInput()) from valid in Optional(use).ToFin(Fail: op.InvalidInput()) from result in valid(arg: geometry) select result;
    }
    private sealed record Owned(GeometryBase Geometry) : DocumentGeometry {
        internal override Fin<T> Use<T>(Op op, Func<GeometryBase, Fin<T>> use) => Optional(use).ToFin(Fail: op.InvalidInput()).Bind(valid => Rasm.Rhino.UI.RhinoUi.Protect(valid: () => { try { return valid(arg: Geometry); } finally { Geometry.Dispose(); } }));
    }
}

public abstract record DocumentTarget {
    private DocumentTarget() { }

    public static Fin<DocumentTarget> Selection(CommandSelection selection) => Optional(selection).ToFin(Fail: Op.Of(name: nameof(DocumentTarget)).InvalidInput()).Map(value => (DocumentTarget)new SelectionTarget(value));

    public static Fin<DocumentTarget> Reference(CommandSelection.Reference reference) => reference.ObjectId switch { Guid id when id != Guid.Empty => Fin.Succ<DocumentTarget>(value: new ReferenceTarget(reference)), _ => Fin.Fail<DocumentTarget>(error: Op.Of(name: nameof(DocumentTarget)).InvalidInput()) };

    public static Fin<DocumentTarget> Reference(ObjRef reference) => Optional(reference).ToFin(Fail: Op.Of(name: nameof(DocumentTarget)).InvalidInput()).Map(static value => CommandSelection.Reference.Of(reference: value, preselected: false)).Bind(Reference);

    public static Fin<DocumentTarget> Objects(IEnumerable<Guid> objectIds) => TargetIds(ids: objectIds, op: Op.Of(name: nameof(DocumentTarget))).Map(ids => (DocumentTarget)new ObjectsTarget(ids));

    public static Fin<DocumentTarget> Filter(ObjectEnumeratorSettings settings) =>
        Optional(settings).ToFin(Fail: Op.Of(name: nameof(DocumentTarget)).InvalidInput()).Map(value => (DocumentTarget)new FilterTarget(value));

    internal Fin<int> Select(RhinoDoc document, bool selected, DocumentSelectionPolicy policy, Op op) =>
        Use(document: document, op: op,
            selection: value => value.SelectInto(document: document, selected: selected, policy: policy, op: op),
            reference: value => value.Use(document: document, op: op, use: native => UnitResult(success: document.Objects.Select(native, selected, policy.Highlight, policy.Persistent, policy.IgnoreGrips, policy.IgnoreLayerLocking, policy.IgnoreLayerVisibility), op: op).Map(static _ => 1)),
            objects: ids => CountResult(count: document.Objects.Select(ids.AsIterable(), selected, policy.Highlight, policy.Persistent, policy.IgnoreGrips, policy.IgnoreLayerLocking, policy.IgnoreLayerVisibility), expected: ids.Count, op: op));

    internal Fin<Unit> Delete(RhinoDoc document, bool quiet, bool ignoreModes, Op op) =>
        Use(document: document, op: op,
            selection: value => value.ObjectTargets.TraverseM(reference => reference.Use(document: document, op: op, use: native => UnitResult(success: document.Objects.Delete(native, quiet, ignoreModes), op: op))).As().Map(static _ => unit),
            reference: value => value.Use(document: document, op: op, use: native => UnitResult(success: document.Objects.Delete(native, quiet, ignoreModes), op: op)),
            objects: ids => DeleteIds(document: document, ids: ids, quiet: quiet, ignoreModes: ignoreModes, op: op));

    internal Fin<Unit> Replace(RhinoDoc document, object replacement, bool ignoreModes, Op op) =>
        Use(document: document, op: op,
            selection: value => value.Single().Bind(reference => reference.Use(document: document, op: op, use: native => ReplaceOne(document: document, reference: native, replacement: replacement, ignoreModes: ignoreModes, op: op))),
            reference: value => value.Use(document: document, op: op, use: native => ReplaceOne(document: document, reference: native, replacement: replacement, ignoreModes: ignoreModes, op: op)),
            objects: ids => ReplaceIds(document: document, ids: ids, replacement: replacement, ignoreModes: ignoreModes, op: op));

    internal Fin<Seq<Guid>> Ids(RhinoDoc document, Op op) =>
        Use(document: document, op: op,
            selection: static value => Fin.Succ(value: value.ObjectIds),
            reference: value => value.Use(document: document, op: op, use: native => native.ObjectId switch {
                Guid id when id != Guid.Empty => Fin.Succ(value: Seq(id)),
                _ => Fin.Fail<Seq<Guid>>(error: op.InvalidResult()),
            }),
            objects: static ids => Fin.Succ(value: ids));

    internal Fin<Seq<Guid>> Transform(RhinoDoc document, Transform transform, bool deleteOriginal, Op op) =>
        Use(document: document, op: op,
            selection: selection => selection.ObjectTargets.TraverseM(reference => reference.Use(document: document, op: op, use: native => IdResult(id: document.Objects.Transform(objref: native, xform: transform, deleteOriginal: deleteOriginal), op: op))).As(),
            reference: reference => reference.Use(document: document, op: op, use: native => IdResult(id: document.Objects.Transform(objref: native, xform: transform, deleteOriginal: deleteOriginal), op: op).Map(static id => Seq(id))),
            objects: ids => ids.TraverseM(id => IdResult(id: document.Objects.Transform(objectId: id, xform: transform, deleteOriginal: deleteOriginal), op: op)).As());

    internal abstract Fin<T> Use<T>(RhinoDoc document, Op op, Func<CommandSelection, Fin<T>> selection, Func<CommandSelection.Reference, Fin<T>> reference, Func<Seq<Guid>, Fin<T>> objects);

    private sealed record SelectionTarget(CommandSelection Value) : DocumentTarget {
        internal override Fin<T> Use<T>(RhinoDoc document, Op op, Func<CommandSelection, Fin<T>> selection, Func<CommandSelection.Reference, Fin<T>> reference, Func<Seq<Guid>, Fin<T>> objects) => (Value, document) switch { (CommandSelection value, RhinoDoc doc) when value.Document.RuntimeSerialNumber == doc.RuntimeSerialNumber => selection(arg: value), _ => Fin.Fail<T>(error: op.InvalidInput()) };
    }

    private sealed record ReferenceTarget(CommandSelection.Reference Value) : DocumentTarget {
        internal override Fin<T> Use<T>(RhinoDoc document, Op op, Func<CommandSelection, Fin<T>> selection, Func<CommandSelection.Reference, Fin<T>> reference, Func<Seq<Guid>, Fin<T>> objects) => reference(arg: Value);
    }

    private sealed record ObjectsTarget(Seq<Guid> Values) : DocumentTarget {
        internal override Fin<T> Use<T>(RhinoDoc document, Op op, Func<CommandSelection, Fin<T>> selection, Func<CommandSelection.Reference, Fin<T>> reference, Func<Seq<Guid>, Fin<T>> objects) => objects(arg: Values);
    }

    private sealed record FilterTarget(ObjectEnumeratorSettings Settings) : DocumentTarget {
        internal override Fin<T> Use<T>(RhinoDoc document, Op op, Func<CommandSelection, Fin<T>> selection, Func<CommandSelection.Reference, Fin<T>> reference, Func<Seq<Guid>, Fin<T>> objects) =>
            Optional(document).ToFin(Fail: op.InvalidInput()).Bind(valid => toSeq(valid.Objects.GetObjectIdList(settings: Settings)).Distinct() switch {
                Seq<Guid> ids when !ids.IsEmpty => objects(arg: ids),
                _ => Fin.Fail<T>(error: op.InvalidResult()),
            });
    }

    private static Fin<Seq<Guid>> TargetIds(IEnumerable<Guid> ids, Op op) =>
        Optional(ids)
            .ToFin(Fail: op.InvalidInput())
            .Bind(values => toSeq(values)
                .TraverseM(id => id switch {
                    Guid value when value != Guid.Empty => Fin.Succ(value: value),
                    _ => Fin.Fail<Guid>(error: op.InvalidInput()),
                })
                .As())
            .Bind(values => values.Distinct() switch {
                Seq<Guid> target when !target.IsEmpty => Fin.Succ(value: target),
                _ => Fin.Fail<Seq<Guid>>(error: op.InvalidInput()),
            });

    private static Fin<Unit> DeleteIds(RhinoDoc document, IEnumerable<Guid> ids, bool quiet, bool ignoreModes, Op op) =>
        TargetIds(ids: ids, op: op)
            .Bind(target => ignoreModes switch {
                false => document.Objects.Delete(target.AsIterable(), quiet) switch {
                    int count when count == target.Count => Fin.Succ(value: unit),
                    _ => Fin.Fail<Unit>(error: op.InvalidResult()),
                },
                true => target
                    .TraverseM(id => Optional(document.Objects.FindId(id))
                        .ToFin(Fail: op.InvalidResult())
                        .Bind(native => native.IsDeleted switch {
                            false => UnitResult(success: document.Objects.Delete(native, quiet, true), op: op),
                            true => Fin.Fail<Unit>(error: op.InvalidResult()),
                        }))
                    .Map(static _ => unit),
            });

    private static Fin<Unit> ReplaceIds(RhinoDoc document, IEnumerable<Guid> ids, object replacement, bool ignoreModes, Op op) =>
        TargetIds(ids: ids, op: op).Bind(target => target.Count switch { 1 => ReplaceOne(document: document, objectId: target[0], replacement: replacement, ignoreModes: ignoreModes, op: op), _ => Fin.Fail<Unit>(error: op.InvalidInput()) });

    internal static Fin<Guid> IdResult(Guid id, Op op) => id switch { Guid value when value != Guid.Empty => Fin.Succ(value: value), _ => Fin.Fail<Guid>(error: op.InvalidResult()) };

    private static Fin<Unit> ReplaceOne(RhinoDoc document, Guid objectId, object replacement, bool ignoreModes, Op op) =>
        ReplaceGeometry(replacement: replacement, op: op, use: geometry => document.Objects.Replace(objectId: objectId, geometry: geometry, ignoreModes: ignoreModes));

    private static Fin<Unit> ReplaceOne(RhinoDoc document, ObjRef reference, object replacement, bool ignoreModes, Op op) =>
        ReplaceGeometry(replacement: replacement, op: op, use: geometry => document.Objects.Replace(objref: reference, geometry: geometry, ignoreModes: ignoreModes));

    private static Fin<Unit> ReplaceGeometry(object replacement, Op op, Func<GeometryBase, bool> use) =>
        from valid in Optional(use).ToFin(Fail: op.InvalidInput())
        from geometry in DocumentGeometry.Of(source: replacement)
        from result in geometry.Use(op: op, use: native => UnitResult(success: valid(arg: native), op: op))
        select result;

    private static Fin<int> CountResult(int count, int expected, Op op) =>
        count switch {
            int value when value == expected => Fin.Succ(value: value),
            _ => Fin.Fail<int>(error: op.InvalidResult()),
        };

    private static Fin<Unit> UnitResult(bool success, Op op) =>
        success switch {
            true => Fin.Succ(value: unit),
            false => Fin.Fail<Unit>(error: op.InvalidResult()),
        };
}

public sealed record DocumentEdit {
    internal DocumentEdit(RhinoDoc document, Rasm.Domain.Context domain) {
        ArgumentNullException.ThrowIfNull(argument: document);
        ArgumentNullException.ThrowIfNull(argument: domain);
        Document = document;
        Domain = domain;
    }

    public RhinoDoc Document { get; }
    public Rasm.Domain.Context Domain { get; }

    public Fin<DocumentReceipt> Commit(DocumentTransaction transaction) =>
        from document in Available(op: Op.Of(name: nameof(Commit)))
        from plan in Optional(transaction).ToFin(Fail: Op.Of(name: nameof(Commit)).InvalidInput())
        from receipt in plan.Operations switch {
            Seq<DocumentOp> operations when !operations.IsEmpty => Mutate(document: document, name: plan.Name, run: () => operations.TraverseM(operation => operation.Apply(document: document, domain: Domain, op: Op.Of(name: plan.Name))).As().Map(static receipts => receipts.Fold(DocumentReceipt.Empty, static (state, receipt) => state + receipt))),
            _ => Fin.Fail<DocumentReceipt>(error: Op.Of(name: nameof(Commit)).InvalidInput()),
        }
        from _ in plan.Redraw.Apply(edit: this)
        select receipt;

    public Fin<Guid> Add(GeometryBase geometry, ObjectAttributes? attributes = null) =>
        from g in Optional(geometry).ToFin(Fail: Op.Of(name: nameof(Add)).InvalidInput())
        from ids in Commit(transaction: DocumentTransaction.One(name: nameof(Add), operation: new DocumentOp.Create(Sources: Seq<object>(g), Attributes: attributes), redraw: DocumentRedraw.None)).Map(static receipt => receipt.Created)
        from id in ids.Count switch {
            1 => Fin.Succ(value: ids[0]),
            _ => Fin.Fail<Guid>(error: Op.Of(name: nameof(Add)).InvalidResult()),
        }
        select id;

    public Fin<Guid> Add(object source, ObjectAttributes? attributes = null) =>
        from geometry in DocumentGeometry.Of(source: source)
        from id in geometry.Use(op: Op.Of(name: nameof(Add)), use: native => Add(geometry: native, attributes: attributes))
        select id;

    public Fin<Seq<Guid>> Add(IEnumerable<GeometryBase> geometries, ObjectAttributes? attributes = null) =>
        from source in Optional(geometries).ToFin(Fail: Op.Of(name: nameof(Add)).InvalidInput())
        from ids in Commit(transaction: DocumentTransaction.One(name: nameof(Add), operation: new DocumentOp.Create(Sources: toSeq(source).Map(static geometry => (object)geometry), Attributes: attributes), redraw: DocumentRedraw.None)).Map(static receipt => receipt.Created)
        select ids;

    public Fin<Seq<Guid>> AddPoints(IEnumerable<Point3d> points, ObjectAttributes? attributes = null) =>
        from document in Available(op: Op.Of(name: nameof(AddPoints)))
        from values in Optional(points).ToFin(Fail: Op.Of(name: nameof(AddPoints)).InvalidInput())
        from ids in (Point3d[])[.. values.AsIterable()] switch {
            { Length: > 0 } native when toSeq(native).ForAll(static point => point.IsValid) => Mutate(document: document, name: nameof(AddPoints), run: () => toSeq(attributes switch { ObjectAttributes attrs => document.Objects.AddPoints(native, attrs), _ => document.Objects.AddPoints(native) }) switch { Seq<Guid> result when result.Count == native.Length && result.ForAll(static id => id != Guid.Empty) => Fin.Succ(value: result), _ => Fin.Fail<Seq<Guid>>(error: Op.Of(name: nameof(AddPoints)).InvalidResult()) }),
            _ => Fin.Fail<Seq<Guid>>(error: Op.Of(name: nameof(AddPoints)).InvalidInput()),
        }
        select ids;

    public Fin<Unit> Replace(DocumentTarget target, object replacement, bool ignoreModes = false) =>
        Commit(transaction: DocumentTransaction.One(name: nameof(Replace), operation: new DocumentOp.Replace(Target: target, Replacement: replacement, IgnoreModes: ignoreModes), redraw: DocumentRedraw.None)).Map(static _ => unit);

    public Fin<Unit> Delete(DocumentTarget target, bool quiet = true, bool ignoreModes = false) =>
        Commit(transaction: DocumentTransaction.One(name: nameof(Delete), operation: new DocumentOp.Lifecycle(Target: target, Action: DocumentLifecycle.Delete(quiet: quiet, ignoreModes: ignoreModes)), redraw: DocumentRedraw.None)).Map(static _ => unit);

    public Fin<Seq<Guid>> Transform(DocumentTarget target, Transform transform, bool deleteOriginal = true) =>
        Commit(transaction: DocumentTransaction.One(name: nameof(Transform), operation: new DocumentOp.Transform(Target: target, Xform: transform, DeleteOriginal: deleteOriginal), redraw: DocumentRedraw.None)).Map(static receipt => receipt.Transformed);

    public Fin<int> Attributes(DocumentTarget target, Func<ObjectAttributes, Fin<ObjectAttributes>> change, bool quiet = true) =>
        Commit(transaction: DocumentTransaction.One(name: nameof(Attributes), operation: new DocumentOp.AttributeChange(Target: target, Change: change, Quiet: quiet), redraw: DocumentRedraw.None)).Map(static receipt => receipt.AttributeChanged.Count);

    public Fin<int> Select(DocumentTarget target, bool selected = true, DocumentSelectionPolicy? policy = null) =>
        Commit(transaction: DocumentTransaction.One(name: nameof(Select), operation: new DocumentOp.Lifecycle(Target: target, Action: DocumentLifecycle.Select(selected: selected, policy: policy ?? DocumentSelectionPolicy.Default)), redraw: DocumentRedraw.None)).Map(static receipt => receipt.Selected.Count);

    public Fin<int> SetSelection(DocumentTarget target, DocumentSelectionPolicy? policy = null) =>
        from document in Available(op: Op.Of(name: nameof(SetSelection))) from valid in Optional(target).ToFin(Fail: Op.Of(name: nameof(SetSelection)).InvalidInput()) from ids in valid.Ids(document: document, op: Op.Of(name: nameof(SetSelection)))
        from chosen in ids.Distinct() switch { Seq<Guid> values when !values.IsEmpty => Fin.Succ(value: values), _ => Fin.Fail<Seq<Guid>>(error: Op.Of(name: nameof(SetSelection)).InvalidInput()) }
        let active = policy ?? DocumentSelectionPolicy.Default
        from count in document.Objects.SetSelectedObjects(objectIds: chosen.AsIterable(), syncHighlight: active.Highlight, persistentSelect: active.Persistent, ignoreGripsState: active.IgnoreGrips, ignoreLayerLocking: active.IgnoreLayerLocking, ignoreLayerVisibility: active.IgnoreLayerVisibility) switch {
            int value when value == chosen.Count => Fin.Succ(value: value),
            _ => Fin.Fail<int>(error: Op.Of(name: nameof(SetSelection)).InvalidResult()),
        }
        select count;

    public Fin<int> Hide(DocumentTarget target, bool hidden = true) =>
        Commit(transaction: DocumentTransaction.One(name: nameof(Hide), operation: new DocumentOp.Lifecycle(Target: target, Action: DocumentLifecycle.Hide(hidden: hidden)), redraw: DocumentRedraw.None)).Map(static receipt => receipt.Hidden.Count);

    public Fin<int> Lock(DocumentTarget target, bool locked = true) =>
        Commit(transaction: DocumentTransaction.One(name: nameof(Lock), operation: new DocumentOp.Lifecycle(Target: target, Action: DocumentLifecycle.Lock(locked: locked)), redraw: DocumentRedraw.None)).Map(static receipt => receipt.Locked.Count);

    public Fin<int> Reveal(DocumentTarget target) => from count in Select(target: target, selected: true) from _ in Redraw() select count;

    public Fin<int> UnselectAll(bool ignorePersistentSelections = false) =>
        from document in Available(op: Op.Of(name: nameof(UnselectAll)))
        from count in document.Objects.UnselectAll(ignorePersistentSelections: ignorePersistentSelections) switch {
            int value and >= 0 => Fin.Succ(value: value),
            _ => Fin.Fail<int>(error: Op.Of(name: nameof(UnselectAll)).InvalidResult()),
        }
        select count;

    public Fin<Unit> Redraw() =>
        Available(op: Op.Of(name: nameof(Redraw)))
            .Map(document => {
                document.Views.Redraw();
                return unit;
            });

    private Fin<RhinoDoc> Available(Op op) =>
        Document switch {
            { IsAvailable: true, IsClosing: false, IsInitializing: false, IsOpening: false } document => Fin.Succ(value: document),
            _ => Fin.Fail<RhinoDoc>(error: op.InvalidInput()),
        };

    private static Fin<T> Mutate<T>(RhinoDoc document, string name, Func<Fin<T>> run) => Optional(run).ToFin(Fail: Op.Of(name: name).InvalidInput()).Bind(valid => Rasm.Rhino.UI.RhinoUi.Protect(valid: () => { uint undo = document.UndoRecordingIsActive switch { true => 0u, false => document.BeginUndoRecord(description: name) }; try { return valid(); } finally { _ = undo > 0u && document.EndUndoRecord(undoRecordSerialNumber: undo); } }));

    internal static Fin<Seq<Guid>> ApplyState(Seq<Guid> ids, RhinoDoc document, Op op, Func<RhinoObject, bool> ready, Func<RhinoObject, bool> done, Func<Guid, bool> apply) =>
        ids.TraverseM(id => Optional(document.Objects.FindId(id))
            .ToFin(Fail: op.InvalidResult())
            .Bind(native => (ready(arg: native), done(arg: native)) switch {
                (false, _) => Fin.Fail<Option<Guid>>(error: op.InvalidInput()),
                (_, true) => Fin.Succ(value: Option<Guid>.None),
                _ => apply(arg: id) switch {
                    true => Fin.Succ(value: Some(id)),
                    false => Fin.Fail<Option<Guid>>(error: op.InvalidResult()),
                },
            })).As().Map(static result => result.Somes());

    internal static Fin<Seq<Guid>> AddRaw(RhinoDoc document, Rasm.Domain.Context domain, IEnumerable<object> sources, ObjectAttributes? attributes, Op op) =>
        from validDocument in Optional(document).ToFin(Fail: op.InvalidInput())
        from validDomain in Optional(domain).ToFin(Fail: op.InvalidInput())
        from source in Optional(sources).ToFin(Fail: op.InvalidInput())
        from values in toSeq(source) switch {
            Seq<object> items when !items.IsEmpty => Fin.Succ(value: items),
            _ => Fin.Fail<Seq<object>>(error: op.InvalidInput()),
        }
        from ids in values.TraverseM(value =>
            from geometry in DocumentGeometry.Of(source: value)
            from id in geometry.Use(op: op, use: native =>
                from _ in Requirement.Basic.Apply(context: validDomain, value: native, cancel: CancellationToken.None).ToFin()
                from created in DocumentTarget.IdResult(id: attributes switch { ObjectAttributes attrs => validDocument.Objects.Add(native, attrs), _ => validDocument.Objects.Add(native) }, op: op)
                select created)
            select id).As()
        select ids;

}
