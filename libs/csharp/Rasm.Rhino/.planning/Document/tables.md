# [RASM_RHINO_TABLES]

`Rasm.Rhino.Document` owns document-table vocabulary, object addressing, mutation programs, undo/redraw bracketing, and consequence evidence. `TableKind` captures each admitted host document table, `TableTarget` freezes explicit, runtime, and host-query addressing, and `TableOp` closes the mutation family. `Tables.Commit` executes one admitted program inside one session capability window, refreshes the kernel `Context`, seals every undo exit, compensates redraw state on every outcome, and returns one typed fact stream.

## [01]-[INDEX]

- [02]-[TABLE_VOCABULARY]: `TableKind` — document-table identity, component correspondence, and reclamation behavior.
- [03]-[TARGET_ALGEBRA]: `ObjectRuntime`, `BoundsMatch`, `TablePredicate`, and `TableTarget` — immutable object addressing and query composition.
- [04]-[TRANSACTION_RAIL]: mutation policy rows, `NamedRestore`, `HistoryRoll`, `TableOp`, `TableTransaction`, `GeometryIntake`, and `Tables`.
- [05]-[RECEIPTS]: `TableSlot`, `TableFact`, and `TableReceipt` — one runtime-addressable consequence stream.
- [06]-[SURFACE_LEDGER]: the page owner map.

## [02]-[TABLE_VOCABULARY]

- Owner: `TableKind` `[SmartEnum<int>]` binds each admitted document table to its `ModelComponentType` and table-owned reclamation delegate.
- Entry: `ForComponentType(ModelComponentType) : Seq<TableKind>` returns every mapped row, expands `ModelComponentType.Mixed` across every explicit correspondence, and treats `ModelComponentType.Unset` as absent correspondence. `Reclaim(RhinoDoc, Op) : Fin<int>` invokes the row delegate and rejects a table with no host reclamation member.
- Law: table behavior resides on the row. A table extension declares component correspondence and reclamation behavior at construction, so no external dictionary or accessibility flag can drift from the vocabulary.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Threading;
using Rasm.Domain;
using Rasm.Numerics;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.Geometry;

namespace Rasm.Rhino.Document;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class TableKind {
    public static readonly TableKind Objects = new(key: 0, componentType: ModelComponentType.ModelGeometry, reclaim: NoReclaim);
    public static readonly TableKind Manifest = new(key: 1, componentType: ModelComponentType.Mixed, reclaim: NoReclaim);
    public static readonly TableKind Bitmaps = new(key: 2, componentType: ModelComponentType.Image, reclaim: NoReclaim);
    public static readonly TableKind Materials = new(key: 3, componentType: ModelComponentType.Material, reclaim: NoReclaim);
    public static readonly TableKind Linetypes = new(key: 4, componentType: ModelComponentType.LinePattern, reclaim: static (document, op) => Count(document.Linetypes.PurgeUnused(), op));
    public static readonly TableKind Layers = new(key: 5, componentType: ModelComponentType.Layer, reclaim: static (document, op) => Count(document.Layers.PurgeUnused(), op));
    public static readonly TableKind Groups = new(key: 6, componentType: ModelComponentType.Group, reclaim: static (document, op) => Count(document.Groups.PurgeUnused(), op));
    public static readonly TableKind DimStyles = new(key: 7, componentType: ModelComponentType.DimStyle, reclaim: static (document, op) => Count(document.DimStyles.PurgeUnused(), op));
    public static readonly TableKind Lights = new(key: 8, componentType: ModelComponentType.RenderLight, reclaim: NoReclaim);
    public static readonly TableKind HatchPatterns = new(key: 9, componentType: ModelComponentType.HatchPattern, reclaim: static (document, op) => Count(document.HatchPatterns.PurgeUnused(), op));
    public static readonly TableKind Views = new(key: 10, componentType: ModelComponentType.Unset, reclaim: NoReclaim);
    public static readonly TableKind NamedViews = new(key: 11, componentType: ModelComponentType.Unset, reclaim: NoReclaim);
    public static readonly TableKind InstanceDefinitions = new(key: 12, componentType: ModelComponentType.InstanceDefinition, reclaim: static (document, op) => Count(document.InstanceDefinitions.PurgeUnused(), op));
    public static readonly TableKind NamedConstructionPlanes = new(key: 13, componentType: ModelComponentType.Unset, reclaim: NoReclaim);
    public static readonly TableKind NamedPositions = new(key: 14, componentType: ModelComponentType.Unset, reclaim: NoReclaim);
    public static readonly TableKind NamedLayerStates = new(key: 15, componentType: ModelComponentType.Unset, reclaim: NoReclaim);
    public static readonly TableKind Snapshots = new(key: 16, componentType: ModelComponentType.Unset, reclaim: NoReclaim);
    public static readonly TableKind Strings = new(key: 17, componentType: ModelComponentType.Unset, reclaim: NoReclaim);
    public static readonly TableKind RuntimeData = new(key: 18, componentType: ModelComponentType.Unset, reclaim: NoReclaim);
    public static readonly TableKind SectionStyles = new(key: 19, componentType: ModelComponentType.SectionStyle, reclaim: NoReclaim);
    public static readonly TableKind Markups = new(key: 20, componentType: ModelComponentType.Markup, reclaim: NoReclaim);
    public static readonly TableKind PageViewGroups = new(key: 21, componentType: ModelComponentType.PageViewGroup, reclaim: NoReclaim);
    public static readonly TableKind RenderMaterials = new(key: 22, componentType: ModelComponentType.RenderContent, reclaim: NoReclaim);
    public static readonly TableKind RenderEnvironments = new(key: 23, componentType: ModelComponentType.RenderContent, reclaim: NoReclaim);
    public static readonly TableKind RenderTextures = new(key: 24, componentType: ModelComponentType.RenderContent, reclaim: NoReclaim);

    public ModelComponentType ComponentType { get; }

    [UseDelegateFromConstructor]
    internal partial Fin<int> Reclaim(RhinoDoc document, Op key);

    public static Seq<TableKind> ForComponentType(ModelComponentType type) =>
        type switch {
            ModelComponentType.Unset => Seq<TableKind>(),
            ModelComponentType.Mixed => Items.AsIterable()
                .Filter(static kind => kind.ComponentType is not ModelComponentType.Unset and not ModelComponentType.Mixed)
                .ToSeq(),
            _ => Items.AsIterable().Filter(kind => kind.ComponentType == type).ToSeq(),
        };

    private static Fin<int> Count(int value, Op key) =>
        guard(value >= 0, key.InvalidResult()).ToFin().Map(_ => value);

    private static Fin<int> NoReclaim(RhinoDoc document, Op key) =>
        Fin.Fail<int>(error: key.Unsupported(geometryType: typeof(TableKind), outputType: typeof(int)));
}
```

## [03]-[TARGET_ALGEBRA]

- Owner: `ObjectRuntime` admits the durable `(Guid, runtime serial)` pair required after an object leaves the active-id index. `ObjectQuery` freezes the complete `ObjectEnumeratorSettings` product and replaces its live `ViewportFilter` slot with the stable `ViewportTarget` owner. `TableTarget` `[Union]` closes explicit ids, runtime pairs, and admitted queries. `TablePredicate` `[Union]` adds composable tag, draw-color, and kernel-bounds predicates; `BoundsMatch` owns containment versus intersection behavior as rows.
- Entry: `ObjectQuery.Of(ObjectEnumeratorSettings)`, `TableTarget.Of(params ReadOnlySpan<Guid>)`, `Deleted(params ReadOnlySpan<ObjectRuntime>)`, and `Query(ObjectQuery, params ReadOnlySpan<TablePredicate>)` are the only constructors. `Resolve` returns distinct ids; `Serials` preserves or resolves runtime pairs. A deleted-object lifecycle request composes `Deleted` from a prior receipt instead of attempting `FindId`, which cannot find deleted objects.
- Law: query settings are copied at admission and rebuilt at execution. Mutable caller settings never change an admitted target, the viewport resolves from stable identity inside the document callback, and every public host filter field remains available. Predicate evaluation rides `Fin`; invalid geometry fails the query instead of disappearing from the result.
- Law: bounds predicates compose the kernel `BoundsOf` owner. Inflation remains host-query policy, while geometry classification, coercion, and bounds validity stay kernel-owned.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
public sealed record ObjectRuntime {
    private ObjectRuntime(Guid id, uint serial) {
        Id = id;
        Serial = serial;
    }

    public Guid Id { get; }
    public uint Serial { get; }

    internal static Fin<ObjectRuntime> Of(Guid id, uint serial, Op? key = null) {
        Op op = key.OrDefault();
        return guard(id != Guid.Empty && serial > 0u, op.InvalidInput()).ToFin()
            .Map(_ => new ObjectRuntime(id: id, serial: serial));
    }
}

public sealed class ObjectQuery {
    private readonly ObjectEnumeratorSettings settings;
    private readonly Option<Viewport.ViewportTarget> viewport;

    private ObjectQuery(ObjectEnumeratorSettings settings, Option<Viewport.ViewportTarget> viewport) {
        this.settings = settings;
        this.viewport = viewport;
    }

    internal bool SelectsOnlyDeleted => settings.DeletedObjects
        && !settings.NormalObjects
        && !settings.LockedObjects
        && !settings.HiddenObjects
        && !settings.IdefObjects;

    public static Fin<ObjectQuery> Of(ObjectEnumeratorSettings settings, Option<Viewport.ViewportTarget> viewport = default, Op? key = null) {
        Op op = key.OrDefault();
        return from source in Optional(settings).ToFin(Fail: op.InvalidInput())
               from _ in guard(source.ViewportFilter is null, op.InvalidInput()).ToFin()
               from target in viewport.Traverse(item => Optional(item).ToFin(Fail: op.InvalidInput())).As()
               select new ObjectQuery(settings: Copy(source: source, viewport: null), viewport: target);
    }

    internal Fin<ObjectEnumeratorSettings> Build(RhinoDoc document, Op key) =>
        viewport.Match(
            Some: target =>
                from rows in target.Resolve(document: document, key: key)
                from row in Tables.One(rows: rows, key: key)
                select Copy(source: settings, viewport: row.Viewport),
            None: () => Fin.Succ(value: Copy(source: settings, viewport: null)));

    private static ObjectEnumeratorSettings Copy(ObjectEnumeratorSettings source, RhinoViewport? viewport) =>
        new() {
            NormalObjects = source.NormalObjects,
            LockedObjects = source.LockedObjects,
            HiddenObjects = source.HiddenObjects,
            IdefObjects = source.IdefObjects,
            DeletedObjects = source.DeletedObjects,
            SubObjectSelected = source.SubObjectSelected,
            ActiveObjects = source.ActiveObjects,
            ReferenceObjects = source.ReferenceObjects,
            IncludeLights = source.IncludeLights,
            IncludeGrips = source.IncludeGrips,
            IncludePhantoms = source.IncludePhantoms,
            UseFastSelection = source.UseFastSelection,
            SelectedObjectsFilter = source.SelectedObjectsFilter,
            VisibleFilter = source.VisibleFilter,
            ObjectTypeFilter = source.ObjectTypeFilter,
            ClassTypeFilter = source.ClassTypeFilter,
            LayerIndexFilter = source.LayerIndexFilter,
            MaterialIndexFilter = source.MaterialIndexFilter,
            NameFilter = source.NameFilter,
            ViewportFilter = viewport,
            SpaceFilter = source.SpaceFilter,
        };
}

[SmartEnum]
public sealed partial class BoundsMatch {
    public static readonly BoundsMatch Intersects = new(static (region, candidate) => BoundingBox.Intersection(a: region, b: candidate).IsValid);
    public static readonly BoundsMatch Contains = new(static (region, candidate) => region.Contains(box: candidate));

    [UseDelegateFromConstructor]
    internal partial bool Test(BoundingBox region, BoundingBox candidate);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TablePredicate {
    private TablePredicate() { }

    private sealed record TagCase(string Key, Option<string> Expected) : TablePredicate;
    private sealed record ColorCase(System.Drawing.Color Value) : TablePredicate;
    private sealed record BoundsCase(BoundingBox Region, BoundsMatch Match) : TablePredicate;

    public static Fin<TablePredicate> Tag(string key, Option<string> expected = default) =>
        Op.Of().AcceptText(value: key).Map(valid => (TablePredicate)new TagCase(Key: valid, Expected: expected));

    public static Fin<TablePredicate> Color(System.Drawing.Color value) =>
        guard(!value.IsEmpty, Op.Of().InvalidInput()).ToFin()
            .Map(_ => (TablePredicate)new ColorCase(Value: value));

    public static Fin<TablePredicate> Bounds(BoundingBox region, BoundsMatch match, double inflation = 0.0) {
        Op op = Op.Of();
        return from relation in Optional(match).ToFin(Fail: op.InvalidInput())
               from _ in guard(region.IsValid && double.IsFinite(inflation) && inflation >= 0.0, op.InvalidInput()).ToFin()
               select (TablePredicate)new BoundsCase(Region: Inflated(region: region, amount: inflation), Match: relation);
    }

    internal Fin<bool> Match(RhinoDoc document, RhinoObject native, Op key) =>
        Switch(
            state: (Document: document, Native: native, Op: key),
            tagCase: static (context, predicate) => Fin.Succ(value: Optional(context.Native.Attributes)
                .Bind(attributes => Optional(attributes.GetUserString(key: predicate.Key)))
                .Map(stored => predicate.Expected.Map(expected => string.Equals(a: stored, b: expected, comparisonType: StringComparison.Ordinal)).IfNone(noneValue: true))
                .IfNone(noneValue: false)),
            colorCase: static (context, predicate) => Fin.Succ(value: Optional(context.Native.Attributes)
                .Map(attributes => attributes.DrawColor(document: context.Document) == predicate.Value)
                .IfNone(noneValue: false)),
            boundsCase: static (context, predicate) => Optional(context.Native.Geometry)
                .ToFin(Fail: context.Op.InvalidResult())
                .Bind(geometry => geometry.BoundsOf(key: context.Op))
                .Map(candidate => predicate.Match.Test(region: predicate.Region, candidate: candidate)));

    private static BoundingBox Inflated(BoundingBox region, double amount) {
        BoundingBox expanded = region;
        _ = Op.SideWhen(amount > 0.0, () => expanded.Inflate(xAmount: amount, yAmount: amount, zAmount: amount));
        return expanded;
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TableTarget {
    private TableTarget() { }

    private sealed record IdsCase(Seq<Guid> Values) : TableTarget;
    private sealed record RuntimeCase(Seq<ObjectRuntime> Values) : TableTarget;
    private sealed record QueryCase(ObjectQuery Query, Seq<TablePredicate> Predicates) : TableTarget;

    public static Fin<TableTarget> Of(params ReadOnlySpan<Guid> ids) {
        Op op = Op.Of();
        return toSeq(ids.ToArray())
            .TraverseM(id => id != Guid.Empty ? Fin.Succ(value: id) : Fin.Fail<Guid>(error: op.InvalidInput()))
            .As()
            .Map(static values => (TableTarget)new IdsCase(Values: values.Distinct()));
    }

    public static Fin<TableTarget> Deleted(params ReadOnlySpan<ObjectRuntime> values) {
        Op op = Op.Of();
        return toSeq(values.ToArray())
            .TraverseM(value => Optional(value).ToFin(Fail: op.InvalidInput()))
            .As()
            .Map(static admitted => (TableTarget)new RuntimeCase(Values: admitted.DistinctBy(static value => value.Serial)));
    }

    public static Fin<TableTarget> Query(ObjectQuery query, params ReadOnlySpan<TablePredicate> predicates) {
        Op op = Op.Of();
        return from source in Optional(query).ToFin(Fail: op.InvalidInput())
               from filters in toSeq(predicates.ToArray()).TraverseM(predicate => Optional(predicate).ToFin(Fail: op.InvalidInput())).As()
               select (TableTarget)new QueryCase(Query: source, Predicates: filters);
    }

    internal bool RetainsRuntime => Switch(
        idsCase: static _ => false,
        runtimeCase: static _ => true,
        queryCase: static target => target.Query.SelectsOnlyDeleted);

    internal Fin<Seq<Guid>> Resolve(RhinoDoc document, Op key) =>
        Switch(
            state: (Document: document, Op: key),
            idsCase: static (_, target) => Fin.Succ(value: target.Values),
            runtimeCase: static (_, target) => Fin.Succ(value: target.Values.Map(static value => value.Id)),
            queryCase: static (context, target) => Evaluate(
                    target: target,
                    document: context.Document,
                    key: context.Op)
                .Map(static rows => rows.Map(static native => native.Id).Distinct()));

    internal Fin<Seq<ObjectRuntime>> Serials(RhinoDoc document, Op key) =>
        Switch(
            state: (Document: document, Op: key),
            idsCase: static (context, target) => Tables.Runtime(document: context.Document, ids: target.Values, key: context.Op),
            runtimeCase: static (_, target) => Fin.Succ(value: target.Values),
            queryCase: static (context, target) => Evaluate(
                    target: target,
                    document: context.Document,
                    key: context.Op)
                .Bind(rows => rows.TraverseM(native => ObjectRuntime.Of(
                    id: native.Id,
                    serial: native.RuntimeSerialNumber,
                    key: context.Op)).As()));

    private static Fin<Seq<RhinoObject>> Evaluate(QueryCase target, RhinoDoc document, Op key) =>
        from settings in target.Query.Build(document: document, key: key)
        from objects in Optional(document.Objects.GetObjectList(settings: settings))
            .ToFin(Fail: key.InvalidResult())
            .Map(static values => toSeq(values))
        from matches in objects.TraverseM(native => target.Predicates
            .TraverseM(predicate => predicate.Match(document: document, native: native, key: key))
            .As()
            .Map(verdicts => (Native: native, Matches: verdicts.ForAll(identity)))).As()
        select matches.Filter(static match => match.Matches).Map(static match => match.Native);
}
```

## [04]-[TRANSACTION_RAIL]

- Owner: policy vocabularies close every provider-mode discriminant on rows. `TableOp` `[Union]` carries admitted per-occurrence payloads; `TableTransaction` `[Union]` distinguishes recorded, immediate, and navigation programs by shape rather than flags. `UndoBracket` is the shared document transaction capsule composed by table, block, and session-regime commit spines.
- Entry: `TableOp` factories admit raw payloads once. `TableTransaction.Recorded`, `Immediate`, and `Navigate` admit program shape before `Tables.Commit(DocumentSession, TableTransaction)` enters the host boundary. `Immediate` admits one non-undoing operation; recorded programs admit only undo-recorded operations, so the rollback guarantee never covers an untracked side effect.
- Law: `TransformPolicy.Relocate` reports the transformed identity as `Moved`; `Copy` and `History` report only the minted identity as `Created`. Sources remain unchanged on copy/history paths. Selection facts derive from before/after runtime snapshots, and state facts use separate `Hidden`/`Shown` and `Locked`/`Unlocked` slots.
- Law: `TableOp.Traits` totally classifies every case onto one of four trait rows — `Sourced`, `Recorded`, `Immediate`, `Navigation` — carrying undo recording, navigation, and kernel-context demand as one derived product. A host effect that cannot be reversed by the document record enters only an immediate transaction, so a recorded program has no untracked side effect.
- Law: `Amend` owns a duplicated `ObjectAttributes` lease, exposes only an in-place `Fin<Unit>` change callback, commits the duplicate synchronously, and disposes it before the operation leaves the host boundary. Canonical callback is the objects page's typed `AttributeProgram.Apply` — `Amend(target, program.Apply, notice)` — so attribute mutation is a closed `AttributeEdit` program; a hand-written mutation lambda survives only where no program case carries the member.
- Law: deleted-object operations require a runtime-preserving target. Explicit deleted rows and deleted-object queries preserve runtime serials without re-entering the active-id index, deletion captures runtime pairs before mutation, and receipts project them for a later `Revive` or `Expunge` request.
- Law: geometry intake resolves `Kind`, applies `Requirement.ForKind` to the original value, then composes the kernel `GeometryForm` lease operation. Native geometry remains borrowed; every value-form conversion is owned and disposed by `Lease.Use` after the host copies it.
- Law: page import carries `DocumentPath` and re-proves `DocumentFile.ThreeDm` inside the callback. Named-view restore carries `ViewportTarget`, resolves exactly one viewport immediately before the host call, and never retains a live viewport handle in request data.
- Law: the commit spine keeps the document handle inside one `DocumentSession.Demand`. It proves mutation, undo, and redraw needs against one snapshot before the first edit and refreshes the kernel context inside that window. Outside a command it owns the undo record, closes it on every exit, rolls a failed program back, clears the failed record from redo, and appends close, rollback, or redraw-restoration faults to the primary fault. Inside a command it enlists in `CurrentUndoRecordSerialNumber`, never closes or undoes the command-owned record, and returns the operation fault for the command boundary to propagate. An active non-command record is rejected before mutation, and redraw occurs only after success.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum]
public sealed partial class ObjectCustody {
    public static readonly ObjectCustody Resident = new(isReference: false);
    public static readonly ObjectCustody Reference = new(isReference: true);
    internal bool IsReference { get; }
}

[SmartEnum]
public sealed partial class ObjectMode {
    public static readonly ObjectMode Respect = new(ignoresModes: false);
    public static readonly ObjectMode Ignore = new(ignoresModes: true);
    internal bool IgnoresModes { get; }
}

[SmartEnum]
public sealed partial class Notice {
    public static readonly Notice Quiet = new(suppressesWarnings: true);
    public static readonly Notice Report = new(suppressesWarnings: false);
    internal bool SuppressesWarnings { get; }
}

[SmartEnum]
public sealed partial class TransformPolicy {
    public static readonly TransformPolicy Relocate = new(slot: TableSlot.Moved, apply: static (table, id, motion) => table.Transform(objectId: id, xform: motion, deleteOriginal: true));
    public static readonly TransformPolicy Copy = new(slot: TableSlot.Created, apply: static (table, id, motion) => table.Transform(objectId: id, xform: motion, deleteOriginal: false));
    public static readonly TransformPolicy History = new(slot: TableSlot.Created, apply: static (table, id, motion) => table.TransformWithHistory(objectId: id, xform: motion));

    internal TableSlot Slot { get; }

    [UseDelegateFromConstructor]
    internal partial Guid Apply(ObjectTable table, Guid id, Transform motion);
}

[SmartEnum]
public sealed partial class SelectionEdit {
    public static readonly SelectionEdit Add = new(apply: static (table, ids, policy) => table.Select(objectIds: ids, select: true, syncHighlight: policy.Highlight, persistentSelect: policy.Persistent, ignoreGripsState: policy.IgnoreGrips, ignoreLayerLocking: policy.IgnoreLayerLocking, ignoreLayerVisibility: policy.IgnoreLayerVisibility));
    public static readonly SelectionEdit Remove = new(apply: static (table, ids, policy) => table.Select(objectIds: ids, select: false, syncHighlight: policy.Highlight, persistentSelect: policy.Persistent, ignoreGripsState: policy.IgnoreGrips, ignoreLayerLocking: policy.IgnoreLayerLocking, ignoreLayerVisibility: policy.IgnoreLayerVisibility));
    public static readonly SelectionEdit Replace = new(apply: static (table, ids, policy) => table.SetSelectedObjects(objectIds: ids, syncHighlight: policy.Highlight, persistentSelect: policy.Persistent, ignoreGripsState: policy.IgnoreGrips, ignoreLayerLocking: policy.IgnoreLayerLocking, ignoreLayerVisibility: policy.IgnoreLayerVisibility));

    [UseDelegateFromConstructor]
    internal partial int Apply(ObjectTable table, IEnumerable<Guid> ids, SelectionPolicy policy);
}

[SmartEnum]
public sealed partial class ObjectState {
    public static readonly ObjectState Hidden = new(slot: TableSlot.Hidden, done: static native => native.IsHidden, apply: static (table, id, ignore) => table.Hide(objectId: id, ignoreLayerMode: ignore));
    public static readonly ObjectState Shown = new(slot: TableSlot.Shown, done: static native => !native.IsHidden, apply: static (table, id, ignore) => table.Show(objectId: id, ignoreLayerMode: ignore));
    public static readonly ObjectState Locked = new(slot: TableSlot.Locked, done: static native => native.IsLocked, apply: static (table, id, ignore) => table.Lock(objectId: id, ignoreLayerMode: ignore));
    public static readonly ObjectState Unlocked = new(slot: TableSlot.Unlocked, done: static native => !native.IsLocked, apply: static (table, id, ignore) => table.Unlock(objectId: id, ignoreLayerMode: ignore));

    internal TableSlot Slot { get; }

    [UseDelegateFromConstructor]
    internal partial bool Done(RhinoObject native);

    [UseDelegateFromConstructor]
    internal partial bool Apply(ObjectTable table, Guid id, bool ignoreLayerMode);
}

[SmartEnum]
public sealed partial class SelectionClear {
    public static readonly SelectionClear Transient = new(ignorePersistent: true);
    public static readonly SelectionClear All = new(ignorePersistent: false);
    internal bool IgnorePersistent { get; }
}

[SmartEnum]
public sealed partial class FlashMode {
    public static readonly FlashMode SelectionColor = new(useSelectionColor: true);
    public static readonly FlashMode Visibility = new(useSelectionColor: false);
    internal bool UseSelectionColor { get; }
}

[SmartEnum]
public sealed partial class DeletedPolicy {
    public static readonly DeletedPolicy Keep = new(purges: false);
    public static readonly DeletedPolicy Purge = new(purges: true);
    internal bool Purges { get; }
}

[SmartEnum]
public sealed partial class RedrawPolicy {
    public static readonly RedrawPolicy None = new(enabled: false, defers: false, suppress: false);
    public static readonly RedrawPolicy Continuous = new(enabled: true, defers: false, suppress: false);
    public static readonly RedrawPolicy Immediate = new(enabled: true, defers: false, suppress: true);
    public static readonly RedrawPolicy Deferred = new(enabled: true, defers: true, suppress: true);

    internal bool Enabled { get; }
    internal bool Defers { get; }
    internal bool Suppress { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record NamedRestore {
    private NamedRestore() { }

    private sealed record ProportionalCase(int Index, Viewport.ViewportTarget Target) : NamedRestore;
    private sealed record AnimatedCase(int Index, Viewport.ViewportTarget Target, Dimension Frames, int DelayMs) : NamedRestore;

    public static Fin<NamedRestore> Proportional(int index, Viewport.ViewportTarget target) {
        Op op = Op.Of();
        return from _ in guard(index >= 0, op.InvalidInput()).ToFin()
               from address in Optional(target).ToFin(Fail: op.InvalidInput())
               select (NamedRestore)new ProportionalCase(Index: index, Target: address);
    }

    public static Fin<NamedRestore> Animated(int index, Viewport.ViewportTarget target, Dimension frames, TimeSpan delay) {
        Op op = Op.Of();
        return from _ in guard(index >= 0, op.InvalidInput()).ToFin()
               from address in Optional(target).ToFin(Fail: op.InvalidInput())
               from __ in guard(delay >= TimeSpan.Zero && delay.Ticks % TimeSpan.TicksPerMillisecond is 0 && delay.TotalMilliseconds <= int.MaxValue, op.InvalidInput()).ToFin()
               select (NamedRestore)new AnimatedCase(Index: index, Target: address, Frames: frames, DelayMs: (int)delay.TotalMilliseconds);
    }

    internal Fin<Unit> Apply(RhinoDoc document, Op key) =>
        Switch(
            state: (Document: document, Op: key),
            proportionalCase: static (context, restore) =>
                from rows in restore.Target.Resolve(document: context.Document, key: context.Op)
                from row in Tables.One(rows: rows, key: context.Op)
                from _ in context.Op.Confirm(success: context.Document.NamedViews.RestoreWithAspectRatio(index: restore.Index, viewport: row.Viewport))
                select unit,
            animatedCase: static (context, restore) =>
                from rows in restore.Target.Resolve(document: context.Document, key: context.Op)
                from row in Tables.One(rows: rows, key: context.Op)
                from _ in context.Op.Confirm(success: context.Document.NamedViews.RestoreAnimatedConstantTime(index: restore.Index, viewport: row.Viewport, frames: restore.Frames.Value, ms_delay: restore.DelayMs))
                select unit);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HistoryRoll {
    private HistoryRoll() { }

    private sealed record UndoCase : HistoryRoll;
    private sealed record RedoCase : HistoryRoll;
    private sealed record ClearUndoCase(DeletedPolicy Deleted, Option<uint> Serial) : HistoryRoll;
    private sealed record ClearRedoCase : HistoryRoll;

    public static HistoryRoll Undo { get; } = new UndoCase();
    public static HistoryRoll Redo { get; } = new RedoCase();

    public static HistoryRoll ClearRedo { get; } = new ClearRedoCase();

    public static Fin<HistoryRoll> ClearUndo(DeletedPolicy deleted, Option<uint> serial = default) {
        Op op = Op.Of();
        return from policy in Optional(deleted).ToFin(Fail: op.InvalidInput())
               from _ in guard(serial.Map(static value => value > 0u).IfNone(noneValue: true), op.InvalidInput()).ToFin()
               select (HistoryRoll)new ClearUndoCase(Deleted: policy, Serial: serial);
    }

    internal Fin<Unit> Apply(RhinoDoc document, Op key) =>
        Switch(
            state: (Document: document, Op: key),
            undoCase: static (context, _) => context.Op.Confirm(success: context.Document.Undo()),
            redoCase: static (context, _) => context.Op.Confirm(success: context.Document.Redo()),
            clearUndoCase: static (context, roll) => context.Op.Catch(() => {
                roll.Serial.Match(
                    Some: serial => context.Document.ClearUndoRecords(undoSerialNumber: serial, purgeDeletedObjects: roll.Deleted.Purges),
                    None: () => context.Document.ClearUndoRecords(purgeDeletedObjects: roll.Deleted.Purges));
                return Fin.Succ(value: unit);
            }),
            clearRedoCase: static (context, _) => context.Op.Catch(() => {
                context.Document.ClearRedoRecords();
                return Fin.Succ(value: unit);
            }));
}

internal readonly record struct TableOpTraits(bool RecordsUndo, bool Navigates, bool RequiresContext) {
    internal static TableOpTraits Sourced { get; } = new(RecordsUndo: true, Navigates: false, RequiresContext: true);
    internal static TableOpTraits Recorded { get; } = new(RecordsUndo: true, Navigates: false, RequiresContext: false);
    internal static TableOpTraits Immediate { get; } = new(RecordsUndo: false, Navigates: false, RequiresContext: false);
    internal static TableOpTraits Navigation { get; } = new(RecordsUndo: false, Navigates: true, RequiresContext: false);
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TableOp {
    private TableOp() { }

    private sealed record AddCase(Seq<object> Sources, Option<ObjectAttributes> Attributes, Option<HistoryRecord> History, ObjectCustody Custody) : TableOp;
    private sealed record ReplaceCase(TableTarget Target, object Replacement, ObjectMode Modes) : TableOp;
    private sealed record DeleteCase(TableTarget Target, Notice Notice, ObjectMode Modes) : TableOp;
    private sealed record TransformCase(TableTarget Target, Transform Motion, TransformPolicy Policy) : TableOp;
    private sealed record AmendCase(TableTarget Target, Func<ObjectAttributes, Fin<Unit>> Change, Notice Notice) : TableOp;
    private sealed record SelectCase(TableTarget Target, SelectionEdit Edit, SelectionPolicy Policy) : TableOp;
    private sealed record StateCase(TableTarget Target, ObjectState State, ObjectMode Modes) : TableOp;
    private sealed record ClearSelectionCase(SelectionClear Scope) : TableOp;
    private sealed record FlashCase(TableTarget Target, FlashMode Mode) : TableOp;
    private sealed record ReviveCase(TableTarget Target) : TableOp;
    private sealed record ExpungeCase(TableTarget Target) : TableOp;
    private sealed record CloudCase(Dimension X, Dimension Y, Dimension Z, Arr<Point3d> Box, Option<ObjectAttributes> Attributes, Option<HistoryRecord> History, ObjectCustody Custody) : TableOp;
    private sealed record RebindCase(TableTarget Target, int DefinitionIndex) : TableOp;
    private sealed record ReclaimCase(TableKind Kind) : TableOp;
    private sealed record ImportPageCase(DocumentPath Path, Guid MainViewportId, string PageName) : TableOp;
    private sealed record RestoreViewCase(NamedRestore Restore) : TableOp;
    private sealed record RollCase(HistoryRoll Navigation) : TableOp;

    public static Fin<TableOp> Add(ObjectCustody custody, Option<ObjectAttributes> attributes = default, Option<HistoryRecord> history = default, params ReadOnlySpan<object> sources) {
        Op op = Op.Of();
        return from policy in Optional(custody).ToFin(Fail: op.InvalidInput())
               from values in toSeq(sources.ToArray()).TraverseM(source => Optional(source).ToFin(Fail: op.InvalidInput())).As()
               from _ in guard(!values.IsEmpty, op.InvalidInput()).ToFin()
               select (TableOp)new AddCase(Sources: values, Attributes: attributes, History: history, Custody: policy);
    }

    public static Fin<TableOp> Replace(TableTarget target, object replacement, ObjectMode modes) {
        Op op = Op.Of();
        return (Optional(target).ToFin(Fail: op.InvalidInput()), Optional(replacement).ToFin(Fail: op.InvalidInput()), Optional(modes).ToFin(Fail: op.InvalidInput()))
            .Apply(static (address, geometry, policy) => (TableOp)new ReplaceCase(Target: address, Replacement: geometry, Modes: policy)).As();
    }

    public static Fin<TableOp> Delete(TableTarget target, Notice notice, ObjectMode modes) {
        Op op = Op.Of();
        return (Optional(target).ToFin(Fail: op.InvalidInput()), Optional(notice).ToFin(Fail: op.InvalidInput()), Optional(modes).ToFin(Fail: op.InvalidInput()))
            .Apply(static (address, reporting, policy) => (TableOp)new DeleteCase(Target: address, Notice: reporting, Modes: policy)).As();
    }

    public static Fin<TableOp> Transform(TableTarget target, Transform motion, TransformPolicy policy) {
        Op op = Op.Of();
        return from address in Optional(target).ToFin(Fail: op.InvalidInput())
               from transform in op.AcceptInput(value: motion)
               from mode in Optional(policy).ToFin(Fail: op.InvalidInput())
               select (TableOp)new TransformCase(Target: address, Motion: transform, Policy: mode);
    }

    public static Fin<TableOp> Amend(TableTarget target, Func<ObjectAttributes, Fin<Unit>> change, Notice notice) {
        Op op = Op.Of();
        return (Optional(target).ToFin(Fail: op.InvalidInput()), Optional(change).ToFin(Fail: op.InvalidInput()), Optional(notice).ToFin(Fail: op.InvalidInput()))
            .Apply(static (address, revise, reporting) => (TableOp)new AmendCase(Target: address, Change: revise, Notice: reporting)).As();
    }

    public static Fin<TableOp> Select(TableTarget target, SelectionEdit edit, SelectionPolicy policy) {
        Op op = Op.Of();
        return (Optional(target).ToFin(Fail: op.InvalidInput()), Optional(edit).ToFin(Fail: op.InvalidInput()))
            .Apply((address, mutation) => (TableOp)new SelectCase(Target: address, Edit: mutation, Policy: policy)).As();
    }

    public static Fin<TableOp> State(TableTarget target, ObjectState state, ObjectMode modes) {
        Op op = Op.Of();
        return (Optional(target).ToFin(Fail: op.InvalidInput()), Optional(state).ToFin(Fail: op.InvalidInput()), Optional(modes).ToFin(Fail: op.InvalidInput()))
            .Apply(static (address, mutation, policy) => (TableOp)new StateCase(Target: address, State: mutation, Modes: policy)).As();
    }

    public static Fin<TableOp> ClearSelection(SelectionClear scope) =>
        Optional(scope).ToFin(Fail: Op.Of().InvalidInput()).Map(static value => (TableOp)new ClearSelectionCase(Scope: value));

    public static Fin<TableOp> Flash(TableTarget target, FlashMode mode) {
        Op op = Op.Of();
        return (Optional(target).ToFin(Fail: op.InvalidInput()), Optional(mode).ToFin(Fail: op.InvalidInput()))
            .Apply(static (address, display) => (TableOp)new FlashCase(Target: address, Mode: display)).As();
    }

    public static Fin<TableOp> Revive(TableTarget target) =>
        Retained(target: target, mint: static value => new ReviveCase(Target: value));

    public static Fin<TableOp> Expunge(TableTarget target) =>
        Retained(target: target, mint: static value => new ExpungeCase(Target: value));

    private static Fin<TableOp> Retained(TableTarget target, Func<TableTarget, TableOp> mint) =>
        Optional(target).ToFin(Fail: Op.Of().InvalidInput())
            .Bind(value => value.RetainsRuntime ? Fin.Succ(value: mint(arg: value)) : Fin.Fail<TableOp>(Op.Of().InvalidInput()));

    public static Fin<TableOp> Cloud(Dimension x, Dimension y, Dimension z, Arr<Point3d> box, ObjectCustody custody, Option<ObjectAttributes> attributes = default, Option<HistoryRecord> history = default) {
        Op op = Op.Of();
        return from policy in Optional(custody).ToFin(Fail: op.InvalidInput())
               from _ in guard(
                   box.Count is 8
                   && (long)x.Value * y.Value * z.Value <= int.MaxValue,
                   op.InvalidInput()).ToFin()
               from __ in box.AsIterable().ToSeq().TraverseM(point => op.AcceptInput(value: point)).As()
               select (TableOp)new CloudCase(X: x, Y: y, Z: z, Box: box, Attributes: attributes, History: history, Custody: policy);
    }

    public static Fin<TableOp> Rebind(TableTarget target, int definitionIndex) {
        Op op = Op.Of();
        return from address in Optional(target).ToFin(Fail: op.InvalidInput())
               from _ in guard(definitionIndex >= 0, op.InvalidInput()).ToFin()
               select (TableOp)new RebindCase(Target: address, DefinitionIndex: definitionIndex);
    }

    public static Fin<TableOp> Reclaim(TableKind kind) =>
        Optional(kind).ToFin(Fail: Op.Of().InvalidInput()).Map(static value => (TableOp)new ReclaimCase(Kind: value));

    public static Fin<TableOp> ImportPage(DocumentPath path, Guid mainViewportId, string pageName) {
        Op op = Op.Of();
        return from name in op.AcceptText(value: pageName)
               from viewport in op.AcceptInput(value: mainViewportId)
               select (TableOp)new ImportPageCase(Path: path, MainViewportId: viewport, PageName: name);
    }

    public static Fin<TableOp> RestoreView(NamedRestore restore) =>
        Optional(restore).ToFin(Fail: Op.Of().InvalidInput()).Map(static value => (TableOp)new RestoreViewCase(Restore: value));

    public static Fin<TableOp> Roll(HistoryRoll navigation) =>
        Optional(navigation).ToFin(Fail: Op.Of().InvalidInput()).Map(static value => (TableOp)new RollCase(Navigation: value));

    internal TableOpTraits Traits => Switch(
        addCase: static _ => TableOpTraits.Sourced,
        replaceCase: static _ => TableOpTraits.Sourced,
        deleteCase: static _ => TableOpTraits.Recorded,
        transformCase: static _ => TableOpTraits.Recorded,
        amendCase: static _ => TableOpTraits.Recorded,
        selectCase: static _ => TableOpTraits.Immediate,
        stateCase: static _ => TableOpTraits.Recorded,
        clearSelectionCase: static _ => TableOpTraits.Immediate,
        flashCase: static _ => TableOpTraits.Immediate,
        reviveCase: static _ => TableOpTraits.Recorded,
        expungeCase: static _ => TableOpTraits.Immediate,
        cloudCase: static _ => TableOpTraits.Recorded,
        rebindCase: static _ => TableOpTraits.Recorded,
        reclaimCase: static _ => TableOpTraits.Immediate,
        importPageCase: static _ => TableOpTraits.Recorded,
        restoreViewCase: static _ => TableOpTraits.Immediate,
        rollCase: static _ => TableOpTraits.Navigation);

    internal Fin<TableReceipt> Apply(RhinoDoc document, Option<Context> domain, Op op) =>
        Switch(
            (Document: document, Domain: domain, Op: op),
            addCase: static (context, edit) =>
                from model in context.Domain.ToFin(Fail: context.Op.MissingContext())
                from ids in edit.Sources.TraverseM(source => GeometryIntake.Admit(source: source, domain: model, key: context.Op)
                    .Bind(lease => lease.Use(native => context.Op.AcceptValue(value: context.Document.Objects.Add(
                            geometry: native,
                            attributes: edit.Attributes.IfNoneUnsafe((ObjectAttributes?)null),
                            history: edit.History.IfNoneUnsafe((HistoryRecord?)null),
                            reference: edit.Custody.IsReference))))).As()
                from runtime in Tables.Runtime(document: context.Document, ids: ids, key: context.Op)
                select TableReceipt.Objects(slot: TableSlot.Created, values: runtime),
            replaceCase: static (context, edit) =>
                from model in context.Domain.ToFin(Fail: context.Op.MissingContext())
                from ids in edit.Target.Resolve(document: context.Document, key: context.Op)
                from single in Tables.One(rows: ids, key: context.Op)
                from _ in GeometryIntake.Admit(source: edit.Replacement, domain: model, key: context.Op)
                    .Bind(lease => lease.Use(native => context.Op.Confirm(success: context.Document.Objects.Replace(objectId: single, geometry: native, ignoreModes: edit.Modes.IgnoresModes))))
                from runtime in Tables.Runtime(document: context.Document, ids: Seq(single), key: context.Op)
                select TableReceipt.Objects(slot: TableSlot.Replaced, values: runtime),
            deleteCase: static (context, edit) =>
                from targets in edit.Target.Serials(document: context.Document, key: context.Op)
                from _ in edit.Modes.IgnoresModes
                    ? targets.TraverseM(target => Optional(context.Document.Objects.FindId(target.Id)).ToFin(Fail: context.Op.InvalidResult())
                        .Bind(native => context.Op.Confirm(success: context.Document.Objects.Delete(obj: native, quiet: edit.Notice.SuppressesWarnings, ignoreModes: true)))).As().Map(static _ => unit)
                    : context.Op.Confirm(success: context.Document.Objects.Delete(objectIds: targets.Map(static target => target.Id).AsIterable(), quiet: edit.Notice.SuppressesWarnings) == targets.Count)
                select TableReceipt.Objects(slot: TableSlot.Deleted, values: targets),
            transformCase: static (context, edit) =>
                from ids in edit.Target.Resolve(document: context.Document, key: context.Op)
                from transformed in ids.TraverseM(id => context.Op.AcceptValue(value: edit.Policy.Apply(table: context.Document.Objects, id: id, motion: edit.Motion))).As()
                from runtime in Tables.Runtime(document: context.Document, ids: transformed, key: context.Op)
                select TableReceipt.Objects(slot: edit.Policy.Slot, values: runtime),
            amendCase: static (context, edit) =>
                from ids in edit.Target.Resolve(document: context.Document, key: context.Op)
                from amended in ids.TraverseM(id =>
                    from native in Optional(context.Document.Objects.FindId(id)).ToFin(Fail: context.Op.InvalidResult())
                    from attributes in Optional(native.Attributes?.Duplicate()).ToFin(Fail: context.Op.InvalidResult())
                    from _ in new Lease<ObjectAttributes>.Owned(Value: attributes).Use(owned =>
                        from __ in edit.Change(arg: owned)
                        from ___ in context.Op.Confirm(success: context.Document.Objects.ModifyAttributes(
                            objectId: id,
                            newAttributes: owned,
                            quiet: edit.Notice.SuppressesWarnings))
                        select unit)
                    select id).As()
                from runtime in Tables.Runtime(document: context.Document, ids: amended, key: context.Op)
                select TableReceipt.Objects(slot: TableSlot.Amended, values: runtime),
            selectCase: static (context, edit) =>
                from ids in edit.Target.Resolve(document: context.Document, key: context.Op)
                from receipt in SelectionSpan(
                    document: context.Document,
                    apply: () => edit.Edit.Apply(table: context.Document.Objects, ids: ids.AsIterable(), policy: edit.Policy),
                    op: context.Op)
                select receipt,
            stateCase: static (context, edit) =>
                from targets in edit.Target.Serials(document: context.Document, key: context.Op)
                from changed in Tables.ApplyState(document: context.Document, targets: targets, state: edit.State, modes: edit.Modes, key: context.Op)
                select TableReceipt.Objects(slot: edit.State.Slot, values: changed),
            clearSelectionCase: static (context, edit) => SelectionSpan(
                document: context.Document,
                apply: () => context.Document.Objects.UnselectAll(ignorePersistentSelections: edit.Scope.IgnorePersistent), op: context.Op),
            flashCase: static (context, edit) =>
                from targets in edit.Target.Serials(document: context.Document, key: context.Op)
                from objects in targets.TraverseM(target => Optional(context.Document.Objects.FindId(target.Id)).ToFin(Fail: context.Op.InvalidResult())).As()
                from _ in context.Op.Catch(() => {
                    context.Document.Views.FlashObjects(list: objects.AsIterable(), useSelectionColor: edit.Mode.UseSelectionColor);
                    return Fin.Succ(value: unit);
                })
                select TableReceipt.Objects(slot: TableSlot.Flashed, values: targets),
            reviveCase: static (context, edit) => Lifecycle(
                document: context.Document, target: edit.Target, slot: TableSlot.Revived,
                apply: static (objects, serial) => objects.Undelete(runtimeSerialNumber: serial), op: context.Op),
            expungeCase: static (context, edit) => Lifecycle(
                document: context.Document, target: edit.Target, slot: TableSlot.Expunged,
                apply: static (objects, serial) => objects.Purge(runtimeSerialNumber: serial), op: context.Op),
            cloudCase: static (context, edit) =>
                from id in context.Op.AcceptValue(value: context.Document.Objects.AddOrderedPointCloud(
                    xCt: edit.X.Value,
                    yCt: edit.Y.Value,
                    zCt: edit.Z.Value,
                    box: edit.Box.ToArray(),
                    attributes: edit.Attributes.IfNoneUnsafe((ObjectAttributes?)null),
                    history: edit.History.IfNoneUnsafe((HistoryRecord?)null),
                    reference: edit.Custody.IsReference))
                from runtime in Tables.Runtime(document: context.Document, ids: Seq(id), key: context.Op)
                select TableReceipt.Objects(slot: TableSlot.Created, values: runtime),
            rebindCase: static (context, edit) =>
                from ids in edit.Target.Resolve(document: context.Document, key: context.Op)
                from rebound in ids.TraverseM(id => context.Op.Confirm(success: context.Document.Objects.ReplaceInstanceObject(objectId: id, instanceDefinitionIndex: edit.DefinitionIndex)).Map(_ => id)).As()
                from runtime in Tables.Runtime(document: context.Document, ids: rebound, key: context.Op)
                select TableReceipt.Objects(slot: TableSlot.Replaced, values: runtime),
            reclaimCase: static (context, edit) => edit.Kind.Reclaim(document: context.Document, key: context.Op)
                .Map(count => TableReceipt.Component(kind: edit.Kind, tally: count)),
            importPageCase: static (context, edit) =>
                from path in edit.Path.Resolve(file: DocumentFile.ThreeDm, key: context.Op)
                from _ in context.Op.Confirm(success: context.Document.Views.ImportPageView(filename: path, mainViewportId: edit.MainViewportId, pageName: edit.PageName))
                select TableReceipt.Component(kind: TableKind.Views, tally: context.Document.Views.PageViewCount),
            restoreViewCase: static (context, edit) => edit.Restore.Apply(document: context.Document, key: context.Op)
                .Map(_ => TableReceipt.Component(kind: TableKind.NamedViews, tally: 1)),
            rollCase: static (context, edit) => edit.Navigation.Apply(document: context.Document, key: context.Op)
                .Map(static _ => TableReceipt.Empty));

    private static Fin<TableReceipt> SelectionSpan(RhinoDoc document, Func<int> apply, Op op) =>
        from before in Tables.Selected(document: document, key: op)
        from _ in guard(apply() >= 0, op.InvalidResult()).ToFin()
        from after in Tables.Selected(document: document, key: op)
        select TableReceipt.SelectionDelta(before: before, after: after);

    private static Fin<TableReceipt> Lifecycle(RhinoDoc document, TableTarget target, TableSlot slot, Func<ObjectTable, uint, bool> apply, Op op) =>
        from targets in target.Serials(document: document, key: op)
        from changed in targets.TraverseM(value => op.Confirm(success: apply(document.Objects, value.Serial)).Map(_ => value)).As()
        select TableReceipt.Objects(slot: slot, values: changed);
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct SelectionPolicy(bool Highlight, bool IgnoreGrips, bool Persistent, bool IgnoreLayerLocking, bool IgnoreLayerVisibility) {
    public static SelectionPolicy Default { get; } = new(Highlight: true, IgnoreGrips: true, Persistent: true, IgnoreLayerLocking: false, IgnoreLayerVisibility: false);
}

public sealed class TableCustomUndo {
    private TableCustomUndo(string name, EventHandler<CustomUndoEventArgs> handler, Option<object> tag) {
        Name = name;
        Handler = handler;
        Tag = tag;
    }

    internal string Name { get; }
    private EventHandler<CustomUndoEventArgs> Handler { get; }
    private Option<object> Tag { get; }

    public static Fin<TableCustomUndo> Of(string name, EventHandler<CustomUndoEventArgs> handler, Option<object> tag = default) {
        Op op = Op.Of();
        return from admitted in op.AcceptText(value: name)
               from callback in Optional(handler).ToFin(Fail: op.InvalidInput())
               select new TableCustomUndo(name: admitted, handler: callback, tag: tag);
    }

    internal Fin<string> Register(RhinoDoc document, Op key) =>
        Tag.Match(
            Some: tag => key.Confirm(success: document.AddCustomUndoEvent(description: Name, handler: Handler, tag: tag)),
            None: () => key.Confirm(success: document.AddCustomUndoEvent(description: Name, handler: Handler)))
        .Map(_ => Name);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TableTransaction {
    private TableTransaction() { }

    private sealed record RecordedCase(string Name, Seq<TableOp> Operations, RedrawPolicy Redraw, Seq<TableCustomUndo> CustomUndo) : TableTransaction;
    private sealed record ImmediateCase(string Name, Seq<TableOp> Operations, RedrawPolicy Redraw) : TableTransaction;
    private sealed record NavigationCase(string Name, TableOp Operation, RedrawPolicy Redraw) : TableTransaction;

    public static Fin<TableTransaction> Recorded(string name, RedrawPolicy redraw, Seq<TableCustomUndo> customUndo, params ReadOnlySpan<TableOp> operations) {
        Op op = Op.Of();
        return from plan in Admit(name: name, redraw: redraw, operations: operations, op: op)
               from undo in customUndo.TraverseM(item => Optional(item).ToFin(Fail: op.InvalidInput())).As()
               from _ in guard(
                   (!plan.Operations.IsEmpty || !undo.IsEmpty)
                   && plan.Operations.ForAll(static operation => operation.Traits is { RecordsUndo: true, Navigates: false }),
                   op.InvalidInput()).ToFin()
               select (TableTransaction)new RecordedCase(Name: plan.Name, Operations: plan.Operations, Redraw: plan.Redraw, CustomUndo: undo);
    }

    public static Fin<TableTransaction> Immediate(string name, RedrawPolicy redraw, params ReadOnlySpan<TableOp> operations) {
        Op op = Op.Of();
        return from plan in Admit(name: name, redraw: redraw, operations: operations, op: op)
               from _ in guard(
                   plan.Operations.Count is 1
                   && plan.Operations.ForAll(static operation => operation.Traits is { RecordsUndo: false, Navigates: false }),
                   op.InvalidInput()).ToFin()
               select (TableTransaction)new ImmediateCase(Name: plan.Name, Operations: plan.Operations, Redraw: plan.Redraw);
    }

    public static Fin<TableTransaction> Navigate(string name, HistoryRoll navigation, RedrawPolicy redraw) {
        Op op = Op.Of();
        return from admitted in op.AcceptText(value: name)
               from policy in Optional(redraw).ToFin(Fail: op.InvalidInput())
               from operation in TableOp.Roll(navigation: navigation)
               select (TableTransaction)new NavigationCase(Name: admitted, Operation: operation, Redraw: policy);
    }

    internal TransactionPlan Materialize() =>
        Switch(
            recordedCase: static transaction => new TransactionPlan(Name: transaction.Name, Operations: transaction.Operations, Redraw: transaction.Redraw, CustomUndo: transaction.CustomUndo, RequiresUndo: true, RecordsUndo: true),
            immediateCase: static transaction => new TransactionPlan(Name: transaction.Name, Operations: transaction.Operations, Redraw: transaction.Redraw, CustomUndo: Seq<TableCustomUndo>(), RequiresUndo: false, RecordsUndo: false),
            navigationCase: static transaction => new TransactionPlan(Name: transaction.Name, Operations: Seq(transaction.Operation), Redraw: transaction.Redraw, CustomUndo: Seq<TableCustomUndo>(), RequiresUndo: true, RecordsUndo: false));

    private static Fin<(string Name, Seq<TableOp> Operations, RedrawPolicy Redraw)> Admit(string name, RedrawPolicy redraw, ReadOnlySpan<TableOp> operations, Op op) =>
        from admitted in op.AcceptText(value: name)
        from policy in Optional(redraw).ToFin(Fail: op.InvalidInput())
        from program in toSeq(operations.ToArray()).TraverseM(operation => Optional(operation).ToFin(Fail: op.InvalidInput())).As()
        select (Name: admitted, Operations: program, Redraw: policy);
}

internal readonly record struct TransactionPlan(
    string Name,
    Seq<TableOp> Operations,
    RedrawPolicy Redraw,
    Seq<TableCustomUndo> CustomUndo,
    bool RequiresUndo,
    bool RecordsUndo);

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class GeometryIntake {
    internal static Fin<Lease<GeometryBase>> Admit(object source, Context domain, Op key) =>
        from value in Optional(source).ToFin(Fail: key.InvalidInput())
        from kind in value.KindOf(context: domain)
        from _ in Requirement.ForKind(kind: kind).Apply(context: domain, value: value, cancel: CancellationToken.None).ToFin()
        from lease in value.GeometryForm(key: key)
        select lease;
}

public static class Tables {
    public static Fin<TableReceipt> Commit(DocumentSession session, TableTransaction transaction, Op? key = null) {
        Op op = key.OrDefault();
        return from scope in Optional(session).ToFin(Fail: op.InvalidInput())
               from request in Optional(transaction).ToFin(Fail: op.InvalidInput())
               let plan = request.Materialize()
               let needs = Seq(SessionNeed.Mutate)
                   + (plan.RequiresUndo ? Seq(SessionNeed.Undo) : Seq<SessionNeed>())
                   + (plan.Redraw.Enabled ? Seq(SessionNeed.Redraw) : Seq<SessionNeed>())
               from receipt in scope.Demand(
                   use: document => Run(document: document, plan: plan, op: op),
                   key: op,
                   needs: needs.ToArray())
               select receipt;
    }

    internal static Fin<T> One<T>(Seq<T> rows, Op key) =>
        rows switch { [var only] => Fin.Succ(value: only), _ => Fin.Fail<T>(error: key.InvalidInput()) };

    internal static Fin<Seq<ObjectRuntime>> Runtime(RhinoDoc document, Seq<Guid> ids, Op key) =>
        ids.Distinct().TraverseM(id => Optional(document.Objects.FindId(id))
            .ToFin(Fail: key.InvalidResult())
            .Bind(native => ObjectRuntime.Of(id: id, serial: native.RuntimeSerialNumber, key: key))).As();

    internal static Fin<Seq<ObjectRuntime>> Selected(RhinoDoc document, Op key) =>
        Optional(document.Objects.GetSelectedObjects(includeLights: true, includeGrips: true))
            .ToFin(Fail: key.InvalidResult())
            .Bind(values => values.AsIterable().ToSeq()
                .TraverseM(native => ObjectRuntime.Of(id: native.Id, serial: native.RuntimeSerialNumber, key: key)).As())
            .Map(static values => values.DistinctBy(static value => value.Id));

    internal static Fin<Seq<ObjectRuntime>> ApplyState(RhinoDoc document, Seq<ObjectRuntime> targets, ObjectState state, ObjectMode modes, Op key) =>
        targets.TraverseM(target => Optional(document.Objects.FindId(target.Id))
            .ToFin(Fail: key.InvalidResult())
            .Bind(native => state.Done(native: native)
                ? Fin.Succ(value: Option<ObjectRuntime>.None)
                : state.Apply(table: document.Objects, id: target.Id, ignoreLayerMode: modes.IgnoresModes)
                    ? Fin.Succ(value: Some(target))
                    : Fin.Fail<Option<ObjectRuntime>>(error: key.InvalidResult()))).As().Map(static values => values.Somes());

    private static Fin<TableReceipt> Run(RhinoDoc document, TransactionPlan plan, Op op) =>
        from domain in plan.Operations.Exists(static operation => operation.Traits.RequiresContext)
            ? Rasm.Domain.Context.Of(doc: document).ToFin().Map(Some)
            : Fin.Succ(Option<Context>.None)
        from receipt in Mutate(
            document: document,
            plan: plan,
            run: () =>
                from custom in plan.CustomUndo.TraverseM(undo => undo.Register(document: document, key: op)).As()
                from folded in plan.Operations
                    .TraverseM(operation => operation.Apply(document: document, domain: domain, op: op)).As()
                    .Map(static receipts => receipts.Fold(TableReceipt.Empty, static (state, value) => state + value))
                select folded + TableReceipt.CustomUndo(names: custom),
            op: op)
        from _ in plan.Redraw.Enabled
            ? op.Catch(() => {
                document.Views.Redraw(deferred: plan.Redraw.Defers);
                return Fin.Succ(value: unit);
            })
            : Fin.Succ(value: unit)
        select receipt;

    private static Fin<TableReceipt> Mutate(RhinoDoc document, TransactionPlan plan, Func<Fin<TableReceipt>> run, Op op) =>
        op.Catch(() => {
            bool priorRedraw = document.Views.RedrawEnabled;
            Fin<Unit> suppressed = op.Catch(() => {
                _ = Op.SideWhen(plan.Redraw.Suppress, () => document.Views.EnableRedraw(
                    enable: false,
                    redrawDocument: false,
                    redrawLayers: false));
                return Fin.Succ(value: unit);
            });
            Fin<TableReceipt> outcome = suppressed.Bind(_ => op.Catch(() => {
                    using UndoBracket undo = UndoBracket.Begin(
                        document: document,
                        name: plan.Name,
                        recordsUndo: plan.RecordsUndo);
                    Fin<TableReceipt> executed = guard(undo.Admitted, op.InvalidResult()).ToFin()
                        .Bind(_ => op.Catch(run));
                    return undo.Seal(outcome: executed, stamp: static (receipt, serial) => receipt + TableReceipt.Undo(serial: serial), key: op);
                }));
            K<Validation<Error>, Unit> restored = op.Catch(() => {
                _ = Op.SideWhen(plan.Redraw.Suppress, () => document.Views.EnableRedraw(
                    enable: priorRedraw,
                    redrawDocument: false,
                    redrawLayers: false));
                return Fin.Succ(value: unit);
            }).ToValidation();
            K<Validation<Error>, TableReceipt> settled = outcome.ToValidation();
            return (settled, restored).Apply(static (receipt, _) => receipt).As().ToFin();
        });
}

// --- [BOUNDARIES] -------------------------------------------------------------------------
internal ref struct UndoBracket {
    private readonly RhinoDoc document;
    private readonly uint serial;
    private readonly bool required;
    private readonly bool owned;
    private readonly bool enlisted;
    private bool closed;
    private bool terminal;

    private UndoBracket(RhinoDoc document, uint serial, bool required, bool owned, bool enlisted) {
        this.document = document;
        this.serial = serial;
        this.required = required;
        this.owned = owned;
        this.enlisted = enlisted;
        closed = false;
        terminal = false;
    }

    public bool Admitted => !required || ((owned || enlisted) && serial > 0u);

    public static UndoBracket Begin(RhinoDoc document, string name, bool recordsUndo) {
        bool active = document.UndoRecordingIsActive;
        bool inCommand = global::Rhino.Commands.Command.InCommand();
        bool owned = recordsUndo && !inCommand && !active;
        bool enlisted = recordsUndo && inCommand && active && document.CurrentUndoRecordSerialNumber > 0u;
        return new UndoBracket(
            document: document,
            serial: owned ? document.BeginUndoRecord(description: name) : enlisted ? document.CurrentUndoRecordSerialNumber : 0u,
            required: recordsUndo,
            owned: owned,
            enlisted: enlisted);
    }

    public Fin<TReceipt> Seal<TReceipt>(Fin<TReceipt> outcome, Func<TReceipt, uint, TReceipt> stamp, Op key) {
        if (terminal) {
            return Fin.Fail<TReceipt>(error: key.InvalidResult());
        }
        terminal = true;
        RhinoDoc owner = document;
        uint record = serial;
        bool ownsRecord = owned;
        bool joinsRecord = enlisted;
        Fin<TReceipt> prepared = outcome.Bind(receipt =>
            from fold in Optional(stamp).ToFin(Fail: key.InvalidInput())
            from stamped in key.Catch(() => Fin.Succ(value: fold(receipt, record)))
            select stamped);
        Fin<Option<Error>> closure = CloseBounded(key: key);
        return prepared.Match(
            Succ: receipt => closure.Match(
                Succ: _ => Fin.Succ(value: receipt),
                Fail: close => Fin.Fail<TReceipt>(error: close + key.Caution(
                    concern: "undo record remains open after bounded close recovery"))),
            Fail: primary => closure.Match(
                Succ: recovered => Rollback<TReceipt>(
                    document: owner,
                    owned: ownsRecord,
                    enlisted: joinsRecord,
                    primary: recovered.Match(
                        Some: close => primary + close,
                        None: () => primary),
                    key: key),
                Fail: close => Fin.Fail<TReceipt>(error: primary
                    + close
                    + key.Caution(concern: "undo record could not close, so rollback was not executed"))));
    }

    public void Dispose() {
        if (terminal) {
            return;
        }
        terminal = true;
        _ = CloseBounded(key: Op.Of());
    }

    private Fin<Option<Error>> CloseBounded(Op key) => Close(key: key).Match(
        Succ: static _ => Fin.Succ(Option<Error>.None),
        Fail: first => Close(key: key).Match(
            Succ: _ => Fin.Succ(value: Some(first)),
            Fail: second => Fin.Fail<Option<Error>>(error: first + second)));

    private Fin<Unit> Close(Op key) {
        if (closed) { return Fin.Succ(value: unit); }
        RhinoDoc owner = document;
        uint record = serial;
        bool ownsRecord = owned;
        Fin<Unit> outcome = key.Catch(() => key.Confirm(
            success: !ownsRecord || (record > 0u && owner.EndUndoRecord(undoRecordSerialNumber: record))));
        if (outcome.IsSucc) { closed = true; }
        return outcome;
    }

    private static Fin<TReceipt> Rollback<TReceipt>(RhinoDoc document, bool owned, bool enlisted, Error primary, Op key) =>
        !owned
            ? Fin.Fail<TReceipt>(error: enlisted
                ? primary + key.Caution(concern: "command-owned undo record requires boundary failure propagation")
                : primary)
            : key.Catch(() =>
                key.Confirm(success: document.Undo()).Map(_ => {
                    document.ClearRedoRecords();
                    return unit;
                }))
                .Match(
                    Succ: _ => Fin.Fail<TReceipt>(error: primary),
                    Fail: rollback => Fin.Fail<TReceipt>(error: primary + rollback));
}
```

## [05]-[RECEIPTS]

- Owner: `TableSlot` `[SmartEnum<int>]` names object consequences. `TableFact` `[Union]` carries object runtime evidence, component-table tallies, undo serials, and custom-undo names without a second payload discriminator. `TableReceipt` is the additive fold over that one fact stream.
- Entry: `Ids(TableSlot)` and `Runtime(TableSlot)` project object consequences; `Components`, `UndoRecords`, and `CustomUndoNames` project the remaining fact cases. A receipt can feed its deleted runtime rows directly into `TableTarget.Deleted`.
- Law: `UndoBracket.Seal<TReceipt>` is receipt-agnostic: table, block, and session-regime spines fold the sealed serial into their own receipt without a foreign-receipt hop. The stamp executes before close, so a stamp fault remains rollback-capable.
- Law: `Seal` owns bounded close recovery and the terminal rollback decision. A failed program rolls back after a recovered close and reports the recovered close fault; an unrecoverable close reports rollback as unexecuted. `Dispose` cannot re-enter close after any seal attempt.
- Law: fact construction remains internal to the receipt. Slot/body mismatch, empty identity, zero runtime serial, negative tally, and zero undo serial cannot enter the public stream.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class TableSlot {
    public static readonly TableSlot Created = new(key: 0);
    public static readonly TableSlot Replaced = new(key: 1);
    public static readonly TableSlot Deleted = new(key: 2);
    public static readonly TableSlot Moved = new(key: 3);
    public static readonly TableSlot Selected = new(key: 4);
    public static readonly TableSlot Unselected = new(key: 5);
    public static readonly TableSlot Hidden = new(key: 6);
    public static readonly TableSlot Shown = new(key: 7);
    public static readonly TableSlot Locked = new(key: 8);
    public static readonly TableSlot Unlocked = new(key: 9);
    public static readonly TableSlot Flashed = new(key: 10);
    public static readonly TableSlot Amended = new(key: 11);
    public static readonly TableSlot Revived = new(key: 12);
    public static readonly TableSlot Expunged = new(key: 13);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record TableFact {
    private TableFact() { }
    internal sealed record ObjectCase(TableSlot Slot, ObjectRuntime Value) : TableFact;
    internal sealed record ComponentCase(TableKind Kind, int Tally) : TableFact;
    internal sealed record UndoCase(uint Serial) : TableFact;
    internal sealed record CustomUndoCase(string Name) : TableFact;
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct TableReceipt : IDetachedDocumentResult {
    private readonly Seq<TableFact> facts;

    private TableReceipt(Seq<TableFact> facts) => this.facts = facts;

    public static TableReceipt Empty { get; } = new(facts: Seq<TableFact>());

    public static TableReceipt operator +(TableReceipt left, TableReceipt right) =>
        new(facts: left.facts + right.facts);

    internal static TableReceipt Objects(TableSlot slot, Seq<ObjectRuntime> values) =>
        new(facts: values
            .Filter(static value => value is { Id: var id, Serial: > 0u } && id != Guid.Empty)
            .DistinctBy(static value => value.Id)
            .Map(value => (TableFact)new TableFact.ObjectCase(Slot: slot, Value: value)));

    internal static TableReceipt Component(TableKind kind, int tally) =>
        tally >= 0
            ? new(facts: Seq<TableFact>(new TableFact.ComponentCase(Kind: kind, Tally: tally)))
            : Empty;

    internal static TableReceipt Undo(uint serial) =>
        serial > 0u
            ? new(facts: Seq<TableFact>(new TableFact.UndoCase(Serial: serial)))
            : Empty;

    internal static TableReceipt CustomUndo(Seq<string> names) =>
        new(facts: names
            .Filter(static name => !string.IsNullOrWhiteSpace(value: name))
            .Map(static name => (TableFact)new TableFact.CustomUndoCase(Name: name)));

    internal static TableReceipt SelectionDelta(Seq<ObjectRuntime> before, Seq<ObjectRuntime> after) =>
        Objects(slot: TableSlot.Selected, values: after.Filter(value => !before.Exists(item => item.Id == value.Id)))
        + Objects(slot: TableSlot.Unselected, values: before.Filter(value => !after.Exists(item => item.Id == value.Id)));

    public Seq<Guid> Ids(TableSlot slot) => Runtime(slot: slot).Map(static value => value.Id);

    public Seq<ObjectRuntime> Runtime(TableSlot slot) =>
        facts.Choose(fact => fact is TableFact.ObjectCase { Slot: var factSlot, Value: var value } && factSlot == slot
            ? Some(value)
            : Option<ObjectRuntime>.None);

    public Seq<(TableKind Kind, int Tally)> Components =>
        facts.Choose(static fact => fact is TableFact.ComponentCase component
            ? Some((component.Kind, component.Tally))
            : Option<(TableKind, int)>.None);

    public Seq<uint> UndoRecords =>
        facts.Choose(static fact => fact is TableFact.UndoCase undo
            ? Some(undo.Serial)
            : Option<uint>.None);

    public Seq<string> CustomUndoNames =>
        facts.Choose(static fact => fact is TableFact.CustomUndoCase undo
            ? Some(undo.Name)
            : Option<string>.None);

    public int Count(TableSlot slot) => Runtime(slot: slot).Count;
}
```

## [06]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]            | [OWNER]            | [FORM]                   | [ENTRY]                               |
| :-----: | :------------------- | :----------------- | :----------------------- | :------------------------------------ |
|  [01]   | document tables      | `TableKind`        | keyed behavior rows      | `ForComponentType` / `Reclaim`        |
|  [02]   | object addressing    | `TableTarget`      | ids/runtime/query union  | `Of` / `Deleted` / `Query`            |
|  [03]   | query predicates     | `TablePredicate`   | frozen predicate union   | `Tag` / `Color` / `Bounds`            |
|  [04]   | mutation program     | `TableOp`          | admitted total union     | operation factories / `Apply`         |
|  [05]   | commit scope         | `TableTransaction` | program-mode union       | `Recorded` / `Immediate` / `Navigate` |
|  [06]   | resource ingress     | `GeometryIntake`   | kernel lease composition | `Admit`                               |
|  [07]   | table commit spine   | `Tables`           | session/redraw fold      | `Commit`                              |
|  [08]   | shared undo bracket  | `UndoBracket`      | receipt-generic terminal | `Begin` / `Seal`                      |
|  [09]   | consequence evidence | `TableReceipt`     | runtime fact stream      | typed projections                     |
