"""Display formation, colorspace transforms and readings, encoding domains, and RGB geometry."""

from collections.abc import Callable
from enum import auto, Enum
from functools import partial
from typing import Annotated, Final, Self

from expression import Error, identity, Nothing, Ok, Option, Some
from expression.collections import Block
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO
from scipy.linalg import block_diag

from blender.imaging.files.buffers import applied
from blender.imaging.files.images import ImageFile, interop
from blender.records.faults import accumulated, Fault, Outcome, paired, Reason
from blender.records.fields import Name, NonNegative

# --- [TYPES] ----------------------------------------------------------------------------


class Domain(Enum):
    """Encoding a plane holds."""

    SCENE_LINEAR = auto()
    DISPLAY = auto()


type ColorSpace = Name
type Stops = Annotated[float, msgspec.Meta(ge=-float(np.finfo(np.float32).maxexp), le=float(np.finfo(np.float32).maxexp))]

# --- [CONSTANTS] ------------------------------------------------------------------------

GREY: Final = 0.18
REC709_SCENE: Final = "lin_rec709_scene"

# --- [MODELS] ---------------------------------------------------------------------------


class DisplayView(msgspec.Struct, frozen=True, kw_only=True):
    """Scene-linear formation through OCIO's viewing pipeline: exposure, look, display view, and display gamma."""

    display: Name
    view: Name
    looks_override: str = ""
    looks_override_enabled: bool = False
    exposure: Stops = 0.0
    gamma: NonNegative = 1.0

    @classmethod
    def for_encoding(cls, config: PyOpenColorIO.Config, display: str, view: Option[str] = Nothing) -> Outcome[Option[tuple[Self, PyOpenColorIO.Processor]]]:
        """Returns first lookless matrix-only scene view with its processor, absent when none qualifies."""

        def discovered(name: str) -> Outcome[Option[tuple[Self, PyOpenColorIO.Processor]]]:
            candidate = cls(display=display, view=name)
            return (
                Block
                .of_seq(
                    PyOpenColorIO.GroupTransform([source], direction=direction)
                    for native in Option.of_optional(config.getViewTransform(config.getDisplayViewTransformName(display, name))).to_list()
                    if native.getReferenceSpaceType() == PyOpenColorIO.REFERENCE_SPACE_SCENE and not config.getDisplayViewLooks(display, name)
                    for space in candidate.colorspace(config).to_list()
                    if space.getReferenceSpaceType() == PyOpenColorIO.REFERENCE_SPACE_DISPLAY and not space.isData()
                    for side, direction in (
                        (PyOpenColorIO.VIEWTRANSFORM_DIR_FROM_REFERENCE, PyOpenColorIO.TRANSFORM_DIR_FORWARD),
                        (PyOpenColorIO.VIEWTRANSFORM_DIR_TO_REFERENCE, PyOpenColorIO.TRANSFORM_DIR_INVERSE),
                    )
                    for source in Option.of_optional(native.getTransform(side)).to_list()
                )
                .try_head()
                .map(
                    lambda request: paired(
                        processor(config, request).map(lambda built: built.getOptimizedProcessor(PyOpenColorIO.OPTIMIZATION_DEFAULT).createGroupTransform()), candidate.transform(config)
                    ).map(lambda built: Some((candidate, built[1])) if all(isinstance(op, PyOpenColorIO.MatrixTransform) for op in built[0]) else Nothing)
                )
                .default_value(Ok(Nothing))
            )

        return accumulated(map(discovered, view.map(lambda name: (name,)).default_with(lambda: tuple(config.getViews(display))))).map(
            lambda candidates: Block.of_seq(candidates).choose(identity).try_head()
        )

    @classmethod
    def encoding(cls, config: PyOpenColorIO.Config, display: str) -> Outcome[str]:
        """Returns the colorspace name of a display's certified baseline encoding, the first lookless matrix-only scene view in native view order."""
        found: Outcome[Option[tuple[DisplayView, PyOpenColorIO.Processor]]] = cls.for_encoding(config, display)
        return found.bind(
            lambda choice: (
                choice
                .bind(lambda pair: pair[0].colorspace(config))
                .map(PyOpenColorIO.ColorSpace.getName)
                .to_result_with(lambda: Fault.of(Reason.ABSENT, config.getName() or repr(config), member=f"display encoding {display}"))
            )
        )

    def colorspace(self, config: PyOpenColorIO.Config) -> Option[PyOpenColorIO.ColorSpace]:
        """Returns view colorspace, resolving shared view display-name token."""
        name = config.getDisplayViewColorSpaceName(self.display, self.view)
        return Option.of_optional(config.getColorSpace(self.display if name == PyOpenColorIO.OCIO_VIEW_USE_DISPLAY_NAME else name))

    def transform(self, config: PyOpenColorIO.Config, source: str = PyOpenColorIO.ROLE_SCENE_LINEAR) -> Outcome[PyOpenColorIO.Processor]:
        """Returns the viewing pipeline processor from `source`, or color fault."""
        built = PyOpenColorIO.LegacyViewingPipeline()
        built.setDisplayViewTransform(PyOpenColorIO.DisplayViewTransform(src=source, display=self.display, view=self.view))
        built.setLooksOverrideEnabled(self.looks_override_enabled)
        built.setLooksOverride(self.looks_override)
        if self.exposure:
            built.setLinearCC(PyOpenColorIO.MatrixTransform.Scale([2.0**self.exposure] * 3 + [1.0]))
        if self.gamma != 1.0:
            built.setDisplayCC(PyOpenColorIO.ExponentTransform([1.0 / max(self.gamma, float(np.spacing(np.float32(1))))] * 3 + [1.0]))
        return called(built.getProcessor, repr(self), config)

    def form(self, config: PyOpenColorIO.Config, values: npt.NDArray[np.float32], source: str = PyOpenColorIO.ROLE_SCENE_LINEAR) -> Outcome[npt.NDArray[np.float32]]:
        """Returns associated display colors of associated `values` in `source`."""
        return self.transform(config, source).map(lambda built: applied(built.getDefaultCPUProcessor(), values))

    def formed(self, config: PyOpenColorIO.Config, values: npt.NDArray[np.float32], source: str = PyOpenColorIO.ROLE_SCENE_LINEAR) -> Outcome[ImageFile]:
        """Returns associated `values` in `source` formed into an image tagged with the view colorspace."""
        return paired(
            self.form(config, values, source), self.colorspace(config).to_result_with(lambda: Fault.of(Reason.ABSENT, config.getName() or repr(config), member=f"{self.display}/{self.view}"))
        ).map(lambda found: ImageFile(pixels=found[0], colorspace=found[1].getName()))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [TRANSFORMS]


def bracketed(space: str, operation: PyOpenColorIO.Transform) -> PyOpenColorIO.GroupTransform:
    """Returns `operation` run in `space` between scene-linear conversions."""
    return PyOpenColorIO.GroupTransform([
        PyOpenColorIO.ColorSpaceTransform(src=PyOpenColorIO.ROLE_SCENE_LINEAR, dst=space),
        operation,
        PyOpenColorIO.ColorSpaceTransform(src=space, dst=PyOpenColorIO.ROLE_SCENE_LINEAR),
    ])


def xyz(config: PyOpenColorIO.Config, space: str) -> PyOpenColorIO.Transform:
    """Returns `space` conversion to CIE XYZ D65 through interchange roles."""
    display = Option.of_optional(config.getColorSpace(space)).filter(lambda found: found.getReferenceSpaceType() == PyOpenColorIO.REFERENCE_SPACE_DISPLAY).is_some()
    return (
        PyOpenColorIO.ColorSpaceTransform(src=space, dst=PyOpenColorIO.ROLE_INTERCHANGE_DISPLAY)
        if display
        else PyOpenColorIO.GroupTransform([
            PyOpenColorIO.ColorSpaceTransform(src=space, dst=PyOpenColorIO.ROLE_INTERCHANGE_SCENE),
            PyOpenColorIO.BuiltinTransform(style="UTILITY - ACES-AP0_to_CIE-XYZ-D65_BFD"),
        ])
    )


def matrix(values: npt.NDArray[np.float64]) -> PyOpenColorIO.MatrixTransform:
    """Returns OCIO transform of 3 by 3 color matrix, alpha passed through."""
    return PyOpenColorIO.MatrixTransform(matrix=block_diag(values, [1.0]).ravel().tolist())


def lin_to_log() -> PyOpenColorIO.LogCameraTransform:
    """Returns OCIO's GRADING_LIN stop axis as a forward log camera transform."""
    shift = -0.000157849851665374
    slope = 1.0 / (GREY + shift)
    return PyOpenColorIO.LogCameraTransform(
        linSideBreak=[0.0041318374739483946] * 3,
        base=2.0,
        logSideSlope=[1.0] * 3,
        logSideOffset=[0.0] * 3,
        linSideSlope=[slope] * 3,
        linSideOffset=[shift * slope] * 3,
        linearSlope=[363.034608563] * 3,
    )


# --- [BUILDS]


def called[**P, T](call: Callable[P, T], subject: str, *args: P.args, **kwargs: P.kwargs) -> Outcome[T]:
    """Returns the value of one OCIO call, or color fault naming `subject` for either OCIO exception class."""
    return catch(catch(call, exception=PyOpenColorIO.Exception), exception=PyOpenColorIO.ExceptionMissingFile)(*args, **kwargs).map_error(  # type: ignore[return-value]  # ty: ignore[invalid-return-type]
        partial(Fault.of, Reason.FAILED, subject)
    )


def processor(config: PyOpenColorIO.Config, transform: PyOpenColorIO.Transform) -> Outcome[PyOpenColorIO.Processor]:
    """Returns processor, or color fault."""
    return called(config.getProcessor, type(transform).__name__, transform)


def converted(config: PyOpenColorIO.Config, transform: PyOpenColorIO.Transform, values: npt.NDArray[np.float32]) -> Outcome[npt.NDArray[np.float32]]:
    """Applies `transform` to associated colors through its CPU processor, or color fault."""
    return processor(config, transform).map(lambda built: applied(built.getDefaultCPUProcessor(), values))


# --- [READINGS]


def basis(config: PyOpenColorIO.Config, space: str) -> Outcome[npt.NDArray[np.float64]]:
    """Returns linear RGB-to-XYZ D65 matrix of `space`, or base fault."""

    def held(group: PyOpenColorIO.GroupTransform) -> Outcome[npt.NDArray[np.float64]]:
        match tuple(group):
            case ():
                return Ok(np.eye(3))
            case (PyOpenColorIO.MatrixTransform() as op,) if not any(op.getOffset()) and np.isfinite(values := np.asarray(op.getMatrix(), dtype=np.float64).reshape(4, 4)[:3, :3]).all():
                return Ok(values)
            case _:
                return Error(Fault.of(Reason.INVALID, space, f"transform {group} lacks linear RGB geometry"))

    return processor(config, xyz(config, space)).map(lambda built: built.getOptimizedProcessor(PyOpenColorIO.OPTIMIZATION_DEFAULT).createGroupTransform()).bind(held)


def rendering_basis(config: PyOpenColorIO.Config, space: str) -> Outcome[npt.NDArray[np.float64]]:
    """Returns the RGB-to-XYZ basis of `space` when its primaries hold positive luminance and one white sign with an invertible white-normalized matrix, else base fault."""

    @np.errstate(all="ignore")
    def held(values: npt.NDArray[np.float64]) -> Outcome[npt.NDArray[np.float64]]:
        sums = values.sum(axis=0)
        normalized = values / sums
        signed = (values[1] > 0.0).all() and ((sums > 0.0).all() or (sums < 0.0).all()) and np.isfinite(sums.sum()) and np.isfinite(values.sum(axis=1)).all()
        return (
            Ok(values)
            if signed and np.isfinite(normalized).all() and abs(np.linalg.slogdet(normalized).sign) == 1.0
            else Error(Fault.of(Reason.INVALID, space, f"transform {matrix(values)} lacks linear RGB geometry"))
        )

    return basis(config, space).bind(held)


def luma(config: PyOpenColorIO.Config) -> Outcome[npt.NDArray[np.float64]]:
    """Returns scene-linear luminance coefficients, the Y row of its RGB-to-XYZ matrix, or base fault."""
    return basis(config, PyOpenColorIO.ROLE_SCENE_LINEAR).map(lambda matrix: matrix[1])


def hdr(space: PyOpenColorIO.ColorSpace) -> bool:
    """Whether a display colorspace's OCIO encoding is hdr-video or edr-video."""
    return space.getEncoding() in {"hdr-video", "edr-video"}


def pq(space: PyOpenColorIO.ColorSpace) -> bool:
    """Whether a display colorspace's interop ID names absolute PQ luminance."""
    return interop(space).rpartition(":")[2].startswith("pq_")


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "GREY",
    "REC709_SCENE",
    "ColorSpace",
    "DisplayView",
    "Domain",
    "Stops",
    "basis",
    "bracketed",
    "called",
    "converted",
    "hdr",
    "lin_to_log",
    "luma",
    "matrix",
    "pq",
    "processor",
    "rendering_basis",
    "xyz",
]
