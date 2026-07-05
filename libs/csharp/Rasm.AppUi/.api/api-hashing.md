# [RASM_APPUI_API_HASHING]

AppUi-delta overlay over the shared-tier `libs/csharp/.api/api-hashing.md` canonical (the full `System.IO.Hashing` surface — the `NonCryptographicHashAlgorithm` accumulator base, `XxHash3`/`XxHash64`/`XxHash128`/`XxHash32`, `Crc32`/`Crc64`, and the one-shot / incremental / stream-sink call shapes). This page states ONLY the AppUi admission delta; it re-documents no member.

## [01]-[ONE_HASHER_LAW]

[MINT_IS_KERNEL]:
- Every AppUi content-identity MINT composes the kernel `Rasm.Domain` `ContentHash.Of(ReadOnlySpan<byte>) -> UInt128` seed-zero entry — the federation one-hasher (`Rasm/.planning/Domain/identity.md`, the `Rasm/ARCHITECTURE.md:87-92` one-hasher law): the capture runtime `ContentHash` delegate binding and the walkthrough per-frame proof, the command payload digest, the notebook replay-input hashes, and the `Collab/sync.md` snapshot-accelerator key all resolve through it.
- An AppUi-local `System.IO.Hashing` `XxHash128.Hash`/`HashToUInt128` MINT call site is the DELETED form — it forks the federation seed and breaks the TS/python seed-row reproduction the one-hasher law demands. Hex/wire encodings (`:x32`) stay boundary projections at the consuming seam, never a second mint.

[DECODE_ONLY_ADMISSION]:
- The package's sole AppUi admission is DECODE-only payload-key reads: a Compute-minted `ResidencyPayload.ContentKey` / splat-tile key arrives already content-addressed (`Render/pipeline.md`, `Render/meshlets.md`, `Render/reality.md` per `[V5]`), and AppUi keys tiles by that payload key — it never re-hashes the decoded component floats (`reality.md`'s local `XxHash128` re-hash is the deleted form).
- Reject: an AppUi-side content-key fold beside `ContentHash.Of`; a per-page `XxHash128` mint for cache/receipt identity where the kernel entry owns it; treating the hash base as a `Stream` (it is NOT — the shared canonical documents the `Append`/`GetCurrentHash` accumulator surface, no public `Write`).
