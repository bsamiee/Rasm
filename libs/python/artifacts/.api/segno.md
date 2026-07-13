# [PY_ARTIFACTS_API_SEGNO]

`segno` (dist `segno`, import `segno`) supplies the pure-Python QR and Micro-QR surface for the `graphic/marks/encode#MARK` `qr` arm — the segno case of the ONE `Mark` owner that spans python-barcode's linear 1D `linear` arm and zxing-cpp's 2D-matrix `matrix` arm. It is a `make`/`make_qr`/`make_micro`/`make_sequence` factory family resolving content into a `QRCode` (or a `QRCodeSequence` structured-append span) over the full `error`/`version`/`mode`/`mask`/`encoding`/`eci`/`micro`/`boost_error` parameter axis, a `segno.helpers` structured-payload family (`make_vcard`/`make_mecard`/`make_wifi`/`make_epc_qr`/`make_geo`/`make_email` plus the `_data` text twins) that pre-formats vCard/MeCard/WiFi/EPC/geo/email content into canonical QR text before encode, a `QRCode.save` serializer keyed by `kind` or filename extension over the in-package SVG/PNG/PDF/EPS/PAM/PBM/PPM/LaTeX/XBM/XPM/ANSI/text registry with NO raster dependency (PNG is zlib-only, never Pillow), and a `DataOverflowError`/`ValueError` raise pair the codec maps onto the closed `MarkFault` vocabulary. The codec owner binds segno as the `EncodeArm(qr=(SegnoFactory, accepts))` case keyed by `SegnoFactory.MAKE`/`MAKE_MICRO`/`MAKE_SEQUENCE`, threads the six `SHARED_FACTORY_KEYS` plus the per-row `SegnoKey` accepts through one `getattr(segno, factory)` spread, serializes through `symbol.save(kind="svg", **SvgStyle)` with the full per-module-color-plus-structural styling band, folds the content through `segno.helpers.make_*_data` exactly once at `of_encode` ingress, and stamps `designator`/`version`/`error`/`mask`/`mode`/`symbol_size` (or the `QRCodeSequence` symbol count) onto the shared `RasterFact.score`; it never re-implements the Reed-Solomon QR encoding segno already owns, never hand-concatenates a `WIFI:`/`vCard`/`MECARD:` grammar a helper owns in full, never forces a Pillow raster path the in-package serializers make unnecessary, never mints a parallel symbol type per output format, and routes DataMatrix/PDF417/Aztec/MaxiCode/rMQR to the zxing-cpp `matrix` arm and the linear symbologies to python-barcode because this package is strictly QR/Micro-QR.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `segno`
- package: `segno`
- import: `segno` (dist name `segno`, import name `segno`; the payload grammar is `segno.helpers`, the policy vocabulary `segno.consts`)
- owner: `artifacts`
- rail: imaging — the `graphic/marks/encode#MARK` `qr` `EncodeArm` arm (`Symbology.QR`/`MICRO_QR`/`QR_SEQUENCE`)
- license: `BSD` (`Classifier: License :: OSI Approved :: BSD License`; permissive, no copyleft obligation; the Pantone/proprietary rejection in `_REBUILD_BRIEF` [03] does not touch it)
- build-floor: `Requires-Python: >=3.5`; pure-Python wheel (`py3-none-any`, `Root-Is-Purelib: true`), no compiled extension and no cp-gate — installs on the cp315 interpreter unconditionally
- installed: `1.6.6`
- asset: every serializer is in-package and dependency-free — the SVG/PDF/EPS/LaTeX vector path, the Netpbm/XBM/XPM/text path, the ANSI/terminal path, AND the `png` raster path (zlib-only via `write_png`, NO Pillow); raster post-processing routes to the admitted `pyvips`/`pillow` packages only when explicitly required, never a per-package pin and never a forced raster dependency through this owner
- entry points: console script `segno` (CLI; `man segno.1`); the codec owner is import-only and never shells the CLI
- capability: QR (versions 1-40) and Micro-QR (M1-M4) symbol generation, L/M/Q/H error correction with `boost_error` auto-raise, numeric/alphanumeric/byte/kanji/hanzi/ECI segment-mode selection, explicit data-mask selection, structured-append `QRCodeSequence` spanning multiple symbols, the `segno.helpers` structured-payload grammar (vCard/MeCard/WiFi/EPC/geo/email + `_data` text twins), the boolean module matrix (`matrix`/`matrix_iter`), the `designator`/`version`/`error`/`mask`/`mode`/`symbol_size`/`is_micro` evidence surface, and dependency-free SVG/PNG/PDF/EPS/PAM/PBM/PPM/LaTeX/XBM/XPM/ANSI/text serialization plus `svg_inline`/`svg_data_uri`/`png_data_uri` inline forms

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: symbol and sequence roots
- rail: imaging

`segno.__all__` is exactly `('make', 'make_qr', 'make_micro', 'make_sequence', 'QRCode', 'QRCodeSequence', 'DataOverflowError')`. `make` returns a `QRCode`; `make_sequence` returns a `QRCodeSequence` — a `tuple` subclass whose elements are `QRCode` symbols, sharing the `save`/`terminal` serializer surface across the whole structured-append span. `DataOverflowError` (a `ValueError` subtype, re-exported from `segno.encoder`) is raised when content exceeds the largest admissible version for the requested error level; the `qr` arm maps it onto `MarkFault.overflow` carrying the `Symbology`, distinct from the non-overflow `ValueError` an out-of-range `version`/`mask`/`mode`/`error` factory parameter raises (mapped onto `MarkFault.parameter`).

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]   | [CAPABILITY]                                                                                |
| :-----: | :------------------ | :-------------- | :------------------------------------------------------------------------------------------ |
|  [01]   | `QRCode`            | symbol          | one QR/Micro-QR symbol (serializers/reads in [03]); the `make`/`make_qr`/`make_micro` yield |
|  [02]   | `QRCodeSequence`    | symbol sequence | structured-append `tuple[QRCode, ...]`; `save`/`terminal` span it (`_segno_score` -> `len`) |
|  [03]   | `DataOverflowError` | error           | content-exceeds-capacity `ValueError`; the `overflow` `MarkFault` cause                     |

[PUBLIC_TYPE_SCOPE]: encoding policy vocabulary
- rail: imaging

`segno.consts` carries the bounded policy vocabularies the factory rows accept as lowercase strings: the error-correction axis, the segment-mode axis, the symbol-version axis, and the capacity tables that bound `DataOverflowError`. Consumers pass the lowercase string (`error='h'`, `mode='byte'`, `version='m4'`); the constants are the canonical discriminant set the receipt records. The codec owner does NOT import the constants directly — it threads the `MarkPayload`-admitted lowercase strings through the `SHARED_FACTORY_KEYS` spread — but the vocabulary is the closed axis the `MarkPayload` keys validate against.

| [INDEX] | [SYMBOL]                                                                                      | [TYPE_FAMILY]       |
| :-----: | :-------------------------------------------------------------------------------------------- | :------------------ |
|  [01]   | `ERROR_LEVEL_L` / `ERROR_LEVEL_M` / `ERROR_LEVEL_Q` / `ERROR_LEVEL_H`                         | error axis          |
|  [02]   | `MODE_NUMERIC` / `MODE_ALPHANUMERIC` / `MODE_BYTE` / `MODE_KANJI` / `MODE_HANZI` / `MODE_ECI` | segment-mode axis   |
|  [03]   | `VERSION_M1` / `VERSION_M2` / `VERSION_M3` / `VERSION_M4`                                     | symbol-version axis |
|  [04]   | `VERSION_RANGE_01_09` / `VERSION_RANGE_10_26` / `VERSION_RANGE_27_40`                         | version-band axis   |
|  [05]   | `SYMBOL_CAPACITY` / `RSYMBOL_CAPACITY` / `MICRO_VERSIONS`                                     | capacity table      |

- [01]-[ERROR]: the L/M/Q/H redundancy levels (`error='l'..'h'`); `ERROR_LEVEL_TO_MICRO_MAPPING` carries the Micro-QR-admissible subset.
- [02]-[MODE]: the segment-encoding modes (`mode='numeric'..'byte'`); `MODE_STRUCTURED_APPEND` heads a `make_sequence` span and `SUPPORTED_MODES`/`MODE_MAPPING` carry the admissible set.
- [03]-[VERSION]: the four Micro-QR versions (`version='m1'..'m4'`); `MICRO_VERSIONS`/`MICRO_VERSION_MAPPING` carry the Micro set.
- [04]-[VERSION_BAND]: the three full-QR version bands keying char-count-indicator length; the real names are the `01_09`/`10_26`/`27_40` triple, never a bare `VERSION_RANGE_*`.
- [05]-[CAPACITY]: the per-version/per-error byte-capacity tables segno consults to raise `DataOverflowError` when content exceeds the largest admissible version — the table backing the `overflow` `MarkFault` boundary.

[NOTE]: `segno.consts` also carries the internal Galois-field/format-info/matrix-type tables (`GALIOS_EXP`/`GALIOS_LOG`/`GEN_POLY`/`FORMAT_INFO*`/`TYPE_*`/`ALIGNMENT_POS`) the encoder and the `matrix_iter(verbose=True)` module-type stream use; these are encoder internals, never a `MarkPayload` axis — the codec reads the `TYPE_*` discriminants only through `matrix_iter(verbose=True)` when a custom renderer needs the per-module classification, never by importing the raw tables.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: factory functions
- rail: imaging

The factory rows share the six `SHARED_FACTORY_KEYS` (`error`, `version`, `mode`, `mask`, `encoding`, `boost_error`) plus the per-row accepts; the shared head `(content, error=None, version=None, mode=None, mask=None, encoding=None, boost_error=True)` is elided as `…` in the table, which carries only the per-row accepts. `make` auto-selects QR or Micro-QR unless `micro` forces the kind. `eci` enables the Extended Channel Interpretation header for a non-default `encoding`. The `qr` arm resolves the factory through `getattr(segno, factory)` over the `SegnoFactory` member (the name travels as the enum, never a bare literal) and threads `{key: opts[key] for key in (*SHARED_FACTORY_KEYS, *accepts) if key in opts}` so an over-key never reaches a factory that rejects it.

| [INDEX] | [SURFACE]       | [CALL_SHAPE]                               | [CAPABILITY]                                                     |
| :-----: | :-------------- | :----------------------------------------- | :--------------------------------------------------------------- |
|  [01]   | `make`          | `make(…, eci=False, micro=None) -> QRCode` | encode to a `QRCode` (auto QR/Micro); `MAKE` row + `eci`/`micro` |
|  [02]   | `make_qr`       | `make_qr(…, eci=False) -> QRCode`          | force a full QR (no Micro auto-select); accepts `eci`            |
|  [03]   | `make_micro`    | `make_micro(…) -> QRCode`                  | force a Micro-QR; `SegnoFactory.MAKE_MICRO`, accepts none        |
|  [04]   | `make_sequence` | `make_sequence(…, symbol_count=None)`      | structured-append span; `MAKE_SEQUENCE` row + `symbol_count`     |

[ENTRYPOINT_SCOPE]: `segno.helpers` structured-payload factories
- rail: imaging

The `helpers` module pre-formats domain payloads into the canonical QR text grammar before `make`. The `make_*` rows return an encoded `QRCode`; the `make_*_data` twins return the formatted `str` so the codec folds encoding policy itself — and the codec ALWAYS takes the `_data` twin, folding it to canonical text through `_resolved_content` at `of_encode` ingress (the `Content` closed family's `wifi`/`vcard`/`mecard`/`geo`/`email` cases), never the `QRCode`-returning `make_*` (which does bypass the one factory axis and the `MarkPayload` parameter band). The full helper public surface is `('make_email', 'make_epc_qr', 'make_geo', 'make_geo_data', 'make_make_email_data', 'make_mecard', 'make_mecard_data', 'make_vcard', 'make_vcard_data', 'make_wifi', 'make_wifi_data', 'quote')` — each `make_*` has a `make_*_data` `str` twin except `make_epc_qr`. This is the structured-content axis: never hand-format a vCard/WiFi/MeCard/geo/email string in the imaging owner when the helper owns the grammar. The `make_email` text twin is genuinely named `make_make_email_data` (a doubled-verb upstream typo, not `make_email_data`); `_resolved_content` calls `helpers.make_make_email_data(...)` exactly as spelled. The three wide field grammars carry their full signature here:

```python signature
make_vcard_data(name, displayname, email=None, phone=None, fax=None, videophone=None, memo=None,
    nickname=None, birthday=None, url=None, pobox=None, street=None, city=None, region=None, zipcode=None,
    country=None, org=None, lat=None, lng=None, source=None, rev=None, title=None, photo_uri=None,
    cellphone=None, homephone=None, workphone=None) -> str   # 26-field vCard = Content.vcard VCardFields
make_mecard_data(name, reading=None, email=None, phone=None, videophone=None, memo=None, nickname=None,
    birthday=None, url=None, pobox=None, roomno=None, houseno=None, city=None, prefecture=None,
    zipcode=None, country=None) -> str                       # 16-field MeCard = Content.mecard MeCardFields
make_epc_qr(name, iban, amount, text=None, reference=None, bic=None, purpose=None, encoding=None) -> QRCode
```

| [INDEX] | [SURFACE]              | [CALL_SHAPE]                                         | [CAPABILITY]                                           |
| :-----: | :--------------------- | :--------------------------------------------------- | :----------------------------------------------------- |
|  [01]   | `make_vcard_data`      | `make_vcard_data(name, displayname, …) -> str`       | 26-field vCard; `VCardFields` `**fields` (sig above)   |
|  [02]   | `make_mecard_data`     | `make_mecard_data(name, …) -> str`                   | 16-field MeCard; `MeCardFields` `**fields` (sig above) |
|  [03]   | `make_wifi_data`       | `make_wifi_data(ssid, password, security, …) -> str` | WiFi join; `Content.wifi` 4-tuple, already complete    |
|  [04]   | `make_geo_data`        | `make_geo_data(lat, lng) -> str`                     | `geo:` URI; `Content.geo` `(lat, lng)` tuple           |
|  [05]   | `make_make_email_data` | `make_make_email_data(to, cc=None, …) -> str`        | `mailto:`; `Content.email` 5-tuple (typo, see lead)    |
|  [06]   | `make_epc_qr`          | `make_epc_qr(name, iban, amount, …) -> QRCode`       | SEPA EPC payment symbol; one justified exclusion       |
|  [07]   | `helpers.quote`        | `quote(s) -> str`                                    | helpers-internal value-escaper for the `_data` forms   |

[ENTRYPOINT_SCOPE]: `QRCode` render and inspect
- rail: imaging

`save` selects the serializer by filename extension or explicit `kind`; serializer kwargs (`scale`, `border`, `dark`, `light`, plus per-kind options) flow through `**kw`. Read properties carry the symbol metadata the receipt records. `QRCodeSequence` exposes only `save`/`terminal` over the whole span. The codec's `qr` arm calls `symbol.save(BytesIO(), kind="svg", scale=..., border=..., dark=..., light=..., **SvgStyle)` so the SVG bytes land in the same in-memory sink as the python-barcode `write(BytesIO(), ...)` and zxing `to_svg().encode()` arms, then reads `symbol_size`/`designator`/`version`/`error`/`mask`/`mode` for the `RasterFact.score`.

| [INDEX] | [SURFACE]             | [CALL_SHAPE]                                       | [CAPABILITY]                                              |
| :-----: | :-------------------- | :------------------------------------------------- | :-------------------------------------------------------- |
|  [01]   | `save`                | `save(out: IO \| str, kind=None, **kw) -> None`    | serialize by extension/`kind`; the `kind="svg"` sink      |
|  [02]   | `svg_inline`          | `svg_inline(**kw) -> str`                          | inline SVG markup (no XML decl); in-document embed        |
|  [03]   | `svg_data_uri`        | `svg_data_uri(xmldecl=False, …, **kw) -> str`      | SVG `data:` URI for a self-contained document tree        |
|  [04]   | `png_data_uri`        | `png_data_uri(**kw) -> str`                        | PNG `data:` URI (zlib raster, no Pillow)                  |
|  [05]   | `matrix_iter`         | `matrix_iter(scale=1, border=None, verbose=False)` | module iterator; `verbose=True` -> per-module `TYPE_*`    |
|  [06]   | `symbol_size`         | `symbol_size(scale=1, border=None) -> tuple`       | rendered `(w, h)`; `SYMBOL_SIZE` `190x190` @ scale-10     |
|  [07]   | `terminal`            | `terminal(out=None, border=None, compact=False)`   | ANSI/TTY render; `ans` `save` is the file-sink twin       |
|  [08]   | `show`                | `show(delete_after=20, scale=10, …) -> None`       | open in default image viewer; dev/diagnostic, off-rail    |
|  [09]   | `matrix`              | attribute -> `tuple[bytearray, ...]`               | boolean module matrix; feeds a custom renderer            |
|  [10]   | `version`             | property -> `int \| str`                           | resolved version (`1`-`40`/`'M1'`-`'M4'`); `VERSION` fact |
|  [11]   | `error`               | property -> `str`                                  | resolved level (`'L'`/`'M'`/`'Q'`/`'H'`); `ERROR` fact    |
|  [12]   | `mode`                | property -> `str \| None`                          | resolved primary segment mode; the `MODE` score fact      |
|  [13]   | `mask`                | attribute -> `int`                                 | resolved data-mask index; the `MASK` score fact           |
|  [14]   | `designator`          | property -> `str`                                  | designator (e.g. `'1-H'`); the `DESIGNATOR` score fact    |
|  [15]   | `is_micro`            | property -> `bool`                                 | Micro-QR kind flag                                        |
|  [16]   | `default_border_size` | property -> `int`                                  | the kind-specific default quiet-zone width                |

[ENTRYPOINT_SCOPE]: serializer kinds and per-kind options
- rail: imaging

`save`/`kind` dispatch over one in-package serializer registry — the real `_VALID_SERIALIZERS` keys are exactly `svg`/`png`/`eps`/`txt`/`pdf`/`ans`/`pbm`/`pam`/`ppm`/`tex`/`xbm`/`xpm` (filename extension or explicit `kind`); every kind is a row on the single `save` surface, never a parallel symbol type. ANSI/TTY render is the `ans` `save` kind (the `QRCode.terminal()` method is the separate render-to-stdout surface with its `compact`/Windows variants); `svg_debug` is the standalone `write_svg_debug` writer, NOT a `save` kind. The text-mode kinds (`eps`/`txt`/`tex`/`ans`/`xbm`/`xpm`) write `str`, so they need a text sink (`io.StringIO`/text-mode file); the codec's `kind="svg"` `BytesIO` path is bytes-native. The `colormap` band (the high-level per-module color keys the `save` kwargs build) is the SVG/PNG/PPM color axis; the structural axis is per-kind. Vector kinds (`svg`, `pdf`, `eps`, `tex`) and text kinds need no raster dependency; `png` is the only raster kind and is still pure-Python (`write_png`, zlib-only, no Pillow). The `SvgStyle` `MarkPayload` band threads the full SVG axis through `**opts.get("svg", frozendict())`.

| [INDEX] | [KIND]                | [OUTPUT]                                                            |
| :-----: | :-------------------- | :------------------------------------------------------------------ |
|  [01]   | `svg`                 | scalable vector markup (the `qr` arm's serializer)                  |
|  [02]   | `png`                 | PNG raster (`write_png`, zlib, no Pillow)                           |
|  [03]   | `pdf`                 | single-page vector PDF                                              |
|  [04]   | `eps`                 | Encapsulated PostScript vector (text sink)                          |
|  [05]   | `tex`                 | LaTeX `tikz` source (text sink)                                     |
|  [06]   | `pam` / `pbm` / `ppm` | Netpbm raster family                                                |
|  [07]   | `xbm` / `xpm`         | X BitMap / X PixMap                                                 |
|  [08]   | `txt`                 | plain-text matrix dump (text sink)                                  |
|  [09]   | `ans`                 | ANSI/TTY render (text sink); `QRCode.terminal()` is the stdout twin |

Per-kind `save` options:
- [01]-[SVG]: `xmldecl`, `svgns`, `title`, `desc`, `svgid`, `svgclass='segno'`, `lineclass='qrline'`, `omitsize`, `unit`, `encoding='utf-8'`, `svgversion`, `nl`, `draw_transparent` + the `colormap` per-module colors (`finder_dark`/`data_dark`/`alignment_dark`/`timing_dark`/`version_dark`/`format_dark`/`dark_module`/`separator`/`quiet_zone` and `*_light` twins).
- [02]-[PNG]: `colormap`, `compresslevel=9`, `dpi`.
- [03]-[PDF] / [04]-[EPS]: `dark='#000'`, `light`.
- [05]-[TEX]: `dark='black'`, `unit='pt'`, `url`.
- [06]-[NETPBM]: `pam`/`ppm`: `dark`/`light` (+ `colormap` for `ppm`); `pbm`: `plain`.
- [07]-[XBM_XPM]: `xbm`: `name='img'` (text sink); `xpm`: `dark`/`light` (text sink).
- [08]-[TXT]: `dark='1'`, `light='0'`.
- [09]-[ANS]: `border`.

## [04]-[IMPLEMENTATION_LAW]

[IMAGING_QR]:
- import: `lazy import segno` (and `lazy from segno import helpers`) at `graphic/marks/encode#MARK` boundary scope only; module-level import is banned by the manifest import policy. The annotation-only `from segno import QRCode, QRCodeSequence` rides the `if TYPE_CHECKING` block (the `_segno_score` signature), never the runtime import.
- factory axis: one `getattr(segno, factory)` spread over the `SegnoFactory` member owns symbol encoding — `make`/`make_micro`/`make_sequence` are three `SYMBOLOGIES` rows (`Symbology.QR`/`MICRO_QR`/`QR_SEQUENCE`) each binding `EncodeArm(qr=(SegnoFactory, accepts))`, never a per-config builder type and never a literal factory name. `make_qr` (force-full-QR) is the `make` semantics with `micro=False`, so the codec admits it as the `make` row plus the `micro` accept rather than a fourth `SegnoFactory` member. The six `SHARED_FACTORY_KEYS` thread the common parameters; the per-row `accepts` (`eci`/`micro` on `make`, `symbol_count` on `make_sequence`, none on `make_micro`) thread the factory-specific keys, key-filtered against the admitted `frozendict` so an over-key never crashes a factory that rejects it.
- payload axis: `segno.helpers.make_*_data` owns the structured-content grammar (vCard/MeCard/WiFi/geo/email); the `Content` closed family (`raw`/`wifi`/`vcard`/`mecard`/`geo`/`email`) folds to canonical text through `_resolved_content` at `of_encode` ingress exactly once — the rich `vcard`/`mecard` cases spreading the full closed `VCardFields` (26 keys) / `MeCardFields` (16 keys) `TypedDict` as `**fields`, the small-grammar `wifi`/`geo`/`email` cases already complete over their helper's 4/2/5 parameters. The codec NEVER hand-concatenates a `WIFI:`/`vCard` string and NEVER models a thin slice of a contact the helper carries in full; a malformed payload raises `ValueError`/`TypeError` mapped onto `MarkFault.content`. `make_epc_qr` is the one justified exclusion (no `_data` text twin — a documented growth axis).
- render axis: `symbol.save(BytesIO(), kind="svg", **SvgStyle)` is the single serializer surface keyed by `kind` — SVG/PNG/PDF/EPS/Netpbm/LaTeX/text/ANSI(`ans`) is a `kind` row with no raster dependency (PNG is zlib-only), never a parallel QR type. The full SVG styling surface (the `colormap` per-module color keys plus the structural `svgclass`/`lineclass`/`svgid`/`omitsize`/`unit`/`xmldecl`/`svgns`/`title`/`desc`/`svgversion`/`nl`/`draw_transparent` axis) threads through the nested closed `SvgStyle` `TypedDict` band spread as `**opts.get("svg", frozendict())`, never a `dark`/`light`-only two-knob slice of a ~28-knob surface. A rejected style combination (`omitsize` with `unit`) raises a serializer `ValueError` mapped onto `MarkFault.render` (structurally distinct from the admission-time `MarkFault.options`), never an unwrapped raise escaping the encode capsule. `matrix`/`matrix_iter` feed a custom renderer only when no built-in kind fits.
- error-correction axis: `error` selects the L/M/Q/H redundancy row by deployment surface (print vs screen); `boost_error=True` raises the level when spare capacity allows without growing the version. Both ride the `MarkPayload` `error`/`boost_error` keys through `SHARED_FACTORY_KEYS`.
- sequence axis: `make_sequence` is the structured-append row spanning a `QRCodeSequence` (a `QRCode` tuple); a multi-symbol payload is one `make_sequence` plus one `QRCodeSequence.save(kind="svg")` keyed by `symbol_count`, never a hand-stitched concatenation. The `_segno_score` `factory is SegnoFactory.MAKE_SEQUENCE` identity guard routes the sequence row to `len(QRCodeSequence)` (the `SYMBOLS` score fact) so the `QRCodeSequence` never reaches the `QRCode`-only `designator`/`symbol_size` surface.
- fault axis: `DataOverflowError` (content past the largest version) maps onto `MarkFault.overflow` carrying the `Symbology`, a non-overflow factory `ValueError` (out-of-range `version`/`mask`/`mode`/`error`) onto `MarkFault.parameter`, a serializer `ValueError` (rejected `SvgStyle`) onto `MarkFault.render`, and a `segno.helpers` payload-format `ValueError`/`TypeError` onto `MarkFault.content` — each named exactly at the segno arm, never a bare `except Exception` flattening the causes and never `None`-as-failure.
- evidence: each symbol stamps `designator`/`version`/`error`/`mask`/`mode`/`symbol_size` onto the shared `RasterFact.score` `frozendict` keyed by the `MarkFact` vocabulary (the sequence row stamps the symbol count); the SVG path reports the default zero pixel dimensions (the module extent rides the score), projected to `core/receipt#RECEIPT` `ArtifactReceipt.Preview(key, 0, 0, scores)` at the boundary.
- boundary: segno owns QR/Micro-QR generation and serialization with NO Pillow dependency (PNG included); the `qr` arm composes beside the python-barcode `linear` arm and the zxing-cpp `matrix` arm under the one `Mark` owner; the SVG bytes feed the `svgelements` figure owner and the `document` owners directly; raster post-processing routes to `pyvips`/`pillow` only when explicitly required; live UI stays outside this package; DataMatrix/PDF417/Aztec/MaxiCode/rMQR route to zxing-cpp and the linear symbologies to python-barcode (segno is strictly QR/Micro-QR).

[STACKING]:
- The `qr` arm folds onto the SHARED `libs/python/.api` substrate exactly as the python-barcode `linear` and zxing-cpp `matrix` arms do: the SVG bytes from `symbol.save(BytesIO(), kind="svg", **SvgStyle)` cross the seam as a `Result[RasterFact, MarkFault]` (`expression` `Ok`/`Error`), the `MarkPayload`/`SvgStyle`/`Content` bands are admitted once through the module-level `_PAYLOAD = TypeAdapter(MarkPayload)` and a `ValidationError` maps onto `MarkFault.options` as the full `tuple[str, ...]` of `.errors()` `loc` paths (`pydantic`), the closed `Symbology`/`SegnoFactory`/`SegnoKey`/`MarkFault`/`Content` vocabularies and the `RasterFact` value object are `msgspec.Struct`/`expression.tagged_union` shapes, the `_encode` worker is `beartype`-contracted at definition time (`_contracted` lifts a `BeartypeCallHintViolation` onto `MarkFault.contract`) and offloaded off the event loop under the one `_MARK_LANE` `CapacityLimiter` via `anyio.to_thread.run_sync`, and the `RasterFact.score` evidence threads onto `ArtifactReceipt.Preview.scores` for the `structlog`/OTel receipt — never a flat folder-only call.
- The dependency-free SVG byte stream stacks straight into the folder-tier figure rail: the SVG markup parses through `svgelements` `SVG.parse(BytesIO(svg), reify=True).bbox()` for scale-to-fit / n-up / `bbox()` query before document egress (the `Mark.layered` `_mark_bbox` projection, a parse failure railed onto `MarkFault.geometry`), landing a QR in the same figure-composition rail as a python-barcode linear `write(BytesIO())` SVG, a `great-tables` table SVG, and a `vl-convert` chart SVG. One imaging owner discriminates on `(EncodeArm, kind)`, never a per-format QR type.
- The `qr`, `linear`, and `matrix` arms share the imaging-receipt shape (`RasterFact` -> `ArtifactReceipt.Preview`), so a mixed QR + linear + 2D-matrix label sheet folds through one `Mark.over(MarkOp | Iterable[MarkOp])` entrypoint into one `Block[ArtifactReceipt]` stream — the segno `designator`/`version`/`error`/`mask`/`mode`/`symbol_size`, the python-barcode `get_fullcode` check digit, and the zxing resolved `format`/`ec_level` all land on the same `Preview.scores` `frozendict[str, float | str]` band (which also carries the `graphic/raster/measure#MEASURE` perceptual `float` band — one mint absorbing perceptual and machine-readable-mark evidence under the `float | str` value union).
- `segno.helpers.make_*_data` returns formatted payload `str` that the `Content` closed family carries typed once (the `VCardFields`/`MeCardFields` closed `TypedDict` a `msgspec`/`pydantic` boundary admits), so the structured-payload contract is validated at `of_encode` ingress and the QR encoding is a pure transform — never a structured object threaded into the interior, never a re-validated `dict[str, object]` bag in the arm.
- `QRCode.save(BytesIO(), kind="svg")` writes into the same `io.BytesIO` sink that feeds a `stream-zip` `MemberFile` data iterable, so a batch of QR symbols streams into a ZIP label bundle without buffering whole files — identical to the python-barcode `write(BytesIO())` and zxing `to_svg().encode()` bundle paths. `QRCode.png_data_uri()` / `svg_data_uri()` produce inline `data:` URIs for embedding directly in an HTML/SVG document tree when the consumer is a single self-contained document, avoiding a separate asset write.
- The `Decode` round-trip the segno `qr` arm cannot express is owned by the zxing-cpp `matrix` arm (`graphic/marks/decode#DECODE` `read_barcodes` over `MarkOp.Decode`): segno is generation-only, so an encode-correctness proof for a segno QR is a `zxingcpp.read_barcodes` decode pass on the rendered raster, not a segno capability — the `Mark` owner provides the round-trip across arms, never segno alone.

[RAIL_LAW]:
- Package: `segno`
- Owns: QR (versions 1-40) and Micro-QR (M1-M4) symbol generation, L/M/Q/H error correction with `boost_error` auto-raise, numeric/alphanumeric/byte/kanji/hanzi/ECI segment-mode and explicit data-mask control, structured-append `QRCodeSequence` spans, the `segno.helpers` structured-payload grammar (vCard/MeCard/WiFi/EPC/geo/email + `_data` text twins), the boolean module matrix (`matrix`/`matrix_iter`), the `designator`/`version`/`error`/`mask`/`mode`/`symbol_size`/`is_micro` evidence surface, and dependency-free SVG/PNG/PDF/EPS/Netpbm/LaTeX/text/terminal serialization plus `svg_inline`/`svg_data_uri`/`png_data_uri` inline forms
- Accept: `getattr(segno, factory)` resolution over the `SegnoFactory` member with the six `SHARED_FACTORY_KEYS` plus per-row `SegnoKey` accepts, `make_sequence` structured-append keyed by `symbol_count`, the `Content` family folding through `helpers.make_*_data` (the genuinely-spelled `make_make_email_data` included), `symbol.save(BytesIO(), kind="svg", **SvgStyle)` SVG bytes folding to `RasterFact`, the nested closed `SvgStyle` per-module-color-plus-structural band on `MarkPayload`, the `DataOverflowError`/`ValueError` raises mapped onto distinct `MarkFault` cases, SVG bytes feeding the `svgelements`/document figure owners and the `stream-zip` bundle sink
- Reject: wrapper-renames of `make`/`save`; a hand-rolled Reed-Solomon QR encoder; a hand-formatted vCard/WiFi/MeCard/geo/email string where `segno.helpers` owns the grammar; a thin slice of a contact grammar (the 6-field vcard slice the helper carries in full at 26 fields); the `QRCode`-returning `make_*` helper where the `_data` twin folds to text through the one factory axis; a forced Pillow raster path where the in-package serializers (PNG included) need no dependency; a `dark`/`light`-only slice of the ~28-knob `SvgStyle` surface; a parallel symbol type per output format; an unwrapped `save` letting a serializer `ValueError` escape the encode capsule; a bare `except Exception` flattening the `DataOverflowError`/`ValueError` causes; a `VERSION_RANGE_*` reference where the real names are the `01_09`/`10_26`/`27_40` triple; a claimed 2D-matrix or linear symbology (DataMatrix/PDF417/Code128) this package does not own — those route to the zxing-cpp `matrix` and python-barcode `linear` arms; identity minting the runtime owns
