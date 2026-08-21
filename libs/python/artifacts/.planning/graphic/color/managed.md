# [PY_ARTIFACTS_GRAPHIC_COLOR_MANAGED]

Color-managed raster egress and the OCIO config plane — the downstream half of the color sub-domain, where `graphic/color/derive#DERIVE` is the upstream colorimetric source. `ColorManaged` is one behavior-dense frozen owner over the closed `ManageOp` family, and three transform engines answer three different questions on it: `PyOpenColorIO` resolves what a project's colorspaces MEAN and moves a scene-referred field between them, `pyvips` runs the device-to-device ICC egress at 8 and 16 bits under one `IccTransform` policy bundle, and `imagecodecs`'s lcms2 arms carry the float ICC edge pyvips's integer pipeline cannot reach. `ConfigSource` is the closed four-case config vocabulary every OCIO leg threads and the behavior-dense owner of the config graph — one memoized `Config` per source per worker, the processor acquired off it, the file-rules classification read from it — so no leg reads `GetCurrentConfig` and nothing below the composition root writes `SetCurrentConfig`. Spaces name `OcioRole` members wherever a role exists, so a config swap moves the meaning and no page re-spells a colorspace string.

Every produced blob, field, plate, and lattice lands at an `OutPath` and the receipt measures what landed. `Managed` writes the ICC-converted container through the `graphic/raster/io#IO` `CODEC` row's own libvips writer column, so one codec table serves the raster funnel and this egress and neither restates a suffix or an option builder; `Plane` closes the float ICC hole through `imagecodecs.cms_transform` with `cms_profile_validate` gating an untrusted blob before it reaches liblcms2; `Space` and `View` are the config-resolved colorspace and display-referred moves; `Export` folds the colour-science grade chain and writes at a `BitDepth`; `Plate`, `Lut`, `Swatch`, and `Separate` author separations, bake LUTs through OCIO's `Baker` or colour-science's `LUTSequence`, graduate CxF3 device declarations, and measure a finished PDF's per-ink coverage. Every arm crosses `self.lane.offload` as one runtime `Kernel` whose trait alone derives isolation and worker-death retry — a path-writing arm declares `idempotent=False`, so no retry replays an externally visible write — and every worker returns `Result[T, ManageFault]` the arm flattens once onto `RuntimeRail[ArtifactReceipt]`; the pre-run key mints synchronously through the bare `ContentIdentity.key` over `_canon`'s length-framed per-arm preimage, and every receipt threads that one key.

## [01]-[INDEX]

- [02]-[MANAGED]: `ColorManaged` owns color-managed egress and the OCIO config plane over the closed `ManageOp` family — `Managed`/`Plane` the two ICC legs split by depth engine, `Space`/`View` the config-resolved colorspace and display-view moves, `Export` the graded write, and the `Plate`/`Lut`/`Swatch`/`Separate` terminals minting `ArtifactReceipt.Color` into the settled `core/receipt#RECEIPT` cases under the closed `ManagedFact` vocabulary; `ConfigSource` carrying the memoized config graph, its processor acquisition, and the file-rules classification, every arm crossing `self.lane.offload` and threading one `RuntimeRail`.

## [02]-[MANAGED]

- Cases: `ManageOp` cases — `Managed(raster, path, src, dst, transform, codec, grade)` the `uint8`/`uint16` raster and its `GradeStep` chain crossing the `HOSTILE` process seam where the worker normalizes by dtype maximum, folds the chain, applies pyvips `icc_transform` under the `IccTransform` bundle, and writes the destination-profile-embedded container through the `raster/io#IO` `CODEC` libvips writer column — the one native-`libvips` process leg, its bare raster riding `Wire.SHARED_MEMORY`; `Plane(field, path, src, dst, transform, alpha)` the float ICC leg over `imagecodecs.cms_transform(outdtype=np.float32)` on a `RELEASING` thread, the depth pyvips refuses, its `AlphaBand` declaring which trailing band carries THROUGH the three-component transform instead of across it; `Space(field, path, config, src, dst, look)` and `View(field, path, config, src, display, view)` the two config-resolved moves — scene-referred colorspace-to-colorspace with an optional look, and display-referred through `DisplayViewTransform` — both `RELEASING`; `Export(field, path, depth, grade)` the grade fold and the bit-depth-correct `colour.write_image`; `Plate(document, path, channels, transform)` authoring one `/Separation` colorspace per spot and the joint `/DeviceN` over the pikepdf raw object model; `Lut(bake, path, size, shaper, intent)` baking the closed `LutBake` axis — a `GradeStep` chain through `colour.LUTSequence`, or a config-resolved space or display-view chain through `ocio.Baker`, which writes the CLF and CTF containers `colour.write_LUT` carries no method for; `Swatch(document)` graduating the CxF3 device half; `Separate(document, page, dpi, plates)` rendering the finished PDF's per-ink coverage plates through `pdf_oxide`, minting the MEASURED peak TAC and per-ink coverages and landing each plate as a 16-bit image when the egress is declared — matched by one total `match`. `_grade` folds the ordered chain (`cctf`/`broadcast`/`colourspace`/`correction`/`lut`/`managed`) — the shared module-level core every field arm reaches inside its offload worker, never duplicated per arm nor run on the loop.
- Auto: `ConfigSource` owns the config graph whole. `_config` memoizes one `Config` per source per worker process under `functools.cache`, so OCIO's own per-`Config` processor cache makes every later acquisition a hash lookup and the catalog's acquire-outside-the-fold law holds without a hand-rolled table; `identity` reads `Processor.getCacheID()` and `hasChannelCrosstalk()` before any pixel moves, `applied` compiles `getOptimizedCPUProcessor(ingress_depth, F32, OPTIMIZATION_DEFAULT)` so an 8- or 16-bit ingress normalizes INSIDE the compiled chain rather than through a caller-side divide, skips an `isNoOp()` chain whole, and applies through one `PackedImageDesc` pair over the flattened `(texels, channels)` view — a sub-RGB field crosses as a broadcast triple with its trailing channels carried through, legal exactly because a crosstalk chain over it already refused; `classified` answers `getColorSpaceFromFilepath` with `getCanonicalName`, so an ingest reads the config's declared file rules instead of forking a stem convention, and its caller consumes the VALUE. `Managed` and `Plane` fold `_grade` inside their worker via `Block.of_seq(grade).fold`, so decode-linearize, transfer, primary-convert, device-correct, LUT-grade, config-move, and re-encode interleave in one chain; the `managed` `GradeStep` seats the config graph beside the colour-science steps and the `lut` step reads every container `ocio.FileTransform` carries — CLF, CTF, CDL, ICC, and the cube family — where `colour.read_LUT` reaches six formats. `_icc_apply` resolves each `ProfileRef` through `named` (a `BuiltinProfile` passing its own engine column, raw `bytes` gated by `imagecodecs.cms_profile_validate` before the temp-file write on one `ExitStack`), runs `icc_transform`, reads back the embedded profile and the egress `interpretation`, runs the optional Pillow `_softproof` when a `proof` profile is set (the plain-vs-`GAMUTCHECK` diff counting out-of-press-gamut pixels — the lcms2 signal pyvips lacks), reads the peak Total Area Coverage off a CMYK egress, and returns eight scalars rather than the encoded buffer, so the process seam carries evidence and never the megabyte product it already wrote. Every worker returns `Result[T, ManageFault]`: one boundary arm per worker maps its provider family — `ocio.Exception`/`ocio.ExceptionMissingFile` onto `<ocio-space>`/`<ocio-config>`, `imagecodecs.CmsError` onto `<icc-profile>` — and `_lifted` folds the typed fault onto the `MANAGED_REFUSED` row exactly once where the arm flattens the nested rail, so no OCIO or lcms2 raise reaches the interior and no arm reconstructs an exception.
- Growth: a new managed operation is one `ManageOp` case, one total dispatch arm on `_produced`, one `_canon` preimage arm, and one receipt projection — the durable seat is SHARED, `_emit` awaiting `Journal.record` over `receipt.evidence()` once above the whole fan, so a new arm inherits the `OPERATIONAL` fact and its `STORAGE` charge rather than minting a second seat; a new grade step is one `GradeStep` case folded by `_grade` and one `_step` preimage arm; a new LUT bake modality is one `LutBake` case and one `_lut_author` arm; a new config resolution is one `ConfigSource` case and one `_config` arm; a new output container is one `ConvertFormat` member on its owning `raster/io#IO` row, with zero rows here; a new broadcast curve is one `BroadcastCurve` member and its `_BROADCAST_ROSTER` memberships, admission proving the kind-curve pairing the colour registries admit; a new role, LUT format, interpolation, OCIO depth, built-in profile, curve, intent, PCS, depth, alpha posture, or black-point posture is one member in its closed vocabulary, a profile member filling the engine columns its rosters carry and earning no seat where a name builds a profile yet no transform; a new evidence scalar is one `ManagedFact` row; a new refusal is one `ManageFault` member breaking every capture at type-check. New boundary invariants refine on the existing `ManagedRaster`, `ColorField`, `ColorOperand`, `CorrectionMatrix`, `ProfileBytes`, `PdfBytes`, `CxfBytes`, `OutPath`, `PageIndex`, `Dpi`, `LutSize`, `ShaperSize`, or `Coverage` admission axis.
- Boundary: colorimetry, appearance models, spectral computation, gamut mapping, palettes, and the CxF color half are `graphic/color/derive#DERIVE`'s, and `ColorModel` is that page's vocabulary composed here — this page mints no colour-model enum of its own. Display-container codec facts are `graphic/raster/io#IO`'s `CODEC` rows, composed by import rather than restated. Deep-pixel plane storage, mip ladders, and KTX2 containers are `graphic/texture`'s; a plane converts through `ConfigSource.applied` at its caller BEFORE it enters that estate, and no texture page imports this owner. Process-wide OCIO state — `SetCurrentConfig`, `SetLoggingLevel`, `SetEnvVariable`, `ClearAllCaches` — is the composition root's, so `GetCurrentConfig` gets no reader here and `GetVersion()` rides the startup census, never a per-call fact.
- Packages: `opencolorio` (the config-driven transform graph — `Config.CreateFromBuiltinConfig`/`CreateFromEnv`/`CreateFromFile`/`CreateRaw`, `getProcessor` over a name pair, a `LookTransform`, a `DisplayViewTransform`, or a `GroupTransform` of `FileTransform`s, `getOptimizedCPUProcessor`, `PackedImageDesc`, `isNoOp`/`hasChannelCrosstalk`/`getCacheID`, `getColorSpaceFromFilepath`/`getCanonicalName`, and `Baker`), `pyvips` (`icc_transform` device egress and `write_to_buffer` under `ForeignKeep.ICC`), `imagecodecs` (the lcms2 `cms_profile`/`cms_profile_validate`/`cms_transform` float ICC edge), `colour-science` (the CCTF, broadcast-transfer, RGB-colourspace, matrix-correction, image-write, and `LUT1D`/`LUT3D`/`LUTSequence` surfaces), `pillow` (`ImageCms.buildProofTransform` soft proofing alone), `pikepdf` (the `/Separation` and `/DeviceN` raw object model), `pdf_oxide` (`render_separations`), `colour-cxf` (`read_cxf` device half), with `expression`/`numpy`/`beartype` and the runtime `Journal`/`LanePolicy`/`Kernel`/`KernelTrait`/`Wire`; the full member surface lives in the package `.api` catalogs.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
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

from rasm.artifacts.core.hooks import ArtifactsLeg
from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.core.receipt import ArtifactReceipt
from rasm.artifacts.graphic.color.derive import AdaptMethod, ColorModel
from rasm.artifacts.graphic.raster.io import CODEC, CodecEmit, CodecPolicy, RasterEngine, writer
from rasm.artifacts.graphic.raster.process import ConvertFormat
from rasm.runtime.faults import TERMINAL, BoundaryFault, FaultRow, RuntimeRail, rostered
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.journal import Journal
from rasm.runtime.lanes import LanePolicy
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
    # ONE float field type across every arm: `float32` is the deep-pixel plane a texture caller converts before it
    # enters that estate, `float64` the document and measurement field, and the channel band spans a single coverage
    # plate through RGBA — a second array alias per depth or per channel count is the fork this admits away.
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
type ColorOperand = ManagedRaster | ColorField  # the config legs take either: the operand's own dtype IS the processor's ingress bit depth
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
    # Roles carry the config's OWN names as values, because a role name IS what `getProcessor` consumes and the
    # provider constants (`ocio.ROLE_SCENE_LINEAR`, …) carry exactly these strings. Dereferencing those constants at
    # module scope would reify the `lazy import` proxy at import and crash a host with no OCIO build, so the owned
    # mirror is the form: naming a role survives a config swap where a colorspace string binds to one config.
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
    # Mirrors `PyOpenColorIO.BitDepth` MEMBER NAMES, resolved at the call seam through `getattr` — the member objects
    # themselves cannot ride a module-level row without reifying the lazy proxy. Bit depth is a PROCESSOR property,
    # so the ingress row here is what `getOptimizedCPUProcessor` compiles the normalization into.
    UINT8 = "BIT_DEPTH_UINT8"
    UINT16 = "BIT_DEPTH_UINT16"
    F16 = "BIT_DEPTH_F16"
    F32 = "BIT_DEPTH_F32"


class LutInterp(StrEnum):  # mirrors `PyOpenColorIO.Interpolation` member names; tetrahedral is the 3-D default every grading tool assumes
    NEAREST = "INTERP_NEAREST"
    LINEAR = "INTERP_LINEAR"
    TETRAHEDRAL = "INTERP_TETRAHEDRAL"
    CUBIC = "INTERP_CUBIC"
    BEST = "INTERP_BEST"


class LutFormat(StrEnum):
    # Rows mirror the full `Baker.getFormats()` roster: an owned mirror admits every provider member, because a
    # dropped row raises on the interior `LutFormat(value)` reconstruction the receipt round-trip performs. CLF and
    # CTF are the two the estate cannot otherwise write — `colour.write_LUT` registers six methods, neither among them.
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


class ManagedFact(StrEnum):
    INTENT = "intent"
    BLACK_POINT = "black_point"
    PCS = "pcs"
    DEPTH = "depth"
    CODEC = "codec"
    BANDS = "bands"
    CHANNELS = "channels"
    ALPHA = "alpha"
    TEXELS = "texels"
    EMBEDDED = "embedded"
    BUFFER_BYTES = "buffer_bytes"
    GRADE = "grade"
    SPACE = "space"
    GAMUT = "gamut"
    SPOTS = "spots"
    INK = "ink"
    PAGES = "pages"
    SIZE = "size"
    SHAPER = "shaper"
    FORMAT = "format"
    ENTRIES = "entries"
    INKS = "inks"
    PLATES = "plates"
    DPI = "dpi"
    CONFIG = "config"
    CACHE_ID = "cache_id"
    SRC_SPACE = "src_space"
    DST_SPACE = "dst_space"
    DISPLAY = "display"
    VIEW = "view"
    LOOK = "look"
    CROSSTALK = "crosstalk"


class AlphaBand(StrEnum):
    # WHETHER the trailing channel is alpha, declared because the field cannot answer it: a four-band float plane is
    # RGB-plus-alpha or CMYK ink and the array is identical either way. Alpha is not colour and never crosses an ICC
    # transform, so the declaration is what splits it off and rejoins it rather than a guess at the seam.
    NONE = "none"
    TRAILING = "trailing"

    @property
    def colour_bands(self) -> int:
        return 1 if self is AlphaBand.TRAILING else 0


class ProfileNames(NamedTuple):
    # Two ICC engines ship two different built-in rosters: pyvips `icc_transform` takes a libvips device NAME, and
    # `imagecodecs.cms_profile` takes an lcms2 built-in name. A member reaches a leg only where its column is filled,
    # so `Plane` refuses a P3 or CMYK built-in on `<profile-engine>` instead of raising inside liblcms2, and a name
    # whose profile constructs yet builds no transform (`rgb`, `gray`, `null` on this lcms2) earns no member at all.
    vips: str | None
    cms: str | None


class BuiltinProfile(ProfileNames, Enum):
    SRGB = ProfileNames("srgb", "srgb")
    P3 = ProfileNames("p3", None)
    CMYK = ProfileNames("cmyk", None)
    ADOBE_RGB = ProfileNames(None, "adobergb")
    XYZ = ProfileNames(None, "xyz")


type ProfileRef = ProfileBytes | BuiltinProfile
type SpaceRef = OcioRole | str  # a role name resolves as a colorspace name on every config surface that takes one
type ChainTarget = SpaceRef | tuple[str, str]  # a name is the colorspace move, a (display, view) pair the display-referred one


# --- [MODELS] ---------------------------------------------------------------------------
@tagged_union(frozen=True)
class ConfigSource:
    # Four closed cases resolve a config, and this owner holds the graph they resolve to behavior-dense: one memoized
    # `Config` per source per worker, the processor acquired off it, the transform identity read off that processor,
    # and the file-rules classification answered from it. `GetCurrentConfig` gets no reader and `SetCurrentConfig` no
    # writer — process-wide config state is the composition root's, so every leg threads the source it declares.
    tag: Literal["builtin", "env", "file", "raw"] = tag()
    builtin: str = case()  # an `ocio://` name; the shipped ACES CG and Studio set resolves with no file on disk
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
        # Read BEFORE any pixel moves: `getCacheID()` is the transform's version on the receipt, a 32-hex digest or
        # else the processor's own no-op marker for an identity chain. `hasChannelCrosstalk()` decides whether a
        # sub-RGB operand may be transformed at all — a crosstalk chain over one channel computes nothing meaningful.
        processor = _processor(self, src, target, look)
        return processor.getCacheID(), processor.hasChannelCrosstalk()

    def applied(self, field: ColorOperand, src: SpaceRef, target: ChainTarget, look: str = "", /) -> ColorField:
        # This entry IS the in-memory half of the move the `Space`/`View` arms land, so a texture caller converting a plane
        # into the `scene_linear` role before publication and the receipted egress cannot disagree.
        return _transformed(_processor(self, src, target, look), field)

    def classified(self, path: OutPath, /) -> tuple[str, int]:
        # `FileRules` answers from the config's own declaration — canonical colorspace name with the matched rule index as
        # provenance. An ingest inferring a space from a stem convention of its own forks the project's rules, so the
        # texture and ingest planes consume this VALUE through their caller and import no symbol from this page.
        config = _config(self)
        name, rule = config.getColorSpaceFromFilepath(path)
        return config.getCanonicalName(name), rule


_BUILTIN: Final[ConfigSource] = ConfigSource(builtin="ocio://default")  # the registry's own recommended row, so the pin tracks the distribution
_RAW: Final[ConfigSource] = ConfigSource(raw=None)  # the minimal single-data-space config a file-LUT chain compiles against, naming no colorspace


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
        # `ColorModel.rgb` is the `colour.RGB_COLOURSPACES` key: an appearance or luma-chroma model names no primaries
        # and no cctf, so it can never be a `RGB_to_RGB` end and the pair refuses here rather than KeyErroring in the fold.
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
    # WHAT the lattice samples, closed: a colour-science grade chain, a config-resolved colorspace move, or a
    # config-resolved display view. The engine follows the case — `colour.write_LUT` registers six container methods
    # and `ocio.Baker` twelve, so the CLF and CTF the estate's own `GradeStep.Lut` reads are only writable on the
    # config legs, and a `graded` bake keeps the `_grade` law so the authored file and the in-memory table agree.
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
    # One policy value carries the whole ICC posture: intent, black point, connection space, egress depth, the
    # encoder coordinates, the lcms2 proof profile, and the spot declarations. The encoder coordinates ride
    # `raster/io#IO`'s OWN `CodecPolicy` rather than a local `quality`/`effort` pair, because that owner already
    # defines them, bounds them, and derives `rate` from them — two owners for one pair meant this page defaulted
    # `(92, 6)` while the raster page defaulted `(80, 4)`, so the same container encoded differently depending on
    # which surface composed it, and a third coordinate would have had to land twice.
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
            # pyvips `icc_transform` admits 8/16-bit alone — a FLOAT32 request refuses HERE so produced pixels and
            # receipt depth never disagree; float ICC egress is the `Plane` arm's own lcms2 capability.
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
        # lcms2 builds a three-component transform and SILENTLY drops every band past the third, so the colour band
        # count is proved HERE — a one- or two-channel plane has no three-component form and a four-band plane is
        # admitted only as RGB plus a declared alpha, never as CMYK ink this leg's built-in roster cannot address.
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
        # `colour.write_LUT` keys its container off the path suffix and registers five of them, so a graded bake into
        # a CLF or CTF path refuses HERE rather than raising a bare `KeyError` mid-write; a config bake names its
        # container as a `LutFormat` value the `Baker` resolves, so the suffix decides nothing on that leg.
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

# this page's ONE raise anchor. Every refusal this folder mints is caller-repairable — a colorspace the config does
# not name, a container the linked build cannot write, a malformed ICC blob — so the row is TERMINAL and the closed
# `ManageFault` token rides as its one NAMED coordinate.
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
# each colour OETFS/EOTFS/OOTFS registry admits a distinct curve set; Broadcast proves the pairing so no worker KeyErrors
_BROADCAST_ROSTER: Final[frozendict[TransferKind, frozenset[BroadcastCurve]]] = frozendict({
    TransferKind.OETF: frozenset({BroadcastCurve.BT709, BroadcastCurve.BT2100_PQ, BroadcastCurve.BT2100_HLG}),
    TransferKind.EOTF: frozenset({BroadcastCurve.BT1886, BroadcastCurve.BT2100_PQ, BroadcastCurve.BT2100_HLG}),
    TransferKind.OOTF: frozenset({BroadcastCurve.BT2100_PQ, BroadcastCurve.BT2100_HLG}),
})
# This table is the pyvips leg's OWN: `icc_transform` takes a bit count and admits 8/16 alone, so the `Managed` admission
# refuses FLOAT32 and the config legs never read this row — a processor compiles their ingress depth instead.
_DEPTH_BITS: Final[frozendict[BitDepth, int]] = frozendict({BitDepth.UINT8: 8, BitDepth.UINT16: 16})
_INGRESS_DEPTH: Final[frozendict[str, OcioDepth]] = frozendict({
    # numpy dtype name to the processor's ingress depth; `float64` has no OCIO row, so it crosses cast to F32
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
    # ONE row set serves both lcms2 faces the page reaches — `ImageCms.Intent` and `imagecodecs.CMS.INTENT` spell the
    # same four ICC intents, so the member NAMES resolve at each call seam and neither engine gets a table of its own;
    # `AUTO` is a pyvips-only posture with no lcms2 member, so it folds onto the perceptual row the spec defaults to.
    RenderingIntent.PERCEPTUAL: "PERCEPTUAL",
    RenderingIntent.RELATIVE: "RELATIVE_COLORIMETRIC",
    RenderingIntent.SATURATION: "SATURATION",
    RenderingIntent.ABSOLUTE: "ABSOLUTE_COLORIMETRIC",
    RenderingIntent.AUTO: "PERCEPTUAL",
})
# Coverage plates are lossless single-channel tint fields, and 16-bit PNG is the container every prepress reader takes
_PLATE_DEPTH: Final[str] = BitDepth.UINT16.value
# `colour.io.LUT_WRITE_METHODS` resolves these suffixes; every other container the estate authors is a `Baker` format,
# and the two rosters together are why the LUT terminal carries two engines rather than one with an unreachable half
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
        # PRE-RUN key over the op's length-framed canonical preimage through the bare synchronous mint; the railed
        # `ContentIdentity.of` Struct encode is the rejected form — `ManageOp` is a tagged union, not a wire Struct.
        return ContentIdentity.key(f"color-managed-{self.op.tag}", _canon(self.op))

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        # ONE durable seat over the whole op fan, seated ABOVE `_produced` rather than inside its arms: every arm
        # settles onto the same two cases, so a per-arm record is one fold written nine times and the tenth arm
        # would land without one. `OPERATIONAL` and the `STORAGE` charge derive from each case's own rows, and the
        # `ManagedFact` band never reaches the diff — its leaf set is this producer's own instrumentation, and an
        # audit row whose width tracks it compares nothing across two runs. Recording suspends on the journal's
        # bounded intake, so the seat is this awaitable fold and `contribute` stays the synchronous projection.
        match await self._produced():
            case Result(tag="ok", ok=receipt):
                return (await Journal.record(receipt.evidence())).map(lambda _landed: receipt)
            case refused:
                return Error(refused.error)

    async def _produced(self) -> RuntimeRail[ArtifactReceipt]:
        match self.op:
            case ManageOp(tag="managed", managed=(raster, path, src_profile, dst_profile, transform, codec, grade)):
                crossed = await self.lane.offload(
                    # bare rasters ride the span channel at zero payload bytes
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
                return self._railed(crossed, lambda produced: self._previewed(produced, transform, codec, grade))
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
                return self._railed(crossed, lambda produced: self._profiled(produced, transform, alpha))
            case ManageOp(tag="space", space=(field, path, config, src, dst, look)):
                crossed = await self.lane.offload(Kernel.of(_ocio_apply, KernelTrait.RELEASING, idempotent=False), field, path, config, src, dst, look)
                return self._railed(crossed, lambda produced: self._spaced(produced, config, src, dst, look))
            case ManageOp(tag="view", view=(field, path, config, src, display, view)):
                crossed = await self.lane.offload(
                    Kernel.of(_ocio_apply, KernelTrait.RELEASING, idempotent=False), field, path, config, src, (display, view), ""
                )
                return self._railed(crossed, lambda produced: self._spaced(produced, config, src, (display, view), ""))
            case ManageOp(tag="export", export=(field, path, depth, grade)):
                crossed = await self.lane.offload(Kernel.of(_export_image, KernelTrait.RELEASING, idempotent=False), field, path, depth.value, grade)
                return self._railed(crossed, lambda produced: self._exported(produced, depth, grade))
            case ManageOp(tag="plate", plate=(document, path, channels, transform)):
                crossed = await self.lane.offload(Kernel.of(_plate_author, KernelTrait.RELEASING, idempotent=False), document, path, channels)
                return crossed.map(lambda plated: self._plated(plated, channels, transform))
            case ManageOp(tag="lut", lut=(bake, path, size, shaper, intent)):
                crossed = await self.lane.offload(Kernel.of(_lut_author, KernelTrait.RELEASING, idempotent=False), bake, path, size, shaper)
                return self._railed(crossed, lambda produced: self._lutted(bake, intent, size, shaper, produced))
            case ManageOp(tag="separate", separate=(document, page, dpi, plates)):
                crossed = await self.lane.offload(
                    Kernel.of(_separate, KernelTrait.RELEASING, idempotent=plates.is_none()), document, page, dpi, plates
                )
                return crossed.map(lambda measured: self._separated(measured, dpi, len(document)))
            case ManageOp(tag="swatch", swatch=document):
                crossed = await self.lane.offload(Kernel.of(separations, KernelTrait.RELEASING), document)
                return crossed.map(lambda channels: self._swatched(channels, len(document)))
            case _:
                assert_never(self.op)

    # --- [PROJECTIONS] ------------------------------------------------------------------
    def _railed[T](self, crossed: RuntimeRail[Result[T, ManageFault]], project: Callable[[T], ArtifactReceipt], /) -> RuntimeRail[ArtifactReceipt]:
        # ONE flatten for the worker's nested carrier: the outer rail carries the boundary classification the runtime
        # owns, the inner one this folder's typed refusal, and `_lifted` folds the second onto the first exactly here.
        return crossed.bind(lambda produced: produced.map_error(_lifted).map(project))

    def _previewed(
        self, produced: tuple[int, int, int, int, bool, str, int, float], transform: IccTransform, codec: ConvertFormat, grade: tuple[GradeStep, ...]
    ) -> ArtifactReceipt:
        bytes_, width, height, bands, embedded, space, gamut, ink = produced
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
            ManagedFact.SPOTS.value: float(len(transform.separations)),
            ManagedFact.INK.value: ink,
        })
        return ArtifactReceipt.Preview(self._key, width, height, bytes_, scores)

    def _profiled(self, produced: tuple[int, int, int, int], transform: IccTransform, alpha: AlphaBand) -> ArtifactReceipt:
        bytes_, width, height, channels = produced
        scores: frozendict[str, float | str] = frozendict({
            ManagedFact.INTENT.value: transform.intent.value,
            ManagedFact.BLACK_POINT.value: float(transform.black_point.enabled),
            ManagedFact.DEPTH.value: BitDepth.FLOAT32.value,
            ManagedFact.CHANNELS.value: float(channels),
            ManagedFact.ALPHA.value: alpha.value,
            ManagedFact.TEXELS.value: float(width * height),
        })
        return ArtifactReceipt.Preview(self._key, width, height, bytes_, scores)

    def _spaced(
        self, produced: tuple[int, int, int, int, str, bool], config: ConfigSource, src: SpaceRef, target: ChainTarget, look: str
    ) -> ArtifactReceipt:
        bytes_, width, height, channels, cache_id, crosstalk = produced
        display, view = target if isinstance(target, tuple) else ("", "")
        scores: frozendict[str, float | str] = frozendict({
            ManagedFact.CONFIG.value: config.label,
            ManagedFact.CACHE_ID.value: cache_id,
            ManagedFact.SRC_SPACE.value: str(src),
            ManagedFact.DST_SPACE.value: "" if isinstance(target, tuple) else str(target),
            ManagedFact.DISPLAY.value: display,
            ManagedFact.VIEW.value: view,
            ManagedFact.LOOK.value: look,
            ManagedFact.CROSSTALK.value: float(crosstalk),
            ManagedFact.CHANNELS.value: float(channels),
            ManagedFact.TEXELS.value: float(width * height),
        })
        return ArtifactReceipt.Preview(self._key, width, height, bytes_, scores)

    def _exported(self, produced: tuple[ColorField, int], depth: BitDepth, grade: tuple[GradeStep, ...]) -> ArtifactReceipt:
        toned, bytes_ = produced
        height, width = toned.shape[0], toned.shape[1]
        scores: frozendict[str, float | str] = frozendict({
            ManagedFact.DEPTH.value: depth.value,
            ManagedFact.BUFFER_BYTES.value: float(toned.nbytes),
            ManagedFact.GRADE.value: float(len(grade)),
        })
        return ArtifactReceipt.Preview(self._key, width, height, bytes_, scores)

    def _plated(self, plated: tuple[int, int], channels: tuple[SpotChannel, ...], transform: IccTransform) -> ArtifactReceipt:
        bytes_, pages = plated
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
            bytes_,
            facts,
        )

    def _lutted(self, bake: LutBake, intent: RenderingIntent, size: int, shaper: int, produced: tuple[int, int, str, str]) -> ArtifactReceipt:
        entries, bytes_, space, fmt = produced
        return ArtifactReceipt.Color(
            self._key,
            space,
            intent.value,
            0.0,
            0,
            bytes_,
            frozendict({
                ManagedFact.GRADE.value: float(len(bake.graded[1]) if bake.tag == "graded" else 0),
                ManagedFact.SIZE.value: float(size),
                ManagedFact.SHAPER.value: float(shaper),
                ManagedFact.FORMAT.value: fmt,
                ManagedFact.ENTRIES.value: float(entries),
            }),
        )

    def _separated(self, measured: tuple[float, tuple[tuple[str, float], ...], Option[int]], dpi: int, bytes_: int) -> ArtifactReceipt:
        tac_peak, coverages, plates = measured
        facts: frozendict[str, float | str] = frozendict({
            ManagedFact.INKS.value: float(len(coverages)),
            ManagedFact.DPI.value: float(dpi),
            # an undeclared plate egress omits the row: absence of a measurement is not a zero byte count
            **plates.map(lambda written: {ManagedFact.PLATES.value: float(written)}).default_value({}),
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
def _lifted(fault: ManageFault, /) -> BoundaryFault:
    # Every refusal this folder mints is caller-repairable — a colorspace the config does not name, a container the
    # linked build cannot write, a malformed ICC blob — so the whole vocabulary folds onto the construction case
    return MANAGED_REFUSED.raised(fault)


def _framed(*chunks: bytes) -> tuple[bytes, ...]:
    # patterns row [05]: count-frame the tuple and length-frame every chunk so adjacent variable-width fields never re-split
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
    # Rows serve this leg only through their NATIVE libvips encoder: the imagecodecs ARRAY writer beside it takes an 8-bit
    # `Frame`, never this pipeline's 16-bit or CMYK egress, so a container libvips writes only through that leg
    # refuses at admission on the row's own tag rather than quantizing a device conversion at the last hop
    # `writers` maps an engine to a TUPLE of emitters — the preference run a missing provider falls through — so a
    # match against a bare `CodecEmit` head never fired and this predicate answered False for every container,
    # refusing every codec at `ManageOp.Managed` admission and killing the whole managed egress.
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
    # ONE `Config` per source per worker process. OCIO's `Processor` cache lives on the `Config`, so memoizing the
    # source is what makes every later acquisition a hash lookup and satisfies the acquire-outside-the-fold law with
    # no table of the owner's own; `functools.cache` keys on the frozen source, whose four cases are hashable whole.
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
    # One descriptor shape carries the flattened (texels, channels) view, so every operand rank folds onto one call; the
    # explicit channel/x/y stride triple is mandatory the moment a bit depth is named on the constructor
    return ocio.PackedImageDesc(buf, buf.shape[0], 1, buf.shape[1], depth, buf.itemsize, buf.strides[0], buf.strides[0] * buf.shape[0])


def _processor(source: ConfigSource, src: SpaceRef, target: ChainTarget, look: str, /) -> "ocio.Processor":
    # ONE acquisition surface over the three chain shapes the config resolves; `target` discriminates structurally, so
    # no mode flag re-states what the value already carries and a display-referred chain never loses its name pair.
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
    # Every config leg reaches this ONE apply core — the two `ManageOp` arms, the `managed` grade step, and the
    # file-LUT step alike. Bit depth is a PROCESSOR property, so `getOptimizedCPUProcessor` compiles the ingress
    # normalization into the chain and an 8- or 16-bit operand never pays a caller-side divide; the source buffer is
    # never the destination, so the in-place `applyRGB` mutation contract cannot reach a caller's array.
    # Exemption: a measured native kernel — the descriptor pair, the identity short-circuit, and the destination
    # allocation are the platform-forced statement seam pybind11 admits no expression form for.
    depth = _INGRESS_DEPTH.get(str(field.dtype), OcioDepth.F32)  # float64 carries no OCIO row, so it crosses cast to F32
    cpu = processor.getOptimizedCPUProcessor(_depth(depth), _depth(OcioDepth.F32), ocio.OptimizationFlags.OPTIMIZATION_DEFAULT)
    if cpu.isNoOp():
        return np.ascontiguousarray(field, dtype=np.float32)
    channels = field.shape[-1]
    flat = np.ascontiguousarray(field, dtype=_CARRIER[depth]).reshape(-1, channels)
    # `PackedImageDesc` refuses fewer than three channels outright, so a gray or gray+alpha operand crosses as a
    # broadcast triple with its trailing channels carried through untouched — legal exactly where the crosstalk gate
    # already passed, because a chain with no crosstalk answers every replicated channel identically.
    wide = flat if channels >= 3 else np.ascontiguousarray(np.repeat(flat[:, :1], 3, axis=1))
    egress = np.empty(wide.shape, dtype=np.float32)
    cpu.apply(_desc(wide, _depth(depth)), _desc(egress, _depth(OcioDepth.F32)))
    landed = egress if channels >= 3 else np.concatenate([egress[:, :1], flat[:, 1:].astype(np.float32)], axis=1)
    return landed.reshape(*field.shape[:-1], channels)


def _guarded[T](thunk: Callable[[], Result[T, ManageFault]], /) -> Result[T, ManageFault]:
    # One adapter bounds every provider family this page's transform legs raise: `ExceptionMissingFile` and
    # `Exception` are the whole OCIO surface (sibling classes, so two arms discriminate an unresolvable config or LUT
    # reference from every naming, validation, and compilation refusal) and `CmsError` the whole lcms2 one. An
    # unlisted raise propagates as the defect it is and the runtime classifies it; nothing else reaches the interior.
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
                # ONE LUT reader for every container the estate writes: `ocio.FileTransform` carries CLF, CTF, CDL,
                # ICC, and the whole cube/3dl/spi/csp family, a strict superset of `colour.read_LUT`'s six registered
                # methods — so a `.clf` this page's own `Baker` leg authored reads back, which no colour reader does.
                # File LUTs name no colorspace, so the minimal raw config compiles them: the chain IS the whole graph.
                group = ocio.GroupTransform([ocio.FileTransform(src=path, interpolation=getattr(ocio.Interpolation, interp.value)) for path in paths])
                return _transformed(_config(_RAW).getProcessor(group, ocio.TransformDirection.TRANSFORM_DIR_FORWARD), acc)
            case GradeStep(tag="managed", managed=(source, src, target)):
                return source.applied(acc, src, target)
            case _ as unreachable:
                assert_never(unreachable)

    return Block.of_seq(steps).fold(applied, field)


def _written(toned: ColorField, path: OutPath, depth: str, /) -> tuple[ColorField, int]:
    # Every field arm lands through this one egress, so the produced artifact and the measured byte count are one act
    colour.write_image(toned, path, depth)
    return toned, Path(path).stat().st_size


def _export_image(field: ColorField, path: OutPath, depth: str, grade: tuple[GradeStep, ...]) -> Result[tuple[ColorField, int], ManageFault]:
    # Chains carry a `managed` or `lut` step, so the graded write rides the same boundary adapter the config legs do
    return _guarded(lambda: Ok(_written(_grade(field, grade), path, depth)))


def _ocio_apply(
    field: ColorOperand, path: OutPath, source: ConfigSource, src: SpaceRef, target: ChainTarget, look: str
) -> Result[tuple[int, int, int, int, str, bool], ManageFault]:
    # Config-resolved moves read identity first so a refusal costs no pixels, then apply once, then write.
    # Egress depth is the processor's own contract rather than a knob — every compiled chain lands F32 — so the
    # scene-referred product is written at the depth the transform actually produced.
    def resolved() -> Result[tuple[int, int, int, int, str, bool], ManageFault]:
        cache_id, crosstalk = source.identity(src, target, look)
        return (
            Error("<channel-crosstalk>")  # a crosstalk chain reads channels the operand does not carry
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
    # lcms2 carries the float ICC leg pyvips cannot: `icc_transform` admits 8/16-bit alone, while `cms_transform`
    # retypes and transforms in one call at `outdtype=np.float32`, so a scene-linear or deeper-than-16-bit plane
    # crosses a device profile without the quantization the integer pipeline forces. Profiles cross as BLOBS — a
    # name string raises — and each validates before liblcms2 opens it. Alpha carries THROUGH, never across: the
    # transform emits three components and drops every further band without a word, so the DECLARED alpha splits
    # off ahead of the call and rejoins after. The split reads the declaration rather than the band count, because
    # a four-band float plane is RGB-plus-alpha or CMYK ink and the array is identical either way.
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
    # Built-ins resolve through lcms2's own roster; a caller-supplied blob is untrusted material, so the header
    # validates here and raises `CmsError` the one boundary adapter maps, never mid-transform inside liblcms2
    resolved = imagecodecs.cms_profile(ref.cms) if isinstance(ref, BuiltinProfile) else ref
    imagecodecs.cms_profile_validate(resolved)
    return resolved


def _softproof(rgb8: NDArray[np.uint8], reference: str | ImageCms.ImageCmsProfile, proof_path: str, intent: RenderingIntent) -> int:
    # simulate the press/proof profile (buildProofTransform, reference as input+display, proof the press); the plain-vs-GAMUTCHECK
    # output diff marks the out-of-press-gamut pixels — the lcms2 PDF/X preflight signal pyvips has no member for.
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
    # Read-side prepress audit worker: pdf_oxide renders one grayscale coverage plate per page ink (pixel intensity ==
    # tint %), so per-ink mean coverage and the true per-pixel ink-sum PEAK TAC are MEASURED off the finished PDF,
    # never re-derived by hand. A declared egress lands each plate as its own single-channel field through the same
    # write leg every other field arm uses, so the audit that renders the separations can also emit them; an undeclared
    # egress carries `Nothing`, which is the absence of a measurement rather than a zero byte count.
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
        # Built-ins pass their own libvips device name with no file at all; a caller-supplied blob is untrusted
        # material crossing into liblcms2, so `_blob` validates the ICC header BEFORE the temp file exists and the
        # refusal lands on the typed rail rather than mid-write inside the transform
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
        with ExitStack() as stack:  # the profile temp files must outlive the lazy icc_transform until write_to_buffer pulls pixels
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
            # Total Area Coverage — the peak (C+M+Y+K) ink sum over the converted CMYK field, the ISO 12647 / PDF-X-4 ink-limit
            # preflight paired with gamut. cmyk guarantees 4 bands, so `maxpos()[0]` over the band sum normalizes
            # against the depth ceiling, and a non-cmyk egress reads 0.0.
            ink = float((managed[0] + managed[1] + managed[2] + managed[3]).maxpos()[0]) / float((1 << depth) - 1) * 100.0 if space == "cmyk" else 0.0
            # Containers land HERE and only their length crosses the process seam, so the product is durable and the
            # pickled payload stays eight scalars instead of the megabytes the encode just produced
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
    # This half of the codec gate PROBES: `_vips_native` reads the row's tag alone so admission stays provider-free on
    # its loop, where this one runs the column's own memoized trial write, which reads the LINKED build rather than a
    # registry membership — an unbuilt encoder refuses at the capability gate before the lazy pipeline pays a decode.
    # Suffix and option builder both belong to the row, so no container spelling or quality literal lands here.
    # `_writer` is the raster owner's OWN preference fold — it walks the engine's ordered emitter run and takes the
    # first whose build probe passes — so this leg composes it instead of re-deriving a walk that a tuple-shaped
    # `writers` column had already broken here: matching the tuple against a bare `CodecEmit` head never fired, and
    # every managed egress returned `<codec-writer>`. The option builder takes the WHOLE `CodecPolicy`, so the two
    # coordinates cross as the one owner rather than as a positional pair the row would have to re-assemble.
    match writer(codec, RasterEngine.LIBVIPS):
        case Ok(CodecEmit(tag="native", native=(suffix, _probe, options))):
            return Ok((suffix, options(codec_policy)))
        case _:
            return Error("<codec-writer>")


def _plate_author(document: PdfBytes, path: OutPath, channels: tuple[SpotChannel, ...]) -> tuple[int, int]:
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
        pdf.save(path)
        return Path(path).stat().st_size, len(pdf.pages)


def lut_bytes(space: ColorModel, grade: tuple[GradeStep, ...], size: LutSize, /) -> bytes:
    # in-memory half of the SAME bake: the raw float32 N³x3 table `graphic/raster/process#PROCESS`
    # `Transform.LUT_3D` decodes — one `_grade` law feeds the `.cube`/`.csp` container AND the raster consumer,
    # so the authored file and the in-memory wire cannot disagree.
    return _lattice(grade, size).astype(np.float32).reshape(-1, 3).tobytes()


def _lattice(grade: tuple[GradeStep, ...], size: LutSize, /) -> ColorField:
    axis = np.linspace(0.0, 1.0, size)
    r, g, b = np.meshgrid(axis, axis, axis, indexing="ij")
    return _grade(np.stack([r, g, b], axis=-1), grade)


def _lut_author(bake: LutBake, path: OutPath, size: LutSize, shaper: ShaperSize) -> Result[tuple[int, int, str, str], ManageFault]:
    # Two engines, one terminal: a grade chain bakes through colour-science's LUT family into the six containers
    # `write_LUT` registers, and a config-resolved chain bakes through `ocio.Baker` into all twelve — CLF and CTF
    # included, which the estate's own `GradeStep.Lut` reads back and no colour writer authors. Shaping makes a log
    # or PQ input tractable: a 1-D pre-curve linearizes the domain so the cube samples it uniformly instead of
    # brute-forcing a lattice large enough to resolve the toe.
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
    # CLF names a 1-D pre-curve into a 3-D cube as its canonical shape: linearizing the domain lets the cube sample it
    # uniformly, where a log or PQ input otherwise needs a lattice large enough to resolve its own toe
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
    # Exemption: `Baker` is a mutable provider builder with no value constructor, so the chain is a setter sequence;
    # `target` discriminates structurally exactly as `_processor` does, so the two config surfaces read one law.
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
    "ManagedFact",
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
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
