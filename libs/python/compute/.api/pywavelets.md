# [PY_COMPUTE_API_PYWAVELETS]

`pywavelets` (import `pywt`) owns the discrete and continuous wavelet-transform surface for the compute signal rail — single/multilevel 1D/2D/nD DWT and inverse, stationary and wavelet-packet decompositions, additive MRA, the fully-separable transform, and the CWT over a `Wavelet`/`ContinuousWavelet` catalogue. `compute`'s signal owner composes it beside `scipy.signal`, which holds Fourier/FIR-IIR spectral analysis; the cascade filter-bank convolution stays PyWavelets', never re-implemented.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pywavelets`
- package: `pywavelets`
- module: `pywt` (dist `pywavelets`; all public transforms, types, and helpers at top level)
- rail: signal processing

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: wavelet, packet, and mode types
- Discrete/continuous wavelets construct by name — `Wavelet(name)`, `ContinuousWavelet(name)`; packet trees as `WaveletPacket*(data, wavelet, mode=…, maxlevel=None, axis)`; `wavefun` returns scaling/wavelet functions on a grid.

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]      | [CAPABILITY]                                                                     |
| :-----: | :--------------------------- | :----------------- | :------------------------------------------------------------------------------- |
|  [01]   | `Wavelet`                    | discrete wavelet   | orthogonal/biorthogonal filter bank + `wavefun`; fields in fence                 |
|  [02]   | `ContinuousWavelet`          | continuous wavelet | CWT basis with frequency/bandwidth/bound fields + `wavefun`; fields in fence     |
|  [03]   | `DiscreteContinuousWavelet`  | wavelet factory    | name-discriminating factory returning a `Wavelet` or `ContinuousWavelet`         |
|  [04]   | `WaveletPacket`              | 1D packet tree     | full/best-basis 1D packets; `get_level`/`get_leaf_nodes`/`reconstruct`/`walk`    |
|  [05]   | `WaveletPacket2D`            | 2D packet tree     | 2D packet tree (`LL`/`LH`/`HL`/`HH` quadrant nodes)                              |
|  [06]   | `WaveletPacketND`            | nD packet tree     | nD packet tree (corner-string subnodes), `mode='smooth'` default                 |
|  [07]   | `Node` / `Node2D` / `NodeND` | packet node        | per-level tree node carrying coefficient data and child accessors                |
|  [08]   | `FswavedecnResult`           | FSWT result        | `approx`/`coeffs`/`coeff_slices`/`detail_keys`/`wavelets`/`modes` → `fswaverecn` |
|  [09]   | `Modes`                      | extension modes    | signal-extension vocabulary; `Modes.from_object` normalizes                      |

[Wavelet]: `filter_bank` `dec_lo` `dec_hi` `rec_lo` `rec_hi` `dec_len` `rec_len` `vanishing_moments_psi` `orthogonal` `biorthogonal` `wavefun(level=8)`
[ContinuousWavelet]: `center_frequency` `bandwidth_frequency` `lower_bound` `upper_bound` `complex_cwt` `wavefun(level=8, length=None)`
[Modes]: `symmetric` `reflect` `periodic` `periodization` `zero` `constant` `smooth` `antisymmetric` `antireflect`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: discrete transforms (single and multilevel)
- `wavelet` takes a `Wavelet` or its name; multilevel forms return the approximation and a detail list, and nD forms key details by corner string (`'aa'`, `'ad'`, …).

| [INDEX] | [SURFACE]                                                          | [SHAPE] | [CAPABILITY]                                 |
| :-----: | :----------------------------------------------------------------- | :------ | :------------------------------------------- |
|  [01]   | `dwt(data, wavelet, mode='symmetric', axis=-1) -> (cA, cD)`        | static  | single-level 1D DWT                          |
|  [02]   | `idwt(cA, cD, wavelet, mode='symmetric', axis=-1)`                 | static  | single-level 1D inverse                      |
|  [03]   | `wavedec(data, wavelet, mode='symmetric', level=None, axis=-1)`    | static  | multilevel 1D DWT (`[cAn, cDn, …, cD1]`)     |
|  [04]   | `waverec(coeffs, wavelet, mode='symmetric', axis=-1)`              | static  | multilevel 1D inverse                        |
|  [05]   | `dwt2/idwt2(data, wavelet, mode, axes=(-2,-1)) -> (cA,(cH,cV,cD))` | static  | single-level 2D DWT and inverse              |
|  [06]   | `wavedec2/waverec2(data, wavelet, mode, level, axes=(-2,-1))`      | static  | multilevel 2D DWT and inverse                |
|  [07]   | `wavedecn/waverecn(data, wavelet, mode, level, axes)`              | static  | multilevel nD DWT and inverse (corner dicts) |
|  [08]   | `dwtn/idwtn(data, wavelet, mode, axes) -> corner-keyed dict`       | static  | single-level nD DWT and inverse              |

[ENTRYPOINT_SCOPE]: stationary, continuous, denoising, and catalogue
- Stationary transforms share `(data, wavelet, level=None, start_level=0, trim_approx=False, norm=False)` over `axis`/`axes`; inverses take `(coeffs, wavelet, norm=False, axis=-1)`. `cwt(data, scales, wavelet, sampling_period=1.0, method='conv'|'fft', *, precision=12) -> (coefs, freqs)`.

| [INDEX] | [SURFACE]                                            | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :--------------------------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `swt(data, wavelet, …, axis=-1)` / `iswt(coeffs, …)` | static   | stationary (undecimated) 1D DWT + inverse   |
|  [02]   | `swt2(…, axes=(-2,-1))` / `iswt2(…)`                 | static   | stationary 2D DWT + inverse                 |
|  [03]   | `swtn(…, axes=None)` / `iswtn(…)`                    | static   | stationary nD DWT + inverse                 |
|  [04]   | `cwt(data, scales, wavelet, …) -> (coefs, freqs)`    | static   | continuous transform                        |
|  [05]   | `threshold(data, value, mode='soft', substitute=0)`  | static   | soft/hard/garrote/greater/less thresholding |
|  [06]   | `threshold_firm(data, value_low, value_high)`        | static   | firm (semi-soft) two-knee thresholding      |
|  [07]   | `wavelist(family=None, kind='all') -> list[str]`     | static   | enumerate the wavelet catalogue             |
|  [08]   | `families(short=True) -> list[str]`                  | static   | wavelet family names                        |
|  [09]   | `.wavefun(level=8, length=None)`                     | instance | scaling/wavelet functions (object method)   |
|  [10]   | `dwt_max_level(data_len, filter_len) -> int`         | static   | max useful decimated decomposition level    |
|  [11]   | `swt_max_level(input_len) -> int`                    | static   | max useful stationary decomposition level   |
|  [12]   | `dwt_coeff_len(data_len, filter_len, mode) -> int`   | static   | one-level DWT output coefficient length     |
|  [13]   | `scale2frequency(wavelet, scale, precision=8)`       | static   | CWT scale->frequency conversion             |
|  [14]   | `frequency2scale(wavelet, freq, precision=8)`        | static   | CWT frequency->scale conversion             |
|  [15]   | `central_frequency(wavelet, precision=8)`            | static   | wavelet center frequency                    |
|  [16]   | `integrate_wavelet(wavelet, precision=8)`            | static   | integrated wavelet/scaling function for CWT |

[ENTRYPOINT_SCOPE]: multiresolution analysis, fully-separable transform, coefficient packing, and filter helpers
- Additive MRA shares `(data, wavelet, level=None, transform=…, mode='periodization')` over `axis`/`axes`, inverted by `imra*`; `coeffs_to_array`/`ravel_coeffs` flatten the nested `wavedecn` structure with `coeff_slices`/`coeff_shapes` metadata for solver/optimizer consumption.

| [INDEX] | [SURFACE]                                                                   | [SHAPE] | [CAPABILITY]                                     |
| :-----: | :-------------------------------------------------------------------------- | :------ | :----------------------------------------------- |
|  [01]   | `mra(…, axis=-1, transform='swt')` / `imra(mra_coeffs)`                     | static  | additive 1D MRA, components sum to input         |
|  [02]   | `mra2(…, axes=(-2,-1), transform='swt2')` / `imra2(…)`                      | static  | additive 2D MRA                                  |
|  [03]   | `mran(…, axes=None, transform='swtn')` / `imran(…)`                         | static  | additive nD MRA                                  |
|  [04]   | `fswavedecn(data, wavelet, mode='symmetric', levels=None, axes=None)`       | static  | fully-separable; per-axis level/wavelet/mode     |
|  [05]   | `fswaverecn(result)`                                                        | static  | inverse fully-separable transform                |
|  [06]   | `coeffs_to_array(coeffs, padding=0, axes=None) -> (arr, coeff_slices)`      | static  | pack `wavedecn` coeffs to one padded array       |
|  [07]   | `array_to_coeffs(arr, coeff_slices, output_format='wavedecn')`              | static  | unpack padded array to nested coeffs             |
|  [08]   | `ravel_coeffs(coeffs, axes=None) -> (arr, coeff_slices, coeff_shapes)`      | static  | flatten coeffs to a 1D vector (optimizer input)  |
|  [09]   | `unravel_coeffs(arr, coeff_slices, coeff_shapes, output_format='wavedecn')` | static  | reconstruct nested coeffs from the vector        |
|  [10]   | `wavedecn_shapes(shape, wavelet, mode='symmetric', level=None, axes=None)`  | static  | per-level coeff shapes without transforming      |
|  [11]   | `wavedecn_size(shapes)`                                                     | static  | total coeff count from shapes                    |
|  [12]   | `downcoef(part, data, wavelet, mode='symmetric', level=1)`                  | static  | single `part='a'`/`'d'` coefficient extraction   |
|  [13]   | `upcoef(part, coeffs, wavelet, level=1, take=0)`                            | static  | partial reconstruction from one coefficient part |
|  [14]   | `orthfilt(scaling_filter)`                                                  | static  | build orthogonal FIR filter bank from a filter   |
|  [15]   | `qmf(filt)`                                                                 | static  | quadrature-mirror filter flip                    |
|  [16]   | `pad(x, pad_widths, mode)`                                                  | static  | mode-aware array padding                         |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One DWT family spans dimensionality by axis rank and depth by `level`; `idwt`/`waverec*` are exact inverses, so perfect reconstruction (`waverec(wavedec(x)) ≈ x`) is the verification invariant, and each transform records wavelet, level, mode, axis, and coefficient shapes as a signal receipt — never a hand-iterated filter cascade.
- `mode` (signal-extension policy) and `wavelet` (filter bank, resolved by name through `wavelist`/`families`) are transform parameters, never a separate padded-then-transform path or hardcoded coefficient array.
- Decimated `dwt`, undecimated `swt`, additive `mra` (components SUM to input, `sum(mra(x)) ≈ x`, inverted by `imra*`), and per-axis-independent `fswavedecn` (returning a `FswavedecnResult` inverted only by `fswaverecn`) are decomposition rows on one surface, never parallel transform engines.
- `cwt` returns scale-frequency coefficients (`method='conv'|'fft'`) with `scale2frequency`/`frequency2scale`/`central_frequency` owning the scale↔frequency map; `threshold`/`threshold_firm` denoise detail coefficients between decomposition and reconstruction.
- `ravel_coeffs`/`coeffs_to_array` own the flat↔nested coefficient bijection with `coeff_slices`/`coeff_shapes`, inverted by `unravel_coeffs`/`array_to_coeffs`; `wavedecn_shapes`/`wavedecn_size` predict the packed layout without transforming.

[STACKING]:
- `scipy`(`.api/scipy.md`): `scipy.signal` owns Fourier/short-time-Fourier spectral analysis and FIR/IIR filtering; the signal owner pairs `scipy.signal.spectrogram` (stationary spectrum) with `pywt.cwt` (time-scale) over a shared `sampling_period`/`fs` so the frequency axes align.
- `jax`(`.api/jax.md`)/`optax`(`.api/optax.md`): a wavelet-domain regularized inverse ravels the `wavedecn` coefficients to one 1D vector, optimizes under an L1/sparsity penalty through the JAX/optax owner, then `unravel_coeffs`+`waverecn` reconstructs the spatial field — the packing helpers own the flat↔nested map, never a hand-built index.
- `arviz`(`.api/arviz.md`): a wavelet-denoised posterior trace applies `threshold`/`threshold_firm` to `wavedec` detail coefficients, the reconstructed trace returning to `arviz` diagnostics with the threshold captured beside wavelet/level/mode in the receipt.
- within-lib: the `compute` signal owner folds `dwt`/`wavedec`, `swt`, `cwt`, and the `Wavelet` catalogue into one denoise pass, thresholding between `wavedec` and `waverec` beside `scipy.signal`.

[LOCAL_ADMISSION]:
- `pywavelets` admits the `dwt`/`wavedec`, `swt`, `mra`, `fswavedecn`, `cwt`, catalogue, thresholding, and coefficient-packing surfaces at boundary scope; a live fence reaching a wider member binds it under a named signal consumer.

[RAIL_LAW]:
- Package: `pywavelets`
- Owns: discrete (decimated and stationary), additive-MRA, fully-separable, and continuous wavelet transforms in 1D/2D/nD, wavelet-packet trees, coefficient thresholding, the flat↔nested coefficient bijection, and the wavelet catalogue with filter banks and `wavefun`
- Accept: the `dwt`/`wavedec` family over `mode`/`level`/`axis` rows, `swt*` for shift-invariance, `mra*` for additive bands, `fswavedecn` for per-axis-independent decomposition, `cwt` for continuous analysis, `threshold`/`threshold_firm` for denoising, `ravel_coeffs`/`coeffs_to_array` feeding an optimizer
- Reject: a wrapper-rename of `dwt`/`wavedec`; a hand-rolled cascade filter bank, hardcoded wavelet coefficients, or a hand-built coefficient index map; reconstructing an additive band sum from `waverec` where `mra`/`imra` own it; a parallel engine per dimensionality or decimation mode
