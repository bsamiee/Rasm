# [TYPESCRIPT_ARCHITECTURE_POSTURE]

One page fixes the Effect application shape, the state and data layer, and the host topology for the TS lib branch. The doctrine altitudes mirror the C# suite: one owner per axis, one rail per concern, derivation over enumeration, admission once at the boundary, zero flat code.

## [1]-[APP_SHAPE]

Each suite altitude has exactly one TS form; a second form on the same altitude is the named defect:

- Vocabulary axis: `Schema.Literal` unions plus vocabulary `Record` objects carrying behavior values.
- Domain shape: one `Schema.Class` per boundary concept; projections derive from the owning class, never parallel structs.
- Error rail: `Data.TaggedError` families, one fault family per rail; `FaultDetailWire` reconstructs the .NET faults as one tagged family.
- Effect rail: `Effect<A, E, R>` carries effects, `Either` carries pure branching, and `Stream` carries connect-es server-streams; thrown exceptions never cross a module boundary.
- Service: `Effect.Service` classes, one owner per axis.
- Composition: one `Layer` graph at the app entry; one runtime per host surface.
- Boundary: `Schema` decode at the wire edge; the interior never re-validates, `Option` carries absence, and `null` exists only at the JSON boundary.

The service owner budget is closed; a new capability lands as a method or row on an existing owner, never as a sibling service.

| [INDEX] | [AXIS]            | [OWNER]          | [CONSUMES]                                                                      |
| :-----: | :---------------- | :--------------- | :------------------------------------------------------------------------------ |
|   [1]   | wire clients      | `WireClients`    | the four browser-dialable generated services over one shared grpc-web transport |
|   [2]   | snapshot decode   | `SnapshotFeed`   | msgpack decoder and header reads over the Persistence codec rows                |
|   [3]   | runtime records   | `RuntimeFeed`    | JSON record decode and receipt-envelope payload binding                         |
|   [4]   | command gateway   | `CommandGateway` | intent keys, availability rows, invocation, and deep-link routing               |
|   [5]   | evidence timeline | `EvidenceFeed`   | correlation-keyed envelope ingestion and the skew-band fold                     |

## [2]-[STATE_LAYER]

Server-stream subscriptions feed key-discriminated stores — each store is one fold over event kinds into an immutable keyed map. Effect state primitives own every store; a parallel view-state library over domain data is the rejected form.

[STORES]:
- Document store: one fold over the `DocumentService.documentEvents` stream into a map keyed by entity key; add, update, and remove project from the event kind.
- Progress cells: per-correlation monotonic cells over the `ComputeService.progress` stream; a mark ranked below the held rank never regresses the cell.
- Health and degradation: one subscribed record over `Health.watch` plus `DegradationWire`; capability keys gate feature surfaces.
- Evidence timeline: a correlation-keyed timeline over receipt envelopes, ordered by the HLC stamp; skew bands render as uncertainty regions without recomputing the fold.
- Conflicts and presence: `ConflictReceiptWire` and `PresenceRowWire`; presence rows expire on `expiresAt`, and conflict receipts feed the inspector pane.
- Command availability: keyed `CommandAvailabilityWire` rows; the degradation level key is an input, never re-derived.

Stream interruption folds to a typed retry policy value; a disconnected store renders its last value with a staleness marker and resumes on redial.

## [3]-[HOST_TOPOLOGY]

One co-hosted origin carries the SPA and every dashboard route:

- Co-hosted SPA: the web-service app root serves the built SPA, so static assets and the grpc-web endpoint share one origin; CORS is zero day-one, and the CORS header row is designed-only growth for a cross-origin deployment.
- Browser credential: bearer is the browser case on the Compute `CredentialPolicy` axis and attaches only when the cross-origin growth row activates; mTLS client certificates and UDS peer-credential are structurally absent in the browser.
- Browser streams: ArtifactSync bidi is excluded on the browser row, so `WireClients` carries no ArtifactSync client; the browser collaboration decomposition — server-stream down, unary up — is designed-only growth of rpc rows on the .NET side.
- Evidence dashboard: an SPA route ingesting the receipt-envelope timeline and correlation drill-down joins.
- Benchmark dashboards: SPA routes rendering `BenchmarkClaimWire` rows fingerprint-gated by `HostFingerprintWire`.
- Companion control panel: an SPA route driving the three ControlService verbs — `captureSupport`, `setDegradation`, `reloadOptions` — and `CommandIntent` deep links through string keys.
- View boundary: components from the React line in the workspace catalog subscribe at the edge; Effect owns every concern below the component boundary.
