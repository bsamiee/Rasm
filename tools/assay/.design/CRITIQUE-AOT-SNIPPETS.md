# [H1][CRITIQUE_AOT_SNIPPETS]
>**Dictum:** *Wave 2 scores every AOT/aspect/cyclopts snippet against `coding-python` (5/10 = integration sketch; 12/10 = production spine). Wave 3 rewrites the spine only.*

Sources: `AOT.md`, `aspect.md`, `aot-architecture.md`, `aot-beartype.md`, `aot-otel.md`, `aot-stamina.md`, `aot-structlog.md`, `research-cyclopts-snippet.md`. Rubric: PEP 695 `**P` + `@wraps` + `id(dec)` idempotency; `Hom[**P,T]=Callable[P,Result[T,Fault]]`; `match`/`pipe`/`@effect.result` over `if`/`for`/`try`; registration as frozen tuple + algebraic fold; boundary `try` only with `RASM_BOUNDARY_EXEMPTION`.

---
## [1][PER_SNIPPET_SCORES]

| ID | Source (approx lines) | Score | Gap summary |
| --- | --- | :---: | --- |
| AOT-01 | `AOT.md` L29–36 `compose` | 7/10 | `reduce`+lambda; no `assemble`/monotonic `Result`; no `id(dec)` guard |
| AOT-02 | `AOT.md` L56–60 `checked` | 8/10 | Layer tuple OK; inner `lambda` lacks `@wraps`/marker attr |
| AOT-03 | `AOT.md` L84–89 `retried` | 6/10 | `isinstance` ladder; stamina `dec` not wrapped as `Hom` seam |
| ARCH-01 | `aot-architecture.md` L66–76 `compose` | 7/10 | Duplicate of AOT-01; add `Inversion` from `decorators.md` |
| ASP-01 | `aspect.md` L10–19 type aliases | 9/10 | Canonical `Hom`/`Layer`/`Spawn`; keep as Wave 3 header |
| ASP-02 | `aspect.md` L34–41 `compose` | 7/10 | Same as AOT-01 |
| ASP-03 | `aspect.md` L69–73 `_transient` | 6/10 | `isinstance` + union; use errno table + `match` |
| BEAR-01 | `aot-beartype.md` L27–32 `checked` | 8/10 | Align with ASP-01 `Layer` return; document `violation_type` |
| BEAR-02 | `aot-beartype.md` L55–58 illustrative `run_check` | 8/10 | Doc-only; production needs full `Spawn` lift |
| BEAR-03 | `aot-beartype.md` L69–72 strategy | 5/10 | `dict.get` branch; `match settings.beartype` + frozen map |
| BEAR-04 | `aot-beartype.md` L82–86 claw gate | 6/10 | Boundary OK; prefer `match os.environ["ASSAY_CLAW"]` arm table |
| OTEL-01 | `aot-otel.md` L12–28 `_install` | 9/10 | `match` gate; Wave 3: move install behind `aspect` import hook |
| OTEL-02 | `aot-otel.md` L36–53 `traced` | 10/10 | Reference spine: `wraps`, `match res`, `bound_contextvars` |
| OTEL-03 | `aot-otel.md` L66–72 fan-out | 4/10 | Imperative `for`+`enumerate`; `Block` indexed `start_soon` fold |
| STAM-01 | `aot-stamina.md` L10–22 `retried` | 8/10 | Factory OK; wire `RetryHook` in same `Layer` module |
| STAM-02 | `aot-stamina.md` L34–47 adapter | 6/10 | Bare `try/except`; rewrite as `@effect.async_result` + single `except*` at TG |
| LOG-01 | `aot-structlog.md` L12–33 `_configure` | 6/10 | `if ci else` renderer; `match cfg.log_format` + `Literal["ci","human"]` |
| LOG-02 | `aot-structlog.md` L43–66 `logged` | 8/10 | Strong `match res`; replace `(log.error if … else log.info)` with level table + `match fault.status` |
| LOG-03 | `aot-structlog.md` L88–92 fan-out | 4/10 | Same as OTEL-03 |
| LOG-04 | `aot-structlog.md` L102–105 `_check_ctx` | 9/10 | Bind-as-data; keep |
| SNIP-A | `research-cyclopts-snippet.md` L16–37 `_configure` | 6/10 | Duplicate LOG-01; `is_configured` early return is fine |
| SNIP-B | `research-cyclopts-snippet.md` L40–46 `Envelope` | 9/10 | Wire contract; no change |
| SNIP-C | `research-cyclopts-snippet.md` L48–107 `rail`/`_emit` | 4/10 | Per-invoke `AssaySettings()`; `Outcome.map` OK; missing `@effect.result` envelope path |
| SNIP-D | `research-cyclopts-snippet.md` L109–116 `_adapt` | 5/10 | `__annotations__` mutation; Cyclopts `Parameter` + frozen default OK; type via `typing.get_type_hints` boundary |
| SNIP-E | `research-cyclopts-snippet.md` L118–143 `__main__` | 4/10 | `for row in REGISTRY` / `setdefault`; `meta` uses `if result is None` |
| SNIP-F | `research-cyclopts-snippet.md` L163–165 probe | N/A | Bash probe; not Python spine |

**Aggregate:** 21 Python snippets; mean **6.8/10** (above 5/10 integration truth, **5.2 points** below 12/10 target). Best: OTEL-02, ASP-01, SNIP-B, LOG-04. Weakest cluster: registry/`__main__` folds (SNIP-C/D/E) + fan-out loops (OTEL-03, LOG-03).

---
## [2][WAVE3_REWRITE_DIRECTIVES]

### [2.i][COMPOSE_SPINE] — replaces AOT-01, ASP-02, ARCH-01

```python
# Target: decorators.md assemble law, assay Slot specialization
def compose[**P, T](*layers: Layer[P, T]) -> Callable[[Hom[P, T]], Hom[P, T]]:
    match assemble(layers):  # monotonic Slot fold → Result[dec, Inversion]
        case Ok(ordered): return lambda fn: pipe(ordered, block.fold(apply_layer, guard(fn)))
        case Error(inv): raise CompositionError(inv)  # decoration-time only
```

- Replace `sorted(..., reverse=True) + reduce` with `assemble`/`block.fold`; attach `frozenset[int]` of `id(dec)` on wrapper (`aspect.md` §6).
- `apply_layer`: `@wraps` + marker `__assay_layers__`.

### [2.ii][LAYER_FACTORIES] — checked, logged, traced, retried

| Factory | Directive |
| --- | --- |
| `checked` | Keep BEAR-01; return `(Slot.checked, dec)` where `dec` is `@wraps`-preserved `beartype(conf)(fn)`. |
| `logged` | Merge LOG-02 + OTEL correlation; `keys: Bind[P]`; terminal `match res` + `RailStatus → level` table (no inline ternary). |
| `traced` | Copy OTEL-02 verbatim; `attrs: Callable[P, Attrs]`. |
| `retried` | STAM-01 on `Spawn[**P,T]` only; never on rail `Hom`; register stamina `RetryHook` for `retry.attempts`. |

### [2.iii][REGISTRY_AND_MAIN] — SNIP-C/D/E

- **`build_app(registry: tuple[Bind,...]) -> App`:** one expression — `groupby(claim)` → nested `App` commands via `Map`/`TraverseM`, zero `for`/`setdefault` (`research-cyclopts-snippet.md` §2 table).
- **`rail(bind)`:** `@effect.result` generator: `settings` injected once (`Parameter(parse=False)` / meta `parse_args` ignored tuple per `research-cyclopts-api.md` §6); `ArtifactScope.open` as `bind`; `_emit` stays `outcome.map`/`default_with` on `Result[Report,Fault]`.
- **`meta`:** always `resolve_returncode(result)`; `match result: case None: 0 case env: resolve_returncode(env)`; leaves return `Envelope` only.
- **Stdout:** sole `msgspec.json.encode` in `_emit`; keep stderr structlog discipline (LOG-03 guards).

### [2.iv][ENGINE_BOUNDARY] — STAM-02 + OTEL-03 + LOG-03

- **`run_check`:** `@compose(checked(), traced(...), retried())` on `_spawn`; lift with `@effect.async_result` — no domain `try` except marked `except _TRANSIENT` at adapter (PYS0001).
- **Fan-out:** replace `for i, check in enumerate(checks)` with indexed `Block` + `tg.start_soon` via `Map(..., index)` + `TraverseM(identity)` pattern from `CLAUDE.md` §4.
- **`_transient`:** `match exc:` on closed exception set + `ResourceBusyError` arm; optional `errno` frozenset filter (STAM-06 open decision).

### [2.v][CONFIGURE_AND_GATES] — SNIP-A, LOG-01, BEAR-03/04, OTEL-01

- Renderer: `match cfg.log_format` not `if ci`.
- Beartype strategy: `match settings.beartype: case "off": O0 case _: O1`.
- Claw: env arm table at `__init__.py` boundary only.
- OTel: keep OTEL-01 `match (_GATE, provider)`; do not duplicate structlog configure in `main()`.

---
## [3][WAVE3_SPINE_PRIORITY]

| Priority | Deliverable | Replaces IDs |
| :---: | --- | --- |
| P0 | `core/aspect.py` — ASP-01 + OTEL-02 + LOG-02 + STAM-01 + BEAR-01 + compose §2.i | AOT-01/02/03, ASP-02, ARCH-01 |
| P1 | `composition/registry.py` — `rail`/`build_app`/`_emit` | SNIP-C/D |
| P2 | `__main__.py` — `build_app(REGISTRY)` + meta `match` | SNIP-E |
| P3 | `core/engine.py` — STAM-02 adapter + fan-out fold | STAM-02, OTEL-03, LOG-03 |
| P4 | `__init__.py` — LOG-01 + BEAR-04 | SNIP-A, BEAR-04 |

**Contract tests (Wave 3):** `tests/tools/assay/test_cyclopts_contract.py` — flatten tree, `__cyclopts_returncode__`, help→`None`→0 (`research-cyclopts-snippet.md` §2).

---
## [4][FURTHER_CONSIDERATION]

- **`compose` as `Result` vs trust sort:** open decision §5 in `aot-architecture.md` — Wave 3 should ship `assemble` even for four slots (cheap, prevents fifth-aspect regressions).
- **`Spawn` vs `Hom` typing:** `retried` must not be applied to rail handlers; ty needs separate `compose_spawn` or overload on `Layer` channel (`aspect.md` §1).
- **Claw vs OTel import order:** BEAR-04 claw must remain first statement when enabled; OTel install in `aspect.py` must not import model before claw in CI matrix.

**Paths:** `tools/assay/core/aspect.py`, `composition/registry.py`, `decorators.md` (`assemble`), `research-cyclopts-api.md` §6, `AUDIT.md` §3.
