namespace Rasm.Rhino.Commands;

// --- [MODELS] ---------------------------------------------------------------------------
public abstract partial record DocumentTarget {
    private DocumentTarget() { }

    public static DocumentTarget Selection(CommandSelection selection) => new SelectionCase(Value: selection);
    public static DocumentTarget Reference(ObjRef reference) => new ReferenceCase(Value: reference);
    public static DocumentTarget Objects(IEnumerable<Guid> objectIds) => new ObjectsCase(Value: objectIds);

    internal abstract Fin<int> Select(RhinoDoc document, bool selected, Op op);
    internal abstract Fin<Unit> Delete(RhinoDoc document, bool quiet, bool ignoreModes, Op op);

    private sealed record SelectionCase(CommandSelection Value) : DocumentTarget {
        internal override Fin<int> Select(RhinoDoc document, bool selected, Op op) =>
            Optional(Value)
                .ToFin(Fail: op.InvalidInput())
                .Bind(selection => ReferenceEquals(objA: selection.Document, objB: document) switch {
                    true => selection.SelectInto(document: document, selected: selected, op: op),
                    false => Fin.Fail<int>(error: op.InvalidInput()),
                });

        internal override Fin<Unit> Delete(RhinoDoc document, bool quiet, bool ignoreModes, Op op) =>
            Optional(Value)
                .ToFin(Fail: op.InvalidInput())
                .Bind(selection => ObjectIds(document: document, ids: selection.ObjectIds, quiet: quiet, op: op));
    }

    private sealed record ReferenceCase(ObjRef Value) : DocumentTarget {
        internal override Fin<int> Select(RhinoDoc document, bool selected, Op op) =>
            Optional(Value)
                .ToFin(Fail: op.InvalidInput())
                .Bind(reference => document.Objects.Select(Seq(reference).AsIterable(), selected) switch {
                    int count and >= 0 => Fin.Succ(value: count),
                    _ => Fin.Fail<int>(error: op.InvalidResult()),
                });

        internal override Fin<Unit> Delete(RhinoDoc document, bool quiet, bool ignoreModes, Op op) =>
            Optional(Value)
                .ToFin(Fail: op.InvalidInput())
                .Bind(reference => document.Objects.Delete(reference, quiet, ignoreModes) switch {
                    true => Fin.Succ(value: unit),
                    false => Fin.Fail<Unit>(error: op.InvalidResult()),
                });
    }

    private sealed record ObjectsCase(IEnumerable<Guid> Value) : DocumentTarget {
        internal override Fin<int> Select(RhinoDoc document, bool selected, Op op) =>
            Optional(Value)
                .ToFin(Fail: op.InvalidInput())
                .Bind(ids => document.Objects.Select(ids, selected) switch {
                    int count and >= 0 => Fin.Succ(value: count),
                    _ => Fin.Fail<int>(error: op.InvalidResult()),
                });

        internal override Fin<Unit> Delete(RhinoDoc document, bool quiet, bool ignoreModes, Op op) =>
            ObjectIds(document: document, ids: Value, quiet: quiet, op: op);
    }

    private static Fin<Unit> ObjectIds(RhinoDoc document, IEnumerable<Guid> ids, bool quiet, Op op) =>
        Optional(ids)
            .ToFin(Fail: op.InvalidInput())
            .Bind(values => document.Objects.Delete(values, quiet) switch {
                > 0 => Fin.Succ(value: unit),
                _ => Fin.Fail<Unit>(error: op.InvalidResult()),
            });
}

// --- [SERVICES] -------------------------------------------------------------------------
public sealed record DocumentEdit {
    internal DocumentEdit(RhinoDoc document) {
        ArgumentNullException.ThrowIfNull(argument: document);
        Document = document;
    }

    public RhinoDoc Document { get; }

    public Fin<Guid> Add(GeometryBase geometry, ObjectAttributes? attributes = null) =>
        Optional(geometry)
            .ToFin(Fail: Op.Of(name: nameof(Add)).InvalidInput())
            .Bind(g => attributes switch {
                ObjectAttributes attrs => Document.Objects.Add(g, attrs),
                _ => Document.Objects.Add(g),
            } switch {
                Guid id when id != Guid.Empty => Fin.Succ(value: id),
                _ => Fin.Fail<Guid>(error: Op.Of(name: nameof(Add)).InvalidResult()),
            });

    public Fin<Seq<Guid>> AddPoints(IEnumerable<Point3d> points, ObjectAttributes? attributes = null) =>
        Optional(points)
            .ToFin(Fail: Op.Of(name: nameof(AddPoints)).InvalidInput())
            .Bind(values => (Point3d[])[.. values.AsIterable()] switch {
                { Length: > 0 } native => toSeq(attributes switch {
                    ObjectAttributes attrs => Document.Objects.AddPoints(native, attrs),
                    _ => Document.Objects.AddPoints(native),
                }) switch {
                    Seq<Guid> ids when !ids.IsEmpty && ids.ForAll(static id => id != Guid.Empty) => Fin.Succ(value: ids),
                    _ => Fin.Fail<Seq<Guid>>(error: Op.Of(name: nameof(AddPoints)).InvalidResult()),
                },
                _ => Fin.Fail<Seq<Guid>>(error: Op.Of(name: nameof(AddPoints)).InvalidInput()),
            });

    public Fin<Unit> Replace(ObjRef reference, GeometryBase geometry, bool ignoreModes = false) =>
        (Optional(reference).ToFin(Fail: Op.Of(name: nameof(Replace)).InvalidInput()),
         Optional(geometry).ToFin(Fail: Op.Of(name: nameof(Replace)).InvalidInput()))
            .Apply((r, g) => UnitResult(success: Document.Objects.Replace(r, g, ignoreModes), op: Op.Of(name: nameof(Replace))))
            .As()
            .Bind(static result => result);

    public Fin<Unit> Delete(DocumentTarget target, bool quiet = true, bool ignoreModes = false) =>
        Optional(target)
            .ToFin(Fail: Op.Of(name: nameof(Delete)).InvalidInput())
            .Bind(valid => valid.Delete(document: Document, quiet: quiet, ignoreModes: ignoreModes, op: Op.Of(name: nameof(Delete))));

    public Fin<int> Select(DocumentTarget target, bool selected = true) =>
        Optional(target)
            .ToFin(Fail: Op.Of(name: nameof(Select)).InvalidInput())
            .Bind(valid => valid.Select(document: Document, selected: selected, op: Op.Of(name: nameof(Select))));

    public Fin<int> UnselectAll(bool ignorePersistentSelections = false) =>
        CountResult(count: Document.Objects.UnselectAll(ignorePersistentSelections: ignorePersistentSelections), op: Op.Of(name: nameof(UnselectAll)));

    public Fin<Unit> Redraw() {
        Document.Views.Redraw();
        return Fin.Succ(value: unit);
    }

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
