"""View layer list, commands, and per-layer overrides."""

from collections.abc import Callable
import colorsys
from contextlib import ExitStack
import itertools
from typing import Annotated, ClassVar, Final

import bpy
from expression import Ok, Option, Result, Some
import mathutils
import msgspec
import numpy as np
import PyOpenColorIO

from blender.bpy.commands.operators import Call, Done, Empty, Operator, Outcome, Own, tallied, Unchanged
from blender.bpy.data.ids.store import IdName, vacant
from blender.bpy.data.rna import conditions
from blender.bpy.data.rna.descriptors import Descriptor, rooted
from blender.bpy.data.rna.snapshots import Scenes, ViewLayers
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Registrable
from blender.bpy.faults import BlenderEditFault, BlenderFaults, BlenderResult
from blender.bpy.render.color.spaces import config_of
from blender.bpy.render.view_layers.collections import applied, CollectionState, copying, Extent, Flag, isolates, pasting
from blender.bpy.scene.materials import Clay, material, Utility
from blender.bpy.settings.attachments import Attached, Attachment
from blender.bpy.ui.layout.lists import ItemList
from blender.bpy.ui.layout.rows import Caption, Flow, joined, Line, Matrix, Member, OperatorButton, placed, Property, RowDraw
from blender.bpy.ui.layout.sections import Single
from blender.imaging.faults import accumulated, paired
from blender.imaging.parameters import Case, Choice, Rgb
from blender.rendering.outputs.naming import Count, Fields, OutputName, Word

# --- [TYPES] ----------------------------------------------------------------------------

type Palette = Spectrum | Published


class Role(Choice):
    """View layer purpose."""

    BEAUTY = 1, "Beauty"
    DATA = 2, "Data"


class Direction(Choice):
    """Direction active layer moves in list."""

    UP = 1, "Up", "Move active view layer up the list"
    DOWN = 2, "Down", "Move active view layer down the list"


class Swatch(Choice):
    """Published color-blind safe palette with its sRGB swatches as byte triples."""

    swatches: bytes

    def __init__(self, value: int, label: str, swatches: str) -> None:
        """Sets member label and swatches from hexadecimal triples."""
        super().__init__(value, label)
        self.swatches = bytes.fromhex(swatches)

    IBM = 1, "IBM", "648FFF 785EF0 DC267F FE6100 FFB000"
    WONG = 2, "Wong", "000000 E69F00 56B4E9 009E73 F0E442 0072B2 D55E00 CC79A7"
    TOL = 3, "Tol", "332288 117733 44AA99 88CCEE DDCC77 CC6677 AA4499 882255"


class Others(Choice):
    """Flag every other top-level collection takes in a split layer."""

    INDIRECT_ONLY = 1, "Indirect Only"
    HOLDOUT = 2, "Holdout"
    EXCLUDE = 3, "Exclude"


class Within(Choice):
    """Scenes a rename reaches."""

    SCENE = 1, "This Scene"
    FILE = 2, "Every Scene"


# --- [CONSTANTS] ------------------------------------------------------------------------

_USE: Final = Descriptor.declared(bpy.types.ViewLayer, "use", bpy.types.BoolProperty)
_NAME: Final = Descriptor.declared(bpy.types.ViewLayer, "name", bpy.types.StringProperty)
_OVERRIDE: Final = Descriptor.declared(bpy.types.ViewLayer, "material_override", bpy.types.PointerProperty)
_LAYERS: Final = Descriptor.declared(bpy.types.Scene, "view_layers", bpy.types.CollectionProperty)

# --- [MODELS] ---------------------------------------------------------------------------


class Spectrum(Case, frozen=True, kw_only=True, tag=1):
    """Hues spaced evenly on color wheel."""

    title = "Spectrum"
    saturation: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Saturation", description="Saturation of every hue")] = 0.7
    value: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Value", description="Brightness of every hue")] = 0.36


class Published(Case, frozen=True, kw_only=True, tag=2):
    """Color-blind safe palette cycled over layers."""

    title = "Published"
    swatches: Annotated[Swatch, msgspec.Meta(title="Palette", description="Published palette the layers cycle through")] = Swatch.IBM


class Layer(msgspec.Struct, frozen=True, kw_only=True):
    """Purpose and tag of a view layer."""

    role: Annotated[Role, msgspec.Meta(title="Role", description="Beauty layers form image, data layers hold data passes and mattes")] = Role.BEAUTY
    color: Annotated[Rgb, msgspec.Meta(title="Tag", description="Layer color in lists and on its nodes")] = msgspec.field(default_factory=lambda: colors(Spectrum(saturation=0.0), 1)[0])


class Moving(msgspec.Struct, frozen=True, kw_only=True):
    """Active layer move."""

    direction: Annotated[Direction, msgspec.Meta(title="Direction", description="Direction active layer moves in list")] = Direction.UP


class Renaming(msgspec.Struct, frozen=True, kw_only=True):
    """Rename of every layer by its list position."""

    template: Annotated[str, msgspec.Meta(min_length=1, title="Names", description="Layer name grammar holding {order:<width>} and {layer}")] = "{order:2}_{layer}"
    within: Annotated[Within, msgspec.Meta(title="Scenes", description="Scenes rename reaches")] = Within.SCENE


class Tinting(msgspec.Struct, frozen=True, kw_only=True):
    """Tag colors written from a palette."""

    palette: Annotated[Palette, msgspec.Meta(title="Palette", description="Palette tags take in list order")] = Spectrum()


class Splitting(msgspec.Struct, frozen=True, kw_only=True):
    """Split of top-level collections into layers."""

    others: Annotated[Others, msgspec.Meta(title="Others", description="Flag other top-level collections take in a new layer")] = Others.INDIRECT_ONLY


class LayerOverride(Line, frozen=True, kw_only=True):
    """Row of layer list with move buttons and layer commands."""

    layer: ClassVar[Attachment[Layer]] = Attachment(key="layer", form=Layer, owner=bpy.types.ViewLayer)
    prerequisites: ClassVar[tuple[Registrable, ...]] = (layer,)

    def listed(self) -> ItemList[Layer, bpy.types.ViewLayer]:
        """Returns layer list with use column and move buttons."""
        return ItemList(
            key="layers",
            item=bpy.types.ViewLayer,
            collection=Member(root=bpy.types.Scene, path="view_layers"),
            active=Member(root=bpy.types.Window, path="view_layer"),
            attachment=Some(self.layer),
            columns=(Property(target=Member(root=bpy.types.ViewLayer, path="use"), text=""),),
            side=tuple(
                OperatorButton(target=Own(command=MOVE, values=Moving(direction=direction)), text="", icon=Some("TRIA_UP" if direction is Direction.UP else "TRIA_DOWN")) for direction in Direction
            ),
        )

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Draws layer list over command grid."""
        return placed((self.listed(), Caption(rows=tuple(OperatorButton(target=action) for action in ACTIONS), flow=Flow.GRID)), identity).map(joined)

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns layer list and commands."""
        return Ok((self.listed(), MOVE, *ACTIONS))

    def section(self) -> Single[Empty]:
        """Returns view layer list section."""
        return Single(key="layers", title="View Layers", rows=(self,))

    @staticmethod
    def overrides() -> Single[Empty]:
        """Returns override grid over view layers and scene samples."""
        filters = tuple(
            prop.identifier
            for prop in bpy.types.ViewLayer.bl_rna.properties
            if isinstance(prop, bpy.types.BoolProperty) and not prop.is_readonly and prop.identifier.startswith("use_") and not prop.identifier.startswith("use_pass_")
        )
        return Single(
            key="layer_overrides",
            title="Layer Overrides",
            rows=(
                Matrix(scope=ViewLayers(), settings=("use", "material_override", "world_override", "samples", *filters, "cycles.use_denoising")),
                Matrix(scope=Scenes(), settings=("cycles.samples",)),
            ),
        )


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [LAYERS]


def colors(palette: Palette, count: int) -> tuple[Rgb, ...]:
    """Returns scene-linear palette colors in list order."""
    match palette:
        case Spectrum(saturation=saturation, value=value):
            encoded = tuple(colorsys.hsv_to_rgb(index / count, saturation, value) for index in range(count))
        case Published(swatches=chosen):
            encoded = tuple(map(tuple, np.resize(np.frombuffer(chosen.swatches, np.uint8).reshape(-1, 3), (count, 3)) / 255))
    return tuple(Rgb(red=linear.r, green=linear.g, blue=linear.b) for linear in (mathutils.Color(rgb).from_srgb_to_scene_linear() for rgb in encoded))


def encoded(color: Rgb) -> tuple[float, float, float]:
    """Returns sRGB form of a scene-linear color, as `Node.color` and document rasters hold."""
    shown = mathutils.Color((color.red, color.green, color.blue)).from_scene_linear_to_srgb()
    return (shown.r, shown.g, shown.b)


def tinted(scene: bpy.types.Scene, palette: Palette, attached: Attached[Layer]) -> BlenderResult[Outcome]:
    """Writes palette tags onto layers in list order where they differ."""
    layers = tuple(scene.view_layers)

    def wrote(held: tuple[Layer, ...]) -> BlenderResult[Outcome]:
        if not (differing := tuple((layer, record, color) for layer, record, color in zip(layers, held, colors(palette, len(layers))) if record.color != color)):
            return Ok(Unchanged(reason=f"View layers of {scene.name} hold palette tags"))
        return tallied(tuple(attached.write(layer, msgspec.structs.replace(record, color=color)) for layer, record, color in differing))

    return accumulated(map(attached.read, layers)).bind(wrote)


def renamed(scene: bpy.types.Scene, grammar: OutputName) -> BlenderResult[int]:
    """Names each layer from list position and base name, returning renamed count."""

    def named(index: int, layer: bpy.types.ViewLayer) -> BlenderResult[bool]:
        base = grammar.parse(layer.name).to_option().bind(lambda fields: Option.of_optional(fields.words.get(Word.LAYER))).default_value(layer.name)
        wanted: BlenderResult[str] = grammar.compose(Fields(words={Word.LAYER: base}, counts={Count.ORDER: index + 1}))
        return Result[str, BlenderFaults].bind(wanted, lambda value: _NAME.write(layer, value)).map(Option.is_some)

    return accumulated(itertools.starmap(named, enumerate(scene.view_layers))).map(sum)


def split(scene: bpy.types.Scene, flag: Flag) -> BlenderResult[tuple[str, ...]]:
    """Adds one layer per top-level collection no layer isolates, removing every added layer on fault."""
    lone = tuple(collection for collection in scene.collection.children if not any(isolates(layer, collection, flag) for layer in scene.view_layers))
    with ExitStack() as rollback:

        def made(collection: bpy.types.Collection) -> BlenderResult[str]:
            layer = scene.view_layers.new(vacant(collection.name, scene.view_layers.keys()))
            rollback.callback(scene.view_layers.remove, layer)
            return applied(CollectionState(layer=layer.name, collection=IdName.of(collection), flag=flag, value=False, extent=Extent.ISOLATE), scene).map(lambda _: layer.name)

        if (added := (accumulated(map(made, lone)))).is_ok():
            rollback.pop_all()
        return added


def overridden(layer: bpy.types.ViewLayer, setup: Clay | Utility, config: PyOpenColorIO.Config, identity: Identity) -> BlenderResult[None]:
    """Toggles layer material override to owned setup material, data setups writing layer role with it."""

    def toggled(held: bpy.types.Material) -> BlenderResult[None]:
        on = not _OVERRIDE.equal(_OVERRIDE.read(layer), IdName.of(held))
        wrote = _OVERRIDE.write(layer, held if on else None)
        match setup:
            case Utility():
                role = LayerOverride.layer.revised(identity, layer, lambda record: Ok(msgspec.structs.replace(record, role=Role.DATA if on else Role.BEAUTY)))
                return paired(wrote, role).map(lambda _: None)
            case Clay():
                return wrote.map(lambda _: None)

    return material(setup, config, identity).bind(toggled)


# --- [COMMANDS]


def moved(call: Call[Moving]) -> BlenderResult[Outcome]:
    """Moves the active layer one place in the list, `Unchanged` at the list end."""

    def shifted(scene: bpy.types.Scene, layer: bpy.types.ViewLayer) -> BlenderResult[Outcome]:
        index = scene.view_layers.find(layer.name)
        match call.record.direction:
            case Direction.UP:
                target = index - 1
            case Direction.DOWN:
                target = index + 1
        if not 0 <= target < len(scene.view_layers):
            return Ok(Unchanged(reason=f"{layer.name} is at list end"))
        return BlenderEditFault.attempted(repr(scene.view_layers), lambda: scene.view_layers.move(index, target)).map(lambda _: Done())

    return paired(call.root(bpy.types.Scene), call.root(bpy.types.ViewLayer)).bind(lambda pair: shifted(*pair))


def soloed(call: Call[Empty]) -> BlenderResult[Outcome]:
    """Renders the active layer alone."""
    return paired(call.root(bpy.types.Scene), call.root(bpy.types.ViewLayer)).bind(
        lambda pair: accumulated(_USE.write(layer, layer == pair[1]) for layer in pair[0].view_layers).map(
            lambda priors: Done() if any(prior.is_some() for prior in priors) else Unchanged(reason=f"{pair[1].name} renders alone")
        )
    )


def renaming(call: Call[Renaming]) -> BlenderResult[Outcome]:
    """Renames the layers of the scenes the rename reaches."""
    scenes: BlenderResult[tuple[bpy.types.Scene, ...]]
    match call.record.within:
        case Within.SCENE:
            scenes = rooted(bpy.types.Scene, call.context).map(lambda scene: (scene,))
        case Within.FILE:
            scenes = rooted(bpy.types.BlendData, call.context).map(lambda data: tuple(data.scenes))
    return (
        paired(OutputName.of(call.record.template), scenes)
        .bind(lambda pair: accumulated(renamed(scene, pair[0]) for scene in pair[1]))
        .map(lambda counts: Done(notes=(("INFO", f"Renamed {sum(counts)} view layers"),)) if sum(counts) else Unchanged(reason="View layers hold their composed names"))
    )


def overriding(setup: Clay | Utility) -> Callable[[Call[Empty]], BlenderResult[Done]]:
    """Returns the body toggling the active layer's override to the setup material."""
    return lambda call: paired(config_of(), call.root(bpy.types.ViewLayer)).bind(lambda pair: overridden(pair[1], setup, pair[0], call.identity)).map(lambda _: Done())


# --- [COMPOSITION] ----------------------------------------------------------------------

MOVE: Final = Operator(
    key="view_layer_move",
    title="Move View Layer",
    description="Move active view layer in list",
    form=Moving,
    run=moved,
    condition=Some(conditions.Count(collection=_LAYERS, least=2)),
    undo="Move View Layer",
)
ACTIONS: Final[tuple[Operator[Empty] | Operator[Renaming] | Operator[Tinting] | Operator[Splitting], ...]] = (
    Operator(key="view_layer_solo", title="Solo View Layer", description="Render active view layer alone", form=Empty, run=soloed),
    Operator(key="view_layer_rename", title="Rename View Layers", description="Name view layers by list position and base name", form=Renaming, run=renaming),
    Operator(
        key="view_layer_tint",
        title="Tint View Layers",
        description="Set view layer tags from palette in list order",
        form=Tinting,
        run=lambda call: paired(call.root(bpy.types.Scene), LayerOverride.layer.bound(call.identity)).bind(lambda pair: tinted(pair[0], call.record.palette, pair[1])),
    ),
    Operator(
        key="view_layer_split",
        title="View Layers from Collections",
        description="Add a view layer per top-level collection no layer isolates",
        form=Splitting,
        run=lambda call: (
            Result[bpy.types.Scene, BlenderFaults]
            .bind(call.root(bpy.types.Scene), lambda scene: split(scene, Flag[call.record.others.name]))
            .map(lambda added: Done(notes=tuple(("INFO", f"Added {name}") for name in added)) if added else Unchanged(reason="Each top-level collection has a layer isolating it"))
        ),
    ),
    Operator(key="view_layer_clay", title="Clay Override", description="Toggle clay material override on active layer", form=Empty, run=overriding(Clay())),
    Operator(key="view_layer_data", title="Data Layer", description="Toggle data material override and role on active layer", form=Empty, run=overriding(Utility())),
    Operator(key="view_layer_copy", title="Copy View Layer", description="Copy active view layer settings", form=Empty, undo=False, run=copying(lambda layer: layer)),
    Operator(
        key="view_layer_paste",
        title="Paste View Layer",
        description="Paste settings into active view layer",
        form=Empty,
        run=pasting(lambda call: call.root(bpy.types.ViewLayer).map(lambda layer: (layer,)), "View layers match clipboard"),
    ),
    Operator(
        key="view_layer_paste_all",
        title="Paste to Used Layers",
        description="Paste settings into every used view layer",
        form=Empty,
        run=pasting(
            lambda call: (
                Result[bpy.types.Scene, BlenderFaults]
                .bind(call.root(bpy.types.Scene), lambda scene: ViewLayers(scenes=(scene.name,)).owners(call.context))
                .map(lambda found: tuple(layer for layer in found if bool(_USE.read(layer))))
            ),
            "View layers match clipboard",
        ),
    ),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "ACTIONS",
    "MOVE",
    "Direction",
    "Layer",
    "LayerOverride",
    "Moving",
    "Others",
    "Palette",
    "Published",
    "Renaming",
    "Role",
    "Spectrum",
    "Splitting",
    "Swatch",
    "Tinting",
    "Within",
    "colors",
    "encoded",
    "moved",
    "overridden",
    "overriding",
    "renamed",
    "renaming",
    "soloed",
    "split",
    "tinted",
]
