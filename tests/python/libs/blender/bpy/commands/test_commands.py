"""Native command execution, report failures, context, macros, callbacks, keymaps, and command-line parsing."""

from base64 import b64encode
from collections.abc import Generator, Mapping
from contextlib import ExitStack
from functools import partial
import os
from pathlib import Path
import subprocess
import sys
from typing import Final

from bl_ui.space_view3d import VIEW3D_MT_pivot_pie
import bpy
from expression import Option, Some
import msgspec
import pytest

from blender.bpy.commands.invocations import Folder, Open, Popup, Save, Suppressible
from blender.bpy.commands.keymaps import Button, Chord, Keymap
from blender.bpy.commands.modal import Gesture, Modal
from blender.bpy.commands.operators import (
    Answer,
    BoundBody,
    Call,
    called,
    Command,
    Done,
    Empty,
    idname,
    Macro,
    Operation,
    operation,
    Operator,
    Placement,
    PopupKind,
    PopupTarget,
    rna_type,
    Setting,
    Unchanged,
)
from blender.bpy.data.rna.conditions import Setting as RequiredSetting
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.data.rna.paths import definition
from blender.bpy.data.rna.properties import Callbacks
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import CliCommand, Registration
from blender.bpy.ui.elements.panels import PreferencesPage
from blender.records.faults import Error, Fault, Nothing, Ok, Outcome, Reason
from tests.python.libs.blender.bpy.background.session import Acquired

# --- [CONSTANTS] ------------------------------------------------------------------------

COUNT: Final = "fixture_count"
CHOSEN: Final = "fixture_chosen"

# --- [MODELS] ---------------------------------------------------------------------------


class Amount(bpy.types.PropertyGroup):
    """Count step and the way it applies."""

    amount: bpy.props.IntProperty(name="Amount", default=1)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    mode: bpy.props.EnumProperty(name="Mode", items=(("ADD", "Add", "Adds the amount to the count"), ("SET", "Set", "Sets the count to the amount")))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [BODIES]


def counted(answer: Outcome[Answer], call: Call[Amount]) -> Outcome[Answer]:
    """Adds the amount to the scene count, or sets the count to it.

    Scene custom property holds the count.

    Returns:
        Supplied outcome after the scene write.
    """
    scene: bpy.types.Scene = call.context.scene  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    scene[COUNT] = call.properties.amount + (int(scene.get(COUNT, 0)) if call.properties.mode == "ADD" else 0)
    return answer


@Operator.of(Empty)
def kept(_call: Call[Empty]) -> Outcome[Answer]:
    """Leaves the file as it is."""
    return Ok(Unchanged(reason="Nothing to change"))


@Operator.of(Empty)
def refused(_call: Call[Empty]) -> Outcome[Answer]:
    """Refuses for a requirement the context lacks."""
    return Error(Fault.of(Reason.REQUIRED, "a fixture strip"))


def chosen(call: Call[Empty]) -> Outcome[Answer]:
    """Keeps the names of the paths its invoke form chose in a scene custom property."""
    scene: bpy.types.Scene = call.context.scene  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    scene[CHOSEN] = "/".join(path.name for path in call.paths)
    return Ok(Done())


def stepped(bound: str, call: Call[Amount]) -> Outcome[Answer]:
    """Sets the scene count to the amount times the length of the bound manifest ID."""
    scene: bpy.types.Scene = call.context.scene  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    scene[COUNT] = call.properties.amount * len(bound)
    return Ok(Done())


# --- [READS]


def _ran(identity: Identity, command: Command, values: Mapping[str, Setting | Option[Setting]]) -> tuple[Outcome[frozenset[str]], int]:
    """Calls a command from a zero count, answering its outcome and the count it left."""
    scene: bpy.types.Scene = bpy.context.scene  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    scene[COUNT] = 0
    return operation(identity, command).bind(lambda found: called(found, values)), int(scene[COUNT])


# --- [COMPOSITION] ----------------------------------------------------------------------

COUNTED: Final = Operator.of(Amount)(partial(counted, Ok(Done(notes=(("INFO", "Counted"),)))))
IDENTIFIED: Final = Operator.of(Amount, named=Some("identified"), callbacks=Some(lambda identity: {"amount": Callbacks[Amount](get=lambda _properties: len(identity.manifest.id))}))(COUNTED.run)
REDO: Final = Operator.of(Amount, named=Some("redo"), options=frozenset(), invoke=Some(Popup(partial(bpy.types.WindowManager.invoke_props_popup))))(COUNTED.run)
CONFIRMED: Final = Operator.of(Amount, named=Some("confirmed"), invoke=Some(Suppressible(message=lambda _context, _properties: Ok("Count now?"))))(COUNTED.run)
OPENED: Final = Operator.of(Empty, named=Some("opened"), invoke=Some(Open(extensions=lambda _properties: (".txt",))))(chosen)
SAVED: Final = Operator.of(Empty, named=Some("saved"), invoke=Some(Save(extensions=lambda _properties: (".txt",))))(chosen)
FOLDER: Final = Operator.of(Empty, named=Some("folder"), invoke=Some(Folder()))(chosen)
DRAGGED: Final = Operator.of(
    Amount,
    named=Some("dragged"),
    options=frozenset({"MODAL_PRIORITY", "UNDO"}),
    invoke=Some(Modal[None](gesture=Gesture.DRAG, begin=lambda _context, _travel: Ok(None), preview=lambda _context, _state, _travel: Ok(Nothing), restore=lambda _context, _state: Ok(None))),
)(COUNTED.run)
OVER: Final = Operator.of(Amount)(BoundBody(lambda identity: Ok(identity.manifest.id), stepped))
TWICE: Final = Macro(key="twice", description="Counts by two, then by three", steps=(Placement(COUNTED, {"amount": 2}), Placement(COUNTED, {"amount": 3})))
CANCEL_AFTER: Final = Operator.of(Amount, named=Some("cancel_after"))(partial(counted, Ok(Unchanged(reason="Fixture cancelled after write"))))
SKIP_AFTER: Final = Operator.of(Amount, named=Some("skip_after"))(partial(counted, Ok(Done(skipped=Fault.of(Reason.ABSENT, "fixture skipped item", member="target")))))
REFUSE_AFTER: Final = Operator.of(Amount, named=Some("refuse_after"))(partial(counted, Error(Fault.of(Reason.REQUIRED, "fixture command permission"))))
REPORT_AFTER: Final = Operator.of(Amount, named=Some("report_after"))(partial(counted, Ok(Done(notes=(("ERROR_INVALID_INPUT", "Fixture error report"),)))))
POLLED: Final = Operator.of(Amount, named=Some("polled_stage"), condition=Some(RequiredSetting(descriptor=Descriptor(root=bpy.types.Scene, prop=bpy.types.Scene.bl_rna.properties["use_gravity"]))))(
    COUNTED.run
)
PARTIAL_MACRO: Final = Macro(
    key="partial_macro", description="Writes before a later step cancels", steps=(Placement(COUNTED, {"amount": 2}), Placement(CANCEL_AFTER, {"amount": 3}), Placement(COUNTED, {"amount": 100}))
)
CANCELLED_MACRO: Final = Macro(key="cancelled_macro", description="Cancels before any step finishes", steps=(Placement(CANCEL_AFTER, {"amount": 3}), Placement(COUNTED, {"amount": 100})))
REPOLL_MACRO: Final = Macro(
    key="repoll_macro", description="Changes a later requirement after polling", steps=(Placement.context(bpy.ops.wm.context_toggle, ("scene", "use_gravity")), Placement(POLLED, {"amount": 7}))
)
KEYS: Final = Keymap(
    keymap="3D View",
    bindings=(
        (Chord(type="F", ctrl=1), Placement(COUNTED, {"amount": 4})),
        (Chord(type=Button.SELECT, alt=1), Placement.context(bpy.ops.wm.context_toggle, ("scene", "use_gravity"))),
        (Chord(type="G", shift=1), Placement.context(bpy.ops.wm.context_set_value, ("scene", "frame_current"), value="5")),
        (Chord(type="Q", alt=1), Placement(PopupTarget(VIEW3D_MT_pivot_pie, PopupKind.PIE))),
    ),
)
ECHO: Final = CliCommand(key="echo", decoder=msgspec.json.Decoder(dict[str, int]), run=lambda value, trailing: Ok(value["code"] + len(trailing)))


@pytest.fixture
def _command_scene() -> Generator[None]:
    """Scopes native command writes to a temporary scene and removes it after the case."""
    scene = bpy.data.scenes.new("Command test")
    with ExitStack() as release:
        release.callback(bpy.data.scenes.remove, scene)
        with bpy.context.temp_override(scene=scene):
            yield


@pytest.fixture
def echo_cli(acquired: Acquired, pytestconfig: pytest.Config) -> tuple[tuple[str, ...], dict[str, str]]:
    """Returns native CLI arguments with a child-owned extension scope and declared test-source imports."""
    with acquired(()) as identity:
        script = (
            f"from {__name__} import ECHO; "
            "import bpy; "
            "from contextlib import ExitStack; "
            "from tests.python.libs.blender.bpy.background.session import fixture; "
            "scope=ExitStack(); bpy.app.handlers.exit_pre.append(lambda *_arguments: scope.close()); "
            "acquire=scope.enter_context(fixture()); scope.enter_context(acquire((ECHO,)))"
        )
        argv = (bpy.app.binary_path, "--python-use-system-env", "--background", "--factory-startup", "--python-expr", script, "--command", identity.identifier(ECHO.key))
    return argv, os.environ | {"PYTHONPATH": os.pathsep.join(filter(None, (*map(str, pytestconfig.getini("pythonpath")), os.environ.get("PYTHONPATH"))))}


_SCENE: Final = pytest.mark.usefixtures(_command_scene.name)


# --- [TESTS] ----------------------------------------------------------------------------
# --- [CANCELLATION]


@_SCENE
@pytest.mark.parametrize("command", [COUNTED, SKIP_AFTER], ids=lambda command: command.key)
def test_completion_and_warnings_retain_native_writes(acquired: Acquired, command: Command) -> None:
    """Completion and skipped-item warnings return FINISHED after the scene write."""
    with acquired((command,)) as identity:
        assert _ran(identity, command, {"amount": 3}) == (Ok(frozenset({"FINISHED"})), 3)


@_SCENE
def test_cancellation_retains_the_native_write(acquired: Acquired) -> None:
    """Cancelled execution retains preceding writes and records the native return set."""
    with acquired((CANCEL_AFTER,)) as identity:
        answer, count = _ran(identity, CANCEL_AFTER, {"amount": 3})
        faults = answer.swap().default_with(lambda value: pytest.fail(repr(value)))
        assert len(faults) == 1
        fault = faults[0]
        assert count == 3
        assert fault.reason is Reason.ANSWERED
        assert fault.member == "CANCELLED"
        assert fault.subject == f"bpy.ops.{idname(identity, CANCEL_AFTER)}(amount=3)"


@_SCENE
@pytest.mark.parametrize(
    "command, values, cause, retained, message",
    [
        (REFUSE_AFTER, {"amount": 3}, RuntimeError, 3, "fixture command permission"),
        (REPORT_AFTER, {"amount": 3}, RuntimeError, 3, "Fixture error report"),
        (COUNTED, {"amount": "refused"}, TypeError, 0, "amount"),
        (COUNTED, {"undeclared": 3}, TypeError, 0, "undeclared"),
    ],
)
def test_failures_preserve_native_causes_and_write_order(acquired: Acquired, command: Command, values: Mapping[str, Setting], cause: type[Exception], retained: int, message: str) -> None:
    """Input refusal precedes execution while reported failures retain completed writes."""
    with acquired((command,)) as identity:
        answer, count = _ran(identity, command, values)
        faults = answer.swap().default_with(lambda value: pytest.fail(repr(value)))
        assert len(faults) == 1
        fault = faults[0]
        assert count == retained
        assert fault.reason is Reason.FAILED
        assert isinstance(fault.cause, cause)
        assert message in str(fault.cause)
        assert fault.subject.startswith(f"bpy.ops.{idname(identity, command)}(")


# --- [CONTEXT]


def test_poll_and_execution_use_the_overridden_scene(acquired: Acquired) -> None:
    """Poll refuses another scene before mutation and execution writes only that scene."""
    with acquired((POLLED,)) as identity, ExitStack() as stack:
        source = bpy.data.scenes.new("Command source")
        target = bpy.data.scenes.new("Command target")
        stack.callback(bpy.data.batch_remove, ids=(source, target))
        source[COUNT], target[COUNT], target.use_gravity = 19, 0, False
        native: Operation = operation(identity, POLLED).default_with(lambda faults: pytest.fail(repr(faults)))

        with bpy.context.temp_override(scene=source):
            blocked = called(native, {"amount": 3}, override=Some(bpy.context.temp_override(scene=target)))
            faults = blocked.swap().default_with(lambda value: pytest.fail(repr(value)))
            assert len(faults) == 1
            fault = faults[0]
            assert fault.reason is Reason.FAILED
            assert isinstance(fault.cause, RuntimeError)
            assert "poll()" in str(fault.cause)
            assert "Requires" in str(fault.cause)
            assert bpy.types.Scene.bl_rna.properties["use_gravity"].name in str(fault.cause)
            assert (source[COUNT], target[COUNT]) == (19, 0)
            assert bpy.context.scene == source

            target.use_gravity = True
            assert called(native, {"amount": 3}, override=Some(bpy.context.temp_override(scene=target))) == Ok(frozenset({"FINISHED"}))
            assert (source[COUNT], target[COUNT]) == (19, 3)
            assert bpy.context.scene == source


# --- [MACROS]


@_SCENE
def test_macro_acquires_its_step_declarations(acquired: Acquired) -> None:
    """A macro alone acquires its native steps and releases their callable with its own."""
    with acquired((TWICE,)) as identity:
        assert _ran(identity, TWICE, {}) == (Ok(frozenset({"FINISHED"})), 5)
        assert _ran(identity, COUNTED, {"amount": 2}) == (Ok(frozenset({"FINISHED"})), 2)
    assert operation(identity, TWICE).swap().default_with(lambda value: pytest.fail(repr(value)))[0].reason is Reason.ABSENT
    assert operation(identity, COUNTED).swap().default_with(lambda value: pytest.fail(repr(value)))[0].reason is Reason.ABSENT


@_SCENE
def test_partial_macro_cancellation_keeps_finished_steps(acquired: Acquired) -> None:
    """Native macro completion retains partial writes and skips every later step."""
    with acquired((PARTIAL_MACRO,)) as identity:
        assert _ran(identity, PARTIAL_MACRO, {}) == (Ok(frozenset({"FINISHED"})), 5)


@_SCENE
def test_initial_macro_cancellation_skips_later_steps(acquired: Acquired) -> None:
    """Macro cancellation before any completed step retains the cancelled body's write."""
    with acquired((CANCELLED_MACRO,)) as identity:
        answer, count = _ran(identity, CANCELLED_MACRO, {})
        faults = answer.swap().default_with(lambda value: pytest.fail(repr(value)))
        assert len(faults) == 1
        fault = faults[0]
        assert count == 3
        assert fault.reason is Reason.ANSWERED
        assert fault.member == "CANCELLED"


def test_native_macro_polls_before_its_steps(acquired: Acquired) -> None:
    """Native execution runs a later step after an earlier step changes its requirement."""
    with acquired((REPOLL_MACRO,)) as identity, ExitStack() as stack:
        scene = bpy.data.scenes.new("Macro context")
        stack.callback(bpy.data.scenes.remove, scene)
        scene.use_gravity = True
        with bpy.context.temp_override(scene=scene):
            assert _ran(identity, REPOLL_MACRO, {}) == (Ok(frozenset({"FINISHED"})), 7)
            assert not scene.use_gravity


# --- [CALLBACKS]


@_SCENE
def test_identity_callback_preserves_the_shared_form(acquired: Acquired) -> None:
    """Identity-bound getters remain separate from explicit values and the static default."""
    with acquired((COUNTED, IDENTIFIED)) as identity:
        finished = Ok(frozenset({"FINISHED"}))
        bound = len(identity.manifest.id)
        assert _ran(identity, IDENTIFIED, {}) == (finished, bound)
        assert _ran(identity, COUNTED, {"amount": 19}) == (finished, 19)
        assert _ran(identity, IDENTIFIED, {}) == (finished, bound)
        assert _ran(identity, COUNTED, {"amount": Nothing}) == (finished, 1)


@_SCENE
def test_operator_release_removes_the_native_callable(acquired: Acquired) -> None:
    """Registration release removes the operator and preserves native lookup failure."""
    with acquired((COUNTED,)) as identity:
        assert _ran(identity, COUNTED, {"amount": 3}) == (Ok(frozenset({"FINISHED"})), 3)
    faults = operation(identity, COUNTED).swap().default_with(lambda value: pytest.fail(repr(value)))
    assert len(faults) == 1
    fault = faults[0]
    assert fault.reason is Reason.ABSENT
    assert isinstance(fault.cause, KeyError)
    assert fault.member == idname(identity, COUNTED)


# --- [KEYMAPS]


@_SCENE
@pytest.mark.parametrize("selection, value, pie_drag", [("LEFT", "CLICK", False), ("RIGHT", "PRESS", True)])
def test_keymap_uses_native_preferences_and_placement_properties(acquired: Acquired, selection: str, value: str, *, pie_drag: bool) -> None:
    """Native shortcuts follow the active mouse preference and retain context, pie, and command properties."""
    manager: bpy.types.WindowManager = bpy.context.window_manager  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    active: bpy.types.KeyConfig = manager.keyconfigs.active  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    preferences: bpy.types.KeyConfigPreferences = active.preferences  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    with ExitStack() as release:
        release.callback(setattr, preferences, "select_mouse", preferences.select_mouse)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        release.callback(setattr, preferences, "use_pie_click_drag", preferences.use_pie_click_drag)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        preferences.select_mouse, preferences.use_pie_click_drag = selection, pie_drag  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        with acquired((KEYS,)) as identity:
            items = tuple(item for _, item in KEYS.counterparts(bpy.context))
            counting = tuple(item for item in items if item.idname == idname(identity, COUNTED))
            assert {(item.type, item.ctrl, item.oskey) for item in counting} == ({("F", 1, 0), ("F", 0, 1)} if sys.platform == "darwin" else {("F", 1, 0)})
            for item in counting:
                properties: Amount = item.properties  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
                assert _ran(identity, COUNTED, {"amount": properties.amount}) == (Ok(frozenset({"FINISHED"})), 4)
            (selecting,) = tuple(item for item in items if item.idname == "wm.context_toggle")
            selecting_properties: bpy.types.OperatorProperties = selecting.properties  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            assert (selecting.type, selecting.value, selecting.alt, selecting_properties.data_path) == (f"{selection}MOUSE", value, 1, "scene.use_gravity")
            (setting,) = tuple(item for item in items if item.idname == "wm.context_set_value")
            setting_properties: bpy.types.OperatorProperties = setting.properties  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            assert (setting.type, setting.shift, setting_properties.data_path, setting_properties.value) == ("G", 1, "scene.frame_current", "5")
            (pie,) = tuple(item for item in items if item.idname == "wm.call_menu_pie")
            pie_properties: bpy.types.OperatorProperties = pie.properties  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            menu: type[bpy.types.Menu] = VIEW3D_MT_pivot_pie  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            assert (pie.type, pie.alt, pie.value, pie_properties.name) == ("Q", 1, "CLICK_DRAG" if pie_drag else "PRESS", definition(menu).identifier)
        assert KEYS.counterparts(bpy.context) == ()
        assert operation(identity, COUNTED).swap().default_with(lambda value: pytest.fail(repr(value)))[0].reason is Reason.ABSENT


@_SCENE
def test_shared_keymap_release_preserves_native_entries_and_user_edits(acquired: Acquired) -> None:
    """Independent releases preserve another source, edited counterpart, and unrelated entries."""
    with acquired((COUNTED,)) as identity, ExitStack() as stack:
        manager: bpy.types.WindowManager = bpy.context.window_manager  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        addon: bpy.types.KeyConfig = manager.keyconfigs.addon  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        user: bpy.types.KeyConfig = manager.keyconfigs.user  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        native: Operation = operation(identity, COUNTED).default_with(lambda faults: pytest.fail(repr(faults)))
        keymap = addon.keymaps.new("3D View", space_type="VIEW_3D", region_type="WINDOW")
        left = Keymap(keymap=keymap.name, bindings=((Chord(type="F19"), Placement(native, {"amount": 11})),))
        right = Keymap(keymap=keymap.name, bindings=((Chord(type="F19"), Placement(native, {"amount": 13})),))
        first, second = Registration(identity.package, (left,)), Registration(identity.package, (right,))
        stack.callback(manager.keyconfigs.update, keep_properties=True)
        unrelated = keymap.keymap_items.new("wm.context_toggle", type="F19", value="PRESS")
        unrelated_properties: bpy.types.OperatorProperties = unrelated.properties  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        unrelated_properties.data_path = "scene.use_gravity"
        unrelated_id = unrelated.id
        stack.callback(keymap.keymap_items.remove, unrelated)
        baseline = frozenset(item.id for item in keymap.keymap_items)
        first.register()
        stack.callback(first.unregister)
        second.register()
        stack.callback(second.unregister)
        manager.keyconfigs.update(keep_properties=True)

        ((_, left_item),) = left.counterparts(bpy.context)
        ((user_map, right_item),) = right.counterparts(bpy.context)
        left_properties: Amount = left_item.properties  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        right_properties: Amount = right_item.properties  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        assert left_properties.amount == 11
        assert right_properties.amount == 13
        custom = user_map.keymap_items.new("wm.context_set_int", type="F19", value="PRESS")
        custom_properties: bpy.types.OperatorProperties = custom.properties  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        custom_properties.data_path, custom_properties.value = "scene.frame_current", 23
        custom_operator, custom_event = custom.idname, custom.type

        def custom_item() -> tuple[bpy.types.KeyMap, bpy.types.KeyMapItem]:
            current: bpy.types.KeyMap = user.keymaps.find_match(keymap)  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            return current, next(item for item in current.keymap_items if item.idname == custom_operator and item.type == custom_event)

        def remove_custom() -> None:
            current, held = custom_item()
            current.keymap_items.remove(held)

        def restore_right() -> None:
            for owner, held in right.counterparts(bpy.context):
                owner.restore_item_to_default(held)
            manager.keyconfigs.update(keep_properties=True)

        stack.callback(remove_custom)
        with ExitStack() as edits:
            edits.callback(restore_right)
            right_item.type, right_item.alt, right_properties.amount = "F20", 1, 17
            manager.keyconfigs.update(keep_properties=True)
            first.unregister()
            assert left.counterparts(bpy.context) == ()
            ((_, edited),) = right.counterparts(bpy.context)
            edited_properties: Amount = edited.properties  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            assert (edited.type, edited.alt, edited_properties.amount) == ("F20", 1, 17)
            assert _ran(identity, COUNTED, {"amount": edited_properties.amount}) == (Ok(frozenset({"FINISHED"})), 17)
            (remaining,) = tuple(item for item in keymap.keymap_items if item.id not in baseline)
            remaining_properties: Amount = remaining.properties  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            assert (remaining.type, remaining_properties.amount) == ("F19", 13)

        second.unregister()
        assert right.counterparts(bpy.context) == ()
        assert frozenset(item.id for item in keymap.keymap_items) == baseline
        unrelated_source: bpy.types.KeyMapItem = keymap.keymap_items.from_id(unrelated_id)  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        source_properties: bpy.types.OperatorProperties = unrelated_source.properties  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        assert source_properties.data_path == "scene.use_gravity"
        _, unrelated_user = custom_item()
        user_properties: bpy.types.OperatorProperties = unrelated_user.properties  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        assert (user_properties.data_path, user_properties.value) == ("scene.frame_current", 23)

        invalid = Registration(identity.package, (Keymap(keymap=keymap.name, bindings=((Chord(type="F19"), Placement(native, {"undeclared": 1})),)),))
        stack.callback(invalid.unregister)
        with pytest.raises(RuntimeError, match="undeclared"):
            invalid.register()
        assert frozenset(item.id for item in keymap.keymap_items) == baseline
        assert keymap.keymap_items.from_id(unrelated_id) == unrelated_source
        _, unrelated_user = custom_item()
        user_properties = unrelated_user.properties  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        assert (user_properties.data_path, user_properties.value) == ("scene.frame_current", 23)


@_SCENE
def test_enum_values_choose_set_then_default_add(acquired: Acquired) -> None:
    """Native enum values select replacement while a later unset value uses the declared default."""
    with acquired((COUNTED,)) as identity:
        scene: bpy.types.Scene = bpy.context.scene  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        scene[COUNT] = 19
        native = operation(identity, COUNTED).default_with(lambda faults: pytest.fail(repr(faults)))
        assert called(native, {"amount": 3, "mode": "SET"}) == Ok(frozenset({"FINISHED"}))
        assert scene[COUNT] == 3
        assert called(native, {"amount": 2, "mode": Nothing}) == Ok(frozenset({"FINISHED"}))
        assert scene[COUNT] == 5


@_SCENE
def test_bound_body_reads_registered_identity(acquired: Acquired) -> None:
    """Bound body computes its native write from the acquired extension identity."""
    with acquired((OVER,)) as identity:
        assert _ran(identity, OVER, {"amount": 2}) == (Ok(frozenset({"FINISHED"})), 2 * len(identity.manifest.id))


@_SCENE
def test_empty_and_refused_bodies_preserve_state(acquired: Acquired) -> None:
    """Empty and refused native bodies keep the counter unchanged and retain their distinct diagnoses."""
    with acquired((kept, refused)) as identity:
        unchanged, count = _ran(identity, kept, {})
        faults = unchanged.swap().default_with(lambda value: pytest.fail(repr(value)))
        assert len(faults) == 1
        fault = faults[0]
        assert count == 0
        assert fault.reason is Reason.ANSWERED
        assert fault.member == "CANCELLED"
        denied, count = _ran(identity, refused, {})
        faults = denied.swap().default_with(lambda value: pytest.fail(repr(value)))
        assert len(faults) == 1
        fault = faults[0]
        assert count == 0
        assert fault.reason is Reason.FAILED
        assert isinstance(fault.cause, RuntimeError)
        assert "Requires a fixture strip" in str(fault.cause)


@pytest.mark.parametrize(
    "command, options",
    [pytest.param(REDO, frozenset({"REGISTER", "UNDO"}), id=REDO.key), pytest.param(DRAGGED, frozenset({"REGISTER", "UNDO", "BLOCKING", "GRAB_CURSOR", "MODAL_PRIORITY"}), id=DRAGGED.key)],
)
def test_native_flags_control_undo_and_modal_contracts(acquired: Acquired, command: Command, options: frozenset[str]) -> None:
    """Registered native classes expose the requested undo and modal options."""
    with acquired((command,)) as identity:
        native = bpy.types.Operator.bl_rna_get_subclass_py(rna_type(operation(identity, command).default_with(lambda faults: pytest.fail(repr(faults)))).identifier)
        assert native is not None
        assert native.bl_options == options


@_SCENE
def test_browser_paths_and_confirmation_use_native_properties(acquired: Acquired, tmp_path: Path) -> None:
    """Native file properties reach the body and confirmation writes the preference after execution."""
    with acquired((FOLDER, OPENED, SAVED, CONFIRMED, PreferencesPage())) as identity:
        scene: bpy.types.Scene = bpy.context.scene  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        target = tmp_path / "chosen.txt"
        target.write_text("chosen", encoding="utf-8")
        scene[CHOSEN] = target.name
        native = operation(identity, FOLDER).default_with(lambda faults: pytest.fail(repr(faults)))
        faults = called(native, {}).swap().default_with(lambda value: pytest.fail(repr(value)))
        assert len(faults) == 1
        fault = faults[0]
        assert fault.reason is Reason.FAILED
        assert isinstance(fault.cause, RuntimeError)
        assert scene[CHOSEN] == target.name
        assert called(native, {"directory": str(tmp_path)}) == Ok(frozenset({"FINISHED"}))
        assert scene[CHOSEN] == tmp_path.name
        assert called(operation(identity, OPENED).default_with(lambda faults: pytest.fail(repr(faults))), {"filepath": str(target)}) == Ok(frozenset({"FINISHED"}))
        assert scene[CHOSEN] == target.name
        destination = tmp_path / "saved.txt"
        assert called(operation(identity, SAVED).default_with(lambda faults: pytest.fail(repr(faults))), {"filepath": str(destination)}) == Ok(frozenset({"FINISHED"}))
        assert scene[CHOSEN] == destination.name
        preferences = identity.preferences(bpy.types.AddonPreferences).default_with(lambda: pytest.fail("Registered preferences are absent"))
        flag = Suppressible.flag(CONFIRMED.key)
        assert getattr(preferences, flag) is True
        assert _ran(identity, CONFIRMED, {"suppress": True}) == (Ok(frozenset({"FINISHED"})), 1)
        assert getattr(preferences, flag) is False


def test_registered_cli_preserves_trailing_host_tokens(echo_cli: tuple[tuple[str, ...], dict[str, str]]) -> None:
    """Blender passes trailing host tokens to the registered command without executing them."""
    argv, environment = echo_cli
    code = 7
    trailing = "--", "--python-expr", "raise SystemExit(99)"
    document = b64encode(msgspec.json.encode({"code": code})).decode()
    child = subprocess.run((*argv, document, *trailing), env=environment, capture_output=True, text=True, check=False)
    assert child.returncode == code + len(trailing), child.stdout + child.stderr


@pytest.mark.parametrize("arguments, message", [((), "arguments"), (("%%",), "unreadable"), ((b64encode(msgspec.json.encode({"code": "seven"})).decode(),), "expected `int`, got `str`")])
def test_registered_cli_reports_typed_document_refusals(echo_cli: tuple[tuple[str, ...], dict[str, str]], arguments: tuple[str, ...], message: str) -> None:
    """Native command parsing reports missing arguments and codec failures with a failing exit."""
    argv, environment = echo_cli
    child = subprocess.run((*argv, *arguments), env=environment, capture_output=True, text=True, check=False)
    assert child.returncode == 1, child.stdout + child.stderr
    assert message in (child.stdout + child.stderr).lower()
