# [PY_ARTIFACTS_GRAPHIC_COLOR_MANAGED]

The ICC/LUT/CCTF color-managed raster-egress owner — the one native-libvips process-seam leg of the color sub-domain. `ColorManaged` is ONE owner folding the colour-science grade chain on the cp315 core then, for the `Managed` arm, crossing the runtime gated-band subprocess seam where pyvips `icc_transform` applies the source->destination ICC profile transform under the selected `RenderingIntent`. colour-science (NumFOCUS-affiliated, BSD-3, NumPy-backed, pure-Python, cp315-native) carries the `cctf_encoding`/`cctf_decoding`, `oetf`/`eotf`/`ootf`, `RGB_to_RGB`, and `read_LUT`->`LUTSequence.apply` grade chain plus the bit-depth-correct `write_image` egress entirely on the core; pyvips (`icc_transform`/`Intent`, the liblcms2-backed ICC engine the sibling `graphic/raster/io#RASTER` owns, binding a Forge-provisioned native `libvips` that is not on the cp315-core loader path) carries the genuinely-native ICC apply that cannot resolve in the cp315-core process, so the `Managed` arm dispatches it onto the `runtime/reliability/faults#FAULT`-owned `anyio.to_process.run_sync` gated-band subprocess seam through the module-level `_icc_apply` function bounded by the `anyio.CapacityLimiter` offload slot. `ManageOp` is ONE closed family carrying each operation's typed payload, never an erased `params` bag, dispatched by one total `match` returning `RuntimeRail[ArtifactReceipt]` — keying the color-managed bytes through `ContentIdentity.of` and contributing the settled `core/receipt#RECEIPT` `ArtifactReceipt.Preview` case, whose `scores` band carries the applied `RenderingIntent`, black-point-compensation flag, output depth/bands, byte volume, and grade-step count as the color-management evidence the pyvips image-op `evidence` axis names — the one receipt family every artifacts producer contributes to, never a parallel managed-receipt type. It consumes the grade chain and color-space provenance `graphic/color/derive#DERIVE` resolves and hands the color-managed raster to `graphic/raster/io#RASTER` and `document/emit#DOCUMENT`, so figure and PDF output is color-managed rather than naive sRGB. This page closes the `ICC_COLOR_MANAGED_OUTPUT` idea — the color-managed egress side of the visual pipeline.

## [01]-[INDEX]

- [01]-[MANAGED]: the one `ColorManaged` owner over the closed-payload `ManageOp` family — `Managed`/`Export` keying the color-managed bytes through `ContentIdentity.of` and contributing the settled `core/receipt#RECEIPT` `ArtifactReceipt.Preview(key, width, height, scores)` case with the `scores` evidence band, the `ManageOp` discriminant the closed `ManageOpTag` literal, never a bare `str`; the colour-science grade chain folded over the ordered `GradeStep` family on the cp315 core — a `cctf` CCTF direction (`cctf_encoding`/`cctf_decoding` keyed by `Transfer`+`ToneCurve`), a `broadcast` named scene/display/end-to-end transfer (`oetf`/`eotf`/`ootf` keyed by `TransferKind`+`BroadcastCurve`), a `colourspace` primary-matrix conversion (`RGB_to_RGB` over `RGB_COLOURSPACES` keyed by `RgbSpace`, its `chromatic_adaptation_transform` keyed by `Adaptation`), a `lut` ordered `LUTSequence` (the `.clf` path subsuming ASC-CDL ops) — then `colour.write_image` at the `BitDepth` policy the bit-depth-correct non-ICC egress; pyvips `icc_transform` the gated source->destination ICC apply under the `RenderingIntent` (a `StrEnum` mirroring the pyvips `Intent` nicknames, never a parallel intent table) with `black_point_compensation` and output `depth`, on the `runtime/reliability/faults#FAULT`-owned `to_process.run_sync` gated-band seam bounded by the module `CapacityLimiter`; the `Managed` arm is the one leg crossing the gated `libvips` subprocess seam while the `Export` arm needs no subprocess — its blocking `colour.write_image` disk write offloads to an `anyio.to_thread` slot bounded by `_EXPORT_LANE` — so `apply` is the uniform awaited consumer contract and only the `Managed` arm crosses a separate process.

## [02]-[MANAGED]

- Owner: `ColorManaged` the one ICC/LUT/CCTF color-managed raster-egress owner discriminating operation over the closed `ManageOp` family; `ManageOp` an `expression.tagged_union` whose every case carries its own typed payload, never a shared erased `params` dict; the owner contributes the settled `core/receipt#RECEIPT` `ArtifactReceipt.Preview` case keyed by `ContentIdentity.of` over the managed bytes (the `Managed` arm) or the written field (the `Export` arm), its `scores: frozendict[str, float | str]` band carrying the per-egress color-management evidence so the managed egress is a producer of the one `ArtifactReceipt` family every artifacts owner contributes to, never a parallel managed-receipt type the receipt owner would import; colour-science is the CCTF/broadcast-transfer/primary-matrix/LUT grade engine on the cp315 core, its `write_image` bit-depth egress offloaded to an `anyio.to_thread` slot for the blocking disk write; pyvips `icc_transform` the liblcms2-backed ICC raster apply on the gated band — the same `icc_transform` engine `graphic/raster/io#RASTER` owns, never a second pillow `ImageCms` transform; `RenderingIntent` the `StrEnum` whose member nickname mirrors a pyvips `Intent` constant and passes directly to `icc_transform`'s `intent=`, never a parallel intent table. The grade chain and color-space provenance this owner consumes is the `graphic/color/derive#DERIVE` `ColorReceipt.path` provenance the derivation owner produces — the caller threads it into the `GradeStep` chain and the source/destination profiles, this owner derives no color; the `cctf` step keys the same `CCTF_ENCODINGS`/`CCTF_DECODINGS` registries the derivation owner's spectral chain reads, threaded here as the transfer payload, never a second tone-curve vocabulary.
- Cases: `ManageOp` cases — `Managed(raster, src_profile, dst_profile, intent, bpc, depth, grade)` (the `uint8`/`uint16` raster normalized to the unit range by its dtype maximum, then the colour-science `GradeStep` chain folded on the core, then the pyvips `icc_transform` source/destination-profile transform under a `RenderingIntent` with `black_point_compensation` and output `depth` on the gated subprocess seam — the one native-libvips subprocess leg, the managed bytes keyed through `ContentIdentity.of` into the `ArtifactReceipt.Preview` content key with the ICC evidence on `scores`) · `Export(field, path, depth, grade)` (the `GradeStep` chain folded on the core then the bit-depth-correct `colour.write_image` at the `BitDepth` policy offloaded to an `anyio.to_thread` slot for the blocking disk write, the written field keyed through `ContentIdentity.of` with the `depth` folded into the content key so two depths of one field never collide, projecting the `ArtifactReceipt.Preview` content key with the egress evidence on `scores`, crossing no subprocess — removing the native-libvips process seam from non-ICC image egress) — matched by one total `match`/`case`. The `_grade` fold over the ordered `GradeStep` chain — `cctf` CCTF direction, `broadcast` OETF/EOTF/OOTF transfer, `colourspace` `RGB_to_RGB` primary-matrix conversion under a selected `Adaptation` transform, `lut` `LUTSequence` — is the shared core fold both arms reach, never duplicated per arm.
- Entry: `ColorManaged.apply` is `async` over the runtime `async_boundary` and dispatches the `ManageOp` case, returning one `RuntimeRail[ArtifactReceipt]` and contributing the settled `core/receipt#RECEIPT` `ArtifactReceipt.Preview(key, width, height, scores)` case — the key the `ContentIdentity.of` digest over the managed bytes or written field, the width/height the managed-raster dimensions, the `scores` band the applied intent, BPC, depth, output bands, byte volume, and grade-step count — all flat scalars the receipt owner reads through its own `_facts` arm with no producer value object imported, mirroring the settled `graphic/raster/io#RASTER` `Raster.of -> RuntimeRail[ArtifactReceipt]` producer shape so the managed egress lands in the one receipt family beside every other producer; the `Export` arm's grade resolves on the cp315 core and its blocking `colour.write_image` disk write offloads to an `anyio.to_thread.run_sync` slot bounded by `_EXPORT_LANE` (no subprocess), while the `Managed` arm's ICC apply crosses the runtime `reliability/faults#FAULT` `anyio.to_process.run_sync` gated-band subprocess seam — the one leg crossing a separate process, the genuine separate-process crossing the Forge-provisioned `libvips` needs because the cp315-core `execution/lanes#LANE` `to_interpreter.run_sync` subinterpreter offload shares the host interpreter version and cannot load the native `libvips` the pyvips `cffi` binding `dlopen`s, which is not on the cp315-core loader path. The `to_process.run_sync` seam, bounded by the module `CapacityLimiter` `_ICC_LANE` per the `anyio` offload law, is the lane the runtime `reliability/faults#FAULT` owner settled, so it is settled fence code here and folds inline, never a forwarding helper. The result is a settled rail the figure and document owners consume, never a re-derivation per output.
- Auto: both arms first fold the ordered `GradeStep` chain through `_grade` on the core via `Block.of_seq(grade).fold` — a `cctf` step applies `_TRANSFER[direction](field, function=curve.value)` (the `colour.cctf_encoding`/`cctf_decoding` direction keyed by `Transfer`, the function name by `ToneCurve`), a `broadcast` step applies `_BROADCAST[kind](field, function=curve.value)` (the `colour.oetf`/`eotf`/`ootf` direction keyed by `TransferKind`, the named function by `BroadcastCurve`), a `colourspace` step applies `colour.RGB_to_RGB(field, RGB_COLOURSPACES[source], RGB_COLOURSPACES[target], chromatic_adaptation_transform=adapt)` (the 3x3 primary-matrix conversion keyed by `RgbSpace`, its chromatic-adaptation transform keyed by `Adaptation`), and a `lut` step applies `colour.LUTSequence(*(colour.read_LUT(path) for path in paths)).apply(field)`, so decode-linearize, transfer, primary-convert, LUT-grade, and re-encode interleave in one ordered chain; `Managed` then normalizes the `uint8`/`uint16` raster to the unit range by its dtype maximum (`raster / np.iinfo(raster.dtype).max`), folds it through `_grade`, and crosses the `to_process.run_sync(_icc_apply, ..., limiter=_ICC_LANE)` gated seam where `_icc_apply` lifts the toned array through `pyvips.Image.new_from_array`, materializes the source and destination ICC profiles to temp files through `NamedTemporaryFile(delete_on_close=False)`, runs `image.icc_transform(target, input_profile=source, intent=intent, black_point_compensation=bpc, depth=depth)`, and returns the `write_to_buffer(".png")` managed bytes plus width/height/`bands`, which key through `ContentIdentity.of("color-managed", blob)` and project `ArtifactReceipt.Preview` with the `scores` ICC evidence band; `Export` folds the field through `_grade`, offloads the bit-depth-correct write through `to_thread.run_sync(colour.write_image, toned, path, depth.value, limiter=_EXPORT_LANE)` (the blocking disk write off the loop, `bit_depth` passed positionally), then keys the toned field bytes through `ContentIdentity.of(f"color-export-{depth.value}", ...)` (the `depth` in the content key so two depths of one field never collide) and projects `ArtifactReceipt.Preview` with the field dimensions and the `depth`/`bytes`/`grade` evidence band. The grade chain runs once on the core for both arms; the ICC apply rides the gated process band and the non-ICC write rides a thread slot.
- Packages: `colour-science` (`cctf_encoding`/`cctf_decoding` the generic gamma/log CCTF directions over the `CCTF_ENCODINGS`/`CCTF_DECODINGS` registries feeding `Transfer`+`ToneCurve`, `oetf`/`eotf`/`ootf` the named broadcast scene/display/end-to-end transfer trio over the `OETFS`/`EOTFS`/`OOTFS` registries feeding `TransferKind`+`BroadcastCurve`, `RGB_to_RGB` the direct named-RGB->RGB primary-matrix conversion over the `RGB_COLOURSPACES` registry feeding `RgbSpace` with its `chromatic_adaptation_transform` over the `CHROMATIC_ADAPTATION_TRANSFORMS` registry feeding `Adaptation`, `read_LUT` the `.cube`/`.clf`/`.csp` LUT IO, `LUTSequence`/`LUTSequence.apply` the ordered LUT pipeline, `write_image` the bit-depth-correct egress keyed by `BitDepth`) on the cp315 core; `pyvips` (`Image.new_from_array` the host-array seam, `Image.icc_transform(output_profile, input_profile=, intent=)` the liblcms2 ICC apply, the `Intent` rendering-intent enum, `Image.write_to_buffer` the encoded egress, `Image.width`/`height`/`bands` the output-geometry evidence) on the gated band, the same `libvips` ICC provider `graphic/raster/io#RASTER` owns; `numpy` (the dtype-derived `uint8`/`uint16`->unit-range normalization via `np.iinfo`, and the `float64` toned field); `anyio` (`to_process.run_sync` the gated-band subprocess seam for the ICC apply, `to_thread.run_sync` the thread slot for the blocking `Export` disk write, `CapacityLimiter` the bounded `_ICC_LANE`/`_EXPORT_LANE` offload slots); `frozendict` (the `_TRANSFER`/`_BROADCAST` transfer-callable tables, the `_DEPTH_BITS` libvips output-depth-clamp map, and the `Preview` `scores` evidence band); runtime (`faults.RuntimeRail`/`async_boundary` and the `faults`-owned `anyio.to_process.run_sync` subprocess-seam the gated ICC arm crosses — the genuine separate-process crossing distinct from the cp315-core `execution/lanes#LANE` `to_interpreter.run_sync` subinterpreter offload, both settled at their owners; `content_identity.ContentIdentity` the content key), `graphic/color/derive#DERIVE` (the `ColorReceipt.path` grade/space provenance the managed payload consumes), `core/receipt#RECEIPT` (`ArtifactReceipt.Preview` carrying the managed-raster content key plus the egress evidence band).
- Growth: a new managed operation is one `ManageOp` case carrying its payload plus one acceptor arm contributing `ArtifactReceipt.Preview`, its `tag` extending the one `ManageOpTag` literal; a new grade step is one `GradeStep` case folded by `_grade` (a tone-mapping operator, an ASC-CDL beyond the `.clf` `lut` path, a 3x3 measured colour-correction matrix via `colour.colour_correction`); a new CCTF function is one `ToneCurve` row keying `CCTF_ENCODINGS`/`CCTF_DECODINGS`; a new broadcast function is one `BroadcastCurve` row keying `OETFS`/`EOTFS`/`OOTFS`; a new primary space is one `RgbSpace` row keying `RGB_COLOURSPACES`; a new chromatic-adaptation transform is one `Adaptation` row keying `CHROMATIC_ADAPTATION_TRANSFORMS`; a new transfer direction is one `Transfer`/`TransferKind` row; a new rendering intent is one `RenderingIntent` row mirroring its pyvips `Intent` nickname; a new output bit depth is one `BitDepth` row keying `colour.write_image` plus one `_DEPTH_BITS` libvips-clamp row; a richer ICC leg (gamut-coverage audit, profile-completeness probe) lands as one carried payload field on the existing `Managed` arm plus one `scores` key — never a parallel managed surface, never a second image writer; zero new surface.
- Boundary: no color derivation (that stays at `graphic/color/derive#DERIVE` — this owner resolves no convert/difference/temperature/palette/gamut/cvd and imports neither the `ColorOp` family nor ColorAide); no chart/scene rendering (that stays at the visual owners); the managed arms key the color-managed raster or written field through `ContentIdentity.of` and contribute one `ArtifactReceipt.Preview` whose `scores` band carries the color-management evidence — a parallel managed-receipt type beside the one `ArtifactReceipt` family is the deleted form; the ICC engine is pyvips `icc_transform` — the same engine `graphic/raster/io#RASTER` owns — never a second pillow `ImageCms.buildTransform`/`applyTransform` transform beside it (pillow `ImageCms` survives only as the `exchange/metadata#METADATA` profile-NAME read `getProfileDescription`, a distinct descriptive-metadata concern, never an ICC transform here); bit-depth-correct image egress folds through `colour.write_image` on the core for the non-ICC `Export` arm and pyvips `write_to_buffer` inside the gated ICC `Managed` arm, never a second non-ICC image writer; the `cctf`/`broadcast`/`colourspace`/`LUT` grade math resolves pure-Python on the core, the non-ICC `Export` disk write offloads to an `anyio.to_thread` slot (blocking I/O off the loop, no subprocess), but the pyvips ICC apply binds the Forge-provisioned native `libvips` that is not on the cp315-core loader path, so it alone dispatches onto the `faults`-owned `to_process.run_sync` gated-band subprocess seam — a different process than the cp315-core `to_interpreter.run_sync` subinterpreter offload can host. The `colour.temperature`/CAM/convert/delta-E derivation surface and the ColorAide gamut/CVD/palette engine are NOT this page's — they are `graphic/color/derive#DERIVE`'s; this owner cites colour-science only for the grade/transfer/LUT/image-egress chain. Every colour-science spelling below is settled fence code; the pyvips `icc_transform` apply surface, the `Intent` nickname mirror, and the `to_process.run_sync` seam are settled by inheritance from their owners with the catalogue-deepen gaps tracked in [03]-[RESEARCH].

```python signature
from collections.abc import Callable
from enum import StrEnum
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


class Adaptation(StrEnum):
    BRADFORD = "Bradford"
    CAT02 = "CAT02"
    CAT16 = "CAT16"
    VON_KRIES = "Von Kries"
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


class BitDepth(StrEnum):
    UINT8 = "uint8"
    UINT16 = "uint16"
    FLOAT32 = "float32"


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
_ICC_LANE: CapacityLimiter = CapacityLimiter(4)
_EXPORT_LANE: CapacityLimiter = CapacityLimiter(8)


@tagged_union(frozen=True)
class GradeStep:
    tag: Literal["cctf", "broadcast", "colourspace", "lut"] = tag()
    cctf: tuple[Transfer, ToneCurve] = case()
    broadcast: tuple[TransferKind, BroadcastCurve] = case()
    colourspace: tuple[RgbSpace, RgbSpace, Adaptation] = case()
    lut: tuple[str, ...] = case()

    @staticmethod
    def Cctf(direction: Transfer, curve: ToneCurve = ToneCurve.SRGB) -> "GradeStep":
        return GradeStep(cctf=(direction, curve))

    @staticmethod
    def Broadcast(kind: TransferKind, curve: BroadcastCurve = BroadcastCurve.BT709) -> "GradeStep":
        return GradeStep(broadcast=(kind, curve))

    @staticmethod
    def Colourspace(source: RgbSpace, target: RgbSpace, adapt: Adaptation = Adaptation.BRADFORD) -> "GradeStep":
        return GradeStep(colourspace=(source, target, adapt))

    @staticmethod
    def Lut(*paths: str) -> "GradeStep":
        return GradeStep(lut=paths)


@tagged_union(frozen=True)
class ManageOp:
    tag: ManageOpTag = tag()
    managed: tuple[ManagedRaster, bytes, bytes, RenderingIntent, bool, BitDepth, tuple[GradeStep, ...]] = case()
    export: tuple[NDArray[np.float64], str, BitDepth, tuple[GradeStep, ...]] = case()

    @staticmethod
    def Managed(raster: ManagedRaster, src_profile: bytes, dst_profile: bytes, intent: RenderingIntent = RenderingIntent.RELATIVE, bpc: bool = True, depth: BitDepth = BitDepth.UINT8, grade: tuple[GradeStep, ...] = ()) -> "ManageOp":
        return ManageOp(managed=(raster, src_profile, dst_profile, intent, bpc, depth, grade))

    @staticmethod
    def Export(field: NDArray[np.float64], path: str, depth: BitDepth = BitDepth.UINT16, grade: tuple[GradeStep, ...] = ()) -> "ManageOp":
        return ManageOp(export=(field, path, depth, grade))


class ColorManaged(Struct, frozen=True):
    op: ManageOp

    async def apply(self) -> RuntimeRail[ArtifactReceipt]:
        return await async_boundary(f"color.managed.{self.op.tag}", self._compute)

    async def _compute(self) -> ArtifactReceipt:
        match self.op:
            case ManageOp(tag="managed", managed=(raster, src_profile, dst_profile, intent, bpc, depth, grade)):
                toned = self._grade(raster / np.float64(np.iinfo(raster.dtype).max), grade)
                blob, width, height, bands = await to_process.run_sync(
                    _icc_apply, toned, src_profile, dst_profile, intent.value, bpc, _DEPTH_BITS[depth], limiter=_ICC_LANE
                )
                scores: frozendict[str, float | str] = frozendict({
                    "intent": intent.value,
                    "bpc": float(bpc),
                    "depth": depth.value,
                    "bands": float(bands),
                    "bytes": float(len(blob)),
                    "grade": float(len(grade)),
                })
                return ArtifactReceipt.Preview(ContentIdentity.of("color-managed", blob), width, height, scores)
            case ManageOp(tag="export", export=(field, path, depth, grade)):
                toned = self._grade(field, grade)
                await to_thread.run_sync(colour.write_image, toned, path, depth.value, limiter=_EXPORT_LANE)
                height, width = toned.shape[0], toned.shape[1]
                scores = frozendict({"depth": depth.value, "bytes": float(toned.nbytes), "grade": float(len(grade))})
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
                case GradeStep(tag="lut", lut=paths):
                    return colour.LUTSequence(*(colour.read_LUT(path) for path in paths)).apply(acc)
                case _ as unreachable:
                    assert_never(unreachable)

        return Block.of_seq(steps).fold(applied, field)


def _icc_apply(toned: Tristimulus, src_profile: bytes, dst_profile: bytes, intent: str, bpc: bool, depth: int) -> tuple[bytes, int, int, int]:
    from tempfile import NamedTemporaryFile

    import pyvips

    image = pyvips.Image.new_from_array(toned)
    with (
        NamedTemporaryFile(suffix=".icc", delete_on_close=False) as source,
        NamedTemporaryFile(suffix=".icc", delete_on_close=False) as target,
    ):
        source.write(src_profile)
        target.write(dst_profile)
        source.close()
        target.close()
        managed = image.icc_transform(target.name, input_profile=source.name, intent=intent, black_point_compensation=bpc, depth=depth)
        return managed.write_to_buffer(".png"), managed.width, managed.height, managed.bands
```

## [03]-[RESEARCH]

- [ICC_TRANSFORM] [RESEARCH]: the pyvips `Image.icc_transform`/`Intent` ICC-managed conversion surface reflects in the folder `.api` catalogue for `pyvips` (`3.1.1`, surface read from the lock-built sdist over the Forge-provisioned `libvips`): the `[03]-[ENTRYPOINTS]` generated-operation row `[06]` `icc_transform(output_profile, *, input_profile=..., intent=Intent.RELATIVE, ...)`, the `[02]-[PUBLIC_TYPES]` `Intent` enum row `[03]` (`PERCEPTUAL`/`RELATIVE`/`SATURATION`/`ABSOLUTE`/`AUTO`), the `new_from_array` host-array construction row `[05]`, the `write_to_buffer` egress row `[02]`, and the `Image.width`/`height`/`bands` output-geometry properties the `[04]-[IMPLEMENTATION_LAW]` `evidence` axis names as image-receipt facts. The `_icc_apply` `icc_transform(target, input_profile=source, intent=intent)`/`write_to_buffer(".png")`/`width`/`height`/`bands` body is therefore settled fence code on the gated band, the same `icc_transform` engine `graphic/raster/io#RASTER` already composes (`source.icc_transform("srgb", intent=Intent.RELATIVE)`), so the pillow `ImageCms.buildTransform`/`applyTransform` second ICC engine is the deleted divergence — pillow `ImageCms` survives only as the `exchange/metadata#METADATA` profile-NAME read (`getProfileDescription`), a distinct concern, never an ICC transform here. The two open legs are the `black_point_compensation` and `depth` keyword arguments the catalogue row elides behind its `...` tail — load-bearing ICC controls (BPC prevents shadow-crush on a perceptual/relative conversion, `depth` sets the 8/16-bit output the PNG egress carries, fed by the `_DEPTH_BITS` libvips-clamp map) — so they stay a marked RESEARCH catalogue-deepen item until an `icc_transform` signature reflection pass lands on the provisioned `libvips` worker, and the `NamedTemporaryFile(delete_on_close=False)` profile materialization is the `system-apis.md` `[FILE_IO]` owner's spelling for the libvips profile-filename convention. Close-condition: `.api` catalogue carries the `icc_transform` `black_point_compensation`/`depth` keywords. The ICC apply is the one gated leg crossing the provisioned-`libvips` process seam.
- [GRADE_CHAIN] [RESEARCH]: the colour-science grade-and-egress chain verifies against the folder `.api` catalogue for `colour-science` (`0.4.7`, cp315-core pure-Python): `cctf_encoding`/`cctf_decoding` (the generic gamma/log CCTF directions over the `CCTF_ENCODINGS`/`CCTF_DECODINGS` registries keyed by `Transfer`+`ToneCurve`) at the `[03]` spectral-and-CCTF rows `[02]`/`[03]`; `oetf`/`eotf`/`ootf` (the named broadcast scene/display/end-to-end transfer trio over the `OETFS`/`EOTFS`/`OOTFS` registries keyed by `TransferKind`+`BroadcastCurve`) at the `[03]` transfer-function row `[03]`; `RGB_to_RGB(RGB, input_colourspace, output_colourspace, chromatic_adaptation_transform=...)` (the 3x3 primary-matrix conversion over the `RGB_COLOURSPACES` registry keyed by `RgbSpace`, the `chromatic_adaptation_transform` keyed by `Adaptation`) at the RGB-space row `[01]`; `read_LUT` the `.cube`/`.clf`/`.csp` LUT IO at row `[05]`, `LUTSequence`/`LUTSequence.apply` the ordered LUT pipeline at the `[02]` LUT-family row `[06]` (the `.clf` path subsuming ASC-CDL ops); and `write_image(image, path, bit_depth=...)` the bit-depth-correct egress keyed by `BitDepth` at row `[08]`. Every colour-science spelling resolves on the core. The broadcast `function=` strings (`ITU-R BT.709`/`ITU-R BT.1886`/`ITU-R BT.2100 PQ`) are catalogue-confirmed defaults on the transfer-function row, so `BroadcastCurve` is settled fence code. The two open legs are the registry-key member-string sets the catalogue confirms by axis but does not enumerate: the `CCTF_ENCODINGS`/`CCTF_DECODINGS` member strings the `ToneCurve` rows carry (`sRGB`/`Gamma 2.2`/`ST 2084`/`ITU-R BT.1886`/`ProPhoto RGB`) — a curve must key BOTH registries for the `Transfer.DECODE`->grade->`Transfer.ENCODE` round trip — and the `RGB_COLOURSPACES` member names the `RgbSpace` rows carry (`sRGB`/`Display P3`/`ITU-R BT.2020`/`ACEScg`/`ProPhoto RGB`); both vocabularies stay marked RESEARCH catalogue-deepen items until a `CCTF_ENCODINGS`/`RGB_COLOURSPACES` key reflection pass lands. Close-condition: `.api` catalogue enumerates the `CCTF_ENCODINGS`/`CCTF_DECODINGS` and `RGB_COLOURSPACES` member strings and carries the `LUTSequence.apply` method row (row `[06]` confirms the `LUTSequence` constructor but not its `apply`).
- [ADAPTATION] [RESEARCH]: the `colourspace` `GradeStep` keys `RGB_to_RGB`'s `chromatic_adaptation_transform=` through the `Adaptation` vocabulary (`Bradford`/`CAT02`/`CAT16`/`Von Kries`/`CMCCAT2000`/`Sharp`), `Bradford` the ICC-standard default for a managed primary conversion. The `chromatic_adaptation_transform` kwarg is catalogue-confirmed on the `colour-science` RGB-space row `[01]`, but the `CHROMATIC_ADAPTATION_TRANSFORMS` registry its member strings key is not yet enumerated in the folder `.api` (which lists only the disjoint `CHROMATIC_ADAPTATION_METHODS` keying the standalone `chromatic_adaptation()` — the two share `Von Kries` while `Bradford`/`CAT02`/`CAT16`/`Sharp` exist only in the transforms registry, so binding `Adaptation` to the methods registry breaks the default Bradford path, the same seam `graphic/color/derive#DERIVE` tracks). Close-condition: `.api` catalogue enumerates the `CHROMATIC_ADAPTATION_TRANSFORMS` member strings.
- [ICC_SEAM] [RESOLVED]: the `Managed` arm's process dispatch is spelled `anyio.to_process.run_sync(_icc_apply, ..., limiter=_ICC_LANE)`, the gated-band subprocess seam the runtime `reliability/faults#FAULT` owner settled and `graphic/raster/io#RASTER` composes — a subinterpreter shares the host interpreter version and cannot load the Forge-provisioned native `libvips` the pyvips `cffi` binding `dlopen`s, so the `Managed` arm crosses this genuine separate-process seam rather than the cp315-core `execution/lanes#LANE` `to_interpreter.run_sync` subinterpreter offload. The seam spelling is settled against the branch `anyio` `.api` catalogue `[03]` offload row `[06]` `to_process.run_sync(func, *args, cancellable=False, limiter=None)` and the `CapacityLimiter` type row `[02]` `[05]` — there is no catalogue gap, the `limiter=_ICC_LANE(4)` bound satisfies the `anyio` `[LOCAL_ADMISSION]` offload law (mirroring `composition/compose#COMPOSE`'s `_GATE`). The `_icc_apply` body is a module-level function dispatched by qualified name across the process seam (`to_process.run_sync` cannot target a bound method or closure), so it stays out of the `ColorManaged` owner deliberately, not as a stray helper; its return tuple is primitive (`bytes`, three `int`s) so the geometry evidence crosses the pickle hop without a value object.
