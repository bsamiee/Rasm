"""Unclamped channel mixing and color-space interpretation through OCIO."""

from typing import Annotated, override, Self

from expression import Ok, Option, Some
import msgspec
import numpy as np
import PyOpenColorIO

from blender.imaging.chain import Effect, Stage
from blender.imaging.color.processors import matrix
from blender.imaging.faults import ImagingResult
from blender.imaging.parameters import Case, ColorSpace, Finite, Presentation, Rgb

# --- [TYPES] ----------------------------------------------------------------------------

type Weight = Annotated[Finite, Presentation(soft=Some((-2.0, 2.0)), precision=Some(3))]
type Remap = Mixer | Conversion

# --- [MODELS] ---------------------------------------------------------------------------


class Weights(msgspec.Struct, frozen=True, kw_only=True):
    """Input-channel weights summed by one output channel."""

    red: Annotated[Weight, msgspec.Meta(description="Red input weight")]
    green: Annotated[Weight, msgspec.Meta(description="Green input weight")]
    blue: Annotated[Weight, msgspec.Meta(description="Blue input weight")]


class Mixer(Case, frozen=True, kw_only=True, tag=1):
    """Linear channel mixing with signed weights and unclamped output."""

    red: Annotated[Weights, msgspec.Meta(description="Weights summing into output red")] = Weights(red=1.0, green=0.0, blue=0.0)
    green: Annotated[Weights, msgspec.Meta(description="Weights summing into output green")] = Weights(red=0.0, green=1.0, blue=0.0)
    blue: Annotated[Weights, msgspec.Meta(description="Weights summing into output blue")] = Weights(red=0.0, green=0.0, blue=1.0)

    @classmethod
    def of(cls, rows: tuple[Rgb, Rgb, Rgb]) -> Self:
        """Constructs output-channel weights from RGB rows."""
        return cls(**{name: Weights(**msgspec.structs.asdict(row)) for name, row in zip(cls.__struct_fields__, rows)})

    @property
    def transform(self) -> PyOpenColorIO.MatrixTransform:
        """Fresh matrix transform keeping alpha."""
        return matrix(np.array(msgspec.structs.astuple(self), dtype=np.float64))


class Conversion(Case, frozen=True, kw_only=True, tag=2):
    """Colorspace interpretation and conversion through OCIO config."""

    source: Annotated[ColorSpace, msgspec.Meta(description="Colorspace values read as")] = PyOpenColorIO.ROLE_SCENE_LINEAR
    target: Annotated[ColorSpace, msgspec.Meta(description="Colorspace values convert to")] = PyOpenColorIO.ROLE_SCENE_LINEAR

    @property
    def transform(self) -> PyOpenColorIO.ColorSpaceTransform:
        """Fresh conversion from source to target."""
        return PyOpenColorIO.ColorSpaceTransform(src=self.source, dst=self.target)


class Primaries(Effect, frozen=True, kw_only=True, tag=9):
    """Remaps scene-linear channels before gamut compression."""

    stage = Stage.PRIMARIES
    pointwise = True
    remap: Annotated[Remap, msgspec.Meta(description="Channel mixing or colorspace interpretation")] = Mixer()

    @override
    def transform(self, config: PyOpenColorIO.Config) -> ImagingResult[Option[PyOpenColorIO.Transform]]:
        """Returns remap OCIO operation."""
        return Ok(Some(self.remap.transform))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Conversion", "Mixer", "Primaries", "Remap", "Weight", "Weights"]
