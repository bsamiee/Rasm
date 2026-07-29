# [PY_ARTIFACTS_GRAPHIC_RASTER_IO]

Raster IO, conversion, and working-surface behavior live on the closed-payload `RasterOp` family. `Raster` holds pillow decode/transpose/resize/alpha/save/montage/contact/geometry, pyvips fused decode/downscale/ICC/smartcrop/pyramid, delegated MIME detection, produced-raster transforms, source generation, and measured transforms. One `_CODEC` row per container binds the writer serving each engine — a pillow or libvips NATIVE encoder under its own linked-build probe, or the imagecodecs ARRAY writer for a container neither linked build writes — beside the `BandLaw` mode admission and the `FrameLaw` clock rung every arm reads, so egress crosses one pillow and one libvips function and no arm re-spells a save, a mode, or a timing key. Every operation folds into `RasterFact` or the closed `RasterFault` vocabulary, and each farm member lowers to its own `ArtifactWork`.

pillow, scikit-image, and pyvips are host-native worker packages off the runtime loader path, so `Raster` carries the caller-threaded `lane: LanePolicy` — the same seam field `exchange/detect#DETECT` and `graphic/color/derive#DERIVE` carry — and every worker arm crosses `lane.offload(Kernel.of(_worker_raster, KernelTrait.HOSTILE), op)` onto the shared runtime process band, never a folder-minted `CapacityLimiter` that oversubscribes the host against libvips's own thread pool, never the unbounded default, never a class-qualified `LanePolicy.offload` with no bound instance. `Detect` is the one arm off that seam: `puremagic` is pure-Python with a bundled `magic_data.json`, so `_emit` delegates it lane-threaded to `exchange/detect#DETECT` in-process (the `PUREMAGIC` engine's `RELEASING` thread kernel) with no process crossing, no retry, no payload pickle. `_worker_raster` is `@beartype(conf=FAULT_CONF)`-woven, so a contract violation raises the one `BeartypeCallHintViolation` the runtime `CLASSIFY` table folds onto the `RuntimeRail` as `BoundaryFault.api`, and an exhausted worker death terminates through the lane's `guard`/`async_boundary` conversion — neither is a `RasterFault` case, because the runtime owns both vocabularies and a parallel local case is a second carrier for one fact.

`RasterFact` is canonical on `graphic/raster/process#PROCESS`; this page, `graphic/marks/encode#MARK`, and `graphic/raster/measure#MEASURE` import the one declaration, and its `score: frozendict[str, float | str]` is the exact type `core/receipt#RECEIPT` `ArtifactReceipt.Preview.scores` carries, so the metrics floats, the detect/probe strings, and the marks facts project through one `_previewed` pass with no coerce — `Preview.bytes_` takes `len(fact.data)` on the fixed slot, never a band entry. Array-to-PNG egress is `graphic/raster/process#PROCESS`'s `_save_array`; this page exports no raster composable beside the rail.

## [01]-[INDEX]

- [02]-[IO]: `Raster` owns the host-free raster plane — pillow working surface, fused libvips pipeline, delegated `exchange/detect#DETECT` MIME gate, and the scikit-image `Transform` arm the process/measure siblings own — every worker arm crossing the caller-threaded runtime process lane, `Detect` in-process off it, folding into one `RasterFact` projected to `ArtifactReceipt.Preview`.

## [02]-[IO]

- Owner: `Raster` owns the closed `RasterOp` family. `GeometryOp` is its payload-carrying geometry sub-axis: fixed transforms carry no payload, `rotate` carries one angle, `affine` carries six coefficients, `perspective` carries eight coefficients, and `reduce` carries one positive factor. `RasterEngine` and `FitMode` remain policy vocabularies because engine reach and sizing behavior vary independently of operation identity. `_ENGINE` is one `frozendict[RasterEngine, EngineOps]`, so `_worker_raster` reads `probe`/`thumbnail`/`convert`/`crop` by one lookup and pillow and libvips share one op shape; `_CODEC` is the peer `frozendict[ConvertFormat, CodecRow]` carrying every codec fact — the per-engine `CodecEmit`, the `BandLaw` mode admission, the `FrameLaw` clock rung — so an arm resolves one row and never consults a parallel membership set the next container silently drops out of. Every static policy table on this page is a `frozendict` row set, the same container the transform tables use, so the composed `TRANSFORMS | MEASURE_TRANSFORMS` union is one total lookup.
- Cases: `Probe`/`Thumbnail`/`Convert`/`Crop` are engine-polymorphic; `Montage`/`Deframe` split by engine; `Composite`/`SmartCrop`/`Pyramid` are libvips-owned; `Geometry`/`Quantize`/`Children`/`Sequence`/`Contact` are pillow-owned; `Detect` delegates in-process; `Transform` carries an encoded operand; `Generate` carries only a source `Transform` and `TransformPolicy`. `Transform` rejects source rows, and `Generate` rejects operand rows before the worker crossing.
- Entry: `Raster.emit` discriminates on `self.ops` being one `RasterOp` or a tuple — `_normalized` folds either into one `Block[RasterOp]` at the head, so arity is a value property, never a `batch` knob. Each member lowers to its own `ArtifactWork` carrying that member's `RasterFault` as its boundary fault and binding `self.lane` into the work thunk, so one corrupt input faults its node while siblings complete under the plan's front drain — never a fail-fast batch that discards every sibling on the first bad payload.
- Auto: `RasterOp.admitted` validates empty collections, extents, timing arity, indices, codec ranges, geometry factors, transform operands, policy compatibility, and source payload timing before provider dispatch. `_emit` routes `Detect` in-process and crosses every other admitted op through the worker. `_worker_raster` total-dispatches under provision capture; pillow and libvips arms retain their provider-specific guards. `_transformed` decodes image/reference/mask rows once through `img_as_ubyte`; `_generated` constructs the source-only `TransformInput` without bytes or decode.
- Receipt: each op folds into `RasterFact` and projects to `core/receipt#RECEIPT` `ArtifactReceipt.Preview(key, width, height, bytes_, scores)` at the rail boundary — `ContentIdentity.key` mints the bare `ContentKey` over the produced bytes, `bytes_` takes `len(fact.data)`, and `fact.score` threads straight onto `Preview.scores` with no coerce; `Detect` reports zero dimensions with the resolved mime/class/container and native-`float` confidence, `Probe` reports header facts without transcoding, and measured transforms report perceptual and geometric facts.
- Growth: a new raster op is one `RasterOp` case, one `admitted` arm, and one `_worker_raster` arm; a new engine-polymorphic op one `EngineOps` field with a pillow and a libvips arm; a new sizing mode one `FitMode` case with its two branches; a new blend/crop/pyramid form one `BlendMode`/`CropFocus`/`PyramidLayout` member the libvips call resolves by nickname; a new geometric op one payload-correct `GeometryOp` case with one pillow arm; a new scikit-image transform one `Transform` member with a `TRANSFORMS`/`MEASURE_TRANSFORMS` row on the owning page; a new codec one `ConvertFormat` member with one `_CODEC` row naming a `CodecEmit` per engine it serves and answering the `BandLaw` and `FrameLaw` columns — an engine whose linked build writes the container nowhere carries no column and `_writer` faults `codec` for it; a new container band set one `BandLaw` member with its `_BANDS` entry; a new per-frame encoder key one `FrameLaw` rung carrying that key on every wider rung's cumulative `_CLOCK` tuple; a new engine one `RasterEngine` member with one `_ENGINE` bundle and one writer column on every `_CODEC` row that engine can write; a new fault cause one `RasterFault` case breaking every capture at type-check.
- Boundary: `_CODEC` writer columns carry libvips saver suffixes and pillow format names as literals because provider imports remain worker-local, and each literal is simultaneously the call spelling and its own capability-probe key. Every libvips saver suffix proves the build REGISTERED the operation and never that the operation's own encoder backend linked — `get_suffixes` offers `.heic` on a libheif carrying no HEVC encoder and `heifsave` then refuses, and `.avif` rides that same delegating saver — so the libvips probe is a memoized one-shot trial write and the missing backend refuses at the capability gate exactly as an unregistered suffix does. Container DEPTH stops at 8 bits: `ConvertFormat` names display containers, `Frame` is `uint8` whole, and the 16-bit, half, and float lanes of these same codec families are the deep-pixel texture plane's — a widened member here pushes an 8-bit intermediate onto a texture path and quantizes it silently. `BandLaw` states which MODES a container carries, never how alpha associates: association is a deep-pixel plane fact and a straight-versus-associated conversion at 8 bits quantizes catastrophically at low alpha, so this funnel declares admission and the texture plane owns the conversion. Payload-bearing operations carry canonical bytes rather than `pyvips.Source`/`Target`; `Generate` carries no bytes because source identity derives from its typed operation and policy. Streaming intake belongs to the consumer that owns stream identity. Descriptive EXIF/IPTC/XMP tags stay `exchange/metadata#METADATA`'s; MIME classification stays `exchange/detect#DETECT`'s; transform acceptors stay on process/measure; runtime contract and worker faults stay `BoundaryFault` cases.

```python signature
import os
from builtins import frozendict
from collections.abc import Callable, Iterable
from dataclasses import dataclass
from enum import StrEnum
from functools import cache, partial
from io import BytesIO
from struct import pack
from typing import Final, Literal, assert_never

import numpy as np
from beartype import beartype
from expression import Error, Ok, Result, case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct

from rasm.runtime.faults import FAULT_CONF, BoundaryFault, RuntimeRail
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.workers import Kernel, KernelTrait

from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.core.receipt import ArtifactReceipt
from rasm.artifacts.graphic.raster.process import ConvertFormat, Frame, RasterFact, Transform, TransformInput, TransformNeeds, TransformPolicy

lazy import imagecodecs

# libvips reads VIPS_CONCURRENCY once at library init, so the two-pool bound lands before the lazy binding resolves — this
# binding carries no concurrency_set/concurrency_get pair and a runtime call raises AttributeError
os.environ.setdefault("VIPS_CONCURRENCY", "1")
lazy import pyvips
lazy from PIL import Image, ImageOps, ImageSequence, UnidentifiedImageError, features
lazy from skimage import io as skio, util as skutil

lazy from rasm.artifacts.exchange.detect import Detect, DetectEngine, DetectIdentity, Source
lazy from rasm.artifacts.graphic.raster.measure import MEASURE_TRANSFORMS
lazy from rasm.artifacts.graphic.raster.process import TRANSFORMS

type RasterOpTag = Literal[
    "thumbnail",
    "convert",
    "crop",
    "probe",
    "montage",
    "composite",
    "transform",
    "generate",
    "detect",
    "smartcrop",
    "pyramid",
    "geometry",
    "deframe",
    "sequence",
    "contact",
    "quantize",
    "children",
]
type Pixels = tuple[int, int]
type Box = tuple[int, int, int, int]

_QUALITY: Final[int] = 80  # the codec-quality coordinate every `_CODEC` option builder reads; `Convert` re-exposes it, the rest bind it
_EFFORT: Final[int] = 4  # the encoder-effort coordinate each row spells in its own dialect — libvips `effort`/`compression`, pillow `method`/`speed`, libjxl effort


class RasterEngine(StrEnum):
    PILLOW = "pillow"
    LIBVIPS = "libvips"


class FitMode(StrEnum):
    CONTAIN = "contain"  # fit inside the box, preserve aspect, no crop (pillow ImageOps.contain / libvips crop=NONE)
    COVER = "cover"  # fill the box, crop the overflow (pillow ImageOps.fit / libvips crop=ATTENTION)
    STRETCH = "stretch"  # force the exact box, ignore aspect (pillow resize / libvips size=FORCE)
    PAD = "pad"  # fit inside, then letterbox to the exact box with background (pillow ImageOps.pad / libvips embed+Extend)


class CropFocus(StrEnum):  # the libvips Interesting model SmartCrop resolves by .value nickname
    ATTENTION = "attention"  # saliency-map peak (default)
    ENTROPY = "entropy"  # maximum-entropy window
    CENTRE = "centre"
    LOW = "low"
    HIGH = "high"
    ALL = "all"  # keep the whole image (the no-crop verdict the caller reads off the result box)


class PyramidLayout(StrEnum):  # the libvips ForeignDzLayout deep-zoom pyramid form dzsave_buffer emits by .value nickname
    DZ = "dz"  # DeepZoom
    ZOOMIFY = "zoomify"
    GOOGLE = "google"
    IIIF = "iiif"
    IIIF3 = "iiif3"


@tagged_union(frozen=True)
class GeometryOp:
    tag: Literal["flip_h", "flip_v", "rotate_90", "rotate_180", "rotate_270", "transpose", "transverse", "rotate", "affine", "perspective", "reduce"] = tag()
    flip_h: None = case()
    flip_v: None = case()
    rotate_90: None = case()
    rotate_180: None = case()
    rotate_270: None = case()
    transpose: None = case()
    transverse: None = case()
    rotate: float = case()
    affine: tuple[float, float, float, float, float, float] = case()
    perspective: tuple[float, float, float, float, float, float, float, float] = case()
    reduce: int = case()


class BlendMode(StrEnum):  # the libvips composite2 nickname vocabulary passed by .value (VipsBlendMode order); OVER is the source-over default
    CLEAR = "clear"
    SOURCE = "source"
    OVER = "over"
    IN = "in"
    OUT = "out"
    ATOP = "atop"
    DEST = "dest"
    DEST_OVER = "dest-over"
    DEST_IN = "dest-in"
    DEST_OUT = "dest-out"
    DEST_ATOP = "dest-atop"
    XOR = "xor"
    ADD = "add"
    SATURATE = "saturate"
    MULTIPLY = "multiply"
    SCREEN = "screen"
    OVERLAY = "overlay"
    DARKEN = "darken"
    LIGHTEN = "lighten"
    COLOUR_DODGE = "colour-dodge"
    COLOUR_BURN = "colour-burn"
    HARD_LIGHT = "hard-light"
    SOFT_LIGHT = "soft-light"
    DIFFERENCE = "difference"
    EXCLUSION = "exclusion"


class QuantizeMethod(StrEnum):  # NAMES congruent with Image.Quantize so Image.Quantize[method.name] resolves the provider enum; PIL enum stays at the edge
    MEDIANCUT = "median-cut"
    MAXCOVERAGE = "max-coverage"
    FASTOCTREE = "fast-octree"  # the only method admitting RGBA without a flatten
    LIBIMAGEQUANT = "libimagequant"  # build-dependent; the highest-quality quantizer


class DitherMode(StrEnum):  # member NAMES congruent with Image.Dither so Image.Dither[dither.name] resolves the provider enum
    NONE = "none"
    ORDERED = "ordered"
    RASTERIZE = "rasterize"
    FLOYDSTEINBERG = "floyd-steinberg"


@tagged_union(frozen=True)
class RasterFault:
    tag: Literal["decode", "bomb", "encode", "engine", "provision", "detect", "codec", "reference", "policy", "bounds", "empty", "extent", "arity", "range"] = tag()
    decode: str = case()
    bomb: tuple[int, int] = case()
    encode: str = case()
    engine: str = case()
    provision: str = case()
    detect: str = case()
    codec: ConvertFormat = case()  # a build-dependent AVIF/HEIF/WebP encoder the linked build lacks — the capability gate, distinct from an encode fault
    reference: Transform = case()  # a row whose needs (reference/mask) the payload omits — the row declares, this seam executes
    policy: tuple[Transform, str, str] = case()
    bounds: str = case()  # a frame/page/child index past the available count — the range fault distinct from a content fault
    empty: RasterOpTag = case()
    extent: tuple[RasterOpTag, tuple[int, ...]] = case()
    arity: tuple[RasterOpTag, int, int] = case()
    range: tuple[RasterOpTag, str, float] = case()


@tagged_union(frozen=True)
class RasterOp:
    tag: RasterOpTag = tag()
    thumbnail: tuple[bytes, Pixels, ConvertFormat, RasterEngine, FitMode] = case()
    convert: tuple[bytes, ConvertFormat, int, int, RasterEngine] = case()
    crop: tuple[bytes, Box, ConvertFormat, RasterEngine] = case()
    probe: tuple[bytes, RasterEngine] = case()
    montage: tuple[tuple[bytes, ...], int, Pixels, ConvertFormat, RasterEngine] = case()
    composite: tuple[bytes, bytes, Pixels, BlendMode, ConvertFormat] = case()
    transform: tuple[bytes, Transform, bytes, bytes, TransformPolicy] = case()
    generate: tuple[Transform, TransformPolicy] = case()
    detect: tuple[bytes] = case()
    smartcrop: tuple[bytes, Pixels, CropFocus, ConvertFormat] = case()
    pyramid: tuple[bytes, PyramidLayout, int, ConvertFormat] = case()
    geometry: tuple[bytes, GeometryOp, ConvertFormat] = case()
    deframe: tuple[bytes, int, ConvertFormat, RasterEngine] = case()
    sequence: tuple[tuple[bytes, ...], tuple[int, ...], int, int, ConvertFormat] = case()
    contact: tuple[bytes, int, Pixels, ConvertFormat] = case()
    quantize: tuple[bytes, int, QuantizeMethod, DitherMode, ConvertFormat] = case()
    children: tuple[bytes, int, ConvertFormat] = case()

    @staticmethod
    def Thumbnail(
        payload: bytes,
        size: Pixels,
        fmt: ConvertFormat = ConvertFormat.PNG,
        engine: RasterEngine = RasterEngine.PILLOW,
        fit: FitMode = FitMode.CONTAIN,
    ) -> "RasterOp":
        return RasterOp(thumbnail=(payload, size, fmt, engine, fit))

    @staticmethod
    def Convert(
        payload: bytes, codec: ConvertFormat, quality: int = _QUALITY, effort: int = _EFFORT, engine: RasterEngine = RasterEngine.PILLOW
    ) -> "RasterOp":
        return RasterOp(convert=(payload, codec, quality, effort, engine))

    @staticmethod
    def Crop(payload: bytes, box: Box, fmt: ConvertFormat = ConvertFormat.PNG, engine: RasterEngine = RasterEngine.PILLOW) -> "RasterOp":
        return RasterOp(crop=(payload, box, fmt, engine))

    @staticmethod
    def Probe(payload: bytes, engine: RasterEngine = RasterEngine.PILLOW) -> "RasterOp":
        return RasterOp(probe=(payload, engine))

    @staticmethod
    def Montage(
        tiles: tuple[bytes, ...], columns: int, cell: Pixels, fmt: ConvertFormat = ConvertFormat.PNG, engine: RasterEngine = RasterEngine.PILLOW
    ) -> "RasterOp":
        return RasterOp(montage=(tiles, columns, cell, fmt, engine))

    @staticmethod
    def Composite(
        base: bytes, overlay: bytes, position: Pixels = (0, 0), mode: BlendMode = BlendMode.OVER, fmt: ConvertFormat = ConvertFormat.PNG
    ) -> "RasterOp":
        return RasterOp(composite=(base, overlay, position, mode, fmt))

    @staticmethod
    def Transform(
        payload: bytes, kind: Transform, reference: bytes = b"", mask: bytes = b"", policy: TransformPolicy = TransformPolicy(default=None)
    ) -> "RasterOp":
        return RasterOp(transform=(payload, kind, reference, mask, policy))

    @staticmethod
    def Generate(kind: Transform, policy: TransformPolicy = TransformPolicy(default=None)) -> "RasterOp":
        return RasterOp(generate=(kind, policy))

    @staticmethod
    def Detect(payload: bytes) -> "RasterOp":
        return RasterOp(detect=(payload,))

    @staticmethod
    def SmartCrop(payload: bytes, size: Pixels, focus: CropFocus = CropFocus.ATTENTION, fmt: ConvertFormat = ConvertFormat.PNG) -> "RasterOp":
        return RasterOp(smartcrop=(payload, size, focus, fmt))

    @staticmethod
    def Pyramid(payload: bytes, layout: PyramidLayout = PyramidLayout.DZ, tile: int = 254, fmt: ConvertFormat = ConvertFormat.JPEG) -> "RasterOp":
        return RasterOp(pyramid=(payload, layout, tile, fmt))

    @staticmethod
    def Geometry(payload: bytes, op: GeometryOp, fmt: ConvertFormat = ConvertFormat.PNG) -> "RasterOp":
        return RasterOp(geometry=(payload, op, fmt))

    @staticmethod
    def Deframe(payload: bytes, index: int = 0, fmt: ConvertFormat = ConvertFormat.PNG, engine: RasterEngine = RasterEngine.PILLOW) -> "RasterOp":
        return RasterOp(deframe=(payload, index, fmt, engine))

    @staticmethod
    def Sequence(
        frames: tuple[bytes, ...], delays: tuple[int, ...] = (), loop: int = 0, disposal: int = 2, fmt: ConvertFormat = ConvertFormat.TIFF
    ) -> "RasterOp":
        return RasterOp(sequence=(frames, delays, loop, disposal, fmt))

    @staticmethod
    def Contact(payload: bytes, columns: int = 4, cell: Pixels = (256, 256), fmt: ConvertFormat = ConvertFormat.PNG) -> "RasterOp":
        return RasterOp(contact=(payload, columns, cell, fmt))

    @staticmethod
    def Quantize(
        payload: bytes,
        colors: int = 256,
        method: QuantizeMethod = QuantizeMethod.MEDIANCUT,
        dither: DitherMode = DitherMode.FLOYDSTEINBERG,
        fmt: ConvertFormat = ConvertFormat.PNG,
    ) -> "RasterOp":
        return RasterOp(quantize=(payload, colors, method, dither, fmt))

    @staticmethod
    def Children(payload: bytes, index: int = 0, fmt: ConvertFormat = ConvertFormat.PNG) -> "RasterOp":
        return RasterOp(children=(payload, index, fmt))

    def admitted(self, /) -> Result["RasterOp", RasterFault]:
        match self:
            case RasterOp(tag="thumbnail", thumbnail=(_, (width, height), _, _, _)) if width <= 0 or height <= 0:
                return Error(RasterFault(extent=(self.tag, (width, height))))
            case RasterOp(tag="convert", convert=(_, _, quality, _, _)) if not 0 <= quality <= 100:
                return Error(RasterFault(range=(self.tag, "quality", float(quality))))
            case RasterOp(tag="convert", convert=(_, _, _, effort, _)) if effort < 0:
                return Error(RasterFault(range=(self.tag, "effort", float(effort))))
            case RasterOp(tag="crop", crop=(_, (_, _, width, height), _, _)) if width <= 0 or height <= 0:
                return Error(RasterFault(extent=(self.tag, (width, height))))
            case RasterOp(tag="montage", montage=((), _, _, _, _)) | RasterOp(tag="sequence", sequence=((), _, _, _, _)):
                return Error(RasterFault(empty=self.tag))
            case RasterOp(tag="montage", montage=(_, columns, (width, height), _, _)) | RasterOp(tag="contact", contact=(_, columns, (width, height), _)) if (
                min(columns, width, height) <= 0
            ):
                return Error(RasterFault(extent=(self.tag, (columns, width, height))))
            case RasterOp(tag="montage", montage=(_, columns, (width, height), _, _)) | RasterOp(tag="contact", contact=(_, columns, (width, height), _)) if (
                Image.MAX_IMAGE_PIXELS is not None and columns * width * height > Image.MAX_IMAGE_PIXELS
            ):
                # one grid ROW already breaches Pillow's bomb ceiling — BOTH tiled arms refuse pre-run on one gate,
                # and `_grid` gates the composed extent again where the decoded tile count is known. A ceiling on one
                # arm alone is the divergence where one grid refuses a hostile extent and its twin allocates it.
                return Error(RasterFault(bomb=(columns * width * height, int(Image.MAX_IMAGE_PIXELS))))
            case RasterOp(tag="transform", transform=(_, kind, reference, mask, policy)):
                row = (TRANSFORMS | MEASURE_TRANSFORMS)[kind]
                match (row.accepts(policy), row.needs):
                    case (False, _):
                        return Error(RasterFault(policy=(kind, row.policy.tag, policy.tag)))
                    case (True, TransformNeeds.REFERENCE) if not reference:
                        return Error(RasterFault(reference=kind))
                    case (True, TransformNeeds.MASK) if not mask:
                        return Error(RasterFault(reference=kind))
                    case (True, TransformNeeds.SOURCE):
                        return Error(RasterFault(policy=(kind, "image", "source")))
                    case (True, TransformNeeds.NONE | TransformNeeds.REFERENCE | TransformNeeds.MASK):
                        return Ok(self)
                    case _ as unreachable:
                        assert_never(unreachable)
            case RasterOp(tag="generate", generate=(kind, policy)):
                row = (TRANSFORMS | MEASURE_TRANSFORMS)[kind]
                match (row.accepts(policy), row.needs):
                    case (False, _):
                        return Error(RasterFault(policy=(kind, row.policy.tag, policy.tag)))
                    case (True, TransformNeeds.SOURCE):
                        return Ok(self)
                    case (True, needs):
                        return Error(RasterFault(policy=(kind, "source", needs.value)))
            case RasterOp(tag="smartcrop", smartcrop=(_, (width, height), _, _)) if width <= 0 or height <= 0:
                return Error(RasterFault(extent=(self.tag, (width, height))))
            case RasterOp(tag="pyramid", pyramid=(_, _, tile, _)) if tile <= 0:
                return Error(RasterFault(extent=(self.tag, (tile,))))
            case RasterOp(tag="geometry", geometry=(_, GeometryOp(tag="reduce", reduce=factor), _)) if factor <= 0:
                return Error(RasterFault(range=(self.tag, "factor", float(factor))))
            case RasterOp(tag="deframe", deframe=(_, index, _, _)) | RasterOp(tag="children", children=(_, index, _)) if index < 0:
                return Error(RasterFault(range=(self.tag, "index", float(index))))
            case RasterOp(tag="sequence", sequence=(frames, delays, _, _, _)) if delays and len(delays) != len(frames):
                return Error(RasterFault(arity=(self.tag, len(frames), len(delays))))
            case RasterOp(tag="sequence", sequence=(_, delays, _, _, _)) if any(delay < 0 for delay in delays):
                return Error(RasterFault(range=(self.tag, "delay", float(min(delays)))))
            case RasterOp(tag="sequence", sequence=(_, _, loop, disposal, _)) if loop < 0 or not 0 <= disposal <= 3:
                # disposal is the closed GIF method band 0..3; an out-of-band value would reach save_all unchecked
                return Error(RasterFault(range=(self.tag, "animation", float(disposal if not 0 <= disposal <= 3 else loop))))
            case RasterOp(tag="quantize", quantize=(_, colors, _, _, _)) if not 1 <= colors <= 256:
                return Error(RasterFault(range=(self.tag, "colors", float(colors))))
            case RasterOp():
                return Ok(self)
            case _ as unreachable:
                assert_never(unreachable)


class Raster(Struct, frozen=True):
    ops: RasterOp | tuple[RasterOp, ...]
    lane: LanePolicy  # the caller-threaded offload seam — isolation, band, retry, and boundary are runtime-owned

    def emit(self, /) -> Iterable[ArtifactWork]:
        # one node per member — per-member PRE-RUN input keys keep elision per-member: a re-issued farm re-renders only changed ops.
        return tuple(
            ArtifactWork(
                key=_keyed(op),
                work=partial(Raster._emit, op, self.lane),
                parents=(),
                admission=Admission(keyed=None),
                cost=1.0,
            )
            for op in _normalized(self.ops)
        )

    @staticmethod
    async def _emit(op: RasterOp, lane: LanePolicy, /) -> RuntimeRail[ArtifactReceipt]:
        match op.admitted():  # Result is a constructor-function rail: patterns match the tagged shape, never Ok/Error class heads
            case Result(tag="error", error=fault):
                return Error(BoundaryFault(boundary=(f"raster.{op.tag}", f"{fault.tag}:{fault}")))
            case Result(tag="ok", ok=valid):
                match valid:
                    case RasterOp(tag="detect", detect=(payload,)):
                        identity = await Detect(lane=lane, engine=DetectEngine.PUREMAGIC).of(Source.Buffer(payload))
                        return identity.map(lambda di: _detected(valid, payload, di))
                    case _:
                        produced = await lane.offload(Kernel.of(_worker_raster, KernelTrait.HOSTILE), valid)
                        return produced.bind(
                            lambda res: res.map(lambda fact: _previewed(valid, fact)).map_error(
                                lambda fault: BoundaryFault(boundary=(f"raster.{valid.tag}", f"{fault.tag}:{fault}"))
                            )
                        )


def _normalized[T](values: T | Iterable[T], /) -> Block[T]:
    # Folds arity ONCE for the page — `Raster.ops` and the pillow frame sequence both discriminate here, so arity stays a
    # value property on every axis. Matching the CARRIER (Block/tuple/list) rather than the element type keeps the fold
    # element-agnostic AND leaves `bytes`/`str` — iterable, never a member collection — on the singleton arm.
    match values:
        case Block() as block:
            return block
        case tuple() | list() as many:
            return Block.of_seq(many)
        case lone:
            return Block.singleton(lone)


def _canonical(value: object, /) -> bytes:
    # length-framed canonical chunk (patterns rows [05]/[06]): every variable-width field frames its length and
    # every tuple counts its parts, so two adjacent collections can never shift one digest; bool reads before int.
    match value:
        case None:
            return b"\x00"
        case bool() as flag:
            return b"\x01" if flag else b"\x02"
        case bytes() as raw:
            return len(raw).to_bytes(8, "little") + raw
        case str() as text:
            return len(encoded := text.encode()).to_bytes(8, "little") + encoded
        case int() as number:
            # length-framed variable-width signed encoding — Python ints are unbounded, so a fixed 8-byte
            # window overflows on a large admitted reduction; the frame keeps adjacent chunks unshiftable.
            raw = number.to_bytes(number.bit_length() // 8 + 1, "little", signed=True)
            return len(raw).to_bytes(8, "little") + raw
        case float() as scalar:
            return pack("<d", scalar)
        case GeometryOp() | TransformPolicy() as tagged:
            return _canonical((tagged.tag, getattr(tagged, tagged.tag)))
        case tuple() as parts:
            return len(parts).to_bytes(8, "little") + b"".join(_canonical(part) for part in parts)
        case _ as unreachable:
            assert_never(unreachable)


def _keyed(op: RasterOp, /) -> ContentKey:
    # bare pre-run input key: `ContentIdentity.key` (not the railed `of`) over the case payload's canonical bytes.
    return ContentIdentity.key(f"raster-{op.tag}", _canonical(getattr(op, op.tag)))


def _previewed(op: RasterOp, fact: RasterFact, /) -> ArtifactReceipt:
    # receipt.slot threads the SAME pre-run `_keyed(op)` identity the node scheduled under (the reuse-fold hit/miss
    # law); the output-byte address rides the score band, never the slot.
    return ArtifactReceipt.Preview(
        _keyed(op), fact.width, fact.height, len(fact.data), fact.score | {"address": ContentIdentity.key(f"raster-{op.tag}", fact.data).hex}
    )
```

```python signature
@beartype(conf=FAULT_CONF)
def _worker_raster(op: RasterOp) -> Result[RasterFact, RasterFault]:
    # FAULT_CONF raises the one BeartypeCallHintViolation the runtime CLASSIFY table folds onto the
    # RuntimeRail as BoundaryFault.api — never a bare @beartype throwing an unclassified raise.
    try:
        match op:
            case RasterOp(tag="detect", detect=(_payload,)):
                return Error(RasterFault(detect="<detect-routed-in-process>"))  # totality witness only; `_emit` routes detect in-process
            case RasterOp(tag="probe", probe=(payload, engine)):
                return _ENGINE[engine].probe(payload)
            case RasterOp(tag="thumbnail", thumbnail=(payload, size, fmt, engine, fit)):
                return _ENGINE[engine].thumbnail(payload, size, fmt, fit)
            case RasterOp(tag="convert", convert=(payload, codec, quality, effort, engine)):
                return _ENGINE[engine].convert(payload, codec, quality, effort)
            case RasterOp(tag="crop", crop=(payload, box, fmt, engine)):
                return _ENGINE[engine].crop(payload, box, fmt)
            case RasterOp(tag="montage", montage=(tiles, columns, cell, fmt, engine)):
                return _montage(tiles, columns, cell, fmt, engine)
            case RasterOp(tag="composite", composite=(base, overlay, position, mode, fmt)):
                return _composite(base, overlay, position, mode, fmt)
            case RasterOp(tag="transform", transform=(payload, kind, reference, mask, policy)):
                return _transformed(payload, kind, reference, mask, policy)
            case RasterOp(tag="generate", generate=(kind, policy)):
                return _generated(kind, policy)
            case RasterOp(tag="smartcrop", smartcrop=(payload, size, focus, fmt)):
                return _smartcrop(payload, size, focus, fmt)
            case RasterOp(tag="pyramid", pyramid=(payload, layout, tile, fmt)):
                return _pyramid(payload, layout, tile, fmt)
            case RasterOp(tag="geometry", geometry=(payload, geo, fmt)):
                return _geometry(payload, geo, fmt)
            case RasterOp(tag="deframe", deframe=(payload, index, fmt, engine)):
                return _deframe(payload, index, fmt, engine)
            case RasterOp(tag="sequence", sequence=(frames, delays, loop, disposal, fmt)):
                return _sequence(frames, delays, loop, disposal, fmt)
            case RasterOp(tag="contact", contact=(payload, columns, cell, fmt)):
                return _contact(payload, columns, cell, fmt)
            case RasterOp(tag="quantize", quantize=(payload, colors, method, dither, fmt)):
                return _quantized(payload, colors, method, dither, fmt)
            case RasterOp(tag="children", children=(payload, index, fmt)):
                return _children(payload, index, fmt)
            case _ as unreachable:
                assert_never(unreachable)
    except ImportError as absent:
        return Error(RasterFault(provision=absent.name or "<worker-module>"))
    except OSError as unloadable:  # pyvips cffi dlopen of an unprovisioned libvips (the guards trap every content OSError before here)
        return Error(RasterFault(provision=str(unloadable)))


def _pillow_guarded(work: Callable[[], RasterFact], /) -> Result[RasterFact, RasterFault]:
    try:
        return Ok(work())
    except UnidentifiedImageError:
        return Error(RasterFault(decode="<pillow-unidentified>"))
    except Image.DecompressionBombError:
        return Error(RasterFault(bomb=(0, int(Image.MAX_IMAGE_PIXELS or 0))))
    except (EOFError, IndexError) as fault:
        # a seek/get_child_images/crop range overrun; IndexError is a LookupError sibling of KeyError, so it never shadows the encode arm's KeyError
        return Error(RasterFault(bounds=str(fault)))
    except (OSError, ValueError, KeyError) as fault:
        return Error(RasterFault(encode=type(fault).__name__))


def _vips_guarded(work: Callable[[], RasterFact], /) -> Result[RasterFact, RasterFault]:
    try:
        return Ok(work())
    except IndexError as fault:
        # pre-dispatch page/crop range gates raise IndexError exactly as the Pillow arms do -> bounds
        return Error(RasterFault(bounds=str(fault)))
    except pyvips.Error as fault:
        return Error(RasterFault(engine=str(fault)))


class BandLaw(StrEnum):
    # WHICH modes a container carries. One closed column replaces an alpha boolean beside a mode literal hidden in the
    # array admission: color-ness and alpha are two independent container facts, and a row that states only the second
    # hands a grayscale plane to a color-only encoder that refuses it as a content fault.
    FULL = "full"  # gray and color, with or without alpha
    OPAQUE = "opaque"  # gray and color, no alpha channel — an alpha-bearing mode flattens before the writer
    COLOR = "color"  # color only, with or without alpha — a gray mode promotes before the writer


class FrameLaw(StrEnum):
    # WHAT the container's encoder composes across frames, as a CUMULATIVE ladder: each rung is its predecessor plus one
    # timing key, so the clock builds by reading the rung rather than by asking three independent booleans which key it
    # may spell — and the state a boolean pair cannot express (multi-frame with no clock) becomes a rung of its own.
    SINGLE = "single"  # one frame; a multi-frame request refuses at the capability gate
    PAGES = "pages"  # multi-frame directory, no clock at all
    TIMED = "timed"  # per-frame duration
    LOOPED = "looped"  # duration + loop
    DISPOSED = "disposed"  # duration + loop + per-frame disposal


_BANDS: Final[frozendict[BandLaw, frozenset[str]]] = frozendict({
    # Rows declare their admitted working modes. `RGB` closes every set, so `_moded`'s terminal is this law and not a defensive
    # branch; a container carrying no RGB form would be a plane container, and planes are the deep-pixel estate's.
    BandLaw.FULL: frozenset({"L", "LA", "RGB", "RGBA"}),
    BandLaw.OPAQUE: frozenset({"L", "RGB"}),
    BandLaw.COLOR: frozenset({"RGB", "RGBA"}),
})
_CLOCK: Final[frozendict[FrameLaw, tuple[str, ...]]] = frozendict({
    # Each rung licenses its own per-frame save kwargs, cumulative by construction. Keys absent from a rung are knobs the
    # plugin reads nowhere, so spelling it attests a timing the file never carries.
    FrameLaw.SINGLE: (),
    FrameLaw.PAGES: (),
    FrameLaw.TIMED: ("duration",),
    FrameLaw.LOOPED: ("duration", "loop"),
    FrameLaw.DISPOSED: ("duration", "loop", "disposal"),
})


@tagged_union(frozen=True)
class CodecEmit:
    # WHO writes the container bytes, orthogonal to the RasterEngine owning the working surface. `native` is the writer the
    # engine's own provider ships — its call spelling, its build probe, its option builder; `array` is the imagecodecs writer
    # for a container NEITHER provider's linked build carries, taking the 8-bit `Frame` whichever working surface produced it.
    tag: Literal["native", "array"] = tag()
    native: tuple[str, Callable[[], bool], Callable[[int, int], frozendict[str, object]]] = case()
    array: tuple[Callable[[], bool], Callable[[Frame, int, int], bytes]] = case()


@dataclass(frozen=True, slots=True, kw_only=True)
class CodecRow:
    # ONE row per container: every codec fact a working arm reads — writer per engine, mode admission, frame law — so an
    # arm never consults a parallel membership set the next codec silently drops out of. Two row laws close the shape:
    # a rung past SINGLE names a container pillow's own `SAVE_ALL` registry carries, because `save_all` on a format that
    # registry lacks raises a bare `KeyError` the encode arm reads as a content fault; and an `array` writer column pairs
    # only with SINGLE, because the codec substrate takes one `Frame` and composes no container framing.
    writers: frozendict[RasterEngine, CodecEmit]  # the writer serving each engine; an absent key is a container that engine cannot write
    bands: BandLaw  # the modes the container carries; every egress resolves its working mode against this set
    frames: FrameLaw  # the cumulative clock rung; SINGLE refuses a multi-frame request at the capability gate


def _pillow_writer(name: str, feature: str | None, options: Callable[[int, int], frozendict[str, object]], /) -> CodecEmit:
    # `features.check` reads pillow's OPTIONAL-codec table alone, so only a row whose plugin links an external encoder names
    # a flag; a core plugin names `None` because probing an unlisted name warns and answers False, which would refuse a
    # container every pillow build writes. The probe therefore answers the LINKED build and no row asserts a lean wheel's gap.
    return CodecEmit(native=(name, (lambda: True) if feature is None else (lambda: features.check(feature)), options))


@cache
def _vips_backed(suffix: str, /) -> bool:
    # Each saver suffix is the `write_to_buffer` spelling AND its own probe key, yet membership proves only that the build
    # REGISTERED the operation: libvips delegates the whole HEIF family — `.heic`, `.heif`, and `.avif` alike — to libheif,
    # so `get_suffixes` offers `.heic` on a libheif linking no HEVC encoder and `heifsave` then refuses "Unsupported
    # compression" mid-write, where the lazy pipeline has already paid the decode and the guard reads it as an engine
    # fault. The probe is therefore a one-shot trial write of a 1x1 sRGB image, memoized per suffix per process, so a
    # delegating saver's missing backend refuses at the capability gate exactly as an unregistered suffix does — and the
    # trial exercises the real call path, not a name. Content failures keep raising from the working pipeline, where
    # `_vips_guarded` classes them `engine`; nothing at egress is re-read as a capability gap.
    if suffix not in pyvips.base.get_suffixes():
        return False
    try:
        pyvips.Image.black(1, 1, bands=3).colourspace(pyvips.Interpretation.SRGB).write_to_buffer(suffix)
    except pyvips.Error:
        return False
    return True


def _vips_writer(suffix: str, options: Callable[[int, int], frozendict[str, object]], /) -> CodecEmit:
    return CodecEmit(native=(suffix, partial(_vips_backed, suffix), options))


def _array_writer(probe: Callable[[], bool], encode: Callable[[Frame, int, int], bytes], /) -> CodecEmit:
    # Imagecodecs leg: `<CODEC>.available` is the ONE attribute safe on an absent native core (every other read raises
    # `DelayedImportError`), and the encode closure drops the quality/effort coordinates its container carries no knob for.
    return CodecEmit(array=(probe, encode))


def _shared(emit: CodecEmit, /) -> frozendict[RasterEngine, CodecEmit]:
    # a container NEITHER linked build writes rides one array writer across the whole engine vocabulary — each working
    # surface hands it the same admitted `Frame`, so the row derives its columns instead of enumerating them per engine
    return frozendict({engine: emit for engine in RasterEngine})


def _writer(codec: ConvertFormat, engine: RasterEngine, /) -> Result[CodecEmit, RasterFault]:
    # Rows DECLARE which writer serves the engine and that writer's own probe READS the linked build, so an unbuilt
    # AVIF/WebP/JXL encoder faults `codec` — the capability gate — before `save`/`write_to_buffer` raises the opaque
    # provider error the `encode` arm would misclassify as a content fault. Every producing arm resolves here on the rail
    # BEFORE its guarded body, so a container the engine cannot write refuses without paying a decode.
    match _CODEC[codec].writers.get(engine):
        case CodecEmit(tag="native", native=(_, probe, _)) | CodecEmit(tag="array", array=(probe, _)) as emit if probe():
            return Ok(emit)
        case _:
            return Error(RasterFault(codec=codec))


def _moded(image: "Image.Image", law: BandLaw, /) -> str:
    # Resolves mode ONCE for every egress, native and array alike: the row's admitted set decides and the source's
    # own alpha and gray-ness pick within it, so a flatten never invents color and a promotion never invents alpha. A
    # palette, CMYK, or high-bit-depth working mode carries neither property into the set and lands on the widest
    # admitted member; `RGB` closes every `_BANDS` set, so the terminal states the law rather than guarding against it.
    admitted, alpha = _BANDS[law], image.has_transparency_data
    gray = image.mode in {"1", "L", "LA", "I;16"}
    for candidate in (image.mode, "LA" if gray else "RGBA", "RGBA", "L" if gray else "RGB"):
        if candidate in admitted and (alpha or candidate not in {"LA", "RGBA"}):
            return candidate
    return "RGB"


def _banded(image: "Image.Image", /, *, law: BandLaw) -> "Image.Image":
    return image if image.mode == (admitted := _moded(image, law)) else image.convert(admitted)


def _framed(image: "Image.Image", /, *, law: BandLaw) -> Frame:
    # Array writers admit exactly the modes their container carries — `qoi_encode` answers a gray plane with
    # "photometric 1 not supported" — so the array leg reads the same row law the native leg does and holds no second
    # admission literal of its own.
    return np.asarray(_banded(image, law=law))


def _framed_vips(image: "pyvips.Image", /, *, law: BandLaw) -> Frame:
    # `numpy()` hands out the band-interleaved buffer directly and a ONE-band image drops the band axis entirely, so a
    # color-only container promotes through `colourspace` BEFORE the read rather than handing the codec a 2-D array; the
    # uchar cast bounds the ushort/float working formats libvips carries natively back onto this page's 8-bit funnel, so
    # a deep plane never reaches an array writer unquantized
    colored = image.colourspace(pyvips.Interpretation.SRGB) if image.bands < 3 and _BANDS[law].isdisjoint({"L", "LA"}) else image
    return colored.cast(pyvips.BandFormat.UCHAR).numpy()


def _pillow_bytes(
    frames: "Image.Image | Iterable[Image.Image]", codec: ConvertFormat, emit: CodecEmit, quality: int, effort: int, /, **save: object
) -> bytes:
    # Owns the ONE pillow egress, arity-polymorphic: a lone image and a frame sequence enter the same call and `_normalized`
    # discriminates, so `save_all`/`append_images` is a property of the payload rather than a second writer. Mode
    # admission and encoder options both read the row, so no arm re-spells `save(format=...)` and silently hands the
    # encoder a band set it refuses — the flatten a no-alpha target needs and the promotion a color-only target needs are
    # one fold; `**save` carries only what the ARM contributes beyond the row (the `_CLOCK` keys `_sequence` folds).
    block = _normalized(frames).map(partial(_banded, law=_CODEC[codec].bands))
    match emit:
        case CodecEmit(tag="native", native=(name, _, options)):
            multi = frozendict({"save_all": True, "append_images": list(block.tail())}) if len(block) > 1 else frozendict()
            sink = BytesIO()
            block.head().save(sink, format=name, **options(quality, effort) | multi | save)
            return sink.getvalue()
        case CodecEmit(tag="array", array=(_, encode)):
            # single-frame by the row law: an `array` column pairs only with `FrameLaw.SINGLE`, so no multi-frame block reaches here
            return encode(_framed(block.head(), law=_CODEC[codec].bands), quality, effort)
        case _ as unreachable:
            assert_never(unreachable)


def _vips_bytes(image: "pyvips.Image", codec: ConvertFormat, emit: CodecEmit, quality: int, effort: int, /) -> bytes:
    # Owns the ONE libvips egress; `write_to_buffer` strips metadata by default, so every arm — not just Convert — retains
    # ICC/EXIF/XMP here and an `icc_transform`-managed profile survives a thumbnail, crop, composite, or smartcrop egress.
    # libvips carries bands rather than modes, so the row's admitted set decides the flatten and `_framed_vips` the promotion.
    law = _CODEC[codec].bands
    flat = image.flatten() if image.hasalpha() and _BANDS[law].isdisjoint({"LA", "RGBA"}) else image
    match emit:
        case CodecEmit(tag="native", native=(suffix, _, options)):
            keep = pyvips.ForeignKeep.ICC | pyvips.ForeignKeep.EXIF | pyvips.ForeignKeep.XMP
            return flat.write_to_buffer(suffix, keep=keep, **options(quality, effort))
        case CodecEmit(tag="array", array=(_, encode)):
            return encode(_framed_vips(flat, law=law), quality, effort)
        case _ as unreachable:
            assert_never(unreachable)


def _detected(op: RasterOp, payload: bytes, identity: "DetectIdentity", /) -> ArtifactReceipt:
    # project the delegated DetectIdentity onto the shared Preview score band; the puremagic sniff fold owned once
    # upstream. receipt.slot threads the pre-run `_keyed(op)` identity; the payload address rides the band.
    return ArtifactReceipt.Preview(
        _keyed(op),
        0,
        0,
        len(payload),
        frozendict({
            "address": ContentIdentity.key(f"raster-{op.tag}", payload).hex,
            "mime": identity.mime,
            "media_class": identity.media_class.value,
            "container": identity.container.value,
            "extension": identity.extensions[0] if identity.extensions else "",
            "confidence": identity.confidence,  # the native float ambiguity signal libmagic cannot supply — the exchange/detect Trust gate input
            "candidates": float(len(identity.matches)),
            "trust": identity.trust.value,
        }),
    )


def _transformed(payload: bytes, kind: Transform, reference: bytes, mask: bytes, policy: TransformPolicy, /) -> Result[RasterFact, RasterFault]:
    table = TRANSFORMS | MEASURE_TRANSFORMS
    row = table[kind]
    try:
        match row.needs:
            case TransformNeeds.NONE:
                frame = skutil.img_as_ubyte(skio.imread(BytesIO(payload)))
                tx = TransformInput(image=(frame, kind, policy))
            case TransformNeeds.REFERENCE:
                frame = skutil.img_as_ubyte(skio.imread(BytesIO(payload)))
                tx = TransformInput(reference=(frame, kind, reference, policy))
            case TransformNeeds.MASK:
                frame = skutil.img_as_ubyte(skio.imread(BytesIO(payload)))
                tx = TransformInput(mask=(frame, kind, mask, policy))
            case TransformNeeds.SOURCE:
                return Error(RasterFault(policy=(kind, "image", "source")))
            case _ as unreachable:
                assert_never(unreachable)
        return Ok(row.arm(tx))
    except (ValueError, OSError, KeyError) as fault:
        return Error(RasterFault(engine=f"skimage:{kind.value}:{type(fault).__name__}"))


def _generated(kind: Transform, policy: TransformPolicy, /) -> Result[RasterFact, RasterFault]:
    try:
        return Ok(TRANSFORMS[kind].arm(TransformInput(source=(kind, policy))))
    except (ValueError, OSError, KeyError) as fault:
        return Error(RasterFault(engine=f"pillow:{kind.value}:{type(fault).__name__}"))


def _thumbnail_pillow(payload: bytes, size: Pixels, fmt: ConvertFormat, fit: FitMode) -> Result[RasterFact, RasterFault]:
    def work(emit: CodecEmit) -> RasterFact:
        image = ImageOps.exif_transpose(Image.open(BytesIO(payload)))
        match fit:
            case FitMode.COVER:
                fitted = ImageOps.fit(image, size)
            case FitMode.STRETCH:
                fitted = image.resize(size)
            case FitMode.CONTAIN:
                fitted = ImageOps.contain(image, size)
            case FitMode.PAD:
                fitted = ImageOps.pad(image, size)
            case _ as unreachable:
                assert_never(unreachable)
        return RasterFact(_pillow_bytes(fitted, fmt, emit, _QUALITY, _EFFORT), *fitted.size)

    return _writer(fmt, RasterEngine.PILLOW).bind(lambda emit: _pillow_guarded(partial(work, emit)))


def _thumbnail_libvips(payload: bytes, size: Pixels, fmt: ConvertFormat, fit: FitMode) -> Result[RasterFact, RasterFault]:
    def work(emit: CodecEmit) -> RasterFact:
        crop = pyvips.Interesting.ATTENTION if fit is FitMode.COVER else pyvips.Interesting.NONE
        sizing = pyvips.Size.FORCE if fit is FitMode.STRETCH else pyvips.Size.DOWN
        shrunk = pyvips.Image.new_from_buffer(payload, "", access=pyvips.Access.SEQUENTIAL, fail_on=pyvips.FailOn.ERROR).thumbnail_image(
            size[0], height=size[1], size=sizing, crop=crop
        )
        image = (
            shrunk.embed((size[0] - shrunk.width) // 2, (size[1] - shrunk.height) // 2, size[0], size[1], extend=pyvips.Extend.BACKGROUND)
            if fit is FitMode.PAD
            else shrunk
        )
        return RasterFact(_vips_bytes(image, fmt, emit, _QUALITY, _EFFORT), image.width, image.height)

    return _writer(fmt, RasterEngine.LIBVIPS).bind(lambda emit: _vips_guarded(partial(work, emit)))


def _convert_pillow(payload: bytes, codec: ConvertFormat, quality: int, effort: int) -> Result[RasterFact, RasterFault]:
    def work(emit: CodecEmit) -> RasterFact:
        image = ImageOps.exif_transpose(Image.open(BytesIO(payload)))
        return RasterFact(_pillow_bytes(image, codec, emit, quality, effort), *image.size)

    return _writer(codec, RasterEngine.PILLOW).bind(lambda emit: _pillow_guarded(partial(work, emit)))


def _convert_libvips(payload: bytes, codec: ConvertFormat, quality: int, effort: int) -> Result[RasterFact, RasterFault]:
    def work(emit: CodecEmit) -> RasterFact:
        source = pyvips.Image.new_from_buffer(payload, "", access=pyvips.Access.SEQUENTIAL, fail_on=pyvips.FailOn.ERROR).autorot()
        managed = source.icc_transform("srgb", intent=pyvips.Intent.RELATIVE) if source.get_typeof("icc-profile-data") != 0 else source
        return RasterFact(_vips_bytes(managed, codec, emit, quality, effort), managed.width, managed.height)

    return _writer(codec, RasterEngine.LIBVIPS).bind(lambda emit: _vips_guarded(partial(work, emit)))


def _crop_pillow(payload: bytes, box: Box, fmt: ConvertFormat) -> Result[RasterFact, RasterFault]:
    def work(emit: CodecEmit) -> RasterFact:
        # decoded-extent gate: Pillow `.crop` silently zero-pads past the image edge and libvips `extract_area`
        # raises an opaque engine error — both arms gate identically here, so an out-of-image crop faults `bounds`
        left, top, width, height = box
        image = ImageOps.exif_transpose(Image.open(BytesIO(payload)))
        if left < 0 or top < 0 or left + width > image.width or top + height > image.height:
            raise IndexError(f"crop {box} of {image.width}x{image.height}")
        region = image.crop((left, top, left + width, top + height))
        return RasterFact(_pillow_bytes(region, fmt, emit, _QUALITY, _EFFORT), *region.size)

    return _writer(fmt, RasterEngine.PILLOW).bind(lambda emit: _pillow_guarded(partial(work, emit)))


def _crop_libvips(payload: bytes, box: Box, fmt: ConvertFormat) -> Result[RasterFact, RasterFault]:
    def work(emit: CodecEmit) -> RasterFact:
        left, top, width, height = box
        source = pyvips.Image.new_from_buffer(payload, "", access=pyvips.Access.SEQUENTIAL, fail_on=pyvips.FailOn.ERROR)
        if left < 0 or top < 0 or left + width > source.width or top + height > source.height:
            raise IndexError(f"crop {box} of {source.width}x{source.height}")
        image = source.extract_area(*box)
        return RasterFact(_vips_bytes(image, fmt, emit, _QUALITY, _EFFORT), image.width, image.height)

    return _writer(fmt, RasterEngine.LIBVIPS).bind(lambda emit: _vips_guarded(partial(work, emit)))


def _probe_pillow(payload: bytes) -> Result[RasterFact, RasterFault]:
    def work() -> RasterFact:
        with Image.open(BytesIO(payload)) as image:
            score: frozendict[str, float | str] = frozendict({
                "format": image.format or "",
                "mode": image.mode,
                "frames": str(getattr(image, "n_frames", 1)),
                "icc": "present" if image.info.get("icc_profile") else "absent",
            })
            return RasterFact(payload, image.width, image.height, score)

    return _pillow_guarded(work)


def _probe_libvips(payload: bytes) -> Result[RasterFact, RasterFault]:
    def work() -> RasterFact:
        image = pyvips.Image.new_from_buffer(payload, "", access=pyvips.Access.SEQUENTIAL, fail_on=pyvips.FailOn.ERROR)
        pages = image.get("n-pages") if image.get_typeof("n-pages") != 0 else 1
        score: frozendict[str, float | str] = frozendict({
            "interpretation": str(image.interpretation),
            "bands": str(image.bands),
            "pages": str(pages),
            "icc": "present" if image.get_typeof("icc-profile-data") != 0 else "absent",
        })
        return RasterFact(payload, image.width, image.height, score)

    return _vips_guarded(work)


def _grid(
    tiles: list["Image.Image"], columns: int, cell: Pixels, fmt: ConvertFormat, emit: CodecEmit, score: frozendict[str, float | str], /
) -> RasterFact:
    # Composes the ONE pillow grid both tiled arms reach — Montage over decoded payloads, Contact over the frames of
    # one animation — so the COMPOSED extent obeys Pillow's bomb ceiling exactly once. `Image.new` allocates unchecked and
    # `admitted` bounds only a single grid ROW, so the tile count that turns an admitted row into a hostile allocation is
    # knowable only here; a ceiling on one arm alone is the divergence where one grid refuses and its twin allocates.
    cell_w, cell_h = cell
    rows = -(-len(tiles) // columns)
    pixels = columns * cell_w * rows * cell_h
    if Image.MAX_IMAGE_PIXELS is not None and pixels > Image.MAX_IMAGE_PIXELS:
        raise Image.DecompressionBombError(f"grid {pixels} pixels exceeds MAX_IMAGE_PIXELS {Image.MAX_IMAGE_PIXELS}")
    grid = Image.new("RGBA", (columns * cell_w, rows * cell_h))
    for index, tile in enumerate(tiles):
        tile.thumbnail(cell)
        row, col = divmod(index, columns)
        grid.paste(tile, (col * cell_w, row * cell_h))
    return RasterFact(_pillow_bytes(grid, fmt, emit, _QUALITY, _EFFORT), *grid.size, score)


def _montage(tiles: tuple[bytes, ...], columns: int, cell: Pixels, fmt: ConvertFormat, engine: RasterEngine) -> Result[RasterFact, RasterFault]:
    match engine:
        case RasterEngine.PILLOW:

            def work(emit: CodecEmit) -> RasterFact:
                return _grid([Image.open(BytesIO(blob)) for blob in tiles], columns, cell, fmt, emit, frozendict({"tiles": float(len(tiles))}))

            return _writer(fmt, RasterEngine.PILLOW).bind(lambda emit: _pillow_guarded(partial(work, emit)))
        case RasterEngine.LIBVIPS:

            def work(emit: CodecEmit) -> RasterFact:
                # fused arrayjoin grid: each cell shrinks-on-load, the grid computes in one streamed pass — large-tile parity pillow's paste loop cannot match
                cells = [
                    pyvips.Image.new_from_buffer(blob, "", access=pyvips.Access.SEQUENTIAL, fail_on=pyvips.FailOn.ERROR).thumbnail_image(
                        cell[0], height=cell[1], size=pyvips.Size.DOWN
                    )
                    for blob in tiles
                ]
                grid = pyvips.Image.arrayjoin(cells, across=columns)
                return RasterFact(_vips_bytes(grid, fmt, emit, _QUALITY, _EFFORT), grid.width, grid.height)

            return _writer(fmt, RasterEngine.LIBVIPS).bind(lambda emit: _vips_guarded(partial(work, emit)))
        case _ as unreachable:
            assert_never(unreachable)


def _composite(base: bytes, overlay: bytes, position: Pixels, mode: BlendMode, fmt: ConvertFormat) -> Result[RasterFact, RasterFault]:
    def work(emit: CodecEmit) -> RasterFact:
        canvas = pyvips.Image.new_from_buffer(base, "", access=pyvips.Access.SEQUENTIAL, fail_on=pyvips.FailOn.ERROR)
        layer = pyvips.Image.new_from_buffer(overlay, "", access=pyvips.Access.SEQUENTIAL, fail_on=pyvips.FailOn.ERROR)
        merged = canvas.composite2(layer, mode.value, x=position[0], y=position[1])
        return RasterFact(_vips_bytes(merged, fmt, emit, _QUALITY, _EFFORT), merged.width, merged.height)

    return _writer(fmt, RasterEngine.LIBVIPS).bind(lambda emit: _vips_guarded(partial(work, emit)))


def _smartcrop(payload: bytes, size: Pixels, focus: CropFocus, fmt: ConvertFormat) -> Result[RasterFact, RasterFault]:
    def work(emit: CodecEmit) -> RasterFact:
        # content-aware crop: libvips saliency/entropy extracts the interesting window a fixed-box Crop cannot
        image = (
            pyvips.Image.new_from_buffer(payload, "", access=pyvips.Access.SEQUENTIAL, fail_on=pyvips.FailOn.ERROR)
            .autorot()
            .smartcrop(size[0], size[1], interesting=focus.value)
        )
        return RasterFact(_vips_bytes(image, fmt, emit, _QUALITY, _EFFORT), image.width, image.height)

    return _writer(fmt, RasterEngine.LIBVIPS).bind(lambda emit: _vips_guarded(partial(work, emit)))


def _pyramid(payload: bytes, layout: PyramidLayout, tile: int, fmt: ConvertFormat) -> Result[RasterFact, RasterFault]:
    # dzsave keys its TILE encoder by the same saver suffix the row already carries, so the pyramid reads the writer
    # column instead of lowering the enum value — a container libvips writes only through the array leg has no tile
    # spelling at all, and the `array` arm faults `codec` rather than handing dzsave a suffix the build never registered
    def work(suffix: str) -> RasterFact:
        # DeepZoom/Zoomify/IIIF pyramid tiling to one zip blob — the large-scan tiled-viewer export
        image = pyvips.Image.new_from_buffer(payload, "", access=pyvips.Access.SEQUENTIAL, fail_on=pyvips.FailOn.ERROR).autorot()
        blob = image.dzsave_buffer(layout=layout.value, tile_size=tile, suffix=suffix, container="zip")
        return RasterFact(blob, image.width, image.height)

    match _writer(fmt, RasterEngine.LIBVIPS):
        case Result(tag="error", error=fault):
            return Error(fault)
        case Result(tag="ok", ok=CodecEmit(tag="native", native=(suffix, _, _))):
            return _vips_guarded(partial(work, suffix))
        case Result(tag="ok"):
            return Error(RasterFault(codec=fmt))
        case _ as unreachable:
            assert_never(unreachable)


def _geometry(payload: bytes, op: GeometryOp, fmt: ConvertFormat) -> Result[RasterFact, RasterFault]:
    def work(emit: CodecEmit) -> RasterFact:
        image = ImageOps.exif_transpose(Image.open(BytesIO(payload)))
        match op:
            case GeometryOp(tag="rotate", rotate=angle):
                out = image.rotate(angle, resample=Image.Resampling.BICUBIC, expand=True)
            case GeometryOp(tag="reduce", reduce=factor):
                out = image.reduce(factor)
            case GeometryOp(tag="affine", affine=coefficients):
                out = image.transform(image.size, Image.Transform.AFFINE, coefficients, resample=Image.Resampling.BICUBIC)
            case GeometryOp(tag="perspective", perspective=coefficients):
                out = image.transform(image.size, Image.Transform.PERSPECTIVE, coefficients, resample=Image.Resampling.BICUBIC)
            case GeometryOp(tag="flip_h"):
                out = image.transpose(Image.Transpose.FLIP_LEFT_RIGHT)
            case GeometryOp(tag="flip_v"):
                out = image.transpose(Image.Transpose.FLIP_TOP_BOTTOM)
            case GeometryOp(tag="rotate_90"):
                out = image.transpose(Image.Transpose.ROTATE_90)
            case GeometryOp(tag="rotate_180"):
                out = image.transpose(Image.Transpose.ROTATE_180)
            case GeometryOp(tag="rotate_270"):
                out = image.transpose(Image.Transpose.ROTATE_270)
            case GeometryOp(tag="transpose"):
                out = image.transpose(Image.Transpose.TRANSPOSE)
            case GeometryOp(tag="transverse"):
                out = image.transpose(Image.Transpose.TRANSVERSE)
            case _ as unreachable:
                assert_never(unreachable)
        return RasterFact(_pillow_bytes(out, fmt, emit, _QUALITY, _EFFORT), *out.size)

    return _writer(fmt, RasterEngine.PILLOW).bind(lambda emit: _pillow_guarded(partial(work, emit)))


def _deframe(payload: bytes, index: int, fmt: ConvertFormat, engine: RasterEngine) -> Result[RasterFact, RasterFault]:
    match engine:
        case RasterEngine.PILLOW:

            def work(emit: CodecEmit) -> RasterFact:
                # seek to the display-index frame, re-encode single-frame; an index past n_frames raises IndexError -> bounds
                image = Image.open(BytesIO(payload))
                frames = int(getattr(image, "n_frames", 1))
                if not 0 <= index < frames:
                    raise IndexError(f"frame {index} of {frames}")
                image.seek(index)
                return RasterFact(
                    _pillow_bytes(image, fmt, emit, _QUALITY, _EFFORT), *image.size, frozendict({"frame": float(index), "frames": float(frames)})
                )

            return _writer(fmt, RasterEngine.PILLOW).bind(lambda emit: _pillow_guarded(partial(work, emit)))
        case RasterEngine.LIBVIPS:

            def work(emit: CodecEmit) -> RasterFact:
                # libvips page= streams one page of a huge multi-page TIFF/PDF scan without materializing the whole
                # document; the n-pages probe gates the index first, so an invalid page faults `bounds` exactly as the
                # Pillow arm does, never an opaque provider `engine` raise
                probe = pyvips.Image.new_from_buffer(payload, "", access=pyvips.Access.SEQUENTIAL, fail_on=pyvips.FailOn.ERROR)
                pages = int(probe.get("n-pages")) if probe.get_typeof("n-pages") != 0 else 1
                if not 0 <= index < pages:
                    raise IndexError(f"page {index} of {pages}")
                image = pyvips.Image.new_from_buffer(payload, "", page=index, access=pyvips.Access.SEQUENTIAL, fail_on=pyvips.FailOn.ERROR)
                return RasterFact(
                    _vips_bytes(image, fmt, emit, _QUALITY, _EFFORT), image.width, image.height, frozendict({"frame": float(index), "frames": float(pages)})
                )

            return _writer(fmt, RasterEngine.LIBVIPS).bind(lambda emit: _vips_guarded(partial(work, emit)))
        case _ as unreachable:
            assert_never(unreachable)


def _sequence(frames: tuple[bytes, ...], delays: tuple[int, ...], loop: int, disposal: int, fmt: ConvertFormat) -> Result[RasterFact, RasterFault]:
    def work(emit: CodecEmit) -> RasterFact:
        # Multi-frame WRITE: `_pillow_bytes` discriminates on the block arity and composes save_all/append_images, so
        # this arm contributes only the clock the row's rung licenses. `_CLOCK` is cumulative, so the rung NAMES its keys
        # and the fold never asks three booleans which one it may spell — a container carrying pages and no clock keeps
        # every key off, and a key its plugin reads nowhere is never spelled at all.
        images = [Image.open(BytesIO(blob)) for blob in frames]
        clock: frozendict[str, object] = frozendict({"duration": delays, "loop": loop, "disposal": disposal})
        timing = frozendict({key: clock[key] for key in _CLOCK[_CODEC[fmt].frames] if key != "duration" or delays})
        return RasterFact(_pillow_bytes(images, fmt, emit, _QUALITY, _EFFORT, **timing), *images[0].size, frozendict({"frames": float(len(images))}))

    match _CODEC[fmt].frames:
        case FrameLaw.SINGLE:
            # a SINGLE-frame container composes no framing at all, whichever writer serves it: pillow's own `SAVE_ALL`
            # registry answers `save_all` on a JPEG, BMP, or QOI target with a bare `KeyError` the encode arm would read
            # as a content fault, and an array writer takes one `Frame`. The row refuses HERE, at the capability gate,
            # rather than paying every decode and then encoding frame zero while discarding the rest of the sequence.
            return Error(RasterFault(codec=fmt))
        case _:
            return _writer(fmt, RasterEngine.PILLOW).bind(lambda emit: _pillow_guarded(partial(work, emit)))


def _contact(payload: bytes, columns: int, cell: Pixels, fmt: ConvertFormat) -> Result[RasterFact, RasterFault]:
    def work(emit: CodecEmit) -> RasterFact:
        # filmstrip contact sheet: ImageSequence.Iterator walks every frame of an animated GIF/APNG/WebP or multi-page
        # TIFF and `_grid` tiles each into one grid — the multi-frame READ inverse of Sequence's multi-frame WRITE
        with Image.open(BytesIO(payload)) as image:
            tiles = [frame.copy() for frame in ImageSequence.Iterator(image)]
        return _grid(tiles, columns, cell, fmt, emit, frozendict({"frames": float(len(tiles))}))

    return _writer(fmt, RasterEngine.PILLOW).bind(lambda emit: _pillow_guarded(partial(work, emit)))


def _quantized(payload: bytes, colors: int, method: QuantizeMethod, dither: DitherMode, fmt: ConvertFormat) -> Result[RasterFact, RasterFault]:
    def work(emit: CodecEmit) -> RasterFact:
        # indexed-color small-file export — Image.quantize over the QuantizeMethod/DitherMode vocab resolved to the PIL enum by name; subsumes convert(palette=ADAPTIVE)
        source = ImageOps.exif_transpose(Image.open(BytesIO(payload)))
        rgb = (
            source if source.mode in {"RGB", "RGBA", "L"} else source.convert("RGB")
        )  # quantize admits only RGB/RGBA/L; a P/CMYK/I;16 source flattens to RGB first
        indexed = rgb.quantize(colors=colors, method=Image.Quantize[method.name], dither=Image.Dither[dither.name])
        return RasterFact(
            _pillow_bytes(indexed, fmt, emit, _QUALITY, _EFFORT), *indexed.size, frozendict({"colors": float(colors), "palette": method.value})
        )

    return _writer(fmt, RasterEngine.PILLOW).bind(lambda emit: _pillow_guarded(partial(work, emit)))


def _children(payload: bytes, index: int, fmt: ConvertFormat) -> Result[RasterFact, RasterFault]:
    def work(emit: CodecEmit) -> RasterFact:
        # embedded-thumbnail / multi-resolution sub-image extract via get_child_images — the preview a fresh decode would miss; an index past the count raises IndexError -> bounds
        with Image.open(BytesIO(payload)) as image:
            children = image.get_child_images()
            if not 0 <= index < len(children):
                raise IndexError(f"child {index} of {len(children)}")
            child = children[index]
            return RasterFact(
                _pillow_bytes(child, fmt, emit, _QUALITY, _EFFORT), *child.size, frozendict({"child": float(index), "children": float(len(children))})
            )

    return _writer(fmt, RasterEngine.PILLOW).bind(lambda emit: _pillow_guarded(partial(work, emit)))


@dataclass(frozen=True, slots=True, kw_only=True)
class EngineOps:
    thumbnail: Callable[[bytes, Pixels, ConvertFormat, FitMode], Result[RasterFact, RasterFault]]
    convert: Callable[[bytes, ConvertFormat, int, int], Result[RasterFact, RasterFault]]
    crop: Callable[[bytes, Box, ConvertFormat], Result[RasterFact, RasterFault]]
    probe: Callable[[bytes], Result[RasterFact, RasterFault]]


_ENGINE: Final[frozendict[RasterEngine, EngineOps]] = frozendict({
    RasterEngine.PILLOW: EngineOps(thumbnail=_thumbnail_pillow, convert=_convert_pillow, crop=_crop_pillow, probe=_probe_pillow),
    RasterEngine.LIBVIPS: EngineOps(thumbnail=_thumbnail_libvips, convert=_convert_libvips, crop=_crop_libvips, probe=_probe_libvips),
})
_CODEC: Final[frozendict[ConvertFormat, CodecRow]] = frozendict({
    # ONE row per display container. The writer column an engine holds IS that engine's capability claim, probed against
    # its linked build; an engine with no column writes the container nowhere and `_writer` faults `codec` for it.
    ConvertFormat.PNG: CodecRow(
        writers=frozendict({
            RasterEngine.PILLOW: _pillow_writer("PNG", None, lambda quality, effort: frozendict({"optimize": True})),
            RasterEngine.LIBVIPS: _vips_writer(".png", lambda quality, effort: frozendict({"compression": effort})),
        }),
        bands=BandLaw.FULL,
        frames=FrameLaw.DISPOSED,  # APNG: the pillow plugin composes duration, loop, and per-frame disposal through save_all
    ),
    ConvertFormat.JPEG: CodecRow(
        writers=frozendict({
            RasterEngine.PILLOW: _pillow_writer("JPEG", None, lambda quality, effort: frozendict({"quality": quality, "optimize": True})),
            RasterEngine.LIBVIPS: _vips_writer(".jpg", lambda quality, effort: frozendict({"Q": quality})),
        }),
        bands=BandLaw.OPAQUE,
        frames=FrameLaw.SINGLE,
    ),
    ConvertFormat.WEBP: CodecRow(
        writers=frozendict({
            RasterEngine.PILLOW: _pillow_writer("WEBP", "webp", lambda quality, effort: frozendict({"quality": quality, "method": effort})),
            RasterEngine.LIBVIPS: _vips_writer(".webp", lambda quality, effort: frozendict({"Q": quality, "effort": effort})),
        }),
        bands=BandLaw.FULL,
        frames=FrameLaw.LOOPED,
    ),
    ConvertFormat.AVIF: CodecRow(
        writers=frozendict({
            RasterEngine.PILLOW: _pillow_writer("AVIF", "avif", lambda quality, effort: frozendict({"quality": quality, "speed": effort})),
            RasterEngine.LIBVIPS: _vips_writer(".avif", lambda quality, effort: frozendict({"Q": quality, "effort": effort})),
        }),
        bands=BandLaw.FULL,
        frames=FrameLaw.TIMED,  # the AVIF sequence carries per-frame duration and no loop count
    ),
    ConvertFormat.GIF: CodecRow(
        writers=frozendict({
            # timing rides Sequence, palette rides Quantize
            RasterEngine.PILLOW: _pillow_writer("GIF", None, lambda quality, effort: frozendict({"optimize": True})),
            RasterEngine.LIBVIPS: _vips_writer(".gif", lambda quality, effort: frozendict({"effort": effort})),
        }),
        bands=BandLaw.FULL,
        frames=FrameLaw.DISPOSED,
    ),
    ConvertFormat.TIFF: CodecRow(
        writers=frozendict({
            RasterEngine.PILLOW: _pillow_writer("TIFF", None, lambda quality, effort: frozendict({"compression": "tiff_lzw"})),
            RasterEngine.LIBVIPS: _vips_writer(".tif", lambda quality, effort: frozendict({"compression": "lzw"})),
        }),
        bands=BandLaw.FULL,
        frames=FrameLaw.PAGES,  # the directory composes through save_all and carries no per-frame clock
    ),
    ConvertFormat.BMP: CodecRow(
        writers=frozendict({
            RasterEngine.PILLOW: _pillow_writer("BMP", None, lambda quality, effort: frozendict()),
            # libvips ships loaders for BMP and no saver at any build, so the fused surface egresses through the
            # array writer rather than refusing a container the estate's own codec substrate writes
            RasterEngine.LIBVIPS: _array_writer(lambda: imagecodecs.BMP.available, lambda frame, quality, effort: imagecodecs.bmp_encode(frame)),
        }),
        # the WEAKEST writer's reach, stated per writer: `imagecodecs.bmp_encode` carries RGBA on the array leg
        # while the pillow saver is RGB-only, and one band column serves both — so the row flattens for BOTH and
        # an alpha-bearing egress routes through PNG or QOI rather than shipping a container one leg would refuse
        bands=BandLaw.OPAQUE,
        frames=FrameLaw.SINGLE,
    ),
    ConvertFormat.JXL: CodecRow(
        # JPEG XL at DISPLAY depth: neither pillow nor libvips links a JXL saver, so one array writer serves both working
        # surfaces off the shared 8-bit `Frame`. `level` is the 0-100 quality coordinate every other row already carries
        # and `effort` the libjxl encode effort; the 16-bit, half, and float JXL lanes are the deep-pixel plane's alone.
        writers=_shared(
            _array_writer(
                lambda: imagecodecs.JPEGXL.available,
                lambda frame, quality, effort: imagecodecs.jpegxl_encode(frame, level=quality, effort=effort),
            )
        ),
        bands=BandLaw.FULL,
        frames=FrameLaw.SINGLE,
    ),
    ConvertFormat.QOI: CodecRow(
        # Lossless byte-stream container: fixed 8-bit RGB/RGBA by SPECIFICATION, so it carries no quality or effort
        # coordinate on either leg and the row's band law promotes a gray plane before either encoder sees it — pillow
        # answers one with `Unsupported QOI image mode` and imagecodecs with `photometric 1 not supported`, and both are
        # that same container fact the row states once. Pillow links its own saver; libvips registers no `.qoi` suffix at
        # any build, so the fused surface takes the array leg — the inverse column shape BMP carries.
        writers=frozendict({
            RasterEngine.PILLOW: _pillow_writer("QOI", None, lambda quality, effort: frozendict()),
            RasterEngine.LIBVIPS: _array_writer(lambda: imagecodecs.QOI.available, lambda frame, quality, effort: imagecodecs.qoi_encode(frame)),
        }),
        bands=BandLaw.COLOR,
        frames=FrameLaw.SINGLE,
    ),
})
```

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Raster production flow
    accDescr: Raster.emit fanning one ArtifactWork per member, op normalization and admission, the engine-polymorphic worker arms, codec resolution across the pillow, libvips, and imagecodecs writers, and the RasterFact egress into ArtifactReceipt.Preview.
    Emit["Raster.emit: one ArtifactWork per member, lane bound into the thunk"] --> Norm["_normalized(ops) -> Block[RasterOp]"]
    Norm --> Admit["RasterOp.admitted -> typed empty / extent / arity / range / reference / policy faults"]
    Admit --> Member["per-member _emit(op, lane)"]
    Member -->|"detect (delegated, in-process)"| Det["Detect(lane, PUREMAGIC).of(Source.Buffer) -> DetectIdentity -> _detected"]
    Member -->|"every other op"| Cross["lane.offload(Kernel.of(_worker_raster, HOSTILE))"]
    Cross -->|"worker death / BeartypeCallHintViolation"| Runtime["runtime BoundaryFault (lanes guard + CLASSIFY api row)"]
    Cross --> Worker["@beartype(conf=FAULT_CONF) _worker_raster match"]
    Worker -->|"ImportError / dlopen OSError"| Prov["RasterFault.provision"]
    Worker --> Row["every producing arm: _writer(codec, engine) -> _CODEC row's CodecEmit"]
    Row -->|"no column for the engine / pillow feature absent / libvips trial write refuses"| Codec["RasterFault.codec"]
    Row -->|"native"| Band["_moded: row BandLaw -> admitted mode (flatten | promote)"]
    Row -->|"array"| Band
    Band --> Egress["_pillow_bytes (arity-polymorphic save_all) | _vips_bytes (ForeignKeep ICC/EXIF/XMP)"]
    Worker --> Eng["probe / thumbnail / convert / crop -> _ENGINE[engine] (FitMode)"]
    Worker --> Mont["montage -> _montage (pillow _grid paste | libvips arrayjoin)"]
    Worker --> Comp["composite -> _composite (libvips composite2 BlendMode)"]
    Worker --> Sc["smartcrop -> _smartcrop (libvips smartcrop CropFocus)"]
    Worker --> Py["pyramid -> _pyramid (libvips dzsave_buffer PyramidLayout -> zip)"]
    Worker --> Ge["geometry -> _geometry (pillow transpose/rotate/transform/reduce)"]
    Worker --> Df["deframe -> _deframe (pillow seek | libvips page=)"]
    Worker --> Seq["sequence -> _sequence (FrameLaw rung gate, then _CLOCK keys through save_all)"]
    Seq -->|"FrameLaw.SINGLE"| Codec
    Worker --> Ct["contact -> _contact (ImageSequence.Iterator frames -> _grid)"]
    Ct --> Gridn["_grid: one composed-extent bomb ceiling for both tiled arms"]
    Mont --> Gridn
    Gridn --> Egress
    Worker --> Qz["quantize -> _quantized (pillow Image.quantize palette)"]
    Worker --> Ch["children -> _children (pillow get_child_images)"]
    Worker --> Tx["transform -> _transformed (image/reference/mask + img_as_ubyte)"]
    Worker --> Gn["generate -> _generated (source payload, no decode)"]
    Tx --> Proc["graphic/raster/process: produced-raster families"]
    Tx --> Meas["graphic/raster/measure: measured-score families"]
    Gn --> Proc
    Eng --> Fact["Result[RasterFact, RasterFault]"]
    Mont --> Fact
    Comp --> Fact
    Sc --> Fact
    Py --> Fact
    Ge --> Fact
    Df --> Fact
    Seq --> Fact
    Qz --> Fact
    Ch --> Fact
    Proc --> Fact
    Meas --> Fact
    Egress --> Fact
    Codec --> Rail
    Prov --> Rail
    Runtime --> Rail
    Det -->|"_detected -> ArtifactReceipt.Preview"| Rail
    Fact --> Preview["_previewed -> ArtifactReceipt.Preview(key, width, height, bytes_, score)"]
    Preview --> Rail["per-member RuntimeRail[ArtifactReceipt]"]
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
