# [CENSUS_CORE]

Read-only truthful census of `libs/typescript/core`: `.planning/` design pages, `.api/` catalogs, `README.md`, `ARCHITECTURE.md`, `IDEAS.md`, `TASKLOG.md`. Every file listed below was fully read; LOC via `loc`.

## [1]-[FILE_REGISTER]

Four sub-domains, 22 design pages, each a `Schema.Class`/assembled-owner module with statics carrying its whole algebra.

### [1.1]-[VALUE] (`value/*.md` — the cross-language value floor)

| [FILE] | [LOC] | [OWNS] | [ENTRY_SURFACES] | [MASS] |
| :----- | :---: | :----- | :--------------- | :----: |
| `value/schema.md` | 122 | `Refined` (Guid-v7, OrdinalKey, JsonPointer, Locale brands), `Ingress` (decode-budget ceilings + `bounded` schema combinator) | `Refined.Guid/OrdinalKey/JsonPointer/Locale`, `Ingress.floor`, `Ingress.bounded(schema, budget?)` | light — two small owners, no class |
| `value/identity.md` | 93 | `AppIdentity` (app/tenant/build/host four-dim identity, `Schema.Class`), `TenantContext` (scope-key tenancy value) | `AppIdentity.scoped/.label`, `TenantContext.scope` | light |
| `value/contentKey.md` | 151 | `ContentKey` (XxHash128 seed-0 `:x32` brand), `Digest` (width-row hasher table: content/trace/check + session algebra + `mac` keyed seal) | `Digest.mint/session/absorb/finish/mac/FromBytes` | medium — sole `hash-wasm` import site |
| `value/clock.md` | 183 | `Hlc` (two-half stamp, `Schema.Class`, byte layout twin), `Uncertainty` (wall-clock window + grade ladder) | `Hlc.tick/receive/Order/FromBytes/physicalOf/delta`, `Uncertainty.around/precedes/hull/contains` | medium |
| `value/quantity.md` | 157 | `Dimension` (7-axis SI exponent vector + named rows), `Quantity`/`QuantityFault` (Either-railed SI arithmetic) | `Dimension.product/quotient/pow` + named statics, `Quantity.of/sum/difference/product/quotient/scale/pow/ratio` | medium |
| `value/fault.md` | 283 | `FaultClass` (10-class severity table), `FaultCapture`/`FaultEnricher` (crash evidence + enrichment port), `Budget` (4-row compiled retry `Schedule`s), `Degrade` (3-rung silence ladder) | `FaultClass.of/dominant/retryable`, `Budget.schedule(kind)`, `Degrade.level` | heavy — 4 clusters, widest-consumed vocabulary in the folder |

### [1.2]-[STATE] (`state/*.md` — host-free state algebra)

| [FILE] | [LOC] | [OWNS] | [ENTRY_SURFACES] | [MASS] |
| :----- | :---: | :----- | :--------------- | :----: |
| `state/merge.md` | 321 | `Merge.Instance<A>` (lawful-merge contract: max/min/lattice/first/counter/flag/union/optional/struct), `Converge`/`Breach` (law-obligation surface), `Merge.cell` (STM keyed transactional table) | `Merge.fold/monoid/convergent/cell`, `Converge.obligations/witness/commutes/tables` | heavy — every CRDT/fold in the branch composes this |
| `state/fold.md` | 696 | `Fold.Plan`/`Fold.run` (keyed-fold contract), `AsOf` (one 3-coordinate read time), `Replay.memory/ordered/joined/grouped/closure/versioned` (d2mini/d2ts incremental engines), `Window` (watermark panes) | `Fold.plan/step/trace/run`, `Replay.*`, `Window.mark/verdict/spread/panes/close` | heaviest page in folder — the differential-dataflow engine binding |
| `state/causal.md` | 262 | `Vector` (version-vector lattice), `Causal.compare/admit/frontier/finalize/retention/tracker` (happened-before, delivery buffer, STM frontier tracker) | `Vector.compare/join/meet/observe`, `Causal.tracker()` | heavy |
| `state/commit.md` | 119 | `Commit` (content-keyed commit class + branch head + Merkle summary/divergence) | `Commit.summarize/diverges/Branch/Merkle` | light-medium |
| `state/machine.md` | 172 | `Transition.Table`/`Spec` (closed transition system), pure `step`/`drive`, `@effect/experimental Machine`-backed serializable actor (`boot`/`restore`) | `Transition.spec/step/drive/boot/restore` | medium |
| `state/evidence.md` | 378 | `Receipt`/`ReceiptEnvelope` (C# AppHost outcome union + LWW fold), `ProgressMark`/`Progress` (progress state product + rollup), `Availability` (degradation lattice + gate) | `ReceiptEnvelope.latest/plan`, `Progress.fraction/stalled/rollup`, `Availability.worst/admits/plan` | heavy — 4 clusters, the wire-decoded evidence hub |
| `state/feed.md` | 230 | `DocumentRef` (content-keyed result-doc ref), `Feed.Entry` (Receipt/Progress/Shift/Document tagged enum), ordered/coalescing `Feed` fold | `Feed.absorb/merge/plan/window/recent` | medium |
| `state/presence.md` | 162 | `Presence.Op` (Join/Beat/Leave), per-actor state product, roster status reads | `Presence.plan/status/roster` | light-medium |

### [1.3]-[INTERCHANGE] (`interchange/*.md` — C#-minted wire plane)

| [FILE] | [LOC] | [OWNS] | [ENTRY_SURFACES] | [MASS] |
| :----- | :---: | :----- | :--------------- | :----: |
| `interchange/format.md` | 348 | `Proto` (protobuf-es engine, 34-family suite, one registry), `Cbor` (canonical decoder + DoS gate), `Pack` (MessagePack + `Hlc` ext row), `Patch` (RFC 6902 engine) | `Proto.frame/family/stream/peek`, `Cbor.frame/frames/GateLive`, `Pack.schema/stream/encode/transfer`, `Patch.apply/diff/guarded/key` | heaviest single-concern page — 4 independent byte-dialect engines |
| `interchange/codec.md` | 1135 | `Wire` (closed 42-family census + keyed registry), `WireFault`/`Quarantine` (fault rail + poison intake), `Parity` (content-key verify family), landing classes for evidence/identity/version/BIM/shell/geo/appearance planes, `CrdtOp`, `feed`, `Gap`/`OpLog` | `Wire.decode/encode/schema/stream/diverted/verifiedSnapshot/admittedGraph`, `Quarantine.intake/replayed/divert`, `Parity.verified/roundtrip/cells`, `feed(family, frames)`, `OpLog.stream/frontier` | by far the largest file in the folder — the plane's central registry plus ~30 wire-owned landing classes (`RenderReceipt`, `FaultDetail`, `FlagVerdict`, `BindingStatus`/`CoercedValue`/`WriteReceipt`, `ControlIntent`, `LayoutProgram`, `BcfTopic`/`BcfViewpoint`, `BimModel`/`BimDiff`/`IdsAudit`, `Material`/`PbrGroups`/`AppearanceSummary`, `GeoFeature`, `SnapshotHeader`, `HostFingerprint`/`Claim`, `Credential`) |
| `interchange/frame.md` | 250 | `ArtifactFrame` (budgeted keyed reassembly + verify), `GeometryFrame` (GLB envelope/tensor/zero-copy views), `Residency` (manifest/delta ledger fold) | `ArtifactFrame.reassembled`, `GeometryFrame.stream/view`, `Residency.stream/folded` | medium |
| `interchange/contract.md` | 304 | `ContractDrift` (verdict receipt + severity/grade tables), `DescriptorGate` (boot-time FileDescriptorSet diff service) | `ContractDrift.dominant/graded`, `DescriptorGate.verdict/census/admitted` | medium |
| `interchange/invoke.md` | 347 | `Transport` (ConnectError->FaultDetail fold), `Dial` (protocol-lane Config + ExecutionPlan failover), `Capability`/`CapabilityDescriptor` (kind-total SDK bind), `Gateway`/`CommandPayload`/`Dispatched`/`AvailabilityGate`/`SupportCapture`/`SupportIntake`/duplex channel | `Dial.client/plan/unary/stream`, `Capability.bind`, `Gateway.submit/duplex` | heavy — 4 clusters, both invocation directions |

### [1.4]-[OBSERVE] (`observe/*.md` — observability vocabulary, zero exporters)

| [FILE] | [LOC] | [OWNS] | [ENTRY_SURFACES] | [MASS] |
| :----- | :---: | :----- | :--------------- | :----: |
| `observe/convention.md` | 192 | `Convention` (semconv stable/incubating rows, Rasm-owned name families, `AppIdentity -> Identity` projection) | `Convention.attr/metric/event/value/identity(identity)` | light-medium — pure vocabulary, sole `@opentelemetry/semantic-conventions` importer in core |
| `observe/slo.md` | 183 | `Sli`/`Slo.Objective` (SLI policy value), `_BURN` (4-row multi-window multi-burn-rate table), `Slo.evaluate/budget/spent`, `Alert.of` (spec derivation) | `Slo.evaluate/burn/budget/spent`, `Alert.of/severity` | medium |
| `observe/board.md` | 293 | `Query` (typed PromQL-dialect expression algebra), `Panel` (6-row tagged union), `DashboardModel` (identity-derived dashboard + shelf layout + pack dispatch) | `DashboardModel.of/laid/pack/suite`, `Query.render` | medium — the pack layer composes `slo`+`convention` payloads only, imports no later-wave vocabulary |

### [1.5]-[ROOT_DOCS]

| [FILE] | [LOC] | [ROLE] |
| :----- | :---: | :----- |
| `README.md` | 62 | Router table (22 pages), domain-package roster, substrate-package roster |
| `ARCHITECTURE.md` | 62 | Domain map (`codemap`), seams table, organization/boundaries prose |
| `IDEAS.md` | 28 | Forward idea pool — **empty** (`(none)` in both OPEN and CLOSED) |
| `TASKLOG.md` | 29 | Task pool — **empty** (`(none)` in both OPEN and CLOSED) |
| `project.json` | — | Nx project descriptor, not read for design content |

Total design-page LOC: 6602 across 22 files (`interchange/codec.md` alone is 1135, ~17% of the folder).

## [2]-[API_ROSTER]

`.api/core/` (11 catalogs, folder-scoped) plus 8 substrate catalogs at `libs/typescript/.api/` consumed but not owned by `core`.

| [CATALOG] | [PACKAGE] | [DEPTH_SIGNAL] |
| :-------- | :-------- | :------------- |
| `core/.api/bufbuild-protobuf.md` | `@bufbuild/protobuf` | present — backs `Proto` engine (fromBinary/toBinary/createRegistry/reflect/buildPath), `contract.md`'s descriptor diff, `invoke.md`'s `DescService`/`Client` typing |
| `core/.api/connectrpc-connect.md` | `@connectrpc/connect` | present — backs `Transport`/`Dial`/`Capability` (`ConnectError`, `Code`, `createClient`, `Interceptor`) |
| `core/.api/connectrpc-connect-web.md` | `@connectrpc/connect-web` | present — backs `Dial`'s two lane factories (`createConnectTransport`/`createGrpcWebTransport`) |
| `core/.api/cbor-x.md` | `cbor-x` | present — backs `Cbor` engine; page itself documents a local `declare module` quirk-augmentation (shipped `.d.ts` mislabels `setMaxLimits`, phantoms `MAX_LIMITS_OPTIONS`) not yet reconciled into the catalog |
| `core/.api/msgpack-msgpack.md` | `@msgpack/msgpack` | present — backs `Pack` engine (`Decoder`/`Encoder`/`ExtensionCodec`/`decodeMultiStream`) |
| `core/.api/rfc6902.md` | `rfc6902` | present — backs `Patch` engine (`applyPatch`/`createPatch`/`createTests`/`isDestructive`) |
| `core/.api/hash-wasm.md` | `hash-wasm` | present — backs `Digest` (createXXHash128/64, createCRC32, createBLAKE3, `IHasher`) |
| `core/.api/electric-sql-d2mini.md` | `@electric-sql/d2mini` | present — backs `Replay.memory/ordered/joined/grouped/closure` (in-memory/browser dataflow) |
| `core/.api/electric-sql-d2ts.md` | `@electric-sql/d2ts` | present — backs `Replay.versioned` and `Replay.closure`'s `Index`/`Antichain`/fixpoint machinery |
| `core/.api/effect-typeclass.md` | `@effect/typeclass` | present — backs `Merge.Instance` (`Semigroup`/`Monoid`/`Bounded`/`data/*` atom instances) |
| `core/.api/opentelemetry-semantic-conventions.md` | `@opentelemetry/semantic-conventions` | present — backs `Convention`'s stable/incubating attribute imports |

Substrate tier consumed (owned at `libs/typescript/.api/`, not this folder): `effect.md`, `effect-platform.md`, `effect-platform-node.md`, `effect-platform-browser.md`, `effect-platform-bun.md`, `effect-experimental.md` (backs `machine.md`'s `Machine.makeSerializable`), `effect-opentelemetry.md`, `ssh2.md` (unused by core).

No `.api` catalog gap found: every package named in `ARCHITECTURE.md`'s `[02]`/`[03]` package rosters (`@bufbuild/protobuf`, `@connectrpc/connect`, `@connectrpc/connect-web`, `cbor-x`, `@msgpack/msgpack`, `rfc6902`, `hash-wasm`, `@electric-sql/d2mini`, `@electric-sql/d2ts`, `@effect/typeclass`, `@opentelemetry/semantic-conventions`, `effect`, `@effect/platform`) has a corresponding catalog file at one of the two tiers.

## [3]-[CAPABILITY_MAP]

### [3.1]-[VERIFIED_AS_CLAIMED]

- Four value/state/interchange/observe sub-domains as `README.md`/`ARCHITECTURE.md` describe, each page a design fence with real `.planning/*.md` backing — no phantom router entries: all 22 `[01]-[ROUTER]` rows in `README.md` resolve to files that exist and were read.
- `ARCHITECTURE.md`'s `codemap` names 21 eventual source files (`value/*.ts` x6, `state/*.ts` x8, `interchange/*.ts` x5, `observe/*.ts` x3 — wait, 6+8+5+3=22, matches page count) — one-to-one with the 22 design pages; no orphan design page without a codemap node, no codemap node without a page.
- `ARCHITECTURE.md`'s `[02]-[SEAMS]` table names 20 cross-branch/cross-language seams; every named seam owner (`value/contentKey`, `value/clock`, `value/quantity`, `interchange/codec`, `interchange/contract`, `state/fold`, `state/feed`, `value/fault`, `observe/convention`, `observe/board`) is a real cluster inside the page it claims — verified by direct read, not inferred.
- Boundary law ("core imports nothing from the branch, nothing host-bound") holds by inspection: every `import` block across all 22 pages resolves to `effect`/`@effect/*`/named external packages/sibling `core/src/**` relative paths only — no `../../security`, `../../data`, `../../runtime`, `../../ui` import anywhere in the read fences.
- `interchange/codec.md`'s 42-row wire census is internally consistent: every family in `_families` has exactly one `_census` row, and every `home: "codec"` row has a matching entry in `_landingRows`/`_schemas` (spot-checked full cross-reference during read — no dangling row found in the read fences).

### [3.2]-[MISMATCHES_AND_GAPS]

- **IDEAS.md and TASKLOG.md are both fully empty** (`(none)` in every section) — the folder carries zero forward idea pool and zero open/closed task history, despite `README.md`'s own prose pointing to `IDEAS.md` "the forward pool" and `TASKLOG.md` "the open work." This is a genuine gap between what the README implies exists and what is on disk: either the folder is claimed fully settled with no deferred work (plausible given the density of every page) or the pool was never populated/was cleared without replacement.
- **`cbor-x` API catalog likely stale against the page's own documented quirk**: `interchange/format.md`'s `CBOR_ENGINE` cluster states the shipped `cbor-x` `index.d.ts` "mislabels the size gate `setMaxLimits` and phantoms `MAX_LIMITS_OPTIONS` while the runtime exports `setSizeLimits` and `decodeIter`," and the page carries its own local `declare module "cbor-x"` augmentation as the fix. Whether `core/.api/cbor-x.md` itself documents this mismatch (so a rebuild agent does not silently trust the catalog's typings over the page's corrective declaration) could not be fully confirmed without opening the catalog file — flagged as a verify-before-consuming risk for the next phase, not a confirmed defect.
- **No sub-folder `.planning` nesting beyond the four named domains** — `ARCHITECTURE.md`'s codemap claims exactly `value/`, `state/`, `interchange/`, `observe/` and the folder listing confirms no fifth top-level `.planning` sub-folder exists; the map is not aspirational beyond what is on disk.
- **`interchange/codec.md` at 1135 LOC is markedly the densest file in the branch's `core` folder** (next largest is `state/fold.md` at 696) — this is a direct, explicit consequence of the "ONE keyed-decode registry" design law (a new wire family is one row, never a new page), not an unaccounted-for sprawl; the page's own prose states this design intent plainly, so the size is claimed capability, not a hidden cost, but it is the single highest-complexity target for any downstream rebuild/critique pass.
- **No evidence of drift between `ARCHITECTURE.md`'s `[04]-[BOUNDARIES]` claims and the read fences**: "Secret derivation is the security folder's concern" holds (no argon2id/bcrypt/scrypt/pbkdf2 anywhere in `value/contentKey.md`, only content-identity hashing); "Persistence, transport, serving, rendering, and exporters are later-wave concerns" holds (no `@effect/sql`, no OTLP SDK/exporter package, no rendering library import found in any of the 22 pages).
