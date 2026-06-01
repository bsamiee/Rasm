namespace Rasm.Rhino.Commands;

// --- [TYPES] ------------------------------------------------------------------------------
public enum DocumentLifecycle { Purge, Undelete }

[Union(SwitchMapStateParameterName = "context")]
public abstract partial record DocumentOp {
    private DocumentOp() { }

    public sealed record Create(IEnumerable<object> Sources, ObjectAttributes? Attributes = null, HistoryRecord? History = null, bool Reference = false) : DocumentOp;
    public sealed record Replace(DocumentTarget Target, object Replacement, bool IgnoreModes = false) : DocumentOp;
    public sealed record Delete(DocumentTarget Target, bool Quiet = true, bool IgnoreModes = false) : DocumentOp;
    public sealed record Transform(DocumentTarget Target, global::Rhino.Geometry.Transform Xform, bool DeleteOriginal = true) : DocumentOp;
    public sealed record AttributeChange(DocumentTarget Target, Func<ObjectAttributes, Fin<ObjectAttributes>> Change, bool Quiet = true) : DocumentOp;
    public sealed record SetSelection(DocumentTarget Target, DocumentSelectionPolicy Policy) : DocumentOp;
    public sealed record UnselectAll(bool IgnorePersistentSelections = false) : DocumentOp;
    public sealed record ObjectState(
        DocumentTarget Target,
        Option<bool> Selected = default,
        Option<bool> Hidden = default,
        Option<bool> Locked = default,
        DocumentSelectionPolicy? SelectionPolicy = null) : DocumentOp;
    public sealed record Flash(DocumentTarget Target, bool UseSelectionColor = true) : DocumentOp;
    public sealed record Lifecycle(DocumentTarget Target, DocumentLifecycle Change) : DocumentOp;
    public sealed record Resource(DocumentResourceKind Kind, string Name, Func<RhinoDoc, Op, Fin<Unit>> Change) : DocumentOp;

    internal Fin<DocumentReceipt> Apply(RhinoDoc document, Context domain, Op op) =>
        Switch(
            (Document: document, Domain: domain, Op: op),
            create: static (ctx, edit) =>
                from doc in Optional(ctx.Document).ToFin(Fail: ctx.Op.InvalidInput())
                from ids in DocumentEdit.AddRaw(document: doc, domain: ctx.Domain, sources: edit.Sources, attributes: edit.Attributes, history: edit.History, reference: edit.Reference, op: ctx.Op)
                select DocumentReceipt.Objects(slot: DocumentReceiptSlot.Created, ids: ids)
                    + (edit.History switch {
                        HistoryRecord => DocumentReceipt.Resource(kind: DocumentResourceKind.HistoryRecord, name: nameof(Create)),
                        _ => DocumentReceipt.Empty,
                    }),
            replace: static (ctx, edit) => from target in Optional(edit.Target).ToFin(Fail: ctx.Op.InvalidInput())
                                           from value in Optional(edit.Replacement).ToFin(Fail: ctx.Op.InvalidInput())
                                           from ids in target.Ids(document: ctx.Document, op: ctx.Op)
                                           from _ in target.Replace(document: ctx.Document, replacement: value, ignoreModes: edit.IgnoreModes, op: ctx.Op)
                                           select DocumentReceipt.Objects(slot: DocumentReceiptSlot.Replaced, ids: ids),
            delete: static (ctx, edit) => from target in Optional(edit.Target).ToFin(Fail: ctx.Op.InvalidInput())
                                          from ids in target.Ids(document: ctx.Document, op: ctx.Op)
                                          from _ in target.Delete(document: ctx.Document, quiet: edit.Quiet, ignoreModes: edit.IgnoreModes, op: ctx.Op)
                                          select DocumentReceipt.Objects(slot: DocumentReceiptSlot.Deleted, ids: ids),
            transform: static (ctx, edit) => from target in Optional(edit.Target).ToFin(Fail: ctx.Op.InvalidInput())
                                             from _ in guard(edit.Xform.IsValid, ctx.Op.InvalidInput())
                                             from originals in target.Ids(document: ctx.Document, op: ctx.Op)
                                             from ids in target.Transform(document: ctx.Document, transform: edit.Xform, deleteOriginal: edit.DeleteOriginal, op: ctx.Op)
                                             select DocumentReceipt.Objects(groups: Seq(
                                                 (Slot: DocumentReceiptSlot.Created, Ids: edit.DeleteOriginal ? Seq<Guid>() : ids),
                                                 (Slot: DocumentReceiptSlot.Deleted, Ids: edit.DeleteOriginal ? originals : Seq<Guid>()),
                                                 (Slot: DocumentReceiptSlot.Transformed, Ids: originals),
                                                 (Slot: DocumentReceiptSlot.Lifecycle, Ids: edit.DeleteOriginal ? originals : Seq<Guid>()))),
            attributeChange: static (ctx, edit) =>
                from target in Optional(edit.Target).ToFin(Fail: ctx.Op.InvalidInput())
                from change in Optional(edit.Change).ToFin(Fail: ctx.Op.InvalidInput())
                from ids in target.Ids(document: ctx.Document, op: ctx.Op)
                from changed in ids.TraverseM(id =>
                    from native in Optional(ctx.Document.Objects.FindId(id)).ToFin(Fail: ctx.Op.InvalidResult())
                    from attributes in Optional(native.Attributes?.Duplicate()).ToFin(Fail: ctx.Op.InvalidResult())
                    from next in change(arg: attributes)
                    from _ in ctx.Op.Confirm(success: ctx.Document.Objects.ModifyAttributes(objectId: id, newAttributes: next, quiet: edit.Quiet))
                    select id).As()
                select DocumentReceipt.Objects(slot: DocumentReceiptSlot.Attributes, ids: changed),
            setSelection: static (ctx, edit) =>
                from target in Optional(edit.Target).ToFin(Fail: ctx.Op.InvalidInput())
                from ids in target.Ids(document: ctx.Document, op: ctx.Op).Map(static values => values.Distinct())
                from before in DocumentEdit.SelectedIds(document: ctx.Document, op: ctx.Op)
                from _ in ctx.Op.Confirm(success: edit.Policy.Select((highlight, persistent, ignoreGrips, ignoreLayerLocking, ignoreLayerVisibility) => ctx.Document.Objects.SetSelectedObjects(
                    objectIds: ids.AsIterable(),
                    syncHighlight: highlight,
                    persistentSelect: persistent,
                    ignoreGripsState: ignoreGrips,
                    ignoreLayerLocking: ignoreLayerLocking,
                    ignoreLayerVisibility: ignoreLayerVisibility)) == ids.Count)
                from after in DocumentEdit.SelectedIds(document: ctx.Document, op: ctx.Op)
                select DocumentReceipt.SelectionDelta(before: before, after: after),
            unselectAll: static (ctx, edit) =>
                from before in DocumentEdit.SelectedIds(document: ctx.Document, op: ctx.Op)
                from count in ctx.Document.Objects.UnselectAll(ignorePersistentSelections: edit.IgnorePersistentSelections) switch {
                    int value and >= 0 => Fin.Succ(value: value),
                    _ => Fin.Fail<int>(error: ctx.Op.InvalidResult()),
                }
                from after in DocumentEdit.SelectedIds(document: ctx.Document, op: ctx.Op)
                select DocumentReceipt.SelectionDelta(before: before, after: after),
            objectState: static (ctx, edit) =>
                from target in Optional(edit.Target).ToFin(Fail: ctx.Op.InvalidInput())
                from ids in target.Ids(document: ctx.Document, op: ctx.Op)
                from selection in edit.Selected.Case switch {
                    bool value => from before in DocumentEdit.SelectedIds(document: ctx.Document, op: ctx.Op)
                                  from _ in target.Select(document: ctx.Document, selected: value, policy: edit.SelectionPolicy ?? DocumentSelectionPolicy.Default, op: ctx.Op)
                                  from after in DocumentEdit.SelectedIds(document: ctx.Document, op: ctx.Op)
                                  select DocumentReceipt.SelectionDelta(before: before, after: after),
                    _ => Fin.Succ(value: DocumentReceipt.Empty),
                }
                from hidden in edit.Hidden.Case switch {
                    bool value => DocumentEdit.ApplyState(ids: ids, document: ctx.Document, op: ctx.Op,
                        ready: native => !value || !native.IsLocked,
                        done: native => native.IsHidden == value,
                        apply: id => value ? ctx.Document.Objects.Hide(objectId: id, ignoreLayerMode: true) : ctx.Document.Objects.Show(objectId: id, ignoreLayerMode: true)),
                    _ => Fin.Succ(value: Seq<Guid>()),
                }
                from locked in edit.Locked.Case switch {
                    bool value => DocumentEdit.ApplyState(ids: ids, document: ctx.Document, op: ctx.Op,
                        ready: native => !value || !native.IsHidden,
                        done: native => native.IsLocked == value,
                        apply: id => value ? ctx.Document.Objects.Lock(objectId: id, ignoreLayerMode: true) : ctx.Document.Objects.Unlock(objectId: id, ignoreLayerMode: true)),
                    _ => Fin.Succ(value: Seq<Guid>()),
                }
                select selection
                    + DocumentReceipt.Objects(slot: DocumentReceiptSlot.Hidden, ids: hidden)
                    + DocumentReceipt.Objects(slot: DocumentReceiptSlot.Locked, ids: locked),
            flash: static (ctx, edit) =>
                from target in Optional(edit.Target).ToFin(Fail: ctx.Op.InvalidInput())
                from ids in target.Ids(document: ctx.Document, op: ctx.Op)
                from objects in ids.TraverseM(id => Optional(ctx.Document.Objects.FindId(id)).ToFin(Fail: ctx.Op.InvalidResult())).As()
                from _ in UI.RhinoUi.Protect(valid: () => {
                    ctx.Document.Views.FlashObjects(list: objects.AsIterable(), useSelectionColor: edit.UseSelectionColor);
                    return Fin.Succ(value: unit);
                })
                select DocumentReceipt.Objects(slot: DocumentReceiptSlot.Flashed, ids: ids),
            lifecycle: static (ctx, edit) =>
                from target in Optional(edit.Target).ToFin(Fail: ctx.Op.InvalidInput())
                from targets in target.RuntimeTargets(document: ctx.Document, op: ctx.Op)
                from changed in targets.TraverseM(item => edit.Change switch {
                    DocumentLifecycle.Purge => Optional(ctx.Document.Objects.Find(runtimeSerialNumber: item.RuntimeSerialNumber)).ToFin(Fail: ctx.Op.InvalidResult()).Bind(native => ctx.Document.Objects.Purge(native) switch {
                        true => Fin.Succ(value: item.Id),
                        false => Fin.Fail<Guid>(error: ctx.Op.InvalidResult()),
                    }),
                    DocumentLifecycle.Undelete => ctx.Document.Objects.Undelete(runtimeSerialNumber: item.RuntimeSerialNumber) switch {
                        true => Fin.Succ(value: item.Id),
                        false => Fin.Fail<Guid>(error: ctx.Op.InvalidResult()),
                    },
                    _ => Fin.Fail<Guid>(error: ctx.Op.InvalidInput()),
                }).As()
                select DocumentReceipt.Objects(slot: DocumentReceiptSlot.Lifecycle, ids: changed),
            resource: static (ctx, edit) =>
                from kind in Optional(edit.Kind).ToFin(Fail: ctx.Op.InvalidInput())
                from name in DocumentEdit.NonBlank(value: edit.Name, op: ctx.Op)
                from change in Optional(edit.Change).ToFin(Fail: ctx.Op.InvalidInput())
                from _ in change(arg1: ctx.Document, arg2: ctx.Op)
                select DocumentReceipt.Resource(kind: kind, name: name));

    internal bool RecordsUndo => this is not Flash;
}

[SmartEnum<int>]
public sealed partial class DocumentResourceKind {
    public static readonly DocumentResourceKind Object = new(key: 0, componentType: ModelComponentType.ModelGeometry);
    public static readonly DocumentResourceKind Layer = new(key: 1, componentType: ModelComponentType.Layer);
    public static readonly DocumentResourceKind Material = new(key: 2, componentType: ModelComponentType.Material);
    public static readonly DocumentResourceKind Group = new(key: 3, componentType: ModelComponentType.Group);
    public static readonly DocumentResourceKind Block = new(key: 4, componentType: ModelComponentType.InstanceDefinition);
    public static readonly DocumentResourceKind View = new(key: 5, componentType: ModelComponentType.Unset);
    public static readonly DocumentResourceKind NamedView = new(key: 6, componentType: ModelComponentType.Unset);
    public static readonly DocumentResourceKind Layout = new(key: 7, componentType: ModelComponentType.Unset);
    public static readonly DocumentResourceKind NamedLayerState = new(key: 8, componentType: ModelComponentType.Unset);
    public static readonly DocumentResourceKind Linetype = new(key: 9, componentType: ModelComponentType.LinePattern);
    public static readonly DocumentResourceKind DimensionStyle = new(key: 10, componentType: ModelComponentType.DimStyle);
    public static readonly DocumentResourceKind Hatch = new(key: 11, componentType: ModelComponentType.HatchPattern);
    public static readonly DocumentResourceKind ConstructionPlane = new(key: 12, componentType: ModelComponentType.Unset);
    public static readonly DocumentResourceKind Image = new(key: 13, componentType: ModelComponentType.Image);
    public static readonly DocumentResourceKind TextureMapping = new(key: 14, componentType: ModelComponentType.TextureMapping);
    public static readonly DocumentResourceKind TextStyle = new(key: 15, componentType: ModelComponentType.TextStyle);
    public static readonly DocumentResourceKind RenderLight = new(key: 16, componentType: ModelComponentType.RenderLight);
    public static readonly DocumentResourceKind HistoryRecord = new(key: 17, componentType: ModelComponentType.HistoryRecord);
    public static readonly DocumentResourceKind SectionStyle = new(key: 18, componentType: ModelComponentType.SectionStyle);
    public static readonly DocumentResourceKind Markup = new(key: 19, componentType: ModelComponentType.Markup);
    public static readonly DocumentResourceKind PageViewGroup = new(key: 20, componentType: ModelComponentType.PageViewGroup);
    public static readonly DocumentResourceKind RenderContent = new(key: 21, componentType: ModelComponentType.RenderContent);
    public static readonly DocumentResourceKind RenderMaterial = new(key: 22, componentType: ModelComponentType.Unset);
    public static readonly DocumentResourceKind RenderEnvironment = new(key: 23, componentType: ModelComponentType.Unset);
    public static readonly DocumentResourceKind RenderTexture = new(key: 24, componentType: ModelComponentType.Unset);
    public static readonly DocumentResourceKind FileReference = new(key: 25, componentType: ModelComponentType.Unset);
    public static readonly DocumentResourceKind EmbeddedFile = new(key: 26, componentType: ModelComponentType.EmbeddedFile);
    public static readonly DocumentResourceKind Metadata = new(key: 27, componentType: ModelComponentType.Unset);
    public static readonly DocumentResourceKind Text = new(key: 28, componentType: ModelComponentType.Unset);
    public static readonly DocumentResourceKind EarthAnchor = new(key: 29, componentType: ModelComponentType.Unset);
    public static readonly DocumentResourceKind Sun = new(key: 30, componentType: ModelComponentType.Unset);
    public static readonly DocumentResourceKind NamedPosition = new(key: 31, componentType: ModelComponentType.Unset);

    public ModelComponentType ComponentType { get; }

    public static Option<DocumentResourceKind> ForComponentType(ModelComponentType type) =>
        type == ModelComponentType.Unset
            ? Option<DocumentResourceKind>.None
            : Items.AsIterable().Find(kind => kind.ComponentType == type);

    public DocumentResourceChange Change(string name) =>
        new(Kind: this, Name: name);
}

[Union]
public abstract partial record DocumentTarget {
    private DocumentTarget() { }

    public sealed record SelectionCase(CommandSelection Value) : DocumentTarget;
    public sealed record ReferenceCase(CommandSelection.Reference Value) : DocumentTarget;
    public sealed record ObjectsCase(Seq<Guid> Values) : DocumentTarget;
    public sealed record FilterCase(ObjectEnumeratorSettings Settings) : DocumentTarget;
    public sealed record PredicateCase(ObjectEnumeratorSettings Settings, Func<RhinoDoc, RhinoObject, bool> Predicate) : DocumentTarget;
    public sealed record PickCase(CommandPickPolicy Policy) : DocumentTarget;

    public static Fin<DocumentTarget> Selection(CommandSelection selection) =>
        Optional(selection).ToFin(Fail: Op.Of(name: nameof(DocumentTarget)).InvalidInput()).Map(value => (DocumentTarget)new SelectionCase(value));

    public static Fin<DocumentTarget> Reference(CommandSelection.Reference reference) =>
        reference.ObjectId switch {
            Guid id when id != Guid.Empty => Fin.Succ<DocumentTarget>(value: new ReferenceCase(reference)),
            _ => Fin.Fail<DocumentTarget>(error: Op.Of(name: nameof(DocumentTarget)).InvalidInput()),
        };

    public static Fin<DocumentTarget> Objects(IEnumerable<Guid> objectIds) =>
        TargetIds(ids: objectIds, op: Op.Of(name: nameof(DocumentTarget))).Map(ids => (DocumentTarget)new ObjectsCase(ids));

    public static Fin<DocumentTarget> Filter(ObjectEnumeratorSettings settings) =>
        Optional(settings).ToFin(Fail: Op.Of(name: nameof(DocumentTarget)).InvalidInput()).Map(value => (DocumentTarget)new FilterCase(value));

    public static Fin<DocumentTarget> Layer(int layerIndex) =>
        layerIndex switch {
            >= 0 => Filter(settings: QuerySettings(configure: s => s.LayerIndexFilter = layerIndex)),
            _ => Fin.Fail<DocumentTarget>(error: Op.Of(name: nameof(Layer)).InvalidInput()),
        };

    public static Fin<DocumentTarget> UserString(string key, Option<string> value = default) =>
        DocumentEdit.NonBlank(value: key, op: Op.Of(name: nameof(UserString))).Map(valid => (DocumentTarget)new PredicateCase(QuerySettings(), Attribute(test: (attributes, _) =>
            Optional(attributes.GetUserString(key: valid)).Map(stored => value.Case switch {
                string expected => string.Equals(a: stored, b: expected, comparisonType: StringComparison.Ordinal),
                _ => !string.IsNullOrEmpty(value: stored),
            }).IfNone(noneValue: false))));

    public static Fin<DocumentTarget> DrawColor(System.Drawing.Color color) =>
        guard(!color.IsEmpty, Op.Of(name: nameof(DrawColor)).InvalidInput()).ToFin()
            .Map(_ => (DocumentTarget)new PredicateCase(QuerySettings(), Attribute(test: (attributes, document) => attributes.DrawColor(document: document) == color)));

    public static Fin<DocumentTarget> Region(BoundingBox bounds, bool fullyInside = false, bool accurate = true) =>
        guard(bounds.IsValid, Op.Of(name: nameof(Region)).InvalidInput()).ToFin()
            .Map(_ => (DocumentTarget)new PredicateCase(QuerySettings(), (document, native) =>
                Optional(native.Geometry).Map(geometry => geometry.GetBoundingBox(accurate: accurate)).Filter(static box => box.IsValid).Map(box =>
                    fullyInside
                        ? Contains(region: bounds, box: box)
                        : BoundingBox.Intersection(a: bounds, b: box).IsValid).IfNone(noneValue: false)));

    public static Fin<DocumentTarget> ClippingPlanes() =>
        Filter(settings: QuerySettings(configure: s => s.ObjectTypeFilter = ObjectType.ClipPlane));

    public static Fin<DocumentTarget> Pick(CommandPickPolicy policy) =>
        Optional(policy).ToFin(Fail: Op.Of(name: nameof(Pick)).InvalidInput()).Map(value => (DocumentTarget)new PickCase(value));

    internal Fin<int> Select(RhinoDoc document, bool selected, DocumentSelectionPolicy policy, Op op) =>
        Resolve(document: document, op: op,
            selection: value => value.SelectInto(document: document, selected: selected, policy: policy, op: op),
            reference: value => value.Use(document: document, op: op, use: native => op.Confirm(success: policy.Select((highlight, persistent, ignoreGrips, ignoreLayerLocking, ignoreLayerVisibility) => document.Objects.Select(native, selected, highlight, persistent, ignoreGrips, ignoreLayerLocking, ignoreLayerVisibility))).Map(static _ => 1)),
            objects: ids => policy.Select((highlight, persistent, ignoreGrips, ignoreLayerLocking, ignoreLayerVisibility) => document.Objects.Select(ids.AsIterable(), selected, highlight, persistent, ignoreGrips, ignoreLayerLocking, ignoreLayerVisibility)) switch {
                int value when value == ids.Count => Fin.Succ(value: value),
                _ => Fin.Fail<int>(error: op.InvalidResult()),
            });

    internal Fin<Unit> Delete(RhinoDoc document, bool quiet, bool ignoreModes, Op op) =>
        Resolve(document: document, op: op,
            selection: value => value.ObjectTargets.TraverseM(reference => reference.Use(document: document, op: op, use: native => op.Confirm(success: document.Objects.Delete(native, quiet, ignoreModes)))).As().Map(static _ => unit),
            reference: value => value.Use(document: document, op: op, use: native => op.Confirm(success: document.Objects.Delete(native, quiet, ignoreModes))),
            objects: ids => DeleteIds(document: document, ids: ids, quiet: quiet, ignoreModes: ignoreModes, op: op));

    internal Fin<Unit> Replace(RhinoDoc document, object replacement, bool ignoreModes, Op op) =>
        Resolve(document: document, op: op,
            selection: value => value.Single().Bind(reference => reference.Use(document: document, op: op, use: native => ReplaceGeometry(replacement: replacement, op: op, use: geometry => document.Objects.Replace(objref: native, geometry: geometry, ignoreModes: ignoreModes)))),
            reference: value => value.Use(document: document, op: op, use: native => ReplaceGeometry(replacement: replacement, op: op, use: geometry => document.Objects.Replace(objref: native, geometry: geometry, ignoreModes: ignoreModes))),
            objects: ids => TargetIds(ids: ids, op: op).Bind(target => target.Count switch {
                1 => ReplaceGeometry(replacement: replacement, op: op, use: geometry => document.Objects.Replace(objectId: target[0], geometry: geometry, ignoreModes: ignoreModes)),
                _ => Fin.Fail<Unit>(error: op.InvalidInput()),
            }));

    internal Fin<Seq<Guid>> Ids(RhinoDoc document, Op op) =>
        Resolve(document: document, op: op,
            selection: static value => Fin.Succ(value: value.MutationObjectIds),
            reference: value => value.Use(document: document, op: op, use: native => native.ObjectId switch {
                Guid id when id != Guid.Empty => Fin.Succ(value: Seq(id)),
                _ => Fin.Fail<Seq<Guid>>(error: op.InvalidResult()),
            }),
            objects: static ids => Fin.Succ(value: ids));

    internal Fin<Seq<(Guid Id, uint RuntimeSerialNumber)>> RuntimeTargets(RhinoDoc document, Op op) =>
        Resolve(document: document, op: op,
            selection: value => value.ObjectTargets.TraverseM(reference => reference.Use(document: document, op: op, use: _ => Fin.Succ(value: (Id: reference.MutationObjectId, reference.RuntimeSerialNumber)))).As(),
            reference: value => value.Use(document: document, op: op, use: _ => Fin.Succ(value: Seq((Id: value.MutationObjectId, value.RuntimeSerialNumber)))),
            objects: ids => ids.TraverseM(id => Optional(document.Objects.FindId(id)).ToFin(Fail: op.InvalidResult()).Map(native => (Id: id, native.RuntimeSerialNumber))).As());

    internal Fin<Seq<Guid>> Transform(RhinoDoc document, Transform transform, bool deleteOriginal, Op op) =>
        Resolve(document: document, op: op,
            selection: selection => selection.ObjectTargets.TraverseM(reference => reference.Use(document: document, op: op, use: native => IdResult(id: document.Objects.Transform(objref: native, xform: transform, deleteOriginal: deleteOriginal), op: op))).As(),
            reference: reference => reference.Use(document: document, op: op, use: native => IdResult(id: document.Objects.Transform(objref: native, xform: transform, deleteOriginal: deleteOriginal), op: op).Map(static id => Seq(id))),
            objects: ids => ids.TraverseM(id => IdResult(id: document.Objects.Transform(objectId: id, xform: transform, deleteOriginal: deleteOriginal), op: op)).As());

    internal Fin<T> Resolve<T>(RhinoDoc document, Op op, Func<CommandSelection, Fin<T>> selection, Func<CommandSelection.Reference, Fin<T>> reference, Func<Seq<Guid>, Fin<T>> objects) =>
        Switch(
            (Doc: document, Op: op, S: selection, R: reference, O: objects),
            selectionCase: static (ctx, c) => (c.Value, ctx.Doc) switch {
                (CommandSelection value, RhinoDoc doc) when value.Document.RuntimeSerialNumber == doc.RuntimeSerialNumber => ctx.S(arg: value),
                _ => Fin.Fail<T>(error: ctx.Op.InvalidInput()),
            },
            referenceCase: static (ctx, c) => c.Value.Use(document: ctx.Doc, op: ctx.Op, use: _ => ctx.R(arg: c.Value)),
            objectsCase: static (ctx, c) => ctx.O(arg: c.Values),
            filterCase: static (ctx, c) => Optional(ctx.Doc).ToFin(Fail: ctx.Op.InvalidInput()).Bind(valid => toSeq(valid.Objects.GetObjectIdList(settings: c.Settings)).Distinct() switch {
                Seq<Guid> ids when !ids.IsEmpty => ctx.O(arg: ids),
                _ => Fin.Fail<T>(error: ctx.Op.InvalidResult()),
            }),
            predicateCase: static (ctx, c) =>
                from validDocument in Optional(ctx.Doc).ToFin(Fail: ctx.Op.InvalidInput())
                from predicate in Optional(c.Predicate).ToFin(Fail: ctx.Op.InvalidInput())
                from ids in toSeq(validDocument.Objects.GetObjectList(settings: c.Settings))
                    .Filter(native => predicate(arg1: validDocument, arg2: native))
                    .Map(static native => native.Id)
                    .Distinct() switch {
                        Seq<Guid> values when !values.IsEmpty => Fin.Succ(value: values),
                        _ => Fin.Fail<Seq<Guid>>(error: ctx.Op.InvalidResult()),
                    }
                from result in ctx.O(arg: ids)
                select result,
            pickCase: static (ctx, c) =>
                from picked in CommandSelection.Pick(document: ctx.Doc, policy: c.Policy)
                from result in ctx.S(arg: picked)
                select result);

    private static ObjectEnumeratorSettings QuerySettings(Action<ObjectEnumeratorSettings>? configure = null) {
        ObjectEnumeratorSettings settings = new() { NormalObjects = true, LockedObjects = true, HiddenObjects = true };
        configure?.Invoke(obj: settings);
        return settings;
    }

    private static Func<RhinoDoc, RhinoObject, bool> Attribute(Func<ObjectAttributes, RhinoDoc, bool> test) =>
        (document, native) => Optional(native.Attributes).Map(attributes => test(arg1: attributes, arg2: document)).IfNone(noneValue: false);

    private static bool Contains(BoundingBox region, BoundingBox box) =>
        box.Min.X >= region.Min.X && box.Min.Y >= region.Min.Y && box.Min.Z >= region.Min.Z
        && box.Max.X <= region.Max.X && box.Max.Y <= region.Max.Y && box.Max.Z <= region.Max.Z;

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
                false => op.Confirm(success: document.Objects.Delete(target.AsIterable(), quiet) == target.Count),
                true => target
                    .TraverseM(id => Optional(document.Objects.FindId(id))
                        .ToFin(Fail: op.InvalidResult())
                        .Bind(native => native.IsDeleted switch {
                            false => op.Confirm(success: document.Objects.Delete(native, quiet, ignoreModes: true)),
                            true => Fin.Fail<Unit>(error: op.InvalidResult()),
                        }))
                    .Map(static _ => unit),
            });

    internal static Fin<Guid> IdResult(Guid id, Op op) => op.AcceptValue(value: id);

    private static Fin<Unit> ReplaceGeometry(object replacement, Op op, Func<GeometryBase, bool> use) =>
        from valid in Optional(use).ToFin(Fail: op.InvalidInput())
        from geometry in GeometrySource.From(source: replacement)
        from result in geometry.Use(op: op, use: native => op.Confirm(success: valid(arg: native)))
        select result;
}

internal abstract record GeometrySource {
    private GeometrySource() { }
    internal abstract Fin<T> Use<T>(Op op, Func<GeometryBase, Fin<T>> use);

    internal static Fin<GeometrySource> From(object source) {
        Op op = Op.Of(name: nameof(GeometrySource));
        return Optional(source).ToFin(Fail: op.InvalidInput()).Bind(value => value switch {
            Surface surface when surface.IsValid => Fin.Succ<GeometrySource>(value: new Owned(() => surface.ToBrep())),
            GeometryBase geometry => Fin.Succ<GeometrySource>(value: new Borrowed(geometry)),
            Point3d point when point.IsValid => Fin.Succ<GeometrySource>(value: new Owned(() => new Point(location: point))),
            Line line when line.IsValid => Fin.Succ<GeometrySource>(value: new Owned(() => new LineCurve(line: line))),
            Circle circle when circle.IsValid => Fin.Succ<GeometrySource>(value: new Owned(() => new ArcCurve(circle: circle))),
            Arc arc when arc.IsValid => Fin.Succ<GeometrySource>(value: new Owned(() => new ArcCurve(arc: arc))),
            Ellipse ellipse when ellipse.IsValid => Fin.Succ<GeometrySource>(value: new Owned(() => ellipse.ToNurbsCurve())),
            Polyline polyline when polyline.IsValid => Fin.Succ<GeometrySource>(value: new Owned(() => new PolylineCurve(polyline))),
            Rectangle3d rect when rect.IsValid => Fin.Succ<GeometrySource>(value: new Owned(() => rect.ToNurbsCurve())),
            Box box when box.IsValid => Fin.Succ<GeometrySource>(value: new Owned(() => box.ToBrep())),
            BoundingBox bounds when bounds.IsValid => Fin.Succ<GeometrySource>(value: new Owned(() => bounds.ToBrep())),
            Sphere sphere when sphere.IsValid => Fin.Succ<GeometrySource>(value: new Owned(() => sphere.ToBrep())),
            Cylinder cyl when cyl.IsValid => Fin.Succ<GeometrySource>(value: new Owned(() => cyl.ToBrep(capBottom: true, capTop: true))),
            Cone cone when cone.IsValid => Fin.Succ<GeometrySource>(value: new Owned(() => cone.ToBrep(capBottom: true))),
            Torus torus when torus.IsValid => Fin.Succ<GeometrySource>(value: new Owned(() => torus.ToBrep())),
            _ => Fin.Fail<GeometrySource>(error: op.InvalidInput()),
        });
    }

    internal static GeometrySource Own(GeometryBase geometry) => new Owned(() => geometry);

    private sealed record Borrowed(GeometryBase Geometry) : GeometrySource {
        internal override Fin<T> Use<T>(Op op, Func<GeometryBase, Fin<T>> use) =>
            op.Catch(() => from geometry in Optional(Geometry).ToFin(Fail: op.InvalidInput())
                           from valid in Optional(use).ToFin(Fail: op.InvalidInput())
                           from result in valid(arg: geometry)
                           select result);
    }
    private sealed record Owned(Func<GeometryBase> Build) : GeometrySource {
        internal override Fin<T> Use<T>(Op op, Func<GeometryBase, Fin<T>> use) =>
            op.Catch(() => Optional(use).ToFin(Fail: op.InvalidInput()).Bind(valid => {
                using GeometryBase owned = Build();
                return valid(arg: owned);
            }));
    }
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct DocumentCustomUndo(string Name, EventHandler<CustomUndoEventArgs> Undo, Option<object> Data = default) {
    internal Fin<string> Register(RhinoDoc document, Op op) {
        string label = Name;
        EventHandler<CustomUndoEventArgs> handler = Undo;
        Option<object> data = Data;
        return from validDocument in Optional(document).ToFin(Fail: op.InvalidInput())
               from name in DocumentEdit.NonBlank(value: label, op: op)
               from undo in Optional(handler).ToFin(Fail: op.InvalidInput())
               from _ in UI.RhinoUi.Protect(valid: () => data.Case switch {
                   object tag => op.Confirm(success: validDocument.AddCustomUndoEvent(description: name, handler: undo, tag: tag)),
                   _ => op.Confirm(success: validDocument.AddCustomUndoEvent(description: name, handler: undo)),
               })
               select name;
    }
}

public readonly record struct DocumentResourceChange(DocumentResourceKind Kind, string Name);

[SmartEnum<int>]
public sealed partial class DocumentReceiptSlot {
    public static readonly DocumentReceiptSlot Created = new(key: 0, label: "created"), Replaced = new(key: 1, label: "replaced"), Deleted = new(key: 2, label: "deleted"), Transformed = new(key: 3, label: "transformed");
    public static readonly DocumentReceiptSlot Selected = new(key: 4, label: "selected"), Unselected = new(key: 5, label: "unselected"), Hidden = new(key: 6, label: "hidden"), Locked = new(key: 7, label: "locked"), Flashed = new(key: 8, label: "flashed");
    public static readonly DocumentReceiptSlot Attributes = new(key: 9, label: "attributes"), Lifecycle = new(key: 10, label: "lifecycle"), Resources = new(key: 11, label: "resources"), Undo = new(key: 12, label: "undo"), CustomUndo = new(key: 13, label: "custom undo");

    public string Label { get; }
    public bool TracksObjects => this != Resources && this != Undo && this != CustomUndo;
}

public readonly record struct DocumentReceipt {
    private readonly Seq<Change> changes;
    private Seq<Change> Changes => changes;

    private DocumentReceipt(Seq<Change> changes) =>
        this.changes = changes;

    public static DocumentReceipt Empty { get; } = new(changes: Seq<Change>());
    public static Seq<DocumentReceiptSlot> Slots { get; } = toSeq(DocumentReceiptSlot.Items);

    public Seq<Guid> Created => Ids(slot: DocumentReceiptSlot.Created);
    public Seq<Guid> Replaced => Ids(slot: DocumentReceiptSlot.Replaced);
    public Seq<Guid> Deleted => Ids(slot: DocumentReceiptSlot.Deleted);
    public Seq<Guid> Transformed => Ids(slot: DocumentReceiptSlot.Transformed);
    public Seq<Guid> Selected => Ids(slot: DocumentReceiptSlot.Selected);
    public Seq<Guid> Unselected => Ids(slot: DocumentReceiptSlot.Unselected);
    public Seq<Guid> Hidden => Ids(slot: DocumentReceiptSlot.Hidden);
    public Seq<Guid> Locked => Ids(slot: DocumentReceiptSlot.Locked);
    public Seq<Guid> Flashed => Ids(slot: DocumentReceiptSlot.Flashed);
    public Seq<Guid> AttributeChanged => Ids(slot: DocumentReceiptSlot.Attributes);
    public Seq<Guid> LifecycleChanged => Ids(slot: DocumentReceiptSlot.Lifecycle);
    public Seq<DocumentResourceChange> ResourceChanged => Changes.Choose(static change => change.ResourceChanged);
    public Seq<uint> UndoRecords => Changes.Choose(static change => change.UndoRecord);
    public Seq<string> CustomUndo => Changes.Choose(static change => change.CustomUndoName);

    public static DocumentReceipt operator +(DocumentReceipt left, DocumentReceipt right) =>
        new(changes: left.Changes + right.Changes);

    public static DocumentReceipt Objects(DocumentReceiptSlot slot, Seq<Guid> ids) =>
        From(changes: ids.Distinct().Choose(id => Change.Object(slot: slot, id: id)));
    public static DocumentReceipt Objects(Seq<(DocumentReceiptSlot Slot, Seq<Guid> Ids)> groups) =>
        From(changes: groups.Bind(static group =>
            group.Ids.Distinct().Choose(id => Change.Object(slot: group.Slot, id: id))));
    public static DocumentReceipt Objects(Seq<(DocumentReceiptSlot Slot, Seq<Guid> Ids)> groups, Seq<DocumentResourceChange> resources) =>
        Objects(groups: groups) + Resources(changes: resources);
    public static DocumentReceipt Objects(DocumentReceiptSlot slot, Seq<Guid> ids, Seq<DocumentResourceChange> resources) =>
        Objects(slot: slot, ids: ids) + Resources(changes: resources);
    public static DocumentReceipt Objects(DocumentReceiptSlot slot, Seq<Guid> ids, DocumentResourceKind kind, string name) =>
        Objects(slot: slot, ids: ids) + Resource(kind: kind, name: name);

    public static DocumentReceipt Resources(Seq<DocumentResourceChange> changes) =>
        From(changes: changes.Map(static change => Change.Resource(value: change)));

    public static DocumentReceipt Resource(DocumentResourceKind kind, string name) {
        ArgumentNullException.ThrowIfNull(argument: kind);
        return Resources(changes: Seq(kind.Change(name: name)));
    }

    public static DocumentReceipt UndoRecord(uint serial) =>
        serial > 0u ? From(changes: Seq(Change.Undo(serial: serial))) : Empty;

    public static DocumentReceipt CustomUndoRecords(Seq<string> names) =>
        From(changes: names.Map(static name => Change.CustomUndo(name: name)));

    internal static DocumentReceipt SelectionDelta(Seq<Guid> before, Seq<Guid> after) =>
        Objects(slot: DocumentReceiptSlot.Selected, ids: after.Filter(id => !before.Exists(item => item == id)))
        + Objects(slot: DocumentReceiptSlot.Unselected, ids: before.Filter(id => !after.Exists(item => item == id)));

    public Seq<Guid> Ids(DocumentReceiptSlot slot) =>
        Changes.Filter(change => change.Slot == slot).Choose(static change => change.ObjectId);

    public int Count(DocumentReceiptSlot slot) =>
        Changes.Count(change => change.Slot == slot);

    public UI.UiStatus Status(string verb) {
        DocumentReceipt self = this;
        Seq<(string Name, int Count)> rows = Slots.Map(slot => (Name: slot.Label, Count: self.Count(slot: slot))).Filter(static change => change.Count > 0);
        return UI.UiStatus.Script(message: rows.IsEmpty switch {
            true => $"{verb}: no document changes",
            false => $"{verb}: {string.Join(separator: ", ", values: rows.Map(static change => $"{change.Name} {change.Count}").AsIterable())}",
        });
    }

    private static DocumentReceipt From(Seq<Change> changes) =>
        new(changes: changes);

    private readonly record struct Change(DocumentReceiptSlot Slot, Option<Guid> ObjectId = default, Option<DocumentResourceChange> ResourceChanged = default, Option<uint> UndoRecord = default, Option<string> CustomUndoName = default) {
        internal static Option<Change> Object(DocumentReceiptSlot slot, Guid id) {
            ArgumentNullException.ThrowIfNull(argument: slot);
            return id != Guid.Empty && slot.TracksObjects
                ? Some(new Change(Slot: slot, ObjectId: id))
                : Option<Change>.None;
        }

        internal static Change Resource(DocumentResourceChange value) =>
            new(Slot: DocumentReceiptSlot.Resources, ResourceChanged: value);
        internal static Change Undo(uint serial) =>
            new(Slot: DocumentReceiptSlot.Undo, UndoRecord: serial);
        internal static Change CustomUndo(string name) =>
            new(Slot: DocumentReceiptSlot.CustomUndo, CustomUndoName: name);
    }
}

public readonly record struct DocumentRedraw(bool Enabled, bool SuppressDuringCommit = false) {
    public static DocumentRedraw After { get; } = new(Enabled: true);
    public static DocumentRedraw None { get; } = new(Enabled: false);
}

public readonly record struct DocumentSelectionPolicy(bool Highlight, bool IgnoreGrips, bool Persistent, bool IgnoreLayerLocking, bool IgnoreLayerVisibility) {
    public static DocumentSelectionPolicy Default { get; } = new(Highlight: true, IgnoreGrips: true, Persistent: true, IgnoreLayerLocking: false, IgnoreLayerVisibility: false);

    internal T Select<T>(Func<bool, bool, bool, bool, bool, T> native) =>
        native(arg1: Highlight, arg2: Persistent, arg3: IgnoreGrips, arg4: IgnoreLayerLocking, arg5: IgnoreLayerVisibility);
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

// --- [SERVICES] ---------------------------------------------------------------------------
public sealed record DocumentEdit {
    internal DocumentEdit(RhinoDoc document, Context domain) {
        ArgumentNullException.ThrowIfNull(argument: document);
        ArgumentNullException.ThrowIfNull(argument: domain);
        Document = document;
        Domain = domain;
    }

    public RhinoDoc Document { get; }
    public Context Domain { get; }

    public Fin<DocumentReceipt> Commit(DocumentTransaction transaction) {
        Op op = Op.Of(name: nameof(Commit));
        return from plan in Optional(transaction).ToFin(Fail: op.InvalidInput())
               from name in NonBlank(value: plan.Name, op: op)
               from _ in guard(!plan.Operations.IsEmpty || !plan.CustomUndo.IsEmpty, op.InvalidInput())
               from __ in guard(plan.UndoRecorded || plan.CustomUndo.IsEmpty, Op.Of(name: name).InvalidInput())
               from receipt in Commit(
                   name: name,
                   redraw: plan.Redraw,
                   undoRecorded: plan.UndoRecorded && (plan.Operations.Exists(static operation => operation.RecordsUndo) || !plan.CustomUndo.IsEmpty),
                   run: (document, domain, runOp) =>
                       from customUndo in plan.CustomUndo.TraverseM(undo => undo.Register(document: document, op: Op.Of(name: name))).As()
                       from result in plan.Operations.TraverseM(operation => operation.Apply(document: document, domain: domain, op: runOp)).As().Map(static receipts => receipts.Fold(DocumentReceipt.Empty, static (state, value) => state + value))
                       select result + DocumentReceipt.CustomUndoRecords(names: customUndo))
               select receipt;
    }

    internal Fin<DocumentReceipt> Commit(string name, DocumentRedraw redraw, bool undoRecorded, Func<RhinoDoc, Context, Op, Fin<DocumentReceipt>> run) {
        Op op = Op.Of(name: nameof(Commit));
        return from document in Available(op: op)
               from label in NonBlank(value: name, op: op)
               from active in Optional(run).ToFin(Fail: Op.Of(name: label).InvalidInput())
               from receipt in Mutate(document: document, name: label, recordsUndo: undoRecorded, suppressRedraw: redraw.SuppressDuringCommit, run: () => active(arg1: document, arg2: Domain, arg3: Op.Of(name: label)))
               from _ in redraw.Enabled switch {
                   true => Redraw(),
                   false => Fin.Succ(value: unit),
               }
               select receipt;
    }

    internal Fin<Unit> Redraw() =>
        Available(op: Op.Of(name: nameof(Redraw)))
            .Map(document => {
                document.Views.Redraw();
                return unit;
            });

    private Fin<RhinoDoc> Available(Op op) =>
        Document switch {
            { IsAvailable: true, IsClosing: false, IsInitializing: false, IsOpening: false, IsCreating: false } document => Fin.Succ(value: document),
            _ => Fin.Fail<RhinoDoc>(error: op.InvalidInput()),
        };

    private static Fin<DocumentReceipt> Mutate(RhinoDoc document, string name, bool recordsUndo, bool suppressRedraw, Func<Fin<DocumentReceipt>> run) =>
        Optional(run).ToFin(Fail: Op.Of(name: name).InvalidInput()).Bind(valid => UI.RhinoUi.Protect(valid: () => {
            Op op = Op.Of(name: name);
            uint undo = (recordsUndo, document.UndoRecordingIsActive) switch { (true, false) => document.BeginUndoRecord(description: name), _ => 0u };
            bool closed = true;
            bool priorRedraw = document.Views.RedrawEnabled;
            Fin<DocumentReceipt> result = Fin.Fail<DocumentReceipt>(error: op.InvalidResult());
            _ = suppressRedraw ? Redraw(document: document, enabled: false) : unit;
            try {
                result = valid();
            } finally {
                _ = suppressRedraw ? Redraw(document: document, enabled: priorRedraw) : unit;
                closed = undo switch { > 0u => document.EndUndoRecord(undoRecordSerialNumber: undo), _ => true };
            }
            return result.Bind(value => closed switch {
                true => Fin.Succ(value: value + DocumentReceipt.UndoRecord(serial: undo)),
                false => Fin.Fail<DocumentReceipt>(error: op.InvalidResult()),
            });
        }));

    private static Unit Redraw(RhinoDoc document, bool enabled) {
        document.Views.EnableRedraw(enable: enabled, redrawDocument: enabled, redrawLayers: enabled);
        return unit;
    }

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

    internal static Fin<Seq<Guid>> AddRaw(RhinoDoc document, Context domain, IEnumerable<object> sources, ObjectAttributes? attributes, HistoryRecord? history, bool reference, Op op) =>
        from validDocument in Optional(document).ToFin(Fail: op.InvalidInput())
        from validDomain in Optional(domain).ToFin(Fail: op.InvalidInput())
        from source in Optional(sources).ToFin(Fail: op.InvalidInput())
        from values in toSeq(source) switch {
            Seq<object> items when !items.IsEmpty => Fin.Succ(value: items),
            _ => Fin.Fail<Seq<object>>(error: op.InvalidInput()),
        }
        from ids in values.TraverseM(value =>
            from geometry in GeometrySource.From(source: value)
            from id in geometry.Use(op: op, use: native =>
                from _ in Requirement.Basic.Apply(context: validDomain, value: native, cancel: CancellationToken.None).ToFin()
                from created in DocumentTarget.IdResult(id: (attributes, history) switch {
                    (ObjectAttributes attrs, HistoryRecord record) => validDocument.Objects.Add(native, attrs, record, reference),
                    (ObjectAttributes attrs, _) => validDocument.Objects.Add(native, attrs),
                    _ => validDocument.Objects.Add(native),
                }, op: op)
                select created)
            select id).As()
        select ids;

    internal static Fin<Seq<Guid>> LiveObjectIds(RhinoDoc document) =>
        Optional(document).ToFin(Fail: Op.Of(name: nameof(LiveObjectIds)).InvalidInput()).Map(static value => toSeq(value.Objects.GetObjectIdList(settings: new ObjectEnumeratorSettings { NormalObjects = true, LockedObjects = true, HiddenObjects = true })).Distinct());

    internal static Fin<Seq<Guid>> SelectedIds(RhinoDoc document, Op op) =>
        Optional(document.Objects.GetSelectedObjects(includeLights: true, includeGrips: true)).ToFin(Fail: op.InvalidInput()).Map(static values => toSeq(values).Map(static native => native.Id).Distinct());

    internal static Fin<string> NonBlank(string value, Op op) =>
        op.AcceptText(value: value).MapFail(_ => op.InvalidInput());
}
