# [UI_OVERLAY]

Overlay owns anchored floats, drag-dismissable sheets, the one command vocabulary its palette and its pointer-invoked surfaces both read, and presence cursors. Floating-ui positions anchors, vaul owns sheets, cmdk owns palette filtering, react-stately holds the invocation point, and RAC owns standard aria overlays. React-aria props and floating refs merge only where semantics meet geometry; each surface keeps one positioner and one focus owner. Module: `ui/src/view/overlay.ts`.

## [01]-[INDEX]

- [02]-[ANCHOR_HOST]: Overlay stacks the floating-anchor hooks — middleware pipeline, dismiss policy, arrow, delay group; `Overlay`.
- [03]-[SHEET_HOST]: Overlay hosts the vaul sheet rows — detent policy, drag discipline, nesting; `Overlay`.
- [04]-[PALETTE]: Overlay owns the `Overlay.Command` vocabulary — scope, derived grant legality, keymap census, hosting shells, the copy rail; `Overlay`.
- [05]-[PRESENCE_COHORT]: live presence cursors — the roster projection, client-point and virtual-point anchoring; `Overlay`.

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

```typescript signature
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

```typescript signature
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
  content: {}, // Title and Description always mount, so Radix's own aria-labelledby/describedby wiring names live nodes and nothing here overrides it
  handle: { preventCycle: true }, // the handle drags; detent cycling stays the atom's write
})
```

## [04]-[PALETTE]

[PALETTE]:
- Owner: the command law riding `Overlay` — the `Overlay.Command` vocabulary: one table admitted through `Overlay.commands`, where each row carries `icon` (a named `LucideIcon` — the row's identity, tree-shaken), `label` (a `system/intl` catalog key), `keywords` (alias tokens for the scorer), `scope` (which surface the row answers on), `binding` (its canonical chord, or `null` where the row carries no key), `needs` (the grants its legality requires, empty where the row is unconditionally live), and `run` (the intent Effect a boundary adapter hands the app runtime); the global subset renders through cmdk — `Command.Input`/`Command.List`/`Command.Group`/`Command.Item`/`Command.Empty` — with controlled `value`/`onValueChange`, `useCommandState((s) => s.filtered.count)` driving the count/empty rows without list re-render.
- Packages: `cmdk` (`Command` compound, `CommandDialog`, `Command.Loading`, `useCommandState`, `defaultFilter`); `lucide-react` (`LucideIcon` — icon-as-identity); `react-aria` (`KeyboardShortcutBindings` — the record `system/act#DISCRETE_ROWS` feeds `useKeyboard`); `react-stately` (`MenuTriggerType`'s `contextMenu` arm, `OverlayTriggerState.point`/`setPoint`); `react-aria-components` (`MenuTrigger` — the aria-menu arm); `system/intl` (labels, `useFilter` pre-normalization where locale-sensitive); `system/primitive` (`Clipboard` — the copy rail); `@tanstack/react-virtual` + `@floating-ui/react` `inner`/`useInnerOffset` (the virtualized lane).
- Law: ONE vocabulary, two readers — `scope` admits a row into a surface, so viewer marks, review rows, and tree nodes each reach their own subset through `Overlay.scoped` while the palette narrows to `global`; a second command table beside this one is the fork the column exists to foreclose, and a new surface class is one vocabulary member rather than a new record.
- Law: legality is DERIVED at every row and STORED on none — `needs` declares which members of the closed `_grants` roster a row requires, the projection carries the `held` set composition folded this render, and `Overlay.admits` answers `Either<Command, Missing>` whose left names exactly the grants the row wanted and the surface lacks; an `enabled`/`visible`/`disabled` boolean written onto a row is the named defect, because it freezes at authoring time a verdict the state it claims to read moves out from under, and a row requiring nothing states `needs: []` — an empty set is a STATED answer, never an omitted column.
- Law: a grant exists only beside the live read that already publishes it — `advance` and `revert` derive from `core:state/machine#MACROSTEP_FOLD`'s `Transition.legal(config)` and `Transition.admits(config, signal)` over the standing surface's forward and backward signals, `selection` from a non-empty present set at `viewer/mark#SELECTION_FOLD`, `residency` and `accelerated` from `viewer/scene`'s resident graft and its baked acceleration band, and `clipboard` and `egress` from the `Clipboard` and `Egress` Tags (`system/primitive#CLIPBOARD_PORT`, `view/export#EGRESS_PORT`) being satisfied at the browser root; a member with no such producer is decoration, and a face re-deriving a transition guard the macrostep already folds forks the machine's own row table into a second, unverified answer.
- Law: refusal RENDERS, never silences — a refused palette row stays mounted carrying cmdk's own `disabled` beside its `Missing` grants as the rendered reason, while `Overlay.bindings` EXCLUDES that row's chord outright so the key travels on to whatever else claims it instead of meeting a bound no-op (`useKeyboard({ shortcuts })` is the sole consumption path, so an excluded row is unbound by construction). This is not the disabled-veneer defect `view/form#WIZARD` names: that veneer sits over an unguarded machine and states a verdict nothing derived, where this one reads the same evidence `run` reads and names the missing grant beside the row.
- Law: scope and legality are orthogonal reads over the one table — `Overlay.scoped` decides WHICH surface a row answers on and `Overlay.admits` decides whether it is live there, so a surface never folds legality into its scope filter and no command is minted twice to carry two states of itself.
- Law: pointer invocation SELECTS, then opens — the scoped surface establishes its own selection before the float mounts, so `run` stays a nullary intent reading that surface's selection atom and no row carries a subject; a `run` taking the invoked mark, row, or node forks the vocabulary once per scope.
- Law: `run` is an Effect, never a thunk — the copy rail composes the `Clipboard` Tag from `system/primitive#CLIPBOARD_PORT` through the requirement channel and the row handles its own refusal into a toast note, so a row is total (`Effect<void, never, R>`) by the time it enters the table; cmdk's `onSelect` and the shortcut action are the two boundary adapters that hand it to the app runtime, and a `navigator.clipboard` read or a bare thunk in a spec row is the named defect.
- Law: the positional surface is the anchored HOST carrying a point reference, never a fourth host row — `Overlay.hosts.anchored.anchor` is `reference`, and a reference is a trigger element OR a `VirtualElement` — `Overlay.virtual(rect)` wraps any live rect and `Overlay.point(x, y)` is its zero-size instance, which `autoUpdate` tracks like any anchor; an aria MENU of scoped rows instead rides RAC `MenuTrigger` at `trigger: "contextMenu"`, where RAC pins `offset: 0` and `usePopover` derives the same zero-size rect from `state.point`, and one surface takes exactly one of the two positioners.
- Law: the invocation point converts ONCE, and here — `useContextMenu` answers `x`/`y` RELATIVE TO THE TARGET (`system/act#DISCRETE_ROWS` owns the gesture) while `OverlayTriggerState.point` is viewport-relative by definition, so the fold is `rect = target.getBoundingClientRect()` then `setPoint({ x: rect.x + e.x, y: rect.y + e.y })`, and a raw `ContextMenuEvent` coordinate reaching a positioner lands the float off by the target's origin. This page writes the point shape structurally and never imports it — react-stately exports no type for it, and its barrel `Point` is the virtualizer's geometry class.
- Law: a chord is declared once and read twice — the `binding` column is the canonical shortcut string, projected by `Overlay.bindings` into the `shortcuts: KeyboardShortcutBindings` record `useKeyboard` matches and by `Overlay.chord` into the display tokens the row renders beside its label; `Mod` is the mandated spelling so one table serves Apple and non-Apple, and the census runs both expansions while the DISPLAY expansion derives from `Overlay.usePlatform`.
- Law: a display token spells THREE ways per PLATFORM off two tables — `_SPELLINGS` keys the four modifiers by platform (⌘/⌃/⇧/⌥ glyphs and Apple words on `apple`; Ctrl/Win/Alt words on `other`, since a glyph a keyboard never prints misleads) and `_DISPLAY` keys the platform-invariant tokens, each row the closed register set: `condensed` (the kbd chip form), `prose` (the full word a menu hint renders), and `spoken` (the accessible announcement string — "command", "backtick", "forward slash"); `Overlay.display(tokens, register, platform)` folds a chord's tokens through both tables — platform spellings first — with an unlisted token answering itself title-cased for the visual registers and lowercased for the spoken one, so a kbd chip, a tooltip, and an announcement never spell one key three drifting ways and `usePlatform`'s read is the fold's own input.
- Law: the platform read is SSR-safe and singular — `Overlay.usePlatform` is a `useSyncExternalStore` row whose subscription is inert (a platform never changes mid-session), whose client snapshot reads the navigator once, and whose SERVER snapshot answers `other`, so prerendered markup spells the non-Apple form and hydration re-reads without a mismatch tear; a `navigator.platform` read in a row beside this hook re-mints the probe.
- Law: the parse is this page's because the shipped one is private — `createKeyboardShortcutHandler` and its parse/canonicalize helpers are internal, `useKeyboard({ shortcuts })` being the one consumption path, so display and census re-derive the same canonical form: modifier tokens are case-insensitive and order-free, the recognized set is `Alt`/`Control`/`Meta`/`Shift`/`Mod` — a `Ctrl` spelling is NOT among them, the parser drops it into the key slot and the modifier vanishes, which `Overlay.Chord` refuses at the declaration — and the canonical form orders Alt, Control, Meta, Shift ahead of the lowercased, alias-folded key.
- Law: `Overlay.commands` is the construction gate and its refusal carries evidence — two rows normalizing to one canonical form make the shipped matcher hand the key to whichever entry came LATER, a silent shadow no type sees, so the census refuses the whole table with the colliding form and the rows that produced it; it runs both `Mod` expansions apart, because a pair colliding only under Cmd still shadows on every Apple host, and a user-rebound key is one written value on the row re-entering this same gate.
- Law: bindings ARM AT MOUNT of their scope surface — the global subset arms on the palette's document-level bundle and a scoped subset arms with its owner's mount, so an unmounted scope answers no key and dispatch always leaves through the one table; a surface binding a key handler beside the record re-opens the shadow the census closes.
- Law: hosting picks exactly one shell, and `Overlay.hosts` is the row table that makes the choice checkable — each row names which package owns the portal, which owns the focus trap, and what the shell anchors against, all three closed unions so a mismatched pairing fails at the declaration rather than in review: `modal` takes `CommandDialog` (cmdk's own Radix portal and trap, viewport-centred), `sheet` takes a BARE `Command` inside `Drawer.Content` (vaul owns both, edge-docked), and `anchored` takes a bare `Command` inside `FloatingFocusManager` (floating-ui owns position, portal, and trap). Two focus traps on one surface is the named defect the `trap` column exists to expose, `CommandDialog` inside either non-modal host is that defect spelled twice, and `useListNavigation`/`useTypeahead` never stack over a cmdk list.
- Law: a result set past the DOM budget virtualizes — the anchored combobox lane windows pre-filtered rows through `useVirtualizer` with `useListNavigation({ virtual: true })` keeping focus on the input (`scrollToIndex` reveals the active row), and `inner`/`useInnerOffset` anchor the tall scrollable list at the active item; cmdk's own list serves below the budget — one lane per palette, chosen by row count.
- Law: item `value` is the stable spec key, never the visible label — filtering and selection survive label localization; `keywords` carry the localized aliases.
- Law: async command sources set `shouldFilter={false}` and render pre-filtered rows from an atom (`Atom.debounce`d query, `Result`-folded, `Command.Loading` on the `waiting` arm) — the machine keeps keyboard/selection, the store owns matching.
- Law: palette motion is `Motion.palette` through the entering/exiting variants — or, where the palette morphs into its result surface, the `Motion` physical plane's `layoutId` morph; one owner per surface, never both.
- Growth: a new command is one spec row; a new scope is one vocabulary member its surface reads through the same selector; a new legality axis is one `_grants` member landed beside the owner that publishes its read; a new palette surface is one hosting-shell choice — the table never forks.

```typescript signature
import { Command, CommandDialog, defaultFilter, useCommandState } from "cmdk"
import { Fault } from "@rasm/ts/core"
import { Array, Either, HashSet, Option, Record, Schema, type Effect } from "effect"
import type { LucideIcon } from "lucide-react"
import { useSyncExternalStore, type ComponentProps } from "react"
import type { KeyboardShortcutBindings } from "react-aria"

const _hosts = {
  modal: { shell: CommandDialog, portal: "cmdk", trap: "cmdk", anchor: "viewport" },
  sheet: { shell: Command, portal: "vaul", trap: "vaul", anchor: "edge" },
  anchored: { shell: Command, portal: "floating-ui", trap: "floating-ui", anchor: "reference" },
} as const

// which surface a row answers on: the palette reads `global`, a positional float reads its own member,
// and `shell` is the chrome's — view/shell#COMMAND_SEAM contributes its toggle and collapse rows there
const _scopes = ["global", "mark", "node", "row", "shell"] as const

// what a row NEEDS to be live: every member is a live read some owner already publishes — the machine's admitted
// signals (advance/revert), the selection fold's present set, the scene's resident graft and its baked acceleration
// band, and the two capability Tags the browser root satisfies — so composition folds the held set once per render
// and no row ever stores the verdict this roster derives
const _grants = ["accelerated", "advance", "clipboard", "egress", "residency", "revert", "selection"] as const

// _ORDER, _MODIFIERS, and _ALIASES restate the shipped matcher's sort order, modifier tokens, and key aliases; `ctrl` is absent
// from the token table on purpose — that spelling parses as a KEY and its modifier silently disappears
const _ORDER = ["Alt", "Control", "Meta", "Shift"] as const

const _MODIFIERS: Record.ReadonlyRecord<string, (typeof _ORDER)[number]> = {
  alt: "Alt",
  control: "Control",
  meta: "Meta",
  shift: "Shift",
}

const _ALIASES: Record.ReadonlyRecord<string, string> = {
  del: "delete",
  down: "arrowdown",
  esc: "escape",
  ins: "insert",
  left: "arrowleft",
  pagedown: "pagedown",
  pageup: "pageup",
  right: "arrowright",
  space: " ",
  up: "arrowup",
}

// Two legs partition the table gate and each reason renders its OWN subject: a grammar refusal names the binding
// the shipped parser reads as no chord at all, a census refusal names every row claiming one canonical spelling.
// The rows column is non-empty by construction on both arms — a refusal naming no row is a verdict about nothing —
// and the class column is the core lattice's, so no rank or retry literal lands beside it.
const _commandFamily = Fault.Class.family(["chord", "clash"] as const, {
  chord: Fault.Class.row({
    class: "malformed",
    leg: "grammar",
    detail: Schema.Struct({ chord: Schema.String, rows: Schema.NonEmptyArray(Schema.String) }),
    render: ({ chord, rows }) => `${Array.join(rows, ", ")} binds ${chord}, which parses to no chord`,
  }),
  clash: Fault.Class.row({
    class: "conflicted",
    leg: "census",
    detail: Schema.Struct({ chord: Schema.String, rows: Schema.NonEmptyArray(Schema.String) }),
    render: ({ chord, rows }) => `${chord} is claimed by ${Array.join(rows, ", ")}`,
  }),
})

declare namespace CommandFault {
  type Case = typeof _commandFamily.payload.Type
  type Reason = (typeof _commandFamily.kinds)[number]
}

class CommandFault extends Schema.TaggedError<CommandFault>()("CommandFault", {
  case: _commandFamily.payload,
}) {
  get class(): Fault.Class.Kind {
    return _commandFamily.classOf(this.case.reason)
  }
  get leg(): string {
    return _commandFamily.legOf(this.case.reason)
  }
  override get message(): string {
    return _commandFamily.render(this.case)
  }
}

declare namespace Overlay {
  type Modifier = "alt" | "control" | "meta" | "mod" | "shift"
  // a literal binding validates where it is written: every `+`-separated head is a recognized
  // modifier and the tail is a real key, so a chord whose modifier the shipped parser would swallow never reaches a row
  type Chord<S> = S extends string
    ? S extends `${infer Head}+${infer Rest}`
      ? Lowercase<Head> extends Overlay.Modifier ? ([Overlay.Chord<Rest>] extends [never] ? never : S) : never
      : Lowercase<S> extends Overlay.Modifier | "" ? never : S
    : S
  type Scope = (typeof _scopes)[number]
  type Grant = (typeof _grants)[number]
  type Missing = Array.NonEmptyReadonlyArray<Overlay.Grant> // a refusal names what it wanted and lacked; an empty miss is admission, never a refusal shaped like one
  type Register = (typeof _registers)[number]
  type Owner = "cmdk" | "floating-ui" | "vaul"
  type Command<R = never> = {
    readonly icon: LucideIcon
    readonly label: string // a system/intl catalog key: the scorer reads its RESOLVED text through keywords, never this key
    readonly keywords: ReadonlyArray<string>
    readonly scope: Overlay.Scope
    readonly binding: string | null
    readonly needs: ReadonlyArray<Overlay.Grant> // what legality REQUIRES; the verdict derives per render and is stored nowhere
    readonly run: Effect.Effect<void, never, R> // total by construction: the row folds its own refusal into a note
  }
  type Host = keyof typeof _hosts
  type HostRow = {
    readonly shell: typeof Command | typeof CommandDialog
    readonly portal: Overlay.Owner
    readonly trap: Overlay.Owner
    readonly anchor: "edge" | "reference" | "viewport"
  }
  type Entry = {
    readonly item: ComponentProps<typeof Command.Item>
    readonly chord: ReadonlyArray<string> // display tokens beside the label; empty where the row carries no key
    readonly missing: ReadonlyArray<Overlay.Grant> // the rendered reason a row is inert; empty where it is live
  }
  type Palette = {
    readonly root: ComponentProps<typeof Command>
    readonly entries: ReadonlyArray<Overlay.Entry>
  }
  type Projection<R> = {
    readonly table: Record.ReadonlyRecord<string, Overlay.Command<R>>
    readonly resolve: (label: string) => string
    readonly mod: "Control" | "Meta" // the host's `Mod` expansion for DISPLAY only; matching resolves inside useKeyboard
    readonly held: HashSet.HashSet<Overlay.Grant> // composition's fold over the live reads; the palette derives every verdict against it
    readonly fork: (run: Effect.Effect<void, never, R>) => void
    readonly remote: boolean
  }
  type _Rows<T extends Record.ReadonlyRecord<Overlay.Host, Overlay.HostRow> = typeof _hosts> = T // row guard: a host missing its ownership columns fails at the declaration
}

const _chord = (binding: string, mod: "Control" | "Meta"): Option.Option<ReadonlyArray<string>> => {
  const parts = Array.map(binding.split("+"), (part) => part.toLowerCase())
  const held = Array.filterMap(parts, (part) => (part === "mod" ? Option.some(mod) : Record.get(_MODIFIERS, part)))
  // every non-modifier token overwrites the shipped parser's key slot, so the LAST one is the chord's key
  return Option.map(
    Array.findLast(parts, (part) => part !== "mod" && !Record.has(_MODIFIERS, part)),
    (key) =>
      Array.append(
        Array.filter(_ORDER, (name) => Array.contains(held, name)),
        Option.getOrElse(Record.get(_ALIASES, key), () => key),
      ),
  )
}

const _canonical = (binding: string, mod: "Control" | "Meta"): string =>
  Option.match(_chord(binding, mod), { onNone: () => binding, onSome: Array.join("+") })

const _commands = <R, const T extends Record.ReadonlyRecord<string, Overlay.Command<R>>>(
  table: T & { readonly [K in keyof T]: { readonly binding: Overlay.Chord<T[K]["binding"]> } },
): Either.Either<T, CommandFault> => {
  const bound = Array.filterMap(Record.toEntries(table), ([key, row]) =>
    row.binding === null ? Option.none() : Option.some([key, row.binding] as const))
  const malformed = Array.findFirst(bound, ([, chord]) => Option.isNone(_chord(chord, "Meta")))
  // _commands censuses the two expansions APART: grouping them together reads one `Mod` row as its own rival
  const collided = Array.findFirst(
    Array.flatMap(["Control", "Meta"] as const, (mod) =>
      Record.toEntries(Array.groupBy(bound, ([, chord]) => _canonical(chord, mod)))),
    ([, rows]) => rows.length > 1,
  )
  return Option.match(
    Option.orElse(
      Option.map(malformed, ([key, chord]) => new CommandFault({ case: { reason: "chord", chord, rows: [key] } })),
      () =>
        Option.map(collided, ([chord, rows]) =>
          new CommandFault({ case: { reason: "clash", chord, rows: Array.map(rows, ([key]) => key) } })),
    ),
    { onNone: () => Either.right(table), onSome: Either.left },
  )
}

const _scoped = <R>(
  table: Record.ReadonlyRecord<string, Overlay.Command<R>>,
  scope: Overlay.Scope,
): Record.ReadonlyRecord<string, Overlay.Command<R>> => Record.filter(table, (row) => row.scope === scope)

// legality is one set difference over declared data, so a row's verdict is recomputed from the live reads on every
// consultation and the refusal carries WHICH grants were wanted — a bare boolean here would drop exactly that fact
const _admits = <R>(
  row: Overlay.Command<R>,
  held: HashSet.HashSet<Overlay.Grant>,
): Either.Either<Overlay.Command<R>, Overlay.Missing> => {
  const unmet = Array.filter(row.needs, (grant) => !HashSet.has(held, grant))
  return Array.isNonEmptyReadonlyArray(unmet) ? Either.left(unmet) : Either.right(row)
}

const _bindings = <R>(
  table: Record.ReadonlyRecord<string, Overlay.Command<R>>,
  held: HashSet.HashSet<Overlay.Grant>,
  fork: (run: Effect.Effect<void, never, R>) => void,
): KeyboardShortcutBindings =>
  // an action returning nothing counts as HANDLED — the matcher preventDefaults and stops propagation,
  // which is exactly the command semantics; a row wanting the key to travel on returns `false` instead,
  // and a row whose grants are unmet never enters the record at all, so its chord reaches whoever else claims it
  Record.fromEntries(Array.filterMap(Record.toEntries(table), ([, row]) =>
    Either.match(_admits(row, held), {
      onLeft: () => Option.none<readonly [string, () => void]>(),
      onRight: (live) =>
        live.binding === null ? Option.none() : Option.some([live.binding, () => fork(live.run)] as const),
    })))

const _registers = ["condensed", "prose", "spoken"] as const

// modifier spellings split by PLATFORM before they split by register — ⌘ means nothing on a Windows keyboard and
// "Command" misnames its Meta key — so the four modifier rows live in a platform-keyed table the display fold
// consults ahead of the invariant key table, and _usePlatform's read is consumed here, never decorative
const _SPELLINGS = {
  apple: {
    Alt: { condensed: "⌥", prose: "Option", spoken: "option" },
    Control: { condensed: "⌃", prose: "Control", spoken: "control" },
    Meta: { condensed: "⌘", prose: "Command", spoken: "command" },
    Shift: { condensed: "⇧", prose: "Shift", spoken: "shift" },
  },
  other: {
    Alt: { condensed: "Alt", prose: "Alt", spoken: "alt" },
    Control: { condensed: "Ctrl", prose: "Control", spoken: "control" },
    Meta: { condensed: "Win", prose: "Windows", spoken: "windows" },
    Shift: { condensed: "Shift", prose: "Shift", spoken: "shift" },
  },
} as const satisfies Record<"apple" | "other", Record.ReadonlyRecord<string, Record<(typeof _registers)[number], string>>>

// one display table, three registers per platform-invariant token; tokens outside the table answer themselves
// through the register's own case fold
const _DISPLAY = {
  arrowdown: { condensed: "↓", prose: "Down", spoken: "down arrow" },
  arrowleft: { condensed: "←", prose: "Left", spoken: "left arrow" },
  arrowright: { condensed: "→", prose: "Right", spoken: "right arrow" },
  arrowup: { condensed: "↑", prose: "Up", spoken: "up arrow" },
  backspace: { condensed: "⌫", prose: "Backspace", spoken: "backspace" },
  delete: { condensed: "⌦", prose: "Delete", spoken: "delete" },
  enter: { condensed: "↵", prose: "Enter", spoken: "enter" },
  escape: { condensed: "Esc", prose: "Escape", spoken: "escape" },
  tab: { condensed: "⇥", prose: "Tab", spoken: "tab" },
  " ": { condensed: "Space", prose: "Space", spoken: "space" },
  "`": { condensed: "`", prose: "`", spoken: "backtick" },
  "/": { condensed: "/", prose: "/", spoken: "forward slash" },
  "\\": { condensed: "\\", prose: "\\", spoken: "backslash" },
  ",": { condensed: ",", prose: ",", spoken: "comma" },
  ".": { condensed: ".", prose: ".", spoken: "period" },
} as const satisfies Record.ReadonlyRecord<string, Record<(typeof _registers)[number], string>>

const _display = (
  tokens: ReadonlyArray<string>,
  register: Overlay.Register,
  platform: "apple" | "other",
): ReadonlyArray<string> =>
  Array.map(tokens, (token) =>
    Option.match(
      Option.orElse(Record.get(_SPELLINGS[platform], token), () => Record.get(_DISPLAY, token)),
      {
        onSome: (row) => row[register],
        // an unlisted token is a plain key: the visual registers title-case it, the spoken one keeps it lowercase
        onNone: () => (register === "spoken" ? token.toLowerCase() : token.charAt(0).toUpperCase() + token.slice(1)),
      },
    ))

// rules-of-hooks reads the NAME, so the `use` prefix is load-bearing; the server snapshot answers `other` so
// prerendered chords spell the non-Apple form and hydration re-reads without a tear
const _usePlatform = (): "apple" | "other" =>
  useSyncExternalStore(
    () => () => {}, // a platform never changes mid-session: the subscription is inert
    () => (/mac|iphone|ipad/i.test(globalThis.navigator.platform) ? "apple" : "other"),
    () => "other",
  )

const _virtual = (rect: () => DOMRect): VirtualElement => ({ getBoundingClientRect: rect })

// _point mints the zero-size rect the RAC arm derives from `state.point` through usePopover's own getTargetRect
const _point = (x: number, y: number): VirtualElement => _virtual(() => new DOMRect(x, y, 0, 0))

const _palette = <R>(projection: Overlay.Projection<R>): Overlay.Palette => ({
  // an async source hands the store the matching and renders pre-filtered rows; the local table keeps cmdk's scorer
  root: { shouldFilter: !projection.remote, loop: true, filter: defaultFilter },
  entries: Array.map(Record.toEntries(_scoped(projection.table, "global")), ([key, row]) => {
    // one verdict read twice at one site: cmdk's own `disabled` refuses the selection and the same miss renders the reason
    const missing = Option.getOrElse(
      Either.getLeft(_admits(row, projection.held)),
      (): ReadonlyArray<Overlay.Grant> => [],
    )
    return {
      item: {
        value: key, // the stable spec key: filtering and selection survive label localization
        keywords: [projection.resolve(row.label), ...row.keywords], // the localized text enters the scorer HERE, never as the value
        disabled: !Array.isEmptyReadonlyArray(missing),
        onSelect: () => projection.fork(row.run),
      },
      chord: row.binding === null ? [] : Option.getOrElse(_chord(row.binding, projection.mod), () => []),
      missing,
    }
  }),
})

// rules-of-hooks reads the NAME, so the `use` prefix is load-bearing: this member holds a cmdk store subscription
const _useMatched = (): number => useCommandState((state) => state.filtered.count) // the count and empty rows read the store, so the list never re-renders to report its own size
```

## [05]-[PRESENCE_COHORT]

[PRESENCE_COHORT]:
- Owner: `Overlay.cursors(seen, horizon, lease, project)` — the collaborative-cursor projection: the presence fold table (per-actor `Presence.State`, entering the view plane as an atom through `system/atom#LIVE_BRIDGE`) folds against `Presence.roster`'s lease verdicts into one anchored row per SIGHTED actor — the actor's `cursor` axis mapped through the caller's projection and wrapped by `Overlay.point`, the worn `face` carrying name and hue, the `idle` flag carrying the dimming a recipe styles; a `gone` verdict and an actor whose cursor axis never arrived both leave the projection entirely, so ageing is the roster's lease and never a local timer per cursor.
- Packages: `@floating-ui/react` (`useClientPoint`, `FloatingPortal`); `@rasm/ts/core` (`Presence` — the roster read, the lease law, and the `cursor`/`face` axes arrive settled; `Clock.Hlc` and `Fold.Table` type the horizon and the table); `effect` (`Array`, `HashMap`, `Option`).
- Law: the LOCAL pointer never enters this projection — its own affordance (a cursor-attached label, a drag ghost) anchors through `useClientPoint(context)`, the shipped cursor-follow anchor, while every REMOTE actor anchors by point; a hand-built rect wrapper on either side restates one of the two.
- Law: coordinates arrive projected, and the projection answers `Option` — `project` maps a `Presence.Point` (tagged `Sheet` or `Scene`, each carrying its own surface id) into viewport coordinates or `Option.none` for a point no registered space resolves, and the fold DROPS an unresolvable actor rather than rendering at a fabricated coordinate; `view/presence#CURSOR_PLANE`'s `Face.project` over the assembled anchor lanes is the supplying seam, so a map surface's viewer projection and a plain sheet feed the same cohort and this page holds no coordinate math and no surface branch.
- Law: presence is render-only — cursors mount in one `FloatingPortal` at the `Theme.Scale.z` cursor rank with `Motion.panel` on the row, never intercept pointer events (`pointer-events: none` in the recipe), and carry no focus semantics; the cohort is a projection of state, not an interaction surface.
- Law: identity is the whole `Presence.Key` — tenant scope beside actor, because an actor id alone collides across tenants on a shared surface; name and hue derive from the worn `face` profile through the theme ramp, so no per-actor style state lives in the component.
- Boundary: the roster fold, lease policy, and status verdicts live in the core presence plane; the horizon reads the one frame clock through the same bridge, and the projection arrives as a parameter so the cohort is surface-agnostic.

```typescript signature
import { Presence, type Clock, type Fold } from "@rasm/ts/core"
import { Array, HashMap, Option } from "effect"

declare namespace Overlay {
  type Cursor = {
    readonly key: Presence.Key
    readonly face: Option.Option<Presence.Profile>
    readonly anchor: VirtualElement
    readonly idle: boolean
  }
  type Shape = {
    readonly admits: typeof _admits
    readonly bindings: typeof _bindings
    readonly chord: typeof _chord
    readonly commands: typeof _commands
    readonly cursors: typeof _cursors
    readonly dismiss: typeof _dismiss
    readonly display: typeof _display
    readonly hook: typeof _presentHook
    readonly hosts: typeof _hosts
    readonly middleware: typeof _middleware
    readonly palette: typeof _palette
    readonly point: typeof _point
    readonly present: typeof _present
    readonly roots: typeof _roots
    readonly scoped: typeof _scoped
    readonly sheet: typeof _sheet
    readonly useMatched: typeof _useMatched
    readonly usePlatform: typeof _usePlatform
    readonly virtual: typeof _virtual
  }
}

const _cursors = (
  seen: Fold.Table<Presence.Key, Presence.State>,
  horizon: Clock.Hlc,
  lease: Presence.Lease,
  project: (point: Presence.Point) => Option.Option<readonly [number, number]>,
): ReadonlyArray<Overlay.Cursor> =>
  Array.filterMap(HashMap.toEntries(Presence.roster(seen, horizon, lease)), ([key, status]) =>
    status === "gone"
      ? Option.none()
      : Option.flatMap(HashMap.get(seen, key), (state) =>
        // cursor-axis presence marks an actor SIGHTED here; a live actor who has never moved renders nothing,
        // and an actor whose point no registered space resolves DROPS rather than landing at a guessed pixel
        Option.flatMap(state.cursor, (worn) =>
          Option.map(project(worn.value), ([x, y]) => ({
            key,
            face: Option.map(state.face, (face) => face.value),
            anchor: _point(x, y),
            idle: status === "idle",
          })))))

const Overlay: Overlay.Shape = {
  admits: _admits,
  bindings: _bindings,
  chord: _chord,
  commands: _commands,
  cursors: _cursors,
  dismiss: _dismiss,
  display: _display,
  hook: _presentHook,
  hosts: _hosts,
  middleware: _middleware,
  palette: _palette,
  point: _point,
  present: _present,
  roots: _roots,
  scoped: _scoped,
  sheet: _sheet,
  useMatched: _useMatched,
  usePlatform: _usePlatform,
  virtual: _virtual,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { CommandFault, Overlay }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
