"""Effect stack, active effect, and strip, save, and preview rows."""

from importlib.resources import files

from bl_ui.properties_compositor import SCENE_MT_add_compositor_effect, SCENE_PT_compositor_effects
from bl_ui.space_node import NODE_PT_backdrop
from bl_ui.space_sequencer import SEQUENCER_MT_modifier_add
from expression import Some

from blender.bpy.render.color.luts import Enabled
from blender.bpy.render.effects.compositing.layers import LayerStacks
from blender.bpy.render.effects.detail.denoise import spatial_denoise
from blender.bpy.render.effects.detail.grain import grain
from blender.bpy.render.effects.detail.sharpen import sharpen
from blender.bpy.render.effects.generators.flares import lens_flare, light_rays
from blender.bpy.render.effects.generators.noise import noise_field
from blender.bpy.render.effects.generators.particles import particles
from blender.bpy.render.effects.generators.shapes import shapes
from blender.bpy.render.effects.grade.cdl import cdl
from blender.bpy.render.effects.grade.curves import curves, hue_curves
from blender.bpy.render.effects.grade.gamut import gamut_compression
from blender.bpy.render.effects.grade.lookup import lookup
from blender.bpy.render.effects.grade.primaries import primaries
from blender.bpy.render.effects.grade.primary import contrast, primary, saturation
from blender.bpy.render.effects.grade.white_balance import white_balance
from blender.bpy.render.effects.grade.zones import zones
from blender.bpy.render.effects.library import save_effect
from blender.bpy.render.effects.optics.atmosphere import atmosphere
from blender.bpy.render.effects.optics.defocus import defocus
from blender.bpy.render.effects.optics.distortion import aberration, distortion
from blender.bpy.render.effects.optics.glare import bloom, flares, glare, halation
from blender.bpy.render.effects.optics.vignette import vignette
from blender.bpy.render.effects.quantization import dither
from blender.bpy.render.effects.sections import effect_section
from blender.bpy.render.effects.stack import Edge, EffectStack, Pinned
from blender.bpy.render.effects.strips import StripEffect
from blender.bpy.render.effects.stylize.damage import damage, flicker
from blender.bpy.render.effects.stylize.edges import edges
from blender.bpy.render.effects.stylize.emulation import emulation
from blender.bpy.render.effects.stylize.palettes import palette
from blender.bpy.render.effects.stylize.patterns import pattern, pixelation
from blender.bpy.render.effects.tone.curves import hable, neutral, photoreceptor, tone_curve, tony
from blender.bpy.render.effects.tone.exposure import exposure, local_exposure
from blender.bpy.render.effects.tone.levels import levels
from blender.bpy.render.effects.tone.peak import peak_mapping
from blender.bpy.render.effects.viewers import EffectViewer
from blender.bpy.render.effects.warp.displacement import displacement
from blender.bpy.render.effects.warp.projection import projection
from blender.bpy.render.effects.warp.transforms import transform
from blender.bpy.render.previews.nodes import NodePreview, StageThumbnail
from blender.bpy.render.previews.tiles import Tiles
from blender.bpy.render.sessions.queue import QueueRun
from blender.bpy.render.sessions.sets import SetupLocks
from blender.bpy.render.view_layers.passes import PassSet
from blender.bpy.ui.layout.rows import Pulldown
from blender.bpy.ui.surfaces.appends import Append
from blender.bpy.ui.surfaces.panels import Joined, Panel, Tab
from blender.imaging.effects import tone

from .preferences import previewed, previewing

# --- [COMPOSITION] ----------------------------------------------------------------------

stack = EffectStack(
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
        tony(files(tone).joinpath("tony_mc_mapface.spi3d")),
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
    pinned=(LayerStacks.pinned, Pinned(ownership=PassSet.ownership, title=PassSet.title, edge=Edge.TAIL, enables=frozenset({Enabled.RENDER})), QueueRun.pinned),
    gain=Some(exposure),
)
viewer = EffectViewer(stack=stack)
strips = StripEffect(stack=stack)
tiles = Tiles(stack=stack)
nodes = NodePreview(stack=stack, tiles=tiles, settings=previewing, stored=previewed)
sections = effect_section(viewer=viewer, thumbnail=StageThumbnail(preview=nodes), locks=Some(SetupLocks.effect_actions()))
adding = stack.menu(key="effect_add", title="Darkroom", description="Add a Darkroom effect by imaging phase")
rows = (
    stack,
    adding,
    Append(target=SCENE_MT_add_compositor_effect, rows=(Pulldown(menu=adding.key),)),
    tiles,
    nodes,
    strips,
    save_effect,
    Append(target=SEQUENCER_MT_modifier_add, rows=(Pulldown(menu=strips.menu.key),), condition=strips.menu.condition),
    Panel(section=sections, at=Tab(context=SCENE_PT_compositor_effects.bl_context)),
    Panel(section=nodes.section(), at=Joined(space=NODE_PT_backdrop.bl_space_type, category=NODE_PT_backdrop.bl_category)),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["rows", "stack", "tiles", "viewer"]
