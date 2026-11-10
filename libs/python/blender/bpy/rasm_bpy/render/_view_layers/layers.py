"""View layers: the layer list with tags and roles, setup overrides, collection restrictions, palette tints, renames, splits, and the settings clipboard."""

from collections.abc import Mapping
import colorsys
from contextlib import ExitStack
from enum import auto, Enum
from functools import partial, reduce
from types import MappingProxyType
from typing import Annotated, Final

import bpy
from expression import Option, Some
from expression.collections import Block
from expression.extra.result import catch
import mathutils
import networkx as nx
import numpy as np

from rasm_bpy.commands.operators import Answer, answered, Call, Done, Empty, Operator, Placement, Unchanged
from rasm_bpy.data._extension.identity import Identity
from rasm_bpy.data._extension.reports import Refusal
from rasm_bpy.data._rna.conditions import Scenes, ViewLayers
from rasm_bpy.data._rna.descriptors import Descriptor, Value, written
from rasm_bpy.data._rna.paths import rooted
from rasm_bpy.data._rna.properties import choices, enum_choices
from rasm_bpy.data._rna.snapshots import Applied, Snapshot
from rasm_bpy.render._view_layers.passes import layer_of, soloed, used
from rasm_bpy.scene._collections import walked
from rasm_bpy.scene._materials import Clay, material, Utility
from rasm_bpy.settings._attachments import Attachment
from rasm_bpy.ui._layout.lists import ItemList
from rasm_bpy.ui._layout.members import Member
from rasm_bpy.ui._layout.rows import FUNCTIONS, Layout, operator_button, OperatorButton, Property, RowContent
from rasm_bpy.ui._layout.sections import Section
from rasm_bpy.ui._layout.tables import SettingsTable, TreeTable
from rasm_records import accumulated, Fault, Ok, Outcome, paired, Reason
from rasm_rendering import Fields, NumberKey, OutputName, TextKey

# --- [TYPES] ----------------------------------------------------------------------------


class _Extent(Enum):
    """Collections a restriction edit reaches."""

    ONE = auto()
    CHILDREN = auto()
    ISOLATE = auto()


class Role(Enum):
    """View layer purpose."""

    BEAUTY = auto()
    DATA = auto()


class _Palette(Enum):
    """Tag palette: hues spaced on the color wheel, or a published color-blind safe palette valued by its sRGB hex colors."""

    SPECTRUM = ""
    DESIGN_LIBRARY = "648FFF 785EF0 DC267F FE6100 FFB000"
    WONG = "000000 E69F00 56B4E9 009E73 F0E442 0072B2 D55E00 CC79A7"
    TOL = "332288 117733 44AA99 88CCEE DDCC77 CC6677 AA4499 882255"


class _RenameScope(Enum):
    """Scenes a rename reaches."""

    SCENE = auto()
    FILE = auto()


class _Setup(Enum):
    """Material override a layer toggles, valued by its owned surface setup."""

    CLAY = Clay()
    DATA = Utility()


class _Part(Enum):
    """Settings the clipboard copies off the active layer."""

    LAYER = auto()
    COLLECTIONS = auto()


class _PasteScope(Enum):
    """View layers a paste reaches."""

    ACTIVE = auto()
    USED = auto()
    ALL = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

_SATURATION: Final = 0.7
_VALUE: Final = 0.36
_RESTRICTIONS: Final[Mapping[str, Descriptor[bpy.types.BoolProperty]]] = MappingProxyType({
    prop.identifier: Descriptor(root=bpy.types.LayerCollection, prop=prop) for prop in bpy.types.LayerCollection.bl_rna.properties if isinstance(prop, bpy.types.BoolProperty) and not prop.is_readonly
})
_FLAGS: Final = choices((member.prop.identifier, member.prop.name, member.prop.description) for member in _RESTRICTIONS.values())

# --- [MODELS] ---------------------------------------------------------------------------


class CollectionState(bpy.types.PropertyGroup):
    """Restriction value of a collection in one view layer."""

    layer: Annotated[str, bpy.props.StringProperty(name="View Layer", description="View layer the edit writes")]
    collection: Annotated[int, bpy.props.IntProperty(name="Collection", description="Session identifier of the collection the edit writes")]
    flag: Annotated[str, bpy.props.EnumProperty(name="Restriction", description="Restriction the edit writes", items=_FLAGS)]
    value: Annotated[bool, bpy.props.BoolProperty(name="Value", description="Restriction value written")]
    extent: Annotated[str, bpy.props.EnumProperty(name="Extent", description="Collections the edit reaches", items=enum_choices(_Extent))]


class LayerTag(bpy.types.PropertyGroup):
    """Purpose and tag of a view layer."""

    role: Annotated[str, bpy.props.EnumProperty(name="Role", description="Beauty layers form image, data layers hold data passes and mattes", items=enum_choices(Role))]
    color: Annotated[
        mathutils.Color,
        bpy.props.FloatVectorProperty(
            name="Tag", description="Layer color in lists and on its nodes", subtype="COLOR", min=0.0, max=1.0, default=mathutils.Color((_VALUE,) * 3).from_srgb_to_scene_linear()[:]
        ),
    ]


class Renaming(bpy.types.PropertyGroup):
    """Rename of every layer by its list position."""

    template: Annotated[str, bpy.props.StringProperty(name="Names", description="Layer name grammar holding {order:<width>} and {layer}", default="{order:2}_{layer}")]
    scope: Annotated[str, bpy.props.EnumProperty(name="Scenes", description="Scenes rename reaches", items=enum_choices(_RenameScope))]


class Tinting(bpy.types.PropertyGroup):
    """Tag colors written from a palette."""

    palette: Annotated[str, bpy.props.EnumProperty(name="Palette", description="Palette tags take in list order", items=enum_choices(_Palette))]
    saturation: Annotated[float, bpy.props.FloatProperty(name="Saturation", description="Saturation of every spectrum hue", subtype="FACTOR", min=0.0, max=1.0, default=_SATURATION)]
    value: Annotated[float, bpy.props.FloatProperty(name="Value", description="Brightness of every spectrum hue", subtype="FACTOR", min=0.0, max=1.0, default=_VALUE)]


class Splitting(bpy.types.PropertyGroup):
    """Split of top-level collections into layers."""

    others: Annotated[str, bpy.props.EnumProperty(name="Others", description="Restriction other top-level collections take in a new layer", items=_FLAGS, default="indirect_only")]


class Overriding(bpy.types.PropertyGroup):
    """Setup material override toggled on the active layer."""

    setup: Annotated[str, bpy.props.EnumProperty(name="Setup", description="Owned surface setup the override applies", items=enum_choices(_Setup))]


class Clipboard(bpy.types.PropertyGroup):
    """Settings copied off the active layer and the layers a paste reaches."""

    part: Annotated[str, bpy.props.EnumProperty(name="Part", description="View layer settings or collection states the copy takes", items=enum_choices(_Part))]
    scope: Annotated[str, bpy.props.EnumProperty(name="Layers", description="View layers the paste writes", items=enum_choices(_PasteScope))]


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [RESTRICTIONS]


def _applied(scene: bpy.types.Scene, *, layer: str, collection: int, flag: Descriptor[bpy.types.BoolProperty], value: bool, extent: _Extent) -> Outcome[int]:
    """Writes a restriction value onto the collection with session identifier in the named layer, returning changed layer collection count."""

    def written(held: bpy.types.ViewLayer, target: bpy.types.LayerCollection, ancestors: tuple[bpy.types.LayerCollection, ...]) -> Outcome[int]:
        writes: tuple[tuple[bpy.types.LayerCollection, bool], ...]
        match extent:
            case _Extent.ONE:
                writes = ((target, value),)
            case _Extent.CHILDREN:
                writes = tuple((node, value) for node, _ in walked(target))
            case _Extent.ISOLATE:
                kept = frozenset(node for node, _ in walked(target)) | frozenset(ancestors)
                writes = tuple((node, value == (node in kept)) for node, parents in walked(held.layer_collection) if parents)
        return accumulated(flag.write(node, state) for node, state in writes if node != held.layer_collection).map(lambda prior: sum(each.is_some() for each in prior))

    def located(held: bpy.types.ViewLayer) -> Outcome[int]:
        return (
            Block
            .of_seq((node, ancestors) for node, ancestors in walked(held.layer_collection) if node.collection.session_uid == collection)
            .try_head()
            .to_result_with(lambda: Fault.of(Reason.ABSENT, repr(held.layer_collection), member=str(collection)))
            .bind(lambda path: written(held, *path))
        )

    return layer_of(scene, layer).bind(located)


def _grid(context: bpy.types.Context) -> tuple[tuple[str, ...], tuple[tuple[int, bpy.types.Collection, tuple[Option[bpy.types.LayerCollection], ...]], ...]]:
    """Returns the context scene's layer names and collection branches, one cell per view layer in scene order."""
    layers: tuple[bpy.types.ViewLayer, ...] = Option.of_optional(context.scene).map(lambda scene: tuple(scene.view_layers)).default_value(())
    cells = tuple({node.collection.session_uid: node for node, _ in reversed(tuple(walked(layer.layer_collection)))} for layer in layers)
    return tuple(layer.name for layer in layers), tuple(
        (len(ancestors) - 1, node.collection, tuple(Option.of_optional(held.get(node.collection.session_uid)) for held in cells))
        for root in layers[:1]
        for node, ancestors in walked(root.layer_collection)
        if ancestors
    )


# --- [LAYERS]


def _moved(context: bpy.types.Context, current: int, row: int) -> Outcome[None]:
    """Moves the context scene's view layer at `current` to `row`."""
    return rooted(bpy.types.Scene, context).bind(
        lambda scene: catch(scene.view_layers.move, exception=RuntimeError)(current, row).map(lambda _: None).map_error(partial(Fault.of, Refusal.REFUSED, repr(scene.view_layers)))
    )


def colors(count: int, palette: _Palette = _Palette.SPECTRUM, *, saturation: float = _SATURATION, value: float = _VALUE) -> tuple[tuple[float, float, float], ...]:
    """Returns scene-linear palette colors in list order, a published palette cycled and the spectrum spacing hues evenly on the color wheel."""
    encoded = (
        tuple(colorsys.hsv_to_rgb(index / count, saturation, value) for index in range(count))
        if palette is _Palette.SPECTRUM
        else tuple(map(tuple, np.resize(np.frombuffer(bytes.fromhex(palette.value), np.uint8).reshape(-1, 3), (count, 3)) / 255))
    )
    return tuple((red, green, blue) for red, green, blue in (mathutils.Color(rgb).from_srgb_to_scene_linear()[:] for rgb in encoded))


# --- [COMMANDS]


def _seeded(_properties: CollectionState, event: bpy.types.Event, _identity: Identity) -> dict[str, Value]:
    """Seeds the restriction extent from held modifiers, Ctrl to isolate and Shift to include children."""
    return {"extent": (_Extent.ISOLATE if event.ctrl else _Extent.CHILDREN if event.shift else _Extent.ONE).name}


@Operator.of(CollectionState, options=frozenset({"INTERNAL", "UNDO"}), seed=Some(_seeded))
def collection_state(call: Call[CollectionState]) -> Outcome[Answer]:
    """Set collection restriction, Ctrl to isolate, Shift to include children."""
    held = call.properties
    return (
        rooted(bpy.types.Scene, call.context)
        .bind(lambda scene: _applied(scene, layer=held.layer, collection=held.collection, flag=_RESTRICTIONS[held.flag], value=held.value, extent=_Extent[held.extent]))
        .map(lambda count: Done(notes=(("INFO", f"Wrote {count} collection flags"),)) if count else Unchanged(reason="Collection flags hold requested value"))
    )


@Operator.of(Clipboard, options=frozenset())
def view_layer_copy(call: Call[Clipboard]) -> Outcome[Answer]:
    """Copy active view layer settings or its collection states."""
    part = _Part[call.properties.part]
    return (
        paired(
            rooted(bpy.types.ViewLayer, call.context).bind(lambda layer: Snapshot.capture(layer if part is _Part.LAYER else layer.layer_collection, owned=call.identity.owns)),
            rooted(bpy.types.WindowManager, call.context),
        )
        .map(lambda pair: pair[0].copied(pair[1]))
        .map(lambda _: Done())
    )


@Operator.of(Clipboard)
def view_layer_paste(call: Call[Clipboard]) -> Outcome[Answer]:
    """Paste copied view layer settings or collection states into the view layers the scope reaches."""

    def reached(scene: bpy.types.Scene, active: bpy.types.ViewLayer) -> tuple[bpy.types.ViewLayer, ...]:
        match _PasteScope[call.properties.scope]:
            case _PasteScope.ACTIVE:
                return (active,)
            case _PasteScope.USED:
                return used(scene)
            case _PasteScope.ALL:
                return tuple(scene.view_layers)

    return (
        paired(rooted(bpy.types.WindowManager, call.context), rooted(bpy.types.Scene, call.context), rooted(bpy.types.ViewLayer, call.context))
        .bind(lambda found: Snapshot.pasted(found[0], tuple(owner for layer in reached(found[1], found[2]) for owner in (layer, layer.layer_collection))))
        .bind(lambda outcomes: answered(Applied.join(outcomes), "View layers match clipboard"))
    )


@Operator.of(Empty)
def view_layer_solo(call: Call[Empty]) -> Outcome[Answer]:
    """Render active view layer alone."""
    return rooted(bpy.types.ViewLayer, call.context).map(lambda layer: Done() if soloed(layer) else Unchanged(reason=f"{layer.name} renders alone"))


def _renamed(grammar: OutputName, scene: bpy.types.Scene) -> Outcome[int]:
    """Renames scene layers to composed names, freeing occupied targets before assignment and breaking cycles with one temporary name."""
    layers = tuple(scene.view_layers)
    occupied = {layer.name: index for index, layer in enumerate(layers)}

    def stepped(names: tuple[str, ...]) -> Outcome[int]:
        graph: nx.DiGraph[int, dict[str, object], dict[str, object]] = nx.DiGraph()
        graph.add_nodes_from(range(len(layers)))
        graph.add_edges_from((holder, index) for index, name in enumerate(names) if (holder := occupied.get(name, index)) != index)
        groups = tuple(nx.strongly_connected_components(graph))
        steps = (
            step
            for group in nx.topological_sort(nx.condensation(graph, groups))
            for first in (min(groups[group]),)
            for order in (tuple(nx.dfs_preorder_nodes(graph.subgraph(groups[group]), first)),)
            for step in (*(((first, "." if layers[first].name.startswith("_") else "_", False),) if len(order) > 1 else ()), *((index, names[index], True) for index in (*order[1:], first)))
        )
        return reduce(lambda done, step: done.bind(lambda count: written(layers[step[0]], ("name",), step[1]).map(lambda prior: count + (step[2] and prior.is_some()))), steps, Ok(0))

    return accumulated(
        grammar.compose(
            Fields(
                texts={TextKey.LAYER: grammar.parse(layer.name).to_option().bind(lambda fields: Option.of_optional(fields.texts.get(TextKey.LAYER))).default_value(layer.name)},
                numbers={NumberKey.ORDER: index + 1},
            )
        )
        for index, layer in enumerate(layers)
    ).bind(stepped)


@Operator.of(Renaming)
def view_layer_rename(call: Call[Renaming]) -> Outcome[Answer]:
    """Name view layers by list position and base name."""
    scenes: Outcome[tuple[bpy.types.Scene, ...]]
    match _RenameScope[call.properties.scope]:
        case _RenameScope.SCENE:
            scenes = rooted(bpy.types.Scene, call.context).map(lambda scene: (scene,))
        case _RenameScope.FILE:
            scenes = rooted(bpy.types.BlendData, call.context).map(lambda data: tuple(data.scenes))

    return (
        paired(OutputName.of(call.properties.template), scenes)
        .bind(lambda pair: accumulated(_renamed(pair[0], scene) for scene in pair[1]))
        .map(sum)
        .map(lambda count: Done(notes=(("INFO", f"Renamed {count} view layers"),)) if count else Unchanged(reason="View layers hold their composed names"))
    )


@Operator.of(Tinting)
def view_layer_tint(call: Call[Tinting]) -> Outcome[Answer]:
    """Set view layer tags from palette in list order."""
    held = call.properties
    return paired(rooted(bpy.types.Scene, call.context), LAYER_TAG.bound(call.identity)).bind(
        lambda pair: accumulated(
            written(pair[1].struct(layer), ("color",), color)
            for layer, color in zip(pair[0].view_layers, colors(len(pair[0].view_layers), _Palette[held.palette], saturation=held.saturation, value=held.value))
        ).map(lambda priors: Done() if any(prior.is_some() for prior in priors) else Unchanged(reason=f"View layers of {pair[0].name} hold palette tags"))
    )


@Operator.of(Splitting)
def view_layer_split(call: Call[Splitting]) -> Outcome[Answer]:
    """Add a view layer per top-level collection no layer isolates, the others under the chosen flag, removing every added layer on fault."""
    flag = _RESTRICTIONS[call.properties.others]

    def split(scene: bpy.types.Scene) -> Outcome[Answer]:
        isolated = frozenset(
            only.collection
            for layer in scene.view_layers
            for active in (tuple(child for child in layer.layer_collection.children if flag.read(child) is False),)
            if len(active) == 1
            for only in active
        )
        with ExitStack() as rollback:

            def made(collection: bpy.types.Collection) -> Outcome[str]:
                layer = scene.view_layers.new(collection.name)
                rollback.callback(scene.view_layers.remove, layer)
                return _applied(scene, layer=layer.name, collection=collection.session_uid, flag=flag, value=False, extent=_Extent.ISOLATE).map(lambda _: layer.name)

            if (added := accumulated(map(made, tuple(collection for collection in scene.collection.children if collection not in isolated)))).is_ok():
                rollback.pop_all()
        return added.map(lambda names: Done(notes=tuple(("INFO", f"Added {name}") for name in names)) if names else Unchanged(reason="Each top-level collection has a layer isolating it"))

    return rooted(bpy.types.Scene, call.context).bind(split)


def _overridden(layer: bpy.types.ViewLayer, setup: Clay | Utility, identity: Identity) -> Outcome[None]:
    """Toggles layer material override to owned setup material, the data setup writing layer role with it."""

    def toggled(held: bpy.types.Material) -> Outcome[None]:
        on = layer.material_override != held
        layer.material_override = held if on else None
        match setup:
            case Utility():
                return LAYER_TAG.read(identity, layer).bind(lambda group: written(group, ("role",), (Role.DATA if on else Role.BEAUTY).name)).map(lambda _: None)
            case Clay():
                return Ok(None)

    return material(setup, identity).bind(toggled)


@Operator.of(Overriding)
def view_layer_override(call: Call[Overriding]) -> Outcome[Answer]:
    """Toggle a setup material override on the active layer, the data setup writing the layer's data role with it."""
    return rooted(bpy.types.ViewLayer, call.context).bind(lambda layer: _overridden(layer, _Setup[call.properties.setup].value, call.identity)).map(lambda _: Done())


# --- [COMPOSITION] ----------------------------------------------------------------------


def _restrictions(identity: Identity) -> Outcome[tuple[RowContent, ...]]:
    """Builds the restriction grid, each cell pressing the restriction command of its layer and collection."""

    def toggled(line: bpy.types.UILayout, layer: str, cell: bpy.types.LayerCollection, collection: bpy.types.Collection) -> Outcome[None]:
        line.active = cell.is_visible
        return accumulated(
            operator_button(
                line,
                identity,
                Placement(collection_state, {"layer": layer, "collection": collection.session_uid, "flag": member.prop.identifier, "value": not on}),
                {"text": "", "icon": member.prop.icon, "depress": on},
            )
            for member in _RESTRICTIONS.values()
            for on in (bool(member.read(cell)),)
        ).map(lambda _: None)

    return Ok((TreeTable(tree=_grid, cell=toggled, marks=(Property(target=Member(root=bpy.types.Collection, path=("hide_render",)), keywords={"text": "", "icon_only": True}),)),))


LAYER_TAG: Final = Attachment(key="layer", form=LayerTag, owner=bpy.types.ViewLayer)
LAYERS: Final = Section(
    key="layers",
    title=Some("View Layers"),
    rows=(
        Layout(
            rows=(
                ItemList(
                    key="layers",
                    collection=Member(root=bpy.types.Scene, path=("view_layers",)),
                    active=Some(Member(root=bpy.types.Window, path=("view_layer",))),
                    attachment=Some(LAYER_TAG),
                    cells=(Property(target=Member(root=bpy.types.ViewLayer, path=("use",)), keywords={"text": ""}),),
                    move=Some(_moved),
                ),
            ),
            prerequisites=(LAYER_TAG,),
        ),
        lambda layout, _holder, _context: layout.separator(type="LINE"),
        Layout(
            rows=(
                *(OperatorButton(target=command) for command in (view_layer_solo, view_layer_rename, view_layer_tint, view_layer_split)),
                *(
                    OperatorButton(target=command, function=FUNCTIONS["operator_enum"], keywords={"property": member})
                    for command, member in ((view_layer_override, "setup"), (view_layer_copy, "part"), (view_layer_paste, "scope"))
                ),
            ),
            container=Some(lambda layout: layout.grid_flow(row_major=True, even_columns=True)),
        ),
    ),
)
OVERRIDES: Final = Section(
    key="layer_overrides",
    rows=(
        SettingsTable(
            scope=ViewLayers(),
            settings=(
                ("use",),
                ("material_override",),
                ("world_override",),
                ("samples",),
                *(
                    (prop.identifier,)
                    for prop in bpy.types.ViewLayer.bl_rna.properties
                    if isinstance(prop, bpy.types.BoolProperty) and not prop.is_readonly and prop.identifier.startswith("use_") and not prop.identifier.startswith("use_pass_")
                ),
                ("cycles", "use_denoising"),
            ),
        ),
        SettingsTable(scope=Scenes(), settings=(("cycles", "samples"),)),
    ),
)
COLLECTIONS: Final = Section(key="collection_states", title=Some("Collections"), rows=(Layout(builder=Some(_restrictions), prerequisites=(collection_state,)),))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "COLLECTIONS",
    "LAYERS",
    "LAYER_TAG",
    "OVERRIDES",
    "Clipboard",
    "CollectionState",
    "LayerTag",
    "Overriding",
    "Renaming",
    "Role",
    "Splitting",
    "Tinting",
    "collection_state",
    "colors",
    "view_layer_copy",
    "view_layer_override",
    "view_layer_paste",
    "view_layer_rename",
    "view_layer_solo",
    "view_layer_split",
    "view_layer_tint",
]
