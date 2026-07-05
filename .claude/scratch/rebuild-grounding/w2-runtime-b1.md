# W2-RUNTIME-B1 GROUNDING — evidence/evidence.md [improve]

Verified primary extracts only. Every cited member carries a `file:line` (or installed-surface line) anchor. Batch = `libs/python/runtime/.planning/evidence/evidence.md`.

## [00]-[INVENTORIES]

Doctrine root `ls` — `docs/stacks/python/`:
`algorithms.md boundaries.md concurrency.md iteration.md language.md rails-and-effects.md README.md runtime.md shapes.md surfaces-and-dispatch.md system-apis.md` (atlas order in `docs/stacks/python/README.md:11-22`; `iteration.md` is atlas index [04]).

Shared substrate `.api` tier `ls` — `libs/python/.api/`:
`adbc-driver-manager anyio arro3-core beartype daft expression grpcio-tools grpcio msgspec networkx numcodecs numpy opentelemetry-api opentelemetry-exporter-otlp-proto-http opentelemetry-sdk protobuf psutil pydantic-settings pydantic stamina structlog trio universal-pathlib xarray xxhash zlib-ng` (all `.md`).

Folder `.api` tier `ls` — `libs/python/runtime/.api/`:
`apscheduler asyncssh cyclopts fsspec google-cloud-secret-manager grpcio-health-checking grpcio httpx keyring lbt-recipes lz4 msgspec obstore opentelemetry-instrumentation-grpc pollination-handlers protobuf queenbee tree-sitter tree-sitter-python tree-sitter-typescript watchfiles` (all `.md`).

Evidence sub-folder `ls` — `libs/python/runtime/.planning/evidence/`: `evidence.md identity.md reproduction.md`.

Installed surfaces probed (verified-local): `tree_sitter 0.25.2` (`.venv/.../tree_sitter/__init__.pyi`), `tree_sitter_typescript 0.23.2`, `expression 5.6.0`. `assay api resolve tree_sitter` → `status:ok`, `12/12 all paths present`, `.pyi` present.

## [01]-[TARGET PAGE ANCHORS] evidence/evidence.md

- Tracer literal (defect anchor): `evidence.md:53` — `_TRACER: Final = trace.get_tracer("rasm.runtime.evidence")` (raw string literal, NOT `SCOPES[Scope.EVIDENCE]`). Faults import list `evidence.md:43` — `from rasm.runtime.faults import BoundaryFault, Disposition, RuntimeRail, trapped, traversed` (no `SCOPES`/`Scope`).
- Probe vocabulary: `evidence.md:48` — `type Probe = Literal["binding", "tags", "highlights"]` (3 members; no `locals`). `PROBE_SOURCES` rows `evidence.md:139-146` cover binding/tags/highlights only.
- Partial-coverage machinery (claimed, never exercised by a real partial probe): `CompiledProbe.of` `evidence.md:129-132` — `covered = grammars.filter(lambda lang, _grammar: lang in sources)` then `covered.map(...Query...)`.
- `run` hot loop reads capture as string: `evidence.md:203` — `into(lang, name, node) for name, nodes in captures.items() for node in nodes` (capture NAME string, never id-resolved). Drift string compare: `evidence.md:241` — `fact.capture == "name" and fact.text in canonical`; `_cross` filter `evidence.md:248-250`.
- `run` bounds: `QueryCursor(..., match_limit=budget)` `evidence.md:168`; `descendant_count` pre-gate `evidence.md:185-189`; `did_exceed_match_limit` grade `evidence.md:191,198-201`; `progress_callback=lambda step: step < budget` `evidence.md:190`; scoping `set_max_start_depth`/`set_byte_range` `evidence.md:169-172`.
- `ApiCatalogue.reflect` (entry-points-only): `evidence.md:253-262` — `@trapped("reflect", catch=ImportError)`, body yields `MemberFact(distribution, version, import_name, ep.group, ep.name)` per `dist.entry_points` plus one `("import", import_name)` cons row. No importable-member reflection.
- `flatten` re-mints identity: `evidence.md:233` — `flatten: Callable[...] = lambda nested: nested.collect(lambda out: out)` (sibling uses `.collect(identity)` at `reproduction.md:171`).
- Banned appendix present: `evidence.md:284-286` — `## [03]-[RESEARCH]`.
- Single scan span, no corpus-parent: `scan` `evidence.md:225-236` maps `run` across corpus with no wrapping span; per-file `code.query` span opened inside `run` `evidence.md:174`.

## [02]-[SEAM OWNER ANCHORS] (verified imports legal)

- `faults.md:219-237` `trapped[**P, T](subject, *, catch=Exception)` decorator — matches `@trapped("reflect", catch=ImportError)`.
- `faults.md:150` `CLASSIFY` row `(ImportError, lambda s,c: BoundaryFault(import_=(s, type(c).__name__)))` — `PackageNotFoundError` is `ImportError` subclass, routes to `import_`.
- `faults.md:86-91` `BoundaryFault` cases: `deadline: tuple[str, float, str]` (subject,budget,cause) — matches `BoundaryFault(deadline=(f"{probe}:{lang}", float(budget), "descendant-count"))` `evidence.md:187`; `resource: tuple[str, str]` — matches `BoundaryFault(resource=(f"{probe}:{lang}", "match-limit"))` `evidence.md:199`; `import_: tuple[str, str]`.
- `faults.md:240-262` `traversed[T](rails, *, by=Disposition.ABORT)` with `@overload` per `Disposition` `Literal`; `evidence.md:205-236` `scan` mirrors it.
- `faults.md:62-65` `Disposition = ABORT/ACCUMULATE/PARTITION`.
- SCOPES/Scope owner (the underutilized capability): `faults.md:68-76` `class Scope(StrEnum)` includes `EVIDENCE = "evidence"`; `faults.md:172-180` `SCOPES: Final[Map[Scope, str]]` includes `(Scope.EVIDENCE, "rasm.runtime.evidence")`. Deleted-form law: `faults.md:35` — "a per-page tracer/meter/service literal (`trace.get_tracer("<literal>")`) beside the `SCOPES` row the consumer's `Scope` member already carries". Sibling correct usage: `identity.md:149` — `_TRACER: Final[trace.Tracer] = trace.get_tracer(SCOPES[Scope.IDENTITY])`.
- `receipts.md:151-168` `Receipt` union + `Receipt.of(owner, evidence)`; `(phase, subject, facts)` tuple → `fact` case `receipts.md:165-166` — matches `Receipt.of(self.owner, ("emitted", "code.scan", facts))` `evidence.md:281`. `ReceiptContributor` Protocol `receipts.md:183-185` — matches `EvidenceScan.contribute` `evidence.md:272-281`.

## [03]-[BRIEF ANCHORS] RASM-PY-RUNTIME-BRIEF.md

- DAG order (import legality): `:40` — `< {reproduction, evidence, roots, shapes} < wire < lanes < recipe < serve` (evidence downstream of faults/identity/receipts).
- Charter: `:107-109` `[V10_EVIDENCE_CHARTER]` — evidence.md STAYS in runtime, dev-tooling evidence owner feeding external `assay code`/`api`.
- Verdict row (three named deltas): `:162` — `evidence/evidence | 7 | 9 | honest tooling charter; member-level reflect; integer-id hot loop; RESEARCH purge`.
- Coverage gap E12: `:144` — "`reflect` entry-points-only `evidence.md:258-267` vs `ARCH:27`".
- RESEARCH ban: `:55` — "the `[RESEARCH]` appendix is BANNED — ten pages carry it (... `evidence.md:284` ...)".
- tree-sitter package rider (names the unmined members): `:186` — "`tree-sitter` — `Language.id_for_node_kind`/`field_id_for_name` at registry build; `TreeCursor` for hot walks per the catalog's own traversal law".

## [04]-[.API MEMBER ANCHORS] (cited buildout capabilities)

`runtime/.api/tree-sitter.md`:
- `id_for_node_kind`/`field_id_for_name` (integer-id resolution): `tree-sitter.md:121` (`node_kind_for_id`/`id_for_node_kind`), `:123` (`field_name_for_id`/`field_id_for_name`). LOCAL_ADMISSION law: `:142` — "Capture-name and node-kind strings are resolved to ids against the `Language` introspection surface once at registry build, so the hot match loop compares integers, not strings."
- `disable_capture`/`disable_pattern`: `tree-sitter.md:109` — "`Query.disable_capture(name)` / `disable_pattern(i)` | tune | suppress captures/patterns before running".
- `QueryPredicate` handler: `tree-sitter.md:41,110` — custom `#...?` predicate handler passed to `matches`/`captures`.
- `Node.has_error`/`is_error`/`is_missing` (parse-error evidence): `tree-sitter.md:77` and recovery law `:138` — "malformed-input handling reads `has_error`/`is_error`/`is_missing` on the tree, not an exception — `parse` always returns a `Tree`".
- Incremental parse: `tree-sitter.md:60` (`Tree.edit`/`changed_ranges`), edit law `:136` — "`changed_ranges(new_tree)` drives downstream re-processing"; `old_tree` on `parse` `:54`.
- `TreeCursor`: `tree-sitter.md:82-92`, traversal law `:133`.
- `supertypes`/`subtypes`: `tree-sitter.md:124`.

Installed-surface confirmation (`tree_sitter/__init__.pyi`): `supertypes` :40; `id_for_node_kind` :43; `field_id_for_name` :48; `has_error` :79; `descendant_count` :119; `edit` :123,:188; `changed_ranges` :198; `disable_capture` :324; `disable_pattern` :325; `did_exceed_match_limit` :355.

`runtime/.api/tree-sitter-typescript.md`:
- `LOCALS_QUERY` (TS/TSX-only bundled source, the natural partial-coverage probe): `:27` — "`LOCALS_QUERY` | query source | `Final[str]` scope/binding-resolution `.scm` source text"; `:22` — "The TypeScript grammar additionally ships a `LOCALS_QUERY` ... which the Python grammar does not." Installed confirm: `tree_sitter_typescript.LOCALS_QUERY` resolves (len 104); `locals.scm` present under package `queries/`.

`runtime/.api/tree-sitter-python.md`:
- No `LOCALS_QUERY`: `:22` — "The Python grammar ships highlights and tags only — there is no `LOCALS_QUERY` (the TypeScript grammar does ship one)." (This is exactly the `lang in sources` partial-coverage case `CompiledProbe.of` `evidence.md:131` filters.)

`.api/expression.md`:
- `identity(value)`: `:98` — "`identity(value)` | identity fn | passthrough". Installed confirm: `from expression import identity` live.
- `Block.collect`/`choose`/`filter`/`fold`/`map`/`of_seq`/`cons`: `:141-143`. `Map.of_seq`/`empty`/`filter`/`map`/`change`: `:145-148`. `Option.default_value`: `:113`.

`.api/msgspec.md`:
- `Struct` + `gc=False` leaf law: `:146`, `:162`. `Meta` constraint: `:102`, `:147`. `to_builtins(struct, str_keys=True)` → Span attribute mapping (otel stack): `:157`.

`.api/opentelemetry-api.md`:
- `trace.get_tracer` :83; `start_as_current_span` :90; `set_attributes` :92; `is_recording` :98; `set_status` :95; `Status`/`StatusCode` :26-27; attribute value law `:160` (`str|bool|int|float|Sequence[...]`). `Span.add_event` :93 (unused; per-fault/per-file event option).

## [05]-[FOLDER CONTEXT ANCHORS]

- `runtime/README.md:18-20` index rows: `[12]-[IDENTITY]`, `[13]-[REPRODUCTION]`, `[14]-[EVIDENCE]`. Package rows `:53-55` — `tree-sitter`, `tree-sitter-python`, `tree-sitter-typescript`.
- `runtime/ARCHITECTURE.md:29` — `evidence.py # ApiPackage/ApiMember reflection and Structural tree-sitter queries` (ARCH names `ApiMember` — the member-level surface E12/`:144` flags `reflect` as under-covering). Seams `:52` — `evidence ← python:geometry/mesh [CONTENT_KEY]`.
- `watchfiles.md` present in `runtime/.api/` (folder admits a file-watch loop — the consumer context for an incremental-parse scan mode).
