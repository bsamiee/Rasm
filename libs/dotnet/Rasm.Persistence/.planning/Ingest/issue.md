# [PERSISTENCE_INGEST_ISSUE]

Rasm.Persistence is the DURABLE half of the BCF issue-review cycle, and it operates on typed rows alone: the `.bcfzip` container wire has ONE branch custodian — `Rasm.Bim/Review/issues#BCF_ARCHIVE` `BcfArchive` over the catalogued `Smino.Bcf.Toolkit` — so this owner never opens a container, parses no XML, and holds no zip surface. This owner's composition root reads an archive through the custodian, transcribes each `BcfTopic` onto this page's `IssueTopic` row under the `BcfTopic`⇄`IssueTopic` correspondence law, and hands the row set here; the reverse leg releases held rows for the root to transcribe back and write through `BcfArchive.Write`. What this owner keeps is everything only the durable spine can do: element correlation (a BCF `IfcGuid` is exactly the `Element/identity#ELEMENT_IDENTITY` `GlobalIds` mirror, so every viewpoint component resolves one hop to a durable model-qualified `SetKey` through the injected resolve port — a sibling discipline reference resolves across models instead of dropping to absence), the `Query/lane#ELEMENT_SET_ALGEBRA` `KeySelection` projection the clash/IDS/QTO surfaces compose, the `IssueRows.Reconcile` cycle diff, content-addressed snapshot residence, the `store.issue.*` fact stream, and the durable landing at the app composition root — the same row-shape law every Ingest sibling obeys.

Correspondence binds one law per wire: `IssueTopic.Status` carries the custodian's `StatusToken` VERBATIM (the project-vocabulary free string, never the parsed lifecycle enum, so a Persistence round trip launders no foreign tool's state); stamps cross as the custodian's `Instant` — one stamp law per wire, the container family's; camera absence is TYPED (`Option` — a selection-only viewpoint is legal BCF the custodian's XOR gate already admitted, and a fabricated default frame is the deleted form); `BcfVocabulary` hands across as the `IssueVocabulary` registry data; snapshot and bitmap bytes leave the custodian's `BcfFile` payload store into the blob plane with only the `ContentAddress` on the row. Columns the durable cycle never keys — reference links, document references, BIM snippets, header files — stay the custodian's container family and cross only through it. Every unresolved GlobalId stays a carried FACT (a cross-model or retired reference is normal review reality, never a fault), the topic vocabularies stay PROJECT-EXTENSIBLE runtime data, and this owner NEVER computes review logic: rows project to `Rasm.Element` and land durably at the app composition root. `IssueFault` closes the accumulating family whose every case seats one `[FaultCase]` ordinal on the `FaultBand.StoreIssue` decade, facts ride `store.issue.*` carrying ONE `IssueCensus` the handed rows fold in a single traversal, `ProjectionContext` arrives from `Element/graph#STORE_RAIL`, and `FaultBand` from the `Rasm/Domain/rails#FAULT_BAND` roster. This seam opens no source of its own — rows arrive handed, so no `Origin` column rides the spec.

## [01]-[INDEX]

- [02]-[ISSUE_SEAM]: `IssueSource` row-admission seam — the runtime-sourced vocabulary registry, the closed ingest/egress op family over handed rows, the element correlation, the accumulating fault band, and the typed fact stream.
- [03]-[ISSUE_ROWS]: `IssueTopic`/`IssueComment`/`IssueViewpoint` row family at durable-cycle depth, the GlobalId→`SetKey` correlation carrier and `KeySelection` projection, and the `Reconcile` issue-cycle diff.

## [02]-[ISSUE_SEAM]

- Owner: `IssueVersion` is the wire-dialect provenance axis; `VocabularyAxis` is the closed extensible-axis vocabulary; `IssueVocabulary` is the per-project frozen registry; `IssueSpec` fixes dialect, provenance, declared vocabulary, and correlation port; `IssueOp`/`IssueYield` close dispatch over handed rows; `[FaultCase]` closes the fault roster; `IssueFault` is the accumulating family above it; `IssueCensus` folds counts once; `IssueSource` owns `Run`.
- Cases: `IssueOp.Ingest(IssueSpec, Seq<IssueTopic>)` admits a handed row set and yields correlated rows; `IssueOp.Egress(IssueSpec, Seq<IssueTopic>)` releases held rows and yields the count. `IssueFault` is `SeamReject | TopicRejected`; independent failures accumulate through `Validation<Error, …>`.
- Entry: `Run(IssueOp)` is the ONE polymorphic entry over the closed operation union; per-topic admission failures accumulate so one malformed topic never discards sound rows.
- Auto: key admission proves every topic `Guid` unique across the handed set and every comment's viewpoint join resolves inside its own topic — a violation rails `TopicRejected` naming the topic; correlation maps each component's `IfcGuid` through the injected port and lands the result on the row's own `Option<SetKey>`; a topic value outside the declared vocabulary remains byte-faithful because BCF vocabularies are advisory declarations, never a mutation or fault.
- Packages: Rasm.Element (`NodeId`), Rasm (`Rasm/Domain/rails#FAULT_BAND` `FaultBand`), Rasm.Persistence (`Element/graph#STORE_RAIL` `ProjectionContext`), LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime, BCL inbox (zero container surface: `ZipArchive` and `XDocument` are the custodian's, and their appearance here is the deleted form the branch custody ruling names).
- Growth: a new wire dialect is one `IssueVersion` row; a new extensible axis is one `VocabularyAxis` row; a new admission check is one arm inside the one `Admit` fold; a new fault class is one case inside the registry decade; zero new surface — a container codec, a per-dialect reader family, a compiled status/type/priority enum a foreign project breaks, a topic value silently normalized at admission, or a review-logic computation inside this seam is the deleted form because the container wire is the custodian's, the vocabularies are runtime-admitted data, and this owner is the durable half.
- Boundary: the seam owns exactly the row half — admission, correlation, release, facts — and the container wire is `Rasm.Bim/Review/issues#BCF_ARCHIVE` `BcfArchive`'s whole (read, write, version discrimination, vocabulary residence, bitmap parts), the two non-referencing S2 ends meeting at the composition root that owns the `BcfTopic`⇄`IssueTopic` transcription; the durable landing is the app's (`Element/graph#STORE_RAIL` for row residence, `Store/blobstore` for snapshot bytes, both at the composition root per the Ingest row-shape law); GlobalId correlation is a one-hop injected port because the `GlobalIds` mirror is `Element/identity`'s (this page never opens a store connection); `← Rasm.Bim` clash results mint topics the root transcribes and hands to `Ingest` for durable landing, `→ Rasm.Bim` resolved topics return as status moves the planner reads through `Reconcile`; the server-side BCF-API is the custodian's REST projection over the Compute transport — a foreign review server's export lands here as rows the root read through the same custodian.

```csharp
using Rasm.Persistence.Element;

namespace Rasm.Persistence.Ingest;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class IssueVersion {
    public static readonly IssueVersion Bcf21 = new("2.1");
    public static readonly IssueVersion Bcf30 = new("3.0");
}

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

// --- [MODELS] --------------------------------------------------------------------------
public sealed record IssueVocabulary(HashMap<VocabularyAxis, FrozenSet<string>> Declared) {
    public static readonly IssueVocabulary Empty = new(HashMap<VocabularyAxis, FrozenSet<string>>());
    public bool Admits(VocabularyAxis axis, string value) =>
        Declared.Find(axis).Map(set => set.Contains(value)).IfNone(true);
    public int Foreign(IssueTopic topic) =>
        Seq<Option<(VocabularyAxis Axis, string Value)>>(
            topic.Type.Map(value => (VocabularyAxis.TopicType, value)),
            topic.Status.Map(value => (VocabularyAxis.TopicStatus, value)),
            topic.Priority.Map(value => (VocabularyAxis.Priority, value)),
            topic.Stage.Map(value => (VocabularyAxis.Stage, value)))
            .Somes().Append(topic.Labels.Map(static value => (VocabularyAxis.Label, value)))
            .Count(cell => !Admits(cell.Axis, cell.Value));
}

public sealed record IssueSpec(IssueVersion Dialect, IssueVocabulary Declared, Func<string, Option<SetKey>> Resolve);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record IssueOp {
    private IssueOp() { }
    public sealed record Ingest(IssueSpec Spec, Seq<IssueTopic> Rows) : IssueOp;
    public sealed record Egress(IssueSpec Spec, Seq<IssueTopic> Rows) : IssueOp;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record IssueYield {
    private IssueYield() { }
    public sealed record Topics(Seq<IssueTopic> Rows) : IssueYield;
    public sealed record Released(int Count) : IssueYield;
}

// --- [ERRORS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record IssueFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.StoreIssue;
    private IssueFault() { }
    [FaultCase(0)]
    public sealed partial record SeamReject(string Detail) : IssueFault();
    [FaultCase(1)]
    public sealed partial record TopicRejected(Guid Topic, string Detail) : IssueFault();


    public override string Message => Switch(
        seamReject:    static c => $"<issue-seam:{c.Detail}>",
        topicRejected: static c => $"<issue-topic:{c.Topic:D}:{c.Detail}>");
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public readonly record struct IssueCensus(int Topics, int Comments, int Viewpoints, int Unresolved, int Foreign) {
    public static readonly IssueCensus Empty = new(0, 0, 0, 0, 0);

    public static IssueCensus Of(Seq<IssueTopic> topics, IssueVocabulary declared) =>
        topics.Fold(Empty, (census, topic) => new IssueCensus(
            Topics:      census.Topics + 1,
            Comments:    census.Comments + topic.Comments.Count,
            Viewpoints:  census.Viewpoints + topic.Viewpoints.Count,
            Unresolved:  census.Unresolved + topic.Viewpoints.Sum(static view =>
                             (view.Selection + view.Visibility.Exceptions).Count(static component => component.Node.IsNone)),
            Foreign:     census.Foreign + declared.Foreign(topic)));
}

public static class IssueSource {
    public static IO<Validation<Error, IssueYield>> Run(IssueOp op) =>
        op.Switch(
            ingest: handed => Admitted(handed.Spec, handed.Rows),
            egress: handed => Released(handed.Rows));

    static IO<Validation<Error, IssueYield>> Admitted(IssueSpec spec, Seq<IssueTopic> rows) => IO.pure(
        Keyed(rows).Bind(unique => unique.Traverse(topic => Admit(topic, spec.Resolve)).As())
            .Map(static held => (IssueYield)new IssueYield.Topics(held)));

    static IO<Validation<Error, IssueYield>> Released(Seq<IssueTopic> rows) => IO.pure(
        Keyed(rows).Map(static held => (IssueYield)new IssueYield.Released(held.Count)));

    static Validation<Error, Seq<IssueTopic>> Keyed(Seq<IssueTopic> rows) =>
        toSeq(rows.GroupBy(static topic => topic.Key).Where(static group => group.Count() > 1).Select(static group => group.Key))
            .Traverse(static key => (Validation<Error, Guid>)new IssueFault.TopicRejected(key, "<duplicate-key>")).As()
            .Map(_ => rows);

    static Validation<Error, IssueTopic> Admit(IssueTopic topic, Func<string, Option<SetKey>> resolve) =>
        topic.Comments
            .Filter(comment => comment.Viewpoint.Map(view => !topic.Viewpoints.Exists(v => v.Key == view)).IfNone(false))
            .Traverse(comment => (Validation<Error, IssueComment>)new IssueFault.TopicRejected(topic.Key, $"<comment-viewpoint-orphan:{comment.Key:D}>")).As()
            .Map(_ => topic with { Viewpoints = topic.Viewpoints.Map(view => Correlated(view, resolve)) });

    static IssueViewpoint Correlated(IssueViewpoint view, Func<string, Option<SetKey>> resolve) =>
        view with {
            Selection = view.Selection.Map(component => component with { Node = resolve(component.IfcGuid) }),
            Visibility = view.Visibility with { Exceptions = view.Visibility.Exceptions.Map(component => component with { Node = resolve(component.IfcGuid) }) },
            Coloring = view.Coloring.Map(group => (group.Color, group.Components.Map(component => component with { Node = resolve(component.IfcGuid) }))),
        };

}
```

| [INDEX] | [POLICY]       | [VALUE]                                      | [BINDING]                                                |
| :-----: | :------------- | :------------------------------------------- | :------------------------------------------------------- |
|  [01]   | custody        | container wire is the custodian codec's      | rows in, rows out; zero `ZipArchive`/`XDocument` surface |
|  [02]   | correspondence | `BcfTopic`⇄`IssueTopic` at the root          | `Status`⇄`StatusToken` verbatim, `Instant` stamps        |
|  [03]   | vocabularies   | runtime-sourced `IssueVocabulary` registry   | closed axis rows, frozen per-project sets, handed data   |
|  [04]   | foreign values | preserved on the row                         | never normalized, never a fault; round-trip fidelity     |
|  [05]   | fault posture  | applicative `Validation<Error, …>` traversal | every duplicate key and orphaned join reports, not one   |
|  [06]   | counts         | one `IssueCensus` folded in one traversal    | five loose columns walked the handed set four times      |

## [03]-[ISSUE_ROWS]

- Owner: the row family — `IssueTopic` the per-topic aggregate at durable-cycle depth, `IssueComment` the threaded comment row, `IssueViewpoint` the visualization row, `IssueCamera` the closed camera family, `IssueComponent` the element reference carrying its correlation outcome, `IssueVector` the bare XYZ carrier, `IssueDelta` the cycle diff, and `IssueRows` the correlation/diff surface.
- Cases: `IssueCamera` closes at `Perspective(ViewPoint, Direction, Up, FieldOfView, Option<double> AspectRatio)` and `Orthogonal(ViewPoint, Direction, Up, ViewToWorldScale)`; the viewpoint carries it `Option`-valued — a selection-only viewpoint is legal BCF whose absence stays typed, per the custodian's camera-XOR admission, and a fabricated default frame is the deleted form; `IssueComponent` carries the wire `IfcGuid`, the resolved model-qualified `Option<SetKey>`, and the `OriginatingSystem`/`AuthoringToolId` provenance; `IssueViewpoint` carries selection, visibility (`DefaultVisibility` + exceptions + `ViewSetupHints`), coloring groups, clipping planes, and the snapshot's `ContentAddress`.
- Entry: `public static KeySelection IssueTopic.Referenced()` projects every RESOLVED component GlobalId across the topic's viewpoints into the one selection currency (unresolved references stay carried data); `public static IssueDelta IssueRows.Reconcile(Seq<IssueTopic> held, Seq<IssueTopic> update)` correlates by the stable topic `Guid` and partitions the cycle — opened, removed, status moves, assignment moves, comment additions — the issue sibling of the schedule `Reconcile` discipline.
- Auto: correlation is the identity tier's law inverted — a BCF `IfcGuid` is exactly the compressed IFC GlobalId the `GlobalIds` map mirrors, so resolution is one injected-port hop and a re-imported model's fresh `NodeId`s stay correlated because the GlobalId, not the neutral key, is the wire; an unresolved component (a demolished element, a foreign file outside the project) rides `Option.None` on the row and counts on the ingest fact, while a sibling discipline's reference resolves model-qualified through the same port — review reality routinely references what the local model no longer holds; `Reconcile` never invents rows: a topic in `update` absent from `held` is `Opened`, the inverse is `Removed` (a BCF exchange that drops a topic is itself review information), and a shared GUID diffs field-wise into the move partitions.
- Packages: Rasm.Element (`NodeId`), Rasm.Persistence (`Element/codec` `ContentAddress`, `Query/lane#ELEMENT_SET_ALGEBRA` `KeySelection`/`SetKey`), LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime (`Instant` — the custodian's stamp law, one stamp vocabulary per wire), BCL inbox.
- Growth: a new topic axis is one field on `IssueTopic` (`ServerAssignedId` exercised it — the BCF 3.0 column landed as one `Option` field); a new viewpoint capability is one field on `IssueViewpoint`; a new cycle partition is one `IssueDelta` field; zero new surface — a per-dialect row family, a slip DTO beside `IssueDelta`, a topic keyed by title or index instead of GUID, or an unresolved reference silently dropped is the deleted form.
- Boundary: rows are the Persistence half of the coordination-review cycle — `Rasm.Bim` clash/IDS surfaces mint topics from their `KeySelection` results and read `Reconcile`'s partitions for review-state drift, the AppUi viewport consumes `IssueCamera`/`IssueComponent` to restore a view, and the app composition root owns both mappings; `Status`/`Type`/`Priority`/`Stage` carry the custodian's VERBATIM project-vocabulary tokens (the `StatusToken` election), never a compiled enum a foreign project breaks; the snapshot bytes live in the blob plane under their content address (this row carries the ADDRESS; a byte copy beside it forks residence); `RelatedTopics` cross-references stay topic GUIDs because the BCF wire owns that identity; container-only ornament — reference links, document references, BIM snippets, header files — stays the custodian's family and never grows a durable column here.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct IssueVector(double X, double Y, double Z);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record IssueCamera {
    private IssueCamera() { }
    public sealed record Perspective(IssueVector ViewPoint, IssueVector Direction, IssueVector Up, double FieldOfView, Option<double> AspectRatio) : IssueCamera;
    public sealed record Orthogonal(IssueVector ViewPoint, IssueVector Direction, IssueVector Up, double ViewToWorldScale) : IssueCamera;
}

public readonly record struct IssueComponent(string IfcGuid, Option<SetKey> Node, Option<string> OriginatingSystem, Option<string> AuthoringToolId);

public sealed record IssueVisibility(bool DefaultVisibility, Seq<IssueComponent> Exceptions, HashMap<string, bool> ViewSetupHints);

public sealed record IssueViewpoint(
    Guid Key, Option<int> Index, Option<IssueCamera> Camera,
    Seq<IssueComponent> Selection, IssueVisibility Visibility, Seq<(string Color, Seq<IssueComponent> Components)> Coloring,
    Seq<(IssueVector Location, IssueVector Direction)> ClippingPlanes, Option<ContentAddress> Snapshot);

public sealed record IssueComment(Guid Key, Instant Date, string Author, string Body, Option<Guid> Viewpoint, Option<(Instant Date, string Author)> Modified);

public sealed record IssueTopic(
    Guid Key, string Title, Option<string> ServerAssignedId,
    Option<string> Type, Option<string> Status, Option<string> Priority, Option<string> Stage,
    Seq<string> Labels, Option<int> Index,
    Instant Created, string CreatedBy, Option<(Instant Date, string Author)> Modified,
    Option<Instant> Due, Option<string> AssignedTo, Option<string> Description,
    Seq<Guid> RelatedTopics, Seq<IssueComment> Comments, Seq<IssueViewpoint> Viewpoints);

public sealed record IssueDelta(
    Seq<IssueTopic> Opened, Seq<IssueTopic> Removed,
    Seq<(IssueTopic Held, IssueTopic Update)> StatusMoved, Seq<(IssueTopic Held, IssueTopic Update)> Reassigned,
    Seq<(IssueTopic Held, Seq<IssueComment> Added)> Commented);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class IssueRows {
    extension(IssueTopic topic) {
        public KeySelection Referenced() => KeySelection.Of(toSeq(
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
|  [01]   | correlation      | wire `IfcGuid` → `GlobalIds` mirror → `SetKey`       | one injected-port hop; survives re-import re-keying   |
|  [02]   | unresolved refs  | `Option.None` carried on the row                     | review reality, counted on the fact, never dropped    |
|  [03]   | selection bridge | `Referenced()` → `KeySelection`                      | composes the one currency; clash results round-trip   |
|  [04]   | cycle diff       | `Reconcile` GUID-keyed partitions                    | the schedule `Reconcile` discipline on the issue axis |
|  [05]   | wire stamps      | the custodian's `Instant`                            | one stamp law per wire; never a second stamp alphabet |
|  [06]   | snapshots        | `ContentAddress` on the row, bytes in the blob plane | never a byte copy beside the address                  |

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
