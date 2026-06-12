# [RASM_PERSISTENCE]

`Rasm.Persistence` is the durable-state package of the app suite. It owns store
profiles, data lanes, schema and query rails, native SQLite truth, snapshot
codecs, cache indexes, sync and collaboration transports, and redaction and
retention — one rail per concern, consuming AppHost ports (clock, telemetry,
receipts, drain, classification, cache) as settled vocabulary.

## [1]-[SCOPE_AND_INTENT]

- Zero consumers exist; the implementation is full-capability with no holding back.
- Nine finalized planning pages are decision-complete blueprints; implementation transcribes them and never re-designs downstream.
- Variance is rows, cases, and policy values on budgeted axis owners; a new capability is a row, never a new surface.
- The package is not an EF wrapper, repository family, serializer wrapper, provider service set, GH solve-path cache, or domain-model replacement.
- Persistence is RhinoCommon-free; app roots resolve host profile, path, and dsn values before calling in.

## [2]-[ROUTING]

| [INDEX] | [READ_FOR]                               | [OPEN]                                            |
| :-----: | :--------------------------------------- | :------------------------------------------------ |
|   [1]   | rails, seams, package ownership          | [architecture](ARCHITECTURE.md)                   |
|   [2]   | implementation sequence + start gates    | [roadmap](ROADMAP.md)                             |
|   [3]   | capability atlas                         | [features](FEATURES.md)                           |
|   [4]   | implementation charter + finalized pages | [.planning](.planning/README.md)                  |
|   [5]   | package API catalogues                   | [.api](.api/README.md)            |
|   [6]   | suite planning standard                  | [suite standard](../.planning/README.md)          |
|   [7]   | suite ownership ledger                   | [region map](../.planning/region-map/)         |
|   [8]   | C# stack doctrine                        | [C# stack](../../../docs/stacks/csharp/README.md) |
