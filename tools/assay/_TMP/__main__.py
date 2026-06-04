"""Entrypoint: the command tree is a projection of ``REGISTRY``; it owns no verb, flag, or sub-app.

The exit code is never computed here — it originates from ``RailStatus.exit_code`` via
``Envelope.__cyclopts_returncode__`` and is read back by ``resolve_returncode``.

``meta`` is one expression, not a ``match``: under ``result_action="return_value"`` Cyclopts handles
``--help``/``--version`` inside the call, rendering the text to stdout and returning ``None``. Since
``resolve_returncode(None) == 0``, a defensive ``case None`` arm would be dead — the help/version path is
Envelope-free by construction, and the one-Envelope invariant covers matched rails only. ``backend="asyncio"``
seats the sole event loop here; rails own ``anyio.run`` once inside ``run_check`` (never nested).

``main`` wraps dispatch in ``try/finally`` so the OTel ``BatchSpanProcessor`` queue drains before the
process returns. The global accessor yields the SDK provider when ``__init__.py``'s endpoint gate installed
one and the API ``ProxyTracerProvider`` otherwise; the proxy exposes no ``force_flush`` (opentelemetry
1.41.1), so the drain reads the bound method off the provider with an immediate-return identity fallback,
keeping the flush unconditional and safe on every path. The ``try/finally`` is span-drain plumbing only: it
has no ``except``, so pre-rail parse errors stay Envelope-free, written to stderr by Cyclopts before ``meta``.

Importing ``composition.registry`` triggers the package ``__init__.py``, which — before ``meta`` ever
dispatches — binds the settings ``agent_context`` (``{run.id, agent.task.id}`` from
``ASSAY_RUN_ID``/``ASSAY_AGENT_TASK_ID``) into the process-global structlog ContextVar and into the OTel
``Resource``, so every log line, span, and ``Envelope`` this entrypoint drives correlates to its driving
agent task with zero CLI flags; the drain below egresses those already-correlated spans.
"""

import sys
from typing import Final, TYPE_CHECKING

from cyclopts._result_action import resolve_returncode  # noqa: PLC2701  # lazy cyclopts re-export is untyped; concrete module is typed -> int
from opentelemetry.trace import get_tracer_provider

from tools.assay._TMP.composition.registry import build_app, REGISTRY  # noqa: PLC2701  # intra-staging import; _TMP is the package root


if TYPE_CHECKING:
    from collections.abc import Callable


# --- [COMPOSITION] ----------------------------------------------------------------------


app: Final = build_app(REGISTRY)  # self-test + auto attached on root inside build_app, beside the claim sub-apps


@app.meta.default  # the meta app wraps EVERY command result before exit
def meta(*tokens: str) -> int:
    return resolve_returncode(app(tokens, result_action="return_value", backend="asyncio"))


def main(argv: list[str] | None = None) -> int:
    """Drive the ``meta`` dispatch, draining OTel spans before returning.

    ``argv is None`` forwards ``sys.argv[1:]`` so ``python -m`` dispatches a verb, while an explicit ``[]``
    is splayed as-is (root help, exit 0). A malformed token set raises ``SystemExit`` from Cyclopts' own
    parse before any ``Envelope`` is built — propagated through the ``finally`` so the drain still runs.
    The ``finally`` bounds the ``BatchSpanProcessor`` drain at 5 s (matching its default ``schedule_delay``)
    so a short-lived CLI never exits before its spans export; ``force_flush`` is read off the provider with
    an identity fallback, making the drain a zero-cost return when tracing is gated off.
    """
    flush: Callable[[int], bool] = getattr(get_tracer_provider(), "force_flush", lambda _timeout_millis: True)
    try:
        return meta(*(sys.argv[1:] if argv is None else argv))  # None -> sys.argv[1:] (python -m); [] -> splayed empty (root help)
    finally:
        flush(5000)  # drain the BatchSpanProcessor queue before the process returns


if __name__ == "__main__":
    raise SystemExit(main())
