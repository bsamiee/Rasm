"""LUT formats, node spacings and encoded lattices, and text and image LUT files."""

from collections.abc import Iterable, Mapping
from enum import IntEnum
import math
import pathlib
import sys
from typing import Annotated, Final, Self

from expression import Error, Nothing, Ok, Option, Some, try_downcast
from expression.collections import Block
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from blender.imaging.color.processors import called, converted, processor
from blender.imaging.files.images import emitted, ImageFile, read_file, Subimage
from blender.records.faults import accumulated, Fault, Outcome, Reason
from blender.records.fields import Case
from blender.records.stores import committed

# --- [TYPES] ----------------------------------------------------------------------------

type _Single = Annotated[float, msgspec.Meta(ge=-float(np.finfo(np.float32).max), le=float(np.finfo(np.float32).max))]


class LutLayout(IntEnum):
    """Arrangement of uniform cube nodes in an image."""

    HALD = 1
    KEYSHOT = 2

    def written(self, table: PyOpenColorIO.Lut3DTransform, path: pathlib.Path) -> Outcome[pathlib.Path]:
        """Writes stored cube nodes as 16-bit image to a UINT16 writer path.

        Returns:
            Path, outside fault when a Hald size is no perfect square, or `write` fault.
        """
        size = table.getGridSize()
        cube = table.getData().reshape(size, size, size, 3).transpose(2, 1, 0, 3)
        match self:
            case LutLayout.HALD:
                if (level := math.isqrt(size)) * level != size:
                    return Error(Fault.of(Reason.OUTSIDE, str(path), str(size), member="Hald grid sizes that are perfect squares"))
                side = level**3
                pixels = cube.reshape(side, side, 3)
            case LutLayout.KEYSHOT:
                pixels = cube.transpose(1, 0, 2, 3)[::-1].reshape(size, size * size, 3)
        height, width, channels = pixels.shape
        return emitted(path, lambda _: (Subimage(spec=OpenImageIO.ImageSpec(width, height, channels, OpenImageIO.UINT16), attributes=(), pixels=lambda: Ok(np.ascontiguousarray(pixels))),))

    @classmethod
    def read(cls, path: pathlib.Path, config: PyOpenColorIO.Config) -> Outcome[PyOpenColorIO.GroupTransform]:
        """Reads a Hald or KeyShot image as a uniform cube, its dimensions selecting the layout.

        Returns:
            Table group, or `read`, outside, and `color` faults.
        """

        def decoded(image: ImageFile) -> Outcome[PyOpenColorIO.GroupTransform]:
            refused = Fault.of(Reason.OUTSIDE, str(path), str(image.pixels.shape), member="Hald and KeyShot cube images")
            if (pixels := image.pixels).ndim != 3:
                return Error(refused)
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
                    return Error(refused)
            return called(Lattice(size=size).transform, str(path), plane)

        options = OpenImageIO.ImageSpec()
        options.attribute("oiio:UnassociatedAlpha", 1)
        return read_file(path, config, colorspace=Some(PyOpenColorIO.ROLE_DATA), options=Some(options)).bind(decoded)


# --- [CONSTANTS] ------------------------------------------------------------------------

_HALF_LOW: Final = math.log2(float(np.finfo(np.float16).smallest_subnormal))
_HALF_HIGH: Final = math.log2(float(np.finfo(np.float16).max))

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


# --- [SPACINGS]


class Uniform(Case, frozen=True, kw_only=True):
    """Nodes evenly spaced between allocation endpoints."""

    low: _Single = 0.0
    high: _Single = 1.0

    @property
    def shaper(self) -> Option[PyOpenColorIO.Transform]:
        """Node encoding, absent for zero-to-one nodes."""
        return Nothing if (self.low, self.high) == (0.0, 1.0) else Some(PyOpenColorIO.AllocationTransform(allocation=PyOpenColorIO.ALLOCATION_UNIFORM, vars=[self.low, self.high]))


class Logarithmic(Case, frozen=True, kw_only=True):
    """Nodes evenly spaced in stops, scene zero on first node."""

    low: Annotated[float, msgspec.Meta(ge=_HALF_LOW, le=_HALF_HIGH)] = -10.0
    span: Annotated[float, msgspec.Meta(ge=sys.float_info.min, le=_HALF_HIGH - _HALF_LOW)] = 20.0

    @property
    def shaper(self) -> Option[PyOpenColorIO.Transform]:
        """Base-2 allocation of the stop window."""
        return Some(PyOpenColorIO.AllocationTransform(allocation=PyOpenColorIO.ALLOCATION_LG2, vars=[self.low, self.low + self.span, 2.0**self.low]))


class Rational(Case, frozen=True):
    """Nodes encoded by x / (x + 1) over nonnegative half floats."""

    @property
    def shaper(self) -> Option[PyOpenColorIO.Transform]:
        """Half-domain table of x / (x + 1)."""
        values = np.arange(int(np.array(np.inf, dtype=np.float16).view(np.uint16)), dtype=np.uint16).view(np.float16).astype(np.float32)
        compressed = np.zeros(np.iinfo(np.uint16).max + 1, dtype=np.float32)
        compressed[: values.size] = values / (values + 1.0)
        compressed[values.size] = 1.0
        table = PyOpenColorIO.Lut1DTransform(length=compressed.size, inputHalfDomain=True)
        table.setData(np.repeat(compressed, 3))
        return Some(table)


type Spacing = Uniform | Logarithmic | Rational

# --- [LATTICES]


class Lattice(msgspec.Struct, frozen=True, kw_only=True):
    """Cube edge length and node spacing."""

    size: Annotated[int, msgspec.Meta(ge=2, le=129)] = 33
    spacing: Spacing = Uniform()

    def samples(self) -> Outcome[npt.NDArray[np.float32]]:
        """Returns RGB node inputs, evenly spaced codes decoded through the inverse of the spacing's shaper, or color fault."""
        codes = np.repeat(np.linspace(0.0, 1.0, self.size, dtype=np.float64).astype(np.float32)[:, None], 3, axis=1)

        def planed(decoded: npt.NDArray[np.float32]) -> npt.NDArray[np.float32]:
            grid = decoded[:, 0]
            blue, green, red = np.meshgrid(grid, grid, grid, indexing="ij")
            return np.stack((red, green, blue), axis=-1).reshape(self.size, self.size * self.size, 3)

        return (
            self.spacing.shaper
            .map(lambda shaper: converted(PyOpenColorIO.Config(), PyOpenColorIO.GroupTransform([shaper], direction=PyOpenColorIO.TRANSFORM_DIR_INVERSE), codes))
            .default_value(Ok(codes))
            .map(planed)
        )

    def transform(self, outputs: npt.NDArray[np.float32]) -> PyOpenColorIO.GroupTransform:
        """Builds tetrahedral table of `outputs` after node encoding."""
        table = PyOpenColorIO.Lut3DTransform(gridSize=self.size, interpolation=PyOpenColorIO.INTERP_TETRAHEDRAL)
        table.setData(np.ascontiguousarray(outputs.reshape(self.size, self.size, self.size, 3).transpose(2, 1, 0, 3)).reshape(-1))
        return PyOpenColorIO.GroupTransform(transforms=[*self.spacing.shaper.to_list(), table])


class LutOutput(msgspec.Struct, frozen=True, kw_only=True):
    """LUT file format, its sources resampled at lattice nodes when one is held, else written operator for operator."""

    format: LutFormat
    lattice: Option[Lattice] = Nothing

    def written(self, sources: Mapping[pathlib.Path, PyOpenColorIO.GroupTransform]) -> Outcome[tuple[pathlib.Path, ...]]:
        """Encodes every source before publishing each path atomically in mapping order."""

        def baked(source: PyOpenColorIO.GroupTransform, lattice: Lattice) -> str:
            config = PyOpenColorIO.Config()
            entry = PyOpenColorIO.ColorSpace(name="source")
            target = PyOpenColorIO.ColorSpace(name="target", fromReference=source)
            shapers = [PyOpenColorIO.ColorSpace(name="shaper", fromReference=shaper) for shaper in lattice.spacing.shaper.to_list()]
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

        def encoded(item: tuple[pathlib.Path, PyOpenColorIO.GroupTransform]) -> Outcome[tuple[pathlib.Path, str]]:
            path, source = item

            def exact() -> str:
                return source.write(formatName=self.format.name, config=PyOpenColorIO.Config())

            return self.lattice.map(lambda lattice: called(baked, str(path), source, lattice)).default_with(lambda: called(exact, str(path))).map(lambda text: (path, text))

        return accumulated(map(encoded, sources.items())).bind(lambda texts: accumulated(committed(path, text.encode()) for path, text in texts))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [CORRECTIONS]


def corrections(path: pathlib.Path) -> Outcome[tuple[tuple[Option[str], PyOpenColorIO.CDLTransform], ...]]:
    """Reads every ASC correction of a CC, CCC, or CDL file in file order, each with its ID when named."""
    return called(PyOpenColorIO.CDLTransform.CreateGroupFromFile, str(path), str(path)).map(
        lambda group: tuple((Some(correction.getFormatMetadata().getID()).filter(bool), correction) for correction in group if isinstance(correction, PyOpenColorIO.CDLTransform))
    )


# --- [TABLES]


def split(config: PyOpenColorIO.Config, transform: PyOpenColorIO.Transform) -> Outcome[tuple[PyOpenColorIO.GroupTransform, Option[tuple[Lattice, npt.NDArray[np.float32]]]]]:
    """Returns the processor operations of `transform` before its first 3D table, and that table's lattice with the suffix sampled at its nodes."""

    def sampled(built: PyOpenColorIO.Processor) -> Outcome[tuple[PyOpenColorIO.GroupTransform, Option[tuple[Lattice, npt.NDArray[np.float32]]]]]:
        operations = list(built.createGroupTransform())
        return (
            Block
            .of_seq(enumerate(operations))
            .choose(lambda pair: Option.of_optional(try_downcast(PyOpenColorIO.Lut3DTransform, pair[1])).map(lambda table: (pair[0], Lattice(size=table.getGridSize()))))
            .try_head()
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


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Lattice", "Logarithmic", "LutFormat", "LutLayout", "LutOutput", "Rational", "Spacing", "Uniform", "corrections", "split"]
