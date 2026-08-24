# [TYPESCRIPT_BRANCH_RULINGS]

`libs/typescript` rulings settle branch-spanning decisions.

## [01]-[PACKAGES]

- `pnpm-workspace.yaml` `catalog:` owns every registry version while `catalogMode` gates `pnpm add` — a hand-written literal installs unrefused.
- Folder-tier dependency isolation proves at the architecture suite — folders carry no manifest, so every root devDependency resolves from each.
- `libs/typescript` declares no test dependency — proof reaches it from `tests/typescript/*`, and the reverse edge cycles the workspace graph.

## [02]-[SHAPE]

- Folder faults read rank, blame, recovery band, and re-offer route off the core `Fault.Class` rows — a local policy column forks the taxonomy.
- Recovery bands what a re-drive reaches — `terminal` outlasts every schedule, `transient` takes a blind curve, `throttled` names a stated window.
- Re-offer names the caller's route beside it — `wait` re-invokes under the budget, `restart` re-takes the handle, `rescope` narrows the next offer.
- `throttled` classes carry their window on the VALUE as `Fault.Class.After` — `Fault.Budget.schedule` re-seats the row base from it, never a column.
- `retryable` is the derived projection `recovery !== "terminal"` alone — a stored column or raise-supplied field beside the band forks it.
- Each owner mints its tagged fault through `Fault.Class.family` — a shared base or tag-blind factory erases the `_tag` identity `catchTag` reads.
- Family rows carry class, owning leg, and their own subject schema, and each row renders its detail — a free-string `detail` re-opens `reason`.
- Row subjects seat only columns EVERY raiser holds; a coordinate one raiser owns seats REQUIRED on its reason's subject, never a shared `Option`.
- Accumulating admission mints once through `Fault.Class.family(…).census` — re-declaring `{ issues, class, message }` forks one taxonomy into two.
- Family values publish their own census, spreading the vocabulary they mint — a `static readonly roster` restating that tuple drifts from it.
- Untyped failure grades `defect` at `Fault.Class` — `terminal` recovery, `restart` re-offer, system-blamed, refused by every `Fault.Budget` gate.
- `Effect.tryPromise`/`Effect.try` replace `Effect.promise`/`Effect.sync`, and `Effect.async` guards the synchronous throw beside its callback.
- Foreign and `unknown` error channels fold onto the typed rail at the seam that widened them, never past the shielded gate downstream.
- `Tap` owns names, modalities, handlers, and DELIVERY; registrars own rosters and policy rows — a second engine forks veto order and breach count.
- Signal concepts two folders spell own where the surface sits — a second `Tap` registrar double-accounts the buffer and forks each panel's budget.
- Tenancy baggage decodes ONLY through `Identity.Tenant.FromScope` under `Convention.rasm.tenant` — a second parse forks the dimension's alphabet.
- `CookieSpec` owns the one CSRF echo header the serve gate reads and the browser dial stamps — a literal at either end forks the pair fail-closed.
- Deploy-to-process keys derive from `StackOutputs` and `Setting` as ONE typed catalog — a hand-matched pair forks where a rename breaks the build.
- `Source._addressed` publishes the `assets/<digest>/<file>` join `Glb.assetPath` consumes — no import binds them, so a re-derivation forks the path.
- `Source._CACHE_POSTURE` owns the served headers the serve fold copies, selected on the `assets/` prefix — a leaf-pattern selector forks the answer.
- `Source._addressedAll` and `Glb.assetDir` seat sibling leaves under one set-keyed `assets/<digest>/` — a per-file digest splits one asset in half.
- `rasm.*` instruments mint at the core convention owner alone — `@effect/opentelemetry` derives the exported unit from `Convention.wire.unit` alone.
- `rasm.work.family` rides every actor message span, `rasm.work.shard` the lifetime span alone — `toLayer.spanAttributes` is a static record.
- Wasm modules are capability, never code — the folder-owned artifact acquires scoped behind a `Context.Tag`, and no linear-memory view escapes it.
- Scalars cross a seam with typed unit and frame — `jose` reads `uat` in ms, `oauth4webapi` in seconds; `watlas` strides bytes, `meshopt` float32.
- Isolation spells `tenancy` on every descriptor row — `residency`, `partition`, and `scope` name it nowhere, each staying live as its own concept.
- Provider-native retry pins to ONE attempt and `value/fault#RETRY_BUDGET` owns every curve — a nested SDK schedule multiplies effective attempts.
- Closed tenancy seats at `core/value/identity#IDENTITY_OWNER` — every stratum reads `Identity.tenancy` and no folder re-mints `none|single|multi`.
- Backend-plane families key on `signals` alone; `admit` seats on the `_Plane` floor per family shape — re-declaring below forks one coordinate.
- Durability derives from `topology` at BOTH planes through `Profile.recoveryOf` — a stored spec field forks the runner's target off the boot row's.
- Recovery objectives cross STRUCTURALLY — an S2 grader reaches no S3 schema, and `Converge.Profile` naming a spec type refuses every foreign root.
- Env custody is the `Setting` family form — one described record per namespace resolved at its owning construction; a second decode site forks it.
- Config rows resolve at the boot line, never first use — a roster row picks which rows resolve, so optional capability never defers the proof.
- Package-owned config records satisfy the `Setting` family at the package seam — a folder restatement forks it.
- `iac` mints no `Setting` group — deploy-host env owns its own records, shape crossing as decoded `StackSpec`, material as in-graph Doppler rows.
- Counter-plane availability stamps the claim's host fingerprint, never a metric band — a band conflates machine refusal with an uncounted metric.
- Moderation verdicts never borrow the transport reason — transport bands `transient`, a verdict `denied`, so one shared cell re-drives the refusal.
- `MachinePrincipal.credential` is the HTTP presentation alone — SASL and NATS CONNECT carry the bare `token`, so the prefixed form double-prefixes.
- `dpop`-scheme principals present only through security's proved call — `DPoPHandle` publishes `calculateThumbprint()` alone, so other wires refuse.
- `DateTime.distanceDuration` is ABSOLUTE and `distanceDurationEither` lands an equal pair `Left` — deadline gates read signed `distance` or `isPast`.
- `Schema.partialWith` drops the record's node annotation — a closed key domain piped through it re-seats its posture outside or silently reopens.
- Security's `Intake` names the held-octet verify boundary alone — serve's webhook spells `Inbound`, since a borrowed Tag name forks one seam word.
- Queue durability is the SKIP-LOCKED outbox — `data` owns the outbox relation and `runtime` the relay, so no broker deployment enters `iac`.
- `Migrator` binds nowhere — ensure rows census fail-closed at Layer construction and the deploy plane applies them, so runtime mutates no schema.

## [03]-[COLLAPSE]

- Connect egress has ONE supported-pair selector — `core:interchange/invoke#DIAL_AXIS`; runtime supplies the scoped Node adapter alone.
- One `Code`→fault table per branch — `core:interchange/codec` `Wire.Hops`; a folder grading a code beside it forks retry by which module dialled.
- `VariantSchema` binds structurally parallel projections of ONE decoded truth — a semantically divergent form keeps its own declaration.
- Rate limiting stays three postures — `Gate.window` refuses, `Throttle` and `Olap.ingest` delay, `RateLimiter` counts; one owner erases each price.
- Fault altitudes stay three — interchange adopts `FaultDetail`, folders raise `Fault.Class`, `Problem` prices egress, each owning its own source.
- One rate row is four columns — `window`, `limit`, `key`, `cost` — per posture site, `scope` joined into `key` since the counter namespaces nothing.

## [04]-[STRUCTURE]

Retired surfaces fill their gap from the durable owner named, never by re-minting the page.

- `dataflow-system.md` does NOT re-enter — every spine law lives at its folder owner and the branch `ARCHITECTURE.md` carries the spine as diagrams.
- `work/` at top level does NOT re-enter — delivery, report, queue, flow, schedule, and actor capability re-homed into `runtime/.planning/work/`.
- Core mints no `VariantSchema` seat — shapes doctrine owns derivation; each folder seats `Model.Class`/`VariantSchema.make` in place.

## [05]-[PROCESS]

- (none)
