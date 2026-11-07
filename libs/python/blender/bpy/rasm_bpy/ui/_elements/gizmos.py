"""Gizmo groups an extension registers."""

from collections.abc import Callable, Generator, Mapping
from contextlib import contextmanager, ExitStack
from functools import cache, partial
from types import MappingProxyType
from typing import Final, Literal, override, TYPE_CHECKING

import bpy
from expression import Option, Some
from expression.extra.result import catch
import mathutils
import msgspec

from rasm_bpy.commands._modal import _Press, Travel
from rasm_bpy.commands.keymaps import Location, nesting
from rasm_bpy.commands.operators import filled, Pairs, Placement
from rasm_bpy.data._extension.identity import Identity, Key
from rasm_bpy.data._extension.registration import _acquired, _Classes, _Tier, Acquisitions, Declaration
from rasm_bpy.data._rna.conditions import Condition, satisfied
from rasm_bpy.data._rna.descriptors import _assign
from rasm_bpy.data._rna.paths import _rna, Struct
from rasm_bpy.ui._layout.members import _resolved_at, Member, ResolvedMember
from rasm_bpy.ui._layout.rows import icon_prerequisites
from rasm_bpy.ui._resources.icons import Icon, Icons
from rasm_bpy.ui._resources.theme import _color, Shade
from rasm_records import accumulated, Fault, Nothing, Ok, Outcome, paired, Reason

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type _Value = float | tuple[float, ...]
type _Attach = Callable[[bpy.types.Gizmo, bpy.types.Context], Outcome[None]]
type _Handle[S] = Callable[[str, S, bpy.types.Gizmo], None]
type _Bound[S] = tuple[str, Member[type[Struct]] | ResolvedMember | _Handle[S]] | _Attach

# --- [CONSTANTS] ------------------------------------------------------------------------

_TARGETS: Final[Mapping[str, str]] = MappingProxyType({
    "GIZMO_GT_arrow_3d": "offset",
    "GIZMO_GT_dial_3d": "offset",
    "GIZMO_GT_move_3d": "offset",
    "GIZMO_GT_cage_2d": "matrix",
    "GIZMO_GT_cage_3d": "matrix",
})

# --- [MODELS] ---------------------------------------------------------------------------


class TargetHandler[V, S = bpy.types.Context](msgspec.Struct, frozen=True, kw_only=True):
    """Gizmo target read from the group's draw state and written through the live context, with its range."""

    get: Callable[[S], V]
    set: Callable[[bpy.types.Context, V], Outcome[None]]
    range: Option[Callable[[S], tuple[float, float]]] = Nothing

    def bound(self, identity: Identity) -> _Handle[S]:
        """Prepares native target binding from one draw state, reporting writes through the current context."""
        write = identity.reports.reported(self.set)

        def handled(key: str, state: S, gizmo: bpy.types.Gizmo) -> None:
            gizmo.target_set_handler(
                key,
                get=partial(self.get, state),
                set=lambda value: write(bpy.context, value).to_optional(),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                range=self.range.map(lambda extent: partial(extent, state)).to_optional(),
            )

        return handled


class Shape(msgspec.Struct, frozen=True, kw_only=True):
    """Custom gizmo type drawn and selected through one fixed shape, its float targets moved by `drag` from their values at press and restored on cancel."""

    key: Key
    primitive: Literal["POINTS", "LINES", "TRIS", "LINE_STRIP"]
    vertices: tuple[tuple[float, ...], ...]
    targets: Mapping[str, int] = MappingProxyType({})
    drag: Option[Callable[[bpy.types.Gizmo, bpy.types.Context, tuple[tuple[str, _Value], ...], Travel], Outcome[None]]] = Nothing

    def acquisitions(self, identity: Identity) -> Acquisitions:
        """Returns the gizmo type class drawing the shape, dragging through `drag` while one is stated."""
        batch = cache(partial(bpy.types.Gizmo.new_custom_shape, self.primitive, self.vertices))
        reports, targets, drags = identity.reports, self.targets, self.drag

        class Shaped(bpy.types.Gizmo):
            @override
            def draw(self, context: bpy.types.Context) -> None:
                self.draw_custom_shape(batch())

            @override
            def draw_select(self, context: bpy.types.Context, select_id: int | None = 0) -> None:
                self.draw_custom_shape(batch(), select_id=select_id)

        class Dragged(Shaped):
            __slots__ = ("initial", "press")

            @override
            def invoke(self, context: bpy.types.Context, event: bpy.types.Event) -> set["rna_enums.OperatorReturnItems"]:
                self.press = _Press(x=event.mouse_x, y=event.mouse_y, threshold=0)
                self.initial = tuple((target, self.target_get_value(target)) for target in targets if self.target_is_valid(target))
                return {"RUNNING_MODAL"}

            @override
            def modal(self, context: bpy.types.Context, event: bpy.types.Event, tweak: set[Literal["PRECISE", "SNAP"]] | None) -> set["rna_enums.OperatorReturnItems"]:
                tweaks: set[Literal["PRECISE", "SNAP"]] = tweak  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
                for drag in drags.to_list():
                    reports.faulted(drag(self, context, self.initial, self.press.travel(event, snapped="SNAP" in tweaks, precise="PRECISE" in tweaks)))
                return {"RUNNING_MODAL"}

            @override
            def exit(self, context: bpy.types.Context, cancel: bool | None) -> None:
                cancelled: bool = cancel  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
                for target, value in self.initial if cancelled else ():
                    self.target_set_value(target, value)  # type: ignore[call-arg]  # ty: ignore[too-many-positional-arguments]

        base = Dragged if drags.is_some() else Shaped
        declared = tuple({"id": target, "type": "FLOAT", "array_length": length} for target, length in targets.items())
        name = identity.name(bpy.types.Gizmo, self.key)
        return (type(name, (base,), {"bl_idname": name, "bl_target_properties": declared}),)  # ty: ignore[unsupported-dynamic-base]


class Gizmo[S = bpy.types.Context](msgspec.Struct, frozen=True, kw_only=True):
    """Gizmo of a stock type or custom shape placed each redraw, bound to a member, a target handler, or one operator per highlighted part.

    `role` gives the color with alpha at rest and highlighted, read each draw.
    `properties` set every other gizmo member and type property alike, the host forwarding a type property through the gizmo.
    Member or handler binding pushes one undo step per use, and `prepare` runs as the gizmo is pressed.
    """

    kind: str | Shape
    place: Callable[[S], Outcome[Option[mathutils.Matrix]]]
    bind: Member | TargetHandler[float, S] | TargetHandler[tuple[float, ...], S] | tuple[Placement, ...] = ()
    properties: Mapping[str, object] = MappingProxyType({})
    icon: Option[Icon] = Nothing
    role: Callable[[bpy.types.ThemeUserInterface], tuple[Shade, Shade]] = lambda ui: ((*_color(ui.gizmo_primary)[:3], 0.5), ui.gizmo_hi)
    prepare: Option[Callable[[bpy.types.Context, bpy.types.Gizmo], Outcome[None]]] = Nothing

    def bound(self, identity: Identity) -> Outcome[_Bound[S]]:
        """Resolves the binding into a member with its target name or the attach of a handler or operators, absent fault for a member or handler on a kind without target."""
        named = Option.of_optional(next(iter(self.kind.targets), None) if isinstance(self.kind, Shape) else _TARGETS.get(self.kind)).to_result_with(
            lambda: Fault.of(Reason.ABSENT, repr(self.kind), member="target")
        )
        match self.bind:
            case tuple(placements):
                return Ok(partial(_operated, tuple(placement.resolved(identity) for placement in placements)))
            case Member() as member:
                return paired(named, member.located(identity).bind(lambda found: found.bound()))
            case TargetHandler() as handler:
                handled = handler.bound(identity)
                return named.map(lambda key: (key, handled))


class GizmoGroup[S = bpy.types.Context](msgspec.Struct, frozen=True, kw_only=True):
    """Gizmo group of parts in one editor region, labelled by its key, polled by its condition and every member owner it binds.

    `read` supplies one state per draw to every placement and handler getter; absence or faults hide every part.
    Group without `PERSISTENT` shows while a tool names it or an `ensured` scope stands, and `keymap` names the keymap handling its events in place of the host's generic one.
    """

    # --- [STATE]

    key: Key
    space: "rna_enums.SpaceTypeItems"
    gizmos: tuple[Gizmo[S], ...]
    read: Callable[[bpy.types.Context], Outcome[Option[S]]]
    region: "rna_enums.RegionTypeItems" = "WINDOW"
    condition: Option[Condition] = Nothing
    keymap: Option[str] = Nothing
    options: frozenset[Literal["3D", "SCALE", "DEPTH_3D", "SELECT", "PERSISTENT", "SHOW_MODAL_ALL", "EXCLUDE_MODAL", "TOOL_INIT", "TOOL_FALLBACK_KEYMAP", "VR_REDRAWS"]] = frozenset({"PERSISTENT"})

    # --- [REGISTRATION]

    @property
    def prerequisites(self) -> tuple[Declaration, ...]:
        """Icon sets, member attachments, and commands the parts hold."""
        return tuple(held for part in self.gizmos for held in (*icon_prerequisites(part.icon), *_acquired(part.bind if isinstance(part.bind, tuple) else (part.bind,))))

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns each custom shape, then the group class built when the interface tier opens and its icons resolve."""
        kinds = tuple(identity.name(bpy.types.Gizmo, part.kind.key) if isinstance(part.kind, Shape) else part.kind for part in self.gizmos)
        bound = accumulated(part.bound(identity) for part in self.gizmos)
        located = self.keymap.map(lambda name: nesting(name).map(lambda found: Some(found[0]))).default_value(Ok(Nothing))
        shapes = tuple(part.kind for part in self.gizmos if isinstance(part.kind, Shape))
        return paired(bound, located).map(lambda held: (*shapes, (_Tier.INTERFACE, _Classes(build=partial(_grouped, self, identity, identity.name(bpy.types.GizmoGroup, self.key), kinds, *held)))))

    # --- [LINKING]

    @contextmanager
    def ensured(self, identity: Identity) -> Generator[Outcome[None]]:
        """Links a group without `PERSISTENT` into every region of its type for the scope, refused for a persistent or unregistered group."""
        manager: bpy.types.WindowManager = bpy.context.window_manager  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        name = identity.name(bpy.types.GizmoGroup, self.key)
        with ExitStack() as stack:
            yield (
                catch(manager.gizmo_group_type_ensure, exception=RuntimeError)(name)
                .map(lambda _: stack.callback(manager.gizmo_group_type_unlink_delayed, name))
                .map(lambda _: None)
                .map_error(partial(Fault.of, Reason.REFUSED, name))
            )


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [CLASSES]


def _grouped[S](group: GizmoGroup[S], identity: Identity, name: str, kinds: tuple[str, ...], bound: tuple[_Bound[S], ...], keymap: Option[Location]) -> Outcome[tuple[type[bpy.types.GizmoGroup]]]:
    """Builds the group class once every icon resolves, rebinding handlers at each draw since the host caches their values until a rebind."""
    reports, parts = identity.reports, group.gizmos
    members = tuple(member for binding in bound if isinstance(binding, tuple) and isinstance(member := binding[1], Member | ResolvedMember))

    def rebound(gizmo: bpy.types.Gizmo, key: str, member: Member[type[Struct]] | ResolvedMember, context: bpy.types.Context) -> Outcome[None]:
        """Binds the target to the member's current owner, leaving it while the reach finds none."""
        return _resolved_at(member, context, context).bind(
            lambda found: found.starmap(
                lambda setting, at: catch(gizmo.target_set_prop, exception=RuntimeError)(key, *at, index=setting.index.default_value(-1)).map_error(
                    partial(Fault.of, Reason.REFUSED, _rna(setting.path))
                )
            ).default_value(Ok(None))
        )

    def poll(_cls: type[bpy.types.GizmoGroup], context: bpy.types.Context) -> bool:
        if not satisfied(group.condition, context):
            return False
        found = reports.settled(accumulated(_resolved_at(member, context, context) for member in members))
        return found.map(lambda each: all(present.is_some() for present in each)).default_value(value=False)

    def refresh(owner: bpy.types.GizmoGroup, context: bpy.types.Context) -> None:
        reports.faulted(
            accumulated(
                rebound(gizmo, binding[0], member, context) for gizmo, binding in zip(owner.gizmos, bound) if isinstance(binding, tuple) and isinstance(member := binding[1], Member | ResolvedMember)
            )
        )

    def draw_prepare(owner: bpy.types.GizmoGroup, context: bpy.types.Context) -> None:
        preferences: bpy.types.Preferences = context.preferences  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        ui = preferences.themes[0].user_interface
        if (state := reports.settled(group.read(context)).default_value(Nothing)).is_none():
            for gizmo in owner.gizmos:
                gizmo.hide = True
        for snapshot in state.to_list():
            for gizmo, part, binding in zip(owner.gizmos, parts, bound):
                (red, green, blue, alpha), (lit_red, lit_green, lit_blue, lit_alpha) = map(_color, part.role(ui))
                gizmo.color, gizmo.alpha = (red, green, blue), alpha
                gizmo.color_highlight, gizmo.alpha_highlight = (lit_red, lit_green, lit_blue), lit_alpha
                placed = reports.settled(part.place(snapshot)).default_value(Nothing)
                gizmo.hide = placed.is_none()
                if placed.is_some() and isinstance(binding, tuple) and not isinstance(handle := binding[1], Member | ResolvedMember):
                    handle(binding[0], snapshot, gizmo)
                for matrix in placed.to_list():
                    gizmo.matrix_basis = matrix

    def invoke_prepare(owner: bpy.types.GizmoGroup, context: bpy.types.Context, gizmo: bpy.types.Gizmo) -> None:
        for prepare in (prepare for held, part in zip(owner.gizmos, parts) if held == gizmo for prepare in part.prepare.to_list()):
            reports.faulted(prepare(context, gizmo))

    def classed(icons: tuple[Option[int], ...]) -> tuple[type[bpy.types.GizmoGroup]]:
        def setup(owner: bpy.types.GizmoGroup, context: bpy.types.Context) -> None:
            made = tuple((owner.gizmos.new(kind), kind, part) for kind, part in zip(kinds, parts))
            written = accumulated(
                _assign(gizmo, member, value).map_error(partial(Fault.of, Reason.REFUSED, f"{kind}.{member}"))
                for (gizmo, kind, part), icon in zip(made, icons)
                for member, value in (("use_undo", isinstance(part.bind, Member | TargetHandler)), *(("icon_value", value) for value in icon.to_list()), *part.properties.items())
            )
            attached = accumulated(binding(gizmo, context) for (gizmo, _, _), binding in zip(made, bound) if not isinstance(binding, tuple))
            reports.faulted(paired(written, attached))

        native = {
            "bl_idname": name,
            "bl_label": bpy.path.display_name(group.key),
            "bl_space_type": group.space,
            "bl_region_type": group.region,
            "bl_options": set(group.options),
            "poll": classmethod(poll),
            "setup": setup,
            "refresh": refresh,
            "draw_prepare": draw_prepare,
            **({"invoke_prepare": invoke_prepare} if any(part.prepare.is_some() for part in parts) else {}),
            **keymap.map(lambda location: {"setup_keymap": classmethod(lambda _cls, keyconfig: keyconfig.keymaps.new(location[0], space_type=location[1], region_type=location[2]))}).default_value({}),
        }
        return (type(name, (bpy.types.GizmoGroup,), native),)

    return accumulated(Icons.value(identity, part.icon) for part in parts).map(classed)


# --- [TARGETS]


def _operated(placed: tuple[tuple[str, Pairs], ...], gizmo: bpy.types.Gizmo, _context: bpy.types.Context) -> Outcome[None]:
    """Sets one operator per highlighted part in placement order and fills its properties."""
    return accumulated(filled(each, gizmo.target_set_operator(each[0], index=index)) for index, each in enumerate(placed)).map(lambda _: None)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Gizmo", "GizmoGroup", "Shape", "TargetHandler"]
