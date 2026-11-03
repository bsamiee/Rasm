"""Render queue service: dispatch and run commands, the background commands a queue run starts, packs, Manager setup, and the queue's sections."""

from collections.abc import Callable, Generator
from contextlib import contextmanager, ExitStack
import datetime
from functools import cached_property, partial
import importlib.resources
from pathlib import Path
import shutil
from typing import ClassVar, Final, TYPE_CHECKING

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.invocations import Popup
from blender.bpy.commands.operators import BoundBody, Call, called, confirmed, Done, Empty, NoUndo, Operator
from blender.bpy.data.rna.conditions import Setting
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.data.rna.paths import rooted
from blender.bpy.data.rna.snapshots import assigned
from blender.bpy.extension.events import Handler, Timer
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, CliCommand, Declaration
from blender.bpy.files.sessions import Pack, packed, saved
from blender.bpy.render.outputs.framing import FrameSize
from blender.bpy.render.outputs.results import RENDER_ORIGINS
from blender.bpy.render.sessions.plans import checked, Dispatch, Dispatcher, LocalRun, Observed, Plan, PrepareArguments, Queued, QueuePreferences, QUEUES, ranged, scene_of, Sent
from blender.bpy.render.sessions.preflight import Preflight
from blender.bpy.render.sessions.renders import native, Render
from blender.bpy.render.sessions.runs import commanded, prepared, supervised
from blender.bpy.render.sessions.sets import RenderSet
from blender.bpy.render.view_layers.routing import RoutingRow
from blender.bpy.settings.attachments import TextRecord
from blender.bpy.ui.elements.panels import Preference
from blender.bpy.ui.layout import rows
from blender.bpy.ui.layout.sections import Section
from blender.bpy.ui.screens.areas import AreaQuery, EditorState
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired, Reason
from blender.records.stores import content_hash, RecordFile
from blender.rendering.batch import supervision
from blender.rendering.batch.notices import Cancelled, Test
from blender.rendering.batch.queue import Ending, Liveness, Progress, RenderFrames
from blender.rendering.batch.supervision import detached, HostCommands, LocalMachine, Run, RunCommand, RunFile, shutdown_cancelled
from blender.rendering.batch.targets import Flamenco, Manager

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [CONSTANTS] ------------------------------------------------------------------------

_PAUSABLE: Final = Setting(descriptor=Descriptor.declared(bpy.types.SpaceView3D, ("region_3d", "support_pause_render"), bpy.types.BoolProperty))

# --- [SERVICES] -------------------------------------------------------------------------


class QueueRun[C: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Render queue commands, status timer, section with its progress row, and the preference its setup commands sit beside, each frame running through `render` and each plan revised through `planning`."""

    sets: RenderSet[C]
    render: Render = native
    planning: Option[Callable[[Identity, Plan], Outcome[Plan]]] = Nothing
    record: ClassVar[TextRecord[Queued]] = TextRecord(key="queue", form=Queued)
    key: ClassVar[Key] = "render_queue"

    # --- [COMMANDS]

    @cached_property
    def dispatching(self) -> Operator[Dispatch]:
        """Dispatch command binding its dispatcher on call, its dialog summarizing the plan above the scope."""

        def summary(dispatcher: Dispatcher, context: bpy.types.Context, properties: Dispatch) -> Outcome[tuple[str, ...]]:
            def counted(plan: Plan, preferences: QueuePreferences) -> Outcome[tuple[str, ...]]:
                return accumulated(scene_of(scope.scene).map(lambda scene: len(ranged(scene).numbers)) for scope in plan.scopes).map(
                    lambda counts: (type(preferences.target).__name__, f"{len(plan.scopes)} jobs, {sum(counts)} frames")
                )

            return paired(dispatcher.planned(context, properties, datetime.datetime.now(datetime.UTC)), dispatcher.preference.read(dispatcher.identity)).bind(lambda pair: counted(*pair))

        return Operator.of(
            Dispatch,
            icon=Some("RENDER_ANIMATION"),
            condition=Some(Preflight.condition),
            undo=NoUndo(),
            invoke=Some(Popup(partial(bpy.types.WindowManager.invoke_props_dialog))),
            draw=Some(partial(confirmed, self.bound, summary, ("scope",))),
        )(BoundBody(self.bound, dispatch))

    @cached_property
    def controls(self) -> tuple[Operator[Empty], ...]:
        """Run control and packing commands in section order."""
        steps: tuple[tuple[Callable[[Dispatcher, Call[Empty]], Outcome[Done]], rna_enums.IconItems], ...] = (
            (resume, "PLAY"),
            (stop, "CANCEL"),
            (open_log, "TEXT"),
            (cancel_shutdown, "QUIT"),
            (pack, "PACKAGE"),
        )
        return tuple(Operator.of(Empty, icon=Some(icon), undo=NoUndo())(BoundBody(self.bound, step)) for step, icon in steps)

    @cached_property
    def setup(self) -> tuple[Operator[Empty], ...]:
        """Manager reads, job type install, and notice test in preferences order."""
        steps: tuple[tuple[Callable[[Dispatcher, Call[Empty]], Outcome[Done]], rna_enums.IconItems], ...] = ((read_manager, "FILE_REFRESH"), (install_job_type, "IMPORT"), (post_test_notice, "INFO"))
        return tuple(Operator.of(Empty, icon=Some(icon), undo=NoUndo())(BoundBody(self.bound, step)) for step, icon in steps)

    # --- [BINDING]

    @property
    def prerequisites(self) -> tuple[Declaration, ...]:
        """Queue store, session state, and render productions with render set, frame size, and routing bindings read."""
        return (self.record, QUEUES, RENDER_ORIGINS, *self.sets.prerequisites, FrameSize.attachment, *RoutingRow.prerequisites)

    def bound(self, identity: Identity) -> Outcome[Dispatcher]:
        """Binds dispatcher to identity stores, render sets, routing, render, planning, and queue preference."""
        host = accumulated(identity.identifier(command) for command in RunCommand).map(
            lambda names: HostCommands(binary=bpy.app.binary_path, folder=str(identity.folder), commands=dict(zip(RunCommand, names)))
        )
        return paired(host, self.sets.bound(identity), FrameSize().bound(identity), RoutingRow.bound(identity), QueueRun.record.attachment.bound(identity)).map(
            lambda parts: Dispatcher(identity, *parts, self.render, self.planning, self.preference)
        )

    @staticmethod
    def cli(dispatcher: Dispatcher) -> tuple[CliCommand[PrepareArguments], CliCommand[Run], CliCommand[Run | RenderFrames]]:
        """Returns `prepare`, `supervise`, and `render` command rows."""
        return (
            CliCommand(key=RunCommand.PREPARE, decoder=msgspec.json.Decoder(PrepareArguments), run=lambda arguments, trailing: prepared(dispatcher, arguments, trailing).map(lambda _: 0)),
            CliCommand(key=RunCommand.SUPERVISE, decoder=msgspec.json.Decoder(Run), run=lambda arguments, trailing: supervised(dispatcher, arguments, trailing).map(lambda _: 0)),
            CliCommand(
                key=RunCommand.RENDER, decoder=msgspec.json.Decoder[Run | RenderFrames](Run | RenderFrames), run=lambda arguments, trailing: commanded(dispatcher, arguments, trailing).map(lambda _: 0)
            ),
        )

    # --- [SECTIONS]

    def progressed(self, identity: Identity, _context: bpy.types.Context) -> tuple[rows.Keywords, ...]:
        """Progress bar keywords per run from the extension's cached observation alone, its text and fraction of items done."""
        now = datetime.datetime.now(datetime.UTC)

        def local(sent: Sent, progress: Progress, beat: datetime.datetime, stale: datetime.timedelta) -> tuple[str, float]:
            remaining, phase = progress.remaining(sent.queue), progress.phase(beat, now, stale)
            finish = progress.forecast(sent.queue, now).bind(lambda forecast: forecast.finish).map(lambda moment: f"finishing {moment.astimezone():%H:%M}").to_list()
            return " · ".join((*(sent.queue.jobs[part].title for part, _ in remaining[:1]), bpy.path.display_name(phase.name.lower()), *finish)), float(progress.done(sent.queue))

        def work(held: Observed, stale: datetime.timedelta) -> tuple[str, float]:
            return held.sent.map(
                lambda sent: held.progress.map(lambda pair: local(sent, *pair, stale)).default_with(
                    lambda: (" ".join((type(sent.target).__name__, *sent.issued)), 0.0 if isinstance(sent.target, LocalMachine) else 1.0)
                )
            ).default_with(lambda: (" ".join(held.refused) or bpy.app.translations.pgettext_iface("Preparing"), 0.0))

        runs = (
            self.preference
            .read(identity)
            .map(lambda preferences: tuple(work(held, preferences.stale) for held in identity.resources[QUEUES].observed.to_list()))
            .default_with(lambda faults: tuple((fault.message, 0.0) for fault in faults))
        )
        return tuple({"text": text, "factor": done} for text, done in runs)

    @property
    def section(self) -> Section:
        """Queue section: dispatch, run progress read from session state at draw, status, and run controls, the queue registering what the progress reads."""
        return Section(
            key=self.key,
            rows=(
                rows.OperatorButton(target=self.dispatching),
                rows.Layout(rows=lambda identity: Ok((rows.Label(text=partial(self.progressed, identity), function=rows.FUNCTIONS["progress"]),)), prerequisites=(self,)),
                rows.Status(),
                rows.Layout(rows=tuple(rows.OperatorButton(target=command, keywords={"text": ""}) for command in self.controls), container=Some(rows.aligned)),
            ),
        )

    @cached_property
    def preference(self) -> Preference[QueuePreferences]:
        """Queue preferences with Manager reads, job type install, and notice test after the fields."""
        return Preference(key=self.key, form=QueuePreferences, rows=(rows.Layout(rows=tuple(rows.OperatorButton(target=command) for command in self.setup), container=Some(rows.aligned)),))

    # --- [REGISTRATION]

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[Outcome[Acquisitions]]:
        """Registers the queue commands, observing a run dispatched before registration or a file load, and restores paused viewports at scope exit."""
        with ExitStack() as scope:

            def joined(dispatcher: Dispatcher) -> Outcome[Acquisitions]:
                scope.callback(identity.reports.reported(dispatcher.restored))
                return Ok((
                    QUEUES,
                    *self.cli(dispatcher),
                    self.dispatching,
                    *self.controls,
                    *self.setup,
                    self.preference,
                    Timer(body=dispatcher.watched),
                    Handler(handlers=bpy.app.handlers.load_post, body=lambda: dispatcher.watched().map(lambda _: None)),
                ))

            yield self.bound(identity).bind(joined)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [DISPATCH]


def dispatch(dispatcher: Dispatcher, call: Call[Dispatch]) -> Outcome[Done]:
    """Plans jobs, writes their files in a background process, and sends the queue to its target."""
    context, identity, moment = call.context, call.identity, datetime.datetime.now(datetime.UTC)
    day = moment.astimezone().date()
    pathed = dispatcher.routing.pathed

    def gated(plan: Plan, preferences: QueuePreferences) -> Outcome[None]:
        return paired(checked(pathed, plan.scopes, day), Dispatcher.connected() if isinstance(preferences.target, Flamenco) else Ok(None)).map(lambda _: None)

    def paused(manager: bpy.types.WindowManager) -> Outcome[None]:
        def applied(state: EditorState) -> Outcome[None]:
            return accumulated(state.applied(at.area).map(lambda prior: (at, prior)) for at in AreaQuery(space=bpy.types.SpaceView3D, shows=Some(_PAUSABLE)).located(manager.windows)).map(
                dispatcher.state.pausing
            )

        return EditorState.of(bpy.types.SpaceView3D, {("region_3d", "pause_render"): True}).bind(applied)

    def written(plan: Plan, preferences: QueuePreferences, manager: bpy.types.WindowManager) -> Outcome[Done]:
        stem = content_hash(Plan, plan).hex()
        return (
            accumulated(scene_of(scope.scene) for scope in plan.scopes)
            .bind(lambda scenes: accumulated(pathed.took(scene, day) for scene in dict.fromkeys(scenes)))
            .bind(lambda _: RecordFile(path=dispatcher.host.path(stem, RunFile.PLAN), form=Plan).written(plan))
            .bind(lambda _: assigned(dispatcher.queued.struct(manager), {("plan",): stem}))
            .bind(lambda _: saved(Path(bpy.data.filepath), bpy.ops.wm.save_mainfile))
            .bind(
                lambda source: detached(
                    dispatcher.host.argv(RunCommand.PREPARE, PrepareArguments(plan=stem, target=preferences.target), Some(str(source))), dispatcher.host.path(stem, RunFile.PLAN_LOG)
                )
            )
            .map(dispatcher.state.started)
            .map(lambda _: dispatcher.host.path(stem, RunFile.PLAN_LOG))
            .map(lambda log: Done(notes=(("INFO", f"{len(plan.scopes)} jobs preparing, log at {log}"),), skipped=paused(manager).swap().default_value(())))
        )

    return (
        paired(dispatcher.planned(context, call.properties, moment), dispatcher.preference.read(identity), rooted(bpy.types.WindowManager, context))
        .bind(lambda parts: gated(parts[0], parts[1]).map(lambda _: parts))
        .bind(lambda parts: written(*parts))
        .bind(lambda done: dispatcher.watched().map(lambda _: done))
    )


# --- [RUN_CONTROL]


def resume(dispatcher: Dispatcher, call: Call[Empty]) -> Outcome[Done]:
    """Renders the remaining frames of a stopped, failed, or canceled queue."""

    def resent(run: LocalRun, preferences: QueuePreferences) -> Outcome[Done]:
        phase = run.progress.phase(run.beat, datetime.datetime.now(datetime.UTC), preferences.stale)
        resumable = Liveness.STOPPED, Ending.FAILED, Ending.CANCELLED
        return (
            run.target.sent(run.queue, dispatcher.host).map(
                lambda issued: Done(notes=tuple(("INFO", value) for answer in issued for value in answer.to_option().to_list()), skipped=accumulated(issued).swap().default_value(()))
            )
            if phase in resumable
            else Error(Fault.of(Reason.OUTSIDE, str(dispatcher.host.path(run.queue.stem, RunFile.PROGRESS)), str(phase.name), member=", ".join(value.name for value in resumable)))
        )

    return paired(dispatcher.local_run(), dispatcher.preference.read(call.identity)).bind(lambda pair: resent(*pair)).bind(lambda done: dispatcher.watched().map(lambda _: done))


def stop(dispatcher: Dispatcher, call: Call[Empty]) -> Outcome[Done]:
    """Stops the queue and its renders."""

    def halted(run: LocalRun, preferences: QueuePreferences) -> Outcome[Done]:
        def ended(_: None) -> Outcome[Done]:
            persisted = RecordFile(path=dispatcher.host.path(run.queue.stem, RunFile.PROGRESS), form=Progress).revised(lambda held: msgspec.structs.replace(held, ending=Ending.CANCELLED))
            return paired(persisted, dispatcher.posted(preferences.listener, Cancelled(frames=len(run.progress.marks)))).map(lambda _: Done(notes=(("INFO", f"Stopped {run.queue.stem}"),)))

        stopped: Outcome[None] = supervision.stop(run.progress)
        return Result[None, Faults].bind(stopped, ended)

    return paired(dispatcher.local_run(), dispatcher.preference.read(call.identity)).bind(lambda pair: halted(*pair))


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
        .bind(lambda manager: dispatcher.queued.struct(manager).dispatched.to_result_with(lambda: Fault.of(Reason.ABSENT, repr(dispatcher.queued), member="plan")))
        .bind(opened)
    )


def cancel_shutdown(dispatcher: Dispatcher, call: Call[Empty]) -> Outcome[Done]:
    """Cancels the shutdown scheduled after the local queue."""

    def cancelling(run: LocalRun, preferences: QueuePreferences) -> Outcome[Done]:
        return (
            shutdown_cancelled(run.progress).map(lambda _: Done(notes=(("INFO", "Shutdown canceled"),)))
            if preferences.shutdown
            else Error(Fault.of(Reason.ABSENT, repr(preferences), member="shutdown"))
        )

    return paired(dispatcher.local_run(), dispatcher.preference.read(call.identity)).bind(lambda pair: cancelling(*pair))


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
        .bind(lambda scene: dispatcher.routing.pathed.path(scene, moment.date()))
        .bind(lambda path: started(path.base / "packs" / f"{Path(bpy.data.filepath).stem}_{moment:%Y%m%d-%H%M%S}"))
    )


def read_manager(dispatcher: Dispatcher, call: Call[Empty]) -> Outcome[Done]:
    """Reads Manager storage, worker tags, and job types, reporting each worker tag."""

    def flamenco(preferences: QueuePreferences) -> Outcome[Flamenco]:
        match preferences.target:
            case Flamenco() as target:
                return Ok(target)
            case other:
                return Error(Fault.of(Reason.OUTSIDE, repr(preferences), type(other).__name__, member=Flamenco.__name__))

    def noted(manager: Manager) -> Done:
        return Done(notes=(("INFO", f"Job type {'installed' if manager.job_type.is_some() else 'missing'}"), *(("INFO", f"Worker tag {tag.name}") for tag in manager.tags)))

    return dispatcher.preference.read(call.identity).bind(flamenco).bind(lambda target: Dispatcher.connected().bind(lambda _: target.server.opened(Manager.queried))).map(noted)


def install_job_type(dispatcher: Dispatcher, call: Call[Empty]) -> Outcome[Done]:
    """Copies the job type script into the Manager scripts folder."""

    def copied(folder: str) -> Outcome[Done]:
        @catch(exception=OSError)
        def written() -> Path:
            with importlib.resources.as_file(importlib.resources.files(Flamenco.__module__).joinpath(Flamenco.script)) as script:
                return Path(shutil.copy(script, Path(folder) / Flamenco.script))

        return written().map_error(partial(Fault.of, Reason.UNWRITABLE, folder)).map(lambda path: Done(notes=(("INFO", str(path)),)))

    return dispatcher.preference.read(call.identity).bind(
        lambda preferences: preferences.scripts.map(copied).default_with(lambda: Error(Fault.of(Reason.ABSENT, repr(call.context.preferences), member="scripts")))
    )


def post_test_notice(dispatcher: Dispatcher, call: Call[Empty]) -> Outcome[Done]:
    """Posts a test notice to the listener topic."""
    return dispatcher.preference.read(call.identity).bind(lambda preferences: dispatcher.posted(preferences.listener, Test())).map(lambda _: Done(notes=(("INFO", "Notice posted"),)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["QueueRun", "cancel_shutdown", "dispatch", "install_job_type", "open_log", "pack", "post_test_notice", "read_manager", "resume", "stop"]
