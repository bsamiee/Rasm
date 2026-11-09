"""Image loads, tags, pixels, and readings."""

from collections.abc import Iterable
from contextlib import ExitStack
from functools import partial
from pathlib import Path
from tempfile import TemporaryDirectory
from typing import Literal

import bpy
from expression import Option, Some
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from rasm_bpy.data._extension.reports import Refusal
from rasm_bpy.data._rna.descriptors import Value
from rasm_bpy.data._rna.paths import RnaPath
from rasm_bpy.data._rna.snapshots import assigned
from rasm_imaging import called, ImageFile, Lighting, LightingReading, premultiplied, read_file, unassociated
from rasm_records import Error, Fault, Nothing, Ok, Outcome, Reason

# --- [MODELS] ---------------------------------------------------------------------------


class Tag(msgspec.Struct, frozen=True, kw_only=True):
    """Requested image reading, absent values keeping image settings, a space naming a color space or an OCIO role of the current config."""

    space: Option[str] = Nothing
    alpha: Option[Literal["STRAIGHT", "PREMUL", "CHANNEL_PACKED", "NONE"]] = Nothing
    view_as_render: Option[bool] = Nothing
    sequence: bool = False

    def members(self, config: Option[PyOpenColorIO.Config] = Nothing) -> Outcome[dict[RnaPath, Value]]:
        """Returns requested native members, resolving explicit color spaces and leaving omitted fields untouched."""
        return (
            self.space
            .map(
                lambda name: (
                    config
                    .map(Ok)
                    .default_with(lambda: called(PyOpenColorIO.GetCurrentConfig, name))
                    .bind(
                        lambda current: (
                            Option.of_optional(current.getColorSpace(name)).map(lambda space: Some(space.getName())).to_result_with(lambda: Fault.of(Reason.ABSENT, current.getName(), member=name))
                        )
                    )
                )
            )
            .default_value(Ok(Nothing))
            .map(
                lambda space: dict[RnaPath, Value]((
                    *(((("source",), "SEQUENCE"),) if self.sequence else ()),
                    *((("colorspace_settings", "name"), name) for name in space.to_list()),
                    *((("alpha_mode",), alpha) for alpha in self.alpha.to_list()),
                    *((("use_view_as_render",), shown) for shown in self.view_as_render.to_list()),
                ))
            )
        )

    def applied(self, image: bpy.types.Image) -> Outcome[None]:
        """Resolves the tag in Blender's current config and writes its requested native members in host order."""
        return self.members().bind(lambda values: assigned(image, values))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [LOADS]


def _loaded(path: str, tag: Tag) -> Outcome[bpy.types.Image]:
    """Loads or reuses file image, then applies tag."""
    return (
        catch(bpy.data.images.load, exception=RuntimeError)(path, check_existing=True).map_error(partial(Fault.of, Refusal.REFUSED, path)).bind(lambda image: tag.applied(image).map(lambda _: image))
    )


# --- [PIXELS]


def _pixels(image: bpy.types.Image) -> Outcome[npt.NDArray[np.float32]]:
    """Reads float32 pixels bottom row first as height, width, channels, refused fault for an image holding no buffer."""
    (width, height), channels = np.asarray(image.size), image.channels
    plane = np.empty((height, width, channels), np.float32)
    return catch(image.pixels.foreach_get, exception=TypeError)(plane.reshape(-1)).map_error(partial(Fault.of, Refusal.REFUSED, repr(image))).map(lambda _: plane)


def aspect(image: bpy.types.Image) -> Outcome[float]:
    """Returns the default image buffer's displayed width to height, absent fault without a buffer."""
    size: Iterable[int] = image.size  # ty: ignore[unsound-assignment]
    (width, height), display = size, image.display_aspect
    return Ok(width * display.x / (height * display.y)) if width and height else Error(Fault.of(Reason.ABSENT, repr(image), member=image.filepath))


def _read(image: bpy.types.Image, colorspace: str) -> Outcome[ImageFile]:
    """Returns associated float32 pixels top row first in their producer's encoding, absent fault when empty."""

    def formed(plane: npt.NDArray[np.float32]) -> ImageFile:
        flipped = np.ascontiguousarray(plane[::-1])
        return ImageFile(pixels=premultiplied(flipped) if _straight(image) else flipped, colorspace=colorspace)

    return _pixels(image).map(formed) if np.asarray(image.size).prod() else Error(Fault.of(Reason.ABSENT, repr(image), member=image.filepath))


def normalized(image: bpy.types.Image, config: PyOpenColorIO.Config) -> Outcome[ImageFile]:
    """Reads an ordinary image through native full-float linear export, preserving data encoding.

    Render products require their producer's encoding through `read` or a recorded render origin.

    Returns:
        Associated pixels with their encoding, or native configuration, export, and read faults.
    """
    if image.source == "VIEWER" or image.type == "MULTILAYER":
        return Error(Fault.of(Refusal.REQUIRED, repr(image), member="render encoding colorspace"))
    settings: bpy.types.ColorManagedInputColorspaceSettings = image.colorspace_settings  # ty: ignore[invalid-assignment]
    if settings.is_data:
        return _read(image, settings.name)
    refused = partial(Fault.of, Refusal.REFUSED, repr(image))

    def captured(space: str, scene: bpy.types.Scene) -> Outcome[ImageFile]:
        try:
            with ExitStack() as lifetime:
                lifetime.callback(bpy.data.scenes.remove, scene)
                path = Path(lifetime.enter_context(TemporaryDirectory(dir=bpy.app.tempdir)), "image.exr")

                try:
                    output = scene.render.image_settings
                    output.color_management = "OVERRIDE"
                    output.file_format = "OPEN_EXR"
                    output.color_depth = "32"
                    output.color_mode = "RGBA"
                    linear: bpy.types.ColorManagedInputColorspaceSettings = output.linear_colorspace_settings  # ty: ignore[invalid-assignment]
                    linear.name = space  # ty: ignore[invalid-assignment]
                    image.save_render(str(path), scene=scene)
                except (RuntimeError, ValueError, TypeError) as error:
                    return Error(refused(error))
                return read_file(path, config, colorspace=Some(space))
        except OSError as error:
            return Error(refused(error))

    return (
        Option
        .of_optional(config.getColorSpace(PyOpenColorIO.ROLE_SCENE_LINEAR))
        .map(PyOpenColorIO.ColorSpace.getName)
        .to_result_with(lambda: Fault.of(Reason.ABSENT, config.getName(), member=PyOpenColorIO.ROLE_SCENE_LINEAR))
        .bind(lambda space: catch(bpy.data.scenes.new, exception=RuntimeError)("Image Read").map_error(refused).bind(partial(captured, space)))
    )


def _lighting(image: bpy.types.Image | None, config: PyOpenColorIO.Config) -> Outcome[Option[LightingReading]]:
    """Returns the lighting reading of an image, `Nothing` without image or measurable reading."""
    return Option.of_optional(image).map(lambda held: normalized(held, config).bind(lambda file: Lighting().read(config, file.pixels))).default_value(Ok(Nothing))


def _filled(image: bpy.types.Image, plane: npt.NDArray[np.float32]) -> Outcome[None]:
    """Writes associated plane into pixels, widening gray and padding channels."""
    straight = unassociated(plane) if _straight(image) else plane
    color = np.repeat(straight, 3, axis=-1) if straight.shape[2] == 1 else straight
    padded = np.pad(color, ((0, 0), (0, 0), (0, image.channels - color.shape[2])))
    if color.shape[2] < 4 and image.channels == 4:
        padded[..., 3] = 1.0
    return catch(image.pixels.foreach_set, exception=TypeError)(padded[::-1].reshape(-1)).map_error(partial(Fault.of, Refusal.REFUSED, repr(image))).map(lambda _: image.update())


def _straight(image: bpy.types.Image) -> bool:
    """Whether the image's byte buffer stores straight color under an alpha the host reads, which float buffers and data images never do."""
    settings: bpy.types.ColorManagedInputColorspaceSettings = image.colorspace_settings  # ty: ignore[invalid-assignment]
    return not image.is_float and image.channels == 4 and not settings.is_data and image.alpha_mode in {"STRAIGHT", "PREMUL"}


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Tag", "aspect", "normalized"]
