# [PERSISTENCE_INGEST_ISSUE]

Rasm.Persistence ingests and emits BCF issue files through ONE `IssueSource` owner — the `[A.4]` Ingest growth row made real a second time: the buildingSMART BCF container (`.bcfzip` — a zip whose root carries `bcf.version`, `project.bcfp`, and the vocabulary extensions, and whose one-folder-per-topic-GUID layout carries `markup.bcf`, `*.bcfv` viewpoint files, and snapshot images) decodes into typed topic, comment, and viewpoint rows over BCL surfaces alone (`ZipArchive` the container, `XDocument` the markup), so a review cycle that today lives in a foreign coordination tool round-trips through the durable spine: a clash `ElementSet` leaves as topics, resolutions come back as status moves and comments, and `IssueRows.Reconcile` partitions the cycle's drift. `IssueVersion` closes the wire dialects (`2.1` reads extensions off the `project.bcfp` `ExtensionSchema`, `3.0` off root `extensions.xml`), and the topic vocabularies — type, status, priority, stage, label — are PROJECT-EXTENSIBLE by the BCF schema itself, so they admit as a runtime-sourced `IssueVocabulary` registry (keyed axis rows + frozen per-project sets), never a compiled enum a foreign project's status list breaks.

Element references cross as IFC GlobalIds — exactly the `Element/identity#ELEMENT_IDENTITY` `GlobalIds` mirror — so every viewpoint component resolves one hop to a durable `NodeId` through the injected resolve port, an unresolved GlobalId stays a carried FACT (a cross-model or retired reference is normal review reality, never a fault), and a topic's referenced components project to the `Query/lane#ELEMENT_SET_ALGEBRA` `ElementSet` currency the clash/IDS/QTO surfaces already compose. Wire stamps carry their explicit offsets as NodaTime `OffsetDateTime` (BCF dates are zoned `xs:dateTime`; a fabricated UTC collapse is the deleted form), snapshot bytes content-address through `Element/codec` `ContentAddress` with residence left to the blob plane, and this codec NEVER computes review logic: rows project to `Rasm.Element` and land durably at the app composition root, the same row-shape law every Ingest sibling obeys. `IssueFault` closes the accumulating band (`FaultBand.Issue`), facts ride `store.issue.*`, `Origin` arrives from `Ingest/tabular#TABULAR_SOURCE`, and `ProjectionContext` from `Element/graph#STORE_RAIL`.

## [01]-[INDEX]

- [01]-[ISSUE_CODEC]: the `.bcfzip` container codec — the version gate, the per-topic zip layout, the runtime-sourced vocabulary registry, the closed ingest/egress/probe op family, the accumulating fault band, and the typed fact stream.
- [02]-[ISSUE_ROWS]: the topic/comment/viewpoint row family at full BCF axis depth, the camera and component decode, the GlobalId→`NodeId` correlation and `ElementSet` projection, and the `Reconcile` issue-cycle diff.

## [02]-[ISSUE_CODEC]

- Owner: `IssueVersion` the closed wire-dialect axis carrying each dialect's vocabulary residence; `VocabularyAxis` the closed extensible-axis vocabulary; `IssueVocabulary` the per-project frozen registry runtime-admitted from the container's declared extensions; `IssueSpec` the read descriptor fixing source, correlation port, and declared-vocabulary posture; `IssueOp`/`IssueYield` the closed dispatch pair; `IssueFault` the accumulating band; `IssueFactKind`/`IssueFact` the receipt stream; `IssueSource` owning `Run`.
- Cases: `IssueOp.Ingest(IssueSpec)` decodes the container into `Seq<IssueTopic>`; `IssueOp.Egress(IssueSpec, Seq<IssueTopic>)` writes one `.bcfzip` — `bcf.version` stamped from the spec's dialect, one folder per topic GUID, `markup.bcf` per topic, one `.bcfv` per viewpoint, the declared vocabulary emitted at its dialect's residence; `IssueOp.Probe(IssueSpec)` yields the `IssueSurvey` (dialect, topic count, declared vocabulary roster) without decoding topic bodies. `IssueFault` is `UnknownVersion | CodecReject | TopicMalformed | ViewpointMalformed | Aggregate` (`8471`-`8475`). `IssueFactKind` closes `ingest | egress | probe`.
- Entry: `public static IO<Validation<IssueFault, IssueYield>> Run(IssueOp op, ProjectionContext frame, Func<IssueFact, IO<Unit>> sink)` — ONE polymorphic entry over the closed op union through the generated total `Switch`; per-topic decode failures accumulate through the `Semigroup` so one malformed topic never discards a hundred sound ones, and the correlation is the row's own member, never a second entry.
- Auto: the version gate reads root `bcf.version` (`Version` element, `VersionId` attribute) FIRST and rails `UnknownVersion` on a dialect outside the closed axis — no best-effort parse of an unversioned container; vocabulary admission follows the dialect row (`2.1` reads the `project.bcfp` `ExtensionSchema` XSD restriction values, `3.0` reads root `extensions.xml` `TopicTypes`/`TopicStatuses`/`Priorities`/`TopicLabels`/`Stages`/`Users`/`SnippetTypes`) into one `IssueVocabulary` of frozen per-axis sets; a topic value OUTSIDE the declared set is a carried fact on the ingest receipt (the BCF vocabularies are advisory declarations, and round-trip fidelity outranks local taste), never a mutation and never a fault; topic folders key by the topic GUID and a folder whose `markup.bcf` `Topic` `Guid` disagrees with its folder name rails `TopicMalformed` naming both; egress re-emits byte-faithful vocabulary declarations and preserves every unrecognized topic value so a Persistence round trip never launders a foreign tool's state.
- Receipt: every op rides an `IssueFact` under `store.issue.*` — an `ingest` fact carrying the dialect, topic/comment/viewpoint counts, the unresolved-component count, and the foreign-vocabulary count; an `egress` fact carrying the dialect and topic count; a `probe` fact carrying the survey — one kind-discriminated stream stamped `frame.Now()`.
- Packages: BCL inbox (`System.IO.Compression.ZipArchive`/`ZipArchiveEntry` — the container; `System.Xml.Linq.XDocument`/`XElement`/`XAttribute` — the markup wire; zero foreign packages: BCF is zip+XML by construction), Rasm.Element (`NodeId`), Rasm.Persistence (`Element/codec` `ContentAddress`, `Element/graph` `FaultBand`/`ProjectionContext`, `Ingest/tabular` `Origin`, `Query/lane` `ElementSet`), LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime, BCL inbox.
- Growth: a new wire dialect is one `IssueVersion` row carrying its vocabulary residence; a new extensible axis is one `VocabularyAxis` row; a new fault class is one case inside the registry decade; zero new surface — a per-dialect reader family, a compiled status/type/priority enum a foreign project breaks, a topic value silently normalized at ingest, a snapshot byte-copy held beside the content address, or a review-logic computation inside this codec is the deleted form because the dialect is a row, the vocabularies are runtime-admitted data, and the codec is a wire.
- Boundary: the codec owns exactly the wire — decode, encode, survey — and the durable landing is the app's (`Element/graph#STORE_RAIL` for row residence, `Store/blobstore` for snapshot bytes, both at the composition root per the Ingest row-shape law); GlobalId correlation is a one-hop injected port because the `GlobalIds` mirror is `Element/identity`'s (this page never opens a store connection); the BCF camera is VIEW state, not geometry — no NTS vocabulary enters (the geo interior is `Ingest/geospatial`'s), and the `IssueVector` triple stays a bare carrier the AppUi viewport consumes; `← Rasm.Bim` clash results mint topics through `Egress` (the coordination-review currency leaving), `→ Rasm.Bim` resolved topics return as status moves the planner reads through `Reconcile`; the `documents.xml` document catalog and server-side BCF-API live OUTSIDE this file codec — a REST review server is a foreign system whose export lands here as the same `.bcfzip`.

```csharp signature
using System.IO.Compression;
using System.Xml.Linq;
using NodaTime.Text;
using Rasm.Persistence.Element;
using Expected = Rasm.Domain.Expected;

namespace Rasm.Persistence.Ingest;

// --- [TYPES] ----------------------------------------------------------------------------
// The closed wire-dialect axis; each row carries WHERE its vocabulary declarations reside.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class IssueVersion {
    public static readonly IssueVersion Bcf21 = new("2.1", vocabularyEntry: "project.bcfp");
    public static readonly IssueVersion Bcf30 = new("3.0", vocabularyEntry: "extensions.xml");
    public string VocabularyEntry { get; }
    private IssueVersion(string key, string vocabularyEntry) : this(key) => VocabularyEntry = vocabularyEntry;
}

// The extensible-axis vocabulary: BCF projects declare their own value sets per axis, so the axis is
// closed and the VALUES are runtime data — the OWNER_CHOOSER runtime-sourced-vocabulary row.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class VocabularyAxis {
    public static readonly VocabularyAxis TopicType = new("TopicTypes");
    public static readonly VocabularyAxis TopicStatus = new("TopicStatuses");
    public static readonly VocabularyAxis Priority = new("Priorities");
    public static readonly VocabularyAxis Label = new("TopicLabels");
    public static readonly VocabularyAxis Stage = new("Stages");
    public static readonly VocabularyAxis User = new("Users");
    public static readonly VocabularyAxis SnippetType = new("SnippetTypes");
}

// --- [MODELS] ---------------------------------------------------------------------------
// The per-project frozen registry admitted from the container's declared extensions; a value outside
// a declared set is a carried FACT, never a mutation — round-trip fidelity outranks local taste.
public sealed record IssueVocabulary(HashMap<VocabularyAxis, FrozenSet<string>> Declared) {
    public static readonly IssueVocabulary Empty = new(HashMap<VocabularyAxis, FrozenSet<string>>());
    public bool Admits(VocabularyAxis axis, string value) =>
        Declared.Find(axis).Map(set => set.Contains(value)).IfNone(true);
    public int Foreign(IssueTopic topic) =>
        Seq(topic.Type.Map(v => (VocabularyAxis.TopicType, v)), topic.Status.Map(v => (VocabularyAxis.TopicStatus, v)),
            topic.Priority.Map(v => (VocabularyAxis.Priority, v)), topic.Stage.Map(v => (VocabularyAxis.Stage, v)))
            .Somes().Append(topic.Labels.Map(v => (VocabularyAxis.Label, v)))
            .Count(pair => !Admits(pair.Item1, pair.Item2));
}

public sealed record IssueSpec(IssueVersion Dialect, Origin Source, IssueVocabulary Declared, Func<string, Option<NodeId>> Resolve);

public readonly record struct IssueSurvey(IssueVersion Dialect, int Topics, IssueVocabulary Declared);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record IssueOp {
    private IssueOp() { }
    public sealed record Ingest(IssueSpec Spec) : IssueOp;
    public sealed record Egress(IssueSpec Spec, Seq<IssueTopic> Topics) : IssueOp;
    public sealed record Probe(IssueSpec Spec) : IssueOp;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record IssueYield {
    private IssueYield() { }
    public sealed record Topics(Seq<IssueTopic> Rows) : IssueYield;
    public sealed record Written(int Count) : IssueYield;
    public sealed record Survey(IssueSurvey Roster) : IssueYield;
}

// --- [ERRORS] ---------------------------------------------------------------------------
[Union]
public abstract partial record IssueFault : Expected, IValidationError<IssueFault>, Semigroup<IssueFault> {
    private IssueFault() : base() { }
    public sealed record UnknownVersion(string Found) : IssueFault;
    public sealed record CodecReject(string Entry, string Detail) : IssueFault;
    public sealed record TopicMalformed(string Topic, string Detail) : IssueFault;
    public sealed record ViewpointMalformed(string Viewpoint, string Detail) : IssueFault;
    public sealed record Aggregate(Seq<IssueFault> Faults) : IssueFault;

    public override int Code => FaultBand.Issue + Switch(
        unknownVersion:     static _ => 1,
        codecReject:        static _ => 2,
        topicMalformed:     static _ => 3,
        viewpointMalformed: static _ => 4,
        aggregate:          static _ => 5);

    public override string Message => Switch(
        unknownVersion:     static c => $"<issue-version:{c.Found}>",
        codecReject:        static c => $"<issue-codec:{c.Entry}:{c.Detail}>",
        topicMalformed:     static c => $"<issue-topic:{c.Topic}:{c.Detail}>",
        viewpointMalformed: static c => $"<issue-viewpoint:{c.Viewpoint}:{c.Detail}>",
        aggregate:          static c => $"<issue-aggregate:{c.Faults.Count}>");

    public override string Category => Switch(
        unknownVersion:     static _ => "Version",
        codecReject:        static _ => "Codec",
        topicMalformed:     static _ => "Topic",
        viewpointMalformed: static _ => "Viewpoint",
        aggregate:          static _ => "Aggregate");

    public static IssueFault Create(string message) => new CodecReject("wire", message);

    public IssueFault Combine(IssueFault rhs) => (this, rhs) switch {
        (Aggregate l, Aggregate r) => new Aggregate(l.Faults + r.Faults),
        (Aggregate l, _) => new Aggregate(l.Faults.Add(rhs)),
        (_, Aggregate r) => new Aggregate(this.Cons(r.Faults)),
        _ => new Aggregate(Seq(this, rhs)),
    };
}

// --- [OPERATIONS] -----------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class IssueFactKind {
    public static readonly IssueFactKind Ingest = new("store.issue.ingest");
    public static readonly IssueFactKind Egress = new("store.issue.egress");
    public static readonly IssueFactKind Probe = new("store.issue.probe");
}

public readonly record struct IssueFact(IssueFactKind Kind, IssueVersion Dialect, int Topics, int Comments, int Viewpoints, int Unresolved, int Foreign, Instant At);

public static class IssueSource {
    // The registry-mounted census derives from the kind vocabulary, whose keys already carry the full slot spelling.
    public static readonly Seq<StoreSlot> Slots =
        toSeq(IssueFactKind.Items).Map(static kind => StoreSlot.Create(kind.Key));

    // ONE polymorphic entry; the version gate runs before any topic body decodes, per-topic faults
    // accumulate through the Semigroup, and every arm lands one kind-discriminated fact on the sink.
    public static IO<Validation<IssueFault, IssueYield>> Run(IssueOp op, ProjectionContext frame, Func<IssueFact, IO<Unit>> sink) =>
        op.Switch(
            ingest: source => Decoded(source.Spec, frame, sink),
            egress: emit => Encoded(emit.Spec, emit.Topics, frame, sink),
            probe:  survey => Surveyed(survey.Spec, frame, sink));

    // Container decode: gate the dialect, admit the vocabulary at its dialect residence, then fold every
    // topic folder applicatively so per-topic refusals accumulate. The zip walk is the platform-forced seam.
    static IO<Validation<IssueFault, IssueYield>> Decoded(IssueSpec spec, ProjectionContext frame, Func<IssueFact, IO<Unit>> sink) =>
        IO.lift(() => {
            using ZipArchive container = Opened(spec.Source);
            return Gate(container, spec.Dialect).Bind(vocabulary =>
                toSeq(container.Entries)
                    .Filter(static entry => entry.FullName.EndsWith("markup.bcf", StringComparison.Ordinal))
                    .Traverse(entry => Topic(container, entry, spec.Resolve)).As()
                    .Map(topics => (Vocabulary: vocabulary, Topics: topics)));
        }).Bind(decoded => decoded.Match(
            Succ: outcome => sink(Fact(IssueFactKind.Ingest, spec, outcome.Topics, outcome.Vocabulary, frame))
                .Map(_ => Validation<IssueFault, IssueYield>.Success(new IssueYield.Topics(outcome.Topics))),
            Fail: fault => IO.pure(Validation<IssueFault, IssueYield>.Fail(fault))));

    // Container encode: bcf.version stamped from the dialect row, the vocabulary emitted at its residence,
    // one GUID-named folder per topic with markup.bcf and one .bcfv per viewpoint.
    static IO<Validation<IssueFault, IssueYield>> Encoded(IssueSpec spec, Seq<IssueTopic> topics, ProjectionContext frame, Func<IssueFact, IO<Unit>> sink) =>
        IO.lift(() => {
            using ZipArchive container = Opened(spec.Source, write: true);
            Stamp(container, "bcf.version", VersionXml(spec.Dialect));
            Stamp(container, spec.Dialect.VocabularyEntry, VocabularyXml(spec.Dialect, spec.Declared));
            topics.Iter(topic => {
                Stamp(container, $"{topic.Key:D}/markup.bcf", MarkupXml(topic));
                topic.Viewpoints.Iter(view => Stamp(container, $"{topic.Key:D}/{view.Key:D}.bcfv", ViewpointXml(view)));
            });
            return topics.Count;
        }).Bind(written => sink(new IssueFact(IssueFactKind.Egress, spec.Dialect, written, 0, 0, 0, 0, frame.Now()))
            .Map(_ => Validation<IssueFault, IssueYield>.Success(new IssueYield.Written(written))));

    static IO<Validation<IssueFault, IssueYield>> Surveyed(IssueSpec spec, ProjectionContext frame, Func<IssueFact, IO<Unit>> sink) =>
        IO.lift(() => {
            using ZipArchive container = Opened(spec.Source);
            return Gate(container, spec.Dialect).Map(vocabulary => new IssueSurvey(
                spec.Dialect,
                toSeq(container.Entries).Count(static entry => entry.FullName.EndsWith("markup.bcf", StringComparison.Ordinal)),
                vocabulary));
        }).Bind(survey => survey.Match(
            Succ: roster => sink(new IssueFact(IssueFactKind.Probe, spec.Dialect, roster.Topics, 0, 0, 0, 0, frame.Now()))
                .Map(_ => Validation<IssueFault, IssueYield>.Success(new IssueYield.Survey(roster))),
            Fail: fault => IO.pure(Validation<IssueFault, IssueYield>.Fail(fault))));

    // The version gate: root bcf.version's VersionId must equal the spec dialect's key BEFORE any body decodes;
    // the vocabulary then admits from the dialect's declared residence into the frozen registry.
    static Validation<IssueFault, IssueVocabulary> Gate(ZipArchive container, IssueVersion dialect) =>
        Optional(container.GetEntry("bcf.version"))
            .Map(static entry => XDocument.Load(entry.Open()).Root?.Attribute("VersionId")?.Value ?? "<absent>")
            .Match(
                Some: found => found == dialect.Key
                    ? Vocabulary(container, dialect)
                    : new IssueFault.UnknownVersion(found),
                None: static () => Validation<IssueFault, IssueVocabulary>.Fail(new IssueFault.UnknownVersion("<no-bcf.version>")));

    static ZipArchive Opened(Origin source, bool write = false) => source switch {
        Origin.FromPath path => new ZipArchive(write ? File.Create(path.Path) : File.OpenRead(path.Path), write ? ZipArchiveMode.Create : ZipArchiveMode.Read),
        Origin.FromStream stream => new ZipArchive(stream.Open(), write ? ZipArchiveMode.Create : ZipArchiveMode.Read),
        _ => throw new InvalidOperationException("<origin>"),
    };

    static void Stamp(ZipArchive container, string entry, XDocument content) {
        using Stream sink = container.CreateEntry(entry).Open();
        content.Save(sink);
    }

    static IssueFact Fact(IssueFactKind kind, IssueSpec spec, Seq<IssueTopic> topics, IssueVocabulary vocabulary, ProjectionContext frame) =>
        new(kind, spec.Dialect,
            topics.Count,
            topics.Sum(static t => t.Comments.Count),
            topics.Sum(static t => t.Viewpoints.Count),
            topics.Sum(static t => t.Viewpoints.Sum(static v => (v.Selection + v.Visibility.Exceptions).Count(static c => c.Node.IsNone))),
            topics.Sum(vocabulary.Foreign),
            frame.Now());

    // Vocabulary admission at the dialect's residence: each axis element's child values freeze into one set;
    // an absent declaration is the permissive empty registry, never a fault.
    static Validation<IssueFault, IssueVocabulary> Vocabulary(ZipArchive container, IssueVersion dialect) =>
        Optional(container.GetEntry(dialect.VocabularyEntry)).Match(
            Some: entry => Try.lift(() => XDocument.Load(entry.Open())).Run()
                .Match(
                    Succ: declared => Validation<IssueFault, IssueVocabulary>.Success(new IssueVocabulary(
                        toHashMap(toSeq(VocabularyAxis.Items).Map(axis => (axis,
                            toSeq(declared.Descendants(axis.Key).Elements()).Map(static value => value.Value).ToFrozenSet(StringComparer.Ordinal)))))),
                    Fail: error => Validation<IssueFault, IssueVocabulary>.Fail(new IssueFault.CodecReject(dialect.VocabularyEntry, error.Message))),
            None: static () => Validation<IssueFault, IssueVocabulary>.Success(IssueVocabulary.Empty));

    // One topic folder: the folder GUID and the markup Topic Guid must agree; comments and viewpoints
    // decode off the markup, each .bcfv resolves its components through the injected port.
    static Validation<IssueFault, IssueTopic> Topic(ZipArchive container, ZipArchiveEntry markup, Func<string, Option<NodeId>> resolve) =>
        Try.lift(() => {
            XElement topic = XDocument.Load(markup.Open()).Root!.Element("Topic")!;
            string folder = markup.FullName[..markup.FullName.IndexOf('/', StringComparison.Ordinal)];
            return (Folder: folder, Wire: topic, Markup: XDocument.Load(markup.Open()).Root!);
        }).Run()
        .Match(
            Succ: loaded => Guid.TryParse(loaded.Wire.Attribute("Guid")?.Value, out Guid key) && string.Equals(loaded.Folder, key.ToString("D"), StringComparison.OrdinalIgnoreCase)
                ? Row(container, loaded.Markup, loaded.Wire, key, resolve)
                : new IssueFault.TopicMalformed(loaded.Folder, "<folder-guid-disagreement>"),
            Fail: error => Validation<IssueFault, IssueTopic>.Fail(new IssueFault.TopicMalformed(markup.FullName, error.Message)));

    static XDocument VersionXml(IssueVersion dialect) =>
        new(new XElement("Version", new XAttribute("VersionId", dialect.Key)));

    static XDocument VocabularyXml(IssueVersion dialect, IssueVocabulary declared) =>
        new(new XElement("Extensions", declared.Declared.AsIterable().Map(static pair =>
            new XElement(pair.Key.Key, pair.Value.Order(StringComparer.Ordinal).Select(static value => new XElement(pair.Key.Key.TrimEnd('s'), value))))));

    // The per-field wire fold: every markup child reads through Text/Stamp/Many so absence is Option and a
    // zoned stamp parses ONCE through the extended-ISO pattern — the mechanical transcription, no review logic.
    static Validation<IssueFault, IssueTopic> Row(ZipArchive container, XElement markup, XElement topic, Guid key, Func<string, Option<NodeId>> resolve) =>
        toSeq(markup.Elements("Viewpoints")).Append(toSeq(topic.Elements("Viewpoints")))
            .Traverse(reference => Viewpoint(container, key, reference, resolve)).As()
            .Map(views => new IssueTopic(
                key, Text(topic, "Title").IfNone(""), Text(topic, "ServerAssignedId"),
                Optional(topic.Attribute("TopicType")?.Value), Optional(topic.Attribute("TopicStatus")?.Value),
                Text(topic, "Priority"), Text(topic, "Stage"),
                toSeq(topic.Elements("Labels")).Map(static label => label.Value), Text(topic, "Index").Bind(ParseInt),
                Stamp(topic, "CreationDate").IfNone(OffsetDateTime.MinIsoValue), Text(topic, "CreationAuthor").IfNone(""),
                (Stamp(topic, "ModifiedDate"), Text(topic, "ModifiedAuthor")).Apply(static (date, author) => (date, author)).As(),
                Stamp(topic, "DueDate"), Text(topic, "AssignedTo"), Text(topic, "Description"),
                toSeq(topic.Elements("RelatedTopics").Elements("RelatedTopic")).Choose(static related => ParseGuid(related.Attribute("Guid")?.Value)),
                toSeq(markup.Elements("Comment")).Choose(comment => Comment(comment)),
                views));

    static Validation<IssueFault, IssueViewpoint> Viewpoint(ZipArchive container, Guid topic, XElement reference, Func<string, Option<NodeId>> resolve) =>
        (ParseGuid(reference.Attribute("Guid")?.Value), Optional(reference.Element("Viewpoint")?.Value))
            .Apply((key, file) => Try.lift(() => XDocument.Load(container.GetEntry($"{topic:D}/{file}")!.Open()).Root!).Run()
                .Match(
                    Succ: info => Validation<IssueFault, IssueViewpoint>.Success(new IssueViewpoint(
                        key, Text(reference, "Index").Bind(ParseInt), Camera(info),
                        Components(info.Element("Components")?.Element("Selection"), resolve),
                        new IssueVisibility(
                            bool.TryParse(info.Element("Components")?.Element("Visibility")?.Attribute("DefaultVisibility")?.Value, out bool visible) && visible,
                            Components(info.Element("Components")?.Element("Visibility")?.Element("Exceptions"), resolve),
                            toHashMap(toSeq(info.Element("Components")?.Element("ViewSetupHints")?.Attributes() ?? []).Map(static hint => (hint.Name.LocalName, bool.TryParse(hint.Value, out bool on) && on)))),
                        toSeq(info.Elements("Components").Elements("Coloring").Elements("Color")).Map(color =>
                            (color.Attribute("Color")?.Value ?? "", Components(color, resolve))),
                        toSeq(info.Elements("ClippingPlanes").Elements("ClippingPlane")).Map(static plane =>
                            (Vector(plane.Element("Location")), Vector(plane.Element("Direction")))),
                        Optional(reference.Element("Snapshot")?.Value).Bind(snapshot => Snapshot(container, topic, snapshot)))),
                    Fail: error => new IssueFault.ViewpointMalformed($"{topic:D}/{file}", error.Message)))
            .As()
            .IfNone(() => Validation<IssueFault, IssueViewpoint>.Fail(new IssueFault.ViewpointMalformed(topic.ToString("D"), "<guid-or-file-absent>")));

    // The camera family: PerspectiveCamera and OrthogonalCamera are the two VisualizationInfo elements;
    // a viewpoint with neither decodes an identity orthogonal frame rather than fabricating perspective.
    static IssueCamera Camera(XElement info) =>
        info.Element("PerspectiveCamera") is { } perspective
            ? new IssueCamera.Perspective(Vector(perspective.Element("CameraViewPoint")), Vector(perspective.Element("CameraDirection")), Vector(perspective.Element("CameraUpVector")),
                double.TryParse(perspective.Element("FieldOfView")?.Value, CultureInfo.InvariantCulture, out double fov) ? fov : 60d,
                Optional(perspective.Element("AspectRatio")?.Value).Bind(ParseDouble))
            : info.Element("OrthogonalCamera") is { } orthogonal
                ? new IssueCamera.Orthogonal(Vector(orthogonal.Element("CameraViewPoint")), Vector(orthogonal.Element("CameraDirection")), Vector(orthogonal.Element("CameraUpVector")),
                    double.TryParse(orthogonal.Element("ViewToWorldScale")?.Value, CultureInfo.InvariantCulture, out double scale) ? scale : 1d)
                : new IssueCamera.Orthogonal(new IssueVector(0, 0, 0), new IssueVector(0, 0, -1), new IssueVector(0, 1, 0), 1d);

    static Seq<IssueComponent> Components(XElement? parent, Func<string, Option<NodeId>> resolve) =>
        toSeq(parent?.Elements("Component") ?? []).Map(component => {
            string guid = component.Attribute("IfcGuid")?.Value ?? "";
            return new IssueComponent(guid, resolve(guid), Optional(component.Attribute("OriginatingSystem")?.Value), Optional(component.Attribute("AuthoringToolId")?.Value));
        });

    static Option<IssueComment> Comment(XElement comment) =>
        (ParseGuid(comment.Attribute("Guid")?.Value), Stamp(comment, "Date"), Text(comment, "Author"))
            .Apply((key, date, author) => new IssueComment(key, date, author, Text(comment, "Comment").IfNone(""),
                Optional(comment.Element("Viewpoint")?.Attribute("Guid")?.Value).Bind(ParseGuid),
                (Stamp(comment, "ModifiedDate"), Text(comment, "ModifiedAuthor")).Apply(static (d, a) => (d, a)).As()))
            .As();

    static Option<ContentAddress> Snapshot(ZipArchive container, Guid topic, string file) =>
        Optional(container.GetEntry($"{topic:D}/{file}")).Map(entry => {
            using Stream image = entry.Open();
            using MemoryStream bytes = new();
            image.CopyTo(bytes);
            return ContentAddress.Of(ContentHash.Of(bytes.ToArray()));
        });

    static XDocument MarkupXml(IssueTopic topic) =>
        new(new XElement("Markup",
            new XElement("Topic",
                new XAttribute("Guid", topic.Key.ToString("D")),
                topic.Type.Map(static value => new XAttribute("TopicType", value)).OrNull(),
                topic.Status.Map(static value => new XAttribute("TopicStatus", value)).OrNull(),
                new XElement("Title", topic.Title),
                topic.Labels.Map(static label => new XElement("Labels", label)),
                new XElement("CreationDate", Iso(topic.Created)),
                new XElement("CreationAuthor", topic.CreatedBy),
                topic.Due.Map(static due => new XElement("DueDate", Iso(due))).OrNull(),
                topic.AssignedTo.Map(static who => new XElement("AssignedTo", who)).OrNull(),
                topic.Description.Map(static text => new XElement("Description", text)).OrNull()),
            topic.Comments.Map(static comment => new XElement("Comment",
                new XAttribute("Guid", comment.Key.ToString("D")),
                new XElement("Date", Iso(comment.Date)), new XElement("Author", comment.Author), new XElement("Comment", comment.Body),
                comment.Viewpoint.Map(static view => new XElement("Viewpoint", new XAttribute("Guid", view.ToString("D")))).OrNull())),
            topic.Viewpoints.Map(static view => new XElement("Viewpoints",
                new XAttribute("Guid", view.Key.ToString("D")),
                new XElement("Viewpoint", $"{view.Key:D}.bcfv"),
                view.Snapshot.Map(static _ => new XElement("Snapshot", "snapshot.png")).OrNull()))));

    static XDocument ViewpointXml(IssueViewpoint view) =>
        new(new XElement("VisualizationInfo",
            new XAttribute("Guid", view.Key.ToString("D")),
            new XElement("Components",
                new XElement("Selection", view.Selection.Map(ComponentXml)),
                new XElement("Visibility", new XAttribute("DefaultVisibility", view.Visibility.DefaultVisibility),
                    new XElement("Exceptions", view.Visibility.Exceptions.Map(ComponentXml)))),
            view.Camera.Switch<XElement>(
                perspective: static camera => new XElement("PerspectiveCamera",
                    VectorXml("CameraViewPoint", camera.ViewPoint), VectorXml("CameraDirection", camera.Direction), VectorXml("CameraUpVector", camera.Up),
                    new XElement("FieldOfView", camera.FieldOfView)),
                orthogonal: static camera => new XElement("OrthogonalCamera",
                    VectorXml("CameraViewPoint", camera.ViewPoint), VectorXml("CameraDirection", camera.Direction), VectorXml("CameraUpVector", camera.Up),
                    new XElement("ViewToWorldScale", camera.ViewToWorldScale)))));

    static XElement ComponentXml(IssueComponent component) => new("Component", new XAttribute("IfcGuid", component.IfcGuid));
    static XElement VectorXml(string name, IssueVector vector) => new(name, new XElement("X", vector.X), new XElement("Y", vector.Y), new XElement("Z", vector.Z));
    static IssueVector Vector(XElement? triple) => new(Axis(triple, "X"), Axis(triple, "Y"), Axis(triple, "Z"));
    static double Axis(XElement? triple, string axis) => double.TryParse(triple?.Element(axis)?.Value, CultureInfo.InvariantCulture, out double value) ? value : 0d;
    static Option<string> Text(XElement parent, string name) => Optional(parent.Element(name)?.Value).Filter(static value => value.Length > 0);
    static Option<OffsetDateTime> Stamp(XElement parent, string name) => Text(parent, name).Bind(static text =>
        OffsetDateTimePattern.ExtendedIso.Parse(text) is { Success: true } parsed ? Some(parsed.Value) : None);
    static Option<int> ParseInt(string text) => int.TryParse(text, CultureInfo.InvariantCulture, out int value) ? Some(value) : None;
    static Option<double> ParseDouble(string text) => double.TryParse(text, CultureInfo.InvariantCulture, out double value) ? Some(value) : None;
    static Option<Guid> ParseGuid(string? text) => Guid.TryParse(text, out Guid value) ? Some(value) : None;
    static string Iso(OffsetDateTime stamp) => OffsetDateTimePattern.ExtendedIso.Format(stamp);
}

// `XElement` content ignores null children, so an absent optional member emits nothing — the wire
// projection of `Option` at this one XML seam.
file static class OptionXml {
    extension<T>(Option<T> value) where T : class {
        public object? OrNull() => value.MatchUnsafe<object?>(Some: static held => held, None: static () => null);
    }
}
```

| [INDEX] | [POLICY]       | [VALUE]                                    | [BINDING]                                              |
| :-----: | :------------- | :----------------------------------------- | :----------------------------------------------------- |
|  [01]   | version gate   | root `bcf.version` `VersionId` first       | outside the closed axis rails; no best-effort parse    |
|  [02]   | vocabularies   | runtime-sourced `IssueVocabulary` registry | closed axis rows, frozen per-project value sets        |
|  [03]   | foreign values | carried facts on the receipt               | never normalized, never a fault; round-trip fidelity   |
|  [04]   | container      | BCL `ZipArchive` + `XDocument`             | zero foreign packages; BCF is zip+XML by construction  |
|  [05]   | fault posture  | `Semigroup` accumulation per topic         | one malformed topic never discards the sound remainder |

## [03]-[ISSUE_ROWS]

- Owner: the row family — `IssueTopic` the per-topic aggregate at full BCF axis depth, `IssueComment` the threaded comment row, `IssueViewpoint` the visualization row, `IssueCamera` the closed camera family, `IssueComponent` the element reference carrying its correlation outcome, `IssueVector` the bare XYZ carrier — plus `IssueDelta` the cycle diff and `IssueRows` the correlation/diff surface.
- Cases: `IssueCamera` closes at `Perspective(ViewPoint, Direction, Up, FieldOfView, Option<double> AspectRatio)` and `Orthogonal(ViewPoint, Direction, Up, ViewToWorldScale)` — the two `VisualizationInfo` camera elements, each decoding its `CameraViewPoint`/`CameraDirection`/`CameraUpVector` triples; `IssueComponent` carries the wire `IfcGuid`, the resolved `Option<NodeId>`, and the `OriginatingSystem`/`AuthoringToolId` provenance attributes; `IssueViewpoint` carries selection, visibility (`DefaultVisibility` + exceptions + `ViewSetupHints`), coloring groups, clipping planes, and the snapshot's `ContentAddress`.
- Entry: `public static ElementSet IssueTopic.Referenced()` projects every RESOLVED component GlobalId across the topic's viewpoints into the one selection currency (unresolved references stay carried data); `public static IssueDelta IssueRows.Reconcile(Seq<IssueTopic> held, Seq<IssueTopic> update)` correlates by the stable topic `Guid` and partitions the cycle — opened, closed, status moves, assignment moves, comment additions — the issue sibling of the schedule `Reconcile` discipline.
- Auto: correlation is the identity tier's law inverted — a BCF `IfcGuid` is exactly the compressed IFC GlobalId the `GlobalIds` map mirrors, so resolution is one injected-port hop and a re-imported model's fresh `NodeId`s stay correlated because the GlobalId, not the neutral key, is the wire; an unresolved component (a demolished element, a sibling discipline's model, a foreign file) rides `Option.None` on the row and counts on the ingest fact — review reality routinely references what the local model no longer holds; `Reconcile` never invents rows: a topic in `update` absent from `held` is `Opened`, the inverse is `Removed` (a BCF exchange that drops a topic is itself review information), and a shared GUID diffs field-wise into the move partitions.
- Receipt: the row family carries no stream of its own — the `[02]` facts carry the counts, and the durable landing's receipts are the store rail's.
- Packages: Rasm.Element (`NodeId`), Rasm.Persistence (`Element/codec` `ContentAddress`, `Query/lane#ELEMENT_SET_ALGEBRA` `ElementSet`), LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime (`OffsetDateTime` — the zoned wire stamp preserved, never a fabricated UTC collapse), BCL inbox.
- Growth: a new topic axis is one field on `IssueTopic` (`ServerAssignedId` exercised it — the BCF 3.0 column landed as one `Option` field); a new viewpoint capability is one field on `IssueViewpoint`; a new cycle partition is one `IssueDelta` field; zero new surface — a per-dialect row family, a slip DTO beside `IssueDelta`, a topic keyed by title or index instead of GUID, or an unresolved reference silently dropped is the deleted form.
- Boundary: rows are the Persistence half of the coordination-review cycle — `Rasm.Bim` clash/IDS surfaces mint topics from their `ElementSet` results and read `Reconcile`'s partitions for review-state drift, the AppUi viewport consumes `IssueCamera`/`IssueComponent` to restore a view, and the app composition root owns both mappings; the snapshot bytes live in the blob plane under their content address (this row carries the ADDRESS; a byte copy beside it forks residence); `RelatedTopics` cross-references stay topic GUIDs because the BCF wire owns that identity.

```csharp signature
// --- [MODELS] ---------------------------------------------------------------------------
public readonly record struct IssueVector(double X, double Y, double Z);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record IssueCamera {
    private IssueCamera() { }
    public sealed record Perspective(IssueVector ViewPoint, IssueVector Direction, IssueVector Up, double FieldOfView, Option<double> AspectRatio) : IssueCamera;
    public sealed record Orthogonal(IssueVector ViewPoint, IssueVector Direction, IssueVector Up, double ViewToWorldScale) : IssueCamera;
}

// The element reference: the wire IfcGuid is the correlation key, the resolved NodeId the durable
// outcome, and None is carried review reality — never a fault, never a dropped component.
public readonly record struct IssueComponent(string IfcGuid, Option<NodeId> Node, Option<string> OriginatingSystem, Option<string> AuthoringToolId);

public sealed record IssueVisibility(bool DefaultVisibility, Seq<IssueComponent> Exceptions, HashMap<string, bool> ViewSetupHints);

public sealed record IssueViewpoint(
    Guid Key, Option<int> Index, IssueCamera Camera,
    Seq<IssueComponent> Selection, IssueVisibility Visibility, Seq<(string Color, Seq<IssueComponent> Components)> Coloring,
    Seq<(IssueVector Location, IssueVector Direction)> ClippingPlanes, Option<ContentAddress> Snapshot);

public sealed record IssueComment(Guid Key, OffsetDateTime Date, string Author, string Body, Option<Guid> Viewpoint, Option<(OffsetDateTime Date, string Author)> Modified);

public sealed record IssueTopic(
    Guid Key, string Title, Option<string> ServerAssignedId,
    Option<string> Type, Option<string> Status, Option<string> Priority, Option<string> Stage,
    Seq<string> Labels, Option<int> Index,
    OffsetDateTime Created, string CreatedBy, Option<(OffsetDateTime Date, string Author)> Modified,
    Option<OffsetDateTime> Due, Option<string> AssignedTo, Option<string> Description,
    Seq<Guid> RelatedTopics, Seq<IssueComment> Comments, Seq<IssueViewpoint> Viewpoints);

// The cycle diff: opened/removed partition by GUID presence, the move partitions diff shared GUIDs
// field-wise — the issue sibling of the schedule baseline/update Reconcile discipline.
public sealed record IssueDelta(
    Seq<IssueTopic> Opened, Seq<IssueTopic> Removed,
    Seq<(IssueTopic Held, IssueTopic Update)> StatusMoved, Seq<(IssueTopic Held, IssueTopic Update)> Reassigned,
    Seq<(IssueTopic Held, Seq<IssueComment> Added)> Commented);

// --- [OPERATIONS] -----------------------------------------------------------------------
public static class IssueRows {
    extension(IssueTopic topic) {
        // Every RESOLVED component across the topic's viewpoints, minted into the one selection currency.
        public ElementSet Referenced() => ElementSet.Of(toSeq(
            topic.Viewpoints.Bind(static view => view.Selection + view.Visibility.Exceptions + view.Coloring.Bind(static group => group.Components))
                .Choose(static component => component.Node)));
    }

    public static IssueDelta Reconcile(Seq<IssueTopic> held, Seq<IssueTopic> update) {
        HashMap<Guid, IssueTopic> prior = toHashMap(held.Map(static t => (t.Key, t)));
        HashMap<Guid, IssueTopic> next = toHashMap(update.Map(static t => (t.Key, t)));
        Seq<(IssueTopic Held, IssueTopic Update)> shared = toSeq(held.Choose(t => next.Find(t.Key).Map(u => (t, u))));
        return new IssueDelta(
            Opened: update.Filter(t => !prior.ContainsKey(t.Key)),
            Removed: held.Filter(t => !next.ContainsKey(t.Key)),
            StatusMoved: shared.Filter(static pair => pair.Held.Status != pair.Update.Status),
            Reassigned: shared.Filter(static pair => pair.Held.AssignedTo != pair.Update.AssignedTo),
            Commented: toSeq(shared.Choose(static pair =>
                toSeq(pair.Update.Comments.Filter(c => !pair.Held.Comments.Exists(h => h.Key == c.Key))) is { IsEmpty: false } added
                    ? Some((pair.Held, added))
                    : None)));
    }
}
```

| [INDEX] | [POLICY]         | [VALUE]                                              | [BINDING]                                             |
| :-----: | :--------------- | :--------------------------------------------------- | :---------------------------------------------------- |
|  [01]   | correlation      | wire `IfcGuid` → `GlobalIds` mirror → `NodeId`       | one injected-port hop; survives re-import re-keying   |
|  [02]   | unresolved refs  | `Option.None` carried on the row                     | review reality, counted on the fact, never dropped    |
|  [03]   | selection bridge | `Referenced()` → `ElementSet`                        | composes the one currency; clash results round-trip   |
|  [04]   | cycle diff       | `Reconcile` GUID-keyed partitions                    | the schedule `Reconcile` discipline on the issue axis |
|  [05]   | wire stamps      | `OffsetDateTime` preserving the declared offset      | never a fabricated UTC `Instant`                      |
|  [06]   | snapshots        | `ContentAddress` on the row, bytes in the blob plane | never a byte copy beside the address                  |

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
