# [PY_ARTIFACTS_GRAPHIC_COLOR_MANAGED]

ICC/LUT/CCTF color-managed raster-egress owner — the one native-`libvips` process-seam leg of the color sub-domain. `ColorManaged` is one owner over the closed `ManageOp` family: the `Managed` arm crosses the runtime worker subprocess seam where the colour-science grade chain folds and pyvips `icc_transform` applies the source→destination ICC transform under one `IccTransform` policy bundle (`RenderingIntent`, black-point compensation, `ConnectionSpace` PCS, output `BitDepth`, an optional lcms2 soft-proof profile, and a CxF `ColorCmykplusN` separations declaration) and writes through one `ManagedCodec` embedding the destination profile under an explicit `ForeignKeep.ICC` flag; the `Export` arm folds the same chain on a `to_thread` slot and runs the bit-depth-correct `colour.write_image`; the `Plate`/`Lut`/`Swatch` terminals author separations, bake the grade into a `LUT3D`, and graduate a decoded CxF3 declaration. colour-science carries the `cctf`/`oetf`/`eotf`/`ootf`/`RGB_to_RGB`/CCM/LUT grade chain, and both raster arms fold that multi-MB-image chain inside their offload worker, never on the event loop nor as a `toned` arg evaluated before the offload.

pyvips carries the genuinely-native ICC apply that cannot resolve in the runtime process — a Forge-provisioned `libvips` off the runtime loader path — so `Managed` dispatches the raster-plus-grade onto the runtime process lane worker through the module-level `_icc_apply`. Pillow `ImageCms.buildProofTransform` rides the same `to_process` worker for the one surviving lcms2 soft-proof role pyvips has no member for, counting out-of-press-gamut pixels; `colour_cxf.read_cxf` decodes the CxF3 device half into the `SpotChannel` separations the CMYK/DeviceN egress records as `ManagedFact.SPOTS` evidence — carried, not an applied device space, since libvips authors no named DeviceN channel. A profile crosses as a `ProfileRef` — raw `bytes` to a temp file, or a `BuiltinProfile` device name passed straight to `liblcms2`. `ManageOp` dispatches by one total `match` returning `RuntimeRail[ArtifactReceipt]`, keying bytes through `ContentIdentity.of` and contributing the settled `core/receipt#RECEIPT` `ArtifactReceipt.Preview`/`Color` cases under the closed `ManagedFact` vocabulary — never a parallel managed-receipt type. It consumes the grade chain and color-space provenance `graphic/color/derive#DERIVE` resolves and hands the color-managed raster to `graphic/raster/io#RASTER` and `document/emit#DOCUMENT`.

## [01]-[INDEX]

- [01]-[MANAGED]: the ICC/LUT/CCTF color-managed raster-egress owner over the closed `ManageOp` family — `Managed` the pyvips `icc_transform` leg under one `IccTransform` bundle on the `to_process` worker, `Export` the grade chain plus `colour.write_image` on a `to_thread` slot, and the `Plate`/`Lut`/`Swatch` terminals minting `ArtifactReceipt.Color` into the settled `core/receipt#RECEIPT` cases under the closed `ManagedFact` vocabulary.

## [02]-[MANAGED]

- Cases: `ManageOp` cases — `Managed(raster, src_profile, dst_profile, transform, codec, grade)` the `uint8`/`uint16` raster and its `GradeStep` chain crossing the `to_process` seam where the worker normalizes to unit range, folds the colour-science grade chain, and applies the pyvips `icc_transform` under the `IccTransform` bundle with the destination profile embedded — the one native-`libvips` subprocess leg; `Export(field, path, depth, grade)` the field and chain crossing one `to_thread` slot for the grade fold and the bit-depth-correct `colour.write_image`, the worker returning the toned array so the content key folds `depth` in and two depths never collide, crossing no subprocess; `Plate(document, channels, transform)` authoring one `/Separation` colorspace per spot plus the joint `/DeviceN` over the pikepdf raw object model, minting `ArtifactReceipt.Color`; `Lut(space, path, grade, size, intent)` baking the ordered chain into a `colour.LUT3D` written through `colour.write_LUT`; `Swatch(document)` graduating the CxF3 device half directly to `Color` — matched by one total `match`. `_grade` folds the ordered chain (`cctf`/`broadcast`/`colourspace`/`correction`/`lut`) — the shared module-level core both arms reach inside their offload worker, never duplicated per arm nor run on the loop.
- Auto: both raster arms fold the ordered `GradeStep` chain through the module-level `_grade` inside their offload worker via `Block.of_seq(grade).fold`, so decode-linearize, transfer, primary-convert, device-correct, LUT-grade, and re-encode interleave in one chain. `Managed` crosses `to_process.run_sync(_icc_apply, ..., limiter=_ICC_LANE)` where `_icc_apply` normalizes the raster to unit range by its dtype maximum, folds `_grade`, lifts the toned array through `pyvips.Image.new_from_array`, resolves each `ProfileRef` through `named` (a `BuiltinProfile` passed straight, raw `bytes` written to a temp file on one `ExitStack`), runs `icc_transform`, reads back the embedded profile and the egress `interpretation`, runs the optional Pillow `_softproof` when a `proof` profile is set (the plain-vs-`GAMUTCHECK` diff counting out-of-press-gamut pixels — the lcms2 signal pyvips lacks), reads the peak Total Area Coverage off a CMYK egress (the 4-band sum's `maxpos` normalized to the depth ceiling, `0.0` for a non-cmyk egress), and returns the `write_to_buffer(codec.value, keep=ForeignKeep.ICC)` bytes plus the ICC evidence. `Export` crosses `to_thread.run_sync(_export_image, ...)` where the field folds through `_grade` then `colour.write_image` at the `BitDepth`, the worker returning the toned array so the content key reads one stored fact. Each arm's worker runs the grade chain once, never on the loop nor as a `toned` arg evaluated before the offload.
- Growth: a new managed operation is one `ManageOp` case plus one acceptor arm contributing its receipt case (`Preview` for a raster egress, `Color` for a plate/LUT/swatch terminal); a new grade step one `GradeStep` case folded by `_grade`; the soft-proof control (a `proof` field on `IccTransform` plus the `GAMUT`/`SPACE` `ManagedFact` rows) and the separations declaration (a `separations` `SpotChannel` field plus the `SPOTS` row) are in-place field-plus-row growth, never a payload-arity change breaking every `Managed` call; a new output format one `ManagedCodec` plus `_CODEC_OPTS` row; a new built-in profile one `BuiltinProfile` row; a new CCTF/broadcast/primary/adaptation function one `ToneCurve`/`BroadcastCurve`/`RgbSpace`/`AdaptMethod` row keying its colour registry; a new intent/PCS/bit-depth one `RenderingIntent`/`ConnectionSpace`/`BitDepth` row; a new evidence scalar one `ManagedFact` row plus one `_icc_apply` return slot (the `INK` TAC preflight is that); never a parallel managed surface, never a second image writer; zero new surface.

```python signature
from collections.abc import Callable
from contextlib import ExitStack
from dataclasses import dataclass
from enum import StrEnum
from io import BytesIO
from tempfile import NamedTemporaryFile
from typing import Final, Literal, assert_never

import colour
import numpy as np
from expression import case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from numpy.typing import NDArray

from rasm.artifacts.graphic.color.derive import AdaptMethod
from rasm.runtime.identity import CANONICAL_POLICY, ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy, Modality
from rasm.runtime.resilience import RetryClass
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.core.plan import Admission, ArtifactWork
from artifacts.core.receipt import ArtifactReceipt

lazy import pikepdf  # Separation/DeviceN plate authoring over the raw object model, in the thread worker
lazy import pyvips  # reifies on first `_icc_apply` use, keeping native libvips off the parent loader path
lazy from PIL import (
    Image as PilImage,
    ImageCms,
)  # lcms2 soft-proof/gamut-warning only — the pillow role pyvips has no member for, never the device-egress engine
lazy from colour_cxf import (
    cxf3,
    read_cxf,
)  # CxF3 device-half decode for the ColorCmykplusN separations declaration

type Tristimulus = NDArray[np.float64]
type ManagedRaster = NDArray[np.uint8] | NDArray[np.uint16]
type ManageOpTag = Literal["managed", "export", "plate", "lut", "swatch"]


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
    BPC = "bpc"
    PCS = "pcs"
    DEPTH = "depth"
    CODEC = "codec"
    BANDS = "bands"
    EMBEDDED = "embedded"
    BYTES = "bytes"
    GRADE = "grade"
    SPACE = "space"  # the egress libvips Interpretation (srgb/cmyk/...) — the separations-relevant output colourspace
    GAMUT = "gamut"  # the soft-proof out-of-press-gamut pixel count (0 when no proof profile is set)
    SPOTS = "spots"  # the CxF ColorCmykplusN N-channel spot-separation count the egress declares
    INK = "ink"  # the peak Total Area Coverage % over the converted CMYK field — the ISO 12647 / PDF-X-4 ink-limit preflight (0.0 for a non-cmyk egress)
    PAGES = "pages"  # plate-authored page count on the Color band
    SIZE = "size"  # LUT lattice size per axis on the Color band
    ENTRIES = "entries"  # LUT lattice entry count (size**3) on the Color band; per-spot coverages ride dynamic `spot:<name>` keys


type ProfileRef = bytes | BuiltinProfile


_TRANSFER: Final[Map[Transfer, Callable[..., Tristimulus]]] = Map.of_seq([
    (Transfer.ENCODE, colour.cctf_encoding),
    (Transfer.DECODE, colour.cctf_decoding),
])
_BROADCAST: Final[Map[TransferKind, Callable[..., Tristimulus]]] = Map.of_seq([
    (TransferKind.OETF, colour.oetf),
    (TransferKind.EOTF, colour.eotf),
    (TransferKind.OOTF, colour.ootf),
])
# libvips icc_transform emits 8/16-bit only, so FLOAT32 managed output clamps to 16; the Export arm keeps the unclamped colour.write_image depth.
_DEPTH_BITS: Final[Map[BitDepth, int]] = Map.of_seq([(BitDepth.UINT8, 8), (BitDepth.UINT16, 16), (BitDepth.FLOAT32, 16)])
# per-codec libvips egress policy: lossless PNG/TIFF preserve managed fidelity, WEBP near-lossless, JPEG high-Q for an 8-bit web target.
_CODEC_OPTS: Final[Map[ManagedCodec, frozendict[str, int | str | bool]]] = Map.of_seq([
    (ManagedCodec.PNG, frozendict({"compression": 6})),
    (ManagedCodec.TIFF, frozendict({"compression": "lzw"})),
    (ManagedCodec.JPEG, frozendict({"Q": 92})),
    (ManagedCodec.WEBP, frozendict({"Q": 92, "lossless": True})),
])
# the managed RenderingIntent nickname -> the Pillow ImageCms.Intent member name, resolved inside the worker so no PIL symbol touches the parent loader path; the ICC codes stay named, never magic ints.
_PROOF_INTENT_NAME: Final[Map[str, str]] = Map.of_seq([
    (RenderingIntent.PERCEPTUAL.value, "PERCEPTUAL"),
    (RenderingIntent.RELATIVE.value, "RELATIVE_COLORIMETRIC"),
    (RenderingIntent.SATURATION.value, "SATURATION"),
    (RenderingIntent.ABSOLUTE.value, "ABSOLUTE_COLORIMETRIC"),
    (RenderingIntent.AUTO.value, "PERCEPTUAL"),
])


@tagged_union(frozen=True)
class GradeStep:
    tag: Literal["cctf", "broadcast", "colourspace", "correction", "lut"] = tag()
    cctf: tuple[Transfer, ToneCurve] = case()
    broadcast: tuple[TransferKind, BroadcastCurve] = case()
    colourspace: tuple[RgbSpace, RgbSpace, AdaptMethod] = case()
    correction: NDArray[np.float64] = case()
    lut: tuple[str, ...] = case()

    @staticmethod
    def Cctf(direction: Transfer, curve: ToneCurve = ToneCurve.SRGB) -> "GradeStep":
        return GradeStep(cctf=(direction, curve))

    @staticmethod
    def Broadcast(kind: TransferKind, curve: BroadcastCurve = BroadcastCurve.BT709) -> "GradeStep":
        return GradeStep(broadcast=(kind, curve))

    @staticmethod
    def Colourspace(source: RgbSpace, target: RgbSpace, adapt: AdaptMethod = AdaptMethod.BRADFORD) -> "GradeStep":
        return GradeStep(colourspace=(source, target, adapt))

    @staticmethod
    def Correction(
        ccm: NDArray[np.float64], /
    ) -> "GradeStep":  # the device-calibration CCM is fitted upstream at derive#DERIVE; this step only applies it
        return GradeStep(correction=ccm)

    @staticmethod
    def Lut(*paths: str) -> "GradeStep":
        return GradeStep(lut=paths)


@dataclass(frozen=True, slots=True, kw_only=True)
class SpotChannel:
    name: str
    coverage: float  # ink coverage percentage from the CxF SpotColorType


@dataclass(frozen=True, slots=True, kw_only=True)
class IccTransform:
    intent: RenderingIntent = RenderingIntent.RELATIVE
    bpc: bool = True
    pcs: ConnectionSpace = ConnectionSpace.LAB
    depth: BitDepth = BitDepth.UINT8
    proof: bytes | None = (
        None  # a press/proof ICC profile; when set the to_process worker soft-proofs it and counts out-of-gamut pixels — the lcms2 role pyvips has no member for
    )
    separations: tuple[
        SpotChannel, ...
    ] = ()  # the CxF ColorCmykplusN spot declaration separations() decodes; its count rides ManagedFact.SPOTS as preflight evidence — pyvips converts to the CMYK profile but authors no named DeviceN channel


_ICC_DEFAULT: IccTransform = IccTransform()


@tagged_union(frozen=True)
class ManageOp:
    tag: ManageOpTag = tag()
    managed: tuple[ManagedRaster, ProfileRef, ProfileRef, IccTransform, ManagedCodec, tuple[GradeStep, ...]] = case()
    export: tuple[NDArray[np.float64], str, BitDepth, tuple[GradeStep, ...]] = case()
    plate: tuple[bytes, tuple[SpotChannel, ...], IccTransform] = case()
    lut: tuple[RgbSpace, RenderingIntent, tuple[GradeStep, ...], int, str] = case()
    swatch: bytes = case()

    @staticmethod
    def Managed(
        raster: ManagedRaster,
        src_profile: ProfileRef,
        dst_profile: ProfileRef,
        transform: IccTransform = _ICC_DEFAULT,
        codec: ManagedCodec = ManagedCodec.PNG,
        grade: tuple[GradeStep, ...] = (),
    ) -> "ManageOp":
        return ManageOp(managed=(raster, src_profile, dst_profile, transform, codec, grade))

    @staticmethod
    def Export(field: NDArray[np.float64], path: str, depth: BitDepth = BitDepth.UINT16, grade: tuple[GradeStep, ...] = ()) -> "ManageOp":
        return ManageOp(export=(field, path, depth, grade))

    @staticmethod
    def Plate(document: bytes, channels: tuple[SpotChannel, ...], transform: IccTransform = _ICC_DEFAULT) -> "ManageOp":
        return ManageOp(plate=(document, channels, transform))

    @staticmethod
    def Lut(
        space: RgbSpace,
        path: str,
        grade: tuple[GradeStep, ...] = (),
        size: int = 33,
        intent: RenderingIntent = RenderingIntent.RELATIVE,
    ) -> "ManageOp":
        return ManageOp(lut=(space, intent, grade, size, path))

    @staticmethod
    def Swatch(document: bytes) -> "ManageOp":
        return ManageOp(swatch=document)


class ColorManaged(Struct, frozen=True):
    op: ManageOp

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=1.0)

    @property
    def _key(self) -> ContentKey:
        # key-over-INPUT: canonical ManageOp payload minted PRE-RUN — the produced blob's own address rides the score band.
        return ContentIdentity.of(f"color-managed-{self.op.tag}", self.op, policy=CANONICAL_POLICY)

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        return await async_boundary(f"color.managed.{self.op.tag}", self._compute)

    async def _compute(self) -> ArtifactReceipt:
        match self.op:
            case ManageOp(tag="managed", managed=(raster, src_profile, dst_profile, transform, codec, grade)):
                # the whole normalize+grade+ICC pass and the optional lcms2 soft-proof ride the to_process worker, never the loop.
                crossed = await LanePolicy.offload(
                    _icc_apply,
                    raster,
                    src_profile,
                    dst_profile,
                    transform.intent.value,
                    transform.bpc,
                    transform.pcs.value,
                    _DEPTH_BITS[transform.depth],
                    codec,
                    grade,
                    transform.proof,
                    len(transform.separations),
                    modality=Modality.PROCESS,
                    retry=RetryClass.OCCT,
                )
                blob, width, height, bands, embedded, space, gamut, spots, ink = crossed.default_with(_managed_raise)
                scores: frozendict[str, float | str] = frozendict({
                    ManagedFact.INTENT.value: transform.intent.value,
                    ManagedFact.BPC.value: float(transform.bpc),
                    ManagedFact.PCS.value: transform.pcs.value,
                    ManagedFact.DEPTH.value: transform.depth.value,
                    ManagedFact.CODEC.value: codec.value,
                    ManagedFact.BANDS.value: float(bands),
                    ManagedFact.EMBEDDED.value: float(embedded),
                    ManagedFact.BYTES.value: float(len(blob)),
                    ManagedFact.GRADE.value: float(len(grade)),
                    ManagedFact.SPACE.value: space,
                    ManagedFact.GAMUT.value: float(gamut),
                    ManagedFact.SPOTS.value: float(spots),
                    ManagedFact.INK.value: ink,
                })
                return ArtifactReceipt.Preview(self._key, width, height, scores)
            case ManageOp(tag="export", export=(field, path, depth, grade)):
                # the grade fold and the blocking write ride one to_thread slot; the worker returns the toned array (no pickle) so the content key reads one stored fact.
                toned = (await LanePolicy.offload(_export_image, field, path, depth.value, grade, modality=Modality.THREAD, retry=RetryClass.OCCT)).default_with(_managed_raise)
                height, width = toned.shape[0], toned.shape[1]
                scores = frozendict({
                    ManagedFact.DEPTH.value: depth.value,
                    ManagedFact.BYTES.value: float(toned.nbytes),
                    ManagedFact.GRADE.value: float(len(grade)),
                })
                return ArtifactReceipt.Preview(ContentIdentity.of(f"color-export-{depth.value}", toned.tobytes()), width, height, scores)
            case ManageOp(tag="plate", plate=(document, channels, transform)):
                # Separation/DeviceN plate authoring rides one thread slot; the declared TAC (the overprint
                # coverage sum) and the per-spot coverages graduate to the Color case, the receipt keyed pre-run.
                plated, pages = (
                    await LanePolicy.offload(_plate_author, document, channels, modality=Modality.THREAD, retry=RetryClass.OCCT)
                ).default_with(_managed_raise)
                facts: frozendict[str, float | str] = frozendict({
                    ManagedFact.BYTES.value: float(len(plated)),
                    ManagedFact.PAGES.value: float(pages),
                    **{f"spot:{channel.name}": channel.coverage for channel in channels},
                })
                return ArtifactReceipt.Color(
                    self._key,
                    "device_n" if len(channels) > 1 else "separation",
                    transform.intent.value,
                    sum(channel.coverage for channel in channels),
                    len(channels),
                    facts,
                )
            case ManageOp(tag="lut", lut=(space, intent, grade, size, path)):
                # the LUT-authoring terminal: the baked lattice writes on a thread slot; a LUT lays down no ink,
                # so tac_peak/plates project 0 and the band carries the lattice facts.
                entries = (
                    await LanePolicy.offload(_lut_author, space, grade, size, path, modality=Modality.THREAD, retry=RetryClass.OCCT)
                ).default_with(_managed_raise)
                return ArtifactReceipt.Color(
                    self._key,
                    space.value,
                    intent.value,
                    0.0,
                    0,
                    frozendict({ManagedFact.GRADE.value: float(len(grade)), ManagedFact.SIZE.value: float(size), ManagedFact.ENTRIES.value: float(entries)}),
                )
            case ManageOp(tag="swatch", swatch=document):
                # the CxF3 spot-swatch terminal: the decoded ColorCmykplusN declaration graduates to Color
                # evidence; ABSOLUTE is the paper-white spot-proofing intent, the coverage sum the declared TAC.
                channels = separations(document)
                return ArtifactReceipt.Color(
                    self._key,
                    "cmyk_plus_n",
                    RenderingIntent.ABSOLUTE.value,
                    sum(channel.coverage for channel in channels),
                    len(channels),
                    frozendict({f"spot:{channel.name}": channel.coverage for channel in channels}),
                )
            case _:
                assert_never(self.op)


def _grade(field: Tristimulus, steps: tuple[GradeStep, ...]) -> Tristimulus:
    # the shared ordered-chain fold; module-level so it runs INSIDE the worker, never on the loop — offload-resident by construction.
    def applied(acc: Tristimulus, step: GradeStep) -> Tristimulus:
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


def _export_image(field: Tristimulus, path: str, depth: str, grade: tuple[GradeStep, ...]) -> Tristimulus:
    # the to_thread export worker: grade fold then the bit-depth-correct blocking write; the toned array returns (no pickle) so the content key reads one stored fact.
    toned = _grade(field, grade)
    colour.write_image(toned, path, depth)
    return toned


def _softproof(rgb8: NDArray[np.uint8], reference: str | ImageCms.ImageCmsProfile, proof_path: str, intent: str) -> int:
    # simulate the press/proof profile (buildProofTransform, reference as input+display, proof the press); the plain-vs-GAMUTCHECK
    # output diff marks the out-of-press-gamut pixels — the lcms2 PDF/X preflight signal pyvips has no member for.
    origin = PilImage.fromarray(rgb8, "RGB")
    intent_member = getattr(ImageCms.Intent, _PROOF_INTENT_NAME[intent])
    proof_intent = ImageCms.Intent.ABSOLUTE_COLORIMETRIC  # paper-white-simulating proof intent, the soft-proof standard
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


def _managed_raise(fault: object) -> object:
    # terminal collapse at the native seam: an offload fault reconstructs the raise the node's rail folds.
    raise ValueError(str(fault))


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
    proof: bytes | None,
    spots: int,
) -> tuple[bytes, int, int, int, bool, str, int, int, float]:
    toned = _grade(raster / np.float64(np.iinfo(raster.dtype).max), grade)  # normalize-to-unit then grade, inside the worker

    def named(stack: ExitStack, profile: ProfileRef, /) -> str:
        if isinstance(profile, str):  # BuiltinProfile is a StrEnum, so a device name passes straight to liblcms2
            return profile
        handle = stack.enter_context(NamedTemporaryFile(suffix=".icc", delete_on_close=False))
        handle.write(profile)
        handle.close()
        return handle.name

    image = pyvips.Image.new_from_array(toned)
    with ExitStack() as stack:  # the profile temp files must outlive the lazy icc_transform until write_to_buffer pulls pixels
        src_path = named(stack, src)
        managed = image.icc_transform(named(stack, dst), input_profile=src_path, intent=intent, black_point_compensation=bpc, pcs=pcs, depth=depth)
        # a bytes src carries a real ICC path lcms2 opens; a BuiltinProfile device name has no lcms2 built-in beyond sRGB, so the soft-proof references the sRGB display.
        reference = src_path if isinstance(src, bytes) else ImageCms.createProfile("sRGB")
        gamut = (
            _softproof(np.clip(np.asarray(toned)[..., :3] * 255.0, 0.0, 255.0).astype(np.uint8), reference, named(stack, proof), intent)
            if proof is not None
            else 0
        )
        space = str(managed.interpretation)  # the egress libvips Interpretation (cmyk for a CMYK/separations target, srgb otherwise)
        # Total Area Coverage — the peak (C+M+Y+K) ink sum over the converted CMYK field, the ISO 12647 / PDF-X-4 ink-limit
        # preflight paired with gamut. cmyk guarantees 4 bands, so the band sum's maxpos()[0] peak normalizes to the depth ceiling; 0.0 for a non-cmyk egress.
        ink = float((managed[0] + managed[1] + managed[2] + managed[3]).maxpos()[0]) / float((1 << depth) - 1) * 100.0 if space == "cmyk" else 0.0
        return (
            managed.write_to_buffer(
                codec.value, keep=pyvips.ForeignKeep.ICC, **_CODEC_OPTS[codec]
            ),  # explicit ICC retention on egress, never default-metadata-retention alone
            managed.width,
            managed.height,
            managed.bands,
            managed.get_typeof("icc-profile-data") != 0,
            space,
            gamut,
            spots,
            ink,
        )


def _plate_author(document: bytes, channels: tuple[SpotChannel, ...]) -> tuple[bytes, int]:
    # the write-side plate model: one /Separation colorspace per spot (Type 2 K-ramp tint; the CMYK alternate
    # only drives non-separating viewers) plus the joint /DeviceN for a multi-channel set (Type 4 calculator
    # folding the N tints onto min(1, sum) K), registered on every page under its ink name.
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


def _lut_author(space: RgbSpace, grade: tuple[GradeStep, ...], size: int, path: str) -> int:
    # bake the grade chain into a LUT3D lattice: the identity cube folds through the SAME `_grade` law the raster arms
    # apply, so a baked .cube/.clf and a live grade cannot disagree; write_LUT keys the container format off the path suffix.
    axis = np.linspace(0.0, 1.0, size)
    r, g, b = np.meshgrid(axis, axis, axis, indexing="ij")
    colour.write_LUT(colour.LUT3D(table=_grade(np.stack([r, g, b], axis=-1), grade), name=f"rasm-{space.value}"), path)
    return size**3


def separations(document: bytes, /) -> tuple[SpotChannel, ...]:
    # decode the CxF3 DEVICE half — the ColorCmykplusN spot declaration (managed owns the device half; the spectral/Lab
    # color half is derive#DERIVE's), each named SpotColorType channel at its coverage.
    resources = read_cxf(document).resources
    collection = resources.object_collection if resources else None
    return tuple(
        SpotChannel(name=str(spot.name or ""), coverage=float(spot.percentage or 0.0))
        for obj in (collection.object_value if collection else ())
        for device in ((obj.device_color_values,) if obj.device_color_values else ())  # narrow the optional DeviceColorValues to non-None once
        for member in device.choice
        if isinstance(member, cxf3.ColorCmykplusN)
        for spot in member.spot_color
    )
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
