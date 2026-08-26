# [UI_STATUS]

Status owns the feedback family between the toast queue and the field error: posture from the one atom `Result` fold, the two placement tiers, the empty and result faces, the skeleton plane veiling real children, and the progress gauges. Posture is a closed vocabulary computed from `Result` arms, so no `{ loading, error, data }` flag record re-derives `Exit` by hand or admits the impossible states the fold excludes. `_WINDOWS` leases every waiting face a declared entry delay and death, so no skeleton flashes for a fast answer and none outlives its data. Module: `ui/src/view/status.ts`.

Composition facts arrive settled: every posture, tier, and gauge colors through a `_tone` KEY onto `system/token#TONE_VOCABULARY`'s roster, so this page carries zero hue and restyling the whole family is one token edit; announcement composes `system/primitive#TOAST_ANNOUNCE`'s one region, the only announcer a face reaches; the field tier stays `view/form`'s `FieldError` boundary and the toast stays primitive's queue, so this page owns the faces BETWEEN those two. Refusal evidence arrives already typed on the `Result` it folds, so no fault family exists here.

## [01]-[INDEX]

- [02]-[POSTURE_FOLD]: `Status` closes the posture vocabulary, derives it from one `Result` fold, and seats the placement tiers and tone/fill axes; `Status`.
- [03]-[WINDOWS]: `_WINDOWS` leases every transient kind an entry delay and a dismissal window, with the horizon evidence rows; `Status`.
- [04]-[SKELETON_PLANE]: `Status` veils real children through a context wrapper — inert veil, phase-synced shimmer, leased delay; `Status`.
- [05]-[EMPTY_RESULT]: `Status` seats the empty-state slot anatomy, its drop-target reuse, and the terminal faces with their repair affordance; `Status`.
- [06]-[PROGRESS_PLANE]: gauge decomposition over RAC render props, the delayed self-synced spinner, and the status dot; `Status`.

## [02]-[POSTURE_FOLD]

[POSTURE_FOLD]:
- Owner: `Status` — the posture vocabulary and its one derivation: `_postures` is the closed six-arm roster (`empty`/`loading`/`refreshing`/`resolved`/`refused`/`torn`), `Status.of(result, extent?)` folds any atom `Result` into exactly one posture, `_TIERS` seats the two page-scale placements as aria-posture DATA rows, `_ANATOMY` is the slot roster every tiered face renders (indicator/content/title/description/actions), and the recipe axes are fill × tone × size — all orthogonal, tone derived from the token roster.
- Packages: `@effect-atom/atom-react` (`Result` — `matchWithWaiting`, `isWaiting`, `isSuccess`); `effect` (`Array`, `Option`, `Record`); `react-aria-components` (the tier hosts are plain landmarks and live regions — no widget); `class-variance-authority` through `system/primitive#STYLED_SPINE`; `system/token` (`Theme`, `cn`).
- Entry: `Status.of` is the one posture read — a surface computes its posture from its own atom's `Result` and renders the row that posture selects; a hand-kept `isLoading` boolean beside an atom is the flag pair `system/atom#WRITE_AND_FOLD` already forecloses, restated here because this page is where the temptation lands.
- Law: posture derives, never mirrors — the fold sieves `waiting` first (`isWaiting` + `isSuccess` splits first-load from stale-while-revalidate), then folds the settled arms, so `loading` and `refreshing` are structurally distinct faces (a blank region versus last-good data under a refresh hold) and no surface can show a spinner over data it already holds.
- Law: `empty` is a cardinality fact on success, never a failure — the optional `extent` reader admits the axis (a roster length, a hit count), and a success reading zero selects the empty face; a caller without a countable payload never constructs the posture, so the arm is unreachable rather than defaulted.
- Law: each tier is an aria posture as data — `banner` renders a labelled `<section tabIndex={-1}>` landmark with NO role and NO live semantics (arrival is announced by the caller's policy through `Primitive.announce`, and the tabIndex admits programmatic focus after a route-level event); `alert` renders `role="alert"` (implicitly assertive) with the converged indicator/content anatomy. Minting a banner with `aria-live`, or a second live region beside the alert, double-speaks against the one announcer.
- Law: the field tier is `view/form`'s — a validation refusal renders through `FieldError` rows at the field it names, so this page's tiers never carry a field path; a form-level refusal that is not field-addressable lands on the `alert` tier through the same error shape `Form.errors` folds.
- Law: tone is a KEY and posture owns the mapping — `_tone` is total over the posture roster (`satisfies` breaks on a new posture), two postures may share a tone because the row carries the MEANING and the hue is its derivation, and the fill axis (`soft`/`surface`/`outline`) names only generated slot utilities; a hex literal, a local tone union, or a per-fill palette forks the token authority.
- Law: recipe axes stay orthogonal — fill, tone, and size each vary alone and compose by `cn`; a compound that welds two axes into one variant name (`dangerOutline`) re-mints the cross product the axes already generate.
- Boundary: WHICH flows banner versus toast is app policy (blocking evidence banners; non-blocking outcomes ride `Primitive.notify`); the failure envelope for thrown/defect rendering stays `system/primitive#FAILURE_ENVELOPE`'s — `torn` here colors a face, it never replaces the boundary.
- Growth: a new posture is one roster arm with its `_tone` and `_WINDOWS` rows — the `satisfies` contracts break every table that must answer it; a new tier is one `_TIERS` row; a new fill is one `_FILL` row — never a sibling component.

```typescript
import { Result } from "@effect-atom/atom-react"
import { cva } from "class-variance-authority"
import { Duration, Option, Record, type Types } from "effect"
import { Theme } from "../system/token.ts"

const _postures = ["empty", "loading", "refreshing", "resolved", "refused", "torn"] as const

declare namespace Status {
  type Posture = (typeof _postures)[number]
  type Tier = keyof typeof _TIERS
  type Fill = (typeof _fills)[number]
  type Slot = (typeof _slots)[number]
}

const _of = <A, E>(result: Result.Result<A, E>, extent?: (value: A) => number): Status.Posture =>
  Result.isWaiting(result)
    ? Result.isSuccess(result) ? "refreshing" : "loading"
    : Result.matchWithWaiting(result, {
      onWaiting: () => "loading" as const,
      onSuccess: ({ value }) => extent !== undefined && extent(value) === 0 ? "empty" as const : "resolved" as const,
      onError: () => "refused" as const,
      onDefect: () => "torn" as const,
    })

const _tone = {
  empty: { tone: "neutral" },
  loading: { tone: "neutral" },
  refreshing: { tone: "accent" },
  resolved: { tone: "success" },
  refused: { tone: "danger" },
  torn: { tone: "danger" },
} as const satisfies Record<Status.Posture, { readonly tone: Theme.Tone }>

const _TIERS = {
  banner: { host: "section", role: Option.none<"alert">(), labelled: true, focus: Option.some(-1) },
  alert: { host: "div", role: Option.some("alert" as const), labelled: false, focus: Option.none<number>() },
} as const

const _slots = ["indicator", "content", "title", "description", "actions"] as const

const _fills = ["soft", "surface", "outline"] as const

const _FILL = {
  soft: (tone: Theme.Tone) => `bg-${tone}-surface text-${tone}-text`,
  surface: (tone: Theme.Tone) => `bg-${tone}-surface text-${tone}-text border border-${tone}-border`,
  outline: (tone: Theme.Tone) => `border border-${tone}-border text-${tone}-text`,
} as const satisfies Record<Status.Fill, (tone: Theme.Tone) => string>

const _toned = (fill: Status.Fill) => Record.map(Theme.Palette.rows, (_row, tone) => _FILL[fill](tone))

const _face = cva("flex items-start gap-3 rounded-md p-4", {
  variants: {
    tone: _toned("surface"),
    fill: { soft: "border-0", surface: "", outline: "bg-transparent" },
    size: { sm: "p-2 text-sm gap-2", md: "p-4 text-base gap-3" },
  },
  defaultVariants: { tone: "neutral", fill: "surface", size: "md" },
})
```

## [03]-[WINDOWS]

[WINDOWS]:
- Owner: the `_WINDOWS` lease table riding `Status` — one row per transient kind pairing `delay` (the entry window: the face renders nothing until the wait has lasted this long, so a fast answer never flashes a veil) with `linger` (the dismissal window once shown: `Some` holds the face a minimum beat so a slow-then-fast answer never flickers, `None` retracts on the act alone — the same `Option<Duration>` carrier and None-means-act-retracted reading as primitive's urgency rows, so the folder spells transience exactly once); `Status.useWindow(kind, waiting)` is the one hook realizing a row, and `Status.horizon(kind, held)` projects a live window as evidence.
- Law: no transient posture exists without a declared death — every waiting face this page renders (skeleton, spinner, refresh hold) mounts through a `_WINDOWS` row, so an eternal skeleton is unrepresentable: the face either retracts on data (the act) or the page's own lease evidence names the kind that is lying.
- Law: the delay kills the flash structurally — `useWindow` returns `hidden` until `delay` elapses and the caller renders nothing on `hidden`, so the sub-delay resolution path never mounts the veil at all; a CSS-only delay keeps the node in the tree where it still announces to assistive readers.
- Law: horizons publish as evidence, never a new channel — `Status.horizon` answers the `rasm.ui.vital.row` payload shape (`label`/`unit`/`value`), so a surface holding a window past its lease surfaces on the SAME replay point every evidence board already reads, and a ghost skeleton is a sortable row before a user reports it; this page mints no hook point and no instrument.
- Law: the rows are policy this owner declares, not knobs a call site tunes — a caller passes the KIND and never a millisecond literal, so retuning the folder's transience is a table edit here; a per-site `delay` number is the same defect as a per-site toast timeout.
- Boundary: the timers are the platform's — the hook body is the page's one timer boundary; the toast queue's dismissal windows stay primitive's rows (this table leases the WAITING faces, that one leases NOTES).
- Growth: a new transient kind is one row; a new lease axis (a repeat cap, a backoff) is one column every row must answer.

```typescript
import { useEffect, useState } from "react"
import type { Points } from "../system/hook.ts"

declare namespace Status {
  type Window = { readonly delay: Duration.Duration; readonly linger: Option.Option<Duration.Duration> }
  type Kind = keyof typeof _WINDOWS
  type Phase = "hidden" | "shown"
}

const _WINDOWS = {
  skeleton: { delay: Duration.millis(300), linger: Option.some(Duration.millis(400)) },
  spinner: { delay: Duration.seconds(1), linger: Option.some(Duration.millis(400)) },
  refresh: { delay: Duration.millis(150), linger: Option.none<Duration.Duration>() },
} as const satisfies Record<string, Status.Window>

const _useWindow = (kind: Status.Kind, waiting: boolean): Status.Phase => {
  const [shown, setShown] = useState(false)
  const row = _WINDOWS[kind]
  useEffect(() => {
    if (waiting && !shown) {
      const entry = globalThis.setTimeout(() => setShown(true), Duration.toMillis(row.delay))
      return () => globalThis.clearTimeout(entry)
    }
    if (!waiting && shown) {
      return Option.match(row.linger, {
        onNone: () => (setShown(false), undefined),
        onSome: (hold) => {
          const exit = globalThis.setTimeout(() => setShown(false), Duration.toMillis(hold))
          return () => globalThis.clearTimeout(exit)
        },
      })
    }
    return undefined
  }, [waiting, shown, row])
  return shown ? "shown" : "hidden"
}

const _horizon = (kind: Status.Kind, held: Duration.Duration): Points["rasm.ui.vital.row"]["payload"] => ({
  label: `status-${kind}-held`,
  unit: "ms",
  value: Duration.toMillis(held),
})
```

## [04]-[SKELETON_PLANE]

[SKELETON_PLANE]:
- Owner: the skeleton plane riding `Status` — a context wrapper over REAL children, never a painted box roster: `Status.Skeleton` is the boolean context (`Status.useSkeleton()` the read), the wrapper renders its subtree `inert` under the one shimmer hold, and `Status.sync(host)` phase-locks every shimmer in the subtree so a page of veiled regions breathes as one surface instead of strobing out of phase.
- Law: the skeleton is the layout, veiled — wrapping real children keeps every box, line, and gap exactly where the resolved face puts them, so the reveal never reflows; a hand-drawn skeleton box roster drifts from its surface on the first layout edit and is the named defect.
- Law: the veil is `inert` — the subtree drops out of the tab order, the accessibility tree, and hit testing as one attribute, so no per-child `aria-hidden`/`tabIndex` sweep exists; recipes below the context read `useSkeleton()` to swap their fill onto the neutral surface slot, and a recipe that renders live data under the veil marks a missing context read.
- Law: the shimmer is `Motion.holds.pulse` — the one sustained-attention row `system/act#MOTION_ROWS` already leads with `motion-reduce:animate-none`, so reduced motion is a construction fact here for free; a bespoke shimmer keyframe beside the hold is the local-keyframe defect that vocabulary forecloses.
- Law: phase sync is one WAAPI write — `sync` zeroes `startTime` across the subtree's animations so every pulse aligns to one clock; unsynced shimmers read as N loading regions when the surface is ONE loading page.
- Law: entry rides the lease — the wrapper mounts through `useWindow("skeleton", waiting)`, so the veil obeys the same delay and linger law as every transient face; a skeleton mounted bare re-opens the flash the table closed.
- Boundary: WHAT to veil is the surface's composition (a grid veils its rows, a form its fields); the failure face is `[02]`'s postures — a veil never renders over `refused` or `torn`.
- Growth: a skeleton variant (a text-line veil with cloned line boxes) is one recipe row reading the same context — never a second provider.

```typescript
import { createContext, useContext } from "react"

const _Skeleton = createContext(false)

const _useSkeleton = (): boolean => useContext(_Skeleton)

const _sync = (host: HTMLElement): void => {
  for (const animation of host.getAnimations({ subtree: true })) animation.startTime = 0
}
```

## [05]-[EMPTY_RESULT]

[EMPTY_RESULT]:
- Owner: the terminal faces riding `Status` — `_EMPTY` is the empty-state slot anatomy (`media` as an `icon`/`illustration` discriminant, `title`, `description`, `actions`), and the result faces render the `resolved`/`refused`/`torn` postures at page scale with `Status.Action` carrying the repair affordance (`label` an intl catalog key, `run` a total `Effect`), so a terminal face carries the fix instead of only naming the outcome — the same affordance shape primitive's notes carry, declared here because that type is primitive-interior.
- Law: the empty face is the drop face — RAC `DropZone` renders this same anatomy as its idle content and the `isDropTarget` render state restyles it through the recipe's variant, so an importable-when-empty surface is one composition and never a second empty component beside a drop zone.
- Law: `media` is a discriminant, not a slot pair — `icon` takes a `lucide-react` identity at the token size ladder, `illustration` takes the app's own artwork node; the two never co-render, and the discriminant keys the recipe's spacing variant so icon-scale and illustration-scale empties are one recipe.
- Law: actions are bounded — the anatomy seats at most a primary and a secondary action (the shape every surveyed anatomy converges on); a third action marks a face the page tier owns, and the row refuses it structurally by carrying two optional seats rather than an array.
- Law: result faces reuse the tier hosts — a page-scale `refused` renders on the `alert` tier row and a route-scale one on `banner`, so terminal placement is `[02]`'s data and this cluster adds only the anatomy; a bespoke result host forks the aria posture the tiers already decided.
- Boundary: WHAT emptiness means (no rows, no matches, not-yet-created) is the surface's copy through `system/intl`'s catalog; the fault DETAIL rendering (Cause trees, family tags) stays the failure envelope's fold — a result face shows the posture and the affordance, never a stack.
- Growth: a new face axis (a compact list-row empty beside the page empty) is one size variant on the recipe; a new media kind is one discriminant arm.

```typescript
import type { Effect } from "effect"
import type { LucideIcon } from "lucide-react"
import type { ReactNode } from "react"

declare namespace Status {
  type Action = { readonly label: string; readonly run: Effect.Effect<void> }
  type Media =
    | { readonly kind: "icon"; readonly icon: LucideIcon }
    | { readonly kind: "illustration"; readonly art: ReactNode }
  type Empty = {
    readonly media: Option.Option<Status.Media>
    readonly title: string
    readonly description: Option.Option<string>
    readonly primary: Option.Option<Status.Action>
    readonly secondary: Option.Option<Status.Action>
  }
}
```

## [06]-[PROGRESS_PLANE]

[PROGRESS_PLANE]:
- Owner: the gauge plane riding `Status` — the RAC `ProgressBar`/`Meter` render props (`percentage`, `valueText`, `isIndeterminate`) decompose into the `track`/`indicator`/`label`/`value` slot roster with the value slot taking a formatter over the RAC-computed `valueText` (locale-correct by `system/intl`'s ambient provider, so no gauge formats a number itself), the spinner rides the `spinner` lease row with `srText` and the self-sync offset, and `_DOT` is the status-dot recipe keyed by ONE tone column.
- Law: progress and meter stay two semantics on one anatomy — `ProgressBar` states how much of a task has happened, `Meter` states where a level sits in a range; the slot roster and recipe serve both, and the RAC component is the discriminant, so no `kind` knob exists here.
- Law: indeterminate is the render prop, never a posture — `isIndeterminate` selects the indeterminate indicator variant on the same recipe; a separate indeterminate component beside the bar restates the state the render prop already carries.
- Law: the spinner self-synchronizes — a negative `animationDelay` derived from the platform clock modulo the spin period aligns every spinner's rotation phase, mirroring the skeleton's one-clock law; the spin class is `Motion.holds.spin`, so reduced motion resolves structurally and `srText` gives the visual-only element its spoken name.
- Law: the dot carries ONE tone column — per the folder ruling a surface with two closed axes gives tone to one: the dot's tone keys the token roster and its second axis (shape, ring) is a non-color discriminant; categorical series identity is the chart plane's merge scale, and a categorical hue minted here forks the palette derivation that plane already owns.
- Law: gauges are output — every gauge renders an atom-derived reading through the bridge and takes no schema field (`view/form#FIELD_ROSTER`'s law, binding here because this page owns the render side of that split).
- Boundary: WHAT a percentage means (bytes, steps, items) is the caller's `valueText` formatting policy; upload progress specifically arrives through `Form.upload`'s tap parameter as an atom these gauges read.
- Growth: a new gauge posture (a segmented bar, a radial track) is one indicator variant row; a new dot rank is one row on its own closed axis beside the tone column.

```typescript
const _gauges = ["track", "indicator", "label", "value"] as const

declare namespace Status {
  type Gauge = (typeof _gauges)[number]
  type Formatted = (valueText: string, percentage: Option.Option<number>) => ReactNode
}

const _spun = (period: number): { readonly animationDelay: string } => {
  return { animationDelay: `-${globalThis.performance.now() % period}ms` }
}

declare namespace Status {
  type Shape = Types.Simplify<{
    readonly postures: typeof _postures
    readonly of: typeof _of
    readonly tone: typeof _tone
    readonly tiers: typeof _TIERS
    readonly slots: typeof _slots
    readonly fills: typeof _fills
    readonly fill: typeof _FILL
    readonly toned: typeof _toned
    readonly face: typeof _face
    readonly gauges: typeof _gauges
    readonly windows: typeof _WINDOWS
    readonly useWindow: typeof _useWindow
    readonly horizon: typeof _horizon
    readonly Skeleton: typeof _Skeleton
    readonly useSkeleton: typeof _useSkeleton
    readonly sync: typeof _sync
    readonly spun: typeof _spun
  }>
}

const Status: Status.Shape = {
  postures: _postures,
  of: _of,
  tone: _tone,
  tiers: _TIERS,
  slots: _slots,
  fills: _fills,
  fill: _FILL,
  toned: _toned,
  face: _face,
  gauges: _gauges,
  windows: _WINDOWS,
  useWindow: _useWindow,
  horizon: _horizon,
  Skeleton: _Skeleton,
  useSkeleton: _useSkeleton,
  sync: _sync,
  spun: _spun,
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Status }
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
