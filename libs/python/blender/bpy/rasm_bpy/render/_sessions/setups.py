"""Render setup captures, recipes, presets, and locks on settings and compositor entries."""

from collections.abc import Callable, Mapping
from functools import cached_property, partial
from types import GenericAlias, MappingProxyType
from typing import Final, Generic, Protocol, Self, TypeVar

import bpy
from expression import Option, Some
import msgspec
from typing_extensions import TypeForm

from rasm_bpy.commands.operators import Answer, Call, Done, Operator, Placement, tallied, Unchanged
from rasm_bpy.data._extension.identity import Identity, Key
from rasm_bpy.data._extension.registration import Acquisitions
from rasm_bpy.data._rna.paths import _parsed, _rna, RnaPath, rooted, Struct
from rasm_bpy.data._rna.snapshots import Snapshot
from rasm_bpy.render._compositor.colors import COLOR_INPUT
from rasm_bpy.render._compositor.effects import CompositorRecord, Entry
from rasm_bpy.render._compositor.entries import Enabled
from rasm_bpy.render._compositor.inputs import _keyed
from rasm_bpy.render._compositor.offered import OfferedEffects
from rasm_bpy.render._compositor.recipes import _applied, _projected, recorded, Valued
from rasm_bpy.render._sessions.members import _KEY, Members
from rasm_bpy.render._view_layers.passes import _layer_of
from rasm_bpy.settings._attachments import Stored
from rasm_bpy.settings._presets import Preset
from rasm_bpy.ui._layout.rows import _operator_button, ConditionalRow, Layout, RowContent, RowDraw
from rasm_bpy.ui._layout.templates import _PresetGallery
from rasm_bpy.ui._layout.values import _hosted, Listing
from rasm_records import accumulated, content_hash, Fault, Name, Nothing, Ok, Outcome, paired, Reason

# --- [TYPES] ----------------------------------------------------------------------------

type Toggle[K] = Callable[[K, bpy.types.UILayout, Struct, bpy.types.Context], None]

_R_co = TypeVar("_R_co", bound=CompositorRecord, default=CompositorRecord, covariant=True)


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

    def rows(self, members: Members, scope: "_SetupScope[C]", /) -> tuple[RowContent, ...]:
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


class EffectSetup(msgspec.Struct, Generic[_R_co], frozen=True, kw_only=True):  # ruff: ignore[non-pep695-generic-class]
    """Category settings of a scene and its view layers, and the compositor entries it holds by name in stack order."""

    settings: CategorySettings
    effects: Mapping[Name, Valued[Entry[_R_co]]]


class LockRequest(bpy.types.PropertyGroup):
    """Lock key a lock button toggles on the holder it drew, as the text the host button shows."""

    key: bpy.props.StringProperty(name="Key", description="Lock key the button toggles")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class _Locks[K](msgspec.Struct, frozen=True, kw_only=True, omit_defaults=True):
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


class _Locking[K](msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Per-scene locks of one key kind with their toggle command and button."""

    key: Key
    form: TypeForm[_Locks[K]]
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

    def stored(self, identity: Identity) -> Stored[_Locks[K]]:
        """Binds the per-scene lock store."""
        return Stored.keyed(identity, self.key, msgspec.msgpack.Decoder(self.form))

    def read(self, identity: Identity, scene: bpy.types.Scene) -> Outcome[_Locks[K]]:
        """Reads a scene's locks, none before the first lock."""
        return _locked(self.stored(identity), scene=scene)

    def lock_toggle(self, call: Call[LockRequest]) -> Outcome[Done]:
        """Toggles a key's lock on the holder its button drew, in the holder scene's lock store."""
        store = self.stored(call.identity)

        def toggled(owner: Struct, key: K) -> Outcome[Done]:
            return (
                self
                .holder(owner)
                .to_result_with(lambda: Fault.of(Reason.ABSENT, repr(owner), member=self.owner))
                .bind(lambda found: _locked(store, scene=found[0]).bind(lambda locks: store.write(found[0], locks.toggled(found[1], key))))
                .map(lambda _: Done())
            )

        return paired(_hosted(call.identity.identifier(self.owner), call.context), self.parsed(call.properties.key)).bind(lambda parts: toggled(*parts))

    def toggles(self, identity: Identity) -> Toggle[K]:
        """Binds the lock store once into the draw of any key's locked or unlocked toggle on the drawn holder."""

        def toggle(name: str, store: Stored[_Locks[K]], key: K, layout: bpy.types.UILayout, holder: Struct, _context: bpy.types.Context) -> None:
            request = Placement(self.operator, {"key": self.text(key)})

            def shown(scene: bpy.types.Scene, held: str) -> None:
                identity.reports.faulted(
                    _locked(store, scene=scene).bind(
                        lambda locks: _operator_button(layout, identity, request, {"text": "", "emboss": False, "icon": "LOCKED" if key in locks.of(held) else "UNLOCKED"})
                    )
                )

            layout.context_pointer_set(name, holder)
            for found in self.holder(holder).to_list():
                shown(*found)

        return partial(toggle, identity.identifier(self.owner), self.stored(identity))

    def drawn(self, key: K, identity: Identity) -> Outcome[RowDraw]:
        """Draws the toggle of a key's lock on the drawn holder."""
        return Ok(partial(self.toggles(identity), key))

    def row(self, key: K) -> RowContent:
        """Lock toggle of a key on the drawn holder with its command."""
        return Layout(builder=Some(lambda identity: self.drawn(key, identity).map(lambda draw: (draw,))), prerequisites=(self.operator,))


class EntryLocks(ConditionalRow, frozen=True, kw_only=True):
    """Closed panel over a drawn compositor entry listing its enable flags and visible inputs, each beside the toggle of its entry lock."""

    @staticmethod
    def acquisitions(_identity: Identity, /) -> Acquisitions:
        """Acquires the entry lock command."""
        return (ENTRY_LOCKS.operator,)

    def drawn(self, identity: Identity, /) -> Outcome[RowDraw]:
        """Binds the entry lock store once into the panel draw."""
        return Ok(self.conditioned(partial(self.shown, identity, ENTRY_LOCKS.toggles(identity))))

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
            identity.reports.faulted(_keyed(identity, entry).map(lambda pair: listed(entry, *pair)))


class _Categories(msgspec.Struct, frozen=True, kw_only=True):
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

        def held(locks: _Locks[RnaPath]) -> Outcome[tuple[CategorySettings, CategorySettings]]:
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

        return _SETTING_LOCKS.read(members.identity, scene).bind(held)

    @staticmethod
    def applied(_members: Members, scene: bpy.types.Scene, sparse: CategorySettings, /) -> Outcome[None]:
        """Writes sparse scene and view layer settings, absent fault for a layer the scene no longer holds."""
        return (
            sparse.scene
            .apply(scene)
            .outcome.bind(lambda _: accumulated(_layer_of(scene, name).bind(lambda layer: snapshot.apply(layer).outcome) for name, snapshot in sparse.layers.items()))
            .map(lambda _: None)
        )

    @staticmethod
    def rows(_members: Members, _scope: "_SetupScope[CategorySettings]", /) -> tuple[RowContent, ...]:
        """Draws nothing above the member lists."""
        return ()


class EffectSetups(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Category capture with the compositor entries the `effects` own, entry locks kept by every setup, and setup presets drawn as a gallery of `tile` icons.

    `tile` returns the preview icon of a slot showing the preview-enabled entries a setup projects over the drawn scene, in stack order.
    """

    effects: OfferedEffects
    tile: Option[Callable[[Identity, bpy.types.Scene, Key, tuple[Valued[Entry], ...]], Option[int]]] = Nothing

    @cached_property
    def form(self) -> TypeForm[EffectSetup]:
        """Setup record over the offered effect records."""
        return GenericAlias(EffectSetup, (self.effects.form,))  # type: ignore[return-value]  # ty: ignore[invalid-type-form, unsound-return-statement]

    def captured(self, members: Members, scene: bpy.types.Scene, /) -> Outcome[EffectSetup]:
        """Captures category settings and owned entries."""
        return paired(_CATEGORIES.captured(members, scene), recorded(self.effects, members.identity, scene)).map(lambda pair: EffectSetup(settings=pair[0], effects=pair[1]))

    @staticmethod
    def projected(members: Members, scene: bpy.types.Scene, recipe: EffectSetup, current: EffectSetup, /) -> Outcome[tuple[EffectSetup, EffectSetup]]:
        """Projects recipe settings and entries over current ones, setting and entry locks kept."""
        return paired(_CATEGORIES.projected(members, scene, recipe.settings, current.settings), ENTRY_LOCKS.read(members.identity, scene)).map(
            lambda parts: (
                EffectSetup(settings=parts[0][0], effects=recipe.effects),
                EffectSetup(settings=parts[0][1], effects=_projected(current.effects, recipe.effects, locked=parts[1].whole(WHOLE), fields=parts[1].held)),
            )
        )

    def applied(self, members: Members, scene: bpy.types.Scene, sparse: EffectSetup, /) -> Outcome[None]:
        """Writes sparse settings and entries, locked entries and fields kept."""
        return paired(
            _CATEGORIES.applied(members, scene, sparse.settings),
            ENTRY_LOCKS.read(members.identity, scene).bind(lambda locks: _applied(self.effects, members.identity, scene, sparse.effects, locked=locks.whole(WHOLE), fields=locks.held)),
        ).map(lambda _: None)

    def rows(self, _members: Members, scope: "_SetupScope[EffectSetup]", /) -> tuple[RowContent, ...]:
        """Draws setup presets as tiles above the member lists while a tile source is given."""
        return tuple(_PresetGallery(listing=scope.listing, tile=partial(self.tiled, tile)) for tile in self.tile.to_list())

    def tiled(self, tile: Callable[[Identity, bpy.types.Scene, Key, tuple[Valued[Entry], ...]], Option[int]], identity: Identity, context: bpy.types.Context | None, setup: EffectSetup) -> Option[int]:
        """Returns the icon of the preview-enabled entries a setup projects over the context scene's entries, locks kept."""

        def shown(scene: bpy.types.Scene, current: Mapping[Name, Valued[Entry]], locks: _Locks[Key]) -> Option[int]:
            entries = tuple(held for held in _projected(current, setup.effects, locked=locks.whole(WHOLE), fields=locks.held).values() if Enabled.PREVIEW in held.enables)
            return tile(identity, scene, f"{_KEY}_{content_hash(EffectSetups, setup).hex()}", entries)

        return (
            Option
            .of_optional(context)
            .bind(lambda held: rooted(bpy.types.Scene, held).to_option())
            .bind(lambda scene: paired(recorded(self.effects, identity, scene), ENTRY_LOCKS.read(identity, scene)).to_option().bind(lambda parts: shown(scene, *parts)))
        )


class _SetupScope[C: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """Setup recipes of a render set's capture over its unlocked member scenes."""

    members: Members
    capture: Capture[C]

    @property
    def listing(self) -> Listing[C]:
        """Setup preset actions with matches against the active scene."""
        return Listing(preset=Preset(key=_KEY, form=self.capture.form), current=self.current, matches=self.matched, applied=self.applied)

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


def _locked[K](store: Stored[_Locks[K]], *, scene: bpy.types.Scene) -> Outcome[_Locks[K]]:
    """Reads a scene's locks from a store, none before the first lock."""
    return store.read(scene).map(lambda held: held.default_with(_Locks[K]))


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

_CATEGORIES: Final = _Categories()
_SETTING_LOCKS: Final = _Locking[RnaPath](key="render_setup_locks", form=_Locks[RnaPath], holder=_setting_holder, text=_rna, parsed=_parsed)
ENTRY_LOCKS: Final = _Locking[Key](key="entry_locks", form=_Locks[Key], holder=_entry_holder, text=str, parsed=Ok)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ENTRY_LOCKS", "WHOLE", "Capture", "CategorySettings", "EffectSetup", "EffectSetups", "EntryLocks", "LockRequest", "Toggle"]
