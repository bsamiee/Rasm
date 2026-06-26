# [PY_ARTIFACTS_GRAPHIC_COLOR_MANAGED]

The ICC/LUT/CCTF color-managed raster-egress owner — the one native-`libvips` process-seam leg of the color sub-domain. `ColorManaged` is ONE owner that folds the colour-science grade chain on the cp315 core then, for the `Managed` arm, crosses the runtime gated-band subprocess seam where pyvips `icc_transform` applies the source->destination ICC transform under one `IccTransform` policy bundle (`RenderingIntent` + black-point compensation + `ConnectionSpace` PCS + output `BitDepth`) and writes through one `ManagedCodec` egress that embeds the destination profile. colour-science (pure-Python, cp315-core) carries the `cctf_encoding`/`cctf_decoding`, `oetf`/`eotf`/`ootf`, `RGB_to_RGB`, `apply_matrix_colour_correction`, and `read_LUT`->`LUTSequence.apply` grade chain plus the bit-depth-correct `write_image` egress entirely on the core; pyvips (`icc_transform`/`Intent`, the `liblcms2`-backed ICC engine the sibling `graphic/raster/io#RASTER` owns, binding a Forge-provisioned native `libvips` that is not on the cp315-core loader path) carries the genuinely-native ICC apply that cannot resolve in the cp315-core process, so the `Managed` arm dispatches it onto the `runtime/reliability/faults#FAULT`-owned `anyio.to_process.run_sync` gated-band subprocess seam through the module-level `_icc_apply` function bounded by the `anyio.CapacityLimiter` offload slot. A profile crosses as a `ProfileRef` — raw `bytes` materialized to a temp file, or a `BuiltinProfile` device name passed straight to `liblcms2` — so a managed conversion to `srgb`/`p3`/`cmyk` needs no profile file. `ManageOp` is ONE closed family carrying each operation's typed payload, never an erased `params` bag, dispatched by one total `match` returning `RuntimeRail[ArtifactReceipt]` — keying the color-managed bytes through `ContentIdentity.of` and contributing the settled `core/receipt#RECEIPT` `ArtifactReceipt.Preview` case, whose `scores` band carries the applied intent, BPC, PCS, output depth/bands/codec, profile-embedded flag, byte volume, and grade-step count under the closed `ManagedFact` evidence vocabulary the pyvips image-op `evidence` axis names — the one receipt family every artifacts producer contributes to, never a parallel managed-receipt type. It consumes the grade chain and color-space provenance `graphic/color/derive#DERIVE` resolves and hands the color-managed raster to `graphic/raster/io#RASTER` and `document/emit#DOCUMENT`, so figure and PDF output is color-managed rather than naive sRGB. This page closes the `ICC_COLOR_MANAGED_OUTPUT` idea — the color-managed egress side of the visual pipeline.

## [01]-[INDEX]

- [01]-[MANAGED]: the one `ColorManaged` owner over the closed-payload `ManageOp` family — `Managed`/`Export` keying the color-managed bytes through `ContentIdentity.of` and contributing the settled `core/receipt#RECEIPT` `ArtifactReceipt.Preview(key, width, height, scores)` case with the `ManagedFact`-keyed `scores` evidence band (a `StrEnum` whose `.value` rows are the only key strings the `frozendict[str, float | str]` band carries, never a bare literal); the `ManageOp` discriminant the closed `ManageOpTag` literal, never a bare `str`; the colour-science grade chain folded over the ordered `GradeStep` family on the cp315 core — a `cctf` CCTF direction (`cctf_encoding`/`cctf_decoding` keyed by `Transfer`+`ToneCurve`), a `broadcast` named scene/display/end-to-end transfer (`oetf`/`eotf`/`ootf` keyed by `TransferKind`+`BroadcastCurve`), a `colourspace` primary-matrix conversion (`RGB_to_RGB` over `RGB_COLOURSPACES` keyed by `RgbSpace`, its `chromatic_adaptation_transform` keyed by `AdaptMethod`), a `correction` measured device-calibration matrix (`apply_matrix_colour_correction` applying a pre-derived `CCM`), a `lut` ordered `LUTSequence` (the `.clf` path subsuming ASC-CDL ops) — then `colour.write_image` at the `BitDepth` policy for the bit-depth-correct non-ICC egress; pyvips `icc_transform` the gated source->destination ICC apply under one `IccTransform` bundle (`RenderingIntent` mirroring the pyvips `Intent` nicknames, `bpc` black-point compensation, `ConnectionSpace` PCS, output `BitDepth`) writing through the `ManagedCodec` format with the destination profile embedded, on the `runtime/reliability/faults#FAULT`-owned `to_process.run_sync` gated-band seam bounded by the module `CapacityLimiter`; the `Managed` arm is the one leg crossing the gated `libvips` subprocess seam while the `Export` arm needs no subprocess — its blocking `colour.write_image` disk write offloads to an `anyio.to_thread` slot bounded by `_EXPORT_LANE` — so `apply` is the uniform awaited consumer contract and only the `Managed` arm crosses a separate process.

## [02]-[MANAGED]

- Owner: `ColorManaged` the one ICC/LUT/CCTF color-managed raster-egress owner discriminating operation over the closed `ManageOp` family; `ManageOp` an `expression.tagged_union` whose every case carries its own typed payload, never a shared erased `params` dict; the owner contributes the settled `core/receipt#RECEIPT` `ArtifactReceipt.Preview` case keyed by `ContentIdentity.of` over the managed bytes (the `Managed` arm) or the written field (the `Export` arm), its `scores: frozendict[str, float | str]` band keyed by the closed `ManagedFact` evidence vocabulary so the managed egress is a producer of the one `ArtifactReceipt` family every artifacts owner contributes to, never a parallel managed-receipt type the receipt owner would import; colour-science is the CCTF/broadcast-transfer/primary-matrix/LUT grade engine on the cp315 core, its `write_image` bit-depth egress offloaded to an `anyio.to_thread` slot for the blocking disk write; pyvips `icc_transform` the `liblcms2`-backed ICC raster apply on the gated band — the same `icc_transform` engine `graphic/raster/io#RASTER` owns, never a second pillow `ImageCms` transform. `IccTransform` is the one ICC-apply policy bundle (`intent`/`bpc`/`pcs`/`depth`) the `Managed` payload carries pre-constructed, so a new ICC control lands as one field on the bundle and every call site is untouched, never a flat knob-tuple the body re-pairs; `ManagedCodec` is the libvips-suffix egress vocabulary whose `_CODEC_OPTS` row carries each format's fidelity-favouring encoder policy, never a hardcoded `.png`; `ProfileRef = bytes | BuiltinProfile` is the source/destination profile carrier — raw `bytes` materialized to a temp file in the worker, or a `BuiltinProfile` device name (`srgb`/`p3`/`cmyk`) passed straight to `liblcms2` `input_profile`/`output_profile`. The grade chain and color-space provenance this owner consumes is the `graphic/color/derive#DERIVE` `ColorReceipt.path` provenance the derivation owner produces — the caller threads it into the `GradeStep` chain and the source/destination profiles, this owner derives no color; the `cctf` step keys the same `CCTF_ENCODINGS`/`CCTF_DECODINGS` registries the derivation owner's spectral chain reads, threaded here as the transfer payload, never a second tone-curve vocabulary.
- Cases: `ManageOp` cases — `Managed(raster, src_profile, dst_profile, transform, codec, grade)` (the `uint8`/`uint16` raster normalized to the unit range by its dtype maximum, then the colour-science `GradeStep` chain folded on the core, then the pyvips `icc_transform` source/destination transform under the `IccTransform` bundle on the gated subprocess seam — the one native-`libvips` subprocess leg — with the destination profile embedded into the `ManagedCodec` output, the managed bytes keyed through `ContentIdentity.of` into the `ArtifactReceipt.Preview` content key with the ICC evidence on `scores`) · `Export(field, path, depth, grade)` (the `GradeStep` chain folded on the core then the bit-depth-correct `colour.write_image` at the `BitDepth` policy offloaded to an `anyio.to_thread` slot for the blocking disk write, the written field keyed through `ContentIdentity.of` with the `depth` folded into the content key so two depths of one field never collide, projecting the `ArtifactReceipt.Preview` content key with the egress evidence on `scores`, crossing no subprocess — removing the native-`libvips` process seam from non-ICC image egress) — matched by one total `match`/`case`. The `_grade` fold over the ordered `GradeStep` chain — `cctf` CCTF direction, `broadcast` OETF/EOTF/OOTF transfer, `colourspace` `RGB_to_RGB` primary-matrix conversion under a selected `AdaptMethod` transform, `correction` `apply_matrix_colour_correction` measured-CCM apply, `lut` `LUTSequence` — is the shared core fold both arms reach, never duplicated per arm.
- Entry: `ColorManaged.apply` is `async` over the runtime `async_boundary` and dispatches the `ManageOp` case, returning one `RuntimeRail[ArtifactReceipt]` and contributing the settled `core/receipt#RECEIPT` `ArtifactReceipt.Preview(key, width, height, scores)` case — the key the `ContentIdentity.of` digest over the managed bytes or written field, the width/height the managed-raster dimensions, the `scores` band the `ManagedFact`-keyed applied intent, BPC, PCS, depth, output bands, codec, profile-embedded flag, byte volume, and grade-step count — all flat scalars the receipt owner reads through its own `_facts` arm with no producer value object imported, mirroring the settled `graphic/raster/io#RASTER` `Raster.of -> RuntimeRail[...]` producer shape so the managed egress lands in the one receipt family beside every other producer; the `Export` arm's grade resolves on the cp315 core and its blocking `colour.write_image` disk write offloads to an `anyio.to_thread.run_sync` slot bounded by `_EXPORT_LANE` (no subprocess), while the `Managed` arm's ICC apply crosses the runtime `reliability/faults#FAULT` `anyio.to_process.run_sync` gated-band subprocess seam — the one leg crossing a separate process, the genuine separate-process crossing the Forge-provisioned `libvips` needs because the cp315-core `execution/lanes#LANE` `to_interpreter.run_sync` subinterpreter offload shares the host interpreter version and cannot load the native `libvips` the pyvips `cffi` binding `dlopen`s, which is not on the cp315-core loader path. The `to_process.run_sync` seam, bounded by the module `CapacityLimiter` `_ICC_LANE` per the `anyio` offload law, is the lane the runtime `reliability/faults#FAULT` owner settled, so it is settled fence code here and folds inline, never a forwarding helper. The result is a settled rail the figure and document owners consume, never a re-derivation per output.
- Auto: both arms first fold the ordered `GradeStep` chain through `_grade` on the core via `Block.of_seq(grade).fold` — a `cctf` step applies `_TRANSFER[direction](field, function=curve.value)` (the `colour.cctf_encoding`/`cctf_decoding` direction keyed by `Transfer`, the function name by `ToneCurve`), a `broadcast` step applies `_BROADCAST[kind](field, function=curve.value)` (the `colour.oetf`/`eotf`/`ootf` direction keyed by `TransferKind`, the named function by `BroadcastCurve`), a `colourspace` step applies `colour.RGB_to_RGB(field, RGB_COLOURSPACES[source], RGB_COLOURSPACES[target], chromatic_adaptation_transform=adapt)` (the 3x3 primary-matrix conversion keyed by `RgbSpace`, its chromatic-adaptation transform keyed by `AdaptMethod`), a `correction` step applies `colour.apply_matrix_colour_correction(field, ccm)` (the pre-derived measured device-calibration `CCM` applied in-line, the matrix fitted upstream at `graphic/color/derive#DERIVE`), and a `lut` step applies `colour.LUTSequence(*(colour.read_LUT(path) for path in paths)).apply(field)`, so decode-linearize, transfer, primary-convert, device-correct, LUT-grade, and re-encode interleave in one ordered chain; `Managed` then normalizes the `uint8`/`uint16` raster to the unit range by its dtype maximum (`raster / np.iinfo(raster.dtype).max`), folds it through `_grade`, and crosses the `to_process.run_sync(_icc_apply, ..., limiter=_ICC_LANE)` gated seam where `_icc_apply` lifts the toned array through `pyvips.Image.new_from_array`, resolves each `ProfileRef` through `named` (a `BuiltinProfile` device name passed straight, raw `bytes` written to a `NamedTemporaryFile(delete_on_close=False)` entered on one `ExitStack`), runs `image.icc_transform(dst, input_profile=src, intent=intent, black_point_compensation=bpc, pcs=pcs, depth=depth)`, reads back the embedded destination profile through `get_typeof("icc-profile-data")`, and returns the `write_to_buffer(codec.value, **_CODEC_OPTS[codec])` managed bytes plus width/height/`bands`/embedded, which key through `ContentIdentity.of("color-managed", blob)` and project `ArtifactReceipt.Preview` with the `ManagedFact` ICC evidence band; `Export` folds the field through `_grade`, offloads the bit-depth-correct write through `to_thread.run_sync(colour.write_image, toned, path, depth.value, limiter=_EXPORT_LANE)` (the blocking disk write off the loop, `bit_depth` passed positionally), then keys the toned field bytes through `ContentIdentity.of(f"color-export-{depth.value}", ...)` (the `depth` in the content key so two depths of one field never collide) and projects `ArtifactReceipt.Preview` with the field dimensions and the `depth`/`bytes`/`grade` evidence band. The grade chain runs once on the core for both arms; the ICC apply rides the gated process band and the non-ICC write rides a thread slot.
- Packages: `colour-science` (`cctf_encoding`/`cctf_decoding` the generic gamma/log CCTF directions over the `CCTF_ENCODINGS`/`CCTF_DECODINGS` registries feeding `Transfer`+`ToneCurve`, `oetf`/`eotf`/`ootf` the named broadcast scene/display/end-to-end transfer trio over the `OETFS`/`EOTFS`/`OOTFS` registries feeding `TransferKind`+`BroadcastCurve`, `RGB_to_RGB` the direct named-RGB->RGB primary-matrix conversion over the `RGB_COLOURSPACES` registry feeding `RgbSpace` with its `chromatic_adaptation_transform` over the `CHROMATIC_ADAPTATION_TRANSFORMS` registry feeding `AdaptMethod`, `apply_matrix_colour_correction` the pre-derived measured-`CCM` device-calibration apply (the fit stays at `graphic/color/derive#DERIVE`), `read_LUT` the `.cube`/`.clf`/`.csp` LUT IO, `LUTSequence`/`LUTSequence.apply` the ordered LUT pipeline, `write_image` the bit-depth-correct egress keyed by `BitDepth`) on the cp315 core; `pyvips` (`Image.new_from_array` the host-array seam, `Image.icc_transform(output_profile, input_profile=, intent=, black_point_compensation=, pcs=, depth=)` the `liblcms2` ICC apply, the `Intent` rendering-intent enum mirrored by `RenderingIntent`, `Image.get_typeof("icc-profile-data")` the embedded-profile read-back, `Image.write_to_buffer(format_string, **opts)` the encoded egress keyed by `ManagedCodec`+`_CODEC_OPTS`, `Image.width`/`height`/`bands` the output-geometry evidence) on the gated band, the same `libvips` ICC provider `graphic/raster/io#RASTER` owns; `numpy` (the dtype-derived `uint8`/`uint16`->unit-range normalization via `np.iinfo`, and the `float64` toned field); `anyio` (`to_process.run_sync` the gated-band subprocess seam for the ICC apply, `to_thread.run_sync` the thread slot for the blocking `Export` disk write, `CapacityLimiter` the bounded `_ICC_LANE`/`_EXPORT_LANE` offload slots); `contextlib`/`tempfile` (`ExitStack` + `NamedTemporaryFile(delete_on_close=False)` the conditional profile-blob materialization the libvips filename convention forces, system-apis owned); `frozendict` (the `_TRANSFER`/`_BROADCAST` transfer-callable tables, the `_DEPTH_BITS` libvips output-depth-clamp map, the `_CODEC_OPTS` per-codec encoder policy, and the `Preview` `scores` evidence band); runtime (`faults.RuntimeRail`/`async_boundary` and the `faults`-owned `anyio.to_process.run_sync` subprocess-seam the gated ICC arm crosses — the genuine separate-process crossing distinct from the cp315-core `execution/lanes#LANE` `to_interpreter.run_sync` subinterpreter offload, both settled at their owners; `content_identity.ContentIdentity` the content key), `graphic/color/derive#DERIVE` (the `ColorReceipt.path` grade/space provenance the managed payload consumes), `core/receipt#RECEIPT` (`ArtifactReceipt.Preview` carrying the managed-raster content key plus the egress evidence band).
- Growth: a new managed operation is one `ManageOp` case carrying its payload plus one acceptor arm contributing `ArtifactReceipt.Preview`, its `tag` extending the one `ManageOpTag` literal; a new grade step is one `GradeStep` case folded by `_grade` (an HDR tone-mapping operator, an explicit ASC-CDL slope/offset/power op beyond the `.clf` `lut` path); a new ICC control (gamut-warning flag, soft-proof profile) is one field on the existing `IccTransform` bundle plus one `ManagedFact` row, never a payload-arity change breaking every `Managed` call; a new output format is one `ManagedCodec` row plus one `_CODEC_OPTS` encoder-policy row; a new built-in device profile is one `BuiltinProfile` row keying `input_profile`/`output_profile` directly; a new CCTF function is one `ToneCurve` row keying `CCTF_ENCODINGS`/`CCTF_DECODINGS`; a new broadcast function is one `BroadcastCurve` row keying `OETFS`/`EOTFS`/`OOTFS`; a new primary space is one `RgbSpace` row keying `RGB_COLOURSPACES`; a new chromatic-adaptation transform is one `AdaptMethod` row keying `CHROMATIC_ADAPTATION_TRANSFORMS`; a new transfer direction is one `Transfer`/`TransferKind` row; a new rendering intent is one `RenderingIntent` row mirroring its pyvips `Intent` nickname; a new PCS is one `ConnectionSpace` row; a new output bit depth is one `BitDepth` row keying `colour.write_image` plus one `_DEPTH_BITS` libvips-clamp row; a new evidence scalar is one `ManagedFact` row — never a parallel managed surface, never a second image writer; zero new surface.
- Boundary: no color derivation (that stays at `graphic/color/derive#DERIVE` — this owner resolves no convert/difference/temperature/palette/gamut/cvd and imports neither the `ColorOp` family nor ColorAide); it never FITS a colour-correction matrix — the `colour.colour_correction` measured-vs-reference solve is derive's `Correct` arm, while this owner only APPLIES a pre-derived `CCM` as a `correction` grade step through `apply_matrix_colour_correction`, so calibration fit and egress apply stay split across the two owners; no chart/scene rendering (that stays at the visual owners); the managed arms key the color-managed raster or written field through `ContentIdentity.of` and contribute one `ArtifactReceipt.Preview` whose `ManagedFact`-keyed `scores` band carries the color-management evidence — a parallel managed-receipt type beside the one `ArtifactReceipt` family is the deleted form; the ICC engine is pyvips `icc_transform` — the same engine `graphic/raster/io#RASTER` owns — never a second pillow `ImageCms.buildTransform`/`applyTransform` transform beside it (pillow `ImageCms` survives only as the `exchange/metadata#METADATA` profile-NAME read `getProfileDescription`, a distinct descriptive-metadata concern, never an ICC transform here); the EMBEDDED-source-profile path — reading a profile off an already-encoded raster's `icc-profile-data` and `icc_transform`ing it to display — is `graphic/raster/io#RASTER`'s `Convert` arm (`get_typeof("icc-profile-data")` then `icc_transform("srgb")` over a streamed `new_from_buffer` pipeline), so this owner takes the EXPLICIT-profile path (a graded `numpy` field plus a `ProfileRef` source/destination) and never re-implements the embedded-profile convert; bit-depth-correct image egress folds through `colour.write_image` on the core for the non-ICC `Export` arm and pyvips `write_to_buffer` inside the gated ICC `Managed` arm, never a second non-ICC image writer; the `cctf`/`broadcast`/`colourspace`/`LUT` grade math resolves pure-Python on the core, the non-ICC `Export` disk write offloads to an `anyio.to_thread` slot (blocking I/O off the loop, no subprocess), but the pyvips ICC apply binds the Forge-provisioned native `libvips` that is not on the cp315-core loader path, so it alone dispatches onto the `faults`-owned `to_process.run_sync` gated-band subprocess seam — a different process than the cp315-core `to_interpreter.run_sync` subinterpreter offload can host. The `colour.temperature`/CAM/convert/delta-E derivation surface and the ColorAide gamut/CVD/palette engine are NOT this page's — they are `graphic/color/derive#DERIVE`'s; this owner cites colour-science only for the grade/transfer/LUT/CCM-apply/image-egress chain.

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

from artifacts.receipt.receipt import ArtifactReceipt

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
    def Correction(ccm: NDArray[np.float64], /) -> "GradeStep":  # the device-calibration CCM is fitted upstream at derive#DERIVE; this step only applies it
        return GradeStep(correction=ccm)

    @staticmethod
    def Lut(*paths: str) -> "GradeStep":
        return GradeStep(lut=paths)


@dataclass(frozen=True, slots=True, kw_only=True)
class IccTransform:
    intent: RenderingIntent = RenderingIntent.RELATIVE
    bpc: bool = True
    pcs: ConnectionSpace = ConnectionSpace.LAB
    depth: BitDepth = BitDepth.UINT8


_ICC_DEFAULT: IccTransform = IccTransform()


@tagged_union(frozen=True)
class ManageOp:
    tag: ManageOpTag = tag()
    managed: tuple[ManagedRaster, ProfileRef, ProfileRef, IccTransform, ManagedCodec, tuple[GradeStep, ...]] = case()
    export: tuple[NDArray[np.float64], str, BitDepth, tuple[GradeStep, ...]] = case()

    @staticmethod
    def Managed(raster: ManagedRaster, src_profile: ProfileRef, dst_profile: ProfileRef, transform: IccTransform = _ICC_DEFAULT, codec: ManagedCodec = ManagedCodec.PNG, grade: tuple[GradeStep, ...] = ()) -> "ManageOp":
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
                toned = self._grade(raster / np.float64(np.iinfo(raster.dtype).max), grade)
                blob, width, height, bands, embedded = await to_process.run_sync(
                    _icc_apply, toned, src_profile, dst_profile, transform.intent.value, transform.bpc, transform.pcs.value, _DEPTH_BITS[transform.depth], codec, limiter=_ICC_LANE
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
                })
                return ArtifactReceipt.Preview(ContentIdentity.of("color-managed", blob), width, height, scores)
            case ManageOp(tag="export", export=(field, path, depth, grade)):
                toned = self._grade(field, grade)
                await to_thread.run_sync(colour.write_image, toned, path, depth.value, limiter=_EXPORT_LANE)
                height, width = toned.shape[0], toned.shape[1]
                scores = frozendict({
                    ManagedFact.DEPTH.value: depth.value,
                    ManagedFact.BYTES.value: float(toned.nbytes),
                    ManagedFact.GRADE.value: float(len(grade)),
                })
                return ArtifactReceipt.Preview(ContentIdentity.of(f"color-export-{depth.value}", toned.tobytes()), width, height, scores)
            case _:
                assert_never(self.op)

    @staticmethod
    def _grade(field: Tristimulus, steps: tuple[GradeStep, ...]) -> Tristimulus:
        def applied(acc: Tristimulus, step: GradeStep) -> Tristimulus:
            match step:
                case GradeStep(tag="cctf", cctf=(direction, curve)):
                    return _TRANSFER[direction](acc, function=curve.value)
                case GradeStep(tag="broadcast", broadcast=(kind, curve)):
                    return _BROADCAST[kind](acc, function=curve.value)
                case GradeStep(tag="colourspace", colourspace=(source, target, adapt)):
                    return colour.RGB_to_RGB(acc, colour.RGB_COLOURSPACES[source.value], colour.RGB_COLOURSPACES[target.value], chromatic_adaptation_transform=adapt.value)
                case GradeStep(tag="correction", correction=ccm):
                    return colour.apply_matrix_colour_correction(acc, ccm)
                case GradeStep(tag="lut", lut=paths):
                    return colour.LUTSequence(*(colour.read_LUT(path) for path in paths)).apply(acc)
                case _ as unreachable:
                    assert_never(unreachable)

        return Block.of_seq(steps).fold(applied, field)


def _icc_apply(toned: Tristimulus, src: ProfileRef, dst: ProfileRef, intent: str, bpc: bool, pcs: str, depth: int, codec: ManagedCodec) -> tuple[bytes, int, int, int, bool]:
    import pyvips  # native libvips gate kept worker-local: the cp315-core parent carries no libvips on its loader path, so a module-scope import would break parent import

    def named(stack: ExitStack, profile: ProfileRef, /) -> str:
        if isinstance(profile, str):  # BuiltinProfile is a StrEnum, so a device name passes straight to liblcms2
            return profile
        handle = stack.enter_context(NamedTemporaryFile(suffix=".icc", delete_on_close=False))
        handle.write(profile)
        handle.close()
        return handle.name

    image = pyvips.Image.new_from_array(toned)
    with ExitStack() as stack:  # the profile temp files must outlive the lazy icc_transform until write_to_buffer pulls pixels
        managed = image.icc_transform(named(stack, dst), input_profile=named(stack, src), intent=intent, black_point_compensation=bpc, pcs=pcs, depth=depth)
        return (
            managed.write_to_buffer(codec.value, **_CODEC_OPTS[codec]),
            managed.width,
            managed.height,
            managed.bands,
            managed.get_typeof("icc-profile-data") != 0,
        )
```

## [03]-[RESEARCH]

- [ICC_TRANSFORM] [RESOLVED]: the pyvips `Image.icc_transform`/`Intent` ICC-managed conversion surface is settled fence code against the documented pyvips signature `icc_transform(output_profile, *, pcs, intent, black_point_compensation, embedded, input_profile, depth)` — `output_profile` positional, `input_profile` the source characterization, `intent` the `RenderingIntent` nickname (`perceptual`/`relative`/`saturation`/`absolute`/`auto` mirroring the `Intent` enum), `black_point_compensation` the bool that prevents shadow-crush on a perceptual/relative conversion, `pcs` the `ConnectionSpace` profile-connection space (`lab` default / `xyz`), and `depth` the 8/16-bit output the codec carries (fed by the `_DEPTH_BITS` libvips-clamp map). Both `input_profile` and `output_profile` accept a `BuiltinProfile` device name (`srgb`/`p3`/`cmyk`) directly OR a profile filename, so `named` writes raw-`bytes` profiles to an `ExitStack`-entered `NamedTemporaryFile` and passes a `BuiltinProfile` straight. `Image.get_typeof("icc-profile-data")` reads back the destination profile `icc_transform` attaches, which libvips embeds into the `write_to_buffer` output under default metadata retention, so the managed bytes carry their profile and `ManagedFact.EMBEDDED` records it. The same `icc_transform` engine `graphic/raster/io#RASTER` already composes (`source.icc_transform("srgb", intent=Intent.RELATIVE)`), so the pillow `ImageCms.buildTransform`/`applyTransform` second ICC engine is the deleted divergence — pillow `ImageCms` survives only as the `exchange/metadata#METADATA` profile-NAME read (`getProfileDescription`). Close-condition: the folder `pyvips` `.api` `[03]-[ENTRYPOINTS]` row `[06]` enumerates the `pcs`/`black_point_compensation`/`depth`/`embedded` keywords its `...` tail currently elides (the surface is read from the lock-built sdist, native `libvips` unprovisioned, so the row carries the verified-upstream signature pending an `icc_transform` reflection pass on the provisioned worker). The `embedded=True` path — reading a profile off an already-encoded raster's metadata — is `graphic/raster/io#RASTER`'s `Convert` arm, NOT this owner's: a `new_from_array` numpy source carries no embedded profile, so this owner always passes an explicit `input_profile`.
- [GRADE_CHAIN] [RESEARCH]: the colour-science grade-and-egress chain verifies against the folder `.api` catalogue for `colour-science` (`0.4.7`, cp315-core pure-Python): `cctf_encoding`/`cctf_decoding` (the generic gamma/log CCTF directions over the `CCTF_ENCODINGS`/`CCTF_DECODINGS` registries keyed by `Transfer`+`ToneCurve`) at the `[03]` spectral-and-CCTF rows `[02]`/`[03]`; `oetf`/`eotf`/`ootf` (the named broadcast scene/display/end-to-end transfer trio over the `OETFS`/`EOTFS`/`OOTFS` registries keyed by `TransferKind`+`BroadcastCurve`) at the `[03]` transfer-function row `[03]`; `RGB_to_RGB(RGB, input_colourspace, output_colourspace, chromatic_adaptation_transform=...)` (the 3x3 primary-matrix conversion over the `RGB_COLOURSPACES` registry keyed by `RgbSpace`, the `chromatic_adaptation_transform` keyed by `AdaptMethod`) at the RGB-space row `[01]`; `apply_matrix_colour_correction(RGB, CCM)` the pre-derived measured-CCM device-calibration apply at the colour-correction row `[04]` (settled fence code — the matrix is fitted upstream at `graphic/color/derive#DERIVE`'s `Correct` arm via `colour.colour_correction`, applied here as the `correction` grade step, never re-fitted); `read_LUT` the `.cube`/`.clf`/`.csp` LUT IO at row `[05]`, `LUTSequence`/`LUTSequence.apply` the ordered LUT pipeline at the `[02]` LUT-family row `[06]` (the `.clf` path subsuming ASC-CDL ops); and `write_image(image, path, bit_depth=...)` the bit-depth-correct egress keyed by `BitDepth` at row `[08]`. The broadcast `function=` strings (`ITU-R BT.709`/`ITU-R BT.1886`/`ITU-R BT.2100 PQ`) are catalogue-confirmed defaults on the transfer-function row, so `BroadcastCurve` is settled fence code. The two open legs are the registry-key member-string sets the catalogue confirms by axis but does not enumerate: the `CCTF_ENCODINGS`/`CCTF_DECODINGS` member strings the `ToneCurve` rows carry (`sRGB`/`Gamma 2.2`/`ST 2084`/`ITU-R BT.1886`/`ProPhoto RGB`) — a curve must key BOTH registries for the `Transfer.DECODE`->grade->`Transfer.ENCODE` round trip — and the `RGB_COLOURSPACES` member names the `RgbSpace` rows carry (`sRGB`/`Display P3`/`ITU-R BT.2020`/`ACEScg`/`ProPhoto RGB`); both stay catalogue-deepen items until a `CCTF_ENCODINGS`/`RGB_COLOURSPACES` key reflection pass lands. Close-condition: `.api` catalogue enumerates the `CCTF_ENCODINGS`/`CCTF_DECODINGS` and `RGB_COLOURSPACES` member strings and carries the `LUTSequence.apply` method row.
- [ADAPTATION] [RESEARCH]: the `colourspace` `GradeStep` keys `RGB_to_RGB`'s `chromatic_adaptation_transform=` through the `AdaptMethod` vocabulary (`Von Kries`/`Bradford`/`CAT02`/`CAT16`/`CMCCAT2000`/`Sharp`, aligned member-for-member with `graphic/color/derive#DERIVE`'s `AdaptMethod` over the same registry), `Bradford` the ICC-standard default for a managed primary conversion. The `chromatic_adaptation_transform` kwarg is catalogue-confirmed on the `colour-science` RGB-space row `[01]`, but the `CHROMATIC_ADAPTATION_TRANSFORMS` registry its member strings key is not yet enumerated in the folder `.api` (which lists only the disjoint `CHROMATIC_ADAPTATION_METHODS` keying the standalone `chromatic_adaptation()` — the two share `Von Kries` while `Bradford`/`CAT02`/`CAT16`/`Sharp` exist only in the transforms registry, so binding `AdaptMethod` to the methods registry breaks the default Bradford path, the same seam `graphic/color/derive#DERIVE` tracks). Close-condition: `.api` catalogue enumerates the `CHROMATIC_ADAPTATION_TRANSFORMS` member strings.
- [CODEC] [RESOLVED]: the `ManagedCodec` egress vocabulary (`.png`/`.tif`/`.jpg`/`.webp`) keys `Image.write_to_buffer(format_string, **kwargs)` against the `pyvips` `.api` egress row `[02]` and `[04]-[IMPLEMENTATION_LAW]` egress axis, with each `_CODEC_OPTS` row holding the verified encoder kwargs (`compression` int for PNG, `compression` string for TIFF, `Q` for JPEG, `Q`+`lossless` for WEBP) the sibling `graphic/raster/io#RASTER` `_VIPS_KWARGS` already composes — replacing the prior hardcoded `write_to_buffer(".png")` that gave the color-managed egress one format and no encoder policy. A `ManagedCodec` whose bit-depth ceiling is below the `IccTransform.depth` (a 16-bit transform into an 8-bit JPEG/WEBP container) downconverts in libvips, the requested depth and codec both carried as `ManagedFact` evidence so the consumer reads the resolved pair. Close-condition: none — the `write_to_buffer` format-string and the `compression`/`Q`/`lossless` encoder kwargs are catalogue-confirmed.
- [ICC_SEAM] [RESOLVED]: the `Managed` arm's process dispatch is spelled `anyio.to_process.run_sync(_icc_apply, ..., limiter=_ICC_LANE)`, the gated-band subprocess seam the runtime `reliability/faults#FAULT` owner settled and `graphic/raster/io#RASTER` composes — a subinterpreter shares the host interpreter version and cannot load the Forge-provisioned native `libvips` the pyvips `cffi` binding `dlopen`s, so the `Managed` arm crosses this genuine separate-process seam rather than the cp315-core `execution/lanes#LANE` `to_interpreter.run_sync` subinterpreter offload. The seam spelling is settled against the branch `anyio` `.api` catalogue `[03]` offload row `[06]` `to_process.run_sync(func, *args, cancellable=False, limiter=None)` and the `CapacityLimiter` type row `[02]` `[05]` — the `limiter=_ICC_LANE(4)` bound satisfies the `anyio` `[LOCAL_ADMISSION]` offload law. The `_icc_apply` body is a module-level function dispatched by qualified name across the process seam (`to_process.run_sync` cannot target a bound method or closure), so it stays out of the `ColorManaged` owner deliberately, not as a stray helper; its arguments are picklable (`np.ndarray`, `bytes`/`BuiltinProfile`, `str`, `bool`, `int`, `ManagedCodec`) and its return tuple is primitive (`bytes`, three `int`s, `bool`) so the geometry-and-embedding evidence crosses the pickle hop without a value object, the worker reading `_CODEC_OPTS` off the re-imported module rather than a forwarded table.
