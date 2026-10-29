"""Darkroom chain positions: the stage each effect processes at, the phase a run of stages forms, and the effect record a chain entry holds."""

from collections.abc import Mapping
from enum import auto, IntEnum
from functools import singledispatch
from typing import ClassVar

import bpy
from expression import Nothing, Ok, Option
import OpenImageIO
import PyOpenColorIO

from blender.bpy.data.rna.descriptors import Value
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.render.compositor.inputs import CameraFrame
from blender.imaging.color.processors import Domain
from blender.imaging.sampling.integration import Estimate, Precision
from blender.records.faults import Outcome
from blender.records.fields import Case

# --- [TYPES] ----------------------------------------------------------------------------


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


class Effect(Case, frozen=True, kw_only=True):
    """Chain record with its processing stage, whether each output pixel reads its input pixel alone, and the encoding its operation runs in."""

    stage: ClassVar[Stage]
    pointwise: ClassVar[bool]
    domain: ClassVar[Domain] = Domain.SCENE_LINEAR


# --- [OPERATIONS] -----------------------------------------------------------------------


@singledispatch
def transform(_record: Effect, _values: Mapping[Key, Value], _config: PyOpenColorIO.Config, /) -> Outcome[Option[PyOpenColorIO.Transform]]:
    """Returns the OCIO operation of an effect over its entry input values by item key, Nothing for an effect with none."""
    return Ok(Nothing)


@singledispatch
def prepared(
    _record: Effect,
    _identity: Identity,
    _values: Mapping[Key, Value],
    _config: PyOpenColorIO.Config,
    _image: OpenImageIO.ImageBuf,
    _precision: Precision,
    _frame: CameraFrame,
    _scene: bpy.types.Scene,
    /,
) -> Outcome[Option[OpenImageIO.ImageBuf | Estimate[OpenImageIO.ImageBuf] | Mapping[Key, Value]]]:
    """Returns the CPU image replacing the entering color of an effect entry, or input values by item key measured from it, Nothing for an effect its node graph alone evaluates."""
    return Ok(Nothing)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Effect", "Phase", "Stage", "prepared", "transform"]
