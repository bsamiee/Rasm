"""Image files, storage settings, integer coding, buffer operations, and atomic writes."""

from collections.abc import Callable, Mapping
import contextlib
from enum import IntEnum
import functools
import pathlib
import struct
from types import MappingProxyType
import typing
from typing import Annotated, Final, IO, Literal, overload, Self

from expression import Error, Nothing, Ok, Option, Some
from expression.extra.result import catch
import imagecodecs
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from blender.imaging.faults import ImagingReason
from blender.records.faults import Fault, Faults, Outcome, paired, RecordReason
from blender.records.fields import Case
from blender.records.stores import committed

# --- [TYPES] ----------------------------------------------------------------------------

type IntegerDepth = Literal[8, 16]


class Depth(IntEnum):
    """Stored channel type, its lowercased name an OpenImageIO type name."""

    UINT8 = 1
    UINT16 = 2
    HALF = 3
    FLOAT = 4

    def coded(self, plane: npt.NDArray[np.float32], coding: "Coding") -> npt.NDArray[np.uint8 | np.uint16 | np.float32]:
        """Returns straight display values stored at depth, integer depths coded through `coding`."""
        match self:
            case Depth.UINT8:
                return coding.coded(plane, 8)
            case Depth.UINT16:
                return coding.coded(plane, 16)
            case Depth.HALF | Depth.FLOAT:
                return plane


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

DEPTH_WRITERS: Final = MappingProxyType({
    Depth.UINT8: frozenset({"bmp", "dpx", "fits", "ico", "iff", "jpeg", "jpeg2000", "jpegxl", "png", "pnm", "rla", "sgi", "targa", "tiff", "webp"}),
    Depth.UINT16: frozenset({"dpx", "iff", "jpeg2000", "jpegxl", "png", "pnm", "rla", "sgi", "tiff"}),
    Depth.HALF: frozenset({"jpegxl", "openexr", "tiff"}),
    Depth.FLOAT: frozenset({"dpx", "fits", "hdr", "jpegxl", "openexr", "pnm", "rla", "tiff"}),
})
CODEC_WRITERS: Final = MappingProxyType({
    Codec.NONE: frozenset({"openexr", "tiff"}),
    **dict.fromkeys((Codec.RLE, Codec.ZIPS, Codec.PIZ, Codec.PXR24, Codec.B44, Codec.B44A, Codec.HTJ2K32, Codec.HTJ2K256), frozenset({"openexr"})),
    **dict.fromkeys((Codec.LZW, Codec.PACKBITS), frozenset({"tiff"})),
    Codec.LOSSLESS: frozenset({"webp"}),
})

# --- [MODELS] ---------------------------------------------------------------------------


class Plain(Case, frozen=True, kw_only=True):
    """Codec without level."""

    writers = frozenset[str]().union(*CODEC_WRITERS.values())
    margin = 0
    codec: Annotated[Codec, msgspec.Meta(description="Codec the writer applies")] = Codec.NONE

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
    level: Annotated[int, msgspec.Meta(ge=1, le=9, description="Deflate effort from fastest to smallest")] = 6

    @property
    def attributes(self) -> tuple[tuple[str, OpenImageIO.TypeDesc, str | int], ...]:
        """Spec attributes the level sets."""
        return (("compression", OpenImageIO.TypeString, f"zip:{self.level}"), ("png:compressionLevel", OpenImageIO.TypeInt, self.level))


class Dwa(Case, frozen=True, kw_only=True):
    """Lossy DCT compression of OpenEXR color channels."""

    writers = frozenset({"openexr"})
    lossy = True
    blocks: Annotated[Blocks, msgspec.Meta(description="Scanlines per compressed block")] = Blocks.DWAA
    level: Annotated[int, msgspec.Meta(ge=0, le=450, description="Quantization strength, larger levels smaller and lossier")] = 45

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
    level: Annotated[int, msgspec.Meta(ge=0, le=100, description="Encoder quality from smallest to best")] = 90

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
        return tuple(depth for depth in Depth if self.name in DEPTH_WRITERS[depth])

    @property
    def compressions(self) -> tuple[type[Compressions], ...]:
        """Compression cases writer accepts."""
        return tuple(form for form in typing.get_args(Compressions.__value__) if self.name in form.writers)


class Rounded(Case, frozen=True, kw_only=True):
    """Integer codes rounding each straight display value half up."""

    @overload
    @staticmethod
    def coded(plane: npt.NDArray[np.float32], depth: Literal[8]) -> npt.NDArray[np.uint8]: ...
    @overload
    @staticmethod
    def coded(plane: npt.NDArray[np.float32], depth: Literal[16]) -> npt.NDArray[np.uint16]: ...
    @staticmethod
    def coded(plane: npt.NDArray[np.float32], depth: IntegerDepth) -> npt.NDArray[np.uint8 | np.uint16]:
        """Returns clamped integer codes of the same shape."""
        dtype = np.uint8 if depth == 8 else np.uint16
        limits = np.iinfo(dtype)
        scaled = np.floor(plane * limits.max + 0.5)
        return np.clip(scaled, 0, limits.max, out=scaled).astype(dtype)


class Diffused(Case, frozen=True, kw_only=True):
    """Floyd-Steinberg diffusion of separate color errors in raster order, alpha rounded."""

    @staticmethod
    def coded(plane: npt.NDArray[np.float32], depth: IntegerDepth) -> npt.NDArray[np.uint8 | np.uint16]:
        """Returns codes of a straight BW, RGB, or RGBA frame, each color error spread over unvisited neighbors along anti-diagonal wavefronts."""
        height, width, _ = plane.shape
        coded = Rounded.coded(plane, 8) if depth == 8 else Rounded.coded(plane, 16)
        levels = float(np.iinfo(coded.dtype).max)
        color = levels * plane[..., :3]
        ring = np.zeros((height + 1, 4, color.shape[2]), np.float32)
        for step in range(width + 2 * height - 2):
            low, high = max(0, (step - width + 2) // 2), min(height - 1, step // 2)
            rows, ahead = np.arange(low, high + 1), min(high, (step - 1) // 2) + 1
            columns = step - 2 * rows
            visited = np.clip(color[rows, columns] + ring[low : high + 1, step % 4], 0.0, levels)
            rounded = np.floor(visited + 0.5)
            coded[rows, columns, :3], error = rounded, visited - rounded
            ring[low : high + 1, step % 4] = 0.0
            ring[low + 1 : ahead + 1, (step + 1) % 4] += error[: ahead - low] * (3 / 16)
            ring[low + 1 : high + 2, (step + 2) % 4] += error * (5 / 16)
            ring[low + 1 : high + 2, (step + 3) % 4] += error * (1 / 16)
            ring[low : high + 1, (step + 1) % 4] += error * (7 / 16)
        return coded


type Coding = Rounded | Diffused


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
        interop = (
            Option
            .of_optional(config.getColorSpace(self.colorspace))
            .map(PyOpenColorIO.ColorSpace.getInteropID)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            .filter(bool)
        )
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
                    for code in interop
                    .bind(lambda name: Option.of_optional(OpenImageIO.ColorConfig("ocio://default").get_cicp(name)))
                    .filter(lambda code: tagged and code[1] != bt709_transfer)
                    .to_list()
                ),
                *(("colorInteropID", OpenImageIO.TypeString, name) for name in interop.filter(lambda _: not tagged).to_list()),
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

# --- [BUFFERS]


def checked(image: OpenImageIO.ImageBuf) -> Outcome[OpenImageIO.ImageBuf]:
    """Returns buffer, or image fault when buffer holds an error."""
    return Error((Fault(reason=ImagingReason.BUFFER, subject=image.name, cause=image.geterror()),)) if image.has_error else Ok(image)


def located(values: npt.NDArray[np.float32 | np.uint8], like: OpenImageIO.ImageBuf) -> Outcome[OpenImageIO.ImageBuf]:
    """Returns values as buffer with metadata, origin, and full window of `like`."""
    image = OpenImageIO.ImageBuf(values)
    image.copy_metadata(like)
    image.set_origin(like.xbegin, like.ybegin, like.zbegin)
    image.roi_full = like.roi_full
    return checked(image)


def pixels(image: OpenImageIO.ImageBuf) -> Outcome[npt.NDArray[np.float32]]:
    """Returns float32 pixels of buffer, or image fault."""
    return Option.of_optional(image.get_pixels(OpenImageIO.FLOAT)).to_result_with(lambda: (Fault(reason=ImagingReason.BUFFER, subject=image.name, cause=image.geterror()),))


def resampled(values: npt.NDArray[np.float32], size: tuple[int, int]) -> Outcome[npt.NDArray[np.float32]]:
    """Returns values fitted exactly into `size` width and height, letterboxed at their aspect, for linear-light planes."""
    return (
        checked(OpenImageIO.ImageBuf(values))
        .map(lambda source: OpenImageIO.ImageBufAlgo.fit(source, filtername="", filterwidth=0.0, fillmode="letterbox", exact=True, roi=OpenImageIO.ROI(0, size[0], 0, size[1])))
        .bind(checked)
        .bind(pixels)
    )


def fitted(values: npt.NDArray[np.float32], side: int) -> Outcome[npt.NDArray[np.float32]]:
    """Returns values shrunk so their longest side fits `side`, unchanged within it."""
    height, width, channels = values.shape
    longest = max(width, height)
    size = tuple(-(-extent * side // longest) for extent in (width, height))
    return checked(OpenImageIO.ImageBufAlgo.resize(OpenImageIO.ImageBuf(values), roi=OpenImageIO.ROI(0, size[0], 0, size[1], 0, 1, 0, channels))).bind(pixels) if longest > side else Ok(values)


def generations(rgb: npt.NDArray[np.uint8], *, quality: int, subsampling: tuple[int, int], count: int) -> Outcome[npt.NDArray[np.uint8]]:
    """Returns contiguous eight-bit RGB after `count` JPEG encode and decode cycles at IJG `quality` and chroma block `subsampling`."""

    def cycled(held: npt.NDArray[np.uint8], _: int) -> npt.NDArray[np.uint8]:
        coded = imagecodecs.jpeg8_encode(held, level=quality, colorspace=imagecodecs.JPEG8.CS.RGB, outcolorspace=imagecodecs.JPEG8.CS.YCbCr, subsampling=subsampling)
        return np.asarray(imagecodecs.jpeg8_decode(coded, outcolorspace=imagecodecs.JPEG8.CS.RGB), dtype=np.uint8)

    return catch(lambda: functools.reduce(cycled, range(count), rgb), exception=imagecodecs.Jpeg8Error)().map_error(
        lambda error: (Fault(reason=ImagingReason.CODEC, subject=str(rgb.shape), cause=error),)
    )


def unassociated(values: npt.NDArray[np.float32]) -> npt.NDArray[np.float32]:
    """Returns contiguous straight copy of RGB or RGBA, color kept at zero alpha."""
    if (straight := (values.copy())).shape[-1] == 4:
        alpha = straight[..., 3:]
        np.divide(straight[..., :3], alpha, out=straight[..., :3], where=alpha != 0.0)
    return straight


def applied(operation: PyOpenColorIO.CPUProcessor | Callable[[npt.NDArray[np.float32]], npt.NDArray[np.float32]], values: npt.NDArray[np.float32]) -> npt.NDArray[np.float32]:
    """Applies OCIO processor or straight-RGB operation to associated colors."""
    formed = unassociated(values)
    if isinstance(operation, PyOpenColorIO.CPUProcessor):
        if formed.shape[-1] == 4:
            operation.applyRGBA(formed)
        else:
            operation.applyRGB(formed)
    else:
        formed[..., :3] = operation(formed[..., :3])
    if formed.shape[-1] == 4:
        alpha = formed[..., 3:]
        np.multiply(formed[..., :3], alpha, out=formed[..., :3], where=alpha != 0.0)
    return formed


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


def written(path: str, write: Callable[[IO[bytes]], None]) -> Outcome[str]:
    """Streams one whole-file encoder into `path` through one atomic commit, creating its folder.

    Args:
        path: File to replace.
        write: Encoder of whole file.

    Returns:
        Path, or `write` fault for folder creation, encoding, or commit.
    """

    def streamed(stage: pathlib.Path) -> Outcome[None]:
        try:
            with stage.open("wb") as output:
                write(output)
        except (OSError, ValueError, OverflowError, struct.error) as error:
            return Error((Fault(reason=RecordReason.UNWRITABLE, subject=path, cause=error),))
        return Ok(None)

    return committed(pathlib.Path(path), streamed).map(str)


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
                for name, kind, value in subimage.attributes:
                    subimage.spec.attribute(name, kind, value)
                return subimage.spec

            def step(written: Outcome[str], indexed: tuple[int, Subimage[D]]) -> Outcome[str]:
                index, subimage = indexed
                return (
                    written
                    .bind(lambda _: answered(done=index == 0 or output.open(stage, subimage.spec, "AppendSubimage")))
                    .bind(lambda _: subimage.pixels())
                    .bind(lambda array: answered(done=output.write_image(array)))
                )

            with contextlib.closing(output):
                parts = subimages(output)
                return functools.reduce(step, enumerate(parts), answered(done=output.open(stage, [specified(part) for part in parts]))).bind(lambda _: answered(done=output.close()))

        return Option.of_optional(OpenImageIO.ImageOutput.create(path)).map(into).default_with(lambda: Error(refused(OpenImageIO.geterror())))

    return committed(pathlib.Path(path), lambda stage: encode(str(stage))).map(str)


# --- [COLOR_TAGS]


def profile(config: PyOpenColorIO.Config, colorspace: str) -> Outcome[Option[bytes]]:
    """Reads colorspace ICC profile from config search path.

    Args:
        config: Config the colorspace resolves in.
        colorspace: Colorspace name.

    Returns:
        Profile of first resolved name, `Nothing` when none resolves, or `read` fault.
    """
    context = config.getCurrentContext()

    def names(space: PyOpenColorIO.ColorSpace) -> tuple[str, ...]:
        interop = space.getInteropID().rpartition(":")[2]  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        scene = (f"{interop.removesuffix('_scene')}_display.icc",) if interop.endswith("_scene") else ()
        return tuple(
            name
            for name in (
                space.getInterchangeAttribute("icc_profile_name"),  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                *((f"{interop}.icc", *scene) if interop else ()),
            )
            if name
        )

    return (
        Option
        .of_optional(config.getColorSpace(colorspace))
        .map(names)
        .bind(
            lambda found: next(
                (Some(location) for name in found for location in catch(functools.partial(context.resolveFileLocation, name), exception=PyOpenColorIO.ExceptionMissingFile)().to_option().to_list()),
                Nothing,
            )
        )
        .map(
            lambda location: (
                catch(pathlib.Path(location).read_bytes, exception=OSError)().map_error(lambda error: (Fault(reason=RecordReason.UNREADABLE, subject=str(location), cause=error),)).map(Some)
            )
        )
        .default_value(Ok(Nothing))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "CODEC_WRITERS",
    "DEPTH_WRITERS",
    "Blocks",
    "Codec",
    "Coding",
    "Compressions",
    "Deflate",
    "Depth",
    "Diffused",
    "Dwa",
    "Format",
    "ImageFile",
    "IntegerDepth",
    "Plain",
    "Quality",
    "Rounded",
    "Storage",
    "Subimage",
    "applied",
    "checked",
    "emitted",
    "fitted",
    "generations",
    "located",
    "opened",
    "pixels",
    "profile",
    "read_file",
    "resampled",
    "unassociated",
    "written",
]
