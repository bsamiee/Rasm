"""Render sets with category sync and edits across members."""

from collections.abc import Callable, Generator, Hashable
from contextlib import contextmanager, ExitStack
from functools import partial
from itertools import chain, starmap
from pathlib import Path
from typing import Annotated, ClassVar

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.collections import Map
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt

from blender.bpy.commands.operators import answered, Call, Confirm, Done, Empty, Operator, Outcome, Own, Suppressible, tallied, Unchanged
from blender.bpy.data.ids.ownership import Mark, Ownership, Saved
from blender.bpy.data.ids.store import named, vacant
from blender.bpy.data.rna.conditions import Condition, Setting
from blender.bpy.data.rna.descriptors import Descriptor, Native, rooted, struct_at, Value
from blender.bpy.data.rna.snapshots import Applied, Endpoints, NativeValue, Snapshot
from blender.bpy.extension.events.handlers import UpdateHandler
from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.extension.registration import Acquisitions, Band, Registrable
from blender.bpy.faults import BlenderAbsentFault, BlenderEditFault, BlenderFaults, BlenderReadFault, BlenderResult
from blender.bpy.render.effects.stack import Configured, EffectName, EffectStack, Named, projected, viewed
from blender.bpy.render.previews.queue import Job, PreviewQueue, Rank
from blender.bpy.render.previews.scenes import Chain, PreviewEngine
from blender.bpy.render.previews.tiles import Tiles
from blender.bpy.render.view_layers import passes
from blender.bpy.settings.attachments import Attached, Attachment, endpoints, Host, Stored
from blender.bpy.settings.presets import Preset
from blender.bpy.settings.properties import Changed, Choices
from blender.bpy.settings.texts import TextRecord
from blender.bpy.ui.layout.lists import ItemList
from blender.bpy.ui.layout.rows import Line, Matrix, Member, OperatorButton, Property, Pulldown, realized, RowContent, RowDraw
from blender.bpy.ui.layout.sections import Drawn, hosted, Listing, Single, Verb
from blender.bpy.ui.layout.templates import IconView
from blender.bpy.ui.resources.previews import Drawn as Finished, Previews
from blender.bpy.ui.surfaces.menus import Captured, Dropdown, Menu, PresetMenu
from blender.imaging.chain import Effect
from blender.imaging.color.processors import DisplayView
from blender.imaging.faults import accumulated, paired
from blender.imaging.hashing import content_hash, ContentHash
from blender.imaging.parameters import Case
from blender.rendering.edits import Reparametrized, transfer, Tree

# --- [MODELS] ---------------------------------------------------------------------------


class Copies(Case, frozen=True, tag=1):
    """Scenes copied from source scene."""

    title = "Scenes"


class Layers(Case, frozen=True, tag=2):
    """View layers of one scene."""

    title = "View Layers"


type Form = Copies | Layers


class Sets(msgspec.Struct, frozen=True, kw_only=True):
    """File render set settings."""

    form: Annotated[Form, msgspec.Meta(title="Render Sets", description="Copied scenes or view layers of one scene")] = Layers()
    across: Annotated[bool, msgspec.Meta(title="Edit Across", description="Copy category edits to unlocked members")] = False


class SceneMember(msgspec.Struct, frozen=True, kw_only=True):
    """Sync lock and render inclusion of member scene."""

    locked: Annotated[bool, msgspec.Meta(title="Locked")] = False
    rendered: Annotated[bool, msgspec.Meta(title="Render")] = True


class LayerMember(msgspec.Struct, frozen=True, kw_only=True):
    """Sync lock of member view layer."""

    locked: Annotated[bool, msgspec.Meta(title="Locked")] = False


class Category(msgspec.Struct, frozen=True, kw_only=True):
    """Setting paths of one category."""

    key: Key
    paths: tuple[str, ...]
    root: type[bpy.types.Scene] | type[bpy.types.ViewLayer] = bpy.types.Scene

    @property
    def title(self) -> str:
        """Display name of category key."""
        return bpy.path.display_name(self.key)


class Converting(msgspec.Struct, frozen=True, kw_only=True):
    """Target form of render set conversion."""

    form: Annotated[Form, msgspec.Meta(title="Render Sets")] = Copies()


class Setup[E: Effect](msgspec.Struct, frozen=True, kw_only=True):
    """Sparse settings and ordered owned compositor entries."""

    scene: Snapshot
    layers: dict[str, Snapshot]
    effects: dict[EffectName, Configured[E | DisplayView]]

    def projected(self, scene: bpy.types.Scene, current: "Setup[E]", locks: "SetupLocks", layers: tuple[bpy.types.ViewLayer, ...]) -> "BlenderResult[tuple[Setup[E], Setup[E]]]":
        """Returns sparse writes and projected setup of recipe over current values."""
        return paired(
            self.scene.projected(scene, current.scene, locks.scene),
            accumulated(
                self.layers[layer.name].projected(layer, current.layers[layer.name], locks.layers.get(layer.name, frozenset())).map(lambda pair: (layer.name, pair))
                for layer in layers
                if layer.name in self.layers
            ),
        ).map(
            lambda values: (
                msgspec.structs.replace(self, scene=values[0][0], layers={name: pair[0] for name, pair in values[1]}),
                Setup(
                    scene=values[0][1], layers=current.layers | {name: pair[1] for name, pair in values[1]}, effects=projected(current.effects, self.effects, locked=locks.effects, fields=locks.fields)
                ),
            )
        )


class SetupLocks(msgspec.Struct, frozen=True, kw_only=True):
    """Setting paths, entries, and effect fields setups leave unchanged."""

    scene: frozenset[str] = frozenset()
    layers: dict[str, frozenset[str]] = msgspec.field(default_factory=dict)
    effects: frozenset[EffectName] = frozenset()
    fields: dict[EffectName, frozenset[tuple[Key | int, ...]]] = msgspec.field(default_factory=dict)

    @staticmethod
    def bound(identity: Identity) -> BlenderResult[Stored["SetupLocks"]]:
        """Binds setup lock store."""
        return identity.name(Name.ATTACHMENT, "render_setup_locks").map(lambda name: Stored(name=name, decoder=msgspec.msgpack.Decoder(SetupLocks)))

    @staticmethod
    def effect_actions() -> Callable[[Option[tuple[Key | int, ...]]], RowContent]:
        """Returns factory of lock operator and button per entry or parameter path."""

        def changed(call: Call[SetupEffectLock]) -> BlenderResult[Done]:
            def decoded(text: str) -> BlenderResult[tuple[Key | int, ...]]:
                return catch(lambda: msgspec.json.decode(text, type=tuple[str | int, ...]), exception=msgspec.DecodeError)().map_error(lambda error: (BlenderReadFault(request=text, error=error),))

            path: BlenderResult[tuple[Key | int, ...] | None] = Ok(None) if (text := call.record.path) is None else decoded(text)

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

        command = Operator(key="render_setup_effect_lock", title="Lock Setup Effect", description="Toggle entry or parameter lock against setup changes", form=SetupEffectLock, run=changed, undo=True)

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
                                        target=Own(command=command, values=SetupEffectLock(name=holder.name, path=path.map(lambda keys: msgspec.json.encode(keys).decode()).to_optional())),
                                        text=Some(""),
                                        icon=Some("LOCKED" if path.map(lambda path: path in locks.fields.get(holder.name, frozenset())).default_value(holder.name in locks.effects) else "UNLOCKED"),
                                        emboss=False,
                                    )
                                    .drawn(identity)
                                    .map(lambda draw: draw(layout, holder, context))
                                )
                            )
                        )
                    )().to_optional()

            return SetupLocks.bound(identity).map(lambda store: partial(shown, store))

        return lambda path: (command, partial(drawn, path))


class SetupLock(msgspec.Struct, frozen=True, kw_only=True):
    """Setting path a matrix cell lock button toggles."""

    path: str


class SetupEffectLock(Named, frozen=True, kw_only=True):
    """Entry and JSON parameter path, absent path locking whole entry."""

    path: str | None


class SetupPick(msgspec.Struct, frozen=True, kw_only=True):
    """Setup preset a preview picker applies."""

    setup: Annotated[str, msgspec.Meta(title="Setup", description="Render setup preset to apply to unlocked members")] = ""


class RenderSetCommands(msgspec.Struct, frozen=True, kw_only=True):
    """Render set operators, one sync per category."""

    add: Operator[Empty]
    adopt: Operator[Empty]
    restore: Operator[Empty]
    syncs: tuple[Operator[Empty], ...]
    unsync: Operator[Empty]
    convert: Operator[Converting]
    remove: Operator[Empty]
    lock: Operator[Empty]

    @property
    def listed(self) -> tuple[Operator[Empty] | Operator[Converting], ...]:
        """Commands in menu and section order."""
        return self.add, self.adopt, self.restore, *self.syncs, self.unsync, self.convert


class Members(msgspec.Struct, frozen=True, kw_only=True):
    """Stores and resolved descriptors every set operation reads."""

    identity: Identity
    sets: Attached[Sets]
    tag: Descriptor[bpy.types.EnumProperty]
    scenes: Attached[SceneMember]
    layers: Attached[LayerMember]
    categories: tuple[tuple[Category, tuple[Descriptor, ...]], ...]
    samples: Map[str, Descriptor[bpy.types.IntProperty]]
    recovery: Stored[Endpoints]

    # --- [MEMBERSHIP]

    def form(self, context: bpy.types.Context) -> BlenderResult[Form]:
        """Reads file render set form from window manager."""
        match context.window_manager:
            case bpy.types.WindowManager() as manager:
                return self.sets.read(manager).map(lambda record: record.form)
            case None:
                return Error((BlenderAbsentFault(owner="bpy.context", key="window_manager"),))

    def source(self, context: bpy.types.Context) -> BlenderResult[bpy.types.Scene]:
        """Returns marked source of active scene, else active scene."""
        match context.scene:
            case bpy.types.Scene() as scene:
                match RenderSet.ownership.marked(self.identity, scene).bind(lambda mark: mark.of).to_optional():
                    case bpy.types.Scene() as source:
                        return Ok(source)
                    case _:
                        return Ok(scene)
            case None:
                return Error((BlenderAbsentFault(owner="bpy.context", key="scene"),))

    def copies(self, context: bpy.types.Context) -> BlenderResult[tuple[bpy.types.Scene, ...]]:
        """Returns source, then its member scenes in data order."""
        return self.source(context).map(lambda source: (source, *RenderSet.ownership.serving(self.identity, source)))

    def members(self, context: bpy.types.Context) -> BlenderResult[tuple[bpy.types.Scene | bpy.types.ViewLayer, ...]]:
        """Returns members of file form in list order."""

        def listed(form: Form) -> BlenderResult[tuple[bpy.types.Scene | bpy.types.ViewLayer, ...]]:
            match form:
                case Copies():
                    return self.copies(context)
                case Layers():
                    return self.source(context).map(lambda scene: tuple(scene.view_layers))

        return self.form(context).bind(listed)

    def free[O: bpy.types.Scene | bpy.types.ViewLayer](self, owners: tuple[O, ...]) -> BlenderResult[tuple[O, ...]]:
        """Returns unlocked owners in given order."""

        def locked(owner: bpy.types.Scene | bpy.types.ViewLayer) -> BlenderResult[bool]:
            return (self.scenes.read(owner) if isinstance(owner, bpy.types.Scene) else self.layers.read(owner)).map(lambda member: member.locked)

        return accumulated(map(locked, owners)).map(lambda locks: tuple(owner for owner, lock in zip(owners, locks) if not lock))

    def unlocked(self, context: bpy.types.Context) -> BlenderResult[tuple[bpy.types.Scene | bpy.types.ViewLayer, ...]]:
        """Returns unlocked members of file form."""
        return self.members(context).bind(self.free)

    def rendered(self, context: bpy.types.Context) -> BlenderResult[tuple[tuple[bpy.types.Scene, Option[bpy.types.ViewLayer]], ...]]:
        """Returns dispatch scenes, paired with each used layer under layer form."""

        def dispatched(form: Form) -> BlenderResult[tuple[tuple[bpy.types.Scene, Option[bpy.types.ViewLayer]], ...]]:
            match form:
                case Copies():
                    return self.copies(context).bind(
                        lambda scenes: accumulated(self.scenes.read(scene) for scene in scenes).map(
                            lambda records: tuple((scene, Nothing) for scene, record in zip(scenes, records) if record.rendered)
                        )
                    )
                case Layers():
                    return self.source(context).map(lambda scene: tuple((scene, Some(layer)) for layer in scene.view_layers if layer.use))

        return self.form(context).bind(dispatched)

    # --- [SETTINGS]

    def captured(self, category: Category, context: bpy.types.Context) -> BlenderResult[Snapshot]:
        """Captures category from source scene or its layer named like active one."""
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
        """Reads layer sample override, else engine samples."""
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
    """Owners selected by set form and sync locks."""

    members: Members
    owner_type: type[bpy.types.Scene] | type[bpy.types.ViewLayer]
    unlocked: bool

    def owners(self, context: bpy.types.Context) -> BlenderResult[tuple[bpy.types.Scene | bpy.types.ViewLayer, ...]]:
        """Returns owners, unlocked ones alone when `unlocked` is set."""
        return (
            self
            .scenes(context)
            .map(lambda scenes: tuple(layer for scene in scenes for layer in scene.view_layers) if self.owner_type is bpy.types.ViewLayer else scenes)
            .bind(lambda owners: self.members.free(owners) if self.unlocked else Ok(owners))
        )

    def scenes(self, context: bpy.types.Context) -> BlenderResult[tuple[bpy.types.Scene, ...]]:
        """Returns member scenes under copy form, else source scene."""
        return self.members.form(context).bind(lambda form: self.members.copies(context) if isinstance(form, Copies) else self.members.source(context).map(lambda scene: (scene,)))


# --- [SERVICES] -------------------------------------------------------------------------


class SetupScope[E: Effect](msgspec.Struct, frozen=True, kw_only=True):
    """Setup recipes and lock store of one render set."""

    members: Members
    stack: EffectStack[E]
    locks: Stored[SetupLocks]

    @classmethod
    def bound(cls, members: Members, stack: EffectStack[E]) -> BlenderResult["SetupScope[E]"]:
        """Binds setup lock store of members identity."""
        return SetupLocks.bound(members.identity).map(lambda locks: cls(members=members, stack=stack, locks=locks))

    def captured(self, scene: bpy.types.Scene) -> BlenderResult[Setup[E]]:
        """Captures category settings and ordered effects of scene."""
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
        """Captures active scene, substituting no source."""
        scene = rooted(bpy.types.Scene, context)
        return scene.bind(self.captured)

    def held(self, scene: bpy.types.Scene) -> BlenderResult[tuple[SetupLocks, tuple[bpy.types.ViewLayer, ...]]]:
        """Reads setup locks and unlocked view layers of scene."""
        return paired(
            self.locks.read(scene).map(lambda held: held.default_with(SetupLocks)),
            accumulated(self.members.layers.read(layer).map(lambda member: (layer, member.locked)) for layer in scene.view_layers).map(
                lambda rows: tuple(layer for layer, locked in rows if not locked)
            ),
        )

    def applied(self, context: bpy.types.Context, recipe: Setup[E]) -> BlenderResult[Outcome]:
        """Applies recipe to each unlocked member scene it changes."""

        def planned(scene: bpy.types.Scene) -> BlenderResult[Option[tuple[bpy.types.Scene, SetupLocks, Setup[E]]]]:
            return paired(self.captured(scene), self.held(scene)).bind(
                lambda held: recipe.projected(scene, held[0], *held[1]).map(lambda pair: Some((scene, held[1][0], pair[0])) if pair[1] != held[0] else Nothing)
            )

        def written(scene: bpy.types.Scene, locks: SetupLocks, sparse: Setup[E]) -> BlenderResult[None]:
            return (
                sparse.scene
                .apply(scene)
                .outcome.bind(
                    lambda _: paired(
                        accumulated(snapshot.apply(scene.view_layers[name]).outcome for name, snapshot in sparse.layers.items()),
                        self.stack.applied(self.members.identity, scene, sparse.effects, locked=locks.effects, fields=locks.fields),
                    )
                )
                .map(lambda _: None)
            )

        def wrote(plans: tuple[Option[tuple[bpy.types.Scene, SetupLocks, Setup[E]]], ...]) -> BlenderResult[Outcome]:
            match tuple(plan for held in plans for plan in held.to_list()), plans:
                case (), ():
                    return Ok(Unchanged(reason="Every render set member is locked"))
                case (), _:
                    return Ok(Unchanged(reason="Unlocked render set members match setup"))
                case changing, _:
                    return tallied(tuple(starmap(written, changing)))

        return MemberScope(members=self.members, owner_type=bpy.types.Scene, unlocked=True).scenes(context).bind(self.members.free).bind(lambda scenes: accumulated(map(planned, scenes))).bind(wrote)

    def candidate(self, _identity: Identity, context: bpy.types.Context, recipe: Setup[E], head: Option[npt.NDArray[np.float32]]) -> BlenderResult[Chain]:
        """Projects recipe effects over completed render for preview."""
        match context.scene, head:
            case bpy.types.Scene() as scene, Option(tag="some"):
                return (
                    paired(self.captured(scene), self.held(scene))
                    .bind(lambda held: recipe.projected(scene, held[0], *held[1]))
                    .map(lambda pair: Chain(head=head, offered=pair[1].effects, initial_view=Some(viewed(scene))))
                )
            case None, _:
                return Error((BlenderAbsentFault(owner="bpy.context", key="scene"),))
            case _, Option():
                return Error((BlenderAbsentFault(owner="render setup preview", key="completed render"),))

    def observed(self, context: bpy.types.Context) -> BlenderResult[tuple[Hashable, Callable[[tuple[Setup[E], ...]], BlenderResult[tuple[bool, ...]]]]]:
        """Returns revision key and recipe matcher from one capture."""
        match context.scene:
            case bpy.types.Scene() as scene:
                return paired(self.captured(scene), self.held(scene)).map(
                    lambda held: (
                        (content_hash(SetupScope, held[0], held[1][0]), tuple(layer.name for layer in held[1][1])),
                        lambda recipes: accumulated(recipe.projected(scene, held[0], *held[1]).map(lambda pair: pair[1] == held[0]) for recipe in recipes),
                    )
                )
            case None:
                return Error((BlenderAbsentFault(owner="bpy.context", key="scene"),))

    def actions(self) -> BlenderResult[tuple[Operator[SetupLock], Callable[[Identity, Descriptor], BlenderResult[RowDraw]]]]:
        """Returns setting lock operator and matrix cell button factory."""

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
            description="Toggle setting lock against setup changes",
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
                identity.reports.reported(lambda: selected.map(lambda locks: draws[descriptor.path in locks](layout, holder, context)))().to_optional()

            return accumulated(
                OperatorButton(target=Own(command=command, values=SetupLock(path=descriptor.path)), text=Some(""), icon=Some("LOCKED" if locked else "UNLOCKED"), emboss=False)
                .drawn(identity)
                .map(lambda draw: (locked, draw))
                for locked in (False, True)
            ).map(lambda draws: partial(shown, dict(draws)))

        return name.map(lambda name: (command, partial(action, name)))


class SetupGallery[E: Effect](Line, frozen=True, kw_only=True):
    """Setup presets as preview icons over the completed render, a pick applying one."""

    scope: SetupScope[E]
    preset: Preset[Setup[E]]
    tiles: Tiles
    scale: float
    seen: ClassVar[set[str]] = set()

    @property
    def listing(self) -> Listing[Setup[E]]:
        """Preset actions with matches against the active scene."""
        return Listing(
            preset=self.preset, current=self.scope.current, matches=lambda context, recipes: self.scope.observed(context).bind(lambda observed: observed[1](recipes)), applied=self.scope.applied
        )

    @property
    def attachment(self) -> Attachment[SetupPick]:
        """Picked preset holder listing preset previews, each pick applying its preset."""
        return Attachment(
            key="render_setup_pick",
            form=SetupPick,
            owner=bpy.types.WindowManager,
            changed=Some(self.picking),
            bindings=Some({
                vars(SetupPick)["setup"]: Choices(
                    lambda identity, _owner, _context: (
                        (stem, name, tip, Previews(identity=identity).icon_id(f"{self.preset.key}_{stem}").default_value(0), index)
                        for index, (stem, name, tip) in enumerate(self.preset.entries(identity))
                    )
                )
            }),
        )

    @contextmanager
    def picking(self, identity: Identity) -> Generator[Changed]:
        """Defers one apply per pick, its undo step named by the apply verb."""

        def applied(stem: str) -> BlenderResult[Option[str]]:
            return (
                self.preset
                .found(identity, stem)
                .bind(self.preset.read)
                .bind(lambda recipe: self.scope.applied(bpy.context, recipe))
                .map(lambda outcome: Some(Verb.APPLY.title) if isinstance(outcome, Done) else Nothing)
            )

        with identity.reports.deferred(applied) as deferred:
            yield lambda owner, _context, prop: deferred(getattr(owner, prop.identifier))

    def jobs(self, identity: Identity, context: bpy.types.Context) -> BlenderResult[tuple[Job, ...]]:
        """Returns a tile job per stale preset preview after a picker draw, none before a completed render."""
        if identity.package not in SetupGallery.seen:
            return Ok(())
        SetupGallery.seen.discard(identity.package)
        previews = Previews(identity=identity)

        def rendered(scene: bpy.types.Scene, stem: str, side: int) -> BlenderResult[Option[Option[npt.NDArray[np.float32]]]]:
            return (
                self.preset
                .found(identity, stem)
                .bind(self.preset.read)
                .bind(lambda recipe: self.tiles.rendered(identity, partial(self.scope.candidate, identity, bpy.context, recipe), side, scene=scene))
                .map(lambda pixels: Some(Some(pixels)))
            )

        def queued(scene: bpy.types.Scene, revision: Hashable) -> tuple[Job, ...]:
            def job(stem: str, stored: Setup[E] | Path, proxy: ContentHash) -> tuple[Job, ...]:
                slot = f"{self.preset.key}_{stem}"
                stamp = catch(stored.stat, exception=OSError)().to_option().map(lambda status: status.st_mtime_ns) if isinstance(stored, Path) else Nothing
                key = (stem, stamp, revision, proxy)
                return (
                    ()
                    if previews.current(slot, key, self.scale)
                    else (
                        Job(
                            place=(SetupGallery, slot),
                            key=Finished.read(key, self.scale),
                            rank=Rank.SHOWN,
                            run=lambda: previews.tile(slot, key, partial(rendered, scene, stem), self.scale).map(lambda _: None),
                        ),
                    )
                )

            return tuple(each for proxy in Tiles.held[identity.package].proxy.to_list() for stem, stored in self.preset.listed(identity).items() for each in job(stem, stored, proxy.key))

        return rooted(bpy.types.Scene, context).bind(lambda scene: self.scope.observed(context).map(lambda observation: queued(scene, observation[0])))

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Acquires tile renderer, pick holder, preset menu, and preview feed."""
        return Ok((self.tiles, self.attachment, PresetMenu(source=self.listing), (Band.RESOURCES, PreviewQueue.fed(identity, self.jobs))))

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Draws preset previews, marking the picker shown for the preview feed."""
        (field,) = msgspec.structs.fields(SetupPick)

        def marked(draw: RowDraw) -> RowDraw:
            def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                SetupGallery.seen.add(identity.package)
                draw(layout, holder, context)

            return shown

        return (
            self.attachment
            .bound(identity)
            .bind(lambda attached: realized(IconView(choice=Member(root=bpy.types.WindowManager, path=f"{attached.name}.{field.name}"), scale=self.scale, show_labels=True), identity))
            .map(marked)
        )


class Setups[E: Effect](msgspec.Struct, frozen=True, kw_only=True):
    """Setup picker of render set section."""

    stack: EffectStack[E]
    tiles: Tiles
    scale: float

    def bound(self, members: Members) -> BlenderResult[tuple[SetupGallery[E], tuple[Operator[SetupLock], Callable[[Identity, Descriptor], BlenderResult[RowDraw]]]]]:
        """Builds setup picker and lock actions over members."""
        return SetupScope.bound(members, self.stack).bind(
            lambda scope: scope.actions().map(
                lambda actions: (SetupGallery(scope=scope, preset=Preset(key=RenderSet.key, form=Setup[self.stack.form]), tiles=self.tiles, scale=self.scale), actions)  # type: ignore[name-defined]  # ty: ignore[invalid-type-form]
            )
        )


class RenderSet[E: Effect = Effect](msgspec.Struct, frozen=True, kw_only=True):
    """Render set stores, commands, menus, and section."""

    key: ClassVar[Key] = "render_sets"
    title: ClassVar[str] = "Render Sets"
    description: ClassVar[str] = ""
    popovers: ClassVar[tuple[Single[Empty], ...]] = ()
    condition: ClassVar[Option[Condition]] = Nothing
    opened: ClassVar[bool] = False
    record: ClassVar[TextRecord[Sets]] = TextRecord(key="render_set", form=Sets)
    scenes: ClassVar[Attachment[SceneMember]] = Attachment(key="render_set", form=SceneMember, owner=bpy.types.Scene)
    layers: ClassVar[Attachment[LayerMember]] = Attachment(key="render_set", form=LayerMember, owner=bpy.types.ViewLayer)
    ownership: ClassVar[Ownership[bpy.types.Scene]] = Ownership(kind=bpy.types.Scene, key="render_set", saved=Saved.ALWAYS, single=False)
    menu: ClassVar[Key] = key
    categories: tuple[Category, ...] = ()
    setup: Option[Setups[E]] = Nothing

    # --- [BINDING]

    @property
    def prerequisites(self) -> tuple[Registrable, ...]:
        """Set, scene, and layer stores binding reads."""
        return self.record, self.scenes, self.layers

    def bound(self, identity: Identity) -> BlenderResult[Members]:
        """Resolves stores and descriptors."""

        def tagged(sets: Attached[Sets]) -> BlenderResult[tuple[Attached[Sets], Descriptor[bpy.types.EnumProperty]]]:
            form = next(field for field in msgspec.structs.fields(Sets) if field.name == "form")
            return Descriptor.resolve(bpy.types.WindowManager, f"{sets.name}.{form.name}.{Case.__struct_config__.tag_field}", bpy.types.EnumProperty).map(lambda tag: (sets, tag))

        stores = paired(self.record.attachment.bound(identity).bind(tagged), paired(self.scenes.bound(identity), self.layers.bound(identity)))
        selections = accumulated(
            accumulated(Descriptor.resolve(category.root, path, bpy.types.Property) for path in category.paths).map(lambda found: (category, found)) for category in self.categories
        )
        samples = accumulated(Descriptor.resolve(bpy.types.Scene, ".".join(engine.value), bpy.types.IntProperty).map(lambda descriptor: (engine.name, descriptor)) for engine in PreviewEngine)
        recovery = identity.name(Name.ATTACHMENT, "render_set_sync").map(endpoints)
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
        """Returns category preset source applying to unlocked owners."""
        return Captured(preset=Preset(key=category.key, form=Snapshot), scope=MemberScope(members=members, owner_type=category.root, unlocked=True), selection=category.paths)

    # --- [COMMANDS]

    def commands(self, members: Members) -> RenderSetCommands:
        """Returns render set operators."""

        def add(call: Call[Empty]) -> BlenderResult[Done]:
            return members.copies(call.context).bind(lambda scenes: added(members, scenes[0], f"{scenes[0].name} {len(scenes)}")).map(lambda scene: Done(notes=(("INFO", f"Added {scene.name}"),)))

        def adopt(call: Call[Empty]) -> BlenderResult[Outcome]:
            def joined(source: bpy.types.Scene) -> BlenderResult[Outcome]:
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

                if not (
                    unmarked := tuple(scene for scene in bpy.data.scenes if scene != source and scene not in sources and not scene.is_runtime_data and Mark.read(members.identity, scene).is_none())
                ):
                    return Ok(Unchanged(reason="No unmarked scene to adopt"))
                edits = tuple(map(marked, unmarked))
                return tallied(edits, tuple(("INFO", f"Adopted {scene.name}") for edit in edits for scene in edit.to_option().to_list()))

            return members.source(call.context).bind(joined)

        def restore(call: Call[Empty]) -> BlenderResult[Outcome]:
            def removed(scenes: tuple[bpy.types.Scene, ...]) -> BlenderResult[Outcome]:
                source, *copies = scenes
                if not copies:
                    return Ok(Unchanged(reason=f"{source.name} holds no copied render set"))
                for window in (window for manager in bpy.data.window_managers for window in manager.windows):
                    if window.scene in copies:
                        window.scene = source
                edits = tuple(
                    catch(lambda: bpy.data.scenes.remove(scene), exception=RuntimeError)().map_error(lambda error: (BlenderEditFault(request=name, error=error),)).map(lambda _: name)
                    for scene in copies
                    for name in (scene.name,)
                )
                return tallied(edits, tuple(("INFO", f"Removed {name}") for edit in edits for name in edit.to_option().to_list()))

            return members.copies(call.context).bind(removed)

        def touched(context: bpy.types.Context, record: Converting) -> BlenderResult[str]:
            return members.members(context).map(lambda owners: f"Convert {', '.join(owner.name for owner in owners)} to {record.form.title}")

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

        return RenderSetCommands(
            add=Operator[Empty](key="render_set_add", title="Add Render Set", description="Copy source scene as new member", icon=Some("ADD"), form=Empty, run=add),
            adopt=Operator[Empty](key="render_set_adopt", title="Adopt Scenes", description="Add unmarked scenes to source render sets", icon=Some("IMPORT"), form=Empty, run=adopt),
            restore=Operator[Empty](key="render_set_restore", title="Restore Source", description="Remove copied members and show source scene", icon=Some("SCENE_DATA"), form=Empty, run=restore),
            syncs=tuple(syncing(members, category) for category in self.categories),
            unsync=Operator[Empty](
                key="render_set_unsync",
                title="Unsync Render Sets",
                description="Restore pre-sync settings of unlocked members",
                icon=Some("LOOP_BACK"),
                form=Empty,
                run=lambda call: unsynced(members, call.context).map(
                    lambda applied: Done(notes=(("INFO", f"Restored {len(applied)} render set members"),)) if applied else Unchanged(reason="No render set holds synced settings")
                ),
            ),
            convert=Operator[Converting](
                key="render_set_convert",
                title="Convert Render Sets",
                description="Transfer render sets between copied scenes and view layers",
                icon=Some("ARROW_LEFTRIGHT"),
                form=Converting,
                run=lambda call: converted(members, call.record.form, call.context).map(
                    lambda names: names.map(lambda made: Done(notes=tuple(("INFO", f"Created {name}") for name in made))).default_value(Unchanged(reason=f"Render sets use {call.record.form.title}"))
                ),
                invoke=Some(Confirm[Converting](message=Some(touched))),
            ),
            remove=Operator[Empty](key="render_set_remove", title="Remove Render Set", description="Remove active copied member", icon=Some("REMOVE"), form=Empty, run=remove),
            lock=Operator[Empty](
                key="render_set_lock",
                title="Lock Render Set",
                description="Toggle sync lock of active member",
                icon=Some("LOCKED"),
                form=Empty,
                run=lambda call: toggled(members, call.context),
                invoke=Some(Suppressible[Empty](message=lambda _context, _record: "Unlocking never-synced member copies every category from source")),
            ),
        )

    # --- [SECTION]

    def composed(self, members: Members, commands: RenderSetCommands) -> BlenderResult[Single]:
        """Builds member lists and one settings matrix per category."""

        def curves(layout: bpy.types.UILayout, owner: Native, key: str) -> None:
            if isinstance(owner, bpy.types.ColorManagedViewSettings):
                if owner.use_curve_mapping:
                    layout.template_curve_mapping(owner, key, type="COLOR", levels=True)
            else:
                if isinstance(owner, bpy.types.RenderSettings):
                    layout.active = owner.use_motion_blur
                layout.template_curve_mapping(owner, key)

        across = next(field for field in msgspec.structs.fields(Sets) if field.name == "across")
        setup = self.setup.map(lambda setup: setup.bound(members).map(Some)).default_value(Ok(Nothing))
        return setup.map(
            lambda setup: Single(
                key=self.key,
                title=self.title,
                header=tuple(Pulldown(menu=gallery.listing.key, text=Some(""), icon=Some("PRESET")) for gallery, _ in setup.to_list()),
                rows=(
                    *(gallery for gallery, _ in setup.to_list()),
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
                        side=tuple(OperatorButton(target=command, text=Some("")) for command in (commands.add, commands.remove, commands.lock)),
                    ),
                    ItemList[LayerMember, bpy.types.ViewLayer](
                        key="render_set_layers",
                        item=bpy.types.ViewLayer,
                        collection=Member(root=bpy.types.Scene, path="view_layers"),
                        active=Member(root=bpy.types.Window, path="view_layer"),
                        attachment=Some(self.layers),
                        where=Some(lambda context, _item: members.identity.reports.reported(members.form)(context).filter(lambda form: isinstance(form, Layers)).is_some()),
                    ),
                    *(
                        Single(
                            key=category.key,
                            title=category.title,
                            header=(Pulldown(menu=self.captured(members, category).key, text=Some(""), icon=Some("PRESET")), OperatorButton(target=sync, text=Some(""))),
                            rows=(
                                Matrix(
                                    scope=MemberScope(members=members, owner_type=category.root, unlocked=False),
                                    settings=category.paths,
                                    compound=((bpy.types.CurveMapping, curves),),
                                    action=setup.map(lambda bound: bound[1]),
                                ),
                            ),
                        )
                        for category, sync in zip(self.categories, commands.syncs)
                    ),
                ),
            )
        )

    def realized(self, identity: Identity) -> BlenderResult[Drawn]:
        """Builds section over extension stores."""
        return self.bound(identity).bind(lambda members: self.composed(members, self.commands(members)).bind(lambda section: section.realized(identity)))

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Returns section as inline layout panel draw."""
        return self.realized(identity).map(lambda drawn: drawn.inline)

    # --- [REGISTRATION]

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[BlenderResult[Acquisitions]]:
        """Holds deferred edit transfer while registered."""
        with ExitStack() as stack:

            def joined(members: Members) -> BlenderResult[Acquisitions]:
                deferred = stack.enter_context(identity.reports.deferred(partial(propagated, members)))

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
                        *commands.listed,
                        commands.remove,
                        commands.lock,
                        section,
                        UpdateHandler(handlers=bpy.app.handlers.depsgraph_update_post, ids=(bpy.types.Scene,), body=edited),
                        Menu(
                            key=self.menu,
                            title=self.title,
                            description="Create, sync, restore, and convert render sets",
                            body=Dropdown(
                                entries=(*(OperatorButton(target=placed) for placed in commands.listed), *(Pulldown(menu=self.captured(members, category).key) for category in self.categories))
                            ),
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
    """Returns value edits per target from source category."""
    selected = next(selected for held, selected in members.categories if held == category)
    return members.captured(category, context).bind(
        lambda source: accumulated(Snapshot.capture(target, Some(selected)).map(lambda held: (target, held, transfer(held.tree, source.tree))) for target in targets)
    )


def previewed(members: Members, category: Category, context: bpy.types.Context) -> BlenderResult[tuple[tuple[str, tuple[Reparametrized[Value], ...]], ...]]:
    """Returns value edits per unlocked owner for sync confirmation."""
    return (
        MemberScope(members=members, owner_type=category.root, unlocked=True)
        .owners(context)
        .bind(lambda targets: transfers(members, category, context, targets))
        .map(lambda rows: tuple((target.name, edits) for target, _, edits in rows if edits))
    )


def written(members: Members, category: Category, context: bpy.types.Context, targets: tuple[bpy.types.Scene | bpy.types.ViewLayer, ...]) -> BlenderResult[tuple[Applied, ...]]:
    """Applies source category, storing pre-sync and changed values as recovery endpoints."""

    def previous(target: bpy.types.Scene | bpy.types.ViewLayer) -> BlenderResult[Option[tuple[Tree[NativeValue], Tree[NativeValue]]]]:
        return members.recovery.read(target).bind(lambda held: held.map(lambda ends: ends.decoded(target, members.recovery.name).map(Some)).default_value(Ok(Nothing)))

    def recorded(target: bpy.types.Scene | bpy.types.ViewLayer, old: Tree[NativeValue], before: Tree[NativeValue], after: Tree[NativeValue]) -> BlenderResult[None]:
        names = {key: frozenset(owner.values) for key, owner in old.owners.items()}
        for change in transfer(before, after):
            names[change.key] = names.get(change.key, frozenset()) | change.before.keys() | change.after.keys()
        owners = before.owners | {key: msgspec.structs.replace(owner, values=before.owners[key].values | owner.values) if key in before.owners else owner for key, owner in old.owners.items()}
        prior = {key: msgspec.structs.replace(owner, values={name: value for name, value in owner.values.items() if name in names[key]}) for key, owner in owners.items() if key in names}
        latest = {key: msgspec.structs.replace(owner, values={name: value for name, value in owner.values.items() if name in names[key]}) for key, owner in after.owners.items() if key in names}
        return paired(Snapshot.persisted(Tree(roots=tuple(prior), owners=prior)), Snapshot.persisted(Tree(roots=tuple(latest), owners=latest))).bind(
            lambda pair: members.recovery.write(target, Endpoints(before=pair[0], after=pair[1]))
        )

    def applied(source: Snapshot, held: tuple[Option[tuple[Tree[NativeValue], Tree[NativeValue]]], ...]) -> BlenderResult[tuple[Applied, ...]]:
        retained = dict(zip(targets, held))

        def published(target: bpy.types.Scene | bpy.types.ViewLayer, before: Tree[NativeValue]) -> BlenderResult[None]:
            old = retained[target].map(lambda pair: pair[0]).default_value(Tree(roots=(), owners={}))
            owners = before.owners | {key: msgspec.structs.replace(owner, values=before.owners[key].values | owner.values) if key in before.owners else owner for key, owner in old.owners.items()}
            return paired(Snapshot.persisted(Tree(roots=tuple(owners), owners=owners)), retained[target].map(lambda pair: Snapshot.persisted(pair[1])).default_value(Ok(None))).bind(
                lambda pair: members.recovery.write(target, Endpoints(before=pair[0], after=pair[1]))
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


def synced(members: Members, category: Category, context: bpy.types.Context) -> BlenderResult[Outcome]:
    """Copies source category to unlocked owners when any differs."""
    reason = f"Unlocked render sets match source {category.title} settings"

    def wrote(targets: tuple[bpy.types.Scene | bpy.types.ViewLayer, ...]) -> BlenderResult[Outcome]:
        return transfers(members, category, context, targets).bind(
            lambda rows: (
                written(members, category, context, targets).bind(
                    lambda applied: answered(Applied.join(applied), reason, (("INFO", f"Synced {sum(len(each.written) for each in applied)} {category.title} settings"),))
                )
                if any(edits for _, _, edits in rows)
                else Ok(Unchanged(reason=reason))
            )
        )

    return MemberScope(members=members, owner_type=category.root, unlocked=True).owners(context).bind(wrote)


def syncing(members: Members, category: Category) -> Operator[Empty]:
    """Returns category sync operator confirming with change counts."""

    def message(context: bpy.types.Context, _record: Empty) -> BlenderResult[str]:
        return previewed(members, category, context).map(lambda rows: f"Writes {sum(len(edit.after) for _, edits in rows for edit in edits)} {category.title} settings across {len(rows)} render sets")

    return Operator[Empty](
        key=f"render_set_sync_{category.key}",
        title=f"Sync {category.title}",
        description=f"Copy source {category.title} settings to unlocked render sets",
        icon=Some("FILE_REFRESH"),
        condition=Some(Setting(descriptor=members.tag, accepted=(Copies.__name__,))) if category.root is bpy.types.Scene else Nothing,
        form=Empty,
        run=lambda call: synced(members, category, call.context),
        invoke=Some(Confirm[Empty](message=Some(message))),
    )


def toggled(members: Members, context: bpy.types.Context) -> BlenderResult[Done]:
    """Toggles active member lock, first syncing a never-synced member it unlocks."""

    def configured(owner: bpy.types.Scene | bpy.types.ViewLayer, record: SceneMember | LayerMember) -> BlenderResult[None]:
        def synced_first(held: Option[Endpoints]) -> BlenderResult[None]:
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

    def changed[M: SceneMember | LayerMember](attached: Attached[M], owner: bpy.types.Scene | bpy.types.ViewLayer) -> BlenderResult[Done]:
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
                return rooted(bpy.types.Scene, context).bind(lambda scene: changed(members.scenes, scene))
            case Layers():
                return rooted(bpy.types.ViewLayer, context).bind(lambda layer: changed(members.layers, layer))

    return members.form(context).bind(selected)


def unsynced(members: Members, context: bpy.types.Context) -> BlenderResult[tuple[Applied, ...]]:
    """Restores pre-sync values of unlocked owners, keeping records that fail to restore."""

    def reverted(root: type[bpy.types.Scene] | type[bpy.types.ViewLayer]) -> BlenderResult[tuple[Applied, ...]]:
        def undone(owner: bpy.types.Scene | bpy.types.ViewLayer) -> BlenderResult[Option[Applied]]:
            def restored(stored: Endpoints) -> BlenderResult[Option[Applied]]:
                return stored.decoded(owner, members.recovery.name).bind(
                    lambda pair: (applied := Snapshot.restore(*pair)).outcome.bind(lambda _: members.recovery.removed(owner)).map(lambda _: Some(applied))
                )

            return members.recovery.read(owner).bind(lambda held: held.map(restored).default_value(Ok(Nothing)))

        return (
            MemberScope(members=members, owner_type=root, unlocked=True)
            .owners(context)
            .bind(lambda owners: accumulated(map(undone, owners)))
            .map(lambda outcomes: tuple(applied for outcome in outcomes for applied in outcome.to_list()))
        )

    return accumulated(map(reverted, dict.fromkeys(category.root for category, _ in members.categories))).map(lambda groups: tuple(chain.from_iterable(groups)))


def propagated(members: Members, edited: bpy.types.Scene | bpy.types.ViewLayer) -> BlenderResult[Option[str]]:
    """Copies category edits to other unlocked members in one undo step."""

    def wrote(others: tuple[bpy.types.Scene | bpy.types.ViewLayer, ...]) -> BlenderResult[Option[str]]:
        return accumulated(
            Snapshot.capture(edited, Some(selected)).bind(lambda source: accumulated((applied := source.apply(other)).outcome.map(lambda _: applied) for other in others if other != edited))
            for category, selected in members.categories
            if isinstance(edited, category.root)
        ).map(lambda outcomes: Some("Edit Across Render Sets") if any(applied.written for group in outcomes for applied in group) else Nothing)

    return members.unlocked(bpy.context).bind(wrote)


def added(members: Members, source: bpy.types.Scene, name: str) -> BlenderResult[bpy.types.Scene]:
    """Copies source as retained member under the first vacant label of name."""
    return RenderSet.ownership.made(members.identity, lambda: Option.of_optional(source.copy()).to_result_with(lambda: (BlenderAbsentFault(owner=repr(source), key="copy"),)), of=Some(source)).bind(
        lambda scene: named(scene, vacant(name, (held.name for held in bpy.data.scenes if held != scene)))
    )


def converted(members: Members, form: Form, context: bpy.types.Context) -> BlenderResult[Option[tuple[str, ...]]]:
    """Converts render set form, removing created owners on failure, `Nothing` for current form."""

    def switched(manager: bpy.types.WindowManager, current: Sets) -> BlenderResult[Option[tuple[str, ...]]]:
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
                def created() -> tuple[bpy.types.Scene, bpy.types.ViewLayer, bpy.types.ViewLayer]:
                    target = source.view_layers.new(vacant(name, source.view_layers.keys()))
                    made.append((source, target))
                    return scene, layer, target

                return catch(created, exception=RuntimeError)().map_error(lambda error: (BlenderEditFault(request=f"{source!r}.view_layers.new({name!r})", error=error),))

            def filled(scene: bpy.types.Scene, layer: bpy.types.ViewLayer, target: bpy.types.ViewLayer) -> BlenderResult[str]:
                def sampled(pair: tuple[Snapshot, int]) -> BlenderResult[str]:
                    return (
                        pair[0]
                        .apply(target)
                        .outcome.bind(lambda _: Descriptor.resolve(bpy.types.ViewLayer, "samples", bpy.types.IntProperty).bind(lambda descriptor: descriptor.write(target, pair[1])))
                        .map(lambda _: target.name)
                    )

                return paired(Snapshot.capture(layer), members.samples_of(scene, layer)).bind(sampled)

            return (
                accumulated(starmap(placed, planned))
                .bind(lambda targets: passes.synced(source, members.identity).bind(lambda _: accumulated(starmap(filled, targets))))
                .or_else_with(refused)
                .bind(lambda names: committed(names, tuple(copies)))
            )

        def copied(source: bpy.types.Scene) -> BlenderResult[tuple[str, ...]]:
            def each(name: str) -> BlenderResult[str]:
                def retained(scene: bpy.types.Scene) -> BlenderResult[str]:
                    made.append(scene)
                    return accumulated(
                        catch(lambda: scene.view_layers.remove(layer), exception=RuntimeError)().map_error(lambda error: (BlenderEditFault(request=repr(layer), error=error),))
                        for layer in tuple(scene.view_layers)
                        if layer.name != name
                    ).map(lambda _: scene.name)

                return added(members, source, name).bind(retained)

            return accumulated(each(layer.name) for layer in tuple(source.view_layers) if layer.use).or_else_with(refused).bind(committed)

        match current.form, form:
            case Copies(), Layers():
                return members.copies(context).bind(layered).map(Some)
            case Layers(), Copies():
                return members.source(context).bind(copied).map(Some)
            case _:
                return Ok(Nothing)

    manager = rooted(bpy.types.WindowManager, context)
    return manager.bind(lambda manager: members.sets.read(manager).bind(lambda current: switched(manager, current)))


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
    "previewed",
    "propagated",
    "synced",
    "syncing",
    "toggled",
    "transfers",
    "unsynced",
    "written",
]
