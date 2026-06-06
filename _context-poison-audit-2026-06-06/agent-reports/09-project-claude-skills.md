# [PROJECT_CLAUDE_SKILLS_CONTEXT_POISON_AUDIT]

Scope: `/Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/**`.

Read coverage:
- Read repo routing from `CLAUDE.md` and `docs/standards/README.md`.
- Inventoried 126 project-local skill Markdown files, including 19 `SKILL.md` files and 107 reference/template Markdown files.
- Read referenced Bash examples and scanned directly referenced scripts for instructional comments.

## [1][TOP_FINDINGS]

| [ID] | [SEVERITY] | [SMELL] | [PRIMARY_SURFACE] |
| :--- | :--------- | :------ | :---------------- |
| F1 | High | Static "current/latest" version and SHA catalogs poison future generation. | `github-actions` |
| F2 | High | Tool skill exposes reasoning/thinking output as a normal workflow. | `perplexity-tools` |
| F3 | High | Reusable testing skill carries repo-local paths, rails, and runtime ownership doctrine. | `testing-cs` |
| F4 | High | Validation ladders and command catalogs are duplicated inside skills instead of owner docs/tools. | `testing-cs`, `testing-ts`, `coding-bash`, `github-actions` |
| F5 | Medium | Templates and examples encode generated-artifact anti-patterns or contradict their own skill rules. | `testing-ts`, `github-actions`, `coding-bash` |
| F6 | Medium | Skill-eval prompt/meta sections are embedded in reusable skill bodies. | `coding-*` |
| F7 | Medium | Local execution environment details leak into generic tool skills. | `*-tools` |
| F8 | Medium | Defensive/version-scared wording turns reusable skills into brittle compatibility manuals. | `coding-bash`, `github-actions`, `mermaid-diagramming` |

## [2][FINDINGS]

### [F1][HIGH][STATIC_CURRENT_VERSION_AND_SHA_CATALOGS]

`github-actions` says action versions must be resolved at generation time, then ships static "current" tables, exact SHAs, and copied examples with exact pinned SHAs. This is direct context poisoning: future agents can copy stale SHAs or majors while believing the skill has already supplied current truth.

Evidence:
- `.claude/skills/github-actions/SKILL.md:22` routes version resolution through `git ls-remote`, Context7, or WebSearch.
- `.claude/skills/github-actions/SKILL.md:137-151` says static SHA catalogs decay and says never embed static SHAs in reference files.
- `.claude/skills/github-actions/references/version-discovery.md:76-114` embeds a common-actions index with "current major version series".
- `.claude/skills/github-actions/references/common_errors.md:84-93` embeds exact action versions and SHAs, then says to use discovery at generation time.
- `.claude/skills/github-actions/examples/docker-build-push.yml:26-34` and `.claude/skills/github-actions/examples/dependency-review.yml:24-27` embed exact SHA pins in examples.
- `.claude/skills/github-actions/references/modern_features.md:199-205` hard-codes Node runtime timing and an early-testing environment variable around March 4, 2026.

Impact:
- Agents may treat the skill's static tables as source truth instead of resolving actions live.
- The skill contradicts itself: "never embed static SHAs" sits beside embedded static SHAs.

Recommendation:
- Delete static current-version/SHA tables from references.
- Keep only the resolution protocol, pinning format, and validation shape.
- Move examples to placeholder SHAs or mark every static SHA as intentionally illustrative and unusable.

### [F2][HIGH][REASONING_OUTPUT_NORMALIZED]

`perplexity-tools` presents visible chain-of-thought and `<think>` tag stripping as a normal tool workflow. That is context poison because it trains downstream agents to request, handle, or preserve reasoning traces instead of treating them as unavailable/private implementation detail.

Evidence:
- `.claude/skills/perplexity-tools/SKILL.md:12` describes `sonar-reasoning-pro` as visible reasoning.
- `.claude/skills/perplexity-tools/SKILL.md:56-64` documents `strip` for research/reason commands and names visible chain-of-thought.
- `.claude/skills/perplexity-tools/SKILL.md:93-94` says reason output includes `<think>` tags by default.
- `.claude/skills/perplexity-tools/scripts/perplexity.py:11-12` repeats "strip thinking" in script usage.
- `.claude/skills/perplexity-tools/scripts/perplexity.py:81-112` implements optional `<think>` stripping.

Impact:
- Generated research workflows can leak or rely on hidden reasoning text.
- The skill makes "strip the thinking after retrieval" look acceptable, instead of avoiding reasoning traces entirely.

Recommendation:
- Remove visible chain-of-thought wording.
- Treat reasoning outputs as summarized answer content only.
- Make any stripping internal and undocumented, or reject/ignore reasoning traces at the wrapper boundary.

### [F3][HIGH][TESTING_CS_IS_PROJECT_COUPLED]

`testing-cs` is not just a reusable C# testing skill. It embeds Rasm-local test topology, runtime scenario doctrine, project testkit symbols, docs routes, and artifact ownership. Some project-local coupling is expected in `.claude/skills`, but this crosses from "local overlay" into durable repo policy and command-owner duplication.

Evidence:
- `.claude/skills/testing-cs/SKILL.md:16` binds the canonical unit rail to xUnit v3/MTP, CsCheck, the active repo testkit, and a repo-declared runtime scenario rail.
- `.claude/skills/testing-cs/SKILL.md:40-49` embeds paths such as `tests/csharp/libs/<Project>/<MirrorPath>/<Source>.spec.cs`, `tests/csharp/_testkit`, `_architecture`, `_tooling`, `_benchmarks`, and `_fuzz`.
- `.claude/skills/testing-cs/SKILL.md:73-83` describes repo scenario root behavior, harness-provided variables, fact channels, retired bridge flow, runtime health command, and artifact paths.
- `.claude/skills/testing-cs/SKILL.md:117-135` routes tool APIs to `docs/testing-libs/*` and records current local facts such as xUnit arity and `Check.Hash` caveats.
- `.claude/skills/testing-cs/references/bridge-runtime.md:17-25` repeats runtime/static ownership rules and repo-owned runtime artifact behavior.
- `.claude/skills/testing-cs/references/rails-tooling.md:8-22` carries a command catalog for `<quality-router>`, `<test-runner>`, mutation, coverage, and artifact locks.

Impact:
- Generic C# test generation can inherit Rasm runtime assumptions even when the target only needs ordinary managed tests.
- Future repo owner changes have to be synchronized across root policy, tool README/docs, and the skill.

Recommendation:
- Split the reusable testing law/oracle material from Rasm-local scenario and quality-router overlays.
- Keep `Spec.*` and `_testkit` details only in a repo-local overlay explicitly named as Rasm-specific.
- Route command catalogs to the quality owner instead of restating them in the skill.

### [F4][HIGH][VALIDATION_LADDERS_DUPLICATED_FROM_OWNERS]

Several skills carry validation ladders, command lists, and quality gates that belong to repo owners or tool docs. This is a classic context-poisoning surface: validation claims drift, and agents can report false proof from a stale skill instead of reading the current owner.

Evidence:
- `CLAUDE.md:114-120` already owns quality-gate selection and docs-only/move-only exceptions.
- `.claude/skills/testing-cs/SKILL.md:143-160` duplicates static/test/runtime command shapes.
- `.claude/skills/testing-cs/references/rails-tooling.md:8-22` duplicates managed command and mutation command tables.
- `.claude/skills/testing-ts/SKILL.md:48-49` hard-codes `pnpm exec nx test -- --coverage` and `npx stryker run`.
- `.claude/skills/testing-ts/references/validation.md:15-17` and `:43-49` repeat coverage/mutation commands and thresholds.
- `.claude/skills/coding-bash/SKILL.md:161-165` defines completion gates for Bash.
- `.claude/skills/coding-bash/references/validation.md:7-14` defines a full Bash validation pipeline.
- `.claude/skills/coding-bash/references/bash-testing.md:287-338` embeds a canonical GitHub Actions pipeline and coverage gate.
- `.claude/skills/github-actions/SKILL.md:155-203` embeds a validation pipeline, 11 best-practice checks, and troubleshooting table.

Impact:
- Agents can over-run validation for docs-only/comment-only work or under-run validation when the repo owner changed.
- Skill validation text becomes a second owner for tool behavior.

Recommendation:
- Replace duplicated command ladders with owner routes and selection criteria.
- Keep only invariant skill-local checks that are not owned elsewhere.
- Where a command must be shown, use placeholders and require reading the current repo/tool owner before execution.

### [F5][MEDIUM][GENERATED_ARTIFACT_ANTI_PATTERNS]

Some templates/examples encode patterns that conflict with their own skill doctrine or with generated-artifact hygiene.

Evidence:
- `.claude/skills/testing-ts/SKILL.md:166-190` says Schema should be passed directly to `it.effect.prop` and never wrapped with `Arbitrary.make` when possible.
- `.claude/skills/testing-ts/templates/unit-pbt.spec.template.md:25-28` still tells generated specs that schema-derived arbitraries use `Arbitrary.make(Schema)`.
- `.claude/skills/testing-ts/references/validation.md:37` also says schema-derived arbitraries are preferred through `Arbitrary.make(Schema)`, conflicting with the SKILL hard rule.
- `.claude/skills/github-actions/templates/docker-action.template.yml:72-103` embeds a shell wrapper that uses `date +%s`, `echo`, and a partial shell style inside an action template.
- `.claude/skills/github-actions/templates/javascript-action.template.yml:64-110` embeds a complete action implementation with generic class/error handling and placeholder logic that can be copied as artifact content instead of generated code.
- `.claude/skills/coding-bash/templates/standard.template.md:120-123` includes a `_nameref_stub` placeholder function and self-test that asserts placeholder behavior.
- `.claude/skills/coding-bash/examples/service-wrapper.sh:15-17` requires Bash 5.3+ even though the skill description supports Bash 5.2+/5.3.

Impact:
- Agents may copy contradictory or placeholder code into output.
- Template bodies blur "instruction" and "generated artifact" boundaries.

Recommendation:
- Fix the `testing-ts` Schema arbitrary contradiction first.
- Remove placeholder behavior from executable templates.
- Keep templates skeletal enough that placeholders cannot be mistaken for production behavior.

### [F6][MEDIUM][SKILL_EVAL_PROMPTS_IN_REUSABLE_SKILLS]

Several coding skills embed "Skill eval prompts", noisy-context examples, negative controls, and expected behavior. That is meta narration, not durable reusable instruction, and it can leak into task framing.

Evidence:
- `.claude/skills/coding-bash/SKILL.md:167-173` includes explicit invocation, implicit invocation, noisy context, negative control, and compliance checks.
- `.claude/skills/coding-python/SKILL.md:155-161` repeats the same eval-prompt pattern.
- `.claude/skills/coding-python/references/validation.md:93-99` repeats eval prompts again.
- `.claude/skills/coding-ts/SKILL.md:167-173` repeats eval prompts.
- `.claude/skills/coding-pg/references/validation.md:176-182` repeats eval prompts.

Impact:
- Skills become prompt-test transcripts instead of reusable operator instructions.
- Negative controls can steer agents toward meta compliance instead of task evidence.

Recommendation:
- Move eval prompts to a private evaluation fixture outside runtime skill context.
- Keep runtime skills to trigger, owner routes, contracts, and minimal checklists.

### [F7][MEDIUM][LOCAL_ENVIRONMENT_LEAKAGE_IN_TOOL_SKILLS]

Tool skills include local runtime assumptions such as `$CLAUDE_HOME`, `$CODEX_HOME`, 1Password injection, concrete API token names, and sample account IDs. Some of this is useful locally, but it is context poison for reusable skills because it makes generated instructions depend on one workstation/runtime.

Evidence:
- `.claude/skills/context7-tools/SKILL.md:25-40` uses `$CLAUDE_HOME/skills/...` command paths.
- `.claude/skills/exa-tools/SKILL.md:12` says the API key is auto-injected via 1Password; `:27-50` uses `$CLAUDE_HOME`.
- `.claude/skills/perplexity-tools/SKILL.md:10` says API key auto-injected via 1Password; `:26-43` uses `$CLAUDE_HOME`.
- `.claude/skills/tavily-tools/SKILL.md:12` says 1Password injects the API key; `:26-47` uses `$CLAUDE_HOME`.
- `.claude/skills/sonarcloud-tools/SKILL.md:25-29` assumes `SONAR_TOKEN`, `packages/*/coverage/lcov.info`, and `sonar-project.properties` at repo root.
- `.claude/skills/sonarcloud-tools/SKILL.md:46-64` uses `$CODEX_HOME/skills/...`.
- `.claude/skills/hostinger-tools/SKILL.md:55-75` shows concrete VPS/firewall IDs and command shapes.

Impact:
- Agents may tell users to run commands that only work in this local Claude/Codex setup.
- Tool docs become local operator notes instead of reusable skill surfaces.

Recommendation:
- Convert runtime paths to `<skill-root>/scripts/...` or tool invocation names.
- Keep secret injection details in local installation notes, not reusable skills.
- Replace concrete IDs with neutral placeholders and mark destructive operations explicitly.

### [F8][MEDIUM][DEFENSIVE_VERSION_SCARED_WORDING]

Several skills are overloaded with version gates, fallback chains, deprecated-runtime warnings, and beta caveats. Some version awareness is necessary, but the repeated defensive framing makes the model optimize for compatibility chatter instead of current owner truth.

Evidence:
- `.claude/skills/coding-bash/SKILL.md:4-17` frames Bash around 5.2+/5.3, version gating, fork-free substitution, and "when available" ecosystem selection.
- `.claude/skills/coding-bash/references/version-features.md:3-16` dedicates the feature model to 5.2/5.3 boundaries.
- `.claude/skills/coding-bash/references/version-features.md:493-563` provides broad fallback dispatch and feature-polymorphic helper guidance.
- `.claude/skills/github-actions/references/version-discovery.md:65-72` preserves paused immutable action rollout history and current approach narrative.
- `.claude/skills/mermaid-diagramming/SKILL.md:99-101` lists beta status for many diagram types and says syntax may change.
- `.claude/skills/mermaid-diagramming/references/architecture.md:31` says layout is non-deterministic on refresh for known v11.5.0+.

Impact:
- Skills can generate compatibility prose, stale caveats, or fallback helpers even when the active repo wants current direct replacement.
- Repeated version warnings dilute the actual decision point: verify current tool behavior when needed.

Recommendation:
- Keep only version boundaries that change generation behavior.
- Delete history and caveat prose unless it selects a different artifact shape.
- Route freshness-sensitive facts to current docs lookup instead of preserving them in skill prose.

### [F9][MEDIUM][PROJECT_OR_BRAND_COUPLING_IN_LANGUAGE_SKILLS]

Language skills carry non-generic project/brand language and local analyzer names that do not belong in reusable coding standards unless the skill is explicitly scoped as a Rasm-only overlay.

Evidence:
- `.claude/skills/coding-python/SKILL.md:128-143` names "Noesis" as the local standard and semantic enforcement surface.
- `.claude/skills/coding-python/SKILL.md:178-180` lists `cyclopts` for "quality operator and repo tools" and `suitkaise` with local transport details.
- `.claude/skills/coding-csharp/references/validation.md:91-108` maps generic code smells to `CSP####` analyzer coverage.
- `.claude/skills/coding-csharp/SKILL.md:147-151` says "Project Proof" and refers to project static analysis/tests.

Impact:
- Non-Rasm work can inherit Noesis/CSP-specific assumptions.
- Agents may treat local analyzer availability as universal.

Recommendation:
- Rename these files as project overlays, or remove project/brand/analyzer specifics from the reusable layer.
- Keep local analyzer mappings in repo owner docs or a Rasm-specific appendix.

## [3][LOWER_PRIORITY_NOTES]

- `hooks-builder` is inherently prompt/hook-schema oriented, so prompt wording there is not automatically poison. It still has a high density of lifecycle/schema facts that can drift if Claude Code hook APIs change.
- `mermaid-diagramming` has a useful syntax catalog, but its universal "Frontmatter Only" rule may conflict with repo-specific docs standards where Mermaid config placement is separately owned.
- `coding-bash` examples contain many comments that explain what code does. That is acceptable in examples, but because examples are loadable before writing, these comments can be copied into generated scripts.

## [4][GAPS]

- I did not verify freshness-sensitive GitHub Actions, Mermaid, Nx, Perplexity, Bash, or tool-version claims against current upstream docs. The audit flags static current-fact storage as a smell; it does not prove each fact is stale.
- I did not semantically review Python/Bash tool scripts beyond instructional comments and wrapper-facing comments.
- The worktree already had many modified `.claude/skills/**` files before this report. I treated them as current source and did not modify them.

## [5][RECOMMENDED_ORDER]

1. Remove or quarantine static GitHub Actions version/SHA catalogs and fix examples/templates so generated workflows must resolve live.
2. Remove Perplexity reasoning/think-tag handling from user-facing skill docs and wrapper output contracts.
3. Split `testing-cs` into reusable testing doctrine plus Rasm-only runtime/quality overlay.
4. Delete skill-eval prompt sections from runtime skill context.
5. Collapse duplicated validation ladders into owner-route pointers.
6. Fix `testing-ts` `Schema` arbitrary contradiction and remove executable placeholder behavior from templates.

