# [BIM_ISSUE_EXCHANGE]

The BCF 2.1/3.0 issue-exchange owner: one closed `BcfFile`/`BcfTopic`/`BcfComment`/`BcfViewpoint` record family anchored on IFC GlobalIds and carried at the FULL `Bcf30` schema surface — topic references (`RelatedTopics`/`DocumentReferences`/`BimSnippet`/`ReferenceLinks`/header `Files`), modification provenance (`ModifiedDate`/`ModifiedAuthor`/`Index`/`ServerAssignedId`), the typed `BcfCamera` perspective/orthogonal union, the viewpoint markup payload (`Coloring`/`Lines`/`ClippingPlanes`/`Bitmaps`/`BcfVisibility`/`BcfViewHint`), and the archive-level `BcfProject`/`BcfVocabulary`/`BcfDocument` members a `WithDefaults()`-only author drops. The `.bcfzip` codec composes the `Smino.Bcf.Toolkit` surface at both depths — the streaming `BcfExtensions` per-part parse for a native 3.0 container, the `Worker.BcfFromStream` up-conversion for a 2.1 source, and the fluent `BcfBuilder` authoring fold on write — plus a `BcfApi` BCF-API 3.0 resource projection the `Rasm.Compute/Runtime/channels#TRANSPORT_AXIS` transport issues. BCF is an issue/coordination container, never a geometry-or-model interchange row: the `.bcfzip` codec is self-owned here and is NOT a row on the `Exchange/format#FORMAT_AXIS` geometry-format axis, because BCF carries issues and viewpoints, never an `ElementGraph` or `ImportedGeometry`. Viewpoints anchor on IFC GlobalIds — each the seam `Rasm.Element/Graph/element#NODE_MODEL` `Node.Object.ExternalId` the `Projection/semantic#SEMANTIC_PROJECTOR` projects 1:1 from `IfcRoot.GlobalId` [H6]. The page is HOST-LOCAL.

## [01]-[INDEX]

- [02]-[BCF_ARCHIVE]: the `BcfFile` archive root (`BcfTopic`/`BcfComment`/`BcfViewpoint` family, `BcfCamera` and `BcfVisibility` unions, `BcfViewHint` capability vocabulary, `BcfProject`/`BcfVocabulary`/`BcfDocument` members), `BcfEdge` the one foreign-schema correspondence, and the `.bcfzip` codec over `BcfExtensions`/`Worker`/`BcfBuilder`.
- [03]-[TS_PROJECTION]: the generated `Bcf.V1` contract projection the TS UI decodes, the `BcfLifecycle` status election, and the `BcfApi` resource union with its spec-dialect snake_case bodies and per-verb re-drive budget.

## [02]-[BCF_ARCHIVE]

- Owner: `BcfFile` the archive root — the topic set plus the members every `.bcfzip` carries: `BcfProject` the project identity, `BcfVocabulary` the extension vocabularies (the rows `Review/coordination#SIGN_OFF` board lanes and the topic-type axis read), `BcfDocument` the embedded library, `BcfGeneration` the source evidence; `BcfTopic` the issue record at the FULL `Bcf30.Topic` surface; `BcfComment` the threaded comment with its own provenance; `BcfViewpoint` the saved view — the typed `BcfCamera` union, component selection, the `BcfVisibility` regime, the `BcfViewHint` render-hint set, per-component `Coloring`, redline `Lines`, section `ClippingPlanes`, and `Bitmaps`; `BcfEdge` the ONE foreign-schema correspondence carrying every optional-by-schema column and its inverse; `BcfArchive` the `.bcfzip` codec; `BcfApi` the REST projection of the same family.
- Entry: `BcfArchive.Read(ReadOnlyMemory<byte> bcfzip, Op key)` lands thrown container failures and already-typed admissions on one `Fin` through `Op.Catch`, preserving each original `Error` — it sniffs the generation through `BcfExtensions.GetVersionFromStreamArchive`, streams a native 3.0 archive per-part (`ParseMarkups`/`ParseExtensions`/`ParseProject`/`ParseDocuments`, no second full-graph materialization), up-converts a 2.1 source through `Worker.BcfFromStream`, folds both onto one `BcfFile`, and lifts each held `{topicGuid}/{Reference}` bitmap part into `BcfFile.Blobs`; `BcfArchive.Write(BcfFile file, Op key)` seeds the builder from the file's own vocabulary/project/documents (`WithDefaults()` only when the file carries no vocabulary), folds each topic through `BcfBuilder.AddMarkup`, emits through `Worker.ToBcf(bcf, BcfVersionEnum.Bcf30)`, and completes the container with each topic's bitmap parts — a reference the store cannot answer refuses BEFORE the container is touched, so a refusal never leaves a half-written archive. `BcfFile.Of(topics)` is the authoring factory a clash or IDS fold seeds a default-vocabulary file from.
- Auto: `Read` projects every `Bcf30.Topic` column onto `BcfTopic` — `Index`/`ModifiedDate`/`ModifiedAuthor`/`ServerAssignedId`/`ReferenceLinks`/`RelatedTopics`/`DocumentReferences`/`BimSnippet` and the `Markup.Header` `Files` rows — the verbatim `TopicStatus` token landing on `StatusLabel` beside the elected `BcfStatus` lifecycle, and the topic order re-derived deterministically off the schema `Index`-then-`Guid` (a `ConcurrentBag` parse is bag-racy); reads each `Bcf30.ViewPoint` whole, each declared camera transcribed onto its typed case and handed to the ONE `BcfCamera.Admit` gate; `Write` re-authors ALL of it through the nested builders so the write round-trips exactly what the read captured.
- Receipt: the `BcfFile` is the coordination evidence — topics, vocabulary, project, documents, and the source generation — so a CDE or viewer round-trips one typed root through the `Worker` `.bcfzip` codec, and the `BcfApi` REST projection rides the same topic family, never a second vocabulary.
- Packages: Smino.Bcf.Toolkit, Rasm.Contracts (`Bcf.V1.BcfStatus`), Google.Protobuf, NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, `Rasm.Domain` (`Op`, `ICapability`/`CapabilitySet`)
- Growth: a new BCF entity is one record on the family projected from the `Smino.Bcf.Toolkit` graph, its members verified at `.api/api-smino-bcf-toolkit` before the fence spells them; a new topic or viewpoint column is one trailing-defaulted field the read/write folds each gain one line for; a new render hint is one `BcfViewHint` row; a new BCF version is one `BcfVersionEnum` the `Worker` converter already discriminates; the REST projection is one `BcfResource` case; never a row on the geometry-format axis and never a second issue store.
- Boundary: FOREIGN-SCHEMA WIRE COLUMNS CROSS VERBATIM — `BcfHeaderFile.IsExternal`, `BcfBimSnippet.IsExternal`, the generated `BcfFileWire`/`BcfSnippetWire`, and `BcfApiSnippetBody` carry the same BCF 3.0 choices the builder round-trips through `SetIsExternal`; a house origin vocabulary would fork the standard (`RULINGS.md [04]`). The DOMAIN carriers are typed where the schema's own columns carry a corner law: `Components/Visibility` is `BcfVisibility`, a closed pair whose case names what the exception set means, while the three independent `ViewSetupHints` attributes ride one `CapabilitySet<BcfViewHint>`. Both project attribute-by-attribute at the builder, generated contract, and REST body. A board mutation fires the `Model/observability#HOOK_RAIL` `rasm.bim.review.issue` point with `BimFact.IssueMutated`; the CloudEvents announcement remains `Exchange/events#EVENT_PROJECTION`'s observe subscription. `Smino.Bcf.Toolkit` owns container read/write, with bitmap parts the single carve because its model carries references but no payload member; the codec lifts held parts into `BcfFile.Blobs` on read and appends them on write. `BcfStatus` is the generated lifecycle discriminant and `StatusLabel` carries the project-vocabulary token verbatim; `StatusToken` elects the write spelling without losing an extension status. `BcfCamera.Admit` owns the camera XOR and typed absence, and `BcfComment.ReplyToGuid` remains the REST-lane join BCF 3.0 markup omits. Document references accumulate the `DocumentGuid` XOR `Url` refusal beside malformed cameras. `Worker` and `BcfExtensions` complete at this boundary, every throw lowers through the `Fin<T>` funnel, and the foreign fluent builders retain the platform-forced statement seam. Viewpoints anchor on `Node.Object.ExternalId` [H6]. `BcfApi` publishes transport-neutral requests the Compute transport executes. Persistence joins typed rows by IFC GlobalId and owns durable lineage; `BcfArchive` remains the branch's one `.bcfzip` custodian.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using BcfToolkit;
using BcfToolkit.Builder.Bcf30;
using BcfToolkit.Model;
using BcfToolkit.Utils;
using LanguageExt;
using LanguageExt.Common;
using NodaTime;
using Rasm.Contracts.Bcf.V1;
using Rasm.Bim.Model;
using Rasm.Domain;
using Thinktecture;
using Bcf30 = BcfToolkit.Model.Bcf30;
using Op = Rasm.Domain.Op;
using Vector3 = System.Numerics.Vector3;
using static LanguageExt.Prelude;

// The BCF/coordination DOMAIN namespace the ARCHITECTURE seams name and AppUi `Collab/issues` consumes as
// `Rasm.Bim.Coordination.BcfTopic`; the codemap `Review/Coordination.cs`/`Review/Issues.cs` files emit this
// namespace, not folder-derived `Review`.
namespace Rasm.Bim.Coordination;

// --- [TYPES] ------------------------------------------------------------------------------
// The boundary's own generation axis keeps the toolkit enum outside the interior record family.
public enum BcfGeneration : byte { Bcf21 = 0, Bcf30 = 1 }

// The Visibility element's render hints as ONE combinable column. The schema declares three independent
// attributes and every corner is legal, so no CapabilityLaw constrains the set and Rank stays DERIVED from
// declaration order.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BcfViewHint : ICapability<BcfViewHint> {
    public static readonly BcfViewHint Spaces          = new(key: "spaces");
    public static readonly BcfViewHint SpaceBoundaries = new(key: "space-boundaries");
    public static readonly BcfViewHint Openings        = new(key: "openings");
}

// --- [MODELS] -----------------------------------------------------------------------------
// Perspective carries the field of view, orthogonal the view-to-world scale, BOTH the BCF 3.0 aspect ratio — the
// typed discriminant replaces the fov-0 sentinel a consumer had to decode.
[Union]
public abstract partial record BcfCamera {
    private BcfCamera() { }

    public sealed record Perspective(Vector3 Position, Vector3 Direction, Vector3 Up, double FieldOfViewDeg = 60d, double AspectRatio = 0d) : BcfCamera;
    public sealed record Orthogonal(Vector3 Position, Vector3 Direction, Vector3 Up, double ViewToWorldScale = 1d, double AspectRatio = 0d) : BcfCamera;

    // The ONE camera admission BOTH ingress arms compose — the archive's Bcf30.VisualizationInfo read and the
    // BCF-API viewpoint response — each arm projecting its own foreign shape onto the typed case first and this
    // gate owning the whole decision. BCF declares orthogonal_camera XOR perspective_camera: BOTH present is a
    // malformed viewpoint that rails typed, exactly one admits, and NEITHER is the legal selection-only viewpoint
    // carried as typed absence.
    public static Fin<Option<BcfCamera>> Admit(Option<Perspective> perspective, Option<Orthogonal> orthogonal, string viewpoint, Op key) =>
        (perspective.IsSome, orthogonal.IsSome) switch {
            (true, true) => Fin.Fail<Option<BcfCamera>>(new BimFault.Refused(key, BimScope.Review, BimReason.Rejected, string.Join(':', new object?[] { "bcf-camera-xor", viewpoint }))),
            (true, _)    => Fin.Succ(perspective.Map(static p => (BcfCamera)p)),
            (_, true)    => Fin.Succ(orthogonal.Map(static o => (BcfCamera)o)),
            _            => Fin.Succ(Option<BcfCamera>.None),
        };
}

// The viewpoint markup payload rows — each mirrors one Bcf30 schema type host-free. DocumentGuid XOR Url
// discriminates an internal library document from an external one, and BcfEdge admits that XOR.
public sealed record BcfHeaderFile(string Filename, Option<Instant> Date, string Reference, string IfcProject, string IfcSpatialStructureElement, bool IsExternal = true);
public sealed record BcfDocumentReference(string Guid, Option<string> DocumentGuid, Option<string> Url, string Description);
public sealed record BcfBimSnippet(string SnippetType, string Reference, string ReferenceSchema, bool IsExternal);
public sealed record BcfColoring(string Color, Seq<string> GlobalIds);
public sealed record BcfLine(Vector3 Start, Vector3 End);
public sealed record BcfClippingPlane(Vector3 Location, Vector3 Direction);
public sealed record BcfBitmap(string Format, string Reference, Vector3 Location, Vector3 Normal, Vector3 Up, double Height);

// The Components/Visibility regime as ONE closed pair: the schema's DefaultVisibility decides what its Exceptions
// set MEANS, so the case NAMES the set and the inverted read a bare bool forced on every consumer cannot be
// spelled. Default and Exceptions are the derivations the wire and REST registers cross on.
[Union]
public abstract partial record BcfVisibility {
    private BcfVisibility() { }

    public sealed record Showing(Seq<string> Hidden) : BcfVisibility;
    public sealed record Hiding(Seq<string> Visible) : BcfVisibility;

    // An absent Components/Visibility element is the spec's everything-visible default.
    public static readonly BcfVisibility Everything = new Showing(Seq<string>());

    public static BcfVisibility Of(bool @default, Seq<string> exceptions) => @default ? new Showing(exceptions) : new Hiding(exceptions);

    public bool Default => this is Showing;
    public Seq<string> Exceptions => Switch(showing: static s => s.Hidden, hiding: static h => h.Visible);
}

public sealed record BcfViewpoint(
    string Guid,
    Option<BcfCamera> Camera,
    Seq<string> SelectedGlobalIds,
    BcfVisibility Visibility,
    Option<ReadOnlyMemory<byte>> Snapshot,
    Option<CapabilitySet<BcfViewHint>> Hints = default,
    Seq<BcfColoring> Coloring = default,
    Seq<BcfLine> Lines = default,
    Seq<BcfClippingPlane> ClippingPlanes = default,
    Seq<BcfBitmap> Bitmaps = default,
    Option<int> Index = default);

// ReplyToGuid is a REST-lane column only: BCF 3.0 removed the reply join from the markup schema, so the archive
// read lands None and the archive write emits nothing while the BCF-API `comment` resource carries it both ways.
// It stays on the domain record rather than a REST-only mirror, because a topic read from a server and written to
// a container is ONE comment family.
public sealed record BcfComment(
    string Guid,
    string Author,
    string Text,
    Option<string> ViewpointGuid,
    Instant Date,
    Option<Instant> ModifiedDate = default,
    string ModifiedAuthor = "",
    Option<string> ReplyToGuid = default);

// The issue record at the FULL Bcf30.Topic surface: the core nine positional, every further schema column
// trailing-defaulted so a minimal caller still constructs the core. StatusLabel preserves the project-vocabulary
// TopicStatus token verbatim (BCF status is an extensions-defined free string), so a round-trip never rewrites
// "Under Review" to "Open" — StatusToken is the ONE write-side election both folds read.
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

// The archive-level members WithDefaults()-only authoring dropped: the project identity, the extension
// vocabularies the Review/coordination#SIGN_OFF board lanes read, and the library DocumentGuid joins.
public sealed record BcfProject(string ProjectId, string Name);
public sealed record BcfDocument(string Guid, string Filename, string Description, ReadOnlyMemory<byte> Data);
public sealed record BcfVocabulary(
    Seq<string> TopicTypes, Seq<string> TopicStatuses, Seq<string> Priorities, Seq<string> TopicLabels,
    Seq<string> Users, Seq<string> SnippetTypes, Seq<string> Stages);

// The archive root: what a .bcfzip IS. Vocabulary None on write seeds the toolkit defaults. Blobs is the bitmap
// part store keyed by `BcfBitmap.Reference` — Read lifts each held container part into it, an authoring caller
// seeds it beside the `BcfBitmap` rows it mints, and Write refuses a reference the store cannot answer.
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
// The ONE foreign-schema correspondence at the BCF edge, forward and inverse on one owner: the .bcfzip toolkit
// graph and the BCF-API bodies both land their optional-by-schema columns here, so the coalesce is spelled ONCE
// for the whole boundary and the interior never sees a nullable BCF column. BcfProjection pulls these as
// generated carriers through [UseStaticMapper], so the REST lane composes the same admission the archive does.
public static class BcfEdge {
    // BCF declares nearly every scalar optional while the domain records carry non-nullable strings, so an
    // omitted column admits as the empty token the write side re-emits as an empty element.
    public static string Word(string? value) => value ?? "";
    public static bool Flag(bool? value) => value ?? false;
    // A server-assigned stamp is absent on a write body and present on its echo; the epoch floor IS the admitted
    // absence, never a nullable column the interior would re-test.
    public static Instant Stamped(Instant? value) => value ?? Instant.MinValue;
    public static Instant At(DateTime value) => Instant.FromDateTimeUtc(DateTime.SpecifyKind(value, DateTimeKind.Utc));
    public static Option<Instant> AtOf(DateTime? value) => Optional(value).Map(At);
    public static Option<string> OptionText(string? value) => Optional(value).Filter(static v => v.Length > 0);
    public static Option<Instant> OptionMoment(Instant? value) => Optional(value);
    public static Option<int> OptionOrdinal(int? value) => Optional(value);
    public static Seq<T> Rows<T>(IEnumerable<T>? rows) => rows is null ? Seq<T>() : toSeq(rows);

    // CameraViewPoint is a Bcf30.Point while CameraDirection/CameraUpVector are Bcf30.Direction — distinct
    // toolkit types sharing the X/Y/Z shape with no common base, so each takes its own arm.
    public static Vector3 Vec(Bcf30.Point? p) => p is null ? default : new((float)p.X, (float)p.Y, (float)p.Z);
    public static Vector3 Vec(Bcf30.Direction? d) => d is null ? default : new((float)d.X, (float)d.Y, (float)d.Z);
    public static Vector3 Vec(BcfApiPointBody point) => new((float)point.X, (float)point.Y, (float)point.Z);

    // One base64 admission for snapshot and document payloads, decoded non-throwing: a malformed or absent
    // payload yields None, because an optional binary never faults an archive read.
    public static Option<ReadOnlyMemory<byte>> Base64Of(FileData? payload) {
        if (payload?.Data is not { Length: > 0 } data) { return None; }
        byte[] buffer = new byte[data.Length];
        return Convert.TryFromBase64String(data, buffer, out int written) ? Some<ReadOnlyMemory<byte>>(buffer.AsMemory(0, written)) : None;
    }

    // An absent components object is the spec's everything-visible default.
    public static BcfApiComponentsBody Parts(BcfApiComponentsBody? components) =>
        components ?? new BcfApiComponentsBody([], [], new BcfApiVisibilityBody(true, null, []));

    public static Option<CapabilitySet<BcfViewHint>> HintsOf(Bcf30.ViewSetupHints? hints) =>
        Optional(hints).Map(static h => Held(h.SpacesVisible, h.SpaceBoundariesVisible, h.OpeningsVisible));

    public static Option<CapabilitySet<BcfViewHint>> HintsOf(BcfApiViewSetupHintsBody? hints) =>
        Optional(hints).Map(static h => Held(h.SpacesVisible, h.SpaceBoundariesVisible, h.OpeningsVisible));

    // Declaration order IS the fold order, so a fourth schema hint is one BcfViewHint row and one argument.
    static CapabilitySet<BcfViewHint> Held(bool spaces, bool boundaries, bool openings) =>
        Seq((spaces, BcfViewHint.Spaces), (boundaries, BcfViewHint.SpaceBoundaries), (openings, BcfViewHint.Openings))
            .Fold(CapabilitySet<BcfViewHint>.None, static (held, row) => row.Item1 ? held.With(row.Item2) : held);
}

// The `.bcfzip` codec at both toolkit depths. The async surface runs to completion at this codec boundary (the
// one language-owned GetAwaiter().GetResult() bridge), every throw lowering onto Fin<T> through Op.Catch while
// retaining the captured Error.
public static class BcfArchive {
    // Op.Catch flattens the capture rail onto the decode's own Fin: container faults (a
    // malformed zip, an unreadable version) arrive as thrown boundary state, the SEMANTIC faults (a viewpoint
    // declaring two cameras, a reference declaring neither a document nor a URL) arrive already typed and
    // accumulated, and one funnel lands both without discarding the captured error.
    public static Fin<BcfFile> Read(ReadOnlyMemory<byte> bcfzip, Op key) =>
        key.Catch(() => Decode(bcfzip, key));

    public static Fin<byte[]> Write(BcfFile file, Op key) =>
        key.Catch(() => Encode(file, key));

    // The generation sniff is the ONE fork and it rails: an unreadable version is a refusal with its own subject,
    // never a thrown sentinel the outer funnel has to re-classify.
    static Fin<BcfFile> Decode(ReadOnlyMemory<byte> bcfzip, Op key) {
        byte[] bytes = bcfzip.ToArray();
        return Optional(BcfExtensions.GetVersionFromStreamArchive(Fresh(bytes)).GetAwaiter().GetResult()).Match(
            Some: source => source is BcfVersionEnum.Bcf30 ? Native(bytes, key) : Converted(bytes, key),
            None: () => Fin.Fail<BcfFile>(new BimFault.Refused(key, BimScope.Review, BimReason.Rejected, string.Join(':', new object?[] { "bcf-archive", "version", "unreadable" }))));
    }

    // EXEMPTION: the toolkit's per-part parse surface is async and stream-owning, so both ingress bodies are the
    // named platform-forced statement seam — the sequential streams are statements by the foreign API's shape and
    // the whole body lands on ONE Fin.
    static Fin<BcfFile> Native(byte[] bytes, Op key) {
        ConcurrentBag<Bcf30.Markup> markups = BcfExtensions.ParseMarkups<Bcf30.Markup, Bcf30.VisualizationInfo>(Fresh(bytes)).GetAwaiter().GetResult();
        Bcf30.Extensions vocabulary = BcfExtensions.ParseExtensions<Bcf30.Extensions>(Fresh(bytes)).GetAwaiter().GetResult();
        Bcf30.ProjectInfo? project = BcfExtensions.ParseProject<Bcf30.ProjectInfo>(Fresh(bytes)).GetAwaiter().GetResult();
        Bcf30.DocumentInfo? documents = BcfExtensions.ParseDocuments<Bcf30.DocumentInfo>(Fresh(bytes)).GetAwaiter().GetResult();
        return FileOf(toSeq(markups), vocabulary, project, documents, BcfGeneration.Bcf30, key)
            .Map(native => native with { Blobs = BlobsOf(bytes, native.Topics) });
    }

    // Worker.BcfFromStream up-converts a 2.1 source; its `using BcfToolkit.Model.Bcf30;` binds the generic
    // target, so the projection stays single-version off the concrete Bcf30 graph.
    static Fin<BcfFile> Converted(byte[] bytes, Op key) {
        Bcf30.Bcf bcf = new Worker().BcfFromStream(Fresh(bytes)).GetAwaiter().GetResult();
        return FileOf(toSeq(bcf.Markups), bcf.Extensions, bcf.Project, bcf.Document, BcfGeneration.Bcf21, key)
            .Map(converted => converted with { Blobs = BlobsOf(bytes, converted.Topics) });
    }

    // Bitmap payloads are CONTAINER PARTS the toolkit model never carries, so the codec lifts each held
    // `{topicGuid}/{Reference}` part into the file's blob store; a part the container does not hold reads as
    // absent. The topic-folder path scheme holds at both source generations.
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

    // Each per-part parse opens its own ZipArchive over an independent MemoryStream, so a parse helper that
    // disposes its source never strands a sibling read.
    static MemoryStream Fresh(byte[] bytes) => new(bytes, writable: false);

    // The container's topics admit APPLICATIVELY: malformed columns across topics are independent facts, so one
    // read reports every bad camera and every mis-declared reference at once — a fail-fast fold over a federated
    // container makes an operator re-run the read once per defect. Landed re-selects the typed BimFault members
    // back out of the flattened accumulation. ParseMarkups yields a ConcurrentBag, so the fold re-orders topics
    // deterministically (schema Index, then Guid) and a re-read of the same archive is byte-stable.
    static Fin<BcfFile> FileOf(Seq<Bcf30.Markup> markups, Bcf30.Extensions? vocabulary, Bcf30.ProjectInfo? project, Bcf30.DocumentInfo? documents, BcfGeneration source, Op key) =>
        (markups.Traverse(markup => TopicOf(markup, key)).As().Map(topics =>
            new BcfFile(
                toSeq(topics.OrderBy(static t => t.Index.IfNone(int.MaxValue)).ThenBy(static t => t.Guid, StringComparer.Ordinal)),
                Optional(project?.Project).Map(static p => new BcfProject(BcfEdge.Word(p.ProjectId), BcfEdge.Word(p.Name))),
                Optional(vocabulary).Map(static x => new BcfVocabulary(
                    BcfEdge.Rows(x.TopicTypes), BcfEdge.Rows(x.TopicStatuses), BcfEdge.Rows(x.Priorities), BcfEdge.Rows(x.TopicLabels),
                    BcfEdge.Rows(x.Users), BcfEdge.Rows(x.SnippetTypes), BcfEdge.Rows(x.Stages))),
                BcfEdge.Rows(documents?.Documents).Map(static row => new BcfDocument(
                    row.Guid, BcfEdge.Word(row.Filename), BcfEdge.Word(row.Description),
                    BcfEdge.Base64Of(row.DocumentData).IfNone(ReadOnlyMemory<byte>.Empty))),
                source))).ToFin();

    // The comments and viewpoints ride `Topic`, not `Markup` (a Bcf30.Markup carries only Header+Topic); the
    // header IFC-file refs ride markup.Header. The two fallible column families accumulate side by side.
    static Validation<Error, BcfTopic> TopicOf(Bcf30.Markup markup, Op key) {
        Bcf30.Topic topic = markup.Topic;
        return (BcfEdge.Rows(topic.Viewpoints).Traverse(reference => ViewpointOf(reference, key)).As(),
                BcfEdge.Rows(topic.DocumentReferences).Traverse(reference => ReferenceOf(reference, key)).As())
            .Apply((viewpoints, references) => new BcfTopic(
                topic.Guid, BcfEdge.Word(topic.Title), BcfLifecycle.Elect(topic.TopicStatus),
                BcfEdge.Word(topic.TopicType), BcfEdge.Word(topic.Priority), BcfEdge.Word(topic.CreationAuthor),
                BcfEdge.At(topic.CreationDate),
                BcfEdge.Rows(topic.Comments).Map(CommentOf),
                viewpoints,
                BcfEdge.Word(topic.Description), BcfEdge.Word(topic.AssignedTo), BcfEdge.Word(topic.Stage),
                BcfEdge.AtOf(topic.DueDate), BcfEdge.Rows(topic.Labels),
                Optional(topic.Index), BcfEdge.AtOf(topic.ModifiedDate), BcfEdge.Word(topic.ModifiedAuthor),
                BcfEdge.Word(topic.ServerAssignedId),
                BcfEdge.Rows(topic.ReferenceLinks),
                BcfEdge.Rows(topic.RelatedTopics).Map(static r => r.Guid),
                references,
                Optional(topic.BimSnippet).Map(static s => new BcfBimSnippet(
                    BcfEdge.Word(s.SnippetType), BcfEdge.Word(s.Reference), BcfEdge.Word(s.ReferenceSchema), s.IsExternal)),
                BcfEdge.Rows(markup.Header?.Files).Map(static f => new BcfHeaderFile(
                    BcfEdge.Word(f.Filename), BcfEdge.AtOf(f.Date), BcfEdge.Word(f.Reference),
                    BcfEdge.Word(f.IfcProject), BcfEdge.Word(f.IfcSpatialStructureElement), f.IsExternal)),
                BcfEdge.Word(topic.TopicStatus)))
            .As();
    }

    // BCF declares a document reference as DocumentGuid XOR Url — an internal library document or an external
    // link, never both and never neither. The prior read carried both as bare Options and let an ambiguous
    // reference through to a write that emitted it.
    static Validation<Error, BcfDocumentReference> ReferenceOf(Bcf30.DocumentReference reference, Op key) =>
        (BcfEdge.OptionText(reference.DocumentGuid), BcfEdge.OptionText(reference.Url)) switch {
            ({ IsSome: true }, { IsSome: true }) or ({ IsNone: true }, { IsNone: true }) =>
                Validation<Error, BcfDocumentReference>.Fail(new BimFault.Refused(key, BimScope.Review, BimReason.Rejected, string.Join(':', new object?[] { "bcf-archive", "document-reference", reference.Guid }))),
            var (document, url) => Validation<Error, BcfDocumentReference>.Success(
                new BcfDocumentReference(reference.Guid, document, url, BcfEdge.Word(reference.Description))),
        };

    static BcfComment CommentOf(Bcf30.Comment comment) =>
        new(comment.Guid, BcfEdge.Word(comment.Author), BcfEdge.Word(comment.CommentProperty),
            Optional(comment.Viewpoint?.Guid), BcfEdge.At(comment.Date), BcfEdge.AtOf(comment.ModifiedDate),
            BcfEdge.Word(comment.ModifiedAuthor), None);

    static Validation<Error, BcfViewpoint> ViewpointOf(Bcf30.ViewPoint reference, Op key) {
        Bcf30.VisualizationInfo? visualization = reference.VisualizationInfo;
        Bcf30.Components? components = visualization?.Components;
        return (CameraOf(visualization, reference.Guid, key)).ToValidation().Map(camera =>
            new BcfViewpoint(
                reference.Guid, camera,
                BcfEdge.Rows(components?.Selection).Map(static c => c.IfcGuid),
                BcfVisibility.Of(
                    BcfEdge.Flag(components?.Visibility?.DefaultVisibility),
                    BcfEdge.Rows(components?.Visibility?.Exceptions).Map(static c => c.IfcGuid)),
                BcfEdge.Base64Of(reference.SnapshotData),
                BcfEdge.HintsOf(components?.Visibility?.ViewSetupHints),
                BcfEdge.Rows(components?.Coloring).Map(static row => new BcfColoring(
                    BcfEdge.Word(row.Color), BcfEdge.Rows(row.Components).Map(static m => m.IfcGuid))),
                BcfEdge.Rows(visualization?.Lines).Map(static l => new BcfLine(BcfEdge.Vec(l.StartPoint), BcfEdge.Vec(l.EndPoint))),
                BcfEdge.Rows(visualization?.ClippingPlanes).Map(static p => new BcfClippingPlane(BcfEdge.Vec(p.Location), BcfEdge.Vec(p.Direction))),
                BcfEdge.Rows(visualization?.Bitmaps).Map(static b => new BcfBitmap(
                    b.Format.ToString(), BcfEdge.Word(b.Reference), BcfEdge.Vec(b.Location), BcfEdge.Vec(b.Normal), BcfEdge.Vec(b.Up), b.Height)),
                Optional(reference.Index)));
    }

    // The archive arm's half of the ONE camera admission: transcribe each declared toolkit camera onto its typed
    // case, then hand BOTH to BcfCamera.Admit, which owns the XOR law and the typed absence.
    static Fin<Option<BcfCamera>> CameraOf(Bcf30.VisualizationInfo? visualization, string viewpoint, Op key) =>
        BcfCamera.Admit(
            Optional(visualization?.PerspectiveCamera).Map(static p => new BcfCamera.Perspective(
                BcfEdge.Vec(p.CameraViewPoint), BcfEdge.Vec(p.CameraDirection), BcfEdge.Vec(p.CameraUpVector), p.FieldOfView, p.AspectRatio)),
            Optional(visualization?.OrthogonalCamera).Map(static o => new BcfCamera.Orthogonal(
                BcfEdge.Vec(o.CameraViewPoint), BcfEdge.Vec(o.CameraDirection), BcfEdge.Vec(o.CameraUpVector), o.ViewToWorldScale, o.AspectRatio)),
            viewpoint, key);

    // EXEMPTION: the toolkit's fluent builder is MUTATION-shaped — every Set*/Add* returns the same instance and
    // the nested authors are Action<TBuilder> void callbacks — so the whole author is the named platform-forced
    // statement seam. The conditional Set* guards are the builder's own presence contract (an empty string writes
    // an empty element), never domain branching.
    static Fin<byte[]> Encode(BcfFile file, Op key) {
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
        return WithBitmapParts(sink, file, key);
    }

    // Write's half of the container-part gap: every BcfBitmap.Reference must leave as a HELD part, so the codec
    // RESOLVES the whole part set from the blob store before the container is opened and refuses typed — a
    // written archive whose markup references a part the container never holds is unrepresentable, and the
    // refusal never leaves a half-written zip behind.
    static Fin<byte[]> WithBitmapParts(MemoryStream container, BcfFile file, Op key) =>
        file.Topics
            .Bind(topic => topic.Viewpoints.Bind(static v => v.Bitmaps).Map(static b => b.Reference).Distinct()
                .Map(reference => (Topic: topic.Guid, Reference: reference)))
            .Traverse(part => file.Blobs.Find(part.Reference).Match(
                Some: data => Fin.Succ((part.Topic, part.Reference, Data: data)),
                None: () => Fin.Fail<(string Topic, string Reference, ReadOnlyMemory<byte> Data)>(new BimFault.Refused(key, BimScope.Review, BimReason.Rejected, string.Join(':', new object?[] { "bcf-archive", "bitmap", part.Reference })))))
            .As()
            .Map(parts => Parts(container, parts));

    static byte[] Parts(MemoryStream container, Seq<(string Topic, string Reference, ReadOnlyMemory<byte> Data)> parts) {
        using (ZipArchive zip = new(container, ZipArchiveMode.Update, leaveOpen: true)) {
            parts.Iter(part => {
                using Stream entry = zip.CreateEntry($"{part.Topic}/{part.Reference}").Open();
                entry.Write(part.Data.Span);
            });
        }
        return container.ToArray();
    }

    // The viewpoint author re-emits the FULL VisualizationInfo, so a write round-trips exactly what the read
    // captured; a Guid-only author is the hollow form this fold retires.
    static void AuthorViewpoint(ViewPointBuilder vp, BcfViewpoint viewpoint) {
        vp.SetGuid(viewpoint.Guid);
        viewpoint.Index.IfSome(i => vp.SetIndex(i));
        viewpoint.Snapshot.IfSome(bytes => vp.SetSnapshotData(new FileData { Mime = "image/png", Data = Convert.ToBase64String(bytes.Span) }));
        vp.SetVisualizationInfo(info => {
            // A selection-only viewpoint writes NO camera element: the domain absence is the archive absence, so
            // no degenerate origin camera is authored.
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
            // The set projects back attribute by attribute, so the schema's three columns leave exactly as they
            // arrived and an absent hints element stays absent.
            if (viewpoint.Visibility.Default || !viewpoint.Visibility.Exceptions.IsEmpty || viewpoint.Hints.IsSome) {
                info.SetVisibility(visibility => {
                    visibility.SetDefaultVisibility(viewpoint.Visibility.Default);
                    viewpoint.Hints.IfSome(held => visibility.SetViewSetupHints(hints => hints
                        .SetSpaceVisible(held.Admits(BcfViewHint.Spaces))
                        .SetSpaceBoundariesVisible(held.Admits(BcfViewHint.SpaceBoundaries))
                        .SetOpeningVisible(held.Admits(BcfViewHint.Openings))));
                    viewpoint.Visibility.Exceptions.Iter(id => visibility.AddException(component => component.SetIfcGuid(id)));
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

- Owner: `BcfProjection` projects the archive topic family directly onto generated `Bcf.V1` messages; `BcfLifecycle` elects the generated status enum from the open BCF vocabulary; `BcfApi` owns the BCF-API resource union and its snake_case body register.
- Entry: `BcfProjection.Project(Seq<BcfTopic>)` returns one generated `BcfTopicWire` per topic. Transport selects binary framing or ProtoJSON rendering for each message; the producer introduces no collection envelope or serializer dialect. `BcfApi.Project` and `BcfApi.Open` remain the request and response halves over the API body register.
- Auto: Mapperly fills generated message members and their `RepeatedField<T>` collections. Custom mappings cover only carrier laws the generator cannot infer: protobuf oneofs, proto3 optional scalars and strings, `Vector3` to `Point3` or `UnitDirection3`, `Instant` to `Timestamp`, and the bitmap enum.
- Receipt: the generated `BcfTopicWire` and nested `BcfViewpointWire` values are the BCF cross-runtime contract; viewpoints preserve IFC GlobalIds so TS selection and C# coordination share one identity.
- Packages: Rasm.Contracts (`Bcf.V1` messages), Google.Protobuf (`RepeatedField<T>`, `Timestamp`, binary framing and ProtoJSON), Riok.Mapperly, Thinktecture.Runtime.Extensions, NodaTime, NodaTime.Serialization.SystemTextJson, LanguageExt.Core, `Rasm.Domain` (`Op`, `RedrivePolicy`), BCL `System.Text.Json` + `System.Collections.Immutable` + `System.Collections.Frozen`
- Growth: a contract column lands in protobuf and regeneration breaks the mapper until the domain projection answers it; a BCF-API operation lands as one `BcfResource` case answered by both dispatch halves; an OData axis lands on `BcfQuery`.
- Boundary: generated messages own the cross-runtime shape and serialization. `BcfProjection` owns domain conversion only, and transport owns framing or ProtoJSON rendering per message. `BcfApiContext` remains the sole snake_case JSON dialect because BCF-API resources are publisher-defined JSON rather than the estate protobuf wire. `BcfLifecycle` maps the open archive/API token onto the generated status enum while `StatusLabel` preserves the original token. `BcfApiVerb` carries re-drive policy as row data, and Compute executes the transport.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Protobuf.WellKnownTypes;
using LanguageExt;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using Rasm.Bim.Model;
using Rasm.Contracts.Bcf.V1;
using Rasm.Domain;
using Riok.Mapperly.Abstractions;
using Thinktecture;
using Op = Rasm.Domain.Op;
using Vector3 = System.Numerics.Vector3;
using static LanguageExt.Prelude;

namespace Rasm.Bim.Coordination;

// --- [TYPES] ------------------------------------------------------------------------------
// The verb vocabulary carrying its OWN re-drive budget: idempotency is a property of the VERB the spec defines,
// so the budget is a roster cell no dispatch arm re-decides, and the key IS the HTTP method the transport issues.
// Idempotent is declared FIRST because the rows read it at static init.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BcfApiVerb {
    static readonly RedrivePolicy Idempotent = RedrivePolicy.Of(
        law: Schedule.exponential(Duration.FromMilliseconds(500)) | Schedule.maxDelay(Duration.FromSeconds(8)), bound: 4);

    public static readonly BcfApiVerb Get    = new(key: "GET", redrive: Idempotent);
    // A re-driven create mints a duplicate topic or comment the CDE cannot merge, so the one non-idempotent verb
    // declares no budget and the transport's single pass IS the whole attempt.
    public static readonly BcfApiVerb Post   = new(key: "POST", redrive: RedrivePolicy.None);
    public static readonly BcfApiVerb Put    = new(key: "PUT", redrive: Idempotent);
    public static readonly BcfApiVerb Delete = new(key: "DELETE", redrive: Idempotent);

    public RedrivePolicy Redrive { get; }
}

// --- [BODIES]
// The BCF-API 3.0 resource-body register — ONE snake_case dialect serving BOTH directions: the WRITE bodies and
// the GET READ shapes, which per spec are the SAME resources plus the server-assigned provenance columns, carried
// NULLABLE below so the write serializer's WhenWritingNull omission keeps every POST/PUT body spec-conformant
// while a GET response decodes whole. The camera is orthogonal_camera XOR perspective_camera and the snapshot
// rides inline {snapshot_type, snapshot_data} base64.
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
    string? TopicGuid = null, string? ReplyToCommentGuid = null, string? Author = null, Instant? Date = null,
    string? ModifiedAuthor = null, Instant? ModifiedDate = null);
public sealed record BcfApiRelatedTopicBody(string RelatedTopicGuid);
public sealed record BcfApiDocumentReferenceBody(string Guid, string? DocumentGuid, string? Url, string Description);
public sealed record BcfApiFileBody(string IfcProject, string IfcSpatialStructureElement, string Filename, Instant? Date, string Reference);
// The three READ-ONLY response bodies no write arm feeds: the document register row (its bytes ride the single
// document GET as a raw stream), the project header, and the extensions vocabulary BcfVocabulary transcribes 1:1.
public sealed record BcfApiDocumentBody(string Guid, string Filename, string? Description = null);
public sealed record BcfApiProjectBody(string ProjectId, string Name);
public sealed record BcfApiExtensionsBody(
    ImmutableArray<string> TopicType, ImmutableArray<string> TopicStatus, ImmutableArray<string> Priority,
    ImmutableArray<string> TopicLabel, ImmutableArray<string> UserIdType, ImmutableArray<string> SnippetType,
    ImmutableArray<string> Stage);

// --- [OPERATIONS] ---------------------------------------------------------------------------
// The ONE status election both lanes take, a FrozenDictionary roster over the enum's own rows under an
// ordinal-ignore-case comparer. Space and hyphen are the only separators the BCF vocabulary spells ("In
// Progress", "in-progress"), so one separator-free projection is the key and the squeeze allocation is paid only
// when a token carries one. An unrecognized token elects Open and its verbatim form rides StatusLabel.
public static class BcfLifecycle {
    static readonly FrozenDictionary<string, BcfStatus> Rows =
        Enum.GetValues<BcfStatus>()
            .Where(static row => row is not BcfStatus.Unspecified)
            .ToFrozenDictionary(static row => row.ToString(), static row => row, StringComparer.OrdinalIgnoreCase);

    public static BcfStatus Elect(string? token) =>
        Rows.TryGetValue(Squeezed(BcfEdge.Word(token)), out BcfStatus status) ? status : BcfStatus.Open;

    static string Squeezed(string token) =>
        token.AsSpan().ContainsAny(' ', '-') ? new string(token.Where(static c => c is not (' ' or '-')).ToArray()) : token;
}

// Generated protobuf messages own the wire. Mapperly fills their ordinary members and get-only repeated fields;
// the named user mappings below cover only oneofs, proto3 optional scalars/strings, and domain carrier crossings.
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Both)]
[UseStaticMapper(typeof(BcfEdge))]
public static partial class BcfProjection {
    [MapperIgnoreSource(nameof(BcfViewpoint.Snapshot))]
    [MapperIgnoreSource(nameof(BcfViewpoint.Index))]
    [MapperIgnoreTarget(nameof(BcfViewpointWire.Index))]
    [MapProperty(nameof(BcfViewpoint.Hints), nameof(BcfViewpointWire.ViewSetupHints))]
    static partial BcfViewpointWire Viewpoint(BcfViewpoint source);

    [MapperIgnoreSource(nameof(BcfTopic.StatusToken))]
    [MapperIgnoreSource(nameof(BcfTopic.Index))]
    [MapperIgnoreTarget(nameof(BcfTopicWire.Index))]
    static partial BcfTopicWire Topic(BcfTopic source);

    [MapperIgnoreSource(nameof(BcfComment.ViewpointGuid))]
    [MapperIgnoreSource(nameof(BcfComment.ReplyToGuid))]
    [MapperIgnoreTarget(nameof(BcfCommentWire.ViewpointGuid))]
    [MapperIgnoreTarget(nameof(BcfCommentWire.ReplyToGuid))]
    static partial BcfCommentWire Comment(BcfComment source);

    [MapperIgnoreSource(nameof(BcfDocumentReference.DocumentGuid))]
    [MapperIgnoreSource(nameof(BcfDocumentReference.Url))]
    [MapperIgnoreTarget(nameof(BcfDocumentWire.DocumentGuid))]
    [MapperIgnoreTarget(nameof(BcfDocumentWire.Url))]
    static partial BcfDocumentWire Document(BcfDocumentReference source);

    static partial BcfFileWire File(BcfHeaderFile source);
    static partial BcfColoringWire Coloring(BcfColoring source);
    static partial BcfLineWire Line(BcfLine source);
    static partial BcfClippingWire Clipping(BcfClippingPlane source);
    static partial BcfBitmapWire Bitmap(BcfBitmap source);
    static partial BcfSnippetWire Snippet(BcfBimSnippet source);

    public static Seq<BcfTopicWire> Project(Seq<BcfTopic> topics) =>
        topics.Map(static topic => Project(topic));

    [UserMapping(Default = true)]
    public static BcfTopicWire Project(BcfTopic source) {
        BcfTopicWire wire = Topic(source);
        source.Index.IfSome(value => wire.Index = checked((uint)value));
        return wire;
    }

    [UserMapping(Default = true)]
    public static BcfViewpointWire Project(BcfViewpoint source) {
        BcfViewpointWire wire = Viewpoint(source);
        source.Index.IfSome(value => wire.Index = checked((uint)value));
        return wire;
    }

    [UserMapping(Default = true)]
    public static BcfCommentWire Project(BcfComment source) {
        BcfCommentWire wire = Comment(source);
        source.ViewpointGuid.IfSome(value => wire.ViewpointGuid = value);
        source.ReplyToGuid.IfSome(value => wire.ReplyToGuid = value);
        return wire;
    }

    [UserMapping(Default = true)]
    public static BcfDocumentWire Project(BcfDocumentReference source) {
        BcfDocumentWire wire = Document(source);
        source.DocumentGuid.IfSome(value => wire.DocumentGuid = value);
        source.Url.IfSome(value => wire.Url = value);
        return wire;
    }

    [UserMapping] static BcfFileWire Project(BcfHeaderFile source) => File(source);
    [UserMapping] static BcfColoringWire Project(BcfColoring source) => Coloring(source);
    [UserMapping] static BcfLineWire Project(BcfLine source) => Line(source);
    [UserMapping] static BcfClippingWire Project(BcfClippingPlane source) => Clipping(source);
    [UserMapping] static BcfBitmapWire Project(BcfBitmap source) => Bitmap(source);
    [UserMapping] static BcfSnippetWire Project(BcfBimSnippet source) => Snippet(source);

    [UserMapping]
    static BcfCameraWire? Lens(Option<BcfCamera> camera) =>
        camera.Match<BcfCameraWire?>(static value => Project(value), static () => null);

    static BcfCameraWire Project(BcfCamera camera) => camera.Switch(
        perspective: static value => new BcfCameraWire {
            Position = Point(value.Position), Direction = Direction(value.Direction), Up = Direction(value.Up),
            AspectRatio = value.AspectRatio, FieldOfViewDeg = value.FieldOfViewDeg,
        },
        orthogonal: static value => new BcfCameraWire {
            Position = Point(value.Position), Direction = Direction(value.Direction), Up = Direction(value.Up),
            AspectRatio = value.AspectRatio, ViewToWorldScale = value.ViewToWorldScale,
        });

    [UserMapping]
    static BcfVisibilityWire Project(BcfVisibility visibility) => visibility.Switch(
        showing: static value => Showing(value.Hidden),
        hiding: static value => Hiding(value.Visible));

    static BcfVisibilityWire Showing(Seq<string> hidden) {
        BcfShowingWire value = new();
        value.Hidden.Add(hidden);
        return new BcfVisibilityWire { Showing = value };
    }

    static BcfVisibilityWire Hiding(Seq<string> visible) {
        BcfHidingWire value = new();
        value.Visible.Add(visible);
        return new BcfVisibilityWire { Hiding = value };
    }

    [UserMapping]
    static BcfHintsWire? Hints(Option<CapabilitySet<BcfViewHint>> held) =>
        held.Match<BcfHintsWire?>(
            static set => new() {
                SpacesVisible = set.Admits(BcfViewHint.Spaces),
                SpaceBoundariesVisible = set.Admits(BcfViewHint.SpaceBoundaries),
                OpeningsVisible = set.Admits(BcfViewHint.Openings),
            },
            static () => null);

    [UserMapping] static Rasm.Contracts.Spatial.V1.Point3 Point(Vector3 value) => new() { XM = value.X, YM = value.Y, ZM = value.Z };
    [UserMapping] static Rasm.Contracts.Spatial.V1.UnitDirection3 Direction(Vector3 value) => new() { X = value.X, Y = value.Y, Z = value.Z };
    [UserMapping] static Timestamp Stamp(Instant value) => Timestamp.FromDateTime(value.ToDateTimeUtc());
    [UserMapping] static Timestamp? Stamp(Option<Instant> value) => value.Match<Timestamp?>(Stamp, static () => null);
    [UserMapping] static BcfSnippetWire? Snippet(Option<BcfBimSnippet> value) =>
        value.Match<BcfSnippetWire?>(static snippet => Project(snippet), static () => null);
    [UserMapping] static BcfBitmapFormat Format(string value) => value.ToUpperInvariant() switch {
        "PNG" => BcfBitmapFormat.Png,
        "JPG" or "JPEG" => BcfBitmapFormat.Jpg,
        _ => BcfBitmapFormat.Unspecified,
    };

    static string? Text(Option<string> value) => value.Match<string?>(static v => v, static () => null);
    static Instant? Moment(Option<Instant> value) => value.Match<Instant?>(static i => i, static () => null);
    static int? Ordinal(Option<int> value) => value.Match<int?>(static i => i, static () => null);

    // The BCF-API write-body direction: Target-required only, because the spec bodies DROP the server-assigned
    // provenance and sub-resource collections BY LAW — a new body column still diagnoses loudly while the
    // deliberate drops stay quiet. TopicStatus reads the one StatusToken election.
    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(BcfTopic.StatusToken), nameof(BcfApiTopicBody.TopicStatus))]
    public static partial BcfApiTopicBody ToBody(BcfTopic topic);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(BcfComment.Text), nameof(BcfApiCommentBody.Comment))]
    [MapProperty(nameof(BcfComment.ReplyToGuid), nameof(BcfApiCommentBody.ReplyToCommentGuid))]
    public static partial BcfApiCommentBody ToBody(BcfComment comment);

    public static partial BcfApiSnippetBody ToBody(BcfBimSnippet snippet);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public static partial BcfApiFileBody ToBody(BcfHeaderFile file);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public static partial BcfApiDocumentReferenceBody ToBody(BcfDocumentReference reference);

    [UserMapping] static BcfApiSnippetBody? ApiSnippet(Option<BcfBimSnippet> value) =>
        value.Match<BcfApiSnippetBody?>(static s => ToBody(s), static () => null);

    // The viewpoint POST body: the camera union splits onto the spec's orthogonal XOR perspective slots, the
    // snapshot rides inline base64, and selection/coloring/visibility nest under components. Bitmaps stay
    // archive-only — the domain carries a Reference, the REST form demands inline bytes this owner does not hold.
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
                viewpoint.Visibility.Default,
                viewpoint.Hints.Match<BcfApiViewSetupHintsBody?>(
                    static held => new(held.Admits(BcfViewHint.Spaces), held.Admits(BcfViewHint.SpaceBoundaries), held.Admits(BcfViewHint.Openings)),
                    static () => null),
                [.. viewpoint.Visibility.Exceptions.Map(static id => new BcfApiComponentBody(id))])));

    static BcfApiPointBody Point(Vector3 v) => new(v.X, v.Y, v.Z);

    // The READ-lane wire-to-domain inverse over the SAME register — BcfApi.Admit's admitted body lands the typed
    // domain record here. The comment map GENERATES (its columns are 1:1 through BcfEdge's carriers; the
    // topic/reply joins are collection-navigation facts the caller resolves); the topic fold is HAND-COMPOSED
    // because a GET topic carries no sub-resource collections — Comments and Viewpoints land from their OWN reads.
    public static BcfTopic ToDomain(BcfApiTopicBody body) => new(
        body.Guid, body.Title, BcfLifecycle.Elect(body.TopicStatus), body.TopicType, body.Priority,
        BcfEdge.Word(body.CreationAuthor), BcfEdge.Stamped(body.CreationDate),
        Seq<BcfComment>(), Seq<BcfViewpoint>(),
        body.Description, body.AssignedTo, body.Stage, BcfEdge.OptionMoment(body.DueDate),
        toSeq(body.Labels), BcfEdge.OptionOrdinal(body.Index), BcfEdge.OptionMoment(body.ModifiedDate),
        BcfEdge.Word(body.ModifiedAuthor), BcfEdge.Word(body.ServerAssignedId), toSeq(body.ReferenceLinks),
        BimSnippet: Optional(body.BimSnippet).Map(static s => new BcfBimSnippet(s.SnippetType, s.Reference, s.ReferenceSchema, s.IsExternal)),
        StatusLabel: body.TopicStatus);

    // TopicGuid is a collection-navigation fact the caller already holds (it addressed the resource), so it stays
    // ignored; ReplyToCommentGuid is real conversation structure and maps BOTH ways.
    [MapperIgnoreSource(nameof(BcfApiCommentBody.TopicGuid))]
    [MapProperty(nameof(BcfApiCommentBody.Comment), nameof(BcfComment.Text))]
    [MapProperty(nameof(BcfApiCommentBody.ReplyToCommentGuid), nameof(BcfComment.ReplyToGuid))]
    public static partial BcfComment ToDomain(BcfApiCommentBody body);

    // The viewpoint READ inverse — HAND-COMPOSED like its ToBody twin because the camera is a spec XOR the union
    // re-discriminates: exactly one camera slot admits, zero or two is a malformed body faulting typed rather
    // than a guessed projection. Components unpack onto the domain carriers through BcfEdge, the inline base64
    // snapshot decodes to bytes, and bitmaps stay archive-only exactly as the write side declares.
    public static Fin<BcfViewpoint> ToDomain(BcfApiViewpointBody body, Op key) =>
        BcfCamera.Admit(
            Optional(body.PerspectiveCamera).Map(static p => new BcfCamera.Perspective(
                BcfEdge.Vec(p.CameraViewPoint), BcfEdge.Vec(p.CameraDirection), BcfEdge.Vec(p.CameraUpVector), p.FieldOfView, p.AspectRatio)),
            Optional(body.OrthogonalCamera).Map(static o => new BcfCamera.Orthogonal(
                BcfEdge.Vec(o.CameraViewPoint), BcfEdge.Vec(o.CameraDirection), BcfEdge.Vec(o.CameraUpVector), o.ViewToWorldScale, o.AspectRatio)),
            body.Guid, key)
        .Map(camera => {
            BcfApiComponentsBody parts = BcfEdge.Parts(body.Components);
            return new BcfViewpoint(
                body.Guid,
                camera,
                toSeq(parts.Selection).Map(static component => component.IfcGuid),
                BcfVisibility.Of(parts.Visibility.DefaultVisibility, toSeq(parts.Visibility.Exceptions).Map(static component => component.IfcGuid)),
                Optional(body.Snapshot).Map(static snapshot => (ReadOnlyMemory<byte>)Convert.FromBase64String(snapshot.SnapshotData)),
                BcfEdge.HintsOf(parts.Visibility.ViewSetupHints),
                toSeq(parts.Coloring).Map(static row => new BcfColoring(row.Color, toSeq(row.Components).Map(static m => m.IfcGuid))),
                toSeq(body.Lines).Map(static line => new BcfLine(BcfEdge.Vec(line.StartPoint), BcfEdge.Vec(line.EndPoint))),
                toSeq(body.ClippingPlanes).Map(static plane => new BcfClippingPlane(BcfEdge.Vec(plane.Location), BcfEdge.Vec(plane.Direction))),
                Seq<BcfBitmap>(),
                BcfEdge.OptionOrdinal(body.Index));
        });

    // The three mechanical READ inverses beside the viewpoint pair — nullable-to-Option and array-to-Seq lifts
    // over 1:1 columns, spelled once so no response arm hand-transcribes.
    public static BcfDocumentReference ToDomain(BcfApiDocumentReferenceBody body) =>
        new(body.Guid, Optional(body.DocumentGuid), Optional(body.Url), body.Description);

    public static BcfHeaderFile ToDomain(BcfApiFileBody body) =>
        new(body.Filename, Optional(body.Date), body.Reference, body.IfcProject, body.IfcSpatialStructureElement);

    public static BcfVocabulary ToDomain(BcfApiExtensionsBody body) =>
        new(toSeq(body.TopicType), toSeq(body.TopicStatus), toSeq(body.Priority), toSeq(body.TopicLabel),
            toSeq(body.UserIdType), toSeq(body.SnippetType), toSeq(body.Stage));
}

// The BCF-API 3.0 topic-resource request the polymorphic `Project` discriminates on: each case is one resource
// operation the buildingSMART spec defines over the topic graph, COMPLETE over the project-scoped conversation —
// topic, comment, and viewpoint lifecycles plus the SNAPSHOT read (viewpoints are immutable per spec, so no
// revise case exists BY LAW), related topics, document references, header files, the document library, the
// project, and the EXTENSIONS vocabulary — so a CDE conversation never needs a resource this union cannot spell.
// The server-side events change-feed is the `Rasm.Persistence/Version/ledger` CDE-sync concern. Read cases ride
// `Option` for GET arity; never a `Get`/`Post` member family beside `Project`.
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

// The REST RESOURCE descriptor the Compute transport issues: Bim owns the resource MODEL — the verb (carrying its
// own re-drive budget), the resource path, and the spec-dialect body — while the HTTP transport (client, auth,
// the execution of the curve) stays the `Rasm.Compute/Runtime/channels#TRANSPORT_AXIS` owner's. Transport-NEUTRAL
// by construction: no Compute type crosses into it.
public sealed record BcfApiRequest(BcfApiVerb Verb, string Resource, ReadOnlyMemory<byte> Body);

// The OData query axis the spec's collection GETs admit and the lane's paging CURSOR: `Render` is the ONE
// query-string mint every collection route takes (the document-upload `filename` parameter is a spec-named
// binary-POST axis, not OData), `Next` advances by the rows a page actually returned, and exhaustion is the
// caller's `received < Top` read — the spec answers bare arrays with no paging envelope, so the cursor lives on
// the REQUEST and never on a fabricated response wrapper.
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
// case their collection read answers, so arity never mints a sibling.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BcfOutcome {
    private BcfOutcome() { }

    public sealed record Topics(Seq<BcfTopic> Rows) : BcfOutcome;
    public sealed record Comments(Seq<BcfComment> Rows) : BcfOutcome;
    public sealed record Viewpoints(Seq<BcfViewpoint> Rows) : BcfOutcome;
    // ONE raw-bytes case for both binary GETs — the viewpoint snapshot and the single-document fetch carry
    // identical payload semantics, so a per-endpoint byte case is the merged form.
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
// ONE polymorphic `Project` over the resource union, discriminating on the input shape via the generated TOTAL
// `Switch` — the verb DERIVES from the case (POST a create, PUT a revision, GET a read, DELETE a retire; GET and
// DELETE carry no body) and brings its own re-drive budget, every write arm feeding its spec body through the ONE
// `Write` kernel, the document upload carrying raw bytes with the filename on the query string per the spec's
// binary-body POST.
public static class BcfApi {
    public static Fin<BcfApiRequest> Project(string projectId, BcfResource resource, Op key) =>
        resource.Switch(
            state:                   (key, project: $"bcf/3.0/projects/{projectId}", topics: $"bcf/3.0/projects/{projectId}/topics"),
            createTopic:             static (s, r) => Write(BcfApiVerb.Post, s.topics, BcfProjection.ToBody(r.Topic), s.key),
            reviseTopic:             static (s, r) => Write(BcfApiVerb.Put, $"{s.topics}/{r.Topic.Guid}", BcfProjection.ToBody(r.Topic), s.key),
            readTopic:               static (s, r) => Fin.Succ(new BcfApiRequest(BcfApiVerb.Get, r.Guid.Match(Some: g => $"{s.topics}/{g}", None: () => r.Query.IfNone(BcfQuery.All).Render(s.topics)), default)),
            retireTopic:             static (s, r) => Fin.Succ(new BcfApiRequest(BcfApiVerb.Delete, $"{s.topics}/{r.Guid}", default)),
            addComment:              static (s, r) => Write(BcfApiVerb.Post, $"{s.topics}/{r.TopicGuid}/comments", BcfProjection.ToBody(r.Comment), s.key),
            reviseComment:           static (s, r) => Write(BcfApiVerb.Put, $"{s.topics}/{r.TopicGuid}/comments/{r.Comment.Guid}", BcfProjection.ToBody(r.Comment), s.key),
            readComments:            static (s, r) => Fin.Succ(new BcfApiRequest(BcfApiVerb.Get, r.Comment.Match(Some: c => $"{s.topics}/{r.TopicGuid}/comments/{c}", None: () => r.Query.IfNone(BcfQuery.All).Render($"{s.topics}/{r.TopicGuid}/comments")), default)),
            retireComment:           static (s, r) => Fin.Succ(new BcfApiRequest(BcfApiVerb.Delete, $"{s.topics}/{r.TopicGuid}/comments/{r.Guid}", default)),
            addViewpoint:            static (s, r) => Write(BcfApiVerb.Post, $"{s.topics}/{r.TopicGuid}/viewpoints", BcfProjection.ToBody(r.Viewpoint), s.key),
            readViewpoints:          static (s, r) => Fin.Succ(new BcfApiRequest(BcfApiVerb.Get, r.Viewpoint.Match(Some: v => $"{s.topics}/{r.TopicGuid}/viewpoints/{v}", None: () => r.Query.IfNone(BcfQuery.All).Render($"{s.topics}/{r.TopicGuid}/viewpoints")), default)),
            retireViewpoint:         static (s, r) => Fin.Succ(new BcfApiRequest(BcfApiVerb.Delete, $"{s.topics}/{r.TopicGuid}/viewpoints/{r.Guid}", default)),
            readSnapshot:            static (s, r) => Fin.Succ(new BcfApiRequest(BcfApiVerb.Get, $"{s.topics}/{r.TopicGuid}/viewpoints/{r.Viewpoint}/snapshot", default)),
            readRelatedTopics:       static (s, r) => Fin.Succ(new BcfApiRequest(BcfApiVerb.Get, $"{s.topics}/{r.TopicGuid}/related_topics", default)),
            setRelatedTopics:        static (s, r) => Write(BcfApiVerb.Put, $"{s.topics}/{r.TopicGuid}/related_topics", r.RelatedTopics.Map(static g => new BcfApiRelatedTopicBody(g)).ToImmutableArray(), s.key),
            readDocumentReferences:  static (s, r) => Fin.Succ(new BcfApiRequest(BcfApiVerb.Get, $"{s.topics}/{r.TopicGuid}/document_references", default)),
            addDocumentReference:    static (s, r) => Write(BcfApiVerb.Post, $"{s.topics}/{r.TopicGuid}/document_references", BcfProjection.ToBody(r.Reference), s.key),
            reviseDocumentReference: static (s, r) => Write(BcfApiVerb.Put, $"{s.topics}/{r.TopicGuid}/document_references/{r.Reference.Guid}", BcfProjection.ToBody(r.Reference), s.key),
            readFiles:               static (s, r) => Fin.Succ(new BcfApiRequest(BcfApiVerb.Get, $"{s.topics}/{r.TopicGuid}/files", default)),
            setFiles:                static (s, r) => Write(BcfApiVerb.Put, $"{s.topics}/{r.TopicGuid}/files", r.Files.Map(BcfProjection.ToBody).ToImmutableArray(), s.key),
            readDocuments:           static (s, r) => Fin.Succ(new BcfApiRequest(BcfApiVerb.Get, r.Document.Match(Some: d => $"{s.project}/documents/{d}", None: () => $"{s.project}/documents"), default)),
            addDocument:             static (s, r) => Fin.Succ(new BcfApiRequest(BcfApiVerb.Post, $"{s.project}/documents?filename={Uri.EscapeDataString(r.Document.Filename)}", r.Document.Data)),
            readProject:             static (s, _) => Fin.Succ(new BcfApiRequest(BcfApiVerb.Get, s.project, default)),
            readExtensions:          static (s, _) => Fin.Succ(new BcfApiRequest(BcfApiVerb.Get, $"{s.project}/extensions", default)));

    static Fin<BcfApiRequest> Write<T>(BcfApiVerb verb, string resource, T body, Op key) =>
        Encode(body, key).Map(bytes => new BcfApiRequest(verb, resource, bytes));

    // The READ half of the lane — the response-admission seam DECIDED Bim-side: a GET response decodes through the
    // SAME snake_case register the write bodies ride (the read shape IS the resource body plus its nullable
    // server-assigned columns), and the Compute transport hands raw bytes and decodes NOTHING, so a
    // transport-side dialect adapter is the rejected second seam.
    public static Fin<T> Admit<T>(ReadOnlyMemory<byte> body, string resource, Op key) =>
        Decode<T>(body, resource, key);

    static Fin<byte[]> Encode<T>(T body, Op key) =>
        key.Catch(() => JsonSerializer.SerializeToUtf8Bytes(body, BcfApiContext.Json));

    static Fin<T> Decode<T>(ReadOnlyMemory<byte> body, string resource, Op key) =>
        key.Catch(() => JsonSerializer.Deserialize<T>(body.Span, BcfApiContext.Json))
            .Bind(value => value is { } admitted
                ? Fin.Succ(admitted)
                : Fin.Fail<T>(new BimFault.Refused(
                    key, BimScope.Review, BimReason.Rejected, $"bcf-api:read:{resource}:null-payload")));

    // The RESPONSE half completing the round trip: one total Switch over the SAME closed union lands status + raw
    // bytes on the typed BcfOutcome family, so response decode never grows a per-endpoint reader. A non-2xx status
    // refuses ONCE here with the status and the resource case named — the re-drive the verb row declares is the
    // transport's to spend, and this seam only reads the verdict. A write echo re-admits through the same register
    // so the server-assigned columns land; a retire and both replace acknowledgements land Done; the snapshot is a
    // PNG pass-through no register touches. The pagination cursor lives on the REQUEST, so a page walk is: issue,
    // Open, fold rows, advance the query — exhausted when a page returns fewer than Top.
    public static Fin<BcfOutcome> Open(BcfResource resource, int status, ReadOnlyMemory<byte> body, Op key) =>
        status is >= 200 and < 300
            ? resource.Switch(
                state:                   (body, key),
                createTopic:             static (s, _) => Admit<BcfApiTopicBody>(s.body, "topics", s.key).Map(static b => (BcfOutcome)new BcfOutcome.Topics(Seq(BcfProjection.ToDomain(b)))),
                reviseTopic:             static (s, _) => Admit<BcfApiTopicBody>(s.body, "topics", s.key).Map(static b => (BcfOutcome)new BcfOutcome.Topics(Seq(BcfProjection.ToDomain(b)))),
                readTopic:               static (s, r) => r.Guid.IsSome
                                             ? Admit<BcfApiTopicBody>(s.body, "topics", s.key).Map(static b => (BcfOutcome)new BcfOutcome.Topics(Seq(BcfProjection.ToDomain(b))))
                                             : Rows<BcfApiTopicBody>(s.body, "topics", s.key).Map(static rows => (BcfOutcome)new BcfOutcome.Topics(rows.Map(BcfProjection.ToDomain))),
                retireTopic:             static (s, _) => Fin.Succ((BcfOutcome)new BcfOutcome.Done()),
                addComment:              static (s, _) => Admit<BcfApiCommentBody>(s.body, "comments", s.key).Map(static b => (BcfOutcome)new BcfOutcome.Comments(Seq(BcfProjection.ToDomain(b)))),
                reviseComment:           static (s, _) => Admit<BcfApiCommentBody>(s.body, "comments", s.key).Map(static b => (BcfOutcome)new BcfOutcome.Comments(Seq(BcfProjection.ToDomain(b)))),
                readComments:            static (s, r) => r.Comment.IsSome
                                             ? Admit<BcfApiCommentBody>(s.body, "comments", s.key).Map(static b => (BcfOutcome)new BcfOutcome.Comments(Seq(BcfProjection.ToDomain(b))))
                                             : Rows<BcfApiCommentBody>(s.body, "comments", s.key).Map(static rows => (BcfOutcome)new BcfOutcome.Comments(rows.Map(BcfProjection.ToDomain))),
                retireComment:           static (s, _) => Fin.Succ((BcfOutcome)new BcfOutcome.Done()),
                addViewpoint:            static (s, _) => Admit<BcfApiViewpointBody>(s.body, "viewpoints", s.key).Bind(b => BcfProjection.ToDomain(b, s.key)).Map(static v => (BcfOutcome)new BcfOutcome.Viewpoints(Seq(v))),
                readViewpoints:          static (s, r) => r.Viewpoint.IsSome
                                             ? Admit<BcfApiViewpointBody>(s.body, "viewpoints", s.key).Bind(b => BcfProjection.ToDomain(b, s.key)).Map(static v => (BcfOutcome)new BcfOutcome.Viewpoints(Seq(v)))
                                             : Rows<BcfApiViewpointBody>(s.body, "viewpoints", s.key).Bind(rows => rows.TraverseM(b => BcfProjection.ToDomain(b, s.key)).As()).Map(static rows => (BcfOutcome)new BcfOutcome.Viewpoints(rows)),
                retireViewpoint:         static (s, _) => Fin.Succ((BcfOutcome)new BcfOutcome.Done()),
                readSnapshot:            static (s, _) => Fin.Succ((BcfOutcome)new BcfOutcome.Payload(s.body)),
                readRelatedTopics:       static (s, _) => Rows<BcfApiRelatedTopicBody>(s.body, "related_topics", s.key).Map(static rows => (BcfOutcome)new BcfOutcome.Related(rows.Map(static r => r.RelatedTopicGuid))),
                setRelatedTopics:        static (s, _) => Fin.Succ((BcfOutcome)new BcfOutcome.Done()),
                readDocumentReferences:  static (s, _) => Rows<BcfApiDocumentReferenceBody>(s.body, "document_references", s.key).Map(static rows => (BcfOutcome)new BcfOutcome.References(rows.Map(BcfProjection.ToDomain))),
                addDocumentReference:    static (s, _) => Admit<BcfApiDocumentReferenceBody>(s.body, "document_references", s.key).Map(static b => (BcfOutcome)new BcfOutcome.References(Seq(BcfProjection.ToDomain(b)))),
                reviseDocumentReference: static (s, _) => Admit<BcfApiDocumentReferenceBody>(s.body, "document_references", s.key).Map(static b => (BcfOutcome)new BcfOutcome.References(Seq(BcfProjection.ToDomain(b)))),
                readFiles:               static (s, _) => Rows<BcfApiFileBody>(s.body, "files", s.key).Map(static rows => (BcfOutcome)new BcfOutcome.Files(rows.Map(BcfProjection.ToDomain))),
                setFiles:                static (s, _) => Fin.Succ((BcfOutcome)new BcfOutcome.Done()),
                readDocuments:           static (s, r) => r.Document.IsSome
                                             ? Fin.Succ((BcfOutcome)new BcfOutcome.Payload(s.body))
                                             : Rows<BcfApiDocumentBody>(s.body, "documents", s.key).Map(static rows => (BcfOutcome)new BcfOutcome.Documents(rows.Map(static d => new BcfDocument(d.Guid, d.Filename, BcfEdge.Word(d.Description), default)))),
                addDocument:             static (s, _) => Admit<BcfApiDocumentBody>(s.body, "documents", s.key).Map(static d => (BcfOutcome)new BcfOutcome.Documents(Seq(new BcfDocument(d.Guid, d.Filename, BcfEdge.Word(d.Description), default)))),
                readProject:             static (s, _) => Admit<BcfApiProjectBody>(s.body, "project", s.key).Map(static p => (BcfOutcome)new BcfOutcome.Project(new BcfProject(p.ProjectId, p.Name))),
                readExtensions:          static (s, _) => Admit<BcfApiExtensionsBody>(s.body, "extensions", s.key).Map(static e => (BcfOutcome)new BcfOutcome.Extensions(BcfProjection.ToDomain(e))))
            : Fin.Fail<BcfOutcome>(new BimFault.Refused(key, BimScope.Review, BimReason.Rejected, string.Join(':', new object?[] { "bcf-api-status", status.ToString(CultureInfo.InvariantCulture), resource.GetType().Name })));

    // The collection arity carries its OWN absence gate. The single-body admission's non-null test is a reference
    // test and the collection root is a struct: a hostile or truncated CDE answering a bare `null` for a
    // collection GET deserializes to the UNINITIALIZED ImmutableArray, which passes every null check and then
    // throws on its first read — an exception escaping the rail from inside a boundary whose whole purpose is to
    // keep one out. IsDefault is the only predicate that separates it from a legitimately empty page.
    static Fin<Seq<T>> Rows<T>(ReadOnlyMemory<byte> body, string resource, Op key) =>
        Admit<ImmutableArray<T>>(body, resource, key)
            .Bind(rows => rows.IsDefault
                ? Fin.Fail<Seq<T>>(new BimFault.Refused(key, BimScope.Review, BimReason.Rejected, string.Join(':', new object?[] { "bcf-api", "read", resource, "null-collection" })))
                : Fin.Succ(toSeq(rows)));
}

// --- [COMPOSITION] ------------------------------------------------------------------------
// The BCF-API dialect register — the source-generated posture under the spec's snake_case field law, null
// slots OMITTED so the camera XOR and the optional document_guid/url discriminate by ABSENCE per the spec. Two
// naming registers over one issue vocabulary; the write-body roots AND the response roots register here, so
// BcfApi.Open decodes every arm off this one register and no transport-side dialect adapter exists.
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
