# [REFERENCE_DOCUMENTATION]

Reference documentation describes what is true so a reader who already knows the domain looks up an exact fact while working; it describes and only describes. Order facts by the shape of the subject, attach claim-level evidence to every drift-prone fact, and keep instruction and explanation out of the retrieval path. A reference leaf wins when a working reader extracts one fact in one scan; it fails the moment it teaches a path, runs a procedure, or argues a rationale. The binding constraint closes every leaf: each drift-prone fact carries claim-level proof and a freshness trigger, and `Boundaries` routes the path, the rationale, and the policy to their owners.

This standard follows the Diataxis reference quadrant — neutral, product-led description consulted rather than read — and grounds its data-dictionary and glossary rules in ISO/IEC 11179 and controlled-vocabulary practice. `Source of truth:` Diataxis reference quadrant (`diataxis.fr/reference`); ISO/IEC 11179-4 data definitions and ISO/IEC 11179-5:2015 naming principles; controlled-vocabulary preferred-term conventions. `Last verified:` 2026-06-04. `Review trigger:` Diataxis, ISO/IEC 11179, or controlled-vocabulary guidance changes.

## [1][USE_WHEN]

Route a draft to this standard when the reader extracts a fact rather than follows a path:

- external product, library, SDK, protocol, host, runtime, or package facts;
- command, flag, argument, environment variable, error code, status, and configuration lookup;
- terminology glossaries, abbreviation lists, data dictionaries, schema dictionaries, and field catalogs;
- capability and limitation facts when one support fact sits among many other facts and a full policy matrix is not the point;
- examples of at most 12 lines that illustrate one fact without becoming a procedure.

Route elsewhere by topic: HTTP contracts and generated library API surfaces go to API documentation; support status that is itself the policy goes to the support matrix; broad support lifecycle and document-type placement go to the index. The Boundaries section carries those links.

## [2][SOURCE_TRUTH]

Reference prose ranks below every machine-readable and official source. Order reference truth strongest first, and link the stronger source rather than forking it:

1. Repository source, generated output, contracts, manifests, schemas, metadata, lockfiles, and runnable command output.
2. Official specifications, standards, vendor reference docs, release notes, and support policies.
3. Maintainer-controlled examples, migration notes, issue records, or known-limitation pages from the owning project.
4. Curated reference prose in this document.

When a generated contract or official spec changes, the local summary is stale; re-derive it from the source rather than editing the prose in isolation. Curated prose summarizes only the facts a local reader looks up and links the controlling source for the rest.

## [3][PROFILES]

Choose one primary profile per leaf; a leaf that becomes two profiles is two documents. Each profile pins required lookup groups and the field set its entries carry. Read the profile as a record: its `Owns`, `Required groups`, and `Entry fields` are the lookup keys an author reads by field.

### [3.1][FACT_CATALOG]

Owns: grouped facts about one external dependency, runtime, host, or local tool surface.
Required groups: scope, `Source of truth`, one or more named fact groups.
Entry fields: name (required); kind or owner (required); definition (required); constraints or defaults (optional); evidence (required when drift-prone).

### [3.2][COMMAND_REFERENCE]

Owns: command names, flags, arguments, defaults, output shape, exit behavior, side effects, and one example per command.
Required groups: scope, `Source of truth`, one group per command or command family.
Entry fields: invocation (required); flags and arguments (repeatable); default (optional); exit and side effects (required when a command mutates state); example (optional).

### [3.3][GLOSSARY]

Owns: terms, abbreviations, aliases, preferred and rejected terms, and related concepts.
Required groups: scope, term entries grouped or alphabetized.
Entry fields: term (required); sense (required, one per entry); status — preferred, admitted, deprecated, or rejected (required when terms compete); related terms (optional, repeatable).

### [3.4][DATA_DICTIONARY]

Owns: data elements with type, value domain, nullability, provenance, ownership, and source schema.
Required groups: scope, `Source of truth`, element entries.
Entry fields: canonical name (required); definition (required); type or unit (required); value domain and nullability (required); owner and source schema (required); aliases and lineage (optional, repeatable).

### [3.5][CAPABILITY_REFERENCE]

Owns: supported features, limitations, status vocabulary, version constraints, and evidence, where one support fact sits among many.
Required groups: scope, `Status vocabulary`, capability entries, `Source of truth`.
Entry fields: capability (required); status (required, drawn from the status vocabulary); version or environment constraint (optional); evidence (required).

## [4][REQUIRED_STRUCTURE]

Order the headings to the position ring: scope and source first, lookup groups in the body, evidence and route-away last. Copy this template and fill the lookup groups for the chosen profile; the cardinality tag after each heading states whether it is required, optional, conditional, or repeatable.

```markdown template
# [TOPIC_REQUIRED_ONE]

<One-sentence scope.>              (required, one)

## [1][SOURCE_TRUTH_CONDITIONAL]

## [2][STATUS_VOCABULARY_CONDITIONAL]

## [3][LOOKUP_GROUP_REQUIRED]

## [4][EXAMPLES_OPTIONAL_ONLY]

## [5][EVIDENCE_CONDITIONAL_PRESENT]

## [6][BOUNDARIES_REQUIRED_ONE]

```

Section cardinality, stated as the rule the template encodes:

- `Source of truth`: required when any fact can drift; one per leaf.
- `Status vocabulary`: required in a capability reference; absent otherwise.
- Lookup group: required; one or more, named for the subject it catalogs, ordered by the subject's shape so the documentation mirrors the product.
- `Examples`: optional; include only beside a fact a reader is likely to misuse.
- `Evidence`: claim-level by default — attach proof beside each drift-prone fact, table row, or example, not in a page-level section. Add a page-level `## Evidence` block only when every fact shares one source and one review trigger; in the common case (facts with differing sources) there is no `## Evidence` section at all.
- `Boundaries`: required; one link per adjacent owner.

## [5][FACT_ENTRIES]

State only the fields a reader needs to use the fact, and drop fields that carry no decision. Mark each field by cardinality so an author knows what a complete entry holds:

- name or canonical term (required);
- kind, type, category, or owner (required);
- definition or concise description (required);
- allowed values, units, defaults, constraints, lifecycle status, or support status (optional; required when the fact constrains a caller);
- source of truth or evidence (required when the fact can drift);
- `Last verified: YYYY-MM-DD` or `Review trigger:` (required when the fact can drift; one per drift-prone entry);
- related entries or canonical external links (optional, repeatable).

Choose the container by how the reader reads the entry. A single record read by field belongs in a definition block, one `label: value` per line, not a one-row table — the name-value form a glossary or metadata list is built from. A set of peer entries compared across the same columns belongs in a table within the 15-column and 20-row ceiling; past either bound, split the table by group, status, or source. Sparse entries with many empty cells stay tabular, not flattened into prose. Mark every absent value with an em-dash so a blank cell never reads as unknown.

## [6][STATUS_TAGGED_CAPABILITY]

A capability reference catalogs a finite enumerable set of support facts that change over releases; render that set as status-tagged records, never as flat prose. Each capability is a record carrying machine-readable fields an agent filters on. Declare the closed `Status` vocabulary once in `Status vocabulary` and draw every entry's status from it; do not invent a status token per row.

Recurring fields per capability entry:

- `Capability`: the feature, limit, or behavior named for the subject — required.
- `Status`: the support state from the declared vocabulary — required.
- `Constraint`: the version, platform, environment, or flag that bounds the status — required when the status is conditional.
- `Evidence`: the source, command output, or official link that proves the status — required.
- `Review trigger:` or `Last verified:`: the event or date that makes the fact stale — required.

Render homogeneous, short-celled capability sets as a record table within the table ceiling; escalate to a per-item record block once any entry needs a list, a code block, or more than five fields, or once entries are updated independently. When a status maps deterministically to a caller action — supported, deprecated, removed, or blocked-pending-upgrade — carry that map as a lookup table keyed by status, so a reader resolves the action in one scan rather than reconstructing it from prose.

Show the three artifacts the prose names — the declared vocabulary line, one capability record carrying the five recurring fields, and the status-to-action map — because the status vocabulary, the per-capability field set, and the action mapping are the structures most often collapsed into flat prose:

```markdown conceptual
## [1][STATUS_VOCABULARY]

Status: Supported | Deprecated | Removed | Blocked-pending-upgrade

### [1.1][GPU_MESH_TESSELLATION]

Capability: Parallel GPU tessellation of NURBS-derived meshes.
Status: Blocked-pending-upgrade
Constraint: Requires RhinoWIP 8.16+ and a Metal-capable host; absent on the Intel CI runner.
Evidence: `uv run python -m tools.quality api show Rhino.Render.MeshProvider` (returns the gated symbol).
Review trigger: re-verify when the host bundle pin in `Directory.Build.props` advances.

| [INDEX] | [STATUS]                | [CALLER_ACTION]                                                      |
| :-----: | :---------------------- | :------------------------------------------------------------------- |
|   [1]   | Supported               | Call directly; no guard required.                                    |
|   [2]   | Deprecated              | Migrate to the replacement named in the entry; do not add new calls. |
|   [3]   | Removed                 | Remove the call; the symbol no longer resolves.                      |
|   [4]   | Blocked-pending-upgrade | Gate behind the version constraint and fall back on older hosts.     |
```

The next block is `rejected`: it dissolves the same facts into prose, so no status token is enumerable, no field is filterable, and no action resolves in one scan.

```markdown rejected
GPU tessellation is currently not available everywhere — it needs a recent
RhinoWIP and a Metal machine, and on our CI box it just is not there yet, so
for now you should probably guard it and fall back when it is missing.
```

## [7][COMMAND_CODE_CONDITION]

A command reference and a capability reference both carry mappings a reader resolves by key; render the mapping as the table form that matches the question.

- Lookup table: a flat key-to-value mapping optimized for direct retrieval — exit-code-to-meaning, error-code-to-cause, flag-to-effect, environment-variable-to-purpose, status-to-policy. Use it whenever a discrete key resolves to one value, behavior, or next state.
- Decision table: rows are condition combinations, left columns the inputs and the right column the resulting behavior. Use it when two or more independent conditions jointly determine an outcome over a finite combination space — for example, when platform and flag together decide whether a command mutates state.

Keep the stub column a short, unique, scannable key — an identifier, command, code, or status token, never a sentence. Carry any qualifier longer than a cell in a footnote or a notes block after the table. A command that mutates state documents its exit codes and side effects as a lookup table beside the command, not buried in prose.

## [8][GLOSSARIES]

A glossary standardizes terminology against the owning source and is the canonical name-value form: one term, its sense, and its status. It is a controlled vocabulary, so every concept resolves to exactly one preferred term:

- Define one sense per entry; split a term with two senses into two entries keyed by context.
- Mark each competing term's status from the closed set preferred, admitted, deprecated, or rejected whenever two terms compete for one concept, and redirect every non-preferred term to the preferred one rather than re-defining the concept.
- Preserve official names, capitalization, and symbols from the owning source exactly.
- Define an acronym only when the target reader cannot infer it from the expansion.
- Name the owning domain in body prose when a term belongs to another corpus, and carry the cross-link in Boundaries.

## [9][DATA_DICTIONARIES]

A data dictionary describes data elements and their metadata against ISO/IEC 11179: each element is named by its naming principles, defined by its data-definition rules, and bounded by a value domain. It does not replace schemas, migrations, generated contracts, warehouse catalogs, or API contracts; it links the machine-readable schema and catalogs the local lookup facts.

Per element, include the subset that applies and mark cardinality:

- canonical name (required) and aliases (optional, repeatable);
- semantic definition (required);
- owner or source system (required);
- data type, format, unit, precision, or encoding (required);
- value domain, enumerated values, ranges, nullability, uniqueness, and cardinality (required when the element constrains a writer);
- required-status, meaning whether the element may be blank or null (required when the element is mandatory);
- key, relationship, partition, or lineage facts (optional);
- sensitivity, access class, retention, provenance, quality, and freshness facts (optional; required when the element is access-controlled or drift-prone);
- source schema, contract, query, generated catalog, or official data standard (required).

Define each enumerated or coded value's meaning, not just its name, so a coded domain is self-describing. Link the machine-readable schema rather than copying it. A dictionary with more than 20 elements splits by source system or subject area before it crosses the table-row ceiling.

## [10][EXAMPLES_WARNINGS]

A reference example illustrates one fact, runs at most 12 lines, and sits beside the fact it clarifies; it shows shape and never becomes a procedure. Label every fenced block with its intent so a reader knows whether to run it, study it, or avoid it: `copy-safe` for a block safe to run as written, `conceptual` for an illustrative shape, `output-only` for sample output, `deprecated` for a retained-for-recognition form, and `rejected` for a counter-example. Move multi-step usage to a how-to guide and keep the cross-link in Boundaries.

An exemplary reference example is the shape the paragraph above describes: it sits beside the one fact it clarifies, carries its intent in the info string, and shows form without teaching a path. The block below illustrates the flag fact it accompanies — `-n` performs a dry run and changes nothing — and is safe to run as written, so it carries the `copy-safe` label. The rejected form uses `-f`, which mutates state.

```bash copy-safe
# beside the fact: `--dry-run` lists what would be removed and changes nothing.

git clean -xdn
```

A warning belongs in a reference leaf when it states a constraint, destructive behavior, an unsupported combination, a security-sensitive fact, or a likely misuse. State it before the action it governs, keep it factual, and attach the evidence that proves it. The counter-example carries the `rejected` label in its info string, matching the intent-label convention every fenced block obeys.

```text rejected
# deletes the working tree with no confirmation

git clean -xdf
```

## [11][BOUNDARIES]

- [api.md](api.md) owns HTTP contracts, OpenAPI descriptions, and generated library API reference.
- [support-matrix.md](support-matrix.md) owns broad support status, lifecycle dates, compatibility bounds, and deprecation policy when support is the policy.
- [code-documentation.md](code-documentation.md) owns source-level public symbol comments and rationale.
- [../task/how-to.md](../task/how-to.md) owns the step-by-step procedure that a reference example only illustrates.
- [../task/runbook.md](../task/runbook.md) owns operational symptom, triage, mitigation, rollback, and recovery.
- [../explanation/architecture.md](../explanation/architecture.md) owns context, structure, invariants, and trade-offs that a fact does not explain.
- [../explanation/adr.md](../explanation/adr.md) owns the recorded reason a decision was made.
- [README.md](../README.md) owns document-type routing, reader-need classification, placement, and lifecycle.

## [12][REVIEW_CHECKLIST]

- [ ] A one-sentence scope leads the page and the page describes facts rather than teaching a path or arguing a rationale.
- [ ] One primary profile is chosen and its required lookup groups are present.
- [ ] Lookup groups mirror the subject, ordered by the subject's shape.
- [ ] Every required entry field is present and cardinality holds.
- [ ] Glossary entries carry one sense each, mark competing terms by status, and redirect non-preferred terms to the preferred one.
- [ ] Data-dictionary elements use canonical names, cite the owning schema, and define each coded value's meaning.
- [ ] Capability and limitation sets use status-tagged records drawn from a declared status vocabulary, never flat prose.
- [ ] Status-to-action and key-to-value mappings use lookup tables; condition-combination outcomes use decision tables.
- [ ] Command entries that mutate state document exit codes and side effects beside the command.
- [ ] Each drift-prone fact carries claim-level evidence and a `Last verified` date or `Review trigger` beside it.
- [ ] Procedures are linked through Boundaries, not embedded.
- [ ] A single record uses a definition block; peer entries use a table within the 15-column and 20-row ceiling, with em-dash for empty cells.
- [ ] Every fenced block carries an intent label.
- [ ] Boundaries carries one link per adjacent owner and every relative target resolves.
