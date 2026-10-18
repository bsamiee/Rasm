"""Native shortcut batches, platform chords, user counterparts, and keymap editing."""

from collections.abc import Generator, Iterator, Mapping
from contextlib import contextmanager
from enum import auto, Enum
import sys
from typing import Literal, TYPE_CHECKING

from bl_keymap_utils import keymap_hierarchy, platform_helpers
import bpy
from expression import Error, Nothing, Ok, Option
import msgspec
import rna_keymap_ui

from blender.bpy.commands.calls import CalledPopup, MenuPopup, PanelPopup, resolved, ShelfPopup
from blender.bpy.commands.operators import Placed, Stock, Writer
from blender.bpy.data.rna.descriptors import Descriptor, rooted, Scalar
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisition, Band
from blender.bpy.faults import BlenderAbsentFault, BlenderResult
from blender.imaging.faults import accumulated, paired

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Location = tuple[str, rna_enums.SpaceTypeItems, rna_enums.RegionTypeItems]
type Node = tuple[str, rna_enums.SpaceTypeItems, rna_enums.RegionTypeItems, list[Node]]
type Target = Placed | CalledPopup
type ModifierState = Literal[-1, 0, 1]


class Button(Enum):
    """Mouse role assigned by the active key configuration's selection preference."""

    SELECT = auto()
    CONTEXT = auto()


# --- [MODELS] ---------------------------------------------------------------------------


class Toggle(msgspec.Struct, frozen=True, kw_only=True):
    """Toggle of a context-relative native boolean property."""

    path: str

    def invocation(self, identity: Identity, /) -> BlenderResult[tuple[str, Writer]]:
        """Resolves the boolean path and its native context-toggle operator."""
        return Descriptor.resolve(bpy.types.Context, self.path, bpy.types.BoolProperty).bind(lambda _: Stock(operator=bpy.ops.wm.context_toggle, values={"data_path": self.path}).invocation(identity))


class Assign(msgspec.Struct, frozen=True, kw_only=True):
    """Assignment through the context setter named by the native RNA property type."""

    path: str
    value: Scalar

    def invocation(self, identity: Identity, /) -> BlenderResult[tuple[str, Writer]]:
        """Resolves the property's native setter and its placement values."""
        return Descriptor.resolve(bpy.types.Context, self.path, bpy.types.Property).bind(
            lambda setting: Stock(operator=getattr(bpy.ops.wm, f"context_set_{setting.prop.type.lower()}"), values={"data_path": self.path, "value": self.value}).invocation(identity)
        )


class Chord(msgspec.Struct, frozen=True, kw_only=True):
    """Native event pattern, modifier states, and optional preference-selected button."""

    type: "rna_enums.EventTypeItems | Button"
    value: Option["rna_enums.EventValueItems"] = Nothing
    shift: ModifierState = 0
    ctrl: ModifierState = 0
    alt: ModifierState = 0
    oskey: ModifierState = 0
    hyper: ModifierState = 0
    key_modifier: "rna_enums.EventTypeItems" = "NONE"
    direction: "rna_enums.EventDirectionItems" = "ANY"
    repeat: bool = False

    def added(self, manager: bpy.types.WindowManager, keymap: bpy.types.KeyMap, operator: str, *, pie: bool = False) -> Iterator[bpy.types.KeyMapItem]:
        """Yields each added native item for immediate caller ownership."""

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
        event = msgspec.structs.asdict(self)
        event.update(type=kind, value=value)
        if not self.ctrl:
            del event["ctrl"]
        data = [(keymap.name, dict[str, object](), {"items": [(None, event, None)]})]
        return (
            keymap.keymap_items.new(operator, **item)
            for _, _, block in (platform_helpers.keyconfig_data_oskey_from_ctrl_for_macos(data) if sys.platform == "darwin" else data)  # type: ignore[func-returns-value, union-attr]  # ty: ignore[not-iterable]
            for _, item, _ in block["items"]
        )


# --- [SERVICES] -------------------------------------------------------------------------


class Keymap:
    """Shortcut batch owning native add-on items throughout registration."""

    # --- [LIFECYCLE]

    def __init__(self, *, keymap: str, bindings: Mapping[Chord, Target]) -> None:
        """Creates a batch with no acquired native shortcut items."""
        self.keymap, self.bindings = keymap, bindings
        self._items: tuple[tuple[bpy.types.KeyMap, bpy.types.KeyMapItem], ...] = ()

    # --- [REGISTRATION]

    def acquisitions(self, identity: Identity) -> BlenderResult[tuple[tuple[Band, Acquisition], ...]]:
        """Resolves native contexts and acquires targets after operator registration."""

        @contextmanager
        def added(given: tuple[tuple[Location, ...], bpy.types.WindowManager]) -> Generator[BlenderResult[None]]:
            locations, manager = given
            held: list[tuple[bpy.types.KeyMap, bpy.types.KeyMapItem]] = []
            try:
                writes: list[BlenderResult[None]] = []
                for name, space, region in locations:
                    native = manager.keyconfigs.addon.keymaps.new(name, space_type=space, region_type=region)  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]

                    def placed(pair: tuple[str, Writer], chord: Chord, target: Target) -> BlenderResult[None]:
                        operator, write = pair
                        written: list[BlenderResult[None]] = []
                        for item in chord.added(manager, native, operator, pie=isinstance(target, MenuPopup) and target.pie):
                            held.append((native, item))
                            written.append(Option.of_optional(item.properties).map(write).default_with(lambda: Error((BlenderAbsentFault(owner=operator, key="properties"),))))
                        return accumulated(written).map(lambda _: None)

                    writes.extend(invocation(target, identity).bind(lambda pair: placed(pair, chord, target)) for chord, target in self.bindings.items())
                self._items = tuple(held)
                yield accumulated(writes).map(lambda _: None)
            finally:
                self._items = ()
                for native, item in held:
                    native.keymap_items.remove(item)

        return paired(nesting(self.keymap), rooted(bpy.types.WindowManager, bpy.context)).map(lambda given: ((Band.KEYMAP_ITEMS, added(given)),))

    # --- [USER_ITEMS]

    def counterparts(self, context: bpy.types.Context) -> tuple[tuple[bpy.types.KeyMap, bpy.types.KeyMapItem], ...]:
        """Resolves each owned item to the current user keymap and counterpart item."""
        return tuple(
            (keymap, item)
            for manager in Option.of_optional(context.window_manager).to_list()
            for added, mine in self._items
            for keymap in Option.of_optional(manager.keyconfigs.user.keymaps.find_match(added)).to_list()  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
            for item in Option.of_optional(keymap.keymap_items.find_match(added, mine)).to_list()
        )

    def drawn(self, layout: bpy.types.UILayout, context: bpy.types.Context) -> None:
        """Draws the native user shortcut editors, including per-item restore controls."""
        for manager in Option.of_optional(context.window_manager).to_list():
            for keymap, item in self.counterparts(context):
                rna_keymap_ui.draw_kmi([], manager.keyconfigs.user, keymap, item, layout, 0)


# --- [OPERATIONS] -----------------------------------------------------------------------


def nesting(name: str) -> BlenderResult[tuple[Location, ...]]:
    """Returns every distinct native context of a hierarchy name."""

    def walked(nodes: list[Node]) -> Iterator[Location]:
        for label, space, region, children in nodes:
            if label == name:
                yield label, space, region
            yield from walked(children)

    found = tuple(dict.fromkeys(walked(keymap_hierarchy.generate())))  # type: ignore[func-returns-value, arg-type]  # ty: ignore[invalid-argument-type]
    return Ok(found) if found else Error((BlenderAbsentFault(owner="bl_keymap_utils.keymap_hierarchy", key=name),))


def invocation(target: Target, identity: Identity) -> BlenderResult[tuple[str, Writer]]:
    """Resolves a native placement or registered popup into an operator and writer."""
    match target:
        case MenuPopup() | PanelPopup() | ShelfPopup():
            return resolved(target, identity).bind(lambda pair: Stock(operator=pair[0], values=pair[1]).invocation(identity))
        case _:
            return target.invocation(identity)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Assign", "Button", "Chord", "Keymap", "Location", "Target", "Toggle", "invocation", "nesting"]
