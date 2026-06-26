# [PY_ARTIFACTS_GRAPHIC_COLOR_DERIVE]

The upstream color-derivation owner feeding consistent, perceptually-correct color into every visual sub-domain. `Colorimetry` is ONE owner over a two-engine dispatch on the cp315 core. colour-science (NumFOCUS-affiliated, BSD-3, NumPy-backed, pure-Python, host-free) carries the colorimetric truth — the universal `colour.convert` model-pair gateway over 30+ spaces, the CIECAM02 / CAM16 appearance models, `colour.wavelength_to_XYZ` spectral-locus intake, the `colour.delta_E` CIE/CMC/DIN99 difference family, the `colour.chromatic_adaptation` cross-illuminant white-point transform over an `Observer`-keyed `colour.CCS_ILLUMINANTS` whitepoint table, the `colour.msds_to_XYZ` multi-spectral batch intake, the bidirectional correlated-color-temperature axis, the `whiteness`/`yellowness`/`colour_rendering_index`/`colour_fidelity_index`/`dominant_wavelength`/`complementary_wavelength` colorimetric-index family, and the `colour_correction` measured-vs-reference CCM. ColorAide (pure-Python, zero-native, the `everything.ColorAll` all-plugins engine) carries the per-color presentation legs colour-science lacks — `Color.fit(method=...)` perceptual gamut mapping with the `in_gamut` predicate, the full `Color.filter(name=...)` CVD-plus-W3C effect surface scored by `delta_e`, the `Color.steps`/`Color.discrete`/`Color.interpolate` `method`/`hue`-keyed smooth-or-categorical palette over the `Color.harmony` wheel and `Color.average` seed-blend, the `Color.layer` blend-mode/Porter-Duff compositing with `Color.weighted_mix`, the `Color.blackbody` Planckian swatch, the OKLab-perceptual `Color.delta_e(method=...)` `ok`/`jz`/`hyab`/`itp`/`99o`/`cam02`/`cam16`/`hct` difference family colour-science has no Lab-array form for, the WCAG21 `Color.contrast`/`Color.luminance`/`Color.distance`/`Color.cct` safety measures, and the `Color.to_string` CSS-notation egress. `ColorOp` is ONE closed `@tagged_union` family carrying each operation's typed payload, never an erased `params` bag, dispatched by one total `match` returning `RuntimeRail[ColorReceipt]` — the one frozen-dataclass receipt every arm folds into, its `measures` an immutable `frozendict[Metric, float]` evidence map, its `notation` the `Color.to_string` CSS form of each output color, and its `wired()` `ColorReceiptWire` `msgspec.Struct` projection sinking ravelled `coords`+`shape` outward, never an overloaded `scalar` field, an un-encodable `NDArray`-on-the-wire owner, or an erased `object`. Each `Metric` row in the `_METRIC` `frozendict[Metric, MetricSpec]` binds the measure to the input `ColorModel` its engine demands, and `Measure` resolves every sample into that space through the one `_resolve` kernel `Convert` shares, so a difference, a whiteness, a wavelength, and a chromaticity each read their correct space rather than a uniform-signature table mis-feeding the wrong one. Scalar-bounded payloads admit through a refined `Annotated`+`beartype.vale.Is` contract at the `@beartype`-guarded factory so an out-of-range amount, count, spacing, kelvin, or wavelength is refused at construction, and every residual provider raise crosses the `async_boundary` seam. It hands palettes, conversions, adaptations, and measures to `visualization/chart#CHART`, `scene#SCENE`, `visualization/table#TABLE`, `graphic/marks/encode#MARK`, and the document output, so every visual artifact draws color from one owner through its `notation` CSS strings and `coords` arrays; the `ColorReceipt.wired()` `ColorReceiptWire` projection sinks to `data/tabular#WIRE` for columnar persistence, and the color-space and `describe_conversion_path` provenance on `path` feeds the `graphic/color/managed#MANAGED` ICC/LUT raster-egress leg. This page carries no raster egress — every arm resolves on the cp315 core and folds into `ColorReceipt`. It closes the `COLOR_MANAGED_VISUAL_PIPELINE` and `COLORBLIND_SAFE_PALETTE` ideas on the derivation side.

## [01]-[INDEX]

- [01]-[DERIVE]: the two-engine `Colorimetry` owner over the closed-payload `ColorOp` family — `convert`/`adapt`/`gamut`/`filter`/`palette`/`compose`/`temperature`/`measure`/`correct` folding into one typed `ColorReceipt` whose `tag` carries the same closed `ColorOpTag` literal and whose `measures` is a `Metric`-keyed scalar-evidence map, never a bare `str` tag or an overloaded `scalar` field; `colour.convert` is the universal colour-science gateway (with `colour.wavelength_to_XYZ` admitting a spectral-locus `Wavelength` source) and `describe_conversion_path(mode="Long")` recovers the resolved model-graph onto the receipt `path`; `colour.chromatic_adaptation` is the standalone cross-illuminant XYZ adaptation between `Illuminant` white points read from the `Observer`-keyed `_WHITEPOINT` `frozendict[Observer, frozendict[Illuminant, Tristimulus]]` nested table derived once from `colour.CCS_ILLUMINANTS[observer]`, keyed by the `CamMethod` `CHROMATIC_ADAPTATION_METHODS` registry (`Von Kries`/`CMCCAT2000`/`Zhai 2018`/`Li 2025`), distinct from the in-conversion `AdaptMethod` transforms `convert` resolves; ColorAide `everything.ColorAll` owns the disjoint gamut/filter/compose/palette/contrast legs; `ColorModel` is the dual-name `ModelNames(science, aide, spectral)` vocabulary collapsing the two engine spellings into one row whose `science` column keys colour-science and whose `aide` column keys ColorAide (either column `None` where one engine lacks the space) so one model value serves both engines — never two parallel model enums; `ColorFilter` keys the full `Color.filter` CVD-plus-W3C plugin map, `Ramp` is the closed palette-modality `@tagged_union` (`smooth` `Color.steps`, `discrete` `Color.discrete`, `harmony` `Color.harmony`) carrying its own `Interp`/`HueArc`/`Easing`/`Harmony` payload so a perceptually-even ramp, a categorical scale, and a named wheel are three cases of one axis — never a `harmony is None` discriminant beside parallel curve fields; `BlendMode`/`PorterDuff` key the `Color.layer` compositing axes, `FitMethod`/`Harmony` the ColorAide fit/harmony maps, `AdaptMethod` the `convert` `chromatic_adaptation_transform` registry, `CctMethod` the four `colour.temperature` illuminant methods and `Blackbody` the ColorAide Planckian methods, `CorrectMethod` the `colour_correction` CCM solvers, all carried as case payload; `Metric` is the one scalar-evidence vocabulary whose `_METRIC` `frozendict[Metric, MetricSpec]` binds each row to the input `ColorModel` its engine demands — colour-science CIE/CMC/DIN99 `delta_E` over `LAB`, `whiteness`/`yellowness` over `XYZ`, `dominant`/`complementary` wavelength over `XYY`, `cri`/`cfi` over `SPECTRAL`, the ColorAide OKLab-perceptual `ok`/`jz`/`hyab`/`itp`/`99o`/`cam02`/`cam16`/`hct` `delta_e` plus `distance`/`contrast`/`luminance`/`cct`/`duv`/`severity` over `SRGB`, and the `chromaticity-x`/`chromaticity-y` `XYY` reads — so `Measure` resolves each sample into the row's space through the shared `_resolve` kernel under its payload `Observer` before applying, never a uniform-signature table silently mis-feeding the wrong space; `ColorSource = SpectralDistribution | MultiSpectralDistributions | Tristimulus | Wavelength` is the convert/measure input union whose `MultiSpectralDistributions` member folds the batch `colour.msds_to_XYZ` modality, and `CctSource = Kelvin | Chromaticity` the bidirectional `Temperature` payload discriminating by input shape; scalar-bounded payloads (`Amount`/`PaletteCount`/`Spacing`/`Kelvin`/`Wavelength`) are refined `Annotated`+`Is` aliases admitted through the `@beartype`-guarded factory.

## [02]-[DERIVE]

- Owner: `Colorimetry` the one color-derivation owner discriminating operation over the closed `ColorOp` family; `ColorOp` an `expression.tagged_union` whose every case carries its own typed payload, never a shared erased `params` dict; `ColorReceipt` the one frozen-dataclass result every arm folds into, its `tag` carrying the same closed `ColorOpTag` literal the `ColorOp` discriminant carries — never a bare `str` the consumer must re-validate — and its `coords`/`notation`/`space`/`path`/`measures`/`in_gamut` slots recovering each algorithm choice, the `notation` the `Color.to_string` CSS form of each output color so a consumer renders without re-instantiating `Color`, the `measures` an immutable `frozendict[Metric, float]` map replacing any overloaded `scalar` so a difference, a contrast, a luminance, a CCT, a Duv, a chromaticity, and a filter severity each carry their own typed key rather than collapsing onto one ambiguous float, and its `wired()` `ColorReceiptWire` `msgspec.Struct` projection ravelling `coords`+`shape` for `data/tabular#WIRE` egress since an `NDArray` cannot ride a wire struct directly; colour-science is the conversion, appearance-model, CIE/CMC/DIN99-difference, cross-illuminant-adaptation, correlated-color-temperature, colorimetric-index, spectral-locus, CCM-correction, and `describe_conversion_path` graph-query engine on the cp315 core; ColorAide the pure-Python gamut-mapping, filter, compositing, smooth-or-categorical palette, harmony, average-blend, nearest-snap, blackbody, OKLab-perceptual-difference, and contrast/luminance/distance engine on the cp315 core; `ColorModel` the dual-name `ModelNames` vocabulary whose `science`/`aide` columns key the two engines from one model value so a `Palette`/`Gamut`/`Filter`/`Compose` arm reaches ColorAide through `space.aide` while a `Convert`/`Measure` arm reaches colour-science through `source.science`, either column `None` where one engine lacks the space, never a per-engine model enum; the `everything.ColorAll` all-plugins `Color` binds once at the capsule boundary per the manifest import policy, never re-imported per `derive`. The ICC/LUT/CCTF managed raster egress is NOT this owner's concern — it is `graphic/color/managed#MANAGED`'s; `Colorimetry` resolves the color-space and tone-curve provenance that leg consumes but writes no raster.
- Cases: `ColorOp` cases — `Convert(value, source, target, adapt)` (any model-pair through the universal `colour.convert` gateway, the typed `ColorSource = SpectralDistribution | MultiSpectralDistributions | Tristimulus | Wavelength` payload discriminating a single spectral distribution, a multi-channel batch folded through `colour.msds_to_XYZ`, a tristimulus array, and a single-`Wavelength` spectral-locus float folded through `colour.wavelength_to_XYZ(nm, cmfs)` under the payload `Observer` cmfs then converted to the target, absorbing the SD->XYZ->display and CIECAM02/CAM16 appearance arms as `ColorModel` source/target values reached through `source.science`/`target.science`, the receipt `path` recovered from `colour.describe_conversion_path(mode="Long")`) · `Adapt(value, source, target, method, observer)` (one cross-illuminant chromatic-adaptation axis folding `colour.chromatic_adaptation(xyz, source_white, dest_white, method=...)` where the `Illuminant` source/target resolve to XYZ white points through the `Observer`-keyed `_WHITEPOINT[observer]` table derived from `colour.CCS_ILLUMINANTS[observer]`, keyed by the `CamMethod` `CHROMATIC_ADAPTATION_METHODS` registry — the standalone explicit-white-point form `convert`'s in-conversion `chromatic_adaptation_transform` kwarg cannot express, never a parallel adaptation surface) · `Gamut(value, source, target, method)` (`Color(source.aide, value).convert(target.aide).clone().fit(method=...)` perceptual gamut mapping into a destination space, the pre-fit `in_gamut(target.aide)` predicate carried as evidence, the `_path` graph on the receipt `path`) · `Filter(value, name, amount)` (the single `Color.filter` surface keyed by the `ColorFilter` vocabulary — the `protan`/`deutan`/`tritan` CVD legs and the `brightness`/`contrast`/`saturate`/`hue-rotate`/`grayscale`/`sepia`/`invert`/`opacity` W3C effect legs folded into one case, the `amount` a refined `Amount` in `[0, 1]`, the source-to-filtered `delta_e` shift carried as the `Metric.SEVERITY` evidence — so a deficiency simulation and a presentation effect share one arm, never a parallel filter-versus-CVD surface) · `Palette(seed, stop, count, spacing, space, ramp, anchors)` (one perceptual-palette axis whose `Ramp` discriminant folds the `smooth` perceptually-even `Color.steps` ramp `max_delta_e`-spaced and `interp`/`hue`/`easing`-keyed, the `discrete` categorical `Color.discrete` Interpolator `interp`/`hue`/`easing`-keyed and sampled at `count` points for a non-blended data scale, or the `harmony` named-wheel `Color.harmony` — from the `Color.average` seed-blend, the `anchors` brand-color tuple snapping each ramp step to its nearest admissible color via `Color.closest` keyed on `delta_e`, the endpoint WCAG21 `contrast` carried as `Metric.CONTRAST`, the `count` a refined `PaletteCount` >= 1 and `spacing` a refined `Spacing` >= 0 — never a parallel ramp-versus-harmony-versus-discrete surface nor a `harmony is None` discriminant) · `Compose(colors, space, blend, operator, weights)` (the compositing axis whose empty-`weights` arm folds the W3C blend-mode/Porter-Duff stack via `Color.layer` keyed by `BlendMode`/`PorterDuff` and whose non-empty-`weights` arm folds the perceptual `Color.weighted_mix`, so a layered translucent overlay and a weighted N-color blend share one arm) · `Temperature(value, method, planck, space)` (one bidirectional correlated-color-temperature axis whose refined-`Kelvin` payload mints the Planckian swatch via `Color.blackbody` keyed by the `Blackbody` method and records the `colour.temperature.CCT_to_xy` chromaticity on `path` with `Metric.CCT`, and whose `Chromaticity` payload folds the inverse `colour.temperature.xy_to_CCT` to a `Kelvin` on `Metric.CCT`, the `CctMethod` illuminant on `path`, the direction discriminated by the `CctSource` input shape — never a parallel inverse-temperature surface) · `Measure(sample, source, reference, metric, observer)` (the one colorimetric-measure surface keyed by the `Metric` vocabulary over the `_METRIC` `frozendict[Metric, MetricSpec]` policy table — each row binding its measure to the input `ColorModel` it demands so the arm resolves the `source`-spaced sample and the `Option`-carried reference into that space through the shared `_resolve` kernel under the payload `Observer` cmfs and `D65` white before applying (the standard-observer axis `Convert`/`Adapt` also carry, so a 10-degree large-field whiteness/CRI/wavelength measure reads the right cmfs), the colour-science `delta_E`/`whiteness`/`yellowness`/`dominant_wavelength`/`complementary_wavelength`/`colour_rendering_index`/`colour_fidelity_index` rows over `LAB`/`XYZ`/`XYY`/`SPECTRAL` and the ColorAide `distance`/`contrast`/`luminance`/`cct`/`ok`/`jz`/`hyab`/`itp`/`99o`/`cam02`/`cam16`/`hct`/`chromaticity` rows over `SRGB`/`XYY` folded into one case, the resolved coordinates on `coords` and the result on the `metric`-keyed `measures` slot — so every scalar color measure shares one arm and reads its correct input space) · `Correct(measured, reference, method)` (the measured-vs-reference correction-matrix calibration via `colour.colour_correction` keyed by the `CorrectMethod` solver `Cheung 2004`/`Finlayson 2015`/`Vandermonde`, the corrected array on `coords`) — matched by one total `match`/`case`.
- Entry: `Colorimetry.derive` is `async` over the runtime `async_boundary` and dispatches the `ColorOp` case, returning one `RuntimeRail[ColorReceipt]` whose typed `ColorReceipt` carries the resolved `coords` array, the `notation` CSS strings, the output `space`, the `describe_conversion_path` model-graph `path`, the `frozendict[Metric, float]` `measures` evidence map, and the target-space `in_gamut` flag — never an erased `object`, so a re-admitting consumer recovers every algorithm choice and every scalar correlate from the receipt and projects it outward through `wired()`. The `async_boundary` narrows its `catch` to `(ValueError, KeyError)` — the colour-science/ColorAide domain raise a malformed CSS spec, an unregistered space, or a registry miss surfaces as — so an unexpected raise propagates as a defect rather than being silently railed. Admission is two-tier: the `@beartype`-guarded factory refuses an out-of-range scalar payload (`Amount`/`PaletteCount`/`Spacing`/`Kelvin`/`Wavelength`) at construction through its `Annotated`+`Is` contract, and `async_boundary` is the boundary-capture seam mapping every residual provider raise — a malformed CSS color spec, an unregistered space, a degenerate operand — into the runtime fault vocabulary, so the interior is total over the constructed op and threads no parallel `None`-as-failure path; a `NaN` coordinate is the ColorAide powerless-hue sentinel, a valid achromatic channel never gated as a fault. Every arm resolves synchronously on the cp315 core inside the async capsule: colour-science is pure-Python NumPy and ColorAide is pure-Python, so no leg crosses a process seam and no arm forces the async dispatch — the `async_boundary` is the uniform consumer contract the visual owners `await`, mirroring the settled `visualization/chart#EXPORT` `ChartExport.render -> RuntimeRail[ArtifactReceipt]` rail shape so a chart/table/scene consumer reaches color through one awaited rail rather than a per-engine sync branch. The result is a settled rail the visual owners consume, never a re-derivation per engine.
- Auto: `Convert` folds the source through the shared `_resolve` kernel under `colour.domain_range_scale("reference")` — a `Wavelength` float through `colour.wavelength_to_XYZ(nm, cmfs)` then `colour.convert(xyz, "CIE XYZ", target.science)`, a `MultiSpectralDistributions` batch through `colour.msds_to_XYZ(value, cmfs, ...)` then convert, and a spectral/tristimulus source through `colour.convert(value, source.science, target.science, chromatic_adaptation_transform=adapt.value)`, the `cmfs` read from `colour.MSDS_CMFS[observer]` — the receipt carrying `target.science`, the `_notate` CSS `notation`, and the `describe_conversion_path(mode="Long")` resolved model-graph as `path`; `Adapt` folds `colour.chromatic_adaptation(np.asarray(value), _WHITEPOINT[observer][source], _WHITEPOINT[observer][target], method=method.value)` under `colour.domain_range_scale("reference")`, the adapted XYZ on `coords` and the observer/source/target illuminant plus method on `path`; `Gamut` folds the source through `Color(source.aide, value).convert(target.aide)` then a `clone().fit(method=...)`, carrying the pre-fit `in_gamut(target.aide)` predicate as evidence so a no-op fit is recoverable; `Filter` folds `Color('srgb', value).clone().filter(name.value, amount=amount)` and carries the source-to-filtered `delta_e` shift as `Metric.SEVERITY`; `Palette` folds the multi-color `seed` tuple through `Color.average(seeds, space=space.aide, out_space='srgb')` to one perceptually-weighted base, then discriminates on the `Ramp` case — the `smooth` arm folds `Color.steps([base, stop], steps=count, max_steps=count, max_delta_e=spacing, delta_e="2000", method=interp.value, hue=hue.value, progress=_EASING[easing], space=space.aide, out_space='srgb')`, the `discrete` arm folds the `Color.discrete([base, stop], steps=count, method=interp.value, hue=hue.value, progress=_EASING[easing], space=space.aide, out_space='srgb')` Interpolator sampled at `count` even points (`curve(i / (count - 1))`) for the categorical scale, the `harmony` arm folds `base.harmony(name.value, ...)` — then, when `anchors` is non-empty, snaps each step through `step.closest(list(anchors), method="2000")` to the nearest admissible brand color, snapshotting the endpoint WCAG21 `contrast` as `Metric.CONTRAST` and the ramp kind/curve/hue/easing on `path`; `Compose` folds a non-empty-`weights` payload through `Color.weighted_mix(colors, weights=..., space=space.aide, out_space='srgb')` and an empty-`weights` payload through `Color.layer(colors, blend=blend.value, operator=operator.value, space=space.aide, out_space='srgb')`; `Temperature` discriminates on the `CctSource` shape — a `Kelvin` float mints `Color.blackbody(space.aide, value, method=planck.value).convert('srgb')` carrying its `to_string` `notation`, records `colour.temperature.CCT_to_xy(value, method=method.value)` as `Metric.CHROMATICITY_X`/`CHROMATICITY_Y` and the kelvin as `Metric.CCT`, a `Chromaticity` tuple folds `colour.temperature.xy_to_CCT(np.asarray(value), method=method.value)` to the `Metric.CCT` Kelvin beside its `Metric.CHROMATICITY_X`/`CHROMATICITY_Y` reads; `Measure` resolves the `source`-spaced `sample` and the `Option`-carried `reference` (mapped through `_resolve`, defaulting to the payload `Observer` `D65` white when `Nothing`) into the metric's required `space` through `_resolve` under that `Observer`, then applies the `_METRIC[metric]` callable onto the `metric`-keyed `measures` slot with the resolved coordinates on `coords`; `Correct` folds `colour.colour_correction(measured, measured, reference, method=method.value)` to the corrected array on `coords` with its `_notate` `notation`. Each arm writes one `ColorReceipt` with its own typed `frozendict` `measures` keys and `notation` strings, never an overloaded scalar.
- Packages: `colour-science` (`convert` the universal gateway subsuming `sd_to_XYZ`/`XYZ_to_sRGB`/`RGB_to_XYZ`, `wavelength_to_XYZ` the spectral-locus intake, `describe_conversion_path(mode="Long", print_callable=...)` the model-graph query feeding the receipt `path`, `RGB_COLOURSPACES` the named-space registry, `delta_E` over the `DELTA_E_METHODS` registry, `chromatic_adaptation` the standalone cross-white-point CAT over `CHROMATIC_ADAPTATION_METHODS` feeding `CamMethod`, `CHROMATIC_ADAPTATION_TRANSFORMS` feeding `AdaptMethod`, `CCS_ILLUMINANTS`/`xy_to_XYZ` over the `Observer` axis feeding the `_WHITEPOINT` nested table, `MSDS_CMFS`/`SDS_ILLUMINANTS` the observer cmfs and illuminant SPD feeding `msds_to_XYZ`/`wavelength_to_XYZ`, `msds_to_XYZ` the multi-spectral batch intake, `colour.temperature.CCT_to_xy`/`xy_to_CCT` over the illuminant-method axis feeding `CctMethod`, `whiteness`/`yellowness`/`dominant_wavelength`/`complementary_wavelength`/`colour_rendering_index`/`colour_fidelity_index` the colorimetric-index family feeding `Metric`, `colour_correction` over the `Cheung 2004`/`Finlayson 2015`/`Vandermonde` solvers feeding `CorrectMethod`, `SpectralDistribution`, `domain_range_scale`), `coloraide` (`everything.ColorAll` as the all-plugins `Color`; `Color.steps`/`Color.discrete`/`Color.interpolate` with `max_delta_e`/`delta_e`/`method`/`hue` smooth-or-categorical spacing, `Color.harmony` named wheels, `Color.average`/`Color.weighted_mix` perceptual blends, `Color.layer` blend-mode/Porter-Duff compositing, `Color.closest` nearest-by-`delta_e` snap, `Color.blackbody` Planckian swatch, `Color.contrast`/`Color.luminance`/`Color.distance`/`Color.cct`/`Color.delta_e` measures over the `ok`/`jz`/`hyab`/`itp`/`99o`/`cam02`/`cam16`/`hct` OKLab-perceptual methods, `Color.convert`/`Color.clone`/`Color.fit`/`Color.in_gamut`/`Color.filter`/`Color.coords`/`Color.to_string` the CSS-notation egress), `numpy` (the array backing) on the cp315 core; `beartype` (`beartype` factory contract + `beartype.vale.Is` refinement on the `Amount`/`PaletteCount`/`Spacing`/`Kelvin`/`Wavelength` payload aliases); runtime (`faults.RuntimeRail`/`async_boundary`). No `to_process`/`to_interpreter` seam — every arm resolves in-capsule on the core; the ICC raster apply that does cross a process seam lives on `graphic/color/managed#MANAGED`, not here.
- Growth: a new color-derivation operation is one `ColorOp` case carrying its payload plus one acceptor arm folding into `ColorReceipt`, its `tag` extending the one `ColorOpTag` literal the receipt shares; a new colorimetric measure is one `Metric` member plus one `_METRIC` `MetricSpec` row carrying its input `ColorModel` and callable, the receipt key already typed, a measure under a different standard observer the `Observer` axis already on the `Measure` payload; a new filter or CVD type is one `ColorFilter` row; a new gamut-fit method is one `FitMethod` row; a new palette modality is one `Ramp` case, a new interpolation curve one `Interp` row, a new hue arc one `HueArc` row, a new easing curve one `Easing` row and one `_EASING` callable; a new blend mode is one `BlendMode` row and a new compositing operator one `PorterDuff` row; a new harmony wheel is one `Harmony` row; a new appearance or named model is one `ColorModel` row carrying both its `science` and `aide` (or `None` where one engine lacks it) name columns; a new CCT illuminant method is one `CctMethod` row and a new Planckian method one `Blackbody` row; a new in-conversion adaptation transform is one `AdaptMethod` row and a new standalone CAT one `CamMethod` row; a new adaptation white point is one `Illuminant` row keying `CCS_ILLUMINANTS` and a new standard observer one `Observer` row keying the nested `_WHITEPOINT`/`MSDS_CMFS`; a new correction solver is one `CorrectMethod` row; a brand-palette constraint is the `anchors` tuple on the existing `Palette` payload, a weighted blend the `weights` tuple on the existing `Compose` payload, a batch spectral source the `MultiSpectralDistributions` member on the existing `ColorSource` union, a new persisted wire column one `ColorReceiptWire` field derived in `wired()` — never a parallel surface; zero new surface.
- Boundary: no chart/scene rendering (that stays at the visual owners); no ICC/LUT/CCTF raster egress (that is `graphic/color/managed#MANAGED`'s exclusively — this owner resolves the color-space and tone-curve provenance the managed leg consumes but writes no raster and imports no `pillow`); the color arms emit one `ColorReceipt` carrying `coords` arrays, the `notation` CSS strings, the `frozendict[Metric, float]` `measures` map, chromaticities, and palettes, projected outward through the `wired()` `ColorReceiptWire` and consumed inward by the visual and document sub-domains; the gamut-mapping, filter (CVD + W3C), compositing, smooth-or-categorical palette, harmony, average/weighted-blend, nearest-snap, blackbody, OKLab-perceptual difference, and contrast/luminance/distance legs are ColorAide's, and colour-science keeps spectral/CAM/CIE-difference/cross-illuminant-adaptation/temperature/colorimetric-index/CCM/graph-query, never a second convert, gamut-fit, or contrast engine. The difference family spans both engines under the ONE `_METRIC` surface — colour-science `delta_E` owns the CIE/CMC/DIN99 Lab-array rows, ColorAide `Color.delta_e` owns the OKLab-perceptual `ok`/`jz`/`hyab`/`itp`/`99o`/`cam02`/`cam16`/`hct` sRGB-coord rows colour-science has no equivalent for — so the table is one measure surface keyed by `Metric`, never two parallel difference owners. `everything.ColorAll` is the single all-plugins `Color` so OKLCH/HCT and the registered fit/filter/harmony/blend/average/closest/blackbody plugins resolve without a per-space class; the `colour.temperature` illuminant-method literal set, the `describe_conversion_path` `mode`/`print_callable` capture surface, the `CHROMATIC_ADAPTATION_TRANSFORMS`/`CHROMATIC_ADAPTATION_METHODS` registry bindings, the `CCS_ILLUMINANTS` whitepoint keys, the ColorAide harmony-name and `BlendMode`/`PorterDuff` operator strings, and the `science=None` wide-gamut/`HSL`/`HWB` space rows stay [03]-[RESEARCH] catalogue-deepen items until the folder `.api` catalogues carry their full literal sets; every other colour-science and ColorAide spelling below is settled fence code.

```python signature
from collections.abc import Callable
from dataclasses import dataclass
from enum import Enum, StrEnum
from typing import Annotated, Literal, NamedTuple, assert_never

import colour
import numpy as np
from beartype import beartype
from beartype.vale import Is
from builtins import frozendict
from coloraide import ease, ease_in, ease_in_out, ease_out, linear
from coloraide.everything import ColorAll as Color
from colour import MultiSpectralDistributions, SpectralDistribution
from expression import Option, case, tag, tagged_union
from msgspec import Struct
from numpy.typing import NDArray

from rasm.runtime.faults import RuntimeRail, async_boundary

type Tristimulus = NDArray[np.float64]
type Wavelength = Annotated[float, Is[lambda nm: 360.0 <= nm <= 830.0]]
type ColorSource = SpectralDistribution | MultiSpectralDistributions | Tristimulus | Wavelength
type MetricInput = Tristimulus | SpectralDistribution
type Kelvin = Annotated[float, Is[lambda k: k > 0.0]]
type Chromaticity = tuple[float, float]
type CctSource = Kelvin | Chromaticity
type Amount = Annotated[float, Is[lambda a: 0.0 <= a <= 1.0]]
type PaletteCount = Annotated[int, Is[lambda n: n >= 1]]
type Spacing = Annotated[float, Is[lambda s: s >= 0.0]]
type ColorOpTag = Literal["convert", "adapt", "gamut", "filter", "palette", "compose", "temperature", "measure", "correct"]


class ModelNames(NamedTuple):
    science: str | None
    aide: str | None
    spectral: bool = False


class ColorModel(ModelNames, Enum):
    SPECTRAL = ModelNames("Spectral Distribution", None, spectral=True)
    XYZ = ModelNames("CIE XYZ", "xyz-d65")
    XYY = ModelNames("CIE xyY", "xyy")
    LAB = ModelNames("CIE Lab", "lab-d65")
    LCHAB = ModelNames("CIE LCHab", "lch-d65")
    OKLAB = ModelNames("Oklab", "oklab")
    OKLCH = ModelNames("Oklch", "oklch")
    SRGB = ModelNames("sRGB", "srgb")
    ICTCP = ModelNames("ICtCp", "ictcp")
    DISPLAY_P3 = ModelNames(None, "display-p3")
    REC2020 = ModelNames(None, "rec2020")
    HSV = ModelNames("HSV", "hsv")
    HSL = ModelNames(None, "hsl")
    HWB = ModelNames(None, "hwb")
    CIECAM02 = ModelNames("CIECAM02", None)
    CAM16 = ModelNames("CAM16", None)


class Observer(StrEnum):
    CIE_1931_2 = "CIE 1931 2 Degree Standard Observer"
    CIE_1964_10 = "CIE 1964 10 Degree Standard Observer"


class Illuminant(StrEnum):
    D65 = "D65"
    D50 = "D50"
    D55 = "D55"
    D75 = "D75"
    A = "A"
    E = "E"


class AdaptMethod(StrEnum):
    VON_KRIES = "Von Kries"
    BRADFORD = "Bradford"
    CAT02 = "CAT02"
    CAT16 = "CAT16"
    CMCCAT2000 = "CMCCAT2000"
    SHARP = "Sharp"


class CamMethod(StrEnum):
    VON_KRIES = "Von Kries"
    CMCCAT2000 = "CMCCAT2000"
    ZHAI_2018 = "Zhai 2018"
    LI_2025 = "Li 2025"


class FitMethod(StrEnum):
    RAYTRACE = "raytrace"
    OKLCH_CHROMA = "oklch-chroma"
    LCH_CHROMA = "lch-chroma"
    MINDE_CHROMA = "minde-chroma"
    SCALE = "scale"
    SCALE_LUMINANCE = "scale-luminance"


class ColorFilter(StrEnum):
    PROTAN = "protan"
    DEUTAN = "deutan"
    TRITAN = "tritan"
    BRIGHTNESS = "brightness"
    CONTRAST = "contrast"
    SATURATE = "saturate"
    HUE_ROTATE = "hue-rotate"
    GRAYSCALE = "grayscale"
    SEPIA = "sepia"
    INVERT = "invert"
    OPACITY = "opacity"


class Harmony(StrEnum):
    MONOCHROMATIC = "mono"
    COMPLEMENTARY = "complement"
    SPLIT_COMPLEMENTARY = "split"
    ANALOGOUS = "analogous"
    TRIADIC = "triad"
    SQUARE = "square"
    RECTANGLE = "rectangle"
    WHEEL = "wheel"


class Interp(StrEnum):
    LINEAR = "linear"
    CONTINUOUS = "continuous"
    BSPLINE = "bspline"
    NATURAL = "natural"
    MONOTONE = "monotone"
    CSS_LINEAR = "css-linear"


class HueArc(StrEnum):
    SHORTER = "shorter"
    LONGER = "longer"
    INCREASING = "increasing"
    DECREASING = "decreasing"


class Easing(StrEnum):
    LINEAR = "linear"
    EASE = "ease"
    EASE_IN = "ease-in"
    EASE_OUT = "ease-out"
    EASE_IN_OUT = "ease-in-out"


class BlendMode(StrEnum):
    NORMAL = "normal"
    MULTIPLY = "multiply"
    SCREEN = "screen"
    OVERLAY = "overlay"
    DARKEN = "darken"
    LIGHTEN = "lighten"
    COLOR_DODGE = "color-dodge"
    COLOR_BURN = "color-burn"
    HARD_LIGHT = "hard-light"
    SOFT_LIGHT = "soft-light"
    DIFFERENCE = "difference"
    EXCLUSION = "exclusion"
    HUE = "hue"
    SATURATION = "saturation"
    COLOR = "color"
    LUMINOSITY = "luminosity"


class PorterDuff(StrEnum):
    SOURCE_OVER = "source-over"
    DESTINATION_OVER = "destination-over"
    SOURCE_IN = "source-in"
    DESTINATION_IN = "destination-in"
    SOURCE_OUT = "source-out"
    DESTINATION_OUT = "destination-out"
    SOURCE_ATOP = "source-atop"
    DESTINATION_ATOP = "destination-atop"
    XOR = "xor"


class CctMethod(StrEnum):
    DAYLIGHT = "CIE Illuminant D Series"
    KANG = "Kang 2002"
    HERNANDEZ = "Hernandez 1999"
    MCCAMY = "McCamy 1992"


class Blackbody(StrEnum):
    OHNO_2013 = "ohno-2013"
    ROBERTSON_1968 = "robertson-1968"


class CorrectMethod(StrEnum):
    CHEUNG_2004 = "Cheung 2004"
    FINLAYSON_2015 = "Finlayson 2015"
    VANDERMONDE = "Vandermonde"


class Metric(StrEnum):
    DELTA_E_2000 = "deltaE-2000"
    DELTA_E_1994 = "deltaE-1994"
    DELTA_E_1976 = "deltaE-1976"
    DELTA_E_CMC = "deltaE-CMC"
    DELTA_E_DIN99 = "deltaE-DIN99"
    DELTA_E_OK = "deltaE-ok"
    DELTA_E_JZ = "deltaE-jz"
    DELTA_E_HYAB = "deltaE-hyab"
    DELTA_E_ITP = "deltaE-itp"
    DELTA_E_99O = "deltaE-99o"
    DELTA_E_CAM02 = "deltaE-cam02"
    DELTA_E_CAM16 = "deltaE-cam16"
    DELTA_E_HCT = "deltaE-hct"
    DISTANCE = "distance"
    CONTRAST = "contrast"
    LUMINANCE = "luminance"
    WHITENESS = "whiteness"
    YELLOWNESS = "yellowness"
    DOMINANT_WAVELENGTH = "dominant-wavelength"
    COMPLEMENTARY_WAVELENGTH = "complementary-wavelength"
    CRI = "cri"
    CFI = "cfi"
    CCT = "cct"
    DUV = "duv"
    CHROMATICITY_X = "chromaticity-x"
    CHROMATICITY_Y = "chromaticity-y"
    SEVERITY = "severity"


class MetricSpec(NamedTuple):
    space: ColorModel  # the input space the row demands; Measure resolves sample+reference into it before applying fn
    fn: Callable[[MetricInput, MetricInput], float]


# Observer-keyed whitepoint XYZ derived once from CCS_ILLUMINANTS so a 2-degree vs 10-degree adaptation reads the right white.
_WHITEPOINT: frozendict[Observer, frozendict[Illuminant, Tristimulus]] = frozendict({
    observer: frozendict({
        illum: np.asarray(colour.xy_to_XYZ(colour.CCS_ILLUMINANTS[observer.value][illum.value]), dtype=np.float64) for illum in Illuminant
    })
    for observer in Observer
})

# Color.steps/discrete take a progress= easing callable; _EASING resolves the Easing vocabulary to the package's own curves.
_EASING: frozendict[Easing, Callable[[float], float]] = frozendict({
    Easing.LINEAR: linear, Easing.EASE: ease, Easing.EASE_IN: ease_in, Easing.EASE_OUT: ease_out, Easing.EASE_IN_OUT: ease_in_out,
})

# Each row binds the metric to the input ColorModel its engine demands — colour delta_E/whiteness Lab/XYZ, dominant/complementary xy, CRI/CFI a SpectralDistribution, the ColorAide rows sRGB — so Measure resolves sample+reference into that space before applying fn.
_METRIC: frozendict[Metric, MetricSpec] = frozendict({
    Metric.DELTA_E_2000: MetricSpec(ColorModel.LAB, lambda a, b: float(colour.delta_E(b, a, method="CIE 2000"))),
    Metric.DELTA_E_1994: MetricSpec(ColorModel.LAB, lambda a, b: float(colour.delta_E(b, a, method="CIE 1994"))),
    Metric.DELTA_E_1976: MetricSpec(ColorModel.LAB, lambda a, b: float(colour.delta_E(b, a, method="CIE 1976"))),
    Metric.DELTA_E_CMC: MetricSpec(ColorModel.LAB, lambda a, b: float(colour.delta_E(b, a, method="CMC"))),
    Metric.DELTA_E_DIN99: MetricSpec(ColorModel.LAB, lambda a, b: float(colour.delta_E(b, a, method="DIN99"))),
    Metric.DELTA_E_OK: MetricSpec(ColorModel.SRGB, lambda a, b: Color("srgb", list(a)).delta_e(Color("srgb", list(b)), method="ok")),
    Metric.DELTA_E_JZ: MetricSpec(ColorModel.SRGB, lambda a, b: Color("srgb", list(a)).delta_e(Color("srgb", list(b)), method="jz")),
    Metric.DELTA_E_HYAB: MetricSpec(ColorModel.SRGB, lambda a, b: Color("srgb", list(a)).delta_e(Color("srgb", list(b)), method="hyab")),
    Metric.DELTA_E_ITP: MetricSpec(ColorModel.SRGB, lambda a, b: Color("srgb", list(a)).delta_e(Color("srgb", list(b)), method="itp")),
    Metric.DELTA_E_99O: MetricSpec(ColorModel.SRGB, lambda a, b: Color("srgb", list(a)).delta_e(Color("srgb", list(b)), method="99o")),
    Metric.DELTA_E_CAM02: MetricSpec(ColorModel.SRGB, lambda a, b: Color("srgb", list(a)).delta_e(Color("srgb", list(b)), method="cam02")),
    Metric.DELTA_E_CAM16: MetricSpec(ColorModel.SRGB, lambda a, b: Color("srgb", list(a)).delta_e(Color("srgb", list(b)), method="cam16")),
    Metric.DELTA_E_HCT: MetricSpec(ColorModel.SRGB, lambda a, b: Color("srgb", list(a)).delta_e(Color("srgb", list(b)), method="hct")),
    Metric.DISTANCE: MetricSpec(ColorModel.SRGB, lambda a, b: Color("srgb", list(a)).distance(Color("srgb", list(b)), space="lab")),
    Metric.CONTRAST: MetricSpec(ColorModel.SRGB, lambda a, b: Color("srgb", list(a)).contrast(Color("srgb", list(b)))),
    Metric.LUMINANCE: MetricSpec(ColorModel.SRGB, lambda a, _b: Color("srgb", list(a)).luminance()),
    Metric.WHITENESS: MetricSpec(ColorModel.XYZ, lambda a, b: float(colour.whiteness(a, b))),
    Metric.YELLOWNESS: MetricSpec(ColorModel.XYZ, lambda a, _b: float(colour.yellowness(a))),
    Metric.DOMINANT_WAVELENGTH: MetricSpec(ColorModel.XYY, lambda a, b: float(np.asarray(colour.dominant_wavelength(a[:2], b[:2]))[0])),
    Metric.COMPLEMENTARY_WAVELENGTH: MetricSpec(ColorModel.XYY, lambda a, b: float(np.asarray(colour.complementary_wavelength(a[:2], b[:2]))[0])),
    Metric.CRI: MetricSpec(ColorModel.SPECTRAL, lambda a, _b: float(colour.colour_rendering_index(a))),
    Metric.CFI: MetricSpec(ColorModel.SPECTRAL, lambda a, _b: float(colour.colour_fidelity_index(a))),
    Metric.CCT: MetricSpec(ColorModel.SRGB, lambda a, _b: float(Color("srgb", list(a)).cct()[0])),
    Metric.DUV: MetricSpec(ColorModel.SRGB, lambda a, _b: float(Color("srgb", list(a)).cct()[1])),
    Metric.CHROMATICITY_X: MetricSpec(ColorModel.XYY, lambda a, _b: float(a[0])),
    Metric.CHROMATICITY_Y: MetricSpec(ColorModel.XYY, lambda a, _b: float(a[1])),
    Metric.SEVERITY: MetricSpec(ColorModel.SRGB, lambda a, b: Color("srgb", list(a)).delta_e(Color("srgb", list(b)), method="2000")),
})


@tagged_union(frozen=True)
class Ramp:
    tag: Literal["smooth", "discrete", "harmony"] = tag()
    smooth: tuple[Interp, HueArc, Easing] = case()
    discrete: tuple[Interp, HueArc, Easing] = case()
    harmony: Harmony = case()

    @staticmethod
    def Smooth(interp: Interp = Interp.BSPLINE, hue: HueArc = HueArc.SHORTER, easing: Easing = Easing.LINEAR) -> "Ramp":
        return Ramp(smooth=(interp, hue, easing))

    @staticmethod
    def Discrete(interp: Interp = Interp.LINEAR, hue: HueArc = HueArc.SHORTER, easing: Easing = Easing.LINEAR) -> "Ramp":
        return Ramp(discrete=(interp, hue, easing))

    @staticmethod
    def Harmony(name: Harmony = Harmony.ANALOGOUS) -> "Ramp":
        return Ramp(harmony=name)


@tagged_union(frozen=True)
class ColorOp:
    tag: ColorOpTag = tag()
    convert: tuple[ColorSource, ColorModel, ColorModel, AdaptMethod, Observer] = case()
    adapt: tuple[Tristimulus, Illuminant, Illuminant, CamMethod, Observer] = case()
    gamut: tuple[Tristimulus, ColorModel, ColorModel, FitMethod] = case()
    filter: tuple[Tristimulus, ColorFilter, Amount] = case()
    palette: tuple[tuple[str, ...], str, PaletteCount, Spacing, ColorModel, Ramp, tuple[str, ...]] = case()
    compose: tuple[tuple[str, ...], ColorModel, BlendMode, PorterDuff, tuple[float, ...]] = case()
    temperature: tuple[CctSource, CctMethod, Blackbody, ColorModel] = case()
    measure: tuple[ColorSource, ColorModel, Option[Tristimulus], Metric, Observer] = case()
    correct: tuple[Tristimulus, Tristimulus, CorrectMethod] = case()

    @staticmethod
    @beartype
    def Convert(value: ColorSource, source: ColorModel, target: ColorModel, adapt: AdaptMethod = AdaptMethod.BRADFORD, observer: Observer = Observer.CIE_1931_2) -> "ColorOp":
        return ColorOp(convert=(value, source, target, adapt, observer))

    @staticmethod
    def Adapt(value: Tristimulus, source: Illuminant, target: Illuminant, method: CamMethod = CamMethod.VON_KRIES, observer: Observer = Observer.CIE_1931_2) -> "ColorOp":
        return ColorOp(adapt=(value, source, target, method, observer))

    @staticmethod
    def Gamut(value: Tristimulus, source: ColorModel, target: ColorModel, method: FitMethod = FitMethod.OKLCH_CHROMA) -> "ColorOp":
        return ColorOp(gamut=(value, source, target, method))

    @staticmethod
    @beartype
    def Filter(value: Tristimulus, name: ColorFilter, amount: Amount = 1.0) -> "ColorOp":
        return ColorOp(filter=(value, name, amount))

    @staticmethod
    @beartype
    def Palette(seed: tuple[str, ...], stop: str, count: PaletteCount, spacing: Spacing = 0.0, space: ColorModel = ColorModel.OKLCH, ramp: Ramp = Ramp.Smooth(), anchors: tuple[str, ...] = ()) -> "ColorOp":
        return ColorOp(palette=(seed, stop, count, spacing, space, ramp, anchors))

    @staticmethod
    def Compose(colors: tuple[str, ...], space: ColorModel = ColorModel.OKLCH, blend: BlendMode = BlendMode.NORMAL, operator: PorterDuff = PorterDuff.SOURCE_OVER, weights: tuple[float, ...] = ()) -> "ColorOp":
        return ColorOp(compose=(colors, space, blend, operator, weights))

    @staticmethod
    @beartype
    def Temperature(value: CctSource, method: CctMethod = CctMethod.DAYLIGHT, planck: Blackbody = Blackbody.OHNO_2013, space: ColorModel = ColorModel.SRGB) -> "ColorOp":
        return ColorOp(temperature=(value, method, planck, space))

    @staticmethod
    def Measure(sample: ColorSource, metric: Metric, source: ColorModel = ColorModel.SRGB, reference: Tristimulus | None = None, observer: Observer = Observer.CIE_1931_2) -> "ColorOp":
        return ColorOp(measure=(sample, source, Option.of_optional(reference), metric, observer))

    @staticmethod
    def Correct(measured: Tristimulus, reference: Tristimulus, method: CorrectMethod = CorrectMethod.CHEUNG_2004) -> "ColorOp":
        return ColorOp(correct=(measured, reference, method))


@dataclass(frozen=True, slots=True, kw_only=True)
class ColorReceipt:
    tag: ColorOpTag
    coords: Tristimulus
    notation: tuple[str, ...] = ()  # Color.to_string() of each output color so a consumer renders without re-instantiating Color
    space: str = ""
    path: tuple[str, ...] = ()
    measures: frozendict[Metric, float] = frozendict()
    in_gamut: bool = True

    def wired(self) -> "ColorReceiptWire":
        return ColorReceiptWire(
            tag=self.tag,
            coords=tuple(float(c) for c in np.ravel(self.coords)),
            shape=tuple(int(d) for d in self.coords.shape),
            notation=self.notation,
            space=self.space,
            path=self.path,
            measures={metric.value: value for metric, value in self.measures.items()},
            in_gamut=self.in_gamut,
        )


class ColorReceiptWire(Struct, frozen=True, omit_defaults=True):
    tag: ColorOpTag
    coords: tuple[float, ...]  # ravelled; shape reconstructs the N-color palette or single-color row at the sink
    shape: tuple[int, ...]
    notation: tuple[str, ...] = ()
    space: str = ""
    path: tuple[str, ...] = ()
    measures: dict[str, float] = {}
    in_gamut: bool = True


class Colorimetry(Struct, frozen=True):
    op: ColorOp

    async def derive(self) -> RuntimeRail[ColorReceipt]:
        return await async_boundary(f"color.{self.op.tag}", self._compute, catch=(ValueError, KeyError))

    async def _compute(self) -> ColorReceipt:
        match self.op:
            case ColorOp(tag="convert", convert=(value, source, target, adapt, observer)):
                with colour.domain_range_scale("reference"):
                    coords = np.asarray(self._resolve(value, source, target, observer, adapt), dtype=np.float64)
                return ColorReceipt(tag="convert", coords=coords, notation=self._notate(target.aide, coords), space=target.science or target.aide or "", path=self._path(source, target))
            case ColorOp(tag="adapt", adapt=(value, source, target, method, observer)):
                with colour.domain_range_scale("reference"):
                    adapted = np.asarray(colour.chromatic_adaptation(np.asarray(value, dtype=np.float64), _WHITEPOINT[observer][source], _WHITEPOINT[observer][target], method=method.value), dtype=np.float64)
                return ColorReceipt(tag="adapt", coords=adapted, notation=self._notate(ColorModel.XYZ.aide, adapted), space="CIE XYZ", path=(observer.value, source.value, target.value, method.value))
            case ColorOp(tag="gamut", gamut=(value, source, target, method)):
                seeded = Color(source.aide, list(value)).convert(target.aide)
                fitted = seeded.clone().fit(method=method.value)
                coords = np.asarray(fitted.coords(), dtype=np.float64)
                return ColorReceipt(tag="gamut", coords=coords, notation=(fitted.to_string(),), space=target.aide or "", path=self._path(source, target), in_gamut=seeded.in_gamut(target.aide))
            case ColorOp(tag="filter", filter=(value, name, amount)):
                origin = Color("srgb", list(value))
                filtered = origin.clone().filter(name.value, amount=amount)
                coords = np.asarray(filtered.coords(), dtype=np.float64)
                return ColorReceipt(tag="filter", coords=coords, notation=(filtered.to_string(),), space="srgb", measures=frozendict({Metric.SEVERITY: origin.delta_e(filtered, method="2000")}))
            case ColorOp(tag="palette", palette=(seed, stop, count, spacing, space, ramp, anchors)):
                base = Color.average(list(seed), space=space.aide, out_space="srgb")
                match ramp:
                    case Ramp(tag="smooth", smooth=(interp, hue, easing)):
                        ramped = Color.steps([base, stop], steps=count, max_steps=count, max_delta_e=spacing, delta_e="2000", method=interp.value, hue=hue.value, progress=_EASING[easing], space=space.aide, out_space="srgb")
                        trail = (ramp.tag, interp.value, hue.value, easing.value)
                    case Ramp(tag="discrete", discrete=(interp, hue, easing)):
                        curve = Color.discrete([base, stop], steps=count, method=interp.value, hue=hue.value, progress=_EASING[easing], space=space.aide, out_space="srgb")
                        ramped = [curve(i / (count - 1)) if count > 1 else curve(0.0) for i in range(count)]
                        trail = (ramp.tag, interp.value, hue.value, easing.value)
                    case Ramp(tag="harmony", harmony=name):
                        ramped = base.harmony(name.value, space=space.aide, out_space="srgb")
                        trail = (ramp.tag, name.value)
                    case _ as unreachable:
                        assert_never(unreachable)
                snapped = [step.closest(list(anchors), method="2000") for step in ramped] if anchors else ramped
                coords = np.array([step.coords() for step in snapped], dtype=np.float64)
                contrast = float(Color("srgb", list(coords[0])).contrast(Color("srgb", list(coords[-1]))))
                return ColorReceipt(tag="palette", coords=coords, notation=tuple(step.to_string() for step in snapped), space="srgb", path=(space.aide or "", *trail), measures=frozendict({Metric.CONTRAST: contrast}))
            case ColorOp(tag="compose", compose=(colors, space, blend, operator, weights)):
                blended = (
                    Color.weighted_mix(list(colors), weights=list(weights), space=space.aide, out_space="srgb")
                    if weights
                    else Color.layer(list(colors), blend=blend.value, operator=operator.value, space=space.aide, out_space="srgb")
                )
                coords = np.asarray(blended.coords(), dtype=np.float64)
                return ColorReceipt(tag="compose", coords=coords, notation=(blended.to_string(),), space="srgb", path=(blend.value, operator.value))
            case ColorOp(tag="temperature", temperature=(value, method, planck, space)):
                match value:
                    case float() as kelvin:
                        swatch = Color.blackbody(space.aide, kelvin, method=planck.value).convert("srgb")
                        chroma = np.asarray(colour.temperature.CCT_to_xy(kelvin, method=method.value), dtype=np.float64)
                        coords = np.asarray(swatch.coords(), dtype=np.float64)
                        return ColorReceipt(tag="temperature", coords=coords, notation=(swatch.to_string(),), space="srgb", path=(method.value, planck.value), measures=frozendict({Metric.CCT: kelvin, Metric.CHROMATICITY_X: float(chroma[0]), Metric.CHROMATICITY_Y: float(chroma[1])}))
                    case (x, y):
                        kelvin = float(np.ravel(colour.temperature.xy_to_CCT(np.asarray((x, y), dtype=np.float64), method=method.value))[0])
                        return ColorReceipt(tag="temperature", coords=np.asarray((x, y), dtype=np.float64), space="CIE xyY", path=(method.value,), measures=frozendict({Metric.CCT: kelvin, Metric.CHROMATICITY_X: x, Metric.CHROMATICITY_Y: y}))
                    case _ as unreachable:
                        assert_never(unreachable)
            case ColorOp(tag="measure", measure=(sample, source, reference, metric, observer)):
                space, fn = _METRIC[metric]
                with colour.domain_range_scale("reference"):
                    a = self._resolve(sample, source, space, observer)
                    b = reference.map(lambda ref: self._resolve(ref, source, space, observer)).default_with(lambda: self._white(space, observer))
                    value = float(fn(a, b))
                coords = a if isinstance(a, np.ndarray) else np.empty(0, dtype=np.float64)
                return ColorReceipt(tag="measure", coords=coords, space=space.science or space.aide or "", measures=frozendict({metric: value}))
            case ColorOp(tag="correct", correct=(measured, reference, method)):
                corrected = np.asarray(colour.colour_correction(measured, measured, reference, method=method.value), dtype=np.float64)
                return ColorReceipt(tag="correct", coords=corrected, notation=self._notate(ColorModel.SRGB.aide, corrected), space="sRGB", path=(method.value,))
            case _:
                assert_never(self.op)

    @staticmethod
    def _resolve(value: ColorSource, source: ColorModel, target: ColorModel, observer: Observer, adapt: AdaptMethod = AdaptMethod.BRADFORD) -> MetricInput:
        if target.spectral:
            return value  # CRI/CFI read the SpectralDistribution directly; no coordinate form exists to resolve into
        cmfs = colour.MSDS_CMFS[observer.value]
        match value:
            case float() as nm:
                return np.asarray(colour.convert(colour.wavelength_to_XYZ(nm, cmfs), "CIE XYZ", target.science), dtype=np.float64)
            case MultiSpectralDistributions():
                return np.asarray(colour.convert(colour.msds_to_XYZ(value, cmfs, colour.SDS_ILLUMINANTS[Illuminant.D65.value]), "CIE XYZ", target.science), dtype=np.float64)
            case _:
                return np.asarray(colour.convert(value, source.science, target.science, chromatic_adaptation_transform=adapt.value), dtype=np.float64)

    @staticmethod
    def _white(space: ColorModel, observer: Observer) -> Tristimulus:
        if space.spectral:
            return np.empty(0, dtype=np.float64)  # unary spectral metrics ignore the reference operand
        return np.asarray(Colorimetry._resolve(_WHITEPOINT[observer][Illuminant.D65], ColorModel.XYZ, space, observer), dtype=np.float64)

    @staticmethod
    def _notate(space_aide: str | None, coords: Tristimulus) -> tuple[str, ...]:
        if space_aide is None or coords.size == 0:  # appearance-correlate or measure outputs have no CSS form
            return ()
        rows = coords if coords.ndim == 2 else coords[np.newaxis, :]
        return tuple(Color(space_aide, list(row)).to_string() for row in rows)

    @staticmethod
    def _path(source: ColorModel, target: ColorModel) -> tuple[str, ...]:
        if source.science is None or target.science is None:
            return tuple(name for name in (source.aide, target.aide) if name is not None)
        captured: list[str] = []
        colour.describe_conversion_path(source.science, target.science, mode="Long", print_callable=captured.append)
        return (source.science, *captured, target.science)
```

## [03]-[RESEARCH]

- [COLOUR_SCIENCE]: the universal `convert` gateway, `describe_conversion_path` model-graph query, `RGB_COLOURSPACES` named-space registry, `delta_E` over `DELTA_E_METHODS`, `wavelength_to_XYZ` spectral-locus intake, `whiteness`/`yellowness`/`dominant_wavelength`/`complementary_wavelength` colorimetric indices, `colour_rendering_index`/`colour_fidelity_index` light-source-quality indices, `colour_correction` over the `Cheung 2004`/`Finlayson 2015`/`Vandermonde` solvers, `chromatic_adaptation` the standalone cross-white-point CAT, `CCS_ILLUMINANTS`/`xy_to_XYZ` the whitepoint source, `MSDS_CMFS`/`SDS_ILLUMINANTS` the observer cmfs and illuminant SPD registries, `msds_to_XYZ`/`MultiSpectralDistributions` the multi-spectral batch intake, `SpectralDistribution`, and `domain_range_scale` all verify against the folder `.api` catalogue for `colour-science` (cp315-core pure-Python, NumPy-backed), so the `Convert`/`Adapt`/`Measure`/`Correct` colour-science folds are settled fence code. The `Adapt` arm keys the standalone `chromatic_adaptation(XYZ, XYZ_w, XYZ_wr, method=...)` through `CamMethod` over `CHROMATIC_ADAPTATION_METHODS` (`Von Kries`, `Zhai 2018`, `Li 2025`, `CMCCAT2000`) — the catalogue confirms the method axis on the `chromatic_adaptation` row but enumerates only the disjoint registry shared with `Convert`'s `chromatic_adaptation_transform` kwarg, which keys `CHROMATIC_ADAPTATION_TRANSFORMS` (`Bradford`/`CAT02`/`CAT16`/`Von Kries`/`CMCCAT2000`/`Sharp`); the two registries share only `Von Kries`, so `AdaptMethod` (convert transforms) and `CamMethod` (standalone methods) stay distinct vocabularies and binding either to the wrong registry breaks the default path. The `_WHITEPOINT` nested table derives each `Illuminant` XYZ once per `Observer` from `CCS_ILLUMINANTS[observer][name]` through `xy_to_XYZ`, and the `Convert`/`Measure` spectral legs read `MSDS_CMFS[observer]` cmfs and `SDS_ILLUMINANTS[D65]` for `wavelength_to_XYZ`/`msds_to_XYZ` — the `Observer` 2-degree `CIE 1931 2 Degree Standard Observer` row catalogue-confirmed on `MSDS_CMFS`, the 10-degree `CIE 1964 10 Degree Standard Observer` row keying the same registries pending an observer-key reflection pass; the `CHROMATIC_ADAPTATION_METHODS` member set, the `CCS_ILLUMINANTS` `Illuminant` key set, the `whiteness`/`yellowness` `method` literal sets, and the `CHROMATIC_ADAPTATION_TRANSFORMS`/`describe_conversion_path` items below are catalogue-deepen seams since the `.api` lists only `CHROMATIC_ADAPTATION_METHODS` by axis. The universal `convert` subsumes the per-pair `sd_to_XYZ`/`XYZ_to_sRGB`/`RGB_to_XYZ` transforms; the `cctf_encoding`/`read_LUT`/`write_LUT`/`LUTSequence`/`write_image` tone-and-egress surface is NOT this page's — it is `graphic/color/managed#MANAGED`'s.
- [COLORAIDE]: the `everything.ColorAll` `Color` plus `convert`/`clone`/`fit(method=)`/`in_gamut`/`clip`, `filter(name, amount=)`, `delta_e(color, method=)`/`distance(color, space=)`/`contrast(color)`/`luminance()`/`cct(method=)`, `steps`/`discrete`/`interpolate(method=, hue=)`/`mix`/`weighted_mix`/`layer(blend=, operator=)`/`average`/`harmony`/`closest`, `blackbody(space, temp, method=)`, `coords()`, and `to_string()` the CSS-notation egress feeding `ColorReceipt.notation` spellings verify against the folder `.api` catalogue for `coloraide` (cp315 pure-Python, host-free), so the `Gamut`/`Filter`/`Palette`/`Compose`/`Temperature`/`Measure` ColorAide folds are settled fence code. The `Ramp.Discrete` arm folds `Color.discrete(...)`, which returns a reusable `Interpolator` itself callable `(point: float) -> Color`, sampled at `count` even points for a non-blended categorical scale (the catalogue confirms `discrete` returns the callable Interpolator on row `[15]`). The `_METRIC` difference rows fold the ColorAide `Color.delta_e` `ok`/`jz`/`hyab`/`itp`/`99o`/`cam02`/`cam16`/`hct` OKLab-perceptual methods (base `76`/`2000`/`94`/`cmc`/`hyab`/`itp`/`jz`/`ok`, `ColorAll` adds `99o`/`cam02`/`cam16`/`hct`) — the perceptual difference family colour-science's Lab-array `delta_E` has no equivalent for, so the two engines partition the one `_METRIC` measure surface rather than one engine owning all difference. The `Easing` progress vocabulary (`linear`/`ease`/`ease-in`/`ease-out`/`ease-in-out`) maps through the `_EASING` table to the `Color.steps`/`Color.discrete` `progress=` easing callables (`linear`/`ease`/`ease_in`/`ease_out`/`ease_in_out`), so a perceptual ramp shapes its step distribution through the package's own progress surface, never a hand-rolled curve. ColorAide spaces are lowercase-hyphenated registered names, disjoint from the colour-science model strings, so every ColorAide call reaches the space through `ColorModel.aide` and every colour-science call through `ColorModel.science` — the single `ModelNames(science, aide)` row carrying both spellings. The six `FitMethod` rows, the eleven `ColorFilter` rows, the eight base `delta_e` metrics, and the six `Interp` curves are catalogue-enumerated registered names; the `HueArc` arc strings (`shorter` default confirmed), the harmony names, and the `BlendMode`/`PorterDuff` operator strings are the open name-enumeration seams below; `everything.ColorAll` imports as `Color` at boundary scope per the manifest import policy.
- [MEASURE_POLICY] [RESOLVED]: the `_METRIC` `frozendict[Metric, MetricSpec]` binds each measure to the input `ColorModel` its engine demands and `Measure` resolves the `source`-spaced sample (and the `Option`-carried reference, defaulting to the payload `Observer` `D65` white when `Nothing`) into that space through the one `_resolve` kernel `Convert` shares under the payload `Observer` (the standard-observer axis `Convert`/`Adapt` carry) before applying — colour-science `delta_E` over `LAB`, `whiteness`/`yellowness` over `XYZ`, `dominant_wavelength`/`complementary_wavelength` over `XYY` (the `xy` pair sliced from the `xyY` coords), `colour_rendering_index`/`colour_fidelity_index` over a `SpectralDistribution` passed through untouched, and the ColorAide `delta_e`/`distance`/`contrast`/`luminance`/`cct`/`severity` plus the `chromaticity-x`/`chromaticity-y` reads over `SRGB`/`XYY` — replacing the prior uniform `Callable[[ColorSource, Tristimulus | None], float]` table that silently mis-fed `LAB`/`XYZ`/`XYY` rows sRGB coordinates. The `ColorReceipt` is the frozen-dataclass canonical owner carrying the `NDArray` `coords`, the `Color.to_string` `notation`, and the immutable `frozendict[Metric, float]` `measures`; its `wired()` derives the `ColorReceiptWire` `msgspec.Struct` with ravelled `coords`+`shape` for `data/tabular#WIRE`, since an `NDArray` field on a wire struct carries no msgspec codec. The `async_boundary` `catch=(ValueError, KeyError)` narrowing folds the colour-science/ColorAide domain raises onto the rail while an unexpected raise propagates as a defect. Close-condition: none — the metric policy, the `_resolve` kernel, the chromaticity rows, and the receipt/wire split are fence-local.
- [ADMISSION] [RESEARCH]: the `Amount`/`PaletteCount`/`Spacing`/`Kelvin`/`Wavelength` refined aliases carry a local `beartype.vale.Is[...]` predicate enforced by the `@beartype`-guarded `Convert`/`Filter`/`Palette`/`Temperature` factories — the two-tier admission `shapes.md` legislates, refusing an out-of-range amount, count, spacing, kelvin, or wavelength at construction so the dispatch interior is total over well-formed payloads. The `Is` predicates are self-contained (no catalogue dependency) and settled; a construction-time `BeartypeCallHintViolation` surfaces to the caller's admission boundary, distinct from the runtime-provider raise `async_boundary` maps inside `_compute`. Close-condition: none — the contract is fence-local.
- [TEMPERATURE] [RESEARCH]: the colour-science `colour.temperature.CCT_to_xy(CCT, method=...)`/`xy_to_CCT(xy, method=...)` call surface is settled fence code, and the ColorAide `Color.blackbody(space, temp, method=...)` swatch and `Color.cct(method=...)` read are settled against the catalogue CCT leg (the `ohno-2013`/`robertson-1968` Planckian methods named, so `Blackbody` is settled). The inverse arm reads `np.ravel(xy_to_CCT(...))[0]` so a method returning a `[CCT, Duv]` pair and one returning a scalar both narrow to the Kelvin. The one open colour-science leg is the illuminant `method` literal set — the catalogue row names a `method` axis without enumerating the four strings, so the `CctMethod` vocabulary (`{"CIE Illuminant D Series", "Kang 2002", "Hernandez 1999", "McCamy 1992"}`) stays a catalogue-deepen item until a `colour.temperature` method-literal reflection pass lands. Close-condition: `.api` catalogue enumerates the `CCT_to_xy`/`xy_to_CCT` four-method literal set.
- [CONVERSION_PATH] [RESEARCH]: the `Colorimetry._path` graph capture spells `describe_conversion_path(source, target, mode="Long", print_callable=captured.append)`, but the folder `.api` catalogue verifies only the two-argument `describe_conversion_path(source, target)` call without the `mode`/`print_callable` capture kwargs. The full signature is `describe_conversion_path(source, target, mode='Short', width=79, padding=3, print_callable=print, **kwargs) -> None` — `mode='Long'` is load-bearing: the default `'Short'` prints a truncated summary while `'Long'` yields the resolved model-graph the receipt `path` claims. The `mode`/`print_callable`-fed capture and the `(source, *captured, target)` path tuple stay a catalogue-deepen seam until a `describe_conversion_path` signature reflection lands; the `Convert`/`Gamut` receipt-`path` shape is settled. Close-condition: `.api` catalogue carries `describe_conversion_path` with `mode`/`print_callable`.
- [COMPOSE] [RESEARCH]: the `Color.layer(colors, blend=, operator=, space=, out_space=)` classmethod and `Color.weighted_mix(colors, weights=, space=, out_space=)` calls verify against the catalogue composition leg, so the `Compose` arm shape is settled; the W3C `BlendMode` (the sixteen `normal`/`multiply`/`screen`/`overlay`/.../`luminosity` separable-and-non-separable modes) and `PorterDuff` (the nine `source-over`/.../`xor` operators) name sets are not enumerated in the catalogue, so those `StrEnum` rows stay a catalogue-deepen seam. Close-condition: `.api` catalogue enumerates the `Color.layer` blend-mode and Porter-Duff operator name sets.
- [SPACES] [RESEARCH]: the `ColorModel` rows carry both engine spellings — `ICTCP` keys `science="ICtCp"` (confirmed by the catalogue `XYZ_to_ICtCp` named transform) and `aide="ictcp"`, while `DISPLAY_P3`/`REC2020`/`HSL`/`HWB` key `science=None` as ColorAide-only gamut/palette/compose/filter targets because the catalogue confirms their presence in the ColorAide base set but does not enumerate the exact registered space ids nor a colour-science convert-node name; those `aide` strings and a future `science` column stay a catalogue-deepen seam. Close-condition: `.api` catalogue enumerates the ColorAide registered space ids and the colour-science wide-gamut convert nodes.
- [HARMONY] [RESEARCH]: the `Harmony` `StrEnum` carries the ColorAide harmony-wheel names (`mono`/`complement`/`split`/`analogous`/`triad`/`square`/`rectangle`/`wheel`) as the `Color.harmony(name, ...)` payload, but the catalogue documents the call without enumerating the registered names. The row is `rectangle`, NOT `rectangular` (which raises `ValueError: color harmony 'rectangular' cannot be found`), and the singular `complement`/`triad`/`mono` forms not the `-ary`/`-ic` long forms. The `Harmony` row strings stay a catalogue-deepen seam until a `coloraide` harmony-name reflection pass lands; the `Palette` `Harmony` arm shape is settled. Close-condition: `.api` catalogue carries the registered harmony `name` strings.
