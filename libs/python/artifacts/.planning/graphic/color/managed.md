# [PY_ARTIFACTS_GRAPHIC_COLOR_MANAGED]

The ICC/LUT/CCTF color-managed raster-egress owner — the one native-`libvips` process-seam leg of the color sub-domain. `ColorManaged` is ONE owner that, for the `Managed` arm, crosses the runtime worker subprocess seam where the colour-science grade chain folds AND pyvips `icc_transform` applies the source->destination ICC transform under one `IccTransform` policy bundle (`RenderingIntent` + black-point compensation + `ConnectionSpace` PCS + output `BitDepth` + an optional lcms2 soft-proof profile + a CxF `ColorCmykplusN` N-channel separations declaration) and writes through one `ManagedCodec` egress that embeds the destination profile under an explicit `ForeignKeep.ICC` retention flag. Pillow `ImageCms.buildProofTransform` rides the SAME `to_process` worker for the one surviving lcms2 role pyvips has no member for — a soft-proof under `Flags.SOFTPROOFING | Flags.GAMUTCHECK` counting the out-of-press-gamut pixels for PDF/X separations preflight, a DISTINCT control from the deleted device-egress `buildTransform` — while `colour_cxf.read_cxf` decodes the CxF3 device half (`ColorCmykplusN` + its named `SpotColor` channels) into the `SpotChannel` separations declaration the CMYK/DeviceN egress records on `ManagedFact.SPOTS` as separations-preflight evidence — the spot count crosses to the worker and pyvips `icc_transform` converts to the CMYK profile, but libvips authors no named DeviceN channel, so the declaration is carried evidence, not an applied device space. colour-science (pure-Python) carries the `cctf_encoding`/`cctf_decoding`, `oetf`/`eotf`/`ootf`, `RGB_to_RGB`, `apply_matrix_colour_correction`, and `read_LUT`->`LUTSequence.apply` grade chain plus the bit-depth-correct `write_image` egress, and BOTH arms fold that multi-MB-image chain INSIDE their offload worker — the `Managed` arm's `to_process` ICC worker, the `Export` arm's `to_thread` write worker — so no heavy full-image grade ever runs on the event loop nor as a `toned` arg evaluated before the offload; pyvips (`icc_transform`/`Intent`, the `liblcms2`-backed ICC engine the sibling `graphic/raster/io#RASTER` owns, binding a Forge-provisioned native `libvips` that is not on the runtime loader path) carries the genuinely-native ICC apply that cannot resolve in the runtime process, so the `Managed` arm dispatches the raster-plus-grade onto the `runtime/reliability/faults#FAULT`-owned the runtime process lane worker subprocess seam through the module-level `_icc_apply` function bounded by the `anyio.CapacityLimiter` offload slot. A profile crosses as a `ProfileRef` — raw `bytes` materialized to a temp file, or a `BuiltinProfile` device name passed straight to `liblcms2` — so a managed conversion to `srgb`/`p3`/`cmyk` needs no profile file. `ManageOp` is ONE closed family carrying each operation's typed payload, never an erased `params` bag, dispatched by one total `match` returning `RuntimeRail[ArtifactReceipt]` — keying the color-managed bytes through `ContentIdentity.of` and contributing the settled `core/receipt#RECEIPT` `ArtifactReceipt.Preview` case, whose `scores` band carries the applied intent, BPC, PCS, output depth/bands/codec, profile-embedded flag, byte volume, grade-step count, egress interpretation, soft-proof out-of-gamut count, separations spot count, and the peak Total-Area-Coverage ink limit under the closed `ManagedFact` evidence vocabulary the pyvips image-op `evidence` axis names — the one receipt family every artifacts producer contributes to, never a parallel managed-receipt type. The plate, LUT, and swatch terminals mint the `ArtifactReceipt.Color` case — `Plate` authors `/Separation`/`/DeviceN` spot colorspaces over the pikepdf raw object model, `Lut` bakes the grade chain into a `LUT3D` lattice written through `colour.write_LUT`, `Swatch` graduates a decoded CxF3 `ColorCmykplusN` declaration — each projecting the `(space, intent, tac_peak, plates, band)` evidence shape, the declared overprint coverage sum the `tac_peak`, the channel count the `plates`, the per-spot coverages the band. It consumes the grade chain and color-space provenance `graphic/color/derive#DERIVE` resolves and hands the color-managed raster to `graphic/raster/io#RASTER` and `document/emit#DOCUMENT`, so figure and PDF output is color-managed rather than naive sRGB. This page closes the `ICC_COLOR_MANAGED_OUTPUT` idea — the color-managed egress side of the visual pipeline.

## [01]-[INDEX]

- [01]-[MANAGED]: the ICC/LUT/CCTF color-managed raster-egress owner over the closed `ManageOp` family — `Managed` crossing the `to_process` worker where the `GradeStep` chain folds and pyvips `icc_transform` applies the source->destination ICC transform under one `IccTransform` policy bundle (`RenderingIntent`/black-point compensation/`ConnectionSpace` PCS/output `BitDepth`), `Export` crossing one `to_thread` slot where the same chain folds and the bit-depth-correct `colour.write_image` writes — each keyed through `ContentIdentity.of` into the settled `core/receipt#RECEIPT` `ArtifactReceipt.Preview` case whose `scores` band carries the applied intent/BPC/PCS/depth/codec/bands/embedded/bytes/grade-count plus the egress interpretation, the soft-proof out-of-gamut count, the separations spot count, and the peak Total-Area-Coverage ink limit under the closed `ManagedFact` vocabulary — and the `Plate`/`Lut`/`Swatch` terminals minting the `ArtifactReceipt.Color` case with the `(space, intent, tac_peak, plates, band)` evidence shape; the module-level `_grade` fold over the ordered chain (`cctf`/`broadcast`/`colourspace`/`correction`/`lut`) is the one core the raster arms and the LUT baker reach, never run on the event loop.

## [02]-[MANAGED]

- Cases: `ManageOp` cases — `Managed(raster, src_profile, dst_profile, transform, codec, grade)` (the `uint8`/`uint16` raster and its `GradeStep` chain crossing the `to_process` subprocess seam where the worker normalizes to the unit range by the dtype maximum, folds the colour-science grade chain, and applies the pyvips `icc_transform` source/destination transform under the `IccTransform` bundle — the one native-`libvips` subprocess leg carrying the whole normalize+grade+ICC pass off the loop — with the destination profile embedded into the `ManagedCodec` output, the managed bytes keyed through `ContentIdentity.of` into the `ArtifactReceipt.Preview` content key with the ICC evidence on `scores`) · `Export(field, path, depth, grade)` (the field and its `GradeStep` chain crossing one `anyio.to_thread` slot where the worker folds the grade chain then runs the bit-depth-correct `colour.write_image` at the `BitDepth` policy for the blocking disk write — both the grade and the write off the loop, the worker returning the toned array so the written field keys through `ContentIdentity.of` with the `depth` folded into the content key so two depths of one field never collide, projecting the `ArtifactReceipt.Preview` content key with the egress evidence on `scores`, crossing no subprocess — removing the native-`libvips` process seam from non-ICC image egress) · `Plate(document, channels, transform)` (the PDF and its `SpotChannel` declaration crossing one thread slot where `_plate_author` authors one `/Separation` colorspace per spot — `Name`/`Array`/`Dictionary` over the pikepdf raw object model, a Type 2 K-ramp tint — plus the joint `/DeviceN` with its Type 4 `min(1, sum)` calculator for a multi-channel set, registers each under its ink name on every page through `add_resource`, and saves; the arm mints `ArtifactReceipt.Color` with `space` `separation`/`device_n`, the transform intent, the declared-overprint `tac_peak`, the `plates` count, and the per-spot coverage band) · `Lut(space, path, grade, size, intent)` (the ordered `GradeStep` chain baked into a `colour.LUT3D` identity lattice inside one thread slot and written through `colour.write_LUT` — the container format keyed off the path suffix — minting `ArtifactReceipt.Color` with the working `space`, the authoring `intent`, zero `tac_peak`/`plates`, and the lattice facts band) · `Swatch(document)` (the CxF3 device half decoded through `separations` and graduated directly to `ArtifactReceipt.Color` — `cmyk_plus_n` space, the `ABSOLUTE` paper-white spot-proofing intent, the coverage-sum `tac_peak`, the channel `plates`, the per-spot band) — matched by one total `match`/`case`. The `_grade` fold over the ordered `GradeStep` chain — `cctf` CCTF direction, `broadcast` OETF/EOTF/OOTF transfer, `colourspace` `RGB_to_RGB` primary-matrix conversion under a selected `AdaptMethod` transform, `correction` `apply_matrix_colour_correction` measured-CCM apply, `lut` `LUTSequence` — is the shared module-level fold both arms reach INSIDE their offload worker, never duplicated per arm and never run on the event loop.
- Auto: both arms fold the ordered `GradeStep` chain through the module-level `_grade` INSIDE their offload worker (never on the loop) via `Block.of_seq(grade).fold` — a `cctf` step applies `_TRANSFER[direction](acc, function=curve.value)` (the `colour.cctf_encoding`/`cctf_decoding` direction keyed by `Transfer`, the function name by `ToneCurve`), a `broadcast` step applies `_BROADCAST[kind](acc, function=curve.value)` (the `colour.oetf`/`eotf`/`ootf` direction keyed by `TransferKind`, the named function by `BroadcastCurve`), a `colourspace` step applies `colour.RGB_to_RGB(acc, RGB_COLOURSPACES[source], RGB_COLOURSPACES[target], chromatic_adaptation_transform=adapt)` (the 3x3 primary-matrix conversion keyed by `RgbSpace`, its chromatic-adaptation transform keyed by `AdaptMethod`), a `correction` step applies `colour.apply_matrix_colour_correction(acc, ccm)` (the pre-derived measured device-calibration `CCM` applied in-line, the matrix fitted upstream at `graphic/color/derive#DERIVE`), and a `lut` step applies `colour.LUTSequence(*(colour.read_LUT(path) for path in paths)).apply(acc)`, so decode-linearize, transfer, primary-convert, device-correct, LUT-grade, and re-encode interleave in one ordered chain; `Managed` crosses the `to_process.run_sync(_icc_apply, raster, ..., grade, limiter=_ICC_LANE)` seam where `_icc_apply` normalizes the `uint8`/`uint16` raster to the unit range by its dtype maximum (`raster / np.iinfo(raster.dtype).max`), folds it through `_grade`, lifts the toned array through `pyvips.Image.new_from_array`, resolves each `ProfileRef` through `named` (a `BuiltinProfile` device name passed straight, raw `bytes` written to a `NamedTemporaryFile(delete_on_close=False)` entered on one `ExitStack`), runs `image.icc_transform(dst, input_profile=src, intent=intent, black_point_compensation=bpc, pcs=pcs, depth=depth)`, reads back the embedded destination profile through `get_typeof("icc-profile-data")` and the egress `interpretation` (cmyk for a separations target, srgb otherwise), runs the optional Pillow `_softproof` when a `proof` profile is set (`buildProofTransform` under `Flags.SOFTPROOFING | Flags.GAMUTCHECK`, the plain-vs-gamut-check output diff counting out-of-press-gamut pixels — the lcms2 preflight signal pyvips lacks), reads the peak Total Area Coverage off the converted CMYK field for a separations egress (the 4 CMYK bands sum through the band-index `+` overload and `maxpos()[0]` reads the peak per-pixel ink, normalized to the depth ceiling as an `ink` % — the ISO 12647 / PDF-X-4 ink-limit preflight paired with `gamut`, `0.0` for a non-cmyk egress), and returns the `write_to_buffer(codec.value, keep=pyvips.ForeignKeep.ICC, **_CODEC_OPTS[codec])` managed bytes plus width/height/`bands`/embedded/space/gamut/spot-count/ink-TAC, which key through `ContentIdentity.of("color-managed", blob)` and project `ArtifactReceipt.Preview` with the `ManagedFact` ICC + soft-proof + separations evidence band; `Export` crosses the `to_thread.run_sync(_export_image, field, path, depth.value, grade, limiter=_EXPORT_LANE)` slot where `_export_image` folds the field through `_grade` then runs the bit-depth-correct `colour.write_image(toned, path, depth.value)` blocking disk write (`bit_depth` passed positionally) and returns the toned array, then keys the toned field bytes through `ContentIdentity.of(f"color-export-{depth.value}", toned.tobytes())` (the `depth` in the content key so two depths of one field never collide) and projects `ArtifactReceipt.Preview` with the field dimensions and the `depth`/`bytes`/`grade` evidence band. The grade chain runs once INSIDE each arm's offload worker — never on the event loop and never as a `toned` arg evaluated before the offload — the ICC apply riding the process band and the non-ICC write riding a thread slot.
- Growth: a new managed operation is one `ManageOp` case carrying its payload plus one acceptor arm contributing its receipt case — `Preview` for a raster egress, `Color` for a plate/LUT/swatch terminal (the `Plate`/`Lut`/`Swatch` arms are exactly this growth) — its `tag` extending the one `ManageOpTag` literal; a new grade step is one `GradeStep` case folded by `_grade` (an HDR tone-mapping operator, an explicit ASC-CDL slope/offset/power op beyond the `.clf` `lut` path); the soft-proof control (a `proof` press-profile field on the `IccTransform` bundle plus the `GAMUT` out-of-gamut-pixel count and `SPACE` egress-interpretation `ManagedFact` rows) and the separations declaration (a `separations` `SpotChannel`-tuple field `separations()` decodes from the CxF `ColorCmykplusN` device half plus the `SPOTS` count `ManagedFact` row) are exactly that in-place growth — one field on the kw-only `IccTransform` bundle plus one `ManagedFact` row, never a payload-arity change breaking every `Managed` call; a further ICC control (a device-link profile, an explicit N-color proof intent) lands the same way; a new output format is one `ManagedCodec` row plus one `_CODEC_OPTS` encoder-policy row; a new built-in device profile is one `BuiltinProfile` row keying `input_profile`/`output_profile` directly; a new CCTF function is one `ToneCurve` row keying `CCTF_ENCODINGS`/`CCTF_DECODINGS`; a new broadcast function is one `BroadcastCurve` row keying `OETFS`/`EOTFS`/`OOTFS`; a new primary space is one `RgbSpace` row keying `RGB_COLOURSPACES`; a new chromatic-adaptation transform is one `AdaptMethod` row keying `CHROMATIC_ADAPTATION_TRANSFORMS`; a new transfer direction is one `Transfer`/`TransferKind` row; a new rendering intent is one `RenderingIntent` row mirroring its pyvips `Intent` nickname; a new PCS is one `ConnectionSpace` row; a new output bit depth is one `BitDepth` row keying `colour.write_image` plus one `_DEPTH_BITS` libvips-clamp row; a new evidence scalar is one `ManagedFact` row plus one `_icc_apply` return slot — the `INK` Total-Area-Coverage preflight (the peak CMYK band-sum over the converted field) is exactly that in-place growth, one `ManagedFact.INK` row and one worker-computed slot beside `gamut`/`spots`, never a second preflight rail; never a parallel managed surface, never a second image writer; zero new surface.

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

lazy import pikepdf  # Separation/DeviceN plate authoring over the raw object model; reifies on first `_plate_author` use in the thread worker
lazy import pyvips  # native libvips worker gate: the proxy reifies on first `_icc_apply` use in the to_process worker, keeping libvips off the parent loader path
lazy from PIL import (
    Image as PilImage,
    ImageCms,
)  # lcms2 soft-proof/gamut-warning only (the pillow role pyvips has no member for); reifies inside the to_process worker beside pyvips, never the device-egress engine
lazy from colour_cxf import (
    cxf3,
    read_cxf,
)  # CxF3 device-half decode for the ColorCmykplusN N-channel separations declaration; reifies in the parent when separations() is first called

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
# the managed RenderingIntent nickname -> the Pillow ImageCms.Intent member name, resolved inside the soft-proof worker so no PIL symbol touches the parent loader path and the ICC intent codes stay named, never magic ints.
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
        None  # a press/proof ICC profile; when set the to_process worker soft-proofs it (buildProofTransform + Flags.SOFTPROOFING|GAMUTCHECK) and counts out-of-gamut pixels — the pillow lcms2 role pyvips has no member for, a DISTINCT control from the deleted device-egress buildTransform
    )
    separations: tuple[
        SpotChannel, ...
    ] = ()  # the CxF ColorCmykplusN N-channel spot declaration separations() decodes; its count crosses to the worker and rides ManagedFact.SPOTS as separations-preflight evidence (pyvips icc_transform converts to the CMYK profile but authors no named DeviceN channel)


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
                # the whole normalize+grade+ICC pass AND the optional lcms2 soft-proof ride the to_process worker — the full-image
                # grade never runs on the loop nor as a `toned` arg evaluated before the offload; the raster + grade chain cross the seam.
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
                # the grade fold AND the blocking write ride one to_thread slot off the loop; the worker returns the toned
                # array (same address space, no pickle) so the content key, dims, and byte volume read the one stored fact.
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
    # the shared ordered-chain fold both worker arms reach; it is module-level (not a class method) so it runs INSIDE the
    # to_process/to_thread worker, never on the event loop — the heavy full-image transform is offload-resident by construction.
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
    # the to_thread export worker: grade fold then the bit-depth-correct blocking write, both off the loop; the toned array
    # returns to the caller (shared address space, no pickle) so the content key, dims, and byte volume read one stored fact.
    toned = _grade(field, grade)
    colour.write_image(toned, path, depth)
    return toned


def _softproof(rgb8: NDArray[np.uint8], reference: str | ImageCms.ImageCmsProfile, proof_path: str, intent: str) -> int:
    # simulate the press/proof profile against the source colours (buildProofTransform, reference as input+display, proof the press);
    # the plain-vs-GAMUTCHECK output diff marks the out-of-press-gamut pixels — the lcms2 PDF/X separations-preflight signal pyvips has no member for.
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
        # Total Area Coverage — the peak (C+M+Y+K) ink sum over the converted CMYK field, the ISO 12647 / PDF-X-4
        # ink-limit preflight paired with the soft-proof `gamut` count. The `cmyk` egress guarantees 4 bands, so the
        # band-index extract + `+` overload sum the ink and `maxpos()[0]` reads its peak, normalized to the depth
        # ceiling as a percentage; 0.0 for a non-separations (non-cmyk) egress that lays down no press ink.
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
    # the write-side plate model the read-side PdfImage.is_separation/is_device_n flags mirror: one
    # /Separation colorspace per spot (Type 2 K-ramp tint absent a measured CMYK alternate — the alternate
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
    # bake the ordered grade chain into a LUT3D lattice: the identity cube folds through the SAME `_grade`
    # law the raster arms apply, so a baked .cube/.clf and a live grade cannot disagree; write_LUT keys the
    # container format off the path suffix.
    axis = np.linspace(0.0, 1.0, size)
    r, g, b = np.meshgrid(axis, axis, axis, indexing="ij")
    colour.write_LUT(colour.LUT3D(table=_grade(np.stack([r, g, b], axis=-1), grade), name=f"rasm-{space.value}"), path)
    return size**3


def separations(document: bytes, /) -> tuple[SpotChannel, ...]:
    # decode the CxF3 DEVICE half — the ColorCmykplusN N-channel spot declaration the CMYK/DeviceN egress applies (managed owns the
    # device half per colour-cxf; the spectral/Lab color half is derive#DERIVE's), each named SpotColorType channel at its coverage.
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
