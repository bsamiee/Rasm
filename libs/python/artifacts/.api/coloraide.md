# [PY_ARTIFACTS_API_COLORAIDE]

`coloraide` supplies the pure-Python color-engine surface for the `graphic/color/derive#DERIVE` palette source and the `graphic/color/managed#MANAGED` egress provenance: a single `Color` object that parses CSS/named/coordinate input, converts across registered color spaces, fits out-of-gamut coordinates through a registered gamut-mapping method, applies CVD plus W3C filter effects, measures perceptual difference and contrast, builds smooth/discrete/spectral interpolations, blend-/Porter-Duff-composites color stacks, derives harmonies, and reads CCT/chromaticity. In `derive`, `everything.ColorAll` is the single all-plugins engine the closed `ColorOp` family folds onto `ColorReceipt` — `Color.fit`/`in_gamut` own the gamut leg, `Color.filter` the CVD+effect leg, `Color.steps`/`discrete`/`interpolate` the palette leg keyed by interpolation `method`+`hue`, `Color.layer`/`weighted_mix` the compositing leg, `Color.harmony`/`closest`/`average` the wheel/seed leg, `Color.blackbody`/`cct` the Planckian leg, and the OKLab-perceptual `Color.delta_e` family (`ok`/`jz`/`hyab`/`itp`/`99o`/`cam02`/`cam16`/`hct`) the difference rows `colour-science` has no Lab-array form for. The owner removes any hand-rolled sRGB/OKLCH conversion matrix, any bespoke Daltonization/protan-deutan-tritan transform, and any local gamut-fit/Planckian fit because every space, fit method, filter, delta-E metric, CAT, and CCT method is a registered plugin on the class; it never re-implements the spectral SPD→XYZ integration `colour-science` owns and never parses CSS inside `colour-science`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `coloraide`
- package: `coloraide`
- import: `coloraide`
- owner: `artifacts`
- rail: color
- version: `8.10`
- license: `MIT`
- target: pure-Python (zero native extension, abi-agnostic); only runtime dep is the stdlib — no NumPy at import (its own `algebra` module carries the linear-algebra kernels)
- floor: runs on cp315 — `coloraide 8.10` resolved and imported under `python3.15`; no `python_version` marker (no compiled artifact, no ABI gate)
- entry points: library use is import-only; no console script
- capability: one `Color` object owning CSS/named/coordinate parsing, conversion across the 27 base-registered color spaces (`everything.ColorAll` registers 77), gamut mapping through six base fit methods (`raytrace` default; `ColorAll` adds `hct-chroma` + `oklch-cubic` for eight), CVD simulation (`protan`/`deutan`/`tritan`) plus eight W3C filter effects over one 11-row `FILTER_MAP`, eight base delta-E metrics (`ColorAll` adds `99o`/`cam02`/`cam16`/`hct`/`helmlab` for thirteen), WCAG21 contrast (`ColorAll` adds `lstar`), six base interpolation methods (`ColorAll` adds `catrom`/`spectral`/`spectral-continuous` for nine), Planckian-locus CCT/blackbody over `ohno-2013`/`robertson-1968`, chromatic adaptation (`bradford` base; `ColorAll` adds `cat02`/`cat16`/`cmccat2000`/`cmccat97`/`sharp`/`von-kries`/`xyz-scaling`), chromaticity (`xy`/`uv`/`chromaticity`/`split_chromaticity`/`convert_chromaticity`), spectral `wavelength`/`from_wavelength` mapping, interpolation/mixing/harmonies/averaging, masking/layering composition (16 blend modes + 15 Porter-Duff operators), Pointer's-gamut tests, and a plugin `register`/`deregister` surface over eight engine maps

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: color object, match root, top-level exports
- rail: color

`Color` is the single engine object; instances parse on construction and every mutating transform returns `Self` for chaining. `ColorMatch` carries a parse hit from `Color.match`. `ColorAll` (from `coloraide.everything`) is the `Color` subclass with every space, fit, filter, delta-E, CAT, CCT, and interpolation plugin registered — the owner imports this as the working engine. `stop`/`hint`/`cubic_bezier`/`linear` and the `ease*` callables are interpolation easing helpers passed into `interpolate`/`steps`/`discrete`. `NaN` is the channel sentinel for a powerless/undefined hue.

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY]      | [RAIL]                                                       |
| :-----: | :---------------------------------------------- | :----------------- | :----------------------------------------------------------- |
|  [01]   | `Color`                                         | color engine       | parse/convert/fit/filter/distance object, base 27-space set  |
|  [02]   | `everything.ColorAll`                           | color engine       | `Color` subclass, every plugin preloaded (77 spaces)         |
|  [03]   | `ColorMatch`                                    | match record       | `color`/`start`/`end` parse hit from `Color.match`           |
|  [04]   | `interpolate.Interpolator`                      | gradient curve     | reusable callable curve returned by `interpolate`/`discrete` |
|  [05]   | `stop`                                          | interpolation stop | `stop(color, value)` positioned color stop                   |
|  [06]   | `hint`                                          | interpolation hint | `hint(mid)` -> easing callable; midpoint hint between stops  |
|  [07]   | `cubic_bezier`                                  | easing factory     | `cubic_bezier(x1, y1, x2, y2)` -> easing callable            |
|  [08]   | `linear`                                        | easing function    | `linear(t)` identity (linear) progress easing                |
|  [09]   | `ease` / `ease_in` / `ease_out` / `ease_in_out` | easing function    | CSS-named easing progress (tunable `a`/`b`/`c`/`p1`/`p2`)    |
|  [10]   | `NaN`                                           | sentinel           | `float('nan')` channel sentinel for powerless/undefined hue  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Color` construct, convert, channel read/write
- rail: color

Construction parses any `ColorInput` (CSS string, named color, `(space, coords)` shape, a `to_dict` mapping, or another `Color`). The mint family `Color`/`new`/`mutate` shares `(color, data=None, alpha=1.0, **kwargs)`, `update` adds `*, norm=True`, and the reader family `get`/`coords`/`alpha`/`Y` shares the `*, nans=True, precision=None, rounding=None` keyword tail. `new`/`clone` mint without re-parsing, `mutate` rewrites in place, `update` overwrites coordinates keeping the current space, `random` mints in a space; `convert(space, *, fit=False, in_place=False, norm=True)` moves to a registered space and optionally fits. `get`/`set` discriminate one channel name against a `list`/`dict`; `coords` reads the vector, `alpha`/`Y` the alpha and luminance, `space` the id.

| [INDEX] | [SURFACE]       | [CALL_SHAPE]                                            | [CAPABILITY]                                       |
| :-----: | :-------------- | :------------------------------------------------------ | :------------------------------------------------- |
|  [01]   | `Color`         | `Color(color, data=None, alpha=1.0, **kwargs)`          | parse input into a color in its detected space     |
|  [02]   | `Color.new`     | `new(color, …)` -> `Self` (classmethod)                 | mint a sibling color of the same class             |
|  [03]   | `Color.clone`   | `clone()` -> `Self`                                     | duplicate without re-parsing                       |
|  [04]   | `Color.mutate`  | `mutate(color, …)` -> `Self`                            | rewrite in place from new input                    |
|  [05]   | `Color.update`  | `update(color, …, *, norm=True)` -> `Self`              | overwrite coords from new input, keep space        |
|  [06]   | `Color.random`  | `random(space, *, limits=None)` -> `Self` (classmethod) | mint a random color in a space (`limits`)          |
|  [07]   | `Color.convert` | `convert(space, *, fit=False, …)` -> `Self`             | convert to a registered space, optional fit        |
|  [08]   | `Color.space`   | `space()` -> `str`                                      | read the current color-space id                    |
|  [09]   | `Color.get`     | `get(name)` -> `float \| Vector`                        | read one channel (str) or several (`list`/`tuple`) |
|  [10]   | `Color.set`     | `set(name, value=None, *, nans=True)` -> `Self`         | write one channel or many (str->value dict)        |
|  [11]   | `Color.coords`  | `coords()` -> `Vector`                                  | read the color-channel vector (no alpha)           |
|  [12]   | `Color.alpha`   | `alpha()` -> `float`                                    | read the alpha channel                             |
|  [13]   | `Color.Y`       | `Y()` -> `float`                                        | read the luminance (Y) channel                     |

[ENTRYPOINT_SCOPE]: serialize leg
- rail: color

`to_string` selects `fit`/`precision`/`color`/`comma`/`percent`/`hex`/`names`/`upper` through kwargs per the space's CSS serializer; `serialize` is the thin gamut-aware string row; `to_dict` carries the reader tail `*, nans=True, precision=None, rounding=None` and round-trips through `Color(dict)`.

| [INDEX] | [SURFACE]         | [CALL_SHAPE]                                 | [CAPABILITY]                                       |
| :-----: | :---------------- | :------------------------------------------- | :------------------------------------------------- |
|  [01]   | `Color.to_string` | `to_string(**kwargs)` -> `str`               | serialize to a CSS/space string                    |
|  [02]   | `Color.serialize` | `serialize(*, fit=False, **kwargs)` -> `str` | string egress with optional in-serialize fit       |
|  [03]   | `Color.to_dict`   | `to_dict()` -> `Mapping[str, Any]`           | structured mapping (round-trips via `Color(dict)`) |

[ENTRYPOINT_SCOPE]: gamut-map leg
- rail: color

`in_gamut` tests membership in a target space; `fit` maps an out-of-gamut color back inside using a registered fit `method` (`raytrace` default); `clip` is the hard-clip row; conversion-time fitting flows through `convert(space, fit=...)`. Pointer's gamut (the gamut of real surface colors) has its own predicate/fit pair.

| [INDEX] | [SURFACE]                 | [CALL_SHAPE]                                                  | [CAPABILITY]                              |
| :-----: | :------------------------ | :------------------------------------------------------------ | :---------------------------------------- |
|  [01]   | `Color.in_gamut`          | `in_gamut(space=None, *, tolerance=None, **kwargs)` -> `bool` | test membership in a target gamut         |
|  [02]   | `Color.fit`               | `fit(space=None, *, method=None, **kwargs)` -> `Self`         | gamut-map into a space via a `method` row |
|  [03]   | `Color.clip`              | `clip(space=None)` -> `Self`                                  | hard-clip coordinates into a target gamut |
|  [04]   | `Color.in_pointer_gamut`  | `in_pointer_gamut(*, tolerance=7.5e-05)` -> `bool`            | test membership in Pointer's gamut        |
|  [05]   | `Color.fit_pointer_gamut` | `fit_pointer_gamut()` -> `Self`                               | map a color into Pointer's gamut          |

The fit `method` axis (`FIT_MAP`):

| [INDEX] | [TIER]          | [METHODS]                                                                                      |
| :-----: | :-------------- | :--------------------------------------------------------------------------------------------- |
|  [01]   | base (6)        | `raytrace` (default), `oklch-chroma`, `lch-chroma`, `minde-chroma`, `scale`, `scale-luminance` |
|  [02]   | `ColorAll` (+2) | `hct-chroma`, `oklch-cubic`                                                                    |

[ENTRYPOINT_SCOPE]: CVD and W3C filter leg
- rail: color

`filter` applies a registered filter by `name`; `amount` is the per-filter strength; `space`/`out_space` control the working and output space. The CVD rows are `protan`, `deutan`, `tritan`; the W3C filter-effect rows are `brightness`, `contrast`, `saturate`, `hue-rotate`, `grayscale`, `sepia`, `invert`, `opacity`. The full `FILTER_MAP` is 11 rows on both `Color` and `ColorAll` (CVD filters ship in the base registration).
- call: `filter(name, amount=None, *, space=None, out_space=None, in_place=False, **kwargs)` -> `Self`

| [INDEX] | [SURFACE]      | [CAPABILITY]                              |
| :-----: | :------------- | :---------------------------------------- |
|  [01]   | `Color.filter` | apply CVD/W3C filter by registered `name` |

[ENTRYPOINT_SCOPE]: distance, contrast, plugins, parse
- rail: color

`distance` is the raw Euclidean row, `delta_e` selects the perceptual metric, `closest` folds `delta_e` over a candidate set; `contrast`/`luminance` are the WCAG21 safety rows; `register`/`deregister` (classmethods) extend the eight engine maps (space/fit/filter/delta-E/CAT/CCT/contrast/interp); `match` (classmethod) parses one color from a string; `within` iterates the colors composited into the current color; `is_nan` tests the powerless sentinel.

| [INDEX] | [SURFACE]          | [CALL_SHAPE]                                                      | [CAPABILITY]                                  |
| :-----: | :----------------- | :---------------------------------------------------------------- | :-------------------------------------------- |
|  [01]   | `Color.distance`   | `distance(color, *, space='lab')` -> `float`                      | Euclidean distance in a space                 |
|  [02]   | `Color.delta_e`    | `delta_e(color, *, method=None, **kwargs)` -> `float`             | perceptual delta-E by registered method       |
|  [03]   | `Color.closest`    | `closest(colors, *, method=None, **kwargs)` -> `Self`             | nearest color from a set by delta-E           |
|  [04]   | `Color.contrast`   | `contrast(color, method=None)` -> `float`                         | contrast ratio (`wcag21`/`lstar`)             |
|  [05]   | `Color.luminance`  | `luminance(*, white=(0.3127, 0.329))` -> `float`                  | relative luminance                            |
|  [06]   | `Color.register`   | `register(plugin, *, overwrite=False, silent=False)` -> `None`    | register an engine-map plugin                 |
|  [07]   | `Color.deregister` | `deregister(plugin, *, silent=False)` -> `None`                   | remove a registered plugin by name            |
|  [08]   | `Color.match`      | `match(string, start=0, fullmatch=False)` -> `ColorMatch \| None` | parse one color out of a string               |
|  [09]   | `Color.within`     | `within(space, *, norm=True, norm_out=None)` -> `Iterator[Self]`  | iterate source colors composited in           |
|  [10]   | `Color.is_nan`     | `is_nan(name)` -> `bool`                                          | test a channel for the powerless/NaN sentinel |

[ENTRYPOINT_SCOPE]: palette builders
- rail: color

`interpolate`/`discrete` build a reusable `Interpolator`; `steps`/`mix`/`weighted_mix` sample it; `harmony`/`average` derive palettes. The builders share the interpolation axis `interpolate` names in full — `space=None, out_space=None, progress=None, hue='shorter', premultiplied=True, extrapolate=False, domain=None, method=None, padding=None, carryforward=None, powerless=None, **kwargs`; `discrete`/`steps` add `steps`/`max_steps=1000`/`max_delta_e=0`/`delta_e`/`delta_e_args`, `mix` adds `percent=0.5`/`in_place`, `weighted_mix`/`average` add `weights`. All but `mix`/`harmony` are classmethods.

| [INDEX] | [SURFACE]            | [CALL_SHAPE]                                           | [CAPABILITY]                              |
| :-----: | :------------------- | :----------------------------------------------------- | :---------------------------------------- |
|  [01]   | `Color.interpolate`  | `interpolate(colors, *, …)` -> `Interpolator`          | build a reusable interpolator over colors |
|  [02]   | `Color.discrete`     | `discrete(colors, *, steps=None, …)` -> `Interpolator` | stepped (non-blended) interpolator        |
|  [03]   | `Color.steps`        | `steps(colors, *, steps=2, …)` -> `list[Self]`         | sample N colors along an interpolation    |
|  [04]   | `Color.mix`          | `mix(color, percent=0.5, *, in_place=False)` -> `Self` | blend two colors                          |
|  [05]   | `Color.weighted_mix` | `weighted_mix(colors, weights=None, *, …)` -> `Self`   | weighted blend of N colors                |
|  [06]   | `Color.harmony`      | `harmony(name, *, …)` -> `list[Self]`                  | derive a harmony palette by wheel name    |
|  [07]   | `Color.average`      | `average(colors, weights=None, *, …)` -> `Self`        | weighted average of colors                |

The delta-E `method` axis (`DE_MAP`):

| [INDEX] | [TIER]          | [METHODS]                                                      |
| :-----: | :-------------- | :------------------------------------------------------------- |
|  [01]   | base (8)        | `76` (default), `2000`, `94`, `cmc`, `hyab`, `itp`, `jz`, `ok` |
|  [02]   | `ColorAll` (+5) | `99o`, `cam02`, `cam16`, `hct`, `helmlab`                      |

The interpolation `method` axis (`INTERPOLATE_MAP`):

| [INDEX] | [TIER]          | [METHODS]                                                                        |
| :-----: | :-------------- | :------------------------------------------------------------------------------- |
|  [01]   | base (6)        | `linear` (default), `continuous`, `bspline`, `natural`, `monotone`, `css-linear` |
|  [02]   | `ColorAll` (+3) | `catrom`, `spectral`, `spectral-continuous`                                      |

The harmony `name` axis (`harmonies.SUPPORTED`, 8 closed wheels): `complement`, `split`, `triad`, `square`, `rectangle`, `analogous`, `mono`, `wheel`.

[ENTRYPOINT_SCOPE]: `Interpolator` curve object
- rail: color

`interpolate`/`discrete` return an `Interpolator` that is itself callable `(point: float) -> Color` and is sampled per gradient stop; the owner builds the curve once and samples it, never re-deriving per sample. `.steps`/`.discretize` re-sample it (sharing `(steps=2, max_steps=1000, max_delta_e=0, ...)`), `.domain` re-anchors the input domain, and `.ease`/`.premultiply`/`.postdivide`/`.padding` are the curve-construction hooks.

| [INDEX] | [SURFACE]                 | [CALL_SHAPE]                                | [CAPABILITY]                                     |
| :-----: | :------------------------ | :------------------------------------------ | :----------------------------------------------- |
|  [01]   | `Interpolator.call`       | `interp(point: float)` -> `Color`           | sample one color at a normalized/domain point    |
|  [02]   | `Interpolator.steps`      | `steps(…)` -> `list[Color]`                 | sample N colors along the built curve            |
|  [03]   | `Interpolator.discretize` | `discretize(…)` -> `Interpolator`           | derive a stepped curve from this curve           |
|  [04]   | `Interpolator.domain`     | `domain(domain: Sequence[float])` -> `None` | re-anchor the input domain of the curve in place |

[ENTRYPOINT_SCOPE]: composition leg
- rail: color

`mask` zeroes/keeps channels for composition; `layer` (classmethod) blend-/Porter-Duff-composites a stack of colors via `layer(colors, *, blend='normal', operator='source-over', space=None, out_space=None)` -> `Self`; `normalize` normalizes powerless/undefined channels; `is_achromatic` is the gray predicate.

| [INDEX] | [SURFACE]             | [CALL_SHAPE]                                               | [CAPABILITY]                                      |
| :-----: | :-------------------- | :--------------------------------------------------------- | :------------------------------------------------ |
|  [01]   | `Color.mask`          | `mask(channel, *, invert=False, in_place=False)` -> `Self` | keep/zero one or several channels for composition |
|  [02]   | `Color.layer`         | `layer(colors, *, …)` -> `Self` (classmethod)              | blend/Porter-Duff composite a color stack         |
|  [03]   | `Color.normalize`     | `normalize(*, nans=True)` -> `Self`                        | normalize powerless/undefined channels in place   |
|  [04]   | `Color.is_achromatic` | `is_achromatic()` -> `bool`                                | test whether the color is achromatic (gray)       |

The `layer` blend axis (`compositing.blend_modes.SUPPORTED`, 16): `normal`, `multiply`, `screen`, `overlay`, `darken`, `lighten`, `color-dodge`, `color-burn`, `hard-light`, `soft-light`, `difference`, `exclusion`, `hue`, `saturation`, `color`, `luminosity`.

The `layer` operator axis (`compositing.porter_duff.SUPPORTED`, 15): `clear`, `copy`, `destination`, `source-over`, `destination-over`, `source-in`, `destination-in`, `source-out`, `destination-out`, `source-atop`, `destination-atop`, `xor`, `lighter`, `plus-lighter`, `plus-darker`.

[ENTRYPOINT_SCOPE]: CCT, chromaticity, and spectral leg
- rail: color

`blackbody` mints a Planckian-locus color at a temperature and `cct` reads its correlated colour temperature + Duv; `chromatic_adaptation` adapts XYZ between white points by CAT `method`; `from_wavelength`/`wavelength` map a single spectral wavelength to/from a color. `blackbody`/`from_wavelength` share the scaling tail `scale=True, scale_space=None, max_saturation=True, clip_negative=False, preserve_luminance=False` (`blackbody` also takes `**kwargs`); `blackbody`/`chromatic_adaptation`/`from_wavelength` are classmethods.

| [INDEX] | [SURFACE]                    | [CALL_SHAPE]                                                     | [CAPABILITY]                      |
| :-----: | :--------------------------- | :--------------------------------------------------------------- | :-------------------------------- |
|  [01]   | `Color.blackbody`            | `blackbody(space, temp, duv=0.0, *, method=None, …)` -> `Self`   | Planckian color at a temperature  |
|  [02]   | `Color.cct`                  | `cct(*, method=None, **kwargs)` -> `Vector`                      | colour temperature + Duv          |
|  [03]   | `Color.chromatic_adaptation` | `chromatic_adaptation(w1, w2, xyz, *, method=None)` -> `Vector`  | adapt XYZ by CAT method           |
|  [04]   | `Color.from_wavelength`      | `from_wavelength(space, wavelength, *, white=None, …)` -> `Self` | color from a spectral wavelength  |
|  [05]   | `Color.wavelength`           | `wavelength(*, white=None, complementary=False)` -> `tuple[...]` | dominant/complementary wavelength |

`wavelength` returns `tuple[float, Vector, Vector]` (the wavelength plus the two spectral-locus points). Every surface below is a `Color` member. `xy`/`uv`/`split_chromaticity` read chromaticity, `chromaticity`/`convert_chromaticity` (classmethods) mint/transform across the chromaticity spaces (`xy-1931`/`uv-1976`/`uv-1960`/`xyz`) via `chromaticity(space, coords, cspace='uv-1976', *, white=None, scale=False, ...)` and `convert_chromaticity(cspace1, cspace2, coords, *, white=None)`, and `white` reads the space white point.

| [INDEX] | [SURFACE]              | [CALL_SHAPE]                                              | [CAPABILITY]                                  |
| :-----: | :--------------------- | :-------------------------------------------------------- | :-------------------------------------------- |
|  [01]   | `xy` / `uv`            | `xy(*, white=None)` / `uv(mode='1976', *, …)` -> `Vector` | CIE 1931 `xy` / 1976 `u'v'` chromaticity      |
|  [02]   | `chromaticity`         | `chromaticity(space, coords, *, …)` -> `Self`             | mint from chromaticity coords                 |
|  [03]   | `split_chromaticity`   | `split_chromaticity(cspace='uv-1976', *, …)` -> `Vector`  | read `[x_or_u, y_or_v, Y]` split chromaticity |
|  [04]   | `convert_chromaticity` | `convert_chromaticity(…)` -> `Vector`                     | transform between chromaticity spaces         |
|  [05]   | `white`                | `white(cspace='xyz')` -> `Vector`                         | white point of the current space              |

The CCT `method` axis (`CCT_MAP`, 2 on both `Color` and `ColorAll`): `ohno-2013`, `robertson-1968`.

The CAT `method` axis (`CAT_MAP`):

| [INDEX] | [TIER]          | [METHODS]                                                                       |
| :-----: | :-------------- | :------------------------------------------------------------------------------ |
|  [01]   | base (1)        | `bradford`                                                                      |
|  [02]   | `ColorAll` (+7) | `cat02`, `cat16`, `cmccat2000`, `cmccat97`, `sharp`, `von-kries`, `xyz-scaling` |

## [04]-[PLUGIN_MAPS]

[MAP_SCOPE]: registered engine maps — the plugin axes `register`/`deregister` extend
- rail: color

Every variation point is a class-level map; `derive#DERIVE` keys its closed vocabularies (`FitMethod`/`ColorFilter`/`Interp`/`Harmony`/`BlendMode`/`PorterDuff`/`CamMethod`/`Blackbody`) onto these literal sets, so the catalog carries the full base-vs-`ColorAll` cardinality each design row reads as a closed key set. A new space/fit/filter/metric/CAT/CCT/interpolation is `Color.register(plugin)` against the matching map, never a local conversion kernel.

| [INDEX] | [MAP]             | [BASE] | [COLORALL] | [AXIS]                                                              |
| :-----: | :---------------- | :----- | :--------- | :------------------------------------------------------------------ |
|  [01]   | `CS_MAP`          | 27     | 77         | color spaces (`convert`/`fit`/`Color(space, …)` keys)               |
|  [02]   | `FIT_MAP`         | 6      | 8          | gamut-fit methods (`fit(method=…)`)                                 |
|  [03]   | `FILTER_MAP`      | 11     | 11         | CVD + W3C filters (`filter(name=…)`)                                |
|  [04]   | `DE_MAP`          | 8      | 13         | delta-E metrics (`delta_e(method=…)`, `closest`)                    |
|  [05]   | `CONTRAST_MAP`    | 1      | 2          | contrast methods (`contrast(method=…)`: `wcag21`, +`lstar`)         |
|  [06]   | `INTERPOLATE_MAP` | 6      | 9          | interpolation methods (`interpolate`/`steps`/`discrete` `method=…`) |
|  [07]   | `CAT_MAP`         | 1      | 8          | chromatic-adaptation transforms (`chromatic_adaptation(method=…)`)  |
|  [08]   | `CCT_MAP`         | 2      | 2          | CCT methods (`blackbody`/`cct` `method=…`)                          |

The base `CS_MAP` 27 spaces: `srgb`, `srgb-linear`, `hsl`, `hsv`, `hwb`, `lab`, `lab-d65`, `lch`, `lch-d65`, `oklab`, `oklch`, `display-p3`, `display-p3-linear`, `a98-rgb`, `a98-rgb-linear`, `prophoto-rgb`, `prophoto-rgb-linear`, `rec2020`, `rec2020-linear`, `rec2100-pq`, `rec2100-hlg`, `rec2100-linear`, `ictcp`, `jzazbz`, `jzczhz`, `xyz-d50`, `xyz-d65`. The `ColorAll` +50 add the appearance/wide-gamut/print spaces (`cmyk`, `cmy`, `hct`, `luv`, `lchuv`, `hsluv`, `hpluv`, `okhsl`, `okhsv`, `oklrab`, `oklrch`, `din99o`, `lch99o`, `cam16-*`, `cam02-*`, `zcam-jmh`, `hellwig-*`, `aces2065-1`, `acescc`, `acescct`, `acescg`, `rec709`, `xyy`, `ipt`, `igpgtg`, `ryb`, `cubehelix`, `prismatic`, `orgb`, `xyb`, and others) — the `ColorAll` engine is the one the owner uses precisely because OKLCH/HCT/CMYK/CAM resolve without a per-space class.

## [05]-[IMPLEMENTATION_LAW]

[COLOR_ENGINE]:
- import: `from coloraide.everything import ColorAll as Color` at boundary scope only — `derive#DERIVE` aliases `ColorAll` to `Color` so OKLCH/HCT/CMYK/CAM and the registered fit/filter/harmony/blend/average/closest/blackbody/spectral-interp plugins resolve without a per-space class; the base `from coloraide import Color` (27 spaces) is used only where the all-plugins surface is provably unneeded. Module-level import is banned by the manifest import policy.
- object axis: one `Color`/`ColorAll` owns parse, convert, fit, filter, distance, interpolation, composition, CCT, and chromaticity; arity and modality live in method arguments (`space`, `method`, `name`, `hue`, `**kwargs`), never a per-operation builder type; `new`/`clone`/`mutate`/`update`/`random` mint or rewrite without a parallel constructor family; `get`/`set` discriminate one channel name vs a list/dict, never `get_channel`/`set_channels` siblings.
- gamut-map axis: `fit` is the single gamut-mapping surface keyed by `method` (`FIT_MAP` rows, `raytrace` default); `in_gamut` is the predicate row, `clip` the hard-clip row, `in_pointer_gamut`/`fit_pointer_gamut` the real-surface-gamut pair; conversion-time fitting flows through `convert(space, fit=...)` — never parallel mapping functions.
- CVD axis: `filter` is the single filter surface keyed by `name` (`FILTER_MAP`); `protan`/`deutan`/`tritan` are the CVD rows and the eight W3C effects are sibling rows under the same `amount`-parameterized surface; CVD simulation is a `name` row, never a hand-rolled Daltonization transform.
- distance axis: `delta_e` selects the perceptual metric row (`DE_MAP`), `distance` is the raw Euclidean row, `closest` folds `delta_e` over a candidate set; the OKLab-perceptual rows (`ok`/`jz`/`hyab`/`itp`/`99o`/`cam02`/`cam16`/`hct`) are the difference family `colour-science` has no Lab-array form for, so they live on this engine.
- interpolation axis: `interpolate`/`discrete` build one reusable `Interpolator` keyed by `method` (`INTERPOLATE_MAP`, including the `spectral`/`spectral-continuous` mixbox-style pigment-mixing curves); `steps`/`mix`/`weighted_mix` sample it; `hue` strategy (`shorter`/`longer`/`increasing`/`decreasing`/`specified`), `premultiplied` alpha, `domain`, `padding`, and `progress` easing (`stop`/`hint`/`cubic_bezier`/`linear`/`ease*`) are arguments, never parallel gradient functions. The owner builds the interpolator once and samples per gradient stop via `Interpolator(point)`/`.steps(...)`, never re-deriving the curve per sample.
- composition axis: `layer` is the single compositing surface keyed by `blend` (`blend_modes.SUPPORTED`, 16) + `operator` (`porter_duff.SUPPORTED`, 15); `mask` keeps/zeros channels feeding the layer stack; the `weights`-bearing blend is `weighted_mix` on the same engine — never a parallel blend-vs-composite surface.
- CCT axis: `blackbody` mints a Planckian-locus color and `cct` reads temperature+Duv keyed by `method` (`CCT_MAP`: `ohno-2013`/`robertson-1968` — no `ColorAll` additions); `chromatic_adaptation` adapts XYZ between white points keyed by CAT `method` (`CAT_MAP`); CCT is a `method` row, never a hand-rolled Planckian fit.
- chromaticity axis: `xy`/`uv`/`split_chromaticity` read, `chromaticity`/`convert_chromaticity` mint/transform across the chromaticity spaces (`xy-1931`/`uv-1976`/`uv-1960`/`xyz`); `wavelength`/`from_wavelength` map dominant/complementary wavelength and the spectral locus.
- plugin axis: `register`/`deregister` extend the eight engine maps (`CS_MAP`, `FIT_MAP`, `FILTER_MAP`, `DE_MAP`, `CAT_MAP`, `CONTRAST_MAP`, `INTERPOLATE_MAP`, `CCT_MAP`); the base `Color` registers 27 spaces, `everything.ColorAll` registers 77; new spaces, fit methods, filters, metrics, CATs, CCT methods, and interpolation curves are registered plugins on the class, never local conversion kernels.
- evidence: each color captures the resolved space (`space()`), channel vector (`coords`), alpha, in-gamut flag (`in_gamut`), applied fit method, applied filter name + amount, delta-E metric, blend/operator, and (when read) CCT/Duv + chromaticity as a color receipt — the `derive#DERIVE` `ColorReceipt` `measures` `frozendict[Metric, float]` evidence map and `notation` `to_string` CSS form.
- boundary: coloraide owns color parsing, conversion, gamut mapping, CVD+W3C filtering, distance, contrast, interpolation, composition, CCT, and chromaticity with no native dependency; numeric coordinate vectors (`coords`/`get`/`split_chromaticity`) and serialized `to_string` feed the figures/visuals/document owners directly; rasterization, plotting, and the ICC/LUT pixel transform stay outside this package.

[INTEGRATION_STACK]:
- `coloraide` <-> `colour-science` (the two-engine `Colorimetry` split in `derive#DERIVE`): `colour` owns spectral/colorimetric truth (SPD -> XYZ via `sd_to_XYZ`, CIECAM02/CAM16 appearance models, reference CCT, named CIE RGB primaries, ICC-grade adaptation, the CIE/CMC/DIN99 Lab-array `delta_E`, the `whiteness`/`yellowness`/`cri`/`cfi`/`dominant_wavelength` colorimetric indices); `coloraide` owns the disjoint per-color legs — `fit` gamut mapping, `filter` CVD+W3C, `steps`/`discrete`/`interpolate` palettes, `layer`/`weighted_mix` compositing, `harmony`/`closest`/`average`, `blackbody`, the OKLab-perceptual `delta_e` family, and WCAG21 `contrast`/`luminance`/`distance`. The seam is the numeric XYZ/chromaticity vector: a measured XYZ from `colour.sd_to_XYZ` enters as `Color(('xyz-d65', coords))` (or `('lab', …)` per the CxF illuminant) for gamut-mapped CSS output; `coloraide` is never used for spectral integration and `colour` is never used for CSS string parsing. The `ColorModel` `ModelNames(science, aide)` dual-name vocabulary collapses the two engine spellings into one row so one model value serves both — never two parallel model enums.
- `coloraide` <- `colour-cxf` (CxF spot intake): a CxF `ColorCielab`/`ColorCiexyz` decoded from a print partner's `.cxf` (via the `colour-cxf` `cxf3` graph + the `colour-science` XYZ bridge) enters `coloraide` as `Color(('lab', [l, a, b]))` / `Color(('xyz-d65', [x, y, z]))` for gamut-mapped CSS egress of an exchanged spot; `coloraide` never parses CxF XML.
- `coloraide` -> `graphic/color/managed#MANAGED` (egress provenance, not pixel transform): the resolved color-space id (`space()`), the gamut-fit method, and the tone-curve provenance the gamut/CSS leg records feed the `managed` ICC/LUT raster-egress leg as the color-space + CCTF provenance it consumes; the device-link/named-color/N-channel/ICC pixel transform is Pillow `ImageCms` + `colour-science` on the `managed` side, never inside `coloraide`.
- `coloraide` -> figures/visuals/document owners: the channel `Vector` (`coords`/`get`/`split_chromaticity`/`Y`), the `Interpolator` sampled per stop, and the serialized `to_string` CSS strings feed `visualization/chart#CHART`, `scene#SCENE`, `visualization/table#TABLE`, `graphic/marks/encode#MARK`, and the document output directly as numeric data + CSS, never via a render call inside this package.
- universal-rail stacking (`libs/python/.api`, layered ON TOP of this folder catalog): a resolved `Color` projects onto a `msgspec.Struct` color-wire model — the `ColorReceipt.wired()` `ColorReceiptWire` projection sinks ravelled `coords` + `shape` + `notation` outward at the `data/tabular#WIRE` seam, never an `NDArray`-on-the-wire owner or an erased `object`; the scalar-bounded payloads (`Amount` in `[0,1]`, `PaletteCount` >= 1, `Spacing` >= 0, `Kelvin`, `Wavelength`) refine through a `beartype.vale.Is`-guarded `Annotated` contract at the `@beartype`-guarded `derive#DERIVE` factory so an out-of-range amount/count/spacing/kelvin/wavelength is refused at construction; the `measures` evidence map is a `frozendict[Metric, float]` `expression`-folded onto `RuntimeRail[ColorReceipt]` so a fit/filter/measure raise crosses the `runtime/reliability/faults` `async_boundary` seam onto the `RuntimeRail`, never an unhandled coloraide exception; a `structlog` event + OpenTelemetry span records the convert/fit/palette at the color boundary; a batch palette derivation over many seeds runs through the `anyio` structured-concurrency lane under a `CapacityLimiter`, each `Color` resolution a `to_thread` slot since coloraide is pure-Python CPU-bound; the channel vectors `coords` returns are plain `list[float]` the `numpy` rail ravels into the receipt array.

[RAIL_LAW]:
- Package: `coloraide`
- Owns: CSS/named/coordinate color parsing; conversion across 27 base (77 in `ColorAll`) registered color spaces; gamut mapping through `FIT_MAP` (6 base / 8 all); CVD + W3C filtering over the 11-row `FILTER_MAP`; delta-E distance (`DE_MAP`, 8 base / 13 all) + raw Euclidean `distance`; WCAG21/L\* contrast + luminance; six-base/nine-all interpolation (including `spectral`/`spectral-continuous` pigment mixing) with the reusable `Interpolator`; mixing/harmonies (8 wheels)/averaging; masking + 16-blend/15-Porter-Duff `layer` composition; CCT/blackbody (`ohno-2013`/`robertson-1968`) and CAT (`CAT_MAP`, 1 base / 8 all); chromaticity (`xy`/`uv`/`chromaticity`/`split_chromaticity`/`convert_chromaticity`) and spectral `wavelength`/`from_wavelength`; Pointer's-gamut tests; and a plugin `register`/`deregister` surface over the eight engine maps. The per-color CSS/gamut/CVD/palette/compositing presentation engine for the color plane.
- Accept: `everything.ColorAll` as the working engine; gamut-map (`fit`/`in_gamut`/`clip`), CVD+W3C (`filter`), palette (`steps`/`discrete`/`interpolate`/`mix`/`weighted_mix`/`harmony`/`average`/`closest`), compositing (`layer`/`mask`), difference (`delta_e`/`distance`/`contrast`/`luminance`), CCT (`blackbody`/`cct`), chromaticity, and spectral legs feeding `derive#DERIVE`'s `ColorOp` family onto `ColorReceipt`; XYZ/Lab vectors handed off from `colour-science` (and via it from `colour-cxf`) for CSS/gamut presentation; the closed map cardinalities as the `FitMethod`/`ColorFilter`/`Interp`/`Harmony`/`BlendMode`/`PorterDuff`/`CamMethod`/`Blackbody` key sets.
- Reject: wrapper-renames of `fit`/`filter`/`convert`/`layer`/`interpolate`; a hand-rolled gamut-mapping, Daltonization, Planckian-CCT, or sRGB/OKLCH/Lab conversion matrix where a registered method/filter/space exists; a parallel color type per space (use `ColorAll`); a `get_channel`/`set_channels` family where `get`/`set` discriminate; re-deriving an interpolation curve per sample rather than building one `Interpolator`; using `coloraide` for spectral sd->XYZ integration, the CIE/CMC/DIN99 Lab-array `delta_E`, or the colorimetric-index family (those are `colour-science`); parsing CxF/ICC files or running the ICC/LUT pixel transform (those are `colour-cxf`/`managed`); identity minting the runtime owns; a `python_version` marker (pure-Python, runs on cp315).
