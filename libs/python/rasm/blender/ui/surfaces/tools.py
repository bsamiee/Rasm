"""Native toolbar placement, tools, settings, and their registration lifetimes."""

from collections.abc import Callable, Generator
from contextlib import contextmanager, ExitStack
from pathlib import Path
from typing import ClassVar, Literal, TYPE_CHECKING

from bl_ui.space_toolsystem_common import ToolDef
from bl_ui.space_toolsystem_toolbar import IMAGE_PT_tools_active, NODE_PT_tools_active, SEQUENCER_PT_tools_active, VIEW3D_PT_tools_active
import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
import msgspec

from rasm.blender.commands.calls import Operation
from rasm.blender.commands.keymaps import Chord
from rasm.blender.commands.operators import Command, Operator, Pick, Placed, Writer
from rasm.blender.extension.identity import Identity, Key, Name
from rasm.blender.extension.registration import Acquisition, Acquisitions, Band
from rasm.blender.faults import BlenderAbsentFault, BlenderCollisionFault, BlenderFaults, BlenderResult
from rasm.blender.ui.drawing.handlers import Drawing, Drew
from rasm.blender.ui.resources.icons import Icons
from rasm.blender.ui.surfaces.gizmos import Linked
from rasm.imaging.faults import accumulated, paired

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Toolbar = View3D | ImageEditor | NodeEditor | Sequencer
type Placement = Appended | Grouped

# --- [MODELS] ---------------------------------------------------------------------------


class View3D(msgspec.Struct, frozen=True, kw_only=True):
    """3D Viewport toolbar of one native context mode."""

    panel: ClassVar[type[VIEW3D_PT_tools_active]] = VIEW3D_PT_tools_active
    mode: "rna_enums.ContextModeItems"


class ImageEditor(msgspec.Struct, frozen=True, kw_only=True):
    """Image Editor toolbar of one native editor mode."""

    panel: ClassVar[type[IMAGE_PT_tools_active]] = IMAGE_PT_tools_active
    mode: "rna_enums.SpaceImageModeAllItems"


class NodeEditor(msgspec.Struct, frozen=True, kw_only=True):
    """Node Editor toolbar, which has no mode selector."""

    panel: ClassVar[type[NODE_PT_tools_active]] = NODE_PT_tools_active
    mode: ClassVar[None] = None


class Sequencer(msgspec.Struct, frozen=True, kw_only=True):
    """Sequencer toolbar of one native view type."""

    panel: ClassVar[type[SEQUENCER_PT_tools_active]] = SEQUENCER_PT_tools_active
    mode: "rna_enums.SpaceSequencerViewTypeItems"


class Appended(msgspec.Struct, frozen=True, kw_only=True):
    """Toolbar entry appended with an optional preceding separator."""

    separator: bool = False


class Grouped(msgspec.Struct, frozen=True, kw_only=True):
    """Toolbar entry joining the existing group containing its native anchor."""

    anchor: str


class Draft(msgspec.Struct, frozen=True, kw_only=True):
    """Generated native tool class, resolved bindings, and owned acquisitions."""

    tool: type[bpy.types.WorkSpaceTool]
    items: tuple[tuple[Chord, tuple[str, Writer]], ...]
    rows: Acquisitions


class Tool[S = None](msgspec.Struct, frozen=True, kw_only=True):
    """Native toolbar tool with its operator, icon, cursor, and settings."""

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
    widget_properties: list[tuple[str, object]] = msgspec.field(default_factory=list)
    data_block: Option[str] = Nothing
    settings: Option[Callable[[bpy.types.Context, bpy.types.UILayout, bpy.types.WorkSpaceTool], None]] = Nothing

    def drafted(self, identity: Identity, toolbar: Toolbar) -> BlenderResult[Draft]:
        """Builds the native class after resolving names, bindings, and pointer resources."""
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

        def built(parts: tuple[tuple[str, str], tuple[tuple[Option[str], Option[tuple[tuple[tuple[Band, Acquisition], ...], Drew]]], tuple[tuple[Chord, tuple[str, Writer]], ...]]]) -> Draft:
            """Combines resolved names, bindings, gizmos, and pointer drawing into a native tool."""
            (name, operator), ((gizmo, pointer), items) = parts

            def shown(_context: bpy.types.Context, layout: bpy.types.UILayout, tool: bpy.types.WorkSpaceTool) -> None:
                """Draws the active tool's visible operator properties."""
                properties = tool.operator_properties(operator)
                for prop in (held for held in properties.bl_rna.properties if not held.is_hidden):
                    layout.prop(properties, prop.identifier)

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
                        "bl_widget_properties": self.widget_properties,
                        "bl_data_block": self.data_block.to_optional(),
                        "bl_keymap": () if items else None,
                        "bl_operator": operator,
                        "draw_settings": self.settings.default_value(shown),
                        "draw_cursor": pointer.map(lambda acquired: lambda _context, _tool, xy: acquired[1](Some(xy))).to_optional(),
                    },
                ),
                items=items,
                rows=(*self.gizmo.to_list(), *(row for rows, _ in pointer.to_list() for row in rows)),
            )

        return paired(paired(identity.name(Name.OPERATOR, self.key), named), paired(paired(widget, pointed), bound)).map(built)


class Tools[S = None](msgspec.Struct, frozen=True, kw_only=True):
    """One toolbar entry or a group led by the first declared tool."""

    toolbar: Toolbar
    tools: tuple[Tool[S], *tuple[Tool[S], ...]]
    placement: Placement = Appended()

    # --- [ACTIVATION]

    def invocation(self, identity: Identity, tool: Tool[S], *, cycle: bool = False, as_fallback: bool = False) -> BlenderResult[tuple[Operation, dict[str, object]]]:
        """Builds Blender's native activation call for the tool and toolbar space."""
        return identity.name(Name.OPERATOR, tool.key).map(
            lambda name: (bpy.ops.wm.tool_set_by_id, {"name": name, "space_type": self.toolbar.panel.bl_space_type, "cycle": cycle, "as_fallback": as_fallback})
        )

    # --- [REGISTRATION]

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Builds native tool registration after every tool identity resolves."""
        return accumulated(tool.drafted(identity, self.toolbar) for tool in self.tools).map(
            lambda drafts: (*(row for draft in drafts for row in draft.rows), (Band.INTERFACE, self.registered(drafts)))
        )

    @contextmanager
    def registered(self, drafts: tuple[Draft, ...]) -> Generator[BlenderResult[None]]:
        """Owns each successful native registration immediately and unwinds in reverse."""
        panel, mode = self.toolbar.panel, self.toolbar.mode
        owner = f"{panel.__name__}._tools[{mode!r}]"
        context = "All" if mode is None else mode.replace("_", " ").title()
        bindings = tuple((draft, f"{panel.keymap_prefix} {context}, {tool.title}") for tool, draft in zip(self.tools, drafts))

        def planned(tools: list[object]) -> BlenderResult[Callable[[type[bpy.types.WorkSpaceTool]], None]]:
            """Accumulates native placement and existing-tool collisions before registration."""
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
            """Admits required native keyconfigs and accumulates collisions in each present map."""
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
            Option.of_optional(bpy.context.window_manager).map(configured).default_with(lambda: Error((BlenderAbsentFault(owner="bpy.context", key="window_manager"),))).map(Some)
            if any(draft.items for draft in drafts)
            else Ok(Nothing)
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
