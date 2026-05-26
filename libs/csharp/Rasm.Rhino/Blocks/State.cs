using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Drawing;
using Rasm.Rhino.Commands;
using Rhino.DocObjects.Tables;
using InstanceAttributeField = Rhino.Runtime.TextFields.InstanceAttributeField;
using IOPath = System.IO.Path;

namespace Rasm.Rhino.Blocks;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class ReferenceScope {
    public static readonly ReferenceScope Top = new(key: 0);
    public static readonly ReferenceScope TopAndNested = new(key: 1);
    public static readonly ReferenceScope FromOtherDefinitions = new(key: 2);

    public int Native => Key;
}

[SmartEnum<int>]
public sealed partial class UpdatePolicy {
    public static readonly UpdatePolicy Static = new(key: 0, native: InstanceDefinitionUpdateType.Static);
    public static readonly UpdatePolicy LinkedAndEmbedded = new(key: 1, native: InstanceDefinitionUpdateType.LinkedAndEmbedded);
    public static readonly UpdatePolicy Linked = new(key: 2, native: InstanceDefinitionUpdateType.Linked);

    public InstanceDefinitionUpdateType Native { get; }

    public static UpdatePolicy FromNative(InstanceDefinitionUpdateType native) =>
        native switch {
            InstanceDefinitionUpdateType.Linked => Linked,
            InstanceDefinitionUpdateType.LinkedAndEmbedded => LinkedAndEmbedded,
            _ => Static,
        };
}

[SmartEnum<int>]
public sealed partial class LayerStyle {
    public static readonly LayerStyle None = new(key: 0, native: InstanceDefinitionLayerStyle.None);
    public static readonly LayerStyle Active = new(key: 1, native: InstanceDefinitionLayerStyle.Active);
    public static readonly LayerStyle Reference = new(key: 2, native: InstanceDefinitionLayerStyle.Reference);

    public InstanceDefinitionLayerStyle Native { get; }

    public bool AppliesTo(UpdatePolicy policy) =>
        (this, policy) switch {
            _ when this == Reference => policy == UpdatePolicy.Linked,
            _ when this == None => policy == UpdatePolicy.Static || policy == UpdatePolicy.LinkedAndEmbedded,
            _ => true,
        };

    public static LayerStyle FromNative(InstanceDefinitionLayerStyle native) =>
        native switch {
            InstanceDefinitionLayerStyle.Active => Active,
            InstanceDefinitionLayerStyle.Reference => Reference,
            _ => None,
        };
}

[SmartEnum<int>]
public sealed partial class ArchiveStatus {
    public static readonly ArchiveStatus NotALinked = new(key: 0, native: InstanceDefinitionArchiveFileStatus.NotALinkedInstanceDefinition);
    public static readonly ArchiveStatus LinkedFileNotReadable = new(key: 1, native: InstanceDefinitionArchiveFileStatus.LinkedFileNotReadable);
    public static readonly ArchiveStatus LinkedFileNotFound = new(key: 2, native: InstanceDefinitionArchiveFileStatus.LinkedFileNotFound);
    public static readonly ArchiveStatus LinkedFileIsUpToDate = new(key: 3, native: InstanceDefinitionArchiveFileStatus.LinkedFileIsUpToDate);
    public static readonly ArchiveStatus LinkedFileIsNewer = new(key: 4, native: InstanceDefinitionArchiveFileStatus.LinkedFileIsNewer);
    public static readonly ArchiveStatus LinkedFileIsOlder = new(key: 5, native: InstanceDefinitionArchiveFileStatus.LinkedFileIsOlder);
    public static readonly ArchiveStatus LinkedFileIsDifferent = new(key: 6, native: InstanceDefinitionArchiveFileStatus.LinkedFileIsDifferent);

    public InstanceDefinitionArchiveFileStatus Native { get; }

    public bool RequiresRefresh =>
        Native is not InstanceDefinitionArchiveFileStatus.LinkedFileIsUpToDate
            and not InstanceDefinitionArchiveFileStatus.NotALinkedInstanceDefinition;

    public static ArchiveStatus FromNative(InstanceDefinitionArchiveFileStatus native) =>
        native switch {
            InstanceDefinitionArchiveFileStatus.LinkedFileNotReadable => LinkedFileNotReadable,
            InstanceDefinitionArchiveFileStatus.LinkedFileNotFound => LinkedFileNotFound,
            InstanceDefinitionArchiveFileStatus.LinkedFileIsUpToDate => LinkedFileIsUpToDate,
            InstanceDefinitionArchiveFileStatus.LinkedFileIsNewer => LinkedFileIsNewer,
            InstanceDefinitionArchiveFileStatus.LinkedFileIsOlder => LinkedFileIsOlder,
            InstanceDefinitionArchiveFileStatus.LinkedFileIsDifferent => LinkedFileIsDifferent,
            _ => NotALinked,
        };
}

[SmartEnum<int>]
public sealed partial class ConflictPolicy {
    public static readonly ConflictPolicy Replace = new(key: 0);
    public static readonly ConflictPolicy Skip = new(key: 1);
    public static readonly ConflictPolicy Fail = new(key: 2);
    public static readonly ConflictPolicy Rename = new(key: 3);
}

[SmartEnum<int>]
public sealed partial class BlockSourceMode {
    public static readonly BlockSourceMode FromConstruction = new(key: 0);
    public static readonly BlockSourceMode FromDocumentObjects = new(key: 1);
    public static readonly BlockSourceMode FromArchive = new(key: 2);
}

[SmartEnum<int>]
public sealed partial class DeletionPolicy {
    public static readonly DeletionPolicy ReferencesAndQuiet = new(key: 0, deleteReferences: true, quiet: true);
    public static readonly DeletionPolicy ReferencesOnly = new(key: 1, deleteReferences: true, quiet: false);
    public static readonly DeletionPolicy QuietOnly = new(key: 2, deleteReferences: false, quiet: true);
    public static readonly DeletionPolicy Strict = new(key: 3, deleteReferences: false, quiet: false);

    public bool DeleteReferences { get; }
    public bool Quiet { get; }
}

[Union]
public abstract partial record ExplodePolicy {
    private ExplodePolicy() { }
    public sealed record All() : ExplodePolicy;
    public sealed record VisibleOnly() : ExplodePolicy;
    public sealed record VisibleIn(Guid ViewportId) : ExplodePolicy;

    public bool SkipsHidden => this is not All;
    public Guid ViewportFilter => this is VisibleIn v ? v.ViewportId : Guid.Empty;
}

[SmartEnum<int>]
public sealed partial class BlockEdgeKind {
    public static readonly BlockEdgeKind Member = new(key: 0);
    public static readonly BlockEdgeKind Insert = new(key: 1);
    public static readonly BlockEdgeKind Container = new(key: 2);
    public static readonly BlockEdgeKind LinkedArchive = new(key: 3);
}

// --- [MODELS] -----------------------------------------------------------------------------
[ValueObject<Guid>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct DefinitionId {
    public static Fin<DefinitionId> From(Guid value, Op? key = null) =>
        value == Guid.Empty
            ? Fin.Fail<DefinitionId>(error: key.OrDefault().InvalidInput())
            : Fin.Succ(value: Create(value: value));
}

[ValueObject<int>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct DefinitionIndex {
    public static Fin<DefinitionIndex> From(int value, Op? key = null) =>
        value < 0
            ? Fin.Fail<DefinitionIndex>(error: key.OrDefault().InvalidInput())
            : Fin.Succ(value: Create(value: value));
}

[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public readonly partial struct DefinitionName {
    public const int MaxLength = 256;

    public static Fin<DefinitionName> From(string value, Op? key = null) {
        Op op = key.OrDefault();
        return Optional(value).Map(static raw => raw.Trim()).Case switch {
            string trimmed when trimmed.Length > 0
                                && trimmed.Length <= MaxLength
                                && ModelComponent.IsValidComponentName(name: trimmed) =>
                Fin.Succ(value: Create(value: trimmed)),
            _ => Fin.Fail<DefinitionName>(error: op.InvalidInput()),
        };
    }
}

[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public readonly partial struct ArchivePath {
    public static Fin<ArchivePath> From(string value, Op? key = null) {
        Op op = key.OrDefault();
        return Optional(value).Map(static raw => raw.Trim()).Case switch {
            string trimmed when trimmed.Length > 0 && IOPath.IsPathFullyQualified(path: trimmed) =>
                Fin.Succ(value: Create(value: trimmed)),
            _ => Fin.Fail<ArchivePath>(error: op.InvalidInput()),
        };
    }

    public string FileName => IOPath.GetFileName(path: Value);
    public string Stem => IOPath.GetFileNameWithoutExtension(path: Value);
}

// --- [MODELS] [REF] -----------------------------------------------------------------------
[Union]
public abstract partial record DefinitionRef {
    private DefinitionRef() { }
    public sealed record ById(DefinitionId Id) : DefinitionRef;
    public sealed record ByIndex(DefinitionIndex Index) : DefinitionRef;
    public sealed record ByName(DefinitionName Name) : DefinitionRef;

    public static DefinitionRef Of(DefinitionId id) => new ById(Id: id);
    public static DefinitionRef Of(DefinitionIndex index) => new ByIndex(Index: index);
    public static DefinitionRef Of(DefinitionName name) => new ByName(Name: name);
}

// --- [MODELS] [SPECS] ---------------------------------------------------------------------
public sealed record MetadataPatch(
    Option<string> Description = default,
    Option<ArchivePath> Url = default,
    Option<string> UrlDescription = default,
    HashMap<string, string> UserStrings = default) {
    public static MetadataPatch Empty { get; } = new();
    public bool IsEmpty => Description.IsNone && Url.IsNone && UrlDescription.IsNone && UserStrings.IsEmpty;
}

public sealed record AuthorSpec(
    DefinitionName Name,
    Point3d BasePoint,
    UpdatePolicy Update,
    LayerStyle Layer,
    MetadataPatch Metadata) {

    public static Fin<AuthorSpec> Of(
        DefinitionName name,
        Point3d basePoint,
        UpdatePolicy? update = null,
        LayerStyle? layer = null,
        MetadataPatch? metadata = null,
        Op? key = null) {
        Op op = key.OrDefault();
        UpdatePolicy policy = update ?? UpdatePolicy.Static;
        LayerStyle style = layer ?? (policy == UpdatePolicy.Linked ? LayerStyle.Reference : LayerStyle.None);
        return (basePoint.IsValid, style.AppliesTo(policy: policy)) switch {
            (true, true) => Fin.Succ(value: new AuthorSpec(
                Name: name,
                BasePoint: basePoint,
                Update: policy,
                Layer: style,
                Metadata: metadata ?? MetadataPatch.Empty)),
            _ => Fin.Fail<AuthorSpec>(error: op.InvalidInput()),
        };
    }
}

public sealed record BlockMembers(Seq<GeometryBase> Geometry, Seq<ObjectAttributes> Attributes) {
    public static BlockMembers Empty { get; } = new(Geometry: Seq<GeometryBase>(), Attributes: Seq<ObjectAttributes>());
    public int Count => Geometry.Count;
    public bool IsEmpty => Count == 0;

    public static Fin<BlockMembers> Of(Seq<GeometryBase> geometry, Seq<ObjectAttributes>? attributes = null, Op? key = null) {
        Op op = key.OrDefault();
        Seq<ObjectAttributes> attrs = attributes ?? geometry.Map(static _ => new ObjectAttributes());
        return (geometry.IsEmpty, geometry.Count == attrs.Count, geometry.Exists(static g => g is null)) switch {
            (false, true, false) => Fin.Succ(value: new BlockMembers(Geometry: geometry, Attributes: attrs)),
            _ => Fin.Fail<BlockMembers>(error: op.InvalidInput()),
        };
    }
}

public sealed record PreviewSpec(
    int Width = 256,
    int Height = 256,
    Option<Guid> DisplayModeId = default,
    bool Isometric = true,
    bool DrawDecorations = false) {
    public static PreviewSpec Default { get; } = new();

    public static Fin<PreviewSpec> Of(int width, int height, Option<Guid> displayModeId = default, bool isometric = true, bool drawDecorations = false, Op? key = null) {
        Op op = key.OrDefault();
        return (width, height) switch {
            ( >= 16 and <= 4096, >= 16 and <= 4096) =>
                Fin.Succ(value: new PreviewSpec(Width: width, Height: height, DisplayModeId: displayModeId, Isometric: isometric, DrawDecorations: drawDecorations)),
            _ => Fin.Fail<PreviewSpec>(error: op.InvalidInput()),
        };
    }
}

public sealed record BlockSubscriptionPolicy(Option<Func<BlockTableEvent, bool>> Filter = default) {
    public static BlockSubscriptionPolicy Default { get; } = new();
}

// --- [MODELS] [SNAPSHOTS] -----------------------------------------------------------------
public sealed record BlockSnapshot(
    DefinitionId Id,
    DefinitionIndex Index,
    DefinitionName Name,
    Option<string> DeletedName,
    Option<string> Description,
    Option<ArchivePath> Url,
    Option<string> UrlDescription,
    UnitSystem Units,
    UpdatePolicy Update,
    LayerStyle Layer,
    ArchiveStatus Archive,
    Option<ArchivePath> Source,
    ImmutableArray<Guid> MemberIds,
    bool IsDeleted,
    bool IsTenuous,
    int UseCountTop,
    int UseCountNested) {

    public static Fin<BlockSnapshot> From(InstanceDefinition definition, Op? key = null) {
        Op op = key.OrDefault();
        return Optional(definition)
            .ToFin(Fail: op.InvalidInput())
            .Bind(active => op.Catch(() => Project(active: active, op: op)));
    }

    private static Fin<BlockSnapshot> Project(InstanceDefinition active, Op op) =>
        from id in DefinitionId.From(value: active.Id, key: op)
        from idx in DefinitionIndex.From(value: active.Index, key: op)
        from name in DefinitionName.From(value: active.Name ?? string.Empty, key: op)
        let deletedName = NonBlank(value: active.DeletedName)
        let description = NonBlank(value: active.Description)
        let urlDescription = NonBlank(value: active.UrlDescription)
        let url = NonBlank(value: active.Url).Bind(value => ArchivePath.From(value: value, key: op).ToOption())
        let source = ProjectSourceArchive(definition: active, op: op)
        let members = active.GetObjectIds() is Guid[] arr ? [.. arr] : ImmutableArray<Guid>.Empty
        let counts = ReadUseCounts(active: active)
        select new BlockSnapshot(
            Id: id,
            Index: idx,
            Name: name,
            DeletedName: deletedName,
            Description: description,
            Url: url,
            UrlDescription: urlDescription,
            Units: active.UnitSystem,
            Update: UpdatePolicy.FromNative(native: active.UpdateType),
            Layer: LayerStyle.FromNative(native: active.LayerStyle),
            Archive: ArchiveStatus.FromNative(native: active.ArchiveFileStatus),
            Source: source,
            MemberIds: members,
            IsDeleted: active.IsDeleted,
            IsTenuous: active.IsTenuous,
            UseCountTop: counts.Top,
            UseCountNested: counts.Nested);

    private static Option<string> NonBlank(string? value) =>
        Optional(value).Filter(static s => !string.IsNullOrWhiteSpace(value: s));

    private static Option<ArchivePath> ProjectSourceArchive(InstanceDefinition definition, Op op) =>
        NonBlank(value: definition.SourceArchive)
            .Bind(value => ArchivePath.From(value: value, key: op).ToOption());

    private static (int Top, int Nested) ReadUseCounts(InstanceDefinition active) {
        // [BOUNDARY ADAPTER — UseCount(out, out) is the canonical Rhino split; capture once.]
        _ = active.UseCount(topLevelReferenceCount: out int top, nestedReferenceCount: out int nested);
        return (top, nested);
    }
}

public readonly record struct BlockMemberContext(DefinitionId DefId, Guid MemberId, Option<ObjectAttributes> Attrs);

public readonly record struct BlockInsertContext(
    Guid InstanceId,
    DefinitionId DefId,
    Transform InstanceXform,
    Option<Guid> SelectedPartId) {
    public Point3d InsertionPoint => InstanceXform * Point3d.Origin;

    public static Fin<BlockInsertContext> From(InstanceObject instance, Option<Guid> selectedPart = default, Op? key = null) {
        Op op = key.OrDefault();
        return Optional(instance).ToFin(Fail: op.InvalidInput()).Bind(active =>
            Optional(active.InstanceDefinition).ToFin(Fail: op.InvalidResult()).Bind(def =>
                DefinitionId.From(value: def.Id, key: op).Map(id => new BlockInsertContext(
                    InstanceId: active.Id,
                    DefId: id,
                    InstanceXform: active.InstanceXform,
                    SelectedPartId: selectedPart))));
    }
}

public sealed record BlockArchiveSnapshot(
    DefinitionId Id,
    DefinitionName Name,
    Option<string> Description,
    Option<ArchivePath> Source,
    ImmutableArray<Guid> MemberIds) {

    public static Fin<BlockArchiveSnapshot> From(InstanceDefinitionGeometry geometry, Op? key = null) {
        Op op = key.OrDefault();
        return Optional(geometry).ToFin(Fail: op.InvalidInput()).Bind(active => op.Catch(() => Project(active: active, op: op)));
    }

    private static Fin<BlockArchiveSnapshot> Project(InstanceDefinitionGeometry active, Op op) =>
        from id in DefinitionId.From(value: active.Id, key: op)
        from name in DefinitionName.From(value: active.Name ?? string.Empty, key: op)
        let description = Optional(active.Description).Filter(static s => !string.IsNullOrWhiteSpace(value: s))
        let source = Optional(active.SourceArchive)
            .Filter(static s => !string.IsNullOrWhiteSpace(value: s))
            .Bind(value => ArchivePath.From(value: value, key: op).ToOption())
        let members = active.GetObjectIds() is Guid[] ids ? [.. ids] : ImmutableArray<Guid>.Empty
        select new BlockArchiveSnapshot(Id: id, Name: name, Description: description, Source: source, MemberIds: members);
}

public readonly record struct BlockTableEvent(
    InstanceDefinitionTableEventType Kind,
    int Index,
    Option<BlockSnapshot> New,
    Option<BlockArchiveSnapshot> Old) {

    public static BlockTableEvent From(InstanceDefinitionTableEventArgs args) {
        ArgumentNullException.ThrowIfNull(argument: args);
        // [BOUNDARY ADAPTER — OldState wraps pre-mutation native ptr; project archive-readable subset inside the callback.]
        return new BlockTableEvent(
            Kind: args.EventType,
            Index: args.InstanceDefinitionIndex,
            New: Optional(args.NewState).Bind(definition => BlockSnapshot.From(definition: definition).ToOption()),
            Old: Optional(args.OldState).Bind(geometry => BlockArchiveSnapshot.From(geometry: geometry).ToOption()));
    }
}

// --- [MODELS] [GRAPH] ---------------------------------------------------------------------
[Union]
public abstract partial record EdgeTarget {
    private EdgeTarget() { }
    public sealed record ObjectId(Guid Id) : EdgeTarget;
    public sealed record Definition(DefinitionId Id) : EdgeTarget;
    public sealed record Archive(ArchivePath Path) : EdgeTarget;
}

public sealed record BlockGraphNode(DefinitionId Id, DefinitionName Name, ImmutableArray<Guid> Members);

public sealed record BlockGraphEdge(DefinitionId From, BlockEdgeKind Kind, EdgeTarget To);

public sealed record BlockGraph(ImmutableArray<BlockGraphNode> Nodes, ImmutableArray<BlockGraphEdge> Edges) {
    public static BlockGraph Empty { get; } = new(Nodes: [], Edges: []);
}

// --- [MODELS] [RESULTS] -------------------------------------------------------------------
public sealed record MutationReceipt(DocumentReceipt Document, Seq<BlockDiagnostic> Diagnostics) {
    public static MutationReceipt Empty { get; } = new(Document: DocumentReceipt.Empty, Diagnostics: Seq<BlockDiagnostic>());

    public static MutationReceipt Of(DocumentReceipt receipt) =>
        new(Document: receipt, Diagnostics: Seq<BlockDiagnostic>());

    public static MutationReceipt Of(DocumentReceipt receipt, Seq<BlockDiagnostic> diagnostics) =>
        new(Document: receipt, Diagnostics: diagnostics);

    public bool HasDiagnostics => !Diagnostics.IsEmpty;
}

[Union]
public abstract partial record BlockResult {
    private BlockResult() { }
    public sealed record Snapshot(BlockSnapshot Value) : BlockResult;
    public sealed record Snapshots(Seq<BlockSnapshot> Values) : BlockResult;
    public sealed record Members(Seq<BlockMemberContext> Values) : BlockResult;
    public sealed record Inserts(Seq<BlockInsertContext> Values) : BlockResult;
    public sealed record Definitions(Seq<DefinitionId> Values) : BlockResult;
    public sealed record Graph(BlockGraph Value) : BlockResult;
    public sealed record Name(DefinitionName Value) : BlockResult;
    public sealed record Texts(HashMap<string, string> Fields) : BlockResult;
    public sealed record Attributes(Seq<InstanceAttributeField> Values) : BlockResult;
    public sealed record Preview(PreviewHandle Handle) : BlockResult;
}

public sealed class PreviewHandle : IDisposable {
    private readonly Action<PreviewHandle> release;
    private bool disposed;

    internal PreviewHandle(Bitmap bitmap, Action<PreviewHandle> release) {
        Bitmap = bitmap ?? throw new ArgumentNullException(paramName: nameof(bitmap));
        this.release = release ?? throw new ArgumentNullException(paramName: nameof(release));
    }

    public Bitmap Bitmap { get; }

    public void Dispose() {
        // [BOUNDARY ADAPTER — IDisposable; cache or caller decides actual bitmap disposal via release callback.]
        if (disposed) return;
        disposed = true;
        release(obj: this);
    }
}

// --- [ERRORS] -----------------------------------------------------------------------------
[Union]
public abstract partial record BlockDiagnostic {
    private BlockDiagnostic() { }
    public sealed record NotFound(DefinitionRef Ref) : BlockDiagnostic;
    public sealed record DuplicateName(DefinitionName Name, DefinitionId Existing) : BlockDiagnostic;
    public sealed record LinkedSetterIgnored(DefinitionId Id, UpdatePolicy Actual) : BlockDiagnostic;
    public sealed record LinkedRefreshFailed(DefinitionId Id, ArchiveStatus Status) : BlockDiagnostic;
    public sealed record ExplodePartial(int Requested, int Received) : BlockDiagnostic;
    public sealed record HistoryNotRecorded(Guid InstanceId) : BlockDiagnostic;
    public sealed record PreviewUnavailable(DefinitionId Id) : BlockDiagnostic;
    public sealed record GeometryRejected(DefinitionId Id, string Reason) : BlockDiagnostic;
    public sealed record ArchiveLinkFailed(ArchivePath Path) : BlockDiagnostic;
}

// --- [MODELS] [INDEX] ---------------------------------------------------------------------
public sealed class BlockSnapshotIndex {
    private readonly FrozenDictionary<DefinitionName, BlockSnapshot> byName;
    private readonly FrozenDictionary<DefinitionId, BlockSnapshot> byId;
    private readonly FrozenDictionary<DefinitionIndex, BlockSnapshot> byIndex;

    private BlockSnapshotIndex(Seq<BlockSnapshot> snapshots) {
        byName = snapshots.AsIterable()
            .Map(static s => KeyValuePair.Create(key: s.Name, value: s))
            .ToFrozenDictionary();
        byId = snapshots.AsIterable()
            .Map(static s => KeyValuePair.Create(key: s.Id, value: s))
            .ToFrozenDictionary();
        byIndex = snapshots.AsIterable()
            .Map(static s => KeyValuePair.Create(key: s.Index, value: s))
            .ToFrozenDictionary();
    }

    public static BlockSnapshotIndex Empty { get; } = new(snapshots: Seq<BlockSnapshot>());

    public static BlockSnapshotIndex From(Seq<BlockSnapshot> snapshots) =>
        snapshots.IsEmpty ? Empty : new(snapshots: snapshots);

    public Option<BlockSnapshot> Find(DefinitionRef reference) =>
        reference switch {
            DefinitionRef.ById r => byId.Lookup(key: r.Id),
            DefinitionRef.ByIndex r => byIndex.Lookup(key: r.Index),
            DefinitionRef.ByName r => byName.Lookup(key: r.Name),
            _ => Option<BlockSnapshot>.None,
        };

    public Seq<BlockSnapshot> All => toSeq(byId.Values);
    public int Count => byId.Count;
}

// --- [OPERATIONS] [HELPERS] ---------------------------------------------------------------
internal static class BlockReceipt {
    internal static DocumentReceipt Named(string name) =>
        DocumentReceipt.Empty with {
            ResourceChanged = Seq(new DocumentResourceChange(Kind: DocumentResourceKind.Block, Name: name)),
        };

    internal static DocumentReceipt Lifecycle(Guid id, string name) =>
        DocumentReceipt.Empty with {
            LifecycleChanged = Seq(id),
            ResourceChanged = Seq(new DocumentResourceChange(Kind: DocumentResourceKind.Block, Name: name)),
        };
}

internal static class BlockCollectionExtensions {
    internal static Option<V> Lookup<K, V>(this FrozenDictionary<K, V> dict, K key) where K : notnull =>
        dict.TryGetValue(key: key, value: out V? value) ? Some(value!) : Option<V>.None;

    internal static HashMap<K, V> UpdateIf<K, V>(this HashMap<K, V> map, K key, Func<V, V> update) =>
        map.Find(key: key) switch {
            { IsSome: true, Case: V existing } => map.AddOrUpdate(key: key, value: update(arg: existing)),
            _ => map,
        };
}
