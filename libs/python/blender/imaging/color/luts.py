"""LUT formats, encoded lattices, and text and image LUT files."""

from collections.abc import Iterable, Mapping
from enum import IntEnum
from functools import partial
import math
import pathlib
import sys
from typing import Annotated, ClassVar, Self

from expression import Error, Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from blender.imaging.color.processors import converted, processor
from blender.imaging.faults import ImagingReason
from blender.imaging.files.images import emitted, ImageFile, read_file, Subimage
from blender.records.faults import accumulated, Fault, Outcome, RecordReason
from blender.records.fields import Case
from blender.records.stores import committed

# --- [TYPES] ----------------------------------------------------------------------------


class LutLayout(IntEnum):
    """Arrangement of uniform cube nodes in an image."""

    HALD = 1
    KEYSHOT = 2

    @classmethod
    def read(cls, path: str, config: PyOpenColorIO.Config) -> Outcome[PyOpenColorIO.GroupTransform]:
        """Reads Hald or KeyShot image as uniform cube, image dimensions selecting layout.

        Returns:
            Table group, or `read`, LUT shape, and `color` faults.
        """

        def decoded(image: ImageFile[np.float32]) -> Outcome[PyOpenColorIO.GroupTransform]:
            if (pixels := image.pixels).ndim != 3:
                return Error(Fault.of(ImagingReason.SHAPE, path, member=str(pixels.shape)))
            height, width, channels = pixels.shape
            rgb = pixels[..., :3] if channels >= 3 else np.broadcast_to(pixels[..., :1], (height, width, 3))
            level = round(np.cbrt(width))
            match height, width:
                case _ if height == width == level**3:
                    size = level**2
                    plane = rgb.reshape(size, size * size, 3)
                case _ if width == height * height:
                    size = height
                    plane = rgb[::-1].reshape(size, size, size, 3).transpose(1, 0, 2, 3).reshape(size, size * size, 3)
                case _:
                    return Error(Fault.of(ImagingReason.SHAPE, path, member=str(pixels.shape)))
            return catch(Lattice(size=size).transform, exception=PyOpenColorIO.Exception)(plane).map_error(partial(Fault.of, ImagingReason.COLOR, path))

        options = OpenImageIO.ImageSpec()
        options.attribute("oiio:UnassociatedAlpha", 1)
        return read_file(path, config, colorspace=Some(PyOpenColorIO.ROLE_DATA), options=Some(options)).bind(decoded)

    def written(self, table: PyOpenColorIO.Lut3DTransform, path: str) -> Outcome[str]:
        """Writes stored cube nodes as 16-bit image to a UINT16 writer path.

        Returns:
            Path, LUT shape fault when Hald size is no perfect square, or `write` fault.
        """
        size = table.getGridSize()
        cube = table.getData().reshape(size, size, size, 3).transpose(2, 1, 0, 3)
        match self:
            case LutLayout.HALD:
                if (level := math.isqrt(size)) * level != size:
                    return Error(Fault.of(ImagingReason.SHAPE, path, member=str(cube.shape)))
                side = level**3
                pixels = cube.reshape(side, side, 3)
            case LutLayout.KEYSHOT:
                pixels = cube.transpose(1, 0, 2, 3)[::-1].reshape(size, size * size, 3)
        height, width, channels = pixels.shape
        return emitted(path, lambda _: (Subimage(spec=OpenImageIO.ImageSpec(width, height, channels, OpenImageIO.UINT16), attributes=(), pixels=lambda: Ok(np.ascontiguousarray(pixels))),))


# --- [MODELS] ---------------------------------------------------------------------------


class LutFormat(msgspec.Struct, frozen=True, kw_only=True):
    """OCIO format name with file extension."""

    name: str
    extension: str

    @classmethod
    def of(cls, formats: Iterable[tuple[str, str]]) -> tuple[Self, ...]:
        """Returns records of format pairs in input order."""
        return tuple(cls(name=name, extension=extension) for name, extension in formats)

    @classmethod
    def bakeable(cls) -> tuple[Self, ...]:
        """Returns formats `Baker` writes."""
        return cls.of(PyOpenColorIO.Baker.getFormats())

    @classmethod
    def writable(cls) -> tuple[Self, ...]:
        """Returns formats `GroupTransform` writes exactly."""
        return cls.of(PyOpenColorIO.GroupTransform.GetWriteFormats())

    @classmethod
    def readable(cls) -> tuple[Self, ...]:
        """Returns formats `FileTransform` reads."""
        return cls.of(PyOpenColorIO.FileTransform.getFormats())

    @classmethod
    def matrices(cls) -> tuple[Self, ...]:
        """Returns formats both `GroupTransform` and `Baker` write."""
        return tuple(filter(frozenset(cls.bakeable()).__contains__, cls.writable()))


class Uniform(Case, frozen=True, kw_only=True):
    """Nodes evenly spaced between allocation endpoints."""

    limit: ClassVar[float] = float(np.nextafter(np.float32(np.inf), np.float32(0)))
    low: Annotated[float, msgspec.Meta(ge=-limit, le=limit)] = 0.0
    high: Annotated[float, msgspec.Meta(ge=-limit, le=limit)] = 1.0


class Logarithmic(Case, frozen=True, kw_only=True):
    """Nodes evenly spaced in stops, scene zero on first node."""

    low: Annotated[float, msgspec.Meta(ge=-24.0, le=16.0)] = -10.0
    span: Annotated[float, msgspec.Meta(ge=sys.float_info.min, le=40.0)] = 20.0


class Rational(Case, frozen=True):
    """Nodes encoded by x / (x + 1) over nonnegative half floats."""


type Spacing = Uniform | Logarithmic | Rational


class Lattice(msgspec.Struct, frozen=True, kw_only=True):
    """Cube edge length and node encoding."""

    size: Annotated[int, msgspec.Meta(ge=2, le=129)] = 33
    spacing: Spacing = Uniform()

    @property
    def head(self) -> Option[PyOpenColorIO.Transform]:
        """Node encoding, absent for default zero-to-one spacing."""
        match self.spacing:
            case Uniform(low=low, high=high):
                return Nothing if (low, high) == (0.0, 1.0) else Some(PyOpenColorIO.AllocationTransform(allocation=PyOpenColorIO.ALLOCATION_UNIFORM, vars=[low, high]))
            case Logarithmic(low=low, span=span):
                return Some(PyOpenColorIO.AllocationTransform(allocation=PyOpenColorIO.ALLOCATION_LG2, vars=[low, low + span, 2.0**low]))
            case Rational():
                values = np.arange(int(np.array(np.inf, dtype=np.float16).view(np.uint16)), dtype=np.uint16).view(np.float16).astype(np.float32)
                compressed = np.zeros(np.iinfo(np.uint16).max + 1, dtype=np.float32)
                compressed[: values.size] = values / (values + 1.0)
                compressed[values.size] = 1.0
                table = PyOpenColorIO.Lut1DTransform(length=compressed.size, inputHalfDomain=True)
                table.setData(np.repeat(compressed, 3))
                return Some(table)

    def samples(self) -> Outcome[npt.NDArray[np.float32]]:
        """Returns decoded RGB node inputs, or color fault."""
        span = self.spacing if isinstance(self.spacing, Uniform) else Uniform()
        grid = np.linspace(span.low, span.high, self.size, dtype=np.float64).astype(np.float32)
        blue, green, red = np.meshgrid(grid, grid, grid, indexing="ij")
        plane = np.stack((red, green, blue), axis=-1).reshape(self.size, self.size * self.size, 3)
        return (
            self.head
            .filter(lambda _: not isinstance(self.spacing, Uniform))
            .map(lambda head: converted(PyOpenColorIO.Config(), PyOpenColorIO.GroupTransform([head], direction=PyOpenColorIO.TRANSFORM_DIR_INVERSE), plane))
            .default_value(Ok(plane))
        )

    def transform(self, outputs: npt.NDArray[np.float32]) -> PyOpenColorIO.GroupTransform:
        """Builds tetrahedral table of `outputs` after node encoding."""
        table = PyOpenColorIO.Lut3DTransform(gridSize=self.size, interpolation=PyOpenColorIO.INTERP_TETRAHEDRAL)
        table.setData(np.ascontiguousarray(outputs.reshape(self.size, self.size, self.size, 3).transpose(2, 1, 0, 3)).reshape(-1))
        return PyOpenColorIO.GroupTransform(transforms=[*self.head.to_list(), table])


class LutOutput(msgspec.Struct, frozen=True, kw_only=True):
    """LUT file format, its sources resampled at lattice nodes when one is held, else written operator for operator."""

    format: LutFormat
    lattice: Option[Lattice] = Nothing

    def written(self, sources: Mapping[str, PyOpenColorIO.GroupTransform]) -> Outcome[tuple[str, ...]]:
        """Encodes every source before publishing each path atomically in mapping order."""

        def baked(source: PyOpenColorIO.GroupTransform, lattice: Lattice) -> Outcome[str]:
            @catch(exception=PyOpenColorIO.Exception)
            def text() -> str:
                config = PyOpenColorIO.Config()
                entry = PyOpenColorIO.ColorSpace(name="source")
                target = PyOpenColorIO.ColorSpace(name="target", fromReference=source)
                shapers = [PyOpenColorIO.ColorSpace(name="shaper", fromReference=head) for head in lattice.head.to_list()]
                for space in (entry, target, *shapers):
                    config.addColorSpace(space)
                return PyOpenColorIO.Baker(
                    config=config,
                    format=self.format.name,
                    inputSpace=entry.getName(),
                    targetSpace=target.getName(),
                    cubeSize=lattice.size,
                    shaperSize=65536,
                    shaperSpace=shapers[0].getName() if shapers else "",
                ).bake()

            return text().map_error(partial(Fault.of, ImagingReason.COLOR, self.format.name))

        def encoded(item: tuple[str, PyOpenColorIO.GroupTransform]) -> Outcome[tuple[str, str]]:
            path, source = item

            @catch(exception=PyOpenColorIO.Exception)
            def exact() -> str:
                return source.write(formatName=self.format.name, config=PyOpenColorIO.Config())

            return self.lattice.map(lambda lattice: baked(source, lattice)).default_with(lambda: exact().map_error(partial(Fault.of, ImagingReason.COLOR, path))).map(lambda text: (path, text))

        def installed(item: tuple[str, str]) -> Outcome[str]:
            path, text = item
            return committed(
                pathlib.Path(path), lambda stage: catch(stage.write_text, exception=OSError)(text, encoding="utf-8", newline="").map_error(partial(Fault.of, RecordReason.UNWRITABLE, path))
            ).map(str)

        return accumulated(map(encoded, sources.items())).bind(lambda texts: accumulated(map(installed, texts)))


# --- [OPERATIONS] -----------------------------------------------------------------------


def split(config: PyOpenColorIO.Config, transform: PyOpenColorIO.Transform) -> Outcome[tuple[PyOpenColorIO.GroupTransform, Option[tuple[Lattice, npt.NDArray[np.float32]]]]]:
    """Returns the processor operations of `transform` before its first 3D table, and that table's lattice with the suffix sampled at its nodes."""

    def sampled(built: PyOpenColorIO.Processor) -> Outcome[tuple[PyOpenColorIO.GroupTransform, Option[tuple[Lattice, npt.NDArray[np.float32]]]]]:
        operations = list(built.createGroupTransform())
        return (
            Option
            .of_optional(next(((index, Lattice(size=operation.getGridSize())) for index, operation in enumerate(operations) if isinstance(operation, PyOpenColorIO.Lut3DTransform)), None))
            .map(
                lambda found: (
                    found[1]
                    .samples()
                    .bind(lambda samples: converted(PyOpenColorIO.Config(), PyOpenColorIO.GroupTransform(transforms=operations[found[0] :]), samples))
                    .map(lambda outputs: (PyOpenColorIO.GroupTransform(transforms=operations[: found[0]]), Some((found[1], outputs))))
                )
            )
            .default_with(lambda: Ok((PyOpenColorIO.GroupTransform(transforms=operations), Nothing)))
        )

    return processor(config, transform).bind(sampled)


def corrections(path: str) -> Outcome[tuple[tuple[Option[str], PyOpenColorIO.CDLTransform], ...]]:
    """Reads every ASC correction of a CC, CCC, or CDL file in file order, each with its ID when named."""
    return (
        catch(PyOpenColorIO.CDLTransform.CreateGroupFromFile, exception=PyOpenColorIO.Exception)(path)
        .map_error(partial(Fault.of, ImagingReason.COLOR, path))
        .map(lambda group: tuple((Some(correction.getFormatMetadata().getID()).filter(bool), correction) for correction in group if isinstance(correction, PyOpenColorIO.CDLTransform)))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Lattice", "Logarithmic", "LutFormat", "LutLayout", "LutOutput", "Rational", "Spacing", "Uniform", "corrections", "split"]
