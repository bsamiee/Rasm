# Test Infrastructure — Known Issues & Backlog

Open issues only. **Every item is verified real** (reproduced, or confirmed against installed-library source) — nothing speculative. Companion: `.cache/testing-infra-research-ledger.md` holds the full research delta and the rejected-with-rationale list (frozendict, schemathesis, CrossHair-as-dep, atheris, codspeed, ghostwriter — declined on portability/theater grounds; do not re-propose).

The 2026-06-09 wave closed and removed several entries. The cross-package structlog global-config clash is **root-fixed**: one live-resolving config owner now lives in `tools/assay/_logging.py` (`configure_logging` installs a `_StderrLogger` that resolves `sys.stderr` per write under a lock, never binding the stream at config time), both CLI entrypoints call it once via an install-once latch, and the per-test `_configure(LogFormat.CI)` band-aid is deleted — full-suite clean across seeds 1001/1002/1003/7/42/999. The `RailProbe.install` fan_out drop is fixed (a tuple-payload arm now promotes to `FanOut`). The `tests/_bench.py` skip-before-fail concern is affirmed intentional, not theater: the session-end `pytest_benchmark_update_json` Potts/BIC hook is a real implemented regression gate (greedy piecewise-constant fit + `pytest.fail` on sustained level shift), confirmed by source read. Two in-place polymorphic collapses landed (`rails/api.py` `_span_capture` shared tree-sitter window projection, −24 LOC; `automation/engine.py` `_co_resident` byte-identical closure hoist, −7 LOC). A repo-root litter-containment guard law was added (`test_pytest_policy.py`); the stale root `.hypothesis`/`.benchmarks` dirs were leftovers, not active leaks — the in-process homes (`.cache/hypothesis`, `.artifacts/python/benchmarks`) are correctly honored and the `--benchmark-storage=file://.artifacts/...` URI parses correctly (pytest-benchmark strips `file://`).

The prior 2026-06 wave closed: asyncssh ↔ cryptography, the `_strategies.py` `Meta`-drop gaps (`NamedTupleType`, `_fbound` Decimal precision, `_text` empty-window), the `de_eq` fuzzy-eq adapter, `mypy` as a green second type gate, the CLI entry-point smoke laws, and the unconsumed `tools/assay/composition/pipeline.py` materialization-pipeline registry.

## Known bugs (open)

### [HIGH] Mutation lane — stats-phase abort on CPython 3.15.0b1
`mutmut run` clears "no covered mutants" (the `relative_files=false` fix in `.config/coverage-mutmut.ini` is verified) and enters the stats phase, then aborts with `BadTestExecutionCommandsException` — pytest **exit 4** (usage error) under mutmut-3's `change_cwd(mutants/)` stats pass. The *identical* pytest command run manually returns 0 — so pytest rejected its own invocation, almost certainly a missing `--rootdir`/`--import-mode` flag in mutmut's stats invocation under the changed cwd.
- **Upstream status (verified 2026-06-09)**: `mutmut==3.6.0` is the latest PyPI release; no newer version addresses the stats-phase argv. The defect is mutmut-internal (the argv it constructs under `mutants/`), not local config — keep documented, do not hand-roll a fork.
- **Action**: file an upstream mutmut issue with the exact pytest argv it constructs under `mutants/`. The kill-rate gate (`tools/quality/mutation_kill_rate.py`) is structurally complete and binds mutmut's own result surface — it goes live the moment the stats pass runs.
- Prereqs already in place: Rust toolchain + `PYO3_USE_ABI3_FORWARD_COMPATIBILITY=1` for libcst's source build; `-o required_plugins=`, `-m "not network"`, the subprocess-test deselect, and the ty-prefilter disable are all in `[tool.mutmut]`.

## Code smells / weak sections (low severity)

### `tests/_seams.py` / assay conftest
- **`SeamProbe.projected[K]` unused** in the assay specialization (`commands`/`checks` read `captured` directly). Retained as a real engine affordance (post-hoc `calls`-log view distinct from the per-call `project` stream) — flag so a reviewer does not strip it as dead.
- **`loopback_server` ty suppression (narrowed, irreducible)** — `ssh_loopback` now carries only `# ty: ignore[invalid-argument-type]` (a typed `_port` reader removed the former `unresolved-attribute` code and keeps `get_port` checked). The original backlog diagnosis ("Awaitable invariance") was **wrong**: the real cause is that ty cannot bind a `@asynccontextmanager`-decorated generic's type parameter `S` at the call site (the decorated callable is non-subscriptable and `S` stays opaque — proven empirically, unaffected by `Awaitable`→`Coroutine` or S-in-return retypes). Full removal would require replacing `@asynccontextmanager` with a hand-rolled generic async-CM class, reintroducing the teardown/ResourceWarning risk the helper exists to prevent — not warranted for one fixture.

### `tests/_strategies.py`
- **MemoryViewType identity asymmetry** — `st.binary().map(memoryview)` round-trips by equality (msgspec decodes to `bytes`), but a `type()`/identity oracle on a memoryview field would observe the asymmetry. No consumer compares by identity and no memoryview wire field exists, so it is currently inert; a one-line note marks the arm.

### Organization / docs audit residual (2026-06-09, audit-confirmed, unfixed)
A full CLAUDE.md §6 + docstring audit ran (51 files; 32 already clean). All non-conformant section dividers were normalized to the bracketed-UPPERCASE-snake, column-90 grammar. The following audit-confirmed items were **not** applied (deferred; each is real, none speculative):
- `tools/assay/core/engine.py`: `_LOG` / `_TRACER` are filed under `[CONSTANTS]` but logger/tracer handles are `[SERVICES]` (siblings `routing.py` / `aspect.py` file them correctly).
- `tools/assay/composition/registry.py`: `[MODELS]` precedes `[CONSTANTS]`; only `_PROBE_DECODER` is dependency-forced after `_ProbeRow` (it decodes `dict[str, _ProbeRow]`), so the pure constants could lead with the decoder relocated to a `[TABLES]` section.
- `tools/assay/composition/catalog.py`: `select` (an operation) sits under the `[TABLES]` divider with no `[OPERATIONS]` divider.
- `tests/tools/py_analyzer/test_analyzer.py`: no section dividers at all (directory convention is full `[RUNTIME_PRELUDE]`/`[TYPES]`/`[CONSTANTS]`/`[OPERATIONS]`).
- `tools/assay/rails/bridge.py:190`: stale roadmap narration ("full VerifyScenario decoding lands in Wave E") in an otherwise-valid invariant comment.
- `tools/assay/rails/api.py:93`: provenance restatement comment duplicating the `_RESULT_CAP` import.
- `tests/tools/assay/test_main.py`: module docstring carries process/anti-theater narration rather than caller-visible scope only.
- `tests/conftest.py:173`: `[EXPORTS]` divider over pytest fixtures/hooks (no `__all__` surface; should be `[COMPOSITION]`).
- `tests/tools/assay/core/test_aspect.py:73`: redundant adjacent `[LAWS]` + `[SLOT]` dividers.
- `tests/tools/assay/composition/test_registry.py:77`: `[MODELS]` divider over a shared test-double/harness block (a `[HARNESS]` extension names it better).

## Deferred (real, intentionally held — not yet warranted)

- **`model_based` inline stateful driver** — `Bundle`/`multiple`/`consumes`/`invariant` are re-exported from `tests._spec`. A `run_state_machine_as_test` binding helper / inline driver remains deferred until a machine needs `invariant(check_during_init=False)` or multi-machine composition.
- **`register_random`** — no private `random.Random` seam exists in the engines; the `tests/conftest.py` comment names the trigger + the hypothesis-weakref gotcha. Call `register_random(rng)` (strong ref) the moment such a seam is introduced.
- **Coverage per-test contexts** — `--cov-context=test` is documented opt-in (pyproject) but not in default addopts (coverage's dynamic context hard-errors under `--cov` and blocks `-n auto`). Wire it if per-test line attribution becomes worth the constraint.
- **CrossHair symbolic lane** — `rasm-symbolic` profile (`@settings(backend="crosshair")`) is documented in conftest; enable when a cp315 `crosshair-tool` wheel ships (Hypothesis backend, conftest-only — zero engine change).
- **`tools.assay` package-init coverage residual** — the import-time bootstrap-except branch (malformed `ASSAY_*` env, `tools/assay/__init__.py`) and the HUMAN-renderer (tty-stderr) arm in `tools/assay/_logging.py` `configure_logging` are only reachable under a fresh interpreter / different process state, not in-process; in-process smoke laws cannot lift them. (The exact 86%/98% figures predate the 2026-06-09 structlog relocation and need a re-measure.) Lift only if subprocess coverage (`COVERAGE_PROCESS_START`) is ever wired.

## Big refinements (verified real)

1. **Unblock the empirical mutation guardrail** — the single highest-value follow-up (see the mutmut bug above). Converts the structural non-regression proof into a live kill-rate gate.
