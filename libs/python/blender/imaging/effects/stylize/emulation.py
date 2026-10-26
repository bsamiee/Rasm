"""Video compression, corruption, tape, signal, and picture-tube parameters."""

from typing import Annotated

from expression import Error, Ok, Option, Some
import imagecodecs
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO

from blender.imaging.chain import Effect, Stage
from blender.imaging.faults import ImagingBufferFault, ImagingNumericFault, ImagingResult
from blender.imaging.parameters import Choice, Finite, FrameExtent, FrameLength, Gated, Motion, NonNegative, Positive, Presentation, Quantity, Seed, Unit

# --- [TYPES] ----------------------------------------------------------------------------


class Subsampling(Choice):
    """Chroma resolution relative to coded luma."""

    factors: tuple[int, int]

    def __init__(self, value: int, factors: tuple[int, int], label: str | None = None) -> None:
        """Sets chroma block width and height in luma blocks."""
        super().__init__(value, label)
        self.factors = factors

    YUV444 = 1, (1, 1), "4:4:4"
    YUV422 = 2, (2, 1), "4:2:2"
    YUV420 = 3, (2, 2), "4:2:0"
    YUV411 = 4, (4, 1), "4:1:1"


class Layout(Choice):
    """Arrangement of red, green, and blue phosphors."""

    geometry: tuple[float, float, float]

    def __init__(self, value: int, geometry: tuple[float, float, float]) -> None:
        """Sets black-matrix gap, column stagger, and row stagger in triad cells."""
        super().__init__(value)
        self.geometry = geometry

    APERTURE_GRILLE = 1, (0.0, 0.0, 0.0)
    SLOT_MASK = 2, (1.0 / 3.0, 0.5, 0.0)
    SHADOW_MASK = 3, (1.0 / 3.0, 0.0, 0.5)


# --- [MODELS] ---------------------------------------------------------------------------


class Compression(msgspec.Struct, frozen=True, kw_only=True):
    """Repeated JPEG generations at a coded picture width."""

    quality: Annotated[int, msgspec.Meta(ge=1, le=100, description="JPEG quality on IJG scale")] = 60
    coded_width: Annotated[Gated[Annotated[int, msgspec.Meta(ge=1), Presentation(soft=Some((160.0, 1920.0)))]], msgspec.Meta(description="Picture width before coding, input raster size when off")] = (
        Gated(enabled=False, value=640)
    )
    subsampling: Annotated[Subsampling, msgspec.Meta(description="Chroma resolution relative to luma")] = Subsampling.YUV420
    generations: Annotated[int, msgspec.Meta(ge=1, description="JPEG encode and decode cycles"), Presentation(soft=Some((1.0, 8.0)))] = 1

    def cycled(self, rgb: npt.NDArray[np.uint8]) -> ImagingResult[npt.NDArray[np.uint8]]:
        """Applies successive native JPEG generations to coded RGB samples.

        Args:
            rgb: Contiguous eight-bit RGB samples at coded dimensions.

        Returns:
            Decoded RGB samples or the native JPEG refusal.
        """
        try:
            for _ in range(self.generations):
                rgb = imagecodecs.jpeg8_decode(
                    imagecodecs.jpeg8_encode(rgb, level=self.quality, colorspace=imagecodecs.JPEG8.CS.RGB, outcolorspace=imagecodecs.JPEG8.CS.YCbCr, subsampling=self.subsampling.factors),
                    outcolorspace=imagecodecs.JPEG8.CS.RGB,
                )
            return Ok(rgb)
        except imagecodecs.Jpeg8Error as error:
            return Error((ImagingNumericFault(request=self, error=error),))

    def applied(self, source: OpenImageIO.ImageBuf, coded: Option[OpenImageIO.ImageSpec]) -> ImagingResult[OpenImageIO.ImageBuf]:
        """Codes native RGB with optional box reduction and triangle reconstruction.

        Args:
            source: Straight display-encoded RGB, caller holding its alpha channel.
            coded: Validated native data and display windows at coded width, or `Nothing` for input raster size.

        Returns:
            Decoded RGB in input windows, native buffer failures, or JPEG refusal.
        """

        def resized(image: OpenImageIO.ImageBuf, spec: OpenImageIO.ImageSpec, filtername: str) -> ImagingResult[OpenImageIO.ImageBuf]:
            target = OpenImageIO.ImageBuf(spec)
            return Ok(target) if OpenImageIO.ImageBufAlgo.resize(target, image, filtername=filtername, roi=target.roi) else Error((ImagingBufferFault(request=target, error=target.geterror()),))

        def processed(image: OpenImageIO.ImageBuf) -> ImagingResult[OpenImageIO.ImageBuf]:
            return (
                Option
                .of_optional(image.get_pixels(OpenImageIO.UINT8))
                .map(lambda rgb: self.cycled(rgb).bind(lambda values: ImagingBufferFault.located(values, image)))
                .default_with(lambda: Error((ImagingBufferFault(request=image, error=image.geterror()),)))
            )

        return (
            coded
            .map(lambda spec: resized(source, spec, "box"))
            .default_value(Ok(source))
            .bind(processed)
            .bind(lambda image: coded.map(lambda _: resized(image, source.spec(), "triangle")).default_value(Ok(image)))
        )


class Corruption(msgspec.Struct, frozen=True, kw_only=True):
    """Rectangular buffer corruption with a frame-wide RGB split."""

    coverage: Annotated[Unit, msgspec.Meta(description="Share of corrupted cells per pattern")] = 0.2
    cell_width: Annotated[FrameExtent, msgspec.Meta(description="Corrupted cell width"), Presentation(soft=Some((0.005, 1.0)))] = 0.03
    cell_height: Annotated[FrameExtent, msgspec.Meta(description="Corrupted cell height"), Presentation(soft=Some((0.005, 1.0)))] = 0.03
    displacement: Annotated[NonNegative, msgspec.Meta(description="Greatest horizontal cell shift"), Presentation(soft=Some((0.0, 0.2)), quantity=Some(Quantity.FRAME))] = 0.004
    split: Annotated[NonNegative, msgspec.Meta(description="Opposing red and blue offsets across frame"), Presentation(soft=Some((0.0, 0.2)), quantity=Some(Quantity.FRAME))] = 0.0
    chroma: Annotated[Unit, msgspec.Meta(description="Sign-preserving chroma power in struck cells")] = 0.6


class Tape(msgspec.Struct, frozen=True, kw_only=True):
    """Time-base error, head switching, tracking disturbance, and dropouts."""

    jitter: Annotated[NonNegative, msgspec.Meta(description="Standard deviation of horizontal line displacement"), Presentation(soft=Some((0.0, 0.01)), quantity=Some(Quantity.FRAME))] = 0.002
    jitter_period: Annotated[NonNegative, msgspec.Meta(description="Vertical jitter smoothing period"), Presentation(quantity=Some(Quantity.FRAME))] = 1.0 / 65.0
    bounce: Annotated[NonNegative, msgspec.Meta(description="Greatest vertical frame shift"), Presentation(soft=Some((0.0, 0.02)), quantity=Some(Quantity.FRAME))] = 0.0
    head_switching: Annotated[NonNegative, msgspec.Meta(description="Skewed bottom band height"), Presentation(soft=Some((0.0, 0.05)), quantity=Some(Quantity.FRAME))] = 0.0176
    skew: Annotated[Finite, msgspec.Meta(description="Peak horizontal head-switching shift at nine tenths of band"), Presentation(soft=Some((-0.1, 0.1)), quantity=Some(Quantity.FRAME))] = 0.05
    tracking_shift: Annotated[Finite, msgspec.Meta(description="Shift at tracking band center"), Presentation(soft=Some((-0.1, 0.1)), quantity=Some(Quantity.FRAME))] = 0.0
    tracking_height: Annotated[Positive, msgspec.Meta(description="Tracking band height"), Presentation(soft=Some((0.0, 0.25)), quantity=Some(Quantity.FRAME))] = 0.15
    tracking_rate: Annotated[NonNegative, msgspec.Meta(description="Frame heights traveled upward per clock frame")] = 0.0
    mistracking: Annotated[Unit, msgspec.Meta(description="Luma noise in tracking band")] = 0.0
    dropouts: Annotated[Unit, msgspec.Meta(description="Share of dropout-length cells struck per raster line"), Presentation(soft=Some((0.0, 0.2)))] = 0.02
    dropout_length: Annotated[Positive, msgspec.Meta(description="Fading dropout cell length"), Presentation(soft=Some((0.0, 0.2)), quantity=Some(Quantity.FRAME))] = 0.04


class Ghost(msgspec.Struct, frozen=True, kw_only=True):
    """Delayed signal echo from a second transmission path."""

    amplitude: Annotated[Unit, msgspec.Meta(description="Delayed echo strength")] = 0.1
    delay: Annotated[Finite, msgspec.Meta(description="Horizontal echo offset, right above zero"), Presentation(soft=Some((-0.2, 0.2)), quantity=Some(Quantity.FRAME))] = 0.05


class Signal(msgspec.Struct, frozen=True, kw_only=True):
    """Analog video bandwidth, Gaussian noise, and carrier disturbances."""

    luma_blur: Annotated[NonNegative, msgspec.Meta(description="Horizontal luma bandwidth blur"), Presentation(soft=Some((0.0, 0.01)), quantity=Some(Quantity.FRAME))] = 0.0023
    chroma_blur: Annotated[NonNegative, msgspec.Meta(description="Horizontal color-difference bandwidth blur"), Presentation(soft=Some((0.0, 0.05)), quantity=Some(Quantity.FRAME))] = 0.0073
    chroma_vertical: Annotated[NonNegative, msgspec.Meta(description="Vertical color-difference blur"), Presentation(soft=Some((0.0, 0.02)), quantity=Some(Quantity.FRAME))] = 0.0
    chroma_delay: Annotated[Finite, msgspec.Meta(description="Color-difference offset from luma, right above zero"), Presentation(soft=Some((-0.02, 0.02)), quantity=Some(Quantity.FRAME))] = 0.0
    saturation: Annotated[NonNegative, msgspec.Meta(description="Color-difference gain"), Presentation(soft=Some((0.0, 2.0)))] = 1.0
    ringing: Annotated[Unit, msgspec.Meta(description="Second-difference overshoot at luma edges")] = 0.1
    cross_color: Annotated[Unit, msgspec.Meta(description="Fine luma detail decoded as NTSC subcarrier color")] = 0.0
    noise: Annotated[NonNegative, msgspec.Meta(description="Luma noise standard deviation per raster sample"), Presentation(soft=Some((0.0, 0.2)))] = 0.02
    chroma_noise: Annotated[NonNegative, msgspec.Meta(description="Color-difference noise standard deviation per raster sample"), Presentation(soft=Some((0.0, 0.2)))] = 0.03
    hum: Annotated[Unit, msgspec.Meta(description="Depth of mains hum bar drifting upward"), Presentation(soft=Some((0.0, 0.5)))] = 0.0
    ghost: Annotated[Gated[Ghost], msgspec.Meta(description="Multipath echo of whole signal")] = Gated(enabled=False, value=Ghost())


class Phosphors(msgspec.Struct, frozen=True, kw_only=True):
    """Mean-preserving phosphor mask integrated per pixel footprint."""

    layout: Annotated[Layout, msgspec.Meta(description="Phosphor arrangement")] = Layout.APERTURE_GRILLE
    triads: Annotated[int, msgspec.Meta(ge=1, description="Phosphor triads across frame width"), Presentation(soft=Some((80.0, 1280.0)))] = 640
    strength: Annotated[Unit, msgspec.Meta(description="Blend toward mean-one phosphor mask")] = 0.705394


class Scanlines(msgspec.Struct, frozen=True, kw_only=True):
    """Mean-preserving raster lines integrated per pixel footprint."""

    strength: Annotated[Unit, msgspec.Meta(description="Darkening between raster lines before mean normalization")] = 0.4
    interlaced: Annotated[bool, msgspec.Meta(description="Light alternate raster fields per pattern")] = False


class Display(msgspec.Struct, frozen=True, kw_only=True):
    """Picture tube showing a collapsed, glowing, scanned, and masked signal."""

    glow: Annotated[
        Gated[Annotated[FrameLength, msgspec.Meta(title="Radius", description="Gaussian glass blur"), Presentation(soft=Some((0.0, 0.05)))]],
        msgspec.Meta(description="Linear screen-glass scattering before scanlines and phosphors"),
    ] = Gated(enabled=False, value=0.01)
    scanlines: Annotated[Gated[Scanlines], msgspec.Meta(description="Raster lines the beam draws")] = Gated(enabled=True, value=Scanlines())
    phosphors: Annotated[Gated[Phosphors], msgspec.Meta(description="Phosphor mask on glass")] = Gated(enabled=False, value=Phosphors())
    curvature: Annotated[NonNegative, msgspec.Meta(description="Screen glass barrel bulge"), Presentation(soft=Some((0.0, 0.25)))] = 0.03
    roll: Annotated[Finite, msgspec.Meta(description="Frame heights rolled per clock frame without vertical hold"), Presentation(soft=Some((-0.05, 0.05)))] = 0.0
    collapse: Annotated[Unit, msgspec.Meta(description="Power-off collapse to line, then dot")] = 0.0


class Emulation(Effect, frozen=True, kw_only=True, tag=40):
    """Coded, transmitted, taped, received, and displayed video path."""

    stage = Stage.EMULATION
    pointwise = False
    compression: Annotated[Gated[Compression], msgspec.Meta(description="JPEG picture block coding")] = Gated(enabled=False, value=Compression())
    corruption: Annotated[Gated[Corruption], msgspec.Meta(description="Digital picture corruption")] = Gated(enabled=False, value=Corruption())
    tape: Annotated[Gated[Tape], msgspec.Meta(description="Videotape playback errors")] = Gated(enabled=False, value=Tape())
    signal: Annotated[Gated[Signal], msgspec.Meta(description="Analog video bandwidth and noise")] = Gated(enabled=True, value=Signal())
    display: Annotated[Gated[Display], msgspec.Meta(description="Picture tube showing signal")] = Gated(enabled=True, value=Display())
    lines: Annotated[int, msgspec.Meta(ge=1, description="Raster lines across frame height"), Presentation(soft=Some((120.0, 1080.0)))] = 480
    motion: Annotated[Motion, msgspec.Meta(description="Clock of every pattern")] = Motion()
    seed: Annotated[Seed, msgspec.Meta(description="Number selecting every random pattern")] = 0


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Compression", "Corruption", "Display", "Emulation", "Ghost", "Layout", "Phosphors", "Scanlines", "Signal", "Subsampling", "Tape"]
