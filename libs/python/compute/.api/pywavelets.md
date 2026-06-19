# [PY_COMPUTE_API_PYWAVELETS]

`pywavelets` (dist `pywavelets`, import `pywt`) supplies the discrete and continuous wavelet-transform surface for the compute signal-processing rail: single- and multi-level 1D/2D/nD DWT and inverse, stationary (undecimated) and wavelet-packet decompositions, the continuous CWT, and a `Wavelet`/`ContinuousWavelet` family that exposes filter banks and scaling/wavelet functions. The package owner composes `dwt`/`idwt`, `wavedec`/`waverec`, `cwt`, and the `Wavelet` catalogue into the signal owner beside `scipy.signal`; it never re-implements the cascade filter-bank convolution PyWavelets already owns.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pywavelets`
- package: `pywavelets`
- import: `import pywt` (dist name `pywavelets`, import name `pywt`)
- owner: `compute`
- rail: signal processing
- installed: companion-band `python_version<'3.15'` (C/Cython extension; no CPython 3.15 wheel yet) — RESEARCH-capture-pending-on-uv-sync; the member surface below is authored from official documentation and reflection-verifies on uv sync into the companion interpreter band
- entry points: none (library only)
- capability: 1D/2D/nD discrete wavelet transform (single and multilevel), stationary/undecimated DWT, wavelet-packet trees, the continuous wavelet transform, threshold-based denoising, a discrete/continuous wavelet catalogue with filter banks and `wavefun`, and signal-extension-mode control

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: wavelet, packet, and mode types
- rail: signal processing

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]      | [CAPABILITY]                                                                |
| :-----: | :------------------ | :----------------- | :------------------------------------------------------------------------- |
|   [1]   | `Wavelet`           | discrete wavelet   | `Wavelet(name)` — filter bank (`dec_lo`/`dec_hi`/`rec_lo`/`rec_hi`), `wavefun` |
|   [2]   | `ContinuousWavelet` | continuous wavelet | `ContinuousWavelet(name)` — CWT basis with `wavefun`                        |
|   [3]   | `WaveletPacket`     | 1D packet tree     | `WaveletPacket(data, wavelet, mode, maxlevel)` — full/best-basis 1D packets |
|   [4]   | `WaveletPacket2D`   | 2D packet tree     | 2D wavelet-packet decomposition tree                                       |
|   [5]   | `Modes`             | extension modes    | `symmetric`/`reflect`/`periodic`/`periodization`/`zero`/`constant`/`smooth` |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: discrete transforms (single and multilevel)
- rail: signal processing

`wavelet` accepts a `Wavelet` or its string name; `mode` selects the signal-extension policy; multilevel transforms return the approximation plus a list of detail coefficients, and the nD forms key details by a corner string (`'aa'`, `'ad'`, …).

| [INDEX] | [SURFACE]      | [CALL_SHAPE]                                                                 | [CAPABILITY]                              |
| :-----: | :------------- | :-------------------------------------------------------------------------- | :---------------------------------------- |
|   [1]   | `dwt`          | `dwt(data, wavelet, mode='symmetric', axis=-1)` -> `(cA, cD)`                | single-level 1D DWT                       |
|   [2]   | `idwt`         | `idwt(cA, cD, wavelet, mode='symmetric', axis=-1)`                           | single-level 1D inverse                   |
|   [3]   | `wavedec`      | `wavedec(data, wavelet, mode='symmetric', level=None, axis=-1)`             | multilevel 1D DWT (`[cAn, cDn, …, cD1]`)  |
|   [4]   | `waverec`      | `waverec(coeffs, wavelet, mode='symmetric', axis=-1)`                        | multilevel 1D inverse                     |
|   [5]   | `dwt2`/`idwt2` | `dwt2(data, wavelet, mode, axes=(-2,-1))` -> `(cA, (cH, cV, cD))`            | single-level 2D DWT and inverse           |
|   [6]   | `wavedec2`/`waverec2` | `wavedec2(data, wavelet, mode, level, axes=(-2,-1))`                  | multilevel 2D DWT and inverse             |
|   [7]   | `wavedecn`/`waverecn` | `wavedecn(data, wavelet, mode, level, axes)` -> approx + corner dicts | multilevel nD DWT and inverse             |
|   [8]   | `dwtn`/`idwtn` | `dwtn(data, wavelet, mode, axes)` -> corner-keyed dict                       | single-level nD DWT and inverse           |

[ENTRYPOINT_SCOPE]: stationary, continuous, denoising, and catalogue
- rail: signal processing

| [INDEX] | [SURFACE]            | [CALL_SHAPE]                                                              | [CAPABILITY]                              |
| :-----: | :------------------- | :----------------------------------------------------------------------- | :---------------------------------------- |
|   [1]   | `swt`/`iswt`         | `swt(data, wavelet, level=None, start_level=0, axis=-1, trim_approx=False, norm=False)` | stationary (undecimated) DWT and inverse  |
|   [2]   | `swt2`/`iswt2`       | `swt2(data, wavelet, level, ...)`                                        | stationary 2D DWT and inverse             |
|   [3]   | `cwt`                | `cwt(data, scales, wavelet, sampling_period=1.0, method='conv', axis=-1)` -> `(coefs, freqs)` | continuous wavelet transform |
|   [4]   | `threshold`          | `threshold(data, value, mode='soft', substitute=0)`                      | soft/hard/garrote/greater/less thresholding|
|   [5]   | `wavelist`           | `wavelist(family=None, kind='all')` -> `list[str]`                       | enumerate the wavelet catalogue           |
|   [6]   | `families`           | `families(short=True)` -> `list[str]`                                     | wavelet family names                      |
|   [7]   | `wavefun`            | `Wavelet.wavefun(level=8)`                                                | scaling/wavelet functions on a grid       |
|   [8]   | `dwt_max_level`      | `dwt_max_level(data_len, filter_len)` -> `int`                           | maximum useful decomposition level        |
|   [9]   | `frequency2scale`/`scale2frequency` | `scale2frequency(wavelet, scale, precision=8)`            | CWT scale↔frequency conversion            |

## [4]-[IMPLEMENTATION_LAW]

[SIGNAL_WAVELET]:
- import: `import pywt` at boundary scope only; module-level import is banned by the manifest import policy. The dist name is `pywavelets`; the import name is `pywt`.
- transform axis: one DWT family spans dimensionality — `dwt`/`wavedec` (1D), `dwt2`/`wavedec2` (2D), `dwtn`/`wavedecn` (nD) — and decomposition depth is the `level` row, never a hand-iterated filter cascade; `idwt`/`waverec*` are the exact inverses.
- wavelet axis: a `Wavelet` (or its string name) carries the filter bank; the catalogue is enumerated by `wavelist`/`families`, never hardcoded coefficient arrays; `ContinuousWavelet` carries the CWT basis.
- mode axis: `mode` selects the signal-extension policy (`symmetric`/`periodization`/`reflect`/…) as one row on every transform; boundary handling is a parameter, never a separate padded-then-transform path.
- decimation axis: `swt`/`swt2` are the stationary (shift-invariant, undecimated) variant; the decimated `dwt` family and the undecimated `swt` family are decomposition rows, never duplicated transform engines.
- continuous axis: `cwt(data, scales, wavelet)` returns scale-frequency coefficients with `scale2frequency`/`frequency2scale` for the scale↔frequency mapping.
- denoising axis: `threshold` applies the soft/hard/garrote rule to detail coefficients between `wavedec` and `waverec`; thresholding is a coefficient row, never a separate denoiser type.
- evidence: each transform captures wavelet name, level, mode, axis, and coefficient shapes as a signal receipt; perfect reconstruction (`waverec(wavedec(x)) ≈ x`) is the verification invariant.
- boundary: PyWavelets owns wavelet decomposition/reconstruction and the CWT; `scipy.signal` owns FIR/IIR filtering and spectral analysis beside it; the graduation rail hands offline coefficients across the wire; live UI stays outside this package.

[RAIL_LAW]:
- Package: `pywavelets`
- Owns: 1D/2D/nD discrete (decimated and stationary) wavelet transforms, wavelet-packet trees, the continuous wavelet transform, coefficient thresholding, and the wavelet catalogue with filter banks
- Accept: `dwt`/`wavedec` family with `mode`/`level`/`axis` rows, `swt` for shift-invariant decomposition, `cwt` for continuous analysis, `Wavelet`/`wavelist` for the catalogue, `threshold` for coefficient denoising
- Reject: wrapper-renames of `dwt`/`wavedec`; a hand-rolled cascade filter bank where the multilevel transform applies; hardcoded wavelet coefficients where the catalogue resolves them; a parallel transform engine per dimensionality or per decimation mode
