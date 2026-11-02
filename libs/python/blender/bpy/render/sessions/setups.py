"""Render setups: what a render set captures from a member scene, setup recipes applied over unlocked members, the category and compositor entry captures, and the locks setups keep, stored per scene for any key kind."""

from collections.abc import Callable, Mapping
from functools import cached_property, partial
from types import GenericAlias, MappingProxyType
from typing import Final, Protocol, Self

import bpy
from expression import Nothing, Ok, Option, Some
import msgspec
from typing_extensions import TypeForm

from blender.bpy.commands.calls import Placement
from blender.bpy.commands.operators import Answer, Call, Done, Operator, tallied, Unchanged
from blender.bpy.data.rna.paths import parsed, rna, RnaPath, rooted, Struct
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.extension.reports import alerted
from blender.bpy.render.compositor.colors import COLOR_INPUT
from blender.bpy.render.compositor.effects import Entry
from blender.bpy.render.compositor.entries import Enabled
from blender.bpy.render.compositor.inputs import keyed
from blender.bpy.render.compositor.offered import OfferedEffects
from blender.bpy.render.compositor.recipes import applied, projected, recorded, Valued
from blender.bpy.render.sessions.members import KEY, Members
from blender.bpy.render.view_layers.passes import layer_of
from blender.bpy.settings.attachments import Stored
from blender.bpy.settings.presets import Preset
from blender.bpy.ui.layout.rows import ConditionalRow, Layout, operator_button, RowContent, RowDraw
from blender.bpy.ui.layout.templates import PresetGallery
from blender.bpy.ui.layout.values import hosted, Listing
from blender.records.faults import accumulated, Fault, Outcome, paired, Reason
from blender.records.fields import Case, Name
from blender.records.stores import content_hash

# --- [TYPES] ----------------------------------------------------------------------------

type Toggle[K] = Callable[[K, bpy.types.UILayout, Struct, bpy.types.Context], None]


class Capture[C: msgspec.Struct](Protocol):
    """What a render set captures from a member scene, writes back as a setup, and draws above its member lists."""

    @property
    def form(self) -> TypeForm[C]:
        """Record a capture decodes as, a setup preset's form."""
        ...

    def captured(self, members: Members, scene: bpy.types.Scene, /) -> Outcome[C]:
        """Captures a member scene."""
        ...

    def projected(self, members: Members, scene: bpy.types.Scene, recipe: C, current: C, /) -> Outcome[tuple[C, C]]:
        """Returns the sparse writes and the projected capture of a recipe over a scene's current capture, its locks kept."""
        ...

    def applied(self, members: Members, scene: bpy.types.Scene, sparse: C, /) -> Outcome[None]:
        """Writes sparse values to a member scene."""
        ...

    def rows(self, members: Members, scope: "SetupScope[C]", /) -> tuple[RowContent, ...]:
        """Rows drawn above the member lists."""
        ...


# --- [CONSTANTS] ------------------------------------------------------------------------

WHOLE: Final[Key] = ""
_SCENE_HOLDER: Final = ""

# --- [MODELS] ---------------------------------------------------------------------------


class CategorySettings(msgspec.Struct, frozen=True, kw_only=True):
    """Category settings of one scene and of each of its view layers."""

    scene: Snapshot
    layers: Mapping[str, Snapshot]


class EffectSetup[R: Case](msgspec.Struct, frozen=True, kw_only=True):
    """Category settings of a scene and its view layers, and the compositor entries it holds by name in stack order."""

    settings: CategorySettings
    effects: Mapping[Name, Valued[Entry[R]]]


class LockRequest(bpy.types.PropertyGroup):
    """Lock key a lock button toggles on the holder it drew, as the text the host button shows."""

    key: bpy.props.StringProperty(name="Key", description="Lock key the button toggles")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Locks[K](msgspec.Struct, frozen=True, kw_only=True, omit_defaults=True):
    """Keys each holder of one scene keeps from setups, a holder named by its name in the scene, the scene itself by the empty name."""

    held: Mapping[str, frozenset[K]] = MappingProxyType({})

    def of(self, holder: str) -> frozenset[K]:
        """Locked keys of a holder."""
        return self.held.get(holder, frozenset())

    def toggled(self, holder: str, key: K) -> Self:
        """Locks with the key's lock on holder toggled."""
        return msgspec.structs.replace(self, held={**self.held, holder: self.of(holder) ^ {key}})

    def whole(self, key: K) -> frozenset[str]:
        """Holders locking `key`."""
        return frozenset(holder for holder, keys in self.held.items() if key in keys)


class Locking[K](msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Per-scene locks of one key kind with their toggle command and button."""

    key: Key
    form: TypeForm[Locks[K]]
    holder: Callable[[Struct], Option[tuple[bpy.types.Scene, str]]]
    text: Callable[[K], str]
    parsed: Callable[[str], Outcome[K]]

    @property
    def owner(self) -> Key:
        """Context pointer key a toggle button publishes its holder under."""
        return f"{self.key}_owner"

    @cached_property
    def operator(self) -> Operator[LockRequest]:
        """Toggle command keyed after the locks."""
        return Operator.of(LockRequest, named=Some(f"{self.key}_toggle"))(self.lock_toggle)

    def stored(self, identity: Identity) -> Outcome[Stored[Locks[K]]]:
        """Binds the per-scene lock store."""
        return Stored.keyed(identity, self.key, msgspec.msgpack.Decoder(self.form))

    def read(self, identity: Identity, scene: bpy.types.Scene) -> Outcome[Locks[K]]:
        """Reads a scene's locks, none before the first lock."""
        return self.stored(identity).bind(partial(_locked, scene=scene))

    def lock_toggle(self, call: Call[LockRequest]) -> Outcome[Done]:
        """Toggles a key's lock on the holder its button drew, in the holder scene's lock store."""

        def toggled(owner: Struct, store: Stored[Locks[K]], key: K) -> Outcome[Done]:
            return (
                self
                .holder(owner)
                .to_result_with(lambda: Fault.of(Reason.ABSENT, repr(owner), member=self.owner))
                .bind(lambda found: _locked(store, scene=found[0]).bind(lambda locks: store.write(found[0], locks.toggled(found[1], key))))
                .map(lambda _: Done())
            )

        return paired(call.identity.identifier(self.owner).bind(lambda name: hosted(name, call.context)), self.stored(call.identity), self.parsed(call.properties.key)).bind(
            lambda parts: toggled(*parts)
        )

    def toggles(self, identity: Identity) -> Outcome[Toggle[K]]:
        """Binds the lock store once into the draw of any key's locked or unlocked toggle on the drawn holder."""

        def toggle(name: str, store: Stored[Locks[K]], key: K, layout: bpy.types.UILayout, holder: Struct, _context: bpy.types.Context) -> None:
            request = Placement(self.operator, {"key": self.text(key)})

            def shown(scene: bpy.types.Scene, held: str) -> None:
                alerted(
                    layout,
                    _locked(store, scene=scene).bind(
                        lambda locks: operator_button(layout, identity, request, {"text": "", "emboss": False, "icon": "LOCKED" if key in locks.of(held) else "UNLOCKED"})
                    ),
                )

            layout.context_pointer_set(name, holder)
            for found in self.holder(holder).to_list():
                shown(*found)

        return paired(identity.identifier(self.owner), self.stored(identity)).map(lambda parts: partial(toggle, *parts))

    def drawn(self, key: K, identity: Identity) -> Outcome[RowDraw]:
        """Draws the toggle of a key's lock on the drawn holder."""
        return self.toggles(identity).map(lambda toggle: partial(toggle, key))

    def row(self, key: K) -> RowContent:
        """Lock toggle of a key on the drawn holder with its command."""
        return Layout(rows=lambda identity: self.drawn(key, identity).map(lambda draw: (draw,)), prerequisites=(self.operator,))


class EntryLocks(ConditionalRow, frozen=True, kw_only=True):
    """Closed panel over a drawn compositor entry listing its enable flags and visible inputs, each beside the toggle of its entry lock."""

    @staticmethod
    def acquisitions(_identity: Identity, /) -> Outcome[Acquisitions]:
        """Acquires the entry lock command."""
        return Ok((ENTRY_LOCKS.operator,))

    def drawn(self, identity: Identity, /) -> Outcome[RowDraw]:
        """Binds the entry lock store once into the panel draw."""
        return ENTRY_LOCKS.toggles(identity).map(lambda toggle: self.conditioned(partial(self.shown, identity, toggle)))

    @staticmethod
    def shown(identity: Identity, toggle: Toggle[Key], layout: bpy.types.UILayout, holder: object, context: bpy.types.Context) -> None:
        """Draws one line per lockable key of the entry, inputs named under their panel."""

        def named(item: bpy.types.NodeTreeInterfaceSocket) -> str:
            parent: bpy.types.NodeTreeInterfacePanel = item.parent  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            return f"{parent.name} > {item.name}" if parent.name else item.name

        def listed(entry: bpy.types.SceneCompositorEffect, group: bpy.types.NodeTree, sockets: Mapping[Key, str]) -> None:
            interface: bpy.types.NodeTreeInterface = group.interface  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            live = {item.identifier: item for item in interface.items_tree if isinstance(item, bpy.types.NodeTreeInterfaceSocket) and item.in_out == "INPUT"}
            lockable = (
                *((enabled.descriptor.prop.name, enabled.value) for enabled in Enabled),
                *((named(item), key) for key, identifier in sockets.items() if key != COLOR_INPUT.key for item in Option.of_optional(live.get(identifier)).to_list() if not item.hide_in_modifier),
            )
            header, body = layout.panel(f"{entry.name}_{ENTRY_LOCKS.key}", default_closed=True)
            header.label(text=bpy.path.display_name(ENTRY_LOCKS.key))
            for inner, (name, key) in ((inner, pair) for inner in Option.of_optional(body).to_list() for pair in lockable):
                line = inner.row(align=True)
                line.label(text=name)
                toggle(key, line, entry, context)

        if isinstance(entry := holder, bpy.types.SceneCompositorEffect):
            alerted(layout, keyed(identity, entry).map(lambda pair: listed(entry, *pair)))


class Categories(msgspec.Struct, frozen=True, kw_only=True):
    """Category capture: the categories' settings of a scene and its unlocked view layers, setups keeping each locked setting."""

    @property
    def form(self) -> TypeForm[CategorySettings]:
        """Category settings record of a setup preset."""
        return CategorySettings

    @staticmethod
    def captured(members: Members, scene: bpy.types.Scene, /) -> Outcome[CategorySettings]:
        """Captures scene and view layer category settings."""
        layers = members.selected(bpy.types.ViewLayer)
        return paired(
            Snapshot.capture(scene, Some(members.selected(bpy.types.Scene))),
            accumulated(Snapshot.capture(layer, Some(layers)).map(lambda snapshot: (layer.name, snapshot)) for layer in scene.view_layers) if layers else Ok(()),
        ).map(lambda values: CategorySettings(scene=values[0], layers=dict(values[1])))

    @staticmethod
    def projected(members: Members, scene: bpy.types.Scene, recipe: CategorySettings, current: CategorySettings, /) -> Outcome[tuple[CategorySettings, CategorySettings]]:
        """Projects recipe settings over current ones on the scene and its unlocked view layers present in both, locked settings kept."""

        def held(locks: Locks[RnaPath]) -> Outcome[tuple[CategorySettings, CategorySettings]]:
            layers = tuple(layer for layer in members.unlocked(scene.view_layers) if layer.name in recipe.layers and layer.name in current.layers)
            return paired(
                recipe.scene.projected(scene, current.scene, locks.of(_SCENE_HOLDER)),
                accumulated(recipe.layers[layer.name].projected(layer, current.layers[layer.name], locks.of(layer.name)).map(lambda pair: (layer.name, pair)) for layer in layers),
            ).map(
                lambda values: (
                    CategorySettings(scene=values[0][0], layers={name: pair[0] for name, pair in values[1]}),
                    CategorySettings(scene=values[0][1], layers={**current.layers, **{name: pair[1] for name, pair in values[1]}}),
                )
            )

        return SETTING_LOCKS.read(members.identity, scene).bind(held)

    @staticmethod
    def applied(_members: Members, scene: bpy.types.Scene, sparse: CategorySettings, /) -> Outcome[None]:
        """Writes sparse scene and view layer settings, absent fault for a layer the scene no longer holds."""
        return (
            sparse.scene
            .apply(scene)
            .outcome.bind(lambda _: accumulated(layer_of(scene, name).bind(lambda layer: snapshot.apply(layer).outcome) for name, snapshot in sparse.layers.items()))
            .map(lambda _: None)
        )

    @staticmethod
    def rows(_members: Members, _scope: "SetupScope[CategorySettings]", /) -> tuple[RowContent, ...]:
        """Draws nothing above the member lists."""
        return ()


class EffectSetups[R: Case](msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Category capture with the compositor entries the `effects` own, entry locks kept by every setup, and setup presets drawn as a gallery of `tile` icons.

    `tile` returns the preview icon of a slot showing the preview-enabled entries a setup projects over the drawn scene, in stack order.
    """

    effects: OfferedEffects[R]
    tile: Option[Callable[[Identity, Key, tuple[Valued[Entry[R]], ...]], Option[int]]] = Nothing

    @cached_property
    def form(self) -> TypeForm[EffectSetup[R]]:
        """Setup record over the offered effect records."""
        return GenericAlias(EffectSetup, (self.effects.form,))  # type: ignore[return-value]  # ty: ignore[invalid-type-form]

    def captured(self, members: Members, scene: bpy.types.Scene, /) -> Outcome[EffectSetup[R]]:
        """Captures category settings and owned entries."""
        return paired(CATEGORIES.captured(members, scene), recorded(self.effects, members.identity, scene)).map(lambda pair: EffectSetup(settings=pair[0], effects=pair[1]))

    @staticmethod
    def projected(members: Members, scene: bpy.types.Scene, recipe: EffectSetup[R], current: EffectSetup[R], /) -> Outcome[tuple[EffectSetup[R], EffectSetup[R]]]:
        """Projects recipe settings and entries over current ones, setting and entry locks kept."""
        return paired(CATEGORIES.projected(members, scene, recipe.settings, current.settings), ENTRY_LOCKS.read(members.identity, scene)).map(
            lambda parts: (
                EffectSetup(settings=parts[0][0], effects=recipe.effects),
                EffectSetup(settings=parts[0][1], effects=projected(current.effects, recipe.effects, locked=parts[1].whole(WHOLE), fields=parts[1].held)),
            )
        )

    def applied(self, members: Members, scene: bpy.types.Scene, sparse: EffectSetup[R], /) -> Outcome[None]:
        """Writes sparse settings and entries, locked entries and fields kept."""
        return paired(
            CATEGORIES.applied(members, scene, sparse.settings),
            ENTRY_LOCKS.read(members.identity, scene).bind(lambda locks: applied(self.effects, members.identity, scene, sparse.effects, locked=locks.whole(WHOLE), fields=locks.held)),
        ).map(lambda _: None)

    def rows(self, _members: Members, scope: "SetupScope[EffectSetup[R]]", /) -> tuple[RowContent, ...]:
        """Draws setup presets as tiles above the member lists while a tile source is given."""
        return tuple(PresetGallery(listing=scope.listing, tile=partial(self.tiled, tile)) for tile in self.tile.to_list())

    def tiled(self, tile: Callable[[Identity, Key, tuple[Valued[Entry[R]], ...]], Option[int]], identity: Identity, context: bpy.types.Context | None, setup: EffectSetup[R]) -> Option[int]:
        """Returns the icon of the preview-enabled entries a setup projects over the context scene's entries, locks kept."""

        def shown(scene: bpy.types.Scene, current: Mapping[Name, Valued[Entry[R]]], locks: Locks[Key]) -> Option[int]:
            entries = tuple(held for held in projected(current, setup.effects, locked=locks.whole(WHOLE), fields=locks.held).values() if Enabled.PREVIEW in held.enables)
            return tile(identity, f"{KEY}_{scene.session_uid}_{content_hash(EffectSetups, setup).hex()}", entries)

        return (
            Option
            .of_optional(context)
            .bind(lambda held: rooted(bpy.types.Scene, held).to_option())
            .bind(lambda scene: paired(recorded(self.effects, identity, scene), ENTRY_LOCKS.read(identity, scene)).to_option().bind(lambda parts: shown(scene, *parts)))
        )


class SetupScope[C: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """Setup recipes of a render set's capture over its unlocked member scenes."""

    members: Members
    capture: Capture[C]

    @property
    def listing(self) -> Listing[C]:
        """Setup preset actions with matches against the active scene."""
        return Listing(preset=Preset(key=KEY, form=self.capture.form), current=self.current, matches=self.matched, applied=self.applied)

    def current(self, context: bpy.types.Context) -> Outcome[C]:
        """Captures active scene, substituting no source."""
        return rooted(bpy.types.Scene, context).bind(lambda scene: self.capture.captured(self.members, scene))

    def matched(self, context: bpy.types.Context, recipes: tuple[C, ...]) -> Outcome[tuple[bool, ...]]:
        """Answers per recipe whether projecting it leaves the active scene unchanged."""
        return rooted(bpy.types.Scene, context).bind(
            lambda scene: self.capture.captured(self.members, scene).bind(
                lambda held: accumulated(self.capture.projected(self.members, scene, recipe, held).map(lambda pair: pair[1] == held) for recipe in recipes)
            )
        )

    def applied(self, context: bpy.types.Context, recipe: C) -> Outcome[Answer]:
        """Applies recipe to each unlocked member scene it changes."""

        def planned(scene: bpy.types.Scene) -> Outcome[Option[tuple[bpy.types.Scene, C]]]:
            return self.capture.captured(self.members, scene).bind(
                lambda held: self.capture.projected(self.members, scene, recipe, held).map(lambda pair: Some((scene, pair[0])) if pair[1] != held else Nothing)
            )

        def wrote(plans: tuple[Option[tuple[bpy.types.Scene, C]], ...]) -> Outcome[Answer]:
            match tuple(plan for held in plans for plan in held.to_list()), plans:
                case (), ():
                    return Ok(Unchanged(reason="Every render set member is locked"))
                case (), _:
                    return Ok(Unchanged(reason="Unlocked render set members match setup"))
                case changing, _:
                    return tallied(tuple(self.capture.applied(self.members, scene, sparse) for scene, sparse in changing))

        return self.members.member_scenes(context).map(self.members.unlocked).bind(lambda scenes: accumulated(map(planned, scenes))).bind(wrote)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _locked[K](store: Stored[Locks[K]], *, scene: bpy.types.Scene) -> Outcome[Locks[K]]:
    """Reads a scene's locks from a store, none before the first lock."""
    return store.read(scene).map(lambda held: held.default_with(Locks[K]))


def _setting_holder(holder: Struct) -> Option[tuple[bpy.types.Scene, str]]:
    """Returns the scene of a drawn scene or view layer, with the layer's name, the empty name for the scene itself."""
    match holder:
        case bpy.types.Scene() as scene:
            return Some((scene, _SCENE_HOLDER))
        case bpy.types.ViewLayer(id_data=bpy.types.Scene() as scene, name=name):
            return Some((scene, name))
        case _:
            return Nothing


def _entry_holder(holder: Struct) -> Option[tuple[bpy.types.Scene, str]]:
    """Returns the scene and name of a drawn compositor entry."""
    match holder:
        case bpy.types.SceneCompositorEffect(id_data=bpy.types.Scene() as scene, name=name):
            return Some((scene, name))
        case _:
            return Nothing


# --- [COMPOSITION] ----------------------------------------------------------------------

CATEGORIES: Final = Categories()
SETTING_LOCKS: Final = Locking[RnaPath](key="render_setup_locks", form=Locks[RnaPath], holder=_setting_holder, text=rna, parsed=parsed)
ENTRY_LOCKS: Final = Locking[Key](key="entry_locks", form=Locks[Key], holder=_entry_holder, text=str, parsed=Ok)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "CATEGORIES",
    "ENTRY_LOCKS",
    "SETTING_LOCKS",
    "WHOLE",
    "Capture",
    "Categories",
    "CategorySettings",
    "EffectSetup",
    "EffectSetups",
    "EntryLocks",
    "LockRequest",
    "Locking",
    "Locks",
    "SetupScope",
    "Toggle",
]
