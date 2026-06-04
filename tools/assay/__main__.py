"""CLI entrypoint: the command tree is a projection of ``REGISTRY``, owning no verb, flag, or sub-app."""

import sys
from typing import Final, TYPE_CHECKING

from cyclopts._result_action import resolve_returncode  # noqa: PLC2701  # lazy cyclopts re-export is untyped; concrete module is typed -> int
from opentelemetry.trace import get_tracer_provider

from tools.assay.composition.registry import build_app, REGISTRY  # intra-package import; tools.assay is the package root


if TYPE_CHECKING:
    from collections.abc import Callable


# --- [COMPOSITION] ----------------------------------------------------------------------


app: Final = build_app(REGISTRY)  # self-test + auto attached on root inside build_app, beside the claim sub-apps


@app.meta.default  # the meta app wraps EVERY command result before exit
def meta(*tokens: str) -> int:
    """Resolve every command's returned ``Envelope`` to its process exit code."""
    return resolve_returncode(app(tokens, result_action="return_value", backend="asyncio"))


def main(argv: list[str] | None = None) -> int:
    """Drive ``meta`` dispatch, draining OTel spans in a ``finally`` before returning.

    Args:
        argv: ``None`` forwards ``sys.argv[1:]`` (``python -m``); an explicit ``[]`` is splayed as-is
            (root help, exit 0).

    Returns:
        The process exit code. A malformed token set raises ``SystemExit`` from Cyclopts' own parse
        before any ``Envelope`` is built, propagated through the ``finally`` so the drain still runs.
    """
    # force_flush read off the provider with an identity fallback: zero-cost return when tracing is gated off.
    flush: Callable[[int], bool] = getattr(get_tracer_provider(), "force_flush", lambda _timeout_millis: True)
    try:
        return meta(*(sys.argv[1:] if argv is None else argv))
    finally:
        flush(5000)  # 5 s bound matches BatchSpanProcessor default schedule_delay: spans export before exit


if __name__ == "__main__":
    raise SystemExit(main())
