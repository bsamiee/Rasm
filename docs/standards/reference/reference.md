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

## [2][SOURCE_AUTHORITY]

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

## [3][PROFILES]

Choose one primary profile per reference leaf. Split the leaf when a second profile changes source truth, entry fields, or lookup order.

### [3.1][FACT_CATALOG]

Owns: grouped facts about one external dependency, runtime, host, product, or local tool surface.
Required groups: scope, source truth, one or more fact groups.
Entry fields: name; kind or owner; definition; constraints, defaults, or lifecycle status where applicable; evidence when drift-prone.

### [3.2][COMMAND_REFERENCE]

Owns: command names, flags, arguments, defaults, output shape, exit behavior, side effects, and at most one short example where misuse is likely.
Required groups: scope, source truth, one group per command or command family.
Entry fields: invocation; flags and arguments; defaults where present; exit and side effects when the command mutates state; example only where it clarifies one fact.

### [3.3][GLOSSARY]

Owns: terms, abbreviations, aliases, preferred terms, admitted terms, deprecated terms, rejected terms, and related concepts.
Required groups: scope, term entries grouped by domain or alphabetized.
Entry fields: term; sense; status when terms compete; preferred term for every non-preferred term; related terms where useful.

### [3.4][DATA_DICTIONARY]

Owns: data elements with definition, type, value domain, nullability, provenance, ownership, and source schema.
Required groups: scope, source truth, element entries.
Entry fields: canonical name; definition; type or unit; value domain and nullability; owner and source schema; aliases and lineage where applicable.

### [3.5][CAPABILITY_REFERENCE]

Owns: supported features, limitations, status vocabulary, version constraints, and evidence where support is one fact among many.
Required groups: scope, status vocabulary, capability entries, source truth.
Entry fields: capability; status from the declared vocabulary; version or environment constraint when conditional; evidence and freshness field.

## [4][REQUIRED_STRUCTURE]

Order reference leaves to source first, lookup in the body, and boundaries last.

Required core:

```markdown template
# [TOPIC]

<One-sentence scope.>

## [1][LOOKUP_GROUP]

## [N][BOUNDARIES]

## [N][REVIEW_CHECKLIST]
```

Conditional additions:

```markdown template
## [N][SOURCE_TRUTH]

## [N][STATUS_VOCABULARY]

## [N][EXAMPLES]

## [N][EVIDENCE]
```

Section cardinality:

- Opening scope: required, single.
- `Source truth`: required when any fact can drift; single.
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

## [6][CAPABILITY_ENTRIES]

A capability reference catalogs a finite set of support facts that change over releases; render that set as status-tagged records. Declare the closed `Status` vocabulary once and draw every entry from it.

Capability entry fields:

- `Capability`: feature, limit, or behavior.
- `Status`: support state from the declared vocabulary.
- `Constraint`: version, platform, environment, entitlement, or flag when status is conditional.
- `Evidence`: source, command output, or official link proving the status.
- `Review trigger:` or `Last verified:`: event or date that makes the fact stale.

When status maps deterministically to caller action, include a compact lookup table keyed by status:

| [INDEX] | [STATUS] | [ACTION]      | [NOTE]                          |
| :-----: | :------- | :------------ | :------------------------------ |
|   [1]   | Current  | call directly | no guard required               |
|   [2]   | Legacy   | migrate       | replacement named in entry      |
|   [3]   | Blocked  | guard         | fallback required by constraint |

The table is conceptual. Replace status names and actions with the page's declared vocabulary.

## [7][KEYED_MAPPINGS]

Use lookup tables for direct key-to-value retrieval: exit-code-to-meaning, error-code-to-cause, flag-to-effect, environment-variable-to-purpose, or status-to-policy. Use decision tables only when two or more independent conditions jointly determine an outcome over a finite combination space.

Keep the stub column a short key: identifier, command, code, status token, or term. Move qualifications longer than a cell into a visible note immediately after the table. A mutating command documents exit codes and side effects beside the command, not in surrounding prose.

## [8][GLOSSARIES]

A glossary is a local controlled vocabulary. Every concept resolves to one preferred term inside the bounded context, even when external vocabulary practice uses different labels.

- Define one sense per entry; split a term with two senses into context-specific entries.
- Mark competing terms with a local closed status set, usually `preferred`, `admitted`, `deprecated`, or `rejected`.
- Redirect every non-preferred term to the preferred term instead of redefining the concept.
- Preserve official names, capitalization, and symbols from the owning source.
- Define an acronym only when the target reader cannot infer it from the expansion.
- Name the owning domain when a term belongs to another corpus, and route that corpus through `Boundaries`.

This local stricter policy maps to external practice rather than claiming external standards require the same terms. SKOS-style preferred, alternate, and hidden labels can support local preferred terms, admitted terms, and hidden-search labels. A local `rejected` term is repository policy unless a named adopted vocabulary defines a stronger authorization status.

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
- [ ] Required entry fields are present with claim-level proof for drift-prone facts.
- [ ] Glossary entries carry one sense each and redirect non-preferred terms.
- [ ] Data-dictionary elements cite exact source schemas and use ISO 11179 parts accurately.
- [ ] Capability sets use status-tagged records from a declared vocabulary.
- [ ] Keyed mappings use lookup tables; condition combinations use decision tables.
- [ ] Sparse or heterogeneous entries use records instead of unnecessary tables.
- [ ] Examples illustrate one fact and every fenced block carries an intent label.
- [ ] Procedures, rationale, broad support policy, and generated API catalogs route through boundaries.
- [ ] Every relative link resolves.
