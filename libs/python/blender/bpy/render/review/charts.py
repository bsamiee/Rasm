"""Reference chart placements, calibration measurements, and patch overlays through the selected OCIO viewing pipeline."""

from enum import StrEnum
from typing import ClassVar, Self

import bpy
from expression import Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.data.ids.images import normalized
from blender.bpy.data.ids.ownership import Ownership, Saved
from blender.bpy.data.rna.conditions import Setting
from blender.bpy.data.rna.paths import root_in
from blender.bpy.data.rna.properties import enum_choices
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Declaration
from blender.bpy.render.review.history import RenderHistory
from blender.bpy.settings.attachments import Attached, Attachment, Stored
from blender.bpy.ui.drawing.canvas import Canvas, DrawType
from blender.bpy.ui.drawing.charts import Layer
from blender.bpy.ui.drawing.handlers import HandlerRefresh
from blender.bpy.ui.images import buffers
from blender.bpy.ui.images.views import framed, image_type, overlaid
from blender.bpy.ui.resources.theme import Metrics
from blender.imaging.color.calibration import ChartPlacement, Colors, converted_colors, PublishedChart
from blender.imaging.color.processors import converted_image
from blender.imaging.files.buffers import rgba
from blender.imaging.files.images import ImageFile
from blender.imaging.geometry.frames import Homography, Quad
from blender.imaging.geometry.panoramas import PanoramaView
from blender.records.faults import Error, Fault, Nothing, Ok, Outcome, paired, Reason

# --- [TYPES] ----------------------------------------------------------------------------


class Need(StrEnum):
    """Chart requirement, each value the report template of its fault over the subject lacking it."""

    PLACED = "Requires {subject} chart corners bounding a convex quad in view"
    SAMPLED = "Requires every {subject} patch sampled"
    CHOSEN = "Requires {subject} image with sampled chart"
    MATCHED = "Requires target on source chart {subject}"
    LIT = "Requires lit neutral patch in {subject} and target"


# --- [MODELS] ---------------------------------------------------------------------------


class PlacedChart(msgspec.Struct, frozen=True, kw_only=True):
    """Chart placement on an image and optional color-managed patch means."""

    placement: ChartPlacement = ChartPlacement()
    samples: Colors = ()
    colorspace: str = PyOpenColorIO.ROLE_SCENE_LINEAR

    def sampled(self, config: PyOpenColorIO.Config) -> Outcome[Option[Colors]]:
        """Returns sampled patch means in the current scene-linear space, absent before sampling."""
        return converted_colors(config, self.colorspace, PyOpenColorIO.ROLE_SCENE_LINEAR, self.samples).map(Some) if self.samples else Ok(Nothing)

    def positioned(self, placement: ChartPlacement) -> Self:
        """Returns a changed placement without measurements from its prior patch footprints."""
        return self if placement == self.placement else msgspec.structs.replace(self, placement=placement, samples=())


class ChartImage(bpy.types.PropertyGroup):
    """Chart over an image: whether it draws, the chart a placement positions, and its patch fill opacity, its placement and samples stored on the group."""

    shown: bpy.props.BoolProperty(name="Show Chart", description="Draw chart and handles over image", default=True)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    chart: bpy.props.EnumProperty(name="Chart", description="Chart a placement positions", items=enum_choices(PublishedChart))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    opacity: bpy.props.FloatProperty(name="Opacity", description="Patch fill opacity", subtype="FACTOR", min=0.0, max=1.0, default=0.5)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


# --- [SERVICES] -------------------------------------------------------------------------


class Charting(msgspec.Struct, frozen=True, kw_only=True):
    """Reference charts read from and drawn over images.

    Patch fills and reference edges pass through the selected OCIO viewing pipeline. Calibration samples remain explicit pre-effects measurements.
    """

    charts: ClassVar[Attachment[ChartImage]] = Attachment(key="chart", form=ChartImage, owner=bpy.types.Image)
    placed: ClassVar[Stored[PlacedChart]] = Stored(name="placed", decoder=msgspec.msgpack.Decoder(PlacedChart))
    views: ClassVar[Ownership[bpy.types.Image]] = Ownership(kind=bpy.types.Image, key="chart_view", saved=Saved.USED)
    imaged: ClassVar[Setting] = image_type(("IMAGE", "RENDER_RESULT"))

    # --- [READS]

    def shown(self, identity: Identity, context: bpy.types.Context) -> Option[tuple[bpy.types.Image, bpy.types.Image]]:
        """Returns the chart's image owner and actual shown image, identical outside a panorama view."""
        return (
            root_in(bpy.types.SpaceImageEditor, context)
            .bind(lambda space: Option.of_optional(space.image))
            .map(
                lambda image: (
                    self.views.marked(identity, image).bind(lambda mark: mark.of).bind(lambda block: Some(block) if isinstance(block, bpy.types.Image) else Nothing).default_value(image),
                    image,
                )
            )
        )

    def charted(self, identity: Identity, context: bpy.types.Context) -> Outcome[Option[tuple[bpy.types.Image, PlacedChart, Attached[ChartImage], bpy.types.Image]]]:
        """Returns the shown image's drawn chart, its panorama's chart for a chart view image, `Nothing` while the editor shows no charted image."""

        def held(chart: Attached[ChartImage], image: bpy.types.Image, shown: bpy.types.Image) -> Outcome[Option[tuple[bpy.types.Image, PlacedChart, Attached[ChartImage], bpy.types.Image]]]:
            return self.kept(chart, image).map(
                lambda found: found.filter(lambda kept: chart.struct(image).shown and (kept.placement.panorama is not None) == (image != shown)).map(lambda kept: (image, kept, chart, shown))
            )

        return self.charts.bound(identity).bind(lambda chart: self.shown(identity, context).starmap(lambda image, shown: held(chart, image, shown)).default_value(Ok(Nothing)))

    def kept(self, chart: Attached[ChartImage], image: bpy.types.Image) -> Outcome[Option[PlacedChart]]:
        """Returns an image's stored chart, `Nothing` for an image no write left a chart on, its attachment group left uncreated."""
        return self.placed.read(chart.struct(image)) if chart.stored(image) else Ok(Nothing)

    def record(self, identity: Identity, image: bpy.types.Image) -> Outcome[PlacedChart]:
        """Returns an image's chart, sampled fault naming an image holding none."""
        return self.charts.bound(identity).bind(lambda chart: self.kept(chart, image)).bind(lambda found: found.to_result_with(lambda: Fault.of(Need.SAMPLED, image.name)))

    def sampled(self, identity: Identity, _owner: object, image: bpy.types.Image) -> bool:
        """Returns whether an image holds a chart with every patch sampled, the poll of an image pointer choosing sampled charts."""
        return self.record(identity, image).map(lambda record: bool(record.samples)).default_value(value=False)

    @staticmethod
    def centered(chart: PublishedChart, ratio: float, panorama: PanoramaView | None = None) -> ChartPlacement:
        """Returns a chart centered on the image across half its width, its cells square at the image's aspect ratio."""
        columns, rows = chart.shape
        return ChartPlacement(chart=chart, panorama=panorama).realigned(Quad.of((Quad().samples[:, :2] - 0.5) * (0.5, 0.5 * ratio * rows / columns) + 0.5))

    @staticmethod
    def read(identity: Identity, image: bpy.types.Image, config: PyOpenColorIO.Config) -> Outcome[ImageFile]:
        """Returns normalized associated RGBA from an ordinary image or an explicit completed frame's raw source."""
        return (
            RenderHistory
            .opened(identity, image, config)
            .bind(
                lambda held: held.map(
                    lambda frame: paired(frame.source(), frame.source_encoding).bind(lambda found: converted_image(config, PyOpenColorIO.ROLE_SCENE_LINEAR, found[0], source=found[1]))
                ).default_with(
                    lambda: (
                        Error(Fault.of(Reason.REQUIRED, repr(image), member="explicit captured file and pass source"))
                        if image.type in {"RENDER_RESULT", "COMPOSITING", "MULTILAYER"}
                        else normalized(image, config)
                    )
                )
            )
            .map(lambda found: msgspec.structs.replace(found, pixels=rgba(found.pixels)))
        )

    # --- [REGISTRATION]

    @property
    def prerequisites(self) -> tuple[Declaration, ...]:
        """Chart attachment and recorded render sources."""
        return (self.charts, RenderHistory())

    def acquisitions(self, identity: Identity) -> Acquisitions:
        """Returns chart views and the patch overlay."""
        return (
            self.views,
            overlaid(
                Patches(charting=self, identity=identity),
                self.imaged,
                (RenderHistory.signal[identity], HandlerRefresh(handlers=bpy.app.handlers.depsgraph_update_post, ids=(bpy.types.Scene, bpy.types.Image))),
            ),
        )


class Patches(msgspec.Struct, frozen=True, kw_only=True):
    """Part batching the current editor's patch fills and reference edges in frame fractions."""

    draw_type: ClassVar[DrawType] = "POST_PIXEL"
    charting: Charting
    identity: Identity

    def read(self, _metrics: Metrics, /) -> Outcome[Option[tuple[Layer, Layer]]]:
        """Batches the shown chart's fills at its opacity and its reference edges opaque."""
        charting, identity, context, config = self.charting, self.identity, bpy.context, PyOpenColorIO.GetCurrentConfig()

        def layered(image: bpy.types.Image, record: PlacedChart, chart: Attached[ChartImage], shown: bpy.types.Image) -> Outcome[Option[tuple[Layer, Layer]]]:
            def batched(homography: Homography, samples: Colors, references: npt.NDArray[np.float32]) -> tuple[Layer, Layer]:
                corners = homography.to_image(record.placement.footprints)[..., :2].astype(np.float32)
                count = len(corners)
                starts, positions = (np.arange(count, dtype=np.uint32) * 4)[:, np.newaxis, np.newaxis], corners.reshape(-1, 2)
                alphas = np.concatenate((np.full((count, 1), chart.struct(image).opacity, np.float32), np.ones((count, 1), np.float32)))
                colors = np.concatenate((np.asarray(samples, np.float32), references))
                tinted = np.repeat(np.concatenate((colors, alphas), axis=-1), 4, axis=0)
                return (
                    Layer.of("TRIS", positions, tinted[: 4 * count], indices=Some((starts + np.array(((0, 1, 2), (0, 2, 3)), np.uint32)).reshape(-1, 3))),
                    Layer.of("LINES", positions, tinted[4 * count :], indices=Some((starts + np.array(((0, 1), (1, 2), (2, 3), (3, 0)), np.uint32)).reshape(-1, 2))),
                )

            def colored(shown: tuple[bpy.types.Image, ImageFile], reference: Colors, placement: ChartPlacement) -> Outcome[Option[tuple[Layer, Layer]]]:
                return buffers.projection(context, shown[0], config).bind(
                    lambda view: paired(view.form(config, rgba(shown[1].pixels), shown[1].colorspace), view.form(config, np.asarray(reference, np.float32))).bind(
                        lambda formed: placement.means(formed[0]).map(lambda measured: measured.map(lambda sample: batched(*sample, formed[1])))
                    )
                )

            return record.placement.viewed.map(
                lambda quad: paired(
                    RenderHistory.opened(identity, shown, config).bind(
                        lambda held: held.map(
                            lambda frame: paired(frame.read(), frame.encoding).bind(lambda found: converted_image(config, PyOpenColorIO.ROLE_SCENE_LINEAR, found[0], source=found[1]))
                        ).default_with(lambda: normalized(shown, config))
                    ),
                    record.placement.chart.values(config),
                ).bind(lambda found: colored((shown, found[0]), found[1], msgspec.structs.replace(record.placement, quad=quad, panorama=None)))
            ).default_value(Ok(Nothing))

        return charting.charted(identity, context).bind(lambda found: found.map(lambda held: layered(*held)).default_value(Ok(Nothing)))

    @staticmethod
    def drawn(state: tuple[Layer, Layer], canvas: Canvas, /) -> None:
        """Draws the shown chart's fills then edges through the frame."""
        with Canvas.mapped(framed(canvas.region)):
            for layer in state:
                layer.drawn(canvas)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ChartImage", "Charting", "Need", "Patches", "PlacedChart"]
