# [PY_ARTIFACTS_GRAPHIC_MARKS_MARK]

Machine-readable-mark vocabulary owns every creatable and readable identity the marks plane carries. `TAXONOMY` binds each `Symbology` to a behavior class and physical decode carrier, including `MarkClass.SCAN` rows whose zxing-readable formats have no admitted generator; carrier-less generated symbols remain explicit through `None`.

`graphic/marks/encode#MARK` imports this page for both directions of the `MarkOp` family and `graphic/marks/decode#DECODE` imports it for the scan substrate — decode never imports encode, and the one-way encode -> decode import composes the scan kernel under the single `Mark` rail. Carrier rows fold the content-discipline aliases onto the carrier that physically renders and decodes them (ISBN-10/ISBN-13/ISSN over the EAN-13 carrier, PZN over Code39, GS1-128 over Code128 — a python-barcode subclass renders through its carrier's machinery, so it decodes as that carrier), while `EAN14` carries `None`: python-barcode's `EAN-14` renders through the EAN-13 pattern machinery with a seven-digit right half, a stretched symbol no zxing reader recognizes. Behavior lives above: the generation arms, the decode scan, and the `MarkOp` encode/decode/verify dispatch are encode's, the `read_barcodes` detector policy decode's, and the canonical `RasterFact` all directions fold into is `graphic/raster/process#PROCESS`'s.

## [01]-[INDEX]

- [01]-[MARK]: `Symbology`, `TAXONOMY`, factory-specific option bands, `DecodeSource`, `PixelFormat`, and `MarkFault` form the neutral marks vocabulary.

## [02]-[MARK]

- Owner: `Symbology` is the closed identity every plane surface keys on. `MarkClass` distinguishes segno QR, python-barcode linear, zxing matrix, and scan-only families; `TAXONOMY` is total over the vocabulary and drives both behavior directions.
- Cases: `DecodeSource.Raster(payload)` carries encoded raster bytes; `DecodeSource.Pixels(frame, fmt)` requires explicit `PixelFormat`, so a raw frame never inherits an ambient RGB assumption.
- Payload: QR admission selects `QrPayload`, `MicroQrPayload`, or `QrSequencePayload`, so `eci` and `symbol_count` exist only on accepting factories; linear and matrix bands keep creator and renderer fields distinct. Encode selects the exact `TypeAdapter` from `Symbology`, making cross-family and cross-factory keys admission faults.
- Faults: `MarkFault` is the plane-wide rail vocabulary. `unscannable` identifies a carrier-less generated symbol, while `uncreatable` identifies a scan-only symbology presented to encode; structured cases retain offending symbology, option locations, and source-shape evidence.
- Growth: a symbology adds one vocabulary member and one `TAXONOMY` row; a factory-specific option extends only its closed band; a fault, source modality, or channel layout adds one case or member to its existing owner.
- Boundary: no provider import crosses this page — segno/python-barcode/zxing enter only on behavior pages, and the carrier column stores provider display names as `str | None`. Encode owns `MarkOp` and `Content`; decode owns `DecodeScope.scan`; `graphic/raster/process#PROCESS` owns `RasterFact`.

```python signature
# --- [TYPES] ----------------------------------------------------------------------------
from enum import StrEnum
from typing import Final, Literal, NotRequired, ReadOnly, TypedDict

import numpy as np
from builtins import frozendict
from expression import case, tag, tagged_union
from numpy.typing import NDArray

type Frame = NDArray[np.uint8]


class Symbology(StrEnum):
    QR = "qr"
    MICRO_QR = "micro-qr"
    QR_SEQUENCE = "qr-sequence"
    CODE128 = "code128"
    CODE39 = "code39"
    CODE93 = "code93"
    EAN13 = "ean13"
    EAN8 = "ean8"
    EAN14 = "ean14"
    UPCA = "upca"
    UPCE = "upce"
    ITF = "itf"
    CODABAR = "codabar"
    ISBN10 = "isbn10"
    ISBN13 = "isbn13"
    ISSN = "issn"
    PZN = "pzn"
    GS1_128 = "gs1_128"
    DATABAR = "databar"
    DATABAR_EXPANDED = "databar-expanded"
    DATABAR_LIMITED = "databar-limited"
    DX_FILM_EDGE = "dx-film-edge"
    DATA_MATRIX = "data-matrix"
    PDF417 = "pdf417"
    COMPACT_PDF417 = "compact-pdf417"
    MICRO_PDF417 = "micro-pdf417"
    AZTEC = "aztec"
    MAXICODE = "maxicode"
    RMQR = "rmqr"


class MarkClass(StrEnum):
    QR = "qr"
    LINEAR = "linear"
    MATRIX = "matrix"
    SCAN = "scan"


class PixelFormat(StrEnum):
    RGB = "rgb"
    BGR = "bgr"
    RGBA = "rgba"
    BGRA = "bgra"
    ABGR = "abgr"
    ARGB = "argb"
    LUM = "luminance"
    LUMA = "luminance-alpha"


# --- [MODELS] ---------------------------------------------------------------------------
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


class QrMake(TypedDict, closed=True):
    error: NotRequired[ReadOnly[str]]
    version: NotRequired[ReadOnly[int | str]]
    mode: NotRequired[ReadOnly[str]]
    mask: NotRequired[ReadOnly[int]]
    encoding: NotRequired[ReadOnly[str]]
    boost_error: NotRequired[ReadOnly[bool]]


class QrFullMake(TypedDict, closed=True):
    error: NotRequired[ReadOnly[str]]
    version: NotRequired[ReadOnly[int | str]]
    mode: NotRequired[ReadOnly[str]]
    mask: NotRequired[ReadOnly[int]]
    encoding: NotRequired[ReadOnly[str]]
    boost_error: NotRequired[ReadOnly[bool]]
    eci: NotRequired[ReadOnly[bool]]


class QrSequenceMake(TypedDict, closed=True):
    error: NotRequired[ReadOnly[str]]
    version: NotRequired[ReadOnly[int | str]]
    mode: NotRequired[ReadOnly[str]]
    mask: NotRequired[ReadOnly[int]]
    encoding: NotRequired[ReadOnly[str]]
    boost_error: NotRequired[ReadOnly[bool]]
    symbol_count: NotRequired[ReadOnly[int]]


class QrRender(TypedDict, closed=True):
    scale: NotRequired[ReadOnly[int]]
    border: NotRequired[ReadOnly[int]]
    dark: NotRequired[ReadOnly[str]]
    light: NotRequired[ReadOnly[str]]
    svg: NotRequired[ReadOnly[SvgStyle]]


class MatrixMake(TypedDict, closed=True):
    ec_level: NotRequired[ReadOnly[str]]  # the sole genuine create_barcode key; geometry is the writer's


class MatrixRender(TypedDict, closed=True):
    scale: NotRequired[ReadOnly[int]]
    add_hrt: NotRequired[ReadOnly[bool]]
    add_quiet_zones: NotRequired[ReadOnly[bool]]


class QrPayload(TypedDict, closed=True):
    make: NotRequired[ReadOnly[QrFullMake]]
    render: NotRequired[ReadOnly[QrRender]]


class MicroQrPayload(TypedDict, closed=True):
    make: NotRequired[ReadOnly[QrMake]]
    render: NotRequired[ReadOnly[QrRender]]


class QrSequencePayload(TypedDict, closed=True):
    make: NotRequired[ReadOnly[QrSequenceMake]]
    render: NotRequired[ReadOnly[QrRender]]


class LinearPayload(TypedDict, closed=True):
    text: NotRequired[ReadOnly[str]]
    render: NotRequired[ReadOnly[WriterOptions]]


class MatrixPayload(TypedDict, closed=True):
    make: NotRequired[ReadOnly[MatrixMake]]
    render: NotRequired[ReadOnly[MatrixRender]]


type OptionBand = QrPayload | MicroQrPayload | QrSequencePayload | LinearPayload | MatrixPayload


@tagged_union(frozen=True)
class DecodeSource:
    tag: Literal["raster", "pixels"] = tag()
    raster: bytes = case()
    pixels: tuple[Frame, PixelFormat] = case()

    @staticmethod
    def Raster(payload: bytes, /) -> "DecodeSource":
        return DecodeSource(raster=payload)

    @staticmethod
    def Pixels(frame: Frame, fmt: PixelFormat, /) -> "DecodeSource":
        return DecodeSource(pixels=(frame, fmt))


# --- [TABLES] ---------------------------------------------------------------------------
# The ONE class-and-carrier correspondence: total over Symbology, carrier the zxing decode display
# name barcode_formats_from_str parses, None where no zxing reader decodes the rendered symbol;
# every scope and dispatch derives from this table, never a second enumerated map.
TAXONOMY: Final[frozendict[Symbology, tuple[MarkClass, str | None]]] = frozendict({
    Symbology.QR: (MarkClass.QR, "QRCode"),
    Symbology.MICRO_QR: (MarkClass.QR, "MicroQRCode"),
    Symbology.QR_SEQUENCE: (MarkClass.QR, "QRCode"),
    Symbology.CODE128: (MarkClass.LINEAR, "Code128"),
    Symbology.CODE39: (MarkClass.LINEAR, "Code39"),
    Symbology.CODE93: (MarkClass.SCAN, "Code93"),
    Symbology.EAN13: (MarkClass.LINEAR, "EAN13"),
    Symbology.EAN8: (MarkClass.LINEAR, "EAN8"),
    Symbology.EAN14: (MarkClass.LINEAR, None),  # python-barcode renders a stretched EAN-13-machinery symbol no zxing reader decodes
    Symbology.UPCA: (MarkClass.LINEAR, "UPCA"),
    Symbology.UPCE: (MarkClass.SCAN, "UPCE"),
    Symbology.ITF: (MarkClass.LINEAR, "ITF"),
    Symbology.CODABAR: (MarkClass.LINEAR, "Codabar"),
    Symbology.ISBN10: (MarkClass.LINEAR, "EAN13"),
    Symbology.ISBN13: (MarkClass.LINEAR, "EAN13"),
    Symbology.ISSN: (MarkClass.LINEAR, "EAN13"),
    Symbology.PZN: (MarkClass.LINEAR, "Code39"),
    Symbology.GS1_128: (MarkClass.LINEAR, "Code128"),
    Symbology.DATABAR: (MarkClass.SCAN, "DataBar"),
    Symbology.DATABAR_EXPANDED: (MarkClass.SCAN, "DataBarExpanded"),
    Symbology.DATABAR_LIMITED: (MarkClass.SCAN, "DataBarLimited"),
    Symbology.DX_FILM_EDGE: (MarkClass.SCAN, "DXFilmEdge"),
    Symbology.DATA_MATRIX: (MarkClass.MATRIX, "DataMatrix"),
    Symbology.PDF417: (MarkClass.MATRIX, "PDF417"),
    Symbology.COMPACT_PDF417: (MarkClass.MATRIX, "CompactPDF417"),
    Symbology.MICRO_PDF417: (MarkClass.MATRIX, "MicroPDF417"),
    Symbology.AZTEC: (MarkClass.MATRIX, "Aztec"),
    Symbology.MAXICODE: (MarkClass.MATRIX, "MaxiCode"),
    Symbology.RMQR: (MarkClass.MATRIX, "RMQRCode"),
})


# --- [ERRORS] ---------------------------------------------------------------------------
@tagged_union(frozen=True)
class MarkFault:
    tag: Literal[
        "overflow",
        "parameter",
        "unknown",
        "illegal",
        "arity",
        "ec_level",
        "content",
        "render",
        "options",
        "geometry",
        "unreadable",
        "malformed",
        "unscannable",
        "uncreatable",
        "contract",
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
    unreadable: str = case()  # a Raster source Pillow could not open or fully decode
    malformed: str = case()  # a Pixels frame whose rank/dtype/channel/stride contract refuses admission
    unscannable: Symbology = case()  # a decode/verify scope member whose TAXONOMY carrier is None
    uncreatable: Symbology = case()
    contract: str = case()


# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = [
    "DecodeSource",
    "Frame",
    "LinearPayload",
    "MarkClass",
    "MarkFault",
    "MatrixMake",
    "MatrixPayload",
    "MatrixRender",
    "MicroQrPayload",
    "OptionBand",
    "PixelFormat",
    "QrFullMake",
    "QrMake",
    "QrPayload",
    "QrRender",
    "QrSequenceMake",
    "QrSequencePayload",
    "SvgStyle",
    "Symbology",
    "TAXONOMY",
    "WriterOptions",
]
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
