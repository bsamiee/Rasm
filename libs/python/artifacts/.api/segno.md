# [PY_ARTIFACTS_API_SEGNO]

`segno` supplies the pure-Python QR and Micro-QR code surface for the artifacts imaging rail: a `make` factory family that encodes content into a `QRCode`/`QRCodeSequence`, a `segno.helpers` payload family that pre-formats vCard/WiFi/EPC/geo/MeCard/email content before encoding, and a multi-format `save` path that serializes to SVG/PNG/PDF/EPS/PAM/PBM/PPM/LaTeX/XBM/XPM/ANSI/text with no raster dependency. The package owner composes `make`, `helpers`, `QRCode`, and `save` into the `PreviewOp.QR` path; it removes the `qrcode` Pillow leak because every serializer is in-package, and it never re-implements the Reed-Solomon QR encoding segno already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `segno`
- package: `segno`
- import: `segno`
- owner: `artifacts`
- rail: imaging
- installed: `1.6.6`
- entry points: console script `segno` (CLI); library use is import-only
- capability: QR (versions 1-40) and Micro-QR (M1-M4) symbol generation, error-correction L/M/Q/H, numeric/alphanumeric/byte/kanji/hanzi/ECI mode selection, boolean module matrix, structured-append sequences, structured-payload helpers, and dependency-free SVG/PNG/PDF/EPS/PAM/PBM/PPM/LaTeX/XBM/XPM/ANSI/text serialization

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: symbol and sequence roots
- rail: imaging

`make` returns a `QRCode`; `make_sequence` returns a `QRCodeSequence` (a `tuple` subclass of `QRCode` symbols) for structured-append spanning multiple symbols, sharing the `save`/`terminal` serializer surface across the whole span. `DataOverflowError` is raised when content exceeds the largest admissible version for the requested error level.

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]   | [RAIL]                                                |
| :-----: | :------------------ | :-------------- | :---------------------------------------------------- |
|  [01]   | `QRCode`            | symbol          | one QR or Micro-QR symbol with serializers            |
|  [02]   | `QRCodeSequence`    | symbol sequence | structured-append `tuple[QRCode, ...]` with span-save |
|  [03]   | `DataOverflowError` | error           | content-exceeds-capacity failure (`ValueError`)       |

[PUBLIC_TYPE_SCOPE]: encoding vocabulary
- rail: imaging

`segno.consts` carries the bounded policy vocabularies the factory rows accept as strings: `ERROR_LEVEL_L/M/Q/H` (the L/M/Q/H redundancy axis), `MODE_NUMERIC/ALPHANUMERIC/BYTE/KANJI/HANZI/ECI` (the segment-encoding axis), and `VERSION_M1..M4`/`VERSION_RANGE_*` (the symbol-version axis). Consumers pass the lowercase string (`error='h'`, `mode='byte'`); the constants are the canonical discriminant set the receipt records.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: factory functions
- rail: imaging

The factory rows share `error`, `version`, `mode`, `mask`, `encoding`, and `boost_error` policy; `make` auto-selects QR or Micro-QR unless `micro` forces the kind. `eci` enables the Extended Channel Interpretation header for non-default `encoding`.

| [INDEX] | [SURFACE]       | [CALL_SHAPE]                                                                                                                 | [CAPABILITY]                             |
| :-----: | :-------------- | :--------------------------------------------------------------------------------------------------------------------------- | :--------------------------------------- |
|  [01]   | `make`          | `make(content, error=None, version=None, mode=None, mask=None, encoding=None, eci=False, micro=None, boost_error=True)`      | encode content to a `QRCode` (auto kind) |
|  [02]   | `make_qr`       | `make_qr(content, error=None, version=None, mode=None, mask=None, encoding=None, eci=False, boost_error=True)`               | force a full QR symbol                   |
|  [03]   | `make_micro`    | `make_micro(content, error=None, version=None, mode=None, mask=None, encoding=None, boost_error=True)`                       | force a Micro-QR symbol                  |
|  [04]   | `make_sequence` | `make_sequence(content, error=None, version=None, mode=None, mask=None, encoding=None, boost_error=True, symbol_count=None)` | structured-append `QRCodeSequence`       |

[ENTRYPOINT_SCOPE]: `segno.helpers` structured-payload factories
- rail: imaging

The `helpers` module pre-formats domain payloads into the canonical QR text grammar before `make`. The `make_*` rows return an encoded `QRCode`; the `make_*_data` twins return the formatted `str` so a caller can fold encoding policy itself. This is the structured-content axis: never hand-format a vCard/WiFi/EPC string in the imaging owner when the helper owns the grammar.

| [INDEX] | [SURFACE]                                | [CALL_SHAPE]                                                                                              | [CAPABILITY]                                       |
| :-----: | :--------------------------------------- | :-------------------------------------------------------------------------------------------------------- | :------------------------------------------------- |
|  [01]   | `helpers.make_vcard` / `make_vcard_data` | `make_vcard(name, displayname, email=None, phone=None, url=None, org=None, lat=None, lng=None, ...)`      | vCard contact symbol / formatted vCard text        |
|  [02]   | `helpers.make_mecard` / `make_mecard_data` | `make_mecard(name, reading=None, email=None, phone=None, url=None, birthday=None, ...)`                 | MeCard contact symbol / formatted MeCard text      |
|  [03]   | `helpers.make_wifi` / `make_wifi_data`   | `make_wifi(ssid, password=None, security=None, hidden=False)`                                             | WiFi join symbol / formatted WiFi text             |
|  [04]   | `helpers.make_epc_qr`                    | `make_epc_qr(name, iban, amount, text=None, reference=None, bic=None, purpose=None, encoding=None)`       | SEPA EPC credit-transfer payment symbol            |
|  [05]   | `helpers.make_geo` / `make_geo_data`     | `make_geo(lat, lng)`                                                                                      | geo-location symbol / formatted `geo:` text        |
|  [06]   | `helpers.make_email` / `make_make_email_data` | `make_email(to, cc=None, bcc=None, subject=None, body=None)`                                         | mailto symbol / formatted `mailto:` text           |

[ENTRYPOINT_SCOPE]: `QRCode` render and inspect
- rail: imaging

`save` selects the serializer by file extension or explicit `kind`; serializer kwargs (`scale`, `border`, `dark`, `light`, plus per-kind options like SVG `xmldecl`/`svgclass` or PNG `dpi`/`compresslevel`) flow through `**kw`. Read properties carry the symbol metadata the receipt records. `QRCodeSequence` exposes the same `save`/`terminal` over the whole span.

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]                                                                                     | [CAPABILITY]                                            |
| :-----: | :------------------------- | :----------------------------------------------------------------------------------------------- | :------------------------------------------------------ |
|  [01]   | `QRCode.save`              | `save(out, kind=None, **kw)`                                                                     | serialize to a file/stream by extension or `kind`       |
|  [02]   | `QRCode.svg_inline`        | `svg_inline(**kw)` -> `str`                                                                       | inline SVG markup string (no XML decl)                  |
|  [03]   | `QRCode.svg_data_uri`      | `svg_data_uri(xmldecl=False, encode_minimal=False, omit_charset=False, nl=False, **kw)` -> `str` | SVG `data:` URI                                         |
|  [04]   | `QRCode.png_data_uri`      | `png_data_uri(**kw)` -> `str`                                                                     | PNG `data:` URI                                         |
|  [05]   | `QRCode.matrix_iter`       | `matrix_iter(scale=1, border=None, verbose=False)`                                               | iterate the (optionally scaled/verbose) module matrix   |
|  [06]   | `QRCode.symbol_size`       | `symbol_size(scale=1, border=None)` -> `(w, h)`                                                  | rendered pixel/point dimensions                         |
|  [07]   | `QRCode.terminal`          | `terminal(out=None, border=None, compact=False)`                                                 | render to ANSI/TTY (auto-detects Windows console)       |
|  [08]   | `QRCode.show`              | `show(delete_after=20, scale=10, border=None, dark='#000', light='#fff')`                        | open in the default image viewer (PNG temp)             |
|  [09]   | `QRCode.matrix`            | property -> `tuple[bytearray, ...]`                                                              | the boolean module matrix                               |
|  [10]   | `QRCode.version`           | property                                                                                         | resolved symbol version (`1`-`40` or `'M1'`-`'M4'`)     |
|  [11]   | `QRCode.error`             | property                                                                                         | resolved error-correction level (`'L'/'M'/'Q'/'H'`)     |
|  [12]   | `QRCode.mode`              | property                                                                                         | resolved primary segment mode                           |
|  [13]   | `QRCode.mask`              | property                                                                                         | resolved data-mask pattern index                        |
|  [14]   | `QRCode.designator`        | property                                                                                         | version-and-error designator string (e.g. `'1-H'`)      |
|  [15]   | `QRCode.is_micro`          | property                                                                                         | Micro-QR kind flag                                      |
|  [16]   | `QRCode.default_border_size` | property                                                                                       | the kind-specific default quiet-zone width              |

[ENTRYPOINT_SCOPE]: serializer kinds
- rail: imaging

`save`/`kind` dispatch over one in-package serializer registry; every kind is a row on the single `save` surface, never a parallel symbol type. Vector kinds (`svg`, `pdf`, `eps`, `tex`) and text kinds need no raster dependency; `png` is the only raster kind and is still pure-Python (zlib-only, no Pillow).

| [INDEX] | [KIND]                                    | [OUTPUT]                                  |
| :-----: | :---------------------------------------- | :---------------------------------------- |
|  [01]   | `svg` / `svg_debug`                       | scalable vector markup (debug variant adds error visualization) |
|  [02]   | `png`                                     | PNG raster (zlib, no Pillow)              |
|  [03]   | `pdf`                                     | single-page vector PDF                    |
|  [04]   | `eps`                                     | Encapsulated PostScript vector            |
|  [05]   | `tex`                                     | LaTeX `tikz` source                       |
|  [06]   | `pam` / `pbm` / `ppm`                      | Netpbm raster family                      |
|  [07]   | `xbm` / `xpm`                             | X BitMap / X PixMap                       |
|  [08]   | `txt`                                     | plain-text matrix dump                    |
|  [09]   | `terminal` / `terminal_compact` / `terminal_win` | ANSI/TTY render (compact + Windows variants) |

## [04]-[IMPLEMENTATION_LAW]

[IMAGING_QR]:
- import: `import segno` (and `from segno import helpers` for payloads) at boundary scope only; module-level import is banned by the manifest import policy.
- factory axis: one `make` owns symbol encoding; `version`/`error`/`mode`/`mask`/`micro`/`eci`/`encoding` are call rows sourced from the `consts` vocabulary, never a per-config builder type; `make_qr`/`make_micro` force the kind only when auto-selection must be overridden.
- payload axis: `segno.helpers.make_*` owns the structured-content grammar (vCard/MeCard/WiFi/EPC/geo/email); the imaging owner calls a helper to format the payload, never hand-concatenates a vCard or WiFi string; `make_*_data` returns the formatted `str` when the caller folds its own encoding policy.
- render axis: `save` is the single serializer surface keyed by `kind` (or filename extension); SVG/PNG/PDF/EPS/Netpbm/LaTeX/text/terminal is a `kind` row with no raster dependency (PNG is zlib-only), never a parallel QR type — `matrix`/`matrix_iter` feed a custom renderer when no built-in kind fits.
- error-correction axis: `error` selects the L/M/Q/H redundancy row by deployment surface (print vs screen); `boost_error=True` raises the level when spare capacity allows without growing the version.
- sequence axis: `make_sequence` is the structured-append row spanning a `QRCodeSequence` (a `QRCode` tuple); multi-symbol payloads are a row whose `save`/`terminal` cover the whole span, never a hand-stitched concatenation.
- evidence: each symbol captures version, error level, mode, mask, designator, module count (`symbol_size`), serializer kind, and output byte length as an imaging receipt; a `QRCodeSequence` adds the symbol count.
- boundary: segno owns QR/Micro-QR generation and serialization with no Pillow dependency; raster post-processing routes to `pyvips`/`pillow` only when explicitly required; the SVG path feeds the `svgelements` figure owner (scale-to-fit / n-up / bounds) and the document owners directly; live UI stays outside this package.

[STACKING]:
- `segno.QRCode.svg_inline()` / `save(kind='svg')` emits the SVG that the `svgelements` figure owner ingests via `SVG.parse(...)` for scale-to-fit, n-up, and `bbox()` query before document egress — the QR symbol stacks into the same figure-composition rail as `great-tables` and `vl-convert` SVG.
- `segno.helpers.make_*_data` returns formatted payload `str` that a `msgspec`/`pydantic` boundary model can validate (IBAN, e.164 phone, RFC-822 email) before `make` encodes it, so the structured-payload contract is typed once and the QR encoding is a pure transform.
- `QRCode.save(out_bytes_io, kind=...)` writes into the same `io.BytesIO`/file sink that feeds the `stream-zip` `MemberFile` data iterable, so a batch of QR symbols streams into a ZIP bundle without buffering whole files.
- `QRCode.png_data_uri()` / `svg_data_uri()` produce inline `data:` URIs for embedding directly in an HTML/SVG document tree, avoiding a separate asset write when the consumer is a single self-contained document.

[RAIL_LAW]:
- Package: `segno`
- Owns: QR/Micro-QR symbol generation, error-correction and mode control, structured-append sequences, structured-payload helpers (vCard/MeCard/WiFi/EPC/geo/email), and dependency-free SVG/PNG/PDF/EPS/Netpbm/LaTeX/text/terminal serialization
- Accept: QR symbol generation and serialization feeding the imaging, figure (`svgelements`), document, and bundle (`stream-zip`) owners
- Reject: wrapper-renames of `make`/`save`; a hand-rolled Reed-Solomon encoder; a hand-formatted vCard/WiFi/EPC string where `segno.helpers` owns the grammar; a forced Pillow raster path where the in-package serializers (PNG included) need no dependency; a parallel symbol type per output format; identity minting the runtime owns
