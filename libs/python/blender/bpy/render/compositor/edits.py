"""Edit commands of the effects an extension offers: adding by kind or from files, moving, removing, and resetting entries, the add menu, and the handlers keeping entries current."""

from collections.abc import Callable, Generator
from contextlib import contextmanager
from functools import cached_property, partial, update_wrapper
from typing import Concatenate, Final

import bpy
from expression import Nothing, Ok, Option, Some
import msgspec

from blender.bpy.commands.calls import Placement
from blender.bpy.commands.invocations import Open
from blender.bpy.commands.operators import Answer, Call, Done, Empty, Operator, Unchanged
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.mirrors import choices
from blender.bpy.data.rna.paths import rooted
from blender.bpy.extension.events import Handler, Subscription, updated
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderReason
from blender.bpy.files.interchange import dropped_in, Interchange
from blender.bpy.render.compositor.blending import Entry
from blender.bpy.render.compositor.effects import CompositorEffect, Named, Opened
from blender.bpy.render.compositor.entries import EffectRows, EXECUTING, members
from blender.bpy.render.compositor.inputs import defaulted, FRAMING, hold, WORKING_SPACE
from blender.bpy.render.outputs.results import produced, PRODUCTIONS
from blender.bpy.ui.layout.rows import OperatorButton
from blender.bpy.ui.surfaces.menus import Dropdown, Menu, Submenus
from blender.records.faults import accumulated, Fault, Outcome
from blender.records.fields import Case
from blender.records.trees import Toward

# --- [CONSTANTS] ------------------------------------------------------------------------

_RANK: Final[Key] = "RANK"

# --- [MODELS] ---------------------------------------------------------------------------


class Shift(Named):
    """Named entry and requested move."""

    toward: bpy.props.EnumProperty(  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
        name="Toward",
        items=choices((
            *((step.name, bpy.path.display_name(step.name.lower()), f"Move effect one place {step.name.lower()} the stack") for step in Toward),
            (_RANK, "Processing Order", "Move effect into processing order"),
        )),
        default=_RANK,
    )


class Choice(bpy.types.PropertyGroup):
    """Offered effect a command adds, its items the offering rows."""

    kind: bpy.props.EnumProperty(name="Effect", description="Offered effect", items=())  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


# --- [SERVICES] -------------------------------------------------------------------------


class EffectEdits[R: Case](msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Edit commands, add menu, and upkeep handlers over the effects a set of rows offers."""

    rows: EffectRows[R]

    # --- [COMMANDS]

    @cached_property
    def choice(self) -> type[Choice]:
        """Form choosing one offered effect, an item per row under its key, title, and record documentation."""
        return chosen(self.rows.rows)

    @cached_property
    def adding(self) -> Operator[Choice]:
        """Command adding the chosen effect at its rank."""
        return Operator(form=self.choice, run=bound(add_effect, self.rows))

    @cached_property
    def opening(self) -> tuple[tuple[CompositorEffect[R], Operator[Empty]], ...]:
        """Per-row commands of effects opened from files, the drop handler's operators."""

        def command(row: CompositorEffect[R], opened: Opened[R]) -> Operator[Empty]:
            return Operator(form=Empty, run=bound(bound(open_effect, self.rows), row), named=Some(f"add_{row.kind}"), invoke=Some(Open(extensions=lambda _: opened.extensions())))

        return tuple((row, command(row, opened)) for row in self.rows.rows for opened in (row.source,) if isinstance(opened, Opened))

    @cached_property
    def moving(self) -> Operator[Shift]:
        """Command moving an entry one step or into its rank."""
        return Operator(form=Shift, run=bound(move_effect, self.rows))

    @cached_property
    def removing(self) -> Operator[Named]:
        """Command removing an owned entry."""
        return Operator(form=Named, run=bound(remove_effect, self.rows))

    @cached_property
    def resetting(self) -> Operator[Named]:
        """Command resetting an entry to its source record and interface defaults."""
        return Operator(form=Named, run=bound(reset_effect, self.rows))

    @property
    def menu(self) -> Menu:
        """Add menu of every effect in rank order, one submenu per phase group where every phase holds one."""
        openers = {row.kind: operator for row, operator in self.opening}
        buttons = tuple(
            (phase.group, OperatorButton(target=openers.get(row.kind, Placement(self.adding, {"kind": row.kind})), keywords={"text": row.title}))
            for phase in self.rows.phases
            for member in phase.members
            for row in members(member)
        )
        filed = tuple((group, button) for held, button in buttons for group in held.to_list())
        key = self.adding.key
        return Menu(
            key=key,
            title=bpy.path.display_name(key),
            description=self.adding.description,
            body=Submenus(entries=filed) if len(filed) == len(buttons) else Dropdown(entries=tuple(button for _, button in buttons), search=True),
        )

    # --- [LIFETIME]

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[Outcome[Acquisitions]]:
        """Yields offered effects, commands, handlers, and subscriptions with deferred refresh, and the working-space follow-through of entry colors."""

        def refreshed(handle: Handle[bpy.types.Scene], groups: tuple[int, ...]) -> Outcome[Option[str]]:
            def reconciled(row: CompositorEffect[R], scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect) -> Outcome[bool]:
                return row.read(identity, entry).bind(lambda record: row.write(identity, scene, entry, record))

            def update(scene: bpy.types.Scene) -> Outcome[Option[str]]:
                return (
                    Ok(Nothing)
                    if scene.session_uid in identity.held[EXECUTING]
                    else self.rows.refreshed(identity, scene).bind(
                        lambda changed: accumulated(
                            reconciled(held.row, scene, held.entry)
                            if held.row.request.is_some() or any(group.session_uid in groups for group in Option.of_optional(held.entry.node_group).to_list())
                            else held.row.refreshed(identity, scene, held.entry)
                            for held in self.rows.owned(identity, scene)
                        ).map(lambda outcomes: Some("Refresh Effects") if changed or any(outcomes) else Nothing)
                    )
                )

            return handle.resolve().bind(update)

        with identity.reports.deferred(refreshed) as defer:

            def rendered(scene: bpy.types.Scene, source: Option[bpy.types.Scene] = Nothing) -> Outcome[None]:
                return (
                    Ok(None)
                    if scene.session_uid in identity.held[EXECUTING]
                    else accumulated(
                        source
                        .map(lambda evaluated: held.row.named(identity, evaluated, held.entry.name))
                        .default_value(Ok(held.entry))
                        .bind(lambda reading: held.row.read(identity, reading))
                        .bind(lambda record: held.row.write(identity, scene, held.entry, record, source=source))
                        if source.is_some() or held.row.request.is_some()
                        else held.row.refreshed(identity, scene, held.entry)
                        for held in self.rows.owned(identity, scene)
                    ).map(lambda _: None)
                )

            def loaded() -> Outcome[None]:
                return accumulated(
                    self.rows
                    .refreshed(identity, scene)
                    .bind(lambda _: rendered(scene))
                    .bind(lambda _: accumulated(hold(identity, scene, row.kind, row.holds) for row in self.rows.held(identity, scene)))
                    for scene in bpy.data.scenes
                ).map(lambda _: None)

            def imaged(scene: bpy.types.Scene, images: frozenset[bpy.types.Image], trees: tuple[bpy.types.ID, ...] = ()) -> None:
                users = bpy.data.user_map(subset=images, value_types={"NODETREE"}) if images else {}
                defer(Handle.of(scene), tuple(dict.fromkeys(tree.session_uid for tree in (*trees, *(tree for held in users.values() for tree in held)))))

            def edited(scene: bpy.types.Scene, updates: tuple[bpy.types.DepsgraphUpdate, ...]) -> Outcome[None]:
                imaged(
                    scene,
                    frozenset(image for update in updates if isinstance(update.id, bpy.types.Image) for image in (update.id.original,) if isinstance(image, bpy.types.Image)),
                    tuple(update.id for update in updates if isinstance(update.id, bpy.types.CompositorNodeTree)),
                )
                return Ok(None)

            def retagged() -> Outcome[None]:
                for scene in bpy.data.scenes:
                    imaged(scene, frozenset(bpy.data.images))
                return Ok(None)

            def invalidated(_scene: bpy.types.Scene) -> Outcome[None]:
                for image in bpy.data.images:
                    if image.type in {"RENDER_RESULT", "COMPOSITING"}:
                        produced(identity, image, Nothing)
                return Ok(None)

            def built() -> Acquisitions:
                return (
                    PRODUCTIONS,
                    WORKING_SPACE,
                    *self.rows.rows,
                    self.moving,
                    self.removing,
                    self.resetting,
                    self.adding,
                    *(operator for _, operator in self.opening),
                    *(
                        Interchange(
                            key=row.kind,
                            title=Some(row.title),
                            extensions=opens.extensions(),
                            importing=Some(operator),
                            drop=lambda context: dropped_in(frozenset({"NODE_EDITOR", "IMAGE_EDITOR"}), context),
                        )
                        for row, operator in self.opening
                        for opens in (row.source,)
                        if isinstance(opens, Opened)
                    ),
                    Handler(handlers=bpy.app.handlers.load_post, body=loaded),
                    Handler(handlers=bpy.app.handlers.render_init, body=invalidated),
                    Handler(handlers=bpy.app.handlers.depsgraph_update_post, body=partial(updated, (bpy.types.Scene, bpy.types.Action, bpy.types.CompositorNodeTree, bpy.types.Image), edited)),
                    Handler(handlers=bpy.app.handlers.frame_change_post, body=lambda scene, graph: rendered(scene, Some(graph.scene_eval))),
                    *(
                        Subscription(owner=owner, setting=setting, body=lambda: accumulated(rendered(scene) for scene in bpy.data.scenes).map(lambda _: None))
                        for owner, setting in dict.fromkeys((*FRAMING.reads, *(read for row in self.rows.rows for read in row.reads)))
                    ),
                    *(Subscription(owner=bpy.types.Image, setting=bpy.types.Image.bl_rna.properties[name], body=retagged) for name in ("alpha_mode", "use_half_precision")),
                )

            yield Ok(built())


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [COMMANDS]


def add_effect[R: Case](rows: EffectRows[R], call: Call[Choice]) -> Outcome[Answer]:
    """Add the chosen effect at its rank."""
    return (
        rooted(bpy.types.Scene, call.context)
        .bind(
            lambda scene: (
                Option
                .of_optional(rows.keyed.get(call.properties.kind))
                .to_result_with(partial(Fault.of, BlenderReason.ABSENT, repr(tuple(rows.keyed)), member=call.properties.kind))
                .bind(lambda row: row.seeded(scene).bind(lambda record: rows.placed(call.identity, scene, row, Entry(record=record))))
            )
        )
        .map(lambda _: Done())
    )


def open_effect[R: Case](rows: EffectRows[R], row: CompositorEffect[R], call: Call[Empty]) -> Outcome[Answer]:
    """Add the effect at its rank, one entry per chosen file."""
    opened = row.source
    records = tuple(Ok(opened.of(str(path))) for path in call.paths) if isinstance(opened, Opened) else ()
    return (
        rooted(bpy.types.Scene, call.context)
        .bind(lambda scene: accumulated(record.bind(lambda held: rows.placed(call.identity, scene, row, Entry(record=held))) for record in records))
        .map(lambda _: Done())
    )


def reset_effect[R: Case](rows: EffectRows[R], call: Call[Named]) -> Outcome[Answer]:
    """Reset effect to defaults."""
    return (
        rooted(bpy.types.Scene, call.context)
        .bind(
            lambda scene: rows.found(call.identity, scene, call.properties.name).bind(
                lambda held: defaulted(call.identity, held.entry).bind(
                    lambda reset: held.row.seeded(scene).bind(lambda effect: held.row.write(call.identity, scene, held.entry, Entry(record=effect))).map(lambda changed: changed or reset)
                )
            )
        )
        .map(lambda changed: Done() if changed else Unchanged(reason=f"{call.properties.name} holds defaults"))
    )


def move_effect[R: Case](rows: EffectRows[R], call: Call[Shift]) -> Outcome[Answer]:
    """Move effect up, down, or to processing order."""
    return (
        rooted(bpy.types.Scene, call.context)
        .bind(lambda scene: rows.moved(call.identity, scene, call.properties.name, Option.of_optional(Toward.__members__.get(call.properties.toward))))
        .map(lambda changed: Done() if changed else Unchanged(reason=f"{call.properties.name} holds requested position"))
    )


def remove_effect[R: Case](rows: EffectRows[R], call: Call[Named]) -> Outcome[Answer]:
    """Remove compositor effect."""
    return rooted(bpy.types.Scene, call.context).bind(lambda scene: rows.removed(call.identity, scene, call.properties.name)).map(lambda _: Done())


# --- [FORMS]


def chosen[R: Case](rows: tuple[CompositorEffect[R], ...]) -> type[Choice]:
    """Choice form whose `kind` items are the rows, each under its key, title, and record documentation, values fixed per key."""
    items = choices((row.kind, row.title, row.description) for row in rows)

    class Chosen(Choice):
        kind: bpy.props.EnumProperty(name="Effect", description="Offered effect", items=items)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]

    return Chosen


def bound[A, **P, T](body: Callable[Concatenate[A, P], T], argument: A) -> Callable[P, T]:
    """Binds the leading argument of a command body, keeping its name and docstring for the command it registers as."""
    return update_wrapper(partial(body, argument), body)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Choice", "EffectEdits", "Shift", "add_effect", "bound", "chosen", "move_effect", "open_effect", "remove_effect", "reset_effect"]
