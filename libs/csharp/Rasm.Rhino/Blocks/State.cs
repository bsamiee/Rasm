using System.Buffers;
using System.Collections.Frozen;
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

// --- [TYPES] ------------------------------------------------------------------------------
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
}

[Union(SwitchMapStateParameterName = "policy")]
public abstract partial record BasePointPolicy {
    private BasePointPolicy() { }
    public sealed record Preserve() : BasePointPolicy;
    public sealed record Compensate() : BasePointPolicy;
    public sealed record StripScale() : BasePointPolicy;
    public static BasePointPolicy Default { get; } = new Preserve();
}

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

    public static BlockDiagnostic SilentUserStrings(DefinitionId id) =>
        new SilentUserStringMutation(Id: id);
}

[SmartEnum<int>]
public sealed partial class BlockEdgeKind {
    public static readonly BlockEdgeKind Member = new(key: 0);
    public static readonly BlockEdgeKind LinkedArchive = new(key: 1);
    public static readonly BlockEdgeKind InstanceInsert = new(key: 2);
}

[Union]
public abstract partial record BlockOutcome {
    private BlockOutcome() { }
    public sealed record Receipt(MutationReceipt Value) : BlockOutcome;
    public sealed record Snapshots(Seq<Definition> Values) : BlockOutcome;
    public sealed record MembersResult(Seq<Member> Values) : BlockOutcome;
    public sealed record Inserts(Seq<Insert> Values) : BlockOutcome;
    public sealed record Definitions(Seq<DefinitionId> Values) : BlockOutcome;
    public sealed record Graphs(AuditGraph Value) : BlockOutcome;
    public sealed record Name(DefinitionName Value) : BlockOutcome;
    public sealed record Texts(HashMap<string, string> Fields) : BlockOutcome;
    public sealed record Attributes(Seq<InstanceAttributeField> Values) : BlockOutcome;
    public sealed record AttributeMatrix(Seq<AttributeCell> Values) : BlockOutcome;
    public sealed record Preview(PreviewHandle Handle) : BlockOutcome;
    public sealed record Pieces(Seq<FlatPiece> Values) : BlockOutcome;
    public sealed record Probed(DependencyProbe Value) : BlockOutcome;
    public sealed record Refresh(RefreshPlan Value) : BlockOutcome;
    public sealed record Bounds(BoundingBox Value) : BlockOutcome;
    public sealed record Adopted(int Count) : BlockOutcome;
    public sealed record TableStats(int Count, int ActiveCount) : BlockOutcome;
    public sealed record Plan(Seq<DefinitionId> Order) : BlockOutcome;
    public sealed record CycleGroups(Seq<Seq<DefinitionId>> Groups) : BlockOutcome;
    public sealed record ReachInserts(Seq<ReachInsert> Values) : BlockOutcome;
    public sealed record ClosureReport(ArchiveClosureReport Value) : BlockOutcome;
}

[SmartEnum<int>]
public sealed partial class BoundsSpace {
    public static readonly BoundsSpace Definition = new(key: 0);
    public static readonly BoundsSpace Nested = new(key: 1);

    internal BoundingBox Union(InstanceDefinition live, bool accurate) =>
        this == Nested
            ? toSeq(live.GetReferences(wheretoLook: ReferenceScope.TopAndNested.Native) ?? [])
                .Filter(static i => i is not null)
                .Fold(
                    BoundingBox.Empty,
                    static (acc, inst) => BoundingBox.Union(
                        acc,
                        inst!.Geometry?.GetBoundingBox(xform: inst.InstanceXform) ?? BoundingBox.Empty))
            : DefinitionBounds(live: live, accurate: accurate);

    private static BoundingBox DefinitionBounds(InstanceDefinition live, bool accurate) {
        using InstanceReferenceGeometry proxy =
            new(instanceDefinitionId: live.Id, transform: Transform.Identity);
        BoundingBox proxyBox = proxy.GetBoundingBox(accurate: accurate);
        return proxyBox.IsValid
            ? proxyBox
            : toSeq(live.GetObjects())
                .Filter(static o => o?.Geometry is not null)
                .Map(static o => o!.Geometry!)
                .Fold(
                    BoundingBox.Empty,
                    (acc, g) => BoundingBox.Union(acc, g.GetBoundingBox(accurate: accurate)));
    }
}

[SmartEnum<int>]
public sealed partial class ConflictPolicy {
    public static readonly ConflictPolicy Replace = new(key: 0);
    public static readonly ConflictPolicy Skip = new(key: 1);
    public static readonly ConflictPolicy Fail = new(key: 2);
    public static readonly ConflictPolicy Rename = new(key: 3);
}

[SmartEnum<int>]
public sealed partial class ConstraintPolicy {
    public static readonly ConstraintPolicy Schema = new(key: 0);
    public static readonly ConstraintPolicy Extend = new(key: 1);

    internal Fin<Unit> AdmitValues(HashMap<string, string> values, Seq<InstanceAttributeField> schema, Op key) =>
        this == Schema
            ? guard(
                values
                    .Filter((k, _) => !schema.Any(field => string.Equals(a: field.Key, b: k, comparisonType: StringComparison.OrdinalIgnoreCase)))
                    .IsEmpty,
                key.InvalidInput()).ToFin()
            : Fin.Succ(value: unit);
}

[Union(SwitchMapStateParameterName = "state")]
public abstract partial record DefinitionRef {
    private DefinitionRef() { }
    public sealed record ById(DefinitionId Id) : DefinitionRef;
    public sealed record ByIndex(DefinitionIndex Index) : DefinitionRef;
    public sealed record ByName(DefinitionName Name) : DefinitionRef;
    public sealed record ByPath(ArchivePath Path) : DefinitionRef;

    public static DefinitionRef Of(DefinitionId id) => new ById(Id: id);
    public static DefinitionRef Of(DefinitionIndex index) => new ByIndex(Index: index);
    public static DefinitionRef Of(DefinitionName name) => new ByName(Name: name);
    public static DefinitionRef Of(ArchivePath path) => new ByPath(Path: path);

    public bool Matches(InstanceDefinition definition) =>
        Switch(definition,
            byId: static (d, r) => d.Id == r.Id.Value,
            byIndex: static (d, r) => d.Index == r.Index.Value,
            byName: static (d, r) => string.Equals(a: d.Name, b: r.Name.Value, comparisonType: StringComparison.OrdinalIgnoreCase),
            byPath: static (_, _) => false);
}

public enum DependencyProbeUsage { Layer, Linetype, InUse }

[Union]
public abstract partial record DependencyProbe {
    private DependencyProbe() { }
    public sealed record DefinitionDepth(int Depth) : DependencyProbe;
    public sealed record Usage(DependencyProbeUsage Kind, bool Used) : DependencyProbe;
}

[Union]
public abstract partial record DependencyTarget {
    private DependencyTarget() { }
    public sealed record OnDefinition(int OtherIndex) : DependencyTarget;
    public sealed record OnLayer(int LayerIndex) : DependencyTarget;
    public sealed record OnLinetype(int LinetypeIndex) : DependencyTarget;
    public sealed record InUse(ReferenceScope Scope) : DependencyTarget;

    public DependencyProbe ProbeOn(InstanceDefinition live) =>
        Switch(live,
            onDefinition: static (d, t) => (DependencyProbe)new DependencyProbe.DefinitionDepth(Depth: d.UsesDefinition(otherIdefIndex: t.OtherIndex)),
            onLayer: static (d, t) => new DependencyProbe.Usage(Kind: DependencyProbeUsage.Layer, Used: d.UsesLayer(layerIndex: t.LayerIndex)),
            onLinetype: static (d, t) => new DependencyProbe.Usage(Kind: DependencyProbeUsage.Linetype, Used: d.UsesLinetype(linetypeIndex: t.LinetypeIndex)),
            inUse: static (d, t) => new DependencyProbe.Usage(Kind: DependencyProbeUsage.InUse, Used: d.InUse(wheretoLook: t.Scope.Native)));
}

[Union]
public abstract partial record DisplayModeRef {
    private DisplayModeRef() { }
    public sealed record WireframeCase() : DisplayModeRef;
    public sealed record ById(Guid Id) : DisplayModeRef;
    public sealed record ByName(string Name) : DisplayModeRef;

    // Native ids P/Invoke rhcommon_c; defer the table build so touching DisplayModeRef statics never loads the native layer.
    private static readonly Lazy<FrozenDictionary<Guid, DisplayMode>> NativeModes =
        new(static () => new Dictionary<Guid, DisplayMode> {
            [DisplayModeDescription.WireframeId] = DisplayMode.Wireframe,
            [DisplayModeDescription.ShadedId] = DisplayMode.Shaded,
            [DisplayModeDescription.RenderedId] = DisplayMode.RenderPreview,
        }.ToFrozenDictionary());

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

    internal static Fin<DisplayMode> NativeMode(Guid displayId, Op op) =>
        NativeModes.Value.TryGetValue(key: displayId, value: out DisplayMode mode)
            ? Fin.Succ(value: mode)
            : Fin.Fail<DisplayMode>(error: op.InvalidInput());
}

[Union]
public abstract partial record EdgeTarget {
    private EdgeTarget() { }
    public sealed record ObjectId(Guid Id) : EdgeTarget;
    public sealed record ArchiveTarget(ArchivePath Path) : EdgeTarget;
}

[Union]
public abstract partial record ExplodePolicy {
    private ExplodePolicy() { }
    public sealed record All() : ExplodePolicy;
    public sealed record VisibleOnly() : ExplodePolicy;
    public sealed record VisibleIn(Guid ViewportId) : ExplodePolicy;
    public sealed record Native(bool ExplodeNested = true, bool DeleteInstance = true) : ExplodePolicy {
        public DocumentReceipt ExplodeReceipt(Seq<Guid> created, Guid instanceId) =>
            DocumentReceipt.Objects(slot: DocumentReceiptSlot.Created, ids: created)
            + DocumentReceipt.Objects(slot: DocumentReceiptSlot.Deleted, ids: DeleteInstance ? Seq(instanceId) : Seq<Guid>());
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

[Union(SwitchMapStateParameterName = "ctx")]
public abstract partial record GraphQuery {
    private GraphQuery() { }
    public sealed record Members(DefinitionRef Ref) : GraphQuery;
    public sealed record Inserts(DefinitionRef Ref, ReferenceScope Scope) : GraphQuery;
    public sealed record Containers(DefinitionRef Ref) : GraphQuery;
    public sealed record Depends(DefinitionRef Ref, DependencyTarget Target) : GraphQuery;
    public sealed record Audit() : GraphQuery;
    public sealed record Plan(Option<DefinitionRef> Root = default) : GraphQuery;
    public sealed record Cycles(Option<DefinitionRef> Root = default) : GraphQuery;
    public sealed record Stats() : GraphQuery;
    public sealed record Health(BlockFilter? Filter = null) : GraphQuery;
    public sealed record Reach(DefinitionRef Ref, ReferenceScope Scope, DepthPolicy? Policy = null) : GraphQuery;
    public sealed record EnsureIndexed(Option<DefinitionRef> Ref = default) : GraphQuery;
}

[SmartEnum<int>]
public sealed partial class LayerStyle {
    public static readonly LayerStyle None = new(key: 0, native: InstanceDefinitionLayerStyle.None);
    public static readonly LayerStyle Active = new(key: 1, native: InstanceDefinitionLayerStyle.Active);
    public static readonly LayerStyle Reference = new(key: 2, native: InstanceDefinitionLayerStyle.Reference);

    public InstanceDefinitionLayerStyle Native { get; }

    public static LayerStyle FromNative(InstanceDefinitionLayerStyle native) =>
        native switch {
            InstanceDefinitionLayerStyle.Active => Active,
            InstanceDefinitionLayerStyle.Reference => Reference,
            _ => None,
        };

    internal bool AppliesTo(UpdatePolicy policy) =>
        Key switch {
            0 => policy != UpdatePolicy.Linked,
            1 or 2 => policy == UpdatePolicy.Linked,
            _ => false,
        };
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

[Union(SwitchMapStateParameterName = "state")]
public abstract partial record Members {
    private Members() { }
    public sealed record Provided(Seq<GeometryBase> Geometry, Seq<ObjectAttributes> Attributes) : Members;
    public sealed record FromDocument(Seq<Guid> Sources) : Members;
    public sealed record FromConstruction(Seq<GeometryBase> Geometry, Seq<ObjectAttributes> Attributes) : Members;

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

    internal static ObjectAttributes SanitizeAttributes(ObjectAttributes attributes) {
        ObjectAttributes copy = attributes.Duplicate();
        copy.ObjectId = Guid.Empty;
        return copy;
    }
}

[SmartEnum<int>]
public sealed partial class ReferenceScope {
    public static readonly ReferenceScope Top = new(key: 0);
    public static readonly ReferenceScope TopAndNested = new(key: 1);
    public static readonly ReferenceScope FromOtherDefinitions = new(key: 2);

    public int Native => Key;
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
            (_, true) => DocumentReceipt.Objects(slot: DocumentReceiptSlot.Created, ids: Seq(resultId)),
            (true, false) => DocumentReceipt.Objects(slot: DocumentReceiptSlot.Created, ids: Seq(resultId))
                + DocumentReceipt.Objects(slot: DocumentReceiptSlot.Deleted, ids: Seq(instanceId)),
            _ => DocumentReceipt.Objects(slot: DocumentReceiptSlot.Created, ids: Seq(resultId))
                + DocumentReceipt.Objects(slot: DocumentReceiptSlot.Transformed, ids: Seq(resultId)),
        };
}

[SmartEnum<int>]
public sealed partial class UpdatePolicy {
    public static readonly UpdatePolicy Static = new(key: 0, native: InstanceDefinitionUpdateType.Static);
    public static readonly UpdatePolicy LinkedAndEmbedded = new(key: 1, native: InstanceDefinitionUpdateType.LinkedAndEmbedded);
    public static readonly UpdatePolicy Linked = new(key: 2, native: InstanceDefinitionUpdateType.Linked);
#pragma warning disable CS0618 // BOUNDARY ADAPTER — legacy documents and native APIs can still surface Embedded.
    public static readonly UpdatePolicy Embedded = new(key: 3, native: InstanceDefinitionUpdateType.Embedded);
#pragma warning restore CS0618

    public InstanceDefinitionUpdateType Native { get; }

    public static UpdatePolicy FromNative(InstanceDefinitionUpdateType native) =>
        native switch {
            InstanceDefinitionUpdateType.Linked => Linked,
            InstanceDefinitionUpdateType.LinkedAndEmbedded => LinkedAndEmbedded,
#pragma warning disable CS0618 // BOUNDARY ADAPTER — preserve obsolete native Embedded when old documents report it.
            InstanceDefinitionUpdateType.Embedded => Embedded,
#pragma warning restore CS0618
            _ => Static,
        };

    public bool IsLinked => this == Linked || this == LinkedAndEmbedded || this == Embedded;

    public Fin<Unit> RequireLinked(Op key) =>
        guard(IsLinked, key.InvalidInput()).ToFin();

    public Fin<Unit> RejectLinkedModify(Op key) =>
        guard(!IsLinked, key.InvalidInput()).ToFin();
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record ArchiveClosureReport(
    bool Valid,
    Seq<Archive.LinkedArchiveEdge> Edges,
    Seq<ArchivePath> Broken,
    Seq<Seq<ArchivePath>> Cycles,
    Seq<ArchivePath> Truncated = default,
    Seq<string> NativeLog = default,
    Seq<Archive.UnitEdge> UnitEdges = default);

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
        Op key = Op.Of();
        bool fallback = string.Equals(a: stored ?? string.Empty, b: candidate, comparisonType: StringComparison.OrdinalIgnoreCase);
        return (from storedLink in Resolve(raw: stored ?? string.Empty, anchorDirectory: anchorDirectory, key: key)
                from candidateLink in Resolve(raw: candidate, anchorDirectory: anchorDirectory, key: key)
                select string.Equals(a: storedLink.Full.Value, b: candidateLink.Full.Value, comparisonType: StringComparison.OrdinalIgnoreCase))
            .IfFail(_ => fallback);
    }

    private static Option<string> RelativeToAnchor(string anchor, string full) {
        string rel = IOPath.GetRelativePath(relativeTo: anchor, path: full);
        return rel.StartsWith(value: "..", comparisonType: StringComparison.Ordinal) || IOPath.IsPathFullyQualified(path: rel)
            ? Option<string>.None
            : Some(value: rel);
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

}

[StructLayout(LayoutKind.Auto)]
public readonly record struct AttributeCell(
    DefinitionId DefId,
    DefinitionName DefName,
    Option<Guid> InstanceId,
    string Key,
    string Prompt,
    string DefaultValue);

[ComplexValueObject(DefaultStringComparison = StringComparison.Ordinal)]
public sealed partial class AttributeFieldSpec {
    public string Key { get; }
    public string Prompt { get; }
    public string DefaultValue { get; }
    public Plane FieldPlane { get; }

    // Field expression scanned by TextFields.GetInstanceAttributeFields on the authored TextEntity member.
    internal string FieldExpression =>
        $"%<BlockAttributeText(\"{Key}\",\"{Prompt}\",\"{DefaultValue}\")>%";

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref string key,
        ref string prompt,
        ref string defaultValue,
        ref Plane fieldPlane) {
        key = (key ?? string.Empty).Trim();
        prompt = (prompt ?? string.Empty).Trim();
        defaultValue ??= string.Empty;
        validationError = key.Length is > 0 and <= 128 && prompt.Length is > 0 and <= 256 && fieldPlane.IsValid
            ? null
            : new ValidationError(message: "AttributeFieldSpec: Key 1..128 chars, Prompt 1..256 chars, FieldPlane valid.");
    }
}

public sealed record AuditGraph(ImmutableArray<AuditGraph.Node> Nodes, ImmutableArray<AuditGraph.Edge> Edges) {
    public sealed record Node(DefinitionId Id, DefinitionName Name, ImmutableArray<Guid> Members, UpdatePolicy Update, ArchiveStatus Archive, Option<ArchiveLink> Source);
    public sealed record Edge(DefinitionId From, BlockEdgeKind Kind, EdgeTarget To);
}

public sealed record AuthorSpec(
    DefinitionName Name,
    Point3d BasePoint,
    UpdatePolicy Update,
    LayerStyle Layer,
    MetadataPatch Metadata,
    Option<FileEndpoint> Source = default) {

    private static readonly (Func<AuthorSpec, InstanceDefinition, DefinitionId, bool> When, Func<DefinitionId, AuthorSpec, InstanceDefinition, BlockDiagnostic> Make)[] CreateDiagnosticRules = [
        (static (spec, _, _) => !spec.Metadata.UserStrings.IsEmpty, static (id, _, _) => BlockDiagnostic.SilentUserStrings(id: id)),
        (static (spec, live, _) => spec.Update.IsLinked && Definition.NonBlank(value: live.SourceArchive).IsNone, static (id, spec, _) => new BlockDiagnostic.SourceArchiveRequired(Id: id, Requested: spec.Update)),
        (static (spec, live, _) => live.LayerStyle != spec.Layer.Native, static (id, _, live) => new BlockDiagnostic.LinkedSetterIgnored(Id: id, Actual: UpdatePolicy.FromNative(native: live.UpdateType))),
    ];

    public static Fin<AuthorSpec> Of(
        DefinitionName name,
        Point3d basePoint,
        UpdatePolicy? update = null,
        LayerStyle? layer = null,
        Option<FileEndpoint> source = default,
        MetadataPatch? metadata = null,
        Op? key = null) {
        Op op = key.OrDefault();
        UpdatePolicy policy = update ?? UpdatePolicy.Static;
        LayerStyle style = layer ?? (policy == UpdatePolicy.Linked ? LayerStyle.Reference : LayerStyle.None);
        return (basePoint.IsValid, style.AppliesTo(policy: policy), policy.IsLinked, source.Case) switch {
            (true, true, true, FileEndpoint) or (true, true, false, not FileEndpoint) => Fin.Succ(value: new AuthorSpec(
                Name: name,
                BasePoint: basePoint,
                Update: policy,
                Layer: style,
                Metadata: metadata ?? MetadataPatch.Empty,
                Source: source)),
            _ => Fin.Fail<AuthorSpec>(error: op.InvalidInput()),
        };
    }

    public Fin<AuthorSpec> Admit(Op key) =>
        Of(name: Name, basePoint: BasePoint, update: Update, layer: Layer, source: Source, metadata: Metadata, key: key);

    public Seq<BlockDiagnostic> CreateDiagnostics(DefinitionId id, InstanceDefinition live) =>
        toSeq(CreateDiagnosticRules)
            .Choose(rule => rule.When(this, live, id)
                ? Some(value: rule.Make(id, this, live))
                : Option<BlockDiagnostic>.None);
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

public readonly record struct BatchPolicy(bool SuppressRedraw = true) {
    public static BatchPolicy Default { get; } = new(SuppressRedraw: true);
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
            InstanceReferenceGeometry r => Fnv64.Mix(acc: Fnv64.Mix(acc: acc, v: Fnv64.HashGuid(value: r.ParentIdefId)), v: XformHash(x: r.Xform)),
            _ => Fnv64.Mix(acc: Fnv64.Mix(acc: acc, v: unchecked((ulong)(int)geometry.ObjectType)), v: geometry.DataCRC(currentRemainder: 0u)),
        };

    private static ulong HashDoubles(Seq<double> values) =>
        values.Fold(initialState: Fnv64.Offset, f: static (acc, value) => Fnv64.Mix(acc: acc, v: BitConverter.DoubleToUInt64Bits(value: value)));

    private static ulong XformHash(Transform x) =>
        HashDoubles(values: Seq(x.M00, x.M01, x.M02, x.M03, x.M10, x.M11, x.M12, x.M13, x.M20, x.M21, x.M22, x.M23, x.M30, x.M31, x.M32, x.M33));

    private static ulong HashAttributes(ObjectAttributes? a) =>
        a is null ? 0UL : Seq(
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
            .Fold(initialState: Fnv64.HashText(value: a.Name ?? string.Empty), f: Fnv64.Mix)
        ^ HashStrings(strings: a.GetUserStrings());

    private static ulong HashStrings(NameValueCollection strings) =>
        strings.AllKeys
            .Order(comparer: StringComparer.Ordinal)
            .Aggregate(seed: Fnv64.Offset, func: (acc, key) =>
                Fnv64.Mix(acc: Fnv64.HashText(value: key ?? string.Empty, seed: acc), v: Fnv64.HashText(value: strings[key] ?? string.Empty)));
}

public sealed record BlockFilter(
    Option<Seq<string>> Archives = default,
    Option<Seq<DefinitionRef>> Refs = default,
    Option<UpdatePolicy> Update = default,
    Option<ArchiveStatus> Archive = default) {
    public static BlockFilter All { get; } = new();
    public static BlockFilter ArchivesOnly(Seq<string> paths) => new(Archives: Some(paths));

    public Seq<InstanceDefinition> Apply(InstanceDefinitionTable table, Option<string> anchorDirectory = default, LinkRefreshPolicy? policy = null, Option<Func<InstanceDefinition, IComparable>> orderBy = default) {
        ArgumentNullException.ThrowIfNull(argument: table);
        LinkRefreshPolicy refresh = policy ?? LinkRefreshPolicy.All;
        Seq<InstanceDefinition> matched = Definition.List(table: table)
            .Filter(d =>
            UpdatePolicy.FromNative(native: d.UpdateType).IsLinked
            && (Archives.Case is not Seq<string> paths || paths.Exists(p => ArchiveLink.Matches(stored: d.SourceArchive, candidate: p, anchorDirectory: anchorDirectory)))
            && (Refs.Case is not Seq<DefinitionRef> refs || refs.Exists(r => r.Matches(definition: d)))
            && (Update.Case is not UpdatePolicy update || UpdatePolicy.FromNative(native: d.UpdateType) == update)
            && (Archive.Case is not ArchiveStatus archive || ArchiveStatus.FromNative(native: d.ArchiveFileStatus) == archive)
            && (!refresh.SkipUpToDate || d.ArchiveFileStatus != InstanceDefinitionArchiveFileStatus.LinkedFileIsUpToDate));
        return orderBy.Case switch {
            Func<InstanceDefinition, IComparable> projection => toSeq(matched.OrderBy(d => projection(arg: d))),
            _ => matched,
        };
    }
}

public sealed record BlockSubscriptionPolicy(
    Option<Func<BlockTableEvent, bool>> Filter = default,
    bool DeferToIdle = true) : Monoid<BlockSubscriptionPolicy> {

    public static BlockSubscriptionPolicy Default { get; } = new();
    public static BlockSubscriptionPolicy Immediate { get; } = new(DeferToIdle: false);
    public static BlockSubscriptionPolicy MutationsOnly { get; } = new(
        Filter: Some<Func<BlockTableEvent, bool>>(value: static e => e.Kind != InstanceDefinitionTableEventType.Sorted));
    public static BlockSubscriptionPolicy Empty => Default;
    public BlockSubscriptionPolicy Combine(BlockSubscriptionPolicy rhs) => this | rhs;
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

public readonly record struct BlockTableEvent(
    InstanceDefinitionTableEventType Kind,
    int Index,
    Option<Definition> New,
    Option<Definition> Old) {

    public static BlockTableEvent From(InstanceDefinitionTableEventArgs args) {
        ArgumentNullException.ThrowIfNull(argument: args);
        Option<string> anchor = Optional(args.Document).Bind(static doc => BlockPaths.DocAnchor(document: doc));
        return new BlockTableEvent(
            Kind: args.EventType,
            Index: args.InstanceDefinitionIndex,
            New: Optional(args.NewState).Bind(d => Definition.From(definition: d, anchorDirectory: anchor).ToOption()),
            Old: Optional(args.OldState).Bind(g => Definition.From(definition: g, anchorDirectory: anchor).ToOption()));
    }
}

public readonly record struct BoundsPolicy(bool Accurate = true, BoundsSpace? Space = null) {
    public static BoundsPolicy Default { get; } = new(Accurate: true);
    private BoundsSpace EffectiveSpace => Space ?? BoundsSpace.Definition;

    internal BoundingBox Union(InstanceDefinition live) =>
        EffectiveSpace.Union(live: live, accurate: Accurate);
}

public readonly record struct ClosureValidationPolicy(
    bool DetectCycles,
    bool DetectUnitMismatches = false,
    int MaxDepth = Archive.LinkedArchiveClosureMaxDepth) {
    public static ClosureValidationPolicy Default { get; } = new(DetectCycles: true);
    public static ClosureValidationPolicy Full { get; } = new(DetectCycles: true, DetectUnitMismatches: true);

    public Fin<ClosureValidationPolicy> Admit(Op key) =>
        MaxDepth is >= 1 and <= Archive.LinkedArchiveClosureMaxDepth
            ? Fin.Succ(value: this)
            : Fin.Fail<ClosureValidationPolicy>(error: key.InvalidInput());
}

public readonly record struct CompactPolicy(bool IgnoreUndoReferences) {
    public static readonly CompactPolicy UndoAware = new(IgnoreUndoReferences: false);
    public static readonly CompactPolicy AllDeleted = new(IgnoreUndoReferences: true);
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
                           Source: NonBlank(value: d.SourceArchive)
                               .Bind(v => ArchiveLink.Resolve(raw: v, anchorDirectory: anchorDirectory, key: op).ToOption()),
                           MemberIds: d.GetObjectIds() is Guid[] ids ? [.. ids] : [],
                           Live: active is null ? Option<LiveStats>.None : Some(value: LiveStats.From(active: active))));
    }

    public bool IsArchiveOnly => Live.IsNone;
    public bool IsLinked => Source.IsSome;

    public AuditGraph.Node ToAuditNode() => new(
        Id: Id,
        Name: Name,
        Members: MemberIds,
        Update: Live.Map(static live => live.Update).IfNone(UpdatePolicy.Static),
        Archive: Live.Map(static live => live.Archive).IfNone(ArchiveStatus.NotALinked),
        Source: Source);

    public Option<LinkedHealth> ToLinkedHealth() =>
        Live.Map(live => new LinkedHealth(
            Id: Id, Name: Name, Source: Source,
            Update: live.Update, Layer: live.Layer, Archive: live.Archive, Live: live,
            Diagnostics: (live.Layer.AppliesTo(policy: live.Update) ? Seq<BlockDiagnostic>() : Seq<BlockDiagnostic>(new BlockDiagnostic.InvalidLayerStyle(Id: Id, Update: live.Update, Layer: live.Layer)))
                + (Source.IsNone || live.Archive.IsBroken ? Seq<BlockDiagnostic>(new BlockDiagnostic.LinkedArchiveIssue(Id: Id, Status: live.Archive)) : Seq<BlockDiagnostic>())));

    internal static Seq<InstanceDefinition> List(InstanceDefinitionTable table, bool ignoreDeleted = true) {
        ArgumentNullException.ThrowIfNull(argument: table);
        return toSeq(table.GetList(ignoreDeleted: ignoreDeleted)).Choose(static d => Optional(d));
    }

    internal static Option<string> NonBlank(string? value) =>
        Optional(value).Filter(static s => !string.IsNullOrWhiteSpace(value: s));
}

[ValueObject<Guid>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct DefinitionId {
    public static Fin<DefinitionId> From(Guid value, Op? key = null) =>
        guard(value != Guid.Empty, key.OrDefault().InvalidInput()).ToFin().Map(_ => Create(value: value));

    public static Option<DefinitionId> FromOption(Guid value) =>
        value != Guid.Empty ? Some(Create(value: value)) : Option<DefinitionId>.None;
}

[ValueObject<int>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct DefinitionIndex {
    public static Fin<DefinitionIndex> From(int value, Op? key = null) =>
        guard(value >= 0, key.OrDefault().InvalidInput()).ToFin().Map(_ => Create(value: value));
}

[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public readonly partial struct DefinitionName {
    public const int MaxLength = 256;
    internal static readonly SearchValues<char> ForbiddenChars = SearchValues.Create(values: ['/', '\\']);

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

    public static Fin<DefinitionPrefix> From(string value, Op? key = null) {
        Op op = key.OrDefault();
        return Optional(value).Map(static raw => raw.Trim()).Case switch {
            string trimmed when trimmed.Length > 0
                                && trimmed.Length <= MaxLength
                                && !trimmed.AsSpan().ContainsAny(values: DefinitionName.ForbiddenChars) =>
                Fin.Succ(value: Create(value: trimmed)),
            _ => Fin.Fail<DefinitionPrefix>(error: op.InvalidInput()),
        };
    }
}

public readonly record struct DeletionPolicy(bool DeleteReferences, bool Quiet) {
    public static readonly DeletionPolicy ReferencesAndQuiet = new(DeleteReferences: true, Quiet: true);
    public static readonly DeletionPolicy ReferencesOnly = new(DeleteReferences: true, Quiet: false);
    public static readonly DeletionPolicy QuietOnly = new(DeleteReferences: false, Quiet: true);
    public static readonly DeletionPolicy Strict = new(DeleteReferences: false, Quiet: false);
}

public readonly record struct DepthPolicy(int MaxDepth = 8, bool StopOnCycle = false) {
    public const int DefaultMaxDepth = 8;
    public const int MaxAllowedDepth = 64;
    public static DepthPolicy Flatten { get; } = new(MaxDepth: DefaultMaxDepth);
    public static DepthPolicy Reach { get; } = new(MaxDepth: DefaultMaxDepth, StopOnCycle: true);
    public static DepthPolicy Default { get; } = Flatten;
    public Fin<DepthPolicy> Admit(Op key) =>
        MaxDepth is >= 1 and <= MaxAllowedDepth
            ? Fin.Succ(value: this)
            : Fin.Fail<DepthPolicy>(error: key.InvalidInput());
    public static Fin<DepthPolicy> Of(int maxDepth, bool stopOnCycle = false, Op? key = null) =>
        new DepthPolicy(MaxDepth: maxDepth, StopOnCycle: stopOnCycle).Admit(key: key.OrDefault());
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct FlatPiece(GeometryBase Geometry, Transform Composed, ImmutableArray<DefinitionId> Path);

public readonly record struct Insert(
    Guid InstanceId,
    DefinitionId DefId,
    Transform InstanceXform,
    Point3d Insertion,
    Option<Guid> SelectedPartId) {

    public static Fin<Insert> From(InstanceObject instance, Option<Guid> selectedPart = default, Op? key = null) {
        Op op = key.OrDefault();
        return Optional(instance).ToFin(Fail: op.InvalidInput()).Bind(active =>
            Optional(active.InstanceDefinition).ToFin(Fail: op.InvalidResult()).Bind(def =>
                DefinitionId.From(value: def.Id, key: op).Map(id => new Insert(
                    InstanceId: active.Id,
                    DefId: id,
                    InstanceXform: active.InstanceXform,
                    Insertion: active.InsertionPoint,
                    SelectedPartId: selectedPart))));
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

public sealed record LinkedHealth(
    DefinitionId Id,
    DefinitionName Name,
    Option<ArchiveLink> Source,
    UpdatePolicy Update,
    LayerStyle Layer,
    ArchiveStatus Archive,
    LiveStats Live,
    Seq<BlockDiagnostic> Diagnostics);

public sealed record LinkMap(
    DefinitionRef Ref,
    FileEndpoint Source,
    UpdatePolicy? Update = null,
    LinkReloadPolicy? Reload = null) {
    public UpdatePolicy EffectiveUpdate => Update ?? UpdatePolicy.Linked;
    public LinkReloadPolicy EffectiveReload => Reload ?? LinkReloadPolicy.NestedQuiet;

    public Fin<LinkMap> Admit(Op key) =>
        guard(EffectiveUpdate.IsLinked, key.InvalidInput()).ToFin().Map(_ => this);
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
        UpdatePolicy update = UpdatePolicy.FromNative(native: active.UpdateType);
        return new LiveStats(
            Units: active.UnitSystem,
            Update: update,
            Layer: LayerStyle.FromNative(native: active.LayerStyle),
            Archive: ArchiveStatus.FromNative(native: active.ArchiveFileStatus),
            IsDeleted: active.IsDeleted,
            IsTenuous: active.IsTenuous,
            IsReference: active.IsReference,
            SkipNestedLinked: update.IsLinked && active.SkipNestedLinkedDefinitions,
            DeletedName: Definition.NonBlank(value: active.DeletedName),
            MemberCount: active.ObjectCount,
            UseCountTop: top,
            UseCountNested: nested);
    }
}

public readonly record struct Member(DefinitionId DefId, Guid MemberId, Option<ObjectAttributes> Attrs);

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
        UserStrings.IsEmpty ? Seq<BlockDiagnostic>() : Seq(BlockDiagnostic.SilentUserStrings(id: id));
}

public sealed record MutationReceipt(DocumentReceipt Document, Seq<BlockDiagnostic> Diagnostics) : Monoid<MutationReceipt> {
    public static MutationReceipt Empty { get; } = new(Document: DocumentReceipt.Empty, Diagnostics: Seq<BlockDiagnostic>());

    public static MutationReceipt Of(DocumentReceipt receipt) => new(Document: receipt, Diagnostics: Seq<BlockDiagnostic>());
    public static MutationReceipt Of(DocumentReceipt receipt, Seq<BlockDiagnostic> diagnostics) => new(Document: receipt, Diagnostics: diagnostics);
    public static MutationReceipt Resources(Seq<DocumentResourceChange> changes) =>
        Of(receipt: DocumentReceipt.Resources(changes: changes));
    public static MutationReceipt Resources(Seq<DocumentResourceChange> changes, Seq<BlockDiagnostic> diagnostics) =>
        Of(receipt: DocumentReceipt.Resources(changes: changes), diagnostics: diagnostics);
    public static MutationReceipt Objects(
        DocumentReceiptSlot slot,
        Seq<Guid> ids,
        DocumentResourceKind? kind = null,
        string? name = null) =>
        Of(receipt: kind is not null && name is not null
            ? DocumentReceipt.Objects(slot: slot, ids: ids, kind: kind, name: name)
            : DocumentReceipt.Objects(slot: slot, ids: ids));

    public static MutationReceipt Objects(
        DocumentReceiptSlot slot,
        Seq<Guid> ids,
        Seq<DocumentResourceChange> resources,
        Seq<BlockDiagnostic> diagnostics = default) =>
        new(Document: DocumentReceipt.Objects(slot: slot, ids: ids, resources: resources),
            Diagnostics: diagnostics);

    public static MutationReceipt Named(string name) =>
        Of(receipt: DocumentReceipt.Resource(kind: DocumentResourceKind.Block, name: name));

    public bool HasDiagnostics => !Diagnostics.IsEmpty;

    public MutationReceipt Combine(MutationReceipt rhs) => this + rhs;

    public static MutationReceipt operator +(MutationReceipt a, MutationReceipt b) {
        ArgumentNullException.ThrowIfNull(argument: a);
        ArgumentNullException.ThrowIfNull(argument: b);
        return new(Document: a.Document + b.Document, Diagnostics: a.Diagnostics + b.Diagnostics);
    }
}

public readonly record struct Placement(
    Transform Transform,
    Option<ObjectAttributes> Attrs,
    Option<HistoryRecord> History,
    bool Reference = false) {
    public static Placement Of(
        Transform xform,
        Option<ObjectAttributes> attrs = default,
        Option<HistoryRecord> history = default,
        bool reference = false) =>
        new(Transform: xform, Attrs: attrs, History: history, Reference: reference);

    public Fin<Placement> Admit(LiveStats live, Op key) =>
        Transform.IsValid && (!Reference || ReferenceAdmits(live: live))
            ? Fin.Succ(value: this)
            : Fin.Fail<Placement>(error: key.InvalidInput());

    private static bool ReferenceAdmits(LiveStats live) =>
        live.IsReference
        || (live.Update == UpdatePolicy.Linked && live.Layer == LayerStyle.Reference);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct PreviewFingerprint(ulong Value);

public sealed record PreviewSpec(
    int Width = 256,
    int Height = 256,
    DisplayModeRef? DisplayMode = null,
    DefinedViewportProjection Projection = DefinedViewportProjection.Perspective,
    IsometricCamera Camera = IsometricCamera.Northeast,
    bool DrawDecorations = false,
    bool ApplyDpiScaling = false,
    Option<Guid> HighlightMemberId = default) {
    public const int DefaultSize = 256;
    public const int MinSize = 16;
    public const int MaxSize = 4096;

    public static PreviewSpec Default { get; } = new(Width: DefaultSize, Height: DefaultSize);

    public static Fin<PreviewSpec> Of(
        int width, int height,
        DisplayModeRef? displayMode = null,
        DefinedViewportProjection projection = DefinedViewportProjection.Perspective,
        IsometricCamera camera = IsometricCamera.Northeast,
        bool drawDecorations = false, bool applyDpiScaling = false,
        Option<Guid> highlightMemberId = default,
        Op? key = null) =>
        (width, height) switch {
            ( >= MinSize and <= MaxSize, >= MinSize and <= MaxSize) => Fin.Succ(value: new PreviewSpec(
                Width: width, Height: height,
                DisplayMode: displayMode,
                Projection: projection, Camera: camera,
                DrawDecorations: drawDecorations, ApplyDpiScaling: applyDpiScaling,
                HighlightMemberId: highlightMemberId)),
            _ => Fin.Fail<PreviewSpec>(error: key.OrDefault().InvalidInput()),
        };

    public DisplayModeRef ResolvedMode => DisplayMode ?? DisplayModeRef.Wireframe;

    public PreviewFingerprint Fingerprint => new(Value: FingerprintParts().Fold(initialState: Fnv64.Offset, f: Fnv64.Mix));

    public Fin<PreviewSpec> Admit(Op key) =>
        Of(Width, Height, DisplayMode, Projection, Camera, DrawDecorations, ApplyDpiScaling, HighlightMemberId, key);

    private static ulong DisplayHash(DisplayModeRef mode) =>
        mode.Switch(
            wireframeCase: static _ => Fnv64.HashText(value: "wireframe"),
            byId: static r => Fnv64.HashGuid(value: r.Id, seed: 1UL),
            byName: static r => Fnv64.HashText(value: r.Name.ToUpperInvariant(), seed: 2UL));

    private Seq<ulong> FingerprintParts() {
        Seq<ulong> common = Seq((ulong)Width, (ulong)Height, (ulong)(int)Projection, ApplyDpiScaling ? 1UL : 0UL, DisplayHash(mode: ResolvedMode));
        return HighlightMemberId.Case switch {
            Guid id => common + Seq(Fnv64.HashGuid(value: id, seed: 3UL)),
            _ => common + Seq((ulong)(int)Camera, DrawDecorations ? 1UL : 0UL, 0UL),
        };
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct ReachInsert(
    Guid InstanceId,
    DefinitionId DefId,
    Transform WorldXform,
    int Depth,
    ImmutableArray<DefinitionId> Path);

public sealed record RefreshPlan(Seq<LinkedHealth> Items) {
    public Seq<LinkedHealth> Refreshable => Items.Filter(static item =>
        item.Update.IsLinked
        && item.Source.IsSome
        && item.Archive.RequiresRefresh
        && !item.Archive.IsBroken);
    public Seq<LinkedHealth> Blocked => Items.Filter(static item => !item.Diagnostics.IsEmpty);
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

public readonly record struct WatchPolicy(TimeSpan Debounce, TimeProvider? Clock = null) {
    public static readonly TimeSpan DefaultDebounce = TimeSpan.FromMilliseconds(value: 500);
    public static readonly TimeSpan MaxDebounce = TimeSpan.FromMinutes(value: 5);
    public static WatchPolicy Default { get; } = new(Debounce: DefaultDebounce);

    public static Fin<WatchPolicy> Of(TimeSpan debounce, Op? key = null) =>
        new WatchPolicy(Debounce: debounce).Admit(key: key.OrDefault());

    public TimeProvider EffectiveClock => Clock ?? TimeProvider.System;

    public Fin<WatchPolicy> Admit(Op key) =>
        Debounce > TimeSpan.Zero && Debounce <= MaxDebounce
            ? Fin.Succ(value: this)
            : Fin.Fail<WatchPolicy>(error: key.InvalidInput());
}

[StructLayout(LayoutKind.Auto)]
internal readonly record struct MemberVisit(
    Guid ObjectId,
    ObjectAttributes Attributes,
    Transform Composed,
    Option<InstanceObject> Instance,
    Option<InstanceDefinition> Definition,
    Option<InstanceReferenceGeometry> Reference,
    Option<RhinoObject> Leaf);

// --- [SERVICES] ---------------------------------------------------------------------------
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

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class BlockPaths {
    internal static Option<string> DocAnchor(RhinoDoc document) =>
        AnchorDir(Optional(document.Path)
            .Filter(static p => !string.IsNullOrWhiteSpace(value: p)));

    internal static Option<string> ArchiveAnchor(Option<string> archivePath) =>
        AnchorDir(archivePath);

    private static Option<string> AnchorDir(Option<string> path) =>
        path.Bind(static p =>
            Optional(IOPath.GetDirectoryName(path: p))
                .Filter(static dir => !string.IsNullOrWhiteSpace(value: dir)));
}

internal static class Fnv64 {
    internal const ulong Offset = 0xCBF29CE484222325UL;
    internal const ulong Prime = 0x100000001B3UL;
    internal static ulong Mix(ulong acc, ulong v) => unchecked((acc ^ v) * Prime);
    internal static ulong HashGuid(Guid value, ulong seed = Offset) {
        Span<byte> bytes = stackalloc byte[16];
        return value.TryWriteBytes(destination: bytes)
            ? HashBytes(values: bytes, seed: seed)
            : value.ToByteArray().Aggregate(seed: seed, func: static (acc, b) => Mix(acc: acc, v: b));
    }

    internal static ulong HashText(string value, ulong seed = Offset) =>
        value.Aggregate(seed: seed, func: static (acc, ch) => Mix(acc: acc, v: ch));

    private static ulong HashBytes(ReadOnlySpan<byte> values, ulong seed) {
        ulong acc = seed;
        foreach (byte b in values) acc = Mix(acc: acc, v: b); // BOUNDARY ADAPTER — span foreach is zero-alloc value-type enumeration; no LanguageExt combinator exists for ReadOnlySpan<byte>.
        return acc;
    }
}
