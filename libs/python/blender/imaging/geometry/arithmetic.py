"""Elementwise arithmetic one map is written over, spelled with the Array API names so the numpy module serves as a namespace."""

from typing import Protocol, Self

# --- [TYPES] ----------------------------------------------------------------------------


class Field(Protocol):
    """Value supporting elementwise arithmetic with itself and floats."""

    def __add__(self, other: Self | float, /) -> Self:
        """Elementwise sum."""
        ...

    def __radd__(self, other: float, /) -> Self:
        """Elementwise sum with a leading float."""
        ...

    def __sub__(self, other: Self | float, /) -> Self:
        """Elementwise difference."""
        ...

    def __rsub__(self, other: float, /) -> Self:
        """Elementwise difference from a leading float."""
        ...

    def __mul__(self, other: Self | float, /) -> Self:
        """Elementwise product."""
        ...

    def __rmul__(self, other: float, /) -> Self:
        """Elementwise product with a leading float."""
        ...

    def __truediv__(self, other: Self | float, /) -> Self:
        """Elementwise quotient."""
        ...

    def __rtruediv__(self, other: float, /) -> Self:
        """Elementwise quotient of a leading float."""
        ...


class Namespace[V: Field, B](Protocol):
    """Elementwise functions over values `V` with conditions `B`, the Array API subset maps read."""

    def negative(self, x: V, /) -> V:
        """Negation."""
        ...

    def abs(self, x: V, /) -> V:
        """Magnitude."""
        ...

    def sqrt(self, x: V, /) -> V:
        """Square root."""
        ...

    def sin(self, x: V, /) -> V:
        """Sine of radians."""
        ...

    def cos(self, x: V, /) -> V:
        """Cosine of radians."""
        ...

    def tan(self, x: V, /) -> V:
        """Tangent of radians."""
        ...

    def asin(self, x: V, /) -> V:
        """Inverse sine in radians."""
        ...

    def atan(self, x: V, /) -> V:
        """Inverse tangent in radians."""
        ...

    def atan2(self, x1: V, x2: V, /) -> V:
        """Quadrant-aware inverse tangent of `x1 / x2`."""
        ...

    def hypot(self, x1: V, x2: V, /) -> V:
        """Length of the vector of `x1` and `x2`."""
        ...

    def log(self, x: V, /) -> V:
        """Natural logarithm."""
        ...

    def floor(self, x: V, /) -> V:
        """Greatest integer at or below."""
        ...

    def remainder(self, x1: V, x2: V | float, /) -> V:
        """Remainder taking the sign of `x2`."""
        ...

    def minimum(self, x1: V, x2: V | float, /) -> V:
        """Lesser value."""
        ...

    def maximum(self, x1: V, x2: V | float, /) -> V:
        """Greater value."""
        ...

    def less(self, x1: V, x2: V | float, /) -> B:
        """Whether `x1` lies below `x2`."""
        ...

    def greater(self, x1: V, x2: V | float, /) -> B:
        """Whether `x1` lies above `x2`."""
        ...

    def less_equal(self, x1: V, x2: V | float, /) -> B:
        """Whether `x1` lies at or below `x2`."""
        ...

    def greater_equal(self, x1: V, x2: V | float, /) -> B:
        """Whether `x1` lies at or above `x2`."""
        ...

    def logical_and(self, x1: B, x2: B, /) -> B:
        """Both conditions."""
        ...

    def logical_or(self, x1: B, x2: B, /) -> B:
        """Either condition."""
        ...

    def where(self, condition: B, x1: V | float, x2: V | float, /) -> V:
        """`x1` where `condition` holds, else `x2`."""
        ...


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Field", "Namespace"]
