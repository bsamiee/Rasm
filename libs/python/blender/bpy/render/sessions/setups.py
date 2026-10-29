"""Render setups: what a render set captures from a member scene, setup recipes applied over unlocked members, and the locks setups keep, stored per scene for any key kind."""

from collections.abc import Callable, Mapping
from functools import cached_property, partial
from types import MappingProxyType
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
from blender.bpy.render.sessions.members import Category, KEY, Members, MemberScope
from blender.bpy.render.view_layers.passes import layer_of
from blender.bpy.settings.attachments import Stored
from blender.bpy.settings.presets import Preset
from blender.bpy.ui.layout.rows import OperatorButton, RowContent, RowDraw
from blender.bpy.ui.layout.values import hosted, Listing
from blender.records.faults import accumulated, Fault, Outcome, paired, Reason

# --- [TYPES] ----------------------------------------------------------------------------


class Capture[C: msgspec.Struct](Protocol):
    """What a render set captures from a member scene, writes back as a setup, and draws above its member lists."""

    @property
    def form(self) -> TypeForm[C]:
        """Record a capture decodes as, a setup preset's form."""
        ...

    @property
    def categories(self) -> tuple[Category, ...]:
        """Settings the set syncs from its source."""
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


# --- [MODELS] ---------------------------------------------------------------------------


class CategorySettings(msgspec.Struct, frozen=True, kw_only=True):
    """Category settings of one scene and of each of its view layers."""

    scene: Snapshot
    layers: Mapping[str, Snapshot]


class LockRequest(bpy.types.PropertyGroup):
    """Lock key a lock button toggles on the holder it drew, as the text the host button carries."""

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
        """Holders locking a key, the key one holder kind marks a whole holder locked by."""
        return frozenset(holder for holder, keys in self.held.items() if key in keys)


class Locking[K](msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Locks of one key kind every scene stores: the holder scene and name a drawn struct gives, the locks record over the key kind, the key as the text its button carries, one toggle command, and the toggle drawn beside each key."""

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

    def drawn(self, key: K, identity: Identity) -> Outcome[RowDraw]:
        """Draws a locked or unlocked toggle of a key's lock on the drawn holder."""

        def toggle(name: str, store: Stored[Locks[K]], draws: Mapping[bool, RowDraw], layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
            layout.context_pointer_set(name, holder)
            for scene, held in self.holder(holder).to_list():
                identity.reports.reported(lambda: _locked(store, scene=scene).map(lambda locks: draws[key in locks.of(held)](layout, holder, context)))().to_optional()

        request = Placement(self.operator, {"key": self.text(key)})
        return paired(
            identity.identifier(self.owner),
            self.stored(identity),
            accumulated(OperatorButton(target=request, keywords={"text": "", "emboss": False}, icon=Some("LOCKED" if held else "UNLOCKED")).drawn(identity) for held in (False, True)),
        ).map(lambda parts: partial(toggle, parts[0], parts[1], dict(zip((False, True), parts[2]))))


class Categories(msgspec.Struct, frozen=True, kw_only=True):
    """Category capture: the categories' settings of a scene and its unlocked view layers, setups keeping each locked setting."""

    categories: tuple[Category, ...] = ()

    @property
    def form(self) -> TypeForm[CategorySettings]:
        """Category settings record of a setup preset."""
        return CategorySettings

    @classmethod
    def captured(cls, members: Members, scene: bpy.types.Scene, /) -> Outcome[CategorySettings]:
        """Captures scene and view layer category settings."""
        layers = members.selected(bpy.types.ViewLayer)
        return paired(
            Snapshot.capture(scene, Some(members.selected(bpy.types.Scene))),
            accumulated(Snapshot.capture(layer, Some(layers)).map(lambda snapshot: (layer.name, snapshot)) for layer in scene.view_layers) if layers else Ok(()),
        ).map(lambda values: CategorySettings(scene=values[0], layers=dict(values[1])))

    @classmethod
    def projected(cls, members: Members, scene: bpy.types.Scene, recipe: CategorySettings, current: CategorySettings, /) -> Outcome[tuple[CategorySettings, CategorySettings]]:
        """Projects recipe settings over current ones on the scene and its unlocked view layers present in both, locked settings kept."""

        def held(locks: Locks[RnaPath]) -> Outcome[tuple[CategorySettings, CategorySettings]]:
            layers = tuple(layer for layer in members.free(scene.view_layers) if layer.name in recipe.layers and layer.name in current.layers)
            return paired(
                recipe.scene.projected(scene, current.scene, locks.of("")),
                accumulated(recipe.layers[layer.name].projected(layer, current.layers[layer.name], locks.of(layer.name)).map(lambda pair: (layer.name, pair)) for layer in layers),
            ).map(
                lambda values: (
                    CategorySettings(scene=values[0][0], layers={name: pair[0] for name, pair in values[1]}),
                    CategorySettings(scene=values[0][1], layers={**current.layers, **{name: pair[1] for name, pair in values[1]}}),
                )
            )

        return SETTING_LOCKS.read(members.identity, scene).bind(held)

    @classmethod
    def applied(cls, _members: Members, scene: bpy.types.Scene, sparse: CategorySettings, /) -> Outcome[None]:
        """Writes sparse scene and view layer settings, absent fault for a layer the scene no longer holds."""
        return (
            sparse.scene
            .apply(scene)
            .outcome.bind(lambda _: accumulated(layer_of(scene, name).bind(lambda layer: snapshot.apply(layer).outcome) for name, snapshot in sparse.layers.items()))
            .map(lambda _: None)
        )

    @classmethod
    def rows(cls, _members: Members, _scope: "SetupScope[CategorySettings]", /) -> tuple[RowContent, ...]:
        """Draws nothing above the member lists."""
        return ()


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

        return MemberScope(members=self.members, owner_type=bpy.types.Scene, unlocked=True).scenes(context).map(self.members.free).bind(lambda scenes: accumulated(map(planned, scenes))).bind(wrote)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _locked[K](store: Stored[Locks[K]], *, scene: bpy.types.Scene) -> Outcome[Locks[K]]:
    """Reads a scene's locks from a store, none before the first lock."""
    return store.read(scene).map(lambda held: held.default_with(Locks[K]))


def _setting_holder(holder: Struct) -> Option[tuple[bpy.types.Scene, str]]:
    """Returns the scene of a drawn scene or view layer, with the layer's name, the empty name for the scene itself."""
    match holder:
        case bpy.types.Scene() as scene:
            return Some((scene, ""))
        case bpy.types.ViewLayer(id_data=bpy.types.Scene() as scene, name=name):
            return Some((scene, name))
        case _:
            return Nothing


# --- [COMPOSITION] ----------------------------------------------------------------------

SETTING_LOCKS: Final = Locking[RnaPath](key="render_setup_locks", form=Locks[RnaPath], holder=_setting_holder, text=rna, parsed=parsed)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["SETTING_LOCKS", "Capture", "Categories", "CategorySettings", "LockRequest", "Locking", "Locks", "SetupScope"]
