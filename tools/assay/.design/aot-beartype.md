# [H1][AOT_BEARTYPE_DESIGN]
>**Dictum:** *One memoized `BeartypeConf` flows through a `@checked` Layer at the two seams; the claw hook is a test-gated amplifier, never the production posture.*

Pins beartype as the runtime boundary type-checker for `assay`, expressed AOT/decorator-style. Verified against `beartype>=0.22.2` (docs at 0.23.0): `beartype(conf=...)`; `BeartypeConf(*, is_color=None, is_debug=False, is_pep484_tower=False, strategy=BeartypeStrategy.O1, violation_type=..., warning_cls_on_decorator_exception=...)` — an **immutable, self-caching (memoized-on-params)** dataclass; `BeartypeStrategy.{O0,O1,On,Ologn}` where **only `O0` (identity noop) and `O1` (constant-time, samples one item per container nesting level) are implemented** — `On`/`Ologn` are roadmapped, not yet selectable (corrects `aspect.md` open-decision 4's `Ologn` aside); `beartype.claw.{beartype_this_package,beartype_package,beartype_packages,beartype_all}(conf=...)`; `beartype.door.{is_bearable,die_if_unbearable}(obj, hint, *, conf=...)`; violations are `beartype.roar.BeartypeViolation` subclasses (`BeartypeCallHintParamViolation`, `BeartypeCallHintReturnViolation`). beartype natively understands PEP 695 generics and `Annotated`, and treats `msgspec.Meta` metadata as opaque (it is not a `beartype.vale` validator), so it checks the base type and never re-runs msgspec constraints.

---
## [1][CLAW_HOOK_VS_DECORATOR]
>**Dictum:** *`assay` is a first-party `python -m` app — claw's ideal target — yet the two-seam architecture forbids claw as the standing posture.*

`beartype_this_package()` placed once in `tools/assay/__init__.py` is true AOT: zero per-function decorators, whole-package coverage, plus PEP 526 annotated-assignment checks the bare `@beartype` decorator cannot do. But it is **global**, and the architecture (ARCHITECTURE.md §9, IMPLEMENTATION.md core-need 5) mandates cross-cutting libraries attach at **exactly two seams** — `run_check` and the rail runner — leaving private closures (`_argv`, `_place`, `_splice`, `_overlay`, `_guarded`, `_stream`) bare for `ty` to prove statically.

| [DIMENSION] | [`beartype_this_package()` claw] | [explicit `@checked` at two seams] |
| ----------- | -------------------------------- | ---------------------------------- |
| Coverage | All callables/classes + PEP 526 assignments, every submodule. | The two public Engine entrypoints + the rail handler only. |
| Startup cost | AST-rewrites + decorates every submodule at import — paid on **every** short-lived CLI invocation. | Decorates two functions at import; negligible. |
| Hot-path safety | Over-checks the inner spawn/stream closures the design wants bare. | Inner loop pays nothing; seam-scoped by construction. |
| `ty` interplay | Duplicates at runtime what `ty` already proves on the interior. | Complementary: `ty` owns the interior, beartype the dynamic boundary. |
| Third-party noise | Self-package only ⇒ msgspec/pydantic/expression unhooked (safe); `beartype_all()` would be reckless. | None; scoped to assay's own seams. |
| Seam discipline | Breaks invariant — global, not a `Layer`. | Upholds invariant — one `Layer`, slot-ordered. |

**Recommendation:** explicit `@checked` is the production posture; claw is retained only as a **test/CI amplifier** (§6).

---
## [2][CHECKED_FACTORY]
>**Dictum:** *Declare the conf once as a memoized singleton; `checked()` emits the outermost `Layer`.*

```python
_CONF: Final = BeartypeConf(is_pep484_tower=True, strategy=BeartypeStrategy.O1)  # memoized ⇒ identity-stable

def checked[**P, T](*, conf: BeartypeConf = _CONF) -> Layer[P, T]:
    return (Slot.checked, lambda fn: beartype(conf=conf)(fn))   # signature-transparent; reads, never rewrites Ok/Error
```

`is_pep484_tower=True` expands every `float` hint to `float | int`, killing false positives where call sites pass `int` literals to `timeout`/`duration_ms` (`float`) params. The conf is a module-level `Final` singleton (already `extend-immutable-calls` in `pyproject.toml`); beartype memoizes confs on their parameter set, so even repeated `BeartypeConf(...)` calls collapse to one cached instance — no per-decoration allocation. `checked()` returns `(Slot.checked, dec)`; `compose` (aspect.md §2) sorts it **outermost** (`Slot.checked = 0`), so beartype validates the public arguments once and fails fast *before* any span, bind, or retry cost, and validates the **final** `Result` the whole stack returns. It composes ahead of `logged ▷ traced ▷ retried ▷ operation` at both seams.

---
## [3][OWNERSHIP_BOUNDARIES]
>**Dictum:** *Three checkers, three disjoint jurisdictions; zero redundant validation.*

| [CHECKER] | [WHEN] | [BOUNDARY OWNED] | [VALIDATES] |
| --------- | ------ | ---------------- | ----------- |
| `ty` | Static (edit/CI) | The whole module graph, incl. bare interior closures. | Everything statically decidable; the interior beartype never sees. |
| `msgspec` | Decode-time | The **wire** boundary (JSON in/out, bridge/C# payloads). | `Meta(ge=,gt=,le=)` constraints, `forbid_unknown_fields`, `tag`/`kind` dispatch — value contents. |
| `beartype` | Call-time | The two **function** seams. | That the runtime object *is* a `Check`/`AssaySettings` and the return *is* `Result[Completed, Fault]` — object shape. |

No overlap: beartype checks the **base type** of `Annotated[float, msgspec.Meta(gt=0)]` and ignores the `Meta` (msgspec's job at decode); msgspec never inspects function call shapes; `ty` never runs. beartype earns its keep precisely where static proof ends — data born at runtime from `msgspec.json.decode`, catalog rows built as data, and `msgspec.defstruct` detail types (model.md §5) that have no static identity. Do **not** `@beartype` the closures that only consume already-decoded structs, and do **not** enable claw globally; both reintroduce the redundancy this table forbids.

---
## [4][RESULT_CHANNEL_SAFETY]
>**Dictum:** *beartype reads the return value; it cannot flip `Error` to `Ok`, and it never touches the exception channel.*

beartype validates the declared codomain `Result[Completed, Fault]` and returns the value **unmodified** — a domain `Error(Fault(...))` is a fully valid inhabitant of that type and passes through untouched; there is no code path from `Result` back to bare `T` or to a coerced `Ok`. Because `@checked` is a pure success/return-channel reader (no `try`/`except`), the raising `@retried` spawn channel below it (aspect.md §1) is invisible to beartype.

```python
@compose(checked(), traced(span="assay.check.x", attrs=_check_attrs), retried())
def run_check(check: Check, *, settings: AssaySettings) -> Result[Completed, Fault]: ...

run_check(busy_check, settings=s)   # -> Error(Fault(status=BUSY, returncode=5)) ; beartype passes it through verbatim
```

The lone failure beartype can raise here is a genuine contract breach (the value inside `Ok` is not a `Completed`) — a `BeartypeCallHintReturnViolation`, i.e. a programmer bug, deliberately surfaced, not channel interference.

---
## [5][PERFORMANCE]
>**Dictum:** *`O1` at two seams is free; the expensive lever is claw, kept out of production.*

`O1` samples one item per container nesting level per call, so a `tuple[Tool, ...]` catalog arg or `tuple[str, ...]` argv costs O(1), not O(n). `@checked` fires once per `Check` and once per rail invocation — **never** inside the stream reader, the bounded tail deque, or per retry attempt (it is outermost, so retries re-run only the spawn). For a short-lived CLI running N checks, total beartype overhead is microseconds·N — keep it **enabled in both production and CI**. The disable lever is a single conf swap, selected once via settings, not a code edit:

```python
_STRATEGY = {"off": BeartypeStrategy.O0}.get(settings.beartype, BeartypeStrategy.O1)  # O0 == identity noop
_CONF = BeartypeConf(is_pep484_tower=True, strategy=_STRATEGY)
```

The genuinely costly path — whole-package AST rewrite — is the claw hook; that is the thing gated OFF by default and ON only for the test gate.

---
## [6][VERDICT_AND_OPEN_DECISIONS]
>**Dictum:** *Adopt the hybrid: `@checked` decorator in production, claw as a CI-only amplifier.*

**ADOPT (hybrid).** (a) Production: the `@checked(conf=_CONF)` `Layer` at the two seams — one memoized conf, `O1`, tower on, outermost slot, interior left to `ty`. (b) Test/CI only: an opt-in `beartype_this_package()` to amplify coverage to every submodule + PEP 526 assignments during the gate, table-dispatched at the composition root so no `if` enters domain code:

```python
# tools/assay/__init__.py — BOUNDARY (import-time enablement; not domain logic)
{"1": lambda: beartype_this_package(conf=BeartypeConf(is_pep484_tower=True, warning_cls_on_decorator_exception=None))} \
    .get(os.environ.get("ASSAY_CLAW", ""), lambda: None)()
```

`warning_cls_on_decorator_exception=None` makes claw raise (not warn) on decoration edge cases so the gate fails loud. **AVOID** `beartype_all()` (assimilates third-party + stdlib) and standing-production claw (breaks seam discipline, taxes every invocation).

**Open decisions.** (1) **Envelope invariant vs raised violation** — invariant 1 demands exactly one Envelope; a `BeartypeViolation` escaping the stack would crash with none. Resolve with one `__main__` boundary catch rendering `beartype.roar.BeartypeViolation` as an exit-2 `Envelope(error=Fault.fail(...))`, keeping the bug channel distinct from the domain `Fault` rail. Equivalent alternative: set `conf.violation_type` to a typed bug exception caught at the same seam. (2) **`door` for `defstruct` details** — whether the owning rail should `die_if_unbearable(instance, variant)` after `msgspec.convert` for runtime-generated detail types `@checked` cannot see statically. (3) **Claw seam** — enable via `ASSAY_CLAW` env (CI matrix) or via `conftest.py` fixture so unit runs always amplify; the latter guarantees the test gate never forgets it.

---
## [FURTHER_CONSIDERATION]

- **`is_debug=True` surfaces the generated wrapper.** Setting `BeartypeConf(is_debug=True)` prints the synthesized type-checking wrapper source; invaluable once to confirm beartype actually deep-checks `expression.Result[Completed, Fault]` rather than collapsing it to the `Result` origin, since expression's runtime subscription depth governs whether `Ok`'s payload is sampled at all.
- **claw + `msgspec` AST timing.** Because claw rewrites modules at import and msgspec resolves field annotations lazily at first `Encoder`/`Decoder` build (model.md §4, PEP 649/749), the claw hook must precede the first `import tools.assay.core.model`; placing it as the first statement of `__init__.py` is load-bearing, not cosmetic.
- **`beartype.vale.Is` as a constraint bridge.** Where a runtime invariant outranks a wire constraint (e.g. an argv that must be non-empty before spawn), `Annotated[tuple[str, ...], Is[lambda a: len(a) > 0]]` lets beartype enforce it at the seam — a niche complement to `msgspec.Meta`, which only fires at decode, not at internal call boundaries.
