# [UI_SHELL]

Shell owns application chrome as data: one region roster the pane solver realizes, one navigation item vocabulary every rail, tab strip, and breadcrumb renders, one scaffold grammar for page composition, and the shell's command rows on the one overlay vocabulary. react-resizable-panels solves pane constraints and ships the window-splitter interaction whole; RAC supplies tabs, breadcrumbs, disclosure, and tree semantics; routing stays the runtime plane's — hrefs arrive as plain strings through the atom bridge, and this owner resolves none. Module: `ui/src/view/shell.ts`.

Composition facts arrive settled: the pane engine's `Layout` value crosses as `{ [panelId]: percentage }` through `defaultLayout` + `onLayoutChanged` (`.api/react-resizable-panels.md`); persisted grain keys derive through `system/atom#STORE_ROOT`'s versioned seal; the z ladder and breakpoints are `system/token#SCALE_TABLES` rows; enter/exit motion is `system/act#MOTION_ROWS`; the command table, its scope column, and the chord census are `view/overlay#PALETTE`'s — this page legislates rows for it and mints no second table.

## [01]-[INDEX]

- [02]-[REGION_PLANE]: `Shell.Region` rows drive the pane-solver fold, stance stamping, and the persisted layout parcel; `Shell`.
- [03]-[NAV_PLANE]: `Shell.Item` carries href-keyed selection, the hierarchy discriminant, and breadcrumb elision; `Shell`.
- [04]-[SCAFFOLD]: page scaffold areas, the header slot roster, the item-row recipe, the responsive law; `Shell`.
- [05]-[COMMAND_SEAM]: Shell legislates its command rows onto the overlay vocabulary under the binding-collision law; —.

## [02]-[REGION_PLANE]

[REGION_PLANE]:
- Owner: the region algebra riding `Shell` — `Shell.Region` rows carry the whole chrome axis set as data (`side`, `variant`, `collapse`, mixed-unit `size` bounds, `collapsedSize`), `Shell.rail(orientation, regions, held, drive)` folds the roster and the persisted `Layout` into prop records for `Group`, each region `Panel`, the content remainder, and every `Separator`, and `Shell.stamped(region, stance)` projects a region's live stance into the `data-*` attribute record recipes style against — so a new chrome arrangement is a region row set and zero components.
- Packages: `react-resizable-panels` (`Group`, `Panel`, `Separator`, `Layout`, `LayoutChangedMeta` — the solver, the window-splitter aria/key model, and coarse-pointer hit targets all ship inside); `effect` (`Array`, `Option`, `Record`, `Schema`); `react` (`ComponentProps`).
- Entry: one `Shell.rail` call per pane group; a second group is a nested `Group` under the same fold, never a second engine.
- Law: prop records LIFT, never restate — every returned record is `ComponentProps` of the part it feeds (the `Overlay.sheet` law), so a package prop rename breaks here rather than at every app shell; `Panel` and `Separator` stay direct DOM children of their `Group`, ids are explicit and stable (the `useId` fallback re-keys across renders and orphans a persisted layout), and a `Separator` renders between every adjacent pair because edge dragging alone leaves the split unreachable by keyboard.
- Law: persistence is the props pair gated on provenance — `defaultLayout` seeds from the persisted atom and `onLayoutChanged(layout, meta)` writes back only under `meta.isUserInteraction`, so a restore, a constraint recompute, and a programmatic `setLayout` never echo as user edits; the parcel decodes through `Shell.Persisted` on an `Atom.kvs` row whose key mints through the store owner's one `Store.key` member under the versioned seal (`system/atom#STORE_ROOT`) — a schema bump reads as absence and the shell boots on its region defaults, never on a mis-decoded layout.
- Law: exactly one relative panel per group — the content remainder carries `preserve-relative-size` while every chrome region pins `preserve-pixel-size`, so a window resize scales the work surface and holds the rails; the solver requires at least one relative member and this fold supplies it by construction.
- Law: stance is DOM data, never a JS branch — `data-state`/`data-variant`/`data-side`/`data-collapsible` from `Shell.stamped` are the whole styling contract, recipes read them through the `cn` rail's variants, and `data-collapsible` stamps EMPTY while expanded so the collapse selector matches only the collapsed state; a `stance.open` conditional selecting class strings in render is the named defect.
- Law: stance persists at the REGION grain — the parcel's `open` record keys region keys, so a rail and an inspector collapse independently and a toggle command names its region; one shared boolean re-opens every region on any toggle, and the `inspected` scaffold row already proves two collapsible regions in one chrome.
- Law: the desktop frame is two elements per collapsible region — a in-flow GAP element animating width beside a FIXED positioned container carrying the content — so collapse and expand reflow the content column without laying out the rail's interior every frame; the gap/fixed pair is stylesheet structure the stamped attributes select, not module code.
- Law: the mobile arm is the sheet host — under the coarse breakpoint a collapsible region renders through the overlay sheet row with `Shell.Stance.openMobile` as its detent-open state and an SR-only title, so no second drawer engine exists and the region's item rows render identically in both harnesses.
- Growth: a new region is one row; a new chrome arrangement is one roster value; a vertical dock is the `orientation` value; a new stance axis is one `Shell.Stance` field beside its stamp — never a sibling shell component.
- Boundary: which regions exist, their sizes, and their sides are app composition data; the sheet mechanics are `view/overlay#SHEET_HOST`'s; the kvs mechanics and the seal posture are `system/atom`'s.

```typescript signature
import { Array, Option, Record, Schema } from "effect"
import type { ComponentProps } from "react"
import { Group, Panel, Separator } from "react-resizable-panels"
import type { Layout, LayoutChangedMeta } from "react-resizable-panels"
import { Store } from "../system/atom.ts"

const _variants = ["sidebar", "floating", "inset"] as const
const _collapses = ["offcanvas", "icon", "none"] as const
const _sides = ["start", "end"] as const

// `Store.key` mints this key at system/atom#STORE_ROOT: a parcel-shape change bumps the declared ordinal and
// yesterday's layout reads as absence onto the region defaults
const _LAYOUT = Store.key({ domain: "shell", grain: "layout", seal: { posture: "versioned", version: 1 } })

// chrome pins pixels, so the content remainder owns the relative slack above this floor
const _CONTENT = { id: "content", floor: "20%" } as const

declare namespace Shell {
  type Variant = (typeof _variants)[number]
  type Collapse = (typeof _collapses)[number]
  type Side = (typeof _sides)[number]
  type Size = {
    readonly min: number | string
    readonly default: number | string
    readonly max: number | string
  }
  type Region = {
    readonly key: string
    readonly side: Shell.Side
    readonly variant: Shell.Variant
    readonly collapse: Shell.Collapse
    readonly size: Shell.Size
    readonly collapsedSize: Option.Option<number | string>
  }
  type Stance = { readonly open: boolean; readonly openMobile: boolean }
  type Rail = {
    readonly group: ComponentProps<typeof Group>
    readonly content: ComponentProps<typeof Panel>
    readonly panels: Record.ReadonlyRecord<string, ComponentProps<typeof Panel>>
    readonly separator: ComponentProps<typeof Separator>
  }
  type Persisted = typeof _Persisted.Type
}

// react-resizable-panels types Layout as a mutable id-to-percentage record, so the parcel decodes mutable — Grid.Persisted states the same law;
// stance persists PER REGION KEY — the inspected scaffold already seats two collapsible regions, so one boolean would fork them
const _Persisted = Schema.Struct({
  layout: Schema.mutable(Schema.Record({ key: Schema.String, value: Schema.Number })),
  open: Schema.Record({ key: Schema.String, value: Schema.Boolean }),
})

const _rail = (
  orientation: "horizontal" | "vertical",
  regions: ReadonlyArray<Shell.Region>,
  held: Layout,
  drive: (layout: Layout) => void,
): Shell.Rail => ({
  group: {
    orientation,
    defaultLayout: held,
    // a restore or constraint recompute reads false, so a hydration write never round-trips as a user edit
    onLayoutChanged: (layout: Layout, meta: LayoutChangedMeta) => {
      if (meta.isUserInteraction) drive(layout)
    },
  },
  content: { id: _CONTENT.id, minSize: _CONTENT.floor, groupResizeBehavior: "preserve-relative-size" },
  panels: Record.fromEntries(Array.map(regions, (region) => [
    region.key,
    {
      id: region.key,
      minSize: region.size.min,
      maxSize: region.size.max,
      defaultSize: region.size.default,
      // a window resize scales the one relative content panel and holds every pixel-pinned chrome region
      groupResizeBehavior: "preserve-pixel-size",
      ...(region.collapse !== "none" && { collapsible: true }),
      ...Option.match(region.collapsedSize, {
        onNone: () => ({}),
        onSome: (collapsedSize) => ({ collapsedSize }),
      }),
    } satisfies ComponentProps<typeof Panel>,
  ])),
  separator: {},
})

// these attributes ARE the styling contract: recipes select them through cn variants and render branches on none
const _stamped = (region: Shell.Region, stance: Shell.Stance): Record.ReadonlyRecord<string, string> => ({
  "data-side": region.side,
  "data-variant": region.variant,
  "data-state": stance.open ? "expanded" : "collapsed",
  "data-collapsible": stance.open ? "" : region.collapse,
})
```

## [03]-[NAV_PLANE]

[NAV_PLANE]:
- Owner: the navigation vocabulary riding `Shell` — `Shell.Item` is the one item row every rail, tab strip, menu section, and breadcrumb renders: label as a `system/intl` catalog key, `Option`-carried icon and badge, children for hierarchy, and the invocation union — an item NAVIGATES (`href` the app minted through its router) or INVOKES (`command` naming a row on the overlay table), never both and never neither; `Shell.current(live, items)` folds the live href against the roster into the active key and its ancestor trail, and `Shell.crumbs(trail)` applies the elide policy row.
- Packages: `react-aria-components` (`Tabs`/`TabList`/`Tab`/`TabPanel`, `Breadcrumbs`/`Breadcrumb`, `DisclosureGroup`/`Disclosure`/`DisclosurePanel`, `Tree`/`TreeItem`/`TreeItemContent`, `Toolbar`, `Link`, `Button` — the whole nav interaction surface); `lucide-react` (`LucideIcon` — icon as identity); `effect` (`Array`, `Option`).
- Law: selection is ROUTE-KEYED, never selection state — the live href arrives as a plain string through the atom bridge (the app derives it from its router's location cell; this folder types no runtime member), `Shell.current` answers `aria-current` and the ancestor emphasis from href equality alone, and a collapsed ancestor whose descendant is active stays emphasized because the trail carries every ancestor key; a `selectedKeys` cell mirroring the location is the second copy of domain state the atom law forbids.
- Law: the invocation union is the item's whole discriminant — the `href` arm renders RAC `Link` (router-intercepted by the app's navigation ingress), the `command` arm renders RAC `Button` forking the named overlay row, and the two never stack on one item; a nav item embedding its own Effect forks the one command table.
- Law: hierarchy is a closed discriminant, never a component choice — `disclosure` renders chrome groups through `DisclosureGroup`/`Disclosure` (`allowsMultipleExpanded` as roster policy), `tree` renders data-shaped hierarchy through `Tree`/`TreeItem` where depth is arbitrary and rows are the collection; one region declares one `Shell.Nest` value and both arms read the same `Shell.Item` rows.
- Law: tab strips split by the SAME union — items with `href` render as route tabs (RAC `Tab` carries the link and the router intercepts it; no `TabPanel` mounts because the route swap IS the panel change), items without render content tabs with `TabPanel` arms; one item vocabulary, and a hand-built nav-tab component beside RAC `Tabs` is the named defect.
- Law: breadcrumb overflow is a policy row — `_ELIDE` keeps the head and tail crumbs and folds the middle into one overflow entry rendered as a menu of the folded items; the fold is pure, so an elided trail re-expands from the same value and the policy retunes as two numbers.
- Law: the collapsed rail labels through tooltips DECLARATIVELY — the tooltip trigger disables unless the region is collapsed on a fine pointer (`isDisabled` computed from the stamped stance), so an expanded rail never mounts tooltip machinery and no conditional JSX branches on stance.
- Law: direction is structural — logical properties and the `cn` motion groups' `start`/`end` rows carry RTL, and a directional chevron flips through the direction-aware slide rows; a `left`/`right` literal in a nav recipe is the named defect.
- Growth: a new nav surface is one render of the same rows; a new item fact (a count, a presence dot) is one `Shell.Item` field every renderer inherits; a new hierarchy posture is one `_nests` member — never a sibling item type.
- Boundary: href minting, route matching, and admission are the runtime navigation plane's, reached only through app composition; which items exist is app data; tooltip and menu hosting are `view/overlay`'s.

```typescript signature
import { Array, Option } from "effect"
import type { LucideIcon } from "lucide-react"

const _nests = ["disclosure", "tree"] as const

// head and tail survive; the middle folds into one overflow entry — retuning the trail is two numbers
const _ELIDE = { max: 5, head: 1, tail: 2 } as const

declare namespace Shell {
  type Nest = (typeof _nests)[number]
  type Item =
    & {
      readonly key: string
      readonly label: string // a system/intl catalog key: every renderer resolves it at render
      readonly icon: Option.Option<LucideIcon>
      readonly badge: Option.Option<string>
      readonly children: ReadonlyArray<Shell.Item>
    }
    & (
      | { readonly href: string; readonly command?: never }
      | { readonly href?: never; readonly command: string } // names a row on the overlay command table; the item embeds no Effect
    )
  type Trail = {
    readonly active: Option.Option<string>
    readonly opened: ReadonlyArray<string> // every ancestor of the active item: collapsed-ancestor emphasis reads this
  }
  type Crumb = { readonly key: string; readonly label: string; readonly href: string }
  type Crumbs = {
    readonly kept: ReadonlyArray<Shell.Crumb>
    readonly folded: ReadonlyArray<Shell.Crumb> // renders as one overflow menu entry between head and tail
  }
}

const _walk = (
  live: string,
  items: ReadonlyArray<Shell.Item>,
  ancestors: ReadonlyArray<string>,
): Option.Option<Shell.Trail> =>
  Array.head(Array.filterMap(items, (item) =>
    item.href === live
      ? Option.some<Shell.Trail>({ active: Option.some(item.key), opened: ancestors })
      : _walk(live, item.children, Array.append(ancestors, item.key))))

// href equality against the live location string is the WHOLE selection model: no selection cell exists to drift
const _current = (live: string, items: ReadonlyArray<Shell.Item>): Shell.Trail =>
  Option.getOrElse(_walk(live, items, []), () => ({ active: Option.none(), opened: [] }))

const _crumbs = (trail: ReadonlyArray<Shell.Crumb>): Shell.Crumbs =>
  trail.length <= _ELIDE.max
    ? { kept: trail, folded: [] }
    : {
      kept: [...Array.take(trail, _ELIDE.head), ...Array.takeRight(trail, _ELIDE.tail)],
      folded: Array.drop(Array.dropRight(trail, _ELIDE.tail), _ELIDE.head),
    }
```

## [04]-[SCAFFOLD]

[SCAFFOLD]:
- Owner: the page-composition grammar riding `Shell` — `Shell.areas` names the closed scaffold vocabulary as grid-template rows (each value one named-areas string a page stamps onto its grid container), `Shell.header` is the page-header slot roster, and `Shell.item` is the one generic row recipe (media, content, title, description, actions, header, footer as `data-slot` parts) settings surfaces, list rows, and detail panes all compose — so a settings page is item rows inside a scaffold area and no per-surface row component exists.
- Packages: `class-variance-authority` (`cva` — through `system/primitive#STYLED_SPINE`'s recipe law); `system/token` (`cn`, spacing/text utilities).
- Law: a scaffold is one grid value — the named-areas string is the whole layout decision, regions map onto area names, and a new page shape is one `_AREAS` row; hand-authored flex nesting that restates an area row is the named defect.
- Law: responsive values cross as DATA-attributes — a scaffold or region stamps `data-<axis>-narrow`/`data-<axis>-wide` values and the stylesheet's media queries own the branch, so no JS breakpoint resolution exists and hydration cannot disagree with the viewport; container-query variants serve component-local adaptation under the same law.
- Law: the header is slots, never props — `context` (breadcrumb trail), `title`, `trail` (tab strip), `actions`, and `navigation` are `data-slot` parts the recipe places, each optional by omission; a header variant that re-orders slots is one recipe row.
- Law: the item recipe is structural alone — gap, alignment, and slot placement; tone, fill, and emphasis stay the status and token planes' so an item row inherits semantic color from its content rather than carrying a palette axis of its own.
- Law: scrollable scaffold areas earn their region semantics conditionally — an area that overflows takes `role="region"` with its label and `tabIndex={0}` so keyboard users can reach clipped content, and one that fits stays semantically inert; the overflow read is the platform's, never a stored flag.
- Growth: a new scaffold is one `_AREAS` row; a new header arrangement is one recipe row; a new item slot is one `_SLOTS` member every consumer may render — never a sibling scaffold component.
- Boundary: which scaffold a route uses is app data; skip-link and landmark grammar are `system/primitive`'s a11y charter; density is the token plane's one spacing multiplier.

```typescript signature
import { cva } from "class-variance-authority"

// each page shape owns one named-areas string in this closed vocabulary, mapped onto CSS grid-template-areas
const _AREAS = {
  full: `"header" "content"`,
  docked: `"header header" "nav content"`,
  inspected: `"header header header" "nav content aside"`,
  footed: `"header header" "nav content" "footer footer"`,
} as const satisfies Record<string, string>

const _SLOTS = ["media", "content", "title", "description", "actions", "header", "footer"] as const

const _HEADER = ["context", "title", "trail", "actions", "navigation"] as const

declare namespace Shell {
  type Area = keyof typeof _AREAS
  type Slot = (typeof _SLOTS)[number]
  type HeaderSlot = (typeof _HEADER)[number]
}

// structural alone: tone and fill stay the status and token planes', so a row inherits semantic color from its content
const _item = cva("grid grid-cols-[auto_1fr_auto] items-center gap-3 rounded-md px-3 py-2", {
  variants: {
    density: { comfortable: "min-h-12", compact: "min-h-9 gap-2 px-2 py-1" },
    interactive: { true: "hover:bg-neutral-surface focus-visible:ring-2", false: "" },
  },
  defaultVariants: { density: "comfortable", interactive: false },
})

const _header = cva("grid gap-2 [grid-template-areas:var(--header-areas)]", {
  variants: {
    arrangement: {
      stacked: "[--header-areas:'context'_'title'_'actions'_'navigation']",
      inline: "[--header-areas:'context_context'_'title_actions'_'navigation_navigation']",
    },
  },
  defaultVariants: { arrangement: "inline" },
})
```

## [05]-[COMMAND_SEAM]

[COMMAND_SEAM]:
- Law: the shell's actions are ROWS on the one overlay command table — toggle, collapse, and region-focus land as `Overlay.Command` rows under the `shell` scope member, declared at app composition where the table assembles, so this module imports no view sibling and mints no second table; the fence below is the legislated row shape an app transcribes, not a member of this module's export surface.
- Law: the shell binds no key of its own — every chord is the row's `binding` column projected through the overlay bindings record, and a `keydown` listener beside the table re-opens the shadow the chord census closes; the toggle's shipped default is `Mod+b`.
- Law: a chord an editing surface also claims resolves as DATA, never a guard — `Mod+b` collides with the prose editor's bold intent, the editor's own keymap consumes the key at its focused element before the document-level bundle sees it, and an app hosting both rebinds the shell row to an uncontested chord as one written value re-entering the same census; a focus-in-textbox probe wrapped around the matcher is the named defect because it hides the collision the census exists to surface.
- Law: `run` drives the stance atom — each row's Effect writes `Shell.Stance` through the store (a nullary intent per the overlay law), so palette invocation, chord, and rail-button press are one write path and a replay cannot tell them apart.
- Boundary: the `shell` scope member lands on `view/overlay#PALETTE`'s `_scopes` roster; the table, census, and arming law are that page's; the stance atom is `[02]`'s persisted parcel.

```typescript signature
import type { Effect } from "effect"
import { PanelLeft, PanelRight } from "lucide-react"

// legislated app-side rows: the table type, scope roster, and census live at view/overlay#PALETTE
declare const _toggle: Effect.Effect<void>
declare const _inspect: Effect.Effect<void>

const _rows = {
  "shell.toggle": {
    icon: PanelLeft,
    label: "shell.toggle",
    keywords: ["sidebar", "rail", "collapse"],
    scope: "shell",
    binding: "Mod+b", // collides with the editor's bold intent by design: a co-hosting app rebinds this one value
    run: _toggle,
  },
  "shell.inspect": {
    icon: PanelRight,
    label: "shell.inspect",
    keywords: ["inspector", "aside", "panel"],
    scope: "shell",
    binding: null,
    run: _inspect,
  },
} as const

declare namespace Shell {
  type Shape = {
    readonly Persisted: typeof _Persisted
    readonly layout: typeof _LAYOUT
    readonly variants: typeof _variants
    readonly collapses: typeof _collapses
    readonly sides: typeof _sides
    readonly nests: typeof _nests
    readonly areas: typeof _AREAS
    readonly slots: typeof _SLOTS
    readonly headerSlots: typeof _HEADER
    readonly elide: typeof _ELIDE
    readonly rail: typeof _rail
    readonly stamped: typeof _stamped
    readonly current: typeof _current
    readonly crumbs: typeof _crumbs
    readonly item: typeof _item
    readonly header: typeof _header
  }
}

const Shell: Shell.Shape = {
  Persisted: _Persisted,
  layout: _LAYOUT,
  variants: _variants,
  collapses: _collapses,
  sides: _sides,
  nests: _nests,
  areas: _AREAS,
  slots: _SLOTS,
  headerSlots: _HEADER,
  elide: _ELIDE,
  rail: _rail,
  stamped: _stamped,
  current: _current,
  crumbs: _crumbs,
  item: _item,
  header: _header,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Shell }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
