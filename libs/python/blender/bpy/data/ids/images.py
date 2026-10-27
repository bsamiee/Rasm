"""Image pixels, readings, generated images, and proxies."""

from collections.abc import Callable
import math
from pathlib import Path
import sys
from typing import Annotated

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.data.ids.ownership import kept, Ownership
from blender.bpy.data.rna.descriptors import assigned, Descriptor, written
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.identity import Identity
from blender.bpy.faults import BlenderReason
from blender.imaging.color.processors import converted
from blender.imaging.files.buffers import associated, resampled, unassociated
from blender.imaging.files.coding import Depth, Rounded
from blender.imaging.files.images import Format, ImageFile, read_file, Storage
from blender.records.faults import Fault, Faults, Outcome, paired
from blender.records.fields import Case
from blender.records.identity import content_hash, ContentHash

# --- [TYPES] ----------------------------------------------------------------------------

type Downscale = Scaled | Bounded

# --- [MODELS] ---------------------------------------------------------------------------


class ImageFrames(msgspec.Struct, frozen=True, kw_only=True):
    """Frame window of image users and compositor image nodes."""

    frame_start: int = 1
    frame_duration: Annotated[int, msgspec.Meta(ge=1)] = 1
    frame_offset: int = 0

    def applied(self, user: bpy.types.ImageUser | bpy.types.CompositorNodeImage) -> Outcome[None]:
        """Writes frame fields, accumulating faults."""
        return assigned(user, msgspec.structs.asdict(self))


class Tag(msgspec.Struct, frozen=True, kw_only=True):
    """Requested image reading, absent values keeping image settings."""

    space: Option[str] = Nothing
    alpha: Option[str] = Nothing
    view_as_render: Option[bool] = Nothing
    sequence: bool = False

    def applied(self, image: bpy.types.Image) -> Outcome[None]:
        """Writes sequence source, then color and display members, in host order, accumulating faults."""
        return assigned(
            image,
            dict((
                *((("source", "SEQUENCE"),) if self.sequence else ()),
                *((("colorspace_settings", "name"), space) for space in self.space.to_list()),
                *(("alpha_mode", alpha) for alpha in self.alpha.to_list()),
                *(("use_view_as_render", shown) for shown in self.view_as_render.to_list()),
            )),
        )


class Scaled(Case, frozen=True, kw_only=True):
    """Proxy at fraction of each source side."""

    factor: Annotated[float, msgspec.Meta(ge=sys.float_info.min, lt=1.0, description="Proxy side over source side")] = 0.5

    def fitted(self, width: int, height: int) -> Option[tuple[int, int]]:
        """Returns each scaled side rounded up to whole pixel."""
        return Some((math.ceil(width * self.factor), math.ceil(height * self.factor)))


class Bounded(Case, frozen=True, kw_only=True):
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


def loaded(path: str, tag: Tag) -> Outcome[bpy.types.Image]:
    """Loads or reuses file image, then applies tag."""
    return Result[bpy.types.Image, Faults].bind(
        catch(bpy.data.images.load, exception=RuntimeError)(path, check_existing=True).map_error(lambda error: (Fault(reason=BlenderReason.REFUSED, subject=path, cause=error),)),
        lambda image: tag.applied(image).map(lambda _: image),
    )


# --- [PIXELS]


def fingerprinted(image: bpy.types.Image) -> Outcome[Option[ContentHash]]:
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


def read(image: bpy.types.Image, config: PyOpenColorIO.Config) -> Outcome[ImageFile[np.float32]]:
    """Returns associated float32 pixels top row first, absent fault when empty."""
    settings = image.colorspace_settings
    space = config.getColorSpace(PyOpenColorIO.ROLE_SCENE_LINEAR).getName() if image.is_float and not settings.is_data else settings.name  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]

    def formed(pixels: npt.NDArray[np.float32]) -> ImageFile[np.float32]:
        plane = np.ascontiguousarray(pixels[::-1])
        return ImageFile(pixels=associated(plane) if _associated(image) else plane, colorspace=space)

    return _plane(image).map(formed) if np.asarray(image.size).prod() else Error((Fault(reason=BlenderReason.ABSENT, subject=repr(image), member=image.filepath),))


def filled(image: bpy.types.Image, plane: npt.NDArray[np.float32]) -> Outcome[None]:
    """Writes associated plane into pixels, widening gray and padding channels."""
    straight = unassociated(plane) if _associated(image) else plane
    color = np.repeat(straight, 3, axis=-1) if straight.shape[2] == 1 else straight
    pixels = np.pad(color, ((0, 0), (0, 0), (0, image.channels - color.shape[2])))
    if color.shape[2] < 4 and image.channels == 4:
        pixels[..., 3] = 1.0
    image.pixels.foreach_set(pixels[::-1].reshape(-1))  # type: ignore[arg-type]
    image.update()
    return Ok(None)


def generated(
    ownership: Ownership[bpy.types.Image],
    identity: Identity,
    name: str,
    content: ContentHash,
    plane: Callable[[], Outcome[Option[npt.NDArray[np.float32]]]],
    tag: Tag,
    *,
    of: Option[bpy.types.ID] = Nothing,
) -> Outcome[Option[bpy.types.Image]]:
    """Reuses equal-content image before reading plane, else packs plane into new image."""

    def built(values: npt.NDArray[np.float32]) -> Outcome[bpy.types.Image]:
        height, width, channels = values.shape

        return kept(
            catch(bpy.data.images.new, exception=RuntimeError)(name, width, height, alpha=channels == 4, float_buffer=True).map_error(
                lambda error: (Fault(reason=BlenderReason.REFUSED, subject=name, cause=error),)
            ),
            lambda image: (
                tag
                .applied(image)
                .bind(lambda _: filled(image, values))
                .bind(lambda _: catch(image.pack, exception=RuntimeError)().map_error(lambda error: (Fault(reason=BlenderReason.REFUSED, subject=repr(image), cause=error),)))
                .map(lambda _: image)
            ),
        )

    def owned(values: npt.NDArray[np.float32]) -> Outcome[Option[bpy.types.Image]]:
        return built(values).bind(lambda image: ownership.owned(identity, image, of=of, content=Some(content))).map(Some)

    return (
        (ownership.found(identity, of=of, content=Some(content)) if ownership.single else Nothing)
        .map(lambda image: Ok(Some(image)))
        .default_with(lambda: plane().bind(lambda values: values.map(owned).default_value(Ok(Nothing))))
    )


def _plane(image: bpy.types.Image) -> Outcome[npt.NDArray[np.float32]]:
    """Reads float32 pixels bottom row first as height, width, channels."""
    (width, height), channels = np.asarray(image.size), image.channels
    pixels = np.empty((height, width, channels), np.float32)
    image.pixels.foreach_get(pixels.reshape(-1))
    return Ok(pixels)


def _associated(image: bpy.types.Image) -> bool:
    """Whether image byte buffer stores straight color with alpha."""
    return not image.is_float and image.channels == 4 and not image.colorspace_settings.is_data and image.alpha_mode in {"STRAIGHT", "PREMUL"}  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]


# --- [PROXIES]


def proxied(image: bpy.types.Image, identity: Identity, downscale: Downscale, config: PyOpenColorIO.Config) -> Outcome[Option[str]]:
    """Points image at linear-light proxy beside source file, Nothing when source fits."""

    def linear(file: ImageFile[np.float32], width: int, height: int) -> Outcome[npt.NDArray[np.float32]]:
        def moved(values: npt.NDArray[np.float32], direction: PyOpenColorIO.TransformDirection) -> Outcome[npt.NDArray[np.float32]]:
            transform = PyOpenColorIO.ColorSpaceTransform(src=file.colorspace, dst=PyOpenColorIO.ROLE_SCENE_LINEAR, direction=direction)
            return converted(config, transform, values) if values.shape[2] in {3, 4} else Ok(values)

        return moved(file.pixels, PyOpenColorIO.TRANSFORM_DIR_FORWARD).bind(lambda values: resampled(values, (width, height))).bind(lambda values: moved(values, PyOpenColorIO.TRANSFORM_DIR_INVERSE))

    def proxy(key: str, source: str, row: Format, file: ImageFile[np.float32], width: int, height: int) -> Outcome[str]:
        origin = Path(source)
        path, depth = str(origin.with_stem(f"{origin.stem}.{width}x{height}")), row.depths[-1] if image.is_float else Depth.UINT8

        def stored(values: npt.NDArray[np.float32]) -> Outcome[str]:
            match depth:
                case Depth.UINT8 | Depth.UINT16:
                    return ImageFile(pixels=depth.coded(unassociated(values), Rounded()), colorspace=file.colorspace, header=file.header).write(path, Storage(), {}, config)
                case Depth.HALF | Depth.FLOAT as floated:
                    return ImageFile(pixels=values, colorspace=file.colorspace, header=file.header).write(path, Storage(depth=16 if floated is Depth.HALF else 32), {}, config)

        return linear(file, width, height).bind(stored).map(lambda _: image.__setitem__(key, source)).bind(lambda _: written(image, "filepath", path).map(lambda _: path))

    def sourced(key: str) -> Outcome[Option[str]]:
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
                lambda row: Result[ImageFile[np.float32], Faults].bind(
                    read_file(source, config),
                    lambda file: downscale.fitted(file.pixels.shape[1], file.pixels.shape[0]).map(lambda size: proxy(key, source, row, file, *size).map(Some)).default_value(Ok(Nothing)),
                )
            )
            .default_with(lambda: Error((Fault(reason=BlenderReason.ABSENT, subject="output_format_list", member=suffix),)))
        )

    return identity.identifier("proxy").bind(sourced)


def restored(image: bpy.types.Image, identity: Identity) -> Outcome[None]:
    """Points image back at stored source path and removes proxy property."""

    def back(key: str) -> Outcome[None]:
        match image.get(key):
            case str(source):
                return written(image, "filepath", source).map(lambda _: image.pop(key)).map(lambda _: None)
            case _:
                return Ok(None)

    return identity.identifier("proxy").bind(back)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Bounded", "Downscale", "ImageFrames", "Scaled", "Tag", "filled", "fingerprinted", "generated", "loaded", "proxied", "read", "restored"]
