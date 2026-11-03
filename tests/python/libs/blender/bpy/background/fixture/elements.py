"""Interface element rows: panels at every placement kind, menus of every body, a header, appends, gizmo groups with a custom shape, toolbar tools, an asset shelf, and the preferences page, with probes reading what the host registered."""

from collections.abc import Iterable, Sequence
from typing import Final

import _bpy_types
from bl_ui import UI_MT_button_context_menu
from bl_ui.properties_output import RENDER_PT_format
from bl_ui.properties_scene import SCENE_PT_unit
from bl_ui.space_toolsystem_common import ToolDef
from bl_ui.space_toolsystem_toolbar import VIEW3D_PT_tools_active
from bl_ui.space_view3d import VIEW3D_MT_add
import bpy
from expression import Nothing, Ok, Option, Some
from expression.extra.result import catch
from mathutils import Matrix

from blender.bpy.commands.keymaps import Chord
from blender.bpy.commands.operators import called, Placement
from blender.bpy.data.rna.paths import definition
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Declaration, Gathering, Registration
from blender.bpy.ui.drawing.handlers import Signal
from blender.bpy.ui.elements.gizmos import Gizmo, GizmoGroup, Shape, TargetHandler
from blender.bpy.ui.elements.menus import Append, ContextMenu, Dropdown, Header, Menu, Pie, Submenus
from blender.bpy.ui.elements.panels import Beside, Child, DynamicPlacement, EditorRegion, FixedPlacement, Panel, PanelCopy, POPOVER, Preference, PreferencesPage, Sidebar
from blender.bpy.ui.elements.shelves import Shelf
from blender.bpy.ui.elements.tools import Tool, Tools
from blender.bpy.ui.layout.members import Member
from blender.bpy.ui.layout.rows import Label, OperatorButton, Property
from blender.bpy.ui.layout.sections import Section
from blender.bpy.ui.resources.icons import ICONS
from blender.records.faults import Outcome

from .commands import binding, COUNTED
from .extension import PACKAGE

# --- [CONSTANTS] ------------------------------------------------------------------------

DOCKED: Final = Signal()
PANELS: Final = (
    "FIXTURE_PT_main",
    "FIXTURE_PT_main_detail",
    "FIXTURE_PT_main_scene_pt_unit",
    "FIXTURE_PT_main_scene_pt_unit_detail",
    "FIXTURE_PT_nested",
    "FIXTURE_PT_regioned",
    "FIXTURE_PT_popped",
    "FIXTURE_PT_docked",
)
STOCK: Final[tuple[type[_bpy_types.Menu | _bpy_types.Panel], ...]] = (VIEW3D_MT_add, SCENE_PT_unit, UI_MT_button_context_menu)
MENUS: Final = ("FIXTURE_MT_main", "FIXTURE_MT_searched", "FIXTURE_MT_grouped", "FIXTURE_MT_grouped_first", "FIXTURE_MT_grouped_second", "FIXTURE_MT_pie")

# --- [MODELS] ---------------------------------------------------------------------------


class Settings(bpy.types.PropertyGroup):
    """Preference group fields."""

    level: bpy.props.IntProperty(name="Level", default=3)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    hidden: bpy.props.BoolProperty(name="Hidden", options={"HIDDEN"})  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


# --- [SERVICES] -------------------------------------------------------------------------


class Held:
    """Placement the docked panel reads, and value the shape gizmo's target handler reads and writes."""

    __slots__ = ("dock", "value")

    def __init__(self) -> None:
        """Holds the Sidebar dock and a half value."""
        self.dock: FixedPlacement = Sidebar(space="VIEW_3D", category="Fixture")
        self.value = 0.5

    def docked(self, _identity: Identity) -> Outcome[FixedPlacement]:
        """Reads the held dock."""
        return Ok(self.dock)

    def read(self, _context: bpy.types.Context) -> float:
        """Reads the handler-bound value."""
        return self.value

    def written(self, _context: bpy.types.Context, value: float) -> Outcome[None]:
        """Writes the handler-bound value."""
        self.value = value
        return Ok(None)


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [READS]


def _registered(base: type[bpy.types.Panel | bpy.types.Menu | bpy.types.Header | bpy.types.AssetShelf | bpy.types.GizmoGroup], name: str) -> Option[dict[str, object]]:
    """Reads every member the host registers a class of a base by, None for each the class leaves unset, Nothing while the host holds no class of the name."""
    return Option.of_optional(base.bl_rna_get_subclass_py(name)).map(
        lambda cls: {prop.identifier: getattr(cls, prop.identifier, None) for prop in definition(cls).properties if prop.is_registered or prop.is_registered_optional}
    )


def _origin(_context: bpy.types.Context) -> Outcome[Option[Matrix]]:
    """Places a gizmo at the world origin."""
    return Ok(Some(Matrix.Identity(4)))


def _extended() -> tuple[bool, ...]:
    """Reads whether each stock panel and menu holds draw functions beyond its own."""
    return tuple(stock.is_extended() for stock in STOCK)


def _tools() -> tuple[tuple[str, object], ...]:
    """Reads the 3D View object-mode toolbar as idnames, flyout groups as their idname tuples, separators as None, dynamic entries left out."""

    def listed(entry: object) -> Option[tuple[str, object]]:
        match entry:
            case None:
                return Some(("separator", None))
            case ToolDef(idname=idname):
                return Some(("tool", idname))
            case tuple():
                return Some(("group", tuple(item.idname for item in entry if isinstance(item, ToolDef))))
            case _:
                return Nothing

    toolbars: Iterable[tuple[str | None, Sequence[object]]] = VIEW3D_PT_tools_active.tools_all()  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    return tuple(found for entry in dict(toolbars)["OBJECT"] for found in listed(entry).to_list())


def _refused(rows: tuple[Declaration | Gathering, ...]) -> str:
    """Registers a scope over rows the library refuses, answering the error type its registration raised."""
    return catch(Registration(PACKAGE, rows).register, exception=Exception)().swap().map(lambda error: type(error).__name__).default_value("")


# --- [PROBES]


def panels(_identity: Identity) -> dict[str, object]:
    """Reads the registered members of every panel class."""
    return {name: _registered(bpy.types.Panel, name) for name in PANELS}


def iconed(identity: Identity) -> dict[str, object]:
    """Reads whether the Sidebar panel holds the brand icon and the docked panel's members before and after the dock moves."""
    sidebar = _registered(bpy.types.Panel, "FIXTURE_PT_main").map(lambda read: read["bl_icon_value"]) == identity.resources[ICONS].brand
    before = _registered(bpy.types.Panel, "FIXTURE_PT_docked")
    HELD.dock = EditorRegion(space="IMAGE_EDITOR", region="UI")
    fired = DOCKED.fired()
    after = _registered(bpy.types.Panel, "FIXTURE_PT_docked")
    HELD.dock = Sidebar(space="VIEW_3D", category="Fixture")
    return {"sidebar icon": sidebar, "before": before, "fired": fired, "after": after}


def menus(_identity: Identity) -> dict[str, object]:
    """Reads the registered members of every menu class."""
    return {name: _registered(bpy.types.Menu, name) for name in MENUS}


def headed(_identity: Identity) -> object:
    """Reads the registered members of the header class."""
    return _registered(bpy.types.Header, "FIXTURE_HT_main")


def appended(_identity: Identity) -> dict[str, object]:
    """Reads whether each stock panel and menu holds draw functions beyond its own before and after registration."""
    return {"before": BASELINE, "after": _extended()}


def gizmos(identity: Identity) -> dict[str, object]:
    """Reads the registered gizmo groups, the host callbacks the main group implements, and the custom gizmo type, and links and unlinks the group that is not persistent."""
    native = bpy.types.GizmoGroup.bl_rna_get_subclass_py("FIXTURE_GGT_main")
    shape = bpy.types.Gizmo.bl_rna_get_subclass_py("FIXTURE_GT_mark")
    with GIZMOS.ensured(identity) as linked:
        ensured = linked
    with PERSISTENT.ensured(identity) as linked:
        refused = linked
    return {
        "group": _registered(bpy.types.GizmoGroup, "FIXTURE_GGT_main"),
        "persistent": _registered(bpy.types.GizmoGroup, "FIXTURE_GGT_persistent"),
        "callbacks": sorted({"setup_keymap", "invoke_prepare"}.intersection(dir(native))),
        "shape": shape is not None and tuple(target["id"] for target in shape.bl_target_properties),
        "ensured": ensured,
        "refused": refused,
    }


def tooled(identity: Identity) -> dict[str, object]:
    """Reads the toolbar around the extension tools, the tool keymap items, and activates the first tool through its placement."""
    toolbar = _tools()
    manager: bpy.types.WindowManager = bpy.context.window_manager  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    window = manager.windows[0]
    view = next(area for area in window.screen.areas if area.type == "VIEW_3D")
    activation = TOOLS.activation(identity, TOOLS.tools[0])
    ran = activation.bind(lambda placement: placement.resolved(identity)).bind(
        lambda placed: called(bpy.ops.wm.tool_set_by_id, dict(placed[1]), override=Some(bpy.context.temp_override(window=window, area=view)))
    )
    active = window.workspace.tools.from_space_view3d_mode("OBJECT", create=False)
    addon: bpy.types.KeyConfig = manager.keyconfigs.addon  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    return {
        "toolbar": toolbar,
        "activation": activation.bind(lambda placement: placement.resolved(identity)),
        "ran": ran,
        "active": Option.of_optional(active).map(lambda tool: tool.idname),
        "keymaps": tuple((keymap.name, binding(item)) for keymap in addon.keymaps if keymap.name != "3D View" for item in keymap.keymap_items if item.idname == "fixture.counted"),
        "brush": catch(Tool, exception=ValueError)(key="brushed", operator=COUNTED, icon="ops.generic.select_circle", brush_type=Some("DRAW")).swap().map(lambda error: type(error).__name__),
    }


def shelved(_identity: Identity) -> object:
    """Reads the registered members of the asset shelf class."""
    return _registered(bpy.types.AssetShelf, "FIXTURE_AST_main")


def preferred(identity: Identity) -> dict[str, object]:
    """Reads the preferences page's class properties and values, and the show command it registers."""
    preferences = identity.preferences(bpy.types.AddonPreferences)
    names = preferences.map(lambda held: tuple(sorted(prop.identifier for prop in held.bl_rna.properties if prop.identifier not in {"rna_type", "bl_idname"})))
    return {
        "properties": names,
        "read": SETTINGS.read(identity).map(lambda held: held.level),
        "show": bpy.types.Operator.bl_rna_get_subclass_py("FIXTURE_OT_show_preferences") is not None,
        "purge": bpy.types.Operator.bl_rna_get_subclass_py("FIXTURE_OT_clear_stored_settings") is not None,
    }


# --- [RELEASES]


def released_classes() -> dict[str, bool]:
    """Answers which element classes the host holds."""
    return {
        **{name: bpy.types.Panel.bl_rna_get_subclass_py(name) is not None for name in PANELS},
        **{name: bpy.types.Menu.bl_rna_get_subclass_py(name) is not None for name in MENUS},
        "FIXTURE_HT_main": bpy.types.Header.bl_rna_get_subclass_py("FIXTURE_HT_main") is not None,
        "FIXTURE_AST_main": bpy.types.AssetShelf.bl_rna_get_subclass_py("FIXTURE_AST_main") is not None,
        "FIXTURE_GGT_main": bpy.types.GizmoGroup.bl_rna_get_subclass_py("FIXTURE_GGT_main") is not None,
        "FIXTURE_GT_mark": bpy.types.Gizmo.bl_rna_get_subclass_py("FIXTURE_GT_mark") is not None,
    }


def released_elements() -> dict[str, object]:
    """Reads whether the stock panels and menus hold draw functions beyond their own and the extension tools left after release."""
    return {"appended": _extended(), "tools": tuple(entry for entry in _tools() if "fixture" in repr(entry))}


def refusals() -> dict[str, str]:
    """Registers menus and headers the library refuses: one holding a section, a pie slot over a value that is no boolean or enum."""
    section = Section(key="refused")
    return {
        "menu section": _refused((Menu(key="sectioned", description="Holds a section", body=Dropdown(entries=(section,))),)),
        "header section": _refused((Header(key="sectioned", space="VIEW_3D", rows=(section,)),)),
        "pie value": _refused((Menu(key="valued", description="Slots a value", body=Pie(west=Some(Property(target=Member(root=bpy.types.Scene, path=("frame_start",)))))),)),
        "unknown filter": _refused((Shelf(key="filtered", space="VIEW_3D", filters=frozenset({"filter_fixture"}), assets=frozenset({"probe"})),)),
    }


# --- [COMPOSITION] ----------------------------------------------------------------------

BASELINE: Final = _extended()
HELD: Final = Held()
SHAPE: Final = Shape(key="mark", primitive="TRIS", vertices=((0.0, 0.0, 0.0), (1.0, 0.0, 0.0), (0.0, 1.0, 0.0)), targets={"offset": 1})
GIZMOS: Final = GizmoGroup(
    key="main",
    space="VIEW_3D",
    gizmos=(
        Gizmo(kind="GIZMO_GT_arrow_3d", place=_origin, bind=Member(root=bpy.types.Object, path=("empty_display_size",), reach=Some(lambda context: Option.of_optional(context.active_object)))),
        Gizmo(kind=SHAPE, place=_origin, bind=TargetHandler[float](get=HELD.read, set=HELD.written), prepare=Some(lambda _context, _gizmo: Ok(None))),
        Gizmo(kind="GIZMO_GT_button_2d", place=_origin, bind=(Placement(COUNTED, {"amount": 2}),), icon=Some("ADD"), properties={"draw_options": {"BACKDROP", "OUTLINE"}}),
    ),
    keymap=Some("3D View"),
    options=frozenset(),
)
PERSISTENT: Final = GizmoGroup(key="persistent", space="VIEW_3D", gizmos=(Gizmo(kind="GIZMO_GT_dial_3d", place=_origin),))
TOOLS: Final = Tools(
    space="VIEW_3D",
    mode=Some("OBJECT"),
    after=frozenset({"builtin.transform"}),
    separator=True,
    group=True,
    tools=(
        Tool(key="probe", operator=COUNTED, icon="ops.generic.select_circle", bindings=((Chord(type="LEFTMOUSE"), Placement(COUNTED, {"amount": 6})),)),
        Tool(key="probe_second", operator=COUNTED, icon="ops.generic.select_circle", widget=Some(GIZMOS)),
    ),
)
SETTINGS: Final = Preference(key="settings", form=Settings)
PAGE: Final = PreferencesPage(preferences=(SETTINGS,), sections=(Section(key="page", rows=(Label(text="Fixture"),)),))
LABELED: Final = (Label(text="Fixture"),)

rows: Final = (
    Panel(
        section=Section(key="main", rows=LABELED),
        at=Sidebar(space="VIEW_3D", category="Fixture"),
        copies=(PanelCopy(at=Beside(SCENE_PT_unit)),),
        children=(Section(key="detail", opened=True),),
        units=Some(12),
    ),
    Panel(section=Section(key="nested", rows=LABELED), at=Child(parent=RENDER_PT_format)),
    Panel(section=Section(key="regioned", rows=LABELED), at=EditorRegion(space="IMAGE_EDITOR", region="TOOLS"), options=frozenset({"HIDE_HEADER"})),
    Panel(section=Section(key="popped", rows=LABELED), at=POPOVER, units=Some(10)),
    Panel(section=Section(key="docked", rows=LABELED), at=DynamicPlacement(read=HELD.docked, changed=DOCKED)),
    Menu(key="main", description="Fixture commands", body=Dropdown(entries=(OperatorButton(target=COUNTED), *LABELED))),
    Menu(key="searched", description="Searchable commands", body=Dropdown(entries=LABELED, order=Some(lambda _context: (0, 4)), search=True)),
    Menu(key="grouped", description="Grouped commands", body=Submenus(entries=(("first", Label(text="First")), ("second", Label(text="Second"))))),
    Menu(key="pie", description="Radial commands", body=Pie(west=Some(OperatorButton(target=COUNTED)), east=Some(Property(target=Member(root=bpy.types.Scene, path=("use_gravity",)))))),
    Header(key="main", space="VIEW_3D", rows=LABELED),
    Append(target=VIEW3D_MT_add, rows=(OperatorButton(target=COUNTED),)),
    Append(target=SCENE_PT_unit, rows=LABELED, prepend=True),
    Append(target=ContextMenu(menu=UI_MT_button_context_menu, hovered=bpy.types.Scene), rows=LABELED),
    GIZMOS,
    PERSISTENT,
    TOOLS,
    Shelf(key="main", space="VIEW_3D", filters=frozenset({"filter_mesh"}), assets=frozenset({"probe"}), visible=True, drag=False),
    PAGE,
)
PROBES: Final = (panels, iconed, menus, headed, appended, gizmos, tooled, shelved, preferred)
RELEASED: Final = (released_classes, released_elements, refusals)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["PROBES", "RELEASED", "rows"]
