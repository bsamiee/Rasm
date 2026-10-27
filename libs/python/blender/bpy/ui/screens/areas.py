"""Editor queries, area contexts, and screen arrangements applied one host pass per step."""

from collections.abc import Callable, Iterable, Mapping
from functools import partial
import sys
from typing import Annotated, Literal, Self, TYPE_CHECKING

import bpy
from expression import Nothing, Ok, Option, Result, Some
from expression.collections import Block
import msgspec

from blender.bpy.commands.calls import called, Operation
from blender.bpy.data.rna.conditions import Condition
from blender.bpy.data.rna.descriptors import rooted, written
from blender.bpy.faults import BlenderReason
from blender.bpy.ui.screens.editors import EditorState
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------


type Edge = Literal["LEFT", "RIGHT", "TOP", "BOTTOM"]
type Step = Split | Retype | EditorState | Arrangement

# --- [MODELS] ---------------------------------------------------------------------------


class Located(msgspec.Struct, frozen=True, kw_only=True):
    """Window and area for one host call."""

    window: bpy.types.Window
    area: bpy.types.Area

    @classmethod
    def current(cls, context: bpy.types.Context) -> Outcome[Self]:
        """Returns context window and area, accumulating absences."""
        return paired(rooted(bpy.types.Window, context), rooted(bpy.types.Area, context)).map(lambda pair: cls(window=pair[0], area=pair[1]))

    @classmethod
    def found(cls, window: bpy.types.Window, pointer: int) -> Option[Self]:
        """Returns live area by session pointer, Nothing once removed."""
        return Block.of_seq((*window.screen.areas, *window.global_areas)).filter(lambda area: area.as_pointer() == pointer).try_head().map(lambda area: cls(window=window, area=area))

    def region(self, kind: "rna_enums.RegionTypeItems" = "WINDOW") -> Outcome[bpy.types.Region]:
        """Returns first region of a type."""
        return Block.of_seq(self.area.regions).filter(lambda region: region.type == kind).try_head().to_result_with(partial(Fault.of, BlenderReason.ABSENT, repr(self.area), member=kind))

    def override(self, kind: "rna_enums.RegionTypeItems" = "WINDOW") -> Outcome[bpy.types.ContextTempOverride]:
        """Returns operator context of a region."""
        return self.region(kind).map(lambda region: bpy.context.temp_override(window=self.window, area=self.area, region=region))

    def run(self, operator: Operation, properties: Mapping[str, object], /, *, context: "rna_enums.OperatorContextItems" = "EXEC_DEFAULT") -> Outcome[None]:
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
            and all(condition.holds(bpy.context, Option.of_optional(area.spaces.active)) for condition in self.shows.to_list())
        )

    def located(self, windows: Iterable[bpy.types.Window]) -> tuple[Located, ...]:
        """Returns matching areas, largest first."""
        matches: list[Located] = []
        for located in (Located(window=window, area=area) for window in windows for area in (*window.screen.areas, *window.global_areas)):
            with bpy.context.temp_override(window=located.window, area=located.area):
                if self.matches(located.area):
                    matches.append(located)
        return tuple(sorted(matches, key=lambda located: -(located.area.width * located.area.height)))

    def first(self, windows: Iterable[bpy.types.Window]) -> Outcome[Located]:
        """Returns largest matching area, or fault naming editor."""
        return Block.of_seq(self.located(windows)).try_head().to_result_with(partial(Fault.of, BlenderReason.ABSENT, "WindowManager.windows", member=self.ui_type.default_value(self.space)))


class Split(msgspec.Struct, frozen=True, kw_only=True):
    """Editor split off at an edge, under half of the source span, its state applied on the next pass."""

    side: Edge
    share: Annotated[float, msgspec.Meta(ge=sys.float_info.min, lt=0.5)]
    ui_type: str
    state: EditorState = EditorState()


class Retype(msgspec.Struct, frozen=True, kw_only=True):
    """Editor type change, its state applied on the next pass."""

    ui_type: str
    state: EditorState = EditorState()


class Arrangement(msgspec.Struct, frozen=True, kw_only=True):
    """Steps applied to one area one host pass apart, then an optional maximization, holding the writes that revert each."""

    at: Located
    steps: tuple[Step, ...] = ()
    use_hide_panels: Option[bool] = Nothing
    undo: tuple[Callable[[], Outcome[None]], ...] = ()

    def advanced(self) -> Outcome[Option[Self]]:
        """Applies the next step on the live area, Nothing once every step and the maximization applied."""
        return Result[Located, Faults].bind(
            Located.found(self.at.window, self.at.area.as_pointer()).to_result_with(partial(Fault.of, BlenderReason.ABSENT, repr(self.at.window.screen), member=str(self.at.area.as_pointer()))),
            self.stepped,
        )

    def stepped(self, at: Located) -> Outcome[Option[Self]]:
        """Applies the head step, a retype or split leaving its state as the next step and a nested arrangement merging its reverts once settled."""
        window = at.window

        def kept(undo: Callable[[], Outcome[None]], *following: Step) -> Option[Self]:
            return Some(msgspec.structs.replace(self, steps=(*following, *self.steps[1:]), undo=(*self.undo, undo)))

        def following(state: EditorState) -> tuple[EditorState, ...]:
            return (state,) if state.values else ()

        match self.steps:
            case (EditorState() as state, *_):
                return state.applied(at.area).map(
                    lambda prior: prior.map(lambda held: kept(partial(_reached, window, at.area.as_pointer(), lambda live: held.applied(live.area).map(lambda _: None)))).default_value(Some(self))
                )
            case (Retype(ui_type=ui_type, state=state), *_):
                prior = at.area.ui_type
                return written(at.area, "ui_type", ui_type).map(
                    lambda _: kept(partial(_reached, window, at.area.as_pointer(), lambda live: written(live.area, "ui_type", prior).map(lambda _: None)), *following(state))
                )
            case (Split(side=side, share=share, ui_type=ui_type, state=state), *_):
                before = frozenset(area.as_pointer() for area in window.screen.areas)
                return (
                    at
                    .run(bpy.ops.screen.area_split, {"direction": "HORIZONTAL" if side in {"TOP", "BOTTOM"} else "VERTICAL", "factor": 1.0 - share if side in {"TOP", "RIGHT"} else share})
                    .bind(
                        lambda _: (
                            Block
                            .of_seq(window.screen.areas)
                            .filter(lambda area: area.as_pointer() not in before)
                            .try_head()
                            .to_result_with(partial(Fault.of, BlenderReason.ABSENT, repr(window.screen), member="screen.area_split"))
                        )
                    )
                    .bind(
                        lambda area: written(area, "ui_type", ui_type).map(
                            lambda _: kept(
                                partial(_reached, window, area.as_pointer(), lambda live: live.run(bpy.ops.screen.area_close, {})),
                                *((Arrangement(at=Located(window=window, area=area), steps=(state,)),) if state.values else ()),
                            )
                        )
                    )
                )
            case (Arrangement() as nested, *rest):
                return nested.advanced().bind(
                    lambda moved: moved.map(lambda held: Ok(Some(msgspec.structs.replace(self, steps=(held, *rest))))).default_with(
                        lambda: msgspec.structs.replace(self, steps=tuple(rest), undo=(*self.undo, *nested.undo)).stepped(at)
                    )
                )
            case _:
                return self.use_hide_panels.map(
                    lambda hidden: at.run(bpy.ops.screen.screen_full_area, {"use_hide_panels": hidden}).map(
                        lambda _: Some(
                            msgspec.structs.replace(
                                self,
                                use_hide_panels=Nothing,
                                undo=(*self.undo, lambda: called(bpy.ops.screen.back_to_previous, {}, override=Some(bpy.context.temp_override(window=window))).map(lambda _: None)),
                            )
                        )
                    )
                ).default_value(Ok(Nothing))

    def reverted(self) -> Outcome[None]:
        """Reverts applied steps in reverse order, accumulating refusals."""
        return accumulated(undo() for undo in reversed(self.undo)).map(lambda _: None)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _reached(window: bpy.types.Window, pointer: int, revert: Callable[[Located], Outcome[None]]) -> Outcome[None]:
    """Reverts the area by session pointer while it lives, Ok once removed."""
    return Located.found(window, pointer).map(revert).default_value(Ok(None))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["AreaQuery", "Arrangement", "Edge", "Located", "Retype", "Split", "Step"]
