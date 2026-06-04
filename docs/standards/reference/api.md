---
description: Standard for API contracts, generated API reference, and curated API facts
---

# API standards

API documentation is contract-backed reference. It separates owned
machine-readable contracts, generated library reference, and curated external
API facts from explanatory prose.

## Use when

Use this standard for:

- HTTP API contracts and OpenAPI descriptions;
- generated library API reference;
- public API surfaces derived from source metadata;
- curated external API, SDK, protocol, or vendor facts.

Do not use it for public symbol comment style alone, general reference leaves,
or tutorials. Use [code-documentation.md](code-documentation.md),
[reference.md](reference.md), or [tutorial.md](../learning/tutorial.md).

## External basis

Use OpenAPI for HTTP contract semantics. Use XML documentation comments and
DocFX-style generation for .NET library API reference when those tools own the
generated output. Preserve external API semantics from official specifications
or vendor docs when the project does not own the contract.

## Placement

- Owned HTTP contract: near the API owner or generated contract output.
- Generated library reference: generated docs entrypoint or API reference site.
- Curated external API facts: owner-local reference corpus or
  `docs/external-libs`.

Narrative pages may explain authentication, versioning, examples, lifecycle, and
operational constraints. They must link to the contract instead of duplicating
endpoint tables.

## API families

- HTTP contract: an OpenAPI Description is the owned HTTP API contract.
- Generated library reference: source metadata, XML documentation comments,
  assemblies, and generated reference models own public symbol truth.
- Curated API reference: external API, SDK, protocol, and vendor facts belong in
  reference leaves unless an owned generated contract or reference model exists.

## HTTP API requirements

Use OpenAPI 3.2.0 for new HTTP API contracts unless a documented consumer
toolchain requires an older supported OpenAPI line.

Required contract content:

- `openapi` version;
- `info.title` and `info.version`;
- stable `operationId` values;
- `servers` when the API has a non-obvious base URL;
- `paths` and operations;
- tags that organize operations without replacing resource names;
- request and response schemas;
- parameters and request bodies;
- response status codes, media types, and error response schemas;
- security schemes and per-operation authorization requirements;
- examples for non-obvious parameters, request bodies, responses, and errors;
- versioning, lifecycle, and deprecation policy.

When agents, generated clients, or automation consume the contract,
descriptions must carry caller-safe semantics:

- Operation descriptions state preconditions, authorization constraints, valid
  state transitions, idempotency, and when not to invoke the operation.
- Parameter and request-body descriptions state units, ranges, defaults,
  mutually exclusive fields, required combinations, and unsafe values.
- Schema descriptions state invariants, lifecycle meaning, nullability or
  absence semantics, and generated-field behavior.
- Error descriptions state failure causes, retry or abort guidance, and whether
  the caller can repair the request.

## Library API requirements

Generated library API docs must come from source, assemblies, side-by-side XML
documentation files, or equivalent language metadata.

Public visible types and members should document:

- purpose;
- type parameters;
- parameter meaning, units, or caller obligations;
- return values, effects, or typed failure channels;
- property values when useful;
- domain constraints or examples when misuse is likely;
- actual thrown exceptions;
- cross-references when they resolve;
- inherited contracts only when inherited text remains accurate.

APIs that return typed results, effects, validation values, or status objects
must document success and failure channels without implying thrown exceptions.

## Curated API reference

Use curated API reference when the project documents an external API, SDK,
protocol, or vendor surface it does not generate. Curated API facts must:

- cite the official specification, vendor docs, local generated metadata, or
  checked-in contract that proves the fact;
- state version or retrieval date for drift-prone facts;
- separate stable lookup facts from examples and migration notes;
- link to generated or official reference instead of copying catalogs;
- avoid claiming contract ownership over external behavior.

## Contract precedence

1. Owned machine-readable contracts, generated reference output, and contract
   tests.
2. Source metadata, assemblies, XML documentation comments, and generator
   configuration.
3. Official specifications, standards, and vendor reference docs for external
   APIs.
4. Curated prose that explains examples, migration, policy, or design intent.

Curated prose must not fork a generated contract, generated reference model, or
official external API source.

## Boundaries

- Code documentation owns source-level public symbol comments.
- Reference docs own curated lookup facts that are not generated contracts.
- How-to guides own API usage procedures.
- Tutorials own learning paths through API use.

## Review checklist

- [ ] API family is clear.
- [ ] HTTP APIs use OpenAPI 3.2.0 unless a documented consumer constraint
      exists.
- [ ] HTTP contracts include paths, operations, schemas, security, errors,
      examples, and lifecycle policy.
- [ ] Agent- or client-consumed descriptions include preconditions, state
      transitions, authorization constraints, repairability, and when not to
      call the operation.
- [ ] Generated contracts are linked, not copied.
- [ ] Library public symbols have accurate generated metadata.
- [ ] Typed success, failure, and effect channels are documented without false
      throw claims.
- [ ] External API facts cite official or generated evidence and avoid ownership
      claims.
