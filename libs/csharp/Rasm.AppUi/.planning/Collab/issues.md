# [APPUI_ISSUE_BOARD]

The coordination rail is the openBIM issue board: `Issue` composes one AppUi `Viewpoint` view-state with a `Rasm.Bim`-owned BCF topic consumed at the boundary, the comment conversation is a `Collab/sync.md` `CollabDoc` `map` container keyed by comment GUID (the bespoke comment CRDT is DROPPED root-up), `IssueTile` projects each issue onto the dashboard tile family, and `IssueBoard` is the board projection owning the issue-to-viewpoint binding. Every comment mutation rides the ONE `IntentLedger.Commit` rail — the durable intent lands first, the live container applies through the same `IntentApply` dispatch replay uses — so live convergence and durable truth cannot silently diverge and the board holds no state replay cannot reconstruct. The page owns the UI issue projection, the comment lens over the shared merge authority, the snapshot tile, and the topic-to-viewpoint binding; the substrate is the `Render/pipeline.md#VIEWPOINT_CODEC` `Viewpoint` receipt, the `Collab/sync.md` merge authority and edit-intent stream, the `charts-dashboards` dashboard tiles, and the `Rasm.Bim/Review/issues#BCF_ARCHIVE` `BcfTopic`/`BcfComment`/`BcfViewpoint` contract at the package edge. AppUi composes the BCF topic plus its own `Viewpoint` and the one collab owner into the board and never re-mints a BCF semantic schema; a second BCF model or a direct BCF-XML writer here is the rejected form.

## [01]-[INDEX]

- [02]-[ISSUE_MODEL]: Issue composing the `Viewpoint`, the BCF topic, and the snapshot; the status row vocabulary.
- [03]-[COMMENT_LENS]: The comment conversation as a `CollabDoc` map container; the one commit rail; BCF projection at the boundary.
- [04]-[ISSUE_TILE]: Dashboard-tile projection of the issue list with status brushing.
- [05]-[BOARD_PROJECTION]: Board owning the issue-to-viewpoint binding, the merge-authority re-projection, and the BCF round-trip.

## [02]-[ISSUE_MODEL]

- Owner: `IssueStatus` `[SmartEnum<string>]` the coordination lifecycle whose rows carry the cross-filter `Bit` ordinal AND the `BcfStatus` correspondence as columns; `Issue` the board issue record; `IssueBinding` the topic-to-viewpoint binding; `IssueFault` the typed fault family on the `AppUiFaultBand.Issue` registry row (6510).
- Cases: `IssueStatus` = open, in-progress, resolved, closed, reopened; `IssueFault` = Text | TopicMalformed | ViewpointUnbound | CommentConflict.
- Entry: `public static Fin<Issue> FromTopic(BcfTopic topic, ClockPolicy clocks)` — projects a `Rasm.Bim` BCF topic consumed at the boundary into a board issue binding its viewpoints onto the AppUi `Viewpoint` receipt; `public BcfTopic ToTopic()` — `with`-updates the carried source row (board-edited columns only) or mints a core-column topic for a board-authored issue, never a second BCF schema.
- Auto: each issue carries the BCF topic identity (the GUID, title, status, type, priority, author, and creation instant) plus its bound `Viewpoint` set, its comment projection, and the consumed source row so the widened `BcfTopic` columns the board never edits (description, assignment, stage, due date, labels, provenance, references, snippet, files, status label) survive the round-trip untouched and a coordination issue is one unit the board renders; the status correspondence is ROW DATA — each `IssueStatus` row carries its `BcfStatus` column, `FromBcf` is the `Items`-derived frozen index over that column, and `ToTopic` reads `Status.Bcf` directly, so the board lifecycle and the BCF status are one vocabulary with zero hand-enumerated mapping switches; each BCF viewpoint binds onto the AppUi `Viewpoint` through `ViewpointCodec.FromBcf` so the issue's saved view rides the one portable view-state receipt the viewport, the markup, and the reality-capture overlay share — the issue mints no second camera-snapshot shape; the snapshot tile is the viewpoint's rendered thumbnail through the visuals capture lane so the board shows the issue's view at a glance.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.Bim (project)
- Growth: a new issue field is one `Issue` member; a new lifecycle state is one `IssueStatus` row carrying its bit and BCF columns; a new fault is one `IssueFault` case (one `detail` ordinal on the 6510 row); zero new surface.
- Boundary: the issue composes the `Rasm.Bim/Review/issues#BCF_ARCHIVE` `BcfTopic`/`BcfComment`/`BcfViewpoint` contract consumed at the package edge — AppUi owns the `Viewpoint` receipt and the board projection while `Rasm.Bim` owns the openBIM topic/component/comment exchange semantics, the two meeting only at the topic contract, so a second BCF model or a direct `.bcfzip`/BCF-XML writer inside `Collab/` is the rejected form; the BCF viewpoint binds onto the AppUi `Viewpoint` through `ViewpointCodec.FromBcf` so the issue's view-state is the one portable receipt and a parallel issue-camera shape is the deleted form; the issue round-trips back to a `BcfTopic` through `ToTopic` — a `with`-update over the carried source row touching only the board-edited columns (title, status, type, priority, comments, viewpoints), each viewpoint re-encoded over its guid-matched source row and `StatusLabel` cleared only on a board status change — so a CDE or external BCF viewer reads the board's issues and the round-trip is lossless through the `Rasm.Bim` archive codec, never an AppUi-local BCF writer; the comment projection preserves the `BcfComment` `ModifiedDate`/`ModifiedAuthor` provenance columns so a board pass never strips modification history.

```csharp signature
// --- [ERRORS] --------------------------------------------------------------------------
[Union]
public abstract partial record IssueFault : Expected, IValidationError<IssueFault> {
    private IssueFault(string detail, int code) : base(detail, code, None) { }

    public static IssueFault Create(string message) => new Text(message);

    public sealed record Text : IssueFault { public Text(string detail) : base(detail, AppUiFaultBand.Issue.Code(0)) { } }
    public sealed record TopicMalformed : IssueFault { public TopicMalformed(string detail) : base(detail, AppUiFaultBand.Issue.Code(1)) { } }
    public sealed record ViewpointUnbound : IssueFault { public ViewpointUnbound(string detail) : base(detail, AppUiFaultBand.Issue.Code(2)) { } }
    public sealed record CommentConflict : IssueFault { public CommentConflict(string detail) : base(detail, AppUiFaultBand.Issue.Code(3)) { } }
}

// --- [TYPES] ---------------------------------------------------------------------------
// Row columns carry both derived correspondences: Bit is the cross-filter ordinal, Bcf the exchange
// status — FromBcf is the Items-derived frozen index, so no hand-enumerated mapping switch exists.
[SmartEnum<string>]
public sealed partial class IssueStatus {
    public static readonly IssueStatus Open = new("open", bit: 0, bcf: Rasm.Bim.Coordination.BcfStatus.Open);
    public static readonly IssueStatus InProgress = new("in-progress", bit: 1, bcf: Rasm.Bim.Coordination.BcfStatus.InProgress);
    public static readonly IssueStatus Resolved = new("resolved", bit: 2, bcf: Rasm.Bim.Coordination.BcfStatus.Resolved);
    public static readonly IssueStatus Closed = new("closed", bit: 3, bcf: Rasm.Bim.Coordination.BcfStatus.Closed);
    public static readonly IssueStatus Reopened = new("reopened", bit: 4, bcf: Rasm.Bim.Coordination.BcfStatus.Reopened);

    public int Bit { get; }
    public Rasm.Bim.Coordination.BcfStatus Bcf { get; }

    private static readonly Lazy<FrozenDictionary<Rasm.Bim.Coordination.BcfStatus, IssueStatus>> ByBcf =
        new(static () => Items.ToFrozenDictionary(static row => row.Bcf));

    public static IssueStatus FromBcf(Rasm.Bim.Coordination.BcfStatus status) =>
        ByBcf.Value.TryGetValue(status, out IssueStatus? row) ? row : Reopened;
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record IssueBinding(string ViewpointGuid, Viewpoint View);

public sealed record CommentEntry(
    string CommentId,
    string Author,
    string Text,
    Option<string> ViewpointGuid,
    bool Resolved,
    Instant Date,
    Option<Instant> ModifiedAt = default,
    string ModifiedBy = "",
    Option<ulong> Editor = default);

// Source is the consumed contract row kept once at the boundary: the widened BcfTopic columns the
// board never edits (description, assignment, stage, due date, labels, provenance, references,
// snippet, files, status label) ride it through ToTopic untouched, so the round-trip stays lossless.
public sealed record Issue(
    string Guid,
    string Title,
    IssueStatus Status,
    string TopicType,
    string Priority,
    string Author,
    Instant CreatedAt,
    Seq<IssueBinding> Bindings,
    Seq<CommentEntry> Comments,
    Option<string> SnapshotKey,
    Option<Rasm.Bim.Coordination.BcfTopic> Source = default) {
    public static Fin<Issue> FromTopic(Rasm.Bim.Coordination.BcfTopic topic, ClockPolicy clocks) =>
        Fin.Succ(new Issue(
            topic.Guid, topic.Title, IssueStatus.FromBcf(topic.Status), topic.TopicType, topic.Priority,
            topic.Author, topic.CreationDate,
            topic.Viewpoints.Map(vp => new IssueBinding(vp.Guid, ViewpointCodec.FromBcf(vp.Guid, vp, clocks))),
            topic.Comments.Map(static c => new CommentEntry(c.Guid, c.Author, c.Text, c.ViewpointGuid, false, c.Date, c.ModifiedDate, c.ModifiedAuthor)),
            topic.Viewpoints.Find(static vp => vp.Snapshot.IsSome).Map(static vp => vp.Guid),
            Some(topic)));

    // Board-edited columns land as a with-update on the carried source row; each viewpoint re-encodes
    // over its guid-matched source row so the widened viewpoint columns survive; StatusLabel clears
    // only on a board status change, so the project-vocabulary verbatim token survives an untouched pass.
    public Rasm.Bim.Coordination.BcfTopic ToTopic() =>
        Bindings.Map(binding => ViewpointCodec.ToBcf(
            binding.ViewpointGuid, binding.View,
            Source.Bind(topic => topic.Viewpoints.Find(vp => vp.Guid == binding.ViewpointGuid)))) switch {
            var viewpoints => Source.Match(
                Some: topic => topic with {
                    Title = Title, Status = Status.Bcf, TopicType = TopicType, Priority = Priority,
                    Comments = CommentLens.Materialize(Comments), Viewpoints = viewpoints,
                    StatusLabel = Status.Bcf == topic.Status ? topic.StatusLabel : "",
                },
                None: () => new Rasm.Bim.Coordination.BcfTopic(
                    Guid, Title, Status.Bcf, TopicType, Priority, Author, CreatedAt,
                    CommentLens.Materialize(Comments), viewpoints)),
        };
}
```

```mermaid
flowchart LR
    BcfTopic -->|FromTopic| Issue
    Issue --> IssueBinding
    IssueBinding --> Viewpoint
    Issue -->|CommentLens| CollabDoc
    Issue --> IssueTile
    Issue -->|ToTopic| BcfTopic
```

## [03]-[COMMENT_LENS]

- Owner: `CommentLens` — the comment conversation as a `Collab/sync.md` `CollabDoc` `map` container attach keyed by comment GUID; NO page-local CRDT and NO page-local write kernel exist — every live column write rides the `Collab/sync.md` `IntentApply` comment arms through the one `IntentLedger.Commit` rail, so the live register shape and the replay register shape are one dispatch by construction (the `CommentOp` `[Union]` + `CommentThread` register AND the duplicated page-local `WriteEntry` kernel are DROPPED root-up).
- Entry: `public static Fin<LoroMap> Thread(CollabDoc doc, string topicGuid)` — attaches the topic's comment map (`comments/{topicGuid}`); `public static IO<Fin<Unit>> Put(CollabDoc doc, IntentLedger ledger, string topicGuid, CommentEntry entry, ClockPolicy clocks)` — the ONE comment write verb: row existence discriminates the durable case, an absent GUID landing `EditIntent.CommentAdd` and an existing one `EditIntent.CommentEdit`, so add-versus-edit is recoverable from the merge authority's own state, never a caller flag and never an add-only misclassification; `public static IO<Fin<Unit>> Resolve(CollabDoc doc, IntentLedger ledger, string topicGuid, string commentId, ClockPolicy clocks)` — admits only an existing comment row, a missing GUID failing `IssueFault.CommentConflict` before the rail runs.
- Auto: each comment is one map key (its GUID) whose value is a nested mergeable map carrying the author, body, viewpoint-guid, resolved, at, edited-by, and edited-at columns — the `LoroDoc` map container IS the convergence law, so concurrent same-comment edits resolve through the one merge authority and the board holds no ordering, LWW, or merge algebra of its own; the mutation path is `IntentLedger.Commit` — durable projection FIRST, then the live apply through the same total `IntentApply` dispatch replay uses, the whole path one `IO<Fin<Unit>>` carrier the composition edge runs, so a durable refusal returns before any live mutation and a mid-pipeline `.Run()` inside a `Fin` transform is the deleted form; a superseded concurrent edit surfaces through the container diff feed (`CollabDoc.Changes`) for the presence UI, never silently dropped; the lens projects the container state to `Seq<CommentEntry>` for the `Issue` record — each row carrying its `GetLastEditor` peer provenance so the board attributes the last touch — and materializes to the `Rasm.Bim` `BcfComment` set for the topic round-trip.
- Packages: LoroCs (via `Collab/sync.md` owners), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.Bim (project)
- Growth: a new comment column is one nested-map key written by its `IntentApply` arm; zero new surface, zero new CRDT, zero new write kernel.
- Boundary: the comment thread rides the `Collab/sync.md#DOCUMENT_OWNER` map-container charter — the one merge authority; durable truth rides the `Collab/sync.md#DURABLE_INTENT` typed edit-intent stream (`CommentAdd`/`CommentEdit`/`CommentResolve` rows on the ONE union), so a comment op persisting as an opaque Loro byte, a parallel per-page op union, or a live write that bypasses the intent rail is the deleted form; the lens materializes to the `Rasm.Bim` `BcfComment` record — modification provenance included — for the topic round-trip so a board comment exports to the openBIM container, never an AppUi-local comment schema.

```csharp signature
public static class CommentLens {
    public const string BoardOrigin = "board";

    public static Fin<LoroMap> Thread(CollabDoc doc, string topicGuid) =>
        IntentApply.As<LoroMap>(doc, CollabContainer.Map, $"comments/{topicGuid}");

    // ONE write verb: the merge authority's own row state discriminates add-versus-edit, and the
    // mutation rides IntentLedger.Commit — durable-first, live apply through the replay dispatch.
    public static IO<Fin<Unit>> Put(CollabDoc doc, IntentLedger ledger, string topicGuid, CommentEntry entry, ClockPolicy clocks) =>
        IO.lift(() => Has(doc, topicGuid, entry.CommentId)).Bind(probe => probe.Match(
            Succ: exists => ledger.Commit(doc, exists
                    ? new EditIntent.CommentEdit(doc.Key, System.Guid.Parse(entry.CommentId), topicGuid, entry.Text, entry.Author, clocks.Now)
                    : new EditIntent.CommentAdd(doc.Key, System.Guid.Parse(entry.CommentId), topicGuid, entry.Text, entry.Author, entry.ViewpointGuid, clocks.Now),
                BoardOrigin),
            Fail: static error => IO.pure(Fin.Fail<Unit>(error))));

    // Resolve gates on row existence: a resolve of a GUID the thread never held would mint an orphan
    // row replay cannot rehydrate, so a missing comment fails the rail before the durable projection.
    public static IO<Fin<Unit>> Resolve(CollabDoc doc, IntentLedger ledger, string topicGuid, string commentId, ClockPolicy clocks) =>
        IO.lift(() => Has(doc, topicGuid, commentId)).Bind(probe => probe.Match(
            Succ: exists => exists
                ? ledger.Commit(doc, new EditIntent.CommentResolve(doc.Key, System.Guid.Parse(commentId), topicGuid, clocks.Now), BoardOrigin)
                : IO.pure(Fin.Fail<Unit>(new IssueFault.CommentConflict($"resolve: no comment row {commentId}"))),
            Fail: static error => IO.pure(Fin.Fail<Unit>(error))));

    public static Fin<Seq<CommentEntry>> Project(CollabDoc doc, string topicGuid) =>
        Thread(doc, topicGuid).Bind(thread => CollabDoc.Lift(() => ReadEntries(thread)));

    public static Seq<Rasm.Bim.Coordination.BcfComment> Materialize(Seq<CommentEntry> comments) =>
        comments.OrderBy(static entry => entry.Date, Comparer<Instant>.Default)
            .Map(static entry => new Rasm.Bim.Coordination.BcfComment(
                entry.CommentId, entry.Author, entry.Text, entry.ViewpointGuid, entry.Date, entry.ModifiedAt, entry.ModifiedBy))
            .ToSeq();

    // Existence probes ride Keys() — zero transient container handles minted for a yes/no answer.
    static Fin<bool> Has(CollabDoc doc, string topicGuid, string commentId) =>
        Thread(doc, topicGuid).Bind(thread =>
            CollabDoc.Lift(() => thread.Keys().Contains(KeyOf(commentId))));

    static string KeyOf(string commentId) => System.Guid.Parse(commentId).ToString("N");

    static Seq<CommentEntry> ReadEntries(LoroMap thread) =>
        thread.Keys().AsIterable()
            .Map(key => Read(thread, key))
            .Somes()
            .ToSeq();

    // Transient read handle: the nested row wrapper frees before return, per the sync handle law.
    static Option<CommentEntry> Read(LoroMap thread, string key) {
        using LoroMap? row = thread.Get(key)?.AsLoroMap();
        return Optional(row).Bind(live => EntryOf(thread, key, live));
    }

    // Read-side projection over the register the IntentApply arms write; GetLastEditor is the loro
    // per-key provenance the board's attribution column reads.
    static Option<CommentEntry> EntryOf(LoroMap thread, string key, LoroMap row) =>
        (Str(row, "author"), Str(row, "body"), Stamp(row, "at")).Apply((author, body, at) =>
            new CommentEntry(
                System.Guid.ParseExact(key, "N").ToString(), author, body,
                Str(row, "viewpoint"), Flag(row, "resolved"), at,
                Stamp(row, "edited-at"), Str(row, "edited-by").IfNone(string.Empty),
                Optional(thread.GetLastEditor(key))));

    static Option<string> Str(LoroMap row, string key) =>
        row.Get(key)?.AsValue() is LoroValue.String s ? Some(s.Value) : None;

    static bool Flag(LoroMap row, string key) =>
        row.Get(key)?.AsValue() is LoroValue.Bool b && b.Value;

    static Option<Instant> Stamp(LoroMap row, string key) =>
        row.Get(key)?.AsValue() is LoroValue.I64 at ? Some(Instant.FromUnixTimeMilliseconds(at.Value)) : None;
}
```

## [04]-[ISSUE_TILE]

- Owner: `IssueTile` the dashboard-tile projection of an issue; `IssueFilter` the cross-filter status bitset.
- Entry: `public static Seq<IssueTile> Project(IssueBoard board, IssueFilter filter)` — projects the board's issues onto the dashboard tile family under the status cross-filter; the tile list is the dashboard's issue lane, never a second list owner; `public static IssueFilter Of(params ReadOnlySpan<IssueStatus> rows)` — the bitset builder folding status rows through their own `Bit` column, arity absorbed by the span.
- Auto: each issue projects onto one dashboard tile carrying its title, status, priority, author, and snapshot key so the board's issues render as the dashboard tile lane; the status cross-filter is the dashboard bitset brushing so selecting a status in one tile brushes the issue list exactly as the chart dashboard cross-filters — the bit position is the `IssueStatus.Bit` row column, never a hand-mapped ordinal switch; the snapshot tile renders the issue's bound viewpoint thumbnail through the visuals capture lane so the dashboard shows each issue's view without a second render owner.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new tile field is one `IssueTile` member; a new filter axis is one `IssueFilter` bitset column; zero new surface.
- Boundary: the issue list rides the `charts-dashboards` dashboard tile family with the cross-filter bitset brushing so the board reuses the dashboard owner and a second tile or list owner is the deleted form; the status filter is the dashboard bitset so a per-tile filter flag is the rejected form; the snapshot tile renders through the visuals capture lane so the board mints no second render owner — the tile is the issue's bound `Viewpoint` rendered through the settled capture row.

```csharp signature
public readonly record struct IssueFilter(uint StatusMask) {
    public static readonly IssueFilter All = new(uint.MaxValue);

    public static IssueFilter Of(params ReadOnlySpan<IssueStatus> rows) =>
        new(LanguageExt.Iterable<IssueStatus>.FromSpan(rows).Fold(0u, static (mask, row) => mask | (1u << row.Bit)));

    public bool Admits(IssueStatus status) => (StatusMask & (1u << status.Bit)) != 0u;
}

public sealed record IssueTile(string Guid, string Title, IssueStatus Status, string Priority, string Author, Option<string> SnapshotKey);

public static class IssueTiles {
    public static Seq<IssueTile> Project(IssueBoard board, IssueFilter filter) =>
        board.Issues
            .Filter(issue => filter.Admits(issue.Status))
            .Map(static issue => new IssueTile(issue.Guid, issue.Title, issue.Status, issue.Priority, issue.Author, issue.SnapshotKey));
}
```

## [05]-[BOARD_PROJECTION]

- Owner: `IssueBoard` the board projection owning the issue set and the BCF round-trip.
- Entry: `public static Fin<IssueBoard> Load(Seq<BcfTopic> topics, ClockPolicy clocks)` — folds a `Rasm.Bim`-read BCF topic set into the board issues; `public Fin<IssueBoard> Synced(CollabDoc doc)` — re-projects every issue's comment set FROM the merge authority through `CommentLens.Project`, so a refreshed board is a pure read of the shared document and can never materialize comment state replay cannot reconstruct; `public Fin<Seq<BcfTopic>> Save()` — projects the board issues back onto the BCF topic set for the `Rasm.Bim` archive writer, so the board round-trips through the openBIM container.
- Auto: the board folds each BCF topic into one `Issue` binding its viewpoints onto the AppUi `Viewpoint`, its comments onto the shared map container, and its snapshot onto the tile so the board is the projection over the topic set; the board owns the issue-to-viewpoint binding so navigating to an issue applies its bound `Viewpoint` onto the viewport camera and section through the viewpoint codec; the board's durable state rides the `Collab/sync.md#DURABLE_INTENT` typed edit-intent stream — a board edit is one intent row on the one union through the one commit rail, never a board-local receipt or store; the save projects each issue back to a `BcfTopic` so the `Rasm.Bim` `BcfArchive.Write` emits the `.bcfzip` and the round-trip is one vocabulary.
- Receipt: board and comment durability is the one edit-intent stream; a board edit projects one `EditIntent` row.
- Packages: LanguageExt.Core, NodaTime, Rasm.Bim (project), Rasm.Persistence (project)
- Growth: a new board view is one projection over the issue set; zero new surface.
- Boundary: the board is the PROJECTION over the issue set and owns the issue-to-viewpoint binding so navigating to an issue applies its bound `Viewpoint` onto the viewport through the viewpoint codec — the board owns the binding, never the BCF semantic schema; a board operation that replaces collaboration state with a caller-supplied value is the deleted form — comment state enters the board only through `Synced`'s merge-authority read, so every exposed mutation path is either an intent on the one union or a pure re-projection; the board round-trips through the `Rasm.Bim/Review/issues#BCF_ARCHIVE` `BcfArchive.Read`/`Write` so AppUi reads and writes the openBIM container through the `Rasm.Bim` codec and a direct `.bcfzip`/BCF-XML writer here is the rejected form; the board's durable truth is the edit-intent stream and its live convergence is the one `CollabDoc` — a board-local store or second sync is the deleted form.

```csharp signature
public sealed record IssueBoard(string Key, Seq<Issue> Issues) {
    public static Fin<IssueBoard> Load(Seq<Rasm.Bim.Coordination.BcfTopic> topics, ClockPolicy clocks) =>
        topics.Traverse(topic => Issue.FromTopic(topic, clocks)).As()
            .Map(issues => new IssueBoard("coordination", issues.ToSeq()));

    public Fin<Seq<Rasm.Bim.Coordination.BcfTopic>> Save() =>
        Fin.Succ(Issues.Map(static issue => issue.ToTopic()));

    public Option<Viewpoint> Navigate(string guid) =>
        Issues.Find(issue => issue.Guid == guid).Bind(static issue => issue.Bindings.HeadOrNone().Map(static binding => binding.View));

    // Comment state enters the board ONLY as a read of the merge authority — a caller-supplied
    // comment set would mint state the ledger replay cannot reconstruct.
    public Fin<IssueBoard> Synced(CollabDoc doc) =>
        Issues.Traverse(issue => CommentLens.Project(doc, issue.Guid).Map(comments => issue with { Comments = comments })).As()
            .Map(issues => this with { Issues = issues.ToSeq() });
}
```

## [06]-[RESEARCH]

- [BCF_TOPIC_SEAM]: the `Rasm.Bim/Review/issues#BCF_ARCHIVE` `BcfTopic`/`BcfComment`/`BcfViewpoint` record member set the board consumes at the boundary is the finalized `Rasm.Bim.Coordination` surface — the topic core columns (GUID/title/status/type/priority/author/creation-instant) plus the trailing-defaulted widened columns (description, assignment, stage, due date, labels, index, provenance, server id, reference links, related topics, document references, snippet, header files, `StatusLabel`) the carried source row preserves, the comment GUID/author/text/viewpoint-guid/date/`ModifiedDate`/`ModifiedAuthor` columns, and the viewpoint `BcfCamera` `Perspective`/`Orthogonal` union, `SelectedGlobalIds`, `VisibilityExceptions`/`DefaultVisibility` pair, `Snapshot`, `Coloring`, and `ClippingPlanes` columns anchored on IFC GlobalIds, with the closed five-state `BcfStatus` enum riding the `IssueStatus.Bcf` row column; the `BcfViewpoint`-to-AppUi-`Viewpoint` projection (the `BcfCamera` position-direction-up-to-`ViewCamera` eye-target-up correspondence with per-arm `FieldOfViewDeg`/`ViewToWorldScale` scalars, and the `SelectedGlobalIds`/`VisibilityExceptions` sets under the `DefaultVisibility` convention) is the one `Render/pipeline.md#VIEWPOINT_CODEC` `ViewpointCodec.FromBcf`/`ToBcf` over the consumed contract — the board folds each topic through that single codec and re-mints no second viewpoint mapping; the exact `Rasm.Bim` BCF record column spellings and namespace are the package-edge surface composed through the codec, never re-minted.
