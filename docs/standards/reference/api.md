# [API_DOCUMENTATION]

API documentation is contract-backed reference for a callable surface. It tells an agent which contract carries caller-visible truth, which profile-specific facts may be curated, how generated output is refreshed, and which adjacent document carries everything else. It is not a command catalog, generated symbol mirror, host SDK tutorial, support policy, or local-tool operator guide.

This standard assumes local source-collection mechanics already live in repo instruction files, route READMEs, generated artifacts, and tooling documentation. Do not restate those mechanics here. Follow the local tool route, then write the smallest API page that preserves contract identity, caller obligations, failure/status behavior, proof fields, and adjacent-route handoff.

## [1][USE_WHEN]

Apply this standard to a surface that callers, generated clients, or agents invoke or parse:
- local HTTP contracts and OpenAPI descriptions;
- local CLI, local tool, command-envelope, or agent-callable contracts;
- MCP or tool-callable schemas and catalogs;
- generated host/API metadata queried through maintained local tooling;
- generated library reference built from source, assemblies, XML documentation, docstrings, TSDoc, or equivalent source comments;
- curated upstream HTTP API facts;
- curated upstream SDK, protocol, or product API facts.

Route source-level public symbol comment style to [code-documentation.md](code-documentation.md). Route lookup facts that are not callable API surfaces to [reference.md](reference.md), support status to [support-matrix.md](support-matrix.md), API procedures to [how-to.md](../task/how-to.md), learning paths to [tutorial.md](../learning/tutorial.md), and first entry routes to [readme.md](readme.md).

[AUTHORING_CONTRACT]:
- Agent use: locate the controlling contract or upstream artifact, collect generated or maintained truth through the local route, then document only the caller-facing facts the chosen profile requires.
- Required produced structure: opening scope, `Contract`, one profile body, `API change maintenance`, `Boundaries`, and `Validation`.
- Section cardinality: one primary profile, one contract record, and only the profile sections triggered by the callable surface.
- Adjacent checks: code documentation for source comments, reference for command flags and lookup tables, README for entrypoints, support matrix for lifecycle, how-to/tutorial for procedures, architecture for routing.
- Maintenance triggers: callable contract, parameter, field, status, error, envelope, generated output, source comment, support row, or entrypoint changes.
- Stale prevention: generated catalogs are linked or regenerated, never hand-copied.

## [2][PROFILES]

Choose one primary profile per page. Split the page when a second profile would force a different contract source or body structure.

| [INDEX] | [PROFILE]                   | [BODY_OWNS]                                      | [OMIT]                               |
| :-----: | :-------------------------- | :----------------------------------------------- | :----------------------------------- |
|   [1]   | Local HTTP contract         | contract link, caller constraints, errors        | hand-copied endpoint catalog         |
|   [2]   | Local CLI/tool contract     | invocation envelope, channels, statuses, effects | flag inventory and command recipes   |
|   [3]   | MCP/tool callable contract  | schema/catalog, setup, fields, failure carrier   | plugin installation workflow         |
|   [4]   | Generated host/API metadata | source key, query shape, output, miss behavior   | broad host/package catalog           |
|   [5]   | Generated library reference | generation record and symbol-family links        | edited generated mirrors             |
|   [6]   | Upstream HTTP API facts     | curated upstream facts and caller impact         | copied endpoint catalog              |
|   [7]   | Upstream SDK/protocol facts | curated upstream facts and local use boundary    | local rail posture or task procedure |

Local and generated profiles name the generated artifact and refresh route. Upstream profiles name the maintained source beside drift-prone facts and never imply local routing of upstream behavior.

## [3][REQUIRED_STRUCTURE]

Use this skeleton, then replace `<PROFILE_BODY>` with the one selected profile section. Do not publish empty optional headings.

```markdown template
# [API_SURFACE]

<Scope: one sentence naming the callable surface, profile, controlling contract or upstream artifact, and generated-output route when one exists.>

## [1][CONTRACT]

## [2][<PROFILE_BODY>]

## [N][API_CHANGE_MAINTENANCE]

## [N][BOUNDARIES]

## [N][VALIDATION]
```

Use these required profile bodies:
- `HTTP_CONTRACT`: local HTTP only; link OpenAPI or checked-in contract output.
- `COMMAND_ENVELOPE`: local CLI/tool only; name invocation prefix, output channels, statuses, side effects, artifact paths, and source truth.
- `TOOL_SCHEMA`: MCP/tool only; name catalog/schema source, authorization/setup, preconditions, field cards, failure carrier, side effects, and evidence.
- `GENERATED_HOST_METADATA`: generated host/API metadata only; name source key, resolution order, query shapes, output fields, empty/miss behavior, artifacts, and refresh route.
- `GENERATED_LIBRARY_REFERENCE`: generated library reference only; name generation record and symbol-family links.
- `EXTERNAL_FACTS`: upstream HTTP, SDK, protocol, or product facts only; name the maintained source or generated proof for every drift-prone fact.

Conditional profile sections appear only when the callable surface exposes the concern: authorization, HTTP mechanics, schemas, errors, async, versioning, examples, generated artifacts, and status/failure contracts.

## [4][CONTRACT_SOURCE]

API prose ranks below the contract or source it describes. Resolve conflicts in this order:
1. Scope README, local instruction files, and tool/source implementation for local agent-callable APIs.
2. Local machine-readable contracts, checked-in schemas, generated reference output, contract tests, generated clients, command output, local XML, and decompile output.
3. Source comments, assemblies, XML documentation comments, generator configuration, source code, and package manifests.
4. Maintained upstream contracts or protocol sources.
5. Curated prose that explains local policy, examples, migration, or caller risk.

Curated prose must not fork generated contracts, generated reference models, local host/API query output, or maintained upstream sources. If prose and source truth disagree, source truth controls and prose is corrected or deleted.

Use [proof.md](../proof.md) for evidence strength, freshness, source conflicts, and proof-field mechanics. API records use proof-local fields in this order when a claim needs them: `Evidence`, `Generated from`, `Source of truth`, `Last verified`, `Review trigger`.

Keep source and proof fields distinct. `Contract source` and `Source of truth` identify controlling artifacts; `Evidence` identifies the observed check or source span that proves the current claim. Do not put a command transcript in `Source of truth`, and do not let a source path stand in for verification when the claim is about generated output, parser behavior, or renderer behavior.

## [5][CONTRACT_RECORDS]

Use one contract record per callable surface or surface family. Omit fields that do not apply; do not replace omitted fields with `n/a`.

```text template
Surface: `<callable surface>`
Profile: `<one profile from PROFILES>`
Contract source: `<OpenAPI file, schema, command envelope, tool catalog, generated artifact, upstream source, source path, XML, or decompile output>`
Generated artifact: `<output path, URL, or generated reference route; omit when absent>`
Invocation or query shape: `<method/path, command prefix, tool name, source key query, or symbol-family route>`
Failure/status carrier: `<HTTP status and body, stdout envelope, stderr diagnostic, exit code, tool error, typed return, exception, miss record, or transport failure>`
Side effects: `<file edits, generated artifacts, runtime state, external state, or omit when none>`
Consumer/toolchain: `<validator, renderer, client, SDK, or generator constraint; omit when unconstrained>`
Route-away: `<command inventory, procedure, runbook, support policy, tutorial, README, architecture, or reference route body>`
Evidence: `<status check, generated output, command result, maintained source, or proof gap>`
Generated from: `<exact generation or query route; omit when manual or external>`
Source of truth: `<contract, schema, source path, generated artifact, XML/decompile output, or maintained upstream source>`
Proof gap: `<missing source, unrun generator, unsupported renderer, or omitted when proved>`
Last verified: YYYY-MM-DD
Review trigger: `<contract, source, generated output, field, status, side effect, toolchain, support row, or upstream source changes>`
```

Use field cards only for inputs, outputs, parameters, envelope members, or schema fields whose parser checks, defaults, source, side effects, or failure behavior matter independently:

```text template
Field: `<name>`
Type/schema: `<type, schema path, enum, or carrier>`
Required/default: `<required, optional, default, or generated>`
Allowed values: `<closed values, range, or units; omit when unconstrained>`
Source: `<contract, schema, source path, XML/decompile output, or generated artifact>`
Field check: `<parser, gate, schema, or proof gap>`
Failure/status behavior: `<status, exit, typed failure, error, or miss behavior>`
Review trigger: `<field, source, schema, parser, or generated output changes>`
```

## [6][PROFILE_RULES]

Profile rules split by API surface:

[LOCAL_HTTP]:
- Use OpenAPI 3.2.0 for new local HTTP contracts unless a named consumer toolchain pins another supported line.
- Link generated operations instead of hand-copying endpoint tables.
- Curate only caller constraints that source truth cannot make obvious: preconditions, authorization constraints, valid state transitions, idempotency, skip conditions, units, ranges, null-or-absent semantics, unsafe values, repairability, and retry-or-abort guidance.
- HTTP mechanics appear only when the contract proves them: authorization, pagination, filtering, sorting, expansion, rate limiting, idempotency, async/long-running operations, schemas, and errors.
- For Problem Details, treat `type` as the problem identifier and HTTP status as transport class; never require callers to parse `detail` as machine-readable data.

[LOCAL_CLI_TOOL]:
- Document invocation prefix, stdout/stderr or return envelope, status values, exit codes, side effects, artifact paths, source truth, and review triggers.
- Route flags, command inventory, and runnable recipes to reference or task routes.
- Keep stdout/stderr, typed return, artifacts, and diagnostics separate when agents parse them differently.
- Use a command card for each callable family that needs independent proof: `Command`, `Invocation`, `Preconditions`, `Inputs`, `Outputs`, `Side effects`, `Exit behavior`, `Evidence`, `Source of truth`, and `Review trigger`.
- Use a CLI envelope record when agents or automation consume channels: `Stdout`, `Stderr`, `Exit status`, `Artifacts`, `External state`, `Diagnostics`, `Failure reading`, and `Close signal`. Do not merge stdout payload, stderr diagnostics, and durable artifacts into one prose field.

[MCP_TOOL]:
- Document catalog/schema source, authorization or connector setup, preconditions, input field cards, output field cards, side effects, failure carrier, evidence, source of truth, and review trigger.
- Route installation, workflow, and recovery elsewhere unless they change the callable schema or preconditions.

[HOST_METADATA]:
- Document source key, resolution order, maintained query shapes, output fields, empty/miss behavior, generated artifacts, and refresh route.
- Use local tooling and generated output as source truth; do not transcribe broad host or package catalogs.
- Route host support and availability to support matrix, task procedure to how-to, and source comments to code documentation.

[LIBRARY_REFERENCE]:
- Mark generated pages or sections as generated and name the generation route.
- Link symbol-family output instead of hand-copying all members.
- When generated symbol text is wrong, repair source comments or generator input first, then regenerate or relink.
- Document success, failure, effects, and real thrown exceptions without implying phantom throws.

[UPSTREAM_FACTS]:
- Name the maintained upstream source or locally generated upstream reference beside facts that can drift.
- Separate stable lookup facts from examples and migration notes.
- Do not copy upstream catalogs or imply local routing of upstream behavior.
- Route local support dates, compatibility, and migration windows to support matrix or how-to.

## [7][API_CHANGE_MAINTENANCE]

Every API page states how caller-facing changes refresh adjacent docs. Use this selector before profile-specific details so agents update the contract first, then only the adjacent routes whose reader action changes.

| [INDEX] | [CHANGE_CLASS]              | [CONTRACT_ACTION]                 | [ROUTE_CHECK]                            |
| :-----: | :-------------------------- | :-------------------------------- | :--------------------------------------- |
|   [1]   | contract source             | regenerate or compare output      | code documentation, architecture, README |
|   [2]   | field or envelope behavior  | update record and examples        | reference, how-to, tutorial              |
|   [3]   | command surface             | update callable contract          | reference, README, runbook               |
|   [4]   | lifecycle or availability   | update lifecycle fact             | support matrix, roadmap                  |
|   [5]   | generated artifact identity | update generation record and link | architecture, README, reference          |

Change classes are routing keys, not complete inventories. When a changed fact spans several classes, apply the highest contract-owning row first, then use the API surface card below for the adjacent routes that still change behavior.

Use API surface cards only when adjacent routes change caller or agent behavior. A surface card links contract routes and adjacent docs; it never lists operations, symbols, flags, or endpoint rows.

```text template
Surface: `<callable surface>`
Profile: `<one profile from PROFILES>`
Changed fact: `<contract, generated surface, command envelope, schema, status, or error behavior>`
Consumed by: `<README, how-to, tutorial, support matrix, reference, code documentation, architecture, or other adjacent route>`
Use in this document: `<caller or agent decision this API page changes>`
Update when: `<contract, generated artifact, command envelope, status, error behavior, or adjacent route changes>`
Close when: `<consuming route updates, route is removed, or fact routes away>`
Route-away: `<adjacent route body; omit untriggered routes>`
```

## [8][VERSIONING]

State versioning and deprecation as one explicit statement per contract, or route support dates to support matrix. Distinguish versioning scheme, deprecation notice, sunset or removal signal, migration target, and support lifecycle date.
Name the maintained source only when a local consumer or support policy depends on that source.

## [9][BOUNDARIES]

- [code-documentation.md](code-documentation.md) carries source-level public symbol comments that generated library reference consumes.
- [reference.md](reference.md) carries curated lookup facts, command inventories, option tables, and status vocabularies that are not callable API contracts.
- [support-matrix.md](support-matrix.md) carries supported versions, compatibility bounds, lifecycle dates, and support-policy deprecation facts.
- [how-to.md](../task/how-to.md) carries procedures that call an API.
- [tutorial.md](../learning/tutorial.md) carries learning paths through API use.
- [readme.md](readme.md) carries first entry routes and route-document maps.
- [architecture.md](../explanation/architecture.md) carries current structure, routing boundaries, and generated-output placement when those facts affect code editing.

## [10][VALIDATION]

Use this verification checklist by group:

[CONTRACT_SOURCE]:
- [ ] The page declares exactly one profile and uses that profile's required body.
- [ ] Contract source, generated artifact, source of truth, and refresh route are named where they exist.
- [ ] Generated contracts and generated reference output are linked or regenerated, not transcribed.
- [ ] Local API/tool source-collection mechanics are linked to their route instead of repeated.

[PROFILE_FACTS]:
- [ ] CLI/tool pages name invocation, output channels or return envelope, statuses, exit codes, side effects, artifact paths, source truth, and review triggers.
- [ ] MCP/tool pages name catalog/schema source, authorization/setup, preconditions, input/output fields, side effects, and failure carrier.
- [ ] Generated host/API pages name source key, resolution order, query shape, output fields, empty/miss behavior, artifact paths, and refresh route.
- [ ] Upstream SDK/test-tool pages distinguish upstream API facts from local rail posture and route command recipes or strategy policy away.

[HTTP_ERRORS_VERSIONING]:
- [ ] HTTP-only sections appear only for HTTP surfaces that expose the concern.
- [ ] OpenAPI pages use OpenAPI 3.2.0 or name the consumer/toolchain pin.
- [ ] Error documentation carries status mapping, body shape, problem identifier or local error code, repairability, and retry guidance.
- [ ] Versioning distinguishes deprecation notice, sunset or removal signal, migration target, and support lifecycle dates.

[BOUNDARIES_PROOF]:
- [ ] API surface cards appear only when adjacent route links change caller or agent behavior.
- [ ] Every ordinary fenced block carries an intent label, and every relative link resolves.
