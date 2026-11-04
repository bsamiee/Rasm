"""Native panel placements, menu callbacks, nested command dependencies, and registration release."""

from collections.abc import Generator
from contextlib import contextmanager, ExitStack

from bl_ui.properties_scene import SCENE_PT_unit
from bl_ui.space_toolsystem_common import ToolDef
from bl_ui.space_toolsystem_toolbar import VIEW3D_PT_tools_active
from bl_ui.space_view3d import VIEW3D_MT_add
import bpy
from expression import Option, Result, Some
from mathutils import Matrix
import pytest

from blender.bpy.commands.keymaps import Chord
from blender.bpy.commands.operators import Answer, Call, called, Done, Empty, operation, Operator, Placement
from blender.bpy.data.rna.paths import definition
from blender.bpy.ui.drawing.handlers import Signal
from blender.bpy.ui.elements.gizmos import Gizmo, GizmoGroup, Shape
from blender.bpy.ui.elements.menus import Append, Dropdown, Header, Menu, Pie, Submenus
from blender.bpy.ui.elements.panels import Beside, DynamicPlacement, EditorRegion, FixedPlacement, Panel, PanelCopy, Preference, PreferencesPage, Sidebar
from blender.bpy.ui.elements.shelves import Shelf
from blender.bpy.ui.elements.tools import Tool, Tools
from blender.bpy.ui.layout.rows import Label, OperatorButton
from blender.bpy.ui.layout.sections import Section
from blender.records.faults import Fault, Ok, Outcome, Reason
from tests.python.libs.blender.bpy.background.session import Acquired

# --- [TYPES] ----------------------------------------------------------------------------

type PanelChange = tuple[str, type[bpy.types.Panel], EditorRegion]

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_preferences_read_native_group_and_emit_changed_on_edit(acquired: Acquired) -> None:
    """Preference group shares native storage with the page and emits a change for an actual edit."""

    class Settings(bpy.types.PropertyGroup):
        """User preference edited through native RNA."""

        level: bpy.props.IntProperty(name="Level", default=3)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]

    preference = Preference(key="settings", form=Settings)
    page = PreferencesPage(preferences=(preference,))
    changes: list[int] = []

    def changed() -> Outcome[None]:
        changes.append(1)
        return Ok(None)

    with acquired((page,)) as identity, page.changed.followed(changed):
        match preference.read(identity), identity.preferences(bpy.types.AddonPreferences):
            case Result(tag="ok", ok=group), Option(some=native):
                assert group.level == 3
                assert getattr(native, preference.key) == group
                name = definition(native).identifier
                group.level = 7
                assert getattr(native, preference.key).level == 7
                assert changes == [1]
            case found:
                pytest.fail(repr(found))
    assert bpy.types.AddonPreferences.bl_rna_get_subclass_py(name) is None


def test_native_panel_placements_copies_children_and_release(acquired: Acquired) -> None:
    """Native panels retain editor placement, copied context, nesting, and release every owned class."""
    panel = Panel(
        section=Section(key="main", rows=(Label(text="Fixture"),)),
        at=Sidebar(space="VIEW_3D", category="Fixture"),
        copies=(PanelCopy(at=Beside(SCENE_PT_unit)),),
        children=(Section(key="detail", opened=True),),
    )
    with acquired((panel,)) as identity:
        names = tuple(identity.name(bpy.types.Panel, key) for key in ("main", "main_detail", "main_scene_pt_unit", "main_scene_pt_unit_detail"))
        for name in names:
            native = bpy.types.Panel.bl_rna_get_subclass_py(name)
            assert native is not None
            assert native.bl_owner_id == identity.package
        native = bpy.types.Panel.bl_rna_get_subclass_py(names[0])
        assert native is not None
        assert (native.bl_space_type, native.bl_region_type, native.bl_category) == ("VIEW_3D", "UI", "Fixture")
        copied = bpy.types.Panel.bl_rna_get_subclass_py(names[2])
        assert copied is not None
        assert (copied.bl_space_type, copied.bl_region_type, copied.bl_context) == ("PROPERTIES", "WINDOW", "scene")
        child = bpy.types.Panel.bl_rna_get_subclass_py(names[3])
        assert child is not None
        assert child.bl_parent_id == names[2]
    assert all(bpy.types.Panel.bl_rna_get_subclass_py(name) is None for name in names)


@contextmanager
def dynamic_panel(acquired: Acquired) -> Generator[PanelChange]:
    """Holds the registered panel after its placement change is requested, releasing its native class at exit."""
    changed = Signal()
    placement: list[FixedPlacement] = [Sidebar(space="VIEW_3D", category="Fixture")]
    panel = Panel(section=Section(key="docked", rows=(Label(text="Fixture"),)), at=DynamicPlacement(read=lambda _identity: Ok(placement[0]), changed=changed))
    target = EditorRegion(space="IMAGE_EDITOR", region="UI")
    with acquired((panel,)) as identity:
        name = identity.name(bpy.types.Panel, panel.section.key)
        before: type[bpy.types.Panel] | None = bpy.types.Panel.bl_rna_get_subclass_py(name)  # ty: ignore[unsound-assignment]
        assert before is not None
        assert (before.bl_space_type, before.bl_region_type) == ("VIEW_3D", "UI")
        placement[0] = target
        assert changed.fired() == Ok(None)
        yield name, before, target
    assert bpy.types.Panel.bl_rna_get_subclass_py(name) is None


def test_dynamic_panel_signal_queues_native_placement(acquired: Acquired) -> None:
    """GUI signals queue the change while background signals apply it before returning."""
    with dynamic_panel(acquired) as (name, before, target):
        after = bpy.types.Panel.bl_rna_get_subclass_py(name)
        assert after is not None
        assert (after.bl_space_type, after.bl_region_type) == ((target.space, target.region) if bpy.app.background else (before.bl_space_type, before.bl_region_type))


def panel_ready(change: PanelChange) -> bool:
    """Whether the native main loop registered the requested panel placement."""
    name, _, target = change
    native = bpy.types.Panel.bl_rna_get_subclass_py(name)
    return native is not None and (native.bl_space_type, native.bl_region_type) == (target.space, target.region)


def panel_finished(scope: ExitStack, change: PanelChange) -> tuple[str, str]:
    """Checks the native deferred placement, closes its held scope, and checks registration release."""
    name, before, target = change
    try:
        after: type[bpy.types.Panel] | None = bpy.types.Panel.bl_rna_get_subclass_py(name)  # ty: ignore[unsound-assignment]
        assert after is not None
        assert after is not before
        placed = after.bl_space_type, after.bl_region_type
        assert placed == (target.space, target.region)
        return placed
    finally:
        scope.close()


def test_native_menu_header_shelf_and_appended_callbacks_release(acquired: Acquired) -> None:
    """Menus, headers, shelves, and stock draw extensions share their native registration lifetime."""
    labels = (Label(text="Fixture"),)
    menu = Menu(key="searched", description="Fixture commands", body=Dropdown(entries=labels, search=True))
    grouped = Menu(key="grouped", description="Grouped commands", body=Submenus(entries=(("first", labels[0]), ("second", labels[0]))))
    pie = Menu(key="pie", description="Radial commands", body=Pie(west=Some(OperatorButton(target=menu.popup))))
    header = Header(key="main", space="VIEW_3D", rows=labels)
    shelf = Shelf(key="main", space="VIEW_3D", assets=frozenset(), filters=frozenset({"filter_mesh"}), visible=True, drag=False)
    before = VIEW3D_MT_add.is_extended()
    with acquired((menu, grouped, pie, header, shelf, Append(target=VIEW3D_MT_add, rows=labels))) as identity:
        menu_names = tuple(identity.name(bpy.types.Menu, key) for key in (menu.key, grouped.key, f"{grouped.key}_first", f"{grouped.key}_second", pie.key))
        assert all(bpy.types.Menu.bl_rna_get_subclass_py(name) is not None for name in menu_names)
        name = identity.name(bpy.types.Menu, menu.key)
        native = bpy.types.Menu.bl_rna_get_subclass_py(name)
        assert native is not None
        assert native.bl_options == {"SEARCH_ON_KEY_PRESS"}
        assert VIEW3D_MT_add.is_extended() is True
        for key in ("first", "second"):
            child = bpy.types.Menu.bl_rna_get_subclass_py(identity.name(bpy.types.Menu, f"{grouped.key}_{key}"))
            assert child is not None
            assert child.bl_label == bpy.path.display_name(key)
        shelf_name = identity.name(bpy.types.AssetShelf, shelf.key)
        native_shelf = bpy.types.AssetShelf.bl_rna_get_subclass_py(shelf_name)
        assert native_shelf is not None
        assert native_shelf.filter_mesh is True
        header_name = identity.name(bpy.types.Header, header.key)
        assert bpy.types.Header.bl_rna_get_subclass_py(header_name) is not None
    assert VIEW3D_MT_add.is_extended() is before
    assert all(bpy.types.Menu.bl_rna_get_subclass_py(name) is None for name in menu_names)
    assert bpy.types.Header.bl_rna_get_subclass_py(header_name) is None
    assert bpy.types.AssetShelf.bl_rna_get_subclass_py(shelf_name) is None


def test_tool_only_commands_register_run_bind_and_release(acquired: Acquired) -> None:
    """Commands declared only by a tool or its binding register, execute, and release with that toolbar."""
    calls: list[str] = []

    @Operator.of(Empty)
    def primary(_call: Call[Empty]) -> Outcome[Answer]:
        calls.append("primary")
        return Ok(Done())

    @Operator.of(Empty)
    def bound(_call: Call[Empty]) -> Outcome[Answer]:
        calls.append("bound")
        return Ok(Done())

    first = Tool(key="probe", operator=primary, icon="ops.generic.select_circle", bindings=((Chord(type="LEFTMOUSE"), Placement(bound)),))
    second = Tool(key="probe_second", operator=primary, icon="ops.generic.select_circle")
    tools = Tools(space="VIEW_3D", mode=Some("OBJECT"), after=frozenset({"builtin.transform"}), separator=True, group=True, tools=(first, second))
    manager = bpy.data.window_managers[0]
    window = manager.windows[0]
    area = next(area for area in window.screen.areas if area.type == "VIEW_3D")
    prior = window.workspace.tools.from_space_view3d_mode("OBJECT", create=False)
    assert prior is not None
    previous = prior.idname
    with acquired((tools,)) as identity, bpy.context.temp_override(window=window, area=area), ExitStack() as restore:
        restore.callback(bpy.ops.wm.tool_set_by_id, name=previous, space_type="VIEW_3D")
        names = tuple(identity.name(bpy.types.Operator, command.key) for command in (primary, bound))
        for command in (primary, bound):
            assert operation(identity, command).bind(lambda found: called(found, {})) == Ok(frozenset({"FINISHED"}))
        assert calls == ["primary", "bound"]
        activation = tools.activation(identity, first)
        operator, values = activation.resolved(identity)
        assert operator == "wm.tool_set_by_id"
        assert called(bpy.ops.wm.tool_set_by_id, dict(values)) == Ok(frozenset({"FINISHED"}))
        active = window.workspace.tools.from_space_view3d_mode("OBJECT", create=False)
        assert active is not None
        assert active.idname == identity.name(bpy.types.Operator, first.key)
        addon = manager.keyconfigs.addon
        assert addon is not None
        keymaps = tuple(keymap for keymap in addon.keymaps if any(item.idname == names[1] for item in keymap.keymap_items))
        (keymap,) = keymaps
        assert next(item for item in keymap.keymap_items if item.idname == names[1]).type == "LEFTMOUSE"
        keymap_name = keymap.name
        toolbar = dict(VIEW3D_PT_tools_active.tools_all())["OBJECT"]  # type: ignore[call-overload]  # ty: ignore[no-matching-overload]
        group = next(entry for entry in toolbar if isinstance(entry, tuple) and any(isinstance(tool, ToolDef) and tool.idname == active.idname for tool in entry))
        assert tuple(tool.idname for tool in group) == tuple(identity.name(bpy.types.Operator, tool.key) for tool in tools.tools)
    restored = window.workspace.tools.from_space_view3d_mode("OBJECT", create=False)
    assert restored is not None
    assert restored.idname == previous
    for command in (primary, bound):
        match operation(identity, command):
            case Result(error=(Fault(reason=reason, cause=KeyError(), member=member),)):
                assert reason is Reason.ABSENT
                assert member in names
            case found:
                pytest.fail(repr(found))
    assert addon.keymaps.get(keymap_name) is None


def test_gizmo_only_command_and_shape_follow_native_registration(acquired: Acquired) -> None:
    """Command held only by a gizmo placement registers and transient group scope preserves native failures."""
    calls: list[int] = []

    @Operator.of(Empty)
    def placed(_call: Call[Empty]) -> Outcome[Answer]:
        calls.append(1)
        return Ok(Done())

    shape = Shape(key="mark", primitive="TRIS", vertices=((0.0, 0.0, 0.0), (1.0, 0.0, 0.0), (0.0, 1.0, 0.0)), targets={"offset": 1})
    group = GizmoGroup(
        key="main",
        space="VIEW_3D",
        read=lambda context: Ok(Some(context)),
        gizmos=(
            Gizmo(kind="GIZMO_GT_button_2d", place=lambda _context: Ok(Some(Matrix.Identity(4))), bind=(Placement(placed),)),
            Gizmo(kind=shape, place=lambda _context: Ok(Some(Matrix.Identity(4)))),
        ),
        options=frozenset(),
    )
    persistent = GizmoGroup(key="persistent", space="VIEW_3D", read=lambda context: Ok(Some(context)), gizmos=())
    with acquired((group, persistent)) as identity:
        assert operation(identity, placed).bind(lambda found: called(found, {})) == Ok(frozenset({"FINISHED"}))
        assert calls == [1]
        group_name = identity.name(bpy.types.GizmoGroup, group.key)
        shape_name = identity.name(bpy.types.Gizmo, shape.key)
        assert bpy.types.Gizmo.bl_rna_get_subclass_py(shape_name) is not None
        with group.ensured(identity) as held:
            assert held == Ok(None)
        with persistent.ensured(identity) as refused:
            match refused:
                case Result(error=(Fault(reason=reason, cause=RuntimeError()),)):
                    assert reason is Reason.REFUSED
                case found:
                    pytest.fail(repr(found))
    assert bpy.types.GizmoGroup.bl_rna_get_subclass_py(group_name) is None
    assert bpy.types.Gizmo.bl_rna_get_subclass_py(shape_name) is None
