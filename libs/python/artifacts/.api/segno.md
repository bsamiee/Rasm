# [PY_ARTIFACTS_API_SEGNO]

`segno` supplies the pure-Python QR and Micro-QR code surface for the artifacts imaging rail: a `make` factory family that encodes content into a `QRCode`/`QRCodeSequence`, plus a multi-format `save` path that serializes to SVG/PNG/PDF/EPS/text and other formats with no raster dependency. The package owner composes `make`, `QRCode`, and `save` into the `PreviewOp.QR` path; it removes the `qrcode` Pillow leak because every serializer is in-package, and it never re-implements the Reed-Solomon QR encoding segno already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `segno`
- package: `segno`
- import: `segno`
- owner: `artifacts`
- rail: imaging
- installed: `1.6.6` reflected via `python -c "import segno"` on cp315
- entry points: console script `segno` (CLI); library use is import-only
- capability: QR (versions 1-40) and Micro-QR (M1-M4) symbol generation, error-correction L/M/Q/H, boolean module matrix, structured-append sequences, and dependency-free SVG/PNG/PDF/EPS/PAM/PPM/LaTeX/XBM/XPM/ANSI/text serialization

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: symbol and sequence roots
- rail: imaging

`make` returns a `QRCode`; `make_sequence` returns a `QRCodeSequence` for structured-append spanning multiple symbols. `DataOverflowError` is raised when content exceeds the largest admissible version for the requested error level.

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]   | [RAIL]                                     |
| :-----: | :------------------ | :-------------- | :----------------------------------------- |
|  [01]   | `QRCode`            | symbol          | one QR or Micro-QR symbol with serializers |
|  [02]   | `QRCodeSequence`    | symbol sequence | structured-append multi-symbol sequence    |
|  [03]   | `DataOverflowError` | error           | content-exceeds-capacity failure           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: factory functions
- rail: imaging

The factory rows share `error`, `version`, `mode`, `mask`, `encoding`, and `boost_error` policy; `make` auto-selects QR or Micro-QR unless `micro` forces the kind.

| [INDEX] | [SURFACE]       | [CALL_SHAPE]                                                                                                                 | [CAPABILITY]                             |
| :-----: | :-------------- | :--------------------------------------------------------------------------------------------------------------------------- | :--------------------------------------- |
|  [01]   | `make`          | `make(content, error=None, version=None, mode=None, mask=None, encoding=None, eci=False, micro=None, boost_error=True)`      | encode content to a `QRCode` (auto kind) |
|  [02]   | `make_qr`       | `make_qr(content, error=None, version=None, mode=None, mask=None, encoding=None, eci=False, boost_error=True)`               | force a full QR symbol                   |
|  [03]   | `make_micro`    | `make_micro(content, error=None, version=None, mode=None, mask=None, encoding=None, boost_error=True)`                       | force a Micro-QR symbol                  |
|  [04]   | `make_sequence` | `make_sequence(content, error=None, version=None, mode=None, mask=None, encoding=None, boost_error=True, symbol_count=None)` | structured-append `QRCodeSequence`       |

[ENTRYPOINT_SCOPE]: `QRCode` render and inspect
- rail: imaging

`save` selects the serializer by file extension or explicit `kind`; serializer kwargs (`scale`, `border`, `dark`, `light`) flow through `**kw`. Read properties carry the symbol metadata the receipt records.

| [INDEX] | [SURFACE]             | [CALL_SHAPE]                                                                                     | [CAPABILITY]                                  |
| :-----: | :-------------------- | :----------------------------------------------------------------------------------------------- | :-------------------------------------------- |
|  [01]   | `QRCode.save`         | `save(out, kind=None, **kw)`                                                                     | serialize to SVG/PNG/PDF/EPS/text (by `kind`) |
|  [02]   | `QRCode.svg_inline`   | `svg_inline(**kw)` -> `str`                                                                      | inline SVG markup string                      |
|  [03]   | `QRCode.svg_data_uri` | `svg_data_uri(xmldecl=False, encode_minimal=False, omit_charset=False, nl=False, **kw)` -> `str` | SVG `data:` URI                               |
|  [04]   | `QRCode.png_data_uri` | `png_data_uri(**kw)` -> `str`                                                                    | PNG `data:` URI                               |
|  [05]   | `QRCode.matrix_iter`  | `matrix_iter(scale=1, border=None, verbose=False)`                                               | iterate the module matrix                     |
|  [06]   | `QRCode.symbol_size`  | `symbol_size(scale=1, border=None)`                                                              | rendered pixel/point dimensions               |
|  [07]   | `QRCode.terminal`     | `terminal(out=None, border=None, compact=False)`                                                 | render to ANSI/TTY                            |
|  [08]   | `QRCode.matrix`       | property                                                                                         | the boolean module matrix                     |
|  [09]   | `QRCode.version`      | property                                                                                         | resolved symbol version                       |
|  [10]   | `QRCode.error`        | property                                                                                         | resolved error-correction level               |
|  [11]   | `QRCode.designator`   | property                                                                                         | version-and-error designator string           |
|  [12]   | `QRCode.is_micro`     | property                                                                                         | Micro-QR kind flag                            |

## [04]-[IMPLEMENTATION_LAW]

[IMAGING_QR]:
- import: `import segno` at boundary scope only; module-level import is banned by the manifest import policy.
- factory axis: one `make` owns symbol encoding; `version`/`error`/`mode`/`mask`/`micro` are call rows, never a per-config builder type; `make_qr`/`make_micro` force the kind only when auto-selection must be overridden.
- render axis: `save` is the single serializer surface keyed by `kind` (or filename extension); SVG/PNG/PDF/EPS/text is a `kind` row with no raster dependency, never a parallel QR type — `matrix`/`matrix_iter` feed a custom renderer.
- error-correction axis: `error` selects the L/M/Q/H redundancy row by the deployment surface (print vs screen); `boost_error` raises the level when capacity allows.
- sequence axis: `make_sequence` is the structured-append row spanning a `QRCodeSequence`; multi-symbol payloads are a row, never a hand-stitched concatenation.
- evidence: each symbol captures version, error level, designator, module count, serializer kind, and output byte length as an imaging receipt.
- boundary: segno owns QR/Micro-QR generation and serialization with no Pillow dependency; raster post-processing routes to `pillow` only when explicitly required; the SVG path feeds the document and visuals owners directly; live UI stays outside this package.

[RAIL_LAW]:
- Package: `segno`
- Owns: QR/Micro-QR symbol generation, error-correction control, structured-append sequences, and dependency-free SVG/PNG/PDF/EPS/text serialization
- Accept: QR symbol generation and serialization feeding the imaging, document, and visuals owners
- Reject: wrapper-renames of `make`/`save`; a hand-rolled Reed-Solomon encoder; a forced Pillow raster path where the in-package serializers need no dependency; a parallel symbol type per output format; identity minting the runtime owns
