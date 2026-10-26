"""Linear Raw DNG of a scene-linear frame through tifffile."""

import fractions
import functools
from typing import IO

from expression import Nothing, Option
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO
import tifffile

from blender.imaging.color.processors import basis
from blender.imaging.faults import ImagingResult
from blender.imaging.files.images import written
from blender.records.faults import NumericFault

# --- [MODELS] ---------------------------------------------------------------------------


class Optics(msgspec.Struct, frozen=True, kw_only=True):
    """Source camera focal length in meters and f-number."""

    focal_length: float
    f_number: float


class LinearRaw(msgspec.Struct, frozen=True, kw_only=True):
    """Camera and exposure tags of Linear Raw DNG."""

    make: str
    model: str
    exposure: float = 0.0
    optics: Option[Optics] = Nothing

    def written(self, path: str, frame: npt.NDArray[np.float32], config: PyOpenColorIO.Config) -> ImagingResult[str]:
        """Writes scene-linear frame as Linear Raw DNG.

        Args:
            path: File to write.
            frame: C-contiguous `(height, width, 3)` scene-linear frame.
            config: Config defining scene-linear space.

        Returns:
            Path, or `color`, `numeric`, or `write` fault.
        """

        def signed(*values: float) -> tuple[tuple[int, int], ...]:
            limits = np.iinfo(np.int32)
            fractions_ = (fractions.Fraction(value) for value in values)
            bounded = (
                fraction.limit_denominator(min(limits.max, (limits.max if fraction > 0 else -limits.min) * fraction.denominator // abs(fraction.numerator))) if fraction else fraction
                for fraction in fractions_
            )
            return tuple(fraction.as_integer_ratio() for fraction in bounded)

        def tiff(matrix: npt.NDArray[np.float64], output: IO[bytes]) -> None:
            rows = (
                ("Make", tifffile.DATATYPE.ASCII, 0, self.make),
                ("Model", tifffile.DATATYPE.ASCII, 0, self.model),
                ("UniqueCameraModel", tifffile.DATATYPE.ASCII, 0, f"{self.make} {self.model}"),
                ("DNGVersion", tifffile.DATATYPE.BYTE, 4, (1, 4, 0, 0)),
                ("DNGBackwardVersion", tifffile.DATATYPE.BYTE, 4, (1, 4, 0, 0)),
                ("ColorMatrix1", tifffile.DATATYPE.SRATIONAL, 9, signed(*matrix.reshape(-1).tolist())),
                ("CalibrationIlluminant1", tifffile.DATATYPE.SHORT, 1, 21),
                ("AsShotNeutral", tifffile.DATATYPE.RATIONAL, 3, (tifffile.rational(1.0),) * 3),
                ("BaselineExposure", tifffile.DATATYPE.SRATIONAL, 1, signed(self.exposure)),
                *(
                    row
                    for optics in self.optics.to_list()
                    for row in (
                        ("FocalLength", tifffile.DATATYPE.RATIONAL, 1, tifffile.rational(optics.focal_length * 1000.0)),
                        ("FNumber", tifffile.DATATYPE.RATIONAL, 1, tifffile.rational(optics.f_number)),
                    )
                ),
            )
            tifffile.imwrite(
                output,
                frame,
                photometric=tifffile.PHOTOMETRIC.LINEAR_RAW,
                planarconfig=tifffile.PLANARCONFIG.CONTIG,
                subfiletype=tifffile.FILETYPE.UNDEFINED,
                metadata=None,
                software=False,
                bigtiff=False,
                extratags=[(tifffile.TIFF.TAGS[name], kind, count, value, True) for name, kind, count, value in rows],
            )

        def inverted(matrix: npt.NDArray[np.float64]) -> ImagingResult[npt.NDArray[np.float64]]:
            return catch(lambda: np.linalg.inv(matrix), exception=np.linalg.LinAlgError)().map_error(lambda error: (NumericFault(request=matrix, error=error),))

        return basis(config, PyOpenColorIO.ROLE_SCENE_LINEAR).bind(inverted).bind(lambda matrix: written(path, functools.partial(tiff, matrix)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["LinearRaw", "Optics"]
