# [PY_ARTIFACTS_API_ZXING_CPP]

`zxing-cpp` (dist `zxing-cpp`, import `zxingcpp`) supplies the 2D-matrix encoded-mark surface for the artifacts imaging rail beside the segno QR arm and the python-barcode linear arm: a `create_barcode(content, format, **kwargs)` writer that builds a `Barcode` for DataMatrix/PDF417/Aztec/MaxiCode (and QR/Micro-QR/linear), a dependency-free `Barcode.to_svg` serializer, and a `read_barcodes(image) -> list[Barcode]` decoder closing the encode/decode round-trip the segno and python-barcode arms cannot. The package owner composes `create_barcode`, `to_svg`, and `read_barcodes` into the `PreviewOp.MARK`/`PreviewOp.DECODE` 2D-matrix path; it reads the widest symbology set of any wrapped OSS encoder, and the matrix concern (DataMatrix/PDF417/Aztec/MaxiCode) is dropped from python-barcode and routed here rather than re-implemented.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `zxing-cpp`
- package: `zxing-cpp`
- import: `zxingcpp` (dist name `zxing-cpp`, import name `zxingcpp`)
- owner: `artifacts`
- rail: imaging
- license: `Apache-2.0` (OSI); native pybind11 extension over the ZXing-C++ library, distributed as a compiled wheel
- asset: the SVG path (`to_svg`/`write_barcode_to_svg`) needs no dependency; the raster path (`to_image`/`write_barcode_to_image`) yields a zxing `Image` and decode accepts a numpy BGR/grayscale array, a PIL image, a buffer, or a `zxingcpp.ImageView`; numpy/Pillow are admitted manifest packages, never a per-package pin
- installed: `3.0.0` reflected via direct import over the cp314-bound distribution
- band: gated `python_version<'3.15'` — wheels cap at cp314 with no cp315 wheel, so the owner imports it only inside the `faults`-owned `anyio.to_process.run_sync` companion subprocess worker (the same gated band as `pillow`/`scikit-image`), never on the cp315-core owner
- entry points: library use is import-only; the surface is module-level functions plus the `Barcode`/`BarcodeFormat`/`BarcodeFormats`/`Position`/`Point`/`Error` types
- capability: encode and decode for the widest wrapped symbology set — 2D-matrix DataMatrix/PDF417/Compact PDF417/MicroPDF417/Aztec/MaxiCode/QR/Micro-QR/rMQR plus linear Code128/Code39/Code93/Codabar/ITF/EAN/UPC/DataBar — with dependency-free SVG output, per-symbol error-correction and geometry knobs, full decode metadata (text, format, position, orientation, error-correction level, content type, symbology identifier, raw bytes), and an encode/decode round-trip

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: barcode value and symbology enum
- rail: imaging

`Barcode` is the single value both `create_barcode` (write) and `read_barcode`/`read_barcodes` (read) yield; the write path populates `format`/`text`/`bytes`/`to_svg`/`to_image`, and the read path additionally populates `valid`/`position`/`orientation`/`error`/`content_type`/`symbology_identifier`/`ec_level`/`extra`. `BarcodeFormat` is the symbology enum; in 3.0 `str(BarcodeFormat.X)` returns the human display name WITH separators (`'Data Matrix'`, `'QR Code'`, `'Code 128'`, `'EAN-13'`, `'Micro QR Code'`) while `.name` is the separatorless identifier (`'DataMatrix'`, `'QRCode'`, `'Code128'`, `'EAN13'`) — resolve a format through `barcode_format_from_str` or the enum attribute, never by formatting the enum to a string and re-parsing it. `BarcodeFormat` also carries set-valued members (`AllMatrix`/`AllLinear`/`AllCreatable`/`AllReadable`/`All`) usable as decode scopes.

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]    | [CAPABILITY]                                                                                                                          |
| :-----: | :------------------------ | :--------------- | :---------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Barcode`                 | barcode value    | the create/decode result; read props `text`/`bytes`/`format`/`symbology`/`valid`/`position`/`orientation`/`error`/`content_type`/`symbology_identifier`/`ec_level`/`extra`, write methods `to_svg`/`to_image` |
|  [02]   | `BarcodeFormat`           | symbology enum   | symbology selector; matrix `DataMatrix`/`PDF417`/`CompactPDF417`/`MicroPDF417`/`Aztec`/`MaxiCode`/`QRCode`/`MicroQRCode`/`RMQRCode`, linear `Code128`/`Code39`/`Code93`/`Codabar`/`ITF`/`EAN13`/`EAN8`/`UPCA`/`UPCE`/`DataBar*`, set values `AllMatrix`/`AllLinear`/`AllCreatable`/`AllReadable`/`All` |
|  [03]   | `BarcodeFormats`          | format set       | the plural decode scope `read_barcodes(formats=)` accepts; build from `barcode_formats_from_str("Data Matrix,QR Code")` or a tuple of `BarcodeFormat` (the `\|` operator is deprecated in 3.0) |
|  [04]   | `Position`                | quad position    | the decoded quad; `.top_left`/`.top_right`/`.bottom_right`/`.bottom_left` corner `Point`s, `str(position)` = `"x0xy0 x1xy1 x2xy2 x3xy3"` |
|  [05]   | `Point`                   | corner point     | integer `.x`/`.y` of one `Position` corner                                                                                          |
|  [06]   | `Error`                   | decode fault     | the `Barcode.error` value (or `None`); `ErrorType` members `Checksum`/`Format`/`Unsupported`/`None`                                  |
|  [07]   | `ContentType`             | content enum     | `Text`/`Binary`/`Mixed`/`GS1`/`ISO15434`/`UnknownECI` classifying the decoded payload                                               |
|  [08]   | `Image` / `ImageView`     | raster carriers  | `to_image` output (`Image`) and the custom-stride decode input view (`ImageView` over `ImageFormat` `RGB`/`BGR`/`RGBA`/`BGRA`/`Lum`) |
|  [09]   | `TextMode` / `Binarizer` / `EanAddOnSymbol` | decode policy enums | `TextMode` `HRI`/`Plain`/`ECI`/`Escaped`/`Hex`, `Binarizer` `LocalAverage`/`GlobalHistogram`/`FixedThreshold`/`BoolCast`, `EanAddOnSymbol` `Ignore`/`Read`/`Require` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: encode (create + serialize)
- rail: imaging

`create_barcode` builds a `Barcode` from string or bytes content for any creatable format; error-correction and geometry ride `**kwargs` (`ec_level` a string `'L'`/`'M'`/`'Q'`/`'H'` for QR or an integer for Aztec/PDF417, plus `width`/`height`/`scale`/`margin`), never positional arguments. `Barcode.to_svg` serializes a dependency-free SVG string; `Barcode.to_image` and the `write_barcode_to_*` module functions are the equivalent free-function forms over the same `(scale, add_hrt, add_quiet_zones)` axis.

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]                                                                                          | [CAPABILITY]                                                       |
| :-----: | :------------------------- | :--------------------------------------------------------------------------------------------------- | :----------------------------------------------------------------- |
|  [01]   | `create_barcode`           | `create_barcode(content, format: BarcodeFormat, **kwargs) -> Barcode`                                | encode content; `kwargs` carry `ec_level`/`width`/`height`/`scale`/`margin` |
|  [02]   | `Barcode.to_svg`           | `to_svg(scale=1, add_hrt=False, add_quiet_zones=True) -> str`                                         | dependency-free SVG string                                         |
|  [03]   | `Barcode.to_image`         | `to_image(scale=1, add_hrt=False, add_quiet_zones=True) -> Image`                                     | raster `Image` for byte encode                                     |
|  [04]   | `write_barcode_to_svg`     | `write_barcode_to_svg(barcode, scale=1, add_hrt=False, add_quiet_zones=True) -> str`                  | free-function form of `to_svg`                                     |
|  [05]   | `write_barcode_to_image`   | `write_barcode_to_image(barcode, scale=1, add_hrt=False, add_quiet_zones=True) -> Image`              | free-function form of `to_image`                                   |
|  [06]   | `barcode_format_from_str`  | `barcode_format_from_str(str) -> BarcodeFormat`                                                       | resolve a format from `'EAN13'` or `'EAN-13'` (both accepted)      |

[ENTRYPOINT_SCOPE]: decode (read)
- rail: imaging

`read_barcodes` decodes every symbol in an image and returns a `list[Barcode]`; `read_barcode` returns the first `Barcode | None`. The `formats` scope narrows the search to one `BarcodeFormat`, a `BarcodeFormats`, or `barcode_formats_from_str(...)`; the remaining keyword axis (`try_rotate`/`try_downscale`/`try_invert`/`text_mode`/`binarizer`/`is_pure`/`ean_add_on_symbol`/`return_errors`) tunes the detector. Each decoded `Barcode` carries `text`, `format`, `valid`, `position`, `orientation`, `error`, `content_type`, `symbology_identifier`, `ec_level`, and `bytes`.

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]                                                                                          | [CAPABILITY]                                                       |
| :-----: | :------------------------- | :--------------------------------------------------------------------------------------------------- | :----------------------------------------------------------------- |
|  [01]   | `read_barcodes`            | `read_barcodes(image, formats=BarcodeFormats(), try_rotate=True, try_downscale=True, try_invert=True, text_mode=TextMode.HRI, binarizer=Binarizer.LocalAverage, is_pure=False, ean_add_on_symbol=EanAddOnSymbol.Ignore, return_errors=False) -> list[Barcode]` | decode every symbol in the image |
|  [02]   | `read_barcode`             | `read_barcode(image, formats=..., ...) -> Barcode \| None`                                            | decode the first symbol (same keyword axis)                        |
|  [03]   | `barcode_formats_from_str` | `barcode_formats_from_str(str) -> BarcodeFormats`                                                     | parse a `"Data Matrix,QR Code"` decode scope                       |
|  [04]   | `barcode_formats_list`     | `barcode_formats_list() -> str`                                                                       | the comma-joined display-name list of every known format           |

## [04]-[IMPLEMENTATION_LAW]

[IMAGING_MATRIX]:
- import: `import zxingcpp` at boundary scope ONLY, inside the gated-band companion subprocess worker the `faults`-owned `anyio.to_process.run_sync` dispatches; never a module-level or cp315-core import, because the wheel caps at cp314 and resolves no cp315 wheel. The dist name is `zxing-cpp`; the import name is `zxingcpp`.
- format axis: one `BarcodeFormat` enum keys symbology selection on both encode and decode — `create_barcode(content, BarcodeFormat.DataMatrix)` and `read_barcodes(image, formats=BarcodeFormat.DataMatrix)`. In 3.0 the enum `str()` is the human display name with separators (`'Data Matrix'`/`'QR Code'`/`'Code 128'`/`'EAN-13'`) while `.name` is separatorless (`'DataMatrix'`/`'QRCode'`/`'Code128'`/`'EAN13'`); resolve a format through the enum attribute or `barcode_format_from_str` (which accepts both `'EAN13'` and `'EAN-13'`), never by formatting the enum to a string and re-parsing the separatorless name — the 3.0 ToString rename breaks that pattern.
- creatable axis: 3.0 encodes DataMatrix, PDF417 (and Compact PDF417), Aztec, QR, Micro-QR, AND MaxiCode — MaxiCode is creatable in 3.0 (the 2.x decode-only limitation was lifted by the ZXing-C++ writer rewrite), verified by a successful `create_barcode("...", BarcodeFormat.MaxiCode).to_svg()`; `BarcodeFormat.AllCreatable` names the full encode set. Linear EAN/UPC/Code128/ITF also encode but overlap python-barcode's owned domain, so the admitted scope of this package on the preview rail is the 2D-matrix concern python-barcode drops.
- option axis: `create_barcode(**kwargs)` carries `ec_level` (a string `'L'`/`'M'`/`'Q'`/`'H'` for QR, an integer for Aztec/PDF417 — an out-of-range string raises `ValueError("Invalid ecLevel: ...")`), plus `width`/`height`/`scale`/`margin`; never a positional `ec_level` argument and never a per-format encoder function. The serialize axis (`scale`/`add_hrt`/`add_quiet_zones`) lives on `to_svg`/`to_image`, not on `create_barcode`.
- decode axis: `read_barcodes` returns a `list[Barcode]` and `read_barcode` the first `Barcode | None`; scope the search with `formats=` (a single `BarcodeFormat`, a `BarcodeFormats`, or `barcode_formats_from_str("Data Matrix,QR Code")` — the `\|` union operator is deprecated in 3.0, pass a tuple/array or parse a string instead). Each decoded `Barcode` yields `text`, `format`, `valid`, `position` (a four-corner `Position`), `orientation`, `error` (an `Error` or `None`), `content_type`, `symbology_identifier`, `ec_level`, and `bytes`; map a non-`valid` symbol or a non-`None` `error` at the boundary rather than trusting `text`.
- round-trip axis: `create_barcode(content, fmt).to_image(scale=N)` then `read_barcodes(image)` recovers the content with `valid=True` and the matching `format`, so the encoded mark carries a generation-correctness proof from one decode pass — the round-trip a QR-only or linear-only owner cannot express. The decode-confidence fact (`valid`, `format`, `text`, `error`) lands on the imaging receipt.
- dimensionality axis: this package owns the 2D-matrix concern (DataMatrix/PDF417/Aztec/MaxiCode) plus the QR/Micro-QR and linear sets; on the artifacts preview rail it is admitted for the 2D-matrix symbologies python-barcode explicitly drops and the decode round-trip segno and python-barcode lack, never as a second QR or linear encoder displacing the segno/python-barcode arms.
- evidence: each encoded/decoded symbol captures format display name, content, error-correction level, validity, decode error, and output byte length as an imaging receipt fact.
- stacking: the `to_svg` string output composes directly into the document/figure owners (an inline `<svg>` fragment) with no rasterization, exactly like the segno SVG and the python-barcode `SVGWriter` fragment, so a mixed QR + linear + 2D-matrix label sheet folds into one imaging receipt stream; the `to_image` raster path stacks the same gated-band Pillow re-encode the `_gated_raster` arm already runs. One imaging owner discriminates on `(symbology-arm, sink)` rather than minting a per-format type, and the decode arm is the inverse no sibling provides.
- boundary: zxing-cpp owns the 2D-matrix encode/decode arm and the encode/decode round-trip; segno owns the QR/Micro-QR arm and python-barcode the linear arm beside it on the cp315 core, while this package rides the gated `python_version<'3.15'` companion subprocess seam; the SVG output feeds the document and figure owners directly; live UI stays outside this package.

[RAIL_LAW]:
- Package: `zxing-cpp`
- Owns: 2D-matrix DataMatrix/PDF417/Aztec/MaxiCode (plus QR/Micro-QR/rMQR and linear) encode through `create_barcode`, dependency-free SVG via `Barcode.to_svg`, raster via `Barcode.to_image`, multi-symbol decode via `read_barcodes`/`read_barcode` with full symbol metadata, and the encode/decode round-trip generation-correctness proof
- Accept: `create_barcode(content, BarcodeFormat.X, **kwargs)` with `ec_level`/`scale`/`margin` knobs, `to_svg(add_quiet_zones=)` dependency-free output, `read_barcodes(image, formats=)` scoped decode over numpy/PIL/buffer input, the `format`/`text`/`valid`/`position`/`error` decode facts mapped at the boundary, the gated `python_version<'3.15'` companion subprocess seam, and the SVG fragment feeding the document and figure owners
- Reject: wrapper-renames of `create_barcode`/`read_barcodes`; a positional `ec_level` argument (it is a `**kwargs` key); formatting the `BarcodeFormat` enum to a string and re-parsing the separatorless name (the 3.0 `str()` is the separated display name — use `barcode_format_from_str` or the enum attribute); the deprecated `\|` format-union operator; a claim that MaxiCode is decode-only (3.0 encodes it); trusting decode `text` without checking `valid`/`error`; a second QR or linear encoder displacing the segno/python-barcode arms; a module-level or cp315-core import of the gated wheel
