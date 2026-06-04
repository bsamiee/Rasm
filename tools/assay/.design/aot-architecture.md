# [H1][ASSAY_AOT_ARCHITECTURE]
>**Dictum:** *Decorators carry cross-cutting behavior; data carries registration. AOT style is adopted only where a concern is genuinely cross-cutting, and rejected wherever it would hide a row, a constant, or import-order state.*

Synthesis facet. The governing standard is `.claude/skills/coding-python/SKILL.md` and its `decorators.md`/`transforms.md` foundations. The skill scopes decorators to **cross-cutting composition** (`trace > authorize > validate > cache > govern > retry > operation`), demands **PEP 695 `**P` parameter-spec preservation**, mandates a **declared, monotonic, justified Slot order** (validated by an `assemble`/`compose` fold), and bans **god decorators, helper/def spam, single-caller extraction, and stringly-typed routing** in favor of **variable-driven dispatch over data tables**. Nothing in the skill endorses decorators for *registration*; its decorator doctrine is exclusively AOP.

---
## [1][DOCTRINE]
>**Dictum:** *Three AOT constructs, three verdicts: aspect decorators ADOPT, registration decorators REJECT, decorator factories ADOPT under the >=3-shape gate.*

| [CONSTRUCT] | [VERDICT] | [GROUNDING] |
| ----------- | --------- | ----------- |
| (a) Cross-cutting aspect decorators (`checked`/`logged`/`traced`/`retried`) | **ADOPT** | Skill "cross-cutting composition: decorator stacks"; AOP is the *one* sanctioned decorator use. Two seams only: `run_check`, rail runner. |
| (b) Registration decorators (`@rail`/`@tool`/`@parser`) building catalog/registry at import | **REJECT** | Violates "growth lands in a data row" (ARCH §3/§6, INVARIANT 3); hides data, couples to import order, defeats msgspec/snapshot/property laws. Skill favors data tables + variable-driven dispatch. |
| (c) Decorator factories for repeated method shapes | **ADOPT, gated** | `transforms.md` "Decorator Algebra": one factory, N behaviors via data — but skill bans god decorators and single-caller extraction. Apply only at >=3 repeated shapes whose variance is data. The four aspects ARE the factories. |

The line is sharp: a decorator is justified when it injects **behavior orthogonal to the operation's domain** (a span, a bind, a retry loop, a typecheck) that would otherwise be duplicated inline at every call. It is *not* justified when it merely *records a fact* (this handler serves `static build`; this row runs `ruff`). Facts are data; behavior is aspects.

---
## [2][REGISTRATION_IS_DATA]
>**Dictum:** *`TOOLS` and `REGISTRY` stay literal frozen tuples. `@tool`/`@rail`/`@parser` are rejected. This is definitive.*

A `@rail(claim, verb)` / `@tool(...)` / `@parser(kind)` decorator is **strictly inferior** to the explicit `REGISTRY: tuple[Bind, ...]` and `TOOLS: tuple[Tool, ...]`, on every axis that matters here:

| [AXIS] | [DATA TUPLE] | [REGISTRATION DECORATOR] |
| ------ | ------------ | ------------------------ |
| Discoverability | One readable table, top-to-bottom; `select`/`group-by` are pure filters (`catalog.md` §3, `registry.md` §1). | Rows scattered as import side effects across modules; the set is never visible in one place. |
| Import-order coupling | Single total definition; membership is deterministic and independent of what got imported (`catalog.md` sort key is authoring-order-independent). | Registry contents depend on which modules imported and when; forces eager imports to be complete. |
| Static analyzability | `ty`/`ruff`/`msgspec` see one typed value `tuple[Tool, ...]`. | A mutated module-global `list` accreted by effects is opaque to static reasoning. |
| msgspec data-first | `Tool` is a frozen `msgspec.Struct`; the catalog is itself encodable/snapshot-testable; `hypothesis`/`inline-snapshot` laws run over the projection (ARCH §7). | Registration-as-effect makes the registry a runtime artifact, not a serializable value — the data-first thesis collapses. |
| Skill alignment | "Programmatic logic: variable-driven dispatch, zero stringly-typed routing." | A `@parser(kind)` re-introduces a stringly keyed global lookup — the exact anti-pattern. |

**Verdict.** Registration stays pure data. `parser` is a direct value reference on the row (`parser=parse_findings`, or a Protocol impl), never a `@parser`-registered global. Decorators are reserved exclusively for the four aspects. `__main__` *folds* `REGISTRY` into the Cyclopts tree via `_adapt` — "registration is data, not decorators" (`main.md` §1). This is the load-bearing reconciliation with msgspec: a row is a value you can encode, sort, diff, and prove; a decorator is an effect you cannot.

---
## [3][CONFLICT_RECONCILIATION]
>**Dictum:** *One stack order, one Check shape, one pinned Detail/Mode surface. `aspect.md` is canonical; `engine.md` and `registry.md` must be corrected.*

### [3.i][STACK_ORDER] — DEFINITIVE: `checked ▷ logged ▷ traced ▷ retried ▷ operation` (checked OUTERMOST)

`aspect.md` is correct; `engine.md` and `registry.md` (both `traced ▷ logged ▷ retried ▷ checked`, checked innermost) are **wrong** and must be amended. The order is a *declared, monotonic, justified* Slot variant — `Slot(checked=0, logged=1, traced=2, retried=3)` validated by `compose` — exactly the mechanism the skill mandates (not the literal template string). Per-channel justification:

| [LAYER] | [CHANNEL] | [WHY THIS POSITION] |
| ------- | --------- | ------------------- |
| `checked` (outermost) | **bug/exception** — raises `BeartypeCallHintViolation`, deliberately distinct from domain `Fault`. | Validate the public args **once** and fail fast before any span/bind/retry cost; never re-validate per retry attempt (the engine draft's innermost placement re-ran beartype on every attempt). A contract breach is a programmer bug surfaced as exit-2 at `__main__`, not a quality signal worth a span. |
| `logged` | **Result** — pass-through; binds contextvars, projects `match res` to one event, returns the same `Result`. | Must be outer of `traced` so `bound_contextvars(rail, verb, run_id)` is live before the span opens; the span and every child span/log then inherit the keys, and `merge_contextvars` injects `trace_id`/`span_id` into each line. |
| `traced` | **Result** + exception *recording* (re-raises, never swallows). | Must enclose **all** retries: one span per `Check`/rail with `retry.attempts` as an attribute — `@trace @retry` spans the whole loop; the reverse emits a span per attempt. |
| `retried` (innermost) | **exception only** — `stamina` on the raising `Spawn`, below the single boundary adapter. | A domain `Error(Fault)` is a *value* and flows past `stamina` untouched; a non-zero tool exit is `Ok(Completed)` and is never retried; only a transient spawn/probe *exception* retries; `Cancelled` is `BaseException` and never reaches the `Exception` predicate. Retry re-executes only the spawn — never the typecheck, bind, or span setup. |

Divergence from the skill template (`trace` outermost) is sanctioned: the template puts `trace` first to observe authorize/validate *rejections* of untrusted input; assay's `checked` guards *internal contract breaches* (bugs), which should crash with a traceback, not produce a misleading span. The governing law — declared + monotonic + justified slots — is satisfied.

**Rail seam carries NO `retried`.** A rail is not a transient spawn (`aspect.md` SEAM_MAP). Rail runner = `compose(checked(), logged(event="rail"), traced(span=..., attrs=_rail_attrs))`; engine seam = `compose(checked(), traced(span=..., attrs=_check_attrs), retried())`. `registry.md` §2/§4 incorrectly include `retried` at the rail seam — strike it.

### [3.ii][CHECK_SHAPE] — CONFIRMED

`Check` **inlines** routed fields `{paths, owner, solution, glob}` (no separate `Routed` member) and takes `ArtifactScope` as a **run-time parameter** to `run_check(check, *, settings, scope)`, never as a stored field (`model.md` §3). This keeps `core/model.py` importing only `core/status.py` (stage 2): a stored `Routed`/`ArtifactScope` would force a `routing`/`settings` import and a cycle. `engine.md`'s `_argv(check)` reading `check.routed.*`/`check.scope` must be rewritten to read the inlined scalars plus the `scope` argument. The routing-internal `Routed` struct (`routing.md`: `files/projects/groups/full_triggers`, one per language `route()`) and the per-Tool `Check` inlined fields are **distinct concepts, not parallel models** — `Routed` is the routing product (1 per language), `Check` is the engine binding (N per `Routed`); no data-first violation.

### [3.iii][DETAIL_TAGS_AND_MODE] — PIN IN `core/model.py` BEFORE RAILS

- **Detail tags.** Pin **explicit short tags** `verify`/`package`/`api`/`test` on the four variants (`VerifySummary(Detail, tag="verify")`, `PackageRun(tag="package")`, `ApiSurface(tag="api")`, `TestRun(tag="test")`), overriding the `tag=str.lower` class-name default. Rails and wire consumers reference these short keys (`rails.md` §3); they are stable wire contract. All four live in `core/model.py` (stage 2) so every rail (stage 8-9) imports a fixed, closed `Report.detail` union with `forbid_unknown_fields` (INVARIANT 6).
- **Mode enum.** Pin one `Mode(StrEnum)` in `core/model.py` as the **operation-kind axis** (`CHECK`/`WRITE`/`RESTORE`/`BUILD`/`RUN`/`LIST`/`MUTATION`/`CLIENT`/`VERIFY`/`QUERY`/`STAGE`/`DEPLOY`/`PUBLISH`) carrying a `stream: bool` capture payload via `__new__`. This reconciles the conflict between `model.md` §1 (Mode = capture-only `CAPTURE`/`STREAM`) and `catalog.md`/`rails.md` (Mode = operation kind): the member *is* the operation kind; the engine reads `mode.stream` for the capture/stream dispatch. One enum, both concerns, pinned before any rail or catalog row references it.

---
## [4][COMPOSE_COMBINATOR]
>**Dictum:** *One slot-keyed, monotonic, idempotent fold; one application site per seam; transparent to `ty`.*

```python
type Hom[**P, T] = Callable[P, Result[T, Fault]]                # rail-facing shape
type Layer[**P, T] = tuple[Slot, Callable[[Hom[P, T]], Hom[P, T]]]

class Slot(IntEnum):
    checked = 0; logged = 1; traced = 2; retried = 3           # outer -> inner

def compose[**P, T](*layers: Layer[P, T]) -> Callable[[Hom[P, T]], Hom[P, T]]:
    ordered = sorted(layers, key=lambda layer: layer[0], reverse=True)   # highest slot applied first => innermost
    return lambda fn: reduce(lambda acc, layer: layer[1](acc), ordered, fn)
```

- **Slot order + monotonicity.** Sorting by `Slot` (reverse) makes `retried` innermost and `checked` outermost; a *regression* is structurally impossible because input is slot-keyed, not positional, and reused slots are permitted (two trace enrichers). This is the `assemble` law from `decorators.md`, specialized to assay's four slots.
- **Idempotency guard.** Each `dec` carries a `frozenset[int]` of `id(dec)` on its wrapper (structural identity, never call-counting); double-`compose` is a no-op, not a doubled span/bind. Survives reimport and concurrent decoration.
- **Type transparency.** Every layer is `Hom -> Hom` with PEP 695 `**P`, `Concatenate` where a layer injects a parameter, and `@wraps`, so `ty` sees `P` and `Result[T, Fault]` through all four. Codomain widening (`Fault | F`) is explicit; zero `Any`/`cast`/`Callable[..., Any]`. `retried` is the lone `Spawn -> Spawn` layer; the single boundary adapter inside `run_check` lifts the raising spawn to `Result`, keeping the stack above it uniformly `Hom`.
- **Single site per seam.** Exactly one `compose(...)` call in the rail runner and one in `run_check`; rails, handlers, the engine body, and `Tool` rows reference no aspect.

---
## [5][AOT_RULES_FOR_ASSAY]
>**Dictum:** *Six rules; the implementer follows them without re-deriving the doctrine.*

1. **Decorate aspects only.** `checked`/`logged`/`traced`/`retried` attach at exactly two seams (`run_check`, rail runner). Never inline a trace/log/retry/typecheck call in a rail, the engine body, or the catalog.
2. **Keep registration data.** `TOOLS` and `REGISTRY` are literal frozen tuples; growth is a row. No `@tool`/`@rail`/`@parser` decorator, no plugin auto-discovery, no metaclass struct auto-registration (`Base` uses msgspec config inheritance, not `StructMeta`).
3. **Factories only at >=3 shapes.** A decorator factory is justified only for >=3 repeated shapes whose variance is data; never a god `@aspect`/`with_concern` collapsing the four (their channels differ — Result vs exception — and the skill bans god decorators). Each factory states its `Slot` and channel in code.
4. **Never decorate to hide a constant** or a single-call helper. A fact (claim, verb, runner prefix, parser reference) is a field on a row, not a decorator argument.
5. **Compose once per seam.** The `Slot` IntEnum + monotonic `compose` is the only stacking mechanism; idempotency via `id(dec)` frozenset.
6. **Preserve the signature.** Every decorator is PEP 695 `**P`/`@wraps` with explicit codomain widening; `ty` sees `Result[T, Fault]` through the stack; zero `Any`.

---
## [6][REJECTIONS_AND_OPEN_DECISIONS]
>**Dictum:** *Name where AOT is refused to protect the data-first, anti-spam architecture, and what remains unsettled.*

**AOT REJECTED (to preserve data-first):**
- `@tool`/`@rail`/`@parser` registration decorators — §2 (hide data, import-order coupling, defeat msgspec/snapshot laws).
- Decorator/`@app.command`-per-verb CLI definition — `__main__` folds `REGISTRY` via `_adapt` (`main.md` §1).
- A god `@aspect`/`with_concern` match/case decorator collapsing the four aspects — channels differ; skill bans god decorators.
- Metaclass / `__init_subclass__` auto-registration of structs or detail variants — the `Report.detail` union is an explicit closed type pinned in `model.py`; `Base` inherits msgspec config only.
- Plugin import-scanning to populate the catalog — explicit tuple; deterministic membership.

**OPEN DECISIONS:**
1. **Mode payload shape.** Confirm `Mode` carries only `stream: bool`, or also a `mutates`-hint (currently `mutates` is a `Tool` field — keep it there; `Mode` stays capture-only payload). Pin before catalog rows author `mode=...`.
2. **Tracing contract breaches.** `checked` outermost means a `BeartypeCallHintViolation` is *not* traced (it crashes to exit-2). Accept the trade-off; revisit only if observability of internal contract breaches is required (would demand a thin outer `traced`, contradicting fail-fast — currently rejected).
3. **Per-`Check` `@logged`.** Optional/debug at the engine seam (span attributes already carry the receipt); default **off**.
4. **`retried` adapter site.** Confirm the single `Spawn -> Result` boundary adapter lives in `run_check` (one marked boundary exemption), so the compose stack stays uniform `Hom` above it.
5. **`compose` validation surface.** Decide whether `compose` returns `Result[..., Inversion]` (full `assemble` fold, decoration-time diagnostics) or trusts the sort (current draft); lean on the sort for four fixed slots, escalate to `assemble` only if a fifth concern with a contested slot is added.
