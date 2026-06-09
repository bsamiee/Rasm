# Test Infrastructure — Known Issues & Backlog

Open issues only. **Every item is verified real** (reproduced, or confirmed against installed-library source) — nothing speculative. Companion: `.cache/testing-infra-research-ledger.md` holds the full research delta and the rejected-with-rationale list (frozendict, schemathesis, CrossHair-as-dep, atheris, codspeed, ghostwriter — declined on portability/theater grounds; do not re-propose).

The 2026-06 wave closed and removed several entries: asyncssh ↔ cryptography (verified fixed under current pins), the `_strategies.py` `Meta`-drop gaps (`NamedTupleType`, `_fbound` Decimal precision, `_text` empty-window), the `de_eq` fuzzy-eq adapter (shipped), `mypy` as a green second type gate (`[tool.mypy] files` + scoped overrides), and CLI entry-point smoke laws (`__main__.py` 74% → 94%). The unconsumed `tools/assay/composition/pipeline.py` materialization-pipeline registry + its test were removed (749 LOC of spec-as-code with zero production consumer), which also closed a latent `test_law_coverage_gate` failure.

## Known bugs (open)

### [HIGH] Mutation lane — stats-phase abort on CPython 3.15.0b1
`mutmut run` clears "no covered mutants" (the `relative_files=false` fix in `.config/coverage-mutmut.ini` is verified) and enters the stats phase, then aborts with `BadTestExecutionCommandsException` — pytest **exit 4** (usage error) under mutmut-3's `change_cwd(mutants/)` stats pass. The *identical* pytest command run manually returns 0 — so pytest rejected its own invocation, almost certainly a missing `--rootdir`/`--import-mode` flag in mutmut's stats invocation under the changed cwd.
- **Action**: file an upstream mutmut issue with the exact pytest argv it constructs under `mutants/`. The kill-rate gate (`tools/quality/mutation_kill_rate.py`) is structurally complete and binds mutmut's own result surface — it goes live the moment the stats pass runs.
- Prereqs already in place: Rust toolchain + `PYO3_USE_ABI3_FORWARD_COMPATIBILITY=1` for libcst's source build; `-o required_plugins=`, `-m "not network"`, the subprocess-test deselect, and the ty-prefilter disable are all in `[tool.mutmut]`.

### [MED] Cross-package structlog global-config ownership (`tools.quality` ↔ `tools.assay`) — mitigated, root fix open
`tools/assay/__init__.py` and `tools/quality/__main__.py` BOTH call `structlog.configure()` at import, each mutating the **global** structlog config. In the combined `tests` suite (one process), whichever configures last wins. `tools.quality` installs `PrintLoggerFactory(file=sys.stderr)`, eagerly binding `sys.stderr` at config time; under pytest fd-capture that handle is a transient capsys stream which later closes, so every subsequent assay rail log printed through the clobbered global factory crashes with `ValueError: I/O operation on closed file` (cascaded across ~22 laws under e.g. `--randomly-seed=1001`). Harmless in production (the two CLIs run in separate processes), real only in the combined test process.
- **Mitigation in place (band-aid)**: the assay `_isolate_sut_state` autouse fixture re-applies `tools.assay._configure(LogFormat.CI)` per assay test, restoring assay's factory + processors over any sibling-suite reset. Verified robust across 6 seeds (1001/1002/1003/7/42/999 → 1185 passed each).
- **Root fix (open)**: stop two packages globally clobbering one structlog config. Options: (a) a shared structlog-config owner the test session installs once; (b) per-package config isolation so each suite owns its own logger config; (c) neither package eagerly binds `sys.stderr` — resolve it live per write (also fixes a latent production broken-pipe / stderr-redirection robustness gap, esp. `tools.quality`'s eager `PrintLoggerFactory(file=sys.stderr)`). Until then, any NEW top-level test package that configures structlog can re-trigger the clash, and `tools.quality`'s own tests remain unprotected by the assay band-aid (not observed failing, but structurally exposed).

## Code smells / weak sections (low severity)

### `tests/_bench.py`
- **Skip-before-fail ordering** — the dispersion floor (`pytest.skip` on `rel_iqr > max_rel_iqr`) runs *before* the absolute-budget assertion, so an over-budget-AND-noisy row is skipped, never failed (intended: "untrustworthy → skip"). Consequence: the per-run absolute gate is unenforced for chronically-noisy subjects; their only gate is the session-end Potts/BIC regression hook.

### `tests/_seams.py` / assay conftest
- **`RailProbe.install` fan_out drop** — the assay shim promotes `member -> Shape` with `case _ -> Sync`; the original `fan_out` arm is gone. Verified dead today (no law routes `fan_out` through `install`). But a *future* law installing a new async/factory member via `install()` without extending the promotion match silently gets a `Sync` double. The engine `SeamProbe` keeps the `FanOut` variant; only the assay shim omits it.
- **`SeamProbe.projected[K]` unused** in the assay specialization (`commands`/`checks` read `captured` directly). Retained as a real engine affordance (post-hoc `calls`-log view distinct from the per-call `project` stream) — flag so a reviewer does not strip it as dead.
- **`loopback_server` ty Awaitable-invariance suppression** — `ssh_loopback` carries `# ty: ignore[invalid-argument-type, unresolved-attribute]` (S=SSHAcceptor through `Awaitable` invariance). If a second consumer appears, type `listen` as `Callable[[], Coroutine[object, object, S]]` + an `async def` overload to bind `S` cleanly and drop the per-site suppression.

### `tests/_strategies.py`
- **MemoryViewType identity asymmetry** — `st.binary().map(memoryview)` round-trips by equality (msgspec decodes to `bytes`), but a `type()`/identity oracle on a memoryview field would observe the asymmetry. No consumer compares by identity and no memoryview wire field exists, so it is currently inert; a one-line note marks the arm.

## Deferred (real, intentionally held — not yet warranted)

- **`model_based` inline stateful driver** — `Bundle`/`multiple`/`consumes`/`invariant` are re-exported from `tests._spec`. A `run_state_machine_as_test` binding helper / inline driver remains deferred until a machine needs `invariant(check_during_init=False)` or multi-machine composition.
- **`register_random`** — no private `random.Random` seam exists in the engines; the `tests/conftest.py` comment names the trigger + the hypothesis-weakref gotcha. Call `register_random(rng)` (strong ref) the moment such a seam is introduced. *(A future shared-testkit pass could generalize this into an auto-registering seam for any package's `random.Random` usage.)*
- **Coverage per-test contexts** — `--cov-context=test` is documented opt-in (pyproject) but not in default addopts (coverage's dynamic context hard-errors under `--cov` and blocks `-n auto`). Wire it if per-test line attribution becomes worth the constraint.
- **CrossHair symbolic lane** — `rasm-symbolic` profile (`@settings(backend="crosshair")`) is documented in conftest; enable when a cp315 `crosshair-tool` wheel ships (Hypothesis backend, conftest-only — zero engine change).
- **`__init__.py` entry-point coverage residual (86%)** — the import-time bootstrap-except branch (malformed `ASSAY_*` env) and the `_configure(LogFormat.HUMAN)` arm (tty stderr) are only reachable under a fresh interpreter / different process state, not in-process; in-process smoke laws cannot lift them. TOTAL holds at 98%. Lift only if subprocess coverage (`COVERAGE_PROCESS_START`) is ever wired.

## Big refinements (verified real)

1. **Unblock the empirical mutation guardrail** — the single highest-value follow-up (see the mutmut bug above). Converts the structural non-regression proof into a live kill-rate gate.
2. **Resolve the cross-package structlog config ownership** at the source (see the [MED] bug above) so the per-test band-aid can be removed and any future test package is safe.
