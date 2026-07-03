# [CONTRACTS_CORPUS]

`tests/contracts/` is the cross-language frozen corpus: the wire bytes and canonical JSON that prove every polyglot seam decodes to the same facts in C#, Python, and TypeScript. The corpus is a neutral seam — it belongs to no single language tree, and no language-local snapshot store substitutes for it. Corpus assets are rebuilt ground-up whenever the owning wire contract changes shape; stale assets are regenerated or deleted, never patched by hand or aliased for old consumers.

## [01]-[PRODUCER_CONSUMER]

C# is the sole producer: corpus assets are emitted by the owning C# wire surface, never authored by hand and never written by Python or TypeScript. Consumers are round-trip read-only — each consumer decodes an asset, re-encodes it, and proves equivalence through its language snapshot rail (`Verify` in C#, `inline-snapshot` in Python, `toMatchFileSnapshot` in TypeScript): wire bytes re-encode byte-identical, and the canonical JSON projection re-encodes to the same canonical facts. A consumer that cannot round-trip an asset has found a seam defect in its decoder or in the producer, never a reason to fork a language-local variant of the asset.

## [02]-[LAYOUT]

Assets subdivide by seam, then by message: one directory per cross-language seam, one asset pair (wire bytes plus canonical JSON) per message shape inside it. A seam directory exists only when a real producer emits into it — no reserved directories, no placeholder assets, no speculative message homes.

```text conceptual
tests/contracts/
└── <seam>/
    ├── <message>.bin    # frozen wire bytes, producer-emitted
    └── <message>.json   # canonical JSON projection of the same payload
```

Peer folders beside the seam directories — descriptor-set snapshots, exported schemas, or other contract assets — land the day they become real, never in advance.

## [03]-[REGENERATION]

Regeneration triggers when the owning C# wire contract changes: the producer re-emits the affected seam's assets, and every consumer round-trip re-proves in the same change. `buf breaking` (FILE category, against `main`) is the required proto gate from the day the first `.proto` lands. A regenerated asset that breaks a consumer round-trip blocks the change until the seam is reconciled on both ends; committing the new bytes while a consumer still decodes the old shape is a corpus corruption, not a migration strategy.
