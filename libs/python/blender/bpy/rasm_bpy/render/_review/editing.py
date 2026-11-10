"""Chart edits on images: the commands placing, sampling, flipping, turning, centering, resetting, and viewing the shown image's chart, its corner cage and flip buttons, and its section rows."""

from collections.abc import Callable
from enum import Enum
from functools import cached_property, partial, update_wrapper
import math
from typing import Annotated, ClassVar, TYPE_CHECKING

import bpy
from expression import Option, Some
from mathutils import Matrix
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from rasm_bpy.commands.invocations import Popup
from rasm_bpy.commands.operators import Answer, Call, Done, Empty, Invoke, Operator, Placement, Unchanged
from rasm_bpy.data._extension.identity import Identity
from rasm_bpy.data._extension.registration import Acquisitions, Declaration
from rasm_bpy.data._ids.images import Tag
from rasm_bpy.data._rna.descriptors import written
from rasm_bpy.data._rna.paths import rooted
from rasm_bpy.data._rna.properties import annotated, enum_choices
from rasm_bpy.nodes._content import Generated
from rasm_bpy.render._review.charts import ChartImage, Charting, Need, PlacedChart
from rasm_bpy.settings._attachments import Attached
from rasm_bpy.ui._drawing.handlers import HandlerRefresh
from rasm_bpy.ui._elements.gizmos import Gizmo, GizmoGroup
from rasm_bpy.ui._images.views import displayed_aspect, framed, FrameRegion, HANDLE
from rasm_bpy.ui._layout.members import Member
from rasm_bpy.ui._layout.rows import grouped, Layout, OperatorButton, Property, RowContent
from rasm_bpy.ui._resources.icons import Icon
from rasm_imaging import ChartPlacement, Clipped, Finding, ImageFile, PanoramaView, PublishedChart, Quad, Reversed, Skewed, StMap, Wide
from rasm_records import accumulated, content_hash, Fault, Nothing, Ok, Outcome, paired, Reason

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------


class Mirror(Enum):
    """Patch order mirror axis, valued by its corner permutation in `Quad` corner order."""

    HORIZONTAL = (1, 0, 3, 2)
    VERTICAL = (3, 2, 1, 0)


# --- [MODELS] ---------------------------------------------------------------------------


class ChartFraming(bpy.types.PropertyGroup):
    """Share of the view width a framed chart spans."""

    share: Annotated[float, bpy.props.FloatProperty(name="Share", description="View width chart spans", subtype="FACTOR", min=0.01, max=1.0, default=0.5)]


class Flipping(bpy.types.PropertyGroup):
    """Flip axis."""

    mirror: Annotated[str, bpy.props.EnumProperty(name="Mirror", description="Patch order mirror axis", items=enum_choices(Mirror))]


# --- [SERVICES] -------------------------------------------------------------------------


class ChartEditing(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Commands, corner cage, and flip buttons over the shown image's chart, the buttons drawn under the `across` and `upright` glyphs."""

    # --- [STATE]

    horizontal: Icon
    vertical: Icon
    charting: ClassVar[Charting] = Charting()

    # --- [COMMANDS]

    def located(self, identity: Identity, context: bpy.types.Context) -> Outcome[tuple[bpy.types.Image, PlacedChart, Attached[ChartImage]]]:
        """Returns the shown image, its drawn chart, and the chart attachment, absent fault while the editor shows no charted image."""
        return (
            self.charting
            .charted(identity, context)
            .map(lambda found: found.map(lambda chart: chart[:3]))
            .bind(lambda found: found.to_result_with(partial(Fault.of, Reason.ABSENT, repr(context.area), member=identity.identifier(self.charting.charts.key))))
        )

    def edited[T: bpy.types.PropertyGroup](self, change: Callable[[Call[T], bpy.types.Image, PlacedChart], Outcome[PlacedChart]], call: Call[T]) -> Outcome[Answer]:
        """Writes the shown chart's change, unchanged while the change keeps it."""

        def kept(image: bpy.types.Image, record: PlacedChart, chart: Attached[ChartImage]) -> Outcome[Answer]:
            return change(call, image, record).bind(
                lambda revised: Ok(Unchanged(reason=f"{image.name} chart unchanged")) if revised == record else self.charting.placed.write(chart.struct(image), revised).map(lambda _: Done())
            )

        return self.located(call.identity, call.context).bind(lambda found: kept(*found))

    def moved[T: bpy.types.PropertyGroup](self, move: Callable[[Call[T], npt.NDArray[np.float64]], Outcome[npt.NDArray[np.float64]]], call: Call[T]) -> Outcome[Answer]:
        """Writes the shown chart's corners as a move places them in its alignment frame."""
        return self.edited(
            lambda each, image, record: record.placement.aligned.bind(
                lambda aligned: (
                    aligned
                    .to_result_with(lambda: Fault.of(Need.PLACED, image.name))
                    .bind(lambda homography: move(each, homography.quad.samples))
                    .map(lambda samples: record.positioned(record.placement.realigned(Quad.of(samples))))
                )
            ),
            call,
        )

    def place_chart(self, call: Call[Empty]) -> Outcome[Answer]:
        """Place a square-cell chart at shown image center."""

        def placed(images: tuple[bpy.types.Image, bpy.types.Image], chart: Attached[ChartImage], ratio: float) -> Outcome[None]:
            image, shown = images
            chosen = PublishedChart[chart.struct(image).chart]
            placement = (
                Ok(self.charting.centered(chosen, ratio))
                if image == shown
                else self.charting
                .kept(chart, image)
                .bind(lambda kept: kept.to_result_with(partial(Fault.of, Need.PLACED, image.name)))
                .map(lambda kept: self.charting.centered(chosen, ratio, kept.placement.panorama))
            )
            return placement.bind(lambda held: self.charting.placed.write(chart.struct(image), PlacedChart(placement=held)))

        images = self.charting.shown(call.identity, call.context).to_result_with(partial(Fault.of, Reason.ABSENT, repr(call.context.space_data), member="image"))
        return paired(images, self.charting.charts.bound(call.identity), displayed_aspect(call.context)).bind(lambda found: placed(*found)).map(lambda _: Done())

    def sample_chart(self, call: Call[Empty]) -> Outcome[Answer]:
        """Measure each patch's mean color, reporting the chart's placement findings."""
        config = PyOpenColorIO.GetCurrentConfig()

        def described(finding: Finding, chart: PublishedChart) -> str:
            match finding:
                case Reversed():
                    return "Chart reads reversed"
                case Skewed(narrowest=narrowest, widest=widest):
                    return f"Corner angles run {math.degrees(narrowest):.0f} to {math.degrees(widest):.0f} degrees"
                case Wide(field_of_view=field_of_view):
                    return f"View spans {math.degrees(field_of_view):.0f} degrees"
                case Clipped(patches=patches):
                    names = tuple(chart.checker.data)
                    return f"Patches {', '.join(names[index] for index in patches)} leave image"

        def kept(image: bpy.types.Image, record: PlacedChart, chart: Attached[ChartImage], file: ImageFile) -> Outcome[Answer]:
            means = record.placement.means(file.pixels).bind(lambda found: found.to_result_with(lambda: Fault.of(Need.PLACED, image.name)))
            return paired(means, displayed_aspect(call.context).bind(record.placement.findings)).bind(
                lambda found: self.charting.placed.write(chart.struct(image), msgspec.structs.replace(record, samples=found[0][1], colorspace=file.colorspace)).map(
                    lambda _: Done(notes=(("INFO", f"Sampled {image.name}"), *(("WARNING", described(finding, record.placement.chart)) for held in found[1].to_list() for finding in held)))
                )
            )

        return self.located(call.identity, call.context).bind(lambda found: self.charting.read(call.identity, found[0], config).bind(lambda file: kept(*found, file)))

    @staticmethod
    def clear_samples(_call: Call[Empty], _image: bpy.types.Image, record: PlacedChart) -> Outcome[PlacedChart]:
        """Remove patch samples."""
        return Ok(msgspec.structs.replace(record, samples=()))

    def reset_chart(self, call: Call[Empty], _image: bpy.types.Image, record: PlacedChart) -> Outcome[PlacedChart]:
        """Reset chart layout."""
        return displayed_aspect(call.context).map(lambda ratio: record.positioned(self.charting.centered(record.placement.chart, ratio, record.placement.panorama)))

    @staticmethod
    def leave_view(call: Call[Empty], image: bpy.types.Image, record: PlacedChart) -> Outcome[PlacedChart]:
        """Show panorama holding chart."""
        return (
            rooted(bpy.types.SpaceImageEditor, call.context)
            .bind(lambda space: written(space, ("image",), image))
            .map(lambda _: record.positioned(msgspec.structs.replace(record.placement, panorama=None)))
        )

    @staticmethod
    def flip_chart(call: Call[Flipping], samples: npt.NDArray[np.float64]) -> Outcome[npt.NDArray[np.float64]]:
        """Mirror patch order inside chart corners."""
        return Ok(samples[list(Mirror[call.properties.mirror].value)])

    @staticmethod
    def turn_chart(_call: Call[Empty], samples: npt.NDArray[np.float64]) -> Outcome[npt.NDArray[np.float64]]:
        """Rotate patch order a quarter turn."""
        return Ok(np.roll(samples, 1, axis=0))

    @staticmethod
    def center_chart(call: Call[Empty], samples: npt.NDArray[np.float64]) -> Outcome[npt.NDArray[np.float64]]:
        """Center chart in visible frame."""

        def middle(region: bpy.types.Region) -> npt.NDArray[np.float64]:
            frame = framed(region)
            return np.array(((region.width / 2 - frame.x) / frame.width, (region.height / 2 - frame.y) / frame.height, 1.0), np.float64)

        return rooted(bpy.types.Region, call.context).map(lambda region: samples + (middle(region) - samples.mean(axis=0)) * (1.0, 1.0, 0.0))

    def view_chart(self, call: Call[ChartFraming]) -> Outcome[Answer]:
        """Show panorama chart in a rectilinear view."""
        config = PyOpenColorIO.GetCurrentConfig()

        def baked(image: bpy.types.Image, record: PlacedChart, chart: Attached[ChartImage], placement: ChartPlacement, file: ImageFile) -> Outcome[Answer]:
            plane = file.pixels
            view = Option.of_optional(placement.panorama).default_with(PanoramaView)
            width = max(1, math.ceil(plane.shape[1] * view.view.field_of_view / math.tau))
            height = max(1, math.ceil(width / view.view.aspect))

            def warped() -> Outcome[Option[npt.NDArray[np.float32]]]:
                return StMap.bake(width, height, view.to_panorama, view.source.extension, source_size=(plane.shape[1], plane.shape[0])).warp(plane).map(Some)

            return (
                Generated(
                    ownership=self.charting.views,
                    content=content_hash(PanoramaView, view, arrays=(plane,)),
                    name=f"{image.name} View",
                    plane=warped,
                    tag=Tag(space=Some(file.colorspace), view_as_render=Some(image.source != "GENERATED" and image.use_view_as_render)),
                )
                .made(call.identity, of=Some(image))
                .bind(
                    lambda shown: accumulated(
                        self.charting.placed.write(chart.struct(image), record.positioned(placement)).bind(
                            lambda _: rooted(bpy.types.SpaceImageEditor, call.context).bind(lambda space: written(space, ("image",), made))
                        )
                        for made in shown.to_list()
                    )
                )
                .map(lambda _: Done(notes=(("INFO", f"Viewing {image.name} chart"),)))
            )

        return self.located(call.identity, call.context).bind(
            lambda found: (
                (Some(found[1].placement) if found[1].placement.panorama is not None else found[1].placement.framed(call.properties.share))
                .to_result_with(lambda: Fault.of(Need.PLACED, found[0].name))
                .bind(lambda placement: self.charting.read(call.identity, found[0], config).bind(lambda file: baked(*found, placement, file)))
            )
        )

    def _bound[T: bpy.types.PropertyGroup, **P, R](
        self, form: type[T], apply: Callable[[Callable[P, R], Call[T]], Outcome[Answer]], step: Callable[P, R], /, *, icon: Option["rna_enums.IconItems"] = Nothing, invoke: Option[Invoke[T]] = Nothing
    ) -> Operator[T]:
        """Returns the operator running a step through `edited` or `moved` over the shown image's chart, described by the step."""
        return Operator[T](condition=Some(self.charting.imaged), form=form, run=update_wrapper(partial(apply, step), step), icon=icon, invoke=invoke)

    @cached_property
    def placing(self) -> Operator[Empty]:
        """Command placing the chosen chart on the shown image."""
        return Operator.of(Empty, condition=Some(self.charting.imaged))(self.place_chart)

    @cached_property
    def sampling(self) -> Operator[Empty]:
        """Command sampling the shown chart's patches."""
        return Operator.of(Empty, condition=Some(self.charting.imaged))(self.sample_chart)

    @cached_property
    def clearing(self) -> Operator[Empty]:
        """Command clearing the shown chart's samples after a confirmation."""
        return self._bound(Empty, self.edited, self.clear_samples, invoke=Some(Popup[Empty](partial(bpy.types.WindowManager.invoke_confirm, confirm_text="Clear", icon="WARNING"))))

    @cached_property
    def flipping(self) -> Operator[Flipping]:
        """Command mirroring the shown chart's patch order."""
        return self._bound(Flipping, self.moved, self.flip_chart)

    @cached_property
    def turning(self) -> Operator[Empty]:
        """Command turning the shown chart's patch order."""
        return self._bound(Empty, self.moved, self.turn_chart, icon=Some("FILE_REFRESH"))

    @cached_property
    def centering(self) -> Operator[Empty]:
        """Command centering the shown chart in the visible frame."""
        return self._bound(Empty, self.moved, self.center_chart, icon=Some("ALIGN_CENTER"))

    @cached_property
    def resetting(self) -> Operator[Empty]:
        """Command placing the shown chart again."""
        return self._bound(Empty, self.edited, self.reset_chart, icon=Some("LOOP_BACK"))

    @cached_property
    def viewing(self) -> Operator[ChartFraming]:
        """Command showing the shown panorama chart in a rectilinear view."""
        return Operator.of(ChartFraming, icon=Some("ZOOM_SELECTED"), condition=Some(self.charting.imaged), invoke=Some(Popup[ChartFraming](partial(bpy.types.WindowManager.invoke_props_dialog))))(
            self.view_chart
        )

    @cached_property
    def leaving(self) -> Operator[Empty]:
        """Command returning from a chart view to its panorama."""
        return self._bound(Empty, self.edited, self.leave_view, icon=Some("SCREEN_BACK"))

    # --- [HANDLES]

    def quad(self, identity: Identity, context: bpy.types.Context) -> Outcome[Option[Quad]]:
        """Returns the shown chart's corners in its alignment frame, Nothing while a corner falls behind its view."""
        return self.charting.charted(identity, context).map(lambda found: found.bind(lambda pair: pair[1].placement.viewed))

    def requad(self, identity: Identity, context: bpy.types.Context, quad: Quad) -> Outcome[None]:
        """Writes the shown chart's corners an edit of its alignment frame gives."""

        def realigned(image: bpy.types.Image, record: PlacedChart, chart: Attached[ChartImage]) -> Outcome[None]:
            return self.charting.placed.write(chart.struct(image), record.positioned(record.placement.realigned(quad)))

        return self.located(identity, context).bind(lambda found: realigned(*found))

    def edge(self, identity: Identity, corners: tuple[int, int], context: bpy.types.Context) -> Outcome[Option[Matrix]]:
        """Places a flip button at the middle of the shown chart's edge between two corners."""
        return self.quad(identity, context).map(
            lambda found: found.bind(
                lambda quad: Option.of_optional(context.region).map(framed).map(lambda frame: quad.samples[list(corners), :2].mean(axis=0) * (frame.width, frame.height) + (frame.x, frame.y))
            ).map(lambda at: Matrix.Translation((*at, 0.0)))
        )

    # --- [SECTIONS]

    @property
    def mirrors(self) -> tuple[tuple[Mirror, Icon, tuple[int, int]], ...]:
        """Each mirror axis with its button icon and the corners of the edge its gizmo sits on."""
        return ((Mirror.HORIZONTAL, self.horizontal, (0, 1)), (Mirror.VERTICAL, self.vertical, (0, 3)))

    @property
    def rows(self) -> tuple[RowContent, ...]:
        """Shown image's chart fields and the chart command groups."""
        return (
            Layout(
                builder=Some(
                    lambda identity: Ok(
                        tuple(
                            Property(target=Member(root=self.charting.charts, path=(name,), reach=Some(lambda context: self.charting.shown(identity, context).map(lambda images: images[0]))))
                            for name in annotated(ChartImage)
                        )
                    )
                )
            ),
            Layout(rows=tuple(OperatorButton(target=command) for command in (self.placing, self.sampling, self.clearing)), container=Some(grouped)),
            Layout(
                rows=(
                    *(OperatorButton(target=Placement(self.flipping, {"mirror": mirror.name}), keywords={"text": ""}, icon=Some(icon)) for mirror, icon, _ in self.mirrors),
                    *(OperatorButton(target=command, keywords={"text": ""}) for command in (self.turning, self.centering, self.resetting, self.viewing, self.leaving)),
                ),
                container=Some(grouped),
            ),
        )

    # --- [REGISTRATION]

    @property
    def prerequisites(self) -> tuple[Declaration, ...]:
        """Charts the edits read."""
        return (self.charting,)

    def acquisitions(self, identity: Identity) -> Acquisitions:
        """Returns the commands, the corner cage, and the flip buttons."""
        imaged = self.charting.imaged
        flips = tuple(
            Gizmo(
                kind="GIZMO_GT_button_2d",
                place=partial(self.edge, identity, corners),
                bind=(Placement(self.flipping, {"mirror": mirror.name}),),
                icon=Some(icon),
                properties={"scale_basis": HANDLE, "draw_options": frozenset({"OUTLINE", "BACKDROP"})},
                role=lambda ui: (ui.wcol_tool.inner, ui.wcol_tool.inner_sel),
            )
            for mirror, icon, corners in self.mirrors
        )
        return (
            self.placing,
            self.sampling,
            self.clearing,
            self.flipping,
            self.turning,
            self.centering,
            self.resetting,
            self.viewing,
            self.leaving,
            FrameRegion(
                key="chart_corners",
                read=partial(self.quad, identity),
                write=partial(self.requad, identity),
                transform=frozenset({"TRANSLATE", "ROTATE", "SCALE"}),
                corners=True,
                shown=imaged,
                refreshes=(HandlerRefresh(handlers=bpy.app.handlers.depsgraph_update_post, ids=(bpy.types.Scene, bpy.types.Image)),),
            ),
            GizmoGroup(key="chart", space="IMAGE_EDITOR", read=lambda context: Ok(Some(context)), gizmos=flips, condition=Some(imaged)),
        )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ChartEditing", "ChartFraming", "Flipping", "Mirror"]
