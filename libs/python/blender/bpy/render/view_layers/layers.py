"""View layers: the layer list with tags and roles, setup overrides, collection restrictions, palette tints, renames, splits, and the settings clipboard."""

from collections.abc import Mapping
import colorsys
from contextlib import ExitStack
from enum import auto, Enum
from functools import partial
import itertools
from types import MappingProxyType
from typing import Final

import bpy
from expression import Nothing, Ok, Option, Result, Some
from expression.collections import Block
from expression.extra.result import catch
import mathutils
import numpy as np

from blender.bpy.commands.calls import Placement
from blender.bpy.commands.operators import Answer, answered, Call, Done, Empty, Operator, Unchanged
from blender.bpy.data.rna.conditions import Scenes, ViewLayers
from blender.bpy.data.rna.descriptors import Descriptor, Value, written
from blender.bpy.data.rna.mirrors import choices, enum_choices
from blender.bpy.data.rna.paths import rooted
from blender.bpy.data.rna.snapshots import Applied, Snapshot
from blender.bpy.extension.identity import Identity
from blender.bpy.render.view_layers.passes import layer_of, soloed, used
from blender.bpy.scene.collections import LayerPath, walked
from blender.bpy.scene.materials import Clay, material, Utility
from blender.bpy.settings.attachments import Attached, Attachment
from blender.bpy.ui.layout.lists import ItemList
from blender.bpy.ui.layout.rows import FUNCTIONS, Layout, Member, OperatorButton, pressed, Property, RowContent
from blender.bpy.ui.layout.sections import Section
from blender.bpy.ui.layout.tables import Branch, Matrix, TreeMatrix
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired, Reason
from blender.rendering.outputs.naming import Fields, NumberField, OutputName, Word

# --- [TYPES] ----------------------------------------------------------------------------


class Spread(Enum):
    """Collections a restriction edit reaches."""

    ONE = auto()
    CHILDREN = auto()
    ISOLATE = auto()


class Role(Enum):
    """View layer purpose."""

    BEAUTY = auto()
    DATA = auto()


class Swatch(Enum):
    """Published color-blind safe palette, valued by its sRGB hex colors."""

    IBM = "648FFF 785EF0 DC267F FE6100 FFB000"
    WONG = "000000 E69F00 56B4E9 009E73 F0E442 0072B2 D55E00 CC79A7"
    TOL = "332288 117733 44AA99 88CCEE DDCC77 CC6677 AA4499 882255"


class Within(Enum):
    """Scenes a rename reaches."""

    SCENE = auto()
    FILE = auto()


class Setup(Enum):
    """Material override a layer toggles, valued by its owned surface setup."""

    CLAY = Clay()
    DATA = Utility()


class Part(Enum):
    """Settings the clipboard copies off the active layer."""

    LAYER = auto()
    COLLECTIONS = auto()


class Reached(Enum):
    """View layers a paste reaches."""

    ACTIVE = auto()
    USED = auto()
    ALL = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

_SATURATION: Final = 0.7
_VALUE: Final = 0.36
RESTRICTIONS: Final[Mapping[str, Descriptor[bpy.types.BoolProperty]]] = MappingProxyType({
    prop.identifier: Descriptor.declared(bpy.types.LayerCollection, (prop.identifier,), bpy.types.BoolProperty)
    for prop in bpy.types.LayerCollection.bl_rna.properties
    if isinstance(prop, bpy.types.BoolProperty) and not prop.is_readonly
})
_FLAGS: Final = choices((member.prop.identifier, member.prop.name, member.prop.description) for member in RESTRICTIONS.values())

# --- [MODELS] ---------------------------------------------------------------------------


class CollectionState(bpy.types.PropertyGroup):
    """Restriction value of a collection in one view layer."""

    layer: bpy.props.StringProperty(name="View Layer", description="View layer the edit writes")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    collection: bpy.props.IntProperty(name="Collection", description="Session identifier of the collection the edit writes")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    flag: bpy.props.EnumProperty(name="Restriction", description="Restriction the edit writes", items=_FLAGS)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    value: bpy.props.BoolProperty(name="Value", description="Restriction value written")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    extent: bpy.props.EnumProperty(name="Extent", description="Collections the edit reaches", items=enum_choices(Spread))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class LayerTag(bpy.types.PropertyGroup):
    """Purpose and tag of a view layer."""

    role: bpy.props.EnumProperty(name="Role", description="Beauty layers form image, data layers hold data passes and mattes", items=enum_choices(Role))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    color: bpy.props.FloatVectorProperty(  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
        name="Tag", description="Layer color in lists and on its nodes", subtype="COLOR", min=0.0, max=1.0, default=mathutils.Color((_VALUE,) * 3).from_srgb_to_scene_linear()[:]
    )


class Renaming(bpy.types.PropertyGroup):
    """Rename of every layer by its list position."""

    template: bpy.props.StringProperty(name="Names", description="Layer name grammar holding {order:<width>} and {layer}", default="{order:2}_{layer}")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    within: bpy.props.EnumProperty(name="Scenes", description="Scenes rename reaches", items=enum_choices(Within))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Tinting(bpy.types.PropertyGroup):
    """Tag colors written from a palette."""

    palette: bpy.props.EnumProperty(  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
        name="Palette",
        description="Palette tags take in list order",
        items=choices((("SPECTRUM", "Spectrum", "Hues spaced evenly on color wheel"), *((swatch.name, swatch.name, "Color-blind safe palette cycled over layers") for swatch in Swatch))),
    )
    saturation: bpy.props.FloatProperty(name="Saturation", description="Saturation of every spectrum hue", subtype="FACTOR", min=0.0, max=1.0, default=_SATURATION)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    value: bpy.props.FloatProperty(name="Value", description="Brightness of every spectrum hue", subtype="FACTOR", min=0.0, max=1.0, default=_VALUE)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Splitting(bpy.types.PropertyGroup):
    """Split of top-level collections into layers."""

    others: bpy.props.EnumProperty(name="Others", description="Restriction other top-level collections take in a new layer", items=_FLAGS, default="indirect_only")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Overriding(bpy.types.PropertyGroup):
    """Setup material override toggled on the active layer."""

    setup: bpy.props.EnumProperty(name="Setup", description="Owned surface setup the override applies", items=enum_choices(Setup))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Clipboard(bpy.types.PropertyGroup):
    """Settings copied off the active layer and the layers a paste reaches."""

    part: bpy.props.EnumProperty(name="Part", description="View layer settings or collection states the copy takes", items=enum_choices(Part))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    scope: bpy.props.EnumProperty(name="Layers", description="View layers the paste writes", items=enum_choices(Reached))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


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
        target: Outcome[LayerPath] = found(held, collection).to_result_with(lambda: Fault.of(Reason.ABSENT, repr(held.layer_collection), member=str(collection)))
        return Result[LayerPath, Faults].bind(target, lambda path: written(held, *path))

    return layer_of(scene, layer).bind(located)


def grid(context: bpy.types.Context) -> tuple[tuple[str, ...], tuple[Branch[bpy.types.Collection, bpy.types.LayerCollection], ...]]:
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


def moved(context: bpy.types.Context, current: int, row: int) -> Outcome[None]:
    """Moves the context scene's view layer at `current` to `row`."""
    return rooted(bpy.types.Scene, context).bind(
        lambda scene: catch(scene.view_layers.move, exception=RuntimeError)(current, row).map(lambda _: None).map_error(partial(Fault.of, Reason.REFUSED, repr(scene.view_layers)))
    )


def colors(count: int, swatch: Option[Swatch] = Nothing, *, saturation: float = _SATURATION, value: float = _VALUE) -> tuple[tuple[float, float, float], ...]:
    """Returns scene-linear palette colors in list order, the swatch cycled or hues spaced evenly on the color wheel without one."""
    encoded = swatch.map(lambda chosen: tuple(map(tuple, np.resize(np.frombuffer(bytes.fromhex(chosen.value), np.uint8).reshape(-1, 3), (count, 3)) / 255))).default_with(
        lambda: tuple(colorsys.hsv_to_rgb(index / count, saturation, value) for index in range(count))
    )
    return tuple((red, green, blue) for red, green, blue in (mathutils.Color(rgb).from_srgb_to_scene_linear()[:] for rgb in encoded))


def tinted(scene: bpy.types.Scene, attached: Attached[LayerTag], chosen: tuple[tuple[float, float, float], ...]) -> Outcome[Answer]:
    """Writes the chosen tags onto layers in list order where they differ."""
    return accumulated(written(attached.struct(layer), ("color",), color) for layer, color in zip(scene.view_layers, chosen)).map(
        lambda priors: Done() if any(prior.is_some() for prior in priors) else Unchanged(reason=f"View layers of {scene.name} hold palette tags")
    )


def renamed(scene: bpy.types.Scene, grammar: OutputName) -> Outcome[int]:
    """Names each layer from list position and base name, returning renamed count."""

    def named(index: int, layer: bpy.types.ViewLayer) -> Outcome[bool]:
        base = grammar.parse(layer.name).to_option().bind(lambda fields: Option.of_optional(fields.words.get(Word.LAYER))).default_value(layer.name)
        wanted: Outcome[str] = grammar.compose(Fields(words={Word.LAYER: base}, counts={NumberField.ORDER: index + 1}))
        return Result[str, Faults].bind(wanted, lambda value: written(layer, ("name",), value)).map(Option.is_some)

    return accumulated(itertools.starmap(named, enumerate(scene.view_layers))).map(sum)


def split(scene: bpy.types.Scene, flag: Descriptor[bpy.types.BoolProperty]) -> Outcome[tuple[str, ...]]:
    """Adds one layer per top-level collection no layer isolates, the others under flag, removing every added layer on fault."""
    lone = tuple(
        collection
        for collection in scene.collection.children
        if not any(all(flag.read(child) == (child.collection != collection) for child in layer.layer_collection.children) for layer in scene.view_layers)
    )
    with ExitStack() as rollback:

        def made(collection: bpy.types.Collection) -> Outcome[str]:
            layer = scene.view_layers.new(collection.name)
            rollback.callback(scene.view_layers.remove, layer)
            return applied(scene, layer=layer.name, collection=collection.session_uid, flag=flag, value=False, extent=Spread.ISOLATE).map(lambda _: layer.name)

        if (added := accumulated(map(made, lone))).is_ok():
            rollback.pop_all()
        return added


def overridden(layer: bpy.types.ViewLayer, setup: Clay | Utility, identity: Identity) -> Outcome[None]:
    """Toggles layer material override to owned setup material, the data setup writing layer role with it."""

    def toggled(held: bpy.types.Material) -> Outcome[None]:
        on = layer.material_override != held
        layer.material_override = held if on else None
        match setup:
            case Utility():
                return LAYER.read(identity, layer).bind(lambda group: written(group, ("role",), (Role.DATA if on else Role.BEAUTY).name)).map(lambda _: None)
            case Clay():
                return Ok(None)

    return material(setup, identity).bind(toggled)


# --- [COMMANDS]


def _seeded(_properties: CollectionState, event: bpy.types.Event, _identity: Identity) -> dict[str, Value]:
    """Seeds the restriction extent from held modifiers, Ctrl to isolate and Shift to include children."""
    return {"extent": (Spread.ISOLATE if event.ctrl else Spread.CHILDREN if event.shift else Spread.ONE).name}


@Operator.of(CollectionState, internal=True, seed=Some(_seeded))
def collection_state(call: Call[CollectionState]) -> Outcome[Answer]:
    """Set collection restriction, Ctrl to isolate, Shift to include children."""
    held = call.properties
    return (
        rooted(bpy.types.Scene, call.context)
        .bind(lambda scene: applied(scene, layer=held.layer, collection=held.collection, flag=RESTRICTIONS[held.flag], value=held.value, extent=Spread[held.extent]))
        .map(lambda count: Done(notes=(("INFO", f"Wrote {count} collection flags"),)) if count else Unchanged(reason="Collection flags hold requested value"))
    )


@Operator.of(Clipboard, undo=Nothing)
def view_layer_copy(call: Call[Clipboard]) -> Outcome[Answer]:
    """Copy active view layer settings or its collection states."""
    part = Part[call.properties.part]
    return (
        paired(rooted(bpy.types.ViewLayer, call.context).bind(lambda layer: Snapshot.capture(layer if part is Part.LAYER else layer.layer_collection)), rooted(bpy.types.WindowManager, call.context))
        .map(lambda pair: pair[0].copied(pair[1]))
        .map(lambda _: Done())
    )


@Operator.of(Clipboard)
def view_layer_paste(call: Call[Clipboard]) -> Outcome[Answer]:
    """Paste copied view layer settings or collection states into the view layers the scope reaches."""

    def reached(scene: bpy.types.Scene, active: bpy.types.ViewLayer) -> tuple[bpy.types.ViewLayer, ...]:
        match Reached[call.properties.scope]:
            case Reached.ACTIVE:
                return (active,)
            case Reached.USED:
                return used(scene)
            case Reached.ALL:
                return tuple(scene.view_layers)

    return (
        paired(rooted(bpy.types.WindowManager, call.context), rooted(bpy.types.Scene, call.context), rooted(bpy.types.ViewLayer, call.context))
        .bind(lambda found: Snapshot.pasted(found[0], tuple(owner for layer in reached(found[1], found[2]) for owner in (layer, layer.layer_collection))))
        .bind(lambda outcomes: answered(Applied.join(outcomes), "View layers match clipboard"))
    )


@Operator.of(Empty)
def view_layer_solo(call: Call[Empty]) -> Outcome[Answer]:
    """Render active view layer alone."""
    return paired(rooted(bpy.types.Scene, call.context), rooted(bpy.types.ViewLayer, call.context)).map(
        lambda pair: Done() if soloed(pair[0], pair[1].name) else Unchanged(reason=f"{pair[1].name} renders alone")
    )


@Operator.of(Renaming)
def view_layer_rename(call: Call[Renaming]) -> Outcome[Answer]:
    """Name view layers by list position and base name."""
    scenes: Outcome[tuple[bpy.types.Scene, ...]]
    match Within[call.properties.within]:
        case Within.SCENE:
            scenes = rooted(bpy.types.Scene, call.context).map(lambda scene: (scene,))
        case Within.FILE:
            scenes = rooted(bpy.types.BlendData, call.context).map(lambda data: tuple(data.scenes))
    return (
        paired(OutputName.of(call.properties.template), scenes)
        .bind(lambda pair: accumulated(renamed(scene, pair[0]) for scene in pair[1]))
        .map(lambda counts: Done(notes=(("INFO", f"Renamed {sum(counts)} view layers"),)) if sum(counts) else Unchanged(reason="View layers hold their composed names"))
    )


@Operator.of(Tinting)
def view_layer_tint(call: Call[Tinting]) -> Outcome[Answer]:
    """Set view layer tags from palette in list order."""
    held = call.properties
    swatch = Option.of_optional(Swatch.__members__.get(held.palette))
    return paired(rooted(bpy.types.Scene, call.context), LAYER.bound(call.identity)).bind(
        lambda pair: tinted(pair[0], pair[1], colors(len(pair[0].view_layers), swatch, saturation=held.saturation, value=held.value))
    )


@Operator.of(Splitting)
def view_layer_split(call: Call[Splitting]) -> Outcome[Answer]:
    """Add a view layer per top-level collection no layer isolates."""
    return (
        rooted(bpy.types.Scene, call.context)
        .bind(lambda scene: split(scene, RESTRICTIONS[call.properties.others]))
        .map(lambda added: Done(notes=tuple(("INFO", f"Added {name}") for name in added)) if added else Unchanged(reason="Each top-level collection has a layer isolating it"))
    )


@Operator.of(Overriding)
def view_layer_override(call: Call[Overriding]) -> Outcome[Answer]:
    """Toggle a setup material override on the active layer, the data setup writing the layer's data role with it."""
    return rooted(bpy.types.ViewLayer, call.context).bind(lambda layer: overridden(layer, Setup[call.properties.setup].value, call.identity)).map(lambda _: Done())


# --- [COMPOSITION] ----------------------------------------------------------------------


def _restrictions(identity: Identity) -> Outcome[tuple[RowContent, ...]]:
    """Builds the restriction grid, each cell pressing the restriction command of its layer and collection."""

    def toggled(line: bpy.types.UILayout, layer: str, cell: bpy.types.LayerCollection, collection: bpy.types.Collection) -> Outcome[None]:
        line.active = cell.is_visible
        return accumulated(
            pressed(
                line,
                identity,
                Placement(collection_state, {"layer": layer, "collection": collection.session_uid, "flag": member.prop.identifier, "value": not on}),
                {"text": "", "icon": member.prop.icon, "depress": on},
            )
            for member in RESTRICTIONS.values()
            for on in (bool(member.read(cell)),)
        ).map(lambda _: None)

    return Ok((TreeMatrix(tree=grid, cell=toggled, marks=(Property(target=Member(root=bpy.types.Collection, path=("hide_render",)), keywords={"text": "", "icon_only": True}),)),))


def _enumerated(command: Operator[Clipboard] | Operator[Overriding], member: str) -> OperatorButton:
    """One button per item of the command's enum property, under the item's label."""
    return OperatorButton(target=command, function=FUNCTIONS["operator_enum"], keywords={"property": member})


LAYER: Final = Attachment(key="layer", form=LayerTag, owner=bpy.types.ViewLayer)
LAYERS: Final = Section(
    key="layers",
    title="View Layers",
    rows=(
        Layout(
            rows=(
                ItemList(
                    key="layers",
                    item=bpy.types.ViewLayer,
                    collection=Member(root=bpy.types.Scene, path=("view_layers",)),
                    active=Some(Member(root=bpy.types.Window, path=("view_layer",))),
                    attachment=Some(LAYER),
                    cells=(Property(target=Member(root=bpy.types.ViewLayer, path=("use",)), keywords={"text": ""}),),
                    move=Some(moved),
                ),
            ),
            prerequisites=(LAYER,),
        ),
        lambda layout, _holder, _context: layout.separator(type="LINE"),
        Layout(
            rows=(
                *(OperatorButton(target=command) for command in (view_layer_solo, view_layer_rename, view_layer_tint, view_layer_split)),
                _enumerated(view_layer_override, "setup"),
                _enumerated(view_layer_copy, "part"),
                _enumerated(view_layer_paste, "scope"),
            ),
            container=Some(lambda layout: layout.grid_flow(row_major=True, even_columns=True)),
        ),
    ),
)
OVERRIDES: Final = Section(
    key="layer_overrides",
    title="Layer Overrides",
    rows=(
        Matrix(
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
        Matrix(scope=Scenes(), settings=(("cycles", "samples"),)),
    ),
)
COLLECTIONS: Final = Section(key="collection_states", title="Collections", rows=(Layout(rows=_restrictions, prerequisites=(collection_state,)),))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "COLLECTIONS",
    "LAYER",
    "LAYERS",
    "OVERRIDES",
    "RESTRICTIONS",
    "Clipboard",
    "CollectionState",
    "LayerTag",
    "Overriding",
    "Part",
    "Reached",
    "Renaming",
    "Role",
    "Setup",
    "Splitting",
    "Spread",
    "Swatch",
    "Tinting",
    "Within",
    "applied",
    "collection_state",
    "colors",
    "found",
    "grid",
    "moved",
    "overridden",
    "renamed",
    "split",
    "tinted",
    "view_layer_copy",
    "view_layer_override",
    "view_layer_paste",
    "view_layer_rename",
    "view_layer_solo",
    "view_layer_split",
    "view_layer_tint",
]
