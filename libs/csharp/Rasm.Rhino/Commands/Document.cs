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

    public static Fin<DocumentTarget> Objects(IEnumerable<Guid> objectIds) => TargetIds(ids: objectIds, op: Op.Of(name: nameof(DocumentTarget))).Map(ids => (DocumentTarget)new ObjectsTarget(ids));

    public static Fin<DocumentTarget> Filter(ObjectEnumeratorSettings settings) =>
        Optional(settings).ToFin(Fail: Op.Of(name: nameof(DocumentTarget)).InvalidInput()).Map(value => (DocumentTarget)new FilterTarget(value));

    public static Fin<DocumentTarget> Layer(int layerIndex) =>
        layerIndex switch {
            >= 0 => Filter(settings: new ObjectEnumeratorSettings { NormalObjects = true, LockedObjects = true, HiddenObjects = true, LayerIndexFilter = layerIndex }),
            _ => Fin.Fail<DocumentTarget>(error: Op.Of(name: nameof(Layer)).InvalidInput()),
        };

    public static Fin<DocumentTarget> UserString(string key, Option<string> value = default) =>
        DocumentEdit.NonBlank(value: key, op: Op.Of(name: nameof(UserString))).Map(valid => (DocumentTarget)new PredicateTarget(QuerySettings(), (_, native) =>
            Optional(native.Attributes?.GetUserString(key: valid)).Map(stored => value.Case switch {
                string expected => string.Equals(a: stored, b: expected, comparisonType: StringComparison.Ordinal),
                _ => !string.IsNullOrEmpty(value: stored),
            }).IfNone(false)));

    public static Fin<DocumentTarget> DrawColor(global::System.Drawing.Color color) =>
        color.IsEmpty switch {
            false => Fin.Succ<DocumentTarget>(value: new PredicateTarget(QuerySettings(), (document, native) => Optional(native.Attributes).Map(attributes => attributes.DrawColor(document: document) == color).IfNone(false))),
            true => Fin.Fail<DocumentTarget>(error: Op.Of(name: nameof(DrawColor)).InvalidInput()),
        };

    public static Fin<DocumentTarget> ClippingPlanes() =>
        Filter(settings: new ObjectEnumeratorSettings { NormalObjects = true, LockedObjects = true, HiddenObjects = true, ObjectTypeFilter = ObjectType.ClipPlane });

    public static Fin<DocumentTarget> Pick(CommandPickPolicy policy) =>
        Optional(policy).ToFin(Fail: Op.Of(name: nameof(Pick)).InvalidInput()).Map(value => (DocumentTarget)new PickTarget(value));

    internal Fin<int> Select(RhinoDoc document, bool selected, DocumentSelectionPolicy policy, Op op) =>
        Use(document: document, op: op,
            selection: value => value.SelectInto(document: document, selected: selected, policy: policy, op: op),
            reference: value => value.Use(document: document, op: op, use: native => DocumentEdit.UnitResult(success: document.Objects.Select(native, selected, policy.Highlight, policy.Persistent, policy.IgnoreGrips, policy.IgnoreLayerLocking, policy.IgnoreLayerVisibility), op: op).Map(static _ => 1)),
            objects: ids => CountResult(count: document.Objects.Select(ids.AsIterable(), selected, policy.Highlight, policy.Persistent, policy.IgnoreGrips, policy.IgnoreLayerLocking, policy.IgnoreLayerVisibility), expected: ids.Count, op: op));

    internal Fin<Unit> Delete(RhinoDoc document, bool quiet, bool ignoreModes, Op op) =>
        Use(document: document, op: op,
            selection: value => value.ObjectTargets.TraverseM(reference => reference.Use(document: document, op: op, use: native => DocumentEdit.UnitResult(success: document.Objects.Delete(native, quiet, ignoreModes), op: op))).As().Map(static _ => unit),
            reference: value => value.Use(document: document, op: op, use: native => DocumentEdit.UnitResult(success: document.Objects.Delete(native, quiet, ignoreModes), op: op)),
            objects: ids => DeleteIds(document: document, ids: ids, quiet: quiet, ignoreModes: ignoreModes, op: op));

    internal Fin<Unit> Replace(RhinoDoc document, object replacement, bool ignoreModes, Op op) =>
        Use(document: document, op: op,
            selection: value => value.Single().Bind(reference => reference.Use(document: document, op: op, use: native => ReplaceGeometry(replacement: replacement, op: op, use: geometry => document.Objects.Replace(objref: native, geometry: geometry, ignoreModes: ignoreModes)))),
            reference: value => value.Use(document: document, op: op, use: native => ReplaceGeometry(replacement: replacement, op: op, use: geometry => document.Objects.Replace(objref: native, geometry: geometry, ignoreModes: ignoreModes))),
            objects: ids => ReplaceIds(document: document, ids: ids, replacement: replacement, ignoreModes: ignoreModes, op: op));

    internal Fin<Seq<Guid>> Ids(RhinoDoc document, Op op) =>
        Use(document: document, op: op,
            selection: static value => Fin.Succ(value: value.MutationObjectIds),
            reference: value => value.Use(document: document, op: op, use: native => native.ObjectId switch {
                Guid id when id != Guid.Empty => Fin.Succ(value: Seq(id)),
                _ => Fin.Fail<Seq<Guid>>(error: op.InvalidResult()),
            }),
            objects: static ids => Fin.Succ(value: ids));

    internal Fin<Seq<(Guid Id, uint RuntimeSerialNumber)>> RuntimeTargets(RhinoDoc document, Op op) =>
        Use(document: document, op: op,
            selection: value => value.ObjectTargets.TraverseM(reference => reference.Use(document: document, op: op, use: _ => Fin.Succ(value: (Id: reference.MutationObjectId, reference.RuntimeSerialNumber)))).As(),
            reference: value => value.Use(document: document, op: op, use: _ => Fin.Succ(value: Seq((Id: value.MutationObjectId, value.RuntimeSerialNumber)))),
            objects: ids => ids.TraverseM(id => Optional(document.Objects.FindId(id)).ToFin(Fail: op.InvalidResult()).Map(native => (Id: id, native.RuntimeSerialNumber))).As());

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

    private sealed record PredicateTarget(ObjectEnumeratorSettings Settings, Func<RhinoDoc, RhinoObject, bool> Predicate) : DocumentTarget {
        internal override Fin<T> Use<T>(RhinoDoc document, Op op, Func<CommandSelection, Fin<T>> selection, Func<CommandSelection.Reference, Fin<T>> reference, Func<Seq<Guid>, Fin<T>> objects) =>
            from validDocument in Optional(document).ToFin(Fail: op.InvalidInput())
            from predicate in Optional(Predicate).ToFin(Fail: op.InvalidInput())
            from ids in toSeq(validDocument.Objects.GetObjectList(settings: Settings))
                .Filter(native => predicate(arg1: validDocument, arg2: native))
                .Map(static native => native.Id)
                .Distinct() switch {
                    Seq<Guid> values when !values.IsEmpty => Fin.Succ(value: values),
                    _ => Fin.Fail<Seq<Guid>>(error: op.InvalidResult()),
                }
            from result in objects(arg: ids)
            select result;
    }

    private sealed record PickTarget(CommandPickPolicy Policy) : DocumentTarget {
        internal override Fin<T> Use<T>(RhinoDoc document, Op op, Func<CommandSelection, Fin<T>> selection, Func<CommandSelection.Reference, Fin<T>> reference, Func<Seq<Guid>, Fin<T>> objects) =>
            from picked in CommandSelection.Pick(document: document, policy: Policy)
            from result in selection(arg: picked)
            select result;
    }

    private static ObjectEnumeratorSettings QuerySettings() =>
        new() { NormalObjects = true, LockedObjects = true, HiddenObjects = true };

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
                            false => DocumentEdit.UnitResult(success: document.Objects.Delete(native, quiet, true), op: op),
                            true => Fin.Fail<Unit>(error: op.InvalidResult()),
                        }))
                    .Map(static _ => unit),
            });

    private static Fin<Unit> ReplaceIds(RhinoDoc document, IEnumerable<Guid> ids, object replacement, bool ignoreModes, Op op) =>
        TargetIds(ids: ids, op: op).Bind(target => target.Count switch { 1 => ReplaceGeometry(replacement: replacement, op: op, use: geometry => document.Objects.Replace(objectId: target[0], geometry: geometry, ignoreModes: ignoreModes)), _ => Fin.Fail<Unit>(error: op.InvalidInput()) });

    internal static Fin<Guid> IdResult(Guid id, Op op) => id switch { Guid value when value != Guid.Empty => Fin.Succ(value: value), _ => Fin.Fail<Guid>(error: op.InvalidResult()) };

    private static Fin<Unit> ReplaceGeometry(object replacement, Op op, Func<GeometryBase, bool> use) =>
        from valid in Optional(use).ToFin(Fail: op.InvalidInput())
        from geometry in DocumentGeometry.Of(source: replacement)
        from result in geometry.Use(op: op, use: native => DocumentEdit.UnitResult(success: valid(arg: native), op: op))
        select result;

    private static Fin<int> CountResult(int count, int expected, Op op) =>
        count switch {
            int value when value == expected => Fin.Succ(value: value),
            _ => Fin.Fail<int>(error: op.InvalidResult()),
        };

}

public readonly record struct DocumentRedraw(bool Enabled, bool SuppressDuringCommit = false) {
    public static DocumentRedraw After { get; } = new(Enabled: true);
    public static DocumentRedraw None { get; } = new(Enabled: false);
}

public sealed record DocumentTransaction(
    string Name,
    Seq<DocumentOp> Operations,
    DocumentRedraw Redraw,
    Seq<DocumentCustomUndo> CustomUndo = default,
    bool UndoRecorded = true) {
    public static DocumentTransaction Batch(string name, params DocumentOp[] operations) =>
        new(Name: name, Operations: toSeq(operations), Redraw: DocumentRedraw.After);

    public DocumentTransaction WithoutUndo() =>
        this with { UndoRecorded = false };

    public DocumentTransaction WithRedraw(DocumentRedraw redraw) =>
        this with { Redraw = redraw };
}
public readonly record struct DocumentReceipt(Seq<Guid> Created, Seq<Guid> Replaced, Seq<Guid> Deleted, Seq<Guid> Transformed, Seq<Guid> Selected, Seq<Guid> Unselected, Seq<Guid> Hidden, Seq<Guid> Locked, Seq<Guid> Flashed, Seq<Guid> AttributeChanged, Seq<Guid> LifecycleChanged, Seq<DocumentResourceChange> ResourceChanged, Seq<uint> UndoRecords, Seq<string> CustomUndo) {
    public static DocumentReceipt Empty { get; } = new(Created: Seq<Guid>(), Replaced: Seq<Guid>(), Deleted: Seq<Guid>(), Transformed: Seq<Guid>(), Selected: Seq<Guid>(), Unselected: Seq<Guid>(), Hidden: Seq<Guid>(), Locked: Seq<Guid>(), Flashed: Seq<Guid>(), AttributeChanged: Seq<Guid>(), LifecycleChanged: Seq<Guid>(), ResourceChanged: Seq<DocumentResourceChange>(), UndoRecords: Seq<uint>(), CustomUndo: Seq<string>());
    public static DocumentReceipt operator +(DocumentReceipt left, DocumentReceipt right) =>
        Add(left: left, right: right);

    public static DocumentReceipt Add(DocumentReceipt left, DocumentReceipt right) =>
        new(Created: left.Created + right.Created, Replaced: left.Replaced + right.Replaced, Deleted: left.Deleted + right.Deleted, Transformed: left.Transformed + right.Transformed, Selected: left.Selected + right.Selected, Unselected: left.Unselected + right.Unselected, Hidden: left.Hidden + right.Hidden, Locked: left.Locked + right.Locked, Flashed: left.Flashed + right.Flashed, AttributeChanged: left.AttributeChanged + right.AttributeChanged, LifecycleChanged: left.LifecycleChanged + right.LifecycleChanged, ResourceChanged: left.ResourceChanged + right.ResourceChanged, UndoRecords: left.UndoRecords + right.UndoRecords, CustomUndo: left.CustomUndo + right.CustomUndo);

    public Rasm.Rhino.UI.UiStatus Status(string verb) {
        Seq<(string Name, int Count)> changes = Seq(("created", Created.Count), ("replaced", Replaced.Count), ("deleted", Deleted.Count), ("transformed", Transformed.Count), ("selected", Selected.Count), ("unselected", Unselected.Count), ("hidden", Hidden.Count), ("locked", Locked.Count), ("flashed", Flashed.Count), ("attributes", AttributeChanged.Count), ("lifecycle", LifecycleChanged.Count), ("resources", ResourceChanged.Count), ("undo", UndoRecords.Count), ("custom undo", CustomUndo.Count)).Filter(static change => change.Count > 0);
        return Rasm.Rhino.UI.UiStatus.Script(message: changes.IsEmpty switch {
            true => $"{verb}: no document changes",
            false => $"{verb}: {string.Join(separator: ", ", values: changes.Map(static change => $"{change.Name} {change.Count}").AsIterable())}",
        });
    }
}

public readonly record struct DocumentResourceChange(DocumentResourceKind Kind, string Name, Option<DocumentFileMode> FileMode = default);
public readonly record struct DocumentCustomUndo(string Name, EventHandler<global::Rhino.Commands.CustomUndoEventArgs> Undo, Option<object> Data = default) {
    internal Fin<string> Register(RhinoDoc document, Op op) {
        string label = Name;
        EventHandler<global::Rhino.Commands.CustomUndoEventArgs> handler = Undo;
        Option<object> data = Data;
        return from validDocument in Optional(document).ToFin(Fail: op.InvalidInput())
               from name in DocumentEdit.NonBlank(value: label, op: op)
               from undo in Optional(handler).ToFin(Fail: op.InvalidInput())
               from _ in Rasm.Rhino.UI.RhinoUi.Protect(valid: () => data.Case switch {
                   object tag => DocumentEdit.UnitResult(success: validDocument.AddCustomUndoEvent(description: name, handler: undo, tag: tag), op: op),
                   _ => DocumentEdit.UnitResult(success: validDocument.AddCustomUndoEvent(description: name, handler: undo), op: op),
               })
               select name;
    }
}
public enum DocumentResourceKind { Table, Block, View, File }
public enum DocumentLifecycle { Purge, Undelete }
public enum DocumentFileMode { Import, Export, ExportSelected, SaveAs }

public abstract record DocumentOp {
    private DocumentOp() { }
    internal abstract Fin<DocumentReceipt> Apply(RhinoDoc document, Rasm.Domain.Context domain, Op op);
    public sealed record Create(IEnumerable<object> Sources, ObjectAttributes? Attributes = null) : DocumentOp {
        internal override Fin<DocumentReceipt> Apply(RhinoDoc document, Rasm.Domain.Context domain, Op op) => from edit in Optional(document).ToFin(Fail: op.InvalidInput()) from ids in DocumentEdit.AddRaw(document: edit, domain: domain, sources: Sources, attributes: Attributes, op: op) select DocumentReceipt.Empty with { Created = ids };
    }

    public sealed record Replace(DocumentTarget Target, object Replacement, bool IgnoreModes = false) : DocumentOp {
        internal override Fin<DocumentReceipt> Apply(RhinoDoc document, Rasm.Domain.Context domain, Op op) => from target in Optional(Target).ToFin(Fail: op.InvalidInput()) from value in Optional(Replacement).ToFin(Fail: op.InvalidInput()) from ids in target.Ids(document: document, op: op) from _ in target.Replace(document: document, replacement: value, ignoreModes: IgnoreModes, op: op) select DocumentReceipt.Empty with { Replaced = ids };
    }

    public sealed record Delete(DocumentTarget Target, bool Quiet = true, bool IgnoreModes = false) : DocumentOp {
        internal override Fin<DocumentReceipt> Apply(RhinoDoc document, Rasm.Domain.Context domain, Op op) => from target in Optional(Target).ToFin(Fail: op.InvalidInput()) from ids in target.Ids(document: document, op: op) from _ in target.Delete(document: document, quiet: Quiet, ignoreModes: IgnoreModes, op: op) select DocumentReceipt.Empty with { Deleted = ids, LifecycleChanged = ids };
    }

    public sealed record Transform(DocumentTarget Target, global::Rhino.Geometry.Transform Xform, bool DeleteOriginal = true) : DocumentOp {
        internal override Fin<DocumentReceipt> Apply(RhinoDoc document, Rasm.Domain.Context domain, Op op) => from target in Optional(Target).ToFin(Fail: op.InvalidInput()) from _ in guard(Xform.IsValid, op.InvalidInput()) from originals in target.Ids(document: document, op: op) from ids in target.Transform(document: document, transform: Xform, deleteOriginal: DeleteOriginal, op: op) select DocumentReceipt.Empty with { Created = ids, Deleted = DeleteOriginal ? originals : Seq<Guid>(), Transformed = ids, LifecycleChanged = DeleteOriginal ? originals : Seq<Guid>() };
    }

    public sealed record AttributeChange(DocumentTarget Target, Func<ObjectAttributes, Fin<ObjectAttributes>> Change, bool Quiet = true) : DocumentOp {
        internal override Fin<DocumentReceipt> Apply(RhinoDoc document, Rasm.Domain.Context domain, Op op) =>
            from target in Optional(Target).ToFin(Fail: op.InvalidInput())
            from change in Optional(Change).ToFin(Fail: op.InvalidInput())
            from ids in target.Ids(document: document, op: op)
            from changed in ids.TraverseM(id => from native in Optional(document.Objects.FindId(id)).ToFin(Fail: op.InvalidResult()) from attributes in Optional(native.Attributes?.Duplicate()).ToFin(Fail: op.InvalidResult()) from next in change(arg: attributes) from _ in document.Objects.ModifyAttributes(objectId: id, newAttributes: next, quiet: Quiet) switch { true => Fin.Succ(value: unit), false => Fin.Fail<Unit>(error: op.InvalidResult()) } select id).As()
            select DocumentReceipt.Empty with { AttributeChanged = changed };
    }

    public sealed record SetSelection(DocumentTarget Target, DocumentSelectionPolicy Policy) : DocumentOp {
        internal override Fin<DocumentReceipt> Apply(RhinoDoc document, Rasm.Domain.Context domain, Op op) =>
            from target in Optional(Target).ToFin(Fail: op.InvalidInput())
            from ids in target.Ids(document: document, op: op)
            from chosen in ids.Distinct() switch { Seq<Guid> values when !values.IsEmpty => Fin.Succ(value: values), _ => Fin.Fail<Seq<Guid>>(error: op.InvalidInput()) }
            from before in DocumentEdit.SelectedIds(document: document, op: op)
            from count in document.Objects.SetSelectedObjects(objectIds: chosen.AsIterable(), syncHighlight: Policy.Highlight, persistentSelect: Policy.Persistent, ignoreGripsState: Policy.IgnoreGrips, ignoreLayerLocking: Policy.IgnoreLayerLocking, ignoreLayerVisibility: Policy.IgnoreLayerVisibility) switch { int value when value == chosen.Count => Fin.Succ(value: value), _ => Fin.Fail<int>(error: op.InvalidResult()) }
            from after in DocumentEdit.SelectedIds(document: document, op: op)
            select DocumentReceipt.Empty with {
                Selected = after.Filter(id => !before.Exists(item => item == id)),
                Unselected = before.Filter(id => !after.Exists(item => item == id)),
            };
    }

    public sealed record UnselectAll(bool IgnorePersistentSelections = false) : DocumentOp {
        internal override Fin<DocumentReceipt> Apply(RhinoDoc document, Rasm.Domain.Context domain, Op op) =>
            from before in DocumentEdit.SelectedIds(document: document, op: op)
            from count in document.Objects.UnselectAll(ignorePersistentSelections: IgnorePersistentSelections) switch { int value and >= 0 => Fin.Succ(value: value), _ => Fin.Fail<int>(error: op.InvalidResult()) }
            from after in DocumentEdit.SelectedIds(document: document, op: op)
            select DocumentReceipt.Empty with { Unselected = before.Filter(id => !after.Exists(item => item == id)) };
    }

    public sealed record ObjectState(
        DocumentTarget Target,
        Option<bool> Selected = default,
        Option<bool> Hidden = default,
        Option<bool> Locked = default,
        DocumentSelectionPolicy? SelectionPolicy = null) : DocumentOp {
        internal override Fin<DocumentReceipt> Apply(RhinoDoc document, Rasm.Domain.Context domain, Op op) =>
            from target in Optional(Target).ToFin(Fail: op.InvalidInput())
            from ids in target.Ids(document: document, op: op)
            from selection in Selected.Case switch {
                bool value => target.Select(document: document, selected: value, policy: SelectionPolicy ?? DocumentSelectionPolicy.Default, op: op).Map(_ => value switch {
                    true => (Selected: ids, Unselected: Seq<Guid>()),
                    false => (Selected: Seq<Guid>(), Unselected: ids),
                }),
                _ => Fin.Succ(value: (Selected: Seq<Guid>(), Unselected: Seq<Guid>())),
            }
            from hidden in Hidden.Case switch {
                bool value => DocumentEdit.ApplyState(ids: ids, document: document, op: op, ready: native => !value || !native.IsLocked, done: native => native.IsHidden == value, apply: id => value ? document.Objects.Hide(objectId: id, ignoreLayerMode: true) : document.Objects.Show(objectId: id, ignoreLayerMode: true)),
                _ => Fin.Succ(value: Seq<Guid>()),
            }
            from locked in Locked.Case switch {
                bool value => DocumentEdit.ApplyState(ids: ids, document: document, op: op, ready: native => !value || !native.IsHidden, done: native => native.IsLocked == value, apply: id => value ? document.Objects.Lock(objectId: id, ignoreLayerMode: true) : document.Objects.Unlock(objectId: id, ignoreLayerMode: true)),
                _ => Fin.Succ(value: Seq<Guid>()),
            }
            select DocumentReceipt.Empty with { Selected = selection.Selected, Unselected = selection.Unselected, Hidden = hidden, Locked = locked };
    }

    public sealed record Flash(DocumentTarget Target, bool UseSelectionColor = true) : DocumentOp {
        internal override bool RecordsUndo => false;

        internal override Fin<DocumentReceipt> Apply(RhinoDoc document, Rasm.Domain.Context domain, Op op) =>
            from target in Optional(Target).ToFin(Fail: op.InvalidInput())
            from ids in target.Ids(document: document, op: op)
            from objects in ids.TraverseM(id => Optional(document.Objects.FindId(id)).ToFin(Fail: op.InvalidResult())).As()
            from _ in Rasm.Rhino.UI.RhinoUi.Protect(valid: () => {
                document.Views.FlashObjects(list: objects.AsIterable(), useSelectionColor: UseSelectionColor);
                return Fin.Succ(value: unit);
            })
            select DocumentReceipt.Empty with { Flashed = ids };
    }

    public sealed record Lifecycle(DocumentTarget Target, DocumentLifecycle Change) : DocumentOp {
        internal override Fin<DocumentReceipt> Apply(RhinoDoc document, Rasm.Domain.Context domain, Op op) =>
            from target in Optional(Target).ToFin(Fail: op.InvalidInput())
            from targets in target.RuntimeTargets(document: document, op: op)
            from changed in targets.TraverseM(item => Change switch {
                DocumentLifecycle.Purge => Optional(document.Objects.Find(runtimeSerialNumber: item.RuntimeSerialNumber)).ToFin(Fail: op.InvalidResult()).Bind(native => document.Objects.Purge(native) switch { true => Fin.Succ(value: item.Id), false => Fin.Fail<Guid>(error: op.InvalidResult()) }),
                DocumentLifecycle.Undelete => document.Objects.Undelete(runtimeSerialNumber: item.RuntimeSerialNumber) switch { true => Fin.Succ(value: item.Id), false => Fin.Fail<Guid>(error: op.InvalidResult()) },
                _ => Fin.Fail<Guid>(error: op.InvalidInput()),
            }).As()
            select DocumentReceipt.Empty with { LifecycleChanged = changed };
    }

    public sealed record Resource(DocumentResourceKind Kind, string Name, Func<RhinoDoc, Fin<string>> Change) : DocumentOp {
        internal override Fin<DocumentReceipt> Apply(RhinoDoc document, Rasm.Domain.Context domain, Op op) =>
            from name in DocumentEdit.NonBlank(value: Name, op: op)
            from change in Optional(Change).ToFin(Fail: op.InvalidInput())
            from label in change(arg: document).Map(value => string.IsNullOrWhiteSpace(value: value) ? name : value)
            select DocumentReceipt.Empty with { ResourceChanged = Seq(new DocumentResourceChange(Kind: Kind, Name: label)) };
    }

    public sealed record File(string FilePath, DocumentFileMode Mode) : DocumentOp {
        internal override bool RecordsUndo => Mode == DocumentFileMode.Import;

        internal override Fin<DocumentReceipt> Apply(RhinoDoc document, Rasm.Domain.Context domain, Op op) =>
            DocumentEdit.NonBlank(value: FilePath, op: op).Bind((string path) =>
                Snapshot(document: document).Bind((Seq<Guid> created) =>
                    Write(document: document, path: path, op: op).Bind((Unit _) =>
                        Snapshot(document: document).Map((Seq<Guid> after) =>
                            DocumentReceipt.Empty with {
                                Created = after.Filter(id => !created.Exists(item => item == id)),
                                ResourceChanged = Seq(new DocumentResourceChange(Kind: DocumentResourceKind.File, Name: path, FileMode: Some(Mode))),
                            }))));

        private Fin<Seq<Guid>> Snapshot(RhinoDoc document) =>
            Mode == DocumentFileMode.Import ? DocumentEdit.LiveObjectIds(document: document) : Fin.Succ(value: Seq<Guid>());

        private Fin<Unit> Write(RhinoDoc document, string path, Op op) =>
            Mode switch {
                DocumentFileMode.Import => DocumentEdit.UnitResult(success: document.Import(filePath: path), op: op),
                DocumentFileMode.Export => DocumentEdit.UnitResult(success: document.Export(filePath: path), op: op),
                DocumentFileMode.ExportSelected => DocumentEdit.UnitResult(success: document.ExportSelected(filePath: path), op: op),
                DocumentFileMode.SaveAs => DocumentEdit.UnitResult(success: document.SaveAs(path), op: op),
                _ => Fin.Fail<Unit>(error: op.InvalidInput()),
            };
    }

    internal virtual bool RecordsUndo => true;
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
        from name in NonBlank(value: plan.Name, op: Op.Of(name: nameof(Commit)))
        from _ in guard(!plan.Operations.IsEmpty || !plan.CustomUndo.IsEmpty, Op.Of(name: nameof(Commit)).InvalidInput())
        from __ in guard(plan.UndoRecorded || plan.CustomUndo.IsEmpty, Op.Of(name: name).InvalidInput())
        from receipt in Mutate(document: document, name: name, recordsUndo: plan.UndoRecorded && (plan.Operations.Exists(static operation => operation.RecordsUndo) || !plan.CustomUndo.IsEmpty), suppressRedraw: plan.Redraw.SuppressDuringCommit, run: () =>
            from customUndo in plan.CustomUndo.TraverseM(undo => undo.Register(document: document, op: Op.Of(name: name))).As()
            from result in plan.Operations.TraverseM(operation => operation.Apply(document: document, domain: Domain, op: Op.Of(name: name))).As().Map(static receipts => receipts.Fold(DocumentReceipt.Empty, static (state, value) => state + value))
            select result with { CustomUndo = customUndo })
        from redraw in plan.Redraw.Enabled switch {
            true => Redraw(),
            false => Fin.Succ(value: unit),
        }
        select receipt;

    internal Fin<Unit> Redraw() =>
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

    private static Fin<DocumentReceipt> Mutate(RhinoDoc document, string name, bool recordsUndo, bool suppressRedraw, Func<Fin<DocumentReceipt>> run) =>
        Optional(run).ToFin(Fail: Op.Of(name: name).InvalidInput()).Bind(valid => Rasm.Rhino.UI.RhinoUi.Protect(valid: () => {
            uint undo = (recordsUndo, document.UndoRecordingIsActive) switch { (true, false) => document.BeginUndoRecord(description: name), _ => 0u };
            bool closed = true;
            Fin<DocumentReceipt> result = Fin.Fail<DocumentReceipt>(error: Op.Of(name: name).InvalidResult());
            _ = suppressRedraw switch {
                true => ((Func<Unit>)(() => { document.Views.EnableRedraw(enable: false, redrawDocument: false, redrawLayers: false); return unit; }))(),
                false => unit,
            };
            try {
                result = valid();
            } finally {
                _ = suppressRedraw switch {
                    true => ((Func<Unit>)(() => { document.Views.EnableRedraw(enable: true, redrawDocument: true, redrawLayers: true); return unit; }))(),
                    false => unit,
                };
                closed = undo switch {
                    > 0u => document.EndUndoRecord(undoRecordSerialNumber: undo),
                    _ => true,
                };
            }
            return result.Bind(value => closed switch {
                true => Fin.Succ(value: value with { UndoRecords = undo switch { > 0u => Seq(undo), _ => Seq<uint>() } }),
                false => Fin.Fail<DocumentReceipt>(error: Op.Of(name: name).InvalidResult()),
            });
        }));

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

    internal static Fin<Seq<Guid>> LiveObjectIds(RhinoDoc document) =>
        Optional(document).ToFin(Fail: Op.Of(name: nameof(LiveObjectIds)).InvalidInput()).Map(static value => toSeq(value.Objects.GetObjectIdList(settings: new ObjectEnumeratorSettings { NormalObjects = true, LockedObjects = true, HiddenObjects = true })).Distinct());

    internal static Fin<Seq<Guid>> SelectedIds(RhinoDoc document, Op op) =>
        Optional(document.Objects.GetSelectedObjects(includeLights: true, includeGrips: true)).ToFin(Fail: op.InvalidInput()).Map(static values => toSeq(values).Map(static native => native.Id).Distinct());

    internal static Fin<string> NonBlank(string value, Op op) =>
        string.IsNullOrWhiteSpace(value: value) switch {
            false => Fin.Succ(value: value),
            true => Fin.Fail<string>(error: op.InvalidInput()),
        };

    internal static Fin<Unit> UnitResult(bool success, Op op) =>
        success switch {
            true => Fin.Succ(value: unit),
            false => Fin.Fail<Unit>(error: op.InvalidResult()),
        };
}
