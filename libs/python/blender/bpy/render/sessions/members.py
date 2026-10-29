"""Render set membership: the file's set form, member scenes or view layers with their sync locks, category settings per root, category sync across unlocked members, and the set steps over them."""

from collections.abc import Iterable, Mapping
from enum import StrEnum
from functools import partial
from itertools import chain
from typing import Final

import bpy
from expression import Error, Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.operators import Answer, answered, Call, Done, Empty, tallied, Unchanged
from blender.bpy.data.ids.ownership import Mark, Ownership, Saved
from blender.bpy.data.rna.descriptors import Descriptor, Value
from blender.bpy.data.rna.mirrors import choices
from blender.bpy.data.rna.paths import RnaPath, rooted
from blender.bpy.data.rna.restoration import Endpoints
from blender.bpy.data.rna.snapshots import Applied, assigned, Snapshot
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.render.view_layers.passes import layer_of, used
from blender.bpy.settings.attachments import Attached, Recovery
from blender.bpy.settings.presets import Preset
from blender.records.faults import accumulated, Fault, Outcome, Reason
from blender.records.trees import Reparametrized, transfer

# --- [TYPES] ----------------------------------------------------------------------------

type Owner = bpy.types.Scene | bpy.types.ViewLayer


class Form(StrEnum):
    """Render set form, valued by its persisted enum identifier."""

    LAYERS = "Layers"
    COPIES = "Copies"


# --- [CONSTANTS] ------------------------------------------------------------------------

KEY: Final[Key] = "render_sets"
FORMS: Final = choices(((Form.LAYERS.value, Form.LAYERS.value, "View layers of one scene"), (Form.COPIES.value, Form.COPIES.value, "Scenes copied from the source scene")))
OWNERSHIP: Final = Ownership(kind=bpy.types.Scene, key="render_set", saved=Saved.ALWAYS, single=False)

# --- [MODELS] ---------------------------------------------------------------------------


class Sets(bpy.types.PropertyGroup):
    """File render set settings."""

    form: bpy.props.EnumProperty(name="Render Sets", description="Copied scenes or view layers of one scene", items=FORMS, default=Form.LAYERS.value)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    across: bpy.props.BoolProperty(name="Edit Across", description="Copy category edits to unlocked members")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Membership(bpy.types.PropertyGroup):
    """Sync lock of a member view layer, the host's `use` its render inclusion."""

    locked: bpy.props.BoolProperty(name="Locked")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class SceneMembership(Membership):
    """Sync lock and render inclusion of a member scene."""

    rendered: bpy.props.BoolProperty(name="Render", default=True)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Category(msgspec.Struct, frozen=True, kw_only=True):
    """Settings of one category on a scene or its view layers, each a typed path or a path the host states as text."""

    key: Key
    settings: tuple[RnaPath | str, ...]
    root: type[bpy.types.Scene] | type[bpy.types.ViewLayer] = bpy.types.Scene

    @property
    def title(self) -> str:
        """Display name of category key."""
        return bpy.path.display_name(self.key)

    @property
    def preset(self) -> Preset[Snapshot]:
        """Preset of the category's settings."""
        return Preset(key=self.key, form=Snapshot)


# --- [SERVICES] -------------------------------------------------------------------------


class Members(msgspec.Struct, frozen=True, kw_only=True):
    """Stores and resolved descriptors every set operation reads."""

    identity: Identity
    sets: Attached[Sets]
    scenes: Attached[SceneMembership]
    layers: Attached[Membership]
    categories: Mapping[Category, tuple[Descriptor, ...]]
    recovery: Recovery[Endpoints]

    # --- [MEMBERSHIP]

    def form(self, context: bpy.types.Context) -> Outcome[Form]:
        """Reads file render set form from window manager."""
        return rooted(bpy.types.WindowManager, context).map(lambda manager: Form(self.sets.struct(manager).form))

    def source(self, context: bpy.types.Context) -> Outcome[bpy.types.Scene]:
        """Returns marked source of active scene, else active scene."""

        def sourced(scene: bpy.types.Scene) -> bpy.types.Scene:
            match OWNERSHIP.marked(self.identity, scene).bind(lambda mark: mark.of).to_optional():
                case bpy.types.Scene() as source:
                    return source
                case _:
                    return scene

        return rooted(bpy.types.Scene, context).map(sourced)

    def copies(self, context: bpy.types.Context) -> Outcome[tuple[bpy.types.Scene, ...]]:
        """Returns source, then its member scenes in data order."""
        return self.source(context).map(lambda source: (source, *OWNERSHIP.serving(self.identity, source)))

    def members(self, context: bpy.types.Context, *, unlocked: bool = False) -> Outcome[tuple[Owner, ...]]:
        """Returns members of file form in list order, unlocked ones alone when `unlocked` is set."""
        return self.form(context).bind(lambda form: MemberScope(members=self, owner_type=bpy.types.Scene if form is Form.COPIES else bpy.types.ViewLayer, unlocked=unlocked).owners(context))

    def membership(self, owner: Owner) -> Membership:
        """Returns the membership group of a member scene or view layer."""
        return self.scenes.struct(owner) if isinstance(owner, bpy.types.Scene) else self.layers.struct(owner)

    def free[O: Owner](self, owners: Iterable[O]) -> tuple[O, ...]:
        """Returns unlocked owners in given order."""
        return tuple(owner for owner in owners if not self.membership(owner).locked)

    def rendered(self, context: bpy.types.Context) -> Outcome[tuple[tuple[bpy.types.Scene, Option[bpy.types.ViewLayer]], ...]]:
        """Returns dispatch scenes, paired with each used layer under layer form."""

        def dispatched(form: Form) -> Outcome[tuple[tuple[bpy.types.Scene, Option[bpy.types.ViewLayer]], ...]]:
            match form:
                case Form.COPIES:
                    return self.copies(context).map(lambda scenes: tuple((scene, Nothing) for scene in scenes if self.scenes.struct(scene).rendered))
                case Form.LAYERS:
                    return self.source(context).map(lambda scene: tuple((scene, Some(layer)) for layer in used(scene)))

        return self.form(context).bind(dispatched)

    # --- [SETTINGS]

    def selected(self, root: type[bpy.types.Scene] | type[bpy.types.ViewLayer]) -> tuple[Descriptor, ...]:
        """Resolved settings of every category on root."""
        return tuple(descriptor for category, descriptors in self.categories.items() if category.root is root for descriptor in descriptors)

    def captured(self, category: Category, context: bpy.types.Context) -> Outcome[Snapshot]:
        """Captures category from source scene or its layer named like active one."""

        def origin(scene: bpy.types.Scene) -> Outcome[Owner]:
            match category.root, context.view_layer:
                case root, _ if root is bpy.types.Scene:
                    return Ok(scene)
                case _, bpy.types.ViewLayer(name=name):
                    return layer_of(scene, name)
                case _, None:
                    return Error(Fault.of(Reason.ABSENT, "bpy.context", member="view_layer"))

        return self.source(context).bind(origin).bind(lambda source: Snapshot.capture(source, Some(self.categories[category])))


class MemberScope(msgspec.Struct, frozen=True, kw_only=True):
    """Owners selected by set form and sync locks."""

    members: Members
    owner_type: type[bpy.types.Scene] | type[bpy.types.ViewLayer]
    unlocked: bool

    def owners(self, context: bpy.types.Context) -> Outcome[tuple[Owner, ...]]:
        """Returns owners of the member scenes, unlocked ones alone when `unlocked` is set."""
        return (
            self
            .scenes(context)
            .map(lambda scenes: tuple(layer for scene in scenes for layer in scene.view_layers) if self.owner_type is bpy.types.ViewLayer else scenes)
            .map(lambda owners: self.members.free(owners) if self.unlocked else owners)
        )

    def scenes(self, context: bpy.types.Context) -> Outcome[tuple[bpy.types.Scene, ...]]:
        """Returns member scenes under copy form, else source scene."""
        return self.members.form(context).bind(lambda form: self.members.copies(context) if form is Form.COPIES else self.members.source(context).map(lambda scene: (scene,)))


# --- [OPERATIONS] -----------------------------------------------------------------------

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
            return catch(bpy.data.scenes.remove, exception=RuntimeError)(scene).map_error(partial(Fault.of, Reason.REFUSED, repr(scene)))
        case (scene, layer):
            return catch(scene.view_layers.remove, exception=RuntimeError)(layer).map_error(partial(Fault.of, Reason.REFUSED, repr(layer)))


def added(members: Members, source: bpy.types.Scene, name: str) -> Outcome[bpy.types.Scene]:
    """Copies source as retained member under name, the host suffixing a taken name."""

    def copied() -> Outcome[bpy.types.Scene]:
        scene: bpy.types.Scene = source.copy()  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        return Ok(scene)

    return OWNERSHIP.made(members.identity, copied, of=Some(source)).bind(lambda scene: assigned(scene, {("name",): name}).map(lambda _: scene))


def _dropped(source: bpy.types.Scene, scenes: tuple[bpy.types.Scene, ...]) -> Outcome[Done]:
    """Shows source in place of copied members, then removes them, noting each removal."""
    shown(source, scenes)
    edits = tuple(removed(scene).map(lambda _: name) for scene in scenes for name in (scene.name,))
    return tallied(edits, tuple(("INFO", f"Removed {name}") for edit in edits for name in edit.to_option().to_list()))


# --- [SYNC]


def transfers(
    members: Members, category: Category, context: bpy.types.Context, targets: tuple[Owner, ...]
) -> Outcome[tuple[tuple[Owner, Snapshot, tuple[Reparametrized[Value, RnaPath, RnaPath], ...]], ...]]:
    """Returns value edits per target from source category."""
    return members.captured(category, context).bind(
        lambda source: accumulated(Snapshot.capture(target, Some(members.categories[category])).map(lambda held: (target, held, transfer(held.tree, source.tree))) for target in targets)
    )


def previewed(members: Members, category: Category, context: bpy.types.Context) -> Outcome[tuple[tuple[str, tuple[Reparametrized[Value, RnaPath, RnaPath], ...]], ...]]:
    """Returns value edits per unlocked owner for sync confirmation."""
    return (
        MemberScope(members=members, owner_type=category.root, unlocked=True)
        .owners(context)
        .bind(lambda targets: transfers(members, category, context, targets))
        .map(lambda rows: tuple((target.name, edits) for target, _, edits in rows if edits))
    )


def written(members: Members, category: Category, context: bpy.types.Context, targets: tuple[Owner, ...]) -> Outcome[tuple[Applied, ...]]:
    """Applies source category, recording each target's pre-sync and synced values."""
    return members.captured(category, context).bind(
        lambda source: accumulated(members.recovery.recorded(target, ((target, source),), lambda ends: ends) for target in targets).map(lambda groups: tuple(chain.from_iterable(groups)))
    )


def propagated(members: Members, edited: Owner) -> Outcome[Option[str]]:
    """Copies category edits to other unlocked members in one undo step."""

    def wrote(others: tuple[Owner, ...]) -> Outcome[Option[str]]:
        return accumulated(
            Snapshot.capture(edited, Some(selected)).bind(lambda source: accumulated((applied := source.apply(other)).outcome.map(lambda _: applied) for other in others if other != edited))
            for category, selected in members.categories.items()
            if isinstance(edited, category.root)
        ).map(lambda outcomes: Some("Edit Across Render Sets") if any(applied.written for group in outcomes for applied in group) else Nothing)

    return members.members(bpy.context, unlocked=True).bind(wrote)


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
        return _dropped(source, tuple(copies))

    return members.copies(call.context).bind(restored)


def render_set_sync(category: Category, members: Members, call: Call[Empty]) -> Outcome[Answer]:
    """Copies the source settings of a category to unlocked render sets."""
    context, reason = call.context, f"Unlocked render sets match source {category.title} settings"

    def wrote(targets: tuple[Owner, ...]) -> Outcome[Answer]:
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


def render_set_unsync(members: Members, call: Call[Empty]) -> Outcome[Answer]:
    """Restores the pre-sync settings of unlocked members, keeping records that fail to restore."""
    return (
        accumulated(
            MemberScope(members=members, owner_type=root, unlocked=True).owners(call.context).bind(lambda owners: accumulated(map(members.recovery.reverted, owners)))
            for root in dict.fromkeys(category.root for category in members.categories)
        )
        .map(lambda groups: sum(held for group in groups for held in group))
        .map(lambda count: Done(notes=(("INFO", f"Restored {count} render set members"),)) if count else Unchanged(reason="No render set holds synced settings"))
    )


def render_set_remove(members: Members, call: Call[Empty]) -> Outcome[Answer]:
    """Removes the active copied member."""

    def dropped(source: bpy.types.Scene) -> Outcome[Answer]:
        match call.context.scene:
            case bpy.types.Scene() as scene if scene in OWNERSHIP.serving(members.identity, source):
                return _dropped(source, (scene,))
            case _:
                return Error(Fault.of(Reason.ABSENT, repr(call.context.scene), member=OWNERSHIP.key))

    return members.source(call.context).bind(dropped)


def render_set_lock(members: Members, call: Call[Empty]) -> Outcome[Answer]:
    """Toggles the sync lock of the active member, first syncing every category into a never-synced member it unlocks."""
    context = call.context

    def configured(owner: Owner) -> Outcome[None]:
        def synced_first(held: Option[Endpoints]) -> Outcome[None]:
            return (
                accumulated(
                    written(members, category, context, targets)
                    for category in members.categories
                    for targets in (tuple(owner.view_layers) if isinstance(owner, bpy.types.Scene) and category.root is bpy.types.ViewLayer else (owner,) if isinstance(owner, category.root) else (),)
                    if targets
                ).map(lambda _: None)
                if held.is_none()
                else Ok(None)
            )

        return members.recovery.stored.read(owner).bind(synced_first)

    def changed(owner: Owner) -> Outcome[Answer]:
        record = members.membership(owner)
        unlocking = bool(record.locked)
        return (
            (configured(owner) if unlocking else Ok(None))
            .bind(lambda _: assigned(record, {("locked",): not unlocking}))
            .map(lambda _: Done(notes=(("INFO", f"{'Unlocked' if unlocking else 'Locked'} {owner.name}"),)))
        )

    def selected(form: Form) -> Outcome[Answer]:
        match form:
            case Form.COPIES:
                return rooted(bpy.types.Scene, context).bind(changed)
            case Form.LAYERS:
                return rooted(bpy.types.ViewLayer, context).bind(changed)

    return members.form(context).bind(selected)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "FORMS",
    "KEY",
    "OWNERSHIP",
    "Category",
    "Form",
    "MemberScope",
    "Members",
    "Membership",
    "Owner",
    "SceneMembership",
    "Sets",
    "added",
    "previewed",
    "propagated",
    "removed",
    "render_set_add",
    "render_set_adopt",
    "render_set_lock",
    "render_set_remove",
    "render_set_restore",
    "render_set_sync",
    "render_set_unsync",
    "shown",
    "transfers",
    "written",
]
