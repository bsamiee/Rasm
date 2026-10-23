"""Closed-form projective maps between convex image quads and unit plane."""

from fractions import Fraction
import math
from typing import Self

from expression import Nothing, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt

from blender.imaging.parameters import Position

# --- [MODELS] ---------------------------------------------------------------------------


class Quad(msgspec.Struct, frozen=True, kw_only=True):
    """Image quad corners."""

    top_left: Position = (0.0, 1.0)
    top_right: Position = (1.0, 1.0)
    bottom_right: Position = (1.0, 0.0)
    bottom_left: Position = (0.0, 0.0)

    @classmethod
    def of(cls, samples: npt.NDArray[np.float64]) -> Self:
        """Reads corners from first two sample components."""
        return cls(**{name: (float(samples[index, 0]), float(samples[index, 1])) for index, name in enumerate(cls.__struct_fields__)})

    @property
    def samples(self) -> npt.NDArray[np.float64]:
        """Corner positions with unit coverage in declaration order."""
        return np.array([(*corner, 1.0) for corner in msgspec.structs.astuple(self)])


class Homography(msgspec.Struct, frozen=True, kw_only=True):
    """Projective map between strictly convex quad and unit plane."""

    quad: Quad

    @classmethod
    def of(cls, quad: Quad) -> Option[Self]:
        """Returns map of convex quad."""
        corners = tuple(tuple(Fraction(value) for value in corner) for corner in msgspec.structs.astuple(quad))
        edges = tuple((following[0] - corner[0], following[1] - corner[1]) for corner, following in zip(corners, corners[1:] + corners[:1]))
        turns = tuple(edge[0] * following[1] - edge[1] * following[0] for edge, following in zip(edges, edges[1:] + edges[:1]))
        return Some(cls(quad=quad)) if all(turn > 0 for turn in turns) or all(turn < 0 for turn in turns) else Nothing

    @property
    def matrix(self) -> tuple[tuple[Fraction, ...], ...]:
        """Exact square-to-quad coefficients with positive near-side homogeneous scale."""
        (x0, y0), (x1, y1), (x2, y2), (x3, y3) = ((Fraction(x), Fraction(y)) for x, y in (self.quad.bottom_left, self.quad.bottom_right, self.quad.top_right, self.quad.top_left))
        sx, sy = x0 - x1 + x2 - x3, y0 - y1 + y2 - y3
        turn = (x1 - x2) * (y3 - y2) - (x3 - x2) * (y1 - y2)
        sign = 1 if turn > 0 else -1
        g = (sx * (y3 - y2) - (x3 - x2) * sy) * sign
        h = ((x1 - x2) * sy - sx * (y1 - y2)) * sign
        i = abs(turn)
        return (((x1 - x0) * i + g * x1, (x3 - x0) * i + h * x3, x0 * i), ((y1 - y0) * i + g * y1, (y3 - y0) * i + h * y3, y0 * i), (g, h, i))

    def to_image(self, samples: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        """Maps plane samples to image, zero coverage beyond vanishing line."""
        return projected(self.matrix, samples)

    def to_plane(self, samples: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        """Maps image samples to plane, zero coverage beyond vanishing line."""
        matrix = self.matrix
        cofactors = tuple(
            tuple(
                matrix[(row + 1) % 3][(column + 1) % 3] * matrix[(row + 2) % 3][(column + 2) % 3] - matrix[(row + 1) % 3][(column + 2) % 3] * matrix[(row + 2) % 3][(column + 1) % 3]
                for column in range(3)
            )
            for row in range(3)
        )
        determinant = sum((value * cofactor for value, cofactor in zip(matrix[0], cofactors[0])), start=Fraction())
        sign = 1 if determinant > 0 else -1
        inverse = tuple(tuple(cofactors[column][row] * sign for column in range(3)) for row in range(3))
        return projected(inverse, samples)


# --- [OPERATIONS] -----------------------------------------------------------------------


def projected(matrix: tuple[tuple[Fraction, ...], ...], samples: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
    """Projects samples in exact arithmetic rounded once, covering representable near-side positions alone."""

    def point(x: float, y: float, coverage: float) -> tuple[float, float, float]:
        if not (math.isfinite(x) and math.isfinite(y)):
            return 0.0, 0.0, 0.0
        horizontal, vertical = Fraction(x), Fraction(y)
        a, b, w = (row[0] * horizontal + row[1] * vertical + row[2] for row in matrix)
        if w <= 0:
            return 0.0, 0.0, 0.0
        try:
            return float(a / w), float(b / w), coverage
        except OverflowError:
            return 0.0, 0.0, 0.0

    return np.stack(np.vectorize(point, otypes=[np.float64, np.float64, np.float64])(*np.unstack(samples, axis=-1)), axis=-1, dtype=np.float64)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Homography", "Quad", "projected"]
