# [RASM_RHINO_ARCHIVE]

Standalone `File3dm` transaction rail (`Rasm.Rhino.Exchange`). One `ArchiveOp` union carries bounded reads, metadata inspection, embedded extraction, amendment, serialization, persistence, verification, diff, and shared-materialization programs through `Archives.Apply`; one owned archive lease bounds host lifetime, one `ArchiveReceipt` carries detached output plus the Exchange-wide `ExchangeEvidence`, and one ordered `ArchiveProgram` retains requested cardinality, every executed step, the stop index, and mutation-attempt truth. Exact returned or landed bytes feed the kernel content key, and every graph projection derives from one stored node/link shape.

## [01]-[INDEX]

- [02]-[SOURCE_AND_SLICE]: `ArchiveSource` the path/byte ingress, `ArchiveSlice` the partial-read rows, `ArchiveWritePolicy` the write-options projection.
- [03]-[GRAPH_AND_METADATA]: `ResourceRole`, `ResourceNode`, `ResourceLink`, `ExchangeEvidence`, `ArchiveGraph` with derived projections, `ArchiveMetadata`, `ArchiveDelta`, `ArchiveVerdict`.
- [04]-[PATCH_FAMILY]: `ArchivePatch` — the closed mutation vocabulary over settings, strings, named views, metadata, and preview.
- [05]-[TRANSACTION_RAIL]: `ArchiveOp`, `ArchiveYield`, `ArchiveReceipt`, `ArchiveStep`/`ArchiveProgram`, and `Archives.Apply` — one materialization, one dispatch, one release.

## [02]-[SOURCE_AND_SLICE]

- Owner: `ArchiveSource` `[Union]` — `PathCase(DocumentPath)` and `BytesCase(ReadOnlyMemory<byte>)`; every ingress admits once and owns byte memory before deferred execution. `ArchiveSlice` `[SmartEnum<int>]` owns the partial-read vocabulary. `ArchiveVersion` admits the host write interval; `MeshTarget` and `MeshPayload` carry the legal object-kind/payload matrix; `ArchiveWritePolicy` projects those values into one fresh `File3dmWriteOptions`.
- Law: slice filtering is a path privilege — the filtered `File3dm.Read` overloads take a file path, so a `BytesCase` materialization is always full and a filtered request over bytes records an `ExchangeEvidence.DegradedCase` instead of silently widening.
- Law: `ArchiveVersion` admits `0` or Rhino's writable `[2, RhinoApp.ExeVersion]` interval. `ArchiveWritePolicy.Of` admits one row per `MeshTarget` and rejects a render-bearing payload where the target row forbids render meshes. `Current` preserves host defaults, `Lean` suppresses every modeled mesh payload, and `Host` mints a fresh host option object per write.
- Growth: a new host table filter is one `ArchiveSlice` row; a new mesh kind or payload is one data row; a new write knob is one `ArchiveWritePolicy` field projected by `Host`.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.FileIO;

namespace Rasm.Rhino.Exchange;

// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ArchiveSource {
    private ArchiveSource() { }
    public sealed record PathCase(DocumentPath Path) : ArchiveSource;
    public sealed record BytesCase(ReadOnlyMemory<byte> Bytes) : ArchiveSource;

    public static Fin<ArchiveSource> Of(string path, Op? key = null) {
        Op op = key.OrDefault();
        return op.Catch(() => Fin.Succ(value: (ArchiveSource)new PathCase(Path: DocumentPath.Create(value: path))));
    }

    public static Fin<ArchiveSource> Of(ReadOnlyMemory<byte> bytes, Op? key = null) =>
        guard(!bytes.IsEmpty, key.OrDefault().InvalidInput()).ToFin()
            .Map(_ => (ArchiveSource)new BytesCase(Bytes: bytes.ToArray()));
}

[SmartEnum<int>]
public sealed partial class ArchiveSlice {
    public static readonly ArchiveSlice Full = new(key: 0,
        tables: File3dm.TableTypeFilter.None, objects: File3dm.ObjectTypeFilter.None, filtered: false);
    public static readonly ArchiveSlice Header = new(key: 1,
        tables: File3dm.TableTypeFilter.Properties | File3dm.TableTypeFilter.Settings,
        objects: File3dm.ObjectTypeFilter.None, filtered: true);
    public static readonly ArchiveSlice Objects = new(key: 2,
        tables: File3dm.TableTypeFilter.ObjectTable | File3dm.TableTypeFilter.Layer,
        objects: File3dm.ObjectTypeFilter.Any, filtered: true);
    public static readonly ArchiveSlice Resources = new(key: 3,
        tables: File3dm.TableTypeFilter.Layer | File3dm.TableTypeFilter.Material
              | File3dm.TableTypeFilter.Linetype | File3dm.TableTypeFilter.Group
              | File3dm.TableTypeFilter.InstanceDefinition | File3dm.TableTypeFilter.Bitmap
              | File3dm.TableTypeFilter.TextureMapping,
        objects: File3dm.ObjectTypeFilter.None, filtered: true);
    public static readonly ArchiveSlice Layers = new(key: 4,
        tables: File3dm.TableTypeFilter.Layer, objects: File3dm.ObjectTypeFilter.None, filtered: true);
    public static readonly ArchiveSlice Strings = new(key: 5,
        tables: File3dm.TableTypeFilter.UserTable, objects: File3dm.ObjectTypeFilter.None, filtered: true);

    public File3dm.TableTypeFilter Tables { get; }
    public File3dm.ObjectTypeFilter Objects { get; }
    public bool Filtered { get; }
}

// --- [MODELS] -------------------------------------------------------------------------------
[ValueObject<int>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct ArchiveVersion {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
        validationError = value == 0 || value is >= 2 && value <= RhinoApp.ExeVersion
            ? null
            : new ValidationError(message: $"Archive version must be 0 or within [2,{RhinoApp.ExeVersion}].");
}

[SmartEnum<int>]
public sealed partial class MeshTarget {
    public static readonly MeshTarget Brep = new(key: 0, kind: ObjectType.Brep, supportsRender: true);
    public static readonly MeshTarget Extrusion = new(key: 1, kind: ObjectType.Extrusion, supportsRender: true);
    public static readonly MeshTarget SubD = new(key: 2, kind: ObjectType.SubD, supportsRender: true);
    public static readonly MeshTarget Mesh = new(key: 3, kind: ObjectType.Mesh, supportsRender: false);

    public ObjectType Kind { get; }
    public bool SupportsRender { get; }
}

[SmartEnum<int>]
public sealed partial class MeshPayload {
    public static readonly MeshPayload None = new(key: 0, render: false, analysis: false);
    public static readonly MeshPayload RenderOnly = new(key: 1, render: true, analysis: false);
    public static readonly MeshPayload AnalysisOnly = new(key: 2, render: false, analysis: true);
    public static readonly MeshPayload RenderAndAnalysis = new(key: 3, render: true, analysis: true);

    public bool Render { get; }
    public bool Analysis { get; }
}

public readonly record struct MeshWrite(MeshTarget Target, MeshPayload Payload);

public sealed record ArchiveWritePolicy {
    private ArchiveWritePolicy(ArchiveVersion version, bool saveUserData, Seq<MeshWrite> meshes) =>
        (Version, SaveUserData, Meshes) = (version, saveUserData, meshes);

    public static ArchiveWritePolicy Current { get; } = new(
        version: ArchiveVersion.Create(value: 0),
        saveUserData: true,
        meshes: Seq<MeshWrite>());

    public static ArchiveWritePolicy Lean { get; } = new(
        version: ArchiveVersion.Create(value: 0),
        saveUserData: false,
        meshes: Seq(
            new MeshWrite(Target: MeshTarget.Brep, Payload: MeshPayload.None),
            new MeshWrite(Target: MeshTarget.Extrusion, Payload: MeshPayload.None),
            new MeshWrite(Target: MeshTarget.SubD, Payload: MeshPayload.None),
            new MeshWrite(Target: MeshTarget.Mesh, Payload: MeshPayload.None)));

    public ArchiveVersion Version { get; }
    public bool SaveUserData { get; }
    public Seq<MeshWrite> Meshes { get; }

    public static Fin<ArchiveWritePolicy> Of(
        ArchiveVersion version,
        bool saveUserData,
        Seq<MeshWrite> meshes = default,
        Op? key = null) {
        Op op = key.OrDefault();
        return from _payloads in guard(meshes.ForAll(static row =>
                   row.Target is not null && row.Payload is not null
                   && (!row.Payload.Render || row.Target.SupportsRender)), op.InvalidInput()).ToFin()
               from _unique in guard(meshes.Map(static row => row.Target).Distinct().Count == meshes.Count, op.InvalidInput()).ToFin()
               select new ArchiveWritePolicy(version: version, saveUserData: saveUserData, meshes: meshes);
    }

    internal File3dmWriteOptions Host() {
        File3dmWriteOptions options = new() { Version = Version.Value, SaveUserData = SaveUserData };
        _ = Meshes.Iter(row => {
            options.EnableRenderMeshes(objectType: row.Target.Kind, enable: row.Payload.Render);
            options.EnableAnalysisMeshes(objectType: row.Target.Kind, enable: row.Payload.Analysis);
        });
        return options;
    }
}
```

## [03]-[GRAPH_AND_METADATA]

- Owner: this page owns `ExchangeEvidence` `[Union]` for the whole Exchange folder; native diagnostics, broken resource links, degraded capability, empty results, host-default behavior, and mutation/undo state carry typed payloads that archive, live-document, and publish receipts share. `ResourceRole` `[SmartEnum<int>]` owns the archive resource vocabulary; `ResourceNode`/`ResourceLink` own the stored graph facts; `ArchiveGraph` derives role, name, integrity, and summary projections; `ArchiveMetadata`, `ArchiveDelta`, and `ArchiveVerdict` are detached terminal values.
- Law: node identity inside one graph is `(Role, Name, Id)`; archive identity across graphs is the kernel content key — `ArchiveDelta.Of` compares node sets by role and name and carries both archives' `UInt128` content keys, so "same archive" is answered by `ContentHash.Of` over each exact serialized payload and never by a local delta rule.
- Law: the graph is stored once and projected many times — role, name, link-integrity, and summary reads fold the same node/link sequences.
- Law: native diagnostics survive as `ExchangeEvidence.NativeCase` rows whose `Succeeded` field disambiguates warnings from invalidity — `ReadWithLog`/`WriteWithLog`/`IsValidWithLog` out-strings remain typed evidence after a returned result and remain fault detail when no result exists.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class ResourceRole {
    public static readonly ResourceRole Layer = new(key: 0);
    public static readonly ResourceRole Material = new(key: 1);
    public static readonly ResourceRole Group = new(key: 2);
    public static readonly ResourceRole Block = new(key: 3);
    public static readonly ResourceRole Instance = new(key: 4);
    public static readonly ResourceRole ModelView = new(key: 5);
    public static readonly ResourceRole NamedView = new(key: 6);
    public static readonly ResourceRole Layout = new(key: 7);
    public static readonly ResourceRole Embedded = new(key: 8);
    public static readonly ResourceRole RenderMaterial = new(key: 9);
    public static readonly ResourceRole RenderEnvironment = new(key: 10);
    public static readonly ResourceRole RenderTexture = new(key: 11);
    public static readonly ResourceRole StringEntry = new(key: 12);
    public static readonly ResourceRole DimensionStyle = new(key: 13);
    public static readonly ResourceRole LinkedArchive = new(key: 14);
    public static readonly ResourceRole Settings = new(key: 15);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ExchangeEvidence {
    private ExchangeEvidence() { }
    public sealed record NativeCase(string Surface, bool Succeeded, string Detail, Option<DocumentPath> Target = default) : ExchangeEvidence;
    public sealed record BrokenLinkCase(ResourceLink Link) : ExchangeEvidence;
    public sealed record DegradedCase(string Surface, string Detail) : ExchangeEvidence;
    public sealed record EmptyCase(string Surface) : ExchangeEvidence;
    public sealed record HostDefaultsCase(string Surface, string Detail) : ExchangeEvidence;
    public sealed record MutationCase(
        string Surface,
        bool Attempted,
        bool Committed,
        bool MayRemain,
        Option<uint> UndoRecord) : ExchangeEvidence;
    public sealed record UnitCase(string Surface, LengthUnit Before, LengthUnit After, bool GeometryScaled) : ExchangeEvidence;
}

// --- [MODELS] -------------------------------------------------------------------------------
public readonly record struct ResourceNode(ResourceRole Role, string Name, Option<Guid> Id);

public readonly record struct ResourceLink(ResourceNode From, ResourceNode To, ResourceRole Relation);

public sealed record ArchiveGraph(Seq<ResourceNode> Nodes, Seq<ResourceLink> Links) {
    public HashMap<ResourceRole, Seq<ResourceNode>> ByRole() =>
        Nodes.Fold(HashMap<ResourceRole, Seq<ResourceNode>>(), static (map, node) =>
            map.AddOrUpdate(node.Role, existing => existing.Add(node), () => Seq(node)));

    public Seq<string> Names(ResourceRole role) =>
        Nodes.Filter(node => node.Role == role).Map(static node => node.Name);

    public Seq<ResourceLink> Broken() =>
        Links.Filter(link => !Nodes.Exists(node => node == link.To));

    public Seq<(ResourceRole Role, int Count)> Summary() =>
        ByRole().AsIterable().Map(static pair => (pair.Key, pair.Value.Count)).ToSeq();
}

public sealed record ArchiveMetadata(
    Option<string> Notes,
    int ArchiveVersion,
    Option<(string CreatedBy, string LastEditedBy, int Revision, DateTime CreatedOn, DateTime LastEditedOn)> Revision,
    Option<(string Name, string Url, string Details)> Application,
    bool EarthAnchored,
    Seq<(string Name, Guid Id)> Layouts,
    int DimensionStyles,
    bool HasPreview);

public sealed record ArchiveDelta(
    UInt128 SourceKey,
    UInt128 OtherKey,
    Seq<ResourceNode> Added,
    Seq<ResourceNode> Removed,
    Seq<ResourceNode> Retained) {
    public bool Identical => SourceKey == OtherKey;

    internal static ArchiveDelta Of(UInt128 sourceKey, UInt128 otherKey, ArchiveGraph source, ArchiveGraph other) {
        LanguageExt.HashSet<(ResourceRole, string)> before = toHashSet(source.Nodes.Map(static n => (n.Role, n.Name)));
        LanguageExt.HashSet<(ResourceRole, string)> after = toHashSet(other.Nodes.Map(static n => (n.Role, n.Name)));
        return new(
            SourceKey: sourceKey,
            OtherKey: otherKey,
            Added: other.Nodes.Filter(n => !before.Contains((n.Role, n.Name))),
            Removed: source.Nodes.Filter(n => !after.Contains((n.Role, n.Name))),
            Retained: source.Nodes.Filter(n => after.Contains((n.Role, n.Name))));
    }
}

public sealed record ArchiveVerdict(bool Valid, int InvalidObjects, int BrokenLinks);
```

## [04]-[PATCH_FAMILY]

- Owner: `ArchivePatch` `[Union]` is the closed mutation vocabulary. `PreviewPatch` owns copied encoded bytes or explicit removal. `ArchiveChange` carries the changed resource plus detached evidence, so custom model/page unit values and the model-scaling decision survive the mutation boundary without retaining host settings.
- Law: a patch mutates the leased in-memory archive only; `AmendCase` writes a same-directory temporary archive after every patch lands and atomically replaces the target after nonempty-byte verification, so neither patch failure nor write failure exposes a half-applied target.
- Law: model-unit conversion admits source and destination `LengthUnit` values through the kernel `Context` owner and consumes `Context.ScaleTo`; the meters-per-unit ratio scales geometry before `File3dmSettings.ModelUnits` receives the destination, so custom unit name and scale survive. `PageUnits` relabeling remains independent.
- Law: string deletion is absence — `StringCase` with `None` value deletes through `File3dmStringTable.Delete`, so the value option carries the full write/delete decision. `NotesCase` carries the host's full notes surface — text plus the `IsVisible`/`IsHtml` columns as optional overrides — and commits the whole carrier back through the `Notes` setter so every axis writes through.
- Boundary: `PreviewPatch.Set` copies encoded bytes at admission, decodes and clones the bitmap while the stream remains live, and disposes both bitmaps after `SetPreviewImage` copies the pixels; `Clear` passes the host's null sentinel. Decode failure rails before archive mutation.
- Growth: a new mutable archive surface is one case with its application arm; the amended yield and the total dispatch break loudly until the case is handled.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ArchivePatch {
    private ArchivePatch() { }
    public sealed record NotesCase(string Notes, Option<bool> Visible = default, Option<bool> Html = default) : ArchivePatch;
    public sealed record SettingsCase(Option<LengthUnit> ModelUnits, Option<LengthUnit> PageUnits, bool ScaleModelGeometry) : ArchivePatch;
    public sealed record StringCase(string Key, Option<string> Value) : ArchivePatch;
    public sealed record NamedViewCase(string Name, Option<string> Rename, bool Delete) : ArchivePatch;
    public sealed record PreviewCase(PreviewPatch Preview) : ArchivePatch;

    internal Fin<ArchiveChange> Apply(File3dm archive, Op op) => Switch(
        state: (Archive: archive, Op: op),
        notesCase: static (ctx, patch) => ctx.Op.Catch(() => {
            File3dmNotes notes = ctx.Archive.Notes;
            notes.Notes = patch.Notes;
            _ = patch.Visible.Iter(value => notes.IsVisible = value);
            _ = patch.Html.Iter(value => notes.IsHtml = value);
            ctx.Archive.Notes = notes;
            return Fin.Succ(value: ArchiveChange.Of(new ResourceNode(Role: ResourceRole.StringEntry, Name: nameof(NotesCase), Id: None)));
        }),
        settingsCase: static (ctx, patch) =>
            from modelEvidence in patch.ModelUnits
                .Map(units => ModelUnits(
                    archive: ctx.Archive,
                    target: units,
                    scaleGeometry: patch.ScaleModelGeometry,
                    op: ctx.Op))
                .IfNone(Fin.Succ(value: Option<ExchangeEvidence>.None))
            from pageEvidence in patch.PageUnits
                .Map(units => PageUnits(archive: ctx.Archive, target: units, op: ctx.Op))
                .IfNone(Fin.Succ(value: Option<ExchangeEvidence>.None))
            select new ArchiveChange(
                Resource: new ResourceNode(Role: ResourceRole.Settings, Name: nameof(SettingsCase), Id: None),
                Evidence: modelEvidence.ToSeq() + pageEvidence.ToSeq()),
        stringCase: static (ctx, patch) => ctx.Op.Catch(() => {
            _ = patch.Value.Case switch {
                string value => Op.Side(() => ctx.Archive.Strings.SetString(section: null, entry: patch.Key, value: value)),
                _ => Op.Side(() => ctx.Archive.Strings.Delete(section: null, entry: patch.Key)),
            };
            return Fin.Succ(value: ArchiveChange.Of(new ResourceNode(Role: ResourceRole.StringEntry, Name: patch.Key, Id: None)));
        }),
        namedViewCase: static (ctx, patch) => ctx.Op.Catch(() =>
            Optional(ctx.Archive.AllNamedViews.FindName(name: patch.Name)).ToFin(Fail: ctx.Op.InvalidInput()).Bind(found =>
                (patch.Delete, patch.Rename.Case) switch {
                    (true, _) => ctx.Op.Confirm(success: ctx.Archive.AllNamedViews.Delete(item: found))
                        .Map(_ => ArchiveChange.Of(new ResourceNode(Role: ResourceRole.NamedView, Name: patch.Name, Id: None))),
                    (false, string next) => ctx.Op.Catch(() => {
                        found.Name = next;
                        return Fin.Succ(value: ArchiveChange.Of(new ResourceNode(Role: ResourceRole.NamedView, Name: next, Id: None)));
                    }),
                    _ => Fin.Succ(value: ArchiveChange.Of(new ResourceNode(Role: ResourceRole.NamedView, Name: patch.Name, Id: None))),
                })),
        previewCase: static (ctx, patch) => Optional(patch.Preview).ToFin(Fail: ctx.Op.InvalidInput())
            .Bind(preview => preview.Apply(archive: ctx.Archive, op: ctx.Op))
            .Map(_ => ArchiveChange.Of(new ResourceNode(Role: ResourceRole.Embedded, Name: nameof(PreviewCase), Id: None))));

    private static Fin<Option<ExchangeEvidence>> ModelUnits(File3dm archive, LengthUnit target, bool scaleGeometry, Op op) {
        LengthUnit before = archive.Settings.ModelUnits;
        return from current in Context.Of(units: before).ToFin()
               from destination in Context.Of(units: target).ToFin()
               from factor in current.ScaleTo(target: destination)
               from _scaled in scaleGeometry
                   ? toSeq(archive.Objects)
                       .TraverseM(entry => Optional(entry.Geometry)
                           .ToFin(Fail: op.InvalidResult(detail: $"{entry.Id}: geometry unrealized (null native pointer)."))
                           .Bind(geometry => op.Confirm(success: geometry.Scale(scaleFactor: factor))))
                       .As()
                       .Map(static _ => unit)
                   : Fin.Succ(value: unit)
               from _written in op.Catch(() => {
                   archive.Settings.ModelUnits = target;
                   return Fin.Succ(value: unit);
               })
               select Some<ExchangeEvidence>(new ExchangeEvidence.UnitCase(
                   Surface: nameof(File3dmSettings.ModelUnits),
                   Before: before,
                   After: target,
                   GeometryScaled: scaleGeometry));
    }

    private static Fin<Option<ExchangeEvidence>> PageUnits(File3dm archive, LengthUnit target, Op op) =>
        from _target in Context.Of(units: target).ToFin()
        from evidence in op.Catch(() => {
            LengthUnit before = archive.Settings.PageUnits;
            archive.Settings.PageUnits = target;
            return Fin.Succ(value: Some<ExchangeEvidence>(new ExchangeEvidence.UnitCase(
                Surface: nameof(File3dmSettings.PageUnits),
                Before: before,
                After: target,
                GeometryScaled: false)));
        })
        select evidence;
}

public sealed record PreviewPatch {
    private PreviewPatch(Option<ReadOnlyMemory<byte>> image) => Image = image;

    public static PreviewPatch Clear { get; } = new(image: None);
    internal Option<ReadOnlyMemory<byte>> Image { get; }

    public static Fin<PreviewPatch> Set(ReadOnlyMemory<byte> image, Op? key = null) =>
        guard(!image.IsEmpty, key.OrDefault().InvalidInput()).ToFin()
            .Map(_ => new PreviewPatch(image: Some<ReadOnlyMemory<byte>>(image.ToArray())));

    internal Fin<Unit> Apply(File3dm archive, Op op) => Image.Match(
        Some: bytes => op.Catch(() => {
            using System.IO.MemoryStream stream = new(buffer: bytes.ToArray(), writable: false);
            using System.Drawing.Bitmap decoded = new(stream: stream);
            using System.Drawing.Bitmap detached = new(image: decoded);
            archive.SetPreviewImage(image: detached);
            return Fin.Succ(value: unit);
        }),
        None: () => op.Catch(() => archive.SetPreviewImage(image: null!)));
}

public sealed record ArchiveChange(ResourceNode Resource, Seq<ExchangeEvidence> Evidence) {
    internal static ArchiveChange Of(ResourceNode resource) => new(Resource: resource, Evidence: Seq<ExchangeEvidence>());
}
```

## [05]-[TRANSACTION_RAIL]

- Owner: `ArchiveOp` `[Union]` is the standalone request family. Extraction, amendment, and persistence each carry an `OutputPolicy` — the operations rail's one collision/directory/landing owner — so replace-versus-refuse, parent-directory minting, and bounded ordinal renaming are the same rows every Exchange egress obeys, never a second archive-local collision vocabulary. `ArchiveYield` carries detached result data; `ArchiveReceipt` carries the yield plus evidence; `ArchiveStep` retains source ordinal, the failed step's evidence, and mutation residue truth; `ArchiveProgram` retains requested cardinality and the ordered executed prefix.
- Entry: `Archives.Apply(ArchiveSource, ArchiveOp, Op?) : Fin<ArchiveReceipt>` — no live document or session enters the archive scope.
- Law: `InspectCase` over a `PathCase` never constructs a `File3dm` — the static header reads (`ReadNotes`, `ReadArchiveVersion`, `ReadRevisionHistory`, `ReadApplicationData`, `ReadEarthAnchorPoint`, `ReadPageViews`, `ReadDimensionStyles`, `ReadPreviewImage`) answer from the file, and the batch dispatcher routes an inner inspect over a path source to the same static reads; only a `BytesCase` inspect projects the in-memory header with `ExchangeEvidence.DegradedCase`, so the yield shape never forks on ingress and the degraded row is emitted only where the layout roster is genuinely unreachable.
- Law: `SerializeCase` keys the exact `ToByteArray(policy.Host())` payload it returns; `PersistCase` and `AmendCase` write and verify a same-directory temporary file, move it over the target, and key the bytes that were committed, so content identity names the landed artifact.
- Law: every nonempty `ReadWithLog`, `WriteWithLog`, and `IsValidWithLog` diagnostic becomes `ExchangeEvidence.NativeCase` with the native call's outcome; a native call without a result carries the same diagnostic in its fault, and an invalid object without native text receives an explicit failed fallback row.
- Law: `VerifyCase` folds every object's validity fact plus every native log and broken graph link into one verdict/evidence pair; archive-wide validity never substitutes for these object and relationship witnesses. `File3dmObject.Geometry` is runtime-null for an unrealized native pointer, so every geometry read — validity, unit scaling — guards through `Optional` before dereference and reports the null as an explicit failed witness, never an escape.
- Law: extraction admits a case-insensitively unique basename set before the first save; folder existence and per-file collision ride each landing's `OutputPolicy` rows. Amendment rejects an empty patch sequence because unchanged persistence already belongs to `PersistCase`.
- Law: `BatchCase` shares one materialization and dispatches the request sequence in source order; nesting is refused at admission, the first failure seals the executed prefix, and `ArchiveProgram.Requested`, `Steps`, `StoppedAt`, `MutationAttempted`, and `MutationMayRemain` distinguish completion, skipped suffix, external mutation, and possible residue.
- Law: standalone archive mutation has no undo facility. Every landed artifact stages through `OutputPolicy.Land` — the operations rail's one staging kernel — with the archive's own hooks bound once in `Archives.Land`: `WriteWithLog` into the temporary as the stage payload carrying the native log, and byte re-materialization (`ValidateArchiveBytes`) as the validation, so a landed 3dm is proven parseable both before and after the move; `Land` is internal because the operations rail's fresh-archive geometry emission lands through the same hook, never a second `WriteWithLog` staging spelling. Successful extraction, persistence, and amendment emit `MutationCase(Committed: true, MayRemain: false, UndoRecord: None)`. A failed mutating step conservatively emits `MutationCase(MayRemain: true)` because interruption or post-move verification can leave a committed target, and multi-file extraction can retain an earlier committed prefix.
- Boundary: `File3dm`, static-read `ViewInfo`/`DimensionStyle`, `EarthAnchorPoint`, and preview `Bitmap` values live only inside owned lease windows; every yield contains local value shapes, copied byte memory, paths, hashes, or typed faults before release.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ArchiveOp {
    private ArchiveOp() { }
    public sealed record SnapshotCase(ArchiveSlice Slice) : ArchiveOp;
    public sealed record InspectCase : ArchiveOp;
    public sealed record ExtractCase(DocumentPath Folder, OutputPolicy Output) : ArchiveOp;
    public sealed record AmendCase(Seq<ArchivePatch> Patches, DocumentPath Target, ArchiveWritePolicy Policy, OutputPolicy Output) : ArchiveOp;
    public sealed record SerializeCase(ArchiveWritePolicy Policy) : ArchiveOp;
    public sealed record PersistCase(DocumentPath Target, ArchiveWritePolicy Policy, OutputPolicy Output) : ArchiveOp;
    public sealed record VerifyCase : ArchiveOp;
    public sealed record DiffCase(ArchiveSource Other) : ArchiveOp;
    public sealed record BatchCase(Seq<ArchiveOp> Program) : ArchiveOp;

    public static Fin<ArchiveOp> Batch(Op? key = null, params ReadOnlySpan<ArchiveOp> program) {
        Op op = key.OrDefault();
        return ((ArchiveOp)new BatchCase(Program: toSeq(program.ToArray()))).Admit(op: op);
    }

    internal Fin<ArchiveOp> Admit(Op op) => this is BatchCase batch
        ? from _count in guard(!batch.Program.IsEmpty, op.InvalidInput()).ToFin()
          from _present in guard(batch.Program.ForAll(static item => item is not null), op.InvalidInput()).ToFin()
          from _flat in guard(!batch.Program.Exists(static item => item is BatchCase), op.InvalidInput()).ToFin()
          select this
        : Fin.Succ(value: this);

    internal bool Mutates() => this is ExtractCase or AmendCase or PersistCase;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ArchiveYield {
    private ArchiveYield() { }
    public sealed record GraphCase(ArchiveGraph Graph) : ArchiveYield;
    public sealed record MetadataCase(ArchiveMetadata Metadata) : ArchiveYield;
    public sealed record ExtractedCase(Seq<(string Name, DocumentPath Target, UInt128 ContentKey)> Files) : ArchiveYield;
    public sealed record AmendedCase(Seq<ResourceNode> Changed, DocumentPath Target, UInt128 ContentKey) : ArchiveYield;
    public sealed record BytesCase(ReadOnlyMemory<byte> Bytes, UInt128 ContentKey) : ArchiveYield;
    public sealed record PersistedCase(DocumentPath Target, UInt128 ContentKey) : ArchiveYield;
    public sealed record VerdictCase(ArchiveVerdict Verdict) : ArchiveYield;
    public sealed record DeltaCase(ArchiveDelta Delta) : ArchiveYield;
    public sealed record ProgramCase(ArchiveProgram Program) : ArchiveYield;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ArchiveStep {
    private ArchiveStep() { }
    public sealed record SucceededCase(int Index, bool MutationAttempted, ArchiveReceipt Receipt) : ArchiveStep;
    public sealed record FailedCase(int Index, bool MutationAttempted, bool MutationMayRemain, Error Failure, Seq<ExchangeEvidence> Evidence) : ArchiveStep;

    internal bool AttemptedMutation => Switch(
        succeededCase: static step => step.MutationAttempted,
        failedCase: static step => step.MutationAttempted);

    internal bool MayRetainMutation => Switch(
        succeededCase: static _ => false,
        failedCase: static step => step.MutationMayRemain);

    internal Seq<ExchangeEvidence> Evidence() => Switch(
        succeededCase: static step => step.Receipt.Evidence,
        failedCase: static step => step.Evidence);
}

public sealed record ArchiveProgram {
    private ArchiveProgram(int requested, Seq<ArchiveStep> steps) =>
        (Requested, Steps) = (requested, steps);

    public int Requested { get; }
    public Seq<ArchiveStep> Steps { get; }
    public Option<int> StoppedAt => Steps.Fold(
        Option<int>.None,
        static (found, step) => found.IsSome
            ? found
            : step is ArchiveStep.FailedCase failed ? Some(failed.Index) : Option<int>.None);
    public bool Completed => Steps.Count == Requested && StoppedAt.IsNone;
    public bool Failed => StoppedAt.IsSome;
    public bool MutationAttempted => Steps.Exists(static step => step.AttemptedMutation);
    public bool MutationMayRemain => Steps.Exists(static step => step.MayRetainMutation);
    public Seq<ExchangeEvidence> Evidence => Steps.Bind(static step => step.Evidence());

    internal static ArchiveProgram Of(int requested, Seq<ArchiveStep> steps) =>
        new(requested: requested, steps: steps);
}

public sealed record ArchiveReceipt(ArchiveYield Yield, Seq<ExchangeEvidence> Evidence) : IDetachedDocumentResult {
    internal static ArchiveReceipt Of(ArchiveYield yield, Seq<ExchangeEvidence> evidence = default) =>
        new(Yield: yield, Evidence: evidence);

    internal static ArchiveReceipt Program(int requested, Seq<ArchiveStep> steps) {
        ArchiveProgram program = ArchiveProgram.Of(requested: requested, steps: steps);
        return new(Yield: new ArchiveYield.ProgramCase(Program: program), Evidence: program.Evidence);
    }
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Archives {
    public static Fin<ArchiveReceipt> Apply(ArchiveSource source, ArchiveOp request, Op? key = null) {
        Op op = key.OrDefault();
        return from ingress in Optional(source).ToFin(Fail: op.InvalidInput())
               from requested in Optional(request).ToFin(Fail: op.InvalidInput())
               from admitted in requested.Admit(op: op)
               from yield in admitted switch {
                   ArchiveOp.InspectCase when ingress is ArchiveSource.PathCase path => InspectPath(path: path.Path, op: op),
                   _ => Materialized(source: ingress, request: admitted, op: op),
               }
               select yield;
    }

    private static Fin<ArchiveReceipt> Materialized(ArchiveSource source, ArchiveOp request, Op op) {
        ArchiveSlice slice = request is ArchiveOp.SnapshotCase snapshot ? snapshot.Slice : ArchiveSlice.Full;
        return Open(source: source, slice: slice, op: op).Bind(opened =>
            opened.Lease.Use(archive =>
                Dispatch(source: source, archive: archive, request: request, evidence: opened.Evidence, op: op)));
    }

    private static Fin<(Lease<File3dm> Lease, Seq<ExchangeEvidence> Evidence)> Open(ArchiveSource source, ArchiveSlice slice, Op op) =>
        source.Switch(
            state: (Slice: slice, Op: op),
            pathCase: static (ctx, ingress) => ctx.Op.Catch(() => {
                string log = string.Empty;
                File3dm? archive = ctx.Slice.Filtered
                    ? File3dm.ReadWithLog(path: ingress.Path.Value, tableTypeFilterFilter: ctx.Slice.Tables, objectTypeFilter: ctx.Slice.Objects, errorLog: out log)
                    : File3dm.ReadWithLog(path: ingress.Path.Value, errorLog: out log);
                Option<string> native = Optional(log).Filter(static text => !string.IsNullOrWhiteSpace(value: text));
                return Optional(archive).ToFin(Fail: ctx.Op.InvalidResult(
                    detail: $"{nameof(File3dm.ReadWithLog)}: {native.IfNone("returned no archive without native detail.")}")).Map(model =>
                    ((Lease<File3dm>)new Lease<File3dm>.Owned(Value: model),
                     native.Map(text => (ExchangeEvidence)new ExchangeEvidence.NativeCase(
                             Surface: nameof(File3dm.ReadWithLog),
                             Succeeded: true,
                             Detail: text,
                             Target: Some(ingress.Path))).ToSeq()));
            }),
            bytesCase: static (ctx, ingress) => ctx.Op.Catch(() =>
                Optional(File3dm.FromByteArray(bytes: ingress.Bytes.ToArray())).ToFin(Fail: ctx.Op.InvalidResult()).Map(model =>
                    ((Lease<File3dm>)new Lease<File3dm>.Owned(Value: model),
                     ctx.Slice.Filtered
                         ? Seq<ExchangeEvidence>(new ExchangeEvidence.DegradedCase(
                             Surface: nameof(ArchiveSlice),
                             Detail: "Byte ingress admits only full reads; the slice filter is path-only."))
                         : Seq<ExchangeEvidence>()))));

    private static Fin<ArchiveReceipt> Dispatch(
        ArchiveSource source,
        File3dm archive,
        ArchiveOp request,
        Seq<ExchangeEvidence> evidence,
        Op op) =>
        request.Switch(
            state: (Source: source, Archive: archive, Evidence: evidence, Op: op),
            snapshotCase: static (ctx, _) =>
                Graph(archive: ctx.Archive, op: ctx.Op).Map(graph => ArchiveReceipt.Of(
                    yield: new ArchiveYield.GraphCase(Graph: graph),
                    evidence: ctx.Evidence + (graph.Nodes.IsEmpty
                        ? Seq<ExchangeEvidence>(new ExchangeEvidence.EmptyCase(Surface: nameof(ArchiveGraph)))
                        : Seq<ExchangeEvidence>()))),
            inspectCase: static (ctx, _) => ctx.Source.Switch(
                state: ctx,
                pathCase: static (inner, source) => InspectPath(path: source.Path, op: inner.Op)
                    .Map(receipt => receipt with { Evidence = inner.Evidence + receipt.Evidence }),
                bytesCase: static (inner, _) => MetadataOf(archive: inner.Archive, op: inner.Op)
                    .Map(receipt => receipt with { Evidence = inner.Evidence + receipt.Evidence })),
            extractCase: static (ctx, request) => Extract(archive: ctx.Archive, folder: request.Folder, output: request.Output, op: ctx.Op)
                .Map(receipt => receipt with {
                    Evidence = ctx.Evidence
                        + receipt.Evidence
                        + (receipt.Yield is ArchiveYield.ExtractedCase { Files.IsEmpty: false }
                            ? Seq<ExchangeEvidence>(new ExchangeEvidence.MutationCase(
                                Surface: nameof(ArchiveOp.ExtractCase),
                                Attempted: true,
                                Committed: true,
                                MayRemain: false,
                                UndoRecord: None))
                            : Seq<ExchangeEvidence>(new ExchangeEvidence.EmptyCase(Surface: nameof(ArchiveOp.ExtractCase)))),
                }),
            amendCase: static (ctx, request) =>
                from _patches in guard(!request.Patches.IsEmpty, ctx.Op.InvalidInput()).ToFin()
                from changes in request.Patches
                    .TraverseM(patch => patch.Apply(archive: ctx.Archive, op: ctx.Op))
                    .As()
                from written in Land(
                    archive: ctx.Archive,
                    target: request.Target,
                    policy: request.Policy,
                    output: request.Output,
                    op: ctx.Op)
                select ArchiveReceipt.Of(
                    yield: new ArchiveYield.AmendedCase(
                        Changed: changes.Map(static change => change.Resource),
                        Target: written.Target,
                        ContentKey: written.ContentKey),
                    evidence: ctx.Evidence + changes.Bind(static change => change.Evidence)
                        + Committed(surface: nameof(ArchiveOp.AmendCase), written: written)),
            serializeCase: static (ctx, request) =>
                ArchiveBytes(archive: ctx.Archive, policy: request.Policy, op: ctx.Op).Map(bytes =>
                    ArchiveReceipt.Of(
                        yield: new ArchiveYield.BytesCase(Bytes: bytes, ContentKey: ContentHash.Of(canonicalBytes: bytes)),
                        evidence: ctx.Evidence)),
            persistCase: static (ctx, request) =>
                Land(archive: ctx.Archive, target: request.Target, policy: request.Policy, output: request.Output, op: ctx.Op)
                    .Map(written => ArchiveReceipt.Of(
                        yield: new ArchiveYield.PersistedCase(
                            Target: written.Target,
                            ContentKey: written.ContentKey),
                        evidence: ctx.Evidence + Committed(surface: nameof(ArchiveOp.PersistCase), written: written))),
            verifyCase: static (ctx, _) =>
                Graph(archive: ctx.Archive, op: ctx.Op).Bind(graph => ctx.Op.Catch(() => {
                    Seq<(bool Valid, Option<ExchangeEvidence> Evidence)> checks = toSeq(ctx.Archive.Objects).Map(entry => {
                        string subject = entry.Name ?? entry.Id.ToString();
                        return Optional(entry.Geometry).Match(
                            Some: geometry => {
                                bool valid = geometry.IsValidWithLog(log: out string log);
                                Option<ExchangeEvidence> native = string.IsNullOrWhiteSpace(value: log)
                                    ? valid
                                        ? Option<ExchangeEvidence>.None
                                        : Some<ExchangeEvidence>(new ExchangeEvidence.NativeCase(
                                            Surface: nameof(Rhino.Runtime.CommonObject.IsValidWithLog),
                                            Succeeded: false,
                                            Detail: $"{subject}: invalid without native detail."))
                                    : Some<ExchangeEvidence>(new ExchangeEvidence.NativeCase(
                                        Surface: nameof(Rhino.Runtime.CommonObject.IsValidWithLog),
                                        Succeeded: valid,
                                        Detail: $"{subject}: {log}"));
                                return (Valid: valid, Evidence: native);
                            },
                            None: () => (Valid: false, Evidence: Some<ExchangeEvidence>(new ExchangeEvidence.NativeCase(
                                Surface: nameof(Rhino.Runtime.CommonObject.IsValidWithLog),
                                Succeeded: false,
                                Detail: $"{subject}: geometry unrealized (null native pointer)."))));
                    });
                    int invalid = checks.Count(static check => !check.Valid);
                    Seq<ExchangeEvidence> native = checks.Choose(static check => check.Evidence);
                    Seq<ExchangeEvidence> severed = graph.Broken().Map(static link =>
                        (ExchangeEvidence)new ExchangeEvidence.BrokenLinkCase(Link: link));
                    return Fin.Succ(value: ArchiveReceipt.Of(
                        yield: new ArchiveYield.VerdictCase(Verdict: new ArchiveVerdict(
                            Valid: invalid == 0 && severed.IsEmpty,
                            InvalidObjects: invalid,
                            BrokenLinks: severed.Count)),
                        evidence: ctx.Evidence + native + severed));
                })),
            diffCase: static (ctx, request) =>
                from sourceBytes in ArchiveBytes(archive: ctx.Archive, policy: ArchiveWritePolicy.Current, op: ctx.Op)
                from sourceGraph in Graph(archive: ctx.Archive, op: ctx.Op)
                from other in Open(source: request.Other, slice: ArchiveSlice.Full, op: ctx.Op)
                from receipt in other.Lease.Use(otherArchive =>
                    from otherBytes in ArchiveBytes(archive: otherArchive, policy: ArchiveWritePolicy.Current, op: ctx.Op)
                    from otherGraph in Graph(archive: otherArchive, op: ctx.Op)
                    select ArchiveReceipt.Of(
                        yield: new ArchiveYield.DeltaCase(Delta: ArchiveDelta.Of(
                            sourceKey: ContentHash.Of(canonicalBytes: sourceBytes),
                            otherKey: ContentHash.Of(canonicalBytes: otherBytes),
                            source: sourceGraph,
                            other: otherGraph)),
                        evidence: ctx.Evidence + other.Evidence))
                select receipt,
            batchCase: static (ctx, request) => {
                ArchiveFold folded = request.Program.Map(static (inner, index) => (Inner: inner, Index: index)).Fold(
                    new ArchiveFold(Steps: Seq<ArchiveStep>(), Stopped: false),
                    (state, item) => {
                        if (state.Stopped) {
                            return state;
                        }
                        bool mutationAttempted = item.Inner.Mutates();
                        ArchiveStep step = Dispatch(
                            source: ctx.Source,
                            archive: ctx.Archive,
                            request: item.Inner,
                            evidence: Seq<ExchangeEvidence>(),
                            op: ctx.Op).Match<ArchiveStep>(
                                Succ: receipt => new ArchiveStep.SucceededCase(
                                    Index: item.Index,
                                    MutationAttempted: receipt.Evidence.Exists(static fact =>
                                        fact is ExchangeEvidence.MutationCase { Attempted: true }),
                                    Receipt: receipt),
                                Fail: failure => new ArchiveStep.FailedCase(
                                    Index: item.Index,
                                    MutationAttempted: mutationAttempted,
                                    MutationMayRemain: mutationAttempted,
                                    Failure: failure,
                                    Evidence: mutationAttempted
                                        ? Seq<ExchangeEvidence>(new ExchangeEvidence.MutationCase(
                                            Surface: item.Inner.GetType().Name,
                                            Attempted: true,
                                            Committed: false,
                                            MayRemain: true,
                                            UndoRecord: None))
                                        : Seq<ExchangeEvidence>()));
                        return new ArchiveFold(
                            Steps: state.Steps.Add(step),
                            Stopped: step is ArchiveStep.FailedCase);
                    });
                ArchiveReceipt receipt = ArchiveReceipt.Program(requested: request.Program.Count, steps: folded.Steps);
                return Fin.Succ(value: receipt with { Evidence = ctx.Evidence + receipt.Evidence });
            });

    private sealed record ArchiveFold(Seq<ArchiveStep> Steps, bool Stopped);

    internal static Fin<Landed<Option<string>>> Land(
        File3dm archive,
        DocumentPath target,
        ArchiveWritePolicy policy,
        OutputPolicy output,
        Op op) => output.Land(
        target: target,
        codec: FileCodec.ThreeDm,
        stage: temporary => op.Catch(() => {
            bool wrote = archive.WriteWithLog(path: temporary, options: policy.Host(), errorLog: out string log);
            Option<string> native = Optional(log).Filter(static text => !string.IsNullOrWhiteSpace(value: text));
            return wrote
                ? Fin.Succ(value: native)
                : Fin.Fail<Option<string>>(error: op.InvalidResult(
                    detail: $"{nameof(File3dm.WriteWithLog)}: {native.IfNone("returned false without native detail.")}"));
        }),
        validate: Some<Func<byte[], Fin<Unit>>>(bytes => ValidateArchiveBytes(bytes: bytes, op: op)),
        key: op);

    private static Seq<ExchangeEvidence> Committed(string surface, Landed<Option<string>> written) =>
        Seq<ExchangeEvidence>(new ExchangeEvidence.MutationCase(
            Surface: surface,
            Attempted: true,
            Committed: true,
            MayRemain: false,
            UndoRecord: None))
        + written.Stage.Map(text => (ExchangeEvidence)new ExchangeEvidence.NativeCase(
            Surface: nameof(File3dm.WriteWithLog),
            Succeeded: true,
            Detail: text,
            Target: Some(written.Target))).ToSeq();

    private static Fin<byte[]> ArchiveBytes(File3dm archive, ArchiveWritePolicy policy, Op op) =>
        op.Catch(() => Optional(archive.ToByteArray(options: policy.Host())).ToFin(Fail: op.InvalidResult()))
            .Bind(bytes => ValidateArchiveBytes(bytes: bytes, op: op).Map(_ => bytes));

    private static Fin<Unit> ValidateArchiveBytes(byte[] bytes, Op op) =>
        from _nonempty in guard(bytes.Length > 0, op.InvalidResult()).ToFin()
        from archive in op.Catch(() => Optional(File3dm.FromByteArray(bytes: bytes)).ToFin(Fail: op.InvalidResult()))
        from _released in new Lease<File3dm>.Owned(Value: archive).Use(static _ => Fin.Succ(value: unit))
        select unit;

    private static Seq<TResult> ProjectOwned<T, TResult>(T[]? values, Func<T, TResult> project)
        where T : class, IDisposable =>
        toSeq(values ?? Array.Empty<T>())
            .Map(value => new Lease<T>.Owned(Value: value).Use(project))
            .ToSeq();

    private static Fin<ArchiveReceipt> InspectPath(DocumentPath path, Op op) =>
        op.Catch(() => {
            string createdBy = string.Empty, lastEditedBy = string.Empty;
            int revision = 0;
            DateTime createdOn = default, lastEditedOn = default;
            bool hasRevision = File3dm.ReadRevisionHistory(
                path: path.Value, createdBy: out createdBy, lastEditedBy: out lastEditedBy,
                revision: out revision, createdOn: out createdOn, lastEditedOn: out lastEditedOn);
            File3dm.ReadApplicationData(path: path.Value, applicationName: out string appName, applicationUrl: out string appUrl, applicationDetails: out string appDetails);
            bool anchored;
            using (EarthAnchorPoint? anchor = File3dm.ReadEarthAnchorPoint(path: path.Value)) {
                anchored = anchor is { } value && value.EarthLocationIsSet();
            }
            Seq<(string Name, Guid Id)> layouts = ProjectOwned(
                values: File3dm.ReadPageViews(path: path.Value),
                project: static view => (view.Name, view.Viewport.Id));
            int dimensionStyles = ProjectOwned(
                values: File3dm.ReadDimensionStyles(path: path.Value),
                project: static _ => unit).Count;
            return Fin.Succ(value: ArchiveReceipt.Of(
                yield: new ArchiveYield.MetadataCase(Metadata: new ArchiveMetadata(
                    Notes: Optional(File3dm.ReadNotes(path: path.Value)).Filter(static text => !string.IsNullOrWhiteSpace(value: text)),
                    ArchiveVersion: File3dm.ReadArchiveVersion(path: path.Value),
                    Revision: hasRevision ? Some((createdBy, lastEditedBy, revision, createdOn, lastEditedOn)) : None,
                    Application: string.IsNullOrWhiteSpace(value: appName) ? None : Some((appName, appUrl, appDetails)),
                    EarthAnchored: anchored,
                    Layouts: layouts,
                    DimensionStyles: dimensionStyles,
                    HasPreview: Optional(File3dm.ReadPreviewImage(path: path.Value)).Map(static bitmap => {
                        using System.Drawing.Bitmap preview = bitmap;
                        return true;
                    }).IfNone(noneValue: false)))));
        });

    private static Fin<ArchiveReceipt> Extract(
        File3dm archive,
        DocumentPath folder,
        OutputPolicy output,
        Op op) => op.Catch(() => {
        Seq<(File3dmEmbeddedFile File, string Name)> files = toSeq(archive.EmbeddedFiles)
            .Map(static file => (File: file, Name: System.IO.Path.GetFileName(file.Filename)));
        return from _names in guard(
                   files.ForAll(static row => !string.IsNullOrWhiteSpace(value: row.Name))
                   && files.Map(static row => row.Name.ToUpperInvariant()).Distinct().Count == files.Count,
                   op.InvalidInput()).ToFin()
               from landed in files.TraverseM(row => ExtractOne(
                       file: row.File,
                       name: row.Name,
                       folder: folder,
                       output: output,
                       op: op))
                   .As()
               select ArchiveReceipt.Of(yield: new ArchiveYield.ExtractedCase(Files: landed));
    });

    private static Fin<(string Name, DocumentPath Target, UInt128 ContentKey)> ExtractOne(
        File3dmEmbeddedFile file,
        string name,
        DocumentPath folder,
        OutputPolicy output,
        Op op) =>
        from target in op.Catch(() => Fin.Succ(value: DocumentPath.Create(value: System.IO.Path.Join(folder.Value, name))))
        from landed in output.Land(
            target: target,
            codec: None,
            stage: temporary => op.Catch(() => op.Confirm(success: file.SaveToFile(filename: temporary))),
            key: op)
        select (
            Name: name,
            Target: landed.Target,
            ContentKey: landed.ContentKey);

    private static Seq<ResourceNode> Rows<T>(IEnumerable<T> table, ResourceRole role, Func<T, string> name, Func<T, Option<Guid>> id) =>
        toSeq(table).Map(row => new ResourceNode(Role: role, Name: name(arg: row), Id: id(arg: row)));

    private static Fin<ArchiveGraph> Graph(File3dm archive, Op op) =>
        op.Catch(() => {
            Seq<(int Index, ResourceNode Node)> layerRows = toSeq(archive.AllLayers).Map(static layer =>
                (layer.Index, new ResourceNode(Role: ResourceRole.Layer, Name: layer.Name, Id: Some(layer.Id))));
            HashMap<int, ResourceNode> layerByIndex = toHashMap(layerRows);
            Seq<ResourceNode> nodes = layerRows.Map(static row => row.Node)
                + Rows(archive.AllMaterials, ResourceRole.Material, static row => row.Name, static row => Some(row.Id))
                + Rows(archive.AllGroups, ResourceRole.Group, static row => row.Name, static row => Some(row.Id))
                + Rows(archive.AllInstanceDefinitions, ResourceRole.Block, static row => row.Name, static row => Some(row.Id))
                + Rows(archive.AllViews, ResourceRole.ModelView, static row => row.Name, static row => Some(row.Viewport.Id))
                + Rows(archive.AllNamedViews, ResourceRole.NamedView, static row => row.Name, static row => Some(row.Viewport.Id))
                + Rows(archive.EmbeddedFiles, ResourceRole.Embedded, static row => row.Filename, static _ => None)
                + Rows(archive.RenderMaterials, ResourceRole.RenderMaterial, static row => row.Name, static row => Some(row.Id))
                + Rows(archive.RenderEnvironments, ResourceRole.RenderEnvironment, static row => row.Name, static row => Some(row.Id))
                + Rows(archive.RenderTextures, ResourceRole.RenderTexture, static row => row.Name, static row => Some(row.Id));
            Seq<ResourceLink> links = toSeq(archive.Objects)
                .Choose(entry => Optional(entry.Attributes).Map(attributes =>
                    new ResourceLink(
                        From: new ResourceNode(Role: ResourceRole.Instance, Name: entry.Name ?? string.Empty, Id: Some(entry.Id)),
                        To: layerByIndex.Find(attributes.LayerIndex).IfNone(() => new ResourceNode(Role: ResourceRole.Layer, Name: string.Empty, Id: None)),
                        Relation: ResourceRole.Layer)));
            return Fin.Succ(value: new ArchiveGraph(Nodes: nodes, Links: links));
        });

    private static Fin<ArchiveReceipt> MetadataOf(File3dm archive, Op op) =>
        op.Catch(() => {
            bool anchored;
            using (EarthAnchorPoint anchor = archive.EarthAnchorPoint) {
                anchored = anchor.EarthLocationIsSet();
            }
            return Fin.Succ(value: ArchiveReceipt.Of(
                yield: new ArchiveYield.MetadataCase(Metadata: new ArchiveMetadata(
                    Notes: Optional(archive.Notes.Notes).Filter(static text => !string.IsNullOrWhiteSpace(value: text)),
                    ArchiveVersion: archive.ArchiveVersion,
                    Revision: archive.Revision > 0 || !string.IsNullOrWhiteSpace(value: archive.CreatedBy)
                        ? Some((archive.CreatedBy, archive.LastEditedBy, archive.Revision, archive.Created, archive.LastEdited))
                        : None,
                    Application: string.IsNullOrWhiteSpace(value: archive.ApplicationName)
                        ? None
                        : Some((archive.ApplicationName, archive.ApplicationUrl, archive.ApplicationDetails)),
                    EarthAnchored: anchored,
                    Layouts: Seq<(string, Guid)>(),
                    DimensionStyles: archive.AllDimStyles.Count,
                    HasPreview: Optional(archive.GetPreviewImage()).Map(static bitmap => {
                        using System.Drawing.Bitmap preview = bitmap;
                        return true;
                    }).IfNone(noneValue: false))),
                evidence: Seq<ExchangeEvidence>(new ExchangeEvidence.DegradedCase(
                    Surface: nameof(MetadataOf),
                    Detail: "Byte ingress projects the in-memory header; the layout roster is a path-only read."))));
        });
}
```

```mermaid
---
config:
  theme: base
  look: classic
  layout: elk
  flowchart:
    curve: linear
    padding: 25
  themeVariables:
    darkMode: true
    fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"
    useGradient: false
    dropShadow: "none"
    background: "#282A36"
    primaryColor: "#44475A"
    primaryTextColor: "#F8F8F2"
    primaryBorderColor: "#BD93F9"
    lineColor: "#FF79C6"
    textColor: "#F8F8F2"
    edgeLabelBackground: "#21222C"
    labelBackgroundColor: "#21222C"
  themeCSS: ".nodeLabel{font-size:13px;font-weight:500}.edgeLabel{font-size:12px;font-weight:500}.cluster-label .nodeLabel{font-size:13.5px;font-weight:700;letter-spacing:.08em}.edge-thickness-normal{stroke-width:2px}.edge-thickness-thick{stroke-width:3px}.edge-pattern-dashed,.edge-pattern-dotted{stroke-width:1.5px;stroke-dasharray:4 6}.node rect,.node circle,.node polygon,.node path,.node .outer-path{stroke-width:1.5px;filter:none!important}.cluster rect{stroke-width:1px!important;stroke-dasharray:5 4!important;filter:none!important}.marker path{transform:scale(.8);transform-origin:5px 5px}.marker circle{transform:scale(.48);transform-origin:5px 5px}.edgeLabel rect{transform-box:fill-box;transform-origin:center;transform:scale(1.1,1.2)}"
---
flowchart LR
    accTitle: Standalone Rhino archive transaction rail
    accDescr: Archive source and request enter one route discriminator, static inspection bypasses materialization, every materialized arm folds detached values or an ordered program into one receipt, and typed evidence traces the terminal.
    Source([ArchiveSource]) --> Entry[[Archives.Apply]]
    Request([ArchiveOp]) --> Entry
    Entry --> Route{Dispatch route?}
    Route -->|path inspect| Header[/Static header reads/]
    Route -->|materialized op| Lease[(Owned File3dm lease)]
    Lease --> Dispatch[[Total Switch]]
    Dispatch -->|graph or metadata| Projection[Detached projection]
    Dispatch -->|bytes or write| Keyed[Verified bytes and content key]
    Dispatch -->|batch| Program[Ordered ArchiveProgram]
    Header --> Receipt
    Projection --> Receipt[/ArchiveReceipt/]
    Keyed --> Receipt
    Program --> Receipt
    Evidence[(ExchangeEvidence)] -.->|trace| Receipt
    linkStyle 3,4,6,7,8 stroke:#FF79C6,color:#F8F8F2
    linkStyle 9,10,11,12 stroke:#50FA7B,color:#F8F8F2
    linkStyle 13 stroke:#6272A4,color:#F8F8F2,stroke-width:1.5px,stroke-dasharray:4 6
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef success fill:#50FA7BBF,stroke:#50FA7B,color:#282A36
    classDef data fill:#FFB86CBF,stroke:#FFB86C,color:#282A36
    classDef boundary fill:#282A36,stroke:#BD93F9,color:#F8F8F2
    class Route,Dispatch,Projection,Keyed,Program primary
    class Receipt success
    class Lease,Evidence data
    class Source,Request,Entry,Header boundary
```
