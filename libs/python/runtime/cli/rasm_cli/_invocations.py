"""Invocations of a command-line tool: the settings every tool reads from flags and its own variables, its per-user folders, and one command line run inside the process telemetry scope."""

from collections.abc import Callable, Generator
from contextlib import contextmanager, ExitStack
from dataclasses import dataclass, field
from enum import auto, StrEnum
import inspect
from pathlib import Path
import sys
from typing import Annotated, Final, NoReturn

from cyclopts import App, Group, Parameter
lazy from expression import Result
lazy from opentelemetry import trace
lazy from opentelemetry.configuration import ConfigurationError
lazy from opentelemetry.semconv.attributes.error_attributes import ERROR_TYPE
lazy from opentelemetry.trace import Span, Status, StatusCode
lazy from platformdirs import PlatformDirs
lazy import structlog
lazy from structlog.stdlib import BoundLogger

from rasm_cli._results import exited
from rasm_telemetry import LogSettings
lazy from rasm_records import Error, Fault, Reason
lazy from rasm_telemetry.process import instrumented

# --- [TYPES] ----------------------------------------------------------------------------


class Folder(StrEnum):
    """Kinds of per-user folder a tool writes, each the name of the command parameter an invocation supplies it to."""

    CACHE = auto()
    LOG = auto()
    RUNTIME = auto()
    STATE = auto()


# --- [MODELS] ---------------------------------------------------------------------------


@Parameter(name="*", group=Group.create_ordered("Process"))
@dataclass(frozen=True, slots=True, kw_only=True)
class Settings:
    """Settings every tool reads before its command, each a flag and the variable `<TOOL>_<FLAG>` a `cyclopts.config.Env` source of the tool's meta app names, a log field left unset by both reading `LOG_<FIELD>`.

    Attributes:
        log: Levels and rotation of the log, each field the flag `--log-<field>`, one logger's level `--log-filter.<logger>`, a size, age, or count outside its bounds refused as a usage error.
        log_target: Destination of the log: `file` for the rotated file `<log folder>/<tool>.log`, `stderr` for the standard error a container collects.
        log_dir: Folder of the log file, the per-user log folder when omitted.
        cache_dir: Folder of files the tool can make again, the per-user cache folder when omitted.
        state_dir: Folder of files kept between runs, the per-user state folder when omitted.
        runtime_dir: Folder of sockets and locks of running processes, the per-user runtime folder when omitted.
    """

    log: Annotated[LogSettings, Parameter(name="*", name_transform="log-{}".format)] = field(default_factory=LogSettings)
    log_target: str = "file"
    log_dir: Path | None = None
    cache_dir: Path | None = None
    state_dir: Path | None = None
    runtime_dir: Path | None = None

    def folder(self, kind: Folder, tool: str, /) -> Path:
        """Returns the folder of a kind: its flag or variable, else the platform's per-user folder of a tool, neither created."""
        platform = PlatformDirs(tool, appauthor=False)
        return {
            Folder.CACHE: self.cache_dir or platform.user_cache_path,
            Folder.LOG: self.log_dir or platform.user_log_path,
            Folder.RUNTIME: self.runtime_dir or platform.user_runtime_path,
            Folder.STATE: self.state_dir or platform.user_state_path,
        }[kind].expanduser()


DEFAULTS: Final = Settings()

# --- [SERVICES] -------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class Invocation:
    """One command line of a tool inside its telemetry scope: the logger of the tool, bound at its first entry, the call applying another settings record's levels to the running process, and the span holding the run."""

    app: App
    tokens: tuple[str, ...]
    settings: Settings
    log: BoundLogger
    reload: Callable[[LogSettings], None]
    span: Span

    def folder(self, kind: Folder, /) -> Path:
        """Returns the tool's folder of a kind, not created."""
        return self.settings.folder(kind, self.app.name[0])

    def ended(self, result: object, /) -> NoReturn:
        """Writes a command's result through `exited`, logs each fault with its fields, marks the span failed by the first, logs the one `INFO` entry of the run with its status, and exits with that status."""
        status = exited(result if isinstance(result, Result) else None)
        faults = result.swap().default_value(()) if isinstance(result, Result) else ()
        for fault in faults:
            self.log.error("Command refused", reason=fault.reason.name, subject=fault.subject, member=fault.member, cause=fault.cause)
        for fault in faults[:1]:
            self.span.set_status(Status(StatusCode.ERROR, fault.message))
            self.span.set_attribute(ERROR_TYPE, fault.reason.name)
        self.log.info("Command ended", status=status)
        sys.exit(status)

    async def run(self) -> NoReturn:
        """Parses the command line, supplies each unparsed parameter named for a `Folder` its folder, runs the command, and ends with its result, the start a `DEBUG` entry."""
        self.log.debug("Command started")
        command, bound, unparsed = self.app.parse_args(self.tokens)
        result = command(*bound.args, **bound.kwargs, **{name: self.folder(Folder(name)) for name in unparsed})
        self.ended(await result if inspect.isawaitable(result) else result)


# --- [OPERATIONS] -----------------------------------------------------------------------


@contextmanager
def invoked(app: App, tokens: tuple[str, ...], settings: Settings, /) -> Generator[Invocation]:
    """Holds the telemetry scope of one command line: the process instrumented under the app's name, and one span named by the tool and its command.

    The log is the file `<log folder>/<tool>.log` or the process's standard error, as the log target names, and the service version is the app's `version` string, empty where the app states none.
    The log level applies to the loggers named for the tool and for the root package of its meta command, the log filter to the loggers it names, every other logger passing warnings and above.
    A log target or level name outside its accepted values logs one warning naming its variable, its value, and the accepted values, the target leaving its default in effect and the level name reading as `NOTSET`.
    The tool's meta command awaits `Invocation.run` or hands a result to `Invocation.ended`.
    The SDK reads the `OTEL_*` variables of the process environment, and the span continues the trace context a parent wrote there.
    A log folder refusing creation exits with its `UNWRITABLE` fault, and an exporter, protocol, or configuration file the SDK refuses with its `INVALID` fault, before the scope opens.

    Yields:
        Invocation of the command line.
    """
    tool, module = app.name[0], inspect.getmodule(app.meta.default_command)
    sinks = {"file": settings.folder(Folder.LOG, tool) / f"{tool}.log", "stderr": sys.stderr}
    unrecognized = (*((("target", settings.log_target, " ".join(sinks)),) if settings.log_target not in sinks else ()), *settings.log.unrecognized)
    with ExitStack() as stack:
        try:
            reload = stack.enter_context(
                instrumented(
                    sinks.get(settings.log_target, sinks[DEFAULTS.log_target]),
                    service=tool,
                    version=app.version if isinstance(app.version, str) else "",
                    settings=settings.log,
                    scopes=(tool, *((module.__name__.partition(".")[0],) if module else ())),
                )
            )
        except OSError as error:
            sys.exit(exited(Error(Fault.of(Reason.UNWRITABLE, tool, error))))
        except (RuntimeError, ConfigurationError) as error:
            sys.exit(exited(Error(Fault.of(Reason.INVALID, tool, error))))
        for name, value, accepted in unrecognized:
            structlog.stdlib.get_logger(__name__).warning("Setting unrecognized", variable=f"{tool.upper()}_LOG_{name.upper()}", value=value, accepted=accepted)
        span = stack.enter_context(trace.get_tracer(tool).start_as_current_span(" ".join((tool, *(token for token in tokens[:1] if token in app)))))
        yield Invocation(app=app, tokens=tokens, settings=settings, log=structlog.stdlib.get_logger(tool), reload=reload, span=span)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["DEFAULTS", "Folder", "Invocation", "Settings", "invoked"]
