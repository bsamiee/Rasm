"""Project the assay registry into the CLI command tree."""

import sys
from typing import Final, TYPE_CHECKING

from cyclopts._result_action import resolve_returncode  # noqa: PLC2701  # lazy cyclopts re-export is untyped; concrete module is typed -> int
from cyclopts.exceptions import CycloptsError
from opentelemetry.trace import get_tracer_provider

from tools.assay.composition.registry import build_app, parse_fault, REGISTRY


if TYPE_CHECKING:
    from collections.abc import Callable


# --- [COMPOSITION] ----------------------------------------------------------------------


app: Final = build_app(REGISTRY)  # self-test + auto attached on root inside build_app, beside the claim sub-apps


@app.meta.default  # the meta app wraps EVERY command result before exit
def meta(*tokens: str) -> int:
    """Resolve CLI tokens to a process exit code.

    Args:
        *tokens: Command-line tokens excluding the executable name.

    Returns:
        The exit code exposed by the returned `Envelope`.
    """
    try:
        result = app(tokens, result_action="return_value", backend="asyncio", exit_on_error=False, print_error=False)
    except CycloptsError as parse_error:
        result = parse_fault(tokens, str(parse_error))
    return resolve_returncode(result)


def main(argv: list[str] | None = None) -> int:
    """Run the CLI and flush tracing before returning.

    Args:
        argv: Command tokens to dispatch. `None` forwards `sys.argv[1:]`.

    Returns:
        The process exit code.
    """
    # force_flush read off the provider with an identity fallback: zero-cost return when tracing is gated off.
    flush: Callable[[int], bool] = getattr(get_tracer_provider(), "force_flush", lambda _timeout_millis: True)
    try:
        return meta(*(sys.argv[1:] if argv is None else argv))
    finally:
        flush(5000)  # 5 s bound matches BatchSpanProcessor default schedule_delay: spans export before exit


if __name__ == "__main__":
    raise SystemExit(main())
