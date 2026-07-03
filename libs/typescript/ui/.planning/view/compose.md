# [UI_COMPOSE]

`view/compose.ts` is the composition plane where the spine's primitives assemble into working surfaces: the Schema→aria form binding (one kernel `Schema` owning wire decode AND live field validity), the command palette (a `CommandSpec` vocabulary over cmdk's machine), the headless grid (TanStack modeling + react-aria semantics + virtual windowing, all folded through one table-state atom), the anchored-overlay and drag-sheet hosts (floating-ui and vaul as disjoint overlay classes), the presence-cursor cohort (virtual-element anchoring over a live `state` feed), and the sanitize gate every HTML-bearing string passes before a DOM sink. Each cluster is one owner whose growth is a row — a spec, a column, a snap point — never a sibling component.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                                      |
| :-----: | :---------------- | :--------------------------------------------------------------------------- |
|   [1]   | `FORM_BINDING`    | the Schema→aria validation seam and the submit round-trip                     |
|   [2]   | `COMMAND_PALETTE` | the `CommandSpec` vocabulary and the cmdk hosting law                         |
|   [3]   | `GRID_COLLECTION` | the one-atom table fold, aria-grid semantics, and virtual windowing           |
|   [4]   | `ANCHOR_SHEET`    | the floating-anchor owner, the vaul sheet owner, and the overlay-class law    |
|   [5]   | `PRESENCE_COHORT` | live presence cursors over virtual-element anchoring                          |
|   [6]   | `SANITIZE_GATE`   | the one DOMPurify gate before any `dangerouslySetInnerHTML` sink              |

## [2]-[FORM_BINDING]

- Owner: `Compose.form` — the Schema→aria binding: one kernel field `Schema` projects through `Schema.standardSchemaV1` (the package surface used directly — a forwarding wrapper is the named defect) into the standard-schema validator RAC fields consume, decode failures land in `FormValidationContext` keyed by field name (server faults and live validation share one error shape), `validationBehavior: "aria"` marks invalid via ARIA without blocking native submit, and `FieldError` renders the `ValidationResult`; the tw-rac `invalid:`/`required:` variants style validity with zero JS branching.
- Packages: `effect` `Schema` (`standardSchemaV1`, `decodeUnknownEither`, `ArrayFormatter` at the terminal reporting edge only), `react-aria-components` (`Form`, `TextField`, `NumberField`, `FieldError`, `FormValidationContext`), `react-dom` (`useFormStatus` for submit pending, `requestFormReset` after a successful action).
- Law: one Schema, both duties — the same owner that decodes the wire payload validates the live field; a parallel validator, a regex beside a brand, or a hand `errors` record is the named defect.
- Law: submit awaits the store — the action writes through `useAtomSet(mutation, { mode: "promise" })` inside `startTransition`; pending state reads `useFormStatus`, refusal reconciles the optimistic write, and the fault set projects into `FormValidationContext` by field path from the `ParseError` tree.
- Law: field-grain re-render rides `AtomRef` — a large form draft is one atom, each field a `useAtomRefProp(ref, key)` cursor so an edited field re-renders alone (`atom/derive`'s cursor law composed here).
- Growth: a new field kind is one RAC field row bound to its schema field; a new form is a schema plus rows — never a form library.

```typescript
import { Array, Either, ParseResult, Record, Schema } from "effect"

declare namespace Form {
  type Errors = Readonly<Record<string, ReadonlyArray<string>>>
}

const _errors = <A, I>(schema: Schema.Schema<A, I>) =>
  (raw: I): Form.Errors =>
    Either.match(Schema.decodeUnknownEither(schema, { errors: "all" })(raw), {
      onRight: () => Record.empty<string, ReadonlyArray<string>>(),
      onLeft: (fault) =>
        Record.map(
          Array.groupBy(ParseResult.ArrayFormatter.formatErrorSync(fault), (issue) => issue.path.join(".")),
          Array.map((issue) => issue.message),
        ),
    })
```

## [3]-[COMMAND_PALETTE]

- Owner: the `CommandSpec` vocabulary riding `Compose`: one `as const satisfies Record<string, CommandSpec>` table where each row carries `icon` (a named `LucideIcon` — the row's identity, tree-shaken), `label` (an `intl/message` catalog key), `keywords` (alias tokens for the scorer), and `run` (the intent write — an atom setter or callable atom the app wires); the palette renders the table through cmdk with controlled `value`/`onValueChange`, `useCommandState((s) => s.filtered.count)` driving the count/empty rows without list re-render — the type never shadows cmdk's own `Command` binding.
- Packages: `cmdk` (`Command` compound, `useCommandState`, `defaultFilter`), `lucide-react` (`LucideIcon` — icon-as-identity), `intl/message` (labels), `intl/format` (`useFilter` pre-normalization where locale-sensitive).
- Law: hosting picks exactly one shell — `CommandDialog` for the global modal palette (its own Radix portal + focus trap); a BARE `Command` inside `Drawer.Content` for a sheet palette (vaul owns portal/trap); a bare `Command` inside `FloatingFocusManager` for an anchored palette (floating-ui owns position/portal/trap) — two focus traps on one surface is the named defect, and `useListNavigation`/`useTypeahead` never stack over a cmdk list.
- Law: item `value` is the stable spec key, never the visible label — filtering and selection survive label localization; `keywords` carry the localized aliases.
- Law: async command sources set `shouldFilter={false}` and render pre-filtered rows from an atom (`Atom.debounce`d query, `Result`-folded) — the machine keeps keyboard/selection, the store owns matching.
- Growth: a new command is one spec row; a new palette surface is one hosting-shell choice — the table never forks.

```typescript
import type { LucideIcon } from "lucide-react"

type CommandSpec = {
  readonly icon: LucideIcon
  readonly label: string
  readonly keywords: ReadonlyArray<string>
  readonly run: () => void
}

declare const _specs: Record<string, CommandSpec>
```

## [4]-[GRID_COLLECTION]

- Owner: `Compose.grid` — the headless grid fold: ONE atom holds the whole TanStack `TableState` (sorting, filters, selection, pagination, sizing), `useReactTable` reads it as controlled `state` with `onStateChange` writing through `useAtomSet` (the exact `Updater` shape `makeStateUpdater` builds), columns type against the wire-decoded row Schema via `createColumnHelper<Row>()`, and only the used row models import (`getCoreRowModel` always; sort/filter/facet models on demand).
- Law: semantics and modeling split by owner — TanStack derives (rows, headers, facets, `flexRender` the only bridge); react-aria supplies the `grid`/`row`/`columnheader`/`gridcell` roles and roving keyboard over the rendered markup; `aria-rowcount`/`aria-rowindex` stay on the FULL logical count while windowing mounts the visible span.
- Law: windowing is `useVirtualizer` — `count` from the row model, `estimateSize` + `measureElement` for variable rows, `getTotalSize()` sizing the spacer, `rangeExtractor` unioning pinned/sticky indices; a selection echo scrolls through `scrollToIndex(index, { align: "center" })` when the `GlobalId` selection atom changes (`viewer/mark/selection`'s sync row consumes this).
- Law: `RowSelectionState` keys are `GlobalId` strings where the grid fronts model elements — the table's selection slice and the viewer's selection set are ONE atom projected two ways, never two stores reconciled.
- Law: RAC `Table` remains the owner for interactive accessible collections WITHOUT heavy derivation — the TanStack fold is earned by faceting/grouping/virtual scale; one collection never runs both engines.
- Growth: a new column is one `columnHelper` row; a new derived behavior is one row-model import; a new state slice persists by backing the atom with `Atom.kvs` and its owning schema.

```typescript
import type { RowSelectionState, SortingState, Updater } from "@tanstack/react-table"
import { functionalUpdate } from "@tanstack/react-table"

declare namespace Grid {
  type Slice = { readonly sorting: SortingState; readonly rowSelection: RowSelectionState }
}

const _apply = <K extends keyof Grid.Slice>(key: K) =>
  (state: Grid.Slice, updater: Updater<Grid.Slice[K]>): Grid.Slice =>
    ({ ...state, [key]: functionalUpdate(updater, state[key]) })
```

## [5]-[ANCHOR_SHEET]

- Owner: `Compose.anchor` — the floating-anchor host: `useFloating` with `whileElementsMounted: autoUpdate` and the canonical `offset → flip → shift → size` middleware pipeline; interactions merge through `useInteractions([useClick, useDismiss, useRole])` into the three prop-getters; `FloatingFocusManager` (modal for dialogs, non-modal + `preserveTabOrder` for menus) and `FloatingPortal` complete the stack; open state binds an atom through `useFloatingRootContext({ open, onOpenChange })` so visibility is store state; enter/exit rides `useTransitionStyles` phases consuming `Scale.ease` values.
- Owner: the sheet host law riding `Compose` — the vaul host: `Drawer.Root` with `open`/`onOpenChange` and `activeSnapPoint`/`setActiveSnapPoint` atom-bound, `snapPoints` + `fadeFromIndex` as the detent policy row, `Drawer.Title`+`Drawer.Description` always present (visually hidden when no heading shows), `handleOnly` for drag-origin discipline; the sheet's drag is vaul's own — no `use-gesture` binding on a sheet surface.
- Law: the overlay-class division is absolute — anchored overlays ride floating-ui's stack; the drag-dismissable sheet rides vaul's bundled Radix stack; RAC `Popover`/`Modal` own standard aria overlays; exactly one semantic owner and one positioner per element, with react-aria's `mergeProps` + floating-ui's `useMergeRefs` folding the two prop/ref systems where aria semantics meet floating geometry.
- Law: nested floats needing dismissal coordination take `FloatingTree`/`FloatingNode`; flat overlays never mount the tree.
- Law: the `size.apply` style write is floating-ui's platform-forced statement seam — the middleware hands a live element and the write is the documented application; this card carries the exemption.
- Growth: a new anchored surface is one hook composition over the same pipeline; a new sheet detent is one `snapPoints` entry.

```typescript
import { autoUpdate, flip, offset, shift, size } from "@floating-ui/react"
import type { Placement } from "@floating-ui/react"

declare namespace Anchor {
  type Options = { readonly placement: Placement; readonly gap: number; readonly floor: number }
}

const _middleware = (options: Anchor.Options) => [
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

## [6]-[PRESENCE_COHORT]

- Owner: the presence-cohort law riding `Compose` — the collaborative-cursor cohort: a live presence feed (a `state` fold of peer positions, entering as an atom through `atom/derive`'s bridge) renders one cursor row per peer, each anchored by a floating-ui `VirtualElement` whose `getBoundingClientRect` projects the peer's world coordinate through the owning surface's projection (`viewer/geo/project`'s `project` seam for map surfaces, plain viewport coordinates elsewhere); cursors mount in one `FloatingPortal`, motion rides `Motion.panel`, and idle peers age out by the feed's own TTL fold — never a local timer per cursor.
- Law: presence is render-only — cursors never intercept pointer events (`pointer-events: none` in the recipe) and carry no focus semantics; the cohort is a projection of state, not an interaction surface.
- Law: per-peer identity is keyed by the feed's peer id; label/color derive from the peer row through the theme ramp — no per-peer style state in the component.
- Boundary: the presence feed's shape and TTL policy live in `state`; the projection function arrives as a parameter so the cohort is surface-agnostic.

## [7]-[SANITIZE_GATE]

- Owner: `Compose.sanitize` — the one DOMPurify gate: `setConfig` pins the project allow-list once at module init (`USE_PROFILES { html: true }`, no inline event attributes ever), `sanitize(dirty)` is the only path to `dangerouslySetInnerHTML`, and the `removed` diagnostic feeds an evidence row when a strip occurs; rich-text catalog messages (`intl/message`) and any wire-borne HTML band pass here — a raw string reaching a DOM sink is the named defect.
- Law: the gate is isomorphic — the same policy sanitizes server-prerendered and client-hydrated strings, so hydration never diverges on sanitizer output.
- Law: config is global and singular — a per-call config drift is rejected; a surface needing a stricter profile passes an explicit `Config` at ITS call while the global stays the floor.

```typescript
import DOMPurify from "isomorphic-dompurify"

DOMPurify.setConfig({ USE_PROFILES: { html: true }, FORBID_ATTR: ["style"] })

const _sanitize = (dirty: string): string => DOMPurify.sanitize(dirty)

const Compose: {
  readonly form: { readonly errors: typeof _errors }
  readonly grid: { readonly apply: typeof _apply }
  readonly anchor: { readonly middleware: typeof _middleware }
  readonly sanitize: typeof _sanitize
} = {
  form: { errors: _errors },
  grid: { apply: _apply },
  anchor: { middleware: _middleware },
  sanitize: _sanitize,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Compose }
```
