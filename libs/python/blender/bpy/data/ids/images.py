"""Image pixels, readings, and generated images."""

from collections.abc import Callable
from functools import partial
from typing import Annotated

import bpy
from expression import Error, Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.data.ids.ownership import kept, Ownership
from blender.bpy.data.rna.descriptors import assigned
from blender.bpy.extension.identity import Identity
from blender.bpy.faults import BlenderReason
from blender.imaging.files.buffers import associated, unassociated
from blender.imaging.files.images import ImageFile
from blender.records.faults import Fault, Outcome
from blender.records.identity import ContentHash

# --- [MODELS] ---------------------------------------------------------------------------


class ImageFrames(msgspec.Struct, frozen=True, kw_only=True):
    """Frame window of image users and compositor image nodes."""

    frame_start: int = 1
    frame_duration: Annotated[int, msgspec.Meta(ge=1)] = 1
    frame_offset: int = 0

    def applied(self, user: bpy.types.ImageUser | bpy.types.CompositorNodeImage) -> Outcome[None]:
        """Writes frame fields, accumulating faults."""
        return assigned(user, {("frame_start",): self.frame_start, ("frame_duration",): self.frame_duration, ("frame_offset",): self.frame_offset})


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
                *(((("source",), "SEQUENCE"),) if self.sequence else ()),
                *((("colorspace_settings", "name"), space) for space in self.space.to_list()),
                *((("alpha_mode",), alpha) for alpha in self.alpha.to_list()),
                *((("use_view_as_render",), shown) for shown in self.view_as_render.to_list()),
            )),
        )


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [LOADS]


def loaded(path: str, tag: Tag) -> Outcome[bpy.types.Image]:
    """Loads or reuses file image, then applies tag."""
    return (
        catch(bpy.data.images.load, exception=RuntimeError)(path, check_existing=True)
        .map_error(partial(Fault.of, BlenderReason.REFUSED, path))
        .bind(lambda image: tag.applied(image).map(lambda _: image))
    )


# --- [PIXELS]


def pixels(image: bpy.types.Image) -> Outcome[npt.NDArray[np.float32]]:
    """Reads float32 pixels bottom row first as height, width, channels, refusal fault for an image holding no buffer."""
    (width, height), channels = np.asarray(image.size), image.channels
    plane = np.empty((height, width, channels), np.float32)
    return catch(image.pixels.foreach_get, exception=TypeError)(plane.reshape(-1)).map_error(partial(Fault.of, BlenderReason.REFUSED, repr(image))).map(lambda _: plane)


def read(image: bpy.types.Image, config: PyOpenColorIO.Config) -> Outcome[ImageFile[np.float32]]:
    """Returns associated float32 pixels top row first, absent fault when empty."""
    settings: bpy.types.ColorManagedInputColorspaceSettings = image.colorspace_settings  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    space = (
        Option.of_optional(config.getColorSpace(PyOpenColorIO.ROLE_SCENE_LINEAR)).map(lambda held: held.getName()).default_value(settings.name)
        if image.is_float and not settings.is_data
        else settings.name
    )

    def formed(plane: npt.NDArray[np.float32]) -> ImageFile[np.float32]:
        flipped = np.ascontiguousarray(plane[::-1])
        return ImageFile(pixels=associated(flipped) if _associated(image) else flipped, colorspace=space)

    return pixels(image).map(formed) if np.asarray(image.size).prod() else Error(Fault.of(BlenderReason.ABSENT, repr(image), member=image.filepath))


def filled(image: bpy.types.Image, plane: npt.NDArray[np.float32]) -> Outcome[None]:
    """Writes associated plane into pixels, widening gray and padding channels."""
    straight = unassociated(plane) if _associated(image) else plane
    color = np.repeat(straight, 3, axis=-1) if straight.shape[2] == 1 else straight
    padded = np.pad(color, ((0, 0), (0, 0), (0, image.channels - color.shape[2])))
    if color.shape[2] < 4 and image.channels == 4:
        padded[..., 3] = 1.0
    return catch(image.pixels.foreach_set, exception=TypeError)(padded[::-1].reshape(-1)).map_error(partial(Fault.of, BlenderReason.REFUSED, repr(image))).map(lambda _: image.update())


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
            catch(bpy.data.images.new, exception=RuntimeError)(name, width, height, alpha=channels == 4, float_buffer=True).map_error(partial(Fault.of, BlenderReason.REFUSED, name)),
            lambda image: (
                tag
                .applied(image)
                .bind(lambda _: filled(image, values))
                .bind(lambda _: catch(image.pack, exception=RuntimeError)().map_error(partial(Fault.of, BlenderReason.REFUSED, repr(image))))
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


def _associated(image: bpy.types.Image) -> bool:
    """Whether image byte buffer stores straight color with alpha."""
    settings: bpy.types.ColorManagedInputColorspaceSettings = image.colorspace_settings  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    return not image.is_float and image.channels == 4 and not settings.is_data and image.alpha_mode in {"STRAIGHT", "PREMUL"}


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ImageFrames", "Tag", "filled", "generated", "loaded", "pixels", "read"]
