"""Display formation, colorspace transforms, encoding domains, and RGB geometry."""

from enum import auto, Enum
from functools import reduce
from typing import Annotated, Final, Self

from expression import Error, identity, Nothing, Ok, Option, Some
from expression.collections import Block
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO
from scipy.linalg import block_diag

from blender.imaging.color.observer import coordinates
from blender.imaging.faults import colored, ImagingBaseFault, ImagingResult, ImagingSpectralFault
from blender.imaging.files.images import applied
from blender.records.faults import accumulated, NumericFault, paired
from blender.records.fields import Gated, Presentation

# --- [TYPES] ----------------------------------------------------------------------------


class Domain(Enum):
    """Encoding a plane holds."""

    SCENE_LINEAR = auto()
    DISPLAY = auto()


type Gamma = Annotated[float, msgspec.Meta(ge=0.0, le=5.0)]
type Stops = Annotated[float, msgspec.Meta(ge=-32.0, le=32.0), Presentation(soft=Some((-10.0, 10.0)), step=Some(0.01), precision=Some(3))]
type ColorSpace = Annotated[str, msgspec.Meta(min_length=1)]

# --- [CONSTANTS] ------------------------------------------------------------------------

GREY: Final = 0.18
REC709_SCENE: Final = "lin_rec709_scene"

# --- [MODELS] ---------------------------------------------------------------------------


class DisplayView(msgspec.Struct, frozen=True, kw_only=True):
    """Scene-linear formation through OCIO's viewing pipeline: exposure, look, display view, and display gamma."""

    display: Annotated[str, msgspec.Meta(min_length=1, description="Target display device")]
    view: Annotated[str, msgspec.Meta(min_length=1, description="View transform of display")]
    look: Annotated[Gated[str], msgspec.Meta(description="Look overriding the view's looks")] = Gated(enabled=False, value="")
    exposure: Annotated[Stops, msgspec.Meta(description="Scene-linear exposure before the view")] = 0.0
    gamma: Annotated[Gamma, msgspec.Meta(description="Display exponent after the view")] = 1.0

    @classmethod
    def for_encoding(cls, config: PyOpenColorIO.Config, display: str, view: Option[str] = Nothing) -> ImagingResult[Option[tuple[Self, PyOpenColorIO.Processor]]]:
        """Returns first lookless matrix-only scene view with its processor, absent when none qualifies."""

        def discovered(name: str) -> ImagingResult[Option[tuple[Self, PyOpenColorIO.Processor]]]:
            candidate = cls(display=display, view=name)
            return (
                Option
                .of_optional(
                    next(
                        (
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
                        ),
                        None,
                    )
                )
                .map(
                    lambda request: paired(
                        colored(request, lambda: config.getProcessor(request).getOptimizedProcessor(PyOpenColorIO.OPTIMIZATION_DEFAULT).createGroupTransform()), candidate.transform(config)
                    ).map(lambda built: Some((candidate, built[1])) if all(isinstance(op, PyOpenColorIO.MatrixTransform) for op in built[0]) else Nothing)
                )
                .default_value(Ok(Nothing))
            )

        return accumulated(map(discovered, view.map(lambda name: (name,)).default_with(lambda: tuple(config.getViews(display))))).map(
            lambda candidates: Block.of_seq(candidates).choose(identity).try_head()
        )

    def colorspace(self, config: PyOpenColorIO.Config) -> Option[PyOpenColorIO.ColorSpace]:
        """Returns view colorspace, resolving shared view display-name token."""
        name = config.getDisplayViewColorSpaceName(self.display, self.view)
        return Option.of_optional(config.getColorSpace(self.display if name == PyOpenColorIO.OCIO_VIEW_USE_DISPLAY_NAME else name))

    def transform(self, config: PyOpenColorIO.Config, source: str = PyOpenColorIO.ROLE_SCENE_LINEAR) -> ImagingResult[PyOpenColorIO.Processor]:
        """Returns the viewing pipeline processor from `source`, or color fault."""

        def pipeline() -> PyOpenColorIO.Processor:
            built = PyOpenColorIO.LegacyViewingPipeline()
            built.setDisplayViewTransform(PyOpenColorIO.DisplayViewTransform(src=source, display=self.display, view=self.view))
            built.setLooksOverrideEnabled(self.look.enabled)
            built.setLooksOverride(self.look.value)
            if self.exposure:
                built.setLinearCC(PyOpenColorIO.MatrixTransform.Scale([2.0**self.exposure] * 3 + [1.0]))
            if self.gamma != 1.0:
                built.setDisplayCC(PyOpenColorIO.ExponentTransform([1.0 / max(self.gamma, float(np.spacing(np.float32(1))))] * 3 + [1.0]))
            return built.getProcessor(config)

        return colored(self, pipeline)

    def form(self, config: PyOpenColorIO.Config, values: npt.NDArray[np.float32], source: str = PyOpenColorIO.ROLE_SCENE_LINEAR) -> ImagingResult[npt.NDArray[np.float32]]:
        """Returns associated display colors of associated `values` in `source`."""
        return self.transform(config, source).map(lambda built: applied(built.getDefaultCPUProcessor(), values))


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


def processor(config: PyOpenColorIO.Config, transform: PyOpenColorIO.Transform) -> ImagingResult[PyOpenColorIO.Processor]:
    """Returns processor, or color fault."""
    return colored(transform, lambda: config.getProcessor(transform))


def converted(config: PyOpenColorIO.Config, transform: PyOpenColorIO.Transform, values: npt.NDArray[np.float32]) -> ImagingResult[npt.NDArray[np.float32]]:
    """Applies `transform` to associated colors through its CPU processor, or color fault."""
    return processor(config, transform).map(lambda built: applied(built.getDefaultCPUProcessor(), values))


# --- [READINGS]


def basis(config: PyOpenColorIO.Config, space: str) -> ImagingResult[npt.NDArray[np.float64]]:
    """Returns linear RGB-to-XYZ D65 matrix of `space`, or base fault."""
    request = xyz(config, space)

    def composed(group: PyOpenColorIO.GroupTransform) -> ImagingResult[npt.NDArray[np.float64]]:
        unsupported = (ImagingBaseFault(space=space, transform=group),)
        matrices = tuple(op for op in group if isinstance(op, PyOpenColorIO.MatrixTransform))
        if len(matrices) != len(group):
            return Error(unsupported)
        try:
            with np.errstate(over="raise", invalid="raise"):
                affine = reduce(
                    np.matmul, (np.vstack((np.column_stack((np.asarray(op.getMatrix(), dtype=np.float64).reshape(4, 4), op.getOffset())), np.eye(1, 5, 4))) for op in reversed(matrices)), np.eye(5)
                )
        except FloatingPointError:
            return Error(unsupported)
        return Ok(affine[:3, :3]) if np.isfinite(affine[:3]).all() and (affine[:3, 3:] == 0.0).all() else Error(unsupported)

    return colored(request, lambda: config.getProcessor(request).getOptimizedProcessor(PyOpenColorIO.OPTIMIZATION_DEFAULT).createGroupTransform()).bind(composed)


def rendering_basis(config: PyOpenColorIO.Config, space: str) -> ImagingResult[npt.NDArray[np.float64]]:
    """Returns the RGB-to-XYZ basis of `space` when its primaries hold positive luminance and one white sign with an invertible white-normalized matrix, else base fault."""

    def held(values: npt.NDArray[np.float64]) -> ImagingResult[npt.NDArray[np.float64]]:
        unsupported = (ImagingBaseFault(space=space, transform=matrix(values)),)
        try:
            with np.errstate(over="raise", invalid="raise", divide="raise"):
                sums = values.sum(axis=0)
                if not ((values[1] > 0.0).all() and ((sums > 0.0).all() or (sums < 0.0).all()) and np.isfinite(sums.sum()) and np.isfinite(values.sum(axis=1)).all()):
                    return Error(unsupported)
                normalized = values / sums
                return Ok(values) if np.isfinite(normalized).all() and abs(np.linalg.slogdet(normalized).sign) == 1.0 else Error(unsupported)
        except FloatingPointError:
            return Error(unsupported)

    return basis(config, space).bind(held)


def matching(config: PyOpenColorIO.Config, space: str, tristimulus: npt.NDArray[np.float64]) -> ImagingResult[npt.NDArray[np.float64]]:
    """Returns signed RGB matching weights with unit channel integrals."""

    def solved(values: npt.NDArray[np.float64]) -> ImagingResult[npt.NDArray[np.float64]]:
        try:
            weights = np.linalg.solve(values, tristimulus.T).T
        except np.linalg.LinAlgError as error:
            return Error((NumericFault(request=values, error=error),))
        unsupported = (ImagingSpectralFault(space=space, values=weights),)
        try:
            with np.errstate(over="raise", invalid="raise", divide="raise"):
                integral = weights.sum(axis=0)
                return Ok(weights / integral) if np.isfinite(weights).all() and np.isfinite(integral).all() and (integral != 0.0).all() else Error(unsupported)
        except FloatingPointError:
            return Error(unsupported)

    return basis(config, space).bind(solved)


def luma(config: PyOpenColorIO.Config) -> ImagingResult[npt.NDArray[np.float64]]:
    """Returns scene-linear luminance coefficients, the Y row of its RGB-to-XYZ matrix, or base fault."""
    return basis(config, PyOpenColorIO.ROLE_SCENE_LINEAR).map(lambda matrix: matrix[1])


def chromaticities(config: PyOpenColorIO.Config, space: str) -> ImagingResult[npt.NDArray[np.float64]]:
    """Returns float64 xy rows of red, green, blue, and white from unit codes every display encoding fixes, or color fault."""
    return converted(config, xyz(config, space), np.vstack((np.eye(3, dtype=np.float32), np.ones((1, 3), dtype=np.float32)))).map(coordinates)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "GREY",
    "REC709_SCENE",
    "ColorSpace",
    "DisplayView",
    "Domain",
    "Gamma",
    "Stops",
    "basis",
    "bracketed",
    "chromaticities",
    "converted",
    "lin_to_log",
    "luma",
    "matching",
    "matrix",
    "processor",
    "rendering_basis",
    "xyz",
]
