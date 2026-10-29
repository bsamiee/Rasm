"""Render sets of copied scenes or view layers: the set service with its commands, section, menus, and form conversion."""

from collections.abc import Callable, Generator
from contextlib import contextmanager, ExitStack
from functools import cached_property, partial, update_wrapper
from itertools import starmap
from types import MappingProxyType
from typing import ClassVar, Final, TYPE_CHECKING

import bpy
from expression import Error, Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.calls import Called, Opened
from blender.bpy.commands.invocations import Popup, Suppressible
from blender.bpy.commands.operators import Answer, Call, Done, Draw, Empty, Invoke, Operator, tallied, Unchanged
from blender.bpy.data.ids.ownership import Mark
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.data.rna.paths import parsed, rooted
from blender.bpy.data.rna.snapshots import assigned, Snapshot
from blender.bpy.extension.events import Handler, updated
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Registrable
from blender.bpy.faults import BlenderReason
from blender.bpy.render.sessions.engines import samples
from blender.bpy.render.sessions.members import (
    added,
    Category,
    Form,
    FORMS,
    KEY,
    Members,
    MemberScope,
    Membership,
    OWNERSHIP,
    previewed,
    propagated,
    removed,
    SceneMembership,
    Sets,
    shown,
    synced,
    toggled,
    unsynced,
)
from blender.bpy.render.sessions.setups import Capture, SETTING_LOCKS, SetupScope
from blender.bpy.render.view_layers.passes import used
from blender.bpy.settings.attachments import Attachment, Recovery
from blender.bpy.settings.presets import Preset
from blender.bpy.settings.texts import TextRecord
from blender.bpy.ui.layout.lists import ItemList
from blender.bpy.ui.layout.members import Member
from blender.bpy.ui.layout.presets import Captured, menu_key
from blender.bpy.ui.layout.rows import Layout, Opens, OperatorButton, Property, RowContent
from blender.bpy.ui.layout.sections import Section
from blender.bpy.ui.layout.tables import Matrix
from blender.bpy.ui.surfaces.menus import Dropdown, Menu, PresetMenu
from blender.records.faults import accumulated, Fault, Outcome, paired

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [CONSTANTS] ------------------------------------------------------------------------

_SAMPLES: Final = Descriptor.declared(bpy.types.ViewLayer, ("samples",), bpy.types.IntProperty)

# --- [MODELS] ---------------------------------------------------------------------------


class Converting(bpy.types.PropertyGroup):
    """Target form of render set conversion."""

    form: bpy.props.EnumProperty(name="Render Sets", items=FORMS, default=Form.COPIES.value)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


# --- [SERVICES] -------------------------------------------------------------------------


class RenderSet[C: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Render set stores, commands binding the set's members on call, menus, and section over what its capture holds."""

    key: ClassVar[Key] = KEY
    title: ClassVar[str] = bpy.path.display_name(KEY)
    record: ClassVar[TextRecord[Sets]] = TextRecord(key="render_set", form=Sets)
    scenes: ClassVar[Attachment[SceneMembership]] = Attachment(key="render_set", form=SceneMembership, owner=bpy.types.Scene)
    layers: ClassVar[Attachment[Membership]] = Attachment(key="render_set", form=Membership, owner=bpy.types.ViewLayer)
    menu: ClassVar[Key] = KEY
    capture: Capture[C]

    # --- [BINDING]

    @property
    def prerequisites(self) -> tuple[Registrable, ...]:
        """Set, scene, and layer stores binding reads."""
        return self.record, self.scenes, self.layers

    def bound(self, identity: Identity) -> Outcome[Members]:
        """Resolves stores and each category's settings on its root."""
        selections = accumulated(
            accumulated(
                Ok(setting)
                if isinstance(setting, Descriptor)
                else (parsed(setting) if isinstance(setting, str) else Ok(setting)).bind(lambda path: Descriptor.resolve(category.root, path, bpy.types.Property))
                for setting in category.settings
            ).map(lambda found: (category, found))
            for category in self.capture.categories
        )
        return paired(self.record.attachment.bound(identity), self.scenes.bound(identity), self.layers.bound(identity), selections, Recovery.endpoints(identity, "render_set_sync")).map(
            lambda joined: Members(identity=identity, sets=joined[0], scenes=joined[1], layers=joined[2], categories=MappingProxyType(dict(joined[3])), recovery=joined[4])
        )

    def stepped[T: bpy.types.PropertyGroup](self, step: Callable[[Members, Call[T]], Outcome[Answer]], call: Call[T]) -> Outcome[Answer]:
        """Runs a set step over the members the call's identity binds."""
        return self.bound(call.identity).bind(lambda members: step(members, call))

    def stepping[T: bpy.types.PropertyGroup](
        self, step: Callable[[Members, Call[T]], Outcome[Answer]], icon: "rna_enums.IconItems", form: type[T], /, *, invoke: Option[Invoke[T, None]] = Nothing, draw: Option[Draw] = Nothing
    ) -> Operator[T]:
        """Command running a set step, keyed and described by the step."""
        return Operator[T](icon=Some(icon), form=form, run=update_wrapper(partial(self.stepped, step), step), invoke=invoke, draw=draw)

    # --- [COMMANDS]

    @cached_property
    def adding(self) -> Operator[Empty]:
        """Command copying the source scene as a member."""
        return self.stepping(render_set_add, "ADD", Empty)

    @cached_property
    def adopting(self) -> Operator[Empty]:
        """Command adopting unmarked scenes."""
        return self.stepping(render_set_adopt, "IMPORT", Empty)

    @cached_property
    def restoring(self) -> Operator[Empty]:
        """Command removing copied members."""
        return self.stepping(render_set_restore, "SCENE_DATA", Empty)

    @cached_property
    def syncs(self) -> tuple[Operator[Empty], ...]:
        """One sync command per category, keyed by its category and confirmed with its change counts."""
        return tuple(
            Operator[Empty](
                icon=Some("FILE_REFRESH"),
                form=Empty,
                run=update_wrapper(partial(self.stepped, partial(render_set_sync, category)), render_set_sync),
                named=Some(f"render_set_sync_{category.key}"),
                invoke=Some(Popup(partial(bpy.types.WindowManager.invoke_props_dialog))),
                draw=Some(partial(self.confirmed, partial(_sync_message, category), ())),
            )
            for category in self.capture.categories
        )

    @cached_property
    def unsyncing(self) -> Operator[Empty]:
        """Command restoring pre-sync settings."""
        return self.stepping(render_set_unsync, "LOOP_BACK", Empty)

    @cached_property
    def converting(self) -> Operator[Converting]:
        """Command converting the set form, confirmed with the members it converts."""
        return self.stepping(
            render_set_convert,
            "ARROW_LEFTRIGHT",
            Converting,
            invoke=Some(Popup(partial(bpy.types.WindowManager.invoke_props_dialog))),
            draw=Some(
                partial(
                    self.confirmed,
                    lambda members, context, _properties: members.members(context).map(lambda owners: f"Converts {', '.join(owner.name for owner in owners)} to the form below"),
                    ("form",),
                )
            ),
        )

    @cached_property
    def removing(self) -> Operator[Empty]:
        """Command removing the active copied member."""
        return self.stepping(render_set_remove, "REMOVE", Empty)

    @cached_property
    def locking(self) -> Operator[Empty]:
        """Command toggling the active member's sync lock."""
        return self.stepping(render_set_lock, "LOCKED", Empty, invoke=Some(Suppressible[Empty](message=lambda _context, _record: "Unlocking never-synced member copies every category from source")))

    @property
    def listed(self) -> tuple[Operator[Empty] | Operator[Converting], ...]:
        """Commands in menu order."""
        return self.adding, self.adopting, self.restoring, *self.syncs, self.unsyncing, self.converting

    def confirmed(
        self,
        message: Callable[[Members, bpy.types.Context, bpy.types.OperatorProperties], Outcome[str]],
        fields: tuple[str, ...],
        layout: bpy.types.UILayout,
        context: bpy.types.Context,
        properties: bpy.types.OperatorProperties,
        identity: Identity,
    ) -> None:
        """Draws a confirmation dialog's message over the members the identity binds, or the faults computing it, above the form fields it names."""
        lines = self.bound(identity).bind(lambda members: message(members, context, properties)).map(lambda text: (text,)).default_with(lambda faults: tuple(fault.message for fault in faults))
        for line in lines:
            layout.label(text=line)
        for name in fields:
            layout.prop(properties, name)

    # --- [SECTION]

    def rows(self, members: Members) -> tuple[RowContent, ...]:
        """Capture rows, member lists, and one settings matrix per category with its setting locks."""
        scope = SetupScope(members=members, capture=self.capture)
        return (
            *self.capture.rows(members, scope),
            Property(target=Member(root=bpy.types.WindowManager, path=(members.sets.name, "across"))),
            ItemList[SceneMembership, bpy.types.Scene](
                key="render_set_scenes",
                item=bpy.types.Scene,
                collection=Member(root=bpy.types.BlendData, path=("scenes",)),
                active=Some(Member(root=bpy.types.Window, path=("scene",))),
                attachment=Some(self.scenes),
                where=Some(lambda context, _ui, item: members.form(context).bind(lambda form: members.copies(context).map(lambda scenes: item in scenes) if form is Form.COPIES else Ok(False))),
                side=tuple(OperatorButton(target=command, keywords={"text": ""}) for command in (self.adding, self.removing, self.locking)),
            ),
            ItemList[Membership, bpy.types.ViewLayer](
                key="render_set_layers",
                item=bpy.types.ViewLayer,
                collection=Member(root=bpy.types.Scene, path=("view_layers",)),
                active=Some(Member(root=bpy.types.Window, path=("view_layer",))),
                attachment=Some(self.layers),
                cells=(Property(target=Member(root=bpy.types.ViewLayer, path=("use",)), keywords={"text": ""}),),
                where=Some(lambda context, _ui, _item: members.form(context).map(lambda form: form is Form.LAYERS)),
            ),
            *(
                Section(
                    key=category.key,
                    title=category.title,
                    header=(Opens(target=Opened(menu_key(_category_preset(category)), Called.MENU), keywords={"text": ""}, icon=Some("PRESET")), OperatorButton(target=sync, keywords={"text": ""})),
                    rows=(
                        Matrix(
                            scope=MemberScope(members=members, owner_type=category.root, unlocked=False),
                            settings=tuple(descriptor.path for descriptor in members.categories[category]),
                            compound=((bpy.types.CurveMapping, lambda layout, owner, key: layout.template_curve_mapping(owner, key)),),
                            action=Some(lambda identity, setting: SETTING_LOCKS.drawn(setting.path, identity)),
                        ),
                    ),
                )
                for category, sync in zip(self.capture.categories, self.syncs)
            ),
        )

    @property
    def section(self) -> Section:
        """Render set section: setup presets in its header, then rows over the members the drawing identity binds."""
        return Section(
            key=self.key,
            title=self.title,
            header=(Opens(target=Opened(menu_key(Preset(key=KEY, form=self.capture.form)), Called.MENU), keywords={"text": ""}, icon=Some("PRESET")),),
            rows=(Layout(rows=lambda identity: self.bound(identity).map(self.rows)),),
        )

    # --- [REGISTRATION]

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[Outcome[Acquisitions]]:
        """Holds deferred edit transfer while registered, with the commands, the rows the section draws, and the preset menus over the extension's members."""
        with ExitStack() as stack:

            def joined(members: Members) -> Acquisitions:
                deferred = stack.enter_context(identity.reports.deferred(partial(propagated, members)))

                def edited(scene: bpy.types.Scene, _updates: tuple[bpy.types.DepsgraphUpdate, ...]) -> Outcome[None]:
                    context = bpy.context

                    def handed(record: Sets) -> None:
                        if record.across and scene == context.scene:
                            for owner in Option.of_optional(context.scene if Form(record.form) is Form.COPIES else context.view_layer).to_list():
                                deferred(owner)

                    return rooted(bpy.types.WindowManager, context).map(lambda manager: handed(members.sets.struct(manager)))

                listing = SetupScope(members=members, capture=self.capture).listing
                return (
                    OWNERSHIP,
                    SETTING_LOCKS.operator,
                    *self.listed,
                    self.removing,
                    self.locking,
                    Layout(rows=self.rows(members)),
                    PresetMenu(source=listing),
                    Handler(handlers=bpy.app.handlers.depsgraph_update_post, body=partial(updated, (bpy.types.Scene,), edited)),
                    Menu(
                        key=self.menu,
                        title=self.title,
                        description="Create, sync, restore, and convert render sets",
                        body=Dropdown(
                            entries=(
                                *(OperatorButton(target=placed) for placed in self.listed),
                                Opens(target=Opened(listing.key, Called.MENU)),
                                *(Opens(target=Opened(menu_key(_category_preset(category)), Called.MENU)) for category in self.capture.categories),
                            )
                        ),
                    ),
                    *(
                        PresetMenu(
                            source=Captured(
                                preset=_category_preset(category),
                                scope=MemberScope(members=members, owner_type=category.root, unlocked=True),
                                selection=tuple(descriptor.path for descriptor in members.categories[category]),
                            )
                        )
                        for category in self.capture.categories
                    ),
                )

            yield self.bound(identity).map(joined)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [STEPS]


def render_set_add(members: Members, call: Call[Empty]) -> Outcome[Answer]:
    """Copies the source scene as a new member."""
    return members.copies(call.context).bind(lambda scenes: added(members, scenes[0], f"{scenes[0].name} {len(scenes)}")).map(lambda scene: Done(notes=(("INFO", f"Added {scene.name}"),)))


def render_set_adopt(members: Members, call: Call[Empty]) -> Outcome[Answer]:
    """Adds unmarked scenes to the source render sets."""

    def joined(source: bpy.types.Scene) -> Outcome[Answer]:
        sources = frozenset(scene for _, mark in OWNERSHIP.held(members.identity) for scene in mark.of.to_list())
        if not (unmarked := tuple(scene for scene in bpy.data.scenes if scene != source and scene not in sources and not scene.is_runtime_data and Mark.read(members.identity, scene).is_none())):
            return Ok(Unchanged(reason="No unmarked scene to adopt"))
        edits = tuple(OWNERSHIP.owned(members.identity, scene, of=Some(source)) for scene in unmarked)
        return tallied(edits, tuple(("INFO", f"Adopted {scene.name}") for edit in edits for scene in edit.to_option().to_list()))

    return members.source(call.context).bind(joined)


def render_set_restore(members: Members, call: Call[Empty]) -> Outcome[Answer]:
    """Removes copied members and shows the source scene."""

    def restored(scenes: tuple[bpy.types.Scene, ...]) -> Outcome[Answer]:
        source, *copies = scenes
        if not copies:
            return Ok(Unchanged(reason=f"{source.name} holds no copied render set"))
        shown(source, copies)
        edits = tuple(removed(scene).map(lambda _: name) for scene in copies for name in (scene.name,))
        return tallied(edits, tuple(("INFO", f"Removed {name}") for edit in edits for name in edit.to_option().to_list()))

    return members.copies(call.context).bind(restored)


def render_set_sync(category: Category, members: Members, call: Call[Empty]) -> Outcome[Answer]:
    """Copies the source settings of a category to unlocked render sets."""
    return synced(members, category, call.context)


def render_set_unsync(members: Members, call: Call[Empty]) -> Outcome[Answer]:
    """Restores the pre-sync settings of unlocked members."""
    return unsynced(members, call.context).map(lambda count: Done(notes=(("INFO", f"Restored {count} render set members"),)) if count else Unchanged(reason="No render set holds synced settings"))


def render_set_convert(members: Members, call: Call[Converting]) -> Outcome[Answer]:
    """Transfers render sets between copied scenes and view layers."""
    return converted(members, Form(call.properties.form), call.context).map(
        lambda names: names.map(lambda made: Done(notes=tuple(("INFO", f"Created {name}") for name in made))).default_value(Unchanged(reason=f"Render sets use {call.properties.form}"))
    )


def render_set_remove(members: Members, call: Call[Empty]) -> Outcome[Answer]:
    """Removes the active copied member."""

    def dropped(source: bpy.types.Scene) -> Outcome[Answer]:
        match call.context.scene:
            case bpy.types.Scene() as scene if scene in OWNERSHIP.serving(members.identity, source):
                name = scene.name
                shown(source, (scene,))
                return removed(scene).map(lambda _: Done(notes=(("INFO", f"Removed {name}"),)))
            case _:
                return Error(Fault.of(BlenderReason.ABSENT, repr(call.context.scene), member=OWNERSHIP.key))

    return members.source(call.context).bind(dropped)


def render_set_lock(members: Members, call: Call[Empty]) -> Outcome[Answer]:
    """Toggles the sync lock of the active member."""
    return toggled(members, call.context)


# --- [CONFIRMATIONS]


def _sync_message(category: Category, members: Members, context: bpy.types.Context, _properties: bpy.types.OperatorProperties) -> Outcome[str]:
    """States the settings a category sync writes across unlocked members."""
    return previewed(members, category, context).map(lambda rows: f"Writes {sum(len(edit.after) for _, edits in rows for edit in edits)} {category.title} settings across {len(rows)} render sets")


def _category_preset(category: Category) -> Preset[Snapshot]:
    """Preset of one category's settings."""
    return Preset(key=category.key, form=Snapshot)


# --- [CONVERSION]


def converted(members: Members, form: Form, context: bpy.types.Context) -> Outcome[Option[tuple[str, ...]]]:
    """Converts render set form, removing created owners on failure, `Nothing` for current form."""

    def switched(current: Sets, rollback: ExitStack) -> Outcome[Option[tuple[str, ...]]]:
        def committed(names: tuple[str, ...], old: tuple[bpy.types.Scene, ...] = ()) -> Outcome[tuple[str, ...]]:
            def replaced(scene: bpy.types.Scene) -> Outcome[tuple[str, ...]]:
                shown(scene, old)
                return accumulated(map(removed, old)).map(lambda _: names)

            return assigned(current, {("form",): form.value}).map(lambda _: rollback.pop_all()).bind(lambda _: members.source(context).bind(replaced) if old else Ok(names))

        def layered(scenes: tuple[bpy.types.Scene, ...]) -> Outcome[tuple[str, ...]]:
            source, *copies = scenes
            planned = tuple((scene, layer, scene.name if len(layers) == 1 else f"{scene.name} {layer.name}") for scene in copies for layers in (used(scene),) for layer in layers)

            def placed(scene: bpy.types.Scene, layer: bpy.types.ViewLayer, name: str) -> Outcome[tuple[bpy.types.Scene, bpy.types.ViewLayer, bpy.types.ViewLayer]]:
                def created() -> tuple[bpy.types.Scene, bpy.types.ViewLayer, bpy.types.ViewLayer]:
                    target = source.view_layers.new(name)
                    rollback.callback(removed, (source, target))
                    return scene, layer, target

                return catch(created, exception=RuntimeError)().map_error(partial(Fault.of, BlenderReason.REFUSED, f"{source!r}.view_layers.new({name!r})"))

            def filled(scene: bpy.types.Scene, layer: bpy.types.ViewLayer, target: bpy.types.ViewLayer) -> Outcome[str]:
                return (
                    Snapshot
                    .capture(layer)
                    .bind(lambda snapshot: snapshot.apply(target).outcome)
                    .bind(lambda _: samples(scene, layer).map(lambda count: _SAMPLES.write(target, count).map(lambda _: None)).default_value(Ok(None)))
                    .map(lambda _: target.name)
                )

            return accumulated(starmap(placed, planned)).bind(lambda targets: accumulated(starmap(filled, targets))).bind(lambda names: committed(names, tuple(copies)))

        def copied(source: bpy.types.Scene) -> Outcome[tuple[str, ...]]:
            def each(name: str) -> Outcome[str]:
                def retained(scene: bpy.types.Scene) -> Outcome[str]:
                    rollback.callback(removed, scene)
                    return accumulated(removed((scene, layer)) for layer in tuple(scene.view_layers) if layer.name != name).map(lambda _: scene.name)

                return added(members, source, name).bind(retained)

            return accumulated(each(layer.name) for layer in used(source)).bind(committed)

        match Form(current.form), form:
            case Form.COPIES, Form.LAYERS:
                return members.copies(context).bind(layered).map(Some)
            case Form.LAYERS, Form.COPIES:
                return members.source(context).bind(copied).map(Some)
            case _:
                return Ok(Nothing)

    def held(manager: bpy.types.WindowManager) -> Outcome[Option[tuple[str, ...]]]:
        with ExitStack() as rollback:
            return switched(members.sets.struct(manager), rollback)

    return rooted(bpy.types.WindowManager, context).bind(held)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Converting",
    "RenderSet",
    "converted",
    "render_set_add",
    "render_set_adopt",
    "render_set_convert",
    "render_set_lock",
    "render_set_remove",
    "render_set_restore",
    "render_set_sync",
    "render_set_unsync",
]
