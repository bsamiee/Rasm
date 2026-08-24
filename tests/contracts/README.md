# [CONTRACTS_CORPUS]

`tests/contracts/` registers every process or publisher wire boundary once, including same-language process crossings. Each named producer, minter, inbound application reader, and consumer binds the exact boundary entrypoint. `manifest.json` is the registry, the Assay msgspec model is its grammar, and `manifest.schema.json` derives from that model.

## [01]-[AUTHORITY]

Each entry groups cases under one law; every case is the atomic authority and readiness unit:

- `infrastructure`: Two or more independent minters construct the value from their own inputs.
- `domain`: Exactly one semantic producer owns the value.
- `application`: Deploying applications and external clients supply a public inbound value; the estate names its exact readers alone.
- `publisher`: Immutable upstream publishers own the definition bytes.

## [02]-[LAYOUT]

```text conceptual
tests/contracts/
├── manifest.json · manifest.schema.json            # Authored registry and schema derived from the rail model
├── .api/                                           # Corpus tool catalog: buf, plugins, and the Assay-owned gate
├── proto/buf.md                                    # Module front door the BSR renders; absence falls back to the repo README
├── proto/rasm/contracts/<family>/*.proto           # Estate module; sibling files split by ownership, one package per family
├── vendor/<publisher>/                             # Immutable publisher sources, licenses, and conformance vectors
└── <seam>/                                         # Present only when one or more cases carry verified proof assets
    ├── <vector>.bin                                # Frozen binary, framed identity vector, or publisher evidence
    ├── <vector>.h5 · <vector>.mtx                  # Native scientific-container specimens decoded by their format libraries
    ├── <vector>.facts.json                         # Typed normalized facts for semantic conformance or value parity
    └── <fqn>.jsonschema.strict.bundle.json         # Gate-derived only when a named consumer evaluates the document
```

[PROTO_SEATING]: One estate family lands under `proto/rasm/contracts/<family>/`, declares `package rasm.contracts.<family>`, and carries no file option; managed mode derives every language option. Root `buf.yaml` names this one release unit `buf.build/rasm/contracts`; publisher modules remain unnamed and never enter that publication.

[PUBLISHER_SEATING]: One publisher lands under `vendor/<publisher>/`. Its proto is an independent Buf module under `proto/`; its definition records the local source, immutable repository commit and upstream path, colocated Apache-2.0 license, and license SHA-256.

## [03]-[DEFINITION]

Each case owns one machine-resolved `definition`:
- `proto`: Exact message FQN and framing, resolved from the built image; RPC actors also bind the service method and message direction.
- `cloudevent`: Exact CloudEvents protobuf envelope with one application event-type discriminant.
- `law`: One repo cluster `path.md#[NN]-[CLUSTER]` for a framing seam the type system cannot hold.
- `publisher`: Exact publisher format, local source, and immutable upstream origin; publisher cases alone.
- `schema`: DERIVED `json-strict-bundle`, `derivedFrom: proto:<fqn>` or `msgspec:<type>`, only for a real evaluator.

`definition` owns decoder routing. Entry `id` derives the corpus directory. Every actor binds one live fence and one literal source symbol through `coordinate`, then declares generated, package, or proof custody. RPC actors also bind one exact method and request or response direction.

Generated actors alone select public Buf roots, and descriptor support closure stays generated rather than hand-rostered. Generated actors name descriptor-validated `supports` where their boundary uses a generated symbol outside that closure; a support selects codegen, never a case or reader.

## [04]-[PROOF]

Readiness is tagged:

- `blocked`: Decision-complete authority and actors with exact unmet executable evidence; no vectors.
- `verified`: One primary oracle with non-empty proof vectors.

Oracle vocabulary is closed:

- `semantic-conformance`: One specimen decodes to one typed expected-facts asset.
- `semantic-roundtrip`: Protobuf decode, normalized encode, and second decode preserve the value and reject unknown fields.
- `value-parity`: One independently minted specimen per declared minter decodes to one typed expected value.
- `external-digest`: Exact non-Protobuf external bytes are the contract.
- `publisher-digest`: Immutable publisher bytes match their recorded upstream custody.

Every asset independently records its byte count and tagged `xxh128` or `sha256` fingerprint. Publisher assets require SHA-256. Protobuf deterministic serialization is never cross-runtime canonical byte identity. Each value-parity specimen carries `actor_key(minter)`, derived from the actor anchor and coordinate; aliases and inferred path provenance are not admitted. Native HDF5 and Matrix Market laws use their official decoders and typed facts. They are estate contracts, never publisher custody or generated SDKs.

## [05]-[REGISTRATION]

Each new grouping lands one entry carrying a non-empty case set and one law sentence under 240 characters. Domain cases name the exact producer, infrastructure cases every independent minter, application cases a public typed input beside every generated or package ingress, and publisher cases the frozen local source and immutable origin.

Application authority never covers a missing estate producer: a deploying application owns the inbound value or the case is not application. Every non-publisher case names one literal reachable reader, since a producer, descriptor, registry slot, or equivalence helper establishes no crossing. Verification atomically replaces blockers with vectors, and blocked evidence materializes no directory.

## [06]-[GATE]

`assay contracts check` is the one gate: build, STANDARD lint carving named rules where a spelling lawfully breaks them, format diff over the estate module alone, manifest audit, and scratch freshness.

`assay contracts generate` writes clean public roots, locked support closure, emitted `.api` rosters, and derived schemas locally. `assay contracts publish` reruns the complete gate under the same lease, then re-resolves the unchanged default-label commit or re-proves exact module absence immediately before pushing the named estate module publicly, carrying no publisher module and no Git metadata. Missing labels, authentication failures, and network failures never bootstrap.
