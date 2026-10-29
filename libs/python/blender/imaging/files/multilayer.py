"""Multipart OpenEXR files holding passes under "<layer>.<channel>" names, and the Cryptomatte mattes their headers name."""

from collections.abc import Callable, Mapping
from copy import replace
from enum import IntEnum
from functools import partial
import itertools
import pathlib
import re
from types import MappingProxyType
from typing import Annotated, Self

from expression import Nothing, Ok, Option, Some
from expression.collections import Block
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO

from blender.imaging.files.buffers import Coded, Depth
from blender.imaging.files.images import Compressions, Deflate, emitted, opened, Storage, Subimage
from blender.records.faults import accumulated, Fault, Outcome, Reason

# --- [TYPES] ----------------------------------------------------------------------------

type _Manifest = dict[str, Annotated[str, msgspec.Meta(pattern="^[0-9a-fA-F]{8}$")]]


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


class Layer(msgspec.Struct, frozen=True, kw_only=True):
    """Part channels sharing the name before their last dot, by the name after it."""

    part: int
    name: str
    channels: tuple[str, ...]


class FilePart(msgspec.Struct, frozen=True, kw_only=True):
    """File part with string header attributes."""

    index: int
    width: int
    height: int
    channels: tuple[str, ...]
    header: Mapping[str, str]


class Multilayer(msgspec.Struct, frozen=True, kw_only=True):
    """Multilayer file path and parts."""

    path: pathlib.Path
    parts: tuple[FilePart, ...]

    @property
    def layers(self) -> tuple[Layer, ...]:
        """Part layers in channel order."""
        return tuple(
            Layer(part=part.index, name=name, channels=tuple(channel.rpartition(".")[2] for channel in part.channels if channel.rpartition(".")[0] == name))
            for part in self.parts
            for name in dict.fromkeys(channel.rpartition(".")[0] for channel in part.channels)
        )

    @classmethod
    def read(cls, path: pathlib.Path) -> Outcome[Self]:
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
                            index=index,
                            width=spec.width,
                            height=spec.height,
                            channels=spec.channelnames,
                            header=MappingProxyType({held.name: held.value for held in spec.extra_attribs if isinstance(held.value, str)}),
                        )
                        for index, spec in specs
                    ),
                )
            )

        return opened(path, indexed)

    def plane(self, layer: Layer, channels: tuple[str, ...], /) -> Outcome[npt.NDArray[np.float32]]:
        """Reads named layer channels in plane order as a float32 plane.

        Returns:
            `(height, width, len(channels))` plane, or `read` fault per channel the layer lacks.
        """
        held = self.parts[layer.part].channels

        def read(indices: tuple[int, ...], image: OpenImageIO.ImageInput) -> Outcome[npt.NDArray[np.float32]]:
            first, last = min(indices), max(indices) + 1
            return (
                Option
                .of_optional(image.read_image(layer.part, 0, first, last, OpenImageIO.FLOAT))
                .map(lambda span: np.ascontiguousarray(span[..., [index - first for index in indices]]))
                .to_result_with(lambda: Fault.of(Reason.UNREADABLE, str(self.path), image.geterror()))
            )

        return accumulated(
            Option.of_optional(next((index for index, full in enumerate(held) if full == (f"{layer.name}.{name}" if layer.name else name)), None)).to_result_with(
                partial(Fault.of, Reason.UNREADABLE, f"{self.path}:{layer.name}", f"No {name} channel", member=name)
            )
            for name in channels
        ).bind(lambda indices: opened(self.path, partial(read, indices)))

    def picked(self, names: tuple[str, ...], channels: tuple[str, ...], /) -> Outcome[npt.NDArray[np.float32]]:
        """Reads named channels of the first layer of `names` the file holds, absent fault naming the last name."""
        return (
            Block
            .of_seq(names)
            .choose(lambda name: Block.of_seq(self.layers).filter(lambda found: found.name == name).try_head())
            .try_head()
            .to_result_with(lambda: Fault.of(Reason.UNREADABLE, str(self.path), f"No {names[-1]} layer", member=names[-1]))
            .bind(lambda layer: self.plane(layer, channels))
        )

    @staticmethod
    def written(
        path: pathlib.Path, passes: tuple[Pass, ...], layout: PassLayout, storage: Storage, size: tuple[int, int], fields: Mapping[str, str], plane: Callable[[Pass], Outcome[npt.NDArray[np.float32]]]
    ) -> Outcome[pathlib.Path]:
        """Writes passes in file order to one file, `storage` setting color pass type and compression, each plane read once as its part writes.

        Returns:
            Path, or `write` fault.
        """
        groups = tuple((item,) for item in passes) if layout is PassLayout.PARTS else (passes,)
        lossy = storage.compression.filter(lambda compression: compression.lossy).is_some()

        def subimage(group: tuple[Pass, ...]) -> Subimage[Coded]:
            channels = tuple((f"{item.layer}.{channel}", OpenImageIO.TypeDesc((Depth.FLOAT if item.role is PassRole.DATA else storage.depth).basetype)) for item in group for channel in item.channels)
            spec = OpenImageIO.ImageSpec(*size, len(channels), channels[0][1])
            spec.channelnames = tuple(name for name, _ in channels)
            spec.channelformats = tuple(kind for _, kind in channels)
            stored = msgspec.structs.replace(storage, compression=Some(Deflate())) if lossy and any(item.role is PassRole.DATA for item in group) else storage
            named = (("name", OpenImageIO.TypeString, group[0].layer),) if layout is PassLayout.PARTS else ()
            return Subimage(
                spec=spec,
                attributes=(*stored.attributes, *named, *((name, OpenImageIO.TypeString, text) for name, text in fields.items())),
                pixels=lambda: accumulated(plane(item).bind(stored.stored) if item.role is PassRole.COLOR else plane(item) for item in group).map(lambda planes: np.concatenate(planes, axis=-1)),
            )

        return emitted(path, lambda _: tuple(subimage(group) for group in groups))

    @staticmethod
    def rewritten(path: pathlib.Path, display: Option[OpenImageIO.ROI], channels: Callable[[tuple[str, ...]], tuple[str, ...]], compression: Option[Compressions]) -> Outcome[pathlib.Path]:
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
                    Option.of_optional(image.read_image(index, 0, 0, spec.nchannels, OpenImageIO.FLOAT)).to_result_with(lambda: Fault.of(Reason.UNREADABLE, str(path), image.geterror()))
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


class Matte(msgspec.Struct, frozen=True, kw_only=True):
    """Cryptomatte layer with hash per ID and its rank layers in order, each holding its RGBA channel names."""

    name: str
    ids: Mapping[str, int]
    ranks: tuple[Layer, ...]
    file: Multilayer

    @classmethod
    def found(cls, file: Multilayer) -> Outcome[tuple[Self, ...]]:
        """Decodes every Cryptomatte layer a file's header names.

        Returns:
            Mattes in header order, or `read` fault per undecodable manifest.
        """
        header = {key: value for part in file.parts for key, value in part.header.items()}
        layers = sorted(file.layers, key=lambda owner: owner.name)

        def decoded(name: str, manifest: str) -> Outcome[Self]:
            ranks = tuple(
                replace(layer, channels=tuple(folded[key] for key in "rgba"))
                for layer in layers
                if re.fullmatch(f"{re.escape(name)}[0-9]{{2}}", layer.name) and (folded := {channel.casefold(): channel for channel in layer.channels}).keys() >= set("rgba")
            )
            return (
                catch(msgspec.json.decode, exception=msgspec.DecodeError)(manifest, type=_Manifest)
                .map_error(partial(Fault.of, Reason.UNREADABLE, str(file.path)))
                .map(
                    lambda entries: cls(
                        name=name,
                        ids=MappingProxyType({entity: code & 0x807FFFFF | min(max(code >> 23 & 0xFF, 1), 254) << 23 for entity, digits in entries.items() for code in (int(digits, 16),)}),
                        ranks=ranks,
                        file=file,
                    )
                )
            )

        return accumulated(
            decoded(name, manifest)
            for key, name in header.items()
            if key.startswith("cryptomatte/") and key.endswith("/name")
            for manifest in Option.of_optional(header.get(f"{key.removesuffix('/name')}/manifest")).to_list()
        )

    def coverage(self, names: frozenset[str]) -> Outcome[npt.NDArray[np.float32]]:
        """Sums coverage of named IDs, unknown names adding nothing.

        Returns:
            `(height, width)` coverage, or `read` fault.
        """
        targets = np.fromiter((code for entity, code in self.ids.items() if entity in names), dtype=np.uint32)
        return self._planes().map(lambda planes: np.where(np.isin(planes[0], targets), planes[1], 0.0).sum(axis=-1, dtype=np.float32))

    def areas(self) -> Outcome[dict[str, float]]:
        """Measures frame share each manifest name covers.

        Returns:
            Share per name, 0 for names no pixel holds, or `read` fault.
        """
        part = self.file.parts[0]

        def summed(planes: tuple[npt.NDArray[np.uint32], npt.NDArray[np.float32]]) -> dict[str, float]:
            codes, inverse = np.unique(planes[0], return_inverse=True)
            found = dict(zip(codes.tolist(), (np.bincount(inverse.reshape(-1), weights=planes[1].reshape(-1), minlength=codes.size) / (part.width * part.height)).tolist()))
            return {entity: found.get(code, 0.0) for entity, code in self.ids.items()}

        return self._planes().map(summed)

    def filled(self, names: frozenset[str], color: npt.NDArray[np.float64]) -> Outcome[npt.NDArray[np.float32]]:
        """Fills named ID coverage with a `(3,)` color as associated RGBA.

        Returns:
            `(height, width, 4)` plane with coverage alpha, or `read` fault.
        """
        tint = np.append(color, 1.0).astype(np.float32)
        return self.coverage(names).map(lambda covered: covered[..., None] * tint)

    def _planes(self) -> Outcome[tuple[npt.NDArray[np.uint32], npt.NDArray[np.float32]]]:
        """ID codes and coverage weights of every rank pair, stacked along the last axis, `(height, width, 2 * ranks)`."""
        part = self.file.parts[0]
        empty = np.zeros((part.height, part.width, 0), dtype=np.float32)
        return (
            accumulated(self.file.plane(rank, rank.channels) for rank in self.ranks)
            .map(lambda ranked: np.concatenate((empty, *ranked), axis=-1))
            .map(lambda stacked: (stacked.view(np.uint32)[..., 0::2], stacked[..., 1::2]))
        )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["FilePart", "Layer", "Matte", "Multilayer", "Pass", "PassLayout", "PassRole"]
