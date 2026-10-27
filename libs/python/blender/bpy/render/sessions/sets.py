"""Render sets of copied scenes or view layers, each set's capture synced, applied as setups, and locked across members."""

from collections.abc import Generator, Iterable, Mapping
from contextlib import contextmanager, ExitStack
from functools import partial
from itertools import chain, starmap
from types import MappingProxyType
from typing import ClassVar, Final, Protocol, Self

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.collections import Map
from expression.extra.result import catch
import msgspec
from typing_extensions import TypeForm

from blender.bpy.commands.operators import Answer, answered, Call, Confirm, Done, Empty, Operator, Own, Suppressible, tallied, Unchanged
from blender.bpy.data.ids.ownership import Mark, Ownership, Saved
from blender.bpy.data.rna.descriptors import assigned, choices, Descriptor, Native, parsed, rna, RnaPath, rooted, Struct, struct_at, Value
from blender.bpy.data.rna.snapshots import Applied, Endpoints, Snapshot
from blender.bpy.extension.events import Handler, updated
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Registrable
from blender.bpy.faults import BlenderReason
from blender.bpy.settings.attachments import Attached, Attachment, Recovery, Stored
from blender.bpy.settings.presets import Preset
from blender.bpy.settings.texts import TextRecord
from blender.bpy.ui.layout.lists import ItemList
from blender.bpy.ui.layout.rows import Matrix, Member, OperatorButton, Property, Pulldown, RowContent, RowDraw
from blender.bpy.ui.layout.sections import SectionDraws, Single
from blender.bpy.ui.layout.values import hosted, Listing
from blender.bpy.ui.surfaces.menus import Captured, Dropdown, Menu, PresetMenu
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired, RecordReason
from blender.records.fields import Case
from blender.records.trees import Reparametrized, transfer

# --- [TYPES] ----------------------------------------------------------------------------


class Capture[C: msgspec.Struct](Protocol):
    """What a render set captures from a member scene, writes back as a setup, and draws above its member lists."""

    @property
    def form(self) -> TypeForm[C]:
        """Record a capture decodes as, a setup preset's form."""
        ...

    @property
    def categories(self) -> tuple["Category", ...]:
        """Settings the set syncs from its source."""
        ...

    def captured(self, members: "Members", scene: bpy.types.Scene, /) -> Outcome[C]:
        """Captures a member scene."""
        ...

    def projected(self, members: "Members", scene: bpy.types.Scene, recipe: C, current: C, /) -> Outcome[tuple[C, C]]:
        """Returns the sparse writes and the projected capture of a recipe over a scene's current capture, its locks kept."""
        ...

    def applied(self, members: "Members", scene: bpy.types.Scene, sparse: C, /) -> Outcome[None]:
        """Writes sparse values to a member scene."""
        ...

    def rows(self, members: "Members", scope: "SetupScope[C]", /) -> tuple[RowContent, ...]:
        """Rows drawn above the member lists."""
        ...


# --- [CONSTANTS] ------------------------------------------------------------------------

OWNER: Final = "render_setup_owner"
SAMPLES: Final = Descriptor.declared(bpy.types.ViewLayer, "samples", bpy.types.IntProperty)
ENGINE_SAMPLES: Final = MappingProxyType({
    "BLENDER_EEVEE": (("eevee", "taa_render_samples"), ("eevee", "taa_samples")),
    "BLENDER_WORKBENCH": (("display", "render_aa"), ("display", "viewport_aa")),
    "CYCLES": (("cycles", "samples"), ("cycles", "preview_samples")),
})

# --- [MODELS] ---------------------------------------------------------------------------


class Copies(Case, frozen=True):
    """Scenes copied from source scene."""


class Layers(Case, frozen=True):
    """View layers of one scene."""


type Form = Copies | Layers


class Sets(bpy.types.PropertyGroup):
    """File render set settings."""

    form: bpy.props.EnumProperty(name="Render Sets", description="Copied scenes or view layers of one scene", items=choices((case.__name__, case.__name__, "") for case in (Layers, Copies)))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    across: bpy.props.BoolProperty(name="Edit Across", description="Copy category edits to unlocked members")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class SceneMember(bpy.types.PropertyGroup):
    """Sync lock and render inclusion of member scene."""

    locked: bpy.props.BoolProperty(name="Locked")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    rendered: bpy.props.BoolProperty(name="Render", default=True)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class LayerMember(bpy.types.PropertyGroup):
    """Sync lock of member view layer."""

    locked: bpy.props.BoolProperty(name="Locked")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Category(msgspec.Struct, frozen=True, kw_only=True):
    """Settings of one category on a scene or its view layers, each an RNA handle or a path the host states."""

    key: Key
    settings: tuple[Descriptor | str | RnaPath, ...]
    root: type[bpy.types.Scene] | type[bpy.types.ViewLayer] = bpy.types.Scene

    @property
    def title(self) -> str:
        """Display name of category key."""
        return bpy.path.display_name(self.key)

    @property
    def paths(self) -> tuple[str | RnaPath, ...]:
        """Setting paths from the category root."""
        return tuple(setting.path if isinstance(setting, Descriptor) else setting for setting in self.settings)


class Converting(bpy.types.PropertyGroup):
    """Target form of render set conversion."""

    form: bpy.props.EnumProperty(name="Render Sets", items=choices((case.__name__, case.__name__, "") for case in (Copies, Layers)))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class CategorySettings(msgspec.Struct, frozen=True, kw_only=True):
    """Category settings of one scene and of each of its view layers."""

    scene: Snapshot
    layers: Mapping[str, Snapshot]


class LockRequest(bpy.types.PropertyGroup):
    """Setting path a lock button toggles on the scene or view layer it drew."""

    path: bpy.props.StringProperty(name="Path", description="Setting path the lock toggles")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class SettingLocks(msgspec.Struct, frozen=True, kw_only=True):
    """Setting paths setups leave unchanged on a scene and on each of its view layers."""

    scene: frozenset[str] = frozenset()
    layers: Mapping[str, frozenset[str]] = msgspec.field(default_factory=dict)

    @staticmethod
    def bound(identity: Identity) -> Outcome[Stored["SettingLocks"]]:
        """Binds setting lock store."""
        return identity.identifier("render_setup_locks").map(lambda name: Stored(name=name, decoder=msgspec.msgpack.Decoder(SettingLocks)))

    def locking(self, owner: bpy.types.Scene | bpy.types.ViewLayer, path: str) -> tuple[bool, Self]:
        """Answers whether owner holds the path's lock, with the locks toggling it."""
        match owner:
            case bpy.types.Scene():
                return path in self.scene, msgspec.structs.replace(self, scene=self.scene ^ {path})
            case bpy.types.ViewLayer(name=layer):
                paths = self.layers.get(layer, frozenset())
                return path in paths, msgspec.structs.replace(self, layers={**self.layers, layer: paths ^ {path}})


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
    scenes: Attached[SceneMember]
    layers: Attached[LayerMember]
    categories: tuple[tuple[Category, tuple[Descriptor, ...]], ...]
    samples: Map[str, Descriptor[bpy.types.Property]]
    recovery: Recovery[Endpoints]

    # --- [MEMBERSHIP]

    def form(self, context: bpy.types.Context) -> Outcome[Form]:
        """Reads file render set form from window manager."""
        return rooted(bpy.types.WindowManager, context).map(lambda manager: formed(self.sets.struct(manager).form))

    def source(self, context: bpy.types.Context) -> Outcome[bpy.types.Scene]:
        """Returns marked source of active scene, else active scene."""

        def sourced(scene: bpy.types.Scene) -> bpy.types.Scene:
            match RenderSet.ownership.marked(self.identity, scene).bind(lambda mark: mark.of).to_optional():
                case bpy.types.Scene() as source:
                    return source
                case _:
                    return scene

        return rooted(bpy.types.Scene, context).map(sourced)

    def copies(self, context: bpy.types.Context) -> Outcome[tuple[bpy.types.Scene, ...]]:
        """Returns source, then its member scenes in data order."""
        return self.source(context).map(lambda source: (source, *RenderSet.ownership.serving(self.identity, source)))

    def members(self, context: bpy.types.Context) -> Outcome[tuple[bpy.types.Scene | bpy.types.ViewLayer, ...]]:
        """Returns members of file form in list order."""

        def listed(form: Form) -> Outcome[tuple[bpy.types.Scene | bpy.types.ViewLayer, ...]]:
            match form:
                case Copies():
                    return self.copies(context)
                case Layers():
                    return self.source(context).map(lambda scene: tuple(scene.view_layers))

        return self.form(context).bind(listed)

    def free[O: bpy.types.Scene | bpy.types.ViewLayer](self, owners: tuple[O, ...]) -> Outcome[tuple[O, ...]]:
        """Returns unlocked owners in given order."""
        return Ok(tuple(owner for owner in owners if not (self.scenes.struct(owner) if isinstance(owner, bpy.types.Scene) else self.layers.struct(owner)).locked))

    def unlocked(self, context: bpy.types.Context) -> Outcome[tuple[bpy.types.Scene | bpy.types.ViewLayer, ...]]:
        """Returns unlocked members of file form."""
        return self.members(context).bind(self.free)

    def rendered(self, context: bpy.types.Context) -> Outcome[tuple[tuple[bpy.types.Scene, Option[bpy.types.ViewLayer]], ...]]:
        """Returns dispatch scenes, paired with each used layer under layer form."""

        def dispatched(form: Form) -> Outcome[tuple[tuple[bpy.types.Scene, Option[bpy.types.ViewLayer]], ...]]:
            match form:
                case Copies():
                    return self.copies(context).map(lambda scenes: tuple((scene, Nothing) for scene in scenes if self.scenes.struct(scene).rendered))
                case Layers():
                    return self.source(context).map(lambda scene: tuple((scene, Some(layer)) for layer in scene.view_layers if layer.use))

        return self.form(context).bind(dispatched)

    # --- [SETTINGS]

    def selected(self, root: type[bpy.types.Scene] | type[bpy.types.ViewLayer]) -> tuple[Descriptor, ...]:
        """Resolved settings of every category on root."""
        return tuple(descriptor for category, descriptors in self.categories if category.root is root for descriptor in descriptors)

    def described(self, category: Category) -> tuple[Descriptor, ...]:
        """Resolved settings of one category."""
        return next(selected for held, selected in self.categories if held == category)

    def captured(self, category: Category, context: bpy.types.Context) -> Outcome[Snapshot]:
        """Captures category from source scene or its layer named like active one."""
        selected = self.described(category)

        def origin(scene: bpy.types.Scene) -> Outcome[bpy.types.Scene | bpy.types.ViewLayer]:
            match category.root, context.view_layer:
                case root, _ if root is bpy.types.Scene:
                    return Ok(scene)
                case _, bpy.types.ViewLayer(name=name):
                    return Option.of_optional(scene.view_layers.get(name)).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(scene.view_layers), member=name))
                case _, None:
                    return Error(Fault.of(BlenderReason.ABSENT, "bpy.context", member="view_layer"))

        return self.source(context).bind(origin).bind(lambda source: Snapshot.capture(source, Some(selected)))

    def samples_of(self, scene: bpy.types.Scene, layer: bpy.types.ViewLayer) -> Outcome[int]:
        """Reads layer sample override, else engine render samples, an enum member counting by its item's host value."""
        if layer.samples:
            return Ok(layer.samples)

        def counted(descriptor: Descriptor[bpy.types.Property], owner: Native) -> Outcome[int]:
            match descriptor.prop, descriptor.read(owner):
                case bpy.types.EnumProperty(enum_items=items), str(identifier):
                    return Ok(items[identifier].value)
                case _, int(count):
                    return Ok(count)
                case _, other:
                    return Error(Fault.of(RecordReason.OUTSIDE, rna(descriptor.path), repr(other), member="sample count"))

        selected: Outcome[Descriptor[bpy.types.Property]] = self.samples.try_find(scene.render.engine).to_result_with(
            lambda: Fault.of(BlenderReason.ABSENT, repr(scene.render), member=f"{scene.render.engine} samples")
        )
        return Result[Descriptor[bpy.types.Property], Faults].bind(
            selected,
            lambda descriptor: (
                struct_at(scene, descriptor.owner).map(lambda owner: counted(descriptor, owner)).default_with(lambda: Error(Fault.of(BlenderReason.ABSENT, repr(scene), member=rna(descriptor.owner))))
            ),
        )


class MemberScope(msgspec.Struct, frozen=True, kw_only=True):
    """Owners selected by set form and sync locks."""

    members: Members
    owner_type: type[bpy.types.Scene] | type[bpy.types.ViewLayer]
    unlocked: bool

    def owners(self, context: bpy.types.Context) -> Outcome[tuple[bpy.types.Scene | bpy.types.ViewLayer, ...]]:
        """Returns owners, unlocked ones alone when `unlocked` is set."""
        return (
            self
            .scenes(context)
            .map(lambda scenes: tuple(layer for scene in scenes for layer in scene.view_layers) if self.owner_type is bpy.types.ViewLayer else scenes)
            .bind(lambda owners: self.members.free(owners) if self.unlocked else Ok(owners))
        )

    def scenes(self, context: bpy.types.Context) -> Outcome[tuple[bpy.types.Scene, ...]]:
        """Returns member scenes under copy form, else source scene."""
        return self.members.form(context).bind(lambda form: self.members.copies(context) if isinstance(form, Copies) else self.members.source(context).map(lambda scene: (scene,)))


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
        """Projects recipe settings over current ones on the scene and its unlocked view layers, locked settings kept."""

        def locked(owner: bpy.types.Scene | bpy.types.ViewLayer, texts: frozenset[str]) -> frozenset[RnaPath]:
            return frozenset(steps for text in texts for steps in parsed(owner, text).to_option().to_list())

        def held(locks: SettingLocks, layers: tuple[bpy.types.ViewLayer, ...]) -> Outcome[tuple[CategorySettings, CategorySettings]]:
            return paired(
                recipe.scene.projected(scene, current.scene, locked(scene, locks.scene)),
                accumulated(
                    recipe.layers[layer.name].projected(layer, current.layers[layer.name], locked(layer, locks.layers.get(layer.name, frozenset()))).map(lambda pair: (layer.name, pair))
                    for layer in layers
                    if layer.name in recipe.layers
                ),
            ).map(
                lambda values: (
                    CategorySettings(scene=values[0][0], layers={name: pair[0] for name, pair in values[1]}),
                    CategorySettings(scene=values[0][1], layers={**current.layers, **{name: pair[1] for name, pair in values[1]}}),
                )
            )

        return paired(SettingLocks.bound(members.identity).bind(lambda store: locks_of(store, scene)), members.free(tuple(scene.view_layers))).bind(lambda parts: held(*parts))

    @classmethod
    def applied(cls, _members: Members, scene: bpy.types.Scene, sparse: CategorySettings, /) -> Outcome[None]:
        """Writes sparse scene and view layer settings."""
        return sparse.scene.apply(scene).outcome.bind(lambda _: accumulated(snapshot.apply(scene.view_layers[name]).outcome for name, snapshot in sparse.layers.items())).map(lambda _: None)

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
        return Listing(preset=Preset(key=RenderSet.key, form=self.capture.form), current=self.current, matches=self.matched, applied=self.applied)

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

        return MemberScope(members=self.members, owner_type=bpy.types.Scene, unlocked=True).scenes(context).bind(self.members.free).bind(lambda scenes: accumulated(map(planned, scenes))).bind(wrote)


# --- [SERVICES] -------------------------------------------------------------------------


class RenderSet[C: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """Render set stores, commands, menus, and section over what its capture holds."""

    key: ClassVar[Key] = "render_sets"
    title: ClassVar[str] = bpy.path.display_name(key)
    description: ClassVar[str] = ""
    popovers: ClassVar[tuple[Single[Empty], ...]] = ()
    opened: ClassVar[bool] = False
    record: ClassVar[TextRecord[Sets]] = TextRecord(key="render_set", form=Sets)
    scenes: ClassVar[Attachment[SceneMember]] = Attachment(key="render_set", form=SceneMember, owner=bpy.types.Scene)
    layers: ClassVar[Attachment[LayerMember]] = Attachment(key="render_set", form=LayerMember, owner=bpy.types.ViewLayer)
    ownership: ClassVar[Ownership[bpy.types.Scene]] = Ownership(kind=bpy.types.Scene, key="render_set", saved=Saved.ALWAYS, single=False)
    menu: ClassVar[Key] = key
    capture: Capture[C]

    # --- [BINDING]

    @property
    def prerequisites(self) -> tuple[Registrable, ...]:
        """Set, scene, and layer stores binding reads."""
        return self.record, self.scenes, self.layers

    def bound(self, identity: Identity) -> Outcome[Members]:
        """Resolves stores and descriptors."""
        selections = accumulated(
            accumulated(Ok(setting) if isinstance(setting, Descriptor) else Descriptor.resolve(category.root, setting, bpy.types.Property) for setting in category.settings).map(
                lambda found: (category, found)
            )
            for category in self.capture.categories
        )
        samples = accumulated(Descriptor.resolve(bpy.types.Scene, path, bpy.types.Property).map(lambda descriptor: (engine, descriptor)) for engine, (path, _) in ENGINE_SAMPLES.items())
        return paired(
            self.record.attachment.bound(identity), self.scenes.bound(identity), self.layers.bound(identity), selections, samples, identity.identifier("render_set_sync").map(Recovery.endpoints)
        ).map(lambda joined: Members(identity=identity, sets=joined[0], scenes=joined[1], layers=joined[2], categories=joined[3], samples=Map.of_seq(joined[4]), recovery=joined[5]))

    @staticmethod
    def captured(members: Members, category: Category) -> Captured:
        """Returns category preset source applying to unlocked owners."""
        return Captured(
            preset=Preset(key=category.key, form=Snapshot),
            scope=MemberScope(members=members, owner_type=category.root, unlocked=True),
            selection=tuple(descriptor.path for descriptor in members.described(category)),
        )

    # --- [COMMANDS]

    def commands(self, members: Members) -> RenderSetCommands:
        """Returns render set operators."""

        def render_set_add(call: Call[Empty]) -> Outcome[Done]:
            """Copies the source scene as a new member."""
            return members.copies(call.context).bind(lambda scenes: added(members, scenes[0], f"{scenes[0].name} {len(scenes)}")).map(lambda scene: Done(notes=(("INFO", f"Added {scene.name}"),)))

        def render_set_adopt(call: Call[Empty]) -> Outcome[Answer]:
            """Adds unmarked scenes to the source render sets."""

            def joined(source: bpy.types.Scene) -> Outcome[Answer]:
                sources = frozenset(scene for _, mark in self.ownership.held(members.identity) for scene in mark.of.to_list())

                def marked(scene: bpy.types.Scene) -> Outcome[bpy.types.Scene]:
                    def kept() -> bpy.types.Scene:
                        scene.is_runtime_data = False
                        scene.use_fake_user = True
                        return scene

                    return (
                        Mark(key=self.ownership.key, of=Some(source))
                        .write(members.identity, scene)
                        .bind(lambda _: catch(kept, exception=AttributeError)().map_error(partial(Fault.of, BlenderReason.REFUSED, repr(scene))))
                    )

                if not (
                    unmarked := tuple(scene for scene in bpy.data.scenes if scene != source and scene not in sources and not scene.is_runtime_data and Mark.read(members.identity, scene).is_none())
                ):
                    return Ok(Unchanged(reason="No unmarked scene to adopt"))
                edits = tuple(map(marked, unmarked))
                return tallied(edits, tuple(("INFO", f"Adopted {scene.name}") for edit in edits for scene in edit.to_option().to_list()))

            return members.source(call.context).bind(joined)

        def render_set_restore(call: Call[Empty]) -> Outcome[Answer]:
            """Removes copied members and shows the source scene."""

            def restored(scenes: tuple[bpy.types.Scene, ...]) -> Outcome[Answer]:
                source, *copies = scenes
                if not copies:
                    return Ok(Unchanged(reason=f"{source.name} holds no copied render set"))
                shown(source, copies)
                edits = tuple(removed(scene).map(lambda _: name) for scene in copies for name in (scene.name,))
                return tallied(edits, tuple(("INFO", f"Removed {name}") for edit in edits for name in edit.to_option().to_list()))

            return members.copies(call.context).bind(restored)

        def touched(context: bpy.types.Context, properties: Converting) -> Outcome[str]:
            return members.members(context).map(lambda owners: f"Convert {', '.join(owner.name for owner in owners)} to {properties.form}")

        def render_set_remove(call: Call[Empty]) -> Outcome[Done]:
            """Removes the active copied member."""

            def dropped(source: bpy.types.Scene) -> Outcome[Done]:
                match call.context.scene:
                    case bpy.types.Scene() as scene if Mark.read(members.identity, scene).filter(lambda mark: mark.key == RenderSet.ownership.key and mark.of == Some(source)).is_some():
                        name = scene.name
                        shown(source, (scene,))
                        return removed(scene).map(lambda _: Done(notes=(("INFO", f"Removed {name}"),)))
                    case _:
                        return Error(Fault.of(BlenderReason.ABSENT, repr(call.context.scene), member=RenderSet.ownership.key))

            return members.source(call.context).bind(dropped)

        def render_set_unsync(call: Call[Empty]) -> Outcome[Answer]:
            """Restores the pre-sync settings of unlocked members."""
            return unsynced(members, call.context).map(
                lambda count: Done(notes=(("INFO", f"Restored {count} render set members"),)) if count else Unchanged(reason="No render set holds synced settings")
            )

        def render_set_convert(call: Call[Converting]) -> Outcome[Answer]:
            """Transfers render sets between copied scenes and view layers."""
            return converted(members, formed(call.properties.form), call.context).map(
                lambda names: names.map(lambda made: Done(notes=tuple(("INFO", f"Created {name}") for name in made))).default_value(Unchanged(reason=f"Render sets use {call.properties.form}"))
            )

        def render_set_lock(call: Call[Empty]) -> Outcome[Answer]:
            """Toggles the sync lock of the active member."""
            return toggled(members, call.context)

        return RenderSetCommands(
            add=Operator[Empty](icon=Some("ADD"), form=Empty, run=render_set_add),
            adopt=Operator[Empty](icon=Some("IMPORT"), form=Empty, run=render_set_adopt),
            restore=Operator[Empty](icon=Some("SCENE_DATA"), form=Empty, run=render_set_restore),
            syncs=tuple(syncing(members, category) for category in self.capture.categories),
            unsync=Operator[Empty](icon=Some("LOOP_BACK"), form=Empty, run=render_set_unsync),
            convert=Operator[Converting](icon=Some("ARROW_LEFTRIGHT"), form=Converting, run=render_set_convert, invoke=Some(Confirm[Converting](message=Some(touched)))),
            remove=Operator[Empty](icon=Some("REMOVE"), form=Empty, run=render_set_remove),
            lock=Operator[Empty](
                icon=Some("LOCKED"),
                form=Empty,
                run=render_set_lock,
                invoke=Some(Suppressible[Empty](message=lambda _context, _record: "Unlocking never-synced member copies every category from source")),
            ),
        )

    # --- [SECTION]

    def composed(self, members: Members, commands: RenderSetCommands) -> Single:
        """Builds the capture's rows, member lists, and one settings matrix per category with its setting locks."""
        scope = SetupScope(members=members, capture=self.capture)
        return Single(
            key=self.key,
            title=self.title,
            header=(Pulldown(menu=scope.listing.key, text="", icon=Some("PRESET")),),
            rows=(
                *self.capture.rows(members, scope),
                Property(target=Member(root=bpy.types.WindowManager, path=(members.sets.name, "across"))),
                ItemList[SceneMember, bpy.types.Scene](
                    key="render_set_scenes",
                    item=bpy.types.Scene,
                    collection=Member(root=bpy.types.BlendData, path=("scenes",)),
                    active=Member(root=bpy.types.Window, path=("scene",)),
                    attachment=Some(self.scenes),
                    where=Some(
                        lambda context, item: members.identity.reports.reported(
                            lambda: members.form(context).bind(lambda form: members.copies(context).map(lambda scenes: item in scenes) if isinstance(form, Copies) else Ok(False))
                        )().default_value(False)
                    ),
                    side=tuple(OperatorButton(target=command, text="") for command in (commands.add, commands.remove, commands.lock)),
                ),
                ItemList[LayerMember, bpy.types.ViewLayer](
                    key="render_set_layers",
                    item=bpy.types.ViewLayer,
                    collection=Member(root=bpy.types.Scene, path=("view_layers",)),
                    active=Member(root=bpy.types.Window, path=("view_layer",)),
                    attachment=Some(self.layers),
                    where=Some(lambda context, _item: members.identity.reports.reported(members.form)(context).filter(lambda form: isinstance(form, Layers)).is_some()),
                ),
                *(
                    Single(
                        key=category.key,
                        title=category.title,
                        header=(Pulldown(menu=self.captured(members, category).key, text="", icon=Some("PRESET")), OperatorButton(target=sync, text="")),
                        rows=(
                            Matrix(
                                scope=MemberScope(members=members, owner_type=category.root, unlocked=False),
                                settings=category.paths,
                                compound=((bpy.types.CurveMapping, lambda layout, owner, key: layout.template_curve_mapping(owner, key)),),
                                action=Some((LOCK, lambda identity, setting: drawn(rna(setting.path), identity))),
                            ),
                        ),
                    )
                    for category, sync in zip(self.capture.categories, commands.syncs)
                ),
            ),
        )

    def realized(self, identity: Identity) -> Outcome[SectionDraws]:
        """Builds section over extension stores."""
        return self.bound(identity).bind(lambda members: self.composed(members, self.commands(members)).realized(identity))

    def drawn(self, identity: Identity) -> Outcome[RowDraw]:
        """Returns section as inline layout panel draw."""
        return self.realized(identity).map(lambda drawn: drawn.inline)

    # --- [REGISTRATION]

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[Outcome[Acquisitions]]:
        """Holds deferred edit transfer while registered."""
        with ExitStack() as stack:

            def joined(members: Members) -> Acquisitions:
                deferred = stack.enter_context(identity.reports.deferred(partial(propagated, members)))

                def edited(scene: bpy.types.Scene, _updates: tuple[bpy.types.DepsgraphUpdate, ...]) -> Outcome[None]:
                    context = bpy.context

                    def handed(record: Sets) -> None:
                        if record.across and scene == context.scene:
                            for owner in Option.of_optional(context.scene if record.form == Copies.__name__ else context.view_layer).to_list():
                                deferred(owner)

                    return rooted(bpy.types.WindowManager, context).map(lambda manager: handed(members.sets.struct(manager)))

                commands = self.commands(members)
                listing = SetupScope(members=members, capture=self.capture).listing
                return (
                    self.ownership,
                    *commands.listed,
                    commands.remove,
                    commands.lock,
                    self.composed(members, commands),
                    PresetMenu(source=listing),
                    Handler(handlers=bpy.app.handlers.depsgraph_update_post, body=partial(updated, (bpy.types.Scene,), edited)),
                    Menu(
                        key=self.menu,
                        title=self.title,
                        description="Create, sync, restore, and convert render sets",
                        body=Dropdown(
                            entries=(
                                *(OperatorButton(target=placed) for placed in commands.listed),
                                Pulldown(menu=listing.key),
                                *(Pulldown(menu=self.captured(members, category).key) for category in self.capture.categories),
                            )
                        ),
                    ),
                    *(PresetMenu(source=self.captured(members, category)) for category in self.capture.categories),
                )

            yield self.bound(identity).map(joined)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [FORMS]


def formed(name: str) -> Form:
    """Returns the render set form an enum identifier names."""
    return Copies() if name == Copies.__name__ else Layers()


# --- [LOCKS]


def locks_of(store: Stored[SettingLocks], scene: bpy.types.Scene) -> Outcome[SettingLocks]:
    """Reads scene setting locks, none before the first lock."""
    return store.read(scene).map(lambda held: held.default_with(SettingLocks))


def render_setup_lock(call: Call[LockRequest]) -> Outcome[Done]:
    """Toggles the request's setting lock on the scene or view layer its button drew, in the owner scene's lock store."""

    def toggled(owner: object, store: Stored[SettingLocks]) -> Outcome[Done]:
        match owner:
            case bpy.types.Scene() | bpy.types.ViewLayer() if isinstance(scene := owner.id_data, bpy.types.Scene):
                return locks_of(store, scene).bind(lambda locks: store.write(scene, locks.locking(owner, call.properties.path)[1])).map(lambda _: Done())
            case _:
                return Error(Fault.of(BlenderReason.ABSENT, repr(owner), member=OWNER))

    return paired(call.identity.identifier(OWNER).bind(lambda name: hosted(name, call.context)), SettingLocks.bound(call.identity)).bind(lambda parts: toggled(*parts))


def drawn(path: str, identity: Identity) -> Outcome[RowDraw]:
    """Draws a locked or unlocked toggle of a setting path's lock on the drawn scene or view layer."""

    def toggle(name: str, store: Stored[SettingLocks], draws: Mapping[bool, RowDraw], layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
        layout.context_pointer_set(name, holder)
        if isinstance(holder, bpy.types.Scene | bpy.types.ViewLayer) and isinstance(scene := holder.id_data, bpy.types.Scene):
            identity.reports.reported(lambda: locks_of(store, scene).map(lambda locks: draws[locks.locking(holder, path)[0]](layout, holder, context)))().to_optional()

    request = Own(command=LOCK, values={"path": path})
    return paired(
        identity.identifier(OWNER),
        SettingLocks.bound(identity),
        accumulated(OperatorButton(target=request, text="", icon=Some("LOCKED" if held else "UNLOCKED"), emboss=False).drawn(identity) for held in (False, True)),
    ).map(lambda parts: partial(toggle, parts[0], parts[1], dict(zip((False, True), parts[2]))))


# --- [MEMBERS]


def shown(source: bpy.types.Scene, replaced: Iterable[bpy.types.Scene]) -> None:
    """Shows source in every window showing a replaced scene."""
    held = frozenset(replaced)
    for window in (window for manager in bpy.data.window_managers for window in manager.windows):
        if window.scene in held:
            window.scene = source


def removed(owner: bpy.types.Scene | tuple[bpy.types.Scene, bpy.types.ViewLayer]) -> Outcome[None]:
    """Removes a member scene, or a view layer from its scene."""
    match owner:
        case bpy.types.Scene() as scene:
            return catch(bpy.data.scenes.remove, exception=RuntimeError)(scene).map_error(partial(Fault.of, BlenderReason.REFUSED, repr(scene)))
        case (scene, layer):
            return catch(scene.view_layers.remove, exception=RuntimeError)(layer).map_error(partial(Fault.of, BlenderReason.REFUSED, repr(layer)))


# --- [TRANSFERS]


def transfers(
    members: Members, category: Category, context: bpy.types.Context, targets: tuple[bpy.types.Scene | bpy.types.ViewLayer, ...]
) -> Outcome[tuple[tuple[bpy.types.Scene | bpy.types.ViewLayer, Snapshot, tuple[Reparametrized[Value, RnaPath], ...]], ...]]:
    """Returns value edits per target from source category."""
    selected = next(selected for held, selected in members.categories if held == category)
    return members.captured(category, context).bind(
        lambda source: accumulated(Snapshot.capture(target, Some(selected)).map(lambda held: (target, held, transfer(held.tree, source.tree))) for target in targets)
    )


def previewed(members: Members, category: Category, context: bpy.types.Context) -> Outcome[tuple[tuple[str, tuple[Reparametrized[Value, RnaPath], ...]], ...]]:
    """Returns value edits per unlocked owner for sync confirmation."""
    return (
        MemberScope(members=members, owner_type=category.root, unlocked=True)
        .owners(context)
        .bind(lambda targets: transfers(members, category, context, targets))
        .map(lambda rows: tuple((target.name, edits) for target, _, edits in rows if edits))
    )


def written(members: Members, category: Category, context: bpy.types.Context, targets: tuple[bpy.types.Scene | bpy.types.ViewLayer, ...]) -> Outcome[tuple[Applied, ...]]:
    """Applies source category, recording each target's pre-sync and synced values."""
    return members.captured(category, context).bind(
        lambda source: accumulated(members.recovery.recorded(target, ((target, source),), lambda ends: ends) for target in targets).map(lambda groups: tuple(chain.from_iterable(groups)))
    )


def synced(members: Members, category: Category, context: bpy.types.Context) -> Outcome[Answer]:
    """Copies source category to unlocked owners when any differs."""
    reason = f"Unlocked render sets match source {category.title} settings"

    def wrote(targets: tuple[bpy.types.Scene | bpy.types.ViewLayer, ...]) -> Outcome[Answer]:
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

    def message(context: bpy.types.Context, _record: Empty) -> Outcome[str]:
        return previewed(members, category, context).map(lambda rows: f"Writes {sum(len(edit.after) for _, edits in rows for edit in edits)} {category.title} settings across {len(rows)} render sets")

    def sync(call: Call[Empty]) -> Outcome[Answer]:
        return synced(members, category, call.context)

    sync.__name__ = sync.__qualname__ = f"render_set_sync_{category.key}"
    sync.__doc__ = f"Copies the source {category.title} settings to unlocked render sets."
    return Operator[Empty](icon=Some("FILE_REFRESH"), form=Empty, run=sync, invoke=Some(Confirm[Empty](message=Some(message))))


def toggled(members: Members, context: bpy.types.Context) -> Outcome[Done]:
    """Toggles active member lock, first syncing a never-synced member it unlocks."""

    def configured(owner: bpy.types.Scene | bpy.types.ViewLayer, record: SceneMember | LayerMember) -> Outcome[None]:
        def synced_first(held: Option[Endpoints]) -> Outcome[None]:
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

        return members.recovery.stored.read(owner).bind(synced_first) if record.locked else Ok(None)

    def changed[M: SceneMember | LayerMember](attached: Attached[M], owner: bpy.types.Scene | bpy.types.ViewLayer) -> Outcome[Done]:
        record = attached.struct(owner)
        unlocking = bool(record.locked)
        return configured(owner, record).bind(lambda _: assigned(record, {"locked": not unlocking})).map(lambda _: Done(notes=(("INFO", f"{'Unlocked' if unlocking else 'Locked'} {owner.name}"),)))

    def selected(form: Form) -> Outcome[Done]:
        match form:
            case Copies():
                return rooted(bpy.types.Scene, context).bind(lambda scene: changed(members.scenes, scene))
            case Layers():
                return rooted(bpy.types.ViewLayer, context).bind(lambda layer: changed(members.layers, layer))

    return members.form(context).bind(selected)


def unsynced(members: Members, context: bpy.types.Context) -> Outcome[int]:
    """Restores pre-sync values of unlocked owners, keeping records that fail to restore, answering the restored count."""
    return accumulated(
        MemberScope(members=members, owner_type=root, unlocked=True).owners(context).bind(lambda owners: accumulated(map(members.recovery.reverted, owners)))
        for root in dict.fromkeys(category.root for category, _ in members.categories)
    ).map(lambda groups: sum(held for group in groups for held in group))


def propagated(members: Members, edited: bpy.types.Scene | bpy.types.ViewLayer) -> Outcome[Option[str]]:
    """Copies category edits to other unlocked members in one undo step."""

    def wrote(others: tuple[bpy.types.Scene | bpy.types.ViewLayer, ...]) -> Outcome[Option[str]]:
        return accumulated(
            Snapshot.capture(edited, Some(selected)).bind(lambda source: accumulated((applied := source.apply(other)).outcome.map(lambda _: applied) for other in others if other != edited))
            for category, selected in members.categories
            if isinstance(edited, category.root)
        ).map(lambda outcomes: Some("Edit Across Render Sets") if any(applied.written for group in outcomes for applied in group) else Nothing)

    return members.unlocked(bpy.context).bind(wrote)


def added(members: Members, source: bpy.types.Scene, name: str) -> Outcome[bpy.types.Scene]:
    """Copies source as retained member under name, the host suffixing a taken name."""
    return RenderSet.ownership.made(
        members.identity, lambda: Option.of_optional(source.copy()).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(source), member="copy")), of=Some(source)
    ).bind(lambda scene: assigned(scene, {"name": name}).map(lambda _: scene))


def converted(members: Members, form: Form, context: bpy.types.Context) -> Outcome[Option[tuple[str, ...]]]:
    """Converts render set form, removing created owners on failure, `Nothing` for current form."""

    def switched(current: Sets) -> Outcome[Option[tuple[str, ...]]]:
        made: list[tuple[bpy.types.Scene, bpy.types.ViewLayer] | bpy.types.Scene] = []

        def refused(faults: Faults) -> Outcome[tuple[str, ...]]:
            return paired(Error(faults), accumulated(map(removed, reversed(made)))).map(lambda _: ())

        def committed(names: tuple[str, ...], old: tuple[bpy.types.Scene, ...] = ()) -> Outcome[tuple[str, ...]]:
            def finished(_: None) -> Outcome[tuple[str, ...]]:
                def replaced(scene: bpy.types.Scene) -> Outcome[tuple[str, ...]]:
                    shown(scene, old)
                    return accumulated(map(removed, old)).map(lambda _: names)

                return members.source(context).bind(replaced) if old else Ok(names)

            return assigned(current, {"form": type(form).__name__}).or_else_with(lambda faults: refused(faults).map(lambda _: None)).bind(finished)

        def layered(scenes: tuple[bpy.types.Scene, ...]) -> Outcome[tuple[str, ...]]:
            source, *copies = scenes
            planned = tuple(
                (scene, layer, scene.name if len(used) == 1 else f"{scene.name} {layer.name}")
                for scene in copies
                for used in (tuple(layer for layer in scene.view_layers if layer.use),)
                for layer in used
            )

            def placed(scene: bpy.types.Scene, layer: bpy.types.ViewLayer, name: str) -> Outcome[tuple[bpy.types.Scene, bpy.types.ViewLayer, bpy.types.ViewLayer]]:
                def created() -> tuple[bpy.types.Scene, bpy.types.ViewLayer, bpy.types.ViewLayer]:
                    target = source.view_layers.new(name)
                    made.append((source, target))
                    return scene, layer, target

                return catch(created, exception=RuntimeError)().map_error(partial(Fault.of, BlenderReason.REFUSED, f"{source!r}.view_layers.new({name!r})"))

            def filled(scene: bpy.types.Scene, layer: bpy.types.ViewLayer, target: bpy.types.ViewLayer) -> Outcome[str]:
                def sampled(pair: tuple[Snapshot, int]) -> Outcome[str]:
                    return pair[0].apply(target).outcome.bind(lambda _: SAMPLES.write(target, pair[1])).map(lambda _: target.name)

                return paired(Snapshot.capture(layer), members.samples_of(scene, layer)).bind(sampled)

            return accumulated(starmap(placed, planned)).bind(lambda targets: accumulated(starmap(filled, targets))).or_else_with(refused).bind(lambda names: committed(names, tuple(copies)))

        def copied(source: bpy.types.Scene) -> Outcome[tuple[str, ...]]:
            def each(name: str) -> Outcome[str]:
                def retained(scene: bpy.types.Scene) -> Outcome[str]:
                    made.append(scene)
                    return accumulated(removed((scene, layer)) for layer in tuple(scene.view_layers) if layer.name != name).map(lambda _: scene.name)

                return added(members, source, name).bind(retained)

            return accumulated(each(layer.name) for layer in tuple(source.view_layers) if layer.use).or_else_with(refused).bind(committed)

        match formed(current.form), form:
            case Copies(), Layers():
                return members.copies(context).bind(layered).map(Some)
            case Layers(), Copies():
                return members.source(context).bind(copied).map(Some)
            case _:
                return Ok(Nothing)

    return rooted(bpy.types.WindowManager, context).bind(lambda manager: switched(members.sets.struct(manager)))


# --- [COMPOSITION] ----------------------------------------------------------------------

LOCK: Final = Operator(form=LockRequest, run=render_setup_lock)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "LOCK",
    "OWNER",
    "SAMPLES",
    "Capture",
    "Categories",
    "Category",
    "CategorySettings",
    "Converting",
    "Copies",
    "Form",
    "LayerMember",
    "Layers",
    "LockRequest",
    "MemberScope",
    "Members",
    "RenderSet",
    "RenderSetCommands",
    "SceneMember",
    "Sets",
    "SettingLocks",
    "SetupScope",
    "added",
    "converted",
    "drawn",
    "formed",
    "locks_of",
    "previewed",
    "propagated",
    "removed",
    "render_setup_lock",
    "shown",
    "synced",
    "syncing",
    "toggled",
    "transfers",
    "unsynced",
    "written",
]
