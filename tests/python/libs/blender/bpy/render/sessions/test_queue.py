"""Native preparation processes, dispatch records, resumable files, and timer ownership."""

from collections.abc import Generator
from contextlib import ExitStack
from functools import partial
from hashlib import sha256
from pathlib import Path

import bpy
from expression import Some
import msgspec
import pytest

from blender.bpy.render.sessions.plans import Dispatcher, Observed, QueueState, Refused, Sent
from blender.bpy.render.sessions.queue import QueueRun
from blender.bpy.render.sessions.sets import RenderSet
from blender.bpy.render.sessions.setups import CATEGORIES
from blender.records.faults import Error, Fault, Nothing, Ok, Reason
from blender.records.stores import RecordFile
from blender.rendering.batch.queue import Ending, Frames, Job, MarkedCamera, Progress, RenderQueue
from blender.rendering.batch.supervision import detached, LocalMachine, RunCommand, RunFile
from blender.rendering.batch.targets import JobFile
from tests.python.libs.blender.bpy.background.session import Acquired

# --- [TYPES] ----------------------------------------------------------------------------

type Queued = tuple[Dispatcher, RenderQueue]

# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.fixture
def queued(acquired: Acquired, tmp_path: Path) -> Generator[Queued]:
    """Yields an acquired dispatcher with actual queue and log records."""
    queue_run = QueueRun(sets=RenderSet(capture=CATEGORIES))
    with acquired((queue_run,)) as identity, ExitStack() as release:
        dispatcher = queue_run.bound(identity).default_with(lambda faults: pytest.fail(str(faults)))
        host = dispatcher.host
        manager: bpy.types.WindowManager = bpy.context.window_manager  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        scene: bpy.types.Scene = bpy.context.scene  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        settings = dispatcher.queued.struct(manager)
        release.callback(setattr, settings, "plan", settings.plan)
        source, content = tmp_path / "job.blend", b"fixture job"
        source.write_bytes(content)
        frame = scene.frame_current
        queue = RenderQueue(
            jobs=(
                Job(
                    file=str(source),
                    digest=sha256(content).digest(),
                    scene=scene.name,
                    engine=scene.render.engine,
                    frames=Frames(start=frame, end=frame, step=scene.frame_step),
                    camera=MarkedCamera(),
                    pixels=(scene.render.resolution_x, scene.render.resolution_y),
                    output_root=str(tmp_path),
                    outputs={},
                    passes=(),
                ),
            ),
            blender=bpy.app.version_string,
            extension=identity.manifest.version,
        )
        settings.plan = queue.stem
        for file in RunFile:
            release.callback(host.path(queue.stem, file).unlink, missing_ok=True)
        record = RecordFile(path=host.path(queue.stem, RunFile.QUEUE), form=RenderQueue)
        assert record.written(queue) == Ok(record.path)
        host.path(queue.stem, RunFile.LOG).write_text("queue log", encoding="utf-8")
        yield dispatcher, queue


def test_preparation_process_lifecycle_and_recordless_failure(queued: Queued, tmp_path: Path) -> None:
    """Child stdout, native exits, refusal precedence, and failure causes survive process cleanup."""
    alive, crashed = QueueState(), QueueState()
    _dispatcher, queue = queued
    plan = queue.stem
    marker, code = "prepare child", 3
    log = tmp_path / "prepare.log"
    unwritten = Observed(plan=plan, sent=Nothing, progress=Nothing)
    assert QueueState().exited() == Nothing
    with ExitStack() as release:
        running = (
            detached(("python3", "-c", "from threading import Event; Event().wait()"), tmp_path / "waiting.log")
            .map(release.enter_context)
            .map(partial(alive.started, plan))
            .default_with(lambda faults: pytest.fail(str(faults)))
        )
        release.callback(running.kill)
        exiting = (
            detached(("python3", "-c", f"print({marker!r}, flush=True); raise SystemExit({code})"), log)
            .map(release.enter_context)
            .map(partial(crashed.started, plan))
            .default_with(lambda faults: pytest.fail(str(faults)))
        )
        running_status = alive.exited()
        assert running_status == Nothing
        assert unwritten.unrecorded(running_status.map(lambda held: held[1])) == Nothing
    assert running.returncode != 0
    assert alive.exited().is_some()
    assert exiting.returncode == code
    exited = crashed.exited()
    assert exited == Some((plan, code))
    assert log.read_text(encoding="utf-8").splitlines() == [marker]
    status = exited.map(lambda held: held[1])
    assert unwritten.unrecorded(status) == Some(Fault.of(Reason.FAILED, RunCommand.PREPARE, (code, (RunFile.SENT, RunFile.REFUSED))))
    assert Observed(plan=plan, sent=Nothing, progress=Nothing, refused=("Scene holds no camera",)).unrecorded(status) == Nothing


def test_queue_scope_releases_native_observation_timer() -> None:
    """Queue state owns one native timer and unregisters it when its scope closes."""
    tick, other = lambda: None, lambda: None
    with QueueState.held() as state:
        state.watched(tick)
        state.watched(other)
        assert bpy.app.timers.is_registered(tick)
        assert not bpy.app.timers.is_registered(other)
    assert not bpy.app.timers.is_registered(tick)


@pytest.mark.parametrize("ending", [*Ending, None])
def test_local_endings_keep_resumable_files(queued: Queued, ending: Ending | None) -> None:
    """Pending work continues, and success alone deletes queue files."""
    dispatcher, queue = queued
    sent = RecordFile(path=dispatcher.host.path(queue.stem, RunFile.SENT), form=Sent)
    assert sent.written(Sent(queue=queue, target=LocalMachine(), issued=())) == Ok(sent.path)
    progress = RecordFile(path=dispatcher.host.path(queue.stem, RunFile.PROGRESS), form=Progress)
    assert progress.written(Progress(queue=queue.key, process=0, ending=ending)) == Ok(progress.path)
    assert dispatcher.ticked() == Ok(ending is None)
    assert dispatcher.state.observed.bind(lambda held: held.progress).map(lambda held: held[0].ending) == Some(ending)
    files = RunFile.QUEUE, RunFile.PROGRESS, RunFile.LOG
    assert tuple(dispatcher.host.path(queue.stem, file).exists() for file in files) == (ending is not Ending.SUCCEEDED,) * len(files)


def test_remote_submission_has_no_local_progress(queued: Queued) -> None:
    """Remote submission retains its issued records without local progress."""
    dispatcher, queue = queued
    target = JobFile()
    sent = RecordFile(path=dispatcher.host.path(queue.stem, RunFile.SENT), form=Sent)
    record = Sent(queue=queue, target=target, issued=(str(Path(dispatcher.host.folder) / "sent.job"),))
    assert sent.written(record) == Ok(sent.path)
    assert dispatcher.ticked() == Ok(value=False)
    assert dispatcher.state.observed.map(lambda held: (held.sent, held.progress)) == Some((Some(record), Nothing))


def test_refused_preparation_preserves_cause(queued: Queued) -> None:
    """Refused preparation retains its original cause without a sent record."""
    dispatcher, queue = queued
    messages = ("Scene holds no camera",)
    refused = RecordFile(path=dispatcher.host.path(queue.stem, RunFile.REFUSED), form=Refused)
    assert refused.written(Refused(messages=messages)) == Ok(refused.path)
    assert dispatcher.ticked() == Ok(value=False)
    assert dispatcher.state.observed.map(lambda held: (held.sent, held.refused)) == Some((Nothing, messages))


@pytest.mark.parametrize("code", [0, 1])
def test_recordless_preparation_preserves_native_status(queued: Queued, tmp_path: Path, code: int) -> None:
    """Recordless successful or failed process exit retains its native status."""
    dispatcher, queue = queued
    launched = detached(("python3", "-c", f"raise SystemExit({code})"), tmp_path / "prepare.log").default_with(lambda faults: pytest.fail(str(faults)))
    with launched as child:
        dispatcher.state.started(queue.stem, child)
        assert child.wait() == code
        assert dispatcher.state.exited() == Some((queue.stem, code))
        answer = dispatcher.ticked()
    expected = Fault.of(Reason.FAILED, RunCommand.PREPARE, (code, (RunFile.SENT, RunFile.REFUSED)))
    assert answer == Error(expected)
    assert dispatcher.state.observed.map(lambda held: held.refused) == Some(expected)


def test_old_preparation_exit_does_not_fail_newly_selected_plan(queued: Queued, tmp_path: Path) -> None:
    """A real old child exit cannot mark a different pending native queue plan as failed."""
    dispatcher, original = queued
    source = tmp_path / "replacement.blend"
    content, code = b"replacement job", 3
    source.write_bytes(content)
    replacement = msgspec.structs.replace(original, jobs=(msgspec.structs.replace(original.jobs[0], file=str(source), digest=sha256(content).digest()),))
    path = dispatcher.host.path(replacement.stem, RunFile.QUEUE)
    with ExitStack() as release:
        release.callback(path.unlink, missing_ok=True)
        stored = RecordFile(path=path, form=RenderQueue)
        assert stored.written(replacement) == Ok(path)
        launched = detached(("python3", "-c", f"raise SystemExit({code})"), tmp_path / "old-prepare.log").default_with(lambda faults: pytest.fail(str(faults)))
        with launched as child:
            dispatcher.state.started(original.stem, child)
            manager: bpy.types.WindowManager = bpy.context.window_manager  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            dispatcher.queued.struct(manager).plan = replacement.stem
            assert child.wait() == code
            assert dispatcher.state.exited() == Some((original.stem, code))
            assert dispatcher.ticked() == Ok(value=True)
        assert dispatcher.state.observed == Some(Observed(plan=replacement.stem, sent=Nothing, progress=Nothing))
        assert stored.read() == Ok(replacement)
