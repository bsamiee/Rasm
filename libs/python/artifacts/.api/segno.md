# [PY_ARTIFACTS_API_SEGNO]

`segno` (import `segno`) owns pure-Python QR (versions 1-40) and Micro-QR (M1-M4) generation for the `qr` arm of the one `Mark` owner spanning python-barcode's `linear` and zxing-cpp's `matrix` arms. A `make`/`make_micro`/`make_sequence` factory resolves content into a `QRCode` or structured-append `QRCodeSequence`, `segno.helpers` pre-formats vCard/MeCard/WiFi/EPC/geo/email payloads to canonical QR text, and every serializer is in-package and dependency-free down to the zlib-only `png` path, never Pillow.

`EncodeArm(qr=(SegnoFactory, accepts))` binds it, `DataOverflowError`/`ValueError` mapping onto the closed `MarkFault` vocabulary; DataMatrix/PDF417/Aztec/MaxiCode/rMQR route to zxing-cpp and the linear symbologies to python-barcode.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `segno`
- package: `segno` (BSD)
- import: `segno` (payload grammar `segno.helpers`, policy vocabulary `segno.consts`)
- owner: `artifacts`
- rail: imaging — the `graphic/marks/encode#MARK` `qr` `EncodeArm` arm (`Symbology.QR`/`MICRO_QR`/`QR_SEQUENCE`)
- asset: pure-Python wheel, no compiled extension, no cp-gate; every serializer is in-package and dependency-free including the `png` raster path (`write_png`, zlib-only, no Pillow), so raster post-processing routes to `pyvips`/`pillow` only when explicitly required
- entry points: console script `segno`; the codec owner is import-only, never shelling the CLI

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: symbol and sequence roots

`make` yields a `QRCode`; `make_sequence` yields a `QRCodeSequence`, a `tuple` subclass of `QRCode` symbols sharing the `save`/`terminal` serializer surface across the structured-append span. `DataOverflowError` is a `ValueError` subtype re-exported from `segno.encoder`.

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]   | [CAPABILITY]                                                                                |
| :-----: | :------------------ | :-------------- | :------------------------------------------------------------------------------------------ |
|  [01]   | `QRCode`            | symbol          | one QR/Micro-QR symbol (serializers/reads in [03]); the `make`/`make_qr`/`make_micro` yield |
|  [02]   | `QRCodeSequence`    | symbol sequence | structured-append `tuple[QRCode, ...]`; `save`/`terminal` span it (`_segno_score` -> `len`) |
|  [03]   | `DataOverflowError` | error           | content-exceeds-capacity `ValueError`; the `overflow` `MarkFault` cause                     |

[PUBLIC_TYPE_SCOPE]: encoding policy vocabulary

`segno.consts` carries the bounded policy vocabularies the factory rows accept as lowercase strings (`error='h'`, `mode='byte'`, `version='m4'`) and the capacity tables that bound `DataOverflowError`; the codec threads the `MarkPayload`-admitted strings through `SHARED_FACTORY_KEYS` rather than importing the constants, and the Galois-field/format-info/matrix-type tables are encoder internals a custom renderer reaches only through `matrix_iter(verbose=True)`, never a `MarkPayload` axis.

| [INDEX] | [SYMBOL]                                                                                      | [TYPE_FAMILY]       |
| :-----: | :-------------------------------------------------------------------------------------------- | :------------------ |
|  [01]   | `ERROR_LEVEL_L` / `ERROR_LEVEL_M` / `ERROR_LEVEL_Q` / `ERROR_LEVEL_H`                         | error axis          |
|  [02]   | `MODE_NUMERIC` / `MODE_ALPHANUMERIC` / `MODE_BYTE` / `MODE_KANJI` / `MODE_HANZI` / `MODE_ECI` | segment-mode axis   |
|  [03]   | `VERSION_M1` / `VERSION_M2` / `VERSION_M3` / `VERSION_M4`                                     | symbol-version axis |
|  [04]   | `VERSION_RANGE_01_09` / `VERSION_RANGE_10_26` / `VERSION_RANGE_27_40`                         | version-band axis   |
|  [05]   | `SYMBOL_CAPACITY` / `RSYMBOL_CAPACITY` / `MICRO_VERSIONS`                                     | capacity table      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: factory functions
- shared head: `(content, error, version, mode, mask, encoding, boost_error)`, elided as `…`; the table carries only per-row accepts

`make` auto-selects QR or Micro-QR unless `micro` forces the kind, and `eci` emits the Extended Channel Interpretation header for a non-default `encoding`.

| [INDEX] | [SURFACE]       | [CALL_SHAPE]                               | [CAPABILITY]                                                     |
| :-----: | :-------------- | :----------------------------------------- | :--------------------------------------------------------------- |
|  [01]   | `make`          | `make(…, eci=False, micro=None) -> QRCode` | encode to a `QRCode` (auto QR/Micro); `MAKE` row + `eci`/`micro` |
|  [02]   | `make_qr`       | `make_qr(…, eci=False) -> QRCode`          | force a full QR (no Micro auto-select); accepts `eci`            |
|  [03]   | `make_micro`    | `make_micro(…) -> QRCode`                  | force a Micro-QR; `SegnoFactory.MAKE_MICRO`, accepts none        |
|  [04]   | `make_sequence` | `make_sequence(…, symbol_count=None)`      | structured-append span; `MAKE_SEQUENCE` row + `symbol_count`     |

[ENTRYPOINT_SCOPE]: `segno.helpers` structured-payload factories

Each `make_*` returns an encoded `QRCode`; the codec always takes the `make_*_data` `str` twin, folded to canonical text through `_resolved_content` at `of_encode` ingress. `make_email`'s twin is genuinely `make_make_email_data` (a doubled-verb upstream typo); `make_epc_qr` alone carries no `_data` twin.

| [INDEX] | [SURFACE]              | [CALL_SHAPE]                                              | [CAPABILITY]                                        |
| :-----: | :--------------------- | :-------------------------------------------------------- | :-------------------------------------------------- |
|  [01]   | `make_vcard_data`      | `make_vcard_data(name, displayname, *fields) -> str`      | 26-field vCard; `Content.vcard` `VCardFields`       |
|  [02]   | `make_mecard_data`     | `make_mecard_data(name, *fields) -> str`                  | 16-field MeCard; `Content.mecard` `MeCardFields`    |
|  [03]   | `make_wifi_data`       | `make_wifi_data(ssid, password, security, hidden) -> str` | WiFi join; `Content.wifi` 4-field, already complete |
|  [04]   | `make_geo_data`        | `make_geo_data(lat, lng) -> str`                          | `geo:` URI; `Content.geo` `(lat, lng)`              |
|  [05]   | `make_make_email_data` | `make_make_email_data(to, cc, bcc, subject, body) -> str` | `mailto:`; `Content.email` 5-field                  |
|  [06]   | `make_epc_qr`          | `make_epc_qr(name, iban, amount, *fields) -> QRCode`      | SEPA EPC payment symbol; no `_data` twin            |
|  [07]   | `helpers.quote`        | `quote(s) -> str`                                         | value-escaper the `_data` forms compose             |

[ENTRYPOINT_SCOPE]: `QRCode` render and inspect

`save` selects the serializer by filename extension or explicit `kind`, with serializer kwargs flowing through `**kw`; the read properties carry the symbol metadata the `RasterFact.score` records, and `QRCodeSequence` exposes only `save`/`terminal` over the whole span.

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

`save`/`kind` dispatch over one in-package serializer registry (`_VALID_SERIALIZERS`), every kind a row on the single `save` surface, never a parallel symbol type. Text-mode kinds (`eps`/`txt`/`tex`/`ans`/`xbm`/`xpm`) write `str` and need a text sink, while the codec's `kind="svg"` `BytesIO` path is bytes-native; vector and text kinds need no raster dependency, and `png` is the one raster kind yet still pure-Python. `QRCode.terminal()` is the separate render-to-stdout surface the `ans` `save` kind mirrors, and `svg_debug` is the standalone `write_svg_debug` writer, not a `save` kind.

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
- [01]-[SVG]: `xmldecl`, `svgns`, `title`, `desc`, `svgid`, `svgclass='segno'`, `lineclass='qrline'`, `omitsize`, `unit`, `encoding='utf-8'`, `svgversion`, `nl`, `draw_transparent`, and the `colormap` per-module colors (`finder_dark`/`data_dark`/`alignment_dark`/`timing_dark`/`version_dark`/`format_dark`/`dark_module`/`separator`/`quiet_zone` with `*_light` twins).
- [02]-[PNG]: `colormap`, `compresslevel=9`, `dpi`.
- [03]-[PDF] / [04]-[EPS]: `dark='#000'`, `light`.
- [05]-[TEX]: `dark='black'`, `unit='pt'`, `url`.
- [06]-[NETPBM]: `pam`/`ppm`: `dark`/`light` (+ `colormap` for `ppm`); `pbm`: `plain`.
- [07]-[XBM_XPM]: `xbm`: `name='img'`; `xpm`: `dark`/`light`.
- [08]-[TXT]: `dark='1'`, `light='0'`.
- [09]-[ANS]: `border`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- factory axis: one `getattr(segno, factory)` spread over the `SegnoFactory` member owns symbol encoding — `make`/`make_micro`/`make_sequence` are three `SYMBOLOGIES` rows (`Symbology.QR`/`MICRO_QR`/`QR_SEQUENCE`) each binding `EncodeArm(qr=(SegnoFactory, accepts))`; `make_qr` is the `make` row with `micro=False`. `SHARED_FACTORY_KEYS` threads the six common parameters and the per-row `accepts` the factory-specific keys, key-filtered so an over-key never reaches a rejecting factory.
- payload axis: the `Content` closed family (`raw`/`wifi`/`vcard`/`mecard`/`geo`/`email`) folds to canonical text through `_resolved_content` at `of_encode` ingress exactly once — the `vcard`/`mecard` cases spread the closed `VCardFields` (26) / `MeCardFields` (16) `TypedDict` as `**fields`, the `wifi`/`geo`/`email` cases already complete over their helper's 4/2/5 parameters.
- render axis: `symbol.save(BytesIO(), kind="svg", **SvgStyle)` is the single serializer surface keyed by `kind`; the closed `SvgStyle` `TypedDict` threads the per-module `colormap` colors and the full structural SVG axis as `**opts.get("svg", frozendict())`. `matrix`/`matrix_iter` feed a custom renderer only when no built-in kind fits.
- error-correction axis: `error` selects the L/M/Q/H redundancy row by deployment surface; `boost_error=True` raises the level whenever spare capacity admits it without growing the version. Both ride the `MarkPayload` keys through `SHARED_FACTORY_KEYS`.
- sequence axis: `make_sequence` spans a `QRCodeSequence`, resolved with one `QRCodeSequence.save(kind="svg")` keyed by `symbol_count`; the `_segno_score` `factory is SegnoFactory.MAKE_SEQUENCE` guard routes the sequence to `len(QRCodeSequence)` (the `SYMBOLS` fact) so it never reaches the `QRCode`-only `designator`/`symbol_size` surface.
- fault axis: `DataOverflowError` maps onto `MarkFault.overflow` carrying the `Symbology`, an out-of-range `version`/`mask`/`mode`/`error` `ValueError` onto `MarkFault.parameter`, a serializer `ValueError` (rejected `SvgStyle`, e.g. `omitsize` with `unit`) onto `MarkFault.render`, and a `segno.helpers` payload-format `ValueError`/`TypeError` onto `MarkFault.content` — each named exactly at the segno arm.
- evidence: each symbol stamps `designator`/`version`/`error`/`mask`/`mode`/`symbol_size` onto the shared `RasterFact.score` `frozendict` keyed by the `MarkFact` vocabulary (the sequence stamps the symbol count); the SVG path reports zero pixel dimensions, projected to `core/receipt#RECEIPT` `ArtifactReceipt.Preview(key, 0, 0, scores)` at the boundary.

[STACKING]:
- `expression`/`pydantic`/`msgspec`/`beartype`/`anyio`/`structlog` (`.api/` substrate): the SVG bytes from `symbol.save(BytesIO(), kind="svg", **SvgStyle)` cross the seam as a `Result[RasterFact, MarkFault]`, the `MarkPayload`/`SvgStyle`/`Content` bands are admitted once through `_PAYLOAD = TypeAdapter(MarkPayload)` and a `ValidationError` maps onto `MarkFault.options` as the `.errors()` `loc` `tuple[str, ...]`, the closed `Symbology`/`SegnoFactory`/`SegnoKey`/`MarkFault`/`Content` vocabularies and `RasterFact` are `msgspec`/`tagged_union` shapes, the `_encode` worker is `beartype`-contracted and offloaded under the one `_MARK_LANE` `CapacityLimiter` via `anyio.to_thread`, and `RasterFact.score` threads onto `ArtifactReceipt.Preview.scores` for the receipt.
- `svgelements`(`.api/svgelements.md`): the dependency-free SVG markup parses through `SVG.parse(BytesIO(svg), reify=True).bbox()` for scale-to-fit / n-up / `bbox()` query before document egress (the `Mark.layered` `_mark_bbox` projection, a parse failure railed onto `MarkFault.geometry`), landing a QR in the same figure-composition rail as a python-barcode SVG, a `great-tables` table SVG, and a `vl-convert` chart SVG.
- `python-barcode`(`.api/python-barcode.md`) + `zxing-cpp`(`.api/zxing-cpp.md`): the `qr`, `linear`, and `matrix` arms share the `RasterFact` -> `ArtifactReceipt.Preview` shape, so a mixed QR + linear + 2D-matrix label sheet folds through one `Mark.over(MarkOp | Iterable[MarkOp])` into one `Block[ArtifactReceipt]` stream where the segno `designator`/`version`/`error`/`mask`/`mode`/`symbol_size`, the python-barcode `get_fullcode` check digit, and the zxing resolved `format`/`ec_level` all land on one `Preview.scores` `frozendict[str, float | str]` band (also carrying the `graphic/raster/measure#MEASURE` perceptual `float` band).
- `stream-zip`(`.api/stream-zip.md`): the `save(BytesIO(), kind="svg")` stream feeds a `MemberFile` data iterable, so a batch of QR symbols streams into a ZIP label bundle without buffering whole files; `png_data_uri()` / `svg_data_uri()` produce inline `data:` URIs for a single self-contained document tree, avoiding a separate asset write.
- within-lib: `segno.helpers.make_*_data` returns formatted `str` the `Content` family carries typed once at ingress, so the QR encoding is a pure transform; the encode-correctness round-trip segno cannot express is a `zxingcpp.read_barcodes` decode pass on the rendered raster, owned by the zxing-cpp `matrix` arm (`graphic/marks/decode#DECODE`), the `Mark` owner providing it across arms.

[LOCAL_ADMISSION]:
- lazy `import segno` (and `from segno import helpers`) at the `encode#MARK` boundary; module-level import violates the manifest import policy, and the annotation-only `from segno import QRCode, QRCodeSequence` rides the `if TYPE_CHECKING` block.
- segno is strictly QR/Micro-QR — DataMatrix/PDF417/Aztec/MaxiCode/rMQR route to the zxing-cpp `matrix` arm and the linear symbologies to python-barcode; the dependency-free SVG bytes feed the `svgelements`/document figure owners with no rasterization, and live UI stays outside this package.

[RAIL_LAW]:
- Package: `segno`
- Owns: QR (versions 1-40) and Micro-QR (M1-M4) generation, L/M/Q/H error correction with `boost_error` auto-raise, numeric/alphanumeric/byte/kanji/hanzi/ECI segment-mode and explicit data-mask control, structured-append `QRCodeSequence` spans, the `segno.helpers` structured-payload grammar (vCard/MeCard/WiFi/EPC/geo/email + `_data` twins), the boolean module matrix (`matrix`/`matrix_iter`), the `designator`/`version`/`error`/`mask`/`mode`/`symbol_size`/`is_micro` evidence surface, and dependency-free SVG/PNG/PDF/EPS/Netpbm/LaTeX/text/terminal serialization and `svg_inline`/`svg_data_uri`/`png_data_uri` inline forms
- Accept: `getattr(segno, factory)` resolution over the `SegnoFactory` member with the six `SHARED_FACTORY_KEYS` plus per-row `SegnoKey` accepts, `make_sequence` keyed by `symbol_count`, the `Content` family folding through `helpers.make_*_data` (the genuinely-spelled `make_make_email_data` included), `symbol.save(BytesIO(), kind="svg", **SvgStyle)` SVG bytes folding to `RasterFact`, the nested closed `SvgStyle` per-module-color-plus-structural band on `MarkPayload`, the `DataOverflowError`/`ValueError` raises mapped onto distinct `MarkFault` cases, SVG bytes feeding the `svgelements`/document figure owners and the `stream-zip` bundle sink
- Reject: wrapper-renames of `make`/`save`; a hand-rolled Reed-Solomon QR encoder; a hand-formatted vCard/WiFi/MeCard/geo/email string where `segno.helpers` owns the grammar; a thin slice of a contact grammar the helper carries in full at 26 fields; the `QRCode`-returning `make_*` where the `_data` twin folds to text through the one factory axis; a forced Pillow raster path where the in-package serializers (PNG included) need none; a `dark`/`light`-only slice of the full `SvgStyle` surface; a parallel symbol type per output format; an unwrapped `save` letting a serializer `ValueError` escape the encode capsule; a bare `except Exception` flattening the `DataOverflowError`/`ValueError` causes; a `VERSION_RANGE_*` reference where the real names are the `01_09`/`10_26`/`27_40` triple; a claimed 2D-matrix or linear symbology this package does not own; identity minting the runtime owns
