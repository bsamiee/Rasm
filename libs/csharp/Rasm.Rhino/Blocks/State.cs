using System.Buffers;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Drawing;
using System.Runtime.InteropServices;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Exchange;
using Rhino.Collections;
using Rhino.DocObjects.Tables;
using InstanceAttributeField = Rhino.Runtime.TextFields.InstanceAttributeField;
using IOPath = System.IO.Path;

namespace Rasm.Rhino.Blocks;

// --- [CONSTANTS] [FNV] --------------------------------------------------------------------
internal static class Fnv64 {
    internal const ulong Offset = 0xCBF29CE484222325UL;
    internal const ulong Prime = 0x100000001B3UL;
    internal static ulong Mix(ulong acc, ulong v) => unchecked((acc ^ v) * Prime);
    internal static ulong HashGuid(Guid value, ulong seed = Offset) =>
        value.ToByteArray().Aggregate(seed: seed, func: static (acc, b) => Mix(acc: acc, v: b));
    internal static ulong HashText(string value, ulong seed = Offset) =>
        value.Aggregate(seed: seed, func: static (acc, ch) => Mix(acc: acc, v: ch));
}

// --- [CONSTANTS] [PATHS] ----------------------------------------------------------------
internal static class BlockPaths {
    internal static Option<string> DocAnchor(RhinoDoc document) =>
        Optional(document.Path)
            .Filter(static path => !string.IsNullOrWhiteSpace(value: path))
            .Bind(static path => Optional(IOPath.GetDirectoryName(path: path)).Filter(static dir => !string.IsNullOrWhiteSpace(value: dir)));

    internal static Option<string> ArchiveAnchor(Option<string> archivePath) =>
        archivePath.Bind(static path => Optional(IOPath.GetDirectoryName(path: path)).Filter(static s => !string.IsNullOrWhiteSpace(value: s)));
}

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
    public bool IsLinked => this == Linked || this == LinkedAndEmbedded;

    public Fin<Unit> RequireLinked(Op key) =>
        IsLinked ? Fin.Succ(value: unit) : Fin.Fail<Unit>(error: key.InvalidInput());

    public Fin<Unit> RejectLinkedModify(Op key) =>
        IsLinked ? Fin.Fail<Unit>(error: key.InvalidInput()) : Fin.Succ(value: unit);

    public static UpdatePolicy FromNative(InstanceDefinitionUpdateType native) =>
        native switch {
            InstanceDefinitionUpdateType.Linked => Linked,
            InstanceDefinitionUpdateType.LinkedAndEmbedded => LinkedAndEmbedded,
#pragma warning disable CS0618 // BOUNDARY ADAPTER — legacy documents may still report obsolete Embedded; collapse to LinkedAndEmbedded on read.
            InstanceDefinitionUpdateType.Embedded => LinkedAndEmbedded,
#pragma warning restore CS0618
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
            _ when this == None => policy == UpdatePolicy.Static || policy == UpdatePolicy.LinkedAndEmbedded,
            _ when this == Active => policy == UpdatePolicy.Linked,
            _ when this == Reference => policy == UpdatePolicy.Linked,
            _ => false,
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

    public bool CanRefresh =>
        Native is InstanceDefinitionArchiveFileStatus.LinkedFileIsUpToDate
            or InstanceDefinitionArchiveFileStatus.LinkedFileIsNewer
            or InstanceDefinitionArchiveFileStatus.LinkedFileIsOlder
            or InstanceDefinitionArchiveFileStatus.LinkedFileIsDifferent;

    public bool IsBroken =>
        Native is InstanceDefinitionArchiveFileStatus.LinkedFileNotReadable
            or InstanceDefinitionArchiveFileStatus.LinkedFileNotFound;

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

public readonly record struct DeletionPolicy(bool DeleteReferences, bool Quiet) {
    public static readonly DeletionPolicy ReferencesAndQuiet = new(DeleteReferences: true, Quiet: true);
    public static readonly DeletionPolicy ReferencesOnly = new(DeleteReferences: true, Quiet: false);
    public static readonly DeletionPolicy QuietOnly = new(DeleteReferences: false, Quiet: true);
    public static readonly DeletionPolicy Strict = new(DeleteReferences: false, Quiet: false);
}

[SmartEnum<int>]
public sealed partial class TransformPolicy {
    public static readonly TransformPolicy Copy = new(key: 0, deleteOriginal: false, usesHistory: false);
    public static readonly TransformPolicy Move = new(key: 1, deleteOriginal: true, usesHistory: false);
    public static readonly TransformPolicy History = new(key: 2, deleteOriginal: false, usesHistory: true);

    public bool DeleteOriginal { get; }
    public bool UsesHistory { get; }

    public Guid Apply(ObjectTable objects, Guid instanceId, Transform xform) {
        ArgumentNullException.ThrowIfNull(argument: objects);
        return UsesHistory
            ? objects.TransformWithHistory(objectId: instanceId, xform: xform)
            : objects.Transform(objectId: instanceId, xform: xform, deleteOriginal: DeleteOriginal);
    }

    public DocumentReceipt InstanceTransform(Guid instanceId, Guid resultId) =>
        (DeleteOriginal, UsesHistory) switch {
            (_, true) => DocumentReceipt.Empty with { Created = Seq(resultId) },
            (true, false) => DocumentReceipt.Empty with { Created = Seq(resultId), Deleted = Seq(instanceId) },
            _ => DocumentReceipt.Empty with { Created = Seq(resultId), Transformed = Seq(resultId) },
        };
}

public readonly record struct LinkRefreshPolicy(bool SkipUpToDate) {
    public static readonly LinkRefreshPolicy All = new(SkipUpToDate: false);
    public static readonly LinkRefreshPolicy Changed = new(SkipUpToDate: true);
}

public readonly record struct LinkReloadPolicy(bool UpdateNestedLinks, bool Quiet) {
    public static readonly LinkReloadPolicy NestedQuiet = new(UpdateNestedLinks: true, Quiet: true);
    public static readonly LinkReloadPolicy NestedVerbose = new(UpdateNestedLinks: true, Quiet: false);
    public static readonly LinkReloadPolicy ShallowQuiet = new(UpdateNestedLinks: false, Quiet: true);
    public static readonly LinkReloadPolicy ShallowVerbose = new(UpdateNestedLinks: false, Quiet: false);
}

public readonly record struct CompactPolicy(bool IgnoreUndoReferences) {
    public static readonly CompactPolicy UndoAware = new(IgnoreUndoReferences: false);
    public static readonly CompactPolicy AllDeleted = new(IgnoreUndoReferences: true);
}

[SmartEnum<int>]
public sealed partial class LinkedPolicy {
    public static readonly LinkedPolicy Prompt = new(key: 0, native: LinkedInstanceDefinitionUpdateStyle.Prompt);
    public static readonly LinkedPolicy AlwaysUpdate = new(key: 1, native: LinkedInstanceDefinitionUpdateStyle.AlwaysUpdate);
    public static readonly LinkedPolicy NeverUpdate = new(key: 2, native: LinkedInstanceDefinitionUpdateStyle.NeverUpdate);

    public LinkedInstanceDefinitionUpdateStyle Native { get; }

    public static LinkedPolicy FromNative(LinkedInstanceDefinitionUpdateStyle native) =>
        native switch {
            LinkedInstanceDefinitionUpdateStyle.AlwaysUpdate => AlwaysUpdate,
            LinkedInstanceDefinitionUpdateStyle.NeverUpdate => NeverUpdate,
            _ => Prompt,
        };
}

[SmartEnum<int>]
public sealed partial class BlockEdgeKind {
    public static readonly BlockEdgeKind Member = new(key: 0);
    public static readonly BlockEdgeKind LinkedArchive = new(key: 1);
    public static readonly BlockEdgeKind InstanceInsert = new(key: 2);
}

[Union]
public abstract partial record ExplodePolicy {
    private ExplodePolicy() { }
    public sealed record All() : ExplodePolicy;
    public sealed record VisibleOnly() : ExplodePolicy;
    public sealed record VisibleIn(Guid ViewportId) : ExplodePolicy;
    public sealed record Native(bool ExplodeNested = true, bool DeleteInstance = true) : ExplodePolicy {
        public DocumentReceipt ExplodeReceipt(Seq<Guid> created, Guid instanceId) =>
            DocumentReceipt.Empty with {
                Created = created,
                Deleted = DeleteInstance ? Seq(instanceId) : Seq<Guid>(),
            };
    }

    public (bool SkipsHidden, Guid ViewportFilter) Resolve() =>
        Switch(
            all: static _ => (SkipsHidden: false, ViewportFilter: Guid.Empty),
            visibleOnly: static _ => (SkipsHidden: true, ViewportFilter: Guid.Empty),
            visibleIn: static v => (SkipsHidden: true, ViewportFilter: v.ViewportId),
            native: static _ => (SkipsHidden: false, ViewportFilter: Guid.Empty));

    public bool IncludesNestedInstances => Switch(
        all: static _ => true,
        visibleOnly: static _ => true,
        visibleIn: static _ => true,
        native: static n => n.ExplodeNested);

    public bool UsesNativePieces => this is Native;
}

[Union]
public abstract partial record DisplayModeRef {
    private DisplayModeRef() { }
    public sealed record WireframeCase() : DisplayModeRef;
    public sealed record ById(Guid Id) : DisplayModeRef;
    public sealed record ByName(string Name) : DisplayModeRef;

    public static readonly DisplayModeRef Wireframe = new WireframeCase();
    public static DisplayModeRef Of(Guid id) => new ById(Id: id);
    public static DisplayModeRef Of(string name) => new ByName(Name: name);

    public Fin<Guid> Resolve(Op op) =>
        Switch(op,
            wireframeCase: static (_, _) => Fin.Succ(value: DisplayModeDescription.WireframeId),
            byId: static (_, r) => Fin.Succ(value: r.Id),
            byName: static (k, r) => Optional(DisplayModeDescription.FindByName(englishName: r.Name))
                .Map(static desc => desc.Id)
                .ToFin(Fail: k.InvalidInput()));

    internal static DisplayMode NativeMode(Guid displayId) =>
        displayId == DisplayModeDescription.WireframeId ? DisplayMode.Wireframe
        : displayId == DisplayModeDescription.ShadedId ? DisplayMode.Shaded
        : displayId == DisplayModeDescription.RenderedId ? DisplayMode.RenderPreview
        : DisplayMode.Shaded;
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct PreviewFingerprint(ulong Value);

[Union]
public abstract partial record DependencyTarget {
    private DependencyTarget() { }
    public sealed record OnDefinition(int OtherIndex) : DependencyTarget;
    public sealed record OnLayer(int LayerIndex) : DependencyTarget;
    public sealed record OnLinetype(int LinetypeIndex) : DependencyTarget;
    public sealed record InUse(ReferenceScope Scope) : DependencyTarget;

    public DependencyProbe ProbeOn(InstanceDefinition live) =>
        Switch(live,
            onDefinition: static (d, t) => new DependencyProbe.DefinitionDepth(Depth: d.UsesDefinition(otherIdefIndex: t.OtherIndex)),
            onLayer: static (d, t) => new DependencyProbe.LayerUsed(Used: d.UsesLayer(layerIndex: t.LayerIndex)),
            onLinetype: static (d, t) => (DependencyProbe)new DependencyProbe.LinetypeUsed(Used: d.UsesLinetype(linetypeIndex: t.LinetypeIndex)),
            inUse: static (d, t) => new DependencyProbe.InUse(Used: d.InUse(wheretoLook: t.Scope.Native)));
}

[Union]
public abstract partial record DependencyProbe {
    private DependencyProbe() { }
    public sealed record DefinitionDepth(int Depth) : DependencyProbe;
    public sealed record LayerUsed(bool Used) : DependencyProbe;
    public sealed record LinetypeUsed(bool Used) : DependencyProbe;
    public sealed record InUse(bool Used) : DependencyProbe;
}

[Union(SwitchMapStateParameterName = "ctx")]
public abstract partial record GraphQuery {
    private GraphQuery() { }
    public sealed record Members(DefinitionRef Ref) : GraphQuery;
    public sealed record Inserts(DefinitionRef Ref, ReferenceScope Scope) : GraphQuery;
    public sealed record Containers(DefinitionRef Ref) : GraphQuery;
    public sealed record Depends(DefinitionRef Ref, DependencyTarget Target) : GraphQuery;
    public sealed record Audit() : GraphQuery;
    public sealed record Plan(Option<DefinitionRef> Root = default) : GraphQuery;
    public sealed record Stats() : GraphQuery;
    public sealed record Health(BlockFilter? Filter = null) : GraphQuery;
    public sealed record Reach(DefinitionRef Ref, ReferenceScope Scope, DepthPolicy? Policy = null) : GraphQuery;
    public sealed record EnsureIndexed(Option<DefinitionRef> Ref = default) : GraphQuery;
}

public sealed record BlockFilter(
    Option<Seq<string>> Archives = default,
    Option<Seq<DefinitionRef>> Refs = default,
    Option<UpdatePolicy> Update = default,
    Option<ArchiveStatus> Archive = default) {
    public static BlockFilter All { get; } = new();
    public static BlockFilter ArchivesOnly(Seq<string> paths) => new(Archives: Some(paths));
    public static BlockFilter RefsOnly(Seq<DefinitionRef> refs) => new(Refs: Some(refs));
    public static BlockFilter UpdateOnly(UpdatePolicy update) => new(Update: Some(update));

    public Seq<InstanceDefinition> Apply(InstanceDefinitionTable table, Option<string> anchorDirectory = default, LinkRefreshPolicy? policy = null) {
        LinkRefreshPolicy refresh = policy ?? LinkRefreshPolicy.All;
        return toSeq(table).Filter(d =>
            UpdatePolicy.FromNative(native: d.UpdateType).IsLinked
            && FilterPredicates(anchorDirectory: anchorDirectory).All(predicate => predicate(d))
            && (!refresh.SkipUpToDate || d.ArchiveFileStatus != InstanceDefinitionArchiveFileStatus.LinkedFileIsUpToDate));

        IEnumerable<Func<InstanceDefinition, bool>> FilterPredicates(Option<string> anchorDirectory) => [
            _ => Archives.Case switch {
                Seq<string> paths => paths.Exists(p => ArchiveLink.Matches(stored: _.SourceArchive, candidate: p, anchorDirectory: anchorDirectory)),
                _ => true,
            },
            _ => Refs.Case switch {
                Seq<DefinitionRef> refs => refs.Exists(r => r.Matches(definition: _)),
                _ => true,
            },
            _ => Update.Case switch {
                UpdatePolicy update => UpdatePolicy.FromNative(native: _.UpdateType) == update,
                _ => true,
            },
            _ => Archive.Case switch {
                ArchiveStatus archive => ArchiveStatus.FromNative(native: _.ArchiveFileStatus) == archive,
                _ => true,
            },
        ];
    }
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
    private static readonly SearchValues<char> ForbiddenChars = SearchValues.Create(values: ['/', '\\']);

    public static Fin<DefinitionName> From(string value, Op? key = null) {
        Op op = key.OrDefault();
        return Optional(value).Map(static raw => raw.Trim()).Case switch {
            string trimmed when trimmed.Length > 0
                                && trimmed.Length <= MaxLength
                                && ModelComponent.IsValidComponentName(name: trimmed)
                                && !trimmed.AsSpan().ContainsAny(values: ForbiddenChars) =>
                Fin.Succ(value: Create(value: trimmed)),
            _ => Fin.Fail<DefinitionName>(error: op.InvalidInput()),
        };
    }

    /// Replace forbidden characters with '_' before admission (Speckle CleanBlockDefinitionName pattern).
    public static Fin<DefinitionName> Sanitize(string value, Op? key = null) {
        Op op = key.OrDefault();
        string cleaned = (value ?? string.Empty).Trim()
            .Replace(oldChar: '/', newChar: '_')
            .Replace(oldChar: '\\', newChar: '_');
        return From(value: cleaned, key: op);
    }
}

[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public readonly partial struct DefinitionPrefix {
    public const int MaxLength = DefinitionName.MaxLength;
    private static readonly SearchValues<char> ForbiddenChars = SearchValues.Create(values: ['/', '\\']);

    public static Fin<DefinitionPrefix> From(string value, Op? key = null) {
        Op op = key.OrDefault();
        return Optional(value).Map(static raw => raw.Trim()).Case switch {
            string trimmed when trimmed.Length > 0
                                && trimmed.Length <= MaxLength
                                && !trimmed.AsSpan().ContainsAny(values: ForbiddenChars) =>
                Fin.Succ(value: Create(value: trimmed)),
            _ => Fin.Fail<DefinitionPrefix>(error: op.InvalidInput()),
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

/// Resolved archive link: absolute path for IO plus optional relative stored in 3dm for relocation.
public readonly record struct ArchiveLink(ArchivePath Full, Option<string> Relative) {
    public string Stored => Relative.IfNone(noneValue: Full.Value);

    public Fin<FileEndpoint> ToEndpoint() =>
        FileEndpoint.From(path: Full.Value, relative: Relative);

    public static Fin<ArchiveLink> Resolve(string raw, Option<string> anchorDirectory, Op? key = null) {
        Op op = key.OrDefault();
        return Optional(raw).Map(static value => value.Trim()).Case switch {
            string trimmed when trimmed.Length == 0 => Fin.Fail<ArchiveLink>(error: op.InvalidInput()),
            string trimmed when IOPath.IsPathFullyQualified(path: trimmed) =>
                from full in ArchivePath.From(value: IOPath.GetFullPath(path: trimmed), key: op)
                let relative = anchorDirectory.Bind(dir => RelativeToAnchor(anchor: dir, full: full.Value))
                select new ArchiveLink(Full: full, Relative: relative),
            string relative when anchorDirectory.Case is string anchor =>
                from full in ArchivePath.From(value: IOPath.GetFullPath(path: IOPath.Combine(path1: anchor, path2: relative)), key: op)
                select new ArchiveLink(Full: full, Relative: Some(value: relative)),
            string relative when IOPath.IsPathFullyQualified(path: IOPath.GetFullPath(path: relative)) =>
                from full in ArchivePath.From(value: IOPath.GetFullPath(path: relative), key: op)
                select new ArchiveLink(Full: full, Relative: Option<string>.None),
            _ => Fin.Fail<ArchiveLink>(error: op.InvalidInput()),
        };
    }

    public static bool Matches(string? stored, string candidate, Option<string> anchorDirectory) {
        Op key = Op.Of(name: nameof(Matches));
        return Resolve(raw: stored ?? string.Empty, anchorDirectory: anchorDirectory, key: key).Match(
            Succ: storedLink => Resolve(raw: candidate, anchorDirectory: anchorDirectory, key: key).Match(
                Succ: candidateLink => string.Equals(a: storedLink.Full.Value, b: candidateLink.Full.Value, comparisonType: StringComparison.OrdinalIgnoreCase),
                Fail: _ => string.Equals(a: stored ?? string.Empty, b: candidate, comparisonType: StringComparison.OrdinalIgnoreCase)),
            Fail: _ => string.Equals(a: stored ?? string.Empty, b: candidate, comparisonType: StringComparison.OrdinalIgnoreCase));
    }

    private static Option<string> RelativeToAnchor(string anchor, string full) {
        string rel = IOPath.GetRelativePath(relativeTo: anchor, path: full);
        return rel.StartsWith(value: "..", comparisonType: StringComparison.Ordinal) || IOPath.IsPathFullyQualified(path: rel)
            ? Option<string>.None
            : Some(value: rel);
    }
}

[ValueObject<ulong>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct BlockContentHash {
    public static BlockContentHash Of(Members.Provided members) {
        ArgumentNullException.ThrowIfNull(argument: members);
        Seq<GeometryBase> geometry = members.Geometry;
        Seq<ObjectAttributes> attributes = members.Attributes;
        ulong geometryHash = geometry.Fold(initialState: Fnv64.Offset, f: HashGeometry);
        ulong attributeHash = attributes.Fold(initialState: geometryHash, f: static (acc, a) => Fnv64.Mix(acc: acc, v: HashAttributes(a: a)));
        return Create(value: Fnv64.Mix(acc: attributeHash, v: unchecked((ulong)geometry.Count ^ ((ulong)attributes.Count << 32))));
    }

    private static ulong HashGeometry(ulong acc, GeometryBase? geometry) =>
        geometry switch {
            null => acc,
            InstanceReferenceGeometry r => Fnv64.Mix(acc: Fnv64.Mix(acc: acc, v: HashGuid(value: r.ParentIdefId)), v: XformHash(x: r.Xform)),
            _ => Fnv64.Mix(acc: Fnv64.Mix(acc: acc, v: BoundsAndKindHash(geometry: geometry)), v: geometry.DataCRC(currentRemainder: 0u)),
        };

    private static ulong HashAttributes(ObjectAttributes? a) =>
        a switch {
            null => 0UL,
            _ => HashAttributeCore(a: a),
        };

    private static ulong XformHash(Transform x) =>
        Seq(
                x.M00, x.M01, x.M02, x.M03,
                x.M10, x.M11, x.M12, x.M13,
                x.M20, x.M21, x.M22, x.M23,
                x.M30, x.M31, x.M32, x.M33)
            .Fold(initialState: Fnv64.Offset, f: static (acc, value) => Fnv64.Mix(acc: acc, v: BitConverter.DoubleToUInt64Bits(value: value)));

    private static ulong BoundsAndKindHash(GeometryBase geometry) {
        BoundingBox box = geometry.GetBoundingBox(accurate: true);
        return Seq(
                (double)geometry.ObjectType,
                box.Min.X, box.Min.Y, box.Min.Z,
                box.Max.X, box.Max.Y, box.Max.Z)
            .Fold(initialState: Fnv64.Offset, f: static (acc, value) => Fnv64.Mix(acc: acc, v: BitConverter.DoubleToUInt64Bits(value: value)));
    }

    private static ulong HashAttributeCore(ObjectAttributes a) =>
        Seq(
                unchecked((uint)a.LayerIndex),
                unchecked((uint)a.MaterialIndex),
                unchecked((uint)a.LinetypeIndex),
                unchecked((uint)a.ObjectColor.ToArgb()),
                unchecked((uint)a.PlotColor.ToArgb()),
                BitConverter.DoubleToUInt64Bits(value: a.PlotWeight),
                unchecked((uint)a.Mode),
                unchecked((uint)a.ColorSource),
                unchecked((uint)a.MaterialSource),
                unchecked((uint)a.LinetypeSource),
                a.Visible ? 1UL : 0UL)
            .Fold(initialState: HashText(value: a.Name ?? string.Empty), f: Fnv64.Mix)
        ^ HashStrings(strings: a.GetUserStrings());

    private static ulong HashStrings(NameValueCollection strings) =>
        strings.AllKeys
            .OrderBy(key => key, comparer: StringComparer.Ordinal)
            .Aggregate(seed: Fnv64.Offset, func: (acc, key) =>
                Fnv64.Mix(acc: HashText(value: key ?? string.Empty, seed: acc), v: HashText(value: strings[key] ?? string.Empty)));

    private static ulong HashGuid(Guid value) => Fnv64.HashGuid(value: value);

    private static ulong HashText(string value, ulong seed = Fnv64.Offset) => Fnv64.HashText(value: value, seed: seed);
}

[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public readonly partial struct Axis {
    public static readonly Axis X = Create(value: "X");
    public static readonly Axis Y = Create(value: "Y");
    public static readonly Axis Z = Create(value: "Z");

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = (value ?? string.Empty).Trim().ToUpperInvariant();
        validationError = value is "X" or "Y" or "Z" ? null : new ValidationError(message: "Axis must be X|Y|Z (case-insensitive).");
    }
}

[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public readonly partial struct SnapshotName {
    public const int MaxLength = 128;
    private static readonly SearchValues<char> ForbiddenChars =
        SearchValues.Create(values: ['/', '\\', '"', '\'', '\n', '\r', '=']);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = (value ?? string.Empty).Trim();
        validationError = value.Length is > 0 and <= MaxLength && !value.AsSpan().ContainsAny(values: ForbiddenChars)
            ? null
            : new ValidationError(message: "SnapshotName 1..128 chars, no /\\\"'=\\n\\r.");
    }
}

// --- [MODELS] [REF] -----------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "state")]
public abstract partial record DefinitionRef {
    private DefinitionRef() { }
    public sealed record ById(DefinitionId Id) : DefinitionRef;
    public sealed record ByIndex(DefinitionIndex Index) : DefinitionRef;
    public sealed record ByName(DefinitionName Name) : DefinitionRef;

    public static DefinitionRef Of(DefinitionId id) => new ById(Id: id);
    public static DefinitionRef Of(DefinitionIndex index) => new ByIndex(Index: index);
    public static DefinitionRef Of(DefinitionName name) => new ByName(Name: name);

    public bool Matches(InstanceDefinition definition) =>
        Switch(definition,
            byId: static (d, r) => d.Id == r.Id.Value,
            byIndex: static (d, r) => d.Index == r.Index.Value,
            byName: static (d, r) => string.Equals(a: d.Name, b: r.Name.Value, comparisonType: StringComparison.OrdinalIgnoreCase));
}

// --- [MODELS] [MEMBERS] -------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "state")]
public abstract partial record Members {
    private Members() { }
    public sealed record Provided(Seq<GeometryBase> Geometry, Seq<ObjectAttributes> Attributes) : Members;
    public sealed record FromDocument(Seq<Guid> Sources) : Members;
    public sealed record FromConstruction(Seq<GeometryBase> Geometry, Seq<ObjectAttributes> Attributes) : Members;

    internal static ObjectAttributes SanitizeAttributes(ObjectAttributes attributes) {
        ObjectAttributes copy = attributes.Duplicate();
        copy.ObjectId = Guid.Empty;
        return copy;
    }

    public static Fin<Provided> OfProvided(Seq<GeometryBase> geometry, Seq<ObjectAttributes>? attributes = null, Op? key = null) {
        Op op = key.OrDefault();
        Seq<ObjectAttributes> attrs = (attributes ?? geometry.Map(static _ => new ObjectAttributes()))
            .Map(static a => SanitizeAttributes(attributes: a));
        return (geometry.IsEmpty, geometry.Count == attrs.Count, geometry.Exists(static g => g is null)) switch {
            (false, true, false) => Fin.Succ(value: new Provided(Geometry: geometry, Attributes: attrs)),
            _ => Fin.Fail<Provided>(error: op.InvalidInput()),
        };
    }

    public static Fin<Members> Of(Seq<GeometryBase> geometry, Seq<ObjectAttributes>? attributes = null, Op? key = null) =>
        OfProvided(geometry: geometry, attributes: attributes, key: key).Map(static p => (Members)p);

    public static Members From(Seq<Guid> sources) => new FromDocument(Sources: sources);
}

// --- [MODELS] [SPECS] ---------------------------------------------------------------------
public sealed record MetadataPatch(
    Option<string> Description = default,
    Option<ArchivePath> Url = default,
    Option<string> UrlDescription = default,
    HashMap<string, string> UserStrings = default,
    Option<ArchivableDictionary> UserDictionary = default) {
    public static MetadataPatch Empty { get; } = new();
    public bool IsEmpty =>
        Description.IsNone && Url.IsNone && UrlDescription.IsNone && UserStrings.IsEmpty && UserDictionary.IsNone;

    public Seq<BlockDiagnostic> ModifyDiagnostics(DefinitionId id) =>
        UserStrings.IsEmpty ? Seq<BlockDiagnostic>() : Seq<BlockDiagnostic>(new BlockDiagnostic.SilentUserStringMutation(Id: id));
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

    public Fin<AuthorSpec> Admit(Op key) =>
        Of(name: Name, basePoint: BasePoint, update: Update, layer: Layer, metadata: Metadata, key: key);

    private static readonly (Func<AuthorSpec, InstanceDefinition, DefinitionId, bool> When, Func<DefinitionId, AuthorSpec, InstanceDefinition, BlockDiagnostic> Make)[] CreateDiagnosticRules = [
        (static (spec, _, _) => !spec.Metadata.UserStrings.IsEmpty, static (id, _, _) => new BlockDiagnostic.SilentUserStringMutation(Id: id)),
        (static (spec, _, _) => spec.Update != UpdatePolicy.Static, static (id, spec, _) => new BlockDiagnostic.SourceArchiveRequired(Id: id, Requested: spec.Update)),
        (static (spec, live, _) => live.LayerStyle != spec.Layer.Native, static (id, _, live) => new BlockDiagnostic.LinkedSetterIgnored(Id: id, Actual: UpdatePolicy.FromNative(native: live.UpdateType))),
    ];

    public Seq<BlockDiagnostic> CreateDiagnostics(DefinitionId id, InstanceDefinition live) =>
        toSeq(CreateDiagnosticRules)
            .Filter(rule => rule.When(this, live, id))
            .Map(rule => rule.Make(id, this, live));
}

public sealed record PreviewSpec(
    int Width = 256,
    int Height = 256,
    DisplayModeRef? DisplayMode = null,
    DefinedViewportProjection Projection = DefinedViewportProjection.Perspective,
    IsometricCamera Camera = IsometricCamera.Northeast,
    bool DrawDecorations = false,
    bool ApplyDpiScaling = false,
    Option<Guid> HighlightMemberId = default) {

    public static PreviewSpec Default { get; } = new();

    public DisplayModeRef ResolvedMode => DisplayMode ?? DisplayModeRef.Wireframe;

    public PreviewFingerprint Fingerprint => new(Value: Seq(
            (ulong)Width,
            (ulong)Height,
            (ulong)(int)Projection,
            (ulong)(int)Camera,
            DrawDecorations ? 1UL : 0UL,
            ApplyDpiScaling ? 1UL : 0UL,
            DisplayHash(mode: ResolvedMode),
            HighlightMemberId.Map(static id => Fnv64.HashGuid(value: id, seed: 3UL)).IfNone(noneValue: 0UL))
        .Fold(initialState: Fnv64.Offset, f: Fnv64.Mix));

    public Fin<PreviewSpec> Admit(Op key) =>
        Of(Width, Height, DisplayMode, Projection, Camera, DrawDecorations, ApplyDpiScaling, HighlightMemberId, key);

    public static Fin<PreviewSpec> Of(
        int width, int height,
        DisplayModeRef? displayMode = null,
        DefinedViewportProjection projection = DefinedViewportProjection.Perspective,
        IsometricCamera camera = IsometricCamera.Northeast,
        bool drawDecorations = false, bool applyDpiScaling = false,
        Option<Guid> highlightMemberId = default,
        Op? key = null) =>
        (width, height) switch {
            ( >= 16 and <= 4096, >= 16 and <= 4096) => Fin.Succ(value: new PreviewSpec(
                Width: width, Height: height,
                DisplayMode: displayMode,
                Projection: projection, Camera: camera,
                DrawDecorations: drawDecorations, ApplyDpiScaling: applyDpiScaling,
                HighlightMemberId: highlightMemberId)),
            _ => Fin.Fail<PreviewSpec>(error: key.OrDefault().InvalidInput()),
        };

    private static ulong DisplayHash(DisplayModeRef mode) =>
        mode.Switch(
            wireframeCase: static _ => Fnv64.HashText(value: "wireframe"),
            byId: static r => Fnv64.HashGuid(value: r.Id, seed: 1UL),
            byName: static r => Fnv64.HashText(value: r.Name.ToUpperInvariant(), seed: 2UL));
}

public sealed record BlockSubscriptionPolicy(
    Option<Func<BlockTableEvent, bool>> Filter = default,
    bool DeferToIdle = true) : Monoid<BlockSubscriptionPolicy> {

    public static BlockSubscriptionPolicy Default { get; } = new();
    public static BlockSubscriptionPolicy Immediate { get; } = new(DeferToIdle: false);
    /// Mutation and content events only — excludes `Sorted` (UI sort-index reorder; no RhinoCommon `Sort()` API).
    public static BlockSubscriptionPolicy MutationsOnly { get; } = new(
        Filter: Some<Func<BlockTableEvent, bool>>(value: static e => e.Kind != InstanceDefinitionTableEventType.Sorted));
    public static BlockSubscriptionPolicy Empty => Default;
    public BlockSubscriptionPolicy Combine(BlockSubscriptionPolicy rhs) => this | rhs;

    /// Identity = Default; filter conjunction commutes.
    public static BlockSubscriptionPolicy operator |(BlockSubscriptionPolicy a, BlockSubscriptionPolicy b) {
        ArgumentNullException.ThrowIfNull(argument: a);
        ArgumentNullException.ThrowIfNull(argument: b);
        return new(Filter: (a.Filter, b.Filter) switch {
            ( { IsSome: true, Case: Func<BlockTableEvent, bool> af }, { IsSome: true, Case: Func<BlockTableEvent, bool> bf }) =>
                Some<Func<BlockTableEvent, bool>>(value: e => af(arg: e) && bf(arg: e)),
            ( { IsSome: true }, _) => a.Filter,
            (_, { IsSome: true }) => b.Filter,
            _ => Option<Func<BlockTableEvent, bool>>.None,
        },
            DeferToIdle: a.DeferToIdle || b.DeferToIdle);
    }
}

public readonly record struct BatchPolicy(bool SuppressRedraw = true) {
    public static BatchPolicy Default { get; } = new(SuppressRedraw: true);
}

public readonly record struct WatchPolicy(TimeSpan Debounce, TimeProvider? Clock = null) {
    public static WatchPolicy Default { get; } = new(Debounce: TimeSpan.FromMilliseconds(value: 500));
    public TimeProvider EffectiveClock => Clock ?? TimeProvider.System;
    public Fin<WatchPolicy> Admit(Op key) =>
        Debounce > TimeSpan.Zero && Debounce <= TimeSpan.FromMinutes(value: 5)
            ? Fin.Succ(value: this)
            : Fin.Fail<WatchPolicy>(error: key.InvalidInput());
    public static Fin<WatchPolicy> Of(TimeSpan debounce, Op? key = null) =>
        new WatchPolicy(Debounce: debounce).Admit(key: key.OrDefault());
}

public readonly record struct DepthPolicy(int MaxDepth = 8, bool StopOnCycle = false) {
    public static DepthPolicy Flatten { get; } = new(MaxDepth: 8);
    public static DepthPolicy Reach { get; } = new(MaxDepth: 8, StopOnCycle: true);
    public static DepthPolicy Default { get; } = Flatten;
    public Fin<DepthPolicy> Admit(Op key) =>
        MaxDepth is >= 1 and <= 64
            ? Fin.Succ(value: this)
            : Fin.Fail<DepthPolicy>(error: key.InvalidInput());
    public static Fin<DepthPolicy> Of(int maxDepth, bool stopOnCycle = false, Op? key = null) =>
        new DepthPolicy(MaxDepth: maxDepth, StopOnCycle: stopOnCycle).Admit(key: key.OrDefault());
}

public readonly record struct BoundsPolicy(bool Accurate = true, bool ExpandNested = false) {
    public static BoundsPolicy Default { get; } = new(Accurate: true);
}

public readonly record struct BakePolicy(
    ConflictPolicy? Conflict = null,
    bool RestoreInstances = true,
    LinkCreatePolicy? Link = null) {
    public static BakePolicy Default { get; } = new(Conflict: ConflictPolicy.Rename);
    public ConflictPolicy EffectiveConflict => Conflict ?? ConflictPolicy.Rename;
    public LinkCreatePolicy EffectiveLink => Link ?? LinkCreatePolicy.Default;

    public Fin<MutationReceipt> RestoreInstancesWhen(Func<Fin<MutationReceipt>> place) {
        ArgumentNullException.ThrowIfNull(argument: place);
        return RestoreInstances ? place() : Fin.Succ(value: MutationReceipt.Empty);
    }
}

public readonly record struct LinkCreatePolicy(
    UpdatePolicy? Update = null,
    LayerStyle? Layer = null,
    LinkReloadPolicy? Reload = null) {
    public static LinkCreatePolicy Default { get; } = new(Update: UpdatePolicy.Linked, Layer: LayerStyle.Reference, Reload: LinkReloadPolicy.NestedQuiet);
    public UpdatePolicy EffectiveUpdate => Update ?? UpdatePolicy.Linked;
    public LayerStyle EffectiveLayer => Layer ?? LayerStyle.Reference;
    public LinkReloadPolicy EffectiveReload => Reload ?? LinkReloadPolicy.NestedQuiet;
    public Fin<LinkCreatePolicy> Admit(Op key) =>
        EffectiveUpdate.IsLinked && EffectiveLayer.AppliesTo(policy: EffectiveUpdate)
            ? Fin.Succ(value: this)
            : Fin.Fail<LinkCreatePolicy>(error: key.InvalidInput());
}

public readonly record struct Placement(
    Transform Transform,
    Option<ObjectAttributes> Attrs,
    Option<HistoryRecord> History,
    bool Reference = false) {
    public static Placement Of(Transform xform, bool reference = false) =>
        new(Transform: xform, Attrs: Option<ObjectAttributes>.None, History: Option<HistoryRecord>.None, Reference: reference);
    public static Placement Of(Transform xform, ObjectAttributes attrs, bool reference = false) =>
        new(Transform: xform, Attrs: Some(attrs), History: Option<HistoryRecord>.None, Reference: reference);
    public static Placement Of(Transform xform, ObjectAttributes attrs, HistoryRecord history, bool reference = false) =>
        new(Transform: xform, Attrs: Some(attrs), History: Some(history), Reference: reference);

    public Fin<Placement> Admit(LiveStats live, Op key) =>
        !Reference || ReferenceAdmits(live: live)
            ? Fin.Succ(value: this)
            : Fin.Fail<Placement>(error: key.InvalidInput());

    private static bool ReferenceAdmits(LiveStats live) =>
        live.IsReference
        || (live.Update == UpdatePolicy.Linked && live.Layer == LayerStyle.Reference);
}

public sealed record LinkMap(
    DefinitionRef Ref,
    FileEndpoint Source,
    UpdatePolicy? Update = null,
    LinkReloadPolicy? Reload = null) {
    public UpdatePolicy EffectiveUpdate => Update ?? UpdatePolicy.Linked;
    public LinkReloadPolicy EffectiveReload => Reload ?? LinkReloadPolicy.NestedQuiet;

    public Fin<LinkMap> Admit(Op key) =>
        EffectiveUpdate.IsLinked
            ? Fin.Succ(value: this)
            : Fin.Fail<LinkMap>(error: key.InvalidInput());
}

// --- [MODELS] [PROJECTIONS] ---------------------------------------------------------------
public readonly record struct LiveStats(
    UnitSystem Units,
    UpdatePolicy Update,
    LayerStyle Layer,
    ArchiveStatus Archive,
    bool IsDeleted,
    bool IsTenuous,
    bool IsReference,
    bool SkipNestedLinked,
    Option<string> DeletedName,
    int MemberCount,
    int UseCountTop,
    int UseCountNested) {

    internal static LiveStats From(InstanceDefinition active) {
        _ = active.UseCount(topLevelReferenceCount: out int top, nestedReferenceCount: out int nested);
        return new LiveStats(
            Units: active.UnitSystem,
            Update: UpdatePolicy.FromNative(native: active.UpdateType),
            Layer: LayerStyle.FromNative(native: active.LayerStyle),
            Archive: ArchiveStatus.FromNative(native: active.ArchiveFileStatus),
            IsDeleted: active.IsDeleted,
            IsTenuous: active.IsTenuous,
            IsReference: active.IsReference,
            SkipNestedLinked: active.SkipNestedLinkedDefinitions,
            DeletedName: Definition.NonBlank(value: active.DeletedName),
            MemberCount: active.ObjectCount,
            UseCountTop: top,
            UseCountNested: nested);
    }
}

public sealed record Definition(
    DefinitionId Id,
    Option<DefinitionIndex> Index,
    DefinitionName Name,
    Option<string> Description,
    Option<ArchivePath> Url,
    Option<string> UrlDescription,
    Option<ArchiveLink> Source,
    ImmutableArray<Guid> MemberIds,
    Option<LiveStats> Live) {

    public bool IsArchiveOnly => Live.IsNone;
    public bool IsLinked => Source.IsSome;

    public Graph.Node ToAuditNode() => new(
        Id: Id,
        Name: Name,
        Members: MemberIds,
        Update: Live.Map(static live => live.Update).IfNone(UpdatePolicy.Static),
        Archive: Live.Map(static live => live.Archive).IfNone(ArchiveStatus.NotALinked),
        Source: Source);

    /// Diagnostics fold = InvalidLayerStyle (when layer/update mismatch) + LinkedArchiveIssue (when source missing or broken).
    public Option<LinkedHealth> ToLinkedHealth() =>
        Live.Map(live => new LinkedHealth(
            Id: Id, Name: Name, Source: Source,
            Update: live.Update, Layer: live.Layer, Archive: live.Archive, Live: live,
            Diagnostics: (live.Layer.AppliesTo(policy: live.Update) ? Seq<BlockDiagnostic>() : Seq<BlockDiagnostic>(new BlockDiagnostic.InvalidLayerStyle(Id: Id, Update: live.Update, Layer: live.Layer)))
                + (Source.IsNone || live.Archive.IsBroken ? Seq<BlockDiagnostic>(new BlockDiagnostic.LinkedArchiveIssue(Id: Id, Status: live.Archive)) : Seq<BlockDiagnostic>())));

    /// `InstanceDefinition : InstanceDefinitionGeometry`; one factory discriminates on runtime type.
    /// Live arm validates Index + projects LiveStats; archive arm leaves both as None.
    public static Fin<Definition> From(InstanceDefinitionGeometry definition, Option<string> anchorDirectory = default, Op? key = null) {
        Op op = key.OrDefault();
        InstanceDefinition? active = definition as InstanceDefinition;
        return Optional(definition).ToFin(Fail: op.InvalidInput())
            .Bind(d => from id in DefinitionId.From(value: d.Id, key: op)
                       from name in DefinitionName.From(value: d.Name ?? string.Empty, key: op)
                       from idx in active is null
                           ? Fin.Succ(value: Option<DefinitionIndex>.None)
                           : DefinitionIndex.From(value: active.Index, key: op).Map(Some)
                       select new Definition(
                           Id: id,
                           Index: idx,
                           Name: name,
                           Description: NonBlank(value: d.Description),
                           Url: NonBlank(value: d.Url).Bind(v => ArchivePath.From(value: v, key: op).ToOption()),
                           UrlDescription: NonBlank(value: d.UrlDescription),
                           Source: (NonBlank(value: d.SourceArchive) | NonBlank(value: d.Url))
                               .Bind(v => ArchiveLink.Resolve(raw: v, anchorDirectory: anchorDirectory, key: op).ToOption()),
                           MemberIds: d.GetObjectIds() is Guid[] ids ? [.. ids] : [],
                           Live: active is null ? Option<LiveStats>.None : Some(value: LiveStats.From(active: active))));
    }

    internal static Option<string> NonBlank(string? value) =>
        Optional(value).Filter(static s => !string.IsNullOrWhiteSpace(value: s));
}

public readonly record struct Member(DefinitionId DefId, Guid MemberId, Option<ObjectAttributes> Attrs);

[StructLayout(LayoutKind.Auto)]
public readonly record struct ReachInsert(
    Guid InstanceId,
    DefinitionId DefId,
    Transform WorldXform,
    int Depth,
    ImmutableArray<DefinitionId> Path);

public readonly record struct Insert(
    Guid InstanceId,
    DefinitionId DefId,
    Transform InstanceXform,
    Option<Guid> SelectedPartId) {
    public Point3d InsertionPoint => InstanceXform * Point3d.Origin;

    public static Fin<Insert> From(InstanceObject instance, Option<Guid> selectedPart = default, Op? key = null) {
        Op op = key.OrDefault();
        return Optional(instance).ToFin(Fail: op.InvalidInput()).Bind(active =>
            Optional(active.InstanceDefinition).ToFin(Fail: op.InvalidResult()).Bind(def =>
                DefinitionId.From(value: def.Id, key: op).Map(id => new Insert(
                    InstanceId: active.Id,
                    DefId: id,
                    InstanceXform: active.InstanceXform,
                    SelectedPartId: selectedPart))));
    }
}

public readonly record struct BlockTableEvent(
    InstanceDefinitionTableEventType Kind,
    int Index,
    Option<Definition> New,
    Option<Definition> Old) {
    /// `Sorted` reflects UI sort-index changes only; RhinoCommon exposes no public `InstanceDefinitionTable.Sort()`.

    public static BlockTableEvent From(InstanceDefinitionTableEventArgs args) {
        ArgumentNullException.ThrowIfNull(argument: args);
        // OldState's native ptr invalidates after callback; project inside.
        Option<string> anchor = Optional(args.Document).Bind(static doc => BlockPaths.DocAnchor(document: doc));
        return new BlockTableEvent(
            Kind: args.EventType,
            Index: args.InstanceDefinitionIndex,
            New: Optional(args.NewState).Bind(d => Definition.From(definition: d, anchorDirectory: anchor).ToOption()),
            Old: Optional(args.OldState).Bind(g => Definition.From(definition: g, anchorDirectory: anchor).ToOption()));
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct FlatPiece(GeometryBase Geometry, Transform Composed, ImmutableArray<DefinitionId> Path);

[StructLayout(LayoutKind.Auto)]
public readonly record struct AttributeCell(
    DefinitionId DefId,
    DefinitionName DefName,
    Option<Guid> InstanceId,
    string Key,
    string Prompt,
    string DefaultValue);

// --- [MODELS] [GRAPH] ---------------------------------------------------------------------
[Union]
public abstract partial record EdgeTarget {
    private EdgeTarget() { }
    public sealed record ObjectId(Guid Id) : EdgeTarget;
    public sealed record ArchiveTarget(ArchivePath Path) : EdgeTarget;
}

public sealed record Graph(ImmutableArray<Graph.Node> Nodes, ImmutableArray<Graph.Edge> Edges) {
    public static Graph Empty { get; } = new(Nodes: [], Edges: []);

    public sealed record Node(DefinitionId Id, DefinitionName Name, ImmutableArray<Guid> Members, UpdatePolicy Update, ArchiveStatus Archive, Option<ArchiveLink> Source);
    public sealed record Edge(DefinitionId From, BlockEdgeKind Kind, EdgeTarget To);
}

public sealed record LinkedHealth(
    DefinitionId Id,
    DefinitionName Name,
    Option<ArchiveLink> Source,
    UpdatePolicy Update,
    LayerStyle Layer,
    ArchiveStatus Archive,
    LiveStats Live,
    Seq<BlockDiagnostic> Diagnostics);

public sealed record RefreshPlan(Seq<LinkedHealth> Items) {
    public Seq<LinkedHealth> Refreshable => Items.Filter(static item => item.Update.IsLinked && item.Source.IsSome && item.Archive.CanRefresh);
    public Seq<LinkedHealth> Blocked => Items.Filter(static item => !item.Diagnostics.IsEmpty);
}

// --- [MODELS] [RESULTS] -------------------------------------------------------------------
public sealed record MutationReceipt(DocumentReceipt Document, Seq<BlockDiagnostic> Diagnostics) : Monoid<MutationReceipt> {
    public static MutationReceipt Empty { get; } = new(Document: DocumentReceipt.Empty, Diagnostics: Seq<BlockDiagnostic>());

    public MutationReceipt Combine(MutationReceipt rhs) => this + rhs;

    public static MutationReceipt Of(DocumentReceipt receipt) => new(Document: receipt, Diagnostics: Seq<BlockDiagnostic>());
    public static MutationReceipt Of(DocumentReceipt receipt, Seq<BlockDiagnostic> diagnostics) => new(Document: receipt, Diagnostics: diagnostics);
    public static MutationReceipt Named(string name) => Of(receipt: DocumentReceipt.Empty with {
        ResourceChanged = Seq(new DocumentResourceChange(Kind: DocumentResourceKind.Block, Name: name)),
    });
    public static MutationReceipt Lifecycle(Guid id, string name) => Of(receipt: DocumentReceipt.Empty with {
        LifecycleChanged = Seq(id),
        ResourceChanged = Seq(new DocumentResourceChange(Kind: DocumentResourceKind.Block, Name: name)),
    });

    public bool HasDiagnostics => !Diagnostics.IsEmpty;

    public static MutationReceipt operator +(MutationReceipt a, MutationReceipt b) {
        ArgumentNullException.ThrowIfNull(argument: a);
        ArgumentNullException.ThrowIfNull(argument: b);
        return new(Document: a.Document + b.Document, Diagnostics: a.Diagnostics + b.Diagnostics);
    }
}

[Union]
public abstract partial record BlockOutcome {
    private BlockOutcome() { }
    public sealed record Receipt(MutationReceipt Value) : BlockOutcome;
    public sealed record Snapshot(Definition Value) : BlockOutcome;
    public sealed record Snapshots(Seq<Definition> Values) : BlockOutcome;
    public sealed record MembersResult(Seq<Member> Values) : BlockOutcome;
    public sealed record Inserts(Seq<Insert> Values) : BlockOutcome;
    public sealed record Definitions(Seq<DefinitionId> Values) : BlockOutcome;
    public sealed record Graphs(Graph Value) : BlockOutcome;
    public sealed record Name(DefinitionName Value) : BlockOutcome;
    public sealed record Texts(HashMap<string, string> Fields) : BlockOutcome;
    public sealed record Attributes(Seq<InstanceAttributeField> Values) : BlockOutcome;
    public sealed record AttributeMatrix(Seq<AttributeCell> Values) : BlockOutcome;
    public sealed record Preview(PreviewHandle Handle) : BlockOutcome;
    public sealed record Pieces(Seq<FlatPiece> Values) : BlockOutcome;
    public sealed record Probed(DependencyProbe Value) : BlockOutcome;
    public sealed record Refresh(RefreshPlan Value) : BlockOutcome;
    public sealed record Bounds(BoundingBox Value) : BlockOutcome;
    public sealed record TableStats(int Count, int ActiveCount) : BlockOutcome;
    public sealed record Plan(Seq<DefinitionId> Order) : BlockOutcome;
    public sealed record ReachInserts(Seq<ReachInsert> Values) : BlockOutcome;
}

public sealed class PreviewHandle(Bitmap bitmap, Action<PreviewHandle> release) : IDisposable {
    private readonly Action<PreviewHandle> release = release ?? throw new ArgumentNullException(paramName: nameof(release));
    private bool disposed;

    public Bitmap Bitmap { get; } = bitmap ?? throw new ArgumentNullException(paramName: nameof(bitmap));

    public void Dispose() {
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
    public sealed record ConflictFailed(DefinitionName Name, DefinitionId Existing) : BlockDiagnostic;
    public sealed record ReusedExisting(DefinitionId Existing) : BlockDiagnostic;
    public sealed record LinkedSetterIgnored(DefinitionId Id, UpdatePolicy Actual) : BlockDiagnostic;
    public sealed record ExplodePartial(int Requested, int Received) : BlockDiagnostic;
    public sealed record GeometryRejected(DefinitionId Id, string Reason) : BlockDiagnostic;
    public sealed record SourceArchiveRequired(DefinitionId Id, UpdatePolicy Requested) : BlockDiagnostic;
    public sealed record SilentUserStringMutation(DefinitionId Id) : BlockDiagnostic;
    public sealed record InvalidLayerStyle(DefinitionId Id, UpdatePolicy Update, LayerStyle Layer) : BlockDiagnostic;
    public sealed record LinkedArchiveIssue(DefinitionId Id, ArchiveStatus Status) : BlockDiagnostic;
}

