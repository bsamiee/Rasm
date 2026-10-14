"""Collection states, clipboard operations, and restriction controls per view layer."""

from collections.abc import Callable
from typing import Annotated

import bpy
from expression import Option, Result, Some
from expression.collections import Block
import msgspec

from blender.bpy.commands.operators import answered, Call, Done, Empty, Operator, Outcome, Unchanged
from blender.bpy.data.rna.descriptors import Descriptor, IdName
from blender.bpy.data.rna.snapshots import Applied, Snapshot
from blender.bpy.extension.events.handlers import PathHandler
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderAbsentFault, BlenderFaults, BlenderResult
from blender.bpy.scene.collections import LayerPath, walked
from blender.bpy.settings.presets import copied, pasted
from blender.bpy.ui.layout.rows import Branch, Caption, Flow, joined, Line, Member, OperatorButton, placed, Property, RowDraw, Toggle, TreeMatrix
from blender.bpy.ui.layout.sections import Single
from blender.bpy.ui.screens.areas import AreaQuery
from blender.bpy.ui.screens.editors import EditorState
from blender.imaging.faults import accumulated, paired
from blender.imaging.parameters import Choice

# --- [TYPES] ----------------------------------------------------------------------------


class Flag(Choice):
    """Layer collection restrictions with identifiers derived from their member names."""

    EXCLUDE = 1, "Exclude"
    HOLDOUT = 2, "Holdout"
    INDIRECT_ONLY = 3, "Indirect Only"
    HIDE_VIEWPORT = 4, "Hide in Viewport"


class Extent(Choice):
    """Collections reached by a restriction edit."""

    ONE = 1, "Collection"
    CHILDREN = 2, "With Children"
    ISOLATE = 3, "Isolate"


# --- [MODELS] ---------------------------------------------------------------------------


class CollectionState(msgspec.Struct, frozen=True, kw_only=True):
    """Desired restriction on a collection within one named view layer."""

    layer: Annotated[str, msgspec.Meta(min_length=1, title="View Layer", description="View layer the edit writes")]
    collection: Annotated[IdName, msgspec.Meta(title="Collection", description="Collection the edit writes")]
    flag: Annotated[Flag, msgspec.Meta(title="Flag", description="Collection restriction the edit writes")]
    value: Annotated[bool, msgspec.Meta(title="Value", description="Restriction value written")]
    extent: Annotated[Extent, msgspec.Meta(title="Extent", description="Collections the edit reaches")] = Extent.ONE


class CollectionStates(Line, frozen=True, kw_only=True):
    """Collection restriction grid, clipboard commands, and Outliner columns."""

    # --- [COMMANDS]

    @staticmethod
    def commands(states: dict[Flag, Descriptor[bpy.types.BoolProperty]]) -> tuple[Operator[CollectionState], Operator[Empty], Operator[Empty], Operator[Empty]]:
        """Returns the restriction command followed by its clipboard commands."""

        def seeded(record: CollectionState, event: bpy.types.Event) -> CollectionState:
            return msgspec.structs.replace(record, extent=Extent.ISOLATE if event.ctrl else Extent.CHILDREN if event.shift else Extent.ONE)

        def pasting(owners: Callable[[bpy.types.Context], BlenderResult[tuple[bpy.types.LayerCollection, ...]]]) -> Callable[[Call[Empty]], BlenderResult[Outcome]]:
            return lambda call: (
                paired(call.root(bpy.types.WindowManager), owners(call.context))
                .bind(lambda pair: pasted(*pair))
                .bind(lambda outcomes: answered(Applied.join(outcomes), "Collection states already match the clipboard"))
            )

        def copy(call: Call[Empty]) -> BlenderResult[Done]:
            layer: BlenderResult[bpy.types.ViewLayer] = call.root(bpy.types.ViewLayer)
            manager: BlenderResult[bpy.types.WindowManager] = call.root(bpy.types.WindowManager)
            return (
                paired(Result[bpy.types.ViewLayer, BlenderFaults].bind(layer, lambda held: Snapshot.capture(held.layer_collection)), manager)
                .map(lambda pair: copied(pair[1], pair[0]))
                .map(lambda _: Done())
            )

        return (
            Operator(
                key="collection_state",
                title="Set Collection State",
                description="Write a collection restriction, Ctrl isolating and Shift reaching children",
                form=CollectionState,
                internal=True,
                seed=Some(seeded),
                run=lambda call: (
                    Result[bpy.types.Scene, BlenderFaults]
                    .bind(call.root(bpy.types.Scene), lambda scene: applied(call.record, scene, states))
                    .map(lambda count: Done(notes=(("INFO", f"Wrote {count} collection flags"),)) if count else Unchanged(reason="Collection flags already hold that state"))
                ),
            ),
            Operator(key="collection_copy", title="Copy Collection States", description="Copy the active view layer's collection states", form=Empty, undo=False, run=copy),
            Operator(
                key="collection_paste",
                title="Paste Collection States",
                description="Paste copied collection states into the active view layer",
                form=Empty,
                run=pasting(
                    lambda context: (
                        Option.of_optional(context.view_layer).map(lambda layer: (layer.layer_collection,)).to_result_with(lambda: (BlenderAbsentFault(owner=repr(context), key="view_layer"),))
                    )
                ),
            ),
            Operator(
                key="collection_paste_all",
                title="Paste States to Every Layer",
                description="Paste copied collection states into every view layer of the scene",
                form=Empty,
                run=pasting(
                    lambda context: (
                        Option
                        .of_optional(context.scene)
                        .map(lambda scene: tuple(layer.layer_collection for layer in scene.view_layers))
                        .to_result_with(lambda: (BlenderAbsentFault(owner=repr(context), key="scene"),))
                    )
                ),
            ),
        )

    # --- [LAYOUT]

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Returns the realized collection grid and its clipboard controls."""

        def built(states: dict[Flag, Descriptor[bpy.types.BoolProperty]], visible: Descriptor[bpy.types.BoolProperty]) -> BlenderResult[RowDraw]:
            state, *clipboard = self.commands(states)

            def toggled(layer: str, cell: bpy.types.LayerCollection, collection: bpy.types.Collection) -> tuple[Toggle[CollectionState], ...]:
                return tuple(
                    Toggle(record=CollectionState(layer=layer, collection=IdName.of(collection), flag=flag, value=not on), icon=member.prop.icon, depress=on)
                    for flag, member in states.items()
                    for on in (bool(member.read(cell)),)
                )

            def tree(context: bpy.types.Context) -> tuple[tuple[str, ...], tuple[Branch[bpy.types.Collection, bpy.types.LayerCollection], ...]]:
                return Option.of_optional(context.scene).map(lambda scene: tuple(scene.view_layers)).map(lambda layers: (tuple(layer.name for layer in layers), grid(layers))).default_value(((), ()))

            matrix = TreeMatrix(
                tree=tree,
                command=state,
                toggles=toggled,
                active=Some(lambda cell: bool(visible.read(cell))),
                marks=(Property(target=Member(root=bpy.types.Collection, path="hide_render"), text=Some(""), icon_only=True),),
            )
            return placed((matrix, Caption(rows=tuple(OperatorButton(target=row) for row in clipboard), flow=Flow.GRID)), identity).map(joined)

        return paired(flags(), Descriptor.resolve(bpy.types.LayerCollection, "is_visible", bpy.types.BoolProperty)).bind(lambda pair: built(*pair))

    def section(self) -> Single[Empty]:
        """Returns the collection grid's section."""
        return Single(key="collection_states", title="Collections", rows=(self,))

    # --- [REGISTRATION]

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns command declarations and the Outliner restriction-column handler."""
        query = AreaQuery(space="OUTLINER")
        columns = EditorState.of(bpy.types.SpaceOutliner, {"show_restrict_column_holdout": True, "show_restrict_column_indirect_only": True})
        return paired(flags(), columns).map(
            lambda pair: (
                *self.commands(pair[0]),
                PathHandler(
                    handlers=bpy.app.handlers.load_post,  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                    body=lambda _: (
                        Result[bpy.types.WindowManager, BlenderFaults]
                        .bind(
                            Option.of_optional(bpy.context.window_manager).to_result_with(lambda: (BlenderAbsentFault(owner=repr(bpy.context), key="window_manager"),)),
                            lambda manager: accumulated(pair[1].applied(found.area) for found in query.located(manager.windows)),
                        )
                        .map(lambda _: None)
                    ),
                ),
            )
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def flags() -> BlenderResult[dict[Flag, Descriptor[bpy.types.BoolProperty]]]:
    """Returns resolved restriction descriptors or every missing or mistyped native member."""
    return accumulated(Descriptor.resolve(bpy.types.LayerCollection, flag.name.lower(), bpy.types.BoolProperty).map(lambda member: (flag, member)) for flag in Flag).map(dict)


def found(layer: bpy.types.ViewLayer, collection: IdName) -> Option[LayerPath]:
    """Returns the collection's first native tree occurrence and root-first ancestors."""
    return Block.of_seq((node, ancestors) for node, ancestors in walked(layer.layer_collection) if IdName.of(node.collection) == collection).try_head()


def applied(state: CollectionState, scene: bpy.types.Scene, flags: dict[Flag, Descriptor[bpy.types.BoolProperty]]) -> BlenderResult[int]:
    """Returns the changed-node count, or missing native targets and refused assignments."""
    member = flags[state.flag]

    def written(layer: bpy.types.ViewLayer, target: bpy.types.LayerCollection, ancestors: tuple[bpy.types.LayerCollection, ...]) -> BlenderResult[int]:
        inside = tuple(node for node, _ in walked(target))
        writes: tuple[tuple[bpy.types.LayerCollection, bool], ...]
        match state.extent:
            case Extent.ONE:
                writes = ((target, state.value),)
            case Extent.CHILDREN:
                writes = tuple((node, state.value) for node in inside)
            case Extent.ISOLATE:
                kept = (*inside, *ancestors[1:])
                writes = tuple((node, state.value == (node in kept)) for node, parents in walked(layer.layer_collection) if parents)
        return accumulated(member.write(node, value) for node, value in writes if node != layer.layer_collection).map(lambda prior: sum(value.is_some() for value in prior))

    def located(layer: bpy.types.ViewLayer) -> BlenderResult[int]:
        target: BlenderResult[LayerPath] = found(layer, state.collection).to_result_with(lambda: (BlenderAbsentFault(owner=repr(layer.layer_collection), key=state.collection.name),))
        return Result[LayerPath, BlenderFaults].bind(target, lambda path: written(layer, *path))

    layer: BlenderResult[bpy.types.ViewLayer] = Option.of_optional(scene.view_layers.get(state.layer)).to_result_with(lambda: (BlenderAbsentFault(owner=repr(scene.view_layers), key=state.layer),))
    return Result[bpy.types.ViewLayer, BlenderFaults].bind(layer, located)


def isolates(layer: bpy.types.ViewLayer, collection: bpy.types.Collection, flag: Flag, flags: dict[Flag, Descriptor[bpy.types.BoolProperty]]) -> bool:
    """Returns whether only the named top-level collection has this restriction cleared."""
    member = flags[flag]
    return all(member.read(child) == (child.collection != collection) for child in layer.layer_collection.children)


def grid(layers: tuple[bpy.types.ViewLayer, ...]) -> tuple[Branch[bpy.types.Collection, bpy.types.LayerCollection], ...]:
    """Returns collection branches across the native view-layer order of one scene."""
    return tuple(
        Branch(depth=len(ancestors) - 1, label=node.collection, cells=tuple(found(layer, IdName.of(node.collection)).starmap(lambda held, _ancestors: held) for layer in layers))
        for node, ancestors in walked(layers[0].layer_collection)
        if ancestors
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["CollectionState", "CollectionStates", "Extent", "Flag", "applied", "flags", "found", "grid", "isolates"]
