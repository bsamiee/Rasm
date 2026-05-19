namespace Rasm.Rhino.Commands;

public sealed record DocumentTarget {
    private DocumentTarget(TargetKind kind, object value) => (Kind, Value) = (kind, value);

    private TargetKind Kind { get; }
    private object Value { get; }

    public static DocumentTarget Selection(CommandSelection selection) => new(kind: TargetKind.Selection, value: selection);
    public static DocumentTarget Reference(CommandSelection.Reference reference) =>
        new(kind: TargetKind.Reference, value: reference);
    public static DocumentTarget Reference(ObjRef reference) =>
        new(kind: TargetKind.Reference, value: Optional(reference)
            .Map(static value => CommandSelection.Reference.Of(reference: value, preselected: false))
            .Filter(static value => value.ObjectId != Guid.Empty)
            .IfNone(default(CommandSelection.Reference)));
    public static DocumentTarget Objects(IEnumerable<Guid> objectIds) =>
        new(kind: TargetKind.Objects, value: Optional(objectIds).Map(static ids => toSeq(ids)).IfNone(Seq<Guid>()));

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

    private Fin<T> Use<T>(RhinoDoc document, Op op, Func<CommandSelection, Fin<T>> selection, Func<CommandSelection.Reference, Fin<T>> reference, Func<Seq<Guid>, Fin<T>> objects) =>
        (Kind, Value) switch {
            (TargetKind.Selection, CommandSelection value) when ReferenceEquals(objA: value.Document, objB: document) => selection(arg: value),
            (TargetKind.Selection, CommandSelection) => Fin.Fail<T>(error: op.InvalidInput()),
            (TargetKind.Reference, CommandSelection.Reference value) when value.ObjectId != Guid.Empty => reference(arg: value),
            (TargetKind.Objects, Seq<Guid> ids) => TargetIds(ids: ids, op: op).Bind(objects),
            _ => Fin.Fail<T>(error: op.InvalidInput()),
        };

    private enum TargetKind { Selection, Reference, Objects }

    private static Fin<Seq<Guid>> TargetIds(IEnumerable<Guid> ids, Op op) =>
        Optional(ids)
            .ToFin(Fail: op.InvalidInput())
            .Bind(values => toSeq(values.Where(static id => id != Guid.Empty).Distinct()) switch {
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
        from id in attributes switch {
            ObjectAttributes attrs => document.Objects.Add(g, attrs),
            _ => document.Objects.Add(g),
        } switch {
            Guid value when value != Guid.Empty => Fin.Succ(value: value),
            _ => Fin.Fail<Guid>(error: Op.Of(name: nameof(Add)).InvalidResult()),
        }
        select id;

    public Fin<Seq<Guid>> AddPoints(IEnumerable<Point3d> points, ObjectAttributes? attributes = null) =>
        from document in Available(op: Op.Of(name: nameof(AddPoints)))
        from values in Optional(points).ToFin(Fail: Op.Of(name: nameof(AddPoints)).InvalidInput())
        from ids in (Point3d[])[.. values.AsIterable()] switch {
            { Length: > 0 } native => toSeq(attributes switch {
                ObjectAttributes attrs => document.Objects.AddPoints(native, attrs),
                _ => document.Objects.AddPoints(native),
            }) switch {
                Seq<Guid> result when result.Count == native.Length && result.ForAll(static id => id != Guid.Empty) => Fin.Succ(value: result),
                _ => Fin.Fail<Seq<Guid>>(error: Op.Of(name: nameof(AddPoints)).InvalidResult()),
            },
            _ => Fin.Fail<Seq<Guid>>(error: Op.Of(name: nameof(AddPoints)).InvalidInput()),
        }
        select ids;

    public Fin<Unit> Replace(CommandSelection.Reference reference, GeometryBase geometry, bool ignoreModes = false) =>
        from document in Available(op: Op.Of(name: nameof(Replace)))
        from g in Optional(geometry).ToFin(Fail: Op.Of(name: nameof(Replace)).InvalidInput())
        from result in reference.Use(document: document, op: Op.Of(name: nameof(Replace)), use: native => document.Objects.Replace(native, g, ignoreModes) switch {
            true => Fin.Succ(value: unit),
            false => Fin.Fail<Unit>(error: Op.Of(name: nameof(Replace)).InvalidResult()),
        })
        select result;

    public Fin<Unit> Delete(DocumentTarget target, bool quiet = true, bool ignoreModes = false) =>
        from document in Available(op: Op.Of(name: nameof(Delete)))
        from valid in Optional(target).ToFin(Fail: Op.Of(name: nameof(Delete)).InvalidInput())
        from result in valid.Delete(document: document, quiet: quiet, ignoreModes: ignoreModes, op: Op.Of(name: nameof(Delete)))
        select result;

    public Fin<int> Select(DocumentTarget target, bool selected = true) =>
        from document in Available(op: Op.Of(name: nameof(Select)))
        from valid in Optional(target).ToFin(Fail: Op.Of(name: nameof(Select)).InvalidInput())
        from result in valid.Select(document: document, selected: selected, op: Op.Of(name: nameof(Select)))
        select result;

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
}
