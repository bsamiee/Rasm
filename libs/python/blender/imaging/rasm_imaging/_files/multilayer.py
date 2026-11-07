"""Multipart OpenEXR files holding passes under "<layer>.<channel>" names, and the Cryptomatte mattes their headers name."""

from collections import defaultdict
from collections.abc import Callable, Iterator, Mapping
from copy import replace
from enum import IntEnum
from functools import partial
import itertools
import pathlib
import re
from types import MappingProxyType
from typing import Annotated, Self

from expression import Option, Some
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from rasm_imaging._files.buffers import Depth
from rasm_imaging._files.images import Compression, Deflate, emitted, ImageFile, opened, read_file, read_pixels, Storage, Subimage
from rasm_records import accumulated, content_hash, ContentHash, Error, Fault, Nothing, Ok, Outcome, paired, Reason

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


class Multilayer(msgspec.Struct, frozen=True, kw_only=True):
    """Multilayer file path and parts."""

    path: pathlib.Path
    parts: Mapping[int, OpenImageIO.ImageSpec]

    @property
    def layers(self) -> tuple[Layer, ...]:
        """Part layers in channel order."""
        return tuple(
            Layer(part=index, name=name, channels=tuple(channel.rpartition(".")[2] for channel in part.channelnames if channel.rpartition(".")[0] == name))
            for index, part in self.parts.items()
            for name in dict.fromkeys(channel.rpartition(".")[0] for channel in part.channelnames)
        )

    @classmethod
    def read(cls, path: pathlib.Path) -> Outcome[Self]:
        """Indexes the parts of the file at `path`.

        Returns:
            Indexed file, or `UNREADABLE` fault.
        """

        def indexed(image: OpenImageIO.ImageInput) -> Outcome[Self]:
            specs = ((index, image.spec()) for index in itertools.takewhile(lambda index: image.seek_subimage(index, 0), itertools.count()))
            return Ok(cls(path=path, parts=MappingProxyType(dict(specs))))

        return opened(path, indexed)

    def plane(self, layer: Layer, channels: tuple[str, ...], /) -> Outcome[npt.NDArray[np.float32]]:
        """Reads named layer channels in plane order as a float32 plane.

        Returns:
            `(height, width, len(channels))` plane, `ABSENT` fault per channel the layer lacks, or `UNREADABLE` fault.
        """
        return self._indices(layer, channels).bind(lambda indices: opened(self.path, lambda image: read_pixels(self.path, image, self.parts[layer.part], subimage=layer.part, channels=Some(indices))))

    def image(self, layer: Layer, channels: tuple[str, ...], config: PyOpenColorIO.Config, /) -> Outcome[ImageFile]:
        """Reads selected channels with the part's native metadata and source colorspace, or channel, read, and color faults."""
        return self._indices(layer, channels).bind(lambda indices: read_file(self.path, config, subimage=layer.part, channels=Some(indices)))

    def fingerprint(self, include: Callable[[str], bool] = lambda _: True) -> Outcome[ContentHash]:
        """Hashes every held part's native geometry, channel encoding, included metadata, and pixels, or returns metadata decode and pixel read faults."""

        def channel_hashes(image: OpenImageIO.ImageInput, index: int, spec: OpenImageIO.ImageSpec) -> Iterator[Outcome[ContentHash]]:
            first = 0
            for kind, channels in itertools.groupby(spec.get_channelformats()):
                end = first + sum(1 for _ in channels)
                yield (
                    Option
                    .of_optional(image.read_image(index, 0, first, end, kind))
                    .to_result_with(lambda: Fault.of(Reason.UNREADABLE, str(self.path), image.geterror(), member=str(kind)))
                    .map(lambda plane: content_hash(type(self), arrays=(plane,)))
                )
                first = end

        def indexed(image: OpenImageIO.ImageInput) -> Outcome[ContentHash]:
            def hashed(index: int, spec: OpenImageIO.ImageSpec) -> Outcome[ContentHash]:
                header = OpenImageIO.ImageSpec(spec)
                header.extra_attribs.sort()
                attributes = accumulated(
                    Option
                    .of_optional(field.value)
                    .to_result_with(partial(Fault.of, Reason.OUTSIDE, f"{self.path}:{field.name}", field.type, member="metadata type exposed by OpenImageIO"))
                    .map(lambda value: (field.name, str(field.type), memoryview(value) if isinstance(value, np.ndarray) else value))
                    for field in header.extra_attribs
                    if include(field.name)
                )
                header.extra_attribs.clear()
                return paired(attributes, accumulated(channel_hashes(image, index, spec))).map(
                    lambda parts: content_hash(type(self), index, header.to_xml(), tuple(map(str, spec.channelformats)), *parts)
                )

            return accumulated(itertools.starmap(hashed, self.parts.items())).map(lambda hashes: content_hash(type(self), *hashes))

        return opened(self.path, indexed)

    def _indices(self, layer: Layer, channels: tuple[str, ...]) -> Outcome[tuple[int, ...]]:
        held = self.parts[layer.part].channelnames
        positions = {held[index]: index for index in reversed(range(len(held)))}
        return accumulated(
            Option.of_optional(positions.get(f"{layer.name}.{name}" if layer.name else name)).to_result_with(partial(Fault.of, Reason.ABSENT, f"{self.path}:{layer.name}", member=f"{name} channel"))
            for name in channels
        )

    def picked(self, names: tuple[str, ...], channels: tuple[str, ...], /) -> Outcome[npt.NDArray[np.float32]]:
        """Reads named channels of the first layer of `names` the file holds, `ABSENT` fault naming the last name."""
        layers = self.layers
        return (
            Option
            .of_optional(next((layer for name in names for layer in layers if layer.name == name), None))
            .to_result_with(lambda: Fault.of(Reason.ABSENT, str(self.path), member=f"{names[-1]} layer"))
            .bind(lambda layer: self.plane(layer, channels))
        )

    @staticmethod
    def written(
        path: pathlib.Path, passes: tuple[Pass, ...], layout: PassLayout, storage: Storage, size: tuple[int, int], fields: Mapping[str, str], plane: Callable[[Pass], Outcome[npt.NDArray[np.float32]]]
    ) -> Outcome[pathlib.Path]:
        """Writes passes in file order to one file, `storage` setting color pass type and compression, each plane read once as its part writes.

        Returns:
            Path, or `UNWRITABLE` fault.
        """
        groups = tuple((item,) for item in passes) if layout is PassLayout.PARTS else (passes,)
        lossy = storage.compression.filter(lambda compression: compression.lossy).is_some()

        def subimage(group: tuple[Pass, ...]) -> Subimage:
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
    def rewritten(path: pathlib.Path, display: Option[OpenImageIO.ROI], channels: Callable[[tuple[str, ...]], tuple[str, ...]], compression: Option[Compression]) -> Outcome[pathlib.Path]:
        """Rewrites windows to `display` in data window pixels, channel names through `channels`, and `compression` where a part names none, skipping unchanged files.

        Returns:
            Path, or `UNREADABLE` or `UNWRITABLE` fault.
        """

        def parts(image: OpenImageIO.ImageInput) -> Outcome[Option[tuple[Subimage, ...]]]:
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

        def subimage(spec: OpenImageIO.ImageSpec, names: tuple[str, ...], array: npt.NDArray[np.float32]) -> Subimage:
            for frame in display.to_list():
                spec.x, spec.y, spec.full_x, spec.full_y, spec.full_width, spec.full_height = -frame.xbegin, -frame.ybegin, 0, 0, frame.width, frame.height
            spec.channelnames = names
            return Subimage(
                spec=spec, attributes=compression.filter(lambda _: not spec.get_string_attribute("compression")).map(lambda held: held.attributes).default_value(()), pixels=lambda: Ok(array)
            )

        return opened(path, parts).bind(lambda read: read.map(lambda parts: emitted(path, lambda _: parts)).default_value(Ok(path)))


class Matte(msgspec.Struct, frozen=True, kw_only=True):
    """One view's Cryptomatte layer with hash per ID and rank layers in order, each holding its RGBA channel names."""

    name: str
    ids: Mapping[str, int]
    ranks: tuple[Layer, ...]
    file: Multilayer

    @property
    def view(self) -> Option[str]:
        """Native view of the held parts, absent for the default view."""
        return Some(next(iter(self.file.parts.values())).get_string_attribute("view")).filter(bool)

    @classmethod
    def found(cls, file: Multilayer, *, header: OpenImageIO.ImageSpec) -> Outcome[tuple[Self, ...]]:
        """Decodes `header` Cryptomatte declarations into actual rank groups, partitioned by native part view.

        Returns:
            Mattes in declaration and view order, `UNREADABLE` fault per undecodable manifest, or `OUTSIDE` for single-part multiview channels the native binding cannot separate.
        """
        declarations = {attribute.name: value for attribute in header.extra_attribs if isinstance(value := attribute.value, str)}
        if not (
            jobs := tuple(
                (name, manifest)
                for key, name in declarations.items()
                if key.startswith("cryptomatte/") and key.endswith("/name")
                for manifest in Option.of_optional(declarations.get(f"{key.removesuffix('/name')}/manifest")).to_list()
            )
        ):
            return Ok(())
        layout = accumulated(
            Error(Fault.of(Reason.OUTSIDE, f"{file.path}:{index}", multiview, member="per-part view metadata"))
            for index, spec in file.parts.items()
            if (multiview := spec.getattribute("multiView")) is not None
        )
        layers = sorted(file.layers, key=lambda owner: owner.name)
        part_views = {index: spec.get_string_attribute("view") for index, spec in file.parts.items()}
        parts: defaultdict[str, dict[int, OpenImageIO.ImageSpec]] = defaultdict(dict)
        for index, spec in file.parts.items():
            parts[part_views[index]][index] = spec
        views = {view: replace(file, parts=MappingProxyType(specs)) for view, specs in parts.items()}

        def decoded(name: str, manifest: str) -> Outcome[tuple[Self, ...]]:
            ranks: defaultdict[str, list[Layer]] = defaultdict(list)
            for layer in layers:
                if re.fullmatch(f"{re.escape(name)}[0-9]{{2}}", layer.name) and (folded := {channel.casefold(): channel for channel in layer.channels}).keys() >= set("rgba"):
                    ranks[part_views[layer.part]].append(replace(layer, channels=tuple(folded[key] for key in "rgba")))
            return (
                catch(lambda: msgspec.json.decode(manifest, type=_Manifest), exception=msgspec.DecodeError)()
                .map_error(partial(Fault.of, Reason.UNREADABLE, str(file.path)))
                .map(lambda entries: MappingProxyType({entity: code & 0x807FFFFF | min(max(code >> 23 & 0xFF, 1), 254) << 23 for entity, digits in entries.items() for code in (int(digits, 16),)}))
                .map(lambda ids: tuple(cls(name=name, ids=ids, ranks=tuple(ranked), file=views[view]) for view, ranked in ranks.items()))
            )

        return paired(layout, accumulated(itertools.starmap(decoded, jobs))).map(lambda groups: tuple(itertools.chain.from_iterable(groups[1])))

    def coverage(self, names: frozenset[str]) -> Outcome[npt.NDArray[np.float32]]:
        """Sums coverage of named IDs, unknown names adding nothing.

        Returns:
            `(height, width)` coverage, or `UNREADABLE` fault.
        """
        targets = np.fromiter((code for entity, code in self.ids.items() if entity in names), dtype=np.uint32)
        return self._planes().map(lambda planes: planes[1].sum(axis=-1, dtype=np.float32, where=np.isin(planes[0], targets)))

    def areas(self) -> Outcome[dict[str, float]]:
        """Measures frame share each manifest name covers.

        Returns:
            Share per name, 0 for names no pixel holds, or `UNREADABLE` fault.
        """

        def summed(planes: tuple[npt.NDArray[np.uint32], npt.NDArray[np.float32]]) -> dict[str, float]:
            codes, inverse = np.unique(planes[0], return_inverse=True)
            found = dict(zip(codes.tolist(), (np.bincount(inverse.reshape(-1), weights=planes[1].reshape(-1), minlength=codes.size) / np.prod(planes[0].shape[:2])).tolist()))
            return {entity: found.get(code, 0.0) for entity, code in self.ids.items()}

        return self._planes().map(summed)

    def filled(self, names: frozenset[str], color: npt.NDArray[np.float64]) -> Outcome[npt.NDArray[np.float32]]:
        """Fills named ID coverage with a `(3,)` color as associated RGBA.

        Returns:
            `(height, width, 4)` plane with coverage alpha, or `UNREADABLE` fault.
        """
        tint = np.append(color, 1.0).astype(np.float32)
        return self.coverage(names).map(lambda covered: covered[..., None] * tint)

    def _planes(self) -> Outcome[tuple[npt.NDArray[np.uint32], npt.NDArray[np.float32]]]:
        """ID codes and coverage weights of every rank pair, stacked along the last axis, `(height, width, 2 * ranks)`."""
        part = next(iter(self.file.parts.values()))
        return (
            accumulated(self.file.plane(rank, rank.channels) for rank in self.ranks)
            .map(lambda ranked: np.concatenate(ranked, axis=-1) if len(ranked) > 1 else ranked[0] if ranked else np.empty((part.height, part.width, 0), dtype=np.float32))
            .map(lambda stacked: (stacked.view(np.uint32)[..., 0::2], stacked[..., 1::2]))
        )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Layer", "Matte", "Multilayer", "Pass", "PassLayout", "PassRole"]
