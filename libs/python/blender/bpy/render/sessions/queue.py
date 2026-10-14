"""Native render queue dispatch, background commands, and persistent run progress."""

import base64
from collections.abc import Callable, Generator, Iterable, Mapping
from contextlib import closing, contextmanager, ExitStack
import datetime
from enum import StrEnum
from fractions import Fraction
from functools import reduce
import hashlib
import importlib.resources
from operator import itemgetter
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
import OpenImageIO

from blender.bpy.commands.calls import called
from blender.bpy.commands.operators import Acquired, Call, Done, Empty, Operator, Properties
from blender.bpy.data.ids.store import resolved
from blender.bpy.data.rna.conditions import AllOf, check, Condition, Setting
from blender.bpy.data.rna.descriptors import Descriptor, struct_at
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
from blender.rendering.batch.queue import Bound, Fixed, Frames, Job, load, Marker, RenderFrames, RenderQueue, store
from blender.rendering.batch.supervision import cleared, Host, LocalMachine, Run, RunFile, stop, supervise
from blender.rendering.batch.targets import send, Target
from blender.rendering.faults import RenderingFaults, RenderingReadFault, RenderingResult, RenderingRunFault
from blender.rendering.outputs.framing import Side
from blender.rendering.outputs.naming import Output

# --- [TYPES] ----------------------------------------------------------------------------


class Reach(Choice):
    """Native scenes, layers, or selected cameras included in a dispatch."""

    SETS = 1, "Render Sets"
    SCENE = 2, "Scene"
    LAYER = 3, "View Layer"
    CAMERAS = 4, "Selected Cameras"


class PlanFile(StrEnum):
    """Files produced by a dispatch, named from the same plan stem."""

    PLAN = ".plan.json"
    LOG = ".plan.log"
    SENT = ".sent.json"

    def beside(self, plan: Path) -> Path:
        """Returns this dispatch file beside its plan."""
        return plan.with_name(f"{plan.name.removesuffix(PlanFile.PLAN)}{self}")


# --- [MODELS] ---------------------------------------------------------------------------


class Dispatch(msgspec.Struct, frozen=True, kw_only=True):
    """Scope selected by the native dispatch dialog."""

    reach: Annotated[Reach, msgspec.Meta(title="Render", description="Scenes, layers, or selected cameras the queue renders")] = Reach.SETS


class Plan(msgspec.Struct, frozen=True, kw_only=True):
    """Ordered native job scopes and the dispatch boundary's clock reading."""

    moment: datetime.datetime
    scopes: Annotated[tuple[JobScope, ...], msgspec.Meta(min_length=1)]


class Preparing(msgspec.Struct, frozen=True, kw_only=True):
    """Stored plan and target consumed by the background prepare command."""

    plan: str
    target: Target


class Sent(msgspec.Struct, frozen=True, kw_only=True):
    """Self-contained queue and every successful submitted path or farm identifier."""

    queue: RenderQueue
    target: Target
    issued: tuple[str, ...]


class Queued(msgspec.Struct, frozen=True, kw_only=True):
    """Plan path retained in the native file-level text record."""

    plan: str | None = None


class Denoising(msgspec.Struct, frozen=True, kw_only=True):
    """Scene preference for a temporal reconstruction job following its render."""

    temporal: Gated[TemporalDenoise] = Gated(enabled=False, value=TemporalDenoise())


class Queueing(msgspec.Struct, frozen=True, kw_only=True):
    """Shared target, notification, power, heartbeat, and Manager-script preferences."""

    target: Annotated[Target, msgspec.Meta(title="Target", description="Where the queue renders")] = LocalMachine()
    listener: Listener = Listener()
    power: PowerHold = PowerHold()
    heartbeat: Heartbeat = Heartbeat()
    scripts: Annotated[Gated[str], msgspec.Meta(title="Flamenco Scripts", description="The Manager's job-type scripts folder")] = Gated(enabled=False, value="")


class Observed(msgspec.Struct, frozen=True, kw_only=True):
    """Submission and local progress last read by the native status timer."""

    sent: Option[Sent]
    progress: Option[tuple[Progress, datetime.datetime]]


# --- [SERVICES] -------------------------------------------------------------------------


class QueueState:
    """Native-session observations and prior viewport states held by one queue row."""

    def __init__(self) -> None:
        """Starts without a Manager answer, observed dispatch, or paused viewport."""
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
    """Native stores and session resources consumed by queue operations."""

    identity: Identity
    host: Host
    members: Members
    condition: AllOf
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
        """Admits network work while `bpy.app.online_access` holds, the value preferences and `--offline-mode` decide together."""
        preference = Setting(descriptor=Descriptor(root=bpy.types.Preferences, owner="system", prop=bpy.types.PreferencesSystem.bl_rna.properties["use_online_access"]))
        return Ok(None) if bpy.app.online_access else Error((BlenderUnmetFault(requirement=preference),))

    # --- [PLANNING]

    def planned(self, context: bpy.types.Context, dispatch: Dispatch, moment: datetime.datetime) -> BlenderResult[Plan]:
        """Plans the selected native scopes, followed by their enabled temporal jobs."""

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
                    return (
                        Option
                        .of_optional(context.view_layer)
                        .map(lambda layer: (JobScope(scene=scene.name, layer=layer.name, camera=bound(scene)),))
                        .to_result_with(lambda: (BlenderAbsentFault(owner=repr(context), key="view_layer"),))
                    )
                case Reach.CAMERAS:
                    return Ok(tuple(JobScope(scene=scene.name, camera=Fixed(camera=camera.name)) for camera in context.selected_objects if camera.type == "CAMERA"))

        def denoised(scope: JobScope) -> BlenderResult[tuple[JobScope, ...]]:
            return self.denoise.read(bpy.data.scenes[scope.scene]).map(lambda held: (scope, *((msgspec.structs.replace(scope, temporal=held.temporal.value),) if held.temporal.enabled else ())))

        scene: BlenderResult[bpy.types.Scene] = Option.of_optional(context.scene).to_result_with(lambda: (BlenderAbsentFault(owner=repr(context), key="scene"),))
        return (
            Result[bpy.types.Scene, BlenderFaults]
            .bind(scene, reached)
            .bind(lambda scopes: accumulated(map(denoised, scopes)))
            .map(lambda groups: tuple(scope for group in groups for scope in group))
            .bind(lambda scopes: Ok(Plan(moment=moment, scopes=scopes)) if scopes else Error((BlenderAbsentFault(owner=repr(context), key=dispatch.reach.name),)))
        )

    # --- [DISPATCH]

    def dispatched(self, call: Call[Dispatch]) -> BlenderResult[Done]:
        """Checks the plan, steps versions, saves the file, stores the plan, starts `prepare`, and pauses viewport renders."""
        context, identity = call.context, call.identity
        moment = datetime.datetime.now(datetime.UTC)
        day = moment.astimezone().date()

        def gated(plan: Plan, queueing: Queueing) -> BlenderResult[None]:
            online = Dispatcher.connected() if isinstance(queueing.target, Flamenco) else Ok(None)
            return paired(checked(self.pathed, plan.scopes, day), paired(check(self.condition, context), online)).map(lambda _: None)

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
                return detached(argv, PlanFile.LOG.beside(path)).map_error(lambda faults: (BlenderRenderingFault(faults=faults),))

            return (
                accumulated(self.pathed.took(scene, day) for scene in scenes)
                .bind(lambda _: store(path, msgspec.json.encode(plan, order="deterministic")).map_error(lambda faults: (BlenderRenderingFault(faults=faults),)))
                .bind(lambda _: self.queued.write(manager, Queued(plan=str(path))))
                .bind(lambda _: saved(Path(bpy.data.filepath), SaveMode.ACTIVE))
                .bind(lambda source: identity.name(Name.CLI, QueueRun.prepare).bind(lambda command: started(source, command)))
                .map(lambda log: Done(notes=(("INFO", f"{len(plan.scopes)} jobs preparing, logged at {log}"),), skipped=paused(manager).swap().to_option()))
            )

        manager: BlenderResult[bpy.types.WindowManager] = Option.of_optional(context.window_manager).to_result_with(lambda: (BlenderAbsentFault(owner=repr(context), key="window_manager"),))
        return (
            paired(paired(self.planned(context, call.record, moment), self.settings(identity)), manager)
            .bind(lambda parts: gated(*parts[0]).map(lambda _: (*parts[0], parts[1])))
            .bind(lambda parts: written(*parts))
        )

    def prepared(self, arguments: Preparing, trailing: tuple[str, ...]) -> BlenderResult[Sent]:
        """Writes each job file of a stored plan, sends the queue to its target, and stores the `Sent` record."""
        if trailing:
            return Error((BlenderChoiceFault(path=QueueRun.prepare, identifier=repr(trailing), accepted=()),))
        path = Path(arguments.plan)
        source = Path(bpy.data.filepath)

        def sent(jobs: tuple[Job, ...]) -> BlenderResult[Sent]:
            queue = RenderQueue(jobs=jobs, blender=bpy.app.version_string, extension=self.identity.manifest.version)
            answer = send(arguments.target, queue, self.host)
            match answer.tag:
                case "error":
                    return Error((BlenderRenderingFault(faults=answer.error),))
                case "ok":
                    issued = answer.ok
            record = Sent(queue=queue, target=arguments.target, issued=tuple(value for result in issued for value in result.to_option().to_list()))
            for result in issued:
                match result.tag:
                    case "ok":
                        self.identity.reports.logger.info(result.ok)
                    case "error":
                        self.identity.reports.hold((BlenderRenderingFault(faults=result.error),))
            return store(PlanFile.SENT.beside(path), msgspec.json.encode(record, order="deterministic")).map_error(lambda faults: (BlenderRenderingFault(faults=faults),)).map(lambda _: record)

        loaded: BlenderResult[Plan] = load(path, Plan).map_error(lambda faults: (BlenderRenderingFault(faults=faults),))
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
        """Acquires deferred native inputs before rendering and consuming each requested frame."""

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
        """Executes local progress or a farm worker's exact native frame request."""
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
        """Supervises local processes under the native power hold and awaits final notices."""
        if trailing:
            return Error((BlenderChoiceFault(path=QueueRun.supervise, identifier=repr(trailing), accepted=()),))

        def ran(queue: RenderQueue, queueing: Queueing) -> BlenderResult[Progress]:
            records = self.host.path(queue, RunFile.PROGRESS)

            def completed(progress: Progress) -> BlenderResult[Progress]:
                self.identity.reports.reported(posted)(
                    queueing.listener, Completed(frames=len(progress.marks), seconds=sum((Fraction(mark.seconds) for mark in progress.marks), Fraction()))
                ).default_value(None)
                return (shut() if queueing.power.shutdown else Ok(None)).map_error(lambda faults: (BlenderRenderingFault(faults=faults),)).map(lambda _: progress)

            def failed(faults: RenderingFaults) -> BlenderResult[Progress]:
                notice: Notice
                match faults:
                    case (RenderingRunFault(error=int() as code), *_) if code != 1:
                        notice = Exhausted(job=queue.stem, attempts=run.target.retries + 1)
                    case _:
                        notice = Failed(job=queue.stem, reason=" ".join(text for _, text in rendered((BlenderRenderingFault(faults=faults),))))
                self.identity.reports.reported(posted)(queueing.listener, notice).default_value(None)
                return Error((BlenderRenderingFault(faults=faults),))

            answer = observed(records)
            match answer.tag:
                case "error":
                    return Error((BlenderRenderingFault(faults=answer.error),))
                case "ok":
                    found = answer.ok
            if not Progress.begun(found.map(itemgetter(0)), queue, os.getpid()).marks:
                self.identity.reports.reported(posted)(queueing.listener, Started(jobs=len(queue.jobs), frames=len(queue.steps))).default_value(None)
            result = queueing.power.held(lambda: supervise(run.target, queue, self.host, queueing.heartbeat))
            match result.tag:
                case "ok":
                    return completed(result.ok)
                case "error":
                    return failed(result.error)

        return paired(load(Path(run.queue), RenderQueue).map_error(lambda faults: (BlenderRenderingFault(faults=faults),)), self.settings(self.identity)).bind(lambda pair: ran(*pair))

    def rendered(self, run: Run) -> BlenderResult[tuple[StageMark, ...]]:
        """Renders the first remaining scheduled job run and persists each native frame mark."""

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
                    self.identity.reports.reported(posted)(queueing.listener, Estimated(estimate=estimate)).default_value(None)
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
                persisted: BlenderResult[Progress] = (
                    load(records, Progress)
                    .map(lambda held: msgspec.structs.replace(held, marks=(*held.marks, mark)))
                    .bind(lambda held: store(records, msgspec.json.encode(held, order="deterministic")).map(lambda _: held))
                    .map_error(lambda faults: (BlenderRenderingFault(faults=faults),))
                )
                return Result[Progress, BlenderFaults].bind(persisted, lambda held: finished(held, mark))

            def runs(progress: Progress) -> BlenderResult[tuple[StageMark, ...]]:
                remaining = frozenset(progress.remaining(queue))
                steps = tuple(step for step in progress.pending if step in remaining)
                return (
                    self.frames(bpy.data.scenes[queue.jobs[steps[0][0]].scene], tuple(frame for _, frame in steps), lambda frame, seconds: marked(steps[0][0], frame, seconds).bind(appended))
                    if steps
                    else Ok(())
                )

            loaded: BlenderResult[Progress] = load(records, Progress).map_error(lambda faults: (BlenderRenderingFault(faults=faults),))
            return Result[Progress, BlenderFaults].bind(loaded, runs)

        return paired(load(Path(run.queue), RenderQueue).map_error(lambda faults: (BlenderRenderingFault(faults=faults),)), self.settings(self.identity)).bind(lambda pair: ran(*pair))

    # --- [SESSION_PROGRESS]

    def observed(self, context: bpy.types.Context) -> BlenderResult[Option[Observed]]:
        """Reads the dispatch file and local progress, admitting a not-yet-written submission."""

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

        manager: BlenderResult[bpy.types.WindowManager] = Option.of_optional(context.window_manager).to_result_with(lambda: (BlenderAbsentFault(owner=repr(context), key="window_manager"),))
        return (
            Result[bpy.types.WindowManager, BlenderFaults]
            .bind(manager, self.queued.read)
            .bind(
                lambda queued: Option.of_optional(queued.plan).map(lambda plan: read(Path(plan)).map(Some).map_error(lambda faults: (BlenderRenderingFault(faults=faults),))).default_value(Ok(Nothing))
            )
        )

    def kept(self) -> BlenderResult[tuple[RenderQueue, LocalMachine, Progress, datetime.datetime]]:
        """Returns the observed local run or the absence of resumable local progress."""
        match self.state.observed.bind(lambda held: held.sent.bind(lambda sent: held.progress.map(lambda pair: (sent, *pair)))).to_optional():
            case (Sent(queue=queue, target=LocalMachine() as target), progress, beat):
                return Ok((queue, target, progress, beat))
            case _:
                return Error((BlenderAbsentFault(owner=repr(self.queued), key=RunFile.PROGRESS),))

    def restored(self) -> BlenderResult[Option[str]]:
        """Restores surviving viewports through the acquisition's admitted deferred callback."""
        paused, self.state.paused = self.state.paused, ()
        return accumulated(prior.applied(now.area) for at, prior in paused for now in Located.found(at.window, at.area.as_pointer()).to_list()).map(lambda _: Nothing)

    def ticked(self, restore: Callable[[], None]) -> BlenderResult[Option[float]]:
        """Keeps final progress after cleanup and schedules viewport restoration at an ending."""

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
                .map(lambda queue: cleared(queue, self.host).map_error(lambda faults: (BlenderRenderingFault(faults=faults),)))
                .default_value(Ok(None))
            )

        self.identity.reports.reported(lambda: self.observed(bpy.context).bind(acted))().default_value(None)
        return Ok(Some(1.0))

    def runs(self, _context: bpy.types.Context) -> Iterable[rows.Work]:
        """Draws cached progress without network requests or progress-record reads."""
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
        """Resends only a stopped, failed, or cancelled local queue."""

        def resent(queue: RenderQueue, target: LocalMachine, progress: Progress, beat: datetime.datetime, queueing: Queueing) -> BlenderResult[Done]:
            phase = progress.phase(beat, datetime.datetime.now(datetime.UTC), queueing.heartbeat)
            resumable = Liveness.STOPPED, Ending.FAILED, Ending.CANCELLED
            return (
                send(target, queue, self.host)
                .map(
                    lambda issued: Done(
                        notes=tuple(("INFO", value) for answer in issued for value in answer.to_option().to_list()),
                        skipped=accumulated(issued).map_error(lambda faults: (BlenderRenderingFault(faults=faults),)).swap().to_option(),
                    )
                )
                .map_error(lambda faults: (BlenderRenderingFault(faults=faults),))
                if phase in resumable
                else Error((BlenderChoiceFault(path=str(self.host.path(queue, RunFile.PROGRESS)), identifier=phase.name, accepted=tuple(value.name for value in resumable)),))
            )

        return paired(self.kept(), self.settings(call.identity)).bind(lambda pair: resent(*pair[0], pair[1]))

    def stopped(self, call: Call[Empty]) -> BlenderResult[Done]:
        """Stops the process group, persists cancellation, and awaits its independent notice."""

        def halted(queue: RenderQueue, progress: Progress, queueing: Queueing) -> BlenderResult[Done]:
            records = self.host.path(queue, RunFile.PROGRESS)

            def ended(_: None) -> BlenderResult[Done]:
                persisted = (
                    load(records, Progress)
                    .map(lambda held: msgspec.structs.replace(held, ending=Ending.CANCELLED))
                    .bind(lambda held: store(records, msgspec.json.encode(held, order="deterministic")))
                    .map_error(lambda faults: (BlenderRenderingFault(faults=faults),))
                )
                return paired(persisted, posted(queueing.listener, Cancelled(frames=len(progress.marks)))).map(lambda _: Done(notes=(("INFO", f"Stopped {queue.stem}"),)))

            stopped: BlenderResult[None] = stop(progress).map_error(lambda faults: (BlenderRenderingFault(faults=faults),))
            return Result[None, BlenderFaults].bind(stopped, ended)

        return paired(self.kept(), self.settings(call.identity)).bind(lambda pair: halted(pair[0][0], pair[0][2], pair[1]))


class QueueRun(msgspec.Struct, frozen=True, kw_only=True):
    """Row of the render queue: its commands, operator rows, status timer, section, and progress row."""

    dispatch: ClassVar[Key] = "dispatch"
    resume: ClassVar[Key] = "resume"
    stop: ClassVar[Key] = "stop"
    log: ClassVar[Key] = "log"
    shutdown_cancel: ClassVar[Key] = "shutdown_cancel"
    pack: ClassVar[Key] = "pack"
    flamenco_read: ClassVar[Key] = "flamenco_read"
    flamenco_install: ClassVar[Key] = "flamenco_install"
    notice_test: ClassVar[Key] = "notice_test"
    prepare: ClassVar[Key] = "prepare"
    supervise: ClassVar[Key] = "supervise"
    render: ClassVar[Key] = "render"
    record: ClassVar[TextRecord[Queued]] = TextRecord(key="queue", form=Queued)
    denoise: ClassVar[Attachment[Denoising]] = Attachment(key="queue_denoise", form=Denoising, owner=bpy.types.Scene)
    inputs: ClassVar[Attachment[Rendered]] = Attachment(key="queue_inputs", form=Rendered, owner=bpy.types.Scene)
    page_rows: ClassVar[tuple[Registrable, ...]] = (record, denoise, inputs)
    pinned: ClassVar[Pinned] = Pinned(ownership=Rendered.ownership, edge=Edge.HEAD, enables=frozenset({Enabled.RENDER}))
    key: ClassVar[Key] = "queue"
    title: ClassVar[str] = "Render Queue"
    popovers: ClassVar[tuple[Single[Empty], ...]] = ()
    condition: ClassVar[Option[Condition]] = Nothing
    sets: RenderSet
    stack: EffectStack
    settings: Callable[[Identity], BlenderResult[Queueing]]
    state: QueueState = msgspec.field(default_factory=QueueState)

    # --- [BINDING]

    @property
    def bindings(self) -> Option[Mapping[MemberDescriptorType, Choices]]:
        """Supplies cached Manager tags for the unchanged native target preference."""
        return Some({vars(Flamenco)["tag"]: self.state.tags})

    def bound(self, identity: Identity) -> BlenderResult[Dispatcher]:
        """Dispatcher over the identity's stores, the row's sets and stack, and the row's session state."""

        def built(
            parts: tuple[tuple[tuple[Host, Members], tuple[AllOf, Framed]], tuple[tuple[PassRouting, Pathed], tuple[Attached[Queued], tuple[Attached[Denoising], Attached[Rendered]]]]],
        ) -> Dispatcher:
            ((host, members), (condition, framed)), ((routing, pathed), (queued, (denoise, inputs))) = parts
            return Dispatcher(
                identity=identity,
                host=host,
                members=members,
                condition=condition,
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
            paired(paired(host, self.sets.bound(identity)), paired(Preflight().condition, FrameSize().bound(identity))),
            paired(
                paired(PassSet().bound(identity), ScenePaths().bound(identity)),
                paired(QueueRun.record.attachment.bound(identity), paired(QueueRun.denoise.bound(identity), QueueRun.inputs.bound(identity))),
            ),
        ).map(built)

    # --- [COMMANDS]

    @staticmethod
    def commands(dispatcher: Dispatcher) -> tuple[CliCommand[Preparing], CliCommand[Run], CliCommand[Run | RenderFrames]]:
        """`prepare`, `supervise`, and `render` command rows."""
        return (
            CliCommand(key=QueueRun.prepare, decoder=msgspec.json.Decoder(Preparing), run=dispatcher.prepared),
            CliCommand(key=QueueRun.supervise, decoder=msgspec.json.Decoder(Run), run=dispatcher.supervised),
            CliCommand(key=QueueRun.render, decoder=msgspec.json.Decoder[Run | RenderFrames](Run | RenderFrames), run=dispatcher.commanded),
        )

    @staticmethod
    def operators(dispatcher: Dispatcher) -> tuple[Operator[Dispatch] | Operator[Empty], ...]:
        """Dispatch, resume, stop, log, shutdown cancel, pack, Flamenco, and notice test rows."""
        identity = dispatcher.identity

        def drawn(layout: bpy.types.UILayout, context: bpy.types.Context, properties: bpy.types.OperatorProperties) -> None:
            planned = (
                dispatching
                .bound(identity)
                .bind(lambda pair: pair[1].read(properties))
                .bind(lambda record: paired(dispatcher.planned(context, record, datetime.datetime.now(datetime.UTC)), dispatcher.settings(identity)))
            )
            lines = planned.map(
                lambda pair: (pair[1].target.title, f"{len(pair[0].scopes)} jobs, {sum(len(ranged(bpy.data.scenes[scope.scene]).numbers) for scope in pair[0].scopes)} frames")
            ).default_with(lambda faults: tuple(text for _, text in rendered(faults)))
            for line in lines:
                layout.label(text=line)
            for field in Dispatch.__struct_fields__:
                layout.prop(properties, field)

        def logged(call: Call[Empty]) -> BlenderResult[Done]:
            def opened_log(plan: Path) -> BlenderResult[Done]:
                sent = dispatcher.state.observed.bind(lambda held: held.sent).filter(lambda record: isinstance(record.target, LocalMachine))
                log = sent.map(lambda record: dispatcher.host.path(record.queue, RunFile.LOG)).default_with(lambda: PlanFile.LOG.beside(plan))
                return called(bpy.ops.wm.path_open, {"filepath": str(log)}).map(lambda _: Done())

            manager: BlenderResult[bpy.types.WindowManager] = Option.of_optional(call.context.window_manager).to_result_with(
                lambda: (BlenderAbsentFault(owner=repr(call.context), key="window_manager"),)
            )
            return (
                Result[bpy.types.WindowManager, BlenderFaults]
                .bind(manager, dispatcher.queued.read)
                .bind(lambda queued: Option.of_optional(queued.plan).to_result_with(lambda: (BlenderAbsentFault(owner=QueueRun.record.key, key="plan"),)))
                .bind(lambda plan: opened_log(Path(plan)))
            )

        def shutdown_cancelled(call: Call[Empty]) -> BlenderResult[Done]:
            def cancelling(progress: Progress, power: PowerHold) -> BlenderResult[Done]:
                return (
                    cancelled(progress).map_error(lambda faults: (BlenderRenderingFault(faults=faults),)).map(lambda _: Done(notes=(("INFO", "Shutdown cancelled"),)))
                    if power.shutdown
                    else Error((BlenderAbsentFault(owner=repr(power), key="shutdown"),))
                )

            return paired(dispatcher.kept(), dispatcher.settings(call.identity)).bind(lambda pair: cancelling(pair[0][2], pair[1].power))

        def packing(call: Call[Empty]) -> BlenderResult[Done]:
            moment = datetime.datetime.now(datetime.UTC).astimezone()

            def done(pack: Path, missing: tuple[BlenderAbsentFault, ...]) -> Done:
                skipped = accumulated(Error((found,)) for found in missing).swap().to_option()
                return Done(notes=(("INFO", str(pack)),), skipped=skipped)

            return (
                call
                .root(bpy.types.Scene)
                .bind(lambda scene: dispatcher.pathed.path(scene, moment.date()))
                .bind(lambda path: packed(path.base / "packs" / f"{Path(bpy.data.filepath).stem}_{moment:%Y%m%d-%H%M%S}", compress=Some(True)))
                .bind(
                    lambda pack: resolved(item.owner for item in pack.missing).map(
                        lambda owners: done(pack.blend, tuple(BlenderAbsentFault(owner=repr(owner), key=str(item.path)) for item, owner in zip(pack.missing, owners)))
                    )
                )
            )

        def flamenco(queueing: Queueing) -> BlenderResult[Flamenco]:
            match queueing.target:
                case Flamenco() as target:
                    return Ok(target)
                case other:
                    return Error((BlenderChoiceFault(path=repr(queueing), identifier=other.title, accepted=(Flamenco.title,)),))

        def read(call: Call[Empty]) -> BlenderResult[Done]:
            def held(manager: Manager) -> Done:
                dispatcher.state.manager = Some(manager)
                return Done(notes=(("INFO", f"{len(manager.tags)} worker tags, job type {'installed' if manager.job_type.is_some() else 'missing'}"),))

            return (
                dispatcher
                .settings(call.identity)
                .bind(flamenco)
                .bind(lambda target: Dispatcher.connected().bind(lambda _: exchange.opened(target.server, queried).map_error(lambda faults: (BlenderRenderingFault(faults=faults),))))
                .map(held)
            )

        def installed(call: Call[Empty]) -> BlenderResult[Done]:
            def copied(folder: str) -> BlenderResult[Done]:
                def written() -> Path:
                    with importlib.resources.as_file(importlib.resources.files(Flamenco.__module__).joinpath(Flamenco.script)) as script:
                        return Path(shutil.copy(script, Path(folder) / Flamenco.script))

                return catch(written, exception=OSError)().map_error(lambda error: (BlenderWriteFault(request=str(folder), error=error),)).map(lambda path: Done(notes=(("INFO", str(path)),)))

            return dispatcher.settings(call.identity).bind(
                lambda queueing: copied(queueing.scripts.value) if queueing.scripts.enabled else Error((BlenderAbsentFault(owner=repr(call.context.preferences), key="scripts"),))
            )

        def tested(call: Call[Empty]) -> BlenderResult[Done]:
            return dispatcher.settings(call.identity).bind(lambda queueing: posted(queueing.listener, Test())).map(lambda _: Done(notes=(("INFO", "Notice posted"),)))

        dispatching = Operator[Dispatch](
            key=QueueRun.dispatch,
            title="Render Queue",
            description="Plan the reach's jobs, save their job files in the background, and send the queue to its target",
            icon=Some("RENDER_ANIMATION"),
            condition=Some(dispatcher.condition),
            form=Dispatch,
            run=dispatcher.dispatched,
            undo=False,
            invoke=Some(Properties()),
            draw=Some(drawn),
        )
        return (
            dispatching,
            Operator(
                key=QueueRun.resume,
                title="Resume Queue",
                description="Render a stopped, failed, or cancelled queue's remaining frames",
                icon=Some("PLAY"),
                undo=False,
                form=Empty,
                run=dispatcher.resumed,
            ),
            Operator(key=QueueRun.stop, title="Stop Queue", description="End the running queue and its renders", icon=Some("CANCEL"), undo=False, form=Empty, run=dispatcher.stopped),
            Operator(key=QueueRun.log, title="Open Queue Log", description="Open the log of the file's last dispatch", icon=Some("TEXT"), undo=False, form=Empty, run=logged),
            Operator(
                key=QueueRun.shutdown_cancel, title="Cancel Shutdown", description="Cancel the shutdown a finished queue scheduled", icon=Some("QUIT"), undo=False, form=Empty, run=shutdown_cancelled
            ),
            Operator(
                key=QueueRun.pack,
                title="Pack Project",
                description="Write the file with every external file it uses under the render folder",
                icon=Some("PACKAGE"),
                undo=False,
                form=Empty,
                run=packing,
            ),
            Operator(
                key=QueueRun.flamenco_read,
                title="Read Manager",
                description="Read the Flamenco Manager's storage, worker tags, and job types",
                icon=Some("FILE_REFRESH"),
                undo=False,
                form=Empty,
                run=read,
            ),
            Operator(
                key=QueueRun.flamenco_install,
                title="Install Job Type",
                description="Copy the render job type into the Flamenco Manager's scripts folder",
                icon=Some("IMPORT"),
                undo=False,
                form=Empty,
                run=installed,
            ),
            Operator(key=QueueRun.notice_test, title="Test Notice", description="Post a notice to the listener's topic", icon=Some("INFO"), undo=False, form=Empty, run=tested),
        )

    # --- [SECTION]

    def realized(self, identity: Identity) -> BlenderResult[Drawn]:
        """Builds the queue controls, cached progress, and native report banner."""
        return self.bound(identity).bind(
            lambda dispatcher: Single(
                key=self.key,
                title=self.title,
                rows=(
                    *(rows.OperatorButton(target=Acquired(key=key)) for key in (self.dispatch, self.flamenco_read, self.flamenco_install, self.notice_test)),
                    rows.Progress(runs=dispatcher.runs),
                    Status(),
                    *(rows.OperatorButton(target=Acquired(key=key)) for key in (self.resume, self.stop, self.log, self.shutdown_cancel, self.pack)),
                ),
            ).realized(identity)
        )

    def drawn(self, identity: Identity) -> BlenderResult[rows.RowDraw]:
        """Realizes this queue section as a native layout panel."""
        return self.realized(identity).map(lambda drawn: drawn.inline)

    # --- [REGISTRATION]

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[BlenderResult[Acquisitions]]:
        """Holds restoration admission while retaining the native queue declarations."""
        with ExitStack() as scope:

            def joined(dispatcher: Dispatcher) -> BlenderResult[Acquisitions]:
                restore = scope.enter_context(identity.reports.deferred(dispatcher.restored))
                scope.callback(identity.reports.reported(dispatcher.restored))
                return Ok((self.record, self.denoise, self.inputs, *self.commands(dispatcher), *self.operators(dispatcher), Timer(body=lambda: dispatcher.ticked(restore))))

            yield self.bound(identity).bind(joined)


# --- [OPERATIONS] -----------------------------------------------------------------------


def ranged(scene: bpy.types.Scene) -> Frames:
    """Returns the native scene's inclusive frame range and frame step."""
    return Frames(start=scene.frame_start, end=scene.frame_end, step=scene.frame_step)


def scoped(dispatcher: Dispatcher, scope: JobScope, plan: Plan, source: Path, plan_file: Path, part: int) -> BlenderResult[Job]:
    """Writes one scope's packed job file from the saved source and answers its job."""
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

        return catch(read, exception=OSError)().map_error(lambda error: (BlenderReadFault(request=str(path), error=error),))

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
    """Awaits a gated listener post after native online-access admission."""
    return (Dispatcher.connected() if listener.topic.enabled else Ok(None)).bind(lambda _: published(listener, notice).map_error(lambda faults: (BlenderRenderingFault(faults=faults),)))


def peaked(path: Path) -> BlenderResult[Option[int]]:
    """Reads the native EXR memory stamp and closes the image input."""

    def read(image: OpenImageIO.ImageInput) -> Option[int]:
        with closing(image) as held:
            return Option.of_optional(held.spec().getattribute("Memory")).map(lambda text: int(text.removesuffix("M")) * 2**20)

    return opened(str(path)).map_error(lambda faults: (BlenderImagingFault(faults=faults),)).map(read)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Denoising", "Dispatch", "Dispatcher", "Observed", "Plan", "PlanFile", "Preparing", "QueueRun", "QueueState", "Queued", "Queueing", "Reach", "Sent", "peaked", "posted", "ranged", "scoped"]
