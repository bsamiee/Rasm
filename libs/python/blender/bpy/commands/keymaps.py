"""Add-on shortcuts with user keymap editors."""

from collections.abc import Callable, Generator, Iterator, Mapping
from contextlib import contextmanager
from enum import auto, Enum
import sys
from typing import Final, Literal, NotRequired, TYPE_CHECKING, TypedDict

from bl_keymap_utils import keymap_hierarchy, platform_helpers
import bpy
from expression import Nothing, Ok, Option
import msgspec
import rna_keymap_ui

from blender.bpy.commands.calls import MenuPopup, Placed, Stock, Writer
from blender.bpy.data.rna.descriptors import Descriptor, rna, RnaPath, rooted, Scalar
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisition, Band
from blender.bpy.faults import BlenderReason
from blender.records.faults import accumulated, Fault, Outcome, paired

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Location = tuple[str, rna_enums.SpaceTypeItems, rna_enums.RegionTypeItems]
type Node = tuple[str, rna_enums.SpaceTypeItems, rna_enums.RegionTypeItems, list[Node]]
type ModifierState = Literal[-1, 0, 1]
type KeyconfigData = list[tuple[str, dict[str, object], dict[str, list[tuple[None, Event, None]]]]]


class Button(Enum):
    """Mouse role resolved through `select_mouse` preference."""

    SELECT = auto()
    CONTEXT = auto()


class Event(TypedDict):
    """`KeyMapItems.new` keywords of one chord, `ctrl` present on the chords macOS doubles as Cmd."""

    type: "rna_enums.EventTypeItems"
    value: "rna_enums.EventValueItems"
    any: bool
    shift: ModifierState
    ctrl: NotRequired[ModifierState]
    alt: ModifierState
    oskey: ModifierState
    hyper: ModifierState
    key_modifier: "rna_enums.EventTypeItems"
    direction: "rna_enums.EventDirectionItems"
    repeat: bool
    head: bool


# --- [CONSTANTS] ------------------------------------------------------------------------

_OSKEY_FROM_CTRL: Final[Callable[[KeyconfigData], KeyconfigData]] = platform_helpers.keyconfig_data_oskey_from_ctrl_for_macos  # type: ignore[assignment]  # ty: ignore[invalid-assignment]

# --- [MODELS] ---------------------------------------------------------------------------


class Toggle(msgspec.Struct, frozen=True, kw_only=True):
    """Toggle of context-relative boolean property."""

    path: RnaPath

    def invocation(self, identity: Identity, /) -> Outcome[tuple[str, Writer]]:
        """Resolves boolean path and its `wm.context_toggle` operator."""
        return Descriptor.resolve(bpy.types.Context, self.path, bpy.types.BoolProperty).bind(
            lambda _: Stock(operator=bpy.ops.wm.context_toggle, values={"data_path": rna(self.path)}).invocation(identity)
        )


class Assign(msgspec.Struct, frozen=True, kw_only=True):
    """Assignment through `wm.context_set_value` of a value the context path accepts."""

    path: RnaPath
    value: Scalar

    def invocation(self, identity: Identity, /) -> Outcome[tuple[str, Writer]]:
        """Resolves the context path and the setter values."""
        return Descriptor.resolve(bpy.types.Context, self.path, bpy.types.Property).bind(
            lambda _: Stock(operator=bpy.ops.wm.context_set_value, values={"data_path": rna(self.path), "value": repr(self.value)}).invocation(identity)
        )


class Chord(msgspec.Struct, frozen=True, kw_only=True):
    """`KeyMapItems.new` event pattern with modifier states."""

    type: "rna_enums.EventTypeItems | Button"
    value: Option["rna_enums.EventValueItems"] = Nothing
    any: bool = False
    shift: ModifierState = 0
    ctrl: ModifierState = 0
    alt: ModifierState = 0
    oskey: ModifierState = 0
    hyper: ModifierState = 0
    key_modifier: "rna_enums.EventTypeItems" = "NONE"
    direction: "rna_enums.EventDirectionItems" = "ANY"
    repeat: bool = False
    head: bool = False

    def added(self, manager: bpy.types.WindowManager, keymap: bpy.types.KeyMap, operator: str, *, pie: bool = False) -> Iterator[bpy.types.KeyMapItem]:
        """Adds keymap items lazily, Ctrl chords doubled as Cmd on macOS, caller holding each for removal."""

        def preference(name: str) -> Option[object]:
            return (
                Option
                .of_optional(manager.keyconfigs.active)
                .bind(lambda config: Option.of_optional(config.preferences))
                .bind(lambda preferences: Option.of_optional(preferences.bl_rna.properties.get(name)).map(lambda prop: getattr(preferences, prop.identifier)))
            )

        left = preference("select_mouse").map(lambda selected: selected == "LEFT").default_value(True)
        match self.type:
            case Button.SELECT:
                kind: rna_enums.EventTypeItems = "LEFTMOUSE" if left else "RIGHTMOUSE"
            case Button.CONTEXT:
                kind = "RIGHTMOUSE" if left else "LEFTMOUSE"
            case str():
                kind = self.type
        value: rna_enums.EventValueItems = self.value.default_with(lambda: "CLICK_DRAG" if pie and preference("use_pie_click_drag").map(bool).default_value(False) else "PRESS")
        event = Event(
            type=kind,
            value=value,
            any=self.any,
            shift=self.shift,
            alt=self.alt,
            oskey=self.oskey,
            hyper=self.hyper,
            key_modifier=self.key_modifier,
            direction=self.direction,
            repeat=self.repeat,
            head=self.head,
        )
        if self.ctrl:
            event["ctrl"] = self.ctrl
        data: KeyconfigData = [(keymap.name, {}, {"items": [(None, event, None)]})]
        return (keymap.keymap_items.new(operator, **item) for _, _, block in (_OSKEY_FROM_CTRL(data) if sys.platform == "darwin" else data) for _, item, _ in block["items"])


# --- [SERVICES] -------------------------------------------------------------------------


class Keymap:
    """Add-on keymap items held during registration."""

    # --- [LIFECYCLE]

    def __init__(self, *, keymap: str, bindings: Mapping[Chord, Placed]) -> None:
        """Stores bindings with no acquired items."""
        self.keymap, self.bindings = keymap, bindings
        self._items: tuple[tuple[bpy.types.KeyMap, bpy.types.KeyMapItem], ...] = ()

    # --- [REGISTRATION]

    def acquisitions(self, identity: Identity) -> Outcome[tuple[tuple[Band, Acquisition], ...]]:
        """Adds each binding's items to every space of the keymap name after operator registration."""

        @contextmanager
        def added(given: tuple[tuple[Location, ...], bpy.types.WindowManager]) -> Generator[Outcome[None]]:
            locations, manager = given
            addon: bpy.types.KeyConfig = manager.keyconfigs.addon  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            held: list[tuple[bpy.types.KeyMap, bpy.types.KeyMapItem]] = []

            def placed(native: bpy.types.KeyMap, chord: Chord, target: Placed, invocation: tuple[str, Writer]) -> Outcome[None]:
                operator, write = invocation
                items = tuple(chord.added(manager, native, operator, pie=isinstance(target, MenuPopup) and target.pie))
                held.extend((native, item) for item in items)
                return accumulated(Option.of_optional(item.properties).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, operator, member="properties")).bind(write) for item in items).map(
                    lambda _: None
                )

            try:
                natives = tuple(addon.keymaps.new(name, space_type=space, region_type=region) for name, space, region in locations)
                answer = accumulated(target.invocation(identity).map(lambda invocation: (chord, target, invocation)) for chord, target in self.bindings.items()).bind(
                    lambda bound: accumulated(placed(native, *binding) for native in natives for binding in bound).map(lambda _: None)
                )
                self._items = tuple(held)
                yield answer
            finally:
                self._items = ()
                for native, item in held:
                    native.keymap_items.remove(item)

        return paired(nesting(self.keymap), rooted(bpy.types.WindowManager, bpy.context)).map(lambda given: ((Band.KEYMAP_ITEMS, added(given)),))

    # --- [USER_ITEMS]

    def counterparts(self, context: bpy.types.Context) -> tuple[tuple[bpy.types.KeyMap, bpy.types.KeyMapItem], ...]:
        """Returns user keymap and item matching each owned item."""
        manager: bpy.types.WindowManager = context.window_manager  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        user: bpy.types.KeyConfig = manager.keyconfigs.user  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        return tuple(
            (keymap, item)
            for added, mine in self._items
            for keymap in Option.of_optional(user.keymaps.find_match(added)).to_list()
            for item in Option.of_optional(keymap.keymap_items.find_match(added, mine)).to_list()
        )

    def drawn(self, layout: bpy.types.UILayout, context: bpy.types.Context) -> None:
        """Draws user shortcut editors with per-item restore controls."""
        manager: bpy.types.WindowManager = context.window_manager  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        user: bpy.types.KeyConfig = manager.keyconfigs.user  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        for keymap, item in self.counterparts(context):
            rna_keymap_ui.draw_kmi([], user, keymap, item, layout, 0)


# --- [OPERATIONS] -----------------------------------------------------------------------


def nesting(name: str) -> Outcome[tuple[Location, ...]]:
    """Returns distinct spaces and regions of keymap name."""

    def walked(nodes: list[Node]) -> Iterator[Location]:
        for label, space, region, children in nodes:
            if label == name:
                yield label, space, region
            yield from walked(children)

    found = tuple(dict.fromkeys(walked(keymap_hierarchy.generate())))  # type: ignore[func-returns-value, arg-type]  # ty: ignore[invalid-argument-type]
    return Ok(found).filter_with(bool, lambda _: Fault.of(BlenderReason.ABSENT, "bl_keymap_utils.keymap_hierarchy", member=name))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Assign", "Button", "Chord", "Keymap", "Location", "Toggle", "nesting"]
