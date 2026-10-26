"""Image pixels, readings, generated images, and proxies."""

from collections.abc import Callable
import math
from pathlib import Path
from typing import Annotated

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from blender.bpy.data.ids.ownership import kept, Ownership
from blender.bpy.data.ids.store import vacant
from blender.bpy.data.rna.descriptors import assigned, Descriptor, written
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.identity import Identity, Name
from blender.bpy.faults import BlenderAbsentFault, BlenderEditFault, BlenderFaults, BlenderResult
from blender.imaging.color.processors import applied, processor, unassociated
from blender.imaging.faults import Access, ImagingBufferFault, ImagingFileFault, ImagingResult, paired
from blender.imaging.files.images import Depth, Format, ImageFile, read_file, Storage
from blender.imaging.hashing import content_hash, ContentHash
from blender.imaging.parameters import Case

# --- [TYPES] ----------------------------------------------------------------------------

type Downscale = Scaled | Bounded

# --- [MODELS] ---------------------------------------------------------------------------


class ImageFrames(msgspec.Struct, frozen=True, kw_only=True):
    """Frame window of image users and compositor image nodes."""

    frame_start: int = 1
    frame_duration: Annotated[int, msgspec.Meta(ge=1)] = 1
    frame_offset: int = 0

    def applied(self, user: bpy.types.ImageUser | bpy.types.CompositorNodeImage) -> BlenderResult[None]:
        """Writes frame fields, accumulating faults."""
        return assigned(user, msgspec.structs.asdict(self))


class Tag(msgspec.Struct, frozen=True, kw_only=True):
    """Requested image reading, absent values keeping image settings."""

    space: Option[str] = Nothing
    alpha: Option[str] = Nothing
    view_as_render: Option[bool] = Nothing
    sequence: bool = False

    def applied(self, image: bpy.types.Image) -> BlenderResult[None]:
        """Writes sequence source, then color and display members, in host order, accumulating faults."""
        return assigned(
            image,
            dict((
                *((("source", "SEQUENCE"),) if self.sequence else ()),
                *(("colorspace_settings.name", space) for space in self.space.to_list()),
                *(("alpha_mode", alpha) for alpha in self.alpha.to_list()),
                *(("use_view_as_render", shown) for shown in self.view_as_render.to_list()),
            )),
        )


class Scaled(Case, frozen=True, kw_only=True, tag=1):
    """Proxy at fraction of each source side."""

    factor: Annotated[float, msgspec.Meta(gt=0.0, lt=1.0, description="Proxy side over source side")] = 0.5

    def fitted(self, width: int, height: int) -> Option[tuple[int, int]]:
        """Returns each scaled side rounded up to whole pixel."""
        return Some((math.ceil(width * self.factor), math.ceil(height * self.factor)))


class Bounded(Case, frozen=True, kw_only=True, tag=2):
    """Longest-side proxy bound."""

    title = "Longest Side"
    pixels: Annotated[int, msgspec.Meta(ge=1, le=2**31 - 1, description="Longest proxy side")] = 2048

    def fitted(self, width: int, height: int) -> Option[tuple[int, int]]:
        """Returns sides rounded up, or Nothing for source within bound."""
        longest = max(width, height)
        fitted_width, fitted_height = ((side * self.pixels + longest - 1) // longest for side in (width, height))
        return Some((fitted_width, fitted_height)) if longest > self.pixels else Nothing


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [LOADS]


def loaded(path: str, tag: Tag) -> BlenderResult[bpy.types.Image]:
    """Loads or reuses file image, then applies tag."""
    return BlenderEditFault.attempted(path, lambda: bpy.data.images.load(path, check_existing=True)).bind(lambda image: tag.applied(image).map(lambda _: image))


# --- [PIXELS]


def fingerprinted(image: bpy.types.Image) -> BlenderResult[Option[ContentHash]]:
    """Hashes image settings, pixels, and packed bytes.

    Args:
        image: Image with current buffers as content.

    Returns:
        Content hash, Nothing for movie, sequence, tiled, multilayer, render result, multiview, empty, or unloaded viewer image.
    """
    if (source := (image.source)) in {"MOVIE", "SEQUENCE", "TILED"} or (source == "VIEWER" and not image.has_data):
        return Ok(Nothing)
    if not np.asarray(image.size).prod() or image.type in {"MULTILAYER", "RENDER_RESULT"} or image.is_multiview:
        return Ok(Nothing)
    members = Descriptor.saved(image, lambda prop: prop != bpy.types.Image.bl_rna.properties["pixels"])
    buffers = Snapshot.of(bpy.types.Image, {"type": image.type, "is_float": image.is_float})
    return paired(_plane(image), Snapshot.capture(image, Some(members))).map(
        lambda pair: Some(content_hash(bpy.types.Image, pair[1], buffers, arrays=(pair[0], *(np.frombuffer(part.packed_file.data, np.uint8) for part in image.packed_files))))  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
    )


def read(image: bpy.types.Image, config: PyOpenColorIO.Config) -> BlenderResult[ImageFile[np.float32]]:
    """Returns associated float32 pixels top row first, absent fault when empty."""
    settings = image.colorspace_settings

    def formed(pixels: npt.NDArray[np.float32]) -> ImageFile[np.float32]:
        plane = np.ascontiguousarray(pixels[::-1])
        if _associated(image):
            np.multiply(plane[..., :3], plane[..., 3:], out=plane[..., :3], where=plane[..., 3:] != 0.0)
        return ImageFile(pixels=plane, colorspace=config.getColorSpace(PyOpenColorIO.ROLE_SCENE_LINEAR).getName() if image.is_float and not settings.is_data else settings.name)  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]

    return _plane(image).map(formed) if np.asarray(image.size).prod() else Error((BlenderAbsentFault(owner=repr(image), key=image.filepath),))


def filled(image: bpy.types.Image, plane: npt.NDArray[np.float32]) -> BlenderResult[None]:
    """Writes associated plane into pixels, widening gray and padding channels."""

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
    """Reuses equal-content image before reading plane, else packs plane into new image."""

    def built(values: npt.NDArray[np.float32]) -> BlenderResult[bpy.types.Image]:
        height, width, channels = values.shape

        return kept(
            BlenderEditFault.attempted(name, lambda: bpy.data.images.new(vacant(name, bpy.data.images.keys()), width, height, alpha=channels == 4, float_buffer=True)),
            lambda image: tag.applied(image).bind(lambda _: filled(image, values)).bind(lambda _: BlenderEditFault.attempted(repr(image), image.pack)).map(lambda _: image),
        )

    def owned(values: npt.NDArray[np.float32]) -> BlenderResult[Option[bpy.types.Image]]:
        return built(values).bind(lambda image: ownership.owned(identity, image, of=of, content=Some(content))).map(Some)

    return (
        (ownership.found(identity, of=of, content=Some(content)) if ownership.single else Nothing)
        .map(lambda image: Ok(Some(image)))
        .default_with(lambda: plane().bind(lambda values: values.map(owned).default_value(Ok(Nothing))))
    )


def _plane(image: bpy.types.Image) -> BlenderResult[npt.NDArray[np.float32]]:
    """Reads float32 pixels bottom row first as height, width, channels."""
    (width, height), channels = np.asarray(image.size), image.channels
    try:
        pixels = np.empty((height, width, channels), np.float32)
    except MemoryError as error:
        return Error((ImagingFileFault(access=Access.READ, request=repr(image), error=error),))
    image.pixels.foreach_get(pixels.reshape(-1))
    return Ok(pixels)


def _associated(image: bpy.types.Image) -> bool:
    """Whether image byte buffer stores straight color with alpha."""
    return not image.is_float and image.channels == 4 and not image.colorspace_settings.is_data and image.alpha_mode in {"STRAIGHT", "PREMUL"}  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]


# --- [PROXIES]


def proxied(image: bpy.types.Image, identity: Identity, downscale: Downscale, config: PyOpenColorIO.Config) -> BlenderResult[Option[str]]:
    """Points image at linear-light proxy beside source file, Nothing when source fits."""

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
            Result[str, BlenderFaults]
            .bind(resampled(file, width, height).bind(stored), lambda _: BlenderEditFault.attempted(key, lambda: image.__setitem__(key, source)))
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
                lambda row: Result[ImageFile[np.float32], BlenderFaults].bind(
                    read_file(source, config),
                    lambda file: downscale.fitted(file.pixels.shape[1], file.pixels.shape[0]).map(lambda size: proxy(key, source, row, file, *size).map(Some)).default_value(Ok(Nothing)),
                )
            )
            .default_with(lambda: Error((BlenderAbsentFault(owner="output_format_list", key=suffix),)))
        )

    return identity.name(Name.ATTACHMENT, "proxy").bind(sourced)


def restored(image: bpy.types.Image, identity: Identity) -> BlenderResult[None]:
    """Points image back at stored source path and removes proxy property."""

    def back(key: str) -> BlenderResult[None]:
        match image.get(key):
            case str(source):
                return written(image, "filepath", source).bind(lambda _: BlenderEditFault.attempted(key, lambda: image.pop(key)))
            case _:
                return Ok(None)

    return identity.name(Name.ATTACHMENT, "proxy").bind(back)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Bounded", "Downscale", "ImageFrames", "Scaled", "Tag", "filled", "fingerprinted", "generated", "loaded", "proxied", "read", "restored"]
