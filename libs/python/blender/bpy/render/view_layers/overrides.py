"""View layer list, commands, and per-layer overrides."""

import colorsys
from contextlib import ExitStack
from enum import auto, Enum
from functools import partial
import itertools
from types import MappingProxyType
from typing import Annotated, ClassVar, Final

import bpy
from expression import Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import mathutils
import msgspec
import numpy as np
import PyOpenColorIO

from blender.bpy.commands.operators import Answer, Call, Done, Empty, Operator, Unchanged
from blender.bpy.data.ids.store import IdName
from blender.bpy.data.rna.descriptors import Descriptor, written
from blender.bpy.data.rna.mirrors import case_choices, choices
from blender.bpy.data.rna.paths import rooted
from blender.bpy.data.rna.scopes import Scenes, ViewLayers
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Registrable
from blender.bpy.faults import BlenderReason
from blender.bpy.render.view_layers.collections import applied, copying, isolates, pasting, RESTRICTIONS, Spread
from blender.bpy.render.view_layers.passes import soloed, used
from blender.bpy.scene.materials import Clay, material, Utility
from blender.bpy.settings.attachments import Attached, Attachment
from blender.bpy.ui.layout.lists import ItemList
from blender.bpy.ui.layout.members import Member
from blender.bpy.ui.layout.rows import Layout, Line, OperatorButton, Property, realized, RowDraw
from blender.bpy.ui.layout.sections import Section
from blender.bpy.ui.layout.tables import Matrix
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired
from blender.records.fields import Case
from blender.rendering.outputs.naming import Fields, NumberField, OutputName, Word

# --- [TYPES] ----------------------------------------------------------------------------

type Palette = Spectrum | Published


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


# --- [CONSTANTS] ------------------------------------------------------------------------

_NAME: Final = Descriptor.declared(bpy.types.ViewLayer, ("name",), bpy.types.StringProperty)
_OVERRIDE: Final = Descriptor.declared(bpy.types.ViewLayer, ("material_override",), bpy.types.PointerProperty)
_ITEMS: Final = MappingProxyType({kind: choices((member.name, bpy.path.display_name(member.name.lower()), "") for member in kind) for kind in (Role, Within)})

# --- [MODELS] ---------------------------------------------------------------------------


class Spectrum(Case, frozen=True, kw_only=True):
    """Hues spaced evenly on color wheel."""

    saturation: Annotated[float, msgspec.Meta(ge=0.0, le=1.0)] = 0.7
    value: Annotated[float, msgspec.Meta(ge=0.0, le=1.0)] = 0.36


class Published(Case, frozen=True, kw_only=True):
    """Color-blind safe palette cycled over layers."""

    swatches: Swatch = Swatch.IBM


class LayerTag(bpy.types.PropertyGroup):
    """Purpose and tag of a view layer."""

    role: bpy.props.EnumProperty(name="Role", description="Beauty layers form image, data layers hold data passes and mattes", items=_ITEMS[Role])  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    color: bpy.props.FloatVectorProperty(  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
        name="Tag", description="Layer color in lists and on its nodes", subtype="COLOR", min=0.0, max=1.0, default=mathutils.Color((Spectrum().value,) * 3).from_srgb_to_scene_linear()[:]
    )


class Renaming(bpy.types.PropertyGroup):
    """Rename of every layer by its list position."""

    template: bpy.props.StringProperty(name="Names", description="Layer name grammar holding {order:<width>} and {layer}", default="{order:2}_{layer}")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    within: bpy.props.EnumProperty(name="Scenes", description="Scenes rename reaches", items=_ITEMS[Within])  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Tinting(bpy.types.PropertyGroup):
    """Tag colors written from a palette."""

    palette: bpy.props.EnumProperty(name="Palette", description="Palette tags take in list order", items=case_choices(Palette))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    saturation: bpy.props.FloatProperty(name="Saturation", description="Saturation of every spectrum hue", subtype="FACTOR", min=0.0, max=1.0, default=Spectrum().saturation)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    value: bpy.props.FloatProperty(name="Value", description="Brightness of every spectrum hue", subtype="FACTOR", min=0.0, max=1.0, default=Spectrum().value)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    swatches: bpy.props.EnumProperty(name="Swatches", description="Published palette the layers cycle through", items=choices((swatch.name, swatch.name, "") for swatch in Swatch))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Splitting(bpy.types.PropertyGroup):
    """Split of top-level collections into layers."""

    others: bpy.props.EnumProperty(  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
        name="Others",
        description="Restriction other top-level collections take in a new layer",
        items=choices((member.prop.identifier, member.prop.name, member.prop.description) for member in RESTRICTIONS.values()),
        default=RESTRICTIONS["indirect_only"].prop.identifier,
    )


class LayerOverride(Line, frozen=True, kw_only=True):
    """Row of layer list, reordered by dragging, over layer commands."""

    layer: ClassVar[Attachment[LayerTag]] = Attachment(key="layer", form=LayerTag, owner=bpy.types.ViewLayer)
    prerequisites: ClassVar[tuple[Registrable, ...]] = (layer,)

    def listed(self) -> ItemList[LayerTag, bpy.types.ViewLayer]:
        """Returns layer list with use column, its grip reordering scene layers."""
        return ItemList(
            key="layers",
            item=bpy.types.ViewLayer,
            collection=Member(root=bpy.types.Scene, path=("view_layers",)),
            active=Some(Member(root=bpy.types.Window, path=("view_layer",))),
            attachment=Some(self.layer),
            cells=(Property(target=Member(root=bpy.types.ViewLayer, path=("use",)), keywords={"text": ""}),),
            move=Some(moved),
        )

    def drawn(self, identity: Identity) -> Outcome[RowDraw]:
        """Draws layer list over command grid."""
        commands = Layout(rows=tuple(OperatorButton(target=action) for action in ACTIONS), container=Some(lambda layout: layout.grid_flow(row_major=True, even_columns=True)))
        return realized((self.listed(), lambda layout, _holder, _context: layout.separator(type="LINE"), commands), identity)

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Returns layer list and commands."""
        return Ok((self.listed(), *ACTIONS))

    def section(self) -> Section[Empty]:
        """Returns view layer list section."""
        return Section(key="layers", title="View Layers", rows=(self,))

    @staticmethod
    def overrides() -> Section[Empty]:
        """Returns override grid over view layers and scene samples."""
        filters = tuple(
            (prop.identifier,)
            for prop in bpy.types.ViewLayer.bl_rna.properties
            if isinstance(prop, bpy.types.BoolProperty) and not prop.is_readonly and prop.identifier.startswith("use_") and not prop.identifier.startswith("use_pass_")
        )
        return Section(
            key="layer_overrides",
            title="Layer Overrides",
            rows=(
                Matrix(scope=ViewLayers(), settings=(("use",), ("material_override",), ("world_override",), ("samples",), *filters, ("cycles", "use_denoising"))),
                Matrix(scope=Scenes(), settings=(("cycles", "samples"),)),
            ),
        )


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [LAYERS]


def moved(context: bpy.types.Context, current: int, row: int) -> Outcome[None]:
    """Moves the context scene's view layer at `current` to `row`."""
    return rooted(bpy.types.Scene, context).bind(
        lambda scene: catch(scene.view_layers.move, exception=RuntimeError)(current, row).map(lambda _: None).map_error(partial(Fault.of, BlenderReason.REFUSED, repr(scene.view_layers)))
    )


def colors(palette: Palette, count: int) -> tuple[tuple[float, float, float], ...]:
    """Returns scene-linear palette colors in list order."""
    match palette:
        case Spectrum(saturation=saturation, value=value):
            encoded = tuple(colorsys.hsv_to_rgb(index / count, saturation, value) for index in range(count))
        case Published(swatches=chosen):
            encoded = tuple(map(tuple, np.resize(np.frombuffer(bytes.fromhex(chosen.value), np.uint8).reshape(-1, 3), (count, 3)) / 255))
    return tuple((red, green, blue) for red, green, blue in (mathutils.Color(rgb).from_srgb_to_scene_linear()[:] for rgb in encoded))


def encoded(color: tuple[float, float, float]) -> tuple[float, float, float]:
    """Returns sRGB form of a scene-linear color, as `Node.color` and document rasters hold."""
    red, green, blue = mathutils.Color(color).from_scene_linear_to_srgb()[:]
    return red, green, blue


def tinted(scene: bpy.types.Scene, palette: Palette, attached: Attached[LayerTag]) -> Outcome[Answer]:
    """Writes palette tags onto layers in list order where they differ."""
    layers = tuple(scene.view_layers)
    return accumulated(written(attached.struct(layer), ("color",), color) for layer, color in zip(layers, colors(palette, len(layers)))).map(
        lambda priors: Done() if any(prior.is_some() for prior in priors) else Unchanged(reason=f"View layers of {scene.name} hold palette tags")
    )


def renamed(scene: bpy.types.Scene, grammar: OutputName) -> Outcome[int]:
    """Names each layer from list position and base name, returning renamed count."""

    def named(index: int, layer: bpy.types.ViewLayer) -> Outcome[bool]:
        base = grammar.parse(layer.name).to_option().bind(lambda fields: Option.of_optional(fields.words.get(Word.LAYER))).default_value(layer.name)
        wanted: Outcome[str] = grammar.compose(Fields(words={Word.LAYER: base}, counts={NumberField.ORDER: index + 1}))
        return Result[str, Faults].bind(wanted, lambda value: _NAME.write(layer, value)).map(Option.is_some)

    return accumulated(itertools.starmap(named, enumerate(scene.view_layers))).map(sum)


def split(scene: bpy.types.Scene, flag: Descriptor[bpy.types.BoolProperty]) -> Outcome[tuple[str, ...]]:
    """Adds one layer per top-level collection no layer isolates, removing every added layer on fault."""
    lone = tuple(collection for collection in scene.collection.children if not any(isolates(layer, collection, flag) for layer in scene.view_layers))
    with ExitStack() as rollback:

        def made(collection: bpy.types.Collection) -> Outcome[str]:
            layer = scene.view_layers.new(collection.name)
            rollback.callback(scene.view_layers.remove, layer)
            return applied(scene, layer=layer.name, collection=collection.session_uid, flag=flag, value=False, extent=Spread.ISOLATE).map(lambda _: layer.name)

        if (added := (accumulated(map(made, lone)))).is_ok():
            rollback.pop_all()
        return added


def overridden(layer: bpy.types.ViewLayer, setup: Clay | Utility, config: PyOpenColorIO.Config, identity: Identity) -> Outcome[None]:
    """Toggles layer material override to owned setup material, data setups writing layer role with it."""

    def toggled(held: bpy.types.Material) -> Outcome[None]:
        on = not _OVERRIDE.equal(_OVERRIDE.read(layer), IdName.of(held))
        wrote = _OVERRIDE.write(layer, held if on else None)
        match setup:
            case Utility():
                role = LayerOverride.layer.read(identity, layer).bind(lambda group: written(group, ("role",), (Role.DATA if on else Role.BEAUTY).name))
                return paired(wrote, role).map(lambda _: None)
            case Clay():
                return wrote.map(lambda _: None)

    return material(setup, config, identity).bind(toggled)


# --- [COMMANDS]


def view_layer_solo(call: Call[Empty]) -> Outcome[Answer]:
    """Render active view layer alone."""
    return paired(rooted(bpy.types.Scene, call.context), rooted(bpy.types.ViewLayer, call.context)).bind(
        lambda pair: soloed(pair[0], pair[1].name).map(lambda changed: Done() if changed else Unchanged(reason=f"{pair[1].name} renders alone"))
    )


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


def _overriding(setup: Clay | Utility, call: Call[Empty]) -> Outcome[Done]:
    """Toggles the active layer's override to the setup material."""
    return rooted(bpy.types.ViewLayer, call.context).bind(lambda layer: overridden(layer, setup, PyOpenColorIO.GetCurrentConfig(), call.identity)).map(lambda _: Done())


def view_layer_tint(call: Call[Tinting]) -> Outcome[Answer]:
    """Set view layer tags from palette in list order."""
    held = call.properties
    palette = msgspec.convert({"kind": held.palette, "saturation": held.saturation, "value": held.value, "swatches": Swatch[held.swatches].value}, Palette)
    return paired(rooted(bpy.types.Scene, call.context), LayerOverride.layer.bound(call.identity)).bind(lambda pair: tinted(pair[0], palette, pair[1]))


def view_layer_split(call: Call[Splitting]) -> Outcome[Answer]:
    """Add a view layer per top-level collection no layer isolates."""
    return (
        Result[bpy.types.Scene, Faults]
        .bind(rooted(bpy.types.Scene, call.context), lambda scene: split(scene, RESTRICTIONS[call.properties.others]))
        .map(lambda added: Done(notes=tuple(("INFO", f"Added {name}") for name in added)) if added else Unchanged(reason="Each top-level collection has a layer isolating it"))
    )


def view_layer_clay(call: Call[Empty]) -> Outcome[Done]:
    """Toggle clay material override on active layer."""
    return _overriding(Clay(), call)


def view_layer_data(call: Call[Empty]) -> Outcome[Done]:
    """Toggle data material override and role on active layer."""
    return _overriding(Utility(), call)


def view_layer_copy(call: Call[Empty]) -> Outcome[Done]:
    """Copy active view layer settings."""
    return copying(lambda layer: layer, call)


def view_layer_paste(call: Call[Empty]) -> Outcome[Answer]:
    """Paste settings into active view layer."""
    return pasting(lambda held: rooted(bpy.types.ViewLayer, held.context).map(lambda layer: (layer,)), "View layers match clipboard", call)


def view_layer_paste_all(call: Call[Empty]) -> Outcome[Answer]:
    """Paste settings into every used view layer."""
    return pasting(lambda held: rooted(bpy.types.Scene, held.context).map(used), "View layers match clipboard", call)


# --- [COMPOSITION] ----------------------------------------------------------------------

ACTIONS: Final[tuple[Operator[Empty] | Operator[Renaming] | Operator[Tinting] | Operator[Splitting], ...]] = (
    Operator(form=Empty, run=view_layer_solo),
    Operator(form=Renaming, run=view_layer_rename),
    Operator(form=Tinting, run=view_layer_tint),
    Operator(form=Splitting, run=view_layer_split),
    Operator(form=Empty, run=view_layer_clay),
    Operator(form=Empty, run=view_layer_data),
    Operator(form=Empty, undo=Nothing, run=view_layer_copy),
    Operator(form=Empty, run=view_layer_paste),
    Operator(form=Empty, run=view_layer_paste_all),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "ACTIONS",
    "LayerOverride",
    "LayerTag",
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
    "renamed",
    "split",
    "tinted",
    "view_layer_clay",
    "view_layer_copy",
    "view_layer_data",
    "view_layer_paste",
    "view_layer_paste_all",
    "view_layer_rename",
    "view_layer_solo",
    "view_layer_split",
    "view_layer_tint",
]
