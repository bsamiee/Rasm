"""File-backed grades and native processor samples for host lookup tables."""

from typing import Annotated, override

from expression import Nothing, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from rasm.imaging.chain import Effect, Stage
from rasm.imaging.color.luts import Lattice
from rasm.imaging.color.processors import applied, processor
from rasm.imaging.faults import ImagingResult
from rasm.imaging.parameters import ColorSpace, Gated, GraphForm

# --- [MODELS] ---------------------------------------------------------------------------


class Lookup(Effect, frozen=True, kw_only=True, tag=18):
    """LUT file applied in its declared color space."""

    stage = Stage.LOOKUP
    title = "LUT"
    pointwise = True
    source: Annotated[str, msgspec.Meta(min_length=1, title="File", description="LUT file in a format OpenColorIO reads"), GraphForm.IMAGE]
    space: Annotated[ColorSpace, msgspec.Meta(title="Color Space", description="Color space the LUT reads and writes")] = PyOpenColorIO.ROLE_COLOR_PICKING
    prefix: Annotated[Gated[Lattice], msgspec.Meta(title="Prefix Sampling", description="Authored domain and resolution for hosts sampling operations before the first 3D table")] = Gated(
        enabled=False, value=Lattice()
    )

    @property
    def file(self) -> PyOpenColorIO.FileTransform:
        """Native file operation with its best supported interpolation."""
        return PyOpenColorIO.FileTransform(src=self.source, interpolation=PyOpenColorIO.INTERP_BEST)

    @override
    def transform(self, config: PyOpenColorIO.Config) -> ImagingResult[Option[PyOpenColorIO.Transform]]:
        return Ok(Some(self.file))

    def tile(self, config: PyOpenColorIO.Config) -> ImagingResult[tuple[PyOpenColorIO.GroupTransform, Option[tuple[Lattice, npt.NDArray[np.float32]]]]]:
        """Returns native prefix operations and the first 3D table's sampled suffix, when present."""

        def sampled(built: PyOpenColorIO.Processor) -> ImagingResult[tuple[PyOpenColorIO.GroupTransform, Option[tuple[Lattice, npt.NDArray[np.float32]]]]]:
            operations = list(built.createGroupTransform())
            table = next((Some((index, Lattice(size=operation.getGridSize()))) for index, operation in enumerate(operations) if isinstance(operation, PyOpenColorIO.Lut3DTransform)), Nothing)
            match table:
                case Option(tag="some", some=(split, lattice)):
                    head = PyOpenColorIO.GroupTransform(transforms=operations[:split])
                    suffix = PyOpenColorIO.GroupTransform(transforms=operations[split:])
                    return processor(PyOpenColorIO.Config(), suffix).bind(
                        lambda resolved: lattice.samples().map(lambda samples: (head, Some((lattice, applied(resolved.getDefaultCPUProcessor(), samples)))))
                    )
                case Option():
                    return Ok((PyOpenColorIO.GroupTransform(transforms=operations), Nothing))

        return processor(config, self.file).bind(sampled)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Lookup"]
