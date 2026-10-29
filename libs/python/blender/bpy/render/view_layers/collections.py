"""Collection restrictions per view layer with clipboard commands."""

from collections.abc import Callable, Mapping
from enum import auto, Enum
from types import MappingProxyType
from typing import Final

import bpy
from expression import Nothing, Ok, Option, Result, Some
from expression.collections import Block

from blender.bpy.commands.calls import Placement
from blender.bpy.commands.operators import Answer, answered, Call, Done, Empty, Operator, Unchanged
from blender.bpy.data.rna.descriptors import Descriptor, Value
from blender.bpy.data.rna.mirrors import choices
from blender.bpy.data.rna.paths import Native, rooted, Struct
from blender.bpy.data.rna.snapshots import Applied, Snapshot
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderReason
from blender.bpy.scene.collections import LayerPath, walked
from blender.bpy.ui.layout.members import Member
from blender.bpy.ui.layout.rows import Layout, Line, OperatorButton, pressed, Property, realized, RowDraw
from blender.bpy.ui.layout.sections import Section
from blender.bpy.ui.layout.tables import Branch, TreeMatrix
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired

# --- [TYPES] ----------------------------------------------------------------------------


class Spread(Enum):
    """Collections a restriction edit reaches."""

    ONE = auto()
    CHILDREN = auto()
    ISOLATE = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

RESTRICTIONS: Final[Mapping[str, Descriptor[bpy.types.BoolProperty]]] = MappingProxyType({
    prop.identifier: Descriptor.declared(bpy.types.LayerCollection, (prop.identifier,), bpy.types.BoolProperty)
    for prop in bpy.types.LayerCollection.bl_rna.properties
    if isinstance(prop, bpy.types.BoolProperty) and not prop.is_readonly
})
_VISIBLE: Final = Descriptor.declared(bpy.types.LayerCollection, ("is_visible",), bpy.types.BoolProperty)

# --- [MODELS] ---------------------------------------------------------------------------


class CollectionState(bpy.types.PropertyGroup):
    """Restriction value of a collection in one view layer."""

    layer: bpy.props.StringProperty(name="View Layer", description="View layer the edit writes")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    collection: bpy.props.IntProperty(name="Collection", description="Session identifier of the collection the edit writes")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    flag: bpy.props.EnumProperty(  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
        name="Restriction", description="Restriction the edit writes", items=choices((member.prop.identifier, member.prop.name, member.prop.description) for member in RESTRICTIONS.values())
    )
    value: bpy.props.BoolProperty(name="Value", description="Restriction value written")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    extent: bpy.props.EnumProperty(name="Extent", description="Collections the edit reaches", items=choices((spread.name, bpy.path.display_name(spread.name.lower()), "") for spread in Spread))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class CollectionStates(Line, frozen=True, kw_only=True):
    """Collection restriction grid and clipboard commands."""

    @classmethod
    def drawn(cls, identity: Identity) -> Outcome[RowDraw]:
        """Returns collection grid with clipboard buttons."""

        def toggled(line: bpy.types.UILayout, layer: str, cell: bpy.types.LayerCollection, collection: bpy.types.Collection) -> Outcome[None]:
            line.active = bool(_VISIBLE.read(cell))
            return accumulated(
                pressed(
                    line,
                    identity,
                    Placement(STATE, {"layer": layer, "collection": collection.session_uid, "flag": member.prop.identifier, "value": not on}),
                    {"text": "", "icon": member.prop.icon, "depress": on},
                )
                for member in RESTRICTIONS.values()
                for on in (bool(member.read(cell)),)
            ).map(lambda _: None)

        def tree(context: bpy.types.Context) -> tuple[tuple[str, ...], tuple[Branch[bpy.types.Collection, bpy.types.LayerCollection], ...]]:
            return Option.of_optional(context.scene).map(lambda scene: tuple(scene.view_layers)).map(lambda layers: (tuple(layer.name for layer in layers), grid(layers))).default_value(((), ()))

        matrix = TreeMatrix(tree=tree, cell=toggled, marks=(Property(target=Member(root=bpy.types.Collection, path=("hide_render",)), keywords={"text": "", "icon_only": True}),))
        clipboard = Layout(rows=tuple(OperatorButton(target=row) for row in CLIPBOARD), container=Some(lambda layout: layout.grid_flow(row_major=True, even_columns=True)))
        return realized((matrix, lambda layout, _holder, _context: layout.separator(type="LINE"), clipboard), identity)

    def section(self) -> Section[Empty]:
        """Returns collection grid section."""
        return Section(key="collection_states", title="Collections", rows=(self,))

    @classmethod
    def acquisitions(cls, _identity: Identity) -> Outcome[Acquisitions]:
        """Returns the restriction and clipboard commands."""
        return Ok((STATE, *CLIPBOARD))


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [RESTRICTIONS]


def found(layer: bpy.types.ViewLayer, collection: int) -> Option[LayerPath]:
    """Returns the first layer collection of the collection with session identifier, with ancestors from root."""
    return Block.of_seq((node, ancestors) for node, ancestors in walked(layer.layer_collection) if node.collection.session_uid == collection).try_head()


def applied(scene: bpy.types.Scene, *, layer: str, collection: int, flag: Descriptor[bpy.types.BoolProperty], value: bool, extent: Spread) -> Outcome[int]:
    """Writes a restriction value onto the collection with session identifier in the named layer, returning changed layer collection count."""

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
        return accumulated(flag.write(node, state) for node, state in writes if node != held.layer_collection).map(lambda prior: sum(each.is_some() for each in prior))

    def located(held: bpy.types.ViewLayer) -> Outcome[int]:
        target: Outcome[LayerPath] = found(held, collection).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(held.layer_collection), member=str(collection)))
        return Result[LayerPath, Faults].bind(target, lambda path: written(held, *path))

    named: Outcome[bpy.types.ViewLayer] = Option.of_optional(scene.view_layers.get(layer)).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(scene.view_layers), member=layer))
    return Result[bpy.types.ViewLayer, Faults].bind(named, located)


def isolates(layer: bpy.types.ViewLayer, collection: bpy.types.Collection, flag: Descriptor[bpy.types.BoolProperty]) -> bool:
    """Checks whether collection alone of top-level collections has flag cleared."""
    return all(flag.read(child) == (child.collection != collection) for child in layer.layer_collection.children)


def grid(layers: tuple[bpy.types.ViewLayer, ...]) -> tuple[Branch[bpy.types.Collection, bpy.types.LayerCollection], ...]:
    """Returns collection branches with one cell per view layer in scene order."""
    return tuple(
        (len(ancestors) - 1, node.collection, tuple(found(layer, node.collection.session_uid).starmap(lambda held, _ancestors: held) for layer in layers))
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
        paired(Result[bpy.types.ViewLayer, Faults].bind(rooted(bpy.types.ViewLayer, call.context), lambda layer: Snapshot.capture(owner(layer))), rooted(bpy.types.WindowManager, call.context))
        .map(lambda pair: pair[0].copied(pair[1]))
        .map(lambda _: Done())
    )


def pasting(owners: Callable[[Call[Empty]], Outcome[tuple[Struct, ...]]], reason: str, call: Call[Empty]) -> Outcome[Answer]:
    """Pastes clipboard settings onto the structs `owners` reads, `Unchanged` for `reason` when each holds them."""
    return paired(rooted(bpy.types.WindowManager, call.context), owners(call)).bind(lambda pair: Snapshot.pasted(*pair)).bind(lambda outcomes: answered(Applied.join(outcomes), reason))


def collection_state(call: Call[CollectionState]) -> Outcome[Answer]:
    """Set collection restriction, Ctrl to isolate, Shift to include children."""
    return (
        Result[bpy.types.Scene, Faults]
        .bind(
            rooted(bpy.types.Scene, call.context),
            lambda scene: applied(
                scene, layer=call.properties.layer, collection=call.properties.collection, flag=RESTRICTIONS[call.properties.flag], value=call.properties.value, extent=Spread[call.properties.extent]
            ),
        )
        .map(lambda count: Done(notes=(("INFO", f"Wrote {count} collection flags"),)) if count else Unchanged(reason="Collection flags hold requested value"))
    )


def collection_copy(call: Call[Empty]) -> Outcome[Done]:
    """Copy active layer collection states."""
    return copying(lambda layer: layer.layer_collection, call)


def collection_paste(call: Call[Empty]) -> Outcome[Answer]:
    """Paste collection states into active view layer."""
    return pasting(lambda held: rooted(bpy.types.ViewLayer, held.context).map(lambda layer: (layer.layer_collection,)), "Collection states match clipboard", call)


def collection_paste_all(call: Call[Empty]) -> Outcome[Answer]:
    """Paste collection states into every view layer."""
    return pasting(lambda held: rooted(bpy.types.Scene, held.context).map(lambda scene: tuple(layer.layer_collection for layer in scene.view_layers)), "Collection states match clipboard", call)


# --- [COMPOSITION] ----------------------------------------------------------------------

STATE: Final = Operator(form=CollectionState, internal=True, seed=Some(seeded), run=collection_state)
CLIPBOARD: Final = (Operator(form=Empty, undo=Nothing, run=collection_copy), Operator(form=Empty, run=collection_paste), Operator(form=Empty, run=collection_paste_all))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "CLIPBOARD",
    "RESTRICTIONS",
    "STATE",
    "CollectionState",
    "CollectionStates",
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
