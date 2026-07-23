# [PY_ARTIFACTS_API_COLORAIDE]

`coloraide` mints the pure-Python per-color engine feeding the `graphic/color/derive#DERIVE` palette source and `graphic/color/managed#MANAGED` egress provenance: one `Color` object parses CSS/named/coordinate input, converts across registered spaces, and folds gamut, filtering, difference, interpolation, compositing, CCT, and chromaticity. `everything.ColorAll` is the all-plugins engine the closed `ColorOp` family folds onto the closed `Derivation` family. It owns the per-color legs `colour-science` has no Lab-array form for; spectral integration and the ICC/LUT pixel transform stay outside.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `coloraide`
- package: `coloraide` (MIT)
- module: `coloraide`
- rail: color
- target: pure-Python, zero native extension, ABI-agnostic; stdlib-only runtime with no NumPy at import (the bundled `algebra` module carries the linear-algebra kernels)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: color object, match record, top-level exports

`Color` is the single engine object: it parses on construction and every mutating transform returns `Self` for chaining. `everything.ColorAll` subclasses `Color` with every space/fit/filter/delta-E/CAT/CCT/interpolation plugin preloaded — the working engine the owner imports. `stop`/`hint`/`cubic_bezier`/`linear` and the `ease*` callables pass into `interpolate`/`steps`/`discrete` as easing; `NaN` is the powerless/undefined-hue channel sentinel.

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY]      | [CAPABILITY]                                          |
| :-----: | :---------------------------------------------- | :----------------- | :---------------------------------------------------- |
|  [01]   | `Color`                                         | color engine       | base parse/convert/fit/filter/distance object         |
|  [02]   | `everything.ColorAll`                           | color engine       | `Color` subclass, every plugin preloaded              |
|  [03]   | `ColorMatch`                                    | match record       | `color`/`start`/`end` parse hit from `Color.match`    |
|  [04]   | `interpolate.Interpolator`                      | gradient curve     | reusable callable curve from `interpolate`/`discrete` |
|  [05]   | `stop`                                          | interpolation stop | `stop(color, value)` positioned color stop            |
|  [06]   | `hint`                                          | interpolation hint | `hint(mid)` midpoint easing between stops             |
|  [07]   | `cubic_bezier`                                  | easing factory     | `cubic_bezier(x1, y1, x2, y2)` easing callable        |
|  [08]   | `linear`                                        | easing function    | `linear(t)` identity progress easing                  |
|  [09]   | `ease` / `ease_in` / `ease_out` / `ease_in_out` | easing function    | CSS-named easing progress (tunable knots)             |
|  [10]   | `NaN`                                           | sentinel           | `float('nan')` channel sentinel for a powerless hue   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construct, convert, channel read/write, serialize

Construction parses any `ColorInput` (CSS string, named color, `(space, coords)` shape, a `to_dict` mapping, or another `Color`). `Color`/`new`/`mutate` share `(color, data=None, alpha=1.0, **kwargs)`; the reader family `get`/`coords`/`alpha`/`Y`/`to_dict` shares the `*, nans=True, precision=None, rounding=None` tail. `to_string` selects `fit`/`precision`/`color`/`comma`/`percent`/`hex`/`names`/`upper` through kwargs per the space's CSS serializer.

| [INDEX] | [SURFACE]                                                               | [SHAPE]  | [CAPABILITY]                                      |
| :-----: | :---------------------------------------------------------------------- | :------- | :------------------------------------------------ |
|  [01]   | `Color(color, data=None, alpha=1.0, **kwargs)`                          | ctor     | parse input into its detected space               |
|  [02]   | `Color.new(color, …) -> Self`                                           | factory  | mint a sibling color of the same class            |
|  [03]   | `Color.clone() -> Self`                                                 | instance | duplicate without re-parsing                      |
|  [04]   | `Color.mutate(color, …) -> Self`                                        | instance | rewrite in place from new input                   |
|  [05]   | `Color.update(color, …, *, norm=True) -> Self`                          | instance | overwrite coords from new input, keep space       |
|  [06]   | `Color.random(space, *, limits=None) -> Self`                           | factory  | mint a random color in a space                    |
|  [07]   | `Color.convert(space, *, fit=False, in_place=False, norm=True) -> Self` | instance | convert to a registered space, optional fit       |
|  [08]   | `Color.space() -> str`                                                  | instance | read the current color-space id                   |
|  [09]   | `Color.get(name) -> float \| Vector`                                    | instance | read one channel (str) or several (`list`)        |
|  [10]   | `Color.set(name, value=None, *, nans=True) -> Self`                     | instance | write one channel or many (str→value dict)        |
|  [11]   | `Color.coords() -> Vector`                                              | instance | read the channel vector (no alpha)                |
|  [12]   | `Color.alpha() -> float`                                                | instance | read the alpha channel                            |
|  [13]   | `Color.Y() -> float`                                                    | instance | read the luminance (Y) channel                    |
|  [14]   | `Color.to_string(**kwargs) -> str`                                      | instance | serialize to a CSS/space string                   |
|  [15]   | `Color.serialize(*, fit=False, **kwargs) -> str`                        | instance | string egress with optional in-serialize fit      |
|  [16]   | `Color.to_dict() -> Mapping`                                            | instance | structured mapping, round-trips via `Color(dict)` |

[ENTRYPOINT_SCOPE]: gamut-map and CVD/W3C filter legs

`fit` is the single gamut-mapping surface keyed by `method` (`FIT_MAP`, `raytrace` default); `in_gamut` the predicate, `clip` the hard-clip row, `in_pointer_gamut`/`fit_pointer_gamut` the real-surface-gamut pair; conversion-time fitting flows through `convert(space, fit=...)`. `filter` applies one registered CVD or W3C effect by `name` (`FILTER_MAP`), `amount` its strength, `space`/`out_space`/`in_place` the working/output space and in-place write.

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :---------------------------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `Color.in_gamut(space=None, *, tolerance=None, **kwargs) -> bool` | instance | test membership in a target gamut         |
|  [02]   | `Color.fit(space=None, *, method=None, **kwargs) -> Self`         | instance | gamut-map into a space via a `method` row |
|  [03]   | `Color.clip(space=None) -> Self`                                  | instance | hard-clip coordinates into a target gamut |
|  [04]   | `Color.in_pointer_gamut(*, tolerance=7.5e-05) -> bool`            | instance | test membership in Pointer's gamut        |
|  [05]   | `Color.fit_pointer_gamut() -> Self`                               | instance | map a color into Pointer's gamut          |
|  [06]   | `Color.filter(name, amount=None, **kwargs) -> Self`               | instance | apply a CVD/W3C filter by `name`          |

[ENTRYPOINT_SCOPE]: distance, contrast, plugins, parse

`delta_e` selects the perceptual metric (`DE_MAP`), `distance` the raw Euclidean row, `closest` folds `delta_e` over a candidate set; `contrast`/`luminance` are the WCAG21 safety rows; `register`/`deregister` extend the eight engine maps; `match` parses one color from a string; `within` iterates the composited sources; `is_nan` tests the powerless sentinel.

| [INDEX] | [SURFACE]                                                             | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :-------------------------------------------------------------------- | :------- | :---------------------------------- |
|  [01]   | `Color.distance(color, *, space='lab') -> float`                      | instance | Euclidean distance in a space       |
|  [02]   | `Color.delta_e(color, *, method=None, **kwargs) -> float`             | instance | perceptual delta-E by `method`      |
|  [03]   | `Color.closest(colors, *, method=None, **kwargs) -> Self`             | fold     | nearest color from a set by delta-E |
|  [04]   | `Color.contrast(color, method=None) -> float`                         | instance | contrast ratio (`CONTRAST_MAP`)     |
|  [05]   | `Color.luminance(*, white=(0.3127, 0.329)) -> float`                  | instance | relative luminance                  |
|  [06]   | `Color.register(plugin, *, overwrite=False, silent=False) -> None`    | static   | register an engine-map plugin       |
|  [07]   | `Color.deregister(plugin, *, silent=False) -> None`                   | static   | remove a registered plugin by name  |
|  [08]   | `Color.match(string, start=0, fullmatch=False) -> ColorMatch \| None` | static   | parse one color out of a string     |
|  [09]   | `Color.within(space, *, norm=True, norm_out=None) -> Iterator[Self]`  | instance | iterate source colors composited in |
|  [10]   | `Color.is_nan(name) -> bool`                                          | instance | test a channel for the NaN sentinel |

[ENTRYPOINT_SCOPE]: palette builders

`interpolate`/`discrete` build a reusable `Interpolator` keyed by `method` (`INTERPOLATE_MAP`); `steps`/`mix`/`weighted_mix` sample it; `harmony`/`average` derive palettes. All builders share the interpolation axis `space, out_space, progress, hue='shorter', premultiplied=True, extrapolate=False, domain, method, padding, carryforward, powerless, **kwargs`; `discrete`/`steps` add `steps, max_steps=1000, max_delta_e=0, delta_e, delta_e_args`, `mix` adds `percent=0.5, in_place`, `weighted_mix`/`average` add `weights`.

| [INDEX] | [SURFACE]                                                  | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :--------------------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `Color.interpolate(colors, *, …) -> Interpolator`          | factory  | build a reusable interpolator over colors |
|  [02]   | `Color.discrete(colors, *, steps=None, …) -> Interpolator` | factory  | stepped (non-blended) interpolator        |
|  [03]   | `Color.steps(colors, *, steps=2, …) -> list[Self]`         | factory  | sample N colors along an interpolation    |
|  [04]   | `Color.mix(color, percent=0.5, *, in_place=False) -> Self` | instance | blend two colors                          |
|  [05]   | `Color.weighted_mix(colors, weights=None, *, …) -> Self`   | fold     | weighted blend of N colors                |
|  [06]   | `Color.harmony(name, *, …) -> list[Self]`                  | instance | derive a harmony palette by wheel name    |
|  [07]   | `Color.average(colors, weights=None, *, …) -> Self`        | fold     | weighted average of colors                |

[ENTRYPOINT_SCOPE]: `Interpolator` curve object

`interpolate`/`discrete` return an `Interpolator` that is itself callable `(point) -> Color`; the owner builds the curve once and samples per gradient stop, never re-deriving per sample. `.steps`/`.discretize` re-sample it (sharing `steps=2, max_steps=1000, max_delta_e=0, ...`), `.domain` re-anchors the input domain, and `.ease`/`.premultiply`/`.postdivide`/`.padding` are the curve-construction hooks.

| [INDEX] | [SURFACE]                                    | [SHAPE]  | [CAPABILITY]                                  |
| :-----: | :------------------------------------------- | :------- | :-------------------------------------------- |
|  [01]   | `Interpolator(point) -> Color`               | operator | sample one color at a normalized/domain point |
|  [02]   | `Interpolator.steps(…) -> list[Color]`       | instance | sample N colors along the built curve         |
|  [03]   | `Interpolator.discretize(…) -> Interpolator` | instance | derive a stepped curve from this curve        |
|  [04]   | `Interpolator.domain(domain) -> None`        | instance | re-anchor the input domain in place           |

[ENTRYPOINT_SCOPE]: compositing

`layer` is the single compositing surface keyed by `blend` (`blend_modes.SUPPORTED`, `normal` default) + `operator` (`porter_duff.SUPPORTED`, `source-over` default), `space`/`out_space` the working/output space; `mask` keeps/zeros channels feeding the stack; `normalize` normalizes powerless channels; `is_achromatic` is the gray predicate.

| [INDEX] | [SURFACE]                                                           | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :------------------------------------------------------------------ | :------- | :------------------------------------ |
|  [01]   | `Color.mask(channel, *, invert=False, in_place=False) -> Self`      | instance | keep/zero channels for composition    |
|  [02]   | `Color.layer(colors, *, blend, operator, space, out_space) -> Self` | factory  | blend/Porter-Duff composite a stack   |
|  [03]   | `Color.normalize(*, nans=True) -> Self`                             | instance | normalize powerless channels in place |
|  [04]   | `Color.is_achromatic() -> bool`                                     | instance | test whether the color is achromatic  |

[ENTRYPOINT_SCOPE]: CCT, chromaticity, and spectral legs

`blackbody` mints a Planckian-locus color and `cct` reads its temperature + Duv keyed by `method` (`CCT_MAP`); `chromatic_adaptation` adapts XYZ between white points by CAT `method` (`CAT_MAP`); `from_wavelength`/`wavelength` map a single spectral wavelength to/from a color. `blackbody`/`from_wavelength` share the scaling tail `scale=True, scale_space=None, max_saturation=True, clip_negative=False, preserve_luminance=False`. `wavelength` returns `(wavelength, locus_a, locus_b)`; `chromaticity`/`convert_chromaticity` cross the chromaticity spaces `xy-1931`/`uv-1976`/`uv-1960`/`xyz`.

| [INDEX] | [SURFACE]                                                                       | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :------------------------------------------------------------------------------ | :------- | :---------------------------------------- |
|  [01]   | `Color.blackbody(space, temp, duv=0.0, *, method=None, …) -> Self`              | factory  | Planckian color at a temperature          |
|  [02]   | `Color.cct(*, method=None, **kwargs) -> Vector`                                 | instance | colour temperature + Duv                  |
|  [03]   | `Color.chromatic_adaptation(w1, w2, xyz, *, method=None) -> Vector`             | static   | adapt XYZ by CAT method                   |
|  [04]   | `Color.from_wavelength(space, wavelength, *, white=None, …) -> Self`            | factory  | color from a spectral wavelength          |
|  [05]   | `Color.wavelength(*, white=None, complementary=False) -> tuple`                 | instance | dominant/complementary wavelength + locus |
|  [06]   | `Color.xy(*, white=None) -> Vector`                                             | instance | CIE 1931 `xy` chromaticity                |
|  [07]   | `Color.uv(mode='1976', *, white=None) -> Vector`                                | instance | CIE 1976 `u'v'` chromaticity              |
|  [08]   | `Color.chromaticity(space, coords, cspace='uv-1976', *, …) -> Self`             | factory  | mint from chromaticity coords             |
|  [09]   | `Color.split_chromaticity(cspace='uv-1976', *, white=None) -> Vector`           | instance | read `[x\|u, y\|v, Y]` split chromaticity |
|  [10]   | `Color.convert_chromaticity(cspace1, cspace2, coords, *, white=None) -> Vector` | static   | transform between chromaticity spaces     |
|  [11]   | `Color.white(cspace='xyz') -> Vector`                                           | instance | white point of the current space          |

## [04]-[PLUGIN_MAPS]

[MAP_SCOPE]: the eight class-level engine maps `register`/`deregister` extend

Every variation point is a class-level map, so a new space/fit/filter/metric/CAT/CCT/interpolation is `Color.register(plugin)` against the matching map, never a local kernel. `derive#DERIVE` keys its closed vocabularies (`FitMethod`/`ColorFilter`/`Interp`/`Harmony`/`BlendMode`/`PorterDuff`/`CamMethod`/`Blackbody`) onto these literal sets; the base `Color` registers the `[BASE]` rows and `everything.ColorAll` the `[COLORALL]` superset — OKLCH/HCT/CMYK/CAM resolve on `ColorAll` without a per-space class.

| [INDEX] | [MAP]             | [BASE] | [COLORALL] | [AXIS]                                                             |
| :-----: | :---------------- | -----: | ---------: | :----------------------------------------------------------------- |
|  [01]   | `CS_MAP`          |     27 |         77 | color spaces (`convert`/`fit`/`Color(space, …)` keys)              |
|  [02]   | `FIT_MAP`         |      6 |          8 | gamut-fit methods (`fit(method=…)`)                                |
|  [03]   | `FILTER_MAP`      |     11 |         11 | CVD + W3C filters (`filter(name=…)`)                               |
|  [04]   | `DE_MAP`          |      8 |         13 | delta-E metrics (`delta_e(method=…)`, `closest`)                   |
|  [05]   | `CONTRAST_MAP`    |      1 |          2 | contrast methods (`contrast(method=…)`)                            |
|  [06]   | `INTERPOLATE_MAP` |      6 |          9 | interpolation methods (`interpolate`/`steps`/`discrete`)           |
|  [07]   | `CAT_MAP`         |      1 |          8 | chromatic-adaptation transforms (`chromatic_adaptation(method=…)`) |
|  [08]   | `CCT_MAP`         |      2 |          2 | CCT methods (`blackbody`/`cct` `method=…`)                         |

Method rosters, `base` then the `ColorAll` additions:
- `FIT_MAP` base: `raytrace` `oklch-chroma` `lch-chroma` `minde-chroma` `scale` `scale-luminance`; `ColorAll` adds `hct-chroma` `oklch-cubic`.
- `DE_MAP` base: `76` `2000` `94` `cmc` `hyab` `itp` `jz` `ok`; `ColorAll` adds `99o` `cam02` `cam16` `hct` `helmlab`.
- `INTERPOLATE_MAP` base: `linear` `continuous` `bspline` `natural` `monotone` `css-linear`; `ColorAll` adds `catrom` `spectral` `spectral-continuous`.
- `CAT_MAP` base: `bradford`; `ColorAll` adds `cat02` `cat16` `cmccat2000` `cmccat97` `sharp` `von-kries` `xyz-scaling`.
- `CCT_MAP`: `ohno-2013` `robertson-1968`.
- `CONTRAST_MAP`: `wcag21`; `ColorAll` adds `lstar`.
- `FILTER_MAP`: CVD `protan` `deutan` `tritan`; W3C `brightness` `contrast` `saturate` `hue-rotate` `grayscale` `sepia` `invert` `opacity`.
- `harmonies.SUPPORTED`: `complement` `split` `triad` `square` `rectangle` `analogous` `mono` `wheel`.
- `blend_modes.SUPPORTED`: `normal` `multiply` `screen` `overlay` `darken` `lighten` `color-dodge` `color-burn` `hard-light` `soft-light` `difference` `exclusion` `hue` `saturation` `color` `luminosity`.
- `porter_duff.SUPPORTED`: `clear` `copy` `destination` `source-over` `destination-over` `source-in` `destination-in` `source-out` `destination-out` `source-atop` `destination-atop` `xor` `lighter` `plus-lighter` `plus-darker`.
- `CS_MAP` base: `srgb` `srgb-linear` `hsl` `hsv` `hwb` `lab` `lab-d65` `lch` `lch-d65` `oklab` `oklch` `display-p3` `display-p3-linear` `a98-rgb` `a98-rgb-linear` `prophoto-rgb` `prophoto-rgb-linear` `rec2020` `rec2020-linear` `rec2100-pq` `rec2100-hlg` `rec2100-linear` `ictcp` `jzazbz` `jzczhz` `xyz-d50` `xyz-d65`.
- `CS_MAP` `ColorAll` adds the appearance/wide-gamut/print spaces `cmyk` `cmy` `hct` `luv` `lchuv` `hsluv` `hpluv` `okhsl` `okhsv` `oklrab` `oklrch` `din99o` `lch99o` `cam16-*` `cam02-*` `zcam-jmh` `hellwig-*` `aces2065-1` `acescc` `acescct` `acescg` `rec709` `xyy` `ipt` `igpgtg` `ryb` `cubehelix` `prismatic` `orgb` `xyb` and others.

## [05]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One `Color`/`ColorAll` object owns parse, convert, fit, filter, distance, interpolation, composition, CCT, and chromaticity; arity and modality ride method arguments (`space`, `method`, `name`, `hue`, `**kwargs`), never a per-operation builder type. `new`/`clone`/`mutate`/`update`/`random` mint or rewrite without a parallel constructor family; `get`/`set` discriminate one channel name against a `list`/`dict`.
- Every variation point is a class-level map row extended via `register`/`deregister` over the eight engine maps; `method=`/`name=` selects the row, never a parallel per-method function or a local conversion kernel.
- `interpolate`/`discrete` build one reusable `Interpolator` sampled per gradient stop via `Interpolator(point)`/`.steps`, never a curve re-derived per sample.
- Each `Derivation` case captures only its operation's evidence — resolved space, channel coordinates, CSS notation, path, a `Metric`-keyed `frozendict[Metric, float]`, or the `GamutFit` witness; `Derivation.coords` projects the coordinate array common to every case.

[STACKING]:
- `colour-science`(`.api/colour-science.md`): a measured XYZ/Lab from `colour.sd_to_XYZ` or an appearance-model coordinate enters as `Color(('xyz-d65', coords))` / `Color(('lab', …))` for gamut-mapped CSS egress; `colour` owns spectral/colorimetric truth per its `[RAIL_LAW]`, `coloraide` the per-color presentation. `ColorModel`'s `ModelNames(science, aide)` dual-name collapses the two engine spellings into one row.
- `colour-cxf`(`.api/colour-cxf.md`): a CxF `ColorCielab`/`ColorCiexyz` decoded from a `.cxf` enters as `Color(('lab', …))` / `Color(('xyz-d65', …))` for gamut-mapped spot egress; `coloraide` never parses CxF XML.
- `graphic/color/managed#MANAGED`: `space()` id and the `GamutFit` method recorded on `Derivation` feed the ICC/LUT raster-egress leg as color-space provenance; the device ICC pixel transform is pyvips `icc_transform` on the `managed` side, never here.
- figures/visuals/document owners: the channel `Vector` (`coords`/`get`/`split_chromaticity`/`Y`), the sampled `Interpolator`, and `to_string` CSS feed `visualization/chart/spec#CHART`, `scene/render#SCENE`, `visualization/table#TABLE`, `graphic/marks/encode#MARK`, and document output as numeric data + CSS, never a render call here.
- within-lib: a resolved `Color` projects into one `Derivation` case, threaded through `expression`'s `RuntimeRail[Derivation]` with `beartype.vale.Is`-guarded `Annotated` bounds on scalar/vector/rank/finiteness and `Result[ColorOp, DeriveFault]` field factories; `LanePolicy.offload` supplies the interpreter-local CPU crossing and `numpy` materializes `coords`.

[LOCAL_ADMISSION]:
- Color logic enters through `everything.ColorAll` aliased to `Color` at boundary scope; the base `from coloraide import Color` enters only where the all-plugins surface is provably unneeded, and module-level import is banned by the manifest import policy.
- A new space/fit/filter/metric/CAT/CCT/interpolation enters as a registered plugin on the matching engine map, never a local conversion kernel.

[RAIL_LAW]:
- Package: `coloraide`
- Owns: CSS/named/coordinate color parsing; conversion across the registered color spaces; gamut mapping (`FIT_MAP`) and Pointer's-gamut tests; CVD + W3C filtering (`FILTER_MAP`); delta-E and raw Euclidean distance; WCAG21/L\* contrast and luminance; the reusable-`Interpolator` interpolation family including `spectral` pigment mixing; mixing, harmonies, and averaging; masking and blend/Porter-Duff `layer` compositing; CCT/blackbody and CAT adaptation; chromaticity and spectral `wavelength`/`from_wavelength`; and a plugin `register`/`deregister` surface over the eight engine maps — the per-color CSS/gamut/CVD/palette/compositing presentation engine for the color plane.
- Accept: `everything.ColorAll` as the working engine; the gamut-map, CVD+W3C, palette, compositing, difference, CCT, chromaticity, and spectral legs feeding `derive#DERIVE`'s `ColorOp` and `Derivation` families; XYZ/Lab vectors handed off from `colour-science` and `colour-cxf` for CSS and gamut presentation; the map cardinalities as the `FitMethod`/`ColorFilter`/`Interp`/`Harmony`/`BlendMode`/`PorterDuff`/`CamMethod`/`Blackbody` key sets.
- Reject: wrapper-renames of `fit`/`filter`/`convert`/`layer`/`interpolate`; a hand-rolled gamut-map, Daltonization, Planckian-CCT, or sRGB/OKLCH/Lab conversion matrix where a registered method/filter/space exists; a per-space color type where `ColorAll` serves; a `get_channel`/`set_channels` family where `get`/`set` discriminate; a per-sample interpolation curve where one `Interpolator` serves; `coloraide` for spectral sd→XYZ integration, the Lab-array `delta_E`, or the colorimetric-index family (those are `colour-science`); parsing CxF/ICC files or running the ICC/LUT pixel transform (those are `colour-cxf`/`managed`); identity minting the runtime owns; a `python_version` marker or version pin.
