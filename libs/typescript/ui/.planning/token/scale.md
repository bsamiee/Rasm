# [UI_SCALE]

`token/scale.ts` is the dimension-and-motion vocabulary: one `--spacing` multiplier scaling the whole layout system, a paired text scale (each step carrying its line-height), radius/breakpoint/easing rows, and the motion vocabulary — named enter/exit compositions over `tw-animate-css`'s one keyframe mechanism (trigger + axis setters + timing modifiers), reduced-motion-gated by construction. Every row is an `as const` table emitted through `token/theme`'s `Theme.css` fold and consumed as class strings through the one `cn` rail; a component never writes a raw pixel, a bespoke `@keyframes`, or a bare `animate-in` without setters.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]      | [OWNS]                                                                            |
| :-----: | :------------- | :--------------------------------------------------------------------------------- |
|   [1]   | `SCALE_TABLES` | the spacing multiplier, text/radius/breakpoint/easing rows, and their `@theme` emit |
|   [2]   | `MOTION_ROWS`  | the named enter/exit composition vocabulary over the tw-animate axis mechanism      |

## [2]-[SCALE_TABLES]

- Owner: `Scale` — one assembled owner over four interior anchors: `_spacing` (the single multiplier — a density change is this one token, never a scale rewrite), `_text` (step → `{ size, leading }` pairs, each emitting the `--text-*` + `--text-*--line-height` twin), `_radius`, `_ease` (easing curves as cubic-bezier rows), and `_breakpoint`; `Scale.css()` folds all of them through `Theme.css` into the `@theme` declarations the build stylesheet inlines.
- Packages: `tailwindcss` v4 namespaces are the sink (`--spacing`, `--text-*`, `--radius-*`, `--ease-*`, `--breakpoint-*` — one namespace row generates the variable and its utility family); `effect` `Record` folds the emission.
- Law: the type scale is paired data — a text step without its line-height is half a token; the emission writes both variables from one row so `text-<step>` always carries its leading.
- Law: scale values are derivation, not enumeration — the text ladder derives from a ratio fold over the base size (a modular scale), so retuning typography is two numbers, never twelve edits.
- Law: no JS reads these values at runtime — the tables exist to emit CSS; a component consumes `p-4`/`text-lg`/`rounded-md` utilities through `cn`, and a runtime pixel computation over `_spacing` marks logic that belongs in CSS.
- Boundary: the emission mechanism is `token/theme`'s `Theme.css`; container-query and aspect namespaces join as rows here when a consumer earns them; the Vite integration (`@tailwindcss/vite`) is app build wiring.
- Growth: a new axis (a z-index ladder, a shadow ramp) is one interior anchor plus one line in `Scale.css` — never a hand-written utility or a component-local constant.

```typescript
import { Array, Record } from "effect"
import { Theme } from "./theme.ts"

const _spacing = "0.25rem"

const _RATIO = 1.2
const _BASE = 1

const _steps = ["xs", "sm", "base", "lg", "xl", "2xl", "3xl", "4xl"] as const

const _text = Record.fromEntries(
  Array.map(_steps, (step, rank) => {
    const size = _BASE * _RATIO ** (rank - 2)
    return [step, { size: `${size.toFixed(3)}rem`, leading: `${Math.max(1.2, 1.6 - rank * 0.05).toFixed(2)}` }] as const
  }),
)

const _radius = { sm: "0.25rem", md: "0.5rem", lg: "0.75rem", full: "9999px" } as const

const _ease = {
  out: "cubic-bezier(0.16, 1, 0.3, 1)",
  in: "cubic-bezier(0.7, 0, 0.84, 0)",
  spring: "cubic-bezier(0.34, 1.56, 0.64, 1)",
} as const

const _breakpoint = { sm: "40rem", md: "48rem", lg: "64rem", xl: "80rem" } as const

declare namespace Scale {
  type Step = (typeof _steps)[number]
  type Radius = keyof typeof _radius
  type Ease = keyof typeof _ease
}

const Scale: {
  readonly steps: typeof _steps
  readonly radius: typeof _radius
  readonly ease: typeof _ease
  readonly breakpoint: typeof _breakpoint
  readonly css: () => string
} = {
  steps: _steps,
  radius: _radius,
  ease: _ease,
  breakpoint: _breakpoint,
  css: () =>
    [
      Theme.css("spacing", { "": _spacing }),
      Theme.css("text", Record.map(_text, (row) => row.size)),
      Theme.css("text", Record.fromEntries(Record.collect(_text, (step, row) => [`${step}--line-height`, row.leading] as const))),
      Theme.css("radius", _radius),
      Theme.css("ease", _ease),
      Theme.css("breakpoint", _breakpoint),
    ].join("\n"),
}
```

## [3]-[MOTION_ROWS]

- Owner: `Motion` — the named motion vocabulary: one `as const satisfies Record<string, Motion.Row>` table whose rows compose `tw-animate-css`'s single mechanism — `animate-in`/`animate-out` trigger + axis setters (`fade-*`, `zoom-*`, `slide-*`, `blur-*`, `spin-*`) + timing modifiers (`animation-duration-*`, `delay-*`, `fill-mode-*`) — into enter/exit class pairs keyed by surface concept (`overlay`, `sheet`, `palette`, `toast`, `panel`). Every row leads with `motion-reduce:animate-none` so reduced motion is a construction fact.
- Packages: `tw-animate-css` (imported once in the token stylesheet as `@import "tw-animate-css";` after the tailwind entry — pure CSS, zero runtime), `tailwindcss` core `motion-reduce:` variant.
- Law: a motion is trigger plus at least one axis setter — a bare `animate-in` animates nothing; the row table makes the pairing structural because every row string carries both.
- Law: never author a `@keyframes` for an enter/exit effect the six axes express; the named component animations (`animate-accordion-down/up`, `animate-collapsible-down/up`, `animate-caret-blink`) are the only sanctioned self-contained keyframes and ride rows here, not bespoke CSS.
- Law: the RAC transition phases bind these rows through variants — `entering:` and `exiting:` (the `tailwindcss-react-aria-components` mappings of `data-entering`/`data-exiting`) scope the enter/exit halves, so overlay motion is one `cn(Motion.overlay.enter, Motion.overlay.exit)` class string with zero JS lifecycle code.
- Law: the row strings participate in `cn` conflict resolution — the motion class groups were taught to the one merge instance at `token/theme`, so a caller override of `delay-*` or `fade-in-*` wins deterministically.
- Boundary: View Transition document-level motion is `act/transition`'s; `@floating-ui/react` `useTransitionStyles` phases consume `_ease` values where an overlay needs style-object motion; the sheet's drag physics are `vaul`'s own and take no Motion row.
- Growth: a new surface motion is one row composing existing setters; a new axis is upstream (`tw-animate-css`), never a local keyframe.

```typescript
const _kinds = ["overlay", "sheet", "palette", "toast", "panel"] as const

declare namespace Motion {
  type Row = { readonly enter: string; readonly exit: string }
  type Kind = (typeof _kinds)[number]
}

const _rows = {
  overlay: {
    enter: "motion-reduce:animate-none entering:animate-in entering:fade-in-0 entering:zoom-in-95 entering:animation-duration-150",
    exit: "motion-reduce:animate-none exiting:animate-out exiting:fade-out-0 exiting:zoom-out-95 exiting:animation-duration-100",
  },
  sheet: {
    enter: "motion-reduce:animate-none entering:animate-in entering:slide-in-from-bottom entering:animation-duration-300",
    exit: "motion-reduce:animate-none exiting:animate-out exiting:slide-out-to-bottom exiting:animation-duration-200",
  },
  palette: {
    enter: "motion-reduce:animate-none entering:animate-in entering:fade-in-0 entering:slide-in-from-top-2 entering:animation-duration-150",
    exit: "motion-reduce:animate-none exiting:animate-out exiting:fade-out-0 exiting:animation-duration-100",
  },
  toast: {
    enter: "motion-reduce:animate-none entering:animate-in entering:slide-in-from-right entering:fade-in-0 entering:animation-duration-200",
    exit: "motion-reduce:animate-none exiting:animate-out exiting:slide-out-to-right exiting:fade-out-0 exiting:animation-duration-150",
  },
  panel: {
    enter: "motion-reduce:animate-none entering:animate-in entering:fade-in-0 entering:animation-duration-100",
    exit: "motion-reduce:animate-none exiting:animate-out exiting:fade-out-0 exiting:animation-duration-100",
  },
} as const satisfies Record<Motion.Kind, Motion.Row>

const Motion: typeof _rows & { readonly kinds: typeof _kinds } = {
  ..._rows,
  kinds: _kinds,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Motion, Scale }
```
