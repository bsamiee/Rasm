# [REFERENCE_DOCUMENTATION]

Reference documentation is lookup truth for a reader who already knows the domain and needs one exact fact while working. It describes and only describes: facts, fields, commands, terms, limits, statuses, and current constraints. It fails when it teaches a path, argues a rationale, hides procedure in tables, or lets drift-prone claims float without nearby proof.

## [1][USE_WHEN]

Route a page to this standard when the reader extracts a fact rather than follows a path:
- external product, library, SDK, protocol, host, runtime, or package facts.
- command, flag, argument, environment variable, error code, status, and configuration lookup.
- terminology glossaries, abbreviation lists, preferred terms, rejected terms, and aliases.
- data dictionaries, schema dictionaries, field catalogs, and value-domain catalogs.
- capability and limitation facts when support status is one lookup fact among many.
- short examples that illustrate one fact without becoming a procedure.

Route HTTP contracts and generated API surfaces to [api.md](api.md), broad support policy to [support-matrix.md](support-matrix.md), procedures to [how-to.md](../task/how-to.md), operational recovery to [runbook.md](../task/runbook.md), and rationale to explanation documents.

[AUTHORING_CONTRACT]:
- Agent use: decide the lookup profile, state the source model, then publish only facts a reader can extract without following a task path.
- Required produced structure: lead, `Scope`, one or more lookup sets, optional status vocabulary or examples, `Boundaries`, and `Validation`.
- Section cardinality: one primary profile per leaf; lookup sets repeat by subject shape; examples appear only beside a likely misuse.
- Adjacent checks: check API, code-documentation, support-matrix, README, how-to, runbook, tutorial, roadmap, architecture, and ADR only when a fact changes the reader action those routes control.
- Maintenance triggers: update the reference when a source, generated artifact, command output, status vocabulary, field value, maintained source, or adjacent route changes.

## [2][PROFILES]

Choose one primary profile per reference leaf. Split the leaf when a second profile changes source model, entry fields, or lookup order.

[FACT_CATALOG]:
- Carries: grouped facts about one external dependency, runtime, host, product, or local tool surface.
- Required sets: scope and one or more fact sets.
- Entry shape: fact card or homogeneous lookup table.
- Route-away: procedures, support policy, generated API catalogs, and architecture rationale.

[COMMAND_REFERENCE]:
- Carries: command names, flags, arguments, defaults, output shape, exit behavior, side effects, and short misuse examples.
- Required sets: scope and one set per command or command family.
- Entry shape: command family card plus keyed flag or exit-code mappings.
- Route-away: step-by-step task execution and operational recovery.

[GLOSSARY]:
- Carries: terms, abbreviations, aliases, preferred terms, admitted terms, deprecated terms, rejected terms, and related concepts.
- Required sets: scope, term entries grouped by domain or alphabetized.
- Entry shape: glossary term card or short definition block.
- Route-away: explanatory concept essays and external ontology catalogs.

[DATA_DICTIONARY]:
- Carries: data elements with definition, type, value domain, nullability, provenance, routing, and source schema.
- Required sets: scope and element entries.
- Entry shape: data element card or schema-local generated table.
- Route-away: schema files, migrations, generated API contracts, and warehouse catalogs.

[CAPABILITY_REFERENCE]:
- Carries: supported features, limitations, status vocabulary, version constraints, and evidence where support is one fact among many.
- Required sets: scope, status vocabulary, and capability entries.
- Entry shape: status-tagged capability records.
- Route-away: broad support matrices and future roadmap intent.

[SOURCE_MAP_CATALOG]:
- Carries: source-key maps, package graph posture, BCL/shared-framework surfaces, external-library posture, testing-tool facts, and replacement maps.
- Required sets: scope with source model, one or more fact sets, and route-away table.
- Entry shape: fact cards or grouped records when rows need proof/update fields.
- Route-away: callable API contracts, support lifecycle, test strategy, how-to commands, architecture, and source comments.

Use this lookup archetype selector. The record trigger names the first signal that the row needs records instead of a wider table:

| [INDEX] | [ARCHETYPE]               | [USE_FOR]                           | [RECORD_TRIGGER]               |
| :-----: | :------------------------ | :---------------------------------- | :----------------------------- |
|   [1]   | dependency API fact page  | package or SDK facts                | independent member proof       |
|   [2]   | repo posture page         | local adoption leaves               | path or support facts spill    |
|   [3]   | system surface map        | BCL, host, or package maps          | source or review fields appear |
|   [4]   | replacement map           | approved replacement choice         | replacement gate appears       |
|   [5]   | package/tool fact catalog | package graph or testing-tool facts | policy changes reader action   |

## [3][REQUIRED_STRUCTURE]

Order reference leaves to source first, lookup in the body, and boundaries last.

Use this required core:

```markdown template
# [TOPIC]

<One-sentence scope.>

## [1][SCOPE]

## [2][LOOKUP_GROUP]

## [N][BOUNDARIES]

## [N][VALIDATION]
```

Add these conditional sections only when their trigger applies:

```markdown template
## [N][STATUS_VOCABULARY]

## [N][EXAMPLES]
```

[SECTION_CARDINALITY]:
- Opening scope: required, single.
- `Source model`: required inside `Scope` for any reference leaf whose facts can drift, are generated, are package/tool facts, or are sourced from command output.
- `Status vocabulary`: required for capability references; absent otherwise.
- Lookup set: required; one or more, named for the subject being cataloged.
- `Examples`: optional; include only beside a likely misuse.
- `Boundaries`: required, single.
- `Validation`: required, single.

Profile skeletons make the generic `LOOKUP_GROUP` concrete:
- Fact catalog: `Scope`, one or more subject fact sets, `Boundaries`, and `Validation`.
- Command reference: `Scope`, one command-family set per callable family, keyed mappings for flags, arguments, output, exit behavior, and side effects, `Boundaries`, and `Validation`.
- Glossary: `Scope`, term sets by domain or alphabet, alias and status handling where needed, `Boundaries`, and `Validation`.
- Data dictionary: `Scope`, element sets by source system or subject area, source schema links, `Boundaries`, and `Validation`.
- Capability reference: `Scope`, `Status vocabulary`, capability entries, `Boundaries`, and `Validation`.
- Source map or package/tool fact catalog: `Scope` with source model, grouped fact records, route-away table, `Boundaries`, and `Validation`.

Source model records use proof fields only when the source proves drift-prone facts:

```text template
Source model: `<maintained source, generated artifact, command output, manifest, source map, or package/tool corpus>`
Evidence: `<source path, command output, generated artifact, maintained source, or proof gap>`
Generated from: `<command, schema, generator, or omitted when manually sourced>`
Source of truth: `<controlling path, manifest, generated contract, package source, or upstream source>`
Last verified: YYYY-MM-DD
Review trigger: `<source, command, generated artifact, package, support, or route change>`
Refresh command: `<exact command; omit when no command refreshes the facts>`
```

Use a compact source-truth table only when every row shares the same update trigger. Otherwise promote each source or fact set to a definition block.

## [4][REFERENCE_BASELINES]

Reference prose ranks below machine-readable truth:
1. Repository source, generated output, contracts, manifests, schemas, source comments, lockfiles, and runnable command output.
2. Maintained upstream contracts, release notes, and support policies.
3. Source-controlled examples, migration notes, issue records, or known-limitation pages.
4. Curated reference prose in the document.

This standard carries local term statuses such as `preferred`, `admitted`, `deprecated`, and `rejected`. When a generated contract or maintained source changes, re-derive the local summary from the source rather than editing prose in isolation.

Every reference-local status vocabulary must declare exact casing, active values, blocked or unavailable values when present, returnable values when present, terminal values, omitted shared lifecycle states, and removal behavior before the first table or record that uses it. Title-case support or capability labels are display terms; they are not shared lifecycle values unless the record explicitly maps them.

## [5][FACT_ENTRIES]

State only fields a reader needs to use the fact:
- name or canonical term.
- kind, type, category, or route.
- definition or concise description.
- allowed values, units, defaults, constraints, lifecycle status, or support status when the fact constrains a caller.
- source, command, maintained route, or generated artifact when the fact can drift, following [proof.md](../proof.md).
- related entries or canonical local links where useful.

Choose the container by how the reader reads the entry. A single record read by field belongs in a definition block. Homogeneous peer entries compared across the same columns belong in a table within the shared table ceiling. Sparse, heterogeneous, or independently updated entries belong in per-entry definition blocks or subsection records, not a table with many empty cells. Mark absent table values with an em-dash so blanks never read as unknown.

Use a minimal fact card when the fact is stable and only needs lookup fields:

```text template
Name: `<canonical fact, term, command, field, or capability>`
Kind: `<runtime | host | package | command | field | status | ...>`
Definition: `<one factual statement>`
Constraint: `<allowed values, unit, default, or lifecycle; omit when unconstrained>`
Use: `<one sentence naming the reader action this fact changes>`
```

Use a proof-bearing fact card when the fact can drift, routes adjacent work, or changes generated/API/support behavior:

```text template
Name: `<canonical fact, term, command, field, or capability>`
Definition: `<one factual statement>`
Constraint: `<allowed values, unit, default, or lifecycle; omit when unconstrained>`
Use: `<one sentence naming the reader action this fact changes>`
Evidence: `<source, command, generated artifact, or maintained reference>`
Generated from: `<generator, command, or schema; omit when manually sourced>`
Source of truth: `<controlling path, schema, manifest, generated contract, or upstream source>`
Last verified: YYYY-MM-DD
Review trigger: `<event that makes this fact stale>`
Route-away: `<README | API | code documentation | support matrix | how-to | runbook | tutorial | roadmap | architecture; omit when absent>`
```

Omit fields that do not apply, except `Name` and `Definition`. Put run IDs, source versions, and proof gaps in `Evidence` or a visible proof-gap sentence, not in `Last verified`.

Route changed reference facts by the first route whose reader action changes:

| [INDEX] | [CHANGED_FACT]                                                                   | [ROUTE_TO]                                        |
| :-----: | :------------------------------------------------------------------------------- | :------------------------------------------------ |
|   [1]   | callable contract, generated output, input/output/failure carrier                | [api.md](api.md)                                  |
|   [2]   | command flag, lookup value, replacement, package posture, source key             | [reference.md](reference.md)                      |
|   [3]   | ordered procedure input, verification, migration step                            | [how-to.md](../task/how-to.md)                    |
|   [4]   | current path, dependency, scope boundary, invariant, generated-contract boundary | [architecture.md](../explanation/architecture.md) |
|   [5]   | future sequence, milestone, exit proof, deferred work, removal timing            | [roadmap.md](../explanation/roadmap.md)           |
|   [6]   | lifecycle, compatibility, support, deprecation policy                            | [support-matrix.md](support-matrix.md)            |
|   [7]   | operational symptom, rollback, recovery                                          | [runbook.md](../task/runbook.md)                  |
|   [8]   | public symbol semantics, source comments, generated anchors                      | [code-documentation.md](code-documentation.md)    |
|   [9]   | entrypoint or first-use route                                                    | [readme.md](readme.md)                            |

When a reference fact changes a procedure, recovery path, public surface, entry map, support decision, tutorial variant, roadmap sequence, architecture boundary, or symbol contract, update the adjacent route at the point it consumes the fact instead of copying that route into the reference page:

```text template
Changed fact: `<fact anchor and changed value>`
Consumed by: `<README, API, code documentation, support matrix, how-to, runbook, tutorial, roadmap, or architecture path>`
Use in this document: `<lookup, call, migrate, recover, learn, route, or maintain decision>`
Update when: `<fact value, source schema, command output, support status, or generated contract changes>`
Close when: `<consuming route updates or explicitly routes away the fact>`
Route-away: `<content that stays in the adjacent route>`
```

## [6][CAPABILITY_ENTRIES]

A capability reference catalogs a finite set of support facts that change over releases; render that set as status-tagged records. Declare the closed `Status` vocabulary once and draw every entry from it.

Capability entries use these fields:
- `Capability`: feature, limit, or behavior.
- `Status`: support state from the declared vocabulary.
- `Constraint`: version, platform, environment, entitlement, or flag when status is conditional.
- `Evidence`: source, command output, or maintained link when the support fact can drift, with exact evidence details defined by [proof.md](../proof.md).

When status maps deterministically to caller decision, include a compact lookup table keyed by status:

| [INDEX] | [STATUS]               | [DECISION]           | [NOTE]                     |
| :-----: | :--------------------- | :------------------- | :------------------------- |
|   [1]   | `<declared-supported>` | use without guard    | no guard required          |
|   [2]   | `<declared-limited>`   | check constraint     | constraint named in entry  |
|   [3]   | `<declared-retired>`   | route to replacement | replacement named in entry |

The table is conceptual. Replace status names and decisions with the page's declared vocabulary; migration steps route to a how-to or support matrix.

## [7][KEYED_MAPPINGS]

Use lookup tables for direct key-to-value retrieval: exit-code-to-meaning, error-code-to-cause, flag-to-effect, environment-variable-to-purpose, or status-to-policy. Use decision tables only when two or more independent conditions jointly determine an outcome over a finite combination space.

Keep the stub column a short key: identifier, command, code, status token, or term. Move qualifications longer than a cell into a visible note immediately after the table. A mutating command documents exit codes and side effects beside the command, not in surrounding prose.

Use a command family card before flag, argument, or exit-code tables:

```text template
Command family: `<command or subcommand>`
Invocation pattern: `<command> <required-args> [optional-flags]`
Output shape: `<stdout, JSON schema, files, artifacts, or external state>`
Mutation: `<none, filesystem, network, service state, or other>`
Exit behavior: `<exit codes or status contract>`
Route-away: `<how-to for normal task or runbook for recovery; omit when absent>`
```

When a command emits a machine-consumed envelope, add a CLI envelope record before examples:

```text template
Stdout: `<payload, schema, or empty>`
Stderr: `<diagnostics, progress, or empty>`
Exit status: `<success and failure codes>`
Artifacts: `<files, reports, captures, or omit when absent>`
External state: `<service, filesystem, network, runtime, or none when the domain truly allows none>`
Failure reading: `<how an agent distinguishes usage, validation, runtime, and transport failures>`
Review trigger: `<command, output shape, exit status, artifact, or failure behavior changes>`
```

```markdown conceptual
| [INDEX] | [FLAG]      | [EFFECT]        | [SIDE_EFFECT] | [DEFAULT] |
| :-----: | :---------- | :-------------- | :------------ | :-------- |
|   [1]   | `--dry-run` | reports plan    | —             | off       |
|   [2]   | `--out`     | writes artifact | filesystem    | required  |
```

This lookup table is valid because each row maps a flag to caller-facing effect. The how-to carries the ordered task that combines those flags into a workflow.

## [8][GLOSSARIES]

A glossary is a local controlled vocabulary. Every concept resolves to one preferred term inside the bounded context, even when another vocabulary uses different labels.

- Define one sense per entry; split a term with two senses into context-specific entries.
- Mark competing terms with a local closed status set, usually `preferred`, `admitted`, `deprecated`, or `rejected`.
- Redirect every non-preferred term to the preferred term instead of redefining the concept.
- Preserve source names, capitalization, and symbols from the controlling source.
- Define an acronym only when the target reader cannot infer it from the expansion.
- Name the controlling source when a term belongs to another corpus, and route that corpus through `Boundaries`.

This local stricter policy defines the repository vocabulary without claiming another standard requires the same terms. Preferred, alternate, and hidden labels can support local preferred terms, admitted terms, and hidden-search labels. A local `rejected` term is repository policy unless an adopted vocabulary defines a stronger authorization status.

Glossary entries use one sense per card when the term has aliases or status:

```text template
Term: `<preferred term>`
Sense: `<one bounded-context meaning>`
Status: preferred | admitted | deprecated | rejected
Preferred term: `<preferred term; required for non-preferred entries>`
Related: `<term anchors; omit when unrelated>`
```

## [9][DATA_DICTIONARIES]

A data dictionary describes data elements against their controlling schema. The dictionary does not replace schemas, migrations, generated contracts, warehouse catalogs, or API contracts.

Per element, include the subset that applies:

[IDENTITY_MEANING]:
- canonical name and aliases.
- semantic definition.
- route or source system.
- data type, format, unit, precision, or encoding.
- value domain, enumerated values, ranges, nullability, uniqueness, and cardinality when they constrain a writer.
- required status when the element may not be blank or null.

[GOVERNANCE_SOURCE]:
- key, relationship, partition, or lineage facts.
- sensitivity, access class, retention, provenance, quality, and recency facts when applicable.
- source schema, contract, query, generated catalog, or maintained data standard.

Define every coded value's meaning, not just its name. Link the machine-readable schema rather than copying it. Split dictionaries with more than 20 elements by source system or subject area before the table exceeds the row ceiling.

Use a data element card when schema fields need proof or lineage:

```text template
Canonical name: `<field or element name>`
Definition: `<semantic definition, not type echo>`
Type/unit: `<source type, format, precision, or unit>`
Value domain: `<enum, range, pattern, nullability, or required state>`
Source system: `<schema, table, contract, generator, or maintained source path>`
Aliases/lineage: `<aliases, prior names, or source mapping; omit when absent>`
Sensitivity/access: `<classification; omit when unrestricted or not classified>`
```

## [10][EXAMPLES_WARNINGS]

A reference example illustrates one fact, runs at most 12 lines, and never becomes a procedure. Place it beside the fact it clarifies and label the fence with intent.

Use this command fact entry:

```text template
Invocation: `uv run python -m tools.quality static report libs/csharp/Rasm`
Flag: `report`
Effect: reports static diagnostics for the scoped C# project closure.
Side effect: does not intentionally mutate tracked source.
```

Replace placeholders with exact command help, a source path, or a generated command reference before publishing a real command reference.

Rejected warning: This command has a dry-run mode, but the warning omits invocation, effect, and side effect.
Reason: the accepted shape gives the reader one lookup fact plus the information needed to refresh it. The rejected shape hides the invocation, effect, and side effect.

## [11][BOUNDARIES]

[REFERENCE_ROUTES]:
- [api.md](api.md) carries HTTP contracts, OpenAPI descriptions, and generated library API reference.
- [support-matrix.md](support-matrix.md) carries broad support status, lifecycle dates, compatibility bounds, and deprecation policy when support is the policy.
- [code-documentation.md](code-documentation.md) carries source-level public symbol comments and inline rationale.
- [how-to.md](../task/how-to.md) carries step-by-step procedures.
- [runbook.md](../task/runbook.md) carries operational symptom, triage, mitigation, rollback, and recovery.

[ADJACENT_ROUTES]:
- [tutorial.md](../learning/tutorial.md) carries learning paths that consume reference facts.
- [roadmap.md](../explanation/roadmap.md) carries future sequence when a fact changes planned work.
- [architecture.md](../explanation/architecture.md) carries context, structure, invariants, and trade-offs.
- [adr.md](../explanation/adr.md) carries recorded decision rationale.
- [README.md](../README.md) carries document-type routing, placement, and lifecycle.

## [12][VALIDATION]

Use this verification checklist by group:

[PROFILE_SOURCE]:
- [ ] The page describes facts rather than teaching a path or arguing a rationale.
- [ ] One primary profile is chosen and its lookup sets are present.
- [ ] Drift-prone, generated, source-map, package graph, and command-output facts have a source model.
- [ ] External-library pages choose package fact catalog, capability reference, repo posture, or generated/package API fact profile instead of relying on file names.

[ENTRIES]:
- [ ] Lookup sets mirror the subject's shape.
- [ ] Fact cards carry source guidance, route-away, and use fields where they change maintenance behavior.
- [ ] Command family cards name invocation pattern, output shape, mutation, and exit behavior.
- [ ] Glossary entries carry one sense each and redirect non-preferred terms.
- [ ] Data-dictionary elements cite exact source schemas when schema truth can drift.
- [ ] Capability sets use status-tagged records from a declared vocabulary.

[MAPPINGS_EXAMPLES]:
- [ ] Keyed mappings use lookup tables; condition combinations use decision tables.
- [ ] Sparse or heterogeneous entries use records instead of unnecessary tables.
- [ ] Examples illustrate one fact and every ordinary fenced block carries an intent label.

[BOUNDARIES]:
- [ ] Procedures, learning paths, future sequence, rationale, broad support policy, and generated API catalogs route through boundaries when reference facts change reader action.
- [ ] Every relative link resolves.
