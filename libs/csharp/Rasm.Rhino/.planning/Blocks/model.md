# [RASM_RHINO_BLOCK_MODEL]

Block state vocabulary (`Rasm.Rhino.Blocks`) owns one live address, one whole-state projection, one dependency request, and closed policy values for every mutation and preview seam. Native discriminants enter once, `LinkState` carries their interior meaning, `BlockStamp` separates process-local geometry change from federation content identity, and every sibling re-resolves the Document-owned `ResourceRef` through `Definitions.Lens` inside its document window.

## [01]-[INDEX]

| [INDEX] | [OWNER] | [CONTRACT] |
| :-----: | :------ | :--------- |
|  [01]   | `Definitions` | folder `ResourceLens<InstanceDefinition>` row |
|  [02]   | `BlockSnapshot` | whole-state evidence and dependency probes |
|  [03]   | policy values | authoring, deletion, explosion, placement, and preview decisions |

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

`BlockSnapshot` resolves its `ResourceRef` through `Definitions.Lens`, then captures identity, normalized source state, member order, placed-reference evidence, usage, containers, and both change probes in one host read. Member admission revalidates geometry and attributes with native diagnostic evidence before either enters identity. `BlockDependency` admits table indexes against their live bounds before one table fold composes the native dependency probes; its single integer result is `0` when absent, `1` for a present table dependency, and the host nesting depth for a definition dependency.

`LinkState` normalizes obsolete `Embedded` definitions to `Static`. Linked cases require a nonblank source and preserve embed, tenuous, nested-link, layer-style, and archive-health evidence for refresh policy.

`BlockStamp.Geometry` remains an in-process invalidation probe. `BlockStamp.Content` hashes length-prefixed definition fields and a detached `File3dm` serialization of every admitted geometry and attribute payload. Ordinal-derived archive ids preserve member order without admitting live definition, member, or archive-minted identity.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using CommunityToolkit.HighPerformance.Buffers;

// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class ReferenceScope {
    public static readonly ReferenceScope Direct = new(key: 0, hostValue: 0);
    public static readonly ReferenceScope Nested = new(key: 1, hostValue: 1);
    public static readonly ReferenceScope Definition = new(key: 2, hostValue: 2);

    public int HostValue { get; }
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
        string Path,
        bool AlsoEmbedded,
        SourceHealth Health,
        InstanceDefinitionLayerStyle LayerStyle,
        bool Tenuous,
        bool SkipNested) : LinkState;

    internal static Fin<LinkState> Of(InstanceDefinition definition, Op key) =>
        definition.UpdateType switch {
            InstanceDefinitionUpdateType.Static or InstanceDefinitionUpdateType.Embedded =>
                Fin.Succ<LinkState>(value: new Static()),
            InstanceDefinitionUpdateType.Linked or InstanceDefinitionUpdateType.LinkedAndEmbedded =>
                from path in key.AcceptText(value: definition.SourceArchive)
                from health in SourceHealth.Of(status: definition.ArchiveFileStatus)
                    .ToFin(Fail: key.InvalidResult(detail: definition.ArchiveFileStatus.ToString()))
                select (LinkState)new Linked(
                    Path: path,
                    AlsoEmbedded: definition.UpdateType is InstanceDefinitionUpdateType.LinkedAndEmbedded,
                    Health: health,
                    LayerStyle: definition.LayerStyle,
                    Tenuous: definition.IsTenuous,
                    SkipNested: definition.SkipNestedLinkedDefinitions),
            var unknown => Fin.Fail<LinkState>(error: key.InvalidResult(detail: unknown.ToString())),
        };
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BlockDependency {
    private BlockDependency() { }
    public sealed record Layer(int Index) : BlockDependency;
    public sealed record Linetype(int Index) : BlockDependency;
    public sealed record Definition(ResourceRef Target) : BlockDependency;

    internal Fin<int> Measure(InstanceDefinition owner, RhinoDoc document, Op key) => Switch(
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
                value: context.Owner.UsesDefinition(otherIdefIndex: nested.Index)))));

    private static Fin<int> MeasureTable(
        int index,
        InstanceDefinition owner,
        RhinoDoc document,
        Func<RhinoDoc, int> count,
        Func<InstanceDefinition, int, bool> includes,
        Op op) => op.Catch(() => index >= 0 && index < count(arg: document)
        ? Fin.Succ(value: includes(arg1: owner, arg2: index) ? 1 : 0)
        : Fin.Fail<int>(error: op.InvalidInput()));
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

    internal static Fin<BlockUsage> Of(int total, int topLevel, int nested, Op key) {
        ValidationError? error = Validate(total, topLevel, nested, out BlockUsage? usage);
        return error is null && usage is not null
            ? Fin.Succ(value: usage)
            : Fin.Fail<BlockUsage>(error: key.InvalidResult(detail: error?.ToString() ?? nameof(BlockUsage)));
    }
}

public sealed record BlockStamp(GeometryCrc Geometry, UInt128 Content);

internal sealed record BlockMemberProjection(
    Guid Id,
    GeometryBase Geometry,
    ObjectAttributes Attributes);

public sealed record BlockPlacement(Guid Id, Transform Motion, Point3d Insertion);

public sealed record BlockSnapshot(
    Guid Key,
    int Index,
    string Name,
    Option<string> Description,
    LinkState Link,
    int ObjectCount,
    Seq<Guid> MemberIds,
    Seq<BlockPlacement> Placements,
    BlockUsage Usage,
    bool InUse,
    Seq<int> ContainerIndexes,
    BlockStamp Stamp) {
    public static Fin<BlockSnapshot> Of(ResourceRef target, RhinoDoc document, ReferenceScope scope, Op key) =>
        from address in Optional(target).ToFin(Fail: key.InvalidInput())
        from owner in Optional(document).ToFin(Fail: key.InvalidInput())
        from referenceScope in Optional(scope).ToFin(Fail: key.InvalidInput())
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
                   from usage in BlockUsage.Of(total: total, topLevel: topLevel, nested: nested, key: key)
                   let description = Optional(active.Description).Filter(static text => text.Length > 0)
                   from content in Identity(
                       name: active.Name,
                       description: description,
                       update: active.UpdateType,
                       status: active.ArchiveFileStatus,
                       style: active.LayerStyle,
                       source: Optional(active.SourceArchive).Filter(static path => path.Length > 0),
                       tenuous: active.IsTenuous,
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
                       Placements: placements,
                       Usage: usage,
                       InUse: !placements.IsEmpty,
                       ContainerIndexes: toSeq(active.GetContainers()).Map(static container => container.Index),
                       Stamp: new BlockStamp(Geometry: geometry, Content: content));
        })
        select snapshot;

    public Fin<int> Probe(BlockDependency dependency, RhinoDoc document, Op key) =>
        Optional(dependency).ToFin(Fail: key.InvalidInput())
            .Bind(active => Resolve(document: document, key: key)
                .Bind(owner => active.Measure(owner: owner, document: document, key: key)));

    private Fin<InstanceDefinition> Resolve(RhinoDoc document, Op key) =>
        ResourceRef.Of(id: Key).Bind(target => Definitions.Resolve(target: target, document: document, key: key));

    private static Fin<UInt128> Identity(
        string name,
        Option<string> description,
        InstanceDefinitionUpdateType update,
        InstanceDefinitionArchiveFileStatus status,
        InstanceDefinitionLayerStyle style,
        Option<string> source,
        bool tenuous,
        bool skipNested,
        int objectCount,
        uint crc,
        Seq<BlockMemberProjection> members,
        Op op) => op.Catch(() => {
        using ArrayPoolBufferWriter<byte> bytes = new();
        Write(bytes: bytes, value: name);
        Write(bytes: bytes, value: description.IfNone(string.Empty));
        Write(bytes: bytes, value: (int)update);
        Write(bytes: bytes, value: (int)status);
        Write(bytes: bytes, value: (int)style);
        Write(bytes: bytes, value: source.IfNone(string.Empty));
        Write(bytes: bytes, value: tenuous);
        Write(bytes: bytes, value: skipNested);
        Write(bytes: bytes, value: objectCount);
        Write(bytes: bytes, value: crc);
        Write(bytes: bytes, value: members.Count);

        using File3dm archive = new();
        Seq<(Guid Expected, Guid Actual)> archived = members
            .Map((member, index) => {
                Guid expected = ArchiveId(ordinal: index);
                using ObjectAttributes attributes = member.Attributes.Duplicate();
                attributes.ObjectId = expected;
                return (
                    Expected: expected,
                    Actual: archive.Objects.Add(item: member.Geometry, attributes: attributes));
            })
            .Strict();
        return from _ in archived
                   .Traverse(row => guard(row.Actual == row.Expected, op.InvalidResult()).ToFin().ToValidation())
                   .As()
                   .ToFin()
               from payload in Optional(archive.ToByteArray()).ToFin(Fail: op.InvalidResult())
               let __ = Write(bytes: bytes, value: payload)
               select ContentHash.Of(canonicalBytes: bytes.WrittenSpan);
    });

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

    private static void Write<T>(ArrayPoolBufferWriter<byte> bytes, T value) where T : unmanaged, IBinaryInteger<T> {
        _ = value.TryWriteBigEndian(destination: bytes.GetSpan(sizeHint: Unsafe.SizeOf<T>()), bytesWritten: out int written);
        bytes.Advance(count: written);
    }

    private static void Write(ArrayPoolBufferWriter<byte> bytes, bool value) =>
        Write(bytes: bytes, value: value ? 1 : 0);

    private static void Write(ArrayPoolBufferWriter<byte> bytes, string value) {
        int count = Encoding.UTF8.GetByteCount(value: value);
        Write(bytes: bytes, value: count);
        _ = Encoding.UTF8.GetBytes(value, bytes.GetSpan(sizeHint: count));
        bytes.Advance(count: count);
    }

    private static Unit Write(ArrayPoolBufferWriter<byte> bytes, byte[] value) {
        Write(bytes: bytes, value: value.Length);
        value.AsSpan().CopyTo(destination: bytes.GetSpan(sizeHint: value.Length));
        bytes.Advance(count: value.Length);
        return unit;
    }
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
    public static readonly DeletionPolicy RetainReferences = new(key: 0, deleteReferences: false, quiet: true);
    public static readonly DeletionPolicy Cascade = new(key: 1, deleteReferences: true, quiet: true);
    public static readonly DeletionPolicy InteractiveCascade = new(key: 2, deleteReferences: true, quiet: false);

    public bool DeleteReferences { get; }
    public bool Quiet { get; }
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
        HistoryRecord History,
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
         Optional(decoration).ToFin(Fail: key.InvalidInput()).ToValidation())
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

| [INDEX] | [OWNER] | [KIND] | [INGRESS] | [EGRESS] |
| :-----: | :------ | :----- | :-------- | :------- |
|  [01]   | `Definitions` | lens row | `Lens` | `Resolve` |
|  [02]   | `BlockSnapshot` | record | `Of` · `Probe` | state · dependency measure |
|  [03]   | `LinkState` | union | `Of` | static or linked evidence |
|  [04]   | policy owners | generated values | generated admission | native arguments |
|  [05]   | `BlockPreview` | union | factories | `Render` |

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
