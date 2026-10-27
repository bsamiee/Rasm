"""Multipart OpenEXR files holding passes under "<layer>.<channel>" names."""

from collections.abc import Callable, Mapping
from enum import IntEnum
import itertools
from typing import Self

from expression import Nothing, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO

from blender.imaging.files.coding import Depth
from blender.imaging.files.images import Compressions, Deflate, emitted, opened, Storage, Subimage
from blender.records.faults import accumulated, Fault, Outcome, RecordReason

# --- [TYPES] ----------------------------------------------------------------------------


class PassRole(IntEnum):
    """Pass content deciding stored type."""

    COLOR = 1
    DATA = 2


class PassLayout(IntEnum):
    """Pass arrangement in file."""

    PARTS = 1
    INTERLEAVED = 2


# --- [MODELS] ---------------------------------------------------------------------------


class Pass(msgspec.Struct, frozen=True, kw_only=True):
    """Pass to write."""

    layer: str
    channels: tuple[str, ...]
    role: PassRole


class Channel(msgspec.Struct, frozen=True, kw_only=True):
    """Channel ID and index among part channels."""

    name: str
    index: int


class Layer(msgspec.Struct, frozen=True, kw_only=True):
    """Part channels sharing name before last dot."""

    part: int
    name: str
    channels: tuple[Channel, ...]


class FilePart(msgspec.Struct, frozen=True, kw_only=True):
    """File part with string header attributes."""

    index: int
    width: int
    height: int
    channels: tuple[str, ...]
    header: Mapping[str, str]


class Multilayer(msgspec.Struct, frozen=True, kw_only=True):
    """Multilayer file path and parts."""

    path: str
    parts: tuple[FilePart, ...]

    @property
    def layers(self) -> tuple[Layer, ...]:
        """Part layers in channel order."""
        return tuple(
            Layer(
                part=part.index, name=name, channels=tuple(Channel(name=channel.rpartition(".")[2], index=index) for index, channel in enumerate(part.channels) if channel.rpartition(".")[0] == name)
            )
            for part in self.parts
            for name in dict.fromkeys(channel.rpartition(".")[0] for channel in part.channels)
        )

    @classmethod
    def read(cls, path: str) -> Outcome[Self]:
        """Indexes the parts of the file at `path`.

        Returns:
            Indexed file, or `read` fault.
        """

        def indexed(image: OpenImageIO.ImageInput) -> Outcome[Self]:
            specs = ((index, image.spec()) for index in itertools.takewhile(lambda index: image.seek_subimage(index, 0), itertools.count()))
            return Ok(
                cls(
                    path=path,
                    parts=tuple(
                        FilePart(
                            index=index, width=spec.width, height=spec.height, channels=spec.channelnames, header={held.name: held.value for held in spec.extra_attribs if isinstance(held.value, str)}
                        )
                        for index, spec in specs
                    ),
                )
            )

        return opened(path, indexed)

    def plane(self, layer: Layer, channels: tuple[Channel, ...]) -> Outcome[npt.NDArray[np.float32]]:
        """Reads layer channels in plane order as a float32 plane.

        Returns:
            `(height, width, len(channels))` plane, or `read` fault.
        """
        first, last = min(channel.index for channel in channels), max(channel.index for channel in channels) + 1

        def pixels(image: OpenImageIO.ImageInput) -> Outcome[npt.NDArray[np.float32]]:
            return (
                Option
                .of_optional(image.read_image(layer.part, 0, first, last, OpenImageIO.FLOAT))
                .map(lambda span: np.ascontiguousarray(span[..., [channel.index - first for channel in channels]]))
                .to_result_with(lambda: Fault.of(RecordReason.UNREADABLE, self.path, image.geterror()))
            )

        return opened(self.path, pixels)

    @staticmethod
    def written(
        path: str, passes: tuple[Pass, ...], layout: PassLayout, storage: Storage, size: tuple[int, int], fields: Mapping[str, str], plane: Callable[[Pass], Outcome[npt.NDArray[np.float32]]]
    ) -> Outcome[str]:
        """Writes passes in file order to one file, `storage` setting color pass type and compression, each plane read once as its part writes.

        Returns:
            Path, or `write` fault.
        """
        groups = tuple((item,) for item in passes) if layout is PassLayout.PARTS else (passes,)
        lossy = storage.compression.filter(lambda compression: compression.lossy).is_some()

        def subimage(group: tuple[Pass, ...]) -> Subimage[np.float32]:
            channels = tuple(
                (f"{item.layer}.{channel}", OpenImageIO.TypeDesc(OpenImageIO.FLOAT if item.role is PassRole.DATA or storage.depth is Depth.FLOAT else OpenImageIO.HALF))
                for item in group
                for channel in item.channels
            )
            spec = OpenImageIO.ImageSpec(*size, len(channels), channels[0][1])
            spec.channelnames = tuple(name for name, _ in channels)
            spec.channelformats = tuple(kind for _, kind in channels)
            stored = msgspec.structs.replace(storage, compression=Some(Deflate())) if lossy and any(item.role is PassRole.DATA for item in group) else storage
            named = (("name", OpenImageIO.TypeString, group[0].layer),) if layout is PassLayout.PARTS else ()
            return Subimage(
                spec=spec,
                attributes=(*stored.attributes, *named, *((name, OpenImageIO.TypeString, text) for name, text in fields.items())),
                pixels=lambda: accumulated(plane(item) for item in group).map(
                    lambda planes: np.concatenate([stored.clipped(array) if item.role is PassRole.COLOR else array for item, array in zip(group, planes)], axis=-1)
                ),
            )

        return emitted(path, lambda _: tuple(subimage(group) for group in groups))

    @staticmethod
    def rewritten(path: str, display: Option[OpenImageIO.ROI], channels: Callable[[tuple[str, ...]], tuple[str, ...]], compression: Option[Compressions]) -> Outcome[str]:
        """Rewrites windows to `display` in data window pixels, channel names through `channels`, and `compression` where a part names none, skipping unchanged files.

        Returns:
            Path, or `read` or `write` fault.
        """

        def parts(image: OpenImageIO.ImageInput) -> Outcome[Option[tuple[Subimage[np.float32], ...]]]:
            specs = tuple(OpenImageIO.ImageSpec(image.spec()) for _ in itertools.takewhile(lambda index: image.seek_subimage(index, 0), itertools.count()))
            renamed = tuple(channels(spec.channelnames) for spec in specs)
            return (
                Ok(Nothing)
                if display.is_none() and all(names == spec.channelnames for spec, names in zip(specs, renamed))
                else accumulated(
                    Option.of_optional(image.read_image(index, 0, 0, spec.nchannels, OpenImageIO.FLOAT)).to_result_with(lambda: Fault.of(RecordReason.UNREADABLE, path, image.geterror()))
                    for index, spec in enumerate(specs)
                ).map(lambda arrays: Some(tuple(map(subimage, specs, renamed, arrays))))
            )

        def subimage(spec: OpenImageIO.ImageSpec, names: tuple[str, ...], array: npt.NDArray[np.float32]) -> Subimage[np.float32]:
            for frame in display.to_list():
                spec.x, spec.y, spec.full_x, spec.full_y, spec.full_width, spec.full_height = -frame.xbegin, -frame.ybegin, 0, 0, frame.width, frame.height
            spec.channelnames = names
            return Subimage(
                spec=spec, attributes=compression.filter(lambda _: not spec.get_string_attribute("compression")).map(lambda held: held.attributes).default_value(()), pixels=lambda: Ok(array)
            )

        return opened(path, parts).bind(lambda read: read.map(lambda parts: emitted(path, lambda _: parts)).default_value(Ok(path)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Channel", "FilePart", "Layer", "Multilayer", "Pass", "PassLayout", "PassRole"]
