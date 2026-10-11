"""Render sets, native category transfers, stored reversals, and edits across members."""

from collections.abc import Callable, Generator, Hashable
from contextlib import contextmanager, ExitStack
from functools import partial
from itertools import starmap
from typing import Annotated, ClassVar

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.collections import Map
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt

from rasm.blender.commands.operators import Acquired, Call, Confirm, Done, Empty, Operator, Own, Suppressible
from rasm.blender.data.ids.ownership import Mark, Ownership, Saved
from rasm.blender.data.ids.store import named
from rasm.blender.data.rna.conditions import Condition, Setting
from rasm.blender.data.rna.descriptors import Descriptor, struct_at, Value
from rasm.blender.data.rna.snapshots import Applied, NativeValue, Snapshot
from rasm.blender.extension.events.handlers import UpdateHandler
from rasm.blender.extension.identity import Identity, Key, Name
from rasm.blender.extension.registration import Acquisitions, Registrable
from rasm.blender.faults import BlenderAbsentFault, BlenderCollisionFault, BlenderEditFault, BlenderFaults, BlenderReadFault, BlenderResult
from rasm.blender.render.effects.stack import Configured, EffectName, EffectStack, Named, projected, viewed
from rasm.blender.render.previews.scenes import Chain, PreviewEngine
from rasm.blender.render.previews.tiles import Tiles
from rasm.blender.render.view_layers import passes
from rasm.blender.settings.attachments import Attached, Attachment, Host, Stored
from rasm.blender.settings.presets import Preset
from rasm.blender.settings.texts import TextRecord
from rasm.blender.ui.layout.browser import Browser
from rasm.blender.ui.layout.lists import ItemList
from rasm.blender.ui.layout.rows import Matrix, Member, OperatorButton, Owner, Property, Pulldown, RowContent, RowDraw
from rasm.blender.ui.layout.sections import Drawn, hosted, Listing, Single
from rasm.blender.ui.screens.areas import AreaQuery
from rasm.blender.ui.surfaces.menus import Captured, Dropdown, Menu, PresetMenu
from rasm.imaging.chain import Effect
from rasm.imaging.color.processors import DisplayView
from rasm.imaging.faults import accumulated, paired
from rasm.imaging.hashing import content_hash
from rasm.imaging.parameters import Case
from rasm.rendering.edits import Reparametrized, transfer, Tree

# --- [MODELS] ---------------------------------------------------------------------------


class Copies(Case, frozen=True, tag=1):
    """Scenes copied from a retained source scene."""

    title = "Scenes"


class Layers(Case, frozen=True, tag=2):
    """View layers belonging to one scene."""

    title = "View Layers"


type Form = Copies | Layers


class Sets(msgspec.Struct, frozen=True, kw_only=True):
    """File's set form and native edit-across preference."""

    form: Annotated[Form, msgspec.Meta(title="Render Sets", description="Copied scenes or the view layers of one scene")] = Layers()
    across: Annotated[bool, msgspec.Meta(title="Edit Across", description="Apply native category edits to every unlocked member")] = False


class SceneMember(msgspec.Struct, frozen=True, kw_only=True):
    """Synchronization lock and dispatch admission of a member scene."""

    locked: Annotated[bool, msgspec.Meta(title="Locked")] = False
    rendered: Annotated[bool, msgspec.Meta(title="Render")] = True


class LayerMember(msgspec.Struct, frozen=True, kw_only=True):
    """Synchronization lock of a native view layer."""

    locked: Annotated[bool, msgspec.Meta(title="Locked")] = False


class Category(msgspec.Struct, frozen=True, kw_only=True):
    """Native setting selection shared by sync, presets, and the board."""

    key: Key
    paths: tuple[str, ...]
    root: type[bpy.types.Scene] | type[bpy.types.ViewLayer] = bpy.types.Scene

    @property
    def title(self) -> str:
        """Blender's display name of the category key."""
        return bpy.path.display_name(self.key)


class Converting(msgspec.Struct, frozen=True, kw_only=True):
    """Requested form for native scene and view-layer transfer."""

    form: Annotated[Form, msgspec.Meta(title="Render Sets")] = Copies()


class Setup[E: Effect](msgspec.Struct, frozen=True, kw_only=True):
    """Sparse native render settings and ordered owned compositor entries."""

    scene: Snapshot
    layers: dict[str, Snapshot]
    effects: dict[EffectName, Configured[E | DisplayView]]


class SetupLocks(msgspec.Struct, frozen=True, kw_only=True):
    """Native setting paths, entry order, and effect fields retained by setup presets."""

    scene: frozenset[str] = frozenset()
    layers: dict[str, frozenset[str]] = msgspec.field(default_factory=dict)
    effects: frozenset[EffectName] = frozenset()
    fields: dict[EffectName, frozenset[tuple[Key | int, ...]]] = msgspec.field(default_factory=dict)

    @staticmethod
    def bound(identity: Identity) -> BlenderResult[Stored["SetupLocks"]]:
        """Binds the setup lock store shared by native settings and effect controls."""
        return identity.name(Name.ATTACHMENT, "render_setup_locks").map(lambda name: Stored(name=name, decoder=msgspec.msgpack.Decoder(SetupLocks)))

    @staticmethod
    def effect_actions() -> Callable[[Option[tuple[Key | int, ...]]], RowContent]:
        """Declares one native command for entry and typed parameter locks."""

        def changed(call: Call[SetupEffectLock]) -> BlenderResult[Done]:
            path: BlenderResult[tuple[Key | int, ...] | None] = catch(lambda: msgspec.json.decode(call.record.path, type=tuple[str | int, ...] | None), exception=msgspec.DecodeError)().map_error(
                lambda error: (BlenderReadFault(request=call.record.path, error=error),)
            )

            def written(store: Stored[SetupLocks], scene: bpy.types.Scene, path: tuple[Key | int, ...] | None) -> BlenderResult[Done]:
                return (
                    store
                    .read(scene)
                    .map(lambda held: held.default_with(SetupLocks))
                    .bind(
                        lambda locks: store.write(
                            scene,
                            msgspec.structs.replace(locks, effects=locks.effects ^ {call.record.name})
                            if path is None
                            else msgspec.structs.replace(locks, fields=locks.fields | {call.record.name: locks.fields.get(call.record.name, frozenset()) ^ {path}}),
                        )
                    )
                    .map(lambda _: Done())
                )

            return paired(SetupLocks.bound(call.identity), paired(call.root(bpy.types.Scene), path)).bind(lambda parts: written(parts[0], *parts[1]))

        command = Operator(
            key="render_setup_effect_lock", title="Lock Setup Effect", description="Keep or release this entry or parameter when applying a render setup", form=SetupEffectLock, run=changed, undo=True
        )

        def drawn(path: Option[tuple[Key | int, ...]], identity: Identity) -> BlenderResult[RowDraw]:
            def shown(store: Stored[SetupLocks], layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                if isinstance(holder, bpy.types.SceneCompositorEffect) and isinstance(holder.id_data, bpy.types.Scene):
                    scene = holder.id_data
                    identity.reports.reported(
                        lambda: (
                            store
                            .read(scene)
                            .map(lambda held: held.default_with(SetupLocks))
                            .bind(
                                lambda locks: (
                                    OperatorButton(
                                        target=Own(command=command, values=SetupEffectLock(name=holder.name, path=msgspec.json.encode(path.to_optional()).decode())),
                                        text=Some(""),
                                        icon=Some("LOCKED" if path.map(lambda path: path in locks.fields.get(holder.name, frozenset())).default_value(holder.name in locks.effects) else "UNLOCKED"),
                                        emboss=False,
                                    )
                                    .drawn(identity)
                                    .map(lambda draw: draw(layout, holder, context))
                                )
                            )
                        )
                    )().default_value(None)

            return SetupLocks.bound(identity).map(lambda store: partial(shown, store))

        return lambda path: (command, partial(drawn, path))


class SetupLock(msgspec.Struct, frozen=True, kw_only=True):
    """Canonical native setting path toggled from its owner's matrix cell."""

    path: str


class SetupEffectLock(Named, frozen=True, kw_only=True):
    """Native operator transport for an optional canonical setup parameter path."""

    path: str


class Members(msgspec.Struct, frozen=True, kw_only=True):
    """Acquired stores and resolved native definitions used by every set operation."""

    identity: Identity
    sets: Attached[Sets]
    tag: Descriptor[bpy.types.EnumProperty]
    scenes: Attached[SceneMember]
    layers: Attached[LayerMember]
    categories: tuple[tuple[Category, tuple[Descriptor, ...]], ...]
    samples: Map[str, Descriptor[bpy.types.IntProperty]]
    recovery: Stored[tuple[Tree[tuple[tuple[object, str], object]], Tree[tuple[tuple[object, str], object]] | None]]

    # --- [MEMBERSHIP]

    def form(self, context: bpy.types.Context) -> BlenderResult[Form]:
        """Reads the file's current form from the native window manager."""
        match context.window_manager:
            case bpy.types.WindowManager() as manager:
                return self.sets.read(manager).map(lambda record: record.form)
            case None:
                return Error((BlenderAbsentFault(owner="bpy.context", key="window_manager"),))

    def source(self, context: bpy.types.Context) -> BlenderResult[bpy.types.Scene]:
        """Returns the active scene's marked source, or the active scene itself."""
        match context.scene:
            case bpy.types.Scene() as scene:
                match Mark.read(self.identity, scene).filter(lambda mark: mark.key == RenderSet.ownership.key).bind(lambda mark: mark.of).to_optional():
                    case bpy.types.Scene() as source:
                        return Ok(source)
                    case _:
                        return Ok(scene)
            case None:
                return Error((BlenderAbsentFault(owner="bpy.context", key="scene"),))

    def copies(self, context: bpy.types.Context) -> BlenderResult[tuple[bpy.types.Scene, ...]]:
        """Returns the source followed by its marked member scenes in native order."""
        return self.source(context).map(lambda source: (source, *(scene for scene, mark in RenderSet.ownership.held(self.identity) if mark.of == Some(source))))

    def members(self, context: bpy.types.Context) -> BlenderResult[tuple[bpy.types.Scene | bpy.types.ViewLayer, ...]]:
        """Returns the form's owners in native list order."""

        def listed(form: Form) -> BlenderResult[tuple[bpy.types.Scene | bpy.types.ViewLayer, ...]]:
            match form:
                case Copies():
                    return self.copies(context)
                case Layers():
                    return self.source(context).map(lambda scene: tuple(scene.view_layers))

        return self.form(context).bind(listed)

    def free[O: bpy.types.Scene | bpy.types.ViewLayer](self, owners: tuple[O, ...]) -> BlenderResult[tuple[O, ...]]:
        """Reads every member lock and returns the unlocked owners in the same order."""

        def locked(owner: bpy.types.Scene | bpy.types.ViewLayer) -> BlenderResult[bool]:
            return (self.scenes.read(owner) if isinstance(owner, bpy.types.Scene) else self.layers.read(owner)).map(lambda member: member.locked)

        return accumulated(map(locked, owners)).map(lambda locks: tuple(owner for owner, lock in zip(owners, locks, strict=True) if not lock))

    def unlocked(self, context: bpy.types.Context) -> BlenderResult[tuple[bpy.types.Scene | bpy.types.ViewLayer, ...]]:
        """Returns unlocked members after reading the native file form."""
        return self.members(context).bind(self.free)

    def rendered(self, context: bpy.types.Context) -> BlenderResult[tuple[tuple[bpy.types.Scene, Option[bpy.types.ViewLayer]], ...]]:
        """Returns dispatch scenes, with a selected layer for the layer form."""

        def dispatched(form: Form) -> BlenderResult[tuple[tuple[bpy.types.Scene, Option[bpy.types.ViewLayer]], ...]]:
            match form:
                case Copies():
                    return self.copies(context).bind(
                        lambda scenes: accumulated(self.scenes.read(scene) for scene in scenes).map(
                            lambda records: tuple((scene, Nothing) for scene, record in zip(scenes, records, strict=True) if record.rendered)
                        )
                    )
                case Layers():
                    return self.source(context).map(lambda scene: tuple((scene, Some(layer)) for layer in scene.view_layers if layer.use))

        return self.form(context).bind(dispatched)

    # --- [SETTINGS]

    def captured(self, category: Category, context: bpy.types.Context) -> BlenderResult[Snapshot]:
        """Captures a category from its source scene or the corresponding named layer."""
        selected = next(selected for held, selected in self.categories if held == category)

        def origin(scene: bpy.types.Scene) -> BlenderResult[bpy.types.Scene | bpy.types.ViewLayer]:
            match category.root, context.view_layer:
                case root, _ if root is bpy.types.Scene:
                    return Ok(scene)
                case _, bpy.types.ViewLayer(name=name):
                    return Option.of_optional(scene.view_layers.get(name)).to_result_with(lambda: (BlenderAbsentFault(owner=repr(scene.view_layers), key=name),))
                case _, None:
                    return Error((BlenderAbsentFault(owner="bpy.context", key="view_layer"),))

        return self.source(context).bind(origin).bind(lambda source: Snapshot.capture(source, Some(selected)))

    def samples_of(self, scene: bpy.types.Scene, layer: bpy.types.ViewLayer) -> BlenderResult[int]:
        """Reads the native layer override or its engine's sample setting."""
        if layer.samples:
            return Ok(layer.samples)
        selected: BlenderResult[Descriptor[bpy.types.IntProperty]] = self.samples.try_find(scene.render.engine).to_result_with(
            lambda: (BlenderAbsentFault(owner=repr(scene.render), key=f"{scene.render.engine} samples"),)
        )
        return Result[Descriptor[bpy.types.IntProperty], BlenderFaults].bind(
            selected,
            lambda descriptor: (
                struct_at(scene, descriptor.owner).map(lambda owner: getattr(owner, descriptor.prop.identifier)).to_result_with(lambda: (BlenderAbsentFault(owner=repr(scene), key=descriptor.owner),))
            ),
        )


class MemberScope(msgspec.Struct, frozen=True, kw_only=True):
    """Native scene or layer owners selected by set form and synchronization locks."""

    members: Members
    owner_type: type[bpy.types.Scene] | type[bpy.types.ViewLayer]
    unlocked: bool

    def owners(self, context: bpy.types.Context) -> BlenderResult[tuple[bpy.types.Scene | bpy.types.ViewLayer, ...]]:
        """Selects scene or layer owners and reads every requested lock."""
        return (
            self
            .scenes(context)
            .map(lambda scenes: tuple(layer for scene in scenes for layer in scene.view_layers) if self.owner_type is bpy.types.ViewLayer else scenes)
            .bind(lambda owners: self.members.free(owners) if self.unlocked else Ok(owners))
        )

    def scenes(self, context: bpy.types.Context) -> BlenderResult[tuple[bpy.types.Scene, ...]]:
        """Returns source scenes selected by the file's member form."""
        return self.members.form(context).bind(lambda form: self.members.copies(context) if isinstance(form, Copies) else self.members.source(context).map(lambda scene: (scene,)))


# --- [SERVICES] -------------------------------------------------------------------------


class SetupScope[E: Effect](msgspec.Struct, frozen=True, kw_only=True):
    """One render set's complete recipes and native parameter-lock store."""

    members: Members
    stack: EffectStack[E]
    locks: Stored[SetupLocks]

    @classmethod
    def bound(cls, members: Members, stack: EffectStack[E]) -> BlenderResult["SetupScope[E]"]:
        """Binds persistent locks to the acquired render-set identity."""
        return SetupLocks.bound(members.identity).map(lambda locks: cls(members=members, stack=stack, locks=locks))

    def captured(self, scene: bpy.types.Scene) -> BlenderResult[Setup[E]]:
        """Captures category settings and ordered effects from one native scene."""
        selected = tuple(descriptor for category, descriptors in self.members.categories if category.root is bpy.types.Scene for descriptor in descriptors)
        layers = tuple(descriptor for category, descriptors in self.members.categories if category.root is bpy.types.ViewLayer for descriptor in descriptors)
        return paired(
            Snapshot.capture(scene, Some(selected)),
            paired(
                accumulated(Snapshot.capture(layer, Some(layers)).map(lambda snapshot: (layer.name, snapshot)) for layer in scene.view_layers) if layers else Ok(()),
                self.stack.captured(self.members.identity, scene),
            ),
        ).map(lambda values: Setup(scene=values[0], layers=dict(values[1][0]), effects=values[1][1]))

    def current(self, context: bpy.types.Context) -> BlenderResult[Setup[E]]:
        """Captures the active scene without substituting a source or another render set."""
        scene: BlenderResult[bpy.types.Scene] = Option.of_optional(context.scene).to_result_with(lambda: (BlenderAbsentFault(owner="bpy.context", key="scene"),))
        return Result[bpy.types.Scene, BlenderFaults].bind(scene, self.captured)

    def held(self, scene: bpy.types.Scene) -> BlenderResult[tuple[SetupLocks, tuple[bpy.types.ViewLayer, ...]]]:
        """Reads parameter locks and every native view-layer member lock once."""
        return paired(
            self.locks.read(scene).map(lambda held: held.default_with(SetupLocks)),
            accumulated(self.members.layers.read(layer).map(lambda member: (layer, member.locked)) for layer in scene.view_layers).map(
                lambda rows: tuple(layer for layer, locked in rows if not locked)
            ),
        )

    def projected(self, scene: bpy.types.Scene, recipe: Setup[E], current: Setup[E], locks: SetupLocks, layers: tuple[bpy.types.ViewLayer, ...]) -> BlenderResult[tuple[Setup[E], Setup[E]]]:
        """Projects sparse settings and effect fields over captured current values."""
        return paired(
            recipe.scene.projected(scene, current.scene, locks.scene),
            accumulated(
                recipe.layers[layer.name].projected(layer, current.layers[layer.name], locks.layers.get(layer.name, frozenset())).map(lambda pair: (layer.name, pair))
                for layer in layers
                if layer.name in recipe.layers
            ),
        ).map(
            lambda values: (
                Setup(scene=values[0][0], layers={name: pair[0] for name, pair in values[1]}, effects=recipe.effects),
                Setup(
                    scene=values[0][1],
                    layers=current.layers | {name: pair[1] for name, pair in values[1]},
                    effects=projected(current.effects, recipe.effects, locked=locks.effects, fields=locks.fields),
                ),
            )
        )

    def applied(self, context: bpy.types.Context, recipe: Setup[E]) -> BlenderResult[Done]:
        """Applies sparse recipes across unlocked members, sharing scene writes under layers."""

        def written(scene: bpy.types.Scene) -> BlenderResult[None]:
            def prepared(held: tuple[Setup[E], tuple[SetupLocks, tuple[bpy.types.ViewLayer, ...]]]) -> BlenderResult[None]:
                current, (locks, layers) = held
                return self.projected(scene, recipe, current, locks, layers).bind(
                    lambda pair: (
                        pair[0]
                        .scene.apply(scene)
                        .outcome.bind(
                            lambda _: paired(
                                accumulated(snapshot.apply(scene.view_layers[name]).outcome for name, snapshot in pair[0].layers.items()),
                                self.stack.applied(self.members.identity, scene, pair[0].effects, locked=locks.effects, fields=locks.fields),
                            ).map(lambda _: None)
                        )
                    )
                )

            return paired(self.captured(scene), self.held(scene)).bind(prepared)

        return (
            MemberScope(members=self.members, owner_type=bpy.types.Scene, unlocked=True)
            .scenes(context)
            .bind(self.members.free)
            .bind(lambda scenes: accumulated(written(scene) for scene in scenes).map(lambda _: Done()))
        )

    def candidate(self, _identity: Identity, context: bpy.types.Context, recipe: Setup[E], head: Option[npt.NDArray[np.float32]]) -> BlenderResult[Chain]:
        """Projects effect parameters over the current frame without applying scene settings."""
        match context.scene, head:
            case bpy.types.Scene() as scene, Option(tag="some"):
                return (
                    paired(self.captured(scene), self.held(scene))
                    .bind(lambda held: self.projected(scene, recipe, held[0], *held[1]))
                    .map(lambda pair: Chain(head=head, offered=pair[1].effects, initial_view=Some(viewed(scene))))
                )
            case None, _:
                return Error((BlenderAbsentFault(owner="bpy.context", key="scene"),))
            case _, Option():
                return Error((BlenderAbsentFault(owner="render setup preview", key="completed render"),))

    def browser(self, preset: Preset[Setup[E]], tiles: Tiles, main: AreaQuery) -> Browser[Setup[E]]:
        """Composes the complete setup catalog with native actions and transient previews."""
        return Browser(
            listing=Listing(preset=preset, current=self.current, matches=lambda context, recipes: self.observed(context).bind(lambda observed: observed[1](recipes)), applied=self.applied),
            tiles=tiles,
            candidate=self.candidate,
            observed=self.observed,
            main=main,
        )

    def observed(self, context: bpy.types.Context) -> BlenderResult[tuple[Hashable, Callable[[tuple[Setup[E], ...]], BlenderResult[tuple[bool, ...]]]]]:
        """Captures one revision and its candidate matcher from the same native settings and locks."""
        match context.scene:
            case bpy.types.Scene() as scene:
                return paired(self.captured(scene), self.held(scene)).map(
                    lambda held: (
                        (content_hash(SetupScope, held[0], held[1][0]), tuple(layer.name for layer in held[1][1])),
                        lambda recipes: accumulated(self.projected(scene, recipe, held[0], *held[1]).map(lambda pair: pair[1] == held[0]) for recipe in recipes),
                    )
                )
            case None:
                return Error((BlenderAbsentFault(owner="bpy.context", key="scene"),))

    def actions(self) -> BlenderResult[tuple[Operator[SetupLock], Callable[[Identity, Descriptor], BlenderResult[RowDraw]]]]:
        """Composes native setting locks from matrix holders and canonical descriptors."""

        def changed(owner: Host, path: str) -> BlenderResult[Done]:
            match owner:
                case bpy.types.Scene():
                    return (
                        self.locks
                        .read(owner)
                        .map(lambda held: held.default_with(SetupLocks))
                        .bind(lambda locks: self.locks.write(owner, msgspec.structs.replace(locks, scene=locks.scene ^ {path})))
                        .map(lambda _: Done())
                    )
                case bpy.types.ViewLayer(id_data=bpy.types.Scene() as scene) as layer:
                    return (
                        self.locks
                        .read(scene)
                        .map(lambda held: held.default_with(SetupLocks))
                        .bind(lambda locks: self.locks.write(scene, msgspec.structs.replace(locks, layers=locks.layers | {layer.name: locks.layers.get(layer.name, frozenset()) ^ {path}})))
                        .map(lambda _: Done())
                    )
                case _:
                    return Error((BlenderAbsentFault(owner=repr(owner), key="render setup owner"),))

        name = self.members.identity.name(Name.ATTACHMENT, "render_setup_owner")
        command = Operator(
            key="render_setup_lock",
            title="Lock Setup Setting",
            description="Keep or release this native setting when applying a render setup",
            form=SetupLock,
            run=lambda call: name.bind(lambda name: hosted(name, call.context)).bind(lambda owner: changed(owner, call.record.path)),
        )

        def action(name: str, identity: Identity, descriptor: Descriptor) -> BlenderResult[RowDraw]:
            def shown(draws: dict[bool, RowDraw], layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                layout.context_pointer_set(name, holder)
                match holder:
                    case bpy.types.Scene():
                        selected = self.locks.read(holder).map(lambda held: held.default_with(SetupLocks).scene)
                    case bpy.types.ViewLayer(id_data=bpy.types.Scene() as scene, name=layer):
                        selected = self.locks.read(scene).map(lambda held: held.default_with(SetupLocks).layers.get(layer, frozenset()))
                    case _:
                        return
                identity.reports.reported(lambda: selected.map(lambda locks: draws[descriptor.path in locks](layout, holder, context)))().default_value(None)

            return accumulated(
                OperatorButton(target=Own(command=command, values=SetupLock(path=descriptor.path)), text=Some(""), icon=Some("LOCKED" if locked else "UNLOCKED"), emboss=False)
                .drawn(identity)
                .map(lambda draw: (locked, draw))
                for locked in (False, True)
            ).map(lambda draws: partial(shown, dict(draws)))

        return name.map(lambda name: (command, partial(action, name)))


class Setups[E: Effect](msgspec.Struct, frozen=True, kw_only=True):
    """Complete setup catalog and preview policy for a render-set header."""

    stack: EffectStack[E]
    tiles: Tiles
    main: AreaQuery

    def bound(self, members: Members) -> BlenderResult[tuple[Browser[Setup[E]], tuple[Operator[SetupLock], Callable[[Identity, Descriptor], BlenderResult[RowDraw]]]]]:
        """Composes browser actions over the section's acquired native members."""
        return SetupScope.bound(members, self.stack).bind(
            lambda scope: scope.actions().map(lambda actions: (scope.browser(Preset(key=RenderSet.key, form=Setup[self.stack.form]), self.tiles, self.main), actions))  # type: ignore[name-defined]  # ty: ignore[invalid-type-form]
        )


class RenderSet[E: Effect = Effect](msgspec.Struct, frozen=True, kw_only=True):
    """Native set stores, commands, preset menus, and the category board section."""

    key: ClassVar[Key] = "render_sets"
    title: ClassVar[str] = "Render Sets"
    popovers: ClassVar[tuple[Single[Empty], ...]] = ()
    condition: ClassVar[Option[Condition]] = Nothing
    record: ClassVar[TextRecord[Sets]] = TextRecord(key="render_set", form=Sets)
    scenes: ClassVar[Attachment[SceneMember]] = Attachment(key="render_set", form=SceneMember, owner=bpy.types.Scene)
    layers: ClassVar[Attachment[LayerMember]] = Attachment(key="render_set", form=LayerMember, owner=bpy.types.ViewLayer)
    ownership: ClassVar[Ownership[bpy.types.Scene]] = Ownership(kind=bpy.types.Scene, key="render_set", saved=Saved.ALWAYS, single=False)
    menu: ClassVar[Key] = key
    categories: tuple[Category, ...] = ()
    setup: Option[Setups[E]] = Nothing

    # --- [BINDING]

    @property
    def page_rows(self) -> tuple[Registrable, ...]:
        """Declares native stores, the setup browser's screen layout among them, before the lock row and its suppressible preference."""
        return (
            self.record,
            self.scenes,
            self.layers,
            *self.setup.map(lambda _: (Browser.layouts,)).default_value(()),
            Operator[Empty](
                key="render_set_lock",
                title="Lock Render Set",
                description="Keep or release the active member's synchronization lock",
                icon=Some("LOCKED"),
                form=Empty,
                run=lambda call: self.bound(call.identity).bind(lambda members: toggled(members, call.context)),
                invoke=Some(Suppressible[Empty](message=lambda _context, _record: "Unlocking a member never synced first copies every category from its source")),
            ),
        )

    def bound(self, identity: Identity) -> BlenderResult[Members]:
        """Resolves registered stores, category selections, and actual engine sample metadata."""

        def tagged(sets: Attached[Sets]) -> BlenderResult[tuple[Attached[Sets], Descriptor[bpy.types.EnumProperty]]]:
            form = next(field for field in msgspec.structs.fields(Sets) if field.name == "form")
            return Descriptor.resolve(bpy.types.WindowManager, f"{sets.name}.{form.name}.{Case.__struct_config__.tag_field}", bpy.types.EnumProperty).map(lambda tag: (sets, tag))

        stores = paired(self.record.attachment.bound(identity).bind(tagged), paired(self.scenes.bound(identity), self.layers.bound(identity)))
        selections = accumulated(
            accumulated(Descriptor.resolve(category.root, path, bpy.types.Property) for path in category.paths).map(lambda found: (category, found)) for category in self.categories
        )
        samples = accumulated(Descriptor.resolve(bpy.types.Scene, ".".join(engine.value), bpy.types.IntProperty).map(lambda descriptor: (engine.name, descriptor)) for engine in PreviewEngine)
        recovery = identity.name(Name.ATTACHMENT, "render_set_sync").map(
            lambda name: Stored(name=name, decoder=msgspec.msgpack.Decoder(tuple[Tree[tuple[tuple[object, str], object]], Tree[tuple[tuple[object, str], object]] | None]))
        )
        return paired(paired(stores, paired(selections, samples)), recovery).map(
            lambda joined: Members(
                identity=identity,
                sets=joined[0][0][0][0],
                tag=joined[0][0][0][1],
                scenes=joined[0][0][1][0],
                layers=joined[0][0][1][1],
                categories=joined[0][1][0],
                samples=Map.of_seq(joined[0][1][1]),
                recovery=joined[1],
            )
        )

    @staticmethod
    def captured(members: Members, category: Category) -> Captured:
        """Captures a category preset and applies it over its unlocked native owners."""
        return Captured(preset=Preset(key=category.key, form=Snapshot), scope=MemberScope(members=members, owner_type=category.root, unlocked=True), selection=category.paths)

    # --- [COMMANDS]

    def commands(self, members: Members) -> tuple[Operator[Empty] | Operator[Converting], ...]:
        """Returns native add, adopt, restore, category sync, reversal, and conversion rows."""

        def add(call: Call[Empty]) -> BlenderResult[Done]:
            return (
                members
                .copies(call.context)
                .bind(lambda scenes: added(members, scenes[0], f"{scenes[0].name} {len(scenes)}"))
                .map(lambda pair: Done(notes=(("INFO", f"Added {pair[0].name}"),), skipped=pair[1]))
            )

        def adopt(call: Call[Empty]) -> BlenderResult[Done]:
            def joined(source: bpy.types.Scene) -> BlenderResult[Done]:
                sources = frozenset(scene for _, mark in self.ownership.held(members.identity) for scene in mark.of.to_list())

                def marked(scene: bpy.types.Scene) -> BlenderResult[bpy.types.Scene]:
                    def kept() -> bpy.types.Scene:
                        scene.is_runtime_data = False
                        scene.use_fake_user = True
                        return scene

                    return (
                        Mark(key=self.ownership.key, of=Some(source))
                        .write(members.identity, scene)
                        .bind(lambda _: catch(kept, exception=AttributeError)().map_error(lambda error: (BlenderEditFault(request=repr(scene), error=error),)))
                    )

                return accumulated(
                    marked(scene) for scene in tuple(bpy.data.scenes) if scene != source and scene not in sources and not scene.is_runtime_data and Mark.read(members.identity, scene).is_none()
                ).map(lambda scenes: Done(notes=tuple(("INFO", f"Adopted {scene.name}") for scene in scenes)))

            return members.source(call.context).bind(joined)

        def restore(call: Call[Empty]) -> BlenderResult[Done]:
            def removed(scenes: tuple[bpy.types.Scene, ...]) -> BlenderResult[Done]:
                source, *copies = scenes
                for window in (window for manager in bpy.data.window_managers for window in manager.windows):
                    if window.scene in copies:
                        window.scene = source
                return accumulated(
                    catch(lambda: bpy.data.scenes.remove(scene), exception=RuntimeError)().map_error(lambda error: (BlenderEditFault(request=repr(scene), error=error),)).map(lambda _: name)
                    for scene in copies
                    for name in (scene.name,)
                ).map(lambda names: Done(notes=tuple(("INFO", f"Removed {name}") for name in names)))

            return members.copies(call.context).bind(removed)

        def touched(context: bpy.types.Context, record: Converting) -> BlenderResult[str]:
            return members.members(context).map(lambda owners: f"Convert {', '.join(owner.name for owner in owners)} to {record.form.title}")

        return (
            Operator[Empty](key="render_set_add", title="Add Render Set", description="Copy the source scene into a retained member", icon=Some("ADD"), form=Empty, run=add),
            Operator[Empty](key="render_set_adopt", title="Adopt Scenes", description="Join the file's unmarked scenes to the source's render sets", icon=Some("IMPORT"), form=Empty, run=adopt),
            Operator[Empty](key="render_set_restore", title="Restore Source", description="Remove copied members and show their original scene", icon=Some("SCENE_DATA"), form=Empty, run=restore),
            *(syncing(members, category) for category in self.categories),
            Operator[Empty](
                key="render_set_unsync",
                title="Unsync Render Sets",
                description="Restore every unlocked member's pre-sync native settings",
                icon=Some("LOOP_BACK"),
                form=Empty,
                run=lambda call: unsynced(members, call.context).map(lambda applied: Done(notes=(("INFO", f"Restored {sum(len(each.written) for each in applied)} settings"),))),
            ),
            Operator[Converting](
                key="render_set_convert",
                title="Convert Render Sets",
                description="Transfer render sets between copied scenes and view layers",
                icon=Some("ARROW_LEFTRIGHT"),
                form=Converting,
                run=lambda call: converted(members, call.record.form, call.context).map(lambda names: Done(notes=tuple(("INFO", f"Created {name}") for name in names))),
                invoke=Some(Confirm[Converting](message=Some(touched))),
            ),
        )

    @staticmethod
    def listed(members: Members) -> Operator[Empty]:
        """Declares removal of the active marked scene without admitting user scenes."""

        def remove(call: Call[Empty]) -> BlenderResult[Done]:
            def removed(source: bpy.types.Scene) -> BlenderResult[Done]:
                match call.context.scene:
                    case bpy.types.Scene() as scene if Mark.read(members.identity, scene).filter(lambda mark: mark.key == RenderSet.ownership.key and mark.of == Some(source)).is_some():
                        name = scene.name
                        for window in (window for manager in bpy.data.window_managers for window in manager.windows):
                            if window.scene == scene:
                                window.scene = source
                        return (
                            catch(lambda: bpy.data.scenes.remove(scene), exception=RuntimeError)()
                            .map_error(lambda error: (BlenderEditFault(request=name, error=error),))
                            .map(lambda _: Done(notes=(("INFO", f"Removed {name}"),)))
                        )
                    case _:
                        return Error((BlenderAbsentFault(owner=repr(call.context.scene), key=RenderSet.ownership.key),))

            return members.source(call.context).bind(removed)

        return Operator[Empty](key="render_set_remove", title="Remove Render Set", description="Remove the active copied member", icon=Some("REMOVE"), form=Empty, run=remove)

    # --- [SECTION]

    def composed(self, members: Members, commands: tuple[Operator[Empty] | Operator[Converting], ...]) -> BlenderResult[Single]:
        """Builds native member lists and one divergence matrix per category."""

        def curves(layout: bpy.types.UILayout, owner: Owner, key: str) -> None:
            if isinstance(owner, bpy.types.ColorManagedViewSettings):
                if owner.use_curve_mapping:
                    layout.template_curve_mapping(owner, key, type="COLOR", levels=True)
            else:
                if isinstance(owner, bpy.types.RenderSettings):
                    layout.active = owner.use_motion_blur
                layout.template_curve_mapping(owner, key)

        remove = OperatorButton(target=Acquired(key="render_set_remove"))
        across = next(field for field in msgspec.structs.fields(Sets) if field.name == "across")
        setup = self.setup.map(lambda setup: setup.bound(members).map(Some)).default_value(Ok(Nothing))
        return setup.map(
            lambda setup: Single(
                key=self.key,
                title=self.title,
                header=tuple(browser for browser, _ in setup.to_list()),
                rows=(
                    Property(target=Member(root=bpy.types.WindowManager, path=f"{members.sets.name}.{across.name}")),
                    ItemList[SceneMember, bpy.types.Scene](
                        key="render_set_scenes",
                        item=bpy.types.Scene,
                        collection=Member(root=bpy.types.BlendData, path="scenes"),
                        active=Member(root=bpy.types.Window, path="scene"),
                        attachment=Some(self.scenes),
                        where=Some(
                            lambda context, item: members.identity.reports.reported(
                                lambda: members.form(context).bind(lambda form: members.copies(context).map(lambda scenes: item in scenes) if isinstance(form, Copies) else Ok(False))
                            )().default_value(False)
                        ),
                        side=(remove,),
                    ),
                    ItemList[LayerMember, bpy.types.ViewLayer](
                        key="render_set_layers",
                        item=bpy.types.ViewLayer,
                        collection=Member(root=bpy.types.Scene, path="view_layers"),
                        active=Member(root=bpy.types.Window, path="view_layer"),
                        attachment=Some(self.layers),
                        where=Some(lambda context, _item: members.identity.reports.reported(members.form)(context).filter(lambda form: isinstance(form, Layers)).is_some()),
                    ),
                    *(OperatorButton(target=placed) for placed in commands),
                    *(
                        Single(
                            key=category.key,
                            title=category.title,
                            header=(Pulldown(menu=self.captured(members, category).key), OperatorButton(target=Acquired(key=f"render_set_sync_{category.key}"))),
                            rows=(
                                Matrix(
                                    scope=MemberScope(members=members, owner_type=category.root, unlocked=False),
                                    settings=category.paths,
                                    compound=((bpy.types.CurveMapping, curves),),
                                    action=setup.map(lambda bound: bound[1]),
                                ),
                            ),
                        )
                        for category in self.categories
                    ),
                ),
            )
        )

    def realized(self, identity: Identity) -> BlenderResult[Drawn]:
        """Binds the native section to the current extension's stores."""
        return self.bound(identity).bind(lambda members: self.composed(members, self.commands(members)).bind(lambda section: section.realized(identity)))

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Realizes this section as a native layout-panel row."""
        return self.realized(identity).map(lambda drawn: drawn.inline)

    # --- [REGISTRATION]

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[BlenderResult[Acquisitions]]:
        """Holds deferred edit admission while retaining every native set declaration."""
        with ExitStack() as stack:

            def joined(members: Members) -> BlenderResult[Acquisitions]:
                deferred = stack.enter_context(identity.reports.deferred(partial(mirrored, members)))

                def edited(scene: bpy.types.Scene, _updates: tuple[bpy.types.DepsgraphUpdate, ...]) -> BlenderResult[None]:
                    context = bpy.context

                    def handed(record: Sets) -> None:
                        if record.across and scene == context.scene:
                            for owner in Option.of_optional(context.scene if isinstance(record.form, Copies) else context.view_layer).to_list():
                                deferred(owner)

                    match context.window_manager:
                        case bpy.types.WindowManager() as manager:
                            return members.sets.read(manager).map(handed)
                        case None:
                            return Error((BlenderAbsentFault(owner="bpy.context", key="window_manager"),))

                commands = self.commands(members)
                return self.composed(members, commands).map(
                    lambda section: (
                        self.ownership,
                        self.listed(members),
                        section,
                        UpdateHandler(handlers=bpy.app.handlers.depsgraph_update_post, ids=(bpy.types.Scene,), body=edited),
                        Menu(
                            key=self.menu,
                            title=self.title,
                            description="Create, synchronize, restore, and convert render sets",
                            body=Dropdown(entries=(*(OperatorButton(target=placed) for placed in commands), *(Pulldown(menu=self.captured(members, category).key) for category in self.categories))),
                        ),
                        *(PresetMenu(source=self.captured(members, category)) for category in self.categories),
                    )
                )

            yield self.bound(identity).bind(joined)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [TRANSFERS]


def transfers(
    members: Members, category: Category, context: bpy.types.Context, targets: tuple[bpy.types.Scene | bpy.types.ViewLayer, ...]
) -> BlenderResult[tuple[tuple[bpy.types.Scene | bpy.types.ViewLayer, Snapshot, tuple[Reparametrized[Value], ...]], ...]]:
    """Captures the source category and every target before deriving native value edits."""
    selected = next(selected for held, selected in members.categories if held == category)
    return members.captured(category, context).bind(
        lambda source: accumulated(Snapshot.capture(target, Some(selected)).map(lambda held: (target, held, transfer(held.tree, source.tree))) for target in targets)
    )


def previewed(members: Members, category: Category, context: bpy.types.Context) -> BlenderResult[tuple[tuple[str, tuple[Reparametrized[Value], ...]], ...]]:
    """Returns changed native values per unlocked owner for a confirmation dialog."""
    return (
        MemberScope(members=members, owner_type=category.root, unlocked=True)
        .owners(context)
        .bind(lambda targets: transfers(members, category, context, targets))
        .map(lambda rows: tuple((target.name, edits) for target, _, edits in rows if edits))
    )


def written(members: Members, category: Category, context: bpy.types.Context, targets: tuple[bpy.types.Scene | bpy.types.ViewLayer, ...]) -> BlenderResult[tuple[Applied, ...]]:
    """Publishes native priors before sync and retains genuine normalized endpoints."""

    def previous(target: bpy.types.Scene | bpy.types.ViewLayer) -> BlenderResult[Option[tuple[Tree[NativeValue], Tree[NativeValue]]]]:
        return members.recovery.read(target).bind(
            lambda held: held.map(
                lambda pair: paired(
                    Snapshot.decoded(pair[0]),
                    Result[Tree[tuple[tuple[object, str], object]], BlenderFaults].bind(
                        Option.of_optional(pair[1]).to_result_with(lambda: (BlenderAbsentFault(owner=repr(target), key="render_set_sync.after"),)), Snapshot.decoded
                    ),
                ).map(Some)
            ).default_value(Ok(Nothing))
        )

    def recorded(target: bpy.types.Scene | bpy.types.ViewLayer, old: Tree[NativeValue], before: Tree[NativeValue], after: Tree[NativeValue]) -> BlenderResult[None]:
        names = {key: frozenset(owner.values) for key, owner in old.owners.items()}
        for change in transfer(before, after):
            names[change.key] = names.get(change.key, frozenset()) | change.before.keys() | change.after.keys()
        owners = before.owners | {key: msgspec.structs.replace(owner, values=before.owners[key].values | owner.values) if key in before.owners else owner for key, owner in old.owners.items()}
        prior = {key: msgspec.structs.replace(owner, values={name: value for name, value in owner.values.items() if name in names[key]}) for key, owner in owners.items() if key in names}
        latest = {key: msgspec.structs.replace(owner, values={name: value for name, value in owner.values.items() if name in names[key]}) for key, owner in after.owners.items() if key in names}
        return paired(Snapshot.persisted(Tree(roots=tuple(prior), owners=prior)), Snapshot.persisted(Tree(roots=tuple(latest), owners=latest))).bind(lambda pair: members.recovery.write(target, pair))

    def applied(source: Snapshot, held: tuple[Option[tuple[Tree[NativeValue], Tree[NativeValue]]], ...]) -> BlenderResult[tuple[Applied, ...]]:
        retained = dict(zip(targets, held))

        def published(target: bpy.types.Scene | bpy.types.ViewLayer, before: Tree[NativeValue]) -> BlenderResult[None]:
            old = retained[target].map(lambda pair: pair[0]).default_value(Tree(roots=(), owners={}))
            owners = before.owners | {key: msgspec.structs.replace(owner, values=before.owners[key].values | owner.values) if key in before.owners else owner for key, owner in old.owners.items()}
            return paired(Snapshot.persisted(Tree(roots=tuple(owners), owners=owners)), retained[target].map(lambda pair: Snapshot.persisted(pair[1])).default_value(Ok(None))).bind(
                lambda pair: members.recovery.write(target, pair)
            )

        def completed(outcomes: tuple[Applied, ...]) -> BlenderResult[tuple[Applied, ...]]:
            return accumulated(
                paired(
                    outcome.outcome,
                    outcome.restoration.map(
                        lambda pair: pair[1].bind(partial(recorded, target, retained[target].map(lambda previous: previous[0]).default_value(Tree(roots=(), owners={})), pair[0]))
                    ).default_value(Ok(None)),
                ).map(lambda _: outcome)
                for target, outcome in zip(targets, outcomes)
            )

        return source.apply_all(targets, restoration=Some(published)).bind(completed)

    scenes = tuple(dict.fromkeys(scene for target in targets if isinstance(target, bpy.types.ViewLayer) for scene in (target.id_data,) if isinstance(scene, bpy.types.Scene)))
    return accumulated(passes.synced(scene, members.identity) for scene in scenes).bind(
        lambda _: paired(members.captured(category, context), accumulated(previous(target) for target in targets)).bind(lambda pair: applied(*pair))
    )


def synced(members: Members, category: Category, context: bpy.types.Context) -> BlenderResult[tuple[Applied, ...]]:
    """Copies one source category to every unlocked owner of its native root."""
    return MemberScope(members=members, owner_type=category.root, unlocked=True).owners(context).bind(lambda targets: written(members, category, context, targets))


def syncing(members: Members, category: Category) -> Operator[Empty]:
    """Declares a category transfer with the actual native changes in its confirmation."""

    def message(context: bpy.types.Context, _record: Empty) -> BlenderResult[str]:
        return previewed(members, category, context).map(lambda rows: f"Writes {sum(len(edit.after) for _, edits in rows for edit in edits)} {category.title} settings across {len(rows)} render sets")

    return Operator[Empty](
        key=f"render_set_sync_{category.key}",
        title=f"Sync {category.title}",
        description=f"Copy the source's {category.title} settings to every unlocked render set",
        icon=Some("FILE_REFRESH"),
        condition=Some(Setting(descriptor=members.tag, accepted=(Copies.__name__,))) if category.root is bpy.types.Scene else Nothing,
        form=Empty,
        run=lambda call: synced(members, category, call.context).map(lambda applied: Done(notes=(("INFO", f"Synced {sum(len(each.written) for each in applied)} {category.title} settings"),))),
        invoke=Some(Confirm[Empty](message=Some(message))),
    )


def toggled(members: Members, context: bpy.types.Context) -> BlenderResult[Done]:
    """Changes a native member lock, first synchronizing a never-synced unlocked member."""

    def configured(owner: bpy.types.Scene | bpy.types.ViewLayer, record: SceneMember | LayerMember) -> BlenderResult[None]:
        def synced_first(held: Option[tuple[Tree[tuple[tuple[object, str], object]], Tree[tuple[tuple[object, str], object]] | None]]) -> BlenderResult[None]:
            return (
                accumulated(
                    written(members, category, context, targets)
                    for category, _ in members.categories
                    for targets in (tuple(owner.view_layers) if isinstance(owner, bpy.types.Scene) and category.root is bpy.types.ViewLayer else (owner,) if isinstance(owner, category.root) else (),)
                    if targets
                ).map(lambda _: None)
                if held.is_none()
                else Ok(None)
            )

        return members.recovery.read(owner).bind(synced_first) if record.locked else Ok(None)

    def flipped[M: SceneMember | LayerMember](attached: Attached[M], owner: bpy.types.Scene | bpy.types.ViewLayer) -> BlenderResult[Done]:
        return attached.read(owner).bind(
            lambda record: (
                configured(owner, record)
                .bind(lambda _: attached.write(owner, msgspec.structs.replace(record, locked=not record.locked)))
                .map(lambda _: Done(notes=(("INFO", f"{'Unlocked' if record.locked else 'Locked'} {owner.name}"),)))
            )
        )

    def selected(form: Form) -> BlenderResult[Done]:
        match form:
            case Copies():
                return Option.of_optional(context.scene).map(lambda scene: flipped(members.scenes, scene)).default_with(lambda: Error((BlenderAbsentFault(owner="bpy.context", key="scene"),)))
            case Layers():
                return (
                    Option.of_optional(context.view_layer).map(lambda layer: flipped(members.layers, layer)).default_with(lambda: Error((BlenderAbsentFault(owner="bpy.context", key="view_layer"),)))
                )

    return members.form(context).bind(selected)


def unsynced(members: Members, context: bpy.types.Context) -> BlenderResult[tuple[Applied, ...]]:
    """Restores genuine endpoints conditionally and retains failed recovery records."""

    def reverted(root: type[bpy.types.Scene] | type[bpy.types.ViewLayer]) -> BlenderResult[tuple[Applied, ...]]:
        def undone(owner: bpy.types.Scene | bpy.types.ViewLayer) -> BlenderResult[Option[Applied]]:
            def restored(stored: tuple[Tree[tuple[tuple[object, str], object]], Tree[tuple[tuple[object, str], object]] | None]) -> BlenderResult[Option[Applied]]:
                return paired(
                    Snapshot.decoded(stored[0]),
                    Result[Tree[tuple[tuple[object, str], object]], BlenderFaults].bind(
                        Option.of_optional(stored[1]).to_result_with(lambda: (BlenderAbsentFault(owner=repr(owner), key="render_set_sync.after"),)), Snapshot.decoded
                    ),
                ).bind(lambda pair: (applied := Snapshot.restore(*pair)).outcome.bind(lambda _: members.recovery.removed(owner)).map(lambda _: Some(applied)))

            return members.recovery.read(owner).bind(lambda held: held.map(restored).default_value(Ok(Nothing)))

        return (
            MemberScope(members=members, owner_type=root, unlocked=True)
            .owners(context)
            .bind(lambda owners: accumulated(map(undone, owners)))
            .map(lambda outcomes: tuple(applied for outcome in outcomes for applied in outcome.to_list()))
        )

    return accumulated(map(reverted, dict.fromkeys(category.root for category, _ in members.categories))).map(lambda groups: tuple(applied for group in groups for applied in group))


def mirrored(members: Members, edited: bpy.types.Scene | bpy.types.ViewLayer) -> BlenderResult[Option[str]]:
    """Transfers native category edits to other unlocked members as one undo step."""

    def wrote(others: tuple[bpy.types.Scene | bpy.types.ViewLayer, ...]) -> BlenderResult[Option[str]]:
        return accumulated(
            Snapshot.capture(edited, Some(selected)).bind(lambda source: accumulated((applied := source.apply(other)).outcome.map(lambda _: applied) for other in others if other != edited))
            for category, selected in members.categories
            if isinstance(edited, category.root)
        ).map(lambda outcomes: Some("Edit Across Render Sets") if any(applied.written for group in outcomes for applied in group) else Nothing)

    return members.unlocked(bpy.context).bind(wrote)


def added(members: Members, source: bpy.types.Scene, name: str) -> BlenderResult[tuple[bpy.types.Scene, Option[BlenderFaults]]]:
    """Makes a retained native scene member, preserving Blender's suffixed name."""

    def renamed(scene: bpy.types.Scene) -> BlenderResult[tuple[bpy.types.Scene, Option[BlenderFaults]]]:
        def collided(faults: BlenderFaults) -> BlenderResult[tuple[bpy.types.Scene, Option[BlenderFaults]]]:
            match faults:
                case (BlenderCollisionFault(),):
                    return Ok((scene, Some(faults)))
                case _:
                    return Error(faults)

        return named(scene, name).map(lambda native: (native, Nothing)).or_else_with(collided)

    return RenderSet.ownership.made(members.identity, lambda: Option.of_optional(source.copy()).to_result_with(lambda: (BlenderAbsentFault(owner=repr(source), key="copy"),)), of=Some(source)).bind(
        renamed
    )


def converted(members: Members, form: Form, context: bpy.types.Context) -> BlenderResult[tuple[str, ...]]:
    """Converts native members, releasing newly created owners if preparation refuses."""

    def switched(manager: bpy.types.WindowManager, current: Sets) -> BlenderResult[tuple[str, ...]]:
        made: list[tuple[bpy.types.Scene, bpy.types.ViewLayer] | bpy.types.Scene] = []

        def removed(owner: tuple[bpy.types.Scene, bpy.types.ViewLayer] | bpy.types.Scene) -> BlenderResult[None]:
            return catch(lambda: owner[0].view_layers.remove(owner[1]) if isinstance(owner, tuple) else bpy.data.scenes.remove(owner), exception=RuntimeError)().map_error(
                lambda error: (BlenderEditFault(request=repr(owner), error=error),)
            )

        def refused(faults: BlenderFaults) -> BlenderResult[tuple[str, ...]]:
            return paired(Error(faults), accumulated(map(removed, reversed(made)))).map(lambda _: ())

        def committed(names: tuple[str, ...], old: tuple[bpy.types.Scene, ...] = ()) -> BlenderResult[tuple[str, ...]]:
            def finished(_: None) -> BlenderResult[tuple[str, ...]]:
                source = members.source(context)

                def shown(scene: bpy.types.Scene) -> BlenderResult[tuple[str, ...]]:
                    for window in manager.windows:
                        if window.scene in old:
                            window.scene = scene
                    return accumulated(map(removed, old)).map(lambda _: names)

                return source.bind(shown) if old else Ok(names)

            return members.sets.write(manager, msgspec.structs.replace(current, form=form)).or_else_with(lambda faults: refused(faults).map(lambda _: None)).bind(finished)

        def layered(scenes: tuple[bpy.types.Scene, ...]) -> BlenderResult[tuple[str, ...]]:
            source, *copies = scenes
            planned = tuple(
                (scene, layer, scene.name if len(used) == 1 else f"{scene.name} {layer.name}")
                for scene in copies
                for used in (tuple(layer for layer in scene.view_layers if layer.use),)
                for layer in used
            )

            def placed(scene: bpy.types.Scene, layer: bpy.types.ViewLayer, name: str) -> BlenderResult[tuple[bpy.types.Scene, bpy.types.ViewLayer, bpy.types.ViewLayer]]:
                def created() -> bpy.types.ViewLayer:
                    target = source.view_layers.new(name)
                    made.append((source, target))
                    return target

                created_layer: BlenderResult[bpy.types.ViewLayer] = catch(created, exception=RuntimeError)().map_error(
                    lambda error: (BlenderEditFault(request=f"{source!r}.view_layers.new({name!r})", error=error),)
                )
                return Result[bpy.types.ViewLayer, BlenderFaults].bind(
                    created_layer, lambda target: Ok((scene, layer, target)) if target.name == name else Error((BlenderCollisionFault(request=name, error=target.name),))
                )

            def filled(scene: bpy.types.Scene, layer: bpy.types.ViewLayer, target: bpy.types.ViewLayer) -> BlenderResult[str]:
                def sampled(pair: tuple[Snapshot, int]) -> BlenderResult[str]:
                    return (
                        pair[0]
                        .apply(target)
                        .outcome.bind(lambda _: Descriptor.resolve(bpy.types.ViewLayer, "samples", bpy.types.IntProperty).bind(lambda descriptor: descriptor.write(target, pair[1])))
                        .map(lambda _: target.name)
                    )

                return paired(Snapshot.capture(layer), members.samples_of(scene, layer)).bind(sampled)

            prepared = accumulated(starmap(placed, planned)).bind(lambda targets: passes.synced(source, members.identity).bind(lambda _: accumulated(starmap(filled, targets))))
            return prepared.or_else_with(refused).bind(lambda names: committed(names, tuple(copies)))

        def copied(source: bpy.types.Scene) -> BlenderResult[tuple[str, ...]]:
            def each(name: str) -> BlenderResult[str]:
                def retained(pair: tuple[bpy.types.Scene, Option[BlenderFaults]]) -> BlenderResult[str]:
                    scene, collision = pair
                    made.append(scene)
                    trimmed: BlenderResult[tuple[None, ...]] = accumulated(
                        catch(lambda: scene.view_layers.remove(layer), exception=RuntimeError)().map_error(lambda error: (BlenderEditFault(request=repr(layer), error=error),))
                        for layer in tuple(scene.view_layers)
                        if layer.name != name
                    )
                    named_scene: BlenderResult[None] = collision.map(Error).default_value(Ok(None))
                    return paired(trimmed, named_scene).map(lambda _: scene.name)

                return added(members, source, name).bind(retained)

            return accumulated(each(layer.name) for layer in tuple(source.view_layers) if layer.use).or_else_with(refused).bind(committed)

        match current.form, form:
            case Copies(), Layers():
                return members.copies(context).bind(layered)
            case Layers(), Copies():
                return members.source(context).bind(copied)
            case _:
                return Ok(())

    manager: BlenderResult[bpy.types.WindowManager] = Option.of_optional(context.window_manager).to_result_with(lambda: (BlenderAbsentFault(owner="bpy.context", key="window_manager"),))
    return Result[bpy.types.WindowManager, BlenderFaults].bind(manager, lambda manager: members.sets.read(manager).bind(lambda current: switched(manager, current)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Category",
    "Converting",
    "Copies",
    "Form",
    "LayerMember",
    "Layers",
    "MemberScope",
    "Members",
    "RenderSet",
    "SceneMember",
    "Sets",
    "Setup",
    "SetupEffectLock",
    "SetupLock",
    "SetupLocks",
    "SetupScope",
    "Setups",
    "added",
    "converted",
    "mirrored",
    "previewed",
    "synced",
    "syncing",
    "toggled",
    "transfers",
    "unsynced",
    "written",
]
