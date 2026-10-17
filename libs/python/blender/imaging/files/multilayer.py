"""Multipart OpenEXR files holding passes under "<layer>.<channel>" names."""

from collections.abc import Callable, Mapping
import itertools
from typing import Self

from expression import Nothing, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO

from blender.imaging.faults import accumulated, ImagingReadFault, ImagingResult
from blender.imaging.files.images import Compressions, Deflate, emitted, opened, Storage, Subimage
from blender.imaging.parameters import Choice

# --- [TYPES] ----------------------------------------------------------------------------


class Role(Choice):
    """What a pass holds, deciding its stored type."""

    COLOR = 1
    DATA = 2


class Layout(Choice):
    """How a file holds its passes."""

    PARTS = 1, "Multipart"
    INTERLEAVED = 2


# --- [MODELS] ---------------------------------------------------------------------------


class Pass(msgspec.Struct, frozen=True, kw_only=True):
    """Pass a write stores: its layer name, channel ids, and role."""

    layer: str
    channels: tuple[str, ...]
    role: Role


class Window(msgspec.Struct, frozen=True, kw_only=True):
    """Frame inside a larger data window: its offset and size."""

    x: int
    y: int
    width: int
    height: int


class Channel(msgspec.Struct, frozen=True, kw_only=True):
    """Channel id and its position among its part's channels."""

    name: str
    index: int


class Layer(msgspec.Struct, frozen=True, kw_only=True):
    """Channels of one part sharing the text before their last dot."""

    part: int
    name: str
    channels: tuple[Channel, ...]


class Part(msgspec.Struct, frozen=True, kw_only=True):
    """One part of a file: its size, channel names, and string header attributes."""

    index: int
    width: int
    height: int
    channels: tuple[str, ...]
    header: dict[str, str]


class Multilayer(msgspec.Struct, frozen=True, kw_only=True):
    """Parts of a multilayer file at its path."""

    path: str
    parts: tuple[Part, ...]

    @property
    def layers(self) -> tuple[Layer, ...]:
        """Every part's layers in channel order."""
        return tuple(
            Layer(
                part=part.index, name=name, channels=tuple(Channel(name=channel.rpartition(".")[2], index=index) for index, channel in enumerate(part.channels) if channel.rpartition(".")[0] == name)
            )
            for part in self.parts
            for name in dict.fromkeys(channel.rpartition(".")[0] for channel in part.channels)
        )

    @classmethod
    def read(cls, path: str) -> ImagingResult[Self]:
        """Indexes a file's parts.

        Args:
            path: File to index.

        Returns:
            The file's parts; the `read` case holding the path and OpenImageIO's text.
        """

        def indexed(image: OpenImageIO.ImageInput) -> ImagingResult[Self]:
            specs = ((index, image.spec()) for index in itertools.takewhile(lambda index: image.seek_subimage(index, 0), itertools.count()))
            return Ok(
                cls(
                    path=path,
                    parts=tuple(
                        Part(
                            index=index, width=spec.width, height=spec.height, channels=spec.channelnames, header={held.name: held.value for held in spec.extra_attribs if isinstance(held.value, str)}
                        )
                        for index, spec in specs
                    ),
                )
            )

        return opened(path, indexed)

    def plane(self, layer: Layer, channels: tuple[Channel, ...]) -> ImagingResult[npt.NDArray[np.float32]]:
        """Reads channels of one layer as a float32 plane in the order given.

        Args:
            layer: Layer holding the channels.
            channels: Channels of the layer in the order the plane holds them.

        Returns:
            The `(height, width, len(channels))` plane; the `read` case holding the path and OpenImageIO's text.
        """
        first, last = min(channel.index for channel in channels), max(channel.index for channel in channels) + 1

        def pixels(image: OpenImageIO.ImageInput) -> ImagingResult[npt.NDArray[np.float32]]:
            return (
                Option
                .of_optional(image.read_image(layer.part, 0, first, last, OpenImageIO.FLOAT))
                .map(lambda span: np.ascontiguousarray(span[..., [channel.index - first for channel in channels]]))
                .to_result_with(lambda: (ImagingReadFault(request=self.path, error=image.geterror()),))
            )

        return opened(self.path, pixels)


# --- [OPERATIONS] -----------------------------------------------------------------------


def written(
    path: str, passes: tuple[Pass, ...], layout: Layout, storage: Storage, size: tuple[int, int], fields: Mapping[str, str], plane: Callable[[Pass], ImagingResult[npt.NDArray[np.float32]]]
) -> ImagingResult[str]:
    """Writes passes to one file, each pass's plane supplied when its part writes.

    Args:
        path: File to write.
        passes: Passes in file order.
        layout: One part per pass, or every pass in one part.
        storage: Stored type of color passes and the file's compression.
        size: Width and height of every pass.
        fields: Text fields every part's header stores.
        plane: Plane of a pass, read once per pass.

    Returns:
        The path; the `write` case holding the path and OpenImageIO's text or the `OSError`.
    """
    groups = tuple((item,) for item in passes) if layout is Layout.PARTS else (passes,)
    lossy = storage.compression.filter(lambda compression: compression.lossy).is_some()

    def subimage(group: tuple[Pass, ...]) -> Subimage[np.float32]:
        channels = tuple(
            (f"{item.layer}.{channel}", OpenImageIO.TypeDesc(OpenImageIO.FLOAT if item.role is Role.DATA or storage.depth == 32 else OpenImageIO.HALF)) for item in group for channel in item.channels
        )
        spec = OpenImageIO.ImageSpec(*size, len(channels), channels[0][1])
        spec.channelnames = tuple(name for name, _ in channels)
        spec.channelformats = tuple(kind for _, kind in channels)
        stored = msgspec.structs.replace(storage, compression=Some(Deflate())) if lossy and any(item.role is Role.DATA for item in group) else storage
        named = (("name", OpenImageIO.TypeString, group[0].layer),) if layout is Layout.PARTS else ()
        return Subimage(
            spec=spec,
            attributes=(*stored.attributes, *named, *((name, OpenImageIO.TypeString, text) for name, text in fields.items())),
            pixels=lambda: accumulated(plane(item) for item in group).map(
                lambda planes: np.concatenate([stored.clipped(array) if item.role is Role.COLOR else array for item, array in zip(group, planes, strict=True)], axis=-1)
            ),
        )

    return emitted(path, lambda _: tuple(subimage(group) for group in groups))


def rewritten(path: str, display: Option[Window], channels: Callable[[tuple[str, ...]], tuple[str, ...]], compression: Option[Compressions]) -> ImagingResult[str]:
    """Publishes changed windows and channel names after reading and closing the original file.

    Args:
        path: File to rewrite.
        display: Frame inside the data window, `Nothing` keeping the windows the file holds.
        channels: Pure channel-name transform for each native part, preserving order and channel count.
        compression: Explicit codec the source owner knows but the reader omits, `Nothing` keeping the header's own.

    Returns:
        The path; the `read` or `write` case holding the path and OpenImageIO's text or the `OSError`.
    """

    def parts(image: OpenImageIO.ImageInput) -> ImagingResult[Option[tuple[Subimage[np.float32], ...]]]:
        specs = tuple(OpenImageIO.ImageSpec(image.spec()) for _ in itertools.takewhile(lambda index: image.seek_subimage(index, 0), itertools.count()))
        renamed = tuple(channels(spec.channelnames) for spec in specs)
        return (
            Ok(Nothing)
            if display.is_none() and all(names == spec.channelnames for spec, names in zip(specs, renamed, strict=True))
            else accumulated(
                Option.of_optional(image.read_image(index, 0, 0, spec.nchannels, OpenImageIO.FLOAT)).to_result_with(lambda: (ImagingReadFault(request=path, error=image.geterror()),))
                for index, spec in enumerate(specs)
            ).map(lambda arrays: Some(tuple(itertools.starmap(subimage, zip(specs, renamed, arrays, strict=True)))))
        )

    def subimage(spec: OpenImageIO.ImageSpec, names: tuple[str, ...], array: npt.NDArray[np.float32]) -> Subimage[np.float32]:
        for window in display.to_list():
            spec.x, spec.y, spec.full_x, spec.full_y, spec.full_width, spec.full_height = -window.x, -window.y, 0, 0, window.width, window.height
        spec.channelnames = names
        return Subimage(spec=spec, attributes=compression.filter(lambda _: not spec.get_string_attribute("compression")).map(lambda held: held.attributes).default_value(()), pixels=lambda: Ok(array))

    return opened(path, parts).bind(lambda read: read.map(lambda parts: emitted(path, lambda _: parts)).default_value(Ok(path)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Channel", "Layer", "Layout", "Multilayer", "Part", "Pass", "Role", "Window", "rewritten", "written"]
