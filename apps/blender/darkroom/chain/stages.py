"""Darkroom chain positions: the stage each effect processes at, the phase a run of stages forms, and the effect record a chain entry holds."""

from abc import ABCMeta, abstractmethod
from enum import auto, IntEnum
from typing import ClassVar

from expression import Nothing, Ok, Option
import PyOpenColorIO

from blender.imaging.faults import ImagingResult
from blender.records.fields import Case, CaseMeta

# --- [TYPES] ----------------------------------------------------------------------------


class EffectMeta(CaseMeta, ABCMeta):
    """Metaclass of effect records with abstract members."""


class Phase(IntEnum):
    """Run of consecutive stages, in processing order."""

    OPTICS = 0
    GRADE = auto()
    FORMATION = auto()
    FINISH = auto()


class Stage(IntEnum):
    """Processing position ranked by declaration order, `single` holding the stages a chain keeps one entry of."""

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
    single: ClassVar[frozenset["Stage"]]

    @property
    def phase(self) -> Phase:
        """Optics before exposure, grade before tone, formation through peak, finish after."""
        return Phase.OPTICS if self < Stage.EXPOSURE else Phase.GRADE if self < Stage.TONE else Phase.FORMATION if self <= Stage.PEAK else Phase.FINISH


Stage.single = frozenset({Stage.CDL, Stage.TONE, Stage.FORMATION, Stage.PEAK, Stage.QUANTIZATION})

# --- [MODELS] ---------------------------------------------------------------------------


class Effect(Case, metaclass=EffectMeta, frozen=True, kw_only=True):
    """Chain record with its processing stage, pixel locality, and the OCIO operation it has where one exists."""

    stage: ClassVar[Stage]

    @property
    @abstractmethod
    def pointwise(self) -> bool:
        """Whether each output pixel reads its input pixel alone."""
        ...

    def transform(self, _config: PyOpenColorIO.Config, /) -> ImagingResult[Option[PyOpenColorIO.Transform]]:
        """Returns the OCIO operation of the effect, Nothing for an effect with none."""
        return Ok(Nothing)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Effect", "EffectMeta", "Phase", "Stage"]
