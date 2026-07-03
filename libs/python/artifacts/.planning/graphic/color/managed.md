# [PY_ARTIFACTS_GRAPHIC_COLOR_MANAGED]

The ICC/LUT/CCTF color-managed raster-egress owner — the one native-`libvips` process-seam leg of the color sub-domain. `ColorManaged` is ONE owner that, for the `Managed` arm, crosses the runtime worker subprocess seam where the colour-science grade chain folds AND pyvips `icc_transform` applies the source->destination ICC transform under one `IccTransform` policy bundle (`RenderingIntent` + black-point compensation + `ConnectionSpace` PCS + output `BitDepth` + an optional lcms2 soft-proof profile + a CxF `ColorCmykplusN` N-channel separations declaration) and writes through one `ManagedCodec` egress that embeds the destination profile under an explicit `ForeignKeep.ICC` retention flag. Pillow `ImageCms.buildProofTransform` rides the SAME `to_process` worker for the one surviving lcms2 role pyvips has no member for — a soft-proof under `Flags.SOFTPROOFING | Flags.GAMUTCHECK` counting the out-of-press-gamut pixels for PDF/X separations preflight, a DISTINCT control from the deleted device-egress `buildTransform` — while `colour_cxf.read_cxf` decodes the CxF3 device half (`ColorCmykplusN` + its named `SpotColor` channels) into the `SpotChannel` separations declaration the CMYK/DeviceN egress records on `ManagedFact.SPOTS` as separations-preflight evidence — the spot count crosses to the worker and pyvips `icc_transform` converts to the CMYK profile, but libvips authors no named DeviceN channel, so the declaration is carried evidence, not an applied device space. colour-science (pure-Python) carries the `cctf_encoding`/`cctf_decoding`, `oetf`/`eotf`/`ootf`, `RGB_to_RGB`, `apply_matrix_colour_correction`, and `read_LUT`->`LUTSequence.apply` grade chain plus the bit-depth-correct `write_image` egress, and BOTH arms fold that multi-MB-image chain INSIDE their offload worker — the `Managed` arm's `to_process` ICC worker, the `Export` arm's `to_thread` write worker — so no heavy full-image grade ever runs on the event loop nor as a `toned` arg evaluated before the offload; pyvips (`icc_transform`/`Intent`, the `liblcms2`-backed ICC engine the sibling `graphic/raster/io#RASTER` owns, binding a Forge-provisioned native `libvips` that is not on the runtime loader path) carries the genuinely-native ICC apply that cannot resolve in the runtime process, so the `Managed` arm dispatches the raster-plus-grade onto the `runtime/reliability/faults#FAULT`-owned `anyio.to_process.run_sync` worker subprocess seam through the module-level `_icc_apply` function bounded by the `anyio.CapacityLimiter` offload slot. A profile crosses as a `ProfileRef` — raw `bytes` materialized to a temp file, or a `BuiltinProfile` device name passed straight to `liblcms2` — so a managed conversion to `srgb`/`p3`/`cmyk` needs no profile file. `ManageOp` is ONE closed family carrying each operation's typed payload, never an erased `params` bag, dispatched by one total `match` returning `RuntimeRail[ArtifactReceipt]` — keying the color-managed bytes through `ContentIdentity.of` and contributing the settled `core/receipt#RECEIPT` `ArtifactReceipt.Preview` case, whose `scores` band carries the applied intent, BPC, PCS, output depth/bands/codec, profile-embedded flag, byte volume, grade-step count, egress interpretation, soft-proof out-of-gamut count, separations spot count, and the peak Total-Area-Coverage ink limit under the closed `ManagedFact` evidence vocabulary the pyvips image-op `evidence` axis names — the one receipt family every artifacts producer contributes to, never a parallel managed-receipt type. It consumes the grade chain and color-space provenance `graphic/color/derive#DERIVE` resolves and hands the color-managed raster to `graphic/raster/io#RASTER` and `document/emit#DOCUMENT`, so figure and PDF output is color-managed rather than naive sRGB. This page closes the `ICC_COLOR_MANAGED_OUTPUT` idea — the color-managed egress side of the visual pipeline.

## [01]-[INDEX]

- [01]-[MANAGED]: the ICC/LUT/CCTF color-managed raster-egress owner over the closed `ManageOp` family — `Managed` crossing the `to_process` worker where the `GradeStep` chain folds and pyvips `icc_transform` applies the source->destination ICC transform under one `IccTransform` policy bundle (`RenderingIntent`/black-point compensation/`ConnectionSpace` PCS/output `BitDepth`), `Export` crossing one `to_thread` slot where the same chain folds and the bit-depth-correct `colour.write_image` writes — each keyed through `ContentIdentity.of` into the settled `core/receipt#RECEIPT` `ArtifactReceipt.Preview` case whose `scores` band carries the applied intent/BPC/PCS/depth/codec/bands/embedded/bytes/grade-count plus the egress interpretation, the soft-proof out-of-gamut count, the separations spot count, and the peak Total-Area-Coverage ink limit under the closed `ManagedFact` vocabulary; the module-level `_grade` fold over the ordered chain (`cctf`/`broadcast`/`colourspace`/`correction`/`lut`) is the one core both worker arms reach, never run on the event loop.

## [02]-[MANAGED]

- Cases: `ManageOp` cases — `Managed(raster, src_profile, dst_profile, transform, codec, grade)` (the `uint8`/`uint16` raster and its `GradeStep` chain crossing the `to_process` subprocess seam where the worker normalizes to the unit range by the dtype maximum, folds the colour-science grade chain, and applies the pyvips `icc_transform` source/destination transform under the `IccTransform` bundle — the one native-`libvips` subprocess leg carrying the whole normalize+grade+ICC pass off the loop — with the destination profile embedded into the `ManagedCodec` output, the managed bytes keyed through `ContentIdentity.of` into the `ArtifactReceipt.Preview` content key with the ICC evidence on `scores`) · `Export(field, path, depth, grade)` (the field and its `GradeStep` chain crossing one `anyio.to_thread` slot where the worker folds the grade chain then runs the bit-depth-correct `colour.write_image` at the `BitDepth` policy for the blocking disk write — both the grade and the write off the loop, the worker returning the toned array so the written field keys through `ContentIdentity.of` with the `depth` folded into the content key so two depths of one field never collide, projecting the `ArtifactReceipt.Preview` content key with the egress evidence on `scores`, crossing no subprocess — removing the native-`libvips` process seam from non-ICC image egress) — matched by one total `match`/`case`. The `_grade` fold over the ordered `GradeStep` chain — `cctf` CCTF direction, `broadcast` OETF/EOTF/OOTF transfer, `colourspace` `RGB_to_RGB` primary-matrix conversion under a selected `AdaptMethod` transform, `correction` `apply_matrix_colour_correction` measured-CCM apply, `lut` `LUTSequence` — is the shared module-level fold both arms reach INSIDE their offload worker, never duplicated per arm and never run on the event loop.
- Auto: both arms fold the ordered `GradeStep` chain through the module-level `_grade` INSIDE their offload worker (never on the loop) via `Block.of_seq(grade).fold` — a `cctf` step applies `_TRANSFER[direction](acc, function=curve.value)` (the `colour.cctf_encoding`/`cctf_decoding` direction keyed by `Transfer`, the function name by `ToneCurve`), a `broadcast` step applies `_BROADCAST[kind](acc, function=curve.value)` (the `colour.oetf`/`eotf`/`ootf` direction keyed by `TransferKind`, the named function by `BroadcastCurve`), a `colourspace` step applies `colour.RGB_to_RGB(acc, RGB_COLOURSPACES[source], RGB_COLOURSPACES[target], chromatic_adaptation_transform=adapt)` (the 3x3 primary-matrix conversion keyed by `RgbSpace`, its chromatic-adaptation transform keyed by `AdaptMethod`), a `correction` step applies `colour.apply_matrix_colour_correction(acc, ccm)` (the pre-derived measured device-calibration `CCM` applied in-line, the matrix fitted upstream at `graphic/color/derive#DERIVE`), and a `lut` step applies `colour.LUTSequence(*(colour.read_LUT(path) for path in paths)).apply(acc)`, so decode-linearize, transfer, primary-convert, device-correct, LUT-grade, and re-encode interleave in one ordered chain; `Managed` crosses the `to_process.run_sync(_icc_apply, raster, ..., grade, limiter=_ICC_LANE)` seam where `_icc_apply` normalizes the `uint8`/`uint16` raster to the unit range by its dtype maximum (`raster / np.iinfo(raster.dtype).max`), folds it through `_grade`, lifts the toned array through `pyvips.Image.new_from_array`, resolves each `ProfileRef` through `named` (a `BuiltinProfile` device name passed straight, raw `bytes` written to a `NamedTemporaryFile(delete_on_close=False)` entered on one `ExitStack`), runs `image.icc_transform(dst, input_profile=src, intent=intent, black_point_compensation=bpc, pcs=pcs, depth=depth)`, reads back the embedded destination profile through `get_typeof("icc-profile-data")` and the egress `interpretation` (cmyk for a separations target, srgb otherwise), runs the optional Pillow `_softproof` when a `proof` profile is set (`buildProofTransform` under `Flags.SOFTPROOFING | Flags.GAMUTCHECK`, the plain-vs-gamut-check output diff counting out-of-press-gamut pixels — the lcms2 preflight signal pyvips lacks), reads the peak Total Area Coverage off the converted CMYK field for a separations egress (the 4 CMYK bands sum through the band-index `+` overload and `maxpos()[0]` reads the peak per-pixel ink, normalized to the depth ceiling as an `ink` % — the ISO 12647 / PDF-X-4 ink-limit preflight paired with `gamut`, `0.0` for a non-cmyk egress), and returns the `write_to_buffer(codec.value, keep=pyvips.ForeignKeep.ICC, **_CODEC_OPTS[codec])` managed bytes plus width/height/`bands`/embedded/space/gamut/spot-count/ink-TAC, which key through `ContentIdentity.of("color-managed", blob)` and project `ArtifactReceipt.Preview` with the `ManagedFact` ICC + soft-proof + separations evidence band; `Export` crosses the `to_thread.run_sync(_export_image, field, path, depth.value, grade, limiter=_EXPORT_LANE)` slot where `_export_image` folds the field through `_grade` then runs the bit-depth-correct `colour.write_image(toned, path, depth.value)` blocking disk write (`bit_depth` passed positionally) and returns the toned array, then keys the toned field bytes through `ContentIdentity.of(f"color-export-{depth.value}", toned.tobytes())` (the `depth` in the content key so two depths of one field never collide) and projects `ArtifactReceipt.Preview` with the field dimensions and the `depth`/`bytes`/`grade` evidence band. The grade chain runs once INSIDE each arm's offload worker — never on the event loop and never as a `toned` arg evaluated before the offload — the ICC apply riding the process band and the non-ICC write riding a thread slot.
- Growth: a new managed operation is one `ManageOp` case carrying its payload plus one acceptor arm contributing `ArtifactReceipt.Preview`, its `tag` extending the one `ManageOpTag` literal; a new grade step is one `GradeStep` case folded by `_grade` (an HDR tone-mapping operator, an explicit ASC-CDL slope/offset/power op beyond the `.clf` `lut` path); the soft-proof control (a `proof` press-profile field on the `IccTransform` bundle plus the `GAMUT` out-of-gamut-pixel count and `SPACE` egress-interpretation `ManagedFact` rows) and the separations declaration (a `separations` `SpotChannel`-tuple field `separations()` decodes from the CxF `ColorCmykplusN` device half plus the `SPOTS` count `ManagedFact` row) are exactly that in-place growth — one field on the kw-only `IccTransform` bundle plus one `ManagedFact` row, never a payload-arity change breaking every `Managed` call; a further ICC control (a device-link profile, an explicit N-color proof intent) lands the same way; a new output format is one `ManagedCodec` row plus one `_CODEC_OPTS` encoder-policy row; a new built-in device profile is one `BuiltinProfile` row keying `input_profile`/`output_profile` directly; a new CCTF function is one `ToneCurve` row keying `CCTF_ENCODINGS`/`CCTF_DECODINGS`; a new broadcast function is one `BroadcastCurve` row keying `OETFS`/`EOTFS`/`OOTFS`; a new primary space is one `RgbSpace` row keying `RGB_COLOURSPACES`; a new chromatic-adaptation transform is one `AdaptMethod` row keying `CHROMATIC_ADAPTATION_TRANSFORMS`; a new transfer direction is one `Transfer`/`TransferKind` row; a new rendering intent is one `RenderingIntent` row mirroring its pyvips `Intent` nickname; a new PCS is one `ConnectionSpace` row; a new output bit depth is one `BitDepth` row keying `colour.write_image` plus one `_DEPTH_BITS` libvips-clamp row; a new evidence scalar is one `ManagedFact` row plus one `_icc_apply` return slot — the `INK` Total-Area-Coverage preflight (the peak CMYK band-sum over the converted field) is exactly that in-place growth, one `ManagedFact.INK` row and one worker-computed slot beside `gamut`/`spots`, never a second preflight rail; never a parallel managed surface, never a second image writer; zero new surface.

```python signature
from collections.abc import Callable
from contextlib import ExitStack
from dataclasses import dataclass
from enum import StrEnum
from tempfile import NamedTemporaryFile
from typing import Literal, assert_never

import colour
import numpy as np
from anyio import CapacityLimiter, to_process, to_thread
from builtins import frozendict
from expression import case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct
from numpy.typing import NDArray

from rasm.runtime.content_identity import ContentIdentity
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.core.receipt import ArtifactReceipt

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
type ManageOpTag = Literal["managed", "export"]


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


class AdaptMethod(StrEnum):
    VON_KRIES = "Von Kries"
    BRADFORD = "Bradford"
    CAT02 = "CAT02"
    CAT16 = "CAT16"
    CMCCAT2000 = "CMCCAT2000"
    SHARP = "Sharp"


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


type ProfileRef = bytes | BuiltinProfile


_TRANSFER: frozendict[Transfer, Callable[..., Tristimulus]] = frozendict({
    Transfer.ENCODE: colour.cctf_encoding,
    Transfer.DECODE: colour.cctf_decoding,
})
_BROADCAST: frozendict[TransferKind, Callable[..., Tristimulus]] = frozendict({
    TransferKind.OETF: colour.oetf,
    TransferKind.EOTF: colour.eotf,
    TransferKind.OOTF: colour.ootf,
})
# libvips icc_transform emits 8/16-bit only, so FLOAT32 managed output clamps to 16; the Export arm keeps the unclamped colour.write_image depth.
_DEPTH_BITS: frozendict[BitDepth, int] = frozendict({BitDepth.UINT8: 8, BitDepth.UINT16: 16, BitDepth.FLOAT32: 16})
# per-codec libvips egress policy: lossless PNG/TIFF preserve managed fidelity, WEBP near-lossless, JPEG high-Q for an 8-bit web target.
_CODEC_OPTS: frozendict[ManagedCodec, frozendict[str, int | str | bool]] = frozendict({
    ManagedCodec.PNG: frozendict({"compression": 6}),
    ManagedCodec.TIFF: frozendict({"compression": "lzw"}),
    ManagedCodec.JPEG: frozendict({"Q": 92}),
    ManagedCodec.WEBP: frozendict({"Q": 92, "lossless": True}),
})
# the managed RenderingIntent nickname -> the Pillow ImageCms.Intent member name, resolved inside the soft-proof worker so no PIL symbol touches the parent loader path and the ICC intent codes stay named, never magic ints.
_PROOF_INTENT_NAME: frozendict[str, str] = frozendict({
    RenderingIntent.PERCEPTUAL.value: "PERCEPTUAL",
    RenderingIntent.RELATIVE.value: "RELATIVE_COLORIMETRIC",
    RenderingIntent.SATURATION.value: "SATURATION",
    RenderingIntent.ABSOLUTE.value: "ABSOLUTE_COLORIMETRIC",
    RenderingIntent.AUTO.value: "PERCEPTUAL",
})
_ICC_LANE: CapacityLimiter = CapacityLimiter(4)
_EXPORT_LANE: CapacityLimiter = CapacityLimiter(8)


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


class ColorManaged(Struct, frozen=True):
    op: ManageOp

    async def apply(self) -> RuntimeRail[ArtifactReceipt]:
        return await async_boundary(f"color.managed.{self.op.tag}", self._compute)

    async def _compute(self) -> ArtifactReceipt:
        match self.op:
            case ManageOp(tag="managed", managed=(raster, src_profile, dst_profile, transform, codec, grade)):
                # the whole normalize+grade+ICC pass AND the optional lcms2 soft-proof ride the to_process worker — the full-image
                # grade never runs on the loop nor as a `toned` arg evaluated before the offload; the raster + grade chain cross the seam.
                blob, width, height, bands, embedded, space, gamut, spots, ink = await to_process.run_sync(
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
                    limiter=_ICC_LANE,
                )
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
                return ArtifactReceipt.Preview(ContentIdentity.of("color-managed", blob), width, height, scores)
            case ManageOp(tag="export", export=(field, path, depth, grade)):
                # the grade fold AND the blocking write ride one to_thread slot off the loop; the worker returns the toned
                # array (same address space, no pickle) so the content key, dims, and byte volume read the one stored fact.
                toned = await to_thread.run_sync(_export_image, field, path, depth.value, grade, limiter=_EXPORT_LANE)
                height, width = toned.shape[0], toned.shape[1]
                scores = frozendict({
                    ManagedFact.DEPTH.value: depth.value,
                    ManagedFact.BYTES.value: float(toned.nbytes),
                    ManagedFact.GRADE.value: float(len(grade)),
                })
                return ArtifactReceipt.Preview(ContentIdentity.of(f"color-export-{depth.value}", toned.tobytes()), width, height, scores)
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

## [03]-[RESEARCH]

- [ICC_TRANSFORM] [RESOLVED]: the pyvips `Image.icc_transform`/`Intent` ICC-managed conversion surface is settled fence code against the documented pyvips signature `icc_transform(output_profile, *, pcs, intent, black_point_compensation, embedded, input_profile, depth)` — `output_profile` positional, `input_profile` the source characterization, `intent` the `RenderingIntent` nickname (`perceptual`/`relative`/`saturation`/`absolute`/`auto` mirroring the `Intent` enum), `black_point_compensation` the bool that prevents shadow-crush on a perceptual/relative conversion, `pcs` the `ConnectionSpace` profile-connection space (`lab` default / `xyz`), and `depth` the 8/16-bit output the codec carries (fed by the `_DEPTH_BITS` libvips-clamp map). Both `input_profile` and `output_profile` accept a `BuiltinProfile` device name (`srgb`/`p3`/`cmyk`) directly OR a profile filename, so `named` writes raw-`bytes` profiles to an `ExitStack`-entered `NamedTemporaryFile` and passes a `BuiltinProfile` straight. `Image.get_typeof("icc-profile-data")` reads back the destination profile `icc_transform` attaches, which libvips embeds into the `write_to_buffer` output under default metadata retention, so the managed bytes carry their profile and `ManagedFact.EMBEDDED` records it. The same `icc_transform` engine `graphic/raster/io#RASTER` already composes (`source.icc_transform("srgb", intent=Intent.RELATIVE)`), so ONLY the pillow `ImageCms.buildTransform`/`profileToProfile` device-egress engine is the deleted divergence — `buildProofTransform` SURVIVES as the DISTINCT soft-proof / gamut-warning role pyvips has no member for (the `_softproof` worker builds it under `Flags.SOFTPROOFING | Flags.GAMUTCHECK`, the plain-vs-gamut-check diff counting out-of-press-gamut pixels for PDF/X separations preflight), beside the `exchange/metadata#METADATA` profile-NAME read (`getProfileDescription`); the earlier note's blanket "second ICC engine deleted" was too broad and is corrected here. Close-condition: the folder `pyvips` `.api` `[03]-[ENTRYPOINTS]` row `[06]` enumerates the `pcs`/`black_point_compensation`/`depth`/`embedded` keywords its `...` tail currently elides (the surface is read from the lock-built sdist, native `libvips` unprovisioned, so the row carries the verified-upstream signature pending an `icc_transform` reflection pass on the provisioned worker). The `embedded=True` path — reading a profile off an already-encoded raster's metadata — is `graphic/raster/io#RASTER`'s `Convert` arm, NOT this owner's: a `new_from_array` numpy source carries no embedded profile, so this owner always passes an explicit `input_profile`.
- [ADAPTATION] [RESEARCH]: the `colourspace` `GradeStep` keys `RGB_to_RGB`'s `chromatic_adaptation_transform=` through the `AdaptMethod` vocabulary (`Von Kries`/`Bradford`/`CAT02`/`CAT16`/`CMCCAT2000`/`Sharp`, aligned member-for-member with `graphic/color/derive#DERIVE`'s `AdaptMethod` over the same registry), `Bradford` the ICC-standard default for a managed primary conversion. The `chromatic_adaptation_transform` kwarg is catalogue-confirmed on the `colour-science` RGB-space row `[01]`, but the `CHROMATIC_ADAPTATION_TRANSFORMS` registry its member strings key is not yet enumerated in the folder `.api` (which lists only the disjoint `CHROMATIC_ADAPTATION_METHODS` keying the standalone `chromatic_adaptation()` — the two share `Von Kries` while `Bradford`/`CAT02`/`CAT16`/`Sharp` exist only in the transforms registry, so binding `AdaptMethod` to the methods registry breaks the default Bradford path, the same seam `graphic/color/derive#DERIVE` tracks). Close-condition: `.api` catalogue enumerates the `CHROMATIC_ADAPTATION_TRANSFORMS` member strings.
- [CODEC] [RESOLVED]: the `ManagedCodec` egress vocabulary (`.png`/`.tif`/`.jpg`/`.webp`) keys `Image.write_to_buffer(format_string, **kwargs)` against the `pyvips` `.api` egress row `[02]` and `[04]-[IMPLEMENTATION_LAW]` egress axis, with each `_CODEC_OPTS` row holding the verified encoder kwargs (`compression` int for PNG, `compression` string for TIFF, `Q` for JPEG, `Q`+`lossless` for WEBP) the sibling `graphic/raster/io#RASTER` `_VIPS_KWARGS` already composes — replacing the prior hardcoded `write_to_buffer(".png")` that gave the color-managed egress one format and no encoder policy. A `ManagedCodec` whose bit-depth ceiling is below the `IccTransform.depth` (a 16-bit transform into an 8-bit JPEG/WEBP container) downconverts in libvips, the requested depth and codec both carried as `ManagedFact` evidence so the consumer reads the resolved pair. Close-condition: none — the `write_to_buffer` format-string and the `compression`/`Q`/`lossless` encoder kwargs are catalogue-confirmed.
- [SOFT_PROOF_SEPARATIONS] [RESOLVED]: the pillow `ImageCms.buildProofTransform(inputProfile, outputProfile, proofProfile, inMode, outMode, renderingIntent, proofRenderingIntent, flags)` + `Flags.SOFTPROOFING`/`Flags.GAMUTCHECK` + `applyTransform` + `Intent`/`createProfile` surface verifies against the cp315 pillow 12.2 wheel — the DISTINCT lcms2 soft-proof role pyvips has no member for, RE-ADMITTED after the prior blanket deletion. `_softproof` builds the proof transform twice (plain vs `GAMUTCHECK`), applies both to the source colours, and counts the differing pixels (the gamut-check alarm paints the out-of-press-gamut set) onto `ManagedFact.GAMUT`; the managed `RenderingIntent` nickname resolves to the `ImageCms.Intent` member by name through `_PROOF_INTENT_NAME` INSIDE the worker so no PIL symbol touches the parent loader path, and a `BuiltinProfile` device-name src falls back to `createProfile("sRGB")` as the soft-proof display reference. The CxF3 DEVICE half decodes through `colour_cxf.read_cxf` -> `cxf3.ColorCmykplusN.spot_color` (verified `SpotColor(name, percentage)` fields) into the `SpotChannel` declaration on `IccTransform.separations`, the count riding `ManagedFact.SPOTS` — managed owns the device half per `colour-cxf.md`, while the spectral/Lab COLOR half (the `sd_to_XYZ` spot-spectrum resolution) is `graphic/color/derive#DERIVE`'s Spot intake, decoded once at the source and never duplicated here. `pyvips.enums.ForeignKeep.ICC` on `write_to_buffer(keep=)` and `Image.interpretation` (`Interpretation.CMYK` for a CMYK/separations egress) are the catalogue-confirmed pyvips rows `[25]`/`[08]` (native `libvips` unprovisioned, so read from the verified catalogue tier like the `icc_transform` surface). The `ManagedFact.INK` Total-Area-Coverage preflight completes the separations-preflight pair: the same `_icc_apply` worker that reads `gamut` reads the peak per-pixel ink sum off the just-converted CMYK image through the band-index `managed[0] + managed[1] + managed[2] + managed[3]` sum (the `cmyk` egress guarantees the 4 bands, both the `image[i]` band-extract and the `+` overload the catalogue `[12]` band-algebra rows) then `maxpos()[0]` (catalogue `[13]`) for the peak, normalized to the depth ceiling as a percentage and gated on the `cmyk` egress interpretation so a non-separations output reports `0.0` — the ISO 12647 / PDF-X-4 total-ink-limit signal `gamut` alone does not carry, computed in-hand from the ICC-converted field rather than re-deriving it in a ghostscript/veraPDF oracle (those own the assembled-PDF preflight, this owns the CMYK-raster ink evidence). The stale `from artifacts.receipt.receipt import ArtifactReceipt` import is corrected to the canonical `from artifacts.core.receipt import ArtifactReceipt` every sibling producer uses. Close-condition: none — the soft-proof/separations/retention surfaces are fence-local and verified; the sibling `graphic/marks/encode#MARK` and `graphic/raster/io#RASTER` carry the SAME stale `receipt.receipt` import, corrected on their own pages.
