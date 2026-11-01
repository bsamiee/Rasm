"""Darkroom effect catalog: every effect in processing order over the optics, grade, formation, and finish phases between the pinned temporal and pass entries, with the staged render, preview tiles, layer stacks and their looks, strip placement, node previews, and the active entry editor."""

from functools import partial
from types import MappingProxyType
from typing import Any, Final

from bl_ui.properties_compositor import SCENE_PT_compositor_effects
from bl_ui.space_node import NODE_PT_backdrop
import bpy
from expression import Ok, Option, Some

from blender.bpy.extension.identity import Identity, Key
from blender.bpy.nodes.interfaces import Socket
from blender.bpy.render.compositor.editors import EntryEditor
from blender.bpy.render.compositor.effects import Entry
from blender.bpy.render.compositor.entries import EffectRows, Exclusive, Phase
from blender.bpy.render.compositor.inputs import path_key, snapshot
from blender.bpy.render.compositor.layers import Adjustment, LayerStack, PINNED, StackLayer
from blender.bpy.render.compositor.placement import Edge, Enabled, Pinned
from blender.bpy.render.compositor.recipes import Valued
from blender.bpy.render.compositor.stacks import LayerStacks
from blender.bpy.render.compositor.strips import Strips
from blender.bpy.render.previews.nodes import NodePreview
from blender.bpy.render.previews.tiles import EffectTiles, EntryTile
from blender.bpy.render.sessions.setups import ENTRY_LOCKS, EntryLocks, WHOLE
from blender.bpy.render.sessions.staging import StagedRender
from blender.bpy.render.view_layers.routing import PassSet
from blender.bpy.ui.layout.rows import Label, Layout
from blender.bpy.ui.surfaces.panels import Beside, Panel
from blender.records.fields import Case

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

# --- [CONSTANTS] ------------------------------------------------------------------------

_LOOK: Final[Key] = "look"

# --- [OPERATIONS] -----------------------------------------------------------------------


def _look(name: str, *settings: tuple[Case, Socket, float]) -> Valued[LayerStack[Case]]:
    """Look stack of one adjustment layer through each effect record at its socket's value."""
    layer = StackLayer[Case](key=_LOOK, name=name, source=Adjustment(), effects=tuple(Entry(record=record) for record, _, _ in settings))
    return Valued(structure=LayerStack(layers=(layer,)), values=snapshot({path_key(place, socket): value for place, (_, socket, value) in zip(layer.places, settings, strict=True)}))


def _unformed(identity: Identity, context: bpy.types.Context) -> tuple[str, ...]:
    """Notice while the active entry's effect ranks after formation and no formation entry precedes it."""
    return tuple(
        f"Needs earlier {formation.title} entry"
        for scene in Option.of_optional(context.scene).to_list()
        for entry in Option.of_optional(scene.compositor_effects.active).to_list()
        for row in chain.row(identity, entry).to_list()
        if chain.rank(row) > chain.rank(formation) and not any(scene.compositor_effects.find(formed.name) < scene.compositor_effects.find(entry.name) for formed in formation.entries(identity, scene))
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

tones: Final = Exclusive[Any](members=(tone_curve, hable, tony, neutral, photoreceptor))
chain: Final = EffectRows[Any](
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
        Phase(
            key="grade",
            members=(exposure, flicker, grain, white_balance, primaries, gamut_compression, levels, zones, Exclusive(members=(cdl,)), primary, saturation, curves, hue_curves, lookup, local_exposure),
        ),
        Phase(key="formation", members=(tones, Exclusive(members=(formation,)), Exclusive(members=(peak_mapping,)))),
        Phase(key="finish", members=(pixelation, palette, pattern, edges, damage, emulation, Exclusive(members=(dither,)))),
    ),
    pinned=(
        PINNED,
        Pinned(ownership=temporal.OWNERSHIP, edge=Edge.HEAD, enables=frozenset({Enabled.RENDER})),
        Pinned(ownership=PassSet.ownership, edge=Edge.TAIL, enables=frozenset({Enabled.RENDER})),
    ),
)
render: Final = StagedRender(rows=chain)
tiles: Final = EffectTiles[Any](rows=chain, closing=Some(formation))
stacks: Final = LayerStacks[Any](
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
    EntryEditor(
        rows=chain,
        header=(ENTRY_LOCKS.row(WHOLE),),
        leading=(EntryTile(tiles=tiles), Layout(rows=lambda identity: Ok((Label(text=partial(_unformed, identity), icon=Some("STATUS_ERROR_FILLED")),)))),
        trailing=(EntryLocks(),),
    ),
    Panel(section=stacks.section(), at=Beside(SCENE_PT_compositor_effects)),
    Panel(section=nodes.section(), at=Beside(NODE_PT_backdrop)),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["chain", "render", "rows", "stacks", "tiles", "tones"]
