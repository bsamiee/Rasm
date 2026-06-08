# [STACKS_AGENTS]

Scope: `docs/stacks/**`. Parent instructions and `docs/standards` own general documentation behavior; this file adds the stack-doc topology rules that every language leaf specializes.

## [1]-[READ_BEHAVIOR]

- When changing a language stack, read that stack's `README.md`, nearest `AGENTS.md` where present, package-manager or build manifests, and the concept page that owns the coding decision.
- When changing stack folder shape, add or update the language README chooser before adding concept pages.
- When changing package graph, tool graph, runtime, or build truth, use the newest viable package-manager surface for that language and record current graph facts in the language stack's platform page where present; otherwise route current graph facts to manifests until a platform page exists.
- When changing cross-stack precedence, read `docs/usage/`; when changing host SDK semantics, read `docs/hosts/` where present.
- When changing documentation form, route through `docs/standards/README.md` and the owning standard.

## [2]-[OWNER_CONTRACT]

Stack docs are implicit agent guidance and decision atlases for language work. Each language folder owns its own durable stack guidance: C#, Python, TypeScript, Bash, PostgreSQL, and future language stacks all resolve through their own README, leaf overlay, concept pages, platform pages, and planning files where present.

A language README chooses the page that answers the reader's coding decision. Concept pages own durable language, domain, package-backed capability, runtime, testing, and proof choices. Platform pages, where present, own package-manager truth, build graph truth, tool graph truth, and adoption gates.

Organize stack pages by domain, category, or concept. Approved external libraries are first-class implementation material inside the concept page that owns the decision; package names do not define folder or file topology unless a platform page records current graph state.

Each `language.md` starts with a canonical chooser table for primitive selection. Each row states one durable decision as concern, accepted surface, and rejected substitute. Use neutral concern names that describe the coding choice, not release names, package names, feature marketing, or broad categories. Keep cells atomic: one owner primitive or compact phrase per cell, no prose explanations, links, source notes, version history, caveats, examples, or multi-sentence rationale.

For table-driven stack pages, promote a row or row family into a card only when the coding decision needs expanded guidance: concept complexity, advanced implementation shape, replacement of a common local workaround, or recently added language capability that benefits from executable form.

After the chooser, build sections from row families that need more than lookup. A post-table section must add usage boundaries, composition rules, or accepted/rejected replacements that change implementation; do not restate the chooser row as loose bullets. Prefer one matrix or contrast record per decision family over mixed prose, unscoped lists, and floating examples. Route runtime, package, proof, platform, and architecture detail to the concept page that owns that decision.

Code snippets belong inside cards that justify executable guidance. Use snippets to clarify extended explanation, complex mechanics, advanced code shape, or replacement of casts, facades, wrappers, fake protocols, helper ladders, and other workarounds; keep snippets neutral, minimal, and capability-dense.

Package-manager absence is not a downgrade signal. If a domain needs a stronger external library that is not yet admitted by the manifest, write the target capability and adoption gap in the owning concept, platform page, roadmap, or architecture route instead of weakening the guidance to available built-ins.

Mine skills, prompts, memory, reports, external research, and sub-agent output as source material for any language stack. Verify useful rules against package managers, source, manifests, generated contracts, maintained provider docs, or active stack pages, then rewrite them into this implicit docs structure.

## [3]-[ROUTE_AWAY]

Route README orientation to the language README, package and tool graph truth to platform pages or manifests, command syntax to tool owners, cross-stack precedence to `docs/usage/`, host SDK truth to `docs/hosts/`, implementation sequence to `.planning/ROADMAP.md`, planned structure to `.planning/ARCHITECTURE.md`, and document form to `docs/standards/`.

Future skills extract from stack docs. Do not make a stack page depend on a skill as the durable source for language, package, or methodology guidance.

## [4]-[REJECTIONS]

- No skill-owned stack guidance or skill route as documentation authority.
- No external-library-specific files or folders when a concept page can own the decision.
- No built-in fallback guidance that exists only because a stronger package is missing from the current manifest.
- No package/API catalogs, provider manuals, source-link collections, version changelogs, command ladders, or validation ceremonies in stack concept pages.
- No copied C#, Python, TypeScript, Bash, PostgreSQL, or other language doctrine in this parent overlay.
- No empty language leaf overlay; create a leaf `AGENTS.md` only with real local behavior deltas.
