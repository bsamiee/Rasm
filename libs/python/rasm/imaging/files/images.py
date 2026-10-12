"""Image files and native storage with colorspace tags and atomic publication."""

from collections.abc import Callable, Mapping
import functools
import pathlib
import tempfile
import typing
from typing import Annotated, Literal, Self

from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from rasm.imaging.faults import Faults, ImagingFaults, ImagingReadFault, ImagingResult, ImagingUnresolvedFault, ImagingWriteFault, paired
from rasm.imaging.parameters import Case, Choice

# --- [TYPES] ----------------------------------------------------------------------------


class Writable(Choice):
    """Closed set whose members name the writers that store them."""

    writers: frozenset[str]

    def __new__(cls, value: int, label: str, *writers: str) -> Self:
        """Creates a member from its persistent number, label, and writers."""
        member = int.__new__(cls, value)
        member._value_, member.label, member.writers = value, label, frozenset(writers)
        return member


class Depth(Writable):
    """Stored channel type, its lowercased name an OpenImageIO type name."""

    UINT8 = 1, "8-bit Integer", "bmp", "dpx", "fits", "ico", "iff", "jpeg", "jpeg2000", "png", "pnm", "rla", "sgi", "targa", "tiff", "webp"
    UINT16 = 2, "16-bit Integer", "dpx", "iff", "jpeg2000", "png", "pnm", "rla", "sgi", "tiff"
    HALF = 3, "16-bit Float", "openexr", "tiff"
    FLOAT = 4, "32-bit Float", "dpx", "fits", "hdr", "openexr", "pnm", "rla", "tiff"


class Codec(Writable):
    """Codec that takes no level."""

    NONE = 1, "None", "openexr", "tiff"
    RLE = 2, "RLE", "openexr"
    ZIPS = 3, "ZIPS", "openexr"
    PIZ = 4, "PIZ", "openexr"
    PXR24 = 5, "Pxr24", "openexr"
    B44 = 6, "B44", "openexr"
    B44A = 7, "B44A", "openexr"
    HTJ2K32 = 8, "HTJ2K 32 Scanlines", "openexr"
    HTJ2K256 = 9, "HTJ2K 256 Scanlines", "openexr"
    LZW = 10, "LZW", "tiff"
    PACKBITS = 11, "PackBits", "tiff"
    LOSSLESS = 12, "Lossless", "webp"


class Blocks(Choice):
    """Scanlines per DWA block, its lowercased name the OpenEXR codec."""

    DWAA = 1, "32 Scanlines"
    DWAB = 2, "256 Scanlines"


# --- [MODELS] ---------------------------------------------------------------------------


class Plain(Case, frozen=True, kw_only=True, tag=1):
    """Codec that takes no level."""

    title = "Plain"
    writers = frozenset[str]().union(*(codec.writers for codec in Codec))
    margin = 0
    codec: Annotated[Codec, msgspec.Meta(description="Codec the writer applies")] = Codec.NONE

    @property
    def lossy(self) -> bool:
        """Whether the codec discards information."""
        return self.codec in {Codec.PXR24, Codec.B44, Codec.B44A}

    @property
    def attributes(self) -> tuple[tuple[str, OpenImageIO.TypeDesc, str | int], ...]:
        """Spec attributes the codec sets."""
        return (("compression", OpenImageIO.TypeString, self.codec.name.lower()),)


class Deflate(Case, frozen=True, kw_only=True, tag=2):
    """Lossless zlib deflate at a level."""

    title = "Deflate"
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
        """Headroom below the half maximum the level's quantization takes."""
        return 2048 + 32 * self.level if self.level <= 50 else 3624 + 128 * (self.level - 50)

    @property
    def attributes(self) -> tuple[tuple[str, OpenImageIO.TypeDesc, str | int], ...]:
        """Spec attributes the blocks and level set."""
        return (("compression", OpenImageIO.TypeString, f"{self.blocks.name.lower()}:{self.level}"),)


class Quality(Case, frozen=True, kw_only=True, tag=4):
    """Lossy compression at an encoder quality."""

    title = "Quality"
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
    """Stored type of a float plane and compression of a write, integer codes storing at their dtype."""

    depth: Literal[16, 32] = 32
    compression: Option[Compressions] = Nothing

    @property
    def attributes(self) -> tuple[tuple[str, OpenImageIO.TypeDesc, str | int], ...]:
        """Spec attributes of the compression, none for the writer's own default."""
        return self.compression.map(lambda compression: compression.attributes).default_value(())

    def clipped[D: np.float32 | np.uint8 | np.uint16](self, plane: npt.NDArray[D]) -> npt.NDArray[D]:
        """Clips a plane to the magnitude half storage keeps under the compression's margin.

        Args:
            plane: Plane the write stores.

        Returns:
            The plane clipped to 65504 less the margin at 16-bit storage, otherwise the plane itself.
        """
        bound = 65504.0 - self.compression.map(lambda compression: compression.margin).default_value(0)
        return np.clip(plane, -bound, bound) if self.depth == 16 else plane


class Format(msgspec.Struct, frozen=True, kw_only=True):
    """One entry of OpenImageIO's extension list."""

    name: str
    extensions: tuple[str, ...]

    @classmethod
    def readable(cls) -> tuple[Self, ...]:
        """Lists every format a reader exists for.

        Returns:
            The extension list entries in OpenImageIO's order.
        """
        entries = OpenImageIO.get_string_attribute("extension_list").split(";")
        return tuple(cls(name=name, extensions=tuple(suffixes.split(","))) for name, _, suffixes in (entry.partition(":") for entry in entries))

    @classmethod
    def writable(cls) -> tuple[Self, ...]:
        """Lists every format a writer exists for that stores a depth.

        Returns:
            The readable entries named by the output format list and holding a depth.
        """
        writers = frozenset(OpenImageIO.get_string_attribute("output_format_list").split(","))
        return tuple(row for row in cls.readable() if row.name in writers and row.depths)

    @property
    def depths(self) -> tuple[Depth, ...]:
        """Depths the format's writer stores unchanged."""
        return tuple(depth for depth in Depth if self.name in depth.writers)

    @property
    def compressions(self) -> tuple[type[Compressions], ...]:
        """Compression cases the format's writer reads."""
        return tuple(form for form in typing.get_args(Compressions.__value__) if self.name in form.writers)


class Subimage[D: np.float32 | np.uint8 | np.uint16](msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """One subimage of a write: its spec, the attributes it takes, and its pixels on demand."""

    spec: OpenImageIO.ImageSpec
    attributes: tuple[tuple[str, OpenImageIO.TypeDesc, object], ...]
    pixels: Callable[[], ImagingResult[npt.NDArray[D]]]


class ImageFile[D: np.float32 | np.uint8 | np.uint16](msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Pixels of an image file, their colorspace, and the header a read held."""

    pixels: npt.NDArray[D]
    colorspace: str
    header: tuple[OpenImageIO.ParamValue, ...] = ()

    def write(self, path: str, storage: Storage, fields: Mapping[str, str], config: PyOpenColorIO.Config) -> ImagingResult[str]:
        """Writes the pixels with the header, fields, storage, and color tags of their colorspace.

        Args:
            path: File to write, its suffix naming the writer.
            storage: Stored channel type and compression.
            fields: Text fields stored by name.
            config: Config the colorspace resolves in.

        Returns:
            The path; a `read` fault for profile IO, or a `write` fault holding the path and OpenImageIO's text or the `OSError`.
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
    """Reads every channel of a file's first subimage as float32 with its colorspace.

    Args:
        path: File to read.
        config: Config the colorspace resolves in.
        colorspace: Explicit name, alias, or role, bypassing tags and file rules; `Nothing` infers from the file.
        options: Native reader configuration; `Nothing` retains provider defaults.

    Returns:
        The file; the `read` case holding the path and OpenImageIO's text, or `unresolved` holding the config and the explicit or inferred name.
    """

    def decoded(image: OpenImageIO.ImageInput) -> ImagingResult[ImageFile[np.float32]]:
        try:
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
            decoded = paired(resolved, pixels).map(lambda pair: ImageFile(pixels=pair[1], colorspace=pair[0], header=tuple(spec.extra_attribs)))
        finally:
            closed = Ok(None) if image.close() else Error((ImagingReadFault(request=path, error=image.geterror()),))
        return paired(decoded, closed).map(lambda pair: pair[0])

    return opened(path, options=options).bind(decoded)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [IO]


def opened(path: str, *, options: Option[OpenImageIO.ImageSpec] = Nothing) -> ImagingResult[OpenImageIO.ImageInput]:
    """Opens a file for reading.

    Args:
        path: File to open.
        options: Native reader configuration; `Nothing` retains provider defaults.

    Returns:
        The input, which the caller closes; the `read` case holding the path and OpenImageIO's text.
    """
    image = options.map(lambda spec: OpenImageIO.ImageInput.open(path, spec)).default_with(lambda: OpenImageIO.ImageInput.open(path))
    return Option.of_optional(image).to_result_with(lambda: (ImagingReadFault(request=path, error=OpenImageIO.geterror()),))


def published[T, F](path: str, encode: Callable[[str], Result[T, Faults[F]]]) -> Result[str, Faults[F | ImagingWriteFault]]:
    """Publishes an encoded sibling after the encoder closes it successfully.

    Args:
        path: Final file to replace.
        encode: Encoder of the supplied sibling, answering after its native resources close.

    Returns:
        The final path; the encoder's faults and any publication or cleanup `write` faults.
    """
    target = pathlib.Path(path)

    def refused(error: OSError) -> Faults[F | ImagingWriteFault]:
        return (ImagingWriteFault(request=path, error=error),)

    def committed(stage: tempfile.TemporaryDirectory[str]) -> Result[str, Faults[F | ImagingWriteFault]]:
        try:
            sibling = pathlib.Path(stage.name) / target.name
            written = Result[T, Faults[F | ImagingWriteFault]].bind(encode(str(sibling)), lambda _: catch(lambda: sibling.replace(target), exception=OSError)().map_error(refused))
        finally:
            cleaned = catch(stage.cleanup, exception=OSError)().map_error(refused)
        return paired(written, cleaned).map(lambda _: path)

    return (
        catch(lambda: target.parent.mkdir(parents=True, exist_ok=True), exception=OSError)()
        .map_error(refused)
        .bind(lambda _: catch(lambda: tempfile.TemporaryDirectory(dir=target.parent), exception=OSError)().map_error(refused))
        .bind(committed)
    )


def emitted[D: np.float32 | np.uint8 | np.uint16](path: str, subimages: Callable[[OpenImageIO.ImageOutput], tuple[Subimage[D], ...]]) -> ImagingResult[str]:
    """Encodes and closes subimages through one output before publishing the file.

    Args:
        path: File to write, its suffix naming the writer.
        subimages: Subimages built for the writer the path names.

    Returns:
        The path; the `write` case holding the path and OpenImageIO's text or the `OSError`.
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

            try:
                parts = subimages(output)
                written = functools.reduce(step, enumerate(parts), answered(done=output.open(stage, [specified(part) for part in parts])))
            finally:
                closed = answered(done=output.close())
            return paired(written, closed).map(lambda _: path)

        return Option.of_optional(OpenImageIO.ImageOutput.create(path)).to_result_with(lambda: refused(OpenImageIO.geterror())).bind(into)

    return published(path, encode)


# --- [COLOR_TAGS]


def profile(config: PyOpenColorIO.Config, colorspace: str) -> ImagingResult[Option[bytes]]:
    """Reads the ICC profile of a colorspace from the config's search path.

    Args:
        config: Config the colorspace resolves in.
        colorspace: Colorspace name.

    Returns:
        The profile bytes of the first resolved name, `Nothing` when none resolves, or a read fault for failed profile IO.
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

    resolved = (
        Option
        .of_optional(config.getColorSpace(colorspace))
        .map(names)
        .bind(
            lambda found: next(
                (Some(location) for name in found for location in catch(functools.partial(context.resolveFileLocation, name), exception=PyOpenColorIO.ExceptionMissingFile)().to_option().to_list()),
                Nothing,
            )
        )
    )
    return resolved.map(
        lambda location: catch(pathlib.Path(location).read_bytes, exception=OSError)().map_error(lambda error: (ImagingReadFault(request=location, error=error),)).map(Some)
    ).default_value(Ok(Nothing))


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
]
