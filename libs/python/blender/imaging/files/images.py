"""Image files, storage settings, and atomic writes."""

from collections.abc import Callable, Mapping
import contextlib
from enum import IntEnum
import functools
import pathlib
from types import MappingProxyType
import typing
from typing import Annotated, Final, Literal, Self

from expression import Error, Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from blender.imaging.color.processors import interop
from blender.imaging.faults import ImagingReason
from blender.imaging.files.coding import Depth
from blender.records.faults import Fault, Faults, Outcome, paired, RecordReason
from blender.records.fields import Case
from blender.records.stores import committed

# --- [TYPES] ----------------------------------------------------------------------------


class Codec(IntEnum):
    """Codec without level."""

    NONE = 1
    RLE = 2
    ZIPS = 3
    PIZ = 4
    PXR24 = 5
    B44 = 6
    B44A = 7
    HTJ2K32 = 8
    HTJ2K256 = 9
    LZW = 10
    PACKBITS = 11
    LOSSLESS = 12


class Blocks(IntEnum):
    """Scanlines per DWA block, its lowercased name the OpenEXR codec."""

    DWAA = 1
    DWAB = 2


# --- [TABLES] ---------------------------------------------------------------------------

_DEPTH_WRITERS: Final = MappingProxyType({
    Depth.UINT8: frozenset({"bmp", "dpx", "fits", "ico", "iff", "jpeg", "jpeg2000", "jpegxl", "png", "pnm", "rla", "sgi", "targa", "tiff", "webp"}),
    Depth.UINT16: frozenset({"dpx", "iff", "jpeg2000", "jpegxl", "png", "pnm", "rla", "sgi", "tiff"}),
    Depth.HALF: frozenset({"jpegxl", "openexr", "tiff"}),
    Depth.FLOAT: frozenset({"dpx", "fits", "hdr", "jpegxl", "openexr", "pnm", "rla", "tiff"}),
})
_CODEC_WRITERS: Final = MappingProxyType({
    Codec.NONE: frozenset({"openexr", "tiff"}),
    **dict.fromkeys((Codec.RLE, Codec.ZIPS, Codec.PIZ, Codec.PXR24, Codec.B44, Codec.B44A, Codec.HTJ2K32, Codec.HTJ2K256), frozenset({"openexr"})),
    **dict.fromkeys((Codec.LZW, Codec.PACKBITS), frozenset({"tiff"})),
    Codec.LOSSLESS: frozenset({"webp"}),
})

# --- [MODELS] ---------------------------------------------------------------------------


class Plain(Case, frozen=True, kw_only=True):
    """Codec without level."""

    writers = frozenset[str]().union(*_CODEC_WRITERS.values())
    margin = 0
    codec: Codec = Codec.NONE

    @property
    def lossy(self) -> bool:
        """Whether codec discards data."""
        return self.codec in {Codec.PXR24, Codec.B44, Codec.B44A}

    @property
    def attributes(self) -> tuple[tuple[str, OpenImageIO.TypeDesc, str | int], ...]:
        """Spec attributes the codec sets."""
        return (("compression", OpenImageIO.TypeString, self.codec.name.lower()),)


class Deflate(Case, frozen=True, kw_only=True):
    """Lossless zlib deflate at a level."""

    writers = frozenset({"openexr", "png", "tiff"})
    lossy = False
    margin = 0
    level: Annotated[int, msgspec.Meta(ge=1, le=9)] = 6

    @property
    def attributes(self) -> tuple[tuple[str, OpenImageIO.TypeDesc, str | int], ...]:
        """Spec attributes the level sets."""
        return (("compression", OpenImageIO.TypeString, f"zip:{self.level}"), ("png:compressionLevel", OpenImageIO.TypeInt, self.level))


class Dwa(Case, frozen=True, kw_only=True):
    """Lossy DCT compression of OpenEXR color channels."""

    writers = frozenset({"openexr"})
    lossy = True
    blocks: Blocks = Blocks.DWAA
    level: Annotated[int, msgspec.Meta(ge=0, le=450)] = 45

    @property
    def margin(self) -> int:
        """Half-float headroom level quantization needs."""
        return 2048 + 32 * self.level if self.level <= 50 else 3624 + 128 * (self.level - 50)

    @property
    def attributes(self) -> tuple[tuple[str, OpenImageIO.TypeDesc, str | int], ...]:
        """Spec attributes the blocks and level set."""
        return (("compression", OpenImageIO.TypeString, f"{self.blocks.name.lower()}:{self.level}"),)


class Quality(Case, frozen=True, kw_only=True):
    """Lossy compression at an encoder quality."""

    writers = frozenset({"jpeg", "webp"})
    lossy = True
    margin = 0
    level: Annotated[int, msgspec.Meta(ge=0, le=100)] = 90

    @property
    def attributes(self) -> tuple[tuple[str, OpenImageIO.TypeDesc, str | int], ...]:
        """Spec attributes the quality sets."""
        return (("CompressionQuality", OpenImageIO.TypeInt, self.level),)


type Compressions = Plain | Deflate | Dwa | Quality


class Storage(msgspec.Struct, frozen=True, kw_only=True):
    """Float plane depth and compression, integer codes stored at their dtype."""

    depth: Literal[16, 32] = 32
    compression: Option[Compressions] = Nothing

    @property
    def attributes(self) -> tuple[tuple[str, OpenImageIO.TypeDesc, str | int], ...]:
        """Spec attributes of compression, none for writer default."""
        return self.compression.map(lambda compression: compression.attributes).default_value(())

    def clipped[D: np.float32 | np.uint8 | np.uint16](self, plane: npt.NDArray[D]) -> npt.NDArray[D]:
        """Clips plane to half-float range less compression margin.

        Args:
            plane: Plane the write stores.

        Returns:
            Plane clipped to 65504 less margin at 16-bit depth, else plane unchanged.
        """
        bound = 65504.0 - self.compression.map(lambda compression: compression.margin).default_value(0)
        return np.clip(plane, -bound, bound) if self.depth == 16 else plane


class Format(msgspec.Struct, frozen=True, kw_only=True):
    """OpenImageIO extension list entry."""

    name: str
    extensions: tuple[str, ...]

    @classmethod
    def readable(cls) -> tuple[Self, ...]:
        """Lists every format a reader exists for.

        Returns:
            Extension list entries in OpenImageIO order.
        """
        entries = OpenImageIO.get_string_attribute("extension_list").split(";")
        return tuple(cls(name=name, extensions=tuple(suffixes.split(","))) for name, _, suffixes in (entry.partition(":") for entry in entries))

    @classmethod
    def writable(cls) -> tuple[Self, ...]:
        """Lists formats with a writer for some depth.

        Returns:
            Readable entries the output format list names, each with a depth.
        """
        writers = frozenset(OpenImageIO.get_string_attribute("output_format_list").split(","))
        return tuple(row for row in cls.readable() if row.name in writers and row.depths)

    @property
    def depths(self) -> tuple[Depth, ...]:
        """Depths format writer stores unchanged."""
        return tuple(depth for depth in Depth if self.name in _DEPTH_WRITERS[depth])

    @property
    def compressions(self) -> tuple[type[Compressions], ...]:
        """Compression cases writer accepts."""
        return tuple(form for form in typing.get_args(Compressions.__value__) if self.name in form.writers)


class Subimage[D: np.float32 | np.uint8 | np.uint16](msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Written subimage with pixels on demand."""

    spec: OpenImageIO.ImageSpec
    attributes: tuple[tuple[str, OpenImageIO.TypeDesc, object], ...]
    pixels: Callable[[], Outcome[npt.NDArray[D]]]


class ImageFile[D: np.float32 | np.uint8 | np.uint16](msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Pixels, colorspace, and header of read file."""

    pixels: npt.NDArray[D]
    colorspace: str
    header: tuple[OpenImageIO.ParamValue, ...] = ()

    def write(self, path: str, storage: Storage, fields: Mapping[str, str], config: PyOpenColorIO.Config) -> Outcome[str]:
        """Writes pixels with header, fields, storage, and colorspace tags.

        Args:
            path: File to write, suffix naming writer.
            storage: Stored channel type and compression.
            fields: Text fields stored by name.
            config: Config the colorspace resolves in.

        Returns:
            Path, `read` fault for profile IO, or `write` fault.
        """
        (height, width, channels), coded = self.pixels.shape, np.issubdtype(self.pixels.dtype, np.integer)
        identifier = Option.of_optional(config.getColorSpace(self.colorspace)).map(interop).filter(bool)
        bt709_transfer = 1

        def subimages(output: OpenImageIO.ImageOutput, icc: Option[bytes]) -> tuple[Subimage[D], ...]:
            tagged = bool(output.supports("cicp"))
            dropped = {"oiio", "iccprofile", "cicp", "colorinteropid", "chromaticities"} | ({row.name for row in Format.readable()} - {output.format_name()})
            attributes = (
                *((held.name, held.type, held.value) for held in self.header if held.name.partition(":")[0].lower() not in dropped),
                *((name, OpenImageIO.TypeString, text) for name, text in fields.items()),
                *storage.attributes,
                ("oiio:UnassociatedAlpha", OpenImageIO.TypeInt, int(coded)),
                ("tiff:half", OpenImageIO.TypeInt, 1),
                *(("ICCProfile", OpenImageIO.TypeDesc(f"uint8[{len(data)}]"), np.frombuffer(data, np.uint8)) for data in icc.to_list()),
                *(
                    ("CICP", OpenImageIO.TypeDesc("int[4]"), (code[0], code[1], 0, 1))
                    for code in identifier
                    .bind(lambda name: Option.of_optional(OpenImageIO.ColorConfig("ocio://default").get_cicp(name)))
                    .filter(lambda code: tagged and code[1] != bt709_transfer)
                    .to_list()
                ),
                *(("colorInteropID", OpenImageIO.TypeString, name) for name in identifier.filter(lambda _: not tagged).to_list()),
            )
            return (
                Subimage(
                    spec=OpenImageIO.ImageSpec(
                        width, height, channels, OpenImageIO.TypeDesc(self.pixels.dtype.name) if coded else OpenImageIO.TypeDesc(OpenImageIO.HALF if storage.depth == 16 else OpenImageIO.FLOAT)
                    ),
                    attributes=attributes,
                    pixels=lambda: Ok(self.pixels if coded else storage.clipped(self.pixels)),
                ),
            )

        return profile(config, self.colorspace).bind(lambda icc: emitted(path, lambda output: subimages(output, icc.filter(lambda _: channels >= 3))))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [IO]


def read_file(path: str, config: PyOpenColorIO.Config, *, colorspace: Option[str] = Nothing, options: Option[OpenImageIO.ImageSpec] = Nothing) -> Outcome[ImageFile[np.float32]]:
    """Reads every channel of first subimage as float32 with its colorspace.

    Args:
        path: File to read.
        config: Config the colorspace resolves in.
        colorspace: Name, alias, or role bypassing tags and file rules, inferred from file when `Nothing`.
        options: Reader configuration, OpenImageIO defaults when `Nothing`.

    Returns:
        File, or `read` or `unresolved` fault.
    """

    def decoded(image: OpenImageIO.ImageInput) -> Outcome[ImageFile[np.float32]]:
        spec, requested = image.spec(), colorspace.default_with(lambda: config.getColorSpaceFromFilepath(path)[0])
        resolved = (
            (
                Option.of_optional(config.getColorSpace(requested))
                if colorspace.is_some()
                else (
                    Option
                    .of_optional(spec.get_string_attribute("oiio:ColorSpace"))
                    .bind(lambda name: Option.of_optional(config.getColorSpace(name)))
                    .or_else_with(lambda: Nothing if config.filepathOnlyMatchesDefaultRule(path) else Option.of_optional(config.getColorSpace(requested)))
                    .or_else_with(
                        lambda: Option.of_optional(config.getColorSpace("default_float" if spec.format.basetype in {OpenImageIO.HALF, OpenImageIO.FLOAT, OpenImageIO.DOUBLE} else "default_byte"))
                    )
                    .or_else_with(lambda: Option.of_optional(config.getColorSpace(requested)))
                )
            )
            .map(PyOpenColorIO.ColorSpace.getName)
            .to_result_with(lambda: (Fault(reason=ImagingReason.UNRESOLVED, subject=config.getName(), member=requested),))
        )
        read = Option.of_optional(image.read_image(0, 0, 0, spec.nchannels, OpenImageIO.FLOAT)).to_result_with(lambda: (Fault(reason=RecordReason.UNREADABLE, subject=path, cause=image.geterror()),))
        return paired(resolved, read).map(lambda pair: ImageFile(pixels=pair[1], colorspace=pair[0], header=tuple(spec.extra_attribs)))

    return opened(path, decoded, options=options)


def opened[T](path: str, use: Callable[[OpenImageIO.ImageInput], Outcome[T]], *, options: Option[OpenImageIO.ImageSpec] = Nothing) -> Outcome[T]:
    """Opens a file for reading, uses it, and closes it on every path.

    Args:
        path: File to open.
        use: Reader of open input.
        options: Reader configuration, OpenImageIO defaults when `Nothing`.

    Returns:
        Reader value, or `read` fault for open, read, or close.
    """

    def held(image: OpenImageIO.ImageInput) -> Outcome[T]:
        with contextlib.closing(image):
            return use(image)

    image = options.map(lambda spec: OpenImageIO.ImageInput.open(path, spec)).default_with(lambda: OpenImageIO.ImageInput.open(path))
    return Option.of_optional(image).map(held).default_with(lambda: Error((Fault(reason=RecordReason.UNREADABLE, subject=path, cause=OpenImageIO.geterror()),)))


def written(path: str, write: Callable[[pathlib.Path], object]) -> Outcome[str]:
    """Runs one whole-file encoder over the stage of `path` through one atomic commit, creating its folder.

    Args:
        path: File to replace.
        write: Encoder opening, writing, and closing the stage path it receives.

    Returns:
        Path, or `write` fault for folder creation, encoding, or commit.
    """
    return committed(
        pathlib.Path(path),
        lambda stage: catch(catch(write, exception=OSError), exception=ValueError)(stage).map_error(lambda error: (Fault(reason=RecordReason.UNWRITABLE, subject=path, cause=error),)),
    ).map(str)


def emitted[D: np.float32 | np.uint8 | np.uint16](path: str, subimages: Callable[[OpenImageIO.ImageOutput], tuple[Subimage[D], ...]]) -> Outcome[str]:
    """Encodes subimages through one output, closing it before replacing file.

    Args:
        path: File to write, suffix naming writer.
        subimages: Subimages built for writer of path.

    Returns:
        Path, or `write` fault.
    """

    def refused(error: str) -> Faults:
        return (Fault(reason=RecordReason.UNWRITABLE, subject=path, cause=error),)

    def encode(stage: str) -> Outcome[str]:
        def into(output: OpenImageIO.ImageOutput) -> Outcome[str]:
            def answered(*, done: bool) -> Outcome[str]:
                return Ok(path) if done else Error(refused(output.geterror()))

            def specified(subimage: Subimage[D]) -> OpenImageIO.ImageSpec:
                spec = OpenImageIO.ImageSpec(subimage.spec)
                for name, kind, value in subimage.attributes:
                    spec.attribute(name, kind, value)
                return spec

            def step(written: Outcome[str], indexed: tuple[int, tuple[OpenImageIO.ImageSpec, Subimage[D]]]) -> Outcome[str]:
                index, (spec, subimage) = indexed
                return (
                    written
                    .bind(lambda _: answered(done=index == 0 or output.open(stage, spec, "AppendSubimage")))
                    .bind(lambda _: subimage.pixels())
                    .bind(lambda array: answered(done=output.write_image(array)))
                )

            with contextlib.closing(output):
                parts = subimages(output)
                specs = tuple(specified(part) for part in parts)
                return functools.reduce(step, enumerate(zip(specs, parts)), answered(done=output.open(stage, list(specs)))).bind(lambda _: answered(done=output.close()))

        return Option.of_optional(OpenImageIO.ImageOutput.create(path)).map(into).default_with(lambda: Error(refused(OpenImageIO.geterror())))

    return committed(pathlib.Path(path), lambda stage: encode(str(stage))).map(str)


# --- [COLOR_TAGS]


def profile(config: PyOpenColorIO.Config, colorspace: str) -> Outcome[Option[bytes]]:
    """Reads the ICC profile a colorspace names from config search path.

    Args:
        config: Config the colorspace resolves in.
        colorspace: Colorspace name.

    Returns:
        Profile, `Nothing` when the colorspace names none, or `read` fault for a named profile the search path misses or fails to read.
    """
    context = config.getCurrentContext()

    def loaded(name: str) -> Outcome[Option[bytes]]:
        refused: Callable[[Exception], Faults] = lambda error: (Fault(reason=RecordReason.UNREADABLE, subject=name, cause=error),)
        return (
            catch(context.resolveFileLocation, exception=PyOpenColorIO.ExceptionMissingFile)(name)
            .map_error(refused)
            .bind(lambda location: catch(pathlib.Path(location).read_bytes, exception=OSError)().map_error(refused))
            .map(Some)
        )

    return (
        Option
        .of_optional(config.getColorSpace(colorspace))
        .map(lambda space: str(space.getInterchangeAttribute("icc_profile_name")))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        .filter(bool)
        .map(loaded)
        .default_value(Ok(Nothing))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Blocks", "Codec", "Compressions", "Deflate", "Dwa", "Format", "ImageFile", "Plain", "Quality", "Storage", "Subimage", "emitted", "opened", "profile", "read_file", "written"]
