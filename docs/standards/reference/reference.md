# [REFERENCE_DOCUMENTATION]

Reference documentation is lookup truth for a reader who already knows the domain and needs one exact fact while working. It describes and only describes: facts, fields, commands, terms, limits, statuses, and current constraints. It fails when it teaches a path, argues a rationale, hides procedure in tables, or lets drift-prone claims float without nearby proof.

## [1][USE_WHEN]

Route a page to this standard when the reader extracts a fact rather than follows a path:

- external product, library, SDK, protocol, host, runtime, or package facts;
- command, flag, argument, environment variable, error code, status, and configuration lookup;
- terminology glossaries, abbreviation lists, preferred terms, rejected terms, and aliases;
- data dictionaries, schema dictionaries, field catalogs, and value-domain catalogs;
- capability and limitation facts when support status is one lookup fact among many;
- short examples that illustrate one fact without becoming a procedure.

Route HTTP contracts and generated API surfaces to [api.md](api.md), broad support policy to [support-matrix.md](support-matrix.md), procedures to [how-to.md](../task/how-to.md), operational recovery to [runbook.md](../task/runbook.md), and rationale to explanation documents.

## [2][REFERENCE_BASELINES]

Reference prose ranks below machine-readable and official truth:

1. Repository source, generated output, contracts, manifests, schemas, metadata, lockfiles, and runnable command output.
2. Official specifications, standards, vendor reference docs, release notes, and support policies.
3. Maintainer-controlled examples, migration notes, issue records, or known-limitation pages.
4. Curated reference prose in the document.

Reference quadrant
    Source of truth: [Diátaxis Reference](https://diataxis.fr/reference/).
    Last verified: 2026-06-04
    Review trigger: Diátaxis reference guidance changes.

Data definitions
    Source of truth: [ISO/IEC 11179-4:2004](https://www.iso.org/standard/35346.html) for data-definition formulation.
    Last verified: 2026-06-04
    Review trigger: ISO 11179-4 status or replacement changes.

Data naming
    Source of truth: [ISO/IEC 11179-5:2015](https://www.iso.org/standard/60341.html) for naming principles; ISO currently marks the publication as published and to be revised.
    Last verified: 2026-06-04
    Review trigger: ISO 11179-5 revision publishes or status changes.

Local glossary policy
    Source of truth: this standard for local term statuses such as `preferred`, `admitted`, `deprecated`, and `rejected`.
    Review trigger: local terminology policy or status vocabulary changes.

SKOS label mapping
    Source of truth: [SKOS](https://www.w3.org/TR/skos-reference/) preferred, alternate, and hidden lexical labels.
    Last verified: 2026-06-04
    Review trigger: SKOS Recommendation or local terminology-status vocabulary changes.

When a generated contract or official source changes, re-derive the local summary from the source rather than editing prose in isolation.

ISO 11179 informs data-dictionary definitions and naming. Do not claim ISO conformance, exact ISO field requirements, or a stricter naming rule unless the exact standard text is verified during the change and cited beside the rule.

## [3][PROFILES]

Choose one primary profile per reference leaf. Split the leaf when a second profile changes source truth, entry fields, or lookup order.

Fact catalog
    Owns: grouped facts about one external dependency, runtime, host, product, or local tool surface.
    Required groups: scope, source truth, one or more fact groups.
    Entry shape: fact card or homogeneous lookup table.
    Route-away: procedures, support policy, generated API catalogs, and architecture rationale.

Command reference
    Owns: command names, flags, arguments, defaults, output shape, exit behavior, side effects, and short misuse examples.
    Required groups: scope, source truth, one group per command or command family.
    Entry shape: command family card plus keyed flag or exit-code mappings.
    Route-away: step-by-step task execution and operational recovery.

Glossary
    Owns: terms, abbreviations, aliases, preferred terms, admitted terms, deprecated terms, rejected terms, and related concepts.
    Required groups: scope, term entries grouped by domain or alphabetized.
    Entry shape: glossary term card or short definition block.
    Route-away: explanatory concept essays and external ontology catalogs.

Data dictionary
    Owns: data elements with definition, type, value domain, nullability, provenance, ownership, and source schema.
    Required groups: scope, source truth, element entries.
    Entry shape: data element card or schema-owned generated table.
    Route-away: schema files, migrations, generated API contracts, and warehouse catalogs.

Capability reference
    Owns: supported features, limitations, status vocabulary, version constraints, and evidence where support is one fact among many.
    Required groups: scope, status vocabulary, capability entries, source truth.
    Entry shape: status-tagged capability records.
    Route-away: broad support matrices and future roadmap intent.

## [4][REQUIRED_STRUCTURE]

Order reference leaves to source first, lookup in the body, and boundaries last.

Required core:

```markdown template
# [TOPIC]

<One-sentence scope.>

## [1][CONTROL]

## [2][LOOKUP_GROUP]

## [N][BOUNDARIES]

## [N][REVIEW_CHECKLIST]
```

Conditional additions:

```markdown template
## [N][STATUS_VOCABULARY]

## [N][EXAMPLES]

## [N][EVIDENCE]
```

Section cardinality:

- Opening scope: required, single.
- `Source truth`: required for fact catalogs, command references, data dictionaries, capability references, and any drift-prone glossary; glossaries backed only by stable local policy may inline source truth in entries.
- `Status vocabulary`: required for capability references; absent otherwise.
- Lookup group: required; one or more, named for the subject being cataloged.
- `Examples`: optional; include only beside a likely misuse.
- `Evidence`: page-level only when every fact shares one source and trigger; otherwise omit and attach proof beside each fact.
- `Boundaries`: required, single.
- `Review checklist`: required, single.

## [5][FACT_ENTRIES]

State only fields a reader needs to use the fact:

- name or canonical term;
- kind, type, category, or owner;
- definition or concise description;
- allowed values, units, defaults, constraints, lifecycle status, or support status when the fact constrains a caller;
- source truth or evidence when the fact can drift;
- `Last verified: YYYY-MM-DD` or `Review trigger:` when the fact can drift;
- related entries or canonical external links where useful.

Choose the container by how the reader reads the entry. A single record read by field belongs in a definition block. Homogeneous peer entries compared across the same columns belong in a table within the shared table ceiling. Sparse, heterogeneous, or independently updated entries belong in per-entry definition blocks or subsection records, not a table with many empty cells. Mark absent table values with an em-dash so blanks never read as unknown.

Use a fact card when a fact has proof, route, or refresh behavior an agent must preserve:

```text template
Name: `<canonical fact, term, command, field, or capability>`
Kind: `<runtime | host | package | command | field | status | ...>`
Definition: `<one factual statement>`
Constraint: `<allowed values, unit, default, or lifecycle; omit when unconstrained>`
Source of truth: `<source path, generated contract, official source, or command>`
Evidence: `<exact proof; omit only for stable local term policy>`
Review trigger: `<source, generated output, command help, or owner change>`
Route-away: `<api | support matrix | how-to | runbook | architecture; omit when absent>`
Use: `<one sentence naming the reader action this fact changes>`
```

Omit fields that do not apply, except `Name`, `Definition`, `Source truth`, and `Review trigger` when the fact can drift.

## [6][CAPABILITY_ENTRIES]

A capability reference catalogs a finite set of support facts that change over releases; render that set as status-tagged records. Declare the closed `Status` vocabulary once and draw every entry from it.

Capability entry fields:

- `Capability`: feature, limit, or behavior.
- `Status`: support state from the declared vocabulary.
- `Constraint`: version, platform, environment, entitlement, or flag when status is conditional.
- `Evidence`: source, command output, or official link proving the status.
- `Review trigger:` or `Last verified:`: event or date that makes the fact stale.

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
Source of truth: `<source path, generated command reference, or --help output>`
Refresh command: `<command that prints current help; omit when unavailable>`
Invocation pattern: `<command> <required-args> [optional-flags]`
Output shape: `<stdout, JSON schema, files, artifacts, or external state>`
Mutation: `<none, filesystem, network, service state, or other>`
Exit behavior: `<exit codes or status contract>`
Route-away: `<how-to for normal task or runbook for recovery; omit when absent>`
Review trigger: command implementation, help output, schema, or side-effect path changes.
```

## [8][GLOSSARIES]

A glossary is a local controlled vocabulary. Every concept resolves to one preferred term inside the bounded context, even when external vocabulary practice uses different labels.

- Define one sense per entry; split a term with two senses into context-specific entries.
- Mark competing terms with a local closed status set, usually `preferred`, `admitted`, `deprecated`, or `rejected`.
- Redirect every non-preferred term to the preferred term instead of redefining the concept.
- Preserve official names, capitalization, and symbols from the owning source.
- Define an acronym only when the target reader cannot infer it from the expansion.
- Name the owning domain when a term belongs to another corpus, and route that corpus through `Boundaries`.

This local stricter policy maps to external practice rather than claiming external standards require the same terms. SKOS-style preferred, alternate, and hidden labels can support local preferred terms, admitted terms, and hidden-search labels. A local `rejected` term is repository policy unless a named adopted vocabulary defines a stronger authorization status.

Glossary entries use one sense per card when the term has aliases or status:

```text template
Term: `<preferred term>`
Sense: `<one bounded-context meaning>`
Status: preferred | admitted | deprecated | rejected
Preferred term: `<preferred term; required for non-preferred entries>`
Related: `<term anchors; omit when unrelated>`
Source of truth: `<local policy, official vocabulary, or source path>`
Review trigger: local terminology policy or owning vocabulary changes.
```

## [9][DATA_DICTIONARIES]

A data dictionary describes data elements against their owning schema. ISO/IEC 11179-4:2004 informs semantic definitions; ISO/IEC 11179-5:2015 informs naming principles. The dictionary does not replace schemas, migrations, generated contracts, warehouse catalogs, or API contracts.

Per element, include the subset that applies:

- canonical name and aliases;
- semantic definition;
- owner or source system;
- data type, format, unit, precision, or encoding;
- value domain, enumerated values, ranges, nullability, uniqueness, and cardinality when they constrain a writer;
- required status when the element may not be blank or null;
- key, relationship, partition, or lineage facts;
- sensitivity, access class, retention, provenance, quality, and freshness facts when applicable;
- source schema, contract, query, generated catalog, or official data standard.

Define every coded value's meaning, not just its name. Link the machine-readable schema rather than copying it. Split dictionaries with more than 20 elements by source system or subject area before the table exceeds the row ceiling.

Use a data element card when schema fields need proof or lineage:

```text template
Canonical name: `<field or element name>`
Definition: `<semantic definition, not type echo>`
Type/unit: `<source type, format, precision, or unit>`
Value domain: `<enum, range, pattern, nullability, or required state>`
Owner/source system: `<owner, schema, table, contract, or generator>`
Aliases/lineage: `<aliases, prior names, or source mapping; omit when absent>`
Sensitivity/access: `<classification; omit when unrestricted or not classified>`
Evidence: `<schema path, generated catalog, query, or official data standard>`
Review trigger: schema, generator, source-system, or data-contract change.
```

## [10][EXAMPLES_WARNINGS]

A reference example illustrates one fact, runs at most 12 lines, and never becomes a procedure. Place it beside the fact it clarifies and label the fence with intent.

Command fact entry:

```text template
Invocation: `example-tool plan --dry-run`
Flag: `--dry-run`
Effect: reports planned changes.
Side effect: mutates nothing.
Evidence: `<tool help output or command reference path>`.
Review trigger: command help or implementation changes.
```

Replace the evidence placeholder with exact command help, a source path, or a generated command reference before publishing a real command reference.

Rejected prose-only warning:

```text rejected
This tool has a dry-run mode that can be safer in some cases.
```

The accepted shape gives the reader one lookup fact plus proof. The rejected shape hides the invocation, effect, side effect, and refresh trigger.

## [11][BOUNDARIES]

- [api.md](api.md) owns HTTP contracts, OpenAPI descriptions, and generated library API reference.
- [support-matrix.md](support-matrix.md) owns broad support status, lifecycle dates, compatibility bounds, and deprecation policy when support is the policy.
- [code-documentation.md](code-documentation.md) owns source-level public symbol comments and inline rationale.
- [how-to.md](../task/how-to.md) owns step-by-step procedures.
- [runbook.md](../task/runbook.md) owns operational symptom, triage, mitigation, rollback, and recovery.
- [architecture.md](../explanation/architecture.md) owns context, structure, invariants, and trade-offs.
- [adr.md](../explanation/adr.md) owns recorded decision rationale.
- [README.md](../README.md) owns document-type routing, placement, and lifecycle.

## [12][REVIEW_CHECKLIST]

- [ ] The page describes facts rather than teaching a path or arguing a rationale.
- [ ] One primary profile is chosen and its lookup groups are present.
- [ ] Lookup groups mirror the subject's shape.
- [ ] Fact cards carry source truth, evidence or proof gap, review trigger, route-away, and use fields where they change maintenance behavior.
- [ ] Command family cards name invocation pattern, output shape, mutation, exit behavior, and refresh command where available.
- [ ] Glossary entries carry one sense each and redirect non-preferred terms.
- [ ] Data-dictionary elements cite exact source schemas and treat ISO 11179 as informing guidance unless exact standard text is verified.
- [ ] Capability sets use status-tagged records from a declared vocabulary.
- [ ] Keyed mappings use lookup tables; condition combinations use decision tables.
- [ ] Sparse or heterogeneous entries use records instead of unnecessary tables.
- [ ] Examples illustrate one fact and every ordinary fenced block carries an intent label.
- [ ] Procedures, rationale, broad support policy, and generated API catalogs route through boundaries.
- [ ] Every relative link resolves.
