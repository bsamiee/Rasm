"""One log file rotated and locked across threads and forked processes through the handler of `rasm_telemetry.LogSettings.handler`, copied from four files of concurrent-log-handler `tests/` (0.9.31)."""

from collections.abc import Callable
from datetime import datetime, timedelta, UTC
import logging
import multiprocessing
import multiprocessing.synchronize
import os
from pathlib import Path
import signal
import subprocess
import sys
import textwrap
import threading
import time

from concurrent_log_handler import ConcurrentRotatingFileHandler, ConcurrentTimedRotatingFileHandler
import portalocker
import pytest
import time_machine

from rasm_telemetry import LogSettings

# --- [CONSTANTS] ------------------------------------------------------------------------

MIN_WAIT_SECONDS = 0.05
MAX_WAIT_SECONDS = 0.75
TIMEOUT = 10
HOLDER_SCRIPT = textwrap.dedent("""
    import sys
    import time

    from portalocker import LOCK_EX, lock, unlock

    with open(sys.argv[1], "a") as fh:
        lock(fh, LOCK_EX)
        print("locked", flush=True)
        time.sleep(float(sys.argv[2]))
        unlock(fh)
    """)

# --- [OPERATIONS] -----------------------------------------------------------------------


def _make_record(msg: str) -> logging.LogRecord:
    """Returns an `INFO` record of a message (`test_concurrent_rotation_multiprocess.py:32-35`)."""
    return logging.makeLogRecord({"msg": msg, "levelno": logging.INFO, "levelname": "INFO", "name": "test"})


def _backdate_rollover(handler: ConcurrentTimedRotatingFileHandler, seconds: int = 10) -> None:
    """Sets `rolloverAt` to `seconds` in the past and persists it to the lock file (`test_concurrent_rotation_multiprocess.py:38-46`)."""
    past_time = int(time.time()) - seconds
    handler.clh._do_lock()
    handler.rolloverAt = past_time
    handler.write_rollover_time()
    handler.clh._do_unlock()


def _forked_worker(handler: ConcurrentTimedRotatingFileHandler, barrier: multiprocessing.synchronize.Barrier, result_queue: multiprocessing.Queue[tuple[str, int | str]]) -> None:
    """Waits for every sibling, then emits one record and reports its rollover count (`test_concurrent_rotation_multiprocess.py:52-65`)."""
    try:
        barrier.wait(timeout=15)
        handler.emit(_make_record(f"pid {os.getpid()}"))
        result_queue.put(("rollovers", handler.num_rollovers))
    except Exception as exc:
        result_queue.put(("error", str(exc)))
    finally:
        handler.close()


def _start_paused_write(tmp_path: Path, name: str, monkeypatch: pytest.MonkeyPatch) -> tuple[logging.Handler, logging.Logger, threading.Thread, threading.Event]:
    """Starts a thread paused inside `do_write`, holding every lock (`test_fork_deadlock.py:52-66,69-89`)."""
    in_write = threading.Event()
    resume = threading.Event()
    do_write = ConcurrentRotatingFileHandler.do_write

    def pausing(self: ConcurrentRotatingFileHandler, msg: str) -> None:
        if "from thread" in msg:
            in_write.set()
            resume.wait(TIMEOUT)
        do_write(self, msg)

    monkeypatch.setattr(ConcurrentRotatingFileHandler, "do_write", pausing)
    handler = LogSettings().handler(tmp_path / "fork.log", service="rasm-telemetry-tests", version="0")
    logger = logging.getLogger(name)
    logger.handlers.clear()
    logger.addHandler(handler)
    logger.setLevel(logging.INFO)
    logger.propagate = False
    thread = threading.Thread(target=logger.info, args=("from thread",))
    thread.start()
    assert in_write.wait(TIMEOUT), "The thread never reached do_write()"
    return handler, logger, thread, resume


def _fork_and_run(child: Callable[[], int]) -> int:
    """Forks, runs `child` in the forked process, which exits with its result, and returns its PID (`test_fork_deadlock.py:92-106`)."""
    if not (hasattr(os, "fork") and hasattr(signal, "alarm")):
        pytest.skip("os.fork() not available on Windows")
    if (pid := os.fork()) == 0:
        signal.alarm(TIMEOUT)
        exit_code = 1
        try:
            exit_code = child()
        finally:
            os._exit(exit_code)
    return pid


def test_concurrent_timed_rotation_threads_single_rollover(tmp_path: Path) -> None:
    """Threads sharing one handler past its rollover time rotate it once (`test_concurrent_rotation_multiprocess.py:71-122`)."""
    log_file = tmp_path / "threaded.log"
    handler = LogSettings(age=timedelta(seconds=3600), files=5).handler(log_file, service="rasm-telemetry-tests", version="0")
    assert isinstance(handler, ConcurrentTimedRotatingFileHandler)
    handler.emit(_make_record("init"))
    _backdate_rollover(handler)
    n = 8
    barrier = threading.Barrier(n)
    errors: list[Exception] = []

    def worker() -> None:
        try:
            barrier.wait()
            handler.emit(_make_record(f"thread {threading.get_ident()}"))
        except Exception as exc:
            errors.append(exc)

    threads = [threading.Thread(target=worker) for _ in range(n)]
    for t in threads:
        t.start()
    for t in threads:
        t.join()
    handler.close()
    assert not errors, f"Thread errors: {errors}"
    rotated = [f for f in tmp_path.iterdir() if f.name.startswith("threaded.log.") and "lock" not in f.name]
    assert handler.num_rollovers == 1, f"Expected 1 rollover, got {handler.num_rollovers}. Rotated files: {[f.name for f in rotated]}"
    assert len(rotated) == 1, f"Expected 1 rotated file, found: {[f.name for f in rotated]}"


@pytest.mark.skipif(sys.platform == "win32", reason="os.fork() not available on Windows")
def test_concurrent_timed_rotation_forked_handler_single_rollover(tmp_path: Path) -> None:
    """Forked children sharing a handler made before the fork rotate it once in total (`test_concurrent_rotation_multiprocess.py:128-202`)."""
    log_file = tmp_path / "fork_test.log"
    handler = LogSettings(age=timedelta(seconds=3600), files=5).handler(log_file, service="rasm-telemetry-tests", version="0")
    assert isinstance(handler, ConcurrentTimedRotatingFileHandler)
    handler.emit(_make_record("init"))
    _backdate_rollover(handler)
    n = 5
    ctx = multiprocessing.get_context("fork")
    barrier = ctx.Barrier(n)
    result_queue = ctx.Queue()
    processes = [ctx.Process(target=_forked_worker, args=(handler, barrier, result_queue)) for _ in range(n)]
    for p in processes:
        p.start()
    for p in processes:
        p.join(timeout=30)
    handler.close()
    results = [result_queue.get_nowait() for _ in range(n)]
    child_errors = [r[1] for r in results if r[0] == "error"]
    assert not child_errors, f"Child process errors: {child_errors}"
    total_rollovers = sum(r[1] for r in results if r[0] == "rollovers")
    rotated = [f for f in tmp_path.iterdir() if f.name.startswith("fork_test.log.") and "lock" not in f.name]
    assert total_rollovers == 1, f"Expected exactly 1 rollover across {n} forked processes, got {total_rollovers}. Rotated files: {[f.name for f in rotated]}"
    assert len(rotated) == 1, f"Expected exactly 1 rotated file, found: {[f.name for f in rotated]}"


def test_emit_does_not_stall_while_another_process_holds_lock(tmp_path: Path) -> None:
    """An emit waits for a lock another process holds and finishes soon after its release (`test_lock_wait_latency.py:58-111`)."""
    hold_seconds = 0.2
    timeout = 30
    handler = LogSettings().handler(tmp_path / "wait.log", service="rasm-telemetry-tests", version="0")
    logger = logging.getLogger("clh.lock_wait_latency")
    logger.handlers.clear()
    logger.addHandler(handler)
    logger.setLevel(logging.INFO)
    logger.propagate = False
    try:
        logger.info("before")
        holder = subprocess.Popen(
            [sys.executable, "-c", HOLDER_SCRIPT, str(Path(*ConcurrentRotatingFileHandler.baseLockFilename(str(tmp_path / "wait.log")))), str(hold_seconds)], stdout=subprocess.PIPE, text=True
        )
        try:
            assert holder.stdout is not None
            assert holder.stdout.readline().strip() == "locked"
            t0 = time.perf_counter()
            logger.info("while locked")
            waited = time.perf_counter() - t0
            assert holder.wait(timeout) == 0
        finally:
            if holder.poll() is None:
                holder.kill()
                holder.wait(timeout)
            holder.stdout.close()
    finally:
        handler.close()
        logger.handlers.clear()
    assert waited >= MIN_WAIT_SECONDS, f"The emit took only {waited * 1000:.0f} ms, so it did not wait for the second process. The test did not create lock contention."
    assert waited < MAX_WAIT_SECONDS, f"The emit waited {waited * 1000:.0f} ms for a lock that was held for {hold_seconds * 1000:.0f} ms. Expected less than {MAX_WAIT_SECONDS * 1000:.0f} ms."
    content = (tmp_path / "wait.log").read_text(encoding="utf-8")
    assert "before" in content
    assert "while locked" in content


@pytest.mark.skipif(sys.platform == "win32", reason="os.fork() not available on Windows")
@pytest.mark.filterwarnings("ignore::DeprecationWarning")
def test_fork_while_another_thread_is_in_emit(tmp_path: Path, monkeypatch: pytest.MonkeyPatch) -> None:
    """A child forked while another thread is inside an emit logs without hanging (`test_fork_deadlock.py:109-130`)."""
    if not (hasattr(os, "WIFSIGNALED") and hasattr(os, "WTERMSIG") and hasattr(os, "WEXITSTATUS")):
        pytest.skip("os.WIFSIGNALED() not available on Windows")
    handler, logger, thread, resume = _start_paused_write(tmp_path, "clh.fork_deadlock", monkeypatch)

    def child() -> int:
        logger.info("from child")
        return 0

    pid = _fork_and_run(child)
    resume.set()
    thread.join(TIMEOUT)
    _, status = os.waitpid(pid, 0)
    handler.close()
    logger.handlers.clear()
    assert not os.WIFSIGNALED(status), f"The child hung on its first log call and was killed by signal {os.WTERMSIG(status)}. It inherited _thread_lock in the held state."
    assert os.WEXITSTATUS(status) == 0, "The child's log call raised an exception"
    content = (tmp_path / "fork.log").read_text(encoding="utf-8")
    assert "from thread" in content
    assert "from child" in content


@pytest.mark.skipif(sys.platform == "win32", reason="os.fork() not available on Windows")
@pytest.mark.filterwarnings("ignore::DeprecationWarning")
def test_child_close_keeps_parent_lock(tmp_path: Path, monkeypatch: pytest.MonkeyPatch) -> None:
    """A forked child closing the handler leaves the parent's file lock held (`test_fork_deadlock.py:133-166`)."""
    if not (hasattr(os, "WIFSIGNALED") and hasattr(os, "WTERMSIG") and hasattr(os, "WEXITSTATUS")):
        pytest.skip("os.WIFSIGNALED() not available on Windows")
    lock_was_released = 2
    handler, logger, thread, resume = _start_paused_write(tmp_path, "clh.fork_child_close", monkeypatch)

    def child() -> int:
        handler.close()
        with Path(*ConcurrentRotatingFileHandler.baseLockFilename(str(tmp_path / "fork.log"))).open("r+", encoding="utf-8") as lock_file:
            try:
                portalocker.lock(lock_file, portalocker.LOCK_EX | portalocker.LOCK_NB)
            except portalocker.exceptions.LockException:
                return 0
            return lock_was_released

    pid = _fork_and_run(child)
    _, status = os.waitpid(pid, 0)
    resume.set()
    thread.join(TIMEOUT)
    handler.close()
    logger.handlers.clear()
    assert not os.WIFSIGNALED(status), f"The child hung (signal {os.WTERMSIG(status)})"
    assert os.WEXITSTATUS(status) != lock_was_released, "The child's close() released the parent's file lock. It inherited is_locked=True and unlocked the shared file description."
    assert os.WEXITSTATUS(status) == 0, "The child raised an exception"
    assert "from thread" in (tmp_path / "fork.log").read_text(encoding="utf-8")


def test_timed_rollover_on_startup_after_downtime(tmp_path: Path) -> None:
    """A handler opened after its file's rollover time passed rotates the stale file at once, the first record being the exemplar's, built with a message and no level, which the handler must write for the file to exist (`test_timed_rollover_downtime.py:10-78`)."""
    log_file = tmp_path / "test_startup_rollover.log"
    rollover_interval_seconds = 2
    with time_machine.travel(datetime.now(UTC), tick=False) as clock:
        h1 = LogSettings(age=timedelta(seconds=rollover_interval_seconds), files=5).handler(log_file, service="rasm-telemetry-tests", version="0")
        h1.emit(logging.makeLogRecord({"msg": "initial log to create files"}))
        h1.close()
        clock.shift(rollover_interval_seconds + 1)
        h2 = LogSettings(age=timedelta(seconds=rollover_interval_seconds), files=5).handler(log_file, service="rasm-telemetry-tests", version="0")
        h2.close()
    files_in_dir = list(tmp_path.glob("*"))
    rotated_log_files = [f for f in files_in_dir if str(f.name).startswith(log_file.name + ".")]
    assert rotated_log_files, "A rotated log file should have been created on startup, but was not."
