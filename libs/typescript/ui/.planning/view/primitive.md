# [UI_PRIMITIVE]

`view/primitive.ts` is the headless component spine: react-aria-components is ONE render-props + context + slot pattern instanced ~65 ways, and this module owns how the folder rides it — the `styled` recipe factory fusing a `cva` variant table with RAC's render-state through `composeRenderProps` and the one `cn` rail, the roster law (which library owns which primitive class), the toast queue + live-region announce rows, and the failure envelope (`react-error-boundary` folding the atom's squashed tagged `E`). Styling is state-as-data: RAC emits `data-*` attributes, the `tailwindcss-react-aria-components` variants read them, and a `className` function survives only where a variant cannot express the state.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]          | [OWNS]                                                                          |
| :-----: | :----------------- | :-------------------------------------------------------------------------------- |
|   [1]   | `STYLED_SPINE`     | `Primitive.styled` — the cva × composeRenderProps × cn recipe factory              |
|   [2]   | `ROSTER_LAW`       | the component-family ownership table and the slot/context composition rules        |
|   [3]   | `TOAST_ANNOUNCE`   | the toast queue owner and the imperative live-region rail                          |
|   [4]   | `FAILURE_ENVELOPE` | the error-boundary row — Suspense + boundary as the whole async-failure rail       |

## [2]-[STYLED_SPINE]

- Owner: `Primitive.styled(recipe)` — the one styled-atom factory: takes a `cva` recipe (base + variant axes + `defaultVariants` + `compoundVariants`) and returns the `className` composer a RAC component consumes — `composeRenderProps` layers the recipe's selected classes under the caller's own `className` (value or render function), `cn` resolves conflicts last-wins, and `VariantProps<typeof recipe>` lifts the axis union into the component's prop type intersected with `ComponentProps` of the wrapped element.
- Packages: `class-variance-authority` (`cva`, `VariantProps`), `react-aria-components` (`composeRenderProps` — the exact layering idiom), `token/theme` (`cn` — the only composer; a bare `clsx`/`twMerge` import here is the named defect).
- Law: state styles as variants first — `selected:bg-accent pressed:scale-95` class strings over the tw-rac `data-*` mappings; the `className` FUNCTION form is reached only for state no variant expresses, and the recipe stays a static analyzable string table either way.
- Law: prop types lift, never restate — a styled row's props are `ComponentPropsWithRef<typeof Target> & VariantProps<typeof recipe>`; a hand-authored prop interface beside a wrapped component marks the extractor family unused.
- Law: element override is `render` on the aria spine and `Slot`/`asChild` on the non-aria plane — a RAC component swaps its DOM element through the `render` prop; a radix-based atom polymorphs through `createSlot(ownerName)`; the two mechanisms never stack on one node.
- Growth: a new styled atom is one recipe row plus one wrapped component; a new visual axis is one variant row in the recipe — never a second class-composition mechanism.

```typescript
import { cva, type VariantProps } from "class-variance-authority"
import type { ClassValue } from "clsx"
import { composeRenderProps } from "react-aria-components"
import { cn } from "../token/theme.ts"

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

- Law: the RAC families are seed data on one pattern — actions, collections, pickers, overlays, fields, toggles, date/time, color, structure, interaction — and a `view` row COMPOSES the pattern (`Xxx`/`XxxContext`/`XxxStateContext` triple), never memorizes per-component APIs; a standard widget takes the RAC component, and a raw `use<Widget>` + `use<Widget>State` pair is reached only for a bespoke DOM structure RAC does not ship.
- Law: one semantic owner per primitive class — accessible collection = RAC `Table`/`Virtualizer`; heavy data-grid modeling = TanStack (`view/compose`); anchored non-aria positioning = floating-ui; drag sheet = vaul; in-field filtering = RAC `Autocomplete`; global palette = cmdk; a radix `Label`/`Separator`/`VisuallyHidden` never enters an aria field where RAC owns the part.
- Law: compound composition reads state contexts — `ListStateContext`, `OverlayTriggerStateContext`, `SelectStateContext` expose the live `<P>State` to descendants; prop-drilling widget state or an ad-hoc `useState` beside a modeled state is the named defect, and cross-slot prop injection collapses nested provider towers through the single `Provider` `values` array.
- Law: controlled props bind to atoms — `selectedKeys`/`value`/`isOpen`/`sortDescriptor`/`expandedKeys` read `useAtomValue` and write `useAtomSet`; RAC runs controlled wherever the app owns the state, and `useListData`/`useAsyncList` survive only for RAC-native ephemeral collections no atom owns.
- Law: interaction-local state stays in the widget — react-stately holds the open overlay, the focused key, uncommitted segments; domain truth lives in the atom; the seam is the controlled-prop pair and never a mirror.
- Boundary: the field/validation composition, pickers, palette, grid, anchors, and sheets are `view/compose`'s rows; discrete interaction hooks are `act/gesture`'s; locale infra (`I18nProvider`, `useFilter`) is `intl/format`'s.

## [4]-[TOAST_ANNOUNCE]

- Owner: `Primitive.toasts` — one module-level `UNSTABLE_ToastQueue<Note>` (react-stately's queue; pre-stable marker carried on the card, not hidden) with `maxVisibleToasts` policy; the region row renders `UNSTABLE_ToastRegion`/`UNSTABLE_Toast*` over the queue with `Motion.toast` enter/exit variants, and the queue's `add`/`close` are the only imperative surface — an Effect flow enqueues through `Effect.sync(() => Primitive.toasts.add(note, { timeout }))` at its consuming boundary.
- Owner: the announce rail — `announce(message, assertiveness?, timeout?)` from `@react-aria/live-announcer`: `"assertive"` interrupts (faults, blocking status), `"polite"` waits (progress, counts); one global visually-hidden region, `destroyAnnouncer()` on host teardown; a bespoke `aria-live` div or a second announcer region is the named defect, and an element-scoped SR-only label uses `VISUALLY_HIDDEN_STYLES`/`VisuallyHidden` instead.
- Law: a toast NOTE is data — `{ key, tone, message }` where `message` is an `intl/message` catalog key resolved at render, so toasts localize like every other string; the tone vocabulary maps to recipe variants.
- Law: visual toast and SR announcement are one act — the region carries its built-in live region, so `toasts.add` alone reaches assistive tech; a separate `announce` call per toast double-speaks.
- Boundary: which flows toast is app policy; the `Result`-failure path routes through the boundary row below, not through toasts, unless the failure is non-blocking evidence.

```typescript
import { UNSTABLE_ToastQueue } from "react-aria-components"

type Note = { readonly key: string; readonly tone: "neutral" | "success" | "danger"; readonly message: string }

const _toasts = new UNSTABLE_ToastQueue<Note>({ maxVisibleToasts: 4 })
```

## [5]-[FAILURE_ENVELOPE]

- Owner: `Primitive.boundary` — the one error-boundary row: `ErrorBoundary` with the `fallbackRender` arm (the discriminated prop union's render-prop member), `resetKeys` bound to the failing atom's input so a new query clears a stale error, `onReset` re-running the failed atom through `useAtomRefresh`, and `onError` as the app-wired observation prop (telemetry is not a `ui` edge — the app composes the sink).
- Law: the async-failure rail is Suspense plus this boundary — `useAtomSuspense` suspends `waiting` to the nearest `<Suspense>` and throws `Cause.squash(cause)` (the squashed tagged `E`) on `Failure`, so `FallbackProps.error` IS the tagged fault and the fallback folds it with `Match.tagsExhaustive` into localized problem rows; a carrier-less throw (event handler, raw promise) escalates through `useErrorBoundary().showBoundary(Cause.squash(cause))` so both paths land the same shape.
- Law: every route/panel/atom-bound subtree wraps once — boundary granularity is the recovery granularity; a per-component `try`/`catch`, an `instanceof` ladder in a fallback, or a `componentDidCatch` class is the named defect.
- Law: the fold's `error as E` is the platform's type-erasure seam — React strips the thrown type, `FallbackProps.error` arrives `unknown`, and the boundary re-asserts exactly the squashed tagged `E` the atom rail threw; this card carries the cast exemption, and the assertion is legal nowhere else in the module.
- Law: the root callbacks frame the boundary — `onCaughtError`/`onUncaughtError` on `createRoot` are `browser`'s boot options observing what boundaries caught and what escaped; this row owns only the in-tree envelope.
- Growth: a new fault presentation is one `Match` arm in the fallback fold keyed by the family tag; a new recovery affordance rides `resetErrorBoundary`.

```typescript
import type { ReactNode } from "react"
import type { ErrorBoundaryProps, FallbackProps } from "react-error-boundary"

declare namespace Boundary {
  type Fold<E> = (error: E, reset: FallbackProps["resetErrorBoundary"]) => ReactNode
  type Props<E> = {
    readonly fold: Fold<E>
    readonly resetKeys?: ReadonlyArray<unknown>
    readonly onReset?: ErrorBoundaryProps["onReset"]
    readonly onError?: ErrorBoundaryProps["onError"]
  }
}

const _fallbackRender = <E,>(fold: Boundary.Fold<E>): ((props: FallbackProps) => ReactNode) =>
  ({ error, resetErrorBoundary }) => fold(error as E, resetErrorBoundary)

const Primitive: {
  readonly styled: typeof _styled
  readonly recipes: { readonly button: typeof _button }
  readonly toasts: typeof _toasts
  readonly boundary: typeof _fallbackRender
} = {
  styled: _styled,
  recipes: { button: _button },
  toasts: _toasts,
  boundary: _fallbackRender,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Primitive }
```
