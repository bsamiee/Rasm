"""Render sets of copied scenes or view layers: the set service with its commands, section, menus, and form conversion."""

from collections.abc import Callable, Generator, Mapping
from contextlib import contextmanager, ExitStack
from functools import cached_property, partial
from itertools import starmap
from types import MappingProxyType
from typing import ClassVar, Final, TYPE_CHECKING

import bpy
from expression import Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.calls import Called, Opened
from blender.bpy.commands.invocations import Popup, Suppressible
from blender.bpy.commands.operators import Answer, Call, confirmed, Done, Empty, Operator, Over, Unchanged
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.data.rna.paths import parsed, rooted
from blender.bpy.data.rna.snapshots import assigned, Snapshot
from blender.bpy.extension.events import Handler, updated
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Registrable
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
    render_set_add,
    render_set_adopt,
    render_set_lock,
    render_set_remove,
    render_set_restore,
    render_set_sync,
    render_set_unsync,
    SceneMembership,
    Sets,
    shown,
)
from blender.bpy.render.sessions.setups import Capture, SETTING_LOCKS, SetupScope
from blender.bpy.render.view_layers.passes import used
from blender.bpy.settings.attachments import Attachment, Recovery, TextRecord
from blender.bpy.settings.presets import Preset
from blender.bpy.ui.layout.lists import ItemList
from blender.bpy.ui.layout.rows import Layout, Member, Opens, OperatorButton, Property, RowContent
from blender.bpy.ui.layout.sections import Section
from blender.bpy.ui.layout.tables import Matrix
from blender.bpy.ui.layout.values import Captured, menu_key
from blender.bpy.ui.surfaces.menus import Dropdown, Menu, PresetMenu
from blender.records.faults import accumulated, Fault, Outcome, paired, Reason

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
                (parsed(setting) if isinstance(setting, str) else Ok(setting)).bind(lambda path: Descriptor.resolve(category.root, path, bpy.types.Property)) for setting in category.settings
            ).map(lambda found: (category, found))
            for category in self.capture.categories
        )
        return paired(self.record.attachment.bound(identity), self.scenes.bound(identity), self.layers.bound(identity), selections, Recovery.endpoints(identity, "render_set_sync")).map(
            lambda joined: Members(identity=identity, sets=joined[0], scenes=joined[1], layers=joined[2], categories=MappingProxyType(dict(joined[3])), recovery=joined[4])
        )

    # --- [COMMANDS]

    @cached_property
    def commands(self) -> Mapping[Callable[..., Outcome[Answer]], Operator[Empty] | Operator[Converting]]:
        """Set commands keyed by their step, each binding the set's members on call."""
        icons: tuple[tuple[Callable[[Members, Call[Empty]], Outcome[Answer]], rna_enums.IconItems], ...] = (
            (render_set_add, "ADD"),
            (render_set_adopt, "IMPORT"),
            (render_set_restore, "SCENE_DATA"),
            (render_set_unsync, "LOOP_BACK"),
            (render_set_remove, "REMOVE"),
        )

        def message(members: Members, context: bpy.types.Context, _properties: bpy.types.OperatorProperties) -> Outcome[tuple[str, ...]]:
            return members.members(context).map(lambda owners: (f"Converts {', '.join(owner.name for owner in owners)} to the form below",))

        converting = Operator.of(
            Converting, icon=Some("ARROW_LEFTRIGHT"), invoke=Some(Popup(partial(bpy.types.WindowManager.invoke_props_dialog))), draw=Some(partial(confirmed, self.bound, message, ("form",)))
        )
        locking = Operator.of(Empty, icon=Some("LOCKED"), invoke=Some(Suppressible[Empty](message=lambda _context, _record: "Unlocking never-synced member copies every category from source")))
        return MappingProxyType({
            **{step: Operator.of(Empty, icon=Some(icon))(Over(self.bound, step)) for step, icon in icons},
            render_set_convert: converting(Over(self.bound, render_set_convert)),
            render_set_lock: locking(Over(self.bound, render_set_lock)),
        })

    @cached_property
    def syncs(self) -> tuple[Operator[Empty], ...]:
        """One sync command per category, keyed by its category and confirmed with its change counts."""

        def message(category: Category, members: Members, context: bpy.types.Context, _properties: bpy.types.OperatorProperties) -> Outcome[tuple[str, ...]]:
            return previewed(members, category, context).map(
                lambda rows: (f"Writes {sum(len(edit.after) for _, edits in rows for edit in edits)} {category.title} settings across {len(rows)} render sets",)
            )

        return tuple(
            Operator.of(
                Empty,
                icon=Some("FILE_REFRESH"),
                named=Some(f"render_set_sync_{category.key}"),
                invoke=Some(Popup(partial(bpy.types.WindowManager.invoke_props_dialog))),
                draw=Some(partial(confirmed, self.bound, partial(message, category), ())),
            )(Over(self.bound, partial(render_set_sync, category)))
            for category in self.capture.categories
        )

    @property
    def listed(self) -> tuple[Operator[Empty] | Operator[Converting], ...]:
        """Commands in menu order."""
        commands = self.commands
        return (commands[render_set_add], commands[render_set_adopt], commands[render_set_restore], *self.syncs, commands[render_set_unsync], commands[render_set_convert])

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
                side=tuple(OperatorButton(target=self.commands[step], keywords={"text": ""}) for step in (render_set_add, render_set_remove, render_set_lock)),
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
                    header=(Opens(target=Opened(menu_key(category.preset), Called.MENU), keywords={"text": ""}, icon=Some("PRESET")), OperatorButton(target=sync, keywords={"text": ""})),
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
                    *self.commands.values(),
                    *self.syncs,
                    Layout(rows=self.rows(members)),
                    PresetMenu(source=listing),
                    Handler(handlers=bpy.app.handlers.depsgraph_update_post, body=partial(updated, (bpy.types.Scene,), edited)),
                    Menu(
                        key=self.key,
                        description="Create, sync, restore, and convert render sets",
                        body=Dropdown(
                            entries=(
                                *(OperatorButton(target=placed) for placed in self.listed),
                                Opens(target=Opened(listing.key, Called.MENU)),
                                *(Opens(target=Opened(menu_key(category.preset), Called.MENU)) for category in self.capture.categories),
                            )
                        ),
                    ),
                    *(
                        PresetMenu(
                            source=Captured(
                                preset=category.preset,
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


def render_set_convert(members: Members, call: Call[Converting]) -> Outcome[Answer]:
    """Transfers render sets between copied scenes and view layers."""
    return converted(members, Form(call.properties.form), call.context).map(
        lambda names: names.map(lambda made: Done(notes=tuple(("INFO", f"Created {name}") for name in made))).default_value(Unchanged(reason=f"Render sets use {call.properties.form}"))
    )


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

                return catch(created, exception=RuntimeError)().map_error(partial(Fault.of, Reason.REFUSED, f"{source!r}.view_layers.new({name!r})"))

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

__all__ = ["Converting", "RenderSet", "converted", "render_set_convert"]
