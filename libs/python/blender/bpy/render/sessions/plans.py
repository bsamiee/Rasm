"""Render queue plans: the dispatch form and the scopes it plans, queue preferences read as the records a queue runs on, the dispatcher every queue step binds with its observation of a dispatched run, and the checks a plan passes before dispatch."""

from collections.abc import Callable, Generator
from contextlib import contextmanager, ExitStack
import datetime
from enum import auto, Enum
from functools import partial
from itertools import groupby, starmap
import threading
from typing import Final, Self

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
import msgspec
import numpy as np

from blender.bpy.data.rna.conditions import Setting
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.data.rna.mirrors import case_choices, enum_choices
from blender.bpy.data.rna.paths import rooted
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.identity import Identity, Resource
from blender.bpy.render.outputs.framing import Framed
from blender.bpy.render.outputs.paths import OutputPath, Pathed
from blender.bpy.render.sessions.members import Members
from blender.bpy.render.sessions.preflight import audited
from blender.bpy.render.sessions.renders import Render
from blender.bpy.render.view_layers.passes import layer_of, used
from blender.bpy.render.view_layers.routing import PassRouting
from blender.bpy.settings.attachments import Attached
from blender.bpy.ui.screens.areas import EditorState, Located
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired, Reason, typed
from blender.records.stores import RecordFile
from blender.rendering.batch.notices import Listener, Notice, published
from blender.rendering.batch.queue import Ending, FixedCamera, Frames, JobCamera, MarkedCamera, Marker, observed, Progress, RenderQueue
from blender.rendering.batch.supervision import cleared, HostCommands, LocalMachine, RunFile
from blender.rendering.batch.targets import Endpoint, Flamenco, JobFile, Renderon, Target
from blender.rendering.outputs.naming import NumberField, Output, Word

# --- [TYPES] ----------------------------------------------------------------------------


class Reach(Enum):
    """Dispatch render scope."""

    SETS = auto()
    SCENE = auto()
    LAYER = auto()
    CAMERAS = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

_ONLINE: Final = Setting(descriptor=Descriptor.declared(bpy.types.Preferences, ("system", "use_online_access"), bpy.types.BoolProperty))

# --- [MODELS] ---------------------------------------------------------------------------


class Dispatch(bpy.types.PropertyGroup):
    """Dispatch dialog options."""

    reach: bpy.props.EnumProperty(name="Render", description="Scope queue renders", items=enum_choices(Reach))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class JobScope(msgspec.Struct, frozen=True, kw_only=True):
    """Planned job render scope: the scene values its job file takes before writing, and the plan positions of scopes whose frames it reads."""

    scene: str
    camera: JobCamera
    layer: str | None = None
    values: Snapshot | None = None
    prerequisites: tuple[int, ...] = ()


class Plan(msgspec.Struct, frozen=True, kw_only=True):
    """Ordered job scopes and dispatch time."""

    moment: datetime.datetime
    scopes: tuple[JobScope, ...]


class Preparing(msgspec.Struct, frozen=True, kw_only=True):
    """Plan key and target `prepare` command reads."""

    plan: str
    target: Target


class Sent(msgspec.Struct, frozen=True, kw_only=True):
    """Sent queue with target and each issued path or farm job ID."""

    queue: RenderQueue
    target: Target
    issued: tuple[str, ...]


class Queued(bpy.types.PropertyGroup):
    """Key of the last dispatched plan, stored in the file text record, empty before the first dispatch."""

    plan: bpy.props.StringProperty(name="Plan", description="Content key of the dispatched plan")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]

    @property
    def dispatched(self) -> Option[str]:
        """Plan key, `Nothing` before the first dispatch."""
        return Option.of_optional(self.plan or None)


class Queueing(bpy.types.PropertyGroup):
    """Queue preferences: target, notices, power hold, heartbeat, and Flamenco scripts folder, read as the records the queue runs on."""

    kind: bpy.props.EnumProperty(name="Target", description="Where queue renders", items=case_choices(Target))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
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
    awake: bpy.props.BoolProperty(name="Keep Awake", description="Prevent sleep while queue renders", default=True)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    shutdown: bpy.props.BoolProperty(name="Shut Down", description="Shut down one minute after queue finishes")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    interval: bpy.props.FloatProperty(  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
        name="Heartbeat", description="Seconds between progress record touches", subtype="TIME_ABSOLUTE", min=float(np.finfo(np.float32).tiny), max=threading.TIMEOUT_MAX, default=3.0
    )
    misses: bpy.props.IntProperty(name="Missed Heartbeats", description="Missed heartbeats before run reads as stopped", min=2, default=10)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
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
    def stale(self) -> datetime.timedelta:
        """Heartbeat age past which a run reads as stopped."""
        return datetime.timedelta(seconds=self.interval * self.misses)

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
    """Session state of one extension's queue: last observation, paused viewports with the state each held before the pause, and the timer observing a pending run."""

    __slots__ = ("observed", "paused", "watching")

    def __init__(self) -> None:
        """Holds no observation, paused viewport, or timer."""
        self.observed: Option[Observed] = Nothing
        self.paused: tuple[tuple[Located, EditorState], ...] = ()
        self.watching: Option[Callable[[], float | None]] = Nothing

    @classmethod
    @contextmanager
    def held(cls) -> Generator[Self]:
        """Yields fresh state, stopping its observation timer at scope exit."""
        state = cls()
        with ExitStack() as scope:
            scope.callback(state.unwatched)
            yield state

    def watched(self, tick: Callable[[], float | None]) -> None:
        """Starts the observation timer unless one runs, the timer stopping itself once nothing is pending."""
        if not self.watching.filter(bpy.app.timers.is_registered).is_some():
            self.watching = Some(tick)
            bpy.app.timers.register(tick, first_interval=1.0, persistent=True)

    def unwatched(self) -> None:
        """Stops the observation timer while the host holds it."""
        for tick in self.watching.filter(bpy.app.timers.is_registered).to_list():
            bpy.app.timers.unregister(tick)

    def pausing(self, held: tuple[tuple[Located, Option[EditorState]], ...]) -> None:
        """Holds each viewport a pause changed with the state it held before."""
        self.paused = (*self.paused, *((at, prior) for at, found in held for prior in found.to_list()))

    def resumed(self) -> tuple[tuple[Located, EditorState], ...]:
        """Returns paused viewports with their prior states, holding none after."""
        paused, self.paused = self.paused, ()
        return paused


QUEUES: Final = Resource[QueueState](made=lambda _identity: QueueState.held())


class Dispatcher(msgspec.Struct, frozen=True):
    """Stores, routing, render, plan revision, and settings queue operations read, with the extension's session state."""

    identity: Identity
    host: HostCommands
    members: Members
    framed: Framed
    routing: PassRouting
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
        return Ok(None) if bpy.app.online_access else Error(Fault.of(Reason.REQUIRED, _ONLINE.message))

    @staticmethod
    def posted(listener: Listener, notice: Notice) -> Outcome[None]:
        """Posts notice, requiring online access for an enabled topic."""
        return (Dispatcher.connected() if listener.topic is not None else Ok(None)).bind(lambda _: published(listener, notice))

    # --- [PLANNING]

    def planned(self, context: bpy.types.Context, dispatch: Dispatch, moment: datetime.datetime) -> Outcome[Plan]:
        """Plans reach scopes, then revises the plan through the extension's planning."""

        def marked(scene: bpy.types.Scene) -> MarkedCamera:
            return MarkedCamera(
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
                        lambda members: tuple(JobScope(scene=held.name, layer=layer.map(lambda value: value.name).to_optional(), camera=marked(held)) for held, layer in members)
                    )
                case Reach.SCENE:
                    return Ok((JobScope(scene=scene.name, camera=marked(scene)),))
                case Reach.LAYER:
                    return rooted(bpy.types.ViewLayer, context).map(lambda layer: (JobScope(scene=scene.name, layer=layer.name, camera=marked(scene)),))
                case Reach.CAMERAS:
                    return Ok(tuple(JobScope(scene=scene.name, camera=FixedCamera(camera=camera.name)) for camera in context.selected_objects if camera.type == "CAMERA"))

        return (
            rooted(bpy.types.Scene, context)
            .bind(reached)
            .bind(lambda scopes: Ok(Plan(moment=moment, scopes=scopes)) if scopes else Error(Fault.of(Reason.ABSENT, repr(context), member=dispatch.reach)))
            .bind(lambda plan: self.planning.map(lambda revise: revise(self.identity, plan)).default_value(Ok(plan)))
        )

    # --- [OBSERVATION]

    def observation(self, context: bpy.types.Context) -> Outcome[Option[Observed]]:
        """Reads sent record and local progress, `Nothing` sent before `prepare` writes it."""

        def pending(faults: Faults) -> Outcome[Option[Sent]]:
            match faults:
                case (Fault(reason=Reason.UNREADABLE, cause=FileNotFoundError()),):
                    return Ok(Nothing)
                case _:
                    return Error(faults)

        def read(plan: str) -> Outcome[Observed]:
            return (
                Result[Sent, Faults]
                .map(RecordFile(path=self.host.path(plan, RunFile.SENT), form=Sent).read(), Some)
                .or_else_with(pending)
                .bind(
                    lambda sent: (
                        sent
                        .filter(lambda record: isinstance(record.target, LocalMachine))
                        .map(lambda record: observed(self.host.path(record.queue.stem, RunFile.PROGRESS)))
                        .default_value(Ok(Nothing))
                        .map(lambda progress: Observed(sent=sent, progress=progress))
                    )
                )
            )

        return rooted(bpy.types.WindowManager, context).bind(lambda manager: self.queued.struct(manager).dispatched.map(lambda plan: read(plan).map(Some)).default_value(Ok(Nothing)))

    def kept(self) -> Outcome[tuple[RenderQueue, LocalMachine, Progress, datetime.datetime]]:
        """Returns observed local run, absent fault without local progress."""
        match self.state.observed.bind(lambda held: held.sent.bind(lambda sent: held.progress.map(lambda pair: (sent, *pair)))).to_optional():
            case (Sent(queue=queue, target=LocalMachine() as target), progress, beat):
                return Ok((queue, target, progress, beat))
            case _:
                return Error(Fault.of(Reason.ABSENT, repr(self.queued), member=RunFile.PROGRESS))

    def restored(self) -> Outcome[Option[str]]:
        """Restores paused viewports still open."""
        return accumulated(prior.applied(now.area) for at, prior in self.state.resumed() for now in Located.found(at.window, at.area.as_pointer()).to_list()).map(lambda _: Nothing)

    def ticked(self) -> Outcome[bool]:
        """Refreshes the observation, restoring viewports and clearing a succeeded local run once it ends, answering whether a dispatched run is still pending."""
        state = self.state

        def acted(fresh: Option[Observed]) -> Outcome[bool]:
            prior = state.observed
            current = fresh.map(
                lambda now: msgspec.structs.replace(
                    now, progress=now.progress.or_else_with(lambda: prior.bind(lambda last: last.progress).filter(lambda pair: now.sent.filter(lambda sent: sent.queue.key == pair[0].queue).is_some()))
                )
            )
            state.observed = current
            ended = current.bind(lambda held: held.progress).bind(lambda pair: Option.of_optional(pair[0].ending))
            pending = current.map(lambda held: held.sent.map(lambda sent: isinstance(sent.target, LocalMachine) and ended.is_none()).default_value(True)).default_value(False)
            return paired(
                self.restored() if ended.is_some() and state.paused else Ok(Nothing),
                fresh
                .bind(lambda now: now.sent.bind(lambda sent: now.progress.filter(lambda pair: pair[0].ending is Ending.SUCCEEDED).map(lambda _: sent.queue)))
                .map(lambda queue: cleared(queue, self.host))
                .default_value(Ok(None)),
            ).map(lambda _: pending)

        return self.observation(bpy.context).bind(acted)

    def tick(self) -> float | None:
        """Host timer body refreshing the observation each second, stopping once no dispatched run is pending."""
        return 1.0 if self.identity.reports.reported(self.ticked)().default_value(False) else None

    def watched(self) -> Outcome[Option[float]]:
        """Observes a dispatched run until nothing it dispatched is pending, the session state holding the one timer, answering as a one-shot timer body."""
        self.state.watched(self.tick)
        return Ok(Nothing)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [SCENES]


def scene_of(name: str) -> Outcome[bpy.types.Scene]:
    """Returns the scene a planned scope or job names, absent fault once it is gone."""
    return Option.of_optional(bpy.data.scenes.get(name)).to_result_with(lambda: Fault.of(Reason.ABSENT, repr(bpy.data.scenes), member=name))


def ranged(scene: bpy.types.Scene) -> Frames:
    """Returns inclusive frame range and step of scene."""
    return Frames(start=scene.frame_start, end=scene.frame_end, step=scene.frame_step)


# --- [CHECKS]


def cameras(scene: bpy.types.Scene, scope: JobScope, frames: Frames) -> Outcome[None]:
    """Accumulates frames without camera and missing or non-camera bindings."""
    match scope.camera:
        case MarkedCamera(camera=camera, markers=markers) as bound:
            gaps = tuple(next(run) for missing, run in groupby(frames.numbers, key=lambda frame: bound.at(frame).is_none()) if missing)
            bindings = (*(("scene camera", name) for name in Option.of_optional(camera).to_list()), *((f"marker at frame {marker.frame}", marker.camera) for marker in markers))
        case FixedCamera(camera=camera):
            gaps, bindings = (), (("fixed camera", camera),)

    def checked(binding: str, name: str) -> Outcome[bpy.types.Object]:
        found: Outcome[bpy.types.Object] = Option.of_optional(bpy.data.objects.get(name)).to_result_with(lambda: Fault.of(Reason.ABSENT, repr(bpy.data.objects), member=name))
        return found.bind(lambda bound: typed(bpy.types.Camera, binding, bound.data).map(lambda _: bound))

    return accumulated((*(Error(Fault.of(Reason.ABSENT, repr(scene), member=str(frame))) for frame in gaps), *starmap(checked, bindings))).map(lambda _: None)


def templated(path: OutputPath, scope: JobScope, scene: bpy.types.Scene, frames: Frames) -> Outcome[None]:
    """Accumulates output path faults and missing layer, pass, or frame template fields."""
    layers = Option.of_optional(scope.layer).map(lambda layer: (layer,)).default_with(lambda: tuple(layer.name for layer in used(scene)))
    keys = path.name.keys
    varied = (
        *((Word.LAYER,) if len(layers) > 1 and Word.LAYER not in keys else ()),
        *((Word.PASS,) if not keys & {Word.PASS, Word.OUTPUT} else ()),
        *((NumberField.FRAME,) if len(frames.numbers) > 1 and NumberField.FRAME not in keys else ()),
    )
    return paired(
        accumulated(path.written(layer, output, Nothing, frames.start, "") for layer in layers for output in Output),
        accumulated(Error(Fault.of(Reason.REQUIRED, f"{{{field}}} in output template to name one file per {field}")) for field in varied),
    ).map(lambda _: None)


def checked(pathed: Pathed, scopes: tuple[JobScope, ...], day: datetime.date) -> Outcome[None]:
    """Audits the file, then every scope's cameras, output path, and named view layer."""

    def audited_scope(scope: JobScope, scene: bpy.types.Scene) -> Outcome[None]:
        frames = ranged(scene)
        return paired(
            cameras(scene, scope, frames),
            pathed.path(scene, day).bind(partial(templated, scope=scope, scene=scene, frames=frames)),
            Option.of_optional(scope.layer).map(lambda name: layer_of(scene, name).map(lambda _: None)).default_value(Ok(None)),
        ).map(lambda _: None)

    return paired(audited(), accumulated(scene_of(scope.scene).bind(partial(audited_scope, scope)) for scope in scopes)).map(lambda _: None)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "QUEUES",
    "Dispatch",
    "Dispatcher",
    "JobScope",
    "Observed",
    "Plan",
    "Preparing",
    "QueueState",
    "Queued",
    "Queueing",
    "Reach",
    "Sent",
    "cameras",
    "checked",
    "ranged",
    "scene_of",
    "templated",
]
