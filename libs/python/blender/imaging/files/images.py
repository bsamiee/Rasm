"""Image files, storage settings, and atomic publication."""

from collections.abc import Callable, Mapping
import functools
import pathlib
import struct
import tempfile
import typing
from typing import Annotated, BinaryIO, Literal, Self

from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from blender.imaging.effects.quantization import codes
from blender.imaging.faults import Faults, ImagingFaults, ImagingReadFault, ImagingResult, ImagingUnresolvedFault, ImagingWriteFault, paired, scoped
from blender.imaging.parameters import Case, Choice

# --- [TYPES] ----------------------------------------------------------------------------


class Writable(Choice):
    """Choice naming writers that store each member."""

    writers: frozenset[str]

    def __init__(self, value: int, writers: tuple[str, ...], label: str | None = None) -> None:
        """Sets member writers and label."""
        super().__init__(value, label)
        self.writers = frozenset(writers)


class Depth(Writable):
    """Stored channel type, its lowercased name an OpenImageIO type name."""

    UINT8 = 1, ("bmp", "dpx", "fits", "ico", "iff", "jpeg", "jpeg2000", "png", "pnm", "rla", "sgi", "targa", "tiff", "webp"), "8-bit Integer"
    UINT16 = 2, ("dpx", "iff", "jpeg2000", "png", "pnm", "rla", "sgi", "tiff"), "16-bit Integer"
    HALF = 3, ("openexr", "tiff"), "16-bit Float"
    FLOAT = 4, ("dpx", "fits", "hdr", "openexr", "pnm", "rla", "tiff"), "32-bit Float"

    def coded(self, plane: npt.NDArray[np.float32]) -> npt.NDArray[np.uint8 | np.uint16 | np.float32]:
        """Returns display values coded at depth."""
        match self:
            case Depth.UINT8:
                return codes(plane, 8)
            case Depth.UINT16:
                return codes(plane, 16)
            case Depth.HALF | Depth.FLOAT:
                return plane


class Codec(Writable):
    """Codec without level."""

    NONE = 1, ("openexr", "tiff")
    RLE = 2, ("openexr",), "RLE"
    ZIPS = 3, ("openexr",), "ZIPS"
    PIZ = 4, ("openexr",), "PIZ"
    PXR24 = 5, ("openexr",)
    B44 = 6, ("openexr",)
    B44A = 7, ("openexr",)
    HTJ2K32 = 8, ("openexr",), "HTJ2K 32 Scanlines"
    HTJ2K256 = 9, ("openexr",), "HTJ2K 256 Scanlines"
    LZW = 10, ("tiff",), "LZW"
    PACKBITS = 11, ("tiff",), "PackBits"
    LOSSLESS = 12, ("webp",)


class Blocks(Choice):
    """Scanlines per DWA block, its lowercased name the OpenEXR codec."""

    DWAA = 1, "32 Scanlines"
    DWAB = 2, "256 Scanlines"


# --- [MODELS] ---------------------------------------------------------------------------


class Plain(Case, frozen=True, kw_only=True, tag=1):
    """Codec without level."""

    writers = frozenset[str]().union(*(codec.writers for codec in Codec))
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


class Deflate(Case, frozen=True, kw_only=True, tag=2):
    """Lossless zlib deflate at a level."""

    writers = frozenset({"openexr", "png", "tiff"})
    lossy = False
    margin = 0
    level: Annotated[int, msgspec.Meta(ge=1, le=9, description="Deflate effort from fastest to smallest")] = 6

    @property
    def attributes(self) -> tuple[tuple[str, OpenImageIO.TypeDesc, str | int], ...]:
        """Spec attributes the level sets."""
        return (("compression", OpenImageIO.TypeString, f"zip:{self.level}"), ("png:compressionLevel", OpenImageIO.TypeInt, self.level))


class Dwa(Case, frozen=True, kw_only=True, tag=3):
    """Lossy DCT compression of OpenEXR color channels."""

    title = "DWA"
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


class Quality(Case, frozen=True, kw_only=True, tag=4):
    """Lossy compression at an encoder quality."""

    writers = frozenset({"jpeg", "webp"})
    lossy = True
    margin = 0
    level: Annotated[int, msgspec.Meta(ge=0, le=100, title="Quality", description="Encoder quality from smallest to best")] = 90

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
        return tuple(depth for depth in Depth if self.name in depth.writers)

    @property
    def compressions(self) -> tuple[type[Compressions], ...]:
        """Compression cases writer accepts."""
        return tuple(form for form in typing.get_args(Compressions.__value__) if self.name in form.writers)


class Subimage[D: np.float32 | np.uint8 | np.uint16](msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Written subimage with pixels on demand."""

    spec: OpenImageIO.ImageSpec
    attributes: tuple[tuple[str, OpenImageIO.TypeDesc, object], ...]
    pixels: Callable[[], ImagingResult[npt.NDArray[D]]]


class ImageFile[D: np.float32 | np.uint8 | np.uint16](msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Pixels, colorspace, and header of read file."""

    pixels: npt.NDArray[D]
    colorspace: str
    header: tuple[OpenImageIO.ParamValue, ...] = ()

    def write(self, path: str, storage: Storage, fields: Mapping[str, str], config: PyOpenColorIO.Config) -> ImagingResult[str]:
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
                    .bind(lambda name: Option.of_optional(OpenImageIO.ColorConfig("ocio://default").get_cicp(name)))  # type: ignore[no-untyped-call]
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


def read_file(path: str, config: PyOpenColorIO.Config, *, colorspace: Option[str] = Nothing, options: Option[OpenImageIO.ImageSpec] = Nothing) -> ImagingResult[ImageFile[np.float32]]:
    """Reads every channel of first subimage as float32 with its colorspace.

    Args:
        path: File to read.
        config: Config the colorspace resolves in.
        colorspace: Name, alias, or role bypassing tags and file rules, inferred from file when `Nothing`.
        options: Reader configuration, OpenImageIO defaults when `Nothing`.

    Returns:
        File, or `read` or `unresolved` fault.
    """

    def decoded(image: OpenImageIO.ImageInput) -> ImagingResult[ImageFile[np.float32]]:
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
            .to_result_with(lambda: (ImagingUnresolvedFault(request=config, error=(requested,)),))
        )
        pixels = Option.of_optional(image.read_image(0, 0, 0, spec.nchannels, OpenImageIO.FLOAT)).to_result_with(lambda: (ImagingReadFault(request=path, error=image.geterror()),))
        return paired(resolved, pixels).map(lambda pair: ImageFile(pixels=pair[1], colorspace=pair[0], header=tuple(spec.extra_attribs)))

    return opened(path, decoded, options=options)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [IO]


def opened[T](path: str, use: Callable[[OpenImageIO.ImageInput], ImagingResult[T]], *, options: Option[OpenImageIO.ImageSpec] = Nothing) -> ImagingResult[T]:
    """Opens a file for reading, uses it, and closes it on every path.

    Args:
        path: File to open.
        use: Reader of open input.
        options: Reader configuration, OpenImageIO defaults when `Nothing`.

    Returns:
        Reader value, or `read` fault for open, read, or close.
    """
    image = options.map(lambda spec: OpenImageIO.ImageInput.open(path, spec)).default_with(lambda: OpenImageIO.ImageInput.open(path))
    return scoped(
        Option.of_optional(image).to_result_with(lambda: (ImagingReadFault(request=path, error=OpenImageIO.geterror()),)),
        use,
        lambda held: Ok(None) if held.close() else Error((ImagingReadFault(request=path, error=held.geterror()),)),
    )


def published[T, F](path: str, encode: Callable[[str], Result[T, Faults[F]]]) -> Result[str, Faults[F | ImagingWriteFault]]:
    """Replaces file with encoded sibling.

    Args:
        path: File to replace.
        encode: Encoder of supplied sibling, returning after its resources close.

    Returns:
        Path, or encoder faults with replace and cleanup `write` faults.
    """
    target = pathlib.Path(path)

    def committed(sibling: pathlib.Path) -> Result[str, Faults[F | ImagingWriteFault]]:
        return (
            Result[T, Faults[F | ImagingWriteFault]]
            .bind(encode(str(sibling)), lambda _: catch(lambda: sibling.replace(target), exception=OSError)().map_error(lambda error: (ImagingWriteFault(request=path, error=error),)))
            .map(lambda _: path)
        )

    return staging(target, committed)


def staging[T, F](target: pathlib.Path, use: Callable[[pathlib.Path], Result[T, Faults[F | ImagingWriteFault]]]) -> Result[T, Faults[F | ImagingWriteFault]]:
    """Runs `use` on target-named path in temporary sibling folder removed on every path.

    Args:
        target: File the staged path takes its name from.
        use: Work on staged path.

    Returns:
        `use` result, with `write` fault for folder creation or cleanup.
    """

    def refused(error: OSError) -> Faults[F | ImagingWriteFault]:
        return (ImagingWriteFault(request=target, error=error),)

    return (
        catch(lambda: target.parent.mkdir(parents=True, exist_ok=True), exception=OSError)()
        .map_error(refused)
        .bind(
            lambda _: scoped(
                catch(tempfile.TemporaryDirectory[str], exception=OSError)(dir=target.parent).map_error(refused),
                lambda stage: use(pathlib.Path(stage.name) / target.name),
                lambda stage: catch(stage.cleanup, exception=OSError)().map_error(refused),
            )
        )
    )


def streamed(path: str, stage: str, write: Callable[[BinaryIO], None]) -> ImagingResult[None]:
    """Streams `write` into `stage`, closing it on every path.

    Args:
        path: File the stage replaces, named in faults.
        stage: Staged sibling `published` supplies.
        write: Encoder of whole file.

    Returns:
        None, or `write` fault.
    """

    def refused(error: OSError | ValueError | OverflowError | struct.error) -> ImagingFaults:
        return (ImagingWriteFault(request=path, error=error),)

    def into(output: BinaryIO) -> ImagingResult[None]:
        try:
            write(output)
        except (OSError, ValueError, OverflowError, struct.error) as error:
            return Error(refused(error))
        return Ok(None)

    return scoped(catch(lambda: pathlib.Path(stage).open("wb"), exception=OSError)().map_error(refused), into, lambda output: catch(output.close, exception=OSError)().map_error(refused))


def emitted[D: np.float32 | np.uint8 | np.uint16](path: str, subimages: Callable[[OpenImageIO.ImageOutput], tuple[Subimage[D], ...]]) -> ImagingResult[str]:
    """Encodes subimages through one output, closing it before replacing file.

    Args:
        path: File to write, suffix naming writer.
        subimages: Subimages built for writer of path.

    Returns:
        Path, or `write` fault.
    """

    def refused(error: str) -> ImagingFaults:
        return (ImagingWriteFault(request=path, error=error),)

    def encode(stage: str) -> ImagingResult[str]:
        def into(output: OpenImageIO.ImageOutput) -> ImagingResult[str]:
            def answered(*, done: bool) -> ImagingResult[str]:
                return Ok(path) if done else Error(refused(output.geterror()))

            def specified(subimage: Subimage[D]) -> OpenImageIO.ImageSpec:
                for name, kind, value in subimage.attributes:
                    subimage.spec.attribute(name, kind, value)
                return subimage.spec

            def step(written: ImagingResult[str], indexed: tuple[int, Subimage[D]]) -> ImagingResult[str]:
                index, subimage = indexed
                return (
                    written
                    .bind(lambda _: answered(done=index == 0 or output.open(stage, subimage.spec, "AppendSubimage")))
                    .bind(lambda _: subimage.pixels())
                    .bind(lambda array: answered(done=output.write_image(array)))
                )

            parts = subimages(output)
            return functools.reduce(step, enumerate(parts), answered(done=output.open(stage, [specified(part) for part in parts])))

        return scoped(
            Option.of_optional(OpenImageIO.ImageOutput.create(path)).to_result_with(lambda: refused(OpenImageIO.geterror())),
            into,
            lambda output: Ok(None) if output.close() else Error(refused(output.geterror())),
        )

    return published(path, encode)


# --- [COLOR_TAGS]


def profile(config: PyOpenColorIO.Config, colorspace: str) -> ImagingResult[Option[bytes]]:
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
        .map(lambda location: catch(pathlib.Path(location).read_bytes, exception=OSError)().map_error(lambda error: (ImagingReadFault(request=location, error=error),)).map(Some))
        .default_value(Ok(Nothing))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Blocks",
    "Codec",
    "Compressions",
    "Deflate",
    "Depth",
    "Dwa",
    "Format",
    "ImageFile",
    "Plain",
    "Quality",
    "Storage",
    "Subimage",
    "Writable",
    "emitted",
    "opened",
    "profile",
    "published",
    "read_file",
    "staging",
    "streamed",
]
