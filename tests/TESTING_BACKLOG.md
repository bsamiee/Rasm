# Test Infrastructure — Known Issues & Backlog

Curated from the 2026-06 testkit refactor. **Every item was verified real** (reproduced, or confirmed against installed-library source) — nothing speculative. Companion: `.cache/testing-infra-research-ledger.md` holds the full research delta, the 19 implemented findings, and the rejected-with-rationale list (frozendict, schemathesis, CrossHair-as-dep, atheris, codspeed, ghostwriter — all declined on portability/theater grounds; do not re-propose).

## Known bugs (verified — need fixing)

### [HIGH] Mutation lane — stats-phase abort on CPython 3.15.0b1
`mutmut run` now clears "no covered mutants" (the `relative_files=false` fix in `.config/coverage-mutmut.ini` is verified) and enters the stats phase, then aborts with `BadTestExecutionCommandsException` — pytest **exit 4** (usage error) under mutmut-3's `change_cwd(mutants/)` stats pass. The *identical* pytest command run manually returns 0 (1035 passed). Exit 4 = pytest rejected its own invocation (not a test failure, not the earlier frozen-bootstrap guess) — almost certainly a missing `--rootdir`/`--import-mode` flag in mutmut's stats invocation under the changed cwd.
- **Action**: file an upstream mutmut issue with the exact pytest argv it constructs under `mutants/`. The kill-rate gate (`tools/quality/mutation_kill_rate.py`) is structurally complete and binds mutmut's own result surface — it goes live the moment the stats pass runs. This unblocks the empirical kill-rate guardrail (Wave 4 was accepted on the structural non-regression proof instead).
- Prereqs already in place: Rust toolchain + `PYO3_USE_ABI3_FORWARD_COMPATIBILITY=1` for libcst's source build; `-o required_plugins=`, `-m "not network"`, the subprocess-test deselect, and the ty-prefilter disable are all in `[tool.mutmut]`.

### [MED] asyncssh ↔ cryptography incompatibility (network lane)
`asyncssh.generate_private_key("ssh-ed25519")` (the `ssh_loopback` fixture) raises `KeyGenerationError: Encryption algorithm must be a KeySerializationEncryption instance` — a cryptography API change the installed asyncssh calls incorrectly. **Latent**: the suite deselects `-m network`, so it never fires — but the ssh loopback laws cannot actually run under the current pins.
- **Action**: bump `asyncssh` (or pin `cryptography`) and re-run `-m network` to confirm the loopback laws pass.

## Code smells / weak sections (verified — low severity)

### `tests/_strategies.py`
- **NamedTupleType drops `Annotated` per-element constraints** — `NamedTupleType -> resolve(cls)` falls to `st.from_type`, which ignores `msgspec.Meta`. A field typed `Annotated[int, Meta(ge=...)]` inside a NamedTuple generates unconstrained values. No current model exercises it. Fix: add a `NamedTupleType` case in `resolve` mirroring the `StructType` `st.builds(cls, **{f.name: _node(f.type)})` path (`.fields` is isomorphic). *This is the last msgspec node that still drops `Meta`.*
- **`_fbound` float coercion on Decimal bounds** — returns `float`, so a high-precision Decimal bound (`le=Decimal("0.1")`, `decimal_places>=15`) can drift one ULP and admit a boundary-violating value. Exact for the verified `decimal_places<=2` cases. Fix: thread `Decimal` unconverted (`float | Decimal | None`) at the cost of the shared float-arm typing.
- **`_text` empty-language hazard** — `from_regex(pattern).filter(lo<=len<=hi)` silently exhausts the filter (→ `filter_too_much` health-check) when a fixed-length regex can't satisfy the length window, instead of returning a clean `st.nothing()`. No current pattern triggers it.
- **MemoryViewType identity asymmetry** — `st.binary().map(memoryview)` round-trips by equality (msgspec decodes to `bytes`) but a `type()`/identity oracle on a memoryview field would see the asymmetry.

### `tests/_bench.py`
- **Skip-before-fail ordering** — the dispersion floor (`pytest.skip` on `rel_iqr > max_rel_iqr`) runs *before* the absolute-budget assertion, so an over-budget-AND-noisy row is skipped, never failed. Intended ("untrustworthy → skip"), but the per-run absolute gate is unenforced for chronically-noisy subjects; their only gate is the session-end Potts/BIC regression hook.

### `tests/_seams.py` / assay conftest
- **`RailProbe.install` fan_out drop** — the zero-edit assay shim promotes `member -> Shape` with `case _ -> Sync`; the original `fan_out` arm is gone. Verified dead (no law routes `fan_out` through `install`; rail laws use direct `monkeypatch.setattr`). But a *future* law installing a new async/factory member via `install()` without extending the promotion match silently gets a `Sync` double. The engine's `SeamProbe` keeps the `FanOut` variant; only the assay shim omits it.
- **`SeamProbe.projected[K]` unused** in the assay specialization (`commands`/`checks` read `captured` directly). Retained as a real engine affordance (post-hoc `calls`-log view distinct from the per-call `project` stream) — not dead scaffolding; flag so a reviewer doesn't strip it.
- **`loopback_server` ty Awaitable-invariance suppression** — `ssh_loopback` carries `# ty: ignore[invalid-argument-type, unresolved-attribute]` (S=SSHAcceptor through `Awaitable` invariance). If a second consumer appears, type `listen` as `Callable[[], Coroutine[object, object, S]]` + an `async def` overload to bind `S` cleanly and drop the per-site suppression.

## Deferred (real, intentionally held — not yet warranted)

- **`model_based` stateful expansion** — re-export Bundle/`multiple`/`consumes`/`invariant` + a `run_state_machine_as_test` binding helper. The passthrough is intentional; defer until a machine needs `invariant(check_during_init=False)` or multi-machine composition.
- **`dirty-equals` fuzzy-eq adapter for `_spec`** — a sound ~3-line `de_eq` adapter, but no current law needs fuzzy equality the existing `eq` callable can't express.
- **Coverage per-test contexts** — `--cov-context=test` is documented opt-in (pyproject) but not in default addopts (coverage's dynamic context hard-errors under `--cov` and blocks `-n auto`). Wire it if per-test line attribution becomes worth the constraint.
- **`register_random`** — no private `random.Random` seam exists in the engines; call it (strong ref) the moment one is introduced.
- **CrossHair symbolic lane** — `rasm-symbolic` profile (`@settings(backend="crosshair")`) is documented in conftest; enable when a cp315 `crosshair-tool` wheel ships (Hypothesis backend, conftest-only — zero engine change).

## Big refinements (verified real)

1. **Unblock the empirical mutation guardrail** — the single highest-value follow-up (see the mutmut bug above). Converts the structural non-regression proof into a live kill-rate gate.
2. **`resolve()` NamedTupleType constraint case** — close the last constraint-honoring gap in the unified Meta-algebra.
3. **CLI entry-point coverage** — `tools/assay/__main__.py` (74%) and `__init__.py` (86%) drag below the 95 floor (TOTAL holds at 98%). Add in-process smoke laws for the bootstrap/tracing paths if entry-point coverage matters.
4. **mypy as a second type gate** — `ty` is binding today (`[tool.mypy]` documents this). If mypy is ever wired in, 2 pre-existing HEAD errors (`test_rail_package.py:376` explicit-any, `test_rail_code.py:280` type-arg) plus the session's 14 verified-non-defect dialect divergences (TypeForm, no-for-loop comprehensions, `disallow_any` on test Callables) need a scoped `[[tool.mypy.overrides]]` for `tests/**`, *not* code contortion.
