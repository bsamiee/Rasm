# [TS_UI_API_COLORJS_IO]

`colorjs.io` is the perceptual color engine the `token/theme` and `token/scale` planes compose: it parses/serializes every CSS Color 4 space, converts through a named-space registry (`to(space)`), interpolates perceptually-even palette ramps (`range`/`steps` in OKLCH/OKLab), gamut-maps to sRGB/P3 (`toGamut`/`toGamutCSS`/`toGamutRayTrace`) and computes maximally-saturated in-gamut chroma (`maxChroma`, gamut-relative spaces), gates accessibility through the parameterized `contrast` and `deltaE` rails, and extends through a hooks/plugin surface. It ships two entry lanes — the ergonomic OO `Color` class (`.`) and the tree-shakeable functional API (`./fn` + `./spaces`) — and carries zero runtime dependencies. Tokens are authored and mixed here in OKLCH, decoded once through `effect` `Schema.transform` into a `PlainColorObject` interior, then fan out to two color-space sinks: `serialize`d into `tailwindcss`'s `@theme --color-*` namespace for the CSS token plane, and `to("srgb"/"srgb-linear")`-converted into the `coords` three's `ColorManagement` renders for the `scope:viewer` plane — one authority, so token color and rendered color are the same color-space artifact. Palette ramps are `steps`/`range`, never hand-listed hex stops; contrast thresholds are `contrastAPCA`/`contrastWCAG21` refinements, never hand-rolled ratio math.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `colorjs.io`
- package: `colorjs.io`
- license: `MIT`
- deps: none — zero runtime dependency, framework-agnostic, pure synchronous math
- catalog-verdict: KEEP
- runtime: universal — no DOM/React coupling; runs at build, render, worker, or the theme atom identically
- exports: `.` (default `Color` class + all types), `./fn` (tree-shakeable functions + `ColorSpace`/`hooks`/`spaces`), `./spaces` (the space-registration roster), `./src/*` (per-module type/source access)
- lane law: browser bundles import `./fn` + register only the needed `./spaces`; the `Color` class is the authoring/ergonomic lane and pulls the full space registry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the color value algebra
- rail: token/color
- `ColorTypes` is the one input union every operation accepts — a string, a plain object, or a `Color`; `PlainColorObject` is the decoded interior a token brand carries; `Coords`/`Ref` address channels. These are the shapes a `Schema` boundary and a design page type against; the union/tuple definitions are the signature below, the table the consumer boundary per type.

```ts signature
type ColorTypes = ColorObject | ColorConstructor | string | PlainColorObject
type PlainColorObject = { space: ColorSpace; coords: Coords; alpha: number | null }
type Coords = [number | null, number | null, number | null]
type Ref = string | [string | ColorSpace, string] | { space; coordId }
type Range = ((p: number) => Color) & { rangeArgs }
type Algorithms = "WCAG21" | "APCA" | "Michelson" | "Weber" | "Lstar" | "deltaPhi"    // derived from the contrast* roster
type Methods = "76" | "CMC" | "2000" | "Jz" | "ITP" | "OK" | "OK2" | "HCT" | "Helmlab" // derived from the deltaE* roster
```

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY]      | [CONSUMER_BOUNDARY]                                           |
| :-----: | :------------------------------------------------ | :----------------- | :------------------------------------------------------------ |
|  [01]   | `ColorTypes`                                      | input union        | every color input — a string, plain object, or `Color`        |
|  [02]   | `PlainColorObject`                                | decoded interior   | the `Schema` decoded interior a `ColorToken` wraps            |
|  [03]   | `Coords`                                          | channel triple     | the three-channel coordinate vector (`null` = missing)        |
|  [04]   | `Ref`                                             | channel address    | `"oklch.l"`, `["oklch","c"]`, or object form for `get`/`set`  |
|  [05]   | `Range`                                           | interpolation fn   | the reusable ramp `range` returns; `steps` consumes it        |
|  [06]   | `RangeOptions` / `MixOptions` (= `RangeOptions`)  | interpolation opts | `space`, `outputSpace`, `hue`, `progression`, `premultiplied` |
|  [07]   | `StepsOptions`                                    | steps opts         | adds `maxDeltaE`, `deltaEMethod`, `steps`, `maxSteps`         |
|  [08]   | `SerializeOptions`                                | emit opts          | `precision`, `format`, `inGamut`, `alpha`, `commas`           |
|  [09]   | `ToGamutOptions`                                  | gamut opts         | `method`, `space`, `deltaEMethod`, `jnd`, `blackWhiteClamp`   |
|  [10]   | `Algorithms`                                      | contrast vocab     | the `algorithm` argument to `contrast`                        |
|  [11]   | `Methods`                                         | ΔE vocab           | the `method` argument to `deltaE` + `deltaEMethod`            |
|  [12]   | `SpaceOptions` / `Format` / `CoordMeta`           | space definition   | `ColorSpace` construction + custom-format registration        |
|  [13]   | `White` / `CAT`                                   | adaptation         | white-point + chromatic-adaptation-transform registration     |
|  [14]   | `Cam16Input` / `Cam16Object` / `Cam16Environment` | appearance model   | CAM16/HCT viewing-condition inputs (Material color)           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: one parameterized operation per concern
- rail: token/color
- every concern is one polymorphic call discriminating on argument value, not a per-space/per-method family: `to(space)` converts to any registered space, `contrast(bg, fg, algorithm)` selects any algorithm, `deltaE(color, method)` selects any ΔE method, `serialize(color, options)` emits any format. Per-method functions (`deltaE2000`, `contrastAPCA`, `to`-space accessors) are the value vocabulary behind these entries, not parallel APIs. Instance methods (`Color`), static methods (`Color.*`), and free functions (`./fn`) are three surfaces over the same operations.

| [INDEX] | [SURFACE]                            | [ENTRY_FAMILY]      | [CONSUMER_BOUNDARY]                                                  |
| :-----: | :----------------------------------- | :------------------ | :------------------------------------------------------------------- |
|  [01]   | `new Color(...)` · `parse`           | construct/parse     | ingress from CSS string/object; `parse` → `ColorConstructor`         |
|  [02]   | `Color.try` / `tryColor`             | safe parse          | non-throwing ingress → `\| null`; feeds `Schema.transformOrFail`     |
|  [03]   | `to(color, space, opts?)`            | convert             | one call to any registered space by id — the conversion rail         |
|  [04]   | `serialize` · `display`              | emit                | CSS string; `display` returns a displayable serialization + `.color` |
|  [05]   | `get` · `set` · `getAll` · `setAll`  | channel access      | read/write a channel by `Ref`; `set` takes a coord updater fn        |
|  [06]   | `inGamut` · `toGamut` · `toGamutCSS` | gamut               | P3/sRGB fit test + map; CSS Color 4 + ray-trace variants             |
|  [07]   | `contrast(bg, fg, algorithm)`        | a11y gate           | one parameterized contrast over the algorithm vocabulary             |
|  [08]   | `deltaE(c1, c2, method?)`            | perceptual distance | one parameterized ΔE over the method vocabulary; `deltas`/`distance` |
|  [09]   | `range` · `mix` · `steps`            | interpolation       | palette ramps; `steps` bounds by `maxDeltaE`/`maxSteps`              |
|  [10]   | `getLuminance` / `setLuminance`      | derived metrics     | relative luminance, tonal variation, CIE chromaticity                |
|  [11]   | `equals` · `clone` · `.toJSON()`     | value ops           | structural equality, copy, plain-object serialization                |
|  [12]   | `ColorSpace.register` / `.get`       | space registry      | register/resolve spaces; `./spaces` is the seed data                 |
|  [13]   | `Color.defineFunction` / `extend`    | extension           | new methods, coord accessors, internal hooks                         |
|  [14]   | `maxChroma(l, h, { space, gamut })`  | gamut chroma        | max in-gamut chroma for a lightness/hue in the polar space           |
|  [15]   | `GamutRelativeColorSpace`            | space class         | chroma-relative space; chroma in `[0, 1]` stays in gamut             |

- [01]-[CONSTRUCT]: `new Color(input)` / `new Color(space, coords, alpha?)` · `Color.get` · `parse(str)` · `getColor`.
- [02]-[SAFE_PARSE]: `Color.try(input, opts?)` / `tryColor(input, opts?)` → `Color | null` / `PlainColorObject | null`.
- [04]-[EMIT]: `serialize(color, opts?)` (`.toString`) · `display(color, opts?)` — `display` carries `.color`.
- [05]-[CHANNEL]: `get(color, ref)` · `set(color, ref, value | fn)` · `getAll` · `setAll`.
- [06]-[GAMUT]: `inGamut(color, space?, { epsilon }?)` · `toGamut(color, opts | space)` · `toGamutCSS(origin, { space }?)` · `toGamutRayTrace(origin, { space }?)`.
- [07]-[CONTRAST]: `contrast(bg, fg, algorithm | { algorithm })` + `contrastAPCA`/`contrastWCAG21`/`contrastMichelson`/`contrastWeber`/`contrastLstar`/`contrastDeltaPhi`.
- [08]-[DELTA_E]: `deltaE(color1, color2, method?)` + `deltaE76/CMC/2000/Jz/ITP/OK/OK2/HCT/Helmlab` · `deltaEMethods` · `deltas` · `distance`.
- [09]-[INTERPOLATE]: `range(c1, c2, opts?)` → `Range` · `mix(c1, c2, p?, opts?)` · `steps(c1, c2, opts?)` / `steps(range, opts?)` · `isRange` — `hue`/`progression`/`premultiplied`.
- [10]-[METRICS]: `getLuminance`/`setLuminance` · `.luminance` · `lighten`/`darken` · `uv`/`xy`.
- [12]-[REGISTRY]: `ColorSpace.register(space)` / `.get(id)` / `.resolveCoord(ref)` / `.all` / `.registry` · `spaces`.
- [13]-[EXTENSION]: `Color.defineFunction`/`defineFunctions`/`extend` · `hooks.add(name, cb, first?)`/`hooks.run` · `Hooks` · `defaults`.
- [14]-[GAMUT_CHROMA]: `maxChroma(l, h, { space, gamut })` · `new GamutRelativeColorSpace({ base, gamutSpace, chroma? })` — chroma `1` is the most-colorful in-gamut color, so any chroma in `[0, 1]` stays in gamut.

[ENTRYPOINT_SCOPE]: the space roster (seed data for the conversion rail)
- rail: token/color
- `./spaces` registers these named spaces into `ColorSpace.registry`; `to`/`serialize`/`range` reference them by id. Register the subset a bundle needs (`sRGB`, `P3`, `OKLCH`, `OKLab`) rather than the whole registry — the parameterized `to(space)` call is the mechanism, this list is its vocabulary. Each RGB working space carries a `_Linear` companion, and `REC_2020` also a `_Scene_Referred` variant. Gamut-relative spaces rescale chroma against an RGB gamut, so chroma in `[0, 1]` maps to the most-saturated in-gamut color for a lightness/hue.

| [INDEX] | [GROUP]        | [SPACES]                                                            |
| :-----: | :------------- | :------------------------------------------------------------------ |
|  [01]   | tristimulus    | `XYZ_D65`, `XYZ_D50`, `XYZ_ABS_D65`                                 |
|  [02]   | CIE            | `Lab`, `Lab_D65`, `LCH`, `Luv`, `LCHuv`, `HSLuv`, `HPLuv`           |
|  [03]   | RGB + polar    | `sRGB`, `HSL`, `HWB`, `HSV`, `P3`, `A98RGB`, `ProPhoto`, `REC_2020` |
|  [04]   | OK family      | `OKLab`, `OKLCH`, `OKLrab`, `OKLrCH`, `Okhsl`, `Okhsv`              |
|  [05]   | gamut-relative | `OKLCH_sRGB`, `OKLCH_P3`, `OKLCH_REC_2020`                          |
|  [06]   | gamut-relative | `LCH_sRGB`, `LCH_P3`, `LCH_REC_2020`, `HSL_P3`, `HSL_REC2020`       |
|  [07]   | Helmholtz      | `Helmlab`, `HelmGen`, `HelmGenLCh`                                  |
|  [08]   | appearance     | `CAM16_JMh`, `HCT` (Material color)                                 |

## [04]-[IMPLEMENTATION_LAW]

[COLOR_SEMANTICS]:
- one input union, one conversion rail: every operation accepts `ColorTypes` and converts through named spaces via `to(space)`; the per-space accessors (`color.oklch.l`) and per-method functions are conveniences over the parameterized entries, never the primary surface a design page targets.
- two lanes, one algebra: the `Color` class chains and mutates (`set` returns the color), the `./fn` functions are pure and return `PlainColorObject`. Bundle-sensitive folder code (browser) uses `./fn` + explicit `ColorSpace.register`; authoring/tooling code uses `Color`. Output is identical.
- gamut is explicit: `inGamut` tests, `toGamut({ method, space, jnd })` maps by ΔE-bounded chroma reduction or `"clip"`, `toGamutCSS` runs the CSS Color 4 algorithm (OKLCH chroma reduction toward the gamut boundary), and `toGamutRayTrace` runs its ray-trace variant. `serialize({ inGamut: true })` clamps at emit; `display()` returns the serialization the current environment renders — falling back to the widest CSS-supported space — and carries the source color on `.color`, never an `@media` block.
- pure and synchronous: no `Effect`, no async, no DOM. It composes inside a `derive` selector or a build step without a rail; wrap in `Effect.sync` only inside an effectful pipeline.

[INTEGRATION_LAW]:
- Stack with `effect` `Schema` as the token boundary: a `ColorToken` is `Brand.Branded<string, "ColorToken">`; `Schema.transform(Schema.String, ColorStruct, { decode: parse, encode: serialize })` crosses the CSS-string ↔ `PlainColorObject` boundary exactly once — the decoded interior is the plain object, the encoded wire shape is the `oklch(...)` string. Use `Schema.transformOrFail` with `Color.try`/`tryColor` mapping `null` to a `ParseError` so malformed tokens fail at decode, not at render.
- Stack with `token/scale` for palette ramps: generate the lightness/tonal ladder with `steps(base, target, { space: "oklch", steps, maxDeltaE, hue: "shorter" })` or a reusable `range(...)` — one parameterized, perceptually-even ramp is the scale row, replacing a hand-listed hex table. `progression` supplies the easing, `premultiplied` handles alpha ramps. For a maximally-saturated in-gamut ramp, `maxChroma(l, h, { space: "oklch", gamut })` or the `OKLCH_sRGB`/`OKLCH_P3` gamut-relative spaces pin each stop's chroma to the gamut boundary rather than a fixed value.
- Stack with `token/theme` for a11y gating: gate foreground/background token pairs with `contrastAPCA`/`contrastWCAG21` (the parameterized `contrast(bg, fg, algorithm)`) wired as a `Schema.filter` refinement on a `ColorTokenPair` brand — pairs below the APCA/WCAG threshold reject at decode. `deltaE2000`/`deltaEOK` (the parameterized `deltaE(a, b, method)`) drives nearest-token snapping and theme-diff.
- Stack with `@effect-atom` (`ONE_FOLD_ONE_BINDING`) as the token state owner feeding both color-space sinks: the theme atom holds decoded `PlainColorObject` tokens, and `derive` selectors project them purely to the two consumers below — conversion never mutates the atom, no write per frame.
- Stack with `tailwindcss` (`.api/tailwindcss.md`) as the CSS token sink: the OKLCH values authored here — laddered by `steps`/`range`, gamut-fit, `contrastAPCA`/`contrastWCAG21`-gated — are `serialize({ format: "oklch", inGamut: true })`-emitted into the `@theme --color-*` namespace, which tailwind compiles into the `--color-*` custom property plus its `bg-`/`text-`/`ring-` utility family; the palette tailwind emits is a colorjs.io-computed color-space artifact, never a hand-listed hex table, and `display()` emits the serialization the current environment renders, carrying the wide-gamut source on `.color`.
- Stack with `three` `ColorManagement` (`.api/three.md`, `scope:viewer`) as the render color-space sink: the same OKLCH values are gamut-fit and `to("srgb", { inGamut: true })` / `to("srgb-linear")`-converted — colorjs.io's `srgb`/`srgb-linear` space ids are the exact string values of three's `SRGBColorSpace`/`LinearSRGBColorSpace` constants, so no remap table — and the `PlainColorObject.coords` `[r, g, b]` feed `three.Color.setRGB(r, g, b, SRGBColorSpace | LinearSRGBColorSpace)`, where `ColorManagement` transforms source→working. colorjs.io owns OKLCH authoring + gamut fit, three owns the working-space transform, meeting at the sRGB/linear coordinate wire — token color and rendered color are one color-space contract. Beyond the sRGB path, `CAM16_JMh`/`HCT` + `deltaEHCT`/`deltaEITP` supply appearance-correct/HDR-aware color for OpenPBR/scene tokens where the WCAG-oriented sRGB path is insufficient; register those spaces only in the `scope:viewer` bundle.
- Stack with the `react-aria`/`react-stately` color-picker widgets (`.api/react-aria.md`, `.api/react-stately.md`): `useColorArea`/`useColorWheel`/`ColorPickerState` edit channels on react-aria's own `parseColor` model in `HSL`/`HSV`/`HWB`/`sRGB`, then hand off to colorjs.io at the display boundary — `to(space)` for channel-space conversion, `inGamut`/`toGamut` for the picker's gamut fit, `serialize` for the emitted value — so the widget's interactive edit and the token pipeline share this one conversion rail; register `HSL`/`HSV`/`HWB`/`sRGB` in that bundle.
- Extension seam over reinvention: a new named space is `ColorSpace.register(new ColorSpace(SpaceOptions))`, a new derived method is `Color.defineFunction`/`extend`, and internal interception is `hooks.add(name, cb)` — the parameterized registry and hook surface absorb new capability, never a forked conversion helper.

[LOCAL_ADMISSION]:
- Color algebra only; token vocabulary and CSS-variable emission live in `token/theme`/`token/scale`, the render-space handoff is `to("srgb"/"srgb-linear")` → three `Color`/`ColorManagement` in `scope:viewer`, the decode boundary is `effect` `Schema`, and state is `@effect-atom`.
- register the space subset a bundle needs via `./fn` + `ColorSpace.register` — the CSS plane needs `OKLCH`/`OKLab`/`sRGB`/`P3`, the `scope:viewer` bundle adds `sRGB_Linear` + `CAM16_JMh`/`HCT`; pull the full `Color` class only in authoring/non-shipping code.
- one `to(space)`/`contrast(...algorithm)`/`deltaE(...method)`/`serialize(...format)` parameterized call per concern; a new space/algorithm/method is a registry row or an argument value, never a new operation family.

[RAIL_LAW]:
- Package: `colorjs.io`
- Owns: parsing/serialization of CSS Color 4 spaces, named-space conversion, perceptual interpolation (`range`/`mix`/`steps`), gamut mapping (`toGamut`/`toGamutCSS`/`toGamutRayTrace`), max-in-gamut chroma (`maxChroma`, gamut-relative spaces), the parameterized `contrast` and `deltaE` rails, luminance/chromaticity/variation, and the `ColorSpace`/`hooks`/`defineFunction` extension surface
- Accept: `./fn` + explicit `ColorSpace.register` for shipped bundles, OKLCH/OKLab authoring, `Schema.transform`(`parse`/`serialize`) + `Color.try` as the token boundary, `steps`/`range` for palette ramps, `contrast*`/`deltaE*` as parameterized a11y + distance gates, `serialize`/`display` for the `tailwindcss` `@theme --color-*` CSS-variable + displayable-fallback emission, `to("srgb"/"srgb-linear", { inGamut: true })` → `coords` for the three `Color.setRGB`/`ColorManagement` render-space handoff, registry/`defineFunction`/`hooks` for extension
- Reject: hand-rolled hex/HSL math or WCAG ratio arithmetic, a per-space `toRgb`/`toHsl` conversion family instead of `to(space)`, hand-listed palette stops instead of `steps`/`range`, re-minting the OKLCH gamut map instead of `toGamutCSS`, hand-converting OKLCH→display RGB for three instead of `to("srgb"/"srgb-linear")` whose ids match three's color-space constants, importing the full `Color` class where `./fn` + registered spaces suffice, treating parse/serialize as async
