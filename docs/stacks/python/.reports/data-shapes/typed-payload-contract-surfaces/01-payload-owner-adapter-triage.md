# Payload, Owner, And Adapter Triage — Decision System

# Triage Mandate

- Every concept at every pipeline stage resolves to exactly one primary artifact class: typed contract payload, materialized owner, or boundary adapter expression — not two, not a hybrid dict-with-comments.
- Triage runs before naming types; the stage role map at the composition root supplies inputs; the decision output binds one owner module per concept per stage.
- When two artifacts appear equally plausible, promotion gate side breaks the tie: evidence before the gate is payload law; durable invariants after the gate are owner law; translation between foreign and canonical layouts is adapter law.
- Graduation is explicit: payloads do not accrete behavior; owners do not absorb wire aliases; adapters do not become durable domain parameters.

# Promotion Gate Law

- Promotion is the sole lawful payload→owner transition — one root admission expression per ingress route; no interior validate-then-owner shortcut, no durable dict assignability threading past the gate, no assurance replay substituting for gate construction.
- Triage assigns artifact class by gate side; the gate executes transition — triage rows name `{artifact_class, promotion_expression}` but do not perform promotion; adapters may terminate on validated payload only when promotion is intentionally deferred at root.
- Payload imports retire from interior modules in the same promotion unit that binds the gate expression — staging completion, event admission, and ingress validation into invariant-bearing space all exit through the gate, not through parameter typing shrink.
- Egress demotion is projection from canonical owner — wire struct, closed egress payload assignability, or bytes — never mutation of the domain instance into a dict seam and never lawful interior demotion of owner to durable payload parameters.
- Assurance loops certify post-promotion steady state on their own cadence — boot, pre-merge, scheduled replay, post-promotion certify, health tick — and may run when zero promotion units are in flight; assurance never widens contracts or re-admits payload-shaped interior parameters to absorb gate debt.

# Ordered Triage Sequence

- Step 1 — locate stage role: boundary ingress, staging partial, patch delta, event evidence, egress assignability, or keyword-callable root — stage fixes which artifact classes are legal candidates.
- Step 2 — test durability: values that must not survive promotion → payload or adapter carrier; values that enter domain folds → owner after one promotion expression at the gate.
- Step 3 — test foreignness: any wire or provider field rename, discriminant mismatch, or cardinality gap → adapter owns correspondence before owner construction; skip adapter only on trusted canonical replay.
- Step 4 — test runtime class need: hashable static evidence row with no untrusted ingress → NamedTuple; hashable boundary row requiring decode validation → msgspec `Struct(frozen=True, forbid_unknown_fields=True)`; hot bytes egress or trusted struct decode → msgspec Struct; alias or coercion-heavy ingress → Pydantic model; static dict law only → TypedDict payload.
- Step 5 — test invariant density: cross-field proof, smart constructors, replacement targets → owner; key compatibility and requiredness only → payload.
- Step 6 — bind composition root triage row and reject interior imports that contradict the outcome.

# Primary Decision Axes

- **Durability** — does the value survive past the promotion gate into domain folds, caches, or variant dispatch? durable → owner; ephemeral → payload or adapter-only carrier.
- **Invariant density** — does the shape enforce cross-field rules, smart construction, or replacement semantics? yes → owner; static key compatibility only → payload.
- **Foreignness** — does field naming, cardinality, discriminant encoding, or schema version differ from canonical? yes → adapter owns correspondence; interior never sees foreign layout.
- **Wire visibility** — is the shape published on the network, CLI schema, or provider contract? wire-facing static compatibility → payload or wire struct; canonical truth → owner.
- **Runtime class need** — is hashability, slot stability, codec speed, or JSON Schema compilation required at this stage? routes to NamedTuple, msgspec Struct, or Pydantic ingress — not TypedDict by default.
- **Openness posture** — does the contract admit typed extension bands or forbid unknown keys? TypedDict owns openness law via PEP 728 `closed` and `extra_items`; Struct uses `forbid_unknown_fields`; NamedTuple rejects extension entirely.

# Choose Typed Contract Payload

- Pre-materialization evidence: ingress staging, patch delta, event envelope, egress assignability target, keyword-callable `Unpack` surface.
- Static dictionary contract suffices: declared keys, per-key requiredness, `ReadOnly` evidence, and explicit `closed` / `extra_items` posture — no methods, no smart constructors, no nested invariant engine.
- Durable handoff intentionally deferred: config snapshot, provider compatibility probe, replay table row validated through `TypeAdapter(Payload)` without immediate owner promotion at the gate.
- Discriminant-selected body before tagged union materialization: closed envelope plus per-variant body payloads selected by `match` — not a materialized variant family yet.
- Partial construction or patch semantics where `NotRequired` and `ReadOnly` express update law without replacement owner in the same expression.
- Programmatic contract tables: PEP 749 `annotationlib.get_annotations` plus `__required_keys__`, `__optional_keys__`, and `__closed__` — payload modules stay type-only integration sources.

# Choose Materialized Owner

- Value crosses into invariant-bearing domain space: promotion gate exit is the binding moment — no lawful owner choice without a gate expression that constructs it.
- Smart construction returns `Result[T, E]` or tagged union arms require interior `match` with `assert_never`.
- Cross-field invariants, computed slots, enrichment, or immutable replacement (`model_copy`, `structs.replace`, `copy.replace`) belong on the durable instance.
- Variant family dispatch runs on tagged union owners — payload seams supply static discriminant evidence only; family logic never reads raw staging dicts post-gate.
- Absence semantics use vocabulary sentinels or `Option[T]` on owner fields — not payload `NotRequired` alone after promotion.
- Interior module signatures accept frozen `BaseModel`, msgspec `Struct`, dataclass, or rich class owners — never boundary payload assignability as a parameter type.
- Receipts, policy tables, and algorithm evidence with typed route or solver fields stay on materialized owners — not parallel payload mirrors.

# Choose Boundary Adapter

- Foreign layout must decode, normalize, validate, promote, or project without leaking rename tables into domain modules.
- Adapter expression owns the ordered pipeline slice: carrier decode → validate into payload or ingress model → discriminant `match` → construct owner at gate → optional egress projection.
- Field correspondence rows live once in the adapter module: wire key → canonical key, alias → validation name, provider overlay → extension fold target.
- Anti-corruption when ingress field count, discriminant encoding, or enum wire tokens differ from canonical — adapter documents each omission as wire-optional, domain-default, or computed-on-materialize.
- Dual-surface ingress (HTTP dict, CLI kwargs, settings env) converges through one adapter graph per concept — not parallel interior entrypoints.
- Adapter may terminate on validated payload when promotion is intentionally deferred; adapter must not stop on raw `dict[str, object]` when a static contract can name the shape.
- Egress adapter projects owner → wire struct or closed egress payload → bytes; interior modules emit canonical owners only.

# NamedTuple Versus TypedDict

- NamedTuple when field set is fixed, small, ordered, hashable, and never receives untrusted ingress — coordinate pairs, contract-table frozenset carriers, discriminant key bundles, promotion gate evidence tuples.
- NamedTuple rejects openness, PEP 728 `extra_items`, patch `NotRequired` semantics, and runtime dict assignability — do not use for wire envelopes with provider overlays.
- TypedDict when assignability to `dict` views matters, openness law applies, or `TypeAdapter` validates mapping material — not when the artifact must be hashable, slot-stable, or bytes-backed.
- NamedTuple ingress validates through constructor arity and types — not `TypeAdapter(TypedDict)`; promotion unpacks NamedTuple fields into owner constructors explicitly at the adapter gate exit.
- When a fixed row crosses untrusted decode, triage msgspec `Struct(frozen=True, forbid_unknown_fields=True)` instead of NamedTuple — repo policy treats NamedTuple as checker evidence, not boundary admission.
- Do not mirror the same concept as NamedTuple and TypedDict at the same stage; choose tuple evidence for fixed static rows, dict contract for mapping seams.

# msgspec Struct Versus Payload

- msgspec Struct owns wire egress and high-volume trusted-internal encode/decode — `frozen=True`, `forbid_unknown_fields=True`, optional `rename`, `omit_defaults`, `tag_field` / `tag` for tagged wire families, `cache_hash=True` when struct instances key registries after egress graduation.
- TypedDict owns pre-materialization static compatibility — no bytes path, no runtime field forbiddance on assignability alone, no `msgspec.convert` hot lane on the payload type itself.
- Ingress foreign bytes targeting domain invariants: Pydantic ingress model or `TypeAdapter` into payload first — Struct direct decode only when wire layout matches struct fields and trust posture documents single-pass admission with drift negatives.
- Never declare parallel TypedDict and Struct field lists for the same concept at the same stage slot; wire struct graduates from canonical projection, payload types ingress assignability — adapter correspondence rows connect them.
- Struct construction after decode is trusted; direct `Struct(**kwargs)` on untrusted material bypasses validation — same defect class as skipping `TypeAdapter` on closed payloads.
- Egress law: domain owner → adapter `msgspec.convert` or explicit struct construction → bytes; egress TypedDict, when used, types dict literals built in the adapter — not interior return types.

# Wire Payload Versus Domain Owner

- Wire payload names static field compatibility for foreign or published layouts — closed or bounded `extra_items`, discriminant literals aligned to wire tokens, `ReadOnly` on identity fields handlers must not rewrite.
- Domain owner names canonical invariants, vocabulary-enriched fields, and replacement targets — no wire aliases, no provider-specific key names, no comment-documented dict contracts.
- Same concept may carry wire payload at ingress, canonical owner in domain, wire struct at egress — three roles, three modules, one vocabulary export, adapter-owned maps between roles.
- Promotion is the sole lawful wire-to-domain transition; demotion for egress is projection, not mutation of the domain instance into a dict seam.
- JSON Schema authority for wire-visible payload names flows from `TypeAdapter(Payload).json_schema()` or ingress model schema — domain owner schema is not substituted at the wire boundary.
- When wire and canonical required-key sets diverge, split artifacts — do not widen payload requiredness to absorb domain defaults or tighten owner fields to match wire omissions without adapter documentation.

# Pydantic Ingress Model Versus TypedDict Payload

- Choose Pydantic ingress model when ingress needs alias graphs, `BeforeValidator` normalization, discriminated union routing on wire, computed wire fields, or published JSON Schema with coercion policy.
- Choose TypedDict payload when ingress needs static dict compatibility, checker-driven keyword surfaces, staging or patch law, or type-only modules without runtime model class — and validation via `TypeAdapter` suffices.
- Both may appear in one route: payload types keyword kwargs; ingress model wraps validated dict for alias collapse before promotion at the gate — adapter owns ordering, domain imports owner only.
- Do not duplicate field lists across payload and ingress model without adapter-documented divergence — discriminant vocabulary and required-key frozensets must reconcile.

# Python 3.15 Triage Signals

- PEP 749 `annotationlib.get_annotations(..., format=annotationlib.Format.FORWARDREF)` is the canonical read path for payload generics at contract introspection and adapter specialization sites — triage generic payloads before choosing parallel non-generic duplicates; raw `__annotations__` misses deferred thunks.
- PEP 696 default type parameters on generic payloads stabilize extension atom defaults — triage uses defaults for fixed-band envelopes, not unbounded `object` erosion.
- PEP 728 `closed=True` and `extra_items=T` on the payload candidate are mutually exclusive at definition and expose `__closed__` plus `extra_items` at runtime — triage picks closed wire envelopes or bounded extension bands before adapter binding; default-open assignability still rejects undeclared keys at construction.
- PEP 692 `Unpack` at root keyword entry triages to payload plus adapter validate expression — interior triage rejects forwarded `**kwargs` regardless of owner choice downstream.
- PEP 705 item-level `ReadOnly[T]` is the payload mutability signal — triage does not promote experimental PEP 767 attribute read-only into payload law; readiness modules isolate protocol read-only until PEP 767 is final and stdlib-promoted.

# Graduation And Demotion Triggers

- Payload → owner when staging completes, event admitted, or ingress validated into invariant-bearing space — one promotion unit retires staging payload imports from interior modules through the gate expression.
- Owner → wire struct when stable egress layout, volume encode, or persistence bytes require struct policy — graduation adds projection adapter rows, does not mutate owner fields.
- Payload → retired when concept graduates to variant family or wire struct and interior modules still import payload for the same field set — collapse in one promotion unit.
- Owner → never demoted to durable payload parameter in domain folds — egress projection produces assignability targets, not interior API shrink.
- Adapter-only carrier (`dict[str, object]`) → payload or ingress model when field set stabilizes — triage revisits at version events.

# Composition Root Binding

- Root owns triage outcomes: which concepts get payload modules, which routes bind `TypeAdapter(Payload)` versus ingress model versus direct struct decode, where promotion gate expressions live.
- One triage row per concept per stage: `{concept, stage, artifact_class, owner_module, promotion_expression, egress_projection}`.
- Interior imports follow triage: domain → owners and vocabulary; adapters → payloads, ingress models, struct projections, correspondence tables; payloads never imported from domain modules.
- Multi-route ingress shares one triage row for payload law; root wiring differs by carrier only.

# Trusted Replay And Volume Lanes

- Trusted replay validates into the same boundary artifact class as live ingress — payload type, ingress model, or struct decode path pinned beside schema version at the composition root; replay triage never shortcuts to owner construction without the live validate expression and identical gate binding.
- Volume egress triage prefers msgspec Struct when encode dominates and layout is stable; volume ingress still triages through adapter validation unless trust posture documents single-pass struct decode with drift negatives.
- In-process handoff between bounded contexts triages to canonical owner or frozen snapshot at the process boundary — not payload assignability threading through interior packages.
