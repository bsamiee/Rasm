# [RASM_RHINO_BLOCK_MODEL]

The block state vocabulary (`Rasm.Rhino.Blocks`). One `BlockRef` union addresses a definition by id, name, or table index through one resolution fold; one `BlockSnapshot` reads the definition's whole state — identity, update and archive discriminants, layer style, member roster, usage topology — in a single pass; and the reference-scope, conflict, deletion, explode, and preview policies are compact value rows the operation rail consumes. Native enum adaptation stays at this edge: the host update-type, archive-status, and layer-style discriminants ride the snapshot as seam values, one derived `LinkState` fold over (update × status × source path) is the interior vocabulary, the census-era per-native-enum wrapper types are dead, and the wrapper-identifier roster (`DefinitionId`/`DefinitionIndex`/`DefinitionName`/`DefinitionPrefix`/`ArchivePath` and kin) collapses into the one address union. Change detection composes the document geometry probe — `GeometryCrc` chained over the member roster — and federation identity is the kernel `ContentHash` over canonical bytes; the census-era local FNV hasher is dead.

## [01]-[INDEX]

- [02]-[ADDRESS]: `BlockRef` — the one definition address union and its resolution fold.
- [03]-[SNAPSHOT]: `ReferenceScope`, `SourceHealth`, the derived `LinkState` fold, `BlockUsage`, and the one `BlockSnapshot` read product with the `BlockStamp` change probe.
- [04]-[POLICY_ROWS]: `ConflictPolicy`, `DeletionPolicy`, `ExplodePolicy`, `Placement`, and the `PreviewSpec` render request.
- [05]-[SURFACE_LEDGER]: the page's owner table.

## [02]-[ADDRESS]

- Owner: `BlockRef` `[Union]` — `ById` over the definition guid, `ByName` over the validated definition name, `ByIndex` over the table index; one `Resolve` fold answers the live `InstanceDefinition` and every arm treats deleted definitions as absent.
- Law: the address is the package's one definition identity — receipts, graph vertices, and lifecycle keys carry the guid, prose-facing surfaces carry the name, and the table index appears only where the host answers one; a wrapper struct per identity form is the deleted shape.
- Law: resolution reads live per call — the table mutates under commands and linked refresh, so no resolved handle is cached on a value; a consumer holding a `BlockRef` re-resolves at each use inside the owning operation.
- Boundary: `Resolve` is the only site naming the table `Find` members and the roster scan; every sibling page addresses through this union.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BlockRef {
    private BlockRef() { }
    public sealed record ById(Guid Value) : BlockRef;
    public sealed record ByName(string Value) : BlockRef;
    public sealed record ByIndex(int Value) : BlockRef;

    public static Fin<BlockRef> Of(Guid id) =>
        id != Guid.Empty
            ? Fin.Succ<BlockRef>(value: new ById(Value: id))
            : Fin.Fail<BlockRef>(error: Op.Of(name: nameof(BlockRef)).InvalidInput());

    public static Fin<BlockRef> Of(string name) =>
        Op.Of(name: nameof(BlockRef)).AcceptText(value: name).Map(static valid => (BlockRef)new ByName(Value: valid));

    public static Fin<BlockRef> Of(int index) =>
        index >= 0
            ? Fin.Succ<BlockRef>(value: new ByIndex(Value: index))
            : Fin.Fail<BlockRef>(error: Op.Of(name: nameof(BlockRef)).InvalidInput());

    internal Fin<InstanceDefinition> Resolve(RhinoDoc document, Op key) =>
        Switch(
            state: (Document: document, Op: key),
            byId: static (ctx, address) => Optional(ctx.Document.InstanceDefinitions.Find(
                instanceId: address.Value, ignoreDeletedInstanceDefinitions: true)).ToFin(Fail: ctx.Op.MissingContext()),
            byName: static (ctx, address) => Optional(ctx.Document.InstanceDefinitions.Find(address.Value))
                .ToFin(Fail: ctx.Op.MissingContext()),
            byIndex: static (ctx, address) => ctx.Op.Catch(() =>
                Optional(ctx.Document.InstanceDefinitions[address.Value]).ToFin(Fail: ctx.Op.MissingContext())));
}
```

## [03]-[SNAPSHOT]

- Owner: `ReferenceScope` `[SmartEnum<int>]` — the host `wheretoLook` axis as named rows whose key IS the native argument: `Direct` the top-level document placements, `Nested` adding placements inside other definitions, `Worksession` adding reference-document placements. `SourceHealth` `[SmartEnum<int>]` — the linked-archive availability vocabulary over the verified native status roster. `LinkState` `[Union]` — the ONE derived fold over (update type × archive status × source path): `Static` and `Linked` with its path, embed grant, and health; the host's `Embedded` update row is obsolete and normalizes to `Static`, so a separate embedded case is unconstructible. `BlockUsage` — the split reference tally. `BlockSnapshot` — the one definition read product. `BlockStamp` — the in-process change probe chaining the document `GeometryCrc` over the member geometry roster.
- Law: usage is queried, never assumed — the snapshot resolves `UseCount` with its top-level and nested split, `InUse` under the asked scope, and the container set before any delete or purge decision reads it; the layer, linetype, and nesting probes (`UsesLayer`, `UsesLinetype`, `UsesDefinition`) are read directly off the resolved definition at the deciding site, and a consumer counting references by scanning objects re-derives what the host already answers.
- Law: the native discriminants stop here — `InstanceDefinitionUpdateType`, `InstanceDefinitionArchiveFileStatus`, and `InstanceDefinitionLayerStyle` ride the snapshot as seam values so evidence is never dropped, and interior branching reads `LinkState` — the one derived vocabulary — never a re-derivation of the product; a wrapper row type per native enum is the deleted form.
- Law: a linked state demands its source — `LinkState.Of` refuses a linked update type whose source archive is blank, because every linked-source transition on the operation rail addresses that path; the health rows split not-found from not-readable from the three drift directions so refresh policy dispatches on the row.
- Law: identity is two probes with distinct scopes — `BlockStamp` answers "did this definition's geometry change inside this process" for cache and preview invalidation, and federation identity composes the kernel `ContentHash` over the kernel's canonical byte encode; a host CRC persisted as durable identity is the deleted form because the CRC is host-version-local.
- Growth: a new definition fact is one snapshot field read in the same pass; a new scope is one `ReferenceScope` row; a new availability state is one `SourceHealth` row.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class ReferenceScope {
    public static readonly ReferenceScope Direct = new(key: 0);
    public static readonly ReferenceScope Nested = new(key: 1);
    public static readonly ReferenceScope Worksession = new(key: 2);
}

[SmartEnum<int>]
public sealed partial class SourceHealth {
    public static readonly SourceHealth Current = new(key: 0, stale: false, broken: false);
    public static readonly SourceHealth Newer = new(key: 1, stale: true, broken: false);
    public static readonly SourceHealth Older = new(key: 2, stale: true, broken: false);
    public static readonly SourceHealth Different = new(key: 3, stale: true, broken: false);
    public static readonly SourceHealth NotFound = new(key: 4, stale: false, broken: true);
    public static readonly SourceHealth Unreadable = new(key: 5, stale: false, broken: true);

    public bool Stale { get; }
    public bool Broken { get; }

    internal static Option<SourceHealth> Of(InstanceDefinitionArchiveFileStatus status) =>
        status switch {
            InstanceDefinitionArchiveFileStatus.LinkedFileIsUpToDate => Some(Current),
            InstanceDefinitionArchiveFileStatus.LinkedFileIsNewer => Some(Newer),
            InstanceDefinitionArchiveFileStatus.LinkedFileIsOlder => Some(Older),
            InstanceDefinitionArchiveFileStatus.LinkedFileIsDifferent => Some(Different),
            InstanceDefinitionArchiveFileStatus.LinkedFileNotFound => Some(NotFound),
            InstanceDefinitionArchiveFileStatus.LinkedFileNotReadable => Some(Unreadable),
            _ => Option<SourceHealth>.None,
        };
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LinkState {
    private LinkState() { }
    public sealed record Static : LinkState;
    public sealed record Linked(string Path, bool AlsoEmbedded, SourceHealth Health) : LinkState;

    internal static Fin<LinkState> Of(
        InstanceDefinitionUpdateType update,
        InstanceDefinitionArchiveFileStatus status,
        Option<string> source,
        Op key) =>
        update switch {
            InstanceDefinitionUpdateType.Static => Fin.Succ<LinkState>(value: new Static()),
            InstanceDefinitionUpdateType.Linked or InstanceDefinitionUpdateType.LinkedAndEmbedded =>
                from path in source.ToFin(Fail: key.InvalidResult(detail: nameof(InstanceDefinition.SourceArchive)))
                from health in SourceHealth.Of(status: status).ToFin(Fail: key.InvalidResult(detail: status.ToString()))
                select (LinkState)new Linked(
                    Path: path,
                    AlsoEmbedded: update is InstanceDefinitionUpdateType.LinkedAndEmbedded,
                    Health: health),
            var legacy => Fin.Fail<LinkState>(error: key.InvalidResult(detail: legacy.ToString())),
        };
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct BlockUsage(int Total, int TopLevel, int Nested);

public sealed record BlockSnapshot(
    Guid Key,
    int Index,
    string Name,
    Option<string> Description,
    InstanceDefinitionUpdateType UpdateType,
    InstanceDefinitionArchiveFileStatus ArchiveStatus,
    InstanceDefinitionLayerStyle LayerStyle,
    LinkState Link,
    int ObjectCount,
    Seq<Guid> MemberIds,
    BlockUsage Usage,
    bool InUse,
    Seq<int> ContainerIndexes) {
    public static Fin<BlockSnapshot> Of(InstanceDefinition definition, ReferenceScope scope, Op key) =>
        Optional(definition).ToFin(Fail: key.InvalidInput()).Bind(active => key.Catch(() => {
            int total = active.UseCount(topLevelReferenceCount: out int topLevel, nestedReferenceCount: out int nested);
            Option<string> source = Optional(active.SourceArchive).Filter(static path => path.Length > 0);
            return LinkState.Of(update: active.UpdateType, status: active.ArchiveFileStatus, source: source, key: key)
                .Map(link => new BlockSnapshot(
                    Key: active.Id,
                    Index: active.Index,
                    Name: active.Name,
                    Description: Optional(active.Description).Filter(static text => text.Length > 0),
                    UpdateType: active.UpdateType,
                    ArchiveStatus: active.ArchiveFileStatus,
                    LayerStyle: active.LayerStyle,
                    Link: link,
                    ObjectCount: active.ObjectCount,
                    MemberIds: toSeq(active.GetObjects()).Map(static member => member.Id),
                    Usage: new BlockUsage(Total: total, TopLevel: topLevel, Nested: nested),
                    InUse: active.InUse(wheretoLook: (int)scope),
                    ContainerIndexes: toSeq(active.GetContainers()).Map(static container => container.Index)));
        }));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class BlockStamp {
    public static Fin<GeometryCrc> Of(InstanceDefinition definition, Op key) =>
        Optional(definition).ToFin(Fail: key.InvalidInput()).Bind(active => key.Catch(() =>
            Fin.Succ(value: GeometryCrc.Create(value: toSeq(active.GetObjects())
                .Choose(static member => Optional(member.Geometry))
                .Fold(0u, static (chain, geometry) => geometry.DataCRC(currentRemainder: chain))))));
}
```

## [04]-[POLICY_ROWS]

- Owner: `ConflictPolicy` `[SmartEnum<int>]` — the authoring collision decision: `Fail` refuses an existing name, `Reuse` returns the existing definition, `Mint` derives an unused name through the host name minter; `DeletionPolicy` — the reference and reporting decision a delete carries; `ExplodePolicy` — nesting depth and the viewport-scoped hidden-piece skip as one value; `Placement` — one validated instance placement: transform, attributes, history, reference grant; `PreviewSpec` `[Union]` — the two verified render requests: the standard projection render and the axonometric render with its display-mode id, camera, and decoration grant.
- Law: a policy arrives pre-constructed and carries the whole decision — the operation rail dispatches the value it was handed and reconstructs nothing; a boolean tail re-deriving conflict or explode behavior at a call site is the deleted form.
- Law: a placement admits only a valid transform — the factory refuses an invalid motion so the host placement member never sees one, and the reference grant is placement data because reference objects enter worksessions differently.
- Law: the preview request discriminates the host overload — the two `CreatePreviewBitmap` signatures are two cases of one request, so a caller states what it wants and never selects an overload; the rendered product crosses as an owned lease, never a bare bitmap field.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class ConflictPolicy {
    public static readonly ConflictPolicy Fail = new(key: 0);
    public static readonly ConflictPolicy Reuse = new(key: 1);
    public static readonly ConflictPolicy Mint = new(key: 2);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PreviewSpec {
    private PreviewSpec() { }
    public sealed record Standard(
        DefinedViewportProjection Projection,
        DisplayMode Mode,
        System.Drawing.Size Size,
        bool DpiScaling = true) : PreviewSpec;
    public sealed record Axonometric(
        Guid DisplayModeId,
        DefinedViewportProjection Projection,
        IsometricCamera Camera,
        System.Drawing.Size Size,
        bool Decorations = false,
        bool DpiScaling = true) : PreviewSpec;

    internal Fin<System.Drawing.Bitmap> Render(InstanceDefinition definition, Op key) =>
        Switch(
            state: (Definition: definition, Op: key),
            standard: static (ctx, spec) => ctx.Op.Catch(() => Optional(ctx.Definition.CreatePreviewBitmap(
                definedViewportProjection: spec.Projection, displayMode: spec.Mode,
                bitmapSize: spec.Size, applyDpiScaling: spec.DpiScaling)).ToFin(Fail: ctx.Op.InvalidResult())),
            axonometric: static (ctx, spec) => ctx.Op.Catch(() => Optional(ctx.Definition.CreatePreviewBitmap(
                displayModeId: spec.DisplayModeId, viewportProjection: spec.Projection, isometricCamera: spec.Camera,
                drawDecorations: spec.Decorations, bitmapSize: spec.Size, applyDpiScaling: spec.DpiScaling)).ToFin(Fail: ctx.Op.InvalidResult())));
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct DeletionPolicy(bool DeleteReferences = true, bool Quiet = true);

public readonly record struct ExplodePolicy(bool Nested = false, Option<Guid> SkipHiddenInViewport = default);

public sealed record Placement(
    Transform Motion,
    Option<ObjectAttributes> Attributes = default,
    Option<HistoryRecord> History = default,
    bool Reference = false) {
    public static Fin<Placement> Of(
        Transform motion,
        Option<ObjectAttributes> attributes = default,
        Option<HistoryRecord> history = default,
        bool reference = false) =>
        motion.IsValid
            ? Fin.Succ(value: new Placement(Motion: motion, Attributes: attributes, History: history, Reference: reference))
            : Fin.Fail<Placement>(error: Op.Of(name: nameof(Placement)).InvalidInput());
}
```

## [05]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]           | [OWNER]          | [FORM]                                            | [ENTRY]                        |
| :-----: | :------------------ | :--------------- | :------------------------------------------------- | :------------------------------ |
|  [01]   | definition address  | `BlockRef`       | one union: id, name, index                          | `Of` / `Resolve`                 |
|  [02]   | reference scope     | `ReferenceScope` | rows whose key is the native `wheretoLook`          | `(int)scope`                     |
|  [03]   | definition state    | `BlockSnapshot`  | one-pass read, native discriminants at the seam     | `Of(definition, scope, key)`     |
|  [04]   | link vocabulary     | `LinkState`      | one derived fold over update × status × source      | `BlockSnapshot.Link`             |
|  [05]   | change probe        | `BlockStamp`     | chained `GeometryCrc` over the member roster        | `Of(definition, key)`            |
|  [06]   | operation policies  | `ConflictPolicy` | conflict, deletion, explode, placement rows         | operation rail payloads          |
|  [07]   | preview request     | `PreviewSpec`    | two verified render overloads as one union          | `Render(definition, key)`        |
