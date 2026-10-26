"""Collection restrictions per view layer with clipboard commands."""

from collections.abc import Callable, Mapping
from enum import auto, Enum
from types import MappingProxyType
from typing import Final

import bpy
from expression import Option, Result, Some
from expression.collections import Block

from blender.bpy.commands.operators import Answer, answered, Call, Done, Empty, Operator, Unchanged
from blender.bpy.data.rna.descriptors import Descriptor, Native, rooted, Struct, Value
from blender.bpy.data.rna.snapshots import Applied, Snapshot
from blender.bpy.extension.events.handlers import LOAD_POST, PathHandler
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderReason
from blender.bpy.scene.collections import LayerPath, walked
from blender.bpy.settings.presets import copied, pasted
from blender.bpy.ui.layout.rows import Branch, Caption, Flow, joined, Line, Member, OperatorButton, placed, Property, RowDraw, Toggle, TreeMatrix
from blender.bpy.ui.layout.sections import Single
from blender.bpy.ui.screens.areas import AreaQuery
from blender.bpy.ui.screens.editors import EditorState
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired

# --- [TYPES] ----------------------------------------------------------------------------


class Restriction(Enum):
    """Layer collection restrictions, member names deriving RNA identifiers."""

    EXCLUDE = auto()
    HOLDOUT = auto()
    INDIRECT_ONLY = auto()
    HIDE_VIEWPORT = auto()


class Spread(Enum):
    """Collections a restriction edit reaches."""

    ONE = auto()
    CHILDREN = auto()
    ISOLATE = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

FLAGS: Final[Mapping[Restriction, Descriptor[bpy.types.BoolProperty]]] = MappingProxyType({
    flag: Descriptor.declared(bpy.types.LayerCollection, flag.name.lower(), bpy.types.BoolProperty) for flag in Restriction
})
_VISIBLE: Final = Descriptor.declared(bpy.types.LayerCollection, "is_visible", bpy.types.BoolProperty)
_COLUMNS: Final = {"show_restrict_column_holdout": True, "show_restrict_column_indirect_only": True}

# --- [MODELS] ---------------------------------------------------------------------------


class CollectionState(bpy.types.PropertyGroup):
    """Restriction value of a collection in one view layer."""

    layer: bpy.props.StringProperty(name="View Layer", description="View layer the edit writes")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    collection: bpy.props.IntProperty(name="Collection", description="Session identifier of the collection the edit writes")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    flag: bpy.props.EnumProperty(name="Restriction", description="Restriction the edit writes", items=tuple((flag.name, member.prop.name, member.prop.description) for flag, member in FLAGS.items()))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    value: bpy.props.BoolProperty(name="Value", description="Restriction value written")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    extent: bpy.props.EnumProperty(name="Extent", description="Collections the edit reaches", items=tuple((spread.name, bpy.path.display_name(spread.name.lower()), "") for spread in Spread))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class CollectionStates(Line, frozen=True, kw_only=True):
    """Collection restriction grid, clipboard commands, and Outliner columns."""

    @classmethod
    def drawn(cls, identity: Identity) -> Outcome[RowDraw]:
        """Returns collection grid with clipboard buttons."""

        def toggled(layer: str, cell: bpy.types.LayerCollection, collection: bpy.types.Collection) -> tuple[Toggle, ...]:
            return tuple(
                Toggle(values={"layer": layer, "collection": collection.session_uid, "flag": flag.name, "value": not on}, icon=member.prop.icon, depress=on)
                for flag, member in FLAGS.items()
                for on in (bool(member.read(cell)),)
            )

        def tree(context: bpy.types.Context) -> tuple[tuple[str, ...], tuple[Branch[bpy.types.Collection, bpy.types.LayerCollection], ...]]:
            return Option.of_optional(context.scene).map(lambda scene: tuple(scene.view_layers)).map(lambda layers: (tuple(layer.name for layer in layers), grid(layers))).default_value(((), ()))

        matrix = TreeMatrix(
            tree=tree,
            command=STATE,
            toggles=toggled,
            active=Some(lambda cell: bool(_VISIBLE.read(cell))),
            marks=(Property(target=Member(root=bpy.types.Collection, path="hide_render"), text="", icon_only=True),),
        )
        return placed((matrix, Caption(rows=tuple(OperatorButton(target=row) for row in CLIPBOARD), flow=Flow.GRID)), identity).map(joined)

    def section(self) -> Single[Empty]:
        """Returns collection grid section."""
        return Single(key="collection_states", title="Collections", rows=(self,))

    @classmethod
    def acquisitions(cls, _identity: Identity) -> Outcome[Acquisitions]:
        """Returns commands and load handler showing Outliner restriction columns."""
        query = AreaQuery(space="OUTLINER")
        return EditorState.of(bpy.types.SpaceOutliner, _COLUMNS).map(
            lambda columns: (
                STATE,
                *CLIPBOARD,
                PathHandler(
                    handlers=LOAD_POST,
                    body=lambda _: (
                        rooted(bpy.types.WindowManager, bpy.context).bind(lambda manager: accumulated(columns.applied(found.area) for found in query.located(manager.windows))).map(lambda _: None)
                    ),
                ),
            )
        )


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [RESTRICTIONS]


def found(layer: bpy.types.ViewLayer, collection: int) -> Option[LayerPath]:
    """Returns the first layer collection of the collection with session identifier, with ancestors from root."""
    return Block.of_seq((node, ancestors) for node, ancestors in walked(layer.layer_collection) if node.collection.session_uid == collection).try_head()


def applied(scene: bpy.types.Scene, *, layer: str, collection: int, flag: Restriction, value: bool, extent: Spread) -> Outcome[int]:
    """Writes a restriction value onto the collection with session identifier in the named layer, returning changed layer collection count."""
    member = FLAGS[flag]

    def written(held: bpy.types.ViewLayer, target: bpy.types.LayerCollection, ancestors: tuple[bpy.types.LayerCollection, ...]) -> Outcome[int]:
        inside = tuple(node for node, _ in walked(target))
        writes: tuple[tuple[bpy.types.LayerCollection, bool], ...]
        match extent:
            case Spread.ONE:
                writes = ((target, value),)
            case Spread.CHILDREN:
                writes = tuple((node, value) for node in inside)
            case Spread.ISOLATE:
                kept = (*inside, *ancestors[1:])
                writes = tuple((node, value == (node in kept)) for node, parents in walked(held.layer_collection) if parents)
        return accumulated(member.write(node, state) for node, state in writes if node != held.layer_collection).map(lambda prior: sum(each.is_some() for each in prior))

    def located(held: bpy.types.ViewLayer) -> Outcome[int]:
        target: Outcome[LayerPath] = found(held, collection).to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=repr(held.layer_collection), member=str(collection)),))
        return Result[LayerPath, Faults].bind(target, lambda path: written(held, *path))

    named: Outcome[bpy.types.ViewLayer] = Option.of_optional(scene.view_layers.get(layer)).to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=repr(scene.view_layers), member=layer),))
    return Result[bpy.types.ViewLayer, Faults].bind(named, located)


def isolates(layer: bpy.types.ViewLayer, collection: bpy.types.Collection, flag: Restriction) -> bool:
    """Checks whether collection alone of top-level collections has flag cleared."""
    return all(FLAGS[flag].read(child) == (child.collection != collection) for child in layer.layer_collection.children)


def grid(layers: tuple[bpy.types.ViewLayer, ...]) -> tuple[Branch[bpy.types.Collection, bpy.types.LayerCollection], ...]:
    """Returns collection branches with one cell per view layer in scene order."""
    return tuple(
        Branch(depth=len(ancestors) - 1, label=node.collection, cells=tuple(found(layer, node.collection.session_uid).starmap(lambda held, _ancestors: held) for layer in layers))
        for node, ancestors in walked(layers[0].layer_collection)
        if ancestors
    )


# --- [COMMANDS]


def seeded(_properties: CollectionState, event: bpy.types.Event, _identity: Identity) -> Mapping[str, Value]:
    """Seeds the restriction extent from held modifiers, Ctrl to isolate and Shift to include children."""
    return {"extent": (Spread.ISOLATE if event.ctrl else Spread.CHILDREN if event.shift else Spread.ONE).name}


def copying(owner: Callable[[bpy.types.ViewLayer], Native], call: Call[Empty]) -> Outcome[Done]:
    """Copies the settings `owner` reads off the active layer to the clipboard."""
    return (
        paired(Result[bpy.types.ViewLayer, Faults].bind(call.root(bpy.types.ViewLayer), lambda layer: Snapshot.capture(owner(layer))), call.root(bpy.types.WindowManager))
        .map(lambda pair: copied(pair[1], pair[0]))
        .map(lambda _: Done())
    )


def pasting(owners: Callable[[Call[Empty]], Outcome[tuple[Struct, ...]]], reason: str, call: Call[Empty]) -> Outcome[Answer]:
    """Pastes clipboard settings onto the structs `owners` reads, `Unchanged` for `reason` when each holds them."""
    return paired(call.root(bpy.types.WindowManager), owners(call)).bind(lambda pair: pasted(*pair)).bind(lambda outcomes: answered(Applied.join(outcomes), reason))


def collection_state(call: Call[CollectionState]) -> Outcome[Answer]:
    """Set collection restriction, Ctrl to isolate, Shift to include children."""
    return (
        Result[bpy.types.Scene, Faults]
        .bind(
            call.root(bpy.types.Scene),
            lambda scene: applied(
                scene, layer=call.properties.layer, collection=call.properties.collection, flag=Restriction[call.properties.flag], value=call.properties.value, extent=Spread[call.properties.extent]
            ),
        )
        .map(lambda count: Done(notes=(("INFO", f"Wrote {count} collection flags"),)) if count else Unchanged(reason="Collection flags hold requested value"))
    )


def collection_copy(call: Call[Empty]) -> Outcome[Done]:
    """Copy active layer collection states."""
    return copying(lambda layer: layer.layer_collection, call)


def collection_paste(call: Call[Empty]) -> Outcome[Answer]:
    """Paste collection states into active view layer."""
    return pasting(lambda held: held.root(bpy.types.ViewLayer).map(lambda layer: (layer.layer_collection,)), "Collection states match clipboard", call)


def collection_paste_all(call: Call[Empty]) -> Outcome[Answer]:
    """Paste collection states into every view layer."""
    return pasting(lambda held: held.root(bpy.types.Scene).map(lambda scene: tuple(layer.layer_collection for layer in scene.view_layers)), "Collection states match clipboard", call)


# --- [COMPOSITION] ----------------------------------------------------------------------

STATE: Final = Operator(form=CollectionState, internal=True, seed=Some(seeded), run=collection_state)
CLIPBOARD: Final = (Operator(form=Empty, undo=False, run=collection_copy), Operator(form=Empty, run=collection_paste), Operator(form=Empty, run=collection_paste_all))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "CLIPBOARD",
    "FLAGS",
    "STATE",
    "CollectionState",
    "CollectionStates",
    "Restriction",
    "Spread",
    "applied",
    "collection_copy",
    "collection_paste",
    "collection_paste_all",
    "collection_state",
    "copying",
    "found",
    "grid",
    "isolates",
    "pasting",
    "seeded",
]
