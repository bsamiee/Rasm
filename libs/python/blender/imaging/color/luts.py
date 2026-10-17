"""LUT formats, encoded lattices, and native text and image interchange."""

from collections.abc import Iterable, Mapping
import math
import pathlib
from typing import Annotated, ClassVar, Self

from expression import Error, Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from blender.imaging.color.processors import applied, processor
from blender.imaging.faults import accumulated, colored, ImagingLutShapeFault, ImagingResult, ImagingWriteFault
from blender.imaging.files.images import emitted, ImageFile, published, read_file, Subimage
from blender.imaging.parameters import Case, Choice

# --- [TYPES] ----------------------------------------------------------------------------


class LutLayout(Choice):
    """Arrangement of uniform cube nodes in an image."""

    HALD = 1
    KEYSHOT = 2, "KeyShot"


# --- [MODELS] ---------------------------------------------------------------------------


class LutFormat(msgspec.Struct, frozen=True, kw_only=True):
    """Provider's format name and file extension stored together."""

    name: str
    extension: str

    @classmethod
    def of(cls, formats: Iterable[tuple[str, str]]) -> tuple[Self, ...]:
        """Converts native format pairs into persistent format records in provider order."""
        return tuple(cls(name=name, extension=extension) for name, extension in formats)

    @classmethod
    def bakeable(cls) -> tuple[Self, ...]:
        """Formats the native baker writes."""
        return cls.of(PyOpenColorIO.Baker.getFormats())

    @classmethod
    def writable(cls) -> tuple[Self, ...]:
        """Formats a native transform group writes exactly."""
        return cls.of(PyOpenColorIO.GroupTransform.GetWriteFormats())

    @classmethod
    def readable(cls) -> tuple[Self, ...]:
        """Formats a native file transform reads."""
        return cls.of(PyOpenColorIO.FileTransform.getFormats())

    @classmethod
    def matrices(cls) -> tuple[Self, ...]:
        """Exactly written formats the native baker also writes."""
        return tuple(row for row in cls.writable() if row in cls.bakeable())


class Uniform(Case, frozen=True, kw_only=True, tag=1):
    """Nodes evenly spaced between native allocation endpoints."""

    limit: ClassVar[float] = float(np.nextafter(np.float32(np.inf), np.float32(0)))
    low: Annotated[float, msgspec.Meta(ge=-limit, le=limit, description="Input at the first uniform node")] = 0.0
    high: Annotated[float, msgspec.Meta(ge=-limit, le=limit, description="Input at the last uniform node")] = 1.0


class Logarithmic(Case, frozen=True, kw_only=True, tag=2):
    """Nodes evenly spaced in stops with scene zero on the first node."""

    low: Annotated[float, msgspec.Meta(ge=-24.0, le=16.0, description="Stops from 1.0 to the first node above zero")] = -10.0
    span: Annotated[float, msgspec.Meta(gt=0.0, le=40.0, description="Stops from the low allocation to the high allocation")] = 20.0


class Rational(Case, frozen=True, tag=3):
    """Nodes encoded by x / (x + 1) over the nonnegative half-float domain."""


type Spacing = Uniform | Logarithmic | Rational


class Lattice(msgspec.Struct, frozen=True, kw_only=True):
    """Cube's edge length and node encoding."""

    size: Annotated[int, msgspec.Meta(ge=2, le=129, description="Nodes along each axis")] = 33
    spacing: Annotated[Spacing, msgspec.Meta(description="Encoding ahead of the table")] = Uniform()

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

    def samples(self) -> ImagingResult[npt.NDArray[np.float32]]:
        """Returns decoded RGB node inputs, or a native encoding processor refusal."""
        grid = (
            np.linspace(self.spacing.low, self.spacing.high, self.size, dtype=np.float64).astype(np.float32)
            if isinstance(self.spacing, Uniform)
            else np.linspace(0.0, 1.0, self.size, dtype=np.float32)
        )
        blue, green, red = np.meshgrid(grid, grid, grid, indexing="ij")
        plane = np.stack((red, green, blue), axis=-1).reshape(self.size, self.size * self.size, 3)
        return self.head.map(
            lambda head: processor(PyOpenColorIO.Config(), PyOpenColorIO.GroupTransform([head], direction=PyOpenColorIO.TRANSFORM_DIR_INVERSE)).map(
                lambda built: plane if isinstance(self.spacing, Uniform) else applied(built.getDefaultCPUProcessor(), plane)
            )
        ).default_value(Ok(plane))

    def transform(self, outputs: npt.NDArray[np.float32]) -> PyOpenColorIO.GroupTransform:
        """Builds a tetrahedral table from sampled outputs and its node encoding."""
        table = PyOpenColorIO.Lut3DTransform(gridSize=self.size, interpolation=PyOpenColorIO.INTERP_TETRAHEDRAL)
        table.setData(np.ascontiguousarray(outputs.reshape(self.size, self.size, self.size, 3).transpose(2, 1, 0, 3)).reshape(-1))
        return PyOpenColorIO.GroupTransform(transforms=[*self.head.to_list(), table])


class Bake(Case, frozen=True, kw_only=True, tag=1):
    """Source resampled into native format tables at encoded lattice nodes."""

    format: LutFormat
    lattice: Lattice = Lattice()


class Exact(Case, frozen=True, kw_only=True, tag=2):
    """Source's operators written without resampling."""

    format: LutFormat


type LutOutput = Bake | Exact


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [IMAGES]


def image_read(path: str, config: PyOpenColorIO.Config) -> ImagingResult[PyOpenColorIO.GroupTransform]:
    """Reads a Hald or KeyShot image as a native uniform cube.

    Args:
        path: Image to read, its dimensions selecting the layout.
        config: Config resolving the data role without a color conversion.

    Returns:
        The native table group; image IO, unsupported shape, or native table refusals.
    """

    def decoded(image: ImageFile[np.float32]) -> ImagingResult[PyOpenColorIO.GroupTransform]:
        if (pixels := (image.pixels)).ndim != 3:
            return Error((ImagingLutShapeFault(path=path, shape=pixels.shape),))
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
                return Error((ImagingLutShapeFault(path=path, shape=pixels.shape),))
        return colored(path, lambda: Lattice(size=size).transform(plane))

    options = OpenImageIO.ImageSpec()
    options.attribute("oiio:UnassociatedAlpha", 1)
    return read_file(path, config, colorspace=Some(PyOpenColorIO.ROLE_DATA), options=Some(options)).bind(decoded)


def image_written(layout: LutLayout, table: PyOpenColorIO.Lut3DTransform, path: str) -> ImagingResult[str]:
    """Exports a native cube's stored nodes through a 16-bit image spec.

    Args:
        layout: Arrangement of the cube's nodes in the image.
        table: Uniform cube whose stored values are exported.
        path: Final image path, using a writer that preserves UINT16 for 16-bit nodes.

    Returns:
        The final path; an incompatible Hald edge or native image write refusal.
    """
    size = table.getGridSize()
    cube = table.getData().reshape(size, size, size, 3).transpose(2, 1, 0, 3)
    match layout:
        case LutLayout.HALD:
            if (level := (math.isqrt(size))) * level != size:
                return Error((ImagingLutShapeFault(path=path, shape=cube.shape),))
            side = level**3
            pixels = cube.reshape(side, side, 3)
        case LutLayout.KEYSHOT:
            pixels = cube.transpose(1, 0, 2, 3)[::-1].reshape(size, size * size, 3)
    height, width, channels = pixels.shape
    return emitted(path, lambda _: (Subimage(spec=OpenImageIO.ImageSpec(width, height, channels, OpenImageIO.UINT16), attributes=(), pixels=lambda: Ok(np.ascontiguousarray(pixels))),))


# --- [TEXT]


def written(output: LutOutput, sources: Mapping[str, PyOpenColorIO.GroupTransform]) -> ImagingResult[tuple[str, ...]]:
    """Encodes every source before publishing each path atomically in mapping order."""

    def encoded(item: tuple[str, PyOpenColorIO.GroupTransform]) -> ImagingResult[tuple[str, str]]:
        path, source = item
        match output:
            case Exact():
                text = colored(source, lambda: source.write(formatName=output.format.name, config=PyOpenColorIO.Config()))
            case Bake():

                def baker() -> PyOpenColorIO.Baker:
                    config = PyOpenColorIO.Config()
                    entry = PyOpenColorIO.ColorSpace(name="source")
                    target = PyOpenColorIO.ColorSpace(name="target", fromReference=source)
                    shapers = [PyOpenColorIO.ColorSpace(name="shaper", fromReference=head) for head in output.lattice.head.to_list()]
                    for space in (entry, target, *shapers):
                        config.addColorSpace(space)
                    return PyOpenColorIO.Baker(
                        config=config,
                        format=output.format.name,
                        inputSpace=entry.getName(),
                        targetSpace=target.getName(),
                        cubeSize=output.lattice.size,
                        shaperSize=65536,
                        shaperSpace=shapers[0].getName() if shapers else "",
                    )

                text = colored(output.format.name, baker).bind(lambda built: colored(built, built.bake))
        return text.map(lambda written: (path, written))

    def installed(item: tuple[str, str]) -> ImagingResult[str]:
        path, text = item
        return published(
            path,
            lambda destination: catch(lambda: pathlib.Path(destination).write_text(text, encoding="utf-8", newline=""), exception=OSError)().map_error(
                lambda error: (ImagingWriteFault(request=path, error=error),)
            ),
        )

    return accumulated(map(encoded, sources.items())).bind(lambda texts: accumulated(map(installed, texts)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Bake", "Exact", "Lattice", "Logarithmic", "LutFormat", "LutLayout", "LutOutput", "Rational", "Spacing", "Uniform", "image_read", "image_written", "written"]
