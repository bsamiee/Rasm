# [TS_UI_API_TW_ANIMATE_CSS]

`tw-animate-css` owns one parameterized enter/exit motion mechanism: a single `enter`/`exit` `@keyframes` pair reads six typed custom-property axes, and every animation is an `animate-in`/`animate-out` trigger composed with per-axis setter utilities and orthogonal timing modifiers. It ships three named component animations wired to headless-kit content-height vars. Boundary: a pure-CSS Tailwind plugin — no JS, no types, no ABI — consumed as the utility-class and custom-property vocabulary Tailwind compiles.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `tw-animate-css`
- package: `tw-animate-css` (MIT)
- module: pure CSS — ships `dist/tw-animate.css` (default) and `dist/tw-animate-prefix.css` (`./prefix`, every utility namespaced under `tw-`); no `types`, no JS, nothing binds `tsc`
- runtime: build-time Tailwind compile, zero client JS; `tailwindcss` peer is the hard compile host authoring `@utility`, `@theme inline`, `--value()`, `@property` primitives
- rail: token/scale — the enter/exit motion-class vocabulary a `cn`-folded `view`/`token` row writes

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the enter/exit axis registers — six `@property`-typed custom properties

`@property` declares each axis with a syntax and initial value so it animates and inherits; the `enter`/`exit` keyframes read exactly these six axes (× enter/exit), the collapse point that yields two keyframes for the whole surface. Each cell names the setter that drives the axis and its keyframe effect.

| [INDEX] | [SYMBOL]                                                              | [TYPE_FAMILY]    | [CAPABILITY]                              |
| :-----: | :-------------------------------------------------------------------- | :--------------- | :---------------------------------------- |
|  [01]   | `--tw-enter-opacity` / `--tw-exit-opacity` (`@property`, initial `1`) | opacity axis     | `fade-in`/`fade-out`; keyframe `opacity`  |
|  [02]   | `--tw-enter-scale` / `--tw-exit-scale` (initial `1`)                  | scale axis       | `zoom-in`/`zoom-out`; `scale3d(...)`      |
|  [03]   | `--tw-enter-rotate` / `--tw-exit-rotate` (initial `0`)                | rotate axis      | `spin-in`/`spin-out`; `rotate(...)`       |
|  [04]   | `--tw-enter-blur` / `--tw-exit-blur` (initial `0`)                    | blur axis        | `blur-in`/`blur-out`; `filter: blur(...)` |
|  [05]   | `--tw-enter-translate-x` / `--tw-exit-translate-x` (initial `0`)      | x-translate axis | `slide-*-left/right/start/end`; `x`       |
|  [06]   | `--tw-enter-translate-y` / `--tw-exit-translate-y` (initial `0`)      | y-translate axis | `slide-*-top/bottom`; `y`                 |

[PUBLIC_TYPE_SCOPE]: the timing registers and theme-token scales

`--animate-in`/`--animate-out` are the master `@theme` animations that become the `animate-in`/`animate-out` utilities; the `@property`-typed `--tw-animation-*` axes and the `--animation-*`/`--percentage-*` scales are the bounded vocabularies the setter and modifier utilities resolve against.

| [INDEX] | [SYMBOL]                                                                    | [TYPE_FAMILY]    | [CAPABILITY]                          |
| :-----: | :-------------------------------------------------------------------------- | :--------------- | :------------------------------------ |
|  [01]   | `--animate-in` / `--animate-out` (`@theme`, = `enter`/`exit` + timing vars) | master animation | the `animate-in`/`out` utilities      |
|  [02]   | `--tw-animation-{delay,direction,duration,fill-mode,iteration-count}`       | timing axis      | set by the timing modifiers           |
|  [03]   | `--animate-accordion-down/up`                                               | named animation  | accordion — `view/primitive`          |
|  [04]   | `--animate-collapsible-down/up` / `--animate-caret-blink`                   | named animation  | collapsible, caret — `view/primitive` |
|  [05]   | `--animation-delay-{0,75,100,150,200,300,500,700,1000}`                     | delay scale      | `delay-*`; `delay-[…]` arbitrary      |
|  [06]   | `--animation-repeat-{0,1,infinite}`                                         | timing scale     | `repeat-*` values                     |
|  [07]   | `--animation-direction-{normal,reverse,alternate,alternate-reverse}`        | timing scale     | `direction-*` values                  |
|  [08]   | `--animation-fill-mode-{none,forwards,backwards,both}`                      | timing scale     | `fill-mode-*` values                  |
|  [09]   | `--percentage-{0,5,…,100}` / `--percentage-translate-full`                  | fraction scale   | `fade-*`/`zoom-*` percentages         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the trigger and per-axis setter utilities

`animate-in`/`animate-out` bind the `enter`/`exit` keyframe and each setter writes one `--tw-enter/exit-*` axis; every setter carries an arbitrary-value `-*` variant (`fade-in-[.3]`, `slide-in-from-top-[10px]`), and `slide` `start`/`end` resolve dir-aware logical edges through `:dir(ltr/rtl)`.

| [INDEX] | [SURFACE]                                                                            | [SHAPE]          | [CAPABILITY]                  |
| :-----: | :----------------------------------------------------------------------------------- | :--------------- | :---------------------------- |
|  [01]   | `animate-in` / `animate-out`                                                         | trigger          | binds keyframe; inert alone   |
|  [02]   | `fade-in` / `fade-in-<0..100>` / `fade-out` / `fade-out-*`                           | opacity setter   | `-opacity`; `fade-in-0` = 0   |
|  [03]   | `zoom-in` / `zoom-in-<n>` / `-zoom-in-*` / `zoom-out` / `zoom-out-*` / `-zoom-out-*` | scale setter     | `-scale`; neg mirror          |
|  [04]   | `spin-in` / `spin-in-<deg>` / `-spin-in` / `spin-out` / `spin-out-*` / `-spin-out-*` | rotate setter    | `-rotate`; 30deg default      |
|  [05]   | `blur-in` / `blur-in-<n>` / `blur-out` / `blur-out-*`                                | blur setter      | `-blur`; 20px default         |
|  [06]   | `slide-in-from-{top,bottom,left,right,start,end}` (+ `-*`) / `slide-out-to-{…}`      | translate setter | `-translate-{x,y}`; dir-aware |

[ENTRYPOINT_SCOPE]: the timing modifiers and named component animations

Timing modifiers set the `--tw-animation-*` axes the master `--animate-in/out` reads, composing orthogonally to the setters; `delay-*` sets `animation-delay`, never `transition-delay`. Named animations are complete `--animate-*` theme entries: accordion and collapsible read the headless kit's content-height var, caret is a self-contained opacity loop.

| [INDEX] | [SURFACE]                                                          | [SHAPE]         | [CAPABILITY]                            |
| :-----: | :----------------------------------------------------------------- | :-------------- | :-------------------------------------- |
|  [01]   | `animation-duration-<n>` / `delay-<n>` / `repeat-<0\|1\|infinite>` | timing modifier | duration, delay, iteration count        |
|  [02]   | `direction-<normal\|reverse\|alternate\|alternate-reverse>`        | timing modifier | animation direction                     |
|  [03]   | `fill-mode-<none\|forwards\|backwards\|both>`                      | timing modifier | `forwards` holds the exit state         |
|  [04]   | `running` / `paused` / `play-state-*`                              | play state      | pause and resume motion — `act/gesture` |
|  [05]   | `animate-accordion-down` / `animate-accordion-up`                  | named animation | accordion height — `view/primitive`     |
|  [06]   | `animate-collapsible-down` / `animate-collapsible-up`              | named animation | collapsible height — `view/primitive`   |
|  [07]   | `animate-caret-blink`                                              | named animation | `cmdk` caret blink — `view/compose`     |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One mechanism, six axes: `animate-in`/`animate-out` bind the `enter`/`exit` keyframe pair and each `fade`/`zoom`/`spin`/`blur`/`slide` utility sets one `--tw-enter/exit-*` axis, so `animate-in fade-in-0 zoom-in-95 slide-in-from-top-2` is three axis setters over one keyframe and a new motion is a new setter combination; `animate-in` alone holds every axis at its identity value, so a motion is always trigger + at least one setter.
- Timing composes orthogonally: `animation-duration-*`/`delay-*`/`repeat-*`/`direction-*`/`fill-mode-*` set the `--tw-animation-*` axes the master `--animate-in/out` reads, and `--tw-animation-duration` falls back to Tailwind core's `--tw-duration`, then `.15s`.
- Named animations bind their own height or opacity keyframes outside the axis space: accordion and collapsible read the headless kit's `--radix-*`/`--bits-*`/`--reka-*`/`--kb-*`/`--ngp-*` content-height var, caret is a self-contained opacity loop.

[STACKING]:
- `class-variance-authority`(`.api/class-variance-authority.md`) + `clsx`(`.api/clsx.md`) + `tailwind-merge`(`.api/tailwind-merge.md`): motion classes are `ClassValue`s a `cva` motion axis carries as variant values and the one `cn = twMerge(clsx(...))` folds; extend `tailwind-merge` through `extendTailwindMerge` so the `fade`/`zoom`/`slide`/`duration`/`delay` groups take part in last-wins conflict resolution, or two conflicting `fade-in-*` both survive.
- `tailwindcss`(`.api/tailwindcss.md`): `@import "tw-animate-css"` after `@import "tailwindcss"` in the theme entry registers its `@theme inline` tokens and `@utility` families into the one compile.
- `@radix-ui/*` / `vaul` / `cmdk` data-state (`.api/vaul.md`, `.api/cmdk.md`, `.api/radix-ui-react-slot.md`): the entering-leaving pattern is `data-[state=open]:animate-in data-[state=open]:fade-in-0 data-[state=closed]:animate-out`, and the height animations read the exact `--radix-*-content-height` these kits publish, so no bridging var is authored.
- `react-aria-components` + `tailwindcss-react-aria-components`(`.api/react-aria-components.md`, `.api/tailwindcss-react-aria-components.md`): RAC exposes `data-entering`/`data-exiting`, the plugin maps them to variants, and the classes attach — `data-[entering]:animate-in data-[entering]:fade-in-0`; `react-aria` owns the presence lifecycle, tw-animate the keyframe.
- `act/transition` native View Transitions (`.api/types-react.md` `ViewTransition`): the alternative at the same seam, chosen XOR tw-animate — native cross-document or shared-element transition versus keyframe enter/exit, never layered on one element.
- `effect` `Match`(`libs/typescript/.api/effect.md`): drive a motion `ClassValue` from closed-family state through `Match.value(state).pipe(Match.when(…), Match.exhaustive)` folded through `cn`, only for cross-field motion logic the declarative `cva`/`data-state` route cannot express.

[LOCAL_ADMISSION]:
- Reach for the `./prefix` build only under a utility-name collision; the default build otherwise.
- Gate reduced motion at the consumer with Tailwind core's `motion-reduce:animate-none` — the plugin does not auto-respect `prefers-reduced-motion`.

[RAIL_LAW]:
- Package: `tw-animate-css`
- Owns: the enter/exit motion mechanism — the six-axis `enter`/`exit` keyframe pair, its `animate-in`/`animate-out` trigger and per-axis setter and timing-modifier utilities, the `@theme` timing and percentage scales, and the accordion/collapsible/caret named animations
- Accept: a trigger + setter + timing-modifier composition, arbitrary `-*` values against the bounded scales, `data-state`/`data-entering`-bound stateful motion, classes folded through the extended `cn` rail, Tailwind as the compile host
- Reject: a hand-authored `@keyframes` for an enter/exit effect the axes express, a bare `animate-in` with no setter, conflicting animation utilities without the `tailwind-merge` extension, a hand-measured height instead of the kit content-height var, a keyframe enter/exit stacked under a native View Transition on one element
