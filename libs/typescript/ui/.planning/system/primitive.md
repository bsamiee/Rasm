# [UI_PRIMITIVE]

The headless component spine: react-aria-components is ONE render-props + context + slot pattern instanced across every family, and this module owns how the folder rides it — the `styled` recipe factory fusing a `cva` variant table with RAC's render-state through `composeRenderProps` and the one `cn` rail, the roster law (which library owns which primitive class, radix included), the toast queue + live-region announce rows, the failure envelope (`react-error-boundary` folding the atom's squashed tagged `E`), and the one sanitize gate every HTML-bearing string passes before a DOM sink. Styling is state-as-data: RAC emits `data-*` attributes, the `tailwindcss-react-aria-components` variants read them, and a `className` function survives only where a variant cannot express the state. The module is `ui/src/system/primitive.ts`.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]          | [OWNS]                                                                      | [PUBLIC]    |
| :-----: | :----------------- | :---------------------------------------------------------------------------- | :---------- |
|  [01]   | `STYLED_SPINE`     | `Primitive.styled` — the cva × composeRenderProps × cn recipe factory          | `Primitive` |
|  [02]   | `ROSTER_LAW`       | the component-family ownership table and the slot/context composition rules    | —           |
|  [03]   | `TOAST_ANNOUNCE`   | the toast queue owner and the imperative live-region rail                      | `Primitive` |
|  [04]   | `FAILURE_ENVELOPE` | the error-boundary row — Suspense + boundary as the whole async-failure rail   | `Primitive` |
|  [05]   | `SANITIZE_GATE`    | the one DOMPurify gate before any `dangerouslySetInnerHTML` sink               | `Primitive` |

## [2]-[STYLED_SPINE]

[STYLED_SPINE]:
- Owner: `Primitive.styled(recipe)` — the one styled-atom factory: takes a `cva` recipe (base + variant axes + `defaultVariants` + `compoundVariants`) and returns the `className` composer a RAC component consumes — `composeRenderProps` layers the recipe's selected classes under the caller's own `className` (value or render function), `cn` resolves conflicts last-wins, and `VariantProps<typeof recipe>` lifts the axis union into the component's prop type intersected with `ComponentProps` of the wrapped element.
- Packages: `class-variance-authority` (`cva`, `VariantProps`); `react-aria-components` (`composeRenderProps` — the exact layering idiom); `system/token` (`cn` — the only composer; a bare `clsx`/`twMerge` import here is the named defect).
- Law: state styles as variants first — `selected:bg-accent pressed:scale-95` class strings over the tw-rac `data-*` mappings; the `className` FUNCTION form is reached only for state no variant expresses, and the recipe stays a static analyzable string table either way.
- Law: prop types lift, never restate — a styled row's props are `ComponentPropsWithRef<typeof Target> & VariantProps<typeof recipe>`; a hand-authored prop interface beside a wrapped component marks the extractor family unused.
- Law: element override is `render` on the aria spine and `Slot`/`asChild` on the non-aria plane — a RAC component swaps its DOM element through the `render` prop; a radix-based atom polymorphs through `createSlot(ownerName)`; the two mechanisms never stack on one node.
- Growth: a new styled atom is one recipe row plus one wrapped component; a new visual axis is one variant row in the recipe — never a second class-composition mechanism.

```typescript
import { cva, type VariantProps } from "class-variance-authority"
import type { ClassValue } from "clsx"
import { composeRenderProps } from "react-aria-components"
import { cn } from "./token.ts"

declare namespace Primitive {
  type Recipe = ReturnType<typeof cva>
  type Variants<R extends Recipe> = VariantProps<R>
  type ClassName<S> = string | ((state: S & { readonly defaultClassName: string | undefined }) => string) | undefined
}

const _styled = <R extends Primitive.Recipe, S>(recipe: R) =>
  (variants: Primitive.Variants<R>, className: Primitive.ClassName<S>): ((state: S & { readonly defaultClassName: string | undefined }) => string) =>
    composeRenderProps(className, (own: ClassValue) => cn(recipe(variants), own))

const _button = cva("inline-flex items-center gap-2 rounded-md outline-none focus-visible:ring-2", {
  variants: {
    tone: { neutral: "bg-surface text-fg", accent: "bg-accent text-accent-fg", danger: "bg-danger text-danger-fg" },
    size: { sm: "h-8 px-2 text-sm", md: "h-10 px-3 text-base" },
  },
  defaultVariants: { tone: "neutral", size: "md" },
  compoundVariants: [{ tone: "danger", size: "sm", class: "font-medium" }],
})
```

## [3]-[ROSTER_LAW]

[ROSTER_LAW]:
- Law: the RAC families are seed data on one pattern — actions (`ToggleButtonGroup`/`Toolbar`), collections (`GridList`/`ListBox`/`Menu`/`Tree`/`TagGroup`/`Tabs`/`Breadcrumbs`), pickers (`ComboBox`/`SearchField`/`Autocomplete`), fields, toggles, gauges (`Slider`/`Meter`/`ProgressBar`), date/time (`Calendar`/`RangeCalendar`/`DateField`/`TimeField`/`DatePicker`/`DateRangePicker` over `@internationalized/date` `DateValue`), color (`ColorPicker`/`ColorWheel`/`ColorArea`/`ColorSlider`/`ColorField`), structure (`Disclosure`/`DisclosureGroup`), and interaction — and a view row COMPOSES the pattern (`Xxx`/`XxxContext`/`XxxStateContext` triple), never memorizes per-component APIs; a standard widget takes the RAC component, and a raw `use<Widget>` + `use<Widget>State` pair is reached only for a bespoke DOM structure RAC does not ship.
- Law: one semantic owner per primitive class — accessible collection = RAC `Table`/`Virtualizer`; heavy data-grid modeling = TanStack (`view/table`); anchored non-aria positioning = floating-ui (`view/overlay`); drag sheet = vaul (`view/overlay`); in-field filtering = RAC `Autocomplete`; global palette = cmdk (`view/overlay`); a radix `Label`/`Separator`/`VisuallyHidden` never enters an aria field where RAC owns the part.
- Law: collection reordering is `useDragAndDrop` — RAC's own drag-and-drop hooks with `DropIndicator` rows and `isTextDropItem` refinement at the external-payload seam; a pointer-sequence reorder hand-rolled over `system/act` continuous gestures on a RAC collection is the double-bind defect.
- Law: compound composition reads state contexts — `ListStateContext`, `OverlayTriggerStateContext`, `SelectStateContext` expose the live `<P>State` to descendants; prop-drilling widget state or an ad-hoc `useState` beside a modeled state is the named defect, and cross-slot prop injection collapses nested provider towers through the single `Provider` `values` array.
- Law: controlled props bind to atoms — `selectedKeys`/`value`/`isOpen`/`sortDescriptor`/`expandedKeys` read `useAtomValue` and write `useAtomSet`; RAC runs controlled wherever the app owns the state, and `useListData`/`useAsyncList` survive only for RAC-native ephemeral collections no atom owns.
- Law: interaction-local state stays in the widget — react-stately holds the open overlay, the focused key, uncommitted segments; domain truth lives in the atom; the seam is the controlled-prop pair and never a mirror.
- Law: date/time and color interiors stay foreign at the seam — `DateValue` segments and RAC's color state are widget-interior currency; a committed value crosses to the domain as the owning kernel scalar (`DateTime.Utc` through `system/intl`'s epoch seam, `Theme.Color` through its decode) at the controlled-prop boundary, never stored foreign.
- Boundary: field/validation composition is `view/form`'s; grid modeling is `view/table`'s; palette, anchors, and sheets are `view/overlay`'s; discrete interaction hooks are `system/act`'s; locale infra (`I18nProvider`, `useFilter`) is `system/intl`'s.

## [4]-[TOAST_ANNOUNCE]

[TOAST_ANNOUNCE]:
- Owner: `Primitive.toasts` — one module-level `UNSTABLE_ToastQueue<Note>` (react-stately's queue; pre-stable marker carried on this card, not hidden) with `maxVisibleToasts` policy; the region row renders `UNSTABLE_ToastRegion`/`UNSTABLE_Toast*` over the queue with `Motion.toast` enter/exit variants, and the queue's `add`/`close` are the only imperative surface — an Effect flow enqueues through `Effect.sync(() => Primitive.toasts.add(note, { timeout }))` at its consuming boundary.
- Owner: the announce rail — `announce(message, assertiveness?, timeout?)` from `@react-aria/live-announcer`: `"assertive"` interrupts (faults, blocking status), `"polite"` waits (progress, counts); one global visually-hidden region, `destroyAnnouncer()` on host teardown; a bespoke `aria-live` div or a second announcer region is the named defect, and an element-scoped SR-only label uses `VisuallyHidden` instead.
- Law: a toast NOTE is data — `{ key, tone, message }` where `message` is a `system/intl` catalog key resolved at render, so toasts localize like every other string; the tone vocabulary maps to recipe variants.
- Law: visual toast and SR announcement are one act — the region carries its built-in live region, so `toasts.add` alone reaches assistive tech; a separate `announce` call per toast double-speaks.
- Boundary: which flows toast is app policy; the `Result`-failure path routes through the boundary row below, not through toasts, unless the failure is non-blocking evidence.

```typescript
import { UNSTABLE_ToastQueue } from "react-aria-components"

type Note = { readonly key: string; readonly tone: "neutral" | "success" | "danger"; readonly message: string }

const _toasts = new UNSTABLE_ToastQueue<Note>({ maxVisibleToasts: 4 })
```

## [5]-[FAILURE_ENVELOPE]

[FAILURE_ENVELOPE]:
- Owner: `Primitive.boundary` — the one error-boundary row: `ErrorBoundary` with the `fallbackRender` arm (the discriminated prop union's render-prop member), `resetKeys` bound to the failing atom's input so a new query clears a stale error, `onReset` re-running the failed atom through `useAtomRefresh`, and `onError` as the app-wired observation prop (telemetry is not a `ui` edge — the app composes the sink).
- Law: the async-failure rail is Suspense plus this boundary — `useAtomSuspense` suspends `waiting` to the nearest `<Suspense>` and throws `Cause.squash(cause)` (the squashed tagged `E`) on `Failure`, so `FallbackProps.error` IS the tagged fault and the fallback folds it with `Match.tagsExhaustive` into localized problem rows; a carrier-less throw (event handler, raw promise) escalates through `useErrorBoundary().showBoundary(Cause.squash(cause))` so both paths land the same shape.
- Law: every route/panel/atom-bound subtree wraps once — boundary granularity is the recovery granularity; a per-component `try`/`catch`, an `instanceof` ladder in a fallback, or a `componentDidCatch` class is the named defect.
- Law: the fold's `error as E` is the platform's type-erasure seam — React strips the thrown type, `FallbackProps.error` arrives `unknown`, and the boundary re-asserts exactly the squashed tagged `E` the atom rail threw; this card carries the cast exemption, and the assertion is legal nowhere else in the module.
- Law: the root callbacks frame the boundary — `onCaughtError`/`onUncaughtError` on `createRoot` are the browser boot options observing what boundaries caught and what escaped; this row owns only the in-tree envelope.
- Growth: a new fault presentation is one `Match` arm in the fallback fold keyed by the family tag; a new recovery affordance rides `resetErrorBoundary`.

```typescript
import type { ReactNode } from "react"
import type { FallbackProps } from "react-error-boundary"

declare namespace Boundary {
  type Fold<E> = (error: E, reset: FallbackProps["resetErrorBoundary"]) => ReactNode
}

const _fallbackRender = <E,>(fold: Boundary.Fold<E>): ((props: FallbackProps) => ReactNode) =>
  ({ error, resetErrorBoundary }) => fold(error as E, resetErrorBoundary)
```

## [6]-[SANITIZE_GATE]

[SANITIZE_GATE]:
- Owner: `Primitive.sanitize` — the one DOMPurify gate: `setConfig` pins the project allow-list once at module init (`USE_PROFILES { html: true }`, no inline event attributes ever), `sanitize(dirty)` is the only path to `dangerouslySetInnerHTML`, and the `removed` diagnostic feeds an evidence row when a strip occurs; rich-text catalog messages (`system/intl`) and any wire-borne HTML band pass here — a raw string reaching a DOM sink is the named defect.
- Packages: `isomorphic-dompurify` (`setConfig`, `sanitize` — the same module resolves server and browser).
- Law: the gate is isomorphic — the same policy sanitizes server-prerendered and client-hydrated strings, so hydration never diverges on sanitizer output.
- Law: config is global and singular — a per-call config drift is rejected; a surface needing a stricter profile passes an explicit `Config` at ITS call while the global stays the floor.

```typescript
import DOMPurify from "isomorphic-dompurify"

DOMPurify.setConfig({ USE_PROFILES: { html: true }, FORBID_ATTR: ["style"] })

const _sanitize = (dirty: string): string => DOMPurify.sanitize(dirty)

const Primitive: {
  readonly styled: typeof _styled
  readonly recipes: { readonly button: typeof _button }
  readonly toasts: typeof _toasts
  readonly boundary: typeof _fallbackRender
  readonly sanitize: typeof _sanitize
} = {
  styled: _styled,
  recipes: { button: _button },
  toasts: _toasts,
  boundary: _fallbackRender,
  sanitize: _sanitize,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Primitive }
```
