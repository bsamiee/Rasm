# [RASM_RHINO_BLOCK_MODEL]

Block state vocabulary (`Rasm.Rhino.Blocks`) owns one live address, one whole-state projection, one dependency request, and closed policy values for every mutation and preview seam. Native discriminants enter once, `LinkState` carries their interior meaning, `BlockStamp` separates process-local geometry change from federation content identity, and every sibling re-resolves the Document-owned `ResourceRef` through `Definitions.Lens` inside its document window.

## [01]-[INDEX]

- [02]-[ADDRESS]: `Definitions.Lens` this folder's one `ResourceLens<InstanceDefinition>` row over the document spine's `ResourceRef`, resolution re-entering the mutable table per use so no native definition escapes its owning document window.
- [03]-[SNAPSHOT]: `BlockSnapshot` capturing identity, normalized source state, member order, scoped placement evidence, usage, containers, and both change probes in one host read, `BlockDependency`/`BlockDependencyAnswer` folding the native table probes onto a typed answer, `SourceMode` and `LayerScope` closing the host source and layer discriminants at the boundary, and `LinkState` carrying their interior meaning.
- [04]-[POLICY_VALUES]: closed `ConflictPolicy`, `DeletionPolicy`, `ExplodePolicy`, `Placement`, and `BlockPreview` owners carrying host arguments as data, `PreviewFrame` admitting projection, extent, and raster scale once for every modality.
- [05]-[SURFACE_LEDGER]: owner-to-kind-to-ingress-to-egress roster across the lens row, snapshot, link state, policy owners, and preview union.

## [02]-[ADDRESS]

Document spine's `ResourceRef` is the only definition address; `Definitions.Lens` is this folder's one `ResourceLens<InstanceDefinition>` row, so resolution re-enters the mutable table per use, rejects deleted or invalid entries, and prevents a native `InstanceDefinition` from escaping its owning document window — a folder-local address union beside the lens row is the deleted form.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
public static class Definitions {
    internal static readonly ResourceLens<InstanceDefinition> Lens = new(
        ById: static (document, id) => document.InstanceDefinitions.Find(instanceId: id, ignoreDeletedInstanceDefinitions: true),
        ByName: static (document, name) => document.InstanceDefinitions.Find(name) is { IsDeleted: false } named ? named : null,
        ByIndex: static (document, index) => index >= 0 && index < document.InstanceDefinitions.Count
            && document.InstanceDefinitions[index] is { IsDeleted: false } row ? row : null);

    internal static Fin<InstanceDefinition> Resolve(ResourceRef target, RhinoDoc document, Op key) =>
        target.Resolve(document: document, lens: Lens, key: key);
}
```

## [03]-[SNAPSHOT]

`BlockSnapshot` resolves its `ResourceRef` through `Definitions.Lens`, then captures identity, normalized source state, member order, the reference scope its placement census ran under, placed-reference evidence, usage, containers, and both change probes in one host read. Member admission revalidates geometry and attributes with native diagnostic evidence before either enters identity. `Placements` and `InUse` answer only within `Scope`, while `Usage` carries the host's own scope-free tally, so a census reading empty under one scope is never confused with a definition nothing places. Containers carry stable ids, matching the topology rail's own vertex spelling. `BlockDependency` admits table indexes against their live bounds before one table fold composes the native dependency probes, and answers `BlockDependencyAnswer` — presence for a table probe, nesting depth for a definition probe — because one integer standing for both makes a depth of one indistinguishable from a boolean true.

`SourceMode` is the boundary owner for the definition's source axis: three live rows keyed on `InstanceDefinitionUpdateType` ordinals, the retired ordinal `1` folded onto `Static` at admission so a legacy archive reads without the fence ever spelling the `[Obsolete]` host case, and a `Regenerates` behavior column the refresh rail reads. `LayerScope` closes the linked-definition layer policy the same way, so no raw host discriminant crosses into a public payload. `LinkState` linked cases require a nonblank source and preserve embed, tenuous, nested-link, layer-scope, and archive-health evidence for refresh policy.

Every host read on this page runs inside `DocumentSession.Demand`, which resolves on the host command thread — live sessions marshal through `RhinoApp.InvokeAndWait` and headless sessions stay on the caller. That demand IS the affinity rail the thread-affine members (`GetReferences`, every `CreatePreviewBitmap` overload) require; composing one outside a demand is the deleted form.

`BlockStamp.Geometry` remains an in-process invalidation probe. `BlockStamp.Content` hashes length-prefixed STORED definition fields and a detached `File3dm` serialization of every admitted geometry and attribute payload; live-state axes — archive health and tenuous resolution — stay snapshot columns on `LinkState.Linked` and are declared derived, never preimage, because a probe that re-reads the linked file on every access would rehash an unchanged definition differently on two consecutive reads. Every preimage write is railed on its own byte count, so a short write refuses instead of shifting the field boundaries the hash depends on. Ordinal-derived archive ids preserve member order without admitting live definition, member, or archive-minted identity.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using CommunityToolkit.HighPerformance.Buffers;
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace Rasm.Rhino.Blocks;

// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class ReferenceScope {
    public static readonly ReferenceScope Direct = new(key: 0, hostValue: 0);
    public static readonly ReferenceScope Nested = new(key: 1, hostValue: 1);
    public static readonly ReferenceScope Definition = new(key: 2, hostValue: 2);

    // The host ordinal is the `GetReferences(int)` argument and nothing else, so it stays inside the folder that
    // spells that call; a public column invites a caller to pass an integer the vocabulary already owns.
    internal int HostValue { get; }
}

[SmartEnum<int>]
public sealed partial class SourceMode {
    public static readonly SourceMode Static = new(
        key: (int)InstanceDefinitionUpdateType.Static,
        regenerates: static (_, _) => true);
    public static readonly SourceMode LinkedAndEmbedded = new(
        key: (int)InstanceDefinitionUpdateType.LinkedAndEmbedded,
        regenerates: static (styled, _) => styled);
    public static readonly SourceMode Linked = new(
        key: (int)InstanceDefinitionUpdateType.Linked,
        regenerates: static (styled, readable) => styled && readable);

    // Retired host ordinal: the source axis once carried a fourth case the host marks `[Obsolete("Always use Static")]`.
    // Admission folds it onto `Static` so a legacy archive reads and no fence spells the obsolete member.
    private const int RetiredEmbeddedOrdinal = 1;

    internal InstanceDefinitionUpdateType Host => (InstanceDefinitionUpdateType)Key;

    internal bool Reads => this != Static;

    internal bool Embeds => this != Linked;

    [UseDelegateFromConstructor]
    internal partial bool Regenerates(bool styled, bool readable);

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

[SmartEnum<int>]
public sealed partial class SourceHealth {
    public static readonly SourceHealth Current = new(key: (int)InstanceDefinitionArchiveFileStatus.LinkedFileIsUpToDate, stale: false, broken: false);
    public static readonly SourceHealth Newer = new(key: (int)InstanceDefinitionArchiveFileStatus.LinkedFileIsNewer, stale: true, broken: false);
    public static readonly SourceHealth Older = new(key: (int)InstanceDefinitionArchiveFileStatus.LinkedFileIsOlder, stale: true, broken: false);
    public static readonly SourceHealth Different = new(key: (int)InstanceDefinitionArchiveFileStatus.LinkedFileIsDifferent, stale: true, broken: false);
    public static readonly SourceHealth NotFound = new(key: (int)InstanceDefinitionArchiveFileStatus.LinkedFileNotFound, stale: false, broken: true);
    public static readonly SourceHealth Unreadable = new(key: (int)InstanceDefinitionArchiveFileStatus.LinkedFileNotReadable, stale: false, broken: true);

    public bool Stale { get; }
    public bool Broken { get; }

    internal static Option<SourceHealth> Of(InstanceDefinitionArchiveFileStatus status) =>
        TryGet((int)status, out SourceHealth? found) ? Some(found) : Option<SourceHealth>.None;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LinkState {
    private LinkState() { }
    public sealed record Static : LinkState;
    public sealed record Linked(
        DocumentPath Path,
        bool AlsoEmbedded,
        SourceHealth Health,
        LayerScope LayerStyle,
        bool Tenuous,
        bool SkipNested) : LinkState;

    internal static Fin<LinkState> Of(InstanceDefinition definition, Op key) =>
        from mode in SourceMode.Of(update: definition.UpdateType, key: key)
        from state in mode.Reads
            ? from path in DocumentPath.Of(value: definition.SourceArchive, key: key)
              from health in SourceHealth.Of(status: definition.ArchiveFileStatus)
                  .ToFin(Fail: key.InvalidResult(detail: definition.ArchiveFileStatus.ToString()))
              from scope in LayerScope.Of(style: definition.LayerStyle, key: key)
              select (LinkState)new Linked(
                  Path: path,
                  AlsoEmbedded: mode.Embeds,
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

// The probe answered ONE `int` carrying three unrelated meanings — `0` absent, `1` a present table dependency,
// and the host's nesting DEPTH for a definition probe — so `> 0` read as "used" on two arms and as "used at least
// one level deep" on the third, and no reader could tell a depth of one from a boolean true. The answer family is
// `BlockGraphAnswer`'s discipline applied here: presence and depth are different questions with different types.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BlockDependencyAnswer : IDetachedDocumentResult {
    private BlockDependencyAnswer() { }
    public sealed record Uses(bool Value) : BlockDependencyAnswer;
    public sealed record Nesting(int Levels) : BlockDependencyAnswer;
}

// --- [MODELS] ------------------------------------------------------------------------------
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
            ? validationError
            : new ValidationError(message: "usage counts are inconsistent");

    internal static Fin<BlockUsage> Of(int total, int topLevel, int nested, Op key) =>
        Admission.Admitted(
            fault: Validate(total, topLevel, nested, out BlockUsage? admitted),
            value: admitted,
            refusal: key.InvalidResult(detail: nameof(BlockUsage)));
}

public sealed record BlockStamp(GeometryCrc Geometry, UInt128 Content);

internal sealed record BlockMemberProjection(
    Guid Id,
    GeometryBase Geometry,
    ObjectAttributes Attributes);

public sealed record BlockPlacement(Guid Id, Transform Motion, Point3d Insertion);

// `Scope` is the placement census's own question, and the snapshot could not answer it: `Placements` and `InUse`
// derive from `GetReferences(scope)`, so a `Direct` read answering "not in use" and a `Definition` read answering
// "in use" were the same shape with no column separating them. `Usage` stays the host's own scope-free tally, so
// the pair reads as census-under-this-scope beside total-across-the-document.
public sealed record BlockSnapshot(
    Guid Key,
    int Index,
    string Name,
    Option<string> Description,
    LinkState Link,
    int ObjectCount,
    Seq<Guid> MemberIds,
    ReferenceScope Scope,
    Seq<BlockPlacement> Placements,
    BlockUsage Usage,
    bool InUse,
    Seq<Guid> ContainerIds,
    BlockStamp Stamp) {
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
                   let geometry = GeometryCrc.Create(value: crc)
                   from link in LinkState.Of(definition: active, key: key)
                   from mode in SourceMode.Of(update: active.UpdateType, key: key)
                   from usage in BlockUsage.Of(total: total, topLevel: topLevel, nested: nested, key: key)
                   let description = Op.Text(active.Description)
                   from content in Identity(
                       name: active.Name,
                       description: description,
                       mode: mode,
                       style: active.LayerStyle,
                       source: Op.Text(active.SourceArchive),
                       skipNested: active.SkipNestedLinkedDefinitions,
                       objectCount: active.ObjectCount,
                       crc: crc,
                       members: projected,
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
                       InUse: !placements.IsEmpty,
                       ContainerIds: toSeq(active.GetContainers()).Map(static container => container.Id),
                       Stamp: new BlockStamp(Geometry: geometry, Content: content));
        })
        select snapshot;

    public Fin<BlockDependencyAnswer> Probe(BlockDependency dependency, RhinoDoc document, Op key) =>
        key.Need(dependency)
            .Bind(active => Resolve(document: document, key: key)
                .Bind(owner => active.Measure(owner: owner, document: document, key: key)));

    private Fin<InstanceDefinition> Resolve(RhinoDoc document, Op key) =>
        ResourceRef.Of(id: Key).Bind(target => Definitions.Resolve(target: target, document: document, key: key));

    // The preimage is STORED CONTENT alone. `ArchiveFileStatus` and `IsTenuous` are live probes — the first
    // re-reads the linked file's timestamps on every access, the second reports whether the host has resolved the
    // link yet — so folding them in made one unchanged definition hash differently on two consecutive reads and
    // across two machines, which is the one thing a federation identity must never do. Both remain snapshot
    // columns through `LinkState.Linked`, declared derived live state that no consumer may treat as identity.
    private static Fin<UInt128> Identity(
        string name,
        Option<string> description,
        SourceMode mode,
        InstanceDefinitionLayerStyle style,
        Option<string> source,
        bool skipNested,
        int objectCount,
        uint crc,
        Seq<BlockMemberProjection> members,
        Op op) => op.Catch(() => {
        using ArrayPoolBufferWriter<byte> bytes = new();
        return from _ in Write(bytes: bytes, value: name, op: op)
               from __ in Write(bytes: bytes, value: description.IfNone(string.Empty), op: op)
               from ___ in Write(bytes: bytes, value: mode.Key, op: op)
               from ____ in Write(bytes: bytes, value: (int)style, op: op)
               from _____ in Write(bytes: bytes, value: source.IfNone(string.Empty), op: op)
               from ______ in Write(bytes: bytes, value: skipNested, op: op)
               from _______ in Write(bytes: bytes, value: objectCount, op: op)
               from ________ in Write(bytes: bytes, value: crc, op: op)
               from _________ in Write(bytes: bytes, value: members.Count, op: op)
               from payload in Archived(members: members, op: op)
               from __________ in Write(bytes: bytes, value: payload, op: op)
               select ContentHash.Of(canonicalBytes: bytes.WrittenSpan);
    });

    // Exemption: `File3dm.Objects.Add` is the archive's own accumulator and each member must both mint its
    // ordinal id and land in the same pass, so the staging runs as one effectful fold rather than a projection —
    // a `Map` doing this work looked pure and could not be reordered or made lazy without silently reordering the
    // archive, which is exactly the byte sequence the identity hashes.
    private static Fin<byte[]> Archived(Seq<BlockMemberProjection> members, Op op) => op.Catch(() => {
        using File3dm archive = new();
        return members
            .Fold(
                Fin.Succ(value: unit),
                (rail, member) => rail.Bind(_ => op.Catch(() => {
                    Guid expected = ArchiveId(ordinal: archive.Objects.Count);
                    using ObjectAttributes attributes = member.Attributes.Duplicate();
                    attributes.ObjectId = expected;
                    return guard(
                        archive.Objects.Add(item: member.Geometry, attributes: attributes) == expected,
                        op.InvalidResult()).ToFin();
                })))
            .Bind(_ => Optional(archive.ToByteArray()).ToFin(Fail: op.InvalidResult()));
    });

    // The upper twelve bytes are the ordinal's zero PREFIX, not incidental scratch: `stackalloc` on a span the
    // runtime zero-initializes is what makes ordinal `n` map to one stable guid on every machine, so the write
    // deliberately fills only the trailing four and reads the rest as declared zeroes.
    private static Guid ArchiveId(int ordinal) {
        Span<byte> bytes = stackalloc byte[16];
        BinaryPrimitives.WriteInt32BigEndian(destination: bytes[^sizeof(int)..], value: checked(ordinal + 1));
        return new Guid(bytes, bigEndian: true);
    }

    private static Fin<Unit> Valid(Rhino.Runtime.CommonObject value, Op op) => op.Catch(() =>
        value.IsValidWithLog(out string log)
            ? Fin.Succ(value: unit)
            : Fin.Fail<Unit>(error: new Fault.InvalidValue(
                Label: value.GetType().Name,
                Requirement: string.IsNullOrWhiteSpace(value: log) ? "Native object validity failed." : log,
                Key: Some(op))));

    // `TryWriteBigEndian` reports a SHORT write as `false` and answers a byte count the caller then advances by:
    // discarding the verdict advanced the writer past a field that was never fully written, so the preimage
    // silently lost bytes and two different definitions hashed alike. Every leg is now railed on that verdict.
    private static Fin<Unit> Write<T>(ArrayPoolBufferWriter<byte> bytes, T value, Op op)
        where T : unmanaged, IBinaryInteger<T> =>
        op.Catch(() => {
            bool written = value.TryWriteBigEndian(
                destination: bytes.GetSpan(sizeHint: Unsafe.SizeOf<T>()),
                bytesWritten: out int count);
            return guard(written && count == Unsafe.SizeOf<T>(), op.InvalidResult()).ToFin()
                .Map(_ => Op.Side(() => bytes.Advance(count: count)));
        });

    private static Fin<Unit> Write(ArrayPoolBufferWriter<byte> bytes, bool value, Op op) =>
        Write(bytes: bytes, value: value ? 1 : 0, op: op);

    private static Fin<Unit> Write(ArrayPoolBufferWriter<byte> bytes, string value, Op op) =>
        op.Catch(() => {
            int count = Encoding.UTF8.GetByteCount(value: value);
            return Write(bytes: bytes, value: count, op: op).Bind(_ => {
                int encoded = Encoding.UTF8.GetBytes(value, bytes.GetSpan(sizeHint: count));
                return guard(encoded == count, op.InvalidResult()).ToFin()
                    .Map(__ => Op.Side(() => bytes.Advance(count: encoded)));
            });
        });

    private static Fin<Unit> Write(ArrayPoolBufferWriter<byte> bytes, byte[] value, Op op) =>
        Write(bytes: bytes, value: value.Length, op: op).Bind(_ => op.Catch(() => {
            value.AsSpan().CopyTo(destination: bytes.GetSpan(sizeHint: value.Length));
            return Fin.Succ(value: Op.Side(() => bytes.Advance(count: value.Length)));
        }));
}
```

## [04]-[POLICY_VALUES]

Closed policy owners carry host arguments as data. Operations dispatch `ConflictPolicy`, `DeletionPolicy`, `ExplodePolicy`, `Placement`, and `BlockPreview`; call sites never reconstruct those decisions from boolean tails or nullable overload slots.

`BlockPreview` selects all verified preview modalities, including member selection. `PreviewExtent` carries dimensions and their width, height, and pixel budget through one admission gate; `PreviewFrame` admits projection, extent, and raster scale once for every modality, while each case carries only its distinct target, display, camera, and decoration evidence. A rendered bitmap crosses only through lifecycle custody, and generated union dispatch selects each host overload once from its admitted case.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
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

    // The dialogue column is the spine's `HostInteraction` row, not a second bare bool spelling the same axis: the
    // host `quiet` argument every folder passes has ONE vocabulary, and a policy carrying its own boolean forks it.
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
    public sealed record Visible(ExplodeDepth Depth, Guid ViewportId) : ExplodePolicy;
}

[SmartEnum<int>]
public sealed partial class PlacementKind {
    public static readonly PlacementKind Ordinary = new(key: 0, isReference: false);
    public static readonly PlacementKind Reference = new(key: 1, isReference: true);

    public bool IsReference { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Placement {
    private Placement() { }
    public sealed record Bare(Transform Motion) : Placement;
    public sealed record Attributed(Transform Motion, ObjectAttributes Attributes) : Placement;
    public sealed record Recorded(
        Transform Motion,
        ObjectAttributes Attributes,
        Lease<HistoryRecord> History,
        PlacementKind Kind) : Placement;
}

[ComplexValueObject]
public sealed partial class PreviewExtent {
    public int Width { get; }
    public int Height { get; }
    public int MaxWidth { get; }
    public int MaxHeight { get; }
    public long MaxPixels { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int width,
        ref int height,
        ref int maxWidth,
        ref int maxHeight,
        ref long maxPixels) =>
        validationError = width > 0
            && height > 0
            && maxWidth > 0
            && maxHeight > 0
            && maxPixels > 0
            && width <= maxWidth
            && height <= maxHeight
            && width <= maxPixels / height
            ? validationError
            : new ValidationError(message: "preview extent is invalid");

    internal System.Drawing.Size ToSize() => new(width: Width, height: Height);
}

[SmartEnum<int>]
public sealed partial class RasterScale {
    public static readonly RasterScale Device = new(key: 0, applyDpiScaling: true);
    public static readonly RasterScale Pixel = new(key: 1, applyDpiScaling: false);

    public bool ApplyDpiScaling { get; }
}

[ComplexValueObject]
internal sealed partial class PreviewFrame {
    public DefinedViewportProjection Projection { get; }
    public PreviewExtent Extent { get; }
    public RasterScale Scale { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref DefinedViewportProjection projection,
        ref PreviewExtent extent,
        ref RasterScale scale) =>
        validationError = !Enum.IsDefined(value: projection) || extent is null || scale is null
            ? new ValidationError(message: "preview frame is invalid")
            : null;

    internal static Fin<PreviewFrame> Of(
        DefinedViewportProjection projection,
        PreviewExtent extent,
        RasterScale scale,
        Op key) => Admission.Admitted(
            fault: Validate(projection, extent, scale, out PreviewFrame? admitted),
            value: admitted,
            refusal: key.InvalidInput());
}

[SmartEnum<int>]
public sealed partial class PreviewDecoration {
    public static readonly PreviewDecoration Plain = new(key: 0, draw: false);
    public static readonly PreviewDecoration Drawn = new(key: 1, draw: true);

    public bool Draw { get; }
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BlockPreview {
    private BlockPreview() { }
    private sealed record StandardCase(PreviewFrame Frame, DisplayMode Mode) : BlockPreview;
    private sealed record SelectedCase(
        Guid MemberId, PreviewFrame Frame, DisplayMode Mode) : BlockPreview;
    private sealed record AxonometricCase(
        Guid DisplayModeId, PreviewFrame Frame, IsometricCamera Camera, PreviewDecoration Decoration) : BlockPreview;

    public static Fin<BlockPreview> Standard(
        DefinedViewportProjection projection, DisplayMode mode, PreviewExtent extent, RasterScale scale, Op key) =>
        (PreviewFrame.Of(projection: projection, extent: extent, scale: scale, key: key).ToValidation(),
         guard(Enum.IsDefined(value: mode), key.InvalidInput()).ToFin().ToValidation())
        .Apply((frame, _) => (BlockPreview)new StandardCase(Frame: frame, Mode: mode))
        .As()
        .ToFin();

    public static Fin<BlockPreview> Selected(
        Guid memberId, DefinedViewportProjection projection, DisplayMode mode,
        PreviewExtent extent, RasterScale scale, Op key) =>
        (guard(memberId != Guid.Empty, key.InvalidInput()).ToFin().ToValidation(),
         PreviewFrame.Of(projection: projection, extent: extent, scale: scale, key: key).ToValidation(),
         guard(Enum.IsDefined(value: mode), key.InvalidInput()).ToFin().ToValidation())
        .Apply((_, frame, _) => (BlockPreview)new SelectedCase(MemberId: memberId, Frame: frame, Mode: mode))
        .As()
        .ToFin();

    public static Fin<BlockPreview> Axonometric(
        Guid displayModeId, DefinedViewportProjection projection, IsometricCamera camera,
        PreviewExtent extent, PreviewDecoration decoration, RasterScale scale, Op key) =>
        (guard(displayModeId != Guid.Empty, key.InvalidInput()).ToFin().ToValidation(),
         PreviewFrame.Of(projection: projection, extent: extent, scale: scale, key: key).ToValidation(),
         guard(Enum.IsDefined(value: camera), key.InvalidInput()).ToFin().ToValidation(),
         key.Need(decoration).ToValidation())
        .Apply((_, frame, _, admittedDecoration) =>
            (BlockPreview)new AxonometricCase(
                DisplayModeId: displayModeId, Frame: frame, Camera: camera, Decoration: admittedDecoration))
        .As()
        .ToFin();

    internal Fin<System.Drawing.Bitmap> Render(InstanceDefinition definition, Op key) =>
        Switch(
            context: (Definition: definition, Op: key),
            standardCase: static (context, spec) => context.Op.Catch(() => Optional(context.Definition.CreatePreviewBitmap(
                    definedViewportProjection: spec.Frame.Projection,
                    displayMode: spec.Mode,
                    bitmapSize: spec.Frame.Extent.ToSize(),
                    applyDpiScaling: spec.Frame.Scale.ApplyDpiScaling))
                .ToFin(Fail: context.Op.InvalidResult())),
            selectedCase: static (context, spec) => context.Op.Catch(() => Optional(context.Definition.CreatePreviewBitmap(
                    definitionObjectId: spec.MemberId,
                    viewportProjection: spec.Frame.Projection,
                    displayMode: spec.Mode,
                    bitmapSize: spec.Frame.Extent.ToSize(),
                    applyDpiScaling: spec.Frame.Scale.ApplyDpiScaling))
                .ToFin(Fail: context.Op.InvalidResult())),
            axonometricCase: static (context, spec) => context.Op.Catch(() => Optional(context.Definition.CreatePreviewBitmap(
                    displayModeId: spec.DisplayModeId,
                    viewportProjection: spec.Frame.Projection,
                    isometricCamera: spec.Camera,
                    drawDecorations: spec.Decoration.Draw,
                    bitmapSize: spec.Frame.Extent.ToSize(),
                    applyDpiScaling: spec.Frame.Scale.ApplyDpiScaling))
                .ToFin(Fail: context.Op.InvalidResult())));
}
```

## [05]-[SURFACE_LEDGER]

| [INDEX] | [OWNER]                 | [KIND]           | [INGRESS]           | [EGRESS]                        |
| :-----: | :---------------------- | :--------------- | :------------------ | :------------------------------ |
|  [01]   | `Definitions`           | lens row         | `Lens`              | `Resolve`                       |
|  [02]   | `BlockSnapshot`         | record           | `Of` · `Probe`      | scoped state · typed dependency |
|  [03]   | `BlockDependencyAnswer` | union            | `Measure`           | presence or nesting depth       |
|  [04]   | `SourceMode`            | keyed vocabulary | `Of`                | regeneration column · host      |
|  [05]   | `LayerScope`            | keyed vocabulary | `Of`                | layer policy · host             |
|  [06]   | `LinkState`             | union            | `Of`                | static or linked evidence       |
|  [07]   | policy owners           | generated values | generated admission | native arguments                |
|  [08]   | `BlockPreview`          | union            | factories           | `Render`                        |

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
