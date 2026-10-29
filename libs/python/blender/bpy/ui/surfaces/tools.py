"""Workspace tools registered into a toolbar with their keymap, widget, cursor drawing, header settings, and the extra settings popover."""

from collections.abc import Callable, Generator
from contextlib import contextmanager, ExitStack
from functools import partial, reduce
from pathlib import Path
from typing import Literal, TYPE_CHECKING

from bl_ui.space_toolsystem_common import ToolDef
import bpy
from expression import Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.calls import Command, idname, Operation, Placement
from blender.bpy.commands.invocations import Pick
from blender.bpy.commands.keymaps import Bindings, resolved, Row
from blender.bpy.commands.operators import Operator
from blender.bpy.data.rna.paths import definition, Struct
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Band
from blender.bpy.ui.drawing.handlers import Drawing, Drew
from blender.bpy.ui.layout.rows import acquired, realized, RowContent, RowDraw
from blender.bpy.ui.resources.icons import Icons
from blender.bpy.ui.surfaces.gizmos import Gizmos
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired, Reason

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type _Draws = tuple[Option[RowDraw], Option[RowDraw], Option[RowDraw]]

# --- [MODELS] ---------------------------------------------------------------------------


class Tool[S = None](msgspec.Struct, frozen=True, kw_only=True):
    """Toolbar tool labelled by its key, running an operator, with its icon, key bindings in the tool keymap, widget group, cursor drawing, and header settings.

    `icon` names a stock geometry icon (`ops.generic.select_circle`) or a staged `.dat` geometry icon, the only icon form the toolbar loads.
    `settings` draw over the operator's tool properties, every visible one while none is declared, `widget_settings` over the widget group's properties, and `extra_settings` in the host's extra options popover the header opens.
    A brush type narrows a brush tool's brushes and requires `USE_BRUSHES`.
    """

    key: Key
    operator: Command | Operation
    icon: Path | str
    bindings: Bindings = ()
    description: Option[str | Callable[[bpy.types.Context, ToolDef, bpy.types.KeyMap | None], str]] = Nothing
    cursor: "Option[rna_enums.WindowCursorItems]" = Nothing
    widget: Option[Gizmos | type[bpy.types.GizmoGroup]] = Nothing
    widget_properties: tuple[tuple[str, object], ...] = ()
    pointer: Option[Drawing[S]] = Nothing
    options: frozenset[Literal["KEYMAP_FALLBACK", "USE_BRUSHES"]] = frozenset()
    brush_type: Option[str] = Nothing
    data_block: Option[str] = Nothing
    settings: tuple[RowContent, ...] = ()
    widget_settings: tuple[RowContent, ...] = ()
    extra_settings: tuple[RowContent, ...] = ()

    def __post_init__(self) -> None:
        """Refuses a brush type on a tool that offers no brushes, and a staged icon the toolbar cannot load."""
        if self.brush_type.is_some() and "USE_BRUSHES" not in self.options:
            raise ValueError(self.key)
        if isinstance(self.icon, Path) and self.icon.suffix != ".dat":
            raise ValueError(self.icon)


class Tools[S = None](msgspec.Struct, frozen=True, kw_only=True):
    """Tools of one toolbar space and context mode, the first after a tool `after` names or at the end, each later tool after its predecessor.

    `group` opens a flyout group led by the first tool, and `separator` draws a separator before the entry.
    """

    space: "rna_enums.SpaceTypeItems"
    tools: tuple[Tool[S], *tuple[Tool[S], ...]]
    mode: Option[str] = Nothing
    after: frozenset[str] = frozenset()
    separator: bool = False
    group: bool = False

    # --- [ACTIVATION]

    def activation(self, identity: Identity, tool: Tool[S], *, cycle: bool = False, as_fallback: bool = False) -> Outcome[Placement]:
        """Returns the `wm.tool_set_by_id` placement activating the tool in its space."""
        flags = {flag: True for flag, stated in (("cycle", cycle), ("as_fallback", as_fallback)) if stated}
        return identity.name(bpy.types.Operator, tool.key).map(lambda name: Placement(bpy.ops.wm.tool_set_by_id, {"name": name, "space_type": self.space, **flags}))

    # --- [REGISTRATION]

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns what every tool holds, then the toolbar registration, each tool class built once the interface band opens and its settings realize."""
        pointers = tuple(tool.pointer.map(lambda drawing: drawing.acquired(identity)) for tool in self.tools)
        held = tuple(
            row
            for tool, pointed in zip(self.tools, pointers, strict=True)
            for row in (
                *(widget for widget in tool.widget.to_list() if isinstance(widget, Gizmos)),
                *(row for rows, _ in pointed.to_list() for row in rows),
                *acquired((*tool.settings, *tool.widget_settings, *tool.extra_settings)),
            )
        )
        return Ok((*held, (Band.INTERFACE, _registered(self, identity, tuple(pointed.map(lambda pair: pair[1]) for pointed in pointers)))))

    def drafted(self, identity: Identity, tool: Tool[S], pointer: Option[Drew]) -> Outcome[type[bpy.types.WorkSpaceTool]]:
        """Builds the tool class once its names, keymap rows, widget, and settings resolve."""
        match tool.operator:
            case Operator(invoke=Option(tag="some", some=Pick(cursor=picked))):
                pending: Option[rna_enums.WindowCursorItems] = Some(picked)
            case _:
                pending = Nothing
        widget = tool.widget.map(lambda held: (held.named(identity) if isinstance(held, Gizmos) else Ok(definition(held).identifier)).map(Some)).default_value(Ok(Nothing))

        def realizing(rows: tuple[RowContent, ...]) -> Outcome[Option[RowDraw]]:
            """Realizes declared rows into one draw, Nothing while none is declared."""
            return realized(rows, identity).map(Some) if rows else Ok(Nothing)

        def built(name: str, operator: str, rows: tuple[Row, ...], group: Option[str], draws: _Draws) -> type[bpy.types.WorkSpaceTool]:
            native = {
                "bl_space_type": self.space,
                "bl_context_mode": self.mode.to_optional(),
                "bl_idname": name,
                "bl_label": bpy.path.display_name(tool.key),
                "bl_description": tool.description.to_optional(),
                "bl_icon": Icons.path(identity, tool.icon) if isinstance(tool.icon, Path) else tool.icon,
                "bl_cursor": tool.cursor.or_else(pending).to_optional(),
                "bl_widget": group.to_optional(),
                "bl_widget_properties": list(tool.widget_properties),
                "bl_options": set(tool.options),
                "bl_brush_type": tool.brush_type.to_optional(),
                "bl_data_block": tool.data_block.to_optional(),
                "bl_keymap": rows or None,
                "bl_operator": operator,
                "draw_settings": partial(_settings, operator, group, draws),
                "draw_cursor": pointer.map(lambda drew: partial(_pointed, drew)).to_optional(),
            }
            return type(name, (bpy.types.WorkSpaceTool,), native)

        drawn = paired(realizing(tool.settings), realizing(tool.widget_settings), realizing(tool.extra_settings))
        return paired(identity.name(bpy.types.Operator, tool.key), idname(identity, tool.operator), resolved(identity, tool.bindings), widget, drawn).map(lambda parts: built(*parts))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [CLASSES]


@contextmanager
def _registered[S](tools: Tools[S], identity: Identity, pointers: tuple[Option[Drew], ...]) -> Generator[Outcome[None]]:
    """Registers each tool in order for the scope after the one before, named by the `bl_idname` its class is built under, the host building its keymap in the add-on keyconfig, stopping at the first refusal."""
    with ExitStack() as stack:

        def placed(classes: tuple[type[bpy.types.WorkSpaceTool], ...], result: Outcome[None], order: int) -> Outcome[None]:
            cls, first = classes[order], order == 0
            after = (set(tools.after) or None) if first else {classes[order - 1].__name__}
            return result.bind(
                lambda _: (
                    catch(bpy.utils.register_tool, exception=Exception)(cls, after=after, separator=tools.separator and first, group=tools.group and first)
                    .map(lambda _: stack.callback(bpy.utils.unregister_tool, cls))
                    .map(lambda _: None)
                    .map_error(partial(Fault.of, Reason.UNREGISTERED, cls.__name__))
                )
            )

        drafts = accumulated(tools.drafted(identity, tool, pointer) for tool, pointer in zip(tools.tools, pointers, strict=True))
        yield drafts.bind(lambda classes: reduce(partial(placed, classes), range(len(classes)), Result[None, Faults](ok=None)))


# --- [DRAWING]


def _settings(operator: str, group: Option[str], draws: _Draws, context: bpy.types.Context, layout: bpy.types.UILayout, tool: bpy.types.WorkSpaceTool, *, extra: bool = False) -> None:
    """Draws the header settings, every visible tool operator property while none is declared, and opens the extra options popover while extra settings exist, or draws the extra settings inside that popover."""

    def every(layout: bpy.types.UILayout, properties: Struct, _context: bpy.types.Context) -> None:
        for prop in (held for held in properties.bl_rna.properties if not held.is_hidden):
            layout.prop(properties, prop.identifier)

    settings, widgeted, extras = draws
    properties = tool.operator_properties(operator)
    if extra:
        for draw in extras.to_list():
            draw(layout, properties, context)
        return
    settings.default_value(every)(layout, properties, context)
    for draw, held in ((draw, held) for draw in widgeted.to_list() for held in group.to_list()):
        draw(layout, tool.gizmo_group_properties(held), context)
    if extras.is_some():
        layout.popover("TOPBAR_PT_tool_settings_extra", text="...")


def _pointed(drew: Drew, _context: bpy.types.Context, _tool: bpy.types.WorkSpaceTool, xy: tuple[int, int]) -> None:
    """Draws the cursor drawing at the pointer in window pixels."""
    drew("POST_PIXEL", Some(xy))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Tool", "Tools"]
