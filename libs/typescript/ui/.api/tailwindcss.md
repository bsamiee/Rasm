# [TS_UI_API_TAILWINDCSS]

`tailwindcss` is a CSS-first styling engine: an `@theme` namespace authored in CSS generates a CSS custom property and its utility-class family from one declaration, so a new color, size, or motion token is one line and no component hardcodes a value.

`@tailwindcss/vite` resolves the directive surface at build with zero client JS; the typed `tailwindcss/plugin` API stays for the rare algorithmic utility family, and every utility reaches a component only through the `cva`/`clsx`/`twMerge` variant dispatch.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `tailwindcss`
- package: `tailwindcss` (MIT)
- module: CSS-first stylesheets (`index.css`, `theme.css`, `preflight.css`, `utilities.css`); a dual ESM+CJS `dist/` carries the typed JS compat surface, `sideEffects: false`
- runtime: build-time CSS generation, zero client JS; `@tailwindcss/vite` peer binds the branch Vite build
- rail: token plane — the utility and token vocabulary every component styles through

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the `@theme` namespace vocabulary — one namespace generates a variable and its utility family; `token/theme` and `token/scale` are the folder owners that consume it.

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY]       | [CONSUMER]    | [CAPABILITY]                               |
| :-----: | :----------- | :------------------ | :------------ | :----------------------------------------- |
|  [01]   | `--color-*`  | color namespace     | `token/theme` | OKLCH palette emitting color utilities     |
|  [02]   | `--font-*`   | type namespace      | `token/scale` | font family weight spacing leading         |
|  [03]   | `--text-*`   | text-size namespace | `token/scale` | type scale with paired line-height         |
|  [04]   | `--spacing`  | layout namespace    | `token/scale` | spacing radius breakpoint container aspect |
|  [05]   | `--shadow-*` | effect namespace    | `token/theme` | shadow blur perspective filter tokens      |
|  [06]   | `--ease-*`   | motion namespace    | `token/scale` | easing curves and named animations         |

[TYPE]: `--font-*` `--font-weight-*` `--tracking-*` `--leading-*`
[TEXT_SIZE]: `--text-*` `--text-*--line-height`
[LAYOUT]: `--spacing` `--radius-*` `--breakpoint-*` `--container-*` `--aspect-*`
[EFFECT]: `--shadow-*` `--inset-shadow-*` `--drop-shadow-*` `--text-shadow-*` `--blur-*` `--perspective-*`
[MOTION]: `--ease-*` `--animate-*` `--default-transition-*`

[PUBLIC_TYPE_SCOPE]: the typed JS plugin surface (`tailwindcss/plugin`) — read when a token axis is authored in JS rather than `@theme` CSS.

[CONFIG]: `Config` `UserConfig` `ThemeConfig`
[PLUGIN]: `PluginAPI` `PluginFn` `PluginWithOptions<T>` `Plugin`
[CSS_OBJECT]: `CssInJs` `NamedUtilityValue` `PluginUtils`
[POLICY]: `DarkModeStrategy` `ContentFile`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the CSS-first directive and function surface `@tailwindcss/vite` resolves at build — the primary authoring rail; `@theme` blocks tokens, the `--*()` functions read them inline.

[ENGINE]: `@import "tailwindcss"`
[DEFINE]: `@theme` (`inline` substitutes, `static` emits every variable, `reference` imports without emit)
[AUTHOR]: `@utility` `@variant` `@custom-variant` `@apply`
[WIRE]: `@source` `@reference` `@plugin` `@config`
[READ]: `theme(--token)` `--spacing(n)` `--alpha(color / pct)`

[ENTRYPOINT_SCOPE]: build integration and the typed JS compat API — `@tailwindcss/vite`, `tailwindcss/plugin`, and the default-value subpaths.

| [INDEX] | [SURFACE]                                     | [SHAPE] | [CAPABILITY]                          |
| :-----: | :-------------------------------------------- | :------ | :------------------------------------ |
|  [01]   | `tailwindcss({ optimize })`                   | factory | the one Vite build integration        |
|  [02]   | `plugin(fn, config?)`                         | factory | a programmatic utility or variant set |
|  [03]   | `plugin.withOptions(fn, cfg?)`                | factory | a parameterized plugin                |
|  [04]   | `colors` `defaultTheme` `flattenColorPalette` | static  | stock palette and default objects     |

- `plugin` is the default export of `tailwindcss/plugin`, passing the `PluginAPI` (`addUtilities`/`matchUtilities`/`addVariant`/`matchVariant`/`addBase`/`addComponents`/`theme`); the default-value imports are `tailwindcss/colors`, `tailwindcss/defaultTheme`, `tailwindcss/lib/util/flattenColorPalette`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- CSS-first is the law, JS config the compat: `@import "tailwindcss"` is the single entry, `@theme` declares tokens, `@tailwindcss/vite` resolves the directive surface at build, and `tailwindcss/plugin` serves only an algorithmic utility family CSS cannot enumerate.
- One `@theme` namespace generates a CSS variable and its whole utility family from one declaration, so a color, size, radius, easing, or animation lands as one line, readable back through `theme()` or raw `var()`.
- Variants are data-attribute selectors: `@custom-variant dark (&:where([data-theme=dark] *))` binds the theme selector and react-aria states become variants through the tw-rac plugin, so a component styles state declaratively with no JS class branching.
- One `--spacing` multiplier scales every `p-`/`m-`/`gap-`/`w-`/`h-` utility, so a density change is one token edit; `--text-*` pairs each step with its line-height and `--breakpoint-*`/`--container-*` drive responsive and container-query variants.

[STACKING]:
- `colorjs.io` (`.api/colorjs.io.md`): `@theme --color-*` OKLCH values are computed and gamut-mapped through `colorjs.io`, so the palette is a color-space artifact tailwind emits utilities from.
- `class-variance-authority` (`.api/class-variance-authority.md`) + `clsx` (`.api/clsx.md`): `cva` maps typed variant props to utility strings and `clsx` folds the conditionals — the dispatch layer between token vocabulary and rendered component.
- `tailwind-merge` (`.api/tailwind-merge.md`): `twMerge` resolves conflicting utilities last-wins; a customized `@theme` teaches ONE shared `extendTailwindMerge(fromTheme(...))` instance the custom groups, else custom-token conflicts survive un-deduplicated.
- `tailwindcss-react-aria-components` (`.api/tailwindcss-react-aria-components.md`): loaded via `@plugin`, it exposes react-aria states as `selected:`/`pressed:`/`focus-visible:` variants binding the primitive spine to the token vocabulary.
- `tw-animate-css` (`.api/tw-animate-css.md`): imported alongside `tailwindcss`, it contributes the `--animate-*` keyframe rows the motion scale composes.
- `effect` `Schema` (`libs/typescript/.api/effect.md`): the `Theme` token record is a `Schema`-typed value; a theme switch is a validated write of the `data-theme` attribute the `@custom-variant` selectors key on.

[LOCAL_ADMISSION]:
- Author every color, size, radius, easing, and animation as one `@theme` line and consume its generated variable and utilities.
- Reserve `tailwindcss/plugin` for an algorithmic utility family over a computed value set (`matchUtilities`); `@utility` and `@theme` own every static utility.
- Bind interaction and theme state through `@custom-variant` and the react-aria-components plugin, expressed as data-attribute variants.
- Compose every conflict-prone class string through the `cva`/`clsx`/`twMerge` rail and wire the build through `@tailwindcss/vite`.

[RAIL_LAW]:
- Package: `tailwindcss` + `@tailwindcss/vite`
- Owns: the `@theme` token-namespace vocabulary, the CSS directive and function surface, and the typed JS compat plugin API and Vite build
- Accept: CSS-first `@theme` token authoring, one namespace generating variable and utilities, data-attribute variants, the single `--spacing` scale, `cva`/`clsx`/`twMerge` dispatch, `colorjs.io`-computed OKLCH values
- Reject: a `tailwind.config.js` for CSS-authorable values, a hand-written utility or raw `var()` where a namespace emits the pair, JS class-string branching for a data-attribute-expressible state, raw class concatenation bypassing `twMerge`
