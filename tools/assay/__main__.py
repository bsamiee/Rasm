"""CLI entrypoint that wires the Assay command registry into a Cyclopts meta-app.

Constructs the application at import time, routes argv through the meta dispatcher,
and drains the OpenTelemetry span buffer before process exit.
"""

import sys
from typing import Final

from cyclopts.exceptions import CycloptsError
from opentelemetry.trace import get_tracer_provider
from pydantic import ValidationError

from tools.assay import (
    _DRAIN_MS,  # noqa: PLC2701  # cross-module private: flush budget must match BatchSpanProcessor cadence
    configure_logging,
    install_tracing,
)
from tools.assay.composition.registry import build_app, parse_fault, REGISTRY
from tools.assay.composition.settings import AssaySettings
from tools.assay.core.model import wire_safe
from tools.assay.core.status import Step


# --- [OPERATIONS] -----------------------------------------------------------------------


def _no_command(tokens: tuple[str, ...]) -> bool:
    # Cyclopts routes bare or incomplete argv to `help_print`/`version_print` (return None); a flag token marks deliberate intent.
    try:
        command, _bound, _ignored = app.parse_args(tokens, exit_on_error=False, print_error=False)
    except CycloptsError:
        return False
    return getattr(command, "__name__", "") in {"help_print", "version_print"} and not any(token in _HELP_TOKENS for token in tokens)


def _returncode(result: object) -> int:
    # Load-bearing: cyclopts result_action="return_value" returns the result verbatim and never invokes __cyclopts_returncode__ itself.
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
            # Load-bearing: cyclopts error_formatter only console-prints (gated on print_error=True) and the error re-raises regardless,
            # so this catch is the sole path that can fold a parse failure into the single stdout Envelope.
            except CycloptsError as parse_error:
                return parse_fault(tokens, str(parse_error))
            except ValidationError as config_error:
                # Malformed ASSAY_* env var raises at AssaySettings() construction; fold to a config-step Envelope.
                return parse_fault(tokens, str(config_error), step=Step.CONFIG)
            except Exception as exc:  # noqa: BLE001  # CLI boundary: preserve single-Envelope stdout contract for unexpected dispatch faults
                return parse_fault(tokens, f"{type(exc).__name__}: {exc}", step=Step.DISPATCH)


# --- [COMPOSITION] ----------------------------------------------------------------------

app: Final = build_app(REGISTRY)
_HELP_TOKENS: Final[frozenset[str]] = frozenset((*app.help_flags, *app.version_flags))


@app.meta.default
def meta(*tokens: str) -> int:
    """Resolve CLI tokens to the returned Envelope exit code.

    Returns:
        Process exit code derived from the emitted Envelope.
    """
    return _returncode(_dispatch(tokens))


def main(argv: list[str] | None = None) -> int:
    """Run the CLI, then force-flush and shut down the tracer provider before exit.

    Returns:
        Process exit code derived from the dispatched command's Envelope.
    """
    configure_logging()  # explicit call: do not rely on import-time side effects for log configuration
    # Scrub lone surrogates (os.fsdecode surrogateescape from invalid-UTF-8 argv) so untrusted tokens cannot crash the wire encoder.
    tokens = tuple(wire_safe(token) for token in (sys.argv[1:] if argv is None else argv))

    def _install() -> None:
        try:
            install_tracing(AssaySettings().otel_endpoint)
        except ValidationError as exc:
            _ = exc

    def _drain(provider: object) -> None:
        # Budgets flush at _DRAIN_MS to avoid blocking exit under a slow OTLP endpoint, then releases SDK workers.
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
