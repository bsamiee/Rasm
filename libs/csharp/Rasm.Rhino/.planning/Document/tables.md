# [RASM_RHINO_TABLES]

`Rasm.Rhino.Document` owns document-table vocabulary, object addressing, mutation programs, undo/redraw bracketing, and consequence evidence. `TableKind` captures each admitted host document table, `TableTarget` freezes explicit, runtime, and host-query addressing, and `TableOp` closes the mutation family. `Tables.Commit` executes one admitted program inside one session capability window, refreshes the kernel `Context`, seals every undo exit, compensates redraw state on every outcome, and returns one typed fact stream.

## [01]-[INDEX]

- [02]-[TABLE_VOCABULARY]: `TableKind` — document-table identity, component correspondence, and reclamation behavior.
- [03]-[TARGET_ALGEBRA]: `ObjectRuntime`, `BoundsMatch`, `TablePredicate`, `TableTarget`, and `ViewportTarget` — immutable object addressing, viewport addressing, and query composition.
- [04]-[TRANSACTION_RAIL]: mutation policy rows, `NamedRestore`, `HistoryRoll`, `TableOp`, `TableTransaction`, `GeometryIntake`, and `Tables`.
- [05]-[RECEIPTS]: `TableSlot`, `TableFact`, and `TableReceipt` — one runtime-addressable consequence stream.
- [06]-[SURFACE_LEDGER]: the page owner map.

## [02]-[TABLE_VOCABULARY]

- Owner: `TableKind` `[SmartEnum<int>]` binds each admitted document table to its `ModelComponentType` and table-owned reclamation delegate.
- Entry: `ForComponentType(ModelComponentType) : Fin<Seq<TableKind>>` returns every mapped row, expands `ModelComponentType.Mixed` across every explicit correspondence, treats `ModelComponentType.Unset` as absent correspondence, and rejects an undefined foreign ordinal. `Reclaim(RhinoDoc, Op) : Fin<int>` invokes the row delegate and rejects a table with no host reclamation member.
- Law: table behavior resides on the row. A table extension declares component correspondence and reclamation behavior at construction, so no external dictionary or accessibility flag can drift from the vocabulary.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
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
                    .Filter(static kind => kind.ComponentType is not ModelComponentType.Unset and not ModelComponentType.Mixed)
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

- Owner: `ObjectRuntime` `[ComplexValueObject]` admits the durable `(Guid, runtime serial)` pair required after an object leaves the active-id index. `ObjectQuery` freezes the complete `ObjectEnumeratorSettings` product and replaces its live `ViewportFilter` slot with the stable `ViewportTarget` owner. `TableTarget` `[Union]` closes nonempty explicit ids, nonempty runtime pairs, and admitted queries. `TablePredicate` `[Union]` adds composable tag, draw-color, and kernel-bounds predicates; `BoundsMatch` owns containment versus intersection behavior as rows.
- Owner: `ViewportTarget` `[Union]` is the corpus-wide VIEWPORT address — active, named, id, page, detail, and census cases closed as one owner beside `TableKind` (which table), `TableTarget` (which objects), and `ResourceRef` (which component). `ViewportScope` `[SmartEnum<int>]` carries the model, page, and detail census generators and `EveryCase` freezes their set; `ViewportRef` is the ephemeral resolved row pairing `RhinoView`, `RhinoViewport`, and an optional `DetailViewObject`. `Active`/`Named`/`Id`/`Page`/`Detail`/`Every` construct, and `Resolve`, `ResolveOne`, and `ResolveViewport` fold one address to every row, exactly one row, or one native viewport inside the caller's document callback.
- Law: viewport resolution names `RhinoDoc.Views.ActiveView`, `.Find`, `.GetViewList`, `.GetPageViews`, `RhinoPageView.GetDetailViews`, and `DetailViewObject.Viewport` exactly once; a detail address matches either `DetailViewObject.Id` or `DetailViewObject.Viewport.Id`, and a resolution yielding no row refuses before any consumer projects it.
- Law: an addressed row binds `RhinoView.MainViewport` — the viewport the address names — because `RhinoPageView.ActiveViewport` silently returns an active detail; only `ActiveCase` binds `ActiveViewport`, adopting the host's active semantics, and a detail row binds `DetailViewObject.Viewport` and carries its `DetailViewObject` so a detail commit or scale conversion reads the owning object without a second lookup.
- Law: viewport rows resolve live per call inside the document callback and leave as detached addresses or one native viewport, never a retained handle; `ResolveViewport` composes `Resolve` and `Tables.One`, so the single-viewport consumers — `ObjectQuery` viewport filtering, `NamedRestore`, and the annotation dimension-scale probe — share one fold and no call site re-spells the resolve-then-one triple.
- Owner: `ResourceRef` is the corpus-wide COMPONENT address — id, name, index closed as one `[Union]` over a per-table `ResourceLens<TComponent>` — completing the addressing triad beside `TableKind` (which table) and `TableTarget` (which objects); `ResourceId`, `ResourceName`, and `ResourceIndex` admit the native address scalars once, and `ResourceId.Maybe`/`Admit` plus `ResourceIndex.Admit` are the sole `Guid.Empty` and negative-index sentinel projectors.
- Law: each component table contributes exactly one lens — Annotation's style, linetype, hatch, and section rails and Blocks' definition rail each declare one `ResourceLens<T>` row — and no folder mints a second address family; resolution reads live per call inside the owning operation, because tables mutate under commands, so no resolved component is cached on a value.
- Entry: `ObjectQuery.Of(ObjectEnumeratorSettings)`, `TableTarget.Of(params ReadOnlySpan<Guid>)`, `Deleted(params ReadOnlySpan<ObjectRuntime>)`, and `Query(ObjectQuery, params ReadOnlySpan<TablePredicate>)` are the only constructors. `Resolve` returns distinct ids; `Serials` preserves or resolves runtime pairs. `ObjectRuntime.Canonical` derives every runtime-pair deduplication from generated structural equality. A deleted-object lifecycle request composes `Deleted` from a prior receipt instead of attempting `FindId`, which cannot find deleted objects.
- Law: query settings are copied at admission and rebuilt at execution. Mutable caller settings never change an admitted target, the viewport resolves from stable identity inside the document callback, and every public host filter field remains available. Predicate evaluation accumulates independent object and predicate faults through `Validation<Error, T>` before lowering once to `Fin<T>`.
- Law: bounds predicates admit `BoundingBox.IsValid` before corner accumulation and compose the kernel `BoundsOf` owner. Containment and intersection derive from catalogued `Center` and `Diagonal` evidence; inflation remains host-query policy, while candidate classification and coercion stay kernel-owned.
- Boundary: `BoundingBox.Inflate` mutates a copied struct, so `Inflated` is the one statement kernel and never mutates request evidence.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
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

public sealed class ObjectQuery {
    private readonly ObjectEnumeratorSettings settings;
    private readonly Option<ViewportTarget> viewport;

    private ObjectQuery(ObjectEnumeratorSettings settings, Option<ViewportTarget> viewport) {
        this.settings = settings;
        this.viewport = viewport;
    }

    internal bool SelectsOnlyDeleted => settings.DeletedObjects
        && !settings.NormalObjects
        && !settings.LockedObjects
        && !settings.HiddenObjects
        && !settings.IdefObjects;

    public static Fin<ObjectQuery> Of(ObjectEnumeratorSettings settings, Option<ViewportTarget> viewport = default, Op? key = null) {
        Op op = key.OrDefault();
        return from source in Optional(settings).ToFin(Fail: op.InvalidInput())
               from _ in guard(source.ViewportFilter is null, op.InvalidInput()).ToFin()
               from target in viewport.Traverse(item => Optional(item).ToFin(Fail: op.InvalidInput())).As()
               select new ObjectQuery(settings: Copy(source: source, viewport: null), viewport: target);
    }

    internal Fin<ObjectEnumeratorSettings> Build(RhinoDoc document, Op key) =>
        viewport.Match(
            Some: target => target.ResolveViewport(document: document, key: key)
                .Map(resolved => Copy(source: settings, viewport: resolved)),
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
    private sealed record ColorCase(System.Drawing.Color Value) : TablePredicate;
    private sealed record BoundsCase(BoundingBox Region, BoundsMatch Match) : TablePredicate;

    public static Fin<TablePredicate> Tag(string key, Option<string> expected = default) =>
        Op.Of().AcceptText(value: key).Map(valid => (TablePredicate)new TagCase(Key: valid, Expected: expected));

    public static Fin<TablePredicate> Color(System.Drawing.Color value) =>
        guard(!value.IsEmpty, Op.Of().InvalidInput()).ToFin()
            .Map(_ => (TablePredicate)new ColorCase(Value: value));

    public static Fin<TablePredicate> Bounds(BoundingBox region, BoundsMatch match, double inflation = 0.0) {
        Op op = Op.Of();
        return from _ in guard(region.IsValid, op.InvalidInput()).ToFin()
               from predicate in (
                Optional(match).ToFin(Fail: op.InvalidInput()).ToValidation(),
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

    public static Fin<TableTarget> Query(ObjectQuery query, params ReadOnlySpan<TablePredicate> predicates) {
        Op op = Op.Of();
        return (
                Optional(query).ToFin(Fail: op.InvalidInput()).ToValidation(),
                Admission.All(values: predicates, key: op).ToValidation())
            .Apply(static (source, filters) => (TableTarget)new QueryCase(
                Query: source,
                Predicates: filters))
            .As()
            .ToFin();
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
                .Bind(rows => rows
                    .Traverse(native => ObjectRuntime.Of(
                        id: native.Id,
                        serial: native.RuntimeSerialNumber,
                        key: context.Op).ToValidation())
                    .As()
                    .ToFin()));

    private static Fin<Seq<RhinoObject>> Evaluate(QueryCase target, RhinoDoc document, Op key) =>
        from settings in target.Query.Build(document: document, key: key)
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

    private Func<RhinoDoc, Seq<ViewportRef>> Select { get; }
    internal Seq<ViewportRef> Resolve(RhinoDoc document) => Select(document).Strict();
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
                    .Bind(scope => scope.Resolve(document: ctx.Document))
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
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
        validationError = value >= 0 ? null : new ValidationError(message: "ResourceIndex requires a non-negative value.");

    internal static Fin<ResourceIndex> Admit(int value, Op key) =>
        value >= 0 ? Fin.Succ(value: Create(value)) : Fin.Fail<ResourceIndex>(error: key.InvalidResult());
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

    public static Fin<ResourceRef> Of(Guid id) =>
        ResourceId.Maybe(id).Map(static value => (ResourceRef)new ById(value: value))
            .ToFin(Fail: Op.Of(name: nameof(ResourceRef)).InvalidInput());

    public static Fin<ResourceRef> Of(string name) =>
        Op.Of(name: nameof(ResourceRef)).AcceptText(value: name).Map(static valid => (ResourceRef)new ByName(value: ResourceName.Create(valid)));

    public static Fin<ResourceRef> Of(int index) =>
        index >= 0
            ? Fin.Succ<ResourceRef>(value: new ByIndex(value: ResourceIndex.Create(index)))
            : Fin.Fail<ResourceRef>(error: Op.Of(name: nameof(ResourceRef)).InvalidInput());

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

- Owner: policy vocabularies close every provider-mode discriminant on rows. `SelectionPolicy` `[ComplexValueObject]` generates the complete highlight, grip, persistence, layer-lock, and layer-visibility product from independent axes. `TableOp` `[Union]` carries admitted per-occurrence payloads; `TableTransaction` `[Union]` distinguishes recorded, immediate, and navigation programs by shape, and `TransactionUndo` derives required versus recorded undo behavior without plan booleans. `UndoBracket` is the shared document transaction capsule, `RedrawScope.Within` is the one suppress/restore/success-gated-flush redraw bracket, and `DocumentCommit.Sealed` composes the two as the ONE commit envelope — every folder commit rail (table, layer, session-regime, annotation draft, block, object, render content, render settings, exchange, sheet, persistence preset, user-text, capture-adopt) commits through it, and a hand-spelled `UndoBracket.Begin` or redraw triple beside it is the deleted form; the flush fires only after the prior redraw state is restored, so a suppressing policy still lands its terminal repaint.
- Entry: `TableOp` factories admit raw payloads once. `Add` and `Replace` seal heterogeneous geometry inside `GeometryIntake`; its later `Admit` stage applies the fresh kernel `Context`, `Requirement`, and `GeometryForm` lease without exposing the raw value again. `TableTransaction.Recorded`, `Immediate`, and `Navigate` admit program shape before `Tables.Commit(DocumentSession, TableTransaction)` enters the host boundary.
- Law: `TransformPolicy.Relocate` reports the transformed identity as `Moved`; `Copy` and `History` report only the minted identity as `Created`. Sources remain unchanged on copy/history paths. Selection facts derive from before/after runtime snapshots, and state facts use separate `Hidden`/`Shown` and `Locked`/`Unlocked` slots.
- Law: `TableOp.Traits` totally classifies every case onto one of four trait rows — `Sourced`, `Recorded`, `Immediate`, `Navigation` — carrying undo recording, navigation, and kernel-context demand as one derived product. A host effect that cannot be reversed by the document record enters only an immediate transaction, so a recorded program has no untracked side effect.
- Law: `Amend` owns a duplicated `ObjectAttributes` lease, exposes only an in-place `Fin<Unit>` change callback, commits the duplicate synchronously, and disposes it before the operation leaves the host boundary. Canonical callback is the objects page's typed `AttributeProgram.Apply` — `Amend(target, program.Apply, notice)` — so attribute mutation is a closed `AttributeEdit` program; a hand-written mutation lambda survives only where no program case carries the member.
- Law: deleted-object operations require a runtime-preserving target. Explicit deleted rows and deleted-object queries preserve runtime serials without re-entering the active-id index, deletion captures runtime pairs before mutation, and receipts project them for a later `Revive` or `Expunge` request.
- Law: `GeometryIntake` is the staged boundary union: `Of` separates native borrowed geometry from value-form conversion, while `Admit` resolves `Kind` and applies `Requirement.ForKind` under the fresh document context. Native geometry remains borrowed; every value-form conversion composes `GeometryForm` and is disposed by `Lease.Use` after the host copies it.
- Law: page import carries `DocumentPath` and re-proves `DocumentFile.ThreeDm` inside the callback. Named-view restore carries `ViewportTarget`, resolves exactly one viewport immediately before the host call, and never retains a live viewport handle in request data.
- Law: named-view restore closes direct, proportional, constant-speed, and constant-time host modalities as `NamedRestore` cases. Delay and speed enter as admitted values, so no boolean or overload discriminator crosses the transaction boundary.
- Law: `Tables.Commit` keeps the document handle inside one `DocumentSession.Demand`, proving mutation, undo, and redraw needs against one snapshot before the first edit and refreshing the kernel context inside that window. Outside a command it owns the undo record, closes it on every exit, rolls a failed program back, clears the failed record from redo, and appends close, rollback, or redraw-restoration faults to the primary fault. Inside a command it enlists in `CurrentUndoRecordSerialNumber`, never closes or undoes the command-owned record, and returns the operation fault for the command boundary to propagate. `UndoBracket.Stamper` stamps only a required positive serial the admission guard already proved; an immediate program bypasses stamping, and invalid undo evidence fails before receipt construction. An active non-command record is rejected before mutation, and redraw occurs only after success.
- Law: `DocumentCommit.Sealed` and `Tables.Commit` carry a generic railed receipt projection executed inside the bracket after the undo-serial stamp and before sealing — a consumer fold that must observe the committed receipt (a command stage folding state) enters as `project`, its refusal rolls the owned record back like any operation fault, and a wrapper folding state outside the bracket is the deleted form; the identity projection is the default modality, so receipt-shaped rails commit unchanged.
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

    private sealed record DirectCase(int Index, ViewportTarget Target) : NamedRestore;
    private sealed record ProportionalCase(int Index, ViewportTarget Target) : NamedRestore;
    private sealed record SpeedCase(int Index, ViewportTarget Target, double UnitsPerFrame, int DelayMs) : NamedRestore;
    private sealed record TimeCase(int Index, ViewportTarget Target, Dimension Frames, int DelayMs) : NamedRestore;

    public static Fin<NamedRestore> Direct(int index, ViewportTarget target) =>
        Addressed(index: index, target: target, Op.Of())
            .ToFin()
            .Map(static address => (NamedRestore)new DirectCase(Index: address.Index, Target: address.Target));

    public static Fin<NamedRestore> Proportional(int index, ViewportTarget target) =>
        Addressed(index: index, target: target, Op.Of())
            .ToFin()
            .Map(static address => (NamedRestore)new ProportionalCase(Index: address.Index, Target: address.Target));

    public static Fin<NamedRestore> ConstantTime(int index, ViewportTarget target, Dimension frames, TimeSpan delay) {
        Op op = Op.Of();
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

    public static Fin<NamedRestore> ConstantSpeed(int index, ViewportTarget target, double unitsPerFrame, TimeSpan delay) {
        Op op = Op.Of();
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
            Optional(target).ToFin(Fail: op.InvalidInput()).ToValidation())
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

    public static Fin<HistoryRoll> ClearUndo(DeletedPolicy deleted, Option<uint> serial = default) {
        Op op = Op.Of();
        return (
                Optional(deleted).ToFin(Fail: op.InvalidInput()).ToValidation(),
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
        return (
                Optional(custody).ToFin(Fail: op.InvalidInput()).ToValidation(),
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

    public static Fin<TableOp> Replace(TableTarget target, object replacement, ObjectMode modes) {
        Op op = Op.Of();
        return (
                Optional(target).ToFin(Fail: op.InvalidInput()).ToValidation(),
                GeometryIntake.Of(source: replacement, key: op).ToValidation(),
                Optional(modes).ToFin(Fail: op.InvalidInput()).ToValidation())
            .Apply(static (address, geometry, policy) =>
                (TableOp)new ReplaceCase(Target: address, Replacement: geometry, Modes: policy))
            .As()
            .ToFin();
    }

    public static Fin<TableOp> Delete(TableTarget target, Notice notice, ObjectMode modes) =>
        Admitted(first: target, second: notice, third: modes, mint: static (address, reporting, policy) =>
            new DeleteCase(Target: address, Notice: reporting, Modes: policy));

    public static Fin<TableOp> Transform(TableTarget target, Transform motion, TransformPolicy policy) {
        Op op = Op.Of();
        return (
                Optional(target).ToFin(Fail: op.InvalidInput()).ToValidation(),
                op.AcceptInput(value: motion).ToValidation(),
                Optional(policy).ToFin(Fail: op.InvalidInput()).ToValidation())
            .Apply(static (address, transform, mode) => (TableOp)new TransformCase(
                Target: address,
                Motion: transform,
                Policy: mode))
            .As()
            .ToFin();
    }

    public static Fin<TableOp> Amend(TableTarget target, Func<ObjectAttributes, Fin<Unit>> change, Notice notice) =>
        Admitted(first: target, second: change, third: notice, mint: static (address, revise, reporting) =>
            new AmendCase(Target: address, Change: revise, Notice: reporting));

    public static Fin<TableOp> Select(TableTarget target, SelectionEdit edit, SelectionPolicy policy) =>
        Admitted(first: target, second: edit, third: policy, mint: static (address, mutation, admitted) =>
            new SelectCase(Target: address, Edit: mutation, Policy: admitted));

    public static Fin<TableOp> State(TableTarget target, ObjectState state, ObjectMode modes) =>
        Admitted(first: target, second: state, third: modes, mint: static (address, mutation, policy) =>
            new StateCase(Target: address, State: mutation, Modes: policy));

    public static Fin<TableOp> ClearSelection(SelectionClear scope) =>
        Optional(scope).ToFin(Fail: Op.Of().InvalidInput()).Map(static value => (TableOp)new ClearSelectionCase(Scope: value));

    public static Fin<TableOp> Flash(TableTarget target, FlashMode mode) =>
        Admitted(first: target, second: mode, mint: static (address, display) =>
            new FlashCase(Target: address, Mode: display));

    private static Fin<TableOp> Admitted<T1, T2>(T1 first, T2 second, Func<T1, T2, TableOp> mint)
        where T1 : class where T2 : class {
        Op op = Op.Of();
        return (
                Optional(first).ToFin(Fail: op.InvalidInput()).ToValidation(),
                Optional(second).ToFin(Fail: op.InvalidInput()).ToValidation())
            .Apply(mint)
            .As()
            .ToFin();
    }

    private static Fin<TableOp> Admitted<T1, T2, T3>(T1 first, T2 second, T3 third, Func<T1, T2, T3, TableOp> mint)
        where T1 : class where T2 : class where T3 : class {
        Op op = Op.Of();
        return (
                Optional(first).ToFin(Fail: op.InvalidInput()).ToValidation(),
                Optional(second).ToFin(Fail: op.InvalidInput()).ToValidation(),
                Optional(third).ToFin(Fail: op.InvalidInput()).ToValidation())
            .Apply(mint)
            .As()
            .ToFin();
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
        return (
                Optional(custody).ToFin(Fail: op.InvalidInput()).ToValidation(),
                guard(
                    box.Count is 8
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

    public static Fin<TableOp> Rebind(TableTarget target, int definitionIndex) {
        Op op = Op.Of();
        return (
                Optional(target).ToFin(Fail: op.InvalidInput()).ToValidation(),
                guard(definitionIndex >= 0, op.InvalidInput()).ToFin().ToValidation())
            .Apply((address, _) => (TableOp)new RebindCase(
                Target: address,
                DefinitionIndex: definitionIndex))
            .As()
            .ToFin();
    }

    public static Fin<TableOp> Reclaim(TableKind kind) =>
        Optional(kind).ToFin(Fail: Op.Of().InvalidInput()).Map(static value => (TableOp)new ReclaimCase(Kind: value));

    public static Fin<TableOp> ImportPage(DocumentPath path, Guid mainViewportId, string pageName) {
        Op op = Op.Of();
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

    public static Fin<TableOp> RestoreView(NamedRestore restore) =>
        Optional(restore).ToFin(Fail: Op.Of().InvalidInput()).Map(static value => (TableOp)new RestoreViewCase(Restore: value));

    public static Fin<TableOp> Roll(HistoryRoll navigation) =>
        Optional(navigation).ToFin(Fail: Op.Of().InvalidInput()).Map(static value => (TableOp)new RollCase(Navigation: value));

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
                from ids in edit.Sources.TraverseM(source => source.Admit(domain: model, key: context.Op)
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
                from _ in edit.Replacement.Admit(domain: model, key: context.Op)
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
            transformCase: static (context, edit) => Mapped(
                document: context.Document,
                target: edit.Target,
                slot: edit.Policy.Slot,
                step: id => context.Op.AcceptValue(value: edit.Policy.Apply(table: context.Document.Objects, id: id, motion: edit.Motion)),
                op: context.Op),
            amendCase: static (context, edit) => Mapped(
                document: context.Document,
                target: edit.Target,
                slot: TableSlot.Amended,
                step: id =>
                    from native in Optional(context.Document.Objects.FindId(id)).ToFin(Fail: context.Op.InvalidResult())
                    from attributes in Optional(native.Attributes?.Duplicate()).ToFin(Fail: context.Op.InvalidResult())
                    from _ in new Lease<ObjectAttributes>.Owned(Value: attributes).Use(owned =>
                        from __ in edit.Change(arg: owned)
                        from ___ in context.Op.Confirm(success: context.Document.Objects.ModifyAttributes(
                            objectId: id,
                            newAttributes: owned,
                            quiet: edit.Notice.SuppressesWarnings))
                        select unit)
                    select id,
                op: context.Op),
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
            rebindCase: static (context, edit) => Mapped(
                document: context.Document,
                target: edit.Target,
                slot: TableSlot.Replaced,
                step: id => context.Op.Confirm(success: context.Document.Objects.ReplaceInstanceObject(objectId: id, instanceDefinitionIndex: edit.DefinitionIndex)).Map(_ => id),
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

    private static Fin<TableReceipt> SelectionSpan(RhinoDoc document, Func<int> apply, Op op) =>
        from before in Tables.Selected(document: document, key: op)
        from _ in guard(apply() >= 0, op.InvalidResult()).ToFin()
        from after in Tables.Selected(document: document, key: op)
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

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref SelectionHighlight highlight,
        ref SelectionGrips grips,
        ref SelectionPersistence persistence,
        ref SelectionLayerLocks layerLocks,
        ref SelectionLayerVisibility layerVisibility) =>
        validationError = highlight is null
            || grips is null
            || persistence is null
            || layerLocks is null
            || layerVisibility is null
            ? new ValidationError(message: "Selection policy is incomplete.")
            : null;

    public static Fin<SelectionPolicy> Of(
        Option<SelectionHighlight> highlight = default,
        Option<SelectionGrips> grips = default,
        Option<SelectionPersistence> persistence = default,
        Option<SelectionLayerLocks> layerLocks = default,
        Option<SelectionLayerVisibility> layerVisibility = default,
        Op? key = null) {
        return Admission.Admitted(
            fault: Validate(
                highlight.IfNone(SelectionHighlight.Synchronize),
                grips.IfNone(SelectionGrips.Ignore),
                persistence.IfNone(SelectionPersistence.Persistent),
                layerLocks.IfNone(SelectionLayerLocks.Respect),
                layerVisibility.IfNone(SelectionLayerVisibility.Respect),
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

    public static Fin<TableCustomUndo> Of(string name, EventHandler<CustomUndoEventArgs> handler, Option<object> tag = default) {
        Op op = Op.Of();
        return (
                op.AcceptText(value: name).ToValidation(),
                Optional(handler).ToFin(Fail: op.InvalidInput()).ToValidation())
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
        Op op = Op.Of();
        return from admitted in (
                   op.AcceptText(value: name).ToValidation(),
                   Admit(redraw: redraw, operations: operations, op: op).ToValidation(),
                   customUndo
                       .Traverse(item => Optional(item).ToFin(Fail: op.InvalidInput()).ToValidation())
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

    public static Fin<TableTransaction> Immediate(RedrawPolicy redraw, params ReadOnlySpan<TableOp> operations) {
        Op op = Op.Of();
        return from plan in Admit(redraw: redraw, operations: operations, op: op)
               from _ in guard(
                   plan.Operations.Count is 1
                   && plan.Operations.ForAll(static operation => operation.Traits is { RecordsUndo: false, Navigates: false }),
                   op.InvalidInput()).ToFin()
               select (TableTransaction)new ImmediateCase(Operations: plan.Operations, Redraw: plan.Redraw);
    }

    public static Fin<TableTransaction> Navigate(HistoryRoll navigation, RedrawPolicy redraw) {
        Op op = Op.Of();
        return (
                TableOp.Roll(navigation: navigation).ToValidation(),
                Optional(redraw).ToFin(Fail: op.InvalidInput()).ToValidation())
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
            Optional(redraw).ToFin(Fail: op.InvalidInput()).ToValidation(),
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
               from fold in Optional(project).ToFin(Fail: op.InvalidInput())
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

    internal static Fin<Seq<ObjectRuntime>> Selected(RhinoDoc document, Op key) =>
        Optional(document.Objects.GetSelectedObjects(includeLights: true, includeGrips: true))
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
                        redrawDocument: false,
                        redrawLayers: false))
                    .Bind(_ => body());
            } finally {
                restored = Op.SideWhen(redraw.Suppress, () => document.Views.EnableRedraw(
                    enable: prior,
                    redrawDocument: false,
                    redrawLayers: false));
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
            : from fold in Optional(stamp).ToFin(Fail: key.InvalidInput())
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

- Owner: `TableSlot` `[SmartEnum<int>]` names object consequences. `TableFact` `[Union]` carries object runtime evidence, component-table tallies, named-view restores, history navigation, undo serials, and custom-undo names without a second payload discriminator. `TableReceipt` is the additive fold over that one fact stream.
- Entry: `Ids(TableSlot, Op?)` and `Runtime(TableSlot, Op?)` fail closed on an invalid slot and project object consequences; `Components`, `Restores`, `History`, `UndoRecords`, and `CustomUndoNames` project the remaining fact cases. A receipt can feed its deleted runtime rows directly into `TableTarget.Deleted`.
- Law: `UndoBracket` is receipt-agnostic: every folder commit rail — table, layer, session-regime, annotation draft, block, object, render content, render settings, exchange, sheet, preset, user-text, capture-adopt — folds the sealed serial into its own receipt through `DocumentCommit.Sealed` without a foreign-receipt hop. `Stamper` and the railed projection execute inside the bracket before sealing, so a stamp or projection fault remains rollback-capable.
- Law: `Seal` owns bounded close recovery and the terminal rollback decision. `Execution × Closure` is one total tuple switch: success requires a fault-free close, recovered close faults fail successful execution, failed execution rolls back after recovered close, and unrecoverable close reports rollback as unexecuted. `Dispose` cannot re-enter close after any seal attempt.
- Law: fact construction remains internal to the receipt. Generated `ObjectRuntime` identity, component-kind presence, nonnegative tallies, and the commit envelope's positive-serial proof guard every fact entering the public stream; invalid evidence fails instead of disappearing as an empty receipt.

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
    internal sealed record RestoreCase(NamedRestore Value) : TableFact;
    internal sealed record HistoryCase(HistoryRoll Value) : TableFact;
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
        new(facts: ObjectRuntime.Canonical(values: values)
            .Map(value => (TableFact)new TableFact.ObjectCase(Slot: slot, Value: value)));

    internal static Fin<TableReceipt> Component(TableKind kind, int tally, Op key) =>
        from admitted in Optional(kind).ToFin(Fail: key.InvalidInput())
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
|  [04]   | mutation program       | `TableOp`                        | admitted total union       | operation factories / `Apply`         |
|  [05]   | commit scope           | `TableTransaction`               | program-mode union         | `Recorded` / `Immediate` / `Navigate` |
|  [06]   | resource ingress       | `GeometryIntake`                 | native/value custody union | `Admit`                               |
|  [07]   | table commit spine     | `Tables`                         | session/redraw fold        | `Commit`                              |
|  [08]   | shared commit envelope | `DocumentCommit` / `UndoBracket` | receipt-generic terminal   | `Sealed` / `Compensated` / `Seal`     |
|  [09]   | consequence evidence   | `TableReceipt`                   | runtime fact stream        | typed projections                     |
|  [10]   | component addressing   | `ResourceRef` / `ResourceLens`   | id/name/index over a lens  | `Of` / `Resolve(document, lens, key)` |
|  [11]   | redraw bracket         | `RedrawScope`                    | suppress/restore/flush     | `Within(document, redraw, body, key)` |
|  [12]   | viewport addressing    | `ViewportTarget`                 | address & census union     | `Active` / `ResolveViewport`          |
