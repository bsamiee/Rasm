# [PY_ARTIFACTS_GRAPHIC_COLOR_MANAGED]

ICC/LUT/CCTF color-managed raster-egress owner — the native `libvips` process-seam leg of the color sub-domain. `ColorManaged` is one behavior-dense frozen owner over the closed `ManageOp` family. `Managed` crosses the runtime process lane, folds the colour-science grade chain, applies pyvips `icc_transform` under one `IccTransform` policy bundle, and writes through `ManagedCodec` with `ForeignKeep.ICC`. `IccTransform` carries `RenderingIntent`, the `BlackPoint` policy value, `ConnectionSpace`, `BitDepth`, an `Option`-carried lcms2 proof profile, and CxF separations. `Export` folds the same chain on the thread lane and runs `colour.write_image`; `Plate`/`Lut`/`Swatch` author separations, bake `LUT3D`, and graduate CxF3 device declarations. Refined arrays, profiles, paths, documents, pages, DPI values, LUT sizes, matrices, and coverages enter once, while cross-field proof, plate, and LUT failures stay on `Result`.

pyvips carries the native ICC apply through `_icc_apply`; Pillow `ImageCms.buildProofTransform` rides the same process worker for lcms2 soft proofing, and proof admission rejects a built-in source Pillow cannot represent. `colour_cxf.read_cxf` decodes the CxF3 device half into `SpotChannel` values on a thread worker. `pdf_oxide.PdfDocument.render_separations` renders one grayscale coverage plate per ink so `Separate` measures peak TAC and per-ink means from the emitted PDF. Raw profile bytes live in one `ExitStack`-bounded temporary-file capsule through the lazy `icc_transform` pull. Every arm crosses `self.lane.offload` as one runtime `Kernel` whose trait alone derives isolation and worker-death retry — the path-writing arms declare `idempotent=False`, so no retry ever replays an externally visible write — and maps its value directly onto `RuntimeRail[ArtifactReceipt]`; the pre-run key mints synchronously through the bare `ContentIdentity.key` over `_canon`'s length-framed per-arm preimage, and every receipt threads that one key.

## [01]-[INDEX]

- [01]-[MANAGED]: the ICC/LUT/CCTF color-managed raster-egress owner over the closed `ManageOp` family — `Managed` the pyvips `icc_transform` leg under one `IccTransform` bundle on the `HOSTILE` process kernel, `Export` the grade chain plus `colour.write_image` on a `RELEASING` thread kernel, and the `Plate`/`Lut`/`Swatch`/`Separate` terminals minting `ArtifactReceipt.Color` (the last measuring the finished PDF's per-ink coverage) into the settled `core/receipt#RECEIPT` cases under the closed `ManagedFact` vocabulary, every arm crossing `self.lane.offload` and threading one `RuntimeRail`.

## [02]-[MANAGED]

- Cases: `ManageOp` cases — `Managed(raster, src_profile, dst_profile, transform, codec, grade)` the `uint8`/`uint16` raster and its `GradeStep` chain crossing the `HOSTILE` process seam where the worker normalizes to unit range, folds the colour-science grade chain, and applies the pyvips `icc_transform` under the `IccTransform` bundle with the destination profile embedded — the one native-`libvips` process leg, its bare raster riding `Wire.SHARED_MEMORY`; `Export(field, path, depth, grade)` the field and chain crossing one `RELEASING` thread kernel for the grade fold and the bit-depth-correct `colour.write_image`, the worker returning the toned array so the content key folds `depth` in and two depths never collide, crossing no process seam; `Plate(document, channels, transform)` authoring one `/Separation` colorspace per spot plus the joint `/DeviceN` over the pikepdf raw object model, minting `ArtifactReceipt.Color`; `Lut(space, path, grade, size, intent)` baking the ordered chain into a `colour.LUT3D` written through `colour.write_LUT`; `Swatch(document)` graduating the CxF3 device half directly to `Color`; `Separate(document, page, dpi)` rendering the finished PDF's per-ink coverage plates through `pdf_oxide` and minting the MEASURED peak TAC and per-ink coverages as `Color` evidence — matched by one total `match`. `_grade` folds the ordered chain (`cctf`/`broadcast`/`colourspace`/`correction`/`lut`) — the shared module-level core both raster arms reach inside their offload worker, never duplicated per arm nor run on the loop.
- Auto: both raster arms fold the ordered `GradeStep` chain through the module-level `_grade` inside their offload worker via `Block.of_seq(grade).fold`, so decode-linearize, transfer, primary-convert, device-correct, LUT-grade, and re-encode interleave in one chain. `Managed` crosses `self.lane.offload(Kernel.of(_icc_apply, KernelTrait.HOSTILE, wire=Wire.SHARED_MEMORY), ...)` — the bare raster rides the span channel at zero payload bytes — where `_icc_apply` normalizes the raster to unit range by its dtype maximum, folds `_grade`, lifts the toned array through `pyvips.Image.new_from_array`, resolves each `ProfileRef` through `named` (a `BuiltinProfile` passed straight, raw `bytes` written to a temp file on one `ExitStack`), runs `icc_transform`, reads back the embedded profile and the egress `interpretation`, runs the optional Pillow `_softproof` when a `proof` profile is set (the plain-vs-`GAMUTCHECK` diff counting out-of-press-gamut pixels — the lcms2 signal pyvips lacks), reads the peak Total Area Coverage off a CMYK egress (the 4-band sum's `maxpos` normalized to the depth ceiling, `0.0` for a non-cmyk egress), and returns the `write_to_buffer(codec.value, keep=ForeignKeep.ICC)` bytes plus the ICC evidence. `Export` crosses `self.lane.offload(Kernel.of(_export_image, KernelTrait.RELEASING, idempotent=False), ...)` where the field folds through `_grade` then `colour.write_image` at the `BitDepth`, the worker returning the toned array so the content key reads one stored fact. Each worker returns a value the arm's `.map` projects onto the receipt, so the fault threads one `RuntimeRail` end to end and no arm reconstructs an exception; each arm's worker runs the grade chain once, never on the loop nor as a `toned` arg evaluated before the offload.
- Growth: a new managed operation is one `ManageOp` case, one total dispatch arm, one `_canon` preimage arm, and one receipt projection; a new grade step is one `GradeStep` case folded by `_grade` plus one `_step` preimage arm; a new output format is one `ManagedCodec` plus `_CODEC_OPTS` row; a new broadcast curve is one `BroadcastCurve` member plus its `_BROADCAST_ROSTER` memberships, admission proving the kind-curve pairing the colour registries admit; a new profile, curve, space, intent, PCS, depth, or black-point posture is one member in its closed vocabulary; a new evidence scalar is one `ManagedFact` row. New boundary invariants refine on the existing `ManagedRaster`, `ColorField`, `CorrectionMatrix`, `ProfileBytes`, `PdfBytes`, `CxfBytes`, `OutPath`, `PageIndex`, `Dpi`, `LutSize`, or `Coverage` admission axis.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable
from contextlib import ExitStack
from dataclasses import dataclass
from enum import StrEnum
from io import BytesIO
from itertools import chain
from pathlib import Path
from tempfile import NamedTemporaryFile
from typing import Annotated, Final, Literal, assert_never

import colour
import numpy as np
from beartype import beartype
from beartype.vale import Is
from builtins import frozendict
from expression import Error, Nothing, Ok, Option, Result, case, tag, tagged_union
from expression.collections import Block, Map
from numpy.typing import NDArray

from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.core.receipt import ArtifactReceipt
from rasm.artifacts.graphic.color.derive import AdaptMethod
from rasm.runtime.faults import RuntimeRail
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.workers import Kernel, KernelTrait, Wire

lazy import pikepdf
lazy import pdf_oxide
lazy import pyvips
lazy from PIL import (
    Image as PilImage,
    ImageCms,
)
lazy from colour_cxf import (
    cxf3,
    read_cxf,
)

# --- [TYPES] ----------------------------------------------------------------------------
type ColorField = Annotated[
    NDArray[np.float64],
    Is[lambda value: 2 <= value.ndim <= 4 and value.shape[-1] in (3, 4) and bool(np.isfinite(value).all())],
]
type CorrectionMatrix = Annotated[
    NDArray[np.float64],
    Is[lambda value: value.shape == (3, 3) and bool(np.isfinite(value).all())],
]
type ManagedRaster = Annotated[
    NDArray[np.uint8] | NDArray[np.uint16],
    Is[lambda value: value.ndim == 3 and value.shape[0] > 0 and value.shape[1] > 0 and value.shape[2] in (3, 4)],
]
type LutSize = Annotated[int, Is[lambda n: 2 <= n <= 256]]
type Coverage = Annotated[float, Is[lambda c: 0.0 <= c <= 100.0]]
type OutPath = Annotated[str, Is[lambda p: len(p) > 0]]
type PdfBytes = Annotated[bytes, Is[lambda data: data.startswith(b"%PDF-")]]
type CxfBytes = Annotated[bytes, Is[lambda data: len(data) > 0]]
type ProfileBytes = Annotated[bytes, Is[lambda data: len(data) > 0]]
type PageIndex = Annotated[int, Is[lambda page: page >= 0]]
type Dpi = Annotated[int, Is[lambda dpi: 36 <= dpi <= 2400]]
type ManageFault = Literal["<empty-channels>", "<empty-lut>", "<icc-depth>", "<proof-source>", "<transfer-route>"]
type ManageOpTag = Literal["managed", "export", "plate", "lut", "swatch", "separate"]


class ToneCurve(StrEnum):
    SRGB = "sRGB"
    GAMMA_2_2 = "Gamma 2.2"
    ST2084 = "ST 2084"
    BT1886 = "ITU-R BT.1886"
    PROPHOTO = "ProPhoto RGB"


class BroadcastCurve(StrEnum):
    BT709 = "ITU-R BT.709"
    BT1886 = "ITU-R BT.1886"
    BT2100_PQ = "ITU-R BT.2100 PQ"
    BT2100_HLG = "ITU-R BT.2100 HLG"


class RgbSpace(StrEnum):
    SRGB = "sRGB"
    DISPLAY_P3 = "Display P3"
    REC2020 = "ITU-R BT.2020"
    ACESCG = "ACEScg"
    PROPHOTO = "ProPhoto RGB"


class Transfer(StrEnum):
    ENCODE = "encode"
    DECODE = "decode"


class TransferKind(StrEnum):
    OETF = "oetf"
    EOTF = "eotf"
    OOTF = "ootf"


class RenderingIntent(StrEnum):
    PERCEPTUAL = "perceptual"
    RELATIVE = "relative"
    SATURATION = "saturation"
    ABSOLUTE = "absolute"
    AUTO = "auto"


class BlackPoint(StrEnum):
    APPLY = "apply"
    OMIT = "omit"

    @property
    def enabled(self) -> bool:
        return self is BlackPoint.APPLY


class ConnectionSpace(StrEnum):
    LAB = "lab"
    XYZ = "xyz"


class BuiltinProfile(StrEnum):
    SRGB = "srgb"
    P3 = "p3"
    CMYK = "cmyk"


class BitDepth(StrEnum):
    UINT8 = "uint8"
    UINT16 = "uint16"
    FLOAT32 = "float32"


class ManagedCodec(StrEnum):
    PNG = ".png"
    TIFF = ".tif"
    JPEG = ".jpg"
    WEBP = ".webp"


class ManagedFact(StrEnum):
    INTENT = "intent"
    BLACK_POINT = "black_point"
    PCS = "pcs"
    DEPTH = "depth"
    CODEC = "codec"
    BANDS = "bands"
    EMBEDDED = "embedded"
    BUFFER_BYTES = "buffer_bytes"
    GRADE = "grade"
    SPACE = "space"
    GAMUT = "gamut"
    SPOTS = "spots"
    INK = "ink"
    PAGES = "pages"
    SIZE = "size"
    ENTRIES = "entries"
    INKS = "inks"
    DPI = "dpi"


type ProfileRef = ProfileBytes | BuiltinProfile


# --- [MODELS] ---------------------------------------------------------------------------
@tagged_union(frozen=True)
class GradeStep:
    tag: Literal["cctf", "broadcast", "colourspace", "correction", "lut"] = tag()
    cctf: tuple[Transfer, ToneCurve] = case()
    broadcast: tuple[TransferKind, BroadcastCurve] = case()
    colourspace: tuple[RgbSpace, RgbSpace, AdaptMethod] = case()
    correction: CorrectionMatrix = case()
    lut: tuple[OutPath, ...] = case()

    @staticmethod
    @beartype
    def Cctf(direction: Transfer, curve: ToneCurve = ToneCurve.SRGB) -> "GradeStep":
        return GradeStep(cctf=(direction, curve))

    @staticmethod
    @beartype
    def Broadcast(kind: TransferKind, curve: BroadcastCurve = BroadcastCurve.BT709) -> Result["GradeStep", ManageFault]:
        return Ok(GradeStep(broadcast=(kind, curve))) if curve in _BROADCAST_ROSTER[kind] else Error("<transfer-route>")

    @staticmethod
    @beartype
    def Colourspace(source: RgbSpace, target: RgbSpace, adapt: AdaptMethod = AdaptMethod.BRADFORD) -> "GradeStep":
        return GradeStep(colourspace=(source, target, adapt))

    @staticmethod
    @beartype
    def Correction(ccm: CorrectionMatrix, /) -> "GradeStep":
        return GradeStep(correction=ccm)

    @staticmethod
    @beartype
    def Lut(*paths: OutPath) -> Result["GradeStep", ManageFault]:
        return Ok(GradeStep(lut=paths)) if paths else Error("<empty-lut>")


@beartype
@dataclass(frozen=True, slots=True, kw_only=True)
class SpotChannel:
    name: Annotated[str, Is[lambda value: len(value.strip()) > 0]]
    coverage: Coverage


@beartype
@dataclass(frozen=True, slots=True, kw_only=True)
class IccTransform:
    intent: RenderingIntent = RenderingIntent.RELATIVE
    black_point: BlackPoint = BlackPoint.APPLY
    pcs: ConnectionSpace = ConnectionSpace.LAB
    depth: BitDepth = BitDepth.UINT8
    proof: Option[ProfileBytes] = Nothing
    separations: tuple[SpotChannel, ...] = ()


_ICC_DEFAULT: IccTransform = IccTransform()


@tagged_union(frozen=True)
class ManageOp:
    tag: ManageOpTag = tag()
    managed: tuple[ManagedRaster, ProfileRef, ProfileRef, IccTransform, ManagedCodec, tuple[GradeStep, ...]] = case()
    export: tuple[ColorField, OutPath, BitDepth, tuple[GradeStep, ...]] = case()
    plate: tuple[PdfBytes, tuple[SpotChannel, ...], IccTransform] = case()
    lut: tuple[RgbSpace, RenderingIntent, tuple[GradeStep, ...], LutSize, OutPath] = case()
    swatch: CxfBytes = case()
    separate: tuple[PdfBytes, PageIndex, Dpi] = case()

    @staticmethod
    @beartype
    def Managed(
        raster: ManagedRaster,
        src_profile: ProfileRef,
        dst_profile: ProfileRef,
        transform: IccTransform = _ICC_DEFAULT,
        codec: ManagedCodec = ManagedCodec.PNG,
        grade: tuple[GradeStep, ...] = (),
    ) -> Result["ManageOp", ManageFault]:
        return (
            Error("<proof-source>")
            if transform.proof.is_some() and not isinstance(src_profile, bytes) and src_profile is not BuiltinProfile.SRGB
            # pyvips `icc_transform` admits 8/16-bit alone — a FLOAT32 request refuses HERE so produced pixels
            # and receipt depth never disagree; float egress remains the Export arm's own capability.
            else Error("<icc-depth>")
            if transform.depth is BitDepth.FLOAT32
            else Ok(ManageOp(managed=(raster, src_profile, dst_profile, transform, codec, grade)))
        )

    @staticmethod
    @beartype
    def Export(field: ColorField, path: OutPath, depth: BitDepth = BitDepth.UINT16, grade: tuple[GradeStep, ...] = ()) -> "ManageOp":
        return ManageOp(export=(field, path, depth, grade))

    @staticmethod
    @beartype
    def Plate(document: PdfBytes, channels: tuple[SpotChannel, ...], transform: IccTransform = _ICC_DEFAULT) -> Result["ManageOp", ManageFault]:
        return Ok(ManageOp(plate=(document, channels, transform))) if channels else Error("<empty-channels>")

    @staticmethod
    @beartype
    def Lut(
        space: RgbSpace,
        path: OutPath,
        grade: tuple[GradeStep, ...] = (),
        size: LutSize = 33,
        intent: RenderingIntent = RenderingIntent.RELATIVE,
    ) -> "ManageOp":
        return ManageOp(lut=(space, intent, grade, size, path))

    @staticmethod
    @beartype
    def Swatch(document: CxfBytes) -> "ManageOp":
        return ManageOp(swatch=document)

    @staticmethod
    @beartype
    def Separate(document: PdfBytes, page: PageIndex = 0, dpi: Dpi = 150) -> "ManageOp":
        return ManageOp(separate=(document, page, dpi))


# --- [TABLES] ---------------------------------------------------------------------------
_TRANSFER: Final[Map[Transfer, Callable[..., ColorField]]] = Map.of_seq([
    (Transfer.ENCODE, colour.cctf_encoding),
    (Transfer.DECODE, colour.cctf_decoding),
])
_BROADCAST: Final[Map[TransferKind, Callable[..., ColorField]]] = Map.of_seq([
    (TransferKind.OETF, colour.oetf),
    (TransferKind.EOTF, colour.eotf),
    (TransferKind.OOTF, colour.ootf),
])
# each colour OETFS/EOTFS/OOTFS registry admits a distinct curve set; Broadcast proves the pairing so no worker KeyErrors
_BROADCAST_ROSTER: Final[Map[TransferKind, frozenset[BroadcastCurve]]] = Map.of_seq([
    (TransferKind.OETF, frozenset({BroadcastCurve.BT709, BroadcastCurve.BT2100_PQ, BroadcastCurve.BT2100_HLG})),
    (TransferKind.EOTF, frozenset({BroadcastCurve.BT1886, BroadcastCurve.BT2100_PQ, BroadcastCurve.BT2100_HLG})),
    (TransferKind.OOTF, frozenset({BroadcastCurve.BT2100_PQ, BroadcastCurve.BT2100_HLG})),
])
_DEPTH_BITS: Final[Map[BitDepth, int]] = Map.of_seq([(BitDepth.UINT8, 8), (BitDepth.UINT16, 16)])  # icc admits 8/16 alone; Managed admission refuses FLOAT32, whose float egress rides Export
_CODEC_OPTS: Final[Map[ManagedCodec, frozendict[str, int | str | bool]]] = Map.of_seq([
    (ManagedCodec.PNG, frozendict({"compression": 6})),
    (ManagedCodec.TIFF, frozendict({"compression": "lzw"})),
    (ManagedCodec.JPEG, frozendict({"Q": 92})),
    (ManagedCodec.WEBP, frozendict({"Q": 92, "lossless": True})),
])
_PROOF_INTENT_NAME: Final[Map[str, str]] = Map.of_seq([
    (RenderingIntent.PERCEPTUAL.value, "PERCEPTUAL"),
    (RenderingIntent.RELATIVE.value, "RELATIVE_COLORIMETRIC"),
    (RenderingIntent.SATURATION.value, "SATURATION"),
    (RenderingIntent.ABSOLUTE.value, "ABSOLUTE_COLORIMETRIC"),
    (RenderingIntent.AUTO.value, "PERCEPTUAL"),
])


# --- [SERVICES] -------------------------------------------------------------------------
@beartype
@dataclass(frozen=True, slots=True, kw_only=True)
class ColorManaged:
    op: ManageOp
    lane: LanePolicy

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=1.0)

    @property
    def _key(self) -> ContentKey:
        # PRE-RUN key over the op's length-framed canonical preimage through the bare synchronous mint; the railed
        # `ContentIdentity.of` Struct encode is the rejected form — `ManageOp` is a tagged union, not a wire Struct.
        return ContentIdentity.key(f"color-managed-{self.op.tag}", _canon(self.op))

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        match self.op:
            case ManageOp(tag="managed", managed=(raster, src_profile, dst_profile, transform, codec, grade)):
                crossed = await self.lane.offload(
                    Kernel.of(_icc_apply, KernelTrait.HOSTILE, wire=Wire.SHARED_MEMORY),  # the bare raster rides the span channel at zero payload bytes
                    raster,
                    src_profile,
                    dst_profile,
                    transform.intent.value,
                    transform.black_point.enabled,
                    transform.pcs.value,
                    _DEPTH_BITS[transform.depth],
                    codec,
                    grade,
                    transform.proof.to_optional(),
                    len(transform.separations),
                )
                return crossed.map(lambda produced: self._previewed(produced, transform, codec, grade))
            case ManageOp(tag="export", export=(field, path, depth, grade)):
                crossed = await self.lane.offload(Kernel.of(_export_image, KernelTrait.RELEASING, idempotent=False), field, path, depth.value, grade)
                return crossed.map(lambda produced: self._exported(produced, depth, grade))
            case ManageOp(tag="plate", plate=(document, channels, transform)):
                crossed = await self.lane.offload(Kernel.of(_plate_author, KernelTrait.RELEASING), document, channels)
                return crossed.map(lambda plated: self._plated(plated, channels, transform))
            case ManageOp(tag="lut", lut=(space, intent, grade, size, path)):
                crossed = await self.lane.offload(Kernel.of(_lut_author, KernelTrait.RELEASING, idempotent=False), space, grade, size, path)
                return crossed.map(lambda produced: self._lutted(space, intent, grade, size, produced))
            case ManageOp(tag="separate", separate=(document, page, dpi)):
                crossed = await self.lane.offload(Kernel.of(_separate, KernelTrait.RELEASING), document, page, dpi)
                return crossed.map(lambda measured: self._separated(measured, dpi, len(document)))
            case ManageOp(tag="swatch", swatch=document):
                crossed = await self.lane.offload(Kernel.of(separations, KernelTrait.RELEASING), document)
                return crossed.map(lambda channels: self._swatched(channels, len(document)))
            case _:
                assert_never(self.op)

    # --- [PROJECTIONS] ------------------------------------------------------------------
    def _previewed(
        self, produced: tuple[bytes, int, int, int, bool, str, int, int, float], transform: IccTransform, codec: ManagedCodec, grade: tuple[GradeStep, ...]
    ) -> ArtifactReceipt:
        blob, width, height, bands, embedded, space, gamut, spots, ink = produced
        scores: frozendict[str, float | str] = frozendict({
            ManagedFact.INTENT.value: transform.intent.value,
            ManagedFact.BLACK_POINT.value: float(transform.black_point.enabled),
            ManagedFact.PCS.value: transform.pcs.value,
            ManagedFact.DEPTH.value: transform.depth.value,
            ManagedFact.CODEC.value: codec.value,
            ManagedFact.BANDS.value: float(bands),
            ManagedFact.EMBEDDED.value: float(embedded),
            ManagedFact.GRADE.value: float(len(grade)),
            ManagedFact.SPACE.value: space,
            ManagedFact.GAMUT.value: float(gamut),
            ManagedFact.SPOTS.value: float(spots),
            ManagedFact.INK.value: ink,
        })
        return ArtifactReceipt.Preview(self._key, width, height, len(blob), scores)

    def _exported(self, produced: tuple[ColorField, int], depth: BitDepth, grade: tuple[GradeStep, ...]) -> ArtifactReceipt:
        toned, bytes_ = produced
        height, width = toned.shape[0], toned.shape[1]
        scores: frozendict[str, float | str] = frozendict({
            ManagedFact.DEPTH.value: depth.value,
            ManagedFact.BUFFER_BYTES.value: float(toned.nbytes),
            ManagedFact.GRADE.value: float(len(grade)),
        })
        return ArtifactReceipt.Preview(self._key, width, height, bytes_, scores)

    def _plated(self, plated: tuple[bytes, int], channels: tuple[SpotChannel, ...], transform: IccTransform) -> ArtifactReceipt:
        blob, pages = plated
        facts: frozendict[str, float | str] = frozendict({
            ManagedFact.PAGES.value: float(pages),
            **{f"spot:{channel.name}": channel.coverage for channel in channels},
        })
        return ArtifactReceipt.Color(
            self._key,
            "device_n" if len(channels) > 1 else "separation",
            transform.intent.value,
            sum(channel.coverage for channel in channels),
            len(channels),
            len(blob),
            facts,
        )

    def _lutted(
        self, space: RgbSpace, intent: RenderingIntent, grade: tuple[GradeStep, ...], size: int, produced: tuple[int, int]
    ) -> ArtifactReceipt:
        entries, bytes_ = produced
        return ArtifactReceipt.Color(
            self._key,
            space.value,
            intent.value,
            0.0,
            0,
            bytes_,
            frozendict({ManagedFact.GRADE.value: float(len(grade)), ManagedFact.SIZE.value: float(size), ManagedFact.ENTRIES.value: float(entries)}),
        )

    def _separated(self, measured: tuple[float, tuple[tuple[str, float], ...]], dpi: int, bytes_: int) -> ArtifactReceipt:
        tac_peak, coverages = measured
        facts: frozendict[str, float | str] = frozendict({
            ManagedFact.INKS.value: float(len(coverages)),
            ManagedFact.DPI.value: float(dpi),
            **{f"spot:{name}": coverage for name, coverage in coverages},
        })
        return ArtifactReceipt.Color(self._key, "separations", RenderingIntent.ABSOLUTE.value, tac_peak, len(coverages), bytes_, facts)

    def _swatched(self, channels: tuple[SpotChannel, ...], bytes_: int, /) -> ArtifactReceipt:
        return ArtifactReceipt.Color(
            self._key,
            "cmyk_plus_n",
            RenderingIntent.ABSOLUTE.value,
            sum(channel.coverage for channel in channels),
            len(channels),
            bytes_,
            frozendict({f"spot:{channel.name}": channel.coverage for channel in channels}),
        )


# --- [OPERATIONS] -----------------------------------------------------------------------
def _framed(*chunks: bytes) -> tuple[bytes, ...]:
    # patterns row [05]: count-frame the tuple and length-frame every chunk so adjacent variable-width fields never re-split
    return (len(chunks).to_bytes(4, "big"), *chain.from_iterable((len(chunk).to_bytes(8, "big"), chunk) for chunk in chunks))


def _packed(*chunks: bytes) -> bytes:
    return b"".join(_framed(*chunks))


def _array(value: ManagedRaster | ColorField, /) -> bytes:
    return _packed(str(value.dtype).encode(), repr(value.shape).encode(), np.ascontiguousarray(value).tobytes())


def _profile(ref: ProfileRef, /) -> bytes:
    return ref.value.encode() if isinstance(ref, BuiltinProfile) else ref


def _step(step: GradeStep, /) -> bytes:
    match step:
        case GradeStep(tag="cctf", cctf=(direction, curve)):
            return _packed(b"cctf", direction.value.encode(), curve.value.encode())
        case GradeStep(tag="broadcast", broadcast=(kind, curve)):
            return _packed(b"broadcast", kind.value.encode(), curve.value.encode())
        case GradeStep(tag="colourspace", colourspace=(source, target, adapt)):
            return _packed(b"colourspace", source.value.encode(), target.value.encode(), adapt.value.encode())
        case GradeStep(tag="correction", correction=ccm):
            return _packed(b"correction", np.ascontiguousarray(ccm).tobytes())
        case GradeStep(tag="lut", lut=paths):
            return _packed(b"lut", *(path.encode() for path in paths))
        case _ as unreachable:
            assert_never(unreachable)


def _bundle(transform: IccTransform, /) -> bytes:
    return _packed(
        transform.intent.value.encode(),
        transform.black_point.value.encode(),
        transform.pcs.value.encode(),
        transform.depth.value.encode(),
        transform.proof.default_value(b""),
        *(_packed(channel.name.encode(), repr(channel.coverage).encode()) for channel in transform.separations),
    )


def _canon(op: ManageOp, /) -> tuple[bytes, ...]:
    match op:
        case ManageOp(tag="managed", managed=(raster, src, dst, transform, codec, grade)):
            return _framed(b"managed", _array(raster), _profile(src), _profile(dst), _bundle(transform), codec.value.encode(), *map(_step, grade))
        case ManageOp(tag="export", export=(field, path, depth, grade)):
            return _framed(b"export", _array(field), path.encode(), depth.value.encode(), *map(_step, grade))
        case ManageOp(tag="plate", plate=(document, channels, transform)):
            return _framed(b"plate", document, _bundle(transform), *(_packed(row.name.encode(), repr(row.coverage).encode()) for row in channels))
        case ManageOp(tag="lut", lut=(space, intent, grade, size, path)):
            return _framed(b"lut", space.value.encode(), intent.value.encode(), size.to_bytes(2, "big"), path.encode(), *map(_step, grade))
        case ManageOp(tag="swatch", swatch=document):
            return _framed(b"swatch", document)
        case ManageOp(tag="separate", separate=(document, page, dpi)):
            return _framed(b"separate", document, page.to_bytes(4, "big"), dpi.to_bytes(2, "big"))
        case _ as unreachable:
            assert_never(unreachable)


def _grade(field: ColorField, steps: tuple[GradeStep, ...]) -> ColorField:
    def applied(acc: ColorField, step: GradeStep) -> ColorField:
        match step:
            case GradeStep(tag="cctf", cctf=(direction, curve)):
                return _TRANSFER[direction](acc, function=curve.value)
            case GradeStep(tag="broadcast", broadcast=(kind, curve)):
                return _BROADCAST[kind](acc, function=curve.value)
            case GradeStep(tag="colourspace", colourspace=(source, target, adapt)):
                return colour.RGB_to_RGB(
                    acc, colour.RGB_COLOURSPACES[source.value], colour.RGB_COLOURSPACES[target.value], chromatic_adaptation_transform=adapt.value
                )
            case GradeStep(tag="correction", correction=ccm):
                return colour.apply_matrix_colour_correction(acc, ccm)
            case GradeStep(tag="lut", lut=paths):
                return colour.LUTSequence(*(colour.read_LUT(path) for path in paths)).apply(acc)
            case _ as unreachable:
                assert_never(unreachable)

    return Block.of_seq(steps).fold(applied, field)


def _export_image(field: ColorField, path: OutPath, depth: str, grade: tuple[GradeStep, ...]) -> tuple[ColorField, int]:
    toned = _grade(field, grade)
    colour.write_image(toned, path, depth)
    return toned, Path(path).stat().st_size


def _softproof(rgb8: NDArray[np.uint8], reference: str | ImageCms.ImageCmsProfile, proof_path: str, intent: str) -> int:
    # simulate the press/proof profile (buildProofTransform, reference as input+display, proof the press); the plain-vs-GAMUTCHECK
    # output diff marks the out-of-press-gamut pixels — the lcms2 PDF/X preflight signal pyvips has no member for.
    origin = PilImage.fromarray(rgb8, "RGB")
    intent_member = getattr(ImageCms.Intent, _PROOF_INTENT_NAME[intent])
    proof_intent = ImageCms.Intent.ABSOLUTE_COLORIMETRIC
    plain = ImageCms.buildProofTransform(
        reference,
        reference,
        proof_path,
        "RGB",
        "RGB",
        renderingIntent=intent_member,
        proofRenderingIntent=proof_intent,
        flags=ImageCms.Flags.SOFTPROOFING,
    )
    warned = ImageCms.buildProofTransform(
        reference,
        reference,
        proof_path,
        "RGB",
        "RGB",
        renderingIntent=intent_member,
        proofRenderingIntent=proof_intent,
        flags=ImageCms.Flags.SOFTPROOFING | ImageCms.Flags.GAMUTCHECK,
    )
    return int(
        np.count_nonzero(np.any(np.asarray(ImageCms.applyTransform(origin, plain)) != np.asarray(ImageCms.applyTransform(origin, warned)), axis=-1))
    )


def _separate(document: PdfBytes, page: PageIndex, dpi: Dpi) -> tuple[float, tuple[tuple[str, float], ...]]:
    # read-side prepress audit worker: pdf_oxide renders one grayscale coverage plate per page ink (pixel intensity == tint %),
    # so per-ink mean coverage and the true per-pixel ink-sum PEAK TAC are MEASURED off the finished PDF, never re-derived by hand.
    doc = pdf_oxide.PdfDocument.from_bytes(document)
    plates = doc.render_separations(page, dpi)
    fields = Block.of_seq(plates).map(lambda plate: (str(plate.ink_name), np.frombuffer(plate.data, dtype=np.uint8).astype(np.float64).reshape(plate.height, plate.width)))
    coverages = tuple((name, float(field.mean()) / 255.0 * 100.0) for name, field in fields)
    tac_peak = float(np.stack([field for _, field in fields]).sum(axis=0).max()) / 255.0 * 100.0 if not fields.is_empty() else 0.0
    return tac_peak, coverages


def _icc_apply(
    raster: ManagedRaster,
    src: ProfileRef,
    dst: ProfileRef,
    intent: str,
    bpc: bool,
    pcs: str,
    depth: int,
    codec: ManagedCodec,
    grade: tuple[GradeStep, ...],
    proof: ProfileBytes | None,
    spots: int,
) -> tuple[bytes, int, int, int, bool, str, int, int, float]:
    toned = _grade(raster / np.float64(np.iinfo(raster.dtype).max), grade)

    def named(stack: ExitStack, profile: ProfileRef, /) -> str:
        if isinstance(profile, str):
            return profile
        handle = stack.enter_context(NamedTemporaryFile(suffix=".icc", delete_on_close=False))
        handle.write(profile)
        handle.close()
        return handle.name

    image = pyvips.Image.new_from_array(toned)
    with ExitStack() as stack:  # the profile temp files must outlive the lazy icc_transform until write_to_buffer pulls pixels
        src_path = named(stack, src)
        managed = image.icc_transform(named(stack, dst), input_profile=src_path, intent=intent, black_point_compensation=bpc, pcs=pcs, depth=depth)
        gamut = (
            _softproof(
                np.clip(np.asarray(toned)[..., :3] * 255.0, 0.0, 255.0).astype(np.uint8),
                src_path if isinstance(src, bytes) else ImageCms.createProfile("sRGB"),
                named(stack, proof),
                intent,
            )
            if proof is not None
            else 0
        )
        space = str(managed.interpretation)
        # Total Area Coverage — the peak (C+M+Y+K) ink sum over the converted CMYK field, the ISO 12647 / PDF-X-4 ink-limit
        # preflight paired with gamut. cmyk guarantees 4 bands, so the band sum's maxpos()[0] peak normalizes to the depth ceiling; 0.0 for a non-cmyk egress.
        ink = float((managed[0] + managed[1] + managed[2] + managed[3]).maxpos()[0]) / float((1 << depth) - 1) * 100.0 if space == "cmyk" else 0.0
        return (
            managed.write_to_buffer(codec.value, keep=pyvips.ForeignKeep.ICC, **_CODEC_OPTS[codec]),
            managed.width,
            managed.height,
            managed.bands,
            managed.get_typeof("icc-profile-data") != 0,
            space,
            gamut,
            spots,
            ink,
        )


def _plate_author(document: PdfBytes, channels: tuple[SpotChannel, ...]) -> tuple[bytes, int]:
    # Each spot owns a Type 2 `/Separation`; a multi-channel set adds one Type 4 `/DeviceN` calculator that folds
    # all tints onto the CMYK alternate and registers every color space on every page.
    with pikepdf.open(BytesIO(document)) as pdf:
        spaces = {
            channel.name: pdf.make_indirect(
                pikepdf.Array([
                    pikepdf.Name("/Separation"),
                    pikepdf.Name(f"/{channel.name}"),
                    pikepdf.Name("/DeviceCMYK"),
                    pikepdf.Dictionary(FunctionType=2, Domain=[0, 1], C0=[0, 0, 0, 0], C1=[0, 0, 0, 1], N=1),
                ])
            )
            for channel in channels
        }
        if len(channels) > 1:
            calculator = ("{ " + "add " * (len(channels) - 1) + "1 min 0 0 0 4 -1 roll }").encode()
            spaces["DeviceN"] = pdf.make_indirect(
                pikepdf.Array([
                    pikepdf.Name("/DeviceN"),
                    pikepdf.Array([pikepdf.Name(f"/{channel.name}") for channel in channels]),
                    pikepdf.Name("/DeviceCMYK"),
                    pdf.make_stream(calculator, pikepdf.Dictionary(FunctionType=4, Domain=[0, 1] * len(channels), Range=[0, 1] * 4)),
                ])
            )
        for page in pdf.pages:  # Exemption: pikepdf pages/resources are a mutable qpdf object tree; add_resource registers in place
            for name, space in spaces.items():
                page.add_resource(space, pikepdf.Name("/ColorSpace"), pikepdf.Name(f"/{name}"))
        sink = BytesIO()
        pdf.save(sink)
        return sink.getvalue(), len(pdf.pages)


def lut_bytes(space: RgbSpace, grade: tuple[GradeStep, ...], size: LutSize, /) -> bytes:
    # in-memory half of the SAME bake: the raw float32 N³x3 table `graphic/raster/process#PROCESS`
    # `Transform.LUT_3D` decodes — one `_grade` law feeds the `.cube`/`.clf` container AND the raster consumer,
    # so the authored file and the in-memory wire cannot disagree.
    axis = np.linspace(0.0, 1.0, size)
    r, g, b = np.meshgrid(axis, axis, axis, indexing="ij")
    return _grade(np.stack([r, g, b], axis=-1), grade).astype(np.float32).reshape(-1, 3).tobytes()


def _lut_author(space: RgbSpace, grade: tuple[GradeStep, ...], size: LutSize, path: OutPath) -> tuple[int, int]:
    # bake the grade chain into a LUT3D lattice: the identity cube folds through the SAME `_grade` law the raster arms
    # apply, so a baked .cube/.clf and a live grade cannot disagree; write_LUT keys the container format off the path suffix.
    axis = np.linspace(0.0, 1.0, size)
    r, g, b = np.meshgrid(axis, axis, axis, indexing="ij")
    colour.write_LUT(colour.LUT3D(table=_grade(np.stack([r, g, b], axis=-1), grade), name=f"rasm-{space.value}"), path)
    return size**3, Path(path).stat().st_size


def separations(document: CxfBytes, /) -> tuple[SpotChannel, ...]:
    # decode the CxF3 DEVICE half — the ColorCmykplusN spot declaration (managed owns the device half; the spectral/Lab
    # color half is derive#DERIVE's), each named SpotColorType channel at its coverage.
    resources = read_cxf(document).resources
    collection = resources.object_collection if resources else None
    return tuple(
        SpotChannel(name=str(spot.name or ""), coverage=float(spot.percentage or 0.0))
        for obj in (collection.object_value if collection else ())
        for device in ((obj.device_color_values,) if obj.device_color_values else ())
        for member in device.choice
        if isinstance(member, cxf3.ColorCmykplusN)
        for spot in member.spot_color
    )


# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = [
    "BitDepth",
    "BlackPoint",
    "BroadcastCurve",
    "BuiltinProfile",
    "ColorField",
    "ColorManaged",
    "ConnectionSpace",
    "Coverage",
    "CxfBytes",
    "Dpi",
    "GradeStep",
    "IccTransform",
    "LutSize",
    "ManageFault",
    "ManageOp",
    "ManagedCodec",
    "ManagedFact",
    "ManagedRaster",
    "OutPath",
    "PageIndex",
    "PdfBytes",
    "ProfileBytes",
    "ProfileRef",
    "RenderingIntent",
    "RgbSpace",
    "SpotChannel",
    "ToneCurve",
    "Transfer",
    "TransferKind",
    "separations",
]
```
