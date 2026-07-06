# [PY_ARTIFACTS_GRAPHIC_MARKS_MARK]

The machine-readable-mark VOCABULARY owner — the s1 neutral floor of the marks plane. One closed `Symbology` taxonomy spans the QR/Micro-QR/structured-append class, the linear 1D registry class, and the 2D-matrix class; the `TAXONOMY` table binds every member to its `MarkClass` and its physical zxing carrier display name — the decode-direction spelling `barcode_formats_from_str` parses, with the content-discipline aliases (ISBN-10/ISBN-13/ISSN/EAN-14 over the EAN-13 carrier, PZN over Code39, GS1-128 over Code128) folded onto the carrier that physically renders and decodes them, so neither behavior page hand-spells a provider format name. The `MarkPayload` closed option band with its nested `SvgStyle`/`WriterOptions` surfaces, the `DecodeSource` raster-bytes/typed-pixel-frame source family carrying its `PixelFormat` channel order, and the `MarkFault` closed fault vocabulary every provider raise on the plane maps into complete the floor.

`graphic/marks/encode` and `graphic/marks/decode` BOTH import this page and NEITHER imports the other — the encode→decode `_zxing_decode` reach and the decode→encode `RasterFact, Symbology` reach are the dead edge pair this owner replaces. Behavior lives above: the segno/python-barcode/zxing generation arms and their `EncodeArm` dispatch are encode's, the `read_barcodes` detector policy (`DecodeScope` and its token vocabularies) is decode's, and the canonical `RasterFact` both fold into is `graphic/raster/process`'s — never re-declared here.

## [01]-[INDEX]

- [01]-[MARK]: the marks-plane neutral vocabulary — `Symbology` (22 members over three `MarkClass`es), the total `TAXONOMY` class-and-carrier table, the `MarkPayload`/`SvgStyle`/`WriterOptions` closed option bands, the `DecodeSource` source family with its `PixelFormat` channel vocabulary, and the closed 13-case `MarkFault` — imported downward by encode and decode, importing nothing artifacts-internal, minting no receipt.

## [02]-[MARK]

- Owner: one vocabulary floor, zero behavior. `Symbology` is the closed mark identity every plane surface keys on — encode's dispatch table, decode's format scope, and every evidence stamp spell the same 22 members. `MarkClass` names the three generation families (`QR` the segno factories, `LINEAR` the python-barcode registry, `MATRIX` the zxing creatables); `TAXONOMY` is the one total `Map[Symbology, tuple[MarkClass, str]]` binding each member to its class and its physical zxing carrier — the carrier column is the decode-direction display name (total, so a scoped decode never KeyErrors), and its `MATRIX` rows are exactly the encode-direction creatable set, one spelling serving both directions.
- Cases: `DecodeSource` is the closed source family — `Raster(payload)` the encoded raster bytes whose untrusted decode earns crash isolation, `Pixels(frame, fmt)` a `numpy` `uint8` frame with its declared `PixelFormat` channel order so the consumer constructs a `zxingcpp.ImageView` over the true layout, never a shape-inferred BGR/grayscale guess. The offload band is the case, never an `engine`/`gated` knob; which lane each case rides is the dispatching page's law.
- Payload: `MarkPayload` is the one closed option band crossing the plane's admission seam — the segno factory axis (`error`/`version`/`mode`/`mask`/`encoding`/`boost_error`/`eci`/`micro`/`symbol_count`), the zxing create axis (`ec_level`), the shared render axis (`scale`/`border`/`margin`/`dark`/`light`/`add_hrt`/`add_quiet_zones`), the python-barcode text/geometry axis (`text`/`writer_options`), and the segno SVG serializer band (`svg`) — with `SvgStyle` (the 15 per-module color keys plus the 13 structural keys) and `WriterOptions` (the 15 linear geometry keys) as its nested closed surfaces. Admission behavior (the `TypeAdapter` ingress and the frozendict deep-fold) is encode's; the shape is law here.
- Faults: `MarkFault` is the one closed fault vocabulary the plane's interior is total over — `overflow` (a payload past the largest symbol version, carrying its `Symbology`), `parameter` (a domain-invalid factory value), `unknown`/`illegal`/`arity` (the linear registry family), `ec_level` (the zxing create raise), `content` (a structured-payload or empty-source failure), `render` (a rejected serializer combination), `options` (an admission failure carrying every error `loc` path), `geometry` (a parse/bounds failure on the layered projection), `worker` (a retry-exhausted worker death), `decode` (a decode source-open fault carrying the decode-side fault value), `contract` (a definition-time contract violation lifted onto the rail). Both pages map provider raises into these cases at the arm that raises them; recovery keys on the case, never a reconstructed message.
- Growth: a new symbology is one `Symbology` member plus one `TAXONOMY` row (the owning behavior page adds its dispatch or scope row); a new generation family is one `MarkClass` member; a new option knob is one `MarkPayload`/`SvgStyle`/`WriterOptions` key; a new fault cause is one `MarkFault` case; a new source modality is one `DecodeSource` case; a new channel layout is one `PixelFormat` member; zero new surface.
- Boundary: no provider import crosses this page — segno/python-barcode/zxing enter only on the behavior pages, and the carrier column stores the provider display name as plain `str`. No `EncodeArm`/`SYMBOLOGIES` dispatch (encode's), no `DecodeScope`/`Binarize`/`TextRead`/`EanAddOn`/`ScopeKind`/`FormatFamily` detector policy (decode's), no `Content` structured-payload grammar (encode's segno-helper coupling), no `RasterFact` (the `graphic/raster/process` canonical), no receipt, no entry, no rail.

```python signature
# --- [TYPES] ----------------------------------------------------------------------------
from enum import StrEnum
from typing import Final, Literal, NotRequired, ReadOnly, TypedDict

import numpy as np
from expression import case, tag, tagged_union
from expression.collections import Map
from numpy.typing import NDArray

type Frame = NDArray[np.uint8]


class Symbology(StrEnum):
    QR = "qr"
    MICRO_QR = "micro-qr"
    QR_SEQUENCE = "qr-sequence"
    CODE128 = "code128"
    CODE39 = "code39"
    EAN13 = "ean13"
    EAN8 = "ean8"
    EAN14 = "ean14"
    UPCA = "upca"
    ITF = "itf"
    CODABAR = "codabar"
    ISBN10 = "isbn10"
    ISBN13 = "isbn13"
    ISSN = "issn"
    PZN = "pzn"
    GS1_128 = "gs1_128"
    DATA_MATRIX = "data-matrix"
    PDF417 = "pdf417"
    COMPACT_PDF417 = "compact-pdf417"
    AZTEC = "aztec"
    MAXICODE = "maxicode"
    RMQR = "rmqr"


class MarkClass(StrEnum):
    QR = "qr"
    LINEAR = "linear"
    MATRIX = "matrix"


class PixelFormat(StrEnum):
    RGB = "rgb"
    BGR = "bgr"
    RGBA = "rgba"
    BGRA = "bgra"
    ABGR = "abgr"
    ARGB = "argb"
    LUM = "luminance"
    LUMA = "luminance-alpha"


class SvgStyle(TypedDict, closed=True):
    finder_dark: NotRequired[ReadOnly[str]]
    finder_light: NotRequired[ReadOnly[str]]
    data_dark: NotRequired[ReadOnly[str]]
    data_light: NotRequired[ReadOnly[str]]
    alignment_dark: NotRequired[ReadOnly[str]]
    alignment_light: NotRequired[ReadOnly[str]]
    timing_dark: NotRequired[ReadOnly[str]]
    timing_light: NotRequired[ReadOnly[str]]
    version_dark: NotRequired[ReadOnly[str]]
    version_light: NotRequired[ReadOnly[str]]
    format_dark: NotRequired[ReadOnly[str]]
    format_light: NotRequired[ReadOnly[str]]
    dark_module: NotRequired[ReadOnly[str]]
    separator: NotRequired[ReadOnly[str]]
    quiet_zone: NotRequired[ReadOnly[str]]
    svgclass: NotRequired[ReadOnly[str]]
    lineclass: NotRequired[ReadOnly[str]]
    svgid: NotRequired[ReadOnly[str]]
    title: NotRequired[ReadOnly[str]]
    desc: NotRequired[ReadOnly[str]]
    unit: NotRequired[ReadOnly[str]]
    encoding: NotRequired[ReadOnly[str]]
    svgversion: NotRequired[ReadOnly[float]]
    omitsize: NotRequired[ReadOnly[bool]]
    svgns: NotRequired[ReadOnly[bool]]
    xmldecl: NotRequired[ReadOnly[bool]]
    nl: NotRequired[ReadOnly[bool]]
    draw_transparent: NotRequired[ReadOnly[bool]]


class WriterOptions(TypedDict, closed=True):
    module_width: NotRequired[ReadOnly[float]]
    module_height: NotRequired[ReadOnly[float]]
    quiet_zone: NotRequired[ReadOnly[float]]
    font_size: NotRequired[ReadOnly[int]]
    font_path: NotRequired[ReadOnly[str]]
    text_distance: NotRequired[ReadOnly[float]]
    text_line_distance: NotRequired[ReadOnly[float]]
    background: NotRequired[ReadOnly[str]]
    foreground: NotRequired[ReadOnly[str]]
    center_text: NotRequired[ReadOnly[bool]]
    guard_height_factor: NotRequired[ReadOnly[float]]
    margin_top: NotRequired[ReadOnly[float]]
    margin_bottom: NotRequired[ReadOnly[float]]
    compress: NotRequired[ReadOnly[bool]]
    with_doctype: NotRequired[ReadOnly[bool]]


class MarkPayload(TypedDict, closed=True):
    error: NotRequired[ReadOnly[str]]
    version: NotRequired[ReadOnly[int | str]]
    mode: NotRequired[ReadOnly[str]]
    mask: NotRequired[ReadOnly[int]]
    encoding: NotRequired[ReadOnly[str]]
    boost_error: NotRequired[ReadOnly[bool]]
    eci: NotRequired[ReadOnly[bool]]
    micro: NotRequired[ReadOnly[bool]]
    symbol_count: NotRequired[ReadOnly[int]]
    ec_level: NotRequired[ReadOnly[str | int]]
    scale: NotRequired[ReadOnly[int]]
    border: NotRequired[ReadOnly[int]]
    margin: NotRequired[ReadOnly[int]]
    dark: NotRequired[ReadOnly[str]]
    light: NotRequired[ReadOnly[str]]
    svg: NotRequired[ReadOnly[SvgStyle]]
    add_hrt: NotRequired[ReadOnly[bool]]
    add_quiet_zones: NotRequired[ReadOnly[bool]]
    text: NotRequired[ReadOnly[str]]
    writer_options: NotRequired[ReadOnly[WriterOptions]]


@tagged_union(frozen=True)
class DecodeSource:
    tag: Literal["raster", "pixels"] = tag()
    raster: bytes = case()
    pixels: tuple[Frame, PixelFormat] = case()

    @staticmethod
    def Raster(payload: bytes, /) -> "DecodeSource":
        return DecodeSource(raster=payload)

    @staticmethod
    def Pixels(frame: Frame, fmt: PixelFormat = PixelFormat.RGB, /) -> "DecodeSource":
        return DecodeSource(pixels=(frame, fmt))


# --- [TABLES] ---------------------------------------------------------------------------
# Total over Symbology so no scoped lookup faults; the carrier column is the zxing display name
# (`barcode_formats_from_str` parses it), content-discipline aliases folded onto their physical carrier.
TAXONOMY: Final[Map[Symbology, tuple[MarkClass, str]]] = Map.of_seq([
    (Symbology.QR, (MarkClass.QR, "QRCode")),
    (Symbology.MICRO_QR, (MarkClass.QR, "MicroQRCode")),
    (Symbology.QR_SEQUENCE, (MarkClass.QR, "QRCode")),
    (Symbology.CODE128, (MarkClass.LINEAR, "Code128")),
    (Symbology.CODE39, (MarkClass.LINEAR, "Code39")),
    (Symbology.EAN13, (MarkClass.LINEAR, "EAN13")),
    (Symbology.EAN8, (MarkClass.LINEAR, "EAN8")),
    (Symbology.EAN14, (MarkClass.LINEAR, "EAN13")),
    (Symbology.UPCA, (MarkClass.LINEAR, "UPCA")),
    (Symbology.ITF, (MarkClass.LINEAR, "ITF")),
    (Symbology.CODABAR, (MarkClass.LINEAR, "Codabar")),
    (Symbology.ISBN10, (MarkClass.LINEAR, "EAN13")),
    (Symbology.ISBN13, (MarkClass.LINEAR, "EAN13")),
    (Symbology.ISSN, (MarkClass.LINEAR, "EAN13")),
    (Symbology.PZN, (MarkClass.LINEAR, "Code39")),
    (Symbology.GS1_128, (MarkClass.LINEAR, "Code128")),
    (Symbology.DATA_MATRIX, (MarkClass.MATRIX, "DataMatrix")),
    (Symbology.PDF417, (MarkClass.MATRIX, "PDF417")),
    (Symbology.COMPACT_PDF417, (MarkClass.MATRIX, "CompactPDF417")),
    (Symbology.AZTEC, (MarkClass.MATRIX, "Aztec")),
    (Symbology.MAXICODE, (MarkClass.MATRIX, "MaxiCode")),
    (Symbology.RMQR, (MarkClass.MATRIX, "RMQRCode")),
])


# --- [ERRORS] ---------------------------------------------------------------------------
@tagged_union(frozen=True)
class MarkFault:
    tag: Literal[
        "overflow", "parameter", "unknown", "illegal", "arity", "ec_level", "content", "render", "options", "geometry", "worker", "decode", "contract"
    ] = tag()
    overflow: Symbology = case()
    parameter: str = case()
    unknown: str = case()
    illegal: str = case()
    arity: str = case()
    ec_level: str = case()
    content: str = case()
    render: str = case()
    options: tuple[str, ...] = case()  # every admission-error loc path, never the first error's type alone
    geometry: str = case()
    worker: str = case()
    decode: str = case()  # the decode-side source-open fault value, carried per-op so a corrupt source rails its own slot
    contract: str = case()
```

`TAXONOMY` is the one place a provider format name is spelled: encode's `MATRIX` arm reads the carrier for its `create_barcode` format resolve, decode's format-scope build joins carriers for `barcode_formats_from_str`, and the alias rows (`ISBN13`/`ISSN`/`EAN14` → `EAN13`, `PZN` → `Code39`, `GS1_128` → `Code128`) make the decode direction total — a python-barcode subclass renders through its carrier's machinery, so it decodes as that carrier. `MarkClass` routes generation (`QR` rows resolve segno factories, `LINEAR` rows the python-barcode registry off `Symbology.value`, `MATRIX` rows the zxing creatables) without a second membership table on either behavior page. `DecodeSource` fixes the trust boundary as data: `Raster` is undecoded untrusted bytes, `Pixels` is a trusted already-decoded frame declaring its channel order — the dispatching page maps the case to its isolation lane. `MarkFault` closes the plane's fault algebra so encode and decode rail into one vocabulary and a consumer recovers on the case regardless of which page minted it.
