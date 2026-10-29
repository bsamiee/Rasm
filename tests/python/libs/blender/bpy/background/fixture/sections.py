"""Section rows: an attached form group with its store, a collection list, and a preset listing, with probes realizing sections and fields and running store, list, and preset commands."""

from collections.abc import Callable, Mapping
from types import MappingProxyType
from typing import Final

import bpy
from expression import Ok, Option, Some
import msgspec

from blender.bpy.commands.calls import called, Setting
from blender.bpy.commands.operators import Answer, Done
from blender.bpy.data.rna.paths import rooted
from blender.bpy.extension.identity import Identity
from blender.bpy.settings.attachments import Attachment
from blender.bpy.settings.presets import Preset
from blender.bpy.ui.layout.lists import ItemList
from blender.bpy.ui.layout.members import Member
from blender.bpy.ui.layout.presets import Listing, Verb
from blender.bpy.ui.layout.rows import Label, Layout
from blender.bpy.ui.layout.sections import fields, Items, reached, Section
from blender.bpy.ui.layout.values import Store
from blender.bpy.ui.surfaces.menus import PresetMenu
from blender.bpy.ui.surfaces.panels import EditorRegion, Panel
from blender.records.faults import Outcome

from .commands import operation

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


# --- [READS]


def _kinds(rows: tuple[object, ...]) -> tuple[str, ...]:
    """Names each row's kind, a section by its key."""
    return tuple(f"Section {row.key}" if isinstance(row, Section) else type(row).__name__ for row in rows)


# --- [PROBES]


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


def realized(identity: Identity) -> dict[str, object]:
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
    """Answers whether the list class, its selection index, and the attached form outlive release."""
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
PROBES: Final[Mapping[str, Callable[[Identity], object]]] = MappingProxyType({"fielded": fielded, "walked": walked, "realized": realized, "listed": listed, "stored": stored, "preset": preset})
RELEASED: Final[Mapping[str, Callable[[], object]]] = MappingProxyType({"lists": released_lists})

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["PROBES", "RELEASED", "rows"]
