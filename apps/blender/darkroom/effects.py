"""Darkroom's effect chain, its Sidebar copy, and the strip, asset, and preview rows over it."""

from importlib.resources import files

import bpy
from expression import Nothing, Some

from rasm.blender.data.rna.conditions import Setting
from rasm.blender.data.rna.descriptors import Descriptor
from rasm.blender.files.assets import AssetLibrary
from rasm.blender.render.color.luts import Enabled
from rasm.blender.render.effects.compositing.layers import LayerStacks
from rasm.blender.render.effects.detail.denoise import spatial_denoise
from rasm.blender.render.effects.detail.grain import grain
from rasm.blender.render.effects.detail.sharpen import sharpen
from rasm.blender.render.effects.generators.flares import lens_flare, light_rays
from rasm.blender.render.effects.generators.noise import noise_field
from rasm.blender.render.effects.generators.particles import particles
from rasm.blender.render.effects.generators.shapes import shapes
from rasm.blender.render.effects.grade.cdl import cdl
from rasm.blender.render.effects.grade.curves import curves, hue_curves
from rasm.blender.render.effects.grade.gamut import gamut_compression
from rasm.blender.render.effects.grade.lookup import lookup
from rasm.blender.render.effects.grade.primaries import primaries
from rasm.blender.render.effects.grade.primary import contrast, primary, saturation
from rasm.blender.render.effects.grade.white_balance import white_balance
from rasm.blender.render.effects.grade.zones import zones
from rasm.blender.render.effects.library import EffectLibrary
from rasm.blender.render.effects.optics.atmosphere import atmosphere
from rasm.blender.render.effects.optics.defocus import defocus
from rasm.blender.render.effects.optics.distortion import aberration, distortion
from rasm.blender.render.effects.optics.glare import bloom, flares, glare, halation
from rasm.blender.render.effects.optics.vignette import vignette
from rasm.blender.render.effects.quantization import dither
from rasm.blender.render.effects.sections import COMPOSITING, effect_section
from rasm.blender.render.effects.stack import Edge, EffectStack, Pinned
from rasm.blender.render.effects.strips import active, StripEffect
from rasm.blender.render.effects.stylize.damage import damage, flicker
from rasm.blender.render.effects.stylize.edges import edges
from rasm.blender.render.effects.stylize.emulation import emulation
from rasm.blender.render.effects.stylize.palettes import palette
from rasm.blender.render.effects.stylize.patterns import pattern, pixelation
from rasm.blender.render.effects.tone.curves import hable, neutral, photoreceptor, tone_curve, tony
from rasm.blender.render.effects.tone.exposure import exposure, local_exposure
from rasm.blender.render.effects.tone.levels import levels
from rasm.blender.render.effects.tone.peak import peak_mapping
from rasm.blender.render.effects.viewers import EffectViewer
from rasm.blender.render.effects.warp.displacement import displacement
from rasm.blender.render.effects.warp.projection import projection
from rasm.blender.render.effects.warp.transforms import transform
from rasm.blender.render.previews.nodes import NodePreview, StageThumbnail
from rasm.blender.render.previews.tiles import Tiles
from rasm.blender.render.sessions.queue import QueueRun
from rasm.blender.render.sessions.sets import SetupLocks
from rasm.blender.render.view_layers.passes import PassSet
from rasm.blender.ui.layout.rows import Member, OperatorButton, Property
from rasm.blender.ui.layout.sections import Single
from rasm.blender.ui.surfaces.panels import Mirror, Panel, Sidebar
from rasm.imaging.effects import tone
from rasm.imaging.effects.detail.denoise import SpatialDenoise
from rasm.imaging.effects.detail.grain import Grain
from rasm.imaging.effects.detail.sharpen import Sharpen
from rasm.imaging.effects.generators.flares import LensFlare, LightRays
from rasm.imaging.effects.generators.noise import NoiseField
from rasm.imaging.effects.generators.particles import Particles
from rasm.imaging.effects.generators.shapes import Shapes
from rasm.imaging.effects.grade.cdl import Cdl
from rasm.imaging.effects.grade.curves import Curves, HueCurves
from rasm.imaging.effects.grade.gamut import GamutCompression
from rasm.imaging.effects.grade.lookup import Lookup
from rasm.imaging.effects.grade.primaries import Primaries
from rasm.imaging.effects.grade.primary import Contrast, Primary, Saturation
from rasm.imaging.effects.grade.white_balance import WhiteBalance
from rasm.imaging.effects.grade.zones import Zones
from rasm.imaging.effects.optics.atmosphere import Atmosphere
from rasm.imaging.effects.optics.defocus import Defocus
from rasm.imaging.effects.optics.distortion import Aberration, Distortion
from rasm.imaging.effects.optics.glare import Bloom, Flares, Glare, Halation
from rasm.imaging.effects.optics.vignette import Vignette
from rasm.imaging.effects.stylize.damage import Flicker
from rasm.imaging.effects.tone.curves import Hable, Neutral, Photoreceptor, ToneCurve, Tony
from rasm.imaging.effects.tone.exposure import Exposure, LocalExposure
from rasm.imaging.effects.tone.levels import Levels
from rasm.imaging.effects.warp.displacement import Displacement
from rasm.imaging.effects.warp.projection import Projection
from rasm.imaging.effects.warp.transforms import Transform

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
            rows=(lambda layout, _holder, context: active(context).map(layout.template_compositor_strip_inputs).default_value(None), *(OperatorButton(target=row) for row in strips.operators)),
        ),
        at=Sidebar(space="SEQUENCE_EDITOR", category=category),
    ),
    Panel(section=nodes.section(), at=Sidebar(space="NODE_EDITOR", category=category)),
)
page_rows = (*stack.page_rows, *nodes.page_rows)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Effects", "page_rows", "panels", "rows", "stack", "tiles", "viewer"]
