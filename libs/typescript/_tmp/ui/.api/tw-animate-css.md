# [API_CATALOGUE] tw-animate-css

`tw-animate-css` is the Tailwind CSS v4 enter/exit animation layer — the v4-native replacement for `tailwindcss-animate`, imported as a pure CSS `@import` with no JavaScript or type surface. It is ONE parameterized mechanism, not a fixed roster: each modifier utility is a `@utility name-*` rule whose numeric suffix is `--value()`-resolved and which sets one registered `--tw-enter-*`/`--tw-exit-*` CSS custom property, and the `animate-in`/`animate-out` base utility runs the single `enter`/`exit` `@keyframes` that folds every set var into one `translate3d()scale3d()rotate()` transform plus `filter: blur()`. So `fade-in-0…100`, `zoom-in-95`, `slide-in-from-top-2` are instances of the same rule. Beside the enter/exit axes it ships the named `animate-accordion-*`/`animate-collapsible-*` height compositions keyed to the Radix/Bits/Reka/Kobalte/NGP content-height vars and `animate-caret-blink`. It ships NO reduced-motion rule — motion suppression is the consumer's `motion-reduce:` responsibility.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `tw-animate-css`
- package / version: `tw-animate-css` @ `1.4.0`
- license: `MIT`
- module: CSS-only — no JS runtime, no `.d.ts`; `.` → `dist/tw-animate.css` (resolved via the `style` export condition), `./prefix` → `dist/tw-animate-prefix.css`
- requires: Tailwind CSS v4 (authored in `@utility`/`@theme`/`@property`/`--value()` v4 syntax); no npm `peerDependencies` declared
- rail: styling-animation

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: none (CSS-only) — the typed surface is the registered CSS custom-property vocabulary
- rail: styling-animation

The package exposes no TypeScript symbols; it is consumed through `@import` and emits utility class names resolved by the Tailwind v4 engine at build. Its stateful "types" are CSS custom properties registered via `@property` (`syntax`/`inherits: false`/`initial-value`) so they are typed, animatable, and SSR-safe — each modifier utility writes one, the `enter`/`exit` keyframes read them.

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [NOTE] |
| :-----: | :------- | :------------ | :----- |
|  [01]   | `--tw-enter-opacity` / `--tw-exit-opacity` | enter/exit opacity | set by `fade-in*`/`fade-out*`; default `1` |
|  [02]   | `--tw-enter-scale` / `--tw-exit-scale` | enter/exit scale | set by `zoom-in*`/`zoom-out*`; default `1` |
|  [03]   | `--tw-enter-rotate` / `--tw-exit-rotate` | enter/exit rotate | set by `spin-in*`/`spin-out*`; default `0` |
|  [04]   | `--tw-enter-translate-x` / `-y` (+ `--tw-exit-*`) | enter/exit translate | set by `slide-in-from-*`/`slide-out-to-*`; default `0` |
|  [05]   | `--tw-enter-blur` / `--tw-exit-blur` | enter/exit blur | set by `blur-in*`/`blur-out*`; default `0` |
|  [06]   | `--tw-animation-duration` / `--tw-animation-delay` | timing | set by `animation-duration-*`/`delay-*`; fall back to Tailwind-core `--tw-duration` |
|  [07]   | `--tw-animation-iteration-count` / `--tw-animation-direction` / `--tw-animation-fill-mode` | playback | set by `repeat-*`/`direction-*`/`fill-mode-*`; defaults `1`/`normal`/`none` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: CSS import targets
- rail: styling-animation

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [NOTE] |
| :-----: | :-------- | :------------- | :----- |
|  [01]   | `@import "tw-animate-css";` | CSS layer | the unprefixed utility layer; lands after `@import "tailwindcss";` at the stylesheet root |
|  [02]   | `@import "tw-animate-css/prefix";` | CSS layer | companion build for a prefixed Tailwind config (`@import "tailwindcss" prefix(...)`) so utilities resolve under the framework prefix |

[ENTRYPOINT_SCOPE]: base + parameterized enter/exit axes (each one `@utility name-*` driven by `--value(number\|ratio\|--percentage-*\|[*])`)
- rail: styling-animation

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [NOTE] |
| :-----: | :-------- | :------------- | :----- |
|  [01]   | `animate-in` / `animate-out` | base keyframe | runs the single `enter`/`exit` keyframe reading whichever `--tw-enter-*`/`--tw-exit-*` vars the modifiers set; compose any subset of modifiers with these |
|  [02]   | `fade-in` / `fade-in-*` / `fade-out` / `fade-out-*` | opacity | `--tw-enter/exit-opacity`; bare → `0`, `-<n>` → `n/100` via `--percentage-*` or arbitrary `[*]` |
|  [03]   | `zoom-in` / `zoom-in-*` / `-zoom-in-*` / `zoom-out` / `zoom-out-*` / `-zoom-out-*` | scale | `--tw-enter/exit-scale`; `<n>` → `n%`, `ratio`, or `--percentage-*`; leading `-` negates |
|  [04]   | `spin-in` / `spin-in-*` / `-spin-in*` / `spin-out` / `spin-out-*` / `-spin-out*` | rotate | `--tw-enter/exit-rotate`; bare → `±30deg`, `<n>` → `n deg`, `ratio` → `360deg`, or `--rotate-*` |
|  [05]   | `slide-in-from-{top,bottom,left,right}` / `-*` (+ `slide-out-to-{…}` / `-*`) | translate | `--tw-enter/exit-translate-{x,y}`; bare → `±100%`, `<n>` → `n·--spacing`, `--percentage-*`, `ratio` → `100%`, or `[length]` |
|  [06]   | `slide-in-from-{start,end}` / `-*` (+ `slide-out-to-{start,end}` / `-*`) | logical translate | RTL-aware translate: `:dir(ltr)`/`:dir(rtl)` flips the sign so `start`/`end` follow writing direction |
|  [07]   | `blur-in` / `blur-in-*` / `blur-out` / `blur-out-*` | blur | `--tw-enter/exit-blur`; bare → `20px`, `<n>` → `n px`, or `--blur-*` |

[ENTRYPOINT_SCOPE]: animation-control utilities (owned by this layer; timing also reads Tailwind-core `--tw-duration`/`--tw-ease`)
- rail: styling-animation

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [NOTE] |
| :-----: | :-------- | :------------- | :----- |
|  [01]   | `animation-duration-*` | duration | `<n>` → `n ms`, `--animation-duration-*`, or `[duration]`; overrides the core `--tw-duration` the base shorthand reads |
|  [02]   | `delay-*` | delay | `<n>` → `n ms`, `--animation-delay-*` (`0`/`75`/`100`/`150`/`200`/`300`/`500`/`700`/`1000`), or `[duration]` |
|  [03]   | `repeat-*` | iteration count | `--animation-repeat-*` (`0`/`1`/`infinite`) or arbitrary `[*]` |
|  [04]   | `direction-*` | direction | `--animation-direction-*` (`normal`/`reverse`/`alternate`/`alternate-reverse`) |
|  [05]   | `fill-mode-*` | fill mode | `--animation-fill-mode-*` (`none`/`forwards`/`backwards`/`both`) |
|  [06]   | `running` / `paused` / `play-state-*` | play state | `animation-play-state`; `play-state-[*]` for arbitrary |

[ENTRYPOINT_SCOPE]: named animation compositions (Tailwind `--animate-*` theme keys)
- rail: styling-animation

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [NOTE] |
| :-----: | :-------- | :------------- | :----- |
|  [01]   | `animate-accordion-down` / `animate-accordion-up` | height | reads `--radix-accordion-content-height` (fallbacks `--bits-`/`--reka-`/`--kb-`/`--ngp-accordion-content-height`) a Radix/Bits/Reka/Kobalte/NGP Accordion sets |
|  [02]   | `animate-collapsible-down` / `animate-collapsible-up` | height | reads `--radix-collapsible-content-height` (+ bits/reka/kb fallbacks) a Collapsible primitive sets |
|  [03]   | `animate-caret-blink` | caret | `1.25s ease-out infinite` opacity blink — the `cmdk` command-input caret |

## [04]-[IMPLEMENTATION_LAW]

[ANIMATION_TOPOLOGY]:
- One parameterized mechanism: every modifier `@utility name-*` sets one or more `--tw-enter-*`/`--tw-exit-*` custom properties (registered via `@property`), and `animate-in`/`animate-out` runs the single `enter`/`exit` `@keyframes` that reads those vars into one `translate3d(--tw-enter-translate-x,-y,0)scale3d(--tw-enter-scale…)rotate(--tw-enter-rotate)` transform plus `filter: blur(--tw-enter-blur)`. The numeric suffix is `--value(number|ratio|--percentage-*|[length])`-resolved, so the amount is data, never a hardcoded utility.
- Composition folds: stack any subset of enter modifiers with `animate-in` (exit modifiers with `animate-out`); each writes its own var and the shared keyframe composes them — `animate-in fade-in-0 zoom-in-95 slide-in-from-top-2` is one enter animation, `animate-out fade-out-0 slide-out-to-bottom` one exit.
- Timing: `animate-in`/`animate-out` expand to the `--animate-in`/`--animate-out` theme shorthand reading `var(--tw-animation-duration, var(--tw-duration, .15s))`, `var(--tw-ease, ease)`, and the `--tw-animation-{delay,iteration-count,direction,fill-mode}` chain. Tailwind-core `duration-*` (`--tw-duration`) and `ease-*` (`--tw-ease`) supply defaults; the tw-animate `animation-duration-*`/`delay-*`/`repeat-*`/`direction-*`/`fill-mode-*` utilities override them.
- No reduced-motion rule ships in `1.4.0`: the CSS contains no `@media (prefers-reduced-motion)` / `motion-reduce` / `@starting-style`. A motion-suppressed render is NOT handled by this layer — gate it with Tailwind's `motion-reduce:` / `motion-safe:` variants or an app-level `@media (prefers-reduced-motion: reduce)` rule.
- Continuous `animate-spin`/`animate-pulse`/`animate-bounce` are Tailwind-core, distinct from this layer's `spin-in`/`spin-out` enter/exit rotations.
- Emits no JavaScript; the entire surface resolves against the Tailwind v4 `@theme` scales (`--percentage-*`, `--animation-*`, `--spacing`, `--translate-*`, `--rotate-*`, `--blur-*`) — token-driven durations/easings flow from the OKLCH `@theme` layer.

[STACKING]:
- import seam: `@import "tailwindcss"; @import "tw-animate-css";` at the design-token stylesheet root beside the `@theme` OKLCH token layer — global once present, never re-imported per component (`theming/tokens.md#THEME_TOKENS`).
- Radix `data-state` overlay enter/exit: bind `data-[state=open]:animate-in` / `data-[state=closed]:animate-out` (+ `fade-*`/`zoom-*`/`slide-*` modifiers) to the `data-state="open|closed"` attribute the `cmdk` `Command.Dialog` (Radix-Dialog shell) and `vaul` `Drawer.Overlay` emit — declarative CSS enter/exit resolving against live OKLCH tokens, never a JS controller (`cmdk.md`, `vaul.md`).
- accordion/collapsible height: `animate-accordion-down/up` and `animate-collapsible-down/up` read the `--radix-accordion-content-height` / `--radix-collapsible-content-height` var (with Bits/Reka/Kobalte/NGP fallbacks) the primitive sets — one named utility, never a JS-measured height.
- cva + tailwind-merge recipe: compose the `animate-*`/`data-[state]:animate-*` utilities into a `cva` variant row read through the one `cn = twMerge(cx(...))` owner so an enter/exit class is theme-reactive (`class-variance-authority.md`, `interaction/command.md#COMMAND_SURFACE`). These names sit OUTSIDE `tailwind-merge`'s default Tailwind-v4 `classGroups`, so a later modifier overriding an earlier one (`fade-in-0` → `fade-in-100`) resolves last-wins only when the merger admits them as a custom `animate` `classGroup` through the memoized `extendTailwindMerge` (`tailwind-merge.md`); distinct enter modifiers (`fade-in-*` + `slide-in-from-*`) never collide, so the default merge is a safe no-op there.
- View Transitions boundary: this layer owns component-local `data-state` enter/exit; route-level choreography is the `<ViewTransition>` capture (`interaction/transition.md#VIEW_TRANSITIONS`), and that page's reduced-motion gate is a JS `matchMedia` read — NOT this layer, which ships no reduced-motion rule.

[LOCAL_ADMISSION]:
- Import once at the design-token stylesheet root after `@import "tailwindcss"`; the utilities are global thereafter, never per component.
- Drive overlay enter/exit through `data-[state]` utilities keyed to the component's own `data-state`, never a JS animation controller.
- Compose enter modifiers with `animate-in` and exit modifiers with `animate-out`; a numeric suffix or arbitrary `[value]` selects the amount — never hand-author a `@keyframes` this layer already supplies.
- Reduced motion is NOT in this layer: gate it with Tailwind's `motion-reduce:` / `motion-safe:` variants or an app `@media (prefers-reduced-motion: reduce)` rule.
- Use `tw-animate-css/prefix` only when the Tailwind config sets a utility prefix.

[RAIL_LAW]:
- package: `tw-animate-css`
- owns: the Tailwind v4 parameterized enter/exit/fade/zoom/slide/spin/blur animation utility layer keyed by `data-state`, plus the named accordion/collapsible/caret compositions
- accept: the CSS `@import`, `animate-in`/`animate-out` + the parameterized modifier and animation-control utilities, the `data-[state]` enter/exit selectors, `animate-accordion-*`/`animate-collapsible-*`/`animate-caret-blink`, the prefixed build
- reject: a hand-authored `@keyframes` for enter/exit this layer supplies; a JS animation controller for a `data-state` transition; a claim that this layer suppresses motion (it ships no reduced-motion rule — the consumer gates it via `motion-reduce:`)
