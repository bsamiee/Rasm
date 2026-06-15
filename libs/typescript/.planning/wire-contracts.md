# [TYPESCRIPT_WIRE_CONTRACTS]

One page owns the single boundary where bytes from the four .NET packages become typed values, and owns nothing above the decode. It holds the generated descriptor surface, the one shared same-origin browser transport, the five decode rails, and the quarantine fold that turns every malformed frame or unknown discriminant into a typed staleness marker. The owning C# `#TS_PROJECTION` fence is the authoritative wire shape; this page names which rail consumes which fence and binds the cross-cluster envelope as a payload discipline, never re-authoring a shape. The page owns clusters 5, 6, and 7 as decode owner and consumes the geometry embedded in clusters 5 and 11 through one geometry rail.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]             | [OWNS]                                                          |
| :-----: | :-------------------- | :-------------------------------------------------------------- |
|   [1]   | TRANSPORT_AND_CLIENTS | one shared transport and one generated client per wire service  |
|   [2]   | DECODE_RAILS          | five codec rails, one per emitted codec, plus the geometry rail |
|   [3]   | QUARANTINE_FOLD       | the tolerance terminal every decode passes through              |

## [2]-[TRANSPORT_AND_CLIENTS]

- Owner: `WireTransport`, the single shared same-origin transport, plus one generated client per browser-dialable service built over it; clients construct only through the descriptor generator the wire-consumption page fixes, never as hand-shaped message literals.
- Cases: the four browser-dialable generated services over one transport; a unary call resolves by await, a server-stream consumes by async iteration; the client-stream capture lane and the bidirectional artifact-sync lane carry no browser client, mirrored from `remote-lane.md#TS_PROJECTION` where the transport-capability shape admits only unary and server-stream on the browser-web row.
- Entry: outbound calls cross one transport whose interceptor stamps the correlation identifier and the trace parent, mirroring the call-spine constants named on `remote-lane.md#TS_PROJECTION`; per-call cancellation threads interruption into transport cancellation through the call signal.
- Packages: the connect transport and its web transport, the protobuf runtime, the descriptor-emitting generator plugin, and the buf toolchain.
- Growth: a new browser-dialable service lands as one generated client row over the same transport; a new remote verb lands as one generated method row; a cross-origin deployment lands the credential and CORS rows as designed-only growth mirrored from the C# boundary.
- Boundary: the transport is same-origin under the co-hosted topology and configures no cross-origin header; the excluded client-stream and bidi lanes carry no browser path because the C# boundary excludes them, never because the branch invents an exclusion.

## [3]-[DECODE_RAILS]

- Owner: `DecodeRail`, the five-codec rail family read by codec key — the protobuf-over-browser-transport rail, the binary snapshot rail, the structured-text receipt rail, `GeometryRail` for embedded geometry, and `FaultDetailRail` for the status-details trailer.
- Cases: the protobuf rail consumes the generated descriptors and decodes unary responses and server-streams against `remote-lane.md#TS_PROJECTION` and `progress-and-observation.md#TS_PROJECTION`; the binary rail decodes snapshot frames and the multi-object sync-segment stream against `snapshot-codecs.md#TS_PROJECTION` and `sync-collaboration.md#TS_PROJECTION` with 64-bit integers mapped to big integers and zero registered extension codecs because the extension-row set is declared empty on its owner page; the structured-text rail decodes each receipt against its owning package contract across clusters 1, 2, 3, 4, 9, 10, and 11; `FaultDetailRail` reconstructs the typed failure union from the status-details trailer named on `remote-lane.md#TS_PROJECTION`.
- Entry: `GeometryRail` is one owner sourcing no wire contract of its own; it decodes only embedded geometry fields, invoked by cluster 5 on the snapshot-delta payloads whose geometry rides the GeoJSON projection owned by `snapshot-codecs.md#TS_PROJECTION`, and invoked by cluster 11 on the geometry embedded inside the evidence envelope payloads. No twelfth fence exists and the rail is never re-authored per cluster.
- Packages: the messagepack codec, the protobuf runtime, and the effect core for the stream and exhaustive-match primitives.
- Growth: a new codec lands as one rail row; a new wire shape on an existing codec lands as one schema row on the owning rail; zero new rail.
- Boundary: the binary rail registers zero extension codecs because the extension-row set is empty by contract; `GeometryRail` provenance is embedded-only and never an invented contract; this domain references no telemetry type because telemetry crosses no wire contract.

## [4]-[QUARANTINE_FOLD]

- Owner: `QuarantineFold`, the single tolerance terminal every decode passes through, turning an unrecognized discriminant or a malformed frame into a typed staleness marker rather than a thrown failure.
- Cases: an unknown discriminant folds to a quarantine case so the stream survives; a disconnect marks the value stale with a typed retry rather than a silent gap; an additive member skip-decodes; a breaking drift surfaces as a typed fault through `FaultDetailRail`, never a silent decode error.
- Entry: the exhaustive case-fold proves at the type level that every wire discriminant the C# side enumerated has a landing; the quarantine case is the landing for the not-yet-enumerated.
- Packages: the effect core for the exhaustive-match primitive.
- Growth: a new union case the C# side adds lands as one literal on the owning rail and one fold arm, with the quarantine case absorbing it until the descriptor set regenerates.
- Boundary: tolerance is a consumption behavior layered over the settled shape, never a modification of it; the cross-cluster envelope binding is consumed as given — every structured-text receipt payload binds as the envelope payload type with the envelope discriminant mirroring the payload discriminant, and each payload decodes against its owning package contract. The `runtime-ports.md#WIRE_LAW` anchor is read only for the converter-precedence and HLC-stamp envelope-payload-binding discipline, never for token grounding; the envelope tokens ground at `runtime-ports.md#TS_PROJECTION`.
