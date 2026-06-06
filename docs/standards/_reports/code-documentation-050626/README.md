# [CODE_DOCUMENTATION_050626]

Scope: source material for June 5, 2026 code-documentation standards research. Use this README to choose a language or method lane; current active standards outrank these reports when wording, routes, or proof status differ.

## [1][SESSION_CARD]

Target: `docs/standards/reference/code-documentation.md` and adjacent shared standards under `docs/standards/`.
Status: PARTIAL source material retained for future promotion.
Source set: code-documentation standard drafts, shared standards, language/tooling manifests, repo examples, language documentation specifications, generator docs, and security guidance captured in reports.
Read order: start with `track-general` for cross-language method, then read the language track that owns the affected code surface.
Boundary: this session maps research facets; it is not a per-language style guide and does not replace active code or docs standards.

## [2][TRACK_CARDS]

`track-general`
    Direction: cross-language rules that decide what code documentation is for before language-specific syntax.
    Facets: methodology, generated documentation, security-sensitive inline docs, agent-facing prose, lifecycle and maintenance.
    Use when: changing the shared code-documentation standard or deciding whether a language report belongs in the common rule.
    Extend by: adding a stronger shared invariant, a new generated-surface route, or a lifecycle correction that affects more than one language.
    Route: promote common rules to `reference/code-documentation.md`; route form, prose, and proof mechanics to their shared standards.

`track-csharp`
    Direction: C# XML documentation, advanced language features, LanguageExt and ROP surfaces, generators, and API style.
    Facets: XML docs, modern C# feature exposure, ROP semantics, Thinktecture/source generation, public API documentation.
    Use when: documenting C# symbols, generated unions/smart enums, functional rails, or analyzer-visible API surfaces.
    Extend by: correcting a generator or language-feature claim with current source proof, or adding a C# facet that changes how docs preserve behavior.
    Route: promote shared doc rules to `reference/code-documentation.md`; route implementation posture to C# overlays or skills.

`track-typescript`
    Direction: TypeScript and Effect documentation surfaces.
    Facets: TSDoc specification, TypeDoc/API Extractor, Effect docs, modern TypeScript features, lifecycle anti-patterns.
    Use when: documenting TypeScript APIs, schema authority, effects, generated API docs, or typed service surfaces.
    Extend by: adding a new toolchain or Effect-specific documentation invariant rather than repeating TSDoc basics.
    Route: promote generalizable rules to the shared code-documentation standard and TypeScript-specific posture to the TypeScript skill or overlay owner.

`track-python`
    Direction: Python docstrings, type metadata, async and ROP surfaces, and schema documentation.
    Facets: docstring generation, PEP-driven type docs, async/result semantics, schema metadata, style anti-patterns.
    Use when: documenting Python modules, generated references, typed message models, async APIs, or Result/Option rails.
    Extend by: tying a docstring rule to a current Python tool or type-system proof, or correcting a stale PEP/tooling claim.
    Route: promote shared rules to `reference/code-documentation.md`; route Python implementation posture to the Python skill or local tool overlay.

`track-bash`
    Direction: Bash command documentation and operational contracts.
    Facets: Bash 5.3 features, ShellCheck/shfmt, command contracts, trap/resource behavior, anti-patterns.
    Use when: documenting shell CLIs, command outputs, exit receipts, traps, resources, or checked shell style.
    Extend by: adding a concrete command-contract or resource-lifecycle invariant; do not add another shell-style summary.
    Route: promote shared command-doc rules to `reference/code-documentation.md`; route executable shell posture to the Bash skill or tool README.

`track-postgres`
    Direction: PostgreSQL comments, migrations, functions, security, RLS and views, extraction, and generated docs.
    Facets: `COMMENT ON`, migration docs, function and security docs, RLS and view documentation, catalog extraction, and generated references.
    Use when: documenting SQL objects, migrations, policies, generated catalogs, or database reference extraction.
    Extend by: adding a proof-backed catalog or migration-doc rule that changes generated documentation behavior.
    Route: promote shared rules to `reference/code-documentation.md`; route SQL implementation posture to the Postgres skill or schema owner.

## [3][NEXT_PASS_RULE]

Before adding a language report, read `track-general` and the target language track. Add a file only when it supplies a new source class, a materially different language feature, a correction, or candidate wording that cannot be folded into an existing report. If a new pass only strengthens an existing facet, update the earlier report and this track card instead of creating another same-topic file.

## [4][PROMOTION_STATE]

This session is retained as source material until the shared code-documentation standard absorbs the durable rules. Keep reports with unique source evidence or proof gaps; prune reports that only repeat generic documentation advice after promotion.
