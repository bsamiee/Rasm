namespace Rasm.Rhino.Commands;

public readonly record struct DocumentRedraw(bool Enabled) {
    public static DocumentRedraw AfterCommit { get; } = new(Enabled: true);
    public static DocumentRedraw None { get; } = new(Enabled: false);

    internal Fin<Unit> Apply(DocumentEdit edit) =>
        Enabled switch {
            true => edit.Redraw(),
            false => Fin.Succ(value: unit),
        };
}

public sealed record DocumentTransaction(string Name, Seq<DocumentOp> Operations, DocumentRedraw Redraw) {
    public static DocumentTransaction One(string name, DocumentOp operation, DocumentRedraw? redraw = null) =>
        new(Name: name, Operations: Seq(operation), Redraw: redraw ?? DocumentRedraw.AfterCommit);
}

public readonly record struct DocumentReceipt(
    Seq<Guid> Created,
    Seq<Guid> Replaced,
    Seq<Guid> Deleted,
    Seq<Guid> Transformed,
    Seq<Guid> Selected,
    Seq<Guid> Hidden,
    Seq<Guid> Locked,
    Seq<Guid> AttributeChanged) {
    public static DocumentReceipt Empty { get; } = new(Created: Seq<Guid>(), Replaced: Seq<Guid>(), Deleted: Seq<Guid>(), Transformed: Seq<Guid>(), Selected: Seq<Guid>(), Hidden: Seq<Guid>(), Locked: Seq<Guid>(), AttributeChanged: Seq<Guid>());
    public static DocumentReceipt Add(DocumentReceipt left, DocumentReceipt right) =>
        new(Created: left.Created + right.Created, Replaced: left.Replaced + right.Replaced, Deleted: left.Deleted + right.Deleted, Transformed: left.Transformed + right.Transformed, Selected: left.Selected + right.Selected, Hidden: left.Hidden + right.Hidden, Locked: left.Locked + right.Locked, AttributeChanged: left.AttributeChanged + right.AttributeChanged);
    public static DocumentReceipt operator +(DocumentReceipt left, DocumentReceipt right) => Add(left: left, right: right);
}

public abstract record DocumentLifecycle {
    private DocumentLifecycle() { }

    internal abstract Fin<DocumentReceipt> Apply(DocumentTarget target, RhinoDoc document, Op op);

    public static DocumentLifecycle Delete(bool quiet = true, bool ignoreModes = false) => new DeleteAction(Quiet: quiet, IgnoreModes: ignoreModes);
    public static DocumentLifecycle Select(bool selected = true, DocumentSelectionPolicy? policy = null) => new SelectAction(Selected: selected, Policy: policy ?? DocumentSelectionPolicy.Default);
    public static DocumentLifecycle Hide(bool hidden = true) => new HideAction(Hidden: hidden);
    public static DocumentLifecycle Lock(bool locked = true) => new LockAction(Locked: locked);

    private sealed record DeleteAction(bool Quiet, bool IgnoreModes) : DocumentLifecycle {
        internal override Fin<DocumentReceipt> Apply(DocumentTarget target, RhinoDoc document, Op op) =>
            from valid in Optional(target).ToFin(Fail: op.InvalidInput())
            from ids in valid.Ids(document: document, op: op)
            from _ in valid.Delete(document: document, quiet: Quiet, ignoreModes: IgnoreModes, op: op)
            select DocumentReceipt.Empty with { Deleted = ids };
    }

    private sealed record SelectAction(bool Selected, DocumentSelectionPolicy Policy) : DocumentLifecycle {
        internal override Fin<DocumentReceipt> Apply(DocumentTarget target, RhinoDoc document, Op op) =>
            from valid in Optional(target).ToFin(Fail: op.InvalidInput())
            from ids in valid.Ids(document: document, op: op)
            from _ in valid.Select(document: document, selected: Selected, policy: Policy, op: op)
            select DocumentReceipt.Empty with { Selected = ids };
    }

    private sealed record HideAction(bool Hidden) : DocumentLifecycle {
        internal override Fin<DocumentReceipt> Apply(DocumentTarget target, RhinoDoc document, Op op) =>
            from valid in Optional(target).ToFin(Fail: op.InvalidInput())
            from ids in valid.Ids(document: document, op: op)
            from changed in DocumentEdit.ApplyState(ids: ids, document: document, op: op, ready: native => !Hidden || !native.IsLocked, done: native => native.IsHidden == Hidden, apply: id => Hidden switch { true => document.Objects.Hide(objectId: id, ignoreLayerMode: true), false => document.Objects.Show(objectId: id, ignoreLayerMode: true) })
            select DocumentReceipt.Empty with { Hidden = changed };
    }

    private sealed record LockAction(bool Locked) : DocumentLifecycle {
        internal override Fin<DocumentReceipt> Apply(DocumentTarget target, RhinoDoc document, Op op) =>
            from valid in Optional(target).ToFin(Fail: op.InvalidInput())
            from ids in valid.Ids(document: document, op: op)
            from changed in DocumentEdit.ApplyState(ids: ids, document: document, op: op, ready: native => !Locked || !native.IsHidden, done: native => native.IsLocked == Locked, apply: id => Locked switch { true => document.Objects.Lock(objectId: id, ignoreLayerMode: true), false => document.Objects.Unlock(objectId: id, ignoreLayerMode: true) })
            select DocumentReceipt.Empty with { Locked = changed };
    }
}

public abstract record DocumentOp {
    private DocumentOp() { }

    internal abstract Fin<DocumentReceipt> Apply(RhinoDoc document, Rasm.Domain.Context domain, Op op);

    public sealed record Create(IEnumerable<object> Sources, ObjectAttributes? Attributes = null) : DocumentOp {
        internal override Fin<DocumentReceipt> Apply(RhinoDoc document, Rasm.Domain.Context domain, Op op) =>
            from edit in Optional(document).ToFin(Fail: op.InvalidInput())
            from ids in DocumentEdit.AddRaw(document: edit, domain: domain, sources: Sources, attributes: Attributes, op: op)
            select DocumentReceipt.Empty with { Created = ids };
    }

    public sealed record Replace(DocumentTarget Target, object Replacement, bool IgnoreModes = false) : DocumentOp {
        internal override Fin<DocumentReceipt> Apply(RhinoDoc document, Rasm.Domain.Context domain, Op op) =>
            from target in Optional(Target).ToFin(Fail: op.InvalidInput())
            from value in Optional(Replacement).ToFin(Fail: op.InvalidInput())
            from ids in target.Ids(document: document, op: op)
            from _ in target.Replace(document: document, replacement: value, ignoreModes: IgnoreModes, op: op)
            select DocumentReceipt.Empty with { Replaced = ids };
    }

    public sealed record Transform(DocumentTarget Target, global::Rhino.Geometry.Transform Xform, bool DeleteOriginal = true) : DocumentOp {
        internal override Fin<DocumentReceipt> Apply(RhinoDoc document, Rasm.Domain.Context domain, Op op) =>
            from target in Optional(Target).ToFin(Fail: op.InvalidInput())
            from _ in guard(Xform.IsValid, op.InvalidInput())
            from ids in target.Transform(document: document, transform: Xform, deleteOriginal: DeleteOriginal, op: op)
            select DocumentReceipt.Empty with { Transformed = ids };
    }

    public sealed record AttributeChange(DocumentTarget Target, Func<ObjectAttributes, Fin<ObjectAttributes>> Change, bool Quiet = true) : DocumentOp {
        internal override Fin<DocumentReceipt> Apply(RhinoDoc document, Rasm.Domain.Context domain, Op op) =>
            from target in Optional(Target).ToFin(Fail: op.InvalidInput())
            from change in Optional(Change).ToFin(Fail: op.InvalidInput())
            from ids in target.Ids(document: document, op: op)
            from changed in ids.TraverseM(id =>
                from native in Optional(document.Objects.FindId(id)).ToFin(Fail: op.InvalidResult())
                from attributes in Optional(native.Attributes?.Duplicate()).ToFin(Fail: op.InvalidResult())
                from next in change(arg: attributes)
                from _ in document.Objects.ModifyAttributes(objectId: id, newAttributes: next, quiet: Quiet) switch { true => Fin.Succ(value: unit), false => Fin.Fail<Unit>(error: op.InvalidResult()) }
                select id).As()
            select DocumentReceipt.Empty with { AttributeChanged = changed };
    }

    public sealed record Lifecycle(DocumentTarget Target, DocumentLifecycle Action) : DocumentOp {
        internal override Fin<DocumentReceipt> Apply(RhinoDoc document, Rasm.Domain.Context domain, Op op) =>
            from target in Optional(Target).ToFin(Fail: op.InvalidInput())
            from action in Optional(Action).ToFin(Fail: op.InvalidInput())
            from receipt in action.Apply(target: target, document: document, op: op)
            select receipt;
    }
}
