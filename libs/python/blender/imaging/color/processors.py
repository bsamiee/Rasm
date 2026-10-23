"""Display formation, colorspace transforms, and associated-color processing."""

from collections.abc import Callable
from functools import reduce
from typing import Annotated, Final, override, Protocol, runtime_checkable, Self

from expression import Error, identity, Nothing, Ok, Option, Some
from expression.collections import Block
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO
from scipy.linalg import block_diag

from blender.imaging.chain import Effect, GREY, Stage
from blender.imaging.color.observer import coordinates
from blender.imaging.faults import accumulated, colored, ImagingBaseFault, ImagingNumericFault, ImagingResult, ImagingSpectralFault, paired
from blender.imaging.parameters import Gated, GraphForm, Stops

# --- [TYPES] ----------------------------------------------------------------------------

type Gamma = Annotated[float, msgspec.Meta(ge=0.0, le=5.0)]


@runtime_checkable
class Spaced(Protocol):
    """Effect with an operation in a declared process space."""

    @property
    def space(self) -> str:
        """Process colorspace or role."""
        ...

    def transform(self, config: PyOpenColorIO.Config) -> ImagingResult[Option[PyOpenColorIO.Transform]]:
        """Returns operation without process-space conversions."""
        ...


# --- [CONSTANTS] ------------------------------------------------------------------------

REC709_SCENE: Final = "lin_rec709_scene"

# --- [MODELS] ---------------------------------------------------------------------------


class DisplayView(Effect, frozen=True, kw_only=True, tag=0):
    """Scene-linear formation through look and display view."""

    stage = Stage.FORMATION
    pointwise = True
    display: Annotated[str, msgspec.Meta(min_length=1, description="Target display device"), GraphForm.SETTING]
    view: Annotated[str, msgspec.Meta(min_length=1, description="View transform of display"), GraphForm.SETTING]
    look: Annotated[Gated[Annotated[str, GraphForm.SETTING]], msgspec.Meta(description="Look before view transform")] = Gated(enabled=False, value="")

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
                        colored(request, lambda: config.getProcessor(request).getOptimizedProcessor(PyOpenColorIO.OPTIMIZATION_DEFAULT).createGroupTransform()),
                        processor(config, PyOpenColorIO.DisplayViewTransform(src=PyOpenColorIO.ROLE_SCENE_LINEAR, display=display, view=name)),
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

    @override
    def transform(self, config: PyOpenColorIO.Config) -> ImagingResult[Option[PyOpenColorIO.Transform]]:
        """Returns look and display view in Blender CPU formation order."""
        data = self.colorspace(config).filter(PyOpenColorIO.ColorSpace.isData).is_some()
        look = (
            Some(
                PyOpenColorIO.LookTransform(
                    src=PyOpenColorIO.ROLE_SCENE_LINEAR,
                    dst=Option.of_optional(config.getLook(self.look.value)).filter(lambda _: not data).map(PyOpenColorIO.Look.getProcessSpace).default_value(PyOpenColorIO.ROLE_SCENE_LINEAR),
                    looks=self.look.value,
                )
            )
            if self.look.enabled
            else Nothing
        )
        return Ok(
            Some(
                PyOpenColorIO.GroupTransform([
                    *look.to_list(),
                    PyOpenColorIO.DisplayViewTransform(
                        src=look.map(PyOpenColorIO.LookTransform.getDst).default_value(PyOpenColorIO.ROLE_SCENE_LINEAR), display=self.display, view=self.view, looksBypass=look.is_some()
                    ),
                ])
            )
        )

    def form(
        self, config: PyOpenColorIO.Config, values: npt.NDArray[np.float32], *, source: str = PyOpenColorIO.ROLE_SCENE_LINEAR, exposure: Stops = 0.0, gamma: Gamma = 1.0
    ) -> ImagingResult[npt.NDArray[np.float32]]:
        """Returns associated display colors under view exposure and gamma."""
        exponent = 1.0 / max(gamma, float(np.spacing(np.float32(1))))
        return self.transform(config).bind(
            lambda op: processor(
                config,
                PyOpenColorIO.GroupTransform([
                    PyOpenColorIO.ColorSpaceTransform(src=source, dst=PyOpenColorIO.ROLE_SCENE_LINEAR),
                    PyOpenColorIO.MatrixTransform.Scale([2.0**exposure] * 3 + [1.0]),
                    *op.to_list(),
                    *([PyOpenColorIO.ExponentTransform([exponent] * 3 + [1.0])] if exponent != 1.0 else []),
                ]),
            ).map(lambda built: applied(built.getDefaultCPUProcessor(), values))
        )


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [TRANSFORMS]


def bracketed(entry: Spaced, config: PyOpenColorIO.Config) -> ImagingResult[Option[PyOpenColorIO.GroupTransform]]:
    """Returns `entry` operation between scene-linear conversions, absent without one."""
    return entry.transform(config).map(
        lambda operation: operation.map(
            lambda op: PyOpenColorIO.GroupTransform([
                PyOpenColorIO.ColorSpaceTransform(src=PyOpenColorIO.ROLE_SCENE_LINEAR, dst=entry.space),
                op,
                PyOpenColorIO.ColorSpaceTransform(src=entry.space, dst=PyOpenColorIO.ROLE_SCENE_LINEAR),
            ])
        )
    )


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


def joined(source: PyOpenColorIO.Config, source_space: str, config: PyOpenColorIO.Config, space: str) -> ImagingResult[PyOpenColorIO.Processor]:
    """Returns processor between configs through interchange roles, or color fault."""
    return colored(PyOpenColorIO.ColorSpaceTransform(src=source_space, dst=space), lambda: PyOpenColorIO.Config.GetProcessorFromConfigs(source, source_space, config, space))


# --- [APPLICATION]


def unassociated(values: npt.NDArray[np.float32]) -> npt.NDArray[np.float32]:
    """Returns contiguous straight copy of RGB or RGBA, color kept at zero alpha."""
    if (straight := (values.copy())).shape[-1] == 4:
        alpha = straight[..., 3:]
        np.divide(straight[..., :3], alpha, out=straight[..., :3], where=alpha != 0.0)
    return straight


def applied(operation: PyOpenColorIO.CPUProcessor | Callable[[npt.NDArray[np.float32]], npt.NDArray[np.float32]], values: npt.NDArray[np.float32]) -> npt.NDArray[np.float32]:
    """Applies OCIO processor or straight-RGB operation to associated colors."""
    formed = unassociated(values)
    if isinstance(operation, PyOpenColorIO.CPUProcessor):
        if formed.shape[-1] == 4:
            operation.applyRGBA(formed)
        else:
            operation.applyRGB(formed)
    else:
        formed[..., :3] = operation(formed[..., :3])
    if formed.shape[-1] == 4:
        alpha = formed[..., 3:]
        np.multiply(formed[..., :3], alpha, out=formed[..., :3], where=alpha != 0.0)
    return formed


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


def matching(config: PyOpenColorIO.Config, space: str, tristimulus: npt.NDArray[np.float64]) -> ImagingResult[npt.NDArray[np.float64]]:
    """Returns signed RGB matching weights with unit channel integrals."""

    def solved(values: npt.NDArray[np.float64]) -> ImagingResult[npt.NDArray[np.float64]]:
        try:
            weights = np.linalg.solve(values, tristimulus.T).T
        except np.linalg.LinAlgError as error:
            return Error((ImagingNumericFault(request=values, error=error),))
        unsupported = (ImagingSpectralFault(space=space, values=weights),)
        try:
            with np.errstate(over="raise", invalid="raise", divide="raise"):
                integral = weights.sum(axis=0)
                return Ok(weights / integral) if np.isfinite(weights).all() and np.isfinite(integral).all() and (integral != 0.0).all() else Error(unsupported)
        except FloatingPointError:
            return Error(unsupported)

    return basis(config, space).bind(solved)


def luma(config: PyOpenColorIO.Config) -> ImagingResult[npt.NDArray[np.float32]]:
    """Returns scene-linear luminance coefficients, or color fault."""
    return processor(config, xyz(config, PyOpenColorIO.ROLE_SCENE_LINEAR)).map(lambda built: applied(built.getDefaultCPUProcessor(), np.eye(3, dtype=np.float32))[:, 1])


def chromaticities(config: PyOpenColorIO.Config, space: str) -> ImagingResult[npt.NDArray[np.float64]]:
    """Returns float64 xy rows of red, green, blue, and white, or color fault."""
    return processor(config, xyz(config, space)).map(lambda built: coordinates(applied(built.getDefaultCPUProcessor(), np.vstack((np.eye(3, dtype=np.float32), np.ones((1, 3), dtype=np.float32))))))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "REC709_SCENE",
    "DisplayView",
    "Gamma",
    "Spaced",
    "applied",
    "basis",
    "bracketed",
    "chromaticities",
    "joined",
    "lin_to_log",
    "luma",
    "matching",
    "matrix",
    "processor",
    "unassociated",
    "xyz",
]
