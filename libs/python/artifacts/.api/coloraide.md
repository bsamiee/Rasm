# [PY_ARTIFACTS_API_COLORAIDE]

`coloraide` supplies the pure-Python color-engine surface for the artifacts figures/color rail: a single `Color` object that parses CSS/named/coordinate input, converts across registered color spaces, fits out-of-gamut coordinates through a registered gamut-mapping method, and applies color-vision-deficiency filters. The package owner composes `Color`, `Color.fit`/`Color.in_gamut`, and `Color.filter` into the gamut-map and CVD figure legs; it removes any hand-rolled sRGB/OKLCH conversion matrices and any bespoke Daltonization/protan-deutan-tritan transform because every space, fit method, and CVD filter is registered in-package, and it never re-implements the delta-E or chromatic-adaptation math coloraide already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `coloraide`
- package: `coloraide`
- import: `coloraide`
- owner: `artifacts`
- rail: color
- version: `8.10`
- license: `MIT`
- entry points: library use is import-only; no console script
- capability: one `Color` object owning CSS/named/coordinate parsing, conversion across the 27 base-registered color spaces (sRGB/OKLCH/OKLab/Lab/LCH/Display-P3/Rec.2020/HSL/HWB and more), gamut mapping through six registered fit methods (`raytrace` default, `oklch-chroma`, `lch-chroma`, `minde-chroma`, `scale`, `scale-luminance`), CVD simulation via the `protan`/`deutan`/`tritan` filters plus eight W3C filter effects, eight base delta-E distance metrics, WCAG21 contrast, six interpolation methods (`linear`/`continuous`/`bspline`/`natural`/`monotone`/`css-linear`), CCT/blackbody and chromaticity transforms, interpolation/mixing/harmonies/averaging, masking/layering composition, and a plugin `register`/`deregister` surface; `coloraide.everything.ColorAll` preloads all 74 spaces plus the extended fit (`hct-chroma`), delta-E (`99o`/`cam02`/`cam16`/`hct`), and CAT (`cat02`/`cat16`/`cmccat2000`/`cmccat97`/`sharp`/`von-kries`/`xyz-scaling`) plugins

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

`distance`/`delta_e` measure perceptual difference (base delta-E methods `76` default, `2000`, `94`, `cmc`, `hyab`, `itp`, `jz`, `ok`; `ColorAll` adds `99o`/`cam02`/`cam16`/`hct`); `contrast` is WCAG21 by default; `interpolate`/`steps`/`mix`/`discrete`/`weighted_mix` build gradients keyed by the interpolation `method` row (`linear` default, `continuous`, `bspline`, `natural`, `monotone`, `css-linear`); `harmony`/`closest`/`average` derive palettes; `register`/`deregister` extend the engine plugin maps; `match` parses one color from a string.

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
|  [15]   | `Color.discrete`    | `discrete(colors, *, space=None, out_space=None, steps=None, max_steps=1000, max_delta_e=0, delta_e=None, delta_e_args=None, domain=None, **interpolate_args)` -> `Interpolator[Self]` (classmethod)                                            | discrete (stepped, non-blended) interpolator    |
|  [16]   | `Color.weighted_mix`| `weighted_mix(colors, weights=None, *, space=None, out_space=None, method=None, premultiplied=True, carryforward=False, powerless=False, hue='shorter', **kwargs)` -> `Self` (classmethod)                                                      | weighted blend of N colors                      |

[ENTRYPOINT_SCOPE]: composition leg
- rail: color

`mask` zeroes/keeps channels for composition; `layer` blend-composites a stack of colors (W3C blend modes + Porter-Duff operators); `mutate`/`normalize`/`random` are the in-place mint/normalize rows; `to_dict` serializes the color to a structured mapping (round-trips through `Color(dict)`); `is_achromatic`/`within` are predicate rows.

| [INDEX] | [SURFACE]          | [CALL_SHAPE]                                                                                                       | [CAPABILITY]                                       |
| :-----: | :----------------- | :---------------------------------------------------------------------------------------------------------------- | :------------------------------------------------- |
|  [01]   | `Color.mask`       | `mask(channel, *, invert=False, in_place=False)` -> `Self`                                                        | keep/zero channels for layered composition         |
|  [02]   | `Color.layer`      | `layer(colors, *, blend='normal', operator='source-over', space=None, out_space=None)` -> `Self` (classmethod)    | blend/Porter-Duff composite a color stack          |
|  [03]   | `Color.to_dict`    | `to_dict(*, nans=True, precision=None, rounding=None)` -> `Mapping[str, Any]`                                     | structured mapping (round-trips via `Color(dict)`) |
|  [04]   | `Color.normalize`  | `normalize(*, nans=True)` -> `Self`                                                                               | normalize powerless/undefined channels in place    |
|  [05]   | `Color.is_achromatic` | `is_achromatic(**kwargs)` -> `bool`                                                                            | test whether the color is achromatic (gray)        |

[ENTRYPOINT_SCOPE]: CCT, chromaticity, and spectral leg
- rail: color

`blackbody` mints a Planckian-locus color at a temperature; `cct` reads the correlated colour temperature and Duv of a color; `chromatic_adaptation` adapts XYZ between white points by CAT method; `from_wavelength`/`wavelength` map a single wavelength to/from a color; `xy`/`uv`/`chromaticity`/`split_chromaticity` read chromaticity coordinates; `white` reads the space white point.

| [INDEX] | [SURFACE]                     | [CALL_SHAPE]                                                                                                                                                  | [CAPABILITY]                                              |
| :-----: | :---------------------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------ | :-------------------------------------------------------- |
|  [01]   | `Color.blackbody`             | `blackbody(space, temp, duv=0.0, *, method=None, scale=True, scale_space=None, max_saturation=True, clip_negative=False, preserve_luminance=False, **kwargs)` -> `Self` (classmethod) | Planckian-locus color at a temperature (CCT method row)   |
|  [02]   | `Color.cct`                   | `cct(*, method=None, **kwargs)` -> `Vector`                                                                                                                   | correlated colour temperature and Duv of the color        |
|  [03]   | `Color.chromatic_adaptation`  | `chromatic_adaptation(w1, w2, xyz, *, method=None)` -> `Vector` (classmethod)                                                                                 | adapt XYZ between white points by CAT method               |
|  [04]   | `Color.from_wavelength`       | `from_wavelength(space, wavelength, *, white=None, scale=True, scale_space=None, max_saturation=True, clip_negative=False, preserve_luminance=False)` -> `Self` (classmethod) | mint a color from a single spectral wavelength             |
|  [05]   | `Color.xy` / `Color.uv`       | `xy(**kwargs)` / `uv(mode='1976', *, white=None)` -> `Vector`                                                                                                 | CIE 1931 `xy` / 1976 `u'v'` chromaticity of the color      |
|  [06]   | `Color.white`                 | `white(cspace='xyz')` -> `Vector`                                                                                                                             | white point of the current space                           |

## [04]-[IMPLEMENTATION_LAW]

[COLOR_ENGINE]:
- import: `from coloraide import Color` at boundary scope only; module-level import is banned by the manifest import policy; the all-plugins variant is `from coloraide.everything import ColorAll` when a space/filter outside the base set is required.
- object axis: one `Color` owns parse, convert, fit, filter, distance, and interpolation; arity and modality live in method arguments (`space`, `method`, `name`, `**kwargs`), never a per-operation builder type; `new`/`clone`/`update` mint or rewrite without a parallel constructor family.
- gamut-map axis: `fit` is the single gamut-mapping surface keyed by `method`; `raytrace`/`oklch-chroma`/`lch-chroma`/`minde-chroma`/`scale`/`scale-luminance` are method rows, never parallel mapping functions; `in_gamut` is the predicate row and `clip` is the hard-clip row; conversion-time fitting flows through `convert(space, fit=...)`.
- CVD axis: `filter` is the single filter surface keyed by `name`; `protan`/`deutan`/`tritan` are the CVD rows and `brightness`/`contrast`/`saturate`/`hue-rotate`/`grayscale`/`sepia`/`invert`/`opacity` are the W3C rows; CVD simulation is a `name` row with `amount`, never a hand-rolled Daltonization transform.
- distance axis: `delta_e` selects the perceptual metric row (base `76` default, `2000`, `94`, `cmc`, `hyab`, `itp`, `jz`, `ok`; `ColorAll` adds `99o`/`cam02`/`cam16`/`hct`) and `distance` is the raw Euclidean row; `closest` folds `delta_e` over a candidate set.
- interpolation axis: `interpolate`/`steps`/`mix`/`discrete`/`weighted_mix` are rows over one interpolation surface keyed by `method` (`linear` default, `continuous`, `bspline`, `natural`, `monotone`, `css-linear`); `hue` strategy, `premultiplied` alpha, `domain`, `padding`, and `progress` easing (`stop`/`hint`/`cubic_bezier`/`ease*`) are arguments, never parallel gradient functions. `interpolate`/`discrete` return a reusable `Interpolator` object that is itself callable `(point: float) -> Color` and carries `.steps(...)`/`.discretize(...)` plus `.domain`/`.extrapolate`; the owner builds the interpolator once and samples it per gradient stop, never re-deriving the curve per sample.
- CCT axis: `blackbody` mints a Planckian-locus color and `cct` reads temperature+Duv keyed by `method` (base `ohno-2013`/`robertson-1968`); `chromatic_adaptation` adapts XYZ between white points keyed by CAT `method` (base `bradford`; `ColorAll` adds `cat02`/`cat16`/`cmccat2000`/`cmccat97`/`sharp`/`von-kries`/`xyz-scaling`); CCT is a `method` row, never a hand-rolled Planckian fit.
- plugin axis: `register`/`deregister` extend the engine maps (`CS_MAP`, `FIT_MAP`, `FILTER_MAP`, `DE_MAP`, `CAT_MAP`, `CONTRAST_MAP`, `INTERPOLATE_MAP`, `CCT_MAP`); the base `Color` registers 27 spaces, `everything.ColorAll` registers 74; new spaces, fit methods, and filters are registered plugins on the class, never local conversion kernels.
- evidence: each color captures the resolved space, channel vector, alpha, in-gamut flag, applied fit method, applied filter name and amount, delta-E metric, and (when read) CCT/Duv as a color receipt.
- boundary: coloraide owns color parsing, conversion, gamut mapping, CVD simulation, distance, CCT, and chromaticity with no native dependency; numeric coordinate vectors feed the figures and visuals owners directly; rasterization and plotting stay outside this package.

[INTEGRATION_STACK]:
- `coloraide` ↔ `colour-science`: `colour` owns spectral/colorimetric truth (SPD → XYZ, ICC-grade appearance models, named CIE RGB primaries, reference CCT estimation); `coloraide` owns per-color CSS/named parsing, gamut mapping, and CVD presentation. The seam is the numeric XYZ vector: a measured XYZ from `colour.sd_to_XYZ` enters `coloraide` as `Color('xyz-d65', coords)` for gamut-mapped CSS output; `coloraide` is never used for spectral integration and `colour` is never used for CSS string parsing.
- `coloraide` → figures/imaging owner: the channel `Vector` (`coords`/`get`/`to_dict`) and serialized `to_string` feed the figures owner and the imaging codec directly as numeric data, never via a render call inside this package.

[RAIL_LAW]:
- Package: `coloraide`
- Owns: CSS/named/coordinate color parsing, conversion across 27 base (74 in `ColorAll`) registered color spaces, gamut mapping through six fit methods, CVD and W3C filtering, delta-E distance, WCAG21 contrast, six-method interpolation/mixing/harmonies/averaging, masking/layering composition, CCT/blackbody and chromaticity transforms, and a plugin registration surface
- Accept: gamut-map and CVD color transforms feeding the figures/color, visuals, and document owners; XYZ vectors handed off from `colour-science` for CSS/gamut presentation
- Reject: wrapper-renames of `fit`/`filter`/`convert`; a hand-rolled gamut-mapping, Daltonization, or Planckian-CCT transform where a registered method/filter exists; a parallel color type per space; local sRGB/OKLCH/Lab conversion matrices the registered spaces own; using `coloraide` for spectral sd→XYZ integration (that is `colour-science`'s domain); identity minting the runtime owns
