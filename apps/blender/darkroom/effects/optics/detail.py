"""Render noise and fine detail: firefly suppression and guided Open Image Denoise over the frame, light passes, or light groups, and Laplace sharpening."""

from collections.abc import Callable
import functools
from typing import Final

import bpy
from expression import Nothing, Option, Some
from expression.collections import Block

from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.identity import Key
from blender.bpy.nodes import functions
from blender.bpy.nodes.desired import item_key, MenuItem, MenuItems
from blender.bpy.nodes.graphs import Expr, group_input, math, menu, mixed, Scalar, switched, vector, where
from blender.bpy.nodes.interfaces import Interface, InterfacePanel, NodeGroup, Socket, StockInput
from blender.bpy.render.compositor import filters, masks
from blender.bpy.render.compositor.colors import COLOR_INPUT, COLOR_RESULT
from blender.bpy.render.compositor.effects import Blended, CompositorEffect
from blender.bpy.render.compositor.inputs import GraphInputs, Holds, path_key, ResultOverBase
from blender.bpy.render.compositor.layers import TermFiltering
from blender.bpy.render.compositor.masks import BLEND, LUMINANCE, luminance_group, OPACITY
from blender.bpy.render.compositor.recombination import LightGroups, Lobes, Recombined, TermFilter, TERMS
from blender.bpy.render.view_layers.passes import DENOISING_ALBEDO, DENOISING_NORMAL, Family, PassFlags, PassRequest
from blender.records.faults import Outcome
from blender.records.fields import Case

from ..grade.exposure import LATER, later

# --- [CONSTANTS] ------------------------------------------------------------------------

# --- [SPATIAL]

_NOISY: Final = Socket(key="noisy", kind=bpy.types.NodeSocketColor)
_ALBEDO: Final = Socket(key="albedo", kind=bpy.types.NodeSocketColor)
_NORMAL: Final = Socket(key="normal", kind=bpy.types.NodeSocketVector)
_RING: Final = tuple((float(dx), float(dy)) for dy in range(-2, 3) for dx in range(-2, 3) if max(abs(dx), abs(dy)) == 2)
_UNGUIDED: Final = MenuItem(name="None")
_ALBEDO_GUIDED: Final = MenuItem(name="Albedo")
_FULLY_GUIDED: Final = MenuItem(name="Albedo and Normal")
_GUIDES: Final = Socket(
    key="guides", kind=bpy.types.NodeSocketMenu, description="Passes guiding filter", default_value=_FULLY_GUIDED.name, items=Some(MenuItems(items=(_UNGUIDED, _ALBEDO_GUIDED, _FULLY_GUIDED)))
)
_PREFILTER: Final = Socket(key="prefilter", kind=StockInput(node=bpy.types.CompositorNodeDenoise, input="Prefilter"))
_QUALITY: Final = Socket(key="quality", kind=StockInput(node=bpy.types.CompositorNodeDenoise, input="Quality"))
_FILTER: Final = Interface(inputs=(_GUIDES, _PREFILTER, _QUALITY))
_THRESHOLD: Final = Socket(key="threshold", kind=bpy.types.NodeSocketFloat, description="Luminance over fourth brightest ring neighbor", min_value=1.0, max_value=10.0, default_value=2.0)
_FIREFLIES: Final = InterfacePanel.of("fireflies", Interface(inputs=(_THRESHOLD,)), description="Firefly suppression before filtering", toggle=Some(True))
_FAMILIES: Final = frozenset(key for key, held, _ in TERMS if isinstance(held, Family))
_FRAME: Final = InterfacePanel.of("frame", _FILTER, description="Incoming frame denoised as one image")
_LIGHT_PASSES: Final = InterfacePanel.of(
    "light_passes",
    Interface(
        panels=tuple(
            InterfacePanel.of(term.key, _FILTER, description=f"Filter over {term.title.lower()} light", toggle=Some(term.key in _FAMILIES))
            for term in Recombined(layer="", recombination=Lobes()).interface.panels
        )
    ),
    description="Light terms of a recombined layer filtered before they sum, one filter per recombined term",
)
_LIGHT_GROUPS: Final = InterfacePanel.of("light_groups", _FILTER, description="Light-group passes of a recombined layer filtered before they sum")
_TARGETS: Final = Interface(panels=(_FRAME, _LIGHT_PASSES, _LIGHT_GROUPS))
_TARGET: Final = Socket(key="target", kind=bpy.types.NodeSocketMenu, description="Frame or recombined light terms to denoise", default_value=_FRAME.title, items=Some(_TARGETS.menu))

# --- [SHARPEN]

_SHARPNESS: Final = Socket(key="sharpness", kind=bpy.types.NodeSocketFloat, description="Gain of each pixel's difference from the mean of its eight neighbors", min_value=0.0, max_value=10.0)

# --- [MODELS] ---------------------------------------------------------------------------

# --- [SPATIAL]


class SpatialDenoise(Case, frozen=True, kw_only=True):
    """Guided spatial denoising after optional firefly suppression."""


# --- [SHARPEN]


class Sharpen(Case, frozen=True, kw_only=True):
    """Laplace sharpening attenuated by exposed luminance steps."""


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [SPATIAL]


def _denoiser(luminance: NodeGroup) -> Outcome[NodeGroup]:
    """Builds the shared firefly and guided denoiser group.

    Fireflies scale toward the fourth brightest valid luminance on the 5x5 ring.

    Args:
        luminance: Working luminance group with image input and luminance output.

    Returns:
        Group with filtered and firefly-cleaned outputs, or graph faults.
    """
    image = group_input(COLOR_INPUT.key, bpy.types.NodeSocketColor)
    level = functions.placed(luminance, {COLOR_INPUT.key: image}, LUMINANCE.key)
    rank, lit = 4, 0.0001
    fourth, valid = filters.ranked(level, _RING, rank)
    marked = (level > fourth * group_input(path_key(_FIREFLIES, _THRESHOLD))) * (1.0 - (level < lit)) * (valid > float(rank))
    cleaned = switched(group_input(_FIREFLIES.key), image, mixed(1.0, image, where(marked, fourth / level, 1.0), blend="MULTIPLY"))
    guide = group_input(_ALBEDO.key, bpy.types.NodeSocketColor)
    albedo = menu(group_input(_GUIDES.key), {_UNGUIDED: (0.8, 0.8, 0.8, 1.0), _ALBEDO_GUIDED: guide, _FULLY_GUIDED: guide})
    normal = switched(albedo.at(_FULLY_GUIDED.name), (0.0, 0.0, 0.0), group_input(_NORMAL.key, bpy.types.NodeSocketVector))
    filtered = Expr(
        kind=bpy.types.CompositorNodeDenoise,
        inputs=(("Image", cleaned), ("Albedo", albedo), ("Normal", normal), ("Prefilter", group_input(_PREFILTER.key)), ("Quality", group_input(_QUALITY.key))),
        socket="Image",
        socket_type=bpy.types.NodeSocketColor,
    )
    interface = Interface(outputs=(COLOR_RESULT, _NOISY), inputs=(COLOR_INPUT, _ALBEDO, _NORMAL, *_FILTER.inputs), panels=(_FIREFLIES,))
    return functions.built("denoiser", interface, {COLOR_RESULT.key: filtered, _NOISY.key: cleaned}, bpy.types.CompositorNodeTree)


def _guides(render: Expr, family: Option[Family]) -> tuple[Expr, Expr]:
    """Cycles denoising albedo and normal of a render layer, a family's color pass as the albedo of its term."""
    albedo = family.bind(lambda held: Block.of_seq(sorted(flag.prop.name for flag in PassFlags.of().families[held][2])).try_head()).default_value(DENOISING_ALBEDO)
    return render.at(albedo, bpy.types.NodeSocketColor), render.at(DENOISING_NORMAL, bpy.types.NodeSocketVector)


def _placed(group: NodeGroup, read: Callable[[Key], Scalar], filter_at: Key, source: Scalar, guides: tuple[Expr, Expr]) -> Expr:
    """Places the denoiser group over a source, its filter controls read under `filter_at` and the firefly controls at the record root."""
    albedo, normal = guides
    controls = {socket.key: read(item_key(filter_at, socket.key)) for socket in _FILTER.inputs} | {key: read(key) for key, _, _, _ in Interface(panels=(_FIREFLIES,)).walked()}
    return functions.placed(group, {COLOR_INPUT.key: source, _ALBEDO.key: albedo, _NORMAL.key: normal} | controls, COLOR_RESULT.key)


def _denoised(shell: GraphInputs[SpatialDenoise]) -> Outcome[ResultOverBase]:
    """Filters the frame under its target over the noisy frame as base, the light-term targets passing the stage input to the layer that filters its terms."""

    def built(group: NodeGroup) -> ResultOverBase:
        frame = _placed(group, group_input, _FRAME.key, shell.color, _guides(shell.render, Nothing)).keyed("target_frame")
        chosen = shell.menu(_TARGET, cases={_FRAME.item: frame, _LIGHT_PASSES.item: shell.color, _LIGHT_GROUPS.item: shell.color}).keyed("target")
        return ResultOverBase(result=chosen, base=switched(chosen.at(_FRAME.title), shell.color, frame.at(_NOISY.key, bpy.types.NodeSocketColor)).keyed("frame_base"))

    return luminance_group(shell.config).bind(_denoiser).map(built)


# --- [LAYERS]


def _term(source: Recombined, read: Callable[[Key], Expr], chosen: Expr, group: NodeGroup, key: Key, held: Expr, family: Option[Family], render: Expr) -> Expr:
    """Filters one recombined term through the denoiser group while the effect's target selects its light passes or light groups."""
    match source.recombination:
        case Lobes():
            at, active = path_key(_LIGHT_PASSES, key), chosen.at(_LIGHT_PASSES.title) * read(path_key(_LIGHT_PASSES, key))
        case LightGroups():
            at, active = _LIGHT_GROUPS.key, chosen.at(_LIGHT_GROUPS.title)
    cleaned = _placed(group, read, at, held, _guides(render, family))
    return switched(active, held, mixed(read(item_key(BLEND, OPACITY.key)), cleaned.at(_NOISY.key, bpy.types.NodeSocketColor), cleaned))


def _term_filter(_record: Case, source: Recombined, read: Callable[[Key], Expr], luminance: NodeGroup) -> Outcome[TermFilter]:
    """Filters a recombined layer's terms through its denoise effect while the effect targets light passes or light groups."""
    targets = menu(read(_TARGET.key), dict.fromkeys(_TARGETS.menu.items, 0.0))
    return _denoiser(luminance).map(lambda group: functools.partial(_term, source, read, targets, group))


# --- [SHARPEN]


def _sharpened(shell: GraphInputs[Sharpen]) -> Outcome[Expr]:
    """Adds the Laplace response scaled by sharpness, gated by the Sobel step of luminance under the later exposure gain."""
    source = shell.color

    def built(level: Expr) -> Expr:
        gate = math("SUBTRACT", 1.0, shell.input(LATER) * filters.filtered(level, "Sobel")["Red"] * 0.25, clamp=True)
        return vector("MULTIPLY_ADD", filters.filtered(source, "Laplace"), shell.input(_SHARPNESS) * gate, source)

    return masks.luminance(shell.config, source).map(built)


# --- [COMPOSITION] ----------------------------------------------------------------------

spatial_denoise: Final = CompositorEffect(
    _denoised,
    record=SpatialDenoise,
    interface=Interface(inputs=(_TARGET,), panels=(_FIREFLIES, *_TARGETS.panels)),
    mix=Blended(),
    request=Some(lambda _record, layer: PassRequest.denoising(layer)),
    holds=Holds(scene=Snapshot.of(bpy.types.Scene, {("cycles", "use_denoising"): False})),
)
filtering: Final = TermFiltering[Case](record=SpatialDenoise, filter=_term_filter)
sharpen: Final = CompositorEffect(_sharpened, record=Sharpen, interface=Interface(inputs=(_SHARPNESS, LATER)), animates=Some(later))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Sharpen", "SpatialDenoise", "filtering", "sharpen", "spatial_denoise"]
