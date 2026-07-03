# [API_CATALOGUE] @tailwindcss/vite

`@tailwindcss/vite` is the Tailwind v4 Vite plugin — a thin host adapter over `@tailwindcss/oxide` (the Rust/napi `Scanner` that detects utility candidates) and `@tailwindcss/node` (the compiler plus the Lightning CSS `optimize` pass), lockstep-pinned to the same `4.3.2` as `tailwindcss`. `tailwindcss(opts?)` returns a three-plugin `Plugin[]` (`:scan` pre-resolve, `:generate:serve` dev, `:generate:build` production+minify), spread into `UserConfig.plugins`. The typed surface is deliberately one factory + one option type + one field; the depth is the ABI, the dev/build/scan decomposition, the CSS-first directive contract (no `tailwind.config.js`), and the build-time/runtime split where this plugin EMITS the stylesheet and the `ui` folder owns the runtime class composition.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@tailwindcss/vite`
- package: `@tailwindcss/vite`
- version: `4.3.2`
- license: `MIT`
- module: `exports["."]` = `{ types: ./dist/index.d.mts, default: ./dist/index.mjs }` — ESM-only (`.mjs`/`.mts`, no CJS entry); the default export is the plugin factory, `PluginOptions` the only named type export
- asset: build-time Vite plugin returning `Plugin[]` — deps `@tailwindcss/node@4.3.2` (the `compile`/`optimize`/`Instrumentation` compiler), `@tailwindcss/oxide@4.3.2` (the Rust `Scanner` content detector), and `tailwindcss@4.3.2` (the core), all lockstep; peer dep `vite ^5.2.0 || ^6 || ^7 || ^8`; no `engines` floor. The plugin owns scanning + generation + HMR only — no runtime surface ships to the browser
- rail: build/css — Tailwind v4 CSS-first scanning, generation, and HMR

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: plugin options — the complete typed surface
- rail: build/css
- `PluginOptions` is the only importable type; `optimize` is the sole field and gates the production Lightning CSS pass.

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]  | [BOUNDARY_NOTE]                                                                          |
| :-----: | :-------------- | :------------- | :--------------------------------------------------------------------------------------- |
|  [01]   | `PluginOptions` | options object | `{ optimize?: boolean \| { minify?: boolean } }` — Lightning CSS optimize/minify control |

[PUBLIC_TYPE_SCOPE]: `optimize` resolution — the build-pass gate
- rail: build/css
- Resolved in the `:scan` plugin's `configResolved`: optimize defaults ON; `optimize: false` disables the whole pass; minify follows Vite's `build.cssMinify` unless `optimize.minify` overrides it.

| [INDEX] | [FIELD]           | [TYPE]                       | [DEFAULT]                                                        |
| :-----: | :---------------- | :--------------------------- | :--------------------------------------------------------------- |
|  [01]   | `optimize`        | `boolean \| { minify? }`     | optimize ON; `false` disables; Lightning CSS runs in `apply:build` |
|  [02]   | `optimize.minify` | `boolean` (when object)      | `build.cssMinify !== false` — minify follows Vite unless overridden |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: plugin factory — the three-plugin array
- rail: build/css
- Spread the return (never wrap as a single element); all three sub-plugins are `enforce: "pre"`, so Tailwind's CSS generation runs ahead of every other plugin transform.

```ts
// dist/index.d.mts — the entire typed surface
type PluginOptions = { optimize?: boolean | { minify?: boolean } };
declare function tailwindcss(opts?: PluginOptions): Plugin[];
export { type PluginOptions, tailwindcss as default };
```

| [INDEX] | [SUB_PLUGIN]                     | [SELECTOR]                    | [BOUNDARY_NOTE]                                                       |
| :-----: | :------------------------------- | :---------------------------- | :-------------------------------------------------------------------- |
|  [01]   | `@tailwindcss/vite:scan`         | `enforce:"pre"`               | `configResolved` resolves `optimize`/`minify`; `configureServer` wires dev |
|  [02]   | `@tailwindcss/vite:generate:serve` | `apply:"serve"`, `enforce:"pre"` | `transform` CSS via the Oxide `Scanner`; `hotUpdate` full-reload on candidate change |
|  [03]   | `@tailwindcss/vite:generate:build` | `apply:"build"`, `enforce:"pre"` | `transform` CSS then the Lightning CSS `optimize`/minify pass         |

## [04]-[IMPLEMENTATION_LAW]

[CSS_TOPOLOGY]:
- Tailwind v4 is CSS-first: no `tailwind.config.js`. Configuration lives in the CSS entry via `@import "tailwindcss"`, `@theme` (design tokens), `@source` (scan-root overrides), `@plugin` (variant/plugin registration), `@utility`, and `@variant`. The Oxide `Scanner` auto-detects content; `@source` overrides the root set.
- the plugin is a thin adapter: `@tailwindcss/oxide`'s `Scanner` finds candidates, `@tailwindcss/node`'s `compile` builds the CSS, and its `optimize` runs Lightning CSS — no PostCSS, cssnano, or autoprefixer pass is involved.
- the dev/build split keys on Vite's `apply`: `:generate:serve` owns incremental rebuild + HMR (`hotUpdate` issues a full reload when the candidate set changes), `:generate:build` adds the optimize/minify pass; both share the Oxide scan.

[INTEGRATION_LAW]:
- Stack with `vite.md`: spread the `Plugin[]` into `UserConfig.plugins`; because all three sub-plugins are `enforce: "pre"`, place `tailwindcss()` FIRST — ahead of `@vitejs/plugin-react` — so CSS generation precedes the React/asset transforms. `optimize.minify` composes with Vite's `build.cssMinify` (Lightning CSS via `@tailwindcss/node`), never a separate minifier row.
- Build-time-only within `Shell/build.md` `BuildPipeline`: one plugin row in the closed set, owning scanning + generation + HMR and holding no runtime surface.
- Cross-folder runtime split with `ui/.api/tailwindcss.md`: this plugin EMITS the stylesheet (`@import "tailwindcss"` -> preflight + generated utilities from the scanned candidates); the `ui` folder owns the RUNTIME class composition — `tailwind-merge`/`class-variance-authority` conflict resolution, and `tailwindcss-react-aria-components`/`tw-animate-css` registered via `@plugin` in the CSS entry (never here). The `@theme` tokens and `@plugin` registrations in the compiled CSS entry are authored by `ui` and referenced here as settled.
- No Effect stacking: a build-time host adapter, not a runtime service — it never enters the `ManagedRuntime` layer graph.

[RAIL_LAW]:
- Package: `@tailwindcss/vite`
- Owns: Tailwind v4 CSS-first source scanning (Oxide), on-demand CSS generation, Lightning CSS optimize/minify, and Vite dev-server HMR integration
- Accept: `PluginOptions` (`optimize`) to `tailwindcss()`; the `Plugin[]` spread first into `UserConfig.plugins`; the `@theme`/`@source`/`@plugin` CSS-entry contract authored by `ui`
- Reject: a `tailwind.config.js` or PostCSS-based Tailwind pipeline alongside this plugin; wrapping the `Plugin[]` as one element; authoring runtime class-merge or variant logic here (the `ui` concern); a CJS `require` (ESM-only package)
