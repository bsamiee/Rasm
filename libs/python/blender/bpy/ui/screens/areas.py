"""Located editors, editor queries, revertable editor state, node editor state, and screen arrangements applied one host pass per step."""

from collections.abc import Callable, Iterable, Mapping
from functools import partial, reduce
from typing import Literal, Self, TYPE_CHECKING

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.collections import Block
import msgspec

from blender.bpy.commands.calls import called, Operation, Setting
from blender.bpy.data.rna.conditions import Condition
from blender.bpy.data.rna.descriptors import Descriptor, Value, written
from blender.bpy.data.rna.paths import Native, RnaPath, rooted, Struct, struct_at
from blender.bpy.faults import BlenderReason
from blender.bpy.ui.layout.members import admits
from blender.records.faults import accumulated, Fault, Outcome, paired

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Edge = Literal["LEFT", "RIGHT", "TOP", "BOTTOM"]
type Step = Split | Retype | EditorState | Arrangement
type _Written = tuple[tuple[tuple[Native, Descriptor[bpy.types.Property], Value | Struct], ...], Outcome[None]]

# --- [MODELS] ---------------------------------------------------------------------------


class Located(msgspec.Struct, frozen=True, kw_only=True):
    """Window, area, and optional region of one host call, the members `Context.temp_override` takes."""

    window: bpy.types.Window
    area: bpy.types.Area
    region: Option[bpy.types.Region] = Nothing

    @classmethod
    def current(cls, context: bpy.types.Context) -> Outcome[Self]:
        """Returns context window, area, and region, accumulating absences of the first two."""
        return paired(rooted(bpy.types.Window, context), rooted(bpy.types.Area, context)).map(lambda pair: cls(window=pair[0], area=pair[1], region=Option.of_optional(context.region)))

    @classmethod
    def found(cls, window: bpy.types.Window, pointer: int) -> Option[Self]:
        """Returns live area by session pointer, Nothing once removed."""
        return Block.of_seq(areas((window,))).filter(lambda at: at.area.as_pointer() == pointer).try_head().map(lambda at: cls(window=at.window, area=at.area))

    def within(self, kind: "rna_enums.RegionTypeItems" = "WINDOW") -> Outcome[Self]:
        """Returns the area located at its first region of a type."""
        return region_in(self.area, kind).map(lambda region: msgspec.structs.replace(self, region=Some(region)))

    @property
    def override(self) -> bpy.types.ContextTempOverride:
        """Context override of window, area, and region when located."""
        return bpy.context.temp_override(window=self.window, area=self.area, region=self.region.to_optional())

    def run(self, operator: Operation, properties: Mapping[str, Setting | Option[Setting]], /, *, context: "rna_enums.OperatorContextItems" = "EXEC_DEFAULT") -> Outcome[None]:
        """Runs operator under the located override."""
        return called(operator, properties, context=context, override=Some(self.override)).map(lambda _: None)


class AreaQuery(msgspec.Struct, frozen=True, kw_only=True):
    """Active space class, `ui_type`, and active space condition."""

    space: type[bpy.types.Space]
    ui_type: Option[str] = Nothing
    shows: Option[Condition] = Nothing

    def matches(self, area: bpy.types.Area) -> bool:
        """Returns whether the area's active space is of the class, its `ui_type` matches, and the condition holds over that space."""
        active = area.spaces.active
        return isinstance(active, self.space) and all(area.ui_type == kind for kind in self.ui_type.to_list()) and admits(self.shows, bpy.context, Some(active))

    def located(self, windows: Iterable[bpy.types.Window]) -> tuple[Located, ...]:
        """Returns matching areas, largest first, each matched under its own override."""

        def matched(at: Located) -> bool:
            with at.override:
                return self.matches(at.area)

        return tuple(sorted(filter(matched, areas(windows)), key=lambda at: -(at.area.width * at.area.height)))

    def first(self, windows: Iterable[bpy.types.Window]) -> Outcome[Located]:
        """Returns largest matching area, or fault naming editor."""
        return Block.of_seq(self.located(windows)).try_head().to_result_with(partial(Fault.of, BlenderReason.ABSENT, "WindowManager.windows", member=self.ui_type.default_value(self.space.__name__)))


class EditorState(msgspec.Struct, frozen=True, kw_only=True):
    """RNA values in dependency order under the active space, or under a region of a type."""

    region: "Option[rna_enums.RegionTypeItems]" = Nothing
    values: tuple[tuple[Descriptor[bpy.types.Property], Value | bytes | Struct], ...] = ()

    @classmethod
    def of(cls, root: type[Struct], values: Mapping[RnaPath, Value | bytes | Struct], region: "Option[rna_enums.RegionTypeItems]" = Nothing) -> Outcome[Self]:
        """Resolves values in declaration order."""
        return accumulated(Descriptor.resolve(root, path, bpy.types.Property).map(lambda setting: (setting, value)) for path, value in values.items()).map(
            lambda pairs: cls(region=region, values=pairs)
        )

    def applied(self, area: bpy.types.Area) -> Outcome[Option[Self]]:
        """Writes values in order, rolling back on refusal or absent owner.

        Returns:
            Replaced values, Nothing while the base or an owner is absent.
        """
        base = self.region.map(lambda kind: region_in(area, kind).to_option()).default_with(lambda: Option.of_optional(area.spaces.active))

        def stepped(state: _Written, setting: Descriptor[bpy.types.Property], value: Value | bytes | Struct) -> Result[_Written, _Written]:
            match base.filter(lambda held: isinstance(held, setting.root)).bind(lambda held: struct_at(held, setting.owner)).map(lambda held: (held, setting.write(held, value))):
                case Option(some=(held, Result(tag="ok", ok=prior))):
                    return Ok(((*state[0], *((held, setting, replaced) for replaced in prior.to_list())), Ok(None)))
                case missed:
                    return Error((state[0], missed.map(lambda pair: pair[1].map(lambda _: None)).default_value(Ok(None))))

        done, stopped = (outcome := reduce(lambda state, pair: state.bind(lambda held: stepped(held, *pair)), self.values, Result[_Written, _Written](ok=((), Ok(None))))).merge()
        return (
            Ok(Some(msgspec.structs.replace(self, values=tuple((setting, prior) for _, setting, prior in reversed(done)))))
            if outcome.is_ok()
            else accumulated((stopped, *(descriptor.write(held, prior) for held, descriptor, prior in reversed(done)))).map(lambda _: Nothing)
        )


class NodeEditing(msgspec.Struct, frozen=True, kw_only=True):
    """Node editor cursor, edited tree, and selected active node."""

    cursor: tuple[float, float]
    tree: bpy.types.NodeTree
    active: Option[bpy.types.Node]

    @classmethod
    def of(cls, context: bpy.types.Context) -> Outcome[Self]:
        """Returns the node editor's state, or fault for a context holding no node editor or edited tree."""
        match context.space_data:
            case bpy.types.SpaceNodeEditor(cursor_location=cursor) as space:
                return (
                    Option
                    .of_optional(space.edit_tree)
                    .map(lambda tree: cls(cursor=(cursor.x, cursor.y), tree=tree, active=Option.of_optional(tree.nodes.active).filter(lambda node: node.select)))
                    .to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(space), member="edit_tree"))
                )
            case _:
                return Error(Fault.of(BlenderReason.ABSENT, repr(context), member="SpaceNodeEditor.WINDOW"))


class Split(msgspec.Struct, frozen=True, kw_only=True):
    """Editor split off at an edge, `share` of the source span inside (0, 0.5), its state applied on the next pass."""

    side: Edge
    share: float
    ui_type: str
    state: EditorState = EditorState()

    def __post_init__(self) -> None:
        """Refuses a share outside (0, 0.5), the smaller part `screen.area_split` makes the new area."""
        if not 0.0 < self.share < 1.0 - self.share:
            raise ValueError(self)


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
        return (
            Located
            .found(self.at.window, self.at.area.as_pointer())
            .to_result_with(partial(Fault.of, BlenderReason.ABSENT, repr(self.at.window.screen), member=str(self.at.area.as_pointer())))
            .bind(self.stepped)
        )

    def stepped(self, at: Located) -> Outcome[Option[Self]]:
        """Applies the head step, a retype or split leaving its state as the next step and a nested arrangement merging its reverts once settled."""
        window = at.window

        def kept(undo: Callable[[], Outcome[None]], *following: Step) -> Option[Self]:
            return Some(msgspec.structs.replace(self, steps=(*following, *self.steps[1:]), undo=(*self.undo, undo)))

        match self.steps:
            case (EditorState() as state, *_):
                return state.applied(at.area).map(
                    lambda prior: prior.map(lambda held: kept(partial(_reached, window, at.area.as_pointer(), lambda live: held.applied(live.area).map(lambda _: None)))).default_value(Some(self))
                )
            case (Retype(ui_type=ui_type, state=state), *_):
                prior = at.area.ui_type
                return written(at.area, ("ui_type",), ui_type).map(
                    lambda _: kept(partial(_reached, window, at.area.as_pointer(), lambda live: written(live.area, ("ui_type",), prior).map(lambda _: None)), *((state,) if state.values else ()))
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
                        lambda area: written(area, ("ui_type",), ui_type).map(
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


def areas(windows: Iterable[bpy.types.Window]) -> tuple[Located, ...]:
    """Returns every screen and global area of the windows."""
    return tuple(Located(window=window, area=area) for window in windows for area in (*window.screen.areas, *window.global_areas))


def region_in(area: bpy.types.Area, kind: "rna_enums.RegionTypeItems" = "WINDOW") -> Outcome[bpy.types.Region]:
    """Returns the area's first region of a type."""
    return Block.of_seq(area.regions).filter(lambda region: region.type == kind).try_head().to_result_with(partial(Fault.of, BlenderReason.ABSENT, repr(area), member=kind))


def _reached(window: bpy.types.Window, pointer: int, revert: Callable[[Located], Outcome[None]]) -> Outcome[None]:
    """Reverts the area by session pointer while it lives, Ok once removed."""
    return Located.found(window, pointer).map(revert).default_value(Ok(None))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["AreaQuery", "Arrangement", "Edge", "EditorState", "Located", "NodeEditing", "Retype", "Split", "Step", "areas", "region_in"]
