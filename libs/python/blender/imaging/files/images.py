"""Image files, formats, storage settings, JPEG generation loss, and atomic writes."""

from collections.abc import Callable, Mapping
import contextlib
from enum import StrEnum
import functools
import pathlib
from types import MappingProxyType
from typing import Annotated, Final, get_args, Literal, Self

from expression import Option, Some
from expression.extra.result import catch
import imagecodecs
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from blender.imaging.files.buffers import checked, Coded, Coding, Depth, pixels, unassociated
from blender.records.faults import Error, Fault, Nothing, Ok, Outcome, paired, Reason
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

interop: Final[Callable[[PyOpenColorIO.ColorSpace], str]] = PyOpenColorIO.ColorSpace.getInteropID  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute, unsound-assignment]
_interchange: Final[Callable[[PyOpenColorIO.ColorSpace, str], str]] = PyOpenColorIO.ColorSpace.getInterchangeAttribute  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute, unsound-assignment]
UNTAGGED: Final = ("default_byte", "default_float")

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

    margin = 0
    codec: Codec = Codec.NONE

    @property
    def writers(self) -> frozenset[str]:
        """Writers the codec names."""
        return _CODEC_WRITERS[self.codec]

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


type Compression = Plain | Deflate | Dwa | Quality


class Storage(msgspec.Struct, frozen=True, kw_only=True):
    """Stored depth, compression, and the coding of integer depths."""

    depth: Depth = Depth.FLOAT
    compression: Option[Compression] = Nothing
    coding: Coding = Coding.ROUNDED

    @property
    def attributes(self) -> tuple[tuple[str, OpenImageIO.TypeDesc, str | int], ...]:
        """Spec attributes of compression, none for writer default."""
        return self.compression.map(lambda compression: compression.attributes).default_value(())

    def stored(self, plane: npt.NDArray[np.float32], /) -> Outcome[npt.NDArray[Coded]]:
        """Returns associated float values as stored: integer depths coded from unassociated values, half floats clipped to the largest half less compression margin, or `FAILED` fault."""
        match self.depth:
            case Depth.UINT8 | Depth.UINT16:
                return self.depth.coded(unassociated(plane), self.coding)
            case Depth.HALF:
                bound = float(np.finfo(np.float16).max) - self.compression.map(lambda compression: compression.margin).default_value(0)
                return Ok(np.clip(plane, -bound, bound))
            case Depth.FLOAT:
                return Ok(plane)


class Format(msgspec.Struct, frozen=True, kw_only=True):
    """OpenImageIO extension list entry."""

    name: str
    extensions: tuple[str, *tuple[str, ...]]

    @classmethod
    def readable(cls) -> tuple[Self, ...]:
        """Returns every format a reader exists for in OpenImageIO extension list order."""
        entries = (entry.partition(":") for entry in OpenImageIO.get_string_attribute("extension_list").split(";"))
        return tuple(cls(name=name, extensions=(first, *rest)) for name, _, suffixes in entries for first, *rest in (suffixes.split(","),))

    @classmethod
    def writable(cls) -> tuple[Self, ...]:
        """Returns readable formats the output format list names with a depth."""
        writers = frozenset(OpenImageIO.get_string_attribute("output_format_list").split(","))
        return tuple(row for row in cls.readable() if row.name in writers and row.depths)

    @classmethod
    def writer(cls, name: str) -> Option[Self]:
        """Returns the writable format named `name`, Nothing for a format OpenImageIO does not write."""
        return Option.of_optional(next((row for row in cls.writable() if row.name == name), None))

    @classmethod
    def of(cls, path: pathlib.Path) -> Outcome[Self]:
        """Returns the writable format OpenImageIO picks for `path`, `OUTSIDE` fault naming every writable suffix and the OpenImageIO refusal."""
        return (
            Option
            .of_optional(OpenImageIO.ImageOutput.create(str(path)))
            .bind(lambda output: cls.writer(output.format_name()))
            .to_result_with(lambda: Fault.of(Reason.OUTSIDE, str(path), OpenImageIO.geterror() or path.suffix, member=", ".join(suffix for row in cls.writable() for suffix in row.suffixes)))
        )

    @property
    def suffixes(self) -> tuple[str, *tuple[str, ...]]:
        """Dotted extensions in OpenImageIO order."""
        first, *rest = self.extensions
        return (f".{first}", *(f".{extension}" for extension in rest))

    @property
    def depths(self) -> tuple[Depth, ...]:
        """Depths format writer stores unchanged."""
        return tuple(depth for depth in Depth if self.name in _DEPTH_WRITERS[depth])

    @property
    def compressions(self) -> tuple[Compression, ...]:
        """Compressions the writer accepts: each plain codec, and every other case at its defaults."""
        cases = (*(Plain(codec=codec) for codec in Codec), *(kind() for kind in get_args(Compression.__value__) if kind is not Plain))
        return tuple(form for form in cases if self.name in form.writers)  # ty: ignore[unsound-return-statement]

    def stored(self, depth: Depth) -> Outcome[Depth]:
        """Accepts a depth the writer stores unchanged."""
        return Ok(depth).filter_with(self.depths.__contains__, lambda held: Fault.of(Reason.OUTSIDE, self.name, held.name, member=", ".join(member.name for member in self.depths)))


class Subimage(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Written subimage with pixels on demand."""

    spec: OpenImageIO.ImageSpec
    attributes: tuple[tuple[str, OpenImageIO.TypeDesc, object], ...]
    pixels: Callable[[], Outcome[npt.NDArray[Coded]]]


class ImageFile(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Associated float pixels, colorspace, and native source header of an image file."""

    pixels: npt.NDArray[np.float32]
    colorspace: str
    header: OpenImageIO.ImageSpec = msgspec.field(default_factory=OpenImageIO.ImageSpec)

    @property
    def aspect(self) -> float:
        """Displayed pixel width over height under the written pixel aspect."""
        return aspect(self.pixels.shape[-2], self.pixels.shape[-3], self.header)

    def fitted(self, size: tuple[int, int], /) -> Outcome[Self]:
        """Shrinks the pixel canvas within width and height, recording native fit windows and retaining source metadata."""
        if self.pixels.shape[1] <= size[0] and self.pixels.shape[0] <= size[1]:
            return Ok(self)
        source = OpenImageIO.ImageBuf(self.pixels)
        source.specmod().extra_attribs.merge(self.header.extra_attribs)
        return checked(OpenImageIO.ImageBufAlgo.fit(source, roi=OpenImageIO.ROI(0, size[0], 0, size[1]))).bind(
            lambda image: pixels(image).map(lambda values: msgspec.structs.replace(self, pixels=values, header=image.spec()))
        )

    def written(self, path: pathlib.Path, storage: Storage, fields: Mapping[str, str], config: PyOpenColorIO.Config) -> Outcome[pathlib.Path]:
        """Writes pixels at the storage depth, integer depths coded through its coding, with header, text fields, compression, and colorspace tags to the writer `path` suffix names.

        Returns:
            Path, `OUTSIDE` fault for a path no writer takes or a depth its writer changes, `UNREADABLE` fault for profile IO, `FAILED` fault for coding, or `UNWRITABLE` fault.
        """
        (height, width, channels), coded = self.pixels.shape, storage.depth in {Depth.UINT8, Depth.UINT16}
        identifier = Option.of_optional(config.getColorSpace(self.colorspace)).map(interop).filter(bool)
        bt709_transfer = 1

        def subimages(icc: Option[bytes], output: OpenImageIO.ImageOutput) -> tuple[Subimage, ...]:
            tagged = bool(output.supports("cicp"))
            dropped = {"oiio", "iccprofile", "cicp", "colorinteropid", "chromaticities"} | ({row.name for row in Format.readable()} - {output.format_name()})
            attributes = (
                *((held.name, held.type, held.value) for held in self.header.extra_attribs if held.name.partition(":")[0].lower() not in dropped),
                *((name, OpenImageIO.TypeString, text) for name, text in fields.items()),
                *storage.attributes,
                ("oiio:UnassociatedAlpha", OpenImageIO.TypeInt, int(coded)),
                ("tiff:half", OpenImageIO.TypeInt, 1),
                *(("ICCProfile", OpenImageIO.TypeDesc(f"uint8[{len(data)}]"), np.frombuffer(data, np.uint8)) for data in icc.to_list() if channels >= 3),
                *(
                    ("CICP", OpenImageIO.TypeDesc("int[4]"), (code[0], code[1], 0, 1))
                    for code in identifier
                    .bind(lambda name: Option.of_optional(OpenImageIO.ColorConfig("ocio://default").get_cicp(name)))  # type: ignore[no-untyped-call]
                    .filter(lambda code: tagged and code[1] != bt709_transfer)
                    .to_list()
                ),
                *(("colorInteropID", OpenImageIO.TypeString, name) for name in identifier.filter(lambda _: not tagged).to_list()),
            )
            return (Subimage(spec=OpenImageIO.ImageSpec(width, height, channels, OpenImageIO.TypeDesc(storage.depth.basetype)), attributes=attributes, pixels=lambda: storage.stored(self.pixels)),)

        return paired(Format.of(path).bind(lambda row: row.stored(storage.depth)), profile(config, self.colorspace)).bind(lambda found: emitted(path, functools.partial(subimages, found[1])))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [GEOMETRY]


def aspect(width: int, height: int, header: OpenImageIO.ImageSpec) -> float:
    """Returns displayed width over height under the header's pixel aspect, square pixels when absent."""
    return width * header.get_float_attribute("PixelAspectRatio", 1.0) / height


# --- [IO]


def read_file(
    path: pathlib.Path,
    config: PyOpenColorIO.Config,
    *,
    colorspace: Option[str] = Nothing,
    options: Option[OpenImageIO.ImageSpec] = Nothing,
    subimage: int = 0,
    channels: Option[tuple[int, ...]] = Nothing,
) -> Outcome[ImageFile]:
    """Reads selected channels of a subimage as float32 with its colorspace.

    An untagged file only the default file rule matches reads in the `UNTAGGED` role of its stored type, where the config holds it.

    Args:
        path: File to read.
        config: Configuration resolving the colorspace.
        colorspace: Colorspace bypassing tags and file rules.
        options: Reader configuration.
        subimage: Native part index.
        channels: Ordered channel indices, every channel when absent.

    Returns:
        File, or `UNREADABLE` or `ABSENT` fault.
    """
    name = str(path)

    def decoded(image: OpenImageIO.ImageInput) -> Outcome[ImageFile]:
        spec = image.spec(subimage, 0)
        byte, real = UNTAGGED
        role = real if spec.format.basetype in {OpenImageIO.HALF, OpenImageIO.FLOAT, OpenImageIO.DOUBLE} else byte
        fallback = Some(role).filter(lambda held: config.hasRole(held) and config.filepathOnlyMatchesDefaultRule(name))
        requested = colorspace.or_else(fallback).default_with(lambda: config.getColorSpaceFromFilepath(name)[0])
        tagged = Nothing if colorspace.is_some() else Option.of_optional(config.getColorSpace(spec.get_string_attribute("oiio:ColorSpace")))
        resolved = (
            tagged
            .or_else_with(lambda: Option.of_optional(config.getColorSpace(requested)))
            .map(PyOpenColorIO.ColorSpace.getName)
            .to_result_with(lambda: Fault.of(Reason.ABSENT, config.getName(), member=requested))
        )
        return paired(resolved, read_pixels(path, image, spec, subimage=subimage, channels=channels)).map(lambda pair: ImageFile(pixels=pair[1], colorspace=pair[0], header=spec))

    return opened(path, decoded, options=options)


def read_pixels(path: pathlib.Path, image: OpenImageIO.ImageInput, spec: OpenImageIO.ImageSpec, *, subimage: int = 0, channels: Option[tuple[int, ...]] = Nothing) -> Outcome[npt.NDArray[np.float32]]:
    """Returns selected float32 channels through one span read, every channel when absent, or an `UNREADABLE` fault."""
    if channels.to_optional() == ():
        return Ok(np.empty((*((spec.depth,) if spec.depth > 1 else ()), spec.height, spec.width, 0), dtype=np.float32))
    first, last = channels.map(lambda selected: (min(selected), max(selected) + 1)).default_value((0, spec.nchannels))
    return (
        Option
        .of_optional(image.read_image(subimage, 0, first, last, OpenImageIO.FLOAT))
        .map(lambda span: channels.map(lambda selected: np.take(span, tuple(index - first for index in selected), axis=-1)).default_value(span))
        .to_result_with(lambda: Fault.of(Reason.UNREADABLE, str(path), image.geterror()))
    )


def read_codes(path: pathlib.Path) -> Outcome[Option[npt.NDArray[np.uint8 | np.uint16]]]:
    """Reads native uniform unsigned eight-bit or sixteen-bit codes, preserving stored alpha association, absent for other channel storage."""
    options = OpenImageIO.ImageSpec()
    options.attribute("oiio:UnassociatedAlpha", 1)

    def decoded(image: OpenImageIO.ImageInput) -> Outcome[Option[npt.NDArray[np.uint8 | np.uint16]]]:
        spec = image.spec()
        formats = spec.get_channelformats()
        match formats:
            case (kind, *_) if kind.basetype in {OpenImageIO.UINT8, OpenImageIO.UINT16} and OpenImageIO.TypeDesc.all_types_equal(formats):  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                codes: npt.NDArray[np.uint8 | np.uint16] | None = image.read_image(0, 0, 0, spec.nchannels, kind)
                return Option.of_optional(codes).to_result_with(lambda: Fault.of(Reason.UNREADABLE, str(path), image.geterror())).map(Some)
            case _:
                return Ok(Nothing)

    return opened(path, decoded, options=Some(options))


def opened[T](path: pathlib.Path, use: Callable[[OpenImageIO.ImageInput], Outcome[T]], *, options: Option[OpenImageIO.ImageSpec] = Nothing) -> Outcome[T]:
    """Opens a file for reading under reader configuration `options`, uses it, and closes it on every path.

    Returns:
        Reader value, or `UNREADABLE` fault for open, read, or close.
    """

    def held(image: OpenImageIO.ImageInput) -> Outcome[T]:
        with contextlib.closing(image):
            return use(image)

    return (
        Option
        .of_optional(OpenImageIO.ImageInput.open(str(path), options.default_with(OpenImageIO.ImageSpec)))
        .map(held)
        .default_with(lambda: Error(Fault.of(Reason.UNREADABLE, str(path), OpenImageIO.geterror())))
    )


def written(path: pathlib.Path, write: Callable[[pathlib.Path], object]) -> Outcome[pathlib.Path]:
    """Runs one encoder opening, writing, and closing the stage path of `path` through one atomic commit, creating its folder.

    Returns:
        Path, or `UNWRITABLE` fault for folder creation, encoding, or commit.
    """
    return committed(path, lambda stage: catch(catch(write, exception=OSError), exception=ValueError)(stage).map_error(functools.partial(Fault.of, Reason.UNWRITABLE, str(path))))


def emitted(path: pathlib.Path, subimages: Callable[[OpenImageIO.ImageOutput], tuple[Subimage, ...]]) -> Outcome[pathlib.Path]:
    """Encodes subimages built for the writer `path` suffix names through one output, closing it before replacing the file.

    Returns:
        Path, or `UNWRITABLE` fault.
    """
    refused = functools.partial(Fault.of, Reason.UNWRITABLE, str(path))

    def into(stage: str, output: OpenImageIO.ImageOutput) -> Outcome[pathlib.Path]:
        def answered(*, done: bool) -> Outcome[pathlib.Path]:
            return Ok(path) if done else Error(refused(output.geterror()))

        def specified(subimage: Subimage) -> OpenImageIO.ImageSpec:
            spec = OpenImageIO.ImageSpec(subimage.spec)
            for name, kind, value in subimage.attributes:
                spec.attribute(name, kind, value)
            return spec

        def step(written: Outcome[pathlib.Path], indexed: tuple[int, tuple[OpenImageIO.ImageSpec, Subimage]]) -> Outcome[pathlib.Path]:
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

    return committed(
        path, lambda stage: Option.of_optional(OpenImageIO.ImageOutput.create(str(path))).map(functools.partial(into, str(stage))).default_with(lambda: Error(refused(OpenImageIO.geterror())))
    )


# --- [CODECS]


def generations(rgb: npt.NDArray[np.uint8], *, quality: int, subsampling: Subsampling, count: int) -> Outcome[npt.NDArray[np.uint8]]:
    """Returns eight-bit RGB after `count` in-memory JPEG encode and decode cycles at `quality` and chroma subsampling, or a `FAILED` fault."""
    sampling = subsampling.replace(":", "")
    try:
        return Ok(functools.reduce(lambda held, _: imagecodecs.jpeg8_decode(imagecodecs.jpeg8_encode(held, level=quality, subsampling=sampling)), range(count), rgb))
    except (imagecodecs.Jpeg8Error, ValueError) as error:
        return Error(Fault.of(Reason.FAILED, generations.__name__, error))


# --- [COLOR_TAGS]


def profile(config: PyOpenColorIO.Config, colorspace: str) -> Outcome[Option[bytes]]:
    """Reads the ICC profile a colorspace names from config search path.

    Returns:
        Profile, `Nothing` when the colorspace names none, or `UNREADABLE` fault for a named profile the search path misses or fails to read.
    """
    context = config.getCurrentContext()

    def loaded(name: str) -> Outcome[Option[bytes]]:
        refused = functools.partial(Fault.of, Reason.UNREADABLE, name)
        return (
            catch(context.resolveFileLocation, exception=PyOpenColorIO.ExceptionMissingFile)(name)
            .map_error(refused)
            .bind(lambda location: catch(pathlib.Path(location).read_bytes, exception=OSError)().map_error(refused))
            .map(Some)
        )

    return Option.of_optional(config.getColorSpace(colorspace)).map(lambda space: _interchange(space, "icc_profile_name")).filter(bool).map(loaded).default_value(Ok(Nothing))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "UNTAGGED",
    "Blocks",
    "Codec",
    "Compression",
    "Deflate",
    "Dwa",
    "Format",
    "ImageFile",
    "Plain",
    "Quality",
    "Storage",
    "Subimage",
    "Subsampling",
    "aspect",
    "emitted",
    "generations",
    "interop",
    "opened",
    "profile",
    "read_codes",
    "read_file",
    "read_pixels",
    "written",
]
