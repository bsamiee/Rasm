"""Reconcile a declared font set from its originals: source, build, check, approve, run and publish each face."""

from functools import partial
from importlib import metadata
import sys
from typing import Annotated, NoReturn

from cyclopts import App, config, Parameter

from rasm_cli import DEFAULTS, exited, Settings
lazy from rasm_cli import invoked
lazy from rasm_foundry._builds import Action, performed

# --- [OPERATIONS] -----------------------------------------------------------------------


def __getattr__(name: str, /) -> object:
    """Binds the action a registered command names to the pipeline run, importing the pipeline at that command's first use."""
    if name in app:
        return partial(performed, Action(name))
    raise AttributeError(f"module {__name__!r} has no attribute {name!r}", name=name, obj=sys.modules[__name__])


# --- [COMPOSITION] ----------------------------------------------------------------------

_DISTRIBUTION = metadata.distribution(__name__.partition(".")[0])

app = App(
    name=next(point.name for point in _DISTRIBUTION.entry_points if point.group == "console_scripts"),
    help=_DISTRIBUTION.metadata["Summary"],
    version=_DISTRIBUTION.version,
    result_action=(exited, "sys_exit"),
    default_parameter=Parameter(negative=()),
    error_formatter=str,
)
app.meta.config = config.Env(f"{app.name[0].upper()}_", command=False)
app.command(f"{__name__}:source", help="Write each selected face's UFO source from its originals.")
app.command(f"{__name__}:build", help="Compile each selected face whose inputs changed since its last build.")
app.command(f"{__name__}:check", help="Verify each selected face's current build.")
app.command(f"{__name__}:approve", help="Approve each selected face's shaping.")
app.command(f"{__name__}:run", help="Build and verify each selected face.")
app.command(f"{__name__}:publish", help="Publish each selected face's verified build into a destination folder.")


@app.meta.default
async def main(*tokens: Annotated[str, Parameter(show=False, allow_leading_hyphen=True)], settings: Settings = DEFAULTS) -> NoReturn:
    """Runs one command line inside the process telemetry scope, every module logger writing to its log."""
    with invoked(app, tokens, settings) as invocation:
        await invocation.run()


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["app"]
