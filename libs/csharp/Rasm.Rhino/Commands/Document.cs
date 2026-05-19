namespace Rasm.Rhino.Commands;

public abstract record DocumentTarget {
    private DocumentTarget() { }

    public static Fin<DocumentTarget> Selection(CommandSelection selection) => Optional(selection).ToFin(Fail: Op.Of(name: nameof(DocumentTarget)).InvalidInput()).Map(value => (DocumentTarget)new SelectionTarget(value));

    public static Fin<DocumentTarget> Reference(CommandSelection.Reference reference) => reference.ObjectId switch { Guid id when id != Guid.Empty => Fin.Succ<DocumentTarget>(value: new ReferenceTarget(reference)), _ => Fin.Fail<DocumentTarget>(error: Op.Of(name: nameof(DocumentTarget)).InvalidInput()) };

    public static Fin<DocumentTarget> Reference(ObjRef reference) => Optional(reference).ToFin(Fail: Op.Of(name: nameof(DocumentTarget)).InvalidInput()).Map(static value => CommandSelection.Reference.Of(reference: value, preselected: false)).Bind(Reference);

    public static Fin<DocumentTarget> Objects(IEnumerable<Guid> objectIds) => TargetIds(ids: objectIds, op: Op.Of(name: nameof(DocumentTarget))).Map(ids => (DocumentTarget)new ObjectsTarget(ids));

    internal Fin<int> Select(RhinoDoc document, bool selected, Op op) =>
        Use(document: document, op: op,
            selection: value => value.SelectInto(document: document, selected: selected, op: op),
            reference: value => value.Use(document: document, op: op, use: native => CountResult(count: document.Objects.Select(Seq(native).AsIterable(), selected), op: op)),
            objects: ids => CountResult(count: document.Objects.Select(ids, selected), op: op));

    internal Fin<Unit> Delete(RhinoDoc document, bool quiet, bool ignoreModes, Op op) =>
        Use(document: document, op: op,
            selection: value => DeleteIds(document: document, ids: value.ObjectIds, quiet: quiet, ignoreModes: ignoreModes, op: op),
            reference: value => value.Use(document: document, op: op, use: native => UnitResult(success: document.Objects.Delete(native, quiet, ignoreModes), op: op)),
            objects: ids => DeleteIds(document: document, ids: ids, quiet: quiet, ignoreModes: ignoreModes, op: op));

    internal Fin<Unit> Replace(RhinoDoc document, GeometryBase geometry, bool ignoreModes, Op op) =>
        Use(document: document, op: op,
            selection: value => ReplaceIds(document: document, ids: value.ObjectIds, geometry: geometry, ignoreModes: ignoreModes, op: op),
            reference: value => value.Use(document: document, op: op, use: native => UnitResult(success: document.Objects.Replace(native, geometry, ignoreModes), op: op)),
            objects: ids => ReplaceIds(document: document, ids: ids, geometry: geometry, ignoreModes: ignoreModes, op: op));

    internal abstract Fin<T> Use<T>(RhinoDoc document, Op op, Func<CommandSelection, Fin<T>> selection, Func<CommandSelection.Reference, Fin<T>> reference, Func<Seq<Guid>, Fin<T>> objects);

    private sealed record SelectionTarget(CommandSelection Value) : DocumentTarget {
        internal override Fin<T> Use<T>(RhinoDoc document, Op op, Func<CommandSelection, Fin<T>> selection, Func<CommandSelection.Reference, Fin<T>> reference, Func<Seq<Guid>, Fin<T>> objects) => (Value, ReferenceEquals(objA: Value?.Document, objB: document)) switch { (CommandSelection value, true) => selection(arg: value), _ => Fin.Fail<T>(error: op.InvalidInput()) };
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
            .Bind(values => toSeq(values).Filter(static id => id != Guid.Empty).Distinct() switch {
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
                true => toSeq(target.AsIterable()
                    .Select(id => document.Objects.FindId(id))
                    .Where(static item => item is { IsDeleted: false })) switch {
                        Seq<RhinoObject> found when found.Count == target.Count => found
                            .TraverseM(item => UnitResult(success: document.Objects.Delete(item, quiet, true), op: op))
                            .Map(static _ => unit),
                        _ => Fin.Fail<Unit>(error: op.InvalidResult()),
                    },
            });

    private static Fin<Unit> ReplaceIds(RhinoDoc document, IEnumerable<Guid> ids, GeometryBase geometry, bool ignoreModes, Op op) =>
        TargetIds(ids: ids, op: op).Bind(target => target.Count switch { 1 => UnitResult(success: document.Objects.Replace(objectId: target[0], geometry: geometry, ignoreModes: ignoreModes), op: op), _ => Fin.Fail<Unit>(error: op.InvalidInput()) });

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
    internal DocumentEdit(RhinoDoc document) {
        ArgumentNullException.ThrowIfNull(argument: document);
        Document = document;
    }

    public RhinoDoc Document { get; }

    public Fin<Guid> Add(GeometryBase geometry, ObjectAttributes? attributes = null) =>
        from document in Available(op: Op.Of(name: nameof(Add)))
        from g in Optional(geometry).ToFin(Fail: Op.Of(name: nameof(Add)).InvalidInput())
        from id in Mutate(document: document, name: nameof(Add), run: () => attributes switch {
            ObjectAttributes attrs => document.Objects.Add(g, attrs),
            _ => document.Objects.Add(g),
        } switch {
            Guid value when value != Guid.Empty => Fin.Succ(value: value),
            _ => Fin.Fail<Guid>(error: Op.Of(name: nameof(Add)).InvalidResult()),
        })
        select id;

    public Fin<Seq<Guid>> AddPoints(IEnumerable<Point3d> points, ObjectAttributes? attributes = null) =>
        from document in Available(op: Op.Of(name: nameof(AddPoints)))
        from values in Optional(points).ToFin(Fail: Op.Of(name: nameof(AddPoints)).InvalidInput())
        from ids in (Point3d[])[.. values.AsIterable()] switch {
            { Length: > 0 } native => Mutate(document: document, name: nameof(AddPoints), run: () => toSeq(attributes switch { ObjectAttributes attrs => document.Objects.AddPoints(native, attrs), _ => document.Objects.AddPoints(native) }) switch { Seq<Guid> result when result.Count == native.Length && result.ForAll(static id => id != Guid.Empty) => Fin.Succ(value: result), _ => Fin.Fail<Seq<Guid>>(error: Op.Of(name: nameof(AddPoints)).InvalidResult()) }),
            _ => Fin.Fail<Seq<Guid>>(error: Op.Of(name: nameof(AddPoints)).InvalidInput()),
        }
        select ids;

    public Fin<Unit> Replace(DocumentTarget target, GeometryBase geometry, bool ignoreModes = false) =>
        from document in Available(op: Op.Of(name: nameof(Replace)))
        from g in Optional(geometry).ToFin(Fail: Op.Of(name: nameof(Replace)).InvalidInput())
        from valid in Optional(target).ToFin(Fail: Op.Of(name: nameof(Replace)).InvalidInput())
        from result in Mutate(document: document, name: nameof(Replace), run: () => valid.Replace(document: document, geometry: g, ignoreModes: ignoreModes, op: Op.Of(name: nameof(Replace))))
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
        from ids in Mutate(document: document, name: nameof(Transform), run: () => valid.Use(document: document, op: Op.Of(name: nameof(Transform)), selection: selection => selection.Project(reference => reference.Use(document: document, op: Op.Of(name: nameof(Transform)), use: native => IdResult(id: document.Objects.Transform(objref: native, xform: transform, deleteOriginal: deleteOriginal), op: Op.Of(name: nameof(Transform))))), reference: reference => reference.Use(document: document, op: Op.Of(name: nameof(Transform)), use: native => IdResult(id: document.Objects.Transform(objref: native, xform: transform, deleteOriginal: deleteOriginal), op: Op.Of(name: nameof(Transform))).Map(static id => Seq(id))), objects: ids => ids.TraverseM(id => IdResult(id: document.Objects.Transform(objectId: id, xform: transform, deleteOriginal: deleteOriginal), op: Op.Of(name: nameof(Transform)))).As()))
        select ids;

    public Fin<int> Select(DocumentTarget target, bool selected = true) =>
        from document in Available(op: Op.Of(name: nameof(Select)))
        from valid in Optional(target).ToFin(Fail: Op.Of(name: nameof(Select)).InvalidInput())
        from result in valid.Select(document: document, selected: selected, op: Op.Of(name: nameof(Select)))
        select result;

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
            { IsAvailable: true, IsClosing: false } document => Fin.Succ(value: document),
            _ => Fin.Fail<RhinoDoc>(error: op.InvalidInput()),
        };

    private static Fin<T> Mutate<T>(RhinoDoc document, string name, Func<Fin<T>> run) => Optional(run).ToFin(Fail: Op.Of(name: name).InvalidInput()).Bind(valid => Rasm.Rhino.UI.RhinoUi.Protect(valid: () => { uint undo = document.UndoRecordingIsActive switch { true => 0u, false => document.BeginUndoRecord(description: name) }; try { return valid(); } finally { _ = undo > 0u && document.EndUndoRecord(undoRecordSerialNumber: undo); } }));

    private static Fin<Guid> IdResult(Guid id, Op op) => id switch { Guid value when value != Guid.Empty => Fin.Succ(value: value), _ => Fin.Fail<Guid>(error: op.InvalidResult()) };

}
