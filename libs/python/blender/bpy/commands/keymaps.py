"""Add-on shortcuts in the host keyconfig-data form, mouse roles read from the default keymap's parameters, with user keymap editors."""

from collections.abc import Generator, Iterator
from contextlib import contextmanager, ExitStack
from enum import auto, Enum
from functools import partial
import inspect
from pathlib import Path
import sys
from types import ModuleType
from typing import Literal, Protocol, Required, TYPE_CHECKING, TypedDict

from bl_keymap_utils import keymap_hierarchy, platform_helpers
import bpy
from expression import Nothing, Ok, Option, Some
import rna_keymap_ui

from blender.bpy.commands.calls import Called, filled, Opened, Pairs, Placement, Setting
from blender.bpy.data.rna.paths import definition, Struct
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Band
from blender.bpy.extension.reports import window_manager
from blender.bpy.faults import BlenderReason
from blender.records.faults import accumulated, Fault, Outcome, paired

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Location = tuple[str, rna_enums.SpaceTypeItems, rna_enums.RegionTypeItems]
type Node = tuple[str, rna_enums.SpaceTypeItems, rna_enums.RegionTypeItems, list[Node]]
type ModifierState = Literal[-1, 0, 1]
type Bindings = tuple[tuple[Chord, Placement], ...]
type Row = tuple[str, Chord, dict[str, list[tuple[str, Setting]]]]
type KeyconfigData = list[tuple[str, dict[str, object], dict[str, list[Row]]]]


class ContextMenuEvent(TypedDict):
    """Event the default keymap's `Params` binds context menus to, in the host keyconfig-data form."""

    type: "rna_enums.EventTypeItems"
    value: "rna_enums.EventValueItems"


class Parameters(Protocol):
    """Members of the default keymap's `Params` the mouse roles and pie presses read."""

    select_mouse: "rna_enums.EventTypeItems"
    select_mouse_value: "rna_enums.EventValueItems"
    action_mouse: "rna_enums.EventTypeItems"
    tool_mouse: "rna_enums.EventTypeItems"
    tool_maybe_tweak_value: "rna_enums.EventValueItems"
    context_menu_event: ContextMenuEvent
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


class Chord(TypedDict, total=False):
    """Event of one binding in the host keyconfig-data form, its keys the `KeyMapItems.new` keywords, a mouse role resolving at registration and an unset value taking the role's value, a pie's press, or `PRESS`."""

    type: Required["rna_enums.EventTypeItems | Button"]
    value: "rna_enums.EventValueItems"
    any: bool
    shift: ModifierState
    ctrl: ModifierState
    alt: ModifierState
    oskey: ModifierState
    hyper: ModifierState
    key_modifier: "rna_enums.EventTypeItems"
    direction: "rna_enums.EventDirectionItems"
    repeat: bool
    head: bool


# --- [SERVICES] -------------------------------------------------------------------------


class Keymap:
    """Add-on keymap items held during registration, its bound `drawn` the layout draw of the user shortcut editors over them."""

    # --- [LIFECYCLE]

    def __init__(self, *, keymap: str, bindings: Bindings) -> None:
        """Stores bindings with no acquired items."""
        self.keymap, self.bindings = keymap, bindings
        self._items: tuple[tuple[bpy.types.KeyMap, bpy.types.KeyMapItem], ...] = ()

    # --- [REGISTRATION]

    def acquisitions(self, identity: Identity, /) -> Outcome[Acquisitions]:
        """Adds each binding's items to every space of the keymap name after operator registration, each item's removal held as it is made."""

        @contextmanager
        def held(locations: tuple[Location, ...], rows: tuple[Row, ...]) -> Generator[Outcome[None]]:
            addon: bpy.types.KeyConfig = window_manager().keyconfigs.addon  # type: ignore[assignment]  # ty: ignore[invalid-assignment]

            def forgotten() -> None:
                self._items = ()

            def added(
                stack: ExitStack, keymap: bpy.types.KeyMap, operator: str, event: Chord, properties: dict[str, list[tuple[str, Setting]]]
            ) -> tuple[tuple[bpy.types.KeyMap, bpy.types.KeyMapItem], Outcome[None]]:
                item = keymap.keymap_items.new(operator, **event)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                stack.callback(keymap.keymap_items.remove, item)
                return (keymap, item), filled((operator, tuple(properties["properties"])), item.properties)

            with ExitStack() as stack:
                stack.callback(forgotten)
                keymaps = tuple(addon.keymaps.new(name, space_type=space, region_type=region) for name, space, region in locations)
                made = tuple(added(stack, keymap, *row) for keymap in keymaps for row in rows)
                self._items = tuple(pair for pair, _ in made)
                yield accumulated(fill for _, fill in made).map(lambda _: None)

        return paired(nesting(self.keymap), resolved(identity, self.bindings)).map(lambda pair: ((Band.KEYMAP_ITEMS, held(*pair)),))

    # --- [USER_ITEMS]

    def counterparts(self, context: bpy.types.Context, /) -> tuple[tuple[bpy.types.KeyMap, bpy.types.KeyMapItem], ...]:
        """Returns user keymap and item matching each owned item."""
        manager: bpy.types.WindowManager = context.window_manager  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        user: bpy.types.KeyConfig = manager.keyconfigs.user  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        return tuple(
            (keymap, item)
            for added, mine in self._items
            for keymap in Option.of_optional(user.keymaps.find_match(added)).to_list()
            for item in Option.of_optional(keymap.keymap_items.find_match(added, mine)).to_list()
        )

    # --- [DRAWING]

    def drawn(self, layout: bpy.types.UILayout, _holder: Struct, context: bpy.types.Context) -> None:
        """Draws user shortcut editors, each under its user keymap as the `keymap` its restore and remove buttons read."""
        manager: bpy.types.WindowManager = context.window_manager  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        for keymap, item in self.counterparts(context):
            column = layout.column()
            column.context_pointer_set("keymap", keymap)
            rna_keymap_ui.draw_kmi([], manager.keyconfigs.user, keymap, item, column, 0)


# --- [OPERATIONS] -----------------------------------------------------------------------


def nesting(name: str, /) -> Outcome[tuple[Location, ...]]:
    """Returns distinct spaces and regions of keymap name."""

    def walked(nodes: list[Node]) -> Iterator[Location]:
        for label, space, region, children in nodes:
            if label == name:
                yield label, space, region
            yield from walked(children)

    found = tuple(dict.fromkeys(walked(keymap_hierarchy.generate())))  # type: ignore[func-returns-value, arg-type]  # ty: ignore[invalid-argument-type]
    return Ok(found).filter_with(bool, lambda _: Fault.of(BlenderReason.ABSENT, "bl_keymap_utils.keymap_hierarchy", member=name))


def parameters(manager: bpy.types.WindowManager, /) -> Outcome[Parameters]:
    """Returns the default keymap's `Params` over the active keyconfig's preferences that name its parameters, the rest at the default keymap's own values, absent fault for a default keyconfig with no preset.

    The default keyconfig's preset loads its keymap data from `keymap_data/blender_default.py` beside it, the module read here.
    """
    default: bpy.types.KeyConfig = manager.keyconfigs.default  # type: ignore[assignment]  # ty: ignore[invalid-assignment]

    def built(preset: str) -> Parameters:
        module: ModuleType = bpy.utils.execfile(str(Path(preset).parent / "keymap_data" / "blender_default.py"))  # type: ignore[func-returns-value, assignment]  # ty: ignore[invalid-assignment]
        accepted = inspect.signature(module.Params).parameters
        keywords = (
            Option
            .of_optional(manager.keyconfigs.active)
            .bind(lambda config: Option.of_optional(config.preferences))
            .map(lambda held: {name: getattr(held, name) for name in accepted if name in definition(held).properties})
            .default_value({})
        )
        params: Parameters = module.Params(**keywords)
        return params

    return Option.of_optional(bpy.utils.preset_find(default.name, "keyconfig")).to_result_with(partial(Fault.of, BlenderReason.ABSENT, default.name, member="keyconfig preset")).map(built)


def resolved(identity: Identity, bindings: Bindings, /) -> Outcome[tuple[Row, ...]]:
    """Resolves bindings into keyconfig-data rows, each mouse role and unset value read from the default keymap's parameters, Ctrl chords doubled as Cmd on macOS."""

    def row(held: Parameters, chord: Chord, placement: Placement, placed: tuple[str, Pairs]) -> Row:
        match chord["type"]:
            case Button() as role:
                kind, valued = role.event(held)
            case str(named):
                kind, valued = named, Nothing
        pie = isinstance(placement.operator, Opened) and placement.operator.kind is Called.PIE
        event: Chord = {**chord, "type": kind, "value": chord.get("value", valued.default_value(held.pie_value if pie else "PRESS"))}
        operator, pairs = placed
        return operator, event, {"properties": list(pairs)}

    def doubled(rows: tuple[Row, ...]) -> tuple[Row, ...]:
        data: KeyconfigData = [("", {}, {"items": list(rows)})]
        darwin: KeyconfigData = platform_helpers.keyconfig_data_oskey_from_ctrl_for_macos(data) if sys.platform == "darwin" else data  # type: ignore[func-returns-value, assignment]  # ty: ignore[invalid-assignment]
        return tuple(item for _, _, block in darwin for item in block["items"])

    return parameters(window_manager()).bind(lambda held: accumulated(placement.resolved(identity).map(partial(row, held, chord, placement)) for chord, placement in bindings).map(doubled))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Bindings", "Button", "Chord", "ContextMenuEvent", "Keymap", "Location", "Parameters", "Row", "nesting", "parameters", "resolved"]
