# [RASM_RHINO_TABLES]

`Rasm.Rhino.Document` owns document-table vocabulary, object addressing, and mutation programs. `TableKind` captures each admitted host document table, `TableTarget` freezes explicit and host-query addressing, and `TableOp` closes the mutation family. `Tables.Commit` executes one admitted program inside one session capability window, refreshes the kernel `Context`, and frames the change through `DocumentCommit.Sealed`.

## [01]-[INDEX]

- [02]-[TABLE_VOCABULARY]: `TableKind` — document-table identity, component correspondence, and reclamation behavior.
- [03]-[TARGET_ALGEBRA]: `ObjectKind`/`ObjectKinds`, `ActiveSpaceUse`, `QueryAxis`, `QuerySpec`, `BoundsMatch`, `TablePredicate`, `TableTarget`, `ViewportTarget`, and the `DefinedView`/`IsoQuadrant` host projection rosters — object-type vocabulary, immutable object addressing, viewport addressing, projection vocabulary, and query composition.
- [04]-[RUN]: policy rows, `SelectionAxis`, `NamedRestore`, `HistoryRoll`, `TableOp`, `TableTransaction`, `GeometryIntake`, and `Tables` — the mutation program and its one commit entry.
- [05]-[SURFACE_LEDGER]: the page owner map.

## [02]-[TABLE_VOCABULARY]

- Owner: `TableKind` `[SmartEnum<int>]` binds each admitted document table to its `ModelComponentType` and table-owned reclamation delegate.
- Entry: `ForComponentType(ModelComponentType) : Fin<Seq<TableKind>>` returns every mapped row, expands `ModelComponentType.Mixed` across every explicit correspondence, treats `ModelComponentType.Unset` as absent correspondence, and rejects an undefined foreign ordinal. `Reclaim(RhinoDoc) : Fin<int>` invokes the row delegate and rejects a table with no host reclamation member.
- Law: table behavior resides on the row. A table extension declares component correspondence and reclamation behavior at construction, so no external dictionary or accessibility flag can drift from the vocabulary.
- Law: `ModelComponentType.Unset` is the ONE row-side sentinel for absent correspondence, so the expansion arm reads as "every row that has one" and a lookup never manufactures a row it cannot also expand; `Mixed` is a QUERY argument alone and never a row value, because a row carrying it would be excluded by name from its own expansion and unreachable by lookup — an inert correspondence column no input returns.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
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

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class TableKind {
    public static readonly TableKind Objects = new(key: 0, componentType: ModelComponentType.ModelGeometry, reclaim: NoReclaim);
    public static readonly TableKind Manifest = new(key: 1, componentType: ModelComponentType.Unset, reclaim: NoReclaim);
    public static readonly TableKind Bitmaps = new(key: 2, componentType: ModelComponentType.Image, reclaim: NoReclaim);
    public static readonly TableKind Materials = new(key: 3, componentType: ModelComponentType.Material, reclaim: NoReclaim);
    public static readonly TableKind Linetypes = new(key: 4, componentType: ModelComponentType.LinePattern, reclaim: static (document, op) => Count(document.Linetypes.PurgeUnused()));
    public static readonly TableKind Layers = new(key: 5, componentType: ModelComponentType.Layer, reclaim: static (document, op) => Count(document.Layers.PurgeUnused()));
    public static readonly TableKind Groups = new(key: 6, componentType: ModelComponentType.Group, reclaim: static (document, op) => Count(document.Groups.PurgeUnused()));
    public static readonly TableKind DimStyles = new(key: 7, componentType: ModelComponentType.DimStyle, reclaim: static (document, op) => Count(document.DimStyles.PurgeUnused()));
    public static readonly TableKind Lights = new(key: 8, componentType: ModelComponentType.RenderLight, reclaim: NoReclaim);
    public static readonly TableKind HatchPatterns = new(key: 9, componentType: ModelComponentType.HatchPattern, reclaim: static (document, op) => Count(document.HatchPatterns.PurgeUnused()));
    public static readonly TableKind Views = new(key: 10, componentType: ModelComponentType.Unset, reclaim: NoReclaim);
    public static readonly TableKind NamedViews = new(key: 11, componentType: ModelComponentType.Unset, reclaim: NoReclaim);
    public static readonly TableKind InstanceDefinitions = new(key: 12, componentType: ModelComponentType.InstanceDefinition, reclaim: static (document, op) => Count(document.InstanceDefinitions.PurgeUnused()));
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
    internal partial Fin<int> Reclaim(RhinoDoc document);

    public static Fin<Seq<TableKind>> ForComponentType(ModelComponentType type) =>
        Enum.IsDefined(value: type)
            ? Fin.Succ(value: type switch {
                ModelComponentType.Unset => Seq<TableKind>(),
                ModelComponentType.Mixed => Items.AsIterable()
                    .Filter(static kind => kind.ComponentType is not ModelComponentType.Unset)
                    .ToSeq(),
                _ => Items.AsIterable().Filter(kind => kind.ComponentType == type).ToSeq(),
            })
            : Fin.Fail<Seq<TableKind>>(error: key.OrDefault().InvalidInput());

    private static Fin<int> Count(int value) =>
        guard(value >= 0, new KernelFault.InvalidResult()).ToFin().Map(_ => value);

    private static Fin<int> NoReclaim(RhinoDoc document) =>
        Fin.Fail<int>(error: new KernelFault.Unsupported(InputType: typeof(TableKind), OutputType: typeof(int)));
}
```

## [03]-[TARGET_ALGEBRA]

- Owner: `ObjectKind` `[SmartEnum<ObjectType>]` is the corpus-wide OBJECT-TYPE vocabulary and `ObjectKinds` its admitted set, seated here because the concept has consumers at S1 (`Commands` modal object asks) and S2 (`HostUi` properties-page scope) and this spine is the lowest stratum both reach; `Mask` is the one OR-fold, `Any` the host's own catch-all row, and no folder mints a second type table.
- Law: a raw `ObjectType` never crosses a public signature — every filter is `ObjectKinds`, every host member taking the flag reads `Mask` at its own call, and a page needing "every type" composes `ObjectKinds.Any` rather than spelling `ObjectType.AnyObject`. The same law binds `MeshType`: the raw host mesh discriminant crosses only as `Objects/materials.md`'s `MeshKind` row.
- Owner: `ActiveSpaceUse` `[SmartEnum<ActiveSpace>]` is the space partition, seated here beside the enumerator's own `SpaceFilter` because an attribute set carries it at S2 and a conduit criterion and a gumball seat read it at S4; the roster mirrors the host enum completely, so `Get` is total over any value a host read returns and the row's `Key` is the one write.
- Owner: `QueryAxis` is the inclusion vocabulary whose every row carries its own host setter; `QuerySpec` `[ComplexValueObject]` holds those axes as ONE `CapabilitySet<QueryAxis>` beside the filter columns and builds the host settings inside the document callback, so the mutable host settings type never crosses a signature, every host sentinel is an `Option` whose absence reads as every row, and the live `ViewportFilter` slot is the stable `ViewportTarget` owner. `TableTarget` `[Union]` closes nonempty explicit ids and admitted queries. `TablePredicate` `[Union]` adds composable tag, draw-color, and kernel-bounds predicates; `BoundsMatch` owns containment versus intersection behavior as rows, each comparison banded on the kernel `Duplicate` tolerance lane.
- Law: the query axes are a SET over a vocabulary whose rows OWN their host writes — `Build` is one fold over `QueryAxis.Items`, so the fourteen parallel bool columns, their fourteen `Of` options, and the fourteen hand assignments delete together and a fifteenth host axis is one row no consumer edits. The three admission constraints the product carries — fast selection demands the selected-only filter (the host silently drops it otherwise), at least one state axis, at least one category axis — state ONCE at construction over the set, so a spec cannot carry a knob the host will silently drop. The kernel `CapabilityLaw` carries corner SETS alone, so these pairwise and quantified clauses ride the owner's own admission rather than a law value.
- Owner: `ViewportTarget` `[Union]` is the corpus-wide VIEWPORT address — active, named, id, page, detail, and census cases closed as one owner beside `TableKind` (which table), `TableTarget` (which objects), and `ResourceRef` (which component). `ViewportScope` `[SmartEnum<int>]` carries the model, page, and detail census generators and `EveryCase` freezes their set; `ViewportRef` is the ephemeral resolved row pairing `RhinoView`, `RhinoViewport`, and an optional `DetailViewObject`. `Active`/`Named`/`Id`/`Page`/`Detail`/`Every` construct, and `Resolve`, `ResolveOne`, and `ResolveViewport` fold one address to every row, exactly one row, or one native viewport inside the caller's document callback.
- Law: viewport resolution names `RhinoDoc.Views.ActiveView`, `.Find`, `.GetViewList`, `.GetPageViews`, `RhinoPageView.GetDetailViews`, and `DetailViewObject.Viewport` exactly once; a detail address matches either `DetailViewObject.Id` or `DetailViewObject.Viewport.Id`, and a resolution yielding no row refuses before any consumer projects it.
- Law: an addressed row binds `RhinoView.MainViewport` — the viewport the address names — because `RhinoPageView.ActiveViewport` silently returns an active detail; only `ActiveCase` binds `ActiveViewport`, adopting the host's active semantics, and a detail row binds `DetailViewObject.Viewport` and carries its `DetailViewObject` so a detail commit or scale conversion reads the owning object without a second lookup.
- Law: viewport rows resolve live per call inside the document callback and leave as detached addresses or one native viewport, never a retained handle; `ResolveViewport` composes `Resolve` and `Tables.One`, so the single-viewport consumers — `QuerySpec` viewport filtering, `NamedRestore`, and the annotation dimension-scale probe — share one fold and no call site re-spells the resolve-then-one triple.
- Owner: `ResourceRef` is the corpus-wide COMPONENT address — id, name, index closed as one `[Union]` over a per-table `ResourceLens<TComponent>` — completing the addressing triad beside `TableKind` (which table) and `TableTarget` (which objects); `ResourceId`, `ResourceName`, and `ResourceIndex` admit the native address scalars once, and the `ResourceId` and `ResourceIndex` `Maybe`/`Admit` pairs are the sole `Guid.Empty` and negative-index sentinel projectors — `Maybe` where the host miss value spells a normal absence, `Admit` where it is a genuine refusal.
- Law: each component table contributes exactly one lens — Annotation's style, linetype, hatch, and section pipelines and Blocks' definition pipeline each declare one `ResourceLens<T>` row — and no folder mints a second address family; resolution reads live per call inside the owning operation, because tables mutate under commands, so no resolved component is cached on a value.
- Entry: `QuerySpec.Of(...)`, `TableTarget.Of(params ReadOnlySpan<Guid>)`, and `Query(QuerySpec, params ReadOnlySpan<TablePredicate>)` are the only constructors. `Resolve` returns distinct ids; `Serials` reads native runtime serials for lifecycle operations. Deleted-object lifecycle composes a query whose admitted axes select deleted objects, so the host query remains the source of the serial required by `Undelete` and `Purge`.
- Law: query settings are BUILT at execution from an admitted value, never copied from a caller's instance — the host settings object exists only inside `QuerySpec.Build`, so no caller retains a handle that can mutate an admitted target, and the viewport resolves from stable identity inside the document callback. Predicate evaluation accumulates independent object and predicate faults through `Validation<Error, T>` before lowering once to `Fin<T>`.
- Law: a predicate distinguishes NON-MATCH from HOST FAULT — a missing tag is a non-match, a missing attribute set is a refusal, because folding both onto `false` silently drops an unreadable object out of every filtered query. Draw-colour comparison lands on the quantized ARGB quadruple of two `PerceptualColor` values: `System.Drawing.Color` equality compares NAME before value, so a named row and its identical literal compare unequal, which is the trap a colour filter walks into on the first system colour.
- Law: bounds predicates admit `BoundingBox.IsValid` before corner accumulation and compose the kernel `BoundsOf` owner; the containment and intersection comparisons read the kernel `Duplicate` tolerance lane admitted ONCE at the factory from the caller's `Context`, so an exact float equality never decides a near-coincident box and no site mints an epsilon. Inflation remains host-query policy, while candidate classification and coercion stay kernel-owned.
- Boundary: `BoundingBox.Inflate` mutates a copied struct, so `Inflated` is the one statement kernel and never mutates request evidence.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
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
    public static readonly ObjectKind AnyObject = new(key: ObjectType.AnyObject);
}

[ComplexValueObject]
[ValidationError]
public sealed partial class ObjectKinds {
    public FrozenSet<ObjectKind> Values { get; }

    internal ObjectType Mask => toSeq(Values).Fold(ObjectType.None, static (mask, kind) => mask | kind.Key);

    public static ObjectKinds Any { get; } = Create(values: FrozenSet.ToFrozenSet([ObjectKind.AnyObject]));

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref FrozenSet<ObjectKind> values) =>
        validationError = values is null || values.Count is 0 || values.Any(static kind => kind is null)
            ? new ValidationError(message: "Object kind set is empty.")
            : null;

    public static Fin<ObjectKinds> OfMask(ObjectType mask) {
        Seq<ObjectKind> rows = mask == ObjectType.AnyObject
            ? Seq(ObjectKind.AnyObject)
            : Items.AsIterable()
                .Filter(row => row.Key != ObjectType.AnyObject && (mask & row.Key) == row.Key && row.Key != ObjectType.None)
                .ToSeq();
        return FactoryBridge.Accept<ObjectKinds>(
            fault: Validate(rows.ToFrozenSet(), out ObjectKinds? admitted),
            admitted: admitted);
    }

    public static Fin<ObjectKinds> Of(params ReadOnlySpan<ObjectKind> values) {
        return FactoryBridge.Accept<ObjectKinds>(
            fault: Validate(toSeq(values.ToArray()).ToFrozenSet(), out ObjectKinds? admitted),
            admitted: admitted);
    }
}

[SmartEnum<ActiveSpace>]
public sealed partial class ActiveSpaceUse {
    public static readonly ActiveSpaceUse None = new(key: ActiveSpace.None);
    public static readonly ActiveSpaceUse Model = new(key: ActiveSpace.ModelSpace);
    public static readonly ActiveSpaceUse Page = new(key: ActiveSpace.PageSpace);
    public static readonly ActiveSpaceUse UvEditor = new(key: ActiveSpace.UVEditorSpace);
    public static readonly ActiveSpaceUse BlockEditor = new(key: ActiveSpace.BlockEditorSpace);
}

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

    public static CapabilitySet<QueryAxis> Baseline => Seed.Value;
    private static readonly Lazy<CapabilitySet<QueryAxis>> Seed =
        new(static () => CapabilitySet<QueryAxis>.Of(Normal, Locked, Active));
}

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
        Option<ViewportTarget> viewport = default) =>
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

    internal Fin<ObjectEnumeratorSettings> Build(RhinoDoc document) =>
        Viewport
            .Traverse(target => target.ResolveViewport(document: document))
            .As()
            .Map(resolved => {
                ObjectEnumeratorSettings settings = toSeq(QueryAxis.Items).Fold(
                    new ObjectEnumeratorSettings(),
                    (held, axis) => axis.Seat(settings: held, held: Axes.Admits(capability: axis)));
                settings.ObjectTypeFilter = Kinds.Mask;
                settings.ClassTypeFilter = HostEdge.Slot(Shape);
                settings.LayerIndexFilter = Layer.Map(static value => value.Value).IfNone(noneValue: AnyIndex);
                settings.MaterialIndexFilter = Material.Map(static value => value.Value).IfNone(noneValue: AnyMaterial);
                settings.NameFilter = Name.IfNone(AnyName);
                settings.SpaceFilter = Space.Key;
                settings.ViewportFilter = HostEdge.Slot(resolved);
                return settings;
            });

    private const int AnyIndex = -1;
    private const int AnyMaterial = int.MinValue + 1;
    private const string AnyName = "*";
}

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

    public static Fin<TablePredicate> Tag(string name, Option<string> expected = default) =>
        key.OrDefault().AcceptText(value: name).Map(valid => (TablePredicate)new TagCase(Key: valid, Expected: expected));

    public static Fin<TablePredicate> Color(PerceptualColor value) =>
        key.OrDefault().Need(value).Map(static admitted => (TablePredicate)new ColorCase(Value: admitted));

    public static Fin<TablePredicate> Bounds(BoundingBox region, BoundsMatch match, Context context, double inflation = 0.0) {
        return from _ in guard(region.IsValid, new KernelFault.InvalidInput()).ToFin()
               from predicate in (
                Admit.Need(match).ToValidation(),
                Admit.Need(context).ToValidation(),
                Try.lift(() => Fin.Succ(value: toSeq(region.GetCorners()))).Run().Bind(static inner => inner)
                    .Bind(corners =>
                        from counted in guard(corners.Count is 8, new KernelFault.InvalidInput()).ToFin()
                        from admitted in corners
                            .Traverse(point => Acceptance.Input(value: point).ToValidation())
                            .As()
                            .ToFin()
                        select admitted)
                    .ToValidation(),
                guard(double.IsFinite(inflation) && inflation >= 0.0, new KernelFault.InvalidInput())
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

    internal Fin<bool> Match(RhinoDoc document, RhinoObject native) =>
        Switch(
            state: (Document: document, Native: native),
            tagCase: static (context, predicate) => Optional(context.Native.Attributes)
                .ToFin(Fail: new KernelFault.InvalidResult())
                .Map(attributes => Optional(attributes.GetUserString())
                    .Map(stored => predicate.Expected
                        .Map(expected => string.Equals(a: stored, b: expected, comparisonType: StringComparison.Ordinal))
                        .IfNone(noneValue: true))
                    .IfNone(noneValue: false)),
            colorCase: static (context, predicate) => Optional(context.Native.Attributes)
                .ToFin(Fail: new KernelFault.InvalidResult())
                .Bind(attributes => Shade(color: attributes.DrawColor(document: context.Document)))
                .Map(drawn => drawn.ToRgb() == predicate.Value.ToRgb()),
            boundsCase: static (context, predicate) => Optional(context.Native.Geometry)
                .ToFin(Fail: new KernelFault.InvalidResult())
                .Bind(geometry => geometry.BoundsOf())
                .Map(candidate => predicate.Match.Test(region: predicate.Region, candidate: candidate, band: predicate.Band)));

    private static BoundingBox Inflated(BoundingBox region, double amount) {
        BoundingBox expanded = region;
        _ = HostEdge.SideWhen(amount > 0.0, () => expanded.Inflate(xAmount: amount, yAmount: amount, zAmount: amount));
        return expanded;
    }

    internal static Fin<PerceptualColor> Shade(System.Drawing.Color color) =>
        PerceptualColor.OfRgb(color.R, color.G, color.B, alpha: color.A);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TableTarget {
    private TableTarget() { }

    private sealed record IdsCase(Seq<Guid> Values) : TableTarget;
    private sealed record QueryCase(QuerySpec Spec, Seq<TablePredicate> Predicates) : TableTarget;

    public static Fin<TableTarget> Of(params ReadOnlySpan<Guid> ids) {
        return from values in toSeq(ids.ToArray())
                   .Traverse(id => (id != Guid.Empty
                       ? Fin.Succ(value: id)
                       : Fin.Fail<Guid>(error: new KernelFault.InvalidInput())).ToValidation())
                   .As()
                   .ToFin()
               let distinct = values.Distinct()
               from _ in guard(!distinct.IsEmpty, new KernelFault.InvalidInput())
               select (TableTarget)new IdsCase(Values: distinct);
    }

    public static Fin<TableTarget> Query(QuerySpec spec, params ReadOnlySpan<TablePredicate> predicates) {
        return (
                Admit.Need(spec).ToValidation(),
                Admission.All(values: predicates).ToValidation())
            .Apply(static (source, filters) => (TableTarget)new QueryCase(
                Spec: source,
                Predicates: filters))
            .As()
            .ToFin();
    }

    internal bool SelectsDeleted => Switch(
        idsCase: static _ => false,
        queryCase: static target => target.Spec.SelectsOnlyDeleted);

    internal Fin<Seq<Guid>> Resolve(RhinoDoc document) =>
        Switch(
            state: document,
            idsCase: static (_, target) => Fin.Succ(value: target.Values),
            queryCase: static (context, target) => Evaluate(
                    target: target,
                    document: context)
                .Map(static rows => rows.Map(static native => native.Id).Distinct()));

    internal Fin<Seq<uint>> Serials(RhinoDoc document) =>
        Switch(
            state: document,
            idsCase: static (context, target) => target.Values
                .Traverse(id => Optional(context.Objects.FindId(id))
                    .ToFin(Fail: new KernelFault.InvalidResult())
                    .Bind(native => guard(native.RuntimeSerialNumber > 0u, new KernelFault.InvalidResult()).ToFin()
                        .Map(_ => native.RuntimeSerialNumber))
                    .ToValidation())
                .As()
                .ToFin(),
            queryCase: static (context, target) => Evaluate(
                    target: target,
                    document: context)
                .Bind(rows => rows
                    .Traverse(native => guard(native.RuntimeSerialNumber > 0u, new KernelFault.InvalidResult()).ToFin()
                        .Map(_ => native.RuntimeSerialNumber)
                        .ToValidation())
                    .As()
                    .ToFin()));

    private static Fin<Seq<RhinoObject>> Evaluate(QueryCase target, RhinoDoc document) =>
        from settings in target.Spec.Build(document: document)
        from objects in Optional(document.Objects.GetObjectList(settings: settings))
            .ToFin(Fail: new KernelFault.InvalidResult())
            .Map(static values => toSeq(values))
        from matches in objects
            .Traverse(native => target.Predicates
                .Traverse(predicate => predicate.Match(document: document, native: native).ToValidation())
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

    public static Fin<ViewportTarget> Every(ReadOnlySpan<ViewportScope> scopes) {
        Seq<ViewportScope> rows = toSeq(scopes.ToArray()).Strict();
        return guard(!rows.IsEmpty && rows.ForAll(static scope => scope is not null), key.OrDefault().InvalidInput()).ToFin()
            .Map(_ => (ViewportTarget)new EveryCase(Scopes: rows.ToFrozenSet()));
    }
    public static Fin<ViewportTarget> Named(string name) =>
        key.OrDefault().AcceptText(value: name).Map(static valid => (ViewportTarget)new NamedCase(Name: valid));
    public static Fin<ViewportTarget> Id(Guid viewportId) =>
        guard(viewportId != Guid.Empty, key.OrDefault().InvalidInput()).ToFin().Map(_ => (ViewportTarget)new IdCase(ViewportId: viewportId));
    public static Fin<ViewportTarget> Page(Guid pageViewId) =>
        guard(pageViewId != Guid.Empty, key.OrDefault().InvalidInput()).ToFin().Map(_ => (ViewportTarget)new PageCase(PageViewId: pageViewId));
    public static Fin<ViewportTarget> Detail(Guid pageViewId, Guid detailId) =>
        guard(pageViewId != Guid.Empty && detailId != Guid.Empty, key.OrDefault().InvalidInput()).ToFin()
            .Map(_ => (ViewportTarget)new DetailCase(PageViewId: pageViewId, DetailId: detailId));

    internal Fin<Seq<ViewportRef>> Resolve(RhinoDoc document) =>
        Switch(
            document,
            activeCase: static (ctx, _) =>
                Optional(ctx.Views.ActiveView).ToFin(Fail: new KernelFault.MissingContext())
                    .Map(view => Seq(ViewportRef.OfActive(view: view))),
            namedCase: static (ctx, target) =>
                Optional(ctx.Views.Find(mainViewportName: target.Name, compareCase: false))
                    .ToFin(Fail: new KernelFault.InvalidInput())
                    .Map(view => Seq(ViewportRef.Of(view: view))),
            idCase: static (ctx, target) => (
                    Optional(ctx.Views.Find(mainViewportId: target.ViewportId))
                        .Map(static view => ViewportRef.Of(view: view))
                    | toSeq(ctx.Views.GetPageViews())
                        .Bind(static page => toSeq(page.GetDetailViews())
                            .Map(detail => ViewportRef.OfDetail(view: page, detail: detail)))
                        .Find(row => row.Viewport.Id == target.ViewportId
                            || row.Detail.Exists(detail => detail.Id == target.ViewportId))
                ).ToFin(Fail: new KernelFault.InvalidInput()).Map(static row => Seq(row)),
            pageCase: static (ctx, target) =>
                PageOf(document: ctx, pageViewId: target.PageViewId)
                    .Map(page => Seq(ViewportRef.Of(view: page))),
            detailCase: static (ctx, target) =>
                from page in PageOf(document: ctx, pageViewId: target.PageViewId)
                from detail in toSeq(page.GetDetailViews())
                    .Find(row => row.Id == target.DetailId || row.Viewport.Id == target.DetailId)
                    .ToFin(Fail: new KernelFault.InvalidInput())
                select Seq(ViewportRef.OfDetail(view: page, detail: detail)),
            everyCase: static (ctx, target) => Fin.Succ(
                toSeq(target.Scopes)
                    .OrderBy(static scope => scope.Key)
                    .Bind(scope => scope.Select(document: ctx))
                    .Strict()));

    internal Fin<ViewportRef> ResolveOne(RhinoDoc document) =>
        Resolve(document: document).Bind(rows => Tables.One(rows: rows));

    internal Fin<RhinoViewport> ResolveViewport(RhinoDoc document) =>
        ResolveOne(document: document).Map(static row => row.Viewport);

    private static Fin<RhinoPageView> PageOf(RhinoDoc document, Guid pageViewId) =>
        toSeq(document.Views.GetPageViews()).Find(page => page.MainViewport.Id == pageViewId).ToFin(Fail: new KernelFault.InvalidInput());
}

internal readonly record struct ViewportRef(RhinoView View, RhinoViewport Viewport, Option<DetailViewObject> Detail) {
    internal static ViewportRef Of(RhinoView view) =>
        new(View: view, Viewport: view.MainViewport, Detail: Option<DetailViewObject>.None);
    internal static ViewportRef OfActive(RhinoView view) =>
        new(View: view, Viewport: view.ActiveViewport, Detail: Option<DetailViewObject>.None);
    internal static ViewportRef OfDetail(RhinoPageView view, DetailViewObject detail) =>
        new(View: view, Viewport: detail.Viewport, Detail: Some(detail));

    internal Fin<TOut> Info<TOut>(Func<ViewportInfo, Fin<TOut>> project) =>
        Try.lift(() => new Lease<ViewportInfo>.Owned(Value: new ViewportInfo(Viewport)).Use(project)).Run().Bind(static inner => inner);
}

// --- [COMPONENT_ADDRESS]
[ValueObject<Guid>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[ValidationError]
public readonly partial struct ResourceId {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Guid value) =>
        validationError = value != Guid.Empty ? null : new ValidationError(message: "ResourceId requires a non-empty value.");

    internal static Option<ResourceId> Maybe(Guid value) => Optional(value).Filter(static id => id != Guid.Empty).Map(Create);

    internal static Fin<ResourceId> Admit(Guid value) => Maybe(value).ToFin(Fail: new KernelFault.InvalidResult());
}

[ValueObject<int>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[ValidationError]
public readonly partial struct ResourceIndex {
    internal const int Absent = -1;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
        validationError = value >= 0 ? null : new ValidationError(message: "ResourceIndex requires a non-negative value.");

    internal static Option<ResourceIndex> Maybe(int value) =>
        value >= 0 ? Some(Create(value)) : Option<ResourceIndex>.None;

    internal static Fin<ResourceIndex> Admit(int value) => Maybe(value).ToFin(Fail: new KernelFault.InvalidResult());
}

[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[ValidationError]
public sealed partial class ResourceName {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim() ?? string.Empty;
        validationError = value.Length > 0 ? null : new ValidationError(string.Join(" | ", new object?[] { nameof(ResourceName) }));
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

    public static Fin<ResourceRef> Of(Guid id) =>
        ResourceId.Maybe(id).Map(static value => (ResourceRef)new ById(value: value))
            .ToFin(Fail: key.OrDefault(name: nameof(ResourceRef)).InvalidInput());

    public static Fin<ResourceRef> Of(string name) =>
        key.OrDefault(name: nameof(ResourceRef)).AcceptText(value: name)
            .Map(static valid => (ResourceRef)new ByName(value: ResourceName.Create(valid)));

    public static Fin<ResourceRef> Of(int index) =>
        ResourceIndex.Admit(value: index, key: key.OrDefault(name: nameof(ResourceRef)))
            .Map(static value => (ResourceRef)new ByIndex(value: value));

    internal Fin<TComponent> Resolve<TComponent>(RhinoDoc document, ResourceLens<TComponent> lens) where TComponent : class =>
        Switch(
            state: (Document: document, Lens: lens),
            byId: static (ctx, address) => Try.lift(() =>
                Optional(ctx.Lens.ById(ctx.Document, address.Value.Value)).ToFin(Fail: new KernelFault.MissingContext())).Run().Bind(static inner => inner),
            byName: static (ctx, address) => Try.lift(() =>
                Optional(ctx.Lens.ByName(ctx.Document, address.Value.Value)).ToFin(Fail: new KernelFault.MissingContext())).Run().Bind(static inner => inner),
            byIndex: static (ctx, address) => Try.lift(() =>
                Optional(ctx.Lens.ByIndex(ctx.Document, address.Value.Value)).ToFin(Fail: new KernelFault.MissingContext())).Run().Bind(static inner => inner));
}
```

## [04]-[RUN]

- Owner: policy vocabularies close every provider-mode discriminant on rows. `SelectionAxis` is the selection-conduct vocabulary the host's own selection members read as ONE `CapabilitySet<SelectionAxis>`. `TableOp` `[Union]` carries admitted per-occurrence payloads; `TableTransaction` `[Union]` distinguishes recorded, immediate, and navigation programs by shape, and the `UndoTrait`/`OpTrait` sets derive required versus recorded undo behavior without plan booleans. The commit envelope — `UndoBracket`, `RedrawScope`, `DocumentCommit.Sealed`, and the `HostInteraction` axis — is `Document/commit.md`'s, and this pipeline composes it.
- Entry: `TableOp` factories admit raw payloads once. `Add` and `Replace` seal heterogeneous geometry inside `GeometryIntake`; its later `Admit` stage applies the fresh kernel `Context`, `Requirement`, and `GeometryForm` lease without exposing the raw value again. `TableTransaction.Recorded`, `Immediate`, and `Navigate` admit program shape before `Tables.Commit(DocumentSession, TableTransaction)` enters the host boundary.
- Law: `TransformPolicy.Relocate` moves the source; `Copy` and `History` preserve it. `ObjectState` skips rows already in the requested state, and selection validates the host's affected-count return directly.
- Law: selection conduct is ONE set. `SelectionAxis.Baseline` carries the host's default selection posture, and every host argument reads `Admits` at its own call.
- Law: an operation-factory key threads through every constructor as a trailing `Op? key = null`, so one caller-minted key spans a whole program and every refusal names the request that produced it; the three `params`-bearing factories carve out and mint at the entry, because an optional before `params` forecloses the positional spread. A host member reporting failure as `Guid.Empty` — `Add`, `AddOrderedPointCloud`, `Transform` — admits through `ResourceId.Admit`, the spine's one empty-guid projector.
- Law: `TableOp.Traits` totally classifies every case onto one of four trait rows — `Sourced`, `Recorded`, `Immediate`, `Navigation` — each carrying its undo, navigation, and kernel-context demands as ONE `CapabilitySet<OpTrait>` column. A host effect that cannot be reversed by the document record enters only an immediate transaction, so a recorded program has no untracked side effect.
- Law: `Amend` owns a duplicated `ObjectAttributes` lease, takes the admitted `AttributeChange` payload, commits the duplicate synchronously, and disposes it before the operation leaves the host boundary. `AttributeChange` is the BOUNDARY type: this spine is S0 and the typed attribute program is S2, so the payload value seats here and the objects page's `AttributeProgram` composes it upward.
- Law: deleted-object operations require a query whose admitted axes select deleted rows. `TableTarget.Serials` reads the native runtime serials from that query inside the operation window, so `Revive` and `Expunge` never re-enter the active-id index.
- Law: `GeometryIntake` is the staged boundary union: `Of` separates native borrowed geometry from value-form conversion, while `Admit` resolves `Kind` and applies `Requirement.ForKind` under the fresh document context. Native geometry remains borrowed; every value-form conversion composes `GeometryForm` and is disposed by `Lease.Use` after the host copies it.
- Law: page import carries `DocumentPath` and re-proves `DocumentFile.ThreeDm` inside the callback. Named-view restore carries `ViewportTarget`, resolves exactly one viewport immediately before the host call, and never retains a live viewport handle in request data; direct, proportional, constant-speed, and constant-time host modalities close as `NamedRestore` cases with delay and speed entering as admitted values.
- Law: `Tables.Commit` keeps the document handle inside one `DocumentSession.Demand`, proving mutation, undo, and redraw needs against one snapshot before the first edit and refreshing the kernel context inside that window; the bracketing, sealing, and rollback are the commit envelope's own laws.
- Boundary: `AddCustomUndoEvent` has no host remove counterpart, so the document retains a `TableCustomUndo` handler, its whole captured object graph, and its arbitrary `object` tag until the undo record clears — a retention no `Subscription` can shorten, unlike every other host attachment in the slice. A handler therefore captures detached ids, stamps, and admitted values only. A captured live `RhinoObject`, `ObjectAttributes`, session, or lease outlives the commit that minted it and is the leak this law forecloses; the events page's process-global custody census carries the matching row.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
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

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SelectionAxis : ICapability<SelectionAxis> {
    public static readonly SelectionAxis SyncHighlight = new(key: "sync-highlight");
    public static readonly SelectionAxis Persistent = new(key: "persistent");
    public static readonly SelectionAxis IgnoreGrips = new(key: "ignore-grips");
    public static readonly SelectionAxis IgnoreLayerLocks = new(key: "ignore-layer-locks");
    public static readonly SelectionAxis IgnoreLayerVisibility = new(key: "ignore-layer-visibility");

    public static CapabilitySet<SelectionAxis> Baseline => Seed.Value;
    private static readonly Lazy<CapabilitySet<SelectionAxis>> Seed = new(static () =>
        CapabilitySet<SelectionAxis>.Of(SyncHighlight, Persistent, IgnoreGrips));
}

[ComplexValueObject]
[ValidationError]
public sealed partial class AttributeChange {
    public Func<ObjectAttributes, Fin<Unit>> Revise { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Func<ObjectAttributes, Fin<Unit>> revise) =>
        validationError = revise is null
            ? new ValidationError(message: "Attribute change carries no revision body.")
            : null;
}

[SmartEnum]
public sealed partial class TransformPolicy {
    public static readonly TransformPolicy Relocate = new(apply: static (table, id, motion) => table.Transform(objectId: id, xform: motion, deleteOriginal: true));
    public static readonly TransformPolicy Copy = new(apply: static (table, id, motion) => table.Transform(objectId: id, xform: motion, deleteOriginal: false));
    public static readonly TransformPolicy History = new(apply: static (table, id, motion) => table.TransformWithHistory(objectId: id, xform: motion));

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
    public static readonly ObjectState Hidden = new(done: static native => native.IsHidden, apply: static (table, id, ignore) => table.Hide(objectId: id, ignoreLayerMode: ignore));
    public static readonly ObjectState Shown = new(done: static native => !native.IsHidden, apply: static (table, id, ignore) => table.Show(objectId: id, ignoreLayerMode: ignore));
    public static readonly ObjectState Locked = new(done: static native => native.IsLocked, apply: static (table, id, ignore) => table.Lock(objectId: id, ignoreLayerMode: ignore));
    public static readonly ObjectState Unlocked = new(done: static native => !native.IsLocked, apply: static (table, id, ignore) => table.Unlock(objectId: id, ignoreLayerMode: ignore));

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

    public static Fin<NamedRestore> Direct(int index, ViewportTarget target) =>
        Addressed(index: index, target: target)
            .ToFin()
            .Map(static address => (NamedRestore)new DirectCase(Index: address.Index, Target: address.Target));

    public static Fin<NamedRestore> Proportional(int index, ViewportTarget target) =>
        Addressed(index: index, target: target)
            .ToFin()
            .Map(static address => (NamedRestore)new ProportionalCase(Index: address.Index, Target: address.Target));

    public static Fin<NamedRestore> ConstantTime(
        int index,
        ViewportTarget target,
        Rasm.Numerics.Dimension frames,
        TimeSpan delay) {
        return (
                Addressed(index: index, target: target),
                guard(frames.Value > 0, new KernelFault.InvalidInput()).ToFin().ToValidation(),
                Delay(delay: delay).ToValidation())
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
        TimeSpan delay) {
        return (
                Addressed(index: index, target: target),
                guard(double.IsFinite(unitsPerFrame) && unitsPerFrame > 0.0, new KernelFault.InvalidInput()).ToFin().ToValidation(),
                Delay(delay: delay).ToValidation())
            .Apply((address, _, ms) => (NamedRestore)new SpeedCase(
                Index: address.Index,
                Target: address.Target,
                UnitsPerFrame: unitsPerFrame,
                DelayMs: ms))
            .As()
            .ToFin();
    }

    private static Validation<Error, (int Index, ViewportTarget Target)> Addressed(int index, ViewportTarget target) =>
        (
            guard(index >= 0, new KernelFault.InvalidInput()).ToFin().ToValidation(),
            Admit.Need(target).ToValidation())
        .Apply((_, address) => (Index: index, Target: address))
        .As();

    private static Fin<int> Delay(TimeSpan delay) =>
        guard(
            delay >= TimeSpan.Zero
            && delay.Ticks % TimeSpan.TicksPerMillisecond is 0
            && delay.TotalMilliseconds <= int.MaxValue,
            new KernelFault.InvalidInput()).ToFin().Map(_ => (int)delay.TotalMilliseconds);

    internal Fin<Unit> Apply(RhinoDoc document) =>
        from address in Switch(
            directCase: static restore => Fin.Succ(value: (restore.Index, restore.Target)),
            proportionalCase: static restore => Fin.Succ(value: (restore.Index, restore.Target)),
            speedCase: static restore => Fin.Succ(value: (restore.Index, restore.Target)),
            timeCase: static restore => Fin.Succ(value: (restore.Index, restore.Target)))
        from viewport in address.Target.ResolveViewport(document: document)
        from applied in Switch(
            state: (Document: document, Viewport: viewport),
            directCase: static (context, restore) =>
                from _ in Admit.Confirm(success: context.Document.NamedViews.Restore(
                    index: restore.Index,
                    viewport: context.Viewport))
                select unit,
            proportionalCase: static (context, restore) =>
                from _ in Admit.Confirm(success: context.Document.NamedViews.RestoreWithAspectRatio(
                    index: restore.Index,
                    viewport: context.Viewport))
                select unit,
            speedCase: static (context, restore) =>
                from _ in Admit.Confirm(success: context.Document.NamedViews.RestoreAnimatedConstantSpeed(
                    index: restore.Index,
                    viewport: context.Viewport,
                    units_per_frame: restore.UnitsPerFrame,
                    ms_delay: restore.DelayMs))
                select unit,
            timeCase: static (context, restore) =>
                from _ in Admit.Confirm(success: context.Document.NamedViews.RestoreAnimatedConstantTime(
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

    public static Fin<HistoryRoll> ClearUndo(DeletedPolicy deleted, Option<uint> serial = default) {
        return (
                Admit.Need(deleted).ToValidation(),
                guard(
                    serial.Map(static value => value > 0u).IfNone(noneValue: true),
                    new KernelFault.InvalidInput()).ToFin().ToValidation())
            .Apply((policy, _) => (HistoryRoll)new ClearUndoCase(Deleted: policy, Serial: serial))
            .As()
            .ToFin();
    }

    internal Fin<Unit> Apply(RhinoDoc document) =>
        Switch(
            state: document,
            undoCase: static (context, _) => Admit.Confirm(success: context.Undo()),
            redoCase: static (context, _) => Admit.Confirm(success: context.Redo()),
            clearUndoCase: static (context, roll) => Try.lift(() => {
                roll.Serial.Match(
                    Some: serial => context.ClearUndoRecords(undoSerialNumber: serial, purgeDeletedObjects: roll.Deleted.Key),
                    None: () => context.ClearUndoRecords(purgeDeletedObjects: roll.Deleted.Key));
                return Fin.Succ(value: unit);
            }).Run().Bind(static inner => inner),
            clearRedoCase: static (context, _) => Try.lift(() => {
                context.ClearRedoRecords();
                return Fin.Succ(value: unit);
            }).Run().Bind(static inner => inner));
}

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
    private sealed record ClearSelectionCase(SelectionClear Scope) : TableOp;
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
        return (
                Admit.Need(custody).ToValidation(),
                toSeq(sources.ToArray())
                    .Traverse(source => GeometryIntake.Of(source: source).ToValidation())
                    .As(),
                guard(sources.Length > 0, new KernelFault.InvalidInput()).ToFin().ToValidation())
            .Apply(static (policy, values, _) => (TableOp)new AddCase(
                Sources: values,
                Attributes: attributes,
                History: history,
                Custody: policy))
            .As()
            .ToFin();
    }

    public static Fin<TableOp> Replace(TableTarget target, object replacement, ModeRegard modes) {
        return (
                Admit.Need(target).ToValidation(),
                GeometryIntake.Of(source: replacement).ToValidation(),
                Admit.Need(modes).ToValidation())
            .Apply(static (address, geometry, policy) =>
                (TableOp)new ReplaceCase(Target: address, Replacement: geometry, Modes: policy))
            .As()
            .ToFin();
    }

    public static Fin<TableOp> Delete(TableTarget target, HostInteraction interaction, ModeRegard modes) =>
        Admitted(first: target, second: interaction, third: modes, mint: static (address, dialogue, policy) =>
            new DeleteCase(Target: address, Interaction: dialogue, Modes: policy));

    public static Fin<TableOp> Transform(TableTarget target, Transform motion, TransformPolicy policy) {
        return (
                Admit.Need(target).ToValidation(),
                Acceptance.Input(value: motion).ToValidation(),
                Admit.Need(policy).ToValidation())
            .Apply(static (address, transform, mode) => (TableOp)new TransformCase(
                Target: address,
                Motion: transform,
                Policy: mode))
            .As()
            .ToFin();
    }

    public static Fin<TableOp> Amend(TableTarget target, AttributeChange change, HostInteraction interaction) =>
        Admitted(first: target, second: change, third: interaction, mint: static (address, revise, dialogue) =>
            new AmendCase(Target: address, Change: revise, Interaction: dialogue));

    public static Fin<TableOp> Select(TableTarget target, SelectionEdit edit, CapabilitySet<SelectionAxis> policy) {
        return (
                Admit.Need(target).ToValidation(),
                Admit.Need(edit).ToValidation())
            .Apply((address, mutation) => (TableOp)new SelectCase(Target: address, Edit: mutation, Policy: policy))
            .As()
            .ToFin();
    }

    public static Fin<TableOp> State(TableTarget target, ObjectState state, ModeRegard modes) =>
        Admitted(first: target, second: state, third: modes, mint: static (address, mutation, policy) =>
            new StateCase(Target: address, State: mutation, Modes: policy));

    public static Fin<TableOp> ClearSelection(SelectionClear scope) =>
        key.OrDefault().Need(scope)
            .Map(value => (TableOp)new ClearSelectionCase(Scope: value));

    public static Fin<TableOp> Flash(TableTarget target, FlashMode mode) =>
        Admitted(first: target, second: mode, mint: static (address, display) =>
            new FlashCase(Target: address, Mode: display));

    private static Fin<TableOp> Admitted<T1, T2>(T1 first, T2 second, Func<T1, T2, TableOp> mint)
        where T1 : class where T2 : class {
        return (
                Admit.Need(first).ToValidation(),
                Admit.Need(second).ToValidation())
            .Apply(mint)
            .As()
            .ToFin();
    }

    private static Fin<TableOp> Admitted<T1, T2, T3>(T1 first, T2 second, T3 third, Func<T1, T2, T3, TableOp> mint)
        where T1 : class where T2 : class where T3 : class {
        return (
                Admit.Need(first).ToValidation(),
                Admit.Need(second).ToValidation(),
                Admit.Need(third).ToValidation())
            .Apply(mint)
            .As()
            .ToFin();
    }

    public static Fin<TableOp> Revive(TableTarget target) =>
        Retained(target: target, mint: static value => new ReviveCase(Target: value));

    public static Fin<TableOp> Expunge(TableTarget target) =>
        Retained(target: target, mint: static value => new ExpungeCase(Target: value));

    private static Fin<TableOp> Retained(TableTarget target, Func<TableTarget, TableOp> mint) {
        return Admit.Need(target)
            .Bind(value => value.SelectsDeleted ? Fin.Succ(value: mint(arg: value)) : Fin.Fail<TableOp>(new KernelFault.InvalidInput()));
    }

    public static Fin<TableOp> Cloud(
        Rasm.Numerics.Dimension x,
        Rasm.Numerics.Dimension y,
        Rasm.Numerics.Dimension z,
        Arr<Point3d> box,
        ObjectCustody custody,
        Option<ObjectAttributes> attributes = default,
        Option<HistoryRecord> history = default) {
        return (
                Admit.Need(custody).ToValidation(),
                guard(
                    box.Count is 8
                    && x != default && y != default && z != default
                    && x.Value <= int.MaxValue / y.Value / z.Value,
                    new KernelFault.InvalidInput()).ToFin().ToValidation(),
                box.AsIterable().ToSeq()
                    .Traverse(point => Acceptance.Input(value: point).ToValidation())
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

    public static Fin<TableOp> Rebind(TableTarget target, ResourceIndex definitionIndex) {
        return (
                Admit.Need(target).ToValidation(),
                Admit.Need(definitionIndex).ToValidation())
            .Apply(static (address, index) => (TableOp)new RebindCase(
                Target: address,
                DefinitionIndex: index))
            .As()
            .ToFin();
    }

    public static Fin<TableOp> Reclaim(TableKind kind) =>
        Optional(kind).ToFin(Fail: key.OrDefault().InvalidInput()).Map(static value => (TableOp)new ReclaimCase(Kind: value));

    public static Fin<TableOp> ImportPage(DocumentPath path, Guid mainViewportId, string pageName) {
        return (
                guard(path != default, new KernelFault.InvalidInput()).ToFin().ToValidation(),
                Acceptance.Input(value: mainViewportId).ToValidation(),
                Acceptance.Text(value: pageName).ToValidation())
            .Apply((_, viewport, name) => (TableOp)new ImportPageCase(
                Path: path,
                MainViewportId: viewport,
                PageName: name))
            .As()
            .ToFin();
    }

    public static Fin<TableOp> RestoreView(NamedRestore restore) =>
        Optional(restore).ToFin(Fail: key.OrDefault().InvalidInput()).Map(static value => (TableOp)new RestoreViewCase(Restore: value));

    public static Fin<TableOp> Roll(HistoryRoll navigation) =>
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

    internal Fin<Unit> Apply(RhinoDoc document, Option<Context> domain) =>
        Switch(
            (Document: document, Domain: domain),
            addCase: static (context, edit) =>
                from model in context.Domain.ToFin(Fail: new KernelFault.MissingContext())
                from ids in edit.Sources.TraverseM(source => source.Admit(domain: model)
                    .Bind(lease => lease.Use(native => ResourceId.Admit(
                        value: context.Document.Objects.Add(
                            geometry: native,
                            attributes: HostEdge.Slot(edit.Attributes),
                            history: HostEdge.Slot(edit.History),
                            reference: edit.Custody.Key)).Map(static id => id.Value)))).As()
                select unit,
            replaceCase: static (context, edit) =>
                from model in context.Domain.ToFin(Fail: new KernelFault.MissingContext())
                from ids in edit.Target.Resolve(document: context.Document)
                from single in Tables.One(rows: ids)
                from _ in edit.Replacement.Admit(domain: model)
                    .Bind(lease => lease.Use(native => Admit.Confirm(success: context.Document.Objects.Replace(objectId: single, geometry: native, ignoreModes: edit.Modes.Key))))
                select unit,
            deleteCase: static (context, edit) =>
                from targets in edit.Target.Resolve(document: context.Document)
                from _ in edit.Modes.Key
                    ? targets.TraverseM(target => Optional(context.Document.Objects.FindId(target)).ToFin(Fail: new KernelFault.InvalidResult())
                        .Bind(native => Admit.Confirm(success: context.Document.Objects.Delete(obj: native, quiet: edit.Interaction.IsQuiet, ignoreModes: true)))).As().Map(static _ => unit)
                    : Admit.Confirm(success: context.Document.Objects.Delete(objectIds: targets.AsIterable(), quiet: edit.Interaction.IsQuiet) == targets.Count)
                select unit,
            transformCase: static (context, edit) => Mapped(
                document: context.Document,
                target: edit.Target,
                step: id => ResourceId.Admit(
                    value: edit.Policy.Apply(table: context.Document.Objects, id: id, motion: edit.Motion)).Map(static _ => unit)),
            amendCase: static (context, edit) => Mapped(
                document: context.Document,
                target: edit.Target,
                step: id =>
                    from native in Optional(context.Document.Objects.FindId(id)).ToFin(Fail: new KernelFault.InvalidResult())
                    from attributes in Optional(native.Attributes?.Duplicate()).ToFin(Fail: new KernelFault.InvalidResult())
                    from _ in new Lease<ObjectAttributes>.Owned(Value: attributes).Use(owned =>
                        from __ in edit.Change.Revise(arg: owned)
                        from ___ in Admit.Confirm(success: context.Document.Objects.ModifyAttributes(
                            objectId: id,
                            newAttributes: owned,
                            quiet: edit.Interaction.IsQuiet))
                        select unit)
                    select unit),
            selectCase: static (context, edit) =>
                from ids in edit.Target.Resolve(document: context.Document)
                from _ in guard(
                    edit.Edit.Apply(table: context.Document.Objects, ids: ids.AsIterable(), held: edit.Policy) >= 0,
                    new KernelFault.InvalidResult())
                select unit,
            stateCase: static (context, edit) =>
                from targets in edit.Target.Resolve(document: context.Document)
                from _ in Tables.ApplyState(document: context.Document, targets: targets, state: edit.State, modes: edit.Modes)
                select unit,
            clearSelectionCase: static (context, edit) => guard(
                context.Document.Objects.UnselectAll(ignorePersistentSelections: edit.Scope.Key) >= 0,
                new KernelFault.InvalidResult()).ToFin(),
            flashCase: static (context, edit) =>
                from targets in edit.Target.Resolve(document: context.Document)
                from objects in targets.TraverseM(target => Optional(context.Document.Objects.FindId(target)).ToFin(Fail: new KernelFault.InvalidResult())).As()
                from _ in Try.lift(() => {
                    context.Document.Views.FlashObjects(list: objects.AsIterable(), useSelectionColor: edit.Mode.Key);
                    return Fin.Succ(value: unit);
                }).Run().Bind(static inner => inner)
                select unit,
            reviveCase: static (context, edit) => Lifecycle(
                document: context.Document, target: edit.Target,
                apply: static (objects, serial) => objects.Undelete(runtimeSerialNumber: serial)),
            expungeCase: static (context, edit) => Lifecycle(
                document: context.Document, target: edit.Target,
                apply: static (objects, serial) => objects.Purge(runtimeSerialNumber: serial)),
            cloudCase: static (context, edit) =>
                from id in ResourceId.Admit(
                    value: context.Document.Objects.AddOrderedPointCloud(
                        xCt: edit.X.Value,
                        yCt: edit.Y.Value,
                        zCt: edit.Z.Value,
                        box: edit.Box.ToArray(),
                        attributes: HostEdge.Slot(edit.Attributes),
                        history: HostEdge.Slot(edit.History),
                        reference: edit.Custody.Key))
                select unit,
            rebindCase: static (context, edit) => Mapped(
                document: context.Document,
                target: edit.Target,
                step: id => Admit.Confirm(success: context.Document.Objects.ReplaceInstanceObject(objectId: id, instanceDefinitionIndex: edit.DefinitionIndex.Value))),
            reclaimCase: static (context, edit) => edit.Kind.Reclaim(document: context.Document)
                .Map(static _ => unit),
            importPageCase: static (context, edit) =>
                from path in edit.Path.Resolve(file: DocumentFile.ThreeDm)
                let before = context.Document.Views.PageViewCount
                from _ in Admit.Confirm(success: context.Document.Views.ImportPageView(filename: path, mainViewportId: edit.MainViewportId, pageName: edit.PageName))
                let imported = context.Document.Views.PageViewCount - before
                from __ in guard(imported > 0, new KernelFault.InvalidResult())
                select unit,
            restoreViewCase: static (context, edit) =>
                from _ in edit.Restore.Apply(document: context.Document)
                select unit,
            rollCase: static (context, edit) =>
                from _ in edit.Navigation.Apply(document: context.Document)
                select unit);

    private static Fin<Unit> Lifecycle(RhinoDoc document, TableTarget target, Func<ObjectTable, uint, bool> apply) =>
        from targets in target.Serials(document: document)
        from _ in targets.TraverseM(serial => Admit.Confirm(success: apply(document.Objects, serial))).As()
        select unit;

    private static Fin<Unit> Mapped(RhinoDoc document, TableTarget target, Func<Guid, Fin<Unit>> step) =>
        from ids in target.Resolve(document: document)
        from _ in ids.TraverseM(step).As()
        select unit;
}

// --- [MODELS] --------------------------------------------------------------------------
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
        return (
                Acceptance.Text(value: name).ToValidation(),
                Admit.Need(handler).ToValidation())
            .Apply((admitted, callback) => new TableCustomUndo(
                name: admitted,
                handler: callback,
                tag: tag))
            .As()
            .ToFin();
    }

    internal Fin<string> Register(RhinoDoc document) =>
        Tag.Match(
            Some: tag => Admit.Confirm(success: document.AddCustomUndoEvent(description: Name, handler: Handler, tag: tag)),
            None: () => Admit.Confirm(success: document.AddCustomUndoEvent(description: Name, handler: Handler)))
        .Map(_ => Name);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TableTransaction {
    private TableTransaction() { }

    private sealed record RecordedCase(string Name, Seq<TableOp> Operations, RedrawPolicy Redraw, Seq<TableCustomUndo> CustomUndo) : TableTransaction;
    private sealed record ImmediateCase(Seq<TableOp> Operations, RedrawPolicy Redraw) : TableTransaction;
    private sealed record NavigationCase(TableOp Operation, RedrawPolicy Redraw) : TableTransaction;

    public static Fin<TableTransaction> Recorded(string name, RedrawPolicy redraw, Seq<TableCustomUndo> customUndo, params ReadOnlySpan<TableOp> operations) {
        return from admitted in (
                   Acceptance.Text(value: name).ToValidation(),
                   Admit(redraw: redraw, operations: operations).ToValidation(),
                   customUndo
                       .Traverse(item => Admit.Need(item).ToValidation())
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
                   new KernelFault.InvalidInput())
               select (TableTransaction)new RecordedCase(
                   Name: admitted.Name,
                   Operations: admitted.Plan.Operations,
                   Redraw: admitted.Plan.Redraw,
                   CustomUndo: admitted.Undo);
    }

    public static Fin<TableTransaction> Immediate(RedrawPolicy redraw, params ReadOnlySpan<TableOp> operations) {
        return from plan in Admit(redraw: redraw, operations: operations)
               from _ in guard(
                   plan.Operations.Count is 1
                   && plan.Operations.ForAll(static operation =>
                       !operation.Traits.Demands().Admits(capability: OpTrait.RecordsUndo)
                       && !operation.Traits.Demands().Admits(capability: OpTrait.Navigates)),
                   new KernelFault.InvalidInput())
               select (TableTransaction)new ImmediateCase(Operations: plan.Operations, Redraw: plan.Redraw);
    }

    public static Fin<TableTransaction> Navigate(HistoryRoll navigation, RedrawPolicy redraw) {
        return (
                TableOp.Roll(navigation: navigation).ToValidation(),
                Admit.Need(redraw).ToValidation())
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
        ReadOnlySpan<TableOp> operations) =>
        (
            Admit.Need(redraw).ToValidation(),
            Admission.All(values: operations).ToValidation())
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

// --- [OPERATIONS] ----------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GeometryIntake {
    private GeometryIntake() { }

    private sealed record NativeCase(GeometryBase Source) : GeometryIntake;
    private sealed record ValueCase(object Source) : GeometryIntake;

    public static Fin<GeometryIntake> Of(object source) =>
        Optional(source).ToFin(Fail: key.OrDefault().InvalidInput()).Map(static value => value switch {
            GeometryBase native => (GeometryIntake)new NativeCase(Source: native),
            _ => new ValueCase(Source: value),
        });

    internal Fin<Lease<GeometryBase>> Admit(Context domain) =>
        Switch(
            state: domain,
            nativeCase: static (context, intake) =>
                from _ in Require(source: intake.Source, domain: context)
                select (Lease<GeometryBase>)new Lease<GeometryBase>.Borrowed(Value: intake.Source),
            valueCase: static (context, intake) =>
                from _ in Require(source: intake.Source, domain: context)
                from lease in intake.Source.GeometryForm()
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
    public static Fin<Unit> Commit(
        DocumentSession session,
        TableTransaction transaction) {
        return from admission in Admission.Pair(first: session, second: transaction)
               let plan = admission.Second.Materialize()
               from _ in admission.First.Demand(
                   use: document => Run(document: document, plan: plan),
                   needs: SessionNeed.Mutation(
                       undo: plan.Undo.Demands().Admits(capability: UndoTrait.Required),
                       redraw: plan.Redraw).ToArray())
               select unit;
    }

    internal static Fin<T> One<T>(Seq<T> rows) =>
        rows switch { [var only] => Fin.Succ(value: only), _ => Fin.Fail<T>(error: new KernelFault.InvalidInput()) };

    internal static Fin<Unit> ApplyState(RhinoDoc document, Seq<Guid> targets, ObjectState state, ModeRegard modes) =>
        targets.TraverseM(target => Optional(document.Objects.FindId(target))
            .ToFin(Fail: new KernelFault.InvalidResult())
            .Bind(native => state.Done(native: native)
                ? Fin.Succ(value: unit)
                : state.Apply(table: document.Objects, id: target, ignoreLayerMode: modes.Key)
                    ? Fin.Succ(value: unit)
                    : Fin.Fail<Unit>(error: new KernelFault.InvalidResult()))).As().Map(static _ => unit);

    private static Fin<Unit> Run(RhinoDoc document, TransactionPlan plan) =>
        from domain in plan.Operations.Exists(static operation => operation.Traits.Demands().Admits(capability: OpTrait.RequiresContext))
            ? Rasm.Domain.Context.Of(doc: document).ToFin().Map(Some)
            : Fin.Succ(Option<Context>.None)
        from _ in DocumentCommit.Sealed(
            document: document,
            name: plan.RecordName.IfNone(nameof(Tables)),
            recordsUndo: plan.Undo.Demands().Admits(capability: UndoTrait.Records),
            redraw: plan.Redraw,
            run: () =>
                from _registered in plan.CustomUndo.TraverseM(undo => undo.Register(document: document)).As()
                from _applied in plan.Operations
                    .TraverseM(operation => operation.Apply(document: document, domain: domain)).As()
                select unit,
            project: Fin.Succ)
        select unit;
}
```

## [05]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]              | [OWNER]                        | [FORM]                       | [ENTRY]                               |
| :-----: | :--------------------- | :----------------------------- | :--------------------------- | :------------------------------------ |
|  [01]   | document tables        | `TableKind`                    | keyed behavior rows          | `ForComponentType` / `Reclaim`        |
|  [02]   | object addressing      | `TableTarget`                  | ids/query union              | `Of` / `Query`                        |
|  [03]   | query predicates       | `TablePredicate`               | frozen predicate union       | `Tag` / `Color` / `Bounds`            |
|  [04]   | query axes + product   | `QueryAxis` / `QuerySpec`      | seated rows + admitted value | `Of` / `Build(document, key)`         |
|  [05]   | mutation program       | `TableOp`                      | admitted total union         | operation factories / `Apply`         |
|  [06]   | attribute payload      | `AttributeChange`              | admitted mutation body       | `TableOp.Amend`                       |
|  [07]   | commit scope           | `TableTransaction`             | program-mode union           | `Recorded` / `Immediate` / `Navigate` |
|  [08]   | resource ingress       | `GeometryIntake`               | native/value custody union   | `Admit`                               |
|  [09]   | table commit spine     | `Tables`                       | session/commit fold          | `Commit`                              |
|  [10]   | component addressing   | `ResourceRef` / `ResourceLens` | id/name/index over a lens    | `Of` / `Resolve(document, lens, key)` |
|  [11]   | viewport addressing    | `ViewportTarget`               | address & census union       | `Active` / `ResolveViewport`          |
|  [12]   | object-type vocabulary | `ObjectKind` / `ObjectKinds`   | keyed rows over a set        | `Of` / `Any` / `Mask` / `OfMask`      |
|  [13]   | space partition        | `ActiveSpaceUse`               | host-keyed rows              | `Get` / `Key`                         |
|  [14]   | selection conduct      | `SelectionAxis`                | capability vocabulary        | `Baseline` / `Admits`                 |

- Packages: `RhinoCommon` (`Rasm.Rhino/.api/api-rhinocommon-document.md` + `api-rhinocommon-document-state.md` — the component-table surface: layers, groups, views, instance definitions, named states); `Thinktecture.Runtime.Extensions` (`libs/dotnet/.api/api-thinktecture-runtime-extensions.md` — `[SmartEnum]` query/selection/trait rows and `[ValueObject]` `ResourceName` with `[KeyMemberEqualityComparer]`); `LanguageExt.Core` (`libs/dotnet/.api/api-languageext.md` — `Seq`/`HashMap` table projections); kernel `Domain/results` + `Domain/validation` (`FactoryBridge.Row` host-enum admission).

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
