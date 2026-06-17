# [INTERCHANGE_IDEAS]

The forward pool of higher-order concepts for the wire boundary. Each idea is a card — a bracketed slug leader, the capability, what it unlocks, and the gap or modern technique it draws on. An idea drives one or more `TASKLOG.md` tasks; a finished or dropped idea moves to `[2]-[CLOSED]` with a one-line disposition.

## [1]-[OPEN]

[PROTOCOL_SELECTION_AXIS]: the single hardwired browser transport collapses into one polymorphic transport owner over a protocol-selection axis.
- Routes long-lived server-stream legs over the Connect protocol (`createConnectTransport`, standard HTTP, trailer-free) and the binary-frame legs over gRPC-Web where the backend dictates, the choice one `TransportProtocol` case.
- Escapes the roughly-60s gRPC-Web server-stream cap throttling `generate`, `subtreeFetch`, `progress`, the health watch, and `documentEvents`; gains DevTools-debuggable HTTP and a single seam where a future WebTransport leg is one more protocol case.
- Connect-ES v2 ships `createConnectTransport` alongside `createGrpcWebTransport`; the prior design hardwired the gRPC-Web factory and inherited the HTTP-1 server-stream cap. A server-enforced deadline arrives as `Code.DeadlineExceeded` on the `ConnectError` distinct from the `Code.Canceled` a client `AbortError`/`TimeoutError` yields, so the fault rail keys the deadline leg by the connect `Code`.

[STRUCTURED_DRIFT_REPORT]: the opaque quarantine `Stale` marker becomes a structured drift-report driving the contract-drift classifier.
- Builds a `DriftReport` from the Effect Schema `ArrayFormatter` error paths, feeds the Identical/Additive/Breaking classifier, and surfaces a Breaking drift as a typed `FaultDetail.Quarantine` carrying the exact failing field path.
- Turns silent quarantine into actionable drift evidence the UI renders and the platform telemetry-ships; an additive change is observable and a breaking change carries its exact failing path.
- `ArrayFormatter`/`TreeFormatter` give structured error paths; the prior fold discarded the `ParseError` into an opaque `Stale` marker, losing every diagnostic the untrusted-ingress charter needs.

[DECODE_BUDGET_REFINEMENT]: the refinement vocabulary gains decode-budget rows hardening the untrusted-ingress boundary.
- Adds `maxDepth`, `maxFrames`, and `maxAssembledBytes` budgets enforced as `Schema.filter` and `Stream` bounds, plus an `isomorphic-dompurify` sanitization row for every DOM-bound text field, each one typed row.
- Hardens the boundary against JSON-bomb, unbounded-recursion, and unbounded-frame-stream DoS and mXSS, making the quarantine charter a real security owner.
- Deeply-nested-payload recursion exhaustion and frame-stream flooding are known untrusted-ingress classes; the prior refinement enforced identity brands and envelope bounds but carried no ingress budget or DOM-bound sanitization despite the charter.

[TRANSFERABLE_STREAM_REASSEMBLY]: the artifact-frame reassembly becomes a transferable-stream worker boundary.
- Lifts the Connect server-stream into an Effect `Stream`, projects to a transferable `ReadableStream` piped to the `platform` `DecodeWorkerPool`, and runs the Crc32-verify, stitch, and XxHash128-key off the main thread with zero-copy BYOB reads.
- Keeps the main thread responsive during large GLB delivery, makes the worker boundary an explicit zero-copy seam, and positions the rail for a future WebTransport raw-byte transport feeding the same seam.
- Transferable `ReadableStream`, BYOB reads, and WebTransport-in-Workers are the current off-main-thread idiom; the prior design transferred a plain `Uint8Array` per frame and left the worker boundary implicit.

[CONTENT_KEY_PARITY_HARNESS]: a tier-2 byte-identity harness proving the browser content key is bit-identical to the C# digest.
- Proves the browser 128-bit content key over an assembled blob is bit-identical to the C# `XxHash128` suite hash (seed zero, two-64-bit-half order), and asserts the same two-half order for the `useBigInt64` and `DataView` `getBigUint64` HLC bigint round-trip; shared with the future Python IFC-to-GLB companion.
- A re-tessellation of identical bytes by any runtime becomes a content-addressed cache hit across the platform, and a bigint HLC half-swap is caught before it folds a fresh op as stale.
- Content-identity is the one C#-owned seed reproduced bit-identically in all three runtimes; the harness is gated on the `artifacts/frame-reassembly.md` `CONTENT_HASHING` question because the admitted `xxhash-wasm` exposes only `h32`/`h64`, so the 128-bit browser provider (a 128-bit-capable wasm hash, or an XXH64 seed downgrade) must resolve before parity can be asserted.

## [2]-[CLOSED]

None.
