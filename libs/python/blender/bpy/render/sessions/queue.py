"""Render queue service: dispatch and run commands, the background commands a queue run starts, packs, Manager setup, and the queue's sections."""

from collections.abc import Callable, Generator
from contextlib import contextmanager, ExitStack
import datetime
from functools import cached_property, partial, update_wrapper
import importlib.resources
from pathlib import Path
import shutil
from typing import ClassVar, Final, TYPE_CHECKING

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.calls import called
from blender.bpy.commands.invocations import Popup
from blender.bpy.commands.operators import Call, Done, Empty, Operator
from blender.bpy.data.rna.conditions import Setting
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.data.rna.paths import rooted
from blender.bpy.data.rna.snapshots import assigned
from blender.bpy.extension.events import Handler, Timer
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, CliCommand, Declaration
from blender.bpy.faults import BlenderReason
from blender.bpy.files.sessions import Pack, packed, saved
from blender.bpy.render.outputs.framing import FrameSize
from blender.bpy.render.outputs.paths import ScenePaths
from blender.bpy.render.sessions.jobs import prepared
from blender.bpy.render.sessions.plans import checked, Dispatch, Dispatcher, Observed, Plan, Preparing, Queued, Queueing, QUEUES, ranged, scene_of, Sent
from blender.bpy.render.sessions.preflight import Preflight
from blender.bpy.render.sessions.renders import native, Render
from blender.bpy.render.sessions.runs import commanded, kept, restored, supervised, watched
from blender.bpy.render.sessions.sets import RenderSet
from blender.bpy.render.view_layers.routing import PassSet
from blender.bpy.settings.texts import TextRecord
from blender.bpy.ui.layout import rows
from blender.bpy.ui.layout.sections import Section
from blender.bpy.ui.screens.areas import AreaQuery, EditorState
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired, RecordReason
from blender.records.identity import content_hash
from blender.records.stores import RecordFile
from blender.rendering.batch import supervision
from blender.rendering.batch.flamenco import Flamenco, Manager
from blender.rendering.batch.notices import Cancelled, Test
from blender.rendering.batch.power import cancelled, PowerHold
from blender.rendering.batch.processes import detached, HostCommands, RunCommand, RunFile
from blender.rendering.batch.progress import Ending, Heartbeat, Liveness, Progress
from blender.rendering.batch.queue import RenderFrames, RenderQueue
from blender.rendering.batch.supervision import LocalMachine, Run

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Step = Callable[[Dispatcher, Call[Empty]], Outcome[Done]]

# --- [CONSTANTS] ------------------------------------------------------------------------

_PAUSABLE: Final = Setting(descriptor=Descriptor.declared(bpy.types.SpaceView3D, ("region_3d", "support_pause_render"), bpy.types.BoolProperty))

# --- [SERVICES] -------------------------------------------------------------------------


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
            form: Dispatch = properties  # type: ignore[assignment]  # ty: ignore[invalid-assignment]

            def summary(plan: Plan, queueing: Queueing) -> Outcome[tuple[str, str]]:
                return accumulated(scene_of(scope.scene).map(lambda scene: len(ranged(scene).numbers)) for scope in plan.scopes).map(
                    lambda counts: (type(queueing.target).__name__, f"{len(plan.scopes)} jobs, {sum(counts)} frames")
                )

            lines = (
                self
                .bound(identity)
                .bind(lambda dispatcher: paired(dispatcher.planned(context, form, datetime.datetime.now(datetime.UTC)), dispatcher.settings(identity)))
                .bind(lambda pair: summary(*pair))
                .default_with(lambda faults: tuple(fault.message for fault in faults))
            )
            for line in lines:
                layout.label(text=line)
            layout.prop(properties, "reach")

        return Operator[Dispatch](
            icon=Some("RENDER_ANIMATION"),
            condition=Some(Preflight.condition),
            form=Dispatch,
            run=update_wrapper(partial(self.stepped, dispatch), dispatch),
            undo=Nothing,
            invoke=Some(Popup(partial(bpy.types.WindowManager.invoke_props_dialog))),
            draw=Some(drawn),
        )

    @cached_property
    def controls(self) -> tuple[Operator[Empty], ...]:
        """Run control and packing commands in section order."""
        return self.stepping(resume, "PLAY"), self.stepping(stop, "CANCEL"), self.stepping(open_log, "TEXT"), self.stepping(cancel_shutdown, "QUIT"), self.stepping(pack, "PACKAGE")

    @cached_property
    def setup(self) -> tuple[Operator[Empty], ...]:
        """Manager reads, job type install, and notice test in preferences order."""
        return self.stepping(read_manager, "FILE_REFRESH"), self.stepping(install_job_type, "IMPORT"), self.stepping(post_test_notice, "INFO")

    def stepping(self, step: Step, icon: "rna_enums.IconItems") -> Operator[Empty]:
        """Command running a dispatcher step, keyed and described by the step."""
        return Operator(icon=Some(icon), undo=Nothing, form=Empty, run=update_wrapper(partial(self.stepped, step), step))

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
            lambda names: HostCommands(binary=bpy.app.binary_path, folder=str(identity.folder), commands=dict(zip(RunCommand, names)))
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
            CliCommand(key=RunCommand.PREPARE, decoder=msgspec.json.Decoder(Preparing), run=lambda arguments, trailing: prepared(dispatcher, arguments, trailing).map(lambda _: 0)),
            CliCommand(key=RunCommand.SUPERVISE, decoder=msgspec.json.Decoder(Run), run=lambda arguments, trailing: supervised(dispatcher, arguments, trailing).map(lambda _: 0)),
            CliCommand(
                key=RunCommand.RENDER, decoder=msgspec.json.Decoder[Run | RenderFrames](Run | RenderFrames), run=lambda arguments, trailing: commanded(dispatcher, arguments, trailing).map(lambda _: 0)
            ),
        )

    # --- [SECTIONS]

    def progressed(self, identity: Identity, _context: bpy.types.Context) -> tuple[rows.Keywords, ...]:
        """Progress bar keywords per run from the extension's cached observation alone, its text and fraction of items done."""
        now = datetime.datetime.now(datetime.UTC)

        def local(sent: Sent, progress: Progress, beat: datetime.datetime, heartbeat: Heartbeat) -> tuple[str, float]:
            remaining, phase = progress.remaining(sent.queue), progress.phase(beat, now, heartbeat)
            finish = progress.forecast(sent.queue, now).bind(lambda forecast: forecast.finish).map(lambda moment: f"finishing {moment.astimezone():%H:%M}").to_list()
            return " · ".join((*(sent.queue.jobs[part].title for part, _ in remaining[:1]), bpy.path.display_name(phase.name.lower()), *finish)), float(progress.done(sent.queue))

        def work(held: Observed, heartbeat: Heartbeat) -> tuple[str, float]:
            return held.sent.map(
                lambda sent: held.progress.map(lambda pair: local(sent, *pair, heartbeat)).default_with(
                    lambda: (" ".join((type(sent.target).__name__, *sent.issued)), 0.0 if isinstance(sent.target, LocalMachine) else 1.0)
                )
            ).default_with(lambda: (bpy.app.translations.pgettext_iface("Preparing"), 0.0))

        runs = (
            self
            .settings(identity)
            .map(lambda queueing: tuple(work(held, queueing.heartbeat) for held in identity.held[QUEUES].observed.to_list()))
            .default_with(lambda faults: tuple((fault.message, 0.0) for fault in faults))
        )
        return tuple({"text": text, "factor": done} for text, done in runs)

    @property
    def section(self) -> Section:
        """Queue section: dispatch, run progress read from session state at draw, status, and run controls."""
        return Section(
            key=self.key,
            title=self.title,
            rows=(
                rows.OperatorButton(target=self.dispatching),
                rows.Layout(rows=lambda identity: Ok((rows.Label(text=partial(self.progressed, identity), function=rows.FUNCTIONS["progress"]),))),
                rows.Status(),
                rows.Layout(rows=tuple(rows.OperatorButton(target=command, keywords={"text": ""}) for command in self.controls), container=Some(lambda layout: layout.row(align=True))),
            ),
        )

    @property
    def preferences(self) -> tuple[Section, ...]:
        """Preferences section of Manager reads, job type install, and notice test."""
        return (
            Section(
                key=f"{self.key}_setup",
                title=self.title,
                rows=(rows.Layout(rows=tuple(rows.OperatorButton(target=command) for command in self.setup), container=Some(lambda layout: layout.row(align=True))),),
            ),
        )

    # --- [REGISTRATION]

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[Outcome[Acquisitions]]:
        """Registers the queue commands, observing a run dispatched before registration or a file load, and restores paused viewports at scope exit."""
        with ExitStack() as scope:

            def joined(dispatcher: Dispatcher) -> Outcome[Acquisitions]:
                scope.callback(identity.reports.reported(partial(restored, dispatcher)))
                return Ok((
                    QUEUES,
                    *self.cli(dispatcher),
                    self.dispatching,
                    *self.controls,
                    *self.setup,
                    Timer(body=partial(_started, dispatcher)),
                    Handler(handlers=bpy.app.handlers.load_post, body=lambda: _started(dispatcher).map(lambda _: None)),
                ))

            yield self.bound(identity).bind(joined)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [DISPATCH]


def dispatch(dispatcher: Dispatcher, call: Call[Dispatch]) -> Outcome[Done]:
    """Plans jobs, writes their files in a background process, and sends the queue to its target."""
    context, identity, moment = call.context, call.identity, datetime.datetime.now(datetime.UTC)
    day = moment.astimezone().date()

    def gated(plan: Plan, queueing: Queueing) -> Outcome[None]:
        return paired(checked(dispatcher.pathed, plan.scopes, day), Dispatcher.connected() if isinstance(queueing.target, Flamenco) else Ok(None)).map(lambda _: None)

    def paused(manager: bpy.types.WindowManager) -> Outcome[None]:
        def applied(state: EditorState) -> Outcome[None]:
            return accumulated(state.applied(at.area).map(lambda prior: (at, prior)) for at in AreaQuery(space=bpy.types.SpaceView3D, shows=Some(_PAUSABLE)).located(manager.windows)).map(
                dispatcher.state.pausing
            )

        return EditorState.of(bpy.types.SpaceView3D, {("region_3d", "pause_render"): True}).bind(applied)

    def written(plan: Plan, queueing: Queueing, manager: bpy.types.WindowManager) -> Outcome[Done]:
        stem = content_hash(Plan, plan).hex()
        return (
            accumulated(scene_of(scope.scene) for scope in plan.scopes)
            .bind(lambda scenes: accumulated(dispatcher.pathed.took(scene, day) for scene in dict.fromkeys(scenes)))
            .bind(lambda _: RecordFile(path=dispatcher.host.path(stem, RunFile.PLAN), form=Plan).written(plan))
            .bind(lambda _: assigned(dispatcher.queued.struct(manager), {("plan",): stem}))
            .bind(lambda _: saved(Path(bpy.data.filepath), bpy.ops.wm.save_mainfile))
            .bind(lambda source: detached(dispatcher.host.argv(RunCommand.PREPARE, Preparing(plan=stem, target=queueing.target), Some(str(source))), dispatcher.host.path(stem, RunFile.PLAN_LOG)))
            .map(lambda log: Done(notes=(("INFO", f"{len(plan.scopes)} jobs preparing, log at {log}"),), skipped=paused(manager).swap().to_option()))
        )

    return (
        paired(dispatcher.planned(context, call.properties, moment), dispatcher.settings(identity), rooted(bpy.types.WindowManager, context))
        .bind(lambda parts: gated(parts[0], parts[1]).map(lambda _: parts))
        .bind(lambda parts: written(*parts))
        .map(partial(_observing, dispatcher))
    )


# --- [RUN_CONTROL]


def resume(dispatcher: Dispatcher, call: Call[Empty]) -> Outcome[Done]:
    """Renders the remaining frames of a stopped, failed, or canceled queue."""

    def resent(queue: RenderQueue, target: LocalMachine, progress: Progress, beat: datetime.datetime, queueing: Queueing) -> Outcome[Done]:
        phase = progress.phase(beat, datetime.datetime.now(datetime.UTC), queueing.heartbeat)
        resumable = Liveness.STOPPED, Ending.FAILED, Ending.CANCELLED
        return (
            target.sent(queue, dispatcher.host).map(
                lambda issued: Done(notes=tuple(("INFO", value) for answer in issued for value in answer.to_option().to_list()), skipped=accumulated(issued).swap().to_option())
            )
            if phase in resumable
            else Error(Fault.of(RecordReason.OUTSIDE, str(dispatcher.host.path(queue.stem, RunFile.PROGRESS)), str(phase.name), member=", ".join(value.name for value in resumable)))
        )

    return paired(kept(dispatcher), dispatcher.settings(call.identity)).bind(lambda pair: resent(*pair[0], pair[1])).map(partial(_observing, dispatcher))


def stop(dispatcher: Dispatcher, call: Call[Empty]) -> Outcome[Done]:
    """Stops the queue and its renders."""

    def halted(queue: RenderQueue, progress: Progress, queueing: Queueing) -> Outcome[Done]:
        def ended(_: None) -> Outcome[Done]:
            persisted = RecordFile(path=dispatcher.host.path(queue.stem, RunFile.PROGRESS), form=Progress).revised(lambda held: msgspec.structs.replace(held, ending=Ending.CANCELLED))
            return paired(persisted, dispatcher.posted(queueing.listener, Cancelled(frames=len(progress.marks)))).map(lambda _: Done(notes=(("INFO", f"Stopped {queue.stem}"),)))

        stopped: Outcome[None] = supervision.stop(progress)
        return Result[None, Faults].bind(stopped, ended)

    return paired(kept(dispatcher), dispatcher.settings(call.identity)).bind(lambda pair: halted(pair[0][0], pair[0][2], pair[1]))


def open_log(dispatcher: Dispatcher, call: Call[Empty]) -> Outcome[Done]:
    """Opens the log of the last sent queue, else of the last plan."""

    def opened(plan: str) -> Outcome[Done]:
        log = (
            dispatcher.state.observed
            .bind(lambda held: held.sent)
            .filter(lambda record: isinstance(record.target, LocalMachine))
            .map(lambda record: dispatcher.host.path(record.queue.stem, RunFile.LOG))
            .default_with(lambda: dispatcher.host.path(plan, RunFile.PLAN_LOG))
        )
        return called(bpy.ops.wm.path_open, {"filepath": str(log)}).map(lambda _: Done())

    return (
        rooted(bpy.types.WindowManager, call.context)
        .bind(lambda manager: dispatcher.queued.struct(manager).dispatched.to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(dispatcher.queued), member="plan")))
        .bind(opened)
    )


def cancel_shutdown(dispatcher: Dispatcher, call: Call[Empty]) -> Outcome[Done]:
    """Cancels the shutdown scheduled after the local queue."""

    def cancelling(progress: Progress, power: PowerHold) -> Outcome[Done]:
        return cancelled(progress).map(lambda _: Done(notes=(("INFO", "Shutdown canceled"),))) if power.shutdown else Error(Fault.of(BlenderReason.ABSENT, repr(power), member="shutdown"))

    return paired(kept(dispatcher), dispatcher.settings(call.identity)).bind(lambda pair: cancelling(pair[0][2], pair[1].power))


# --- [OBSERVATION]


def _observing(dispatcher: Dispatcher, done: Done) -> Done:
    """Starts observing the run a step just dispatched or resumed, passing its answer on."""
    watched(dispatcher)
    return done


def _started(dispatcher: Dispatcher) -> Outcome[Option[float]]:
    """Starts observing a run the open file dispatched, as a one-shot timer body, the observation timer stopping at once when none is pending."""
    watched(dispatcher)
    return Ok(Nothing)


# --- [SETUP]


def pack(dispatcher: Dispatcher, call: Call[Empty]) -> Outcome[Done]:
    """Writes a packed file copy under the render output folder."""
    moment = datetime.datetime.now(datetime.UTC).astimezone()

    def finished(pack: Outcome[Pack]) -> Outcome[None]:
        return pack.bind(lambda held: accumulated(Error((found,)) for found in held.skipped).map(lambda _: None))

    def started(target: Path) -> Outcome[Done]:
        return packed(call.identity, target, project=Path(bpy.data.filepath).parent, finished=finished).map(lambda _: Done(notes=(("INFO", f"Packing into {target}"),)))

    return (
        rooted(bpy.types.Scene, call.context)
        .bind(lambda scene: dispatcher.pathed.path(scene, moment.date()))
        .bind(lambda path: started(path.base / "packs" / f"{Path(bpy.data.filepath).stem}_{moment:%Y%m%d-%H%M%S}"))
    )


def read_manager(dispatcher: Dispatcher, call: Call[Empty]) -> Outcome[Done]:
    """Reads Manager storage, worker tags, and job types, reporting each worker tag."""

    def flamenco(queueing: Queueing) -> Outcome[Flamenco]:
        match queueing.target:
            case Flamenco() as target:
                return Ok(target)
            case other:
                return Error(Fault.of(RecordReason.OUTSIDE, repr(queueing), type(other).__name__, member=Flamenco.__name__))

    def noted(manager: Manager) -> Done:
        return Done(notes=(("INFO", f"Job type {'installed' if manager.job_type.is_some() else 'missing'}"), *(("INFO", f"Worker tag {tag.name}") for tag in manager.tags)))

    return dispatcher.settings(call.identity).bind(flamenco).bind(lambda target: Dispatcher.connected().bind(lambda _: target.server.opened(Manager.queried))).map(noted)


def install_job_type(dispatcher: Dispatcher, call: Call[Empty]) -> Outcome[Done]:
    """Copies the job type script into the Manager scripts folder."""

    def copied(folder: str) -> Outcome[Done]:
        @catch(exception=OSError)
        def written() -> Path:
            with importlib.resources.as_file(importlib.resources.files(Flamenco.__module__).joinpath(Flamenco.script)) as script:
                return Path(shutil.copy(script, Path(folder) / Flamenco.script))

        return written().map_error(partial(Fault.of, RecordReason.UNWRITABLE, folder)).map(lambda path: Done(notes=(("INFO", str(path)),)))

    return dispatcher.settings(call.identity).bind(
        lambda queueing: queueing.scripts.map(copied).default_with(lambda: Error(Fault.of(BlenderReason.ABSENT, repr(call.context.preferences), member="scripts")))
    )


def post_test_notice(dispatcher: Dispatcher, call: Call[Empty]) -> Outcome[Done]:
    """Posts a test notice to the listener topic."""
    return dispatcher.settings(call.identity).bind(lambda queueing: dispatcher.posted(queueing.listener, Test())).map(lambda _: Done(notes=(("INFO", "Notice posted"),)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["QueueRun", "Step", "cancel_shutdown", "dispatch", "install_job_type", "open_log", "pack", "post_test_notice", "read_manager", "resume", "stop"]
