"""Render queue registration, dispatch, progress, controls, and farm setup."""

from collections.abc import Callable
import datetime
from functools import cached_property, partial
import importlib.resources
from itertools import groupby
from pathlib import Path
import shutil
from typing import ClassVar, TYPE_CHECKING

import bpy
from expression import Option, Some
from expression.extra.result import catch
import msgspec

from rasm_bpy.commands.invocations import Popup
from rasm_bpy.commands.operators import BoundBody, Call, called, confirmed, Done, Empty, Operator
from rasm_bpy.data._extension.identity import Identity, Key
from rasm_bpy.data._extension.registration import Acquisitions, CliCommand, Declaration, Handler, Timer
from rasm_bpy.data._rna.paths import rooted
from rasm_bpy.data._rna.snapshots import assigned
from rasm_bpy.files._sessions import OPTIONS, Pack, pack_started, saved
from rasm_bpy.render._outputs.framing import FrameSize
from rasm_bpy.render._outputs.results import RENDER_ORIGINS
from rasm_bpy.render._sessions.plans import (
    Dispatch,
    Dispatcher,
    JobScope,
    LocalRun,
    Observed,
    Plan,
    PLAN_DECODER,
    PrepareArguments,
    Queued,
    QueuePreferences,
    QUEUES,
    ranged,
    scene_of,
    Sent,
    templated,
)
from rasm_bpy.render._sessions.preflight import file_audit, Preflight
from rasm_bpy.render._sessions.renders import native, Render
from rasm_bpy.render._sessions.runs import commanded, prepared, supervised
from rasm_bpy.render._sessions.sets import RenderSet
from rasm_bpy.render._view_layers.passes import layer_of
from rasm_bpy.render._view_layers.routing import RoutingRow
from rasm_bpy.settings._attachments import TextRecord
from rasm_bpy.ui._elements.panels import Preference
from rasm_bpy.ui._layout import rows
from rasm_bpy.ui._layout.rows import FUNCTIONS, Label
from rasm_bpy.ui._layout.sections import Section
from rasm_records import accumulated, content_hash, Error, Fault, Nothing, Ok, Outcome, paired, Reason, RecordFile, typed
from rasm_rendering import (
    Cancelled,
    Ending,
    FixedCamera,
    Flamenco,
    Liveness,
    LocalMachine,
    Manager,
    MarkedCamera,
    Program,
    Progress,
    PROGRESS_DECODER,
    RenderFrames,
    Run,
    RunCommand,
    RunFile,
    shutdown_cancelled,
    spawned,
    supervision,
    Target,
    Test,
)

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [SERVICES] -------------------------------------------------------------------------


class QueueRun[C: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Render queue commands, status timer, section with its progress row, and the preference its setup commands sit beside, each frame running through `render` and each plan revised through `planning`."""

    # --- [STATE]

    sets: RenderSet[C]
    render: Render = native
    planning: Option[Callable[[Identity, Plan], Outcome[Plan]]] = Nothing
    preparing: Option[Callable[[Identity, Plan], Outcome[Plan]]] = Nothing
    record: ClassVar[TextRecord[Queued]] = TextRecord(key="queue", form=Queued)
    key: ClassVar[Key] = "render_queue"

    # --- [COMMANDS]

    @cached_property
    def dispatching(self) -> Operator[Dispatch]:
        """Dispatch command binding its dispatcher on call, its dialog summarizing the plan above the scope."""

        def summary(dispatcher: Dispatcher, context: bpy.types.Context, properties: Dispatch) -> Outcome[tuple[str, ...]]:
            def counted(plan: Plan, preferences: QueuePreferences) -> Outcome[tuple[str, ...]]:
                return paired(accumulated(scene_of(scope.scene).map(lambda scene: len(ranged(scene).numbers)) for scope in plan.scopes), preferences.target).map(
                    lambda found: (type(found[1]).__name__, f"{len(plan.scopes)} jobs, {sum(found[0])} frames")
                )

            return paired(dispatcher.planned(context, properties, datetime.datetime.now(datetime.UTC), revising=dispatcher.planning), dispatcher.preference.read(dispatcher.identity)).bind(
                lambda pair: counted(*pair)
            )

        return Operator.of(
            Dispatch,
            icon=Some("RENDER_ANIMATION"),
            condition=Some(Preflight.condition),
            options=frozenset(),
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
        return tuple(Operator.of(Empty, icon=Some(icon), options=frozenset())(BoundBody(self.bound, step)) for step, icon in steps)

    @cached_property
    def setup(self) -> tuple[Operator[Empty], ...]:
        """Manager reads, job type install, and notice test in preferences order."""
        steps: tuple[tuple[Callable[[Dispatcher, Call[Empty]], Outcome[Done]], rna_enums.IconItems], ...] = ((read_manager, "FILE_REFRESH"), (install_job_type, "IMPORT"), (post_test_notice, "INFO"))
        return tuple(Operator.of(Empty, icon=Some(icon), options=frozenset())(BoundBody(self.bound, step)) for step, icon in steps)

    # --- [BINDING]

    def bound(self, identity: Identity) -> Outcome[Dispatcher]:
        """Binds dispatcher to identity stores, render sets, routing, render, planning, and queue preference."""
        return paired(self.sets.bound(identity), FrameSize().bound(identity), RoutingRow.bound(identity), QueueRun.record.attachment.bound(identity)).map(
            lambda parts: Dispatcher(identity, QUEUES[identity], *parts, self.render, self.planning, self.preparing.or_else(self.planning), self.preference)
        )

    @staticmethod
    def cli(dispatcher: Dispatcher) -> tuple[CliCommand[PrepareArguments], CliCommand[Run | Program], CliCommand[Run | RenderFrames]]:
        """Returns `prepare`, `supervise`, and `render` command rows."""
        return (
            CliCommand(key=RunCommand.PREPARE, decoder=msgspec.json.Decoder(PrepareArguments), run=partial(prepared, dispatcher)),
            CliCommand(key=RunCommand.SUPERVISE, decoder=msgspec.json.Decoder[Run | Program](Run | Program), run=partial(supervised, dispatcher)),
            CliCommand(key=RunCommand.RENDER, decoder=msgspec.json.Decoder[Run | RenderFrames](Run | RenderFrames), run=partial(commanded, dispatcher)),
        )

    # --- [SECTIONS]

    def progressed(self, identity: Identity, _context: bpy.types.Context) -> tuple[rows.Keywords, ...]:
        """Progress bar keywords per run from the extension's cached observation alone, its text and fraction of items done."""
        now = datetime.datetime.now(datetime.UTC)

        def local(sent: Sent, progress: Progress, beat: datetime.datetime, stale: datetime.timedelta) -> tuple[str, float]:
            remaining, phase = progress.remaining(sent.queue), progress.phase(beat, now, stale)
            finish = progress.forecast(sent.queue, now).bind(lambda forecast: forecast.finish).map(lambda moment: f"finishing {moment.astimezone():%H:%M}").to_list()
            return " · ".join((*(sent.queue.jobs[part].title for part, _ in remaining[:1]), bpy.path.display_name(phase.name.lower()), *finish)), float(progress.done(sent.queue))

        def work(held: Observed, stale: datetime.timedelta) -> tuple[tuple[str, float], ...]:
            return held.sent.map(
                lambda sent: (
                    held.progress.map(lambda pair: local(sent, *pair, stale)).default_with(
                        lambda: (" ".join((type(sent.target).__name__, *sent.issued)), 0.0 if isinstance(sent.target, LocalMachine) else 1.0)
                    ),
                )
            ).default_with(lambda: () if held.refused else ((bpy.app.translations.pgettext_iface("Preparing"), 0.0),))

        runs = (
            identity.reports
            .settled(self.preference.read(identity))
            .map(lambda preferences: tuple(row for held in QUEUES[identity].observed.to_list() for row in work(held, preferences.stale)))
            .default_value(())
        )
        return tuple({"text": text, "factor": done} for text, done in runs)

    @property
    def section(self) -> Section:
        """Queue section: dispatch, run progress read from session state at draw, and run controls, the queue registering what the progress reads."""
        return Section(
            key=self.key,
            rows=(
                rows.OperatorButton(target=self.dispatching),
                rows.Layout(builder=Some(lambda identity: Ok((Label(text=partial(self.progressed, identity), function=FUNCTIONS["progress"]),))), prerequisites=(self,)),
                rows.Layout(rows=tuple(rows.OperatorButton(target=command, keywords={"text": ""}) for command in self.controls), container=Some(rows.aligned)),
            ),
        )

    @cached_property
    def preference(self) -> Preference[QueuePreferences]:
        """Queue preferences with Manager reads, job type install, and notice test after the fields."""
        return Preference(key=self.key, form=QueuePreferences, rows=(rows.Layout(rows=tuple(rows.OperatorButton(target=command) for command in self.setup), container=Some(rows.aligned)),))

    # --- [REGISTRATION]

    @property
    def prerequisites(self) -> tuple[Declaration, ...]:
        """Queue store, session state, and render productions with render set, frame size, and routing bindings read."""
        return (self.record, QUEUES, RENDER_ORIGINS, *self.sets.prerequisites, FrameSize.attachment, *RoutingRow.prerequisites)

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Registers commands and native queue observation across file loads."""
        return self.bound(identity).map(
            lambda dispatcher: (
                QUEUES,
                *self.cli(dispatcher),
                self.dispatching,
                *self.controls,
                *self.setup,
                self.preference,
                Timer(body=dispatcher.watched),
                Handler(handlers=bpy.app.handlers.load_post, body=lambda: dispatcher.watched().map(lambda _: None)),
            )
        )


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [DISPATCH]


def dispatch(dispatcher: Dispatcher, call: Call[Dispatch]) -> Outcome[Done]:
    """Audits the file, then every scope's cameras, output path, and named view layer, plans jobs, writes their files in a background process, and sends the queue to its target."""
    context, identity, moment = call.context, call.identity, datetime.datetime.now(datetime.UTC)
    day = moment.astimezone().date()
    pathed = dispatcher.routing.pathed

    def audited(scope: JobScope, scene: bpy.types.Scene) -> Outcome[None]:
        frames = ranged(scene)
        match scope.camera:
            case MarkedCamera(camera=camera, markers=markers) as bound:
                gaps = tuple(next(run) for missing, run in groupby(frames.numbers, key=lambda frame: bound.at(frame).is_none()) if missing)
                bindings = (*(("scene camera", name) for name in Option.of_optional(camera).to_list()), *((f"marker at frame {marker.frame}", marker.camera) for marker in markers))
            case FixedCamera(camera=camera):
                gaps, bindings = (), (("fixed camera", camera),)
        return paired(
            accumulated((
                *(Error(Fault.of(Reason.ABSENT, repr(scene), member=str(frame))) for frame in gaps),
                *(
                    Option
                    .of_optional(bpy.data.objects.get(name))
                    .to_result_with(partial(Fault.of, Reason.ABSENT, repr(bpy.data.objects), member=name))
                    .bind(lambda found: typed(bpy.types.Camera, binding, found.data))
                    for binding, name in bindings
                ),
            )),
            pathed.path(scene, day).bind(partial(templated, scope=scope, scene=scene, frames=frames)),
            Option.of_optional(scope.layer).map(lambda name: layer_of(scene, name).map(lambda _: None)).default_value(Ok(None)),
        ).map(lambda _: None)

    def gated(plan: Plan, preferences: QueuePreferences) -> Outcome[Target]:
        return paired(
            file_audit(),
            accumulated(scene_of(scope.scene).bind(partial(audited, scope)) for scope in plan.scopes),
            preferences.target.bind(lambda target: (Dispatcher.connected() if isinstance(target, Flamenco) else Ok(None)).map(lambda _: target)),
        ).map(lambda found: found[2])

    def written(plan: Plan, preferences: QueuePreferences, manager: bpy.types.WindowManager, target: Target) -> Outcome[Done]:
        stem = content_hash(Plan, plan).hex()
        return (
            accumulated(scene_of(scope.scene) for scope in plan.scopes)
            .bind(lambda scenes: accumulated(pathed.took(scene, day) for scene in dict.fromkeys(scenes)))
            .bind(lambda _: RecordFile(path=dispatcher.host.path(stem, RunFile.PLAN), decoder=PLAN_DECODER).written(plan))
            .bind(lambda _: dispatcher.host.pruned(lambda keys: keys[: preferences.kept]))
            .bind(lambda _: assigned(dispatcher.queued.struct(manager), {("plan",): stem}))
            .bind(lambda _: saved(Path(bpy.data.filepath), bpy.ops.wm.save_mainfile))
            .bind(lambda source: spawned(dispatcher.host.argv(RunCommand.PREPARE, PrepareArguments(plan=stem, target=target), Some(str(source))), detached=True))
            .map(partial(dispatcher.state.started, stem))
            .map(lambda _: identity.reports.logger.info("Plan sent", plan=stem, jobs=len(plan.scopes)))
            .map(lambda _: Done(notes=(("INFO", f"{len(plan.scopes)} jobs preparing, log at {identity.reports.log(identity.identifier(RunCommand.PREPARE))}"),)))
        )

    return (
        paired(dispatcher.planned(context, call.properties, moment, revising=dispatcher.preparing), dispatcher.preference.read(identity), rooted(bpy.types.WindowManager, context))
        .bind(lambda parts: gated(parts[0], parts[1]).bind(partial(written, *parts)))
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
            persisted = RecordFile(path=dispatcher.host.path(run.queue.stem, RunFile.PROGRESS), decoder=PROGRESS_DECODER).revised(lambda held: msgspec.structs.replace(held, ending=Ending.CANCELLED))
            return paired(persisted, dispatcher.posted(preferences.listener, Cancelled(frames=len(run.progress.marks)))).map(lambda _: Done(notes=(("INFO", f"Stopped {run.queue.stem}"),)))

        return supervision.stop(run.progress).bind(ended)

    return paired(dispatcher.local_run(), dispatcher.preference.read(call.identity)).bind(lambda pair: halted(*pair))


def open_log(dispatcher: Dispatcher, call: Call[Empty]) -> Outcome[Done]:
    """Opens the render log of the last sent queue, else the log of the `prepare` command its plan ran."""

    def opened(_plan: str) -> Outcome[Done]:
        log = (
            dispatcher.state.observed
            .bind(lambda held: held.sent)
            .filter(lambda record: isinstance(record.target, LocalMachine))
            .map(lambda record: dispatcher.host.path(record.queue.stem, RunFile.LOG))
            .default_value(call.identity.reports.log(call.identity.identifier(RunCommand.PREPARE)))
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
    """Starts packing the saved session file and every dependency under the render output folder, blend paths rewritten to the pack, the reports queue stepping it."""
    moment = datetime.datetime.now(datetime.UTC).astimezone()

    def finished(pack: Outcome[Pack]) -> Outcome[None]:
        return pack.bind(lambda held: accumulated(Error((found,)) for found in held.skipped).map(lambda _: None))

    def started(target: Path) -> Outcome[Done]:
        return pack_started(target, Path(bpy.data.filepath).parent, OPTIONS, finished).bind(call.identity.reports.queued).map(lambda _: Done(notes=(("INFO", f"Packing into {target}"),)))

    return (
        rooted(bpy.types.Scene, call.context)
        .bind(lambda scene: dispatcher.routing.pathed.path(scene, moment.date()))
        .bind(lambda path: started(path.base / "packs" / f"{Path(bpy.data.filepath).stem}_{moment:%Y%m%d-%H%M%S}"))
    )


def read_manager(dispatcher: Dispatcher, call: Call[Empty]) -> Outcome[Done]:
    """Reads Manager storage, worker tags, and job types, reporting each worker tag."""

    def flamenco(preferences: QueuePreferences, target: Target) -> Outcome[Flamenco]:
        match target:
            case Flamenco():
                return Ok(target)
            case other:
                return Error(Fault.of(Reason.OUTSIDE, repr(preferences), type(other).__name__, member=Flamenco.__name__))

    def noted(manager: Manager) -> Done:
        return Done(notes=(("INFO", f"Job type {'installed' if manager.job_type.is_some() else 'missing'}"), *(("INFO", f"Worker tag {tag.name}") for tag in manager.tags)))

    return (
        dispatcher.preference
        .read(call.identity)
        .bind(lambda preferences: preferences.target.bind(partial(flamenco, preferences)))
        .bind(lambda target: Dispatcher.connected().bind(lambda _: target.server.opened(Manager.queried)))
        .map(noted)
    )


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
