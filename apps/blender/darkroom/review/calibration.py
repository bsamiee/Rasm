"""Darkroom chart calibration: a reference chart placed and sampled on an image, its fit written into the chain's primaries, a shader matrix, or transform files."""

from collections.abc import Callable
from enum import auto, Enum, StrEnum
from functools import cached_property, partial, update_wrapper
import math
from pathlib import Path
from typing import ClassVar, Final, TYPE_CHECKING

from bl_ui.space_image import IMAGE_PT_image_properties
from bl_ui.space_node import NODE_PT_active_node_generic
import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
import gpu
from gpu_extras.batch import batch_for_shader
from mathutils import Matrix
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.commands.operators import Answer, Call, Confirm, Done, Empty, Operator, Own, Properties, Save, Unchanged
from blender.bpy.data.ids.images import generated, read, Tag
from blender.bpy.data.ids.ownership import Mark, Ownership, Saved
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.conditions import AllOf, Setting
from blender.bpy.data.rna.descriptors import Descriptor, root_in, rooted, Struct, written
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Registrable
from blender.bpy.faults import BlenderReason
from blender.bpy.nodes.editing import Chosen, inserted
from blender.bpy.render.color.luts import Baking, extension, FORMATS, output, Writing
from blender.bpy.render.color.spaces import viewed
from blender.bpy.render.compositor.effects import Enabled, entered
from blender.bpy.render.outputs.exports import combined
from blender.bpy.render.outputs.results import saved
from blender.bpy.scene.materials import matrix
from blender.bpy.settings.attachments import Attached, Attachment, Stored
from blender.bpy.ui.drawing.canvas import Canvas
from blender.bpy.ui.drawing.handlers import Updated
from blender.bpy.ui.images.cages import Access, FrameRegion, Free, HANDLE
from blender.bpy.ui.images.overlays import overlaid
from blender.bpy.ui.images.views import framed, image_type
from blender.bpy.ui.layout.rows import alerted, Caption, Flow, Member, OperatorButton, Property, Row, Rows
from blender.bpy.ui.layout.sections import Single
from blender.bpy.ui.layout.values import Hosted, Store
from blender.bpy.ui.resources.icons import Glyph
from blender.bpy.ui.resources.theme import Metrics
from blender.bpy.ui.surfaces.gizmos import Button, Gizmos
from blender.bpy.ui.surfaces.panels import Joined, Panel
from blender.imaging.color.calibration import Calibration, CalibrationFit, Chart, ChartPlacement, ChartReference, Clipped, Finding, Limits, Mirrored, Skewed, Wide
from blender.imaging.color.luts import LutOutput
from blender.imaging.color.processors import converted, DisplayView
from blender.imaging.files.images import ImageFile
from blender.imaging.geometry.frames import Homography, Quad
from blender.imaging.geometry.panoramas import PanoramaView
from blender.imaging.geometry.stmaps import StMap
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired
from blender.records.identity import content_hash

from ..catalog import chain
from ..effects.grade.primaries import Primaries

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------


class Target(Enum):
    """Colors samples map onto: published chart values in a config colorspace, or a second image's samples."""

    REFERENCE = auto()
    IMAGE = auto()


class FitSide(Enum):
    """Side of the fitted pair a calibration writes."""

    FORWARD = auto()
    INVERSE = auto()


class Mirror(Enum):
    """Patch order mirror axis, valued by its corner permutation in `Quad` corner order."""

    ACROSS = (1, 0, 3, 2)
    UPRIGHT = (3, 2, 1, 0)


class Need(StrEnum):
    """Chart calibration requirement, each value its poll message over the subject lacking it."""

    PLACED = "Requires {} chart corners bounding a convex quad in view"
    SAMPLED = "Requires every {} patch sampled"
    CHOSEN = "Requires {} image with sampled chart"
    MATCHED = "Requires target on source chart {}"
    LIT = "Requires lit neutral patch in {} and target"

    def unmet(self, subject: str) -> Faults:
        """Returns the unmet fault of subject."""
        return (Fault(reason=BlenderReason.UNMET, subject=self.format(subject)),)


# --- [CONSTANTS] ------------------------------------------------------------------------

SPACES: Final = tuple((item.identifier, item.name, item.description) for item in Descriptor.declared(bpy.types.ColorManagedInputColorspaceSettings, "name", bpy.types.EnumProperty).prop.enum_items)
CHARTS, TARGETS, SIDES, MIRRORS = (tuple((member.name, bpy.path.display_name(member.name.lower()), "") for member in kind) for kind in (Chart, Target, FitSide, Mirror))

# --- [MODELS] ---------------------------------------------------------------------------


class PlacedChart(msgspec.Struct, frozen=True, kw_only=True):
    """Chart placement on an image and its scene-linear patch means in grid order."""

    placement: ChartPlacement = ChartPlacement()
    samples: tuple[tuple[float, float, float], ...] = ()

    @property
    def sampled(self) -> Option[tuple[tuple[float, float, float], ...]]:
        """Samples when every printed patch holds one."""
        return Some(self.samples).filter(lambda held: len(held) == len(ChartReference.of(self.placement.chart).checker.data))


class ChartImage(bpy.types.PropertyGroup):
    """Chart over an image, drawn while shown, its placement and samples stored on the group."""

    shown: bpy.props.BoolProperty(name="Show Chart", description="Draw chart and handles over image", default=True)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Charting(bpy.types.PropertyGroup):
    """Calibration chart, normalization, clamp, and overlay opacity."""

    chart: bpy.props.EnumProperty(name="Chart", description="Placed chart", items=CHARTS)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    normalized: bpy.props.BoolProperty(name="Normalize", description="Keep source neutral luma, gain held as exposure", default=True)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    clamped: bpy.props.BoolProperty(name="Clamp", description="Clip exported values to [0, 1]")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    opacity: bpy.props.FloatProperty(name="Opacity", description="Patch fill opacity", subtype="FACTOR", min=0.0, max=1.0, default=0.5)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Calibrating(bpy.types.PropertyGroup):
    """Scene's calibration pairing and fit choices, its image pointers polled for sampled charts at registration."""

    source: bpy.props.PointerProperty(type=bpy.types.Image, name="Source", description="Sampled source image")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    target: bpy.props.EnumProperty(name="Target", description="Colors samples map onto", items=TARGETS)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    space: bpy.props.EnumProperty(  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
        name="Space", description="Reference value colorspace", items=SPACES, default=PyOpenColorIO.GetCurrentConfig().getColorSpace(PyOpenColorIO.ROLE_SCENE_LINEAR).getName()
    )
    image: bpy.props.PointerProperty(type=bpy.types.Image, name="Image", description="Image source samples map onto")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    side: bpy.props.EnumProperty(name="Direction", description="Source to target or back", items=SIDES)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    gain: bpy.props.BoolProperty(name="Keep Gain", description="Fold exposure gain into transform")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    charting: bpy.props.PointerProperty(type=Charting, name="Options", description="Chart, normalization, clamp, and opacity")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class ChartFraming(bpy.types.PropertyGroup):
    """Share of the view width a framed chart spans."""

    share: bpy.props.FloatProperty(name="Share", description="View width chart spans", subtype="FACTOR", min=0.01, max=1.0, default=0.5)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Flipping(bpy.types.PropertyGroup):
    """Flip axis."""

    mirror: bpy.props.EnumProperty(name="Mirror", description="Patch order mirror axis", items=MIRRORS)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class ChartBatches(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Chart fill batch and colored outline batches."""

    fills: gpu.types.GPUBatch
    edges: tuple[tuple[gpu.types.GPUBatch, tuple[float, float, float, float]], ...]


class _Commands(msgspec.Struct, frozen=True, kw_only=True):
    """Chart calibration commands, one field per button."""

    place: Operator[Empty]
    sample: Operator[Empty]
    clear: Operator[Empty]
    flip: Operator[Flipping]
    turn: Operator[Empty]
    center: Operator[Empty]
    reset: Operator[Empty]
    frame: Operator[ChartFraming]
    leave: Operator[Empty]
    grade: Operator[Empty]
    node: Operator[Empty]
    bake: Operator[Baking]
    write: Operator[Writing]


# --- [SERVICES] -------------------------------------------------------------------------


class Patches(msgspec.Struct, frozen=True, kw_only=True):
    """Chart overlay batching formed patch fills and reference outlines per refresh and drawing the shown chart."""

    identity: Identity

    def read(self, _metrics: Metrics, /) -> Outcome[Option[dict[Handle[bpy.types.Image], ChartBatches]]]:
        """Batches formed patch fills and outlines in frame fractions per charted image."""
        fills, lines = gpu.shader.from_builtin("FLAT_COLOR"), gpu.shader.from_builtin("POLYLINE_UNIFORM_COLOR")
        config = PyOpenColorIO.GetCurrentConfig()

        def formed(scene: bpy.types.Scene, image: bpy.types.Image) -> Outcome[Option[Callable[[npt.NDArray[np.float32]], Outcome[npt.NDArray[np.float32]]]]]:
            match image.type:
                case "RENDER_RESULT":
                    return chain.forming(self.identity, scene, Enabled.RENDER).map(lambda formed: formed.map(lambda transform: partial(converted, config, transform)))
                case _ if image.use_view_as_render:
                    return Ok(Some(partial(viewed(scene).form, config)))
                case _:
                    device = viewed(scene).display
                    return Ok(Some(partial(DisplayView(display=device, view=config.getDefaultView(device)).form, config)))

        def patched(scene: bpy.types.Scene, image: bpy.types.Image, record: PlacedChart, opacity: float) -> Outcome[Option[tuple[Handle[bpy.types.Image], ChartBatches]]]:
            references = ChartReference.of(record.placement.chart).values(config)

            def drawn(form: Callable[[npt.NDArray[np.float32]], Outcome[npt.NDArray[np.float32]]], corners: npt.NDArray[np.float32]) -> Outcome[ChartBatches]:
                return Result[tuple[tuple[float, float, float], ...], Faults].bind(
                    references,
                    lambda values: paired(form(np.array(record.sampled.default_value(values), dtype=np.float32)), form(np.array(values, dtype=np.float32))).map(
                        lambda pair: ChartBatches(
                            fills=batch_for_shader(
                                fills,
                                "TRIS",
                                {
                                    "pos": corners[:, (0, 1, 2, 0, 2, 3)].reshape(-1, 2).tolist(),
                                    "color": np.repeat(np.concatenate((pair[0], np.full((len(pair[0]), 1), opacity, np.float32)), axis=-1), 6, axis=0).tolist(),
                                },
                            ),
                            edges=tuple(
                                (batch_for_shader(lines, "LINE_STRIP", {"pos": quad[(0, 1, 2, 3, 0), :].tolist()}), (float(edge[0]), float(edge[1]), float(edge[2]), float(edge[3])))
                                for quad, edge in zip(np.unstack(corners), np.unstack(np.concatenate((pair[1], np.ones((len(pair[1]), 1), np.float32)), axis=-1)))
                            ),
                        )
                    ),
                )

            return formed(scene, image).bind(
                lambda form: Result[Option[Homography], Faults].bind(
                    record.placement.aligned,
                    lambda aligned: (
                        form
                        .bind(lambda found: aligned.map(lambda homography: (found, homography.to_image(record.placement.footprints)[..., :2].astype(np.float32))))
                        .map(lambda pair: drawn(*pair).map(lambda built: Some((Handle.of(image), built))))
                        .default_value(Ok(Nothing))
                    ),
                )
            )

        def charted_images(scene: bpy.types.Scene, chart: Attached[ChartImage], held: Attached[Calibrating]) -> Outcome[Option[dict[Handle[bpy.types.Image], ChartBatches]]]:
            opacity = held.struct(scene).charting.opacity

            def each(image: bpy.types.Image) -> Outcome[Option[tuple[Handle[bpy.types.Image], ChartBatches]]]:
                return ChartCalibration.placed.read(chart.struct(image)).bind(lambda found: found.map(lambda record: patched(scene, image, record, opacity)).default_value(Ok(Nothing)))

            return accumulated(each(image) for image in bpy.data.images if chart.stored(image)).map(lambda found: Some({key: value for entry in found for key, value in entry.to_list()}).filter(bool))

        return (
            root_in(bpy.types.Scene, bpy.context)
            .map(lambda scene: paired(ChartCalibration.charts.bound(self.identity), ChartCalibration.calibrations.bound(self.identity)).bind(lambda found: charted_images(scene, *found)))
            .default_value(Ok(Nothing))
        )

    def drawn(self, canvas: Canvas, state: dict[Handle[bpy.types.Image], ChartBatches]) -> None:
        """Draws the shown chart's patches scaled through the frame."""
        reports = self.identity.reports
        for chart in reports.reported(ChartCalibration.charts.bound)(self.identity).to_list():
            for shown, frame in (
                (shown, framed(region))
                for shown in reports.reported(charted)(bpy.context, self.identity, chart).bind(lambda found: found).bind(lambda pair: Option.of_optional(state.get(Handle.of(pair[0])))).to_list()
                for region in canvas.region.to_list()
            ):
                gpu.matrix.translate((frame.x, frame.y))
                gpu.matrix.scale((frame.width, frame.height))
                shown.fills.draw(gpu.shader.from_builtin("FLAT_COLOR"))
                lines = canvas.lines(1.0)
                for batch, ink in shown.edges:
                    lines.uniform_float("color", ink)
                    batch.draw(lines)


class ChartCalibration(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Chart placed, sampled, and fitted on images, flipped by glyph buttons, its fit warned past `ceiling` condition."""

    across: Glyph
    upright: Glyph
    ceiling: float = 1e4
    seed: Option[Callable[[Identity], Outcome[Snapshot]]] = Nothing
    charts: ClassVar[Attachment[ChartImage]] = Attachment(key="chart", form=ChartImage, owner=bpy.types.Image)
    calibrations: ClassVar[Attachment[Calibrating]] = Attachment(key="calibration", form=Calibrating, owner=bpy.types.Scene)
    placed: ClassVar[Stored[PlacedChart]] = Stored(name="placed", decoder=msgspec.msgpack.Decoder(PlacedChart))
    views: ClassVar[Ownership[bpy.types.Image]] = Ownership(kind=bpy.types.Image, key="chart_view", saved=Saved.USED)
    imaged: ClassVar[Setting] = image_type(("IMAGE", "RENDER_RESULT"))
    shader: ClassVar[AllOf] = AllOf(
        conditions=(
            Setting(descriptor=Descriptor.declared(bpy.types.SpaceNodeEditor, "tree_type", bpy.types.EnumProperty), accepted=(bpy.types.ShaderNodeTree.__name__,)),
            Setting(descriptor=Descriptor.declared(bpy.types.SpaceNodeEditor, "edit_tree.is_editable", bpy.types.BoolProperty)),
        )
    )

    @property
    def prerequisites(self) -> tuple[Registrable, ...]:
        """Chart attachment."""
        return (self.charts,)

    # --- [FITS]

    def conditioned(self, fit: CalibrationFit) -> tuple[tuple["rna_enums.WmReportItems", str], ...]:
        """Warns of a fit conditioned past the ceiling."""
        return (("WARNING", f"Fitted transform poorly conditioned ({fit.condition:.2e}), check chart alignment"),) if fit.condition > self.ceiling else ()

    @staticmethod
    def fitting[T: bpy.types.PropertyGroup](call: Call[T]) -> Outcome[tuple[Calibrating, Calibration, CalibrationFit]]:
        """Returns the scene's calibration choices with the calibration they fit and its fit."""
        return paired(ChartCalibration.charts.bound(call.identity), call.root(bpy.types.Scene).bind(lambda scene: ChartCalibration.calibrations.read(call.identity, scene))).bind(
            lambda found: calibrated(PyOpenColorIO.GetCurrentConfig(), *found).map(lambda fit: (found[1], *fit))
        )

    # --- [COMMANDS]

    def apply_calibration(self, call: Call[Empty]) -> Outcome[Answer]:
        """Write calibration into scene primaries."""
        return paired(ChartCalibration.fitting(call), call.root(bpy.types.Scene)).bind(
            lambda pair: chain.rows.assigned(call.identity, pair[1], Primaries()).bind(
                lambda entry: entered(call.identity, entry, {"remap": "Mixer", **dict(zip(("mixer_red", "mixer_green", "mixer_blue"), pair[0][1].gained, strict=True))}).map(
                    lambda _: Done(notes=(*self.conditioned(pair[0][2]), ("INFO", f"Calibrated {entry.name}")))
                )
            )
        )

    def insert_calibration(self, call: Call[Empty]) -> Outcome[Answer]:
        """Insert calibration matrix after active node."""

        def spliced(chosen: Chosen, made: tuple[Calibrating, Calibration, CalibrationFit]) -> Outcome[Answer]:
            calibration = made[1]
            return (
                matrix()
                .bind(
                    lambda group: inserted(
                        chosen, group, call.identity, inputs={**dict(zip(("red", "green", "blue"), map(tuple, calibration.matrix.tolist()), strict=True)), "gain": math.exp2(calibration.exposure)}
                    )
                )
                .map(lambda _: Done(notes=(*self.conditioned(made[2]), ("INFO", "Inserted calibration matrix"))))
            )

        return ChartCalibration.fitting(call).bind(lambda made: Chosen.of(call.context).bind(lambda chosen: spliced(chosen, made)))

    def filed[T: Baking | Writing](self, written: LutOutput, call: Call[T]) -> Outcome[Answer]:
        """Writes forward and inverse calibration files through an output, clamped to the unit range when the scene asks."""

        def exported(chosen: Calibrating, calibration: Calibration) -> Outcome[tuple[str, ...]]:
            clamp = (PyOpenColorIO.RangeTransform(minInValue=0.0, maxInValue=1.0, minOutValue=0.0, maxOutValue=1.0),) if chosen.charting.clamped else ()
            return written.written({
                str(destination): PyOpenColorIO.GroupTransform(
                    transforms=[
                        PyOpenColorIO.GroupTransform([calibration.transform], direction=PyOpenColorIO.TRANSFORM_DIR_FORWARD if each is FitSide[chosen.side] else PyOpenColorIO.TRANSFORM_DIR_INVERSE),
                        *clamp,
                    ]
                )
                for each, destination in zip(FitSide, call.paths, strict=True)
            })

        return ChartCalibration.fitting(call).bind(lambda made: exported(made[0], made[1]).map(lambda paths: Done(notes=(*self.conditioned(made[2]), *(("INFO", f"Wrote {path}") for path in paths)))))

    @cached_property
    def commands(self) -> _Commands:
        """Calibration commands, each body binding the calling extension."""

        def located[T: bpy.types.PropertyGroup](call: Call[T]) -> Outcome[tuple[bpy.types.Image, PlacedChart, Attached[ChartImage]]]:
            return ChartCalibration.charts.bound(call.identity).bind(
                lambda chart: charted(call.context, call.identity, chart).bind(
                    lambda found: found.map(lambda pair: (*pair, chart)).to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=repr(call.context.area), member=chart.name),))
                )
            )

        def aspect(context: bpy.types.Context, image: bpy.types.Image) -> Outcome[float]:
            match image.type:
                case "RENDER_RESULT":
                    return (
                        rooted(bpy.types.Scene, context).map(lambda scene: scene.render).map(lambda render: render.resolution_x * render.pixel_aspect_x / (render.resolution_y * render.pixel_aspect_y))
                    )
                case _:
                    width, height = np.asarray(image.size)
                    return Ok(float(width / height))

        def seated(chart: Chart, ratio: float) -> ChartPlacement:
            columns, rows = ChartReference.of(chart).shape
            corners = np.array(((-1.0, 1.0), (1.0, 1.0), (1.0, -1.0), (-1.0, -1.0))) * (0.25, 0.25 * ratio * rows / columns) + 0.5
            return ChartPlacement(chart=chart, quad=Quad.of(corners))

        def planed(context: bpy.types.Context, image: bpy.types.Image, config: PyOpenColorIO.Config) -> Outcome[npt.NDArray[np.float32]]:

            def linear(found: ImageFile[np.float32]) -> Outcome[npt.NDArray[np.float32]]:
                return converted(config, PyOpenColorIO.ColorSpaceTransform(src=found.colorspace, dst=PyOpenColorIO.ROLE_SCENE_LINEAR), np.ascontiguousarray(found.pixels)).map(
                    lambda plane: plane if plane.shape[2] == 4 else np.concatenate((plane, np.ones((*plane.shape[:2], 1), np.float32)), axis=-1)
                )

            match image.type:
                case "RENDER_RESULT":
                    return paired(rooted(bpy.types.Scene, context), rooted(bpy.types.ViewLayer, context)).bind(
                        lambda pair: saved(pair[0]).bind(lambda path: combined(path, Some(pair[1].name))).map(lambda found: found[1])
                    )
                case _:
                    return read(image, config).bind(linear)

        def warned(layout: bpy.types.UILayout, context: bpy.types.Context, _properties: bpy.types.OperatorProperties, identity: Identity) -> None:

            def described(finding: Finding, reference: ChartReference) -> str:
                match finding:
                    case Mirrored():
                        return "Chart reads mirrored"
                    case Skewed(narrowest=narrowest, widest=widest):
                        return f"Corner angles run {math.degrees(narrowest):.0f} to {math.degrees(widest):.0f} degrees"
                    case Wide(field_of_view=field_of_view):
                        return f"View spans {math.degrees(field_of_view):.0f} degrees"
                    case Clipped(patches=patches):
                        names = tuple(reference.checker.data)
                        return f"Patches {', '.join(names[index] for index in patches)} leave image"

            found = (
                ChartCalibration.charts
                .bound(identity)
                .bind(lambda chart: charted(context, identity, chart))
                .bind(lambda found: found.to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=repr(context.area), member=ChartCalibration.charts.key),)))
                .bind(
                    lambda pair: aspect(context, pair[0]).bind(
                        lambda ratio: (
                            pair[1]
                            .placement.findings(ratio, Limits())
                            .map(
                                lambda listed: listed.map(
                                    lambda held: ", ".join(described(finding, ChartReference.of(pair[1].placement.chart)) for finding in held) or "No placement finding"
                                ).default_value(Need.PLACED.format(pair[0].name))
                            )
                        )
                    )
                )
            )
            for text in found.to_option().to_list():
                layout.label(text=text, icon="ERROR")
            alerted(layout, found)

        def edited[T: bpy.types.PropertyGroup](change: Callable[[Call[T], bpy.types.Image, PlacedChart], Outcome[PlacedChart]], call: Call[T]) -> Outcome[Answer]:

            def kept(image: bpy.types.Image, record: PlacedChart, chart: Attached[ChartImage]) -> Outcome[Answer]:
                return change(call, image, record).bind(
                    lambda revised: Ok(Unchanged(reason=f"{image.name} chart unchanged")) if revised == record else ChartCalibration.placed.write(chart.struct(image), revised).map(lambda _: Done())
                )

            return located(call).bind(lambda found: kept(*found))

        def moved[T: bpy.types.PropertyGroup](move: Callable[[Call[T], npt.NDArray[np.float64]], Outcome[npt.NDArray[np.float64]]], call: Call[T]) -> Outcome[Answer]:
            return edited(
                lambda each, image, record: Result[Option[Homography], Faults].bind(
                    record.placement.aligned,
                    lambda aligned: (
                        aligned
                        .to_result_with(lambda: Need.PLACED.unmet(image.name))
                        .bind(lambda homography: move(each, homography.quad.samples))
                        .map(lambda samples: msgspec.structs.replace(record, placement=record.placement.realigned(Quad.of(samples))))
                    ),
                ),
                call,
            )

        def clear_samples(_call: Call[Empty], _image: bpy.types.Image, record: PlacedChart) -> Outcome[PlacedChart]:
            """Remove patch samples."""
            return Ok(msgspec.structs.replace(record, samples=()))

        def reset_chart(call: Call[Empty], image: bpy.types.Image, record: PlacedChart) -> Outcome[PlacedChart]:
            """Reset chart layout."""
            return aspect(call.context, image).map(lambda ratio: msgspec.structs.replace(record, placement=seated(record.placement.chart, ratio)))

        def leave_view(call: Call[Empty], image: bpy.types.Image, record: PlacedChart) -> Outcome[PlacedChart]:
            """Show panorama holding chart."""
            return (
                rooted(bpy.types.SpaceImageEditor, call.context)
                .bind(lambda space: written(space, "image", image))
                .map(lambda _: msgspec.structs.replace(record, placement=msgspec.structs.replace(record.placement, panorama=None)))
            )

        def flip_chart(call: Call[Flipping], samples: npt.NDArray[np.float64]) -> Outcome[npt.NDArray[np.float64]]:
            """Mirror patch order inside chart corners."""
            return Ok(samples[list(Mirror[call.properties.mirror].value)])

        def turn_chart(_call: Call[Empty], samples: npt.NDArray[np.float64]) -> Outcome[npt.NDArray[np.float64]]:
            """Rotate patch order a quarter turn."""
            return Ok(np.roll(samples, 1, axis=0))

        def center_chart(call: Call[Empty], samples: npt.NDArray[np.float64]) -> Outcome[npt.NDArray[np.float64]]:
            """Center chart in visible frame."""

            def middle(region: bpy.types.Region) -> npt.NDArray[np.float64]:
                frame = framed(region)
                return np.array(((region.width / 2 - frame.x) / frame.width, (region.height / 2 - frame.y) / frame.height, 1.0), np.float64)

            return call.root(bpy.types.Region).map(lambda region: samples + (middle(region) - samples.mean(axis=0)) * (1.0, 1.0, 0.0))

        def place_chart(call: Call[Empty]) -> Outcome[Answer]:
            """Place a square-cell chart at shown image center."""
            image: Outcome[bpy.types.Image] = rooted(bpy.types.SpaceImageEditor, call.context).bind(
                lambda space: Option.of_optional(space.image).to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=repr(space), member="image"),))
            )
            return (
                paired(image, ChartCalibration.charts.bound(call.identity), call.root(bpy.types.Scene).bind(lambda scene: ChartCalibration.calibrations.read(call.identity, scene)))
                .bind(
                    lambda found: aspect(call.context, found[0]).bind(
                        lambda ratio: ChartCalibration.placed.write(found[1].struct(found[0]), PlacedChart(placement=seated(Chart[found[2].charting.chart], ratio)))
                    )
                )
                .map(lambda _: Done())
            )

        def sample_chart(call: Call[Empty]) -> Outcome[Answer]:
            """Measure each patch's mean color."""

            def kept(image: bpy.types.Image, record: PlacedChart, chart: Attached[ChartImage], plane: npt.NDArray[np.float32]) -> Outcome[None]:
                return (
                    Result[Option[tuple[tuple[float, float, float], ...]], Faults]
                    .bind(record.placement.means(plane), lambda found: found.to_result_with(lambda: Need.PLACED.unmet(image.name)))
                    .bind(lambda samples: ChartCalibration.placed.write(chart.struct(image), msgspec.structs.replace(record, samples=samples)))
                    .bind(lambda _: call.root(bpy.types.Scene).bind(lambda scene: ChartCalibration.calibrations.read(call.identity, scene)).bind(lambda chosen: written(chosen, "source", image)))
                    .map(lambda _: None)
                )

            return located(call).bind(
                lambda found: (
                    planed(call.context, found[0], PyOpenColorIO.GetCurrentConfig()).bind(lambda plane: kept(*found, plane)).map(lambda _: Done(notes=(("INFO", f"Sampled {found[0].name}"),)))
                )
            )

        def view_chart(call: Call[ChartFraming]) -> Outcome[Answer]:
            """Show panorama chart in a rectilinear view."""

            def baked(image: bpy.types.Image, record: PlacedChart, chart: Attached[ChartImage], placement: ChartPlacement, plane: npt.NDArray[np.float32]) -> Outcome[Answer]:
                view = Option.of_optional(placement.panorama).default_with(PanoramaView)
                width = max(1, math.ceil(plane.shape[1] * view.view.field_of_view / math.tau))
                height = max(1, math.ceil(width / view.view.aspect))

                def warped() -> Outcome[Option[npt.NDArray[np.float32]]]:
                    return StMap.bake(width, height, view.to_panorama, view.source.extension, source_size=(plane.shape[1], plane.shape[0])).warp(plane).map(Some)

                return (
                    generated(
                        ChartCalibration.views,
                        call.identity,
                        f"{image.name} View",
                        content_hash(PanoramaView, view, arrays=(plane,)),
                        warped,
                        Tag(space=Some(PyOpenColorIO.ROLE_SCENE_LINEAR)),
                        of=Some(image),
                    )
                    .bind(
                        lambda shown: accumulated(
                            paired(
                                ChartCalibration.placed.write(chart.struct(image), msgspec.structs.replace(record, placement=placement)),
                                rooted(bpy.types.SpaceImageEditor, call.context).bind(lambda space: written(space, "image", made)),
                            )
                            for made in shown.to_list()
                        )
                    )
                    .map(lambda _: Done(notes=(("INFO", f"Viewing {image.name} chart"),)))
                )

            return located(call).bind(
                lambda found: (
                    (Some(found[1].placement) if found[1].placement.panorama is not None else found[1].placement.framed(call.properties.share))
                    .to_result_with(lambda: Need.PLACED.unmet(found[0].name))
                    .bind(lambda placement: planed(call.context, found[0], PyOpenColorIO.GetCurrentConfig()).bind(lambda plane: baked(*found, placement, plane)))
                )
            )

        def bake_calibration_lut(call: Call[Baking]) -> Outcome[Answer]:
            """Bake forward and inverse calibration LUTs."""
            return self.filed(output(call.properties), call)

        def write_calibration(call: Call[Writing]) -> Outcome[Answer]:
            """Write forward and inverse calibration transform files."""
            return self.filed(LutOutput(format=FORMATS[call.properties.format]), call)

        def sided(path: Path, _properties: Baking | Writing) -> tuple[Path, ...]:
            return tuple(path.with_stem(f"{path.stem}_{side.name.lower()}") for side in FitSide)

        shown = Some(self.imaged)
        return _Commands(
            place=Operator(condition=shown, form=Empty, run=place_chart),
            sample=Operator(condition=shown, form=Empty, run=sample_chart, invoke=Some(Properties(confirm_text=Some("Sample"))), draw=Some(warned)),
            clear=Operator(condition=shown, form=Empty, run=update_wrapper(partial(edited, clear_samples), clear_samples), invoke=Some(Confirm(confirm_text=Some("Clear"), icon="WARNING"))),
            flip=Operator(condition=shown, form=Flipping, run=update_wrapper(partial(moved, flip_chart), flip_chart)),
            turn=Operator(icon=Some("FILE_REFRESH"), condition=shown, form=Empty, run=update_wrapper(partial(moved, turn_chart), turn_chart)),
            center=Operator(icon=Some("ALIGN_CENTER"), condition=shown, form=Empty, run=update_wrapper(partial(moved, center_chart), center_chart)),
            reset=Operator(icon=Some("LOOP_BACK"), condition=shown, form=Empty, run=update_wrapper(partial(edited, reset_chart), reset_chart)),
            frame=Operator(icon=Some("ZOOM_SELECTED"), condition=shown, form=ChartFraming, run=view_chart, invoke=Some(Properties())),
            leave=Operator(icon=Some("SCREEN_BACK"), condition=shown, form=Empty, run=update_wrapper(partial(edited, leave_view), leave_view)),
            grade=Operator(form=Empty, run=self.apply_calibration),
            node=Operator(condition=Some(self.shader), form=Empty, run=self.insert_calibration),
            bake=Operator(form=Baking, run=bake_calibration_lut, invoke=Some(Save[Baking](extensions=extension, destinations=Some(sided))), undo=False),
            write=Operator(form=Writing, run=write_calibration, invoke=Some(Save[Writing](extensions=extension, destinations=Some(sided))), undo=False),
        )

    # --- [REGISTRATION]

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Polls the calibration image pointers for sampled charts, then returns the commands, the free corner handles, the flip buttons, and the patch overlay."""
        refreshes = (Updated(handlers=bpy.app.handlers.depsgraph_update_post, ids=(bpy.types.Scene, bpy.types.Image)),)
        commands = self.commands

        def sampled(_owner: Struct, image: bpy.types.Image) -> bool:
            return (
                ChartCalibration.charts
                .bound(identity)
                .to_option()
                .filter(lambda chart: chart.stored(image))
                .bind(lambda chart: ChartCalibration.placed.read(chart.struct(image)).to_option())
                .bind(lambda found: found.bind(lambda record: record.sampled))
                .is_some()
            )

        def rows(chart: Attached[ChartImage]) -> tuple[Registrable, ...]:
            def access(context: bpy.types.Context) -> Outcome[Option[Access]]:
                def bound(pair: tuple[bpy.types.Image, PlacedChart]) -> Option[Access]:
                    image, record = pair
                    placement = record.placement
                    samples = Option.of_optional(placement.panorama).map(lambda panorama: panorama.to_view(placement.quad.samples)).default_value(placement.quad.samples)

                    def write(quad: Quad) -> Outcome[None]:
                        return ChartCalibration.placed.write(chart.struct(image), msgspec.structs.replace(record, placement=msgspec.structs.replace(placement, quad=placement.realigned(quad).quad)))

                    return Some(samples).filter(lambda corners: bool(np.all(corners[:, 2] > 0.0))).map(lambda corners: Access(quad=Quad.of(corners), write=write))

                return charted(context, identity, chart).map(lambda found: found.bind(bound))

            def place(edge: tuple[int, int], context: bpy.types.Context) -> Outcome[Option[Matrix]]:
                return access(context).map(
                    lambda found: found.bind(
                        lambda held: Option.of_optional(context.region).map(framed).map(lambda frame: held.quad.samples[list(edge), :2].mean(axis=0) * (frame.width, frame.height) + (frame.x, frame.y))
                    ).map(lambda at: Matrix.Translation((*at, 0.0)))
                )

            flips = tuple(
                Button(place=partial(place, edge), glyph=glyph, press=Own(command=commands.flip, values={"mirror": mirror.name}), scale_basis=HANDLE, draw_options=frozenset({"OUTLINE", "BACKDROP"}))
                for edge, glyph, mirror in (((0, 1), self.across, Mirror.ACROSS), ((0, 3), self.upright, Mirror.UPRIGHT))
            )
            return (
                self.views,
                *msgspec.structs.astuple(commands),
                FrameRegion(key="chart_corners", access=access, shape=Free(), kinds=("IMAGE", "RENDER_RESULT"), refreshes=refreshes),
                Gizmos(key="chart", title=bpy.path.display_name("chart"), space="IMAGE_EDITOR", parts=flips, condition=Some(self.imaged)),
                overlaid(Patches(identity=identity), self.imaged, refreshes),
            )

        for name in ("source", "image"):
            Calibrating.__annotations__[name].keywords["poll"] = sampled
        return self.charts.bound(identity).map(rows)

    # --- [SECTIONS]

    def section(self) -> Single[Calibrating]:
        """Image editor calibration section: the shown image's chart switch, the scene's calibration fields, and the command buttons."""
        commands = self.commands
        buttons: tuple[Row, ...] = (
            Caption(rows=tuple(OperatorButton(target=command) for command in (commands.place, commands.sample, commands.clear)), flow=Flow.ALIGNED),
            Caption(
                rows=(
                    *(
                        OperatorButton(target=Own(command=commands.flip, values={"mirror": mirror.name}), text="", icon=Some(glyph))
                        for mirror, glyph in ((Mirror.ACROSS, self.across), (Mirror.UPRIGHT, self.upright))
                    ),
                    *(OperatorButton(target=command, text="") for command in (commands.turn, commands.center, commands.reset, commands.frame, commands.leave)),
                ),
                flow=Flow.ALIGNED,
            ),
            Caption(rows=(OperatorButton(target=commands.grade), OperatorButton(target=commands.bake), OperatorButton(target=commands.write)), flow=Flow.ALIGNED),
        )

        def rows(identity: Identity) -> Outcome[tuple[Row, ...]]:
            return self.charts.bound(identity).map(lambda chart: (Property(target=Member(root=bpy.types.SpaceImageEditor, path=f"image.{chart.name}.shown")), *buttons))

        return Single(
            key="chart_calibration",
            title=bpy.path.display_name("chart_calibration"),
            rows=(Rows(rows=rows),),
            store=Some(Hosted(store=Store(attachment=msgspec.structs.replace(self.calibrations, seed=self.seed)))),
        )

    def node_section(self) -> Single:
        """Node editor matrix splice section, inactive outside an editable shader tree."""

        def rows(identity: Identity) -> Outcome[tuple[Row, ...]]:
            return self.calibrations.bound(identity).map(
                lambda held: (*(Property(target=Member(root=bpy.types.Scene, path=f"{held.name}.{name}")) for name in ("source", "side")), OperatorButton(target=self.commands.node))
            )

        return Single(key="chart_calibration_node", title=bpy.path.display_name("chart_calibration"), rows=(Rows(rows=rows),), condition=Some(self.shader))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [READS]


def charted(context: bpy.types.Context, identity: Identity, chart: Attached[ChartImage]) -> Outcome[Option[tuple[bpy.types.Image, PlacedChart]]]:
    """Returns the shown image's chart, its panorama's chart for a view image, `Nothing` while the editor shows no charted image."""

    def held(image: bpy.types.Image, *, viewing: bool) -> Outcome[Option[tuple[bpy.types.Image, PlacedChart]]]:
        group = chart.struct(image)
        return (
            ChartCalibration.placed.read(group).map(lambda found: found.filter(lambda kept: group.shown and (kept.placement.panorama is not None) == viewing).map(lambda kept: (image, kept)))
            if chart.stored(image)
            else Ok(Nothing)
        )

    return (
        root_in(bpy.types.SpaceImageEditor, context)
        .bind(lambda space: Option.of_optional(space.image))
        .map(
            lambda shown: (
                Mark
                .read(identity, shown)
                .filter(lambda mark: mark.key == ChartCalibration.views.key)
                .bind(lambda mark: mark.of)
                .bind(lambda block: Some((block, True)) if isinstance(block, bpy.types.Image) else Nothing)
                .default_value((shown, False))
            )
        )
        .map(lambda pair: held(pair[0], viewing=pair[1]))
        .default_value(Ok(Nothing))
    )


# --- [FITS]


def calibrated(config: PyOpenColorIO.Config, chart: Attached[ChartImage], chosen: Calibrating) -> Outcome[tuple[Calibration, CalibrationFit]]:
    """Returns the chosen side's calibration, normalized when asked, with the fit it came from."""
    target, side, charting = Target[chosen.target], FitSide[chosen.side], chosen.charting

    def held(image: bpy.types.Image | None, role: str) -> Outcome[tuple[bpy.types.Image, PlacedChart]]:
        match image:
            case None:
                return Error(Need.CHOSEN.unmet(role))
            case bpy.types.Image() if chart.stored(image):
                return ChartCalibration.placed.read(chart.struct(image)).bind(lambda found: found.map(lambda record: (image, record)).to_result_with(lambda: Need.SAMPLED.unmet(image.name)))
            case bpy.types.Image():
                return Error(Need.SAMPLED.unmet(image.name))

    def samples(pair: tuple[bpy.types.Image, PlacedChart]) -> Outcome[tuple[tuple[float, float, float], ...]]:
        return pair[1].sampled.to_result_with(lambda: Need.SAMPLED.unmet(pair[0].name))

    def aimed(source: PlacedChart) -> Outcome[tuple[tuple[float, float, float], ...]]:
        match target:
            case Target.REFERENCE:
                return ChartReference.of(source.placement.chart).values(config, chosen.space)
            case Target.IMAGE:
                return (
                    held(chosen.image, "target")
                    .filter_with(lambda pair: pair[1].placement.chart is source.placement.chart, lambda pair: Need.MATCHED.unmet(f"{source.placement.chart.name}, got {pair[1].placement.chart.name}"))
                    .bind(samples)
                )

    def fitted(
        image: bpy.types.Image, record: PlacedChart, source: tuple[tuple[float, float, float], ...], goal: tuple[tuple[float, float, float], ...]
    ) -> Outcome[tuple[Calibration, CalibrationFit]]:
        neutral = ChartReference.of(record.placement.chart).neutral
        target_space = chosen.space if target is Target.REFERENCE else PyOpenColorIO.ROLE_SCENE_LINEAR

        def selected(fit: CalibrationFit) -> Outcome[Calibration]:
            match side:
                case FitSide.FORWARD:
                    calibration, first, last, source_space, destination_space = fit.forward, source[neutral], goal[neutral], PyOpenColorIO.ROLE_SCENE_LINEAR, target_space
                case FitSide.INVERSE:
                    calibration, first, last, source_space, destination_space = fit.inverse, goal[neutral], source[neutral], target_space, PyOpenColorIO.ROLE_SCENE_LINEAR
            normalized = calibration.normalized(first, last, config, source_space=source_space, target_space=destination_space) if charting.normalized else Ok(Some(calibration))
            return (
                Result[Option[Calibration], Faults]
                .bind(normalized, lambda found: found.to_result_with(lambda: Need.LIT.unmet(image.name)))
                .map(
                    lambda kept: (
                        kept
                        if chosen.gain
                        else msgspec.structs.replace(
                            kept, gained=msgspec.convert(kept.matrix.tolist(), type=tuple[tuple[float, float, float], tuple[float, float, float], tuple[float, float, float]]), exposure=0.0
                        )
                    )
                )
            )

        return Result[CalibrationFit, Faults].bind(Calibration.fit(tuple(zip(source, goal, strict=True))), lambda fit: selected(fit).map(lambda calibration: (calibration, fit)))

    return held(chosen.source, "source").bind(lambda pair: paired(samples(pair), aimed(pair[1])).bind(lambda rows: fitted(*pair, *rows)))


# --- [COMPOSITION] ----------------------------------------------------------------------

calibration: Final = ChartCalibration(across=Path("flip.svg"), upright=Path("flip-upright.svg"))
rows: Final = (calibration, Panel(section=calibration.section(), at=Joined.beside(IMAGE_PT_image_properties)), Panel(section=calibration.node_section(), at=Joined.beside(NODE_PT_active_node_generic)))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Calibrating", "ChartCalibration", "ChartImage", "PlacedChart", "rows"]
