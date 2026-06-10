# Promotion Certificate And Concurrency Calculus — Stack Evolution Receipts Under Parallel Boot

# Promotion Certificate Row Shape

- Every stack version event declares one frozen certificate row: `certificate_id`, `owner_qualname`, `owner_module`, `stack_fingerprint_before`, `stack_fingerprint_after`, `stack_version`, `promotion_kind`, `liability_owner`, `fan_out_contexts`, `proof_layers`, `lattice_row_ids`, `schema_snapshot_ids`, `trust_row_ids`, `negative_fixture_ids`, `rollback_scope`.
- `certificate_id` is a stable snake slug — `add_field_validator_v2`, `pydantic_pin_2_13_4`, `policy_stack_reorder_trace_validate` — not commit hashes alone; hashes annotate rows but do not replace ids.
- `stack_fingerprint_before` and `stack_fingerprint_after` capture outermost `qualname`, synthesis family (`pydantic`, `msgspec`, `dataclass`), `runtime-evaluated-decorators` paths active at class creation, frozen `ConfigDict` key multiset, and `__pydantic_core_schema__` tag multiset after first-touch when pydantic — prose stack lists without fingerprint columns fail admission.
- `promotion_kind` draws from `{decorator_add, decorator_remove, decorator_reorder, dependency_pin, toolchain_register, dataclass_transform_change, trust_repin, federation_fanout}` — marker-only additions without executable semantics map to `toolchain_register` or exempt rows, not `decorator_add`.
- `liability_owner` names the module that must land first in the promotion unit — vocabulary owner, seam adapter, composition root, or boundary migration fold — downstream contexts are consumers, not co-owners per federation propagation law.
- `fan_out_contexts` lists every bounded context importing the admitted owner or mirror ingress projection — partial fan-out without exemption document is merge blocker.
- `proof_layers` is an ordered tuple subset of `{static_checker, ruff_registration, compile_oracle, unwrap_fingerprint, call_time_beartype, seam_lattice, metamorphic_codec, mutation_kill, arch_altitude}` — harness execution order matches layer order; skipping cheaper layers invalidates certificate.
- `lattice_row_ids` indexes every seam decorator transition proof lattice transition row whose `upstream_owner` or `downstream_owner` references the promoted stack — certificate without synchronized lattice bump is promotion defect.
- `schema_snapshot_ids` pairs validation-mode and serialization-mode JSON Schema snapshot ids when `proof_layers` includes `metamorphic_codec` or owners carry `@computed_field`, serializers, or `Field(json_schema_extra=...)`.
- `trust_row_ids` lists composition-root trust rows invalidated when validator graph, encoder identity, or schema version changes — graph evolution without trust re-pin is security defect indexed in certificate metadata.
- `negative_fixture_ids` cross-reference lattice negatives plus certificate-specific anti-patterns — `orphan_runtime_evaluated_decorator`, `fingerprint_without_lattice_sync`, `boot_mutable_adapter_constant`, `boot_policy_skew`, `stale_child_certificate_registry`, `context_leak`, `lock_inversion`.
- `rollback_scope` names modules reverting together on proof failure — vocabulary promotion rolls back all `fan_out_contexts`; seam-only promotion rolls back adapter and remap table without domain interior when fault is cardinality not token.

# Stack Version Morphism Law

- Stack version bumps when promotion alters synthesis, validation phase, projection schema, callable codomain, policy stack order, or compile-graph tag multiset — not when marker-only decorators add static evidence without executable semantics.
- `decorator_remove` on validators or serializers is breaking when wire acceptance width, cross-field law, or egress shape changes — certificate `promotion_kind=decorator_remove` requires ingress mirror update, canonical deduplication checklist flip, and metamorphic codec replay in one unit.
- `decorator_reorder` on policy stacks changes observability and failure archaeology even when static types are unchanged — certificate carries `policy_stack_reorder_*` id and mandates seam failure-injection replay per annotate-wrap policy-stack ordering law.
- `dependency_pin` on `pydantic`, `beartype`, or `msgspec` may recompile inner nodes without source edits — certificate treats pin as stack evolution: dual-mode schema diff, core-schema oracle walk, and call-time beartype samples before merge.
- `toolchain_register` propagates simultaneously: Ruff `runtime-evaluated-decorators`, optional `property-decorators` or `classmethod-decorators`, `extend-immutable-calls`, and at least one reference owner under `ty` strict mode — orphan registration without exercised owner fails certificate static layer.
- Fingerprint morphism is total: every admitted decorator on the owner maps to a compile tag, unwrap depth increment, or static registration path — certificate promotion cannot leave orphan evidence on sibling owners at duplicate altitudes.

# Boot Immutability And Compile Domain Freeze

- Warm-graph freeze is the terminal boot phase after composition root completes liability import, settings construction, pinned constant registration, and parametrized first-touch on every owner on the worker path — freeze publishes read-dominant compile fingerprints, certificate registry rows, and lattice row version maps; post-freeze mutation is equilibrium violation I7 from decorator stack equilibrium reconciliation calculus, not recoverable domain retry.
- Boot row shape: `boot_id`, `worker_kind`, `freeze_epoch`, `import_final_owners`, `first_touch_final_owners`, `pinned_constant_symbols`, `certificate_registry_hash`, `admission_certificate_ids`, `warm_graph_complete`.
- `worker_kind` draws from `{main_interpreter, subinterpreter, multiprocessing_child, thread_pool_worker}` — each kind owns one compile domain; sharing object references across kinds without documented handoff row is boot defect.
- `freeze_epoch` is monotonic per worker — promotion landing after freeze requires `compile refresh` reconciliation row from decorator stack equilibrium reconciliation calculus (process restart, warm-graph rebind, or subinterpreter respawn), not in-place module surgery.
- `import_final_owners` lists pydantic and dataclass owners whose `__pydantic_core_schema__` or synthesized field registry is complete at class-body without deferred first-touch — msgspec struct owners always appear here; generic pydantic owners appear here only when production never references concrete specializations.
- `first_touch_final_owners` lists parametrized pydantic owners requiring `model_rebuild()` or root-pinned `TypeAdapter` first reference before freeze — certificate documents every concrete `get_args` tuple on the worker path; monomorphic import fixtures alone do not discharge boot row.
- `pinned_constant_symbols` enumerates module-level `TypeAdapter`, `Decoder`, `Encoder`, and `BeartypeConf` anchors from `[CONSTANTS]` — post-boot rebinding without closed certificate is indexed negative `boot_mutable_adapter_constant`.
- `certificate_registry_hash` pins closed promotion certificate set imported before freeze — child workers and subinterpreters assert hash parity against parent transmit bundle before publishing handler entrypoints.
- `admission_certificate_ids` index annotationlib VALUE parity receipts from pydantic altitude doctrine — for every pydantic owner on the worker path, `annotationlib.get_annotations(cls, format=VALUE, include_extras=True)` field keys match `cls.model_fields` after required rebuild; certificate failure blocks worker and subinterpreter publish.
- `warm_graph_complete` boolean gates seam handler registration — `false` after partial import leaves open certificates on consumers referencing unloaded fingerprints; interior transforms must not run until `true`.
- `BaseSettings` construction completes before worker threads or parallel importers read the frozen instance — lazy settings accessors on hot paths race under free-threaded root graphs and produce undefined decorator compile domains on settings-derived boot records.
- `importlib.reload` on modules owning admitted decorator stacks without full root re-import and certificate registry replay is boot immutability violation — hot reload duplicates validator identity, splits `stack_fingerprint_after` across importers, and desynchronizes lattice row versions from certificate metadata.
- Class-body compile graphs on pydantic owners materialize at import or first-touch per owner policy — certificate boot row documents which owners are `import_final` versus `first_touch_final`; mixing policies on generic parametrizations in one family is indexed negative `boot_policy_skew`.
- msgspec struct layout is import-final — layout flag or field-order promotion requires process restart and certificate `promotion_kind=decorator_add` on struct owners; no pydantic-style hot rebuild path exists.
- Free-threaded builds (PEP 779) treat `__pydantic_core_schema__` tag multisets, hypothesis strategy registration tables, and certificate `stack_fingerprint_after` as read-only after warm-graph freeze — `functools.cache` on decorator factories preserves internal coherence but does not discharge single-fill proof under parallel first-touch without lock-order row.
- Forward-reference families call `model_rebuild()` during boot before first `model_validate` on worker paths — worker-first validation without rebuild is root wiring defect, not interior domain concern.
- Post-freeze class surgery — `model_config.update()`, `model_fields` mutation, decorator re-application on published class objects — races under parallel importers; altitude or stack changes require redeploy with promotion certificate, not runtime patch.

# Lock-Order Lattice On Decorator Caches

- Decorator admission maintains bounded mutable caches — generic `TypeAdapter` compile memo, `model_rebuild()` gates, hypothesis strategy registration tables, lattice row version maps, certificate registry maps — each cache declares one lock-order row when concurrency is load-bearing under free-threaded boot or parallel first-touch before freeze.
- Lock-order row shape: `cache_id`, `guard_kind`, `acquire_order_rank`, `epoch_rank`, `key_tuple_fields`, `memo_publication`, `invalidation_certificate_ids`, `read_dominant`, `lock_infeasibility_reason`.
- `guard_kind` draws from `{import_lock, threading_lock, contextvar_epoch, lock_free_immutable}` — import-time-only caches default `import_lock`; hot-path generic adapter memo under free-threaded boot defaults `threading_lock` with documented `acquire_order_rank`; `lock_free_immutable` applies when cache publishes `frozendict`/`frozenset` snapshot after single cold fill.
- `memo_publication` draws from `{import_frozendict, contextvar_epoch, threading_lock_fill}` — policy (1) import-published tables imply `acquire_order_rank=forbidden` and `guard_kind=lock_free_immutable`; policy (3) threading fill requires `epoch_rank` and documented `lock_infeasibility_reason` when (1) or (2) were infeasible.
- `epoch_rank` table fixes cross-cache order among decorator memo sites: import-phase catalog registration (0) precedes class-body compile memo (1) precedes first-touch generic adapter memo (2) precedes hypothesis strategy registration (3) precedes lattice or certificate registry publish (4) — acquiring rank-3 lock while waiting on rank-1 fill filled by another task holding rank-3 is inversion defect.
- Global acquisition rule: sort lock symbols `L(cache_id, owner_qualname)` by `(epoch_rank, owner_qualname, cache_id)` lexicographically ascending; release in reverse; never acquire `L_b` while holding `L_a` when `L_a > L_b` in sort order — ad hoc per-developer ordering fails deadlock harness.
- `acquire_order_rank` is the composite sort key ordinal among all decorator caches in one composition root — compile memo acquires before strategy registration table before lattice version map; violating order invites D10 deadlock signal from decorator stack equilibrium reconciliation calculus under parallel first-touch on distinct generic owners sharing root constants.
- `key_tuple_fields` always include concrete type arguments for generic owners — `(cls, tuple(get_args(cls)))` minimum; erased class names alone are invalid cache keys and invalid lock-scope keys.
- Nested lock on same `cache_id` and owner symbol is forbidden — multiple memo sites on one owner collapse to single lock or promote to import-published table split by memo key immutability.
- `threading.RLock` is rejected on decorator caches — memo fill must be idempotent cold-path once under lock; re-entrant memo recursion signals compose defect or promotion to import-published structure.
- Lock hold time budget: fill under lock computes immutable snapshot, assigns to read-dominant table or class attribute, releases — no IO, no port enumeration, no `model_validate` replay inside critical section; holding lock across `await` on async seam stacks is critical-section violation indexed with `async_lock_held_during_hop` when policy stacks interleave.
- `contextvar_epoch` nodes on async seam stacks rank before `threading_lock` nodes on shared memo — async promotion certificates verify `Token.reset()` in call-time receipts before lock-order closure; bare `ctx.set(old)` under concurrency is indexed negative `context_leak` from seam decorator transition proof lattice.
- `invalidation_certificate_ids` lists certificates that must bump cache generation or force process restart — dependency pin certificates invalidate adapter memo without automatic in-process refresh unless root documents hot-swap exemption row.
- `read_dominant` caches serve fingerprint proofs without mutation after boot freeze — lattice row maps and certificate registries are read-dominant; write paths occur only during promotion landing before worker boot, not from call-time policy wrappers.
- Lock-order violation at runtime publishes `fault_kind=lock_inversion` certificate at composition root — seam handlers and interior folds do not catch; enrichment and first-touch chains abort per compose-chain failure short-circuit.
- Call-time policy wrappers must not acquire compile memo locks — interior and seam handlers read frozen fingerprints only; registry mutation at call time violates both phase law from decorator admission doctrine and lock-order rows.
- Free-threaded CI interleave fixture: thread A runs parametrized first-touch on `Owner[int]`, thread B runs first-touch on `Owner[str]` sharing root `TypeAdapter` constant — positive pass requires identical lattice acquire order; inverted acquisition must deadlock or fail assert in harness, not produce divergent `stack_fingerprint_after`.
- Compose-chain DAG from decorator stack equilibrium reconciliation calculus extends single-owner lock-order rows when multiple decorator caches participate in one seam handoff — acyclic global `edge_acquire_before` among `cache_id` nodes prevents D10 under parallel boot; cyclic DAG indicates promotion planning defect.

# Subinterpreter And Multiprocessing Boot Discipline

- PEP 734 subinterpreters (`concurrent.interpreters`) and `concurrent.futures.InterpreterPoolExecutor` workers treat each interpreter as an independent composition root — parent pinned `TypeAdapter` singletons, `BaseSettings` instances, compiled `SchemaValidator` handles, and promotion certificate registries are not portable object references across interpreter boundaries.
- Subinterpreter handoff row shape: `handoff_id`, `parent_boot_id`, `child_boot_id`, `worker_kind`, `transmit_medium`, `snapshot_bytes_hash`, `encoder_identity`, `schema_version`, `certificate_id`, `stack_version`, `stack_fingerprint_after`, `forbidden_fields_absent`.
- `worker_kind` literal records `subinterpreter` or `multiprocessing_child` — algorithm named in row (`msgspec_encode`, `json_canonical`, `pydantic_json`) pins decode oracle on receiving root; hash covers bytes, not Python object id.
- `forbidden_fields_absent` attestation rejects pickle of port-typed fields, `SchemaValidator` handles, open sockets, and `ContextVar` tokens — pickle of compiled validator graphs across seams is security and boot defect unless trusted-replay row documents encoder identity and version pins.
- Child interpreter boot choreography: import promoted certificate registry and assert `certificate_registry_hash` parity → rerun root warm-up subset for worker path owners → replay `admission_certificate_ids` VALUE parity receipts → execute parametrized first-touch per `first_touch_final_owners` → acquire lock-order rows during parallel first-touch → publish `warm_graph_complete=true` on child `boot_id` — skipping certificate registry import before freeze is indexed negative `stale_child_certificate_registry`.
- Cross-interpreter handoff at boundary altitude transmits canonical bytes plus `certificate_id`, `stack_version`, `schema_version`, and `encoder_identity` — never shared module rebinding of adapter constants across interpreter boundaries; receiving root validates decoder identity against certificate `trust_row_ids` before trusted replay shortcuts.
- Parent alias law on live object after transmit does not invalidate child handoff when `snapshot_bytes_hash` matches — child certificate proves decode under child compile domain, not object identity with parent.
- Multiprocessing workers spawned after parent promotion must import promoted certificate registry before warm-graph freeze — child boot inherits parent fingerprint immutability law; stale child images without certificate sync are boot defects, not domain retry concerns.
- Subinterpreter isolation multiplies compile domains per interpreter — each interpreter owns independent `__pydantic_core_schema__` tag multisets, beartype wrapper materialization, and hypothesis strategy tables; federation handoff demotes to bytes plus pinned decoder at receiving root per seam lattice serialization-to-re-ingress and catalog-row-to-cross-process-handoff transition rows.
- Shared-memory transmit of settings or boot config snapshots records `transmit_medium` and `byte_length` — settings decorator promotions remain composition-root boot certificates disjoint from canonical owner certificates unless explicit mirror policy row exists.
- Interpreter pool sizing uses `os.process_cpu_count()` per language doctrine — CPU-bound decorator compile work routes through interpreter isolation, not thread-only pools pretending to share frozen adapter constants without lock-order DAG.
- Subinterpreter respawn after promotion landing replaces in-place hot-swap when `dependency_pin` or struct layout certificates invalidate in-process refresh — certificate `rollback_scope` names whether child interpreters alone respawn or entire pool restarts.

# Federation Fan-Out On Decorator Evidence

- Decorator stack promotion on vocabulary-backed ingress or canonical owners fans along federation edges from cross-context integration federation doctrine — fan-out width equals count of contexts importing the owner or ingress mirror, not import-graph depth alone.
- **Vocabulary propagation** — enum or discriminant decorator change on ingress union fans to every context importing consensus vocabulary rows; certificate `fan_out_contexts` must list all consumers before merge.
- **Seam propagation** — validator or serializer decorator change on anti-corruption ingress fans only to foreign pair owning the adapter; canonical owner promotion fans to interior contexts only when materialization contract changes.
- **Warm-graph propagation** — ingress compile graph or wire serializer change fans to composition-root warm registration and every context caching `TypeAdapter` for that target — cold recompile after promotion without certificate warm layer is defect.
- **Receipt propagation** — new decorator-induced fault kind on fact stream fans to fold reducers importing the kind vocabulary — certificate includes receipt edge ids when promotion alters `ValidationError.loc` prefixes or beartype violation routing.
- Dual liability on one invariant — duplicate validator on ingress and canonical — collapses to single owner per equilibrium row; certificate documents which altitude owns the rule and which surface thins to projection mirror in same promotion unit.
- Propagation graph must remain acyclic — bidirectional certificate obligations between contexts indicate missing vocabulary federation or duplicate seam ownership.

# Harness Receipt Calculus

- Promotion certificates emit harness receipts — machine-readable proof summaries stored beside owner modules for audit replay beyond field equality metamorphics.
- Receipt row shape: `receipt_id`, `certificate_id`, `proof_layer`, `evidence_kind`, `evidence_digest`, `witness_count`, `parametrization_axes`, `attribution_module`.
- `evidence_kind` draws from `{tag_multiset_diff, schema_snapshot_diff, unwrap_depth_parity, signature_frozenset_parity, mutation_kill_ratio, lattice_negative_pass, arch_import_graph_pass}` — each `proof_layers` entry produces at least one receipt row.
- `evidence_digest` stores stable hash of normalized evidence — core-schema tag multiset, JSON Schema canonical form, or `inspect.signature` string — not raw pytest stdout captures.
- `witness_count` records parametrized sample cardinality — generic owners require witnesses per concrete `get_args` tuple referenced in production; monomorphic import-only witnesses under-approximate receipt totality.
- `parametrization_axes` names axes exercised — `literal_arm`, `schema_version`, `trust_posture`, `policy_stack_order`, `pep696_default_specialization` — receipts without declared axes fail exhaustiveness gate when owner is polymorphic.
- Harness execution order on receipts: static checker and Ruff registration receipts before compile oracle receipts before unwrap fingerprint receipts before call-time beartype receipts before seam lattice receipts before metamorphic codec receipts — order matches `proof_layers` on the certificate.
- Receipt debt — suppressed `ValidationError.loc`, erased unwrap depth, or missing negative witness — blocks certificate closure; fix admitting decorator or adapter, do not waive integration owners.

# Concurrent First-Touch And Parametric Races

- Parallel first-touch on `Owner[int]` and `Owner[str]` from distinct threads before boot freeze completes requires lock-order row on generic compile memo — without lock, tag multiset races produce undefined certificate fingerprints.
- PEP 696 default type parameters on generic ingress owners emit distinct first-touch certificates for default versus explicit specializations — persistence keys record which specialization tuple was admitted at the seam; default and explicit rows do not share one receipt.
- `mode="wrap"` sandwich nodes obscure validator bodies for Hypothesis — certificates on ingress arms with wrap admission declare `hypothesis_compiled_strategy` proof flag in `proof_layers` or explicit conformance exemption row with liability owner.
- Async policy stacks on seam handlers preserve coroutine identity through promotion — certificate includes `async_coroutine_preservation` lattice row id and unwrap receipt verifying `inspect.iscoroutinefunction` after promotion landing.
- Cooperative `chain_schema` multi-hop builders on embedded slots require enclosing owner certificate to reference cooperative compile receipt — multi-hop admission without enclosing `GenerateSchema` oracle walk is cross-referenced defect, not standalone certificate closure.

# Promotion Unit Sequencing Law

- Promotion units are atomic changesets indexed by primary `certificate_id` — multiple owner certificates in one unit share liability landing order declared in root promotion plan, not git commit message prose.
- Sequencing within a unit: liability owner modules land first, `fan_out_contexts` consumers second, harness receipt regeneration third, worker boot or warm-graph rebind last — inverting order leaves open certificates on consumers referencing unloaded fingerprints.
- `dependency_pin` certificates precede `decorator_add` certificates on owners compiled against pinned surfaces — adding validators before pin receipt closure risks stale tag multiset baselines.
- `federation_fanout` certificates precede seam behavioral suite expansion when vocabulary rows change — seam corpus append without vocabulary certificate closure misattributes foreign token faults.
- `trust_repin` certificates precede semi-trusted replay suite enablement — widening ingress without trust metadata invalidates replay receipts until re-pin lands.
- Splitting one logical promotion into sequenced certificates requires explicit `depends_on_certificate_ids` edges — parallel merge of dependent certificates without edge declaration is planning defect.

# Backpressure And Promotion Cadence

- Cross-context promotion backpressure is snapshot cadence policy at composition root — receipt consumers that cannot replay metamorphic chains fast enough receive frozen certificate bundles, not live partial promotion state.
- Quantitative thresholds live in root policy records, not per-owner certificates — default cadence caps concurrent `first_touch` promotions per worker, maximum `fan_out_contexts` width per commit, and maximum lattice row bump count per certificate; exceeding thresholds splits promotion into sequenced certificates with explicit dependency edges.
- Certificate dependency edges form a DAG — `pydantic_pin_*` certificates may block `decorator_add_*` certificates on dependent owners until dual-mode schema receipts land; cyclic certificate dependencies indicate promotion planning defect.
- Semi-trusted replay certificates carry explicit field-level skip columns — promotion widening ingress without trust re-pin invalidates `trust_row_ids` and forces `trust_repin` promotion kind before merge.

# Evolution Anti-Patterns On Certificates

- Landing decorator source without `stack_fingerprint_after` and compile oracle receipt — silent graph drift.
- Bumping `pydantic` pin with certificate `promotion_kind=dependency_pin` but single-mode schema receipt only — under-approximates egress after `@computed_field` admission.
- Updating lattice rows without synchronized certificate `stack_version` — harness attributes drift to stale transition proof.
- Per-request `TypeAdapter` construction after boot freeze — bypasses warm-graph certificate and invites compile domain skew under concurrency.
- Promotion unit updating ingress mirror without `fan_out_contexts` completeness — partial federation landing.
- Using call-time validation results to justify import-phase certificate closure — phase boundary violation from decorator admission doctrine.
- Certificate keyed by pytest node id or CI run number instead of `certificate_id` and owner `qualname`.
- Publishing seam handlers before `warm_graph_complete` on boot row — open certificates on consumers referencing unloaded fingerprints.
- Pickling `SchemaValidator`, `TypeAdapter` internals, or settings singletons across subinterpreter seams — bypasses certificate handoff row and trust re-pin law.
- Parallel first-touch without lock-order row under free-threaded boot — benign duplicate compile fiction; produces undefined `stack_fingerprint_after` and D10 equilibrium signal.
- `importlib.reload` on decorator-owning modules without certificate registry replay — splits compile domains within one interpreter.
- Holding compile memo lock across `await` on async policy stacks — critical-section violation; lock-order and epoch modules fail together.
- Child subinterpreter boot skipping `certificate_registry_hash` parity assert — stale promotion metadata on fresh compile domain.

# Certificate-Lattice Synchronization Morphism

- Certificates and lattice rows are coupled morphisms — every `lattice_row_ids` entry must reference owners whose `stack_fingerprint_after` matches the certificate; fingerprint drift without lattice version bump is synchronization defect.
- `boundary_to_ingress_projection` lattice rows require certificate `proof_layers` including `seam_lattice` and `compile_oracle=first_touch` on ingress owner — certificate closure without ingress first-touch receipt invalidates the edge.
- `ingress_projection_to_materialize` rows split failure attribution across certificate layers — ingress `ValidationError` receipts attach to upstream compile oracle; downstream `Result` error receipts attach to canonical constructor witnesses, never conflated in one digest.
- `canonical_to_wire_projection` rows mandate `metamorphic_codec` proof layer when egress serializers are load-bearing — certificate without encode-decode receipt on polymorphic arms blocks wire handoff promotion.
- `stack_fingerprint_promotion` lattice row and certificate `stack_version` bump share one `certificate_id` suffix — partial update of fingerprint row without certificate metadata is merge blocker indexed `fingerprint_without_lattice_sync`.
- Promotion units that add lattice rows without certificate register exhaustiveness debt — arch gate counts production adapters against lattice coverage and certificate coverage independently; either gap fails before behavioral suites.

# Certificate Closure Gates

- Certificate closure requires all `proof_layers` receipts present, all `negative_fixture_ids` passing, and `fan_out_contexts` landing confirmed — partial receipt sets remain open certificates, not waivable harness debt.
- `static_checker` gate closes only when `ty` strict mode reports zero decorator-boundary errors on liable owner and every `runtime-evaluated-decorators` path in fingerprint is exercised on a reference owner.
- `compile_oracle` gate closes when tag multiset diff is empty against promoted baseline or documented intentional delta with reviewer sign-off row — silent tag churn on `dependency_pin` certificates fails closure.
- `unwrap_fingerprint` gate closes when production adapter `inspect.unwrap` depth and law-matrix collected item agree post-promotion — mismatch implicates hypothesis `wraps` ordering per annotate-wrap admission law, not certificate exemption.
- `mutation_kill` gate closes when adapter modules wrapping `@validate_call` meet kill ratio threshold declared in root policy — mutants erasing `__wrapped__` or widening to `Any` must die on fingerprint or static layers, not only happy paths.
- `arch_altitude` gate closes when domain import graphs exclude ingress-only decorator owners and interior `model_validate` on dump output — lattice altitude proof complements certificate arch receipt.
- Open certificates block downstream certificates listing them as dependencies — DAG promotion planning treats open certificate as hard merge blocker across all dependent owners.

# Schema Migration Certificate Hops

- Persisted owners carrying `schema_version: Literal[...]` admit validator-graph evolution through boundary migration certificate hops — distinct from ordinary `decorator_add` certificates when read path replays historical compile graphs.
- Migration hop row shape: `hop_id`, `from_version`, `to_version`, `historical_adapter_id`, `fold_owner_module`, `witness_payload_ids`, `trust_repin_required`.
- `historical_adapter_id` pins `TypeAdapter` compiled at stored version — promotion landing current owner graph does not retroactively rewrite stored bytes without hop certificate and version bump on write path.
- `fold_owner_module` owns total migration fold from historical validated instance to current canonical — domain interiors after hop see only current decorator stack; obsolete validator nodes remain in migration modules with exhaustive witnesses.
- `mode="wrap"` to `mode="before"` refactors on persisted arms are hop events when wire acceptance width changes — hop certificate regenerates core-schema oracle samples and conformance rows per core-schema phase morphism law.
- Trusted replay certificates intersect migration hops — `.raw_function` or `model_construct` bypass invalidates when hop changes compile fingerprint unless `trust_repin_required=false` with documented field-level skip columns.
- Dual-family promotion from msgspec manifest to pydantic canonical requires two certificate families in one promotion unit — single certificate omitting wire projection hop leaves egress compile oracle uncovered.

# Settings Boot Certificate Isolation

- `BaseSettings` decorator promotions compile at composition-root boot altitude only — settings certificates are disjoint from canonical owner certificates unless explicit mirror policy row declares intentional duplication.
- `env_to_settings_compile` certificate carries `proof_layers` excluding interior domain behavioral suites — settings `ValidationError` receipts attribute at process start, not per interior transform.
- `settings_fanout_to_boot_record` certificate lists projected boot config owners in `fan_out_contexts` — domain modules consuming boot records do not re-encode settings `field_validator` law without mirror certificate.
- `settings_customise_sources_order` certificate documents `@override` on source ordering as static evidence — reorder is deployment contract requiring settings certificate, not runtime patch.
- Settings generic parametrization follows pydantic rebuild law — settings certificates include parametrized first-touch receipts for every concrete boot argument tuple, not monomorphic env fixtures alone.
- cyclopts staging surfaces remain boundary certificates with distinct `certificate_id` prefix from settings rows until single decorator-admitted owner collapse completes.

# Worker Snapshot And Cross-Process Handoff

- Worker snapshot transmit carries canonical bytes plus `certificate_id`, `stack_version`, `stack_fingerprint_after`, and subinterpreter `handoff_id` when `worker_kind` is not `main_interpreter` — receivers validate decoder identity against certificate `trust_row_ids` before trusting replay shortcuts.
- Cross-process handoff rejects catalog row payloads or pytest node ids as durable evidence — handoff uses admitted owner bytes, schema version literals, pinned encoder identity, and closed certificate registry hash from boot row metadata only.
- Snapshot acceptance gates compare `stack_fingerprint_after` on receiver against transmitted certificate — fingerprint mismatch after promotion landing on sender invalidates snapshot without re-encode, not silent interior repair.
- Federation degradation to trusted-replay-only posture activates when live foreign ingress unavailable — degradation certificate at root documents posture change and narrows `proof_layers` on affected seam certificates to metamorphic replay subset.
- Subprocess and subinterpreter workers spawned after parent promotion must import promoted certificate registry and replay `admission_certificate_ids` before warm-graph freeze — child boot inherits parent fingerprint immutability law; stale child images without certificate sync are boot defects indexed `stale_child_certificate_registry`.
- Parent `chain_id` or `boot_id` referenced in subinterpreter handoff — child worker chains do not mutate parent compile domain; parent k-step promotion after transmit appends new parent certificate unrelated to in-flight child until child reports `warm_graph_complete`.

# Phase-Attributed Certificate Layers

- Certificate proof layers align with decoration phases from decorator admission doctrine — import-phase defects close on `static_checker` and registry receipts; class-body defects close on `compile_oracle` class-body samples; first-touch defects close on parametrized rebuild receipts; call-time defects close on beartype and policy-stack injection receipts.
- Import-phase certificate failures attribute to catalog decorators and duplicate-registration guards — fault payloads name module global and decorator `qualname`; domain constructors are not suspect until class-body receipts pass.
- Class-body certificate failures attribute to synthesis and competing `dataclass_transform` — `TypeError` at definition site names class and conflicting decorator; deferred `ValidationError` on first consumer call indicates misclassified import-phase certificate.
- First-touch certificate failures attribute to stale generic rebuild or forward-reference resolution — fault injection names parametrized owner and type arguments; call-time beartype receipts are not suspect until first-touch gate closes.
- Call-time certificate failures attribute to outermost enforcing decorator on invoked path — receipts map `ValidationError.loc` to pydantic layers and `BeartypeCallHintViolation` to beartype layers; policy wrappers swallowing typed violations fail call-time receipt closure.
- Conflated phase attribution in one receipt digest fails certificate module — each phase defect owns a distinct `receipt_id` and `proof_layer` column.

# Certificate Exhaustiveness And Fold Totality

- Fold totality on decorator effect surfaces requires every synthesis family in a promotion unit to carry oracle walk receipts — adding msgspec struct owners without `type_info` receipts alongside pydantic owners in the same family is exhaustiveness defect.
- Law-matrix `@spec` rows bind to certificates by owner `qualname` and `certificate_id` — witnesses prove stack fingerprints and phase-attributed failures, not pytest file paths alone.
- Composite promotion certificates on polymorphic ingress unions require `witness_count` covering every `Literal` arm — partial arm coverage under-approximates discriminator decorator admission.
- Shrinking preserves decorator-admitted invariants in generative receipts — invalid post-shrink values must fail at admitting validator with attributable `loc` in receipt witness, not at interior domain logic unrelated to promoted decorator.
- Metamorphic receipt chains on decorated owners: lawful instance → egress serializers → encode → decode → ingress validators → `materialize_*` → canonical equality — certificate `metamorphic_codec` layer closes only when full chain receipts land per polymorphic arm.
- Annotationlib micro-bump parity certificates compare `get_annotations` frozensets against `model_fields` after deferred-evaluation policy changes — parity failure alone attributes to decorator propagation certificate, not domain logic certificate.

# Concurrency Metamorphic Modules

- Boot immutability module replays lawful promotion landing then asserts post-freeze rebinding of pinned constants fails with `boot_mutable_adapter_constant` negative — positive path asserts `certificate_registry_hash` stable across read-only fingerprint probes.
- Lock-order deadlock-or-order module interleaves parallel first-touch on shared generic memo — inverted acquisition must deadlock or fail assert; lawful acquisition produces identical `stack_fingerprint_after` on both threads.
- Subinterpreter handoff module spawns child interpreter or `InterpreterPoolExecutor` worker — transmits bytes bundle with `certificate_id` and asserts child `warm_graph_complete` after independent warm-up; pickle shortcut path is negative-only fixture.
- Admission certificate boot module replays VALUE parity on every pydantic owner in `first_touch_final_owners` and `import_final_owners` after rebuild — mismatch blocks publish gate before seam handler smoke.
- Certificate registry sync module compares parent and child `certificate_registry_hash` after promotion merge — orphan child without parent transmit bundle fails before behavioral suites.
- Free-threaded boot module runs under PEP 779 build when available — exercises `functools.cache`-backed decorator factories with lock-order row mandatory; absence of row produces D10 signal, not skipped test.
- Compose-chain lock DAG module validates acyclic `edge_acquire_before` when ingress and canonical owners share root adapter constant — cyclic DAG fails static promotion planning gate.

# CI Regression Gates On Certificates

- Every production owner carrying three or more shape-admitting decorators declares at least one certificate row or documented exemption in the liable module — owner without certificate fails arch gate before behavioral suites.
- Parametrized contract tests index certificates by `certificate_id` and `negative_fixture_ids` — missing negative coverage for declared anti-pattern fails decorator coverage gate.
- ast-grep flags open certificates referenced as dependencies by merged promotion branches — landing source without closed receipt set blocks merge even when unit tests pass on happy paths.
- Ruff upgrades touching `flake8-type-checking` lists require certificate `toolchain_register` row or exemption — grep proof that every new listed decorator appears in at least one closed certificate fingerprint.
- Dual-mode JSON Schema snapshot CI diffs bind to `schema_snapshot_ids` on certificates — ref churn without constraint change passes only when receipt documents refactor-only delta.
- Stryker prioritizes adapter modules where certificates declare `mutation_kill` layer — kill ratio shortfall on seam handlers fails certificate closure before domain interior mutation campaigns.
- Boot row presence gate requires `warm_graph_complete` attestation on composition-root worker paths before seam handler registration in CI smoke — handlers registered against incomplete warm graph fail arch gate.
- Lock-order registry gate requires `acquire_order_rank` and `epoch_rank` on every decorator cache referenced by free-threaded or parallel first-touch fixtures — undefined cross-cache order fails before interior behavioral suites.
- Subinterpreter handoff gate requires closed `handoff_id` receipt on cross-interpreter fixtures — missing `forbidden_fields_absent` attestation or absent `certificate_registry_hash` parity blocks merge on affected promotion branches.