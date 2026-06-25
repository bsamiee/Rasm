# [PY_COMPUTE_API_PYWAVELETS]

`pywavelets` (dist `pywavelets`, import `pywt`) supplies the discrete and continuous wavelet-transform surface for the compute signal-processing rail: single- and multi-level 1D/2D/nD DWT and inverse, stationary (undecimated) and wavelet-packet decompositions, the continuous CWT, and a `Wavelet`/`ContinuousWavelet` family that exposes filter banks and scaling/wavelet functions. The package owner composes `dwt`/`idwt`, `wavedec`/`waverec`, `cwt`, and the `Wavelet` catalogue into the signal owner beside `scipy.signal`; it never re-implements the cascade filter-bank convolution PyWavelets already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pywavelets`
- package: `pywavelets`
- import: `import pywt` (dist name `pywavelets`, import name `pywt`)
- owner: `compute`
- rail: signal processing
- namespace: `pywt` (all public transforms/types/helpers at top level)
- installed: `1.9.0`; license MIT; manylinux/macOS `cp3x` ABI wheels (C/Cython extension over the FLINT-independent wavelet C kernels)
- gate: `[GATED]` `; python_version<'3.15'` — C/Cython extension ships no CPython 3.15 wheel; the wavelet-transform surface runs only on the companion interpreter band, never the cp315 core
- requires: `numpy`
- entry points: none (library only)
- capability: 1D/2D/nD discrete wavelet transform (single and multilevel), stationary/undecimated DWT (1D/2D/nD), multiresolution analysis (additive MRA), the fully-separable wavelet transform, wavelet-packet trees (1D/2D/nD), the continuous wavelet transform, soft/hard/garrote/firm coefficient thresholding, coefficient ravel/unravel between nested lists and flat arrays, a discrete/continuous wavelet catalogue with filter banks and `wavefun`, FIR filter-bank helpers, and signal-extension-mode control

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: wavelet, packet, and mode types
- rail: signal processing

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]      | [CAPABILITY]                                                                                                            |
| :-----: | :------------------------- | :----------------- | :---------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Wavelet`                  | discrete wavelet   | `Wavelet(name)` — `filter_bank`/`dec_lo`/`dec_hi`/`rec_lo`/`rec_hi`, `dec_len`/`rec_len`, `orthogonal`/`biorthogonal`, `vanishing_moments_psi`, `wavefun(level=8)` |
|  [02]   | `ContinuousWavelet`        | continuous wavelet | `ContinuousWavelet(name)` — CWT basis; `center_frequency`/`bandwidth_frequency`/`complex_cwt`/`lower_bound`/`upper_bound`, `wavefun(level=8, length=None)` |
|  [03]   | `DiscreteContinuousWavelet`| wavelet factory    | name-discriminating factory returning a `Wavelet` or `ContinuousWavelet` — the polymorphic constructor both type names delegate to |
|  [04]   | `WaveletPacket`            | 1D packet tree     | `WaveletPacket(data, wavelet, mode='symmetric', maxlevel=None, axis=-1)` — full/best-basis 1D packets; `get_level`/`get_leaf_nodes`/`reconstruct`/`walk` |
|  [05]   | `WaveletPacket2D`          | 2D packet tree     | 2D wavelet-packet tree (`LL`/`LH`/`HL`/`HH` quadrant nodes)                                                              |
|  [06]   | `WaveletPacketND`          | nD packet tree     | nD wavelet-packet tree (corner-string subnodes), `mode='smooth'` default                                                |
|  [07]   | `Node` / `Node2D` / `NodeND` | packet node      | per-level packet-tree node carrying coefficient data and child accessors                                                |
|  [08]   | `FswavedecnResult`         | FSWT result        | fully-separable transform result; `approx`/`coeffs`/`coeff_slices`/`detail_keys`/`wavelets`/`modes` accessors for `fswaverecn` |
|  [09]   | `Modes`                    | extension modes    | `symmetric`/`reflect`/`periodic`/`periodization`/`zero`/`constant`/`smooth`/`antisymmetric`/`antireflect`; `Modes.modes` lists them, `Modes.from_object` normalizes |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: discrete transforms (single and multilevel)
- rail: signal processing

`wavelet` accepts a `Wavelet` or its string name; `mode` selects the signal-extension policy; multilevel transforms return the approximation plus a list of detail coefficients, and the nD forms key details by a corner string (`'aa'`, `'ad'`, …).

| [INDEX] | [SURFACE]             | [CALL_SHAPE]                                                          | [CAPABILITY]                             |
| :-----: | :-------------------- | :-------------------------------------------------------------------- | :--------------------------------------- |
|  [01]   | `dwt`                 | `dwt(data, wavelet, mode='symmetric', axis=-1)` -> `(cA, cD)`         | single-level 1D DWT                      |
|  [02]   | `idwt`                | `idwt(cA, cD, wavelet, mode='symmetric', axis=-1)`                    | single-level 1D inverse                  |
|  [03]   | `wavedec`             | `wavedec(data, wavelet, mode='symmetric', level=None, axis=-1)`       | multilevel 1D DWT (`[cAn, cDn, …, cD1]`) |
|  [04]   | `waverec`             | `waverec(coeffs, wavelet, mode='symmetric', axis=-1)`                 | multilevel 1D inverse                    |
|  [05]   | `dwt2`/`idwt2`        | `dwt2(data, wavelet, mode, axes=(-2,-1))` -> `(cA, (cH, cV, cD))`     | single-level 2D DWT and inverse          |
|  [06]   | `wavedec2`/`waverec2` | `wavedec2(data, wavelet, mode, level, axes=(-2,-1))`                  | multilevel 2D DWT and inverse            |
|  [07]   | `wavedecn`/`waverecn` | `wavedecn(data, wavelet, mode, level, axes)` -> approx + corner dicts | multilevel nD DWT and inverse            |
|  [08]   | `dwtn`/`idwtn`        | `dwtn(data, wavelet, mode, axes)` -> corner-keyed dict                | single-level nD DWT and inverse          |

[ENTRYPOINT_SCOPE]: stationary, continuous, denoising, and catalogue
- rail: signal processing

| [INDEX] | [SURFACE]                           | [CALL_SHAPE]                                                                                              | [CAPABILITY]                                |
| :-----: | :---------------------------------- | :-------------------------------------------------------------------------------------------------------- | :------------------------------------------ |
|  [01]   | `swt`/`iswt`                        | `swt(data, wavelet, level=None, start_level=0, axis=-1, trim_approx=False, norm=False)`; `iswt(coeffs, wavelet, norm=False, axis=-1)` | stationary (undecimated) 1D DWT and inverse |
|  [02]   | `swt2`/`iswt2`                      | `swt2(data, wavelet, level, start_level=0, axes=(-2,-1), trim_approx=False, norm=False)`                  | stationary 2D DWT and inverse               |
|  [03]   | `swtn`/`iswtn`                      | `swtn(data, wavelet, level, start_level=0, axes=None, trim_approx=False, norm=False)`                     | stationary nD DWT and inverse               |
|  [04]   | `cwt`                               | `cwt(data, scales, wavelet, sampling_period=1.0, method='conv', axis=-1, *, precision=12)` -> `(coefs, freqs)` | continuous wavelet transform (`method='conv'` or `'fft'`) |
|  [05]   | `threshold`                         | `threshold(data, value, mode='soft', substitute=0)`                                                      | soft/hard/garrote/greater/less thresholding |
|  [06]   | `threshold_firm`                    | `threshold_firm(data, value_low, value_high)`                                                            | firm (semi-soft) two-knee thresholding      |
|  [07]   | `wavelist`                          | `wavelist(family=None, kind='all')` -> `list[str]`                                                       | enumerate the wavelet catalogue             |
|  [08]   | `families`                          | `families(short=True)` -> `list[str]`                                                                    | wavelet family names                        |
|  [09]   | `Wavelet.wavefun`/`ContinuousWavelet.wavefun` | `wavefun(level=8)` (discrete) / `wavefun(level=8, length=None)` (continuous)                    | scaling/wavelet functions on a grid (method on the wavelet object, not a module function) |
|  [10]   | `dwt_max_level`/`swt_max_level`     | `dwt_max_level(data_len, filter_len)`; `swt_max_level(input_len)` -> `int`                                | maximum useful decomposition level (decimated / stationary) |
|  [11]   | `dwt_coeff_len`                     | `dwt_coeff_len(data_len, filter_len, mode)` -> `int`                                                     | output coefficient length for one DWT level under a mode |
|  [12]   | `frequency2scale`/`scale2frequency`/`central_frequency` | `scale2frequency(wavelet, scale, precision=8)`; `central_frequency(wavelet, precision=8)`   | CWT scale↔frequency conversion and wavelet center frequency |
|  [13]   | `integrate_wavelet`                 | `integrate_wavelet(wavelet, precision=8)`                                                                | integrated wavelet/scaling function for CWT construction |

[ENTRYPOINT_SCOPE]: multiresolution analysis, fully-separable transform, coefficient packing, and filter helpers
- rail: signal processing
- the additive MRA returns components that SUM back to the input (unlike the DWT's hierarchical coefficient pyramid); the fully-separable transform applies a full 1D DWT along each axis in turn; coefficient packing flattens the nested `wavedecn` coefficient structure to a single array for solver/optimizer consumption and inverts it; filter helpers build orthogonal/quadrature-mirror FIR banks for a custom wavelet.

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                                                                       | [CAPABILITY]                                                                 |
| :-----: | :------------------------------ | :--------------------------------------------------------------------------------- | :--------------------------------------------------------------------------- |
|  [01]   | `mra`/`imra`                    | `mra(data, wavelet, level=None, axis=-1, transform='swt', mode='periodization')`; `imra(mra_coeffs)` | additive 1D multiresolution analysis whose components sum to the signal     |
|  [02]   | `mra2`/`imra2`                  | `mra2(data, wavelet, level=None, axes=(-2,-1), transform='swt2', mode='periodization')`             | additive 2D MRA                                                              |
|  [03]   | `mran`/`imran`                  | `mran(data, wavelet, level=None, axes=None, transform='swtn', mode='periodization')`                | additive nD MRA                                                              |
|  [04]   | `fswavedecn`/`fswaverecn`       | `fswavedecn(data, wavelet, mode='symmetric', levels=None, axes=None)` -> `FswavedecnResult`; `fswaverecn(result)` | fully-separable wavelet transform (independent per-axis level/wavelet/mode) |
|  [05]   | `coeffs_to_array`/`array_to_coeffs` | `coeffs_to_array(coeffs, padding=0, axes=None)` -> `(arr, coeff_slices)`; `array_to_coeffs(arr, coeff_slices, output_format='wavedecn')` | pack/unpack `wavedecn` coefficients to a single padded array and back        |
|  [06]   | `ravel_coeffs`/`unravel_coeffs` | `ravel_coeffs(coeffs, axes=None)` -> `(arr, coeff_slices, coeff_shapes)`; `unravel_coeffs(arr, coeff_slices, coeff_shapes, output_format='wavedecn')` | flatten coefficients to a 1D vector and reconstruct (optimizer/solver input) |
|  [07]   | `wavedecn_shapes`/`wavedecn_size` | `wavedecn_shapes(shape, wavelet, mode='symmetric', level=None, axes=None)`; `wavedecn_size(shapes)` | predict per-level coefficient shapes and total coefficient count without transforming |
|  [08]   | `downcoef`/`upcoef`             | `downcoef(part, data, wavelet, mode='symmetric', level=1)`; `upcoef(part, coeffs, wavelet, level=1, take=0)` | single approximation/detail (`part='a'`/`'d'`) coefficient extraction and partial reconstruction |
|  [09]   | `orthfilt`/`qmf`/`pad`          | `orthfilt(scaling_filter)`; `qmf(filt)`; `pad(x, pad_widths, mode)`                 | build an orthogonal FIR filter bank from a scaling filter, quadrature-mirror flip, and mode-aware array padding |

## [04]-[IMPLEMENTATION_LAW]

[SIGNAL_WAVELET]:
- import: `import pywt` at boundary scope only; module-level import is banned by the manifest import policy. The dist name is `pywavelets`; the import name is `pywt`.
- transform axis: one DWT family spans dimensionality — `dwt`/`wavedec` (1D), `dwt2`/`wavedec2` (2D), `dwtn`/`wavedecn` (nD) — and decomposition depth is the `level` row, never a hand-iterated filter cascade; `idwt`/`waverec*` are the exact inverses.
- wavelet axis: a `Wavelet` (or its string name) carries the filter bank; the catalogue is enumerated by `wavelist`/`families`, never hardcoded coefficient arrays; `ContinuousWavelet` carries the CWT basis.
- mode axis: `mode` selects the signal-extension policy (`symmetric`/`periodization`/`reflect`/…) as one row on every transform; boundary handling is a parameter, never a separate padded-then-transform path.
- decimation axis: `swt`/`swt2`/`swtn` are the stationary (shift-invariant, undecimated) variant; the decimated `dwt` family and the undecimated `swt` family are decomposition rows, never duplicated transform engines.
- separability axis: `wavedecn` applies one shared wavelet/mode/level across all axes; `fswavedecn` is the fully-separable transform that applies an independent full 1D DWT (own level/wavelet/mode) per axis, returning a `FswavedecnResult` inverted only by `fswaverecn` — pick the separable transform when axes have different sampling characteristics, never hand-iterate per-axis `wavedec`.
- additive axis: `mra`/`mra2`/`mran` are the additive multiresolution analysis whose components SUM to the input (`sum(mra(x)) ≈ x`); this is the analysis-of-variance decomposition, distinct from the `wavedec` coefficient pyramid, and is inverted by `imra*` — never reconstruct an additive band sum from `waverec`.
- continuous axis: `cwt(data, scales, wavelet, *, precision=12)` returns scale-frequency coefficients with `method='conv'` (direct) or `'fft'` (frequency-domain) and `scale2frequency`/`frequency2scale`/`central_frequency` for the scale↔frequency mapping.
- denoising axis: `threshold` applies the soft/hard/garrote/greater/less rule and `threshold_firm` the two-knee firm rule to detail coefficients between `wavedec` and `waverec`; thresholding is a coefficient row, never a separate denoiser type.
- packing axis: `ravel_coeffs`/`coeffs_to_array` flatten the nested `wavedecn` coefficient structure to a single array (with `coeff_slices`/`coeff_shapes` metadata) and `unravel_coeffs`/`array_to_coeffs` invert it — this is the bridge that lets a solver/optimizer treat the full coefficient set as one flat parameter vector; `wavedecn_shapes`/`wavedecn_size` predict the packed layout without transforming.
- evidence: each transform captures wavelet name, level, mode, axis, and coefficient shapes as a signal receipt; perfect reconstruction (`waverec(wavedec(x)) ≈ x`, `sum(mra(x)) ≈ x`) is the verification invariant.
- boundary: PyWavelets owns wavelet decomposition/reconstruction and the CWT; `scipy.signal` owns FIR/IIR filtering and spectral analysis beside it; the graduation rail hands offline coefficients across the wire; live UI stays outside this package.

[SIGNAL_STACKING]:
- scipy.signal ↔ pywt: `scipy.signal` owns Fourier/short-time-Fourier spectral analysis and FIR/IIR filtering; PyWavelets owns the time-scale (wavelet) decomposition. The signal owner composes both: `scipy.signal.spectrogram` for the stationary-spectrum view and `pywt.cwt` for the time-scale view of the same trace, sharing the `sampling_period`/`fs` so the frequency axes are comparable.
- optimizer rail (jax/optax/optimistix) ↔ pywt: a wavelet-domain regularized inverse problem ravels the `wavedecn` coefficients with `ravel_coeffs` into a single 1D vector, optimizes the flat vector under an L1/sparsity penalty through the JAX/optax owner, then `unravel_coeffs` + `waverecn` reconstructs the spatial field — the packing helpers, not a hand-built index map, own the flat↔nested bijection.
- arviz/posterior rail ↔ pywt: when a posterior trace is wavelet-denoised, the `threshold`/`threshold_firm` rule is applied to `wavedec` detail coefficients and the reconstructed trace is handed back to `arviz` for diagnostics; the threshold value is captured in the receipt beside the wavelet/level/mode.

[RAIL_LAW]:
- Package: `pywavelets`
- Owns: 1D/2D/nD discrete (decimated and stationary) wavelet transforms, additive multiresolution analysis, the fully-separable wavelet transform, wavelet-packet trees (1D/2D/nD), the continuous wavelet transform, soft/hard/garrote/greater/less/firm coefficient thresholding, coefficient ravel/unravel between nested lists and flat arrays, the wavelet catalogue with filter banks and `wavefun`, and FIR filter-bank helpers
- Accept: `dwt`/`wavedec` family with `mode`/`level`/`axis` rows, `swt`/`swtn` for shift-invariant decomposition, `mra`/`mran` for additive analysis-of-variance bands, `fswavedecn` for per-axis-independent separable decomposition, `cwt` for continuous analysis, `Wavelet`/`ContinuousWavelet`/`wavelist`/`wavefun` for the catalogue, `threshold`/`threshold_firm` for coefficient denoising, `ravel_coeffs`/`coeffs_to_array` for the flat↔nested coefficient bijection feeding an optimizer
- Reject: wrapper-renames of `dwt`/`wavedec`; a hand-rolled cascade filter bank where the multilevel transform applies; hardcoded wavelet coefficients where the catalogue resolves them; a hand-built coefficient index map where `ravel_coeffs`/`unravel_coeffs` own the bijection; reconstructing an additive band sum from `waverec` where `mra`/`imra` own additive analysis; a parallel transform engine per dimensionality or per decimation mode
- License: MIT; gated `; python_version<'3.15'` (C/Cython extension, no cp315 wheel)
