# [INTERCHANGE_TASKLOG]

Open and closed work distilled from `IDEAS.md`. Each task is a card whose leader carries a status marker — `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` when open; `[COMPLETE]` or `[DROPPED]` when closed — and bullets naming the capability or file to build, the external packages to integrate, the integration points and boundaries/wires, and the key considerations. One idea spawns one or more tasks.

## [1]-[OPEN]

[QUEUED] PROTOCOL_SELECTION_AXIS — collapse `WireTransport` onto the protocol axis.
- Build the `TransportProtocol` `Match` dispatch in `transport/transport.md` `WireTransportLive` selecting `createConnectTransport` for the long-lived server-stream legs and `createGrpcWebTransport` where dictated; the `TransportCapabilityWire` two-key tuple keys the per-protocol method set.
- Integrate `@connectrpc/connect-web` (both transport factories) and `effect` `Match`.
- Reads `RuntimeConfig.transportProtocol` and `RuntimeConfig.apiBaseUrl` from `platform` and `AuthSession.tokenHeader` for the interceptor; the `@connectrpc/*` import never escapes this folder; the capability tuple is read from the upstream wire, never invented branch-side.
- The interceptor stamp resolves the live span and token per call through one `Runtime.runPromise`; the fault rail keys the no-trailer landing by the connect `Code` so a server `Code.DeadlineExceeded` leg lands distinctly from the `Code.Canceled` a client `AbortError`/`TimeoutError` yields.

[QUEUED] STRUCTURED_DRIFT_REPORT — author the drift classifier and report.
- Build `QuarantineFold`, the three-case `ContractDrift` `Data.TaggedEnum`, the `ISSUE_DRIFT` issue-tag vocabulary, and `driftReportOf` in `quarantine/drift-terminal.md` running a lenient (`onExcessProperty: "preserve"`) and a strict (`onExcessProperty: "error"`) decode of one schema, classifying Identical/Additive/Breaking from the `ArrayFormatter` issue tags.
- Integrate `effect` `Schema.decodeUnknownEither`, `ArrayFormatter`, the `Record` vocabulary dispatch, `Either`, and `Data.TaggedEnum`.
- A Breaking drift surfaces as `faults/fault-family.md` `FaultDetail.Quarantine` carrying the report; the `projection` fold tier reads the `ContractDrift` outcome and the `ui` renders the report; the fold is the drift-decode boundary for untrusted ingress, never a re-validation of an already-decoded value.
- The three-case fold is total over every decode outcome; an Additive drift never silently faults and a Breaking drift never silently survives, and a transport disconnect is a `FaultDetail`, never a drift class.

[QUEUED] DECODE_BUDGET_REFINEMENT — add the ingress budgets and the sanitizer.
- Build the `DecodeBudget`, `decodeBounded`, and `boundedFrames` rows in `refinement/schema-refinement.md` and the `sanitize` row at `quarantine/drift-terminal.md`, gating the structured-text and artifact-frame ingress paths.
- Integrate `effect` `Schema.filter`/`Stream` bounds and `isomorphic-dompurify` `DOMPurify.sanitize`.
- The budgets gate `codecs/decode-rail.md` structured-text decode and `artifacts/frame-reassembly.md` frame reassembly; the sanitizer runs once at the quarantine terminal before any `ui` render binding; the budgets gate untrusted ingress only, never the trusted same-origin lane.
- A depth, frame-count, or assembled-byte breach faults before runtime exhaustion; the sanitizer is the rendered-text ceiling and the budgets are the byte and shape ceilings.

[QUEUED] TRANSFERABLE_STREAM_REASSEMBLY — make the worker boundary a transferable seam.
- Build the transferable-stream projection in `artifacts/frame-reassembly.md` `ArtifactFrameRail` lifting the framing-fold `Stream` to a transferable `ReadableStream` piped to the worker pool, running the Crc32-verify, stitch, and XxHash128-key off the main thread with BYOB reads.
- Integrate `effect` `Stream` and the resolved 128-bit wasm hash run inside the worker (the `artifacts/frame-reassembly.md` `CONTENT_HASHING` provider gate; the admitted `xxhash-wasm` carries only `h32`/`h64`), the owned table-driven `Crc32`.
- Consumes the `transport/transport.md` `ArtifactFrameStreaming` output and the `platform` `DecodeWorkerPool`; produces the `ArtifactBlob` the `projection` evidence fold and the `ui` GLB viewport read; reassembles only the server-streamed frame type, never the bidi sync method.
- Zero-copy BYOB reads keep the main thread responsive during large GLB delivery; a future WebTransport raw-byte leg feeds the same transferable seam as one transport protocol case.

[BLOCKED] CONTENT_KEY_PARITY_HARNESS — prove cross-runtime byte-identity.
- Build the tier-2 byte-identity harness asserting the browser 128-bit content key over an assembled blob equals the C# `XxHash128` digest (seed zero, two-64-bit-half order) and the `useBigInt64`/`getBigUint64` HLC bigint round-trip holds the same two-half order.
- Integrate the resolved 128-bit-capable wasm hash (the admitted `xxhash-wasm` carries only `h32`/`h64`) and `@msgpack/msgpack` `useBigInt64` `Decoder`.
- Asserts against the C#-owned digest seed and the `long`/`ulong` HLC encoding at the cross-language wire seam; settles the `artifacts/frame-reassembly.md` content key as trusted cross-runtime and the bigint round-trip the `projection` conflict-presence fold depends on.
- Blocked on two gates: the `artifacts/frame-reassembly.md` `CONTENT_HASHING` 128-bit-provider question (admit a 128-bit-capable wasm hash or downgrade the C#-owned seed to XXH64 so `h64Raw` carries it), and the upstream C# `XxHash128` suite-hash plus HLC encoding fixtures; an HLC `logical` off-by-one-half folds a fresh op as stale with no other signal, so the half-order assertion is load-bearing.

[BLOCKED] CAPABILITY_SDK_CODEGEN — admit the second codegen plugin.
- Build the second `buf.gen.yaml` plugin row and the `CapabilitySdk` `Effect.Service` in `transport/transport.md` `CODEGEN_TOOLING` deriving the typed effect-classed command surface and the MCP tool projection from the generated `capabilities_pb.ts`.
- Integrate `@bufbuild/buf`, `@bufbuild/protoc-gen-es`, and the capability-descriptor codegen plugin.
- Consumes the `csharp:Rasm.AppHost/capability/registry#SDK_CODEGEN` `DiscoveryResultWire[]` descriptor at the cross-language wire seam; the `CapabilityClient` dials over the same shared `WireTransport`; the branch reads the generated descriptor and never re-authors a capability shape or hand-writes a command method.
- Blocked on the upstream `csharp:Rasm.AppHost/capability/registry#SDK_CODEGEN` descriptor source; `invoke` is one polymorphic method keyed by descriptor id, never a sibling method per descriptor; the `inputSchema` is the generated per-descriptor JSON Schema, never a hand-built stub.

## [2]-[CLOSED]

None.
