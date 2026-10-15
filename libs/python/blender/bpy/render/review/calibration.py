"""Chart calibration in the image editor: the chart, its samples, the fitted matrix, and its grade, splice, and exports."""

from collections.abc import Callable
from functools import partial
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
from blender.bpy.data.ids.images import generated, read, saved, Tag
from blender.bpy.data.ids.ownership import Mark, Ownership, Saved
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.conditions import AllOf, Setting
from blender.bpy.data.rna.descriptors import Descriptor, root_in, written
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Registrable
from blender.bpy.faults import BlenderAbsentFault, BlenderFaults, BlenderImagingFault, BlenderResult, BlenderUnmetFault
from blender.bpy.nodes.editing import Chosen, inserted
from blender.bpy.nodes.placement import NodeSpacing
from blender.bpy.render.color.luts import Baking, Enabled, Writing
from blender.bpy.render.effects.stack import EffectStack, viewed
from blender.bpy.render.outputs.exports import image_type, plane
from blender.bpy.render.review.frames import Access, Bound, editor, framed, FrameRegion, Free, HANDLE, overlaid
from blender.bpy.scene.materials import matrix, matrix_inputs
from blender.bpy.settings.attachments import Attached, Attachment
from blender.bpy.settings.properties import Definition
from blender.bpy.ui.drawing.canvas import Canvas
from blender.bpy.ui.drawing.handlers import Updated
from blender.bpy.ui.layout.rows import Caption, Flow, joined, Line, Member, OperatorButton, placed, Property, Row, RowDraw
from blender.bpy.ui.layout.sections import fields, Nest, Single
from blender.bpy.ui.resources.icons import Glyph
from blender.bpy.ui.resources.theme import Metrics
from blender.bpy.ui.surfaces.gizmos import Button, Gizmos
from blender.imaging.calibration import Calibration, CalibrationFit, Chart, ChartPlacement, ChartReference, Clipped, Finding, Limits, Mirrored, Skewed, Wide
from blender.imaging.color.luts import LutOutput, written as lut_written
from blender.imaging.color.processors import applied, DisplayView, processor
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
    """Side of the fitted pair a row applies."""

    FORWARD = 1, "Forward"
    INVERSE = 2, "Inverse"


class Mirror(Choice):
    """Axis a chart's patch order mirrors across."""

    ACROSS = 1, "Left to Right"
    UPRIGHT = 2, "Top to Bottom"

    @property
    def order(self) -> tuple[int, int, int, int]:
        """Corner permutation in `Quad` field order."""
        match self:
            case Mirror.ACROSS:
                return (1, 0, 3, 2)
            case Mirror.UPRIGHT:
                return (3, 2, 1, 0)


class Pairing(Choice):
    """Side of a calibration an image takes."""

    SOURCE = 1, "source image"
    TARGET = 2, "target image"


# --- [POINTERS] -------------------------------------------------------------------------

sampled_pointer = Definition[bpy.types.Image](poll=Some(lambda identity, _owner, image: sampled_image(identity, image)))


# --- [MODELS] ---------------------------------------------------------------------------


class Reference(Case, frozen=True, kw_only=True, tag=1):
    """Target of a chart's published values in a config colorspace."""

    title = "Reference"
    space: Annotated[ColorSpace, msgspec.Meta(description="Colorspace the reference values take")] = PyOpenColorIO.ROLE_SCENE_LINEAR


class Sampled(Case, frozen=True, kw_only=True, tag=2):
    """Target of a second image's samples."""

    title = "Image"
    image: Annotated[bpy.types.Image | None, msgspec.Meta(description="Image whose samples the source maps onto"), sampled_pointer] = None


class ChartImage(msgspec.Struct, frozen=True, kw_only=True):
    """Chart placed on an image and its patch samples."""

    shown: Annotated[bool, msgspec.Meta(title="Show Chart", description="Draw the chart and its handles over the image")] = True
    placement: Annotated[ChartPlacement, msgspec.Meta(title="Chart", description="Chart placed in the image")] = ChartPlacement()
    samples: Annotated[tuple[Rgb[float], ...], msgspec.Meta(title="Samples", description="Scene-linear patch means in grid order")] = ()

    @property
    def sampled(self) -> Option[tuple[Rgb[float], ...]]:
        """Samples while one RGB value stands for every printed patch."""
        return Some(self.samples).filter(lambda held: len(held) == len(ChartReference.of(self.placement.chart).checker.data))


class Charting(msgspec.Struct, frozen=True, kw_only=True):
    """Chart, normalization, clamp, and overlay opacity one calibration takes."""

    chart: Annotated[Chart, msgspec.Meta(title="Chart", description="Chart a placement lays out")] = Chart.COLORCHECKER_CLASSIC
    normalized: Annotated[bool, msgspec.Meta(title="Normalize", description="Keep the source neutral's luma and hold the gain as exposure")] = True
    clamped: Annotated[bool, msgspec.Meta(title="Clamp", description="Clip exported values to 0 to 1")] = False
    opacity: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Opacity", description="Opacity of the patch fills")] = 0.5


class Calibrating(msgspec.Struct, frozen=True, kw_only=True):
    """Scene's calibration pairing and fit choices."""

    source: Annotated[bpy.types.Image | None, msgspec.Meta(title="Source", description="Sampled image the calibration maps from"), sampled_pointer] = None
    target: Annotated[Target, msgspec.Meta(title="Target", description="Colors the source's samples map onto")] = Reference()
    direction: Annotated[Direction, msgspec.Meta(title="Direction", description="Source onto target, or target back onto source")] = Direction.FORWARD
    gain: Annotated[bool, msgspec.Meta(title="Keep Gain", description="Fold the exposure gain into the applied and exported transform")] = False
    charting: Annotated[Charting, msgspec.Meta(title="Options", description="Chart, normalization, clamp, and opacity")] = Charting()


class Framing(msgspec.Struct, frozen=True, kw_only=True):
    """Share of the view a framed chart spans."""

    share: Annotated[float, msgspec.Meta(gt=0.0, le=1.0, title="Share", description="Share of the view's width the chart spans")] = 0.5


class Flipping(msgspec.Struct, frozen=True, kw_only=True):
    """Axis a flip mirrors across."""

    mirror: Annotated[Mirror, msgspec.Meta(title="Mirror", description="Axis the chart's patch order mirrors across")] = Mirror.ACROSS


class Drawn(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """One chart's fill batch and outline batches with their colors."""

    fills: gpu.types.GPUBatch
    edges: tuple[tuple[gpu.types.GPUBatch, tuple[float, float, float, float]], ...]


class Patches(msgspec.Struct, frozen=True, kw_only=True):
    """Chart overlay part: every chart's patches formed and batched once per refresh, the shown chart drawn."""

    stack: EffectStack
    identity: Identity

    def read(self, _metrics: Metrics, /) -> BlenderResult[Option[dict[Handle[bpy.types.Image], Drawn]]]:
        """Forms and batches the fill triangles and outline strips of every image holding a chart, in frame fractions."""
        fills, lines = gpu.shader.from_builtin("FLAT_COLOR"), gpu.shader.from_builtin("POLYLINE_UNIFORM_COLOR")

        def formed(scene: bpy.types.Scene, config: PyOpenColorIO.Config, image: bpy.types.Image) -> BlenderResult[Option[Callable[[npt.NDArray[np.float32]], BlenderResult[npt.NDArray[np.float32]]]]]:
            match image.type:
                case "RENDER_RESULT":
                    return self.stack.forming(self.identity, scene, config, Enabled.RENDER).map(
                        lambda chain: chain.map(
                            lambda transform: lambda values: processor(config, transform).map(lambda built: applied(built.getDefaultCPUProcessor(), values)).map_error(BlenderImagingFault.lifted)
                        )
                    )
                case _ if image.use_view_as_render:
                    return (
                        Option
                        .of_optional(scene.view_settings)
                        .map(lambda view: Some(lambda values: viewed(scene).form(config, values, exposure=view.exposure, gamma=view.gamma).map_error(BlenderImagingFault.lifted)))
                        .to_result_with(lambda: (BlenderAbsentFault(owner=repr(scene), key="view_settings"),))
                    )
                case _:
                    return (
                        Option
                        .of_optional(scene.display_settings)
                        .map(
                            lambda display: Some(
                                lambda values: (
                                    DisplayView(display=display.display_device, view=config.getDefaultView(display.display_device)).form(config, values).map_error(BlenderImagingFault.lifted)
                                )
                            )
                        )
                        .to_result_with(lambda: (BlenderAbsentFault(owner=repr(scene), key="display_settings"),))
                    )

        def patched(scene: bpy.types.Scene, config: PyOpenColorIO.Config, image: bpy.types.Image, record: ChartImage, opacity: float) -> BlenderResult[Option[tuple[Handle[bpy.types.Image], Drawn]]]:
            references = ChartReference.of(record.placement.chart).values(config).map_error(BlenderImagingFault.lifted)

            def drawn(form: Callable[[npt.NDArray[np.float32]], BlenderResult[npt.NDArray[np.float32]]], corners: npt.NDArray[np.float32]) -> BlenderResult[Drawn]:
                return Result[tuple[Rgb[float], ...], BlenderFaults].bind(
                    references,
                    lambda values: paired(
                        form(np.array([msgspec.structs.astuple(row) for row in record.sampled.default_value(values)], dtype=np.float32)),
                        form(np.array([msgspec.structs.astuple(row) for row in values], dtype=np.float32)),
                    ).map(
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
                                for quad, edge in zip(np.unstack(corners), np.unstack(np.concatenate((pair[1], np.ones((len(pair[1]), 1), np.float32)), axis=-1)), strict=True)
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
            .map(lambda scene: paired(self.stack.config(), paired(charts.bound(self.identity), calibrations.bound(self.identity))).bind(lambda pair: charts_of(scene, pair[0], *pair[1])))
            .default_value(Ok(Nothing))
        )

    def drawn(self, canvas: Canvas, state: dict[Handle[bpy.types.Image], Drawn]) -> None:
        """Draws the shown chart's patches scaled through the frame."""
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
    """Operator rows of chart calibration, one field per row a section places."""

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


class ChartCalibration(msgspec.Struct, frozen=True, kw_only=True):
    """Row placing chart calibration: the chart records, the view images, the overlay, the handles, the operator rows, and their sections."""

    imaged: ClassVar[Setting] = image_type(("IMAGE", "RENDER_RESULT"))
    shader: ClassVar[AllOf] = AllOf(
        conditions=(
            Setting(descriptor=Descriptor(root=bpy.types.SpaceNodeEditor, owner="", prop=bpy.types.SpaceNodeEditor.bl_rna.properties["tree_type"]), accepted=(bpy.types.ShaderNodeTree.__name__,)),
            Setting(descriptor=Descriptor(root=bpy.types.SpaceNodeEditor, owner="edit_tree", prop=bpy.types.NodeTree.bl_rna.properties["is_editable"])),
        )
    )
    stack: EffectStack
    across: Glyph
    upright: Glyph
    settings: Option[Callable[[Identity], BlenderResult[Charting]]] = Nothing

    @property
    def page_rows(self) -> tuple[Registrable, ...]:
        """Image charts and the scene's calibration, seeded from the settings reader when one is given, which the preferences page acquires."""
        return (charts, msgspec.structs.replace(calibrations, seed=self.settings.map(lambda read: lambda bound: read(bound).map(lambda charting: Calibrating(charting=charting)))))

    def operators(self, identity: Identity) -> ChartCommands:
        """Builds the operator rows over the image editor and node editor conditions."""

        def located(context: bpy.types.Context) -> BlenderResult[tuple[bpy.types.Image, ChartImage, Attached[ChartImage]]]:
            return charts.bound(identity).bind(
                lambda chart: charted(context, identity, chart).bind(
                    lambda found: found.map(lambda pair: (*pair, chart)).to_result_with(lambda: (BlenderAbsentFault(owner=repr(context.area), key=chart.name),))
                )
            )

        def edited[T: msgspec.Struct](change: Callable[[Call[T], npt.NDArray[np.float64]], npt.NDArray[np.float64]]) -> Callable[[Call[T]], BlenderResult[Done]]:
            def run(call: Call[T]) -> BlenderResult[Done]:
                return (
                    located(call.context)
                    .bind(
                        lambda found: (
                            found[1]
                            .placement.aligned.to_result_with(lambda: unplaced(found[0]))
                            .bind(
                                lambda homography: found[2].write(found[0], msgspec.structs.replace(found[1], placement=found[1].placement.realigned(Quad.of(change(call, homography.quad.samples)))))
                            )
                        )
                    )
                    .map(lambda _: Done())
                )

            return run

        def placing(call: Call[Empty]) -> BlenderResult[Done]:
            context = call.context
            return (
                paired(editor_image(context), paired(calibrations.bound(identity), call.root(bpy.types.Scene)).bind(lambda pair: pair[0].read(pair[1])))
                .bind(
                    lambda pair: paired(charts.bound(identity), aspect(context, pair[0])).bind(
                        lambda value: value[0].write(pair[0], ChartImage(placement=ChartPlacement(chart=pair[1].charting.chart, quad=squared(value[1], ChartReference.of(pair[1].charting.chart)))))
                    )
                )
                .map(lambda _: Done())
            )

        def sampling(call: Call[Empty]) -> BlenderResult[Done]:
            context = call.context

            def kept(image: bpy.types.Image, record: ChartImage, chart: Attached[ChartImage], plane: npt.NDArray[np.float32]) -> BlenderResult[None]:
                means: BlenderResult[Option[tuple[Rgb[float], ...]]] = record.placement.means(plane).map_error(BlenderImagingFault.lifted)
                return (
                    Result[Option[tuple[Rgb[float], ...]], BlenderFaults]
                    .bind(means, lambda found: found.to_result_with(lambda: unplaced(image)))
                    .bind(lambda means: chart.write(image, msgspec.structs.replace(record, samples=means)))
                    .bind(
                        lambda _: paired(calibrations.bound(identity), call.root(bpy.types.Scene)).bind(
                            lambda pair: pair[0].read(pair[1]).bind(lambda chosen: pair[0].write(pair[1], msgspec.structs.replace(chosen, source=image)))
                        )
                    )
                )

            return paired(located(context), self.stack.config()).bind(
                lambda pair: planed(context, pair[0][0], pair[1]).bind(lambda plane: kept(*pair[0], plane)).map(lambda _: Done(notes=(("INFO", f"Sampled {pair[0][0].name}"),)))
            )

        def findings(context: bpy.types.Context) -> BlenderResult[str]:
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
                        return f"Patches {', '.join(names[index] for index in patches)} leave the image"

            return located(context).bind(
                lambda found: aspect(context, found[0]).map(
                    lambda ratio: (
                        found[1]
                        .placement.findings(ratio, Limits())
                        .map(lambda listed: ", ".join(map(partial(described, reference=ChartReference.of(found[1].placement.chart)), listed)) or "Chart placement reads no finding")
                        .default_value(Unplaced(image=found[0].name).message)
                    )
                )
            )

        def clearing(call: Call[Empty]) -> BlenderResult[Outcome]:
            def cleared(image: bpy.types.Image, record: ChartImage, chart: Attached[ChartImage]) -> BlenderResult[Outcome]:
                if not record.samples:
                    return Ok(Unchanged(reason=f"The chart of {image.name} holds no samples"))
                return chart.write(image, msgspec.structs.replace(record, samples=())).map(lambda _: Done())

            return located(call.context).bind(lambda found: cleared(*found))

        def centered(call: Call[Empty]) -> BlenderResult[Done]:
            def shifted(found: tuple[bpy.types.Image, ChartImage, Attached[ChartImage]], region: bpy.types.Region) -> BlenderResult[None]:
                image, record, attached = found
                frame = framed(region)
                middle = np.array(((region.width / 2 - frame.x) / frame.width, (region.height / 2 - frame.y) / frame.height, 1.0), np.float64)
                return record.placement.aligned.to_result_with(lambda: unplaced(image)).bind(
                    lambda homography: attached.write(
                        image,
                        msgspec.structs.replace(record, placement=record.placement.realigned(Quad.of(homography.quad.samples + (middle - homography.quad.samples.mean(axis=0)) * (1.0, 1.0, 0.0)))),
                    )
                )

            return paired(located(call.context), call.root(bpy.types.Region)).bind(lambda pair: shifted(*pair)).map(lambda _: Done())

        def resetting(call: Call[Empty]) -> BlenderResult[Done]:
            return (
                located(call.context)
                .bind(
                    lambda found: aspect(call.context, found[0]).bind(
                        lambda ratio: found[2].write(
                            found[0], msgspec.structs.replace(found[1], placement=ChartPlacement(chart=found[1].placement.chart, quad=squared(ratio, ChartReference.of(found[1].placement.chart))))
                        )
                    )
                )
                .map(lambda _: Done())
            )

        def framing(call: Call[Framing]) -> BlenderResult[Done]:
            context = call.context

            def baked(image: bpy.types.Image, record: ChartImage, chart: Attached[ChartImage], placement: ChartPlacement, plane: npt.NDArray[np.float32]) -> BlenderResult[Done]:
                view = placement.panorama.value
                width = max(1, math.ceil(plane.shape[1] * view.view.field_of_view / math.tau))
                height = max(1, math.ceil(width / view.view.aspect))
                key = content_hash(PanoramaView, view, arrays=(plane,))

                def warped() -> BlenderResult[npt.NDArray[np.float32]]:
                    return StMap.bake(width, height, view.to_panorama, view.source.extension, source_size=(plane.shape[1], plane.shape[0])).warp(plane).map_error(BlenderImagingFault.lifted)

                return (
                    generated(views, identity, f"{image.name} View", key, lambda: warped().map(Some), Tag(space=Some(PyOpenColorIO.ROLE_SCENE_LINEAR)), of=Some(image))
                    .bind(
                        lambda shown: accumulated(
                            paired(chart.write(image, msgspec.structs.replace(record, placement=placement)), editor(context).bind(lambda space: written(space, "image", view)))
                            for view in shown.to_list()
                        )
                    )
                    .map(lambda _: Done(notes=(("INFO", f"Viewing the chart of {image.name}"),)))
                )

            return paired(located(context), self.stack.config()).bind(
                lambda pair: (
                    (Some(pair[0][1].placement) if pair[0][1].placement.panorama.enabled else pair[0][1].placement.framed(call.record.share))
                    .to_result_with(lambda: unplaced(pair[0][0]))
                    .bind(lambda placement: planed(context, pair[0][0], pair[1]).bind(lambda plane: baked(*pair[0], placement, plane)))
                )
            )

        def leaving(call: Call[Empty]) -> BlenderResult[Done]:
            return (
                located(call.context)
                .bind(
                    lambda found: paired(
                        found[2].write(
                            found[0],
                            msgspec.structs.replace(found[1], placement=msgspec.structs.replace(found[1].placement, panorama=msgspec.structs.replace(found[1].placement.panorama, enabled=False))),
                        ),
                        editor(call.context).bind(lambda space: written(space, "image", found[0])),
                    )
                )
                .map(lambda _: Done())
            )

        def fitting[T: msgspec.Struct](call: Call[T]) -> BlenderResult[tuple[Calibrating, Calibration, CalibrationFit[Calibration]]]:
            return paired(self.stack.config(), paired(charts.bound(identity), paired(calibrations.bound(identity), call.root(bpy.types.Scene)).bind(lambda pair: pair[0].read(pair[1])))).bind(
                lambda pair: calibrated(pair[0], pair[1][0], pair[1][1]).map(lambda fit: (pair[1][1], *fit))
            )

        def conditioned(fit: CalibrationFit[Calibration]) -> tuple[tuple["rna_enums.WmReportItems", str], ...]:
            ceiling = 1e4
            return (("WARNING", f"Calculated transform is poorly conditioned ({fit.condition_number:.2e}), chart alignment needs a check"),) if fit.condition_number > ceiling else ()

        def grading(call: Call[Empty]) -> BlenderResult[Done]:
            return paired(fitting(call), call.root(bpy.types.Scene)).bind(
                lambda pair: self.stack.assigned(identity, pair[1], Primaries(remap=Mixer.of(pair[0][1].gained))).map(
                    lambda entry: Done(notes=(*conditioned(pair[0][2]), ("INFO", f"Calibrated {entry.name}")))
                )
            )

        def splicing(call: Call[Empty]) -> BlenderResult[Done]:
            def spliced(chosen: Chosen, made: tuple[Calibrating, Calibration, CalibrationFit[Calibration]]) -> BlenderResult[Done]:
                return (
                    matrix()
                    .bind(lambda group: inserted(chosen, group, identity, NodeSpacing(), inputs=matrix_inputs(made[1])))
                    .map(lambda _: Done(notes=(*conditioned(made[2]), ("INFO", "Inserted the calibration matrix"))))
                )

            return fitting(call).bind(lambda made: Chosen.of(call.context).bind(lambda chosen: spliced(chosen, made)))

        def exporting[T: Baking | Writing](call: Call[T]) -> BlenderResult[Done]:
            return paired(fitting(call), call.record.output()).bind(
                lambda pair: exported(pair[1], pair[0][1], call.paths, direction=pair[0][0].direction, clamped=pair[0][0].charting.clamped).map(
                    lambda paths: Done(notes=(*conditioned(pair[0][2]), *(("INFO", f"Wrote {path}") for path in paths)))
                )
            )

        shown = Some(ChartCalibration.imaged)
        return ChartCommands(
            place=Operator(key="chart_place", title="Place Chart", description="Place a chart of square cells at the center of the shown image", condition=shown, form=Empty, run=placing),
            sample=Operator(
                key="chart_sample",
                title="Sample Chart",
                description="Measure each patch's mean color under the chart",
                condition=shown,
                form=Empty,
                run=sampling,
                invoke=Some(Confirm(message=Some(lambda context, _: findings(context)), confirm_text=Some("Sample"), icon="WARNING")),
            ),
            clear=Operator(
                key="chart_clear",
                title="Clear Samples",
                description="Remove the image's patch samples",
                condition=shown,
                form=Empty,
                run=clearing,
                invoke=Some(Confirm(confirm_text=Some("Clear"), icon="WARNING")),
            ),
            flip=Operator(
                key="chart_flip",
                title="Flip Chart",
                description="Mirror the chart's patch order inside its corners",
                condition=shown,
                form=Flipping,
                run=edited(lambda call, samples: samples[list(call.record.mirror.order)]),
            ),
            turn=Operator(
                key="chart_turn",
                title="Turn Chart",
                description="Turn the chart's patch order a quarter inside its corners",
                condition=shown,
                form=Empty,
                run=edited(lambda _, samples: np.roll(samples, 1, axis=0)),
            ),
            center=Operator(key="chart_center", title="Center Chart", description="Move the chart to the center of the visible frame", condition=shown, form=Empty, run=centered),
            reset=Operator(key="chart_reset", title="Reset Chart", description="Return the chart to its first layout", condition=shown, form=Empty, run=resetting),
            frame=Operator(key="chart_frame", title="View Chart", description="Show the panorama's chart in a rectilinear view", condition=shown, form=Framing, run=framing, invoke=Some(Properties())),
            leave=Operator(key="chart_leave", title="Leave View", description="Show the panorama the chart sits in", condition=shown, form=Empty, run=leaving),
            grade=Operator(key="chart_grade", title="Apply Calibration", description="Write the calibration into the scene's primaries", form=Empty, run=grading),
            node=Operator(
                key="chart_node", title="Insert Calibration", description="Insert the calibration matrix after the active node", condition=Some(ChartCalibration.shader), form=Empty, run=splicing
            ),
            bake=Operator(
                key="chart_bake",
                title="Bake Calibration LUT",
                description="Bake the forward and inverse calibration into LUT files",
                form=Baking,
                run=exporting,
                invoke=Some(Save[Baking](extensions=Baking.extensions, destinations=Some(lambda path, _record: export_paths(path)))),
                seed=Some(Baking.seeded),
                undo=False,
            ),
            write=Operator(
                key="chart_write",
                title="Write Calibration",
                description="Write the forward and inverse calibration matrices into transform files",
                form=Writing,
                run=exporting,
                invoke=Some(Save[Writing](extensions=Writing.extensions, destinations=Some(lambda path, _record: export_paths(path)))),
                seed=Some(Writing.seeded),
                undo=False,
            ),
        )

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Answers the view images, operators, corner region, handles, and overlay declarations, the page holding the attachments."""
        refreshes = (Updated(handlers=bpy.app.handlers.depsgraph_update_post, ids=(bpy.types.Scene, bpy.types.Image)),)

        def rows(chart: Attached[ChartImage], commands: ChartCommands) -> tuple[Registrable, ...]:
            def access(context: bpy.types.Context) -> BlenderResult[Option[Access]]:
                def bound(pair: tuple[bpy.types.Image, ChartImage]) -> Option[Access]:
                    image, record = pair
                    placement = record.placement
                    samples = placement.panorama.value.to_view(placement.quad.samples) if placement.panorama.enabled else placement.quad.samples

                    def write(quad: Quad) -> BlenderResult[None]:
                        source = placement.realigned(quad).quad
                        return chart.read(image).bind(lambda current: chart.write(image, msgspec.structs.replace(current, placement=msgspec.structs.replace(current.placement, quad=source))))

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
                Button(
                    place=midpoint(edge), glyph=glyph, press=Own(command=commands.flip, values=Flipping(mirror=mirror)), scale_basis=Some(HANDLE), draw_options=Some(frozenset({"OUTLINE", "BACKDROP"}))
                )
                for edge, glyph, mirror in (((0, 1), self.across, Mirror.ACROSS), ((0, 3), self.upright, Mirror.UPRIGHT))
            )
            return (
                views,
                *msgspec.structs.astuple(commands),
                FrameRegion(key="chart_corners", title="Chart Corners", access=access, shape=Free(), kinds=("IMAGE", "RENDER_RESULT"), refreshes=refreshes),
                Gizmos(key="chart", title="Chart", space="IMAGE_EDITOR", parts=flips, condition=Some(ChartCalibration.imaged)),
                overlaid(Patches(stack=self.stack, identity=identity), ChartCalibration.imaged, refreshes),
            )

        return charts.bound(identity).map(lambda chart: rows(chart, self.operators(identity)))

    def section(self) -> Single:
        """Image editor section: the shown image's chart, the scene's pairing and fit, and the operator buttons."""
        return Single(key="chart_calibration", title="Chart Calibration", rows=(Calibrator(row=self),))

    def node_section(self) -> Single:
        """Node editor section: the source and direction beside the matrix splice, inactive outside an editable shader tree."""

        def rows(identity: Identity) -> BlenderResult[tuple[Row, ...]]:
            return calibrations.bound(identity).map(
                lambda held: (
                    *(Property(target=Member(root=bpy.types.Scene, path=f"{held.name}.{field.name}")) for field in msgspec.structs.fields(Calibrating) if field.name in {"source", "direction"}),
                    OperatorButton(target=self.operators(identity).node),
                )
            )

        return Single(key="chart_calibration_node", title="Chart Calibration", rows=(Bound(rows=rows),), condition=Some(ChartCalibration.shader))


class Calibrator(Line, frozen=True, kw_only=True):
    """Chart calibration body: the shown image's chart fields while it holds a chart, the scene's calibration fields, and the operator buttons."""

    row: ChartCalibration

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Binds both records, walks their fields, and realizes the operator buttons over one identity."""
        commands = self.row.operators(identity)
        buttons: tuple[Row, ...] = (
            Caption(rows=tuple(OperatorButton(target=command) for command in (commands.place, commands.sample, commands.clear)), flow=Flow.ALIGNED),
            Caption(
                rows=(
                    *(
                        OperatorButton(target=Own(command=commands.flip, values=Flipping(mirror=mirror)), icon=Some(glyph))
                        for mirror, glyph in ((Mirror.ACROSS, self.row.across), (Mirror.UPRIGHT, self.row.upright))
                    ),
                    *(OperatorButton(target=command) for command in (commands.turn, commands.center, commands.reset)),
                ),
                flow=Flow.ALIGNED,
            ),
            Caption(rows=(OperatorButton(target=commands.frame), OperatorButton(target=commands.leave)), flow=Flow.ALIGNED),
            Caption(rows=(OperatorButton(target=commands.grade), OperatorButton(target=commands.bake), OperatorButton(target=commands.write)), flow=Flow.ALIGNED),
        )

        def walked[T: msgspec.Struct](attached: Attached[T]) -> BlenderResult[tuple[RowDraw, Nest]]:
            return fields(attached.group, identity).bind(lambda found: placed(found[0], identity).map(lambda draws: (joined(draws), found[1])))

        def draw(parts: tuple[tuple[tuple[Attached[ChartImage], Attached[Calibrating]], tuple[tuple[RowDraw, Nest], tuple[RowDraw, Nest]]], tuple[RowDraw, ...]]) -> RowDraw:
            ((chart, held), ((image_rows, image_nest), (scene_rows, scene_nest))), pressed = parts
            press = joined(pressed)

            def shown(layout: bpy.types.UILayout, holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
                for group in (chart.struct(image) for image in editor_image(context).to_option().filter(chart.stored).to_list()):
                    image_rows(layout, group, context)
                    image_nest(layout, group, context, chart.name)
                for scene in root_in(bpy.types.Scene, context).to_list():
                    group = held.struct(scene)
                    scene_rows(layout, group, context)
                    scene_nest(layout, group, context, held.name)
                press(layout, holder, context)

            return shown

        return paired(charts.bound(identity), calibrations.bound(identity)).bind(
            lambda attached: paired(paired(walked(attached[0]), walked(attached[1])).map(lambda rows: (attached, rows)), placed(buttons, identity)).map(draw)
        )


# --- [ERRORS] ---------------------------------------------------------------------------


class Unplaced(msgspec.Struct, frozen=True, kw_only=True):
    """Chart whose corners bound no convex quad inside its view."""

    image: str

    @property
    def message(self) -> str:
        """Poll message naming the image."""
        return f"Requires a chart in {self.image} whose corners bound a convex quad inside its view"


class Unsampled(msgspec.Struct, frozen=True, kw_only=True):
    """Image whose chart holds no sample of every patch."""

    image: str

    @property
    def message(self) -> str:
        """Poll message naming the image."""
        return f"Requires a sample of every patch in {self.image}"


class Unchosen(msgspec.Struct, frozen=True, kw_only=True):
    """Calibration side holding no image."""

    pairing: Pairing

    @property
    def message(self) -> str:
        """Poll message naming the side."""
        return f"Requires a {self.pairing.label} whose chart holds samples"


class Mismatched(msgspec.Struct, frozen=True, kw_only=True):
    """Target image sampled on another chart than the source."""

    source: Chart
    target: Chart

    @property
    def message(self) -> str:
        """Poll message naming both charts."""
        return f"Requires a target sampled on the {self.source.label} of the source, not the {self.target.label}"


class Unlit(msgspec.Struct, frozen=True, kw_only=True):
    """Neutral patch reading no light."""

    image: str

    @property
    def message(self) -> str:
        """Poll message naming the source image."""
        return f"Requires light on the neutral patch of {self.image} and of its target"


# --- [OPERATIONS] -----------------------------------------------------------------------


def unplaced(image: bpy.types.Image) -> BlenderFaults:
    """Answers the unplaced refusal of an image's chart."""
    return (BlenderUnmetFault(requirement=Unplaced(image=image.name)),)


def sampled_image(identity: Identity, image: bpy.types.Image) -> bool:
    """Whether an image's chart holds a sample of every patch, the poll of every image pointer."""
    return charts.bound(identity).to_option().filter(lambda chart: chart.stored(image)).bind(lambda chart: chart.read(image).to_option()).bind(lambda record: record.sampled).is_some()


def editor_image(context: bpy.types.Context) -> BlenderResult[bpy.types.Image]:
    """Answers the image the context's image editor shows."""
    return editor(context).bind(lambda space: Option.of_optional(space.image).to_result_with(lambda: (BlenderAbsentFault(owner=repr(space), key="image"),)))


def charted(context: bpy.types.Context, identity: Identity, chart: Attached[ChartImage]) -> BlenderResult[Option[tuple[bpy.types.Image, ChartImage]]]:
    """Answers the chart the shown image displays: its own, or its panorama's while it is that panorama's view image."""
    return (
        editor_image(context)
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
        .bind(
            lambda pair: (
                chart.read(pair[0]).map(lambda record: Some(record).filter(lambda held: held.shown and held.placement.panorama.enabled == pair[1]).map(lambda held: (pair[0], held)))
                if chart.stored(pair[0])
                else Ok(Nothing)
            )
        )
    )


def aspect(context: bpy.types.Context, image: bpy.types.Image) -> BlenderResult[float]:
    """Answers the frame's width over height, the render's for the Render Result."""
    return (
        root_in(bpy.types.Scene, context)
        .map(lambda scene: scene.render)
        .to_result_with(lambda: (BlenderAbsentFault(owner=repr(context), key="scene"),))
        .map(lambda render: render.resolution_x * render.pixel_aspect_x / (render.resolution_y * render.pixel_aspect_y))
        if image.type == "RENDER_RESULT"
        else Ok(image.size[0] / image.size[1])  # type: ignore[index]
    )


def squared(ratio: float, reference: ChartReference) -> Quad:
    """Answers a chart of square cells centered across half the frame width."""
    columns, rows = reference.shape
    half = np.array((0.25, 0.25 * ratio * rows / columns))
    corners = np.array(((-1.0, 1.0), (1.0, 1.0), (1.0, -1.0), (-1.0, -1.0))) * half + 0.5
    return Quad.of(np.concatenate((corners, np.ones((4, 1))), axis=-1))


def planed(context: bpy.types.Context, image: bpy.types.Image, config: PyOpenColorIO.Config) -> BlenderResult[npt.NDArray[np.float32]]:
    """Reads an image's scene-linear RGBA plane, the Render Result through its saved multilayer file."""

    def linear(found: ImageFile[np.float32]) -> BlenderResult[npt.NDArray[np.float32]]:
        return (
            processor(config, PyOpenColorIO.ColorSpaceTransform(src=found.colorspace, dst=PyOpenColorIO.ROLE_SCENE_LINEAR))
            .map(lambda built: applied(built.getDefaultCPUProcessor(), np.ascontiguousarray(found.pixels)))
            .map(lambda plane: plane if plane.shape[2] == 4 else np.concatenate((plane, np.ones((*plane.shape[:2], 1), np.float32)), axis=-1))
            .map_error(BlenderImagingFault.lifted)
        )

    match image.type:
        case "RENDER_RESULT":
            return Result[tuple[bpy.types.Scene, bpy.types.ViewLayer], BlenderFaults].bind(
                paired(
                    root_in(bpy.types.Scene, context).to_result_with(lambda: (BlenderAbsentFault(owner=repr(context), key="scene"),)),
                    root_in(bpy.types.ViewLayer, context).to_result_with(lambda: (BlenderAbsentFault(owner=repr(context), key="view_layer"),)),
                ),
                lambda pair: saved(pair[0]).bind(lambda path: plane(path, ("", f"{pair[1].name}.Combined"), ("R", "G", "B", "A"))),
            )
        case _:
            return read(image, config).bind(linear)


def calibrated(config: PyOpenColorIO.Config, chart: Attached[ChartImage], chosen: Calibrating) -> BlenderResult[tuple[Calibration, CalibrationFit[Calibration]]]:
    """Returns the selected normalized calibration and its original native fit diagnostics."""

    def held(image: bpy.types.Image | None, pairing: Pairing) -> BlenderResult[tuple[bpy.types.Image, ChartImage]]:
        return (
            Error((BlenderUnmetFault(requirement=Unchosen(pairing=pairing)),))
            if image is None
            else chart.read(image).map(lambda record: (image, record))
            if chart.stored(image)
            else Error((BlenderUnmetFault(requirement=Unsampled(image=image.name)),))
        )

    def samples(pair: tuple[bpy.types.Image, ChartImage]) -> BlenderResult[tuple[Rgb[float], ...]]:
        return pair[1].sampled.to_result_with(lambda: (BlenderUnmetFault(requirement=Unsampled(image=pair[0].name)),))

    def aimed(source: ChartImage) -> BlenderResult[tuple[Rgb[float], ...]]:
        match chosen.target:
            case Reference(space=space):
                return ChartReference.of(source.placement.chart).values(config, space).map_error(BlenderImagingFault.lifted)
            case Sampled(image=image):
                return (
                    held(image, Pairing.TARGET)
                    .filter_with(
                        lambda pair: pair[1].placement.chart is source.placement.chart,
                        lambda pair: (BlenderUnmetFault(requirement=Mismatched(source=source.placement.chart, target=pair[1].placement.chart)),),
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
                calibration.normalized(first, last, config, source_space=source_space, target_space=destination_space).map_error(BlenderImagingFault.lifted)
                if chosen.charting.normalized
                else Ok(Some(calibration))
            )
            return (Result[Option[Calibration], BlenderFaults].bind(normalized, lambda found: found.to_result_with(lambda: (BlenderUnmetFault(requirement=Unlit(image=image.name)),)))).map(
                lambda selected: selected if chosen.gain else msgspec.structs.replace(selected, gained=selected.matrix, exposure=0.0)
            )

        fit: BlenderResult[CalibrationFit[Calibration]] = Calibration.fit(tuple(zip(source, goal, strict=False))).map_error(BlenderImagingFault.lifted)
        return Result[CalibrationFit[Calibration], BlenderFaults].bind(fit, lambda fit: chosen_side(fit).map(lambda selected: (selected, fit)))

    return held(chosen.source, Pairing.SOURCE).bind(lambda pair: paired(samples(pair), aimed(pair[1])).bind(lambda rows: fitted(*pair, *rows)))


def export_paths(path: Path) -> tuple[Path, ...]:
    """Names both calibration outputs from the browser's selected base path."""
    return tuple(path.with_stem(f"{path.stem}_{side.label}") for side in Direction)


def exported(output: LutOutput, calibration: Calibration, paths: tuple[Path, ...], *, direction: Direction, clamped: bool) -> BlenderResult[tuple[str, ...]]:
    """Writes the forward and inverse calibration files with an optional output clamp."""
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
    ).map_error(BlenderImagingFault.lifted)


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
    "Charting",
    "Direction",
    "Flipping",
    "Framing",
    "Mirror",
    "Mismatched",
    "Pairing",
    "Patches",
    "Reference",
    "Sampled",
    "Target",
    "Unchosen",
    "Unlit",
    "Unplaced",
    "Unsampled",
    "calibrated",
    "calibrations",
    "charted",
    "charts",
    "exported",
    "views",
]
