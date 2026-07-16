# [CONTRACTS_CORPUS]

`tests/contracts/` is the cross-language frozen corpus: the wire bytes and canonical JSON that prove every polyglot seam decodes to the same facts in C#, Python, and TypeScript. This corpus is a neutral seam — it belongs to no single language tree, and no language-local snapshot store substitutes for it. Corpus assets are rebuilt ground-up whenever the owning wire contract changes shape; stale assets are regenerated or deleted, never patched by hand or aliased for old consumers. [MANIFEST.md](MANIFEST.md) is the corpus registry: every committed seam holds one manifest entry before its first asset exists, and the entry's pin state separates frozen expectations from unpinned design gaps.

## [01]-[PRODUCER_CONSUMER]

C# is the sole producer: corpus assets are emitted by the owning C# wire surface, never authored by hand and never written by Python or TypeScript. Consumers are round-trip read-only — each consumer decodes an asset, re-encodes it, and proves equivalence through its language snapshot rail (`Verify` in C#, `inline-snapshot` in Python, `toMatchFileSnapshot` in TypeScript): wire bytes re-encode byte-identical, and the canonical JSON projection re-encodes to the same canonical facts. A consumer that cannot round-trip an asset has found a seam defect in its decoder or in the producer, never a reason to fork a language-local variant of the asset.

## [02]-[LAYOUT]

Assets subdivide by seam, then by message: one directory per cross-language seam, one asset pair (wire bytes plus canonical JSON) per message shape inside it. A seam directory exists only when a real producer emits into it — no reserved directories, no placeholder assets, no speculative message homes. `MANIFEST.md` precedes every directory: a seam is committed by its manifest entry, and its directory appears the day its producer emits.

```text conceptual
tests/contracts/
├── MANIFEST.md          # the seam registry: pin states, producers, frozen expectations
└── <seam>/
    ├── <message>.bin    # frozen wire bytes, producer-emitted
    └── <message>.json   # canonical JSON projection of the same payload
```

Peer assets beside the seam directories — descriptor-set snapshots, exported schemas, or other contract assets — land the day they become real, never in advance, and each is registered as a manifest entry with its own payload kind.

## [03]-[MANIFEST]

[MANIFEST.md](MANIFEST.md) is machine-consumed Markdown: corpus audits verify it against disk, producer pages flip its pin states, and per-language corpus readers resolve assets through it. It keeps this exact shape instead of ordinary page normalization:

[MACHINE_RECORD]:
- Consumer: corpus audits (producer-anchor resolution, pin-state honesty), producers (pin-state graduation), per-language corpus readers (asset resolution by seam and fixture).
- Required shape: one summary lookup table over all entries, then one H3 record per fixture carrying the field grammar below in field order.
- Checked fields: `Producer` anchors resolve to a real page and cluster on disk; `Pin` comes from the closed vocabulary; a `DESIGN-PIN` entry carries `Blocker` and no `Expectation`; a `REAL` entry carries `Expectation` and no `Blocker`; `Payload` comes from the closed vocabulary.
- Owner: this README owns the schema; the manifest owns the instances.
- Refresh trigger: any seam commitment, pin graduation, producer re-anchor, or payload change lands with its manifest entry in the same change.

[ENTRY_SCHEMA]:
- `Seam`: names the corpus directory the fixture's assets land in, lowercase-hyphenated.
- `Producer`: names the owning C# page cluster as `csharp:<page>#<CLUSTER>` — the surface that pins the byte-deriving input and emits the asset. A fixture family with several descriptor sources lists one anchor per source; the sole-producer law holds for each.
- `Consumers`: lists the committed round-trip readers as `lang:<pkg>/<page>#<CLUSTER>` or folder tokens; consumers never re-derive a fixture.
- `Payload`: one or more of `wire-bytes`, `canonical-json`, `digest`, `descriptor-set`.
- `Pin`: `REAL` when the byte-deriving input and expected values are frozen on the producer page — host-derived or deterministically derivable from a settled design; `DESIGN-PIN` when the producer has not pinned the byte-deriving input.
- `Blocker`: `DESIGN-PIN` only — the named producer gap that must close before bytes can derive.
- `Shape`: states the committed payload shape and the law the fixture proves.
- `Expectation`: `REAL` only — the frozen values the producer emit must reproduce.
- `Regenerate when`: names the owning contract change that forces re-emission.

[EXPECTATION_LAW]:
- An `Expectation` field carries committed law data — the value the producer must reproduce byte-for-byte — never a corpus asset. Corpus assets exist only as producer-emitted files under `<seam>/`; a ledger value never substitutes for the emitted asset, and a consumer round-trips the asset, not the ledger.
- A `DESIGN-PIN` entry carries no byte set, no digest, and no fabricated stand-in. Its bytes derive only after the named producer pins the missing input; a fabricated byte set for an unpinned fixture is the rejected form in every runtime.
- Graduation from `DESIGN-PIN` to `REAL` happens on the producer page first: the producer freezes its byte-deriving input there, then flips the manifest entry in the same change. Manifest entries never lead the producer.

## [04]-[REGENERATION]

Regeneration triggers when the owning C# wire contract changes: the producer re-emits the affected seam's assets, updates the manifest entry, and every consumer round-trip re-proves in the same change. `buf breaking` (FILE category, against `main`) is the required proto gate from the day the first `.proto` lands. A regenerated asset that breaks a consumer round-trip blocks the change until the seam is reconciled on both ends; committing the new bytes while a consumer still decodes the old shape is a corpus corruption, not a migration strategy.
