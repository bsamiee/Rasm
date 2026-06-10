# Structural Capability Doctrine

# Critical Signals

- Capability assurance temporal law admits two non-overlapping phases — promotion in-flight and assurance-between-promotions — composition root owns phase transitions; steady-state mark and P1–P10 assurance bind only in sustain phase after post-promotion certify; promotion in-flight wins over scheduled replay and deep assurance on affected port edges.
- Admit `typing.Protocol` when structural polymorphism is load-bearing — multiple implementers satisfy one capability without inheritance; reject nominal variant splits, single-callback shells, fat mirrors, and `@runtime_checkable` unions simulating closed variant modality.
- Static structural proof (PEP 544 + PEP 695/696), runtime presence (`@runtime_checkable`), semantic narrowing (`TypeIs`), and introspection (`get_protocol_members`) are non-interchangeable evidence grades — substituting grades at a seam is a lattice defect regardless of checker green.
- Ports compose by intersection, not fat interfaces; scope maps key `type[Port]` and thread through `@effect.result` generators — reject module-level mutable registries and interior `isinstance` repair on narrowed capabilities.
- Kleisli `Arrow` composition threads fault unions across hops with provenance; async and sync port families stay distinct; tier and version gates resolve at composition root before interior bind chains execute.
- Boundary adapters mirror minimal third-party surfaces; export `Protocol` symbols and `resolve_*` factories only — vendor implementations, tier traversal machinery, and pickled capabilities stay inside providing modules.
- Every admitted port registers law witnesses via `register_law`, frozenset member snapshots, negative partial-implementer cases, and metamorphic re-bind chains where egress participates — proof harness order is static → frozenset → runtime → generative → metamorphic.
- Variant identity stays on `@tagged_union` families with exhaustive `match` — at most one port parameterized on `Member`; per-arm protocol sprawl and registry rows typed as protocol pretending to be catalogs are rejected.

# Admission Gate

- Admit `typing.Protocol` when structural polymorphism is load-bearing: multiple independent implementers must satisfy the same capability without inheritance or local subclassing.
- Admit capability slices adapters and test doubles implement from outside the declaring module.
- Admit when static proof must accept third-party or stdlib types that will never subclass a local base.
- Admit when intersection composition is the natural algebra — `Readable[T]` × `Writable[T]` → `Dual[T]` — and callers depend on the smallest sufficient port.
- Reject nominal splits — variant identity, lifecycle state, discriminant tag, sealed family membership — belong to rich class owners or `@disjoint_base` families with `TypeIs` / `match`, not protocol lattices.
- Reject single-callback shells where `Callable`, `ParamSpec`, or `Concatenate` already own the shape.
- Reject when a stronger owner already carries the contract: variant arms → `@disjoint_base` class family; keyword callbacks → `Callable` / `ParamSpec` / `Concatenate`; single-module kernels or runtime-only shells → concrete class, rich owner, or `TypeIs[ConcreteOwner]`; registries, catalogs, materializers, and Pydantic model families → nominal authorities at boundary only; buffer, stream, and stdlib-owned surfaces → stdlib and boundary mirroring.
- Reject `@runtime_checkable` protocol unions standing in for closed variant modality — disjoint seals and exhaustive `match` own arm proof, not runtime member presence.
- Reject nominal registration or codegen tables typed as protocol pretending to be the catalog — rich owner or enum-keyed table owns registration; port stays at injection edge only.
- Reject exception-style control in domain — port methods return `Result` or owned fault unions, not raises across the port.

# Capability Surface

- One admitted protocol owns one capability; vocabulary is a closed `Literal` or `StrEnum` band; ports compose by intersection, not fat interfaces or string switches.
- Name protocols for operation families — `Decode[T]`, `Store[T]`, `Emitter[T]` — not implementer identity.
- Keep method count minimal with PEP 695 type parameters on the protocol itself; each member must be independently satisfiable at the type level.
- Return `Result` or owned fault unions from port methods when failure is domain-shaped.
- Treat `@property`, `@classmethod`, `@staticmethod`, and `__call__` as first-class protocol members when the capability genuinely includes them; do not add members solely to satisfy checker noise.
- Optional capability — transactional batching, no-op hooks, identity scopes — belongs as protocol default members with pure, side-effect-free bodies, not parallel nominal mixin bases or encoded domain policy.
- Freeze capability requirements at decoration or module scope — `frozenset[Cap]`, `Map[Ver, Callable]` — when authorization or version gates depend on port vocabulary.
- Protocol count stays low; capability slices stay small; intersection composes ports; rejection eliminates shells.

# Static Structural Law

- Structural subtyping is enforced by `ty` and strict `mypy`; CPython performs no signature validation at import time; PEP 544 plus PEP 695/696 form the static contract layer on Python `>=3.15`.
- Declare `class Port(Protocol):` or `class Port[T](Protocol):`; implementers need not inherit — matching members suffice; use built-in generics inside protocol members.
- Let PEP 695 infer variance from member positions; mark refinements with `@override`; prefer `TypeIs[T]` for semantic predicates and `assert_never` exhaustiveness over `cast` repair.
- Reserve `TypeIs[T]` (PEP 742) for predicates that prove exact membership including semantic constraints `isinstance` cannot see; static protocol conformance does not replace runtime semantics.
- Static conformance is PEP 544 + PEP 695/696 + `@override` + `Self`; runtime conformance is `@runtime_checkable` or `TypeIs` — never confused with signature proof.

# Self-Preserving Contracts

- Subclass-preserving port APIs use `typing.Self` (PEP 673), not bound `TypeVar` self boilerplate.
- Admit `Self` on fluent port methods and extension members that narrow the same implementer; pair with `@override` when child protocols tighten parent members.
- Reject `Self` on materialization constructors and pure function closures — nominal owners and `Callable` anchors own those return shapes.

# Runtime Boundaries

- Runtime protocol checks are opt-in, coarse, and boundary-local; static ports and runtime gates serve different evidence grades.
- Apply `@runtime_checkable` only where `isinstance` or `issubclass` is an intentional ingress or adapter-selection gate — it checks member presence via `inspect.getattr_static()`, not signatures or types.
- Re-decorate every protocol subclass used in runtime checks because parent decoration does not propagate under Python 3.15 deprecation rules.
- Prefer `TypeIs[Port]` predicates with semantic guards when conformance exceeds attribute existence — attestation, tier, non-empty state, version band.
- Prefer `Result` narrowing (`filter_with`, `or_else_with`) over bare `isinstance` when rejection must carry typed evidence — `(expected_port, actual_type)` tuples or owned fault unions.
- Do not create `@runtime_checkable` protocols solely to feed `isinstance` where `TypeIs` over a nominal owner or `hasattr` on one known attribute is honest about the evidence grade.
- Treat performance-sensitive hot paths as `hasattr` / `TypeIs` candidates; runtime protocol `isinstance` can be surprisingly slow relative to nominal classes.

# Introspection And Registry

- Use `typing.is_protocol(tp)` and `typing.get_protocol_members(tp)` as `frozenset[str]` member sets at composition roots and in test registration rows; `is_protocol` returns `False` for generic aliases, nominal classes, stdlib `io.Reader`, and `collections.abc` ABCs even when runtime-structural.
- Key adapter tables by `type[Port]`, not stringly registry names; pair introspection with explicit port keys in DI maps.
- Drive scope registration and capability tables from `get_protocol_members` in tests and composition roots — assert adapter surfaces remain compatible when external backends change.
- Do not use `get_protocol_members` as a security or validation rail at runtime; absence of a name in the frozenset does not prove implementer correctness.
- Restrict introspection-driven compatibility matrices to test and registration modules — never per-request authorization paths.
- Key adapter registration tables by `type[Port]` with values typed as `Port`, not as `object`.
- Generate compatibility matrices in tests: for each registered backend type, `required <= getattr(backend, "__dict__", {}).keys() | protocol_descriptors` before marking the row compatible.
- Snapshot `frozenset(get_protocol_members(Port))` in golden tests; CI fails on unexpected diffs when port definitions change, forcing paired updates to law witnesses and adapter rows.
- Fail CI when a new port member lands without a corresponding law witness and updated member frozenset assertion.
- Introspection uses `is_protocol` and `get_protocol_members`; DI and scope maps key on `type[Port]`; rich classes and Pydantic own shape; protocols own replaceable capability at boundaries.

# Rich Owners And Pydantic Boundaries

- Rich class owners and Pydantic models are nominal shape authorities; protocols wrap external capability, not internal record identity.
- Keep domain records, sealed variants, and discriminated unions inside model families with `@disjoint_base` seals; accept structural ports on parameters that consume adapters mirroring third-party surfaces.
- Do not subclass `Protocol` from `BaseModel` or mix protocol bases into model MRO; use `Annotated[..., Field(...)]` on fields and `Protocol` on replaceable-backend parameters.
- When a Pydantic model implements port methods for testing, treat it as a test double implementing structure, not as proof that the model family should have been a protocol.
- Place `@runtime_checkable` mirror protocols at IO and persistence boundaries where dynamic backend selection is real; keep model ingress on `model_validate`, `TypeAdapter`, and `TypeIs` over decoded wire shapes.

# Intersection Join Algebra

- Protocol multiple inheritance is the join operator on capability slices; the declaring module owns the meet by accepting only the smallest sufficient port at each call site.
- Write joins as explicit intersection bases — `class Dual[T](Readable[T], Writable[T], Protocol)` — never as widened protocols re-declaring parent members under new names.
- Caller parameters must name the join, not the union of independent checks — depend on `Readable[T]` when read alone suffices; widen to `Dual[T]` only where write is load-bearing in the same operation.
- Capability authorization composes as frozenset subset algebra — `req <= held` on `frozenset[Cap]` — with requirement sets frozen at decoration or module scope; missing-capability evidence names the first set-difference element — `next(iter(req - held))` — as a typed fault payload.
- Reject intersection joins that duplicate members with incompatible signatures; incompatible refinements belong in separate protocol branches with `@override` proof, not in a single join type.

# Variance Through Port Pipelines

- Ports chained in Kleisli composition inherit variance from each hop; pipeline typing preserves contravariance on inputs and covariance on outputs without manual `TypeVar` bound ceremony.
- Treat each port method as `Arrow[A, B, E]` — `Callable[[A], Result[B, E]]` — composed with `bind`; accumulated fault unions thread as `E | F | G`, not as raised exceptions across hops.
- Output-only type parameters covary through read ports; input-only parameters contravary through transform ports; parameters in both positions lock `T` invariant.
- Port-returned faults carry provenance tied to the producing hop — `Refine[T, U]` returns `Hard[T]`, not `Hard[U]` — so recovery context references pre-transform input; exhaust severity or fault unions at composition boundaries with `match` and `assert_never`.

# Reader-Threaded Scope Resolution

- Structural DI resolves capabilities from an explicit scope value threaded through effects, not ambient globals or import-time singletons.
- Key scope maps by `type[Port]` as `Scope = Map[type, object]`; resolve with `ask` as `curry_flip(1)` on `(cap, scope)` producing `Scope -> Result[C, Missing[C]]` where `Missing.capability: type[C]` is evidence — without a nominal `Reader` wrapper type.
- Compose sequential port resolution inside `@effect.result` generators via `yield from ask(Port)(scope)`; register adapters by folding `(type(c), c)` into the scope map — `block.fold(lambda s, c: s.add(type(c), c), scope)` — instead of string registry names or `singledispatch` tables keyed by nominal labels.
- Fork capability mutation with `copy_context().run` via `scoped(caps, thunk)` so sibling tasks do not observe transient scope writes; composition roots inject scope, domain operations consume it read-only during interior folds.
- Reject module-level mutable registries that replace explicit scope threading.

# Async Port Duality

- Sync and async capability are distinct port families; structural conformance does not bridge across scheduling models.
- Admit separate async protocols when the operation family is inherently async — `class AsyncReadable[T](Protocol): async def read(...) -> Result[T, E]: ...` — rather than optional `async` members on a sync port.
- Keep sync port methods synchronous; do not add `async def` stubs that wrap blocking IO merely to satisfy an async call site.
- Thread async ports through `@effect.async_result` generators and `anyio` structured concurrency at composition roots; unify retry and fan-out at the composition boundary with per-arrow retry policy and accumulated `Fault` unions.
- Propagate cancellation as a sum-type member mapped to `Timeout` faults, not bare `asyncio.CancelledError` across port boundaries; collect parallel results into `Block[T]` or `Block[Fault[E]]` with rail inversion at the boundary.

# Conformance Tier Lattice

- Runtime adapter selection orders candidates by evidence strength; static ports define capability, tiers define preference, predicates define semantic sufficiency.
- Model tiers as ordered `Block[tuple[Tier, type[Port]]]` visited in descending strength with short-circuit on first successful conformance.
- Combine `@runtime_checkable` presence with semantic predicates in nested `TypeIs[Port]` functions — PEP 742 complement narrowing applies only when the predicate is a named function with return annotation.
- Wrap accepted adapters in provenance carriers — `Certified[C](inner, attestation, tier)` — when attestation, tier, or audit metadata must travel with the narrowed capability; implement tier traversal as `block.fold` with `or_else_with` on `Result`.
- Encode rejection as structured tuples — `(expected_cap, actual_type, tier)` — sufficient for diagnostics and `match` routing without parallel error dataclass proliferation.
- Reject flat `isinstance` ladders without tier ordering when multiple adapters satisfy the same port; ordering is part of the conformance contract.

# Version-Gated Capability Surfaces

- Replaceable backends evolving across API versions expose version metadata on the port or companion `Versioned` protocol, not as parallel string parameters.
- Index implementations in `Map[Ver, Callable[..., Result[T, E]]]` where `Ver` is a closed `Literal` band; gate deprecation with `Map[Method, tuple[Ver, Ver]]` keyed by `StrEnum` method identities.
- Extract offered version from positional arguments via `block.try_find(lambda a: isinstance(a, Versioned))` defaulting to the first known route when callers omit versioned adapters.
- Thread version support, sunset threshold, and dispatch as a three-stage Kleisli chain — `try_find(v).to_result(...).bind(adapter).bind(call)` — with `Unsupported` and `Sunsetted` faults owned at the gate; interior domain ports consume already-resolved capabilities.

# Port Fault Severity

- Port methods return `Result` with faults classified by recoverability; composition routes on discriminant properties, not exception types or string codes.
- Define port-local fault unions with `@tagged_union` when severity or recoverability is load-bearing — `absent` recoverable, `denied` terminal — and expose routing via `@property` discriminants such as `.recoverable`.
- Use `filter_with` on `Ok` for authorization gates; accumulate recoverable faults in folds with `or_else_with` while short-circuiting terminal faults through the same path.
- Keep port fault variants file-internal until context seams; seam adapters map port fault unions into boundary `Result` carriers inside `capture` / `async_capture` — sole permitted translation site from port-local severity to shared domain faults.
- Recoverable port faults may accumulate in partial-success maps at the seam — terminal port faults short-circuit without mutating canonical identities already materialized in the same transaction.
- Version-gated and tier-gated rejection evidence projects to boundary diagnostics at the seam; interior transforms do not re-encode tier vocabulary.
- Error type changes at the seam require rail type changes on the exported handoff — adding exception branches on projected port wrappers is a binding defect.

# Structural Conformance Gates

- Structural conformance gates narrow admitted capabilities before the domain operation body executes.
- Implement gates as `filter_with` on `Ok(obj)` preceded by `TypeIs[Port]` — never bare `isinstance` followed by `cast`; preserve callable shape with `Concatenate[Port, P]` and PEP 695 `**P`.
- Precompute requirement frozensets per intersection join algebra at decoration time; runtime work is narrowing and invocation only.
- Stack gates in canonical outer-to-inner order — trace, authorize, validate, cache, govern, retry, structural conform, operation — so authorization sees identity before capability narrowing when both apply.
- Return rejection evidence as `(type[Port], type)` tuples or owned fault unions from the negative branch; do not raise `TypeError` for expected adapter mismatch at domain boundaries.
- Validation decorators admit boundary-normalized values on `Result` rails — structural conformance gates narrow port parameters with `TypeIs` and `filter_with`, not bare `isinstance` plus `cast`.
- Registration decorators on handlers require canonical domain types in signatures before registry insertion — port parameters appear on boundary adapters and composition-root factories, not on catalog row handlers unless the handler is explicitly a capability entrypoint.
- `ContextVar` resolution for scope maps belongs in absorb-style wrappers at the stack edge — smart constructors and model validators do not call `ask(Port)` from ambient context.
- Async structural gates use the async port family at the same stack position as sync gates — scheduling-model mismatch is a definition defect caught at static proof, not at runtime `await` sites inside sync domain logic.
- Kleisli `Arrow` composition, async/sync port duality, `type[Port]` scope maps, tier and version gates, and structural decorators own ingress evidence; domain interiors trust narrowed ports.

# Protocol Extension And Refinement Chains

- Child protocols refine parent contracts by tightening return types or narrowing parameters; the refinement chain is a nominal typing artifact, not runtime inheritance.
- Declare `class Narrow(Wide, Protocol):` with `@override` on every redeclared member; keep refinements capability-aligned — same operation family, not identity refinements that merely rename members.
- Re-apply `@runtime_checkable` on every subclass in the refinement chain used with `isinstance` or `issubclass`; parent decoration does not propagate under Python 3.15 deprecation rules.
- Use `Self` on refinement methods only when the implementer family returns its own runtime type through the narrowed port; test refinement chains with `get_protocol_members` on each link — child member sets must be supersets of parent required names with compatible semantics.

# Stdlib And Boundary Mirroring

- Stdlib and `collections.abc` already own several capability contracts; custom ports that merely rename those surfaces duplicate evidence without load-bearing domain extension.
- Accept `collections.abc.Buffer`, `__buffer__`, `io.Reader`, and `io.Writer` (Python 3.14+); reject revived `typing.IO`, `TextIO`, and `BinaryIO` pseudo-protocol patterns.
- Prefer `Callable` with preserved `ParamSpec` over one-method protocol shells; admit custom ports only when domain methods genuinely extend the stdlib surface beyond the ABC.
- Delegate iterable, mapping, and async iterable contracts to the matching `collections.abc` ABC when the stdlib surface is the actual boundary.
- Boundary mirror protocols extract minimal call-site surfaces at composition or boundary modules; reject mirrors wider than invoked need.

# Third-Party Surface Mirroring

- Boundary adapters declare a structural port that mirrors the replaceable backend's operation family — not the backend's nominal class hierarchy.
- Extract the minimal method set the owning module actually invokes and declare them on `@runtime_checkable` `Protocol` when ingress or adapter selection requires `isinstance`.
- Mirror third-party signatures faithfully — parameter names, keyword-only markers, return unions — so stdlib and ecosystem types assign without adapter shims when structure already matches.
- Place mirror protocols in the composition or boundary module that owns injection, not in domain transforms; domain code depends on the port parameter, never on the third-party import.
- Reject mirror protocols that duplicate `collections.abc` or `io` contracts under new names; extend stdlib ports only when domain methods genuinely augment the stdlib surface.
- Reject mirror protocols wider than call-site need — fat ports with unused members increase conformance proof surface without load-bearing capability.
- Backend SDK upgrades that alter invoked signatures trigger mirror port edits; SDK methods not called by the owning module do not expand the mirror surface.

# Protocol Default Members And Optional Capability

- Optional capability on a port — transactional batching, no-op hooks, identity scopes — belongs as protocol default members, not as parallel nominal mixin bases.
- Implement defaults on the `Protocol` body — `@property def transaction(self) -> AbstractContextManager: return nullcontext()` — so non-capable backends inherit usable behavior without override ceremony.
- Re-bind protocol default descriptors on concrete test doubles — `transaction = Port.transaction` — to prove default-member semantics independent of a production backend.
- Keep default bodies pure and side-effect free at property access; expensive setup belongs inside the returned context manager's `__enter__`, not at descriptor evaluation.
- Reject default members that encode domain policy — authorization tiers, version bands, fault routing — defaults express absent optional capability, not business rules.
- Reject nominal ABC bases solely to supply defaults when a protocol default member preserves structural assignability for third-party implementers.

# Structural Delegation

- Transparent forwarding must preserve static assignability; nominal wrapper classes break structural subtyping unless every member re-exports with matching signatures.
- Prefer direct injection where structure matches; compose cross-cutting concerns with `@effect.result` pipelines at the composition root, not `class Wrapper(Port)` duplicating every method.
- Admit boundary-local nominal delegate capsules only when every port member forwards through typed one-liners with preserved `ParamSpec` on `__call__` members; reject deep wrapper chains using string dispatch.
- Preserve `ParamSpec` and keyword shape on forwarded `__call__` members; erased `getattr(self._inner, name)(*args)` wrappers fail static proof.
- Reject deep wrapper chains that re-implement port surfaces by string dispatch; each hop must carry `@override`-checked signatures or fold into a single boundary pipeline.

# Implementation Opacity At Export Seams

- Cross-context integration exports port definitions and boundary adapters; concrete backend classes stay inside the providing module.
- Export `Protocol` types and `type[Port]` keys through `__all__`; hide adapter construction, credential binding, and backend-specific configuration behind composition-root factories.
- Consuming contexts import the port plus a narrow adapter function — `resolve_filesystem(settings) -> Result[ArtifactFileSystem, ConfigFault]` — not the vendor client module.
- Pair exported ports with documented conformance tier expectations when multiple backends exist — primary, secondary, fallback — without exporting tier traversal machinery.
- Reject re-export of third-party classes as public surface merely because they structurally satisfy a port; stability belongs to the port contract, not vendor type names.
- Reject domain modules that import backend implementations for convenience; scope maps and composition roots bind implementations to `type[Port]` entries.

# Conformance Proof Obligations

- Port correctness is provable through static assignability, runtime structural checks, and introspection member sets — not nominal inheritance declarations.
- Static proof: call sites type parameters as the port; `ty` and strict `mypy` reject members outside `get_protocol_members(Port)`.
- Runtime proof pairs `@runtime_checkable` smoke with semantic `TypeIs` when attestation is load-bearing; assignment proof uses bare structural third-party values without nominal `class Adapter(Port)` unless signatures genuinely translate.
- Default-member proof: at least one test double omits optional overrides and re-binds protocol defaults to exercise descriptor semantics on the conformance surface.
- Drift proof: when backend SDKs add methods, `get_protocol_members` diff tests fail only if the mirror port changes; silent SDK drift must not widen domain call sites.

# Structural Test Doubles And Law Registration

- Test doubles implement port structure directly — stub classes with typed members — and register as law witnesses tied to the port symbol.
- Build doubles as minimal structural classes with only invoked members satisfying the port without inheriting from it.
- Register conformance laws with `register_law(Port, "slug")` so policy runners associate proof witnesses with the port owner, not with individual backend brands.
- Drive generative cases from `get_protocol_members(Port)` — for each required name, assert the double defines a callable or descriptor before exercising behavior laws.
- Co-locate port law modules with boundary tests; domain unit tests receive port-typed fakes through fixtures, not through production backend imports.
- Reject `unittest.mock.Mock` as stand-in for port conformance — erased mocks do not prove signature compatibility and hide missing-member drift.

# Negative Conformance And Partial Implementers

- Proof must witness rejection: partial implementers, wrong return types, and signature skew are first-class negative cases.
- Assert `not isinstance(partial, Port)` for objects missing any required member name from `get_protocol_members(Port)`.
- Assert static rejection via `typing.assert_type` helpers or `@pytest.mark.xfail` on typed assignments when a stub widens parameters or narrows returns against the port.
- Assert gate negative branches return owned faults — `(type[Port], type)` tuples — when AOP structural decorators wrap port parameters; do not rely on raised `TypeError` as the only witness.
- Table negative cases by missing member, wrong async/sync scheduling, and invariant type-parameter misuse — each defect class maps to one targeted test, not one mega invalid object.
- Reject tests that only witness happy-path `isinstance`; negative conformance is half the port contract.

# Serialization And Cross-Process Limits

- Structural conformance is a static and in-process contract; serialization and IPC re-establish capability at the remote boundary.
- Do not pickle or msgpack-encode port-typed capabilities as proof of remote structural assignability; transmit handles, URIs, or configuration slices that the remote composition root resolves to a fresh port implementation.
- Treat `Protocol` types as non-serializable type witnesses; rebuild scope maps at remote roots — not bound port instances or shared mutable singleton backends across processes.
- When multiprocessing workers need filesystem or transport capability, pass immutable settings and reconstruct adapters inside the worker entrypoint — not the bound port instance from the parent.
- Mark port parameters as `# noqa: TC001` or move to `TYPE_CHECKING` only when annotations are never evaluated at runtime; cross-process entrypoints that inspect annotations need runtime-importable port symbols.
- Reject shared mutable singleton backends masquerading as port injection across processes; each process owns an adapter instance registered into a local scope map.

# Stack Placement And Pipeline Anchoring

- Structural ports occupy the capability layer between immutable collection algebra and rail interior — they name replaceable operations, not domain invariants, variant identity, or wire field layout.
- Port injection completes at composition-root boot after settings validate and before interior `@effect.result` transforms execute; domain modules receive port-typed parameters from scope maps, not from ingress models or wire structs.
- Ingress and wire projections never declare `Protocol` field types — foreign bytes materialize to canonical records first; adapters selected by port conformance attach at the root handoff, not inside Pydantic or msgspec field definitions.
- Materialization stage law: validation and normalization stages admit no port parameters; construction and enrichment stages consume canonical shapes only; port-backed side effects invoke through injected capabilities after canonical materialization exit when load-bearing in the same unit of work.
- Persistence read paths rebuild adapters inside worker entrypoints from immutable settings — parent-process port instances do not cross process seams as structural proof; configuration slices cross, capabilities re-resolve locally.
- Trusted-replay decode may skip re-validation of stored canonical graphs but still re-binds port implementations from pinned boot records — replay trust applies to shape bytes, not to backend identity surviving transfer.

# Cross-Axis Seam Handoffs

- Settings boot → domain start: scope map populated with `type[Port]` bindings; handoff artifact is frozen settings plus `Map[type, object]`; composition root owns the seam.
- Foreign ingress → canonical: no port on ingress fields; adapter selected after `materialize_*`; handoff artifact is `Result[Canonical, BoundaryError]`; materialization pipeline owns the seam.
- Canonical transform interior: port parameters only; no `isinstance` repair; handoff artifact is `Result[Canonical, E]`; rail interior owns the seam.
- Canonical → wire egress: optional encode/store port at boundary pipeline; handoff artifact is `WireStruct` bytes; projection lattice owns the seam.
- Cross-context import: exported `Protocol` plus factory; implementation opaque; handoff artifact is port-typed adapter from `resolve_*`; shape system integration owns the seam.
- Variant arm dispatch: `Member` plus exhaustive `match`; not per-arm ports; handoff artifact is materialized union member; class family variant owns the seam.
- Decorator ingress gate: structural `TypeIs[Port]` before body; handoff artifact is `(type[Port], type)` fault or narrowed capability; decorator admission owns the seam.
- Async fan-out boundary: async port family at composition root only; handoff artifact is `Block[T]` or accumulated `Fault`; capability lattice owns the seam.
- IPC and worker spawn: settings in; fresh adapter registration; handoff artifact is immutable boot config, not bound port instance; boundary projection owns the seam.
- Chained seams compose as typed pipelines: settings boot → scope bind → canonical materialize → interior bind over ports → wire project — erased `object` capability slots between steps are rejected.

# Variant Modality Exclusion

- Closed variant identity stays on `@tagged_union` or discriminated Pydantic families with `match` and `TypeIs` — ports implement operations over `Member` parameters, not one protocol per arm.
- Admit at most one port parameterized on a family alias — `Processor[Member]` with exhaustive interior `match` — when algorithms are parametric across arms; reject `CardHandler` / `BankHandler` protocol siblings inside one bounded context.
- `@runtime_checkable` protocol unions simulating closed variant sets are rejected at integration seams — runtime presence cannot discharge exhaustiveness; disjoint seals own modality proof.
- Plugin extension at system boundary uses `singledispatch` or registry rows keyed by foreign nominal types — not protocol branches for in-repo variant arms; open extension and closed families occupy disjoint seams.
- Port-local fault unions stay file-internal — cross-context domain errors export minimal shared carriers with enum or `Literal` discriminants at the seam, not port fault trees re-wrapped as domain variants.

# Rail Interior Consumption Law

- After composition-root narrowing, interior transforms treat port parameters as statically satisfied — no `isinstance`, `hasattr`, or `cast` repair on capabilities mid-pipeline.
- Port methods return owned fault unions into the same `Result` rail as canonical transforms — faults from port hops accumulate with provenance; interior code does not catch vendor exceptions and re-wrap as domain types.
- Scope maps are read-only during interior folds — transient adapter substitution uses `scoped(caps, thunk)` at composition boundaries, not mutable module registries mid-transform.
- Generic port bounds on interior parameters use family aliases or canonical owner types — `T: Member`, not bare `object` or erased `Any`.
- Re-entry of foreign carriers into interior pipelines rematerializes canonical shapes first — ports do not accept wire structs or ingress models as substitute domain owners.

# Composition Root Port Wiring

- Root wiring declares a typed handoff graph per bounded context: `settings -> scope bind -> foreign_ingress -> Result[Canonical, E] -> interior(Result) -> Canonical -> foreign_wire`, with port resolution nodes only at bind and boundary pipeline hops.
- Adapter factories return `Result[Port, ConfigFault]` — credential binding, endpoint selection, and tier preference resolve at the root; consuming modules import the port type and factory, not vendor client classes.
- Scope population folds `(type(c), c)` registrations immutably keyed by `type[Port]`; string registry names do not appear in the handoff graph.
- Every replaceable backend maps to exactly one admitted port and one composition-root bind row.
- Multi-backend contexts document tier expectations beside factory exports without exporting tier traversal machinery to domain interiors.
- Import-cycle resolution for port mirrors and adapters belongs at the composition root — acyclic domain modules within a context import port symbols from boundary packages only.

# Open Extension Versus Structural Ports

- Closed domain dispatch uses `match` on canonical unions with `assert_never` — open extension at foreign boundaries uses `singledispatch` or typed registry rows keyed by verified foreign types, not protocol lattices per plugin brand.
- Structural ports admit third-party and stdlib implementers with independent class hierarchies — registry rows admit in-repo handlers over canonical shapes; the two mechanisms do not substitute for one another at the same seam.
- When a foreign plugin exposes a nominal base, mirror its method surface as a port at the injection boundary — domain code depends on the port; plugin discovery uses tier lattice plus `TypeIs`, not subclass checks against local nominal bases.
- Catalog enums and dispatch tokens supply wire routing metadata — handlers remain `Callable[[Canonical], Result[...]]`; ports supply IO, codec, and transport capability, not dispatch table identity.

# Port Evolution And Member Lifecycle

- Python `>=3.15` governs how admitted structural ports evolve under backend SDK drift, tier policy changes, and cross-context consumer updates — port member promotion, law witness refresh, and scope bind rows change in one governance unit; proof harnesses attribute capability defects to the port owner before domain transforms or backend brands are suspect.
- Port vocabulary evolves through closed capability bands — `StrEnum` tokens, `Literal` version arms, frozenset authorization sets — declared beside the port owner; free strings and unbounded ints do not index adapter rows or tier tables.
- Adding a required protocol member is a breaking evolution event — simultaneous updates to mirror extraction, `get_protocol_members` frozenset snapshots, structural test doubles, negative partial-implementer cases, scope bind rows, and every `register_law(Port, ...)` witness block merge until the promotion unit completes.
- Optional capability arrives through protocol default members on the port body — not through parallel nominal mixin bases or silent no-op overrides scattered across backend modules; default-member proof re-runs when defaults change semantics.
- Deprecating a port member retires call sites before the member disappears from `get_protocol_members` — intermediate releases mark the member documented-only with static checker warnings where supported; removal requires zero production invocations verified by usage-graph diff, not comment-only deprecation.
- Splitting one port into intersection joins — `Readable[T]` × `Writable[T]` — is preferred evolution over widening a monolith; collapse the inverse when a join has a single production implementer and no planned substitution.
- Port refinement chains — `Narrow(Wide, Protocol)` with `@override` — evolve as ordered links; inserting a middle refinement updates runtime `@runtime_checkable` re-decoration on every subclass in the chain used with `isinstance` or `issubclass`.
- Version-gated adapter surfaces evolve through closed `Ver` literal bands on companion `Versioned` metadata — sunset thresholds and unsupported-version faults update in the same unit as `Map[Ver, Callable]` row changes; interior domain ports never re-check version bands after root resolution.

# Simultaneous Port Update Contract

- Backend SDK upgrades that alter invoked signatures trigger mirror port edits, static assignability proofs, and assignment tests with bare structural values — SDK methods not called by the owning module do not expand the mirror surface.
- Tier lattice reordering — primary, secondary, fallback — updates composition-root traversal tables, tier-gated fault payloads, and smoke tests that assert short-circuit on first successful conformance in the same change.
- Scope map schema changes — new `type[Port]` keys, removed bindings, factory signature changes — propagate through root handoff graphs, negative import tests, and parallel `scoped` immutability witnesses together.
- Cross-context consumers receive port symbol stability — exported `Protocol` names and `type[Port]` keys change only with explicit anti-corruption remap at the importing boundary module; silent re-exports of renamed ports fail consumer seam tests.
- Async port family introduction splits sync implementations — new `@effect.async_result` generator sites and async mirror protocols land with sync call sites unchanged; bridging sync blocking IO through `async def` stubs on sync ports is forbidden as an evolution shortcut.
- Authorization frozenset bands on gated ports evolve through subset algebra only — adding a capability token updates decoration-time frozensets, gate negative-branch fault shapes, and authorization law rows; string comparison gates are not an interim migration path.

# Port Proof Harness Architecture

- Proof harness layers stack orthogonally for ports: static structural assignability (`ty`, strict `mypy`, `@override`, PEP 695 variance), introspection tables (`is_protocol`, `get_protocol_members` frozensets), runtime structural smoke (`@runtime_checkable` plus semantic `TypeIs` where attestation is load-bearing), generative backend coverage, and integration re-bind metamorphic chains.
- Harness execution order: static checkers and member frozenset snapshots before runtime smoke before generative laws before cross-process re-bind properties — failures at static layer block expensive backend generative runs.
- Port law modules co-locate with boundary packages — witnesses import port symbols and adapter factories from the providing context; domain modules under test never import vendor backends for proof convenience.
- Proof debt from checker gaps tracks on the language axis — port declarations stay spec-complete; `# type: ignore`, `cast`, and harness suppressions at port seams are merge blockers.
- Arch import rules extend to ports: domain folders never import vendor client modules; consuming contexts import port contracts and `resolve_*` factories only; grep for production backend imports in non-boundary packages fails CI.
- Mutation testing targets boundary adapter pipelines and tier traversal when seam logic is dense — interior domain folds on already-narrowed ports are secondary mutation surfaces.

# Static And Runtime Conformance Matrix

- Representative port owners exercise the matrix in fixed modules: minimal single-method ports, generic `Port[T]` with invariant parameters, intersection joins, refinement chains with `@override`, `@runtime_checkable` subclasses requiring re-decoration, and protocol default members with test doubles that re-bind defaults.
- Static matrix rows assert assignability of third-party structural values without nominal adapter subclasses — production paths that require `class Adapter(Port)` trigger mirror or delegation review before merge.
- Runtime matrix rows pair `@runtime_checkable` presence checks with semantic predicates — attestation, non-empty state, version band — when PEP 544 presence alone is insufficient evidence grade.
- Negative matrix rows catalog partial implementers by defect class — missing member, wrong async/sync scheduling, parameter widening, return narrowing — one targeted test per class, not one invalid object covering all defects.
- `get_protocol_members` golden frozensets snapshot per admitted port — CI diffs fail on unexpected member set changes, forcing paired updates to law witnesses, mirror docs, and scope bind rows in the same promotion unit.
- Cross-checker parity: pyright, mypy, and ty run on the same representative port modules — one green backend does not relax declarations required by the others.

# Metamorphic Capability Re-Bind Chains

- Full stack metamorphic law when port-backed egress participates: canonical materialize → interior transform invoking port → wire project → decode → `materialize_*` → fresh scope bind at worker entrypoint → interior re-invoke — shape round-trip and capability re-bind are independent proof obligations.
- Re-bind metamorphic law asserts adapter identity differs across process seams while canonical identity holds — parent-process port instances never satisfy worker proof; immutable settings cross, adapters reconstruct locally.
- Tier metamorphic law: ordered candidate backends with semantic predicates — first `Ok(Certified[C])` short-circuits; property tests vary candidate order and assert tier evidence in fault payloads on rejection paths.
- Version-gate metamorphic law: adapter rows keyed by closed `Ver` literals — unsupported and sunsetted faults surface at ingress gates with owned fault unions, not vendor exceptions mid-pipeline.
- Trusted-replay chains pin boot records for encoder identity, store key, schema version, and port factory module — replay skips re-validation of stored canonical graphs but re-binds port implementations from pinned records; replay trust does not transfer bound port instances.
- Subset metamorphic declarations document which port operations participate in round-trip — read-only observation ports may omit write members from re-bind chains with explicit rationale in the law module.

# Failure Archaeology And Port Diagnostic Routing

- Failure attribution follows port-owner-first routing: static assignability rejection → mirror port or refinement chain; `@runtime_checkable` false negative → missing member or unredecorated subclass; semantic `TypeIs` rejection → attestation or tier predicate owner; scope `Missing[Port]` → composition-root bind row or `scoped` fork defect.
- Adapter mismatch at AOP structural gates surfaces as owned fault tuples — `(type[Port], type)` — attribution targets gate decorator and declared port requirement, not the wrapped domain operation.
- Port method faults returned on `Result` rails attribute to the producing hop — provenance references pre-transform input on contravariant refine ports; interior code that catches vendor exceptions and re-wraps indicates seam placement defect.
- Cross-context handoff failures with pickled or shared mutable backends attribute to boundary projection owner — ports do not survive wire transfer as structural proof; diagnostic tags name `rebind`, `scope`, or `ipc`, not undifferentiated adapter errors.
- Version and tier rejection evidence — `(expected_cap, actual_type, tier)` — routes to conformance tier owner and root factory configuration; interior transforms re-encoding tier vocabulary indicate context leak.
- Observability spans on seam adapters emit port key, tier selected, and factory identity at bind time — interior domain spans omit vendor module paths; trace vocabulary reuses `Cap` enum owners where capability tokens are load-bearing.

# CI Regression Gates For Port Drift

- New required port member without updated frozenset snapshot, law witness, negative partial-implementer test, and scope bind documentation fails merge — silent SDK drift must not widen domain call sites.
- `@runtime_checkable` subclass used in `isinstance` without re-decoration fails lint policy under Python 3.15 deprecation rules — parent decoration does not propagate.
- Ingress or wire model field typed as `Protocol` fails seam altitude checklist — capability moves to constructor or operation parameters injected at root.
- Three or more protocols differing only by variant arm identity fail per-variant sprawl gate — collapse to one port over `Member` with interior exhaustive `match`.
- Vendor adapter class appearing in public `__all__` fails export opacity gate — port symbol and factory only.
- Module-level mutable registry replacing explicit scope map fails ambient-registry gate — `Map[type, object]` threads from root.
- Stringly capability authorization comparing raw tokens instead of frozenset subset algebra fails gate policy lint.
- Interior domain package grep for `isinstance` on port symbols fails interior-trust gate — narrowing belongs at composition-root or AOP structural decorator.

# Law Registration And Witness Catalog

- Every admitted port symbol registers at least one law via `register_law(Port, "slug")` — slug names the proof family: static assignability, runtime smoke, default-member, negative partial, tier traversal, version gate, or re-bind metamorphic.
- Witness catalog rows keyed by port symbol list required frozenset snapshot hash, registered slugs, bound backend types in smoke tests, and consumer contexts importing the port — orphan ports without catalog rows fail arch review.
- Generative laws draw from `get_protocol_members(Port)` — for each required name, assert double defines callable or descriptor before behavior properties execute.
- Structural test doubles remain minimal — invoked members only — and register as law witnesses on the port owner, not on individual backend brand modules.
- `unittest.mock.Mock` and erased stand-ins do not satisfy law registration — mocks hide missing-member drift and fail signature compatibility proof.
- Law-matrix decorators preserve collected signatures on integration tests — hypothesis strategy parameters follow decorator-admitted-shape rules for visible pytest signatures.
- Fold-derived port fault severity routing laws assert recoverable versus terminal discriminants propagate through composition boundaries without mutating canonical identities already materialized in the same transaction.

# Consumer Seam Contracts

- Providing contexts publish stable port symbols, `type[Port]` keys, narrow `resolve_*` factories, documented tier expectations, and conformance tier vocabulary — vendor implementations, credential modules, and traversal machinery stay private.
- Consuming contexts import through anti-corruption boundary modules — port contract plus factory only; production wiring paths satisfy negative import tests that reject vendor implementation modules.
- Consumer upgrades consume port evolution as documented promotion units — member additions, refinements, and retired members appear in changelog rows tied to frozenset diff and law slug updates; consumers do not pin vendor classes.
- Shared replaceable backends unify under one providing context — parallel identical mirror ports in two contexts collapse to one vocabulary module imported by both; duplicate mirror maintenance is a merge blocker.
- Cross-context handoff transmits canonical shapes, fold-derived receipts, or immutable configuration records — not port instances, picklable capability handles, or shared mutable singleton backends.
- Consumer observability assumes traced canonical values and already-narrowed ports at interior entry — port selection evidence attaches at bind time in the providing context, not per call in consuming domain code.

# Evolution Anti-Patterns

- Widening a port mirror to match full vendor SDK surface when call sites invoke a subset — fat mirrors increase proof obligation without load-bearing capability.
- Removing a port member while production usage graph still references it — breaks structural assignability silently until runtime smoke fails.
- Evolution through nominal wrapper classes that duplicate every port member — transparent structural injection or boundary `@effect.result` pipelines preserve assignability.
- Introducing `@runtime_checkable` solely to feed `isinstance` where `TypeIs` over a nominal owner or single `hasattr` probe is honest about evidence grade.
- Sync/async bridge by declaring `async def` on sync port methods for blocking IO — split port families at evolution time.
- Domain smart constructors or Pydantic models implementing port methods for convenience — extract operations to boundary adapters; models remain nominal shape authorities.
- Registry row typed as protocol pretending to be the catalog — rich owner or enum-keyed table owns registration; port stays at injection edge only.
- Pickling port-typed capabilities across processes as proof of remote assignability — settings cross; adapters rebuild at remote root.

# Proof Anti-Patterns

- Happy-path-only `@runtime_checkable` smoke without negative partial-implementer witnesses — negative conformance is half the port contract.
- Golden frozenset snapshots updated without paired law witness and adapter row changes — introspection green while behavior laws stale.
- Generative tests using unfiltered object factories filtered only by runtime validation — wastes cycles; table-driven doubles from `get_protocol_members` precede behavior properties.
- Integration tests importing vendor backends in domain packages for convenience — bypasses export opacity and consumer seam law.
- Proof suite green on one type checker while matrix modules fail on others — checker gaps are not criteria for omitting port declarations.
- Law registration on pytest file paths instead of port symbols — witnesses must bind to the port owner for harness discovery.
- Metamorphic re-bind chains asserting same adapter instance identity across process spawn — contradicts boundary projection doctrine.
- Tier traversal tests that evaluate weaker backends after first success — short-circuit supremum is part of the conformance contract.

# Integration Proof Matrix

- Stack role coverage: every replaceable backend in a bounded context maps to exactly one admitted port and one composition-root bind row; orphan adapters without port symbols or ports without bind rows fail arch review.
- Seam altitude: no port-typed field on ingress, wire, or settings projections; checklist fails when static signatures show `Protocol` members on lattice siblings other than boundary factory returns.
- Interior trust: domain modules under test receive port doubles through fixtures without `isinstance` patches; grep for `isinstance` on port symbols inside non-boundary packages is a drift signal.
- Negative seam witness: cross-context import tests assert consuming modules cannot import vendor implementation modules; root factories alone satisfy production wiring paths.
- Scope immutability: parallel tests using `scoped` prove sibling tasks do not observe transient bind mutations from concurrent folds.
- Cross-axis round-trip: when port-backed egress participates in persistence, property tests exercise canonical → wire → decode → `materialize_*` with fresh port bind at the worker entrypoint — shape round-trip and capability re-bind are independent proof obligations.
- Law registry linkage: every admitted port symbol has at least one `register_law(Port, ...)` witness and a `get_protocol_members` frozenset snapshot in CI — integration defects surface at the port owner before backend brands are suspect.

# Port Drift Signals

- Ingress port field: Pydantic or msgspec model declares a `Protocol`-typed field; move capability to constructor or operation parameter injected at root.
- Per-variant port sprawl: three or more protocols differing only by arm identity; collapse to one port over `Member` with interior exhaustive `match`.
- Fat mirror: boundary port declares members no call site invokes; trim mirror to minimal surface from `get_protocol_members` diff against usage graph.
- Wrapper export: vendor adapter class appears in `__all__`; hide implementation, export port and factory only.
- Mid-pipeline isinstance: domain transform re-checks port conformance; move narrowing to composition-root gate or AOP structural decorator.
- Ambient registry: module-level mutable dict replaces explicit scope map; thread `Map[type, object]` from root.
- Sync/async bridge: sync port method declared `async def` for blocking IO; split into async port family at root.
- Pickled capability: multiprocessing handoff serializes port instance; pass settings and reconstruct adapter in worker.
- Stringly capability gate: authorization compares raw capability strings instead of frozenset subset algebra on closed `Cap` bands.
- Protocol table owner: registry row typed as protocol pretending to be the catalog; rich owner or enum-keyed table owns registration; port stays at injection edge only.

# Harness And Collapse Tests

- Single implementer port: one production backend, no planned substitution, no generative law requirement; collapse to concrete boundary parameter type unless third-party replacement is scheduled within one release cycle.
- Runtime-only port: exists solely for `isinstance` without static call sites; collapse to `TypeIs` over nominal owner or delete gate.
- Callback-shaped ingress: one method with keyword payload; collapse to `Callable[[Unpack[TypedDict]], R]` with `ParamSpec` preservation, not a protocol shell.
- Stdlib rename: custom bytes or stream port duplicates `collections.abc.Buffer` or `io.Reader`; delete custom port and accept stdlib contract at boundary.
- Nominal mixin defaults: ABC base exists only for default member behavior; fold defaults into protocol body.
- Domain record protocol: Pydantic model implements port for convenience; model stays nominal shape authority; extract operations to boundary adapter implementing structure.
- Duplicate cross-context mirror: identical mirror ports in two bounded contexts; promote one owner to shared boundary vocabulary module.
- Mock-backed law witness: erased mock satisfies smoke test; collapse to structural double with typed members from `get_protocol_members`.
- Fat mirror unused members: usage graph empty for declared members; trim mirror to minimal invoked surface.
- Parallel checker exemptions: port module passes one backend with suppressions; collapse proof debt to language axis and restore spec-complete declarations.
- Done when every admitted port has catalog row, frozenset snapshot, registered law slugs, static/runtime matrix coverage, consumer seam negative imports, and metamorphic re-bind witnesses where egress participates — and every harness collapse row is absent from the bounded context.
- Composition root exports temporal witness row with correct phase at all times — steady-state mark holds only in `assurance_between_promotions` with post-promotion certify current per capability assurance temporal law in Critical Signals.

