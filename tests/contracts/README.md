# [CONTRACTS_CORPUS]

`tests/contracts/` carries both corpus roles: each seam schema DEFINES the contract every conforming branch implements, and the frozen wire bytes and canonical JSON PROVE that C#, Python, and TypeScript resolve it to the same facts — a neutral surface no language tree owns and no language-local snapshot store substitutes for. Definitions and assets rebuild ground-up when a contract changes shape; a stale asset regenerates or dies, never hand-patched.

[MANIFEST.md](MANIFEST.md) is the corpus registry: every committed seam holds one manifest entry before its first asset exists, and the entry's pin state separates frozen expectations from unpinned design gaps.

## [01]-[AUTHORITY]

Each entry declares one of two classes, and the class alone decides who mints the shape, what proves conformance, and which drift is the defect.

[INFRASTRUCTURE]:
- Mint: every branch anchor the entry's `Minters` field names mints the shape from its own inputs; the seam schema is the definition, and no branch is another's source or prerequisite, and the entry freezes the payload-agnostic law alone — each branch-owned payload instantiating it takes its own `domain` entry at its own seam.
- Proof: each minter emits into the seam and every other minter reproduces the identical facts on decode, so parity across minters IS the conformance.
- Drift defect: a second mint of one shape inside one branch — that branch then carries two spellings where the corpus proves one.

[DOMAIN]:
- Mint: the single `Producer` anchor emits the canonical asset, named by the domain capability it holds and never by language rank.
- Proof: every participating branch decodes, re-encodes, and proves equivalence through its local snapshot rail; wire bytes round-trip byte-identically, and canonical JSON re-encodes to the same facts.
- Drift defect: a second producer for one contract — two emitters fork the semantic model the contract carries.

## [02]-[LAYOUT]

Assets subdivide by seam, then by message: one directory per cross-language seam, one definition asset at its head, one asset pair (wire bytes and canonical JSON) per message shape inside it. Each seam directory opens with its `contract.schema.json` and gains frozen assets the day a minter or producer emits — no reserved directories, no placeholder assets, no speculative message homes. `MANIFEST.md` precedes every directory: the entry commits the seam, the directory lands the day its definition does, and the registry is the census a reader walks. Everything below states that landing schema, never the current tree.

```text conceptual
tests/contracts/
├── MANIFEST.md                    # Seam registry: classes, pin states, minters, frozen expectations
├── .api/                          # Corpus tool catalog: the gate binary's commands, config schema, rule vocabulary
├── rasm/<family>/v1/              # Descriptor source per family, seated at the directory its package spells
│   ├── <family>.proto             # Language-neutral message and service declaration
│   └── <family>.descriptor.binpb  # Frozen FileDescriptorSet parity digest
└── <seam>/
    ├── contract.schema.json       # Definition every conforming branch implements
    ├── <message>.bin              # Frozen wire bytes
    └── <message>.json             # Canonical JSON projection of the same payload
```

Peer assets beside the seam directories — descriptor-set snapshots, exported schemas, or other contract assets — land the day they become real, never in advance, and each registers a manifest entry with its own payload kind. A descriptor source and its `FileDescriptorSet` snapshot land together as one peer asset: the snapshot is the drift contract's per-source parity digest, never a seam fixture, so it rides a `DESIGN-PIN` entry without breaching the pin law.

[PROTO_SEATING]: `buf` `PACKAGE_DIRECTORY_MATCH` binds each source to the directory its package spells, so `rasm.<family>.v1` seats at `rasm/<family>/v1/<family>.proto` and one directory holds one package. Every later family inherits that seat rather than re-deciding it, and the corpus root gains a directory per family instead of a flat file set three packages deep.

[SHARED_DEFINITION]: definitions several seams conform to land ONCE beside the seam directories as `<vocabulary>.schema.json`, and each referencing seam's `contract.schema.json` reaches one by `$ref` rather than restating a row. Such a definition registers no manifest entry of its own — a definition names no minter and emits no asset, so the entry schema's exclusive maps have nothing to bind — and every entry conforming to it names it in `Shape`. Rosters ride it as `const` subschemas so a branch validates its own projection against the frozen vocabulary instead of a reader comparing two tables by eye. Two seams re-spelling one roster is the fork this form forecloses; the shared definition freezes before either seam's own definition lands, because a seam schema that fixes a shared row locally has already forked it.

## [03]-[MANIFEST]

[MANIFEST.md](MANIFEST.md) is machine-consumed Markdown: corpus audits verify it against disk, producer pages flip its pin states, and per-language corpus readers resolve assets through it. It keeps this exact shape instead of ordinary page normalization:

[MACHINE_RECORD]:
- Consumer: audits resolve producer anchors and verify pin-state honesty; producers graduate pins; branch readers resolve assets by seam and fixture.
- Required shape: one summary lookup table over all entries, then one H3 record per fixture carrying the field grammar below in field order.
- Checked fields: `Class`/`Pin`/`Payload` use closed terms; every `Minters` and `Producer` anchor resolves on disk; exclusive maps: `infrastructure`→`Minters`, `domain`→`Producer`, `DESIGN-PIN`→`Blocker`, `REAL`→`Expectation`.
- Owner: this README owns the schema; the manifest owns the instances.
- Refresh trigger: any seam commitment, pin graduation, producer re-anchor, or payload change lands with its manifest entry in the same change.

[ENTRY_SCHEMA]:
- `Seam`: names the corpus directory the fixture's assets land in, lowercase-hyphenated.
- `Class`: `infrastructure` binds every branch to mint the shape; `domain` binds one producer to emit it.
- `Minters`: `infrastructure` only — every branch anchor minting the shape, as `lang:<pkg>/<page>#<CLUSTER>`; each mints from its own inputs.
- `Producer`: `domain` only — names the owning page cluster as `lang:<pkg>/<page>#<CLUSTER>`; that surface pins the byte-deriving input and emits the asset.
- `Consumers`: lists the committed round-trip readers as `lang:<pkg>/<page>#<CLUSTER>` or folder tokens; consumers never re-derive a fixture.
- `Payload`: one or more of `wire-bytes`, `canonical-json`, `digest`, `descriptor-set`.
- `Pin`: `REAL` binds producer-page-frozen byte input and expectations, host-derived or settled-design-determined; `DESIGN-PIN` marks input unpinned.
- `Blocker`: `DESIGN-PIN` only — the named producer gap that must close before bytes can derive.
- `Shape`: states the committed payload shape and the law the fixture proves.
- `Expectation`: `REAL` only — the frozen values the producer emit must reproduce.
- `Regenerate when`: names the owning contract change that forces re-emission.

[EXPECTATION_LAW]:
- `Expectation` binds byte-exact emitting law, never an asset; consumers round-trip only emitted `<seam>/` assets, never ledger values.
- `DESIGN-PIN` carries no bytes, digest, or stand-in; every minter or the producer pins the missing input before bytes derive; all runtimes reject fabricated bytes.
- `DESIGN-PIN` graduates to `REAL` only after every minter — or the single producer — freezes its byte-deriving input; the manifest entry follows in the same change.

## [04]-[REGENERATION]

Contract changes trigger regeneration by class: an `infrastructure` schema change re-proves every minter against the new definition, and a `domain` contract change re-emits from its producer. Either path updates the manifest in the same change, and a regenerated asset lands only with every participating binding reconciled.

[PROTO_GATE]: the root `buf.yaml` declares this directory as buf's one module, and every verb runs from `node_modules/.bin/buf`. `buf lint` holds each source to `STANDARD` less the two rules the estate's ruled wire vocabulary displaces; `buf breaking --against '.git#branch=main' --against-config buf.yaml` refuses a `FILE`-category change, reading the baseline under the current rules so a rule edit takes effect on the same run; `buf format -d --exit-code` gates layout on the diff, the bare `--exit-code` form printing each formatted file to stdout instead.

Each source regenerates its snapshot through `buf build --path <source> --as-file-descriptor-set --exclude-imports --exclude-source-info`, the one flag set the three minters compare byte-for-byte. That same exclusion keeps the snapshot out of the breaking baseline — it resolves no imports — and `tests/contracts/.api/bufbuild-buf.md` owns the command, config, and rule surface.
