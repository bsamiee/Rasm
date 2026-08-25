# [RASM_RHINO_BLOCK_MODEL]

Block state vocabulary (`Rasm.Rhino.Blocks`) owns one live address, one whole-state projection, one dependency request, and closed policy values for every mutation and preview seam. Native discriminants enter once, `LinkState` carries their interior meaning, `BlockStamp` separates process-local geometry change from federation content identity, and every sibling re-resolves the Document-owned `ResourceRef` through `Definitions.Lens` inside its document window. The link-condition algebra seats here: `LinkCondition` names what a linked source must hold, `SourceMode` declares which conditions each source axis REQUIRES as a capability set, and the lifecycle rail reads `Regenerates` as one subset test instead of re-deriving a ternary from two bools.

## [01]-[INDEX]

- [02]-[ADDRESS]: `Definitions.Lens` this folder's one live `ResourceLens<InstanceDefinition>` row over the document spine's `ResourceRef`, `Definitions.DeletedLens` its deleted-roster posture twin, resolution re-entering the mutable table per use so no native definition escapes its owning document window.
- [03]-[SNAPSHOT]: `BlockSnapshot` capturing identity, normalized source state, member order, scoped placement evidence, usage, containers, and both change probes in one host read, `BlockDependency`/`BlockDependencyAnswer` folding the native table probes onto a typed answer, `SourceFacet`/`SourceMode`, `LinkCondition`, `ArchiveCondition`/`SourceHealth`, and `LayerScope` closing the host source, condition, health, and layer discriminants at the boundary, and `LinkState` carrying their interior meaning.
- [04]-[POLICY_VALUES]: closed `ConflictPolicy`, `DeletionPolicy`, `ExplodePolicy`, `Placement`, and `BlockPreview` owners carrying host arguments as data, `PreviewDisplay` admitting the display host enum once beside the Document-owned `DefinedView`/`IsoQuadrant` projection rosters this page composes, `PreviewBudget` the allocation policy row, and `PreviewFrame` admitting projection, extent, and raster scale once for every modality.
- [05]-[SURFACE_LEDGER]: owner-to-kind-to-ingress-to-egress roster across the lens rows, snapshot, link state, policy owners, and preview union.

## [02]-[ADDRESS]

- Owner: `Definitions` — the folder's two `ResourceLens<InstanceDefinition>` postures over the document spine's `ResourceRef`: `Lens` resolves live entries and rejects deleted or invalid rows, `DeletedLens` resolves the deleted roster alone for the revive path.
- Law: the spine's `ResourceRef` is the ONLY definition address, resolution re-enters the mutable table per use, and no native `InstanceDefinition` escapes its owning document window — a folder-local address union beside the lens rows is the deleted form.
- Law: the deleted posture is a LENS ROW, never a hand roster walk beside the live lens — the same three address shapes resolve under both postures, so an operation choosing "live" or "deleted" passes a lens and re-spells nothing.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
public static class Definitions {
    internal static readonly ResourceLens<InstanceDefinition> Lens = new(
        ById: static (document, id) => document.InstanceDefinitions.Find(instanceId: id, ignoreDeletedInstanceDefinitions: true),
        ByName: static (document, name) => document.InstanceDefinitions.Find(name) is { IsDeleted: false } named ? named : null,
        ByIndex: static (document, index) => index >= 0 && index < document.InstanceDefinitions.Count
            && document.InstanceDefinitions[index] is { IsDeleted: false } row ? row : null);

    internal static readonly ResourceLens<InstanceDefinition> DeletedLens = new(
        ById: static (document, id) => Roster(document).Find(definition => definition.Id == id).ToNullable(),
        ByName: static (document, name) => Roster(document)
            .Find(definition => string.Equals(definition.Name, name, StringComparison.OrdinalIgnoreCase)).ToNullable(),
        ByIndex: static (document, index) => Roster(document).Find(definition => definition.Index == index).ToNullable());

    internal static Fin<InstanceDefinition> Resolve(ResourceRef target, RhinoDoc document, Op key) =>
        target.Resolve(document: document, lens: Lens, key: key);

    private static Seq<InstanceDefinition> Roster(RhinoDoc document) =>
        toSeq(document.InstanceDefinitions.GetList(ignoreDeleted: false))
            .Choose(static definition => Optional(definition))
            .Filter(static definition => definition.IsDeleted);
}
```

## [03]-[SNAPSHOT]

- Owner: `BlockSnapshot` resolves its `ResourceRef` through `Definitions.Lens`, then captures identity, normalized source state, member order, the reference scope its placement census ran under, placed-reference evidence, usage, containers, and both change probes in one host read; `SourceFacet` is the source-axis capability vocabulary and `SourceMode` its keyed boundary owner carrying facets, required link conditions, and the host ordinal; `LinkCondition` names what a linked source must HOLD; `ArchiveCondition` is the archive-health capability vocabulary and `SourceHealth` its keyed owner; `LayerScope` closes the linked-definition layer policy; `LinkState` carries the interior meaning; `BlockDependency`/`BlockDependencyAnswer` fold the native probes onto typed answers.
- Law: `SourceMode` declares its behaviour as SETS — `Facets` states whether the mode reads a source and embeds a copy, `Requires` states which `LinkCondition` rows a regeneration demands — and `Regenerates(held)` is ONE subset read (`held.AdmitsAll(Requires)`), so the lifecycle rail's refusal DERIVES from the missing condition and the two derived bools plus the ternary over their product are the deleted forms. The retired host ordinal `1` folds onto `Static` at admission so a legacy archive reads without the fence ever spelling the `[Obsolete]` host case.
- Law: `SourceHealth` rows carry ONE `Condition` set over `ArchiveCondition` — a stale link and a broken link are capability rows, not two bool columns — and the vocabulary's absent row (`NotALinkedInstanceDefinition`) stays ABSENCE at the consumer: an unlinked definition has no source health.
- Law: `Placements` and `InUse` answer only within `Scope`, while `Usage` carries the host's own scope-free tally, so a census reading empty under one scope is never confused with a definition nothing places; `InUse` DERIVES from the placement roster and is never stored. Containers carry stable ids, matching the topology rail's own vertex spelling. Member admission revalidates geometry and attributes with native diagnostic evidence before either enters identity.
- Law: `BlockDependency` admits table indexes against their live bounds before one table fold composes the native dependency probes, and answers `BlockDependencyAnswer` — presence for a table probe, nesting depth for a definition probe — because one integer standing for both makes a depth of one indistinguishable from a boolean true.
- Law: every host read on this page runs inside `DocumentSession.Demand`, which resolves on the host command thread — that demand IS the affinity rail the thread-affine members (`GetReferences`, every `CreatePreviewBitmap` overload) require, and composing one outside a demand is the deleted form.
- Law: `BlockStamp.Geometry` remains an in-process invalidation probe; `BlockStamp.Content` is the federation content key minted through the kernel `CanonicalWriter` — STORED definition fields and the admitted member payloads frame canonically, the archive is a PRODUCT of the stamp and never its preimage, and live-state axes (archive health, tenuous resolution) stay snapshot columns declared derived. NAMED LOSS: the prior hand preimage framed big-endian; the kernel writer frames little-endian with int32-LE string length prefixes, so the identity RE-KEYS ONCE at this landing and every stored stamp re-mints — stated here, never re-derived per consumer. Witness: `Identity` is one `ContentHash.Of<TState>` call over chained writer rows where four hand `Write` overloads, an `ArrayPoolBufferWriter`, and a `File3dm.ToByteArray()` round-trip stood.
- Law: member content frames as ROWS — ordinal, the geometry's host `DataCRC` remainder, the attribute set's host `DataCRC` remainder — count-framed by the writer's own `Rows`, so member order is identity, no live `Guid` enters the preimage, and the archive-minted ordinal ids delete with the archive round-trip.
- Boundary: `GeometryCrc` chains the host remainder for the process-local probe and never crosses a boundary; the kernel `ContentHash` is the ONLY federation identity (`Document/geometry.md` states the custody split).
- Packages: RhinoCommon blocks (`.api/api-rhinocommon-blocks.md` — `InstanceDefinition`, `GetReferences`, `UseCount`, `GetContainers`, `UsesLayer`/`UsesLinetype`/`UsesDefinition`), RhinoCommon document (`.api/api-rhinocommon-document.md` — tables the dependency probes bound against), kernel `Domain/identity` (`ContentHash.Of<TState>`, `CanonicalWriter`), kernel `Domain/validation` (`ICapability`, `CapabilitySet`), LanguageExt.Core, Thinktecture.Runtime.Extensions.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace Rasm.Rhino.Blocks;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class ReferenceScope {
    public static readonly ReferenceScope Direct = new(key: 0, hostValue: 0);
    public static readonly ReferenceScope Nested = new(key: 1, hostValue: 1);
    public static readonly ReferenceScope Definition = new(key: 2, hostValue: 2);

    internal int HostValue { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SourceFacet : ICapability<SourceFacet> {
    public static readonly SourceFacet Reads = new(key: "reads");
    public static readonly SourceFacet Embeds = new(key: "embeds");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LinkCondition : ICapability<LinkCondition> {
    public static readonly LinkCondition Styled = new(key: "styled");
    public static readonly LinkCondition Readable = new(key: "readable");
}

[SmartEnum<int>]
public sealed partial class SourceMode {
    public static readonly SourceMode Static = new(
        key: (int)InstanceDefinitionUpdateType.Static,
        facets: CapabilitySet<SourceFacet>.Of(),
        requires: CapabilitySet<LinkCondition>.Of());
    public static readonly SourceMode LinkedAndEmbedded = new(
        key: (int)InstanceDefinitionUpdateType.LinkedAndEmbedded,
        facets: CapabilitySet<SourceFacet>.Of(SourceFacet.Reads, SourceFacet.Embeds),
        requires: CapabilitySet<LinkCondition>.Of(LinkCondition.Styled));
    public static readonly SourceMode Linked = new(
        key: (int)InstanceDefinitionUpdateType.Linked,
        facets: CapabilitySet<SourceFacet>.Of(SourceFacet.Reads),
        requires: CapabilitySet<LinkCondition>.Of(LinkCondition.Styled, LinkCondition.Readable));

    private const int RetiredEmbeddedOrdinal = 1;

    public CapabilitySet<SourceFacet> Facets { get; }
    public CapabilitySet<LinkCondition> Requires { get; }

    internal bool Regenerates(CapabilitySet<LinkCondition> held) => held.AdmitsAll(required: Requires);

    internal static Fin<SourceMode> Of(InstanceDefinitionUpdateType update, Op key) =>
        (int)update is RetiredEmbeddedOrdinal
            ? Fin.Succ(value: Static)
            : key.Row<int, SourceMode>((int)update);
}

[SmartEnum<int>]
public sealed partial class LayerScope {
    public static readonly LayerScope None = new(key: (int)InstanceDefinitionLayerStyle.None);
    public static readonly LayerScope Active = new(key: (int)InstanceDefinitionLayerStyle.Active);
    public static readonly LayerScope Reference = new(key: (int)InstanceDefinitionLayerStyle.Reference);

    internal InstanceDefinitionLayerStyle Host => (InstanceDefinitionLayerStyle)Key;

    internal static Fin<LayerScope> Of(InstanceDefinitionLayerStyle style, Op key) =>
        key.Row<int, LayerScope>((int)style);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ArchiveCondition : ICapability<ArchiveCondition> {
    public static readonly ArchiveCondition Stale = new(key: "stale");
    public static readonly ArchiveCondition Broken = new(key: "broken");
}

[SmartEnum<int>]
public sealed partial class SourceHealth {
    public static readonly SourceHealth Current = new(key: (int)InstanceDefinitionArchiveFileStatus.LinkedFileIsUpToDate, condition: CapabilitySet<ArchiveCondition>.Of());
    public static readonly SourceHealth Newer = new(key: (int)InstanceDefinitionArchiveFileStatus.LinkedFileIsNewer, condition: CapabilitySet<ArchiveCondition>.Of(ArchiveCondition.Stale));
    public static readonly SourceHealth Older = new(key: (int)InstanceDefinitionArchiveFileStatus.LinkedFileIsOlder, condition: CapabilitySet<ArchiveCondition>.Of(ArchiveCondition.Stale));
    public static readonly SourceHealth Different = new(key: (int)InstanceDefinitionArchiveFileStatus.LinkedFileIsDifferent, condition: CapabilitySet<ArchiveCondition>.Of(ArchiveCondition.Stale));
    public static readonly SourceHealth NotFound = new(key: (int)InstanceDefinitionArchiveFileStatus.LinkedFileNotFound, condition: CapabilitySet<ArchiveCondition>.Of(ArchiveCondition.Broken));
    public static readonly SourceHealth Unreadable = new(key: (int)InstanceDefinitionArchiveFileStatus.LinkedFileNotReadable, condition: CapabilitySet<ArchiveCondition>.Of(ArchiveCondition.Broken));

    public CapabilitySet<ArchiveCondition> Condition { get; }

    internal static Option<SourceHealth> Of(InstanceDefinitionArchiveFileStatus status) =>
        TryGet((int)status, out SourceHealth? found) ? Some(found) : Option<SourceHealth>.None;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LinkState {
    private LinkState() { }
    public sealed record Static : LinkState;
    public sealed record Linked(
        DocumentPath Path,
        SourceMode Mode,
        SourceHealth Health,
        LayerScope LayerStyle,
        bool Tenuous,
        bool SkipNested) : LinkState;

    internal static Fin<LinkState> Of(InstanceDefinition definition, Op key) =>
        from mode in SourceMode.Of(update: definition.UpdateType, key: key)
        from state in mode.Facets.Admits(capability: SourceFacet.Reads)
            ? from path in DocumentPath.Of(value: definition.SourceArchive, key: key)
              from health in SourceHealth.Of(status: definition.ArchiveFileStatus)
                  .ToFin(Fail: key.InvalidResult(detail: definition.ArchiveFileStatus.ToString()))
              from scope in LayerScope.Of(style: definition.LayerStyle, key: key)
              select (LinkState)new Linked(
                  Path: path,
                  Mode: mode,
                  Health: health,
                  LayerStyle: scope,
                  Tenuous: definition.IsTenuous,
                  SkipNested: definition.SkipNestedLinkedDefinitions)
            : Fin.Succ<LinkState>(value: new Static())
        select state;
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BlockDependency {
    private BlockDependency() { }
    public sealed record Layer(int Index) : BlockDependency;
    public sealed record Linetype(int Index) : BlockDependency;
    public sealed record Definition(ResourceRef Target) : BlockDependency;

    internal Fin<BlockDependencyAnswer> Measure(InstanceDefinition owner, RhinoDoc document, Op key) => Switch(
        context: (Owner: owner, Document: document, Op: key),
        layer: static (context, probe) => MeasureTable(
            index: probe.Index,
            owner: context.Owner,
            document: context.Document,
            count: static active => active.Layers.Count,
            includes: static (definition, index) => definition.UsesLayer(layerIndex: index),
            op: context.Op),
        linetype: static (context, probe) => MeasureTable(
            index: probe.Index,
            owner: context.Owner,
            document: context.Document,
            count: static active => active.Linetypes.Count,
            includes: static (definition, index) => definition.UsesLinetype(linetypeIndex: index),
            op: context.Op),
        definition: static (context, probe) => Definitions.Resolve(
                target: probe.Target,
                document: context.Document,
                key: context.Op)
            .Bind(nested => context.Op.Catch(() => Fin.Succ(
                value: (BlockDependencyAnswer)new BlockDependencyAnswer.Nesting(
                    Levels: context.Owner.UsesDefinition(otherIdefIndex: nested.Index))))));

    private static Fin<BlockDependencyAnswer> MeasureTable(
        int index,
        InstanceDefinition owner,
        RhinoDoc document,
        Func<RhinoDoc, int> count,
        Func<InstanceDefinition, int, bool> includes,
        Op op) => op.Catch(() => index >= 0 && index < count(arg: document)
        ? Fin.Succ(value: (BlockDependencyAnswer)new BlockDependencyAnswer.Uses(
            Value: includes(arg1: owner, arg2: index)))
        : Fin.Fail<BlockDependencyAnswer>(error: op.InvalidInput()));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BlockDependencyAnswer : IDetachedDocumentResult {
    private BlockDependencyAnswer() { }
    public sealed record Uses(bool Value) : BlockDependencyAnswer;
    public sealed record Nesting(int Levels) : BlockDependencyAnswer;
}

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class BlockUsage {
    public int Total { get; }
    public int TopLevel { get; }
    public int Nested { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int total,
        ref int topLevel,
        ref int nested) =>
        validationError = total >= 0 && topLevel >= 0 && nested >= 0 && total == topLevel + nested
            ? null
            : new ValidationError(message: "Block usage demands a total equal to top-level plus nested.");

    internal static Fin<BlockUsage> Of(int total, int topLevel, int nested, Op key) =>
        key.AcceptValidated<BlockUsage>(
            fault: Validate(total, topLevel, nested, out BlockUsage? admitted),
            admitted: admitted);
}

public sealed record BlockStamp(GeometryCrc Geometry, UInt128 Content);

internal sealed record BlockMemberProjection(
    Guid Id,
    GeometryBase Geometry,
    ObjectAttributes Attributes);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Placement {
    private Placement() { }
    public sealed record Bare : Placement { internal Bare(Transform motion) => Motion = motion; public Transform Motion { get; } }
    public sealed record Attributed : Placement {
        internal Attributed(Transform motion, ObjectAttributes attributes) => (Motion, Attributes) = (motion, attributes);
        public Transform Motion { get; }
        public ObjectAttributes Attributes { get; }
    }
    public sealed record Recorded : Placement {
        internal Recorded(Transform motion, ObjectAttributes attributes, Lease<HistoryRecord> history, PlacementKind kind) =>
            (Motion, Attributes, History, Kind) = (motion, attributes, history, kind);
        public Transform Motion { get; }
        public ObjectAttributes Attributes { get; }
        public Lease<HistoryRecord> History { get; }
        public PlacementKind Kind { get; }
    }

    public static Fin<Placement> Of(Transform motion, Op? key = null) =>
        Motioned(motion: motion, key.OrDefault()).Map(static admitted => (Placement)new Bare(motion: admitted));

    public static Fin<Placement> Of(Transform motion, ObjectAttributes attributes, Op? key = null) {
        Op op = key.OrDefault();
        return (Motioned(motion: motion, op).ToValidation(), op.Need(attributes).ToValidation())
            .Apply(static (admitted, held) => (Placement)new Attributed(motion: admitted, attributes: held))
            .As()
            .ToFin();
    }

    public static Fin<Placement> Of(
        Transform motion, ObjectAttributes attributes, Lease<HistoryRecord> history, PlacementKind kind, Op? key = null) {
        Op op = key.OrDefault();
        return (
                Motioned(motion: motion, op).ToValidation(),
                op.Need(attributes).ToValidation(),
                op.Need(history).ToValidation(),
                op.Need(kind).ToValidation())
            .Apply(static (admitted, held, record, posture) =>
                (Placement)new Recorded(motion: admitted, attributes: held, history: record, kind: posture))
            .As()
            .ToFin();
    }

    private static Fin<Transform> Motioned(Transform motion, Op op) =>
        guard(motion.IsValid, op.InvalidInput()).ToFin().Map(_ => motion);
}

[Equatable]
public sealed partial record BlockSnapshot(
    Guid Key,
    int Index,
    string Name,
    Option<string> Description,
    LinkState Link,
    int ObjectCount,
    [property: OrderedEquality] Seq<Guid> MemberIds,
    ReferenceScope Scope,
    [property: OrderedEquality] Seq<BlockPlacement> Placements,
    BlockUsage Usage,
    [property: OrderedEquality] Seq<Guid> ContainerIds,
    BlockStamp Stamp) {
    public bool InUse => !Placements.IsEmpty;

    public static Fin<BlockSnapshot> Of(ResourceRef target, RhinoDoc document, ReferenceScope scope, Op key) =>
        from address in key.Need(target)
        from owner in key.Need(document)
        from referenceScope in key.Need(scope)
        from active in Definitions.Resolve(target: address, document: owner, key: key)
        from snapshot in key.Catch(() => {
            Seq<RhinoObject> members = toSeq(active.GetObjects());
            Seq<InstanceObject> references = toSeq(active.GetReferences(wheretoLook: referenceScope.HostValue));
            int total = active.UseCount(topLevelReferenceCount: out int topLevel, nestedReferenceCount: out int nested);

            return from projected in members
                       .Traverse(member => (
                           Optional(member).ToFin(Fail: key.InvalidResult()).ToValidation(),
                           guard(member.Id != Guid.Empty, key.InvalidResult()).ToFin().ToValidation(),
                           Optional(member.Geometry).ToFin(Fail: key.InvalidResult()).ToValidation(),
                           Optional(member.Attributes).ToFin(Fail: key.InvalidResult()).ToValidation())
                           .Apply(static (owner, _, shape, attributes) => new BlockMemberProjection(
                               Id: owner.Id,
                               Geometry: shape,
                               Attributes: attributes))
                       .As())
                       .As()
                       .ToFin()
                   from placements in references
                       .Traverse(reference => Optional(reference)
                           .ToFin(Fail: key.InvalidResult())
                           .Bind(placed => guard(placed.Id != Guid.Empty, key.InvalidResult()).ToFin()
                               .Map(_ => new BlockPlacement(
                                   Id: placed.Id,
                                   Motion: placed.InstanceXform,
                                   Insertion: placed.InsertionPoint)))
                           .ToValidation())
                       .As()
                       .ToFin()
                   from _valid in projected
                       .Traverse(member => (
                           Valid(member.Geometry, key).ToValidation(),
                           Valid(member.Attributes, key).ToValidation())
                           .Apply(static (_, _) => unit)
                           .As())
                       .As()
                       .ToFin()
                   from _ in guard(projected.Count == active.ObjectCount, key.InvalidResult()).ToFin()
                   let memberIds = projected.Map(static member => member.Id)
                   let crc = projected.Fold(0u, static (chain, member) =>
                       member.Geometry.DataCRC(currentRemainder: chain))
                   from mode in SourceMode.Of(update: active.UpdateType, key: key)
                   from link in LinkState.Of(definition: active, key: key)
                   from usage in BlockUsage.Of(total: total, topLevel: topLevel, nested: nested, key: key)
                   let description = Op.Text(active.Description)
                   from content in Identity(
                       state: new BlockIdentityState(
                           Name: active.Name,
                           Description: description,
                           Mode: mode,
                           Style: active.LayerStyle,
                           Source: Op.Text(active.SourceArchive),
                           SkipNested: active.SkipNestedLinkedDefinitions,
                           ObjectCount: active.ObjectCount,
                           Members: projected),
                       op: key)
                   select new BlockSnapshot(
                       Key: active.Id,
                       Index: active.Index,
                       Name: active.Name,
                       Description: description,
                       Link: link,
                       ObjectCount: active.ObjectCount,
                       MemberIds: memberIds,
                       Scope: referenceScope,
                       Placements: placements,
                       Usage: usage,
                       ContainerIds: toSeq(active.GetContainers()).Map(static container => container.Id),
                       Stamp: new BlockStamp(Geometry: GeometryCrc.Create(value: crc), Content: content));
        })
        select snapshot;

    public Fin<BlockDependencyAnswer> Probe(BlockDependency dependency, RhinoDoc document, Op key) =>
        key.Need(dependency)
            .Bind(active => Resolve(document: document, key: key)
                .Bind(owner => active.Measure(owner: owner, document: document, key: key)));

    private Fin<InstanceDefinition> Resolve(RhinoDoc document, Op key) =>
        ResourceRef.Of(id: Key).Bind(target => Definitions.Resolve(target: target, document: document, key: key));

    private static Fin<UInt128> Identity(BlockIdentityState state, Op op) =>
        op.Catch(() => Fin.Succ(value: ContentHash.Of(
            state: state,
            chunks: static (held, writer) => {
                _ = writer
                    .String(held.Name)
                    .Optional(held.Description, static (text, rows) => rows.String(text))
                    .Ordinal(held.Mode.Key)
                    .Ordinal((int)held.Style)
                    .Optional(held.Source, static (text, rows) => rows.String(text))
                    .Bool(held.SkipNested)
                    .Ordinal(held.ObjectCount)
                    .Rows(held.Members, static (member, rows) => _ = rows
                        .I64(member.Geometry.DataCRC(currentRemainder: 0u))
                        .I64(member.Attributes.DataCRC(currentRemainder: 0u)));
            })));

    private sealed record BlockIdentityState(
        string Name,
        Option<string> Description,
        SourceMode Mode,
        InstanceDefinitionLayerStyle Style,
        Option<string> Source,
        bool SkipNested,
        int ObjectCount,
        Seq<BlockMemberProjection> Members);

    private static Fin<Unit> Valid(Rhino.Runtime.CommonObject value, Op op) => op.Catch(() =>
        value.IsValidWithLog(out string log)
            ? Fin.Succ(value: unit)
            : Fin.Fail<Unit>(error: new KernelFault.InvalidValue(
                Label: value.GetType().Name,
                Requirement: string.IsNullOrWhiteSpace(value: log) ? "Native object validity failed." : log,
                Key: Some(op))));
}

public sealed record BlockPlacement(Guid Id, Transform Motion, Point3d Insertion);
```

## [04]-[POLICY_VALUES]

- Owner: closed policy owners carry host arguments as data — `ConflictPolicy`, `DeletionPolicy`, `ExplodePolicy`, `Placement`, and `BlockPreview`; `PreviewDisplay` admits the display host enum ONCE as a keyed boundary owner (roster decompile-verified against `Rhino.DocObjects.DisplayMode`) while projection and camera compose the Document-owned `DefinedView`/`IsoQuadrant` rosters (`Document/tables.md [03]`); `PreviewBudget` is the allocation policy row; `PreviewFrame` admits projection, extent, and raster scale once for every modality; `PreviewTarget` is the modality union and `BlockPreview.Of` its ONE mint.
- Law: call sites never reconstruct policy decisions from boolean tails or nullable overload slots; a rendered bitmap crosses only through lifecycle custody, and generated union dispatch selects each host overload once from its admitted case.
- Law: the three preview enums cross a PUBLIC signature only as their boundary rows — each roster excludes the host's `None` ordinal, because a preview demands a projection, a display mode, and (on the axonometric arm) a camera, and admitting `None` would push the refusal into the host call as a null bitmap. The generated `Validate` replaces the four `Enum.IsDefined` guards the factories re-spelled.
- Law: the preview extent is the kernel `AssetExtent` — width, height, scale, and the overflow-safe pixel math are the kernel's — and the allocation CEILING is this consumer's `PreviewBudget` policy row: the max raster edge seeds the extent's `MaxDimension` and the pixel budget admits at `PreviewFrame.Of`, so a preview past the budget refuses at the frame rather than allocating a bitmap nothing consumes. `RasterScale` stays the host DPI-scaling switch (D21 KEEP: `applyDpiScaling` is `CreatePreviewBitmap`'s own argument, a host raster fact with no kernel analogue).
- Law: `DeletionPolicy`'s dialogue column is the spine's `HostInteraction` row, never a second bare bool spelling the same axis.
- Packages: RhinoCommon blocks (`.api/api-rhinocommon-blocks.md:98-100` — the three `CreatePreviewBitmap` overloads), kernel `Interaction/asset` (`AssetExtent`), kernel `Domain/validation` (`CapabilitySet`), Thinktecture.Runtime.Extensions, LanguageExt.Core.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using Rasm.Domain;
using Rasm.Interaction;
using Rasm.Numerics;
using Rasm.Rhino.Document;
using Rhino.Display;
using Rhino.DocObjects;

namespace Rasm.Rhino.Blocks;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class ConflictPolicy {
    public static readonly ConflictPolicy Fail = new(key: 0);
    public static readonly ConflictPolicy Reuse = new(key: 1);
    public static readonly ConflictPolicy Mint = new(key: 2);
}

[SmartEnum<int>]
public sealed partial class DeletionPolicy {
    public static readonly DeletionPolicy RetainReferences = new(key: 0, deleteReferences: false, interaction: HostInteraction.Quiet);
    public static readonly DeletionPolicy Cascade = new(key: 1, deleteReferences: true, interaction: HostInteraction.Quiet);
    public static readonly DeletionPolicy InteractiveCascade = new(key: 2, deleteReferences: true, interaction: HostInteraction.Interactive);

    public bool DeleteReferences { get; }
    public HostInteraction Interaction { get; }
}

[SmartEnum<int>]
public sealed partial class ExplodeDepth {
    public static readonly ExplodeDepth Shallow = new(key: 0, nested: false);
    public static readonly ExplodeDepth Recursive = new(key: 1, nested: true);

    public bool Nested { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ExplodePolicy {
    private ExplodePolicy() { }
    public sealed record All(ExplodeDepth Depth) : ExplodePolicy;
    public sealed record Visible(ExplodeDepth Depth, ResourceId Viewport) : ExplodePolicy;
}

[SmartEnum<int>]
public sealed partial class PlacementKind {
    public static readonly PlacementKind Ordinary = new(key: 0, isReference: false);
    public static readonly PlacementKind Reference = new(key: 1, isReference: true);

    public bool IsReference { get; }
}

// --- [PREVIEW]
[SmartEnum<int>]
public sealed partial class PreviewDisplay {
    public static readonly PreviewDisplay Default = new(key: (int)DisplayMode.Default);
    public static readonly PreviewDisplay Wireframe = new(key: (int)DisplayMode.Wireframe);
    public static readonly PreviewDisplay Shaded = new(key: (int)DisplayMode.Shaded);
    public static readonly PreviewDisplay Rendered = new(key: (int)DisplayMode.RenderPreview);

    internal DisplayMode Host => (DisplayMode)Key;
}

[SmartEnum<int>]
public sealed partial class RasterScale {
    public static readonly RasterScale Device = new(key: 0, applyDpiScaling: true);
    public static readonly RasterScale Pixel = new(key: 1, applyDpiScaling: false);

    public bool ApplyDpiScaling { get; }
}

[SmartEnum<int>]
public sealed partial class PreviewDecoration {
    public static readonly PreviewDecoration Plain = new(key: 0, draw: false);
    public static readonly PreviewDecoration Drawn = new(key: 1, draw: true);

    public bool Draw { get; }
}

// --- [POLICIES] ------------------------------------------------------------------------
public sealed record PreviewBudget(Rasm.Numerics.Dimension MaxEdge, long MaxPixels) {
    public static PreviewBudget Default => Seed.Value;
    private static readonly Lazy<PreviewBudget> Seed = new(static () => new(
        MaxEdge: Rasm.Numerics.Dimension.Create(value: 4096), MaxPixels: 4096L * 4096L));
}

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
internal sealed partial class PreviewFrame {
    public DefinedView Projection { get; }
    public AssetExtent Extent { get; }
    public RasterScale Scale { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref DefinedView projection,
        ref AssetExtent extent,
        ref RasterScale scale) =>
        validationError = projection is not null && scale is not null && extent.IsValid
            ? null
            : new ValidationError(message: "Preview frame demands an admitted projection, extent, and raster scale.");

    internal static Fin<PreviewFrame> Of(
        DefinedView projection,
        AssetExtent extent,
        RasterScale scale,
        Op key,
        Option<PreviewBudget> budget = default) {
        PreviewBudget held = budget.IfNone(PreviewBudget.Default);
        return from _ in guard(extent.PixelCount <= held.MaxPixels, key.InvalidInput(axis: nameof(PreviewBudget.MaxPixels))).ToFin()
               from admitted in key.AcceptValidated<PreviewFrame>(
                   fault: Validate(projection, extent, scale, out PreviewFrame? frame),
                   admitted: frame)
               select admitted;
    }

    internal System.Drawing.Size ToSize() => new(width: Extent.PixelWidth, height: Extent.PixelHeight);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PreviewTarget {
    private PreviewTarget() { }
    public sealed record Whole(PreviewDisplay Mode) : PreviewTarget;
    public sealed record Member(ResourceId MemberId, PreviewDisplay Mode) : PreviewTarget;
    public sealed record Axonometric(ResourceId DisplayModeId, IsoQuadrant Camera, PreviewDecoration Decoration) : PreviewTarget;
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BlockPreview {
    private BlockPreview() { }
    private sealed record Framed(PreviewFrame Frame, PreviewTarget Target) : BlockPreview;

    public static Fin<BlockPreview> Of(PreviewFrame frame, PreviewTarget target, Op? key = null) {
        Op op = key.OrDefault();
        return (op.Need(frame).ToValidation(), op.Need(target).ToValidation())
            .Apply(static (admitted, modality) => (BlockPreview)new Framed(Frame: admitted, Target: modality))
            .As()
            .ToFin();
    }

    internal Fin<System.Drawing.Bitmap> Render(InstanceDefinition definition, Op key) => Switch(
        context: (Definition: definition, Op: key),
        framed: static (context, spec) => spec.Target.Switch(
            context: (context.Definition, context.Op, spec.Frame),
            whole: static (held, target) => held.Op.Catch(() => Optional(held.Definition.CreatePreviewBitmap(
                    definedViewportProjection: held.Frame.Projection.Native,
                    displayMode: target.Mode.Host,
                    bitmapSize: held.Frame.ToSize(),
                    applyDpiScaling: held.Frame.Scale.ApplyDpiScaling))
                .ToFin(Fail: held.Op.InvalidResult())),
            member: static (held, target) => held.Op.Catch(() => Optional(held.Definition.CreatePreviewBitmap(
                    definitionObjectId: target.MemberId.Value,
                    viewportProjection: held.Frame.Projection.Native,
                    displayMode: target.Mode.Host,
                    bitmapSize: held.Frame.ToSize(),
                    applyDpiScaling: held.Frame.Scale.ApplyDpiScaling))
                .ToFin(Fail: held.Op.InvalidResult())),
            axonometric: static (held, target) => held.Op.Catch(() => Optional(held.Definition.CreatePreviewBitmap(
                    displayModeId: target.DisplayModeId.Value,
                    viewportProjection: held.Frame.Projection.Native,
                    isometricCamera: target.Camera.Native,
                    drawDecorations: target.Decoration.Draw,
                    bitmapSize: held.Frame.ToSize(),
                    applyDpiScaling: held.Frame.Scale.ApplyDpiScaling))
                .ToFin(Fail: held.Op.InvalidResult()))));
}
```

## [05]-[SURFACE_LEDGER]

| [INDEX] | [OWNER]                 | [KIND]           | [INGRESS]              | [EGRESS]                        |
| :-----: | :---------------------- | :--------------- | :--------------------- | :------------------------------ |
|  [01]   | `Definitions`           | lens rows        | `Lens` · `DeletedLens` | `Resolve`                       |
|  [02]   | `BlockSnapshot`         | record           | `Of` · `Probe`         | scoped state · typed dependency |
|  [03]   | `BlockDependencyAnswer` | union            | `Measure`              | presence or nesting depth       |
|  [04]   | `SourceMode`            | keyed vocabulary | `Of`                   | facet + condition sets · host   |
|  [05]   | `LayerScope`            | keyed vocabulary | `Of`                   | layer policy · host             |
|  [06]   | `LinkState`             | union            | `Of`                   | static or linked evidence       |
|  [07]   | policy owners           | generated values | generated admission    | native arguments                |
|  [08]   | `BlockPreview`          | union            | `Of(frame, target)`    | `Render`                        |

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
