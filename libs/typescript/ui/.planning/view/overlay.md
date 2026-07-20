# [UI_OVERLAY]

The one overlay owner: anchored positioning rides floating-ui's hook stack with a reason-keyed dismiss policy, the drag-dismissable sheet rides vaul's bundled Radix stack, the global command palette rides cmdk's filtering machine over an `Overlay.Command` vocabulary with a virtualized combobox lane and a `Clipboard`-ported copy command, and the presence-cursor cohort anchors through `useClientPoint` and world-projected `VirtualElement` rows — four overlay classes, exactly one semantic owner and one positioner per element. RAC `Popover`/`Modal` own standard aria overlays outside this module; react-aria's `mergeProps` + floating-ui's `useMergeRefs` fold the two prop/ref systems where aria semantics meet floating geometry; two focus traps on one surface is the named defect. The module is `ui/src/view/overlay.ts`.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                                                  | [PUBLIC]  |
| :-----: | :---------------- | :-------------------------------------------------------------------------------------- | :-------- |
|  [01]   | `ANCHOR_HOST`     | the floating-anchor hook stack, middleware pipeline, dismiss policy, arrow, delay group | `Overlay` |
|  [02]   | `SHEET_HOST`      | the vaul sheet rows — detent policy, drag discipline, nesting                           | `Overlay` |
|  [03]   | `PALETTE`         | the `Overlay.Command` vocabulary, the cmdk hosting-shell law, the copy rail             | `Overlay` |
|  [04]   | `PRESENCE_COHORT` | live presence cursors — client-point and world-projected anchoring                      | —         |

## [02]-[ANCHOR_HOST]

[ANCHOR_HOST]:
- Owner: the anchor-host law riding `Overlay` — `Overlay.middleware(options)` mints the canonical `offset → flip → shift → arrow → size` pipeline (the `arrow` row binds `FloatingArrow`'s geometry when the options carry an arrow ref); the consuming row composes `useFloating` with `whileElementsMounted: autoUpdate`; interactions merge through `useInteractions([useClick, useDismiss, useRole])` into the three prop-getters; `FloatingFocusManager` (modal for dialogs, non-modal + `preserveTabOrder` for menus) and `FloatingPortal` complete the stack, with `FloatingOverlay lockScroll` as the scroll-lock backdrop behind a modal anchored dialog; open state binds an atom through `useFloatingRootContext({ open, onOpenChange })` so visibility is store state; enter/exit rides `useTransitionStyles` phases consuming `Theme.Scale.ease` values.
- Packages: `@floating-ui/react` (`useFloating`, `useFloatingRootContext`, `autoUpdate`, `offset`, `flip`, `shift`, `arrow`, `size`, `useInteractions`, `useClick`, `useDismiss`, `useRole`, `useHover` + `safePolygon`, `useListNavigation`, `useTypeahead`, `useClientPoint`, `useInnerOffset`, `inner`, `useTransitionStyles`, `useMergeRefs`, `FloatingArrow`, `FloatingFocusManager`, `FloatingPortal`, `FloatingOverlay`, `FloatingDelayGroup`, `useDelayGroup`, `FloatingTree`, `FloatingNode`, `useFloatingNodeId`, `useFloatingParentNodeId`, `useFloatingPortalNode`, the `OpenChangeReason` union); `system/token` (`Theme.Scale.ease`, `Theme.Scale.z`).
- Law: dismissal branches on CAUSE, never a boolean — `onOpenChange(open, event, reason)` folds the `OpenChangeReason` union through the `Overlay.dismiss` policy table (`escape-key` restores focus, `outside-press` commits a palette draft, `ancestor-scroll` closes silently, `safe-polygon` is hover-intent traversal and never dismisses); a reason the table does not map is a compile break, and an untyped `onOpenChange(false)` swallowing the cause is the named defect.
- Law: presentation facts publish once — every open and reason-keyed dismissal mints one fact on the `rasm.ui.overlay.present` hook point (`system/hook`, observe modality), so history, probe evidence, and the app bridge tap the presentation stream without wrapping any overlay owner.
- Law: hover-opened anchors take `useHover` with `safePolygon` — the one sanctioned pairing of hover-intent and floating geometry (`system/act` owns the hover hook law); a toolbar's sibling tooltips share one `FloatingDelayGroup` so the delay pays once per traversal (`useDelayGroup` in each tooltip), and list-shaped floats compose `useListNavigation`/`useTypeahead` on the SAME `useInteractions` array, never as a second interaction fold.
- Law: nested floats needing dismissal coordination take `FloatingTree`/`FloatingNode` with `useFloatingNodeId`/`useFloatingParentNodeId` wiring the ids and `useFloatingPortalNode` resolving the container; flat overlays never mount the tree.
- Law: the `size.apply` style write is floating-ui's platform-forced statement seam — the middleware hands a live element and the write is the documented application; the kernel carries the exemption.
- Growth: a new anchored surface is one hook composition over the same pipeline; a new middleware concern is one pipeline row; a new dismiss behavior is one reason row — never a second positioner.

```typescript
import { arrow, autoUpdate, flip, offset, shift, size } from "@floating-ui/react"
import type { OpenChangeReason, Placement, VirtualElement } from "@floating-ui/react"
import type { RefObject } from "react"

declare namespace Overlay {
  type Anchor = {
    readonly placement: Placement
    readonly gap: number
    readonly floor: number
    readonly arrow?: RefObject<SVGSVGElement | null>
  }
  type Detents = { readonly points: ReadonlyArray<number | string>; readonly fadeFrom: number }
  type Dismissal = "close" | "close-silent" | "commit-close" | "ignore"
}

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
- Owner: the sheet law riding `Overlay` — the vaul host: `Drawer.Root` with `open`/`onOpenChange` and `activeSnapPoint`/`setActiveSnapPoint` atom-bound, `snapPoints` + `fadeFromIndex` as the detent policy row (`Overlay.Detents`), `Drawer.Title` + `Drawer.Description` always present (visually hidden when no heading shows), `handleOnly` for drag-origin discipline with `Drawer.Handle` as the affordance; a nested flow mounts `Drawer.NestedRoot`, and `snapToSequentialPoint` disables velocity skipping where detents are semantic stops.
- Packages: `vaul` (`Drawer.Root`, `Drawer.NestedRoot`, `Drawer.Trigger`, `Drawer.Portal`, `Drawer.Overlay`, `Drawer.Content`, `Drawer.Handle`, `Drawer.Title`, `Drawer.Description`, `Drawer.Close`).
- Law: the sheet's drag is vaul's own — no `use-gesture` binding on a sheet surface (`system/act`'s double-bind defect); sheet motion is the drag physics with `Motion.sheet` for programmatic open/close, never both animating one property.
- Law: `repositionInputs` stays default where a sheet hosts fields — the keyboard-avoidance behavior is the package's; a hand-rolled viewport listener beside it is the named defect.
- Growth: a new sheet detent is one `snapPoints` entry; a new sheet surface is one `Drawer.Root` composition — the detent policy row never forks.

## [04]-[PALETTE]

[PALETTE]:
- Owner: the palette law riding `Overlay` — the `Overlay.Command` vocabulary: one `as const satisfies Record<string, Overlay.Command>` table where each row carries `icon` (a named `LucideIcon` — the row's identity, tree-shaken), `label` (a `system/intl` catalog key), `keywords` (alias tokens for the scorer), and `run` (the intent write — an atom setter or callable atom the app wires); the palette renders the table through cmdk — `Command.Input`/`Command.List`/`Command.Group`/`Command.Item`/`Command.Empty` — with controlled `value`/`onValueChange`, `useCommandState((s) => s.filtered.count)` driving the count/empty rows without list re-render.
- Packages: `cmdk` (`Command` compound, `CommandDialog`, `Command.Loading`, `useCommandState`, `defaultFilter`); `lucide-react` (`LucideIcon` — icon-as-identity); `system/intl` (labels, `useFilter` pre-normalization where locale-sensitive); `system/primitive` (`Clipboard` — the copy rail); `@tanstack/react-virtual` + `@floating-ui/react` `inner`/`useInnerOffset` (the virtualized lane).
- Law: hosting picks exactly one shell — `CommandDialog` for the global modal palette (its own Radix portal + focus trap); a BARE `Command` inside `Drawer.Content` for a sheet palette (vaul owns portal/trap); a bare `Command` inside `FloatingFocusManager` for an anchored palette (floating-ui owns position/portal/trap) — two focus traps on one surface is the named defect, and `useListNavigation`/`useTypeahead` never stack over a cmdk list.
- Law: a result set past the DOM budget virtualizes — the anchored combobox lane windows pre-filtered rows through `useVirtualizer` with `useListNavigation({ virtual: true })` keeping focus on the input (`scrollToIndex` reveals the active row), and `inner`/`useInnerOffset` anchor the tall scrollable list at the active item; cmdk's own list serves below the budget — one lane per palette, chosen by row count.
- Law: item `value` is the stable spec key, never the visible label — filtering and selection survive label localization; `keywords` carry the localized aliases.
- Law: async command sources set `shouldFilter={false}` and render pre-filtered rows from an atom (`Atom.debounce`d query, `Result`-folded, `Command.Loading` on the `waiting` arm) — the machine keeps keyboard/selection, the store owns matching.
- Law: copy-shaped commands ride the port — a `run` that yields text (a share link, an id, an evidence row) composes the `Clipboard` Tag from `system/primitive#CLIPBOARD_PORT` and lands its refusal as a toast note; `navigator.clipboard` in a spec row is the named defect.
- Law: palette motion is `Motion.palette` through the entering/exiting variants — or, where the palette morphs into its result surface, the `Motion` physical plane's `layoutId` morph; one owner per surface, never both.
- Growth: a new command is one spec row; a new palette surface is one hosting-shell choice — the table never forks.

```typescript
import type { LucideIcon } from "lucide-react"

declare namespace Overlay {
  type Command = {
    readonly icon: LucideIcon
    readonly label: string
    readonly keywords: ReadonlyArray<string>
    readonly run: () => void
  }
}
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
    readonly middleware: typeof _middleware
    readonly virtual: typeof _virtual
  }
}

const Overlay: Overlay.Shape = {
  dismiss: _dismiss,
  middleware: _middleware,
  virtual: _virtual,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Overlay }
```
