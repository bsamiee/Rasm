# Kleisli Port Pipeline And Fault Threading — Arrow Bind, Variance, And Seam Projection

# Arrow Typing Over Admitted Ports

- Treat each port method as one Kleisli arrow `Callable[[A], Result[B, E]]` — input contravariant, output covariant, fault union owned at the hop that produced it.
- Name pipeline stages by capability family, not backend brand — `decode.bind(store).bind(emit)` where each symbol is a port-typed callable extracted from scope, not vendor client methods.
- Generic port parameters lock invariant when the same `T` appears in contravariant and covariant positions on one hop — split read and transform ports when variance axes diverge instead of erasing to `object`.
- PEP 695 type parameters on `Port[T]` bind at call sites; pipeline composition preserves parameter instantiation through each hop without manual `TypeVar` bound ceremony when ports declare variance-correct members.
- Reject pipelines typed as `Callable[..., object]` or bare `Any` between port hops — erased intermediates hide fault accumulation, scheduling mismatch, and provenance loss at seam review.

# Kleisli Bind And Sequential Resolution

- Scope maps key by `type[Port]` as `Map[type, object]`; `ask` resolves as `curry_flip(1)` on `(cap, scope)` producing `Scope -> Result[C, Missing[C]]` where `Missing.capability: type[C]` is typed evidence — not a nominal `Reader` wrapper or string registry alias.
- Sequential port resolution inside `@effect.result` generators uses `yield from ask(Port)(scope)` before invoking port methods — scope map lookup precedes method dispatch; missing keys surface as `Missing[Port]` with `capability: type[Port]` evidence, not bare `KeyError`.
- Kleisli bind over port arrows threads the success value forward and short-circuits on first terminal fault — `bind` does not catch vendor exceptions and re-wrap; port methods return owned fault unions into the same rail.
- Compose cross-cutting retry, trace, and authorization as outer policy wrappers on the pipeline shell at the composition root — interior domain folds invoke already-narrowed port callables, not re-decorated vendor surfaces.
- Scope population folds `(type(c), c)` immutably — `block.fold(lambda s, c: s.add(type(c), c), scope)` — string registry names and module-level mutable dicts do not appear between bind hops.
- Fork transient capability substitution with `scoped(caps, thunk)` via `copy_context().run` — sibling async tasks and parallel tier traversal must not observe mid-pipeline scope writes from concurrent folds.

# Variance Preservation Through Multi-Hop Pipelines

- Read-only hops covary output type parameters — `Readable[T]` in `T -> Result[U, E]` allows `U` wider than `T` only when the port contract explicitly widens; checker proof rejects silent narrowing on read paths.
- Transform hops contravary input parameters — `Transform[A, B]` accepts supertypes of declared input when contravariance applies; pipeline input at hop N must remain assignable to hop N+1 contravariant parameter without `cast`.
- Refinement ports that tighten return types chain statically — `Wide.bind(Narrow)` requires `Narrow` return assignable to next hop input; incompatible refinements belong in separate protocol branches with `@override`, not coerced through pipeline erasure.
- Intersection join ports at pipeline ingress accept the smallest sufficient slice — depend on `Readable[T]` alone when the pipeline never writes; widen to `Dual[T]` only at the hop that invokes write members.
- Accumulated generic bounds on pipeline entry use family aliases or canonical owner types — `T: Member`, not bare `object` — interior transforms trust static narrowing and do not re-prove variance mid-pipeline.

# Fault Union Threading And Provenance

- Each port hop owns its fault union — `Result[B, E]` at hop k carries `E` produced by hop k's port method or gate; composition accumulates as `E | F | G` through bind, not as raised exception types crossing boundaries.
- Provenance on contravariant refine ports references pre-transform input — `Refine[T, U]` returning `Hard[T]` preserves recovery context tied to `T`, not to post-transform `U`; fault payloads name the producing hop in structured fields, not string codes.
- Recoverable port faults may accumulate in partial-success maps at the composition seam — terminal port faults short-circuit without mutating canonical identities already materialized in the same transaction.
- Port-local fault unions stay file-internal until boundary projection — seam adapters map port faults into shared domain `Result` carriers inside `capture` / `async_capture`; interior transforms do not re-encode tier or version vocabulary from port rejection evidence.
- `filter_with` on `Ok` implements authorization gates between hops — recoverable faults fold with `or_else_with`; terminal faults propagate through the same bind path without exception absorption.

# Port Fault Severity At Composition Boundaries

- Classify port faults by recoverability at the port owner — `@tagged_union` variants expose routing via `@property` discriminants such as `.recoverable`; composition routes on discriminant properties, not exception types or string codes.
- Severity routing belongs at the composition boundary or AOP structural gate — interior pipeline bodies match on fault discriminants from port returns, not on `except VendorError` branches mid-fold.
- Version-gated rejection — `Unsupported`, `Sunsetted` — surfaces at ingress Kleisli chain before interior domain ports consume resolved capabilities; interior hops must not re-check version bands after root resolution.
- Tier-gated rejection evidence `(expected_cap, actual_type, tier)` projects to boundary diagnostics at the seam — interior transforms re-encoding tier tokens indicate context leak from conformance lattice into rail interior.
- Adding a fault variant to a port hop requires simultaneous updates to bind chain witnesses, severity routing laws, and seam projection rows in one promotion unit — silent fault union drift breaks metamorphic re-bind proofs.

# Async Port Family At Pipeline Shell

- Sync and async capability are distinct port families — structural conformance does not bridge scheduling models; pipelines mixing sync port methods with `await` on blocking IO are definition defects caught at static proof.
- Async pipelines thread through `@effect.async_result` generators and `anyio` structured concurrency at the composition root — unify retry and fan-out at the shell boundary with per-arrow retry policy and accumulated `Fault` unions.
- Propagate cancellation as a sum-type member mapped to `Timeout` faults — bare `asyncio.CancelledError` does not cross port boundaries; collect parallel async results into `Block[T]` or `Block[Fault[E]]` with rail inversion at the boundary.
- Sync interior folds must not call async port methods via `asyncio.run` convenience — split pipelines at the scheduling seam; async shell wraps sync domain only through explicit executor ports admitted for that purpose.
- Async fan-out handoff artifact is `Block[T]` or accumulated `Fault` at the capability lattice seam — domain interiors receive narrowed sync or async ports matching the generator model of the enclosing effect.

# Version-Gated Kleisli Ingress Chain

- Version metadata lives on the port or companion `Versioned` protocol — not as parallel string parameters on every pipeline entrypoint.
- Index adapter implementations in `Map[Ver, Callable[..., Result[T, E]]]` where `Ver` is a closed `Literal` band; gate deprecation with `Map[Method, tuple[Ver, Ver]]` keyed by `StrEnum` method identities.
- Three-stage Kleisli ingress — `try_find(v).to_result(...).bind(adapter).bind(call)` — extracts offered version from positional arguments via `block.try_find(lambda a: isinstance(a, Versioned))` defaulting to first known route when callers omit versioned adapters.
- Unsupported and sunsetted faults owned at the version gate — interior domain ports consume already-resolved capabilities; version re-check mid-pipeline indicates root handoff defect.
- Version band evolution updates `Map[Ver, Callable]` rows, gate fault shapes, and metamorphic version-gate laws in one governance unit — free strings and unbounded ints do not index adapter rows.

# Authorization Frozenset Gates Between Hops

- Capability authorization composes as frozenset subset algebra — `req <= held` on `frozenset[Cap]` with requirement sets frozen at decoration or module scope.
- Missing-capability evidence names the first set-difference element — `next(iter(req - held))` — as typed fault payload; string comparison gates are rejected as interim migration paths.
- Authorization gates stack in canonical outer-to-inner order relative to structural conform gates when both apply — trace, authorize, validate, cache, govern, retry, structural conform, operation — identity and tier evidence precede capability narrowing when load-bearing.
- Adding a capability token updates decoration-time frozensets, gate negative-branch fault shapes, and authorization law rows together — partial promotion leaves pipelines authorized on stale token sets.
- Frozenset gates are not runtime `get_protocol_members` substitutes — subset algebra proves capability vocabulary satisfaction; structural conformance proves port shape satisfaction; evidence grades remain orthogonal.

# Intersection Joins In Pipeline Ingress

- Pipeline parameters name the intersection join when multiple slices are load-bearing in one operation — `Dual[T](Readable[T], Writable[T])` at ingress, not independent `isinstance` checks per slice.
- Join member sets equal the union of parent `get_protocol_members` names — runtime gates naming `Dual[T]` need independent `@runtime_checkable` on the join type when `isinstance` is load-bearing, not merely on slice parents `Readable[T]` or `Writable[T]`.
- Undecorated joins inheriting from decorated parents enter the GH-132604 deprecation path — Python 3.15 emits `DeprecationWarning` at the check site; 3.20 raises `TypeError`; treat join decoration as part of the same promotion unit as parent slice edits.
- Reject joins duplicating members with incompatible signatures — incompatible refinements split into separate protocol branches; pipeline bind chains do not coerce through fat joins re-declaring parent members under new names.
- Caller dependency minimization — accept `Readable[T]` at read-only pipeline stages even when the scope map also holds `Writable[T]` bindings; widen parameter type only at the write hop.

# Tier-Ordered Adapter Selection Before Pipeline Bind

- Composition root selects adapter before pipeline interior executes — tier traversal orders `Block[tuple[Tier, type[Port]]]` descending strength with short-circuit on first successful semantic `TypeIs` conformance.
- Wrap accepted adapters in provenance carriers — `Certified[C](inner, attestation, tier)` — when attestation or audit metadata must travel with the narrowed capability through the first bind hop.
- Tier traversal implements as `block.fold` with `or_else_with` on `Result` — weaker candidates must not run after acceptance; fault payloads carry tier evidence on rejection paths.
- Pipeline interior treats tier-selected adapter as statically satisfied — no re-traversal mid-pipeline unless explicit hot-swap port admits substitution at documented composition boundaries via `scoped`.
- Tier reordering updates root traversal tables, tier-gated fault payloads, and smoke tests asserting short-circuit supremum in the same change unit.

# Structural Gates As Pipeline Prefix

- Structural conformance gates are pipeline prefix morphisms — `filter_with` on `Ok(obj)` preceded by `TypeIs[Port]` before the domain operation body executes; never bare `isinstance` followed by `cast`.
- Precompute requirement frozensets per intersection join at decoration time — runtime work at the gate is narrowing and invocation only.
- Gate negative branches return `(type[Port], type)` tuples or owned fault unions — expected adapter mismatch at domain boundaries does not raise `TypeError`.
- Async structural gates use the async port family at the same stack position as sync gates — scheduling-model mismatch is a definition defect, not a runtime discovery inside sync pipeline bodies.
- Validation decorators admit boundary-normalized values on `Result` rails — structural gates narrow port parameters; ingress validation and port conformance remain distinct prefix stages.

# Rail Interior Trust After Pipeline Prefix

- After composition-root narrowing and structural gate prefix, interior transforms treat port parameters as statically satisfied — no `isinstance`, `hasattr`, or `cast` repair on capabilities mid-pipeline.
- Port method faults enter the same `Result` rail as canonical transforms — faults from port hops accumulate with provenance; interior code does not catch vendor exceptions and re-wrap as domain types.
- Scope maps are read-only during interior folds — transient substitution uses `scoped(caps, thunk)` at composition boundaries, not mutable module registries mid-transform.
- Re-entry of foreign carriers rematerializes canonical shapes first — ports do not accept wire structs or ingress models as substitute domain owners inside pipeline interiors.
- Generic port bounds on interior parameters use canonical owner types — interior trust fails when pipelines widen port-typed parameters to erased carriers between hops.

# Metamorphic Re-Bind And Pipeline Proof

- Full-stack metamorphic law when port-backed egress participates — canonical materialize → interior transform invoking port → wire project → decode → `materialize_*` → fresh scope bind at worker entrypoint → interior re-invoke — shape round-trip and capability re-bind are independent proof obligations.
- Re-bind law asserts adapter identity differs across process seams while canonical identity holds — parent-process port instances never satisfy worker proof; immutable settings cross, adapters reconstruct locally.
- Subset metamorphic declarations document which port operations participate in round-trip — read-only observation ports may omit write members from re-bind chains with explicit rationale in the law module.
- Trusted-replay chains pin boot records for encoder identity, store key, schema version, and port factory module — replay skips re-validation of stored canonical graphs but re-binds port implementations from pinned records.
- Pipeline law modules co-locate with boundary packages — witnesses import port symbols and adapter factories; domain modules under test never import vendor backends for pipeline proof convenience.

# Composition Root Handoff Graph

- Root wiring declares typed handoff graph per bounded context — `settings -> scope bind -> foreign_ingress -> Result[Canonical, E] -> interior(Result) -> Canonical -> foreign_wire` with port resolution nodes only at bind and boundary pipeline hops.
- Adapter factories return `Result[Port, ConfigFault]` — credential binding, endpoint selection, and tier preference resolve at root; consuming modules import port type and factory, not vendor client classes.
- Every replaceable backend maps to exactly one admitted port and one composition-root bind row — orphan adapters without port symbols or ports without bind rows fail arch review.
- Import-cycle resolution for port mirrors and adapters belongs at the composition root — acyclic domain modules import port symbols from boundary packages only.
- Multi-backend contexts document tier expectations beside factory exports without exporting tier traversal machinery to domain interiors.

# Pipeline Anti-Patterns

- Mid-pipeline `isinstance` on port symbols after root narrowing — move conformance to composition-root gate or AOP structural decorator prefix.
- Exception catch-and-rewrap around port method invocations — port methods return `Result`; vendor exceptions indicate seam placement defect.
- Sync pipeline calling `async def` port methods through event-loop hacks — split async port family at root shell.
- Version or tier re-check inside interior hops after root resolution — gate vocabulary leaks into rail interior.
- Kleisli chain erasing fault unions to bare `Exception` or string codes — provenance and severity discriminants are load-bearing.
- Shared mutable singleton backend registered once and reused across processes — settings cross IPC; adapters rebuild per interpreter.
- Fat pipeline ingress typing full vendor SDK surface — mirror extracts minimal invoked method set; pipeline parameters name admitted port joins only.
- Stringly authorization between hops comparing raw tokens — frozenset subset algebra on closed `Cap` bands owns authorization evidence.

# Harness Obligations For Pipeline Laws

- Register pipeline laws via `register_law(Port, "kleisli-bind")`, `"fault-severity-routing"`, `"async-shell-fanout"`, `"version-gate-ingress"`, and `"rebind-metamorphic"` slugs tied to port owner symbols — not pytest file paths.
- Static proof precedes generative pipeline properties — fault union threading and variance preservation fail fast in checker matrix modules before expensive backend generative runs.
- Property tests vary tier candidate order and assert short-circuit supremum — weaker backends must not execute after first `Ok(Certified[C])`.
- Mutation testing targets boundary adapter pipelines and tier traversal when seam logic is dense — interior folds on already-narrowed ports are secondary mutation surfaces.
- Cross-checker parity on representative pipeline modules — ty, mypy, and pyright on the same port chain declarations; one green backend does not relax variance or fault union declarations required by others.

# Delegation And Wrapper Policy In Pipelines

- Transparent forwarding preserves static assignability — nominal wrapper classes break structural subtyping unless every port member re-exports with matching signatures and preserved `ParamSpec` on `__call__` members.
- Prefer direct injection where structure matches — compose cross-cutting concerns with `@effect.result` pipelines at the composition root, not `class Wrapper(Port)` duplicating every method between bind hops.
- Admit boundary-local nominal delegate capsules only when every port member forwards through typed one-liners — erased `getattr(self._inner, name)(*args)` wrappers fail static proof and hide fault provenance at the seam.
- Deep wrapper chains using string dispatch are rejected — each hop must carry `@override`-checked signatures or fold into a single boundary pipeline shell.
- Export opacity applies to pipeline factories — consuming contexts import port plus `resolve_*`; vendor adapter classes stay inside the providing module behind bind rows.

# Open Extension Versus Port Pipeline Ingress

- Closed domain dispatch uses exhaustive `match` on canonical unions with `assert_never` — open extension at foreign boundaries uses `singledispatch` or typed registry rows keyed by verified foreign types, not protocol lattices per plugin brand.
- Structural ports admit third-party implementers with independent class hierarchies — registry rows admit in-repo handlers over canonical shapes; the two mechanisms do not substitute at the same pipeline seam.
- When a foreign plugin exposes a nominal base, mirror its invoked method surface as a port at injection boundary — domain pipeline depends on port parameter; plugin discovery uses tier lattice plus `TypeIs`, not subclass checks against local nominal bases.
- Catalog enums and dispatch tokens supply wire routing metadata — handlers remain `Callable[[Canonical], Result[...]]`; ports supply IO, codec, and transport capability in Kleisli chains, not dispatch table identity.

# Failure Archaeology For Pipeline Defects

- Static assignability rejection at pipeline bind — mirror port, refinement chain, or intersection join signature defect.
- `Missing[Port]` at `ask` — composition-root bind row, scope fork via `scoped`, or `ContextVar` absorb wrapper misplacement at stack edge.
- Fault severity mis-routing — seam projection owner or port fault union discriminant property defect; interior re-encoding indicates context leak.
- Async scheduling defect — sync interior awaiting async port method; caught at static proof when async port family split is correct.
- Version or tier fault appearing mid-interior hop — root handoff graph omits gate stage or duplicates resolution node.
- Metamorphic re-bind failure with same adapter instance identity across process spawn — boundary projection owner defect; diagnostic tags name `rebind`, `scope`, or `ipc`.
- Observability spans on seam adapters emit port key, tier selected, and factory identity at bind time — interior domain spans omit vendor module paths.
