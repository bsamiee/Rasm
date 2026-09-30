"""Applies the interface to Rhino and the packages `packages.toml` declares, then edits the files Rhino reads at launch once it quit."""

import anyio
import msgspec
import psutil

from interface.host import Applied, Error, Failed, Host, Line, Measurement, outcome, quitted, registered, reopened, running
from interface.rhino.packages import declared, install, Package
from interface.rhino.session import announced, launched, released, reported, Rhino, run
from interface.rhino.stores import edit
from interface.rhino.window import Measured

# --- [OPERATIONS] -----------------------------------------------------------------------


async def ran(host: Host, rhino: Rhino, packages: tuple[Package, ...]) -> tuple[tuple[Line, ...], tuple[str, ...]]:
    """Report rows of the `main` call inside a fresh Rhino and of the files edited once it quit with the run's error output, a launch sending no port or a Rhino outliving its quit each an error row."""
    port = await launched(rhino)
    rows, stderr = await run(port, t"main(__rhino_doc__)") if isinstance(port, int) else ((port,), ())
    reported = (*rows, *map(Error, await quitted(await registered(running(rhino.bundle)))))
    match outcome(host.app, reported):
        case Applied(folder=folder):
            (record,) = (row.record for row in rows if isinstance(row, Measurement))
            edited = await anyio.to_thread.run_sync(edit, folder, rhino.bundle, msgspec.json.decode(record, type=Measured), packages, host.cache)
            return (*reported, *edited), stderr
        case Failed():
            return reported, stderr


async def applied(host: Host, rhino: Rhino) -> Applied | Failed:
    """Outcome of the staged archives installed and the interface run, the install changes first."""
    packages = await declared()
    match await install(host, rhino, packages):
        case Error() as failed:
            return outcome(host.app, (failed,))
        case changes:
            rows, stderr = await ran(host, rhino, packages)
            return msgspec.structs.replace(outcome(host.app, (*changes, *rows)), stderr=stderr)


async def cleared(host: Host, rhino: Rhino, discovered: tuple[psutil.Process, ...]) -> Applied | Failed:
    """Outcome once every discovered Rhino reported its titled documents, released its untitled ones, and quit, reopened on the titled paths at the end, failed before any release while a titled document holds unsaved edits."""
    ports = await anyio.gather(*(announced(rhino, process) for process in discovered))
    listening = tuple(port for port in ports if isinstance(port, int))
    reports = await anyio.gather(*(reported(port) for port in listening))
    held = (*(port.text for port in ports if isinstance(port, Error)), *(error for _, errors in reports for error in errors))
    if errors := held or tuple(error for errors in await anyio.gather(*(released(port) for port in listening)) for error in errors):
        return outcome(host.app, tuple(map(Error, errors)))
    instances = await registered(discovered)
    async with reopened(rhino.bundle, instances, *(path for paths, _ in reports for path in paths), arguments=("-nosplash",)):
        match await quitted(instances):
            case ():
                return await applied(host, rhino)
            case quit_errors:
                return outcome(host.app, tuple(map(Error, quit_errors)))


# --- [COMPOSITION] ----------------------------------------------------------------------


async def apply(host: Host) -> tuple[Applied | Failed]:
    """Rhino's outcome over the instances running at discovery."""
    match await Rhino.resolve(host):
        case Error() as failed:
            return (outcome(host.app, (failed,)),)
        case Rhino() as rhino:
            return (await cleared(host, rhino, running(rhino.bundle)),)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["apply"]
