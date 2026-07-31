# [UI_REVIEW]

Review folds a decoded `BimDiff` and a decoded `IdsAudit` into ONE per-element row set keyed by `GlobalId`, so a change and every requirement verdict touching the same element arrive as one row. Change kinds and verdicts key the token authority's tone set; the board rides `view/table` rows, the tint and selection echoes ride `viewer/mark`'s op fold, and the reveal is a camera intent. This plane renders decoded truth and authors none. Module: `ui/viewer/src/review.ts`.

## [01]-[INDEX]

- [02]-[CHANGE_VOCABULARY]: the change-kind and verdict row tables closed against the wire, tone and op columns; `Review`.
- [03]-[ROW_JOIN]: the keyed fold joining diff changes and audit verdicts into one row set; `Review`.
- [04]-[BOARD_COLUMNS]: the `Grid` column definitions and the census projections the board header reads; `Review`.
- [05]-[ECHO_ROWS]: selection ops, scene tint, and the per-element camera reveal; `Review`.

## [02]-[CHANGE_VOCABULARY]

[CHANGE_VOCABULARY]:
- Owner: `Review` — one owner whose members are the vocabularies, the join, the column set, and the echo projections; `Review.change` is the change-kind table and `Review.verdict` the audit-verdict table, each keyed against the wire's own shape so a decoded-owner change breaks these rows loudly at compile time.
- Law: both vocabularies close AT THE WIRE — the change keys are exactly the `BimDiff` array fields (`added`/`removed`/`modified`) proved by a guard over `keyof BimDiff`, and the verdict keys are exactly the decoded `IdsAudit` verdict literals (`pass`/`fail`/`unapplicable`) proved by a guard over the landed union; a locally-widened change kind or a re-spelled verdict is the named defect, and the `unapplicable` spelling is the wire's, never normalized to a reader's preference.
- Law: tone is a KEY into the token authority, never a color — every row carries a `Theme.Tone` member and the surface resolves it through the one color authority, so this page holds no palette and a per-surface tone union has no place to exist; the same law binds the verdict table, so a failing requirement and a removed element read the same danger tone by construction rather than by two independent choices.
- Law: the change row carries the behavior every echo reads — `resident` states whether the kind still has live geometry (a removed element has none, so the tint arm skips it and the reveal falls back to its former neighbours), `op` names the `Selection.Op` case a row selection mints, and `rank` orders the board's default sort; a conditional over change names re-derives a column the row already carries.
- Law: a verdict row carries `blocking`, and it is the ONLY place severity is decided — the board's summary count, the tint's participation, and the review's own pass gate all read that column, so a surface asking "is this failure blocking" never re-answers it.
- Packages: `@rasm/ts/core` (`BimDiff`, `IdsAudit`, `BcfViewpoint.GlobalId` — the decoded owners and the one brand); `lucide-react` (the glyph rows — icon-as-identity); `system/token` (`Theme.Tone` — the closed tone set every row keys).
- Boundary: `Wire.decode("DiffWire", octets)` and `Wire.decode("IdsAuditWire", octets)` are the core interchange plane's; this page receives the decoded classes and never re-validates them.
- Growth: a new change kind is one wire field with its row; a new verdict is one wire literal with its row — never a second vocabulary beside either.

```typescript
import type { BimDiff, IdsAudit } from "@rasm/ts/core"
import type { LucideIcon } from "lucide-react"
import { CircleCheck, CircleMinus, CirclePlus, CircleSlash, PencilLine, TriangleAlert } from "lucide-react"
import type { Theme } from "../../src/system/token.ts"
import type { Selection } from "./mark.ts"

const _changes = ["added", "removed", "modified"] as const
const _verdicts = ["pass", "fail", "unapplicable"] as const

const _changeRows = {
  added: { icon: CirclePlus, tone: "success", resident: true, op: "Add", rank: 1 },
  removed: { icon: CircleMinus, tone: "danger", resident: false, op: "Subtract", rank: 0 },
  modified: { icon: PencilLine, tone: "accent", resident: true, op: "Add", rank: 2 },
} as const

const _verdictRows = {
  pass: { icon: CircleCheck, tone: "success", blocking: false },
  fail: { icon: TriangleAlert, tone: "danger", blocking: true },
  unapplicable: { icon: CircleSlash, tone: "neutral", blocking: false },
} as const

declare namespace Review {
  type Changes = typeof _changes
  type Change = keyof typeof _changeRows
  type Verdicts = typeof _verdicts
  type Verdict = keyof typeof _verdictRows
  type ChangeRow = {
    readonly icon: LucideIcon
    readonly tone: Theme.Tone
    readonly resident: boolean // a removed element has no live geometry: the tint arm skips it and the reveal falls back
    readonly op: Selection.Op["_tag"]
    readonly rank: number
  }
  type VerdictRow = { readonly icon: LucideIcon; readonly tone: Theme.Tone; readonly blocking: boolean }
  type _Changes<K extends keyof BimDiff = Change> = K // wire guard: a change key that is not a BimDiff field fails at the declaration
  type _ChangeGap<K extends Change = Exclude<keyof BimDiff, "base" | "next">> = K // reverse guard: a new BimDiff collection column demands its row here; the two non-change keys are the one carve
  type _ChangeRows<T extends Record.ReadonlyRecord<Changes[number], ChangeRow> = typeof _changeRows> = T
  type _Verdicts<K extends IdsAudit["verdicts"][number]["verdict"] = Verdict> = K // wire guard: a verdict outside the landed union fails here
  type _VerdictGap<K extends Verdict = IdsAudit["verdicts"][number]["verdict"]> = K // reverse guard: a widened wire verdict union demands its row here
  type _VerdictRows<T extends Record.ReadonlyRecord<Verdicts[number], VerdictRow> = typeof _verdictRows> = T
}
```

## [03]-[ROW_JOIN]

[ROW_JOIN]:
- Owner: `Review.rows(diff, audit)` — the keyed fold: one `Array.reduce` over `HashMap.modifyAt` visits every diff anchor and every audit anchor, so an element named by both a change and a requirement lands as ONE row carrying both, and an element named by only one lands carrying the other as an empty projection. Two passes over one collection, or a second map holding the audit half beside the diff half, restate the fold this member already is.
- Law: the join key is the one brand — `BcfViewpoint.GlobalId` is `Equal`-stable, so `HashMap` membership is structural and a rebuilt equal id is recognized; a raw string key beside the branded one misses silently, which is exactly the class of bug a review board cannot survive.
- Law: absence is shaped, never sentinel — a row's `change` is `Option<Review.Change>` because an audit names an element the diff never touched, and its `attributes` is an empty array for a change kind that names none; a `"none"` change key standing for the absent case widens a vocabulary closed against the wire.
- Law: the row set is ordered by the vocabulary, never by arrival — the default order is the change row's `rank` then the anchor, composed once as an `Order` instance riding the owner, so the board, the census, and the reveal walk one sequence and a component never re-sorts.
- Law: the census is a projection, never a second traversal — `Review.census(rows)` folds one seeded pass into the per-kind and per-verdict counts the board header reads, and the blocking count reads the verdict row's own column; four `Array.filter().length` reads over one row set is the multi-pass scatter this fold deletes.
- Packages: `effect` (`Array`, `HashMap`, `Option`, `Order`, `Record`); `@rasm/ts/core` (`BcfViewpoint.GlobalId`).
- Boundary: which diff and which audit a session holds is the app's atom state; this member is a pure fold over two decoded values.
- Growth: a new joined facet — a clash set, a cost delta — is one field on the row and one arm in the same fold; the row never splits into a second family.

```typescript
import { BcfViewpoint } from "@rasm/ts/core"
import { Array, HashMap, Option, Order, Record } from "effect"

type GlobalId = typeof BcfViewpoint.GlobalId.Type

declare namespace Review {
  type Requirement = { readonly requirement: string; readonly verdict: Review.Verdict }
  type Row = {
    readonly anchor: GlobalId
    readonly change: Option.Option<Review.Change>
    readonly attributes: ReadonlyArray<string>
    readonly requirements: ReadonlyArray<Review.Requirement>
  }
  type Census = {
    readonly changes: Record.ReadonlyRecord<Review.Change, number>
    readonly verdicts: Record.ReadonlyRecord<Review.Verdict, number>
    readonly blocking: number
  }
}

// the seed carries no anchor: the brand belongs to the key the fold is already holding, and forging one here
// would mint an unadmitted GlobalId every downstream set compares against
const _SEED: Omit<Review.Row, "anchor"> = { change: Option.none(), attributes: [], requirements: [] }

const _order: Order.Order<Review.Row> = Order.combine(
  Order.mapInput(Order.number, (row: Review.Row) => Option.match(row.change, { onNone: () => 9, onSome: (kind) => _changeRows[kind].rank })),
  Order.mapInput(Order.string, (row: Review.Row) => row.anchor),
)

const _touch = (
  held: HashMap.HashMap<GlobalId, Review.Row>,
  anchor: GlobalId,
  edit: (row: Review.Row) => Review.Row,
): HashMap.HashMap<GlobalId, Review.Row> =>
  HashMap.modifyAt(held, anchor, (slot) =>
    Option.some(edit(Option.getOrElse(slot, () => ({ ..._SEED, anchor }))))) // one keyed read-merge-write: insert and update are two arms of the same Option fold

const _rows = (diff: BimDiff, audit: IdsAudit): ReadonlyArray<Review.Row> =>
  Array.sort(
    HashMap.values(
      Array.reduce(
        audit.verdicts,
        Array.reduce(
          [
            ...Array.map(diff.added, (anchor) => [anchor, "added", [] as ReadonlyArray<string>] as const),
            ...Array.map(diff.removed, (anchor) => [anchor, "removed", [] as ReadonlyArray<string>] as const),
            ...Array.map(diff.modified, (row) => [row.anchor, "modified", row.attributes] as const),
          ],
          HashMap.empty<GlobalId, Review.Row>(),
          (held, [anchor, change, attributes]) => _touch(held, anchor, (row) => ({ ...row, change: Option.some(change), attributes })),
        ),
        (held, verdict) =>
          Array.reduce(verdict.anchors, held, (inner, anchor) =>
            _touch(inner, anchor, (row) => ({
              ...row,
              requirements: [...row.requirements, { requirement: verdict.requirement, verdict: verdict.verdict }],
            }))),
      ),
    ),
    _order,
  )

const _ZERO: Review.Census = {
  changes: Record.map(_changeRows, () => 0),
  verdicts: Record.map(_verdictRows, () => 0),
  blocking: 0,
}

const _census = (rows: ReadonlyArray<Review.Row>): Review.Census =>
  Array.reduce(rows, _ZERO, (census, row) =>
    Array.reduce(row.requirements, {
      // one seeded pass answers every count the header reads: four filter-and-length walks over one row set are the scatter this deletes
      ...census,
      changes: Option.match(row.change, {
        onNone: () => census.changes,
        onSome: (kind) => ({ ...census.changes, [kind]: census.changes[kind] + 1 }),
      }),
    }, (inner, held) => ({
      ...inner,
      verdicts: { ...inner.verdicts, [held.verdict]: inner.verdicts[held.verdict] + 1 },
      blocking: inner.blocking + (_verdictRows[held.verdict].blocking ? 1 : 0), // the verdict row decides severity once
    })))
```

## [04]-[BOARD_COLUMNS]

[BOARD_COLUMNS]:
- Owner: `Review.columns` — the static column set the `Grid` fold consumes: `createColumnHelper<Review.Row>()` accessor rows typed against the joined row, each carrying its `ColumnMeta` cell projection and the `globalId` accessor the grid's selection slice keys on, so the board's row selection and the viewer's selection set stay ONE atom projected two ways rather than two stores reconciled.
- Law: the board is a `Grid`, never a second table — `view/table` owns the state fold, the row models, the grid semantics, and the windowing; this page contributes columns and nothing else, so faceting, grouping, and virtual scale arrive for free and a bespoke row renderer beside them is the named defect.
- Law: the change and verdict cells render their vocabulary row, never a conditional — the cell reads `Review.change[kind]` for its glyph and tone and `Review.verdict[held]` for the requirement chips, so a new kind renders the moment its row lands.
- Law: sorting and filtering reference the registry by name — the change column sorts through the vocabulary's own `rank` lifted into a named `sortingFns` row, and the verdict column filters through `getFacetedUniqueValues` over the closed verdict set, so the filter affordance's options are the vocabulary rather than a scan of the data.
- Packages: `@tanstack/react-table` (`createColumnHelper`, `ColumnDef`, the `ColumnMeta` declaration merge); `view/table` (`Grid` — the state fold and every row model).
- Boundary: the `TableState` atom, persistence, windowing, and the aria grid roles are `view/table`'s; this page is a column contribution.
- Growth: a new board column is one `columnHelper` row; a new cell presentation is one arm on the closed projection `view/table` already declares.

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
  _helper.accessor((row) => Array.map(row.requirements, (held) => held.verdict), {
    id: "verdicts",
    header: "verdicts",
    meta: { cell: "text", dimension: Option.none(), nullable: false },
  }),
]
```

## [05]-[ECHO_ROWS]

[ECHO_ROWS]:
- Owner: `Review.echo(rows, kinds)` — the selection projection: the board's kind filter folds to the anchors those kinds name and mints ONE `Selection.Op`, so isolating every failing element is a single op through the one selection fold and never a second set held beside it. `Review.reveal(rows, extent, padding)` is the camera projection over the resident anchors.
- Law: the reveal takes its extent resolver as a PARAMETER, because ids are not geometry — `Camera.Intent.FitBounds` carries the canonical `GeoFeature.Extent` quadruple, and only the owning surface can turn a `GlobalId` set into one, so the resolver arrives exactly as `viewer/mark#ANCHOR_PINS` takes its projection and this page stays surface-agnostic. A resolver answering nothing for an unresident set folds the whole reveal to `Option.none` — no camera moves, which is the correct answer for a review whose changes have no live geometry, and a fabricated extent standing in for the absent one is the named defect.
- Law: echoes are projections, never stores — the tint the scene draws reads the change row's tone through the token authority and the selection membership through `Selection`'s own set, so this page holds no highlight state; `viewer/scene#DRAW_COLLAPSE`'s tint rows and `viewer/mark#ECHO_ROWS`' echo law own the drawing, and this page owns only which anchors and which tone.
- Law: a non-resident change never reaches a tint or a fit — a removed element has no live geometry, so the `resident` column filters it out of both projections and the board still shows it; a reveal that frames a deleted element frames nothing, and a tint bound to absent geometry is a write no draw call consumes.
- Law: the reveal is an intent, never a camera write — `Camera.Intent.FitBounds` over the resident anchors dispatches through `Camera.drive` exactly as `viewer/mark#VIEWPOINT_RESTORE` does, so every surface class handles it and this page reaches into no map or renderer instance.
- Law: the review authors nothing — a verdict is the audit's, a change is the diff's, and an operator's response to either is a BCF topic the `viewer/mark` board already owns; a comment, a status, or an approval minted here forks the one authored-evidence plane.
- Packages: `viewer/mark` (`Selection.Op`, `Selection.apply`); `viewer/geo` (`Camera.Intent`, `Camera.drive`); `@rasm/ts/core` (`BcfViewpoint.GlobalId`, `GeoFeature.Extent`).
- Boundary: which surface is mounted, and whether the tint reaches a deck layer or a batched scene arm, is the owning viewer plane's; this page answers anchors and tones.
- Growth: a new echo is one projection member reading the same rows — never a second selection set or a second camera path.

```typescript
import type { GeoFeature } from "@rasm/ts/core"
import { Camera } from "./geo.ts"

const _resident = (rows: ReadonlyArray<Review.Row>, kinds: ReadonlyArray<Review.Change>): ReadonlyArray<GlobalId> =>
  Array.filterMap(rows, (row) =>
    Option.filterMap(row.change, (kind) => (Array.contains(kinds, kind) ? Option.some(row.anchor) : Option.none())))

const _echo = (rows: ReadonlyArray<Review.Row>, kinds: ReadonlyArray<Review.Change>): Selection.Op =>
  Selection.Op.Replace({ ids: _resident(rows, kinds) }) // one op through the one fold: the board never holds a second set

const _tint = (rows: ReadonlyArray<Review.Row>): ReadonlyArray<readonly [GlobalId, Theme.Tone]> =>
  Array.filterMap(rows, (row) =>
    Option.filterMap(row.change, (kind) =>
      _changeRows[kind].resident ? Option.some([row.anchor, _changeRows[kind].tone] as const) : Option.none()))

const _reveal = (
  rows: ReadonlyArray<Review.Row>,
  extent: (ids: ReadonlyArray<GlobalId>) => Option.Option<GeoFeature.Extent>, // ids are not geometry: only the owning surface resolves one
  padding: number,
): Option.Option<Camera.Intent> =>
  Option.map(
    extent(Array.map(_tint(rows), ([anchor]) => anchor)), // a non-resident change frames nothing, so the tint set IS the fit set
    (bounds) => Camera.Intent.FitBounds({ bounds, padding }),
  )

declare namespace Review {
  type Shape = {
    readonly change: typeof _changeRows
    readonly changes: Review.Changes
    readonly verdict: typeof _verdictRows
    readonly verdicts: Review.Verdicts
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
  verdict: _verdictRows,
  verdicts: _verdicts,
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

[AUDIT_PROVENANCE]-[OPEN]: the decoded `IdsAudit` carries `specification` and verdicts but no model digest, so a board handed an audit computed against a model other than the diff's `next` cannot refuse the pair; read whether the emitting C# `Rasm.Bim/Exchange` owner carries a model coordinate the wire drops, and widen the landing before this page grows an admission it cannot prove.
