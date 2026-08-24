# [UI_REVIEW]

Review joins `ModelDiff` changes and BCF issues by `GlobalId`, folds each topic's comments into one reply tree, then projects one board, selection, tint, and reveal model.

## [01]-[INDEX]

- [02]-[CHANGE_VOCABULARY]: wire-closed change and issue-status rows; `Review`.
- [03]-[ROW_JOIN]: keyed fold joining changes, BCF issues, and their reply trees; `Review`.
- [04]-[BOARD_COLUMNS]: `Grid` column definitions and the census projections the board header reads; `Review`.
- [05]-[ECHO_ROWS]: selection ops, scene tint, and the per-element camera reveal; `Review`.

## [02]-[CHANGE_VOCABULARY]

[CHANGE_VOCABULARY]:
- Owner: `Review` closes presentation against `Wire.ModelDiff` change kinds and `Wire.BcfTopic` statuses.
- Law: each row carries token tone and every selection, residency, blocking, and ordering decision that consumes the key.
- Law: both vocabularies are the corpus's own — the change roster is the generated `kind` oneof's case names, the status roster the `BcfStatus` members `viewer/mark` narrows — so every table keys on a generated spelling, closes against it both ways, and mints no token of its own.
- Boundary: `Review.rows` is the model-diff byte-ingress seam; BCF topics arrive decoded from their owning admission path.

```typescript signature
import { Wire } from "@rasm/core"
import { BcfStatus } from "@rasm\/contracts/rasm/contracts/bcf/bcf_pb"
import { Record } from "effect"
import type { LucideIcon } from "lucide-react"
import { CircleMinus, CirclePlus, Combine, GitBranch, Move3d, PencilLine } from "lucide-react"
import type { Theme } from "../../src/system/token.ts"
import { Mark, type Selection } from "./mark.ts"

// the change roster IS the generated oneof's case space: a row per case, keyed on the case name the corpus emits
const _changeRows = {
  added: { icon: CirclePlus, tone: "success", resident: true, op: "Add", rank: 2 },
  removed: { icon: CircleMinus, tone: "danger", resident: false, op: "Subtract", rank: 0 },
  modified: { icon: PencilLine, tone: "accent", resident: true, op: "Add", rank: 3 },
  moved: { icon: Move3d, tone: "accent", resident: true, op: "Add", rank: 4 },
  split: { icon: GitBranch, tone: "warning", resident: true, op: "Add", rank: 1 },
  merged: { icon: Combine, tone: "warning", resident: true, op: "Add", rank: 1 },
} as const satisfies { readonly [K in Review.Change]: Review.ChangeRow }

const _statusRows = {
  [BcfStatus.OPEN]: { tone: "danger", blocking: true },
  [BcfStatus.IN_PROGRESS]: { tone: "warning", blocking: true },
  [BcfStatus.RESOLVED]: { tone: "success", blocking: false },
  [BcfStatus.CLOSED]: { tone: "neutral", blocking: false },
  [BcfStatus.REOPENED]: { tone: "danger", blocking: true },
} as const satisfies { readonly [K in Review.Status]: { readonly tone: Theme.Tone; readonly blocking: boolean } }

declare namespace Review {
  // the set arm `{ case: undefined }` the corpus's `oneof required` rule refuses is excluded at the type, so the
  // table closes over the six cases a producer can emit
  type Change = Exclude<Wire.ModelDiff["changes"][number]["kind"]["case"], undefined>
  type Status = Mark.Status
  type ChangeRow = {
    readonly icon: LucideIcon
    readonly tone: Theme.Tone
    readonly resident: boolean
    readonly op: Selection.Op["_tag"]
    readonly rank: number
  }
  // both tables close BOTH ways: `satisfies` refuses an excess row and these aliases refuse a generated case or
  // member without one. effect's `Record` namespace shadows the global utility type across this whole module, so
  // every string-keyed record shape here spells `Record.ReadonlyRecord`
  type _ChangeGap<K extends keyof typeof _changeRows = Change> = K
  type _StatusGap<K extends keyof typeof _statusRows = Status> = K
}
```

## [03]-[ROW_JOIN]

[ROW_JOIN]:
- Owner: `Review.rows` admits `ModelDiffWire` bytes through `Wire.decode`, then folds all change and viewpoint anchors into one branded-key map.
- Law: `Option<Change>` represents issue-only rows; ordering reads the change registry through `_rank` and falls back to the admitted `GlobalId`, so the board's ordering and its sortable rank column read one function.
- Law: `Review.census` counts changes, statuses, and blocking issues in the row fold.
- Law: comments fold into ONE reply tree off `replyToGuid` — a root comment carries `None`, and a reply whose parent left the topic, or whose chain reaches no root at all, re-roots instead of vanishing: the wire admits a deleted parent, asserts no acyclicity, and a comment dropped from the tree is evidence lost.
- Law: siblings hold wire time order at every depth, so a thread reads as it was written and no consumer re-sorts it.

```typescript signature
import { Array, DateTime, Effect, HashMap, Match, Option, Order, type ParseResult, pipe } from "effect"

type GlobalId = Selection.Id
type Comment = Wire.BcfTopic["comments"][number]
type Change = Wire.ModelDiff["changes"][number]

declare namespace Review {
  type Note = {
    readonly guid: string
    readonly author: string
    readonly text: string
    readonly at: Option.Option<DateTime.Utc> // absent exactly where the producer omitted the comment's instant
    readonly replies: ReadonlyArray<Review.Note>
  }
  type Issue = {
    readonly guid: string
    readonly title: string
    readonly status: Status
    readonly thread: ReadonlyArray<Review.Note>
  }
  type Row = {
    readonly anchor: GlobalId
    readonly change: Option.Option<Change>
    readonly attributes: ReadonlyArray<string>
    readonly issues: ReadonlyArray<Issue>
  }
  type Census = {
    readonly changes: Record.ReadonlyRecord<Change, number>
    readonly statuses: HashMap.HashMap<Status, number> // enum members key a map, never a string-keyed record
    readonly blocking: number
  }
}

const _SEED: Omit<Review.Row, "anchor"> = { change: Option.none(), attributes: [], issues: [] }

const _rank = (row: Review.Row): number =>
  Option.match(row.change, { onNone: () => Number.MAX_SAFE_INTEGER, onSome: (kind) => _changeRows[kind].rank })

const _order: Order.Order<Review.Row> = Order.combine(
  Order.mapInput(Order.number, _rank),
  Order.mapInput(Order.string, (row: Review.Row) => row.anchor),
)

const _touch = (
  held: HashMap.HashMap<GlobalId, Review.Row>,
  anchor: GlobalId,
  edit: (row: Review.Row) => Review.Row,
): HashMap.HashMap<GlobalId, Review.Row> =>
  HashMap.modifyAt(held, anchor, (slot) =>
    Option.some(edit(Option.getOrElse(slot, () => ({ ..._SEED, anchor })))))

// the wire's own oneof face routes the fold: every case reads its own value column by name, the end arms share
// one projection because they share one message, and the unset arm the corpus rule refuses contributes nothing
const _ends = (end: Extract<Change["kind"], { readonly case: "added" | "removed" }>["value"]): ReadonlyArray<string> =>
  Array.appendAll(Array.fromNullable(end.classification?.code), [end.predefined])

const _attributes = (change: Change): ReadonlyArray<string> =>
  Match.value(change.kind).pipe(
    Match.when({ case: "added" }, ({ value }) => _ends(value)),
    Match.when({ case: "removed" }, ({ value }) => _ends(value)),
    Match.when({ case: "modified" }, ({ value }) => Array.map(value.deltas, (delta) => delta.path)),
    Match.when({ case: "moved" }, () => ["placement"]),
    Match.when({ case: "split" }, ({ value }) => value.counterparts),
    Match.when({ case: "merged" }, ({ value }) => value.counterparts),
    Match.orElse(() => Array.empty<string>()),
  )

// the anchor and the case ride every arm's own `globalId`; the decode is the brand narrowing onto the set's
// element and the unset arm answers none, which is exactly the member the corpus rule refused
const _changed = (change: Change): Option.Option<{ readonly anchor: GlobalId; readonly kind: Review.Change }> =>
  pipe(change.kind, (face) =>
    face.case === undefined
      ? Option.none()
      : Option.map(Selection.decode(face.value.globalId), (anchor) => ({ anchor, kind: face.case })))

const _ROOT = "" // a wire comment guid is NonEmptyString, so the empty key can only ever be the synthetic root bucket

const _byWritten = Order.mapInput(Option.getOrder(DateTime.Order), (comment: Comment) => Mark.instant(comment.date))

const _thread = (comments: ReadonlyArray<Comment>): ReadonlyArray<Review.Note> => {
  const held = Record.fromIterableWith(comments, (comment) => [comment.guid, comment] as const)
  // budget carries the comment count, so a ring terminates at the length of the topic rather than the stack
  const anchored = (guid: string, budget: number): boolean =>
    Option.match(Record.get(held, guid), {
      onNone: () => false,
      onSome: (comment) =>
        Option.match(Option.fromNullable(comment.replyToGuid), {
          onNone: () => true,
          onSome: (parent) => budget > 0 && anchored(parent, budget - 1),
        }),
    })
  const branch = Array.groupBy(Array.sort(comments, _byWritten), (comment) =>
    anchored(comment.guid, comments.length) ? (comment.replyToGuid ?? _ROOT) : _ROOT)
  const grow = (guid: string): ReadonlyArray<Review.Note> =>
    Array.map(Option.getOrElse(Record.get(branch, guid), (): ReadonlyArray<Comment> => []), (comment) => ({
      guid: comment.guid,
      author: comment.author,
      text: comment.text,
      at: Mark.instant(comment.date),
      replies: grow(comment.guid),
    }))
  return grow(_ROOT)
}

const _rows = (
  diff: Uint8Array,
  issues: ReadonlyArray<Wire.BcfTopic>,
): Effect.Effect<ReadonlyArray<Review.Row>, Wire.Fault | ParseResult.ParseError> =>
  Effect.map(
    Wire.decode("ModelDiffWire", diff),
    (admitted) => {
      const changed = Array.reduce(
        Array.filterMap(admitted.changes, (change) => Option.map(_changed(change), (keyed) => ({ ...keyed, change }))),
        HashMap.empty<GlobalId, Review.Row>(),
        (held, { anchor, kind, change }) => _touch(held, anchor, (row) => ({
          ...row,
          change: Option.some(kind),
          attributes: _attributes(change),
        })),
      )
      return Array.sort(
        HashMap.values(
          // the status narrowing is `viewer/mark`'s one seat; an issue it refuses is the member the corpus rule already
          // refused, so the fold is total over every document a producer emits
          Array.reduce(Array.filterMap(issues, (issue) => Option.map(Mark.keys(issue), (keys) => ({ issue, status: keys.status }))), changed, (held, { issue, status }) => {
            // reply tree folds once per topic, never once per anchor: a topic naming forty elements would otherwise rebuild
            // that same thread forty times and hand each row a different object identity for one discussion
            const thread = _thread(issue.comments)
            return Array.reduce(issue.viewpoints, held, (topics, viewpoint) =>
              Array.reduce(Array.filterMap(viewpoint.selectedGlobalIds, Selection.decode), topics, (rows, anchor) =>
                _touch(rows, anchor, (row) => ({
                  ...row,
                  issues: [...row.issues, { guid: issue.guid, title: issue.title, status, thread }],
                }))))
          }),
        ),
        _order,
      )
    },
  )

const _ZERO: Review.Census = {
  changes: Record.map(_changeRows, () => 0),
  statuses: HashMap.fromIterable(Array.map(Mark.statuses.members, (status) => [status, 0] as const)),
  blocking: 0,
}

const _census = (rows: ReadonlyArray<Review.Row>): Review.Census =>
  Array.reduce(rows, _ZERO, (census, row) =>
    Array.reduce(row.issues, {
      ...census,
      changes: Option.match(row.change, {
        onNone: () => census.changes,
        onSome: (kind) => ({ ...census.changes, [kind]: census.changes[kind] + 1 }),
      }),
    }, (inner, issue) => ({
      ...inner,
      statuses: HashMap.modify(inner.statuses, issue.status, (held) => held + 1),
      blocking: inner.blocking + (_statusRows[issue.status].blocking ? 1 : 0),
    })))
```

## [04]-[BOARD_COLUMNS]

[BOARD_COLUMNS]:
- Owner: `Review.columns` contributes typed columns to the shared grid.
- Law: change and issue cells expose registry keys; shared table state owns sorting, filtering, selection, and windowing.
- Law: the columns narrow through the grid's one feature object, so every `sortFn` a column names is a key that object's `sortFns` slot registers.
- Law: that same object registers ONE `columnMeta` type, so a board column carries the whole band record the grid's own column plane carries — cell behavior row, quantity facts, role, nullability, the identity marker — and a board column with no wire quantity states `Option.none()` rather than dropping the field.
- Law: identity rides the marker and nothing else — the `anchor` column carries `identity: true` and every other column states `false`, so the grid resolves the branded `GlobalId` through that column's own accessor and `RowSelectionState` keys on the same brand `viewer/mark` holds. Row-typed callbacks in the shared meta pin the one registered `columnMeta` to a single row shape and break it for the band fold at the other end.
- Law: domain order crosses as DATA — the change column's VALUE is this vocabulary's own rank and the def names the grid's domain-blind `rank` key, while the painted cell stays the token. Sort keys named for this vocabulary seat viewer words in the view tier, which is the strata inversion.

```typescript signature
import { createColumnHelper, type ColumnDef } from "@tanstack/react-table"
import type { Grid } from "../view/table.ts"

const _helper = createColumnHelper<Grid.Features, Review.Row>()

const _columns: ReadonlyArray<ColumnDef<Grid.Features, Review.Row, unknown>> = [
  _helper.accessor((row) => row.anchor, {
    id: "anchor",
    header: "anchor",
    sortFn: "text",
    // grid reads identity through THIS key column's accessor, so the branded anchor reaches row
    // selection, the export parcel, and the scene echo without any of the three re-deriving it
    meta: {
      cell: { render: "text", align: "start", measured: false, editable: false, sort: "text", filter: "includesString" },
      dimension: Option.none(),
      precision: Option.none(),
      role: "key",
      nullable: false,
      identity: true,
    },
  }),
  _helper.accessor(_rank, {
    id: "change",
    header: "change",
    sortFn: "rank",
    // board rows join a wire, never a feed band, so quantity facts are absent by construction while role and nullability
    // stay real column facts, and `editable` stays false across the board because a comment commits at the issue port.
    // sort reads the rank while `token` paints the row's own icon and tone, so severity order never becomes
    // a number on screen and an issue-only row sorts last without a sentinel token
    meta: {
      cell: { render: "token", align: "start", measured: false, editable: false, sort: "rank", filter: "equals" },
      dimension: Option.none(),
      precision: Option.none(),
      role: "category",
      nullable: true,
      identity: false,
    },
  }),
  _helper.accessor((row) => row.attributes.length, {
    id: "attributes",
    header: "attributes",
    meta: {
      cell: { render: "number", align: "end", measured: true, editable: false, sort: "alphanumeric", filter: "inNumberRange" },
      dimension: Option.none(),
      precision: Option.none(),
      role: "measure",
      nullable: false,
      identity: false,
    },
  }),
  _helper.accessor((row) => Array.map(row.issues, (issue) => issue.status), {
    id: "issues",
    header: "issues",
    meta: {
      cell: { render: "text", align: "start", measured: false, editable: false, sort: "text", filter: "includesString" },
      dimension: Option.none(),
      precision: Option.none(),
      role: "category",
      nullable: false,
      identity: false,
    },
  }),
  // roots, not comments: the count answers how many discussions an element carries, which only the reply fold knows —
  // a raw comment length counts every reply as its own conversation
  _helper.accessor((row) => Array.flatMap(row.issues, (issue) => issue.thread).length, {
    id: "threads",
    header: "threads",
    meta: {
      cell: { render: "number", align: "end", measured: true, editable: false, sort: "alphanumeric", filter: "inNumberRange" },
      dimension: Option.none(),
      precision: Option.none(),
      role: "measure",
      nullable: false,
      identity: false,
    },
  }),
]
```

## [05]-[ECHO_ROWS]

[ECHO_ROWS]:
- Owner: `Review.echo`, `Review.tint`, and `Review.reveal` project resident change rows without new state.
- Law: the caller resolves branded ids to `Wire.GeoFeature.Extent`; review emits one selection op or camera intent.
- Law: a viewpoint CARRYING a camera restores through `mark#VIEWPOINT_RESTORE`, which owns the `LookAt` mint from the wire block, so this owner answers `None` and re-derives nothing. Camera-less viewpoints anchor SELECTION only: the board frames each viewpoint's own selected band from the caller's extent read, and the echo and the tint land unchanged either way.

```typescript signature
import { Camera } from "./geo.ts"
import { Selection } from "./mark.ts"

type Extent = typeof Wire.GeoFeature.Extent.Type

const _resident = (rows: ReadonlyArray<Review.Row>, kinds: ReadonlyArray<Review.Change>): ReadonlyArray<GlobalId> =>
  Array.filterMap(rows, (row) =>
    Option.filterMap(row.change, (kind) =>
      Array.contains(kinds, kind) && _changeRows[kind].resident ? Option.some(row.anchor) : Option.none()))

const _echo = (rows: ReadonlyArray<Review.Row>, kinds: ReadonlyArray<Review.Change>): Selection.Op =>
  Selection.Op.Replace({ ids: _resident(rows, kinds) }) // one op through the one fold: the board never holds a second set

const _tint = (rows: ReadonlyArray<Review.Row>): ReadonlyArray<readonly [GlobalId, Theme.Tone]> =>
  Array.filterMap(rows, (row) =>
    Option.filterMap(row.change, (kind) =>
      _changeRows[kind].resident ? Option.some([row.anchor, _changeRows[kind].tone] as const) : Option.none()))

const _reveal = (
  rows: ReadonlyArray<Review.Row>,
  viewpoint: Option.Option<Wire.BcfViewpoint>,
  extent: (ids: ReadonlyArray<GlobalId>) => Option.Option<Extent>,
  padding: number,
): Option.Option<Camera.Intent> =>
  Option.flatMap(
    Option.match(viewpoint, {
      // with no viewpoint the board frames what it tints, and a non-resident change frames nothing, so the tint set
      // doubles as the fit set; a viewpoint carrying its own camera yields nothing here and the restore fold answers
      onNone: () => Option.some(Array.map(_tint(rows), ([anchor]) => anchor)),
      onSome: (held) => held.camera === undefined ? Option.some(Array.filterMap(held.selectedGlobalIds, Selection.decode)) : Option.none(),
    }),
    (ids) => Option.map(extent(ids), (bounds) => Camera.Intent.FitBounds({ bounds, padding })),
  )

declare namespace Review {
  type Shape = {
    readonly change: typeof _changeRows
    readonly status: typeof _statusRows
    readonly rows: typeof _rows
    readonly rank: typeof _rank
    readonly order: typeof _order
    readonly thread: typeof _thread
    readonly census: typeof _census
    readonly columns: typeof _columns
    readonly echo: typeof _echo
    readonly tint: typeof _tint
    readonly reveal: typeof _reveal
  }
}

const Review: Review.Shape = {
  change: _changeRows,
  status: _statusRows,
  rows: _rows,
  rank: _rank,
  order: _order,
  thread: _thread,
  census: _census,
  columns: _columns,
  echo: _echo,
  tint: _tint,
  reveal: _reveal,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Review }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
