# [TS_UI_API_COLORJS_IO]

`colorjs.io` owns the perceptual color algebra: CSS Color 4 parse/serialize, named-space conversion via `to(space)`, perceptual interpolation, gamut mapping, max-in-gamut chroma, and the parameterized `contrast`/`deltaE` rails, zero runtime dependency.

Tokens author in OKLCH, decode once through `effect` `Schema` into a `PlainColorObject`, then fan to the `tailwindcss` `@theme --color-*` plane via `serialize` and the `three` render space via `to("srgb"/"srgb-linear")` — one artifact for CSS and render.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `colorjs.io`
- package: `colorjs.io` (MIT)
- module: ESM; subpath exports `.` (`Color` class + types), `./fn` (tree-shakeable functions, `ColorSpace`/`hooks`/`spaces`), `./spaces` (space-registration roster)
- runtime: universal — no DOM/React coupling; build, render, worker, and the theme atom alike
- rail: token/color — OKLCH authoring, named-space conversion, gamut fit, and the `contrast`/`deltaE` a11y gates

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the color value algebra — the shapes a `Schema` boundary and design fence type against.

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY] | [CAPABILITY]                                                       |
| :-----: | :------------------------------------------------ | :------------ | :----------------------------------------------------------------- |
|  [01]   | `ColorTypes`                                      | union         | `string` `ColorObject` `ColorConstructor` `PlainColorObject`       |
|  [02]   | `PlainColorObject`                                | interface     | decoded interior a `ColorToken` wraps: `.space` `.coords` `.alpha` |
|  [03]   | `Coords`                                          | tuple         | three-channel vector; `null` marks a missing coord                 |
|  [04]   | `Ref`                                             | address       | channel by `"oklch.l"`, `["oklch","c"]`, or `{ space, coordId }`   |
|  [05]   | `Range`                                           | function      | reusable ramp `range` returns; `steps` consumes it                 |
|  [06]   | `RangeOptions` / `MixOptions`                     | options       | `space` `outputSpace` `hue` `progression` `premultiplied`          |
|  [07]   | `StepsOptions`                                    | options       | adds `maxDeltaE` `deltaEMethod` `steps` `maxSteps`                 |
|  [08]   | `SerializeOptions`                                | options       | `precision` `format` `inGamut` `alpha` `commas`                    |
|  [09]   | `ToGamutOptions`                                  | options       | `method` `space` `deltaEMethod` `jnd` `blackWhiteClamp`            |
|  [10]   | `Algorithms`                                      | union         | the `contrast` algorithm vocabulary                                |
|  [11]   | `Methods`                                         | union         | the `deltaE` method vocabulary                                     |
|  [12]   | `SpaceOptions` / `Format` / `CoordMeta`           | definition    | `ColorSpace` construction + custom-format registration             |
|  [13]   | `White` / `CAT`                                   | adaptation    | white-point + chromatic-adaptation-transform registration          |
|  [14]   | `Cam16Input` / `Cam16Object` / `Cam16Environment` | model         | CAM16/HCT viewing-condition inputs                                 |

[ALGORITHMS]: `WCAG21` `APCA` `Michelson` `Weber` `Lstar` `DeltaPhi`
[METHODS]: `76` `CMC` `2000` `Jz` `ITP` `OK` `OK2` `HCT` `Helmlab`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: one parameterized call per concern discriminating on argument value; the free function, the `Color` instance method, and the `Color` static are three surfaces over one operation.

| [INDEX] | [SURFACE]                                                               | [SHAPE] | [CAPABILITY]                                   |
| :-----: | :---------------------------------------------------------------------- | :------ | :--------------------------------------------- |
|  [01]   | `new Color(...)` · `parse` · `getColor`                                 | ctor    | CSS string or object ingress                   |
|  [02]   | `Color.try(input)` · `tryColor(input)`                                  | static  | safe ingress; feeds `Schema.transformOrFail`   |
|  [03]   | `to(color, space, opts?)`                                               | fn      | any registered space — the conversion rail     |
|  [04]   | `serialize(color, opts?)` · `display`                                   | fn      | CSS string; `display` carries `.color`         |
|  [05]   | `get` · `set` · `getAll` · `setAll`                                     | fn      | channel r/w by `Ref`; `set` takes an updater   |
|  [06]   | `inGamut` · `toGamut` · `toGamutCSS` · `toGamutRayTrace`                | fn      | fit test + map; CSS Color 4 + ray-trace        |
|  [07]   | `contrast(bg, fg, algorithm)`                                           | fn      | parameterized a11y gate over `Algorithms`      |
|  [08]   | `deltaE(c1, c2, method?)`                                               | fn      | parameterized distance over `Methods`          |
|  [09]   | `range` · `mix` · `steps` · `isRange`                                   | fn      | palette ramps; `steps` bounds by `maxDeltaE`   |
|  [10]   | `getLuminance` · `setLuminance` · `lighten` · `darken` · `uv` · `xy`    | fn      | luminance, tonal variation, chromaticity       |
|  [11]   | `equals` · `clone` · `toJSON`                                           | fn      | equality, copy, plain-object serialization     |
|  [12]   | `ColorSpace.register` · `.get` · `.resolveCoord` · `.all` · `.registry` | static  | register/resolve spaces; `./spaces` seeds      |
|  [13]   | `Color.defineFunction` · `defineFunctions` · `extend`                   | static  | new methods and coord accessors                |
|  [14]   | `hooks.add(name, cb, first?)` · `hooks.run`                             | static  | internal interception; `Hooks` `defaults`      |
|  [15]   | `maxChroma(l, h, { space, gamut })`                                     | fn      | max in-gamut chroma for a lightness/hue        |
|  [16]   | `GamutRelativeColorSpace`                                               | class   | chroma-relative; chroma `[0,1]` stays in gamut |

[CONTRAST_FNS]: `contrastAPCA` `contrastWCAG21` `contrastMichelson` `contrastWeber` `contrastLstar` `contrastDeltaPhi`
[DELTAE_FNS]: `deltaE76` `deltaECMC` `deltaE2000` `deltaEJz` `deltaEITP` `deltaEOK` `deltaEOK2` `deltaEHCT` `deltaEHelmlab` — `deltaEMethods` `deltas` `distance`

[ENTRYPOINT_SCOPE]: the registered space vocabulary — `./spaces` seeds `ColorSpace.registry` and `to`/`serialize`/`range` reference by id; each RGB working space carries a `_Linear` companion (`REC_2020` also `_Scene_Referred`), and gamut-relative spaces rescale chroma so `[0,1]` maps to the most-saturated in-gamut color.

[TRISTIMULUS]: `XYZ_D65` `XYZ_D50` `XYZ_ABS_D65`
[CIE]: `Lab` `Lab_D65` `LCH` `Luv` `LCHuv` `HSLuv` `HPLuv`
[RGB_POLAR]: `sRGB` `HSL` `HWB` `HSV` `P3` `A98RGB` `ProPhoto` `REC_2020`
[OK_FAMILY]: `OKLab` `OKLCH` `OKLrab` `OKLrCH` `Okhsl` `Okhsv`
[GAMUT_RELATIVE]: `OKLCH_sRGB` `OKLCH_P3` `OKLCH_REC_2020` `LCH_sRGB` `LCH_P3` `LCH_REC_2020` `HSL_P3` `HSL_REC2020`
[HELMHOLTZ]: `Helmlab` `HelmGen` `HelmGenLCh`
[APPEARANCE]: `CAM16_JMh` `HCT`

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One input union, one conversion rail: every operation accepts `ColorTypes` and converts through named spaces via `to(space)`; per-space accessors (`color.oklch.l`) and per-method functions are conveniences over the parameterized entries.
- Two lanes, one algebra: the `Color` class chains and mutates (`set` returns the color), the `./fn` functions are pure and return `PlainColorObject`, output identical.
- Gamut is explicit: `inGamut` tests, `toGamut`/`toGamutCSS`/`toGamutRayTrace` map by ΔE-bounded chroma reduction or clip, `serialize({ inGamut: true })` clamps at emit, and `display` returns the environment-rendered form on `.color`.
- Pure and synchronous: no `Effect`, no async, no DOM; composes inside a `derive` selector or build step, wrapped in `Effect.sync` only inside an effectful pipeline.

[STACKING]:
- `effect` `Schema`: `Schema.transform(String, ColorStruct, { decode: parse, encode: serialize })` crosses the CSS-string ↔ `PlainColorObject` boundary once; `Schema.transformOrFail` + `Color.try`/`tryColor` maps `null` → `ParseError`, so malformed tokens fail at decode.
- `token/scale`: `steps(base, target, { space: "oklch", steps, maxDeltaE, hue })` or a reusable `range(...)` mints one perceptually-even ramp row per scale, replacing a hex table; `maxChroma`/`OKLCH_sRGB`/`OKLCH_P3` pin each stop to the gamut boundary.
- `token/theme`: `contrast(bg, fg, algorithm)` wired as a `Schema.filter` on a `ColorTokenPair` rejects sub-threshold pairs at decode; `deltaE(a, b, method)` drives nearest-token snapping and theme-diff.
- `@effect-atom`(`.api/effect-atom-atom.md`): the theme atom holds decoded `PlainColorObject` tokens, and `derive` selectors project them purely to both sinks, never a write per frame.
- `tailwindcss`(`.api/tailwindcss.md`): `serialize({ format: "oklch", inGamut: true })` emits into the `@theme --color-*` namespace tailwind compiles to `--color-*` with its `bg-`/`text-`/`ring-` utilities; `display` emits the environment-rendered serialization on `.color`.
- `three` `ColorManagement`(`.api/three.md`, `scope:viewer`): `to("srgb"/"srgb-linear", { inGamut: true })` — colorjs.io's `srgb`/`srgb-linear` ids equal three's `SRGBColorSpace`/`LinearSRGBColorSpace` constants — feeds `coords` `[r,g,b]` to `three.Color.setRGB(r, g, b, space)`; `CAM16_JMh`/`HCT` + `deltaEHCT`/`deltaEITP` carry appearance/HDR tokens beyond the sRGB path.
- `react-aria`/`react-stately`(`.api/react-aria.md`, `.api/react-stately.md`): `useColorArea`/`useColorWheel`/`ColorPickerState` edit `parseColor` channels in `HSL`/`HSV`/`HWB`/`sRGB`, then hand off at the display boundary — `to(space)`, `inGamut`/`toGamut`, `serialize` — sharing this one conversion rail.
- Extension seam: a new space is `ColorSpace.register(new ColorSpace(SpaceOptions))`, a new method is `Color.defineFunction`/`extend`, and interception is `hooks.add(name, cb)`; the registry and hook surface absorb new capability.

[LOCAL_ADMISSION]:
- Color algebra only: token vocabulary and CSS-variable emission live in `token/theme`/`token/scale`, the render-space handoff is `to("srgb"/"srgb-linear")` → `three` in `scope:viewer`, the decode boundary is `effect` `Schema`, and state is `@effect-atom`.
- Register the space subset a bundle needs via `./fn` + `ColorSpace.register` — the CSS plane binds `OKLCH`/`OKLab`/`sRGB`/`P3`, the `scope:viewer` bundle adds `sRGB_Linear` + `CAM16_JMh`/`HCT`; pull the full `Color` class only in authoring code.
- One `to(space)`/`contrast(…algorithm)`/`deltaE(…method)`/`serialize(…format)` call per concern; a new space, algorithm, or method is a registry row or argument value, never a new operation family.

[RAIL_LAW]:
- Package: `colorjs.io`
- Owns: CSS Color 4 parse/serialize, named-space conversion, perceptual interpolation (`range`/`mix`/`steps`), gamut mapping (`toGamut`/`toGamutCSS`/`toGamutRayTrace`), max-in-gamut chroma (`maxChroma`, gamut-relative spaces), the parameterized `contrast`/`deltaE` rails, luminance/chromaticity, and the `ColorSpace`/`hooks`/`defineFunction` extension surface.
- Accept: `./fn` + explicit `ColorSpace.register` for shipped bundles, OKLCH/OKLab authoring, `Schema.transform`(`parse`/`serialize`) + `Color.try` as the token boundary, `steps`/`range` palette ramps, `contrast*`/`deltaE*` a11y and distance gates, `serialize`/`display` for `@theme --color-*` emission, `to("srgb"/"srgb-linear")` → `coords` for the three render handoff.
- Reject: hand-rolled hex/HSL or WCAG ratio math, a per-space `toRgb`/`toHsl` family instead of `to(space)`, hand-listed palette stops instead of `steps`/`range`, re-minting the OKLCH gamut map instead of `toGamutCSS`, hand-converting OKLCH→RGB for three instead of the id-matched `to("srgb"/"srgb-linear")`, importing the full `Color` class where `./fn` suffices, treating parse/serialize as async.
