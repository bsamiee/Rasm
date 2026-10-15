"""Native tone curves, rendering primaries, and photoreceptor compositor graphs."""

from collections.abc import Callable, Generator, Mapping
from contextlib import contextmanager, ExitStack
from importlib.resources import as_file
from importlib.resources.abc import Traversable
from itertools import pairwise
from math import e, log2, pi
from pathlib import Path
from typing import ClassVar, get_args

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from blender.bpy.commands.operators import Call, Done, Operator, Outcome, Unchanged
from blender.bpy.data.rna.conditions import Job
from blender.bpy.data.rna.descriptors import Descriptor, Value, written
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.events.handlers import PathHandler, UpdateHandler
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Band
from blender.bpy.faults import BlenderAbsentFault, BlenderFaults, BlenderImagingFault, BlenderReadFault, BlenderResult
from blender.bpy.nodes.graphs import End, ended, Fragment, INPUTS, masked, math, menu, mixed, nested, node, NodeGroup, Operand, Part, switched, vector
from blender.bpy.nodes.interface import Interface, item_key, MenuItems, Socket
from blender.bpy.nodes.trees import fingerprinted
from blender.bpy.render.color.luts import Enabled, lut_files, Sampling
from blender.bpy.render.effects.grade.lookup import tabled, tiles
from blender.bpy.render.effects.stack import CameraFrame, CompositorEffect, Coverage, EffectStack, entries, formation, Named, Shell, viewed
from blender.bpy.render.effects.viewers import ViewerRead
from blender.bpy.render.previews.queue import Job as PreviewJob, PreviewQueue, Rank
from blender.bpy.ui.drawing.canvas import Align
from blender.bpy.ui.drawing.charts import TracePlot, Well
from blender.bpy.ui.drawing.handlers import Signal
from blender.bpy.ui.resources.theme import Metrics
from blender.imaging.analysis.metering import Extent, samples_of
from blender.imaging.chain import Effect, GREY, Stage
from blender.imaging.color.config import admitted
from blender.imaging.color.luts import Rational
from blender.imaging.color.processors import applied, DisplayView, processor
from blender.imaging.compositing.blending import Entry
from blender.imaging.effects.generators.noise import Estimate, Precision
from blender.imaging.effects.grade.lookup import Lookup
from blender.imaging.effects.tone import curves
from blender.imaging.effects.tone.curves import Application, Curve, Hable, Logarithmic, Neutral, Photoreceptor, PiecewisePower, ReinhardExtended, RenderingPrimary, Sigmoid, ToneCurve, Tony
from blender.imaging.faults import accumulated, ImagingBufferFault, ImagingResult, ImagingWhiteFault, paired
from blender.imaging.files.images import ImageFile
from blender.imaging.hashing import content_hash, ContentHash
from blender.imaging.parameters import Gated

# --- [MODELS] ---------------------------------------------------------------------------


class _ResponseRequest(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Native render demand retaining its handles and observed source state."""

    scene: bpy.types.Scene
    entries: tuple[bpy.types.SceneCompositorEffect, ...]
    guide: PyOpenColorIO.CPUProcessor
    window: tuple[float, float]
    columns: int
    caption: str
    observed: tuple[tuple[Snapshot, ...], tuple[Option[ContentHash], ...], tuple[int, int, float, int], str, str, str, tuple[float, float], tuple[str, ...]]
    content: Option[ContentHash]
    guide_levels: ClassVar[tuple[float, ...]] = (*tuple(GREY * 2.0**stop for stop in range(-4, 3)), 1.0)


# --- [SERVICES] -------------------------------------------------------------------------


class Response[E: Effect](msgspec.Struct, kw_only=True, eq=False):
    """Scoped native response sampling outside drawing, with a read-only chart."""

    stack: EffectStack[E]
    sampling: Sampling
    selection: tuple[Stage | type[Effect], ...]
    allocation: PyOpenColorIO.AllocationTransform | str
    caption: str
    guide: Option[PyOpenColorIO.Transform] = Nothing
    enabled: Enabled = Enabled.RENDER
    signal: Signal = msgspec.field(default_factory=Signal)
    columns: int = 0
    wanted: Option[_ResponseRequest] = Nothing
    cached: Option[tuple[_ResponseRequest, npt.NDArray[np.float32], tuple[float, ...]]] = Nothing

    # --- [READOUT]

    def chart(self, metrics: Metrics) -> BlenderResult[Option[TracePlot]]:
        """Reads cached RGB responses and records the device-column demand."""
        self.columns = round(Well.scope(metrics).plot.width)

        def plotted(held: tuple[_ResponseRequest, npt.NDArray[np.float32], tuple[float, ...]]) -> TracePlot:
            request, values, guides = held
            x = (np.arange(request.columns, dtype=np.float32) + 0.5) / request.columns
            return TracePlot.of(
                metrics,
                traces=((np.column_stack((x, values.mean(axis=1))).astype(np.float32), (1.0, 1.0, 1.0, 0.9)),),
                verticals=(),
                horizontals=guides,
                labels=tuple((f"{level:.2%}", 0.0, code, Align.START) for level, code in zip(request.guide_levels, guides)),
                caption=request.caption,
            )

        return Ok(self.cached.filter(lambda held: self.wanted.filter(lambda request: held[0] is request and request.columns == self.columns).is_some()).map(plotted))

    # --- [DEMAND]

    def fed(self, identity: Identity, context: bpy.types.Context) -> BlenderResult[tuple[PreviewJob, ...]]:
        """Returns current native demand, retaining unobservable request identity."""
        if not self.columns:
            return Ok(())

        def allocated(config: PyOpenColorIO.Config) -> BlenderResult[PyOpenColorIO.Processor]:
            def walked(transform: PyOpenColorIO.Transform, *, inverse: bool) -> Generator[tuple[PyOpenColorIO.AllocationTransform, bool]]:
                if isinstance(transform, PyOpenColorIO.AllocationTransform):
                    yield transform, inverse
                elif isinstance(transform, PyOpenColorIO.GroupTransform):
                    for member in transform:
                        yield from walked(member, inverse=inverse ^ (transform.getDirection() == PyOpenColorIO.TRANSFORM_DIR_INVERSE))

            def selected(space: PyOpenColorIO.ColorSpace) -> BlenderResult[PyOpenColorIO.Processor]:
                transform = (
                    Option
                    .of_optional(space.getTransform(PyOpenColorIO.COLORSPACE_DIR_FROM_REFERENCE))
                    .map(lambda value: (value, PyOpenColorIO.TRANSFORM_DIR_FORWARD))
                    .or_else_with(lambda: Option.of_optional(space.getTransform(PyOpenColorIO.COLORSPACE_DIR_TO_REFERENCE)).map(lambda value: (value, PyOpenColorIO.TRANSFORM_DIR_INVERSE)))
                )
                candidates = tuple(pair for value, direction in transform.to_list() for pair in walked(value, inverse=direction == PyOpenColorIO.TRANSFORM_DIR_INVERSE))
                match candidates:
                    case ((value, inverse),):
                        return processor(config, PyOpenColorIO.GroupTransform([value], direction=PyOpenColorIO.TRANSFORM_DIR_FORWARD if inverse else PyOpenColorIO.TRANSFORM_DIR_INVERSE)).map_error(
                            BlenderImagingFault.lifted
                        )
                    case _:
                        return Error((BlenderAbsentFault(owner=space.getName(), key=PyOpenColorIO.AllocationTransform.__name__),))

            match self.allocation:
                case str(name):
                    return Result[PyOpenColorIO.ColorSpace, BlenderFaults].bind(
                        Option.of_optional(config.getColorSpace(name)).to_result_with(lambda: (BlenderAbsentFault(owner=config.getName(), key=name),)), selected
                    )
                case transform:
                    return processor(config, PyOpenColorIO.GroupTransform([transform], direction=PyOpenColorIO.TRANSFORM_DIR_INVERSE)).map_error(BlenderImagingFault.lifted)

        def prepared(scene: bpy.types.Scene, config: PyOpenColorIO.Config) -> BlenderResult[tuple[PreviewJob, ...]]:
            def guided() -> BlenderResult[PyOpenColorIO.Processor]:
                def selected(held: Option[DisplayView]) -> BlenderResult[PyOpenColorIO.Processor]:
                    display = held.map(lambda record: record.display).default_with(lambda: viewed(scene).display)
                    return Result[Option[tuple[DisplayView, PyOpenColorIO.Processor]], BlenderFaults].bind(
                        DisplayView.for_encoding(config, display).map_error(BlenderImagingFault.lifted),
                        lambda found: found.map(lambda pair: pair[1]).to_result_with(lambda: (BlenderAbsentFault(owner=display, key=DisplayView.for_encoding.__name__),)),
                    )

                return self.guide.map(lambda transform: processor(config, transform).map_error(BlenderImagingFault.lifted)).default_with(lambda: formation.current(identity, scene).bind(selected))

            rows = tuple(
                (held.entry, row)
                for held in self.stack.held(identity, scene)
                for row in held.row.to_list()
                if bool(self.enabled.descriptor.read(held.entry)) and (row.record in self.selection or row.record.stage in self.selection)
            )
            selected = tuple(entry for entry, _row in rows)
            locality = accumulated(row.read(identity, entry).map(lambda held: held.effect.pointwise) for entry, row in rows)
            observations = accumulated(paired(Snapshot.capture(entry), Option.of_optional(entry.node_group).map(fingerprinted).default_value(Ok(Nothing))) for entry in selected)
            encoding = paired(guided(), allocated(config)).map(lambda built: tuple(value.getDefaultCPUProcessor() for value in built))

            def requested(parts: tuple[tuple[bool, ...], tuple[tuple[Snapshot, Option[ContentHash]], ...], tuple[PyOpenColorIO.CPUProcessor, ...]]) -> BlenderResult[tuple[PreviewJob, ...]]:
                local, rows, processors = parts
                guide, allocation = processors
                low, high = np.log2(applied(allocation, np.repeat(np.asarray((0.0, 1.0), dtype=np.float32)[:, None], 3, axis=1))[:, 0] / GREY)
                window = float(low), float(high)
                if not all(local):
                    return self.cleared()
                snapshots, fingerprints = tuple(row[0] for row in rows), tuple(row[1] for row in rows)
                frame = scene.session_uid, scene.frame_current, scene.frame_subframe, self.columns
                observed = snapshots, fingerprints, frame, config.getCacheID(), guide.getCacheID(), self.caption, window, tuple(entry.name for entry in selected)
                content = (
                    Some(
                        content_hash(
                            Response,
                            *snapshots,
                            *(key for value in fingerprints for key in value.to_list()),
                            arrays=(np.asarray((*frame, *window), dtype=np.float64), np.frombuffer(guide.getCacheID().encode(), dtype=np.uint8), np.frombuffer(self.caption.encode(), dtype=np.uint8)),
                            config=Some(config),
                        )
                    )
                    if all(value.is_some() for value in fingerprints)
                    else Nothing
                )
                request = self.wanted.filter(lambda held: held.observed == observed and held.entries == selected).default_with(
                    lambda: _ResponseRequest(scene=scene, entries=selected, guide=guide, window=window, columns=self.columns, caption=self.caption, observed=observed, content=content)
                )
                changed = self.wanted.filter(lambda held: held is request).is_none()
                self.wanted = Some(request)
                if self.cached.filter(lambda held: held[0] is request).is_some():
                    return Ok(())
                if changed:
                    self.cached = Nothing
                return (self.signal.fired() if changed else Ok(None)).map(
                    lambda _: (PreviewJob(place=self, key=request.content.default_value(request), rank=Rank.SHOWN, run=lambda: self.rendered(identity, request)),)
                )

            return paired(paired(locality, observations), encoding).map(lambda pair: (*pair[0], pair[1])).bind(requested)

        return (
            self.stack
            .config()
            .bind(lambda config: Option.of_optional(context.scene).map(lambda scene: prepared(scene, config)).default_with(self.cleared))
            .or_else_with(lambda faults: paired(Error(faults), self.cleared()).map(lambda _: ()))
        )

    def rendered(self, identity: Identity, request: _ResponseRequest) -> BlenderResult[None]:
        """Samples the supplied native chain and publishes only its current demand."""
        positions = (np.arange(request.columns, dtype=np.float32) + 0.5) / request.columns
        low, high = request.window
        levels = (GREY * np.exp2(low + (high - low) * positions)).astype(np.float32)
        plane = np.repeat(levels[None, :, None], 3, axis=2)
        guides = np.asarray(request.guide_levels, dtype=np.float32)
        codes = tuple(map(float, applied(request.guide, np.repeat(guides[:, None], 3, axis=1)).mean(axis=1)))

        def published(values: npt.NDArray[np.float32]) -> BlenderResult[None]:
            if self.wanted.filter(lambda current: current is request).is_none():
                return Ok(None)
            self.cached = Some((request, values[0, :, :3], codes))
            return self.signal.fired()

        return self.sampling.sampled(identity, request.scene, request.entries, plane).bind(published)

    # --- [LIFETIME]

    def cleared(self) -> BlenderResult[tuple[PreviewJob, ...]]:
        """Withdraws the current source and its cached response."""
        changed = self.wanted.is_some() or self.cached.is_some()
        self.wanted, self.cached = Nothing, Nothing
        return (self.signal.fired() if changed else Ok(None)).map(lambda _: ())

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[BlenderResult[Acquisitions]]:
        """Holds the queue feed and retires native invalidation callbacks before state."""
        with ExitStack() as scopes:

            def invalidated() -> BlenderResult[Option[str]]:
                return self.cleared().map(lambda _: Nothing) if self.wanted.filter(lambda request: request.content.is_none()).is_some() else Ok(Nothing)

            later = scopes.enter_context(identity.reports.deferred(invalidated))

            @contextmanager
            def mounted() -> Generator[None]:
                try:
                    with PreviewQueue.fed(identity, self.fed):
                        yield
                finally:
                    scopes.close()
                    self.columns, self.wanted, self.cached = 0, Nothing, Nothing

            def loaded(_path: Option[Path]) -> BlenderResult[None]:
                identity.reports.cancelled(later)
                return self.cleared().map(lambda _: None)

            yield Ok((
                (Band.RESOURCES, mounted()),
                PathHandler(handlers=bpy.app.handlers.load_post, body=loaded),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                UpdateHandler(
                    handlers=bpy.app.handlers.depsgraph_update_post,
                    ids=(bpy.types.Scene, bpy.types.NodeTree, bpy.types.Image),
                    body=lambda scene, _updates: Ok(later()) if self.wanted.filter(lambda request: request.scene == scene).is_some() else Ok(None),
                ),
            ))


# --- [OPERATIONS] -----------------------------------------------------------------------


def nonnegative(color: Operand) -> BlenderResult[tuple[Fragment, End]]:
    """Desaturates negative channels toward their non-negative mean."""
    return Fragment.joined(
        node("negative_channels", bpy.types.ShaderNodeSeparateXYZ, {"Vector": color}, {}),
        vector("mean", "DOT_PRODUCT", color, (1.0 / 3.0,) * 3),
        math("level", "MAXIMUM", End(node="mean"), 0.0),
        math("low_pair", "MINIMUM", End(node="negative_channels", socket="X"), End(node="negative_channels", socket="Y")),
        math("low", "MINIMUM", End(node="low_pair"), End(node="negative_channels", socket="Z")),
        math("depth", "SUBTRACT", End(node="level"), End(node="low")),
        math("reach", "DIVIDE", End(node="level"), End(node="depth")),
        math("keep", "MINIMUM", End(node="reach"), 1.0),
        mixed("lifted", "VECTOR", End(node="keep"), End(node="level"), color),
    ).map(ended("lifted", "Result_Vector"))


def curved(shell: Shell[ToneCurve]) -> BlenderResult[tuple[Fragment, End]]:
    """Applies a live per-channel curve over admitted native rendering primaries."""
    curve, _, application = ToneCurve.__struct_fields__
    preserve_hue, red, green, blue, purity_field = Application.__struct_fields__
    config, record, source = shell.config, shell.record, shell.source
    if isinstance(record.curve, Logarithmic | ReinhardExtended) and not record.curve.scene_white.enabled:
        return Error((BlenderAbsentFault(owner=record.title, key="current_frame_white"),))
    keys = (red, green, blue)
    axes = tuple("XYZ")
    kinds: tuple[type[Curve], ...] = get_args(Curve.__value__)

    def scalar(kind: type[Curve]) -> BlenderResult[NodeGroup]:
        value = End(node=INPUTS, socket="value")
        _, value_field = Gated.__struct_fields__

        def sigmoid() -> tuple[Part, ...]:
            contrast, skew, white, black = (End(node=INPUTS, socket=field) for field in Sigmoid.__struct_fields__)
            return (
                math("skew_power", "POWER", 0.2, skew),
                math("inverse_skew", "DIVIDE", 1.0, End(node="skew_power")),
                math("grey_share", "DIVIDE", GREY, white),
                math("grey_root", "POWER", End(node="grey_share"), End(node="inverse_skew")),
                math("grey_gap", "SUBTRACT", 1.0, End(node="grey_root")),
                math("slope", "MULTIPLY", End(node="skew_power"), End(node="grey_gap")),
                math("contrast_share", "MULTIPLY", contrast, 1.0 - GREY),
                math("film_power", "DIVIDE", End(node="contrast_share"), End(node="slope")),
                math("white_relation", "DIVIDE", End(node="grey_gap"), End(node="grey_root")),
                math("black_share", "DIVIDE", black, white),
                math("black_root", "POWER", End(node="black_share"), End(node="inverse_skew")),
                math("inverse_film", "DIVIDE", 1.0, End(node="film_power")),
                math("black_white", "MULTIPLY", End(node="white_relation"), End(node="black_root")),
                math("black_gap", "SUBTRACT", 1.0, End(node="black_root")),
                math("fog_ratio", "DIVIDE", End(node="black_white"), End(node="black_gap")),
                math("fog_root", "POWER", End(node="fog_ratio"), End(node="inverse_film")),
                math("fog_span", "SUBTRACT", 1.0, End(node="fog_root")),
                math("fog_grey", "MULTIPLY", End(node="fog_root"), GREY),
                math("fog", "DIVIDE", End(node="fog_grey"), End(node="fog_span")),
                math("fogged_grey", "ADD", End(node="fog"), GREY),
                math("clipped", "MAXIMUM", value, 0.0),
                math("fogged", "ADD", End(node="fog"), End(node="clipped")),
                math("relative", "DIVIDE", End(node="fogged"), End(node="fogged_grey")),
                math("positive_power", "POWER", End(node="relative"), End(node="film_power")),
                math("low_denominator", "ADD", End(node="white_relation"), End(node="positive_power")),
                math("low_ratio", "DIVIDE", End(node="positive_power"), End(node="low_denominator")),
                math("negative_film", "MULTIPLY", End(node="film_power"), -1.0),
                math("negative_power", "POWER", End(node="relative"), End(node="negative_film")),
                math("high_denominator", "MULTIPLY_ADD", End(node="white_relation"), End(node="negative_power"), 1.0),
                math("high_ratio", "DIVIDE", 1.0, End(node="high_denominator")),
                math("above_grey", "GREATER_THAN", End(node="relative"), 1.0),
                masked("ratio", "FLOAT", End(node="above_grey"), End(node="low_ratio"), End(node="high_ratio")),
                math("shaped", "POWER", End(node="ratio"), End(node="skew_power")),
                math("curve", "MULTIPLY", End(node="shaped"), white),
            )

        def piecewise() -> tuple[Part, ...]:
            toe_strength, toe_length, shoulder_strength, shoulder_length, shoulder_angle, gamma = (End(node=INPUTS, socket=field) for field in PiecewisePower.__struct_fields__)
            return (
                math("toe_shape", "POWER", toe_length, 2.2),
                math("toe_end", "MULTIPLY", End(node="toe_shape"), 0.5),
                math("toe_keep", "SUBTRACT", 1.0, toe_strength),
                math("toe_height", "MULTIPLY", End(node="toe_keep"), End(node="toe_end")),
                math("toe_room", "SUBTRACT", 1.0, End(node="toe_height")),
                math("shoulder_keep", "SUBTRACT", 1.0, shoulder_length),
                math("linear_span", "MULTIPLY", End(node="shoulder_keep"), End(node="toe_room")),
                math("shoulder_start", "ADD", End(node="toe_end"), End(node="linear_span")),
                math("shoulder_height", "ADD", End(node="toe_height"), End(node="linear_span")),
                math("headroom", "POWER", 2.0, shoulder_strength),
                math("white_room", "SUBTRACT", End(node="headroom"), 1.0),
                math("white_base", "ADD", End(node="toe_end"), End(node="toe_room")),
                math("white", "ADD", End(node="white_base"), End(node="white_room")),
                math("overshoot", "MULTIPLY", shoulder_angle, shoulder_strength),
                math("overshoot_x", "MULTIPLY", End(node="overshoot"), 2.0),
                math("overshoot_y", "MULTIPLY", End(node="overshoot"), 0.5),
                math("normalized", "DIVIDE", value, End(node="white")),
                math("toe_x", "DIVIDE", End(node="toe_end"), End(node="white")),
                math("shoulder_x", "DIVIDE", End(node="shoulder_start"), End(node="white")),
                math("intercept", "SUBTRACT", End(node="toe_height"), End(node="toe_end")),
                math("line_base", "MULTIPLY_ADD", End(node="white"), End(node="normalized"), End(node="intercept")),
                math("line", "POWER", End(node="line_base"), gamma),
                math("toe_raised", "POWER", End(node="toe_height"), gamma),
                math("toe_level", "MAXIMUM", End(node="toe_raised"), 1e-5),
                math("shoulder_raised", "POWER", End(node="shoulder_height"), gamma),
                math("shoulder_level", "MAXIMUM", End(node="shoulder_raised"), 1e-5),
                math("ceiling_base", "ADD", End(node="overshoot_y"), 1.0),
                math("ceiling", "POWER", End(node="ceiling_base"), gamma),
                math("inverse_gamma", "DIVIDE", 1.0, gamma),
                math("slope_power", "SUBTRACT", 1.0, End(node="inverse_gamma")),
                math("gamma_slope", "MULTIPLY", gamma, End(node="white")),
                *(
                    part
                    for side in ("toe", "shoulder")
                    for part in (
                        math(f"{side}_slope_base", "POWER", End(node=f"{side}_level"), End(node="slope_power")),
                        math(f"{side}_slope", "MULTIPLY", End(node="gamma_slope"), End(node=f"{side}_slope_base")),
                    )
                ),
                math("toe_power_base", "MULTIPLY", End(node="toe_slope"), End(node="toe_x")),
                math("toe_exponent", "DIVIDE", End(node="toe_power_base"), End(node="toe_level")),
                math("toe_ratio", "DIVIDE", End(node="normalized"), End(node="toe_x")),
                math("toe_power", "POWER", End(node="toe_ratio"), End(node="toe_exponent")),
                math("toe", "MULTIPLY", End(node="toe_power"), End(node="toe_level")),
                math("origin", "ADD", End(node="overshoot_x"), 1.0),
                math("shoulder_run", "SUBTRACT", End(node="origin"), End(node="shoulder_x")),
                math("shoulder_rise", "SUBTRACT", End(node="ceiling"), End(node="shoulder_level")),
                math("shoulder_power_base", "MULTIPLY", End(node="shoulder_slope"), End(node="shoulder_run")),
                math("shoulder_exponent", "DIVIDE", End(node="shoulder_power_base"), End(node="shoulder_rise")),
                math("shoulder_left", "SUBTRACT", End(node="origin"), End(node="normalized")),
                math("shoulder_ratio", "DIVIDE", End(node="shoulder_left"), End(node="shoulder_run")),
                math("shoulder_power", "POWER", End(node="shoulder_ratio"), End(node="shoulder_exponent")),
                math("shoulder_drop", "MULTIPLY", End(node="shoulder_power"), End(node="shoulder_rise")),
                math("shoulder", "SUBTRACT", End(node="ceiling"), End(node="shoulder_drop")),
                math("unit_left", "SUBTRACT", End(node="origin"), 1.0),
                math("unit_ratio", "DIVIDE", End(node="unit_left"), End(node="shoulder_run")),
                math("unit_power", "POWER", End(node="unit_ratio"), End(node="shoulder_exponent")),
                math("unit_drop", "MULTIPLY", End(node="unit_power"), End(node="shoulder_rise")),
                math("unit_shoulder", "SUBTRACT", End(node="ceiling"), End(node="unit_drop")),
                math("below_origin", "LESS_THAN", End(node="normalized"), End(node="origin")),
                math("below_shoulder", "LESS_THAN", End(node="normalized"), End(node="shoulder_x")),
                math("below_toe", "LESS_THAN", End(node="normalized"), End(node="toe_x")),
                math("above_zero", "GREATER_THAN", End(node="normalized"), 0.0),
                masked("pick_shoulder", "FLOAT", End(node="below_origin"), End(node="ceiling"), End(node="shoulder")),
                masked("pick_line", "FLOAT", End(node="below_shoulder"), End(node="pick_shoulder"), End(node="line")),
                masked("pick_toe", "FLOAT", End(node="below_toe"), End(node="pick_line"), End(node="toe")),
                masked("pick_black", "FLOAT", End(node="above_zero"), 0.0, End(node="pick_toe")),
                math("curve", "DIVIDE", End(node="pick_black"), End(node="unit_shoulder")),
            )

        def logarithmic() -> tuple[Part, ...]:
            bias, scene_white, display_white = Logarithmic.__struct_fields__
            return (
                math("white_level", "POWER", 2.0, End(node=INPUTS, socket=item_key(scene_white, value_field))),
                math("exponent", "LOGARITHM", End(node=INPUTS, socket=bias), 0.5),
                math("clipped", "MAXIMUM", value, 0.0),
                *(
                    part
                    for key, operand in (("numerator", End(node="clipped")), ("white_natural", End(node="white_level")))
                    for part in (
                        math(f"{key}_lifted", "ADD", operand, 1.0),
                        math(f"{key}_delta", "SUBTRACT", End(node=f"{key}_lifted"), 1.0),
                        math(f"{key}_log", "LOGARITHM", End(node=f"{key}_lifted"), e),
                        math(f"{key}_correction", "DIVIDE", operand, End(node=f"{key}_delta")),
                        math(f"{key}_compensated", "MULTIPLY", End(node=f"{key}_log"), End(node=f"{key}_correction")),
                        math(f"{key}_distinct", "GREATER_THAN", End(node=f"{key}_delta"), 0.0),
                        masked(key, "FLOAT", End(node=f"{key}_distinct"), operand, End(node=f"{key}_compensated")),
                    )
                ),
                math("white_log", "DIVIDE", End(node="white_natural"), float(np.log(10.0))),
                math("relative", "DIVIDE", End(node="clipped"), End(node="white_level")),
                math("shaped", "POWER", End(node="relative"), End(node="exponent")),
                math("base", "MULTIPLY_ADD", End(node="shaped"), 8.0, 2.0),
                math("base_log", "LOGARITHM", End(node="base"), e),
                math("denominator", "MULTIPLY", End(node="white_log"), End(node="base_log")),
                math("ratio", "DIVIDE", End(node="numerator"), End(node="denominator")),
                math("curve", "MULTIPLY", End(node="ratio"), End(node=INPUTS, socket=display_white)),
            )

        def reinhard() -> tuple[Part, ...]:
            (scene_white,) = ReinhardExtended.__struct_fields__
            return (
                math("white_stops", "MULTIPLY", End(node=INPUTS, socket=item_key(scene_white, value_field)), 2.0),
                math("white_square", "POWER", 2.0, End(node="white_stops")),
                math("denominator", "ADD", value, 1.0),
                math("compressed", "DIVIDE", value, End(node="denominator")),
                math("white_share", "DIVIDE", value, End(node="white_square")),
                math("headroom", "ADD", End(node="white_share"), 1.0),
                math("curve", "MULTIPLY", End(node="compressed"), End(node="headroom")),
            )

        parts = sigmoid() if kind is Sigmoid else piecewise() if kind is PiecewisePower else logarithmic() if kind is Logarithmic else reinhard()
        return paired(Interface.of(kind), Fragment.joined(*parts)).bind(
            lambda pair: NodeGroup.built(
                item_key("tone", kind),
                kind.title,
                msgspec.structs.replace(
                    pair[0], outputs=(Socket(key="curve", name="Curve", kind=bpy.types.NodeSocketFloat),), inputs=(Socket(key="value", name="Value", kind=bpy.types.NodeSocketFloat), *pair[0].inputs)
                ),
                {"curve": (pair[1], End(node="curve"))},
            )
        )

    def primary() -> BlenderResult[NodeGroup]:
        point, white = End(node=INPUTS, socket="point"), End(node=INPUTS, socket="white")
        inset, rotation, purity = (End(node=INPUTS, socket=field) for field in (*RenderingPrimary.__struct_fields__, purity_field))
        first, *_ = keys
        edges = tuple(pairwise((*keys, first)))
        inputs = {key: End(node=INPUTS, socket=key) for key in keys}
        first_reach, second_reach, last_reach = (End(node=f"{a}_{b}_reach") for a, b in edges)
        graph = Fragment.joined(
            vector("direction", "SUBTRACT", point, white),
            math("angle", "MULTIPLY", rotation, End(node=INPUTS, socket="rotation_limit")),
            node("turned", bpy.types.ShaderNodeVectorRotate, {"Vector": End(node="direction", socket="Vector"), "Angle": End(node="angle")}, {"rotation_type": "Z_AXIS"}),
            *(
                part
                for a, b in edges
                for part in (
                    vector(f"{a}_{b}", "SUBTRACT", inputs[b], inputs[a]),
                    vector(f"{a}_{b}_normal", "CROSS_PRODUCT", End(node=f"{a}_{b}", socket="Vector"), (0.0, 0.0, 1.0)),
                    vector(f"{a}_{b}_offset", "SUBTRACT", inputs[a], white),
                    vector(f"{a}_{b}_turn", "CROSS_PRODUCT", End(node=f"{a}_{b}_offset", socket="Vector"), End(node=f"{a}_{b}", socket="Vector")),
                    vector(f"{a}_{b}_span", "DOT_PRODUCT", End(node=f"{a}_{b}_turn", socket="Vector"), (0.0, 0.0, 1.0)),
                    vector(f"{a}_{b}_along", "DOT_PRODUCT", End(node="turned", socket="Vector"), End(node=f"{a}_{b}_normal", socket="Vector")),
                    math(f"{a}_{b}_reach", "DIVIDE", End(node=f"{a}_{b}_along"), End(node=f"{a}_{b}_span")),
                )
            ),
            math("reach_pair", "MAXIMUM", first_reach, second_reach),
            math("reach", "MAXIMUM", End(node="reach_pair"), last_reach),
            math("distance", "DIVIDE", 1.0, End(node="reach")),
            vector("edge", "MULTIPLY_ADD", End(node="turned", socket="Vector"), End(node="distance"), white),
            math("inset_scale", "SUBTRACT", 1.0, inset),
            math("purity_share", "MULTIPLY", purity, inset),
            math("outset_scale", "SUBTRACT", 1.0, End(node="purity_share")),
            *(mixed(f"{side}_point", "VECTOR", End(node=f"{side}_scale"), white, End(node="edge", socket="Vector")) for side in ("inset", "outset")),
        )
        return paired(paired(Interface.of(RenderingPrimary), Interface.of(Application)), graph).bind(
            lambda pair: NodeGroup.built(
                "tone_primary",
                "Rendering Primary",
                Interface(
                    outputs=tuple(Socket(key=f"{side}_point", name=f"{side.title()} Point", kind=bpy.types.NodeSocketVector) for side in ("inset", "outset")),
                    inputs=(
                        *(Socket(key=key, name=key.title(), kind=bpy.types.NodeSocketVector) for key in ("point", *keys, "white")),
                        Socket(key="rotation_limit", name="Rotation Limit", kind=bpy.types.NodeSocketFloat),
                        *pair[0][0].inputs,
                        *(socket for socket in pair[0][1].inputs if socket.key == purity_field),
                    ),
                ),
                {f"{side}_point": (pair[1], End(node=f"{side}_point", socket="Result_Vector")) for side in ("inset", "outset")},
            )
        )

    def matrix() -> BlenderResult[NodeGroup]:
        return Fragment.joined(
            *(
                part
                for key in keys
                for part in (
                    node(f"{key}_parts", bpy.types.ShaderNodeSeparateXYZ, {"Vector": End(node=INPUTS, socket=key)}, {}),
                    math(f"{key}_x", "DIVIDE", End(node=f"{key}_parts", socket="X"), End(node=f"{key}_parts", socket="Y")),
                    math(f"{key}_rest", "SUBTRACT", 1.0, End(node=f"{key}_parts", socket="X")),
                    math(f"{key}_z_base", "SUBTRACT", End(node=f"{key}_rest"), End(node=f"{key}_parts", socket="Y")),
                    math(f"{key}_z", "DIVIDE", End(node=f"{key}_z_base"), End(node=f"{key}_parts", socket="Y")),
                )
            ),
            node(
                "columns",
                bpy.types.FunctionNodeCombineMatrix,
                {
                    **{f"Column {column} Row {row}": End(node=f"{key}_{axis}") for column, key in enumerate(keys, 1) for row, axis in ((1, "x"), (3, "z"))},
                    **{f"Column {column} Row 2": 1.0 for column in range(1, len(keys) + 1)},
                },
                {},
            ),
            node("inverse", bpy.types.FunctionNodeInvertMatrix, {"Matrix": End(node="columns", socket="Matrix")}, {}),
            node("scales", bpy.types.FunctionNodeTransformDirection, {"Direction": End(node=INPUTS, socket="white"), "Transform": End(node="inverse", socket="Matrix")}, {}),
            node("scale_parts", bpy.types.ShaderNodeSeparateXYZ, {"Vector": End(node="scales", socket="Direction")}, {}),
            node("diagonal", bpy.types.FunctionNodeCombineMatrix, {f"Column {column} Row {column}": End(node="scale_parts", socket=axis) for column, axis in enumerate(axes, 1)}, {}),
            node("normalized", bpy.types.FunctionNodeMatrixMultiply, {"Matrix": End(node="columns", socket="Matrix"), "Matrix_001": End(node="diagonal", socket="Matrix")}, {}),
        ).bind(
            lambda graph: NodeGroup.built(
                "tone_matrix",
                "Rendering Matrix",
                Interface(
                    outputs=(Socket(key="matrix", name="Matrix", kind=bpy.types.NodeSocketMatrix),),
                    inputs=tuple(Socket(key=key, name=key.title(), kind=bpy.types.NodeSocketVector) for key in (*keys, "white")),
                ),
                {"matrix": (graph, End(node="normalized", socket="Matrix"))},
            )
        )

    def preserved(rendering: Operand, shaped: Operand, hue: Operand) -> BlenderResult[tuple[Fragment, End]]:
        return Fragment.joined(
            *(node(key, bpy.types.ShaderNodeSeparateXYZ, {"Vector": color}, {}) for key, color in (("r_channels", rendering), ("f_channels", shaped))),
            math("xy_order", "GREATER_THAN", End(node="r_channels", socket="X"), End(node="r_channels", socket="Y")),
            *(
                part
                for key in ("r", "f")
                for part in (
                    masked(f"{key}_low", "FLOAT", End(node="xy_order"), End(node=f"{key}_channels", socket="X"), End(node=f"{key}_channels", socket="Y")),
                    masked(f"{key}_high", "FLOAT", End(node="xy_order"), End(node=f"{key}_channels", socket="Y"), End(node=f"{key}_channels", socket="X")),
                )
            ),
            math("z_below", "LESS_THAN", End(node="r_channels", socket="Z"), End(node="r_low")),
            math("z_above", "GREATER_THAN", End(node="r_channels", socket="Z"), End(node="r_high")),
            *(
                part
                for key in ("r", "f")
                for part in (
                    masked(f"{key}_min", "FLOAT", End(node="z_below"), End(node=f"{key}_low"), End(node=f"{key}_channels", socket="Z")),
                    masked(f"{key}_max", "FLOAT", End(node="z_above"), End(node=f"{key}_high"), End(node=f"{key}_channels", socket="Z")),
                    masked(f"{key}_middle_base", "FLOAT", End(node="z_below"), End(node=f"{key}_channels", socket="Z"), End(node=f"{key}_low")),
                    masked(f"{key}_mid", "FLOAT", End(node="z_above"), End(node=f"{key}_middle_base"), End(node=f"{key}_high")),
                )
            ),
            vector("f_sum", "DOT_PRODUCT", shaped, (1.0,) * 3),
            math("f_ends", "ADD", End(node="f_min"), End(node="f_max")),
            math("mid_rise", "SUBTRACT", End(node="r_mid"), End(node="r_min")),
            math("span", "SUBTRACT", End(node="r_max"), End(node="r_min")),
            math("m", "DIVIDE", End(node="mid_rise"), End(node="span")),
            math("f_span", "SUBTRACT", End(node="f_max"), End(node="f_min")),
            math("hue_mid", "MULTIPLY_ADD", End(node="f_span"), End(node="m"), End(node="f_min")),
            mixed("naive", "FLOAT", hue, End(node="f_mid"), End(node="hue_mid")),
            math("low_sum", "ADD", End(node="r_min"), End(node="r_mid")),
            math("low_share", "DIVIDE", End(node="r_min"), End(node="low_sum")),
            math("k", "MULTIPLY", End(node="low_share"), 2.0),
            math("kept", "ADD", End(node="f_ends"), End(node="naive", socket="Result_Float")),
            mixed("total", "FLOAT", End(node="k"), End(node="kept"), End(node="f_sum")),
            math("rest_low", "SUBTRACT", End(node="total", socket="Result_Float"), End(node="f_max")),
            mixed("blend_low", "FLOAT", End(node="m"), End(node="rest_low"), End(node="f_max")),
            mixed("numerator_low", "FLOAT", hue, End(node="f_mid"), End(node="blend_low", socket="Result_Float")),
            math("m_rest", "SUBTRACT", 1.0, End(node="m")),
            math("denominator_low", "MULTIPLY_ADD", hue, End(node="m_rest"), 1.0),
            math("mid_low", "DIVIDE", End(node="numerator_low", socket="Result_Float"), End(node="denominator_low")),
            math("min_low", "SUBTRACT", End(node="rest_low"), End(node="mid_low")),
            math("rest_high", "SUBTRACT", End(node="total", socket="Result_Float"), End(node="f_min")),
            mixed("blend_high", "FLOAT", End(node="m"), End(node="f_min"), End(node="rest_high")),
            mixed("numerator_high", "FLOAT", hue, End(node="f_mid"), End(node="blend_high", socket="Result_Float")),
            math("denominator_high", "MULTIPLY_ADD", hue, End(node="m"), 1.0),
            math("mid_high", "DIVIDE", End(node="numerator_high", socket="Result_Float"), End(node="denominator_high")),
            math("max_high", "SUBTRACT", End(node="rest_high"), End(node="mid_high")),
            math("high", "GREATER_THAN", End(node="naive", socket="Result_Float"), End(node="f_mid")),
            masked("middle", "FLOAT", End(node="high"), End(node="mid_low"), End(node="mid_high")),
            masked("minimum", "FLOAT", End(node="high"), End(node="min_low"), End(node="f_min")),
            masked("maximum", "FLOAT", End(node="high"), End(node="f_max"), End(node="max_high")),
            math("max_gap", "SUBTRACT", End(node="maximum"), End(node="middle")),
            math("min_gap", "SUBTRACT", End(node="middle"), End(node="minimum")),
            math("outer", "ADD", End(node="maximum"), End(node="minimum")),
            math("preserved_base", "SUBTRACT", End(node="outer"), End(node="middle")),
            vector("below_max", "SUBTRACT", rendering, End(node="r_max")),
            vector("max_sign", "SIGN", End(node="below_max", socket="Vector")),
            vector("above_min", "SUBTRACT", rendering, End(node="r_min")),
            vector("min_sign", "SIGN", End(node="above_min", socket="Vector")),
            vector("toward_max", "MULTIPLY_ADD", End(node="max_sign", socket="Vector"), End(node="max_gap"), End(node="preserved_base")),
            vector("preserved", "MULTIPLY_ADD", End(node="min_sign", socket="Vector"), End(node="min_gap"), End(node="toward_max", socket="Vector")),
        ).map(ended("preserved", "Vector"))

    def body(basis: npt.NDArray[np.float64], groups: tuple[NodeGroup, ...]) -> BlenderResult[tuple[Fragment, End]]:
        primaries, matrices, *shapes = groups
        _x, y, _z = np.unstack(basis)
        normalized = basis / y.sum()
        white_xyz = normalized.sum(axis=1)
        points = {key: (float(x), float(y), 0.0) for key, (x, y, _z) in zip(keys, np.unstack(basis / basis.sum(axis=0), axis=1))}
        white_x, white_y, _white_z = map(float, white_xyz / white_xyz.sum())
        white = white_x, white_y, 0.0
        x, y, _z = np.unstack(np.asarray(tuple(points.values())), axis=1)
        angles = np.sort(np.arctan2(y - white_y, x - white_x))
        first_angle, *_ = angles
        limit = float((pi - np.diff(np.append(angles, first_angle + 2.0 * pi)).max()) / 2.0)
        return Fragment.joined(
            nonnegative(source),
            node("base_matrix", bpy.types.FunctionNodeCombineMatrix, {f"Column {column + 1} Row {row + 1}": float(normalized[row, column]) for row in range(3) for column in range(3)}, {}),
            *(
                nested(
                    f"{key}_primary",
                    primaries,
                    {
                        "point": point,
                        **points,
                        "white": white,
                        "rotation_limit": limit,
                        **{field: End(node=INPUTS, socket=item_key(application, key, field)) for field in RenderingPrimary.__struct_fields__},
                        purity_field: End(node=INPUTS, socket=item_key(application, purity_field)),
                    },
                    "inset_point",
                )
                for key, point in points.items()
            ),
            *(
                nested(f"{side}_matrix", matrices, {**{key: End(node=f"{key}_primary", socket=f"{side}_point") for key in keys}, "white": tuple(map(float, white_xyz))}, "matrix")
                for side in ("inset", "outset")
            ),
            node("base_inverse", bpy.types.FunctionNodeInvertMatrix, {"Matrix": End(node="base_matrix", socket="Matrix")}, {}),
            node("into_rendering", bpy.types.FunctionNodeMatrixMultiply, {"Matrix": End(node="base_inverse", socket="Matrix"), "Matrix_001": End(node="inset_matrix", socket="matrix")}, {}),
            node("outset_inverse", bpy.types.FunctionNodeInvertMatrix, {"Matrix": End(node="outset_matrix", socket="matrix")}, {}),
            node("out_of_rendering", bpy.types.FunctionNodeMatrixMultiply, {"Matrix": End(node="outset_inverse", socket="Matrix"), "Matrix_001": End(node="base_matrix", socket="Matrix")}, {}),
            node("rendering", bpy.types.FunctionNodeTransformDirection, {"Direction": End(node="lifted", socket="Result_Vector"), "Transform": End(node="into_rendering", socket="Matrix")}, {}),
            node("rendered", bpy.types.ShaderNodeSeparateXYZ, {"Vector": End(node="rendering", socket="Direction")}, {}),
            *(
                nested(
                    item_key("curve", kind, key),
                    shape,
                    {
                        "value": End(node="rendered", socket=axis),
                        **{
                            address: End(node=INPUTS, socket=item_key(curve, kind, address))
                            for address, item, _parent, _position in shape.interface.walked()
                            if isinstance(item, tuple)
                            for _socket, side in (item,)
                            if side == "INPUT" and address != "value"
                        },
                    },
                    "curve",
                )
                for kind, shape in zip(kinds, shapes)
                for key, axis in zip(keys, axes)
            ),
            *(node(item_key("curve", kind), bpy.types.ShaderNodeCombineXYZ, {axis: End(node=item_key("curve", kind, key), socket="curve") for key, axis in zip(keys, axes)}, {}) for kind in kinds),
            menu("curve", "VECTOR", End(node=INPUTS, socket=curve), MenuItems.of(kinds), {kind.title: End(node=item_key("curve", kind), socket="Vector") for kind in kinds}),
            preserved(End(node="rendering", socket="Direction"), End(node="curve", socket="Output"), End(node=INPUTS, socket=item_key(application, preserve_hue))),
            node("outset", bpy.types.FunctionNodeTransformDirection, {"Direction": End(node="preserved", socket="Vector"), "Transform": End(node="out_of_rendering", socket="Matrix")}, {}),
        ).map(ended("outset", "Direction"))

    basis: BlenderResult[npt.NDArray[np.float64]] = record.base(config).map_error(BlenderImagingFault.lifted)
    return paired(basis, accumulated((primary(), matrix(), *(scalar(kind) for kind in kinds)))).bind(lambda pair: body(*pair))


def neutralized(source: Operand) -> BlenderResult[tuple[Fragment, End]]:
    """Builds Khronos PBR Neutral over straight linear Rec.709 RGB."""
    compression = Neutral.diffuse - Neutral.reflectance
    return nonnegative(source).bind(
        lambda positive: Fragment.joined(
            node("neutral_channels", bpy.types.ShaderNodeSeparateXYZ, {"Vector": positive}, {}),
            math("minimum_pair", "MINIMUM", End(node="neutral_channels", socket="X"), End(node="neutral_channels", socket="Y")),
            math("minimum", "MINIMUM", End(node="minimum_pair"), End(node="neutral_channels", socket="Z")),
            math("offset_curve", "MULTIPLY_ADD", End(node="minimum"), -1.0 / (4.0 * Neutral.reflectance), 1.0),
            math("offset_low", "MULTIPLY", End(node="minimum"), End(node="offset_curve")),
            math("offset_below", "LESS_THAN", End(node="minimum"), 2.0 * Neutral.reflectance),
            masked("offset", "FLOAT", End(node="offset_below"), Neutral.reflectance, End(node="offset_low")),
            vector("shifted", "SUBTRACT", positive, End(node="offset")),
            node("shifted_channels", bpy.types.ShaderNodeSeparateXYZ, {"Vector": End(node="shifted", socket="Vector")}, {}),
            math("peak_pair", "MAXIMUM", End(node="shifted_channels", socket="X"), End(node="shifted_channels", socket="Y")),
            math("peak", "MAXIMUM", End(node="peak_pair"), End(node="shifted_channels", socket="Z")),
            math("peak_room", "ADD", End(node="peak"), 1.0 - 2.0 * compression),
            math("squeeze", "DIVIDE", (1.0 - compression) ** 2, End(node="peak_room")),
            math("compressed_peak", "SUBTRACT", 1.0, End(node="squeeze")),
            math("peak_ratio", "DIVIDE", End(node="compressed_peak"), End(node="peak")),
            vector("compressed", "SCALE", End(node="shifted", socket="Vector"), End(node="peak_ratio")),
            math("peak_drop", "SUBTRACT", End(node="peak"), End(node="compressed_peak")),
            math("fade_base", "MULTIPLY_ADD", End(node="peak_drop"), Neutral.desaturation, 1.0),
            math("fade_inverse", "DIVIDE", 1.0, End(node="fade_base")),
            math("fade", "SUBTRACT", 1.0, End(node="fade_inverse")),
            mixed("desaturated", "VECTOR", End(node="fade"), End(node="compressed", socket="Vector"), End(node="compressed_peak")),
            math("below", "LESS_THAN", End(node="peak"), compression),
            masked("neutral", "VECTOR", End(node="below"), End(node="desaturated", socket="Result_Vector"), End(node="shifted", socket="Vector")),
        ).map(ended("neutral", "Vector"))
    )


def adapted(shell: Shell[Photoreceptor]) -> BlenderResult[tuple[Fragment, End]]:
    """Uses Blender's native photoreceptor with zero reserved for automatic contrast."""
    intensity, contrast, light_adaptation, chromatic_adaptation = Photoreceptor.__struct_fields__
    _, value = Gated.__struct_fields__
    return (
        switched("contrast", "FLOAT", shell.socket(contrast), 0.0, shell.socket(contrast, value))
        .bind(
            lambda selected: node(
                "tonemap",
                bpy.types.CompositorNodeTonemap,
                {
                    "Image": shell.source,
                    "Type": "R/D Photoreceptor",
                    "Contrast": selected,
                    "Intensity": shell.socket(intensity),
                    "Light Adaptation": shell.socket(light_adaptation),
                    "Chromatic Adaptation": shell.socket(chromatic_adaptation),
                },
                {},
            )
        )
        .map(ended("tonemap", "Image"))
    )


def habled(shell: Shell[Hable]) -> BlenderResult[tuple[Fragment, End]]:
    """Applies independent Hable coefficients with live white normalization."""
    if not shell.record.scene_white.enabled:
        return Error((BlenderAbsentFault(owner=shell.record.title, key="current_frame_white"),))
    a_field, b_field, _, d_field, _, f_field, scene_white = Hable.__struct_fields__
    _, value_field = Gated.__struct_fields__
    a, b, d, f = (shell.socket(field) for field in (a_field, b_field, d_field, f_field))
    white = shell.socket(scene_white, value_field)
    axes = tuple("XYZ")
    return Fragment.joined(
        nonnegative(shell.source),
        node("channels", bpy.types.ShaderNodeSeparateXYZ, {"Vector": End(node="lifted", socket="Result_Vector")}, {}),
        *(
            part
            for name, socket in zip(("slope", "intercept"), normalization)
            for part in (
                node(f"{name}_parts", bpy.types.ShaderNodeSeparateXYZ, {"Vector": shell.socket(socket.key)}, {}),
                math(f"{name}_magnitude", "POWER", 2.0, End(node=f"{name}_parts", socket="X")),
                math(name, "MULTIPLY", End(node=f"{name}_magnitude"), End(node=f"{name}_parts", socket="Y")),
            )
        ),
        math("white", "POWER", 2.0, white),
        math("white_middle", "MULTIPLY", End(node="white"), 0.5),
        math("df", "MULTIPLY", d, f),
        math("white_linear", "MULTIPLY_ADD", a, End(node="white"), b),
        math("white_denominator", "MULTIPLY_ADD", End(node="white"), End(node="white_linear"), End(node="df")),
        math("white_denominator_log", "LOGARITHM", End(node="white_denominator"), 2.0),
        *(
            part
            for axis in axes
            for prefix, value in ((axis.lower(), End(node="channels", socket=axis)),)
            for part in (
                math(f"{prefix}_delta", "SUBTRACT", value, End(node="white")),
                math(f"{prefix}_log", "LOGARITHM", value, 2.0),
                math(f"{prefix}_slope_log", "ADD", End(node=f"{prefix}_log"), End(node="slope_parts", socket="X")),
                math(f"{prefix}_intercept_larger", "GREATER_THAN", End(node="intercept_parts", socket="X"), End(node=f"{prefix}_slope_log")),
                *(
                    masked(f"{prefix}_{key}", "FLOAT", End(node=f"{prefix}_intercept_larger"), off, on)
                    for key, off, on in (
                        ("larger", End(node=f"{prefix}_slope_log"), End(node="intercept_parts", socket="X")),
                        ("smaller", End(node="intercept_parts", socket="X"), End(node=f"{prefix}_slope_log")),
                        ("larger_sign", End(node="slope_parts", socket="Y"), End(node="intercept_parts", socket="Y")),
                    )
                ),
                math(f"{prefix}_log_gap", "SUBTRACT", End(node=f"{prefix}_smaller"), End(node=f"{prefix}_larger")),
                math(f"{prefix}_ratio", "POWER", 2.0, End(node=f"{prefix}_log_gap")),
                math(f"{prefix}_sign_product", "MULTIPLY", End(node="slope_parts", socket="Y"), End(node="intercept_parts", socket="Y")),
                math(f"{prefix}_sum_magnitude", "MULTIPLY_ADD", End(node=f"{prefix}_sign_product"), End(node=f"{prefix}_ratio"), 1.0),
                math(f"{prefix}_sum_log", "LOGARITHM", End(node=f"{prefix}_sum_magnitude"), 2.0),
                math(f"{prefix}_black_sum_log", "ADD", End(node=f"{prefix}_larger"), End(node=f"{prefix}_sum_log")),
                math(f"{prefix}_intercept_present", "ABSOLUTE", End(node="intercept_parts", socket="Y")),
                *(
                    masked(f"{prefix}_{key}", "FLOAT", End(node=f"{prefix}_intercept_present"), off, on)
                    for key, off, on in (
                        ("black_factor_log", End(node=f"{prefix}_slope_log"), End(node=f"{prefix}_black_sum_log")),
                        ("black_sign", End(node="slope_parts", socket="Y"), End(node=f"{prefix}_larger_sign")),
                        ("black_magnitude", 1.0, End(node=f"{prefix}_sum_magnitude")),
                    )
                ),
                math(f"{prefix}_white_factor", "MULTIPLY_ADD", End(node=f"{prefix}_delta"), End(node="slope"), 1.0),
                math(f"{prefix}_white_magnitude", "ABSOLUTE", End(node=f"{prefix}_white_factor")),
                math(f"{prefix}_white_factor_log", "LOGARITHM", End(node=f"{prefix}_white_magnitude"), 2.0),
                math(f"{prefix}_white_sign", "SIGN", End(node=f"{prefix}_white_factor")),
                math(f"{prefix}_centered", "GREATER_THAN", value, End(node="white_middle")),
                *(
                    masked(f"{prefix}_{key}", "FLOAT", End(node=f"{prefix}_centered"), End(node=f"{prefix}_black_{suffix}"), End(node=f"{prefix}_white_{suffix}"))
                    for key, suffix in (("factor_log", "factor_log"), ("factor_sign", "sign"), ("factor_magnitude", "magnitude"))
                ),
                math(f"{prefix}_scale", "MAXIMUM", value, d),
                math(f"{prefix}_scaled_value", "DIVIDE", value, End(node=f"{prefix}_scale")),
                math(f"{prefix}_scaled_d", "DIVIDE", d, End(node=f"{prefix}_scale")),
                math(f"{prefix}_scaled_df", "MULTIPLY", End(node=f"{prefix}_scaled_d"), f),
                math(f"{prefix}_linear", "MULTIPLY_ADD", a, value, b),
                math(f"{prefix}_scaled_denominator", "MULTIPLY_ADD", End(node=f"{prefix}_scaled_value"), End(node=f"{prefix}_linear"), End(node=f"{prefix}_scaled_df")),
                math(f"{prefix}_scale_log", "LOGARITHM", End(node=f"{prefix}_scale"), 2.0),
                math(f"{prefix}_scaled_denominator_log", "LOGARITHM", End(node=f"{prefix}_scaled_denominator"), 2.0),
                math(f"{prefix}_denominator_log", "ADD", End(node=f"{prefix}_scale_log"), End(node=f"{prefix}_scaled_denominator_log")),
                math(f"{prefix}_relative_log", "SUBTRACT", End(node=f"{prefix}_log"), white),
                math(f"{prefix}_numerator_log", "ADD", End(node=f"{prefix}_relative_log"), End(node=f"{prefix}_factor_log")),
                math(f"{prefix}_denominator_ratio_log", "SUBTRACT", End(node="white_denominator_log"), End(node=f"{prefix}_denominator_log")),
                math(f"{prefix}_response_log", "ADD", End(node=f"{prefix}_numerator_log"), End(node=f"{prefix}_denominator_ratio_log")),
                math(f"{prefix}_low_magnitude", "POWER", 2.0, End(node=f"{prefix}_response_log")),
                math(f"{prefix}_low_signed", "MULTIPLY", End(node=f"{prefix}_low_magnitude"), End(node=f"{prefix}_factor_sign")),
                math(f"{prefix}_positive", "GREATER_THAN", value, 0.0),
                math(f"{prefix}_factor_nonzero", "GREATER_THAN", End(node=f"{prefix}_factor_magnitude"), 0.0),
                math(f"{prefix}_low_nonzero", "MULTIPLY", End(node=f"{prefix}_positive"), End(node=f"{prefix}_factor_nonzero")),
                masked(f"{prefix}_low", "FLOAT", End(node=f"{prefix}_low_nonzero"), 0.0, End(node=f"{prefix}_low_signed")),
                math(f"{prefix}_inverse", "DIVIDE", 1.0, value),
                math(f"{prefix}_inverse_square", "MULTIPLY", End(node=f"{prefix}_inverse"), End(node=f"{prefix}_inverse")),
                math(f"{prefix}_relative_delta", "DIVIDE", End(node=f"{prefix}_delta"), value),
                math(f"{prefix}_high_black_factor", "MULTIPLY_ADD", End(node="intercept"), End(node=f"{prefix}_inverse"), End(node="slope")),
                math(f"{prefix}_high_white_factor", "MULTIPLY_ADD", End(node=f"{prefix}_relative_delta"), End(node="slope"), End(node=f"{prefix}_inverse")),
                masked(f"{prefix}_high_factor", "FLOAT", End(node=f"{prefix}_centered"), End(node=f"{prefix}_high_black_factor"), End(node=f"{prefix}_high_white_factor")),
                math(f"{prefix}_high_numerator", "MULTIPLY", End(node=f"{prefix}_high_factor"), End(node="white_denominator")),
                math(f"{prefix}_high_linear", "MULTIPLY_ADD", b, End(node=f"{prefix}_inverse"), a),
                math(f"{prefix}_high_base", "MULTIPLY_ADD", End(node="df"), End(node=f"{prefix}_inverse_square"), End(node=f"{prefix}_high_linear")),
                math(f"{prefix}_high_denominator", "MULTIPLY", End(node="white"), End(node=f"{prefix}_high_base")),
                math(f"{prefix}_high", "DIVIDE", End(node=f"{prefix}_high_numerator"), End(node=f"{prefix}_high_denominator")),
                math(f"{prefix}_large", "GREATER_THAN", value, 1.0),
                masked(f"{prefix}_response", "FLOAT", End(node=f"{prefix}_large"), End(node=f"{prefix}_low"), End(node=f"{prefix}_high")),
            )
        ),
        node("hable", bpy.types.ShaderNodeCombineXYZ, {axis: End(node=f"{axis.lower()}_response") for axis in axes}, {}),
    ).map(ended("hable", "Vector"))


def normalized(_identity: Identity, _scene: bpy.types.Scene, record: Hable) -> BlenderResult[Mapping[Key, Value]]:
    """Derives native Hable coefficients from the evaluated authored record."""
    _, _, _, _, _, _, scene_white = Hable.__struct_fields__
    return (
        Result[Option[tuple[float, float]], BlenderFaults]
        .bind(record.normalization().map_error(BlenderImagingFault.lifted), lambda value: value.to_result_with(lambda: (BlenderAbsentFault(owner=record.title, key=scene_white),)))
        .map(lambda values: {socket.key: (log2(abs(value)) if value else 0.0, float(np.sign(value)), 0.0) for socket, value in zip(normalization, values)})
    )


def tony(table: str | Traversable) -> CompositorEffect[Tony]:
    """Binds the canonical Tony McMapface table supplied by an asset catalog.

    Args:
        table: Canonical SPI3D catalog resource or file path.

    Returns:
        Native Rec.709 row with direct rational shaping and tetrahedral sampling.
    """

    def mapped(shell: Shell[Tony]) -> BlenderResult[tuple[Fragment, End]]:
        try:
            with as_file(Path(table) if isinstance(table, str) else table) as path:
                sampled = (
                    lut_files
                    .current(str(path), shell.config)
                    .bind(lambda path: Lookup(source=path, space=Tony.space).tile(shell.config).map_error(BlenderImagingFault.lifted))
                    .bind(lambda parts: parts[1].to_result_with(lambda: (BlenderAbsentFault(owner=str(path), key="Lut3DTransform"),)))
                )
        except OSError as error:
            return Error((BlenderReadFault(request=table, error=error),))
        return sampled.bind(lambda parts: tabled("tony", Tony.title, msgspec.structs.replace(parts[0], spacing=Rational()), parts[1], shell.source, shell.config))

    return CompositorEffect(record=Tony, graph=mapped, straight=True, initial_view=initial_view, registrables=(tiles, encode_formation))


def encoded(call: Call[Named]) -> BlenderResult[Outcome]:
    """Selects the first encoding-only native view and disables its look."""

    def chosen(scene: bpy.types.Scene, config: PyOpenColorIO.Config, held: Option[DisplayView]) -> BlenderResult[Outcome]:
        display = held.map(lambda record: record.display).default_value(viewed(scene).display)

        def selected(target: DisplayView) -> BlenderResult[Outcome]:
            entry = Option.of_optional(next(iter(entries(call.identity, scene, DisplayView)), None))
            return (
                entry
                .map(lambda entry: formation.read(call.identity, entry).bind(lambda prior: formation.write(call.identity, scene, entry, msgspec.structs.replace(prior, effect=target))))
                .default_with(
                    lambda: written(scene, "view_settings.view_transform", target.view).bind(
                        lambda view: written(scene, "view_settings.look", "None").map(lambda look: view.is_some() or look.is_some())
                    )
                )
                .map(lambda changed: Done() if changed else Unchanged(reason=f"The formation already encodes through {target.view}"))
            )

        return (
            Result[Option[tuple[DisplayView, PyOpenColorIO.Processor]], BlenderFaults]
            .bind(
                DisplayView.for_encoding(config, display).map_error(BlenderImagingFault.lifted),
                lambda view: view.map(lambda pair: pair[0]).to_result_with(lambda: (BlenderAbsentFault(owner=display, key=DisplayView.for_encoding.__name__),)),
            )
            .bind(selected)
        )

    config: BlenderResult[PyOpenColorIO.Config] = admitted(PyOpenColorIO.GetCurrentConfig(), ()).map_error(BlenderImagingFault.lifted)
    return call.root(bpy.types.Scene).bind(lambda scene: paired(config, formation.current(call.identity, scene)).bind(lambda pair: chosen(scene, *pair)))


def whitened[T: (ToneCurve, Hable)](record: T, config: PyOpenColorIO.Config, plane: npt.NDArray[np.float32]) -> BlenderResult[T]:
    """Returns a measured white override from covered associated scene-linear pixels."""
    match record.curve if isinstance(record, ToneCurve) else record:
        case Logarithmic() | ReinhardExtended() | Hable() as curve:
            curve_field, _, _ = ToneCurve.__struct_fields__
            _, _, _, _, _, _, scene_white = Hable.__struct_fields__
            extent: ImagingResult[Extent] = samples_of(config, plane).bind(lambda samples: samples.extent().to_result_with(lambda: (ImagingWhiteFault(white=Nothing),)))
            return (
                extent
                .bind(lambda extent: curves.metered(curve, extent.white))
                .map(lambda resolved: msgspec.structs.replace(record, **({curve_field: resolved} if isinstance(record, ToneCurve) else {scene_white: resolved.scene_white})))
                .map_error(BlenderImagingFault.lifted)
            )
        case _:
            return Ok(record)


def prepared[T: (ToneCurve, Hable)](
    _identity: Identity,
    _scene: bpy.types.Scene,
    record: T,
    config: PyOpenColorIO.Config,
    image: OpenImageIO.ImageBuf,
    _precision: Precision,
    _frame: CameraFrame,
    _entry: bpy.types.SceneCompositorEffect,
) -> BlenderResult[tuple[T, Mapping[Key, OpenImageIO.ImageBuf | Estimate[OpenImageIO.ImageBuf]]]]:
    """Returns an effective automatic-white record without changing authored controls."""
    match record.curve if isinstance(record, ToneCurve) else record:
        case Logarithmic(scene_white=Gated(enabled=False)) | ReinhardExtended(scene_white=Gated(enabled=False)) | Hable(scene_white=Gated(enabled=False)):
            pixels = image.get_pixels(OpenImageIO.FLOAT)
            effective: BlenderResult[T] = Error((BlenderImagingFault(faults=(ImagingBufferFault(request=image, error=image.geterror()),)),)) if pixels is None else whitened(record, config, pixels)
            return effective.map(lambda effective: (effective, {}))
        case _:
            return Ok((record, {}))


def measured[T: (ToneCurve, Hable)](
    row: CompositorEffect[T], identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, file: ImageFile[np.float32]
) -> BlenderResult[Option[str]]:
    """Writes an explicit measured white override, preserving the entry's blend, and names an undo step where the white changed."""
    config: BlenderResult[PyOpenColorIO.Config] = admitted(PyOpenColorIO.GetCurrentConfig(), ()).map_error(BlenderImagingFault.lifted)

    def sampled(config: PyOpenColorIO.Config, held: Entry[T]) -> BlenderResult[Option[str]]:
        match held.effect.curve if isinstance(held.effect, ToneCurve) else held.effect:
            case Logarithmic() | ReinhardExtended() | Hable():
                effective: BlenderResult[T] = whitened(held.effect, config, file.pixels)
                return Result[T, BlenderFaults].bind(
                    effective, lambda record: row.write(identity, scene, entry, msgspec.structs.replace(held, effect=record)).map(lambda changed: Some(meter_white.title) if changed else Nothing)
                )
            case _:
                return Ok(Nothing)

    reading: BlenderResult[tuple[PyOpenColorIO.Config, Entry[T]]] = paired(config, row.read(identity, entry))
    return Result[tuple[PyOpenColorIO.Config, Entry[T]], BlenderFaults].bind(reading, lambda pair: sampled(*pair))


def metered[T: ToneCurve | Hable](row: CompositorEffect[T], reading: ViewerRead, call: Call[Named]) -> BlenderResult[Outcome]:
    """Arms entering-frame measurement for a curve with a scene-white parameter."""

    def armed(entry: bpy.types.SceneCompositorEffect, held: Entry[T]) -> BlenderResult[Outcome]:
        return (
            reading.entering(call.identity, entry).map(lambda _: Done())
            if isinstance(held.effect.curve if isinstance(held.effect, ToneCurve) else held.effect, (Logarithmic, ReinhardExtended, Hable))
            else Ok(Unchanged(reason="This curve has no scene-white parameter"))
        )

    return call.root(bpy.types.Scene).bind(lambda scene: row.named(call.identity, scene, call.record.name)).bind(lambda entry: row.read(call.identity, entry).bind(lambda held: armed(entry, held)))


# --- [COMPOSITION] ----------------------------------------------------------------------

initial_view: Option[Callable[[PyOpenColorIO.Config, bpy.types.Scene], BlenderResult[DisplayView]]] = Some(
    lambda config, scene: Result[Option[tuple[DisplayView, PyOpenColorIO.Processor]], BlenderFaults].bind(
        DisplayView.for_encoding(config, viewed(scene).display).map_error(BlenderImagingFault.lifted),
        lambda view: view.map(lambda pair: pair[0]).to_result_with(lambda: (BlenderAbsentFault(owner=scene.name, key=DisplayView.for_encoding.__name__),)),
    )
)
encode_formation = Operator(key="encode_formation", title="Use Display Encoding", description="Selects an encoding-only formation after the tone curve", form=Named, run=encoded)
reading = ViewerRead(consume=lambda identity, scene, entry, file: measured(tone_curve, identity, scene, entry, file))
meter_white = Operator(
    key="meter_white",
    title="Measure Scene White",
    description="Measures the curve's scene white from its entering frame",
    form=Named,
    run=lambda call: metered(tone_curve, reading, call),
    undo=False,
    condition=Some(Job(kind="RENDER")),
)
tone_curve: CompositorEffect[ToneCurve] = CompositorEffect(
    record=ToneCurve, graph=curved, straight=True, initial_view=initial_view, preparation=Some(prepared), registrables=(encode_formation, meter_white, reading)
)
hable_reading = ViewerRead(consume=lambda identity, scene, entry, file: measured(hable, identity, scene, entry, file))
hable_meter_white = Operator(
    key="meter_hable_white",
    title=meter_white.title,
    description=meter_white.description,
    form=Named,
    run=lambda call: metered(hable, hable_reading, call),
    undo=False,
    condition=Some(Job(kind="RENDER")),
)
normalization = tuple(
    Socket(
        key=item_key(Hable.normalization.__name__, name.lower()),
        name=name,
        kind=bpy.types.NodeSocketVector,
        description="Log magnitude and sign of a Hable coefficient derived from the evaluated record",
        members={Descriptor(root=bpy.types.NodeTreeInterfaceSocket, owner="", prop=bpy.types.NodeTreeInterfaceSocket.bl_rna.properties["hide_in_modifier"]): True},
    )
    for name in ("Slope", "Intercept")
)
hable: CompositorEffect[Hable] = CompositorEffect(
    record=Hable,
    graph=habled,
    straight=True,
    initial_view=initial_view,
    inputs=normalization,
    derived=Some(normalized),
    admitted=Some(lambda record: record.normalization().map(lambda _: record)),
    preparation=Some(prepared),
    registrables=(encode_formation, hable_meter_white, hable_reading),
)
neutral = CompositorEffect(record=Neutral, graph=lambda shell: neutralized(shell.source), straight=True, initial_view=initial_view, registrables=(encode_formation,))
photoreceptor = CompositorEffect(record=Photoreceptor, coverage=Coverage.RESULT, initial_view=initial_view, registrables=(encode_formation,), graph=adapted)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Response",
    "adapted",
    "curved",
    "encode_formation",
    "encoded",
    "hable",
    "hable_meter_white",
    "hable_reading",
    "habled",
    "measured",
    "meter_white",
    "metered",
    "neutral",
    "neutralized",
    "nonnegative",
    "normalized",
    "photoreceptor",
    "prepared",
    "reading",
    "tone_curve",
    "tony",
    "whitened",
]
