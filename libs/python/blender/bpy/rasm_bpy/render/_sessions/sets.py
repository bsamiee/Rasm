"""Render sets, commands, menus, settings, and conversion between copied scenes and view layers."""

from collections.abc import Callable, Generator, Mapping
from contextlib import contextmanager, ExitStack
from functools import cached_property, partial
from itertools import starmap
from types import MappingProxyType
from typing import ClassVar, TYPE_CHECKING

import bpy
from expression import Option, Some
from expression.extra.result import catch
import msgspec

from rasm_bpy.commands.invocations import Popup, Suppressible
from rasm_bpy.commands.operators import Answer, BoundBody, Call, confirmed, Done, Empty, Operator, PopupKind, PopupTarget, Unchanged
from rasm_bpy.data._extension.events import _updated
from rasm_bpy.data._extension.identity import Identity, Key
from rasm_bpy.data._extension.registration import Acquisitions, Handler, Registrable
from rasm_bpy.data._extension.reports import Refusal
from rasm_bpy.data._rna.descriptors import Descriptor, written
from rasm_bpy.data._rna.paths import _parsed, rooted
from rasm_bpy.data._rna.properties import enum_choices
from rasm_bpy.data._rna.snapshots import assigned, Snapshot
from rasm_bpy.render._sessions.engines import _samples
from rasm_bpy.render._sessions.members import (
    _added,
    _KEY,
    _MemberScope,
    _OWNERSHIP,
    _previewed,
    _propagated,
    _removed,
    _shown,
    Category,
    Form,
    Members,
    Membership,
    render_set_add,
    render_set_adopt,
    render_set_lock,
    render_set_remove,
    render_set_restore,
    render_set_sync,
    render_set_unsync,
    SceneMembership,
    Sets,
)
from rasm_bpy.render._sessions.setups import _SETTING_LOCKS, _SetupScope, Capture
from rasm_bpy.render._view_layers.passes import _used
from rasm_bpy.settings._attachments import Attachment, Recovery, TextRecord
from rasm_bpy.settings._presets import Preset
from rasm_bpy.ui._elements.menus import Dropdown, Menu, PresetMenu
from rasm_bpy.ui._layout.lists import _ItemList
from rasm_bpy.ui._layout.members import Member
from rasm_bpy.ui._layout.rows import Layout, Opens, OperatorButton, Property, RowContent
from rasm_bpy.ui._layout.sections import Section
from rasm_bpy.ui._layout.tables import _SettingsTable
from rasm_bpy.ui._layout.values import _menu_key, Captured
from rasm_records import accumulated, Fault, Nothing, Ok, Outcome, paired

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [MODELS] ---------------------------------------------------------------------------


class Converting(bpy.types.PropertyGroup):
    """Target form of render set conversion."""

    form: bpy.props.EnumProperty(name="Render Sets", items=enum_choices(Form), default=Form.COPIES.name)  # ty: ignore[invalid-type-form]


# --- [SERVICES] -------------------------------------------------------------------------


class RenderSet[C: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Render set stores, commands binding the set's members on call, menus, and section over the settings its categories sync and what its capture holds."""

    # --- [STATE]

    key: ClassVar[Key] = _KEY
    record: ClassVar[TextRecord[Sets]] = TextRecord(key="render_set", form=Sets)
    scenes: ClassVar[Attachment[SceneMembership]] = Attachment(key="render_set", form=SceneMembership, owner=bpy.types.Scene)
    layers: ClassVar[Attachment[Membership]] = Attachment(key="render_set", form=Membership, owner=bpy.types.ViewLayer)
    capture: Capture[C]
    categories: tuple[Category, ...] = ()

    # --- [BINDING]

    def bound(self, identity: Identity) -> Outcome[Members]:
        """Resolves stores and each category's settings on its root."""
        selections = accumulated(
            accumulated(
                (_parsed(setting) if isinstance(setting, str) else Ok(setting)).bind(lambda path: Descriptor.resolve(category.root, path, bpy.types.Property)) for setting in category.settings
            ).map(lambda found: (category, found))
            for category in self.categories
        )
        return paired(self.record.attachment.bound(identity), self.scenes.bound(identity), self.layers.bound(identity), selections).map(
            lambda joined: Members(
                identity=identity, sets=joined[0], scenes=joined[1], layers=joined[2], categories=MappingProxyType(dict(joined[3])), recovery=Recovery.before_after(identity, "render_set_sync")
            )
        )

    # --- [COMMANDS]

    @cached_property
    def commands(self) -> Mapping[Callable[[Members, Call[Empty]], Outcome[Answer]] | Callable[[Members, Call[Converting]], Outcome[Answer]], Operator[Empty] | Operator[Converting]]:
        """Set commands keyed by their step, each binding the set's members on call."""
        icons: tuple[tuple[Callable[[Members, Call[Empty]], Outcome[Answer]], rna_enums.IconItems], ...] = (
            (render_set_add, "ADD"),
            (render_set_adopt, "IMPORT"),
            (render_set_restore, "SCENE_DATA"),
            (render_set_unsync, "LOOP_BACK"),
            (render_set_remove, "REMOVE"),
        )

        def message(members: Members, context: bpy.types.Context, _properties: Converting) -> Outcome[tuple[str, ...]]:
            return members.members(context).map(lambda owners: (f"Converts {', '.join(owner.name for owner in owners)} to the form below",))

        converting = Operator.of(
            Converting, icon=Some("ARROW_LEFTRIGHT"), invoke=Some(Popup(partial(bpy.types.WindowManager.invoke_props_dialog))), draw=Some(partial(confirmed, self.bound, message, ("form",)))
        )
        locking = Operator.of(Empty, icon=Some("LOCKED"), invoke=Some(Suppressible[Empty](message=lambda _context, _record: Ok("Unlocking never-synced member copies every category from source"))))
        return MappingProxyType({
            **{step: Operator.of(Empty, icon=Some(icon))(BoundBody(self.bound, step)) for step, icon in icons},
            render_set_convert: converting(BoundBody(self.bound, render_set_convert)),
            render_set_lock: locking(BoundBody(self.bound, render_set_lock)),
        })

    @cached_property
    def syncs(self) -> tuple[Operator[Empty], ...]:
        """One sync command per category, keyed by its category and confirmed with its change counts."""

        def message(category: Category, members: Members, context: bpy.types.Context, _properties: Empty) -> Outcome[tuple[str, ...]]:
            return _previewed(members, category, context).map(
                lambda rows: (f"Writes {sum(len(edit.after) for _, edits in rows for edit in edits)} {category.title} settings across {len(rows)} render sets",)
            )

        return tuple(
            Operator.of(
                Empty,
                icon=Some("FILE_REFRESH"),
                named=Some(f"render_set_sync_{category.key}"),
                invoke=Some(Popup(partial(bpy.types.WindowManager.invoke_props_dialog))),
                draw=Some(partial(confirmed, self.bound, partial(message, category), ())),
            )(BoundBody(self.bound, partial(render_set_sync, category)))
            for category in self.categories
        )

    @property
    def listed(self) -> tuple[Operator[Empty] | Operator[Converting], ...]:
        """Commands in menu order."""
        commands = self.commands
        return (commands[render_set_add], commands[render_set_adopt], commands[render_set_restore], *self.syncs, commands[render_set_unsync], commands[render_set_convert])

    # --- [SECTIONS]

    def rows(self, members: Members) -> tuple[RowContent, ...]:
        """Capture rows, member lists, and one settings matrix per category with its setting locks."""
        scope = _SetupScope(members=members, capture=self.capture)
        return (
            *self.capture.rows(members, scope),
            Property(target=Member(root=bpy.types.WindowManager, path=(members.sets.name, "across"))),
            _ItemList[SceneMembership, bpy.types.Scene](
                key="render_set_scenes",
                collection=Member(root=bpy.types.BlendData, path=("scenes",)),
                active=Some(Member(root=bpy.types.Window, path=("scene",))),
                attachment=Some(self.scenes),
                where=Some(lambda context, _ui, item: members.form(context).bind(lambda form: members.copies(context).map(lambda scenes: item in scenes) if form is Form.COPIES else Ok(value=False))),
                side=tuple(OperatorButton(target=self.commands[step], keywords={"text": ""}) for step in (render_set_add, render_set_remove, render_set_lock)),
            ),
            _ItemList[Membership, bpy.types.ViewLayer](
                key="render_set_layers",
                collection=Member(root=bpy.types.Scene, path=("view_layers",)),
                active=Some(Member(root=bpy.types.Window, path=("view_layer",))),
                attachment=Some(self.layers),
                cells=(Property(target=Member(root=bpy.types.ViewLayer, path=("use",)), keywords={"text": ""}),),
                where=Some(lambda context, _ui, _item: members.form(context).map(lambda form: form is Form.LAYERS)),
            ),
            *(
                Section(
                    key=category.key,
                    header=(Opens(target=PopupTarget(_menu_key(category.preset), PopupKind.MENU), keywords={"text": ""}, icon=Some("PRESET")), OperatorButton(target=sync, keywords={"text": ""})),
                    rows=(
                        _SettingsTable(
                            scope=_MemberScope(members=members, owner_type=category.root, unlocked=False),
                            settings=tuple(descriptor.path for descriptor in members.categories[category]),
                            compound=((bpy.types.CurveMapping, lambda layout, owner, key: layout.template_curve_mapping(owner, key)),),
                            action=Some(lambda identity, setting: _SETTING_LOCKS.drawn(setting.path, identity)),
                        ),
                    ),
                )
                for category, sync in zip(self.categories, self.syncs)
            ),
        )

    @property
    def section(self) -> Section:
        """Render set section: setup presets in its header, then rows over the members the drawing identity binds, the set registering what they read."""
        return Section(
            key=self.key,
            header=(Opens(target=PopupTarget(_menu_key(Preset(key=_KEY, form=self.capture.form)), PopupKind.MENU), keywords={"text": ""}, icon=Some("PRESET")),),
            rows=(Layout(builder=Some(lambda identity: self.bound(identity).map(self.rows)), prerequisites=(self,)),),
        )

    # --- [REGISTRATION]

    @property
    def prerequisites(self) -> tuple[Registrable, ...]:
        """Set, scene, and layer stores binding reads."""
        return self.record, self.scenes, self.layers

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[Outcome[Acquisitions]]:
        """Holds deferred edit transfer while registered, with the commands, the rows the section draws, and the preset menus over the extension's members."""
        with ExitStack() as stack:

            def joined(members: Members) -> Acquisitions:
                deferred = stack.enter_context(identity.reports.deferred(partial(_propagated, members)))

                def edited(scene: bpy.types.Scene, _updates: tuple[bpy.types.DepsgraphUpdate, ...]) -> Outcome[None]:
                    context = bpy.context

                    def handed(record: Sets) -> None:
                        if record.across and scene == context.scene:
                            for owner in (scene, context.view_layer):
                                if owner is not None and any(isinstance(owner, category.root) for category in members.categories):
                                    deferred(owner)

                    return rooted(bpy.types.WindowManager, context).map(lambda manager: handed(members.sets.struct(manager)))

                listing = _SetupScope(members=members, capture=self.capture).listing
                return (
                    _OWNERSHIP,
                    _SETTING_LOCKS.operator,
                    *self.commands.values(),
                    *self.syncs,
                    Layout(rows=self.rows(members)),
                    PresetMenu(source=listing),
                    Handler(handlers=bpy.app.handlers.depsgraph_update_post, body=partial(_updated, (bpy.types.Scene,), edited)),
                    Menu(
                        key=self.key,
                        description="Create, sync, restore, and convert render sets",
                        body=Dropdown(
                            entries=(
                                *(OperatorButton(target=placed) for placed in self.listed),
                                Opens(target=PopupTarget(listing.key, PopupKind.MENU)),
                                *(Opens(target=PopupTarget(_menu_key(category.preset), PopupKind.MENU)) for category in self.categories),
                            )
                        ),
                    ),
                    *(
                        PresetMenu(
                            source=Captured(
                                preset=category.preset,
                                scope=_MemberScope(members=members, owner_type=category.root, unlocked=True),
                                selection=tuple(descriptor.path for descriptor in members.categories[category]),
                            )
                        )
                        for category in self.categories
                    ),
                )

            yield self.bound(identity).map(joined)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [STEPS]


def render_set_convert(members: Members, call: Call[Converting]) -> Outcome[Answer]:
    """Transfers render sets between copied scenes and view layers."""
    return _converted(members, Form[call.properties.form], call.context).map(
        lambda names: names.map(lambda made: Done(notes=tuple(("INFO", f"Created {name}") for name in made))).default_value(Unchanged(reason=f"Render sets use {call.properties.form.lower()}"))
    )


# --- [CONVERSIONS]


def _converted(members: Members, form: Form, context: bpy.types.Context) -> Outcome[Option[tuple[str, ...]]]:
    """Converts render set form, removing created owners on failure, `Nothing` for current form."""

    def switched(current: Sets, rollback: ExitStack) -> Outcome[Option[tuple[str, ...]]]:
        def committed(names: tuple[str, ...], old: tuple[bpy.types.Scene, ...] = ()) -> Outcome[tuple[str, ...]]:
            def replaced(scene: bpy.types.Scene) -> Outcome[tuple[str, ...]]:
                _shown(scene, old)
                return accumulated(map(_removed, old)).map(lambda _: names)

            return assigned(current, {("form",): form.name}).map(lambda _: rollback.pop_all()).bind(lambda _: members.source(context).bind(replaced) if old else Ok(names))

        def layered(scenes: tuple[bpy.types.Scene, ...]) -> Outcome[tuple[str, ...]]:
            source, *copies = scenes
            planned = tuple((scene, layer, scene.name if len(layers) == 1 else f"{scene.name} {layer.name}") for scene in copies for layers in (_used(scene),) for layer in layers)

            def placed(scene: bpy.types.Scene, layer: bpy.types.ViewLayer, name: str) -> Outcome[tuple[bpy.types.Scene, bpy.types.ViewLayer, bpy.types.ViewLayer]]:
                def created() -> tuple[bpy.types.Scene, bpy.types.ViewLayer, bpy.types.ViewLayer]:
                    target = source.view_layers.new(name)
                    rollback.callback(_removed, (source, target))
                    return scene, layer, target

                return catch(created, exception=RuntimeError)().map_error(partial(Fault.of, Refusal.REFUSED, f"{source!r}.view_layers.new({name!r})"))

            def filled(scene: bpy.types.Scene, layer: bpy.types.ViewLayer, target: bpy.types.ViewLayer) -> Outcome[str]:
                return (
                    Snapshot
                    .capture(layer)
                    .bind(lambda snapshot: snapshot.apply(target).outcome)
                    .bind(lambda _: _samples(scene, layer).map(lambda count: written(target, ("samples",), count).map(lambda _: None)).default_value(Ok(None)))
                    .map(lambda _: target.name)
                )

            return accumulated(starmap(placed, planned)).bind(lambda targets: accumulated(starmap(filled, targets))).bind(lambda names: committed(names, tuple(copies)))

        def copied(source: bpy.types.Scene) -> Outcome[tuple[str, ...]]:
            def each(name: str) -> Outcome[str]:
                def retained(scene: bpy.types.Scene) -> Outcome[str]:
                    rollback.callback(_removed, scene)
                    return accumulated(_removed((scene, layer)) for layer in tuple(scene.view_layers) if layer.name != name).map(lambda _: scene.name)

                return _added(members, source, name).bind(retained)

            return accumulated(each(layer.name) for layer in _used(source)).bind(committed)

        match Form[current.form], form:
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

__all__ = ["Converting", "RenderSet", "render_set_convert"]
