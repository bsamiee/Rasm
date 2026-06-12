# [TYPESCRIPT_ARCHITECTURE_POSTURE]

One page fixes the Effect application shape, the state and data layer, the host topology, and the testing law for the TS campaign. The doctrine altitudes mirror the C# suite: one owner per axis, one rail per concern, derivation over enumeration, admission once at the boundary, zero flat code.

## [1]-[APP_SHAPE]

Each suite altitude has exactly one TS form; a second form on the same altitude is the named defect.

| [INDEX] | [ALTITUDE]       | [SUITE_FORM]                          | [TS_FORM]                                                                          |
| :-----: | :--------------- | :------------------------------------ | :----------------------------------------------------------------------------------- |
|   [1]   | vocabulary axis  | `SmartEnum<string>` rows              | `Schema.Literal` unions plus vocabulary `Record` objects carrying behavior values     |
|   [2]   | domain shape     | `[Union]` / `[ValueObject]` owners    | `Schema.Class` per boundary concept; projections via `pick`/`omit`, never parallel structs |
|   [3]   | error rail       | typed fault unions on `Fin`/`Validation` | `Data.TaggedError` families; `FaultDetailWire` reconstructs the .NET faults as one tagged family |
|   [4]   | effect rail      | `Eff`/`IO` carriers                   | `Effect<A, E, R>`; `Either` for pure branching; `Stream` via `Stream.fromAsyncIterable` over connect-es server-streams |
|   [5]   | service          | port records                          | `Effect.Service` classes, one owner per axis                                          |
|   [6]   | composition      | one composition root per process      | one `Layer` graph at the app entry; one runtime per host surface                       |
|   [7]   | boundary         | admission once into evidence owners   | `Schema` decode at the wire edge; the interior never re-validates; `Option` carries absence and `null` exists only at the JSON boundary |

The service owner budget is closed; a new capability lands as a method or row on an existing owner, never as a sibling service.

| [INDEX] | [AXIS]            | [OWNER]          | [CONSUMES]                                                              |
| :-----: | :---------------- | :---------------- | :------------------------------------------------------------------------ |
|   [1]   | wire clients      | `WireClients`     | the four browser-dialable generated services over one shared grpc-web transport; the grpcWeb capability column excludes an ArtifactSync client |
|   [2]   | snapshot decode   | `SnapshotFeed`    | msgpack decoder and header reads over the Persistence codec rows          |
|   [3]   | runtime records   | `RuntimeFeed`     | JSON record decode and receipt-envelope payload binding                   |
|   [4]   | command gateway   | `CommandGateway`  | intent keys, availability rows, invocation, and deep-link routing         |
|   [5]   | evidence timeline | `EvidenceFeed`    | correlation-keyed envelope ingestion and the skew-band fold               |

## [2]-[STATE_LAYER]

Server-stream subscriptions feed key-discriminated stores — the IChangeSet-equivalent is a `Stream.scan` fold over event kinds into immutable keyed maps. Effect state primitives own every store; a parallel view-state library over domain data is the rejected form.

| [INDEX] | [STORE]            | [SOURCE]                                  | [SHAPE]                                                                              |
| :-----: | :----------------- | :----------------------------------------- | :--------------------------------------------------------------------------------------- |
|   [1]   | document store     | `DocumentService.documentEvents` stream    | `Stream.scan` fold into `HashMap` keyed by entity key; add, update, and remove project from the event kind |
|   [2]   | progress cells     | `ComputeService.progress` stream           | per-correlation `SubscriptionRef`; a mark with a lower rank than the held rank never regresses the cell |
|   [3]   | health and degradation | `Health.watch` plus `DegradationWire`  | `SubscriptionRef` of the degradation record; capability keys gate feature surfaces        |
|   [4]   | evidence timeline  | receipt envelopes                          | correlation-keyed `HashMap` of timeline rows ordered by the HLC stamp; skew bands render as uncertainty regions without recomputing the fold |
|   [5]   | conflicts and presence | `ConflictReceiptWire`, `PresenceRowWire` | presence rows expire on `expiresAt`; conflict receipts feed the inspector pane         |
|   [6]   | command availability | `CommandAvailabilityWire`                | keyed `HashMap` of availability rows; the degradation level key is an input, never re-derived |

Stream interruption folds to `Stream.retry` under a `Schedule` policy value; a disconnected store renders its last value with a staleness marker and resumes on redial.

## [3]-[HOST_TOPOLOGY]

| [INDEX] | [SURFACE]               | [HOST]                          | [LAW]                                                                                |
| :-----: | :---------------------- | :------------------------------- | :--------------------------------------------------------------------------------------- |
|   [1]   | co-hosted SPA           | web-service app root             | static assets and the grpc-web endpoint share one origin; bearer attaches only when the cross-origin growth row activates |
|   [2]   | evidence dashboard      | SPA route                        | ingests the receipt-envelope timeline and correlation drill-down joins                    |
|   [3]   | benchmark dashboards    | SPA route                        | renders `BenchmarkClaimWire` rows fingerprint-gated by `HostFingerprintWire`              |
|   [4]   | companion control panel | SPA route                        | drives the three ControlService verbs — captureSupport, setDegradation, reloadOptions — and CommandIntent deep links through string keys |
|   [5]   | view boundary           | React line from the workspace catalog | components subscribe at the edge; Effect owns every concern below the component boundary |

## [4]-[TESTING_POSTURE]

The `testing-ts` rail is the quality law for every TS module; its constraints are binding, not targets.

| [INDEX] | [LAW]              | [VALUE]                                                                                  |
| :-----: | :----------------- | :----------------------------------------------------------------------------------------- |
|   [1]   | spec budget        | 175 LOC flat cap per spec file; one spec per source module per category                     |
|   [2]   | coverage           | 95% per-file V8 coverage — statements, branches, functions, each file measured independently |
|   [3]   | mutation           | Stryker thresholds: break 50, low 60, high 80, with the TS checker eliminating compile-error mutants |
|   [4]   | property law       | `it.effect.prop` with Schema-passed arbitraries; pure transforms prove algebraic laws, effectful boundaries prove properties |
|   [5]   | density            | two to four laws sharing one arbitrary shape pack into one property; edge cases aggregate through `Effect.all` |
|   [6]   | oracle independence | expected values come from laws, external oracles, or standards — never from re-deriving source logic |
|   [7]   | contract specs     | wire decode laws prove against fixtures the .NET proof gates emit, so codec drift fails the TS lane before a dashboard renders it |
