namespace Rasm.Rhino;

// --- [SERVICES] -------------------------------------------------------------------------
public sealed record DocumentEdit {
    public DocumentEdit(RhinoDoc document) {
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

    public Fin<Unit> Delete(ObjRef reference, bool quiet = true, bool ignoreModes = false) =>
        Optional(reference)
            .ToFin(Fail: Op.Of(name: nameof(Delete)).InvalidInput())
            .Bind(r => UnitResult(success: Document.Objects.Delete(r, quiet, ignoreModes), op: Op.Of(name: nameof(Delete))));

    public Fin<Unit> Delete(IEnumerable<Guid> ids, bool quiet = true) =>
        Optional(ids)
            .ToFin(Fail: Op.Of(name: nameof(Delete)).InvalidInput())
            .Bind(values => Document.Objects.Delete(values.AsIterable(), quiet) switch {
                > 0 => Fin.Succ(value: unit),
                _ => Fin.Fail<Unit>(error: Op.Of(name: nameof(Delete)).InvalidResult()),
            });

    public Fin<int> Select(CommandSelection selection, bool selected = true) =>
        Optional(selection)
            .ToFin(Fail: Op.Of(name: nameof(Select)).InvalidInput())
            .Bind(s => ReferenceEquals(objA: s.Document, objB: Document) switch {
                true => s.Items
                    .TraverseM(reference => reference.UseObjRef(
                        document: Document,
                        project: objRef => Fin.Succ(value: Document.Objects.Select(objref: objRef, select: selected) switch {
                            true => 1,
                            false => 0,
                        })))
                    .As()
                    .Map(static counts => counts.Fold(0, static (total, count) => total + count)),
                false => Fin.Fail<int>(error: Op.Of(name: nameof(Select)).InvalidInput()),
            });

    public Fin<int> Select(IEnumerable<Guid> ids, bool selected = true) =>
        Optional(ids)
            .ToFin(Fail: Op.Of(name: nameof(Select)).InvalidInput())
            .Bind(values => CountResult(count: Document.Objects.Select(values.AsIterable(), selected), op: Op.Of(name: nameof(Select))));

    public Fin<int> UnselectAll(bool ignorePersistentSelections = false) =>
        CountResult(count: Document.Objects.UnselectAll(ignorePersistentSelections: ignorePersistentSelections), op: Op.Of(name: nameof(UnselectAll)));

    public Fin<Unit> Redraw() {
        Document.Views.Redraw();
        return Fin.Succ(value: unit);
    }

    public static Fin<Unit> Redraw(RhinoView view) =>
        Optional(view)
            .ToFin(Fail: Op.Of(name: nameof(Redraw)).InvalidInput())
            .Map(v => {
                v.Redraw();
                return unit;
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
