# [BIM_ISSUE_EXCHANGE]

The BCF 3.0 issue-exchange owner: one closed `BcfTopic`/`BcfComment`/`BcfViewpoint` record family anchored on IFC GlobalIds, a self-owned `.bcfzip` archive codec over the BCL `System.IO.Compression` `ZipArchive` surface and the BCF markup XML over the BCL `System.Xml` surface, plus a `BcfApi` REST projection riding Compute's transport. BCF is an issue/coordination container, never a geometry-or-model interchange row — the `.bcfzip` codec is self-owned in `coordination` and is NOT a row on the `Exchange/format#FORMAT_AXIS` geometry-format axis, because BCF carries issues and viewpoints, never a `BimModel` or `ImportedGeometry` import. Viewpoints anchor on the `Model/elements#ELEMENT_MODEL` `GlobalId`, so the issue payload aligns to element GlobalIds only, never to the geometry codec axis. The page composes the `Model/elements#ELEMENT_MODEL` `BimModel` GlobalIds and the `csharp:Compute/Runtime/channels#TRANSPORT_AXIS` transport as settled vocabulary. The page is HOST-LOCAL.

## [01]-[INDEX]

- [01]-[BCF_ARCHIVE]: `BcfTopic`/`BcfComment`/`BcfViewpoint` record family, the `.bcfzip` codec over `ZipArchive`, and the `BcfApi` REST projection.
- [02]-[TS_PROJECTION]: the `BcfWire` host-free JSON producer the TS UI BCF anchor and live-binding decode — the topic/comment/viewpoint payload over the source-generated `BimWireContext` with the `BcfStatus` string discriminant and the `IfcGuid` component anchor.

## [02]-[BCF_ARCHIVE]

- Owner: `BcfTopic` the issue record anchored on its own GUID carrying title/status/type/priority/author and the comment and viewpoint sets; `BcfComment` the threaded comment record; `BcfViewpoint` the camera-and-selection record anchored on IFC GlobalId component selection and clipping; `BcfArchive` the `.bcfzip` codec reading and writing the BCF container over `ZipArchive`; `BcfApi` the REST projection of the same topic family riding Compute's transport.
- Entry: `BcfArchive.Read(ReadOnlyMemory<byte> bcfzip)` folds the `.bcfzip` archive entries (`bcf.version`, `markup.bcf`, `viewpoint.bcfv`, `snapshot.png`) into the typed `BcfTopic` set, and `BcfArchive.Write(Seq<BcfTopic> topics)` emits the `.bcfzip` bytes — `Fin<T>` aborts on a malformed archive or a markup XML the BCF 3.0 schema rejects (`Model/faults#FAULT_BAND` `BimFault.ModelRejected`) lowered with `.ToError()` at the `Boundary` funnel; `BcfApi.Project(BcfTopic topic)` builds the BCF-API REST request the Compute transport issues, never a transport minted here.
- Auto: `Read` opens the archive through `ZipArchive(stream, ZipArchiveMode.Read)`, reads each topic folder's `markup.bcf` through `XDocument.Load` into the `BcfTopic`/`BcfComment` records, parses each `viewpoint.bcfv` camera-and-selection into `BcfViewpoint` anchoring the component selection on the IFC GlobalId, and projects the `snapshot.png` entry as the viewpoint thumbnail; `Write` builds the archive entries in reverse, emitting one topic folder per `BcfTopic` with its `markup.bcf` XML and `viewpoint.bcfv` camera; the `BcfViewpoint` component selection carries IFC-GUID `IfcGuid` component visibility and clipping so a viewer round-trips the exact element selection.
- Receipt: the `BcfTopic` set is the coordination evidence; a CDE or viewer round-trips it through `.bcfzip` and the `BcfApi` REST projection rides the same topic family so the file and REST forms carry one vocabulary, never two.
- Packages: NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm, BCL `System.IO.Compression`/`System.Xml`/`System.Xml.Linq`
- Growth: a new BCF entity is one record on the topic family; a new BCF version is one column on `BcfArchive` reading the `bcf.version` entry; the REST projection is one `BcfApi` shape over Compute's transport; never a row on the geometry-format axis and never a second issue store.
- Boundary: the `.bcfzip` codec is self-owned in `coordination` and is NOT a row on the `Exchange/format#FORMAT_AXIS` geometry-format axis — coupling the issue container to the geometry codec axis is the rejected form because BCF carries issues and viewpoints, never a `BimModel` or `ImportedGeometry` import; the archive read/write rides the BCL `System.IO.Compression` `ZipArchive` surface and the BCF markup the BCL `System.Xml.Linq` `XDocument` surface — a hand-rolled zip or XML writer is the deleted form; viewpoints anchor on the `Model/elements#ELEMENT_MODEL` `GlobalId` so the issue payload aligns to element GlobalIds only, never to a geometry handle; the BCF-API REST shape rides the `csharp:Compute/Runtime/channels#TRANSPORT_AXIS` transport and a transport minted here is the named seam violation; the durable annotation + CDE op-log store is the app-platform `csharp:Rasm.Persistence/Sync/annotation` owner's concern, joined at the `coordination ⇄ csharp:Rasm.Persistence/Sync/annotation # [WIRE]: durable annotation + CDE op-log` seam through the `csharp:Rasm.Persistence/Query/federation#ENTITY_GRAPH` key — a `BcfViewpoint.SelectedGlobalIds`/`VisibleGlobalIds` IFC `GlobalId` resolves to the `FederatedEntity` whose `EntityIdentity.Origin` the durable annotation `Anchor` carries, so a BCF topic and a durable annotation bind one element without this owner gaining a durable-op-log row or Persistence gaining a second BCF schema; `BcfArchive` is the boundary capsule and its codec body carries the language-owned `using var` archive statement the `ZipArchive` decode requires.

```csharp signature
// The BCF/coordination DOMAIN owner namespace — the `coordination` sub-domain the ARCHITECTURE seams name
// and AppUi `Editing/issues` consumes as `Rasm.Bim.Coordination.BcfTopic`/`BcfComment`/`BcfViewpoint`/`BcfStatus`;
// the codemap `Review/Coordination.cs`/`Review/Issues.cs` files emit this namespace, not folder-derived `Review`.
namespace Rasm.Bim.Coordination;

public sealed record BcfViewpoint(
    string Guid,
    Vector3 CameraPosition,
    Vector3 CameraDirection,
    Vector3 CameraUpVector,
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

public static class BcfArchive {
    public static Fin<Seq<BcfTopic>> Read(ReadOnlyMemory<byte> bcfzip) =>
        Try.lift(() => Decode(bcfzip)).Run().MapFail(static error => new BimFault.ModelRejected($"bcf-archive:{error.Message}").ToError());

    public static Fin<byte[]> Write(Seq<BcfTopic> topics) =>
        Try.lift(() => Encode(topics)).Run().MapFail(static error => new BimFault.ModelRejected($"bcf-write:{error.Message}").ToError());

    static Seq<BcfTopic> Decode(ReadOnlyMemory<byte> bcfzip) {
        using var archive = new ZipArchive(new MemoryStream(bcfzip.ToArray()), ZipArchiveMode.Read);
        return archive.Entries
            .Filter(static entry => entry.FullName.EndsWith("markup.bcf", StringComparison.OrdinalIgnoreCase))
            .Map(entry => TopicOf(entry, archive))
            .ToSeq();
    }

    static BcfTopic TopicOf(ZipArchiveEntry markup, ZipArchive archive) {
        using var stream = markup.Open();
        var doc = XDocument.Load(stream);
        var topic = doc.Root!.Element("Topic")!;
        string folder = markup.FullName[..markup.FullName.LastIndexOf('/')];
        return new BcfTopic(
            topic.Attribute("Guid")?.Value ?? "",
            topic.Element("Title")?.Value ?? "",
            Enum.TryParse<BcfStatus>(topic.Attribute("TopicStatus")?.Value, true, out var status) ? status : BcfStatus.Open,
            topic.Attribute("TopicType")?.Value ?? "",
            topic.Element("Priority")?.Value ?? "",
            topic.Element("CreationAuthor")?.Value ?? "",
            topic.Element("CreationDate")?.Value is { } date ? InstantPattern.ExtendedIso.Parse(date).Value : default,
            doc.Root.Elements("Comment").Map(CommentOf).ToSeq(),
            doc.Root.Elements("Viewpoints").Map(vp => ViewpointOf(vp, folder, archive)).ToSeq());
    }

    static BcfComment CommentOf(XElement el) =>
        new(el.Attribute("Guid")?.Value ?? "", el.Element("Author")?.Value ?? "", el.Element("Comment")?.Value ?? "",
            Optional(el.Element("Viewpoint")?.Attribute("Guid")?.Value),
            el.Element("Date")?.Value is { } d ? InstantPattern.ExtendedIso.Parse(d).Value : default);

    static BcfViewpoint ViewpointOf(XElement reference, string folder, ZipArchive archive) {
        string file = reference.Element("Viewpoint")?.Value ?? "";
        using var stream = archive.GetEntry($"{folder}/{file}")!.Open();
        var doc = XDocument.Load(stream);
        var camera = doc.Root!.Element("PerspectiveCamera")!;
        return new BcfViewpoint(
            reference.Attribute("Guid")?.Value ?? "",
            VectorOf(camera.Element("CameraViewPoint")!), VectorOf(camera.Element("CameraDirection")!), VectorOf(camera.Element("CameraUpVector")!),
            double.Parse(camera.Element("FieldOfView")?.Value ?? "60", System.Globalization.CultureInfo.InvariantCulture),
            doc.Root.Descendants("Component").Choose(c => Optional(c.Attribute("IfcGuid")?.Value)).ToSeq(),
            Seq<string>(),
            Option<ReadOnlyMemory<byte>>.None);
    }

    static Vector3 VectorOf(XElement el) => new(
        double.Parse(el.Element("X")?.Value ?? "0", System.Globalization.CultureInfo.InvariantCulture),
        double.Parse(el.Element("Y")?.Value ?? "0", System.Globalization.CultureInfo.InvariantCulture),
        double.Parse(el.Element("Z")?.Value ?? "0", System.Globalization.CultureInfo.InvariantCulture));

    static byte[] Encode(Seq<BcfTopic> topics) {
        using var sink = new MemoryStream();
        using (var archive = new ZipArchive(sink, ZipArchiveMode.Create, leaveOpen: true)) {
            WriteEntry(archive, "bcf.version", "<Version VersionId=\"3.0\" />");
            topics.Iter(topic => WriteEntry(archive, $"{topic.Guid}/markup.bcf", Markup(topic)));
        }
        return sink.ToArray();
    }

    static void WriteEntry(ZipArchive archive, string path, string content) {
        using var stream = archive.CreateEntry(path).Open();
        using var writer = new StreamWriter(stream);
        writer.Write(content);
    }

    static string Markup(BcfTopic topic) =>
        new XDocument(new XElement("Markup", new XElement("Topic",
            new XAttribute("Guid", topic.Guid), new XAttribute("TopicType", topic.TopicType), new XAttribute("TopicStatus", topic.Status.ToString()),
            new XElement("Title", topic.Title), new XElement("Priority", topic.Priority), new XElement("CreationAuthor", topic.Author),
            new XElement("CreationDate", InstantPattern.ExtendedIso.Format(topic.CreationDate))))).ToString();
}
```

## [03]-[TS_PROJECTION]

- Owner: `BcfWire` the host-free JSON wire producer of the `[2]-[BCF_ARCHIVE]` topic family — `BcfWire.Topics` the projected `Seq<BcfTopicWire>` payload carrying the topic/comment/viewpoint graph the `ts:ui/bcf-anchor` panel decodes, `BcfTopicWire`/`BcfCommentWire`/`BcfViewpointWire` the wire records mirroring the codec records with the `BcfStatus` string-stable discriminant and the IFC-GUID component anchor; `BcfWire.Encode`/`Decode` the `Exchange/wire#WIRE_PROJECTION` `BimWireOptions.Json`-bound codec so the BCF payload rides the same source-generated `BimWireContext` and `ThinktectureJsonConverterFactory` machinery the model snapshot rides, never a second serializer.
- Entry: `BcfWire.Encode(Seq<BcfTopic> topics)` projects the codec topic set onto the wire payload and `BcfWire.Decode(ReadOnlyMemory<byte> json)` admits it back — `Fin<T>` aborts on a malformed payload (`Model/faults#FAULT_BAND` `BimFault.ModelRejected`) lowered with `.ToError()` at the `Boundary` funnel, the identical `Try.lift(...).Run().MapFail(...)` funnel the `[2]-[BCF_ARCHIVE]` codec carries; `BcfWire.Anchor(BcfViewpoint viewpoint)` projects the viewpoint `SelectedGlobalIds`/`VisibleGlobalIds` onto the element-GlobalId set the UI live-binding highlights, so a TS pick round-trips the exact `Model/elements#ELEMENT_MODEL` `GlobalId` selection the C# viewpoint carries.
- Auto: `Encode` serializes through `BimWireOptions.Json` — the `BcfStatus` enum serializes by its `[JsonStringEnumMemberName]` lower-kebab name (`open`/`in-progress`/`resolved`/`closed`/`reopened`) so a TS discriminant switches on the string rather than the ordinal, the `Instant` creation/comment dates serialize through the `BimWireContext` `Instant` metadata as `InstantPattern.ExtendedIso` ISO-8601 text, the `Vector3` camera triplet serializes as the kernel `{X,Y,Z}` object the TS camera binding reads, and the `SelectedGlobalIds`/`VisibleGlobalIds` `Seq<string>` serialize as the element-GlobalId arrays the live-binding anchors on; `Decode` re-admits every owner through the same converter factory so a malformed `Instant` or a half-built record faults at admission rather than minting a partial topic.
- Receipt: the `BcfWire.Topics` payload is the one BCF cross-runtime contract — the `ts:ui/bcf-anchor` panel and live-binding decode the same `BcfTopic`/`BcfComment`/`BcfViewpoint` vocabulary the C# branch mints, never re-minting a parallel issue shape; a viewpoint anchors on the `Model/elements#ELEMENT_MODEL` `GlobalId` so the TS selection highlight and the C# component selection carry one element identity, and the `BcfApi` REST JSON and this wire payload carry the one `BcfTopicWire` vocabulary so the file, REST, and web forms never fork.
- Packages: Thinktecture.Runtime.Extensions.Json, Thinktecture.Runtime.Extensions, NodaTime, LanguageExt.Core, Rasm, BCL `System.Text.Json`
- Growth: a new BCF entity on the wire is one `[JsonSerializable]` row on the `Exchange/wire#WIRE_PROJECTION` `BimWireContext` and one wire record mirroring its codec record; a new topic field is one column on `BcfTopicWire`; the TS panel decodes the new column with no second wire vocabulary; never a second serializer beside `BimWireOptions.Json`.
- Boundary: `BcfWire` is HOST-FREE — it carries no RhinoCommon type and no `ZipArchive`/`XDocument` codec surface, only the host-free record graph and the kernel `Vector3`; it rides the `Exchange/wire#WIRE_PROJECTION` `BimWireOptions.Json` and the source-generated `BimWireContext`, and a second `JsonSerializerOptions` or a hand-authored DTO mirror is the deleted form — the BCF rows are `[JsonSerializable]` rows on the one `BimWireContext`; the `BcfStatus` discriminant is the `[JsonStringEnumMemberName]` string so a TS decode switches on a stable name and an ordinal-keyed enum crossing the wire is the named seam violation; the viewpoint anchors on the `Model/elements#ELEMENT_MODEL` `GlobalId` so the payload aligns to element GlobalIds only, never to a geometry handle; the `.bcfzip` archive and the `BcfApi` REST forms project onto this one wire family so the issue vocabulary is minted once in C# and decoded by the TS peer, never re-minted — a TS-side parallel topic shape is the named cross-language drift defect.

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
    Vector3 CameraPosition,
    Vector3 CameraDirection,
    Vector3 CameraUpVector,
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

- [BCF_MARKUP_SCHEMA]: the BCF 3.0 `markup.bcf` / `viewpoint.bcfv` / `bcf.version` XML element grammar — the `Markup`/`Topic`/`Comment`/`Viewpoints` structure, the `PerspectiveCamera`/`OrthogonalCamera`/`Components`/`Component` viewpoint sub-grammar with the IFC-GUID `IfcGuid` component anchor, the `Visibility`/`Selection`/`ClippingPlanes` selection vocabulary, and the topic `TopicStatus`/`TopicType`/`Priority` enumerations — grounds against the published buildingSMART BCF-XML 3.0 schema so the `XDocument` element-name projection matches the container layout before the codec body is final; the `ZipArchive`/`XDocument` BCL surfaces are settled inbox.
- [BCF_API_REST]: the BCF-API 3.0 REST resource shape — the topic/comment/viewpoint endpoint vocabulary and the JSON projection of the same topic family — grounds against the published buildingSMART BCF-API 3.0 specification so the `BcfApi.Project` request shape matches the REST resource model; the request issues over the `csharp:Compute/Runtime/channels#TRANSPORT_AXIS` transport at cross-folder alignment, never a transport minted here, and the file and REST forms carry one `BcfTopic` vocabulary.
- [KERNEL_VECTOR_COMPOSE]: the kernel `Rasm` `Vector3` member spelling the `BcfViewpoint` camera carries confirms against the kernel `Rasm` vector owner at cross-folder alignment so the viewpoint camera reuses the kernel vector rather than a host-bound or BCF-local coordinate type.
