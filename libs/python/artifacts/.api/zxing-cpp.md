# [PY_ARTIFACTS_API_ZXING_CPP]

`zxing-cpp` (import `zxingcpp`) owns the 2D-matrix encoded-mark surface (DataMatrix/PDF417/Aztec/MaxiCode) AND the sole decode inverse across the `graphic/marks` rail: it is the `matrix` `EncodeArm` of `graphic/marks/encode#MARK` — the symbologies the segno `qr` and python-barcode `linear` arms drop — and the only owner of `graphic/marks/decode#DECODE`, closing the encode/decode round-trip neither generation-only arm expresses.


## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `zxing-cpp`
- package: `zxing-cpp` (Apache-2.0)
- import: `zxingcpp`
- owner: `artifacts`
- rail: imaging — the `graphic/marks/encode#MARK` `matrix` `EncodeArm` arm AND the `graphic/marks/decode#DECODE` owner, the one owner spanning both rail directions
- asset: compiled C++20 extension (not pure-Python, not abi3), so a per-interpreter build binds; `Barcode.to_svg` is dependency-free, `Barcode.to_image` yields an 8-bit grayscale `Image` exporting the buffer protocol for zero-copy `numpy.array`, and decode admits a numpy BGR/grayscale array, a PIL image, a `collections.abc.Buffer`, or an `ImageView`
- entry points: import-only; module-level functions and the types in [02]/[03]

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: barcode value and symbology enum

`Barcode` is the single value both `create_barcode` (write) and `read_barcode`/`read_barcodes` (read) yield: the write path populates `format`/`text`/`bytes`, the read path adds `valid`/`position`/`orientation`/`error`/`content_type`/`symbology_identifier`/`ec_level`/`extra`.

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]  | [CAPABILITY]                                                                                        |
| :-----: | :--------------- | :------------- | :-------------------------------------------------------------------------------------------------- |
|  [01]   | `Barcode`        | result value   | create+decode result; `to_svg`/`to_image` writers + the read props (lead)                           |
|  [02]   | `BarcodeFormat`  | symbology enum | per-format selector; `.name` id, `str()` display, `.value` int, `.symbology` family                 |
|  [03]   | `BarcodeFormats` | format set     | plural decode scope; from `barcode_formats_from_str`, an `All*` member, or `barcode_formats_list`   |
|  [04]   | `Position`       | quad position  | `.top_left`/`.top_right`/`.bottom_right`/`.bottom_left` `Point`s; `str()` = `{x}x{y}` quad          |
|  [05]   | `Point`          | corner point   | integer `.x`/`.y` of one `Position` corner                                                          |
|  [06]   | `Error`          | decode fault   | `Barcode.error` or `None`; `.type` an `ErrorType`, `.message` the human string                      |
|  [07]   | `Image`          | raster out     | `to_image` output — 8-bit grayscale, `.shape` + buffer protocol                                     |
|  [08]   | `ImageView`      | frame view     | `ImageView(buffer, width, height, format, row_stride=0, pix_stride=0)` — custom-stride decode input |

[PUBLIC_TYPE_SCOPE]: decode-fault, content, and policy enums

| [INDEX] | [ENUM]           | [VALUES]                                                                                  |
| :-----: | :--------------- | :---------------------------------------------------------------------------------------- |
|  [01]   | `ErrorType`      | `Checksum`/`Format`/`Unsupported`/`None` — classifies `Error.type`                        |
|  [02]   | `ContentType`    | `Text`/`Binary`/`Mixed`/`GS1`/`ISO15434`/`UnknownECI` — decoded-payload class             |
|  [03]   | `ImageFormat`    | `RGB`/`BGR`/`RGBA`/`BGRA`/`ABGR`/`ARGB`/`Lum`/`LumA` — declared `ImageView` channel order |
|  [04]   | `TextMode`       | `HRI`/`Plain`/`ECI`/`Escaped`/`Hex`/`HexECI` — decode text rendering                      |
|  [05]   | `Binarizer`      | `LocalAverage`/`GlobalHistogram`/`FixedThreshold`/`BoolCast` — frame thresholding         |
|  [06]   | `EanAddOnSymbol` | `Ignore`/`Read`/`Require` — EAN add-on handling                                           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: encode (create + serialize)
- every serializer (`to_svg`/`to_image`/`write_barcode_to_*`) carries `(scale=1, add_hrt=False, add_quiet_zones=True)`, elided below

`create_barcode` builds a `Barcode` from `str` (UTF-8, `content_type=Text`) or `bytes` (raw, `content_type=Binary`) content; the `CreatorOptions` axis rides `**kwargs` keyword-only. `Barcode.to_svg` serializes a dependency-free SVG string, `Barcode.to_image` an 8-bit grayscale `Image` whose buffer feeds `numpy.array` directly, and the `write_barcode_to_*` free functions are the module-function forms over the same axis.

| [INDEX] | [SURFACE]                                                                           | [CAPABILITY]                                       |
| :-----: | :---------------------------------------------------------------------------------- | :------------------------------------------------- |
|  [01]   | `create_barcode(content: str \| bytes, format: BarcodeFormat, **kwargs) -> Barcode` | encode content; sole creator key `ec_level`        |
|  [02]   | `Barcode.to_svg(...) -> str`                                                        | dependency-free SVG string                         |
|  [03]   | `Barcode.to_image(...) -> Image`                                                    | 8-bit grayscale; buffer -> `numpy.array` zero-copy |
|  [04]   | `write_barcode_to_svg(barcode, ...) -> str`                                         | free-function form of `to_svg`                     |
|  [05]   | `write_barcode_to_image(barcode, ...) -> Image`                                     | free-function form of `to_image`                   |
|  [06]   | `barcode_format_from_str(str) -> BarcodeFormat`                                     | resolve one format from `'EAN13'` or `'EAN-13'`    |

[ENTRYPOINT_SCOPE]: decode (read)

`read_barcodes` decodes every symbol to a `list[Barcode]`, `read_barcode` the first `Barcode | None`; both default `formats=BarcodeFormats()` (empty = all readable) and a detector axis (`try_rotate`/`try_downscale`/`try_invert`/`text_mode`/`binarizer`/`is_pure`/`ean_add_on_symbol`/`return_errors`, the `...` below). `return_errors=True` is load-bearing: without it an invalid symbol is silently dropped, with it the symbol returns carrying its `Error` as a typed per-symbol fault.

| [INDEX] | [SURFACE]                                                              | [CAPABILITY]                                             |
| :-----: | :--------------------------------------------------------------------- | :------------------------------------------------------- |
|  [01]   | `read_barcodes(image, formats=BarcodeFormats(), ...) -> list[Barcode]` | decode all symbols; numpy/PIL/`Buffer`/`ImageView` input |
|  [02]   | `read_barcode(image, formats=..., ...) -> Barcode \| None`             | decode the first symbol; same detector axis              |
|  [03]   | `barcode_formats_from_str(str) -> BarcodeFormats`                      | parse a `"Data Matrix,QR Code"` scope (both spellings)   |
|  [04]   | `barcode_formats_list(filter=BarcodeFormats()) -> list[BarcodeFormat]` | supported-format roster (filtered); capability probe     |
|  [05]   | `ImageView(buffer, width, height, format, row_stride=0, pix_stride=0)` | raw frame + declared channel order + custom stride       |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- format axis: one `BarcodeFormat` enum keys symbology selection on both encode and decode. `str(format)` is the human display name with separators (`'Data Matrix'`/`'QR Code'`/`'Code 128'`/`'EAN-13'`) while `.name` is separatorless (`'DataMatrix'`/`'QRCode'`); resolve a format through the enum attribute or `barcode_format_from_str` (which accepts both `'EAN13'` and `'EAN-13'`), never by formatting the enum and re-parsing the separatorless name. `Barcode.format` (precise) and `Barcode.symbology` (rolled-up family, `EAN13 -> EANUPC`, `MicroQRCode -> QRCode`) are distinct facts the decode owner records separately.
- creatable axis: `create_barcode` encodes DataMatrix, PDF417 (and Compact PDF417), Aztec, QR, Micro-QR, and MaxiCode; `BarcodeFormat.AllCreatable` names the encode set and `barcode_formats_list(BarcodeFormat.AllCreatable)` enumerates it. Linear EAN/UPC/Code128/ITF also encode but overlap python-barcode's owned domain, so this package's admitted preview scope is the 2D-matrix concern python-barcode drops.
- content axis: `str` encodes as UTF-8 (`content_type=Text`), `bytes` as raw binary (`content_type=Binary`); a binary DataMatrix/Aztec/PDF417 payload rides the `bytes` path and survives the seam as `Barcode.bytes` (carried as `raw`), never a lossy str round-trip.
- option axis: the sole verified creator key is `ec_level` (`'L'`/`'M'`/`'Q'`/`'H'` for QR, an integer-as-string for Aztec/PDF417; an out-of-range value raises `ValueError("Invalid ecLevel: ...")`), keyword-only. `**kwargs` silently ignores an unrecognized key, so geometry (`scale`/`width`/`height`/`margin`/`add_quiet_zones`) is a WRITER concern on `to_svg`/`to_image`/`write_barcode_to_*` — passed to `create_barcode` it is silently dropped, so the arm key-filters its band against the genuine creator keys. `Barcode.ec_level` is `''` where the symbology carries none (EAN/UPC/DataMatrix), recorded present-or-empty.
- decode axis: the owner collapses `read_barcode` into `read_barcodes` (a decode is plural over the raster; "first symbol" is the consumer's `[0]` projection). Scope with `formats=` (a `BarcodeFormat`, a set member, a `BarcodeFormats`, or `barcode_formats_from_str("Data Matrix,QR Code")`); the eight `All*` set members reach readable formats, and `BarcodeFormat` also carries the single-format members (`Code93`, `UPCE`, `DataBar`, `DataBarExpanded`, `DataBarLimited`, `MicroPDF417`, `DXFilmEdge`) — all resolvable live and creatable per `barcode_formats_list(BarcodeFormat.AllCreatable)`. Map a non-`valid` symbol or a non-`None` `error` at the boundary rather than trusting `text`.
- metadata axis: `Barcode.extra` is a `dict` of symbology-specific fields (QR yields `{'DataMask','Version','ECLevel'}`, DataMatrix `{'Version': '8x32'}`), captured stringified onto the per-symbol evidence. `Barcode.error` is `None` or a `zxingcpp.Error` whose `.type` (`ErrorType` `Checksum`/`Format`/`Unsupported`) maps onto the closed fault vocabulary and whose `.message` is the human string.
- frame axis: decode input is a numpy BGR/grayscale array (a bare three-channel array reads BGR, a four-channel array misreads), a PIL image, a `Buffer`, or an `ImageView(buffer, width, height, format, row_stride=0, pix_stride=0)` declaring channel order through `ImageFormat` with non-contiguous stride — the owner wraps a typed frame in an `ImageView` carrying its declared `PixelFormat -> ImageFormat`, never a shape-inferred guess. `to_image`'s grayscale buffer makes `numpy.array(image)` zero-copy, so the encode-then-decode raster never round-trips through PNG bytes.
- round-trip axis: `create_barcode(content, fmt).to_image(scale=N)` then `numpy.array` then `read_barcodes` recovers the content with `valid=True` and matching `format` in one decode pass.
- evidence: `RasterFact.score` carries the encoded and decoded symbol facts needed by the mark owner.

[STACKING]:
- decode substrate depth (`graphic/marks/decode#DECODE`): `DecodeScope` collapses the eight-argument `read_barcodes` detector tail into one frozen value keyed by a `ScopeKind` preset, its format scope an explicit `Symbology` tuple or a `FormatFamily` member resolved through `barcode_formats_from_str` / the `_FAMILY` set-value table; `DecodedSymbol` is a frozen `msgspec.Struct` over the eleven read properties doubling as its own `msgspec.json` wire form; the `Binarize`/`TextRead`/`EanAddOn`/`PixelFormat`/`FormatFamily` `StrEnum` vocabularies remap to the provider `Binarizer`/`TextMode`/`EanAddOnSymbol`/`ImageFormat`/`BarcodeFormat` enums through `frozendict` tables only at the call edge, and the `_admitted` core is `@beartype`-contracted at the C++-binding boundary.
- decode worker seam: an untrusted raster rides a crash-isolated `anyio.to_process.run_sync` worker (a PIL `UnidentifiedImageError`/`DecompressionBombError`/`OSError` converting to `UNREADABLE`) while a trusted `numpy` frame rides `anyio.to_thread.run_sync` through `ImageView` (a wrong-rank/dtype/channel frame converting to `MALFORMED`), both under the one `_MARK_LANE` `CapacityLimiter`.
- `svgelements`(`.api/svgelements.md`) + `stream-zip`(`.api/stream-zip.md`): the dependency-free SVG parses through `SVG.parse(BytesIO(svg)).bbox()` for scale-to-fit / n-up before egress, and `to_svg`/`to_image` write into the `io.BytesIO` sink feeding a `stream-zip` `MemberFile` iterable, so a batch of matrix symbols streams into a ZIP label bundle without buffering — the segno/python-barcode bundle path.
- decode round-trip: the round-trip the segno `qr` and python-barcode `linear` arms cannot express is this owner's — a segno QR or python-barcode linear raster is proven correct by a `zxingcpp.read_barcodes` pass (`valid=True`, matching `format`), so the `Mark` owner folds it across arms through the zxing decode body; one imaging owner discriminates on `(EncodeArm, kind)` and `MarkOp.Decode`, never a per-format type or per-symbology decode entrypoint.

[LOCAL_ADMISSION]:
- lazy `import zxingcpp` at the `graphic/marks/encode#MARK` / `graphic/marks/decode#DECODE` boundary; module-level import violates the manifest import policy, and the annotation-only type imports ride the `if TYPE_CHECKING` block.
- zxing-cpp is the 2D-matrix `matrix` arm and the sole decode owner — QR/Micro-QR encode routes to segno and the linear symbologies to python-barcode; the dependency-free SVG bytes feed the `svgelements`/document figure owners with no rasterization, and the decode body is the round-trip proof across all three arms.

[RAIL_LAW]:
- Package: `zxing-cpp`
- Owns: 2D-matrix DataMatrix/PDF417/Compact-PDF417/Aztec/MaxiCode (with QR/Micro-QR/rMQR and the linear set) encode through `create_barcode` (`str` or `bytes` content), dependency-free SVG via `Barcode.to_svg`, 8-bit grayscale raster via `Barcode.to_image` (buffer-protocol numpy zero-copy), multi-symbol decode via `read_barcodes` with full per-symbol metadata (`text`/`bytes`/`format`/`symbology`/`valid`/`position`/`orientation`/`error`/`content_type`/`symbology_identifier`/`ec_level`/`extra`), set-valued `BarcodeFormat` family scopes and the `barcode_formats_list` roster, the `ImageView` custom-channel-order + custom-stride frame intake, and the encode/decode round-trip generation-correctness proof
- Accept: `create_barcode(content, format, **kwargs)` with `ec_level` key-filtered against the admitted band; `barcode_format_from_str`/`barcode_formats_from_str` resolution over both separator spellings; `read_barcodes(frame, formats=, ..., return_errors=True)` with the detector axis collapsed to one frozen `DecodeScope`; the eight `All*` set members as format scopes; an `ImageView` carrying the declared channel order; `Error.type`/`Error.message` and `ContentType` mapped onto closed fault/content vocabularies; `to_svg().encode()` / `to_image()` bytes folding to `RasterFact`, feeding the `svgelements`/document figure owners and the `stream-zip` bundle sink; the off-loop `to_thread`/`to_process` worker seam under the `_MARK_LANE` `CapacityLimiter`
- Reject: wrapper-renames of `create_barcode`/`read_barcodes`; conflating `Barcode.format` (precise) with `Barcode.symbology` (family); formatting `BarcodeFormat` to a string and re-parsing the separatorless name; a `read_barcode`/`read_barcodes` sibling pair where the decode is plural and "first" is the `[0]` projection; `return_errors=False` dropping invalid symbols where a typed per-symbol fault is wanted; a shape-inferred BGR/grayscale guess where the frame's channel order is a declared `ImageView` `ImageFormat`; asserting an `ec_level` on a format that returns `''`; a `text|format|valid|position` score cram dropping `bytes`/`content_type`/`orientation`/`symbology_identifier`/`extra`; a hand-rolled 2D-matrix encoder/decoder where zxing owns the symbology; a forced PNG-bytes round-trip where the grayscale `Image` buffer is numpy-zero-copy; a claimed QR-only or linear-only ownership displacing the segno `qr` and python-barcode `linear` arms; identity minting the runtime owns
