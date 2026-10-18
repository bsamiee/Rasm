"""Native image pixels, file readings, owned images, proxies, and render-result export."""

from collections.abc import Callable, Generator
from contextlib import contextmanager, ExitStack
from functools import reduce
from itertools import starmap
import math
from pathlib import Path
from typing import Annotated, Literal

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from blender.bpy.data.ids.ownership import kept, Ownership
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.descriptors import Descriptor, Value, written
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.events.handlers import PathHandler, SceneHandler
from blender.bpy.extension.identity import Identity, Name
from blender.bpy.extension.registration import Acquisition, Band, entered
from blender.bpy.faults import BlenderAbsentFault, BlenderEditFault, BlenderFault, BlenderFaults, BlenderImagingFault, BlenderReadFault, BlenderResult
from blender.imaging.color.processors import applied, DisplayView, processor, unassociated
from blender.imaging.faults import accumulated, Faults, ImagingBufferFault, ImagingResult, ImagingWriteFault, scoped
from blender.imaging.files.images import Depth, Format, ImageFile, published, read_file, Storage
from blender.imaging.hashing import content_hash, ContentHash
from blender.imaging.parameters import Case

# --- [TYPES] ----------------------------------------------------------------------------

type Downscale = Scaled | Bounded

# --- [MODELS] ---------------------------------------------------------------------------


class ImageFrames(msgspec.Struct, frozen=True, kw_only=True):
    """Native frame window shared by sequence users and compositor image nodes."""

    frame_start: int = 1
    frame_duration: Annotated[int, msgspec.Meta(ge=1)] = 1
    frame_offset: int = 0

    def applied(self, user: bpy.types.ImageUser | bpy.types.CompositorNodeImage) -> BlenderResult[None]:
        """Writes frame fields, retaining every independent RNA refusal."""
        return accumulated(written(user, name, value) for name, value in msgspec.structs.asdict(self).items()).map(lambda _: None)


class Production(msgspec.Struct, frozen=True, kw_only=True):
    """Completed image's producing scene, actual encoding, and native result layer."""

    scene: Handle[bpy.types.Scene]
    space: str
    layer: Option[str] = Nothing
    view: Option[tuple[DisplayView, float, float]] = Nothing


class Tag(msgspec.Struct, frozen=True, kw_only=True):
    """Requested reading, with absent values retaining the native image's settings."""

    space: Option[str] = Nothing
    alpha: Option[str] = Nothing
    view_as_render: Option[bool] = Nothing
    sequence: bool = False

    def applied(self, image: bpy.types.Image) -> BlenderResult[None]:
        """Sets the source before accumulating independent color and display writes."""
        source = written(image, "source", "SEQUENCE").map(lambda _: None) if self.sequence else Ok(None)
        return source.bind(
            lambda _: accumulated(
                starmap(
                    written,
                    (
                        *((image.colorspace_settings, "name", value) for value in self.space.to_list()),
                        *((image, "alpha_mode", value) for value in self.alpha.to_list()),
                        *((image, "use_view_as_render", value) for value in self.view_as_render.to_list()),
                    ),
                )
            ).map(lambda _: None)
        )


class Scaled(Case, frozen=True, kw_only=True, tag=1):
    """Proxy at a fraction of each source side."""

    factor: Annotated[float, msgspec.Meta(gt=0.0, lt=1.0, description="Proxy side over source side")] = 0.5

    def fitted(self, width: int, height: int) -> Option[tuple[int, int]]:
        """Returns each scaled side rounded up to a whole pixel."""
        return Some((math.ceil(width * self.factor), math.ceil(height * self.factor)))


class Bounded(Case, frozen=True, kw_only=True, tag=2):
    """Proxy bounded by its longest side."""

    title = "Longest Side"
    pixels: Annotated[int, msgspec.Meta(ge=1, le=2**31 - 1, description="Longest proxy side")] = 2048

    def fitted(self, width: int, height: int) -> Option[tuple[int, int]]:
        """Returns exact upward-rounded sides, or absence for a source within the bound."""
        longest = max(width, height)
        fitted_width, fitted_height = ((side * self.pixels + longest - 1) // longest for side in (width, height))
        return Some((fitted_width, fitted_height)) if longest > self.pixels else Nothing


class ResultRead(msgspec.Struct, frozen=True, kw_only=True):
    """Completed-render consumer receiving a multilayer file on the main thread."""

    consume: Callable[[bpy.types.Scene, str], BlenderResult[Option[str]]]

    def acquisitions(self, identity: Identity) -> BlenderResult[tuple[tuple[Band, Acquisition], ...]]:
        """Returns the native completion handler using reports' deferred resource scope."""

        def consumed(handle: Handle[bpy.types.Scene]) -> BlenderResult[Option[str]]:
            return handle.resolve().bind(lambda scene: saved(scene).bind(lambda path: self.consume(scene, path)))

        @contextmanager
        def acquired() -> Generator[BlenderResult[None]]:
            """Retires deferred writes after removing their native completion handler."""
            with ExitStack() as stack:
                deferred = stack.enter_context(identity.reports.deferred(consumed))
                yield (
                    SceneHandler(handlers=bpy.app.handlers.render_complete, body=lambda scene: Ok(None) if scene.session_uid in intermediates else Ok(deferred(Handle.of(scene))))
                    .acquisitions(identity)
                    .bind(lambda pairs: entered(stack, tuple(acquisition for _, acquisition in pairs)))
                )

        return Ok(((Band.APP_HANDLERS, acquired()),))


# --- [SERVICES] -------------------------------------------------------------------------


class Productions(msgspec.Struct, frozen=True, eq=False):
    """Completed image facts keyed by image session identity and displayed render slot index, which undo keeps and a file load replaces."""

    held: dict[tuple[int, int], Option[Production]] = msgspec.field(default_factory=dict)

    @staticmethod
    def key(image: bpy.types.Image) -> tuple[int, int]:
        """Returns the image's session identity and displayed render slot index."""
        return image.session_uid, image.render_slots.active_index  # type: ignore[return-value]  # ty: ignore[invalid-return-type]

    def acquisitions(self, identity: Identity) -> BlenderResult[tuple[tuple[Band, Acquisition], ...]]:
        """Builds the load handler forgetting the replaced images' facts."""

        def forgotten(_: Option[Path]) -> BlenderResult[None]:
            self.held.clear()
            return Ok(None)

        return PathHandler(handlers=bpy.app.handlers.load_post, body=forgotten).acquisitions(identity)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]


intermediates: set[int] = set()
productions = Productions()


@contextmanager
def intermediate(scene: bpy.types.Scene) -> Generator[None]:
    """Holds native captures outside completed-image consumers for the render scope."""
    intermediates.add(scene.session_uid)
    try:
        yield
    finally:
        intermediates.remove(scene.session_uid)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [LOADS]


def produced(image: bpy.types.Image, production: Option[Production]) -> None:
    """Records production facts against a native image and its displayed slot."""
    productions.held[Productions.key(image)] = production


def production(image: bpy.types.Image) -> BlenderResult[Production]:
    """Returns completed production facts for the displayed native slot."""
    return productions.held.get(Productions.key(image), Nothing).to_result_with(lambda: (BlenderAbsentFault(owner=repr(image), key="completed image formation and encoding"),))


def loaded(path: str, tag: Tag) -> BlenderResult[bpy.types.Image]:
    """Loads or reuses the native file image, then applies its requested reading."""
    return BlenderEditFault.attempted(path, lambda: bpy.data.images.load(path, check_existing=True)).bind(lambda image: tag.applied(image).map(lambda _: image))


# --- [PIXELS]


def fingerprinted(image: bpy.types.Image) -> BlenderResult[Option[ContentHash]]:
    """Hashes complete observable image content, retaining unobservable selection as absence.

    Args:
        image: Native image whose current buffers supply graph content.

    Returns:
        Persistent settings, raw shaped pixels, and packed bytes as a content
        digest, or absence for frame, tile, view, layer, or unavailable buffers.
        Native capture faults retain their Blender variants.
    """
    if (source := (image.source)) in {"MOVIE", "SEQUENCE", "TILED"} or (source == "VIEWER" and not image.has_data):
        return Ok(Nothing)
    (width, height), channels = np.asarray(image.size), image.channels
    if not width * height or image.type in {"MULTILAYER", "RENDER_RESULT"} or image.is_multiview:
        return Ok(Nothing)
    try:
        pixels = np.empty((height, width, channels), np.float32)
        image.pixels.foreach_get(pixels.reshape(-1))
    except MemoryError as error:
        return Error((BlenderReadFault(request=repr(image), error=error),))
    members = tuple(Descriptor(root=type(image), owner="", prop=prop) for prop in Snapshot.members(image.bl_rna) if prop != bpy.types.Image.bl_rna.properties["pixels"])  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
    buffers = Snapshot.of(bpy.types.Image, {"type": image.type, "is_float": image.is_float})
    return Snapshot.capture(image, Some(members)).map(
        lambda snapshot: Some(content_hash(bpy.types.Image, snapshot, buffers, arrays=(pixels, *(np.frombuffer(part.packed_file.data, np.uint8) for part in image.packed_files))))  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
    )


def read(image: bpy.types.Image, config: PyOpenColorIO.Config) -> BlenderResult[ImageFile[np.float32]]:
    """Returns associated float32 pixels top first and their native buffer colorspace."""
    (width, height), channels, settings = np.asarray(image.size), image.channels, image.colorspace_settings
    if not width * height:
        return Error((BlenderAbsentFault(owner=repr(image), key=image.filepath),))
    try:
        flat = np.empty(width * height * channels, np.float32)
        image.pixels.foreach_get(flat)
        plane = np.ascontiguousarray(flat.reshape(height, width, channels)[::-1])
        if _associated(image):
            np.multiply(plane[..., :3], plane[..., 3:], out=plane[..., :3], where=plane[..., 3:] != 0.0)
    except MemoryError as error:
        return Error((BlenderReadFault(request=repr(image), error=error),))
    return Ok(ImageFile(pixels=plane, colorspace=config.getColorSpace(PyOpenColorIO.ROLE_SCENE_LINEAR).getName() if image.is_float and not settings.is_data else settings.name))  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]


def filled(image: bpy.types.Image, plane: npt.NDArray[np.float32]) -> BlenderResult[None]:
    """Writes associated pixels, scalar RGB, or XY data into the native image's components."""

    def write() -> None:
        straight = unassociated(plane) if _associated(image) else plane
        color = np.repeat(straight, 3, axis=-1) if straight.shape[2] == 1 else straight
        pixels = np.pad(color, ((0, 0), (0, 0), (0, image.channels - color.shape[2])))
        if color.shape[2] < 4 and image.channels == 4:
            pixels[..., 3] = 1.0
        image.pixels.foreach_set(pixels[::-1].reshape(-1))  # type: ignore[arg-type]
        image.update()

    return BlenderEditFault.attempted(repr(image), write)


def generated(
    ownership: Ownership[bpy.types.Image],
    identity: Identity,
    name: str,
    content: ContentHash,
    plane: Callable[[], BlenderResult[Option[npt.NDArray[np.float32]]]],
    tag: Tag,
    *,
    of: Option[bpy.types.ID] = Nothing,
) -> BlenderResult[Option[bpy.types.Image]]:
    """Reuses content before reading pixels, packing a present plane and retaining absence."""

    def built(values: npt.NDArray[np.float32]) -> BlenderResult[bpy.types.Image]:
        height, width, channels = values.shape

        return kept(
            BlenderEditFault.attempted(name, lambda: bpy.data.images.new(name, width, height, alpha=channels == 4, float_buffer=True)),
            lambda image: tag.applied(image).bind(lambda _: filled(image, values)).bind(lambda _: BlenderEditFault.attempted(repr(image), image.pack)).map(lambda _: image),
        )

    return ownership.made(identity, lambda: plane().bind(lambda values: values.map(lambda values: built(values).map(Some)).default_value(Ok(Nothing))), of=of, content=Some(content), optional=True)


def _associated(image: bpy.types.Image) -> bool:
    """Returns whether the native byte buffer stores straight color with alpha."""
    return not image.is_float and image.channels == 4 and not image.colorspace_settings.is_data and image.alpha_mode in {"STRAIGHT", "PREMUL"}  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]


# --- [PROXIES]


def proxied(image: bpy.types.Image, identity: Identity, downscale: Downscale, config: PyOpenColorIO.Config) -> BlenderResult[Option[str]]:
    """Publishes a linear-light proxy beside the original and switches the native image."""

    def resampled(file: ImageFile[np.float32], width: int, height: int) -> ImagingResult[npt.NDArray[np.float32]]:
        def linear(values: npt.NDArray[np.float32], direction: PyOpenColorIO.TransformDirection) -> ImagingResult[npt.NDArray[np.float32]]:
            transform = PyOpenColorIO.ColorSpaceTransform(src=file.colorspace, dst=PyOpenColorIO.ROLE_SCENE_LINEAR, direction=direction)
            return processor(config, transform).map(lambda built: applied(built.getDefaultCPUProcessor(), values)) if values.shape[2] in {3, 4} else Ok(values)

        def fitted(values: npt.NDArray[np.float32]) -> ImagingResult[npt.NDArray[np.float32]]:
            return (
                ImagingBufferFault
                .checked(OpenImageIO.ImageBuf(values))
                .map(lambda source: OpenImageIO.ImageBufAlgo.fit(source, filtername="", filterwidth=0.0, fillmode="letterbox", exact=True, roi=OpenImageIO.ROI(0, width, 0, height)))
                .bind(ImagingBufferFault.checked)
                .bind(ImagingBufferFault.pixels)
            )

        return linear(file.pixels, PyOpenColorIO.TRANSFORM_DIR_FORWARD).bind(fitted).bind(lambda values: linear(values, PyOpenColorIO.TRANSFORM_DIR_INVERSE))

    def proxy(key: str, source: str, row: Format, file: ImageFile[np.float32], width: int, height: int) -> BlenderResult[str]:
        origin = Path(source)
        path, depth = str(origin.with_stem(f"{origin.stem}.{width}x{height}")), row.depths[-1] if image.is_float else Depth.UINT8

        def stored(values: npt.NDArray[np.float32]) -> ImagingResult[str]:
            match depth:
                case Depth.UINT8 | Depth.UINT16:
                    return ImageFile(pixels=depth.coded(unassociated(values)), colorspace=file.colorspace, header=file.header).write(path, Storage(), {}, config)
                case Depth.HALF | Depth.FLOAT as floated:
                    return ImageFile(pixels=values, colorspace=file.colorspace, header=file.header).write(path, Storage(depth=16 if floated is Depth.HALF else 32), {}, config)

        return (
            resampled(file, width, height)
            .bind(stored)
            .map_error(BlenderImagingFault.lifted)
            .bind(lambda _: BlenderEditFault.attempted(key, lambda: image.__setitem__(key, source)))
            .bind(lambda _: written(image, "filepath", path).map(lambda _: path))
        )

    def sourced(key: str) -> BlenderResult[Option[str]]:
        match image.get(key):
            case str(stored):
                source = stored
            case _:
                source = bpy.path.abspath(image.filepath, library=image.library)
        suffix = Path(source).suffix
        return (
            Option
            .of_optional(next((row for row in Format.writable() if suffix.lower().removeprefix(".") in row.extensions), None))
            .map(
                lambda row: (
                    read_file(source, config)
                    .map_error(BlenderImagingFault.lifted)
                    .bind(lambda file: downscale.fitted(file.pixels.shape[1], file.pixels.shape[0]).map(lambda size: proxy(key, source, row, file, *size).map(Some)).default_value(Ok(Nothing)))
                )
            )
            .default_with(lambda: Error((BlenderAbsentFault(owner="output_format_list", key=suffix),)))
        )

    return identity.name(Name.ATTACHMENT, "proxy").bind(sourced)


def restored(image: bpy.types.Image, identity: Identity) -> BlenderResult[None]:
    """Restores a stored source path and removes its proxy property after the switch."""

    def back(key: str) -> BlenderResult[None]:
        match image.get(key):
            case str(source):
                return written(image, "filepath", source).bind(lambda _: BlenderEditFault.attempted(key, lambda: image.pop(key)))
            case _:
                return Ok(None)

    return identity.name(Name.ATTACHMENT, "proxy").bind(back)


# --- [BUILT]


def built(kind: Literal["RENDER_RESULT", "COMPOSITING"]) -> BlenderResult[bpy.types.Image]:
    """Returns the native image Blender generates of one kind, the Render Result or the shared compositor Viewer whose completion pixels must be read before deferral, or its absence before Blender builds it."""
    return Option.of_optional(next((image for image in bpy.data.images if image.type == kind), None)).to_result_with(lambda: (BlenderAbsentFault(owner=type(bpy.data.images).__name__, key=kind),))


def saved(scene: bpy.types.Scene) -> BlenderResult[str]:
    """Exports the scene's displayed result and all passes, restoring every direct member of its format, the members the export writes."""
    settings, path = scene.render.image_settings, str(Path(bpy.app.tempdir, f"{scene.session_uid}.exr"))
    form: dict[str, Value] = {"media_type": "MULTI_LAYER_IMAGE", "color_depth": "32", "exr_codec": "ZIP", "use_exr_interleave": False, "views_format": "MULTIVIEW", "color_management": "FOLLOW_SCENE"}

    def exported(image: bpy.types.Image) -> BlenderResult[str]:
        def encode(stage: str) -> BlenderResult[None]:
            return BlenderEditFault.attempted(repr(image), lambda: image.save_render(stage, scene=scene))

        def refused(faults: Faults[BlenderFault | ImagingWriteFault]) -> BlenderFaults:
            first, *rest = (BlenderImagingFault(faults=(fault,)) if isinstance(fault, ImagingWriteFault) else fault for fault in faults)
            return (first, *rest)

        return reduce(lambda state, member: state.bind(lambda _: written(settings, *member).map(lambda _: None)), form.items(), Result[None, BlenderFaults].Ok(None)).bind(
            lambda _: published(path, encode).map_error(refused)
        )

    direct = tuple(Descriptor(root=bpy.types.ImageFormatSettings, owner="", prop=prop) for prop in Snapshot.members(settings.bl_rna) if not isinstance(prop, bpy.types.PointerProperty))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
    return built("RENDER_RESULT").bind(lambda image: scoped(Snapshot.capture(settings, Some(direct)), lambda _: exported(image), lambda capture: capture.apply(settings).outcome))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Bounded",
    "Downscale",
    "ImageFrames",
    "Production",
    "Productions",
    "ResultRead",
    "Scaled",
    "Tag",
    "built",
    "filled",
    "fingerprinted",
    "generated",
    "intermediate",
    "intermediates",
    "loaded",
    "produced",
    "production",
    "productions",
    "proxied",
    "read",
    "restored",
    "saved",
]
