# [RASM_RHINO_TABLES]

`Rasm.Rhino.Document` owns document-table vocabulary, object addressing, mutation programs, undo/redraw bracketing, and consequence evidence. `TableKind` captures each admitted host document table, `TableTarget` freezes explicit, runtime, and host-query addressing, and `TableOp` closes the mutation family. `Tables.Commit` executes one admitted program inside one session capability window, refreshes the kernel `Context`, seals every undo exit, compensates redraw state on every outcome, and returns one typed fact stream.

## [01]-[INDEX]

- [02]-[TABLE_VOCABULARY]: `TableKind` — document-table identity, component correspondence, and reclamation behavior.
- [03]-[TARGET_ALGEBRA]: `ObjectKind`/`ObjectKinds`, `ActiveSpaceUse`, `ObjectRuntime`, `QuerySpec`, `BoundsMatch`, `TablePredicate`, `TableTarget`, and `ViewportTarget` — object-type vocabulary, immutable object addressing, viewport addressing, and query composition.
- [04]-[TRANSACTION_RAIL]: mutation policy rows, `NamedRestore`, `HistoryRoll`, `TableOp`, `TableTransaction`, `GeometryIntake`, and `Tables`.
- [05]-[RECEIPTS]: `TableSlot`, `TableFact`, and `TableReceipt` — one runtime-addressable consequence stream — beside `IFactSlot<TBody>`/`Fact<TSlot, TBody>`/`FactStream<TSlot, TBody>`, the parameterized stream every mutation folder composes.
- [06]-[SURFACE_LEDGER]: the page owner map.

## [02]-[TABLE_VOCABULARY]

- Owner: `TableKind` `[SmartEnum<int>]` binds each admitted document table to its `ModelComponentType` and table-owned reclamation delegate.
- Entry: `ForComponentType(ModelComponentType) : Fin<Seq<TableKind>>` returns every mapped row, expands `ModelComponentType.Mixed` across every explicit correspondence, treats `ModelComponentType.Unset` as absent correspondence, and rejects an undefined foreign ordinal. `Reclaim(RhinoDoc, Op) : Fin<int>` invokes the row delegate and rejects a table with no host reclamation member.
- Law: table behavior resides on the row. A table extension declares component correspondence and reclamation behavior at construction, so no external dictionary or accessibility flag can drift from the vocabulary.
- Law: `ModelComponentType.Unset` is the ONE row-side sentinel for absent correspondence, so the expansion arm reads as "every row that has one" and a lookup never manufactures a row it cannot also expand; `Mixed` is a QUERY argument alone and never a row value, because a row carrying it would be excluded by name from its own expansion and unreachable by lookup — an inert correspondence column no input returns.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
// `Rasm.Numerics` carries `Dimension` and `Rhino.Geometry` carries a type of the same simple name, so every
// `Dimension` on this page spells `Rasm.Numerics.Dimension` in full — the alias row the branch rulings own resolves
// the collision at the project level and the full spelling states which one at each use.
using System.Collections.Frozen;
using System.Globalization;
using System.Threading;
using LanguageExt.UnsafeValueAccess;
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
    public static readonly TableKind Manifest = new(key: 1, componentType: ModelComponentType.Unset, reclaim: NoReclaim);
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
    public static readonly TableKind SectionStyles = new(key: 18, componentType: ModelComponentType.SectionStyle, reclaim: NoReclaim);
    public static readonly TableKind Markups = new(key: 19, componentType: ModelComponentType.Markup, reclaim: NoReclaim);
    public static readonly TableKind PageViewGroups = new(key: 20, componentType: ModelComponentType.PageViewGroup, reclaim: NoReclaim);
    public static readonly TableKind RenderMaterials = new(key: 21, componentType: ModelComponentType.RenderContent, reclaim: NoReclaim);
    public static readonly TableKind RenderEnvironments = new(key: 22, componentType: ModelComponentType.RenderContent, reclaim: NoReclaim);
    public static readonly TableKind RenderTextures = new(key: 23, componentType: ModelComponentType.RenderContent, reclaim: NoReclaim);

    public ModelComponentType ComponentType { get; }

    [UseDelegateFromConstructor]
    internal partial Fin<int> Reclaim(RhinoDoc document, Op key);

    public static Fin<Seq<TableKind>> ForComponentType(ModelComponentType type, Op? key = null) =>
        Enum.IsDefined(value: type)
            ? Fin.Succ(value: type switch {
                ModelComponentType.Unset => Seq<TableKind>(),
                ModelComponentType.Mixed => Items.AsIterable()
                    .Filter(static kind => kind.ComponentType is not ModelComponentType.Unset)
                    .ToSeq(),
                _ => Items.AsIterable().Filter(kind => kind.ComponentType == type).ToSeq(),
            })
            : Fin.Fail<Seq<TableKind>>(error: key.OrDefault().InvalidInput());

    private static Fin<int> Count(int value, Op key) =>
        guard(value >= 0, key.InvalidResult()).ToFin().Map(_ => value);

    private static Fin<int> NoReclaim(RhinoDoc document, Op key) =>
        Fin.Fail<int>(error: key.Unsupported(geometryType: typeof(TableKind), outputType: typeof(int)));
}
```

## [03]-[TARGET_ALGEBRA]

- Owner: `ObjectKind` `[SmartEnum<ObjectType>]` is the corpus-wide OBJECT-TYPE vocabulary and `ObjectKinds` its admitted set, seated here because the concept has consumers at S1 (`Commands` modal object asks) and S2 (`HostUi` properties-page scope) and this spine is the lowest stratum both reach; `Mask` is the one OR-fold, `Any` the host's own catch-all row, and no folder mints a second type table.
- Law: a raw `ObjectType` never crosses a public signature — every filter is `ObjectKinds`, every host member taking the flag reads `Mask` at its own call, and a page needing "every type" composes `ObjectKinds.Any` rather than spelling `ObjectType.AnyObject`.
- Owner: `ActiveSpaceUse` `[SmartEnum<ActiveSpace>]` is the space partition, seated here beside the enumerator's own `SpaceFilter` because an attribute set carries it at S2 and a conduit criterion and a gumball seat read it at S4; the roster mirrors the host enum completely, so `Get` is total over any value a host read returns and the row's `Key` is the one write.
- Owner: `ObjectRuntime` `[ComplexValueObject]` admits the durable `(Guid, runtime serial)` pair required after an object leaves the active-id index. `QuerySpec` `[ComplexValueObject]` IS the complete twenty-one-axis `ObjectEnumeratorSettings` product as an admitted value and BUILDS the host settings inside the document callback, so the mutable host settings type never crosses a signature, every host sentinel is an `Option` whose absence reads as "every row", and the live `ViewportFilter` slot is the stable `ViewportTarget` owner. `TableTarget` `[Union]` closes nonempty explicit ids, nonempty runtime pairs, and admitted queries. `TablePredicate` `[Union]` adds composable tag, draw-color, and kernel-bounds predicates; `BoundsMatch` owns containment versus intersection behavior as rows.
- Owner: `ViewportTarget` `[Union]` is the corpus-wide VIEWPORT address — active, named, id, page, detail, and census cases closed as one owner beside `TableKind` (which table), `TableTarget` (which objects), and `ResourceRef` (which component). `ViewportScope` `[SmartEnum<int>]` carries the model, page, and detail census generators and `EveryCase` freezes their set; `ViewportRef` is the ephemeral resolved row pairing `RhinoView`, `RhinoViewport`, and an optional `DetailViewObject`. `Active`/`Named`/`Id`/`Page`/`Detail`/`Every` construct, and `Resolve`, `ResolveOne`, and `ResolveViewport` fold one address to every row, exactly one row, or one native viewport inside the caller's document callback.
- Law: viewport resolution names `RhinoDoc.Views.ActiveView`, `.Find`, `.GetViewList`, `.GetPageViews`, `RhinoPageView.GetDetailViews`, and `DetailViewObject.Viewport` exactly once; a detail address matches either `DetailViewObject.Id` or `DetailViewObject.Viewport.Id`, and a resolution yielding no row refuses before any consumer projects it.
- Law: an addressed row binds `RhinoView.MainViewport` — the viewport the address names — because `RhinoPageView.ActiveViewport` silently returns an active detail; only `ActiveCase` binds `ActiveViewport`, adopting the host's active semantics, and a detail row binds `DetailViewObject.Viewport` and carries its `DetailViewObject` so a detail commit or scale conversion reads the owning object without a second lookup.
- Law: viewport rows resolve live per call inside the document callback and leave as detached addresses or one native viewport, never a retained handle; `ResolveViewport` composes `Resolve` and `Tables.One`, so the single-viewport consumers — `QuerySpec` viewport filtering, `NamedRestore`, and the annotation dimension-scale probe — share one fold and no call site re-spells the resolve-then-one triple.
- Owner: `ResourceRef` is the corpus-wide COMPONENT address — id, name, index closed as one `[Union]` over a per-table `ResourceLens<TComponent>` — completing the addressing triad beside `TableKind` (which table) and `TableTarget` (which objects); `ResourceId`, `ResourceName`, and `ResourceIndex` admit the native address scalars once, and the `ResourceId` and `ResourceIndex` `Maybe`/`Admit` pairs are the sole `Guid.Empty` and negative-index sentinel projectors — `Maybe` where the host miss value spells a normal absence, `Admit` where it is a genuine refusal.
- Law: each component table contributes exactly one lens — Annotation's style, linetype, hatch, and section rails and Blocks' definition rail each declare one `ResourceLens<T>` row — and no folder mints a second address family; resolution reads live per call inside the owning operation, because tables mutate under commands, so no resolved component is cached on a value.
- Entry: `QuerySpec.Of(...)`, `TableTarget.Of(params ReadOnlySpan<Guid>)`, `Deleted(params ReadOnlySpan<ObjectRuntime>)`, and `Query(QuerySpec, params ReadOnlySpan<TablePredicate>)` are the only constructors. `Resolve` returns distinct ids; `Serials` preserves or resolves runtime pairs. `ObjectRuntime.Canonical` derives every runtime-pair deduplication from generated structural equality. A deleted-object lifecycle request composes `Deleted` from a prior receipt instead of attempting `FindId`, which cannot find deleted objects.
- Law: query settings are BUILT at execution from an admitted value, never copied from a caller's instance — the host settings object exists only inside `QuerySpec.Build`, so no caller retains a handle that can mutate an admitted target, the viewport resolves from stable identity inside the document callback, and every one of the host's twenty-one filter axes is a stated column. Predicate evaluation accumulates independent object and predicate faults through `Validation<Error, T>` before lowering once to `Fin<T>`.
- Law: a predicate distinguishes NON-MATCH from HOST FAULT — a missing tag is a non-match, a missing attribute set is a refusal, because folding both onto `false` drops an unreadable object out of every filtered query with no receipt naming it. Draw-colour comparison lands on the quantized ARGB quadruple of two `PerceptualColor` values: `System.Drawing.Color` equality compares NAME before value, so a named row and its identical literal compare unequal, which is the trap a colour filter walks into on the first system colour.
- Law: bounds predicates admit `BoundingBox.IsValid` before corner accumulation and compose the kernel `BoundsOf` owner. Containment and intersection derive from catalogued `Center` and `Diagonal` evidence; inflation remains host-query policy, while candidate classification and coercion stay kernel-owned.
- Boundary: `BoundingBox.Inflate` mutates a copied struct, so `Inflated` is the one statement kernel and never mutates request evidence.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<ObjectType>]
public sealed partial class ObjectKind {
    public static readonly ObjectKind Point = new(key: ObjectType.Point);
    public static readonly ObjectKind PointSet = new(key: ObjectType.PointSet);
    public static readonly ObjectKind Curve = new(key: ObjectType.Curve);
    public static readonly ObjectKind Surface = new(key: ObjectType.Surface);
    public static readonly ObjectKind Brep = new(key: ObjectType.Brep);
    public static readonly ObjectKind Mesh = new(key: ObjectType.Mesh);
    public static readonly ObjectKind Light = new(key: ObjectType.Light);
    public static readonly ObjectKind Annotation = new(key: ObjectType.Annotation);
    public static readonly ObjectKind InstanceDefinition = new(key: ObjectType.InstanceDefinition);
    public static readonly ObjectKind InstanceReference = new(key: ObjectType.InstanceReference);
    public static readonly ObjectKind TextDot = new(key: ObjectType.TextDot);
    public static readonly ObjectKind Grip = new(key: ObjectType.Grip);
    public static readonly ObjectKind Detail = new(key: ObjectType.Detail);
    public static readonly ObjectKind Hatch = new(key: ObjectType.Hatch);
    public static readonly ObjectKind MorphControl = new(key: ObjectType.MorphControl);
    public static readonly ObjectKind SubD = new(key: ObjectType.SubD);
    public static readonly ObjectKind BrepLoop = new(key: ObjectType.BrepLoop);
    public static readonly ObjectKind BrepVertex = new(key: ObjectType.BrepVertex);
    public static readonly ObjectKind Polysurface = new(key: ObjectType.PolysrfFilter);
    public static readonly ObjectKind Edge = new(key: ObjectType.EdgeFilter);
    public static readonly ObjectKind Polyedge = new(key: ObjectType.PolyedgeFilter);
    public static readonly ObjectKind MeshVertex = new(key: ObjectType.MeshVertex);
    public static readonly ObjectKind MeshEdge = new(key: ObjectType.MeshEdge);
    public static readonly ObjectKind MeshFace = new(key: ObjectType.MeshFace);
    public static readonly ObjectKind Cage = new(key: ObjectType.Cage);
    public static readonly ObjectKind Phantom = new(key: ObjectType.Phantom);
    public static readonly ObjectKind ClipPlane = new(key: ObjectType.ClipPlane);
    public static readonly ObjectKind Extrusion = new(key: ObjectType.Extrusion);
    // The host's own catch-all bit, not the OR of the rows above: a filter meaning "every type" reads this row, so no
    // caller re-derives an all-mask that silently omits a type the roster has not yet named.
    public static readonly ObjectKind AnyObject = new(key: ObjectType.AnyObject);
}

[ComplexValueObject]
public sealed partial class ObjectKinds {
    public FrozenSet<ObjectKind> Values { get; }

    // `ObjectType` is a flag enum, so a kind set IS its OR-fold; the mask never leaves this owner as a raw host value
    // except at a host member that takes one, and no caller re-derives the fold.
    internal ObjectType Mask => toSeq(Values).Fold(ObjectType.None, static (mask, kind) => mask | kind.Key);

    public static ObjectKinds Any { get; } = Create(values: FrozenSet.ToFrozenSet([ObjectKind.AnyObject]));

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref FrozenSet<ObjectKind> values) =>
        validationError = values is null || values.Count is 0 || values.Any(static kind => kind is null)
            ? new ValidationError(message: "Object kind set is empty.")
            : null;

    // A host row answers its type as a FLAG WORD, so decomposition is the read counterpart of `Mask` and a
    // single-row lookup is the wrong shape: it picks one bit and drops every other the object actually carries.
    // `AnyObject` is the host's own catch-all bit and matches every row, so it is admitted only when the word IS
    // that bit — otherwise a two-bit word would answer "every type" plus its two real rows.
    public static Fin<ObjectKinds> OfMask(ObjectType mask, Op? key = null) {
        Op op = key.OrDefault();
        Seq<ObjectKind> rows = mask == ObjectType.AnyObject
            ? Seq(ObjectKind.AnyObject)
            : Items.AsIterable()
                .Filter(row => row.Key != ObjectType.AnyObject && (mask & row.Key) == row.Key && row.Key != ObjectType.None)
                .ToSeq();
        return op.AcceptValidated<ObjectKinds>(
            fault: Validate(rows.ToFrozenSet(), out ObjectKinds? admitted),
            admitted: admitted);
    }

    public static Fin<ObjectKinds> Of(Op? key, params ReadOnlySpan<ObjectKind> values) {
        Op op = key.OrDefault();
        return op.AcceptValidated<ObjectKinds>(
            fault: Validate(toSeq(values.ToArray()).ToFrozenSet(), out ObjectKinds? admitted),
            admitted: admitted);
    }
}

// `ActiveSpace` is the document's own space partition — `ObjectEnumeratorSettings.SpaceFilter` takes it here, an
// object attribute set carries it at S2, and a conduit criterion and a gumball seat read it at S4 — so the keyed
// vocabulary seats on this spine and no folder mints a second one.
[SmartEnum<ActiveSpace>]
public sealed partial class ActiveSpaceUse {
    public static readonly ActiveSpaceUse None = new(key: ActiveSpace.None);
    public static readonly ActiveSpaceUse Model = new(key: ActiveSpace.ModelSpace);
    public static readonly ActiveSpaceUse Page = new(key: ActiveSpace.PageSpace);
    public static readonly ActiveSpaceUse UvEditor = new(key: ActiveSpace.UVEditorSpace);
    public static readonly ActiveSpaceUse BlockEditor = new(key: ActiveSpace.BlockEditorSpace);
}

[ComplexValueObject]
public sealed partial class ObjectRuntime {
    public Guid Id { get; }
    public uint Serial { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Guid id, ref uint serial) =>
        validationError = id == Guid.Empty || serial is 0u
            ? new ValidationError(message: "Object runtime identity is incomplete.")
            : null;

    internal static Fin<ObjectRuntime> Of(Guid id, uint serial, Op? key = null) =>
        key.OrDefault().AcceptValidated<ObjectRuntime>(
            fault: Validate(id, serial, out ObjectRuntime? admitted),
            admitted: admitted);

    internal static Seq<ObjectRuntime> Canonical(Seq<ObjectRuntime> values) => values.Distinct();
}

// The complete `ObjectEnumeratorSettings` product as a VALUE. The prior shape took the host settings object from
// the caller and copied it — so the public signature carried a mutable host type, the caller could hold the same
// instance and keep mutating it, and every default (`LayerIndexFilter = -1`, `MaterialIndexFilter` at its own
// sentinel, `NameFilter = "*"`) was the host's private business no admitted value ever stated. `QuerySpec` BUILDS
// the settings instead: every one of the twenty-one host axes is a column, each sentinel is an `Option` whose
// absence means "every row", the type filter is `ObjectKinds`, the index filters are `ResourceIndex`, and the
// viewport axis is the stable `ViewportTarget` resolved inside the document callback.
[ComplexValueObject]
public sealed partial class QuerySpec {
    // --- [STATE]
    public bool Normal { get; }
    public bool Locked { get; }
    public bool Hidden { get; }
    public bool InDefinitions { get; }
    public bool Deleted { get; }
    // --- [CATEGORY]
    public bool Active { get; }
    public bool Referenced { get; }
    // --- [INCLUSION]
    public bool Lights { get; }
    public bool Grips { get; }
    public bool Phantoms { get; }
    // --- [SELECTION]
    public bool SubObjectSelected { get; }
    public bool SelectedOnly { get; }
    public bool FastSelection { get; }
    public bool VisibleOnly { get; }
    // --- [FILTERS]
    public ObjectKinds Kinds { get; }
    public Option<Type> Shape { get; }
    public Option<ResourceIndex> Layer { get; }
    public Option<ResourceIndex> Material { get; }
    public Option<string> Name { get; }
    public ActiveSpaceUse Space { get; }
    public Option<ViewportTarget> Viewport { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref bool normal,
        ref bool locked,
        ref bool hidden,
        ref bool inDefinitions,
        ref bool deleted,
        ref bool active,
        ref bool referenced,
        ref bool lights,
        ref bool grips,
        ref bool phantoms,
        ref bool subObjectSelected,
        ref bool selectedOnly,
        ref bool fastSelection,
        ref bool visibleOnly,
        ref ObjectKinds kinds,
        ref Option<Type> shape,
        ref Option<ResourceIndex> layer,
        ref Option<ResourceIndex> material,
        ref Option<string> name,
        ref ActiveSpaceUse space,
        ref Option<ViewportTarget> viewport) =>
        validationError = kinds is null
            || space is null
            || name.Exists(static value => string.IsNullOrWhiteSpace(value: value))
            // The host ignores `UseFastSelection` unless `SelectedObjectsFilter` is set, and its own remark bars it
            // while the selection is changing. The dependency is a construction fact here, so a spec cannot carry a
            // knob the host will silently drop.
            || (fastSelection && !selectedOnly)
            || !(normal || locked || hidden || inDefinitions || deleted)
            || !(active || referenced)
            ? new ValidationError(message: "Object query spec is incomplete.")
            : null;

    // The host constructor's own posture: normal-or-locked, active, every type, every layer, every material, every
    // name. It is the default here so a caller states only what it narrows.
    public static Fin<QuerySpec> Of(
        Option<bool> normal = default,
        Option<bool> locked = default,
        Option<bool> hidden = default,
        Option<bool> inDefinitions = default,
        Option<bool> deleted = default,
        Option<bool> active = default,
        Option<bool> referenced = default,
        Option<bool> lights = default,
        Option<bool> grips = default,
        Option<bool> phantoms = default,
        Option<bool> subObjectSelected = default,
        Option<bool> selectedOnly = default,
        Option<bool> fastSelection = default,
        Option<bool> visibleOnly = default,
        Option<ObjectKinds> kinds = default,
        Option<Type> shape = default,
        Option<ResourceIndex> layer = default,
        Option<ResourceIndex> material = default,
        Option<string> name = default,
        Option<ActiveSpaceUse> space = default,
        Option<ViewportTarget> viewport = default,
        Op? key = null) =>
        Admission.Admitted(
            fault: Validate(
                normal.IfNone(noneValue: true),
                locked.IfNone(noneValue: true),
                hidden.IfNone(noneValue: false),
                inDefinitions.IfNone(noneValue: false),
                deleted.IfNone(noneValue: false),
                active.IfNone(noneValue: true),
                referenced.IfNone(noneValue: false),
                lights.IfNone(noneValue: false),
                grips.IfNone(noneValue: false),
                phantoms.IfNone(noneValue: false),
                subObjectSelected.IfNone(noneValue: false),
                selectedOnly.IfNone(noneValue: false),
                fastSelection.IfNone(noneValue: false),
                visibleOnly.IfNone(noneValue: false),
                kinds.IfNone(ObjectKinds.Any),
                shape,
                layer,
                material,
                name,
                space.IfNone(ActiveSpaceUse.None),
                viewport,
                out QuerySpec? admitted),
            value: admitted,
            refusal: key.OrDefault().InvalidInput());

    internal bool SelectsOnlyDeleted => Deleted && !Normal && !Locked && !Hidden && !InDefinitions;

    // The one place a host settings object exists, minted fresh per execution inside the document callback: the
    // viewport resolves live, the absent filters write the host's own "everything" sentinels explicitly rather
    // than relying on a default the value never stated, and nothing the caller holds can reach the result.
    internal Fin<ObjectEnumeratorSettings> Build(RhinoDoc document, Op key) =>
        Viewport
            .Traverse(target => target.ResolveViewport(document: document, key: key))
            .As()
            .Map(resolved => new ObjectEnumeratorSettings {
                NormalObjects = Normal,
                LockedObjects = Locked,
                HiddenObjects = Hidden,
                IdefObjects = InDefinitions,
                DeletedObjects = Deleted,
                ActiveObjects = Active,
                ReferenceObjects = Referenced,
                IncludeLights = Lights,
                IncludeGrips = Grips,
                IncludePhantoms = Phantoms,
                SubObjectSelected = SubObjectSelected,
                SelectedObjectsFilter = SelectedOnly,
                UseFastSelection = FastSelection,
                VisibleFilter = VisibleOnly,
                ObjectTypeFilter = Kinds.Mask,
                ClassTypeFilter = Shape.IfNone(defaultValue: null!),
                LayerIndexFilter = Layer.Map(static value => value.Value).IfNone(noneValue: AnyIndex),
                MaterialIndexFilter = Material.Map(static value => value.Value).IfNone(noneValue: AnyMaterial),
                NameFilter = Name.IfNone(AnyName),
                SpaceFilter = Space.Key,
                ViewportFilter = resolved.IfNone(defaultValue: null!),
            });

    // The host's own "no filter" sentinels, named once so no arm re-spells a magic number.
    private const int AnyIndex = -1;
    private const int AnyMaterial = int.MinValue + 1;
    private const string AnyName = "*";
}

[SmartEnum]
public sealed partial class BoundsMatch {
    public static readonly BoundsMatch Intersects = new(static (region, candidate) =>
        Math.Abs(region.Center.X - candidate.Center.X) * 2.0 <= region.Diagonal.X + candidate.Diagonal.X
        && Math.Abs(region.Center.Y - candidate.Center.Y) * 2.0 <= region.Diagonal.Y + candidate.Diagonal.Y
        && Math.Abs(region.Center.Z - candidate.Center.Z) * 2.0 <= region.Diagonal.Z + candidate.Diagonal.Z);
    public static readonly BoundsMatch Contains = new(static (region, candidate) =>
        Math.Abs(region.Center.X - candidate.Center.X) * 2.0 + candidate.Diagonal.X <= region.Diagonal.X
        && Math.Abs(region.Center.Y - candidate.Center.Y) * 2.0 + candidate.Diagonal.Y <= region.Diagonal.Y
        && Math.Abs(region.Center.Z - candidate.Center.Z) * 2.0 + candidate.Diagonal.Z <= region.Diagonal.Z);

    [UseDelegateFromConstructor]
    internal partial bool Test(BoundingBox region, BoundingBox candidate);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TablePredicate {
    private TablePredicate() { }

    private sealed record TagCase(string Key, Option<string> Expected) : TablePredicate;
    private sealed record ColorCase(PerceptualColor Value) : TablePredicate;
    private sealed record BoundsCase(BoundingBox Region, BoundsMatch Match) : TablePredicate;

    public static Fin<TablePredicate> Tag(string name, Option<string> expected = default, Op? key = null) =>
        key.OrDefault().AcceptText(value: name).Map(valid => (TablePredicate)new TagCase(Key: valid, Expected: expected));

    public static Fin<TablePredicate> Color(PerceptualColor value, Op? key = null) =>
        key.OrDefault().Need(value).Map(static admitted => (TablePredicate)new ColorCase(Value: admitted));

    public static Fin<TablePredicate> Bounds(BoundingBox region, BoundsMatch match, double inflation = 0.0, Op? key = null) {
        Op op = key.OrDefault();
        return from _ in guard(region.IsValid, op.InvalidInput()).ToFin()
               from predicate in (
                op.Need(match).ToValidation(),
                op.Catch(() => Fin.Succ(value: toSeq(region.GetCorners())))
                    .Bind(corners =>
                        from counted in guard(corners.Count is 8, op.InvalidInput()).ToFin()
                        from admitted in corners
                            .Traverse(point => op.AcceptInput(value: point).ToValidation())
                            .As()
                            .ToFin()
                        select admitted)
                    .ToValidation(),
                guard(double.IsFinite(inflation) && inflation >= 0.0, op.InvalidInput())
                    .ToFin()
                    .ToValidation())
                   .Apply((relation, _, _) => (TablePredicate)new BoundsCase(
                       Region: Inflated(region: region, amount: inflation),
                       Match: relation))
                   .As()
                   .ToFin()
               select predicate;
    }

    // A missing attribute set is a HOST FAULT, not a non-match: the object exists, so its attributes must. Folding
    // both onto `false` made an unreadable object silently drop out of every filtered query and no receipt named it.
    // Absence of the TAG is the real non-match and stays `false`; absence of the attribute set fails.
    internal Fin<bool> Match(RhinoDoc document, RhinoObject native, Op key) =>
        Switch(
            state: (Document: document, Native: native, Op: key),
            tagCase: static (context, predicate) => Optional(context.Native.Attributes)
                .ToFin(Fail: context.Op.InvalidResult())
                .Map(attributes => Optional(attributes.GetUserString(key: predicate.Key))
                    .Map(stored => predicate.Expected
                        .Map(expected => string.Equals(a: stored, b: expected, comparisonType: StringComparison.Ordinal))
                        .IfNone(noneValue: true))
                    .IfNone(noneValue: false)),
            // `System.Drawing.Color` equality compares NAME before value, so a named row and its literal ARGB twin
            // compare unequal — the trap a draw-colour filter walks into every time. The kernel colour is the
            // filter's identity and the comparison lands on the quantized ARGB quadruple both sides agree on.
            colorCase: static (context, predicate) => Optional(context.Native.Attributes)
                .ToFin(Fail: context.Op.InvalidResult())
                .Bind(attributes => Shade(color: attributes.DrawColor(document: context.Document), key: context.Op))
                .Map(drawn => drawn.ToRgb() == predicate.Value.ToRgb()),
            boundsCase: static (context, predicate) => Optional(context.Native.Geometry)
                .ToFin(Fail: context.Op.InvalidResult())
                .Bind(geometry => geometry.BoundsOf(key: context.Op))
                .Map(candidate => predicate.Match.Test(region: predicate.Region, candidate: candidate)));

    private static BoundingBox Inflated(BoundingBox region, double amount) {
        BoundingBox expanded = region;
        _ = Op.SideWhen(amount > 0.0, () => expanded.Inflate(xAmount: amount, yAmount: amount, zAmount: amount));
        return expanded;
    }

    // The spine's ONE host-colour crossing. `System.Drawing.Color` reaches this page only as the byte quadruple a
    // host read answers with; it never becomes a stored column and never crosses a public signature.
    internal static Fin<PerceptualColor> Shade(System.Drawing.Color color, Op key) =>
        PerceptualColor.OfRgb(color.R, color.G, color.B, alpha: color.A, key: key);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TableTarget {
    private TableTarget() { }

    private sealed record IdsCase(Seq<Guid> Values) : TableTarget;
    private sealed record RuntimeCase(Seq<ObjectRuntime> Values) : TableTarget;
    private sealed record QueryCase(QuerySpec Spec, Seq<TablePredicate> Predicates) : TableTarget;

    public static Fin<TableTarget> Of(params ReadOnlySpan<Guid> ids) {
        Op op = Op.Of();
        return from values in toSeq(ids.ToArray())
                   .Traverse(id => (id != Guid.Empty
                       ? Fin.Succ(value: id)
                       : Fin.Fail<Guid>(error: op.InvalidInput())).ToValidation())
                   .As()
                   .ToFin()
               let distinct = values.Distinct()
               from _ in guard(!distinct.IsEmpty, op.InvalidInput()).ToFin()
               select (TableTarget)new IdsCase(Values: distinct);
    }

    public static Fin<TableTarget> Deleted(params ReadOnlySpan<ObjectRuntime> values) {
        Op op = Op.Of();
        return from admitted in Admission.All(values: values, key: op)
               let distinct = ObjectRuntime.Canonical(values: admitted)
               from _ in guard(!distinct.IsEmpty, op.InvalidInput()).ToFin()
               select (TableTarget)new RuntimeCase(Values: distinct);
    }

    public static Fin<TableTarget> Query(QuerySpec spec, params ReadOnlySpan<TablePredicate> predicates) {
        Op op = Op.Of();   // an optional before `params` forecloses the positional spread — the key mints at the entry
        return (
                op.Need(spec).ToValidation(),
                Admission.All(values: predicates, key: op).ToValidation())
            .Apply(static (source, filters) => (TableTarget)new QueryCase(
                Spec: source,
                Predicates: filters))
            .As()
            .ToFin();
    }

    internal bool RetainsRuntime => Switch(
        idsCase: static _ => false,
        runtimeCase: static _ => true,
        queryCase: static target => target.Spec.SelectsOnlyDeleted);

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
                .Bind(rows => rows
                    .Traverse(native => ObjectRuntime.Of(
                        id: native.Id,
                        serial: native.RuntimeSerialNumber,
                        key: context.Op).ToValidation())
                    .As()
                    .ToFin()));

    private static Fin<Seq<RhinoObject>> Evaluate(QueryCase target, RhinoDoc document, Op key) =>
        from settings in target.Spec.Build(document: document, key: key)
        from objects in Optional(document.Objects.GetObjectList(settings: settings))
            .ToFin(Fail: key.InvalidResult())
            .Map(static values => toSeq(values))
        from matches in objects
            .Traverse(native => target.Predicates
                .Traverse(predicate => predicate.Match(document: document, native: native, key: key).ToValidation())
                .As()
                .Map(verdicts => (Native: native, Matches: verdicts.ForAll(identity))))
            .As()
            .ToFin()
        select matches.Filter(static match => match.Matches).Map(static match => match.Native);
}

// --- [VIEWPORT_ADDRESS]
[SmartEnum<int>]
public sealed partial class ViewportScope {
    public static readonly ViewportScope Model = new(
        key: 0,
        select: static document => toSeq(document.Views.GetViewList(filter: ViewTypeFilter.Model))
            .Map(static view => ViewportRef.Of(view: view)));
    public static readonly ViewportScope Pages = new(
        key: 1,
        select: static document => toSeq(document.Views.GetPageViews())
            .Map(static page => ViewportRef.Of(view: page)));
    public static readonly ViewportScope Details = new(
        key: 2,
        select: static document => toSeq(document.Views.GetPageViews())
            .Bind(static page => toSeq(page.GetDetailViews())
                .Map(detail => ViewportRef.OfDetail(view: page, detail: detail))));

    [UseDelegateFromConstructor]
    internal partial Seq<ViewportRef> Select(RhinoDoc document);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ViewportTarget {
    private ViewportTarget() { }
    internal sealed record ActiveCase : ViewportTarget;
    internal sealed record NamedCase(string Name) : ViewportTarget;
    internal sealed record IdCase(Guid ViewportId) : ViewportTarget;
    internal sealed record PageCase(Guid PageViewId) : ViewportTarget;
    internal sealed record DetailCase(Guid PageViewId, Guid DetailId) : ViewportTarget;
    internal sealed record EveryCase(FrozenSet<ViewportScope> Scopes) : ViewportTarget;

    public static ViewportTarget Active { get; } = new ActiveCase();

    public static Fin<ViewportTarget> Every(ReadOnlySpan<ViewportScope> scopes, Op? key = null) {
        Seq<ViewportScope> rows = toSeq(scopes.ToArray()).Strict();
        return guard(!rows.IsEmpty && rows.ForAll(static scope => scope is not null), key.OrDefault().InvalidInput()).ToFin()
            .Map(_ => (ViewportTarget)new EveryCase(Scopes: rows.ToFrozenSet()));
    }
    public static Fin<ViewportTarget> Named(string name, Op? key = null) =>
        key.OrDefault().AcceptText(value: name).Map(static valid => (ViewportTarget)new NamedCase(Name: valid));
    public static Fin<ViewportTarget> Id(Guid viewportId, Op? key = null) =>
        guard(viewportId != Guid.Empty, key.OrDefault().InvalidInput()).ToFin().Map(_ => (ViewportTarget)new IdCase(ViewportId: viewportId));
    public static Fin<ViewportTarget> Page(Guid pageViewId, Op? key = null) =>
        guard(pageViewId != Guid.Empty, key.OrDefault().InvalidInput()).ToFin().Map(_ => (ViewportTarget)new PageCase(PageViewId: pageViewId));
    public static Fin<ViewportTarget> Detail(Guid pageViewId, Guid detailId, Op? key = null) =>
        guard(pageViewId != Guid.Empty && detailId != Guid.Empty, key.OrDefault().InvalidInput()).ToFin()
            .Map(_ => (ViewportTarget)new DetailCase(PageViewId: pageViewId, DetailId: detailId));

    internal Fin<Seq<ViewportRef>> Resolve(RhinoDoc document, Op key) =>
        Switch(
            (Document: document, Op: key),
            activeCase: static (ctx, _) =>
                Optional(ctx.Document.Views.ActiveView).ToFin(Fail: ctx.Op.MissingContext())
                    .Map(view => Seq(ViewportRef.OfActive(view: view))),
            namedCase: static (ctx, target) =>
                Optional(ctx.Document.Views.Find(mainViewportName: target.Name, compareCase: false))
                    .ToFin(Fail: ctx.Op.InvalidInput())
                    .Map(view => Seq(ViewportRef.Of(view: view))),
            idCase: static (ctx, target) => (
                    Optional(ctx.Document.Views.Find(mainViewportId: target.ViewportId))
                        .Map(static view => ViewportRef.Of(view: view))
                    | toSeq(ctx.Document.Views.GetPageViews())
                        .Bind(static page => toSeq(page.GetDetailViews())
                            .Map(detail => ViewportRef.OfDetail(view: page, detail: detail)))
                        .Find(row => row.Viewport.Id == target.ViewportId
                            || row.Detail.Exists(detail => detail.Id == target.ViewportId))
                ).ToFin(Fail: ctx.Op.InvalidInput()).Map(static row => Seq(row)),
            pageCase: static (ctx, target) =>
                PageOf(document: ctx.Document, pageViewId: target.PageViewId, key: ctx.Op)
                    .Map(page => Seq(ViewportRef.Of(view: page))),
            detailCase: static (ctx, target) =>
                from page in PageOf(document: ctx.Document, pageViewId: target.PageViewId, key: ctx.Op)
                from detail in toSeq(page.GetDetailViews())
                    .Find(row => row.Id == target.DetailId || row.Viewport.Id == target.DetailId)
                    .ToFin(Fail: ctx.Op.InvalidInput())
                select Seq(ViewportRef.OfDetail(view: page, detail: detail)),
            everyCase: static (ctx, target) => Fin.Succ(
                toSeq(target.Scopes)
                    .OrderBy(static scope => scope.Key)
                    .Bind(scope => scope.Select(document: ctx.Document))
                    .Strict()));

    internal Fin<ViewportRef> ResolveOne(RhinoDoc document, Op key) =>
        Resolve(document: document, key: key).Bind(rows => Tables.One(rows: rows, key: key));

    internal Fin<RhinoViewport> ResolveViewport(RhinoDoc document, Op key) =>
        ResolveOne(document: document, key: key).Map(static row => row.Viewport);

    private static Fin<RhinoPageView> PageOf(RhinoDoc document, Guid pageViewId, Op key) =>
        toSeq(document.Views.GetPageViews()).Find(page => page.MainViewport.Id == pageViewId).ToFin(Fail: key.InvalidInput());
}

internal readonly record struct ViewportRef(RhinoView View, RhinoViewport Viewport, Option<DetailViewObject> Detail) {
    internal static ViewportRef Of(RhinoView view) =>
        new(View: view, Viewport: view.MainViewport, Detail: Option<DetailViewObject>.None);
    internal static ViewportRef OfActive(RhinoView view) =>
        new(View: view, Viewport: view.ActiveViewport, Detail: Option<DetailViewObject>.None);
    internal static ViewportRef OfDetail(RhinoPageView view, DetailViewObject detail) =>
        new(View: view, Viewport: detail.Viewport, Detail: Some(detail));

    internal Fin<TOut> Info<TOut>(Func<ViewportInfo, Fin<TOut>> project, Op key) =>
        key.Catch(() => new Lease<ViewportInfo>.Owned(Value: new ViewportInfo(Viewport)).Use(project));
}

// --- [COMPONENT_ADDRESS]
[ValueObject<Guid>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct ResourceId {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Guid value) =>
        validationError = value != Guid.Empty ? null : new ValidationError(message: "ResourceId requires a non-empty value.");

    internal static Option<ResourceId> Maybe(Guid value) => Optional(value).Filter(static id => id != Guid.Empty).Map(Create);

    internal static Fin<ResourceId> Admit(Guid value, Op key) => Maybe(value).ToFin(Fail: key.InvalidResult());
}

[ValueObject<int>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct ResourceIndex {
    internal const int Absent = -1;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
        validationError = value >= 0 ? null : new ValidationError(message: "ResourceIndex requires a non-negative value.");

    internal static Option<ResourceIndex> Maybe(int value) =>
        value >= 0 ? Some(Create(value)) : Option<ResourceIndex>.None;

    internal static Fin<ResourceIndex> Admit(int value, Op key) => Maybe(value).ToFin(Fail: key.InvalidResult());
}

// The commit envelope's own scalar, seated on the spine because the envelope mints it and EVERY folder receipt
// carries it: `UndoBracket` answers `0u` for a program that opened no record, so the value object's refusal of
// zero is what keeps "no record" out of a receipt as a fact claiming record zero.
[ValueObject<uint>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct UndoSerial {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref uint value) =>
        validationError = value is 0u ? new ValidationError(message: "Undo serial must be positive.") : null;

    internal static Option<UndoSerial> Maybe(uint value) =>
        value is 0u ? Option<UndoSerial>.None : Some(Create(value));
}

[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public sealed partial class ResourceName {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim() ?? string.Empty;
        validationError = value.Length > 0 ? null : new ValidationError(message: "ResourceName requires non-blank text.");
    }
}

public sealed record ResourceLens<TComponent>(
    Func<RhinoDoc, Guid, TComponent?> ById,
    Func<RhinoDoc, string, TComponent?> ByName,
    Func<RhinoDoc, int, TComponent?> ByIndex) where TComponent : class;

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ResourceRef : IDetachedDocumentResult {
    private ResourceRef() { }
    public sealed record ById : ResourceRef { internal ById(ResourceId value) => Value = value; public ResourceId Value { get; } }
    public sealed record ByName : ResourceRef { internal ByName(ResourceName value) => Value = value; public ResourceName Value { get; } }
    public sealed record ByIndex : ResourceRef { internal ByIndex(ResourceIndex value) => Value = value; public ResourceIndex Value { get; } }

    public static Fin<ResourceRef> Of(Guid id, Op? key = null) =>
        ResourceId.Maybe(id).Map(static value => (ResourceRef)new ById(value: value))
            .ToFin(Fail: key.OrDefault(name: nameof(ResourceRef)).InvalidInput());

    public static Fin<ResourceRef> Of(string name, Op? key = null) =>
        key.OrDefault(name: nameof(ResourceRef)).AcceptText(value: name)
            .Map(static valid => (ResourceRef)new ByName(value: ResourceName.Create(valid)));

    public static Fin<ResourceRef> Of(int index, Op? key = null) =>
        ResourceIndex.Admit(value: index, key: key.OrDefault(name: nameof(ResourceRef)))
            .Map(static value => (ResourceRef)new ByIndex(value: value));

    internal Fin<TComponent> Resolve<TComponent>(RhinoDoc document, ResourceLens<TComponent> lens, Op key) where TComponent : class =>
        Switch(
            state: (Document: document, Lens: lens, Op: key),
            byId: static (ctx, address) => ctx.Op.Catch(() =>
                Optional(ctx.Lens.ById(ctx.Document, address.Value.Value)).ToFin(Fail: ctx.Op.MissingContext())),
            byName: static (ctx, address) => ctx.Op.Catch(() =>
                Optional(ctx.Lens.ByName(ctx.Document, address.Value.Value)).ToFin(Fail: ctx.Op.MissingContext())),
            byIndex: static (ctx, address) => ctx.Op.Catch(() =>
                Optional(ctx.Lens.ByIndex(ctx.Document, address.Value.Value)).ToFin(Fail: ctx.Op.MissingContext())));
}
```

## [04]-[TRANSACTION_RAIL]

- Owner: policy vocabularies close every provider-mode discriminant on rows. `HostInteraction` is the corpus-wide host-dialogue axis every folder's `quiet` argument reads. `SelectionPolicy` `[ComplexValueObject]` generates the complete highlight, grip, persistence, layer-lock, layer-visibility, light-census, and grip-census product from independent axes, so the selection receipt's population is stated rather than fixed by a literal inside the census helper. `TableOp` `[Union]` carries admitted per-occurrence payloads; `TableTransaction` `[Union]` distinguishes recorded, immediate, and navigation programs by shape, and `TransactionUndo` derives required versus recorded undo behavior without plan booleans. `UndoBracket` is the shared document transaction capsule, `RedrawScope.Within` is the one suppress/restore/success-gated-flush redraw bracket, and `DocumentCommit.Sealed` composes the two as the ONE commit entry — every folder commit rail (table, layer, session-regime, annotation draft, block, object, render content, render settings, exchange, sheet, persistence preset, user-text, capture-adopt) commits through it, and a hand-spelled `UndoBracket.Begin` or redraw triple beside it is the deleted form; the flush fires only after the prior redraw state is restored, so a suppressing policy still lands its terminal repaint.
- Entry: `TableOp` factories admit raw payloads once. `Add` and `Replace` seal heterogeneous geometry inside `GeometryIntake`; its later `Admit` stage applies the fresh kernel `Context`, `Requirement`, and `GeometryForm` lease without exposing the raw value again. `TableTransaction.Recorded`, `Immediate`, and `Navigate` admit program shape before `Tables.Commit(DocumentSession, TableTransaction)` enters the host boundary.
- Law: `TransformPolicy.Relocate` reports the transformed identity as `Moved`; `Copy` and `History` report only the minted identity as `Created`. Sources remain unchanged on copy/history paths. Selection facts derive from before/after runtime snapshots, and state facts use separate `Hidden`/`Shown` and `Locked`/`Unlocked` slots.
- Law: an operation-factory key threads through every constructor as a trailing `Op? key = null`, so one caller-minted key spans a whole program and every refusal names the request that produced it; the three `params`-bearing factories carve out and mint at the entry, because an optional before `params` forecloses the positional spread. A host member reporting failure as `Guid.Empty` — `Add`, `AddOrderedPointCloud`, `Transform` — admits through `ResourceId.Admit`, the spine's one empty-guid projector, so an empty id never enters a receipt as a created object.
- Law: `TableOp.Traits` totally classifies every case onto one of four trait rows — `Sourced`, `Recorded`, `Immediate`, `Navigation` — carrying undo recording, navigation, and kernel-context demand as one derived product. A host effect that cannot be reversed by the document record enters only an immediate transaction, so a recorded program has no untracked side effect.
- Law: `Amend` owns a duplicated `ObjectAttributes` lease, takes the admitted `AttributeChange` payload, commits the duplicate synchronously, and disposes it before the operation leaves the host boundary. `AttributeChange` is the SEAM type: `Commands`-style downward naming applies here too — this spine is S0 and the typed attribute program is S2, so the payload value seats here and the objects page's `AttributeProgram` composes it upward. A bare `Func<ObjectAttributes, Fin<Unit>>` on the case stated no contract and refused no null.
- Law: deleted-object operations require a runtime-preserving target. Explicit deleted rows and deleted-object queries preserve runtime serials without re-entering the active-id index, deletion captures runtime pairs before mutation, and receipts project them for a later `Revive` or `Expunge` request.
- Law: `GeometryIntake` is the staged boundary union: `Of` separates native borrowed geometry from value-form conversion, while `Admit` resolves `Kind` and applies `Requirement.ForKind` under the fresh document context. Native geometry remains borrowed; every value-form conversion composes `GeometryForm` and is disposed by `Lease.Use` after the host copies it.
- Law: page import carries `DocumentPath` and re-proves `DocumentFile.ThreeDm` inside the callback. Named-view restore carries `ViewportTarget`, resolves exactly one viewport immediately before the host call, and never retains a live viewport handle in request data.
- Law: named-view restore closes direct, proportional, constant-speed, and constant-time host modalities as `NamedRestore` cases. Delay and speed enter as admitted values, so no boolean or overload discriminator crosses the transaction boundary.
- Law: `Tables.Commit` keeps the document handle inside one `DocumentSession.Demand`, proving mutation, undo, and redraw needs against one snapshot before the first edit and refreshing the kernel context inside that window. Outside a command it owns the undo record, closes it on every exit, rolls a failed program back, clears the failed record from redo, and appends close, rollback, or redraw-restoration faults to the primary fault. Inside a command it enlists in `CurrentUndoRecordSerialNumber`, never closes or undoes the command-owned record, and returns the operation fault for the command boundary to propagate. `UndoBracket.Stamper` stamps only a required positive serial the admission guard already proved; an immediate program bypasses stamping, and invalid undo evidence fails before receipt construction. An active non-command record is rejected before mutation, and redraw occurs only after success.
- Law: `DocumentCommit.Sealed` and `Tables.Commit` carry a generic railed receipt projection executed inside the bracket after the undo-serial stamp and before sealing — a consumer fold that must observe the committed receipt (a command stage folding state) enters as `project`, its refusal rolls the owned record back like any operation fault, and a wrapper folding state outside the bracket is the deleted form; the identity projection is the default modality, so receipt-shaped rails commit unchanged.
- Boundary: `AddCustomUndoEvent` has no host remove counterpart, so the document retains a `TableCustomUndo` handler, its whole captured object graph, and its arbitrary `object` tag until the undo record clears — a retention no `Subscription` can shorten, unlike every other host attachment in the slice. A handler therefore captures DETACHED evidence only: runtime pairs, stamps, admitted values. A captured live `RhinoObject`, `ObjectAttributes`, session, or lease outlives the commit that minted it and is the leak this law forecloses; the events page's process-global custody census carries the matching row.
- Law: `DocumentCommit.Compensated` owns the whole compensation algebra: land each element, roll back every landed key on the first refusal, and settle source custody through its release policy — every source releases once the fold's fate is decided, a release refusal after success rolls the landed keys back, and rollback then release faults append in that order onto the initiating fault; a suffix-only cleanup inside a rollback lambda or a `.Match` ladder re-spelling release beside the fold is the deleted form, and the identity release is the default modality for sources carrying no custody.

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

// The corpus-wide HOST-DIALOGUE axis, seated on the spine because every folder's host calls take the same
// `quiet` boolean: table deletes and amends here, layer deletes/purges/anointments on the layer rail, definition
// modify/rebind/sever/retarget/delete on the block rail, the Annotation drafting rails' style, linetype, hatch,
// and section writes, and the light purge. One row reads `IsQuiet` at the call; a folder minting its own two-row
// notice vocabulary beside it is the forked form, and a bare `quiet:` literal reading a posture DECISION is the
// unnamed form — neither states which posture the caller asked for. A host argument that is always quiet by
// design carries a comment saying so, because a vocabulary read would offer a choice the surface does not have.
[SmartEnum<int>]
public sealed partial class HostInteraction {
    public static readonly HostInteraction Quiet = new(key: 0, isQuiet: true);
    public static readonly HostInteraction Interactive = new(key: 1, isQuiet: false);

    public bool IsQuiet { get; }
}

// The `Amend` payload as a VALUE at this stratum. `TableOp` is S0 and the typed attribute program is S2, so the
// program cannot be named downward — the seam type seats HERE and `Objects/attributes.md`'s `AttributeProgram`
// composes it upward. A bare `Func<ObjectAttributes, Fin<Unit>>` on the case stated no contract at all: nothing
// said the body may only mutate the duplicate it is handed, and nothing refused a null body until the arm ran.
[ComplexValueObject]
public sealed partial class AttributeChange {
    public Func<ObjectAttributes, Fin<Unit>> Revise { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Func<ObjectAttributes, Fin<Unit>> revise) =>
        validationError = revise is null
            ? new ValidationError(message: "Attribute change carries no revision body.")
            : null;
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
    public static readonly SelectionEdit Add = new(apply: static (table, ids, policy) => Toggled(table: table, ids: ids, policy: policy, select: true));
    public static readonly SelectionEdit Remove = new(apply: static (table, ids, policy) => Toggled(table: table, ids: ids, policy: policy, select: false));
    public static readonly SelectionEdit Replace = new(apply: static (table, ids, policy) => table.SetSelectedObjects(
        objectIds: ids,
        syncHighlight: policy.Highlight.HostValue,
        persistentSelect: policy.Persistence.HostValue,
        ignoreGripsState: policy.Grips.HostValue,
        ignoreLayerLocking: policy.LayerLocks.HostValue,
        ignoreLayerVisibility: policy.LayerVisibility.HostValue));

    [UseDelegateFromConstructor]
    internal partial int Apply(ObjectTable table, IEnumerable<Guid> ids, SelectionPolicy policy);

    private static int Toggled(ObjectTable table, IEnumerable<Guid> ids, SelectionPolicy policy, bool select) =>
        table.Select(
            objectIds: ids,
            select: select,
            syncHighlight: policy.Highlight.HostValue,
            persistentSelect: policy.Persistence.HostValue,
            ignoreGripsState: policy.Grips.HostValue,
            ignoreLayerLocking: policy.LayerLocks.HostValue,
            ignoreLayerVisibility: policy.LayerVisibility.HostValue);
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

// The two repaint columns the host's own `EnableRedraw(enable, redrawDocument, redrawLayers)` takes belong to the
// redraw vocabulary, not to a literal inside the bracket: a suppressing policy that hardcodes `false` on both
// silently forbids the terminal repaint some rails need on the restore edge, and no row could ever ask for it.
[SmartEnum]
public sealed partial class RedrawPolicy {
    public static readonly RedrawPolicy None = new(enabled: false, defers: false, suppress: false, repaintsDocument: false, repaintsLayers: false);
    public static readonly RedrawPolicy Continuous = new(enabled: true, defers: false, suppress: false, repaintsDocument: false, repaintsLayers: false);
    public static readonly RedrawPolicy Immediate = new(enabled: true, defers: false, suppress: true, repaintsDocument: false, repaintsLayers: false);
    public static readonly RedrawPolicy Deferred = new(enabled: true, defers: true, suppress: true, repaintsDocument: false, repaintsLayers: false);
    public static readonly RedrawPolicy Repainting = new(enabled: true, defers: false, suppress: true, repaintsDocument: true, repaintsLayers: true);

    internal bool Enabled { get; }
    internal bool Defers { get; }
    internal bool Suppress { get; }
    internal bool RepaintsDocument { get; }
    internal bool RepaintsLayers { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record NamedRestore {
    private NamedRestore() { }

    private sealed record DirectCase(int Index, ViewportTarget Target) : NamedRestore;
    private sealed record ProportionalCase(int Index, ViewportTarget Target) : NamedRestore;
    private sealed record SpeedCase(int Index, ViewportTarget Target, double UnitsPerFrame, int DelayMs) : NamedRestore;
    private sealed record TimeCase(int Index, ViewportTarget Target, Rasm.Numerics.Dimension Frames, int DelayMs) : NamedRestore;

    public static Fin<NamedRestore> Direct(int index, ViewportTarget target, Op? key = null) =>
        Addressed(index: index, target: target, key.OrDefault())
            .ToFin()
            .Map(static address => (NamedRestore)new DirectCase(Index: address.Index, Target: address.Target));

    public static Fin<NamedRestore> Proportional(int index, ViewportTarget target, Op? key = null) =>
        Addressed(index: index, target: target, key.OrDefault())
            .ToFin()
            .Map(static address => (NamedRestore)new ProportionalCase(Index: address.Index, Target: address.Target));

    public static Fin<NamedRestore> ConstantTime(
        int index,
        ViewportTarget target,
        Rasm.Numerics.Dimension frames,
        TimeSpan delay,
        Op? key = null) {
        Op op = key.OrDefault();
        return (
                Addressed(index: index, target: target, op),
                guard(frames.Value > 0, op.InvalidInput()).ToFin().ToValidation(),
                Delay(delay: delay, op: op).ToValidation())
            .Apply((address, _, ms) => (NamedRestore)new TimeCase(
                Index: address.Index,
                Target: address.Target,
                Frames: frames,
                DelayMs: ms))
            .As()
            .ToFin();
    }

    public static Fin<NamedRestore> ConstantSpeed(
        int index,
        ViewportTarget target,
        double unitsPerFrame,
        TimeSpan delay,
        Op? key = null) {
        Op op = key.OrDefault();
        return (
                Addressed(index: index, target: target, op),
                guard(double.IsFinite(unitsPerFrame) && unitsPerFrame > 0.0, op.InvalidInput()).ToFin().ToValidation(),
                Delay(delay: delay, op: op).ToValidation())
            .Apply((address, _, ms) => (NamedRestore)new SpeedCase(
                Index: address.Index,
                Target: address.Target,
                UnitsPerFrame: unitsPerFrame,
                DelayMs: ms))
            .As()
            .ToFin();
    }

    private static Validation<Error, (int Index, ViewportTarget Target)> Addressed(int index, ViewportTarget target, Op op) =>
        (
            guard(index >= 0, op.InvalidInput()).ToFin().ToValidation(),
            op.Need(target).ToValidation())
        .Apply((_, address) => (Index: index, Target: address))
        .As();

    private static Fin<int> Delay(TimeSpan delay, Op op) =>
        guard(
            delay >= TimeSpan.Zero
            && delay.Ticks % TimeSpan.TicksPerMillisecond is 0
            && delay.TotalMilliseconds <= int.MaxValue,
            op.InvalidInput()).ToFin().Map(_ => (int)delay.TotalMilliseconds);

    internal Fin<Unit> Apply(RhinoDoc document, Op key) =>
        from address in Switch(
            directCase: static restore => Fin.Succ(value: (restore.Index, restore.Target)),
            proportionalCase: static restore => Fin.Succ(value: (restore.Index, restore.Target)),
            speedCase: static restore => Fin.Succ(value: (restore.Index, restore.Target)),
            timeCase: static restore => Fin.Succ(value: (restore.Index, restore.Target)))
        from viewport in address.Target.ResolveViewport(document: document, key: key)
        from applied in Switch(
            state: (Document: document, Viewport: viewport, Op: key),
            directCase: static (context, restore) =>
                from _ in context.Op.Confirm(success: context.Document.NamedViews.Restore(
                    index: restore.Index,
                    viewport: context.Viewport))
                select unit,
            proportionalCase: static (context, restore) =>
                from _ in context.Op.Confirm(success: context.Document.NamedViews.RestoreWithAspectRatio(
                    index: restore.Index,
                    viewport: context.Viewport))
                select unit,
            speedCase: static (context, restore) =>
                from _ in context.Op.Confirm(success: context.Document.NamedViews.RestoreAnimatedConstantSpeed(
                    index: restore.Index,
                    viewport: context.Viewport,
                    units_per_frame: restore.UnitsPerFrame,
                    ms_delay: restore.DelayMs))
                select unit,
            timeCase: static (context, restore) =>
                from _ in context.Op.Confirm(success: context.Document.NamedViews.RestoreAnimatedConstantTime(
                    index: restore.Index,
                    viewport: context.Viewport,
                    frames: restore.Frames.Value,
                    ms_delay: restore.DelayMs))
                select unit)
        select applied;
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

    public static Fin<HistoryRoll> ClearUndo(DeletedPolicy deleted, Option<uint> serial = default, Op? key = null) {
        Op op = key.OrDefault();
        return (
                op.Need(deleted).ToValidation(),
                guard(
                    serial.Map(static value => value > 0u).IfNone(noneValue: true),
                    op.InvalidInput()).ToFin().ToValidation())
            .Apply((policy, _) => (HistoryRoll)new ClearUndoCase(Deleted: policy, Serial: serial))
            .As()
            .ToFin();
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

[SmartEnum<int>]
internal sealed partial class TableOpTraits {
    internal static readonly TableOpTraits Sourced = new(key: 0, recordsUndo: true, navigates: false, requiresContext: true);
    internal static readonly TableOpTraits Recorded = new(key: 1, recordsUndo: true, navigates: false, requiresContext: false);
    internal static readonly TableOpTraits Immediate = new(key: 2, recordsUndo: false, navigates: false, requiresContext: false);
    internal static readonly TableOpTraits Navigation = new(key: 3, recordsUndo: false, navigates: true, requiresContext: false);

    internal bool RecordsUndo { get; }
    internal bool Navigates { get; }
    internal bool RequiresContext { get; }
}

[SmartEnum<int>]
internal sealed partial class TransactionUndo {
    internal static readonly TransactionUndo None = new(key: 0, required: false, records: false);
    internal static readonly TransactionUndo Record = new(key: 1, required: true, records: true);
    internal static readonly TransactionUndo Navigate = new(key: 2, required: true, records: false);

    internal bool Required { get; }
    internal bool Records { get; }
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TableOp {
    private TableOp() { }

    private sealed record AddCase(Seq<GeometryIntake> Sources, Option<ObjectAttributes> Attributes, Option<HistoryRecord> History, ObjectCustody Custody) : TableOp;
    private sealed record ReplaceCase(TableTarget Target, GeometryIntake Replacement, ObjectMode Modes) : TableOp;
    private sealed record DeleteCase(TableTarget Target, HostInteraction Interaction, ObjectMode Modes) : TableOp;
    private sealed record TransformCase(TableTarget Target, Transform Motion, TransformPolicy Policy) : TableOp;
    private sealed record AmendCase(TableTarget Target, AttributeChange Change, HostInteraction Interaction) : TableOp;
    private sealed record SelectCase(TableTarget Target, SelectionEdit Edit, SelectionPolicy Policy) : TableOp;
    private sealed record StateCase(TableTarget Target, ObjectState State, ObjectMode Modes) : TableOp;
    private sealed record ClearSelectionCase(SelectionClear Scope, SelectionPolicy Census) : TableOp;
    private sealed record FlashCase(TableTarget Target, FlashMode Mode) : TableOp;
    private sealed record ReviveCase(TableTarget Target) : TableOp;
    private sealed record ExpungeCase(TableTarget Target) : TableOp;
    private sealed record CloudCase(
        Rasm.Numerics.Dimension X,
        Rasm.Numerics.Dimension Y,
        Rasm.Numerics.Dimension Z,
        Arr<Point3d> Box,
        Option<ObjectAttributes> Attributes,
        Option<HistoryRecord> History,
        ObjectCustody Custody) : TableOp;
    private sealed record RebindCase(TableTarget Target, ResourceIndex DefinitionIndex) : TableOp;
    private sealed record ReclaimCase(TableKind Kind) : TableOp;
    private sealed record ImportPageCase(DocumentPath Path, Guid MainViewportId, string PageName) : TableOp;
    private sealed record RestoreViewCase(NamedRestore Restore) : TableOp;
    private sealed record RollCase(HistoryRoll Navigation) : TableOp;

    public static Fin<TableOp> Add(ObjectCustody custody, Option<ObjectAttributes> attributes = default, Option<HistoryRecord> history = default, params ReadOnlySpan<object> sources) {
        Op op = Op.Of();   // an optional before `params` forecloses the positional spread — the key mints at the entry
        return (
                op.Need(custody).ToValidation(),
                toSeq(sources.ToArray())
                    .Traverse(source => GeometryIntake.Of(source: source, key: op).ToValidation())
                    .As(),
                guard(sources.Length > 0, op.InvalidInput()).ToFin().ToValidation())
            .Apply(static (policy, values, _) => (TableOp)new AddCase(
                Sources: values,
                Attributes: attributes,
                History: history,
                Custody: policy))
            .As()
            .ToFin();
    }

    public static Fin<TableOp> Replace(TableTarget target, object replacement, ObjectMode modes, Op? key = null) {
        Op op = key.OrDefault();
        return (
                op.Need(target).ToValidation(),
                GeometryIntake.Of(source: replacement, key: op).ToValidation(),
                op.Need(modes).ToValidation())
            .Apply(static (address, geometry, policy) =>
                (TableOp)new ReplaceCase(Target: address, Replacement: geometry, Modes: policy))
            .As()
            .ToFin();
    }

    public static Fin<TableOp> Delete(TableTarget target, HostInteraction interaction, ObjectMode modes, Op? key = null) =>
        Admitted(first: target, second: interaction, third: modes, key: key, mint: static (address, dialogue, policy) =>
            new DeleteCase(Target: address, Interaction: dialogue, Modes: policy));

    public static Fin<TableOp> Transform(TableTarget target, Transform motion, TransformPolicy policy, Op? key = null) {
        Op op = key.OrDefault();
        return (
                op.Need(target).ToValidation(),
                op.AcceptInput(value: motion).ToValidation(),
                op.Need(policy).ToValidation())
            .Apply(static (address, transform, mode) => (TableOp)new TransformCase(
                Target: address,
                Motion: transform,
                Policy: mode))
            .As()
            .ToFin();
    }

    public static Fin<TableOp> Amend(TableTarget target, AttributeChange change, HostInteraction interaction, Op? key = null) =>
        Admitted(first: target, second: change, third: interaction, key: key, mint: static (address, revise, dialogue) =>
            new AmendCase(Target: address, Change: revise, Interaction: dialogue));

    public static Fin<TableOp> Select(TableTarget target, SelectionEdit edit, SelectionPolicy policy, Op? key = null) =>
        Admitted(first: target, second: edit, third: policy, key: key, mint: static (address, mutation, admitted) =>
            new SelectCase(Target: address, Edit: mutation, Policy: admitted));

    public static Fin<TableOp> State(TableTarget target, ObjectState state, ObjectMode modes, Op? key = null) =>
        Admitted(first: target, second: state, third: modes, key: key, mint: static (address, mutation, policy) =>
            new StateCase(Target: address, State: mutation, Modes: policy));

    // The clear carries a policy for its CENSUS alone: the before/after spans that produce the receipt read the
    // same two inclusion axes every other selection op reads, so a clear cannot report a different population
    // than the select that preceded it.
    public static Fin<TableOp> ClearSelection(SelectionClear scope, SelectionPolicy census, Op? key = null) =>
        Admitted(first: scope, second: census, key: key, mint: static (value, policy) =>
            new ClearSelectionCase(Scope: value, Census: policy));

    public static Fin<TableOp> Flash(TableTarget target, FlashMode mode, Op? key = null) =>
        Admitted(first: target, second: mode, key: key, mint: static (address, display) =>
            new FlashCase(Target: address, Mode: display));

    private static Fin<TableOp> Admitted<T1, T2>(T1 first, T2 second, Op? key, Func<T1, T2, TableOp> mint)
        where T1 : class where T2 : class {
        Op op = key.OrDefault();
        return (
                op.Need(first).ToValidation(),
                op.Need(second).ToValidation())
            .Apply(mint)
            .As()
            .ToFin();
    }

    private static Fin<TableOp> Admitted<T1, T2, T3>(T1 first, T2 second, T3 third, Op? key, Func<T1, T2, T3, TableOp> mint)
        where T1 : class where T2 : class where T3 : class {
        Op op = key.OrDefault();
        return (
                op.Need(first).ToValidation(),
                op.Need(second).ToValidation(),
                op.Need(third).ToValidation())
            .Apply(mint)
            .As()
            .ToFin();
    }

    public static Fin<TableOp> Revive(TableTarget target, Op? key = null) =>
        Retained(target: target, key: key, mint: static value => new ReviveCase(Target: value));

    public static Fin<TableOp> Expunge(TableTarget target, Op? key = null) =>
        Retained(target: target, key: key, mint: static value => new ExpungeCase(Target: value));

    private static Fin<TableOp> Retained(TableTarget target, Op? key, Func<TableTarget, TableOp> mint) {
        Op op = key.OrDefault();
        return op.Need(target)
            .Bind(value => value.RetainsRuntime ? Fin.Succ(value: mint(arg: value)) : Fin.Fail<TableOp>(op.InvalidInput()));
    }

    public static Fin<TableOp> Cloud(
        Rasm.Numerics.Dimension x,
        Rasm.Numerics.Dimension y,
        Rasm.Numerics.Dimension z,
        Arr<Point3d> box,
        ObjectCustody custody,
        Option<ObjectAttributes> attributes = default,
        Option<HistoryRecord> history = default,
        Op? key = null) {
        Op op = key.OrDefault();
        return (
                op.Need(custody).ToValidation(),
                guard(
                    box.Count is 8
                    && x != default && y != default && z != default
                    && x.Value <= int.MaxValue / y.Value / z.Value,
                    op.InvalidInput()).ToFin().ToValidation(),
                box.AsIterable().ToSeq()
                    .Traverse(point => op.AcceptInput(value: point).ToValidation())
                    .As())
            .Apply((policy, _, _) => (TableOp)new CloudCase(
                X: x,
                Y: y,
                Z: z,
                Box: box,
                Attributes: attributes,
                History: history,
                Custody: policy))
            .As()
            .ToFin();
    }

    public static Fin<TableOp> Rebind(TableTarget target, ResourceIndex definitionIndex, Op? key = null) {
        Op op = key.OrDefault();
        return (
                op.Need(target).ToValidation(),
                op.Need(definitionIndex).ToValidation())
            .Apply(static (address, index) => (TableOp)new RebindCase(
                Target: address,
                DefinitionIndex: index))
            .As()
            .ToFin();
    }

    public static Fin<TableOp> Reclaim(TableKind kind, Op? key = null) =>
        Optional(kind).ToFin(Fail: key.OrDefault().InvalidInput()).Map(static value => (TableOp)new ReclaimCase(Kind: value));

    public static Fin<TableOp> ImportPage(DocumentPath path, Guid mainViewportId, string pageName, Op? key = null) {
        Op op = key.OrDefault();
        return (
                guard(path != default, op.InvalidInput()).ToFin().ToValidation(),
                op.AcceptInput(value: mainViewportId).ToValidation(),
                op.AcceptText(value: pageName).ToValidation())
            .Apply((_, viewport, name) => (TableOp)new ImportPageCase(
                Path: path,
                MainViewportId: viewport,
                PageName: name))
            .As()
            .ToFin();
    }

    public static Fin<TableOp> RestoreView(NamedRestore restore, Op? key = null) =>
        Optional(restore).ToFin(Fail: key.OrDefault().InvalidInput()).Map(static value => (TableOp)new RestoreViewCase(Restore: value));

    public static Fin<TableOp> Roll(HistoryRoll navigation, Op? key = null) =>
        Optional(navigation).ToFin(Fail: key.OrDefault().InvalidInput()).Map(static value => (TableOp)new RollCase(Navigation: value));

    internal TableOpTraits Traits => Map(
        addCase: TableOpTraits.Sourced,
        replaceCase: TableOpTraits.Sourced,
        deleteCase: TableOpTraits.Recorded,
        transformCase: TableOpTraits.Recorded,
        amendCase: TableOpTraits.Recorded,
        selectCase: TableOpTraits.Immediate,
        stateCase: TableOpTraits.Recorded,
        clearSelectionCase: TableOpTraits.Immediate,
        flashCase: TableOpTraits.Immediate,
        reviveCase: TableOpTraits.Recorded,
        expungeCase: TableOpTraits.Immediate,
        cloudCase: TableOpTraits.Recorded,
        rebindCase: TableOpTraits.Recorded,
        reclaimCase: TableOpTraits.Immediate,
        importPageCase: TableOpTraits.Recorded,
        restoreViewCase: TableOpTraits.Immediate,
        rollCase: TableOpTraits.Navigation);

    internal Fin<TableReceipt> Apply(RhinoDoc document, Option<Context> domain, Op op) =>
        Switch(
            (Document: document, Domain: domain, Op: op),
            addCase: static (context, edit) =>
                from model in context.Domain.ToFin(Fail: context.Op.MissingContext())
                // `ObjectTable.Add` reports failure as `Guid.Empty`, which a generic value admission accepts —
                // the receipt then carried an empty id as a created object and `Runtime` failed later with no
                // trace of which source produced it. `ResourceId.Admit` is the spine's ONE empty-guid projector.
                from ids in edit.Sources.TraverseM(source => source.Admit(domain: model, key: context.Op)
                    .Bind(lease => lease.Use(native => ResourceId.Admit(
                        value: context.Document.Objects.Add(
                            geometry: native,
                            attributes: edit.Attributes.ValueUnsafe(),
                            history: edit.History.ValueUnsafe(),
                            reference: edit.Custody.IsReference),
                        key: context.Op).Map(static id => id.Value)))).As()
                from runtime in Tables.Runtime(document: context.Document, ids: ids, key: context.Op)
                select TableReceipt.Objects(slot: TableSlot.Created, values: runtime),
            replaceCase: static (context, edit) =>
                from model in context.Domain.ToFin(Fail: context.Op.MissingContext())
                from ids in edit.Target.Resolve(document: context.Document, key: context.Op)
                from single in Tables.One(rows: ids, key: context.Op)
                from _ in edit.Replacement.Admit(domain: model, key: context.Op)
                    .Bind(lease => lease.Use(native => context.Op.Confirm(success: context.Document.Objects.Replace(objectId: single, geometry: native, ignoreModes: edit.Modes.IgnoresModes))))
                from runtime in Tables.Runtime(document: context.Document, ids: Seq(single), key: context.Op)
                select TableReceipt.Objects(slot: TableSlot.Replaced, values: runtime),
            deleteCase: static (context, edit) =>
                from targets in edit.Target.Serials(document: context.Document, key: context.Op)
                from _ in edit.Modes.IgnoresModes
                    ? targets.TraverseM(target => Optional(context.Document.Objects.FindId(target.Id)).ToFin(Fail: context.Op.InvalidResult())
                        .Bind(native => context.Op.Confirm(success: context.Document.Objects.Delete(obj: native, quiet: edit.Interaction.IsQuiet, ignoreModes: true)))).As().Map(static _ => unit)
                    : context.Op.Confirm(success: context.Document.Objects.Delete(objectIds: targets.Map(static target => target.Id).AsIterable(), quiet: edit.Interaction.IsQuiet) == targets.Count)
                select TableReceipt.Objects(slot: TableSlot.Deleted, values: targets),
            transformCase: static (context, edit) => Mapped(
                document: context.Document,
                target: edit.Target,
                slot: edit.Policy.Slot,
                step: id => ResourceId.Admit(
                    value: edit.Policy.Apply(table: context.Document.Objects, id: id, motion: edit.Motion),
                    key: context.Op).Map(static minted => minted.Value),
                op: context.Op),
            amendCase: static (context, edit) => Mapped(
                document: context.Document,
                target: edit.Target,
                slot: TableSlot.Amended,
                step: id =>
                    from native in Optional(context.Document.Objects.FindId(id)).ToFin(Fail: context.Op.InvalidResult())
                    from attributes in Optional(native.Attributes?.Duplicate()).ToFin(Fail: context.Op.InvalidResult())
                    from _ in new Lease<ObjectAttributes>.Owned(Value: attributes).Use(owned =>
                        from __ in edit.Change.Revise(arg: owned)
                        from ___ in context.Op.Confirm(success: context.Document.Objects.ModifyAttributes(
                            objectId: id,
                            newAttributes: owned,
                            quiet: edit.Interaction.IsQuiet))
                        select unit)
                    select id,
                op: context.Op),
            selectCase: static (context, edit) =>
                from ids in edit.Target.Resolve(document: context.Document, key: context.Op)
                from receipt in SelectionSpan(
                    document: context.Document,
                    policy: edit.Policy,
                    apply: () => edit.Edit.Apply(table: context.Document.Objects, ids: ids.AsIterable(), policy: edit.Policy),
                    op: context.Op)
                select receipt,
            stateCase: static (context, edit) =>
                from targets in edit.Target.Serials(document: context.Document, key: context.Op)
                from changed in Tables.ApplyState(document: context.Document, targets: targets, state: edit.State, modes: edit.Modes, key: context.Op)
                select TableReceipt.Objects(slot: edit.State.Slot, values: changed),
            clearSelectionCase: static (context, edit) => SelectionSpan(
                document: context.Document,
                policy: edit.Census,
                apply: () => context.Document.Objects.UnselectAll(ignorePersistentSelections: edit.Scope.IgnorePersistent),
                op: context.Op),
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
                from id in ResourceId.Admit(
                    value: context.Document.Objects.AddOrderedPointCloud(
                        xCt: edit.X.Value,
                        yCt: edit.Y.Value,
                        zCt: edit.Z.Value,
                        box: edit.Box.ToArray(),
                        attributes: edit.Attributes.ValueUnsafe(),
                        history: edit.History.ValueUnsafe(),
                        reference: edit.Custody.IsReference),
                    key: context.Op)
                from runtime in Tables.Runtime(document: context.Document, ids: Seq(id.Value), key: context.Op)
                select TableReceipt.Objects(slot: TableSlot.Created, values: runtime),
            rebindCase: static (context, edit) => Mapped(
                document: context.Document,
                target: edit.Target,
                slot: TableSlot.Replaced,
                step: id => context.Op.Confirm(success: context.Document.Objects.ReplaceInstanceObject(objectId: id, instanceDefinitionIndex: edit.DefinitionIndex.Value)).Map(_ => id),
                op: context.Op),
            reclaimCase: static (context, edit) => edit.Kind.Reclaim(document: context.Document, key: context.Op)
                .Bind(count => TableReceipt.Component(kind: edit.Kind, tally: count, key: context.Op)),
            importPageCase: static (context, edit) =>
                from path in edit.Path.Resolve(file: DocumentFile.ThreeDm, key: context.Op)
                let before = context.Document.Views.PageViewCount
                from _ in context.Op.Confirm(success: context.Document.Views.ImportPageView(filename: path, mainViewportId: edit.MainViewportId, pageName: edit.PageName))
                let imported = context.Document.Views.PageViewCount - before
                from __ in guard(imported > 0, context.Op.InvalidResult()).ToFin()
                from receipt in TableReceipt.Component(
                    kind: TableKind.Views,
                    tally: imported,
                    key: context.Op)
                select receipt,
            restoreViewCase: static (context, edit) =>
                from _ in edit.Restore.Apply(document: context.Document, key: context.Op)
                select TableReceipt.Restore(value: edit.Restore),
            rollCase: static (context, edit) => edit.Navigation.Apply(document: context.Document, key: context.Op)
                .Map(_ => TableReceipt.History(value: edit.Navigation)));

    private static Fin<TableReceipt> SelectionSpan(RhinoDoc document, SelectionPolicy policy, Func<int> apply, Op op) =>
        from before in Tables.Selected(document: document, policy: policy, key: op)
        from _ in guard(apply() >= 0, op.InvalidResult()).ToFin()
        from after in Tables.Selected(document: document, policy: policy, key: op)
        select TableReceipt.SelectionDelta(before: before, after: after);

    private static Fin<TableReceipt> Lifecycle(RhinoDoc document, TableTarget target, TableSlot slot, Func<ObjectTable, uint, bool> apply, Op op) =>
        from targets in target.Serials(document: document, key: op)
        from changed in targets.TraverseM(value => op.Confirm(success: apply(document.Objects, value.Serial)).Map(_ => value)).As()
        select TableReceipt.Objects(slot: slot, values: changed);

    private static Fin<TableReceipt> Mapped(RhinoDoc document, TableTarget target, TableSlot slot, Func<Guid, Fin<Guid>> step, Op op) =>
        from ids in target.Resolve(document: document, key: op)
        from mapped in ids.TraverseM(step).As()
        from runtime in Tables.Runtime(document: document, ids: mapped, key: op)
        select TableReceipt.Objects(slot: slot, values: runtime);
}

// --- [MODELS] -----------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class SelectionHighlight {
    public static readonly SelectionHighlight Synchronize = new(key: 0, hostValue: true);
    public static readonly SelectionHighlight Preserve = new(key: 1, hostValue: false);
    internal bool HostValue { get; }
}

[SmartEnum<int>]
public sealed partial class SelectionGrips {
    public static readonly SelectionGrips Ignore = new(key: 0, hostValue: true);
    public static readonly SelectionGrips Respect = new(key: 1, hostValue: false);
    internal bool HostValue { get; }
}

[SmartEnum<int>]
public sealed partial class SelectionPersistence {
    public static readonly SelectionPersistence Persistent = new(key: 0, hostValue: true);
    public static readonly SelectionPersistence Transient = new(key: 1, hostValue: false);
    internal bool HostValue { get; }
}

[SmartEnum<int>]
public sealed partial class SelectionLayerLocks {
    public static readonly SelectionLayerLocks Respect = new(key: 0, hostValue: false);
    public static readonly SelectionLayerLocks Ignore = new(key: 1, hostValue: true);
    internal bool HostValue { get; }
}

// The two census axes the host's own `GetSelectedObjects(includeLights, includeGrips)` takes. They were literal
// `true`s inside the census helper, which silently made every selection receipt include lights and grips whatever
// the caller's policy said about them — the policy's other five axes were honoured and these two were not.
[SmartEnum<int>]
public sealed partial class SelectionLights {
    public static readonly SelectionLights Include = new(key: 0, hostValue: true);
    public static readonly SelectionLights Omit = new(key: 1, hostValue: false);
    internal bool HostValue { get; }
}

[SmartEnum<int>]
public sealed partial class SelectionGripCensus {
    public static readonly SelectionGripCensus Include = new(key: 0, hostValue: true);
    public static readonly SelectionGripCensus Omit = new(key: 1, hostValue: false);
    internal bool HostValue { get; }
}

[SmartEnum<int>]
public sealed partial class SelectionLayerVisibility {
    public static readonly SelectionLayerVisibility Respect = new(key: 0, hostValue: false);
    public static readonly SelectionLayerVisibility Ignore = new(key: 1, hostValue: true);
    internal bool HostValue { get; }
}

[ComplexValueObject]
public sealed partial class SelectionPolicy {
    public SelectionHighlight Highlight { get; }
    public SelectionGrips Grips { get; }
    public SelectionPersistence Persistence { get; }
    public SelectionLayerLocks LayerLocks { get; }
    public SelectionLayerVisibility LayerVisibility { get; }
    public SelectionLights Lights { get; }
    public SelectionGripCensus GripCensus { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref SelectionHighlight highlight,
        ref SelectionGrips grips,
        ref SelectionPersistence persistence,
        ref SelectionLayerLocks layerLocks,
        ref SelectionLayerVisibility layerVisibility,
        ref SelectionLights lights,
        ref SelectionGripCensus gripCensus) =>
        validationError = highlight is null
            || grips is null
            || persistence is null
            || layerLocks is null
            || layerVisibility is null
            || lights is null
            || gripCensus is null
            ? new ValidationError(message: "Selection policy is incomplete.")
            : null;

    public static Fin<SelectionPolicy> Of(
        Option<SelectionHighlight> highlight = default,
        Option<SelectionGrips> grips = default,
        Option<SelectionPersistence> persistence = default,
        Option<SelectionLayerLocks> layerLocks = default,
        Option<SelectionLayerVisibility> layerVisibility = default,
        Option<SelectionLights> lights = default,
        Option<SelectionGripCensus> gripCensus = default,
        Op? key = null) {
        return Admission.Admitted(
            fault: Validate(
                highlight.IfNone(SelectionHighlight.Synchronize),
                grips.IfNone(SelectionGrips.Ignore),
                persistence.IfNone(SelectionPersistence.Persistent),
                layerLocks.IfNone(SelectionLayerLocks.Respect),
                layerVisibility.IfNone(SelectionLayerVisibility.Respect),
                lights.IfNone(SelectionLights.Include),
                gripCensus.IfNone(SelectionGripCensus.Include),
                out SelectionPolicy? admitted),
            value: admitted,
            refusal: key.OrDefault().InvalidInput());
    }
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

    public static Fin<TableCustomUndo> Of(string name, EventHandler<CustomUndoEventArgs> handler, Option<object> tag = default, Op? key = null) {
        Op op = key.OrDefault();
        return (
                op.AcceptText(value: name).ToValidation(),
                op.Need(handler).ToValidation())
            .Apply((admitted, callback) => new TableCustomUndo(
                name: admitted,
                handler: callback,
                tag: tag))
            .As()
            .ToFin();
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
    private sealed record ImmediateCase(Seq<TableOp> Operations, RedrawPolicy Redraw) : TableTransaction;
    private sealed record NavigationCase(TableOp Operation, RedrawPolicy Redraw) : TableTransaction;

    public static Fin<TableTransaction> Recorded(string name, RedrawPolicy redraw, Seq<TableCustomUndo> customUndo, params ReadOnlySpan<TableOp> operations) {
        Op op = Op.Of();   // an optional before `params` forecloses the positional spread — the key mints at the entry
        return from admitted in (
                   op.AcceptText(value: name).ToValidation(),
                   Admit(redraw: redraw, operations: operations, op: op).ToValidation(),
                   customUndo
                       .Traverse(item => op.Need(item).ToValidation())
                       .As())
                   .Apply(static (transactionName, plan, undo) => (
                       Name: transactionName,
                       Plan: plan,
                       Undo: undo))
                   .As()
                   .ToFin()
               from _ in guard(
                   (!admitted.Plan.Operations.IsEmpty || !admitted.Undo.IsEmpty)
                   && admitted.Plan.Operations.ForAll(static operation => operation.Traits is { RecordsUndo: true, Navigates: false }),
                   op.InvalidInput()).ToFin()
               select (TableTransaction)new RecordedCase(
                   Name: admitted.Name,
                   Operations: admitted.Plan.Operations,
                   Redraw: admitted.Plan.Redraw,
                   CustomUndo: admitted.Undo);
    }

    // An immediate program carries EXACTLY ONE operation, and the bound is structural rather than arbitrary: an
    // immediate op opens no undo record, so a multi-op immediate program that fails midway leaves the completed
    // prefix landed with no record to roll it back and no compensation the shape can express. A caller wanting two
    // immediate effects commits two transactions and owns the ordering — which is the honest custody either way.
    public static Fin<TableTransaction> Immediate(RedrawPolicy redraw, params ReadOnlySpan<TableOp> operations) {
        Op op = Op.Of();   // an optional before `params` forecloses the positional spread — the key mints at the entry
        return from plan in Admit(redraw: redraw, operations: operations, op: op)
               from _ in guard(
                   plan.Operations.Count is 1
                   && plan.Operations.ForAll(static operation => operation.Traits is { RecordsUndo: false, Navigates: false }),
                   op.InvalidInput()).ToFin()
               select (TableTransaction)new ImmediateCase(Operations: plan.Operations, Redraw: plan.Redraw);
    }

    public static Fin<TableTransaction> Navigate(HistoryRoll navigation, RedrawPolicy redraw, Op? key = null) {
        Op op = key.OrDefault();
        return (
                TableOp.Roll(navigation: navigation, key: op).ToValidation(),
                op.Need(redraw).ToValidation())
            .Apply(static (operation, policy) => (TableTransaction)new NavigationCase(
                Operation: operation,
                Redraw: policy))
            .As()
            .ToFin();
    }

    internal TransactionPlan Materialize() =>
        Switch(
            recordedCase: static transaction => new TransactionPlan(RecordName: Some(transaction.Name), Operations: transaction.Operations, Redraw: transaction.Redraw, CustomUndo: transaction.CustomUndo, Undo: TransactionUndo.Record),
            immediateCase: static transaction => new TransactionPlan(RecordName: Option<string>.None, Operations: transaction.Operations, Redraw: transaction.Redraw, CustomUndo: Seq<TableCustomUndo>(), Undo: TransactionUndo.None),
            navigationCase: static transaction => new TransactionPlan(RecordName: Option<string>.None, Operations: Seq(transaction.Operation), Redraw: transaction.Redraw, CustomUndo: Seq<TableCustomUndo>(), Undo: TransactionUndo.Navigate));

    private static Fin<(Seq<TableOp> Operations, RedrawPolicy Redraw)> Admit(
        RedrawPolicy redraw,
        ReadOnlySpan<TableOp> operations,
        Op op) =>
        (
            op.Need(redraw).ToValidation(),
            Admission.All(values: operations, key: op).ToValidation())
        .Apply(static (policy, program) => (
            Operations: program,
            Redraw: policy))
        .As()
        .ToFin();
}

internal readonly record struct TransactionPlan(
    Option<string> RecordName,
    Seq<TableOp> Operations,
    RedrawPolicy Redraw,
    Seq<TableCustomUndo> CustomUndo,
    TransactionUndo Undo);

// --- [OPERATIONS] -------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GeometryIntake {
    private GeometryIntake() { }

    private sealed record NativeCase(GeometryBase Source) : GeometryIntake;
    private sealed record ValueCase(object Source) : GeometryIntake;

    public static Fin<GeometryIntake> Of(object source, Op? key = null) =>
        Optional(source).ToFin(Fail: key.OrDefault().InvalidInput()).Map(static value => value switch {
            GeometryBase native => (GeometryIntake)new NativeCase(Source: native),
            _ => new ValueCase(Source: value),
        });

    internal Fin<Lease<GeometryBase>> Admit(Context domain, Op key) =>
        Switch(
            state: (Domain: domain, Op: key),
            nativeCase: static (context, intake) =>
                from _ in Require(source: intake.Source, domain: context.Domain)
                select (Lease<GeometryBase>)new Lease<GeometryBase>.Borrowed(Value: intake.Source),
            valueCase: static (context, intake) =>
                from _ in Require(source: intake.Source, domain: context.Domain)
                from lease in intake.Source.GeometryForm(key: context.Op)
                select lease);

    private static Fin<Unit> Require(object source, Context domain) =>
        from kind in source.KindOf(context: domain)
        from _ in Requirement.ForKind(kind: kind)
            .Apply(
                context: domain,
                value: source,
                cancel: CancellationToken.None)
            .ToFin()
        select unit;
}

public static class Tables {
    public static Fin<TableReceipt> Commit(DocumentSession session, TableTransaction transaction, Op? key = null) =>
        Commit(session: session, transaction: transaction, project: static receipt => Fin.Succ(value: receipt), key: key);

    public static Fin<TOut> Commit<TOut>(
        DocumentSession session,
        TableTransaction transaction,
        Func<TableReceipt, Fin<TOut>> project,
        Op? key = null) {
        Op op = key.OrDefault();
        return from admission in Admission.Pair(first: session, second: transaction, key: op)
               from fold in op.Need(project)
               let plan = admission.Second.Materialize()
               from projected in admission.First.Demand(
                   use: document => Run(document: document, plan: plan, project: fold, op: op),
                   key: op,
                   needs: SessionNeed.Mutation(undo: plan.Undo.Required, redraw: plan.Redraw).ToArray())
               select projected;
    }

    internal static Fin<T> One<T>(Seq<T> rows, Op key) =>
        rows switch { [var only] => Fin.Succ(value: only), _ => Fin.Fail<T>(error: key.InvalidInput()) };

    internal static Fin<Seq<ObjectRuntime>> Runtime(RhinoDoc document, Seq<Guid> ids, Op key) =>
        ids.Distinct()
            .Traverse(id => Optional(document.Objects.FindId(id))
                .ToFin(Fail: key.InvalidResult())
                .Bind(native => ObjectRuntime.Of(
                    id: id,
                    serial: native.RuntimeSerialNumber,
                    key: key))
                .ToValidation())
            .As()
            .ToFin();

    internal static Fin<Seq<ObjectRuntime>> Selected(RhinoDoc document, SelectionPolicy policy, Op key) =>
        Optional(document.Objects.GetSelectedObjects(
                includeLights: policy.Lights.HostValue,
                includeGrips: policy.GripCensus.HostValue))
            .ToFin(Fail: key.InvalidResult())
            .Bind(values => values.AsIterable().ToSeq()
                .Traverse(native => ObjectRuntime.Of(
                    id: native.Id,
                    serial: native.RuntimeSerialNumber,
                    key: key).ToValidation())
                .As()
                .ToFin())
            .Map(static values => ObjectRuntime.Canonical(values: values));

    internal static Fin<Seq<ObjectRuntime>> ApplyState(RhinoDoc document, Seq<ObjectRuntime> targets, ObjectState state, ObjectMode modes, Op key) =>
        targets.TraverseM(target => Optional(document.Objects.FindId(target.Id))
            .ToFin(Fail: key.InvalidResult())
            .Bind(native => state.Done(native: native)
                ? Fin.Succ(value: Option<ObjectRuntime>.None)
                : state.Apply(table: document.Objects, id: target.Id, ignoreLayerMode: modes.IgnoresModes)
                    ? Fin.Succ(value: Some(target))
                    : Fin.Fail<Option<ObjectRuntime>>(error: key.InvalidResult()))).As().Map(static values => values.Somes());

    private static Fin<TOut> Run<TOut>(RhinoDoc document, TransactionPlan plan, Func<TableReceipt, Fin<TOut>> project, Op op) =>
        from domain in plan.Operations.Exists(static operation => operation.Traits.RequiresContext)
            ? Rasm.Domain.Context.Of(doc: document).ToFin().Map(Some)
            : Fin.Succ(Option<Context>.None)
        from projected in DocumentCommit.Sealed(
            document: document,
            name: plan.RecordName.IfNone(nameof(Tables)),
            recordsUndo: plan.Undo.Records,
            redraw: plan.Redraw,
            run: () =>
                from custom in plan.CustomUndo.TraverseM(undo => undo.Register(document: document, key: op)).As()
                from folded in plan.Operations
                    .TraverseM(operation => operation.Apply(document: document, domain: domain, op: op)).As()
                    .Map(static receipts => receipts.Fold(TableReceipt.Empty, static (state, value) => state + value))
                select folded + TableReceipt.CustomUndo(names: custom),
            stamp: static (receipt, serial) => receipt + TableReceipt.Undo(serial: serial),
            project: project,
            op: op)
        select projected;
}

// --- [BOUNDARIES] -------------------------------------------------------------------------
internal static class RedrawScope {
    internal static Fin<TOut> Within<TOut>(RhinoDoc document, RedrawPolicy redraw, Func<Fin<TOut>> body, Op key) =>
        key.Catch(() => {
            bool prior = document.Views.RedrawEnabled;
            Fin<TOut> outcome;
            Fin<Unit> restored = Fin.Succ(value: unit);
            try {
                outcome = Op.SideWhen(redraw.Suppress, () => document.Views.EnableRedraw(
                        enable: false,
                        redrawDocument: redraw.RepaintsDocument,
                        redrawLayers: redraw.RepaintsLayers))
                    .Bind(_ => body());
            } finally {
                restored = Op.SideWhen(redraw.Suppress, () => document.Views.EnableRedraw(
                    enable: prior,
                    redrawDocument: redraw.RepaintsDocument,
                    redrawLayers: redraw.RepaintsLayers));
            }
            return Append(primary: outcome, side: restored).Bind(value =>
                Op.SideWhen(redraw.Enabled, () => document.Views.Redraw(deferred: redraw.Defers)).Map(_ => value));
        });

    private static Fin<T> Append<T>(Fin<T> primary, Fin<Unit> side) => primary.BiBind(
        Succ: value => side.Map(_ => value),
        Fail: error => side.Match(
            Succ: _ => Fin.Fail<T>(error: error),
            Fail: fault => Fin.Fail<T>(error: error + fault)));
}

internal static class DocumentCommit {
    internal static Fin<TReceipt> Sealed<TReceipt>(
        RhinoDoc document,
        string name,
        bool recordsUndo,
        RedrawPolicy redraw,
        Func<Fin<TReceipt>> run,
        Func<TReceipt, uint, TReceipt> stamp,
        Op op) =>
        Sealed(
            document: document,
            name: name,
            recordsUndo: recordsUndo,
            redraw: redraw,
            run: run,
            stamp: stamp,
            project: static receipt => Fin.Succ(value: receipt),
            op: op);

    internal static Fin<TOut> Sealed<TReceipt, TOut>(
        RhinoDoc document,
        string name,
        bool recordsUndo,
        RedrawPolicy redraw,
        Func<Fin<TReceipt>> run,
        Func<TReceipt, uint, TReceipt> stamp,
        Func<TReceipt, Fin<TOut>> project,
        Op op) =>
        RedrawScope.Within(document: document, redraw: redraw, key: op, body: () => op.Catch(() => {
            using UndoBracket undo = UndoBracket.Begin(document: document, name: name, recordsUndo: recordsUndo);
            Func<TReceipt, Fin<TReceipt>> stamped = undo.Stamper<TReceipt>(stamp: stamp, key: op);
            Fin<TOut> executed = guard(undo.Admitted, op.InvalidResult()).ToFin()
                .Bind(_ => op.Catch(run))
                .Bind(stamped)
                .Bind(receipt => op.Catch(() => project(receipt)));
            return undo.Seal(outcome: executed, key: op);
        }));

    internal static Fin<Seq<TKey>> Compensated<TSource, TKey>(
        Seq<TSource> source, Func<TSource, Fin<TKey>> land, Func<Seq<TKey>, Fin<Unit>> rollback) =>
        Compensated(source: source, land: land, rollback: rollback, release: static _ => Fin.Succ(value: unit));

    internal static Fin<Seq<TKey>> Compensated<TSource, TKey>(
        Seq<TSource> source,
        Func<TSource, Fin<TKey>> land,
        Func<Seq<TKey>, Fin<Unit>> rollback,
        Func<Seq<TSource>, Fin<Unit>> release) {
        (Seq<TKey> Landed, Option<Error> Fault) outcome = source.Fold(
            (Landed: Seq<TKey>(), Fault: default(Option<Error>)),
            (state, value) => state.Fault.IsSome ? state : land(value).Match(
                Succ: key => (state.Landed.Add(key), default(Option<Error>)),
                Fail: error => (state.Landed, Some(error))));
        return outcome.Fault.Match(
            Some: cause => Unwound<TKey>(primary: cause, rollback(outcome.Landed), release(source)),
            None: () => release(source).Match(
                Succ: _ => Fin.Succ(value: outcome.Landed),
                Fail: cause => Unwound<TKey>(primary: cause, rollback(outcome.Landed))));
    }

    private static Fin<Seq<TKey>> Unwound<TKey>(Error primary, params ReadOnlySpan<Fin<Unit>> compensation) =>
        Fin.Fail<Seq<TKey>>(error: toSeq(compensation.ToArray()).Fold(primary, static (fault, step) => step.Match(
            Succ: _ => fault,
            Fail: error => fault + error)));
}

internal ref struct UndoBracket {
    private readonly RhinoDoc document;
    private readonly uint serial;
    private readonly bool required;
    private readonly bool owned;
    private readonly bool enlisted;
    private bool closed;
    private bool terminal;

    private abstract record Execution<TReceipt> {
        internal sealed record Succeeded(TReceipt Receipt) : Execution<TReceipt>;
        internal sealed record Failed(Error Error) : Execution<TReceipt>;
    }

    private abstract record Closure {
        internal sealed record Closed(Option<Error> Recovered) : Closure;
        internal sealed record Open(Error Error) : Closure;
    }

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

    public Func<TReceipt, Fin<TReceipt>> Stamper<TReceipt>(Func<TReceipt, uint, TReceipt> stamp, Op key) {
        bool stamps = required;
        uint record = serial;
        return receipt => !stamps
            ? Fin.Succ(value: receipt)
            : from fold in key.Need(stamp)
              from stamped in key.Catch(() => Fin.Succ(value: fold(receipt, record)))
              select stamped;
    }

    public Fin<TReceipt> Seal<TReceipt>(Fin<TReceipt> outcome, Op key) {
        if (terminal) {
            return Fin.Fail<TReceipt>(error: key.InvalidResult());
        }
        terminal = true;
        RhinoDoc owner = document;
        bool ownsRecord = owned;
        bool joinsRecord = enlisted;
        Execution<TReceipt> execution = outcome.Match<Execution<TReceipt>>(
            Succ: static receipt => new Execution<TReceipt>.Succeeded(Receipt: receipt),
            Fail: static error => new Execution<TReceipt>.Failed(Error: error));
        Closure closure = CloseBounded(key: key).Match<Closure>(
            Succ: static recovered => new Closure.Closed(Recovered: recovered),
            Fail: static error => new Closure.Open(Error: error));
        return (execution, closure) switch {
            (Execution<TReceipt>.Succeeded _, Closure.Closed { Recovered.Case: Error recovered }) =>
                Fin.Fail<TReceipt>(error: recovered),
            (Execution<TReceipt>.Succeeded success, Closure.Closed _) => Fin.Succ(value: success.Receipt),
            (Execution<TReceipt>.Succeeded _, Closure.Open close) => Fin.Fail<TReceipt>(error: close.Error + key.Caution(
                concern: "undo record remains open after bounded close recovery")),
            (Execution<TReceipt>.Failed failed, Closure.Closed close) => Rollback<TReceipt>(
                    document: owner,
                    owned: ownsRecord,
                    enlisted: joinsRecord,
                    primary: close.Recovered.Map(error => failed.Error + error).IfNone(failed.Error),
                    key: key),
            (Execution<TReceipt>.Failed failed, Closure.Open close) => Fin.Fail<TReceipt>(error: failed.Error
                + close.Error
                + key.Caution(concern: "undo record could not close, so rollback was not executed")),
        };
    }

    public void Dispose() {
        if (terminal) {
            return;
        }
        terminal = true;
        _ = CloseBounded(key: Op.Of());
    }

    private Fin<Option<Error>> CloseBounded(Op key) => Close(key: key).BiBind(
        Succ: static _ => Fin.Succ(Option<Error>.None),
        Fail: first => Close(key: key)
            .Map(_ => Some(first))
            .BindFail(second => Fin.Fail<Option<Error>>(error: first + second)));

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

- Owner: `TableSlot` `[SmartEnum<int>]` names object consequences. `TableFact` `[Union]` carries object runtime evidence, component-table tallies, named-view restores, history navigation, undo serials, and custom-undo names without a second payload discriminator. `TableReceipt` is the additive fold over that one fact stream. `IFactSlot<TBody>` and `FactStream<TSlot, TBody>` are the PARAMETERIZED form of that same machinery, owned once here for every mutation folder whose facts accumulate per operation inside one commit.
- Law: the fact stream is one owner, the vocabularies are the folder's — `FactStream<TSlot, TBody>` holds the accumulation, the `Admits` cross-product gate, the undo-stamp projection, and the slot-keyed reader, while each folder contributes only a `[SmartEnum<int>]` slot vocabulary implementing `IFactSlot<TBody>` and a `[Union]` body family; its own mint factories ride an extension block over the closed instantiation, so a third mutation folder joins by declaring two vocabularies and gains every projection with zero new surface. A folder re-minting a receipt, a fact, a gate, or a projection beside it is the deleted form.
- Law: `Admits` is total in both directions and names its refusal — a slot cannot compile without declaring the bodies it emits, a body cannot enter under a slot that does not name it, and the refusal carries the slot key, because "this receipt rejected a body" is unactionable where "slot 7 does not emit a path" is not.
- Law: the undo stamp is a PROJECTION on the stream, never a rail — `DocumentCommit.Sealed` stamps every sealed receipt including a program that opened no record, `UndoSerial` refuses that zero, so an unrecorded program contributes no fact rather than one claiming record zero, and the total `(receipt, serial) -> receipt` shape holds.
- Boundary: Modeling's `BuildReceipt<TSlot>`/`Built<TSlot>` is a different TIMING CLASS and does not collapse into this owner — it carries build-product evidence bound to one produced value, minted outside `DocumentCommit.Sealed` and read by the builder that produced it, where this stream accumulates consequence facts across every operation inside one commit and is sealed by the undo stamp; merging them would put a commit-scoped undo column on a value that never enters a commit.
- Boundary: the COMMIT ENTRY does not unify. `DraftPlan<TOp>`, `BlockTransaction`, and `TableTransaction` share a four-field carrier — name, program, redraw, undo recording — and nothing else: Draft admits through a `DraftMode` row bundling redraw and recording, Block admits through per-operation trait homogeneity plus a kernel-context census, and Table admits three structurally distinct program shapes with custom-undo handlers and navigation semantics. One carrier under three incompatible admissions is a shape no caller can hold polymorphically, so the merge is refused; what the three genuinely share — the bracket, the redraw scope, and the seal — is already `DocumentCommit.Sealed`, and that IS the unified entry.
- Entry: `Ids(TableSlot, Op?)` and `Runtime(TableSlot, Op?)` fail closed on an invalid slot and project object consequences; `Components`, `Restores`, `History`, `UndoRecords`, and `CustomUndoNames` project the remaining fact cases. A receipt can feed its deleted runtime rows directly into `TableTarget.Deleted`.
- Law: `UndoBracket` is receipt-agnostic: every folder commit rail — table, layer, session-regime, annotation draft, block, object, render content, render settings, exchange, sheet, preset, user-text, capture-adopt — folds the sealed serial into its own receipt through `DocumentCommit.Sealed` without a foreign-receipt hop. `Stamper` and the railed projection execute inside the bracket before sealing, so a stamp or projection fault remains rollback-capable.
- Law: `Seal` owns bounded close recovery and the terminal rollback decision. `Execution × Closure` is one total tuple switch: success requires a fault-free close, recovered close faults fail successful execution, failed execution rolls back after recovered close, and unrecoverable close reports rollback as unexecuted. `Dispose` cannot re-enter close after any seal attempt.
- Law: fact construction remains internal to the receipt. Generated `ObjectRuntime` identity, component-kind presence, nonnegative tallies, and the `DocumentCommit.Sealed` positive-serial proof guard every fact entering the public stream; invalid evidence fails instead of disappearing as an empty receipt.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// The contract a mutation folder contributes to the shared stream: a keyed row that names the body kinds its
// slot emits. A `[SmartEnum<int>]` satisfies it with one `[UseDelegateFromConstructor] Admits` column and
// nothing else — the key is already generated — so a folder joining the stream declares a slot vocabulary and a
// body union and inherits the accumulation, the gate, and every projection.
public interface IFactSlot<in TBody> where TBody : class {
    int Key { get; }
    bool Admits(TBody body);
}

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
    internal sealed record RestoreCase(NamedRestore Value) : TableFact;
    internal sealed record HistoryCase(HistoryRoll Value) : TableFact;
    internal sealed record UndoCase(uint Serial) : TableFact;
    internal sealed record CustomUndoCase(string Name) : TableFact;
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct Fact<TSlot, TBody>(TSlot Slot, TBody Body)
    where TSlot : class, IFactSlot<TBody>
    where TBody : class;

// The per-op fact accumulation two mutation folders had built twice, verbatim: a slot vocabulary, a body union,
// a fact pairing them, and a receipt folding facts across one commit. Only the vocabularies were folder-specific
// — the machinery, the cross-product gate, and the receipt algebra were the same code under two names, so a fix
// to one (the gate naming the offending slot, the zero-serial projection) landed on one and not the other.
//
// `Admits` is THE gate and it is total in both directions: a slot cannot exist without declaring the bodies it
// emits, and a body cannot enter the stream under a slot that does not name it. The refusal carries the slot's
// key, because "this receipt rejected a body" is unactionable and "slot 7 does not emit a path" is not.
//
// GROWTH: a third mutation folder joins by declaring a `[SmartEnum<int>]` slot vocabulary implementing
// `IFactSlot<TBody>` and a `[Union]` body family — no receipt type, no fact type, no projection, no gate. Its
// own mint factories ride an extension block over the closed instantiation, so its call sites read as its own.
//
// NOT THIS: Modeling's `BuildReceipt<TSlot>`/`Built<TSlot>` is a different TIMING CLASS and stays where it is —
// it carries build-product evidence bound to one produced value (geometry plus its bench band), minted outside
// any commit envelope and read by the builder that produced it, whereas this stream accumulates consequence
// facts across every operation inside ONE commit and is sealed by the undo stamp. Collapsing them would put a
// commit-scoped undo column on a value that never enters a commit.
public readonly record struct FactStream<TSlot, TBody> : IDetachedDocumentResult
    where TSlot : class, IFactSlot<TBody>
    where TBody : class {
    private readonly Seq<Fact<TSlot, TBody>> facts;

    private FactStream(Seq<Fact<TSlot, TBody>> facts) => this.facts = facts;

    public static FactStream<TSlot, TBody> Empty { get; } = new(facts: Seq<Fact<TSlot, TBody>>());

    public Seq<Fact<TSlot, TBody>> Facts => facts;

    public static FactStream<TSlot, TBody> operator +(FactStream<TSlot, TBody> left, FactStream<TSlot, TBody> right) =>
        new(facts: left.facts + right.facts);

    public static Fin<FactStream<TSlot, TBody>> Of(TSlot slot, TBody body, Op key) =>
        from row in key.Need(value: slot)
        from payload in key.Need(value: body)
        from _ in guard(
            row.Admits(body: payload),
            key.InvalidResult(detail: row.Key.ToString(CultureInfo.InvariantCulture))).ToFin()
        select new FactStream<TSlot, TBody>(facts: Seq(new Fact<TSlot, TBody>(Slot: row, Body: payload)));

    public static Fin<FactStream<TSlot, TBody>> All(TSlot slot, Seq<TBody> bodies, Op key) =>
        bodies
            .Traverse(body => Of(slot: slot, body: body, key: key).ToValidation())
            .As()
            .ToFin()
            .Map(static streams => streams.Fold(Empty, static (state, next) => state + next));

    // The commit envelope stamps EVERY sealed receipt, including a program that recorded no undo — that serial is
    // `0u` and `UndoSerial` refuses zero — so the stamp is a projection, not a rail: an unrecorded program
    // contributes no fact rather than one asserting record zero, and the total `(receipt, serial) -> receipt`
    // shape `DocumentCommit.Sealed` demands holds. The gate still runs, so a folder whose undo slot does not
    // declare its record body stamps nothing instead of smuggling a body past the cross product.
    public FactStream<TSlot, TBody> Stamped(TSlot slot, Func<UndoSerial, TBody> record, uint serial) =>
        UndoSerial.Maybe(value: serial)
            .Map(record)
            .Filter(slot.Admits)
            .Map(body => this + new FactStream<TSlot, TBody>(
                facts: Seq(new Fact<TSlot, TBody>(Slot: slot, Body: body))))
            .IfNone(noneValue: this);

    public Seq<T> Project<T>(TSlot slot, Func<TBody, Option<T>> select) =>
        facts.Filter(fact => fact.Slot == slot).Choose(fact => select(fact.Body));

    public int FactCount(TSlot slot) => facts.Count(fact => fact.Slot == slot);
}

public readonly record struct TableReceipt : IDetachedDocumentResult {
    private readonly Seq<TableFact> facts;

    private TableReceipt(Seq<TableFact> facts) => this.facts = facts;

    public static TableReceipt Empty { get; } = new(facts: Seq<TableFact>());

    public static TableReceipt operator +(TableReceipt left, TableReceipt right) =>
        new(facts: left.facts + right.facts);

    internal static TableReceipt Objects(TableSlot slot, Seq<ObjectRuntime> values) =>
        new(facts: ObjectRuntime.Canonical(values: values)
            .Map(value => (TableFact)new TableFact.ObjectCase(Slot: slot, Value: value)));

    internal static Fin<TableReceipt> Component(TableKind kind, int tally, Op key) =>
        from admitted in key.Need(kind)
        from _ in guard(tally >= 0, key.InvalidResult()).ToFin()
        select Of(fact: new TableFact.ComponentCase(Kind: admitted, Tally: tally));

    internal static TableReceipt Restore(NamedRestore value) => Of(fact: new TableFact.RestoreCase(Value: value));

    internal static TableReceipt History(HistoryRoll value) => Of(fact: new TableFact.HistoryCase(Value: value));

    internal static TableReceipt Undo(uint serial) => Of(fact: new TableFact.UndoCase(Serial: serial));

    private static TableReceipt Of(TableFact fact) => new(facts: Seq(fact));

    internal static TableReceipt CustomUndo(Seq<string> names) =>
        new(facts: names
            .Map(static name => (TableFact)new TableFact.CustomUndoCase(Name: name)));

    internal static TableReceipt SelectionDelta(Seq<ObjectRuntime> before, Seq<ObjectRuntime> after) =>
        Objects(slot: TableSlot.Selected, values: after.Filter(value => !before.Exists(item => item.Equals(value))))
        + Objects(slot: TableSlot.Unselected, values: before.Filter(value => !after.Exists(item => item.Equals(value))));

    public Fin<Seq<Guid>> Ids(TableSlot slot, Op? key = null) =>
        Runtime(slot: slot, key: key).Map(static values => values.Map(static value => value.Id));

    public Fin<Seq<ObjectRuntime>> Runtime(TableSlot slot, Op? key = null) =>
        Optional(slot).ToFin(Fail: key.OrDefault().InvalidInput()).Map(admitted =>
            ObjectRuntime.Canonical(values: facts.Choose(fact =>
                fact is TableFact.ObjectCase { Slot: var factSlot, Value: var value }
                        && factSlot == admitted
                    ? Some(value)
                    : Option<ObjectRuntime>.None)));

    public Seq<(TableKind Kind, int Tally)> Components =>
        facts.Choose(static fact => fact is TableFact.ComponentCase component
            ? Some((component.Kind, component.Tally))
            : Option<(TableKind, int)>.None);

    public Seq<NamedRestore> Restores =>
        facts.Choose(static fact => fact is TableFact.RestoreCase restore
            ? Some(restore.Value)
            : Option<NamedRestore>.None);

    public Seq<HistoryRoll> History =>
        facts.Choose(static fact => fact is TableFact.HistoryCase history
            ? Some(history.Value)
            : Option<HistoryRoll>.None);

    public Seq<uint> UndoRecords =>
        facts.Choose(static fact => fact is TableFact.UndoCase undo
            ? Some(undo.Serial)
            : Option<uint>.None);

    public Seq<string> CustomUndoNames =>
        facts.Choose(static fact => fact is TableFact.CustomUndoCase undo
            ? Some(undo.Name)
            : Option<string>.None);

    public Fin<int> Count(TableSlot slot, Op? key = null) =>
        Runtime(slot: slot, key: key).Map(static values => values.Count);
}
```

## [06]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]              | [OWNER]                          | [FORM]                     | [ENTRY]                               |
| :-----: | :--------------------- | :------------------------------- | :------------------------- | :------------------------------------ |
|  [01]   | document tables        | `TableKind`                      | keyed behavior rows        | `ForComponentType` / `Reclaim`        |
|  [02]   | object addressing      | `TableTarget`                    | ids/runtime/query union    | `Of` / `Deleted` / `Query`            |
|  [03]   | query predicates       | `TablePredicate`                 | frozen predicate union     | `Tag` / `Color` / `Bounds`            |
|  [04]   | query product          | `QuerySpec`                      | twenty-one-axis value      | `Of` / `Build(document, key)`         |
|  [05]   | mutation program       | `TableOp`                        | admitted total union       | operation factories / `Apply`         |
|  [06]   | attribute payload      | `AttributeChange`                | admitted mutation body     | `TableOp.Amend`                       |
|  [07]   | commit scope           | `TableTransaction`               | program-mode union         | `Recorded` / `Immediate` / `Navigate` |
|  [08]   | resource ingress       | `GeometryIntake`                 | native/value custody union | `Admit`                               |
|  [09]   | table commit spine     | `Tables`                         | session/redraw fold        | `Commit`                              |
|  [10]   | shared commit entry    | `DocumentCommit` / `UndoBracket` | receipt-generic terminal   | `Sealed` / `Compensated` / `Seal`     |
|  [11]   | consequence evidence   | `TableReceipt`                   | runtime fact stream        | typed projections                     |
|  [12]   | undo serial            | `UndoSerial`                     | positive generated value   | `Maybe` / `Sealed` stamp              |
|  [13]   | component addressing   | `ResourceRef` / `ResourceLens`   | id/name/index over a lens  | `Of` / `Resolve(document, lens, key)` |
|  [14]   | redraw bracket         | `RedrawScope`                    | suppress/restore/flush     | `Within(document, redraw, body, key)` |
|  [15]   | viewport addressing    | `ViewportTarget`                 | address & census union     | `Active` / `ResolveViewport`          |
|  [16]   | object-type vocabulary | `ObjectKind` / `ObjectKinds`     | keyed rows over a set      | `Of` / `Any` / `Mask` / `OfMask`      |
|  [17]   | space partition        | `ActiveSpaceUse`                 | host-keyed rows            | `Get` / `Key`                         |
|  [18]   | host dialogue          | `HostInteraction`                | quiet/interactive rows     | `IsQuiet`                             |
|  [19]   | shared fact stream     | `FactStream<TSlot, TBody>`       | slot-gated accumulation    | `Of` / `All` / `Stamped` / `Project`  |
|  [20]   | slot contract          | `IFactSlot<TBody>`               | keyed row with `Admits`    | folder slot vocabularies              |

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
