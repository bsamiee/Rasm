# Annotate-Wrap Admission Algebra — Deferred Evaluation And AOP Stack Semantics

# Critical Signals

- PEP 649 (Accepted) and PEP 749 (Final, Python 3.14) defer annotation evaluation through compiler-emitted `__annotate__` callables — eager `__annotations__` mutation at definition time is undefined under deferred semantics; `annotationlib.get_annotations` is the sole public read surface for decorator factories.
- PEP 749 revises `functools.update_wrapper` / `wraps` to copy only `__annotate__` from the wrapped callable; the wrapper's `__annotations__` descriptor delegates through the copied annotate function, not a frozen dict snapshot — intermediate wrappers that assign `__annotations__` without `__annotate__` delegation break outer policy layers and beartype 0.22+ decoration-time reads.
- beartype 0.22+ (repo pin `>=0.22.2`) implements PEP 649/749 and PEP 695 reads through `annotationlib`; pin upgrades are stack-evolution triggers even when source decorators are unchanged — decoration-time annotation object identity drift without source edits requires call-time regression samples.

# Deferred Annotation Read Contract

- PEP 649/749 make annotations lazy: the compiler emits `__annotate__(format: int) -> dict` on functions, classes, and modules; eager `__annotations__` assignment at definition time is undefined under deferred semantics.
- `annotationlib.get_annotations(obj, *, format=Format.VALUE, include_extras=True)` is the canonical decorator-factory read when `Annotated` metadata (`Field`, `BeforeValidator`, `msgspec.Meta`) is load-bearing.
- `Format.FORWARDREF` is mandatory during class-body and metaclass `__new__` when peer types are not yet bound; `Format.STRING` (PEP 749 rename from PEP 649 `SOURCE`) serves signature renderers and collection harnesses that must not import unresolved symbols.
- `get_annotations` unwraps `__wrapped__` chains automatically for callables; decorator factories that read annotations on wrappers must call through the outermost admitted object or `inspect.unwrap`, never assume the inner target is visible without unwrapping.
- `typing.evaluate_forward_ref` and `ForwardRef.evaluate(owner=...)` on deferred class annotations carry scope from the admitting owner; synthesis factories that materialize forward refs must pass the partial class or namespace owner, not an empty globals dict.
- PEP 749 adds `call_evaluate_function` on `TypeVar.evaluate_bound()`, `ParamSpec.evaluate_default()`, and `TypeAliasType.evaluate_value()` to resolve PEP 695/696 parameter evidence at factory time; erasing parameters to `Any` before evaluation is a decorator-boundary defect.
- Modules still carrying `from __future__ import annotations` store stringified bodies under PEP 749 transitional semantics — `__annotate__` called with `Format.VALUE` returns strings when the future import is active; factories must not mix `Format.VALUE` reads on those owners with deferred `Format.VALUE` reads on peers in one promotion unit.
- `annotationlib.call_annotate_function` is the supported helper for invoking `__annotate__` with format dispatch; public `get_annotations` consumers must never pass `Format.VALUE_WITH_FAKE_GLOBALS` — that format is an internal annotate contract for compiler-generated functions in fake-globals environments.

# Class-Body Annotate Hooks

- Synthesis decorators executing in class-body phase must read annotations from the namespace dict before the class object exists — `cls.__annotate__` is unavailable until `type.__new__` completes.
- `annotationlib.get_annotate_from_class_namespace(namespace)` retrieves the compiler-emitted annotate function; return `None` when the body declares no annotations.
- `annotationlib.call_annotate_function(annotate, format=Format.FORWARDREF, owner=typ)` evaluates class-body annotations without forcing full referent import; this is the metaclass-equivalent read path for custom `@record` factories.
- PEP 749 forbids reading raw namespace keys for `__annotate__`; internal storage names are implementation details that may change across micro releases — use `annotationlib` helpers exclusively.
- Metaclass admission that filters annotations — e.g., extracting `ClassVar` rows — wraps `__annotate__` on the finished class: inner call delegates to the compiler function, outer call projects the admitted field registry.
- Pydantic `BaseModel` subclasses must not compete with a parallel custom metaclass; domain synthesis inherits Pydantic's metaclass chain and reads `model_fields` after class-body phase, not mid-`__new__` namespace patches.

# PEP 749 Wrap Delegation Semantics

- PEP 749 revises `functools.update_wrapper` / `wraps`: the wrapper inherits `__annotate__` from the wrapped callable; the wrapper's `__annotations__` descriptor delegates through the copied annotate function, not a frozen dict snapshot.
- `classmethod` and `staticmethod` expose writable `__annotate__` and `__annotations__` that cache from the underlying callable on first read; shape decorators on classmethods must preserve this delegation — direct `__dict__` assignment on the descriptor breaks beartype and checker reads.
- Deleting `__annotations__` on a wrapper clears `__annotate__` to `None` per PEP 749; decorator factories must not "reset" annotations to repair admission defects — redecorate the inner target instead.
- Setting `__annotations__` on an object to a legal value automatically sets `__annotate__` to `None` — manual annotation dict assignment destroys deferred evaluation and is rejected in repo shape factories.
- `get_annotations(wrapper, format=Format.VALUE)` unwraps `__wrapped__` until a non-wrapped function is found, then evaluates in that object's `__globals__`; policy wrappers that close over foreign modules must keep `__wrapped__` chains intact for scope-correct reads.
- Partially executed modules return annotations executed so far without caching — factory reads during import must tolerate incremental annotation sets or defer until module execution completes.
- `annotationlib` executes arbitrary code when evaluating `Format.VALUE`; audit scripts and codegen harnesses prefer `Format.FORWARDREF` or `Format.STRING` unless load-bearing constraints require runtime objects.

# Wraps Propagation Completeness

- `functools.wraps` delegates to `update_wrapper` with `WRAPPER_ASSIGNMENTS` — in Python 3.14+ this includes `__annotate__` (PEP 749); copied attributes are `__module__`, `__name__`, `__qualname__`, `__doc__`, `__annotate__`, legacy `__annotations__` when present on pre-649 callables, `__type_params__` (PEP 695), and `__wrapped__` set last.
- `wraps` does not copy `__signature__`, `__defaults__`, `__kwdefaults__`, `register`, `dispatch`, `registry`, or custom admission markers — policy and catalog decorators must fold these via extended `assigned=` tuples on `update_wrapper` or explicit `setattr` after `wraps`.
- Under deferred evaluation, copying `__annotations__` without `__annotate__` delegation is insufficient; hostile intermediate wrappers that snapshot annotation dicts break lazy re-evaluation on outer policy layers and beartype 0.22+ compile paths.
- `Concatenate[Ctx, P]` admission that hides a leading parameter requires intentional `__signature__` surgery on the wrapper; the inner target retains full annotations for beartype and `validate_call`, the outer collected surface exposes only `P`.
- Signature surgery and `wraps` conflict when both mutate the same wrapper; pattern: build inner `wraps`-preserved target first, apply `Concatenate` projection on the outer shell, never delete parameter keys from the beartype-checked inner callable.
- Double-decoration guards run after `wraps`; fingerprint tuples (`id(dec)`, `frozenset` on wrapper) attach to the outermost admitted object, not the raw inner function.

# Descriptor Sandwich Order

- Builtin descriptors (`classmethod`, `staticmethod`, `property`) create method objects that beartype must re-wrap; order determines whether runtime checking sees bound or unbound signatures.
- Method admission order: declare body, apply shape decorator (`field_validator`, `computed_field`), apply `classmethod`/`staticmethod` when required, apply `@beartype` last on the callable surface, apply policy wrappers outside beartype on boundary handlers only.
- `@computed_field` stacks above `@property`; Ruff `property-decorators` must list `computed_field` so static analysis treats the stack as a property admission, not a bare method.
- Class-mode `@beartype` on the owning class should be listed first (topmost) so it captures methods after sibling decorators monkey-patch; callable-mode `@beartype` on a single function should be listed last (bottommost) before invocation.
- `inspect.unwrap` on bound methods may stop at `__wrapped__` depth that still carries descriptor prototypes; law-matrix collection compares signatures on the pytest-presented outer callable after unwrap exhaustion, not on arbitrary intermediate descriptors.

# Beartype Admission Placement

- beartype 0.22+ implements PEP 649/749 and PEP 695 annotation reads through `annotationlib`; pin upgrades are stack-evolution triggers even when source decorators are unchanged.
- beartype callable mode preserves `__wrapped__` and integrates with well-behaved decorators; decorator-hostile wrappers that erase annotations or omit `__wrapped__` require beartype outside the hostile layer or redesign of the hostile decorator.
- beartype 0.22 classifies decorator-hostile decorator objects — factories returning non-function wrappers without `__wrapped__` must not sit between beartype and the law subject without explicit QA rows.
- beartype reads annotations as real objects at decoration time via `annotationlib.get_annotations(..., format=Format.VALUE)` on the wrapped target; deferred `__annotate__` must resolve through that path when decoration runs, not through stale `__annotations__` snapshots copied by an intermediate wrapper.
- `BeartypeConf` is composed once in module `[CONSTANTS]`; per-call-site `BeartypeConf(...)` inside factories is rejected — configuration is an immutable admission anchor, not closure state.
- Class-mode beartype iterates methods after class creation; `@override` methods are independent admission sites — each override carries complete annotations, beartype does not inherit from the base wrapper.
- `@beartype` outside `@validate_call` when both guard the same boundary handler; inner `validate_call` compiles pydantic-core schema, outer beartype enforces Python annotation objects — duplicate failure surfaces with incompatible exception types are merge blockers.
- `BeartypeCallHintViolation` must propagate through policy stacks; catching and rethrowing as `ValidationError` or bare `TypeError` erases call-time enforcement attribution.

# AOP Stack As Typed Morphism

- AOP-first modeling treats cross-cutting shape and policy behavior as decorator-composed morphisms on one callable or class owner — domain method bodies own transforms, not authorization, tracing, caching, or ingress validation branches.
- AOP-first modeling treats decorator stacks as a composed morphism chain `trace ∘ authorize ∘ validate ∘ cache ∘ govern ∘ retry ∘ operation` on one `Hom[P, T, E]` surface — not as method-local `if` ladders.
- PEP 695 `**P` types the stack as `Callable[[Callable[P, R]], Callable[P, R | E]]` or `Callable[[Callable[Concatenate[Ctx, P], R]], Callable[P, R | E]]` when context is structurally prepended and discharged through `ContextVar` with `Token.reset()`.
- Stack order is observability-load-bearing: `@trace @retry` spans all attempts; `@retry @trace` traces each attempt independently — identical static types, different failure archaeology; reordering is a stack version event requiring adapter proof replay.
- `assemble`-style monotonic `Slot` ordering (`trace > authorize > validate > cache > govern > retry > operation`) rejects inversions at factory composition time; domain modules import pre-assembled stacks from composition roots, not ad hoc per-handler reorder.
- Codomain widening to `Result[T, E]` preserves exception transparency on the `Ok` path only; policy wrappers consult rail owners for retryable `E`, not pydantic `ValueError` absorption inside validators.
- Registry and catalog decorators append rows at import phase and return the same class or callable object; AOP wrappers must not mutate catalogs at call time.

# Annotate Wrapper Pattern For Synthesis

- Custom synthesis factories that add synthetic fields route through library specifiers (`Field`, `dataclass.field`, `msgspec.field`), not `namespace['__annotations__']` assignment — checker alignment and `get_annotations` parity depend on specifier vocabulary.
- When a factory must hide implementation fields from the public annotate surface, wrap `cls.__annotate__` post-creation: filter keys against an internal `ClassVar` or `PrivateAttr` registry before returning the dict to callers.
- `include_extras=True` on every factory read that compiles validators; stripping `Annotated` metadata collapses admitted shapes to bare `T` and silently drops `BeforeValidator` phases.
- Generic class factories key compiled caches on `(cls, cls.__type_params__, get_args(cls))`; `model_rebuild()` and first-touch `TypeAdapter` compile are part of the public admission contract for parametrized owners.
- Field-registry walks after synthesis read `dataclasses.fields`, `model_fields`, or `msgspec.inspect.type_info` — never the pre-synthesis class dict.

# Validate-Call And Annotate Compile Paths

- `@validate_call` compiles pydantic-core schemas from `get_annotations(fn, format=Format.VALUE, include_extras=True)` at decoration time; incomplete annotations on the inner `__wrapped__` target produce compile defects at decoration, not at first consumer call.
- `validate_call` preserves `.raw_function` on the wrapper; `.raw_function` inherits the inner annotate chain — trust gates call `.raw_function`, default ingress calls the validated wrapper.
- Decoration order on boundary handlers: operational body, `@validate_call`, policy wrappers outermost; `validate_call` sits inside `trace`/`authorize` so `ValidationError.loc` surfaces before span completion when possible.
- `TypeAdapter(T)` at module constants compiles from the admitted model class, not from a decorated handler; adapter and `validate_call` are alternate boundary owners — do not stack both on the same callable without documented admission split.
- `Unpack[Payload]` on keyword-callable hooks preserves payload keys through `wraps` chains only when outer shells do not perform `Concatenate` surgery on `**kwargs` parameters.

# ContextVar Discharge In Wrap Chains

- `Concatenate[Ctx, P]` plus `ContextVar` discharge is the approved context-injection pattern; `ctx.set(old)` without `Token.reset()` under concurrency is rejected — admission stacks use `(token := ctx.set(c), fn(...), token.reset())[1]` or equivalent `Token` scope.
- Policy wrappers that absorb context must not widen inner `**P` to `**kwargs: Any`; context flows through `Concatenate` or explicit closed payload types, never through erased kwargs bags.
- `contextvars` propagate through `wraps`-preserved async callables; policy stacks on async generators preserve context across `yield` boundaries when inner targets use the same `ContextVar` anchors declared in `[CONSTANTS]`.

# Singledispatch And Catalog Wrap Folds

- `functools.singledispatch` and `singledispatchmethod` expose `register`, `dispatch`, and `registry` on the base function; `@wraps` alone omits these — policy wrappers fold dispatch attributes via `block.fold(setattr, wrapper, dispatch_attrs)` after `update_wrapper`.
- `inspect.unwrap(fn, stop=lambda f: hasattr(f, 'registry'))` halts at the dispatch base regardless of intermediate policy layers; catalog rows key off the dispatch base `qualname`, not the outermost trace wrapper.
- Handlers registered through `register` must already carry complete PEP 695 signatures before registry insertion; registration does not repair erased domain types post hoc.
- Test `@spec` decorators on dispatched handlers verify the collected item signature against the law subject on the dispatch base after unwrap-to-registry, not against the outer policy shell.

# Test Harness Wrap Algebra

- Law-matrix decorators apply hypothesis marks in fixed inner order (`@given` before `@settings` before pytest marks); re-applying `wraps` on hypothesis inner closures resurrects strategy parameters on the collected item — rejected.
- `@beartype` on extracted operational helpers under test is permitted; `@beartype` on the collected test function is discouraged when hypothesis rewrites calling conventions — the law subject remains the inner production stack import.
- `pytest.mark.parametrize` outermost above hypothesis when both apply; parametrize indices align with law records, not decorator closure string ids.
- Production stack fingerprint proofs run on adapter handlers; test harness fingerprints run on collected items — both must agree on `__type_params__` and unwrap depth after promotion.

# Annotate Security And Factory Discipline

- `annotationlib.get_annotations(..., format=Format.VALUE)` may execute arbitrary expressions from annotation scopes; codegen and audit harnesses default to `FORWARDREF` unless compiling validators requires runtime `Field` objects.
- Decorator factories are not boundary exemption sites for `eval()` on string annotations; `eval_str=True` is rejected in repo shape factories — use `Format.VALUE` on deferred owners or migrate modules off `from __future__ import annotations`.
- Factory closures must not cache annotation dicts across dissimilar owners; cache keys include `(id(owner), format, include_extras, get_args(owner))` for generic classes.

# Admission Stack Fingerprints

- Callable fingerprints: `inspect.unwrap` depth, outermost `__type_params__`, `inspect.signature` parity with PEP 695 contract, and `get_annotations(outer, include_extras=True)` keyed set versus inner target.
- Class fingerprints: `model_fields` keys versus `get_annotations(cls, include_extras=True)`, `__pydantic_core_schema__` tag set after first-touch, or `msgspec.inspect.type_info` field order for struct owners.
- Wrap-chain fingerprints: presence of `__wrapped__`, propagated `register`/`dispatch` on dispatch owners, and `id(dec)` tuples on double-decoration guards.
- beartype fingerprints: decoration-time annotation object identity on inner target; pin upgrades that change identity without source edits trigger call-time regression samples.
- Fingerprint drift between production adapter and law-matrix collected item blocks merge — pytest must collect the same outer callable the boundary presents to foreign ingress.
- Phase-attributed failure injection walks unwrap chains to name the outermost enforcing decorator: import-time duplicate registration, class-body annotate-hook failure, first-touch stale generic schema, call-time beartype violation — conflated attribution fails the harness module.
