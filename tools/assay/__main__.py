"""Expose the Assay command registry as the process entrypoint.

The entrypoint preserves one stdout Envelope for parse, config, dispatch, and
unexpected faults, then drains tracing outside the command result channel.
"""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

import os
import sys
from typing import Annotated, Final

from cyclopts import Parameter
from cyclopts.exceptions import CycloptsError
from opentelemetry.trace import get_tracer_provider
from pydantic import ValidationError

from tools.assay import (
    _DRAIN_MS,  # noqa: PLC2701  # shared trace budget owned by the package runtime hook
    configure_logging,
    install_tracing,
)
from tools.assay.composition.registry import build_app, parse_fault, REGISTRY
from tools.assay.composition.settings import AssaySettings
from tools.assay.core.model import wire_safe
from tools.assay.core.status import Step


# --- [CONSTANTS] ------------------------------------------------------------------------

# The agent-first offload surface: `--exec ssh://[user@]host[:port]` (or `--exec local`) is the primary, --help-visible
# way to select the execution target; it is admitted through the same validated ASSAY_EXEC_TARGET env path, which stays
# the fallback override. The flag is extracted at the entrypoint boundary so the rest of the token stream dispatches unchanged.
_EXEC_FLAG: Final[str] = "--exec"
_EXEC_ENV: Final[str] = "ASSAY_EXEC_TARGET"


# --- [OPERATIONS] -----------------------------------------------------------------------


def _extract_exec(tokens: tuple[str, ...]) -> tuple[tuple[str, ...], str | None]:
    """Split a leading-or-inline ``--exec <target>`` global flag out of the raw token stream.

    The flag admits ``--exec ssh://...`` (two tokens), ``--exec=ssh://...`` (one token), and ``--exec local`` / ``--exec ""``
    for the local case. The first occurrence wins; the value is admitted downstream through the validated exec-target path.

    Returns:
        The token stream with the flag removed, and the extracted target value (``None`` when the flag is absent).
    """
    inline = next((i for i, token in enumerate(tokens) if token.startswith(f"{_EXEC_FLAG}=")), None)
    spaced = next((i for i, token in enumerate(tokens) if token == _EXEC_FLAG and i + 1 < len(tokens)), None)
    match (inline, spaced):
        case (int() as i, _) if spaced is None or i <= spaced:
            return (*tokens[:i], *tokens[i + 1 :]), tokens[i][len(_EXEC_FLAG) + 1 :]
        case (_, int() as i):
            return (*tokens[:i], *tokens[i + 2 :]), tokens[i + 1]
        case _:
            return tokens, None


def _admit_exec(tokens: tuple[str, ...]) -> tuple[str, ...]:
    # Extract --exec and admit it through the validated ASSAY_EXEC_TARGET env path so the flag is the primary surface and
    # the env var stays a fallback override; `local` normalizes to the empty (Local) value the modal value object admits.
    stripped, target = _extract_exec(tokens)
    if target is not None:
        os.environ[_EXEC_ENV] = "" if target == "local" else target  # noqa: TID251  # entrypoint admission: flag -> validated settings env path
    return stripped


def _no_command(tokens: tuple[str, ...]) -> bool:
    # Cyclopts maps incomplete commands to help/version callbacks; flags mark deliberate help/version intent.
    try:
        command, _bound, _ignored = app.parse_args(tokens, exit_on_error=False, print_error=False)
    except CycloptsError:
        return False
    return getattr(command, "__name__", "") in {"help_print", "version_print"} and not any(token in _HELP_TOKENS for token in tokens)


def _returncode(result: object) -> int:
    # Cyclopts returns values verbatim; exit hooks must be invoked here.
    match result:
        case int() as code:
            return code
        case _ if callable(hook := getattr(result, "__cyclopts_returncode__", None)):
            return int(hook())
        case _:
            return 0


def _dispatch(tokens: tuple[str, ...]) -> object:
    match _no_command(tokens):
        case True:
            return parse_fault(tokens, "no command" if not tokens else f"incomplete command: {' '.join(tokens)}")
        case _:
            try:
                return app(tokens, result_action="return_value", backend="asyncio", exit_on_error=False, print_error=False)
            # Cyclopts only formats when printing; stdout Envelope folding lives at this boundary.
            except CycloptsError as parse_error:
                return parse_fault(tokens, str(parse_error))
            except ValidationError as config_error:
                # Settings construction faults become config-step Envelopes.
                return parse_fault(tokens, str(config_error), step=Step.CONFIG)
            except Exception as exc:  # noqa: BLE001  # CLI boundary: preserve single-Envelope stdout contract for unexpected dispatch faults
                return parse_fault(tokens, f"{type(exc).__name__}: {exc}", step=Step.DISPATCH)


# --- [COMPOSITION] ----------------------------------------------------------------------

app: Final = build_app(REGISTRY)
_HELP_TOKENS: Final[frozenset[str]] = frozenset((*app.help_flags, *app.version_flags))


@app.meta.default
def meta(*tokens: Annotated[str, Parameter(show=False)]) -> int:
    """Resolve CLI tokens to the returned Envelope exit code.

    Returns:
        Process exit code derived from the emitted Envelope.
    """
    return _returncode(_dispatch(tokens))


def main(argv: list[str] | None = None) -> int:
    """Run the CLI, then force-flush and shut down the tracer provider before exit.

    Args:
        argv: Explicit token vector; ``None`` reads the current process arguments.

    Returns:
        Process exit code derived from the dispatched Envelope.
    """
    configure_logging()  # explicit call aligns subprocess and import-time logging state
    # Scrub surrogateescape tokens so untrusted argv cannot crash the wire encoder, then admit the agent-first --exec flag.
    tokens = _admit_exec(tuple(wire_safe(token) for token in (sys.argv[1:] if argv is None else argv)))

    def _install() -> None:
        try:
            install_tracing(AssaySettings().otel_endpoint)
        except ValidationError as exc:
            sys.stderr.write(f"assay: tracing disabled (config invalid): {exc}\n")

    def _drain(provider: object) -> None:
        # Bound flush before releasing SDK workers so a slow OTLP endpoint cannot stall exit.
        match getattr(provider, "force_flush", None):
            case flush if callable(flush):
                try:
                    flush(_DRAIN_MS)
                except Exception as exc:  # noqa: BLE001  # lifecycle boundary: tracing errors go to stderr, not stdout
                    sys.stderr.write(f"assay: trace force_flush failed: {exc}\n")
            case _:
                pass
        match getattr(provider, "shutdown", None):
            case shutdown if callable(shutdown):
                try:
                    shutdown()
                except Exception as exc:  # noqa: BLE001  # lifecycle boundary: tracing errors go to stderr, not stdout
                    sys.stderr.write(f"assay: trace shutdown failed: {exc}\n")
            case _:
                pass

    try:
        _install()
        return meta(*tokens)
    finally:
        _drain(get_tracer_provider())


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["main"]

# --- [ENTRY] ----------------------------------------------------------------------------

if __name__ == "__main__":
    raise SystemExit(main())
