"""Image editor chart calibration with grade, node, and file outputs."""

from collections.abc import Callable
from enum import StrEnum
from functools import cached_property, partial
import math
from pathlib import Path
from typing import Annotated, ClassVar, TYPE_CHECKING

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
import gpu
from gpu_extras.batch import batch_for_shader
from mathutils import Matrix
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.commands.operators import Call, Confirm, Done, Empty, Operator, Outcome, Own, Properties, Save, Unchanged
from blender.bpy.data.ids.images import generated, read, Tag
from blender.bpy.data.ids.ownership import Mark, Ownership, Saved
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.conditions import AllOf, Setting
from blender.bpy.data.rna.descriptors import Descriptor, root_in, rooted, written
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Registrable
from blender.bpy.faults import BlenderAbsentFault, BlenderFaults, BlenderResult, BlenderUnmetFault
from blender.bpy.nodes.editing import Chosen, inserted
from blender.bpy.nodes.placement import NodeSpacing
from blender.bpy.render.color.luts import Baking, Writing
from blender.bpy.render.effects.formation import viewed
from blender.bpy.render.effects.rows import Enabled
from blender.bpy.render.effects.stack import EffectStack
from blender.bpy.render.outputs.exports import combined, image_type, Source
from blender.bpy.render.outputs.results import saved
from blender.bpy.render.review.frames import Access, framed, FrameRegion, Free, HANDLE, overlaid
from blender.bpy.scene.materials import matrix, matrix_inputs
from blender.bpy.settings.attachments import Attached, Attachment, Host
from blender.bpy.settings.properties import Definition
from blender.bpy.ui.drawing.canvas import Canvas
from blender.bpy.ui.drawing.handlers import Updated
from blender.bpy.ui.layout.rows import alerted, Caption, Flow, joined, Line, Member, OperatorButton, placed, Property, Row, RowDraw, Rows
from blender.bpy.ui.layout.sections import fields, Nest, Single
from blender.bpy.ui.resources.icons import Glyph
from blender.bpy.ui.resources.theme import Metrics
from blender.bpy.ui.surfaces.gizmos import Button, Gizmos
from blender.imaging.calibration import Calibration, CalibrationFit, Chart, ChartPlacement, ChartReference, Clipped, Finding, Limits, Mirrored, Skewed, Wide
from blender.imaging.color.luts import LutOutput, written as lut_written
from blender.imaging.color.processors import converted, DisplayView
from blender.imaging.effects.grade.primaries import Mixer, Primaries
from blender.imaging.faults import accumulated, paired
from blender.imaging.files.images import ImageFile
from blender.imaging.geometry.homography import Quad
from blender.imaging.geometry.panoramas import PanoramaView
from blender.imaging.geometry.stmaps import StMap
from blender.imaging.hashing import content_hash
from blender.imaging.parameters import Case, Choice, ColorSpace, Rgb

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Target = Reference | Sampled


class Direction(Choice):
    """Fitted pair side."""

    FORWARD = 1, "Forward"
    INVERSE = 2, "Inverse"


class Mirror(Choice):
    """Patch order mirror axis."""

    ACROSS = 1, "Left to Right", "Mirror patch order left to right"
    UPRIGHT = 2, "Top to Bottom", "Mirror patch order top to bottom"

    @property
    def order(self) -> tuple[int, int, int, int]:
        """Corner permutation in `Quad` field order."""
        match self:
            case Mirror.ACROSS:
                return (1, 0, 3, 2)
            case Mirror.UPRIGHT:
                return (3, 2, 1, 0)


class Need(StrEnum):
    """Chart calibration requirement, each value its poll message over the subject lacking it."""

    PLACED = "Requires {} chart corners bounding a convex quad in view"
    SAMPLED = "Requires every {} patch sampled"
    CHOSEN = "Requires {} image with sampled chart"
    MATCHED = "Requires target on source chart {}"
    LIT = "Requires lit neutral patch in {} and target"

    def unmet(self, subject: str) -> BlenderFaults:
        """Returns the unmet fault of subject."""
        return (BlenderUnmetFault(requirement=ChartUnmet(need=self, subject=subject)),)


# --- [POINTERS] -------------------------------------------------------------------------

sampled_pointer = Definition[bpy.types.Image](poll=Some(lambda identity, _owner, image: sampled_image(identity, image)))


# --- [MODELS] ---------------------------------------------------------------------------


class Reference(Case, frozen=True, kw_only=True, tag=1):
    """Published chart values in a config colorspace."""

    title = "Reference"
    space: Annotated[ColorSpace, msgspec.Meta(description="Reference value colorspace")] = PyOpenColorIO.ROLE_SCENE_LINEAR


class Sampled(Case, frozen=True, kw_only=True, tag=2):
    """Second image's samples."""

    title = "Image"
    image: Annotated[bpy.types.Image | None, msgspec.Meta(description="Image source samples map onto"), sampled_pointer] = None


class ChartImage(msgspec.Struct, frozen=True, kw_only=True):
    """Chart placed on an image and its patch samples."""

    shown: Annotated[bool, msgspec.Meta(title="Show Chart", description="Draw chart and handles over image")] = True
    placement: Annotated[ChartPlacement, msgspec.Meta(title="Chart", description="Placement in image")] = ChartPlacement()
    samples: Annotated[tuple[Rgb[float], ...], msgspec.Meta(title="Samples", description="Scene-linear patch means in grid order")] = ()

    @property
    def sampled(self) -> Option[tuple[Rgb[float], ...]]:
        """Samples when every printed patch holds one."""
        return Some(self.samples).filter(lambda held: len(held) == len(ChartReference.of(self.placement.chart).checker.data))


class Charting(msgspec.Struct, frozen=True, kw_only=True):
    """Calibration chart, normalization, clamp, and overlay opacity."""

    chart: Annotated[Chart, msgspec.Meta(title="Chart", description="Placed chart")] = Chart.COLORCHECKER_CLASSIC
    normalized: Annotated[bool, msgspec.Meta(title="Normalize", description="Keep source neutral luma, gain held as exposure")] = True
    clamped: Annotated[bool, msgspec.Meta(title="Clamp", description="Clip exported values to [0, 1]")] = False
    opacity: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Opacity", description="Patch fill opacity")] = 0.5


class Calibrating(msgspec.Struct, frozen=True, kw_only=True):
    """Scene's calibration pairing and fit choices."""

    source: Annotated[bpy.types.Image | None, msgspec.Meta(title="Source", description="Sampled source image"), sampled_pointer] = None
    target: Annotated[Target, msgspec.Meta(title="Target", description="Colors samples map onto")] = Reference()
    direction: Annotated[Direction, msgspec.Meta(title="Direction", description="Source to target or back")] = Direction.FORWARD
    gain: Annotated[bool, msgspec.Meta(title="Keep Gain", description="Fold exposure gain into transform")] = False
    charting: Annotated[Charting, msgspec.Meta(title="Options", description="Chart, normalization, clamp, and opacity")] = Charting()


class Framing(msgspec.Struct, frozen=True, kw_only=True):
    """Framed chart view share."""

    share: Annotated[float, msgspec.Meta(gt=0.0, le=1.0, title="Share", description="View width chart spans")] = 0.5


class Flipping(msgspec.Struct, frozen=True, kw_only=True):
    """Flip axis."""

    mirror: Annotated[Mirror, msgspec.Meta(title="Mirror", description="Patch order mirror axis")] = Mirror.ACROSS


class Drawn(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Chart fill batch and colored outline batches."""

    fills: gpu.types.GPUBatch
    edges: tuple[tuple[gpu.types.GPUBatch, tuple[float, float, float, float]], ...]


class Patches(msgspec.Struct, frozen=True, kw_only=True):
    """Chart overlay batching patches per refresh and drawing shown chart."""

    stack: EffectStack
    identity: Identity

    def read(self, _metrics: Metrics, /) -> BlenderResult[Option[dict[Handle[bpy.types.Image], Drawn]]]:
        """Batches formed patch fills and outlines in frame fractions per charted image."""
        fills, lines = gpu.shader.from_builtin("FLAT_COLOR"), gpu.shader.from_builtin("POLYLINE_UNIFORM_COLOR")

        def formed(scene: bpy.types.Scene, config: PyOpenColorIO.Config, image: bpy.types.Image) -> BlenderResult[Option[Callable[[npt.NDArray[np.float32]], BlenderResult[npt.NDArray[np.float32]]]]]:
            match image.type:
                case "RENDER_RESULT":
                    return self.stack.forming(self.identity, scene, config, Enabled.RENDER).map(lambda chain: chain.map(lambda transform: partial(converted, config, transform)))
                case _ if image.use_view_as_render:
                    exposure, gamma = scene.view_settings.exposure, scene.view_settings.gamma  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
                    return Ok(Some(lambda values: viewed(scene).form(config, values, exposure=exposure, gamma=gamma)))
                case _:
                    device = scene.display_settings.display_device  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
                    return Ok(Some(lambda values: DisplayView(display=device, view=config.getDefaultView(device)).form(config, values)))

        def patched(scene: bpy.types.Scene, config: PyOpenColorIO.Config, image: bpy.types.Image, record: ChartImage, opacity: float) -> BlenderResult[Option[tuple[Handle[bpy.types.Image], Drawn]]]:
            references = ChartReference.of(record.placement.chart).values(config)

            def drawn(form: Callable[[npt.NDArray[np.float32]], BlenderResult[npt.NDArray[np.float32]]], corners: npt.NDArray[np.float32]) -> BlenderResult[Drawn]:
                return Result[tuple[Rgb[float], ...], BlenderFaults].bind(
                    references,
                    lambda values: paired(form(np.array(record.sampled.default_value(values), dtype=np.float32)), form(np.array(values, dtype=np.float32))).map(
                        lambda pair: Drawn(
                            fills=batch_for_shader(
                                fills,
                                "TRIS",
                                {  # ty: ignore[invalid-argument-type]
                                    "pos": corners[:, (0, 1, 2, 0, 2, 3)].reshape(-1, 2),  # type: ignore[dict-item]
                                    "color": np.repeat(np.concatenate((pair[0], np.full((len(pair[0]), 1), opacity, np.float32)), axis=-1), 6, axis=0),  # type: ignore[dict-item]
                                },
                            ),
                            edges=tuple(
                                (batch_for_shader(lines, "LINE_STRIP", {"pos": quad[(0, 1, 2, 3, 0), :]}), (float(edge[0]), float(edge[1]), float(edge[2]), float(edge[3])))  # type: ignore[dict-item]  # ty: ignore[invalid-argument-type]
                                for quad, edge in zip(np.unstack(corners), np.unstack(np.concatenate((pair[1], np.ones((len(pair[1]), 1), np.float32)), axis=-1)))
                            ),
                        )
                    ),
                )

            return formed(scene, config, image).bind(
                lambda form: (
                    form
                    .bind(lambda found: record.placement.aligned.map(lambda homography: (found, homography.to_image(record.placement.footprints)[..., :2].astype(np.float32))))
                    .map(lambda pair: drawn(*pair).map(lambda built: Some((Handle.of(image), built))))
                    .default_value(Ok(Nothing))
                )
            )

        def charts_of(scene: bpy.types.Scene, config: PyOpenColorIO.Config, chart: Attached[ChartImage], held: Attached[Calibrating]) -> BlenderResult[Option[dict[Handle[bpy.types.Image], Drawn]]]:
            return (
                held
                .read(scene)
                .bind(
                    lambda calibrating: accumulated(
                        chart.read(image).bind(partial(patched, scene, config, image, opacity=calibrating.charting.opacity)) for image in bpy.data.images if chart.stored(image)
                    )
                )
                .map(lambda found: Some({key: value for entry in found for key, value in entry.to_list()}).filter(bool))
            )

        return (
            root_in(bpy.types.Scene, bpy.context)
            .map(lambda scene: paired(self.stack.config(), charts.bound(self.identity), calibrations.bound(self.identity)).bind(lambda found: charts_of(scene, *found)))
            .default_value(Ok(Nothing))
        )

    def drawn(self, canvas: Canvas, state: dict[Handle[bpy.types.Image], Drawn]) -> None:
        """Draws shown chart's patches scaled through frame."""
        context = bpy.context
        for chart in self.identity.reports.reported(charts.bound)(self.identity).to_list():
            for shown, frame in (
                (shown, framed(region))
                for shown in self.identity.reports
                .reported(charted)(context, self.identity, chart)
                .bind(lambda found: found)
                .bind(lambda pair: Option.of_optional(state.get(Handle.of(pair[0]))))
                .to_list()
                for region in canvas.region.to_list()
            ):
                gpu.matrix.translate((frame.x, frame.y))
                gpu.matrix.scale((frame.width, frame.height))
                shown.fills.draw(gpu.shader.from_builtin("FLAT_COLOR"))
                lines = canvas.lines(1.0)
                for batch, ink in shown.edges:
                    lines.uniform_float("color", ink)
                    batch.draw(lines)


class ChartCommands(msgspec.Struct, frozen=True, kw_only=True):
    """Chart calibration operator rows."""

    place: Operator[Empty]
    sample: Operator[Empty]
    clear: Operator[Empty]
    flip: Operator[Flipping]
    turn: Operator[Empty]
    center: Operator[Empty]
    reset: Operator[Empty]
    frame: Operator[Framing]
    leave: Operator[Empty]
    grade: Operator[Empty]
    node: Operator[Empty]
    bake: Operator[Baking]
    write: Operator[Writing]


class ChartCalibration(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Row placing chart calibration declarations and sections."""

    imaged: ClassVar[Setting] = image_type(("IMAGE", "RENDER_RESULT"))
    shader: ClassVar[AllOf] = AllOf(
        conditions=(
            Setting(descriptor=Descriptor.declared(bpy.types.SpaceNodeEditor, "tree_type", bpy.types.EnumProperty), accepted=(bpy.types.ShaderNodeTree.__name__,)),
            Setting(descriptor=Descriptor.declared(bpy.types.SpaceNodeEditor, "edit_tree.is_editable", bpy.types.BoolProperty)),
        )
    )
    stack: EffectStack
    across: Glyph
    upright: Glyph
    seed: Option[Callable[[Identity], BlenderResult[Calibrating]]] = Nothing

    @property
    def prerequisites(self) -> tuple[Registrable, ...]:
        """Chart and calibration attachments, calibrations carrying the seed."""
        return (charts, msgspec.structs.replace(calibrations, seed=self.seed))

    @cached_property
    def commands(self) -> ChartCommands:
        """Calibration operators, each body binding the calling extension."""

        def located(identity: Identity, context: bpy.types.Context) -> BlenderResult[tuple[bpy.types.Image, ChartImage, Attached[ChartImage]]]:
            return charts.bound(identity).bind(
                lambda chart: charted(context, identity, chart).bind(
                    lambda found: found.map(lambda pair: (*pair, chart)).to_result_with(lambda: (BlenderAbsentFault(owner=repr(context.area), key=chart.name),))
                )
            )

        def aspect(context: bpy.types.Context, image: bpy.types.Image) -> BlenderResult[float]:
            match image.type:
                case "RENDER_RESULT":
                    return (
                        rooted(bpy.types.Scene, context).map(lambda scene: scene.render).map(lambda render: render.resolution_x * render.pixel_aspect_x / (render.resolution_y * render.pixel_aspect_y))
                    )
                case _:
                    return Ok(image.size[0] / image.size[1])  # type: ignore[index]

        def seated(chart: Chart, ratio: float) -> ChartPlacement:
            columns, rows = ChartReference.of(chart).shape
            corners = np.array(((-1.0, 1.0), (1.0, 1.0), (1.0, -1.0), (-1.0, -1.0))) * (0.25, 0.25 * ratio * rows / columns) + 0.5
            return ChartPlacement(chart=chart, quad=Quad.of(corners))

        def planed(context: bpy.types.Context, image: bpy.types.Image, config: PyOpenColorIO.Config) -> BlenderResult[npt.NDArray[np.float32]]:

            def linear(found: ImageFile[np.float32]) -> BlenderResult[npt.NDArray[np.float32]]:
                return converted(config, PyOpenColorIO.ColorSpaceTransform(src=found.colorspace, dst=PyOpenColorIO.ROLE_SCENE_LINEAR), np.ascontiguousarray(found.pixels)).map(
                    lambda plane: plane if plane.shape[2] == 4 else np.concatenate((plane, np.ones((*plane.shape[:2], 1), np.float32)), axis=-1)
                )

            match image.type:
                case "RENDER_RESULT":
                    return paired(rooted(bpy.types.Scene, context), rooted(bpy.types.ViewLayer, context)).bind(
                        lambda pair: saved(pair[0]).bind(lambda path: combined(path, Source.LAYER, Some(pair[1].name))).map(lambda found: found[1])
                    )
                case _:
                    return read(image, config).bind(linear)

        def findings(identity: Identity, context: bpy.types.Context) -> BlenderResult[str]:

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

            return located(identity, context).bind(
                lambda found: aspect(context, found[0]).map(
                    lambda ratio: (
                        found[1]
                        .placement.findings(ratio, Limits())
                        .map(lambda listed: ", ".join(described(finding, ChartReference.of(found[1].placement.chart)) for finding in listed) or "No placement finding")
                        .default_value(Need.PLACED.format(found[0].name))
                    )
                )
            )

        def warned(layout: bpy.types.UILayout, context: bpy.types.Context, _properties: bpy.types.OperatorProperties, identity: Identity) -> None:
            found = findings(identity, context)
            for text in found.to_option().to_list():
                layout.label(text=text, icon="ERROR")
            alerted(layout, found)

        def conditioned(fit: CalibrationFit[Calibration]) -> tuple[tuple["rna_enums.WmReportItems", str], ...]:
            ceiling = 1e4
            return (("WARNING", f"Fitted transform poorly conditioned ({fit.condition_number:.2e}), check chart alignment"),) if fit.condition_number > ceiling else ()

        def edited[T: msgspec.Struct](change: Callable[[Call[T], bpy.types.Image, ChartImage], BlenderResult[ChartImage]]) -> Callable[[Call[T]], BlenderResult[Outcome]]:
            def run(call: Call[T]) -> BlenderResult[Outcome]:
                def kept(image: bpy.types.Image, record: ChartImage, chart: Attached[ChartImage]) -> BlenderResult[Outcome]:
                    return change(call, image, record).bind(
                        lambda revised: Ok(Unchanged(reason=f"{image.name} chart unchanged")) if revised == record else chart.write(image, revised).map(lambda _: Done())
                    )

                return located(call.identity, call.context).bind(lambda found: kept(*found))

            return run

        def moved[T: msgspec.Struct](move: Callable[[Call[T], npt.NDArray[np.float64]], BlenderResult[npt.NDArray[np.float64]]]) -> Callable[[Call[T]], BlenderResult[Outcome]]:
            return edited(
                lambda call, image, record: (
                    record.placement.aligned
                    .to_result_with(lambda: Need.PLACED.unmet(image.name))
                    .bind(lambda homography: move(call, homography.quad.samples))
                    .map(lambda samples: msgspec.structs.replace(record, placement=record.placement.realigned(Quad.of(samples))))
                )
            )

        def centered(call: Call[Empty], samples: npt.NDArray[np.float64]) -> BlenderResult[npt.NDArray[np.float64]]:
            def middle(region: bpy.types.Region) -> npt.NDArray[np.float64]:
                frame = framed(region)
                return np.array(((region.width / 2 - frame.x) / frame.width, (region.height / 2 - frame.y) / frame.height, 1.0), np.float64)

            return call.root(bpy.types.Region).map(lambda region: samples + (middle(region) - samples.mean(axis=0)) * (1.0, 1.0, 0.0))

        def placing(call: Call[Empty]) -> BlenderResult[Done]:
            image: BlenderResult[bpy.types.Image] = rooted(bpy.types.SpaceImageEditor, call.context).bind(
                lambda space: Option.of_optional(space.image).to_result_with(lambda: (BlenderAbsentFault(owner=repr(space), key="image"),))
            )
            return (
                paired(image, call.root(bpy.types.Scene).bind(lambda scene: calibrations.read(call.identity, scene)))
                .bind(lambda pair: aspect(call.context, pair[0]).bind(lambda ratio: charts.revised(call.identity, pair[0], lambda _: Ok(ChartImage(placement=seated(pair[1].charting.chart, ratio))))))
                .map(lambda _: Done())
            )

        def sampling(call: Call[Empty]) -> BlenderResult[Done]:
            def kept(image: bpy.types.Image, record: ChartImage, plane: npt.NDArray[np.float32]) -> BlenderResult[None]:
                means: BlenderResult[Option[tuple[Rgb[float], ...]]] = record.placement.means(plane)
                return (
                    Result[Option[tuple[Rgb[float], ...]], BlenderFaults]
                    .bind(means, lambda found: found.to_result_with(lambda: Need.PLACED.unmet(image.name)))
                    .bind(lambda samples: charts.revised(call.identity, image, lambda held: Ok(msgspec.structs.replace(held, samples=samples))))
                    .bind(lambda _: call.root(bpy.types.Scene).bind(lambda scene: calibrations.revised(call.identity, scene, lambda chosen: Ok(msgspec.structs.replace(chosen, source=image)))))
                    .map(lambda _: None)
                )

            return paired(located(call.identity, call.context), self.stack.config()).bind(
                lambda pair: planed(call.context, pair[0][0], pair[1]).bind(lambda plane: kept(pair[0][0], pair[0][1], plane)).map(lambda _: Done(notes=(("INFO", f"Sampled {pair[0][0].name}"),)))
            )

        def framing(call: Call[Framing]) -> BlenderResult[Done]:
            def baked(image: bpy.types.Image, record: ChartImage, chart: Attached[ChartImage], placement: ChartPlacement, plane: npt.NDArray[np.float32]) -> BlenderResult[Done]:
                view = placement.panorama.value
                width = max(1, math.ceil(plane.shape[1] * view.view.field_of_view / math.tau))
                height = max(1, math.ceil(width / view.view.aspect))
                key = content_hash(PanoramaView, view, arrays=(plane,))

                def warped() -> BlenderResult[npt.NDArray[np.float32]]:
                    return StMap.bake(width, height, view.to_panorama, view.source.extension, source_size=(plane.shape[1], plane.shape[0])).warp(plane)

                return (
                    generated(views, call.identity, f"{image.name} View", key, lambda: warped().map(Some), Tag(space=Some(PyOpenColorIO.ROLE_SCENE_LINEAR)), of=Some(image))
                    .bind(
                        lambda shown: accumulated(
                            paired(
                                chart.write(image, msgspec.structs.replace(record, placement=placement)),
                                rooted(bpy.types.SpaceImageEditor, call.context).bind(lambda space: written(space, "image", view)),
                            )
                            for view in shown.to_list()
                        )
                    )
                    .map(lambda _: Done(notes=(("INFO", f"Viewing {image.name} chart"),)))
                )

            return paired(located(call.identity, call.context), self.stack.config()).bind(
                lambda pair: (
                    (Some(pair[0][1].placement) if pair[0][1].placement.panorama.enabled else pair[0][1].placement.framed(call.record.share))
                    .to_result_with(lambda: Need.PLACED.unmet(pair[0][0].name))
                    .bind(lambda placement: planed(call.context, pair[0][0], pair[1]).bind(lambda plane: baked(*pair[0], placement, plane)))
                )
            )

        def fitting[T: msgspec.Struct](call: Call[T]) -> BlenderResult[tuple[Calibrating, Calibration, CalibrationFit[Calibration]]]:
            return paired(self.stack.config(), charts.bound(call.identity), call.root(bpy.types.Scene).bind(lambda scene: calibrations.read(call.identity, scene))).bind(
                lambda found: calibrated(*found).map(lambda fit: (found[2], *fit))
            )

        def grading(call: Call[Empty]) -> BlenderResult[Done]:
            return paired(fitting(call), call.root(bpy.types.Scene)).bind(
                lambda pair: self.stack.assigned(call.identity, pair[1], Primaries(remap=Mixer.of(pair[0][1].gained))).map(
                    lambda entry: Done(notes=(*conditioned(pair[0][2]), ("INFO", f"Calibrated {entry.name}")))
                )
            )

        def splicing(call: Call[Empty]) -> BlenderResult[Done]:
            def spliced(chosen: Chosen, made: tuple[Calibrating, Calibration, CalibrationFit[Calibration]]) -> BlenderResult[Done]:
                return (
                    matrix()
                    .bind(lambda group: inserted(chosen, group, call.identity, NodeSpacing(), inputs=matrix_inputs(made[1])))
                    .map(lambda _: Done(notes=(*conditioned(made[2]), ("INFO", "Inserted calibration matrix"))))
                )

            return fitting(call).bind(lambda made: Chosen.of(call.context).bind(lambda chosen: spliced(chosen, made)))

        def exporting[T: Baking | Writing](call: Call[T]) -> BlenderResult[Done]:
            return paired(fitting(call), call.record.output()).bind(
                lambda pair: exported(pair[1], pair[0][1], call.paths, direction=pair[0][0].direction, clamped=pair[0][0].charting.clamped).map(
                    lambda paths: Done(notes=(*conditioned(pair[0][2]), *(("INFO", f"Wrote {path}") for path in paths)))
                )
            )

        def sided(path: Path, _record: object) -> tuple[Path, ...]:
            return tuple(path.with_stem(f"{path.stem}_{side.label}") for side in Direction)

        shown = Some(ChartCalibration.imaged)
        return ChartCommands(
            place=Operator(key="chart_place", title="Place Chart", description="Place a square-cell chart at shown image center", condition=shown, form=Empty, run=placing),
            sample=Operator(
                key="chart_sample",
                title="Sample Chart",
                description="Measure each patch's mean color",
                condition=shown,
                form=Empty,
                run=sampling,
                invoke=Some(Properties(confirm_text=Some("Sample"))),
                draw=Some(warned),
            ),
            clear=Operator(
                key="chart_clear",
                title="Clear Samples",
                description="Remove patch samples",
                condition=shown,
                form=Empty,
                run=edited(lambda _call, _image, record: Ok(msgspec.structs.replace(record, samples=()))),
                invoke=Some(Confirm(confirm_text=Some("Clear"), icon="WARNING")),
            ),
            flip=Operator(
                key="chart_flip",
                title="Flip Chart",
                description="Mirror patch order inside chart corners",
                condition=shown,
                form=Flipping,
                run=moved(lambda call, samples: Ok(samples[list(call.record.mirror.order)])),
            ),
            turn=Operator(
                key="chart_turn",
                title="Turn Chart",
                description="Rotate patch order a quarter turn",
                icon=Some("FILE_REFRESH"),
                condition=shown,
                form=Empty,
                run=moved(lambda _call, samples: Ok(np.roll(samples, 1, axis=0))),
            ),
            center=Operator(key="chart_center", title="Center Chart", description="Center chart in visible frame", icon=Some("ALIGN_CENTER"), condition=shown, form=Empty, run=moved(centered)),
            reset=Operator(
                key="chart_reset",
                title="Reset Chart",
                description="Reset chart layout",
                icon=Some("LOOP_BACK"),
                condition=shown,
                form=Empty,
                run=edited(lambda call, image, record: aspect(call.context, image).map(lambda ratio: msgspec.structs.replace(record, placement=seated(record.placement.chart, ratio)))),
            ),
            frame=Operator(
                key="chart_frame",
                title="View Chart",
                description="Show panorama chart in a rectilinear view",
                icon=Some("ZOOM_SELECTED"),
                condition=shown,
                form=Framing,
                run=framing,
                invoke=Some(Properties()),
            ),
            leave=Operator(
                key="chart_leave",
                title="Leave View",
                description="Show panorama holding chart",
                icon=Some("SCREEN_BACK"),
                condition=shown,
                form=Empty,
                run=edited(
                    lambda call, image, record: (
                        rooted(bpy.types.SpaceImageEditor, call.context)
                        .bind(lambda space: written(space, "image", image))
                        .map(lambda _: msgspec.structs.replace(record, placement=msgspec.structs.replace(record.placement, panorama=msgspec.structs.replace(record.placement.panorama, enabled=False))))
                    )
                ),
            ),
            grade=Operator(key="chart_grade", title="Apply Calibration", description="Write calibration into scene primaries", form=Empty, run=grading),
            node=Operator(key="chart_node", title="Insert Calibration", description="Insert calibration matrix after active node", condition=Some(ChartCalibration.shader), form=Empty, run=splicing),
            bake=Operator(
                key="chart_bake",
                title="Bake Calibration LUT",
                description="Bake forward and inverse calibration LUTs",
                form=Baking,
                run=exporting,
                invoke=Some(Save[Baking](extensions=Baking.extensions, destinations=Some(sided))),
                seed=Some(Baking.seeded),
                undo=False,
            ),
            write=Operator(
                key="chart_write",
                title="Write Calibration",
                description="Write forward and inverse calibration transform files",
                form=Writing,
                run=exporting,
                invoke=Some(Save[Writing](extensions=Writing.extensions, destinations=Some(sided))),
                seed=Some(Writing.seeded),
                undo=False,
            ),
        )

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns declarations over bound chart attachment."""
        refreshes = (Updated(handlers=bpy.app.handlers.depsgraph_update_post, ids=(bpy.types.Scene, bpy.types.Image)),)
        commands = self.commands

        def rows(chart: Attached[ChartImage]) -> tuple[Registrable, ...]:
            def access(context: bpy.types.Context) -> BlenderResult[Option[Access]]:
                def bound(pair: tuple[bpy.types.Image, ChartImage]) -> Option[Access]:
                    image, record = pair
                    placement = record.placement
                    samples = placement.panorama.value.to_view(placement.quad.samples) if placement.panorama.enabled else placement.quad.samples

                    def write(quad: Quad) -> BlenderResult[None]:
                        source = placement.realigned(quad).quad
                        return charts.revised(identity, image, lambda current: Ok(msgspec.structs.replace(current, placement=msgspec.structs.replace(current.placement, quad=source)))).map(
                            lambda _: None
                        )

                    return Some(samples).filter(lambda corners: bool(np.all(corners[:, 2] > 0.0))).map(lambda corners: Access(quad=Quad.of(corners), write=write))

                return charted(context, identity, chart).map(lambda found: found.bind(bound))

            def midpoint(edge: tuple[int, int]) -> Callable[[bpy.types.Context], BlenderResult[Option[Matrix]]]:
                def place(context: bpy.types.Context) -> BlenderResult[Option[Matrix]]:
                    return access(context).map(
                        lambda found: found.bind(
                            lambda held: (
                                Option.of_optional(context.region).map(framed).map(lambda frame: held.quad.samples[list(edge), :2].mean(axis=0) * (frame.width, frame.height) + (frame.x, frame.y))
                            )
                        ).map(lambda at: Matrix.Translation((*at, 0.0)))
                    )

                return place

            flips = tuple(
                Button(place=midpoint(edge), glyph=glyph, press=Own(command=commands.flip, values=Flipping(mirror=mirror)), scale_basis=HANDLE, draw_options=frozenset({"OUTLINE", "BACKDROP"}))
                for edge, glyph, mirror in (((0, 1), self.across, Mirror.ACROSS), ((0, 3), self.upright, Mirror.UPRIGHT))
            )
            return (
                views,
                *msgspec.structs.astuple(commands),
                FrameRegion(key="chart_corners", title="Chart Corners", access=access, shape=Free(), kinds=("IMAGE", "RENDER_RESULT"), refreshes=refreshes),
                Gizmos(key="chart", title="Chart", space="IMAGE_EDITOR", parts=flips, condition=Some(ChartCalibration.imaged)),
                overlaid(Patches(stack=self.stack, identity=identity), ChartCalibration.imaged, refreshes),
            )

        return charts.bound(identity).map(rows)

    def section(self) -> Single:
        """Image editor calibration section."""
        return Single(key="chart_calibration", title="Chart Calibration", rows=(Calibrator(row=self),))

    def node_section(self) -> Single:
        """Node editor matrix splice section, inactive outside an editable shader tree."""

        def rows(identity: Identity) -> BlenderResult[tuple[Row, ...]]:
            return calibrations.bound(identity).map(
                lambda held: (
                    *(Property(target=Member(root=bpy.types.Scene, path=f"{held.name}.{field.name}")) for field in msgspec.structs.fields(Calibrating) if field.name in {"source", "direction"}),
                    OperatorButton(target=self.commands.node),
                )
            )

        return Single(key="chart_calibration_node", title="Chart Calibration", rows=(Rows(rows=rows),), condition=Some(ChartCalibration.shader))


class Calibrator(Line, frozen=True, kw_only=True):
    """Chart calibration body with chart and scene fields and operator buttons."""

    row: ChartCalibration

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Places both records' fields and operator buttons."""
        commands = self.row.commands
        buttons: tuple[Row, ...] = (
            Caption(rows=tuple(OperatorButton(target=command) for command in (commands.place, commands.sample, commands.clear)), flow=Flow.ALIGNED),
            Caption(
                rows=(
                    *(
                        OperatorButton(target=Own(command=commands.flip, values=Flipping(mirror=mirror)), text="", icon=Some(glyph))
                        for mirror, glyph in ((Mirror.ACROSS, self.row.across), (Mirror.UPRIGHT, self.row.upright))
                    ),
                    *(OperatorButton(target=command, text="") for command in (commands.turn, commands.center, commands.reset, commands.frame, commands.leave)),
                ),
                flow=Flow.ALIGNED,
            ),
            Caption(rows=(OperatorButton(target=commands.grade), OperatorButton(target=commands.bake), OperatorButton(target=commands.write)), flow=Flow.ALIGNED),
        )

        def walked[T: msgspec.Struct](attached: Attached[T], host: Callable[[bpy.types.Context], Option[Host]]) -> BlenderResult[RowDraw]:
            def hosted(rows: RowDraw, nest: Nest) -> RowDraw:
                def shown(layout: bpy.types.UILayout, _holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                    for group in host(context).map(attached.struct).to_list():
                        rows(layout, group, context)
                        nest(layout, group, context, attached.name)

                return shown

            return fields(attached.group, identity).bind(lambda found: placed(found[0], identity).map(lambda draws: hosted(joined(draws), found[1])))

        return paired(charts.bound(identity), calibrations.bound(identity)).bind(
            lambda attached: paired(
                walked(attached[0], lambda context: root_in(bpy.types.SpaceImageEditor, context).bind(lambda space: Option.of_optional(space.image)).filter(attached[0].stored)),
                walked(attached[1], lambda context: root_in(bpy.types.Scene, context)),
                placed(buttons, identity),
            ).map(lambda parts: joined((parts[0], parts[1], *parts[2])))
        )


# --- [ERRORS] ---------------------------------------------------------------------------


class ChartUnmet(msgspec.Struct, frozen=True, kw_only=True):
    """Chart calibration requirement its subject lacks."""

    need: Need
    subject: str

    @property
    def message(self) -> str:
        """Poll message."""
        return self.need.format(self.subject)


# --- [OPERATIONS] -----------------------------------------------------------------------


def sampled_image(identity: Identity, image: bpy.types.Image) -> bool:
    """Whether image chart has every patch sampled, polled by image pointers."""
    return charts.bound(identity).to_option().filter(lambda chart: chart.stored(image)).bind(lambda chart: chart.read(image).to_option()).bind(lambda record: record.sampled).is_some()


def charted(context: bpy.types.Context, identity: Identity, chart: Attached[ChartImage]) -> BlenderResult[Option[tuple[bpy.types.Image, ChartImage]]]:
    """Returns shown image's chart, its panorama's chart for a view image, `Nothing` while the editor shows no charted image."""

    def held(image: bpy.types.Image, *, viewing: bool) -> BlenderResult[Option[tuple[bpy.types.Image, ChartImage]]]:
        return (
            chart.read(image).map(lambda record: Some(record).filter(lambda kept: kept.shown and kept.placement.panorama.enabled == viewing).map(lambda kept: (image, kept)))
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
                .filter(lambda mark: mark.key == views.key)
                .bind(lambda mark: mark.of)
                .bind(lambda block: Some((block, True)) if isinstance(block, bpy.types.Image) else Nothing)
                .default_value((shown, False))
            )
        )
        .map(lambda pair: held(pair[0], viewing=pair[1]))
        .default_value(Ok(Nothing))
    )


def calibrated(config: PyOpenColorIO.Config, chart: Attached[ChartImage], chosen: Calibrating) -> BlenderResult[tuple[Calibration, CalibrationFit[Calibration]]]:
    """Returns selected normalized calibration with original fit diagnostics."""

    def held(image: bpy.types.Image | None, side: str) -> BlenderResult[tuple[bpy.types.Image, ChartImage]]:
        match image:
            case None:
                return Error(Need.CHOSEN.unmet(side))
            case bpy.types.Image() if chart.stored(image):
                return chart.read(image).map(lambda record: (image, record))
            case bpy.types.Image():
                return Error(Need.SAMPLED.unmet(image.name))

    def samples(pair: tuple[bpy.types.Image, ChartImage]) -> BlenderResult[tuple[Rgb[float], ...]]:
        return pair[1].sampled.to_result_with(lambda: Need.SAMPLED.unmet(pair[0].name))

    def aimed(source: ChartImage) -> BlenderResult[tuple[Rgb[float], ...]]:
        match chosen.target:
            case Reference(space=space):
                return ChartReference.of(source.placement.chart).values(config, space)
            case Sampled(image=image):
                return (
                    held(image, "target")
                    .filter_with(
                        lambda pair: pair[1].placement.chart is source.placement.chart, lambda pair: Need.MATCHED.unmet(f"{source.placement.chart.label}, got {pair[1].placement.chart.label}")
                    )
                    .bind(samples)
                )

    def fitted(image: bpy.types.Image, record: ChartImage, source: tuple[Rgb[float], ...], goal: tuple[Rgb[float], ...]) -> BlenderResult[tuple[Calibration, CalibrationFit[Calibration]]]:
        neutral = ChartReference.of(record.placement.chart).neutral
        target_space = chosen.target.space if isinstance(chosen.target, Reference) else PyOpenColorIO.ROLE_SCENE_LINEAR

        def chosen_side(fit: CalibrationFit[Calibration]) -> BlenderResult[Calibration]:
            match chosen.direction:
                case Direction.FORWARD:
                    calibration, first, last, source_space, destination_space = fit.forward, source[neutral], goal[neutral], PyOpenColorIO.ROLE_SCENE_LINEAR, target_space
                case Direction.INVERSE:
                    calibration, first, last, source_space, destination_space = fit.inverse, goal[neutral], source[neutral], target_space, PyOpenColorIO.ROLE_SCENE_LINEAR

            normalized: BlenderResult[Option[Calibration]] = (
                calibration.normalized(first, last, config, source_space=source_space, target_space=destination_space) if chosen.charting.normalized else Ok(Some(calibration))
            )
            return (Result[Option[Calibration], BlenderFaults].bind(normalized, lambda found: found.to_result_with(lambda: Need.LIT.unmet(image.name)))).map(
                lambda selected: selected if chosen.gain else msgspec.structs.replace(selected, gained=selected.matrix, exposure=0.0)
            )

        fit: BlenderResult[CalibrationFit[Calibration]] = Calibration.fit(tuple(zip(source, goal)))
        return Result[CalibrationFit[Calibration], BlenderFaults].bind(fit, lambda fit: chosen_side(fit).map(lambda selected: (selected, fit)))

    return held(chosen.source, "source").bind(lambda pair: paired(samples(pair), aimed(pair[1])).bind(lambda rows: fitted(*pair, *rows)))


def exported(output: LutOutput, calibration: Calibration, paths: tuple[Path, ...], *, direction: Direction, clamped: bool) -> BlenderResult[tuple[str, ...]]:
    """Writes forward and inverse calibration files, optionally clamped."""
    clamp = (PyOpenColorIO.RangeTransform(minInValue=0.0, maxInValue=1.0, minOutValue=0.0, maxOutValue=1.0),) if clamped else ()
    transform = calibration.transform
    return lut_written(
        output,
        {
            str(destination): PyOpenColorIO.GroupTransform(
                transforms=[PyOpenColorIO.GroupTransform([transform], direction=PyOpenColorIO.TRANSFORM_DIR_FORWARD if side is direction else PyOpenColorIO.TRANSFORM_DIR_INVERSE), *clamp]
            )
            for side, destination in zip(Direction, paths)
        },
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

charts = Attachment(key="chart", form=ChartImage, owner=bpy.types.Image)
calibrations = Attachment(key="calibration", form=Calibrating, owner=bpy.types.Scene)
views = Ownership(kind=bpy.types.Image, key="chart_view", saved=Saved.USED)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Calibrating",
    "Calibrator",
    "ChartCalibration",
    "ChartCommands",
    "ChartImage",
    "ChartUnmet",
    "Charting",
    "Direction",
    "Flipping",
    "Framing",
    "Mirror",
    "Need",
    "Patches",
    "Reference",
    "Sampled",
    "Target",
    "calibrated",
    "calibrations",
    "charted",
    "charts",
    "exported",
    "views",
]
