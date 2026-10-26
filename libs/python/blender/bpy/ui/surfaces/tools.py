"""Workspace tools with toolbar placement, settings, and keymaps."""

from collections.abc import Callable, Generator
from contextlib import contextmanager, ExitStack
from pathlib import Path
from typing import ClassVar, Literal, TYPE_CHECKING

from bl_ui.space_toolsystem_common import ToolDef
from bl_ui.space_toolsystem_toolbar import IMAGE_PT_tools_active, NODE_PT_tools_active, SEQUENCER_PT_tools_active, VIEW3D_PT_tools_active
import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
import msgspec

from blender.bpy.commands.calls import Operation, Placed, Writer
from blender.bpy.commands.keymaps import Chord
from blender.bpy.commands.operators import Command, Operator, Pick
from blender.bpy.data.rna.descriptors import rooted
from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.extension.registration import Acquisitions, Band
from blender.bpy.faults import BlenderAbsentFault, BlenderCollisionFault, BlenderFaults, BlenderResult
from blender.bpy.ui.drawing.handlers import Drawing, Drew
from blender.bpy.ui.layout.rows import acquired, placed, RowContent, RowDraw
from blender.bpy.ui.resources.icons import Icons
from blender.bpy.ui.surfaces.gizmos import Linked
from blender.records.faults import accumulated, paired
from blender.records.fields import heading

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Toolbar = View3D | ImageEditor | NodeEditor | Sequencer
type Placement = Appended | Grouped

# --- [MODELS] ---------------------------------------------------------------------------


class View3D(msgspec.Struct, frozen=True, kw_only=True):
    """3D Viewport toolbar of one context mode."""

    panel: ClassVar[type[VIEW3D_PT_tools_active]] = VIEW3D_PT_tools_active
    mode: "rna_enums.ContextModeItems"


class ImageEditor(msgspec.Struct, frozen=True, kw_only=True):
    """Image Editor toolbar of one editor mode."""

    panel: ClassVar[type[IMAGE_PT_tools_active]] = IMAGE_PT_tools_active
    mode: "rna_enums.SpaceImageModeAllItems"


class NodeEditor(msgspec.Struct, frozen=True, kw_only=True):
    """Node Editor toolbar with no mode selector."""

    panel: ClassVar[type[NODE_PT_tools_active]] = NODE_PT_tools_active
    mode: ClassVar[None] = None


class Sequencer(msgspec.Struct, frozen=True, kw_only=True):
    """Sequencer toolbar of one view type."""

    panel: ClassVar[type[SEQUENCER_PT_tools_active]] = SEQUENCER_PT_tools_active
    mode: "rna_enums.SpaceSequencerViewTypeItems"


class Appended(msgspec.Struct, frozen=True, kw_only=True):
    """Toolbar entry appended after separator when set."""

    separator: bool = False


class Grouped(msgspec.Struct, frozen=True, kw_only=True):
    """Toolbar entry joining anchor tool's group."""

    anchor: str


class Draft(msgspec.Struct, frozen=True, kw_only=True):
    """Tool class with key bindings and acquisitions."""

    tool: type[bpy.types.WorkSpaceTool]
    items: tuple[tuple[Chord, tuple[str, Writer]], ...]
    rows: Acquisitions


class Tool[S = None](msgspec.Struct, frozen=True, kw_only=True):
    """Toolbar tool with operator, icon, cursor, and settings."""

    key: Key
    title: str
    operator: Command | Operation
    icon: Path
    bindings: dict[Chord, Placed] = msgspec.field(default_factory=dict)
    description: Option[str | Callable[[bpy.types.Context, ToolDef, bpy.types.KeyMap | None], str]] = Nothing
    cursor: "Option[rna_enums.WindowCursorItems]" = Nothing
    gizmo: Option[Linked] = Nothing
    pointer: Option[Drawing[S]] = Nothing
    options: frozenset[Literal["KEYMAP_FALLBACK", "USE_BRUSHES"]] = frozenset()
    widget_properties: tuple[tuple[str, object], ...] = ()
    data_block: Option[str] = Nothing
    settings: tuple[RowContent, ...] = ()

    def drafted(self, identity: Identity, toolbar: Toolbar) -> BlenderResult[Draft]:
        """Builds tool class once names, bindings, gizmo, and cursor drawing resolve."""
        panel, mode = toolbar.panel, toolbar.mode
        match self.operator:
            case Operator(invoke=Option(tag="some", some=Pick(cursor=picked))):
                pending: Option[rna_enums.WindowCursorItems] = Some(picked)
            case _:
                pending = Nothing
        named = identity.name(Name.OPERATOR, self.operator.key) if isinstance(self.operator, Command) else Ok(self.operator.idname_py())  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        widget = self.gizmo.map(lambda linked: linked.named(identity).map(Some)).default_value(Ok(Nothing))
        pointed = self.pointer.map(lambda drawing: drawing.acquired(identity).map(Some)).default_value(Ok(Nothing))
        bound = accumulated(placed.invocation(identity).map(lambda invocation: (chord, invocation)) for chord, placed in self.bindings.items())

        def built(parts: tuple[tuple[tuple[str, str], tuple[RowDraw, ...]], tuple[tuple[Option[str], Option[tuple[Acquisitions, Drew]]], tuple[tuple[Chord, tuple[str, Writer]], ...]]]) -> Draft:
            """Returns tool draft."""
            ((name, operator), settings), ((gizmo, pointer), items) = parts

            def every(layout: bpy.types.UILayout, properties: "bpy.types.bpy_struct[object]", _context: bpy.types.Context) -> None:
                """Draws every visible tool operator property."""
                for prop in (held for held in properties.bl_rna.properties if not held.is_hidden):
                    layout.prop(properties, prop.identifier)

            def shown(context: bpy.types.Context, layout: bpy.types.UILayout, tool: bpy.types.WorkSpaceTool) -> None:
                """Draws settings rows over tool operator properties, every visible property while none is declared."""
                for draw in settings or (every,):
                    draw(layout, tool.operator_properties(operator), context)

            return Draft(
                tool=type(
                    name,
                    (bpy.types.WorkSpaceTool,),
                    {
                        "bl_space_type": panel.bl_space_type,
                        "bl_context_mode": mode,
                        "bl_idname": name,
                        "bl_label": self.title,
                        "bl_description": self.description.to_optional(),
                        "bl_icon": Icons.path(identity, self.icon),
                        "bl_cursor": self.cursor.or_else(pending).to_optional(),
                        "bl_widget": gizmo.to_optional(),
                        "bl_options": set(self.options),
                        "bl_widget_properties": list(self.widget_properties),
                        "bl_data_block": self.data_block.to_optional(),
                        "bl_keymap": () if items else None,
                        "bl_operator": operator,
                        "draw_settings": shown,
                        "draw_cursor": pointer.map(lambda acquired: lambda _context, _tool, xy: acquired[1](Some(xy))).to_optional(),
                    },
                ),
                items=items,
                rows=(*self.gizmo.to_list(), *(row for rows, _ in pointer.to_list() for row in rows), *acquired(self.settings)),
            )

        return paired(paired(paired(identity.name(Name.OPERATOR, self.key), named), placed(self.settings, identity)), paired(paired(widget, pointed), bound)).map(built)


class Tools[S = None](msgspec.Struct, frozen=True, kw_only=True):
    """Toolbar entry, or group led by first tool."""

    toolbar: Toolbar
    tools: tuple[Tool[S], *tuple[Tool[S], ...]]
    placement: Placement = Appended()

    # --- [ACTIVATION]

    def invocation(self, identity: Identity, tool: Tool[S], *, cycle: bool = False, as_fallback: bool = False) -> BlenderResult[tuple[Operation, dict[str, object]]]:
        """Returns `wm.tool_set_by_id` call activating tool."""
        return identity.name(Name.OPERATOR, tool.key).map(
            lambda name: (bpy.ops.wm.tool_set_by_id, {"name": name, "space_type": self.toolbar.panel.bl_space_type, "cycle": cycle, "as_fallback": as_fallback})
        )

    # --- [REGISTRATION]

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Builds tool registration once every tool resolves."""
        return accumulated(tool.drafted(identity, self.toolbar) for tool in self.tools).map(
            lambda drafts: (*(row for draft in drafts for row in draft.rows), (Band.INTERFACE, self.registered(drafts)))
        )

    @contextmanager
    def registered(self, drafts: tuple[Draft, ...]) -> Generator[BlenderResult[None]]:
        """Registers tools and keymap items for the scope."""
        panel, mode = self.toolbar.panel, self.toolbar.mode
        owner = f"{panel.__name__}._tools[{mode!r}]"
        context = "All" if mode is None else heading(mode)
        bindings = tuple((draft, f"{panel.keymap_prefix} {context}, {tool.title}") for tool, draft in zip(self.tools, drafts))

        def planned(tools: list[object]) -> BlenderResult[Callable[[type[bpy.types.WorkSpaceTool]], None]]:
            """Checks anchor presence and tool ID collisions."""
            grouped = frozenset(member.idname for item in tools if isinstance(item, tuple) and not isinstance(item, ToolDef) for member in item if isinstance(member, ToolDef))
            match self.placement:
                case Appended(separator=separator):
                    lead = Result[Callable[[type[bpy.types.WorkSpaceTool]], None], BlenderFaults](ok=lambda tool: bpy.utils.register_tool(tool, separator=separator, group=len(drafts) > 1))
                case Grouped(anchor=anchor):
                    lead = (
                        Result[Callable[[type[bpy.types.WorkSpaceTool]], None], BlenderFaults](ok=lambda tool: bpy.utils.register_tool(tool, after={anchor}))
                        if anchor in grouped
                        else Result[Callable[[type[bpy.types.WorkSpaceTool]], None], BlenderFaults](error=(BlenderAbsentFault(owner=owner, key=anchor),))
                    )
            ids = grouped | frozenset(item.idname for item in tools if isinstance(item, ToolDef))
            return paired(
                lead, accumulated(Result[None, BlenderFaults](error=(BlenderCollisionFault(request=draft.tool.__name__, error=owner),)) for draft in drafts if draft.tool.__name__ in ids)
            ).map(lambda pair: pair[0])

        def configured(manager: bpy.types.WindowManager) -> BlenderResult[tuple[bpy.types.WindowManager, bpy.types.KeyConfig]]:
            """Resolves keyconfigs, refusing taken keymap names."""
            configs = manager.keyconfigs
            addon, default = Option.of_optional(configs.addon), Option.of_optional(configs.default)
            maps = tuple(pair for present in (addon, default) for config in present.to_list() for pair in config.keymaps.items())
            occupied = accumulated(
                Result[None, BlenderFaults](error=(BlenderCollisionFault(request=keymap_name, error=repr(keymap)),))
                for draft, keymap_name in bindings
                if draft.items
                for name, keymap in maps
                if name == keymap_name
            )
            available: BlenderResult[bpy.types.KeyConfig] = addon.to_result_with(lambda: (BlenderAbsentFault(owner="WindowManager.keyconfigs", key="addon"),))
            native: BlenderResult[bpy.types.KeyConfig] = default.to_result_with(lambda: (BlenderAbsentFault(owner="WindowManager.keyconfigs", key="default"),))
            return paired(occupied, paired(available, native)).map(lambda values: (manager, values[1][0]))

        binding: BlenderResult[Option[tuple[bpy.types.WindowManager, bpy.types.KeyConfig]]] = (
            rooted(bpy.types.WindowManager, bpy.context).bind(configured).map(Some) if any(draft.items for draft in drafts) else Ok(Nothing)
        )
        tools = dict(panel.tools_all())  # type: ignore[call-overload]  # ty: ignore[no-matching-overload]
        leading = paired(
            Option
            .of_optional(tools.get(mode))
            .map(planned)
            .default_with(lambda: Result[Callable[[type[bpy.types.WorkSpaceTool]], None], BlenderFaults](error=(BlenderAbsentFault(owner=f"{panel.__name__}._tools", key=str(mode)),))),
            binding,
        )
        with ExitStack() as resources:
            for lead, _ in leading.to_option().to_list():
                first, *rest = drafts
                lead(first.tool)
                resources.callback(bpy.utils.unregister_tool, first.tool)
                for draft in rest:
                    bpy.utils.register_tool(draft.tool, after={first.tool.__name__})
                    resources.callback(bpy.utils.unregister_tool, draft.tool)
            yield leading.bind(
                lambda acquired: accumulated(
                    Option.of_optional(item.properties).map(write).default_with(lambda: Error((BlenderAbsentFault(owner=repr(item), key="properties"),)))
                    for manager, config in acquired[1].to_list()
                    for draft, keymap_name in bindings
                    for chord, (operator, write) in draft.items
                    for item in chord.added(manager, config.keymaps[keymap_name], operator)
                )
            ).map(lambda _: None)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Appended", "Draft", "Grouped", "ImageEditor", "NodeEditor", "Placement", "Sequencer", "Tool", "Toolbar", "Tools", "View3D"]
