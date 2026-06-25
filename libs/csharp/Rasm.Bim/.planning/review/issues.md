# [BIM_ISSUE_EXCHANGE]

The BCF 2.1/3.0 issue-exchange owner: one closed `BcfTopic`/`BcfComment`/`BcfViewpoint` record family anchored on IFC GlobalIds, the `Smino.Bcf.Toolkit` `.bcfzip` container codec (the `Worker` sink-shape-and-version-polymorphic converter reading and writing both BCF generations through one `IBcf` graph, and the fluent `BcfBuilder` authoring fold the clash-to-topic write composes), plus a `BcfApi` REST projection riding Compute's transport. BCF is an issue/coordination container, never a geometry-or-model interchange row — the `.bcfzip` codec is self-owned in `coordination` and is NOT a row on the `Exchange/format#FORMAT_AXIS` geometry-format axis, because BCF carries issues and viewpoints, never a `BimModel` or `ImportedGeometry` import. Viewpoints anchor on the `Model/elements#ELEMENT_MODEL` `GlobalId`, so the issue payload aligns to element GlobalIds only, never to the geometry codec axis. The page composes the `Model/elements#ELEMENT_MODEL` `BimModel` GlobalIds and the `csharp:Compute/Runtime/channels#TRANSPORT_AXIS` transport as settled vocabulary. The page is HOST-LOCAL.

## [01]-[INDEX]

- [01]-[BCF_ARCHIVE]: `BcfTopic`/`BcfComment`/`BcfViewpoint` record family, the `.bcfzip` codec over `ZipArchive`, and the `BcfApi` REST projection.
- [02]-[TS_PROJECTION]: the `BcfWire` host-free JSON producer the TS UI BCF anchor and live-binding decode — the topic/comment/viewpoint payload over the source-generated `BimWireContext` with the `BcfStatus` string discriminant and the `IfcGuid` component anchor.

## [02]-[BCF_ARCHIVE]

- Owner: `BcfTopic` the issue record anchored on its own GUID carrying title/status/type/priority/author and the comment and viewpoint sets; `BcfComment` the threaded comment record; `BcfViewpoint` the camera-and-selection record anchored on IFC GlobalId component selection and clipping; `BcfArchive` the `.bcfzip` codec composing the `Smino.Bcf.Toolkit` `Worker` round-trip (reading any 2.1/3.0 container into the typed `IBcf` graph, writing through the fluent `BcfBuilder`) and projecting onto the host-neutral record family; `BcfApi` the REST projection of the same topic family riding Compute's transport.
- Entry: `BcfArchive.Read(ReadOnlyMemory<byte> bcfzip)` runs `Worker.BcfFromStream` and folds each `Bcf30.Markup` (`Topic` + comment + viewpoint sets) into the typed `BcfTopic` set, and `BcfArchive.Write(Seq<BcfTopic> topics)` folds the topics through `BcfBuilder.AddMarkup` and emits the `.bcfzip` bytes through `Worker.ToBcf(bcf, BcfVersionEnum.Bcf30)` — `Fin<T>` aborts on a malformed archive or a markup the BCF schema rejects (`Model/faults#FAULT_BAND` `BimFault.ModelRejected`) lowered with `.ToError()` at the `Boundary` funnel; `BcfApi.Project(BcfTopic topic)` builds the BCF-API REST request the Compute transport issues, never a transport minted here.
- Auto: `Read` opens the container through `Worker.BcfFromStream` (the `Worker` sniffing the version through `BcfExtensions.GetVersionFromStreamArchive` and up-converting a 2.1 source to the one internal 3.0 generation), projects each `IBcf` markup's concrete `Bcf30.Topic` (`Guid`/`Title`/`TopicStatus`/`TopicType`/`Priority`/`CreationAuthor`/`CreationDate`) and its `Bcf30.Comment` (`Author`/`CommentProperty`/`Date`/`Viewpoint`) onto the `BcfTopic`/`BcfComment` records, and reads each viewpoint's `VisualizationInfo.PerspectiveCamera` (`CameraViewPoint`/`CameraDirection`/`CameraUpVector` `Bcf30.Point` triplets) and `Components.Selection` IFC-GUID `IfcGuid` set onto `BcfViewpoint`; `Write` folds each `BcfTopic` through `BcfBuilder.AddMarkup` with the `MarkupBuilder.SetGuid`/`SetTitle`/`SetTopicStatus`/`SetPriority`/`SetCreationDate` setters and the nested `AddComment`/`AddViewPoint` builders, then `Worker.ToBcf` serializes the built `IBcf` graph; the `BcfViewpoint` component selection carries IFC-GUID component visibility and clipping so a viewer round-trips the exact element selection.
- Receipt: the `BcfTopic` set is the coordination evidence; a CDE or viewer round-trips it through the `Worker` `.bcfzip` codec and the `BcfApi` REST projection rides the same topic family so the file and REST forms carry one vocabulary, never two.
- Packages: Smino.Bcf.Toolkit, NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new BCF entity is one record on the topic family projected from the `Smino.Bcf.Toolkit` `IBcf` graph; a new BCF version is one `BcfVersionEnum` the `Worker` converter already discriminates (the version-agnostic `IBcf` projection forks no read/write code); the REST projection is one `BcfApi` shape over Compute's transport; never a row on the geometry-format axis and never a second issue store.
- Boundary: the `.bcfzip` codec is self-owned in `coordination` and is NOT a row on the `Exchange/format#FORMAT_AXIS` geometry-format axis — coupling the issue container to the geometry codec axis is the rejected form because BCF carries issues and viewpoints, never a `BimModel` or `ImportedGeometry` import; the container read/write is the `Smino.Bcf.Toolkit` `Worker`/`BcfBuilder` surface and the hand-rolled `ZipArchive`/`XDocument` codec is the retired form — the package owns the BCF 2.1/3.0 schema binding and the host-neutral records project from the version-agnostic `IBcf`/`Bcf30` graph so version selection never forks the projection (`BcfVersionEnum` selects only the read/write asset); the async `Worker` surface runs to completion at this codec boundary (the one language-owned `GetAwaiter().GetResult()` bridge the boundary kernel admits) and every throw lowers onto `Fin<T>`; viewpoints anchor on the `Model/elements#ELEMENT_MODEL` `GlobalId` so the issue payload aligns to element GlobalIds only, never to a geometry handle; the BCF-API REST shape rides the `csharp:Compute/Runtime/channels#TRANSPORT_AXIS` transport and a transport minted here is the named seam violation; the durable annotation + CDE op-log store is the app-platform `csharp:Rasm.Persistence/Sync/annotation` owner's concern, joined at the `coordination ⇄ csharp:Rasm.Persistence/Sync/annotation # [WIRE]: durable annotation + CDE op-log` seam through the `csharp:Rasm.Persistence/Query/federation#ENTITY_GRAPH` key — a `BcfViewpoint.SelectedGlobalIds`/`VisibleGlobalIds` IFC `GlobalId` resolves to the `FederatedEntity` whose `EntityIdentity.Origin` the durable annotation `Anchor` carries, so a BCF topic and a durable annotation bind one element without this owner gaining a durable-op-log row or Persistence gaining a second BCF schema.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using BcfToolkit;
using BcfToolkit.Builder.Bcf30;
using BcfToolkit.Model;
using LanguageExt;
using NodaTime;
using Bcf30 = BcfToolkit.Model.Bcf30;

// The BCF/coordination DOMAIN owner namespace — the `coordination` sub-domain the ARCHITECTURE seams name
// and AppUi `Editing/issues` consumes as `Rasm.Bim.Coordination.BcfTopic`/`BcfComment`/`BcfViewpoint`/`BcfStatus`;
// the codemap `Review/Coordination.cs`/`Review/Issues.cs` files emit this namespace, not folder-derived `Review`.
namespace Rasm.Bim.Coordination;

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

public sealed record BcfTopic(
    string Guid,
    string Title,
    BcfStatus Status,
    string TopicType,
    string Priority,
    string Author,
    Instant CreationDate,
    Seq<BcfComment> Comments,
    Seq<BcfViewpoint> Viewpoints);

// The `.bcfzip` codec composes Smino.Bcf.Toolkit `Worker` (the sink-shape + version polymorphic
// converter) — `Worker.BcfFromStream` reads any 2.1/3.0 container into the typed `IBcf` graph and
// `Worker.ToBcf(IBcf, Bcf30)` serializes it — and authors from scratch through the fluent `BcfBuilder`
// fold, retiring the hand-rolled `ZipArchive`/`XDocument` container parse. The async `Worker` surface
// is run to completion at the codec boundary, every throw lowered onto `Fin<T>`; mixed-version input
// normalizes to one internal BCF 3.0 generation through the up-conversion the converter owns.
public static class BcfArchive {
    public static Fin<Seq<BcfTopic>> Read(ReadOnlyMemory<byte> bcfzip) =>
        Try.lift(() => Decode(bcfzip)).Run().MapFail(static error => new BimFault.ModelRejected($"bcf-archive:{error.Message}").ToError());

    public static Fin<byte[]> Write(Seq<BcfTopic> topics) =>
        Try.lift(() => Encode(topics)).Run().MapFail(static error => new BimFault.ModelRejected($"bcf-write:{error.Message}").ToError());

    static Seq<BcfTopic> Decode(ReadOnlyMemory<byte> bcfzip) {
        using var stream = new MemoryStream(bcfzip.ToArray());
        var bcf = new Worker().BcfFromStream(stream).GetAwaiter().GetResult();
        return bcf.Markups.ToSeq().Map(TopicOf);
    }

    static BcfTopic TopicOf(Bcf30.Markup markup) {
        var topic = markup.Topic;
        return new BcfTopic(
            topic.Guid, topic.Title,
            Enum.TryParse<BcfStatus>(topic.TopicStatus?.Replace(" ", ""), true, out var status) ? status : BcfStatus.Open,
            topic.TopicType ?? "", topic.Priority ?? "", topic.CreationAuthor ?? "", Instant.FromDateTimeUtc(DateTime.SpecifyKind(topic.CreationDate, DateTimeKind.Utc)),
            markup.Comment.ToSeq().Map(CommentOf),
            markup.Viewpoints.ToSeq().Map(ViewpointOf));
    }

    static BcfComment CommentOf(Bcf30.Comment comment) =>
        new(comment.Guid, comment.Author ?? "", comment.CommentProperty ?? "",
            Optional(comment.Viewpoint?.Guid), Instant.FromDateTimeUtc(DateTime.SpecifyKind(comment.Date, DateTimeKind.Utc)));

    static BcfViewpoint ViewpointOf(Bcf30.ViewPoint reference) {
        var visualization = reference.VisualizationInfo;
        var camera = visualization?.PerspectiveCamera;
        return new BcfViewpoint(
            reference.Guid,
            Vec(camera?.CameraViewPoint), Vec(camera?.CameraDirection), Vec(camera?.CameraUpVector),
            camera?.FieldOfView ?? 60d,
            Optional(visualization?.Components?.Selection).Map(s => s.Map(static c => c.IfcGuid).ToSeq()).IfNone(Seq<string>()),
            Seq<string>(),
            Option<ReadOnlyMemory<byte>>.None);
    }

    static System.Numerics.Vector3 Vec(Bcf30.Point? p) => p is null ? default : new((float)p.X, (float)p.Y, (float)p.Z);

    static byte[] Encode(Seq<BcfTopic> topics) {
        var builder = topics.Fold(new BcfBuilder().WithDefaults(), static (b, topic) => b.AddMarkup(markup => {
            markup.SetGuid(topic.Guid).SetTitle(topic.Title).SetTopicType(topic.TopicType)
                .SetTopicStatus(topic.Status.ToString()).SetPriority(topic.Priority).SetCreationAuthor(topic.Author)
                .SetCreationDate(topic.CreationDate.ToDateTimeUtc());
            topic.Comments.Iter(c => markup.AddComment(comment => comment
                .SetGuid(c.Guid).SetAuthor(c.Author).SetComment(c.Text).SetDate(c.Date.ToDateTimeUtc())
                .SetViewPointGuid(c.ViewpointGuid.MatchUnsafe(g => g, () => null))));
            topic.Viewpoints.Iter(v => markup.AddViewPoint(vp => vp.SetGuid(v.Guid)));
        }));
        using var stream = new Worker().ToBcf(builder.Build(), BcfVersionEnum.Bcf30).GetAwaiter().GetResult();
        using var sink = new MemoryStream();
        stream.CopyTo(sink);
        return sink.ToArray();
    }
}
```

## [03]-[TS_PROJECTION]

- Owner: `BcfWire` the host-free JSON wire producer of the `[2]-[BCF_ARCHIVE]` topic family — `BcfWire.Topics` the projected `Seq<BcfTopicWire>` payload carrying the topic/comment/viewpoint graph the `ts:ui/bcf-anchor` panel decodes, `BcfTopicWire`/`BcfCommentWire`/`BcfViewpointWire` the wire records mirroring the codec records with the `BcfStatus` string-stable discriminant and the IFC-GUID component anchor; `BcfWire.Encode`/`Decode` the `Exchange/wire#WIRE_PROJECTION` `BimWireOptions.Json`-bound codec so the BCF payload rides the same source-generated `BimWireContext` and `ThinktectureJsonConverterFactory` machinery the model snapshot rides, never a second serializer.
- Entry: `BcfWire.Encode(Seq<BcfTopic> topics)` projects the codec topic set onto the wire payload and `BcfWire.Decode(ReadOnlyMemory<byte> json)` admits it back — `Fin<T>` aborts on a malformed payload (`Model/faults#FAULT_BAND` `BimFault.ModelRejected`) lowered with `.ToError()` at the `Boundary` funnel, the identical `Try.lift(...).Run().MapFail(...)` funnel the `[2]-[BCF_ARCHIVE]` codec carries; `BcfWire.Anchor(BcfViewpoint viewpoint)` projects the viewpoint `SelectedGlobalIds`/`VisibleGlobalIds` onto the element-GlobalId set the UI live-binding highlights, so a TS pick round-trips the exact `Model/elements#ELEMENT_MODEL` `GlobalId` selection the C# viewpoint carries.
- Auto: `Encode` serializes through `BimWireOptions.Json` — the `BcfStatus` enum serializes by its `[JsonStringEnumMemberName]` lower-kebab name (`open`/`in-progress`/`resolved`/`closed`/`reopened`) so a TS discriminant switches on the string rather than the ordinal, the `Instant` creation/comment dates serialize through the `BimWireContext` `Instant` metadata as `InstantPattern.ExtendedIso` ISO-8601 text, the `System.Numerics.Vector3` camera triplet serializes as the host-free `{X,Y,Z}` object the TS camera binding reads, and the `SelectedGlobalIds`/`VisibleGlobalIds` `Seq<string>` serialize as the element-GlobalId arrays the live-binding anchors on; `Decode` re-admits every owner through the same converter factory so a malformed `Instant` or a half-built record faults at admission rather than minting a partial topic.
- Receipt: the `BcfWire.Topics` payload is the one BCF cross-runtime contract — the `ts:ui/bcf-anchor` panel and live-binding decode the same `BcfTopic`/`BcfComment`/`BcfViewpoint` vocabulary the C# branch mints, never re-minting a parallel issue shape; a viewpoint anchors on the `Model/elements#ELEMENT_MODEL` `GlobalId` so the TS selection highlight and the C# component selection carry one element identity, and the `BcfApi` REST JSON and this wire payload carry the one `BcfTopicWire` vocabulary so the file, REST, and web forms never fork.
- Packages: Thinktecture.Runtime.Extensions.Json, Thinktecture.Runtime.Extensions, NodaTime, LanguageExt.Core, Rasm, BCL `System.Text.Json`
- Growth: a new BCF entity on the wire is one `[JsonSerializable]` row on the `Exchange/wire#WIRE_PROJECTION` `BimWireContext` and one wire record mirroring its codec record; a new topic field is one column on `BcfTopicWire`; the TS panel decodes the new column with no second wire vocabulary; never a second serializer beside `BimWireOptions.Json`.
- Boundary: `BcfWire` is HOST-FREE — it carries no RhinoCommon type and no `ZipArchive`/`XDocument` codec surface, only the host-free record graph and the BCL `System.Numerics.Vector3`; it rides the `Exchange/wire#WIRE_PROJECTION` `BimWireOptions.Json` and the source-generated `BimWireContext`, and a second `JsonSerializerOptions` or a hand-authored DTO mirror is the deleted form — the BCF rows are `[JsonSerializable]` rows on the one `BimWireContext`; the `BcfStatus` discriminant is the `[JsonStringEnumMemberName]` string so a TS decode switches on a stable name and an ordinal-keyed enum crossing the wire is the named seam violation; the viewpoint anchors on the `Model/elements#ELEMENT_MODEL` `GlobalId` so the payload aligns to element GlobalIds only, never to a geometry handle; the `.bcfzip` archive and the `BcfApi` REST forms project onto this one wire family so the issue vocabulary is minted once in C# and decoded by the TS peer, never re-minted — a TS-side parallel topic shape is the named cross-language drift defect.

```csharp signature
namespace Rasm.Bim.Coordination;

public enum BcfStatus : byte {
    [JsonStringEnumMemberName("open")]        Open = 0,
    [JsonStringEnumMemberName("in-progress")] InProgress = 1,
    [JsonStringEnumMemberName("resolved")]    Resolved = 2,
    [JsonStringEnumMemberName("closed")]      Closed = 3,
    [JsonStringEnumMemberName("reopened")]    Reopened = 4,
}

public sealed record BcfViewpointWire(
    string Guid,
    System.Numerics.Vector3 CameraPosition,
    System.Numerics.Vector3 CameraDirection,
    System.Numerics.Vector3 CameraUpVector,
    double FieldOfView,
    Seq<string> SelectedGlobalIds,
    Seq<string> VisibleGlobalIds);

public sealed record BcfCommentWire(
    string Guid,
    string Author,
    string Text,
    Option<string> ViewpointGuid,
    Instant Date);

public sealed record BcfTopicWire(
    string Guid,
    string Title,
    BcfStatus Status,
    string TopicType,
    string Priority,
    string Author,
    Instant CreationDate,
    Seq<BcfCommentWire> Comments,
    Seq<BcfViewpointWire> Viewpoints);

public sealed record BcfWire(Seq<BcfTopicWire> Topics) {
    public static Fin<byte[]> Encode(Seq<BcfTopic> topics) =>
        Try.lift(() => JsonSerializer.SerializeToUtf8Bytes(new BcfWire(topics.Map(Project)), BimWireOptions.Json)).Run()
            .MapFail(static error => new BimFault.ModelRejected($"bcf-wire-encode:{error.Message}").ToError());

    public static Fin<BcfWire> Decode(ReadOnlyMemory<byte> json) =>
        Try.lift(() => JsonSerializer.Deserialize<BcfWire>(json.Span, BimWireOptions.Json)!).Run()
            .MapFail(static error => new BimFault.ModelRejected($"bcf-wire-decode:{error.Message}").ToError());

    public static Seq<string> Anchor(BcfViewpoint viewpoint) => viewpoint.SelectedGlobalIds;

    static BcfTopicWire Project(BcfTopic topic) =>
        new(topic.Guid, topic.Title, topic.Status, topic.TopicType, topic.Priority, topic.Author, topic.CreationDate,
            topic.Comments.Map(static c => new BcfCommentWire(c.Guid, c.Author, c.Text, c.ViewpointGuid, c.Date)),
            topic.Viewpoints.Map(static v => new BcfViewpointWire(
                v.Guid, v.CameraPosition, v.CameraDirection, v.CameraUpVector, v.FieldOfView,
                v.SelectedGlobalIds, v.VisibleGlobalIds)));
}
```

## [04]-[RESEARCH]

- [BCF_TOOLKIT_CODEC]: the `Smino.Bcf.Toolkit` 3.2.0 codec surface the `BcfArchive` composes is decompile-verified at `.api/api-smino-bcf-toolkit` — `Worker.BcfFromStream(Stream)` → `Bcf`, `Worker.ToBcf(IBcf, BcfVersionEnum)` → `Stream`, `BcfExtensions.GetVersionFromStreamArchive` the version sniff, the `BcfBuilder.AddMarkup(Action<MarkupBuilder>)`/`WithDefaults()`/`Build()` authoring fold, and the nested `MarkupBuilder.SetGuid`/`SetTitle`/`SetTopicStatus`/`SetPriority`/`SetCreationAuthor`/`SetCreationDate`/`AddComment`/`AddViewPoint`, `CommentBuilder.SetGuid`/`SetAuthor`/`SetComment`/`SetDate`/`SetViewPointGuid`, and `ViewPointBuilder.SetGuid`/`SetViewPoint` setters; the read projection narrows from `IBcf`→`Bcf30.Markup` (`Topic` + comment/viewpoint sets) then reads the concrete `Bcf30.Topic` (`Guid`/`Title`/`TopicStatus`/`TopicType`/`Priority`/`CreationAuthor`/`CreationDate`/`Description`) and `Bcf30.Comment` (`Author`/`CommentProperty`/`Date`/`Viewpoint`/`Guid`) detail columns the thin `ITopic`/`IComment` markers do not carry, so version selection is one `BcfVersionEnum` and never a forked projection; `BcfVersion.TryParse` accepts only `"2.1"`/`"3.0"` (BCF 2.0 input rejects `Unsupported BCF version`), so the codec's real coverage is BCF 2.1 + 3.0 and the hand-rolled `ZipArchive`/`XDocument` container is retired.
- [BCF_API_REST]: the BCF-API 3.0 REST resource shape — the topic/comment/viewpoint endpoint vocabulary and the JSON projection of the same topic family — grounds against the published buildingSMART BCF-API 3.0 specification so the `BcfApi.Project` request shape matches the REST resource model; the request issues over the `csharp:Compute/Runtime/channels#TRANSPORT_AXIS` transport at cross-folder alignment, never a transport minted here, and the file and REST forms carry one `BcfTopic` vocabulary.
- [HOST_FREE_VECTOR_COMPOSE]: the `BcfViewpoint`/`BcfViewpointWire` camera triplet is the BCL `System.Numerics.Vector3` — the one host-free 3-vector the HOST-FREE BCF record family admits (the kernel `Rasm/Vectors` owner exposes only the host-bound RhinoCommon `Vector3d`, which a host-free coordination record may never carry), so the codec record, the wire record, and the `csharp:Rasm.AppUi/Editing/issues` board projection's `BcfViewpoint` construction all name `System.Numerics.Vector3` and a BCF-local or host-bound coordinate type is the deleted form.
