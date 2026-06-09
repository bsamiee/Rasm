"""Expose the Assay registry as the CLI entrypoint."""

import sys
from typing import Final

from cyclopts.exceptions import CycloptsError
from opentelemetry.trace import get_tracer_provider
from pydantic import ValidationError

from tools.assay import (
    _DRAIN_MS,  # noqa: PLC2701  # intra-package private import: one operator-chosen ~1.5s bound shared with the BatchSpanProcessor cadence
    configure_logging,
    install_tracing,
)
from tools.assay.composition.registry import build_app, parse_fault, REGISTRY
from tools.assay.composition.settings import AssaySettings
from tools.assay.core.model import wire_safe


# --- [COMPOSITION] ----------------------------------------------------------------------


app: Final = build_app(REGISTRY)
# Cyclopts routes no resolvable command to help_print (stdout help, return None); a flag token marks an explicit request.
_HELP_TOKENS: Final[frozenset[str]] = frozenset((*app.help_flags, *app.version_flags))


@app.meta.default
def meta(*tokens: str) -> int:
    """Resolve CLI tokens to the returned Envelope exit code.

    Returns:
        Process exit code derived from the emitted Envelope.
    """
    return _returncode(_dispatch(tokens))


def _dispatch(tokens: tuple[str, ...]) -> object:
    # Peek the resolved command before executing so the no-command auto-help never reaches stdout.
    # Explicit --help/--version carry a flag token and execute normally; a bare claim or empty argv folds to one FAULTED Envelope.
    match _no_command(tokens):
        case True:
            return parse_fault(tokens, "no command" if not tokens else f"incomplete command: {' '.join(tokens)}")
        case _:
            try:
                return app(tokens, result_action="return_value", backend="asyncio", exit_on_error=False, print_error=False)
            except CycloptsError as parse_error:
                return parse_fault(tokens, str(parse_error))
            except ValidationError as config_error:
                # A malformed ASSAY_* env raises when a verb constructs AssaySettings(); fold it to one config: Envelope.
                return parse_fault(tokens, str(config_error), step="config")
            except Exception as exc:  # noqa: BLE001  # final CLI boundary: preserve the one-Envelope stdout contract for unexpected dispatch faults
                return parse_fault(tokens, f"{type(exc).__name__}: {exc}", step="dispatch")


def _returncode(result: object) -> int:
    match result:
        case int() as code:
            return code
        case _ if callable(hook := getattr(result, "__cyclopts_returncode__", None)):
            return int(hook())
        case _:
            return 0


def _no_command(tokens: tuple[str, ...]) -> bool:
    # Cyclopts routes an unresolved verb to help_print/version_print (stdout help, return None); a flag token marks intent.
    try:
        command, _bound, _ignored = app.parse_args(tokens, exit_on_error=False, print_error=False)
    except CycloptsError:
        return False
    return getattr(command, "__name__", "") in {"help_print", "version_print"} and not any(token in _HELP_TOKENS for token in tokens)


def main(argv: list[str] | None = None) -> int:
    """Run the CLI and flush tracing before exit.

    Returns:
        Process exit code returned by the CLI dispatcher.
    """
    configure_logging()  # explicit replacement for the deleted import side effect; no-op once the package bootstrap configured
    # Scrub lone surrogates (os.fsdecode surrogateescape from invalid-UTF-8 argv) at the boundary so untrusted
    # tokens cannot crash the wire encoder; valid Unicode passes through unchanged.
    tokens = tuple(wire_safe(token) for token in (sys.argv[1:] if argv is None else argv))

    def _install() -> None:
        try:
            install_tracing(AssaySettings().otel_endpoint)
        except ValidationError as exc:
            _ = exc

    def _drain(provider: object) -> None:
        # Under a live OTLP endpoint this caps force_flush at _DRAIN_MS (~1.5s), then releases SDK workers.
        match getattr(provider, "force_flush", None):
            case flush if callable(flush):
                try:
                    flush(_DRAIN_MS)
                except Exception as exc:  # noqa: BLE001  # final lifecycle boundary: diagnostics go to stderr, never stdout
                    sys.stderr.write(f"assay: trace force_flush failed: {exc}\n")
            case _:
                pass
        match getattr(provider, "shutdown", None):
            case shutdown if callable(shutdown):
                try:
                    shutdown()
                except Exception as exc:  # noqa: BLE001  # final lifecycle boundary: diagnostics go to stderr, never stdout
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
