# [PY_ARTIFACTS_GRAPHIC_COLOR_MANAGED]

Color-managed raster egress and the OCIO config plane — the downstream half of the color sub-domain, where `graphic/color/derive#DERIVE` is the upstream colorimetric source. `ColorManaged` is one behavior-dense frozen owner over the closed `ManageOp` family, and three transform engines answer three different questions on it: `PyOpenColorIO` resolves what a project's colorspaces MEAN and moves a scene-referred field between them, `pyvips` runs the device-to-device ICC egress at 8 and 16 bits under one `IccTransform` policy bundle, and `imagecodecs`'s lcms2 arms carry the float ICC edge pyvips's integer pipeline cannot reach. `ConfigSource` is the closed four-case config vocabulary every OCIO leg threads and the behavior-dense owner of the config graph — one memoized `Config` per source per worker, the processor acquired off it, the file-rules classification read from it — so no leg reads `GetCurrentConfig` and nothing below the composition root writes `SetCurrentConfig`. Spaces name `OcioRole` members wherever a role exists, so a config swap moves the meaning and no page re-spells a colorspace string.

Every produced blob, field, plate, and LUT lands at an `OutPath`. `Managed` writes the ICC-converted container through the `graphic/raster/io#IO` `CODEC` row's own libvips writer column, so one codec table serves the raster funnel and this egress and neither restates a suffix or an option builder; `Plane` closes the float ICC hole through `imagecodecs.cms_transform` with `cms_profile_validate` gating an untrusted blob before it reaches liblcms2; `Space` and `View` are the config-resolved colorspace and display-referred moves; `Export` folds the colour-science grade chain and writes at a `BitDepth`; `Plate`, `Lut`, `Swatch`, and `Separate` author separations, bake LUTs through OCIO's `Baker` or colour-science's `LUTSequence`, graduate CxF3 device declarations, and measure a finished PDF's per-ink coverage. Every arm crosses `self.lane.offload` as one runtime `Kernel` whose trait alone derives isolation and worker-death retry — a path-writing arm declares `idempotent=False`, so no retry replays an externally visible write — and every worker returns its native value on `RuntimeResult`; the pre-run key mints synchronously through the bare `ContentIdentity.key` over `_canon`'s length-framed per-arm preimage.

## [01]-[INDEX]

- [02]-[MANAGED]: `ColorManaged` owns color-managed egress and the OCIO config plane over the closed `ManageOp` family; `ConfigSource` carries the memoized config graph, processor acquisition, and file-rules classification, and every arm crosses `self.lane.offload` onto one `RuntimeResult`.

## [02]-[MANAGED]

- Cases: `ManageOp` cases — `Managed(raster, path, src, dst, transform, codec, grade)` the `uint8`/`uint16` raster and its `GradeStep` chain crossing the `HOSTILE` process boundary where the worker normalizes by dtype maximum, folds the chain, applies pyvips `icc_transform` under the `IccTransform` bundle, and writes the destination-profile-embedded container through the `raster/io#IO` `CODEC` libvips writer column — the one native-`libvips` process leg, its bare raster riding `Wire.SHARED_MEMORY`; `Plane(field, path, src, dst, transform, alpha)` the float ICC leg over `imagecodecs.cms_transform(outdtype=np.float32)` on a `RELEASING` thread, the depth pyvips refuses, its `AlphaBand` declaring which trailing band carries THROUGH the three-component transform instead of across it; `Space(field, path, config, src, dst, look)` and `View(field, path, config, src, display, view)` the two config-resolved moves — scene-referred colorspace-to-colorspace with an optional look, and display-referred through `DisplayViewTransform` — both `RELEASING`; `Export(field, path, depth, grade)` the grade fold and the bit-depth-correct `colour.write_image`; `Plate(document, path, channels, transform)` authoring one `/Separation` colorspace per spot and the joint `/DeviceN` over the pikepdf raw object model; `Lut(bake, path, size, shaper, intent)` baking the closed `LutBake` axis — a `GradeStep` chain through `colour.LUTSequence`, or a config-resolved space or display-view chain through `ocio.Baker`, which writes the CLF and CTF containers `colour.write_LUT` carries no method for; `Swatch(document)` graduating the CxF3 device half; `Separate(document, page, dpi, plates)` rendering the finished PDF's per-ink coverage plates through `pdf_oxide`, minting the MEASURED peak TAC and per-ink coverages and landing each plate as a 16-bit image when the egress is declared — matched by one total `match`. `_grade` folds the ordered chain (`cctf`/`broadcast`/`colourspace`/`correction`/`lut`/`managed`) — the shared module-level core every field arm reaches inside its offload worker, never duplicated per arm nor run on the loop.
- Auto: `ConfigSource` owns the config graph whole. `_config` memoizes one `Config` per source per worker process under `functools.cache`, so OCIO's own per-`Config` processor cache makes every later acquisition a hash lookup and the catalog's acquire-outside-the-fold law holds without a hand-rolled table; `identity` reads `Processor.getCacheID()` and `hasChannelCrosstalk()` before any pixel moves, `applied` compiles `getOptimizedCPUProcessor(ingress_depth, F32, OPTIMIZATION_DEFAULT)` so an 8- or 16-bit ingress normalizes INSIDE the compiled chain rather than through a caller-side divide, skips an `isNoOp()` chain whole, and applies through one `PackedImageDesc` pair over the flattened `(texels, channels)` view — a sub-RGB field crosses as a broadcast triple with its trailing channels carried through, legal exactly because a crosstalk chain over it already refused; `classified` answers `getColorSpaceFromFilepath` with `getCanonicalName`, so an ingest reads the config's declared file rules instead of forking a stem convention, and its caller consumes the VALUE. `Managed` and `Plane` fold `_grade` inside their worker via `Block.of_seq(grade).fold`, so decode-linearize, transfer, primary-convert, device-correct, LUT-grade, config-move, and re-encode interleave in one chain; the `managed` `GradeStep` seats the config graph beside the colour-science steps and the `lut` step reads every container `ocio.FileTransform` carries — CLF, CTF, CDL, ICC, and the cube family — where `colour.read_LUT` reaches six formats. `_icc_apply` resolves each `ProfileRef` through `named` (a `BuiltinProfile` passing its own engine column, raw `bytes` gated by `imagecodecs.cms_profile_validate` before the temp-file write on one `ExitStack`), runs `icc_transform`, reads back the embedded profile and the egress `interpretation`, runs the optional Pillow `_softproof` when a `proof` profile is set (the plain-vs-`GAMUTCHECK` diff counting out-of-press-gamut pixels — the lcms2 signal pyvips lacks), reads the peak Total Area Coverage off a CMYK egress, and returns eight scalars rather than the encoded buffer, so the process boundary carries evidence and never the megabyte product it already wrote. Every worker returns `Result[T, ManageFault]`: one boundary arm per worker maps its provider family — `ocio.Exception`/`ocio.ExceptionMissingFile` onto `<ocio-space>`/`<ocio-config>`, `imagecodecs.CmsError` onto `<icc-profile>` — and `_lifted` folds the typed fault onto the `MANAGED_REFUSED` row exactly once where the arm flattens the nested result, so no OCIO or lcms2 raise reaches the interior and no arm reconstructs an exception.
- Growth: a new managed operation is one `ManageOp` case, one total dispatch arm on `_produced`, and one `_canon` preimage arm; a new grade step is one `GradeStep` case folded by `_grade` and one `_step` preimage arm; a new LUT bake modality is one `LutBake` case and one `_lut_author` arm; a new config resolution is one `ConfigSource` case and one `_config` arm; a new output container is one `ConvertFormat` member on its owning `raster/io#IO` row, with zero rows here; a new broadcast curve is one `BroadcastCurve` member and its `_BROADCAST_ROSTER` memberships, admission proving the kind-curve pairing the colour registries admit; a new role, LUT format, interpolation, OCIO depth, built-in profile, curve, intent, PCS, depth, alpha posture, or black-point posture is one member in its closed vocabulary, a profile member filling the engine columns its rosters carry and earning no seat where a name builds a profile yet no transform; a new refusal is one `ManageFault` member breaking every capture at type-check. New boundary invariants refine on the existing `ManagedRaster`, `ColorField`, `ColorOperand`, `CorrectionMatrix`, `ProfileBytes`, `PdfBytes`, `CxfBytes`, `OutPath`, `PageIndex`, `Dpi`, `LutSize`, `ShaperSize`, or `Coverage` admission axis.
- Boundary: colorimetry, appearance models, spectral computation, gamut mapping, palettes, and the CxF color half are `graphic/color/derive#DERIVE`'s, and `ColorModel` is that page's vocabulary composed here — this page mints no colour-model enum of its own. Display-container codec facts are `graphic/raster/io#IO`'s `CODEC` rows, composed by import rather than restated. Deep-pixel plane storage, mip ladders, and KTX2 containers are `graphic/texture`'s; a plane converts through `ConfigSource.applied` at its caller BEFORE it enters that module, and no texture page imports this owner. Process-wide OCIO state — `SetCurrentConfig`, `SetLoggingLevel`, `SetEnvVariable`, `ClearAllCaches` — is the composition root's, so `GetCurrentConfig` gets no reader here and `GetVersion()` rides the startup census, never a per-call fact.
- Packages: `opencolorio` (the config-driven transform graph — `Config.CreateFromBuiltinConfig`/`CreateFromEnv`/`CreateFromFile`/`CreateRaw`, `getProcessor` over a name pair, a `LookTransform`, a `DisplayViewTransform`, or a `GroupTransform` of `FileTransform`s, `getOptimizedCPUProcessor`, `PackedImageDesc`, `isNoOp`/`hasChannelCrosstalk`/`getCacheID`, `getColorSpaceFromFilepath`/`getCanonicalName`, and `Baker`), `pyvips` (`icc_transform` device egress and `write_to_buffer` under `ForeignKeep.ICC`), `imagecodecs` (the lcms2 `cms_profile`/`cms_profile_validate`/`cms_transform` float ICC edge), `colour-science` (the CCTF, broadcast-transfer, RGB-colourspace, matrix-correction, image-write, and `LUT1D`/`LUT3D`/`LUTSequence` surfaces), `pillow` (`ImageCms.buildProofTransform` soft proofing alone), `pikepdf` (the `/Separation` and `/DeviceN` raw object model), `pdf_oxide` (`render_separations`), `colour-cxf` (`read_cxf` device half), with `expression`/`numpy`/`beartype` and the runtime `Metrics`/`LanePolicy`/`Kernel`/`KernelTrait`/`Wire`; the full member surface lives in the package `.api` catalogs.

```python
# --- [IMPORTS] --------------------------------------------------------------------------
from collections.abc import Callable
from contextlib import ExitStack
from dataclasses import dataclass
from enum import Enum, StrEnum
from functools import cache
from io import BytesIO
from itertools import chain
from pathlib import Path
from tempfile import NamedTemporaryFile
from typing import Annotated, Final, Literal, NamedTuple, assert_never

import colour
import numpy as np
from beartype import beartype
from beartype.vale import Is
from builtins import frozendict
from expression import Error, Nothing, Ok, Option, Result, case, tag, tagged_union
from expression.collections import Block
from numpy.typing import NDArray

from rasm.artifacts.core.hooks import BYTE_VOLUME, DOMAIN, ArtifactsLeg
from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.graphic.color.derive import AdaptMethod, ColorModel
from rasm.artifacts.graphic.raster.io import CODEC, CodecEmit, CodecPolicy, RasterEngine, writer
from rasm.artifacts.graphic.raster.process import ConvertFormat
from rasm.runtime.faults import TERMINAL, BoundaryFault, FaultRow, RuntimeResult, rostered
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.metrics import Metrics
from rasm.runtime.workers import Kernel, KernelTrait, Wire

lazy import imagecodecs
lazy import pikepdf
lazy import pdf_oxide
lazy import PyOpenColorIO as ocio
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
    NDArray[np.float32] | NDArray[np.float64],
    Is[lambda value: 2 <= value.ndim <= 4 and value.shape[-1] in (1, 2, 3, 4) and bool(np.isfinite(value).all())],
]
type CorrectionMatrix = Annotated[
    NDArray[np.float64],
    Is[lambda value: value.shape == (3, 3) and bool(np.isfinite(value).all())],
]
type ManagedRaster = Annotated[
    NDArray[np.uint8] | NDArray[np.uint16],
    Is[lambda value: value.ndim == 3 and value.shape[0] > 0 and value.shape[1] > 0 and value.shape[2] in (3, 4)],
]
type ColorOperand = ManagedRaster | ColorField
type LutSize = Annotated[int, Is[lambda n: 2 <= n <= 256]]
type ShaperSize = Annotated[int, Is[lambda n: 2 <= n <= 65536]]
type Coverage = Annotated[float, Is[lambda c: 0.0 <= c <= 100.0]]
type OutPath = Annotated[str, Is[lambda p: len(p) > 0]]
type PdfBytes = Annotated[bytes, Is[lambda data: data.startswith(b"%PDF-")]]
type CxfBytes = Annotated[bytes, Is[lambda data: len(data) > 0]]
type ProfileBytes = Annotated[bytes, Is[lambda data: len(data) > 0]]
type PageIndex = Annotated[int, Is[lambda page: page >= 0]]
type Dpi = Annotated[int, Is[lambda dpi: 36 <= dpi <= 2400]]
type ManageFault = Literal[
    "<channel-crosstalk>",
    "<codec-writer>",
    "<empty-channels>",
    "<empty-lut>",
    "<icc-bands>",
    "<icc-depth>",
    "<icc-profile>",
    "<lut-writer>",
    "<ocio-config>",
    "<ocio-space>",
    "<profile-engine>",
    "<proof-source>",
    "<transfer-route>",
]
type ManageOpTag = Literal["managed", "plane", "space", "view", "export", "plate", "lut", "swatch", "separate"]


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


class BitDepth(StrEnum):
    UINT8 = "uint8"
    UINT16 = "uint16"
    FLOAT32 = "float32"


class OcioRole(StrEnum):
    SCENE_LINEAR = "scene_linear"
    DATA = "data"
    REFERENCE = "reference"
    DEFAULT = "default"
    RENDERING = "rendering"
    COLOR_PICKING = "color_picking"
    COLOR_TIMING = "color_timing"
    COMPOSITING_LOG = "compositing_log"
    MATTE_PAINT = "matte_paint"
    TEXTURE_PAINT = "texture_paint"
    INTERCHANGE_SCENE = "aces_interchange"
    INTERCHANGE_DISPLAY = "cie_xyz_d65_interchange"


class OcioDepth(StrEnum):
    UINT8 = "BIT_DEPTH_UINT8"
    UINT16 = "BIT_DEPTH_UINT16"
    F16 = "BIT_DEPTH_F16"
    F32 = "BIT_DEPTH_F32"


class LutInterp(StrEnum):
    NEAREST = "INTERP_NEAREST"
    LINEAR = "INTERP_LINEAR"
    TETRAHEDRAL = "INTERP_TETRAHEDRAL"
    CUBIC = "INTERP_CUBIC"
    BEST = "INTERP_BEST"


class LutFormat(StrEnum):
    FLAME = "flame"
    LUSTRE = "lustre"
    CLF = "Academy/ASC Common LUT Format"
    CTF = "Color Transform Format"
    CINESPACE = "cinespace"
    HOUDINI = "houdini"
    IRIDAS_CUBE = "iridas_cube"
    IRIDAS_ITX = "iridas_itx"
    RESOLVE_CUBE = "resolve_cube"
    SPI1D = "spi1d"
    SPI3D = "spi3d"
    TRUELIGHT = "truelight"


class AlphaBand(StrEnum):
    NONE = "none"
    TRAILING = "trailing"

    @property
    def colour_bands(self) -> int:
        return 1 if self is AlphaBand.TRAILING else 0


class ProfileNames(NamedTuple):
    vips: str | None
    cms: str | None


class BuiltinProfile(ProfileNames, Enum):
    SRGB = ProfileNames("srgb", "srgb")
    P3 = ProfileNames("p3", None)
    CMYK = ProfileNames("cmyk", None)
    ADOBE_RGB = ProfileNames(None, "adobergb")
    XYZ = ProfileNames(None, "xyz")


type ProfileRef = ProfileBytes | BuiltinProfile
type SpaceRef = OcioRole | str
type ChainTarget = SpaceRef | tuple[str, str]


# --- [MODELS] ---------------------------------------------------------------------------
@tagged_union(frozen=True)
class ConfigSource:
    tag: Literal["builtin", "env", "file", "raw"] = tag()
    builtin: str = case()
    env: None = case()
    file: OutPath = case()
    raw: None = case()

    @property
    def label(self) -> str:
        match self:
            case ConfigSource(tag="builtin", builtin=name) | ConfigSource(tag="file", file=name):
                return name
            case ConfigSource(tag="env") | ConfigSource(tag="raw"):
                return self.tag
            case _ as unreachable:
                assert_never(unreachable)

    def identity(self, src: SpaceRef, target: ChainTarget, look: str = "", /) -> tuple[str, bool]:
        processor = _processor(self, src, target, look)
        return processor.getCacheID(), processor.hasChannelCrosstalk()

    def applied(self, field: ColorOperand, src: SpaceRef, target: ChainTarget, look: str = "", /) -> ColorField:
        return _transformed(_processor(self, src, target, look), field)

    def classified(self, path: OutPath, /) -> tuple[str, int]:
        config = _config(self)
        name, rule = config.getColorSpaceFromFilepath(path)
        return config.getCanonicalName(name), rule


_BUILTIN: Final[ConfigSource] = ConfigSource(builtin="ocio://default")
_RAW: Final[ConfigSource] = ConfigSource(raw=None)


@tagged_union(frozen=True)
class GradeStep:
    tag: Literal["cctf", "broadcast", "colourspace", "correction", "lut", "managed"] = tag()
    cctf: tuple[Transfer, ToneCurve] = case()
    broadcast: tuple[TransferKind, BroadcastCurve] = case()
    colourspace: tuple[ColorModel, ColorModel, AdaptMethod] = case()
    correction: CorrectionMatrix = case()
    lut: tuple[LutInterp, tuple[OutPath, ...]] = case()
    managed: tuple[ConfigSource, SpaceRef, ChainTarget] = case()

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
    def Colourspace(source: ColorModel, target: ColorModel, adapt: AdaptMethod = AdaptMethod.BRADFORD) -> Result["GradeStep", ManageFault]:
        return (
            Ok(GradeStep(colourspace=(source, target, adapt)))
            if source.rgb is not None and target.rgb is not None
            else Error("<transfer-route>")
        )

    @staticmethod
    @beartype
    def Correction(ccm: CorrectionMatrix, /) -> "GradeStep":
        return GradeStep(correction=ccm)

    @staticmethod
    @beartype
    def Lut(*paths: OutPath, interp: LutInterp = LutInterp.TETRAHEDRAL) -> Result["GradeStep", ManageFault]:
        return Ok(GradeStep(lut=(interp, paths))) if paths else Error("<empty-lut>")

    @staticmethod
    @beartype
    def Managed(source: ConfigSource = _BUILTIN, src: SpaceRef = OcioRole.SCENE_LINEAR, target: ChainTarget = OcioRole.DATA) -> "GradeStep":
        return GradeStep(managed=(source, src, target))


@tagged_union(frozen=True)
class LutBake:
    tag: Literal["graded", "spaced", "viewed"] = tag()
    graded: tuple[ColorModel, tuple[GradeStep, ...], Option[ToneCurve]] = case()
    spaced: tuple[ConfigSource, SpaceRef, SpaceRef, str, LutFormat, Option[SpaceRef]] = case()
    viewed: tuple[ConfigSource, SpaceRef, str, str, LutFormat, Option[SpaceRef]] = case()

    @staticmethod
    @beartype
    def Graded(space: ColorModel, grade: tuple[GradeStep, ...] = (), shaper: Option[ToneCurve] = Nothing) -> Result["LutBake", ManageFault]:
        return Ok(LutBake(graded=(space, grade, shaper))) if space.rgb is not None else Error("<transfer-route>")

    @staticmethod
    @beartype
    def Spaced(
        source: ConfigSource = _BUILTIN,
        src: SpaceRef = OcioRole.SCENE_LINEAR,
        dst: SpaceRef = OcioRole.COLOR_PICKING,
        fmt: LutFormat = LutFormat.CLF,
        look: str = "",
        shaper: Option[SpaceRef] = Nothing,
    ) -> "LutBake":
        return LutBake(spaced=(source, src, dst, look, fmt, shaper))

    @staticmethod
    @beartype
    def Viewed(
        source: ConfigSource, src: SpaceRef, display: str, view: str, fmt: LutFormat = LutFormat.CLF, shaper: Option[SpaceRef] = Nothing
    ) -> "LutBake":
        return LutBake(viewed=(source, src, display, view, fmt, shaper))


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
    codec_policy: CodecPolicy = CodecPolicy(quality=92, effort=6)
    proof: Option[ProfileBytes] = Nothing
    separations: tuple[SpotChannel, ...] = ()


_ICC_DEFAULT: IccTransform = IccTransform()


@tagged_union(frozen=True)
class ManageOp:
    tag: ManageOpTag = tag()
    managed: tuple[ManagedRaster, OutPath, ProfileRef, ProfileRef, IccTransform, ConvertFormat, tuple[GradeStep, ...]] = case()
    plane: tuple[ColorField, OutPath, ProfileRef, ProfileRef, IccTransform, AlphaBand] = case()
    space: tuple[ColorOperand, OutPath, ConfigSource, SpaceRef, SpaceRef, str] = case()
    view: tuple[ColorOperand, OutPath, ConfigSource, SpaceRef, str, str] = case()
    export: tuple[ColorField, OutPath, BitDepth, tuple[GradeStep, ...]] = case()
    plate: tuple[PdfBytes, OutPath, tuple[SpotChannel, ...], IccTransform] = case()
    lut: tuple[LutBake, OutPath, LutSize, ShaperSize, RenderingIntent] = case()
    swatch: CxfBytes = case()
    separate: tuple[PdfBytes, PageIndex, Dpi, Option[OutPath]] = case()

    @staticmethod
    @beartype
    def Managed(
        raster: ManagedRaster,
        path: OutPath,
        src_profile: ProfileRef,
        dst_profile: ProfileRef,
        transform: IccTransform = _ICC_DEFAULT,
        codec: ConvertFormat = ConvertFormat.PNG,
        grade: tuple[GradeStep, ...] = (),
    ) -> Result["ManageOp", ManageFault]:
        return (
            Error("<proof-source>")
            if transform.proof.is_some() and not isinstance(src_profile, bytes) and src_profile is not BuiltinProfile.SRGB
            else Error("<icc-depth>")
            if transform.depth is BitDepth.FLOAT32
            else Error("<profile-engine>")
            if not _vips_named(src_profile) or not _vips_named(dst_profile)
            else Error("<codec-writer>")
            if not _vips_native(codec)
            else Ok(ManageOp(managed=(raster, path, src_profile, dst_profile, transform, codec, grade)))
        )

    @staticmethod
    @beartype
    def Plane(
        field: ColorField,
        path: OutPath,
        src_profile: ProfileRef,
        dst_profile: ProfileRef,
        transform: IccTransform = _ICC_DEFAULT,
        alpha: AlphaBand = AlphaBand.NONE,
    ) -> Result["ManageOp", ManageFault]:
        return (
            Error("<profile-engine>")
            if not _cms_named(src_profile) or not _cms_named(dst_profile)
            else Error("<icc-bands>")
            if field.shape[-1] - alpha.colour_bands != 3
            else Ok(ManageOp(plane=(field, path, src_profile, dst_profile, transform, alpha)))
        )

    @staticmethod
    @beartype
    def Space(
        field: ColorOperand,
        path: OutPath,
        config: ConfigSource = _BUILTIN,
        src: SpaceRef = OcioRole.TEXTURE_PAINT,
        dst: SpaceRef = OcioRole.SCENE_LINEAR,
        look: str = "",
    ) -> "ManageOp":
        return ManageOp(space=(field, path, config, src, dst, look))

    @staticmethod
    @beartype
    def View(field: ColorOperand, path: OutPath, config: ConfigSource, src: SpaceRef, display: str, view: str) -> "ManageOp":
        return ManageOp(view=(field, path, config, src, display, view))

    @staticmethod
    @beartype
    def Export(field: ColorField, path: OutPath, depth: BitDepth = BitDepth.UINT16, grade: tuple[GradeStep, ...] = ()) -> "ManageOp":
        return ManageOp(export=(field, path, depth, grade))

    @staticmethod
    @beartype
    def Plate(
        document: PdfBytes, path: OutPath, channels: tuple[SpotChannel, ...], transform: IccTransform = _ICC_DEFAULT
    ) -> Result["ManageOp", ManageFault]:
        return Ok(ManageOp(plate=(document, path, channels, transform))) if channels else Error("<empty-channels>")

    @staticmethod
    @beartype
    def Lut(
        bake: LutBake,
        path: OutPath,
        size: LutSize = 33,
        shaper: ShaperSize = 1024,
        intent: RenderingIntent = RenderingIntent.RELATIVE,
    ) -> Result["ManageOp", ManageFault]:
        return (
            Ok(ManageOp(lut=(bake, path, size, shaper, intent)))
            if bake.tag != "graded" or Path(path).suffix in _COLOUR_LUT
            else Error("<lut-writer>")
        )

    @staticmethod
    @beartype
    def Swatch(document: CxfBytes) -> "ManageOp":
        return ManageOp(swatch=document)

    @staticmethod
    @beartype
    def Separate(document: PdfBytes, page: PageIndex = 0, dpi: Dpi = 150, plates: Option[OutPath] = Nothing) -> "ManageOp":
        return ManageOp(separate=(document, page, dpi, plates))


# --- [TABLES] ---------------------------------------------------------------------------

MANAGED_REFUSED: Final[FaultRow[ArtifactsLeg]] = FaultRow(
    leg=ArtifactsLeg.MANAGED, point="produce", arm="config", defect="manage-refused", retriability=TERMINAL, slots=("cause",)
)
RAISES: Final[Block[FaultRow[ArtifactsLeg]]] = rostered(Block.of_seq([MANAGED_REFUSED]))

_TRANSFER: Final[frozendict[Transfer, Callable[..., ColorField]]] = frozendict({
    Transfer.ENCODE: colour.cctf_encoding,
    Transfer.DECODE: colour.cctf_decoding,
})
_BROADCAST: Final[frozendict[TransferKind, Callable[..., ColorField]]] = frozendict({
    TransferKind.OETF: colour.oetf,
    TransferKind.EOTF: colour.eotf,
    TransferKind.OOTF: colour.ootf,
})
_BROADCAST_ROSTER: Final[frozendict[TransferKind, frozenset[BroadcastCurve]]] = frozendict({
    TransferKind.OETF: frozenset({BroadcastCurve.BT709, BroadcastCurve.BT2100_PQ, BroadcastCurve.BT2100_HLG}),
    TransferKind.EOTF: frozenset({BroadcastCurve.BT1886, BroadcastCurve.BT2100_PQ, BroadcastCurve.BT2100_HLG}),
    TransferKind.OOTF: frozenset({BroadcastCurve.BT2100_PQ, BroadcastCurve.BT2100_HLG}),
})
_DEPTH_BITS: Final[frozendict[BitDepth, int]] = frozendict({BitDepth.UINT8: 8, BitDepth.UINT16: 16})
_INGRESS_DEPTH: Final[frozendict[str, OcioDepth]] = frozendict({
    "uint8": OcioDepth.UINT8,
    "uint16": OcioDepth.UINT16,
    "float16": OcioDepth.F16,
    "float32": OcioDepth.F32,
})
_CARRIER: Final[frozendict[OcioDepth, type[np.generic]]] = frozendict({
    OcioDepth.UINT8: np.uint8,
    OcioDepth.UINT16: np.uint16,
    OcioDepth.F16: np.float16,
    OcioDepth.F32: np.float32,
})
_INTENT_NAME: Final[frozendict[RenderingIntent, str]] = frozendict({
    RenderingIntent.PERCEPTUAL: "PERCEPTUAL",
    RenderingIntent.RELATIVE: "RELATIVE_COLORIMETRIC",
    RenderingIntent.SATURATION: "SATURATION",
    RenderingIntent.ABSOLUTE: "ABSOLUTE_COLORIMETRIC",
    RenderingIntent.AUTO: "PERCEPTUAL",
})
_PLATE_DEPTH: Final[str] = BitDepth.UINT16.value
_COLOUR_LUT: Final[frozenset[str]] = frozenset({".cube", ".csp", ".spi1d", ".spi3d", ".spimtx"})


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
        return ContentIdentity.key(f"color-managed-{self.op.tag}", _canon(self.op))

    async def _emit(self) -> RuntimeResult[object]:
        match await self._produced():
            case Result(tag="ok", ok=product):
                match self.op, product:
                    case ManageOp(tag="managed" | "plane" | "space" | "view" | "plate"), (int(size), *_):
                        pass
                    case ManageOp(tag="export"), (_, int(size)):
                        pass
                    case ManageOp(tag="lut"), (_, int(size), _, _):
                        pass
                    case ManageOp(tag="separate", separate=(document, *_)) | ManageOp(tag="swatch", swatch=document), _:
                        size = len(document)
                    case _ as unreachable:
                        assert_never(unreachable)
                Metrics.record({BYTE_VOLUME: float(size)}, domain=DOMAIN, kind="color", scope=self.lane.scope)
                return Ok(product)
            case refused:
                return Error(refused.error)

    async def _produced(self) -> RuntimeResult[object]:
        match self.op:
            case ManageOp(tag="managed", managed=(raster, path, src_profile, dst_profile, transform, codec, grade)):
                crossed = await self.lane.offload(
                    Kernel.of(_icc_apply, KernelTrait.HOSTILE, wire=Wire.SHARED_MEMORY, idempotent=False),
                    raster,
                    path,
                    src_profile,
                    dst_profile,
                    transform.intent,
                    transform.black_point.enabled,
                    transform.pcs.value,
                    _DEPTH_BITS[transform.depth],
                    codec,
                    transform.codec_policy,
                    grade,
                    transform.proof.to_optional(),
                )
                return self._flattened(crossed)
            case ManageOp(tag="plane", plane=(field, path, src_profile, dst_profile, transform, alpha)):
                crossed = await self.lane.offload(
                    Kernel.of(_cms_apply, KernelTrait.RELEASING, idempotent=False),
                    field,
                    path,
                    src_profile,
                    dst_profile,
                    transform.intent,
                    transform.black_point.enabled,
                    alpha,
                )
                return self._flattened(crossed)
            case ManageOp(tag="space", space=(field, path, config, src, dst, look)):
                crossed = await self.lane.offload(Kernel.of(_ocio_apply, KernelTrait.RELEASING, idempotent=False), field, path, config, src, dst, look)
                return self._flattened(crossed)
            case ManageOp(tag="view", view=(field, path, config, src, display, view)):
                crossed = await self.lane.offload(
                    Kernel.of(_ocio_apply, KernelTrait.RELEASING, idempotent=False), field, path, config, src, (display, view), ""
                )
                return self._flattened(crossed)
            case ManageOp(tag="export", export=(field, path, depth, grade)):
                crossed = await self.lane.offload(Kernel.of(_export_image, KernelTrait.RELEASING, idempotent=False), field, path, depth.value, grade)
                return self._flattened(crossed)
            case ManageOp(tag="plate", plate=(document, path, channels, transform)):
                crossed = await self.lane.offload(Kernel.of(_plate_author, KernelTrait.RELEASING, idempotent=False), document, path, channels)
                return crossed
            case ManageOp(tag="lut", lut=(bake, path, size, shaper, intent)):
                crossed = await self.lane.offload(Kernel.of(_lut_author, KernelTrait.RELEASING, idempotent=False), bake, path, size, shaper)
                return self._flattened(crossed)
            case ManageOp(tag="separate", separate=(document, page, dpi, plates)):
                crossed = await self.lane.offload(
                    Kernel.of(_separate, KernelTrait.RELEASING, idempotent=plates.is_none()), document, page, dpi, plates
                )
                return crossed
            case ManageOp(tag="swatch", swatch=document):
                crossed = await self.lane.offload(Kernel.of(separations, KernelTrait.RELEASING), document)
                return crossed
            case _:
                assert_never(self.op)

    def _flattened[T](self, crossed: RuntimeResult[Result[T, ManageFault]], /) -> RuntimeResult[T]:
        return crossed.bind(lambda produced: produced.map_error(_lifted))


# --- [OPERATIONS] -----------------------------------------------------------------------
def _lifted(fault: ManageFault, /) -> BoundaryFault:
    return MANAGED_REFUSED.raised(fault)


def _framed(*chunks: bytes) -> tuple[bytes, ...]:
    return (len(chunks).to_bytes(4, "big"), *chain.from_iterable((len(chunk).to_bytes(8, "big"), chunk) for chunk in chunks))


def _packed(*chunks: bytes) -> bytes:
    return b"".join(_framed(*chunks))


def _array(value: ColorOperand, /) -> bytes:
    return _packed(str(value.dtype).encode(), repr(value.shape).encode(), np.ascontiguousarray(value).tobytes())


def _profile(ref: ProfileRef, /) -> bytes:
    return ref.name.encode() if isinstance(ref, BuiltinProfile) else ref


def _vips_named(ref: ProfileRef, /) -> bool:
    return not isinstance(ref, BuiltinProfile) or ref.vips is not None


def _cms_named(ref: ProfileRef, /) -> bool:
    return not isinstance(ref, BuiltinProfile) or ref.cms is not None


def _vips_native(codec: ConvertFormat, /) -> bool:
    return any(emit.tag == "native" for emit in CODEC[codec].writers.get(RasterEngine.LIBVIPS, ()))


def _source(config: ConfigSource, /) -> bytes:
    return _packed(config.tag.encode(), config.label.encode())


def _target(target: ChainTarget, /) -> bytes:
    return _packed(*(part.encode() for part in target)) if isinstance(target, tuple) else _packed(str(target).encode())


def _step(step: GradeStep, /) -> bytes:
    match step:
        case GradeStep(tag="cctf", cctf=(direction, curve)):
            return _packed(b"cctf", direction.value.encode(), curve.value.encode())
        case GradeStep(tag="broadcast", broadcast=(kind, curve)):
            return _packed(b"broadcast", kind.value.encode(), curve.value.encode())
        case GradeStep(tag="colourspace", colourspace=(source, target, adapt)):
            return _packed(b"colourspace", source.name.encode(), target.name.encode(), adapt.value.encode())
        case GradeStep(tag="correction", correction=ccm):
            return _packed(b"correction", np.ascontiguousarray(ccm).tobytes())
        case GradeStep(tag="lut", lut=(interp, paths)):
            return _packed(b"lut", interp.value.encode(), *(path.encode() for path in paths))
        case GradeStep(tag="managed", managed=(source, src, target)):
            return _packed(b"managed", _source(source), str(src).encode(), _target(target))
        case _ as unreachable:
            assert_never(unreachable)


def _shaper(shaper: Option[SpaceRef], /) -> bytes:
    return shaper.map(str).default_value("").encode()


def _bake(bake: LutBake, /) -> bytes:
    match bake:
        case LutBake(tag="graded", graded=(space, grade, shaper)):
            return _packed(b"graded", space.name.encode(), shaper.map(lambda curve: curve.value).default_value("").encode(), *map(_step, grade))
        case LutBake(tag="spaced", spaced=(source, src, dst, look, fmt, shaper)):
            return _packed(b"spaced", _source(source), str(src).encode(), str(dst).encode(), look.encode(), fmt.value.encode(), _shaper(shaper))
        case LutBake(tag="viewed", viewed=(source, src, display, view, fmt, shaper)):
            return _packed(b"viewed", _source(source), str(src).encode(), display.encode(), view.encode(), fmt.value.encode(), _shaper(shaper))
        case _ as unreachable:
            assert_never(unreachable)


def _bundle(transform: IccTransform, /) -> bytes:
    return _packed(
        transform.intent.value.encode(),
        transform.black_point.value.encode(),
        transform.pcs.value.encode(),
        transform.depth.value.encode(),
        transform.codec_policy.quality.to_bytes(1, "big"),
        transform.codec_policy.effort.to_bytes(1, "big"),
        transform.proof.default_value(b""),
        *(_packed(channel.name.encode(), repr(channel.coverage).encode()) for channel in transform.separations),
    )


def _canon(op: ManageOp, /) -> tuple[bytes, ...]:
    match op:
        case ManageOp(tag="managed", managed=(raster, path, src, dst, transform, codec, grade)):
            return _framed(
                b"managed", _array(raster), path.encode(), _profile(src), _profile(dst), _bundle(transform), codec.value.encode(), *map(_step, grade)
            )
        case ManageOp(tag="plane", plane=(field, path, src, dst, transform, alpha)):
            return _framed(b"plane", _array(field), path.encode(), _profile(src), _profile(dst), _bundle(transform), alpha.value.encode())
        case ManageOp(tag="space", space=(field, path, config, src, dst, look)):
            return _framed(b"space", _array(field), path.encode(), _source(config), str(src).encode(), str(dst).encode(), look.encode())
        case ManageOp(tag="view", view=(field, path, config, src, display, view)):
            return _framed(b"view", _array(field), path.encode(), _source(config), str(src).encode(), display.encode(), view.encode())
        case ManageOp(tag="export", export=(field, path, depth, grade)):
            return _framed(b"export", _array(field), path.encode(), depth.value.encode(), *map(_step, grade))
        case ManageOp(tag="plate", plate=(document, path, channels, transform)):
            return _framed(
                b"plate", document, path.encode(), _bundle(transform), *(_packed(row.name.encode(), repr(row.coverage).encode()) for row in channels)
            )
        case ManageOp(tag="lut", lut=(bake, path, size, shaper, intent)):
            return _framed(b"lut", _bake(bake), path.encode(), intent.value.encode(), size.to_bytes(2, "big"), shaper.to_bytes(4, "big"))
        case ManageOp(tag="swatch", swatch=document):
            return _framed(b"swatch", document)
        case ManageOp(tag="separate", separate=(document, page, dpi, plates)):
            return _framed(b"separate", document, page.to_bytes(4, "big"), dpi.to_bytes(2, "big"), plates.default_value("").encode())
        case _ as unreachable:
            assert_never(unreachable)


@cache
def _config(source: ConfigSource, /) -> "ocio.Config":
    match source:
        case ConfigSource(tag="builtin", builtin=name):
            return ocio.Config.CreateFromBuiltinConfig(name)
        case ConfigSource(tag="env"):
            return ocio.Config.CreateFromEnv()
        case ConfigSource(tag="file", file=path):
            return ocio.Config.CreateFromFile(path)
        case ConfigSource(tag="raw"):
            return ocio.Config.CreateRaw()
        case _ as unreachable:
            assert_never(unreachable)


def _depth(depth: OcioDepth, /) -> "ocio.BitDepth":
    return getattr(ocio.BitDepth, depth.value)


def _desc(buf: NDArray[np.generic], depth: "ocio.BitDepth", /) -> "ocio.PackedImageDesc":
    return ocio.PackedImageDesc(buf, buf.shape[0], 1, buf.shape[1], depth, buf.itemsize, buf.strides[0], buf.strides[0] * buf.shape[0])


def _processor(source: ConfigSource, src: SpaceRef, target: ChainTarget, look: str, /) -> "ocio.Processor":
    config = _config(source)
    match target:
        case (display, view):
            return config.getProcessor(
                ocio.DisplayViewTransform(src=str(src), display=display, view=view), ocio.TransformDirection.TRANSFORM_DIR_FORWARD
            )
        case dst if look:
            return config.getProcessor(ocio.LookTransform(src=str(src), dst=str(dst), looks=look), ocio.TransformDirection.TRANSFORM_DIR_FORWARD)
        case dst:
            return config.getProcessor(str(src), str(dst))


def _transformed(processor: "ocio.Processor", field: ColorOperand, /) -> ColorField:
    depth = _INGRESS_DEPTH.get(str(field.dtype), OcioDepth.F32)
    cpu = processor.getOptimizedCPUProcessor(_depth(depth), _depth(OcioDepth.F32), ocio.OptimizationFlags.OPTIMIZATION_DEFAULT)
    if cpu.isNoOp():
        return np.ascontiguousarray(field, dtype=np.float32)
    channels = field.shape[-1]
    flat = np.ascontiguousarray(field, dtype=_CARRIER[depth]).reshape(-1, channels)
    wide = flat if channels >= 3 else np.ascontiguousarray(np.repeat(flat[:, :1], 3, axis=1))
    egress = np.empty(wide.shape, dtype=np.float32)
    cpu.apply(_desc(wide, _depth(depth)), _desc(egress, _depth(OcioDepth.F32)))
    landed = egress if channels >= 3 else np.concatenate([egress[:, :1], flat[:, 1:].astype(np.float32)], axis=1)
    return landed.reshape(*field.shape[:-1], channels)


def _guarded[T](thunk: Callable[[], Result[T, ManageFault]], /) -> Result[T, ManageFault]:
    try:
        return thunk()
    except ocio.ExceptionMissingFile:
        return Error("<ocio-config>")
    except ocio.Exception:
        return Error("<ocio-space>")
    except imagecodecs.CmsError:
        return Error("<icc-profile>")


def _grade(field: ColorField, steps: tuple[GradeStep, ...]) -> ColorField:
    def applied(acc: ColorField, step: GradeStep) -> ColorField:
        match step:
            case GradeStep(tag="cctf", cctf=(direction, curve)):
                return _TRANSFER[direction](acc, function=curve.value)
            case GradeStep(tag="broadcast", broadcast=(kind, curve)):
                return _BROADCAST[kind](acc, function=curve.value)
            case GradeStep(tag="colourspace", colourspace=(source, target, adapt)):
                return colour.RGB_to_RGB(
                    acc, colour.RGB_COLOURSPACES[source.rgb], colour.RGB_COLOURSPACES[target.rgb], chromatic_adaptation_transform=adapt.value
                )
            case GradeStep(tag="correction", correction=ccm):
                return colour.apply_matrix_colour_correction(acc, ccm)
            case GradeStep(tag="lut", lut=(interp, paths)):
                group = ocio.GroupTransform([ocio.FileTransform(src=path, interpolation=getattr(ocio.Interpolation, interp.value)) for path in paths])
                return _transformed(_config(_RAW).getProcessor(group, ocio.TransformDirection.TRANSFORM_DIR_FORWARD), acc)
            case GradeStep(tag="managed", managed=(source, src, target)):
                return source.applied(acc, src, target)
            case _ as unreachable:
                assert_never(unreachable)

    return Block.of_seq(steps).fold(applied, field)


def _written(toned: ColorField, path: OutPath, depth: str, /) -> tuple[ColorField, int]:
    colour.write_image(toned, path, depth)
    return toned, Path(path).stat().st_size


def _export_image(field: ColorField, path: OutPath, depth: str, grade: tuple[GradeStep, ...]) -> Result[tuple[ColorField, int], ManageFault]:
    return _guarded(lambda: Ok(_written(_grade(field, grade), path, depth)))


def _ocio_apply(
    field: ColorOperand, path: OutPath, source: ConfigSource, src: SpaceRef, target: ChainTarget, look: str
) -> Result[tuple[int, int, int, int, str, bool], ManageFault]:
    def resolved() -> Result[tuple[int, int, int, int, str, bool], ManageFault]:
        cache_id, crosstalk = source.identity(src, target, look)
        return (
            Error("<channel-crosstalk>")
            if crosstalk and field.shape[-1] < 3
            else Ok(_measured(_written(source.applied(field, src, target, look), path, BitDepth.FLOAT32.value), cache_id, crosstalk))
        )

    return _guarded(resolved)


def _measured(produced: tuple[ColorField, int], cache_id: str, crosstalk: bool, /) -> tuple[int, int, int, int, str, bool]:
    landed, bytes_ = produced
    return bytes_, landed.shape[1], landed.shape[0], landed.shape[-1], cache_id, crosstalk


def _cms_apply(
    field: ColorField, path: OutPath, src: ProfileRef, dst: ProfileRef, intent: RenderingIntent, bpc: bool, alpha: AlphaBand
) -> Result[tuple[int, int, int, int], ManageFault]:
    def resolved() -> Result[tuple[int, int, int, int], ManageFault]:
        carried = field[..., -1:].astype(np.float32) if alpha is AlphaBand.TRAILING else None
        managed = imagecodecs.cms_transform(
            np.ascontiguousarray(field[..., :3], dtype=np.float32),
            _blob(src),
            _blob(dst),
            intent=getattr(imagecodecs.CMS.INTENT, _INTENT_NAME[intent]),
            flags=imagecodecs.CMS.FLAGS.BLACKPOINTCOMPENSATION if bpc else 0,
            outdtype=np.float32,
        )
        landed = managed if carried is None else np.concatenate((managed, carried), axis=-1)
        _, bytes_ = _written(landed, path, BitDepth.FLOAT32.value)
        return Ok((bytes_, landed.shape[1], landed.shape[0], landed.shape[-1]))

    return _guarded(resolved)


def _blob(ref: ProfileRef, /) -> ProfileBytes:
    resolved = imagecodecs.cms_profile(ref.cms) if isinstance(ref, BuiltinProfile) else ref
    imagecodecs.cms_profile_validate(resolved)
    return resolved


def _softproof(rgb8: NDArray[np.uint8], reference: str | ImageCms.ImageCmsProfile, proof_path: str, intent: RenderingIntent) -> int:
    origin = PilImage.fromarray(rgb8, "RGB")
    intent_member = getattr(ImageCms.Intent, _INTENT_NAME[intent])
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


def _separate(
    document: PdfBytes, page: PageIndex, dpi: Dpi, plates: Option[OutPath]
) -> tuple[float, tuple[tuple[str, float], ...], Option[int]]:
    doc = pdf_oxide.PdfDocument.from_bytes(document)
    fields = Block.of_seq(doc.render_separations(page, dpi)).map(
        lambda plate: (str(plate.ink_name), np.frombuffer(plate.data, dtype=np.uint8).astype(np.float64).reshape(plate.height, plate.width) / 255.0)
    )
    coverages = tuple((name, float(field.mean()) * 100.0) for name, field in fields)
    tac_peak = float(np.stack([field for _, field in fields]).sum(axis=0).max()) * 100.0 if not fields.is_empty() else 0.0
    written = plates.map(
        lambda root: fields.fold(lambda acc, row: acc + _written(row[1][..., np.newaxis], str(Path(root) / f"{row[0]}.png"), _PLATE_DEPTH)[1], 0)
    )
    return tac_peak, coverages, written


def _icc_apply(
    raster: ManagedRaster,
    path: OutPath,
    src: ProfileRef,
    dst: ProfileRef,
    intent: RenderingIntent,
    bpc: bool,
    pcs: str,
    depth: int,
    codec: ConvertFormat,
    codec_policy: CodecPolicy,
    grade: tuple[GradeStep, ...],
    proof: ProfileBytes | None,
) -> Result[tuple[int, int, int, int, bool, str, int, float], ManageFault]:
    def named(stack: ExitStack, profile: ProfileRef, /) -> str:
        if isinstance(profile, BuiltinProfile):
            return profile.vips
        handle = stack.enter_context(NamedTemporaryFile(suffix=".icc", delete_on_close=False))
        handle.write(_blob(profile))
        handle.close()
        return handle.name

    def landed(emit: tuple[str, frozendict[str, object]], /) -> tuple[int, int, int, int, bool, str, int, float]:
        suffix, options = emit
        toned = _grade(raster / np.float64(np.iinfo(raster.dtype).max), grade)
        image = pyvips.Image.new_from_array(toned)
        with ExitStack() as stack:
            src_path = named(stack, src)
            managed = image.icc_transform(
                named(stack, dst), input_profile=src_path, intent=intent.value, black_point_compensation=bpc, pcs=pcs, depth=depth
            )
            gamut = (
                _softproof(
                    np.clip(np.asarray(toned)[..., :3] * 255.0, 0.0, 255.0).astype(np.uint8),
                    src_path if not isinstance(src, BuiltinProfile) else ImageCms.createProfile("sRGB"),
                    named(stack, proof),
                    intent,
                )
                if proof is not None
                else 0
            )
            space = str(managed.interpretation)
            ink = float((managed[0] + managed[1] + managed[2] + managed[3]).maxpos()[0]) / float((1 << depth) - 1) * 100.0 if space == "cmyk" else 0.0
            Path(path).write_bytes(managed.write_to_buffer(suffix, keep=pyvips.ForeignKeep.ICC, **options))
            return (
                Path(path).stat().st_size,
                managed.width,
                managed.height,
                managed.bands,
                managed.get_typeof("icc-profile-data") != 0,
                space,
                gamut,
                ink,
            )

    return _guarded(lambda: _vips_emit(codec, codec_policy).map(landed))


def _vips_emit(codec: ConvertFormat, codec_policy: CodecPolicy, /) -> Result[tuple[str, frozendict[str, object]], ManageFault]:
    match writer(codec, RasterEngine.LIBVIPS):
        case Ok(CodecEmit(tag="native", native=(suffix, _probe, options))):
            return Ok((suffix, options(codec_policy)))
        case _:
            return Error("<codec-writer>")


def _plate_author(document: PdfBytes, path: OutPath, channels: tuple[SpotChannel, ...]) -> tuple[int, int]:
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
        for page in pdf.pages:
            for name, space in spaces.items():
                page.add_resource(space, pikepdf.Name("/ColorSpace"), pikepdf.Name(f"/{name}"))
        pdf.save(path)
        return Path(path).stat().st_size, len(pdf.pages)


def lut_bytes(space: ColorModel, grade: tuple[GradeStep, ...], size: LutSize, /) -> bytes:
    return _lattice(grade, size).astype(np.float32).reshape(-1, 3).tobytes()


def _lattice(grade: tuple[GradeStep, ...], size: LutSize, /) -> ColorField:
    axis = np.linspace(0.0, 1.0, size)
    r, g, b = np.meshgrid(axis, axis, axis, indexing="ij")
    return _grade(np.stack([r, g, b], axis=-1), grade)


def _lut_author(bake: LutBake, path: OutPath, size: LutSize, shaper: ShaperSize) -> Result[tuple[int, int, str, str], ManageFault]:
    def resolved() -> Result[tuple[int, int, str, str], ManageFault]:
        match bake:
            case LutBake(tag="graded", graded=(space, grade, curve)):
                cube = colour.LUT3D(table=_lattice(grade, size), name=f"rasm-{space.rgb}")
                colour.write_LUT(curve.map(lambda tone: colour.LUTSequence(_shaper_lut(tone, shaper), cube)).default_value(cube), path)
                return Ok((size**3, Path(path).stat().st_size, space.rgb or "", Path(path).suffix))
            case LutBake(tag="spaced", spaced=(source, src, dst, look, fmt, shaper_space)):
                return Ok(_baked(source, src, dst, look, fmt, shaper_space, path, size, shaper))
            case LutBake(tag="viewed", viewed=(source, src, display, view, fmt, shaper_space)):
                return Ok(_baked(source, src, (display, view), "", fmt, shaper_space, path, size, shaper))
            case _ as unreachable:
                assert_never(unreachable)

    return _guarded(resolved)


def _shaper_lut(tone: ToneCurve, size: ShaperSize, /) -> "colour.LUT1D":
    return colour.LUT1D(table=_TRANSFER[Transfer.DECODE](colour.LUT1D.linear_table(size), function=tone.value), name=f"rasm-{tone.value}")


def _baked(
    source: ConfigSource,
    src: SpaceRef,
    target: ChainTarget,
    look: str,
    fmt: LutFormat,
    shaper_space: Option[SpaceRef],
    path: OutPath,
    size: LutSize,
    shaper: ShaperSize,
    /,
) -> tuple[int, int, str, str]:
    baker = ocio.Baker()
    baker.setConfig(_config(source))
    baker.setInputSpace(str(src))
    baker.setCubeSize(size)
    baker.setFormat(fmt.value)
    match target:
        case (display, view):
            baker.setDisplayView(display, view)
        case dst:
            baker.setTargetSpace(str(dst))
            baker.setLooks(look)
    shaper_space.map(lambda space: (baker.setShaperSpace(str(space)), baker.setShaperSize(shaper)))
    Path(path).write_text(baker.bake())
    return size**3, Path(path).stat().st_size, str(src), fmt.value


def separations(document: CxfBytes, /) -> tuple[SpotChannel, ...]:
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
    "AlphaBand",
    "BitDepth",
    "BlackPoint",
    "BroadcastCurve",
    "BuiltinProfile",
    "ColorField",
    "ColorManaged",
    "ColorOperand",
    "ConfigSource",
    "ConnectionSpace",
    "Coverage",
    "CxfBytes",
    "Dpi",
    "GradeStep",
    "IccTransform",
    "LutBake",
    "LutFormat",
    "LutInterp",
    "LutSize",
    "ManageFault",
    "ManageOp",
    "ManagedRaster",
    "OcioDepth",
    "OcioRole",
    "OutPath",
    "PageIndex",
    "PdfBytes",
    "ProfileBytes",
    "ProfileRef",
    "RenderingIntent",
    "ShaperSize",
    "SpaceRef",
    "SpotChannel",
    "ToneCurve",
    "Transfer",
    "TransferKind",
    "lut_bytes",
    "separations",
]
```

## [03]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[OPEN|BLOCKED]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
