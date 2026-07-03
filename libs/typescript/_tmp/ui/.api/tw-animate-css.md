# [API_CATALOGUE] tw-animate-css

`tw-animate-css` is a Tailwind CSS v4 utility layer supplying enter/exit, fade, zoom, slide, spin, and `data-[state]`-keyed animation utilities — the v4-native replacement for `tailwindcss-animate`. It exports no JavaScript runtime surface: it is a single CSS layer imported into the design-token stylesheet through `@import "tw-animate-css"`, after which the `animate-in`/`animate-out`, `fade-*`, `zoom-*`, `slide-in-from-*`, `spin`, and `duration-*`/`delay-*`/`ease-*` utility families and the `data-[state=open]`/`data-[state=closed]` enter/exit selectors are available to every Tailwind class consumer. A prefixed build is exported at `tw-animate-css/prefix` for collision-free composition.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `tw-animate-css`
- package: `tw-animate-css`
- namespace: `tw-animate-css`
- asset: CSS utility layer (no JS runtime)
- rail: styling-animation

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: none

The package exposes no TypeScript surface; it is consumed as a CSS `@import` and emits utility classes resolved by the Tailwind v4 engine at build time.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: CSS import targets
- rail: styling-animation

| [INDEX] | [SURFACE]               | [ENTRY_FAMILY] | [RAIL]                                                       |
| :-----: | :---------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `tw-animate-css`        | CSS layer      | unprefixed `animate-*`/`fade-*`/`zoom-*`/`slide-*` utilities |
|  [02]   | `tw-animate-css/prefix` | CSS layer      | prefixed build for collision-free composition                |

## [04]-[IMPLEMENTATION_LAW]

[ANIMATION_TOPOLOGY]:
- `@import "tw-animate-css";` lands in the same stylesheet that declares the `@theme` design-token layer, after the Tailwind `@import "tailwindcss"` directive
- `animate-in` / `animate-out` are the entry/exit base utilities; compose with `fade-in-0`/`fade-out-0`, `zoom-in-95`/`zoom-out-95`, `slide-in-from-top-2`/`slide-in-from-bottom-2`, and `spin` for the full keyframe set
- `data-[state=open]:animate-in` / `data-[state=closed]:animate-out` bind the enter/exit keyframes to the Radix-style `data-state` attribute every `@radix-ui/*`-derived overlay (`cmdk` `Command.Dialog`, `vaul` `Drawer`) emits
- `duration-*`, `delay-*`, and `ease-*` utilities control timing; `fill-mode-*` controls the post-animation retained state
- the layer is reduced-motion-aware: the keyframe utilities collapse under `@media (prefers-reduced-motion: reduce)` so a motion-suppressed render is correct without a JS gate
- emits no JavaScript; the entire surface is utility class names resolved against the Tailwind v4 `@theme` token values, so token-driven durations and easings flow from the OKLCH token layer

[LOCAL_ADMISSION]:
- Import once at the design-token stylesheet root, never per component; the utilities are global once the layer is present.
- Drive overlay enter/exit through `data-[state]` utilities so the animation rides the component's own `data-state` attribute rather than a JS animation controller.
- Use the View Transitions enter/exit class hook for route-level transitions; reserve this layer for component-local `data-state` enter/exit, not route choreography.

[RAIL_LAW]:
- package: `tw-animate-css`
- owns: the Tailwind v4 enter/exit/fade/zoom/slide/spin animation utility layer keyed by `data-state`
- accept: the CSS `@import`, the `animate-in`/`animate-out` + modifier utilities, the `data-[state]` enter/exit selectors, the prefixed build
- reject: hand-authored `@keyframes` for enter/exit that this layer already supplies; a JS animation controller for a `data-state` transition the utility layer drives declaratively
