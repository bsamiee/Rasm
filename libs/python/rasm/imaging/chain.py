"""Image domains, ordered processing stages, and effect records."""

from abc import ABCMeta, abstractmethod
from enum import auto, Enum, IntEnum
from typing import ClassVar, Final

from expression import Nothing, Ok, Option
import msgspec
import PyOpenColorIO

from rasm.imaging.faults import ImagingResult
from rasm.imaging.parameters import Case

# --- [TYPES] ----------------------------------------------------------------------------


class EffectMeta(msgspec.StructMeta, ABCMeta):
    """Native frozen records with abstract formulation requirements."""


class Domain(Enum):
    """Encoding a processing stage reads."""

    SCENE_LINEAR = auto()
    DISPLAY = auto()


class Stage(IntEnum):
    """Processing position ranked by declaration order."""

    DENOISE = 0
    ATMOSPHERE = auto()
    DEFOCUS = auto()
    GENERATE = auto()
    WARP = auto()
    DISTORTION = auto()
    SHARPEN = auto()
    SCATTER = auto()
    VIGNETTE = auto()
    HALATION = auto()
    EXPOSURE = auto()
    GRAIN = auto()
    WHITE_BALANCE = auto()
    PRIMARIES = auto()
    GAMUT = auto()
    LEVELS = auto()
    ZONES = auto()
    CDL = auto()
    PRIMARY = auto()
    CONTRAST = auto()
    SATURATION = auto()
    CURVES = auto()
    HUE_CURVES = auto()
    LOOKUP = auto()
    LOCAL_EXPOSURE = auto()
    TONE = auto()
    FORMATION = auto()
    PEAK = auto()
    PIXELATE = auto()
    PALETTE = auto()
    PATTERN = auto()
    EDGES = auto()
    DAMAGE = auto()
    EMULATION = auto()
    QUANTIZATION = auto()

    @property
    def domain(self) -> Domain:
        """Encoding read before or after formation."""
        return Domain.SCENE_LINEAR if self <= Stage.FORMATION else Domain.DISPLAY

    @property
    def single(self) -> bool:
        """Whether the stage holds at most one chain entry."""
        return self in {Stage.CDL, Stage.TONE, Stage.FORMATION, Stage.PEAK, Stage.QUANTIZATION}


# --- [CONSTANTS] ------------------------------------------------------------------------

GREY: Final = 0.18

# --- [MODELS] ---------------------------------------------------------------------------


class Effect(Case, metaclass=EffectMeta, frozen=True, kw_only=True):
    """Formulation with a processing stage and pixel locality."""

    stage: ClassVar[Stage]

    @property
    @abstractmethod
    def pointwise(self) -> bool:
        """Whether this record answers each pixel independently of its frame."""
        ...

    def transform(self, config: PyOpenColorIO.Config) -> ImagingResult[Option[PyOpenColorIO.Transform]]:
        """Returns an optional OCIO operation or typed failure."""
        return Ok(Nothing)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["GREY", "Domain", "Effect", "Stage"]
