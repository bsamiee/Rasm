"""Queue probes over a real `prepare` launch: a held child read while it runs and after it exits, and the fault a recordless exit raises."""

from pathlib import Path
import sys
from typing import Final

from expression import Nothing

from blender.bpy.extension.identity import Identity
from blender.bpy.render.sessions.plans import Observed, QueueState
from blender.rendering.batch.supervision import detached

# --- [OPERATIONS] -----------------------------------------------------------------------


def prepared(identity: Identity) -> dict[str, object]:
    """Launches a child through the dispatch launcher, reads its exit while alive and after a recordless exit, and the fault each observation raises."""
    log = identity.folder / "prepare.log"
    unstarted = QueueState()
    alive, crashed = QueueState(), QueueState()
    running = detached((sys.executable, "-c", "import time; time.sleep(60)"), log).map(alive.started)
    exiting = detached((sys.executable, "-c", "raise SystemExit(3)"), log).map(crashed.started)
    while_alive = alive.exited()
    for process in alive.preparing.to_list():
        process.kill()
        process.wait()
    for process in crashed.preparing.to_list():
        process.wait()
    exited = crashed.exited()
    unwritten = Observed(sent=Nothing, progress=Nothing)
    return {
        "launched": (running.map(lambda _: None), exiting.map(lambda _: None)),
        "unstarted": unstarted.exited(),
        "running": while_alive,
        "exited": exited,
        "unrecorded": unwritten.unrecorded(exited),
        "message": unwritten.unrecorded(exited).map(lambda faults: faults[0].message),
        "refused": Observed(sent=Nothing, progress=Nothing, refused=("Scene holds no camera",)).unrecorded(exited),
        "alive": unwritten.unrecorded(while_alive),
        "logged": Path(log).is_file(),
    }


# --- [COMPOSITION] ----------------------------------------------------------------------

rows: Final = ()
PROBES: Final = (prepared,)
RELEASED: Final = ()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["PROBES", "RELEASED", "rows"]
