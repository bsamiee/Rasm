"""Darkroom image formation, tone response, exposure metering, and LUT exports."""

from bl_ui.properties_render import RENDER_PT_color_management
from bl_ui.space_topbar import TOPBAR_MT_file_export
import bpy
from expression import Nothing, Some
import msgspec

from blender.bpy.render.color.exposure import AutoExposure
from blender.bpy.render.color.luts import LutExport
from blender.bpy.render.color.spaces import ConfigInstall, FilmView, WorkingColors
from blender.bpy.render.effects.tone.curves import Response
from blender.bpy.render.effects.tone.exposure import exposure
from blender.bpy.render.effects.viewers import ViewerRead
from blender.bpy.render.outputs.exports import image_type
from blender.bpy.render.previews.scenes import PreviewScene
from blender.bpy.render.review.frames import meter_pick
from blender.bpy.ui.drawing.charts import Outline
from blender.bpy.ui.drawing.handlers import Drawing, Overlay
from blender.bpy.ui.layout.rows import Caption, Flow, Member, OperatorButton
from blender.bpy.ui.layout.sections import Hosted, Single, Store
from blender.bpy.ui.screens.areas import AreaQuery
from blender.bpy.ui.surfaces.appends import Append
from blender.bpy.ui.surfaces.panels import Child, Panel
from blender.imaging.chain import Stage
from blender.imaging.color.authoring import ConfigBuild
from blender.imaging.color.processors import DisplayView
from blender.imaging.effects.tone.film import Film
from blender.imaging.parameters import Gated, Rgb

from .effects import stack

# --- [COMPOSITION] ----------------------------------------------------------------------

film = Film()
install = ConfigInstall(
    build=ConfigBuild(formations=()),
    films=(
        FilmView(name="Film", description="Negative and print film at flim's default stocks", film=film),
        FilmView(
            name="Film Nostalgia",
            description="Warm faded film at flim's nostalgia stocks",
            film=msgspec.structs.replace(
                film,
                pre_exposure=5.563035,
                red=msgspec.structs.replace(film.red, gain=1.1),
                blue=msgspec.structs.replace(film.blue, gain=1.2),
                characteristic=msgspec.structs.replace(film.characteristic, span=23.0 - film.characteristic.low),
                negative=msgspec.structs.replace(film.negative, exposure=5.8),
                backlight=Rgb(red=0.99, green=1.1, blue=1.035989),
                positive=msgspec.structs.replace(film.positive, density=40.0),
                black_point=Gated(enabled=True, value=-0.005),
                saturation=1.1,
            ),
        ),
        FilmView(
            name="Film Silver",
            description="Cool dense film at flim's silver stocks",
            film=msgspec.structs.replace(
                film,
                pre_exposure=3.9,
                pre_filter=Rgb(red=0.95, green=0.975, blue=1.0),
                blue=msgspec.structs.replace(film.blue, gain=1.06),
                negative=msgspec.structs.replace(film.negative, exposure=4.7, density=7.0),
                backlight=Rgb(red=0.9992, green=0.99, blue=1.0),
                positive=msgspec.structs.replace(film.positive, exposure=4.7, density=30.0),
                black_point=Gated(enabled=True, value=0.0005),
                post_filter=Rgb(red=1.0, green=1.0, blue=0.96),
                saturation=1.0,
            ),
        ),
    ),
)
auto_exposure = AutoExposure(stack=stack, gain=exposure, reading=ViewerRead)
sampling = PreviewScene(stack=stack)
luts = LutExport(stack=stack, sampling=sampling, install=install)
response = Response(stack=stack, sampling=sampling, selection=(Stage.TONE, DisplayView), allocation="blender:agxlog_rec2020_scene", caption="Tone Response")
rows = (
    WorkingColors(),
    install,
    luts,
    auto_exposure,
    response,
    Overlay(
        drawing=Drawing(area=AreaQuery(space="IMAGE_EDITOR", ui_type=Some("IMAGE_EDITOR"), shows=Some(image_type(("COMPOSITING",)))), part=Outline(read=response.chart), refreshes=(response.signal,)),
        space=bpy.types.SpaceImageEditor,
        scissor_enabled=False,
    ),
    Append(target=TOPBAR_MT_file_export, rows=tuple(OperatorButton(target=command) for command in (luts.forming, luts.stripping))),
    Panel(
        section=Single(
            key="formation", title="Image Formation", rows=(Caption(rows=(OperatorButton(target=install.installing), OperatorButton(target=luts.toning)), flow=Flow.ALIGNED, separator=Nothing),)
        ),
        at=Child(parent=RENDER_PT_color_management),
        children=(
            Single(
                key=AutoExposure.key,
                title="Auto Exposure",
                rows=(OperatorButton(target=meter_pick), OperatorButton(target=auto_exposure.metering)),
                store=Some(Hosted(host=Member(root=bpy.types.Context, path="scene"), store=Store(attachment=AutoExposure.attachment))),
            ),
        ),
    ),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["rows"]
