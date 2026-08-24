# [RASM_RHINO_TABLES]

`Rasm.Rhino.Document` owns document-table vocabulary, object addressing, mutation programs, and consequence evidence. `TableKind` captures each admitted host document table, `TableTarget` freezes explicit, runtime, and host-query addressing, and `TableOp` closes the mutation family. `Tables.Commit` executes one admitted program inside one session capability window, refreshes the kernel `Context`, frames the change through `Document/commit.md`'s `DocumentCommit.Sealed`, and returns one typed fact stream on `Document/facts.md`'s parameterized owner. The commit envelope, the undo bracket, and the redraw scope are that sibling's; the fact-stream machinery is `facts.md`'s; this page contributes the table vocabularies, the program algebra, and its own slot-and-body pair.

## [01]-[INDEX]

- [02]-[TABLE_VOCABULARY]: `TableKind` — document-table identity, component correspondence, and reclamation behavior.
- [03]-[TARGET_ALGEBRA]: `ObjectKind`/`ObjectKinds`, `ActiveSpaceUse`, `ObjectRuntime`, `QueryAxis`, `QuerySpec`, `BoundsMatch`, `TablePredicate`, `TableTarget`, `ViewportTarget`, and the `DefinedView`/`IsoQuadrant` host projection rosters — object-type vocabulary, immutable object addressing, viewport addressing, projection vocabulary, and query composition.
- [04]-[RUN]: policy rows, `SelectionAxis`, `NamedRestore`, `HistoryRoll`, `TableOp`, `TableTransaction`, `GeometryIntake`, and `Tables` — the mutation program and its one commit entry.
- [05]-[RECEIPTS]: `TableBodyKind`, `TableFact`, `TableSlot`, and the `TableReceipt` alias — this page's conformance to the shared fact stream.
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
using Rasm.Domain;
using Rasm.Numerics;
using Rhino;
using Rhino.Display;
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
        Fin.Fail<int>(error: key.Unsupported(inputType: typeof(TableKind), outputType: typeof(int)));
}
```

## [03]-[TARGET_ALGEBRA]

- Owner: `ObjectKind` `[SmartEnum<ObjectType>]` is the corpus-wide OBJECT-TYPE vocabulary and `ObjectKinds` its admitted set, seated here because the concept has consumers at S1 (`Commands` modal object asks) and S2 (`HostUi` properties-page scope) and this spine is the lowest stratum both reach; `Mask` is the one OR-fold, `Any` the host's own catch-all row, and no folder mints a second type table.
- Law: a raw `ObjectType` never crosses a public signature — every filter is `ObjectKinds`, every host member taking the flag reads `Mask` at its own call, and a page needing "every type" composes `ObjectKinds.Any` rather than spelling `ObjectType.AnyObject`. The same law binds `MeshType`: the raw host mesh discriminant crosses only as `Objects/materials.md`'s `MeshKind` row.
- Owner: `ActiveSpaceUse` `[SmartEnum<ActiveSpace>]` is the space partition, seated here beside the enumerator's own `SpaceFilter` because an attribute set carries it at S2 and a conduit criterion and a gumball seat read it at S4; the roster mirrors the host enum completely, so `Get` is total over any value a host read returns and the row's `Key` is the one write.
- Owner: `ObjectRuntime` `[ComplexValueObject]` admits the durable `(Guid, runtime serial)` pair required after an object leaves the active-id index. `QueryAxis` is the fourteen-row inclusion vocabulary whose every row carries its own host setter; `QuerySpec` `[ComplexValueObject]` holds those axes as ONE `CapabilitySet<QueryAxis>` beside the six filter columns and BUILDS the host settings inside the document callback, so the mutable host settings type never crosses a signature, every host sentinel is an `Option` whose absence reads as "every row", and the live `ViewportFilter` slot is the stable `ViewportTarget` owner. `TableTarget` `[Union]` closes nonempty explicit ids, nonempty runtime pairs, and admitted queries. `TablePredicate` `[Union]` adds composable tag, draw-color, and kernel-bounds predicates; `BoundsMatch` owns containment versus intersection behavior as rows, each comparison banded on the kernel `Duplicate` tolerance lane.
- Law: the query axes are a SET over a vocabulary whose rows OWN their host writes — `Build` is one fold over `QueryAxis.Items`, so the fourteen parallel bool columns, their fourteen `Of` options, and the fourteen hand assignments delete together and a fifteenth host axis is one row no consumer edits. The three admission constraints the product carries — fast selection demands the selected-only filter (the host silently drops it otherwise), at least one state axis, at least one category axis — state ONCE at construction over the set, so a spec cannot carry a knob the host will silently drop. The kernel `CapabilityLaw` carries corner SETS alone, so these pairwise and quantified clauses ride the owner's own admission rather than a law value.
- Owner: `ViewportTarget` `[Union]` is the corpus-wide VIEWPORT address — active, named, id, page, detail, and census cases closed as one owner beside `TableKind` (which table), `TableTarget` (which objects), and `ResourceRef` (which component). `ViewportScope` `[SmartEnum<int>]` carries the model, page, and detail census generators and `EveryCase` freezes their set; `ViewportRef` is the ephemeral resolved row pairing `RhinoView`, `RhinoViewport`, and an optional `DetailViewObject`. `Active`/`Named`/`Id`/`Page`/`Detail`/`Every` construct, and `Resolve`, `ResolveOne`, and `ResolveViewport` fold one address to every row, exactly one row, or one native viewport inside the caller's document callback.
- Law: viewport resolution names `RhinoDoc.Views.ActiveView`, `.Find`, `.GetViewList`, `.GetPageViews`, `RhinoPageView.GetDetailViews`, and `DetailViewObject.Viewport` exactly once; a detail address matches either `DetailViewObject.Id` or `DetailViewObject.Viewport.Id`, and a resolution yielding no row refuses before any consumer projects it.
- Law: an addressed row binds `RhinoView.MainViewport` — the viewport the address names — because `RhinoPageView.ActiveViewport` silently returns an active detail; only `ActiveCase` binds `ActiveViewport`, adopting the host's active semantics, and a detail row binds `DetailViewObject.Viewport` and carries its `DetailViewObject` so a detail commit or scale conversion reads the owning object without a second lookup.
- Law: viewport rows resolve live per call inside the document callback and leave as detached addresses or one native viewport, never a retained handle; `ResolveViewport` composes `Resolve` and `Tables.One`, so the single-viewport consumers — `QuerySpec` viewport filtering, `NamedRestore`, and the annotation dimension-scale probe — share one fold and no call site re-spells the resolve-then-one triple.
- Owner: `ResourceRef` is the corpus-wide COMPONENT address — id, name, index closed as one `[Union]` over a per-table `ResourceLens<TComponent>` — completing the addressing triad beside `TableKind` (which table) and `TableTarget` (which objects); `ResourceId`, `ResourceName`, and `ResourceIndex` admit the native address scalars once, and the `ResourceId` and `ResourceIndex` `Maybe`/`Admit` pairs are the sole `Guid.Empty` and negative-index sentinel projectors — `Maybe` where the host miss value spells a normal absence, `Admit` where it is a genuine refusal.
- Law: each component table contributes exactly one lens — Annotation's style, linetype, hatch, and section rails and Blocks' definition rail each declare one `ResourceLens<T>` row — and no folder mints a second address family; resolution reads live per call inside the owning operation, because tables mutate under commands, so no resolved component is cached on a value.
- Entry: `QuerySpec.Of(...)`, `TableTarget.Of(params ReadOnlySpan<Guid>)`, `Deleted(params ReadOnlySpan<ObjectRuntime>)`, and `Query(QuerySpec, params ReadOnlySpan<TablePredicate>)` are the only constructors. `Resolve` returns distinct ids; `Serials` preserves or resolves runtime pairs. `ObjectRuntime.Canonical` derives every runtime-pair deduplication from generated structural equality. A deleted-object lifecycle request composes `Deleted` from a prior receipt instead of attempting `FindId`, which cannot find deleted objects.
- Law: query settings are BUILT at execution from an admitted value, never copied from a caller's instance — the host settings object exists only inside `QuerySpec.Build`, so no caller retains a handle that can mutate an admitted target, and the viewport resolves from stable identity inside the document callback. Predicate evaluation accumulates independent object and predicate faults through `Validation<Error, T>` before lowering once to `Fin<T>`.
- Law: a predicate distinguishes NON-MATCH from HOST FAULT — a missing tag is a non-match, a missing attribute set is a refusal, because folding both onto `false` drops an unreadable object out of every filtered query with no receipt naming it. Draw-colour comparison lands on the quantized ARGB quadruple of two `PerceptualColor` values: `System.Drawing.Color` equality compares NAME before value, so a named row and its identical literal compare unequal, which is the trap a colour filter walks into on the first system colour.
- Law: bounds predicates admit `BoundingBox.IsValid` before corner accumulation and compose the kernel `BoundsOf` owner; the containment and intersection comparisons read the kernel `Duplicate` tolerance lane admitted ONCE at the factory from the caller's `Context`, so an exact float equality never decides a near-coincident box and no site mints an epsilon. Inflation remains host-query policy, while candidate classification and coercion stay kernel-owned.
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
[ValidationError]
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
[ValidationError]
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

// The fourteen inclusion axes of `ObjectEnumeratorSettings` as a vocabulary whose rows OWN their host writes: the
// row's `Seat` column threads the settings through, so `QuerySpec.Build` is one fold over `Items` and a fifteenth
// host axis is one row no consumer edits.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class QueryAxis : ICapability<QueryAxis> {
    public static readonly QueryAxis Normal = new(key: "normal", seat: static (settings, held) => { settings.NormalObjects = held; return settings; });
    public static readonly QueryAxis Locked = new(key: "locked", seat: static (settings, held) => { settings.LockedObjects = held; return settings; });
    public static readonly QueryAxis Hidden = new(key: "hidden", seat: static (settings, held) => { settings.HiddenObjects = held; return settings; });
    public static readonly QueryAxis InDefinitions = new(key: "in-definitions", seat: static (settings, held) => { settings.IdefObjects = held; return settings; });
    public static readonly QueryAxis Deleted = new(key: "deleted", seat: static (settings, held) => { settings.DeletedObjects = held; return settings; });
    public static readonly QueryAxis Active = new(key: "active", seat: static (settings, held) => { settings.ActiveObjects = held; return settings; });
    public static readonly QueryAxis Referenced = new(key: "referenced", seat: static (settings, held) => { settings.ReferenceObjects = held; return settings; });
    public static readonly QueryAxis Lights = new(key: "lights", seat: static (settings, held) => { settings.IncludeLights = held; return settings; });
    public static readonly QueryAxis Grips = new(key: "grips", seat: static (settings, held) => { settings.IncludeGrips = held; return settings; });
    public static readonly QueryAxis Phantoms = new(key: "phantoms", seat: static (settings, held) => { settings.IncludePhantoms = held; return settings; });
    public static readonly QueryAxis SubObjectSelected = new(key: "sub-object-selected", seat: static (settings, held) => { settings.SubObjectSelected = held; return settings; });
    public static readonly QueryAxis SelectedOnly = new(key: "selected-only", seat: static (settings, held) => { settings.SelectedObjectsFilter = held; return settings; });
    public static readonly QueryAxis FastSelection = new(key: "fast-selection", seat: static (settings, held) => { settings.UseFastSelection = held; return settings; });
    public static readonly QueryAxis VisibleOnly = new(key: "visible-only", seat: static (settings, held) => { settings.VisibleFilter = held; return settings; });

    [UseDelegateFromConstructor]
    internal partial ObjectEnumeratorSettings Seat(ObjectEnumeratorSettings settings, bool held);

    // The host constructor's own posture — normal-or-locked, active — so a caller states only what it narrows.
    // Accessor-backed: the generated roster fills from its own static constructor.
    public static CapabilitySet<QueryAxis> Baseline => Seed.Value;
    private static readonly Lazy<CapabilitySet<QueryAxis>> Seed =
        new(static () => CapabilitySet<QueryAxis>.Of(Normal, Locked, Active));
}

// The complete `ObjectEnumeratorSettings` product as a VALUE: the axes ride ONE set column, each sentinel filter is
// an `Option` whose absence means "every row", the type filter is `ObjectKinds`, the index filters are
// `ResourceIndex`, and the viewport axis is the stable `ViewportTarget` resolved inside the document callback.
[ComplexValueObject]
[ValidationError]
public sealed partial class QuerySpec {
    public CapabilitySet<QueryAxis> Axes { get; }
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
        ref CapabilitySet<QueryAxis> axes,
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
            || (axes.Admits(capability: QueryAxis.FastSelection) && !axes.Admits(capability: QueryAxis.SelectedOnly))
            || !(axes.Admits(capability: QueryAxis.Normal) || axes.Admits(capability: QueryAxis.Locked)
                || axes.Admits(capability: QueryAxis.Hidden) || axes.Admits(capability: QueryAxis.InDefinitions)
                || axes.Admits(capability: QueryAxis.Deleted))
            || !(axes.Admits(capability: QueryAxis.Active) || axes.Admits(capability: QueryAxis.Referenced))
            ? new ValidationError(message: "Object query spec is incomplete.")
            : null;

    public static Fin<QuerySpec> Of(
        Option<CapabilitySet<QueryAxis>> axes = default,
        Option<ObjectKinds> kinds = default,
        Option<Type> shape = default,
        Option<ResourceIndex> layer = default,
        Option<ResourceIndex> material = default,
        Option<string> name = default,
        Option<ActiveSpaceUse> space = default,
        Option<ViewportTarget> viewport = default,
        Op? key = null) =>
        key.OrDefault().AcceptValidated<QuerySpec>(
            fault: Validate(
                axes.IfNone(QueryAxis.Baseline),
                kinds.IfNone(ObjectKinds.Any),
                shape,
                layer,
                material,
                name,
                space.IfNone(ActiveSpaceUse.None),
                viewport,
                out QuerySpec? admitted),
            admitted: admitted);

    internal bool SelectsOnlyDeleted =>
        Axes.Admits(capability: QueryAxis.Deleted)
        && !Axes.Admits(capability: QueryAxis.Normal)
        && !Axes.Admits(capability: QueryAxis.Locked)
        && !Axes.Admits(capability: QueryAxis.Hidden)
        && !Axes.Admits(capability: QueryAxis.InDefinitions);

    // The one place a host settings object exists, minted fresh per execution inside the document callback: the
    // fourteen inclusion axes land as ONE fold over the vocabulary — each row seats its own host member — the
    // absent filters write the host's own "everything" sentinels explicitly, the two host slots the domain never
    // reads back cross through `Op.ToHostSlot`, and nothing the caller holds can reach the result.
    internal Fin<ObjectEnumeratorSettings> Build(RhinoDoc document, Op key) =>
        Viewport
            .Traverse(target => target.ResolveViewport(document: document, key: key))
            .As()
            .Map(resolved => {
                ObjectEnumeratorSettings settings = toSeq(QueryAxis.Items).Fold(
                    new ObjectEnumeratorSettings(),
                    (held, axis) => axis.Seat(settings: held, held: Axes.Admits(capability: axis)));
                settings.ObjectTypeFilter = Kinds.Mask;
                settings.ClassTypeFilter = Op.ToHostSlot(Shape);
                settings.LayerIndexFilter = Layer.Map(static value => value.Value).IfNone(noneValue: AnyIndex);
                settings.MaterialIndexFilter = Material.Map(static value => value.Value).IfNone(noneValue: AnyMaterial);
                settings.NameFilter = Name.IfNone(AnyName);
                settings.SpaceFilter = Space.Key;
                settings.ViewportFilter = Op.ToHostSlot(resolved);
                return settings;
            });

    // The host's own "no filter" sentinels, named once so no arm re-spells a magic number.
    private const int AnyIndex = -1;
    private const int AnyMaterial = int.MinValue + 1;
    private const string AnyName = "*";
}

// Containment and intersection banded on the kernel `Duplicate` lane: an exact float compare decided a
// near-coincident box by representation noise, and the lane read is what makes the band one row rather than an
// epsilon minted per site.
[SmartEnum]
public sealed partial class BoundsMatch {
    public static readonly BoundsMatch Intersects = new(static (region, candidate, band) =>
        Math.Abs(region.Center.X - candidate.Center.X) * 2.0 <= region.Diagonal.X + candidate.Diagonal.X + band.Value
        && Math.Abs(region.Center.Y - candidate.Center.Y) * 2.0 <= region.Diagonal.Y + candidate.Diagonal.Y + band.Value
        && Math.Abs(region.Center.Z - candidate.Center.Z) * 2.0 <= region.Diagonal.Z + candidate.Diagonal.Z + band.Value);
    public static readonly BoundsMatch Contains = new(static (region, candidate, band) =>
        Math.Abs(region.Center.X - candidate.Center.X) * 2.0 + candidate.Diagonal.X <= region.Diagonal.X + band.Value
        && Math.Abs(region.Center.Y - candidate.Center.Y) * 2.0 + candidate.Diagonal.Y <= region.Diagonal.Y + band.Value
        && Math.Abs(region.Center.Z - candidate.Center.Z) * 2.0 + candidate.Diagonal.Z <= region.Diagonal.Z + band.Value);

    [UseDelegateFromConstructor]
    internal partial bool Test(BoundingBox region, BoundingBox candidate, Tolerance band);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TablePredicate {
    private TablePredicate() { }

    private sealed record TagCase(string Key, Option<string> Expected) : TablePredicate;
    private sealed record ColorCase(PerceptualColor Value) : TablePredicate;
    private sealed record BoundsCase(BoundingBox Region, BoundsMatch Match, Tolerance Band) : TablePredicate;

    public static Fin<TablePredicate> Tag(string name, Option<string> expected = default, Op? key = null) =>
        key.OrDefault().AcceptText(value: name).Map(valid => (TablePredicate)new TagCase(Key: valid, Expected: expected));

    public static Fin<TablePredicate> Color(PerceptualColor value, Op? key = null) =>
        key.OrDefault().Need(value).Map(static admitted => (TablePredicate)new ColorCase(Value: admitted));

    // The band admits ONCE from the caller's context — `Context.For` is the branch's one tolerance read — and
    // rides the case, so the evaluation site holds no context and mints no epsilon.
    public static Fin<TablePredicate> Bounds(BoundingBox region, BoundsMatch match, Context context, double inflation = 0.0, Op? key = null) {
        Op op = key.OrDefault();
        return from _ in guard(region.IsValid, op.InvalidInput()).ToFin()
               from predicate in (
                op.Need(match).ToValidation(),
                op.Need(context).ToValidation(),
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
                   .Apply((relation, domain, _, _) => (TablePredicate)new BoundsCase(
                       Region: Inflated(region: region, amount: inflation),
                       Match: relation,
                       Band: domain.For(lane: ToleranceLane.Duplicate)))
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
                .Map(candidate => predicate.Match.Test(region: predicate.Region, candidate: candidate, band: predicate.Band)));

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


// The two host projection rosters, seated at the Document tier because Blocks previews and Viewport projection
// requests admit the SAME host enums. Roster decompile-verified: `Rhino.Display.DefinedViewportProjection` {None,
// Top, Bottom, Left, Right, Front, Back, Perspective, TwoPointPerspective} and `Rhino.Display.IsometricCamera`
// {None, Northeast, Northwest, Southeast, Southwest} (since 8.10). `None` carries no row on either — a request
// naming no projection or no camera is unrepresentable, and a read-back resolves through `Op.Row<THostEnum, TRow>`.
[SmartEnum<int>]
public sealed partial class DefinedView {
    public static readonly DefinedView Top = new(key: (int)DefinedViewportProjection.Top);
    public static readonly DefinedView Bottom = new(key: (int)DefinedViewportProjection.Bottom);
    public static readonly DefinedView Left = new(key: (int)DefinedViewportProjection.Left);
    public static readonly DefinedView Right = new(key: (int)DefinedViewportProjection.Right);
    public static readonly DefinedView Front = new(key: (int)DefinedViewportProjection.Front);
    public static readonly DefinedView Back = new(key: (int)DefinedViewportProjection.Back);
    public static readonly DefinedView Perspective = new(key: (int)DefinedViewportProjection.Perspective);
    public static readonly DefinedView TwoPoint = new(key: (int)DefinedViewportProjection.TwoPointPerspective);

    internal DefinedViewportProjection Native => (DefinedViewportProjection)Key;
}

[SmartEnum<int>]
public sealed partial class IsoQuadrant {
    public static readonly IsoQuadrant Northeast = new(key: (int)IsometricCamera.Northeast);
    public static readonly IsoQuadrant Northwest = new(key: (int)IsometricCamera.Northwest);
    public static readonly IsoQuadrant Southeast = new(key: (int)IsometricCamera.Southeast);
    public static readonly IsoQuadrant Southwest = new(key: (int)IsometricCamera.Southwest);

    internal IsometricCamera Native => (IsometricCamera)Key;
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
[ValidationError]
public readonly partial struct ResourceId {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Guid value) =>
        validationError = value != Guid.Empty ? null : new ValidationError(message: "ResourceId requires a non-empty value.");

    internal static Option<ResourceId> Maybe(Guid value) => Optional(value).Filter(static id => id != Guid.Empty).Map(Create);

    internal static Fin<ResourceId> Admit(Guid value, Op key) => Maybe(value).ToFin(Fail: key.InvalidResult());
}

[ValueObject<int>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[ValidationError]
public readonly partial struct ResourceIndex {
    internal const int Absent = -1;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
        validationError = value >= 0 ? null : new ValidationError(message: "ResourceIndex requires a non-negative value.");

    internal static Option<ResourceIndex> Maybe(int value) =>
        value >= 0 ? Some(Create(value)) : Option<ResourceIndex>.None;

    internal static Fin<ResourceIndex> Admit(int value, Op key) => Maybe(value).ToFin(Fail: key.InvalidResult());
}

// The host component tables key their names ordinal-ignore-case, so the comparison POLICY is a declared type
// argument here and a duplicate probe, a name census, and an occupancy guard read one authority instead of each
// passing `StringComparer.OrdinalIgnoreCase` at its own call site.
[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[ValidationError]
public sealed partial class ResourceName {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim() ?? string.Empty;
        validationError = value.Length > 0 ? null : new ValidationError(string.Join(" | ", new object?[] { Op.Of(), nameof(ResourceName) }));
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

## [04]-[RUN]

- Owner: policy vocabularies close every provider-mode discriminant on rows. `SelectionAxis` is the seven-row selection-conduct vocabulary the host's own select and census members read as ONE `CapabilitySet<SelectionAxis>`. `TableOp` `[Union]` carries admitted per-occurrence payloads; `TableTransaction` `[Union]` distinguishes recorded, immediate, and navigation programs by shape, and the `UndoTrait`/`OpTrait` sets derive required versus recorded undo behavior without plan booleans. The commit envelope — `UndoBracket`, `RedrawScope`, `DocumentCommit.Sealed`, and the `HostInteraction` axis — is `Document/commit.md`'s, and this rail composes it.
- Entry: `TableOp` factories admit raw payloads once. `Add` and `Replace` seal heterogeneous geometry inside `GeometryIntake`; its later `Admit` stage applies the fresh kernel `Context`, `Requirement`, and `GeometryForm` lease without exposing the raw value again. `TableTransaction.Recorded`, `Immediate`, and `Navigate` admit program shape before `Tables.Commit(DocumentSession, TableTransaction, project)` enters the host boundary.
- Law: `TransformPolicy.Relocate` reports the transformed identity as `Moved`; `Copy` and `History` report only the minted identity as `Created`. Sources remain unchanged on copy/history paths. Selection facts derive from before/after runtime snapshots, and state facts use separate `Hidden`/`Shown` and `Locked`/`Unlocked` slots.
- Law: the selection conduct is ONE set — seven two-row vocabularies and a seven-column policy product spelled the same seven bits under fourteen names, and the two census axes were literal `true`s inside the census helper, which silently made every selection receipt include lights and grips whatever the caller asked. `SelectionAxis.Baseline` is the host's own default posture, every host argument reads `Admits` at its own call, and a clear's census reads the same two inclusion rows every other selection op reads, so a clear cannot report a different population than the select that preceded it.
- Law: an operation-factory key threads through every constructor as a trailing `Op? key = null`, so one caller-minted key spans a whole program and every refusal names the request that produced it; the three `params`-bearing factories carve out and mint at the entry, because an optional before `params` forecloses the positional spread. A host member reporting failure as `Guid.Empty` — `Add`, `AddOrderedPointCloud`, `Transform` — admits through `ResourceId.Admit`, the spine's one empty-guid projector, so an empty id never enters a receipt as a created object.
- Law: `TableOp.Traits` totally classifies every case onto one of four trait rows — `Sourced`, `Recorded`, `Immediate`, `Navigation` — each carrying its undo, navigation, and kernel-context demands as ONE `CapabilitySet<OpTrait>` column. A host effect that cannot be reversed by the document record enters only an immediate transaction, so a recorded program has no untracked side effect.
- Law: `Amend` owns a duplicated `ObjectAttributes` lease, takes the admitted `AttributeChange` payload, commits the duplicate synchronously, and disposes it before the operation leaves the host boundary. `AttributeChange` is the SEAM type: this spine is S0 and the typed attribute program is S2, so the payload value seats here and the objects page's `AttributeProgram` composes it upward.
- Law: deleted-object operations require a runtime-preserving target. Explicit deleted rows and deleted-object queries preserve runtime serials without re-entering the active-id index, deletion captures runtime pairs before mutation, and receipts project them for a later `Revive` or `Expunge` request.
- Law: `GeometryIntake` is the staged boundary union: `Of` separates native borrowed geometry from value-form conversion, while `Admit` resolves `Kind` and applies `Requirement.ForKind` under the fresh document context. Native geometry remains borrowed; every value-form conversion composes `GeometryForm` and is disposed by `Lease.Use` after the host copies it.
- Law: page import carries `DocumentPath` and re-proves `DocumentFile.ThreeDm` inside the callback. Named-view restore carries `ViewportTarget`, resolves exactly one viewport immediately before the host call, and never retains a live viewport handle in request data; direct, proportional, constant-speed, and constant-time host modalities close as `NamedRestore` cases with delay and speed entering as admitted values.
- Law: `Tables.Commit` keeps the document handle inside one `DocumentSession.Demand`, proving mutation, undo, and redraw needs against one snapshot before the first edit and refreshing the kernel context inside that window; the bracketing, sealing, rollback, and stamp custody are the commit envelope's own laws. The railed receipt projection executes inside the bracket — a consumer fold that must observe the committed receipt enters as `project`, its refusal rolls the owned record back like any operation fault, and the identity projection spells `project: Fin.Succ`, so receipt-shaped rails commit unchanged and no arity twin exists.
- Boundary: `AddCustomUndoEvent` has no host remove counterpart, so the document retains a `TableCustomUndo` handler, its whole captured object graph, and its arbitrary `object` tag until the undo record clears — a retention no `Subscription` can shorten, unlike every other host attachment in the slice. A handler therefore captures DETACHED evidence only: runtime pairs, stamps, admitted values. A captured live `RhinoObject`, `ObjectAttributes`, session, or lease outlives the commit that minted it and is the leak this law forecloses; the events page's process-global custody census carries the matching row.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// The single-axis policies key ON their host bool: the row name at the construction site carries the semantics and
// the mirror property each one carried deletes — `edit.Custody.Key` IS the host argument.
[SmartEnum<bool>]
public sealed partial class ObjectCustody {
    public static readonly ObjectCustody Resident = new(key: false);
    public static readonly ObjectCustody Reference = new(key: true);
}

[SmartEnum<bool>]
public sealed partial class ModeRegard {
    public static readonly ModeRegard Respect = new(key: false);
    public static readonly ModeRegard Ignore = new(key: true);
}

[SmartEnum<bool>]
public sealed partial class SelectionClear {
    public static readonly SelectionClear All = new(key: false);
    public static readonly SelectionClear Transient = new(key: true);
}

[SmartEnum<bool>]
public sealed partial class FlashMode {
    public static readonly FlashMode Visibility = new(key: false);
    public static readonly FlashMode SelectionColor = new(key: true);
}

[SmartEnum<bool>]
public sealed partial class DeletedPolicy {
    public static readonly DeletedPolicy Keep = new(key: false);
    public static readonly DeletedPolicy Purge = new(key: true);
}

// The seven selection-conduct bits as ONE vocabulary: five ride the host's select members and two its census
// member, and the polarity each row stands for is the row's own key text rather than a `HostValue` mirror bool.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SelectionAxis : ICapability<SelectionAxis> {
    public static readonly SelectionAxis SyncHighlight = new(key: "sync-highlight");
    public static readonly SelectionAxis Persistent = new(key: "persistent");
    public static readonly SelectionAxis IgnoreGrips = new(key: "ignore-grips");
    public static readonly SelectionAxis IgnoreLayerLocks = new(key: "ignore-layer-locks");
    public static readonly SelectionAxis IgnoreLayerVisibility = new(key: "ignore-layer-visibility");
    public static readonly SelectionAxis CensusLights = new(key: "census-lights");
    public static readonly SelectionAxis CensusGrips = new(key: "census-grips");

    // The host's own default posture; accessor-backed because the generated roster fills at static init.
    public static CapabilitySet<SelectionAxis> Baseline => Seed.Value;
    private static readonly Lazy<CapabilitySet<SelectionAxis>> Seed = new(static () =>
        CapabilitySet<SelectionAxis>.Of(SyncHighlight, Persistent, IgnoreGrips, CensusLights, CensusGrips));
}

// The `Amend` payload as a VALUE at this stratum. `TableOp` is S0 and the typed attribute program is S2, so the
// program cannot be named downward — the seam type seats HERE and `Objects/attributes.md`'s `AttributeProgram`
// composes it upward. A bare `Func<ObjectAttributes, Fin<Unit>>` on the case stated no contract at all: nothing
// said the body may only mutate the duplicate it is handed, and nothing refused a null body until the arm ran.
[ComplexValueObject]
[ValidationError]
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
    public static readonly SelectionEdit Add = new(apply: static (table, ids, held) => Toggled(table: table, ids: ids, held: held, select: true));
    public static readonly SelectionEdit Remove = new(apply: static (table, ids, held) => Toggled(table: table, ids: ids, held: held, select: false));
    public static readonly SelectionEdit Replace = new(apply: static (table, ids, held) => table.SetSelectedObjects(
        objectIds: ids,
        syncHighlight: held.Admits(capability: SelectionAxis.SyncHighlight),
        persistentSelect: held.Admits(capability: SelectionAxis.Persistent),
        ignoreGripsState: held.Admits(capability: SelectionAxis.IgnoreGrips),
        ignoreLayerLocking: held.Admits(capability: SelectionAxis.IgnoreLayerLocks),
        ignoreLayerVisibility: held.Admits(capability: SelectionAxis.IgnoreLayerVisibility)));

    [UseDelegateFromConstructor]
    internal partial int Apply(ObjectTable table, IEnumerable<Guid> ids, CapabilitySet<SelectionAxis> held);

    private static int Toggled(ObjectTable table, IEnumerable<Guid> ids, CapabilitySet<SelectionAxis> held, bool select) =>
        table.Select(
            objectIds: ids,
            select: select,
            syncHighlight: held.Admits(capability: SelectionAxis.SyncHighlight),
            persistentSelect: held.Admits(capability: SelectionAxis.Persistent),
            ignoreGripsState: held.Admits(capability: SelectionAxis.IgnoreGrips),
            ignoreLayerLocking: held.Admits(capability: SelectionAxis.IgnoreLayerLocks),
            ignoreLayerVisibility: held.Admits(capability: SelectionAxis.IgnoreLayerVisibility));
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
                    Some: serial => context.Document.ClearUndoRecords(undoSerialNumber: serial, purgeDeletedObjects: roll.Deleted.Key),
                    None: () => context.Document.ClearUndoRecords(purgeDeletedObjects: roll.Deleted.Key));
                return Fin.Succ(value: unit);
            }),
            clearRedoCase: static (context, _) => context.Op.Catch(() => {
                context.Document.ClearRedoRecords();
                return Fin.Succ(value: unit);
            }));
}

// The two derived trait vocabularies: an op row and a program mode each carry their demands as ONE set, so the
// five parallel bool columns the two rosters held delete and a new demand is one vocabulary row.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class OpTrait : ICapability<OpTrait> {
    public static readonly OpTrait RecordsUndo = new(key: "records-undo");
    public static readonly OpTrait Navigates = new(key: "navigates");
    public static readonly OpTrait RequiresContext = new(key: "requires-context");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class UndoTrait : ICapability<UndoTrait> {
    public static readonly UndoTrait Required = new(key: "required");
    public static readonly UndoTrait Records = new(key: "records");
}

[SmartEnum<int>]
internal sealed partial class TableOpTraits {
    internal static readonly TableOpTraits Sourced = new(key: 0, demands: static () =>
        CapabilitySet<OpTrait>.Of(OpTrait.RecordsUndo, OpTrait.RequiresContext));
    internal static readonly TableOpTraits Recorded = new(key: 1, demands: static () =>
        CapabilitySet<OpTrait>.Of(OpTrait.RecordsUndo));
    internal static readonly TableOpTraits Immediate = new(key: 2, demands: static () => CapabilitySet<OpTrait>.Of());
    internal static readonly TableOpTraits Navigation = new(key: 3, demands: static () =>
        CapabilitySet<OpTrait>.Of(OpTrait.Navigates));

    [UseDelegateFromConstructor]
    internal partial CapabilitySet<OpTrait> Demands();
}

[SmartEnum<int>]
internal sealed partial class TransactionUndo {
    internal static readonly TransactionUndo None = new(key: 0, demands: static () => CapabilitySet<UndoTrait>.Of());
    internal static readonly TransactionUndo Record = new(key: 1, demands: static () =>
        CapabilitySet<UndoTrait>.Of(UndoTrait.Required, UndoTrait.Records));
    internal static readonly TransactionUndo Navigate = new(key: 2, demands: static () =>
        CapabilitySet<UndoTrait>.Of(UndoTrait.Required));

    [UseDelegateFromConstructor]
    internal partial CapabilitySet<UndoTrait> Demands();
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TableOp {
    private TableOp() { }

    private sealed record AddCase(Seq<GeometryIntake> Sources, Option<ObjectAttributes> Attributes, Option<HistoryRecord> History, ObjectCustody Custody) : TableOp;
    private sealed record ReplaceCase(TableTarget Target, GeometryIntake Replacement, ModeRegard Modes) : TableOp;
    private sealed record DeleteCase(TableTarget Target, HostInteraction Interaction, ModeRegard Modes) : TableOp;
    private sealed record TransformCase(TableTarget Target, Transform Motion, TransformPolicy Policy) : TableOp;
    private sealed record AmendCase(TableTarget Target, AttributeChange Change, HostInteraction Interaction) : TableOp;
    private sealed record SelectCase(TableTarget Target, SelectionEdit Edit, CapabilitySet<SelectionAxis> Policy) : TableOp;
    private sealed record StateCase(TableTarget Target, ObjectState State, ModeRegard Modes) : TableOp;
    private sealed record ClearSelectionCase(SelectionClear Scope, CapabilitySet<SelectionAxis> Census) : TableOp;
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

    public static Fin<TableOp> Replace(TableTarget target, object replacement, ModeRegard modes, Op? key = null) {
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

    public static Fin<TableOp> Delete(TableTarget target, HostInteraction interaction, ModeRegard modes, Op? key = null) =>
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

    public static Fin<TableOp> Select(TableTarget target, SelectionEdit edit, CapabilitySet<SelectionAxis> policy, Op? key = null) {
        Op op = key.OrDefault();
        return (
                op.Need(target).ToValidation(),
                op.Need(edit).ToValidation())
            .Apply((address, mutation) => (TableOp)new SelectCase(Target: address, Edit: mutation, Policy: policy))
            .As()
            .ToFin();
    }

    public static Fin<TableOp> State(TableTarget target, ObjectState state, ModeRegard modes, Op? key = null) =>
        Admitted(first: target, second: state, third: modes, key: key, mint: static (address, mutation, policy) =>
            new StateCase(Target: address, State: mutation, Modes: policy));

    // The clear carries a census for its RECEIPT alone: the before/after spans read the same two inclusion rows
    // every other selection op reads, so a clear cannot report a different population than the select before it.
    public static Fin<TableOp> ClearSelection(SelectionClear scope, CapabilitySet<SelectionAxis> census, Op? key = null) =>
        key.OrDefault().Need(scope)
            .Map(value => (TableOp)new ClearSelectionCase(Scope: value, Census: census));

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
                // trace of which source produced it. `ResourceId.Admit` is the spine's ONE empty-guid projector,
                // and the two host slots the domain never reads back cross through `Op.ToHostSlot`.
                from ids in edit.Sources.TraverseM(source => source.Admit(domain: model, key: context.Op)
                    .Bind(lease => lease.Use(native => ResourceId.Admit(
                        value: context.Document.Objects.Add(
                            geometry: native,
                            attributes: Op.ToHostSlot(edit.Attributes),
                            history: Op.ToHostSlot(edit.History),
                            reference: edit.Custody.Key),
                        key: context.Op).Map(static id => id.Value)))).As()
                from runtime in Tables.Runtime(document: context.Document, ids: ids, key: context.Op)
                from receipt in TableReceipt.Objects(slot: TableSlot.Created, values: runtime, key: context.Op)
                select receipt,
            replaceCase: static (context, edit) =>
                from model in context.Domain.ToFin(Fail: context.Op.MissingContext())
                from ids in edit.Target.Resolve(document: context.Document, key: context.Op)
                from single in Tables.One(rows: ids, key: context.Op)
                from _ in edit.Replacement.Admit(domain: model, key: context.Op)
                    .Bind(lease => lease.Use(native => context.Op.Confirm(success: context.Document.Objects.Replace(objectId: single, geometry: native, ignoreModes: edit.Modes.Key))))
                from runtime in Tables.Runtime(document: context.Document, ids: Seq(single), key: context.Op)
                from receipt in TableReceipt.Objects(slot: TableSlot.Replaced, values: runtime, key: context.Op)
                select receipt,
            deleteCase: static (context, edit) =>
                from targets in edit.Target.Serials(document: context.Document, key: context.Op)
                from _ in edit.Modes.Key
                    ? targets.TraverseM(target => Optional(context.Document.Objects.FindId(target.Id)).ToFin(Fail: context.Op.InvalidResult())
                        .Bind(native => context.Op.Confirm(success: context.Document.Objects.Delete(obj: native, quiet: edit.Interaction.IsQuiet, ignoreModes: true)))).As().Map(static _ => unit)
                    : context.Op.Confirm(success: context.Document.Objects.Delete(objectIds: targets.Map(static target => target.Id).AsIterable(), quiet: edit.Interaction.IsQuiet) == targets.Count)
                from receipt in TableReceipt.Objects(slot: TableSlot.Deleted, values: targets, key: context.Op)
                select receipt,
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
                    census: edit.Policy,
                    apply: () => edit.Edit.Apply(table: context.Document.Objects, ids: ids.AsIterable(), held: edit.Policy),
                    op: context.Op)
                select receipt,
            stateCase: static (context, edit) =>
                from targets in edit.Target.Serials(document: context.Document, key: context.Op)
                from changed in Tables.ApplyState(document: context.Document, targets: targets, state: edit.State, modes: edit.Modes, key: context.Op)
                from receipt in TableReceipt.Objects(slot: edit.State.Slot, values: changed, key: context.Op)
                select receipt,
            clearSelectionCase: static (context, edit) => SelectionSpan(
                document: context.Document,
                census: edit.Census,
                apply: () => context.Document.Objects.UnselectAll(ignorePersistentSelections: edit.Scope.Key),
                op: context.Op),
            flashCase: static (context, edit) =>
                from targets in edit.Target.Serials(document: context.Document, key: context.Op)
                from objects in targets.TraverseM(target => Optional(context.Document.Objects.FindId(target.Id)).ToFin(Fail: context.Op.InvalidResult())).As()
                from _ in context.Op.Catch(() => {
                    context.Document.Views.FlashObjects(list: objects.AsIterable(), useSelectionColor: edit.Mode.Key);
                    return Fin.Succ(value: unit);
                })
                from receipt in TableReceipt.Objects(slot: TableSlot.Flashed, values: targets, key: context.Op)
                select receipt,
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
                        attributes: Op.ToHostSlot(edit.Attributes),
                        history: Op.ToHostSlot(edit.History),
                        reference: edit.Custody.Key),
                    key: context.Op)
                from runtime in Tables.Runtime(document: context.Document, ids: Seq(id.Value), key: context.Op)
                from receipt in TableReceipt.Objects(slot: TableSlot.Created, values: runtime, key: context.Op)
                select receipt,
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
                from receipt in TableReceipt.Restore(value: edit.Restore, key: context.Op)
                select receipt,
            rollCase: static (context, edit) =>
                from _ in edit.Navigation.Apply(document: context.Document, key: context.Op)
                from receipt in TableReceipt.History(value: edit.Navigation, key: context.Op)
                select receipt);

    private static Fin<TableReceipt> SelectionSpan(RhinoDoc document, CapabilitySet<SelectionAxis> census, Func<int> apply, Op op) =>
        from before in Tables.Selected(document: document, census: census, key: op)
        from _ in guard(apply() >= 0, op.InvalidResult()).ToFin()
        from after in Tables.Selected(document: document, census: census, key: op)
        from receipt in TableReceipt.SelectionDelta(before: before, after: after, key: op)
        select receipt;

    private static Fin<TableReceipt> Lifecycle(RhinoDoc document, TableTarget target, TableSlot slot, Func<ObjectTable, uint, bool> apply, Op op) =>
        from targets in target.Serials(document: document, key: op)
        from changed in targets.TraverseM(value => op.Confirm(success: apply(document.Objects, value.Serial)).Map(_ => value)).As()
        from receipt in TableReceipt.Objects(slot: slot, values: changed, key: op)
        select receipt;

    private static Fin<TableReceipt> Mapped(RhinoDoc document, TableTarget target, TableSlot slot, Func<Guid, Fin<Guid>> step, Op op) =>
        from ids in target.Resolve(document: document, key: op)
        from mapped in ids.TraverseM(step).As()
        from runtime in Tables.Runtime(document: document, ids: mapped, key: op)
        from receipt in TableReceipt.Objects(slot: slot, values: runtime, key: op)
        select receipt;
}

// --- [MODELS] -----------------------------------------------------------------------------
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
                   && admitted.Plan.Operations.ForAll(static operation =>
                       operation.Traits.Demands().Admits(capability: OpTrait.RecordsUndo)
                       && !operation.Traits.Demands().Admits(capability: OpTrait.Navigates)),
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
                   && plan.Operations.ForAll(static operation =>
                       !operation.Traits.Demands().Admits(capability: OpTrait.RecordsUndo)
                       && !operation.Traits.Demands().Admits(capability: OpTrait.Navigates)),
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
    // ONE projecting entry: the identity projection spells `project: Fin.Succ`, so receipt-shaped rails commit
    // unchanged and no arity twin exists beside the railed fold.
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
                   needs: SessionNeed.Mutation(
                       undo: plan.Undo.Demands().Admits(capability: UndoTrait.Required),
                       redraw: plan.Redraw).ToArray())
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

    internal static Fin<Seq<ObjectRuntime>> Selected(RhinoDoc document, CapabilitySet<SelectionAxis> census, Op key) =>
        Optional(document.Objects.GetSelectedObjects(
                includeLights: census.Admits(capability: SelectionAxis.CensusLights),
                includeGrips: census.Admits(capability: SelectionAxis.CensusGrips)))
            .ToFin(Fail: key.InvalidResult())
            .Bind(values => values.AsIterable().ToSeq()
                .Traverse(native => ObjectRuntime.Of(
                    id: native.Id,
                    serial: native.RuntimeSerialNumber,
                    key: key).ToValidation())
                .As()
                .ToFin())
            .Map(static values => ObjectRuntime.Canonical(values: values));

    internal static Fin<Seq<ObjectRuntime>> ApplyState(RhinoDoc document, Seq<ObjectRuntime> targets, ObjectState state, ModeRegard modes, Op key) =>
        targets.TraverseM(target => Optional(document.Objects.FindId(target.Id))
            .ToFin(Fail: key.InvalidResult())
            .Bind(native => state.Done(native: native)
                ? Fin.Succ(value: Option<ObjectRuntime>.None)
                : state.Apply(table: document.Objects, id: target.Id, ignoreLayerMode: modes.Key)
                    ? Fin.Succ(value: Some(target))
                    : Fin.Fail<Option<ObjectRuntime>>(error: key.InvalidResult()))).As().Map(static values => values.Somes());

    private static Fin<TOut> Run<TOut>(RhinoDoc document, TransactionPlan plan, Func<TableReceipt, Fin<TOut>> project, Op op) =>
        from domain in plan.Operations.Exists(static operation => operation.Traits.Demands().Admits(capability: OpTrait.RequiresContext))
            ? Rasm.Domain.Context.Of(doc: document).ToFin().Map(Some)
            : Fin.Succ(Option<Context>.None)
        from projected in DocumentCommit.Sealed(
            document: document,
            name: plan.RecordName.IfNone(nameof(Tables)),
            recordsUndo: plan.Undo.Demands().Admits(capability: UndoTrait.Records),
            redraw: plan.Redraw,
            run: () =>
                from custom in plan.CustomUndo.TraverseM(undo => undo.Register(document: document, key: op)).As()
                from folded in plan.Operations
                    .TraverseM(operation => operation.Apply(document: document, domain: domain, op: op)).As()
                    .Map(static receipts => receipts.Fold(TableReceipt.Empty, static (state, value) => state + value))
                from names in TableReceipt.CustomUndo(names: custom, key: op)
                select folded + names,
            stamp: static (receipt, serial) => receipt.Stamped(
                slot: TableSlot.Recorded,
                record: static value => new TableFact.UndoCase(Serial: value),
                serial: serial),
            project: project,
            op: op)
        select projected;
}
```

## [05]-[RECEIPTS]

- Owner: `TableBodyKind` — the six-row body-kind vocabulary; `TableFact` `[Union]` — the PUBLIC body family carrying object runtime evidence, component-table tallies, named-view restores, history navigation, undo serials, and custom-undo names, answering its kind through one total fold; `TableSlot` `[SmartEnum<int>]` — the slot vocabulary conforming to `Document/facts.md`'s kinded contract with one declared `Bodies` set per row; `TableReceipt` — the `global using` alias onto `FactStream<TableSlot, TableFact>` with this page's mint factories and readers riding one extension block.
- Law: this page CONFORMS to the shared stream and re-mints nothing — the accumulation, the cross-product gate, the undo-stamp projection, and the slot-keyed reader are `facts.md`'s; this page contributes the two vocabularies and its extension block, exactly the two-declaration join the stream's own law promises. NAMED LOSS: the bespoke `TableReceipt` record's interior `Canonical` de-duplication — bought back as the extension's `Runtime` reader, which canonicalizes its projection; witness — `Tables.Run`'s fold over `TableReceipt.Empty` and `+` compiles unchanged on the alias.
- Law: the undo stamp rides `Stamped` on the stream with `TableSlot.Recorded` as its slot and `UndoSerial` as its typed payload — the raw `uint` serial no longer enters a fact, and an unrecorded program contributes no fact rather than one claiming record zero.
- Boundary: the COMMIT ENTRY does not unify. `DraftPlan<TOp>`, `BlockTransaction`, and `TableTransaction` share a four-field carrier — name, program, redraw, undo recording — and nothing else: Draft admits through a `DraftMode` row bundling redraw and recording, Block admits through per-operation trait homogeneity plus a kernel-context census, and Table admits three structurally distinct program shapes with custom-undo handlers and navigation semantics. One carrier under three incompatible admissions is a shape no caller can hold polymorphically, so the merge is refused; what the three genuinely share — the bracket, the redraw scope, and the seal — is already `DocumentCommit.Sealed`, and that IS the unified entry.
- Entry: `Ids(TableSlot, Op?)` and `Runtime(TableSlot, Op?)` fail closed on an invalid slot and project object consequences; `Components`, `Restores`, `HistoryRolls`, `UndoRecords`, and `CustomUndoNames` project the remaining fact cases; a receipt feeds its deleted runtime rows directly into `TableTarget.Deleted`.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
// `TableReceipt` names the shared stream under this page's own identity: two declarations and an extension block
// carry the whole join, per the facts page's conformance law. `global using` is a compilation-unit directive and
// heads the unit — parked past the first declaration it is not an alias, it is a compile error.
global using TableReceipt = Rasm.Rhino.Document.FactStream<Rasm.Rhino.Document.TableSlot, Rasm.Rhino.Document.TableFact>;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TableBodyKind : ICapability<TableBodyKind> {
    public static readonly TableBodyKind Object = new(key: "object");
    public static readonly TableBodyKind Component = new(key: "component");
    public static readonly TableBodyKind Restore = new(key: "restore");
    public static readonly TableBodyKind History = new(key: "history");
    public static readonly TableBodyKind Undo = new(key: "undo");
    public static readonly TableBodyKind Custom = new(key: "custom");
}

// PUBLIC — the stream's alias is public, so its body family is too; the kind answers through one total fold and a
// new case cannot land without a kind row and a slot that admits it.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TableFact : IFactBody<TableBodyKind> {
    private TableFact() { }
    public sealed record ObjectCase(ObjectRuntime Value) : TableFact;
    public sealed record ComponentCase(TableKind Kind, int Tally) : TableFact;
    public sealed record RestoreCase(NamedRestore Value) : TableFact;
    public sealed record HistoryCase(HistoryRoll Value) : TableFact;
    public sealed record UndoCase(UndoSerial Serial) : TableFact;
    public sealed record CustomCase(string Name) : TableFact;

    TableBodyKind IFactBody<TableBodyKind>.Kind => Switch(
        objectCase: static _ => TableBodyKind.Object,
        componentCase: static _ => TableBodyKind.Component,
        restoreCase: static _ => TableBodyKind.Restore,
        historyCase: static _ => TableBodyKind.History,
        undoCase: static _ => TableBodyKind.Undo,
        customCase: static _ => TableBodyKind.Custom);
}

// Conforms to the KINDED slot contract: each row declares the body kinds it emits as one readable set and the
// admission derives — no per-row predicate, no external gate.
[SmartEnum<int>]
public sealed partial class TableSlot : IFactSlot<TableFact, TableBodyKind> {
    public static readonly TableSlot Created = new(key: 0, seated: static () => Objects);
    public static readonly TableSlot Replaced = new(key: 1, seated: static () => Objects);
    public static readonly TableSlot Deleted = new(key: 2, seated: static () => Objects);
    public static readonly TableSlot Moved = new(key: 3, seated: static () => Objects);
    public static readonly TableSlot Selected = new(key: 4, seated: static () => Objects);
    public static readonly TableSlot Unselected = new(key: 5, seated: static () => Objects);
    public static readonly TableSlot Hidden = new(key: 6, seated: static () => Objects);
    public static readonly TableSlot Shown = new(key: 7, seated: static () => Objects);
    public static readonly TableSlot Locked = new(key: 8, seated: static () => Objects);
    public static readonly TableSlot Unlocked = new(key: 9, seated: static () => Objects);
    public static readonly TableSlot Flashed = new(key: 10, seated: static () => Objects);
    public static readonly TableSlot Amended = new(key: 11, seated: static () => Objects);
    public static readonly TableSlot Revived = new(key: 12, seated: static () => Objects);
    public static readonly TableSlot Expunged = new(key: 13, seated: static () => Objects);
    public static readonly TableSlot Reclaimed = new(key: 14, seated: static () => CapabilitySet<TableBodyKind>.Of(TableBodyKind.Component));
    public static readonly TableSlot Restored = new(key: 15, seated: static () => CapabilitySet<TableBodyKind>.Of(TableBodyKind.Restore));
    public static readonly TableSlot Rolled = new(key: 16, seated: static () => CapabilitySet<TableBodyKind>.Of(TableBodyKind.History));
    public static readonly TableSlot Recorded = new(key: 17, seated: static () => CapabilitySet<TableBodyKind>.Of(TableBodyKind.Undo));
    public static readonly TableSlot CustomUndo = new(key: 18, seated: static () => CapabilitySet<TableBodyKind>.Of(TableBodyKind.Custom));

    [UseDelegateFromConstructor]
    private partial CapabilitySet<TableBodyKind> Seated();

    public CapabilitySet<TableBodyKind> Bodies => Seated();

    private static CapabilitySet<TableBodyKind> Objects => CapabilitySet<TableBodyKind>.Of(TableBodyKind.Object);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
// This page's own mint factories and readers over the closed instantiation — the two-declaration join the stream
// law promises, with `Runtime` carrying the canonical de-duplication the bespoke receipt once held.
public static class TableFacts {
    extension(TableReceipt receipt) {
        public static Fin<TableReceipt> Objects(TableSlot slot, Seq<ObjectRuntime> values, Op key) =>
            TableReceipt.All(
                slot: slot,
                bodies: ObjectRuntime.Canonical(values: values).Map(static value => (TableFact)new TableFact.ObjectCase(Value: value)),
                key: key);

        public static Fin<TableReceipt> Component(TableKind kind, int tally, Op key) =>
            from admitted in key.Need(kind)
            from _ in guard(tally >= 0, key.InvalidResult()).ToFin()
            from minted in TableReceipt.Of(slot: TableSlot.Reclaimed, body: new TableFact.ComponentCase(Kind: admitted, Tally: tally), key: key)
            select minted;

        public static Fin<TableReceipt> Restore(NamedRestore value, Op key) =>
            TableReceipt.Of(slot: TableSlot.Restored, body: new TableFact.RestoreCase(Value: value), key: key);

        public static Fin<TableReceipt> History(HistoryRoll value, Op key) =>
            TableReceipt.Of(slot: TableSlot.Rolled, body: new TableFact.HistoryCase(Value: value), key: key);

        public static Fin<TableReceipt> CustomUndo(Seq<string> names, Op key) =>
            TableReceipt.All(
                slot: TableSlot.CustomUndo,
                bodies: names.Map(static name => (TableFact)new TableFact.CustomCase(Name: name)),
                key: key);

        public static Fin<TableReceipt> SelectionDelta(Seq<ObjectRuntime> before, Seq<ObjectRuntime> after, Op key) =>
            from selected in TableReceipt.Objects(
                slot: TableSlot.Selected,
                values: after.Filter(value => !before.Exists(item => item.Equals(value))),
                key: key)
            from unselected in TableReceipt.Objects(
                slot: TableSlot.Unselected,
                values: before.Filter(value => !after.Exists(item => item.Equals(value))),
                key: key)
            select selected + unselected;

        public Fin<Seq<Guid>> Ids(TableSlot slot, Op? key = null) =>
            receipt.Runtime(slot: slot, key: key).Map(static values => values.Map(static value => value.Id));

        public Fin<Seq<ObjectRuntime>> Runtime(TableSlot slot, Op? key = null) =>
            Optional(slot).ToFin(Fail: key.OrDefault().InvalidInput()).Map(admitted =>
                ObjectRuntime.Canonical(values: receipt.Project(
                    slot: admitted,
                    select: static body => body is TableFact.ObjectCase row ? Some(row.Value) : Option<ObjectRuntime>.None)));

        public Seq<(TableKind Kind, int Tally)> Components => receipt.Project(
            slot: TableSlot.Reclaimed,
            select: static body => body is TableFact.ComponentCase row ? Some((row.Kind, row.Tally)) : Option<(TableKind, int)>.None);

        public Seq<NamedRestore> Restores => receipt.Project(
            slot: TableSlot.Restored,
            select: static body => body is TableFact.RestoreCase row ? Some(row.Value) : Option<NamedRestore>.None);

        public Seq<HistoryRoll> HistoryRolls => receipt.Project(
            slot: TableSlot.Rolled,
            select: static body => body is TableFact.HistoryCase row ? Some(row.Value) : Option<HistoryRoll>.None);

        public Seq<UndoSerial> UndoRecords => receipt.Project(
            slot: TableSlot.Recorded,
            select: static body => body is TableFact.UndoCase row ? Some(row.Serial) : Option<UndoSerial>.None);

        public Seq<string> CustomUndoNames => receipt.Project(
            slot: TableSlot.CustomUndo,
            select: static body => body is TableFact.CustomCase row ? Some(row.Name) : Option<string>.None);

        public Fin<int> Count(TableSlot slot, Op? key = null) =>
            receipt.Runtime(slot: slot, key: key).Map(static values => values.Count);
    }
}
```

## [06]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]              | [OWNER]                        | [FORM]                       | [ENTRY]                               |
| :-----: | :--------------------- | :----------------------------- | :--------------------------- | :------------------------------------ |
|  [01]   | document tables        | `TableKind`                    | keyed behavior rows          | `ForComponentType` / `Reclaim`        |
|  [02]   | object addressing      | `TableTarget`                  | ids/runtime/query union      | `Of` / `Deleted` / `Query`            |
|  [03]   | query predicates       | `TablePredicate`               | frozen predicate union       | `Tag` / `Color` / `Bounds`            |
|  [04]   | query axes + product   | `QueryAxis` / `QuerySpec`      | seated rows + admitted value | `Of` / `Build(document, key)`         |
|  [05]   | mutation program       | `TableOp`                      | admitted total union         | operation factories / `Apply`         |
|  [06]   | attribute payload      | `AttributeChange`              | admitted mutation body       | `TableOp.Amend`                       |
|  [07]   | commit scope           | `TableTransaction`             | program-mode union           | `Recorded` / `Immediate` / `Navigate` |
|  [08]   | resource ingress       | `GeometryIntake`               | native/value custody union   | `Admit`                               |
|  [09]   | table commit spine     | `Tables`                       | session/commit fold          | `Commit`                              |
|  [10]   | consequence evidence   | `TableReceipt` alias           | conformed fact stream        | extension mints / typed projections   |
|  [11]   | component addressing   | `ResourceRef` / `ResourceLens` | id/name/index over a lens    | `Of` / `Resolve(document, lens, key)` |
|  [12]   | viewport addressing    | `ViewportTarget`               | address & census union       | `Active` / `ResolveViewport`          |
|  [13]   | object-type vocabulary | `ObjectKind` / `ObjectKinds`   | keyed rows over a set        | `Of` / `Any` / `Mask` / `OfMask`      |
|  [14]   | space partition        | `ActiveSpaceUse`               | host-keyed rows              | `Get` / `Key`                         |
|  [15]   | selection conduct      | `SelectionAxis`                | capability vocabulary        | `Baseline` / `Admits`                 |

- Packages: `RhinoCommon` (`Rasm.Rhino/.api/api-rhinocommon-document.md` + `api-rhinocommon-document-state.md` — the component-table surface: layers, groups, views, instance definitions, named states); `Thinktecture.Runtime.Extensions` (`libs/dotnet/.api/api-thinktecture-runtime-extensions.md` — `[SmartEnum]` query/selection/trait rows, `[Union]` `TableFact`, `[ValueObject]` `ResourceName` with `[KeyMemberEqualityComparer]`); `LanguageExt.Core` (`libs/dotnet/.api/api-languageext.md` — `Seq`/`HashMap` table projections); kernel `Domain/rails` + `Domain/validation` (`Op.Row` host-enum admission).

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
