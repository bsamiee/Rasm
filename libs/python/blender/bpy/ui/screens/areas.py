"""Editor queries, area contexts, and reversible screen arrangements."""

from collections.abc import Iterable, Mapping
from contextlib import ExitStack
from enum import auto, Enum
from typing import Annotated, Literal, Self, TYPE_CHECKING

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
import msgspec

from blender.bpy.commands.calls import called, Operation
from blender.bpy.data.rna.conditions import Condition, holds
from blender.bpy.data.rna.descriptors import rooted, written
from blender.bpy.extension.identity import Identity
from blender.bpy.faults import BlenderAbsentFault, BlenderResult
from blender.bpy.ui.screens.editors import EditorState
from blender.imaging.faults import accumulated, paired

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Step = Split | Retype


class ScreenSide(Enum):
    """Edge an arrangement places an editor or flips a region to."""

    LEFT = auto()
    RIGHT = auto()
    TOP = auto()
    BOTTOM = auto()

    @property
    def opposite(self) -> "ScreenSide":
        """Opposite edge."""
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
        """`screen.area_split` direction."""
        return "HORIZONTAL" if self in {self.TOP, self.BOTTOM} else "VERTICAL"

    @property
    def far(self) -> bool:
        """Whether edge ends split axis."""
        return self in {self.TOP, self.RIGHT}


class Fullscreen(Enum):
    """`screen.screen_full_area` modes."""

    MAXIMIZED = auto()
    FOCUS = auto()


class NewWindow(Enum):
    """Temporary editor windows and area duplicate."""

    PREFERENCES = auto()
    INFO = auto()
    DRIVERS = auto()
    PROJECT = auto()
    DUPLICATE = auto()


# --- [MODELS] ---------------------------------------------------------------------------


class Located(msgspec.Struct, frozen=True, kw_only=True):
    """Window and area for one host call."""

    window: bpy.types.Window
    area: bpy.types.Area

    @classmethod
    def current(cls, context: bpy.types.Context) -> BlenderResult[Self]:
        """Returns context window and area, accumulating absences."""
        return paired(rooted(bpy.types.Window, context), rooted(bpy.types.Area, context)).map(lambda pair: cls(window=pair[0], area=pair[1]))

    @classmethod
    def found(cls, window: bpy.types.Window, pointer: int) -> Option[Self]:
        """Returns live area by session pointer, Nothing once removed."""
        return Option.of_optional(next((area for area in (*window.screen.areas, *window.global_areas) if area.as_pointer() == pointer), None)).map(lambda area: cls(window=window, area=area))

    def region(self, kind: "rna_enums.RegionTypeItems" = "WINDOW") -> BlenderResult[bpy.types.Region]:
        """Returns first region of a type."""
        return Option.of_optional(next((region for region in self.area.regions if region.type == kind), None)).to_result_with(lambda: (BlenderAbsentFault(owner=repr(self.area), key=kind),))

    def override(self, kind: "rna_enums.RegionTypeItems" = "WINDOW") -> BlenderResult["bpy.types.ContextTempOverride"]:
        """Returns operator context of a region."""
        return self.region(kind).map(lambda region: bpy.context.temp_override(window=self.window, area=self.area, region=region))

    def run(self, operator: Operation, properties: Mapping[str, object], /, *, context: "rna_enums.OperatorContextItems" = "EXEC_DEFAULT") -> BlenderResult[None]:
        """Runs operator in window and area."""
        return called(operator, properties, context=context, override=Some(bpy.context.temp_override(window=self.window, area=self.area))).map(lambda _: None)


class AreaQuery(msgspec.Struct, frozen=True, kw_only=True):
    """Editor type, `ui_type`, and active space condition."""

    space: "rna_enums.SpaceTypeItems"
    ui_type: Option[str] = Nothing
    shows: Option[Condition] = Nothing

    def matches(self, area: bpy.types.Area) -> bool:
        """Returns whether area and active space match."""
        return (
            area.type == self.space
            and all(area.ui_type == kind for kind in self.ui_type.to_list())
            and all(holds(condition, bpy.context, Option.of_optional(area.spaces.active)) for condition in self.shows.to_list())
        )

    def located(self, windows: Iterable[bpy.types.Window]) -> tuple[Located, ...]:
        """Returns matching areas, largest first."""
        matches: list[Located] = []
        for located in (Located(window=window, area=area) for window in windows for area in (*window.screen.areas, *window.global_areas)):
            with bpy.context.temp_override(window=located.window, area=located.area):
                if self.matches(located.area):
                    matches.append(located)
        return tuple(sorted(matches, key=lambda located: -(located.area.width * located.area.height)))

    def first(self, windows: Iterable[bpy.types.Window]) -> BlenderResult[Located]:
        """Returns largest matching area, or fault naming editor."""
        return Option.of_optional(next(iter(self.located(windows)), None)).to_result_with(lambda: (BlenderAbsentFault(owner="WindowManager.windows", key=self.ui_type.default_value(self.space)),))


class Split(msgspec.Struct, frozen=True, kw_only=True):
    """Editor split off at edge, under half of source span."""

    side: ScreenSide
    share: Annotated[float, msgspec.Meta(gt=0.0, lt=0.5)]
    ui_type: str
    state: EditorState = EditorState()


class Retype(msgspec.Struct, frozen=True, kw_only=True):
    """Temporary editor type change."""

    ui_type: str
    state: EditorState = EditorState()


class _ArrangedArea(msgspec.Struct, kw_only=True):
    """Arranged area with prior type and values to restore first."""

    area: int
    prior: Option[str]
    restore: EditorState = EditorState()


class Arrangement(msgspec.Struct, frozen=True, kw_only=True):
    """Splits and retypes, then maximization."""

    steps: tuple[Step, ...]
    full: Option[Fullscreen] = Nothing

    def applied(self, located: Located) -> BlenderResult["AppliedArrangement"]:
        """Starts arrangement, undoing finished steps on refusal."""
        return AppliedArrangement(located, self).started()


# --- [SERVICES] -------------------------------------------------------------------------


class AppliedArrangement:
    """Running arrangement with one release stack."""

    def __init__(self, located: Located, arrangement: Arrangement) -> None:
        """Holds pending steps and releases."""
        self.window = located.window
        self.area = located.area.as_pointer()
        self.steps = arrangement.steps
        self.full = arrangement.full
        self.waiting: Option[tuple[_ArrangedArea, EditorState]] = Nothing
        self.releases = ExitStack()
        self.results: list[BlenderResult[None]] = []

    def started(self) -> BlenderResult[Self]:
        """Applies first step, undoing finished steps on refusal."""
        return self.advanced().map(lambda _: self).or_else_with(lambda faults: accumulated((Error(faults), self.released())).map(lambda _: self))

    def advanced(self) -> BlenderResult[Option[Self]]:
        """Applies one step per host pass once editor state applies."""
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

        def undone(placed: _ArrangedArea) -> BlenderResult[None]:
            """Restores retyped area or closes split area."""

            def restored(at: Located) -> BlenderResult[None]:
                """Restores editor type even when state restore fails."""
                return placed.prior.map(lambda ui_type: accumulated((placed.restore.applied(at.area).map(lambda _: None), written(at.area, "ui_type", ui_type))).map(lambda _: None)).default_with(
                    lambda: at.run(bpy.ops.screen.area_close, {})
                )

            return Located.found(self.window, placed.area).map(restored).default_value(Ok(None))

        def entered(at: Located, prior: Option[str], ui_type: str, state: EditorState) -> BlenderResult[None]:
            """Registers release, then retypes, deferring editor state to next pass."""
            placed = _ArrangedArea(area=at.area.as_pointer(), prior=prior)
            self.releases.callback(lambda: self.results.append(undone(placed)))
            self.waiting = Some((placed, state))
            return written(at.area, "ui_type", ui_type).map(lambda _: None)

        def stepped(at: Located, step: Step) -> BlenderResult[None]:
            """Splits or retypes source area."""
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
                    """Drops finished step, waiting for next layout pass."""
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

    def released(self) -> BlenderResult[None]:
        """Releases in reverse order."""
        self.releases.close()
        return accumulated(self.results).map(lambda _: None)


# --- [OPERATIONS] -----------------------------------------------------------------------


def windowed(window: NewWindow, located: Located, identity: Identity) -> BlenderResult[None]:
    """Opens editor window."""
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
