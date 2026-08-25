# [CONTRACTS_RULINGS]

`contracts` rulings settle wire-corpus, registry, and emission decisions.

## [01]-[PACKAGES]

- `Google.Protobuf`, `Grpc.Core.Api`, `Google.Api.CommonProtos`, `Celly.Protovalidate` are the whole C# closure — `RasmGenerated` clears the rest.
- `protobuf-py` and `connectrpc` are the whole Python closure — validation and custody runtimes home at `python:runtime`, never in the wheel.
- `@bufbuild/protobuf` is the whole TypeScript direct closure — every `_pb.ts` module imports its generated-code runtime and no branch folder.
- Local emitters pin with their runtime — `protoc-gen-py`/`protobuf-py`, `protoc-gen-connectrpc`/`connectrpc`, `protoc-gen-es`/`@bufbuild/protobuf`.
- `protocolbuffers/csharp` and `grpc/csharp` run as BSR remote plugins pinned by version and exact revision — no C# generator rides the machine PATH.
- Foreign shapes a lane reaches as a package-shipped client bind through that client — a vendored copy forks the publisher that versions it.
- Vendored generation lands only where a real consumer has no shipped client — `grpc.health.v1` emits for Python alone; other lanes omit it.
- Vendored modules curate per lane as `types:` rows on its emission row, on two grounds alone — a namespace or stdlib collision, or a shipped type.
- `CloudEventsAvro` adds no dependency or export row — a readonly JSON module the `./*` wildcard already resolves.
- Each emission publishes as its own distribution — `Rasm.Contracts`, `rasm-contracts`, `@rasm/contracts` — so a foreign consumer installs no branch.

## [02]-[SHAPE]

- Proto packages spell `rasm.contracts.<family>` and managed mode derives every language option — an inline `csharp_namespace` forks it.
- Proto messages keep their plain name and mint `Wire` only on a co-resident domain collision — `element.proto` suffixes all, `compute.proto` none.
- RPCs bind their payload directly under the lint-required name — an envelope seats only where a second field embeds that payload, stated on site.
- Producer-refused invariants ship as contract LAW, never a wire column — a constant column hands every consumer a knob to renormalize against.
- Estate surfaces spell no version segment — proto packages, event types, and storage keys reshape in place with every consumer updated same-change.
- Proto3 syntax stays until Protobuf-ES preserves optional presence under Editions — cross-language absence outranks edition novelty.
- Protovalidate rules own wire constraints in estate sources; branch admission evaluates them at every decode and emit boundary, repeating none.
- Lane generation selects public roots from real consumers; transitive imports form support closure and never widen the public roster.
- `TELEMETRY_CONVENTION` digests projected row VALUES and absence is one — a stated ceiling parity-proves, so no plane is exempted.
- Mounted instrument rosters, emitter rosters, and SDK member spellings stay branch-owned — each crosses no digest and gates at its own branch.
- One C# assembly carries messages, service bases, and clients — every consumer reads one descriptor graph, and a split forks it.
- `<Nullable>disable</Nullable>` states protoc's nullable-oblivious emission — consumer admission owns message presence.
- Workspace and external consumption resolve one emission identity — `ProjectReference` and `PackageReference` build the same packable project.
- `io=async` emits the Python service family — the branch serves ASGI and dials under anyio, so no sync stub family stands beside it.
- `rasm.contracts` imports spell package and proto path from ONE root — `rasm.contracts.rasm.contracts.<f>.<s>_pb`, vendored trees included.
- `rasm/` and `rasm/contracts/` stay PEP 420 portions under `init_files=false` — no `__init__.py` enters the sweep; `py.typed` is gate-projected.
- One `./*` export owns every TypeScript specifier — the workspace resolves `gen/typescript/*.ts` and `publishConfig` swaps the tarball to `dist`.
- Workspace targets stay bare specifiers — a condition serving `gen` and `dist` at once needs a custom condition every consumer tsconfig declares.
- `valid_types=protovalidate_required` emits `<Name>Valid` and binds it to each `GenMessage` descriptor.
- Publisher descriptors remain direct module inputs — `CloudEventSchema` stays outside estate registries.
- Publisher assets project verbatim per lane — `cloudevents_avro.ts`, `cloudevents.avsc` — consumers compile or parse them, never a transcription.
- Seam kinds stay the corpus-closed vocabulary — corpus-to-emission edges are `[CONTRACT]`, publisher-asset crossings `[EVENT]`; no asset kind mints.

## [03]-[COLLAPSE]

- Generated messages are the one wire vocabulary in every lane — a hand twin, partial, or mirror splits descriptor and validation authority.
- `Rasm.Contracts` stays a generated-only assembly — workspace substrate injection makes the wire floor depend on unrelated libraries.

## [04]-[STRUCTURE]

- `gen/` is the whole created-vs-generated split — every file beneath it, projected `py.typed` and `.avsc` included, dies on the next `generate`.
- Out roots exist the moment a plugin row writes them — a new emission target is one `gen/<target>/` out and one plugin row, never a `.gitkeep`.
- `.api/` carries one catalog per emission target beside the driver tool's `bufbuild-buf.md` — a new `gen/<target>/` lands its own catalog.
- Catalog roster blocks between the gate's markers derive from the image and each lane's roots and closure; the grammar around them stays hand-kept.
- Build outputs ride `.artifacts/<lang>/` for .NET and Python and `dist/` for TypeScript — `pyproject.toml` carries no `dist/` default.
- Raw `gen/typescript/**` never enters the tarball — `tsc --build` emits ESM and declarations into `dist`, its build state riding `.cache/`.
- NuGet output carries the README, XML documentation, and a `.snupkg` embedding the generated source — no source mirror is packaged.
- `Rasm.Contracts.csproj` sets `EnableDefaultItems` off and compiles `gen/dotnet/**/*.cs` alone — the SDK default globs walk the estate root.
- ONE nx project name pins on both `package.json` `nx.name` and the csproj `<Nx><Name>` — nx merges projects by root under the later name.
- `conformance/<seam>/` holds proof VECTORS alone, keyed by entry id — a directory materializes only when a verified case carries assets.
- Vendored protos are their OWN unnamed buf modules outside the format path — `buf format` names `proto/` alone, keeping publisher bytes identical.
- Vendored modules failing `STANDARD` carve NAMED rules in their own block restating `use: [STANDARD]`; one clearing it whole carries no block.
- `disable_builtin` never stands in for a carve — it grades a module against nothing and WARNs on every run while the exit code reads clean.
- Definitions and corpora no branch reaches as client surface seat under `vendor/` as frozen publisher bytes — a transcription grades its own copy.
- `manifest.json` seats a contract only where a peer PROCESS decodes it, same-language included — a single-process golden homes at its branch tests.
- Process separation seats a same-language crossing from both branch fences; one process keeps its seam branch-local.
- Actor coordinates and message names match the anchored cluster body; renaming a fence re-lands its registry case in the same pass.
- Gate-derived projections alone carry a schema, earned by a verified document its consumer evaluates; a hand-authored one defines no seam.
- Estate protos carry in-situ field law and `// --- [SECTION]` banners alone — narrative, drift, or snapshot prose restates a moving owner.
- Module documentation seats `buf.md` at the estate module root — absent it the BSR renders the repo README to every external consumer.
- `manifest.json` is the ONE federation index, readiness authority, and fixture registry — no second wire-fixture roster stands beside it.
- BSR generated SDKs enter no lane — that pipeline carries no type filter and fixes `opt` at the plugin, so roots widen and emission flags vanish.
- Publication opens generated SDKs to foreign consumers alone — vendored modules stay unnamed, so no SDK ever carries a lane's whole emission.
- `[02]-[STRATA]` ranks the corpus descriptor DAG, not the estate — emissions hold no rank here; each branch seats its import root at its own tier.
- Router cards index one catalog and one emission root per lane — generation authors every module, so no design page exists for a card to reach.
- Workspace consumers reach an emission by dependency — `workspace:*`, `ProjectReference`, the uv member — never by a path import into `gen/`.
- `admission.py` and `artifact.py` do NOT re-enter — `python:runtime/transport/body` and `python:runtime/transport/artifact` own them.

## [05]-[PROCESS]

- `buf` runs from the repo root against the `libs/contracts` input through one `buf.gen.yaml` — a build-driven protoc or branch template forks it.
- `buf.gen.yaml` `out:` rows are repo-root relative and `buf.yaml` module paths buf.yaml-relative — a verb run from the folder re-roots every out.
- `assay contracts generate` authors every tree, projection, and roster row together — corpus and generator changes land by full regeneration alone.
- Assay's gated publish rail alone proves publication — descriptor equality and the `main`-label pin settle custody; local snapshots settle nothing.
- One `main` label carries the RELEASED stream and publication re-pins it — a lock shields resolved consumers alone, so an unready commit waits.
- `contracts publish` alone admits exact module absence, re-proved just before bootstrap; a missing label, auth, or network fault never bootstraps.
- `pnpm --filter @rasm/contracts pack` proves the tarball after canonical generation, and no configuration re-derives `files` or `publishConfig`.
- `files` ships `dist` and `README.md` alone, and the gate proves every non-built row on disk — a manifest row cannot outlive its file.
- Every non-publisher case names a literal reachable reader — producers, minters, descriptor slots, and equivalence helpers alone are not crossings.
- Application authority never covers a missing estate producer — an application or external client owns the inbound value, or the class is wrong.
- Verification atomically replaces blockers with vectors — `blocked` carries no vector, `verified` one oracle; blocked evidence makes no directory.
- Protobuf proves decoded semantic roundtrip and unknown-field rejection, never cross-runtime deterministic byte identity.
- Exact byte identity survives only as `external-digest` for byte-defined external formats and `publisher-digest` for immutable custody.
- Semantic conformance and value parity compare typed expected facts — untyped dictionaries and positional asset conventions prove nothing.
- Value parity carries exactly one independently minted specimen per declared minter and derives provenance with `actor_key(actor)`.
- `manifest.schema.json` derives byte-for-byte from the assay msgspec model — cross-field laws stay in the audit, never hand-authored conditionals.
- Generated actors name `supports` for descriptor symbols their boundary uses outside ordinary closure — a support selects codegen, never a crossing.
- Multiple entrypoints in one fence register independently by `actor_key(actor)` — a shared anchor does not collapse byte, socket, or RPC ingress.
