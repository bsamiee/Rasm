# [TS_UI_API_TW_ANIMATE_CSS]

`tw-animate-css` is a pure-CSS Tailwind catalog-bound plugin (no JS, no `.d.ts`, no ABI) that is ONE parameterized enter/exit motion mechanism, not a fixed animation roster: a single `enter`/`exit` `@keyframes` pair reads SIX `@property`-typed custom-property axes (`--tw-enter-*`/`--tw-exit-*`: opacity, scale, rotate, blur, translate-x, translate-y), and every animation is `animate-in`/`animate-out` (the trigger that binds the keyframe) composed with one setter utility per axis (`fade-in-0`, `zoom-in-95`, `spin-in-45`, `blur-in`, `slide-in-from-top-2`) plus the timing modifiers (`duration-*`, `delay-*`, `repeat-*`, `direction-*`, `fill-mode-*`). A new motion is a different combination of setters over the same two keyframes — never a new `@keyframes`. It ships three named component animations pre-wired to headless-kit content-height vars (`animate-accordion-down/up`, `animate-collapsible-down/up`, `animate-caret-blink`), and its classes are the `ClassValue`s the `token/scale` plane folds through the one `cn = twMerge(clsx(...))` rail. This is the successor to `tailwindcss-animate`, rebuilt on Tailwind catalog-bound primitives (`@utility`, `@theme inline`, `--value()`, `@property`).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `tw-animate-css`
- package: `tw-animate-css` (MIT)
- deps: none
- catalog-verdict: KEEP
- asset: pure CSS — `main: dist/tw-animate.css`, NO `types`, NO JS, NO runtime. There is nothing for `tsc` to check; the surface is the utility-class + custom-property vocabulary Tailwind compiles
- peer: `tailwindcss` catalog (`.api/tailwindcss.md`) — hard requirement; uses `@utility`, `@theme inline`, `--value(...)`, and `@property`, none of which exist in catalog
- import: `@import "tw-animate-css";` after `@import "tailwindcss";` in the theme entry CSS; the `./prefix` export (`@import "tw-animate-css/prefix";`) namespaces every utility under `tw-` (`tw-animate-in`) to avoid collisions
- exports: `.` (`dist/tw-animate.css`), `./prefix` (`dist/tw-animate-prefix.css`)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the enter/exit axis registers — six `@property`-typed custom properties
- rail: token/scale
- The typed motion axes. `@property` declares each with a syntax and initial value so it animates and inherits correctly; the `enter`/`exit` keyframes read exactly these. The whole animation surface is projecting values onto these six axes (× enter/exit) — this is the collapse point, the reason there are two keyframes and not thirty.

Every axis is consumed by `token/scale`; each cell names the setter and its keyframe effect.

| [INDEX] | [SYMBOL]                                                              | [TYPE_FAMILY]    | [CONSUMER_BOUNDARY]                       |
| :-----: | :-------------------------------------------------------------------- | :--------------- | :---------------------------------------- |
|  [01]   | `--tw-enter-opacity` / `--tw-exit-opacity` (`@property`, initial `1`) | opacity axis     | `fade-in`/`fade-out`; keyframe `opacity`  |
|  [02]   | `--tw-enter-scale` / `--tw-exit-scale` (initial `1`)                  | scale axis       | `zoom-in`/`zoom-out`; `scale3d(...)`      |
|  [03]   | `--tw-enter-rotate` / `--tw-exit-rotate` (initial `0`)                | rotate axis      | `spin-in`/`spin-out`; `rotate(...)`       |
|  [04]   | `--tw-enter-blur` / `--tw-exit-blur` (initial `0`)                    | blur axis        | `blur-in`/`blur-out`; `filter: blur(...)` |
|  [05]   | `--tw-enter-translate-x` / `--tw-exit-translate-x` (initial `0`)      | x-translate axis | `slide-*-left/right/start/end`; `x`       |
|  [06]   | `--tw-enter-translate-y` / `--tw-exit-translate-y` (initial `0`)      | y-translate axis | `slide-*-top/bottom`; `y`                 |

[PUBLIC_TYPE_SCOPE]: the timing registers + theme-token scales
- rail: token/scale
- The animation timing `@property` axes and the `@theme inline` token scales they draw from. `--animate-in`/`--animate-out` are the master theme animations that become the `animate-in`/`animate-out` utilities; the `--animation-*` and `--percentage-*` scales are the bounded value vocabularies the setter/modifier utilities resolve against.

The `--tw-animation-*` axes are `@property`-typed; consumer `token/scale` unless a cell names another seam.

| [INDEX] | [SYMBOL]                                                                    | [TYPE_FAMILY]    | [CONSUMER_BOUNDARY]                   |
| :-----: | :-------------------------------------------------------------------------- | :--------------- | :------------------------------------ |
|  [01]   | `--animate-in` / `--animate-out` (`@theme`, = `enter`/`exit` + timing vars) | master animation | the `animate-in`/`out` utilities      |
|  [02]   | `--tw-animation-{delay,direction,duration,fill-mode,iteration-count}`       | timing axis      | set by the timing modifiers           |
|  [03]   | `--animate-accordion-down/up`                                               | named animation  | `view/primitive` — accordion, [03]    |
|  [04]   | `--animate-collapsible-down/up` / `--animate-caret-blink`                   | named animation  | `view/primitive` — collapsible, caret |
|  [05]   | `--animation-delay-{0,75,100,150,200,300,500,700,1000}`                     | delay scale      | `delay-*`; `delay-[…]` arbitrary      |
|  [06]   | `--animation-repeat-{0,1,infinite}`                                         | timing scale     | `repeat-*` values                     |
|  [07]   | `--animation-direction-{normal,reverse,alternate,alternate-reverse}`        | timing scale     | `direction-*` values                  |
|  [08]   | `--animation-fill-mode-{none,forwards,backwards,both}`                      | timing scale     | `fill-mode-*` values                  |
|  [09]   | `--percentage-{0,5,…,100}` / `--percentage-translate-full`                  | fraction scale   | `fade-*`/`zoom-*` percentages         |

- [03]-[CONTENT_HEIGHT]: `animate-accordion-*` reads `--radix-accordion-content-height` (+ `--bits-*`/`--reka-*`/`--kb-*`/`--ngp-*` fallbacks); `fade-in-0` → opacity 0, `zoom-in-95` → scale .95.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the trigger + per-axis setter utilities — one keyframe pair, N setters
- rail: token/scale
- The class vocabulary a `view`/`token` row writes. `animate-in`/`animate-out` bind the `enter`/`exit` keyframe; each setter family projects one axis. The rosters below are SEED DATA for the one mechanism — `animate-in fade-in-0 zoom-in-95 slide-in-from-top-2` is the composition, and a new motion is a new set of setters, never a new utility mechanism. Every setter has an arbitrary-value `-*` variant (`fade-in-[.3]`, `slide-in-from-top-[10px]`).

`animate-in`/`animate-out` bind the keyframe (a bare trigger animates nothing); every setter writes `--tw-enter/exit-*`, and `slide` `start`/`end` are dir-aware (`:dir(ltr/rtl)`) logical edges.

| [INDEX] | [SURFACE]                                                                            | [ENTRY_FAMILY]   | [CONSUMER_BOUNDARY]            |
| :-----: | :----------------------------------------------------------------------------------- | :--------------- | :----------------------------- |
|  [01]   | `animate-in` / `animate-out`                                                         | trigger          | `view/primitive`; needs setter |
|  [02]   | `fade-in` / `fade-in-<0..100>` / `fade-out` / `fade-out-*`                           | opacity setter   | `-opacity`; `fade-in-0` = 0    |
|  [03]   | `zoom-in` / `zoom-in-<n>` / `-zoom-in-*` / `zoom-out` / `zoom-out-*` / `-zoom-out-*` | scale setter     | `-scale`; neg mirrors          |
|  [04]   | `spin-in` / `spin-in-<deg>` / `-spin-in` / `spin-out` / `spin-out-*` / `-spin-out-*` | rotate setter    | `-rotate`; 30deg default       |
|  [05]   | `blur-in` / `blur-in-<n>` / `blur-out` / `blur-out-*`                                | blur setter      | `-blur`; 20px default          |
|  [06]   | `slide-in-from-{top,bottom,left,right,start,end}` (+ `-*`) / `slide-out-to-{…}`      | translate setter | `-translate-{x,y}`; dir-aware  |

[ENTRYPOINT_SCOPE]: the timing modifiers + named component animations
- rail: token/scale
- The modifiers that shape the bound animation's timing, and the three named animations for stateful components. The timing modifiers set the `--tw-animation-*` axes the master `--animate-in/out` reads; the named animations are complete `--animate-*` theme entries for the accordion/collapsible/caret patterns.

`delay-*` sets `animation-delay` (not `transition-delay`); named animations read the kit `--radix-*-content-height` (+ fallbacks).

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY]  | [CONSUMER_BOUNDARY]                      |
| :-----: | :----------------------------------------------------------------- | :-------------- | :--------------------------------------- |
|  [01]   | `animation-duration-<n>` / `delay-<n>` / `repeat-<0\|1\|infinite>` | timing modifier | `token/scale` — duration/delay/iteration |
|  [02]   | `direction-<normal\|reverse\|alternate\|alternate-reverse>`        | timing modifier | `direction-*` values                     |
|  [03]   | `fill-mode-<none\|forwards\|backwards\|both>`                      | timing modifier | fill-mode; forwards holds exit           |
|  [04]   | `running` / `paused` / `play-state-*`                              | play state      | `act/gesture` — pause/resume motion      |
|  [05]   | `animate-accordion-down` / `animate-accordion-up`                  | named animation | `view/primitive` — accordion height      |
|  [06]   | `animate-collapsible-down` / `animate-collapsible-up`              | named animation | `view/primitive` — collapsible height    |
|  [07]   | `animate-caret-blink`                                              | named animation | `view/compose` — `cmdk` caret blink      |

## [04]-[IMPLEMENTATION_LAW]

[MOTION_TOPOLOGY]:
- ONE mechanism, six axes: the `enter`/`exit` keyframes read `--tw-enter-*`/`--tw-exit-*` (opacity, scale, rotate, blur, translate-x, translate-y); `animate-in`/`animate-out` bind the keyframe; each `fade`/`zoom`/`spin`/`blur`/`slide` utility SETS one axis. Composition, not enumeration — `animate-in fade-in-0 zoom-in-95 slide-in-from-top-2` is three axis setters over one keyframe, and a new motion is a new setter combination. Never author a `@keyframes` for an enter/exit effect the axes already express.
- The trigger is inert without setters: `animate-in` alone binds `enter` but every axis defaults to its identity value, so nothing moves. A motion is ALWAYS `trigger + ≥1 setter`; the setters are custom-property assignments the keyframe consumes at the `from` (enter) / `to` (exit) edge.
- Timing is separable: `animation-duration-*`/`delay-*`/`repeat-*`/`direction-*`/`fill-mode-*` set the `--tw-animation-*` axes the master `--animate-in/out` reads, so timing composes orthogonally to the axis setters. `--tw-animation-duration` falls back to Tailwind core's `--tw-duration` (`duration-*`) then `.15s`.
- Named animations are complete theme entries, not the axis mechanism: `animate-accordion-*`/`animate-collapsible-*` bind their own height keyframes and read the headless kit's `--radix-*`/`--bits-*`/`--reka-*`/`--kb-*`/`--ngp-*` content-height var; `animate-caret-blink` is a self-contained opacity loop. These are the stateful-component exceptions to the enter/exit axis space.

[INTEGRATION_LAW]:
- Stack with `class-variance-authority` + `clsx` + `tailwind-merge` (`.api/class-variance-authority.md`, `.api/clsx.md`, `.api/tailwind-merge.md`): the animation classes are `ClassValue`s folded through the one `cn = twMerge(clsx(...))`; a `cva` motion axis carries `animate-in fade-in-0 …` as a variant value. Extend `tailwind-merge` (`extendTailwindMerge`) so the tw-animate utility groups (`fade`, `zoom`, `slide`, `duration`, `delay`) participate in last-wins conflict resolution — otherwise two conflicting `fade-in-*` classes both survive.
- Stack with `tailwindcss` catalog-bound (`.api/tailwindcss.md`): `@import "tw-animate-css"` after `@import "tailwindcss"` in the theme entry; the plugin registers its `@theme inline` tokens and `@utility` families into the same compile. Use the `./prefix` build only under a namespace-collision constraint.
- Stack with `@radix-ui/*` / `vaul` / `cmdk` data-state (sibling `.api/*.md`): the shadcn/Radix entering-leaving pattern is `data-[state=open]:animate-in data-[state=open]:fade-in-0 data-[state=open]:zoom-in-95 data-[state=closed]:animate-out data-[state=closed]:fade-out-0`; the accordion/collapsible height animations read the exact `--radix-*-content-height` these kits publish, so no bridging var is authored.
- Stack with `react-aria-components` + `tailwindcss-react-aria-components` (`.api/react-aria.md`): aria overlay/disclosure components expose `data-entering`/`data-exiting`/`data-state`; the RAC Tailwind plugin maps them to variants, and the tw-animate classes attach — `data-[entering]:animate-in data-[entering]:fade-in-0`. `react-aria` owns the presence lifecycle, tw-animate the keyframe.
- Stack with `act/transition.md` (native View Transitions): tw-animate is the per-element CSS-motion path `act` degrades to when the native View Transitions API (`<ViewTransition>`, `.api/types-react.md`) is not the chosen upgrade row; the two are alternatives at the same seam — native cross-document/shared-element transitions vs keyframe enter/exit — never layered on one element.
- Stack with `effect` `Match` (`libs/typescript/.api/effect.md`): drive a motion `ClassValue` from closed-family state (`Match.value(state).pipe(Match.when(…), Match.exhaustive)`) and fold it through `cn`, but exhaust the declarative `cva` variant / `data-state` approach first — reach for `Match` only for cross-field motion logic the table cannot express.

[LOCAL_ADMISSION]:
- Compose motions from `animate-in`/`animate-out` + axis setters + timing modifiers; never author a bespoke `@keyframes` for an enter/exit effect the six axes already express.
- Emit motion classes as `ClassValue`s through the one `cn` rail with `tailwind-merge` extended for the tw-animate groups; never inline conflicting animation utilities without conflict resolution, and never a second class helper beside `cn`.
- Bind stateful motion to the headless kit's `data-state`/`data-entering` attributes (`animate-accordion-*` to Radix content-height); never hand-measure element height or drive presence with a manual timeout.
- Gate reduced-motion at the consumer with Tailwind core's `motion-reduce:animate-none` (or the `intl`/preferences plane); the plugin does not auto-respect `prefers-reduced-motion`.
- Choose the native View Transitions upgrade row XOR tw-animate for a given seam; never stack a keyframe enter/exit under a native view-transition on the same element.

[RAIL_LAW]:
- Package: `tw-animate-css`
- Owns: the enter/exit motion mechanism — the `enter`/`exit` keyframe pair over six `--tw-enter/exit-*` axes, the `animate-in`/`animate-out` triggers, the per-axis setter utilities (`fade`/`zoom`/`spin`/`blur`/`slide`), the timing modifiers (`animation-duration`/`delay`/`repeat`/`direction`/`fill-mode`/`play-state`), the `@theme` timing/percentage scales, and the three named component animations (accordion/collapsible/caret) pre-wired to headless-kit content-height vars
- Accept: a trigger + axis-setter + timing-modifier composition, arbitrary `-*` values against the bounded scales, `data-state`/`data-entering`-bound stateful motion, classes folded through the extended `cn` rail, the `./prefix` build under namespace collision, Tailwind catalog-bound as the compile host
- Reject: a hand-authored `@keyframes` for an enter/exit effect the axes express, a bare `animate-in` with no setter, conflicting animation utilities without `tailwind-merge` extension, hand-measured height instead of the kit content-height var, missing `motion-reduce:` gating, stacking tw-animate under a native View Transition on one element, Tailwind catalog-bound
