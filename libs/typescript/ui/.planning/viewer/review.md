# [UI_REVIEW]

Review joins `ModelDiff` changes and BCF issues by `GlobalId`, then projects one board, selection, tint, and reveal model.

## [01]-[INDEX]

- [02]-[CHANGE_VOCABULARY]: wire-closed change and issue-status rows; `Review`.
- [03]-[ROW_JOIN]: the keyed fold joining changes and BCF issues; `Review`.
- [04]-[BOARD_COLUMNS]: the `Grid` column definitions and the census projections the board header reads; `Review`.
- [05]-[ECHO_ROWS]: selection ops, scene tint, and the per-element camera reveal; `Review`.

## [02]-[CHANGE_VOCABULARY]

[CHANGE_VOCABULARY]:
- Owner: `Review` closes presentation against `Wire.ModelDiff` change kinds and `Wire.BcfTopic` statuses.
- Law: each row carries token tone and every selection, residency, blocking, and ordering decision that consumes the key.
- Boundary: core decodes the values; review only joins and projects them.

```typescript
import { Wire } from "@rasm/ts/core"
import type { LucideIcon } from "lucide-react"
import { CircleMinus, CirclePlus, Combine, GitBranch, Move3d, PencilLine } from "lucide-react"
import type { Theme } from "../../src/system/token.ts"
import type { Selection } from "./mark.ts"

const _changes = ["Added", "Removed", "Modified", "Moved", "Split", "Merged"] as const
const _changeRows = {
  Added: { icon: CirclePlus, tone: "success", resident: true, op: "Add", rank: 2 },
  Removed: { icon: CircleMinus, tone: "danger", resident: false, op: "Subtract", rank: 0 },
  Modified: { icon: PencilLine, tone: "accent", resident: true, op: "Add", rank: 3 },
  Moved: { icon: Move3d, tone: "accent", resident: true, op: "Add", rank: 4 },
  Split: { icon: GitBranch, tone: "warning", resident: true, op: "Add", rank: 1 },
  Merged: { icon: Combine, tone: "warning", resident: true, op: "Add", rank: 1 },
} as const

const _statuses = ["open", "in-progress", "resolved", "closed", "reopened"] as const
const _statusRows = {
  open: { tone: "danger", blocking: true },
  "in-progress": { tone: "warning", blocking: true },
  resolved: { tone: "success", blocking: false },
  closed: { tone: "neutral", blocking: false },
  reopened: { tone: "danger", blocking: true },
} as const

declare namespace Review {
  type Change = Wire.ModelDiff["changes"][number]["kind"]
  type Status = Wire.BcfTopic["status"]
  type ChangeRow = {
    readonly icon: LucideIcon
    readonly tone: Theme.Tone
    readonly resident: boolean
    readonly op: Selection.Op["_tag"]
    readonly rank: number
  }
  type _Changes<T extends Record<Change, ChangeRow> = typeof _changeRows> = T
  type _Statuses<T extends Record<Status, { readonly tone: Theme.Tone; readonly blocking: boolean }> = typeof _statusRows> = T
}
```

## [03]-[ROW_JOIN]

[ROW_JOIN]:
- Owner: `Review.rows` folds all change and viewpoint anchors into one branded-key map.
- Law: `Option<Change>` represents issue-only rows; ordering reads the change registry and falls back to the admitted `GlobalId`.
- Law: `Review.census` counts changes, statuses, and blocking issues in the row fold.

```typescript
import { Array, HashMap, Option, Order, Record } from "effect"

type GlobalId = typeof Wire.BcfViewpoint.GlobalId.Type

declare namespace Review {
  type Issue = { readonly guid: string; readonly title: string; readonly status: Status }
  type Row = {
    readonly anchor: GlobalId
    readonly change: Option.Option<Change>
    readonly attributes: ReadonlyArray<string>
    readonly issues: ReadonlyArray<Issue>
  }
  type Census = {
    readonly changes: Record.ReadonlyRecord<Change, number>
    readonly statuses: Record.ReadonlyRecord<Status, number>
    readonly blocking: number
  }
}

const _SEED: Omit<Review.Row, "anchor"> = { change: Option.none(), attributes: [], issues: [] }

const _order: Order.Order<Review.Row> = Order.combine(
  Order.mapInput(Order.number, (row: Review.Row) =>
    Option.match(row.change, { onNone: () => Number.MAX_SAFE_INTEGER, onSome: (kind) => _changeRows[kind].rank })),
  Order.mapInput(Order.string, (row: Review.Row) => row.anchor),
)

const _touch = (
  held: HashMap.HashMap<GlobalId, Review.Row>,
  anchor: GlobalId,
  edit: (row: Review.Row) => Review.Row,
): HashMap.HashMap<GlobalId, Review.Row> =>
  HashMap.modifyAt(held, anchor, (slot) =>
    Option.some(edit(Option.getOrElse(slot, () => ({ ..._SEED, anchor })))))

const _attributes = (change: Wire.ModelDiff["changes"][number]): ReadonlyArray<string> => {
  switch (change.kind) {
    case "Added":
    case "Removed":
      return [change.class.code, change.predefined]
    case "Modified":
      return Array.map(change.deltas, (delta) => delta.path)
    case "Moved":
      return ["placement"]
    case "Split":
      return change.into
    case "Merged":
      return change.from
  }
}

const _rows = (diff: Wire.ModelDiff, issues: ReadonlyArray<Wire.BcfTopic>): ReadonlyArray<Review.Row> => {
  const changed = Array.reduce(
    diff.changes,
    HashMap.empty<GlobalId, Review.Row>(),
    (held, change) => _touch(held, change.globalId, (row) => ({
      ...row,
      change: Option.some(change.kind),
      attributes: _attributes(change),
    })),
  )
  return Array.sort(
    HashMap.values(
      Array.reduce(issues, changed, (held, issue) =>
        Array.reduce(issue.viewpoints, held, (topics, viewpoint) =>
          Array.reduce(viewpoint.selectedGlobalIds, topics, (rows, anchor) =>
            _touch(rows, anchor, (row) => ({
              ...row,
              issues: [...row.issues, { guid: issue.guid, title: issue.title, status: issue.status }],
            })))),
      ),
    ),
    _order,
  )
}

const _ZERO: Review.Census = {
  changes: Record.map(_changeRows, () => 0),
  statuses: Record.map(_statusRows, () => 0),
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
      statuses: { ...inner.statuses, [issue.status]: inner.statuses[issue.status] + 1 },
      blocking: inner.blocking + (_statusRows[issue.status].blocking ? 1 : 0),
    })))
```

## [04]-[BOARD_COLUMNS]

[BOARD_COLUMNS]:
- Owner: `Review.columns` contributes typed columns to the shared grid.
- Law: change and issue cells expose registry keys; shared table state owns sorting, filtering, selection, and windowing.

```typescript
import { createColumnHelper, type ColumnDef } from "@tanstack/react-table"

const _helper = createColumnHelper<Review.Row>()

const _columns: ReadonlyArray<ColumnDef<Review.Row, unknown>> = [
  _helper.accessor((row) => Option.getOrUndefined(row.change), {
    id: "change",
    header: "change",
    sortingFn: "reviewChange", // the registry row lifting the vocabulary's own rank: no comparator is spelled at the column
    meta: { cell: "text", dimension: Option.none(), nullable: true, globalId: (row: Review.Row) => row.anchor },
  }),
  _helper.accessor((row) => row.attributes.length, {
    id: "attributes",
    header: "attributes",
    meta: { cell: "number", dimension: Option.none(), nullable: false },
  }),
  _helper.accessor((row) => Array.map(row.issues, (issue) => issue.status), {
    id: "issues",
    header: "issues",
    meta: { cell: "text", dimension: Option.none(), nullable: false },
  }),
]
```

## [05]-[ECHO_ROWS]

[ECHO_ROWS]:
- Owner: `Review.echo`, `Review.tint`, and `Review.reveal` project resident change rows without new state.
- Law: the caller resolves branded ids to `Wire.GeoFeature.Extent`; review emits one selection op or camera intent.

```typescript
import { Camera } from "./geo.ts"

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
  extent: (ids: ReadonlyArray<GlobalId>) => Option.Option<Extent>,
  padding: number,
): Option.Option<Camera.Intent> =>
  Option.map(
    extent(Array.map(_tint(rows), ([anchor]) => anchor)), // a non-resident change frames nothing, so the tint set IS the fit set
    (bounds) => Camera.Intent.FitBounds({ bounds, padding }),
  )

declare namespace Review {
  type Shape = {
    readonly change: typeof _changeRows
    readonly changes: typeof _changes
    readonly status: typeof _statusRows
    readonly statuses: typeof _statuses
    readonly rows: typeof _rows
    readonly order: typeof _order
    readonly census: typeof _census
    readonly columns: typeof _columns
    readonly echo: typeof _echo
    readonly tint: typeof _tint
    readonly reveal: typeof _reveal
  }
}

const Review: Review.Shape = {
  change: _changeRows,
  changes: _changes,
  status: _statusRows,
  statuses: _statuses,
  rows: _rows,
  order: _order,
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
