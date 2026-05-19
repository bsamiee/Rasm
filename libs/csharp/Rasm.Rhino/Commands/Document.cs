namespace Rasm.Rhino.Commands;

public readonly record struct DocumentSelectionPolicy(bool Highlight, bool IgnoreGrips, bool Persistent, bool IgnoreLayerLocking, bool IgnoreLayerVisibility) {
    public static DocumentSelectionPolicy Default { get; } = new(Highlight: true, IgnoreGrips: true, Persistent: true, IgnoreLayerLocking: false, IgnoreLayerVisibility: false);
}

public abstract record DocumentTarget {
    private DocumentTarget() { }

    public static Fin<DocumentTarget> Selection(CommandSelection selection) => Optional(selection).ToFin(Fail: Op.Of(name: nameof(DocumentTarget)).InvalidInput()).Map(value => (DocumentTarget)new SelectionTarget(value));

    public static Fin<DocumentTarget> Reference(CommandSelection.Reference reference) => reference.ObjectId switch { Guid id when id != Guid.Empty => Fin.Succ<DocumentTarget>(value: new ReferenceTarget(reference)), _ => Fin.Fail<DocumentTarget>(error: Op.Of(name: nameof(DocumentTarget)).InvalidInput()) };

    public static Fin<DocumentTarget> Reference(ObjRef reference) => Optional(reference).ToFin(Fail: Op.Of(name: nameof(DocumentTarget)).InvalidInput()).Map(static value => CommandSelection.Reference.Of(reference: value, preselected: false)).Bind(Reference);

    public static Fin<DocumentTarget> Objects(IEnumerable<Guid> objectIds) => TargetIds(ids: objectIds, op: Op.Of(name: nameof(DocumentTarget))).Map(ids => (DocumentTarget)new ObjectsTarget(ids));

    internal Fin<int> Select(RhinoDoc document, bool selected, DocumentSelectionPolicy policy, Op op) =>
        Use(document: document, op: op,
            selection: value => value.SelectInto(document: document, selected: selected, policy: policy, op: op),
            reference: value => value.Use(document: document, op: op, use: native => UnitResult(success: document.Objects.Select(native, selected, policy.Highlight, policy.Persistent, policy.IgnoreGrips, policy.IgnoreLayerLocking, policy.IgnoreLayerVisibility), op: op).Map(static _ => 1)),
            objects: ids => CountResult(count: document.Objects.Select(ids.AsIterable(), selected, policy.Highlight, policy.Persistent, policy.IgnoreGrips, policy.IgnoreLayerLocking, policy.IgnoreLayerVisibility), op: op));

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
        Optional(use).ToFin(Fail: op.InvalidInput()).Bind(valid => ToGeometry(replacement: replacement) switch {
            (GeometryBase geometry, bool owned) => Rasm.Rhino.UI.RhinoUi.Protect(valid: () => {
                try {
                    return UnitResult(success: valid(arg: geometry), op: op);
                } finally {
                    _ = owned switch {
                        true => ((Func<Unit>)(() => { geometry.Dispose(); return unit; }))(),
                        false => unit,
                    };
                }
            }),
            _ => Fin.Fail<Unit>(error: op.InvalidInput()),
        });

    private static (GeometryBase Geometry, bool Owned)? ToGeometry(object replacement) =>
        replacement switch {
            GeometryBase geometry => (Geometry: geometry, Owned: false),
            Point3d point => (Geometry: new Point(location: point), Owned: true),
            Line line => (Geometry: new LineCurve(line: line), Owned: true),
            Circle circle => (Geometry: new ArcCurve(circle: circle), Owned: true),
            Arc arc => (Geometry: new ArcCurve(arc: arc), Owned: true),
            Polyline polyline => (Geometry: new PolylineCurve(polyline), Owned: true),
            _ => null,
        };

    private static Fin<int> CountResult(int count, Op op) =>
        count switch {
            >= 0 => Fin.Succ(value: count),
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

    public Fin<Guid> Add(GeometryBase geometry, ObjectAttributes? attributes = null) =>
        from g in Optional(geometry).ToFin(Fail: Op.Of(name: nameof(Add)).InvalidInput())
        from ids in Add(geometries: Seq(g), attributes: attributes)
        from id in ids.Count switch {
            1 => Fin.Succ(value: ids[0]),
            _ => Fin.Fail<Guid>(error: Op.Of(name: nameof(Add)).InvalidResult()),
        }
        select id;

    public Fin<Seq<Guid>> Add(IEnumerable<GeometryBase> geometries, ObjectAttributes? attributes = null) =>
        from document in Available(op: Op.Of(name: nameof(Add)))
        from source in Optional(geometries).ToFin(Fail: Op.Of(name: nameof(Add)).InvalidInput())
        from values in toSeq(source) switch {
            Seq<GeometryBase> items when !items.IsEmpty && items.ForAll(static geometry => geometry is not null) => items
                .TraverseM(geometry => Requirement.Basic.Apply(context: Domain, value: geometry, cancel: CancellationToken.None).ToFin())
                .As(),
            _ => Fin.Fail<Seq<GeometryBase>>(error: Op.Of(name: nameof(Add)).InvalidInput()),
        }
        from ids in Mutate(document: document, name: nameof(Add), run: () =>
            values.TraverseM(geometry => attributes switch {
                ObjectAttributes attrs => DocumentTarget.IdResult(id: document.Objects.Add(geometry, attrs), op: Op.Of(name: nameof(Add))),
                _ => DocumentTarget.IdResult(id: document.Objects.Add(geometry), op: Op.Of(name: nameof(Add))),
            }).As())
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
        from document in Available(op: Op.Of(name: nameof(Replace)))
        from value in Optional(replacement).ToFin(Fail: Op.Of(name: nameof(Replace)).InvalidInput())
        from valid in Optional(target).ToFin(Fail: Op.Of(name: nameof(Replace)).InvalidInput())
        from result in Mutate(document: document, name: nameof(Replace), run: () => valid.Replace(document: document, replacement: value, ignoreModes: ignoreModes, op: Op.Of(name: nameof(Replace))))
        select result;

    public Fin<Unit> Delete(DocumentTarget target, bool quiet = true, bool ignoreModes = false) =>
        from document in Available(op: Op.Of(name: nameof(Delete)))
        from valid in Optional(target).ToFin(Fail: Op.Of(name: nameof(Delete)).InvalidInput())
        from result in Mutate(document: document, name: nameof(Delete), run: () => valid.Delete(document: document, quiet: quiet, ignoreModes: ignoreModes, op: Op.Of(name: nameof(Delete))))
        select result;

    public Fin<Seq<Guid>> Transform(DocumentTarget target, Transform transform, bool deleteOriginal = true) =>
        from document in Available(op: Op.Of(name: nameof(Transform)))
        from _ in guard(transform.IsValid, Op.Of(name: nameof(Transform)).InvalidInput())
        from valid in Optional(target).ToFin(Fail: Op.Of(name: nameof(Transform)).InvalidInput())
        from ids in Mutate(document: document, name: nameof(Transform), run: () => valid.Transform(document: document, transform: transform, deleteOriginal: deleteOriginal, op: Op.Of(name: nameof(Transform))))
        select ids;

    public Fin<int> Select(DocumentTarget target, bool selected = true, DocumentSelectionPolicy? policy = null) =>
        from document in Available(op: Op.Of(name: nameof(Select)))
        from valid in Optional(target).ToFin(Fail: Op.Of(name: nameof(Select)).InvalidInput())
        from result in valid.Select(document: document, selected: selected, policy: policy ?? DocumentSelectionPolicy.Default, op: Op.Of(name: nameof(Select)))
        select result;

    public Fin<int> SetSelection(DocumentTarget target, DocumentSelectionPolicy? policy = null) =>
        from document in Available(op: Op.Of(name: nameof(SetSelection))) from valid in Optional(target).ToFin(Fail: Op.Of(name: nameof(SetSelection)).InvalidInput()) from ids in valid.Ids(document: document, op: Op.Of(name: nameof(SetSelection)))
        from chosen in ids.Distinct() switch { Seq<Guid> values when !values.IsEmpty => Fin.Succ(value: values), _ => Fin.Fail<Seq<Guid>>(error: Op.Of(name: nameof(SetSelection)).InvalidInput()) }
        let active = policy ?? DocumentSelectionPolicy.Default
        from count in document.Objects.SetSelectedObjects(objectIds: chosen.AsIterable(), syncHighlight: active.Highlight, persistentSelect: active.Persistent, ignoreGripsState: active.IgnoreGrips, ignoreLayerLocking: active.IgnoreLayerLocking, ignoreLayerVisibility: active.IgnoreLayerVisibility) switch {
            int value when value >= 0 => Fin.Succ(value: value),
            _ => Fin.Fail<int>(error: Op.Of(name: nameof(SetSelection)).InvalidResult()),
        }
        select count;

    public Fin<int> Hide(DocumentTarget target, bool hidden = true) =>
        from document in Available(op: Op.Of(name: nameof(Hide)))
        from valid in Optional(target).ToFin(Fail: Op.Of(name: nameof(Hide)).InvalidInput())
        from ids in valid.Ids(document: document, op: Op.Of(name: nameof(Hide)))
        from count in Mutate(document: document, name: nameof(Hide), run: () => ApplyState(ids: ids, document: document, op: Op.Of(name: nameof(Hide)), ready: native => !hidden || !native.IsLocked, done: native => native.IsHidden == hidden, apply: id => hidden switch {
            true => document.Objects.Hide(objectId: id, ignoreLayerMode: true),
            false => document.Objects.Show(objectId: id, ignoreLayerMode: true),
        }))
        select count;

    public Fin<int> Lock(DocumentTarget target, bool locked = true) =>
        from document in Available(op: Op.Of(name: nameof(Lock)))
        from valid in Optional(target).ToFin(Fail: Op.Of(name: nameof(Lock)).InvalidInput())
        from ids in valid.Ids(document: document, op: Op.Of(name: nameof(Lock)))
        from count in Mutate(document: document, name: nameof(Lock), run: () => ApplyState(ids: ids, document: document, op: Op.Of(name: nameof(Lock)), ready: native => !locked || !native.IsHidden, done: native => native.IsLocked == locked, apply: id => locked switch {
            true => document.Objects.Lock(objectId: id, ignoreLayerMode: true),
            false => document.Objects.Unlock(objectId: id, ignoreLayerMode: true),
        }))
        select count;

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

    private static Fin<int> ApplyState(Seq<Guid> ids, RhinoDoc document, Op op, Func<RhinoObject, bool> ready, Func<RhinoObject, bool> done, Func<Guid, bool> apply) =>
        ids.TraverseM(id => Optional(document.Objects.FindId(id))
            .ToFin(Fail: op.InvalidResult())
            .Bind(native => (ready(arg: native), done(arg: native)) switch {
                (false, _) => Fin.Fail<bool>(error: op.InvalidInput()),
                (_, true) => Fin.Succ(value: false),
                _ => apply(arg: id) switch {
                    true => Fin.Succ(value: true),
                    false => Fin.Fail<bool>(error: op.InvalidResult()),
                },
            })).As().Map(static result => result.Filter(static changed => changed).Count);

}
