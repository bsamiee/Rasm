# [PY_ARTIFACTS_API_COLORAIDE]

`coloraide` supplies the pure-Python color-engine surface for the artifacts figures/color rail: a single `Color` object that parses CSS/named/coordinate input, converts across registered color spaces, fits out-of-gamut coordinates through a registered gamut-mapping method, and applies color-vision-deficiency filters. The package owner composes `Color`, `Color.fit`/`Color.in_gamut`, and `Color.filter` into the gamut-map and CVD figure legs; it removes any hand-rolled sRGB/OKLCH conversion matrices and any bespoke Daltonization/protan-deutan-tritan transform because every space, fit method, and CVD filter is registered in-package, and it never re-implements the delta-E or chromatic-adaptation math coloraide already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `coloraide`
- package: `coloraide`
- import: `coloraide`
- owner: `artifacts`
- rail: color
- installed: `8.8.1` reflected via `assay api` on cp315
- entry points: library use is import-only; no console script
- capability: one `Color` object owning CSS/named/coordinate parsing, conversion across registered color spaces (sRGB/OKLCH/Lab/HCT and 100+ more), gamut mapping through six registered fit methods (`raytrace` default, `oklch-chroma`, `lch-chroma`, `minde-chroma`, `scale`, `scale-luminance`), CVD simulation via the `protan`/`deutan`/`tritan` filters plus W3C filter effects, eight delta-E distance metrics, WCAG21 contrast, interpolation/mixing/harmonies, and a plugin `register`/`deregister` surface; `coloraide.everything.ColorAll` preloads every registered plugin

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: color object and match root
- rail: color

`Color` is the single engine object; instances parse on construction and every transform returns `Self` for chaining. `ColorMatch` carries a parse hit from `Color.match`. `ColorAll` (from `coloraide.everything`) is the `Color` subclass with every space, fit, filter, delta-E, CAT, and CCT plugin registered. `stop`/`hint`/`cubic_bezier`/`linear` and the `ease*` callables are interpolation easing helpers passed into `interpolate`/`steps`.

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY]      | [RAIL]                                                      |
| :-----: | :---------------------------------------------- | :----------------- | :---------------------------------------------------------- |
|  [01]   | `Color`                                         | color engine       | parse/convert/fit/filter/distance object, base plugin set   |
|  [02]   | `everything.ColorAll`                           | color engine       | `Color` subclass with every registered plugin preloaded     |
|  [03]   | `ColorMatch`                                    | match record       | `color`/`start`/`end` parse hit from `Color.match`          |
|  [04]   | `stop`                                          | interpolation stop | positioned color stop for `interpolate`/`steps`             |
|  [05]   | `hint`                                          | interpolation hint | midpoint hint between two interpolation stops               |
|  [06]   | `cubic_bezier`                                  | easing factory     | cubic-bezier easing function builder                        |
|  [07]   | `linear`                                        | easing function    | identity (linear) progress easing                           |
|  [08]   | `ease` / `ease_in` / `ease_out` / `ease_in_out` | easing function    | CSS-named easing progress functions                         |
|  [09]   | `NaN`                                           | sentinel           | `float('nan')` channel sentinel for powerless/undefined hue |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Color` construct and convert
- rail: color

Construction parses any `ColorInput` (CSS string, named color, `(space, coords)` shape, or another `Color`); `new`/`clone` mint without re-parsing and `update` rewrites coordinates in place. `convert` moves to a registered space and optionally fits during conversion.

| [INDEX] | [SURFACE]         | [CALL_SHAPE]                                                            | [CAPABILITY]                                   |
| :-----: | :---------------- | :---------------------------------------------------------------------- | :--------------------------------------------- |
|  [01]   | `Color`           | `Color(color, data=None, alpha=1.0, **kwargs)`                          | parse input into a color in its detected space |
|  [02]   | `Color.new`       | `new(color, data=None, alpha=1.0, **kwargs)` (classmethod)              | mint a sibling color of the same class         |
|  [03]   | `Color.clone`     | `clone()` -> `Self`                                                     | duplicate without re-parsing                   |
|  [04]   | `Color.update`    | `update(color, data=None, alpha=1.0, *, norm=True, **kwargs)` -> `Self` | overwrite coordinates from new input in place  |
|  [05]   | `Color.convert`   | `convert(space, *, fit=False, in_place=False, norm=True)` -> `Self`     | convert to a registered space, optional fit    |
|  [06]   | `Color.to_string` | `to_string(**kwargs)` -> `str`                                          | serialize to a CSS/space string                |
|  [07]   | `Color.get`       | `get(name, *, nans=True, precision=None, rounding=None)` -> `float      | Vector`                                        | read one or several channel coordinates |
|  [08]   | `Color.set`       | `set(name, value=None, *, nans=True)` -> `Self`                         | write a channel by value or callable           |
|  [09]   | `Color.coords`    | `coords(*, nans=True, precision=None, rounding=None)` -> `Vector`       | read the color-channel vector (no alpha)       |

[ENTRYPOINT_SCOPE]: gamut-map leg
- rail: color

`in_gamut` tests membership in a target space; `fit` maps an out-of-gamut color back inside using a registered fit `method` (`raytrace` default); `clip` is the hard-clip row. The fit `method` axis is `raytrace`, `oklch-chroma`, `lch-chroma`, `minde-chroma`, `scale`, `scale-luminance`.

| [INDEX] | [SURFACE]        | [CALL_SHAPE]                                                  | [CAPABILITY]                                       |
| :-----: | :--------------- | :------------------------------------------------------------ | :------------------------------------------------- |
|  [01]   | `Color.in_gamut` | `in_gamut(space=None, *, tolerance=None, **kwargs)` -> `bool` | test whether coordinates lie inside a target gamut |
|  [02]   | `Color.fit`      | `fit(space=None, *, method=None, **kwargs)` -> `Self`         | gamut-map into a space via a registered fit method |
|  [03]   | `Color.clip`     | `clip(space=None)` -> `Self`                                  | hard-clip coordinates into a target gamut          |

[ENTRYPOINT_SCOPE]: CVD and filter leg
- rail: color

`filter` applies a registered filter by `name`; the CVD rows are `protan`, `deutan`, `tritan`; the W3C filter-effect rows are `brightness`, `contrast`, `saturate`, `hue-rotate`, `grayscale`, `sepia`, `invert`, `opacity`. `amount` is the per-filter strength; `space`/`out_space` control the working and output space.

| [INDEX] | [SURFACE]      | [CALL_SHAPE]                                                                                   | [CAPABILITY]                              |
| :-----: | :------------- | :--------------------------------------------------------------------------------------------- | :---------------------------------------- |
|  [01]   | `Color.filter` | `filter(name, amount=None, *, space=None, out_space=None, in_place=False, **kwargs)` -> `Self` | apply CVD/W3C filter by registered `name` |

[ENTRYPOINT_SCOPE]: distance, contrast, interpolation, plugins
- rail: color

`distance`/`delta_e` measure perceptual difference (delta-E methods `76` default, `2000`, `94`, `cmc`, `hyab`, `itp`, `jz`, `ok`); `contrast` is WCAG21 by default; `interpolate`/`steps`/`mix` build gradients; `harmony`/`closest`/`average` derive palettes; `register`/`deregister` extend the engine plugin maps; `match` parses one color from a string.

| [INDEX] | [SURFACE]           | [CALL_SHAPE]                                                                                                                                                                                                                                   | [CAPABILITY]                                    |
| :-----: | :------------------ | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :---------------------------------------------- |
|  [01]   | `Color.distance`    | `distance(color, *, space='lab')` -> `float`                                                                                                                                                                                                   | Euclidean distance in a space                   |
|  [02]   | `Color.delta_e`     | `delta_e(color, *, method=None, **kwargs)` -> `float`                                                                                                                                                                                          | perceptual delta-E by registered method         |
|  [03]   | `Color.contrast`    | `contrast(color, method=None)` -> `float`                                                                                                                                                                                                      | contrast ratio (WCAG21 default)                 |
|  [04]   | `Color.luminance`   | `luminance(*, white=(0.3127, 0.329))` -> `float`                                                                                                                                                                                               | relative luminance                              |
|  [05]   | `Color.interpolate` | `interpolate(colors, *, space=None, out_space=None, progress=None, hue='shorter', premultiplied=True, extrapolate=False, domain=None, method=None, padding=None, carryforward=None, powerless=None, **kwargs)` -> `Interpolator` (classmethod) | build an interpolator across colors             |
|  [06]   | `Color.steps`       | `steps(colors, *, steps=2, max_steps=1000, max_delta_e=0, delta_e=None, delta_e_args=None, **interpolate_args)` -> `list[Self]` (classmethod)                                                                                                  | sample N discrete colors along an interpolation |
|  [07]   | `Color.mix`         | `mix(color, percent=0.5, *, in_place=False, **interpolate_args)` -> `Self`                                                                                                                                                                     | blend two colors                                |
|  [08]   | `Color.harmony`     | `harmony(name, *, space=None, out_space=None, **kwargs)` -> `list[Self]`                                                                                                                                                                       | derive a color harmony palette                  |
|  [09]   | `Color.closest`     | `closest(colors, *, method=None, **kwargs)` -> `Self`                                                                                                                                                                                          | nearest color from a set by delta-E             |
|  [10]   | `Color.average`     | `average(colors, weights=None, *, space=None, out_space=None, premultiplied=True, carryforward=False, **kwargs)` -> `Self` (classmethod)                                                                                                       | weighted average of colors                      |
|  [11]   | `Color.register`    | `register(plugin, *, overwrite=False, silent=False)` -> `None` (classmethod)                                                                                                                                                                   | register space/fit/filter/delta-E plugins       |
|  [12]   | `Color.deregister`  | `deregister(plugin, *, silent=False)` -> `None` (classmethod)                                                                                                                                                                                  | remove a registered plugin by name              |
|  [13]   | `Color.match`       | `match(string, start=0, fullmatch=False)` -> `ColorMatch                                                                                                                                                                                       | None` (classmethod)                             | parse one color out of a string |
|  [14]   | `Color.is_nan`      | `is_nan(name)` -> `bool`                                                                                                                                                                                                                       | test a channel for the powerless/NaN sentinel   |

## [04]-[IMPLEMENTATION_LAW]

[COLOR_ENGINE]:
- import: `from coloraide import Color` at boundary scope only; module-level import is banned by the manifest import policy; the all-plugins variant is `from coloraide.everything import ColorAll` when a space/filter outside the base set is required.
- object axis: one `Color` owns parse, convert, fit, filter, distance, and interpolation; arity and modality live in method arguments (`space`, `method`, `name`, `**kwargs`), never a per-operation builder type; `new`/`clone`/`update` mint or rewrite without a parallel constructor family.
- gamut-map axis: `fit` is the single gamut-mapping surface keyed by `method`; `raytrace`/`oklch-chroma`/`lch-chroma`/`minde-chroma`/`scale`/`scale-luminance` are method rows, never parallel mapping functions; `in_gamut` is the predicate row and `clip` is the hard-clip row; conversion-time fitting flows through `convert(space, fit=...)`.
- CVD axis: `filter` is the single filter surface keyed by `name`; `protan`/`deutan`/`tritan` are the CVD rows and `brightness`/`contrast`/`saturate`/`hue-rotate`/`grayscale`/`sepia`/`invert`/`opacity` are the W3C rows; CVD simulation is a `name` row with `amount`, never a hand-rolled Daltonization transform.
- distance axis: `delta_e` selects the perceptual metric row (`76` default, `2000`, `94`, `cmc`, `hyab`, `itp`, `jz`, `ok`) and `distance` is the raw Euclidean row; `closest` folds `delta_e` over a candidate set.
- plugin axis: `register`/`deregister` extend the engine maps (`CS_MAP`, `FIT_MAP`, `FILTER_MAP`, `DE_MAP`, `CAT_MAP`, `CONTRAST_MAP`, `INTERPOLATE_MAP`, `CCT_MAP`); new spaces, fit methods, and filters are registered plugins on the class, never local conversion kernels.
- evidence: each color captures the resolved space, channel vector, alpha, in-gamut flag, applied fit method, applied filter name and amount, and delta-E metric as a color receipt.
- boundary: coloraide owns color parsing, conversion, gamut mapping, CVD simulation, and distance with no native dependency; numeric coordinate vectors feed the figures and visuals owners directly; rasterization and plotting stay outside this package.

[RAIL_LAW]:
- Package: `coloraide`
- Owns: CSS/named/coordinate color parsing, conversion across 100+ registered color spaces, gamut mapping through six fit methods, CVD and W3C filtering, delta-E distance, WCAG21 contrast, interpolation/mixing/harmonies, and a plugin registration surface
- Accept: gamut-map and CVD color transforms feeding the figures/color, visuals, and document owners
- Reject: wrapper-renames of `fit`/`filter`/`convert`; a hand-rolled gamut-mapping or Daltonization transform where a registered method/filter exists; a parallel color type per space; local sRGB/OKLCH/Lab conversion matrices the registered spaces own; identity minting the runtime owns
