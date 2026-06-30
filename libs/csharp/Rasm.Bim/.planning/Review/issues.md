# [BIM_ISSUE_EXCHANGE]

The BCF 2.1/3.0 issue-exchange owner: one closed `BcfTopic`/`BcfComment`/`BcfViewpoint` record family anchored on IFC GlobalIds, the `Smino.Bcf.Toolkit` `.bcfzip` container codec (the `Worker` sink-shape-and-version-polymorphic converter reading and writing both BCF generations through one `IBcf` graph, and the fluent `BcfBuilder` authoring fold the clash-to-topic write composes), plus a `BcfApi` BCF-API 3.0 resource projection the `csharp:Compute/Runtime/channels#TRANSPORT_AXIS` transport issues. BCF is an issue/coordination container, never a geometry-or-model interchange row — the `.bcfzip` codec is self-owned in `coordination` and is NOT a row on the `Exchange/format#FORMAT_AXIS` geometry-format axis, because BCF carries issues and viewpoints, never an `ElementGraph` or `ImportedGeometry` import. Viewpoints anchor on IFC GlobalIds — each the seam `Rasm.Element/Graph/element#NODE_MODEL` `Node.Object.ExternalId` the `Projection/semantic#SEMANTIC_PROJECTOR` projects 1:1 from `IfcRoot.GlobalId` [H6] — so the issue payload aligns to element GlobalIds only, never to the geometry codec axis. The page composes the seam `Node.Object.ExternalId` IFC-GlobalId identity and the `csharp:Compute/Runtime/channels#TRANSPORT_AXIS` transport as settled vocabulary. The page is HOST-LOCAL.

## [01]-[INDEX]

- [01]-[BCF_ARCHIVE]: `BcfTopic`/`BcfComment`/`BcfViewpoint` record family, the `.bcfzip` codec over the `Smino.Bcf.Toolkit` `Worker`/`BcfBuilder` surface, and the `BcfApi` REST projection.
- [02]-[TS_PROJECTION]: the `BcfWire` host-free JSON producer the TS UI BCF anchor and live-binding decode — the topic/comment/viewpoint payload over the self-owned source-generated `BcfWireContext` with the `BcfStatus` string discriminant and the `IfcGuid` component anchor, plus the `BcfApi` BCF-API 3.0 resource projection the Compute transport issues.

## [02]-[BCF_ARCHIVE]

- Owner: `BcfTopic` the issue record anchored on its own GUID carrying title/status/type/priority/author, the `Description` body, the `AssignedTo`/`Stage`/`DueDate`/`Labels` metadata, and the comment and viewpoint sets; `BcfComment` the threaded comment record; `BcfViewpoint` the camera-and-selection record anchored on IFC GlobalId component selection and visibility; `BcfArchive` the `.bcfzip` codec composing the `Smino.Bcf.Toolkit` `Worker` round-trip (up-converting any 2.1/3.0 container into the one `Bcf30.Bcf` model, writing through the fluent `BcfBuilder`) and projecting onto the host-neutral record family; `BcfApi` the BCF-API 3.0 resource projection of the same topic family the Compute transport issues.
- Entry: `BcfArchive.Read(ReadOnlyMemory<byte> bcfzip, Op key)` runs `Worker.BcfFromStream` and folds each `Bcf30.Markup`'s `Topic` (with its `Comments` and `Viewpoints` sets) into the typed `BcfTopic` set, and `BcfArchive.Write(Seq<BcfTopic> topics, Op key)` folds the topics through `BcfBuilder.AddMarkup` and emits the `.bcfzip` bytes through `Worker.ToBcf(bcf, BcfVersionEnum.Bcf30)` — `Fin<T>` aborts on a malformed archive or a markup the BCF schema rejects (`Model/faults#FAULT_BAND` `BimFault.ModelRejected`), the typed case (band 2600, `Expected`-derived) lifting BARE onto the `Fin<T>` rail through the `Try.lift(...).Run().MapFail(...)` funnel with NO `.ToError()` hop (the `MapFail` lambda closing over the `Op key`, so it is NOT `static`); `BcfApi.Project(string projectId, BcfResource resource, Op key)` discriminates the BCF-API 3.0 topic-resource request on input shape (topic create/revise/read/retire, comment/viewpoint append-and-read) through the generated total `Switch` into the transport-neutral `BcfApiRequest` (the derived verb + resource path + the `BcfWire` JSON body) the Compute transport issues, never a transport minted here.
- Auto: `Read` opens the container through `Worker.BcfFromStream` (the `Worker` sniffing the version through `BcfExtensions.GetVersionFromStreamArchive` and UP-CONVERTING a 2.1 source to the one internal `Bcf30.Bcf` model — its `using BcfToolkit.Model.Bcf30;` binds the generic target — so the projection is single-version off `Bcf30`), then projects each `Bcf30.Markup`'s `Topic` (`Guid`/`Title`/`TopicStatus`/`TopicType`/`Priority`/`CreationAuthor`/`CreationDate`/`Description`/`AssignedTo`/`Stage`/`DueDate`/`Labels`) and its `Topic.Comments` `Bcf30.Comment` (`Author`/`CommentProperty`/`Date`/`Viewpoint.Guid`) onto the `BcfTopic`/`BcfComment` records, and reads each `Topic.Viewpoints` `Bcf30.ViewPoint`'s camera — the `VisualizationInfo.PerspectiveCamera` (else the `OrthogonalCamera`) `CameraViewPoint` `Bcf30.Point` plus `CameraDirection`/`CameraUpVector` `Bcf30.Direction` triplets — its `Components.Selection` `IfcGuid` set onto `SelectedGlobalIds`, its `Components.Visibility.Exceptions` `IfcGuid` set onto `VisibleGlobalIds`, and its `SnapshotData` `FileData` base64 onto the `Snapshot` bytes; `Write` folds each `BcfTopic` through `BcfBuilder.AddMarkup` with the `MarkupBuilder.SetGuid`/`SetTitle`/`SetTopicType`/`SetTopicStatus`/`SetPriority`/`SetCreationAuthor`/`SetCreationDate`/`SetDescription`/`SetAssignedTo`/`SetStage`/`SetDueDate`/`AddLabel` setters, the nested `AddComment` builder, and the nested `AddViewPoint` builder that re-authors the full `VisualizationInfo` (`SetPerspectiveCamera` from the triplets, `AddSelection`→`SetIfcGuid` per `SelectedGlobalId`, `SetVisibility`→`AddException` per `VisibleGlobalId`, `SetSnapshotData` from the `Snapshot` bytes), then `Worker.ToBcf(bcf, Bcf30)` serializes the built `IBcf` graph; the viewpoint carries its camera, component selection, and per-component visibility so a viewer round-trips the exact element selection rather than the GUID alone.
- Receipt: the `BcfTopic` set is the coordination evidence; a CDE or viewer round-trips it through the `Worker` `.bcfzip` codec and the `BcfApi` REST projection rides the same topic family so the file and REST forms carry one vocabulary, never two.
- Packages: Smino.Bcf.Toolkit, NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new BCF entity is one record on the topic family projected from the `Smino.Bcf.Toolkit` `IBcf` graph; a new BCF version is one `BcfVersionEnum` the `Worker` converter already discriminates (the version-agnostic `IBcf` projection forks no read/write code); the REST projection is one `BcfApi` shape over Compute's transport; never a row on the geometry-format axis and never a second issue store.
- Boundary: the `.bcfzip` codec is self-owned in `coordination` and is NOT a row on the `Exchange/format#FORMAT_AXIS` geometry-format axis — coupling the issue container to the geometry codec axis is the rejected form because BCF carries issues and viewpoints, never an `ElementGraph` or `ImportedGeometry` import; the container read/write is the `Smino.Bcf.Toolkit` `Worker`/`BcfBuilder` surface and the hand-rolled `ZipArchive`/`XDocument` codec is the retired form — the package owns the BCF 2.1/3.0 schema binding and the host-neutral records project from the concrete `Bcf30` graph so version selection never forks the projection (`Worker.BcfFromStream` up-converts a 2.1 source to the one internal `Bcf30.Bcf` model on read, `BcfVersionEnum` selecting only the read/write asset; the thin `IBcf`/`ITopic` markers carry no rich field, so the projection narrows to the concrete `Bcf30.Topic`/`Bcf30.Comment`/`Bcf30.ViewPoint`); the async `Worker` surface runs to completion at this codec boundary (the one language-owned `GetAwaiter().GetResult()` bridge the boundary kernel admits) and every throw lowers onto `Fin<T>` as a BARE `BimFault.ModelRejected` (band 2600 IS the `Expected` `Code`; a `.ToError()` hop is the retired form); viewpoints anchor on IFC GlobalIds — the seam `Rasm.Element/Graph/element#NODE_MODEL` `Node.Object.ExternalId` [H6] — so the issue payload aligns to element GlobalIds only, never to a geometry handle, and a `BcfViewpoint` carrying the retired `BimModel`/`BimElement` element record is the deleted form; the BCF-API REST shape rides the `csharp:Compute/Runtime/channels#TRANSPORT_AXIS` transport and a transport (client/retry/auth) minted here is the named seam violation; the durable lineage + CDE store is the app-platform Persistence owner's concern, joined by the IFC GlobalId the `csharp:Rasm.Persistence/Element/identity#ELEMENT_IDENTITY` `ElementIdentity.GlobalIds` map maintains (the 1:1 `NodeId`↔`Node.Object.ExternalId` projection) and surfaced to a CDE through the `csharp:Rasm.Persistence/Version/provenance#CAUSAL_DAG` W3C-PROV-JSON egress — a `BcfViewpoint.SelectedGlobalIds`/`VisibleGlobalIds` IFC `GlobalId` resolves to the persisted element on that `ExternalId`, so a BCF topic and the durable lineage bind one element without this owner gaining a durable-op-log row or Persistence gaining a second BCF schema.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System;
using System.IO;
using BcfToolkit;
using BcfToolkit.Builder.Bcf30;
using BcfToolkit.Model;
using LanguageExt;
using NodaTime;
using Bcf30 = BcfToolkit.Model.Bcf30;
using Op = Rasm.Domain.Op;
using static LanguageExt.Prelude;

// The BCF/coordination DOMAIN owner namespace — the `coordination` sub-domain the ARCHITECTURE seams name
// and AppUi `Editing/issues` consumes as `Rasm.Bim.Coordination.BcfTopic`/`BcfComment`/`BcfViewpoint`/`BcfStatus`;
// the codemap `Review/Coordination.cs`/`Review/Issues.cs` files emit this namespace, not folder-derived `Review`.
namespace Rasm.Bim.Coordination;

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record BcfViewpoint(
    string Guid,
    System.Numerics.Vector3 CameraPosition,
    System.Numerics.Vector3 CameraDirection,
    System.Numerics.Vector3 CameraUpVector,
    double FieldOfView,
    Seq<string> SelectedGlobalIds,
    Seq<string> VisibleGlobalIds,
    Option<ReadOnlyMemory<byte>> Snapshot);

public sealed record BcfComment(
    string Guid,
    string Author,
    string Text,
    Option<string> ViewpointGuid,
    Instant Date);

// The issue record: the core issue-tracking fields PLUS the BCF 3.0 topic metadata (`Description` the issue body,
// `AssignedTo` the assignee, `Stage` the milestone, `DueDate`, `Labels`), the metadata trailing-defaulted so a clash
// author or a minimal caller still constructs the core nine. A `Bcf30.Topic` field dropped here slices the concept.
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
    Seq<string> Labels = default);

// --- [OPERATIONS] -------------------------------------------------------------------------
// The `.bcfzip` codec composes Smino.Bcf.Toolkit `Worker` (the sink-shape + version polymorphic converter) —
// `Worker.BcfFromStream` reads any 2.1/3.0 container and UP-CONVERTS it to the one internal `Bcf30.Bcf` model
// (the Worker's `using BcfToolkit.Model.Bcf30;` binds the generic target, so a 2.1 source materializes as
// `Bcf30`), and `Worker.ToBcf(IBcf, Bcf30)` serializes it — and authors from scratch through the fluent
// `BcfBuilder` fold, retiring the hand-rolled `ZipArchive`/`XDocument` container parse; the projection stays
// single-version off `Bcf30.Bcf` (the thin `IBcf`/`ITopic` markers carry no rich field, so it narrows to the
// concrete `Bcf30.*`). The async `Worker` surface is run to completion at the codec boundary (the one
// language-owned `GetAwaiter().GetResult()` bridge), every throw lowered onto the `Fin<T>` rail through the
// `Try.lift` funnel as a BARE `BimFault.ModelRejected` (band 2600 IS the `Expected` `Code`; the `MapFail` lambda
// closes over the `Op key`, so it is NOT static and there is NO `.ToError()` hop).
public static class BcfArchive {
    public static Fin<Seq<BcfTopic>> Read(ReadOnlyMemory<byte> bcfzip, Op key) =>
        Try.lift(() => Decode(bcfzip)).Run().MapFail(error => new BimFault.ModelRejected(key, $"bcf-archive:{error.Message}"));

    public static Fin<byte[]> Write(Seq<BcfTopic> topics, Op key) =>
        Try.lift(() => Encode(topics)).Run().MapFail(error => new BimFault.ModelRejected(key, $"bcf-write:{error.Message}"));

    static Seq<BcfTopic> Decode(ReadOnlyMemory<byte> bcfzip) {
        using MemoryStream stream = new(bcfzip.ToArray());
        Bcf30.Bcf bcf = new Worker().BcfFromStream(stream).GetAwaiter().GetResult();
        return bcf.Markups.ToSeq().Map(TopicOf);
    }

    // The comments and viewpoints ride `Topic`, not `Markup` (a `Bcf30.Markup` carries only `Header`+`Topic`),
    // so the projection reads `topic.Comments`/`topic.Viewpoints`, never the absent `markup.Comment`.
    static BcfTopic TopicOf(Bcf30.Markup markup) {
        Bcf30.Topic topic = markup.Topic;
        return new BcfTopic(
            topic.Guid, topic.Title ?? "", StatusOf(topic.TopicStatus),
            topic.TopicType ?? "", topic.Priority ?? "", topic.CreationAuthor ?? "",
            Instant.FromDateTimeUtc(DateTime.SpecifyKind(topic.CreationDate, DateTimeKind.Utc)),
            topic.Comments.ToSeq().Map(CommentOf),
            topic.Viewpoints.ToSeq().Map(ViewpointOf),
            topic.Description ?? "", topic.AssignedTo ?? "", topic.Stage ?? "",
            Optional(topic.DueDate).Map(static d => Instant.FromDateTimeUtc(DateTime.SpecifyKind(d, DateTimeKind.Utc))),
            topic.Labels.ToSeq());
    }

    // The BCF `TopicStatus` is a free project-vocabulary string; the closed `BcfStatus` wire enum parses the
    // space-stripped token case-insensitively ("In Progress" -> InProgress), defaulting `Open` on an absent,
    // unrecognized, or out-of-range status so the wire discriminant never widens past the five-state lifecycle —
    // `Enum.TryParse` alone returns true for ANY integer-parseable token (a malformed "7" would mint an undefined
    // case that then crosses the wire as a numeric the TS string-switch cannot read), so `Enum.IsDefined` gates it.
    static BcfStatus StatusOf(string? status) =>
        Enum.TryParse(status?.Replace(" ", "", StringComparison.Ordinal), ignoreCase: true, out BcfStatus parsed) && Enum.IsDefined(parsed) ? parsed : BcfStatus.Open;

    static BcfComment CommentOf(Bcf30.Comment comment) =>
        new(comment.Guid, comment.Author ?? "", comment.CommentProperty ?? "",
            Optional(comment.Viewpoint?.Guid), Instant.FromDateTimeUtc(DateTime.SpecifyKind(comment.Date, DateTimeKind.Utc)));

    // The viewpoint reads the PERSPECTIVE camera (else the ORTHOGONAL), the `Selection` IfcGuid set onto
    // `SelectedGlobalIds`, the `Visibility.Exceptions` IfcGuid set onto `VisibleGlobalIds`, and the `SnapshotData`
    // base64 onto the `Snapshot` bytes — an import round-trips the framing, selection, visibility, and preview.
    static BcfViewpoint ViewpointOf(Bcf30.ViewPoint reference) {
        Bcf30.VisualizationInfo? visualization = reference.VisualizationInfo;
        Bcf30.Components? components = visualization?.Components;
        (System.Numerics.Vector3 position, System.Numerics.Vector3 direction, System.Numerics.Vector3 up, double fov) = CameraOf(visualization);
        return new BcfViewpoint(
            reference.Guid, position, direction, up, fov,
            GuidsOf(components?.Selection),
            GuidsOf(components?.Visibility?.Exceptions),
            SnapshotOf(reference.SnapshotData));
    }

    // The perspective camera carries the field of view; the orthographic camera has none (fov 0 the consumer
    // reads as orthographic) yet its framing is still captured; a viewpoint with neither yields a default fov.
    static (System.Numerics.Vector3 Position, System.Numerics.Vector3 Direction, System.Numerics.Vector3 Up, double Fov) CameraOf(Bcf30.VisualizationInfo? visualization) =>
        visualization?.PerspectiveCamera is { } perspective
            ? (Vec(perspective.CameraViewPoint), Vec(perspective.CameraDirection), Vec(perspective.CameraUpVector), perspective.FieldOfView)
            : visualization?.OrthogonalCamera is { } orthogonal
                ? (Vec(orthogonal.CameraViewPoint), Vec(orthogonal.CameraDirection), Vec(orthogonal.CameraUpVector), 0d)
                : (default, default, default, 60d);

    static Seq<string> GuidsOf(System.Collections.ObjectModel.Collection<Bcf30.Component>? set) =>
        Optional(set).Map(static s => toSeq(s).Map(static c => c.IfcGuid)).IfNone(Seq<string>());

    // `CameraViewPoint` is a `Bcf30.Point`, `CameraDirection`/`CameraUpVector` are `Bcf30.Direction` — distinct
    // toolkit types sharing the X/Y/Z shape, so each takes its own overload (the toolkit ships no shared base).
    static System.Numerics.Vector3 Vec(Bcf30.Point? p) => p is null ? default : new((float)p.X, (float)p.Y, (float)p.Z);
    static System.Numerics.Vector3 Vec(Bcf30.Direction? d) => d is null ? default : new((float)d.X, (float)d.Y, (float)d.Z);

    // The viewpoint snapshot is the `FileData.Data` base64 the BCF carries; a malformed/absent payload yields
    // None (an optional preview never faults the archive read), decoded non-throwing via `TryFromBase64String`.
    static Option<ReadOnlyMemory<byte>> SnapshotOf(FileData? snapshot) {
        if (snapshot?.Data is not { Length: > 0 } data) { return None; }
        byte[] buffer = new byte[data.Length];
        return Convert.TryFromBase64String(data, buffer, out int written)
            ? Some<ReadOnlyMemory<byte>>(buffer.AsMemory(0, written))
            : None;
    }

    static byte[] Encode(Seq<BcfTopic> topics) {
        BcfBuilder builder = topics.Fold(new BcfBuilder().WithDefaults(), static (acc, topic) => acc.AddMarkup(markup => {
            markup.SetGuid(topic.Guid).SetTitle(topic.Title).SetTopicType(topic.TopicType)
                .SetTopicStatus(topic.Status.ToString()).SetPriority(topic.Priority).SetCreationAuthor(topic.Author)
                .SetCreationDate(topic.CreationDate.ToDateTimeUtc())
                .SetDescription(topic.Description).SetAssignedTo(topic.AssignedTo).SetStage(topic.Stage)
                .SetDueDate(topic.DueDate.MatchUnsafe(static d => d.ToDateTimeUtc(), static () => (DateTime?)null));
            topic.Labels.Iter(l => markup.AddLabel(l));
            topic.Comments.Iter(c => markup.AddComment(comment => comment
                .SetGuid(c.Guid).SetAuthor(c.Author).SetComment(c.Text).SetDate(c.Date.ToDateTimeUtc())
                .SetViewPointGuid(c.ViewpointGuid.MatchUnsafe(static g => g, static () => null))));
            topic.Viewpoints.Iter(v => markup.AddViewPoint(vp => AuthorViewpoint(vp, v)));
        }));
        using Stream stream = new Worker().ToBcf(builder.Build(), BcfVersionEnum.Bcf30).GetAwaiter().GetResult();
        using MemoryStream sink = new();
        stream.CopyTo(sink);
        return sink.ToArray();
    }

    // The viewpoint author re-emits the full `VisualizationInfo` so a write round-trips what the read captured —
    // the perspective camera from the triplets, the `Selection` from `SelectedGlobalIds`, the visibility
    // exceptions from `VisibleGlobalIds`, and the `SnapshotData` from the `Snapshot` bytes; the prior Guid-only
    // author dropped them all, so the "round-trips the exact element selection" receipt was hollow.
    static void AuthorViewpoint(ViewPointBuilder vp, BcfViewpoint viewpoint) {
        vp.SetGuid(viewpoint.Guid);
        viewpoint.Snapshot.IfSome(bytes => vp.SetSnapshotData(new FileData { Mime = "image/png", Data = Convert.ToBase64String(bytes.Span) }));
        vp.SetVisualizationInfo(info => {
            info.SetPerspectiveCamera(camera => camera
                .SetCameraViewPoint(viewpoint.CameraPosition.X, viewpoint.CameraPosition.Y, viewpoint.CameraPosition.Z)
                .SetCameraDirection(viewpoint.CameraDirection.X, viewpoint.CameraDirection.Y, viewpoint.CameraDirection.Z)
                .SetCameraUpVector(viewpoint.CameraUpVector.X, viewpoint.CameraUpVector.Y, viewpoint.CameraUpVector.Z)
                .SetFieldOfView(viewpoint.FieldOfView));
            viewpoint.SelectedGlobalIds.Iter(id => info.AddSelection(component => component.SetIfcGuid(id)));
            if (!viewpoint.VisibleGlobalIds.IsEmpty) {
                info.SetVisibility(visibility => {
                    visibility.SetDefaultVisibility(false);
                    viewpoint.VisibleGlobalIds.Iter(id => visibility.AddException(component => component.SetIfcGuid(id)));
                });
            }
        });
    }
}
```

## [03]-[TS_PROJECTION]

- Owner: `BcfWire` the host-free JSON wire producer of the `[2]-[BCF_ARCHIVE]` topic family — `BcfWire.Topics` the projected `ImmutableArray<BcfTopicWire>` payload carrying the topic/comment/viewpoint graph the `ts:ui/bcf-anchor` panel decodes, `BcfTopicWire`/`BcfCommentWire`/`BcfViewpointWire` the BCL-typed serialization projection of the codec records (the domain records carry LanguageExt `Seq`/`Option` STJ cannot round-trip without converters the canonical context omits, so the wire mirror narrows to `ImmutableArray`/nullable and DROPS the heavy `Snapshot` bytes — two real reasons, never a redundant mirror) with the `BcfStatus` string-stable discriminant and the IFC-GUID component anchor; `BcfWireContext` the SELF-OWNED source-generated `JsonSerializerContext` (`BcfWireContext.Json` the options) the BCF family serializes through — the retired generic `BimWireContext` was a model STJ serializer inside an AEC package (a strata leak; the seam-graph STJ wire is `csharp:Rasm.Persistence/Element/codec#CODEC_AXIS` `SnapshotCodec`'s); `BcfApi` the BCF-API 3.0 resource projection over the same wire body.
- Entry: `BcfWire.Encode(Seq<BcfTopic> topics, Op key)` projects the codec topic set onto the wire payload and `BcfWire.Decode(ReadOnlyMemory<byte> json, Op key)` admits it back — `Fin<T>` aborts on a malformed payload (`Model/faults#FAULT_BAND` `BimFault.ModelRejected`) lifting BARE onto the `Fin<T>` rail (band 2600 IS the `Expected` `Code`, no `.ToError()` hop) through the identical `Try.lift(...).Run().MapFail(...)` funnel the `[2]-[BCF_ARCHIVE]` codec carries; `BcfWire.TopicJson`/`CommentJson`/`ViewpointJson` are the single-entity bodies the `BcfApi.Project` REST projection POSTs/PUTs (one `BcfTopicWire`/`BcfCommentWire`/`BcfViewpointWire`, never the array root, each through the one `Try.lift` funnel); `BcfWire.Anchor(BcfViewpoint viewpoint)` projects the viewpoint `SelectedGlobalIds` onto the element-GlobalId set the UI live-binding highlights, so a TS pick round-trips the exact seam `Node.Object.ExternalId` IFC-GlobalId selection the C# viewpoint carries.
- Auto: `Encode` serializes through `BcfWireContext.Json` — the `BcfStatus` enum serializes by its `[JsonStringEnumMemberName]` lower-kebab name (`open`/`in-progress`/`resolved`/`closed`/`reopened`) under `UseStringEnumConverter` so a TS discriminant switches on the string rather than the ordinal, the `Instant` creation/comment dates serialize through the `ConfigureForNodaTime` `NodaConverters.InstantConverter` as `InstantPattern.ExtendedIso` ISO-8601 text, the BCL `System.Numerics.Vector3` camera triplet serializes as the host-free camel-case `{x,y,z}` object the TS camera binding reads (the struct's `X`/`Y`/`Z` are FIELDS, so `IncludeFields` admits them), and the `SelectedGlobalIds`/`VisibleGlobalIds` `ImmutableArray<string>` serialize as the element-GlobalId arrays the live-binding anchors on; `Decode` re-admits every owner through the same options so a malformed `Instant` or a half-built record faults at admission rather than minting a partial topic.
- Receipt: the `BcfWire.Topics` payload is the one BCF cross-runtime contract — the `ts:ui/bcf-anchor` panel and live-binding decode the same `BcfTopic`/`BcfComment`/`BcfViewpoint` vocabulary the C# branch mints, never re-minting a parallel issue shape; a viewpoint anchors on the seam `Rasm.Element/Graph/element#NODE_MODEL` `Node.Object.ExternalId` IFC GlobalId so the TS selection highlight and the C# component selection carry one element identity, and the `BcfApi` REST JSON and this wire payload carry the one `BcfTopicWire` vocabulary so the file, REST, and web forms never fork.
- Packages: Thinktecture.Runtime.Extensions, NodaTime, NodaTime.Serialization.SystemTextJson, LanguageExt.Core, Rasm, BCL `System.Text.Json` + `System.Collections.Immutable`
- Growth: a new BCF entity on the wire is one `[JsonSerializable]` row on the self-owned `BcfWireContext` and one wire record mirroring its codec record; a new topic field is one column on `BcfTopicWire`; a new BCF-API resource operation is one `BcfResource` case the generated total `Switch` breaks at every site at compile time (never a verb knob, never a `Get`/`Post` member beside `Project`); the TS panel decodes the new column with no second wire vocabulary; never a second serializer beside `BcfWireContext.Json`.
- Boundary: `BcfWire` is HOST-FREE — it carries no RhinoCommon type and no `ZipArchive`/`XDocument` codec surface, only the host-free record graph and the BCL `System.Numerics.Vector3`; it rides the SELF-OWNED `BcfWireContext` source-generated context, and a generic model STJ serializer re-minted in this AEC package (the retired `BimWireContext`) or a second `JsonSerializerOptions` is the deleted form — the BCF rows are `[JsonSerializable]` rows on the one `BcfWireContext`, and the wire records are the BCL-typed serialization projection of the domain records (the `Seq`/`Option`→`ImmutableArray`/nullable narrowing STJ round-trips), never a redundant mirror; the `BcfStatus` discriminant is the `[JsonStringEnumMemberName]` string so a TS decode switches on a stable name and an ordinal-keyed enum crossing the wire is the named seam violation; the viewpoint anchors on the seam `Rasm.Element/Graph/element#NODE_MODEL` `Node.Object.ExternalId` IFC GlobalId so the payload aligns to element GlobalIds only, never to a geometry handle; the `.bcfzip` archive, the `BcfApi` REST body, and this wire payload project onto this one `BcfTopicWire` family so the issue vocabulary is minted once in C# and decoded by the TS peer, never re-minted — a TS-side parallel topic shape is the named cross-language drift defect.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using LanguageExt;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using Op = Rasm.Domain.Op;
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
// The host-free BCL-typed serialization projection of the `[2]-[BCF_ARCHIVE]` domain records: the domain records
// carry LanguageExt `Seq`/`Option` STJ cannot round-trip without converters the canonical `ElementJson` context
// omits, so the wire mirror narrows to `ImmutableArray`/nullable + the BCL `Vector3` camera (its `X`/`Y`/`Z` fields
// serialize under `IncludeFields`) and DROPS the heavy `Snapshot` bytes — two real reasons the wire records exist,
// never a redundant mirror. `BcfStatus` crosses as its `[JsonStringEnumMemberName]` string.
public sealed record BcfViewpointWire(
    string Guid,
    System.Numerics.Vector3 CameraPosition,
    System.Numerics.Vector3 CameraDirection,
    System.Numerics.Vector3 CameraUpVector,
    double FieldOfView,
    ImmutableArray<string> SelectedGlobalIds,
    ImmutableArray<string> VisibleGlobalIds);

public sealed record BcfCommentWire(
    string Guid,
    string Author,
    string Text,
    string? ViewpointGuid,
    Instant Date);

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
    ImmutableArray<string> Labels);

public sealed record BcfWire(ImmutableArray<BcfTopicWire> Topics) {
    public static Fin<byte[]> Encode(Seq<BcfTopic> topics, Op key) =>
        Try.lift(() => JsonSerializer.SerializeToUtf8Bytes(new BcfWire(topics.Map(Project).ToImmutableArray()), BcfWireContext.Json)).Run()
            .MapFail(error => new BimFault.ModelRejected(key, $"bcf-wire-encode:{error.Message}"));

    public static Fin<BcfWire> Decode(ReadOnlyMemory<byte> json, Op key) =>
        Try.lift(() => JsonSerializer.Deserialize<BcfWire>(json.Span, BcfWireContext.Json)!).Run()
            .MapFail(error => new BimFault.ModelRejected(key, $"bcf-wire-decode:{error.Message}"));

    // The single-entity BCF-API bodies the `BcfApi.Project` resource projection POSTs/PUTs — one `BcfTopicWire`,
    // `BcfCommentWire`, or `BcfViewpointWire`, never the array root — each lifted through the one `Try.lift` funnel.
    public static Fin<byte[]> TopicJson(BcfTopic topic, Op key) =>
        Try.lift(() => JsonSerializer.SerializeToUtf8Bytes(Project(topic), BcfWireContext.Json)).Run()
            .MapFail(error => new BimFault.ModelRejected(key, $"bcf-api-topic:{error.Message}"));

    public static Fin<byte[]> CommentJson(BcfComment comment, Op key) =>
        Try.lift(() => JsonSerializer.SerializeToUtf8Bytes(ProjectComment(comment), BcfWireContext.Json)).Run()
            .MapFail(error => new BimFault.ModelRejected(key, $"bcf-api-comment:{error.Message}"));

    public static Fin<byte[]> ViewpointJson(BcfViewpoint viewpoint, Op key) =>
        Try.lift(() => JsonSerializer.SerializeToUtf8Bytes(ProjectViewpoint(viewpoint), BcfWireContext.Json)).Run()
            .MapFail(error => new BimFault.ModelRejected(key, $"bcf-api-viewpoint:{error.Message}"));

    public static Seq<string> Anchor(BcfViewpoint viewpoint) => viewpoint.SelectedGlobalIds;

    static BcfTopicWire Project(BcfTopic topic) =>
        new(topic.Guid, topic.Title, topic.Status, topic.TopicType, topic.Priority, topic.Author, topic.CreationDate,
            topic.Comments.Map(ProjectComment).ToImmutableArray(),
            topic.Viewpoints.Map(ProjectViewpoint).ToImmutableArray(),
            topic.Description, topic.AssignedTo, topic.Stage,
            topic.DueDate.MatchUnsafe(static d => (Instant?)d, static () => null),
            topic.Labels.ToImmutableArray());

    // The comment/viewpoint wire projections — ONE owner each, reused by the topic projection AND the single-entity
    // BCF-API bodies so the `Seq`/`Option` -> `ImmutableArray`/nullable narrowing is minted once. The absent
    // `ViewpointGuid` lowers through `MatchUnsafe` (the null-tolerant match the sibling nullable projections use):
    // the null-guarding `Match` would fault the encode on a comment with no viewpoint, the overwhelmingly common case.
    static BcfCommentWire ProjectComment(BcfComment c) =>
        new(c.Guid, c.Author, c.Text, c.ViewpointGuid.MatchUnsafe(static g => g, static () => null), c.Date);

    static BcfViewpointWire ProjectViewpoint(BcfViewpoint v) =>
        new(v.Guid, v.CameraPosition, v.CameraDirection, v.CameraUpVector, v.FieldOfView,
            v.SelectedGlobalIds.ToImmutableArray(), v.VisibleGlobalIds.ToImmutableArray());
}

// The BCF-API 3.0 REST RESOURCE the Compute transport issues: Bim owns the resource MODEL (the verb + the resource
// path + the `BcfWire` JSON body per the buildingSMART BCF-API 3.0 spec), the HTTP transport (client/retry/auth)
// stays the `csharp:Compute/Runtime/channels#TRANSPORT_AXIS` owner's — a transport minted here is the named seam
// violation. `BcfApiRequest` is a transport-NEUTRAL resource descriptor, never a Compute transport type.
public sealed record BcfApiRequest(BcfApiVerb Verb, string Resource, ReadOnlyMemory<byte> Body);

// The BCF-API 3.0 topic-resource request the polymorphic `Project` discriminates on: each case is one resource
// operation the spec defines over the topic graph, so `Project` maps the input SHAPE to the derived verb + path +
// body and never carries a verb knob or a `Create`/`Read`/`Update`/`Delete` member family. The read cases ride the
// `Option` shape for GET arity — `ReadTopic`/`ReadComments`/`ReadViewpoints` resolve the collection read (`None`)
// from the single read (the resource GUID) on the ONE case rather than forking a second, so the GET arity rides the
// value. The union is complete over the `[2]-[BCF_ARCHIVE]` record family: every owned record (topic, comment,
// viewpoint) is both writable AND readable over the REST surface — the topic carries full create/read/revise/retire,
// the comment and viewpoint a create POST PLUS a read GET. BCF-API returns comments/viewpoints only as separate sub-
// resource GETs (the topic GET does NOT inline them), so a create-only sub-resource the CDE can never pull back to
// rehydrate is the half-round-trip slice this closes — never a `Get`/`Post` member family beside `Project`.
[Union]
public abstract partial record BcfResource {
    private BcfResource() { }

    public sealed record CreateTopic(BcfTopic Topic) : BcfResource;
    public sealed record ReviseTopic(BcfTopic Topic) : BcfResource;
    public sealed record ReadTopic(Option<string> Guid) : BcfResource;
    public sealed record RetireTopic(string Guid) : BcfResource;
    public sealed record AddComment(string TopicGuid, BcfComment Comment) : BcfResource;
    public sealed record ReadComments(string TopicGuid, Option<string> Comment) : BcfResource;
    public sealed record AddViewpoint(string TopicGuid, BcfViewpoint Viewpoint) : BcfResource;
    public sealed record ReadViewpoints(string TopicGuid, Option<string> Viewpoint) : BcfResource;
}

// --- [SERVICES] ---------------------------------------------------------------------------
// ONE polymorphic `Project` over the BCF-API 3.0 topic resource, discriminating on the `BcfResource` input shape via
// the generated TOTAL `Switch` — never a `Create`/`Read`/`Update`/`Delete` member family. Bim owns the resource MODEL
// (the verb DERIVES from the case — POST a new topic, PUT a revision, GET a topic/comment/viewpoint single-or-collection,
// DELETE a topic, POST a comment/viewpoint sub-resource; GET/DELETE carry no body); the HTTP transport (client/retry/auth)
// stays the `csharp:Compute/Runtime/channels#TRANSPORT_AXIS` owner's, a transport minted here being the named seam violation.
public static class BcfApi {
    public static Fin<BcfApiRequest> Project(string projectId, BcfResource resource, Op key) =>
        resource.Switch(
            state:          (key, topics: $"bcf/3.0/projects/{projectId}/topics"),
            createTopic:    static (s, r) => BcfWire.TopicJson(r.Topic, s.key).Map(body => new BcfApiRequest(BcfApiVerb.Post, s.topics, body)),
            reviseTopic:    static (s, r) => BcfWire.TopicJson(r.Topic, s.key).Map(body => new BcfApiRequest(BcfApiVerb.Put, $"{s.topics}/{r.Topic.Guid}", body)),
            readTopic:      static (s, r) => Fin.Succ(new BcfApiRequest(BcfApiVerb.Get, r.Guid.Match(Some: g => $"{s.topics}/{g}", None: () => s.topics), default)),
            retireTopic:    static (s, r) => Fin.Succ(new BcfApiRequest(BcfApiVerb.Delete, $"{s.topics}/{r.Guid}", default)),
            addComment:     static (s, r) => BcfWire.CommentJson(r.Comment, s.key).Map(body => new BcfApiRequest(BcfApiVerb.Post, $"{s.topics}/{r.TopicGuid}/comments", body)),
            readComments:   static (s, r) => Fin.Succ(new BcfApiRequest(BcfApiVerb.Get, r.Comment.Match(Some: c => $"{s.topics}/{r.TopicGuid}/comments/{c}", None: () => $"{s.topics}/{r.TopicGuid}/comments"), default)),
            addViewpoint:   static (s, r) => BcfWire.ViewpointJson(r.Viewpoint, s.key).Map(body => new BcfApiRequest(BcfApiVerb.Post, $"{s.topics}/{r.TopicGuid}/viewpoints", body)),
            readViewpoints: static (s, r) => Fin.Succ(new BcfApiRequest(BcfApiVerb.Get, r.Viewpoint.Match(Some: v => $"{s.topics}/{r.TopicGuid}/viewpoints/{v}", None: () => $"{s.topics}/{r.TopicGuid}/viewpoints"), default)));
}

// --- [COMPOSITION] ------------------------------------------------------------------------
// The SELF-OWNED BCF wire serializer — replaces the retired generic `BimWireContext` (a model STJ context inside an
// AEC package was a strata leak; the seam-graph STJ wire is `csharp:Rasm.Persistence/Element/codec#CODEC_AXIS`
// `SnapshotCodec`'s). It serializes ONLY the BCF coordination family: camel-case names, `[JsonStringEnumMemberName]`
// string enums (`UseStringEnumConverter`), `IncludeFields` so the BCL `Vector3` camera serializes as `{x,y,z}`, and
// the NodaTime `Instant` ISO-8601 converter (`ConfigureForNodaTime`). The `[JsonSerializable]` of `BcfTopicWire`/
// `BcfCommentWire`/`BcfViewpointWire` admits the single-entity `BcfApi` topic/comment/viewpoint bodies. No Thinktecture
// converter — the BCF family carries no `[ValueObject]`/`[SmartEnum]` (`BcfResource` is a request `[Union]`, never serialized).
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, UseStringEnumConverter = true, IncludeFields = true)]
[JsonSerializable(typeof(BcfWire))]
[JsonSerializable(typeof(BcfTopicWire))]
[JsonSerializable(typeof(BcfCommentWire))]
[JsonSerializable(typeof(BcfViewpointWire))]
public sealed partial class BcfWireContext : JsonSerializerContext {
    public static readonly JsonSerializerOptions Json =
        new JsonSerializerOptions(Default.Options).ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
}
```

## [04]-[RESEARCH]

- [BCF_TOOLKIT_CODEC]: the `Smino.Bcf.Toolkit` 3.2.0 codec surface the `BcfArchive` composes is decompile-verified at `.api/api-smino-bcf-toolkit` and `assay api query smino.bcf.toolkit` — `Worker.BcfFromStream(Stream)` → `Task<Bcf30.Bcf>` (the Worker's `using BcfToolkit.Model.Bcf30;` binds the generic target, so a 2.1 source is UP-CONVERTED to the one internal `Bcf30.Bcf` model on read), `Worker.ToBcf(IBcf, BcfVersionEnum.Bcf30)` → `Task<Stream>` the serialize, `BcfExtensions.GetVersionFromStreamArchive` the version sniff, the `BcfBuilder.WithDefaults()`/`AddMarkup(Action<MarkupBuilder>)`/`Build()` authoring fold, the nested `MarkupBuilder.SetGuid`/`SetTitle`/`SetTopicType`/`SetTopicStatus`/`SetPriority`/`SetCreationAuthor`/`SetCreationDate`/`AddComment`/`AddViewPoint`, `CommentBuilder.SetGuid`/`SetAuthor`/`SetComment`/`SetDate`/`SetViewPointGuid`, `ViewPointBuilder.SetGuid`/`SetSnapshotData`/`SetVisualizationInfo`, `VisualizationInfoBuilder.SetPerspectiveCamera`/`AddSelection`/`SetVisibility`, `PerspectiveCameraBuilder.SetCameraViewPoint`/`SetCameraDirection`/`SetCameraUpVector`/`SetFieldOfView`, `ComponentBuilder.SetIfcGuid`, and `VisibilityBuilder.SetDefaultVisibility`/`AddException` setters; the read projection reads the concrete `Bcf30.Bcf.Markups` `ConcurrentBag<Markup>`→`Bcf30.Markup.Topic` (the `Markup` carries ONLY `Header`+`Topic`, so comments and viewpoints ride `Topic.Comments`/`Topic.Viewpoints`, NEVER an absent `Markup.Comment`), the `Bcf30.Topic` (`Guid`/`Title`/`TopicStatus`/`TopicType`/`Priority`/`CreationAuthor`/`CreationDate`/`Description`/`AssignedTo`/`Stage`/`DueDate`/`Labels`) and `Bcf30.Comment` (`Author`/`CommentProperty`/`Date`/`Viewpoint.Guid`/`Guid`) detail columns the thin `ITopic`/`IComment` markers (each an EMPTY interface) do not carry, the `Bcf30.ViewPoint` (`Guid`/`VisualizationInfo`/`SnapshotData`) whose `VisualizationInfo.PerspectiveCamera`/`OrthogonalCamera` carries `CameraViewPoint` `Bcf30.Point` plus `CameraDirection`/`CameraUpVector` `Bcf30.Direction` (each `X`/`Y`/`Z` `double`, distinct toolkit types), the `Components.Selection` `Collection<Component>` and `Components.Visibility.Exceptions` `Collection<Component>` whose `Component.IfcGuid` is the element anchor, and the `FileData.Data` base64 the `SnapshotData` carries; `BcfVersion.TryParse` accepts only `"2.1"`/`"3.0"` (BCF 2.0 input rejects `Unsupported BCF version`), so the codec's real coverage is BCF 2.1 + 3.0 and the hand-rolled `ZipArchive`/`XDocument` container is retired.
- [BCF_API_REST]: the BCF-API 3.0 REST resource shape — the topic/comment/viewpoint endpoint vocabulary and the JSON projection of the same topic family — grounds against the published buildingSMART BCF-API 3.0 specification so the `BcfApi.Project` resource shape matches the REST resource model; `BcfApi.Project(string projectId, BcfResource resource, Op key)` discriminates the closed `BcfResource` request `[Union]` (`CreateTopic`/`ReviseTopic`/`ReadTopic`/`RetireTopic`/`AddComment`/`ReadComments`/`AddViewpoint`/`ReadViewpoints`) on input shape and mints the transport-NEUTRAL `BcfApiRequest(BcfApiVerb, string Resource, ReadOnlyMemory<byte> Body)` — the verb DERIVED from the case (`POST topics` / `PUT topics/{guid}` / `GET topics` or `topics/{guid}` / `DELETE topics/{guid}` / `POST topics/{guid}/comments` / `GET topics/{guid}/comments` or `comments/{guid}` / `POST topics/{guid}/viewpoints` / `GET topics/{guid}/viewpoints` or `viewpoints/{guid}`), the `BcfWire.TopicJson`/`CommentJson`/`ViewpointJson` body for the write verbs and none for `GET`/`DELETE` — so every owned record round-trips over REST (a create-only comment/viewpoint sub-resource the CDE can never pull back is the half-round-trip slice this closes, since the BCF-API topic GET does not inline comments/viewpoints) — which the `csharp:Compute/Runtime/channels#TRANSPORT_AXIS` transport issues at cross-folder alignment; a transport (client/retry/auth) minted here is the named seam violation, a verb knob beside the resource is the rejected form (the verb is recoverable from the case), and the file, REST, and web forms carry one `BcfTopicWire`/`BcfCommentWire`/`BcfViewpointWire` vocabulary.
- [HOST_FREE_VECTOR_COMPOSE]: the `BcfViewpoint`/`BcfViewpointWire` camera triplet is the BCL `System.Numerics.Vector3` — the one host-free 3-vector the HOST-FREE BCF record family admits (the kernel `Rasm/Vectors` owner exposes only the host-bound RhinoCommon `Vector3d`, which a host-free coordination record may never carry), so the codec record, the wire record, and the `csharp:Rasm.AppUi/Editing/issues` board projection's `BcfViewpoint` construction all name `System.Numerics.Vector3` and a BCF-local or host-bound coordinate type is the deleted form; the struct's `X`/`Y`/`Z` are FIELDS, so the wire serializes them as the camel-case `{x,y,z}` object only under the `BcfWireContext` `IncludeFields` option.
