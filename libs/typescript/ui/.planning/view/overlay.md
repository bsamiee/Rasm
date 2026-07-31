# [UI_OVERLAY]

Overlay owns anchored floats, drag-dismissable sheets, command palettes, and presence cursors. Floating-ui positions anchors, vaul owns sheets, cmdk owns palette filtering, and RAC owns standard aria overlays. React-aria props and floating refs merge only where semantics meet geometry; each surface keeps one positioner and one focus owner. Module: `ui/src/view/overlay.ts`.

## [01]-[INDEX]

- [02]-[ANCHOR_HOST]: the floating-anchor hook stack, middleware pipeline, dismiss policy, arrow, delay group; `Overlay`.
- [03]-[SHEET_HOST]: the vaul sheet rows — detent policy, drag discipline, nesting; `Overlay`.
- [04]-[PALETTE]: the `Overlay.Command` vocabulary, the cmdk hosting-shell law, the copy rail; `Overlay`.
- [05]-[PRESENCE_COHORT]: live presence cursors — client-point and world-projected anchoring; —.

## [02]-[ANCHOR_HOST]

[ANCHOR_HOST]:
- Owner: the anchor-host law riding `Overlay` — `Overlay.middleware(options)` mints the canonical `offset → flip → shift → arrow → size` pipeline (the `arrow` row binds `FloatingArrow`'s geometry when the options carry an arrow ref); the consuming row composes `useFloating` with `whileElementsMounted: autoUpdate`; interactions merge through `useInteractions([useClick, useDismiss, useRole])` into the three prop-getters; `FloatingFocusManager` (modal for dialogs, non-modal + `preserveTabOrder` for menus) and `FloatingPortal` complete the stack, with `FloatingOverlay lockScroll` as the scroll-lock backdrop behind a modal anchored dialog; open state binds an atom through `useFloatingRootContext({ open, onOpenChange })` so visibility is store state; enter/exit rides `useTransitionStyles` phases consuming `Theme.Scale.ease` values.
- Packages: `@floating-ui/react` (`useFloating`, `useFloatingRootContext`, `autoUpdate`, `offset`, `flip`, `shift`, `arrow`, `size`, `useInteractions`, `useClick`, `useDismiss`, `useRole`, `useHover` + `safePolygon`, `useListNavigation`, `useTypeahead`, `useClientPoint`, `useInnerOffset`, `inner`, `useTransitionStyles`, `useMergeRefs`, `FloatingArrow`, `FloatingFocusManager`, `FloatingPortal`, `FloatingOverlay`, `FloatingDelayGroup`, `useDelayGroup`, `FloatingTree`, `FloatingNode`, `useFloatingNodeId`, `useFloatingParentNodeId`, `useFloatingPortalNode`, the `OpenChangeReason` union); `system/token` (`Theme.Scale.ease`, `Theme.Scale.z`).
- Law: dismissal branches on CAUSE, never a boolean — `onOpenChange(open, event, reason)` folds the `OpenChangeReason` union through the `Overlay.dismiss` policy table (`escape-key` restores focus, `outside-press` commits a palette draft, `ancestor-scroll` closes silently, `safe-polygon` is hover-intent traversal and never dismisses); a reason the table does not map is a compile break, and an untyped `onOpenChange(false)` swallowing the cause is the named defect.
- Law: presentation facts publish once — every open and reason-keyed dismissal mints one `Overlay.Present` fact through `Overlay.present`, and this page contributes the `rasm.ui.overlay.present` point and runtime row (`system/hook`, observe modality), so history, probe evidence, and the app bridge never wrap an overlay owner.
- Law: hover-opened anchors take `useHover` with `safePolygon` — the one sanctioned pairing of hover-intent and floating geometry (`system/act` owns the hover hook law); a toolbar's sibling tooltips share one `FloatingDelayGroup` so the delay pays once per traversal (`useDelayGroup` in each tooltip), and list-shaped floats compose `useListNavigation`/`useTypeahead` on the SAME `useInteractions` array, never as a second interaction fold.
- Law: nested floats needing dismissal coordination take `FloatingTree`/`FloatingNode` with `useFloatingNodeId`/`useFloatingParentNodeId` wiring the ids and `useFloatingPortalNode` resolving the container; flat overlays never mount the tree.
- Law: the `size.apply` style write is floating-ui's platform-forced statement seam — the middleware hands a live element and the write is the documented application; the kernel carries the exemption.
- Growth: a new anchored surface is one hook composition over the same pipeline; a new middleware concern is one pipeline row; a new dismiss behavior is one reason row — never a second positioner.

```typescript
import { arrow, autoUpdate, flip, offset, shift, size } from "@floating-ui/react"
import type { OpenChangeReason, Placement, VirtualElement } from "@floating-ui/react"
import { Effect, Option } from "effect"
import type { RefObject } from "react"
import { Hook } from "../system/hook.ts"

declare namespace Overlay {
  type Anchor = {
    readonly placement: Placement
    readonly gap: number
    readonly floor: number
    readonly arrow?: RefObject<SVGSVGElement | null>
  }
  type Detents = {
    readonly points: ReadonlyArray<number | string>
    readonly fadeFrom: number
    readonly sequential: boolean // velocity-skip disabled where each detent is a semantic stop rather than a waypoint
  }
  type Dismissal = "close" | "close-silent" | "commit-close" | "ignore"
  type Present =
    | { readonly _tag: "Opened"; readonly overlay: string }
    | { readonly _tag: "Dismissed"; readonly overlay: string; readonly reason: OpenChangeReason }
}

declare module "../system/hook.ts" {
  interface Points {
    readonly "rasm.ui.overlay.present": { readonly modality: "observe"; readonly payload: Overlay.Present }
  }
}

const _presentHook: Hook.Row<"rasm.ui.overlay.present"> = { modality: "observe", depth: 16, source: Option.none() }

const _present = (registry: Hook.Registry, fact: Overlay.Present): Effect.Effect<void> =>
  Effect.asVoid(Hook.publish(registry, "rasm.ui.overlay.present", fact))

const _dismiss = {
  click: "close",
  hover: "close-silent",
  focus: "ignore",
  "focus-out": "close-silent",
  "escape-key": "close",
  "outside-press": "commit-close",
  "reference-press": "close",
  "ancestor-scroll": "close-silent",
  "list-navigation": "ignore",
  "safe-polygon": "ignore",
} as const satisfies Record<OpenChangeReason, Overlay.Dismissal>

const _middleware = (options: Overlay.Anchor) => [
  offset(options.gap),
  flip(),
  shift({ padding: options.gap }),
  ...(options.arrow === undefined ? [] : [arrow({ element: options.arrow })]),
  size({
    apply: ({ availableHeight, elements }) => {
      elements.floating.style.maxHeight = `${Math.max(options.floor, availableHeight)}px`
    },
  }),
]
```

## [03]-[SHEET_HOST]

[SHEET_HOST]:
- Owner: `Overlay.sheet(detents, held, drive, nesting)` — the vaul host as prop records: the detent policy row (`Overlay.Detents`) and the atom-held `Overlay.Detent` fold into `ComponentProps` of `Drawer.Root`, `Drawer.Content`, and `Drawer.Handle`, so `open`/`onOpenChange` and `activeSnapPoint`/`setActiveSnapPoint` are store state at one declaration and a vaul prop rename breaks HERE rather than at every sheet. `Drawer.Title` + `Drawer.Description` are always mounted (visually hidden when no heading shows — Radix warns and the sheet goes unlabeled otherwise), and `handleOnly` pins the drag origin to `Drawer.Handle`.
- Law: nesting is a vocabulary row, never a boolean — `Overlay.roots` maps the closed `flat`/`nested` axis onto `Drawer.Root`/`Drawer.NestedRoot` and the sheet record hands back the resolved component, so a nested flow is one key and the cumulative-scale behavior arrives with it; the flat arm alone takes `modal`, because a nested sheet inherits its parent's modality.
- Law: prop records lift, never restate — every returned record is `ComponentProps` of the part it feeds, the same law `Chart.Bespoke` follows, and a hand-typed sheet-props interface beside this fold is the parallel shape it deletes.
- Packages: `vaul` (`Drawer.Root`, `Drawer.NestedRoot`, `Drawer.Trigger`, `Drawer.Portal`, `Drawer.Overlay`, `Drawer.Content`, `Drawer.Handle`, `Drawer.Title`, `Drawer.Description`, `Drawer.Close`; the `DialogProps` control surface — `snapPoints`/`fadeFromIndex`/`activeSnapPoint`/`setActiveSnapPoint`/`snapToSequentialPoint`/`handleOnly`/`repositionInputs`/`direction`/`dismissible`/`closeThreshold`/`modal` — and `HandleProps.preventCycle`).
- Law: the sheet's drag is vaul's own — no `use-gesture` binding on a sheet surface (`system/act`'s double-bind defect); sheet motion is the drag physics with `Motion.sheet` for programmatic open/close, never both animating one property.
- Law: `repositionInputs` stays default where a sheet hosts fields — the keyboard-avoidance behavior is the package's; a hand-rolled viewport listener beside it is the named defect.
- Law: `dismissible: false` demands the controlled `open` this fold already supplies — an uncontrolled undismissible sheet traps itself open, so the two facts travel as one record.
- Growth: a new sheet detent is one `snapPoints` entry; a new sheet surface is one `Overlay.sheet` call — the detent policy row never forks.

```typescript
import type { ComponentProps } from "react"
import { Drawer } from "vaul"

const _roots = { flat: Drawer.Root, nested: Drawer.NestedRoot } as const

declare namespace Overlay {
  type Nesting = keyof typeof _roots
  type Detent = { readonly open: boolean; readonly at: number | string | null }
  type Sheet = {
    readonly Root: (typeof _roots)[Overlay.Nesting]
    readonly root: ComponentProps<typeof Drawer.Root>
    readonly content: ComponentProps<typeof Drawer.Content>
    readonly handle: ComponentProps<typeof Drawer.Handle>
  }
}

const _sheet = (
  detents: Overlay.Detents,
  held: Overlay.Detent,
  drive: (next: Overlay.Detent) => void,
  nesting: Overlay.Nesting,
): Overlay.Sheet => ({
  Root: _roots[nesting],
  root: {
    open: held.open,
    onOpenChange: (open) => drive({ ...held, open }),
    snapPoints: [...detents.points], // vaul's control surface takes a mutable array: this fold is the one seam the readonly policy row copies at
    fadeFromIndex: detents.fadeFrom,
    activeSnapPoint: held.at,
    setActiveSnapPoint: (at) => drive({ ...held, at }),
    snapToSequentialPoint: detents.sequential,
    handleOnly: true,
    ...(nesting === "flat" && { modal: true }), // a nested sheet inherits its parent's modality; only the flat arm states one
  },
  content: { "aria-describedby": undefined }, // the Description part carries the description; the auto-wired id would name a node this fold never mounts
  handle: { preventCycle: true }, // the handle drags; detent cycling stays the atom's write
})
```

## [04]-[PALETTE]

[PALETTE]:
- Owner: the palette law riding `Overlay` — the `Overlay.Command` vocabulary: one `as const satisfies Record<string, Overlay.Command>` table where each row carries `icon` (a named `LucideIcon` — the row's identity, tree-shaken), `label` (a `system/intl` catalog key), `keywords` (alias tokens for the scorer), and `run` (the intent write — an atom setter or callable atom the app wires); the palette renders the table through cmdk — `Command.Input`/`Command.List`/`Command.Group`/`Command.Item`/`Command.Empty` — with controlled `value`/`onValueChange`, `useCommandState((s) => s.filtered.count)` driving the count/empty rows without list re-render.
- Packages: `cmdk` (`Command` compound, `CommandDialog`, `Command.Loading`, `useCommandState`, `defaultFilter`); `lucide-react` (`LucideIcon` — icon-as-identity); `system/intl` (labels, `useFilter` pre-normalization where locale-sensitive); `system/primitive` (`Clipboard` — the copy rail); `@tanstack/react-virtual` + `@floating-ui/react` `inner`/`useInnerOffset` (the virtualized lane).
- Law: hosting picks exactly one shell, and `Overlay.hosts` is the row table that makes the choice checkable — each row names which package owns the portal and which owns the focus trap, so `modal` takes `CommandDialog` (cmdk's own Radix portal and trap), `sheet` takes a BARE `Command` inside `Drawer.Content` (vaul owns both), and `anchored` takes a bare `Command` inside `FloatingFocusManager` (floating-ui owns position, portal, and trap). Two focus traps on one surface is the named defect the `trap` column exists to expose, `CommandDialog` inside either non-modal host is that defect spelled twice, and `useListNavigation`/`useTypeahead` never stack over a cmdk list.
- Law: a result set past the DOM budget virtualizes — the anchored combobox lane windows pre-filtered rows through `useVirtualizer` with `useListNavigation({ virtual: true })` keeping focus on the input (`scrollToIndex` reveals the active row), and `inner`/`useInnerOffset` anchor the tall scrollable list at the active item; cmdk's own list serves below the budget — one lane per palette, chosen by row count.
- Law: item `value` is the stable spec key, never the visible label — filtering and selection survive label localization; `keywords` carry the localized aliases.
- Law: async command sources set `shouldFilter={false}` and render pre-filtered rows from an atom (`Atom.debounce`d query, `Result`-folded, `Command.Loading` on the `waiting` arm) — the machine keeps keyboard/selection, the store owns matching.
- Law: copy-shaped commands ride the port — a `run` that yields text (a share link, an id, an evidence row) composes the `Clipboard` Tag from `system/primitive#CLIPBOARD_PORT` and lands its refusal as a toast note; `navigator.clipboard` in a spec row is the named defect.
- Law: palette motion is `Motion.palette` through the entering/exiting variants — or, where the palette morphs into its result surface, the `Motion` physical plane's `layoutId` morph; one owner per surface, never both.
- Growth: a new command is one spec row; a new palette surface is one hosting-shell choice — the table never forks.

```typescript
import { Command, CommandDialog, defaultFilter, useCommandState } from "cmdk"
import { Array, Record } from "effect"
import type { LucideIcon } from "lucide-react"
import type { ComponentProps } from "react"

const _hosts = {
  modal: { shell: CommandDialog, portal: "cmdk", trap: "cmdk" },
  sheet: { shell: Command, portal: "vaul", trap: "vaul" },
  anchored: { shell: Command, portal: "floating-ui", trap: "floating-ui" },
} as const

declare namespace Overlay {
  type Command = {
    readonly icon: LucideIcon
    readonly label: string // a system/intl catalog key: the scorer reads its RESOLVED text through keywords, never this key
    readonly keywords: ReadonlyArray<string>
    readonly run: () => void
  }
  type Host = keyof typeof _hosts
  type HostRow = { readonly shell: typeof Command | typeof CommandDialog; readonly portal: string; readonly trap: string }
  type Palette = {
    readonly root: ComponentProps<typeof Command>
    readonly items: ReadonlyArray<ComponentProps<typeof Command.Item>>
  }
  type _Rows<T extends Record.ReadonlyRecord<Overlay.Host, Overlay.HostRow> = typeof _hosts> = T // row guard: a host missing its ownership columns fails at the declaration
}

const _palette = (
  table: Record.ReadonlyRecord<string, Overlay.Command>,
  resolve: (label: string) => string,
  remote: boolean,
): Overlay.Palette => ({
  // an async source hands the store the matching and renders pre-filtered rows; the local table keeps cmdk's scorer
  root: { shouldFilter: !remote, loop: true, filter: defaultFilter },
  items: Array.map(Record.toEntries(table), ([key, row]) => ({
    value: key, // the stable spec key: filtering and selection survive label localization
    keywords: [resolve(row.label), ...row.keywords], // the localized text enters the scorer HERE, never as the value
    onSelect: row.run,
  })),
})

const _matched = (): number => useCommandState((state) => state.filtered.count) // the count and empty rows read the store, so the list never re-renders to report its own size
```

## [05]-[PRESENCE_COHORT]

[PRESENCE_COHORT]:
- Owner: the presence-cohort law riding `Overlay` — the collaborative-cursor cohort: the live roster (`Presence.roster` entering as an atom through `system/atom#LIVE_BRIDGE`) renders one cursor row per actor; the LOCAL pointer's own affordance (a cursor-attached label, a drag ghost) anchors through `useClientPoint(context)` — the shipped cursor-follow anchor, never a hand-built rect wrapper — while REMOTE actors anchor by `Overlay.virtual` wrapping each actor's world coordinate projected through the owning surface's projection (the viewer projection seam for map surfaces, plain viewport coordinates elsewhere); cursors mount in one `FloatingPortal` at the `Theme.Scale.z` cursor rank, motion rides `Motion.panel`, and idle actors age out by the roster's own lease verdicts — never a local timer per cursor.
- Packages: `@floating-ui/react` (`useClientPoint`, `VirtualElement`, `FloatingPortal`); `@rasm/ts/core` (`Presence` — roster shape and lease law arrive settled).
- Law: presence is render-only — cursors never intercept pointer events (`pointer-events: none` in the recipe) and carry no focus semantics; the cohort is a projection of state, not an interaction surface.
- Law: per-actor identity is keyed by `Presence.Actor`; label/color derive from the actor's face metadata through the theme ramp — no per-actor style state in the component.
- Boundary: the roster fold, lease policy, and status verdicts live in the core presence plane; the projection function arrives as a parameter so the cohort is surface-agnostic.

```typescript
const _virtual = (rect: () => DOMRect): VirtualElement => ({ getBoundingClientRect: rect })

declare namespace Overlay {
  type Shape = {
    readonly dismiss: typeof _dismiss
    readonly hook: typeof _presentHook
    readonly hosts: typeof _hosts
    readonly matched: typeof _matched
    readonly middleware: typeof _middleware
    readonly palette: typeof _palette
    readonly present: typeof _present
    readonly roots: typeof _roots
    readonly sheet: typeof _sheet
    readonly virtual: typeof _virtual
  }
}

const Overlay: Overlay.Shape = {
  dismiss: _dismiss,
  hook: _presentHook,
  hosts: _hosts,
  matched: _matched,
  middleware: _middleware,
  palette: _palette,
  present: _present,
  roots: _roots,
  sheet: _sheet,
  virtual: _virtual,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Overlay }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
