# [RASM_PERSISTENCE_ROADMAP]

`Rasm.Persistence` is built through store, schema, projection, snapshot, support, redaction, and cache/index rails. Package lanes follow owner responsibility.

## [1]-[CAPABILITY_RAILS]

| [INDEX] | [RAIL]          | [EXIT_STATE]                                                             |
| :-----: | --------------- | ------------------------------------------------------------------------ |
|   [1]   | Store profile   | Path/scope/schema/host identity received from app root                   |
|   [2]   | Store lifecycle | Closed through ready/draining/maintenance/corrupt states are explicit    |
|   [3]   | Store algebra   | Lifecycle and query operations execute through one dispatch rail         |
|   [4]   | Receipts        | Success/failure cases cover native, schema, migration, redaction, backup |
|   [5]   | Schema          | EF history, user version, migration lock, downgrade guard                |
|   [6]   | Native init     | `Batteries.Init()` and PRAGMA setup are receipt-backed                   |
|   [7]   | Live projection | Serial worker publishes app state and participates in drain              |
|   [8]   | Snapshot        | Envelope has schema, codec, payload, explicit checksum                   |
|   [9]   | Support export  | Classified and redacted artifacts                                        |
|  [10]   | Cache/index     | Model-result cache and benchmark artifact index entities                 |

## [2]-[LANE_CONTRACT]

| [INDEX] | [LANE]       | [CONTRACT]                                      |
| :-----: | ------------ | ----------------------------------------------- |
|   [1]   | EF SQLite    | Active project setup and package references     |
|   [2]   | Redaction    | Classification and redactor registration source |
|   [3]   | MessagePack  | Snapshot round-trip proof and analyzer route    |
|   [4]   | LZ4          | Measured snapshot payload proof                 |
|   [5]   | Bulk import  | Raw `Microsoft.Data.Sqlite` benchmark first     |
|   [6]   | FTS5/JSON1   | Native SQLite probe plus query source           |
|   [7]   | Companion DB | Out-of-process only                             |

The lane set is provider-rich and single-rail. New storage providers, snapshot codecs, cache stores, and companion databases add store-profile cases, query handlers, receipt cases, and proof rows inside the Persistence rail; they do not add repository families or provider-branded public services.

## [3]-[IMPLEMENTATION_DOCTRINE]

- Store logic enters through one lifecycle/query dispatch rail. Repository families, ad hoc stores, and per-entity service sets are rejected.
- Store lifecycle is state-driven. Migration, lock, corruption, native-load, projection, snapshot, export, and drain evidence are first-class receipts.
- EF Core is an implementation rail, not the public model. Public concepts are store profile, entity kind, query shape, operation state, projection state, and receipt evidence.
- Redaction is part of export, not an afterthought. Classification, redactor registration, redacted output proof, and failure receipts ship with support bundles.
- Native SQLite, PRAGMA policy, schema gates, and migration locks are proven at open time before normal operations run.
- Snapshot codecs and compression carry round-trip, size, hash, and compatibility proof.
- No store operation runs in GH solve hot paths.

## [4]-[INTEGRATION]

- AppHost supplies scheduling, drain, support-bundle trigger, retry cadence, and profile/path handoff.
- AppUi observes read-only app-state snapshots on its UI scheduler.
- Compute stores deterministic model-result cache and benchmark artifact index data through the store rail.
- Rhino/GH2 resolve host profile/path before Persistence is called; Persistence stays RhinoCommon-free.
- Kernel code never references EF, SQLite, redaction, snapshot, or store packages.

## [5]-[VALIDATION]

| [INDEX] | [GATE]       | [REQUIRED_STATE]                                      |
| :-----: | ------------ | ----------------------------------------------------- |
|   [1]   | Restore      | Persistence lockfile is current                       |
|   [2]   | Build        | Persistence package scaffold builds                   |
|   [3]   | Architecture | Persistence has no Rhino/GH2/AppUi implementation ref |
|   [4]   | Package      | Direct/transitive package checks are clean            |
|   [5]   | Store laws   | Migration lock, downgrade, checksum, redaction laws   |
