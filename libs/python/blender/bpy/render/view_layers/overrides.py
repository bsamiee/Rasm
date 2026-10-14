"""View layer list, order, roles, tags, names, and per-layer overrides."""

from collections.abc import Callable
import colorsys
from contextlib import ExitStack
import itertools
from typing import Annotated, ClassVar, Self

import bpy
from expression import Ok, Option, Result, Some
from expression.extra.result import catch
import mathutils
import msgspec
import PyOpenColorIO

from blender.bpy.commands.operators import answered, Call, Done, Empty, Operator, Outcome, Own, tallied, Unchanged
from blender.bpy.data.rna import conditions
from blender.bpy.data.rna.descriptors import Descriptor, IdName
from blender.bpy.data.rna.snapshots import Applied, Scenes, Snapshot, ViewLayers
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderAbsentFault, BlenderEditFault, BlenderFaults, BlenderImagingFault, BlenderRenderingFault, BlenderResult
from blender.bpy.render.view_layers.collections import applied, CollectionState, Extent, Flag, flags, isolates
from blender.bpy.scene.materials import Clay, material, Utility
from blender.bpy.settings.attachments import Attached, Attachment
from blender.bpy.settings.presets import copied, pasted
from blender.bpy.ui.layout.lists import ItemList
from blender.bpy.ui.layout.rows import Caption, Flow, joined, Line, Matrix, Member, OperatorButton, placed, Property, RowDraw
from blender.bpy.ui.layout.sections import Single
from blender.imaging.color.config import admitted
from blender.imaging.faults import accumulated, paired
from blender.imaging.parameters import Case, Choice, Rgb
from blender.rendering.outputs.naming import Count, Fields, OutputName, Word

# --- [TYPES] ----------------------------------------------------------------------------

type Palette = Spectrum | Published


class Role(Choice):
    """Purpose of a view layer."""

    BEAUTY = 1, "Beauty"
    DATA = 2, "Data"


class Direction(Choice):
    """Way the active layer moves in the list."""

    UP = 1, "Up"
    DOWN = 2, "Down"


class Swatch(Choice):
    """Published color-blind safe palette, each member holding its sRGB swatches."""

    swatches: tuple[int, ...]

    def __new__(cls, value: int, label: str, *swatches: int) -> Self:
        """Creates a member from its persistent number, its label, and its swatches."""
        member = int.__new__(cls, value)
        member._value_, member.label, member.swatches = value, label, swatches
        return member

    IBM = 1, "IBM", 0x648FFF, 0x785EF0, 0xDC267F, 0xFE6100, 0xFFB000
    WONG = 2, "Wong", 0x000000, 0xE69F00, 0x56B4E9, 0x009E73, 0xF0E442, 0x0072B2, 0xD55E00, 0xCC79A7
    TOL = 3, "Tol", 0x332288, 0x117733, 0x44AA99, 0x88CCEE, 0xDDCC77, 0xCC6677, 0xAA4499, 0x882255


class Others(Choice):
    """Flag every other top-level collection takes in a split layer."""

    INDIRECT_ONLY = 1, "Indirect Only"
    HOLDOUT = 2, "Holdout"
    EXCLUDE = 3, "Exclude"


class Within(Choice):
    """Scenes a rename reaches."""

    SCENE = 1, "This Scene"
    FILE = 2, "Every Scene"


# --- [MODELS] ---------------------------------------------------------------------------


class Spectrum(Case, frozen=True, kw_only=True, tag=1):
    """Hues spaced evenly around the color wheel."""

    title = "Spectrum"
    saturation: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Saturation", description="Saturation of every hue")] = 0.7
    value: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Value", description="Brightness of every hue")] = 0.36


class Published(Case, frozen=True, kw_only=True, tag=2):
    """Published color-blind safe palette cycled over the layers."""

    title = "Published"
    swatches: Annotated[Swatch, msgspec.Meta(title="Palette", description="Published palette the layers cycle through")] = Swatch.IBM


class Layer(msgspec.Struct, frozen=True, kw_only=True):
    """Purpose and tag of a view layer."""

    role: Annotated[Role, msgspec.Meta(title="Role", description="Beauty layers form the image, data layers hold data passes and mattes")] = Role.BEAUTY
    color: Annotated[Rgb, msgspec.Meta(title="Tag", description="Color marking the layer in lists and on its nodes")] = msgspec.field(default_factory=lambda: colors(Spectrum(saturation=0.0), 1)[0])


class Moving(msgspec.Struct, frozen=True, kw_only=True):
    """Move of the active layer."""

    direction: Annotated[Direction, msgspec.Meta(title="Direction", description="Way the active layer moves in the list")] = Direction.UP


class Renaming(msgspec.Struct, frozen=True, kw_only=True):
    """Rename of every layer by its list position."""

    template: Annotated[str, msgspec.Meta(min_length=1, title="Names", description="Layer name grammar holding {order:<width>} and {layer}")] = "{order:2}_{layer}"
    within: Annotated[Within, msgspec.Meta(title="Scenes", description="Scenes whose layers the rename reaches")] = Within.SCENE


class Tinting(msgspec.Struct, frozen=True, kw_only=True):
    """Tag colors written from a palette."""

    palette: Annotated[Palette, msgspec.Meta(title="Palette", description="Palette the layer tags take in list order")] = Spectrum()


class Splitting(msgspec.Struct, frozen=True, kw_only=True):
    """Split of the top-level collections into layers."""

    others: Annotated[Others, msgspec.Meta(title="Others", description="Flag every other top-level collection takes in a new layer")] = Others.INDIRECT_ONLY


class LayerCommands(msgspec.Struct, frozen=True, kw_only=True):
    """Operator rows of the layer list."""

    move: Operator[Moving]
    actions: tuple[Operator[Empty] | Operator[Renaming] | Operator[Tinting] | Operator[Splitting], ...]


class LayerOverride(Line, frozen=True, kw_only=True):
    """Row placing the layer attachment, the layer list with its operator rows, and the override grid."""

    layer: ClassVar[Attachment[Layer]] = Attachment(key="layer", form=Layer, owner=bpy.types.ViewLayer)

    def commands(self, identity: Identity) -> BlenderResult[LayerCommands]:
        """Builds the list's operator rows over descriptors resolved once.

        Args:
            identity: Extension placing the row.

        Returns:
            `Ok` of the rows; `Error` of `register`, `absent`, or `choice` for each refused binding or member.
        """
        members = paired(
            paired(Descriptor.resolve(bpy.types.ViewLayer, "use", bpy.types.BoolProperty), Descriptor.resolve(bpy.types.ViewLayer, "name", bpy.types.StringProperty)),
            paired(Descriptor.resolve(bpy.types.ViewLayer, "material_override", bpy.types.PointerProperty), Descriptor.resolve(bpy.types.Scene, "view_layers", bpy.types.CollectionProperty)),
        )
        bound = paired(self.layer.bound(identity), flags())

        def config() -> BlenderResult[PyOpenColorIO.Config]:
            return admitted(PyOpenColorIO.GetCurrentConfig(), (PyOpenColorIO.ROLE_DATA,)).map_error(lambda faults: (BlenderImagingFault(faults=faults),))

        def built(
            parts: tuple[
                tuple[tuple[Descriptor[bpy.types.BoolProperty], Descriptor[bpy.types.StringProperty]], tuple[Descriptor[bpy.types.PointerProperty], Descriptor[bpy.types.CollectionProperty]]],
                tuple[Attached[Layer], dict[Flag, Descriptor[bpy.types.BoolProperty]]],
            ],
        ) -> LayerCommands:
            ((use, name), (override, layers)), (attached, states) = parts

            def moved(call: Call[Moving]) -> BlenderResult[Outcome]:
                def shifted(scene: bpy.types.Scene, layer: bpy.types.ViewLayer) -> BlenderResult[Outcome]:
                    index = scene.view_layers.find(layer.name)
                    match call.record.direction:
                        case Direction.UP:
                            target = index - 1
                        case Direction.DOWN:
                            target = index + 1
                    if not 0 <= target < len(scene.view_layers):
                        return Ok(Unchanged(reason=f"{layer.name} already ends the list"))
                    return (
                        catch(lambda: scene.view_layers.move(index, target), exception=RuntimeError)()
                        .map_error(lambda error: (BlenderEditFault(request=repr(scene.view_layers), error=error),))
                        .map(lambda _: Done())
                    )

                return paired(call.root(bpy.types.Scene), call.root(bpy.types.ViewLayer)).bind(lambda pair: shifted(*pair))

            def soloed(call: Call[Empty]) -> BlenderResult[Outcome]:
                return paired(call.root(bpy.types.Scene), call.root(bpy.types.ViewLayer)).bind(
                    lambda pair: accumulated(use.write(layer, layer == pair[1]) for layer in pair[0].view_layers).map(
                        lambda priors: Done() if any(prior.is_some() for prior in priors) else Unchanged(reason=f"{pair[1].name} already renders alone")
                    )
                )

            def scoped(call: Call[Renaming]) -> BlenderResult[tuple[bpy.types.Scene, ...]]:
                match call.record.within:
                    case Within.SCENE:
                        return Option.of_optional(call.context.scene).map(lambda scene: (scene,)).to_result_with(lambda: (BlenderAbsentFault(owner=repr(call.context), key="scene"),))
                    case Within.FILE:
                        return (
                            Option.of_optional(call.context.blend_data).map(lambda data: tuple(data.scenes)).to_result_with(lambda: (BlenderAbsentFault(owner=repr(call.context), key="blend_data"),))
                        )

            def overriding(setup: Clay | Utility) -> Callable[[Call[Empty]], BlenderResult[Done]]:
                return lambda call: paired(config(), call.root(bpy.types.ViewLayer)).bind(lambda pair: overridden(pair[1], setup, pair[0], call.identity, attached, override)).map(lambda _: Done())

            def copy(call: Call[Empty]) -> BlenderResult[Done]:
                layer: BlenderResult[bpy.types.ViewLayer] = call.root(bpy.types.ViewLayer)
                manager: BlenderResult[bpy.types.WindowManager] = call.root(bpy.types.WindowManager)
                return paired(Result[bpy.types.ViewLayer, BlenderFaults].bind(layer, Snapshot.capture), manager).map(lambda pair: copied(pair[1], pair[0])).map(lambda _: Done())

            def paste(owners: Callable[[Call[Empty]], BlenderResult[tuple[bpy.types.ViewLayer, ...]]]) -> Callable[[Call[Empty]], BlenderResult[Outcome]]:
                return lambda call: (
                    paired(owners(call), call.root(bpy.types.WindowManager))
                    .bind(lambda pair: pasted(pair[1], pair[0]))
                    .bind(lambda outcomes: answered(Applied.join(outcomes), "View layers already match the clipboard"))
                )

            def renaming(call: Call[Renaming]) -> BlenderResult[Outcome]:
                grammar: BlenderResult[OutputName] = OutputName.of(call.record.template).map_error(lambda faults: (BlenderRenderingFault(faults=faults),))
                return (
                    paired(grammar, scoped(call))
                    .bind(lambda pair: accumulated(renamed(scene, pair[0], name) for scene in pair[1]))
                    .map(lambda counts: Done(notes=(("INFO", f"Renamed {sum(counts)} view layers"),)) if sum(counts) else Unchanged(reason="View layers already hold their composed names"))
                )

            return LayerCommands(
                move=Operator(
                    key="view_layer_move",
                    title="Move View Layer",
                    description="Move the active view layer up or down the list",
                    form=Moving,
                    run=moved,
                    condition=Some(conditions.Count(collection=layers, least=2)),
                    undo="Move View Layer",
                ),
                actions=(
                    Operator(key="view_layer_solo", title="Solo View Layer", description="Render the active view layer alone among its scene's layers", form=Empty, run=soloed),
                    Operator(key="view_layer_rename", title="Rename View Layers", description="Name every view layer from its list position and base name", form=Renaming, run=renaming),
                    Operator(
                        key="view_layer_tint",
                        title="Tint View Layers",
                        description="Write each view layer's tag from a palette in list order",
                        form=Tinting,
                        run=lambda call: Result[bpy.types.Scene, BlenderFaults].bind(call.root(bpy.types.Scene), lambda scene: tinted(scene, call.record.palette, attached)),
                    ),
                    Operator(
                        key="view_layer_split",
                        title="View Layers from Collections",
                        description="Add a view layer per top-level collection no layer isolates",
                        form=Splitting,
                        run=lambda call: (
                            Result[bpy.types.Scene, BlenderFaults]
                            .bind(call.root(bpy.types.Scene), lambda scene: split(scene, Flag[call.record.others.name], states))
                            .map(lambda added: Done(notes=tuple(("INFO", f"Added {name}") for name in added)) if added else Unchanged(reason="A view layer isolates every top-level collection"))
                        ),
                    ),
                    Operator(key="view_layer_clay", title="Clay Override", description="Toggle the clay material override on the active view layer", form=Empty, run=overriding(Clay())),
                    Operator(key="view_layer_data", title="Data Layer", description="Toggle the active view layer between a beauty layer and a data layer", form=Empty, run=overriding(Utility())),
                    Operator(key="view_layer_copy", title="Copy View Layer", description="Copy the active view layer's settings", form=Empty, undo=False, run=copy),
                    Operator(
                        key="view_layer_paste",
                        title="Paste View Layer",
                        description="Paste copied settings into the active view layer",
                        form=Empty,
                        run=paste(
                            lambda call: (
                                Option.of_optional(call.context.view_layer).map(lambda layer: (layer,)).to_result_with(lambda: (BlenderAbsentFault(owner=repr(call.context), key="view_layer"),))
                            )
                        ),
                    ),
                    Operator(
                        key="view_layer_paste_all",
                        title="Paste to Used Layers",
                        description="Paste copied settings into every used view layer of the scene",
                        form=Empty,
                        run=paste(
                            lambda call: (
                                Result[bpy.types.Scene, BlenderFaults]
                                .bind(call.root(bpy.types.Scene), lambda scene: ViewLayers(scenes=(scene.name,)).owners(call.context))
                                .map(lambda found: tuple(layer for layer in found if bool(use.read(layer))))
                            )
                        ),
                    ),
                ),
            )

        return paired(members, bound).map(built)

    @property
    def page_rows(self) -> tuple[Attachment[Layer]]:
        """Layer attachment, whose stored records the preferences page holds."""
        return (self.layer,)

    def listed(self, commands: LayerCommands) -> ItemList[Layer, bpy.types.ViewLayer]:
        """Answers the layer list with its columns and move rows."""
        return ItemList(
            key="layers",
            item=bpy.types.ViewLayer,
            collection=Member(root=bpy.types.Scene, path="view_layers"),
            active=Member(root=bpy.types.Window, path="view_layer"),
            attachment=Some(self.layer),
            columns=(Property(target=Member(root=bpy.types.ViewLayer, path="use"), text=Some("")),),
            side=tuple(OperatorButton(target=Own(command=commands.move, values=Moving(direction=direction)), text=Some("")) for direction in Direction),
        )

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Realizes the list and the operator rows under it."""
        return (
            self
            .commands(identity)
            .bind(lambda commands: placed((self.listed(commands), Caption(rows=tuple(OperatorButton(target=placement) for placement in commands.actions), flow=Flow.GRID)), identity))
            .map(joined)
        )

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Answers the list class and every operator row, the page acquiring the attachment.

        Args:
            identity: Extension placing the row.

        Returns:
            `Ok` of the retained list and operator declarations; `Error` of every construction fault.
        """
        return self.commands(identity).map(lambda commands: (self.listed(commands), commands.move, *commands.actions))

    def section(self) -> Single[Empty]:
        """Section of the layer list and its rows."""
        return Single(key="layers", title="View Layers", rows=(self,))

    @staticmethod
    def overrides() -> Single[Empty]:
        """Section of the override grid across every view layer and the scene samples."""
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


def colors(palette: Palette, count: int) -> tuple[Rgb, ...]:
    """Answers a palette's scene-linear colors for a count of layers.

    Args:
        palette: Spectrum or published palette.
        count: Number of colors.

    Returns:
        The colors in list order.
    """
    match palette:
        case Spectrum(saturation=saturation, value=value):
            encoded = tuple(colorsys.hsv_to_rgb(index / count, saturation, value) for index in range(count))
        case Published(swatches=chosen):
            encoded = tuple(((swatch >> 16 & 0xFF) / 255, (swatch >> 8 & 0xFF) / 255, (swatch & 0xFF) / 255) for swatch in itertools.islice(itertools.cycle(chosen.swatches), count))
    return tuple(Rgb(red=linear.r, green=linear.g, blue=linear.b) for linear in (mathutils.Color(rgb).from_srgb_to_scene_linear() for rgb in encoded))


def encoded(color: Rgb) -> tuple[float, float, float]:
    """Answers a scene-linear color's display encoding, the form `Node.color` and a document raster hold.

    Args:
        color: Scene-linear color.

    Returns:
        The sRGB-encoded components through `mathutils.Color.from_scene_linear_to_srgb`.
    """
    shown = mathutils.Color((color.red, color.green, color.blue)).from_scene_linear_to_srgb()
    return (shown.r, shown.g, shown.b)


def tinted(scene: bpy.types.Scene, palette: Palette, attached: Attached[Layer]) -> BlenderResult[Outcome]:
    """Writes each layer's tag from a palette in list order where it differs.

    Args:
        scene: Scene whose layers take tags.
        palette: Palette the tags come from.
        attached: Bound layer attachment.

    Returns:
        `Ok` of `Done` beside each refused write, or `Unchanged` where every layer holds its tag; `Error` of each layer's `read` fault, or of every write's fault where none wrote.
    """
    layers = tuple(scene.view_layers)

    def wrote(held: tuple[Layer, ...]) -> BlenderResult[Outcome]:
        if not (differing := tuple((layer, record, color) for layer, record, color in zip(layers, held, colors(palette, len(layers)), strict=True) if record.color != color)):
            return Ok(Unchanged(reason=f"View layers of {scene.name} already hold the palette's tags"))
        return tallied(tuple(attached.write(layer, msgspec.structs.replace(record, color=color)) for layer, record, color in differing))

    return accumulated(map(attached.read, layers)).bind(wrote)


def renamed(scene: bpy.types.Scene, grammar: OutputName, name: Descriptor[bpy.types.StringProperty]) -> BlenderResult[int]:
    """Names each layer from its list position and base name.

    Args:
        scene: Scene whose layers rename.
        grammar: Name grammar holding `order` and `layer`.
        name: Descriptor of `ViewLayer.name`.

    Returns:
        `Ok` of the count of layers whose name changed; `Error` of `rendering` for a refused composition or `edit` for a refused write. Blender owns unique stored names.
    """

    def named(index: int, layer: bpy.types.ViewLayer) -> BlenderResult[bool]:
        base = grammar.parse(layer.name).to_option().bind(lambda fields: Option.of_optional(fields.words.get(Word.LAYER))).default_value(layer.name)
        wanted: BlenderResult[str] = grammar.compose(Fields(words={Word.LAYER: base}, counts={Count.ORDER: index + 1})).map_error(lambda faults: (BlenderRenderingFault(faults=faults),))
        return Result[str, BlenderFaults].bind(wanted, lambda value: name.write(layer, value)).map(Option.is_some)

    return accumulated(itertools.starmap(named, enumerate(scene.view_layers))).map(sum)


def split(scene: bpy.types.Scene, flag: Flag, flags: dict[Flag, Descriptor[bpy.types.BoolProperty]]) -> BlenderResult[tuple[str, ...]]:
    """Adds one layer per top-level collection no layer isolates under a flag.

    Args:
        scene: Scene gaining layers.
        flag: Flag every other top-level collection takes.
        flags: Descriptor of each layer collection flag.

    Returns:
        `Ok` of the added layer names, empty where a layer isolates every top-level collection; `Error` of each state write's faults, every added layer removed.
    """
    lone = tuple(collection for collection in scene.collection.children if not any(isolates(layer, collection, flag, flags) for layer in scene.view_layers))
    with ExitStack() as rollback:

        def made(collection: bpy.types.Collection) -> BlenderResult[str]:
            layer = scene.view_layers.new(collection.name)
            rollback.callback(scene.view_layers.remove, layer)
            return applied(CollectionState(layer=layer.name, collection=IdName.of(collection), flag=flag, value=False, extent=Extent.ISOLATE), scene, flags).map(lambda _: layer.name)

        added = accumulated(map(made, lone))
        for _ in added.to_option().to_list():
            rollback.pop_all()
        return added


def overridden(
    layer: bpy.types.ViewLayer, setup: Clay | Utility, config: PyOpenColorIO.Config, identity: Identity, attached: Attached[Layer], override: Descriptor[bpy.types.PointerProperty]
) -> BlenderResult[None]:
    """Toggles a layer's material override to a setup's owned material, a data setup writing the layer's role beside it.

    Args:
        layer: View layer toggled.
        setup: Clay or data setup.
        config: Config admitted with the `data` role.
        identity: Extension owning the material.
        attached: Bound layer attachment.
        override: Descriptor of `ViewLayer.material_override`.

    Returns:
        `Ok` once written; `Error` of the material's faults, `read`, `edit`, or `choice`.
    """

    def toggled(held: bpy.types.Material) -> BlenderResult[None]:
        on = not override.equal(override.read(layer), IdName.of(held))
        wrote = override.write(layer, held if on else None)
        match setup:
            case Utility():
                role = attached.read(layer).bind(lambda record: attached.write(layer, msgspec.structs.replace(record, role=Role.DATA if on else Role.BEAUTY)))
                return paired(wrote, role).map(lambda _: None)
            case Clay():
                return wrote.map(lambda _: None)

    return material(setup, config, identity).bind(toggled)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Direction",
    "Layer",
    "LayerCommands",
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
    "overridden",
    "renamed",
    "split",
    "tinted",
]
