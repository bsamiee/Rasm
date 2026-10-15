"""Editor queries, native operator contexts, and reversible screen arrangements."""

from collections.abc import Iterable, Mapping
from contextlib import ExitStack
from enum import auto, Enum
from typing import Annotated, Literal, Self, TYPE_CHECKING

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
import msgspec

from blender.bpy.commands.calls import called, Operation
from blender.bpy.data.rna.conditions import Condition, holds
from blender.bpy.data.rna.descriptors import written
from blender.bpy.extension.identity import Identity
from blender.bpy.faults import BlenderAbsentFault, BlenderResult
from blender.bpy.ui.screens.editors import EditorState
from blender.imaging.faults import accumulated

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Step = Split | Retype


class ScreenSide(Enum):
    """Edge at which an arrangement places an editor or flips a region."""

    LEFT = auto()
    RIGHT = auto()
    TOP = auto()
    BOTTOM = auto()

    @property
    def opposite(self) -> "ScreenSide":
        """Opposite edge on the same axis."""
        match self:
            case ScreenSide.LEFT:
                return self.RIGHT
            case ScreenSide.RIGHT:
                return self.LEFT
            case ScreenSide.TOP:
                return self.BOTTOM
            case ScreenSide.BOTTOM:
                return self.TOP

    @property
    def direction(self) -> Literal["HORIZONTAL", "VERTICAL"]:
        """Native split direction for this edge."""
        return "HORIZONTAL" if self in {self.TOP, self.BOTTOM} else "VERTICAL"

    @property
    def far(self) -> bool:
        """Whether the edge is at the far end of the native split axis."""
        return self in {self.TOP, self.RIGHT}


class Fullscreen(Enum):
    """Native maximized-area and focus-mode choices."""

    MAXIMIZED = auto()
    FOCUS = auto()


class NewWindow(Enum):
    """Native temporary editor windows and a duplicate of the located area."""

    PREFERENCES = auto()
    INFO = auto()
    DRIVERS = auto()
    PROJECT = auto()
    DUPLICATE = auto()


# --- [MODELS] ---------------------------------------------------------------------------


class Located(msgspec.Struct, frozen=True, kw_only=True):
    """Window and area read together for one native call."""

    window: bpy.types.Window
    area: bpy.types.Area

    @classmethod
    def found(cls, window: bpy.types.Window, pointer: int) -> Option[Self]:
        """Returns a live area by its session pointer, or Nothing after its removal."""
        return Option.of_optional(next((area for area in (*window.screen.areas, *window.global_areas) if area.as_pointer() == pointer), None)).map(lambda area: cls(window=window, area=area))

    def region(self, kind: "rna_enums.RegionTypeItems" = "WINDOW") -> BlenderResult[bpy.types.Region]:
        """Returns the area's first region of this native type, or its absence."""
        return Option.of_optional(next((region for region in self.area.regions if region.type == kind), None)).to_result_with(lambda: (BlenderAbsentFault(owner=repr(self.area), key=kind),))

    def override(self, kind: "rna_enums.RegionTypeItems" = "WINDOW") -> BlenderResult["bpy.types.ContextTempOverride"]:
        """Returns the located region's operator context, or a missing-region fault."""
        return self.region(kind).map(lambda region: bpy.context.temp_override(window=self.window, area=self.area, region=region))

    def run(self, operator: Operation, properties: Mapping[str, object], /, *, context: "rna_enums.OperatorContextItems" = "EXEC_DEFAULT") -> BlenderResult[None]:
        """Runs an area-scoped operator and returns its native refusal."""
        return called(operator, properties, context=context, override=Some(bpy.context.temp_override(window=self.window, area=self.area))).map(lambda _: None)


class AreaQuery(msgspec.Struct, frozen=True, kw_only=True):
    """Editor family, optional sub-editor, and a condition over its active space."""

    space: "rna_enums.SpaceTypeItems"
    ui_type: Option[str] = Nothing
    shows: Option[Condition] = Nothing

    def admits(self, area: bpy.types.Area) -> bool:
        """Returns whether the area and its active space meet this query."""
        return (
            area.type == self.space
            and all(area.ui_type == kind for kind in self.ui_type.to_list())
            and all(holds(condition, bpy.context, Option.of_optional(area.spaces.active)) for condition in self.shows.to_list())
        )

    def located(self, windows: Iterable[bpy.types.Window]) -> tuple[Located, ...]:
        """Returns matching ordinary and global areas, ordered by decreasing pixel area."""
        matches: list[Located] = []
        for located in (Located(window=window, area=area) for window in windows for area in (*window.screen.areas, *window.global_areas)):
            with bpy.context.temp_override(window=located.window, area=located.area):
                if self.admits(located.area):
                    matches.append(located)
        return tuple(sorted(matches, key=lambda located: -(located.area.width * located.area.height)))

    def first(self, windows: Iterable[bpy.types.Window]) -> BlenderResult[Located]:
        """Returns the largest matching editor, or the absent editor request."""
        return Option.of_optional(next(iter(self.located(windows)), None)).to_result_with(lambda: (BlenderAbsentFault(owner="WindowManager.windows", key=self.ui_type.default_value(self.space)),))


class Split(msgspec.Struct, frozen=True, kw_only=True):
    """New editor at an edge, occupying less than half the source area's span."""

    side: ScreenSide
    share: Annotated[float, msgspec.Meta(gt=0.0, lt=0.5)]
    ui_type: str
    state: EditorState = EditorState()


class Retype(msgspec.Struct, frozen=True, kw_only=True):
    """Temporary editor substitution in the located area."""

    ui_type: str
    state: EditorState = EditorState()


class _ArrangedArea(msgspec.Struct, kw_only=True):
    """Acquired editor and the values restored before its previous type returns."""

    area: int
    prior: Option[str]
    restore: EditorState = EditorState()


class Arrangement(msgspec.Struct, frozen=True, kw_only=True):
    """Ordered editor acquisitions, followed by optional native maximization."""

    steps: tuple[Step, ...]
    full: Option[Fullscreen] = Nothing

    def applied(self, located: Located) -> BlenderResult["AppliedArrangement"]:
        """Returns an owned arrangement, unwinding every completed step on refusal."""
        return AppliedArrangement(located, self).started()


# --- [SERVICES] -------------------------------------------------------------------------


class AppliedArrangement:
    """Native layout lifetime whose initial and advancing states share rollback ownership."""

    def __init__(self, located: Located, arrangement: Arrangement) -> None:
        """Owns the pending steps and releases for one native window and source area."""
        self.window = located.window
        self.area = located.area.as_pointer()
        self.steps = arrangement.steps
        self.full = arrangement.full
        self.waiting: Option[tuple[_ArrangedArea, EditorState]] = Nothing
        self.releases = ExitStack()
        self.results: list[BlenderResult[None]] = []

    def started(self) -> BlenderResult[Self]:
        """Applies the first steps, unwinding every completed step on refusal."""
        return self.advanced().map(lambda _: self).or_else_with(lambda faults: accumulated((Error(faults), self.released())).map(lambda _: self))

    def advanced(self) -> BlenderResult[Option[Self]]:
        """Applies one layout step per host pass, waiting for its state before continuing."""
        for placed, state in self.waiting.to_list():
            match Located.found(self.window, placed.area).map(lambda located: state.applied(located.area)):
                case Option(tag="some", some=Result(tag="error", error=faults)):
                    return Error(faults)
                case Option(tag="some", some=Result(ok=Option(tag="none"))):
                    return Ok(Some(self))
                case Option(tag="some", some=Result(ok=Option(some=restore))):
                    placed.restore = restore
                    self.waiting = Nothing
                case _:
                    self.waiting = Nothing

        def entered(at: Located, prior: Option[str], ui_type: str, state: EditorState) -> BlenderResult[None]:
            """Owns release before changing the editor and defers its state to the next pass."""
            placed = _ArrangedArea(area=at.area.as_pointer(), prior=prior)
            self.releases.callback(lambda: self.results.append(self._undone(placed)))
            self.waiting = Some((placed, state))
            return written(at.area, "ui_type", ui_type).map(lambda _: None)

        def stepped(at: Located, step: Step) -> BlenderResult[None]:
            """Splits or retypes in the reacquired source area's native context."""
            match step:
                case Split(side=side, share=share, ui_type=ui_type, state=state):
                    before = frozenset(area.as_pointer() for area in self.window.screen.areas)
                    return (
                        at
                        .run(bpy.ops.screen.area_split, {"direction": side.direction, "factor": 1.0 - share if side.far else share})
                        .bind(
                            lambda _: Option.of_optional(next((area for area in self.window.screen.areas if area.as_pointer() not in before), None)).to_result_with(
                                lambda: (BlenderAbsentFault(owner=repr(self.window.screen), key="screen.area_split"),)
                            )
                        )
                        .bind(lambda area: entered(Located(window=self.window, area=area), Nothing, ui_type, state))
                    )
                case Retype(ui_type=ui_type, state=state):
                    return entered(at, Some(at.area.ui_type), ui_type, state)

        match Located.found(self.window, self.area):
            case Option(tag="none"):
                return Error((BlenderAbsentFault(owner=repr(self.window.screen), key=self.area),))
            case Option(some=at) if self.steps:
                step, *remaining = self.steps

                def held(_: None) -> Option[Self]:
                    """Consumes the acquired step and yields for Blender's layout pass."""
                    self.steps = tuple(remaining)
                    return Some(self)

                return stepped(at, step).map(held)
            case Option(some=at):
                match self.full.map(lambda full: at.run(bpy.ops.screen.screen_full_area, {"use_hide_panels": full is Fullscreen.FOCUS})):
                    case Option(tag="none"):
                        return Ok(Nothing)
                    case Option(some=Result(tag="error", error=faults)):
                        return Error(faults)
                    case _:
                        self.releases.callback(
                            lambda: self.results.append(called(bpy.ops.screen.back_to_previous, {}, override=Some(bpy.context.temp_override(window=self.window))).map(lambda _: None))
                        )
                        self.full = Nothing
                        return Ok(Nothing)

    def _undone(self, placed: _ArrangedArea) -> BlenderResult[None]:
        """Restores a surviving editor or closes a created area, retaining every refusal."""

        def restored(at: Located) -> BlenderResult[None]:
            """Reverts the editor type even when a display member refuses restoration."""
            return placed.prior.map(lambda ui_type: accumulated((placed.restore.applied(at.area).map(lambda _: None), written(at.area, "ui_type", ui_type))).map(lambda _: None)).default_with(
                lambda: at.run(bpy.ops.screen.area_close, {})
            )

        return Located.found(self.window, placed.area).map(restored).default_value(Ok(None))

    def released(self) -> BlenderResult[None]:
        """Releases in reverse acquisition order and returns every native restore refusal."""
        self.releases.close()
        return accumulated(self.results).map(lambda _: None)


# --- [OPERATIONS] -----------------------------------------------------------------------


def windowed(window: NewWindow, located: Located, identity: Identity) -> BlenderResult[None]:
    """Opens a native editor window, or returns the operator's context or edit refusal."""
    match window:
        case NewWindow.PREFERENCES:
            return located.run(bpy.ops.preferences.addon_show, {"module": identity.package})
        case NewWindow.INFO:
            return located.run(bpy.ops.screen.info_log_show, {})
        case NewWindow.DRIVERS:
            return located.run(bpy.ops.screen.drivers_editor_show, {})
        case NewWindow.PROJECT:
            return located.run(bpy.ops.screen.project_setup_show, {})  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        case NewWindow.DUPLICATE:
            return located.run(bpy.ops.screen.area_dupli, {}, context="INVOKE_DEFAULT")


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["AppliedArrangement", "AreaQuery", "Arrangement", "Fullscreen", "Located", "NewWindow", "Retype", "ScreenSide", "Split", "Step", "windowed"]
