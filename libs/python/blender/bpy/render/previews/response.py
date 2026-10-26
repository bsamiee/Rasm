"""Tone response sampling of a stack's tone and formation entries, charted outside drawing."""

from collections.abc import Generator
from contextlib import contextmanager, ExitStack
from pathlib import Path
from typing import ClassVar

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.events.handlers import LOAD_POST, PathHandler, UpdateHandler
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Band
from blender.bpy.faults import BlenderAbsentFault, BlenderFaults, BlenderResult
from blender.bpy.nodes.trees import fingerprinted
from blender.bpy.render.effects.formation import formation, viewed
from blender.bpy.render.effects.rows import Enabled
from blender.bpy.render.effects.stack import EffectStack
from blender.bpy.render.previews.queue import Job, PreviewQueue, Rank
from blender.bpy.render.previews.scenes import PreviewScene
from blender.bpy.ui.drawing.canvas import Align
from blender.bpy.ui.drawing.charts import Grid, TracePlot, Well
from blender.bpy.ui.drawing.handlers import Signal
from blender.bpy.ui.resources.theme import Metrics
from blender.imaging.chain import Effect, GREY, Stage
from blender.imaging.color.processors import applied, DisplayView, processor
from blender.imaging.faults import accumulated, paired
from blender.imaging.hashing import content_hash, ContentHash


class _ResponseRequest(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Response render request with host handles and observed state."""

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
    """Response sampling outside drawing, with read-only chart."""

    stack: EffectStack[E]
    sampling: PreviewScene
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
        """Charts cached RGB responses, recording device columns."""
        self.columns = round(Well.scope(metrics).plot.width)

        def plotted(held: tuple[_ResponseRequest, npt.NDArray[np.float32], tuple[float, ...]]) -> TracePlot:
            request, values, guides = held
            x = (np.arange(request.columns, dtype=np.float32) + 0.5) / request.columns
            return TracePlot.of(
                metrics,
                traces=((np.column_stack((x, values.mean(axis=1))).astype(np.float32), Well.trace),),
                grid=Grid(ink=Well.grid, horizontals=guides),
                labels=tuple((f"{level:.2%}", 0.0, code, Align.START) for level, code in zip(request.guide_levels, guides)),
                caption=request.caption,
            )

        return Ok(self.cached.filter(lambda held: self.wanted.filter(lambda request: held[0] is request and request.columns == self.columns).is_some()).map(plotted))

    # --- [DEMAND]

    def fed(self, identity: Identity, context: bpy.types.Context) -> BlenderResult[tuple[Job, ...]]:
        """Returns current preview jobs, reusing request while observed state holds."""
        if not self.columns:
            return Ok(())

        def allocated(config: PyOpenColorIO.Config) -> BlenderResult[PyOpenColorIO.Processor]:
            def walked(transform: PyOpenColorIO.Transform, *, inverse: bool) -> Generator[tuple[PyOpenColorIO.AllocationTransform, bool]]:
                match transform:
                    case PyOpenColorIO.AllocationTransform():
                        yield transform, inverse
                    case PyOpenColorIO.GroupTransform():
                        for member in transform:
                            yield from walked(member, inverse=inverse ^ (transform.getDirection() == PyOpenColorIO.TRANSFORM_DIR_INVERSE))
                    case _:
                        pass

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
                        return processor(config, PyOpenColorIO.GroupTransform([value], direction=PyOpenColorIO.TRANSFORM_DIR_FORWARD if inverse else PyOpenColorIO.TRANSFORM_DIR_INVERSE))
                    case _:
                        return Error((BlenderAbsentFault(owner=space.getName(), key=PyOpenColorIO.AllocationTransform.__name__),))

            match self.allocation:
                case str(name):
                    return Result[PyOpenColorIO.ColorSpace, BlenderFaults].bind(
                        Option.of_optional(config.getColorSpace(name)).to_result_with(lambda: (BlenderAbsentFault(owner=config.getName(), key=name),)), selected
                    )
                case transform:
                    return processor(config, PyOpenColorIO.GroupTransform([transform], direction=PyOpenColorIO.TRANSFORM_DIR_INVERSE))

        def prepared(scene: bpy.types.Scene, config: PyOpenColorIO.Config) -> BlenderResult[tuple[Job, ...]]:
            def guided() -> BlenderResult[PyOpenColorIO.Processor]:
                def selected(held: Option[DisplayView]) -> BlenderResult[PyOpenColorIO.Processor]:
                    display = held.map(lambda record: record.display).default_with(lambda: viewed(scene).display)
                    return Result[Option[tuple[DisplayView, PyOpenColorIO.Processor]], BlenderFaults].bind(
                        DisplayView.for_encoding(config, display),
                        lambda found: found.map(lambda pair: pair[1]).to_result_with(lambda: (BlenderAbsentFault(owner=display, key=DisplayView.for_encoding.__name__),)),
                    )

                return self.guide.map(lambda transform: processor(config, transform)).default_with(lambda: formation.current(identity, scene).bind(selected))

            rows = tuple(
                (held.entry, held.row)
                for held in self.stack.owned(identity, scene)
                if bool(self.enabled.descriptor.read(held.entry)) and (held.row.record in self.selection or held.row.record.stage in self.selection)
            )
            selected = tuple(entry for entry, _row in rows)
            locality = accumulated(row.read(identity, entry).map(lambda held: held.effect.pointwise) for entry, row in rows)
            observations = accumulated(paired(Snapshot.capture(entry), Option.of_optional(entry.node_group).map(fingerprinted).default_value(Ok(Nothing))) for entry in selected)
            encoding = paired(guided(), allocated(config)).map(lambda built: tuple(value.getDefaultCPUProcessor() for value in built))

            def requested(parts: tuple[tuple[bool, ...], tuple[tuple[Snapshot, Option[ContentHash]], ...], tuple[PyOpenColorIO.CPUProcessor, ...]]) -> BlenderResult[tuple[Job, ...]]:
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
                    lambda _: (Job(place=self, key=request.content.default_value(request), rank=Rank.SHOWN, run=lambda: self.rendered(identity, request)),)
                )

            return paired(paired(locality, observations), encoding).map(lambda pair: (*pair[0], pair[1])).bind(requested)

        return (
            self.stack
            .config()
            .bind(lambda config: Option.of_optional(context.scene).map(lambda scene: prepared(scene, config)).default_with(self.cleared))
            .or_else_with(lambda faults: paired(Error(faults), self.cleared()).map(lambda _: ()))
        )

    def rendered(self, identity: Identity, request: _ResponseRequest) -> BlenderResult[None]:
        """Samples chain and publishes values only for current request."""
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

    def cleared(self) -> BlenderResult[tuple[Job, ...]]:
        """Clears current request and cached response."""
        changed = self.wanted.is_some() or self.cached.is_some()
        self.wanted, self.cached = Nothing, Nothing
        return (self.signal.fired() if changed else Ok(None)).map(lambda _: ())

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[BlenderResult[Acquisitions]]:
        """Holds queue feed, removing invalidation callbacks before clearing."""
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
                PathHandler(handlers=LOAD_POST, body=loaded),
                UpdateHandler(
                    handlers=bpy.app.handlers.depsgraph_update_post,
                    ids=(bpy.types.Scene, bpy.types.NodeTree, bpy.types.Image),
                    body=lambda scene, _updates: Ok(later()) if self.wanted.filter(lambda request: request.scene == scene).is_some() else Ok(None),
                ),
            ))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Response"]
