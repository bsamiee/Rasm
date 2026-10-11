"""Darkroom image formation, tone response, exposure metering, and LUT exports."""

from bl_ui.properties_render import RENDER_PT_color_management
from bl_ui.space_topbar import TOPBAR_MT_file_export
import bpy
from expression import Some
import msgspec

from rasm.blender.commands.operators import Acquired
from rasm.blender.render.color.exposure import AutoExposure
from rasm.blender.render.color.luts import LutExport
from rasm.blender.render.color.spaces import change, ConfigInstall, FilmView
from rasm.blender.render.effects.sections import COMPOSITING
from rasm.blender.render.effects.tone.curves import Response
from rasm.blender.render.effects.tone.exposure import exposure
from rasm.blender.render.effects.viewers import ViewerRead
from rasm.blender.render.outputs.exports import image_type
from rasm.blender.render.previews.scenes import PreviewScene
from rasm.blender.render.review.frames import meter_pick
from rasm.blender.ui.drawing.charts import Outline
from rasm.blender.ui.drawing.handlers import Drawing, Overlay
from rasm.blender.ui.layout.rows import Member, OperatorButton
from rasm.blender.ui.layout.sections import Hosted, Single, Store
from rasm.blender.ui.screens.areas import AreaQuery
from rasm.blender.ui.surfaces.appends import Append
from rasm.blender.ui.surfaces.panels import Child, Mirror, Panel, Sidebar
from rasm.imaging.chain import Stage
from rasm.imaging.color.authoring import ConfigBuild
from rasm.imaging.color.processors import DisplayView
from rasm.imaging.effects.tone.film import Film
from rasm.imaging.parameters import Gated, Rgb

from .effects import stack
from .preferences import category, retention

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
response = Response(stack=stack, sampling=sampling, selection=(Stage.TONE, DisplayView), allocation="blender:agxlog_rec2020_scene", caption="Tone Response")
rows = (
    change,
    install,
    LutExport(stack=stack, sampling=sampling, install=install),
    auto_exposure,
    response,
    Overlay(
        drawing=Drawing(area=AreaQuery(space="IMAGE_EDITOR", ui_type=Some("IMAGE_EDITOR"), shows=Some(image_type(("COMPOSITING",)))), part=Outline(read=response.chart), refreshes=(response.signal,)),
        space=bpy.types.SpaceImageEditor,
        scissor_enabled=False,
        respect_overlays=False,
    ),
    Append(
        target=TOPBAR_MT_file_export,  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        rows=tuple(OperatorButton(target=Acquired(key=key)) for key in (LutExport.formation_bake, LutExport.strip_bake, LutExport.tone_view)),
    ),
)
panels = (
    Panel(
        section=Single(key="formation", title="Image Formation", rows=(OperatorButton(target=change), OperatorButton(target=Acquired(key=ConfigInstall.key)))),
        at=Child(parent=RENDER_PT_color_management),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        mirrors=(Mirror(at=Sidebar(space="VIEW_3D", category=category)), Mirror(at=Sidebar(space="NODE_EDITOR", category=category), condition=Some(COMPOSITING))),
        children=(
            Single(
                key=AutoExposure.key,
                title="Auto Exposure",
                rows=(OperatorButton(target=meter_pick), OperatorButton(target=Acquired(key=AutoExposure.key))),
                store=Some(Hosted(host=Member(root=bpy.types.Context, path="scene"), store=Store(attachment=AutoExposure.attachment, retention=retention))),
            ),
        ),
    ),
)
page_rows = auto_exposure.page_rows

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["page_rows", "panels", "rows"]
