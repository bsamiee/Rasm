"""CLI entrypoint: the command tree is a projection of ``REGISTRY``, owning no verb, flag, or sub-app."""

import sys
from typing import Final, TYPE_CHECKING

from cyclopts._result_action import resolve_returncode  # noqa: PLC2701  # lazy cyclopts re-export is untyped; concrete module is typed -> int
from cyclopts.exceptions import CycloptsError
from opentelemetry.trace import get_tracer_provider

from tools.assay.composition.registry import build_app, parse_fault, REGISTRY  # intra-package import; tools.assay is the package root


if TYPE_CHECKING:
    from collections.abc import Callable


# --- [COMPOSITION] ----------------------------------------------------------------------


app: Final = build_app(REGISTRY)  # self-test + auto attached on root inside build_app, beside the claim sub-apps


@app.meta.default  # the meta app wraps EVERY command result before exit
def meta(*tokens: str) -> int:
    """Resolve every command's returned ``Envelope`` to its process exit code.

    ``exit_on_error=False``/``print_error=False`` make Cyclopts RAISE a ``CycloptsError`` (unknown
    command/option) instead of printing a bare Rich panel and calling ``sys.exit(1)`` — so the boundary
    folds it through ``parse_fault`` into the canonical Fault ``Envelope`` (with structured
    ``error_context``) every rail emits, never a bare Rich panel. A SURPLUS POSITIONAL is NOT a Cyclopts
    error (the variadic ``paths`` sink absorbs it); the verb's ``BaseParams.bound`` arity contract rejects
    it inside ``rail.run`` and folds the SAME ``parse`` ``failing_step`` — one taxonomy, two raise sites.
    """
    try:
        result = app(tokens, result_action="return_value", backend="asyncio", exit_on_error=False, print_error=False)
    except CycloptsError as parse_error:
        result = parse_fault(tokens, str(parse_error))
    return resolve_returncode(result)


def main(argv: list[str] | None = None) -> int:
    """Drive ``meta`` dispatch, draining OTel spans in a ``finally`` before returning.

    Args:
        argv: ``None`` forwards ``sys.argv[1:]`` (``python -m``); an explicit ``[]`` is splayed as-is
            (root help, exit 0).

    Returns:
        The process exit code. A malformed token set is folded into a structured Fault ``Envelope`` by
        ``meta`` (never a bare ``SystemExit``/Rich panel); the ``finally`` still drains spans on exit.
    """
    # force_flush read off the provider with an identity fallback: zero-cost return when tracing is gated off.
    flush: Callable[[int], bool] = getattr(get_tracer_provider(), "force_flush", lambda _timeout_millis: True)
    try:
        return meta(*(sys.argv[1:] if argv is None else argv))
    finally:
        flush(5000)  # 5 s bound matches BatchSpanProcessor default schedule_delay: spans export before exit


if __name__ == "__main__":
    raise SystemExit(main())
