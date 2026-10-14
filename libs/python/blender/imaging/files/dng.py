"""Linear Raw DNG of a scene-linear frame through tifffile."""

import fractions
import pathlib
import struct
from typing import BinaryIO

from expression import Error, Nothing, Ok, Option
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO
import tifffile

from blender.imaging.color.processors import applied, processor, xyz
from blender.imaging.faults import ImagingFaults, ImagingMatrixFault, ImagingResult, ImagingWriteFault, paired
from blender.imaging.files.images import published

# --- [MODELS] ---------------------------------------------------------------------------


class Optics(msgspec.Struct, frozen=True, kw_only=True):
    """Focal length in meters and f-number of the camera a frame came from."""

    focal_length: float
    f_number: float


class LinearRaw(msgspec.Struct, frozen=True, kw_only=True):
    """Camera identity, view exposure, and optics a Linear Raw DNG carries."""

    make: str
    model: str
    exposure: float = 0.0
    optics: Option[Optics] = Nothing

    def written(self, path: str, frame: npt.NDArray[np.float32], config: PyOpenColorIO.Config) -> ImagingResult[str]:
        """Writes a scene-linear frame as Linear Raw DNG.

        Args:
            path: File to write.
            frame: C-contiguous `(height, width, 3)` frame in the config's scene-linear space.
            config: Config whose scene-linear space the frame holds.

        Returns:
            The path; a `processor` or `lut` fault for a failed scene-referred XYZ path, a `matrix` fault for a singular color matrix, or a `write` fault holding the native file or TIFF representation error.
        """

        def signed(*values: float) -> tuple[tuple[int, int], ...]:
            limits = np.iinfo(np.int32)
            fractions_ = (fractions.Fraction(value) for value in values)
            bounded = (
                fraction.limit_denominator(min(limits.max, (limits.max if fraction > 0 else -limits.min) * fraction.denominator // abs(fraction.numerator))) if fraction else fraction
                for fraction in fractions_
            )
            return tuple((fraction.numerator, fraction.denominator) for fraction in bounded)

        def encode(matrix: npt.NDArray[np.float64], stage: str) -> ImagingResult[None]:
            def refused(error: OSError | ValueError | OverflowError | struct.error) -> ImagingFaults:
                return (ImagingWriteFault(request=path, error=error),)

            def into(output: BinaryIO) -> ImagingResult[None]:
                try:
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
                except (OSError, ValueError, OverflowError, struct.error) as error:
                    written = Error(refused(error))
                else:
                    written = Ok(None)
                finally:
                    closed = catch(output.close, exception=OSError)().map_error(refused)
                return paired(written, closed).map(lambda _: None)

            return catch(lambda: pathlib.Path(stage).open("wb"), exception=OSError)().map_error(refused).bind(into)

        def inverted(primaries: npt.NDArray[np.float32]) -> ImagingResult[npt.NDArray[np.float64]]:
            matrix = primaries.T.astype(np.float64)
            return catch(lambda: np.linalg.inv(matrix), exception=np.linalg.LinAlgError)().map_error(lambda error: (ImagingMatrixFault(request=matrix, error=error),))

        primaries = processor(config, xyz(config, PyOpenColorIO.ROLE_SCENE_LINEAR)).map(lambda built: applied(built.getDefaultCPUProcessor(), np.eye(3, dtype=np.float32)))
        return primaries.bind(inverted).bind(lambda matrix: published(path, lambda stage: encode(matrix, stage)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["LinearRaw", "Optics"]
