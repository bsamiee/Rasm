"""Command rows: operators of every answer and invoke form, a decorated body, a step over an identity binding, a macro, add-on shortcuts, and a command-line command, with probes running and reading them."""

from base64 import b64encode
from collections.abc import Callable, Mapping
from functools import partial
from pathlib import Path
from typing import Final

from bl_ui.space_view3d import VIEW3D_MT_add, VIEW3D_MT_pivot_pie
import bpy
from bpy_extras.io_utils import ExportHelper, ImportHelper
from expression import Error, Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.calls import Called, called, Command, filled, idname, Opened, Operation, Placement, Setting
from blender.bpy.commands.invocations import Folder, Open, Pick, Popup, Save, Suppressible
from blender.bpy.commands.keymaps import Button, Chord, Keymap, nesting
from blender.bpy.commands.modal import Gesture, Modal
from blender.bpy.commands.operators import Answer, Call, Done, Empty, Macro, Operator, Over, tallied, Unchanged
from blender.bpy.data.rna.paths import definition
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import CliCommand
from blender.records.faults import Fault, Outcome, Reason

# --- [CONSTANTS] ------------------------------------------------------------------------

COUNT: Final = "fixture_count"
OPERATORS: Final[object] = bpy.ops.fixture  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
CHOSEN: Final = "fixture_chosen"

# --- [MODELS] ---------------------------------------------------------------------------


class Amount(bpy.types.PropertyGroup):
    """Count step and the way it applies."""

    amount: bpy.props.IntProperty(name="Amount", default=1)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    mode: bpy.props.EnumProperty(name="Mode", items=(("ADD", "Add", "Adds the amount to the count"), ("SET", "Set", "Sets the count to the amount")))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [BODIES]


def counted(call: Call[Amount]) -> Outcome[Answer]:
    """Adds the amount to the scene count, or sets the count to it.

    The count lives in a scene custom property.

    Returns:
        Done noting the count.
    """
    scene: bpy.types.Scene = call.context.scene  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    scene[COUNT] = call.properties.amount + (int(scene.get(COUNT, 0)) if call.properties.mode == "ADD" else 0)
    return Ok(Done(notes=(("INFO", "Counted"),)))


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


def operation(identity: Identity, command: Command) -> Operation:
    """Returns the `bpy.ops` callable of an extension command."""
    module, _, name = idname(identity, command).default_value("").partition(".")
    found: Operation = getattr(getattr(bpy.ops, module), name)
    return found


def _ran(identity: Identity, command: Command, values: Mapping[str, Setting | Option[Setting]]) -> tuple[Outcome[frozenset[str]], int]:
    """Calls a command from a zero count, answering its outcome and the count it left."""
    scene: bpy.types.Scene = bpy.context.scene  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    scene[COUNT] = 0
    return called(operation(identity, command), values), int(scene[COUNT])


def binding(item: bpy.types.KeyMapItem) -> dict[str, object]:
    """Reads a keymap item: operator, event, and set properties."""
    return {
        "idname": item.idname,
        "event": (item.type, item.value, item.shift, item.ctrl, item.alt, item.oskey),
        "properties": {
            prop.identifier: getattr(properties, prop.identifier)
            for properties in Option.of_optional(item.properties).to_list()
            for prop in definition(properties).properties
            if prop.identifier != "rna_type" and properties.is_property_set(prop.identifier)
        },
    }


def described(identity: Identity, command: Command) -> dict[str, object]:
    """Reads the registered class of a command: flags, the optional members Nothing where the class leaves them to the host, texts, helpers, callbacks, and properties."""
    rna: bpy.types.Struct = operation(identity, command).get_rna_type()  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    cls: type[bpy.types.Operator] = getattr(bpy.types, rna.identifier)

    def declared[T](read: Callable[[], T]) -> Option[T]:
        return catch(read, exception=AttributeError)().to_option()

    return {
        "options": sorted(cls.bl_options),
        "property": declared(lambda: cls.bl_property),
        "undo_group": declared(lambda: cls.bl_undo_group),
        "cursor": declared(lambda: cls.bl_cursor_pending),
        "label": cls.bl_label,
        "description": cls.bl_description,
        "helpers": sorted(helper.__name__ for helper in (ImportHelper, ExportHelper) if issubclass(cls, helper)),
        "members": sorted({"invoke", "modal", "cancel", "draw", "check"}.intersection(dir(cls))),
        "properties": sorted(prop.identifier for prop in rna.properties if prop.identifier != "rna_type"),
    }


# --- [PROBES]


def classes(identity: Identity) -> dict[str, dict[str, object]]:
    """Reads the registered class of every command."""
    return {command.key: described(identity, command) for command in COMMANDS}


def executed(identity: Identity) -> dict[str, object]:
    """Runs commands by their exec form, answering each outcome with the count it left."""
    return {
        "counted": _ran(identity, COUNTED, {"amount": 3}),
        "set": _ran(identity, COUNTED, {"amount": 3, "mode": "SET"}),
        "unset": _ran(identity, COUNTED, {"amount": Nothing}),
        "kept": _ran(identity, kept, {}),
        "refused": _ran(identity, refused, {}),
        "macro": _ran(identity, TWICE, {}),
        "bogus": _ran(identity, COUNTED, {"bogus": 1}),
        "over": _ran(identity, OVER, {"amount": 2}),
    }


def refusal(identity: Identity) -> str:
    """Answers the host error text of a refused command."""
    return called(operation(identity, refused), {}).swap().map(lambda faults: str(faults[0].cause)).default_value("")


def suppressed(identity: Identity) -> dict[str, object]:
    """Runs the suppressible command with its checkbox set, reading the preference flag before and after."""

    def flag() -> object:
        return identity.preferences(bpy.types.AddonPreferences).map(lambda held: bool(getattr(held, Suppressible.flag(CONFIRMED.key))))

    before = flag()
    return {"before": before, "ran": _ran(identity, CONFIRMED, {"suppress": True}), "after": flag()}


def browsed(identity: Identity) -> dict[str, object]:
    """Runs the folder and file commands without and with a chosen path, answering each outcome with the path names its body received."""
    scene: bpy.types.Scene = bpy.context.scene  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    folder = Path(bpy.app.tempdir)
    target = folder / "chosen.txt"
    target.write_text("chosen", encoding="utf-8")

    def chose(command: Command, values: Mapping[str, Setting | Option[Setting]]) -> tuple[Outcome[frozenset[str]], str]:
        scene[CHOSEN] = ""
        return _ran(identity, command, values)[0], str(scene[CHOSEN]).replace(folder.name, "<folder>")

    return {"unchosen": chose(FOLDER, {}), "folder": chose(FOLDER, {"directory": str(folder)}), "opened": chose(OPENED, {"filepath": str(target)})}


def answered(_identity: Identity) -> dict[str, object]:
    """Folds item edits into the command answer: every edit changed, some refused, every edit refused."""
    failed = Fault.of(Reason.ABSENT, "fixture item")
    return {"all": tallied((Ok(1), Ok(2))), "some": tallied((Ok(1), Error(failed))), "none": tallied((Error(failed), Error(failed)))}


def named(identity: Identity) -> dict[str, object]:
    """Resolves host names of commands, menus, panels, shelves, stock classes, and stock operators."""
    return {
        "command": idname(identity, COUNTED),
        "menu": idname(identity, Opened("main", Called.MENU)),
        "pie": idname(identity, Opened("main", Called.PIE)),
        "panel": idname(identity, Opened("main", Called.PANEL)),
        "shelf": idname(identity, Opened("main", Called.SHELF)),
        "stock menu": idname(identity, Opened(VIEW3D_MT_add, Called.MENU)),
        "stock operator": idname(identity, bpy.ops.wm.context_toggle),
    }


def placed(identity: Identity) -> dict[str, object]:
    """Resolves placements into the host keymap-data form."""
    return {
        "command": Placement(COUNTED, {"amount": 4}).resolved(identity),
        "pie": Placement(Opened(VIEW3D_MT_pivot_pie, Called.PIE)).resolved(identity),
        "toggle": Placement.context(bpy.ops.wm.context_toggle, ("scene", "use_gravity")).resolved(identity),
        "assign": Placement.context(bpy.ops.wm.context_set_value, ("scene", "frame_current"), value="5").resolved(identity),
        "unfilled": filled(("fixture.counted", (("amount", 1),)), object()),
    }


def mismatched(_identity: Identity) -> str:
    """Opens a menu class as a panel, answering the error type construction raises."""
    return catch(Opened, exception=TypeError)(VIEW3D_MT_add, Called.PANEL).swap().map(lambda error: type(error).__name__).default_value("")


def shortcuts(_identity: Identity) -> dict[str, object]:
    """Reads the add-on keymap items the fixture holds and their user counterparts after a keyconfig update."""
    manager: bpy.types.WindowManager = bpy.context.window_manager  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    manager.keyconfigs.update()
    return {"items": keys(), "counterparts": len(KEYS.counterparts(bpy.context))}


def nested(_identity: Identity) -> dict[str, object]:
    """Locates keymap names in the host keymap hierarchy."""
    return {"view": nesting("3D View"), "absent": nesting("Fixture Map")}


def gestures(_identity: Identity) -> dict[str, object]:
    """Reads the operator flags each gesture declares."""
    return {gesture.name: sorted(gesture.options) for gesture in Gesture}


def commanded(_identity: Identity) -> dict[str, object]:
    """Runs the command-line command over an encoded document, no argument, and an undecodable argument."""
    document = b64encode(msgspec.json.encode({"code": 3})).decode()
    return {"code": ECHO.executed("fixture_echo", [document, "trailing"]), "missing": ECHO.executed("fixture_echo", []), "garbled": ECHO.executed("fixture_echo", ["%%"])}


def unbound(_identity: Identity) -> object:
    """Calls an operator the host lacks."""
    absent: Operation = bpy.ops.wm.fixture_absent  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    return called(absent, {})


# --- [RELEASES]


def operators() -> tuple[str, ...]:
    """Lists the extension operators the host still holds."""
    return tuple(dir(OPERATORS))


def keys() -> tuple[dict[str, object], ...]:
    """Reads every 3D View item of the add-on keyconfig."""
    manager: bpy.types.WindowManager = bpy.context.window_manager  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    addon: bpy.types.KeyConfig = manager.keyconfigs.addon  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    return tuple(binding(item) for keymap in addon.keymaps if keymap.name == "3D View" for item in keymap.keymap_items)


# --- [COMPOSITION] ----------------------------------------------------------------------

COUNTED: Final = Operator.of(Amount)(counted)
SEEDED: Final = Operator.of(Amount, named=Some("seeded"), seed=Some(lambda _properties, _event, _identity: {"amount": 7}))(counted)
GROUPED: Final = Operator.of(Empty, named=Some("grouped"), undo=Some(Some("fixture")))(kept.run)
PLAIN: Final = Operator.of(Empty, named=Some("plain"), undo=Nothing, presets=True, internal=True)(kept.run)
DIALOG: Final = Operator.of(Amount, named=Some("dialog"), invoke=Some(Popup(partial(bpy.types.WindowManager.invoke_props_dialog))))(counted)
REDO: Final = Operator.of(Amount, named=Some("redo"), undo=Nothing, invoke=Some(Popup(partial(bpy.types.WindowManager.invoke_props_popup))))(counted)
SEARCH: Final = Operator.of(Amount, named=Some("search"), invoke=Some(Popup(partial(bpy.types.WindowManager.invoke_search_popup))))(counted)
CONFIRMED: Final = Operator.of(Amount, named=Some("confirmed"), invoke=Some(Suppressible(message=lambda _context, _properties: "Count now?")))(counted)
OPENED: Final = Operator.of(Empty, named=Some("opened"), invoke=Some(Open(extensions=lambda _properties: (".txt",))))(chosen)
SAVED: Final = Operator.of(Empty, named=Some("saved"), invoke=Some(Save(extensions=lambda _properties: (".txt",))))(chosen)
FOLDER: Final = Operator.of(Empty, named=Some("folder"), invoke=Some(Folder()))(chosen)
PICKED: Final = Operator.of(Empty, named=Some("picked"), invoke=Some(Pick(cursor="EYEDROPPER")))(kept.run)
DRAGGED: Final = Operator.of(
    Empty,
    named=Some("dragged"),
    invoke=Some(
        Modal[None](gesture=Gesture.DRAG, begin=lambda _context, _travel: Ok(None), preview=lambda _context, _state, _travel: Ok(Nothing), restore=lambda _context, _state: Ok(None), priority=True)
    ),
)(kept.run)
OVER: Final = Operator.of(Amount)(Over(lambda identity: Ok(identity.manifest.id), stepped))
TWICE: Final = Macro(key="twice", description="Counts by two, then by three", steps=(Placement(COUNTED, {"amount": 2}), Placement(COUNTED, {"amount": 3})))
COMMANDS: Final[tuple[Command, ...]] = (COUNTED, kept, refused, SEEDED, GROUPED, PLAIN, DIALOG, REDO, SEARCH, CONFIRMED, OPENED, SAVED, FOLDER, PICKED, DRAGGED, OVER, TWICE)
KEYS: Final = Keymap(
    keymap="3D View",
    bindings=(
        (Chord(type="F", ctrl=1), Placement(COUNTED, {"amount": 4})),
        (Chord(type=Button.SELECT, alt=1), Placement.context(bpy.ops.wm.context_toggle, ("scene", "use_gravity"))),
        (Chord(type="G", shift=1), Placement.context(bpy.ops.wm.context_set_value, ("scene", "frame_current"), value="5")),
        (Chord(type="Q", alt=1), Placement(Opened(VIEW3D_MT_pivot_pie, Called.PIE))),
    ),
)
ECHO: Final = CliCommand(key="echo", decoder=msgspec.json.Decoder(dict[str, int]), run=lambda value, _trailing: Ok(value["code"]))

rows: Final = (*COMMANDS, KEYS, ECHO)
PROBES: Final[tuple[Callable[[Identity], object], ...]] = (classes, executed, refusal, suppressed, browsed, answered, named, placed, mismatched, shortcuts, nested, gestures, commanded, unbound)
RELEASED: Final[tuple[Callable[[], object], ...]] = (operators, keys)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["COMMANDS", "COUNTED", "KEYS", "OPERATORS", "PROBES", "RELEASED", "binding", "described", "operation", "rows"]
