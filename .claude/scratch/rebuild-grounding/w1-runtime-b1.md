# Grounding dossier — w1-runtime-b1 — evidence/reproduction.md [NEW]

Verified primary extracts only. Every anchor is `file:line`. Batch = the single NEW page
`libs/python/runtime/.planning/evidence/reproduction.md`, split out of `evidence/identity.md` §3
per brief `[V7]`. Downstream agents spot-verify these anchors.

## [A] .API TIER INVENTORIES (real `ls`)

### Shared/universal tier — `libs/python/.api/` (27 catalogs)
```
adbc-driver-manager.md anyio.md arro3-core.md beartype.md daft.md expression.md
grpcio-tools.md grpcio.md msgspec.md networkx.md numcodecs.md numpy.md
opentelemetry-api.md opentelemetry-exporter-otlp-proto-http.md opentelemetry-sdk.md
protobuf.md psutil.md pydantic-settings.md pydantic.md stamina.md structlog.md trio.md
universal-pathlib.md xarray.md xxhash.md zlib-ng.md
```
NOTE: `opentelemetry-api.md` present on disk today; brief `[V13]` deletes it (zero runtime delta).

### Folder tier — `libs/python/runtime/.api/` (22 catalogs)
```
apscheduler.md asyncssh.md cyclopts.md fsspec.md google-cloud-secret-manager.md
grpcio-health-checking.md grpcio.md httpx.md keyring.md lbt-recipes.md lz4.md msgspec.md
obstore.md opentelemetry-instrumentation-grpc.md pollination-handlers.md protobuf.md
queenbee.md tree-sitter-python.md tree-sitter-typescript.md tree-sitter.md watchfiles.md
```

### Doctrine root — `docs/stacks/python/` (11 pages)
```
README.md language.md shapes.md surfaces-and-dispatch.md iteration.md rails-and-effects.md
concurrency.md boundaries.md algorithms.md system-apis.md runtime.md
```

## [B] MEMBER VERIFICATION (installed source — every member the maps cite)

expression 5.6.0 (assay `api resolve expression` -> `restored`, 47/47 paths; members grepped in source):
- `.venv/.../expression/collections/block.py:91` `def append(self, other: Block[_TSource]) -> Block[_TSource]`
- `block.py:95` `def choose(self, chooser: Callable[[_TSource], Option[_TResult]]) -> Block[_TResult]`
- `block.py:115` `def collect(self, mapping: Callable[[_TSource], Block[_TResult]]) -> Block[_TResult]`
- `block.py:133` `def cons(self, element)`  · `block.py:138` `def empty() -> Block[Any]`
- `block.py:142` `def filter(self, predicate)`  · `block.py:157` `def fold(self, folder, state)`
- `block.py:239` `def map(self, mapping)`  · `block.py:273` `def sum_by(self, projection)`  · `block.py:299` `def of_seq(xs)`
- `expression/core/result.py:96` `def map` · `:124` `def map_error` · `:136` `def bind` · `:204` `def swap`
- `result.py:222` `def merge(self: Result[_TSource, _TSource]) -> _TSource` — CONSTRAINT: both branches SAME type; `SeedReproduction.contribute` `.map(...->Receipt).map_error(...->Receipt).merge()` satisfies it (both arms Receipt).
- `expression/core/option.py:64` `def default_value(self, value)` · `:76` `def default_with(self, getter)` · `:88` `def map` · `:204` `def is_none(self) -> bool` · `:218` `def of_optional(value)`

msgspec 0.21.1: `.venv/.../msgspec/__init__.pyi:89` `class Struct(metaclass=StructMeta)` (present, `hasattr` True).

xxhash: `.venv/.../xxhash/__init__.pyi:59` `def xxh3_128_intdigest(args: _InputType, seed: int = ...) -> int`; `:55` `def xxh3_64_intdigest(...)` (both `hasattr` True). Reused indirectly via `ContentIdentity.of`, never called in reproduction body.

## [C] SIBLING SEAM ANCHORS the page composes

### identity.md — the split SOURCE (reproduction is §3 extracted)
- `identity.md:41` `type KeyView = Literal["value", "hex", "memory", "digest"]`
- `identity.md:42` `type KeyRender = ContentKey | str | bytes | int`
- `identity.md:48-81` `class ContentKey(Struct, frozen=True, gc=False)`; `:61-72` `project` total `match` over KeyView; `:63-64` `case "hex": return f"{self.value:032x}:{self.fmt}"`; `:74-76` `hex` property; `:79-81` `memory` property `to_bytes(16, "little")`.
- `identity.md:96-137` `IdentitySource` `@tagged_union`; `:104-119` `lift` (arm order tuple/Struct/`Buffer()`/`Iterable()`); `:121-137` `fold` per-modality digest (`whole`/`canonical`/`merkle`/`stream`).
- `identity.md:179-193` `ContentIdentity.of` body (railed, `derived` aspect); `:163-178` the four `@overload` arms keyed on `view` `Literal`.
- `identity.md:196-218` `ContentIdentity.key` bare accessor (infallible byte/stream/merkle).
- SEED_REPRODUCTION §3 (the extracted concept) `identity.md:221-371`:
  - `:245` `type ParityAspect = Literal["value_identity", "memory_layout"]` (2 members)
  - `:249` `type FixtureState = Literal["real", "design_pin"]`
  - `:258-262` `CANONICAL_STREAM`/`CANONICAL_DIGEST = 0x9462A71A5DD13DCFA3B1D6D225FCBE70`/`CANONICAL_LE_MEMORY`
  - `:267-277` `ParityReceipt(Struct, frozen=True)`; `:275-276` `fact` property `(f"{fixture}.{aspect}", "ok"|mismatch)`
  - `:279-287` `ParityRow`; `:284-286` `grade(fixture, key)` derives `key.project(view)`, verdict `observed == expected`
  - `:289-301` `CorpusFixture(name, state, producer, stream: Option[bytes], rows: Block[ParityRow])`
  - `:305-330` `_CORPUS: Block[CorpusFixture]` — ONLY 2 ROWS: `:306-315` IDENTITY_FMT `state="real"` (2 rows digest+memory), `:327-329` GOLDEN_FMT `state="design_pin"` `stream=Nothing` `rows=Block.empty()`
  - `:336-351` `SeedReproduction.grade` — manual `_CORPUS.fold(step, Ok(Block.empty()))`; `:342-349` step uses `acc.bind(... fixture.stream.map(... ContentIdentity.of(fixture.name, stream, view="value", seed=Some(0)) ...).default_value(Ok(graded)))` — WHOLE/seed-zero path only, `Result.bind` ABORT accumulation
  - `:353-370` `contribute` -> `(graded, *pending)`; `:358-364` `grade().map(->Receipt.of emitted).map_error(->Receipt.of fault).merge()`; `:365-369` `_CORPUS.choose(... Some(Receipt.of(name,("planned",name,{"design_pin":producer}))) if stream.is_none() else Nothing)`
- CROSS-FOLDER OBLIGATION PROSE naming the missing rows — `identity.md:229` Boundary: "the corpus rows [3]-[7] (`FAULT_TRIPLES`, `CRDT_OP_SET`, `GLB_BY_KEY`, `HLC_TWO_HALF`, `MATERIAL_LAYER_GOLDEN`) stay DESIGN-PIN on their cross-folder producers and carry no fabricated bytes"; `:224` MATERIAL_LAYER_GOLDEN [H7] float-canon detail; `:10` index [02]-[SEED_REPRODUCTION] naming the ONE_WIRE_FIXTURE_CORPUS + MATERIAL_LAYER_GOLDEN [H7].

### receipts.md — the receipt seam (reproduction is a `ReceiptContributor`)
- `receipts.md:57` `type Phase = Literal["admitted", "planned", "emitted"]`
- `receipts.md:70` `PHASE_LEVEL` Map (`admitted`/`planned`->debug, `emitted`->info)
- `receipts.md:105-133` `Receipt` `@tagged_union` (`fact`/`rejected`/`drained`); `:108` `fact: tuple[Phase, str, str, dict[str, object]]`
- `receipts.md:112-122` `Receipt.of(owner, evidence)` — `match`: `(phase, subject, facts)` triple -> `fact`; `BoundaryFault` -> `rejected`; `DrainReceipt` -> `drained`
- `receipts.md:136-138` `@runtime_checkable class ReceiptContributor(Protocol): def contribute(self) -> Iterable[Receipt]`
- `receipts.md:61` `type Evidence = tuple[Phase, str, dict[str, object]] | BoundaryFault | DrainReceipt[object]`

### faults.md — the rail carrier
- `faults.md:155` `type RuntimeRail[T] = Result[T, BoundaryFault]`
- `faults.md:54` `class BoundaryFault` `@tagged_union`; `:47` `class Disposition(StrEnum)`
- `faults.md:211-213` `traversed` overloads — `:211` `by: Literal[Disposition.ABORT, Disposition.ACCUMULATE]=...) -> RuntimeRail[Block[T]]`; `:213` `by: Literal[Disposition.PARTITION]) -> RuntimeRail[tuple[Block[T], Block[BoundaryFault]]]` (ACCUMULATE combines faults rather than aborting).

### evidence.md — the nearest sibling in-folder (parallel ReceiptContributor pattern)
- `evidence.md:268-281` `EvidenceScan(Struct, frozen=True)` `contribute()` -> `Receipt.of(self.owner, ("emitted","code.scan",facts))` — the sibling contributor shape reproduction mirrors.

## [D] BRIEF SEAM/RIDER ANCHORS covering this page

- `RASM-PY-RUNTIME-BRIEF.md:32` STRUCTURAL_AUTHORITY: "`evidence/identity.md` splitting out `evidence/reproduction.md`".
- `:40` IMPORT_DAG_LAW: `reproduction` in braced peer set `{reproduction, evidence, roots, shapes}` above `telemetry`, below `wire`; imports only `identity` + `receipts` (both strictly earlier).
- `:45` IMPORT_DAG_LAW: "`SeedReproduction` becomes the declared second module `reproduction.py` — its `identity.md:239-241` prelude already imports `identity` AND `receipts`, a hard cycle inside one file and a valid module above `receipts` as two".
- `:97` V7_IDENTITY_MECHANISM: NEW `evidence/reproduction.md` (`SeedReproduction`/`CorpusFixture`/`ParityReceipt` corpus fold — DESIGN-PIN discipline, `MATERIAL_LAYER_GOLDEN` obligation, `planned`-phase law carry over intact). Consumer contracts: `data` (icechunk snapshot-seed fixture, `RASM-PY-DATA-BRIEF.md [V6]`), `geometry` (`GLB_BY_KEY` golden fixture beside MATERIAL_LAYER_GOLDEN, seed-zero `Some(0)` `RepresentationContentHash` parity, `RASM-PY-GEOMETRY-BRIEF.md [V3]`), `compute` (`ARRAY_LAYOUT` cross-backend + `EvidenceBundle` golden bundle, `RASM-PY-COMPUTE-BRIEF.md [V12]`) — "the span-fold collapse may not narrow either surface"; `CorpusFixture`/`ParityReceipt` are exported consumer law.
- `:202` BUILD_LEGS leg 1 BASE: "NEW `evidence/reproduction.md` LAST (V7 split; its prelude binds `identity` AND `receipts` downward)".
- `:85` V4: codemap "gains the missing nodes (`shapes`, `reproduction`, `recipe`)".
- `:161` CAPABILITY_ESCALATION `evidence/identity(+reproduction)` 7 -> 9.5: "module split + canonical name".

## [E] FOLDER-CONTEXT / GOVERNANCE ANCHORS (leg-tail ripples this page triggers)

- `libs/python/runtime/README.md:18-19` current router rows `[12]-[IDENTITY]`, `[13]-[EVIDENCE]` — reproduction needs a NEW router row (governance tail).
- `libs/python/runtime/ARCHITECTURE.md:26-28` codemap `evidence/` sub-tree has `identity.py` + `evidence.py` only — needs `reproduction.py` node.
- `ARCHITECTURE.md:36-37` seam rows attribute cross-runtime parity to `evidence/identity`: `:36` `⇄ csharp:Rasm/Spatial/reconciliation [CONTENT_KEY]: ... (CANONICAL_BYTE_IDENTITY)`; `:37` `⇄ csharp:Rasm.Element/Projection [CONTENT_KEY]: MATERIAL_LAYER_GOLDEN float-bearing ... ([H7])` — these two parity seams RE-ATTRIBUTE from identity to the new `reproduction` owner (the parity binding moves; identity keeps only the digest-mechanics seam). Leg-tail governance work.
- `ARCHITECTURE.md:52` `evidence ← python:geometry/mesh [CONTENT_KEY]: ContentIdentity.of keyed GLB bytes with policy seed` — the geometry GLB consumer whose `GLB_BY_KEY` fixture the reproduction corpus admits.
