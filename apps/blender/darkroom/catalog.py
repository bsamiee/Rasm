"""Darkroom effect catalog: every effect row and the chain over them, with its layer stacks, Viewers, strips, previews, active effect panel, and add menu."""

from typing import Final

from bl_ui.properties_compositor import SCENE_MT_add_compositor_effect, SCENE_PT_compositor_effects
from bl_ui.space_node import NODE_PT_backdrop
from bl_ui.space_sequencer import SEQUENCER_MT_modifier_add
import bpy
from expression import Some

from blender.bpy.data.rna.descriptors import RnaPath, Value
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.render.compositor.blending import Entry
from blender.bpy.render.compositor.effects import Enabled
from blender.bpy.render.compositor.entries import Edge, Pinned
from blender.bpy.render.compositor.layers import Adjustment, LayerStack, StackLayer
from blender.bpy.render.compositor.library import save_effect
from blender.bpy.render.compositor.stacking import OWNERSHIP, TITLE, Valued
from blender.bpy.render.compositor.strips import Strips
from blender.bpy.render.compositor.viewers import Viewers
from blender.bpy.render.previews.nodes import NodePreview
from blender.bpy.render.view_layers.passes import PassSet
from blender.bpy.ui.layout.rows import Pulldown
from blender.bpy.ui.surfaces.appends import Append
from blender.bpy.ui.surfaces.panels import Joined, Panel, PopoverPanel, Tab
from blender.records.trees import Owner, Tree

from .chain import layers
from .chain.panels import effect_section
from .chain.previews import StageThumbnail, Tiles
from .chain.stack import Chain
from .effects.finish.damage import damage
from .effects.finish.dither import dither
from .effects.finish.edges import edges
from .effects.finish.emulation import emulation
from .effects.finish.palettes import palette
from .effects.finish.patterns import pattern, pixelation
from .effects.formation.display import formation, formed_view
from .effects.formation.peak import peak_mapping
from .effects.formation.tone import hable, neutral, photoreceptor, tone_curve, tony
from .effects.grade.cdl import cdl
from .effects.grade.curves import curves, hue_curves
from .effects.grade.exposure import Exposure, exposure, gain, local_exposure
from .effects.grade.flicker import flicker
from .effects.grade.gamut import gamut_compression
from .effects.grade.grain import grain
from .effects.grade.levels import levels
from .effects.grade.lookup import lookup
from .effects.grade.primaries import primaries
from .effects.grade.primary import Contrast, contrast, primary, saturation
from .effects.grade.white_balance import white_balance
from .effects.grade.zones import zones
from .effects.optics.atmosphere import atmosphere
from .effects.optics.defocus import defocus
from .effects.optics.denoise import filtering, Rendered, spatial_denoise
from .effects.optics.displacement import displacement
from .effects.optics.distortion import aberration, distortion
from .effects.optics.flares import lens_flare, light_rays
from .effects.optics.glare import bloom, flares, glare, halation
from .effects.optics.noise import noise_field
from .effects.optics.particles import particles
from .effects.optics.projection import projection
from .effects.optics.shapes import shapes
from .effects.optics.sharpen import sharpen
from .effects.optics.transforms import transform
from .effects.optics.vignette import vignette
from .preferences import previewing
from .rendering.sets import locks

# --- [COMPOSITION] ----------------------------------------------------------------------

chain: Final = Chain(
    effects=(
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
        flares,
        vignette,
        halation,
        exposure,
        flicker,
        grain,
        white_balance,
        primaries,
        gamut_compression,
        levels,
        zones,
        cdl,
        primary,
        contrast,
        saturation,
        curves,
        hue_curves,
        lookup,
        local_exposure,
        tone_curve,
        hable,
        tony,
        neutral,
        photoreceptor,
        peak_mapping,
        pixelation,
        palette,
        pattern,
        edges,
        damage,
        emulation,
        dither,
    ),
    formation=formation,
    view=formed_view,
    gain=gain,
    pinned=(
        Pinned(ownership=OWNERSHIP, title=TITLE, edge=Edge.HEAD),
        Pinned(ownership=Rendered.ownership, title=bpy.path.display_name(Rendered.ownership.key), edge=Edge.HEAD, enables=frozenset({Enabled.RENDER})),
        Pinned(ownership=PassSet.ownership, title=PassSet.title, edge=Edge.TAIL, enables=frozenset({Enabled.RENDER})),
    ),
)
stacks: Final = layers.stacks(
    chain,
    filtering=filtering,
    looks={
        name: Valued(
            structure=LayerStack(layers=(StackLayer(key="look", name=name, source=Adjustment(), effects=(Entry(record=Exposure()), Entry(record=Contrast()))),)),
            values=Snapshot(
                struct="SceneCompositorEffectInterfaceInputs",
                tree=Tree(
                    roots=((),),
                    rows=(
                        ((), Owner[Value, RnaPath](values={}, children=tuple((key,) for key in values))),
                        *(((key,), Owner[Value, RnaPath](values={"value": value})) for key, value in values.items()),
                    ),
                ),
            ),
        )
        for name, values in (
            ("Rhino Filmic, Low Contrast", {"look_effects_1_stops": 4.0, "look_effects_2_slope": 0.5}),
            ("Rhino Filmic, Medium Contrast", {"look_effects_1_stops": 1.0, "look_effects_2_slope": 1.0}),
            ("Rhino Filmic, High Contrast", {"look_effects_1_stops": -1.0, "look_effects_2_slope": 2.0}),
        )
    },
)
tiles: Final = Tiles(chain=chain)
strips: Final = Strips(rows=chain.rows)
nodes: Final = NodePreview(settings=previewing)
adding: Final = chain.rows.menu
rows: Final = (
    chain.rows,
    adding,
    Append(target=SCENE_MT_add_compositor_effect, rows=(Pulldown(menu=adding.key),)),
    tiles,
    nodes,
    strips,
    save_effect,
    stacks,
    Append(target=SEQUENCER_MT_modifier_add, rows=(Pulldown(menu=strips.menu.key),)),
    Panel(section=effect_section(chain, Viewers(rows=chain.rows), thumbnail=StageThumbnail(tiles=tiles), locks=Some(locks)), at=Tab.beside(SCENE_PT_compositor_effects)),
    Panel(section=stacks.section(), at=Tab.beside(SCENE_PT_compositor_effects)),
    Panel(section=stacks.presets(), at=PopoverPanel()),
    Panel(section=nodes.section(), at=Joined.beside(NODE_PT_backdrop)),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["chain", "rows", "stacks", "tiles"]
