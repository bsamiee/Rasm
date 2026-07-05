# [UI_OVERLAY]

The one overlay owner: anchored positioning rides floating-ui's hook stack, the drag-dismissable sheet rides vaul's bundled Radix stack, the global command palette rides cmdk's filtering machine over an `Overlay.Command` vocabulary, and the presence-cursor cohort projects the live roster through floating-ui `VirtualElement` anchoring — four overlay classes, exactly one semantic owner and one positioner per element. RAC `Popover`/`Modal` own standard aria overlays outside this module; react-aria's `mergeProps` + floating-ui's `useMergeRefs` fold the two prop/ref systems where aria semantics meet floating geometry; two focus traps on one surface is the named defect. The module is `ui/src/view/overlay.ts`.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                                     | [PUBLIC]  |
| :-----: | :---------------- | :---------------------------------------------------------------------------- | :-------- |
|  [01]   | `ANCHOR_HOST`     | the floating-anchor hook stack, middleware pipeline, focus/portal composition  | `Overlay` |
|  [02]   | `SHEET_HOST`      | the vaul sheet rows — detent policy, drag discipline, nesting                  | `Overlay` |
|  [03]   | `PALETTE`         | the `Overlay.Command` vocabulary and the cmdk hosting-shell law                    | `Overlay` |
|  [04]   | `PRESENCE_COHORT` | live presence cursors over virtual-element anchoring                           | —         |

## [2]-[ANCHOR_HOST]

[ANCHOR_HOST]:
- Owner: the anchor-host law riding `Overlay` — `Overlay.middleware(options)` is the landed member minting the canonical `offset → flip → shift → size` pipeline; the consuming row composes `useFloating` with `whileElementsMounted: autoUpdate`; interactions merge through `useInteractions([useClick, useDismiss, useRole])` into the three prop-getters; `FloatingFocusManager` (modal for dialogs, non-modal + `preserveTabOrder` for menus) and `FloatingPortal` complete the stack; open state binds an atom through `useFloatingRootContext({ open, onOpenChange })` so visibility is store state; enter/exit rides `useTransitionStyles` phases consuming `Scale.ease` values.
- Packages: `@floating-ui/react` (`useFloating`, `useFloatingRootContext`, `autoUpdate`, `offset`, `flip`, `shift`, `size`, `useInteractions`, `useClick`, `useDismiss`, `useRole`, `useHover` + `safePolygon`, `useListNavigation`, `useTypeahead`, `useTransitionStyles`, `useMergeRefs`, `FloatingFocusManager`, `FloatingPortal`, `FloatingTree`, `FloatingNode`); `system/token` (`Scale.ease`).
- Law: hover-opened anchors take `useHover` with `safePolygon` — the one sanctioned pairing of hover-intent and floating geometry (`system/act` owns the hover hook law); list-shaped floats compose `useListNavigation`/`useTypeahead` on the SAME `useInteractions` array, never as a second interaction fold.
- Law: nested floats needing dismissal coordination take `FloatingTree`/`FloatingNode`; flat overlays never mount the tree.
- Law: the `size.apply` style write is floating-ui's platform-forced statement seam — the middleware hands a live element and the write is the documented application; this card carries the exemption.
- Growth: a new anchored surface is one hook composition over the same pipeline; a new middleware concern is one pipeline row — never a second positioner.

```typescript
import { autoUpdate, flip, offset, shift, size } from "@floating-ui/react"
import type { Placement, VirtualElement } from "@floating-ui/react"

declare namespace Overlay {
  type Anchor = { readonly placement: Placement; readonly gap: number; readonly floor: number }
  type Detents = { readonly points: ReadonlyArray<number | string>; readonly fadeFrom: number }
}

const _middleware = (options: Overlay.Anchor) => [
  offset(options.gap),
  flip(),
  shift({ padding: options.gap }),
  size({
    apply: ({ availableHeight, elements }) => {
      elements.floating.style.maxHeight = `${Math.max(options.floor, availableHeight)}px`
    },
  }),
]
```

## [3]-[SHEET_HOST]

[SHEET_HOST]:
- Owner: the sheet law riding `Overlay` — the vaul host: `Drawer.Root` with `open`/`onOpenChange` and `activeSnapPoint`/`setActiveSnapPoint` atom-bound, `snapPoints` + `fadeFromIndex` as the detent policy row (`Overlay.Detents`), `Drawer.Title` + `Drawer.Description` always present (visually hidden when no heading shows), `handleOnly` for drag-origin discipline with `Drawer.Handle` as the affordance; a nested flow mounts `Drawer.NestedRoot`, and `snapToSequentialPoint` disables velocity skipping where detents are semantic stops.
- Packages: `vaul` (`Drawer.Root`, `Drawer.NestedRoot`, `Drawer.Trigger`, `Drawer.Portal`, `Drawer.Overlay`, `Drawer.Content`, `Drawer.Handle`, `Drawer.Title`, `Drawer.Description`, `Drawer.Close`).
- Law: the sheet's drag is vaul's own — no `use-gesture` binding on a sheet surface (`system/act`'s double-bind defect); sheet motion is the drag physics plus `Motion.sheet` for programmatic open/close, never both animating one property.
- Law: `repositionInputs` stays default where a sheet hosts fields — the keyboard-avoidance behavior is the package's; a hand-rolled viewport listener beside it is the named defect.
- Growth: a new sheet detent is one `snapPoints` entry; a new sheet surface is one `Drawer.Root` composition — the detent policy row never forks.

## [4]-[PALETTE]

[PALETTE]:
- Owner: the palette law riding `Overlay` — the `Overlay.Command` vocabulary: one `as const satisfies Record<string, Overlay.Command>` table where each row carries `icon` (a named `LucideIcon` — the row's identity, tree-shaken), `label` (a `system/intl` catalog key), `keywords` (alias tokens for the scorer), and `run` (the intent write — an atom setter or callable atom the app wires); the palette renders the table through cmdk — `Command.Input`/`Command.List`/`Command.Group`/`Command.Item`/`Command.Empty` — with controlled `value`/`onValueChange`, `useCommandState((s) => s.filtered.count)` driving the count/empty rows without list re-render.
- Packages: `cmdk` (`Command` compound, `CommandDialog`, `Command.Loading`, `useCommandState`, `defaultFilter`); `lucide-react` (`LucideIcon` — icon-as-identity); `system/intl` (labels, `useFilter` pre-normalization where locale-sensitive).
- Law: hosting picks exactly one shell — `CommandDialog` for the global modal palette (its own Radix portal + focus trap); a BARE `Command` inside `Drawer.Content` for a sheet palette (vaul owns portal/trap); a bare `Command` inside `FloatingFocusManager` for an anchored palette (floating-ui owns position/portal/trap) — two focus traps on one surface is the named defect, and `useListNavigation`/`useTypeahead` never stack over a cmdk list.
- Law: item `value` is the stable spec key, never the visible label — filtering and selection survive label localization; `keywords` carry the localized aliases.
- Law: async command sources set `shouldFilter={false}` and render pre-filtered rows from an atom (`Atom.debounce`d query, `Result`-folded, `Command.Loading` on the `waiting` arm) — the machine keeps keyboard/selection, the store owns matching.
- Law: palette motion is `Motion.palette` through the entering/exiting variants; palette availability (which specs render) derives from an availability atom, so a disabled command is data, never a hidden row surprise.
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

declare const _specs: Record<string, Overlay.Command>
```

## [5]-[PRESENCE_COHORT]

[PRESENCE_COHORT]:
- Owner: the presence-cohort law riding `Overlay` — the collaborative-cursor cohort: the live roster (`Presence.roster` entering as an atom through `system/atom#LIVE_BRIDGE`) renders one cursor row per actor, each anchored by a floating-ui `VirtualElement` whose `getBoundingClientRect` projects the actor's world coordinate through the owning surface's projection (the viewer projection seam for map surfaces, plain viewport coordinates elsewhere); cursors mount in one `FloatingPortal`, motion rides `Motion.panel`, and idle actors age out by the roster's own lease verdicts — never a local timer per cursor.
- Packages: `@floating-ui/react` (`VirtualElement`, `FloatingPortal`); `@rasm/ts/core` (`Presence` — roster shape and lease law arrive settled).
- Law: presence is render-only — cursors never intercept pointer events (`pointer-events: none` in the recipe) and carry no focus semantics; the cohort is a projection of state, not an interaction surface.
- Law: per-actor identity is keyed by `Presence.Actor`; label/color derive from the actor's face metadata through the theme ramp — no per-actor style state in the component.
- Boundary: the roster fold, lease policy, and status verdicts live in the core presence plane; the projection function arrives as a parameter so the cohort is surface-agnostic.

```typescript
const _virtual = (rect: () => DOMRect): VirtualElement => ({ getBoundingClientRect: rect })

const Overlay: {
  readonly middleware: typeof _middleware
  readonly virtual: typeof _virtual
} = {
  middleware: _middleware,
  virtual: _virtual,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Overlay }
```
