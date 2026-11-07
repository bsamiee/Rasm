"""Elementwise protocols under Array API names, numpy serving as a namespace, and periodic, normal, and Hermite maps over them."""

import math
from typing import Literal, Protocol

import numpy as np
from optype import CanAddSame, CanModSame, CanMulSame, CanRAddSelf, CanRMulSelf, CanRSubSelf, CanRTruedivSelf, CanSubSame, CanTruedivSame

# --- [TYPES] ----------------------------------------------------------------------------

type WaveProfile = Literal["SIN", "SAW", "TRI"]


class Arithmetic(CanAddSame[float], CanRSubSelf[float], CanMulSame[float], Protocol):
    """Value supporting sums, products, and subtraction from a float."""


class Elementwise(Arithmetic, CanRAddSelf[float], CanSubSame[float], CanRMulSelf[float], CanTruedivSame[float], CanRTruedivSelf[float], CanModSame[float], Protocol):
    """Value supporting elementwise arithmetic with itself and floats."""


class Namespace[V: Elementwise, B](Protocol):
    """Elementwise functions over values `V` with conditions `B`, the Array API subset maps read beside the operators of `Elementwise`."""

    def negative(self, x: V, /) -> V:
        """Negation."""

    def abs(self, x: V, /) -> V:
        """Magnitude."""

    def sqrt(self, x: V, /) -> V:
        """Square root."""

    def sin(self, x: V, /) -> V:
        """Sine of radians."""

    def cos(self, x: V, /) -> V:
        """Cosine of radians."""

    def asin(self, x: V, /) -> V:
        """Inverse sine in radians."""

    def atan2(self, x1: V, x2: V, /) -> V:
        """Quadrant-aware inverse tangent of `x1 / x2`."""

    def hypot(self, x1: V, x2: V, /) -> V:
        """Length of the vector of `x1` and `x2`."""

    def log(self, x: V, /) -> V:
        """Natural logarithm."""

    def exp2(self, x: V, /) -> V:
        """Two raised to the power."""

    def floor(self, x: V, /) -> V:
        """Greatest integer at or below."""

    def minimum(self, x1: V, x2: V | float, /) -> V:
        """Lesser value."""

    def maximum(self, x1: V, x2: V | float, /) -> V:
        """Greater value."""

    def less(self, x1: V, x2: V | float, /) -> B:
        """Whether `x1` lies below `x2`."""

    def greater(self, x1: V, x2: V | float, /) -> B:
        """Whether `x1` lies above `x2`."""

    def less_equal(self, x1: V, x2: V | float, /) -> B:
        """Whether `x1` lies at or below `x2`."""

    def greater_equal(self, x1: V, x2: V | float, /) -> B:
        """Whether `x1` lies at or above `x2`."""

    def logical_and(self, x1: B, x2: B, /) -> B:
        """Both conditions."""

    def logical_or(self, x1: B, x2: B, /) -> B:
        """Either condition."""

    def where(self, condition: B, x1: V | float, x2: V | float, /) -> V:
        """`x1` where `condition` holds, else `x2`."""


# --- [OPERATIONS] -----------------------------------------------------------------------


def periodic[V: Elementwise, B](profile: WaveProfile, cycles: V, ns: Namespace[V, B]) -> V:
    """Returns the Wave Texture profile in [0, 1] at a cycle coordinate, one period per unit, its identifiers those of `ShaderNodeTexWave.wave_profile`."""
    match profile:
        case "SIN":
            return 0.5 - ns.cos(cycles * math.tau) * 0.5
        case "SAW":
            return cycles - ns.floor(cycles)
        case "TRI":
            return ns.abs(cycles - ns.floor(cycles + 0.5)) * 2.0


def gaussian[V: Elementwise, B](radius: V, angle: V, ns: Namespace[V, B]) -> tuple[V, V]:
    """Returns the cosine and sine Box-Muller normals of two distinct uniform draws (two White Noise values).

    Args:
        radius: Uniform draw setting the shared radius, kept off zero by float32 epsilon.
        angle: Uniform draw setting the shared phase.
        ns: Namespace evaluating the draws, `numpy` or the node namespace.

    Returns:
        Normals over one radius and phase.
    """
    epsilon = float(np.finfo(np.float32).eps)
    spread = ns.sqrt(ns.log((radius + epsilon) / (1.0 + 2.0 * epsilon)) * -2.0)
    phase = angle * math.tau
    return spread * ns.cos(phase), spread * ns.sin(phase)


def smoothstep[V: Elementwise](values: V) -> V:
    """Returns cubic Hermite ease of unit-interval values."""
    return values * values * (3.0 - values * 2.0)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Arithmetic", "Elementwise", "Namespace", "WaveProfile", "gaussian", "periodic", "smoothstep"]
