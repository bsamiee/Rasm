namespace Rasm.Rhino.Commands;

// --- [MODELS] ---------------------------------------------------------------------------
public abstract partial record DocumentTarget {
    private DocumentTarget() { }

    public static DocumentTarget Selection(CommandSelection selection) => new SelectionCase(Value: selection);
    public static DocumentTarget Reference(ObjRef reference) =>
        new ReferenceCase(Value: Optional(reference).Map(static value => CommandSelection.Reference.Of(reference: value, preselected: false)).IfNone(default(CommandSelection.Reference)));
    public static DocumentTarget Objects(IEnumerable<Guid> objectIds) =>
        new ObjectsCase(Value: Optional(objectIds).Map(static ids => toSeq(ids)).IfNone(Seq<Guid>()));

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
                .Bind(selection => ReferenceEquals(objA: selection.Document, objB: document) switch {
                    true => DeleteIds(document: document, ids: selection.ObjectIds, quiet: quiet, ignoreModes: ignoreModes, op: op),
                    false => Fin.Fail<Unit>(error: op.InvalidInput()),
                });
    }

    private sealed record ReferenceCase(CommandSelection.Reference Value) : DocumentTarget {
        internal override Fin<int> Select(RhinoDoc document, bool selected, Op op) {
            return Value.ObjectId switch {
                Guid id when id != Guid.Empty => WithReference(document: document, use: reference => CountResult(count: document.Objects.Select(Seq(reference).AsIterable(), selected), op: op)),
                _ => Fin.Fail<int>(error: op.InvalidInput()),
            };
        }

        internal override Fin<Unit> Delete(RhinoDoc document, bool quiet, bool ignoreModes, Op op) {
            return Value.ObjectId switch {
                Guid id when id != Guid.Empty => WithReference(document: document, use: reference => UnitResult(success: document.Objects.Delete(reference, quiet, ignoreModes), op: op)),
                _ => Fin.Fail<Unit>(error: op.InvalidInput()),
            };
        }

        private Fin<T> WithReference<T>(RhinoDoc document, Func<ObjRef, Fin<T>> use) {
            using ObjRef reference = Value.ObjRef(document: document);
            return use(arg: reference);
        }
    }

    private sealed record ObjectsCase(Seq<Guid> Value) : DocumentTarget {
        internal override Fin<int> Select(RhinoDoc document, bool selected, Op op) =>
            TargetIds(ids: Value, op: op)
                .Bind(ids => CountResult(count: document.Objects.Select(ids, selected), op: op));

        internal override Fin<Unit> Delete(RhinoDoc document, bool quiet, bool ignoreModes, Op op) =>
            DeleteIds(document: document, ids: Value, quiet: quiet, ignoreModes: ignoreModes, op: op);
    }

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

// --- [SERVICES] -------------------------------------------------------------------------
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

    public Fin<Unit> Replace(ObjRef reference, GeometryBase geometry, bool ignoreModes = false) =>
        from document in Available(op: Op.Of(name: nameof(Replace)))
        from r in Optional(reference).ToFin(Fail: Op.Of(name: nameof(Replace)).InvalidInput())
        from g in Optional(geometry).ToFin(Fail: Op.Of(name: nameof(Replace)).InvalidInput())
        from result in document.Objects.Replace(r, g, ignoreModes) switch {
            true => Fin.Succ(value: unit),
            false => Fin.Fail<Unit>(error: Op.Of(name: nameof(Replace)).InvalidResult()),
        }
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
