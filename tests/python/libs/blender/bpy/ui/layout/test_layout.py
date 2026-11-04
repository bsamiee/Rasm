"""Native reached members, mixed values, settings history, preset commands, and list release."""

from collections.abc import Mapping
from contextlib import ExitStack

import bpy
from expression import Option, Result, Some
import msgspec
import pytest

from blender.bpy.commands.operators import Answer, called, Done, operation, Setting
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.data.rna.paths import definition
from blender.bpy.settings.attachments import Attachment
from blender.bpy.settings.presets import Preset
from blender.bpy.ui.elements.menus import PresetMenu
from blender.bpy.ui.layout.lists import ItemList
from blender.bpy.ui.layout.members import marked, Member, owner_key, resolved_at
from blender.bpy.ui.layout.values import Listing, Store, StoreCommand
from blender.records.faults import Fault, Nothing, Ok, Outcome, Reason
from tests.python.libs.blender.bpy.background.session import Acquired

# --- [MODELS] ---------------------------------------------------------------------------


class Entry(bpy.types.PropertyGroup):
    """Native list item with an editable value."""

    value: bpy.props.IntProperty(name="Value")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Look(bpy.types.PropertyGroup):
    """Native settings attached to the scene for store and list operations."""

    exposure: bpy.props.FloatProperty(name="Exposure", default=1.0)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    entries: bpy.props.CollectionProperty(type=Entry)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Tone(msgspec.Struct, frozen=True, kw_only=True):
    """Preset value written to a native scene."""

    level: int


# --- [OPERATIONS] -----------------------------------------------------------------------


def test_owner_keys_distinguish_nameless_groups_and_survive_native_id_rename(acquired: Acquired) -> None:
    """Nameless native groups have distinct identities and reacquired wrappers retain their owner's key."""
    attachment = Attachment(key="owners", form=Look, owner=bpy.types.Scene)
    with ExitStack() as scope, acquired((attachment,)) as identity:
        first = bpy.data.scenes.new("First owner")
        scope.callback(bpy.data.scenes.remove, first)
        second = bpy.data.scenes.new("Second owner")
        scope.callback(bpy.data.scenes.remove, second)
        match attachment.read(identity, first), attachment.read(identity, second):
            case Result(tag="ok", ok=one), Result(tag="ok", ok=two):
                assert not one.name
                assert not two.name
                assert owner_key(one) != owner_key(two)
                assert attachment.read(identity, first).map(owner_key) == Ok(owner_key(one))
            case found:
                pytest.fail(repr(found))
        before = owner_key(first)
        first.name = "Renamed owner"
        assert owner_key(first) == before


def test_reached_member_resolves_one_live_native_owner() -> None:
    """Alternating reach runs once and the returned descriptor locates its first native scene."""
    scene = bpy.data.window_managers[0].windows[0].scene
    with ExitStack() as scope:
        other = bpy.data.scenes.new("Reached owner")
        scope.callback(bpy.data.scenes.remove, other)
        calls: list[bpy.types.Scene] = []

        def reach(_context: bpy.types.Context) -> Option[bpy.types.Scene]:
            chosen = scene if not calls else other
            calls.append(chosen)
            return Some(chosen)

        member = Member(root=bpy.types.Scene, path=("frame_start",), reach=Some(reach))
        match resolved_at(member, scene, bpy.context):
            case Result(tag="ok", ok=Option(some=(descriptor, (owner, name)))):
                assert calls == [scene]
                assert owner == scene
                assert descriptor.prop == scene.bl_rna.properties[name]
            case found:
                pytest.fail(repr(found))
        assert resolved_at(Member(root=bpy.types.Scene, path=member.path, reach=Some(lambda _: Nothing)), scene, bpy.context) == Ok(Nothing)


def test_mixed_scene_values_keep_native_label_and_equal_values_leave_it_unset() -> None:
    """Mixed native values add the translated property label and equal owners leave labeling to RNA."""
    scene = bpy.data.window_managers[0].windows[0].scene
    with ExitStack() as scope:
        other = bpy.data.scenes.new("Mixed owner")
        scope.callback(bpy.data.scenes.remove, other)
        other.frame_start = scene.frame_start + 1
        setting = Descriptor(root=bpy.types.Scene, prop=definition(bpy.types.Scene).properties["frame_start"])
        label = bpy.app.translations.pgettext_iface(setting.prop.name, setting.prop.translation_context)
        assert marked(setting, (scene, other)) == Ok(Some(f"{label} —"))
        assert marked(setting, (scene, scene)) == Ok(Nothing)


def test_store_reset_unsets_explicit_default_and_history_restores_values(acquired: Acquired) -> None:
    """Native reset restores values and unsets an explicitly stored default; history reverses both edits."""
    attachment = Attachment(key="look", form=Look, owner=bpy.types.Scene)
    store = Store(attachment=attachment)
    scene = bpy.data.window_managers[0].windows[0].scene
    with acquired((store,)) as identity:
        match attachment.read(identity, scene):
            case Result(tag="ok", ok=look):
                pass
            case failed:
                pytest.fail(repr(failed))

        def ran(command: StoreCommand, values: Mapping[str, Setting]) -> Outcome[frozenset[str]]:
            with bpy.context.temp_override(**{identity.identifier(attachment.key): scene}):  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                return operation(identity, store.commands[command]).bind(lambda found: called(found, values))

        edited = 2.0
        look.exposure = edited
        assert ran(StoreCommand.RESET, {}) == Ok(frozenset({"FINISHED"}))
        exposure: bpy.types.FloatProperty = definition(Look).properties["exposure"]  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        default = exposure.default
        assert look.exposure == default
        assert ran(StoreCommand.RESTORE, {"offset": 1}) == Ok(frozenset({"FINISHED"}))
        assert look.exposure == edited
        assert ran(StoreCommand.RESTORE, {"offset": -1}) == Ok(frozenset({"FINISHED"}))
        look.exposure = default
        assert look.is_property_set("exposure")
        assert ran(StoreCommand.RESET, {}) == Ok(frozenset({"FINISHED"}))
        assert not look.is_property_set("exposure")
        assert look.exposure == default
        match ran(StoreCommand.RESET, {}):
            case Result(error=(Fault(reason=reason),)):
                assert reason is Reason.ANSWERED
            case found:
                pytest.fail(repr(found))
        match operation(identity, store.commands[StoreCommand.RESET]).bind(lambda found: called(found, {})):
            case Result(error=(Fault(reason=reason, cause=RuntimeError()),)):
                assert reason is Reason.FAILED
            case found:
                pytest.fail(repr(found))
    assert identity.identifier(attachment.key) not in bpy.types.Scene.bl_rna.properties


def test_preset_commands_apply_step_wrap_save_and_refuse_taken_names(acquired: Acquired) -> None:
    """Native preset operators change scene values, wrap their order, save, and preserve a name collision."""
    scene = bpy.data.window_managers[0].windows[0].scene
    key = "native_tone"

    def current(_context: bpy.types.Context) -> Outcome[Tone]:
        return Ok(Tone(level=int(scene[key])))

    def matching(context: bpy.types.Context, values: tuple[Tone, ...]) -> Outcome[tuple[bool, ...]]:
        return current(context).map(lambda held: tuple(value == held for value in values))

    def applied(_context: bpy.types.Context, value: Tone) -> Outcome[Answer]:
        scene[key] = value.level
        return Ok(Done())

    listing = Listing(preset=Preset(key="tone", form=Tone, stock={"soft": Tone(level=1), "hard": Tone(level=5)}), current=current, matches=matching, applied=applied)
    menu = PresetMenu(source=listing)
    scene[key] = 0
    with ExitStack() as scope:
        scope.callback(scene.__delitem__, key)
        identity = scope.enter_context(acquired((listing, menu)))
        for command, values, expected in ((StoreCommand.APPLY, {"stem": "soft"}, 1), (StoreCommand.STEP, {"offset": 1}, 5), (StoreCommand.STEP, {"offset": 1}, 1)):
            assert operation(identity, listing.commands[command]).bind(lambda found: called(found, values)) == Ok(frozenset({"FINISHED"}))
            assert scene[key] == expected
        scene[key] = 9
        assert operation(identity, listing.commands[StoreCommand.SAVE]).bind(lambda found: called(found, {"name": "Mine"})) == Ok(frozenset({"FINISHED"}))
        assert tuple(listing.preset.listed(identity)) == ("soft", "hard", "Mine")
        match operation(identity, listing.commands[StoreCommand.SAVE]).bind(lambda found: called(found, {"name": "Soft"})):
            case Result(error=(Fault(reason=reason, cause=RuntimeError()),)):
                assert reason is Reason.FAILED
            case found:
                pytest.fail(repr(found))


def test_list_selection_uses_native_items_and_releases_registered_members(acquired: Acquired) -> None:
    """Registered list selects its native item and releases its class, index, and attachment."""
    attachment = Attachment(key="look", form=Look, owner=bpy.types.Scene)
    listing = ItemList(key="entries", collection=Member(root=Look, path=("entries",)))
    scene = bpy.data.window_managers[0].windows[0].scene
    with acquired((attachment, listing)) as identity:
        match attachment.read(identity, scene):
            case Result(tag="ok", ok=look):
                pass
            case failed:
                pytest.fail(repr(failed))
        look.entries.add().value = 3
        look.entries.add().value = 7
        index = identity.identifier(listing.key)
        setattr(look, index, 1)
        match listing.picked(identity):
            case Result(tag="ok", ok=picked):
                assert picked(look, bpy.context) == (look.entries[1],)
            case found:
                pytest.fail(repr(found))
        name = identity.name(bpy.types.UIList, listing.key)
        assert bpy.types.UIList.bl_rna_get_subclass_py(name) is not None
    assert bpy.types.UIList.bl_rna_get_subclass_py(name) is None
    assert index not in Look.bl_rna.properties
    assert identity.identifier(attachment.key) not in bpy.types.Scene.bl_rna.properties
