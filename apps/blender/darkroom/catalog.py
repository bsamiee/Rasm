"""Darkroom effect catalog: every effect in processing order over the optics, grade, formation, and finish phases between the pinned temporal and pass entries, with the staged render, preview tiles, layer stacks and their looks, strip placement, node previews, and the active entry editor."""

from types import MappingProxyType
from typing import Final

from bl_ui.properties_compositor import SCENE_PT_compositor_effects
from bl_ui.space_node import NODE_PT_backdrop
from expression import Some

from rasm_bpy.nodes import Socket
from rasm_bpy.render import (
    Adjustment,
    CompositorRecord,
    EffectTiles,
    Enabled,
    Entry,
    ENTRY_LOCKS,
    EntryEditor,
    EntryLocks,
    EntryTile,
    LayerStack,
    LayerStackExtension,
    NodePreview,
    OfferedEffects,
    path_key,
    Phase,
    PINNED,
    Pinned,
    PreviewScene,
    RoutingRow,
    snapshot,
    StackEnd,
    StackLayer,
    StagedRender,
    Strips,
    Valued,
    WHOLE,
)
from rasm_bpy.ui import Beside, Panel

from .effects.finish.damage import damage
from .effects.finish.dither import dither
from .effects.finish.edges import edges
from .effects.finish.emulation import emulation
from .effects.finish.patterns import palette, pattern, pixelation
from .effects.formation.display import formation, formed_view, peak_mapping
from .effects.formation.tone import hable, neutral, photoreceptor, tone_curve, tony
from .effects.grade.colorimetry import gamut_compression, primaries, white_balance
from .effects.grade.curves import curves, hue_curves
from .effects.grade.exposure import exposure, flicker, levels, local_exposure, STOPS
from .effects.grade.grain import grain
from .effects.grade.luts import cdl, lookup
from .effects.grade.primary import primary, saturation, SLOPE, zones
from .effects.optics.detail import filtering, sharpen, spatial_denoise
from .effects.optics.displacement import displacement
from .effects.optics.flares import lens_flare, light_rays
from .effects.optics.glare import bloom, ghosts, glare, halation
from .effects.optics.lens import aberration, defocus, distortion, vignette
from .effects.optics.media import atmosphere, noise_field
from .effects.optics.particles import particles
from .effects.optics.shapes import shapes
from .effects.optics.warps import projection, transform
from .rendering import temporal

# --- [OPERATIONS] -----------------------------------------------------------------------


def _look(name: str, *settings: tuple[CompositorRecord, Socket, float]) -> Valued[LayerStack]:
    """Look stack of one adjustment layer through each effect record at its socket's value."""
    layer = StackLayer(key="look", name=name, source=Adjustment(), effects=tuple(Entry(record=record) for record, _, _ in settings))
    return Valued(structure=LayerStack(layers=(layer,)), values=snapshot({path_key(place, socket): value for place, (_, socket, value) in zip(layer.places, settings)}))


# --- [COMPOSITION] ----------------------------------------------------------------------

tones: Final = (tone_curve, hable, tony, neutral, photoreceptor)
chain: Final = OfferedEffects(
    phases=(
        Phase(
            key="optics",
            members=(
                spatial_denoise,
                atmosphere,
                defocus,
                noise_field,
                particles,
                shapes,
                light_rays,
                lens_flare,
                displacement,
                transform,
                projection,
                distortion,
                aberration,
                sharpen,
                bloom,
                glare,
                ghosts,
                vignette,
                halation,
            ),
        ),
        Phase(key="grade", members=(exposure, flicker, grain, white_balance, primaries, gamut_compression, levels, zones, (cdl,), primary, saturation, curves, hue_curves, lookup, local_exposure)),
        Phase(key="formation", members=(tones, (formation,), (peak_mapping,))),
        Phase(key="finish", members=(pixelation, palette, pattern, edges, damage, emulation, (dither,))),
    ),
    pinned=(
        PINNED,
        Pinned(ownership=temporal.OWNERSHIP, edge=StackEnd.HEAD, enables=frozenset({Enabled.RENDER})),
        Pinned(ownership=RoutingRow.ownership, edge=StackEnd.TAIL, enables=frozenset({Enabled.RENDER})),
    ),
)
render: Final = StagedRender(rows=chain, stacks=Some(lambda identity: stacks.bound(identity)))  # ruff: ignore[unnecessary-lambda]
tiles: Final = EffectTiles(rows=chain, closing=Some(formation), preview=PreviewScene(render=render))
stacks: Final[LayerStackExtension] = LayerStackExtension(
    rows=chain,
    before=Some(formation),
    looks=MappingProxyType({
        name: _look(name, (exposure.record(), STOPS, stops), (primary.record(), SLOPE, slope))
        for name, stops, slope in (("Rhino Filmic, Low Contrast", 4.0, 0.5), ("Rhino Filmic, Medium Contrast", 1.0, 1.0), ("Rhino Filmic, High Contrast", -1.0, 2.0))
    }),
    filtering=Some(filtering),
    view=formed_view,
    tile=Some(tiles.icon),
)
nodes: Final = NodePreview()
rows: Final = (
    chain,
    render,
    tiles,
    stacks,
    Strips(rows=chain),
    nodes,
    EntryEditor(rows=chain, header=(ENTRY_LOCKS.row(WHOLE),), leading=(EntryTile(tiles=tiles),), trailing=(EntryLocks(),)),
    Panel(section=stacks.section(), at=Beside(SCENE_PT_compositor_effects)),
    Panel(section=nodes.section(), at=Beside(NODE_PT_backdrop)),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["chain", "render", "rows", "stacks", "tiles", "tones"]
