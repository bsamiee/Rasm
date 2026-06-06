# [DOCS_01_STANDARDS_CONTEXT]

## [TRANSCRIPT]

Scope: context standards-context read for `docs/standards` and `agents-md.md`. I did not edit active standards files.

Read sequence:
- Loaded the repo instruction chain relevant to Markdown standards work: `CLAUDE.md`, root `AGENTS.md`, and `docs/standards/AGENTS.md`.
- Read the requested standards with line numbers: `docs/standards/AGENTS.md`, `docs/standards/agents-md.md`, `docs/standards/README.md`, ``, `docs/standards/information-structure.md`, `docs/standards/style-guide.md`, `docs/standards/proof.md`, and `docs/standards/formatting.md`.
- Ran a focused search for baseline, current-state, compatibility, proof, and future-standard language across those files.
- Ran `fd -H . docs/standards -t f -e md` and confirmed that the current command includes `_reports/` research files.
- Ran `fd -H . docs/standards -t f -e md | wc -l` and got `53`.
- Ran `fd -H . docs/standards -t f -e md -E research | wc -l` and got `23`.

Source posture:
- Local repo files were sufficient for this pass. I did not use current web research because the task asks for current repo standards interpretation, and no external provider behavior needed to be newly asserted.
- Memory was used only as orientation for the known `docs/standards` rebuild context; the findings below cite active repository files and command observations.

## [FINDINGS]

[F1] The standards overlay defines the active corpus with a command that includes deprecated `_reports/` research files. `docs/standards/AGENTS.md:9` says the active corpus is discovered by `fd -H . docs/standards -t f -e md`, but that command currently returns `_reports/` files and 53 Markdown files. Excluding `_reports/` returns 23 files. This conflicts with the same line's instruction to treat prompt notes, session history, deprecated source material, external research, and critiques as inputs only when explicitly named. It also does not match `docs/standards/README.md:115-121`, which defines the standards folder layout as root standards plus explanation, reference, task, and learning families, not `_reports/`.

[F2] `docs/standards/AGENTS.md:40` overstates "honest ignorance of current implementation drift" in a way that can blur proof boundaries. The intended rule is sound for target-standard selection, but the sentence is too broad for documentation that claims present behavior. `docs/standards/README.md:16-23` makes current repository source, manifests, generated contracts, and runnable tool output the strongest source for drift-prone claims. `docs/standards/proof.md:3`, `docs/standards/proof.md:20-27`, and `docs/standards/proof.md:70-72` require claim-level proof and explicit gaps for uncertain facts. The overlay should say that current implementation drift cannot weaken a target standard, while current-state claims still require current evidence.

[F3] `agents-md.md` repeats the future-forward principle but does not yet make it operational enough. The standard asks which newest language, feature, tooling, or methodology should replace older local practice at `docs/standards/agents-md.md:13`; requires a future-standard posture at `docs/standards/agents-md.md:35`; rejects current-baseline caveats at `docs/standards/agents-md.md:65`; applies it to root overlays at `docs/standards/agents-md.md:74`; and restates it for standards and plans at `docs/standards/agents-md.md:91`. The gap is a missing decision shape that tells an author how to encode "future target" separately from "current proof" and "known proof gap." Without that shape, authors can still express current lag as a caveat inside the rule itself.

[F4] Root-vs-folder layering is partially specified but missing a parent and leaf contract. `docs/standards/agents-md.md:26-28` captures Codex load behavior and the risk that leaf overlays do not load when work starts elsewhere. `docs/standards/agents-md.md:68-79` defines the root profile. Root `AGENTS.md:9-11` also requires closer files to override conflicts and root-started work to discover nested overlays. The missing piece is a compact layer matrix for root, parent, and leaf overlays: root routes, parent deduplicates sibling rules, and leaf carries only non-derivable local deltas.

[F5] The standards already protect proof claims, but `agents-md.md` should tie that proof split directly to future-foundation authoring. `docs/standards/AGENTS.md:62` forbids claiming tooling, provider behavior, or gates without current proof. `docs/standards/proof.md:87-103` defines docs-as-code gates and says a gate cannot be claimed passed unless it ran or a current status check proves it. Root `AGENTS.md:82` rejects C# static, test, or bridge proof claims for docs-only instruction edits. `agents-md.md` should add a proof handoff rule: target standards are normative design rules; existence, support, provider behavior, command behavior, and validation claims are evidence claims that route to `proof.md`.

## [AGENTS_MD_GAPS]

[G1] Add a required `TARGET_CURRENT_SPLIT` rule. The rule should state: when an `AGENTS.md` file names a future-facing rule, encode the target plainly; when it names present behavior, attach proof or route to the evidence owner; when proof is missing, mark the gap instead of weakening the target.

Recommended shape:
- Target standard: newest viable rule, method, language, architecture, or tool posture.
- Current-state claim: only facts proven by repository source, generated contract, manifest, tool output, or maintained source.
- Proof gap: the missing source, command, host state, provider proof, or route that blocks a current-state claim.
- Rejected wording: "currently still uses", "for now", "until migration", "compatibility path", "legacy alias", and "partial adoption" when those phrases soften the target instead of documenting a proof gap.

[G2] Add a root-parent-leaf layering table. The current root profile is useful, but folder authors need a direct rule for where guidance belongs.

Recommended layer contract:
- Root overlay: instruction router, source precedence, skill and standards route, first-hop nested overlay discovery, cross-stack owner map, and global rejection categories.
- Parent overlay: shared sibling delta, recurring owner grammar, repeated stop conditions, and subtree-wide trust or runtime boundaries.
- Leaf overlay: local owner identifiers, concrete extension rails, local route-away records, local proof gaps, and folder-specific stop behavior.

[G3] Add a "no baseline caveat" contrast record near `LOCAL_EXTENSION_GRAMMAR`. `docs/standards/agents-md.md:83-91` already shows accepted versus rejected local owner grammar. It should include an `AGENTS.md`-specific contrast for current drift.

Recommended contrast:
Accepted: When adding a new local rule, state the target owner and replacement action; route any current proof claim to `proof.md`.
Rejected: This folder currently still uses the old pattern, so preserve both until migration.
Reason: the rejected form converts current drift into a standard; the accepted form separates target behavior from evidence.

[G4] Add a parent overlay extraction rule to `CORPUS_REBUILD_RULES`. `docs/standards/agents-md.md:135-142` already says repeated sibling rules move to the nearest parent and broad grouped reads become trigger-driven reads. Add that a parent overlay must not become a second root: it carries only subtree-wide deltas that at least 3 siblings share or that a shared runtime, generated surface, host boundary, or trust boundary requires.

[G5] Tighten provider/load semantics proof. `docs/standards/agents-md.md:26` currently states Codex load behavior directly; `:170` and `docs/standards/proof.md:201-202` require current proof for drift-prone provider behavior. Keep the facts, but add a maintenance trigger beside the rule or route it explicitly through `proof.md` so future provider changes do not leave stale load semantics embedded in `agents-md.md`.

## [DOCS_OVERLAY_RECOMMENDATIONS]

[R1] Change the active-corpus discovery rule in `docs/standards/AGENTS.md`. Replace the current command in `docs/standards/AGENTS.md:9` with an exclusion-aware form such as `fd -H . docs/standards -t f -e md -E research`, or state that `_reports/` is source material and not part of the active corpus unless explicitly named. This prevents research files from becoming de facto standards just because they exist under the folder.

[R2] Split the "honest ignorance" sentence in `docs/standards/AGENTS.md:40`. Use two rules instead of one broad rule: target-standard selection ignores implementation drift; present-tense claims about commands, paths, provider behavior, validation, generated artifacts, and support status follow `proof.md`.

[R3] Add a local close-check item for future-foundation proof separation. Extend `docs/standards/AGENTS.md:67-74` with a check that every future-facing rule stays normative and every current-state claim has evidence, a route, or an explicit proof gap.

[R4] Keep `agents-md.md` as the owner for layering semantics, not `docs/standards/AGENTS.md`. `docs/standards/AGENTS.md:19-32` already routes `AGENTS.md` semantics to `agents-md.md`; the overlay should only point there. The root-parent-leaf layer contract belongs in `agents-md.md`, not in the local standards overlay.

[R5] Preserve proof boundaries while removing compatibility prose. Use `docs/standards/proof.md:72` as the guardrail: a qualifier is load-bearing only when evidence is genuinely uncertain. Current-project caveats that merely apologize for lag should be deleted; qualifiers that name an unrun check, missing source, or provisional state should stay as proof gaps.

## [CONFIDENCE]

Confidence: high for the active-corpus and proof-boundary findings, because they are backed by line references and current local command output. Confidence: medium-high for the proposed `agents-md.md` additions, because they are inferred from the existing standards' repeated future-forward rules and the missing operational shape rather than from a failed active edit.
