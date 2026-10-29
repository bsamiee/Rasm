"""Image loads, pixels, readings, and images generated from planes."""

from collections.abc import Callable, Mapping
from functools import partial
from typing import Annotated, ClassVar

import bpy
from expression import Error, Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.data.ids.ownership import kept, Mark, Ownership
from blender.bpy.data.rna.descriptors import differing, written
from blender.bpy.data.rna.paths import RnaPath, struct_at
from blender.bpy.data.rna.snapshots import assigned
from blender.bpy.extension.identity import Identity
from blender.bpy.faults import BlenderReason
from blender.imaging.analysis.lighting import Lighting, LightingReading
from blender.imaging.files.buffers import premultiplied, unassociated
from blender.imaging.files.images import ImageFile
from blender.records.faults import Fault, Outcome
from blender.records.identity import ContentHash

# --- [MODELS] ---------------------------------------------------------------------------


class ImageFrames(msgspec.Struct, frozen=True, kw_only=True):
    """Frame window of image users and compositor image nodes."""

    frame_start: int = 1
    frame_duration: Annotated[int, msgspec.Meta(ge=1)] = 1
    frame_offset: int = 0

    @property
    def members(self) -> dict[RnaPath, int]:
        """Frame fields by the member path each writes."""
        return {("frame_start",): self.frame_start, ("frame_duration",): self.frame_duration, ("frame_offset",): self.frame_offset}

    def applied(self, user: bpy.types.ImageUser | bpy.types.CompositorNodeImage) -> Outcome[None]:
        """Writes frame fields, accumulating faults."""
        return assigned(user, self.members)

    def holds(self, user: bpy.types.ImageUser | bpy.types.CompositorNodeImage) -> bool:
        """Whether the user shows this frame window."""
        return not differing(user, self.members)


class Tag(msgspec.Struct, frozen=True, kw_only=True):
    """Requested image reading, absent values keeping image settings, a space naming a color space or an OCIO role of the current config."""

    space: Option[str] = Nothing
    alpha: Option[str] = Nothing
    view_as_render: Option[bool] = Nothing
    sequence: bool = False

    def applied(self, image: bpy.types.Image) -> Outcome[None]:
        """Writes sequence source, then color and display members, in host order, accumulating faults."""
        config = PyOpenColorIO.GetCurrentConfig()
        return assigned(
            image,
            dict((
                *(((("source",), "SEQUENCE"),) if self.sequence else ()),
                *((("colorspace_settings", "name"), Option.of_optional(config.getColorSpace(space)).map(lambda held: held.getName()).default_value(space)) for space in self.space.to_list()),
                *((("alpha_mode",), alpha) for alpha in self.alpha.to_list()),
                *((("use_view_as_render",), shown) for shown in self.view_as_render.to_list()),
            )),
        )


class Generated(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Packed image generated from a plane, built once per content hash and owned for one purpose."""

    ownership: Ownership[bpy.types.Image]
    content: ContentHash
    name: str
    plane: Callable[[], Outcome[Option[npt.NDArray[np.float32]]]]
    tag: Tag = Tag()

    nested: ClassVar[tuple[()]] = ()

    def made(self, identity: Identity, *, of: Option[bpy.types.ID] = Nothing) -> Outcome[Option[bpy.types.Image]]:
        """Reuses the equal-content image before reading the plane, else packs the plane into a new image, Nothing for a plane holding no pixels.

        Args:
            identity: Extension owning the image.
            of: Data-block the image serves.

        Returns:
            Owned image, refusal fault of any host call.
        """

        def built(values: npt.NDArray[np.float32]) -> Outcome[bpy.types.Image]:
            height, width, channels = values.shape
            return kept(
                catch(bpy.data.images.new, exception=RuntimeError)(self.name, width, height, alpha=channels == 4, float_buffer=True).map_error(partial(Fault.of, BlenderReason.REFUSED, self.name)),
                lambda image: (
                    self.tag
                    .applied(image)
                    .bind(lambda _: filled(image, values))
                    .bind(lambda _: catch(image.pack, exception=RuntimeError)().map_error(partial(Fault.of, BlenderReason.REFUSED, repr(image))))
                    .map(lambda _: image)
                ),
            )

        def owned(values: npt.NDArray[np.float32]) -> Outcome[Option[bpy.types.Image]]:
            return built(values).bind(lambda image: self.ownership.owned(identity, image, of=of, content=Some(self.content))).map(Some)

        return (
            self.ownership
            .reused(identity, of=of, content=Some(self.content))
            .map(lambda image: Ok(Some(image)))
            .default_with(lambda: self.plane().bind(lambda values: values.map(owned).default_value(Ok(Nothing))))
        )

    @property
    def keyed(self) -> object:
        """Builtin form a node graph digest reads, the plane read only when the image is made."""
        return (self.ownership.key, self.content, self.name, msgspec.to_builtins(self.tag))

    def holds(self, live: bpy.types.Node, identity: Identity, /) -> Outcome[bool]:
        """Whether the node shows this content's owned image, or marks this content as holding no pixels."""
        return Ok(
            struct_at(live, ("image",), bpy.types.Image)
            .map(lambda image: Mark.read(identity, image) == Some(Mark(key=self.ownership.key, content=Some(self.content))))
            .default_with(lambda: Mark.read(identity, live).map(lambda mark: mark.content == Some(self.content)).default_value(False))
        )

    def write(self, live: bpy.types.Node, identity: Identity, /) -> Outcome[None]:
        """Points the node at the made image, marking the node with this content while the plane holds no pixels."""
        return self.made(identity).bind(
            lambda image: written(live, ("image",), image.to_optional()).bind(
                lambda _: (
                    Mark
                    .read(identity, live)
                    .map(lambda mark: msgspec.structs.replace(mark, content=Some(self.content) if image.is_none() else Nothing).write(identity, live))
                    .default_value(Error(Fault.of(BlenderReason.ABSENT, repr(live), member=identity.manifest.id)))
                )
            )
        )

    @staticmethod
    def sockets(_live: bpy.types.Node, _identity: Identity, _slot: str, /) -> Mapping[str, str]:
        """No item sockets."""
        return {}


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
        return ImageFile(pixels=premultiplied(flipped) if _straight(image) else flipped, colorspace=space)

    return pixels(image).map(formed) if np.asarray(image.size).prod() else Error(Fault.of(BlenderReason.ABSENT, repr(image), member=image.filepath))


def lighting(image: bpy.types.Image | None, config: PyOpenColorIO.Config) -> Outcome[Option[LightingReading]]:
    """Returns the lighting reading of an image, `Nothing` without image or measurable reading."""
    return Option.of_optional(image).map(lambda held: read(held, config).bind(lambda file: Lighting().read(config, file.pixels))).default_value(Ok(Nothing))


def filled(image: bpy.types.Image, plane: npt.NDArray[np.float32]) -> Outcome[None]:
    """Writes associated plane into pixels, widening gray and padding channels."""
    straight = unassociated(plane) if _straight(image) else plane
    color = np.repeat(straight, 3, axis=-1) if straight.shape[2] == 1 else straight
    padded = np.pad(color, ((0, 0), (0, 0), (0, image.channels - color.shape[2])))
    if color.shape[2] < 4 and image.channels == 4:
        padded[..., 3] = 1.0
    return catch(image.pixels.foreach_set, exception=TypeError)(padded[::-1].reshape(-1)).map_error(partial(Fault.of, BlenderReason.REFUSED, repr(image))).map(lambda _: image.update())


def _straight(image: bpy.types.Image) -> bool:
    """Whether the image's byte buffer stores straight color under an alpha the host reads, which float buffers and data images never do."""
    settings: bpy.types.ColorManagedInputColorspaceSettings = image.colorspace_settings  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    return not image.is_float and image.channels == 4 and not settings.is_data and image.alpha_mode in {"STRAIGHT", "PREMUL"}


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Generated", "ImageFrames", "Tag", "filled", "lighting", "loaded", "pixels", "read"]
