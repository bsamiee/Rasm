"""Image files, storage settings, JPEG generation loss, and atomic writes."""

from collections.abc import Callable, Mapping
import contextlib
from enum import StrEnum
import functools
import pathlib
import tempfile
from types import MappingProxyType
import typing
from typing import Annotated, Final, IO, Literal, Self

from expression import Error, Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from blender.imaging.faults import ImagingReason
from blender.imaging.files.buffers import checked, Depth, pixels
from blender.records.faults import Fault, Outcome, paired, RecordReason
from blender.records.fields import Case
from blender.records.stores import committed

# --- [TYPES] ----------------------------------------------------------------------------


type Subsampling = Literal["4:4:4", "4:2:2", "4:2:0", "4:1:1"]


class Codec(StrEnum):
    """Codec without level, its value the OpenImageIO compression name."""

    NONE = "none"
    RLE = "rle"
    ZIPS = "zips"
    PIZ = "piz"
    PXR24 = "pxr24"
    B44 = "b44"
    B44A = "b44a"
    HTJ2K32 = "htj2k32"
    HTJ2K256 = "htj2k256"
    LZW = "lzw"
    PACKBITS = "packbits"
    LOSSLESS = "lossless"


class Blocks(StrEnum):
    """Scanlines per DWA block, its value the OpenEXR codec."""

    DWAA = "dwaa"
    DWAB = "dwab"


# --- [CONSTANTS] ------------------------------------------------------------------------

interop: Final[Callable[[PyOpenColorIO.ColorSpace], str]] = PyOpenColorIO.ColorSpace.getInteropID  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
interchange: Final[Callable[[PyOpenColorIO.ColorSpace, str], str]] = PyOpenColorIO.ColorSpace.getInterchangeAttribute  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]

# --- [TABLES] ---------------------------------------------------------------------------

_DEPTH_WRITERS: Final = MappingProxyType({
    Depth.UINT8: frozenset({"bmp", "dpx", "fits", "ico", "iff", "jpeg", "jpeg2000", "png", "pnm", "rla", "sgi", "targa", "tiff", "webp"}),
    Depth.UINT16: frozenset({"dpx", "iff", "jpeg2000", "png", "pnm", "rla", "sgi", "tiff"}),
    Depth.HALF: frozenset({"openexr", "tiff"}),
    Depth.FLOAT: frozenset({"dpx", "fits", "hdr", "openexr", "pnm", "rla", "tiff"}),
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
        return (("compression", OpenImageIO.TypeString, self.codec.value),)


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
        return (("compression", OpenImageIO.TypeString, f"{self.blocks.value}:{self.level}"),)


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

    depth: Literal[Depth.HALF, Depth.FLOAT] = Depth.FLOAT
    compression: Option[Compressions] = Nothing

    @property
    def attributes(self) -> tuple[tuple[str, OpenImageIO.TypeDesc, str | int], ...]:
        """Spec attributes of compression, none for writer default."""
        return self.compression.map(lambda compression: compression.attributes).default_value(())

    def clipped[D: np.float32 | np.uint8 | np.uint16](self, plane: npt.NDArray[D]) -> npt.NDArray[D]:
        """Returns plane clipped to the largest half float less compression margin at 16-bit depth, else unchanged."""
        bound = float(np.finfo(np.float16).max) - self.compression.map(lambda compression: compression.margin).default_value(0)
        return np.clip(plane, -bound, bound) if self.depth is Depth.HALF else plane


class Format(msgspec.Struct, frozen=True, kw_only=True):
    """OpenImageIO extension list entry."""

    name: str
    extensions: tuple[str, ...]

    @classmethod
    def readable(cls) -> tuple[Self, ...]:
        """Returns every format a reader exists for in OpenImageIO extension list order."""
        entries = OpenImageIO.get_string_attribute("extension_list").split(";")
        return tuple(cls(name=name, extensions=tuple(suffixes.split(","))) for name, _, suffixes in (entry.partition(":") for entry in entries))

    @classmethod
    def writable(cls) -> tuple[Self, ...]:
        """Returns readable formats the output format list names with a depth."""
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

    @classmethod
    def writer(cls, name: str) -> Option[Self]:
        """Returns the writable format named `name`, Nothing for a format OpenImageIO does not write."""
        return Option.of_optional(next((row for row in cls.writable() if row.name == name), None))

    def stored(self, depth: Depth) -> Outcome[Depth]:
        """Accepts a depth the writer stores unchanged."""
        return Ok(depth).filter_with(self.depths.__contains__, lambda held: Fault.of(RecordReason.OUTSIDE, self.name, held.name, member=", ".join(member.name for member in self.depths)))


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

    def written(self, path: pathlib.Path, storage: Storage, fields: Mapping[str, str], config: PyOpenColorIO.Config) -> Outcome[pathlib.Path]:
        """Writes pixels with header, text fields, storage, and colorspace tags to the writer `path` suffix names.

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
                    spec=OpenImageIO.ImageSpec(width, height, channels, OpenImageIO.TypeDesc(self.pixels.dtype.name) if coded else OpenImageIO.TypeDesc(storage.depth.basetype)),
                    attributes=attributes,
                    pixels=lambda: Ok(self.pixels if coded else storage.clipped(self.pixels)),
                ),
            )

        return profile(config, self.colorspace).bind(lambda icc: emitted(path, lambda output: subimages(output, icc.filter(lambda _: channels >= 3))))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [IO]


def read_file(
    path: pathlib.Path, config: PyOpenColorIO.Config, *, colorspace: Option[str] = Nothing, untagged: Option[tuple[str, str]] = Nothing, options: Option[OpenImageIO.ImageSpec] = Nothing
) -> Outcome[ImageFile[np.float32]]:
    """Reads every channel of first subimage as float32 with its colorspace.

    Args:
        path: File to read.
        config: Configuration resolving the colorspace.
        colorspace: Colorspace bypassing tags and file rules.
        untagged: Integer and float stored colorspaces of a file with no colorspace tag that only the default file rule matches.
        options: Reader configuration.

    Returns:
        File, or `read` or `unresolved` fault.
    """
    name = str(path)

    def decoded(image: OpenImageIO.ImageInput) -> Outcome[ImageFile[np.float32]]:
        spec = image.spec()
        stored_float = spec.format.basetype in {OpenImageIO.HALF, OpenImageIO.FLOAT, OpenImageIO.DOUBLE}
        fallback = untagged.filter(lambda _: config.filepathOnlyMatchesDefaultRule(name)).map(lambda pair: pair[1] if stored_float else pair[0])
        requested = colorspace.or_else(fallback).default_with(lambda: config.getColorSpaceFromFilepath(name)[0])
        tagged = Nothing if colorspace.is_some() else Option.of_optional(config.getColorSpace(spec.get_string_attribute("oiio:ColorSpace")))
        resolved = (
            tagged
            .or_else_with(lambda: Option.of_optional(config.getColorSpace(requested)))
            .map(PyOpenColorIO.ColorSpace.getName)
            .to_result_with(lambda: Fault.of(ImagingReason.UNRESOLVED, config.getName(), member=requested))
        )
        read = Option.of_optional(image.read_image(0, 0, 0, spec.nchannels, OpenImageIO.FLOAT)).to_result_with(lambda: Fault.of(RecordReason.UNREADABLE, name, image.geterror()))
        return paired(resolved, read).map(lambda pair: ImageFile(pixels=pair[1], colorspace=pair[0], header=tuple(spec.extra_attribs)))

    return opened(path, decoded, options=options)


def opened[T](path: pathlib.Path, use: Callable[[OpenImageIO.ImageInput], Outcome[T]], *, options: Option[OpenImageIO.ImageSpec] = Nothing) -> Outcome[T]:
    """Opens a file for reading, uses it, and closes it on every path.

    Returns:
        Reader value, or `read` fault for open, read, or close.
    """

    def held(image: OpenImageIO.ImageInput) -> Outcome[T]:
        with contextlib.closing(image):
            return use(image)

    image = options.map(lambda spec: OpenImageIO.ImageInput.open(str(path), spec)).default_with(lambda: OpenImageIO.ImageInput.open(str(path)))
    return Option.of_optional(image).map(held).default_with(lambda: Error(Fault.of(RecordReason.UNREADABLE, str(path), OpenImageIO.geterror())))


def written(path: pathlib.Path, write: Callable[[pathlib.Path], object]) -> Outcome[pathlib.Path]:
    """Runs one encoder opening, writing, and closing the stage path of `path` through one atomic commit, creating its folder.

    Returns:
        Path, or `write` fault for folder creation, encoding, or commit.
    """
    return committed(path, lambda stage: catch(catch(write, exception=OSError), exception=ValueError)(stage).map_error(functools.partial(Fault.of, RecordReason.UNWRITABLE, str(path))))


def emitted[D: np.float32 | np.uint8 | np.uint16](path: pathlib.Path, subimages: Callable[[OpenImageIO.ImageOutput], tuple[Subimage[D], ...]]) -> Outcome[pathlib.Path]:
    """Encodes subimages built for the writer `path` suffix names through one output, closing it before replacing the file.

    Returns:
        Path, or `write` fault.
    """
    refused = functools.partial(Fault.of, RecordReason.UNWRITABLE, str(path))

    def encode(stage: str) -> Outcome[pathlib.Path]:
        def into(output: OpenImageIO.ImageOutput) -> Outcome[pathlib.Path]:
            def answered(*, done: bool) -> Outcome[pathlib.Path]:
                return Ok(path) if done else Error(refused(output.geterror()))

            def specified(subimage: Subimage[D]) -> OpenImageIO.ImageSpec:
                spec = OpenImageIO.ImageSpec(subimage.spec)
                for name, kind, value in subimage.attributes:
                    spec.attribute(name, kind, value)
                return spec

            def step(written: Outcome[pathlib.Path], indexed: tuple[int, tuple[OpenImageIO.ImageSpec, Subimage[D]]]) -> Outcome[pathlib.Path]:
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

        return Option.of_optional(OpenImageIO.ImageOutput.create(str(path))).map(into).default_with(lambda: Error(refused(OpenImageIO.geterror())))

    return committed(path, lambda stage: encode(str(stage)))


# --- [CODECS]


def generations(rgb: npt.NDArray[np.uint8], *, quality: int, subsampling: Subsampling, count: int) -> Outcome[npt.NDArray[np.uint8]]:
    """Returns contiguous eight-bit RGB after `count` JPEG encode and decode cycles through one temporary file removed on return, at `quality` and `jpeg:subsampling` chroma."""
    attributes = (*Quality(level=quality).attributes, ("jpeg:subsampling", OpenImageIO.TypeString, subsampling))

    @catch(exception=OSError)
    def scoped() -> IO[bytes]:
        return tempfile.NamedTemporaryFile(suffix=".jpg", delete_on_close=False)

    def cycles(file: IO[bytes]) -> Outcome[npt.NDArray[np.uint8]]:
        with file:
            return functools.reduce(lambda held, _: held.bind(functools.partial(cycled, file.name)), range(count), Ok(rgb))

    def cycled(path: str, held: npt.NDArray[np.uint8]) -> Outcome[npt.NDArray[np.uint8]]:
        encoded = OpenImageIO.ImageBuf(held)
        for name, kind, value in attributes:
            encoded.specmod().attribute(name, kind, value)
        return checked(OpenImageIO.ImageBuf(path) if encoded.write(path) else encoded).bind(lambda read: pixels(read, Depth.UINT8))

    return scoped().map_error(functools.partial(Fault.of, RecordReason.UNWRITABLE, tempfile.gettempdir())).bind(cycles)


# --- [COLOR_TAGS]


def profile(config: PyOpenColorIO.Config, colorspace: str) -> Outcome[Option[bytes]]:
    """Reads the ICC profile a colorspace names from config search path.

    Returns:
        Profile, `Nothing` when the colorspace names none, or `read` fault for a named profile the search path misses or fails to read.
    """
    context = config.getCurrentContext()

    def loaded(name: str) -> Outcome[Option[bytes]]:
        refused = functools.partial(Fault.of, RecordReason.UNREADABLE, name)
        return (
            catch(context.resolveFileLocation, exception=PyOpenColorIO.ExceptionMissingFile)(name)
            .map_error(refused)
            .bind(lambda location: catch(pathlib.Path(location).read_bytes, exception=OSError)().map_error(refused))
            .map(Some)
        )

    return Option.of_optional(config.getColorSpace(colorspace)).map(lambda space: interchange(space, "icc_profile_name")).filter(bool).map(loaded).default_value(Ok(Nothing))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Blocks",
    "Codec",
    "Compressions",
    "Deflate",
    "Dwa",
    "Format",
    "ImageFile",
    "Plain",
    "Quality",
    "Storage",
    "Subimage",
    "Subsampling",
    "emitted",
    "generations",
    "interchange",
    "interop",
    "opened",
    "profile",
    "read_file",
    "written",
]
