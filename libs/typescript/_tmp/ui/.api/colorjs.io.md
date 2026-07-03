# [API_CATALOGUE] colorjs.io

`colorjs.io` supplies the `Color` class and its functional twin for wide-gamut color parsing, conversion, gamut mapping, interpolation, contrast, and ΔE across 35+ registered spaces. The default import (`colorjs.io`) is the `Color` class with every method attached and the full space registry loaded; `colorjs.io/fn` exports the same operations as tree-shakeable standalone functions over `ColorTypes`; `colorjs.io/spaces` exports the individual `ColorSpace` modules for explicit registration. Contrast and ΔE are PARAMETERIZED families — `contrast(bg, algorithm)` over the `Algorithms` vocabulary and `deltaE(color2, method)` over the `Methods` vocabulary — not fixed method rosters; `ColorSpace.register` + the `hooks`/`defineFunction` seam make the space and method sets open. This is the perceptual owner behind the `theming/tokens.md#THEME_TOKENS` OKLCH scale and the `interaction/picker.md#PICKER_BEHAVIOR` color projection.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `colorjs.io`
- package / version: `colorjs.io` @ `0.6.1`
- license: `MIT`
- module: `type: module` (ESM-first) with CJS fallbacks; `sideEffects` limited to the space-registry index modules
- exports: `.` → default `Color` class (all methods + full registry) · `./fn` → tree-shakeable standalone functions over `ColorTypes` · `./spaces` → individual `ColorSpace` modules
- rail: color

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: core color value types
- rail: color

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [NOTE]                                                                          |
| :-----: | :----------------- | :------------ | :----------------------------------------------------------------------------- |
|  [01]   | `Color`            | class         | main color instance (default export); extends `SpaceAccessors` for dynamic coord access |
|  [02]   | `ColorSpace`       | class         | color-space definition and static registry                                     |
|  [03]   | `Coords`           | tuple         | `[number \| null, number \| null, number \| null]` — `null` = missing/powerless channel |
|  [04]   | `ColorTypes`       | union         | `ColorObject \| ColorConstructor \| string \| PlainColorObject` — anything coercible to a color |
|  [05]   | `PlainColorObject` | interface     | `{ space: ColorSpace; coords: Coords; alpha: number \| null }` — the canonical wire/boundary shape |
|  [06]   | `ColorObject`      | interface     | loose input: `{ coords; alpha?; spaceId? \| space? }`                          |
|  [07]   | `ColorConstructor` | interface     | `{ spaceId: string; coords: Coords; alpha }` — the `toJSON` serialization shape |
|  [08]   | `SpaceAccessor`    | type          | the dynamic per-space coord-accessor surface `Color` inherits (e.g. `.oklch.l`) |
|  [09]   | `Ref`              | union         | coord reference: `string \| [space, coord] \| { space; coordId }` — accepted by `get`/`set` |

[PUBLIC_TYPE_SCOPE]: option and interpolation types
- rail: color

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [NOTE]                                                                                          |
| :-----: | :----------------- | :------------ | :--------------------------------------------------------------------------------------------- |
|  [01]   | `RangeOptions`     | interface     | `{ space?, outputSpace?, progression?(p), premultiplied?, hue?: "longer"\|"shorter"\|"increasing"\|"decreasing"\|"raw" }` |
|  [02]   | `MixOptions`       | alias         | `= RangeOptions` (there is NO `powerless` field)                                                |
|  [03]   | `StepsOptions`     | interface     | `RangeOptions & { steps?, maxSteps?, maxDeltaE?, deltaEMethod?: Methods }`                      |
|  [04]   | `Range`            | fn type       | `((p: number) => Color) & { rangeArgs }` — interpolant; `p` outside `0..1` extrapolates          |
|  [05]   | `ToGamutOptions`   | interface     | `{ method?: "css"\|"clip"\|"<space>.<coord>", space?, deltaEMethod?, jnd?, blackWhiteClamp? }`  |
|  [06]   | `ParseOptions`     | interface     | `{ meta?/parseMeta?: ParseMeta }` — capture parsed format/type metadata                         |
|  [07]   | `TryColorOptions`  | interface     | `ParseOptions & { cssProperty?, element?: Element, errorMeta? }` — DOM/CSS resolution for `Color.try` |
|  [08]   | `SerializeOptions` | interface     | `{ precision?, format?: string\|Format, inGamut?, coords?, alpha?, commas? }`                   |
|  [09]   | `Display`          | type          | `string & { color: PlainColorObject }` — a displayable string carrying its resolved color       |
|  [10]   | `DeltasReturn`     | interface     | `{ space; coords: [number,number,number]; alpha }` — per-coordinate delta from `deltas`         |

[PUBLIC_TYPE_SCOPE]: space-config and appearance types
- rail: color

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [NOTE]                                                                                          |
| :-----: | :---------------- | :------------ | :--------------------------------------------------------------------------------------------- |
|  [01]   | `SpaceOptions`    | interface     | `{ id, name, base?, fromBase?(coords), toBase?(coords), coords?, white?, cssId?, referred?, formats?, gamutSpace?, aliases?, ε? }` — `ColorSpace` constructor input |
|  [02]   | `CoordMeta`       | interface     | `{ name?, type?, range?, refRange? }` — per-channel metadata                                    |
|  [03]   | `Format`          | interface     | parse/serialize descriptor: `{ type?, name?, id?, coords?, coordGrammar?, parse?, serialize?, test?, commas?, alpha? }` (re-exported as `SpaceFormat`) |
|  [04]   | `RGBOptions`      | interface     | `SpaceOptions & { toXYZ_M?: Matrix3x3; fromXYZ_M?: Matrix3x3 }` — RGB-space matrices            |
|  [05]   | `White` / `CAT`   | types         | white-point tristimulus + chromatic-adaptation-transform descriptor (`{ id, toCone_M, fromCone_M }`) |
|  [06]   | `Matrix3x3` / `Vector3` | tuples  | `3×3` conversion matrix / 3-vector for custom-space math                                        |
|  [07]   | `Cam16Input` / `Cam16Object` / `Cam16Environment` | interfaces | CAM16 appearance-model correlates and viewing environment (backing `CAM16_JMh`/`HCT`) |
|  [08]   | `Methods` / `Algorithms` | derived unions | the ΔE method vocabulary (`76`\|`CMC`\|`2000`\|`Jz`\|`ITP`\|`OK`\|`OK2`\|`HCT`) and the contrast algorithm vocabulary (`WCAG21`\|`APCA`\|`Michelson`\|`Weber`\|`Lstar`\|`DeltaPhi`) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction and factory
- rail: color

| [INDEX] | [SURFACE]                          | [ENTRY_FAMILY] | [NOTE]                                                                       |
| :-----: | :--------------------------------- | :------------- | :-------------------------------------------------------------------------- |
|  [01]   | `new Color(color: ColorTypes)`     | constructor    | parse/coerce any `ColorTypes` (CSS string, `PlainColorObject`, `ColorConstructor`) |
|  [02]   | `new Color(space, coords, alpha?)` | constructor    | construct from a space id/instance + raw `Coords`                            |
|  [03]   | `Color.get(color, options?)`       | static factory | parse or coerce to `Color`; `ParseOptions` captures format metadata          |
|  [04]   | `Color.try(color, options?)`       | static factory | `Color \| null`, non-throwing; `TryColorOptions` resolves `currentColor`/CSS via a DOM `element` |

[ENTRYPOINT_SCOPE]: instance state (mutation vs copy semantics are load-bearing)
- rail: color

| [INDEX] | [SURFACE]                      | [ENTRY_FAMILY] | [NOTE]                                                                       |
| :-----: | :----------------------------- | :------------- | :-------------------------------------------------------------------------- |
|  [01]   | `color.set(ref, value)`        | MUTATE-in-place| sets one coord (value may be `(coord) => number`), returns `this` — fluent, NOT a copy |
|  [02]   | `color.set(props)`             | MUTATE-in-place| multi-coord object update, returns `this`                                    |
|  [03]   | `color.setAll(coords, alpha?)` | MUTATE-in-place| replaces all coords (optionally in a named space), returns `this`            |
|  [04]   | `color.get(ref)`               | read           | numeric coord by `Ref`; `color.getAll(space?)` returns a `PlainColorObject`  |
|  [05]   | `color.to(space, {inGamut?})`  | COPY           | new `Color` in the target space; `{ inGamut }` clamps on convert            |
|  [06]   | `color.clone()`                | COPY           | deep clone (`this`)                                                          |
|  [07]   | `color.toGamut(options?)`      | COPY           | in-gamut copy; `ToGamutOptions.method` = `"css"` (CSS 4 GMA, default) / `"clip"` / `"<space>.<coord>"` |
|  [08]   | `color.inGamut(space?)`        | test           | boolean gamut membership                                                     |
|  [09]   | `color.equals(other)`          | test           | space + coordinate equality                                                  |
|  [10]   | `color.toString(options?)`     | serialize      | CSS string; `SerializeOptions` = `{ precision, format, inGamut, coords, alpha, commas }` |
|  [11]   | `color.display(...args)`       | serialize      | `Display` (a CSS string carrying `.color`) for a device-displayable value    |
|  [12]   | `color.toJSON()`               | serialize      | `ColorConstructor` — the canonical persistable record                       |

[ENTRYPOINT_SCOPE]: interpolation and perceptual operations
- rail: color

| [INDEX] | [SURFACE]                        | [ENTRY_FAMILY] | [NOTE]                                                                       |
| :-----: | :------------------------------- | :------------- | :-------------------------------------------------------------------------- |
|  [01]   | `color.mix(color2, p?, options?)`| blend (COPY)   | new `Color` at proportion `p` (default `0.5`); `MixOptions.space`/`hue` pick the path |
|  [02]   | `color.range(color2, options?)`  | interpolant    | returns a `Range` `(p) => Color`; `RangeOptions.progression`/`hue`/`premultiplied` shape it |
|  [03]   | `color.steps(color2, options?)`  | sample (COPY)  | `Color[]`; `StepsOptions.steps`/`maxSteps`/`maxDeltaE` bound the sampling    |
|  [04]   | `color.lighten(amount?)` / `darken(amount?)` | variation | perceptual lightness shift in OKLCH                                     |
|  [05]   | `color.luminance` (get/set)      | accessor       | relative luminance `0..1` (`getLuminance`/`setLuminance` are the `/fn` twins) |
|  [06]   | `color.distance(color2, space?)` | metric         | Euclidean distance in a space — distinct from perceptual ΔE                  |
|  [07]   | `color.deltas(color2, space?)`   | metric         | `DeltasReturn` per-coordinate signed deltas                                  |
|  [08]   | `color.uv()` / `color.xy()`      | chromaticity   | CIE `uv`/`xy` chromaticity coordinates                                       |

[ENTRYPOINT_SCOPE]: contrast family — one parameterized owner + named shortcuts
- rail: color

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY] | [NOTE]                                                                       |
| :-----: | :------------------------------------- | :------------- | :-------------------------------------------------------------------------- |
|  [01]   | `color.contrast(bg, algorithm)`        | parameterized  | THE owner: `algorithm ∈ Algorithms`; also `Color.contrast(fg, bg, algorithm)` static |
|  [02]   | `color.contrast<Algorithm>(bg)`        | shortcut set   | `contrastWCAG21` / `contrastAPCA` / `contrastMichelson` / `contrastWeber` / `contrastLstar` / `contrastDeltaPhi` — instance and static, one per `Algorithms` member |

[ENTRYPOINT_SCOPE]: ΔE family — one parameterized owner + named shortcuts + registry
- rail: color

| [INDEX] | [SURFACE]                     | [ENTRY_FAMILY] | [NOTE]                                                                       |
| :-----: | :---------------------------- | :------------- | :-------------------------------------------------------------------------- |
|  [01]   | `color.deltaE(color2, method?)`| parameterized  | THE owner: `method ∈ Methods` (defaults to `Color.defaults.deltaE`); also `Color.deltaE(...)` static |
|  [02]   | `color.deltaE<Method>(color2)` | shortcut set   | `deltaE76` / `deltaECMC` / `deltaE2000` / `deltaEJz` / `deltaEITP` / `deltaEOK` / `deltaEOK2` / `deltaEHCT` — instance and static |
|  [03]   | `Color.deltaEMethods`          | registry       | the `Record<Methods, fn>` the parameterized owner dispatches through         |

[ENTRYPOINT_SCOPE]: registry, defaults, and the extensibility seam
- rail: color

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY] | [NOTE]                                                                       |
| :-----: | :------------------------------------- | :------------- | :-------------------------------------------------------------------------- |
|  [01]   | `Color.spaces` / `Color.Space`         | registry       | the map of registered spaces / the `ColorSpace` class re-export             |
|  [02]   | `Color.defaults`                       | config         | `{ gamut_mapping, precision, deltaE, verbose, warn }` — global generation defaults |
|  [03]   | `Color.hooks` (`Hooks.add`/`run`)      | AOP seam       | register callbacks at named hook points to alter internal execution (`Hooks` is also a named class for fresh hook sets) |
|  [04]   | `Color.defineFunction(name, code, opts?)` | plugin      | attach a standalone function as an instance/namespace method (`opts.returns: "color"` lifts it into the fluent chain) |
|  [05]   | `Color.defineFunctions(map)` / `Color.extend(plugin)` | plugin | bulk-register functions / apply a plugin exposing `register(Color)` |
|  [06]   | `Color.parse` / `Color.util` / `Color.WHITES` | utilities | standalone parse fn / internal utilities / standard white-point references  |

[ENTRYPOINT_SCOPE]: `ColorSpace` — custom space registration
- rail: color

| [INDEX] | [SURFACE]                                   | [ENTRY_FAMILY] | [NOTE]                                                                       |
| :-----: | :------------------------------------------ | :------------- | :-------------------------------------------------------------------------- |
|  [01]   | `new ColorSpace(options: SpaceOptions)`     | constructor    | define a space via `fromBase`/`toBase` + `coords` metadata + `formats`      |
|  [02]   | `ColorSpace.register(space)` / `(id, space)`| registration   | register into the global registry (throws on a duplicate id)                |
|  [03]   | `ColorSpace.get(space, ...alternatives)`    | lookup         | resolve by id/alias/instance; `.all` lists every registered space           |
|  [04]   | `ColorSpace.resolveCoord(ref, workingSpace?)`| lookup        | resolve a `Ref` to `CoordMeta & { id, index, space }`                        |
|  [05]   | `ColorSpace.findFormat(filters, spaces?)` / `.DEFAULT_FORMAT` | lookup | format discovery / the fallback `color()` format                     |
|  [06]   | `space.from(color)` / `space.to(space, coords)` | convert    | base↔space coord conversion; `getMinCoords`/`inGamut`/`getFormat`/`equals` complete the surface |
|  [07]   | `space.cssId` / `space.isPolar` / `space.isUnbounded` | getters | CSS id (`display-p3`, `--cam16-jmh`), polar-hue flag, unbounded-gamut flag |

[ENTRYPOINT_SCOPE]: functional twin (`colorjs.io/fn`) and space modules (`colorjs.io/spaces`)
- rail: color

| [INDEX] | [SURFACE]                                             | [ENTRY_FAMILY] | [NOTE]                                                                       |
| :-----: | :---------------------------------------------------- | :------------- | :-------------------------------------------------------------------------- |
|  [01]   | `to` / `parse` / `serialize` / `display` / `clone`    | standalone     | every instance op as a tree-shakeable fn taking the color first             |
|  [02]   | `getColor` / `tryColor` / `get` / `getAll` / `set` / `setAll` | standalone | the parse/read/write twins (`set`/`setAll` mutate the passed color object)   |
|  [03]   | `inGamut` / `toGamut` / `toGamutCSS` / `distance` / `deltas` / `equals` | standalone | gamut, metric, and equality twins (`toGamutCSS` = the CSS 4 GMA standalone) |
|  [04]   | `mix` / `range` / `steps` / `isRange`                 | standalone     | interpolation twins + the `Range` guard                                     |
|  [05]   | `contrast` + `contrast<Algorithm>` / `deltaE` + `deltaEMethods`| standalone | the contrast and ΔE families as fns                                 |
|  [06]   | `ColorSpace` / `RGBColorSpace` / `defaults` / `hooks` / `getLuminance` / `setLuminance` / `uv` / `xy` | standalone | registry, config, hooks, luminance, chromaticity |
|  [07]   | `colorjs.io/spaces` registry vocabulary               | space modules  | `sRGB`/`sRGB_Linear`, `HSL`/`HSV`/`HWB`, `Lab`/`LCH`/`Lab_D65`, `OKLab`/`OKLCH`/`OKLrab`/`OKLrCH`/`Okhsl`/`Okhsv`, `P3`/`A98RGB`/`ProPhoto`/`REC_2020` (+`_Linear`), `XYZ_D65`/`XYZ_D50`/`XYZ_ABS_D65`, `Luv`/`LCHuv`/`HSLuv`/`HPLuv`, `CAM16_JMh`/`HCT` — import a subset for tree-shaking |

## [04]-[IMPLEMENTATION_LAW]

[COLOR_TOPOLOGY]:
- mutation vs copy is load-bearing: `set`/`setAll` MUTATE the receiver and return `this` (fluent chains build a scale by mutating a clone — `theming/tokens.md`); `to`/`mix`/`steps`/`range`/`clone`/`toGamut` return NEW colors; clone first when the original must survive
- `to(space, { inGamut })` optionally clamps on convert; the standalone `to` takes the color as the first argument like every `/fn` twin
- `PlainColorObject` (`{ space, coords, alpha }`) is the canonical boundary shape; `coords` entries may be `null` for missing/powerless channels; `ColorConstructor` (`toJSON`) is the persistable record
- contrast and ΔE are parameterized: `contrast(bg, algorithm)` and `deltaE(color2, method)` dispatch through the `Algorithms`/`Methods` vocabularies (and `deltaEMethods`); the named shortcuts are shorthands, not a closed roster — a new algorithm/method is a registry row, never a new API family
- the space set is a registry, not an enum: `ColorSpace.register` + `colorjs.io/spaces` open it; `Color.spaces`/`ColorSpace.all` enumerate it; a custom space is a `SpaceOptions` with `fromBase`/`toBase`
- extensibility is `hooks` (execution-point callbacks) + `defineFunction(s)`/`extend` (attach methods) + `defaults` (global `gamut_mapping`/`precision`/`deltaE`); a plugin exposes `register(Color)` and applies through `Color.extend`

[STACKING]:
- OKLCH token scale (`theming/tokens.md#THEME_TOKENS`): `new Color(base).to("oklch")`, clone the anchor, `set("l", …)` MUTATE the light/dark ends, `steps(dark, { space: "oklch", steps })` sample, then `toString({ format: "oklch" })` each step — the resulting `ReadonlyArray<string>` feeds a `SubscriptionRef` token cell whose `changes` stream drives one `effect` `Stream.runForEach` `CssVarSync` fold onto `document.documentElement.style`, so a theme swap is one record push with no re-render cascade
- accessibility gate: `contrastWCAG21(bg)` (or the parameterized `contrast(bg, "WCAG21")`) is the perceptual gate a derived contrast-safe token record runs during the same generation fold; `deltaEOK` scores perceptual similarity for palette de-duplication
- picker projection (`interaction/picker.md#PICKER_BEHAVIOR`): a `react-stately` `parseColor` value is round-tripped through `new Color(cssString).to("oklch").toString({ format: "oklch" })` so a pick becomes one perceptual token, contrast-checked in the same fold — never an sRGB hex beside the token projection
- untrusted boundaries: wrap parse in `Color.try` (non-throwing → `Color | null`) or the `/fn` `tryColor`, and lift a null/failed parse into an `effect` `Option`/`Schema.decodeUnknownEither` rail so a raw parse exception never propagates as a domain error
- persistence: store colors as CSS strings or `toJSON()` `ColorConstructor` records behind an `effect` `Schema`; reconstruct with `new Color(stored)` on receipt — the `Color` prototype never crosses a module boundary, only `PlainColorObject`/`ColorConstructor`
- tree-shaking: prefer `colorjs.io/fn` + a `colorjs.io/spaces` subset in a leaf bundle where only a few operations/spaces are used; the default `colorjs.io` import loads every space and method

[LOCAL_ADMISSION]:
- produce `PlainColorObject` (or a `ColorConstructor` record) at domain boundaries; never carry the `Color` prototype between modules
- select the ΔE method by context: `WCAG21`/`APCA` contrast for accessibility gates, `deltaEOK` for perceptual similarity, `deltaE2000` for print/manufacturing tolerance
- register a custom space once via `ColorSpace.register`; set project-wide `Color.defaults` (`gamut_mapping: "css"`, `precision`) at the composition root, never per call
- clone before a fluent `set` chain when the source anchor must be reused (the mutate-in-place semantics make an un-cloned `set` a silent aliasing bug)

[RAIL_LAW]:
- package: `colorjs.io`
- owns: wide-gamut color arithmetic, gamut mapping, interpolation, the parameterized contrast and ΔE families, chromaticity, and the open space registry
- accept: any CSS color string, `PlainColorObject`, `ColorConstructor`, or space+coords; `Color.try` for untrusted parse; `ColorSpace.register`/`hooks`/`defineFunction` for custom spaces and methods
- reject: a hand-rolled OKLCH ramp, WCAG/APCA contrast formula, or ΔE math beside this owner; treating `set`/`setAll` as copies; a second `colorjs.io` import path beside the one token/OKLCH owner
