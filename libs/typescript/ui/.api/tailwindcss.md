# [tailwindcss] — utility styling substrate the token plane compiles into

`tailwindcss` v4 is a CSS-first styling engine, not a JavaScript config library: the design-token plane authors `@theme` namespaces directly in CSS, and each namespace *generates both* a CSS custom property and its utility-class family. `token/theme` compiles into the `--color-*`/`--font-*` namespaces (OKLCH values, project hues extending the stock palette), and `token/scale` into `--spacing`/`--text-*`/`--radius-*`/`--ease-*`/`--animate-*`/`--breakpoint-*` — so declaring one token emits one variable plus its utilities, and no `ui` component hardcodes a color or size. The engine is driven by CSS at-rule directives (`@import "tailwindcss"`, `@theme`, `@utility`, `@variant`, `@custom-variant`, `@apply`, `@source`, `@reference`, `@plugin`, `@config`) resolved at build time by `@tailwindcss/vite`; the legacy JavaScript surface (`tailwindcss/plugin`, `tailwindcss/colors`, `tailwindcss/defaultTheme`) survives as a typed compat layer for programmatic plugins. Utilities never reach a component raw — they flow through the `cva`/`clsx`/`tailwind-merge` variant dispatch, and react-aria component states become variants through `tailwindcss-react-aria-components`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `tailwindcss`
- package: `tailwindcss` (4.3.2, MIT, © Tailwind Labs) — the CSS engine; the Vite integration `@tailwindcss/vite` (4.3.2, MIT) is the build owner
- module format: CSS-first — the primary artifact is the shipped stylesheet set (`index.css`, `theme.css`, `preflight.css`, `utilities.css`); a dual ESM+CJS (`type: commonjs`) `dist/` carries the typed JS compat surface, `sideEffects: false`
- runtime target: build-time CSS generation (zero runtime, zero client JS); the browser receives only generated CSS custom properties + utility classes
- peer: none for the engine; `@tailwindcss/vite` peers `vite@^5.2||^6||^7||^8`, satisfied by the branch Vite build
- asset: CSS-first — directives + `@theme` namespaces verified from the shipped `theme.css`/`index.css` and the official v4 directive reference; the JS compat surface (`tailwindcss/plugin`, `tailwindcss/colors`, `tailwindcss/defaultTheme`, `tailwindcss/lib/util/flattenColorPalette`) IS typed and TSDECL-reflectable via `dist/*.d.ts` (`plugin.d.ts`, `colors.d.ts`, `default-theme.d.ts`)
- rail: token plane — the utility/token vocabulary the `token/theme` and `token/scale` rows compile into and every component styles through

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the `@theme` namespace vocabulary — one namespace generates a variable + its utilities
- rail: token

| [INDEX] | [SYMBOL]                                                       | [TYPE_FAMILY]     | [CONSUMER]                                                        |
| :-----: | :------------------------------------------------------------- | :---------------- | :--------------------------------------------------------------- |
|  [01]   | `--color-*`                                                    | color namespace   | `token/theme` — each `--color-<hue>-<step>` (OKLCH) emits `bg-`/`text-`/`border-`/`ring-`/… utilities; the plane extends the stock palette with project hues (this install carries `mauve`/`olive`/`mist`/`taupe` beyond the base) |
|  [02]   | `--font-*` / `--font-weight-*` / `--tracking-*` / `--leading-*` | type namespace   | `token/scale` — font families, weights, letter-spacing, line-height; drive `font-`/`tracking-`/`leading-` utilities |
|  [03]   | `--text-*` (+ paired `--text-*--line-height`)                  | text-size namespace | `token/scale` — the type scale; each step emits a `text-<step>` utility carrying its default line-height |
|  [04]   | `--spacing` / `--radius-*` / `--breakpoint-*` / `--container-*` / `--aspect-*` | layout namespace | `token/scale` — the single `--spacing` multiplier drives every `p-`/`m-`/`gap-`/`w-`/`h-`; radius, responsive breakpoints, container queries, aspect ratios |
|  [05]   | `--shadow-*` / `--inset-shadow-*` / `--drop-shadow-*` / `--text-shadow-*` / `--blur-*` / `--perspective-*` | effect namespace | `token/theme` — elevation, inner shadow, filter blur, and 3D perspective token families |
|  [06]   | `--ease-*` / `--animate-*` / `--default-transition-*`          | motion namespace  | `token/scale` — easing curves and named keyframe animations (the `tw-animate-css` motion rows land here); the motion-token half of the scale plane |

[PUBLIC_TYPE_SCOPE]: the typed JS plugin surface (`tailwindcss/plugin`)
- rail: token

| [INDEX] | [SYMBOL]                                                       | [TYPE_FAMILY]     | [CONSUMER]                                                        |
| :-----: | :------------------------------------------------------------- | :---------------- | :--------------------------------------------------------------- |
|  [01]   | `Config` / `UserConfig` / `ThemeConfig`                        | config type       | `token/theme` — the theme-config shape when a token axis is authored in JS rather than `@theme` CSS (rare; the CSS path is canonical) |
|  [02]   | `PluginAPI` / `PluginCreator` / `PluginWithOptions<T>`         | plugin type       | `token/scale`, `token/theme` — the API a programmatic plugin receives; `withOptions` is the parameterized-plugin variant |
|  [03]   | `CssInJs` / `NamedUtilityValue` / `PluginUtils`                | css-object type    | `token/scale` — the CSS-in-JS object shape `addUtilities`/`matchUtilities`/`addComponents` consume |
|  [04]   | `DarkModeStrategy` / `ContentFile`                             | policy type        | `token/theme` — dark-mode selector strategy and the content-source descriptor (the `@custom-variant dark` + `@source` CSS equivalents) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the CSS-first directive surface — the primary authoring rail
- rail: token

| [INDEX] | [SURFACE]                                                                                       | [ENTRY_FAMILY] | [CONSUMER]                                                    |
| :-----: | :---------------------------------------------------------------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `@import "tailwindcss";`                                                                        | engine entry   | `token/theme` — the one entry pulling preflight + theme + utilities; the branch's single global stylesheet imports it |
|  [02]   | `@theme { --color-brand-500: oklch(...); }` / `@theme inline` / `@theme static` / `@theme reference` | define tokens | `token/theme`, `token/scale` — declare token namespaces; `inline` substitutes values, `reference` imports without emitting variables, `static` emits every variable unconditionally |
|  [03]   | `@utility name { … }` / `@variant hover { … }` / `@custom-variant dark (&:where([data-theme=dark] *))` + `@slot` | custom utility/variant | `token/scale`, `act/gesture` — author a token-driven utility, apply a variant inside custom CSS, or define a data-attribute-driven variant (the theme/state selectors) |
|  [04]   | `@apply bg-surface text-fg` / `theme(--color-brand-500)` / `--spacing(4)` / `--alpha(var(--color-fg) / 60%)` | inline / read | `view/primitive` — inline existing utilities into a component base layer; `theme()`/`--spacing()` read token values and `--alpha()` derives an OKLCH opacity variant (`color-mix` in oklab) inside arbitrary CSS |
|  [05]   | `@source "../app"` / `@reference "../theme.css"` / `@plugin "./rac"` / `@config "./legacy.js"`  | wiring         | `token/theme` — declare class-detection sources, import theme into a scoped stylesheet without duplication, and load a JS plugin/legacy config |

[ENTRYPOINT_SCOPE]: build integration and the typed JS compat API
- rail: token

| [INDEX] | [SURFACE]                                                                                       | [ENTRY_FAMILY] | [CONSUMER]                                                    |
| :-----: | :---------------------------------------------------------------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `import tailwindcss from '@tailwindcss/vite'` → `tailwindcss({ optimize })` in `vite.config` plugins | build plugin | the app Vite build — the one integration point; scans sources, resolves `@theme`/`@utility`, emits optimized CSS |
|  [02]   | `import plugin from 'tailwindcss/plugin'` → `plugin(({ addUtilities, matchUtilities, addVariant, matchVariant, addBase, addComponents, theme }) => …, config?)` | JS plugin | `token/scale` — a programmatic utility/variant generator when a token family is algorithmic rather than enumerated in `@theme` |
|  [03]   | `plugin.withOptions((opts) => api => …, (opts) => config)`                                      | parameterized plugin | `token/scale` — a plugin accepting options; the parameterized form when one plugin serves many token configurations |
|  [04]   | `import colors from 'tailwindcss/colors'` / `import defaultTheme from 'tailwindcss/defaultTheme'` / `tailwindcss/lib/util/flattenColorPalette` | default values | `token/theme` — the stock OKLCH palette and default theme objects, and the palette-flatten util, read when seeding or extending `@theme` from JS |

## [04]-[IMPLEMENTATION_LAW]

[TOKEN_TOPOLOGY]:
- CSS-first is the law, JS config is compat: the token plane authors `@theme` namespaces in the branch stylesheet, not a `tailwind.config.js`. `@import "tailwindcss"` is the single entry; `@theme` declares tokens; `@tailwindcss/vite` resolves it at build time. The `tailwindcss/plugin` JS surface is reserved for the rare algorithmic utility family that cannot be enumerated in CSS.
- One namespace generates a variable *and* its utilities: declaring `--color-brand-500: oklch(...)` emits both the `--color-brand-500` custom property (readable via `theme()` or raw `var()`) and the `bg-brand-500`/`text-brand-500`/`border-brand-500`/`ring-brand-500`/… utility family. This is the single parameterized token mechanism — a new color, size, radius, easing, or animation is one `@theme` line, never a hand-written utility or a config-object edit. The `--color-*`/`--font-*`/`--text-*`/`--spacing`/`--radius-*`/`--ease-*`/`--animate-*` roster is seed data feeding that one generation rule.
- Variants are data-attribute selectors, not JS conditionals: `@custom-variant dark (&:where([data-theme=dark] *))` binds the theme selector; react-aria component states (`data-selected`, `data-pressed`, `data-focus-visible`) become variants through the `tailwindcss-react-aria-components` plugin, so a component styles its states declaratively (`selected:bg-accent`) with no JS class branching.
- The single `--spacing` multiplier scales the whole layout system: every `p-`/`m-`/`gap-`/`w-`/`h-`/`size-` utility derives from one `--spacing` value, so a density change is one token edit, not a scale rewrite. The `--text-*` scale pairs each step with its line-height, and `--breakpoint-*`/`--container-*` drive responsive + container-query variants from the same namespace mechanism.
- Utilities never reach a component raw: the class string flows through `cva` (variant→class dispatch), `clsx` (conditional fold), and `tailwind-merge` (conflict resolution), so a component's styling is a typed variant surface, not a hand-concatenated class string. `token/theme` owns the token values; the components own only variant selection.

[STACKS_WITH]:
- `colorjs.io` (`.api/colorjs.io.md`): the OKLCH token values in `@theme --color-*` are generated and range-checked through `colorjs.io` (gamut mapping, contrast, interpolation), so the palette is a computed color-space artifact fed into the CSS namespace — the token plane authors color *math*, tailwind emits the *utilities*.
- `class-variance-authority` (`.api/class-variance-authority.md`) + `clsx` (`.api/clsx.md`): `cva` maps a component's typed variant props (size/tone/state) to tailwind utility strings and `clsx` folds the conditionals; this is the dispatch layer between the token vocabulary and the rendered component.
- `tailwind-merge` (`.api/tailwind-merge.md`): `twMerge` deterministically resolves conflicting utilities (a later `p-4` wins over an earlier `p-2`) so composed/overridden class strings collapse to one coherent set — the required last step whenever `ui` merges base + override classes. A customized `@theme` (custom `--color-*`/`--spacing`/`--radius-*` scales) is unknown to the default `twMerge`, so ONE shared `extendTailwindMerge(fromTheme(...))` instance teaches it the custom utility groups — otherwise conflicts on custom-token utilities silently survive un-deduplicated.
- `tailwindcss-react-aria-components` (`.api/tailwindcss-react-aria-components.md`): loaded via `@plugin`, it exposes react-aria component render-prop states as tailwind variants (`selected:`/`pressed:`/`focus-visible:`/`disabled:`), binding the headless interaction states of the `view/primitive` spine to the token vocabulary.
- `tw-animate-css` (`.api/tw-animate-css.md`): imported alongside `tailwindcss`, it contributes the `--animate-*` keyframe/motion token rows the `token/scale` motion half composes, so enter/exit/attention animations are tokens, not per-component keyframes.
- `effect` `Schema` (`.api/effect.md`): the `Theme`/token record is a `Schema`-typed value in `token/theme`; a theme switch is a validated data change writing the `data-theme` attribute the `@custom-variant` selectors key on, keeping the token model typed end-to-end.

[LOCAL_ADMISSION]:
- Author tokens in `@theme` CSS namespaces; never introduce a `tailwind.config.js` for values that belong in CSS — the config path is compat, the CSS path is canonical.
- Add a color/size/radius/easing/animation as one `@theme` line and consume its generated utilities/variable; never hand-write a utility class or a raw `var(--…)` where the namespace already emits the pair.
- Reach for `tailwindcss/plugin` only for an algorithmic utility family (`matchUtilities` over a computed value set); never wrap a static utility list in a JS plugin where `@utility`/`@theme` fits.
- Bind interaction/theme variants through `@custom-variant` + the react-aria-components plugin; never branch class strings in JS for a component state a data-attribute variant expresses.
- Pass every composed class string through `cva`/`clsx`/`twMerge`; never concatenate raw utility strings that can silently conflict.
- Wire the build through `@tailwindcss/vite`; never hand-run the CLI or add a PostCSS pipeline where the Vite plugin owns generation.

[RAIL_LAW]:
- Package: `tailwindcss` (v4 CSS engine) + `@tailwindcss/vite` (build owner)
- Owns: the `@theme` token-namespace vocabulary (color/type/spacing/effect/motion), the CSS directive surface (`@import`/`@theme`/`@utility`/`@variant`/`@custom-variant`/`@apply`/`@source`/`@reference`/`@plugin`/`@config`), the `theme()`/`--spacing()`/`--alpha()` CSS functions, the typed JS compat plugin API, and the Vite build integration
- Accept: CSS-first token authoring in `@theme`, one namespace generating variable + utilities, data-attribute variants for theme/state, the single `--spacing` scale, `cva`/`clsx`/`twMerge` variant dispatch, `colorjs.io`-computed OKLCH values, react-aria-components state variants via `@plugin`, `@tailwindcss/vite` build ownership
- Reject: `tailwind.config.js` for CSS-authorable values, hand-written utilities or raw `var()` where a namespace emits the pair, JS plugins wrapping static utility lists, JS class-string branching for data-attribute-expressible states, raw class concatenation bypassing `twMerge`, a hand-run CLI or PostCSS pipeline beside the Vite plugin
