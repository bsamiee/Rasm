# [BIM_ISSUE_EXCHANGE]

The BCF 2.1/3.0 issue-exchange owner: one closed `BcfFile`/`BcfTopic`/`BcfComment`/`BcfViewpoint` record family anchored on IFC GlobalIds and carried at the FULL `Bcf30` schema surface — topic references (`RelatedTopics`/`DocumentReferences`/`BimSnippet`/`ReferenceLinks`/header `Files`), modification provenance (`ModifiedDate`/`ModifiedAuthor`/`Index`/`ServerAssignedId`), the typed `BcfCamera` perspective/orthogonal union (aspect ratio and view-to-world scale, never an fov-0 sentinel), the viewpoint markup payload (`Coloring`/`Lines`/`ClippingPlanes`/`Bitmaps`/`DefaultVisibility`/`ViewSetupHints`), and the archive-level `BcfProject`/`BcfVocabulary`/`BcfDocument` members a `WithDefaults()`-only author would drop. The `.bcfzip` codec composes the `Smino.Bcf.Toolkit` surface at both depths — the streaming `BcfExtensions` per-part parse (`GetVersionFromStreamArchive` sniff, `ParseMarkups`/`ParseExtensions`/`ParseProject`/`ParseDocuments`) for a native 3.0 container, the `Worker.BcfFromStream` up-conversion for a 2.1 source, and the fluent `BcfBuilder` authoring fold on write — plus a `BcfApi` BCF-API 3.0 resource projection the `Rasm.Compute/Runtime/transport#TRANSPORT_AXIS` transport issues. BCF is an issue/coordination container, never a geometry-or-model interchange row — the `.bcfzip` codec is self-owned in `coordination` and is NOT a row on the `Exchange/format#FORMAT_AXIS` geometry-format axis, because BCF carries issues and viewpoints, never an `ElementGraph` or `ImportedGeometry` import. Viewpoints anchor on IFC GlobalIds — each the seam `Rasm.Element/Graph/element#NODE_MODEL` `Node.Object.ExternalId` the `Projection/semantic#SEMANTIC_PROJECTOR` projects 1:1 from `IfcRoot.GlobalId` [H6] — so the issue payload aligns to element GlobalIds only, never to the geometry codec axis. The page is HOST-LOCAL.

## [01]-[INDEX]

- [02]-[BCF_ARCHIVE]: the `BcfFile` archive root (`BcfTopic`/`BcfComment`/`BcfViewpoint` family, `BcfCamera` union, `BcfProject`/`BcfVocabulary`/`BcfDocument` archive members), the `.bcfzip` codec over the `Smino.Bcf.Toolkit` `BcfExtensions`/`Worker`/`BcfBuilder` surface.
- [03]-[TS_PROJECTION]: the `BcfWire` host-free JSON producer the TS UI BCF anchor and live-binding decode — the Mapperly-generated wire projection over the self-owned source-generated `BcfWireContext` with the `BcfStatus` string discriminant and the `IfcGuid` component anchor, plus the `BcfApi` BCF-API 3.0 resource union covering the full project-scoped conversation with spec-dialect snake_case write bodies (`BcfApiContext`).

## [02]-[BCF_ARCHIVE]

- Owner: `BcfFile` the archive root — the topic set plus the archive-level members every `.bcfzip` carries: `BcfProject` the project identity, `BcfVocabulary` the project extension vocabularies (topic types/statuses/priorities/labels/users/snippet-types/stages — the rows the `Review/coordination#SIGN_OFF` board lanes and topic-type axis read), `BcfDocument` the embedded document library, and the `BcfGeneration` source evidence; `BcfTopic` the issue record at the FULL `Bcf30.Topic` surface — core issue fields, body metadata, modification provenance, reference links, related-topic joins, document references, the BIM snippet, and the markup header IFC-file refs; `BcfComment` the threaded comment with its own modification provenance; `BcfViewpoint` the saved view — the typed `BcfCamera` union, component selection, visibility (default + exceptions + the `BcfViewSetupHints` spaces/space-boundaries/openings render hints), per-component `Coloring`, redline `Lines`, section `ClippingPlanes`, and `Bitmaps`; `BcfArchive` the `.bcfzip` codec; `BcfApi` the BCF-API 3.0 resource projection of the same family the Compute transport issues.
- Entry: `BcfArchive.Read(ReadOnlyMemory<byte> bcfzip, Op key)` lands BOTH fault classes on ONE `Fin` through the self-flattening `Try.lift(...).Run().MapFail(...).Bind(r => r)` funnel — container faults arrive thrown, the viewpoint camera-XOR fault arrives already typed — and sniffs the container generation through `BcfExtensions.GetVersionFromStreamArchive`, streams a native 3.0 archive per-part (`ParseMarkups<Bcf30.Markup, Bcf30.VisualizationInfo>` + `ParseExtensions<Bcf30.Extensions>` + `ParseProject<Bcf30.ProjectInfo>` + `ParseDocuments<Bcf30.DocumentInfo>` — no second full-graph materialization), up-converts a 2.1 source through `Worker.BcfFromStream` (its `using BcfToolkit.Model.Bcf30;` binds the generic target), and folds both onto one `BcfFile`, lifting each held `{topicGuid}/{Reference}` bitmap part into `BcfFile.Blobs`; `BcfArchive.Write(BcfFile file, Op key)` seeds the builder from the file's own vocabulary/project/documents (`BcfBuilder.SetExtensions`/`SetProject`/`SetDocument`; `WithDefaults()` only when the file carries no vocabulary), folds each topic through `BcfBuilder.AddMarkup`, emits the `.bcfzip` bytes through `Worker.ToBcf(bcf, BcfVersionEnum.Bcf30)`, and completes the container with each topic's bitmap parts from `BcfFile.Blobs` — a reference the store cannot answer refuses the write — `Fin<T>` aborts on a malformed archive (`Model/faults#FAULT_BAND` `BimFault.ModelRejected`, band 2600, `Expected`-derived, lifting BARE through the `Try.lift(...).Run().MapFail(...)` funnel with NO `.ToError()` hop; the `MapFail` lambda closes over the `Op key`, so it is NOT `static`); `BcfFile.Of(topics)` is the authoring factory a clash or IDS fold seeds a default-vocabulary file from.
- Auto: `Read` projects every `Bcf30.Topic` column onto `BcfTopic` — including `Index`/`ModifiedDate`/`ModifiedAuthor`/`ServerAssignedId`/`ReferenceLinks`/`RelatedTopics` (`TopicRelatedTopicsRelatedTopic.Guid`)/`DocumentReferences` (`DocumentGuid` XOR `Url` per BCF)/`BimSnippet` and the `Markup.Header` `Files` rows (`Filename`/`Date`/`Reference`/`IfcProject`/`IfcSpatialStructureElement`/`IsExternal`), the verbatim `TopicStatus` token landing on `StatusLabel` beside the parsed `BcfStatus` lifecycle projection and the topic order re-derived deterministically off the schema `Index`-then-`Guid` (a `ConcurrentBag` parse is bag-racy) — reads each `Bcf30.ViewPoint` whole (the `PerspectiveCamera` with `FieldOfView`+`AspectRatio`, the `OrthogonalCamera` with `ViewToWorldScale`+`AspectRatio` — each transcribed onto its typed `BcfCamera` case and handed to the ONE `BcfCamera.Admit` gate, never a lossy shared quadruple), the `Components.Selection`/`Visibility` (`DefaultVisibility` + `Exceptions` + the nested `ViewSetupHints` `SpacesVisible`/`SpaceBoundariesVisible`/`OpeningsVisible`)/`Coloring` (`ComponentColoringColor.Color` + component GlobalIds), the `Lines`/`ClippingPlanes`/`Bitmaps` markup payload, and the `SnapshotData` base64; `Write` re-authors ALL of it through the nested builders (`MarkupBuilder.SetIndex`/`SetModifiedDate`/`SetModifiedAuthor`/`SetServerAssignedId`/`AddReferenceLink`/`AddRelatedTopic`/`AddDocumentReference`/`SetBimSnippet`/`AddHeaderFile`, `CommentBuilder.SetModifiedDate`/`SetModifiedAuthor`, `VisualizationInfoBuilder.SetPerspectiveCamera`/`SetOrthogonalCamera`/`AddSelection`/`SetVisibility`/`AddColoring`/`AddLine`/`AddClippingPlane`/`AddBitmap`, `VisibilityBuilder.SetViewSetupHints`) so the write round-trips exactly what the read captured.
- Receipt: the `BcfFile` is the coordination evidence — topics, vocabulary, project, documents, and the source generation — so a CDE or viewer round-trips one typed root through the `Worker` `.bcfzip` codec, and the `BcfApi` REST projection rides the same topic family so the file and REST forms carry one vocabulary, never two.
- Packages: Smino.Bcf.Toolkit, NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new BCF entity is one record on the family projected from the `Smino.Bcf.Toolkit` graph, its members verified at `.api/api-smino-bcf-toolkit` before the fence spells them; a new topic or viewpoint column is one trailing-defaulted field the read/write folds each gain one line for; a new BCF version is one `BcfVersionEnum` the `Worker` converter already discriminates; the REST projection is one `BcfResource` case over Compute's transport; never a row on the geometry-format axis and never a second issue store.
- Boundary: a board mutation mints the `Exchange/events#EVENTS` `BimEvent.IssueMutated` row at the composing rail's edge — the topic `Guid`, one `BimIssueMutation` row (`topic-opened`/`topic-revised`/`comment-added`/`viewpoint-added`/`status-advanced`), the optional comment `Guid`, and the viewpoint-anchored `GlobalId` set, subject the topic `Guid` — envelope seal and transport being the events owner's and a second emit path here the deleted form; the container read/write is the `Smino.Bcf.Toolkit` surface and the hand-rolled `ZipArchive`/`XDocument` codec is the retired form — the streaming `BcfExtensions` per-part parse serves a native 3.0 container and `Worker.BcfFromStream` up-converts 2.1, so the projection is single-version off the concrete `Bcf30.*` graph; the ONE carve is the bitmap part, because the toolkit model carries no bitmap payload member and its writer emits none (`Bcf30.Bitmap` is Format/Reference/geometry only, decompile-proven), so the codec itself lifts held parts into `BcfFile.Blobs` on read and appends them on write, refusing a `BcfBitmap.Reference` the store cannot answer — a written archive whose markup references a part the container never holds is unrepresentable (`BcfVersion.TryParse` accepts only `"2.1"`/`"3.0"`; the thin `IBcf`/`ITopic` markers carry no rich field); a `BcfTopic`/`BcfViewpoint` that drops a `Bcf30` schema column is the SLICED form this owner deletes — the family carries the whole surface, trailing-defaulted so a minimal author still constructs the core nine; the closed `BcfStatus` is the lifecycle projection and `StatusLabel` carries the project-vocabulary status verbatim — a write that re-emits the parsed enum over an extension status ("Under Review" rewritten "Open") is the deleted lossy form, `StatusToken` the one write-side election; the camera is the closed `BcfCamera` union carried `Option`-valued, admitted by the ONE `BcfCamera.Admit` gate both ingress arms compose under the spec's orthogonal-XOR-perspective law — an fov-0 sentinel discriminating orthographic from perspective, a silent perspective preference when both are declared, and a zero-filled default mint when neither is are all deleted forms (`BOUNDARY_ADMISSION`: the interior never sees a sentinel, and a selection-only viewpoint is legal BCF whose absence stays typed rather than becoming a degenerate origin view every receiving tool renders); `BcfComment.ReplyToGuid` is the REST-lane conversation join carried on the ONE comment family — BCF 3.0 removed the reply join from the markup schema, so the archive read lands typed absence and the archive write emits nothing, and a REST-only comment mirror beside the domain record is the deleted fork; the async `Worker`/`BcfExtensions` surface runs to completion at this codec boundary (the one language-owned `GetAwaiter().GetResult()` bridge the boundary kernel admits) and every throw lowers onto `Fin<T>` as a BARE `BimFault.ModelRejected`; both codec bodies name their statement exemption — the decode's sequential stream-owning parse and the encode's mutation-shaped fluent builder are the foreign surfaces' own shapes, and no domain branching hides among them; viewpoints anchor on IFC GlobalIds — the seam `Node.Object.ExternalId` [H6] — never a geometry handle, and a `BcfViewpoint` carrying the retired `BimModel`/`BimElement` record is the deleted form; the BCF-API REST shape rides the `Rasm.Compute/Runtime/transport#TRANSPORT_AXIS` transport and a transport (client/retry/auth) minted here is the named seam violation; the durable lineage + CDE store is the app-platform Persistence owner's, joined by the IFC GlobalId the `Rasm.Persistence/Element/identity#ELEMENT_IDENTITY` `ElementIdentity.GlobalIds` map maintains and surfaced through the `Rasm.Persistence/Version/provenance#CAUSAL_DAG` W3C-PROV-JSON egress — a BCF topic and the durable lineage bind one element without this owner gaining a durable-op-log row or Persistence gaining a second BCF schema; this codec is the branch's ONE `.bcfzip` custodian under the branch custody ruling — `Rasm.Persistence/Ingest/issue` holds no container surface and joins by typed rows across the seam, the composition root transcribing the read `BcfFile` onto `IssueTopic` rows under the `BcfTopic`⇄`IssueTopic` correspondence law (`Status`⇄`StatusToken` verbatim, stamps as this codec's `Instant`, camera absence typed) and handing egress rows back through `BcfArchive.Write`, so a container codec at the durable end is the deleted form.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Linq;
using BcfToolkit;
using BcfToolkit.Builder.Bcf30;
using BcfToolkit.Model;
using BcfToolkit.Utils;
using LanguageExt;
using NodaTime;
using Bcf30 = BcfToolkit.Model.Bcf30;
using Op = Rasm.Domain.Op;
using Vector3 = System.Numerics.Vector3;
using static LanguageExt.Prelude;

// The BCF/coordination DOMAIN owner namespace — the `coordination` sub-domain the ARCHITECTURE seams name
// and AppUi `Collab/issues` consumes as `Rasm.Bim.Coordination.BcfTopic`/`BcfComment`/`BcfViewpoint`/`BcfStatus`;
// the codemap `Review/Coordination.cs`/`Review/Issues.cs` files emit this namespace, not folder-derived `Review`.
namespace Rasm.Bim.Coordination;

// --- [TYPES] ------------------------------------------------------------------------------
// The admitted-generation evidence — the boundary's own 2-case mirror of BcfVersionEnum so the
// interior record family never carries a toolkit type.
public enum BcfGeneration : byte { Bcf21 = 0, Bcf30 = 1 }

// --- [MODELS] -----------------------------------------------------------------------------
// The closed camera union: perspective carries the field of view, orthogonal the view-to-world
// scale, BOTH the BCF 3.0 aspect ratio — the typed discriminant replaces the fov-0 sentinel a
// consumer had to decode, and a lossless orthographic round-trip stops collapsing to fov 0.
[Union]
public abstract partial record BcfCamera {
    private BcfCamera() { }

    public sealed record Perspective(Vector3 Position, Vector3 Direction, Vector3 Up, double FieldOfViewDeg = 60d, double AspectRatio = 0d) : BcfCamera;
    public sealed record Orthogonal(Vector3 Position, Vector3 Direction, Vector3 Up, double ViewToWorldScale = 1d, double AspectRatio = 0d) : BcfCamera;

    // The ONE camera admission BOTH ingress arms compose — the archive's Bcf30.VisualizationInfo read and the
    // BCF-API viewpoint response — each arm projecting its own foreign shape onto the typed case first (a pure
    // transcription that decides nothing) and this gate owning the whole decision. BCF declares
    // orthogonal_camera XOR perspective_camera: BOTH present is a malformed viewpoint that rails typed, exactly
    // one admits, and NEITHER is the legal selection-only viewpoint carried as typed absence. The two prior
    // spellings are deleted — a silent perspective preference discards the author's other declared view without a
    // word, and a default Perspective(default, default, default) mint publishes a degenerate origin view with a
    // zero direction as authored intent, which every receiving tool renders and none can distinguish from a real
    // camera.
    public static Fin<Option<BcfCamera>> Admit(Option<Perspective> perspective, Option<Orthogonal> orthogonal, string viewpoint, Op key) =>
        (perspective.IsSome, orthogonal.IsSome) switch {
            (true, true) => Fin.Fail<Option<BcfCamera>>(new BimFault.ModelRejected(key, $"bcf-camera-xor:{viewpoint}")),
            (true, _)    => Fin.Succ(perspective.Map(static p => (BcfCamera)p)),
            (_, true)    => Fin.Succ(orthogonal.Map(static o => (BcfCamera)o)),
            _            => Fin.Succ(Option<BcfCamera>.None),
        };
}

// The viewpoint markup payload rows — each mirrors one Bcf30 schema type host-free; DocumentGuid
// XOR Url discriminates an internal library document from an external one per the BCF schema.
// BcfViewSetupHints mirrors the schema's Visibility-nested ViewSetupHints — the spaces/space-boundaries/openings
// render hints a viewer applies before the exception set, so a clash inside a space stays visible.
public sealed record BcfHeaderFile(string Filename, Option<Instant> Date, string Reference, string IfcProject, string IfcSpatialStructureElement, bool IsExternal = true);
public sealed record BcfDocumentReference(string Guid, Option<string> DocumentGuid, Option<string> Url, string Description);
public sealed record BcfBimSnippet(string SnippetType, string Reference, string ReferenceSchema, bool IsExternal);
public sealed record BcfColoring(string Color, Seq<string> GlobalIds);
public sealed record BcfLine(Vector3 Start, Vector3 End);
public sealed record BcfClippingPlane(Vector3 Location, Vector3 Direction);
public sealed record BcfBitmap(string Format, string Reference, Vector3 Location, Vector3 Normal, Vector3 Up, double Height);
public readonly record struct BcfViewSetupHints(bool SpacesVisible, bool SpaceBoundariesVisible, bool OpeningsVisible);

// The saved view at the full VisualizationInfo surface. VisibilityExceptions pairs with
// DefaultVisibility (false => the exceptions are the visible set; true => the hidden set) — the
// prior VisibleGlobalIds name lied under DefaultVisibility=true, so the pair replaces it.
public sealed record BcfViewpoint(
    string Guid,
    Option<BcfCamera> Camera,
    Seq<string> SelectedGlobalIds,
    Seq<string> VisibilityExceptions,
    Option<ReadOnlyMemory<byte>> Snapshot,
    bool DefaultVisibility = false,
    Seq<BcfColoring> Coloring = default,
    Seq<BcfLine> Lines = default,
    Seq<BcfClippingPlane> ClippingPlanes = default,
    Seq<BcfBitmap> Bitmaps = default,
    Option<int> Index = default,
    Option<BcfViewSetupHints> ViewSetupHints = default);

// ReplyToGuid is the threaded-conversation join. It is a REST-lane column only: BCF 3.0 removed the reply join
// from the markup schema, so `Bcf30.Comment` declares no member and `CommentBuilder` no setter — the archive read
// lands None and the archive write emits nothing, while the BCF-API `comment` resource carries it in both
// directions. The column stays on the domain record rather than on a REST-only mirror, because a topic read from a
// server and written to a container is ONE comment family and a second record would fork it.
public sealed record BcfComment(
    string Guid,
    string Author,
    string Text,
    Option<string> ViewpointGuid,
    Instant Date,
    Option<Instant> ModifiedDate = default,
    string ModifiedAuthor = "",
    Option<string> ReplyToGuid = default);

// The issue record at the FULL Bcf30.Topic surface: the core nine positional, every further
// schema column trailing-defaulted so a clash author or a minimal caller still constructs the
// core — a Bcf30.Topic field dropped here slices the concept, so none is. Status is the closed
// lifecycle projection; StatusLabel preserves the project-vocabulary TopicStatus token verbatim
// (BCF status is an extensions-defined free string), so a round-trip never rewrites "Under Review"
// to "Open" — StatusToken is the ONE write-side election both the archive and REST folds read.
public sealed record BcfTopic(
    string Guid,
    string Title,
    BcfStatus Status,
    string TopicType,
    string Priority,
    string Author,
    Instant CreationDate,
    Seq<BcfComment> Comments,
    Seq<BcfViewpoint> Viewpoints,
    string Description = "",
    string AssignedTo = "",
    string Stage = "",
    Option<Instant> DueDate = default,
    Seq<string> Labels = default,
    Option<int> Index = default,
    Option<Instant> ModifiedDate = default,
    string ModifiedAuthor = "",
    string ServerAssignedId = "",
    Seq<string> ReferenceLinks = default,
    Seq<string> RelatedTopics = default,
    Seq<BcfDocumentReference> DocumentReferences = default,
    Option<BcfBimSnippet> BimSnippet = default,
    Seq<BcfHeaderFile> Files = default,
    string StatusLabel = "") {

    public string StatusToken => StatusLabel.Length > 0 ? StatusLabel : Status.ToString();
}

// The archive-level members WithDefaults()-only authoring dropped: the project identity, the
// extension vocabularies (the Review/coordination#SIGN_OFF board lanes and topic-type axis read
// these rows), and the embedded document library the DocumentReferences DocumentGuid joins.
public sealed record BcfProject(string ProjectId, string Name);
public sealed record BcfDocument(string Guid, string Filename, string Description, ReadOnlyMemory<byte> Data);
public sealed record BcfVocabulary(
    Seq<string> TopicTypes, Seq<string> TopicStatuses, Seq<string> Priorities, Seq<string> TopicLabels,
    Seq<string> Users, Seq<string> SnippetTypes, Seq<string> Stages);

// The archive root: what a .bcfzip IS — topics plus project/vocabulary/documents plus the
// admitted-generation evidence. Vocabulary None on write seeds the toolkit defaults. Blobs is
// the bitmap-part payload store keyed by `BcfBitmap.Reference`: Read lifts each held container
// part into it, an authoring caller seeds it beside the `BcfBitmap` rows it mints (the reference
// doubling as the caller's own blob key), and Write refuses a reference the store cannot answer.
public sealed record BcfFile(
    Seq<BcfTopic> Topics,
    Option<BcfProject> Project,
    Option<BcfVocabulary> Vocabulary,
    Seq<BcfDocument> Documents,
    BcfGeneration Source,
    HashMap<string, ReadOnlyMemory<byte>> Blobs = default) {

    public static BcfFile Of(Seq<BcfTopic> topics) => new(topics, None, None, Seq<BcfDocument>(), BcfGeneration.Bcf30);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
// The `.bcfzip` codec at both toolkit depths: Read sniffs the generation (BcfExtensions.
// GetVersionFromStreamArchive), streams a native 3.0 container per-part (ParseMarkups/
// ParseExtensions/ParseProject/ParseDocuments — no second full-graph materialization), and
// up-converts a 2.1 source through Worker.BcfFromStream (its `using BcfToolkit.Model.Bcf30;`
// binds the generic target); Write seeds BcfBuilder from the file's own vocabulary/project/
// documents and folds every topic column back through the nested builders. The async surface
// runs to completion at this codec boundary (the one language-owned GetAwaiter().GetResult()
// bridge), every throw lowered onto Fin<T> as a BARE BimFault.ModelRejected (band 2600 IS the
// Expected Code; the MapFail lambda closes over the Op key, so it is NOT static).
public static class BcfArchive {
    // The self-flattening Bind collapses Try's outer rail onto the decode's own Fin: the container faults (a
    // malformed zip, an unreadable version) arrive as thrown boundary state, the SEMANTIC faults (a viewpoint
    // declaring two cameras) arrive already typed, and one funnel lands both without the captured error being
    // discarded.
    public static Fin<BcfFile> Read(ReadOnlyMemory<byte> bcfzip, Op key) =>
        Try.lift<Fin<BcfFile>>(() => Decode(bcfzip, key)).Run()
            .MapFail(error => new BimFault.ModelRejected(key, $"bcf-archive:{error.Message}"))
            .Bind(static result => result);

    public static Fin<byte[]> Write(BcfFile file, Op key) =>
        Try.lift(() => Encode(file)).Run().MapFail(error => new BimFault.ModelRejected(key, $"bcf-write:{error.Message}"));

    // EXEMPTION: the toolkit's per-part parse surface is async and stream-owning, so the decode body is the named
    // platform-forced statement seam — the sequential `Fresh` streams, the version sniff, and the up-conversion
    // fork are statements by the foreign API's shape, and the whole body lands on ONE Fin.
    static Fin<BcfFile> Decode(ReadOnlyMemory<byte> bcfzip, Op key) {
        byte[] bytes = bcfzip.ToArray();
        BcfVersionEnum version = BcfExtensions.GetVersionFromStreamArchive(Fresh(bytes)).GetAwaiter().GetResult()
            ?? throw new InvalidDataException("bcf-version-unreadable");
        if (version is BcfVersionEnum.Bcf30) {
            ConcurrentBag<Bcf30.Markup> markups = BcfExtensions.ParseMarkups<Bcf30.Markup, Bcf30.VisualizationInfo>(Fresh(bytes)).GetAwaiter().GetResult();
            Bcf30.Extensions vocabulary = BcfExtensions.ParseExtensions<Bcf30.Extensions>(Fresh(bytes)).GetAwaiter().GetResult();
            Bcf30.ProjectInfo? project = BcfExtensions.ParseProject<Bcf30.ProjectInfo>(Fresh(bytes)).GetAwaiter().GetResult();
            Bcf30.DocumentInfo? documents = BcfExtensions.ParseDocuments<Bcf30.DocumentInfo>(Fresh(bytes)).GetAwaiter().GetResult();
            return FileOf(toSeq(markups), vocabulary, project, documents, BcfGeneration.Bcf30, key)
                .Map(native => native with { Blobs = BlobsOf(bytes, native.Topics) });
        }
        Bcf30.Bcf bcf = new Worker().BcfFromStream(Fresh(bytes)).GetAwaiter().GetResult();
        return FileOf(toSeq(bcf.Markups), bcf.Extensions, bcf.Project, bcf.Document, BcfGeneration.Bcf21, key)
            .Map(converted => converted with { Blobs = BlobsOf(bytes, converted.Topics) });
    }

    // Bitmap payloads are CONTAINER PARTS the toolkit model never carries (`Bcf30.Bitmap` declares no data
    // member and the writer emits none), so the codec lifts each held `{topicGuid}/{Reference}` part into
    // the file's blob store; a part the container does not hold reads as absent — an optional binary never
    // faults the archive read. The topic-folder path scheme holds at both source generations.
    static HashMap<string, ReadOnlyMemory<byte>> BlobsOf(byte[] bytes, Seq<BcfTopic> topics) {
        using ZipArchive zip = new(Fresh(bytes), ZipArchiveMode.Read);
        return topics.Fold(HashMap<string, ReadOnlyMemory<byte>>(), (acc, topic) =>
            topic.Viewpoints.Bind(static v => v.Bitmaps).Fold(acc, (map, bitmap) =>
                Optional(zip.GetEntry($"{topic.Guid}/{bitmap.Reference}"))
                    .Map(PartOf)
                    .Match(Some: data => map.AddOrUpdate(bitmap.Reference, data), None: () => map)));
    }

    static ReadOnlyMemory<byte> PartOf(ZipArchiveEntry entry) {
        using Stream source = entry.Open();
        using MemoryStream sink = new();
        source.CopyTo(sink);
        return sink.ToArray();
    }

    // Each per-part parse opens its own ZipArchive over an independent MemoryStream — a parse helper that
    // disposes its source never strands a sibling read.
    static MemoryStream Fresh(byte[] bytes) => new(bytes, writable: false);

    // ParseMarkups yields a ConcurrentBag, so the fold re-orders topics deterministically (the schema
    // Index, then Guid) — a re-read of the same archive is byte-stable, never bag-racy.
    static Fin<BcfFile> FileOf(Seq<Bcf30.Markup> markups, Bcf30.Extensions? vocabulary, Bcf30.ProjectInfo? project, Bcf30.DocumentInfo? documents, BcfGeneration source, Op key) =>
        markups.TraverseM(markup => TopicOf(markup, key)).As().Map(topics =>
        new BcfFile(toSeq(topics.OrderBy(static t => t.Index.IfNone(int.MaxValue)).ThenBy(static t => t.Guid, StringComparer.Ordinal)),
            Optional(project?.Project).Map(static p => new BcfProject(p.ProjectId ?? "", p.Name ?? "")),
            Optional(vocabulary).Map(static x => new BcfVocabulary(
                toSeq(x.TopicTypes), toSeq(x.TopicStatuses), toSeq(x.Priorities), toSeq(x.TopicLabels),
                toSeq(x.Users), toSeq(x.SnippetTypes), toSeq(x.Stages))),
            Optional(documents).Map(static d => toSeq(d.Documents).Map(static row => new BcfDocument(
                row.Guid, row.Filename ?? "", row.Description ?? "",
                Base64Of(row.DocumentData).IfNone(ReadOnlyMemory<byte>.Empty)))).IfNone(Seq<BcfDocument>()),
            source));

    // The comments and viewpoints ride `Topic`, not `Markup` (a `Bcf30.Markup` carries only `Header`+`Topic`),
    // so the projection reads `topic.Comments`/`topic.Viewpoints`; the header IFC-file refs ride `markup.Header`.
    static Fin<BcfTopic> TopicOf(Bcf30.Markup markup, Op key) {
        Bcf30.Topic topic = markup.Topic;
        return toSeq(topic.Viewpoints).TraverseM(reference => ViewpointOf(reference, key)).As().Map(viewpoints =>
        new BcfTopic(
            topic.Guid, topic.Title ?? "", StatusOf(topic.TopicStatus),
            topic.TopicType ?? "", topic.Priority ?? "", topic.CreationAuthor ?? "",
            At(topic.CreationDate),
            toSeq(topic.Comments).Map(CommentOf),
            viewpoints,
            topic.Description ?? "", topic.AssignedTo ?? "", topic.Stage ?? "",
            AtOf(topic.DueDate), toSeq(topic.Labels),
            Optional(topic.Index), AtOf(topic.ModifiedDate), topic.ModifiedAuthor ?? "",
            topic.ServerAssignedId ?? "",
            toSeq(topic.ReferenceLinks),
            toSeq(topic.RelatedTopics).Map(static r => r.Guid),
            toSeq(topic.DocumentReferences).Map(static d => new BcfDocumentReference(
                d.Guid, Optional(d.DocumentGuid).Filter(static g => g.Length > 0), Optional(d.Url).Filter(static u => u.Length > 0), d.Description ?? "")),
            Optional(topic.BimSnippet).Map(static s => new BcfBimSnippet(s.SnippetType ?? "", s.Reference ?? "", s.ReferenceSchema ?? "", s.IsExternal)),
            Optional(markup.Header).Map(static h => toSeq(h.Files).Map(static f => new BcfHeaderFile(
                f.Filename ?? "", AtOf(f.Date), f.Reference ?? "", f.IfcProject ?? "", f.IfcSpatialStructureElement ?? "", f.IsExternal))).IfNone(Seq<BcfHeaderFile>()),
            topic.TopicStatus ?? ""));
    }

    // The BCF `TopicStatus` is a free project-vocabulary string; the closed `BcfStatus` lifecycle token parses the
    // space/hyphen-stripped token case-insensitively ("In Progress" and "in-progress" -> InProgress), defaulting
    // `Open` on an absent, unrecognized, or out-of-range status so the wire discriminant never widens past the
    // five-state lifecycle — `Enum.TryParse` alone returns true for ANY integer-parseable token (a malformed "7"
    // would mint an undefined case that then crosses the wire as a numeric the TS string-switch cannot read), so
    // `Enum.IsDefined` gates it. The verbatim token rides BcfTopic.StatusLabel, so the parse loses nothing.
    static BcfStatus StatusOf(string? status) =>
        Enum.TryParse(status?.Replace(" ", "", StringComparison.Ordinal).Replace("-", "", StringComparison.Ordinal), ignoreCase: true, out BcfStatus parsed) && Enum.IsDefined(parsed) ? parsed : BcfStatus.Open;

    // BCF 3.0 dropped the reply join from the markup schema, so `Bcf30.Comment` declares no reply member and the
    // archive read lands typed absence; the REST lane's `reply_to_comment_guid` is the only carrier.
    static BcfComment CommentOf(Bcf30.Comment comment) =>
        new(comment.Guid, comment.Author ?? "", comment.CommentProperty ?? "",
            Optional(comment.Viewpoint?.Guid), At(comment.Date), AtOf(comment.ModifiedDate), comment.ModifiedAuthor ?? "", None);

    static Fin<BcfViewpoint> ViewpointOf(Bcf30.ViewPoint reference, Op key) {
        Bcf30.VisualizationInfo? visualization = reference.VisualizationInfo;
        Bcf30.Components? components = visualization?.Components;
        return CameraOf(visualization, reference.Guid, key).Map(camera =>
        new BcfViewpoint(
            reference.Guid, camera,
            GuidsOf(components?.Selection),
            GuidsOf(components?.Visibility?.Exceptions),
            Base64Of(reference.SnapshotData),
            components?.Visibility?.DefaultVisibility ?? false,
            Optional(components?.Coloring).Map(static rows => toSeq(rows).Map(static row =>
                new BcfColoring(row.Color ?? "", toSeq(row.Components).Map(static m => m.IfcGuid)))).IfNone(Seq<BcfColoring>()),
            Optional(visualization?.Lines).Map(static rows => toSeq(rows).Map(static l => new BcfLine(Vec(l.StartPoint), Vec(l.EndPoint)))).IfNone(Seq<BcfLine>()),
            Optional(visualization?.ClippingPlanes).Map(static rows => toSeq(rows).Map(static p => new BcfClippingPlane(Vec(p.Location), Vec(p.Direction)))).IfNone(Seq<BcfClippingPlane>()),
            Optional(visualization?.Bitmaps).Map(static rows => toSeq(rows).Map(static b =>
                new BcfBitmap(b.Format.ToString(), b.Reference ?? "", Vec(b.Location), Vec(b.Normal), Vec(b.Up), b.Height))).IfNone(Seq<BcfBitmap>()),
            Optional(reference.Index),
            Optional(components?.Visibility?.ViewSetupHints).Map(static h =>
                new BcfViewSetupHints(h.SpacesVisible, h.SpaceBoundariesVisible, h.OpeningsVisible))));
    }

    // The archive arm's half of the ONE camera admission: transcribe each declared toolkit camera onto its typed
    // case, then hand BOTH to BcfCamera.Admit, which owns the XOR law and the typed absence. No decision lives here.
    static Fin<Option<BcfCamera>> CameraOf(Bcf30.VisualizationInfo? visualization, string viewpoint, Op key) =>
        BcfCamera.Admit(
            Optional(visualization?.PerspectiveCamera).Map(static p => new BcfCamera.Perspective(
                Vec(p.CameraViewPoint), Vec(p.CameraDirection), Vec(p.CameraUpVector), p.FieldOfView, p.AspectRatio)),
            Optional(visualization?.OrthogonalCamera).Map(static o => new BcfCamera.Orthogonal(
                Vec(o.CameraViewPoint), Vec(o.CameraDirection), Vec(o.CameraUpVector), o.ViewToWorldScale, o.AspectRatio)),
            viewpoint, key);

    static Seq<string> GuidsOf(System.Collections.ObjectModel.Collection<Bcf30.Component>? set) =>
        Optional(set).Map(static s => toSeq(s).Map(static c => c.IfcGuid)).IfNone(Seq<string>());

    // `CameraViewPoint` is a `Bcf30.Point`, `CameraDirection`/`CameraUpVector` are `Bcf30.Direction` — distinct
    // toolkit types sharing the X/Y/Z shape, so each takes its own overload (the toolkit ships no shared base).
    static Vector3 Vec(Bcf30.Point? p) => p is null ? default : new((float)p.X, (float)p.Y, (float)p.Z);
    static Vector3 Vec(Bcf30.Direction? d) => d is null ? default : new((float)d.X, (float)d.Y, (float)d.Z);

    static Instant At(DateTime value) => Instant.FromDateTimeUtc(DateTime.SpecifyKind(value, DateTimeKind.Utc));
    static Option<Instant> AtOf(DateTime? value) => Optional(value).Map(At);

    // One base64 admission for snapshot and document payloads: a malformed/absent payload yields None (an
    // optional binary never faults the archive read), decoded non-throwing via `TryFromBase64String`.
    static Option<ReadOnlyMemory<byte>> Base64Of(FileData? payload) {
        if (payload?.Data is not { Length: > 0 } data) { return None; }
        byte[] buffer = new byte[data.Length];
        return Convert.TryFromBase64String(data, buffer, out int written)
            ? Some<ReadOnlyMemory<byte>>(buffer.AsMemory(0, written))
            : None;
    }

    // EXEMPTION: the toolkit's fluent builder is MUTATION-shaped — every Set*/Add* returns the same instance and
    // the nested authors are Action<TBuilder> void callbacks — so the whole author is the named platform-forced
    // statement seam, exactly as the decode boundary is. The conditional Set* guards are the builder's own
    // presence contract (an empty string writes an empty element), never domain branching; the expression form
    // does not exist at this surface, and every throw funnels through the one Fin at Write.
    static byte[] Encode(BcfFile file) {
        BcfBuilder seeded = file.Vocabulary.Match(
            Some: v => new BcfBuilder().SetExtensions(x => {
                v.TopicTypes.Iter(t => x.AddTopicType(t)); v.TopicStatuses.Iter(s => x.AddTopicStatus(s));
                v.Priorities.Iter(p => x.AddPriority(p)); v.TopicLabels.Iter(l => x.AddTopicLabel(l));
                v.Users.Iter(u => x.AddUser(u)); v.SnippetTypes.Iter(s => x.AddSnippetType(s)); v.Stages.Iter(s => x.AddStage(s));
            }),
            None: static () => new BcfBuilder().WithDefaults());
        file.Project.IfSome(p => seeded.SetProject(project => project.SetProjectId(p.ProjectId).SetProjectName(p.Name)));
        if (!file.Documents.IsEmpty) {
            Bcf30.DocumentInfo library = new();
            file.Documents.Iter(d => library.Documents.Add(new Bcf30.Document {
                Guid = d.Guid, Filename = d.Filename, Description = d.Description,
                DocumentData = new FileData { Data = Convert.ToBase64String(d.Data.Span) },
            }));
            seeded.SetDocument(library);
        }
        BcfBuilder builder = file.Topics.Fold(seeded, static (acc, topic) => acc.AddMarkup(markup => {
            markup.SetGuid(topic.Guid).SetTitle(topic.Title).SetTopicType(topic.TopicType)
                .SetTopicStatus(topic.StatusToken).SetPriority(topic.Priority).SetCreationAuthor(topic.Author)
                .SetCreationDate(topic.CreationDate.ToDateTimeUtc())
                .SetDescription(topic.Description).SetAssignedTo(topic.AssignedTo).SetStage(topic.Stage)
                .SetDueDate(topic.DueDate.Match<DateTime?>(static d => d.ToDateTimeUtc(), static () => null))
                .SetModifiedDate(topic.ModifiedDate.Match<DateTime?>(static d => d.ToDateTimeUtc(), static () => null));
            topic.Index.IfSome(i => markup.SetIndex(i));
            if (topic.ModifiedAuthor.Length > 0) { markup.SetModifiedAuthor(topic.ModifiedAuthor); }
            if (topic.ServerAssignedId.Length > 0) { markup.SetServerAssignedId(topic.ServerAssignedId); }
            topic.Labels.Iter(l => markup.AddLabel(l));
            topic.ReferenceLinks.Iter(l => markup.AddReferenceLink(l));
            topic.RelatedTopics.Iter(g => markup.AddRelatedTopic(g));
            topic.DocumentReferences.Iter(d => markup.AddDocumentReference(reference => {
                reference.SetGuid(d.Guid).SetDescription(d.Description);
                d.DocumentGuid.IfSome(g => reference.SetDocumentGuid(g));
                d.Url.IfSome(u => reference.SetUrl(u));
            }));
            topic.BimSnippet.IfSome(s => markup.SetBimSnippet(snippet =>
                snippet.SetSnippetType(s.SnippetType).SetReference(s.Reference).SetReferenceSchema(s.ReferenceSchema).SetIsExternal(s.IsExternal)));
            topic.Files.Iter(f => markup.AddHeaderFile(header => {
                header.SetFileName(f.Filename).SetReference(f.Reference).SetIsExternal(f.IsExternal)
                    .SetIfcProject(f.IfcProject).SetIfcSpatialStructureElement(f.IfcSpatialStructureElement);
                f.Date.IfSome(d => header.SetDate(d.ToDateTimeUtc()));
            }));
            topic.Comments.Iter(c => markup.AddComment(comment => {
                comment.SetGuid(c.Guid).SetAuthor(c.Author).SetComment(c.Text).SetDate(c.Date.ToDateTimeUtc())
                    .SetViewPointGuid(c.ViewpointGuid.Match<string?>(static g => g, static () => null))
                    .SetModifiedDate(c.ModifiedDate.Match<DateTime?>(static d => d.ToDateTimeUtc(), static () => null));
                if (c.ModifiedAuthor.Length > 0) { comment.SetModifiedAuthor(c.ModifiedAuthor); }
            }));
            topic.Viewpoints.Iter(v => markup.AddViewPoint(vp => AuthorViewpoint(vp, v)));
        }));
        using Stream stream = new Worker().ToBcf(builder.Build(), BcfVersionEnum.Bcf30).GetAwaiter().GetResult();
        using MemoryStream sink = new();
        stream.CopyTo(sink);
        return WithBitmapParts(sink, file);
    }

    // Write's half of the container-part gap: every `BcfBitmap.Reference` must leave as a HELD part, so
    // the codec appends each topic's `{topicGuid}/{Reference}` entry from the file's blob store and
    // REFUSES a reference the store cannot answer (the throw lowers through the one Fin funnel) — a
    // written archive whose markup references a part the container never holds is unrepresentable.
    static byte[] WithBitmapParts(MemoryStream container, BcfFile file) {
        using (ZipArchive zip = new(container, ZipArchiveMode.Update, leaveOpen: true)) {
            file.Topics.Iter(topic => topic.Viewpoints.Bind(static v => v.Bitmaps).Map(static b => b.Reference).Distinct().Iter(reference => {
                ReadOnlyMemory<byte> data = file.Blobs.Find(reference)
                    .IfNone(() => throw new InvalidDataException($"bcf-bitmap-unresolved:{reference}"));
                using Stream part = zip.CreateEntry($"{topic.Guid}/{reference}").Open();
                part.Write(data.Span);
            }));
        }
        return container.ToArray();
    }

    // The viewpoint author re-emits the FULL VisualizationInfo — the typed camera case, selection, visibility
    // (default + exceptions), coloring, redlines, clipping planes, and bitmaps — so a write round-trips exactly
    // what the read captured; a Guid-only author is the hollow form this fold retires.
    static void AuthorViewpoint(ViewPointBuilder vp, BcfViewpoint viewpoint) {
        vp.SetGuid(viewpoint.Guid);
        viewpoint.Index.IfSome(i => vp.SetIndex(i));
        viewpoint.Snapshot.IfSome(bytes => vp.SetSnapshotData(new FileData { Mime = "image/png", Data = Convert.ToBase64String(bytes.Span) }));
        vp.SetVisualizationInfo(info => {
            // A selection-only viewpoint writes NO camera element: the domain absence is the archive absence, so
            // the round trip is honest in both directions and no degenerate origin camera is authored.
            viewpoint.Camera.IfSome(lens => lens.Switch(
                perspective: p => info.SetPerspectiveCamera(camera => camera
                    .SetCameraViewPoint(p.Position.X, p.Position.Y, p.Position.Z)
                    .SetCameraDirection(p.Direction.X, p.Direction.Y, p.Direction.Z)
                    .SetCameraUpVector(p.Up.X, p.Up.Y, p.Up.Z)
                    .SetFieldOfView(p.FieldOfViewDeg).SetAspectRatio(p.AspectRatio)),
                orthogonal: o => info.SetOrthogonalCamera(camera => camera
                    .SetCameraViewPoint(o.Position.X, o.Position.Y, o.Position.Z)
                    .SetCameraDirection(o.Direction.X, o.Direction.Y, o.Direction.Z)
                    .SetCameraUpVector(o.Up.X, o.Up.Y, o.Up.Z)
                    .SetViewToWorldScale(o.ViewToWorldScale).SetAspectRatio(o.AspectRatio))));
            viewpoint.SelectedGlobalIds.Iter(id => info.AddSelection(component => component.SetIfcGuid(id)));
            if (viewpoint.DefaultVisibility || !viewpoint.VisibilityExceptions.IsEmpty || viewpoint.ViewSetupHints.IsSome) {
                info.SetVisibility(visibility => {
                    visibility.SetDefaultVisibility(viewpoint.DefaultVisibility);
                    viewpoint.ViewSetupHints.IfSome(h => visibility.SetViewSetupHints(hints => hints
                        .SetSpaceVisible(h.SpacesVisible).SetSpaceBoundariesVisible(h.SpaceBoundariesVisible).SetOpeningVisible(h.OpeningsVisible)));
                    viewpoint.VisibilityExceptions.Iter(id => visibility.AddException(component => component.SetIfcGuid(id)));
                });
            }
            viewpoint.Coloring.Iter(c => info.AddColoring(color => {
                color.SetColor(c.Color);
                c.GlobalIds.Iter(id => color.AddComponent(component => component.SetIfcGuid(id)));
            }));
            viewpoint.Lines.Iter(l => info.AddLine(line => line
                .SetStartPoint(l.Start.X, l.Start.Y, l.Start.Z).SetEndPoint(l.End.X, l.End.Y, l.End.Z)));
            viewpoint.ClippingPlanes.Iter(p => info.AddClippingPlane(plane => plane
                .SetLocation(p.Location.X, p.Location.Y, p.Location.Z).SetDirection(p.Direction.X, p.Direction.Y, p.Direction.Z)));
            viewpoint.Bitmaps.Iter(b => info.AddBitmap(bitmap => bitmap
                .SetFormat(b.Format).SetReference(b.Reference)
                .SetLocation(b.Location.X, b.Location.Y, b.Location.Z)
                .SetNormal(b.Normal.X, b.Normal.Y, b.Normal.Z)
                .SetUp(b.Up.X, b.Up.Y, b.Up.Z).SetHeight(b.Height)));
        });
    }
}
```

## [03]-[TS_PROJECTION]

- Owner: `BcfWire` the host-free JSON wire producer of the `[02]-[BCF_ARCHIVE]` topic family — `BcfWire.Topics` the projected `ImmutableArray<BcfTopicWire>` payload the `ts:ui/bcf-anchor` panel decodes; the wire mirrors exist ONLY where a carrier forces them (the domain records carry LanguageExt `Seq`/`Option` and the `BcfCamera` union STJ cannot round-trip without converters the canonical context omits, so `BcfTopicWire`/`BcfCommentWire`/`BcfViewpointWire`/`BcfDocumentReferenceWire`/`BcfHeaderFileWire`/`BcfColoringWire`/`BcfCameraWire` narrow to `ImmutableArray`/nullable/kind-discriminated flat and DROP the heavy `Snapshot` bytes — the `BcfApi` `ReadSnapshot` resource serves the preview instead), while the BCL-pure domain records (`BcfLine`/`BcfClippingPlane`/`BcfBitmap`/`BcfBimSnippet`/`BcfProject`) serialize DIRECTLY — a mirror without a carrier reason is the deleted form; `BcfWireMapper` the `[Mapper]` static partial class GENERATING the domain-to-wire field transcription (the hand-written `Project`/`ProjectComment`/`ProjectViewpoint` transcriptions retired), its `[UserMapping]` carrier codecs owning the `Option`-to-nullable and `BcfCamera`-to-`BcfCameraWire` crossings and the native enumerable conversion owning `Seq`-to-`ImmutableArray`; `BcfWireContext` the SELF-OWNED source-generated `JsonSerializerContext` and `BcfApiContext` its snake_case spec-dialect sibling (the buildingSMART field spellings at the foreign seam — two naming registers over ONE issue vocabulary); `BcfApi` the BCF-API 3.0 resource projection whose write verbs carry the spec-dialect `BcfApi*Body` register, never the TS payload shapes — `Project` the request half, `Open` the response half landing status + raw bytes on the typed `BcfOutcome` family through the SAME register and the `BcfWireMapper.ToDomain` inverses, `BcfQuery` the OData collection axis whose `Skip` cursor is the lane's paging state (the spec answers bare arrays, so no response wrapper fabricates an envelope).
- Entry: `BcfWire.Encode(Seq<BcfTopic> topics, Op key)` projects the topic set onto the wire payload and `BcfWire.Decode(ReadOnlyMemory<byte> json, Op key)` admits it back (a `null` root thrown into the same funnel, never a `Succ(null!)`) — `Fin<T>` aborts on a malformed payload (`BimFault.ModelRejected` BARE, band 2600, no `.ToError()` hop) through the identical `Try.lift(...).Run().MapFail(...)` funnel the codec carries; `BcfWire.Json<T>(T body, string resource, Op key)` is the ONE spec-dialect body serializer every `BcfApi.Project` write arm feeds (snake_case through `BcfApiContext.Json`, server-assigned columns dropped by law, the fault detail naming the resource route) — the retired per-entity `TopicJson`/`CommentJson`/`ViewpointJson`/`FilesJson`/`RelatedTopicsJson`/`DocumentReferenceJson` sibling family differed only by payload type, and the retired `Anchor` hop merely renamed `BcfViewpoint.SelectedGlobalIds` — the UI live-binding reads the property itself, so a TS pick round-trips the exact seam `Node.Object.ExternalId` IFC-GlobalId selection the C# viewpoint carries.
- Auto: `Encode` serializes through `BcfWireContext.Json` — the `BcfStatus` enum by its `[JsonStringEnumMemberName]` lower-kebab name under `UseStringEnumConverter`, the NodaTime `Instant`/`Instant?` columns through `ConfigureForNodaTime` as ISO-8601 text, the BCL `System.Numerics.Vector3` camera/geometry triplets as camel-case `{x,y,z}` objects (the struct's `X`/`Y`/`Z` are FIELDS, so `IncludeFields` admits them), and the `BcfCameraWire.Kind` string (`"perspective"`/`"orthogonal"`) as the TS camera discriminant; the mapper transcribes every topic column — references, provenance, snippet, header files, coloring, redlines, clipping planes, bitmaps, view-setup hints — so the wire carries the full archive surface minus the binary payloads; `Decode` re-admits through the same options so a malformed `Instant` or a half-built record faults at admission rather than minting a partial topic.
- Receipt: the `BcfWire.Topics` payload is the one BCF cross-runtime contract — the `ts:ui/bcf-anchor` panel and live-binding decode the same vocabulary the C# branch mints; a viewpoint anchors on the seam `Node.Object.ExternalId` IFC GlobalId so the TS selection highlight and the C# component selection carry one element identity, and the file, REST, and web forms project ONE domain vocabulary — the `.bcfzip` codec, the `BcfApiContext` spec-dialect bodies, and this `BcfTopicWire` payload are three registers of the same `BcfTopic` family, never three topic models.
- Packages: Riok.Mapperly, Thinktecture.Runtime.Extensions, NodaTime, NodaTime.Serialization.SystemTextJson, LanguageExt.Core, Rasm, BCL `System.Text.Json` + `System.Collections.Immutable`
- Growth: a new BCF entity on the wire is one `[JsonSerializable]` row and one mapper method; a new topic column is one wire-record column the generated mapper transcribes with zero hand code; a new BCF-API resource operation is one `BcfResource` case the generated total `Switch` breaks at BOTH dispatch halves at compile time — the `Project` request arm and the `Open` response arm land together (never a verb knob, never a `Get`/`Post` member beside `Project`), its write body one `BcfWire.Json` feed and its read body one register root; a new OData axis is one `BcfQuery` column the one `Render` mint serializes; never a serializer beside the two dialect registers (`BcfWireContext.Json` the camelCase TS payload, `BcfApiContext.Json` the snake_case spec bodies), never a per-entity `*Json` sibling beside the one generic funnel, and never a hand-written transcription beside the mapper.
- Boundary: `BcfWire` is HOST-FREE — no RhinoCommon type, no `ZipArchive`/`XDocument` codec surface, only the host-free record graph and the BCL `System.Numerics.Vector3` (the kernel 3-vector owner publishes the host-bound RhinoCommon `Vector3d` alone, so composing it here drags the host into the wire); it rides the SELF-OWNED `BcfWireContext` (the retired generic `BimWireContext` was a model STJ serializer inside an AEC package — a strata leak; the seam-graph STJ wire is `Rasm.Persistence/Element/codec#CODEC_AXIS` `SnapshotCodec`'s); the domain-to-wire transcription is GENERATED (`Riok.Mapperly` `[Mapper]` + `[UserMapping]` carriers — the hand-written per-field projector is the retired form the boundary-transcription law deletes); a wire mirror exists only where `Seq`/`Option`/union carriers force it and a BCL-pure domain record serializes directly; the `BcfStatus` discriminant is the `[JsonStringEnumMemberName]` string and an ordinal-keyed enum crossing the wire is the named seam violation; the REST write bodies are the `BcfApiContext` snake_case spec dialect (`topic_status`/`camera_view_point`/`related_topic_guid`, null slots omitted so the camera XOR discriminates by absence) — a camelCase workspace body POSTed at a conformant CDE is the deleted illusory form; the `.bcfzip` archive, the `BcfApi` REST bodies, and this wire payload project onto one topic vocabulary minted once in C# — a TS-side parallel topic shape, or a third dialect register, is the named drift defect.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using LanguageExt;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using Riok.Mapperly.Abstractions;
using Op = Rasm.Domain.Op;
using Vector3 = System.Numerics.Vector3;
using static LanguageExt.Prelude;

namespace Rasm.Bim.Coordination;

// --- [TYPES] ------------------------------------------------------------------------------
// The five-state BCF lifecycle wire discriminant: a plain enum (the `Review/coordination#SIGN_OFF` SmartEnum owns
// the transition machine OVER it) whose `[JsonStringEnumMemberName]` lower-kebab names cross the wire so a TS decode
// switches on a stable string, never the ordinal. One enum, never widened past the five states.
public enum BcfStatus : byte {
    [JsonStringEnumMemberName("open")]        Open = 0,
    [JsonStringEnumMemberName("in-progress")] InProgress = 1,
    [JsonStringEnumMemberName("resolved")]    Resolved = 2,
    [JsonStringEnumMemberName("closed")]      Closed = 3,
    [JsonStringEnumMemberName("reopened")]    Reopened = 4,
}

public enum BcfApiVerb : byte { Get = 0, Post = 1, Put = 2, Delete = 3 }

// --- [MODELS] -----------------------------------------------------------------------------
// Wire mirrors exist ONLY where a carrier forces them: Seq -> ImmutableArray, Option -> nullable,
// the BcfCamera union -> the Kind-discriminated flat record (unused scalar 0). BCL-pure domain
// records (BcfLine/BcfClippingPlane/BcfBitmap/BcfBimSnippet) serialize directly — no mirror.
public sealed record BcfCameraWire(
    string Kind, Vector3 Position, Vector3 Direction, Vector3 Up,
    double FieldOfView, double ViewToWorldScale, double AspectRatio);

public sealed record BcfColoringWire(string Color, ImmutableArray<string> GlobalIds);
public sealed record BcfDocumentReferenceWire(string Guid, string? DocumentGuid, string? Url, string Description);
public sealed record BcfHeaderFileWire(string Filename, Instant? Date, string Reference, string IfcProject, string IfcSpatialStructureElement, bool IsExternal);

public sealed record BcfViewpointWire(
    string Guid,
    BcfCameraWire? Camera,
    ImmutableArray<string> SelectedGlobalIds,
    ImmutableArray<string> VisibilityExceptions,
    bool DefaultVisibility,
    ImmutableArray<BcfColoringWire> Coloring,
    ImmutableArray<BcfLine> Lines,
    ImmutableArray<BcfClippingPlane> ClippingPlanes,
    ImmutableArray<BcfBitmap> Bitmaps,
    int? Index,
    BcfViewSetupHints? ViewSetupHints);

public sealed record BcfCommentWire(
    string Guid,
    string Author,
    string Text,
    string? ViewpointGuid,
    Instant Date,
    Instant? ModifiedDate,
    string ModifiedAuthor,
    string? ReplyToGuid);

public sealed record BcfTopicWire(
    string Guid,
    string Title,
    BcfStatus Status,
    string TopicType,
    string Priority,
    string Author,
    Instant CreationDate,
    ImmutableArray<BcfCommentWire> Comments,
    ImmutableArray<BcfViewpointWire> Viewpoints,
    string Description,
    string AssignedTo,
    string Stage,
    Instant? DueDate,
    ImmutableArray<string> Labels,
    int? Index,
    Instant? ModifiedDate,
    string ModifiedAuthor,
    string ServerAssignedId,
    ImmutableArray<string> ReferenceLinks,
    ImmutableArray<string> RelatedTopics,
    ImmutableArray<BcfDocumentReferenceWire> DocumentReferences,
    BcfBimSnippet? BimSnippet,
    ImmutableArray<BcfHeaderFileWire> Files,
    string StatusLabel);

// --- [BODIES]
// The BCF-API 3.0 resource-body register — ONE snake_case dialect serving BOTH directions: the WRITE bodies
// (topic_POST/comment_POST/viewpoint_POST/related_topic_PUT/document_reference_POST/file_PUT) and the GET READ
// shapes, which per spec are the SAME resources plus the server-assigned provenance columns (creation/modification
// author+date, server_assigned_id, the comment's topic/reply joins) — carried NULLABLE below so the write
// serializer's WhenWritingNull omission keeps every POST/PUT body spec-conformant while a GET response decodes
// whole. The camera is orthogonal_camera XOR perspective_camera and the snapshot rides inline
// {snapshot_type, snapshot_data} base64. A body is the SECOND dialect of the one topic family — spec field
// spellings at the foreign seam per SEMANTIC_NAMING, never a second issue vocabulary.
public sealed record BcfApiPointBody(double X, double Y, double Z);
public sealed record BcfApiOrthogonalCameraBody(BcfApiPointBody CameraViewPoint, BcfApiPointBody CameraDirection, BcfApiPointBody CameraUpVector, double ViewToWorldScale, double AspectRatio);
public sealed record BcfApiPerspectiveCameraBody(BcfApiPointBody CameraViewPoint, BcfApiPointBody CameraDirection, BcfApiPointBody CameraUpVector, double FieldOfView, double AspectRatio);
public sealed record BcfApiLineBody(BcfApiPointBody StartPoint, BcfApiPointBody EndPoint);
public sealed record BcfApiClippingPlaneBody(BcfApiPointBody Location, BcfApiPointBody Direction);
public sealed record BcfApiSnapshotBody(string SnapshotType, string SnapshotData);
public sealed record BcfApiComponentBody(string IfcGuid);
public sealed record BcfApiColoringBody(string Color, ImmutableArray<BcfApiComponentBody> Components);
public sealed record BcfApiViewSetupHintsBody(bool SpacesVisible, bool SpaceBoundariesVisible, bool OpeningsVisible);
public sealed record BcfApiVisibilityBody(bool DefaultVisibility, BcfApiViewSetupHintsBody? ViewSetupHints, ImmutableArray<BcfApiComponentBody> Exceptions);
public sealed record BcfApiComponentsBody(ImmutableArray<BcfApiComponentBody> Selection, ImmutableArray<BcfApiColoringBody> Coloring, BcfApiVisibilityBody Visibility);
public sealed record BcfApiViewpointBody(
    string Guid, int? Index,
    BcfApiOrthogonalCameraBody? OrthogonalCamera, BcfApiPerspectiveCameraBody? PerspectiveCamera,
    ImmutableArray<BcfApiLineBody> Lines, ImmutableArray<BcfApiClippingPlaneBody> ClippingPlanes,
    BcfApiSnapshotBody? Snapshot, BcfApiComponentsBody? Components);
public sealed record BcfApiSnippetBody(string SnippetType, bool IsExternal, string Reference, string ReferenceSchema);
public sealed record BcfApiTopicBody(
    string Guid, string TopicType, string TopicStatus, ImmutableArray<string> ReferenceLinks,
    string Title, string Priority, int? Index, ImmutableArray<string> Labels,
    string AssignedTo, string Stage, string Description, BcfApiSnippetBody? BimSnippet, Instant? DueDate,
    // Server-assigned READ columns — null on every write (omitted by WhenWritingNull), filled by a GET response.
    string? ServerAssignedId = null, string? CreationAuthor = null, Instant? CreationDate = null,
    string? ModifiedAuthor = null, Instant? ModifiedDate = null);
public sealed record BcfApiCommentBody(
    string Guid, string Comment, string? ViewpointGuid,
    // Server-assigned READ columns — the comment's provenance plus its topic/reply joins.
    string? TopicGuid = null, string? ReplyToCommentGuid = null, string? Author = null, Instant? Date = null,
    string? ModifiedAuthor = null, Instant? ModifiedDate = null);
public sealed record BcfApiRelatedTopicBody(string RelatedTopicGuid);
public sealed record BcfApiDocumentReferenceBody(string Guid, string? DocumentGuid, string? Url, string Description);
public sealed record BcfApiFileBody(string IfcProject, string IfcSpatialStructureElement, string Filename, Instant? Date, string Reference);
// The three READ-ONLY response bodies no write arm feeds: the document register row (its bytes ride the single
// document GET as a raw stream, never a JSON column), the project header, and the extensions vocabulary the
// domain BcfVocabulary transcribes 1:1.
public sealed record BcfApiDocumentBody(string Guid, string Filename, string? Description = null);
public sealed record BcfApiProjectBody(string ProjectId, string Name);
public sealed record BcfApiExtensionsBody(
    ImmutableArray<string> TopicType, ImmutableArray<string> TopicStatus, ImmutableArray<string> Priority,
    ImmutableArray<string> TopicLabel, ImmutableArray<string> UserIdType, ImmutableArray<string> SnippetType,
    ImmutableArray<string> Stage);

// --- [OPERATIONS] ---------------------------------------------------------------------------
// The GENERATED domain-to-wire transcription: Mapperly emits every per-field assignment from the
// partial signatures (the hand-written Project/ProjectComment/ProjectViewpoint transcriptions are
// the retired form); the [UserMapping] carriers own the Option-to-nullable and camera-union
// crossings, the native enumerable conversion owns Seq (IEnumerable) -> ImmutableArray with
// recursive element mapping, and [MapperIgnoreSource] pins the deliberate Snapshot drop so the
// unmapped-member diagnostic stays loud for every OTHER column — a silently dropped field is a
// build warning, never a hollow round-trip claim.
[Mapper]
public static partial class BcfWireMapper {
    [MapperIgnoreSource(nameof(BcfViewpoint.Snapshot))]
    public static partial BcfViewpointWire ToWire(BcfViewpoint viewpoint);
    [MapperIgnoreSource(nameof(BcfTopic.StatusToken))]
    public static partial BcfTopicWire ToWire(BcfTopic topic);
    public static partial BcfCommentWire ToWire(BcfComment comment);
    public static partial BcfHeaderFileWire ToWire(BcfHeaderFile file);
    public static partial BcfDocumentReferenceWire ToWire(BcfDocumentReference reference);

    [UserMapping]
    static BcfCameraWire? Lens(Option<BcfCamera> camera) => camera.Match<BcfCameraWire?>(Lens, static () => null);

    static BcfCameraWire Lens(BcfCamera camera) => camera.Switch(
        perspective: static p => new BcfCameraWire("perspective", p.Position, p.Direction, p.Up, p.FieldOfViewDeg, 0d, p.AspectRatio),
        orthogonal:  static o => new BcfCameraWire("orthogonal", o.Position, o.Direction, o.Up, 0d, o.ViewToWorldScale, o.AspectRatio));

    [UserMapping] static string? Text(Option<string> value) => value.Match<string?>(static v => v, static () => null);
    [UserMapping] static Instant? Moment(Option<Instant> value) => value.Match<Instant?>(static i => i, static () => null);
    [UserMapping] static int? Ordinal(Option<int> value) => value.Match<int?>(static i => i, static () => null);
    [UserMapping] static BcfBimSnippet? Snippet(Option<BcfBimSnippet> value) => value.Match<BcfBimSnippet?>(static s => s, static () => null);
    [UserMapping] static BcfViewSetupHints? Hints(Option<BcfViewSetupHints> value) => value.Match<BcfViewSetupHints?>(static h => h, static () => null);

    // The BCF-API write-body direction: Target-required only, because the spec bodies DROP the server-assigned
    // provenance and sub-resource collections BY LAW — a new body column still diagnoses loudly while the
    // deliberate drops stay quiet; TopicStatus reads the one StatusToken election.
    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(BcfTopic.StatusToken), nameof(BcfApiTopicBody.TopicStatus))]
    public static partial BcfApiTopicBody ToBody(BcfTopic topic);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(BcfComment.Text), nameof(BcfApiCommentBody.Comment))]
    [MapProperty(nameof(BcfComment.ReplyToGuid), nameof(BcfApiCommentBody.ReplyToCommentGuid))]
    public static partial BcfApiCommentBody ToBody(BcfComment comment);

    // The READ-lane wire-to-domain inverse over the SAME register — BcfApi.Admit's admitted body lands the typed
    // domain record here. The comment map GENERATES (its columns are 1:1 through the nullable carriers; the
    // topic/reply joins are collection-navigation facts the caller resolves, ignored loudly); the topic fold is
    // HAND-COMPOSED on the ToBody(BcfViewpoint) precedent because a GET topic carries no sub-resource collections
    // (Comments/Viewpoints land from their OWN reads) and its status re-elects the archive's space/hyphen-stripped
    // five-state normalization with the verbatim token preserved on StatusLabel.
    public static BcfTopic ToDomain(BcfApiTopicBody body) => new(
        body.Guid, body.Title, Election(body.TopicStatus), body.TopicType, body.Priority,
        body.CreationAuthor ?? "", body.CreationDate ?? Instant.MinValue,
        Seq<BcfComment>(), Seq<BcfViewpoint>(),
        body.Description, body.AssignedTo, body.Stage, OptionMoment(body.DueDate),
        toSeq(body.Labels), OptionOrdinal(body.Index), OptionMoment(body.ModifiedDate), body.ModifiedAuthor ?? "",
        body.ServerAssignedId ?? "", toSeq(body.ReferenceLinks),
        BimSnippet: Optional(body.BimSnippet).Map(static s => new BcfBimSnippet(s.SnippetType, s.Reference, s.ReferenceSchema, s.IsExternal)),
        StatusLabel: body.TopicStatus);

    // TopicGuid is a collection-navigation fact the caller already holds (it addressed the resource), so it stays
    // ignored; ReplyToCommentGuid is real conversation structure and maps BOTH ways onto BcfComment.ReplyToGuid.
    [MapperIgnoreSource(nameof(BcfApiCommentBody.TopicGuid))]
    [MapProperty(nameof(BcfApiCommentBody.Comment), nameof(BcfComment.Text))]
    [MapProperty(nameof(BcfApiCommentBody.ReplyToCommentGuid), nameof(BcfComment.ReplyToGuid))]
    public static partial BcfComment ToDomain(BcfApiCommentBody body);

    static BcfStatus Election(string status) =>
        Enum.TryParse(status.Replace(" ", "", StringComparison.Ordinal).Replace("-", "", StringComparison.Ordinal), ignoreCase: true, out BcfStatus parsed) && Enum.IsDefined(parsed) ? parsed : BcfStatus.Open;

    [UserMapping] static string Required(string? value) => value ?? "";
    [UserMapping] static Instant Stamped(Instant? value) => value ?? Instant.MinValue;
    [UserMapping] static Option<string> OptionText(string? value) => Optional(value).Filter(static v => v.Length > 0);
    [UserMapping] static Option<Instant> OptionMoment(Instant? value) => Optional(value);
    [UserMapping] static Option<int> OptionOrdinal(int? value) => Optional(value);

    public static partial BcfApiSnippetBody ToBody(BcfBimSnippet snippet);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public static partial BcfApiFileBody ToBody(BcfHeaderFile file);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public static partial BcfApiDocumentReferenceBody ToBody(BcfDocumentReference reference);

    // The viewpoint POST body: the camera union splits onto the spec's orthogonal_camera XOR perspective_camera
    // slots, the snapshot rides inline base64 (png — the archive mint's mime), and selection/coloring/visibility
    // nest under components. Bitmaps stay archive-only: the domain carries a Reference, the REST form demands
    // inline bitmap bytes this owner does not hold.
    public static BcfApiViewpointBody ToBody(BcfViewpoint viewpoint) => new(
        viewpoint.Guid,
        Ordinal(viewpoint.Index),
        viewpoint.Camera.Match<BcfApiOrthogonalCameraBody?>(
            Some: static lens => lens is BcfCamera.Orthogonal o
                ? new BcfApiOrthogonalCameraBody(Point(o.Position), Point(o.Direction), Point(o.Up), o.ViewToWorldScale, o.AspectRatio) : null,
            None: static () => null),
        viewpoint.Camera.Match<BcfApiPerspectiveCameraBody?>(
            Some: static lens => lens is BcfCamera.Perspective p
                ? new BcfApiPerspectiveCameraBody(Point(p.Position), Point(p.Direction), Point(p.Up), p.FieldOfViewDeg, p.AspectRatio) : null,
            None: static () => null),
        [.. viewpoint.Lines.Map(static l => new BcfApiLineBody(Point(l.Start), Point(l.End)))],
        [.. viewpoint.ClippingPlanes.Map(static c => new BcfApiClippingPlaneBody(Point(c.Location), Point(c.Direction)))],
        viewpoint.Snapshot.Match<BcfApiSnapshotBody?>(static bytes => new BcfApiSnapshotBody("png", Convert.ToBase64String(bytes.Span)), static () => null),
        new BcfApiComponentsBody(
            [.. viewpoint.SelectedGlobalIds.Map(static id => new BcfApiComponentBody(id))],
            [.. viewpoint.Coloring.Map(static c => new BcfApiColoringBody(c.Color, [.. c.GlobalIds.Map(static id => new BcfApiComponentBody(id))]))],
            new BcfApiVisibilityBody(
                viewpoint.DefaultVisibility,
                viewpoint.ViewSetupHints.Match<BcfApiViewSetupHintsBody?>(
                    static h => new BcfApiViewSetupHintsBody(h.SpacesVisible, h.SpaceBoundariesVisible, h.OpeningsVisible),
                    static () => null),
                [.. viewpoint.VisibilityExceptions.Map(static id => new BcfApiComponentBody(id))])));

    [UserMapping] static BcfApiSnippetBody? SnippetBody(Option<BcfBimSnippet> value) =>
        value.Match<BcfApiSnippetBody?>(static s => ToBody(s), static () => null);

    static BcfApiPointBody Point(Vector3 v) => new(v.X, v.Y, v.Z);

    // The viewpoint READ inverse — HAND-COMPOSED like its ToBody twin because the camera is a spec XOR the union
    // re-discriminates: exactly one camera slot admits, zero or two is a malformed body faulting typed rather
    // than a guessed projection. Components unpack onto the flat domain columns (an absent components object is
    // the spec's everything-visible default), the inline base64 snapshot decodes to bytes, and bitmaps stay
    // archive-only exactly as the write side declares.
    public static Fin<BcfViewpoint> ToDomain(BcfApiViewpointBody body, Op key) =>
        BcfCamera.Admit(
            Optional(body.PerspectiveCamera).Map(static p => new BcfCamera.Perspective(
                Domain(p.CameraViewPoint), Domain(p.CameraDirection), Domain(p.CameraUpVector), p.FieldOfView, p.AspectRatio)),
            Optional(body.OrthogonalCamera).Map(static o => new BcfCamera.Orthogonal(
                Domain(o.CameraViewPoint), Domain(o.CameraDirection), Domain(o.CameraUpVector), o.ViewToWorldScale, o.AspectRatio)),
            body.Guid, key)
        .Map(camera => {
            BcfApiComponentsBody parts = body.Components
                ?? new BcfApiComponentsBody([], [], new BcfApiVisibilityBody(true, null, []));
            return new BcfViewpoint(
                body.Guid,
                camera,
                toSeq(parts.Selection).Map(static component => component.IfcGuid),
                toSeq(parts.Visibility.Exceptions).Map(static component => component.IfcGuid),
                Optional(body.Snapshot).Map(static snapshot => (ReadOnlyMemory<byte>)Convert.FromBase64String(snapshot.SnapshotData)),
                parts.Visibility.DefaultVisibility,
                toSeq(parts.Coloring).Map(static row => new BcfColoring(row.Color, toSeq(row.Components).Map(static m => m.IfcGuid))),
                toSeq(body.Lines).Map(static line => new BcfLine(Domain(line.StartPoint), Domain(line.EndPoint))),
                toSeq(body.ClippingPlanes).Map(static plane => new BcfClippingPlane(Domain(plane.Location), Domain(plane.Direction))),
                Seq<BcfBitmap>(),
                OptionOrdinal(body.Index),
                Optional(parts.Visibility.ViewSetupHints).Map(static h => new BcfViewSetupHints(h.SpacesVisible, h.SpaceBoundariesVisible, h.OpeningsVisible)));
        });

    // The three mechanical READ inverses beside the viewpoint pair — nullable-to-Option and array-to-Seq lifts
    // over 1:1 columns, spelled once here so no response arm hand-transcribes.
    public static BcfDocumentReference ToDomain(BcfApiDocumentReferenceBody body) =>
        new(body.Guid, Optional(body.DocumentGuid), Optional(body.Url), body.Description);

    public static BcfHeaderFile ToDomain(BcfApiFileBody body) =>
        new(body.Filename, Optional(body.Date), body.Reference, body.IfcProject, body.IfcSpatialStructureElement);

    public static BcfVocabulary ToDomain(BcfApiExtensionsBody body) =>
        new(toSeq(body.TopicType), toSeq(body.TopicStatus), toSeq(body.Priority), toSeq(body.TopicLabel),
            toSeq(body.UserIdType), toSeq(body.SnippetType), toSeq(body.Stage));

    static Vector3 Domain(BcfApiPointBody point) => new((float)point.X, (float)point.Y, (float)point.Z);
}

public sealed record BcfWire(ImmutableArray<BcfTopicWire> Topics) {
    public static Fin<byte[]> Encode(Seq<BcfTopic> topics, Op key) =>
        Try.lift(() => JsonSerializer.SerializeToUtf8Bytes(new BcfWire([.. topics.Map(BcfWireMapper.ToWire)]), BcfWireContext.Json)).Run()
            .MapFail(error => new BimFault.ModelRejected(key, $"bcf-wire-encode:{error.Message}"));

    // A JSON `null` payload deserializes to a null root — thrown into the SAME funnel so the caller reads one
    // BimFault, never a Succ(null!) escaping the admission.
    public static Fin<BcfWire> Decode(ReadOnlyMemory<byte> json, Op key) =>
        Try.lift(() => JsonSerializer.Deserialize<BcfWire>(json.Span, BcfWireContext.Json) ?? throw new InvalidDataException("null-payload")).Run()
            .MapFail(error => new BimFault.ModelRejected(key, $"bcf-wire-decode:{error.Message}"));

    // ONE spec-body serializer: every BCF-API write body — single-entity or collection — rides this generic funnel
    // through the snake_case BcfApiContext register (never the TS payload root or its camelCase register), the
    // fault detail naming the resource route the dispatch arm already holds. The retired Topic/Comment/Viewpoint/
    // Files/RelatedTopics/DocumentReference *Json sibling family differed only by payload type — the same-rail
    // collapse signal; the domain-to-body mapping rides the dispatch arm that owns the case.
    public static Fin<byte[]> Json<T>(T body, string resource, Op key) =>
        Try.lift(() => JsonSerializer.SerializeToUtf8Bytes(body, BcfApiContext.Json)).Run()
            .MapFail(error => new BimFault.ModelRejected(key, $"bcf-api:{resource}:{error.Message}"));
}

// The BCF-API 3.0 topic-resource request the polymorphic `Project` discriminates on: each case is one resource
// operation the buildingSMART spec defines over the topic graph, COMPLETE over the project-scoped conversation —
// topic create/read/revise/retire, comment create/read/revise/retire, viewpoint create/read/retire plus the
// SNAPSHOT read (the preview the wire payload deliberately drops; viewpoints are immutable per spec, so no
// revise case exists BY LAW), related-topics read/replace, document-references read/create/revise, header-files
// read/replace, the project document library read/upload, the project read, and the EXTENSIONS read (the
// vocabulary `BcfVocabulary` round-trips) — so a CDE conversation never needs a resource this union cannot
// spell; the server-side events change-feed is the `Rasm.Persistence/Version/ledger` CDE-sync polling
// concern, never a resource case here. Read cases ride `Option` for GET arity (the collection read from the
// single read on ONE case); never a `Get`/`Post` member family beside `Project`.
[Union]
public abstract partial record BcfResource {
    private BcfResource() { }

    public sealed record CreateTopic(BcfTopic Topic) : BcfResource;
    public sealed record ReviseTopic(BcfTopic Topic) : BcfResource;
    public sealed record ReadTopic(Option<string> Guid, Option<BcfQuery> Query = default) : BcfResource;
    public sealed record RetireTopic(string Guid) : BcfResource;
    public sealed record AddComment(string TopicGuid, BcfComment Comment) : BcfResource;
    public sealed record ReviseComment(string TopicGuid, BcfComment Comment) : BcfResource;
    public sealed record ReadComments(string TopicGuid, Option<string> Comment, Option<BcfQuery> Query = default) : BcfResource;
    public sealed record RetireComment(string TopicGuid, string Guid) : BcfResource;
    public sealed record AddViewpoint(string TopicGuid, BcfViewpoint Viewpoint) : BcfResource;
    public sealed record ReadViewpoints(string TopicGuid, Option<string> Viewpoint, Option<BcfQuery> Query = default) : BcfResource;
    public sealed record RetireViewpoint(string TopicGuid, string Guid) : BcfResource;
    public sealed record ReadSnapshot(string TopicGuid, string Viewpoint) : BcfResource;
    public sealed record ReadRelatedTopics(string TopicGuid) : BcfResource;
    public sealed record SetRelatedTopics(string TopicGuid, Seq<string> RelatedTopics) : BcfResource;
    public sealed record ReadDocumentReferences(string TopicGuid) : BcfResource;
    public sealed record AddDocumentReference(string TopicGuid, BcfDocumentReference Reference) : BcfResource;
    public sealed record ReviseDocumentReference(string TopicGuid, BcfDocumentReference Reference) : BcfResource;
    public sealed record ReadFiles(string TopicGuid) : BcfResource;
    public sealed record SetFiles(string TopicGuid, Seq<BcfHeaderFile> Files) : BcfResource;
    public sealed record ReadDocuments(Option<string> Document) : BcfResource;
    public sealed record AddDocument(BcfDocument Document) : BcfResource;
    public sealed record ReadProject : BcfResource;
    public sealed record ReadExtensions : BcfResource;
}

// The BCF-API 3.0 REST RESOURCE the Compute transport issues: Bim owns the resource MODEL (the verb + the resource
// path + the spec-dialect body the `BcfApiContext` snake_case register serializes), the HTTP transport
// (client/retry/auth) stays the `Rasm.Compute/Runtime/transport#TRANSPORT_AXIS` owner's — a transport minted here
// is the named seam violation. `BcfApiRequest` is a transport-NEUTRAL resource descriptor, never a Compute
// transport type.
public sealed record BcfApiRequest(BcfApiVerb Verb, string Resource, ReadOnlyMemory<byte> Body);

// The OData query axis the spec's collection GETs admit ($filter/$orderby/$top/$skip) and the lane's paging
// CURSOR: `Render` is the ONE query-string mint every collection route takes (the document-upload `filename`
// parameter is a spec-named binary-POST axis, not OData, and stays on its own arm), `Next` advances the cursor
// by the rows a page actually returned, and exhaustion is the caller's `received < Top` read — the spec's
// responses are bare arrays with no paging envelope, so the cursor lives on the REQUEST and never on a
// fabricated response wrapper.
public sealed record BcfQuery(Option<string> Filter = default, Option<string> OrderBy = default, Option<int> Top = default, int Skip = 0) {
    public static readonly BcfQuery All = new();

    public BcfQuery Next(int received) => this with { Skip = Skip + received };

    public string Render(string route) =>
        Seq(Filter.Map(static f => $"$filter={Uri.EscapeDataString(f)}"),
            OrderBy.Map(static o => $"$orderby={Uri.EscapeDataString(o)}"),
            Top.Map(static t => $"$top={t}"),
            Skip > 0 ? Some($"$skip={Skip}") : None)
        .Somes() is { IsEmpty: false } parameters ? $"{route}?{string.Join('&', parameters)}" : route;
}

// The response peer of `BcfApiRequest`: the transport hands back status + raw bytes and ONE `Open` lands them on
// the typed archive family — each case the collection its resources answer with, `Done` the body-less
// write/retire acknowledgement. Single-read and write-echo responses land as one-element collections on the SAME
// case their collection read answers, so a consumer folds one shape per family and arity never mints a sibling.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BcfOutcome {
    private BcfOutcome() { }

    public sealed record Topics(Seq<BcfTopic> Rows) : BcfOutcome;
    public sealed record Comments(Seq<BcfComment> Rows) : BcfOutcome;
    public sealed record Viewpoints(Seq<BcfViewpoint> Rows) : BcfOutcome;
    // ONE raw-bytes case for both binary GETs — the viewpoint snapshot and the single-document fetch carry
    // identical payload semantics (fetched bytes, no register), so a per-endpoint byte case is the merged form.
    public sealed record Payload(ReadOnlyMemory<byte> Bytes) : BcfOutcome;
    public sealed record Related(Seq<string> TopicGuids) : BcfOutcome;
    public sealed record References(Seq<BcfDocumentReference> Rows) : BcfOutcome;
    public sealed record Files(Seq<BcfHeaderFile> Rows) : BcfOutcome;
    public sealed record Documents(Seq<BcfDocument> Rows) : BcfOutcome;
    public sealed record Project(BcfProject Value) : BcfOutcome;
    public sealed record Extensions(BcfVocabulary Value) : BcfOutcome;
    public sealed record Done : BcfOutcome;
}

// --- [SERVICES] ---------------------------------------------------------------------------
// ONE polymorphic `Project` over the BCF-API 3.0 resource union, discriminating on the `BcfResource` input shape via
// the generated TOTAL `Switch` — the verb DERIVES from the case (POST a create, PUT a revision/replace, GET a read,
// DELETE a retire; GET/DELETE carry no body), every write arm feeding its spec body through the ONE `Write` kernel
// (route computed once, the same string the fault names), the document upload carrying the raw document bytes with
// the filename riding the query string per the spec's binary-body POST.
public static class BcfApi {
    public static Fin<BcfApiRequest> Project(string projectId, BcfResource resource, Op key) =>
        resource.Switch(
            state:                   (key, project: $"bcf/3.0/projects/{projectId}", topics: $"bcf/3.0/projects/{projectId}/topics"),
            createTopic:             static (s, r) => Write(BcfApiVerb.Post, s.topics, BcfWireMapper.ToBody(r.Topic), s.key),
            reviseTopic:             static (s, r) => Write(BcfApiVerb.Put, $"{s.topics}/{r.Topic.Guid}", BcfWireMapper.ToBody(r.Topic), s.key),
            readTopic:               static (s, r) => Fin.Succ(new BcfApiRequest(BcfApiVerb.Get, r.Guid.Match(Some: g => $"{s.topics}/{g}", None: () => r.Query.IfNone(BcfQuery.All).Render(s.topics)), default)),
            retireTopic:             static (s, r) => Fin.Succ(new BcfApiRequest(BcfApiVerb.Delete, $"{s.topics}/{r.Guid}", default)),
            addComment:              static (s, r) => Write(BcfApiVerb.Post, $"{s.topics}/{r.TopicGuid}/comments", BcfWireMapper.ToBody(r.Comment), s.key),
            reviseComment:           static (s, r) => Write(BcfApiVerb.Put, $"{s.topics}/{r.TopicGuid}/comments/{r.Comment.Guid}", BcfWireMapper.ToBody(r.Comment), s.key),
            readComments:            static (s, r) => Fin.Succ(new BcfApiRequest(BcfApiVerb.Get, r.Comment.Match(Some: c => $"{s.topics}/{r.TopicGuid}/comments/{c}", None: () => r.Query.IfNone(BcfQuery.All).Render($"{s.topics}/{r.TopicGuid}/comments")), default)),
            retireComment:           static (s, r) => Fin.Succ(new BcfApiRequest(BcfApiVerb.Delete, $"{s.topics}/{r.TopicGuid}/comments/{r.Guid}", default)),
            addViewpoint:            static (s, r) => Write(BcfApiVerb.Post, $"{s.topics}/{r.TopicGuid}/viewpoints", BcfWireMapper.ToBody(r.Viewpoint), s.key),
            readViewpoints:          static (s, r) => Fin.Succ(new BcfApiRequest(BcfApiVerb.Get, r.Viewpoint.Match(Some: v => $"{s.topics}/{r.TopicGuid}/viewpoints/{v}", None: () => r.Query.IfNone(BcfQuery.All).Render($"{s.topics}/{r.TopicGuid}/viewpoints")), default)),
            retireViewpoint:         static (s, r) => Fin.Succ(new BcfApiRequest(BcfApiVerb.Delete, $"{s.topics}/{r.TopicGuid}/viewpoints/{r.Guid}", default)),
            readSnapshot:            static (s, r) => Fin.Succ(new BcfApiRequest(BcfApiVerb.Get, $"{s.topics}/{r.TopicGuid}/viewpoints/{r.Viewpoint}/snapshot", default)),
            readRelatedTopics:       static (s, r) => Fin.Succ(new BcfApiRequest(BcfApiVerb.Get, $"{s.topics}/{r.TopicGuid}/related_topics", default)),
            setRelatedTopics:        static (s, r) => Write(BcfApiVerb.Put, $"{s.topics}/{r.TopicGuid}/related_topics", r.RelatedTopics.Map(static g => new BcfApiRelatedTopicBody(g)).ToImmutableArray(), s.key),
            readDocumentReferences:  static (s, r) => Fin.Succ(new BcfApiRequest(BcfApiVerb.Get, $"{s.topics}/{r.TopicGuid}/document_references", default)),
            addDocumentReference:    static (s, r) => Write(BcfApiVerb.Post, $"{s.topics}/{r.TopicGuid}/document_references", BcfWireMapper.ToBody(r.Reference), s.key),
            reviseDocumentReference: static (s, r) => Write(BcfApiVerb.Put, $"{s.topics}/{r.TopicGuid}/document_references/{r.Reference.Guid}", BcfWireMapper.ToBody(r.Reference), s.key),
            readFiles:               static (s, r) => Fin.Succ(new BcfApiRequest(BcfApiVerb.Get, $"{s.topics}/{r.TopicGuid}/files", default)),
            setFiles:                static (s, r) => Write(BcfApiVerb.Put, $"{s.topics}/{r.TopicGuid}/files", r.Files.Map(BcfWireMapper.ToBody).ToImmutableArray(), s.key),
            readDocuments:           static (s, r) => Fin.Succ(new BcfApiRequest(BcfApiVerb.Get, r.Document.Match(Some: d => $"{s.project}/documents/{d}", None: () => $"{s.project}/documents"), default)),
            addDocument:             static (s, r) => Fin.Succ(new BcfApiRequest(BcfApiVerb.Post, $"{s.project}/documents?filename={Uri.EscapeDataString(r.Document.Filename)}", r.Document.Data)),
            readProject:             static (s, _) => Fin.Succ(new BcfApiRequest(BcfApiVerb.Get, s.project, default)),
            readExtensions:          static (s, _) => Fin.Succ(new BcfApiRequest(BcfApiVerb.Get, $"{s.project}/extensions", default)));

    static Fin<BcfApiRequest> Write<T>(BcfApiVerb verb, string resource, T body, Op key) =>
        BcfWire.Json(body, resource, key).Map(bytes => new BcfApiRequest(verb, resource, bytes));

    // The READ half of the lane — the response-admission seam DECIDED Bim-side: a GET response decodes through
    // the SAME snake_case BcfApiContext register the write bodies ride (the read shape IS the resource body plus
    // its nullable server-assigned columns), ONE polymorphic Admit landing both GET arities (T the single body or
    // its collection wrapper) on the Fin rail; the Compute transport#TRANSPORT_AXIS owner hands raw bytes and
    // decodes NOTHING — a transport-side dialect adapter is the rejected second seam. The domain lift is the
    // BcfWireMapper ToDomain inverse over the admitted body, never a hand transcription beside it.
    public static Fin<T> Admit<T>(ReadOnlyMemory<byte> body, string resource, Op key) =>
        Try.lift(() => JsonSerializer.Deserialize<T>(body.Span, BcfApiContext.Json)).Run()
            .MapFail(error => new BimFault.ModelRejected(key, $"bcf-api-read:{resource}:{error.Message}"))
            .Bind(value => value is { } admitted
                ? Fin.Succ(admitted)
                : Fin.Fail<T>(new BimFault.ModelRejected(key, $"bcf-api-read:{resource}:null-payload")));

    // The RESPONSE half completing the round trip: the transport hands back status + raw bytes UNDECODED, and
    // this one total Switch over the SAME closed resource union lands them on the typed BcfOutcome family — the
    // issuing case knows its body shape, so response decode never grows a per-endpoint reader. A non-2xx status
    // refuses ONCE here with the status and the resource case named (retry, auth, and backoff are the Compute
    // transport's; this seam only reads the verdict); a write echo re-admits through the SAME snake_case register
    // so the server-assigned columns land; a retire and both replace acknowledgements land Done; the snapshot is
    // a PNG pass-through no register touches. The pagination cursor lives on the REQUEST (BcfQuery.Next), so a
    // page walk is: issue, Open, fold rows, advance the query — exhausted when a page returns fewer than Top.
    public static Fin<BcfOutcome> Open(BcfResource resource, int status, ReadOnlyMemory<byte> body, Op key) =>
        status is >= 200 and < 300
            ? resource.Switch(
                state:                   (body, key),
                createTopic:             static (s, _) => One<BcfApiTopicBody>(s.body, "topics", s.key).Map(static b => (BcfOutcome)new BcfOutcome.Topics(Seq(BcfWireMapper.ToDomain(b)))),
                reviseTopic:             static (s, _) => One<BcfApiTopicBody>(s.body, "topics", s.key).Map(static b => (BcfOutcome)new BcfOutcome.Topics(Seq(BcfWireMapper.ToDomain(b)))),
                readTopic:               static (s, r) => r.Guid.IsSome
                                             ? One<BcfApiTopicBody>(s.body, "topics", s.key).Map(static b => (BcfOutcome)new BcfOutcome.Topics(Seq(BcfWireMapper.ToDomain(b))))
                                             : Many<BcfApiTopicBody>(s.body, "topics", s.key).Map(static rows => (BcfOutcome)new BcfOutcome.Topics(rows.Map(BcfWireMapper.ToDomain))),
                retireTopic:             static (s, _) => Fin.Succ((BcfOutcome)new BcfOutcome.Done()),
                addComment:              static (s, _) => One<BcfApiCommentBody>(s.body, "comments", s.key).Map(static b => (BcfOutcome)new BcfOutcome.Comments(Seq(BcfWireMapper.ToDomain(b)))),
                reviseComment:           static (s, _) => One<BcfApiCommentBody>(s.body, "comments", s.key).Map(static b => (BcfOutcome)new BcfOutcome.Comments(Seq(BcfWireMapper.ToDomain(b)))),
                readComments:            static (s, r) => r.Comment.IsSome
                                             ? One<BcfApiCommentBody>(s.body, "comments", s.key).Map(static b => (BcfOutcome)new BcfOutcome.Comments(Seq(BcfWireMapper.ToDomain(b))))
                                             : Many<BcfApiCommentBody>(s.body, "comments", s.key).Map(static rows => (BcfOutcome)new BcfOutcome.Comments(rows.Map(BcfWireMapper.ToDomain))),
                retireComment:           static (s, _) => Fin.Succ((BcfOutcome)new BcfOutcome.Done()),
                addViewpoint:            static (s, _) => One<BcfApiViewpointBody>(s.body, "viewpoints", s.key).Bind(b => BcfWireMapper.ToDomain(b, s.key)).Map(static v => (BcfOutcome)new BcfOutcome.Viewpoints(Seq(v))),
                readViewpoints:          static (s, r) => r.Viewpoint.IsSome
                                             ? One<BcfApiViewpointBody>(s.body, "viewpoints", s.key).Bind(b => BcfWireMapper.ToDomain(b, s.key)).Map(static v => (BcfOutcome)new BcfOutcome.Viewpoints(Seq(v)))
                                             : Many<BcfApiViewpointBody>(s.body, "viewpoints", s.key).Bind(rows => rows.TraverseM(b => BcfWireMapper.ToDomain(b, s.key)).As()).Map(static rows => (BcfOutcome)new BcfOutcome.Viewpoints(rows)),
                retireViewpoint:         static (s, _) => Fin.Succ((BcfOutcome)new BcfOutcome.Done()),
                readSnapshot:            static (s, _) => Fin.Succ((BcfOutcome)new BcfOutcome.Payload(s.body)),
                readRelatedTopics:       static (s, _) => Many<BcfApiRelatedTopicBody>(s.body, "related_topics", s.key).Map(static rows => (BcfOutcome)new BcfOutcome.Related(rows.Map(static r => r.RelatedTopicGuid))),
                setRelatedTopics:        static (s, _) => Fin.Succ((BcfOutcome)new BcfOutcome.Done()),
                readDocumentReferences:  static (s, _) => Many<BcfApiDocumentReferenceBody>(s.body, "document_references", s.key).Map(static rows => (BcfOutcome)new BcfOutcome.References(rows.Map(BcfWireMapper.ToDomain))),
                addDocumentReference:    static (s, _) => One<BcfApiDocumentReferenceBody>(s.body, "document_references", s.key).Map(static b => (BcfOutcome)new BcfOutcome.References(Seq(BcfWireMapper.ToDomain(b)))),
                reviseDocumentReference: static (s, _) => One<BcfApiDocumentReferenceBody>(s.body, "document_references", s.key).Map(static b => (BcfOutcome)new BcfOutcome.References(Seq(BcfWireMapper.ToDomain(b)))),
                readFiles:               static (s, _) => Many<BcfApiFileBody>(s.body, "files", s.key).Map(static rows => (BcfOutcome)new BcfOutcome.Files(rows.Map(BcfWireMapper.ToDomain))),
                setFiles:                static (s, _) => Fin.Succ((BcfOutcome)new BcfOutcome.Done()),
                readDocuments:           static (s, r) => r.Document.IsSome
                                             ? Fin.Succ((BcfOutcome)new BcfOutcome.Payload(s.body))
                                             : Many<BcfApiDocumentBody>(s.body, "documents", s.key).Map(static rows => (BcfOutcome)new BcfOutcome.Documents(rows.Map(static d => new BcfDocument(d.Guid, d.Filename, d.Description ?? "", default)))),
                addDocument:             static (s, _) => One<BcfApiDocumentBody>(s.body, "documents", s.key).Map(static d => (BcfOutcome)new BcfOutcome.Documents(Seq(new BcfDocument(d.Guid, d.Filename, d.Description ?? "", default)))),
                readProject:             static (s, _) => One<BcfApiProjectBody>(s.body, "project", s.key).Map(static p => (BcfOutcome)new BcfOutcome.Project(new BcfProject(p.ProjectId, p.Name))),
                readExtensions:          static (s, _) => One<BcfApiExtensionsBody>(s.body, "extensions", s.key).Map(static e => (BcfOutcome)new BcfOutcome.Extensions(BcfWireMapper.ToDomain(e))))
            : Fin.Fail<BcfOutcome>(new BimFault.ModelRejected(key, $"bcf-api-status:{status}:{resource.GetType().Name}"));

    static Fin<T> One<T>(ReadOnlyMemory<byte> body, string resource, Op key) => Admit<T>(body, resource, key);

    // The collection arity carries its OWN absence gate. Admit's non-null test is a reference-type test, and the
    // collection root is a struct: a hostile or truncated CDE answering a bare `null` for a collection GET
    // deserializes to the UNINITIALIZED ImmutableArray, which passes every null check and then throws on its first
    // read — an exception escaping the Fin rail from inside a boundary whose whole purpose is to keep one out.
    // IsDefault is the only predicate that separates it from a legitimately empty page.
    static Fin<Seq<T>> Many<T>(ReadOnlyMemory<byte> body, string resource, Op key) =>
        Admit<ImmutableArray<T>>(body, resource, key)
            .Bind(rows => rows.IsDefault
                ? Fin.Fail<Seq<T>>(new BimFault.ModelRejected(key, $"bcf-api-read:{resource}:null-collection"))
                : Fin.Succ(toSeq(rows)));
}

// --- [COMPOSITION] ------------------------------------------------------------------------
// The SELF-OWNED BCF wire serializer — replaces the retired generic `BimWireContext` (a model STJ context inside an
// AEC package was a strata leak; the seam-graph STJ wire is `Rasm.Persistence/Element/codec#CODEC_AXIS`
// `SnapshotCodec`'s). Camel-case names, `[JsonStringEnumMemberName]` string enums (`UseStringEnumConverter`),
// `IncludeFields` so the BCL `Vector3` serializes as `{x,y,z}`, and the NodaTime `Instant` ISO-8601 converters
// (`ConfigureForNodaTime`). ONE registered root — the payload; the source generator walks the nested wire graph
// from it (the single-entity REST bodies ride `BcfApiContext`, never this register). No Thinktecture converter —
// the BCF family carries no `[ValueObject]`/`[SmartEnum]` (`BcfResource`/`BcfCamera` are request/domain unions,
// never serialized).
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, UseStringEnumConverter = true, IncludeFields = true)]
[JsonSerializable(typeof(BcfWire))]
public sealed partial class BcfWireContext : JsonSerializerContext {
    public static readonly JsonSerializerOptions Json =
        new JsonSerializerOptions(Default.Options).ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
}

// The BCF-API dialect register — the SAME source-generated posture under the spec's snake_case field law
// (topic_status/camera_view_point/related_topic_guid), null slots OMITTED so the camera XOR and the optional
// document_guid/url discriminate by ABSENCE per the spec, never a null literal. Two naming registers over one
// issue vocabulary, never a second topic family; the write-body roots AND the response roots (each collection
// GET's array root beside the three read-only bodies) register here, so `BcfApi.Open` decodes every arm off
// this one register and no transport-side dialect adapter exists.
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower, UseStringEnumConverter = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(BcfApiTopicBody))]
[JsonSerializable(typeof(BcfApiCommentBody))]
[JsonSerializable(typeof(BcfApiViewpointBody))]
[JsonSerializable(typeof(BcfApiDocumentReferenceBody))]
[JsonSerializable(typeof(BcfApiDocumentBody))]
[JsonSerializable(typeof(BcfApiProjectBody))]
[JsonSerializable(typeof(BcfApiExtensionsBody))]
[JsonSerializable(typeof(ImmutableArray<BcfApiTopicBody>))]
[JsonSerializable(typeof(ImmutableArray<BcfApiCommentBody>))]
[JsonSerializable(typeof(ImmutableArray<BcfApiViewpointBody>))]
[JsonSerializable(typeof(ImmutableArray<BcfApiDocumentReferenceBody>))]
[JsonSerializable(typeof(ImmutableArray<BcfApiDocumentBody>))]
[JsonSerializable(typeof(ImmutableArray<BcfApiRelatedTopicBody>))]
[JsonSerializable(typeof(ImmutableArray<BcfApiFileBody>))]
public sealed partial class BcfApiContext : JsonSerializerContext {
    public static readonly JsonSerializerOptions Json =
        new JsonSerializerOptions(Default.Options).ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
}
```

## [04]-[RESEARCH]

(none)
