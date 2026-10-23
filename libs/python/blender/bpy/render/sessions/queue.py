"""Render queue dispatch and run progress."""

import base64
from collections.abc import Callable, Generator, Iterable, Mapping
from contextlib import contextmanager, ExitStack
import datetime
from enum import StrEnum
from fractions import Fraction
from functools import reduce
import hashlib
import importlib.resources
from itertools import chain
import os
from pathlib import Path
import posixpath
import shutil
import time
from types import MemberDescriptorType
from typing import Annotated, ClassVar

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.calls import called
from blender.bpy.commands.operators import Call, Done, Empty, Operator, Properties
from blender.bpy.data.ids.store import resolved
from blender.bpy.data.rna.conditions import Condition, Setting
from blender.bpy.data.rna.descriptors import Descriptor, rooted, struct_at
from blender.bpy.extension.events.timers import Timer
from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.extension.registration import Acquisitions, Registrable
from blender.bpy.extension.registries import CliCommand
from blender.bpy.extension.reports import rendered
from blender.bpy.faults import BlenderAbsentFault, BlenderChoiceFault, BlenderFaults, BlenderImagingFault, BlenderReadFault, BlenderRenderingFault, BlenderResult, BlenderUnmetFault, BlenderWriteFault
from blender.bpy.files.blends import BlendFile, OpenFile, packed, saved, SaveMode, session_loaded
from blender.bpy.nodes.changes import made
from blender.bpy.render.color.luts import Enabled
from blender.bpy.render.effects.detail.denoise import motioned, Plane, Rendered
from blender.bpy.render.effects.stack import Edge, EffectStack, Pinned
from blender.bpy.render.outputs.finishing import Written
from blender.bpy.render.outputs.framing import Framed, FrameSize
from blender.bpy.render.outputs.paths import OutputPath, Pathed, ScenePaths
from blender.bpy.render.previews.scenes import rendering
from blender.bpy.render.sessions import execution
from blender.bpy.render.sessions.preflight import checked, JobScope, Preflight
from blender.bpy.render.sessions.sets import Members, RenderSet
from blender.bpy.render.view_layers.passes import pass_flags, PassRouting, PassSet, required
from blender.bpy.settings.attachments import Attached, Attachment
from blender.bpy.settings.properties import Choices
from blender.bpy.settings.texts import TextRecord
from blender.bpy.ui.layout import rows
from blender.bpy.ui.layout.sections import Drawn, Single
from blender.bpy.ui.layout.status import Status
from blender.bpy.ui.screens.areas import AreaQuery, Located
from blender.bpy.ui.screens.editors import EditorState
from blender.imaging.effects.detail.denoise import TemporalDenoise
from blender.imaging.faults import accumulated, paired
from blender.imaging.files.images import opened
from blender.imaging.hashing import content_hash
from blender.imaging.parameters import Choice, Gated
from blender.rendering.batch import exchange
from blender.rendering.batch.estimate import Estimate
from blender.rendering.batch.flamenco import Flamenco, Manager, queried
from blender.rendering.batch.notices import Cancelled, Completed, Estimated, Exhausted, Failed, Listener, Notice, published, Started, Test
from blender.rendering.batch.power import cancelled, PowerHold, shut
from blender.rendering.batch.processes import detached
from blender.rendering.batch.progress import Ending, Heartbeat, Liveness, observed, Progress, StageMark
from blender.rendering.batch.queue import Bound, Fixed, Frames, Job, load, Marker, RenderFrames, RenderQueue, revised, store
from blender.rendering.batch.supervision import cleared, Host, LocalMachine, Run, RunFile, stop, supervise
from blender.rendering.batch.targets import send, Target
from blender.rendering.faults import RenderingFaults, RenderingReadFault, RenderingResult, RenderingRunFault
from blender.rendering.outputs.framing import Side
from blender.rendering.outputs.naming import Output

# --- [TYPES] ----------------------------------------------------------------------------


class Reach(Choice):
    """Dispatch render scope."""

    SETS = 1, "Render Sets"
    SCENE = 2, "Scene"
    LAYER = 3, "View Layer"
    CAMERAS = 4, "Selected Cameras"


class PlanFile(StrEnum):
    """Dispatch file suffixes sharing plan stem."""

    PLAN = ".plan.json"
    LOG = ".plan.log"
    SENT = ".sent.json"

    def beside(self, plan: Path) -> Path:
        """Returns file path beside plan."""
        return plan.with_name(f"{plan.name.removesuffix(PlanFile.PLAN)}{self}")


# --- [MODELS] ---------------------------------------------------------------------------


class Dispatch(msgspec.Struct, frozen=True, kw_only=True):
    """Dispatch dialog options."""

    reach: Annotated[Reach, msgspec.Meta(title="Render", description="Scope queue renders")] = Reach.SETS


class Plan(msgspec.Struct, frozen=True, kw_only=True):
    """Ordered job scopes and dispatch time."""

    moment: datetime.datetime
    scopes: Annotated[tuple[JobScope, ...], msgspec.Meta(min_length=1)]


class Preparing(msgspec.Struct, frozen=True, kw_only=True):
    """Plan path and target `prepare` command reads."""

    plan: str
    target: Target


class Sent(msgspec.Struct, frozen=True, kw_only=True):
    """Sent queue with target and each issued path or farm job ID."""

    queue: RenderQueue
    target: Target
    issued: tuple[str, ...]


class Queued(msgspec.Struct, frozen=True, kw_only=True):
    """Plan path stored in file text record."""

    plan: str | None = None


class Denoising(msgspec.Struct, frozen=True, kw_only=True):
    """Scene setting for temporal denoise job after render."""

    temporal: Gated[TemporalDenoise] = Gated(enabled=False, value=TemporalDenoise())


class Queueing(msgspec.Struct, frozen=True, kw_only=True):
    """Queue preferences."""

    target: Annotated[Target, msgspec.Meta(title="Target", description="Where queue renders")] = LocalMachine()
    listener: Listener = Listener()
    power: PowerHold = PowerHold()
    heartbeat: Heartbeat = Heartbeat()
    scripts: Annotated[Gated[str], msgspec.Meta(title="Flamenco Scripts", description="Manager job type scripts folder")] = Gated(enabled=False, value="")


class Observed(msgspec.Struct, frozen=True, kw_only=True):
    """Sent record and local progress status timer last read."""

    sent: Option[Sent]
    progress: Option[tuple[Progress, datetime.datetime]]


# --- [SERVICES] -------------------------------------------------------------------------


class QueueState:
    """Session state of one queue row."""

    def __init__(self) -> None:
        """Starts empty."""
        self.manager: Option[Manager] = Nothing
        self.observed: Option[Observed] = Nothing
        self.paused: tuple[tuple[Located, EditorState], ...] = ()
        self.tags = Choices(
            lambda _identity, _owner, _context: tuple(
                (identifier, tag.name, Option.of_optional(tag.description).default_value(""))
                for manager in self.manager.to_list()
                for tag in manager.tags
                for identifier in Option.of_optional(tag.id).to_list()
            )
        )


class Dispatcher(msgspec.Struct, frozen=True, kw_only=True):
    """Stores and session state queue operations read."""

    identity: Identity
    host: Host
    members: Members
    framed: Framed
    routing: PassRouting
    pathed: Pathed
    queued: Attached[Queued]
    denoise: Attached[Denoising]
    inputs: Attached[Rendered]
    stack: EffectStack
    state: QueueState
    settings: Callable[[Identity], BlenderResult[Queueing]]

    # --- [ACCESS]

    @staticmethod
    def connected() -> BlenderResult[None]:
        """Requires `bpy.app.online_access`, set by preferences and `--offline-mode`."""
        preference = Setting(descriptor=Descriptor(root=bpy.types.Preferences, owner="system", prop=bpy.types.PreferencesSystem.bl_rna.properties["use_online_access"]))
        return Ok(None) if bpy.app.online_access else Error((BlenderUnmetFault(requirement=preference),))

    # --- [PLANNING]

    def planned(self, context: bpy.types.Context, dispatch: Dispatch, moment: datetime.datetime) -> BlenderResult[Plan]:
        """Plans reach scopes, each followed by enabled temporal denoise job."""

        def bound(scene: bpy.types.Scene) -> Bound:
            return Bound(
                camera=Option.of_optional(scene.camera).map(lambda camera: camera.name).to_optional(),
                markers=tuple(
                    Marker(frame=marker.frame, camera=camera.name)
                    for marker in sorted(scene.timeline_markers, key=lambda marker: marker.frame)
                    for camera in Option.of_optional(marker.camera).to_list()
                ),
            )

        def reached(scene: bpy.types.Scene) -> BlenderResult[tuple[JobScope, ...]]:
            match dispatch.reach:
                case Reach.SETS:
                    return self.members.rendered(context).map(
                        lambda members: tuple(JobScope(scene=held.name, layer=layer.map(lambda value: value.name).to_optional(), camera=bound(held)) for held, layer in members)
                    )
                case Reach.SCENE:
                    return Ok((JobScope(scene=scene.name, camera=bound(scene)),))
                case Reach.LAYER:
                    return rooted(bpy.types.ViewLayer, context).map(lambda layer: (JobScope(scene=scene.name, layer=layer.name, camera=bound(scene)),))
                case Reach.CAMERAS:
                    return Ok(tuple(JobScope(scene=scene.name, camera=Fixed(camera=camera.name)) for camera in context.selected_objects if camera.type == "CAMERA"))

        def denoised(scope: JobScope) -> BlenderResult[tuple[JobScope, ...]]:
            return self.denoise.read(bpy.data.scenes[scope.scene]).map(lambda held: (scope, *((msgspec.structs.replace(scope, temporal=held.temporal.value),) if held.temporal.enabled else ())))

        scene = rooted(bpy.types.Scene, context)
        return (
            scene
            .bind(reached)
            .bind(lambda scopes: accumulated(map(denoised, scopes)))
            .map(lambda groups: tuple(chain.from_iterable(groups)))
            .bind(lambda scopes: Ok(Plan(moment=moment, scopes=scopes)) if scopes else Error((BlenderAbsentFault(owner=repr(context), key=dispatch.reach.name),)))
        )

    # --- [DISPATCH]

    def dispatched(self, call: Call[Dispatch]) -> BlenderResult[Done]:
        """Checks and stores plan, saves file, then starts `prepare` and pauses viewport renders."""
        context, identity = call.context, call.identity
        moment = datetime.datetime.now(datetime.UTC)
        day = moment.astimezone().date()

        def gated(plan: Plan, queueing: Queueing) -> BlenderResult[None]:
            online = Dispatcher.connected() if isinstance(queueing.target, Flamenco) else Ok(None)
            return paired(checked(self.pathed, plan.scopes, day), online).map(lambda _: None)

        def paused(manager: bpy.types.WindowManager) -> BlenderResult[None]:
            def applied(support: Descriptor[bpy.types.BoolProperty], state: EditorState) -> BlenderResult[None]:
                def kept(at: Located, prior: Option[EditorState]) -> None:
                    self.state.paused = (*self.state.paused, *((at, held) for held in prior.to_list()))

                return accumulated(
                    state.applied(at.area).map(lambda prior: kept(at, prior)) for at in AreaQuery(space="VIEW_3D", shows=Some(Setting(descriptor=support))).located(manager.windows)
                ).map(lambda _: None)

            return paired(
                Descriptor.resolve(bpy.types.SpaceView3D, "region_3d.support_pause_render", bpy.types.BoolProperty), EditorState.of(bpy.types.SpaceView3D, {"region_3d.pause_render": True})
            ).bind(lambda pair: applied(*pair))

        def written(plan: Plan, queueing: Queueing, manager: bpy.types.WindowManager) -> BlenderResult[Done]:
            path = identity.folder / f"{content_hash(Plan, plan).hex()}{PlanFile.PLAN}"
            scenes = tuple(dict.fromkeys(bpy.data.scenes[scope.scene] for scope in plan.scopes))

            def started(source: Path, command: str) -> BlenderResult[Path]:
                argv = (self.host.binary, "--background", str(source), "--command", command, base64.b64encode(msgspec.json.encode(Preparing(plan=str(path), target=queueing.target))).decode("ascii"))
                return detached(argv, PlanFile.LOG.beside(path)).map_error(BlenderRenderingFault.lifted)

            return (
                accumulated(self.pathed.took(scene, day) for scene in scenes)
                .bind(lambda _: store(path, plan).map_error(BlenderRenderingFault.lifted))
                .bind(lambda _: self.queued.write(manager, Queued(plan=str(path))))
                .bind(lambda _: saved(Path(bpy.data.filepath), SaveMode.ACTIVE))
                .bind(lambda source: identity.name(Name.CLI, QueueRun.prepare).bind(lambda command: started(source, command)))
                .map(lambda log: Done(notes=(("INFO", f"{len(plan.scopes)} jobs preparing, log at {log}"),), skipped=paused(manager).swap().to_option()))
            )

        manager = rooted(bpy.types.WindowManager, context)
        return (
            paired(paired(self.planned(context, call.record, moment), self.settings(identity)), manager)
            .bind(lambda parts: gated(*parts[0]).map(lambda _: (*parts[0], parts[1])))
            .bind(lambda parts: written(*parts))
        )

    def prepared(self, arguments: Preparing, trailing: tuple[str, ...]) -> BlenderResult[Sent]:
        """Writes job files of stored plan, sends queue to target, and stores `Sent` record."""
        if trailing:
            return Error((BlenderChoiceFault(path=QueueRun.prepare, identifier=repr(trailing), accepted=()),))
        path = Path(arguments.plan)
        source = Path(bpy.data.filepath)

        def sent(jobs: tuple[Job, ...]) -> BlenderResult[Sent]:
            queue = RenderQueue(jobs=jobs, blender=bpy.app.version_string, extension=self.identity.manifest.version)
            match send(arguments.target, queue, self.host):
                case Result(tag="error", error=faults):
                    return Error(BlenderRenderingFault.lifted(faults))
                case Result(ok=results):
                    record = Sent(queue=queue, target=arguments.target, issued=tuple(value for result in results for value in result.to_option().to_list()))
                    for result in results:
                        match result:
                            case Result(tag="ok", ok=issued):
                                self.identity.logger.info("Queue sent", issued=issued)
                            case Result(error=refused):
                                self.identity.reports.hold(BlenderRenderingFault.lifted(refused))
                    return store(PlanFile.SENT.beside(path), record).map_error(BlenderRenderingFault.lifted).map(lambda _: record)

        loaded: BlenderResult[Plan] = load(path, Plan).map_error(BlenderRenderingFault.lifted)
        return (
            Result[Plan, BlenderFaults]
            .bind(
                loaded,
                lambda plan: accumulated(
                    scoped(self, scope, plan, source, path, part).map(
                        lambda job: msgspec.structs.replace(
                            job,
                            prerequisites=(next(index for index, before in enumerate(plan.scopes[:part]) if before == msgspec.structs.replace(scope, temporal=None)),)
                            if scope.temporal is not None
                            else (),
                        )
                    )
                    for part, scope in enumerate(plan.scopes)
                ),
            )
            .bind(sent)
        )

    # --- [BACKGROUND_RENDER]

    def frames[T](self, scene: bpy.types.Scene, frames: tuple[int, ...], consumed: Callable[[int, float], BlenderResult[T]], base: Option[Path] = Nothing) -> BlenderResult[tuple[T, ...]]:
        """Renders and consumes each frame, output rerooted under `base` when set."""

        def rendered(frame: int) -> BlenderResult[T]:
            started = time.monotonic()
            scene.frame_start = scene.frame_end = frame
            return (
                self.stack
                .precision(self.identity)
                .bind(lambda precision: execution.rendered(self.identity, self.stack, scene, precision, {"animation": True}))
                .bind(lambda _: consumed(frame, time.monotonic() - started))
            )

        def prepared(original: OutputPath) -> BlenderResult[None]:
            path = base.map(original.rerooted).default_value(original)
            rerooted = (
                self.pathed.scope
                .write(scene, path)
                .bind(lambda _: path.scene_output(scene))
                .bind(lambda text: Pathed.filepath.write(scene.render, text))
                .bind(lambda _: self.routing.reconciled(scene, bpy.context, path))
                .map(lambda _: None)
                if base.is_some()
                else Ok(None)
            )

            def loaded(values: tuple[Rendered, Denoising]) -> BlenderResult[None]:
                inputs, denoising = values
                source = msgspec.structs.replace(
                    inputs,
                    color=msgspec.structs.replace(inputs.color, path=str(path.base / inputs.color.path)),
                    motion=msgspec.structs.replace(inputs.motion, path=str(path.base / inputs.motion.path)),
                )
                return (
                    source.group(denoising.temporal.value).bind(lambda group: made(group, self.identity, of=Some(scene))).bind(lambda _: self.stack.refreshed(self.identity, scene)).map(lambda _: None)
                )

            return rerooted.bind(lambda _: paired(self.inputs.read(scene), self.denoise.read(scene)).bind(loaded) if self.inputs.stored(scene) else Ok(None))

        return (
            self.pathed.scope
            .read(scene)
            .bind(lambda found: found.to_result_with(lambda: (BlenderAbsentFault(owner=repr(scene), key=self.pathed.scope.name),)))
            .bind(prepared)
            .bind(lambda _: reduce(lambda done, frame: done.bind(lambda values: rendered(frame).map(lambda value: (*values, value))), frames, Result[tuple[T, ...], BlenderFaults](ok=())))
        )

    def commanded(self, arguments: Run | RenderFrames, trailing: tuple[str, ...]) -> BlenderResult[object]:
        """Runs local queue progress or farm worker frame request."""
        match arguments:
            case Run():
                return Error((BlenderChoiceFault(path=QueueRun.render, identifier=repr(trailing), accepted=()),)) if trailing else self.rendered(arguments)
            case RenderFrames(scene=name, frames=frames):
                try:
                    (output_root,) = trailing
                except ValueError as error:
                    return Error((BlenderReadFault(request=repr(trailing), error=error),))
                scene: BlenderResult[bpy.types.Scene] = Option.of_optional(bpy.data.scenes.get(name)).to_result_with(lambda: (BlenderAbsentFault(owner=repr(bpy.data.scenes), key=name),))
                return Result[bpy.types.Scene, BlenderFaults].bind(scene, lambda held: self.frames(held, frames, lambda _frame, _seconds: Ok(None), Some(Path(output_root)))).map(lambda _: None)

    def supervised(self, run: Run, trailing: tuple[str, ...]) -> BlenderResult[Progress]:
        """Supervises local render processes under power hold, posting notices."""
        if trailing:
            return Error((BlenderChoiceFault(path=QueueRun.supervise, identifier=repr(trailing), accepted=()),))

        def ran(queue: RenderQueue, queueing: Queueing) -> BlenderResult[Progress]:
            records = self.host.path(queue, RunFile.PROGRESS)

            def completed(progress: Progress) -> BlenderResult[Progress]:
                self.identity.reports.reported(posted)(
                    queueing.listener, Completed(frames=len(progress.marks), seconds=sum((Fraction(mark.seconds) for mark in progress.marks), Fraction()))
                ).to_optional()
                return (shut() if queueing.power.shutdown else Ok(None)).map_error(BlenderRenderingFault.lifted).map(lambda _: progress)

            def failed(faults: RenderingFaults) -> BlenderResult[Progress]:
                notice: Notice
                match faults:
                    case (RenderingRunFault(error=int(code)), *_) if code != 1:
                        notice = Exhausted(job=queue.stem, attempts=run.target.retries + 1)
                    case _:
                        notice = Failed(job=queue.stem, reason=" ".join(text for _, text in rendered(BlenderRenderingFault.lifted(faults))))
                self.identity.reports.reported(posted)(queueing.listener, notice).to_optional()
                return Error(BlenderRenderingFault.lifted(faults))

            match observed(records):
                case Result(tag="error", error=faults):
                    return Error(BlenderRenderingFault.lifted(faults))
                case Result(ok=observations):
                    if not Progress.begun(observations.map(lambda observation: observation[0]), queue, os.getpid()).marks:
                        self.identity.reports.reported(posted)(queueing.listener, Started(jobs=len(queue.jobs), frames=len(queue.steps))).to_optional()
                    return queueing.power.held(lambda: supervise(run.target, queue, self.host, queueing.heartbeat)).map(completed).default_with(failed)

        return paired(load(Path(run.queue), RenderQueue).map_error(BlenderRenderingFault.lifted), self.settings(self.identity)).bind(lambda pair: ran(*pair))

    def rendered(self, run: Run) -> BlenderResult[tuple[StageMark, ...]]:
        """Renders pending frames of first remaining job, storing each frame mark."""

        def ran(queue: RenderQueue, queueing: Queueing) -> BlenderResult[tuple[StageMark, ...]]:
            records = self.host.path(queue, RunFile.PROGRESS)
            holding = frozenset(part for part, job in enumerate(queue.jobs) if job.frames.numbers)

            def marked(part: int, frame: int, seconds: float) -> BlenderResult[StageMark]:
                scene = bpy.data.scenes[queue.jobs[part].scene]
                peak = peaked(Path(bpy.path.abspath(scene.render.frame_path(frame=frame)))) if scene.render.image_settings.file_format in {"OPEN_EXR", "OPEN_EXR_MULTILAYER"} else Ok(Nothing)
                return peak.map(lambda value: StageMark(part=part, item=frame, seconds=seconds, peak=value.to_optional()))

            def finished(progress: Progress, mark: StageMark) -> BlenderResult[StageMark]:
                before = frozenset(held.part for held in progress.marks[:-1])
                first = holding <= before | {mark.part} and not holding <= before
                for estimate in Estimate.queued(progress, queue, datetime.datetime.now(datetime.UTC)).filter(lambda _: first).to_list():
                    self.identity.reports.reported(posted)(queueing.listener, Estimated(estimate=estimate)).to_optional()
                job = queue.jobs[mark.part]
                scene = bpy.data.scenes[job.scene]
                return (
                    self.pathed.scope
                    .read(scene)
                    .bind(lambda found: found.to_result_with(lambda: (BlenderAbsentFault(owner=repr(scene), key=self.pathed.scope.name),)))
                    .bind(lambda path: path.removed(scene, job.frames.numbers))
                    .map(lambda _: mark)
                    if all(part != mark.part for part, _ in progress.remaining(queue))
                    else Ok(mark)
                )

            def appended(mark: StageMark) -> BlenderResult[StageMark]:
                persisted: BlenderResult[Progress] = revised(records, Progress, lambda held: msgspec.structs.replace(held, marks=(*held.marks, mark))).map_error(BlenderRenderingFault.lifted)
                return Result[Progress, BlenderFaults].bind(persisted, lambda held: finished(held, mark))

            def runs(progress: Progress) -> BlenderResult[tuple[StageMark, ...]]:
                remaining = frozenset(progress.remaining(queue))
                steps = tuple(step for step in progress.pending if step in remaining)
                return (
                    self.frames(bpy.data.scenes[queue.jobs[steps[0][0]].scene], tuple(frame for _, frame in steps), lambda frame, seconds: marked(steps[0][0], frame, seconds).bind(appended))
                    if steps
                    else Ok(())
                )

            loaded: BlenderResult[Progress] = load(records, Progress).map_error(BlenderRenderingFault.lifted)
            return Result[Progress, BlenderFaults].bind(loaded, runs)

        return paired(load(Path(run.queue), RenderQueue).map_error(BlenderRenderingFault.lifted), self.settings(self.identity)).bind(lambda pair: ran(*pair))

    # --- [SESSION_PROGRESS]

    def observed(self, context: bpy.types.Context) -> BlenderResult[Option[Observed]]:
        """Reads sent record and local progress, `Nothing` sent before `prepare` writes it."""

        def read(plan: Path) -> RenderingResult[Observed]:
            def pending(faults: RenderingFaults) -> RenderingResult[Option[Sent]]:
                match faults:
                    case (RenderingReadFault(error=FileNotFoundError()),):
                        return Ok(Nothing)
                    case _:
                        return Error(faults)

            return (
                load(PlanFile.SENT.beside(plan), Sent)
                .map(Some)
                .or_else_with(pending)
                .bind(
                    lambda sent: (
                        sent
                        .filter(lambda record: isinstance(record.target, LocalMachine))
                        .map(lambda record: observed(self.host.path(record.queue, RunFile.PROGRESS)))
                        .default_value(Ok(Nothing))
                        .map(lambda progress: Observed(sent=sent, progress=progress))
                    )
                )
            )

        manager = rooted(bpy.types.WindowManager, context)
        return manager.bind(self.queued.read).bind(
            lambda queued: Option.of_optional(queued.plan).map(lambda plan: read(Path(plan)).map(Some).map_error(BlenderRenderingFault.lifted)).default_value(Ok(Nothing))
        )

    def kept(self) -> BlenderResult[tuple[RenderQueue, LocalMachine, Progress, datetime.datetime]]:
        """Returns observed local run, absent fault without local progress."""
        match self.state.observed.bind(lambda held: held.sent.bind(lambda sent: held.progress.map(lambda pair: (sent, *pair)))).to_optional():
            case (Sent(queue=queue, target=LocalMachine() as target), progress, beat):
                return Ok((queue, target, progress, beat))
            case _:
                return Error((BlenderAbsentFault(owner=repr(self.queued), key=RunFile.PROGRESS),))

    def restored(self) -> BlenderResult[Option[str]]:
        """Restores paused viewports still open, run as deferred callback."""
        paused, self.state.paused = self.state.paused, ()
        return accumulated(prior.applied(now.area) for at, prior in paused for now in Located.found(at.window, at.area.as_pointer()).to_list()).map(lambda _: Nothing)

    def ticked(self, restore: Callable[[], None]) -> BlenderResult[Option[float]]:
        """Refreshes observation each second, restoring viewports once run ends."""

        def acted(fresh: Option[Observed]) -> BlenderResult[None]:
            prior = self.state.observed
            self.state.observed = fresh.map(
                lambda now: msgspec.structs.replace(
                    now, progress=now.progress.or_else_with(lambda: prior.bind(lambda last: last.progress).filter(lambda pair: now.sent.filter(lambda sent: sent.queue.key == pair[0].queue).is_some()))
                )
            )
            if self.state.observed.bind(lambda held: held.progress).bind(lambda pair: Option.of_optional(pair[0].ending)).is_some() and self.state.paused:
                restore()
            return (
                fresh
                .bind(lambda now: now.sent.bind(lambda sent: now.progress.filter(lambda pair: pair[0].ending is Ending.SUCCEEDED).map(lambda _: sent.queue)))
                .map(lambda queue: cleared(queue, self.host).map_error(BlenderRenderingFault.lifted))
                .default_value(Ok(None))
            )

        self.identity.reports.reported(lambda: self.observed(bpy.context).bind(acted))().to_optional()
        return Ok(Some(1.0))

    def runs(self, _context: bpy.types.Context) -> Iterable[rows.Work]:
        """Returns progress rows from cached observation alone."""
        now = datetime.datetime.now(datetime.UTC)

        def local(sent: Sent, progress: Progress, beat: datetime.datetime, heartbeat: Heartbeat) -> rows.Work:
            remaining, phase = progress.remaining(sent.queue), progress.phase(beat, now, heartbeat)
            finish = Estimate.queued(progress, sent.queue, now).bind(lambda estimate: estimate.finish).map(lambda moment: f"finishing {moment.astimezone():%H:%M}").to_list()
            return rows.Work(text=" · ".join((*(sent.queue.jobs[part].title for part, _ in remaining[:1]), phase.label, *finish)), done=float(progress.done(sent.queue)))

        def work(held: Observed, heartbeat: Heartbeat) -> rows.Work:
            return held.sent.map(
                lambda sent: held.progress.map(lambda pair: local(sent, *pair, heartbeat)).default_with(
                    lambda: rows.Work(text=" ".join((sent.target.title, *sent.issued)), done=0.0 if isinstance(sent.target, LocalMachine) else 1.0)
                )
            ).default_with(lambda: rows.Work(text="Preparing", done=0.0))

        return (
            self
            .settings(self.identity)
            .map(lambda queueing: tuple(work(held, queueing.heartbeat) for held in self.state.observed.to_list()))
            .default_with(lambda faults: tuple(rows.Work(text=text, done=0.0) for _, text in rendered(faults)))
        )

    # --- [RUN_CONTROL]

    def resumed(self, call: Call[Empty]) -> BlenderResult[Done]:
        """Resends local queue only when stopped, failed, or canceled."""

        def resent(queue: RenderQueue, target: LocalMachine, progress: Progress, beat: datetime.datetime, queueing: Queueing) -> BlenderResult[Done]:
            phase = progress.phase(beat, datetime.datetime.now(datetime.UTC), queueing.heartbeat)
            resumable = Liveness.STOPPED, Ending.FAILED, Ending.CANCELLED
            return (
                send(target, queue, self.host)
                .map(
                    lambda issued: Done(
                        notes=tuple(("INFO", value) for answer in issued for value in answer.to_option().to_list()),
                        skipped=accumulated(issued).map_error(BlenderRenderingFault.lifted).swap().to_option(),
                    )
                )
                .map_error(BlenderRenderingFault.lifted)
                if phase in resumable
                else Error((BlenderChoiceFault(path=str(self.host.path(queue, RunFile.PROGRESS)), identifier=phase.name, accepted=tuple(value.name for value in resumable)),))
            )

        return paired(self.kept(), self.settings(call.identity)).bind(lambda pair: resent(*pair[0], pair[1]))

    def stopped(self, call: Call[Empty]) -> BlenderResult[Done]:
        """Stops process group, stores cancellation, and posts cancel notice."""

        def halted(queue: RenderQueue, progress: Progress, queueing: Queueing) -> BlenderResult[Done]:
            records = self.host.path(queue, RunFile.PROGRESS)

            def ended(_: None) -> BlenderResult[Done]:
                persisted = revised(records, Progress, lambda held: msgspec.structs.replace(held, ending=Ending.CANCELLED)).map_error(BlenderRenderingFault.lifted)
                return paired(persisted, posted(queueing.listener, Cancelled(frames=len(progress.marks)))).map(lambda _: Done(notes=(("INFO", f"Stopped {queue.stem}"),)))

            stopped: BlenderResult[None] = stop(progress).map_error(BlenderRenderingFault.lifted)
            return Result[None, BlenderFaults].bind(stopped, ended)

        return paired(self.kept(), self.settings(call.identity)).bind(lambda pair: halted(pair[0][0], pair[0][2], pair[1]))

    def logged(self, call: Call[Empty]) -> BlenderResult[Done]:
        """Opens log of last sent queue, else of last plan."""

        def opened_log(plan: Path) -> BlenderResult[Done]:
            log = (
                self.state.observed
                .bind(lambda held: held.sent)
                .filter(lambda record: isinstance(record.target, LocalMachine))
                .map(lambda record: self.host.path(record.queue, RunFile.LOG))
                .default_with(lambda: PlanFile.LOG.beside(plan))
            )
            return called(bpy.ops.wm.path_open, {"filepath": str(log)}).map(lambda _: Done())

        return (
            rooted(bpy.types.WindowManager, call.context)
            .bind(self.queued.read)
            .bind(lambda queued: Option.of_optional(queued.plan).to_result_with(lambda: (BlenderAbsentFault(owner=QueueRun.record.key, key="plan"),)))
            .bind(lambda plan: opened_log(Path(plan)))
        )

    def unscheduled(self, call: Call[Empty]) -> BlenderResult[Done]:
        """Cancels shutdown scheduled after local queue."""

        def cancelling(progress: Progress, power: PowerHold) -> BlenderResult[Done]:
            return (
                cancelled(progress).map_error(BlenderRenderingFault.lifted).map(lambda _: Done(notes=(("INFO", "Shutdown canceled"),)))
                if power.shutdown
                else Error((BlenderAbsentFault(owner=repr(power), key="shutdown"),))
            )

        return paired(self.kept(), self.settings(call.identity)).bind(lambda pair: cancelling(pair[0][2], pair[1].power))

    def packaged(self, call: Call[Empty]) -> BlenderResult[Done]:
        """Writes packed file copy under render output folder."""
        moment = datetime.datetime.now(datetime.UTC).astimezone()

        def done(pack: Path, missing: tuple[BlenderAbsentFault, ...]) -> Done:
            skipped = accumulated(Error((found,)) for found in missing).swap().to_option()
            return Done(notes=(("INFO", str(pack)),), skipped=skipped)

        return (
            call
            .root(bpy.types.Scene)
            .bind(lambda scene: self.pathed.path(scene, moment.date()))
            .bind(lambda path: packed(path.base / "packs" / f"{Path(bpy.data.filepath).stem}_{moment:%Y%m%d-%H%M%S}", compress=Some(True)))
            .bind(
                lambda pack: resolved(item.owner for item in pack.missing).map(
                    lambda owners: done(pack.blend, tuple(BlenderAbsentFault(owner=repr(owner), key=str(item.path)) for item, owner in zip(pack.missing, owners)))
                )
            )
        )

    def managed(self, call: Call[Empty]) -> BlenderResult[Done]:
        """Reads Manager storage, worker tags, and job types into session state."""

        def flamenco(queueing: Queueing) -> BlenderResult[Flamenco]:
            match queueing.target:
                case Flamenco() as target:
                    return Ok(target)
                case other:
                    return Error((BlenderChoiceFault(path=repr(queueing), identifier=other.title, accepted=(Flamenco.title,)),))

        def held(manager: Manager) -> Done:
            self.state.manager = Some(manager)
            return Done(notes=(("INFO", f"{len(manager.tags)} worker tags, job type {'installed' if manager.job_type.is_some() else 'missing'}"),))

        return (
            self
            .settings(call.identity)
            .bind(flamenco)
            .bind(lambda target: Dispatcher.connected().bind(lambda _: exchange.opened(target.server, queried).map_error(BlenderRenderingFault.lifted)))
            .map(held)
        )

    def installed(self, call: Call[Empty]) -> BlenderResult[Done]:
        """Copies job type script into Manager scripts folder."""

        def copied(folder: str) -> BlenderResult[Done]:
            def written() -> Path:
                with importlib.resources.as_file(importlib.resources.files(Flamenco.__module__).joinpath(Flamenco.script)) as script:
                    return Path(shutil.copy(script, Path(folder) / Flamenco.script))

            return catch(written, exception=OSError)().map_error(lambda error: (BlenderWriteFault(request=folder, error=error),)).map(lambda path: Done(notes=(("INFO", str(path)),)))

        return self.settings(call.identity).bind(
            lambda queueing: copied(queueing.scripts.value) if queueing.scripts.enabled else Error((BlenderAbsentFault(owner=repr(call.context.preferences), key="scripts"),))
        )

    def tested(self, call: Call[Empty]) -> BlenderResult[Done]:
        """Posts test notice to listener topic."""
        return self.settings(call.identity).bind(lambda queueing: posted(queueing.listener, Test())).map(lambda _: Done(notes=(("INFO", "Notice posted"),)))


class QueueRun:
    """Render queue commands, operators, status timer, section, and progress row."""

    prepare: ClassVar[Key] = "prepare"
    supervise: ClassVar[Key] = "supervise"
    render: ClassVar[Key] = "render"
    record: ClassVar[TextRecord[Queued]] = TextRecord(key="queue", form=Queued)
    denoise: ClassVar[Attachment[Denoising]] = Attachment(key="queue_denoise", form=Denoising, owner=bpy.types.Scene)
    inputs: ClassVar[Attachment[Rendered]] = Attachment(key="queue_inputs", form=Rendered, owner=bpy.types.Scene)
    pinned: ClassVar[Pinned] = Pinned(ownership=Rendered.ownership, edge=Edge.HEAD, enables=frozenset({Enabled.RENDER}))
    key: ClassVar[Key] = "queue"
    title: ClassVar[str] = "Render Queue"
    description: ClassVar[str] = ""
    popovers: ClassVar[tuple[Single[Empty], ...]] = ()
    condition: ClassVar[Option[Condition]] = Nothing
    opened: ClassVar[bool] = False
    sets: RenderSet
    stack: EffectStack
    settings: Callable[[Identity], BlenderResult[Queueing]]
    state: QueueState
    dispatching: Operator[Dispatch]
    resuming: Operator[Empty]
    stopping: Operator[Empty]
    logging: Operator[Empty]
    unscheduling: Operator[Empty]
    packing: Operator[Empty]
    managing: Operator[Empty]
    installing: Operator[Empty]
    testing: Operator[Empty]

    # --- [OPERATORS]

    def __init__(self, *, sets: RenderSet, stack: EffectStack, settings: Callable[[Identity], BlenderResult[Queueing]]) -> None:
        """Declares queue operators, each binding its dispatcher on call."""
        self.sets, self.stack, self.settings, self.state = sets, stack, settings, QueueState()

        def run[T: msgspec.Struct](step: Callable[[Dispatcher, Call[T]], BlenderResult[Done]]) -> Callable[[Call[T]], BlenderResult[Done]]:
            return lambda call: self.bound(call.identity).bind(lambda dispatcher: step(dispatcher, call))

        def drawn(layout: bpy.types.UILayout, context: bpy.types.Context, properties: bpy.types.OperatorProperties, identity: Identity) -> None:
            lines = (
                paired(self.bound(identity), self.dispatching.bound(identity).bind(lambda pair: pair[1].read(properties)))
                .bind(lambda pair: paired(pair[0].planned(context, pair[1], datetime.datetime.now(datetime.UTC)), pair[0].settings(identity)))
                .map(lambda pair: (pair[1].target.title, f"{len(pair[0].scopes)} jobs, {sum(len(ranged(bpy.data.scenes[scope.scene]).numbers) for scope in pair[0].scopes)} frames"))
                .default_with(lambda faults: tuple(text for _, text in rendered(faults)))
            )
            for line in lines:
                layout.label(text=line)
            for field in Dispatch.__struct_fields__:
                layout.prop(properties, field)

        self.dispatching = Operator[Dispatch](
            key="dispatch",
            title="Render Queue",
            description="Plan jobs, write job files in background process, and send queue to target",
            icon=Some("RENDER_ANIMATION"),
            condition=Some(Preflight.condition),
            form=Dispatch,
            run=run(Dispatcher.dispatched),
            undo=False,
            invoke=Some(Properties()),
            draw=Some(drawn),
        )
        self.resuming = Operator(
            key="resume", title="Resume Queue", description="Render remaining frames of stopped, failed, or canceled queue", icon=Some("PLAY"), undo=False, form=Empty, run=run(Dispatcher.resumed)
        )
        self.stopping = Operator(key="stop", title="Stop Queue", description="Stop queue and its renders", icon=Some("CANCEL"), undo=False, form=Empty, run=run(Dispatcher.stopped))
        self.logging = Operator(key="log", title="Open Queue Log", description="Open last dispatch log", icon=Some("TEXT"), undo=False, form=Empty, run=run(Dispatcher.logged))
        self.unscheduling = Operator(
            key="shutdown_cancel", title="Cancel Shutdown", description="Cancel scheduled shutdown", icon=Some("QUIT"), undo=False, form=Empty, run=run(Dispatcher.unscheduled)
        )
        self.packing = Operator(
            key="pack", title="Pack Project", description="Write packed file copy under render output folder", icon=Some("PACKAGE"), undo=False, form=Empty, run=run(Dispatcher.packaged)
        )
        self.managing = Operator(
            key="flamenco_read", title="Read Manager", description="Read Manager storage, worker tags, and job types", icon=Some("FILE_REFRESH"), undo=False, form=Empty, run=run(Dispatcher.managed)
        )
        self.installing = Operator(
            key="flamenco_install", title="Install Job Type", description="Copy job type script into Manager scripts folder", icon=Some("IMPORT"), undo=False, form=Empty, run=run(Dispatcher.installed)
        )
        self.testing = Operator(key="notice_test", title="Test Notice", description="Post test notice to listener topic", icon=Some("INFO"), undo=False, form=Empty, run=run(Dispatcher.tested))

    # --- [BINDING]

    @property
    def prerequisites(self) -> tuple[Registrable, ...]:
        """Queue, denoise, and input stores with render set, frame size, routing, and path bindings read."""
        return (self.record, self.denoise, self.inputs, *self.sets.prerequisites, FrameSize.attachment, PassSet.attachment, ScenePaths.attachment)

    @property
    def bindings(self) -> Option[Mapping[MemberDescriptorType, Choices]]:
        """Cached Manager tags as `Flamenco.tag` choices."""
        return Some({vars(Flamenco)["tag"]: self.state.tags})

    def bound(self, identity: Identity) -> BlenderResult[Dispatcher]:
        """Binds dispatcher to identity stores, row sets, stack, and session state."""

        def built(parts: tuple[tuple[tuple[Host, Members], Framed], tuple[tuple[PassRouting, Pathed], tuple[Attached[Queued], tuple[Attached[Denoising], Attached[Rendered]]]]]) -> Dispatcher:
            ((host, members), framed), ((routing, pathed), (queued, (denoise, inputs))) = parts
            return Dispatcher(
                identity=identity,
                host=host,
                members=members,
                framed=framed,
                routing=routing,
                pathed=pathed,
                queued=queued,
                denoise=denoise,
                inputs=inputs,
                stack=self.stack,
                state=self.state,
                settings=self.settings,
            )

        host = paired(identity.name(Name.CLI, QueueRun.supervise), identity.name(Name.CLI, QueueRun.render)).map(
            lambda names: Host(binary=bpy.app.binary_path, supervise_command=names[0], render_command=names[1], folder=str(identity.folder))
        )
        return paired(
            paired(paired(host, self.sets.bound(identity)), FrameSize().bound(identity)),
            paired(
                paired(PassSet().bound(identity), ScenePaths().bound(identity)),
                paired(QueueRun.record.attachment.bound(identity), paired(QueueRun.denoise.bound(identity), QueueRun.inputs.bound(identity))),
            ),
        ).map(built)

    # --- [COMMANDS]

    @staticmethod
    def commands(dispatcher: Dispatcher) -> tuple[CliCommand[Preparing], CliCommand[Run], CliCommand[Run | RenderFrames]]:
        """Returns `prepare`, `supervise`, and `render` command rows."""
        return (
            CliCommand(key=QueueRun.prepare, decoder=msgspec.json.Decoder(Preparing), run=dispatcher.prepared),
            CliCommand(key=QueueRun.supervise, decoder=msgspec.json.Decoder(Run), run=dispatcher.supervised),
            CliCommand(key=QueueRun.render, decoder=msgspec.json.Decoder[Run | RenderFrames](Run | RenderFrames), run=dispatcher.commanded),
        )

    # --- [SECTION]

    def realized(self, identity: Identity) -> BlenderResult[Drawn]:
        """Builds queue section."""
        return self.bound(identity).bind(
            lambda dispatcher: Single(
                key=self.key,
                title=self.title,
                rows=(
                    rows.OperatorButton(target=self.dispatching),
                    rows.Progress(runs=dispatcher.runs),
                    Status(),
                    rows.Caption(
                        rows=tuple(rows.OperatorButton(target=command, text=Some("")) for command in (self.resuming, self.stopping, self.logging, self.unscheduling, self.packing)),
                        flow=rows.Flow.ALIGNED,
                        separator=Nothing,
                    ),
                ),
            ).realized(identity)
        )

    @property
    def setup(self) -> Single:
        """Preferences section of Manager reads, job type install, and notice test."""
        return Single(
            key=f"{self.key}_setup",
            title=self.title,
            rows=(rows.Caption(rows=tuple(rows.OperatorButton(target=command) for command in (self.managing, self.installing, self.testing)), flow=rows.Flow.ALIGNED, separator=Nothing),),
        )

    def drawn(self, identity: Identity) -> BlenderResult[rows.RowDraw]:
        """Returns queue section as inline panel draw."""
        return self.realized(identity).map(lambda drawn: drawn.inline)

    # --- [REGISTRATION]

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[BlenderResult[Acquisitions]]:
        """Holds deferred viewport restore while registered."""
        with ExitStack() as scope:

            def joined(dispatcher: Dispatcher) -> BlenderResult[Acquisitions]:
                restore = scope.enter_context(identity.reports.deferred(dispatcher.restored))
                scope.callback(identity.reports.reported(dispatcher.restored))
                return Ok((
                    *self.commands(dispatcher),
                    self.dispatching,
                    self.resuming,
                    self.stopping,
                    self.logging,
                    self.unscheduling,
                    self.packing,
                    self.managing,
                    self.installing,
                    self.testing,
                    Timer(body=lambda: dispatcher.ticked(restore)),
                ))

            yield self.bound(identity).bind(joined)


# --- [OPERATIONS] -----------------------------------------------------------------------


def ranged(scene: bpy.types.Scene) -> Frames:
    """Returns inclusive frame range and step of scene."""
    return Frames(start=scene.frame_start, end=scene.frame_end, step=scene.frame_step)


def scoped(dispatcher: Dispatcher, scope: JobScope, plan: Plan, source: Path, plan_file: Path, part: int) -> BlenderResult[Job]:
    """Writes scope's packed job file from saved source."""
    moment = plan.moment.astimezone()
    render = msgspec.structs.replace(scope, temporal=None)
    followed = any(msgspec.structs.replace(other, temporal=None) == render for other in plan.scopes if other.temporal is not None)

    def composed(scene: bpy.types.Scene) -> BlenderResult[OutputPath]:
        return dispatcher.pathed.path(scene, moment.date()).map(lambda path: path.copied)

    def filed(scene: bpy.types.Scene, path: OutputPath) -> BlenderResult[OutputPath]:
        return path.scene_output(scene).bind(lambda text: Pathed.filepath.write(scene.render, text)).map(lambda _: path)

    def motion(scene: bpy.types.Scene) -> BlenderResult[None]:
        def blurred(setting: Setting) -> BlenderResult[None]:
            return (
                struct_at(scene, setting.descriptor.owner)
                .map(lambda owner: setting.descriptor.write(owner, setting.accepted[0]).map(lambda _: None))
                .default_with(lambda: Error((BlenderAbsentFault(owner=repr(scene), key=setting.descriptor.path),)))
            )

        return accumulated(motioned(layer.name) for layer in dispatcher.routing.used(scene)).bind(
            lambda pairs: paired(
                required(scene, bpy.context, tuple(request for request, _ in pairs), dispatcher.routing.precision), accumulated(map(blurred, dict.fromkeys(setting for _, setting in pairs)))
            ).map(lambda _: None)
        )

    def applied(scene: bpy.types.Scene) -> BlenderResult[tuple[OutputPath, tuple[Side, Side]]]:
        if followed and scope.temporal is None:
            scene.frame_step = 1
        if isinstance(render.camera, Fixed):
            scene.camera = bpy.data.objects[render.camera.camera]
            for marker in scene.timeline_markers:
                marker.camera = None
        scene.render.use_stamp_memory = True
        layers = (
            Option
            .of_optional(render.layer)
            .map(lambda name: accumulated(dispatcher.routing.use.write(layer, layer.name == name) for layer in scene.view_layers).map(lambda _: None))
            .default_value(Ok(None))
        )
        return paired(paired(layers, dispatcher.framed.grown(scene)), motion(scene) if followed else Ok(None)).bind(
            lambda parts: composed(scene).bind(lambda path: filed(scene, path)).bind(lambda path: dispatcher.routing.reconciled(scene, bpy.context, path).map(lambda _: (path, parts[0][1][1])))
        )

    def denoised(scene: bpy.types.Scene, record: TemporalDenoise, copied: OutputPath) -> BlenderResult[OutputPath]:
        def plane(files: tuple[Written, ...], source: str) -> BlenderResult[Plane]:
            return Option.of_optional(next((Plane(path=file.path, name=item) for file in files for item, held in file.passes.items() if held == source), None)).to_result_with(
                lambda: (BlenderAbsentFault(owner=repr(scene), key=source),)
            )

        def muted(path: OutputPath) -> OutputPath:
            for effect in scene.compositor_effects:
                if Option.of_optional(effect.node_group).filter(rendering).is_some():
                    effect.enable_for_render = False
            return path

        frames = ranged(scene)
        return (
            PassSet()
            .written(scene, frames.start, dispatcher.identity, path=Some(copied))
            .bind(lambda files: paired(plane(files, "Combined"), plane(files, "Vector")))
            .map(
                lambda planes: Rendered(
                    color=msgspec.structs.replace(planes[0], path=Path(planes[0].path).relative_to(copied.base).as_posix()),
                    motion=msgspec.structs.replace(planes[1], path=Path(planes[1].path).relative_to(copied.base).as_posix()),
                    first=frames.start,
                    last=frames.end,
                )
            )
            .bind(
                lambda held: (
                    dispatcher.pathed.attached
                    .read(scene)
                    .bind(lambda outputs: dispatcher.pathed.attached.write(scene, msgspec.structs.replace(outputs, version=outputs.version + 1)))
                    .bind(lambda _: composed(scene))
                    .bind(lambda path: filed(scene, path))
                    .bind(lambda path: dispatcher.routing.reconciled(scene, bpy.context, path).map(lambda _: path))
                    .map(muted)
                    .bind(lambda path: paired(dispatcher.inputs.write(scene, held), dispatcher.denoise.write(scene, Denoising(temporal=Gated(enabled=True, value=record)))).map(lambda _: path))
                )
            )
        )

    def digested(path: Path) -> BlenderResult[bytes]:
        def read() -> bytes:
            with path.open("rb") as file:
                return hashlib.file_digest(file, "sha256").digest()

        return catch(read, exception=OSError)().map_error(lambda error: (BlenderReadFault(request=path, error=error),))

    def written(scene: bpy.types.Scene, path: OutputPath, pixels: tuple[Side, Side]) -> BlenderResult[Job]:
        used = dispatcher.routing.used(scene)
        camera = (scope.camera.camera,) if isinstance(scope.camera, Fixed) else ()
        name = " ".join(bpy.path.clean_name(part) for part in (scope.scene, *Option.of_optional(scope.layer).to_list(), *camera, *(("temporal",) if scope.temporal else ())))
        file = path.base / "snapshots" / plan_file.name.removesuffix(PlanFile.PLAN) / f"{part}_{name}.blend"
        layer = Option.of_optional(scope.layer).default_with(lambda: scene.view_layers[0].name)
        outputs = accumulated(path.text(layer, output) for output in Output).map(lambda texts: dict(zip(Output, (posixpath.join(path.root, text) for text in texts))))

        def job(parts: tuple[tuple[dict[Output, str], bytes], int]) -> Job:
            (texts, digest), samples = parts
            return Job(
                file=str(file),
                digest=digest,
                scene=scene.name,
                layer=scope.layer,
                engine=scene.render.engine,
                frames=ranged(scene),
                camera=scope.camera,
                pixels=pixels,
                samples=samples,
                output_root=str(path.base),
                outputs=texts,
                passes=tuple(sorted({descriptor.path for held in used for _, descriptor in pass_flags() for owner in struct_at(held, descriptor.owner).to_list() if bool(descriptor.read(owner))})),
            )

        return (
            dispatcher.pathed.scope
            .write(scene, path)
            .bind(lambda _: called(bpy.ops.file.make_paths_absolute, {}))
            .bind(lambda _: paired(called(bpy.ops.file.pack_all, {}), called(bpy.ops.file.pack_libraries, {})))
            .bind(lambda _: saved(file, SaveMode.COPY, compress=Some(True)))
            .bind(lambda saved_file: paired(paired(outputs, digested(saved_file)), accumulated(dispatcher.members.samples_of(scene, held) for held in used).map(sum)))
            .map(job)
        )

    def prepared(_: BlendFile) -> BlenderResult[Job]:
        scene = bpy.data.scenes[scope.scene]
        return (
            applied(scene)
            .bind(lambda parts: Option.of_optional(scope.temporal).map(lambda record: denoised(scene, record, parts[0]).map(lambda path: (path, parts[1]))).default_value(Ok(parts)))
            .bind(lambda parts: written(scene, *parts))
        )

    return session_loaded(OpenFile(path=source)).bind(prepared)


def posted(listener: Listener, notice: Notice) -> BlenderResult[None]:
    """Posts notice, requiring online access for an enabled topic."""
    return (Dispatcher.connected() if listener.topic.enabled else Ok(None)).bind(lambda _: published(listener, notice).map_error(BlenderRenderingFault.lifted))


def peaked(path: Path) -> BlenderResult[Option[int]]:
    """Reads EXR `Memory` stamp in bytes."""
    return opened(str(path), lambda image: Ok(Option.of_optional(image.spec().getattribute("Memory")).map(lambda text: int(text.removesuffix("M")) * 2**20))).map_error(BlenderImagingFault.lifted)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Denoising", "Dispatch", "Dispatcher", "Observed", "Plan", "PlanFile", "Preparing", "QueueRun", "QueueState", "Queued", "Queueing", "Reach", "Sent", "peaked", "posted", "ranged", "scoped"]
