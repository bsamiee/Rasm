"""Reference color charts on images: the placement record each image holds, its reads, and its overlay of patch fills and reference edges in the colors the editor shows the image in."""

from collections.abc import Mapping
from contextlib import nullcontext
from enum import StrEnum
from functools import partial
from typing import ClassVar, Final

import bpy
from expression import Error, Nothing, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.data.ids.images import fingerprinted, read
from blender.bpy.data.ids.ownership import Ownership, Saved
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.conditions import Setting
from blender.bpy.data.rna.paths import root_in, rooted
from blender.bpy.data.rna.properties import enum_choices
from blender.bpy.extension.identity import Identity, Resource
from blender.bpy.extension.registration import Acquisitions, Declaration
from blender.bpy.render.color.spaces import viewed
from blender.bpy.render.outputs.framing import Framing
from blender.bpy.render.outputs.results import combined, saved
from blender.bpy.render.previews.queue import Job, PreviewFeed, Rank
from blender.bpy.render.previews.scenes import PreviewScene
from blender.bpy.settings.attachments import Attached, Attachment, Stored
from blender.bpy.ui.drawing.canvas import Canvas, DrawType
from blender.bpy.ui.drawing.charts import Layer
from blender.bpy.ui.drawing.handlers import HandlerRefresh, Refresh, Signal
from blender.bpy.ui.images.views import framed, image_type, overlaid
from blender.bpy.ui.resources.theme import Metrics
from blender.imaging.color.calibration import ChartPlacement, Colors, PublishedChart
from blender.imaging.color.processors import converted, DisplayView
from blender.imaging.files.images import ImageFile
from blender.imaging.geometry.frames import Homography, Quad
from blender.records.faults import accumulated, Fault, Outcome, paired, Reason
from blender.records.stores import content_hash, ContentHash

# --- [TYPES] ----------------------------------------------------------------------------


class Need(StrEnum):
    """Chart requirement, each value the report template of its fault over the subject lacking it."""

    PLACED = "Requires {subject} chart corners bounding a convex quad in view"
    SAMPLED = "Requires every {subject} patch sampled"
    CHOSEN = "Requires {subject} image with sampled chart"
    MATCHED = "Requires target on source chart {subject}"
    LIT = "Requires lit neutral patch in {subject} and target"


# --- [CONSTANTS] ------------------------------------------------------------------------

_TRIANGLES: Final = np.array(((0, 1, 2), (0, 2, 3)), np.uint32)
_EDGES: Final = np.array(((0, 1), (1, 2), (2, 3), (3, 0)), np.uint32)
_TRIANGLES.setflags(write=False)
_EDGES.setflags(write=False)

# --- [MODELS] ---------------------------------------------------------------------------


class PlacedChart(msgspec.Struct, frozen=True, kw_only=True):
    """Chart placement on an image and its scene-linear patch means in grid order."""

    placement: ChartPlacement = ChartPlacement()
    samples: Colors = ()

    @property
    def sampled(self) -> Option[Colors]:
        """Samples when every printed patch holds one."""
        return Some(self.samples).filter(lambda held: len(held) == len(self.placement.chart.checker.data))

    def values(self, config: PyOpenColorIO.Config) -> Outcome[npt.NDArray[np.float32]]:
        """Returns the scene-linear patch fills, the samples or else the chart's reference values, stacked over the reference values the edges draw in."""
        return self.placement.chart.values(config).map(lambda references: np.array((*self.sampled.default_value(references), *references), np.float32))


class ChartImage(bpy.types.PropertyGroup):
    """Chart over an image: whether it draws, the chart a placement positions, and its patch fill opacity, its placement and samples stored on the group."""

    shown: bpy.props.BoolProperty(name="Show Chart", description="Draw chart and handles over image", default=True)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    chart: bpy.props.EnumProperty(name="Chart", description="Chart a placement positions", items=enum_choices(PublishedChart))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    opacity: bpy.props.FloatProperty(name="Opacity", description="Patch fill opacity", subtype="FACTOR", min=0.0, max=1.0, default=0.5)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


# --- [SERVICES] -------------------------------------------------------------------------

_COMPOSITED: Final = Resource[dict[ContentHash, npt.NDArray[np.float32]]](made=lambda _identity: nullcontext({}))
_SIGNAL: Final = Resource[Signal](made=lambda _identity: nullcontext(Signal()))


class Charting(msgspec.Struct, frozen=True, kw_only=True):
    """Reference charts read from and drawn over images.

    Patch fills draw the samples, else the reference values, and edges the reference values, both in the colors the editor shows the image in.
    Render Result colors pass through the scene's render-enabled compositor effects, sampled in the background once per content and the scene's view after them.
    """

    charts: ClassVar[Attachment[ChartImage]] = Attachment(key="chart", form=ChartImage, owner=bpy.types.Image)
    placed: ClassVar[Stored[PlacedChart]] = Stored(name="placed", decoder=msgspec.msgpack.Decoder(PlacedChart))
    views: ClassVar[Ownership[bpy.types.Image]] = Ownership(kind=bpy.types.Image, key="chart_view", saved=Saved.USED)
    imaged: ClassVar[Setting] = image_type(("IMAGE", "RENDER_RESULT"))

    # --- [READS]

    def charted(self, identity: Identity, context: bpy.types.Context) -> Outcome[Option[tuple[bpy.types.Image, PlacedChart]]]:
        """Returns the shown image's drawn chart, its panorama's chart for a chart view image, `Nothing` while the editor shows no charted image."""

        def held(chart: Attached[ChartImage], image: bpy.types.Image, *, viewing: bool) -> Outcome[Option[tuple[bpy.types.Image, PlacedChart]]]:
            return self.kept(chart, image).map(lambda found: found.filter(lambda kept: chart.struct(image).shown and (kept.placement.panorama is not None) == viewing).map(lambda kept: (image, kept)))

        def shown(chart: Attached[ChartImage]) -> Outcome[Option[tuple[bpy.types.Image, PlacedChart]]]:
            return (
                root_in(bpy.types.SpaceImageEditor, context)
                .bind(lambda space: Option.of_optional(space.image))
                .map(
                    lambda image: (
                        self.views
                        .marked(identity, image)
                        .bind(lambda mark: mark.of)
                        .bind(lambda block: Some((block, True)) if isinstance(block, bpy.types.Image) else Nothing)
                        .default_value((image, False))
                    )
                )
                .map(lambda pair: held(chart, pair[0], viewing=pair[1]))
                .default_value(Ok(Nothing))
            )

        return self.charts.bound(identity).bind(shown)

    def kept(self, chart: Attached[ChartImage], image: bpy.types.Image) -> Outcome[Option[PlacedChart]]:
        """Returns an image's stored chart, `Nothing` for an image no write left a chart on, its attachment group left uncreated."""
        return self.placed.read(chart.struct(image)) if chart.stored(image) else Ok(Nothing)

    def record(self, identity: Identity, image: bpy.types.Image) -> Outcome[PlacedChart]:
        """Returns an image's chart, sampled fault naming an image holding none."""
        return self.charts.bound(identity).bind(lambda chart: self.kept(chart, image)).bind(lambda found: found.to_result_with(lambda: Fault.of(Need.SAMPLED, image.name)))

    def sampled(self, identity: Identity, _owner: object, image: bpy.types.Image) -> bool:
        """Returns whether an image holds a chart with every patch sampled, the poll of an image pointer choosing sampled charts."""
        return self.record(identity, image).to_option().bind(lambda record: record.sampled).is_some()

    @staticmethod
    def aspect(context: bpy.types.Context, image: bpy.types.Image) -> Outcome[float]:
        """Returns an image's displayed width over height, Render Result's from the scene's framing, absent fault for an image holding no pixels."""
        match image.type:
            case "RENDER_RESULT":
                return rooted(bpy.types.Scene, context).map(lambda scene: Framing.of(scene.render).aspect)
            case _:
                width, height = np.asarray(image.size)
                return Ok(float(width / height)) if width > 0 and height > 0 else Error(Fault.of(Reason.ABSENT, repr(image), member="pixels"))

    @staticmethod
    def centered(chart: PublishedChart, ratio: float) -> ChartPlacement:
        """Returns a chart centered on the image across half its width, its cells square at the image's aspect ratio."""
        columns, rows = chart.shape
        return ChartPlacement(chart=chart, quad=Quad.of(np.array(((-1.0, 1.0), (1.0, 1.0), (1.0, -1.0), (-1.0, -1.0))) * (0.25, 0.25 * ratio * rows / columns) + 0.5))

    @staticmethod
    def plane(context: bpy.types.Context, image: bpy.types.Image) -> Outcome[npt.NDArray[np.float32]]:
        """Returns an image's scene-linear associated RGBA plane, Render Result's as the context view layer's saved Combined pass."""
        config = PyOpenColorIO.GetCurrentConfig()

        def linear(found: ImageFile) -> Outcome[npt.NDArray[np.float32]]:
            return converted(config, PyOpenColorIO.ColorSpaceTransform(src=found.colorspace, dst=PyOpenColorIO.ROLE_SCENE_LINEAR), np.ascontiguousarray(found.pixels)).map(
                lambda plane: plane if plane.shape[2] == 4 else np.concatenate((plane, np.ones((*plane.shape[:2], 1), np.float32)), axis=-1)
            )

        match image.type:
            case "RENDER_RESULT":
                return paired(rooted(bpy.types.Scene, context), rooted(bpy.types.ViewLayer, context)).bind(lambda pair: saved(pair[0]).bind(lambda path: combined(path, Some(pair[1].name))))
            case _:
                return read(image, config).bind(linear)

    # --- [COLORS]

    @staticmethod
    def effects(scene: bpy.types.Scene) -> tuple[bpy.types.SceneCompositorEffect, ...]:
        """Returns the compositor effects a scene render runs, none while compositing is off."""
        return tuple(entry for entry in scene.compositor_effects if entry.enable_for_render) if scene.render.use_compositing else ()

    @staticmethod
    def keyed(effects: tuple[bpy.types.SceneCompositorEffect, ...], values: npt.NDArray[np.float32]) -> Outcome[ContentHash]:
        """Returns the content key of values passed through effects."""
        return accumulated(map(fingerprinted, effects)).map(lambda keys: content_hash(Charting, *(key for found in keys for key in found.to_list()), arrays=(values,)))

    def composited(self, identity: Identity, scene: bpy.types.Scene, values: npt.NDArray[np.float32]) -> Outcome[Option[npt.NDArray[np.float32]]]:
        """Returns scene-linear values passed through the scene's render effects, `Nothing` until their background sampling finished."""
        effects = self.effects(scene)
        return self.keyed(effects, values).map(lambda key: Option.of_optional(identity.resources[_COMPOSITED].get(key))) if effects else Ok(Some(values))

    def colors(self, identity: Identity, scene: bpy.types.Scene, image: bpy.types.Image, values: npt.NDArray[np.float32]) -> Outcome[Option[npt.NDArray[np.float32]]]:
        """Returns scene-linear values in the colors the editor shows the image in: through the effects and the scene's view for Render Result, the scene's view for a view-as-render image, else the display's default view."""
        config, view = PyOpenColorIO.GetCurrentConfig(), viewed(scene)
        match image.type, image.use_view_as_render:
            case "RENDER_RESULT", _:
                return self.composited(identity, scene, values).bind(lambda found: found.map(lambda linear: view.form(config, linear).map(Some)).default_value(Ok(Nothing)))
            case _, True:
                return view.form(config, values).map(Some)
            case _:
                return DisplayView(display=view.display, view=config.getDefaultView(view.display)).form(config, values).map(Some)

    def jobs(self, identity: Identity, context: bpy.types.Context) -> Outcome[tuple[Job, ...]]:
        """Returns the sampling request of each Render Result chart with colors through the scene's effects not held yet."""
        held, config = identity.resources[_COMPOSITED], PyOpenColorIO.GetCurrentConfig()

        def wanted(scene: bpy.types.Scene, image: bpy.types.Image, record: PlacedChart) -> Outcome[Option[Job]]:
            effects = self.effects(scene)

            def kept(key: ContentHash, plane: npt.NDArray[np.float32]) -> Outcome[None]:
                held[key] = plane[0]
                return identity.resources[_SIGNAL].fired()

            def stored(key: ContentHash, values: npt.NDArray[np.float32]) -> Outcome[None]:
                return PreviewScene().sampled(identity, scene, effects, values[np.newaxis]).bind(partial(kept, key))

            return record.values(config).bind(
                lambda values: self.keyed(effects, values).map(
                    lambda key: Some(Job(place=(Charting, Handle.of(image)), key=key, rank=Rank.SHOWN, run=lambda: stored(key, values))).filter(lambda _: bool(effects) and key not in held)
                )
            )

        def images(scene: bpy.types.Scene, chart: Attached[ChartImage]) -> Outcome[tuple[Job, ...]]:
            return accumulated(
                self.kept(chart, image).bind(lambda found: found.map(partial(wanted, scene, image)).default_value(Ok(Nothing))) for image in bpy.data.images if image.type == "RENDER_RESULT"
            ).map(lambda found: tuple(job for each in found for job in each.to_list()))

        return root_in(bpy.types.Scene, context).map(lambda scene: self.charts.bound(identity).bind(partial(images, scene))).default_value(Ok(()))

    # --- [REGISTRATION]

    @staticmethod
    def refreshes(identity: Identity) -> tuple[Refresh, ...]:
        """Returns the refreshes of every drawing over charts: scene and image updates, and each finished effect sampling."""
        return (HandlerRefresh(handlers=bpy.app.handlers.depsgraph_update_post, ids=(bpy.types.Scene, bpy.types.Image)), identity.resources[_SIGNAL])

    @property
    def prerequisites(self) -> tuple[Declaration, ...]:
        """Chart attachment, held effect colors, and their signal."""
        return (self.charts, _COMPOSITED, _SIGNAL)

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns the chart views, the preview scene and background effect sampling, and the patch overlay."""
        return Ok((self.views, PreviewScene(), PreviewFeed(feed=self.jobs), overlaid(Patches(charting=self, identity=identity), self.imaged, self.refreshes(identity))))


class Patches(msgspec.Struct, frozen=True, kw_only=True):
    """Part batching each charted image's patch fills and reference edges once per refresh, in frame fractions, and drawing the shown chart's through the frame."""

    draw_type: ClassVar[DrawType] = "POST_PIXEL"
    charting: Charting
    identity: Identity

    def read(self, _metrics: Metrics, /) -> Outcome[Option[Mapping[Handle[bpy.types.Image], tuple[Layer, Layer]]]]:
        """Batches every charted image's fills at its opacity and its edges opaque, an image with unformed colors left out."""
        charting, identity, config = self.charting, self.identity, PyOpenColorIO.GetCurrentConfig()

        def layered(scene: bpy.types.Scene, image: bpy.types.Image, group: ChartImage, record: PlacedChart) -> Outcome[Option[tuple[Handle[bpy.types.Image], tuple[Layer, Layer]]]]:
            def batched(homography: Homography, colors: npt.NDArray[np.float32]) -> tuple[Handle[bpy.types.Image], tuple[Layer, Layer]]:
                corners = homography.to_image(record.placement.footprints)[..., :2].astype(np.float32)
                count = len(corners)
                starts, positions = (np.arange(count, dtype=np.uint32) * 4)[:, np.newaxis, np.newaxis], corners.reshape(-1, 2)
                alphas = np.concatenate((np.full((count, 1), group.opacity, np.float32), np.ones((count, 1), np.float32)))
                tinted = np.repeat(np.concatenate((colors, alphas), axis=-1), 4, axis=0)
                return Handle.of(image), (
                    Layer.of("TRIS", positions, tinted[: 4 * count], indices=Some((starts + _TRIANGLES).reshape(-1, 3))),
                    Layer.of("LINES", positions, tinted[4 * count :], indices=Some((starts + _EDGES).reshape(-1, 2))),
                )

            return paired(record.placement.aligned, record.values(config)).bind(
                lambda found: found[0].map(lambda homography: charting.colors(identity, scene, image, found[1]).map(lambda formed: formed.map(partial(batched, homography)))).default_value(Ok(Nothing))
            )

        def charted(scene: bpy.types.Scene, chart: Attached[ChartImage]) -> Outcome[Option[Mapping[Handle[bpy.types.Image], tuple[Layer, Layer]]]]:
            return accumulated(
                charting.kept(chart, image).bind(lambda found: found.map(partial(layered, scene, image, chart.struct(image))).default_value(Ok(Nothing))) for image in bpy.data.images
            ).map(lambda found: Some(dict(pair for each in found for pair in each.to_list())).filter(bool))

        return root_in(bpy.types.Scene, bpy.context).map(lambda scene: charting.charts.bound(identity).bind(partial(charted, scene))).default_value(Ok(Nothing))

    def drawn(self, canvas: Canvas, state: Mapping[Handle[bpy.types.Image], tuple[Layer, Layer]], /) -> None:
        """Draws the shown chart's fills then edges through the frame."""
        for layers in (
            self.identity.reports.reported(self.charting.charted)(self.identity, bpy.context).bind(lambda found: found).bind(lambda pair: Option.of_optional(state.get(Handle.of(pair[0])))).to_list()
        ):
            with Canvas.mapped(framed(canvas.region)):
                for layer in layers:
                    layer.drawn(canvas)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ChartImage", "Charting", "Need", "Patches", "PlacedChart"]
