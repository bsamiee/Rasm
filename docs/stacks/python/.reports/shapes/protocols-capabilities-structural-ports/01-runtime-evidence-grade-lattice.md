# Runtime Evidence Grade Lattice

# Critical Signals

- Static structural proof (PEP 544 + PEP 695/696), runtime presence (`@runtime_checkable`), semantic narrowing (PEP 742 `TypeIs`), and introspection (`is_protocol`, `get_protocol_members`) are four non-interchangeable evidence grades — substituting one for another at a seam is a lattice defect regardless of checker green.
- GH-132604 (merged for Python 3.15, PR 143806) deprecates inherited `@runtime_checkable` on undecorated protocol subclasses — every refinement and intersection join link used in `isinstance` or `issubclass` requires independent re-decoration before 3.20 hard `TypeError`.
- PEP 800 forbids `@disjoint_base` on `Protocol` definitions — closed variant modality belongs on nominal sealed families with `TypeIs` and exhaustive `match`, not `@runtime_checkable` protocol unions simulating arm sets.

# Evidence Grade Taxonomy

- **Static grade** — PEP 544 structural subtyping enforced by `ty`, strict `mypy`, and pyright; CPython validates no signatures at import; assignability accepts any object whose members match without inheritance.
- **Presence grade** — opt-in `@runtime_checkable` on the declaring class body sets `_is_runtime_protocol`; `isinstance` and `issubclass` loop `__protocol_attrs__` via `inspect.getattr_static()` — name presence only, never signature, return type, async/sync scheduling, or generic instantiation correctness.
- **Semantic grade** — PEP 742 `TypeIs[Port]` named predicates prove attestation, tier, version band, non-empty state, or scheduling facts presence grade cannot see; complement narrows the negative branch unlike `TypeGuard`.
- **Introspection grade** — `typing.is_protocol(tp)` and `get_protocol_members(tp)` yield `frozenset[str]` member sets at registration roots; proves declared surface, not behavior, scheduling, or authorization.
- Grade selection is seam-local: static ports at call sites; presence or semantic gates at adapter ingress; introspection at catalog registration and CI frozenset snapshots — never mid-pipeline repair on already-narrowed interior parameters.

# PEP 544 Static Structural Law

- PEP 544 protocols are static contract surfaces — "there is no intent to make protocols non-optional in the future" and "no runtime semantics will be imposed for variables or parameters annotated with a protocol class" beyond opt-in `@runtime_checkable`.
- Declare `class Port(Protocol):` or `class Port[T](Protocol):`; implementers need not inherit — matching members suffice; built-in generics inside protocol members follow PEP 695/696 variance inference with `@override` on refinements.
- **Data versus non-data protocols** — protocols with any non-method member (`x: int`) are data protocols; `isinstance` accepts both data and non-data protocols; `issubclass` accepts only non-data protocols and raises `TypeError` listing blocking attribute names when data members block subclass checks.
- **Unsafe overlap** — checkers reject `isinstance(x, P)` when `type(x)` unsafely overlaps `P` (subtype of type-erased `P` with all members `Any` but not a subtype of `P`); safe overlap enables union narrowing after `isinstance` similar to nominal classes.
- **Generic alias blindness** — `@runtime_checkable` applies only to non-generic and unsubscripted generic protocols (`Iterable` ≡ `Iterable[Any]`); subscripted aliases (`Decode[int]`) raise `TypeError` at runtime check sites — parameterized ports key maps on unbound `type[Port]` and bind parameters only at call sites.
- Static assignability accepts third-party classes, stdlib ABCs, and structural test doubles when names and signatures align — capability ports declare replaceable operations; nominal owners declare identity, variant seals, and wire layout; conflating the two at a seam is a grade defect.

# GH-132604 Inherited Runtime Checkability Deprecation

- GH-132604 documents that undecorated protocol subclasses of `@runtime_checkable` parents silently passed `isinstance` and `issubclass` since Python 3.8 — behavior treated as bug; PR 143806 merged February 2026 for 3.15.
- Deprecation emits `DeprecationWarning` at the check site under 3.15; Python 3.20 raises `TypeError` — five feature-release promotion window, not comment-only migration; eternal warning remains an escape hatch per upstream discussion.
- `_is_runtime_protocol` is set only by `@runtime_checkable` on the declaring class body — child protocols in refinement chains (`Narrow(Wide, Protocol)`) do not inherit the flag even when `Wide` is decorated; MRO proximity and static assignability do not propagate runtime checkability.
- Undecorated subclasses set `__typing_is_deprecated_inherited_runtime_protocol__` and trigger deprecation through `_is_deprecated_inherited_runtime_protocol` — CI runs `-W error::DeprecationWarning` on port matrix modules exercising refinement chains.
- Intersection joins (`Dual[T](Readable[T], Writable[T], Protocol)`) used in `isinstance` gates need decoration on the join type when the gate names the join, not merely on slice parents — undecorated joins enter the deprecation path when any ancestor was runtime-checkable.
- Every link in a refinement chain, join type, and mirror port used with runtime checks requires independent `@runtime_checkable` — parent decoration, static narrowing, and `get_protocol_members` superset proof do not substitute.

# @runtime_checkable Presence Mechanics

- `@runtime_checkable` installs `_abc_subclasscheck` for `issubclass` plus an instance loop over `__protocol_attrs__` using `inspect.getattr_static()` — never evaluates annotations, never verifies callable signatures, never distinguishes sync from async scheduling.
- Python 3.12+ instance checks tolerate attributes assigned in `__init__` when static resolution finds the name on the instance — class-body-only declarations still fail for dynamically attached members unless the name appears in `__protocol_attrs__` via protocol-body annotation.
- Protocols whose members are entirely non-callable populate `__non_callable_proto_members__` — `@runtime_checkable` permits `isinstance` but `issubclass` raises `TypeError` listing blocking names; data-protocol subclass gates are presence-grade only on instances.
- Setting `__iter__ = None` on a protocol mixin (stdlib pattern) prevents accidental `collections.abc.Iterable` duck compatibility — structural ports that must not iterate should copy this guard rather than rely on absent `__iter__` alone.
- Hot-path adapter selection prefers single-attribute `hasattr` or semantic `TypeIs` over full protocol `isinstance` — the member loop scales with port surface area and defeats boundary-local coarse-gate intent when mirrors grow fat.
- Subscripted generic protocols, parameterized aliases, and erased `object` carriers always fail or mislead at runtime — presence grade is invalid evidence when the gate subject is not an unbound protocol class.

# PEP 742 TypeIs Semantic Grade

- PEP 742 (Final, Python 3.13+ in `typing`; `typing_extensions` 4.10.0+) adds `TypeIs[R]` — positive branch narrows to intersection of prior type and `R`; negative branch narrows to complement `A ∧ ¬R`; behavior aligns with `isinstance` narrowing, not `TypeGuard` positive-only widening.
- Named functions with return annotation `TypeIs[C]` are mandatory — lambdas cannot carry PEP 742 narrowing; gate decorators wrap `filter_with` on `Ok(obj)` with the predicate, never bare `isinstance` followed by `cast`.
- `TypeIs` requires narrowed type consistent with input type — `TypeIs[B]` is not a subtype of `TypeIs[A]` when `B` is a subtype of `A`; predicates must return `True` iff argument is compatible with `R` and `False` otherwise — unsound predicates produce silent checker lies.
- Semantic predicates prove attestation, non-empty state, version band, tier membership, or scheduling model that `@runtime_checkable` cannot see — stack outer presence (`@runtime_checkable` or coarse `hasattr`), inner semantic sufficiency (`TypeIs`) when load-bearing.
- Tier traversal orders `Block[tuple[Tier, type[Port]]]` descending strength — first successful `TypeIs` short-circuits; weaker candidates must not run after acceptance; rejection evidence carries `(expected_cap, actual_type, tier)` as structured tuples, not parallel error dataclass proliferation.
- Reserve `TypeGuard` only when narrowing to incompatible input types or positive-only guards are intentionally load-bearing — documentation and new gates default to `TypeIs`.

# PEP 800 Variant Modality Exclusion

- PEP 800 (Accepted, Python 3.15; `typing_extensions.disjoint_base` ≥4.15.0) adds `@disjoint_base` for nominal classes — explicitly disallowed on `Protocol` definitions; disjoint-base reachability applies to nominal seals, not structural capability lattices.
- Closed variant identity stays on `@disjoint_base` or `@tagged_union` families with exhaustive `match` and arm `TypeIs` predicates — ports implement operations over `Member` parameters, not one protocol per arm.
- Reject `@runtime_checkable` protocol unions simulating closed variant modality — runtime member presence cannot discharge exhaustive `match`; PEP 800 unreachable-arm analysis and `assert_never` own arm proof between unrelated nominal seals.
- At most one port parameterized on a family alias — `Processor[Member]` with interior exhaustive `match` — when algorithms are parametric across arms; per-arm protocol siblings inside one bounded context are grade defects.
- `isinstance(x, Union[PortA, PortB])` over variant-shaped unions provides presence without exhaustiveness — static checker union narrowing and runtime presence diverge; disjoint nominal families plus `TypeIs` per arm are the lawful modality proof.

# Introspection Surfaces And Annotationlib

- `typing.is_protocol(tp)` returns `True` only for protocol classes — generic aliases, nominal classes, stdlib `io.Reader`, and `collections.abc` ABCs return `False` even when runtime-structural.
- `get_protocol_members(tp)` returns `frozenset[str]` from `__protocol_attrs__` — method names, properties, classmethods, and annotated non-method members; raises `TypeError` when `tp` is not a protocol class.
- Scope maps, adapter tables, and law registration key on `type[Port]` — never on parameterized aliases; `get_protocol_members` is not a security or authorization rail — frozenset membership proves name declaration, not correct behavior.
- PEP 649/749 defer annotations through `__annotate__`; protocol `_proto_hook` conformance and registration roots read `annotationlib.get_annotations(base, format=annotationlib.Format.VALUE)` on protocol bases when MRO `__dict__` misses deferred members — prefer `get_protocol_members` at registration roots over ad hoc `__annotations__` reads that ignore deferral.
- Golden `frozenset(get_protocol_members(Port))` snapshots in CI — unexpected diffs force simultaneous mirror extraction, bind row, and law witness updates in one promotion unit; introspection grade blocks generative backend runs when static grade already fails.

# Capability Port Versus Owner Triage

- Admit `Protocol` when multiple independent implementers satisfy the same operation family without inheritance — stdlib types, vendor SDK classes, and structural test doubles must assign without local subclassing.
- Reject protocol shells for single-callback ingress — `Callable` with preserved `ParamSpec` or `Callable[[Unpack[TypedDict]], R]` owns keyword payload shapes; one-method protocol mirrors duplicate evidence.
- Reject protocol per variant arm — closed identity belongs on `@disjoint_base` or `@tagged_union` families with interior exhaustive `match`; at most one port parameterized on `Member` when algorithms are parametric across arms.
- Reject model bases mixed into protocol MRO — nominal shape authorities implement port methods for tests as doubles, not as proof the family should have been structural.
- Collapse single-implementer ports with no planned substitution to concrete boundary parameter types unless third-party replacement ships within one release cycle — orphan protocol ceremony without substitution load.
- Collapse runtime-only ports existing solely for `isinstance` without static call sites to `TypeIs` over a nominal owner or delete the gate — evidence grade must match actual call-site need.
- Ports own replaceable side effects invoked after canonical materialization exit — never port-typed fields on ingress, wire, or settings projections; ingress port fields fail seam altitude regardless of runtime smoke green.

# Intersection Join And Refinement Runtime Policy

- Intersection joins compose capability slices statically — `Dual[T](Readable[T], Writable[T], Protocol)` widens the member set to the union of parent `get_protocol_members` names; runtime gates naming `Dual[T]` need independent `@runtime_checkable` when `isinstance` is load-bearing.
- Undecorated joins inheriting from decorated parents enter the GH-132604 deprecation path — treat join decoration as part of the same promotion unit as parent slice edits.
- Refinement chains tighten return types or narrow parameters with `@override` on every redeclared member — child frozensets must superset parent required names; incompatible signature refinements belong in separate protocol branches, not one join type.
- Static refinement is a checker artifact — runtime MRO does not propagate checkability; each `@runtime_checkable` link is an explicit opt-in at the evidence grade the gate declares.

# Protocol Default Members At Runtime

- Optional capability — transactional batching, no-op hooks, identity scopes — belongs as protocol default members on the `Protocol` body, not parallel nominal mixin ABCs that break third-party structural assignability.
- Default descriptors on the protocol class supply fallback behavior when implementers omit overrides — `@property def transaction(self) -> AbstractContextManager: return nullcontext()` keeps non-capable backends usable without override ceremony.
- Static checkers treat default bodies as part of the declared contract; runtime `@runtime_checkable` checks still require the member name to resolve — defaults do not satisfy missing names on partial implementers.
- Reject default members encoding domain policy — authorization tiers, version bands, and gate vocabulary are decoration-time frozenset algebra, not absent-capability placeholders.

# Stdlib ABC Versus Custom Mirror Evidence

- `collections.abc.Buffer`, `__buffer__`, `io.Reader`, and `io.Writer` (3.14+) already own byte and stream capability — custom ports that rename those surfaces duplicate proof without load-bearing domain extension.
- `typing.is_protocol` returns `False` for stdlib ABCs — compatibility matrices must not treat ABC membership as protocol introspection; adapter rows key on admitted custom ports or stdlib ABC types explicitly, never on string registry aliases.
- Admit custom mirror ports only when invoked methods genuinely extend the stdlib surface — boundary modules extract minimal call-site surfaces; fat mirrors widen `get_protocol_members` obligation without widening domain capability.
- Prefer `Callable` with preserved `ParamSpec` over one-method protocol shells when the capability is a single callable entry with keyword payload — PEP 692 `Unpack[TypedDict]` owns keyword ingress without nominal callback protocols.

# Free-Threaded Scope And Presence Under Concurrency

- PEP 779 treats free-threaded CPython as supported — scope maps keyed by `type[Port]` remain `Map[type, object]` threaded through explicit parameters, not module-level mutable registries visible across threads without synchronization.
- `ContextVar` holds scope slices at composition-stack edges only — interior domain folds treat scope as read-only; transient capability mutation forks via `copy_context().run` through `scoped(caps, thunk)` so sibling tasks do not observe bind writes.
- Adapter instances are process-local — parent-process port bindings do not satisfy worker proof; immutable settings cross IPC seams, composition roots reconstruct adapters per interpreter or worker entrypoint.
- Shared mutable singleton backends masquerading as port injection fail under parallel tier traversal and async fan-out — each bind row registers a fresh adapter into the local scope map at root boot; presence and semantic grades re-run at worker entry, not assumed from parent `isinstance` success.

# Negative Conformance By Grade

- **Static negatives** — assignment helpers and `@pytest.mark.xfail` on typed stubs when stubs widen parameters or narrow returns; one defect class per test, not one mega invalid object.
- **Presence negatives** — `not isinstance(partial, Port)` for objects missing any `get_protocol_members` name; undecorated refinement `isinstance` asserts `DeprecationWarning` category under GH-132604 policy before 3.20 hard failure.
- **Semantic negatives** — `TypeIs` predicates return `False` on attestation failure, empty state, or wrong tier; gate negative branches return `(type[Port], type)` or owned fault unions, not bare `TypeError` at domain boundaries.
- **Introspection negatives** — CI frozenset diff fails on unexpected member promotion; `is_protocol` false positives on ABCs caught by matrix rows keyed explicitly on ABC types versus custom ports.
- Happy-path-only `@runtime_checkable` smoke without negative partial-implementer witnesses is half the port contract — each admitted grade owns targeted negative rows.

# Grade Selection And Collapse Tests

- Runtime-only port with no static call sites — collapse to `TypeIs` over nominal owner or delete gate; do not add `@runtime_checkable` solely to feed `isinstance`.
- Single `hasattr` probe suffices — do not upgrade to full protocol `isinstance` when one known attribute is the honest presence evidence.
- Single implementer, no substitution plan — collapse to concrete boundary parameter unless third-party replacement is scheduled within one release cycle.
- Callback-shaped ingress — collapse to `Callable[[Unpack[TypedDict]], R]` with `ParamSpec` preservation, not a one-method protocol shell.
- Stdlib rename duplicate — delete custom port and accept stdlib ABC at boundary when call sites invoke only stdlib surface.
- Per-variant protocol sprawl — collapse to one port over `Member` with interior exhaustive `match` and PEP 800 nominal seals per arm.
- Interior `isinstance` on port symbols — defect; narrowing belongs at composition-root gate or structural decorator prefix; interior trusts static grade after root narrowing.
