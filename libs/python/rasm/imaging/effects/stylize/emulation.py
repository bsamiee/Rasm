"""Video compression, corruption, tape, signal, and picture-tube parameters."""

import sys
from typing import Annotated

from expression import Error, Ok, Option, Some
import imagecodecs
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO

from rasm.imaging.chain import Effect, Stage
from rasm.imaging.faults import ImagingBufferFault, ImagingJpegFault, ImagingResult
from rasm.imaging.parameters import Choice, Finite, Gated, Motion, Presentation, Quantity, Seed

# --- [TYPES] ----------------------------------------------------------------------------


class Subsampling(Choice):
    """Chroma resolution relative to coded luma."""

    YUV444 = 1, "4:4:4"
    YUV422 = 2, "4:2:2"
    YUV420 = 3, "4:2:0"
    YUV411 = 4, "4:1:1"

    @property
    def factors(self) -> tuple[int, int]:
        """Chroma block width and height in luma blocks."""
        match self:
            case Subsampling.YUV444:
                return (1, 1)
            case Subsampling.YUV422:
                return (2, 1)
            case Subsampling.YUV420:
                return (2, 2)
            case Subsampling.YUV411:
                return (4, 1)


class Layout(Choice):
    """Arrangement of red, green, and blue phosphors."""

    APERTURE_GRILLE = 1, "Aperture Grille"
    SLOT_MASK = 2, "Slot Mask"
    SHADOW_MASK = 3, "Shadow Mask"

    @property
    def geometry(self) -> tuple[float, float, float]:
        """Black-matrix gap, column stagger, and row stagger in triad cells."""
        match self:
            case Layout.APERTURE_GRILLE:
                return (0.0, 0.0, 0.0)
            case Layout.SLOT_MASK:
                return (1.0 / 3.0, 0.5, 0.0)
            case Layout.SHADOW_MASK:
                return (1.0 / 3.0, 0.0, 0.5)


# --- [MODELS] ---------------------------------------------------------------------------


class Compression(msgspec.Struct, frozen=True, kw_only=True):
    """Repeated JPEG encode/decode generations at a coded picture width."""

    quality: Annotated[int, msgspec.Meta(ge=1, le=100, title="Quality", description="JPEG quality on the IJG scale")] = 60
    coded_width: Annotated[
        Gated[Annotated[int, msgspec.Meta(ge=1), Presentation(soft=Some((160.0, 1920.0)))]],
        msgspec.Meta(title="Coded Width", description="Resize to this picture width before coding; disabled retains the entering raster"),
    ] = Gated(enabled=False, value=640)
    subsampling: Annotated[Subsampling, msgspec.Meta(title="Subsampling", description="Chroma resolution relative to luma")] = Subsampling.YUV420
    generations: Annotated[int, msgspec.Meta(ge=1, title="Generations", description="Successive JPEG encode and decode cycles"), Presentation(soft=Some((1.0, 8.0)))] = 1

    def cycled(self, rgb: npt.NDArray[np.uint8]) -> ImagingResult[npt.NDArray[np.uint8]]:
        """Applies successive native JPEG generations to coded RGB samples.

        Args:
            rgb: Contiguous eight-bit RGB samples at the coded dimensions.

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
            return Error((ImagingJpegFault(request=self, error=error),))

    def applied(self, source: OpenImageIO.ImageBuf, coded: Option[OpenImageIO.ImageSpec]) -> ImagingResult[OpenImageIO.ImageBuf]:
        """Codes native RGB with optional box reduction and triangle reconstruction.

        Args:
            source: Straight display-encoded RGB; its alpha lane is held by the caller.
            coded: Admitted native data and display windows at the coded width,
                or absence to retain the entering raster without resizing.

        Returns:
            Decoded RGB in the entering windows, native buffer failures, or JPEG refusal.
        """

        def buffered(image: OpenImageIO.ImageBuf) -> ImagingResult[OpenImageIO.ImageBuf]:
            return Error((ImagingBufferFault(request=image, error=image.geterror()),)) if image.has_error else Ok(image)

        def resized(image: OpenImageIO.ImageBuf, spec: OpenImageIO.ImageSpec, filtername: str) -> ImagingResult[OpenImageIO.ImageBuf]:
            target = OpenImageIO.ImageBuf(spec)
            return Ok(target) if OpenImageIO.ImageBufAlgo.resize(target, image, filtername=filtername, roi=target.roi) else Error((ImagingBufferFault(request=target, error=target.geterror()),))

        def formed(rgb: npt.NDArray[np.uint8], reference: OpenImageIO.ImageBuf) -> ImagingResult[OpenImageIO.ImageBuf]:
            image = OpenImageIO.ImageBuf(rgb)
            image.copy_metadata(reference)
            image.set_origin(reference.xbegin, reference.ybegin, reference.zbegin)
            image.roi_full = reference.roi_full
            return buffered(image)

        def processed(image: OpenImageIO.ImageBuf) -> ImagingResult[OpenImageIO.ImageBuf]:
            return (
                Option
                .of_optional(image.get_pixels(OpenImageIO.UINT8))
                .map(lambda rgb: self.cycled(rgb).bind(lambda values: formed(values, image)))
                .default_with(lambda: Error((ImagingBufferFault(request=image, error=image.geterror()),)))
            )

        return (
            coded
            .map(lambda spec: resized(source, spec, "box"))
            .default_with(lambda: Ok(source))
            .bind(processed)
            .bind(lambda image: coded.map(lambda _: resized(image, source.spec(), "triangle")).default_with(lambda: Ok(image)))
        )


class Corruption(msgspec.Struct, frozen=True, kw_only=True):
    """Rectangular buffer corruption with a frame-wide RGB split."""

    coverage: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Coverage", description="Share of corrupted cells per pattern")] = 0.2
    cell_width: Annotated[
        float, msgspec.Meta(gt=0.0, le=sys.float_info.max, title="Cell Width", description="Corrupted cell width"), Presentation(soft=Some((0.005, 1.0)), quantity=Some(Quantity.FRAME_GREATER))
    ] = 0.03
    cell_height: Annotated[
        float, msgspec.Meta(gt=0.0, le=sys.float_info.max, title="Cell Height", description="Corrupted cell height"), Presentation(soft=Some((0.005, 1.0)), quantity=Some(Quantity.FRAME_GREATER))
    ] = 0.03
    displacement: Annotated[
        float, msgspec.Meta(ge=0.0, le=sys.float_info.max, title="Displacement", description="Greatest horizontal cell shift"), Presentation(soft=Some((0.0, 0.2)), quantity=Some(Quantity.FRAME_WIDTH))
    ] = 0.004
    split: Annotated[
        float,
        msgspec.Meta(ge=0.0, le=sys.float_info.max, title="Split", description="Opposing red and blue offsets across the frame"),
        Presentation(soft=Some((0.0, 0.2)), quantity=Some(Quantity.FRAME_WIDTH)),
    ] = 0.0
    chroma: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Chroma", description="Strength of struck cells' sign-preserving chroma power")] = 0.6


class Tape(msgspec.Struct, frozen=True, kw_only=True):
    """Time-base error, head switching, tracking disturbance, and dropouts."""

    jitter: Annotated[
        float,
        msgspec.Meta(ge=0.0, le=sys.float_info.max, title="Jitter", description="Standard deviation of horizontal line displacement"),
        Presentation(soft=Some((0.0, 0.01)), quantity=Some(Quantity.FRAME_WIDTH)),
    ] = 0.002
    jitter_period: Annotated[
        float,
        msgspec.Meta(ge=0.0, le=sys.float_info.max, title="Jitter Period", description="Vertical smoothing period of unit-variance line jitter"),
        Presentation(quantity=Some(Quantity.FRAME_HEIGHT)),
    ] = 1.0 / 65.0
    bounce: Annotated[
        float, msgspec.Meta(ge=0.0, le=sys.float_info.max, title="Bounce", description="Greatest vertical frame shift"), Presentation(soft=Some((0.0, 0.02)), quantity=Some(Quantity.FRAME_HEIGHT))
    ] = 0.0
    head_switching: Annotated[
        float,
        msgspec.Meta(ge=0.0, le=sys.float_info.max, title="Head Switching", description="Height of the skewed bottom band"),
        Presentation(soft=Some((0.0, 0.05)), quantity=Some(Quantity.FRAME_HEIGHT)),
    ] = 0.0176
    skew: Annotated[
        Finite, msgspec.Meta(title="Skew", description="Peak horizontal head-switching shift at nine tenths of the band"), Presentation(soft=Some((-0.1, 0.1)), quantity=Some(Quantity.FRAME_WIDTH))
    ] = 0.05
    tracking_shift: Annotated[
        Finite, msgspec.Meta(title="Tracking Shift", description="Horizontal displacement at the tracking band's center"), Presentation(soft=Some((-0.1, 0.1)), quantity=Some(Quantity.FRAME_WIDTH))
    ] = 0.0
    tracking_height: Annotated[
        float,
        msgspec.Meta(gt=0.0, le=sys.float_info.max, title="Tracking Height", description="Height of the tracking band"),
        Presentation(soft=Some((0.0, 0.25)), quantity=Some(Quantity.FRAME_HEIGHT)),
    ] = 0.15
    tracking_rate: Annotated[float, msgspec.Meta(ge=0.0, le=sys.float_info.max, title="Tracking Rate", description="Frame heights traveled upward per clock frame")] = 0.0
    mistracking: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Mistracking", description="Luma noise strength within the tracking band")] = 0.0
    dropouts: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Dropouts", description="Share of dropout-length cells struck per raster line"), Presentation(soft=Some((0.0, 0.2)))] = 0.02
    dropout_length: Annotated[
        float,
        msgspec.Meta(gt=0.0, le=sys.float_info.max, title="Dropout Length", description="Length of a fading dropout cell"),
        Presentation(soft=Some((0.0, 0.2)), quantity=Some(Quantity.FRAME_WIDTH)),
    ] = 0.04


class Ghost(msgspec.Struct, frozen=True, kw_only=True):
    """Delayed signal echo from a second transmission path."""

    amplitude: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Amplitude", description="Delayed echo strength")] = 0.1
    delay: Annotated[Finite, msgspec.Meta(title="Delay", description="Horizontal echo offset, right above zero"), Presentation(soft=Some((-0.2, 0.2)), quantity=Some(Quantity.FRAME_WIDTH))] = 0.05


class Signal(msgspec.Struct, frozen=True, kw_only=True):
    """Analog video bandwidth, Gaussian noise, and carrier disturbances."""

    luma_blur: Annotated[
        float, msgspec.Meta(ge=0.0, le=sys.float_info.max, title="Luma Blur", description="Horizontal luma bandwidth blur"), Presentation(soft=Some((0.0, 0.01)), quantity=Some(Quantity.FRAME_WIDTH))
    ] = 0.0023
    chroma_blur: Annotated[
        float,
        msgspec.Meta(ge=0.0, le=sys.float_info.max, title="Chroma Blur", description="Horizontal color-difference bandwidth blur"),
        Presentation(soft=Some((0.0, 0.05)), quantity=Some(Quantity.FRAME_WIDTH)),
    ] = 0.0073
    chroma_vertical: Annotated[
        float,
        msgspec.Meta(ge=0.0, le=sys.float_info.max, title="Chroma Vertical", description="Vertical color-difference blur across raster lines"),
        Presentation(soft=Some((0.0, 0.02)), quantity=Some(Quantity.FRAME_HEIGHT)),
    ] = 0.0
    chroma_delay: Annotated[
        Finite,
        msgspec.Meta(title="Chroma Delay", description="Color-difference offset relative to luma, right above zero"),
        Presentation(soft=Some((-0.02, 0.02)), quantity=Some(Quantity.FRAME_WIDTH)),
    ] = 0.0
    saturation: Annotated[float, msgspec.Meta(ge=0.0, le=sys.float_info.max, title="Saturation", description="Color-difference gain"), Presentation(soft=Some((0.0, 2.0)))] = 1.0
    ringing: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Ringing", description="Second-difference overshoot at luma edges")] = 0.1
    cross_color: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Cross Color", description="Fine luma detail decoded as NTSC subcarrier color")] = 0.0
    noise: Annotated[float, msgspec.Meta(ge=0.0, le=sys.float_info.max, title="Noise", description="Luma noise standard deviation per raster sample"), Presentation(soft=Some((0.0, 0.2)))] = 0.02
    chroma_noise: Annotated[
        float, msgspec.Meta(ge=0.0, le=sys.float_info.max, title="Chroma Noise", description="Color-difference noise standard deviation per raster sample"), Presentation(soft=Some((0.0, 0.2)))
    ] = 0.03
    hum: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Hum", description="Depth of a mains hum bar drifting upward"), Presentation(soft=Some((0.0, 0.5)))] = 0.0
    ghost: Annotated[Gated[Ghost], msgspec.Meta(title="Ghost", description="Multipath echo of the whole signal")] = Gated(enabled=False, value=Ghost())


class Phosphors(msgspec.Struct, frozen=True, kw_only=True):
    """Mean-preserving phosphor mask integrated over each pixel footprint."""

    layout: Annotated[Layout, msgspec.Meta(title="Layout", description="Red, green, and blue phosphor arrangement")] = Layout.APERTURE_GRILLE
    triads: Annotated[int, msgspec.Meta(ge=1, title="Triads", description="Phosphor triads across the frame width"), Presentation(soft=Some((80.0, 1280.0)))] = 640
    strength: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Strength", description="Blend from the plain picture to the mean-one phosphor mask")] = 0.705394


class Scanlines(msgspec.Struct, frozen=True, kw_only=True):
    """Mean-preserving raster lines integrated over each pixel footprint."""

    strength: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Strength", description="Darkening between raster lines before mean normalization")] = 0.4
    interlaced: Annotated[bool, msgspec.Meta(title="Interlaced", description="Light alternate raster fields per pattern")] = False


class Display(msgspec.Struct, frozen=True, kw_only=True):
    """Picture tube showing a collapsed, glowing, scanned, and masked signal."""

    glow: Annotated[
        Gated[
            Annotated[
                float,
                msgspec.Meta(ge=0.0, le=sys.float_info.max, title="Radius", description="Spread of the screen-glass Gaussian blur"),
                Presentation(soft=Some((0.0, 0.05)), quantity=Some(Quantity.FRAME_GREATER)),
            ]
        ],
        msgspec.Meta(title="Glow", description="Linear screen-glass scattering before scanlines and phosphors"),
    ] = Gated(enabled=False, value=0.01)
    scanlines: Annotated[Gated[Scanlines], msgspec.Meta(title="Scanlines", description="Raster lines drawn by the beam")] = Gated(enabled=True, value=Scanlines())
    phosphors: Annotated[Gated[Phosphors], msgspec.Meta(title="Phosphors", description="Phosphor mask on the glass")] = Gated(enabled=False, value=Phosphors())
    curvature: Annotated[float, msgspec.Meta(ge=0.0, le=sys.float_info.max, title="Curvature", description="Barrel bulge of the screen glass"), Presentation(soft=Some((0.0, 0.25)))] = 0.03
    roll: Annotated[Finite, msgspec.Meta(title="Roll", description="Frame heights rolled per clock frame without vertical hold"), Presentation(soft=Some((-0.05, 0.05)))] = 0.0
    collapse: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Collapse", description="Power-off collapse to a line, then a dot")] = 0.0


class Emulation(Effect, frozen=True, kw_only=True, tag=40):
    """Coded, transmitted, taped, received, and displayed video path."""

    stage = Stage.EMULATION
    title = "Emulation"
    pointwise = False
    compression: Annotated[Gated[Compression], msgspec.Meta(title="Compression", description="JPEG picture block coding")] = Gated(enabled=False, value=Compression())
    corruption: Annotated[Gated[Corruption], msgspec.Meta(title="Corruption", description="Digital picture corruption")] = Gated(enabled=False, value=Corruption())
    tape: Annotated[Gated[Tape], msgspec.Meta(title="Tape", description="Videotape playback errors")] = Gated(enabled=False, value=Tape())
    signal: Annotated[Gated[Signal], msgspec.Meta(title="Signal", description="Analog video bandwidth and noise")] = Gated(enabled=True, value=Signal())
    display: Annotated[Gated[Display], msgspec.Meta(title="Display", description="Picture tube showing the signal")] = Gated(enabled=True, value=Display())
    lines: Annotated[int, msgspec.Meta(ge=1, title="Lines", description="Raster lines across the frame height"), Presentation(soft=Some((120.0, 1080.0)))] = 480
    motion: Annotated[Motion, msgspec.Meta(title="Motion", description="Speed and hold of every pattern's clock")] = Motion()
    seed: Annotated[Seed, msgspec.Meta(title="Seed", description="Variant of every random pattern")] = 0


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Compression", "Corruption", "Display", "Emulation", "Ghost", "Layout", "Phosphors", "Scanlines", "Signal", "Subsampling", "Tape"]
