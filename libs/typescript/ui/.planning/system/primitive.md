# [UI_PRIMITIVE]

Primitive owns the headless component spine: react-aria-components instances ONE render-props + context + slot pattern across every family, and this module rides it through the `styled` recipe factory, the roster law assigning each primitive class its owning library, the announce, failure, and sanitize rails, and the clipboard capability port. Styling is state-as-data: RAC emits `data-*` attributes, the `tailwindcss-react-aria-components` variants read them, and a `className` function survives only where a variant cannot express the state. Module: `ui/src/system/primitive.ts`.

## [01]-[INDEX]

- [02]-[STYLED_SPINE]: `Primitive.styled` — the cva × composeRenderProps × cn recipe factory; `Primitive`.
- [03]-[ROSTER_LAW]: one table assigns each component family its owner, the slot/context composition rules, and the landmark and overflow-grant laws; `Primitive`.
- [04]-[TOAST_ANNOUNCE]: Primitive owns the toast queue and the imperative live-region rail; `Primitive`.
- [05]-[FAILURE_ENVELOPE]: Primitive folds the error-boundary row — Suspense + boundary as the whole async-failure rail; `Primitive`.
- [06]-[SANITIZE_GATE]: Primitive gates every HTML string through DOMPurify before any `dangerouslySetInnerHTML` sink; `Primitive`.
- [07]-[CLIPBOARD_PORT]: `Clipboard` declares the folder's clipboard capability Tag the browser composition satisfies; `Clipboard`.

## [02]-[STYLED_SPINE]

[STYLED_SPINE]:
- Owner: `Primitive.styled(recipe)` — the one styled-atom factory: takes a `cva` recipe (base + variant axes + `defaultVariants` + `compoundVariants`) and returns the `className` composer a RAC component consumes — `composeRenderProps` layers the recipe's selected classes under the caller's own `className` (value or render function), `cn` resolves conflicts last-wins, and `VariantProps<typeof recipe>` lifts the axis union into the component's prop type intersected with `ComponentProps` of the wrapped element.
- Packages: `class-variance-authority` (`cva`, `VariantProps`); `react-aria-components` (`composeRenderProps` — the exact layering idiom); `@radix-ui/react-slot` (`createSlot` — the non-aria polymorph mechanism the element-override law below names); `system/token` (`cn` — the only composer; a bare `clsx`/`twMerge` import here is the named defect).
- Law: state styles as variants first — `selected:bg-accent-solid pressed:scale-95` class strings over the tw-rac `data-*` mappings; the `className` FUNCTION form is reached only for state no variant expresses, and the recipe stays a static analyzable string table either way.
- Law: a tone axis DERIVES from `system/token#TONE_VOCABULARY`'s roster and names only the generated slot utilities — a recipe enumerating three tones freezes the vocabulary at whichever subset its author needed, and a raw hue, hex, or unslotted color class in a recipe is the palette fork the token authority exists to hold.
- Law: prop types lift, never restate — a styled row's props are `ComponentPropsWithRef<typeof Target> & VariantProps<typeof recipe>`; a hand-authored prop interface beside a wrapped component marks the extractor family unused.
- Law: element override is `render` on the aria spine and `Slot`/`asChild` on the non-aria plane — a RAC component swaps its DOM element through the `render` prop; a radix-based atom polymorphs through `createSlot(ownerName)`; the two mechanisms never stack on one node.
- Law: the skip link is the shell's FIRST focusable — `_skip` styles the visually-hidden-until-focused anchor, and `view/shell#SCAFFOLD` renders it ahead of every region and owns the main-region id its href names; a shell without it strands keyboard users behind the whole chrome, and a second skip target per document is the named defect.
- Law: recipes never disable forced-colors adjustment — `forced-color-adjust-none` is admitted only where a semantic color IS the information (a tone dot, a diff ink), and that recipe row declares a system-color fallback beside it, so Windows High Contrast keeps the signal without keeping the palette.
- Growth: a new styled atom is one recipe row and one wrapped component; a new visual axis is one variant row in the recipe — never a second class-composition mechanism.

```typescript signature
import { cva, type VariantProps } from "class-variance-authority"
import type { ClassValue } from "clsx"
import { Record } from "effect"
import { composeRenderProps } from "react-aria-components"
import { cn, Theme } from "./token.ts"

declare namespace Primitive {
  type Recipe = ReturnType<typeof cva>
  type Variants<R extends Recipe> = VariantProps<R>
  type ClassName<S> = string | ((state: S & { readonly defaultClassName: string | undefined }) => string) | undefined
}

const _styled = <R extends Primitive.Recipe, S>(recipe: R) =>
  (variants: Primitive.Variants<R>, className: Primitive.ClassName<S>): ((state: S & { readonly defaultClassName: string | undefined }) => string) =>
    composeRenderProps(className, (own: ClassValue) => cn(recipe(variants), own))

// _filled DERIVES the tone axis from the token roster: a new semantic is a variant on every recipe with zero recipe edits,
// and the class strings name the generated slot utilities rather than re-spelling a palette this module does not own
const _filled = Record.map(Theme.Palette.rows, (_row, tone) => `bg-${tone}-solid text-${tone}-on`) // Record.map preserves the literal tone union; fromEntries widens keys through NonLiteralKey and VariantProps would type tone as string

const _button = cva("inline-flex items-center gap-2 rounded-md outline-none focus-visible:ring-2", {
  variants: {
    tone: _filled,
    size: { sm: "h-8 px-2 text-sm", md: "h-10 px-3 text-base" },
  },
  defaultVariants: { tone: "neutral", size: "md" },
  compoundVariants: [{ tone: "danger", size: "sm", class: "font-medium" }],
})

// sr-only until keyboard focus lifts it into the flow at the top-start corner, ranked on the token z ladder so it
// clears every mounted surface; activation jumps the whole chrome to the scaffold's one main region
const _skip = cva(
  "sr-only focus-visible:not-sr-only focus-visible:fixed focus-visible:start-4 focus-visible:top-4 focus-visible:z-toast focus-visible:rounded-md focus-visible:bg-accent-solid focus-visible:px-3 focus-visible:py-2 focus-visible:text-accent-on focus-visible:outline-none focus-visible:ring-2",
)
```

## [03]-[ROSTER_LAW]

[ROSTER_LAW]:
- Law: the RAC families are seed data on one pattern — actions (`ToggleButtonGroup`/`Toolbar`), collections (`GridList`/`ListBox`/`Menu`/`Tree`/`TagGroup`/`Tabs`/`Breadcrumbs`), pickers (`ComboBox`/`SearchField`/`Autocomplete`), fields, toggles, gauges (`Slider`/`Meter`/`ProgressBar`), date/time (`Calendar`/`RangeCalendar`/`DateField`/`TimeField`/`DatePicker`/`DateRangePicker` over `@internationalized/date` `DateValue`), color (`ColorPicker`/`ColorWheel`/`ColorArea`/`ColorSlider`/`ColorField`), structure (`Disclosure`/`DisclosureGroup`), and interaction — and a view row COMPOSES the pattern (`Xxx`/`XxxContext`/`XxxStateContext` triple), never memorizes per-component APIs; a standard widget takes the RAC component, and a raw `use<Widget>` + `use<Widget>State` pair is reached only for a bespoke DOM structure RAC does not ship.
- Law: one semantic owner per primitive class — accessible collection = RAC `Table`/`Virtualizer`; heavy data-grid modeling = TanStack (`view/table`); anchored non-aria positioning = floating-ui (`view/overlay`); drag sheet = vaul (`view/overlay`); in-field filtering = RAC `Autocomplete`; global palette = cmdk (`view/overlay`); RAC owns the label and separator parts outright — every field label and every divider inside an aria structure is RAC `Label`/`Separator`, so no second implementation of either part exists to reach for — and the radix plane is admitted only where no aria part answers, as `createSlot` polymorphism and an element-scoped SR-only label.
- Law: collection reordering is `useDragAndDrop` — RAC's own drag-and-drop hooks with `DropIndicator` rows and `isTextDropItem` refinement at the external-payload seam; a pointer-sequence reorder hand-rolled over `system/act` continuous gestures on a RAC collection is the double-bind defect.
- Law: single-tab-stop roving containers split by semantics — an aria toolbar is RAC `Toolbar`; a NON-aria roving container (a floating chip rail, a grid of swatches inside a float) is floating-ui `Composite`/`CompositeItem` (`orientation`/`loop`/`cols`/`activeIndex`) without mounting the full float stack — one roving engine per container, chosen by whether RAC owns the part.
- Law: compound composition reads state contexts — `ListStateContext`, `OverlayTriggerStateContext`, `SelectStateContext` expose the live `<P>State` to descendants; prop-drilling widget state or an ad-hoc `useState` beside a modeled state is the named defect, and cross-slot prop injection collapses nested provider towers through the single `Provider` `values` array.
- Law: controlled props bind to atoms — `selectedKeys`/`value`/`isOpen`/`sortDescriptor`/`expandedKeys` read `useAtomValue` and write `useAtomSet`; RAC runs controlled wherever the app owns the state, and `useListData`/`useAsyncList` survive only for RAC-native ephemeral collections no atom owns.
- Law: interaction-local state stays in the widget — react-stately holds the open overlay, the focused key, uncommitted segments; domain truth lives in the atom; the seam is the controlled-prop pair and never a mirror.
- Law: date/time and color interiors stay foreign at the seam — `DateValue` segments and RAC's color state are widget-interior currency; a committed value crosses to the domain as the owning kernel scalar (`DateTime.Utc` through `system/intl`'s epoch seam, `Theme.Color` through its decode) at the controlled-prop boundary, never stored foreign.
- Law: landmarks are structural, never decorative — one `<main>` per document at the scaffold's content region, and nav, aside, header, and footer land as native elements where `view/shell#SCAFFOLD` places them; a div wearing a landmark role where the native element exists is the named defect, and the banner-tier status landmark is `view/status#POSTURE_FOLD`'s labelled section, never a second grammar here.
- Law: a scroll container EARNS `role="region"` + `tabIndex={0}` by measured overflow — `Primitive.useOverflow` grants the pair only while content overflows its box and only under a label, so a fitting container exposes no phantom tab stop and an unlabelled overflowing one receives no grant at all.
- Boundary: field/validation composition is `view/form`'s; grid modeling is `view/table`'s; palette, anchors, and sheets are `view/overlay`'s; discrete interaction hooks are `system/act`'s; locale infra (`I18nProvider`, `useFilter`) is `system/intl`'s.

```typescript signature
import { useEffect, useState } from "react"
import type { RefObject } from "react"

declare namespace Overflow {
  type Grant = { readonly role: "region" | undefined; readonly tabIndex: 0 | undefined }
}

// ResizeObserver measures the grant on every box or content change, never assuming it, and an overflowing container
// without an accessible name receives NO grant, so the unlabelled tab stop is unrepresentable
const _useOverflow = (target: RefObject<HTMLElement | null>, labelled: boolean): Overflow.Grant => {
  const [overflows, setOverflows] = useState(false)
  useEffect(() => {
    // BOUNDARY ADAPTER: ResizeObserver is the platform's measure seam; registration and release bracket on the effect
    const node = target.current
    if (node === null) return undefined
    const observer = new ResizeObserver(() => {
      setOverflows(node.scrollHeight > node.clientHeight || node.scrollWidth > node.clientWidth)
    })
    observer.observe(node)
    return () => observer.disconnect()
  }, [target])
  return overflows && labelled ? { role: "region", tabIndex: 0 } : { role: undefined, tabIndex: undefined }
}
```

## [04]-[TOAST_ANNOUNCE]

[TOAST_ANNOUNCE]:
- Owner: `Primitive.toasts` — one module-level `UNSTABLE_ToastQueue<Note>` (react-stately's queue, re-exported under the prefix; the pre-stable marker is carried on this card, not hidden) with `maxVisibleToasts` policy; the region row renders the prefixed roster the barrel ships — `UNSTABLE_ToastRegion`, `UNSTABLE_ToastList`, `UNSTABLE_Toast`, `UNSTABLE_ToastContent`, `UNSTABLE_ToastStateContext` — over the queue with `Motion.toast` enter/exit variants, and `close` is its imperative retraction.
- Owner: the announce rail — `announce(message, assertiveness?, timeout?)` from `@react-aria/live-announcer`: `"assertive"` interrupts (faults, blocking status), `"polite"` waits (progress, counts); one global visually-hidden region, `destroyAnnouncer()` on host teardown; a bespoke `aria-live` div or a second announcer region is the named defect, and an element-scoped SR-only label takes the radix `VisuallyHidden` instead.
- Owner: `Primitive.notify(note)` — the ONE enqueue: it reads the note's derived urgency row and adds to the queue under that row's dismissal window, so no call site passes a timeout, decides politeness, or spells an urgency literal; `Primitive.toasts` stays the queue value the region renders and `close` the imperative retraction.
- Packages: `react-aria-components` (the prefixed toast roster and `UNSTABLE_ToastQueue`); `@react-aria/live-announcer` (`announce`, `destroyAnnouncer`); `@radix-ui/react-visually-hidden` (`VisuallyHidden` — the element-scoped SR-only label; the aria spine re-exports a same-named component, so the import path is what distinguishes the non-aria plane's usage from a part RAC owns).
- Law: a toast NOTE is the whole notification concept, never a caption — `key` identifies it for retraction and dedup, `tone` keys the token roster, `message` is a `system/intl` catalog key resolved at render so toasts localize like every other string, and `action` carries the repair affordance as an `Option` of a label key and the effect it runs, so a fault-to-toast fold carries the fix instead of only naming the break.
- Law: urgency DERIVES from tone through two closed tables, never from a call-site flag — `_SEVERITY` ranks every roster tone into the `quiet`/`spoken`/`blocking` axis and `_URGENCY` gives each rank its politeness and its dismissal window, so a new tone lands with its rail behavior already decided and a per-site `timeout` number cannot exist. `blocking` carries no window at all: a note the user must act on is retracted by its action or its key, never by a timer.
- Law: politeness is PER NOTE and lands as the content element's role — the toast hook hard-codes `role="alert"` with `aria-atomic` on every note's `contentProps` and exposes no knob at any tier, while the region itself carries only `role="region"` and its label, so the rank realizes by overriding that ONE attribute: `quiet` renders its content under `role="status"` (implicitly polite) and every louder rank keeps the shipped `role="alert"`. One region stays mounted for the whole session; a second region selected by rank, or a bespoke live div beside it, is the double-speak defect the no-second-announcer law already forbids.
- Law: the politeness axis is exactly the two live roles the platform grants — a rank asking for silence is unrealizable, because a note mounted in the region announces by construction; modelling politeness as an optional value spells an arm the DOM cannot render, so the quiet rank buys its restraint from `status` deferring to the user's current utterance rather than from a suppression that does not exist.
- Law: visual toast and SR announcement are one act — the note's rendered content IS its announcement, so `notify` NEVER calls the announce rail; a separate `announce` per toast double-speaks, and the standalone rail serves the non-toast messages ([04]'s second owner) alone.
- Boundary: which flows toast is app policy; the `Result`-failure path routes through the boundary row below, not through toasts, unless the failure is non-blocking evidence; an action's effect is the app's, run at the app's own runtime.

```typescript signature
import { announce, destroyAnnouncer } from "@react-aria/live-announcer"
import { Duration, Effect, Option } from "effect"
import { UNSTABLE_ToastQueue } from "react-aria-components"

declare namespace Note {
  type Assertiveness = "assertive" | "polite" // the standalone announce rail's own axis, spelled as that API takes it
  type Politeness = "status" | "alert" // the note's live semantics AS the content element's role: implicitly polite, implicitly assertive
  type Rank = keyof typeof _URGENCY
  type Action = { readonly label: string; readonly run: Effect.Effect<void> }
  type Urgency = { readonly politeness: Note.Politeness; readonly linger: Option.Option<Duration.Duration> }
}

type Note = {
  readonly key: string
  readonly tone: Theme.Tone
  readonly message: string
  readonly action: Option.Option<Note.Action>
}

// _URGENCY spells the rank axis as the urgency vocabulary; blocking carries no window because its note is retracted by act, not by time
const _URGENCY = {
  quiet: { politeness: "status", linger: Option.some(Duration.seconds(5)) },
  spoken: { politeness: "alert", linger: Option.some(Duration.seconds(9)) },
  blocking: { politeness: "alert", linger: Option.none<Duration.Duration>() },
} as const satisfies Record<string, Note.Urgency>

// total over the roster: a new tone breaks here rather than defaulting into silence
const _SEVERITY = {
  neutral: "quiet",
  accent: "quiet",
  success: "quiet",
  added: "quiet",
  changed: "quiet",
  removed: "spoken",
  caution: "spoken",
  danger: "blocking",
} as const satisfies Record<Theme.Tone, Note.Rank>

const _toasts = new UNSTABLE_ToastQueue<Note>({ maxVisibleToasts: 4 })

const _urgency = (tone: Theme.Tone): Note.Urgency => _URGENCY[_SEVERITY[tone]]

// _live realizes the rank's politeness ONCE: `useToast` writes `role="alert"` into contentProps unconditionally, so
// this spread lands LAST on the content element and is the only place a note's live semantics are decided
const _live = (note: Note): { readonly role: Note.Politeness } => ({ role: _urgency(note.tone).politeness })

const _notify = (note: Note): Effect.Effect<void> =>
  Effect.sync(() => {
    // BOUNDARY ADAPTER: the queue is an imperative host surface, and the absent window omits the key rather than writing undefined
    void _toasts.add(note, {
      ...Option.match(_urgency(note.tone).linger, {
        onNone: () => ({}),
        onSome: (linger) => ({ timeout: Duration.toMillis(linger) }),
      }),
    })
  })

// _announce drives the standalone rail: a message no toast carries (a save status, a result count, a copy receipt) reaches the ONE
// global region; `Assertiveness` is that API's own axis, distinct from the content-element role `_live` decides,
// and the message arrives already resolved through the `system/intl` catalog
const _announce = (message: string, assertiveness: Note.Assertiveness): Effect.Effect<void> =>
  Effect.sync(() => announce(message, assertiveness))

// host teardown releases the singleton region, so a remount re-creates it instead of speaking through a stale twin
const _silence: Effect.Effect<void> = Effect.sync(() => destroyAnnouncer())
```

## [05]-[FAILURE_ENVELOPE]

[FAILURE_ENVELOPE]:
- Owner: `Primitive.boundary` — the one error-boundary row: `ErrorBoundary` with the `fallbackRender` arm (the discriminated prop union's render-prop member), `resetKeys` bound to the failing atom's input so a new query clears a stale error, `onReset` re-running the failed atom through `useAtomRefresh`, and `onError` as the app-wired observation prop (telemetry is not a `ui` edge — the app composes the sink).
- Law: the async-failure rail is Suspense with this boundary — `useAtomSuspense` suspends `waiting` to the nearest `<Suspense>` and throws `Cause.squash(cause)` (the squashed tagged `E`) on `Failure`, so `FallbackProps.error` IS the tagged fault and the fallback folds it with `Match.tagsExhaustive` into localized problem rows; a carrier-less throw (event handler, raw promise) escalates through `useErrorBoundary().showBoundary(Cause.squash(cause))` so both paths land the same shape.
- Law: every route/panel/atom-bound subtree wraps once — boundary granularity is the recovery granularity; a per-component `try`/`catch`, an `instanceof` ladder in a fallback, or a `componentDidCatch` class is the named defect.
- Law: the fold's `error as E` is the platform's type-erasure seam — React strips the thrown type, `FallbackProps.error` arrives `unknown`, and the boundary re-asserts exactly the squashed tagged `E` the atom rail threw; the kernel carries the boundary mark on its first line, and the assertion is legal nowhere else in the module.
- Law: the root callbacks frame the boundary — `onCaughtError`/`onUncaughtError` on `createRoot` are the browser boot options observing what boundaries caught and what escaped; this row owns only the in-tree envelope.
- Growth: a new fault presentation is one `Match` arm in the fallback fold keyed by the family tag; a new recovery affordance rides `resetErrorBoundary`.

```typescript signature
import type { ReactNode } from "react"
import type { FallbackProps } from "react-error-boundary"

declare namespace Boundary {
  type Fold<E> = (error: E, reset: FallbackProps["resetErrorBoundary"]) => ReactNode
}

const _fallbackRender = <E,>(fold: Boundary.Fold<E>): ((props: FallbackProps) => ReactNode) =>
  ({ error, resetErrorBoundary }) => {
    // BOUNDARY ADAPTER
    return fold(error as E, resetErrorBoundary)
  }
```

## [06]-[SANITIZE_GATE]

[SANITIZE_GATE]:
- Owner: `Primitive.sanitize` — the one DOMPurify gate: `_POLICY` is the singular allow-list value (`USE_PROFILES { html: true }`, no inline event attributes ever) applied per call so the module carries zero load-time side effect, `sanitize(dirty)` is the only path to `dangerouslySetInnerHTML`, and the `removed` diagnostic feeds an evidence row when a strip occurs; rich-text catalog messages (`system/intl`) and any wire-borne HTML band pass here — a raw string reaching a DOM sink is the named defect.
- Packages: `isomorphic-dompurify` (`sanitize` — the same module resolves server and browser).
- Law: the gate is isomorphic — the same policy sanitizes server-prerendered and client-hydrated strings, so hydration never diverges on sanitizer output.
- Law: the policy value is singular — a per-call config drift is rejected; a surface needing a stricter profile passes an explicit `Config` at ITS call while `_POLICY` stays the floor.

```typescript signature
import DOMPurify, { type Config } from "isomorphic-dompurify"

const _POLICY = { USE_PROFILES: { html: true }, FORBID_ATTR: ["style"] } as const satisfies Config

const _sanitize = (dirty: string, config?: Config): string => DOMPurify.sanitize(dirty, config ?? _POLICY)

declare namespace Primitive {
  type Shape = {
    readonly styled: typeof _styled
    readonly recipes: { readonly button: typeof _button; readonly skip: typeof _skip }
    readonly useOverflow: typeof _useOverflow
    readonly toasts: typeof _toasts
    readonly notify: typeof _notify
    readonly announce: typeof _announce
    readonly silence: typeof _silence
    readonly urgency: typeof _urgency
    readonly live: typeof _live
    readonly boundary: typeof _fallbackRender
    readonly sanitize: typeof _sanitize
  }
}

const Primitive: Primitive.Shape = {
  styled: _styled,
  recipes: { button: _button, skip: _skip },
  useOverflow: _useOverflow,
  toasts: _toasts,
  notify: _notify,
  announce: _announce,
  silence: _silence,
  urgency: _urgency,
  live: _live,
  boundary: _fallbackRender,
  sanitize: _sanitize,
}
```

## [07]-[CLIPBOARD_PORT]

[CLIPBOARD_PORT]:
- Owner: `Clipboard` — the folder-declared clipboard capability Tag: `copy(text)` and `paste` on a typed fault rail, `granted` as the live verdict stream, declared HERE and satisfied at the browser composition root from the platform clipboard and permissions layers — this folder never imports the platform package, so the capability travels the requirement channel and a test substitutes a Layer.
- Packages: `effect` (`Context`, `Schema`, `Stream`); `@rasm/core` (`Fault.Class`).
- Law: consumers compose the port, never the Web API — the palette copy-command (`view/overlay#PALETTE`) and the probe copy-evidence affordance (`viewer/probe`) reach the clipboard only through this Tag; a `navigator.clipboard` read in a row is the named defect.
- Law: this port owns its own permission custody because the platform's grant axis cannot carry it — `PermissionName` closes without a clipboard member, so no generic permissions port can answer for this capability and the verdict has to travel with the capability itself; `granted` is therefore a stream rather than a read, since a mount-time verdict renders a revoked capability as available for the rest of the session.
- Boundary: this port owns the PASTE BUFFER and nothing else — the file system and the share sheet are `view/export`'s `Egress` capability, so a save, a download, and a share are that port's rows while a copy and a paste are these; the two are one concern only from the user's side, and folding either into the other puts a permission-gated system dialog behind a synchronous clipboard call.
- Law: a refusal names the VERB it refused — the user agent stops a copy and a paste on different grounds, and a caller re-offering has to know which half it lost, so the verb is the row's own declared subject rather than context the catch site reconstructs from where it stood.
- Growth: a blob/image lane is one member row on THIS service shape — never a second clipboard port; a new refusal condition is one family row carrying its core class, leg, subject, and renderer.

```typescript signature
import { Fault } from "@rasm/core"
import { Context, Schema, type Effect, type Stream } from "effect"

// the port's two verbs ARE the refusal subject, so the literal is declared once and both rows render off it
const _Verb = Schema.Literal("copy", "paste")

const _family = Fault.Class.family(
  ["denied", "unavailable"] as const,
  {
    denied: Fault.Class.row({
      class: "denied",
      leg: "clipboard",
      detail: Schema.Struct({ verb: _Verb }),
      render: ({ verb }) => `user agent denied the clipboard ${verb}`,
    }),
    unavailable: Fault.Class.row({
      class: "unavailable",
      leg: "clipboard",
      detail: Schema.Struct({ verb: _Verb }),
      render: ({ verb }) => `clipboard ${verb} is unreachable in this context`,
    }),
  },
)

class ClipboardFault extends Schema.TaggedError<ClipboardFault>()("ClipboardFault", {
  case: _family.payload,
}) {
  get class(): Fault.Class.Kind {
    return _family.classOf(this.case.reason)
  }
  override get message(): string {
    return _family.render(this.case)
  }
}

class Clipboard extends Context.Tag("ui/Clipboard")<Clipboard, {
  readonly copy: (text: string) => Effect.Effect<void, ClipboardFault>
  readonly paste: Effect.Effect<string, ClipboardFault>
  readonly granted: Stream.Stream<PermissionState> // this capability's own live verdict: revocation degrades the affordance at the instant, never at the next mount
}>() {}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Clipboard, ClipboardFault, Primitive }
```

## [08]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
