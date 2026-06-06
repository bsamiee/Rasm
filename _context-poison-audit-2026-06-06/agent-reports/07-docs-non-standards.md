# [DOCS_NON_STANDARDS_CONTEXT_POISON_AUDIT]

Agent: first-wave auditor G
Date: 2026-06-06
Scope: `docs/**` Markdown excluding `docs/standards/**` and excluding `docs/**/_reports/**`
Output: read-only audit report; no source documentation edits made

## [1][FILES_READ]

Governing instructions and standards read before the target pass:
- `CLAUDE.md`
- root `AGENTS.md` from the user-provided active project instructions
- `docs/standards/README.md`
- `docs/standards/agents-md.md`
- `docs/standards/agentic-documentation.md`
- `docs/standards/information-structure.md`
- `docs/standards/style-guide.md`
- `docs/standards/proof.md`
- `docs/standards/formatting.md`

Target docs read in full:
- `docs/usage.md`
- `docs/host-libraries.md`
- `docs/external-libs/README.md`
- `docs/external-libs/csharp/language.md`
- `docs/external-libs/languageext/api.md`
- `docs/external-libs/languageext/collections.md`
- `docs/external-libs/languageext/combinators.md`
- `docs/external-libs/languageext/effects.md`
- `docs/external-libs/languageext/operators.md`
- `docs/external-libs/languageext/prelude.md`
- `docs/external-libs/languageext/rasm.md`
- `docs/external-libs/languageext/traits.md`
- `docs/external-libs/mathnet/api.md`
- `docs/external-libs/mathnet/linear.md`
- `docs/external-libs/mathnet/rasm.md`
- `docs/external-libs/mathnet/sparse.md`
- `docs/external-libs/mathnet/symbolics.md`
- `docs/external-libs/thinktecture/api.md`
- `docs/external-libs/thinktecture/enums.md`
- `docs/external-libs/thinktecture/objects.md`
- `docs/external-libs/thinktecture/rasm.md`
- `docs/external-libs/thinktecture/sourcegen.md`
- `docs/external-libs/thinktecture/union-attributes.md`
- `docs/external-libs/thinktecture/unions.md`
- `docs/system-api-map/README.md`
- `docs/system-api-map/bcl.md`
- `docs/system-api-map/meta.md`
- `docs/system-api-map/packages.md`
- `docs/system-api-map/replacements.md`
- `docs/testing-libs/README.md`
- `docs/testing-libs/archunit/api.md`
- `docs/testing-libs/benchmarkdotnet/api.md`
- `docs/testing-libs/coverlet/api.md`
- `docs/testing-libs/cscheck/api.md`
- `docs/testing-libs/sharpfuzz/api.md`
- `docs/testing-libs/stryker/api.md`
- `docs/testing-libs/verify/api.md`
- `docs/testing-libs/xunit/api.md`

Inventory result: 38 target Markdown files, 2,584 target lines. `docs/standards/**` and `docs/**/_reports/**` were excluded from target findings.

## [2][EXECUTIVE_FINDINGS]

### [2.1][HIGH][INSTRUCTION_MARKER_POISON]

Every target file uses instruction-file invocation markers in ordinary documentation. `rg -l "\[IMPORTANT\]|\[CRITICAL\]"` over the scoped target returned 38 files. This violates the standards boundary that reserves `[IMPORTANT]`, `[CRITICAL]`, `[ALWAYS]`, and `[NEVER]` for instruction surfaces, not reference docs. It also trains future generated docs to lead with agent-weighting syntax instead of normal reader-facing scope prose.

Evidence:
- `docs/external-libs/README.md:3` opens with `[IMPORTANT] Scope: approved non-standard-library APIs...`.
- `docs/system-api-map/bcl.md:3` opens with `[IMPORTANT] BCL APIs do not replace...`.
- `docs/testing-libs/xunit/api.md:3` opens with `[IMPORTANT] Use xUnit v3...`.
- `docs/system-api-map/packages.md:59` uses `[CRITICAL] Candidate does not mean restored...` inside an ordinary package-state reference.

Impact: High. The marker family is a context-poisoning pattern because downstream agents can mistake reference facts for higher-ranked instructions and reproduce the syntax in future artifacts.

Correction target: Replace document-opening marker lines with ordinary lead prose or GitHub alerts only where the container genuinely interrupts the reader. Reserve invocation markers for `AGENTS.md`, `CLAUDE.md`, and prompt assets.

### [2.2][HIGH][GENERIC_API_DOCS_COUPLED_TO_LOCAL_RASM_CONTEXT]

Several files that read like generic library API references contain Rasm, Rhino, GH2, local analyzer, and local-path claims. The `rasm.md` leaves are explicitly project posture files and can carry project facts, but nearby `api.md`, `objects.md`, `xunit/api.md`, `cscheck/api.md`, `verify/api.md`, and package/API leaves leak local examples and commands into reusable library guidance.

Evidence:
- `docs/external-libs/languageext/api.md:3` says "Typical workspace imports" and lists workspace-global LanguageExt namespaces.
- `docs/external-libs/languageext/api.md:35` says `Flatten()` is preferred "when local XML proves availability."
- `docs/external-libs/mathnet/api.md:18` routes host geometry and data-access boundaries to "project usage owner, maintained host metadata, and nested host instruction owner."
- `docs/external-libs/thinktecture/api.md:3` says active package and global-using facts live in package-state docs.
- `docs/testing-libs/archunit/api.md:3` names "the architecture test project" and "the local analyzer."
- `docs/testing-libs/cscheck/api.md:3` makes raw `Check.*` calls subordinate to "the project testkit."
- `docs/testing-libs/verify/api.md:42` gives `tests/csharp/_tooling/ModuleInitializers.cs` as the module-initializer location.

Impact: High. Generic-looking API references become project instruction carriers. Future agents reading these as reusable library docs will import Rasm/Rhino/testkit assumptions into unrelated documentation or code.

Correction target: Split API fact pages from project posture pages. Keep portable package API facts in `api.md` leaves; move local graph, local XML, local testkit, local analyzer, and Rasm path facts into explicit project-use files such as `rasm.md`, `docs/usage.md`, `docs/system-api-map/*`, or test overlays.

### [2.3][HIGH][SOURCE_PROVENANCE_META_CHATTER_WITH_WEAK_PROOF_FIELDS]

The corpus uses ad hoc `[SOURCE]` labels, "local proof showed", and "Verified" prose instead of the proof standard's claim-level `Evidence:`, `Last verified:`, `Review trigger:`, or `Proof gap:` fields. This creates provenance-looking text that is not refreshable.

Evidence:
- `docs/testing-libs/xunit/api.md:15` uses `[SOURCE] xUnit MTP docs: https://xunit.net/docs/getting-started/v3/microsoft-testing-platform`.
- `docs/testing-libs/cscheck/api.md:64` and `docs/testing-libs/cscheck/api.md:120` use `[SOURCE]` labels for CsCheck README links.
- `docs/testing-libs/stryker/api.md:52` uses `[SOURCE] Stryker config docs...`.
- `docs/testing-libs/coverlet/api.md:14` says "local proof showed stricter modes can empty reports" without command, date, or refresh trigger.
- `docs/external-libs/mathnet/api.md:26` says "Verified control surfaces include..." without visible evidence fields.

Impact: High. The text looks authoritative but cannot be refreshed by the next maintainer. It also encourages future docs to paste source labels instead of attaching proof beside each drift-prone command, package, API, or support claim.

Correction target: Convert ad hoc source lines into claim-level proof records only where a drift-prone claim needs proof. Otherwise remove the source chatter and route to a maintained owner. Use `Evidence:`, `Last verified:`, and `Review trigger:` where the claim can drift.

### [2.4][MEDIUM][DUPLICATED_LOAD_ORDER_AND_VALIDATION_POSTURE]

Project graph, proof order, package state, and owner routes are repeated across top-level and nested reference pages. The duplication is often useful in isolation, but the current shape causes future docs to copy route boilerplate rather than link to the owner.

Evidence:
- `docs/usage.md:58-68` carries a proof/source table for packages, build props, local XML, system/external docs, testing docs, standards, platform manuals, and official docs.
- `docs/external-libs/README.md:24-31` repeats package graph, host policy, cross-stack usage, and proof routing.
- `docs/system-api-map/README.md:28-40` repeats adoption and test-consumer routing, including local XML/decompile proof for RhinoWIP/GH2 and boundary-adapter placement.
- `docs/system-api-map/packages.md:76-82` repeats testing package injection, mutation tooling, and direct test rails.
- `docs/testing-libs/README.md:3` and `docs/testing-libs/README.md:18-24` repeat package ownership and test rail ownership rather than routing only to the local test instruction owner.

Impact: Medium. The duplicate load/proof posture makes generated follow-up docs likely to include agent-only routing context, and drift becomes likely when one owner changes.

Correction target: Keep `docs/usage.md` and `docs/system-api-map` as project-use routers. Reduce generic library indexes to a short owner link and remove repeated proof/load-order prose unless the page changes reader action locally.

### [2.5][MEDIUM][DEFENSIVE_VERSION_SCARED_WORDING]

Several docs carry version-scared or migration-shaped language that preserves old states in reader-facing guidance. Some version exclusions are legitimate API facts, but the wording often lacks a proof trigger and can make old baselines look like active doctrine.

Evidence:
- `docs/external-libs/csharp/language.md:6` says "Do not use `LangVersion=preview` / `latest`."
- `docs/external-libs/csharp/language.md:101-103` lists interceptors, file-based app directives, and `LangVersion=preview` / `latest` as out-of-scope statuses.
- `docs/external-libs/languageext/effects.md:46-59` carries v5 deltas from v4, including removed surfaces.
- `docs/external-libs/thinktecture/sourcegen.md:31-46` carries v10 generated-name and generic-union migration detail.
- `docs/system-api-map/bcl.md:155` says "Reject for new code: `DiagnosticSource`, `TraceSource` (legacy spans)...".
- `docs/host-libraries.md:41` says "Refresh latest stable package versions immediately before the first concrete consumer."

Impact: Medium. These lines may be valid, but the corpus lacks consistent freshness fields and can train agents to include defensive "old-vs-new" caveats in durable docs instead of writing source-independent target rules plus proof gaps.

Correction target: Keep version facts only where the page is explicitly a support/API delta reference and attach `Last verified:` or `Review trigger:`. Rewrite target guidance as direct rules where current availability is not the point.

### [2.6][MEDIUM][EXAMPLES_AND_SNIPPETS_ARE_TOO_LOCAL_OR TOO RUNNER_SPECIFIC]

Some examples are useful, but several teach local project shapes instead of library use. This is context poisoning when the page is a generic API reference.

Evidence:
- `docs/testing-libs/archunit/api.md:29-42` uses `DomainRoot`, `DownstreamRoot`, `BoundaryAdapterAttribute`, `IDomainValid`, and `AdmissionRegistry` in an API example.
- `docs/testing-libs/cscheck/api.md:56-62` uses `Vectors.Dimension.TryCreate` in a generator example.
- `docs/testing-libs/cscheck/api.md:126-127` names "Yuksel WSE" and "Cholesky vs LU" as performance assertion examples.
- `docs/testing-libs/verify/api.md:42-50` gives a concrete module initializer in the Rasm `_tooling` rail.
- `docs/external-libs/thinktecture/sourcegen.md:33-43` uses a `FileTarget.Loose` / `FileTarget.Bundled` code sample that is generic enough in naming but still embeds a local alias-rejection scenario as package guidance.

Impact: Medium. Agents can lift local source names into future generic docs and tests. The code examples also risk becoming pseudo-policy when no proof route or project-use boundary is declared.

Correction target: Use neutral sample domains in generic API files. Move Rasm-specific examples into project posture files or test strategy docs, and keep generic examples focused on package mechanics.

### [2.7][LOW][BRACKETED_GROUP_LABEL_OVERUSE]

The docs consistently use bracketed group labels such as `[USE]`, `[OWNER]`, `[SOURCE]`, `[FILES]`, `[DETAIL]`, and `[EXTRAS]`. Some labels are valid scanner aids, but many behave like heading surrogates or template residue.

Evidence:
- `docs/external-libs/README.md:16` uses `[FILES]`.
- `docs/system-api-map/bcl.md:28`, `62`, `85`, `109`, `141`, `174`, `199`, `213`, `227`, and `244` use repeated `[USE]`.
- `docs/system-api-map/replacements.md:25`, `63`, `93`, `118`, `161`, and `196` use repeated `[OWNER]`.
- `docs/testing-libs/cscheck/api.md:48` uses `[DETAIL]`; `docs/testing-libs/verify/api.md:29` uses `[WHY_VERIFY_FITS]`.
- `docs/external-libs/mathnet/sparse.md:119` uses `[EXTRAS]`.

Impact: Low to medium. It is mostly notation clutter, but it reinforces prompt/template style and can confuse generated docs into emitting agent-facing label packets everywhere.

Correction target: Keep group labels only where they introduce a real set. Convert repeated table-local labels to H3 sections or ordinary prose where the section already supplies context.

## [3][FOLDER_NOTES]

### [3.1][TOP_LEVEL_DOCS]

`docs/usage.md` and `docs/host-libraries.md` are explicitly project-use docs, so Rasm/Rhino/GH2 coupling is expected. Their main smell is not local context; it is the use of instruction markers, repeated route/proof posture, and freshness-sensitive candidate wording.

Key evidence:
- `docs/usage.md:19-20` names local API rail commands for RhinoWIP and GH2.
- `docs/usage.md:49-54` rejects future-intent package entries, first-consumer candidates as proof, and current early implementation symbols as doctrine.
- `docs/host-libraries.md:37-41` names an AppUi active matrix with runtime proof pending and a "latest stable package versions" refresh instruction.

### [3.2][EXTERNAL_LIBS]

This folder should make a sharper distinction between universal library API pages and project posture pages. `languageext/rasm.md`, `mathnet/rasm.md`, and `thinktecture/rasm.md` are the correct place for Rasm-specific facts. The leak is that sibling API leaves also use local XML, workspace imports, host boundaries, and package graph state.

High-signal examples:
- `docs/external-libs/languageext/api.md:3`, `35`
- `docs/external-libs/mathnet/api.md:16-18`, `26`
- `docs/external-libs/thinktecture/api.md:3`, `16`
- `docs/external-libs/thinktecture/sourcegen.md:16`, `20`, `26`

### [3.3][SYSTEM_API_MAP]

This folder is project-specific by design and is the least likely to poison generic docs if clearly labeled as project-use infrastructure. The main hazards are instruction markers, proof-looking route tables without claim-level proof, and package/future-candidate phrasing that can drift.

High-signal examples:
- `docs/system-api-map/README.md:3`, `28-40`
- `docs/system-api-map/bcl.md:3`, `110`, `146`, `274`
- `docs/system-api-map/meta.md:28`, `41`, `76`, `86`
- `docs/system-api-map/packages.md:48-59`, `76-82`

### [3.4][TESTING_LIBS]

This folder mixes package API reference with Rasm testkit policy. The boundary is especially blurry in `cscheck/api.md`, `verify/api.md`, and `stryker/api.md`, where local rails, CI settings, module initializer paths, and examples appear inside API leaves.

High-signal examples:
- `docs/testing-libs/archunit/api.md:3`, `19-23`, `29-45`
- `docs/testing-libs/cscheck/api.md:3`, `32-37`, `64`, `80`, `112-120`, `126-129`
- `docs/testing-libs/verify/api.md:17`, `42-53`
- `docs/testing-libs/stryker/api.md:20-26`, `30-37`, `52`

## [4][RECOMMENDED_CLEANUP_ORDER]

1. Replace invocation markers across all 38 target files. This is mechanical, high-signal, and reduces future artifact poisoning immediately.
2. Split or relabel library `api.md` pages so universal API facts stay portable and project posture moves to `rasm.md`, `docs/usage.md`, `docs/system-api-map`, or test overlays.
3. Convert `[SOURCE]`, "local proof showed", and "Verified..." prose into proof-standard fields only where the claim needs proof; delete source labels that are decorative.
4. Deduplicate package graph, proof-order, and local command routing. Keep those in `docs/usage.md`, `docs/system-api-map`, and owner-local instructions.
5. Normalize examples to neutral domains in generic pages; reserve Rasm-specific examples for explicit project-use pages.

## [5][GAPS]

- I did not verify external package docs or local package manifests for factual correctness because this pass was scoped to context-poisoning smells, not API truth.
- I did not audit links or anchors.
- I did not inspect non-Markdown files or generated outputs.
- I did not read `docs/standards/**` as target material; I read selected standards files only as governing rules for the audit.
- I did not edit source docs. The only file written by this pass is this report.
