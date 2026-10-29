"""Render queue runs: background frame renders and supervision the queue's commands run, and the session's observation of a dispatched run while one is pending."""

from collections.abc import Callable
import datetime
from functools import partial, reduce
import os
from pathlib import Path
import time

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
import msgspec

from blender.bpy.data.rna.paths import rooted
from blender.bpy.faults import BlenderReason
from blender.bpy.render.outputs.paths import OutputPath
from blender.bpy.render.sessions.plans import bare, Dispatcher, Observed, Queueing, scene_of, Sent
from blender.bpy.render.sessions.renders import RenderCall
from blender.bpy.ui.screens.areas import Located
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired, RecordReason
from blender.records.stores import RecordFile
from blender.rendering.batch.notices import Completed, Estimated, Failed, Started
from blender.rendering.batch.power import shut
from blender.rendering.batch.processes import RunCommand, RunFile
from blender.rendering.batch.progress import Ending, observed, Progress, StageMark
from blender.rendering.batch.queue import RenderFrames, RenderQueue
from blender.rendering.batch.supervision import cleared, LocalMachine, Run, supervise

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [BACKGROUND_RENDER]


def _scoped(dispatcher: Dispatcher, scene: bpy.types.Scene) -> Outcome[OutputPath]:
    """Returns the output path a scene's job copy holds, absent fault for a scene no job wrote."""
    return dispatcher.pathed.scope.read(scene).bind(lambda found: found.to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(scene), member=dispatcher.pathed.scope.name)))


def frames[T](dispatcher: Dispatcher, scene: bpy.types.Scene, numbers: tuple[int, ...], consumed: Callable[[int, float], Outcome[T]], base: Option[Path] = Nothing) -> Outcome[tuple[T, ...]]:
    """Renders and consumes each frame through the call's own frame range, output rerooted under `base` when set."""

    def frame(number: int) -> Outcome[T]:
        started = time.monotonic()
        return dispatcher.render(dispatcher.identity, scene, RenderCall(animation=True, frame_start=Some(number), frame_end=Some(number))).bind(lambda _: consumed(number, time.monotonic() - started))

    return (
        _scoped(dispatcher, scene)
        .bind(lambda original: base.map(lambda root: dispatcher.placed(scene, original.rerooted(root)).map(lambda _: None)).default_value(Ok(None)))
        .bind(lambda _: reduce(lambda done, number: done.bind(lambda values: frame(number).map(lambda value: (*values, value))), numbers, Result[tuple[T, ...], Faults](ok=())))
    )


def commanded(dispatcher: Dispatcher, arguments: Run | RenderFrames, trailing: tuple[str, ...]) -> Outcome[object]:
    """Runs local queue progress or farm worker frame request."""
    match arguments:
        case Run():
            return bare(RunCommand.RENDER, trailing).bind(lambda _: rendered(dispatcher, arguments))
        case RenderFrames(scene=name, frames=numbers):
            match trailing:
                case (output_root,):
                    return scene_of(name).bind(lambda scene: frames(dispatcher, scene, numbers, lambda _frame, _seconds: Ok(None), Some(Path(output_root)))).map(lambda _: None)
                case _:
                    return Error(Fault.of(RecordReason.OUTSIDE, RunCommand.RENDER, repr(trailing), member="<output root>"))


def supervised(dispatcher: Dispatcher, run: Run, trailing: tuple[str, ...]) -> Outcome[Progress]:
    """Supervises local render processes under power hold, posting notices."""
    identity, host = dispatcher.identity, dispatcher.host

    def ran(queue: RenderQueue, queueing: Queueing) -> Outcome[Progress]:
        def completed(progress: Progress) -> Outcome[Progress]:
            identity.reports.reported(dispatcher.posted)(queueing.listener, Completed(frames=len(progress.marks), elapsed=progress.elapsed)).to_optional()
            return (shut() if queueing.power.shutdown else Ok(None)).map(lambda _: progress)

        def failed(faults: Faults) -> Outcome[Progress]:
            identity.reports.reported(dispatcher.posted)(queueing.listener, Failed(job=queue.stem, reason=" ".join(fault.message for fault in faults))).to_optional()
            return Error(faults)

        def started(observations: Option[tuple[Progress, datetime.datetime]]) -> Outcome[Progress]:
            if not Progress.resumed(observations, queue, os.getpid()).marks:
                identity.reports.reported(dispatcher.posted)(queueing.listener, Started(jobs=len(queue.jobs), frames=len(queue.steps))).to_optional()
            return queueing.power.held(lambda: supervise(run.target, queue, host, queueing.heartbeat)).map(completed).default_with(failed)

        return observed(host.path(queue.stem, RunFile.PROGRESS)).bind(started)

    return bare(RunCommand.SUPERVISE, trailing).bind(lambda _: paired(RecordFile(path=Path(run.queue), form=RenderQueue).read(), dispatcher.settings(identity))).bind(lambda pair: ran(*pair))


def rendered(dispatcher: Dispatcher, run: Run) -> Outcome[tuple[StageMark, ...]]:
    """Renders pending frames of first remaining job, storing each frame mark."""
    identity = dispatcher.identity

    def ran(queue: RenderQueue, queueing: Queueing) -> Outcome[tuple[StageMark, ...]]:
        records = RecordFile(path=dispatcher.host.path(queue.stem, RunFile.PROGRESS), form=Progress)
        holding = frozenset(part for part, job in enumerate(queue.jobs) if job.frames.numbers)

        def finished(progress: Progress, mark: StageMark) -> Outcome[StageMark]:
            before = frozenset(held.part for held in progress.marks[:-1])
            first = holding <= before | {mark.part} and not holding <= before
            for forecast in progress.forecast(queue, datetime.datetime.now(datetime.UTC)).filter(lambda _: first).to_list():
                identity.reports.reported(dispatcher.posted)(queueing.listener, Estimated(forecast=forecast)).to_optional()
            job = queue.jobs[mark.part]
            return (
                scene_of(job.scene).bind(lambda scene: _scoped(dispatcher, scene).bind(lambda path: path.removed(scene, job.frames.numbers))).map(lambda _: mark)
                if all(part != mark.part for part, _ in progress.remaining(queue))
                else Ok(mark)
            )

        def appended(mark: StageMark) -> Outcome[StageMark]:
            persisted: Outcome[Progress] = records.revised(lambda held: msgspec.structs.replace(held, marks=(*held.marks, mark)))
            return Result[Progress, Faults].bind(persisted, lambda held: finished(held, mark))

        def runs(progress: Progress) -> Outcome[tuple[StageMark, ...]]:
            remaining = frozenset(progress.remaining(queue))
            match tuple(step for step in progress.pending if step in remaining):
                case ((part, _), *_) as steps:
                    return scene_of(queue.jobs[part].scene).bind(
                        lambda scene: frames(
                            dispatcher, scene, tuple(frame for _, frame in steps), lambda frame, seconds: appended(StageMark(part=part, item=frame, elapsed=datetime.timedelta(seconds=seconds)))
                        )
                    )
                case _:
                    return Ok(())

        loaded: Outcome[Progress] = records.read()
        return Result[Progress, Faults].bind(loaded, runs)

    return paired(RecordFile(path=Path(run.queue), form=RenderQueue).read(), dispatcher.settings(identity)).bind(lambda pair: ran(*pair))


# --- [SESSION_PROGRESS]


def observation(dispatcher: Dispatcher, context: bpy.types.Context) -> Outcome[Option[Observed]]:
    """Reads sent record and local progress, `Nothing` sent before `prepare` writes it."""

    def read(plan: str) -> Outcome[Observed]:
        def pending(faults: Faults) -> Outcome[Option[Sent]]:
            match faults:
                case (Fault(reason=RecordReason.UNREADABLE, cause=FileNotFoundError()),):
                    return Ok(Nothing)
                case _:
                    return Error(faults)

        return (
            Result[Sent, Faults]
            .map(RecordFile(path=dispatcher.host.path(plan, RunFile.SENT), form=Sent).read(), Some)
            .or_else_with(pending)
            .bind(
                lambda sent: (
                    sent
                    .filter(lambda record: isinstance(record.target, LocalMachine))
                    .map(lambda record: observed(dispatcher.host.path(record.queue.stem, RunFile.PROGRESS)))
                    .default_value(Ok(Nothing))
                    .map(lambda progress: Observed(sent=sent, progress=progress))
                )
            )
        )

    return rooted(bpy.types.WindowManager, context).bind(lambda manager: dispatcher.queued.struct(manager).dispatched.map(lambda plan: read(plan).map(Some)).default_value(Ok(Nothing)))


def kept(dispatcher: Dispatcher) -> Outcome[tuple[RenderQueue, LocalMachine, Progress, datetime.datetime]]:
    """Returns observed local run, absent fault without local progress."""
    match dispatcher.state.observed.bind(lambda held: held.sent.bind(lambda sent: held.progress.map(lambda pair: (sent, *pair)))).to_optional():
        case (Sent(queue=queue, target=LocalMachine() as target), progress, beat):
            return Ok((queue, target, progress, beat))
        case _:
            return Error(Fault.of(BlenderReason.ABSENT, repr(dispatcher.queued), member=RunFile.PROGRESS))


def restored(dispatcher: Dispatcher) -> Outcome[Option[str]]:
    """Restores paused viewports still open."""
    return accumulated(prior.applied(now.area) for at, prior in dispatcher.state.resumed() for now in Located.found(at.window, at.area.as_pointer()).to_list()).map(lambda _: Nothing)


def ticked(dispatcher: Dispatcher) -> Outcome[bool]:
    """Refreshes the observation, restoring viewports and clearing a succeeded local run once it ends, answering whether a dispatched run is still pending."""
    state = dispatcher.state

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
            restored(dispatcher) if ended.is_some() and state.paused else Ok(Nothing),
            fresh
            .bind(lambda now: now.sent.bind(lambda sent: now.progress.filter(lambda pair: pair[0].ending is Ending.SUCCEEDED).map(lambda _: sent.queue)))
            .map(lambda queue: cleared(queue, dispatcher.host))
            .default_value(Ok(None)),
        ).map(lambda _: pending)

    return observation(dispatcher, bpy.context).bind(acted)


def watched(dispatcher: Dispatcher) -> None:
    """Observes a dispatched run each second until nothing it dispatched is pending, the session state holding the one timer."""
    dispatcher.state.watched(partial(_watching, dispatcher))


def _watching(dispatcher: Dispatcher) -> float | None:
    """Host timer refreshing the observation, stopping once no dispatched run is pending."""
    return 1.0 if dispatcher.identity.reports.reported(partial(ticked, dispatcher))().default_value(False) else None


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["commanded", "frames", "kept", "observation", "rendered", "restored", "supervised", "ticked", "watched"]
