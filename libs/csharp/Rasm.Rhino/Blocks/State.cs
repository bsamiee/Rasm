using System.Buffers;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Drawing;
using System.Runtime.InteropServices;
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
    public bool IsLinked => this == Linked || this == LinkedAndEmbedded;

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

[SmartEnum<int>]
public sealed partial class DeletionPolicy {
    public static readonly DeletionPolicy ReferencesAndQuiet = new(key: 0, deleteReferences: true, quiet: true);
    public static readonly DeletionPolicy ReferencesOnly = new(key: 1, deleteReferences: true, quiet: false);
    public static readonly DeletionPolicy QuietOnly = new(key: 2, deleteReferences: false, quiet: true);
    public static readonly DeletionPolicy Strict = new(key: 3, deleteReferences: false, quiet: false);

    public bool DeleteReferences { get; }
    public bool Quiet { get; }
}

[SmartEnum<int>]
public sealed partial class TransformPolicy {
    public static readonly TransformPolicy Copy = new(key: 0, deleteOriginal: false);
    public static readonly TransformPolicy Move = new(key: 1, deleteOriginal: true);

    public bool DeleteOriginal { get; }
}

[SmartEnum<int>]
public sealed partial class LinkRefreshPolicy {
    public static readonly LinkRefreshPolicy All = new(key: 0, skipUpToDate: false);
    public static readonly LinkRefreshPolicy Changed = new(key: 1, skipUpToDate: true);

    public bool SkipUpToDate { get; }
}

[SmartEnum<int>]
public sealed partial class LinkReloadPolicy {
    public static readonly LinkReloadPolicy NestedQuiet = new(key: 0, updateNestedLinks: true, quiet: true);
    public static readonly LinkReloadPolicy NestedVerbose = new(key: 1, updateNestedLinks: true, quiet: false);
    public static readonly LinkReloadPolicy ShallowQuiet = new(key: 2, updateNestedLinks: false, quiet: true);
    public static readonly LinkReloadPolicy ShallowVerbose = new(key: 3, updateNestedLinks: false, quiet: false);

    public bool UpdateNestedLinks { get; }
    public bool Quiet { get; }
}

[SmartEnum<int>]
public sealed partial class CompactPolicy {
    public static readonly CompactPolicy UndoAware = new(key: 0, ignoreUndoReferences: false);
    public static readonly CompactPolicy AllDeleted = new(key: 1, ignoreUndoReferences: true);

    public bool IgnoreUndoReferences { get; }
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

    public (bool SkipsHidden, Guid ViewportFilter) Resolve() =>
        Switch(
            all: static _ => (SkipsHidden: false, ViewportFilter: Guid.Empty),
            visibleOnly: static _ => (SkipsHidden: true, ViewportFilter: Guid.Empty),
            visibleIn: static v => (SkipsHidden: true, ViewportFilter: v.ViewportId));
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
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct PreviewFingerprint(ulong Value);

[Union]
public abstract partial record DependencyTarget {
    private DependencyTarget() { }
    public sealed record OnDefinition(int OtherIndex) : DependencyTarget;
    public sealed record OnLayer(int LayerIndex) : DependencyTarget;
    public sealed record OnLinetype(int LinetypeIndex) : DependencyTarget;

    public Probe ProbeOn(InstanceDefinition live) =>
        Switch(live,
            onDefinition: static (d, t) => new Probe.DefinitionDepth(Depth: d.UsesDefinition(otherIdefIndex: t.OtherIndex)),
            onLayer: static (d, t) => new Probe.LayerUsed(Used: d.UsesLayer(layerIndex: t.LayerIndex)),
            onLinetype: static (d, t) => (Probe)new Probe.LinetypeUsed(Used: d.UsesLinetype(linetypeIndex: t.LinetypeIndex)));
}

[Union]
public abstract partial record Probe {
    private Probe() { }
    public sealed record DefinitionDepth(int Depth) : Probe;
    public sealed record LayerUsed(bool Used) : Probe;
    public sealed record LinetypeUsed(bool Used) : Probe;
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

[ValueObject<ulong>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct BlockContentHash {
    private const ulong FnvOffset = 0xCBF29CE484222325UL;
    private const ulong FnvPrime = 0x100000001B3UL;

    public static BlockContentHash Of(Members.Provided members) {
        ArgumentNullException.ThrowIfNull(argument: members);
        Seq<GeometryBase> geometry = members.Geometry;
        Seq<ObjectAttributes> attributes = members.Attributes;
        ulong geometryHash = geometry.Fold(initialState: FnvOffset, f: HashGeometry);
        ulong attributeHash = attributes.Fold(initialState: geometryHash, f: static (acc, a) => Mix(acc: acc, v: HashAttributes(a: a)));
        return Create(value: Mix(acc: attributeHash, v: unchecked((ulong)geometry.Count ^ ((ulong)attributes.Count << 32))));
    }

    private static ulong HashGeometry(ulong acc, GeometryBase? geometry) =>
        geometry switch {
            null => acc,
            InstanceReferenceGeometry r => Mix(acc: Mix(acc: acc, v: HashGuid(value: r.ParentIdefId)), v: XformHash(x: r.Xform)),
            _ => Mix(acc: Mix(acc: acc, v: BoundsAndKindHash(geometry: geometry)), v: geometry.DataCRC(currentRemainder: 0u)),
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
            .Fold(initialState: FnvOffset, f: static (acc, value) => Mix(acc: acc, v: BitConverter.DoubleToUInt64Bits(value: value)));

    private static ulong BoundsAndKindHash(GeometryBase geometry) {
        BoundingBox box = geometry.GetBoundingBox(accurate: true);
        return Seq(
                (double)geometry.ObjectType,
                box.Min.X, box.Min.Y, box.Min.Z,
                box.Max.X, box.Max.Y, box.Max.Z)
            .Fold(initialState: FnvOffset, f: static (acc, value) => Mix(acc: acc, v: BitConverter.DoubleToUInt64Bits(value: value)));
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
            .Fold(initialState: HashText(value: a.Name ?? string.Empty), f: Mix)
        ^ HashStrings(strings: a.GetUserStrings());

    private static ulong HashStrings(NameValueCollection strings) =>
        strings.AllKeys
            .OrderBy(key => key, comparer: StringComparer.Ordinal)
            .Aggregate(seed: FnvOffset, func: (acc, key) =>
                Mix(acc: HashText(value: key ?? string.Empty, seed: acc), v: HashText(value: strings[key] ?? string.Empty)));

    private static ulong HashText(string value, ulong seed = FnvOffset) =>
        value.Aggregate(seed: seed, func: static (acc, ch) => Mix(acc: acc, v: ch));

    private static ulong HashGuid(Guid value) =>
        value.ToByteArray().Aggregate(seed: FnvOffset, func: static (acc, b) => Mix(acc: acc, v: b));

    private static ulong Mix(ulong acc, ulong v) => unchecked((acc ^ v) * FnvPrime);
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
[Union]
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
[Union]
public abstract partial record Members {
    private Members() { }
    public sealed record Provided(Seq<GeometryBase> Geometry, Seq<ObjectAttributes> Attributes) : Members;
    public sealed record FromDocument(Seq<Guid> Sources) : Members;

    public static Fin<Provided> OfProvided(Seq<GeometryBase> geometry, Seq<ObjectAttributes>? attributes = null, Op? key = null) {
        Op op = key.OrDefault();
        Seq<ObjectAttributes> attrs = attributes ?? geometry.Map(static _ => new ObjectAttributes());
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

    public Fin<AuthorSpec> Admit(Op key) =>
        Of(name: Name, basePoint: BasePoint, update: Update, layer: Layer, metadata: Metadata, key: key);
}

public sealed record PreviewSpec(
    int Width = 256,
    int Height = 256,
    DisplayModeRef? DisplayMode = null,
    DefinedViewportProjection Projection = DefinedViewportProjection.Perspective,
    IsometricCamera Camera = IsometricCamera.Northeast,
    bool DrawDecorations = false,
    bool ApplyDpiScaling = false) {

    public static PreviewSpec Default { get; } = new();

    public DisplayModeRef ResolvedMode => DisplayMode ?? DisplayModeRef.Wireframe;

    private const ulong FingerprintOffset = 0xCBF29CE484222325UL;
    private const ulong FingerprintPrime = 0x100000001B3UL;

    public PreviewFingerprint Fingerprint => new(Value: Seq(
            (ulong)Width,
            (ulong)Height,
            (ulong)(int)Projection,
            (ulong)(int)Camera,
            DrawDecorations ? 1UL : 0UL,
            ApplyDpiScaling ? 1UL : 0UL,
            DisplayHash(mode: ResolvedMode))
        .Fold(initialState: FingerprintOffset, f: MixFingerprint));

    public Fin<PreviewSpec> Admit(Op key) =>
        Of(
            width: Width,
            height: Height,
            displayMode: DisplayMode,
            projection: Projection,
            camera: Camera,
            drawDecorations: DrawDecorations,
            applyDpiScaling: ApplyDpiScaling,
            key: key);

    public static Fin<PreviewSpec> Of(
        int width, int height,
        DisplayModeRef? displayMode = null,
        DefinedViewportProjection projection = DefinedViewportProjection.Perspective,
        IsometricCamera camera = IsometricCamera.Northeast,
        bool drawDecorations = false, bool applyDpiScaling = false,
        Op? key = null) =>
        (width, height) switch {
            ( >= 16 and <= 4096, >= 16 and <= 4096) => Fin.Succ(value: new PreviewSpec(
                Width: width, Height: height,
                DisplayMode: displayMode,
                Projection: projection, Camera: camera,
                DrawDecorations: drawDecorations, ApplyDpiScaling: applyDpiScaling)),
            _ => Fin.Fail<PreviewSpec>(error: key.OrDefault().InvalidInput()),
        };

    private static ulong DisplayHash(DisplayModeRef mode) =>
        mode switch {
            DisplayModeRef.WireframeCase => 0UL,
            DisplayModeRef.ById byId => byId.Id.ToByteArray().Aggregate(seed: 1UL, func: static (acc, value) => MixFingerprint(acc: acc, value: value)),
            DisplayModeRef.ByName byName => byName.Name.ToUpperInvariant().Aggregate(seed: 2UL, func: static (acc, ch) => MixFingerprint(acc: acc, value: ch)),
            _ => ulong.MaxValue,
        };

    private static ulong MixFingerprint(ulong acc, ulong value) =>
        unchecked((acc ^ value) * FingerprintPrime);
}

public sealed record BlockSubscriptionPolicy(
    Option<Func<BlockTableEvent, bool>> Filter = default,
    bool DeferToIdle = true) : Monoid<BlockSubscriptionPolicy> {

    public static BlockSubscriptionPolicy Default { get; } = new();
    public static BlockSubscriptionPolicy Immediate { get; } = new(DeferToIdle: false);
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

public readonly record struct WatchPolicy(TimeSpan Debounce) {
    public static WatchPolicy Default { get; } = new(Debounce: TimeSpan.FromMilliseconds(value: 500));
    public Fin<WatchPolicy> Admit(Op key) =>
        Debounce > TimeSpan.Zero && Debounce <= TimeSpan.FromMinutes(value: 5)
            ? Fin.Succ(value: this)
            : Fin.Fail<WatchPolicy>(error: key.InvalidInput());
    public static Fin<WatchPolicy> Of(TimeSpan debounce, Op? key = null) =>
        new WatchPolicy(Debounce: debounce).Admit(key: key.OrDefault());
}

public readonly record struct FlattenPolicy(int MaxDepth = 8) {
    public static FlattenPolicy Default { get; } = new(MaxDepth: 8);
    public Fin<FlattenPolicy> Admit(Op key) =>
        MaxDepth is >= 1 and <= 64
            ? Fin.Succ(value: this)
            : Fin.Fail<FlattenPolicy>(error: key.InvalidInput());
    public static Fin<FlattenPolicy> Of(int maxDepth, Op? key = null) =>
        new FlattenPolicy(MaxDepth: maxDepth).Admit(key: key.OrDefault());
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

public readonly record struct Placement(Transform Transform, Option<ObjectAttributes> Attrs, Option<HistoryRecord> History) {
    public static Placement Of(Transform xform) => new(Transform: xform, Attrs: Option<ObjectAttributes>.None, History: Option<HistoryRecord>.None);
    public static Placement Of(Transform xform, ObjectAttributes attrs) => new(Transform: xform, Attrs: Some(attrs), History: Option<HistoryRecord>.None);
    public static Placement Of(Transform xform, ObjectAttributes attrs, HistoryRecord history) => new(Transform: xform, Attrs: Some(attrs), History: Some(history));
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
    Option<ArchivePath> Source,
    ImmutableArray<Guid> MemberIds,
    Option<LiveStats> Live) {

    public bool IsArchiveOnly => Live.IsNone;
    public bool IsLinked => Source.IsSome;

    public static Fin<Definition> From(InstanceDefinition active, Op? key = null) {
        Op op = key.OrDefault();
        return Optional(active).ToFin(Fail: op.InvalidInput())
            .Bind(d => op.Catch(() => ProjectLive(active: d, op: op)));
    }

    public static Fin<Definition> From(InstanceDefinitionGeometry archive, Op? key = null) {
        Op op = key.OrDefault();
        return Optional(archive).ToFin(Fail: op.InvalidInput())
            .Bind(g => op.Catch(() => ProjectArchive(active: g, op: op)));
    }

    private static Fin<Definition> ProjectLive(InstanceDefinition active, Op op) =>
        from id in DefinitionId.From(value: active.Id, key: op)
        from idx in DefinitionIndex.From(value: active.Index, key: op)
        from name in DefinitionName.From(value: active.Name ?? string.Empty, key: op)
        let live = LiveStats.From(active: active)
        select new Definition(
            Id: id,
            Index: Some(value: idx),
            Name: name,
            Description: NonBlank(value: active.Description),
            Url: NonBlank(value: active.Url).Bind(v => ArchivePath.From(value: v, key: op).ToOption()),
            UrlDescription: NonBlank(value: active.UrlDescription),
            Source: NonBlank(value: active.SourceArchive).Bind(v => ArchivePath.From(value: v, key: op).ToOption()),
            MemberIds: active.GetObjectIds() is Guid[] arr ? [.. arr] : [],
            Live: Some(value: live));

    private static Fin<Definition> ProjectArchive(InstanceDefinitionGeometry active, Op op) =>
        from id in DefinitionId.From(value: active.Id, key: op)
        from name in DefinitionName.From(value: active.Name ?? string.Empty, key: op)
        select new Definition(
            Id: id,
            Index: Option<DefinitionIndex>.None,
            Name: name,
            Description: NonBlank(value: active.Description),
            Url: NonBlank(value: active.Url).Bind(v => ArchivePath.From(value: v, key: op).ToOption()),
            UrlDescription: NonBlank(value: active.UrlDescription),
            Source: NonBlank(value: active.SourceArchive).Bind(v => ArchivePath.From(value: v, key: op).ToOption()),
            MemberIds: active.GetObjectIds() is Guid[] ids ? [.. ids] : [],
            Live: Option<LiveStats>.None);

    internal static Option<string> NonBlank(string? value) =>
        Optional(value).Filter(static s => !string.IsNullOrWhiteSpace(value: s));
}

public readonly record struct Member(DefinitionId DefId, Guid MemberId, Option<ObjectAttributes> Attrs);

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

    public static BlockTableEvent From(InstanceDefinitionTableEventArgs args) {
        ArgumentNullException.ThrowIfNull(argument: args);
        // OldState's native ptr invalidates after callback; project inside.
        return new BlockTableEvent(
            Kind: args.EventType,
            Index: args.InstanceDefinitionIndex,
            New: Optional(args.NewState).Bind(d => Definition.From(active: d).ToOption()),
            Old: Optional(args.OldState).Bind(g => Definition.From(archive: g).ToOption()));
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct FlatPiece(GeometryBase Geometry, Transform Composed, ImmutableArray<DefinitionId> Path);

// --- [MODELS] [GRAPH] ---------------------------------------------------------------------
[Union]
public abstract partial record EdgeTarget {
    private EdgeTarget() { }
    public sealed record ObjectId(Guid Id) : EdgeTarget;
    public sealed record ArchiveTarget(ArchivePath Path) : EdgeTarget;
}

public sealed record Graph(ImmutableArray<Graph.Node> Nodes, ImmutableArray<Graph.Edge> Edges) {
    public static Graph Empty { get; } = new(Nodes: [], Edges: []);

    public sealed record Node(DefinitionId Id, DefinitionName Name, ImmutableArray<Guid> Members, UpdatePolicy Update, ArchiveStatus Archive, Option<ArchivePath> Source);
    public sealed record Edge(DefinitionId From, BlockEdgeKind Kind, EdgeTarget To);
}

public sealed record LinkedHealth(
    DefinitionId Id,
    DefinitionName Name,
    Option<ArchivePath> Source,
    UpdatePolicy Update,
    LayerStyle Layer,
    ArchiveStatus Archive,
    bool SkipNestedLinked,
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
public abstract partial record BlockResult {
    private BlockResult() { }
    public sealed record Snapshot(Definition Value) : BlockResult;
    public sealed record Snapshots(Seq<Definition> Values) : BlockResult;
    public sealed record MembersResult(Seq<Member> Values) : BlockResult;
    public sealed record Inserts(Seq<Insert> Values) : BlockResult;
    public sealed record Definitions(Seq<DefinitionId> Values) : BlockResult;
    public sealed record Graphs(Graph Value) : BlockResult;
    public sealed record Name(DefinitionName Value) : BlockResult;
    public sealed record Texts(HashMap<string, string> Fields) : BlockResult;
    public sealed record Attributes(Seq<InstanceAttributeField> Values) : BlockResult;
    public sealed record Preview(PreviewHandle Handle) : BlockResult;
    public sealed record Pieces(Seq<FlatPiece> Values) : BlockResult;
    public sealed record Probed(Probe Value) : BlockResult;
    public sealed record Refresh(RefreshPlan Value) : BlockResult;
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

