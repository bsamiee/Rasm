---
description: Standard for curated reference documentation
---

# Reference documentation standards

Reference documents provide lookup truth for readers who already understand the
domain and need accurate facts while working. They are neutral, evidence-backed,
ordered by the shape of the subject, and optimized for retrieval rather than
instruction.

## Use when

Use reference leaves for:

- external product, library, SDK, protocol, host, runtime, or package facts;
- command, option, environment variable, error, status, and configuration
  lookup;
- terminology glossaries, abbreviation lists, data dictionaries, schema
  dictionaries, and field catalogs;
- capability and limitation facts when a full support matrix is unnecessary;
- small examples that illustrate facts without becoming procedures.

Use [api.md](api.md) for HTTP contracts or generated API reference. Use
[support-matrix.md](support-matrix.md) when support status becomes policy.

## Source of truth

Use this order for reference truth:

1. Repository source, generated output, contracts, manifests, schemas, metadata,
   lockfiles, and runnable command output.
2. Official specifications, standards, vendor reference docs, release notes, and
   support policies.
3. Maintainer-controlled examples, migration notes, issue records, or known
   limitations from the owning project.
4. Curated reference prose in the local document.

Reference prose must not fork a stronger source. Link the stronger source and
summarize only the facts needed for local lookup.

## Profiles

- Fact catalog: grouped facts about an external dependency, runtime, host, or
  local tool surface.
- Command reference: command names, flags, arguments, defaults, output shape,
  exit behavior, side effects, and examples.
- Glossary: terms, abbreviations, aliases, preferred terms, rejected terms, and
  related concepts.
- Data dictionary: data elements, fields, columns, units, value domains,
  nullability, allowed values, provenance, quality notes, ownership, and source
  schemas.
- Capability reference: supported features, limitations, status vocabulary,
  version constraints, and evidence.

Choose one primary profile. Split a page that becomes both a procedure and a
lookup catalog.

## Required structure

```markdown
# <Topic>

<One-sentence scope.>

## Source of truth
## Status vocabulary
## <Lookup group>
## Examples
## Evidence
## Review trigger
## Related
```

`Status vocabulary` and `Examples` are optional. `Source of truth`, `Evidence`,
and `Review trigger` are required when any fact can drift.

## Fact entries

Each material entry should state only the fields needed to make the fact usable:

- name or canonical term;
- kind, type, category, or owner;
- definition or concise description;
- allowed values, units, defaults, constraints, lifecycle status, or support
  status when relevant;
- source of truth or evidence;
- last verified date or review trigger for drift-prone facts;
- related entries or canonical external links.

Prefer bullets or definition-style blocks for short entries. Use a table only
when columns are stable and row scanning helps the reader.

## Glossaries

Glossaries standardize terminology:

- Define one sense per entry. Split terms with multiple senses by context.
- Mark preferred terms, aliases, deprecated terms, and rejected terms when the
  distinction affects usage.
- Preserve official names, capitalization, and symbols from the owning source.
- Define acronyms and abbreviations only when the target reader cannot
  reasonably infer them.
- Link to the owner document when the term belongs to another domain.

## Data dictionaries

Data dictionaries describe data elements and their metadata. They do not replace
schemas, migrations, generated contracts, warehouse catalogs, or API contracts.

For each data element, include the subset that applies:

- canonical name and aliases;
- semantic definition;
- owner or source system;
- data type, format, unit, precision, or encoding;
- value domain, enumerated values, ranges, nullability, uniqueness, and
  cardinality;
- key, relationship, partitioning, or lineage facts when relevant;
- sensitivity, access class, retention, provenance, quality, and freshness
  facts when relevant;
- source schema, contract, query, generated catalog, or official data standard.

Link machine-readable schemas instead of copying full schemas.

## Examples and warnings

Reference examples illustrate a fact. They should be short, copy-safe when
possible, and placed next to the fact they clarify. Move multi-step usage to a
how-to guide.

Warnings belong in reference docs when they describe constraints, destructive
behavior, unsupported combinations, security-sensitive facts, or likely misuse.
Keep warnings factual and evidence-backed.

## Boundaries

- Reference states what is true.
- How-to states how to complete a task.
- Tutorial teaches a path.
- Explanation and architecture explain context, structure, and trade-offs.
- ADR explains why a decision was made.
- API documentation owns generated or contract-backed API surfaces.
- Support matrices own broad support status.
- Runbooks own operational symptoms, triage, mitigation, rollback, and recovery.

## Review checklist

- [ ] Scope sentence is present.
- [ ] The primary reference profile is clear.
- [ ] Lookup groups mirror the subject being described.
- [ ] Fact entries are neutral, concise, and complete enough for lookup.
- [ ] Glossary and data dictionary entries use canonical terms and sources.
- [ ] Drift-prone facts have claim-level evidence and a review trigger.
- [ ] Procedures are linked, not embedded.
- [ ] API contracts and generated API reference material are routed to
      [api.md](api.md).
- [ ] Tables are readable, stable, and lookup-oriented.
