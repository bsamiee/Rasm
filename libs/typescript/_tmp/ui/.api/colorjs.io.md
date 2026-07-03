# [API_CATALOGUE] colorjs.io

`colorjs.io` supplies the `Color` class and its functional twin for wide-gamut color parsing, manipulation, gamut mapping, interpolation, contrast calculation, and multi-space serialization. The default import is the `Color` class with all methods attached; the `colorjs.io/fn` entry point exports tree-shakeable standalone functions operating on `PlainColorObject`. `ColorSpace` and associated types support custom space registration.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `colorjs.io`
- package: `colorjs.io`
- namespace: `colorjs.io` / `colorjs.io/fn` / `colorjs.io/spaces`
- asset: runtime ES module + CommonJS
- rail: color

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: core color value types
- rail: color

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [RAIL]                                                          |
| :-----: | :----------------- | :------------ | :-------------------------------------------------------------- |
|  [01]   | `Color`            | class         | main color instance; default export                             |
|  [02]   | `ColorSpace`       | class         | color space definition and registry                             |
|  [03]   | `Coords`           | type alias    | `[number \| null, number \| null, number \| null]`              |
|  [04]   | `ColorTypes`       | type alias    | `ColorObject \| ColorConstructor \| string \| PlainColorObject` |
|  [05]   | `PlainColorObject` | interface     | `{ space, coords, alpha }` wire shape                           |
|  [06]   | `ColorObject`      | interface     | loose input with optional `spaceId` or `space`                  |
|  [07]   | `ColorConstructor` | interface     | `{ spaceId, coords, alpha }` for serialization                  |

[PUBLIC_TYPE_SCOPE]: interpolation and display option types
- rail: color

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [RAIL]                                                       |
| :-----: | :----------------- | :------------ | :----------------------------------------------------------- |
|  [01]   | `MixOptions`       | interface     | `{ space?, hue?, powerless? }` for `mix`                     |
|  [02]   | `StepsOptions`     | interface     | `{ steps?, maxDeltaE?, space? }` steps                       |
|  [03]   | `RangeOptions`     | interface     | options for `range` interpolation factory                    |
|  [04]   | `Range`            | type alias    | `(p: number) => Color` interpolant                           |
|  [05]   | `ParseOptions`     | interface     | options passed to `Color.get` / `parse`                      |
|  [06]   | `SerializeOptions` | interface     | precision and format options for `toString`                  |
|  [07]   | `SpaceOptions`     | interface     | `{ id, name, base?, fromBase?, toBase?, coords?, formats? }` |

[PUBLIC_TYPE_SCOPE]: color space config types
- rail: color

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY] | [RAIL]                                        |
| :-----: | :------------ | :------------ | :-------------------------------------------- |
|  [01]   | `CoordMeta`   | interface     | `{ name?, type?, range?, refRange? }`         |
|  [02]   | `SpaceFormat` | interface     | parse/serialize format descriptor for a space |
|  [03]   | `RGBOptions`  | interface     | additional options for RGB space constructors |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Color class constructor and factory
- rail: color

| [INDEX] | [SURFACE]                          | [ENTRY_FAMILY] | [RAIL]                                |
| :-----: | :--------------------------------- | :------------- | :------------------------------------ |
|  [01]   | `new Color(color: ColorTypes)`     | constructor    | parse any `ColorTypes` input          |
|  [02]   | `new Color(space, coords, alpha?)` | constructor    | construct from space + raw coords     |
|  [03]   | `Color.get(color, options?)`       | static factory | parse or coerce to `Color`            |
|  [04]   | `Color.try(color, options?)`       | static factory | returns `Color \| null`; non-throwing |

[ENTRYPOINT_SCOPE]: Color instance operations
- rail: color

| [INDEX] | [SURFACE]                      | [ENTRY_FAMILY] | [RAIL]                                     |
| :-----: | :----------------------------- | :------------- | :----------------------------------------- |
|  [01]   | `color.to(space)`              | conversion     | returns new `Color` in target space        |
|  [02]   | `color.set(prop, value)`       | mutation copy  | single coord update; returns new `Color`   |
|  [03]   | `color.set(props)`             | mutation copy  | multi-coord update; returns new `Color`    |
|  [04]   | `color.setAll(coords, alpha?)` | mutation copy  | replace all coords at once                 |
|  [05]   | `color.get(ref)`               | read accessor  | numeric coord value by channel reference   |
|  [06]   | `color.getAll()`               | read accessor  | `PlainColorObject` of all coords           |
|  [07]   | `color.inGamut(space?)`        | gamut test     | boolean gamut membership check             |
|  [08]   | `color.toGamut(options?)`      | gamut clamp    | returns in-gamut `Color` copy              |
|  [09]   | `color.equals(other)`          | value equality | coordinate and space equality              |
|  [10]   | `color.clone()`                | copy           | deep clone of this `Color`                 |
|  [11]   | `color.display(...args)`       | stringify      | CSS-displayable string + `.color` property |
|  [12]   | `color.toString(options?)`     | stringify      | serialized CSS color string                |

[ENTRYPOINT_SCOPE]: interpolation and perceptual operations
- rail: color

| [INDEX] | [SURFACE]                             | [ENTRY_FAMILY]   | [RAIL]                                    |
| :-----: | :------------------------------------ | :--------------- | :---------------------------------------- |
|  [01]   | `color.mix(color2, options?)`         | blend            | proportional color mix                    |
|  [02]   | `color.mix(color2, p, options?)`      | blend            | mix at explicit proportion `p`            |
|  [03]   | `color.range(color2, options?)`       | interpolant      | returns `Range` function `(p) => Color`   |
|  [04]   | `color.steps(color2, options?)`       | discrete steps   | `Color[]` array of sampled steps          |
|  [05]   | `color.lighten(amount?)`              | variation        | perceptually lighter color                |
|  [06]   | `color.darken(amount?)`               | variation        | perceptually darker color                 |
|  [07]   | `color.luminance`                     | getter           | relative luminance (0–1)                  |
|  [08]   | `color.contrast(background, method?)` | contrast         | contrast ratio by selected algorithm      |
|  [09]   | `color.contrastWCAG21(background)`    | contrast         | WCAG 2.1 contrast ratio                   |
|  [10]   | `color.contrastAPCA(background)`      | contrast         | APCA perceptual contrast                  |
|  [11]   | `color.deltaE(color2, method?)`       | perceptual delta | color difference by selected ΔE algorithm |
|  [12]   | `color.deltaE2000(color2)`            | perceptual delta | CIE ΔE2000                                |
|  [13]   | `color.deltaEOK(color2)`              | perceptual delta | Oklab ΔEok                                |

[ENTRYPOINT_SCOPE]: ColorSpace static methods
- rail: color

| [INDEX] | [SURFACE]                    | [ENTRY_FAMILY] | [RAIL]                                     |
| :-----: | :--------------------------- | :------------- | :----------------------------------------- |
|  [01]   | `ColorSpace.register(space)` | registration   | register custom or built-in space          |
|  [02]   | `ColorSpace.get(id)`         | lookup         | retrieve registered space by ID or alias   |
|  [03]   | `Color.Space`                | alias          | `ColorSpace` re-exported on the `Color` ns |
|  [04]   | `Color.spaces`               | registry       | map of all registered `ColorSpace` objects |
|  [05]   | `Color.parse`                | parse fn       | `parse(str, options?)` standalone          |

## [04]-[IMPLEMENTATION_LAW]

[COLOR_TOPOLOGY]:
- default import `Color` from `colorjs.io` carries all methods and the full built-in space registry
- `colorjs.io/fn` provides tree-shakeable standalone functions; each takes the color as first argument (type `ColorTypes`)
- `colorjs.io/spaces` re-exports individual space modules for explicit tree-shaking
- `PlainColorObject` is the canonical internal wire shape: `{ space: ColorSpace, coords: Coords, alpha: number | null }`
- `coords` entries may be `null` to represent missing/powerless channels
- `to`, `set`, `mix`, `toGamut` always return new `Color` instances; the original is not mutated
- `Range` is a `(p: number) => Color` function; values outside 0–1 extrapolate

[LOCAL_ADMISSION]:
- Produce `PlainColorObject` at domain boundaries when passing colors between modules without carrying the `Color` prototype.
- `Color.try` for untrusted parse paths; never let raw parse exceptions propagate as domain errors.
- Persist colors as CSS strings or `{ spaceId, coords, alpha }` records; reconstruct via `new Color(stored)` on receipt.
- Select ΔE algorithm by visual context: WCAG2.1 for accessibility gates, ΔEok for perceptual similarity, ΔE2000 for print/manufacturing.

[RAIL_LAW]:
- package: `colorjs.io`
- owns: wide-gamut color arithmetic, gamut mapping, interpolation, contrast, ΔE
- accept: any CSS color string, `PlainColorObject`, space + coords constructor
- reject: hand-rolling gamut mapping, WCAG contrast, or ΔE formulas against this package
