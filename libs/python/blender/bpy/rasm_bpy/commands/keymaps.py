"""Add-on shortcuts in the host keyconfig-data form, mouse roles read from the default keymap's parameters, with user keymap editors."""

from collections.abc import Generator, Iterator
from contextlib import contextmanager, ExitStack
from enum import auto, Enum
from functools import partial
import inspect
from pathlib import Path
import runpy
import sys
from typing import Literal, Protocol, Required, runtime_checkable, TYPE_CHECKING, TypedDict

from bl_keymap_utils import keymap_hierarchy, platform_helpers
import bpy
from expression import Option, Some
import rna_keymap_ui

from rasm_bpy.commands.operators import filled, Pairs, Placement, PopupKind, PopupTarget, Setting
from rasm_bpy.data._extension.identity import Identity
from rasm_bpy.data._extension.registration import acquired_rows, Acquisitions, Declaration, Tier
from rasm_bpy.data._rna.paths import rna_definition, root_in, Struct
from rasm_records import accumulated, Error, Fault, Nothing, Ok, Outcome, paired, Reason

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Location = tuple[str, rna_enums.SpaceTypeItems, rna_enums.RegionTypeItems]
type _Node = tuple[str, rna_enums.SpaceTypeItems, rna_enums.RegionTypeItems, list[_Node]]
type Bindings = tuple[tuple[Chord, Placement], ...]
type _ModifierState = Literal[-1, 0, 1]
type Row = tuple[str, _Event, dict[str, list[tuple[str, Setting]]]]


class _Modifiers(TypedDict, total=False):
    """`KeyMapItems.new` keywords besides the event type and value."""

    any: bool
    shift: _ModifierState
    ctrl: _ModifierState
    alt: _ModifierState
    oskey: _ModifierState
    hyper: _ModifierState
    key_modifier: "rna_enums.EventTypeItems"
    direction: "rna_enums.EventDirectionItems"
    repeat: bool
    head: bool


class _Event(_Modifiers):
    """Binding event in the host keyconfig-data form, its type and value resolved."""

    type: "rna_enums.EventTypeItems"
    value: "rna_enums.EventValueItems"


@runtime_checkable
class Parameters(Protocol):
    """Members of the default keymap's `Params` the mouse roles and pie presses read."""

    select_mouse: "rna_enums.EventTypeItems"
    select_mouse_value: "rna_enums.EventValueItems"
    action_mouse: "rna_enums.EventTypeItems"
    tool_mouse: "rna_enums.EventTypeItems"
    tool_maybe_tweak_value: "rna_enums.EventValueItems"
    context_menu_event: _Event
    pie_value: "rna_enums.EventValueItems"


class Button(Enum):
    """Mouse role the default keymap's parameters resolve from the active keyconfig's preferences."""

    SELECT = auto()
    ACTION = auto()
    TOOL = auto()
    CONTEXT = auto()

    def event(self, parameters: Parameters, /) -> tuple["rna_enums.EventTypeItems", Option["rna_enums.EventValueItems"]]:
        """Returns the event type the role presses, with the value the default keymap binds it at where it states one."""
        match self:
            case Button.SELECT:
                return parameters.select_mouse, Some(parameters.select_mouse_value)
            case Button.ACTION:
                return parameters.action_mouse, Nothing
            case Button.TOOL:
                return parameters.tool_mouse, Some(parameters.tool_maybe_tweak_value)
            case Button.CONTEXT:
                return parameters.context_menu_event["type"], Some(parameters.context_menu_event["value"])


class Chord(_Modifiers, total=False):
    """Binding event as `KeyMapItems.new` keywords, a mouse role resolving at registration, an unset value taking the role's, a pie press, or `PRESS`."""

    type: Required["rna_enums.EventTypeItems | Button"]
    value: "rna_enums.EventValueItems"


# --- [SERVICES] -------------------------------------------------------------------------


class Keymap:
    """Add-on keymap items held during registration, its bound `drawn` the layout draw of the user shortcut editors over them."""

    # --- [LIFECYCLE]

    def __init__(self, *, keymap: str, bindings: Bindings) -> None:
        """Stores bindings with no acquired items."""
        self.keymap, self.bindings = keymap, bindings
        self._items: tuple[tuple[bpy.types.KeyMap, bpy.types.KeyMapItem], ...] = ()

    # --- [REGISTRATION]

    @property
    def prerequisites(self) -> tuple[Declaration, ...]:
        """Declarations each bound placement registers."""
        return acquired_rows(placement for _, placement in self.bindings)

    def acquisitions(self, identity: Identity, /) -> Outcome[Acquisitions]:
        """Adds binding items in every space of the keymap name, holding each item's removal, a background run's absent add-on keyconfig adding none."""

        @contextmanager
        def held(locations: tuple[Location, ...], rows: tuple[Row, ...]) -> Generator[Outcome[None]]:
            def forgotten() -> None:
                self._items = ()

            def added(
                stack: ExitStack, keymap: bpy.types.KeyMap, operator: str, event: _Event, properties: dict[str, list[tuple[str, Setting]]]
            ) -> tuple[tuple[bpy.types.KeyMap, bpy.types.KeyMapItem], Outcome[None]]:
                item = keymap.keymap_items.new(operator, **event)
                stack.callback(keymap.keymap_items.remove, item)
                return (keymap, item), filled((operator, tuple(properties["properties"])), item.properties)

            with ExitStack() as stack:
                stack.callback(forgotten)
                addons = root_in(bpy.types.WindowManager, bpy.context).bind(lambda manager: Option.of_optional(manager.keyconfigs.addon)).to_list()
                keymaps = tuple(addon.keymaps.new(name, space_type=space, region_type=region) for addon in addons for name, space, region in locations)
                made = tuple(added(stack, keymap, *row) for keymap in keymaps for row in rows)
                self._items = tuple(pair for pair, _ in made)
                yield accumulated(fill for _, fill in made).map(lambda _: None)

        return paired(nesting(self.keymap), resolved(identity, self.bindings)).map(lambda pair: ((Tier.KEYMAP_ITEMS, held(*pair)),))

    # --- [USER_ITEMS]

    def counterparts(self, context: bpy.types.Context, /) -> tuple[tuple[bpy.types.KeyMap, bpy.types.KeyMapItem], ...]:
        """Updates native keyconfigs and returns the user keymap and item matching each owned item."""
        for manager in root_in(bpy.types.WindowManager, context).to_list():
            manager.keyconfigs.update(keep_properties=True)
            return tuple(
                (keymap, item)
                for user in Option.of_optional(manager.keyconfigs.user).to_list()
                for added, mine in self._items
                for keymap in Option.of_optional(user.keymaps.find_match(added)).to_list()
                for item in Option.of_optional(keymap.keymap_items.find_match(added, mine)).to_list()
            )
        return ()

    # --- [DRAWING]

    def drawn(self, layout: bpy.types.UILayout, _holder: Struct, context: bpy.types.Context) -> None:
        """Draws user shortcut editors, each under its user keymap as the `keymap` its restore and remove buttons read."""
        for user in root_in(bpy.types.WindowManager, context).bind(lambda manager: Option.of_optional(manager.keyconfigs.user)).to_list():
            for keymap, item in self.counterparts(context):
                column = layout.column()
                column.context_pointer_set("keymap", keymap)
                rna_keymap_ui.draw_kmi([], user, keymap, item, column, 0)


# --- [OPERATIONS] -----------------------------------------------------------------------


def nesting(name: str, /) -> Outcome[tuple[Location, ...]]:
    """Returns distinct spaces and regions of keymap name."""

    def walked(nodes: list[_Node]) -> Iterator[Location]:
        for label, space, region, children in nodes:
            if label == name:
                yield label, space, region
            yield from walked(children)

    found = tuple(dict.fromkeys(walked(keymap_hierarchy.generate())))  # type: ignore[func-returns-value]  # ty: ignore[invalid-argument-type]
    return Ok(found).filter_with(bool, lambda _: Fault.of(Reason.ABSENT, "bl_keymap_utils.keymap_hierarchy", member=name))


def resolved(identity: Identity, bindings: Bindings, /) -> Outcome[tuple[Row, ...]]:
    """Resolves bindings into keyconfig-data rows, mouse roles and unset values from the default keyconfig preset's `Params`, Ctrl chords doubled as Cmd on macOS."""

    def built(manager: bpy.types.WindowManager, inputs: bpy.types.PreferencesInput, preset: str) -> Parameters:
        defined = runpy.run_path(str(Path(preset).parent / "keymap_data" / "blender_default.py"))["Params"]
        stated: dict[str, object] = (
            Option
            .of_optional(manager.keyconfigs.active)
            .bind(lambda config: Option.of_optional(config.preferences))
            .map(lambda held: dict[str, object]((prop.identifier, getattr(held, prop.identifier)) for prop in rna_definition(held).properties))
            .default_value({})
        )
        emulated = inputs.use_mouse_emulate_3_button and inputs.mouse_emulate_3_button_modifier == "ALT"
        alternate = bool(stated.get("use_alt_tool" if stated.get("select_mouse") == "LEFT" else "use_alt_cursor"))
        match defined(**{name: stated[name] for name in inspect.signature(defined).parameters if name in stated}, use_alt_tool_or_cursor=not emulated and alternate):
            case Parameters() as params:
                return params
            case other:
                raise TypeError(other)

    def row(held: Parameters, chord: Chord, placement: Placement, placed: tuple[str, Pairs]) -> Row:
        match chord["type"]:
            case Button() as role:
                kind, valued = role.event(held)
            case str(named):
                kind, valued = named, Nothing
        pie = isinstance(placement.operator, PopupTarget) and placement.operator.kind is PopupKind.PIE
        event: _Event = {**chord, "type": kind, "value": chord.get("value", valued.default_value(held.pie_value if pie else "PRESS"))}
        operator, pairs = placed
        return operator, event, {"properties": list(pairs)}

    def doubled(rows: tuple[Row, ...]) -> tuple[Row, ...]:
        data: list[tuple[str, dict[str, object], dict[str, list[Row]]]] = [("", {}, {"items": list(rows)})]
        darwin: list[tuple[str, dict[str, object], dict[str, list[Row]]]] = platform_helpers.keyconfig_data_oskey_from_ctrl_for_macos(data) if sys.platform == "darwin" else data  # type: ignore[func-returns-value]  # ty: ignore[invalid-assignment]
        return tuple(item for _, _, block in darwin for item in block["items"])

    match bpy.context.window_manager, bpy.context.preferences:
        case bpy.types.WindowManager() as manager, bpy.types.Preferences(inputs=inputs):
            return (
                Option
                .of_optional(manager.keyconfigs.default)
                .to_result_with(partial(Fault.of, Reason.ABSENT, repr(manager.keyconfigs), member="default"))
                .bind(lambda default: Option.of_optional(bpy.utils.preset_find(default.name, "keyconfig")).to_result_with(partial(Fault.of, Reason.ABSENT, default.name, member="keyconfig preset")))
                .map(partial(built, manager, inputs))
                .map(lambda held: doubled(tuple(row(held, chord, placement, placement.resolved(identity)) for chord, placement in bindings)))
            )
        case _:
            return Error(Fault.of(Reason.ABSENT, "bpy.context", member="window_manager"))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Bindings", "Button", "Chord", "Keymap", "Location", "Parameters", "Row", "nesting", "resolved"]
