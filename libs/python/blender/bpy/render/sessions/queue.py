"""Render queue dispatch and run progress."""

from collections.abc import Callable, Generator, Iterable
from contextlib import contextmanager, ExitStack, nullcontext
import datetime
from enum import StrEnum
from fractions import Fraction
from functools import cached_property, reduce
import hashlib
import importlib.resources
from itertools import chain
import os
from pathlib import Path
import posixpath
import shutil
import time
from typing import Annotated, ClassVar, Final, TYPE_CHECKING

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.calls import called
from blender.bpy.commands.operators import Call, Done, Empty, Operator, Properties
from blender.bpy.data.ids.store import resolved
from blender.bpy.data.rna.conditions import Setting
from blender.bpy.data.rna.descriptors import Descriptor, rooted, struct_at
from blender.bpy.extension.events.timers import Timer
from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.extension.registration import Acquisitions, Registrable
from blender.bpy.extension.registries import CliCommand
from blender.bpy.extension.scoped import Scoped
from blender.bpy.faults import BlenderAbsentFault, BlenderFaults, BlenderResult, BlenderUnmetFault
from blender.bpy.files.blends import BlendFile, OpenFile, packed, saved, SaveMode, session_loaded
from blender.bpy.render.outputs.formats import Kind, kind
from blender.bpy.render.outputs.framing import Framed, FrameSize
from blender.bpy.render.outputs.paths import OutputPath, Pathed, ScenePaths
from blender.bpy.render.sessions.preflight import checked, JobScope, Preflight, ranged
from blender.bpy.render.sessions.renders import native, Render, RenderCall
from blender.bpy.render.sessions.sets import Members, RenderSet
from blender.bpy.render.view_layers.passes import pass_flags, PassRequest, PassRouting, PassSet, required
from blender.bpy.settings.attachments import Attached
from blender.bpy.settings.properties import Choices
from blender.bpy.settings.texts import TextRecord
from blender.bpy.ui.layout import rows
from blender.bpy.ui.layout.sections import Single
from blender.bpy.ui.layout.status import Status
from blender.bpy.ui.screens.areas import AreaQuery, Located
from blender.bpy.ui.screens.editors import EditorState
from blender.bpy.ui.surfaces.preferences import PageRows
from blender.imaging.files.images import opened
from blender.records.faults import accumulated, DomainFault, FileFault, paired
from blender.records.fields import Choice, Gated, Side
from blender.records.identity import content_hash
from blender.records.stores import RecordFile
from blender.rendering.batch.estimate import Forecast
from blender.rendering.batch.flamenco import Flamenco, Manager
from blender.rendering.batch.notices import Cancelled, Completed, Estimated, Exhausted, Failed, Listener, Notice, published, Started, Test
from blender.rendering.batch.power import cancelled, PowerHold, shut
from blender.rendering.batch.processes import detached, Host, RunCommand, RunFile
from blender.rendering.batch.progress import Ending, Heartbeat, Liveness, observed, Progress, StageMark
from blender.rendering.batch.queue import Bound, Fixed, Frames, Job, Marker, RenderFrames, RenderQueue
from blender.rendering.batch.supervision import cleared, LocalMachine, Run, stop, supervise
from blender.rendering.batch.targets import send, Target
from blender.rendering.faults import RenderingFaults, RenderingResult, RenderingRunFault
from blender.rendering.outputs.naming import Output

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

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


# --- [CONSTANTS] ------------------------------------------------------------------------

ONLINE: Final = Setting(descriptor=Descriptor.declared(bpy.types.Preferences, "system.use_online_access", bpy.types.BoolProperty))
PAUSABLE: Final = Setting(descriptor=Descriptor.declared(bpy.types.SpaceView3D, "region_3d.support_pause_render", bpy.types.BoolProperty))

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


class Followup(msgspec.Struct, frozen=True, kw_only=True):
    """Job reading a render scope's frames, planned after each scope whose scene it is wanted for, keyed on its scope.

    The render it reads takes `passes` on every used view layer and `settings` on the scene, refusing cameras whose projection is in
    `refused`. `staged` writes what the job reads from the rendered frames before the job's output moves to the next version, and
    `reloaded` restores it in the job's file, under its output root, before its frames render.
    """

    key: Key
    wanted: Callable[[Identity, bpy.types.Scene], BlenderResult[bool]]
    passes: Callable[[str], PassRequest]
    staged: Callable[[Identity, bpy.types.Scene, OutputPath, Frames], BlenderResult[None]]
    reloaded: Callable[[Identity, bpy.types.Scene, Path], BlenderResult[None]]
    settings: tuple[Setting, ...] = ()
    refused: frozenset[str] = frozenset()
    prerequisites: tuple[Registrable, ...] = ()


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


class QueueCommands(msgspec.Struct, frozen=True, kw_only=True):
    """Queue operators: dispatch, run control, packing, and Manager setup."""

    dispatch: Operator[Dispatch]
    resume: Operator[Empty]
    stop: Operator[Empty]
    log: Operator[Empty]
    shutdown_cancel: Operator[Empty]
    pack: Operator[Empty]
    flamenco_read: Operator[Empty]
    flamenco_install: Operator[Empty]
    notice_test: Operator[Empty]

    @property
    def runtime(self) -> tuple[Operator[Empty], ...]:
        """Run control and packing commands in section order."""
        return self.resume, self.stop, self.log, self.shutdown_cancel, self.pack

    @property
    def setup(self) -> tuple[Operator[Empty], ...]:
        """Manager reads, job type install, and notice test in preferences order."""
        return self.flamenco_read, self.flamenco_install, self.notice_test


# --- [SERVICES] -------------------------------------------------------------------------


class QueueState(msgspec.Struct, kw_only=True):
    """Session state of one extension's queue: Manager facts, last observation, and paused viewports."""

    manager: Option[Manager] = Nothing
    observed: Option[Observed] = Nothing
    paused: tuple[tuple[Located, EditorState], ...] = ()


QUEUES: Final = Scoped[QueueState](made=lambda _identity: nullcontext(QueueState()))
TAGS: Final = Choices(
    lambda identity, _owner, _context: tuple(
        (identifier, tag.name, Option.of_optional(tag.description).default_value(""))
        for manager in QUEUES.of(identity).manager.to_list()
        for tag in manager.tags
        for identifier in Option.of_optional(tag.id).to_list()
    )
)


class Dispatcher(msgspec.Struct, frozen=True):
    """Stores, routing, render, follow-ups, and settings queue operations read, with the extension's session state."""

    identity: Identity
    host: Host
    members: Members
    framed: Framed
    routing: PassRouting
    pathed: Pathed
    queued: Attached[Queued]
    render: Render
    follow: tuple[Followup, ...]
    settings: Callable[[Identity], BlenderResult[Queueing]]

    @property
    def state(self) -> QueueState:
        """Session state the extension holds."""
        return QUEUES.of(self.identity)

    def followup(self, key: Key) -> BlenderResult[Followup]:
        """Returns the follow-up a planned scope names."""
        return Option.of_optional(next((followup for followup in self.follow if followup.key == key), None)).to_result_with(lambda: (BlenderAbsentFault(owner=repr(self.follow), key=key),))

    def refused(self, scope: JobScope) -> frozenset[str]:
        """Camera projections the scope's follow-up refuses, none for a render scope."""
        return frozenset(projection for followup in self.follow if followup.key == scope.follow for projection in followup.refused)

    # --- [ACCESS]

    @staticmethod
    def connected() -> BlenderResult[None]:
        """Requires `bpy.app.online_access`, set by preferences and `--offline-mode`."""
        return Ok(None) if bpy.app.online_access else Error((BlenderUnmetFault(requirement=ONLINE),))

    def posted(self, listener: Listener, notice: Notice) -> BlenderResult[None]:
        """Posts notice, requiring online access for an enabled topic."""
        return (Dispatcher.connected() if listener.topic.enabled else Ok(None)).bind(lambda _: published(listener, notice))

    # --- [PLANNING]

    def planned(self, context: bpy.types.Context, dispatch: Dispatch, moment: datetime.datetime) -> BlenderResult[Plan]:
        """Plans reach scopes, each followed by the follow-up jobs its scene wants."""

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

        def followed(scope: JobScope) -> BlenderResult[tuple[JobScope, ...]]:
            scene = bpy.data.scenes[scope.scene]
            return accumulated(followup.wanted(self.identity, scene).map(lambda wanted: (msgspec.structs.replace(scope, follow=followup.key),) if wanted else ()) for followup in self.follow).map(
                lambda groups: (scope, *chain.from_iterable(groups))
            )

        return (
            rooted(bpy.types.Scene, context)
            .bind(reached)
            .bind(lambda scopes: accumulated(map(followed, scopes)))
            .map(lambda groups: tuple(chain.from_iterable(groups)))
            .bind(lambda scopes: Ok(Plan(moment=moment, scopes=scopes)) if scopes else Error((BlenderAbsentFault(owner=repr(context), key=dispatch.reach.name),)))
        )

    # --- [DISPATCH]

    def dispatched(self, call: Call[Dispatch]) -> BlenderResult[Done]:
        """Checks and stores plan, saves file, then starts `prepare` and pauses viewport renders."""
        context, identity, day = call.context, call.identity, datetime.datetime.now(datetime.UTC).astimezone().date()

        def gated(plan: Plan, queueing: Queueing) -> BlenderResult[None]:
            return paired(checked(self.pathed, plan.scopes, day, self.refused), Dispatcher.connected() if isinstance(queueing.target, Flamenco) else Ok(None)).map(lambda _: None)

        def paused(manager: bpy.types.WindowManager) -> BlenderResult[None]:
            def applied(state: EditorState) -> BlenderResult[None]:
                def kept(at: Located, prior: Option[EditorState]) -> None:
                    self.state.paused = (*self.state.paused, *((at, held) for held in prior.to_list()))

                return accumulated(state.applied(at.area).map(lambda prior: kept(at, prior)) for at in AreaQuery(space="VIEW_3D", shows=Some(PAUSABLE)).located(manager.windows)).map(lambda _: None)

            return EditorState.of(bpy.types.SpaceView3D, {"region_3d.pause_render": True}).bind(applied)

        def written(plan: Plan, queueing: Queueing, manager: bpy.types.WindowManager) -> BlenderResult[Done]:
            path = identity.folder / f"{content_hash(Plan, plan).hex()}{PlanFile.PLAN}"
            scenes = tuple(dict.fromkeys(bpy.data.scenes[scope.scene] for scope in plan.scopes))
            return (
                accumulated(self.pathed.took(scene, day) for scene in scenes)
                .bind(lambda _: RecordFile(path=path, form=Plan).written(plan))
                .bind(lambda _: self.queued.write(manager, Queued(plan=str(path))))
                .bind(lambda _: saved(Path(bpy.data.filepath), SaveMode.ACTIVE))
                .bind(lambda source: detached(self.host.argv(RunCommand.PREPARE, Preparing(plan=str(path), target=queueing.target), Some(str(source))), PlanFile.LOG.beside(path)))
                .map(lambda log: Done(notes=(("INFO", f"{len(plan.scopes)} jobs preparing, log at {log}"),), skipped=paused(manager).swap().to_option()))
            )

        return (
            paired(self.planned(context, call.record, datetime.datetime.now(datetime.UTC)), self.settings(identity), rooted(bpy.types.WindowManager, context))
            .bind(lambda parts: gated(parts[0], parts[1]).map(lambda _: parts))
            .bind(lambda parts: written(*parts))
        )

    def prepared(self, arguments: Preparing, trailing: tuple[str, ...]) -> BlenderResult[Sent]:
        """Writes job files of stored plan, sends queue to target, and stores `Sent` record."""
        if trailing:
            return Error((DomainFault(subject=RunCommand.PREPARE, value=repr(trailing), accepted="no trailing arguments"),))
        path = Path(arguments.plan)
        source = Path(bpy.data.filepath)

        def sent(jobs: tuple[Job, ...]) -> BlenderResult[Sent]:
            queue = RenderQueue(jobs=jobs, blender=bpy.app.version_string, extension=self.identity.manifest.version)
            match send(arguments.target, queue, self.host):
                case Result(tag="error", error=faults):
                    return Error(faults)
                case Result(ok=results):
                    record = Sent(queue=queue, target=arguments.target, issued=tuple(value for result in results for value in result.to_option().to_list()))
                    for result in results:
                        match result:
                            case Result(tag="ok", ok=issued):
                                self.identity.logger.info("Queue sent", issued=issued)
                            case Result(error=refused):
                                self.identity.reports.hold(refused)
                    return RecordFile(path=PlanFile.SENT.beside(path), form=Sent).written(record).map(lambda _: record)

        def jobs(plan: Plan) -> BlenderResult[tuple[Job, ...]]:
            return accumulated(
                self.job(scope, plan, source, path, part).map(
                    lambda job: msgspec.structs.replace(
                        job, prerequisites=(next(index for index, before in enumerate(plan.scopes[:part]) if before == scope.base),) if scope.follow is not None else ()
                    )
                )
                for part, scope in enumerate(plan.scopes)
            )

        loaded: BlenderResult[Plan] = RecordFile(path=path, form=Plan).read()
        return Result[Plan, BlenderFaults].bind(loaded, jobs).bind(sent)

    def routed(self, scene: bpy.types.Scene, day: datetime.date) -> BlenderResult[OutputPath]:
        """Composes the job copy's output path, writes it to the scene, and reconciles pass routing to it."""
        return (
            self.pathed
            .path(scene, day)
            .map(lambda path: path.copied)
            .bind(lambda path: path.scene_output(scene).bind(lambda text: Pathed.filepath.write(scene.render, text)).map(lambda _: path))
            .bind(lambda path: self.routing.reconciled(scene, bpy.context, path).map(lambda _: path))
        )

    def job(self, scope: JobScope, plan: Plan, source: Path, plan_file: Path, part: int) -> BlenderResult[Job]:
        """Writes scope's packed job file from saved source."""
        day = plan.moment.astimezone().date()
        render = scope.base
        following = tuple(followup for other in plan.scopes if other.follow is not None and other.base == render for followup in self.follow if followup.key == other.follow)

        def needed(scene: bpy.types.Scene) -> BlenderResult[None]:
            def held(setting: Setting) -> BlenderResult[None]:
                return (
                    struct_at(scene, setting.descriptor.owner)
                    .map(lambda owner: setting.descriptor.write(owner, setting.accepted[0]).map(lambda _: None))
                    .default_with(lambda: Error((BlenderAbsentFault(owner=repr(scene), key=setting.descriptor.path),)))
                )

            requests = tuple(followup.passes(layer.name) for layer in self.routing.used(scene) for followup in following)
            return paired(
                required(scene, bpy.context, requests, self.routing.precision), accumulated(map(held, dict.fromkeys(setting for followup in following for setting in followup.settings)))
            ).map(lambda _: None)

        def applied(scene: bpy.types.Scene) -> BlenderResult[tuple[OutputPath, tuple[Side, Side]]]:
            if following and scope.follow is None:
                scene.frame_step = 1
            if isinstance(render.camera, Fixed):
                scene.camera = bpy.data.objects[render.camera.camera]
                for marker in scene.timeline_markers:
                    marker.camera = None
            scene.render.use_stamp_memory = True
            layers = (
                Option
                .of_optional(render.layer)
                .map(lambda name: accumulated(self.routing.use.write(layer, layer.name == name) for layer in scene.view_layers).map(lambda _: None))
                .default_value(Ok(None))
            )
            return paired(layers, self.framed.grown(scene), needed(scene)).bind(lambda parts: self.routed(scene, day).map(lambda path: (path, parts[1][1])))

        def staged(scene: bpy.types.Scene, followup: Followup, copied: OutputPath) -> BlenderResult[OutputPath]:
            return (
                followup
                .staged(self.identity, scene, copied, ranged(scene))
                .bind(lambda _: self.pathed.attached.read(scene))
                .bind(lambda outputs: self.pathed.attached.write(scene, msgspec.structs.replace(outputs, version=outputs.version + 1)))
                .bind(lambda _: self.routed(scene, day))
            )

        def digested(path: Path) -> BlenderResult[bytes]:
            def read() -> bytes:
                with path.open("rb") as file:
                    return hashlib.file_digest(file, "sha256").digest()

            return catch(read, exception=OSError)().map_error(lambda error: (FileFault(access="read", request=path, error=error),))

        def written(scene: bpy.types.Scene, path: OutputPath, pixels: tuple[Side, Side]) -> BlenderResult[Job]:
            used = self.routing.used(scene)
            camera = (scope.camera.camera,) if isinstance(scope.camera, Fixed) else ()
            name = " ".join(bpy.path.clean_name(part) for part in (scope.scene, *Option.of_optional(scope.layer).to_list(), *camera, *Option.of_optional(scope.follow).to_list()))
            file = path.base / "snapshots" / plan_file.name.removesuffix(PlanFile.PLAN) / f"{part}_{name}.blend"
            layer = Option.of_optional(scope.layer).default_with(lambda: scene.view_layers[0].name)
            outputs = accumulated(path.text(layer, output) for output in Output).map(lambda texts: dict(zip(Output, (posixpath.join(path.root, text) for text in texts))))

            def job(texts: dict[Output, str], digest: bytes, samples: int) -> Job:
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
                self.pathed.scope
                .write(scene, path)
                .bind(lambda _: called(bpy.ops.file.make_paths_absolute, {}))
                .bind(lambda _: paired(called(bpy.ops.file.pack_all, {}), called(bpy.ops.file.pack_libraries, {})))
                .bind(lambda _: saved(file, SaveMode.COPY, compress=Some(True)))
                .bind(lambda saved_file: paired(outputs, digested(saved_file), accumulated(self.members.samples_of(scene, held) for held in used).map(sum)))
                .map(lambda parts: job(*parts))
            )

        def prepared(_: BlendFile) -> BlenderResult[Job]:
            scene = bpy.data.scenes[scope.scene]
            return (
                applied(scene)
                .bind(
                    lambda parts: (
                        Option
                        .of_optional(scope.follow)
                        .map(lambda key: self.followup(key).bind(lambda followup: staged(scene, followup, parts[0])).map(lambda path: (path, parts[1])))
                        .default_value(Ok(parts))
                    )
                )
                .bind(lambda parts: written(scene, *parts))
            )

        return session_loaded(OpenFile(path=source)).bind(prepared)

    # --- [BACKGROUND_RENDER]

    def frames[T](self, scene: bpy.types.Scene, frames: tuple[int, ...], consumed: Callable[[int, float], BlenderResult[T]], base: Option[Path] = Nothing) -> BlenderResult[tuple[T, ...]]:
        """Renders and consumes each frame after every follow-up reloads, output rerooted under `base` when set."""

        def frame(number: int) -> BlenderResult[T]:
            started = time.monotonic()
            scene.frame_start = scene.frame_end = number
            return self.render(self.identity, scene, RenderCall(animation=True)).bind(lambda _: consumed(number, time.monotonic() - started))

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
            return rerooted.bind(lambda _: accumulated(followup.reloaded(self.identity, scene, path.base) for followup in self.follow)).map(lambda _: None)

        return (
            self.pathed.scope
            .read(scene)
            .bind(lambda found: found.to_result_with(lambda: (BlenderAbsentFault(owner=repr(scene), key=self.pathed.scope.name),)))
            .bind(prepared)
            .bind(lambda _: reduce(lambda done, number: done.bind(lambda values: frame(number).map(lambda value: (*values, value))), frames, Result[tuple[T, ...], BlenderFaults](ok=())))
        )

    def commanded(self, arguments: Run | RenderFrames, trailing: tuple[str, ...]) -> BlenderResult[object]:
        """Runs local queue progress or farm worker frame request."""
        match arguments:
            case Run():
                return Error((DomainFault(subject=RunCommand.RENDER, value=repr(trailing), accepted="no trailing arguments"),)) if trailing else self.rendered(arguments)
            case RenderFrames(scene=name, frames=frames):
                match trailing:
                    case (output_root,):
                        scene: BlenderResult[bpy.types.Scene] = Option.of_optional(bpy.data.scenes.get(name)).to_result_with(lambda: (BlenderAbsentFault(owner=repr(bpy.data.scenes), key=name),))
                        return (
                            Result[bpy.types.Scene, BlenderFaults].bind(scene, lambda held: self.frames(held, frames, lambda _frame, _seconds: Ok(None), Some(Path(output_root)))).map(lambda _: None)
                        )
                    case _:
                        return Error((DomainFault(subject=RunCommand.RENDER, value=repr(trailing), accepted="<output root>"),))

    def supervised(self, run: Run, trailing: tuple[str, ...]) -> BlenderResult[Progress]:
        """Supervises local render processes under power hold, posting notices."""
        if trailing:
            return Error((DomainFault(subject=RunCommand.SUPERVISE, value=repr(trailing), accepted="no trailing arguments"),))

        def ran(queue: RenderQueue, queueing: Queueing) -> BlenderResult[Progress]:
            def completed(progress: Progress) -> BlenderResult[Progress]:
                self.identity.reports.reported(self.posted)(
                    queueing.listener, Completed(frames=len(progress.marks), seconds=sum((Fraction(mark.seconds) for mark in progress.marks), Fraction()))
                ).to_optional()
                return (shut() if queueing.power.shutdown else Ok(None)).map(lambda _: progress)

            def failed(faults: RenderingFaults) -> BlenderResult[Progress]:
                notice: Notice
                match faults:
                    case (RenderingRunFault(error=int(code)), *_) if code != 1:
                        notice = Exhausted(job=queue.stem, attempts=run.target.retries + 1)
                    case _:
                        notice = Failed(job=queue.stem, reason=" ".join(fault.message for fault in faults))
                self.identity.reports.reported(self.posted)(queueing.listener, notice).to_optional()
                return Error(faults)

            match observed(self.host.path(queue, RunFile.PROGRESS)):
                case Result(tag="error", error=faults):
                    return Error(faults)
                case Result(ok=observations):
                    if not Progress.resumed(observations, queue, os.getpid()).marks:
                        self.identity.reports.reported(self.posted)(queueing.listener, Started(jobs=len(queue.jobs), frames=len(queue.steps))).to_optional()
                    return queueing.power.held(lambda: supervise(run.target, queue, self.host, queueing.heartbeat)).map(completed).default_with(failed)

        return paired(RecordFile(path=Path(run.queue), form=RenderQueue).read(), self.settings(self.identity)).bind(lambda pair: ran(*pair))

    def rendered(self, run: Run) -> BlenderResult[tuple[StageMark, ...]]:
        """Renders pending frames of first remaining job, storing each frame mark."""

        def ran(queue: RenderQueue, queueing: Queueing) -> BlenderResult[tuple[StageMark, ...]]:
            records = RecordFile(path=self.host.path(queue, RunFile.PROGRESS), form=Progress)
            holding = frozenset(part for part, job in enumerate(queue.jobs) if job.frames.numbers)

            def marked(part: int, frame: int, seconds: float) -> BlenderResult[StageMark]:
                scene = bpy.data.scenes[queue.jobs[part].scene]
                peak = peaked(Path(bpy.path.abspath(scene.render.frame_path(frame=frame)))) if kind(scene.render.image_settings) is not Kind.IMAGE else Ok(Nothing)
                return peak.map(lambda value: StageMark(part=part, item=frame, seconds=seconds, peak=value.to_optional()))

            def finished(progress: Progress, mark: StageMark) -> BlenderResult[StageMark]:
                before = frozenset(held.part for held in progress.marks[:-1])
                first = holding <= before | {mark.part} and not holding <= before
                for forecast in Forecast.queued(progress, queue, datetime.datetime.now(datetime.UTC)).filter(lambda _: first).to_list():
                    self.identity.reports.reported(self.posted)(queueing.listener, Estimated(forecast=forecast)).to_optional()
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
                persisted: BlenderResult[Progress] = records.revised(lambda held: msgspec.structs.replace(held, marks=(*held.marks, mark)))
                return Result[Progress, BlenderFaults].bind(persisted, lambda held: finished(held, mark))

            def runs(progress: Progress) -> BlenderResult[tuple[StageMark, ...]]:
                remaining = frozenset(progress.remaining(queue))
                steps = tuple(step for step in progress.pending if step in remaining)
                return (
                    self.frames(bpy.data.scenes[queue.jobs[steps[0][0]].scene], tuple(frame for _, frame in steps), lambda frame, seconds: marked(steps[0][0], frame, seconds).bind(appended))
                    if steps
                    else Ok(())
                )

            loaded: BlenderResult[Progress] = records.read()
            return Result[Progress, BlenderFaults].bind(loaded, runs)

        return paired(RecordFile(path=Path(run.queue), form=RenderQueue).read(), self.settings(self.identity)).bind(lambda pair: ran(*pair))

    # --- [SESSION_PROGRESS]

    def observed(self, context: bpy.types.Context) -> BlenderResult[Option[Observed]]:
        """Reads sent record and local progress, `Nothing` sent before `prepare` writes it."""

        def read(plan: Path) -> RenderingResult[Observed]:
            def pending(faults: RenderingFaults) -> RenderingResult[Option[Sent]]:
                match faults:
                    case (FileFault(access="read", error=FileNotFoundError()),):
                        return Ok(Nothing)
                    case _:
                        return Error(faults)

            return (
                Result[Sent, RenderingFaults]
                .map(RecordFile(path=PlanFile.SENT.beside(plan), form=Sent).read(), Some)
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

        return (
            rooted(bpy.types.WindowManager, context).bind(self.queued.read).bind(lambda queued: Option.of_optional(queued.plan).map(lambda plan: read(Path(plan)).map(Some)).default_value(Ok(Nothing)))
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
                .map(lambda queue: cleared(queue, self.host))
                .default_value(Ok(None))
            )

        self.identity.reports.reported(lambda: self.observed(bpy.context).bind(acted))().to_optional()
        return Ok(Some(1.0))

    # --- [RUN_CONTROL]

    def resumed(self, call: Call[Empty]) -> BlenderResult[Done]:
        """Resends local queue only when stopped, failed, or canceled."""

        def resent(queue: RenderQueue, target: LocalMachine, progress: Progress, beat: datetime.datetime, queueing: Queueing) -> BlenderResult[Done]:
            phase = progress.phase(beat, datetime.datetime.now(datetime.UTC), queueing.heartbeat)
            resumable = Liveness.STOPPED, Ending.FAILED, Ending.CANCELLED
            return (
                send(target, queue, self.host).map(
                    lambda issued: Done(notes=tuple(("INFO", value) for answer in issued for value in answer.to_option().to_list()), skipped=accumulated(issued).swap().to_option())
                )
                if phase in resumable
                else Error((DomainFault(subject=str(self.host.path(queue, RunFile.PROGRESS)), value=phase.name, accepted=", ".join(value.name for value in resumable)),))
            )

        return paired(self.kept(), self.settings(call.identity)).bind(lambda pair: resent(*pair[0], pair[1]))

    def stopped(self, call: Call[Empty]) -> BlenderResult[Done]:
        """Stops process group, stores cancellation, and posts cancel notice."""

        def halted(queue: RenderQueue, progress: Progress, queueing: Queueing) -> BlenderResult[Done]:
            def ended(_: None) -> BlenderResult[Done]:
                persisted = RecordFile(path=self.host.path(queue, RunFile.PROGRESS), form=Progress).revised(lambda held: msgspec.structs.replace(held, ending=Ending.CANCELLED))
                return paired(persisted, self.posted(queueing.listener, Cancelled(frames=len(progress.marks)))).map(lambda _: Done(notes=(("INFO", f"Stopped {queue.stem}"),)))

            stopped: BlenderResult[None] = stop(progress)
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
            return cancelled(progress).map(lambda _: Done(notes=(("INFO", "Shutdown canceled"),))) if power.shutdown else Error((BlenderAbsentFault(owner=repr(power), key="shutdown"),))

        return paired(self.kept(), self.settings(call.identity)).bind(lambda pair: cancelling(pair[0][2], pair[1].power))

    def packaged(self, call: Call[Empty]) -> BlenderResult[Done]:
        """Writes packed file copy under render output folder."""
        moment = datetime.datetime.now(datetime.UTC).astimezone()

        def done(pack: Path, missing: tuple[BlenderAbsentFault, ...]) -> Done:
            return Done(notes=(("INFO", str(pack)),), skipped=accumulated(Error((found,)) for found in missing).swap().to_option())

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
                    return Error((DomainFault(subject=repr(queueing), value=other.title, accepted=Flamenco.title),))

        def held(manager: Manager) -> Done:
            self.state.manager = Some(manager)
            return Done(notes=(("INFO", f"{len(manager.tags)} worker tags, job type {'installed' if manager.job_type.is_some() else 'missing'}"),))

        return self.settings(call.identity).bind(flamenco).bind(lambda target: Dispatcher.connected().bind(lambda _: target.server.opened(Manager.queried))).map(held)

    def installed(self, call: Call[Empty]) -> BlenderResult[Done]:
        """Copies job type script into Manager scripts folder."""

        def copied(folder: str) -> BlenderResult[Done]:
            def written() -> Path:
                with importlib.resources.as_file(importlib.resources.files(Flamenco.__module__).joinpath(Flamenco.script)) as script:
                    return Path(shutil.copy(script, Path(folder) / Flamenco.script))

            return catch(written, exception=OSError)().map_error(lambda error: (FileFault(access="write", request=folder, error=error),)).map(lambda path: Done(notes=(("INFO", str(path)),)))

        return self.settings(call.identity).bind(
            lambda queueing: copied(queueing.scripts.value) if queueing.scripts.enabled else Error((BlenderAbsentFault(owner=repr(call.context.preferences), key="scripts"),))
        )

    def tested(self, call: Call[Empty]) -> BlenderResult[Done]:
        """Posts test notice to listener topic."""
        return self.settings(call.identity).bind(lambda queueing: self.posted(queueing.listener, Test())).map(lambda _: Done(notes=(("INFO", "Notice posted"),)))


class QueueRun[C: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Render queue commands, status timer, section with its progress row, and preferences section, each frame running through `render`."""

    sets: RenderSet[C]
    settings: Callable[[Identity], BlenderResult[Queueing]]
    render: Render = native
    follow: tuple[Followup, ...] = ()
    record: ClassVar[TextRecord[Queued]] = TextRecord(key="queue", form=Queued)
    key: ClassVar[Key] = "queue"
    title: ClassVar[str] = "Render Queue"

    # --- [COMMANDS]

    @cached_property
    def commands(self) -> QueueCommands:
        """Queue operators, each binding its dispatcher on call."""

        def run[T: msgspec.Struct](step: Callable[[Dispatcher, Call[T]], BlenderResult[Done]]) -> Callable[[Call[T]], BlenderResult[Done]]:
            return lambda call: self.bound(call.identity).bind(lambda dispatcher: step(dispatcher, call))

        def drawn(layout: bpy.types.UILayout, context: bpy.types.Context, properties: bpy.types.OperatorProperties, identity: Identity) -> None:
            lines = (
                paired(self.bound(identity), self.commands.dispatch.bound(identity).bind(lambda pair: pair[1].read(properties)))
                .bind(lambda pair: paired(pair[0].planned(context, pair[1], datetime.datetime.now(datetime.UTC)), pair[0].settings(identity)))
                .map(lambda pair: (pair[1].target.title, f"{len(pair[0].scopes)} jobs, {sum(len(ranged(bpy.data.scenes[scope.scene]).numbers) for scope in pair[0].scopes)} frames"))
                .default_with(lambda faults: tuple(fault.message for fault in faults))
            )
            for line in lines:
                layout.label(text=line)
            for field in Dispatch.__struct_fields__:
                layout.prop(properties, field)

        def control(key: Key, title: str, description: str, icon: "rna_enums.IconItems", step: Callable[[Dispatcher, Call[Empty]], BlenderResult[Done]]) -> Operator[Empty]:
            return Operator(key=key, title=title, description=description, icon=Some(icon), undo=False, form=Empty, run=run(step))

        return QueueCommands(
            dispatch=Operator[Dispatch](
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
            ),
            resume=control("resume", "Resume Queue", "Render remaining frames of stopped, failed, or canceled queue", "PLAY", Dispatcher.resumed),
            stop=control("stop", "Stop Queue", "Stop queue and its renders", "CANCEL", Dispatcher.stopped),
            log=control("log", "Open Queue Log", "Open last dispatch log", "TEXT", Dispatcher.logged),
            shutdown_cancel=control("shutdown_cancel", "Cancel Shutdown", "Cancel scheduled shutdown", "QUIT", Dispatcher.unscheduled),
            pack=control("pack", "Pack Project", "Write packed file copy under render output folder", "PACKAGE", Dispatcher.packaged),
            flamenco_read=control("flamenco_read", "Read Manager", "Read Manager storage, worker tags, and job types", "FILE_REFRESH", Dispatcher.managed),
            flamenco_install=control("flamenco_install", "Install Job Type", "Copy job type script into Manager scripts folder", "IMPORT", Dispatcher.installed),
            notice_test=control("notice_test", "Test Notice", "Post test notice to listener topic", "INFO", Dispatcher.tested),
        )

    # --- [BINDING]

    @property
    def prerequisites(self) -> tuple[Registrable, ...]:
        """Queue store and follow-up rows with render set, frame size, routing, and path bindings read."""
        return (self.record, QUEUES, *(row for followup in self.follow for row in followup.prerequisites), *self.sets.prerequisites, FrameSize.attachment, PassSet.attachment, ScenePaths.attachment)

    def bound(self, identity: Identity) -> BlenderResult[Dispatcher]:
        """Binds dispatcher to identity stores, render sets, routing, render, follow-ups, and settings."""
        host = accumulated(identity.name(Name.CLI, command) for command in RunCommand).map(
            lambda names: Host(binary=bpy.app.binary_path, folder=str(identity.folder), commands=dict(zip(RunCommand, names)))
        )
        return paired(host, self.sets.bound(identity), FrameSize().bound(identity), PassSet().bound(identity), ScenePaths().bound(identity), QueueRun.record.attachment.bound(identity)).map(
            lambda parts: Dispatcher(
                identity=identity, host=parts[0], members=parts[1], framed=parts[2], routing=parts[3], pathed=parts[4], queued=parts[5], render=self.render, follow=self.follow, settings=self.settings
            )
        )

    @staticmethod
    def cli(dispatcher: Dispatcher) -> tuple[CliCommand[Preparing], CliCommand[Run], CliCommand[Run | RenderFrames]]:
        """Returns `prepare`, `supervise`, and `render` command rows."""
        return (
            CliCommand(key=RunCommand.PREPARE, decoder=msgspec.json.Decoder(Preparing), run=dispatcher.prepared),
            CliCommand(key=RunCommand.SUPERVISE, decoder=msgspec.json.Decoder(Run), run=dispatcher.supervised),
            CliCommand(key=RunCommand.RENDER, decoder=msgspec.json.Decoder[Run | RenderFrames](Run | RenderFrames), run=dispatcher.commanded),
        )

    # --- [SECTIONS]

    def runs(self, identity: Identity, _context: bpy.types.Context) -> Iterable[rows.Work]:
        """Returns progress rows from the extension's cached observation alone."""
        now = datetime.datetime.now(datetime.UTC)

        def local(sent: Sent, progress: Progress, beat: datetime.datetime, heartbeat: Heartbeat) -> rows.Work:
            remaining, phase = progress.remaining(sent.queue), progress.phase(beat, now, heartbeat)
            finish = Forecast.queued(progress, sent.queue, now).bind(lambda forecast: forecast.finish).map(lambda moment: f"finishing {moment.astimezone():%H:%M}").to_list()
            return rows.Work(text=" · ".join((*(sent.queue.jobs[part].title for part, _ in remaining[:1]), phase.label, *finish)), done=float(progress.done(sent.queue)))

        def work(held: Observed, heartbeat: Heartbeat) -> rows.Work:
            return held.sent.map(
                lambda sent: held.progress.map(lambda pair: local(sent, *pair, heartbeat)).default_with(
                    lambda: rows.Work(text=" ".join((sent.target.title, *sent.issued)), done=0.0 if isinstance(sent.target, LocalMachine) else 1.0)
                )
            ).default_with(lambda: rows.Work(text="Preparing", done=0.0))

        return (
            self
            .settings(identity)
            .map(lambda queueing: tuple(work(held, queueing.heartbeat) for held in QUEUES.of(identity).observed.to_list()))
            .default_with(lambda faults: tuple(rows.Work(text=fault.message, done=0.0) for fault in faults))
        )

    @property
    def section(self) -> Single:
        """Queue section: dispatch, run progress read from session state at draw, status, and run controls."""
        return Single(
            key=self.key,
            title=self.title,
            rows=(
                rows.OperatorButton(target=self.commands.dispatch),
                (QUEUES, lambda identity: rows.Progress(runs=lambda context: self.runs(identity, context)).drawn(identity)),
                Status(),
                rows.Caption(rows=tuple(rows.OperatorButton(target=command, text="") for command in self.commands.runtime), flow=rows.Flow.ALIGNED, separator=Nothing),
            ),
        )

    @property
    def preferences(self) -> PageRows:
        """Preferences section of Manager reads, job type install, and notice test, with cached Manager tags as `Flamenco.tag` choices."""
        return PageRows(
            sections=(
                Single(
                    key=f"{self.key}_setup",
                    title=self.title,
                    rows=(rows.Caption(rows=tuple(rows.OperatorButton(target=command) for command in self.commands.setup), flow=rows.Flow.ALIGNED, separator=Nothing),),
                ),
            ),
            bindings={vars(Flamenco)["tag"]: TAGS},
        )

    # --- [REGISTRATION]

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[BlenderResult[Acquisitions]]:
        """Holds deferred viewport restore while registered."""
        with ExitStack() as scope:

            def joined(dispatcher: Dispatcher) -> BlenderResult[Acquisitions]:
                restore = scope.enter_context(identity.reports.deferred(dispatcher.restored))
                scope.callback(identity.reports.reported(dispatcher.restored))
                return Ok((QUEUES, *self.cli(dispatcher), self.commands.dispatch, *self.commands.runtime, *self.commands.setup, Timer(body=lambda: dispatcher.ticked(restore))))

            yield self.bound(identity).bind(joined)


# --- [OPERATIONS] -----------------------------------------------------------------------


def peaked(path: Path) -> BlenderResult[Option[int]]:
    """Reads EXR `Memory` stamp in bytes."""
    return opened(str(path), lambda image: Ok(Option.of_optional(image.spec().getattribute("Memory")).map(lambda text: int(text.removesuffix("M")) * 2**20)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "ONLINE",
    "PAUSABLE",
    "QUEUES",
    "TAGS",
    "Dispatch",
    "Dispatcher",
    "Followup",
    "Observed",
    "Plan",
    "PlanFile",
    "Preparing",
    "QueueCommands",
    "QueueRun",
    "QueueState",
    "Queued",
    "Queueing",
    "Reach",
    "Sent",
    "peaked",
]
