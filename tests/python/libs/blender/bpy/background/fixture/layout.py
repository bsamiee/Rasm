"""Layout rows: an attached form group with its store, a collection list, and a preset listing, with probes realizing rows, templates, tables, sections, and fields, and running store, list, and preset commands."""

from collections.abc import Mapping
from pathlib import Path, PurePath
from typing import Final

from bl_ui.space_view3d import VIEW3D_MT_add, VIEW3D_MT_pivot_pie
import bpy
from expression import Ok, Option, Some
import msgspec

from blender.bpy.commands.calls import Called, called, Opened, Placement, Setting
from blender.bpy.commands.operators import Answer, Done
from blender.bpy.data.rna.conditions import Scenes
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.data.rna.paths import rooted
from blender.bpy.extension.identity import Identity
from blender.bpy.settings.attachments import Attachment
from blender.bpy.settings.presets import Preset
from blender.bpy.ui.layout.lists import ItemList
from blender.bpy.ui.layout.rows import accepted, acquired, FUNCTIONS, Label, Layout, marked, Member, Opens, OperatorButton, Property, realized, RowContent, Status
from blender.bpy.ui.layout.sections import fields, Items, reached, Section
from blender.bpy.ui.layout.tables import Matrix, Tree, TreeMatrix
from blender.bpy.ui.layout.templates import ImageFormat, Preview, Shortcut, wheel
from blender.bpy.ui.layout.values import Listing, Store, Verb
from blender.bpy.ui.surfaces.menus import PresetMenu
from blender.bpy.ui.surfaces.panels import EditorRegion, Panel
from blender.records.faults import Outcome

from .commands import COUNTED, KEYS, operation

# --- [CONSTANTS] ------------------------------------------------------------------------

TONE: Final = "fixture_tone"

# --- [MODELS] ---------------------------------------------------------------------------


class Entry(bpy.types.PropertyGroup):
    """Collection item with a name and a value."""

    value: bpy.props.IntProperty(name="Value")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    expanded: bpy.props.BoolProperty(name="Expanded")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Look(bpy.types.PropertyGroup):
    """Attached form with scalars, a hidden member, a nested group, and a collection."""

    exposure: bpy.props.FloatProperty(name="Exposure", default=1.0)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    enabled: bpy.props.BoolProperty(name="Enabled", default=True)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    hidden: bpy.props.IntProperty(name="Hidden", options={"HIDDEN"})  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    entries: bpy.props.CollectionProperty(type=Entry)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Tone(msgspec.Struct, frozen=True, kw_only=True):
    """Preset record of one level."""

    level: int


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [BODIES]


def _current(context: bpy.types.Context) -> Outcome[Tone]:
    """Reads the scene's tone level."""
    return rooted(bpy.types.Scene, context).map(lambda scene: Tone(level=int(scene.get(TONE, 0))))


def _matching(context: bpy.types.Context, values: tuple[Tone, ...]) -> Outcome[tuple[bool, ...]]:
    """Matches presets against the scene's tone level."""
    return _current(context).map(lambda current: tuple(value == current for value in values))


def _applied(context: bpy.types.Context, value: Tone) -> Outcome[Answer]:
    """Writes the preset level onto the scene."""
    scene: bpy.types.Scene = context.scene  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    scene[TONE] = value.level
    return Ok(Done())


def _separated(layout: bpy.types.UILayout, _holder: object, _context: bpy.types.Context) -> None:
    """Draws a line separator, a plain draw as row content."""
    layout.separator(type="LINE")


# --- [READS]


def _drawn(identity: Identity, row: RowContent) -> object:
    """Realizes one row, answering whether a draw came back or the refusal reasons."""
    return realized((row,), identity).map(callable)


def _kinds(rows: tuple[object, ...]) -> tuple[str, ...]:
    """Names each row's kind, a section by its key."""
    return tuple(f"Section {row.key}" if isinstance(row, Section) else type(row).__name__ for row in rows)


# --- [PROBES]


def bound(identity: Identity) -> dict[str, object]:
    """Realizes property rows over present, absent, reached, and wrongly keyworded members, and marks a setting mixed across owners that differ."""
    scene: bpy.types.Scene = bpy.context.scene  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    other = bpy.data.scenes.new("Fixture Mixed")
    other.frame_start = scene.frame_start + 1
    setting = Descriptor.declared(bpy.types.Scene, ("frame_start",), bpy.types.IntProperty)
    mixed, equal = marked(setting, (scene, other)), marked(setting, (scene, scene))
    bpy.data.scenes.remove(other)
    return {
        "present": _drawn(identity, Property(target=Member(root=bpy.types.Scene, path=("frame_start",)), keywords={"slider": True})),
        "element": _drawn(identity, Property(target=Member(root=bpy.types.Object, path=("location", 0)))),
        "reached": _drawn(identity, Property(target=Member(root=bpy.types.Scene, path=("frame_end",), reach=Some(lambda context: Option.of_optional(context.scene))))),
        "scoped": _drawn(identity, Property(target=Member(root=bpy.types.Scene, path=("frame_start",), scope=Some(Scenes())))),
        "absent": _drawn(identity, Property(target=Member(root=bpy.types.Scene, path=("fixture_absent",)))),
        "keyword": _drawn(identity, Property(target=Member(root=bpy.types.Scene, path=("frame_start",)), keywords={"fixture": 1})),
        "glyph": _drawn(identity, Property(target=Member(root=bpy.types.Scene, path=("frame_start",)), icon=Some("ADD"))),
        "unknown glyph": _drawn(identity, Property(target=Member(root=bpy.types.Scene, path=("frame_start",)), icon=Some(Path("fixture_absent.png")))),
        "function": _drawn(identity, Property(target=Member(root=bpy.types.Scene, path=("render", "engine")), function=FUNCTIONS["prop_tabs_enum"])),
        "search": _drawn(
            identity, Property(target=Member(root=bpy.types.Scene, path=("camera",)), search=Some(Member(root=bpy.types.BlendData, path=("objects",))), function=FUNCTIONS["template_search"])
        ),
        "struct": _drawn(identity, Property(target=Member(root=bpy.types.RenderSettings, path=("image_settings", "stereo_3d_format")), function=FUNCTIONS["template_image_stereo_3d"])),
        "foreign struct": _drawn(identity, Property(target=Member(root=bpy.types.RenderSettings, path=("image_settings",)), function=FUNCTIONS["template_node_inputs"])),
        "mixed": mixed.map(lambda text: text == Some(f"{setting.prop.name} —")),
        "equal": equal.map(lambda text: text.is_none()),
    }


def pressed(identity: Identity) -> dict[str, object]:
    """Realizes operator buttons and openers over commands, placements, menus, and pies."""
    return {
        "command": _drawn(identity, OperatorButton(target=COUNTED)),
        "placement": _drawn(identity, OperatorButton(target=Placement(bpy.ops.wm.context_toggle, {"data_path": "scene.use_gravity"}), keywords={"text": "Gravity"})),
        "keyword": _drawn(identity, OperatorButton(target=COUNTED, keywords={"fixture": 1})),
        "acquired": tuple(type(row).__name__ for row in OperatorButton(target=COUNTED).acquisitions(identity).default_value(())),
        "menu": _drawn(identity, Opens(target=Opened(VIEW3D_MT_add, Called.MENU))),
        "panel": _drawn(identity, Opens(target=Opened("main", Called.PANEL))),
        "pie": _drawn(identity, Opens(target=Opened(VIEW3D_MT_pivot_pie, Called.PIE))),
    }


def labelled(identity: Identity) -> dict[str, object]:
    """Realizes fixed and read labels, a label through another function, and the report status row."""
    return {
        "fixed": _drawn(identity, Label(text="Fixture")),
        "read": _drawn(identity, Label(text=lambda context: Option.of_optional(context.scene).map(lambda scene: scene.name).to_list())),
        "single": _drawn(identity, Label(text="Fixture", function=FUNCTIONS["label"])),
        "keyword": _drawn(identity, Label(text="Fixture", keywords={"fixture": 1})),
        "status": _drawn(identity, Status()),
    }


def templated(identity: Identity) -> dict[str, object]:
    """Realizes template rows over data-block, format, color, and keymap members."""
    return {
        "preview": _drawn(identity, Preview(target=Member(root=bpy.types.Scene, path=("world",)))),
        "preview of value": _drawn(identity, Preview(target=Member(root=bpy.types.Scene, path=("frame_start",)))),
        "format": _drawn(identity, ImageFormat(target=Member(root=bpy.types.RenderSettings, path=("image_settings",)))),
        "format keyword": _drawn(identity, ImageFormat(target=Member(root=bpy.types.RenderSettings, path=("image_settings",)), keywords={"fixture": 1})),
        "wheel": _drawn(identity, wheel(Member(root=bpy.types.View3DShading, path=("single_color",)))),
        "shortcut": _drawn(identity, Shortcut(keymap=KEYS)),
    }


def tabled(identity: Identity) -> dict[str, object]:
    """Realizes a setting table across scenes, a path tree, and a tree table with mark rows, and refuses a setting the owner type lacks."""
    return {
        "matrix": _drawn(identity, Matrix(scope=Scenes(), settings=(("frame_start",), ("frame_end",)))),
        "absent setting": _drawn(identity, Matrix(scope=Scenes(), settings=(("fixture_absent",),))),
        "tree": _drawn(identity, Tree(paths=lambda _context: Ok((PurePath("textures/wood.png"), PurePath("textures/stone.png"))))),
        "tree matrix": _drawn(
            identity, TreeMatrix[bpy.types.Scene, bpy.types.Scene](tree=lambda _context: ((), ()), cell=lambda _layout, _owner, _cell, _label: Ok(None), marks=(Label(text="Mark"),))
        ),
    }


def composed(identity: Identity) -> dict[str, object]:
    """Realizes a sub-layout of rows and plain draws, a layout built per identity, and rows whose refusals accumulate."""
    rows = (Label(text="Fixture"), _separated, OperatorButton(target=COUNTED))
    return {
        "layout": _drawn(identity, Layout(rows=rows, container=Some(lambda layout: layout.box()))),
        "built": _drawn(identity, Layout(rows=lambda held: realized((), held).map(lambda _: rows))),
        "acquired": tuple(type(row).__name__ for row in acquired(rows)),
        "refusals": realized((Label(text="Fixture", keywords={"fixture": 1}), Property(target=Member(root=bpy.types.Scene, path=("fixture_absent",)))), identity).map(callable),
    }


def checked(_identity: Identity) -> dict[str, object]:
    """Checks keywords against UILayout function parameters."""
    return {"prop": accepted(FUNCTIONS["prop"], {"text": "Fixture", "slider": True}), "unknown": accepted(FUNCTIONS["prop"], {"txt": "Fixture"})}


def fielded(_identity: Identity) -> dict[str, object]:
    """Builds field rows of a form group, a pointer group, and a data-block type."""
    return {
        "look": _kinds(fields(Look, "look")),
        "replaced": _kinds(fields(Look, "look", {"exposure": lambda member: Label(text=str(member.path))})),
        "scene": _kinds(fields(bpy.types.Scene, "scene"))[-1:],
    }


def walked(_identity: Identity) -> tuple[str, ...]:
    """Walks sections through rows, headers, absent rows, and tuple sub-layouts, each ahead of its own."""
    inner = Section(key="inner", title="Inner")
    nested = Section(key="nested", title="Nested", rows=(Layout(rows=(inner,)),), header=(Section(key="headed", title="Headed"),))
    return tuple(section.key for section in reached((Label(text="Fixture"), nested, Section(key="absent", title="Absent", absent=(Section(key="fallback", title="Fallback"),)))))


def sectioned(identity: Identity) -> dict[str, object]:
    """Realizes sections with and without a header boolean, a store, and holders of each kind."""

    def parts[G: bpy.types.PropertyGroup](section: Section[G]) -> object:
        return section.realized(identity).map(lambda draws: (draws.head.is_some(), draws.tail.is_some()))

    return {
        "plain": parts(Section(key="plain", title="Plain", rows=(Label(text="Fixture"),))),
        "enabled": parts(Section(key="enabled", title="Enabled", enable=Some(Member(root=Look, path=("enabled",))), holder=Some(Member(root=bpy.types.Scene, path=("fixture_look",))))),
        "stored": parts(LOOKED),
        "items": parts(Section(key="items", title="Items", holder=Some(Items(collection=Member(root=Look, path=("entries",)), expanded=Some("expanded"))))),
        "listed": parts(Section(key="listed", title="Listed", holder=Some(ENTRIES))),
        "absent holder": parts(Section(key="absent", title="Absent", holder=Some(Member(root=bpy.types.Scene, path=("fixture_absent",))))),
        "unknown flag": parts(Section(key="flagged", title="Flagged", holder=Some(Items(collection=Member(root=Look, path=("entries",)), expanded=Some("fixture_absent"))))),
        "drawn": Section(key="inline", title="Inline", rows=(Label(text="Fixture"),)).drawn(identity).map(callable),
    }


def listed(identity: Identity) -> dict[str, object]:
    """Reads the registered list class, its selection index, its drag command, and refusals of a filter on a compact list and of a foreign item type."""
    compact = ItemList(key="compact", collection=Member(root=Look, path=("entries",)), item=Entry, where=Some(lambda _context, _ui, _item: Ok(True)), keywords={"type": "COMPACT"})
    foreign = ItemList(key="foreign", collection=Member(root=Look, path=("entries",)), item=bpy.types.Object)
    return {
        "class": bpy.types.UIList.bl_rna_get_subclass_py("FIXTURE_UL_entries") is not None,
        "index": "fixture_entries" in Look.bl_rna.properties,
        "drag": bpy.types.Operator.bl_rna_get_subclass_py("FIXTURE_OT_entries_drag") is not None,
        "picked": ENTRIES.picked(identity).map(callable),
        "drawn": ENTRIES.drawn(identity).map(callable),
        "compact": compact.acquisitions(identity).map(lambda _: None),
        "foreign": foreign.acquisitions(identity).map(lambda _: None),
    }


def stored(identity: Identity) -> dict[str, object]:
    """Runs the store's reset and history commands over the scene host its context pointer names."""
    scene: bpy.types.Scene = bpy.context.scene  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    look: Look = scene.fixture_look  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    commands = STORE.commands

    def ran(verb: Verb, values: Mapping[str, Setting]) -> object:
        with bpy.context.temp_override(fixture_look=scene):
            return called(operation(identity, commands[verb]), values)

    look.exposure = 2.0
    reset = (ran(Verb.RESET, {}), look.exposure)
    older = (ran(Verb.RESTORE, {"offset": 1}), look.exposure)
    newer = (ran(Verb.RESTORE, {"offset": -1}), look.exposure)
    unchanged = ran(Verb.RESET, {})
    unhosted = called(operation(identity, commands[Verb.RESET]), {})
    return {"verbs": tuple(sorted(verb.name for verb in commands)), "reset": reset, "older": older, "newer": newer, "unchanged": unchanged, "unhosted": unhosted}


def preset(identity: Identity) -> dict[str, object]:
    """Applies, steps through, saves, and lists presets of the listing, reading the scene level after each."""
    scene: bpy.types.Scene = bpy.context.scene  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    commands = LISTING.commands

    def ran(verb: Verb, values: Mapping[str, Setting]) -> tuple[object, int]:
        return called(operation(identity, commands[verb]), values), int(scene.get(TONE, 0))

    applied = ran(Verb.APPLY, {"stem": "soft"})
    following = ran(Verb.STEP, {"offset": 1})
    wrapped = ran(Verb.STEP, {"offset": 1})
    scene[TONE] = 9
    saved = ran(Verb.SAVE, {"name": "Mine"})
    listed_stems = tuple(LISTING.preset.listed(identity))
    taken = ran(Verb.SAVE, {"name": "Soft"})
    return {
        "verbs": tuple(sorted(verb.name for verb in commands)),
        "applied": applied,
        "next": following,
        "wrapped": wrapped,
        "saved": saved,
        "listed": listed_stems,
        "taken": taken[0],
        "menu": Option.of_optional(bpy.types.Menu.bl_rna_get_subclass_py("FIXTURE_MT_tone_presets")).map(lambda cls: sorted(cls.bl_options)),
    }


# --- [RELEASES]


def released_lists() -> dict[str, bool]:
    """Answers whether the list class, its selection index, the attached form, and the preset menu outlive release."""
    return {
        "class": bpy.types.UIList.bl_rna_get_subclass_py("FIXTURE_UL_entries") is not None,
        "index": "fixture_entries" in Look.bl_rna.properties,
        "attached": "fixture_look" in bpy.types.Scene.bl_rna.properties,
        "menu": bpy.types.Menu.bl_rna_get_subclass_py("FIXTURE_MT_tone_presets") is not None,
    }


# --- [COMPOSITION] ----------------------------------------------------------------------

LOOK: Final = Attachment(key="look", form=Look, owner=bpy.types.Scene)
STORE: Final = Store(attachment=LOOK)
ENTRIES: Final = ItemList(key="entries", collection=Member(root=Look, path=("entries",)), item=Entry, cells=(Label(text="Cell"),))
LOOKED: Final = Section(key="look", title="Look", holder=Some(Member(root=bpy.types.Scene, path=("fixture_look",))), store=Some(STORE), rows=(ENTRIES,))
LISTING: Final = Listing(preset=Preset(key="tone", form=Tone, stock={"soft": Tone(level=1), "hard": Tone(level=5)}), current=_current, matches=_matching, applied=_applied)

rows: Final = (LOOK, Panel(section=LOOKED, at=EditorRegion(space="PROPERTIES", region="WINDOW")), LISTING, PresetMenu(source=LISTING))
PROBES: Final = (bound, pressed, labelled, templated, tabled, composed, checked, fielded, walked, sectioned, listed, stored, preset)
RELEASED: Final = (released_lists,)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["PROBES", "RELEASED", "rows"]
