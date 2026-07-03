# [colorjs.io] — one color-space authority behind the CSS token rows and the viewer render space

`colorjs.io` is the perceptual color engine the `token/theme` and `token/scale` planes compose: it parses/serializes every CSS Color 4 space, converts through a registry of 30+ spaces by name (`to(space)`), interpolates perceptually-even palette ramps (`range`/`steps` in OKLCH/OKLab), gamut-maps to sRGB/P3 (`toGamut`/`toGamutCSS`), gates accessibility with six contrast algorithms and eight ΔE methods, and extends through a hooks/plugin surface. It ships two entry lanes — the ergonomic OO `Color` class (`.`) and the tree-shakeable functional API (`./fn` + `./spaces`) — and carries zero runtime dependencies. Tokens are authored and mixed here in OKLCH, decoded once through `effect` `Schema.transform` into a `PlainColorObject` interior, then fan out to two color-space sinks: `serialize`d into `tailwindcss`'s `@theme --color-*` namespace for the CSS token plane, and `to("srgb"/"srgb-linear")`-converted into the `coords` three's `ColorManagement` renders for the `scope:viewer` plane — one authority, so token color and rendered color are the same color-space artifact. Palette ramps are `steps`/`range`, never hand-listed hex stops; contrast thresholds are `contrastAPCA`/`contrastWCAG21` refinements, never hand-rolled ratio math.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `colorjs.io`
- package: `colorjs.io`
- version: `0.6.1`
- license: `MIT`
- deps: none — zero runtime dependency, framework-agnostic, pure synchronous math
- ts-floor: TypeScript 5.0 (declaration-stated minimum)
- catalog-verdict: KEEP
- runtime: universal — no DOM/React coupling; runs at build, render, worker, or the theme atom identically
- exports: `.` (default `Color` class + all types), `./fn` (tree-shakeable functions + `ColorSpace`/`hooks`/`spaces`), `./spaces` (the space-registration roster), `./src/*` (per-module type/source access)
- lane law: browser bundles import `./fn` + register only the needed `./spaces`; the `Color` class is the authoring/ergonomic lane and pulls the full space registry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the color value algebra
- rail: token/color
- `ColorTypes` is the one input union every operation accepts — a string, a plain object, or a `Color`; `PlainColorObject` is the decoded interior a token brand carries; `Coords`/`Ref` address channels. These are the shapes a `Schema` boundary and a design page type against.

| [INDEX] | [SYMBOL]                                                                              | [TYPE_FAMILY]     | [CONSUMER / BOUNDARY]                                            |
| :-----: | :----------------------------------------------------------------------------------- | :---------------- | :-------------------------------------------------------------- |
|  [01]   | `ColorTypes = ColorObject \| ColorConstructor \| string \| PlainColorObject`         | input union       | the one accepted color input across every function + constructor |
|  [02]   | `PlainColorObject = { space: ColorSpace; coords: Coords; alpha: number \| null }`     | decoded interior  | the `Schema` decoded type a `ColorToken` brand wraps; `./fn` return |
|  [03]   | `Coords = [number \| null, number \| null, number \| null]`                           | channel triple    | the three-channel coordinate vector (`null` = missing/none)     |
|  [04]   | `Ref = string \| [string \| ColorSpace, string] \| { space; coordId }`                | channel address   | `"oklch.l"`, `["oklch","c"]`, or object form for `get`/`set`    |
|  [05]   | `Range = ((p: number) => Color) & { rangeArgs }`                                      | interpolation fn  | the reusable ramp function `range` returns; `steps` consumes it |
|  [06]   | `RangeOptions` / `MixOptions` (= `RangeOptions`) / `StepsOptions`                     | interpolation opts | `space`, `outputSpace`, `hue`, `progression`, `premultiplied`; steps adds `maxDeltaE`/`deltaEMethod`/`steps`/`maxSteps` |
|  [07]   | `SerializeOptions`                                                                    | emit opts         | `precision`, `format`, `inGamut`, `alpha`, `commas` — CSS string control |
|  [08]   | `ToGamutOptions`                                                                      | gamut opts        | `method` (`"css"`/`"clip"`), `space`, `deltaEMethod`, `jnd`, `blackWhiteClamp` |
|  [09]   | `Algorithms` (`"WCAG21" \| "APCA" \| "Michelson" \| "Weber" \| "Lstar" \| "deltaPhi"`) | contrast vocab  | the `algorithm` argument to `contrast`                          |
|  [10]   | `Methods` (`"76" \| "CMC" \| "2000" \| "Jz" \| "ITP" \| "OK" \| "OK2" \| "HCT"`)      | ΔE vocab          | the `method` argument to `deltaE` + `deltaEMethod` in steps/gamut |
|  [11]   | `SpaceOptions` / `Format` / `CoordMeta` / `White` / `CAT`                             | space definition  | `ColorSpace` construction + custom-format/chromatic-adaptation registration |
|  [12]   | `Cam16Input` / `Cam16Object` / `Cam16Environment`                                    | appearance model  | CAM16/HCT viewing-condition inputs (Material color)             |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: one parameterized operation per concern
- rail: token/color
- every concern is one polymorphic call discriminating on argument value, not a per-space/per-method family: `to(space)` converts to any registered space, `contrast(bg, fg, algorithm)` selects any algorithm, `deltaE(color, method)` selects any ΔE method, `serialize(color, options)` emits any format. The per-method functions (`deltaE2000`, `contrastAPCA`, `to`-space accessors) are the value vocabulary behind these entries, not parallel APIs. Instance methods (`Color`), static methods (`Color.*`), and free functions (`./fn`) are three surfaces over the same operations.

| [INDEX] | [SURFACE]                                                                             | [ENTRY_FAMILY]  | [CONSUMER / BOUNDARY]                                           |
| :-----: | :----------------------------------------------------------------------------------- | :-------------- | :------------------------------------------------------------- |
|  [01]   | `new Color(input)` / `new Color(space, coords, alpha?)` · `Color.get` · `parse(str)` · `getColor` | construct/parse | ingress from CSS string / object; `parse` → `ColorConstructor` |
|  [02]   | `Color.try(input, opts?)` / `tryColor(input, opts?)` → `Color \| null` / `PlainColorObject \| null` | safe parse | non-throwing ingress; feeds `Schema.transformOrFail`           |
|  [03]   | `to(color, space, opts?)` (instance `.to` / static / `./fn`)                          | convert         | one call to any registered space by id; the conversion rail    |
|  [04]   | `serialize(color, opts?)` (`.toString`) · `display(color, opts?)`                     | emit            | CSS string; `display` adds a `@media (color-gamut)` fallback carrying `.color` |
|  [05]   | `get(color, ref)` · `set(color, ref, value \| fn)` · `getAll` · `setAll`              | channel access  | read/write a channel by `Ref`; `set` accepts a coord updater fn |
|  [06]   | `inGamut(color, space?, { epsilon }?)` · `toGamut(color, opts \| space)` · `toGamutCSS(origin, { space }?)` | gamut | P3/sRGB fit test + map; `toGamutCSS` = CSS Color 4 OKLCH chroma reduction |
|  [07]   | `contrast(bg, fg, algorithm \| { algorithm })` + `contrastAPCA`/`contrastWCAG21`/`contrastMichelson`/`contrastWeber`/`contrastLstar`/`contrastDeltaPhi` | a11y gate | one parameterized contrast; six algorithms as the vocabulary   |
|  [08]   | `deltaE(color1, color2, method?)` + `deltaE76/CMC/2000/Jz/ITP/OK/OK2/HCT` · `deltaEMethods` · `deltas` · `distance` | perceptual distance | one parameterized ΔE; eight methods as the vocabulary; `deltas` = per-coord signed diffs |
|  [09]   | `range(c1, c2, opts?)` → `Range` · `mix(c1, c2, p?, opts?)` · `steps(c1, c2, opts?)` / `steps(range, opts?)` · `isRange` | interpolation | palette ramps; `hue`/`progression`/`premultiplied`; `steps` bounds by `maxDeltaE`/`maxSteps` |
|  [10]   | `getLuminance`/`setLuminance` · `.luminance` · `lighten`/`darken` · `uv`/`xy`         | derived metrics | relative luminance, tonal variation, CIE chromaticity          |
|  [11]   | `equals(c1, c2)` · `clone(color)` · `.toJSON()`                                       | value ops       | structural equality, copy, serialization of the plain object   |
|  [12]   | `ColorSpace.register(space)` / `.get(id)` / `.resolveCoord(ref)` / `.all` / `.registry` · `spaces` | space registry | register/resolve spaces; the `./spaces` roster is the seed data |
|  [13]   | `Color.defineFunction`/`defineFunctions`/`extend` · `hooks.add(name, cb, first?)`/`hooks.run` · `Hooks` · `defaults` | extension | plugin surface: new methods, coord accessors, and internal hooks |

[ENTRYPOINT_SCOPE]: the space roster (seed data for the conversion rail)
- rail: token/color
- `./spaces` registers these named spaces into `ColorSpace.registry`; `to`/`serialize`/`range` reference them by id. Register the subset a bundle needs (`sRGB`, `P3`, `OKLCH`, `OKLab`) rather than the whole registry — the parameterized `to(space)` call is the mechanism, this list is its vocabulary.

| [GROUP]        | [SPACES]                                                                                   |
| :------------- | :----------------------------------------------------------------------------------------- |
| tristimulus    | `XYZ_D65`, `XYZ_D50`, `XYZ_ABS_D65`                                                         |
| CIE            | `Lab`, `Lab_D65`, `LCH`, `Luv`, `LCHuv`, `HSLuv`, `HPLuv`                                   |
| RGB + polar    | `sRGB`(`_Linear`), `HSL`, `HWB`, `HSV`, `P3`(`_Linear`), `A98RGB`(`_Linear`), `ProPhoto`(`_Linear`), `REC_2020`(`_Linear`/`_Scene_Referred`) |
| OK family      | `OKLab`, `OKLCH`, `OKLrab`, `OKLrCH`, `Okhsl`, `Okhsv`                                      |
| appearance     | `CAM16_JMh`, `HCT` (Material color)                                                         |

## [04]-[IMPLEMENTATION_LAW]

[COLOR_SEMANTICS]:
- one input union, one conversion rail: every operation accepts `ColorTypes` and converts through named spaces via `to(space)`; the per-space accessors (`color.oklch.l`) and per-method functions are conveniences over the parameterized entries, never the primary surface a design page targets.
- two lanes, one algebra: the `Color` class chains and mutates (`set` returns the color), the `./fn` functions are pure and return `PlainColorObject`. Bundle-sensitive folder code (browser) uses `./fn` + explicit `ColorSpace.register`; authoring/tooling code uses `Color`. Output is identical.
- gamut is explicit: `inGamut` tests, `toGamut({ method, space, jnd })` maps by ΔE-bounded chroma reduction or `"clip"`, and `toGamutCSS` is the CSS Color 4 algorithm (OKLCH chroma reduction toward the gamut boundary). `serialize({ inGamut: true })` clamps at emit; `display()` emits the wide-gamut value plus a narrow-gamut `@media` fallback.
- pure and synchronous: no `Effect`, no async, no DOM. It composes inside a `derive` selector or a build step without a rail; wrap in `Effect.sync` only inside an effectful pipeline.

[INTEGRATION_LAW]:
- Stack with `effect` `Schema` as the token boundary: a `ColorToken` is `Brand.Branded<string, "ColorToken">`; `Schema.transform(Schema.String, ColorStruct, { decode: parse, encode: serialize })` crosses the CSS-string ↔ `PlainColorObject` boundary exactly once — the decoded interior is the plain object, the encoded wire shape is the `oklch(...)` string. Use `Schema.transformOrFail` with `Color.try`/`tryColor` mapping `null` to a `ParseError` so malformed tokens fail at decode, not at render.
- Stack with `token/scale` for palette ramps: generate the lightness/tonal ladder with `steps(base, target, { space: "oklch", steps, maxDeltaE, hue: "shorter" })` or a reusable `range(...)` — one parameterized, perceptually-even ramp is the scale row, replacing a hand-listed hex table. `progression` supplies the easing, `premultiplied` handles alpha ramps.
- Stack with `token/theme` for a11y gating: gate foreground/background token pairs with `contrastAPCA`/`contrastWCAG21` (the parameterized `contrast(bg, fg, algorithm)`) wired as a `Schema.filter` refinement on a `ColorTokenPair` brand — pairs below the APCA/WCAG threshold reject at decode. `deltaE2000`/`deltaEOK` (the parameterized `deltaE(a, b, method)`) drives nearest-token snapping and theme-diff.
- Stack with `@effect-atom` (`ONE_FOLD_ONE_BINDING`) as the token state owner feeding both color-space sinks: the theme atom holds decoded `PlainColorObject` tokens, and `derive` selectors project them purely to the two consumers below — conversion never mutates the atom, no write per frame.
- Stack with `tailwindcss` (`.api/tailwindcss.md`) as the CSS token sink: the OKLCH values authored here — laddered by `steps`/`range`, gamut-fit, `contrastAPCA`/`contrastWCAG21`-gated — are `serialize({ format: "oklch", inGamut: true })`-emitted into the `@theme --color-*` namespace, which tailwind compiles into the `--color-*` custom property plus its `bg-`/`text-`/`ring-` utility family; the palette tailwind emits is a colorjs.io-computed color-space artifact, never a hand-listed hex table, and `display()` adds the P3 `@media (color-gamut)` fallback.
- Stack with `three` `ColorManagement` (`.api/three.md`, `scope:viewer`) as the render color-space sink: the same OKLCH values are gamut-fit and `to("srgb", { inGamut: true })` / `to("srgb-linear")`-converted — colorjs.io's `srgb`/`srgb-linear` space ids are the exact string values of three's `SRGBColorSpace`/`LinearSRGBColorSpace` constants, so no remap table — and the `PlainColorObject.coords` `[r, g, b]` feed `three.Color.setRGB(r, g, b, SRGBColorSpace | LinearSRGBColorSpace)`, where `ColorManagement` transforms source→working. colorjs.io owns OKLCH authoring + gamut fit, three owns the working-space transform, meeting at the sRGB/linear coordinate wire — token color and rendered color are one color-space contract. Beyond the sRGB path, `CAM16_JMh`/`HCT` + `deltaEHCT`/`deltaEITP` supply appearance-correct/HDR-aware color for OpenPBR/scene tokens where the WCAG-oriented sRGB path is insufficient; register those spaces only in the `scope:viewer` bundle.
- Stack with the `react-aria`/`react-stately` color-picker widgets (`.api/react-aria.md`, `.api/react-stately.md`): `useColorArea`/`useColorWheel`/`ColorPickerState` edit channels on react-aria's own `parseColor` model in `HSL`/`HSV`/`HWB`/`sRGB`, then hand off to colorjs.io at the display boundary — `to(space)` for channel-space conversion, `inGamut`/`toGamut` for the picker's gamut fit, `serialize` for the emitted value — so the widget's interactive edit and the token pipeline share this one conversion rail; register `HSL`/`HSV`/`HWB`/`sRGB` in that bundle.
- Extension seam over reinvention: a new named space is `ColorSpace.register(new ColorSpace(SpaceOptions))`, a new derived method is `Color.defineFunction`/`extend`, and internal interception is `hooks.add(name, cb)` — the parameterized registry and hook surface absorb new capability, never a forked conversion helper.

[LOCAL_ADMISSION]:
- the color algebra only; token vocabulary and CSS-variable emission live in `token/theme`/`token/scale`, the render-space handoff is `to("srgb"/"srgb-linear")` → three `Color`/`ColorManagement` in `scope:viewer`, the decode boundary is `effect` `Schema`, and state is `@effect-atom`.
- register the space subset a bundle needs via `./fn` + `ColorSpace.register` — the CSS plane needs `OKLCH`/`OKLab`/`sRGB`/`P3`, the `scope:viewer` bundle adds `sRGB_Linear` + `CAM16_JMh`/`HCT`; pull the full `Color` class only in authoring/non-shipping code.
- one `to(space)`/`contrast(...algorithm)`/`deltaE(...method)`/`serialize(...format)` parameterized call per concern; a new space/algorithm/method is a registry row or an argument value, never a new operation family.

[RAIL_LAW]:
- Package: `colorjs.io`
- Owns: parsing/serialization of CSS Color 4 spaces, named-space conversion, perceptual interpolation (`range`/`mix`/`steps`), gamut mapping (`toGamut`/`toGamutCSS`), the six-algorithm contrast rail, the eight-method ΔE rail, luminance/chromaticity/variation, and the `ColorSpace`/`hooks`/`defineFunction` extension surface
- Accept: `./fn` + explicit `ColorSpace.register` for shipped bundles, OKLCH/OKLab authoring, `Schema.transform`(`parse`/`serialize`) + `Color.try` as the token boundary, `steps`/`range` for palette ramps, `contrast*`/`deltaE*` as parameterized a11y + distance gates, `serialize`/`display` for the `tailwindcss` `@theme --color-*` CSS-variable + wide-gamut fallback emission, `to("srgb"/"srgb-linear", { inGamut: true })` → `coords` for the three `Color.setRGB`/`ColorManagement` render-space handoff, registry/`defineFunction`/`hooks` for extension
- Reject: hand-rolled hex/HSL math or WCAG ratio arithmetic, a per-space `toRgb`/`toHsl` conversion family instead of `to(space)`, hand-listed palette stops instead of `steps`/`range`, re-minting the OKLCH gamut map instead of `toGamutCSS`, hand-converting OKLCH→display RGB for three instead of `to("srgb"/"srgb-linear")` whose ids match three's color-space constants, importing the full `Color` class where `./fn` + registered spaces suffice, treating parse/serialize as async
