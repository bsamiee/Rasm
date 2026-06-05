"""Expose the Assay registry as the CLI entrypoint."""

import sys
from typing import Final, TYPE_CHECKING

from cyclopts._result_action import resolve_returncode  # noqa: PLC2701  # lazy cyclopts re-export is untyped; concrete module is typed -> int
from cyclopts.exceptions import CycloptsError
from opentelemetry.trace import get_tracer_provider

from tools.assay.composition.registry import build_app, parse_fault, REGISTRY


if TYPE_CHECKING:
    from collections.abc import Callable


# --- [COMPOSITION] ----------------------------------------------------------------------


app: Final = build_app(REGISTRY)


@app.meta.default
def meta(*tokens: str) -> int:
    """Resolve CLI tokens to the returned Envelope exit code.

    Returns:
        Process exit code derived from the emitted Envelope.
    """
    try:
        result = app(tokens, result_action="return_value", backend="asyncio", exit_on_error=False, print_error=False)
    except CycloptsError as parse_error:
        result = parse_fault(tokens, str(parse_error))
    return resolve_returncode(result)


def main(argv: list[str] | None = None) -> int:
    """Run the CLI and flush tracing before exit.

    Returns:
        Process exit code returned by the CLI dispatcher.
    """
    # The fallback keeps tracing opt-in without branching the exit path.
    flush: Callable[[int], bool] = getattr(get_tracer_provider(), "force_flush", lambda _timeout_millis: True)
    try:
        return meta(*(sys.argv[1:] if argv is None else argv))
    finally:
        flush(5000)


if __name__ == "__main__":
    raise SystemExit(main())
