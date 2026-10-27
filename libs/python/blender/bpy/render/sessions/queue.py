"""Render queue dispatch and run progress."""

from collections.abc import Callable, Generator, Iterable
from contextlib import contextmanager, ExitStack, nullcontext
import datetime
from enum import auto, Enum, StrEnum
from fractions import Fraction
from functools import cached_property, partial, reduce, update_wrapper
import hashlib
import importlib.resources
import inspect
import os
from pathlib import Path
import posixpath
import shutil
import sys
import threading
import time
import typing
from typing import Annotated, ClassVar, Final, TYPE_CHECKING

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.calls import called
from blender.bpy.commands.operators import Call, Done, Empty, Operator, Properties
from blender.bpy.data.ids.store import IdName, resolved
from blender.bpy.data.rna.conditions import Setting
from blender.bpy.data.rna.descriptors import assigned, Descriptor, rna, rooted, struct_at
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.cli import CliCommand
from blender.bpy.extension.events import Timer
from blender.bpy.extension.identity import Identity, Key, Resource
from blender.bpy.extension.registration import Acquisitions, Declaration
from blender.bpy.faults import BlenderReason
from blender.bpy.files.blends import BlendFile, OpenFile, packed, saved, SaveMode, session_loaded
from blender.bpy.render.outputs.framing import Framed, FrameSize
from blender.bpy.render.outputs.paths import OutputPath, Pathed, ScenePaths
from blender.bpy.render.sessions.preflight import checked, JobScope, Preflight, ranged
from blender.bpy.render.sessions.renders import native, Render, RenderCall
from blender.bpy.render.sessions.sets import Members, RenderSet
from blender.bpy.render.view_layers.passes import pass_flags, PassRouting, PassSet
from blender.bpy.settings.attachments import Attached
from blender.bpy.settings.texts import TextRecord
from blender.bpy.ui.layout import rows
from blender.bpy.ui.layout.sections import Single
from blender.bpy.ui.layout.status import Status
from blender.bpy.ui.screens.areas import AreaQuery, Located
from blender.bpy.ui.screens.editors import EditorState
from blender.bpy.ui.surfaces.preferences import PageRows
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired, RecordReason
from blender.records.fields import Side
from blender.records.identity import content_hash
from blender.records.stores import RecordFile
from blender.rendering.batch.exchange import Endpoint
from blender.rendering.batch.flamenco import Flamenco, Manager
from blender.rendering.batch.notices import Cancelled, Completed, Estimated, Failed, Listener, Notice, published, Started, Test
from blender.rendering.batch.power import cancelled, PowerHold, shut
from blender.rendering.batch.processes import detached, Host, RunCommand, RunFile
from blender.rendering.batch.progress import Ending, Heartbeat, Liveness, observed, Progress, StageMark
from blender.rendering.batch.queue import Bound, Fixed, Job, Marker, RenderFrames, RenderQueue
from blender.rendering.batch.supervision import cleared, LocalMachine, Run, stop, supervise
from blender.rendering.batch.targets import JobFile, Renderon, send, Target
from blender.rendering.outputs.naming import Output

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Step = Callable[[Dispatcher, Call[Empty]], Outcome[Done]]


class Reach(Enum):
    """Dispatch render scope."""

    SETS = auto()
    SCENE = auto()
    LAYER = auto()
    CAMERAS = auto()


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
TARGETS: Final = tuple((kind.__name__, kind.__name__, inspect.getdoc(kind) or "") for kind in typing.get_args(Target.__value__))
PAUSABLE: Final = Setting(descriptor=Descriptor.declared(bpy.types.SpaceView3D, "region_3d.support_pause_render", bpy.types.BoolProperty))

# --- [MODELS] ---------------------------------------------------------------------------


class Dispatch(bpy.types.PropertyGroup):
    """Dispatch dialog options."""

    reach: bpy.props.EnumProperty(name="Render", description="Scope queue renders", items=tuple((member.name, bpy.path.display_name(member.name.lower()), "") for member in Reach))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


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


class Queued(bpy.types.PropertyGroup):
    """Plan path stored in file text record, empty before the first dispatch."""

    plan: bpy.props.StringProperty(name="Plan", description="Dispatched plan file", subtype="FILE_PATH")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Queueing(bpy.types.PropertyGroup):
    """Queue preferences: target, notices, power hold, heartbeat, and Flamenco scripts folder, read as the records the queue runs on."""

    kind: bpy.props.EnumProperty(name="Target", description="Where queue renders", items=TARGETS)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    retries: bpy.props.IntProperty(name="Retries", description="Restarts of crashed render from remaining frames", min=0, default=LocalMachine().retries)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    sampled: bpy.props.BoolProperty(name="Sampled", description="Render first two frames of each ready job before others", default=LocalMachine().sampled)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    manager: bpy.props.StringProperty(name="Manager", description="Flamenco Manager address", default=Flamenco().server.url)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    priority: bpy.props.IntProperty(name="Priority", description="Order among queued jobs", default=Flamenco().priority)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    chunk: bpy.props.IntProperty(name="Chunk", description="Frames each task renders", min=1, default=Flamenco().chunk)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    paused: bpy.props.BoolProperty(name="Paused", description="Jobs wait for start from Manager", default=Flamenco().paused)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    tag: bpy.props.StringProperty(name="Worker Tag", description="Workers taking jobs, every worker when empty")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    executable: bpy.props.StringProperty(name="B-Renderon", description="B-Renderon program, queue file alone when empty", subtype="FILE_PATH")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    blender: bpy.props.StringProperty(name="B-Renderon Blender", description="B-Renderon name of rendering Blender", default=Renderon().blender)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    folder: bpy.props.StringProperty(name="Job Folder", description="Job file folder, extension folder when empty", subtype="DIR_PATH")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    command: bpy.props.StringProperty(name="Command", description="Program taking job file path, job file alone when empty", subtype="FILE_PATH")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    server: bpy.props.StringProperty(name="Notice Server", description="ntfy server", default=Listener().server.url)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    topic: bpy.props.StringProperty(name="Notice Topic", description="Topic devices subscribe to, no notices when empty")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    awake: bpy.props.BoolProperty(name="Keep Awake", description="Prevent sleep while queue renders", default=PowerHold().awake)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    shutdown: bpy.props.BoolProperty(name="Shut Down", description="Shut down one minute after queue finishes", default=PowerHold().shutdown)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    interval: bpy.props.FloatProperty(  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
        name="Heartbeat",
        description="Seconds between progress record touches",
        subtype="TIME_ABSOLUTE",
        unit="TIME_ABSOLUTE",
        min=sys.float_info.min,
        max=threading.TIMEOUT_MAX,
        default=Heartbeat().interval,
    )
    misses: bpy.props.IntProperty(name="Missed Heartbeats", description="Missed heartbeats before run reads as stopped", min=2, default=Heartbeat().misses)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    scripts_folder: bpy.props.StringProperty(name="Flamenco Scripts", description="Manager job type scripts folder, none when empty", subtype="DIR_PATH")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]

    @property
    def target(self) -> Target:
        """Target record of the chosen kind."""
        match self.kind:
            case Flamenco.__name__:
                return Flamenco(server=Endpoint(url=self.manager), priority=self.priority, chunk=self.chunk, paused=self.paused, tag=self.tag or None)
            case Renderon.__name__:
                return Renderon(executable=self.executable or None, blender=self.blender)
            case JobFile.__name__:
                return JobFile(folder=self.folder or None, command=self.command or None)
            case _:
                return LocalMachine(retries=self.retries, sampled=self.sampled)

    @property
    def listener(self) -> Listener:
        """Notice server and topic."""
        return Listener(server=Endpoint(url=self.server), topic=self.topic or None)

    @property
    def power(self) -> PowerHold:
        """Sleep and shutdown hold."""
        return PowerHold(awake=self.awake, shutdown=self.shutdown)

    @property
    def heartbeat(self) -> Heartbeat:
        """Progress touch interval and stop threshold."""
        return Heartbeat(interval=self.interval, misses=self.misses)

    @property
    def scripts(self) -> Option[str]:
        """Manager job type scripts folder, `Nothing` while empty."""
        return Option.of_optional(self.scripts_folder or None)


class Observed(msgspec.Struct, frozen=True, kw_only=True):
    """Sent record and local progress status timer last read."""

    sent: Option[Sent]
    progress: Option[tuple[Progress, datetime.datetime]]


# --- [SERVICES] -------------------------------------------------------------------------


class QueueState:
    """Session state of one extension's queue: Manager facts, last observation, and paused viewports with the state each held before the pause."""

    __slots__ = ("manager", "observed", "paused")

    def __init__(self) -> None:
        """Holds no Manager facts, observation, or paused viewport."""
        self.manager: Option[Manager] = Nothing
        self.observed: Option[Observed] = Nothing
        self.paused: tuple[tuple[Located, EditorState], ...] = ()

    def pausing(self, held: tuple[tuple[Located, Option[EditorState]], ...]) -> None:
        """Holds each viewport a pause changed with the state it held before."""
        self.paused = (*self.paused, *((at, prior) for at, found in held for prior in found.to_list()))

    def resumed(self) -> tuple[tuple[Located, EditorState], ...]:
        """Returns paused viewports with their prior states, holding none after."""
        paused, self.paused = self.paused, ()
        return paused


QUEUES: Final = Resource[QueueState](made=lambda _identity: nullcontext(QueueState()))


class Dispatcher(msgspec.Struct, frozen=True):
    """Stores, routing, render, plan revision, and settings queue operations read, with the extension's session state."""

    identity: Identity
    host: Host
    members: Members
    framed: Framed
    routing: PassRouting
    pathed: Pathed
    queued: Attached[Queued]
    render: Render
    planning: Option[Callable[[Identity, Plan], Outcome[Plan]]]
    settings: Callable[[Identity], Outcome[Queueing]]

    # --- [STATE]

    @property
    def state(self) -> QueueState:
        """Session state the extension holds."""
        return self.identity.held[QUEUES]

    # --- [ACCESS]

    @staticmethod
    def connected() -> Outcome[None]:
        """Requires `bpy.app.online_access`, set by preferences and `--offline-mode`."""
        return Ok(None) if bpy.app.online_access else Error((Fault(reason=BlenderReason.UNMET, subject=ONLINE.message),))

    @staticmethod
    def posted(listener: Listener, notice: Notice) -> Outcome[None]:
        """Posts notice, requiring online access for an enabled topic."""
        return (Dispatcher.connected() if listener.topic is not None else Ok(None)).bind(lambda _: published(listener, notice))

    # --- [PLANNING]

    def planned(self, context: bpy.types.Context, dispatch: Dispatch, moment: datetime.datetime) -> Outcome[Plan]:
        """Plans reach scopes, then revises the plan through the extension's planning."""

        def bound(scene: bpy.types.Scene) -> Bound:
            return Bound(
                camera=Option.of_optional(scene.camera).map(lambda camera: camera.name).to_optional(),
                markers=tuple(
                    Marker(frame=marker.frame, camera=camera.name)
                    for marker in sorted(scene.timeline_markers, key=lambda marker: marker.frame)
                    for camera in Option.of_optional(marker.camera).to_list()
                ),
            )

        def reached(scene: bpy.types.Scene) -> Outcome[tuple[JobScope, ...]]:
            match Reach[dispatch.reach]:
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

        return (
            rooted(bpy.types.Scene, context)
            .bind(reached)
            .bind(lambda scopes: Ok(Plan(moment=moment, scopes=scopes)) if scopes else Error((Fault(reason=BlenderReason.ABSENT, subject=repr(context), member=dispatch.reach),)))
            .bind(lambda plan: self.planning.map(lambda revise: revise(self.identity, plan)).default_value(Ok(plan)))
        )

    # --- [DISPATCH]

    def dispatch(self, call: Call[Dispatch]) -> Outcome[Done]:
        """Plans jobs, writes their files in a background process, and sends the queue to its target."""
        context, identity, day = call.context, call.identity, datetime.datetime.now(datetime.UTC).astimezone().date()

        def gated(plan: Plan, queueing: Queueing) -> Outcome[None]:
            return paired(checked(self.pathed, plan.scopes, day), Dispatcher.connected() if isinstance(queueing.target, Flamenco) else Ok(None)).map(lambda _: None)

        def paused(manager: bpy.types.WindowManager) -> Outcome[None]:
            def applied(state: EditorState) -> Outcome[None]:
                return accumulated(state.applied(at.area).map(lambda prior: (at, prior)) for at in AreaQuery(space="VIEW_3D", shows=Some(PAUSABLE)).located(manager.windows)).map(self.state.pausing)

            return EditorState.of(bpy.types.SpaceView3D, {"region_3d.pause_render": True}).bind(applied)

        def written(plan: Plan, queueing: Queueing, manager: bpy.types.WindowManager) -> Outcome[Done]:
            path = identity.folder / f"{content_hash(Plan, plan).hex()}{PlanFile.PLAN}"
            scenes = tuple(dict.fromkeys(bpy.data.scenes[scope.scene] for scope in plan.scopes))
            return (
                accumulated(self.pathed.took(scene, day) for scene in scenes)
                .bind(lambda _: RecordFile(path=path, form=Plan).written(plan))
                .bind(lambda _: assigned(self.queued.struct(manager), {"plan": str(path)}))
                .bind(lambda _: saved(Path(bpy.data.filepath), SaveMode.ACTIVE))
                .bind(lambda source: detached(self.host.argv(RunCommand.PREPARE, Preparing(plan=str(path), target=queueing.target), Some(str(source))), PlanFile.LOG.beside(path)))
                .map(lambda log: Done(notes=(("INFO", f"{len(plan.scopes)} jobs preparing, log at {log}"),), skipped=paused(manager).swap().to_option()))
            )

        return (
            paired(self.planned(context, call.properties, datetime.datetime.now(datetime.UTC)), self.settings(identity), rooted(bpy.types.WindowManager, context))
            .bind(lambda parts: gated(parts[0], parts[1]).map(lambda _: parts))
            .bind(lambda parts: written(*parts))
        )

    def prepared(self, arguments: Preparing, trailing: tuple[str, ...]) -> Outcome[Sent]:
        """Writes job files of stored plan, sends queue to target, and stores `Sent` record."""
        if trailing:
            return Error((Fault(reason=RecordReason.OUTSIDE, subject=RunCommand.PREPARE, member="no trailing arguments", cause=repr(trailing)),))
        path = Path(arguments.plan)
        source = Path(bpy.data.filepath)

        def sent(jobs: tuple[Job, ...]) -> Outcome[Sent]:
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

        loaded: Outcome[Plan] = RecordFile(path=path, form=Plan).read()
        return Result[Plan, Faults].bind(loaded, lambda plan: accumulated(self.job(scope, plan, source, path, part) for part, scope in enumerate(plan.scopes))).bind(sent)

    def routed(self, scene: bpy.types.Scene, day: datetime.date) -> Outcome[OutputPath]:
        """Composes the job copy's output path, writes it to the scene, and reconciles pass routing to it."""
        return (
            self.pathed
            .path(scene, day)
            .map(lambda path: path.copied)
            .bind(lambda path: path.scene_output().bind(lambda text: Pathed.filepath.write(scene.render, text)).map(lambda _: path))
            .bind(lambda path: self.routing.reconciled(scene, bpy.context, path).map(lambda _: path))
        )

    def job(self, scope: JobScope, plan: Plan, source: Path, plan_file: Path, part: int) -> Outcome[Job]:
        """Writes scope's packed job file from saved source, the scope's values and fixed camera applied first."""
        day = plan.moment.astimezone().date()

        def applied(scene: bpy.types.Scene) -> Outcome[tuple[OutputPath, tuple[Side, Side]]]:
            camera = (
                Snapshot
                .of(bpy.types.Scene, {"camera": IdName(name=scope.camera.camera), **{f"timeline_markers[{index}].camera": None for index in range(len(scene.timeline_markers))}})
                .apply(scene)
                .outcome
                if isinstance(scope.camera, Fixed)
                else Ok(None)
            )
            values = Option.of_optional(scope.values).map(lambda snapshot: snapshot.apply(scene).outcome).default_value(Ok(None))
            layers = (
                Option
                .of_optional(scope.layer)
                .map(lambda name: accumulated(self.routing.use.write(layer, layer.name == name) for layer in scene.view_layers).map(lambda _: None))
                .default_value(Ok(None))
            )
            return paired(camera, values, layers, self.framed.grown(scene)).bind(lambda parts: self.routed(scene, day).map(lambda path: (path, parts[3][1])))

        def digested(path: Path) -> Outcome[bytes]:
            @catch(exception=OSError)
            def read() -> bytes:
                with path.open("rb") as file:
                    return hashlib.file_digest(file, "sha256").digest()

            return read().map_error(lambda error: (Fault(reason=RecordReason.UNREADABLE, subject=str(path), cause=error),))

        def written(scene: bpy.types.Scene, path: OutputPath, pixels: tuple[Side, Side]) -> Outcome[Job]:
            used = self.routing.used(scene)
            camera = (scope.camera.camera,) if isinstance(scope.camera, Fixed) else ()
            name = " ".join(bpy.path.clean_name(held) for held in (scope.scene, *Option.of_optional(scope.layer).to_list(), *camera))
            file = path.base / "snapshots" / plan_file.name.removesuffix(PlanFile.PLAN) / f"{part}_{name}.blend"
            active: bpy.types.ViewLayer = bpy.context.view_layer  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            layer = Option.of_optional(scope.layer).default_value(active.name)
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
                    passes=tuple(
                        sorted({rna(descriptor.path) for held in used for _, descriptor in pass_flags() for owner in struct_at(held, descriptor.owner).to_list() if bool(descriptor.read(owner))})
                    ),
                    prerequisites=scope.prerequisites,
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

        def prepared(_: BlendFile) -> Outcome[Job]:
            scene = bpy.data.scenes[scope.scene]
            return applied(scene).bind(lambda parts: written(scene, *parts))

        return session_loaded(OpenFile(path=source)).bind(prepared)

    # --- [BACKGROUND_RENDER]

    def frames[T](self, scene: bpy.types.Scene, frames: tuple[int, ...], consumed: Callable[[int, float], Outcome[T]], base: Option[Path] = Nothing) -> Outcome[tuple[T, ...]]:
        """Renders and consumes each frame, output rerooted under `base` when set."""

        def frame(number: int) -> Outcome[T]:
            started = time.monotonic()
            scene.frame_start = scene.frame_end = number
            return self.render(self.identity, scene, RenderCall(animation=True)).bind(lambda _: consumed(number, time.monotonic() - started))

        def rerooted(original: OutputPath, root: Path) -> Outcome[None]:
            path = original.rerooted(root)
            return (
                self.pathed.scope
                .write(scene, path)
                .bind(lambda _: path.scene_output())
                .bind(lambda text: Pathed.filepath.write(scene.render, text))
                .bind(lambda _: self.routing.reconciled(scene, bpy.context, path))
                .map(lambda _: None)
            )

        return (
            self.pathed.scope
            .read(scene)
            .bind(lambda found: found.to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=repr(scene), member=self.pathed.scope.name),)))
            .bind(lambda original: base.map(lambda root: rerooted(original, root)).default_value(Ok(None)))
            .bind(lambda _: reduce(lambda done, number: done.bind(lambda values: frame(number).map(lambda value: (*values, value))), frames, Result[tuple[T, ...], Faults](ok=())))
        )

    def commanded(self, arguments: Run | RenderFrames, trailing: tuple[str, ...]) -> Outcome[object]:
        """Runs local queue progress or farm worker frame request."""
        match arguments:
            case Run():
                return Error((Fault(reason=RecordReason.OUTSIDE, subject=RunCommand.RENDER, member="no trailing arguments", cause=repr(trailing)),)) if trailing else self.rendered(arguments)
            case RenderFrames(scene=name, frames=frames):
                match trailing:
                    case (output_root,):
                        scene: Outcome[bpy.types.Scene] = Option.of_optional(bpy.data.scenes.get(name)).to_result_with(
                            lambda: (Fault(reason=BlenderReason.ABSENT, subject=repr(bpy.data.scenes), member=name),)
                        )
                        return Result[bpy.types.Scene, Faults].bind(scene, lambda held: self.frames(held, frames, lambda _frame, _seconds: Ok(None), Some(Path(output_root)))).map(lambda _: None)
                    case _:
                        return Error((Fault(reason=RecordReason.OUTSIDE, subject=RunCommand.RENDER, member="<output root>", cause=repr(trailing)),))

    def supervised(self, run: Run, trailing: tuple[str, ...]) -> Outcome[Progress]:
        """Supervises local render processes under power hold, posting notices."""
        if trailing:
            return Error((Fault(reason=RecordReason.OUTSIDE, subject=RunCommand.SUPERVISE, member="no trailing arguments", cause=repr(trailing)),))

        def ran(queue: RenderQueue, queueing: Queueing) -> Outcome[Progress]:
            def completed(progress: Progress) -> Outcome[Progress]:
                self.identity.reports.reported(self.posted)(
                    queueing.listener, Completed(frames=len(progress.marks), seconds=sum((Fraction(mark.seconds) for mark in progress.marks), Fraction()))
                ).to_optional()
                return (shut() if queueing.power.shutdown else Ok(None)).map(lambda _: progress)

            def failed(faults: Faults) -> Outcome[Progress]:
                self.identity.reports.reported(self.posted)(queueing.listener, Failed(job=queue.stem, reason=" ".join(fault.message for fault in faults))).to_optional()
                return Error(faults)

            match observed(self.host.path(queue, RunFile.PROGRESS)):
                case Result(tag="error", error=faults):
                    return Error(faults)
                case Result(ok=observations):
                    if not Progress.resumed(observations, queue, os.getpid()).marks:
                        self.identity.reports.reported(self.posted)(queueing.listener, Started(jobs=len(queue.jobs), frames=len(queue.steps))).to_optional()
                    return queueing.power.held(lambda: supervise(run.target, queue, self.host, queueing.heartbeat)).map(completed).default_with(failed)

        return paired(RecordFile(path=Path(run.queue), form=RenderQueue).read(), self.settings(self.identity)).bind(lambda pair: ran(*pair))

    def rendered(self, run: Run) -> Outcome[tuple[StageMark, ...]]:
        """Renders pending frames of first remaining job, storing each frame mark."""

        def ran(queue: RenderQueue, queueing: Queueing) -> Outcome[tuple[StageMark, ...]]:
            records = RecordFile(path=self.host.path(queue, RunFile.PROGRESS), form=Progress)
            holding = frozenset(part for part, job in enumerate(queue.jobs) if job.frames.numbers)

            def finished(progress: Progress, mark: StageMark) -> Outcome[StageMark]:
                before = frozenset(held.part for held in progress.marks[:-1])
                first = holding <= before | {mark.part} and not holding <= before
                for forecast in progress.forecast(queue, datetime.datetime.now(datetime.UTC)).filter(lambda _: first).to_list():
                    self.identity.reports.reported(self.posted)(queueing.listener, Estimated(forecast=forecast)).to_optional()
                job = queue.jobs[mark.part]
                scene = bpy.data.scenes[job.scene]
                return (
                    self.pathed.scope
                    .read(scene)
                    .bind(lambda found: found.to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=repr(scene), member=self.pathed.scope.name),)))
                    .bind(lambda path: path.removed(scene, job.frames.numbers))
                    .map(lambda _: mark)
                    if all(part != mark.part for part, _ in progress.remaining(queue))
                    else Ok(mark)
                )

            def appended(mark: StageMark) -> Outcome[StageMark]:
                persisted: Outcome[Progress] = records.revised(lambda held: msgspec.structs.replace(held, marks=(*held.marks, mark)))
                return Result[Progress, Faults].bind(persisted, lambda held: finished(held, mark))

            def runs(progress: Progress) -> Outcome[tuple[StageMark, ...]]:
                remaining = frozenset(progress.remaining(queue))
                steps = tuple(step for step in progress.pending if step in remaining)
                return (
                    self.frames(
                        bpy.data.scenes[queue.jobs[steps[0][0]].scene], tuple(frame for _, frame in steps), lambda frame, seconds: appended(StageMark(part=steps[0][0], item=frame, seconds=seconds))
                    )
                    if steps
                    else Ok(())
                )

            loaded: Outcome[Progress] = records.read()
            return Result[Progress, Faults].bind(loaded, runs)

        return paired(RecordFile(path=Path(run.queue), form=RenderQueue).read(), self.settings(self.identity)).bind(lambda pair: ran(*pair))

    # --- [SESSION_PROGRESS]

    def observed(self, context: bpy.types.Context) -> Outcome[Option[Observed]]:
        """Reads sent record and local progress, `Nothing` sent before `prepare` writes it."""

        def read(plan: Path) -> Outcome[Observed]:
            def pending(faults: Faults) -> Outcome[Option[Sent]]:
                match faults:
                    case (Fault(reason=RecordReason.UNREADABLE, cause=FileNotFoundError()),):
                        return Ok(Nothing)
                    case _:
                        return Error(faults)

            return (
                Result[Sent, Faults]
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

        return rooted(bpy.types.WindowManager, context).bind(lambda manager: queued_plan(self.queued.struct(manager)).map(lambda plan: read(plan).map(Some)).default_value(Ok(Nothing)))

    def kept(self) -> Outcome[tuple[RenderQueue, LocalMachine, Progress, datetime.datetime]]:
        """Returns observed local run, absent fault without local progress."""
        match self.state.observed.bind(lambda held: held.sent.bind(lambda sent: held.progress.map(lambda pair: (sent, *pair)))).to_optional():
            case (Sent(queue=queue, target=LocalMachine() as target), progress, beat):
                return Ok((queue, target, progress, beat))
            case _:
                return Error((Fault(reason=BlenderReason.ABSENT, subject=repr(self.queued), member=RunFile.PROGRESS),))

    def restored(self) -> Outcome[Option[str]]:
        """Restores paused viewports still open, run as deferred callback."""
        return accumulated(prior.applied(now.area) for at, prior in self.state.resumed() for now in Located.found(at.window, at.area.as_pointer()).to_list()).map(lambda _: Nothing)

    def ticked(self, restore: Callable[[], None]) -> Outcome[Option[float]]:
        """Refreshes observation each second, restoring viewports once run ends."""

        def acted(fresh: Option[Observed]) -> Outcome[None]:
            prior = self.state.observed
            current = fresh.map(
                lambda now: msgspec.structs.replace(
                    now, progress=now.progress.or_else_with(lambda: prior.bind(lambda last: last.progress).filter(lambda pair: now.sent.filter(lambda sent: sent.queue.key == pair[0].queue).is_some()))
                )
            )
            self.state.observed = current
            if current.bind(lambda held: held.progress).bind(lambda pair: Option.of_optional(pair[0].ending)).is_some() and self.state.paused:
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

    def resume(self, call: Call[Empty]) -> Outcome[Done]:
        """Renders the remaining frames of a stopped, failed, or canceled queue."""

        def resent(queue: RenderQueue, target: LocalMachine, progress: Progress, beat: datetime.datetime, queueing: Queueing) -> Outcome[Done]:
            phase = progress.phase(beat, datetime.datetime.now(datetime.UTC), queueing.heartbeat)
            resumable = Liveness.STOPPED, Ending.FAILED, Ending.CANCELLED
            return (
                send(target, queue, self.host).map(
                    lambda issued: Done(notes=tuple(("INFO", value) for answer in issued for value in answer.to_option().to_list()), skipped=accumulated(issued).swap().to_option())
                )
                if phase in resumable
                else Error((Fault(reason=RecordReason.OUTSIDE, subject=str(self.host.path(queue, RunFile.PROGRESS)), member=", ".join(value.name for value in resumable), cause=str(phase.name)),))
            )

        return paired(self.kept(), self.settings(call.identity)).bind(lambda pair: resent(*pair[0], pair[1]))

    def stop(self, call: Call[Empty]) -> Outcome[Done]:
        """Stops the queue and its renders."""

        def halted(queue: RenderQueue, progress: Progress, queueing: Queueing) -> Outcome[Done]:
            def ended(_: None) -> Outcome[Done]:
                persisted = RecordFile(path=self.host.path(queue, RunFile.PROGRESS), form=Progress).revised(lambda held: msgspec.structs.replace(held, ending=Ending.CANCELLED))
                return paired(persisted, self.posted(queueing.listener, Cancelled(frames=len(progress.marks)))).map(lambda _: Done(notes=(("INFO", f"Stopped {queue.stem}"),)))

            stopped: Outcome[None] = stop(progress)
            return Result[None, Faults].bind(stopped, ended)

        return paired(self.kept(), self.settings(call.identity)).bind(lambda pair: halted(pair[0][0], pair[0][2], pair[1]))

    def open_log(self, call: Call[Empty]) -> Outcome[Done]:
        """Opens the log of the last sent queue, else of the last plan."""

        def opened_log(plan: Path) -> Outcome[Done]:
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
            .bind(lambda manager: queued_plan(self.queued.struct(manager)).to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=QueueRun.record.key, member="plan"),)))
            .bind(opened_log)
        )

    def cancel_shutdown(self, call: Call[Empty]) -> Outcome[Done]:
        """Cancels the shutdown scheduled after the local queue."""

        def cancelling(progress: Progress, power: PowerHold) -> Outcome[Done]:
            return (
                cancelled(progress).map(lambda _: Done(notes=(("INFO", "Shutdown canceled"),)))
                if power.shutdown
                else Error((Fault(reason=BlenderReason.ABSENT, subject=repr(power), member="shutdown"),))
            )

        return paired(self.kept(), self.settings(call.identity)).bind(lambda pair: cancelling(pair[0][2], pair[1].power))

    def pack(self, call: Call[Empty]) -> Outcome[Done]:
        """Writes a packed file copy under the render output folder."""
        moment = datetime.datetime.now(datetime.UTC).astimezone()

        def done(written: Path, missing: tuple[Fault, ...]) -> Done:
            return Done(notes=(("INFO", str(written)),), skipped=accumulated(Error((found,)) for found in missing).swap().to_option())

        return (
            call
            .root(bpy.types.Scene)
            .bind(lambda scene: self.pathed.path(scene, moment.date()))
            .bind(lambda path: packed(path.base / "packs" / f"{Path(bpy.data.filepath).stem}_{moment:%Y%m%d-%H%M%S}", compress=Some(True)))
            .bind(
                lambda pack: resolved(item.owner for item in pack.missing).map(
                    lambda owners: done(pack.blend, tuple(Fault(reason=BlenderReason.ABSENT, subject=repr(owner), member=str(item.path)) for item, owner in zip(pack.missing, owners)))
                )
            )
        )

    def read_manager(self, call: Call[Empty]) -> Outcome[Done]:
        """Reads Manager storage, worker tags, and job types."""

        def flamenco(queueing: Queueing) -> Outcome[Flamenco]:
            match queueing.target:
                case Flamenco() as target:
                    return Ok(target)
                case other:
                    return Error((Fault(reason=RecordReason.OUTSIDE, subject=repr(queueing), member=Flamenco.__name__, cause=type(other).__name__),))

        def held(manager: Manager) -> Done:
            self.state.manager = Some(manager)
            return Done(notes=(("INFO", f"{len(manager.tags)} worker tags, job type {'installed' if manager.job_type.is_some() else 'missing'}"),))

        return self.settings(call.identity).bind(flamenco).bind(lambda target: Dispatcher.connected().bind(lambda _: target.server.opened(Manager.queried))).map(held)

    def install_job_type(self, call: Call[Empty]) -> Outcome[Done]:
        """Copies the job type script into the Manager scripts folder."""

        def copied(folder: str) -> Outcome[Done]:
            @catch(exception=OSError)
            def written() -> Path:
                with importlib.resources.as_file(importlib.resources.files(Flamenco.__module__).joinpath(Flamenco.script)) as script:
                    return Path(shutil.copy(script, Path(folder) / Flamenco.script))

            return written().map_error(lambda error: (Fault(reason=RecordReason.UNWRITABLE, subject=folder, cause=error),)).map(lambda path: Done(notes=(("INFO", str(path)),)))

        return self.settings(call.identity).bind(
            lambda queueing: queueing.scripts.map(copied).default_with(lambda: Error((Fault(reason=BlenderReason.ABSENT, subject=repr(call.context.preferences), member="scripts"),)))
        )

    def test_notice(self, call: Call[Empty]) -> Outcome[Done]:
        """Posts a test notice to the listener topic."""
        return self.settings(call.identity).bind(lambda queueing: self.posted(queueing.listener, Test())).map(lambda _: Done(notes=(("INFO", "Notice posted"),)))


class QueueRun[C: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Render queue commands, status timer, section with its progress row, and preferences section, each frame running through `render` and each plan revised through `planning`."""

    sets: RenderSet[C]
    settings: Callable[[Identity], Outcome[Queueing]]
    render: Render = native
    planning: Option[Callable[[Identity, Plan], Outcome[Plan]]] = Nothing
    record: ClassVar[TextRecord[Queued]] = TextRecord(key="queue", form=Queued)
    key: ClassVar[Key] = "render_queue"
    title: ClassVar[str] = bpy.path.display_name(key)

    # --- [COMMANDS]

    @cached_property
    def dispatching(self) -> Operator[Dispatch]:
        """Dispatch command binding its dispatcher on call."""

        def drawn(layout: bpy.types.UILayout, context: bpy.types.Context, properties: bpy.types.OperatorProperties, identity: Identity) -> None:
            dispatch: Dispatch = properties  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            lines = (
                self
                .bound(identity)
                .bind(lambda dispatcher: paired(dispatcher.planned(context, dispatch, datetime.datetime.now(datetime.UTC)), dispatcher.settings(identity)))
                .map(lambda pair: (type(pair[1].target).__name__, f"{len(pair[0].scopes)} jobs, {sum(len(ranged(bpy.data.scenes[scope.scene]).numbers) for scope in pair[0].scopes)} frames"))
                .default_with(lambda faults: tuple(fault.message for fault in faults))
            )
            for line in lines:
                layout.label(text=line)
            for name in Dispatch.__annotations__:
                layout.prop(properties, name)

        return Operator[Dispatch](
            icon=Some("RENDER_ANIMATION"),
            condition=Some(Preflight.condition),
            form=Dispatch,
            run=update_wrapper(partial(self.stepped, Dispatcher.dispatch), Dispatcher.dispatch),
            undo=False,
            invoke=Some(Properties()),
            draw=Some(drawn),
        )

    @cached_property
    def controls(self) -> tuple[Operator[Empty], ...]:
        """Run control and packing commands in section order."""
        return (
            self.stepping(Dispatcher.resume, "PLAY"),
            self.stepping(Dispatcher.stop, "CANCEL"),
            self.stepping(Dispatcher.open_log, "TEXT"),
            self.stepping(Dispatcher.cancel_shutdown, "QUIT"),
            self.stepping(Dispatcher.pack, "PACKAGE"),
        )

    @cached_property
    def setup(self) -> tuple[Operator[Empty], ...]:
        """Manager reads, job type install, and notice test in preferences order."""
        return self.stepping(Dispatcher.read_manager, "FILE_REFRESH"), self.stepping(Dispatcher.install_job_type, "IMPORT"), self.stepping(Dispatcher.test_notice, "INFO")

    def stepping(self, step: Step, icon: "rna_enums.IconItems") -> Operator[Empty]:
        """Command running a dispatcher step, keyed and described by the step."""
        return Operator(icon=Some(icon), undo=False, form=Empty, run=update_wrapper(partial(self.stepped, step), step))

    def stepped[T: bpy.types.PropertyGroup](self, step: Callable[[Dispatcher, Call[T]], Outcome[Done]], call: Call[T]) -> Outcome[Done]:
        """Runs a queue step over the dispatcher the call's identity binds."""
        return self.bound(call.identity).bind(lambda dispatcher: step(dispatcher, call))

    # --- [BINDING]

    @property
    def prerequisites(self) -> tuple[Declaration, ...]:
        """Queue store and session state with render set, frame size, routing, and path bindings read."""
        return (self.record, QUEUES, *self.sets.prerequisites, FrameSize.attachment, PassSet.attachment, ScenePaths.attachment)

    def bound(self, identity: Identity) -> Outcome[Dispatcher]:
        """Binds dispatcher to identity stores, render sets, routing, render, planning, and settings."""
        host = accumulated(identity.identifier(command) for command in RunCommand).map(
            lambda names: Host(binary=bpy.app.binary_path, folder=str(identity.folder), commands=dict(zip(RunCommand, names)))
        )
        return paired(host, self.sets.bound(identity), FrameSize().bound(identity), PassSet().bound(identity), ScenePaths().bound(identity), QueueRun.record.attachment.bound(identity)).map(
            lambda parts: Dispatcher(
                identity=identity,
                host=parts[0],
                members=parts[1],
                framed=parts[2],
                routing=parts[3],
                pathed=parts[4],
                queued=parts[5],
                render=self.render,
                planning=self.planning,
                settings=self.settings,
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
            finish = progress.forecast(sent.queue, now).bind(lambda forecast: forecast.finish).map(lambda moment: f"finishing {moment.astimezone():%H:%M}").to_list()
            return rows.Work(text=" · ".join((*(sent.queue.jobs[part].title for part, _ in remaining[:1]), bpy.path.display_name(phase.name.lower()), *finish)), done=float(progress.done(sent.queue)))

        def work(held: Observed, heartbeat: Heartbeat) -> rows.Work:
            return held.sent.map(
                lambda sent: held.progress.map(lambda pair: local(sent, *pair, heartbeat)).default_with(
                    lambda: rows.Work(text=" ".join((type(sent.target).__name__, *sent.issued)), done=0.0 if isinstance(sent.target, LocalMachine) else 1.0)
                )
            ).default_with(lambda: rows.Work(text="Preparing", done=0.0))

        return (
            self
            .settings(identity)
            .map(lambda queueing: tuple(work(held, queueing.heartbeat) for held in identity.held[QUEUES].observed.to_list()))
            .default_with(lambda faults: tuple(rows.Work(text=fault.message, done=0.0) for fault in faults))
        )

    @property
    def section(self) -> Single:
        """Queue section: dispatch, run progress read from session state at draw, status, and run controls."""
        return Single(
            key=self.key,
            title=self.title,
            rows=(
                rows.OperatorButton(target=self.dispatching),
                (self.dispatching, lambda identity: rows.Progress(runs=lambda context: self.runs(identity, context)).drawn(identity)),
                Status(),
                rows.Caption(rows=tuple(rows.OperatorButton(target=command, text="") for command in self.controls), flow=rows.Flow.ALIGNED, separator=Nothing),
            ),
        )

    @property
    def preferences(self) -> PageRows:
        """Preferences section of Manager reads, job type install, and notice test."""
        return PageRows(
            sections=(
                Single(
                    key=f"{self.key}_setup",
                    title=self.title,
                    rows=(rows.Caption(rows=tuple(rows.OperatorButton(target=command) for command in self.setup), flow=rows.Flow.ALIGNED, separator=Nothing),),
                ),
            )
        )

    # --- [REGISTRATION]

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[Outcome[Acquisitions]]:
        """Holds deferred viewport restore while registered."""
        with ExitStack() as scope:

            def joined(dispatcher: Dispatcher) -> Outcome[Acquisitions]:
                restore = scope.enter_context(identity.reports.deferred(dispatcher.restored))
                scope.callback(identity.reports.reported(dispatcher.restored))
                return Ok((QUEUES, *self.cli(dispatcher), self.dispatching, *self.controls, *self.setup, Timer(body=lambda: dispatcher.ticked(restore))))

            yield self.bound(identity).bind(joined)


# --- [OPERATIONS] -----------------------------------------------------------------------


def queued_plan(queued: Queued) -> Option[Path]:
    """Returns the dispatched plan path a queue store holds, `Nothing` before the first dispatch."""
    return Option.of_optional(queued.plan or None).map(Path)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "ONLINE",
    "PAUSABLE",
    "QUEUES",
    "Dispatch",
    "Dispatcher",
    "Observed",
    "Plan",
    "PlanFile",
    "Preparing",
    "QueueRun",
    "QueueState",
    "Queued",
    "Queueing",
    "Reach",
    "Sent",
    "Step",
    "queued_plan",
]
