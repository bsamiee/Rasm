"""Workspace tools an extension registers into a toolbar."""

from collections.abc import Callable, Generator
from contextlib import contextmanager, ExitStack
from functools import partial, reduce
from pathlib import Path
from typing import Literal, TYPE_CHECKING

from bl_ui.space_toolsystem_common import ToolDef
from bl_ui.space_topbar import TOPBAR_PT_tool_settings_extra
import bpy
from expression import Option, Some
from expression.extra.result import catch
import msgspec

from rasm_bpy.commands.invocations import Pick
from rasm_bpy.commands.keymaps import Bindings, resolved, Row
from rasm_bpy.commands.operators import Command, idname, Operation, Operator, Placement
from rasm_bpy.data._extension.identity import Identity, Key
from rasm_bpy.data._extension.registration import _acquired, _Tier, Acquisitions
from rasm_bpy.data._rna.paths import _definition, Struct
from rasm_bpy.ui._drawing.handlers import DrawCallback, Drawing
from rasm_bpy.ui._elements.gizmos import GizmoGroup
from rasm_bpy.ui._layout.rows import _realized, RowContent, RowDraw
from rasm_bpy.ui._resources.icons import Icons
from rasm_records import accumulated, Fault, Nothing, Ok, Outcome, paired, Reason

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type _Draws = tuple[Option[RowDraw], Option[RowDraw], Option[RowDraw]]

# --- [MODELS] ---------------------------------------------------------------------------


class Tool[S = None](msgspec.Struct, frozen=True, kw_only=True):
    """Toolbar tool labelled by its key, running an operator, with its icon, key bindings in the tool keymap, widget group, cursor drawing, and header settings.

    `icon` names a stock geometry icon (`ops.generic.select_circle`) or a packaged `.dat` geometry icon, the only icon form the toolbar loads.
    `settings` draw over the operator's tool properties, every visible one while none is declared, `widget_settings` over the widget group's properties, and `extra_settings` in the host's extra options popover the header opens.
    Brush type narrows a brush tool's brushes and requires `USE_BRUSHES`.
    """

    key: Key
    operator: Command | Operation
    icon: Path | str
    bindings: Bindings = ()
    description: Option[str | Callable[[bpy.types.Context, ToolDef, bpy.types.KeyMap | None], str]] = Nothing
    cursor: "Option[rna_enums.WindowCursorItems]" = Nothing
    widget: Option[GizmoGroup | type[bpy.types.GizmoGroup]] = Nothing
    widget_properties: tuple[tuple[str, object], ...] = ()
    pointer: Option[Drawing[S]] = Nothing
    options: frozenset[Literal["KEYMAP_FALLBACK", "USE_BRUSHES"]] = frozenset()
    brush_type: Option[str] = Nothing
    data_block: Option[str] = Nothing
    settings: tuple[RowContent, ...] = ()
    widget_settings: tuple[RowContent, ...] = ()
    extra_settings: tuple[RowContent, ...] = ()


class Tools[S = None](msgspec.Struct, frozen=True, kw_only=True):
    """Tools of one toolbar space and context mode, the first after a tool `after` names or at the end, each later tool after its predecessor.

    `group` opens a flyout group led by the first tool, and `separator` draws a separator before the entry.
    """

    # --- [STATE]

    space: "rna_enums.SpaceTypeItems"
    tools: tuple[Tool[S], *tuple[Tool[S], ...]]
    mode: Option[str] = Nothing
    after: frozenset[str] = frozenset()
    separator: bool = False
    group: bool = False

    # --- [ACTIVATION]

    def activation(self, identity: Identity, tool: Tool[S], *, cycle: bool = False, as_fallback: bool = False) -> Placement:
        """Returns the `wm.tool_set_by_id` placement activating the tool in its space."""
        return Placement(bpy.ops.wm.tool_set_by_id, {"name": identity.name(bpy.types.Operator, tool.key), "space_type": self.space, "cycle": cycle, "as_fallback": as_fallback})

    # --- [REGISTRATION]

    def acquisitions(self, identity: Identity) -> Acquisitions:
        """Returns what every tool holds, then the toolbar registration, each tool class built once the interface tier opens and its settings realize."""
        pointers = tuple(tool.pointer.map(lambda drawing: drawing.acquired(identity)) for tool in self.tools)
        held = tuple(
            row
            for tool, pointed in zip(self.tools, pointers)
            for row in (
                *(widget for widget in tool.widget.to_list() if isinstance(widget, GizmoGroup)),
                *(row for rows, _ in pointed.to_list() for row in rows),
                *_acquired((tool.operator, *(placement for _, placement in tool.bindings), *tool.settings, *tool.widget_settings, *tool.extra_settings)),
            )
        )
        return (*held, (_Tier.INTERFACE, _registered(self, identity, tuple(pointed.starmap(lambda _rows, callback: callback) for pointed in pointers))))

    def tool_class(self, identity: Identity, tool: Tool[S], pointer: Option[DrawCallback]) -> Outcome[type[bpy.types.WorkSpaceTool]]:
        """Builds the tool class once its names, keymap rows, widget, and settings resolve."""
        match tool.operator:
            case Operator(invoke=Option(tag="some", some=Pick(cursor=picked))):
                pending: Option[rna_enums.WindowCursorItems] = Some(picked)
            case _:
                pending = Nothing
        widget = tool.widget.map(lambda held: identity.name(bpy.types.GizmoGroup, held.key) if isinstance(held, GizmoGroup) else _definition(held).identifier)

        def realizing(rows: tuple[RowContent, ...]) -> Outcome[Option[RowDraw]]:
            """Realizes declared rows into one draw, Nothing while none is declared."""
            return _realized(rows, identity).map(Some) if rows else Ok(Nothing)

        def built(rows: tuple[Row, ...], draws: _Draws) -> type[bpy.types.WorkSpaceTool]:
            name, operator = identity.name(bpy.types.Operator, tool.key), idname(identity, tool.operator)
            native = {
                "bl_space_type": self.space,
                "bl_context_mode": self.mode.to_optional(),
                "bl_idname": name,
                "bl_label": bpy.path.display_name(tool.key),
                "bl_description": tool.description.to_optional(),
                "bl_icon": Icons.path(identity, tool.icon) if isinstance(tool.icon, Path) else tool.icon,
                "bl_cursor": tool.cursor.or_else(pending).to_optional(),
                "bl_widget": widget.to_optional(),
                "bl_widget_properties": list(tool.widget_properties),
                "bl_options": set(tool.options),
                "bl_brush_type": tool.brush_type.to_optional(),
                "bl_data_block": tool.data_block.to_optional(),
                "bl_keymap": rows or None,
                "bl_operator": operator,
                "draw_settings": partial(_settings, operator, widget, draws),
                "draw_cursor": pointer.map(lambda callback: partial(_pointed, callback)).to_optional(),
            }
            return type(name, (bpy.types.WorkSpaceTool,), native)

        drawn = paired(realizing(tool.settings), realizing(tool.widget_settings), realizing(tool.extra_settings))
        return paired(resolved(identity, tool.bindings), drawn).map(lambda parts: built(*parts))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [CLASSES]


@contextmanager
def _registered[S](tools: Tools[S], identity: Identity, pointers: tuple[Option[DrawCallback], ...]) -> Generator[Outcome[None]]:
    """Registers each tool after the one before for the scope, stopping at the first refusal."""
    with ExitStack() as stack:

        def placed(classes: tuple[type[bpy.types.WorkSpaceTool], ...], result: Outcome[None], order: int) -> Outcome[None]:
            cls, first = classes[order], order == 0
            after = (set(tools.after) or None) if first else {classes[order - 1].bl_idname}  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            return result.bind(
                lambda _: (
                    catch(bpy.utils.register_tool, exception=Exception)(cls, after=after, separator=tools.separator and first, group=tools.group and first)
                    .map(lambda _: stack.callback(bpy.utils.unregister_tool, cls))
                    .map(lambda _: None)
                    .map_error(partial(Fault.of, Reason.UNREGISTERED, cls.__name__))
                )
            )

        drafts = accumulated(tools.tool_class(identity, tool, pointer) for tool, pointer in zip(tools.tools, pointers))
        yield drafts.bind(lambda classes: reduce(partial(placed, classes), range(len(classes)), Ok(None)))


# --- [DRAWING]


def _settings(operator: str, group: Option[str], draws: _Draws, context: bpy.types.Context, layout: bpy.types.UILayout, tool: bpy.types.WorkSpaceTool, *, extra: bool = False) -> None:
    """Draws the header settings and the extra options popover button, or the extra settings inside that popover."""

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
        layout.popover(TOPBAR_PT_tool_settings_extra.bl_idname, text="...")


def _pointed(callback: DrawCallback, _context: bpy.types.Context, _tool: bpy.types.WorkSpaceTool, xy: tuple[int, int]) -> None:
    """Draws the cursor drawing at the pointer in window pixels."""
    callback("POST_PIXEL", Some(xy))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Tool", "Tools"]
