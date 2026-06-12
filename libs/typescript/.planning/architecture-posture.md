# [TYPESCRIPT_ARCHITECTURE_POSTURE]

One page fixes the Effect application shape, the state and data layer, and the host topology for the TS lib branch. The doctrine altitudes mirror the C# suite: one owner per axis, one rail per concern, derivation over enumeration, admission once at the boundary, zero flat code.

## [1]-[APP_SHAPE]

Each suite altitude has exactly one TS form; a second form on the same altitude is the named defect.

| [INDEX] | [ALTITUDE]       | [SUITE_FORM]                             | [TS_FORM]                                                                                        |
| :-----: | :--------------- | :--------------------------------------- | :------------------------------------------------------------------------------------------------ |
|   [1]   | vocabulary axis  | `SmartEnum<string>` rows                  | `Schema.Literal` unions plus vocabulary `Record` objects carrying behavior values                   |
|   [2]   | domain shape     | `[Union]` / `[ValueObject]` owners        | `Schema.Class` per boundary concept; projections derive from the owning class, never parallel structs |
|   [3]   | error rail       | typed fault unions on `Fin`/`Validation`  | `Data.TaggedError` families; `FaultDetailWire` reconstructs the .NET faults as one tagged family    |
|   [4]   | effect rail      | `Eff`/`IO` carriers                       | `Effect<A, E, R>`; `Either` for pure branching; `Stream` over connect-es server-streams             |
|   [5]   | service          | port records                              | `Effect.Service` classes, one owner per axis                                                        |
|   [6]   | composition      | one composition root per process          | one `Layer` graph at the app entry; one runtime per host surface                                    |
|   [7]   | boundary         | admission once into evidence owners       | `Schema` decode at the wire edge; the interior never re-validates; `Option` carries absence and `null` exists only at the JSON boundary |

The service owner budget is closed; a new capability lands as a method or row on an existing owner, never as a sibling service.

| [INDEX] | [AXIS]            | [OWNER]          | [CONSUMES]                                                                 |
| :-----: | :---------------- | :--------------- | :-------------------------------------------------------------------------- |
|   [1]   | wire clients      | `WireClients`    | the four browser-dialable generated services over one shared grpc-web transport; the grpcWeb capability column excludes an ArtifactSync client |
|   [2]   | snapshot decode   | `SnapshotFeed`   | msgpack decoder and header reads over the Persistence codec rows             |
|   [3]   | runtime records   | `RuntimeFeed`    | JSON record decode and receipt-envelope payload binding                      |
|   [4]   | command gateway   | `CommandGateway` | intent keys, availability rows, invocation, and deep-link routing            |
|   [5]   | evidence timeline | `EvidenceFeed`   | correlation-keyed envelope ingestion and the skew-band fold                  |

## [2]-[STATE_LAYER]

Server-stream subscriptions feed key-discriminated stores — each store is one fold over event kinds into an immutable keyed map. Effect state primitives own every store; a parallel view-state library over domain data is the rejected form.

| [INDEX] | [STORE]                | [SOURCE]                                  | [LAW]                                                                                   |
| :-----: | :--------------------- | :----------------------------------------- | :--------------------------------------------------------------------------------------- |
|   [1]   | document store         | `DocumentService.documentEvents` stream    | one fold into a map keyed by entity key; add, update, and remove project from the event kind |
|   [2]   | progress cells         | `ComputeService.progress` stream           | per-correlation monotonic cell; a mark ranked below the held rank never regresses the cell |
|   [3]   | health and degradation | `Health.watch` plus `DegradationWire`      | one subscribed degradation record; capability keys gate feature surfaces                  |
|   [4]   | evidence timeline      | receipt envelopes                          | correlation-keyed timeline ordered by the HLC stamp; skew bands render as uncertainty regions without recomputing the fold |
|   [5]   | conflicts and presence | `ConflictReceiptWire`, `PresenceRowWire`   | presence rows expire on `expiresAt`; conflict receipts feed the inspector pane            |
|   [6]   | command availability   | `CommandAvailabilityWire`                  | keyed availability rows; the degradation level key is an input, never re-derived          |

Stream interruption folds to a typed retry policy value; a disconnected store renders its last value with a staleness marker and resumes on redial.

## [3]-[HOST_TOPOLOGY]

| [INDEX] | [SURFACE]               | [HOST]                                | [LAW]                                                                                |
| :-----: | :---------------------- | :------------------------------------ | :------------------------------------------------------------------------------------ |
|   [1]   | co-hosted SPA           | web-service app root                  | static assets and the grpc-web endpoint share one origin; bearer attaches only when the cross-origin growth row activates |
|   [2]   | evidence dashboard      | SPA route                             | ingests the receipt-envelope timeline and correlation drill-down joins                 |
|   [3]   | benchmark dashboards    | SPA route                             | renders `BenchmarkClaimWire` rows fingerprint-gated by `HostFingerprintWire`           |
|   [4]   | companion control panel | SPA route                             | drives the three ControlService verbs — captureSupport, setDegradation, reloadOptions — and CommandIntent deep links through string keys |
|   [5]   | view boundary           | React line from the workspace catalog | components subscribe at the edge; Effect owns every concern below the component boundary |
