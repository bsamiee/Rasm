"""Effect stack, Sidebar mirror, and strip, asset, and preview rows."""

from importlib.resources import files

import bpy
from expression import Nothing, Some

from blender.bpy.data.rna.conditions import Setting
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.files.assets import AssetLibrary
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
from blender.bpy.render.effects.library import EffectLibrary
from blender.bpy.render.effects.optics.atmosphere import atmosphere
from blender.bpy.render.effects.optics.defocus import defocus
from blender.bpy.render.effects.optics.distortion import aberration, distortion
from blender.bpy.render.effects.optics.glare import bloom, flares, glare, halation
from blender.bpy.render.effects.optics.vignette import vignette
from blender.bpy.render.effects.quantization import dither
from blender.bpy.render.effects.sections import COMPOSITING, effect_section
from blender.bpy.render.effects.stack import Edge, EffectStack, Pinned
from blender.bpy.render.effects.strips import active, StripEffect
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
from blender.bpy.ui.layout.rows import Member, OperatorButton, Property
from blender.bpy.ui.layout.sections import Single
from blender.bpy.ui.surfaces.panels import Mirror, Panel, Sidebar
from blender.imaging.effects import tone
from blender.imaging.effects.detail.denoise import SpatialDenoise
from blender.imaging.effects.detail.grain import Grain
from blender.imaging.effects.detail.sharpen import Sharpen
from blender.imaging.effects.generators.flares import LensFlare, LightRays
from blender.imaging.effects.generators.noise import NoiseField
from blender.imaging.effects.generators.particles import Particles
from blender.imaging.effects.generators.shapes import Shapes
from blender.imaging.effects.grade.cdl import Cdl
from blender.imaging.effects.grade.curves import Curves, HueCurves
from blender.imaging.effects.grade.gamut import GamutCompression
from blender.imaging.effects.grade.lookup import Lookup
from blender.imaging.effects.grade.primaries import Primaries
from blender.imaging.effects.grade.primary import Contrast, Primary, Saturation
from blender.imaging.effects.grade.white_balance import WhiteBalance
from blender.imaging.effects.grade.zones import Zones
from blender.imaging.effects.optics.atmosphere import Atmosphere
from blender.imaging.effects.optics.defocus import Defocus
from blender.imaging.effects.optics.distortion import Aberration, Distortion
from blender.imaging.effects.optics.glare import Bloom, Flares, Glare, Halation
from blender.imaging.effects.optics.vignette import Vignette
from blender.imaging.effects.stylize.damage import Flicker
from blender.imaging.effects.tone.curves import Hable, Neutral, Photoreceptor, ToneCurve, Tony
from blender.imaging.effects.tone.exposure import Exposure, LocalExposure
from blender.imaging.effects.tone.levels import Levels
from blender.imaging.effects.warp.displacement import Displacement
from blender.imaging.effects.warp.projection import Projection
from blender.imaging.effects.warp.transforms import Transform

from .preferences import category, previewed, previewing

# --- [TYPES] ----------------------------------------------------------------------------

type Effects = (
    SpatialDenoise
    | Atmosphere
    | Defocus
    | NoiseField
    | Particles
    | Shapes
    | LightRays
    | LensFlare
    | Displacement
    | Transform
    | Projection
    | Distortion
    | Aberration
    | Sharpen
    | Bloom
    | Glare
    | Flares
    | Vignette
    | Halation
    | Exposure
    | Flicker
    | Grain
    | WhiteBalance
    | Primaries
    | GamutCompression
    | Levels
    | Zones
    | Cdl
    | Primary
    | Contrast
    | Saturation
    | Curves
    | HueCurves
    | Lookup
    | LocalExposure
    | ToneCurve
    | Hable
    | Tony
    | Neutral
    | Photoreceptor
)

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
    pinned=(LayerStacks.pinned, Pinned(ownership=PassSet.ownership, edge=Edge.TAIL, enables=frozenset({Enabled.RENDER})), QueueRun.pinned),
    gain=Some(exposure),
)
viewer = EffectViewer(stack=stack)
strips = StripEffect(stack=stack)
tiles = Tiles(stack=stack)
nodes = NodePreview(stack=stack, tiles=tiles, settings=previewing, stored=previewed)
sections = effect_section(viewer=viewer, title="Effects", thumbnail=StageThumbnail(preview=nodes), locks=Some(SetupLocks.effect_actions()))

compositor = Single(
    key="compositor",
    title="Compositor",
    rows=tuple(
        Property(target=Member(root=bpy.types.Scene, path=f"render.{member}"), condition=condition)
        for member, condition in (
            ("use_compositing", Nothing),
            ("compositor_device", Nothing),
            (
                "compositor_precision",
                Some(Setting(descriptor=Descriptor(root=bpy.types.Scene, owner="render", prop=bpy.types.RenderSettings.bl_rna.properties["compositor_device"]), accepted=("GPU",))),
            ),
            ("compositor_denoise_device", Nothing),
            ("compositor_denoise_preview_quality", Nothing),
            ("compositor_denoise_final_quality", Nothing),
        )
    ),
)

rows = (
    stack,
    tiles,
    nodes,
    strips,
    EffectLibrary(
        stack=stack,
        library=AssetLibrary(title="Darkroom Effects", folder="effects"),
        thumbnail=Some(
            lambda identity, record, scene: Descriptor.resolve(bpy.types.AssetShelf, "bl_default_preview_size", bpy.types.IntProperty).bind(
                lambda descriptor: tiles.rendered(identity, record, descriptor.prop.hard_max, scene=scene)
            )
        ),
    ),
)
panels = (
    Panel(section=sections, at=Sidebar(space="NODE_EDITOR", category=category), condition=Some(COMPOSITING), mirrors=(Mirror(at=Sidebar(space="VIEW_3D", category=category)),), children=(compositor,)),
    Panel(
        section=Single(
            key="strip_effect",
            title="Strip Effect",
            rows=(lambda layout, _holder, context: active(context).map(layout.template_compositor_strip_inputs).to_optional(), *(OperatorButton(target=row) for row in strips.operators)),
        ),
        at=Sidebar(space="SEQUENCE_EDITOR", category=category),
    ),
    Panel(section=nodes.section(), at=Sidebar(space="NODE_EDITOR", category=category)),
)
page_rows = (*stack.page_rows, *nodes.page_rows)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Effects", "page_rows", "panels", "rows", "stack", "tiles", "viewer"]
